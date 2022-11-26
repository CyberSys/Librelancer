﻿// MIT License - Copyright (c) Callum McGing
// This file is subject to the terms and conditions defined in
// LICENSE, which is part of this source code package

using System;
using System.Collections.Generic;
using System.Numerics;
using LibreLancer.Render;

namespace LibreLancer.GameData
{
	public class StarSystem
	{
		public string Nickname;
        public Vector2 UniversePosition;
        public int Infocard;
		public string Name;
		//Background
		public Color4 BackgroundColor;
		//Starsphere
		public IDrawable StarsBasic;
		public IDrawable StarsComplex;
		public IDrawable StarsNebula;
		//Lighting
        public Color4 AmbientColor = Color4.Black;
		public List<RenderLight> LightSources = new List<RenderLight>();
		//Objects
		public List<SystemObject> Objects = new List<SystemObject>();
		//Nebulae
		public List<Nebula> Nebulae = new List<Nebula>();
		//Asteroid Fields
		public List<AsteroidField> AsteroidFields = new List<AsteroidField>();
		//Zones
		public List<Zone> Zones = new List<Zone>();
        public Dictionary<string, Zone> ZoneDict = new Dictionary<string, Zone>(StringComparer.OrdinalIgnoreCase);
		//Music
		public string MusicSpace;
		//Clipping
		public float FarClip;
        //Navmap
        public float NavMapScale;
        public Action StarspheresAction;
        public void LoadStarspheres()
        {
            if (StarspheresAction != null)
            {
                StarspheresAction();
                StarspheresAction = null;
            }
        }

        public List<string> ResourceFiles = new List<string>();
        //Optimisation
        public List<int> TexturePanelFiles = new List<int>();
        public StarSystem ()
		{
		}
	}
}

