// MIT License - Copyright (c) Callum McGing
// This file is subject to the terms and conditions defined in
// LICENSE, which is part of this source code package

using LibreLancer.Thorn;

namespace LibreLancer.Thn.Events
{
    public class SetCameraEvent : ThnEvent
    {
        public SetCameraEvent() { }

        public SetCameraEvent(ThornTable table) : base(table) { }

        public override void Run(ThnScriptInstance instance)
        {
            instance.Cutscene.SetCamera(Targets[1]);
        }
    }
}