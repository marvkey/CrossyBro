using System;
using Proof;

namespace CrossyBro
{
    public class CameraMovement : Entity
    {
        public Entity Target;

        public float FollowSpeed = 12.0f;
        public float FollowY = 2.0f;

        void OnUpdate(float ts)
        {
            if (Target == null)
                return;

            Vector3 targetPosition = Target.Transform.WorldTransform.Location;
            targetPosition.y += FollowY;

            Vector3 currentPosition = Transform.Location;

            float t = Mathf.Clamp(FollowSpeed * ts, 0.0f, 1.0f);

            Transform.Location = currentPosition + (targetPosition - currentPosition) * t;
        }
    }
}