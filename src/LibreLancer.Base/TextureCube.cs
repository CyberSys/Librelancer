﻿/* The contents of this file are subject to the Mozilla Public License
 * Version 1.1 (the "License"); you may not use this file except in
 * compliance with the License. You may obtain a copy of the License at
 * http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS IS"
 * basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
 * License for the specific language governing rights and limitations
 * under the License.
 * 
 * 
 * The Initial Developer of the Original Code is Callum McGing (mailto:callum.mcging@gmail.com).
 * Portions created by the Initial Developer are Copyright (C) 2013-2016
 * the Initial Developer. All Rights Reserved.
 */
using System;
using OpenTK.Graphics.OpenGL;

namespace LibreLancer
{
    public class TextureCube : Texture
    {
        public int Size { get; private set; }

        PixelInternalFormat glInternalFormat;
        PixelFormat glFormat;
        PixelType glType;

        public TextureCube( int size, bool mipMap, SurfaceFormat format)
        {
            ID = GL.GenTexture();
            Size = size;
            Format = format;
            Format.GetGLFormat(out glInternalFormat, out glFormat, out glType);
            LevelCount = mipMap ? CalculateMipLevels(size, size) : 1;
            if (glFormat == (PixelFormat)All.CompressedTextureFormats)
                throw new NotImplementedException("Compressed cubemaps");
            //Bind the new TextureCube
            Bind();
            //enable filtering
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            //initialise
            for (int i = 0; i < 6; i++)
            {
                var target = ((CubeMapFace)i).GL();
                GL.TexImage2D(target, 0, glInternalFormat,
                    size, size, 0, glFormat, glType, IntPtr.Zero);
            }
            if (mipMap)
            {
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.GenerateMipmap, 1);
            }
        }

        public void SetData<T>(CubeMapFace face, int level, Rectangle? rect, T[] data, int start, int count) where T : struct
        {
            int x, y, w, h;
            if (rect.HasValue)
            {
                x = rect.Value.X;
                y = rect.Value.Y;
                w = rect.Value.Width;
                h = rect.Value.Height;
            }
            else {
                x = 0;
                y = 0;
                w = Math.Max(1, Size >> level);
                h = Math.Max(1, Size >> level);
            }
            GL.BindTexture(TextureTarget.TextureCubeMap, ID);
            GL.TexSubImage2D<T>(face.GL(), level, x, y, w, h, glFormat, glType, data);
        }

        public void SetData<T>(CubeMapFace face, T[] data) where T : struct
        {
            SetData<T>(face, 0, null, data, 0, data.Length);
        }

        internal override void Bind()
        {
            GL.BindTexture(TextureTarget.TextureCubeMap, ID);
        }

        public override void Dispose()
        {
            GL.DeleteTexture(ID);
        }
    }
}

