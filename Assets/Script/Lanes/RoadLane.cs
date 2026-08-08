using System;
using System.Collections.Generic;
using Proof;

namespace CrossyBro
{
	public class SingleRoadLane : MovingObjectLane
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
			Direction = Proof.Random.Bool() == false ? LaneDirection.Right : LaneDirection.Left;

			Log.Trace($"Lane Direction {Direction.ToString()}");

			base.OnCreate();
		}

		protected override LaneDirection GetDirectionForRow(int rowIndex)
		{
			return Direction;
		}
	}

	public class DoubleRoadLane : MovingObjectLane
	{
		private static readonly LaneType[] s_CannotSpawnAfter =
		{
			LaneType.SingleRoad,
		};

		public override LaneType Type => LaneType.DoubleRoad;
		public override int RowCount => 2;
		public override bool CanSpawnBackToBack => true;
		public override LaneType[] CannotSpawnAfter => s_CannotSpawnAfter;

		private LaneDirection m_FirstLane = LaneDirection.Right;
		private LaneDirection m_SecondLane = LaneDirection.Left;

		protected virtual void OnCreate()
		{
			m_FirstLane = Proof.Random.Bool() == false ? LaneDirection.Right : LaneDirection.Left;
			m_SecondLane = m_FirstLane == LaneDirection.Right ? LaneDirection.Left : LaneDirection.Right;

			Log.Trace($"FirstLane Direction {m_FirstLane.ToString()}");
			Log.Trace($"SecondLane Direction {m_SecondLane.ToString()}");

			base.OnCreate();
		}

		protected override LaneDirection GetDirectionForRow(int rowIndex)
		{
			if (rowIndex == 0)
				return m_FirstLane;

			if (rowIndex == 1)
				return m_SecondLane;

			return LaneDirection.Right;
		}
	}
}