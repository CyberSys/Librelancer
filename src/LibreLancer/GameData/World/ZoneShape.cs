﻿// MIT License - Copyright (c) Callum McGing
// This file is subject to the terms and conditions defined in
// LICENSE, which is part of this source code package

using System;
using System.Numerics;

namespace LibreLancer.GameData.World
{
	public abstract class ZoneShape
	{
		public abstract bool Intersects(BoundingBox box);
		public abstract bool ContainsPoint(Vector3 point);
		public abstract float ScaledDistance(Vector3 point);
		public abstract Vector3 RandomPoint (Func<float> randfunc);
		public abstract ZoneShape Scale(float scale);

        public abstract void Update();

		protected Zone Zone;
		protected ZoneShape(Zone zn)
		{
			Zone = zn;
		}

        public ZoneShape Clone(Zone newZone)
        {
            var o = (ZoneShape) MemberwiseClone();
            o.Zone = newZone;
            return o;
        }
    }
}

