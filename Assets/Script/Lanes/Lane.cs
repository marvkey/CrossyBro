using System;
using Proof;

namespace CrossyBro
{
    public enum LaneType
    {
        Grass,
        SingleRoad,
        DoubleRoad,
        TrainLane,
        Water
    }

    public enum LaneDirection
    {
        Left,
        Right
    }

    public abstract class Lane : Entity
    {
        public int LaneIndex { get; private set; }

        public abstract LaneType Type { get; }
        public abstract int RowCount { get; }
        public abstract bool CanSpawnBackToBack { get; }
        public abstract LaneType[] CannotSpawnAfter { get; }

        public float LaneWidth = 64.0f; // some laens might be wider like the car lanes
        
        public void Initialize(int laneIndex)
        {
            LaneIndex = laneIndex;
        }
        public bool CanSpawnAfter(Lane previousLane)
        {
            if (previousLane == null)
                return true;

            if (!CanSpawnBackToBack && previousLane.Type == Type)
                return false;

            for (int i = 0; i < CannotSpawnAfter.Length; i++)
            {
                if (CannotSpawnAfter[i] == previousLane.Type)
                    return false;
            }

            return true;
        }
    }
}