using System;
using Proof;

namespace CrossyBro
{
    public class Car : Entity
    {
        public float Speed = 8.0f;
        public float WheelRadius =2.0f;
        public float WheelRotationDirection = -1.0f;
        public Entity[] Wheels;

        private LaneDirection m_Direction =  LaneDirection.Right;

        private float m_DistanceTravelled;
        private float m_MaxTravelDistance;
        private Vector3 m_MoveDirection
        {
            get
            {
                    return -Transform.Forward;
            } 

        }
        private Quaternion[] m_WheelBaseRotations;
        private float m_WheelRotation;

        void OnCreate()
        {
            //CacheWheelRotations();
        }

        void OnUpdate(float deltaTime)
        {
         
         
        }

        public void OnPhysicsUpdate(float fixedPhysicsDeltaTime)
        {
            //UpdateWheels(fixedPhysicsDeltaTime);
            GetComponent<RigidBodyComponent>().Location += m_MoveDirection * Speed * fixedPhysicsDeltaTime;

            float movement = Speed * fixedPhysicsDeltaTime;

            m_DistanceTravelled += movement;

            if (m_DistanceTravelled >= m_MaxTravelDistance)
                World.DeleteEntity(this);
        }

        public void SetDirection(LaneDirection direction)
        {
            m_Direction = direction;
            switch (m_Direction)
            {
                case LaneDirection.Left:
                    Transform.RotationQuat = Quaternion.LookRotation(Transform.Forward, Mathf.Up);
                    break;
                case LaneDirection.Right:
                    Transform.RotationQuat = Quaternion.LookRotation(-Transform.Forward, Mathf.Up);
                    break;
            }
        }


        public void Initialize(LaneDirection direction, float speed, float maxTravelDistance)
        {
            Speed = speed;
            m_MaxTravelDistance = maxTravelDistance;
            m_DistanceTravelled = 0.0f;

            SetDirection(direction);
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

            Quaternion wheelSpin = new Quaternion(new Vector3(m_WheelRotation, 0.0f, 0.0f));

            for (int i = 0; i < Wheels.Length; i++)
            {
                if (Wheels[i] == null)
                    continue;

                Wheels[i].Transform.RotationQuat = m_WheelBaseRotations[i] * wheelSpin;
            }
        }
    }
}