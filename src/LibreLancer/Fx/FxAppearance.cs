﻿// MIT License - Copyright (c) Callum McGing
// This file is subject to the terms and conditions defined in
// LICENSE, which is part of this source code package

using System;
using System.Numerics;
using LibreLancer.Utf.Ale;

namespace LibreLancer.Fx
{
	public class FxAppearance : FxNode
	{
		public LineRenderer Debug;
		public bool DrawNormals = false;
		public FxAppearance (AlchemyNode ale) : base(ale)
		{
		}
        public virtual void Draw(ref Particle particle, int pidx, float lasttime, float globaltime, NodeReference reference, ResourceManager res, ParticleEffectInstance instance, ref Matrix4x4 transform, float sparam)
        {

        }
		public virtual void OnParticleSpawned(int idx, NodeReference reference, ParticleEffectInstance instance)
		{
		}

        static readonly AlchemyTransform[] transforms = new AlchemyTransform[32];
        protected bool DoTransform(NodeReference reference, float sparam, float t1, float t2, out Vector3 translate, out Quaternion rotate)
        {
            translate = Vector3.Zero;
            rotate = Quaternion.Identity;

            int idx = -1;
            var pr = reference;
            while(pr != null && !pr.IsAttachmentNode) {
                if(pr.Node.Transform.HasTransform && pr.Node.Transform.Animates) {
                    idx++;
                    transforms[idx] = pr.Node.Transform;
                }
                pr = pr.Parent;
            }
            for (int i = idx; i >= 0; i--) {
                translate += transforms[i].GetDeltaTranslation(sparam, t1, t2);
                rotate *= transforms[i].GetDeltaRotation(sparam, t1, t2);
            }
            return idx != -1;
        }
	}
}

