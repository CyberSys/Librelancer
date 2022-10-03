// MIT License - Copyright (c) Callum McGing
// This file is subject to the terms and conditions defined in
// LICENSE, which is part of this source code package

using LibreLancer.Ini;

namespace LibreLancer.Data.Equipment
{
    public class CountermeasureDropper : Gun
    {
        [Entry("ai_range")] 
        public float AiRange;
    }
}