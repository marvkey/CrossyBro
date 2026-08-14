using System;
using Proof;

namespace CrossyBro
{
	public class MovingLaneObject : Entity
	{
		public float Speed = 8.0f;
		public bool CarryPlayer = false;

		private LaneDirection m_Direction = LaneDirection.Right;

		private float m_DistanceTravelled;
		private float m_MaxTravelDistance;

		protected RigidBodyComponent m_RigidBody;

		private Quaternion m_BaseRotation;
		private bool m_HasBaseRotation;

		private Entity m_MovementRoot;
		public Vector3 Velocity => -Transform.Forward * Speed;
		public Vector3 MovementDelta { get; private set; }

		public Entity MovementRoot
		{
			get
			{
				if (m_MovementRoot != null && Entity.IsValid(m_MovementRoot))
					return m_MovementRoot;

				return this;
			}
		}

		void OnCreate()
		{
			InitializeMovingLaneObject();
		}

		protected void InitializeMovingLaneObject()
		{
			m_RigidBody = GetComponent<RigidBodyComponent>();

			if (CarryPlayer)
				CreateMovementRoot();
		}

		private void CreateMovementRoot()
		{
			if (m_MovementRoot != null && Entity.IsValid(m_MovementRoot))
				return;

			m_MovementRoot = World.CreateEntity("MovingObjectRoot");

			m_MovementRoot.Transform.Location = m_RigidBody.Location;
			m_MovementRoot.Transform.Scale = new Vector3(1.0f);
		}

		public void OnPhysicsUpdate(float fixedPhysicsDeltaTime)
		{
			if (m_RigidBody == null)
				return;

			float movement = Speed * fixedPhysicsDeltaTime;

			MovementDelta = -Transform.Forward * movement;

			// Keep the actual moving object exactly how it already moved.
			m_RigidBody.Translate(MovementDelta);

			// The clean root only mirrors the object's movement.
			// The player can safely parent to this without inheriting
			// the log's stretched scale.
			if (m_MovementRoot != null && Entity.IsValid(m_MovementRoot))
				m_MovementRoot.Transform.Location += MovementDelta;

			m_DistanceTravelled += movement;

			if (m_MaxTravelDistance > 0.0f && m_DistanceTravelled >= m_MaxTravelDistance)
			{
				if (m_MovementRoot != null && Entity.IsValid(m_MovementRoot))
					World.DeleteEntity(m_MovementRoot);

				World.DeleteEntity(this);
			}

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