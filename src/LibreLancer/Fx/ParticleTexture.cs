// MIT License - Copyright (c) Callum McGing
// This file is subject to the terms and conditions defined in
// LICENSE, which is part of this source code package

using System.Numerics;
using LibreLancer.Render;
using LibreLancer.Utf.Mat;

namespace LibreLancer.Fx
{
    public class ParticleTexture
    {
        public string Name;
        public Texture2D Texture;
        public int FrameCount = 1;
        TextureShape shape;
        TexFrameAnimation frameanim;

        public Vector4 GetCoordinates(int frame)
        {
            if (frameanim != null)
            {
                var f = frameanim.Frames[frame];
                var x = f.UV1.X;
                var y = (1 - f.UV1.Y);
                var width = f.UV2.X - f.UV1.X;
                var height = (1 - f.UV2.Y) - y;
                return new Vector4(x, y, width, height);
            }
            if (shape != null)
            {
                return new Vector4(shape.Dimensions.X, shape.Dimensions.Y, shape.Dimensions.Width,
                    shape.Dimensions.Height);
            }
            return new Vector4(0, 0, 1, 1);
        }

        public void Update(string name, ResourceManager res)
        {
            if (string.IsNullOrEmpty(name))
            {
                Name = name;
                Texture = res.NullTexture;
                FrameCount = 1;
            }

            if (Name != name) {
                Texture = null;
                frameanim = null;
                shape = null;
            }
            Name = name;
            if (Texture == null || Texture.IsDisposed)
            {
                if (shape == null && frameanim == null && Texture != null)
                { 
                    Texture = res.FindTexture(name) as Texture2D;
                }
                else if (shape == null && frameanim == null)
                {
                    if (res.TryGetShape(name, out shape))
                    {
                        Texture = (Texture2D) res.FindTexture(shape.Texture);
                    }
                    else if (res.TryGetFrameAnimation(name, out frameanim))
                    {
                        Texture = res.FindTexture(name + "_0") as Texture2D;
                        FrameCount = frameanim.FrameCount;
                    }
                    else
                    {
                        Texture = res.FindTexture(name) as Texture2D;
                    }
                }
                else if (shape != null)
                {
                    Texture = (Texture2D) res.FindTexture(shape.Texture);
                }
                else if (frameanim != null)
                {
                    Texture = res.FindTexture(name + "_0") as Texture2D;
                }
            }
        }
    }
}