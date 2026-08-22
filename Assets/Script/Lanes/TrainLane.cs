using Proof;
using System;
using System.Collections.Generic;

namespace CrossyBro
{
    public class TrainLane : WarningMovingObjectLane
    {
        public override LaneType Type => LaneType.TrainLane;
        public override int RowCount => 1;
        public override bool CanSpawnBackToBack => true;
        public override LaneType[] CannotSpawnAfter => Array.Empty<LaneType>();

        public Entity[] RailCrossing;

        public StaticMesh DarkRailRoad;
        public StaticMesh BrightRailRoad;

        public Prefab RailAlarm;

        public float WarningFlashInterval = 0.25f;

        private bool m_WarningActive = false;
        private bool m_Bright = false;
        private float m_WarningFlashTimer = 0.0f;

        private List<Entity> m_RailAlarms = new List<Entity>();


        protected override void OnCreate()
        {
            Direction = Proof.Random.Bool() == false ? LaneDirection.Right : LaneDirection.Left;

            Log.Trace($"Lane Direction {Direction.ToString()}");

            // Railroad warning signs always start dark.
            SetRailCrossingMesh(DarkRailRoad);

            base.OnCreate();
        }


        protected override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);

            if (!m_WarningActive)
                return;

            m_WarningFlashTimer -= deltaTime;

            if (m_WarningFlashTimer > 0.0f)
                return;

            m_WarningFlashTimer = WarningFlashInterval;
            m_Bright = !m_Bright;

            if (m_Bright)
                SetRailCrossingMesh(BrightRailRoad);
            else
                SetRailCrossingMesh(DarkRailRoad);
        }


        protected override void OnWarningStarted()
        {
            m_WarningActive = true;

            m_Bright = true;
            m_WarningFlashTimer = WarningFlashInterval;

            SetRailCrossingMesh(BrightRailRoad);

            SpawnRailAlarms();
        }


        protected override void OnWarningStopped()
        {
            m_WarningActive = false;
            m_WarningFlashTimer = 0.0f;
            m_Bright = false;

            // Warning must ALWAYS finish dark before the train spawns.
            SetRailCrossingMesh(DarkRailRoad);

            DeleteRailAlarms();
        }


        private void SpawnRailAlarms()
        {
            if (RailAlarm == null || RailCrossing == null)
                return;

            for (int i = 0; i < RailCrossing.Length; i++)
            {
                Entity railCrossing = RailCrossing[i];

                if (!Entity.IsValid(railCrossing))
                    continue;

                Vector3 spawnPosition = railCrossing.WorldTransform.Location;

                Entity railAlarm = World.Instantiate(RailAlarm, spawnPosition);

                if (Entity.IsValid(railAlarm))
                    m_RailAlarms.Add(railAlarm);
            }
        }


        private void DeleteRailAlarms()
        {
            for (int i = 0; i < m_RailAlarms.Count; i++)
            {
                Entity railAlarm = m_RailAlarms[i];

                if (Entity.IsValid(railAlarm))
                    World.DeleteEntity(railAlarm);
            }

            m_RailAlarms.Clear();
        }


        private void SetRailCrossingMesh(StaticMesh mesh)
        {
            if (mesh == null || RailCrossing == null)
                return;

            for (int i = 0; i < RailCrossing.Length; i++)
            {
                Entity railCrossing = RailCrossing[i];

                if (!Entity.IsValid(railCrossing))
                    continue;

                MeshComponent meshComponent = railCrossing.GetComponent<MeshComponent>();

                if (meshComponent == null)
                    continue;

                meshComponent.Mesh = mesh;
            }
        }


        protected override void OnDestroy()
        {
            DeleteRailAlarms();

            base.OnDestroy();
        }
    }
}