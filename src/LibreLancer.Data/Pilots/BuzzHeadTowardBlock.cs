// MIT License - Copyright (c) Callum McGing
// This file is subject to the terms and conditions defined in
// LICENSE, which is part of this source code package

using System;
using System.Collections.Generic;
using LibreLancer.Ini;

namespace LibreLancer.Data.Pilots
{
    public class BuzzHeadTowardBlock : PilotBlock, ICustomEntryHandler
    {
        [Entry("buzz_min_distance_to_head_toward")]
        public float MinDistanceToHeadToward;
        [Entry("buzz_min_distance_to_head_toward_variance_percent")]
        public float MinDistanceToHeadTowardVariancePercent;
        [Entry("buzz_max_time_to_head_away")]
        public float MaxTimeToHeadAway;
        [Entry("buzz_head_toward_engine_throttle")]
        public float HeadTowardEngineThrottle;
        [Entry("buzz_head_toward_turn_throttle")]
        public float HeadTowardTurnThrottle;
        [Entry("buzz_head_toward_roll_throttle")]
        public float HeadTowardRollThrottle;
        [Entry("buzz_head_toward_roll_flip_direction")]
        public bool HeadTowardRollFlipDirection;
        [Entry("buzz_dodge_turn_throttle")] public float DodgeTurnThrottle;
        [Entry("buzz_dodge_cone_angle")] public float DodgeConeAngle;
        [Entry("buzz_dodge_cone_angle_variance_percent")]
        public float DodgeConeAngleVariancePercent;
        [Entry("buzz_dodge_waggle_axis_cone_angle")]
        public float DodgeWaggleAxisConeAngle;
        [Entry("buzz_dodge_roll_angle")] public float DodgeRollAngle;
        [Entry("buzz_dodge_interval_time")] public float DodgeIntervalTime;
        [Entry("buzz_dodge_interval_time_variance_percent")]
        public float DodgeIntervalTimeVariancePercent;

        [Entry("buzz_slide_throttle")] public float SlideThrottle;
        [Entry("buzz_slide_interval_time")] public float SlideIntervalTime;
        [Entry("buzz_slide_interval_time_variance_percent")] public float SlideIntervalVariancePercent;

        public List<HeadTowardsStyle> HeadTowardsStyleWeight = new List<HeadTowardsStyle>();
        public List<DirectionWeight> DodgeDirectionWeights = new List<DirectionWeight>();

        private static readonly CustomEntry[] _custom = new CustomEntry[]
        {
            new(
                "buzz_head_toward_style_weight",
                (s, e) =>
                    ((BuzzHeadTowardBlock) s).HeadTowardsStyleWeight.Add(new HeadTowardsStyle(e))),
            new("buzz_dodge_direction_weight",
                (s, e) => ((BuzzHeadTowardBlock) s).DodgeDirectionWeights.Add(new DirectionWeight(e)))

        };

        IEnumerable<CustomEntry> ICustomEntryHandler.CustomEntries => _custom;
    }
    
    public class HeadTowardsStyle
    {
        public string Style;
        public float Weight;

        public HeadTowardsStyle()
        {
        }

        public HeadTowardsStyle(Entry e)
        {
            Style = e[0].ToString();
            Weight = e[1].ToSingle();
        }
    }
}