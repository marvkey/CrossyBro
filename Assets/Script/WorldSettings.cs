using System;
using Proof;
namespace CrossyBro
{
    public static class WorldData
    {
        public static float GridSize = 8.0f;
        public static Vector3 GridOrigin = Vector3.Zero;

        public static float Snap(float value, float origin = 0.0f)
        {
            return origin + Mathf.Round((value - origin) / GridSize) * GridSize;
        }

        public static Vector3 SnapPosition(Vector3 position)
        {
            position.x = Snap(position.x, GridOrigin.x);
            position.z = Snap(position.z, GridOrigin.z);
            return position;
        }

        public static Vector3 GetMoveOffset(int x, int z)
        {
            return new Vector3(x * GridSize, 0.0f, z * GridSize);
        }
    }
}