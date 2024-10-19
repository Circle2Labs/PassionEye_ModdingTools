using System;
using Unity.Mathematics;
using System.Runtime.InteropServices;

namespace Code.Frameworks.RayTracing
{
    public enum Axis
    {
        X,
        Y,
        Z
    }
    
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct AABB
    {
        [FieldOffset(0)]
        public float3 Min;
        [FieldOffset(12)]
        public float3 Max;

        public AABB(float3 min, float3 max)
        {
            Min = min;
            Max = max;
        }
        
        public static AABB Zero => new AABB(float3.zero, float3.zero);
        
        public static AABB operator +(AABB aabb, float3 offset)
        {
            return new AABB(aabb.Min + offset, aabb.Max + offset);
        }
    }
    
    public static class AABBExtensions
    {
        public static Axis LongestAxis(this AABB aabb)
        {
            float3 size = aabb.Max - aabb.Min;
            if (size.x > size.y && size.x > size.z)
                return Axis.X;
            if (size.y > size.x && size.y > size.z)
                return Axis.Y;
            return Axis.Z;
        }
        
        public static bool WithinBounds(this AABB aabb, float3 point)
        {
            return point.x >= aabb.Min.x && point.x <= aabb.Max.x &&
                   point.y >= aabb.Min.y && point.y <= aabb.Max.y &&
                   point.z >= aabb.Min.z && point.z <= aabb.Max.z;
        }
        
        public static bool WithinBounds(this AABB aabb, Triangle triangle)
        {
            return aabb.WithinBounds(triangle.A) ||
                aabb.WithinBounds(triangle.B) ||
                aabb.WithinBounds(triangle.C);
        }
        
        public static float3 Center(this AABB aabb)
        {
            return (aabb.Min + aabb.Max) / 2;
        }
        
        public static bool IsLeftOfPlane(this AABB aabb, Axis axis, float3 point)
        {
            return axis switch {
                Axis.X => point.x < aabb.Center().x,
                Axis.Y => point.y < aabb.Center().y,
                Axis.Z => point.z < aabb.Center().z,
                _ => throw new System.NotImplementedException()
            };
        }
        
        public static float3 Size(this AABB aabb) {
            return aabb.Max - aabb.Min;
        }
    }
}