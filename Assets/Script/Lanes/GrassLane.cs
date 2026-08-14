using System;
using Proof;

namespace CrossyBro
{
    public class GrassLane : Lane
    {
        public override LaneType Type => LaneType.Grass;
        public override int RowCount => 1;
        public override bool CanSpawnBackToBack => true;
        public override LaneType[] CannotSpawnAfter => Array.Empty<LaneType>();

        public Prefab[] Trees;

        [ClampValueAttribute(0.0f,1.0f)]
        public float MinTreeDensity = 0.20f;
        [ClampValueAttribute(0.0f,1.0f)]
        public float MaxTreeDensity = 0.50f;


        public void OnCreate()
        {
            SpawnTrees();
        }

        private void SpawnTrees()
        {
           
            if (Trees == null || Trees.Length == 0)
                return;

            float tileSize = WorldData.GridSize;
            int slotCount = Mathf.FloorToInt(LaneWidth / tileSize);

            if (slotCount <= 1)
                return;

            float minDensity = Mathf.Clamp(MinTreeDensity, 0.0f, 1.0f);
            float maxDensity = Mathf.Clamp(MaxTreeDensity, minDensity, 1.0f);
            float density = Proof.Random.Float(minDensity, maxDensity);

            int treeCount = Mathf.RoundToInt(slotCount * density);

            // Always leave at least one tile empty.
            if (treeCount >= slotCount)
                treeCount = slotCount - 1;

            int[] slots = new int[slotCount];

            for (int i = 0; i < slotCount; i++)
                slots[i] = i;

            float firstSlotX = Transform.Location.z - LaneWidth * 0.5f + tileSize * 0.5f;

            for (int i = 0; i < treeCount; i++)
            {
                int randomSlotIndex = Proof.Random.Int(i, slotCount - 1);

                int temp = slots[i];
                slots[i] = slots[randomSlotIndex];
                slots[randomSlotIndex] = temp;

                int slot = slots[i];
                int treeIndex = Proof.Random.Int(0, Trees.Length - 1);

                Vector3 spawnPosition = Transform.Location;
                spawnPosition.z = firstSlotX + slot * tileSize;

                Entity tree = World.Instantiate(Trees[treeIndex], spawnPosition);

                // Make the tree a child of this lane so it gets deleted with it.
                AddChild(tree);
            }
        }
    }
}