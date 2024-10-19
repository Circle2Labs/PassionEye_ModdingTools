using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Unity.Mathematics;

namespace Code.Frameworks.RayTracing
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct Ray
    {
        [FieldOffset(0)]
        public float3 Origin;
        [FieldOffset(sizeof(float) * 3)]
        public float3 Direction;
            
        public Ray(float3 origin, float3 direction)
        {
            Origin    = origin;
            Direction = direction;
        }
        
        public const int Size = sizeof(float) * 3 * 2;
    }
    
    public static unsafe class RayExt 
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ToBytes(this float3 vector)
        {
            var bytes = new byte[sizeof(float) * 3];
            var offset = 0;
            var xBytes = BitConverter.GetBytes(vector.x);
            var yBytes = BitConverter.GetBytes(vector.y);
            var zBytes = BitConverter.GetBytes(vector.z);
            foreach (var t in xBytes) bytes[offset++] = t;
            foreach (var t in yBytes) bytes[offset++] = t;
            foreach (var t in zBytes) bytes[offset++] = t;
            return bytes;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ToBytes(this Ray ray)
        {
            var bytes = new byte[Ray.Size];
            var offset = 0;
            var originBytes = ray.Origin.ToBytes();
            var directionBytes = ray.Direction.ToBytes();
            foreach (var t in originBytes) bytes[offset++] = t;
            foreach (var t in directionBytes) bytes[offset++] = t;
            return bytes;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 ToFloat3(this byte[] bytes)
        {
            var x = BitConverter.ToSingle(bytes, 0);
            var y = BitConverter.ToSingle(bytes, sizeof(float));
            var z = BitConverter.ToSingle(bytes, sizeof(float) * 2);
            return new float3(x, y, z);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Ray ToRay(this byte[] bytes)
        {
            var originBytes = new byte[sizeof(float) * 3];
            var directionBytes = new byte[sizeof(float) * 3];
            for (var i = 0; i < sizeof(float) * 3; i++)
            {
                originBytes[i] = bytes[i];
                directionBytes[i] = bytes[i + sizeof(float) * 3];
            }
            var origin = originBytes.ToFloat3();
            var direction = directionBytes.ToFloat3();
            return new Ray(origin, direction);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ToBytes(this Ray[] rays)
        {
            var bytes = new byte[Ray.Size * rays.Length];
            Parallel.For(0, rays.Length, id =>
            {
                int offset = id * Ray.Size;
                var rayBytes = rays[id].ToBytes();
                for (var i = 0; i < Ray.Size; i++) bytes[offset++] = rayBytes[i];
            });
            
            return bytes;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Ray[] ToRays(this byte[] bytes)
        {
            var rays = new Ray[bytes.Length / Ray.Size];
            Parallel.For(0, rays.Length, id =>
            {
                int offset = id * Ray.Size;
                var rayBytes = new byte[Ray.Size];
                for (var i = 0; i < Ray.Size; i++) rayBytes[i] = bytes[offset++];
                rays[id] = rayBytes.ToRay();
            });
            
            return rays;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ToBytesPtr(this Ray[] rays)
        {
            byte[] bytes = new byte[Ray.Size * rays.Length];
            
            fixed (byte* pBytes = bytes)
            {
                fixed (Ray* pRays = rays)
                {
                    Buffer.MemoryCopy(pRays, pBytes, bytes.Length, bytes.Length);
                }
            }
            
            return bytes;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Ray[] ToRaysPtr(this byte[] bytes)
        {
            Ray[] rays = new Ray[bytes.Length / Ray.Size];
            
            fixed (byte* pBytes = bytes)
            {
                fixed (Ray* pRays = rays)
                {
                    Buffer.MemoryCopy(pBytes, pRays, bytes.Length, bytes.Length);
                }
            }
            
            return rays;
        }
    }
}
