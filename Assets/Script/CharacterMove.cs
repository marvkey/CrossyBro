using System;
using System.Text;
using Proof;

namespace CrossyBro
{
	public class CharacterMovement : Entity
	{
		public float PlayerHeight = 2.0f;
		public float JumpHeight = 1.0f;
		public float JumpDuration = 0.22f;
		public float RotationSpeed = 10.0f;

		public Entity Camera;

		private RigidBodyComponent m_RigidBody;

		private bool m_IsJumping = false;
		private float m_JumpTime = 0.0f;
		private Vector3 m_JumpStart;
		private Vector3 m_JumpTarget;

		private bool m_IsTurning = false;
		private float m_TargetYRotation = 0.0f;

		private Vector2 m_RotateRate = Vector2.Zero;
		private float m_RotationAccumulator = 0.0f;

		public bool IsJumping => m_IsJumping;
		public bool IsTurning => m_IsTurning;

		private bool m_Dead = false;
		public bool Dead =>m_Dead;

		private MovingLaneObject m_CurrentMovingObject;
		void OnCreate()
		{
			m_RigidBody = GetComponent<RigidBodyComponent>();
			m_TargetYRotation = Transform.Rotation.y;
			m_RotationAccumulator = Transform.Rotation.y;
			CollisionStayEvent +=OnCollisionStay;
			CollisionEnterEvent += OnCollisionEnter;
			CollisionLeaveEvent += OnCollisionLeave;
		}

		void OnUpdate(float deltaTime)
		{
			UpdateRotate();
		}

		void OnPhysicsUpdate(float fixedPhysicsDeltaTime)
		{
			UpdateRotation(fixedPhysicsDeltaTime);
			if(!Dead)
				UpdateJump(fixedPhysicsDeltaTime);

			//if (m_CurrentMovingObject != null && Entity.IsValid(m_CurrentMovingObject))
				//m_RigidBody.Translate(m_CurrentMovingObject.MovementDelta);
		}

		void OnCollisionEnter(Entity e)
		{
			if(e.HasScript<Car>())
			{

				Car car = e.GetScriptInstance<Car>();
				if (car == null)
					return;

				SetDead(car.Velocity);
			}
		}
		private void SetDead(Vector3 carVelocity)
		{
			SetDead();

			m_RigidBody.Constraints = PhysicsActorConstraint.None;

			Vector3 hitDirection = carVelocity.Normalized;
			float hitStrength = carVelocity.Magnitude;

			m_RigidBody.Gravity = true;
			m_RigidBody.Velocity = Vector3.Zero;

			// Throw the player in the direction the car is travelling.
			m_RigidBody.AddForce(
				hitDirection * hitStrength * 2.0f + Mathf.Up * 5.0f,
				ForceMode.Impulse
			);

			// Rotate around an axis perpendicular to the car movement,
			// so the player tumbles in the direction of the crash.
			Vector3 tumbleAxis = new Vector3(
				hitDirection.z,
				0.0f,
				-hitDirection.x
			);

			m_RigidBody.AddTorque(
				tumbleAxis * hitStrength * 4.0f,
				ForceMode.Impulse
			);
		}
		void SetDead()
		{
			m_Dead = true; 
			Log.Info("Player dead");
		}

		void OnCollisionStay(Entity e)
		{
			if (IsJumping)
				return;

			if (m_CurrentMovingObject != null)
				return;


			MovingLaneObject movingObject = e.GetScriptInstance<MovingLaneObject>();

			if (movingObject == null)
				return;

			if (!movingObject.CarryPlayer)
				return;

			m_CurrentMovingObject = movingObject;
			this.Parent = m_CurrentMovingObject.MovementRoot;
			m_RigidBody.Velocity = Vector3.Zero;
			m_RigidBody.Gravity = false;
			m_RigidBody.IsKinematic = true; // whatever your exposed setter is
			Log.Info("Set parent");
					

		}

		void OnCollisionLeave(Entity e)
		{
			if (!e.HasScriptInstance<MovingLaneObject>())
				return;

			MovingLaneObject obj = e.GetScriptInstance<MovingLaneObject>();

			if (obj == m_CurrentMovingObject)
			{
				m_CurrentMovingObject = null;
				Parent = null;
				{
					Parent = null;

					m_RigidBody.IsKinematic = false;
					m_RigidBody.Velocity = Vector3.Zero;

					m_CurrentMovingObject = null;
				}

			}
		}
		public void Rotate(Vector2 axis)
		{
			m_RotateRate += axis;
		}

		private void UpdateRotate()
		{
			if (m_RotateRate == Vector2.Zero)
				return;

			float rotationAmount = m_RotateRate.x * RotationSpeed * World.GetDeltaTime();

			m_RotationAccumulator += rotationAmount;

			Quaternion rotation = Quaternion.EularToQuat(new Vector3(0.0f, -Mathf.DegreesToRadians(m_RotationAccumulator), 0.0f));

			m_RigidBody.Rotation = rotation;

			if (Camera != null)
				Camera.Transform.RotationQuat = rotation;

			m_TargetYRotation = -m_RotationAccumulator;

			m_RotateRate = Vector2.Zero;
		}

		public bool Jump(Vector3 direction)
		{
			if (m_IsJumping || m_IsTurning || !IsGrounded())
				return false;

			if(Parent != null)
			{
				Parent = null;

				m_RigidBody.IsKinematic = false;
				m_RigidBody.Velocity = Vector3.Zero;

				m_CurrentMovingObject = null;
			}
				

			float moveDistance = WorldData.GridSize;

			direction.y = 0.0f;
			direction.x = Mathf.Round(direction.x);
			direction.z = Mathf.Round(direction.z);

			if (!CanJump(direction, moveDistance))
				return false;

			m_JumpStart = SnapPosition(m_RigidBody.Location, moveDistance);
			m_JumpTarget = m_JumpStart + direction * moveDistance;
			m_JumpTarget = SnapPosition(m_JumpTarget, moveDistance);
			m_JumpTarget.y = m_JumpStart.y;

			m_RigidBody.Location = m_JumpStart;
			m_RigidBody.Velocity = Vector3.Zero;
			m_RigidBody.Gravity = false;

			m_JumpTime = 0.0f;
			m_IsJumping = true;

			return true;
		}

		public bool TurnLeft()
		{
			if (m_IsJumping || m_IsTurning)
				return false;

			m_TargetYRotation += 90.0f;
			m_RotationAccumulator = -m_TargetYRotation;
			m_IsTurning = true;

			return true;
		}

		public bool TurnRight()
		{
			if (m_IsJumping || m_IsTurning)
				return false;

			m_TargetYRotation -= 90.0f;
			m_RotationAccumulator = -m_TargetYRotation;
			m_IsTurning = true;

			return true;
		}

		private bool CanJump(Vector3 direction, float distance)
		{
			RaycastData data = new RaycastData();
			data.Origin = m_RigidBody.Location;
			data.Direction = direction;
			data.MaxDistance = distance;

			data.ExcludedEntities = new ulong[1];
			data.ExcludedEntities[0] = ID;

			if (Physics.RayCast(data, out RaycastHit hit))
			{
				if (hit.Entity.HasSubTag("Tree"))
					return false;
				if(hit.Entity.Name == "Wall")
					return false; 
			}

			return true;
		}

		private void UpdateJump(float deltaTime)
		{
			if (!m_IsJumping)
				return;

			m_JumpTime += deltaTime;

			float t = Mathf.Clamp(m_JumpTime / JumpDuration, 0.0f, 1.0f);

			Vector3 position = m_JumpStart + (m_JumpTarget - m_JumpStart) * t;
			position.y += Mathf.Sin(t * Mathf.PI) * JumpHeight;

			m_RigidBody.Velocity = Vector3.Zero;
			m_RigidBody.Location = position;

			if (t >= 1.0f)
			{
				m_RigidBody.Location = m_JumpTarget;
				m_RigidBody.Velocity = Vector3.Zero;
				m_RigidBody.Gravity = true;

				m_IsJumping = false;

				Log.Trace($"New Position {Transform.Location.ToString()}");
			}
		}

		private void UpdateRotation(float deltaTime)
		{
			if (!m_IsTurning)
				return;

			Quaternion targetRotation = new Quaternion(new Vector3(0.0f, Mathf.DegreesToRadians(m_TargetYRotation), 0.0f));

			m_RigidBody.Rotation = Quaternion.Lerp(m_RigidBody.Rotation, targetRotation, deltaTime * RotationSpeed);

			if (Camera != null)
				Camera.Transform.RotationQuat = m_RigidBody.Rotation;

			float dot = Mathf.Abs(Quaternion.Dot(m_RigidBody.Rotation, targetRotation));
			dot = Mathf.Clamp(dot, 0.0f, 1.0f);

			float angleDifference = Mathf.Acos(dot) * 2.0f;

			if (angleDifference < 0.01f)
			{
				m_RigidBody.Rotation = targetRotation;

				if (Camera != null)
					Camera.Transform.RotationQuat = targetRotation;

				m_RotationAccumulator = -m_TargetYRotation;
				m_IsTurning = false;

				Log.Trace($"New rotation {Transform.Rotation.ToString()}");
			}
		}

		private Vector3 SnapPosition(Vector3 position, float gridSize)
		{
			position.x = Mathf.Round(position.x / gridSize) * gridSize;
			position.z = Mathf.Round(position.z / gridSize) * gridSize;
			return position;
		}

		public bool IsGrounded()
		{
			RaycastData data = new RaycastData();
			data.Origin = m_RigidBody.Location;
			data.Direction = Mathf.Down;
			data.MaxDistance = PlayerHeight * 0.5f + 0.2f;

			data.ExcludedEntities = new ulong[1];
			data.ExcludedEntities[0] = ID;

			return Physics.RayCast(data, out RaycastHit hit);
		}
	}
}