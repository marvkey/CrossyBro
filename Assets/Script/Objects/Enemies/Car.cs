using System;
using Proof;

namespace CrossyBro
{
    public class Car : Entity
    {
        public float Speed = 8.0f;
        public float WheelRadius = 2.0f;
        public float WheelRotationDirection = -1.0f;
        public Entity[] Wheels;

        private LaneDirection m_Direction = LaneDirection.Right;

        private float m_DistanceTravelled;
        private float m_MaxTravelDistance;

        private RigidBodyComponent m_RigidBody;

        private Quaternion m_BaseRotation;
        private bool m_HasBaseRotation;

        private Quaternion[] m_WheelBaseRotations;
        private float m_WheelRotation;

        void OnCreate()
        {
            m_RigidBody = GetComponent<RigidBodyComponent>();

            CacheWheelRotations();
        }

        void OnUpdate(float deltaTime)
        {
            UpdateWheels(deltaTime);
        }

        public void OnPhysicsUpdate(float fixedPhysicsDeltaTime)
        {
            if (m_RigidBody == null)
                return;

            float movement = Speed * fixedPhysicsDeltaTime;

            // Always move using the car entity's actual direction.
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

        private void CacheWheelRotations()
        {
            if (Wheels == null)
                return;

            m_WheelBaseRotations = new Quaternion[Wheels.Length];

            for (int i = 0; i < Wheels.Length; i++)
            {
                if (Wheels[i] == null)
                    continue;

                m_WheelBaseRotations[i] = Wheels[i].Transform.RotationQuat;
            }
        }

        private void UpdateWheels(float deltaTime)
        {
            if (Wheels == null || m_WheelBaseRotations == null || WheelRadius <= 0.0f)
                return;

            float angularSpeed = Speed / WheelRadius;
            m_WheelRotation += angularSpeed * WheelRotationDirection * deltaTime;

            if (Mathf.Abs(m_WheelRotation) >= Mathf.PI * 2.0f)
                m_WheelRotation = 0.0f;

            Quaternion wheelSpin = new Quaternion(
                new Vector3(m_WheelRotation, 0.0f, 0.0f)
            );

            for (int i = 0; i < Wheels.Length; i++)
            {
                if (Wheels[i] == null)
                    continue;

                Wheels[i].Transform.RotationQuat =
                    m_WheelBaseRotations[i] * wheelSpin;
            }
        }
    }
}