using System;
using Proof;

namespace CrossyBro
{
    public class WaterLane : MovingObjectLane
    {
        private static readonly LaneType[] s_CannotSpawnAfter =
        {
        };

        public override LaneType Type => LaneType.Water;
        public override int RowCount => 1;
        public override bool CanSpawnBackToBack => true;
        public override LaneType[] CannotSpawnAfter => s_CannotSpawnAfter;

        public LaneDirection Direction = LaneDirection.Right;

        protected override void OnCreate()
        {
            Direction = Proof.Random.Bool() == false ? LaneDirection.Right : LaneDirection.Left;

            Log.Trace($"Water Direction {Direction.ToString()}");

            base.OnCreate();
        }

        protected override LaneDirection GetDirectionForRow(int rowIndex)
        {
            return Direction;
        }
    }
}