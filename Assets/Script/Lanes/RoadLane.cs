using System;
using System.Collections.Generic;
using System.Linq;
using Proof;
using Random = System.Random;

namespace CrossyBro
{
	public abstract class RoadLane : Lane
	{
		protected class TrafficStream
		{
			public int RowIndex;
			public LaneDirection Direction;
			public float SpawnTimer;
		}

		public Prefab[] Cars;

		public float MinSpeed = 4.0f;
		public float MaxSpeed = 10.0f;

		// Base distance between cars.
		public float MinSpawnPadding = 8.0f;
		public float MaxSpawnPadding = 14.0f;

		// Faster lanes receive additional distance between cars.
		public float SpawnPaddingPerSpeed = 0.5f;

		// Cars spawn slightly outside the visible lane.
		public float EdgeSpawnPadding = 4.0f;

		private float m_Speed;
		private TrafficStream[] m_TrafficStreams;

		protected abstract LaneDirection GetDirectionForRow(int rowIndex);

		private List<Entity> m_SpawnedCars = new  List<Entity>();
		protected virtual void OnCreate()
		{
			if (Cars == null || Cars.Length == 0)
				return;

			float minSpeed = MinSpeed;
			float maxSpeed = MaxSpeed;

			if (minSpeed < 0.1f)
				minSpeed = 0.1f;

			if (maxSpeed < minSpeed)
				maxSpeed = minSpeed;

			// One speed is selected for the entire road lane.
			m_Speed = Proof.Random.Float(minSpeed, maxSpeed);

			m_TrafficStreams = new TrafficStream[RowCount];

			for (int i = 0; i < RowCount; i++)
			{
				TrafficStream stream = new TrafficStream();
				stream.RowIndex = i;
				stream.Direction = GetDirectionForRow(i);

				// Prevent every stream from spawning at the exact same time.
				stream.SpawnTimer = Proof.Random.Float(0.0f, GetNextSpawnDelay());

				m_TrafficStreams[i] = stream;
			}
		}

		public void OnUpdate(float deltaTime)
		{
			if (m_TrafficStreams == null)
				return;

			for (int i = 0; i < m_TrafficStreams.Length; i++)
			{
				TrafficStream stream = m_TrafficStreams[i];

				stream.SpawnTimer -= deltaTime;

				if (stream.SpawnTimer > 0.0f)
					continue;

				SpawnCar(stream);
				stream.SpawnTimer = GetNextSpawnDelay();
			}
		}

		private void SpawnCar(TrafficStream stream)
		{
			int carIndex = Proof.Random.Int(0, Cars.Length - 1);

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
			

			// Cars move across the lane along Z.
			if (stream.Direction == LaneDirection.Right)
			{
				spawnPosition.z -= spawnEdge;
			}
			else
			{
				spawnPosition.z += spawnEdge;
			}

			Transform transform = new Transform();
			transform.Scale = new Vector3(2.0f);
			//if(LaneDirection.Left == stream.Direction)
			//	transform.Rotation= new Vector3(0,180,0);
			transform.Location = spawnPosition;

			Entity carEntity = World.Instantiate(Cars[carIndex], transform);
			m_SpawnedCars.Add(carEntity);
			// The car gets deleted automatically when this lane is deleted.
			//AddChild(carEntity);

			// Replace As<Car>() only if your script getter has another name.
			Car car = carEntity.GetScript<Car>();

			if (car != null)
			{
				float travelDistance = LaneWidth + EdgeSpawnPadding * 4.0f;
				car.Initialize(stream.Direction, m_Speed, travelDistance);
			}
		}

		private float GetNextSpawnDelay()
		{
			float speedPadding = m_Speed * SpawnPaddingPerSpeed;		//Speed 6  → spacing receives +3 units
																		//Speed 12 → spacing receives +6 units
			
			float minimumDistance = MinSpawnPadding + speedPadding;
			float maximumDistance = MaxSpawnPadding + speedPadding;

			if (maximumDistance < minimumDistance)
				maximumDistance = minimumDistance;

			float spawnDistance = Proof.Random.Float(minimumDistance, maximumDistance);

			// Time required for a car to travel the selected spacing distance.
			return spawnDistance / m_Speed;
		}

		protected override void OnDestroy()
		{
			for(int i =0; i <m_SpawnedCars.Count; i++)
			{
				Entity e = m_SpawnedCars[i];
				if(Entity.IsValid(e))
					World.DeleteEntity(e);
			}
		}
	}
    public class SingleRoadLane : RoadLane
    {
        private static readonly LaneType[] s_CannotSpawnAfter =
        {
            LaneType.DoubleRoad,
        };

        public override LaneType Type => LaneType.SingleRoad;
        public override int RowCount => 1;
        public override bool CanSpawnBackToBack => false;
        public override LaneType[] CannotSpawnAfter => s_CannotSpawnAfter;


        public LaneDirection Direction = LaneDirection.Right;
		 
        protected virtual void OnCreate() 
		{
			Direction = Proof.Random.Bool() == false ? Direction = LaneDirection.Right  : Direction =  LaneDirection.Left;
			Log.Trace($"Lane Direction {Direction.ToString()}");
			base.OnCreate();
		}
        protected override LaneDirection GetDirectionForRow(int rowIndex)
        {
	        return Direction;
        }
    
      

    }

    public class DoubleRoadLane : RoadLane
    {
        private static readonly LaneType[] s_CannotSpawnAfter =
        {
            LaneType.SingleRoad,
        };

        public override LaneType Type => LaneType.DoubleRoad;
        public override int RowCount => 2;
        public override bool CanSpawnBackToBack => true;
        public override LaneType[] CannotSpawnAfter => s_CannotSpawnAfter;

        LaneDirection m_FirstLane = LaneDirection.Right;
		LaneDirection m_SecondLane = LaneDirection.Left;
        protected override LaneDirection GetDirectionForRow(int rowIndex)
        {
			if(rowIndex == 0)
				return m_FirstLane;
			if(rowIndex == 1)
				return m_SecondLane;

			return  LaneDirection.Right;
        }
        protected virtual void OnCreate() 
        {
	        m_FirstLane = Proof.Random.Bool() == false ? m_FirstLane = LaneDirection.Right  : m_FirstLane =  LaneDirection.Left;
	        m_SecondLane = Proof.Random.Bool() == true ? m_SecondLane = LaneDirection.Right  : m_SecondLane =  LaneDirection.Left;

	        Log.Trace($"FirstLane Direction {m_FirstLane.ToString()}");
	        Log.Trace($"SecondLane Direction {m_SecondLane.ToString()}");
	        base.OnCreate();
        }
    }

}