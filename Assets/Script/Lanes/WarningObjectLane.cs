using System;
using System.Collections.Generic;
using Proof;

namespace CrossyBro
{
    public abstract class WarningMovingObjectLane : Lane
    {
        private enum WarningLaneState
        {
            Waiting,
            Warning
        }

        public LaneDirection Direction = LaneDirection.Right;

        public Prefab[] MovingObjects;

        public float SpawnHeight = 0.0f;

        public float MinSpeed = 55.0f;
        public float MaxSpeed = 65.0f;

        // How long to wait between moving objects.
        public float MinWaitTime = 4f;
        public float MaxWaitTime = 8.0f;

        // How long the warning plays before the moving object spawns.
        public float MinWarningTime = 3.5f;
        public float MaxWarningTime = 3.9f;

        // Objects spawn slightly outside the visible lane.
        public float EdgeSpawnPadding = 40.0f;

        public float LaneWidth = 64.0f;

        protected float CurrentWarningTime => m_Timer;

        private float m_Speed;
        private float m_Timer;

        private WarningLaneState m_State = WarningLaneState.Waiting;

        private List<Entity> m_SpawnedObjects = new List<Entity>();

        private LaneManager m_LaneManager;


        protected virtual void OnCreate()
        {
            m_LaneManager = World.TryFindEntityByTag("LaneManager")?.GetScript<LaneManager>();

            float minSpeed = m_LaneManager != null ? m_LaneManager.IncreaseByDifficulty(MinSpeed, 25.0f) : MinSpeed;
            float maxSpeed = m_LaneManager != null ? m_LaneManager.IncreaseByDifficulty(MaxSpeed, 25.0f) : MaxSpeed;

            if (minSpeed < 0.1f)
                minSpeed = 0.1f;

            if (maxSpeed < minSpeed)
                maxSpeed = minSpeed;

            // One speed is selected for the entire lane.
            m_Speed = Proof.Random.Float(minSpeed, maxSpeed);

            // Make sure warning visuals/audio are OFF before waiting begins.
            OnWarningStopped();

            StartWaiting();
        }


        protected override void OnUpdate(float deltaTime)
        {
            if (MovingObjects == null || MovingObjects.Length == 0)
            {
                return;
            }

            // Clamp VALUE first, then min/max.
            // Old code had these arguments backwards and caused the timers
            // to count down much faster than real time.
            deltaTime = Mathf.Clamp(deltaTime, 0.0f, 0.1f);

            if (m_State == WarningLaneState.Waiting)
            {
                UpdateWaiting(deltaTime);
            }
            else if (m_State == WarningLaneState.Warning)
            {
                UpdateWarning(deltaTime);
            }
        }


        private void UpdateWaiting(float deltaTime)
        {
            m_Timer -= deltaTime;

            if (m_Timer > 0.0f)
                return;

            StartWarning();
        }


        private void UpdateWarning(float deltaTime)
        {
            m_Timer -= deltaTime;

            if (m_Timer > 0.0f)
                return;

            StopWarning();

            SpawnMovingObject();

            StartWaiting();
        }


        private void StartWaiting()
        {
            m_State = WarningLaneState.Waiting;

            float minimumWait = m_LaneManager != null ? m_LaneManager.DecreaseByDifficulty(MinWaitTime, 25.0f) : MinWaitTime;
            float maximumWait = m_LaneManager != null ? m_LaneManager.DecreaseByDifficulty(MaxWaitTime, 25.0f) : MaxWaitTime;

            if (maximumWait < minimumWait)
                maximumWait = minimumWait;

            m_Timer = Proof.Random.Float(minimumWait, maximumWait);

        }


        private void StartWarning()
        {
            m_State = WarningLaneState.Warning;

            float minimumWarning = MinWarningTime;
            float maximumWarning = MaxWarningTime;

            if (maximumWarning < minimumWarning)
                maximumWarning = minimumWarning;

            m_Timer = Proof.Random.Float(minimumWarning, maximumWarning);


            OnWarningStarted();
        }


        private void StopWarning()
        {
            OnWarningStopped();
        }


        protected virtual void OnWarningStarted()
        {

        }


        protected virtual void OnWarningStopped()
        {

        }


        private void SpawnMovingObject()
        {
            int objectIndex = Proof.Random.Int(0, MovingObjects.Length - 1);

            float spawnEdge = LaneWidth * 0.5f + EdgeSpawnPadding;

            Vector3 spawnPosition = Transform.Location;

            // Objects move across the lane along Z.
            if (Direction == LaneDirection.Right)
            {
                spawnPosition.z -= spawnEdge;
            }
            else
            {
                spawnPosition.z += spawnEdge;
            }

            spawnPosition.y = SpawnHeight;

            Entity objectEntity = World.Instantiate(MovingObjects[objectIndex], spawnPosition);

            m_SpawnedObjects.Add(objectEntity);

            MovingLaneObject movingObject = objectEntity.GetScriptInstance<MovingLaneObject>();

            if (movingObject != null)
            {
                float travelDistance = LaneWidth + EdgeSpawnPadding * 12.0f;

                movingObject.Initialize(Direction, m_Speed, travelDistance);
            }
        }


        protected override void OnDestroy()
        {
            StopWarning();

            for (int i = 0; i < m_SpawnedObjects.Count; i++)
            {
                Entity e = m_SpawnedObjects[i];

                if (Entity.IsValid(e))
                    World.DeleteEntity(e);
            }
        }
    }
}