using System;
using Proof;

namespace CrossyBro
{
    // just for the train as well
    public class Vehicle : MovingLaneObject
    {
      

    }
    public class Car : Vehicle
    {
        public float WheelRadius = 2.0f;
        public float WheelRotationDirection = -1.0f;
        public Entity[] Wheels;

        private Quaternion[] m_WheelBaseRotations;
        private float m_WheelRotation;

        void OnCreate()
        {
            InitializeMovingLaneObject();
            CacheWheelRotations();
        }

        void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);
            UpdateWheels(deltaTime);
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