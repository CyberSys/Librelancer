﻿// MIT License - Copyright (c) Callum McGing
// This file is subject to the terms and conditions defined in
// LICENSE, which is part of this source code package

using System;
using System.Numerics;
using LibreLancer.Graphics;
using LibreLancer.Graphics.Vertices;
using LibreLancer.Shaders;
using LibreLancer.Utf.Dfm;
using LibreLancer.Utf.Mat;

namespace LibreLancer.Render.Materials
{
	public class BasicMaterial : RenderMaterial
    {
        public const int ForceAlpha = (1 << 31);

		public string Type;

		public Color4 Dc = Color4.White;
		public string DtSampler;
		public SamplerFlags DtFlags;
		public float Oc = 1f;
		public bool OcEnabled = false;
		public bool EtEnabled = false;
		public bool AlphaEnabled = false;
        public bool AlphaTest = false;
		public Color4 Ec = Color4.White;
		public string EtSampler;
		public SamplerFlags EtFlags;

		public BasicMaterial(string type, ResourceManager library) : base(library)
		{
			Type = type;
		}

		static ShaderVariables GetShader(RenderContext rstate, IVertexType vertextype, ShaderFeatures caps)
        {
            if (vertextype is Utf.Dfm.DfmVertex)
                return Basic_Skinned.Get(rstate, caps);
            if (vertextype is VertexPositionNormalTexture ||
                    vertextype is VertexPositionNormal)
                return Basic_PositionNormalTexture.Get(rstate, caps);
			if (vertextype is VertexPositionNormalTextureTwo)
                return Basic_PositionNormalTextureTwo.Get(rstate, caps);
            if (vertextype is VertexPositionNormalDiffuseTexture)
                return Basic_PositionNormalColorTexture.Get(rstate, caps);
            if (vertextype is VertexPositionTexture)
                return Basic_PositionTexture.Get(rstate, caps);
            if (vertextype is VertexPosition)
                return Basic_PositionTexture.Get(rstate, caps);
            if(vertextype is VertexPositionColor)
                return Basic_PositionColor.Get(rstate, caps);
            throw new NotImplementedException(vertextype.GetType().Name);
		}

        public override void Use(RenderContext rstate, IVertexType vertextype, ref Lighting lights, int userData)
		{
            ShaderFeatures caps = ShaderFeatures.None;
            if (VertexLighting) caps |= ShaderFeatures.VERTEX_LIGHTING;
            if (EtEnabled) caps |= ShaderFeatures.ET_ENABLED;
            if (Fade) caps |= ShaderFeatures.FADE_ENABLED;
            var dxt1 = GetDxt1();
            if (dxt1 || AlphaTest)
            {
                caps |= ShaderFeatures.ALPHATEST_ENABLED;
                //Shitty way of dealing with alpha_mask
                //FL has a lot of DXT1 textures that aren't part of alpha_mask
                //so this brings overall performance down.
                //Don't change any of this stuff unless you can verify it works
                //in all places! (Check Li01 shipyards, Bw10 tradelanes)
            }
			var shader = GetShader(rstate, vertextype, caps);
			shader.SetWorld(World);
            //Dt
			shader.SetDtSampler(0);
			BindTexture(rstate, 0, DtSampler, 0, DtFlags, ResourceManager.WhiteTextureName);
			//Dc
			shader.SetDc(Dc);
			//Oc
			shader.SetOc(Oc);
			if (AlphaEnabled || Fade || OcEnabled || dxt1 || AlphaTest || (userData & ForceAlpha) == ForceAlpha)
			{
				rstate.BlendMode = BlendMode.Normal;
			}
			else
			{
                rstate.BlendMode = BlendMode.Opaque; //TODO: Maybe I can just leave this out?
			}
			//Fade
			if (Fade) shader.SetFadeRange(new Vector2(FadeNear, FadeFar));
			//MaterialAnim
			if (MaterialAnim != null)
			{
				shader.SetMaterialAnim(new Vector4(
					MaterialAnim.UOffset,
					MaterialAnim.VOffset,
					MaterialAnim.UScale,
					MaterialAnim.VScale
				));
			}
			else
			{
				shader.SetMaterialAnim(new Vector4(0, 0, 1, 1));
			}
            if (Bones != null && vertextype is DfmVertex) {
                shader.Shader.UniformBlockBinding("Bones", 1);
                shader.SetSkinningEnabled(true);
                Bones.BindTo(1, BufferOffset, 200);
            }
            else
                shader.SetSkinningEnabled(false);
            //Ec
			shader.SetEc(Ec);
			//EtSampler
			if (EtEnabled)
			{
				shader.SetEtSampler(1);
				BindTexture(rstate, 1, EtSampler, 1, EtFlags, ResourceManager.NullTextureName);
			}
			//Set lights
            SetLights(shader, ref lights, rstate.FrameNumber);
            rstate.Shader = shader;
        }

		public override void ApplyDepthPrepass(RenderContext rstate)
		{
			rstate.BlendMode = BlendMode.Normal;
            //TODO: This is screwy - Re-do DXT1 test if need be for perf
			var shader = DepthPass_AlphaTest.Get(rstate);
            BindTexture(rstate, 0, DtSampler, 0, DtFlags, ResourceManager.WhiteTextureName);
			shader.SetWorld(World);
            rstate.Shader = shader;
        }

        bool GetDxt1()
        {
            var tex = GetTexture(0, DtSampler);
            if (tex != null) return tex.Dxt1;
            return false;
        }
		public override bool IsTransparent
		{
			get
			{
                return AlphaEnabled && !GetDxt1();
			}
		}
	}
}

