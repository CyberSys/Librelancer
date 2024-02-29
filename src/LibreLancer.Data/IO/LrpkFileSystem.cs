using System;
using System.IO;
using ZstdSharp;

namespace LibreLancer.Data.IO;

public sealed class LrpkFileSystem : BaseFileSystemProvider
{
    private Func<Stream> openStream;

    public LrpkFileSystem(Func<Stream> openStream)
    {
        this.openStream = openStream;
        Refresh();
    }

    public LrpkFileSystem(string filename)
    {
        openStream = () => File.OpenRead(filename);
        Refresh();
    }

    record struct BlockInfo(long Offset, long Length);

    public override void Refresh()
    {
        using var stream = openStream();
        Span<byte> sig = stackalloc byte[4];
        if (stream.Read(sig) != 4)
            throw new FileFormatException();
        if (sig[0] != (byte)'\b' ||
            sig[1] != (byte)'L' ||
            sig[2] != (byte)'P' ||
            sig[3] != 0)
            throw new FileFormatException();
        var reader = new BinaryReader(stream);
        var infoOffset = reader.ReadInt64();
        stream.Seek(infoOffset, SeekOrigin.Begin);
        //Read Block info
        var blockCount = reader.ReadByte();
        BlockInfo[] blocks = new BlockInfo[blockCount];
        for (int i = 0; i < blockCount; i++)
            blocks[i] = new BlockInfo(reader.Read7BitEncodedInt64(), reader.Read7BitEncodedInt64());
        var cache = new BlockCache(blocks, openStream);
        //Read VFS Tree
        Root = (VfsDirectory)ReadEntry(reader, openStream, cache, null);
    }

    static byte[] Decompress(Stream stream, long offset, long length)
    {
        using var wrapped = new SlicedStream(offset, length, stream);
        using var decomp = new DecompressionStream(wrapped);
        var output = new MemoryStream();
        decomp.CopyTo(output);
        return output.ToArray();
    }

    class BlockCache
    {
        public BlockInfo[] Blocks;
        public Func<Stream> GetFileStream;
        public WeakReference<byte[]>[] Decompressed;
        private object[] locks;

        public BlockCache(BlockInfo[] blocks, Func<Stream> getFileStream)
        {
            this.Blocks = blocks;
            this.GetFileStream = getFileStream;
            Decompressed = new WeakReference<byte[]>[blocks.Length];
            for (int i = 0; i < Decompressed.Length; i++)
                Decompressed[i] = new WeakReference<byte[]>(null);
            locks = new object[blocks.Length];
            for (int i = 0; i < locks.Length; i++)
                locks[i] = new object();
        }

        public byte[] GetBlock(int blockIndex)
        {
            lock (locks[blockIndex])
            {
                if (Decompressed[blockIndex].TryGetTarget(out var block))
                    return block;
                FLLog.Debug("Lrpk", $"Decompressing block {blockIndex}");
                block = Decompress(GetFileStream(), Blocks[blockIndex].Offset, Blocks[blockIndex].Length);
                Decompressed[blockIndex].SetTarget(block);
                return block;
            }
        }
    }


    sealed class LrpkOffsetFile : VfsFile
    {
        public Func<Stream> GetFileStream;
        public long Offset;
        public long Length;
        public override Stream OpenRead()
        {
            var stream = GetFileStream();
            return new SlicedStream(Offset, Length, stream);
        }
    }

    sealed class LrpkEmptyFile : VfsFile
    {
        public override Stream OpenRead()
        {
            return new MemoryStream(Array.Empty<byte>(), false);
        }
    }

    sealed class LrpkZstdFile : VfsFile
    {
        public Func<Stream> GetFileStream;
        public long Offset;
        public long Length;
        public override Stream OpenRead()
        {
            var block = Decompress(GetFileStream(), Offset, Length);
            return new MemoryStream(block, false);
        }
    }

    sealed class LrpkBlockFile : VfsFile
    {
        public BlockCache Cache;
        public int Block;
        public long Offset;
        public long Length;

        public override Stream OpenRead()
        {
            var block = Cache.GetBlock(Block);
            var strm = new MemoryStream(block, false);
            return new SlicedStream(Offset, Length, strm);
        }
    }

    // Kind
    // 0: Directory
    // 1: Empty File
    // 2: Uncompressed File
    // 3: ZSTD Compressed File
    // 4-255: Block compressed file

    static VfsItem ReadEntry(BinaryReader reader, Func<Stream> getFileStream, BlockCache blockCache, VfsDirectory parent)
    {
        var kind = reader.ReadByte();
        var name = reader.ReadStringUTF8();
        if (kind == 0)
        {
            var count = reader.Read7BitEncodedInt();
            var dir = new VfsDirectory() { Parent = parent };
            dir.Name = name;
            for (int i = 0; i < count; i++)
            {
                var ent = ReadEntry(reader, getFileStream, blockCache, dir);
                dir.Items[ent.Name] = ent;
            }
            return dir;
        }
        else if (kind == 1)
        {
            return new LrpkEmptyFile() { Name = name };
        }
        else if (kind == 2)
        {
            var off = reader.Read7BitEncodedInt64();
            var length = reader.Read7BitEncodedInt64();
            return new LrpkOffsetFile() { Name = name, Offset = off, Length = length, GetFileStream = getFileStream };
        }
        else if (kind == 3)
        {
            var off = reader.Read7BitEncodedInt64();
            var length = reader.Read7BitEncodedInt64();
            return new LrpkZstdFile() { Name = name, Offset = off, Length = length, GetFileStream = getFileStream };
        }
        else
        {
            var blockIndex = kind - 4;
            var off = reader.Read7BitEncodedInt64();
            var length = reader.Read7BitEncodedInt64();
            return new LrpkBlockFile()
                { Name = name, Block = blockIndex, Offset = off, Length = length, Cache = blockCache };
        }

    }

    public static bool IsLrpk(Stream stream)
    {
        var pos = stream.Position;
        try
        {
            Span<byte> sig = stackalloc byte[4];
            if (stream.Read(sig) != 4)
                return false;
            return sig[0] == (byte)'\b' &&
                   sig[1] == (byte)'L' &&
                   sig[2] == (byte)'P' &&
                   sig[3] == 0;
        }
        catch
        {
            return false;
        }
        finally
        {
            stream.Seek(pos, SeekOrigin.Begin);
        }
    }
}
