using System;
using Proof;

namespace CrossyBro
{
    public class SingleRoadLane : Lane
    {
        private static readonly LaneType[] s_CannotSpawnAfter =
        {
            LaneType.DoubleRoad,
        };

        public override LaneType Type => LaneType.SingleRoad;
        public override int RowCount => 1;
        public override bool CanSpawnBackToBack => false;
        public override LaneType[] CannotSpawnAfter => s_CannotSpawnAfter;

    }

    public class DoubleRoadLane : Lane
    {
        private static readonly LaneType[] s_CannotSpawnAfter =
        {
            LaneType.SingleRoad,
        };

        public override LaneType Type => LaneType.DoubleRoad;
        public override int RowCount => 2;
        public override bool CanSpawnBackToBack => true;
        public override LaneType[] CannotSpawnAfter => s_CannotSpawnAfter;

    }
}