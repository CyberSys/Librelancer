// MIT License - Copyright (c) Callum McGing
// This file is subject to the terms and conditions defined in
// LICENSE, which is part of this source code package

using LibreLancer.GameData.Items;

namespace LibreLancer
{
    public class SEngineComponent : GameComponent
    {
        public EngineEquipment Engine;
        public float Speed;
        public SEngineComponent(GameObject parent) : base(parent)
        {
        }
    }
}