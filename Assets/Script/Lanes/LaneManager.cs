using System;
using System.Collections.Generic;
using Proof;

namespace CrossyBro
{
	public class LaneManager : Entity
	{
		private class ActiveLane
		{
			public Entity Entity;
			public Lane Lane;
			public int StartRow;
			public int RowCount;
		}

		public Entity Player;

		public Prefab PlayerBaseLane;
		public Prefab[] GrassLanes;
		public Prefab[] SingleRoadLanes;
		public Prefab[] DoubleRoadLanes;

		public int RowsAhead = 20;
		public int RowsBehind = 5;

		public float OriginX = 0.0f;

		private readonly List<ActiveLane> m_ActiveLanes = new List<ActiveLane>();

		private int m_CurrentPlayerRow;
		private int m_NextRow;
		private float m_LastSpawnPos = 0.0f;
		private bool m_FirstSpawn = true;

		void OnCreate()
		{
			if (Player == null) return;

			m_CurrentPlayerRow = GetPlayerRow();
			m_NextRow = m_CurrentPlayerRow - RowsBehind;

			SpawnInitialLanes();
			GenerateUntil(m_CurrentPlayerRow + RowsAhead);
		}

		void OnUpdate(float deltaTime)
		{
			if (Player == null) return;

			int playerRow = GetPlayerRow();
			if (playerRow == m_CurrentPlayerRow) return;

			m_CurrentPlayerRow = playerRow;

			GenerateUntil(m_CurrentPlayerRow + RowsAhead);
			DeleteLanesBehind(m_CurrentPlayerRow - RowsBehind);
		}

		private void SpawnInitialLanes()
		{
			// Everything behind the player starts as normal grass.
			while (m_NextRow < m_CurrentPlayerRow)
			{
				Prefab grassPrefab = GetRandomPrefab(LaneType.Grass);
				if (grassPrefab == null || !SpawnLane(LaneType.Grass, grassPrefab)) return;
			}

			// The tile directly underneath the player is the special grass tile with no trees.
			if (PlayerBaseLane != null)
				SpawnLane(LaneType.Grass, PlayerBaseLane);
		}

		private int GetPlayerRow()
		{
			return Mathf.RoundToInt((Player.Transform.Location.x - OriginX) / WorldData.GridSize);
		}

		private void GenerateUntil(int targetRow)
		{
			while (m_NextRow <= targetRow)
			{
				if (!SpawnNextLane()) break;
			}
		}

		private bool SpawnNextLane()
		{
			LaneType laneType = ChooseNextLaneType();
			Prefab prefab = GetRandomPrefab(laneType);

			if (prefab == null) return false;

			return SpawnLane(laneType, prefab);
		}

		private bool SpawnLane(LaneType laneType, Prefab prefab)
		{
			Vector3 spawnPosition = Transform.Location;

			if (m_FirstSpawn)
			{
				spawnPosition.x = OriginX + m_NextRow * WorldData.GridSize;
				m_FirstSpawn = false;
			}
			else
			{
				Lane previousLane = m_ActiveLanes[m_ActiveLanes.Count - 1].Lane;

				float spacing = 8.0f;

				if (previousLane.Type == LaneType.DoubleRoad && laneType == LaneType.DoubleRoad)
					spacing = 16.0f;
				else if (previousLane.Type == LaneType.DoubleRoad || laneType == LaneType.DoubleRoad)
					spacing = 12.0f;

				spawnPosition.x = m_LastSpawnPos + spacing;
			}

			m_LastSpawnPos = spawnPosition.x;

			Entity laneEntity = World.Instantiate(prefab, spawnPosition);
			Lane lane = laneEntity.GetScriptInstance<Lane>();

			if (lane == null)
			{
				World.DeleteEntity(laneEntity);
				return false;
			}

			int startRow = m_NextRow;

			lane.Initialize(startRow);

			ActiveLane activeLane = new ActiveLane();
			activeLane.Entity = laneEntity;
			activeLane.Lane = lane;
			activeLane.StartRow = startRow;
			activeLane.RowCount = lane.RowCount;

			m_ActiveLanes.Add(activeLane);

			m_NextRow += lane.RowCount;

			return true;
		}

		private LaneType ChooseNextLaneType()
		{
			LaneType[] availableTypes = new LaneType[3];
			int availableCount = 0;

			if (HasPrefabs(GrassLanes) && CanSpawnType(LaneType.Grass))
				availableTypes[availableCount++] = LaneType.Grass;

			if (HasPrefabs(SingleRoadLanes) && CanSpawnType(LaneType.SingleRoad))
				availableTypes[availableCount++] = LaneType.SingleRoad;

			if (HasPrefabs(DoubleRoadLanes) && CanSpawnType(LaneType.DoubleRoad))
				availableTypes[availableCount++] = LaneType.DoubleRoad;

			if (availableCount == 0) return LaneType.Grass;

			return availableTypes[Proof.Random.Int(0, availableCount - 1)];
		}

		private bool CanSpawnType(LaneType nextType)
		{
			if (m_ActiveLanes.Count == 0) return true;

			Lane previousLane = m_ActiveLanes[m_ActiveLanes.Count - 1].Lane;
			if (previousLane == null) return true;

			if (previousLane.Type == nextType && !previousLane.CanSpawnBackToBack) return false;

			LaneType[] cannotSpawnAfter = previousLane.CannotSpawnAfter;

			if (cannotSpawnAfter != null)
			{
				for (int i = 0; i < cannotSpawnAfter.Length; i++)
					if (cannotSpawnAfter[i] == nextType) return false;
			}

			return true;
		}

		private Prefab GetRandomPrefab(LaneType laneType)
		{
			Prefab[] prefabs = null;

			switch (laneType)
			{
				case LaneType.Grass: prefabs = GrassLanes; break;
				case LaneType.SingleRoad: prefabs = SingleRoadLanes; break;
				case LaneType.DoubleRoad: prefabs = DoubleRoadLanes; break;
			}

			if (!HasPrefabs(prefabs)) return null;

			return prefabs[Proof.Random.Int(0, prefabs.Length - 1)];
		}

		private bool HasPrefabs(Prefab[] prefabs)
		{
			return prefabs != null && prefabs.Length > 0;
		}

		private void DeleteLanesBehind(int minimumRow)
		{
			for (int i = m_ActiveLanes.Count - 1; i >= 0; i--)
			{
				ActiveLane activeLane = m_ActiveLanes[i];
				int laneEndRow = activeLane.StartRow + activeLane.RowCount - 1;

				if (laneEndRow >= minimumRow) continue;

				if (Entity.IsValid(activeLane.Entity)) World.DeleteEntity(activeLane.Entity);

				m_ActiveLanes.RemoveAt(i);
			}
		}

		protected override void OnDestroy()
		{
			for (int i = 0; i < m_ActiveLanes.Count; i++)
			{
				Entity laneEntity = m_ActiveLanes[i].Entity;
				if (Entity.IsValid(laneEntity)) World.DeleteEntity(laneEntity);
			}

			m_ActiveLanes.Clear();
		}
	}
}