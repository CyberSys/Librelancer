// MIT License - Copyright (c) Callum McGing
// This file is subject to the terms and conditions defined in
// LICENSE, which is part of this source code package

using System.Numerics;
using LibreLancer.Thorn;

namespace LibreLancer.Thn.Events
{
    public class StartPathAnimationEvent : ThnEvent
    {
        public float StartPercent;
        public float StopPercent;
        public Vector3 Offset;
        public AttachFlags Flags;

        public StartPathAnimationEvent()
        {
        }

        public StartPathAnimationEvent(ThornTable table) : base(table)
        {
            if (GetProps(table, out var props))
            {
                GetValue(props, "start_percent", out StartPercent);
                GetValue(props, "stop_percent", out StopPercent, 1);
                GetValue(props, "flags", out Flags);
                GetValue(props, "offset", out Offset);
            }
        }

        public override void Run(ThnScriptInstance instance)
        {
            var obj = instance.Objects[Targets[0]];
            var path = instance.Objects[Targets[1]];
            instance.AddProcessor(new PathAnimation()
            {
                Path = path,
                Object = obj,
                Event = this
            });
        }

        class PathAnimation : ThnEventProcessor
        {
            public ThnObject Object;
            public ThnObject Path;
            public StartPathAnimationEvent Event;

            double time = 0;

            public override bool Run(double delta)
            {
                time += delta;
                Process(Event.GetT((float)time));
                return true;
            }

            void Process(float t)
            {
                float pct = MathHelper.Lerp(Event.StartPercent, Event.StopPercent, t);
                var path = Path.Entity.Path;
                var mat = Path.Rotate * Matrix4x4.CreateTranslation(Path.Translate);
                var pos = Vector3.Transform(path.GetPosition(pct) + Event.Offset, mat);
                if ((Event.Flags & AttachFlags.LookAt) == AttachFlags.LookAt)
                {
                    var orient = Matrix4x4.CreateFromQuaternion(QuaternionEx.LookRotation(path.GetDirection(pct, Event.StartPercent > Event.StopPercent), Vector3.UnitY)) * Path.Rotate;
                    Object.Rotate = orient;
                }
                else if ((Event.Flags & AttachFlags.Orientation) == AttachFlags.Orientation)
                {
                    var orient = Matrix4x4.CreateFromQuaternion(path.GetOrientation(pct)) * Path.Rotate;
                    Object.Rotate = orient;
                }

                if ((Event.Flags & AttachFlags.Position) == AttachFlags.Position)
                {
                    Object.Translate = pos;
                }
            }
        }
    }
}
