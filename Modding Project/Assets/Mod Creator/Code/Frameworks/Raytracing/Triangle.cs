using System;
using Unity.Mathematics;

namespace Code.Frameworks.RayTracing
{
    [Serializable]
    public struct Triangle
    {
        public float3 A;
        public float3 B;
        public float3 C;

        public Triangle(float3 a, float3 b, float3 c)
        {
            A = a;
            B = b;
            C = c;
        }
        
        public const int Size = sizeof(float) * 3 * 3;
    }
}