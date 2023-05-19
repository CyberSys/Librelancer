using System;
using System.Runtime.InteropServices;

namespace LibreLancer.Thorn
{
    public static class LuaCompiler
    {
        [DllImport("thorncompiler")]
        static extern bool thn_compile(
            [MarshalAs(UnmanagedType.LPUTF8Str)] string input,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string name,
            out IntPtr outputBuffer,
            out int outputSize
        );

        [DllImport("thorncompiler")]
        static extern IntPtr thn_geterror();

        [DllImport("thorncompiler")]
        static extern void thn_free(IntPtr buffer);

        /// <summary>
        /// Compiles Lua 3.2 source code into a Lua binary
        /// </summary>
        /// <param name="code">The source code to compile</param>
        /// <param name="name">The name of the file being compiled</param>
        /// <returns>an array of bytes containing the compiled lua binary data</returns>
        /// <exception cref="LuaCompileException">Throws if there is a compile error</exception>
        public static byte[] Compile(string code, string name = "[string]")
        {
            if (!thn_compile(code, name, out IntPtr buf, out int sz))
            {
                var err = thn_geterror();
                var errstring = UnsafeHelpers.PtrToStringUTF8(err);
                thn_free(err);
                throw new LuaCompileException(errstring);
            }
            var compiled = new byte[sz];
            Marshal.Copy(buf, compiled, 0, sz);
            thn_free(buf);
            return compiled;
        }
    }
}
