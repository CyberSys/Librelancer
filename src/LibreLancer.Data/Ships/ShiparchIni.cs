﻿// MIT License - Copyright (c) Callum McGing
// This file is subject to the terms and conditions defined in
// LICENSE, which is part of this source code package

using System;
using System.Collections.Generic;
using System.Linq;
using LibreLancer.Ini;
namespace LibreLancer.Data.Ships
{
	public class ShiparchIni : IniFile
	{
        [Section("ship")]
		public List<Ship> Ships = new List<Ship>();

		public ShiparchIni ()
		{
		}
        
		public void ParseAllInis(IEnumerable<string> paths, FreelancerData fldata)
		{
            ParseAndFill(paths, fldata.VFS);
        }
    }
}

