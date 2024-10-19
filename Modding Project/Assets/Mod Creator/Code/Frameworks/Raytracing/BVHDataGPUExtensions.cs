using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Code.Frameworks.RayTracing
{
    public static class BVHDataGPUExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe byte[] ToBytes(this BVHDataGPU data)
        {
            int nodeSize = sizeof(float) * 6 + sizeof(int) * 5;
            int triangleSize = sizeof(float) * 9;
            
            if (data.Nodes == null || data.Triangles == null)
            {
                var emptyBvhBytes = new byte[2 * sizeof(int)];
                Array.Fill(emptyBvhBytes, (byte)0);
                return emptyBvhBytes;
            }

            int totalSize = sizeof(int) * 2 + data.Nodes.Length * nodeSize + data.Triangles.Length * triangleSize;

            byte[] bytes = new byte[totalSize];

            fixed (byte* ptr = bytes)
            {
                int* intPtr = (int*)ptr;
                *intPtr++ = data.Nodes.Length;
                *intPtr++ = data.Triangles.Length;

                float* floatPtr = (float*)(intPtr);

                for (int i = 0; i < data.Nodes.Length; i++)
                {
                    var node = data.Nodes[i];
                    *floatPtr++ = node.AABB.Min.x;
                    *floatPtr++ = node.AABB.Min.y;
                    *floatPtr++ = node.AABB.Min.z;
                    *floatPtr++ = node.AABB.Max.x;
                    *floatPtr++ = node.AABB.Max.y;
                    *floatPtr++ = node.AABB.Max.z;

                    intPtr = (int*)floatPtr;
                    *intPtr++ = node.Parent;
                    *intPtr++ = node.Left;
                    *intPtr++ = node.Right;
                    *intPtr++ = node.TriangleIndexStart;
                    *intPtr++ = node.TriangleIndexCount;

                    floatPtr = (float*)intPtr;
                }

                for (int i = 0; i < data.Triangles.Length; i++)
                {
                    var tri = data.Triangles[i];
                    *floatPtr++ = tri.A.x;
                    *floatPtr++ = tri.A.y;
                    *floatPtr++ = tri.A.z;
                    *floatPtr++ = tri.B.x;
                    *floatPtr++ = tri.B.y;
                    *floatPtr++ = tri.B.z;
                    *floatPtr++ = tri.C.x;
                    *floatPtr++ = tri.C.y;
                    *floatPtr++ = tri.C.z;
                }
            }

            return bytes;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ToBytes(this BVHDataGPU[] data)
        {
            var sizes = data.Select(x => x.ByteSize).ToArray();
            var totalSize = sizes.Sum() + sizeof(int) * sizes.Length * 2;

            byte[] bytes = new byte[totalSize];

            var offset = 0;

            for (int i=0; i<data.Length; i++)
            {
                data[i].ToBytes().CopyTo(bytes, offset);
                offset += sizes[i];
            }
            
            return bytes;
        }
    }
}