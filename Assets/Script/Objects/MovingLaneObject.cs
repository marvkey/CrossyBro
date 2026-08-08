using System;
using Proof;

namespace CrossyBro
{
    public class MovingLaneObject : Entity
    {
        public float Speed = 8.0f;

        private LaneDirection m_Direction = LaneDirection.Right;

        private float m_DistanceTravelled;
        private float m_MaxTravelDistance;

        protected RigidBodyComponent m_RigidBody;

        private Quaternion m_BaseRotation;
        private bool m_HasBaseRotation;

        void OnCreate()
        {
            InitializeMovingLaneObject();
        }

        protected void InitializeMovingLaneObject()
        {
            m_RigidBody = GetComponent<RigidBodyComponent>();
        }

        public void OnPhysicsUpdate(float fixedPhysicsDeltaTime)
        {
            if (m_RigidBody == null)
                return;

            float movement = Speed * fixedPhysicsDeltaTime;

            // Always move using the entity's actual direction.
            m_RigidBody.Location += -Transform.Forward * movement;

            m_DistanceTravelled += movement;

            if (m_MaxTravelDistance > 0.0f && m_DistanceTravelled >= m_MaxTravelDistance)
                World.DeleteEntity(this);
        }

        public void Initialize(LaneDirection direction, float speed, float maxTravelDistance)
        {
            if (m_RigidBody == null)
                m_RigidBody = GetComponent<RigidBodyComponent>();

            Speed = speed;
            m_MaxTravelDistance = maxTravelDistance;
            m_DistanceTravelled = 0.0f;

            // Save the rotation that came from the prefab.
            if (!m_HasBaseRotation)
            {
                m_BaseRotation = Transform.RotationQuat;
                m_HasBaseRotation = true;
            }

            SetDirection(direction);
        }

        public void SetDirection(LaneDirection direction)
        {
            m_Direction = direction;

            Quaternion rotation = m_BaseRotation;

            // Right uses the prefab's original rotation.
            // Left turns the prefab around by 180 degrees.
            if (m_Direction == LaneDirection.Left)
            {
                Quaternion turnAround = new Quaternion(
                    new Vector3(0.0f, Mathf.PI, 0.0f)
                );

                rotation = m_BaseRotation * turnAround;
            }

            Transform.RotationQuat = rotation;

            if (m_RigidBody != null)
                m_RigidBody.Rotation = rotation;
        }
    }
}