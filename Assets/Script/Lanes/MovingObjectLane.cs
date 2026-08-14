using System;
using System.Collections.Generic;
using Proof;

namespace CrossyBro
{
	public abstract class MovingObjectLane : Lane
	{
		protected class MovingObjectStream
		{
			public int RowIndex;
			public LaneDirection Direction;
			public float SpawnTimer;
		}

		public Prefab[] MovingObjects;

		public float SpawnHeight = 0.0f;
		public float MinSpeed = 4.0f;
		public float MaxSpeed = 10.0f;

		// Base distance between objects.
		public float MinSpawnPadding = 8.0f;
		public float MaxSpawnPadding = 14.0f;

		// Faster lanes receive additional distance between objects.
		public float SpawnPaddingPerSpeed = 0.5f;

		// Objects spawn slightly outside the visible lane.
		public float EdgeSpawnPadding = 24.0f;

		private float m_Speed;
		private MovingObjectStream[] m_MovingObjectStreams;

		protected abstract LaneDirection GetDirectionForRow(int rowIndex);

		private List<Entity> m_SpawnedObjects = new List<Entity>();

		LaneManager m_LaneManager ;
		protected virtual void OnCreate()
		{
			m_LaneManager = World.TryFindEntityByTag("LaneManager").GetScript<LaneManager>();
			if (MovingObjects == null || MovingObjects.Length == 0)
				return;
			float minSpeed = m_LaneManager!= null ? m_LaneManager.IncreaseByDifficulty(MinSpeed,40.0f) : MinSpeed;
			float maxSpeed =  m_LaneManager != null ? m_LaneManager.IncreaseByDifficulty(MaxSpeed,40.0f) : MaxSpeed;
			

			if (minSpeed < 0.1f)
				minSpeed = 0.1f;

			if (maxSpeed < minSpeed)
				maxSpeed = minSpeed;

			// One speed is selected for the entire lane.
			m_Speed = Proof.Random.Float(minSpeed, maxSpeed);

			m_MovingObjectStreams = new MovingObjectStream[RowCount];

			for (int i = 0; i < RowCount; i++)
			{
				MovingObjectStream stream = new MovingObjectStream();
				stream.RowIndex = i;
				stream.Direction = GetDirectionForRow(i);

				// Prevent every stream from spawning at the exact same time.
				stream.SpawnTimer = Proof.Random.Float(0.0f, GetNextSpawnDelay());

				m_MovingObjectStreams[i] = stream;
			}
		}

		public void OnUpdate(float deltaTime)
		{
			if (m_MovingObjectStreams == null)
				return;

			if (MovingObjects.Length == 0)
				Log.Error($"There are no moving objects on this lane prefab");

			for (int i = 0; i < m_MovingObjectStreams.Length; i++)
			{
				MovingObjectStream stream = m_MovingObjectStreams[i];

				stream.SpawnTimer -= deltaTime;

				if (stream.SpawnTimer > 0.0f)
					continue;

				SpawnMovingObject(stream);
				stream.SpawnTimer = GetNextSpawnDelay();
			}
		}

		private void SpawnMovingObject(MovingObjectStream stream)
		{
			int objectIndex = Proof.Random.Int(0, MovingObjects.Length - 1);

			float rowSize = WorldData.GridSize * 0.5f;
			float rowOffset = (stream.RowIndex - (RowCount - 1) * 0.5f) * rowSize;
			float spawnEdge = LaneWidth * 0.5f + EdgeSpawnPadding;

			Vector3 spawnPosition = Transform.Location;

			// Single road stays directly in the center.
			// Double road puts each direction on one side of X.
			if (RowCount == 2)
			{
				float sideOffset = WorldData.GridSize * 0.5f;

				if (stream.RowIndex == 0)
					spawnPosition.x = Transform.Location.x - sideOffset;
				else
					spawnPosition.x = Transform.Location.x + sideOffset;
			}

			// Objects move across the lane along Z.
			if (stream.Direction == LaneDirection.Right)
			{
				spawnPosition.z -= spawnEdge;
			}
			else
			{
				spawnPosition.z += spawnEdge;
			}

			spawnPosition.y = SpawnHeight;
			Transform transform = new Transform();
			transform.Scale = new Vector3(2.0f);
			//if(LaneDirection.Left == stream.Direction)
			//	transform.Rotation = new Vector3(0,180,0);
			transform.Location = spawnPosition;

			Entity objectEntity = World.Instantiate(MovingObjects[objectIndex], spawnPosition);
			m_SpawnedObjects.Add(objectEntity);

			MovingLaneObject movingObject = objectEntity.GetScriptInstance<MovingLaneObject>();

			if (movingObject != null)
			{
				float travelDistance = LaneWidth + EdgeSpawnPadding * 4.0f;
				movingObject.Initialize(stream.Direction, m_Speed, travelDistance);
			}
		}

		private float GetNextSpawnDelay()
		{
			float speedPadding = m_Speed * SpawnPaddingPerSpeed;		//Speed 6  → spacing receives +3 units
																	//Speed 12 → spacing receives +6 units

			float minimumDistance =  m_LaneManager!= null? m_LaneManager.DecreaseByDifficulty(MinSpawnPadding + speedPadding, 20.0f) : MinSpawnPadding + speedPadding;
			float maximumDistance =  m_LaneManager!= null ? m_LaneManager.DecreaseByDifficulty(MaxSpawnPadding + speedPadding, 20.0f) : MaxSpawnPadding + speedPadding;

			if (maximumDistance < minimumDistance)
				maximumDistance = minimumDistance;

			float spawnDistance = Proof.Random.Float(minimumDistance, maximumDistance);

			// Time required for an object to travel the selected spacing distance.
			return spawnDistance / m_Speed;
		}

		protected override void OnDestroy()
		{
			for (int i = 0; i < m_SpawnedObjects.Count; i++)
			{
				Entity e = m_SpawnedObjects[i];

				if (Entity.IsValid(e))
					World.DeleteEntity(e);
			}
		}
	}
}