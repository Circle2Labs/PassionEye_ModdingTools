using Unity.Mathematics;

namespace Code.Frameworks.RayTracing
{
    public struct OBB
    {
        public float3 Center;
        public float3 Extents;
        public float4 Rotation;

        public OBB(float3 center, float3 extents, float4 rotation)
        {
            Center = center;
            Extents = extents;
            Rotation = rotation;
        }
    }
}