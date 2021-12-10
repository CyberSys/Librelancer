﻿// MIT License - Copyright (c) Malte Rupprecht
// This file is subject to the terms and conditions defined in
// LICENSE, which is part of this source code package

using System;
using System.Collections.Generic;
using System.Linq;

using LibreLancer.Ini;

namespace LibreLancer.Data.Solar
{
	public class CollisionGroup : ICustomEntryHandler
	{
        [Entry("obj")]
        public string obj;
        [Entry("child_impulse")]
        public float ChildImpulse;
        [Entry("debris_type")]
        public string DebrisType;
        [Entry("mass")]
        public float Mass;
        [Entry("hit_pts")]
        public float HitPts;
        [Entry("separable", Presence=true)]
        public bool Separable;
        [Entry("root_health_proxy")]
        public bool RootHealthProxy;
        [Entry("parent_impulse")]
        public float ParentImpulse;
        //TODO: See how many of these are valid in vanilla
        [Entry("group_dmg_hp")]
        public string GroupDmgHp;
        [Entry("group_dmg_obj")]
        public string GroupDmgObj;
        [Entry("dmg_hp")]
        public string DmgHp;
        [Entry("dmg_obj")]
        public string DmgObj;

        //TODO
        //fuse = fuse_docking_ring, 0.000000, 1
        private static readonly CustomEntry[] _custom = new CustomEntry[]
        {
            new("fuse", CustomEntry.Ignore)
        };
        
        IEnumerable<CustomEntry> ICustomEntryHandler.CustomEntries => _custom;
    }
}