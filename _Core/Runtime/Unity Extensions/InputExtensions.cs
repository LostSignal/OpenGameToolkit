namespace OGT
{
    using System.Collections.Generic;
    using UnityEngine;

    public static class InputExentions
    {
        private static readonly Dictionary<int, WorldSpace> WorldSpaceCache = new();
        private static long CurrentFrame = -1L;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            WorldSpaceCache.Clear();
            CurrentFrame = -1L;
        }

        public static Vector3 GetWorldSpace(this OGT.Input input, Camera camera, float distance)
        {
            if (CurrentFrame != Time.frameCount)
            {
                WorldSpaceCache.Clear();
                CurrentFrame = Time.frameCount;
            }

            if (WorldSpaceCache.TryGetValue(input.Id, out WorldSpace worldSpace) == false || worldSpace.Distance != distance)
            {
                worldSpace = new WorldSpace
                {
                    Distance = distance,
                    Position = camera.ScreenToWorldPoint(input.CurrentPosition.AddZ(distance)),
                };

                WorldSpaceCache.Add(input.Id, worldSpace);
            }

            return worldSpace.Position;
        }

        private struct WorldSpace
        {
            public float Distance;
            public Vector3 Position;
        }
    }
}
