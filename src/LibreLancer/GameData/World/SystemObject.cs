﻿// MIT License - Copyright (c) Callum McGing
// This file is subject to the terms and conditions defined in
// LICENSE, which is part of this source code package

using System.Linq;
using System.Numerics;
using LibreLancer.GameData.Archetypes;

namespace LibreLancer.GameData.World
{
	public class SystemObject
	{
		public string Nickname;
        public int IdsName;
        public int[] IdsInfo;
        public Base Base; //used for linking IdsInfo
		public Archetype Archetype;
        public Sun Star;
		public Vector3 Position = Vector3.Zero;
        public Vector3 Spin = Vector3.Zero;
		public Matrix4x4? Rotation;
        public ObjectLoadout Loadout;
		public DockAction Dock;
        public Faction Reputation;
        public VisitFlags Visit;

        public int TradelaneSpaceName;
        public int IdsLeft;
        public int IdsRight;
        
        //Properties not yet used in game, but copied from ini for round trip support
        public Pilot Pilot;
        public Faction Faction;
        public int DifficultyLevel;
        public string Behavior; //Unused? Always NOTHING in vanilla
        public float AtmosphereRange;
        public string MsgIdPrefix;
        public Color4? BurnColor;
        public Color4? AmbientColor;
        public string Parent;
        public string Voice;
        public string[] SpaceCostume;

        public SystemObject Clone()
        {
            var o = (SystemObject)MemberwiseClone();
            o.IdsInfo = IdsInfo?.ToArray();
            o.SpaceCostume = SpaceCostume?.ToArray();
            return o;
        }

        public SystemObject ()
		{
		}
    }
}

