using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using System.Runtime.InteropServices;
namespace Code.Frameworks.RayTracing
{
    [Serializable]
    public class BVHNode
    {
        public bool HasParent => Parent != null;
        public bool IsLeafNode => Left == null && Right == null;
        public AABB AABB = AABB.Zero;
        public Triangle[] Triangles = Array.Empty<Triangle>();
        public BVHNode Parent = null;
        public BVHNode Left = null;
        public BVHNode Right = null;

        public int GetNodeCount()
        {
            int count = 1;
            if (Left != null) count += Left.GetNodeCount();
            if (Right != null) count += Right.GetNodeCount();
            return count;
        }

        public Tuple<BVHNodeGPU[], Triangle[]> ToGPU()
        {
            var queue = new Queue<BVHNode>();
            var nodes = new List<BVHNode>();
            var tris = new List<Triangle>();
            queue.Enqueue(this);
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                nodes.Add(node);
                if (node.IsLeafNode)
                {
                    tris.AddRange(node.Triangles);
                }
                else
                {
                    queue.Enqueue(node.Left);
                    queue.Enqueue(node.Right);
                }
            }

            var gpuNodes = new BVHNodeGPU[nodes.Count];
            int trisIndex = 0;
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                gpuNodes[i] = new BVHNodeGPU
                {
                    AABB = node.AABB,
                    Parent = node.Parent == null ? -1 : nodes.IndexOf(node.Parent),
                    Left = node.Left == null ? -1 : nodes.IndexOf(node.Left),
                    Right = node.Right == null ? -1 : nodes.IndexOf(node.Right),
                    TriangleIndexStart = node.IsLeafNode ? trisIndex : -1,
                    TriangleIndexCount = node.IsLeafNode ? node.Triangles.Length : -1
                };
                if (node.IsLeafNode) trisIndex += node.Triangles.Length;
            }

            return new Tuple<BVHNodeGPU[], Triangle[]>(gpuNodes, tris.ToArray());
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct BVHNodeGPU
    {
        [FieldOffset(0)]
        public AABB AABB;
        [FieldOffset(24)]
        public int Parent;
        [FieldOffset(28)]
        public int Left;
        [FieldOffset(32)]
        public int Right;
        [FieldOffset(36)]
        public int TriangleIndexStart;
        [FieldOffset(40)]
        public int TriangleIndexCount;

        public const int Size = sizeof(float) * 6 + sizeof(int) * 5;
    }

    [Serializable]
    public struct BVHDataGPU
    {
        public BVHNodeGPU[] Nodes;
        public Triangle[] Triangles;

        public int ByteSize => Nodes == null || Triangles == null ? 0 : Nodes.Length * BVHNodeGPU.Size + Triangles.Length * Triangle.Size;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe BVHDataGPU FromBytes(byte[] bytes)
        {
            fixed (byte* ptr = bytes)
            {
                int* intPtr = (int*)ptr;
                int nodeCount = *intPtr++;
                int triCount = *intPtr++;

                float* floatPtr = (float*)intPtr;
                var nodes = new BVHNodeGPU[nodeCount];
                for (int i = 0; i < nodeCount; i++)
                {
                    var aabb = new AABB
                    {
                        Min = new float3(*floatPtr++, *floatPtr++, *floatPtr++),
                        Max = new float3(*floatPtr++, *floatPtr++, *floatPtr++)
                    };
                    
                    intPtr = (int*)floatPtr;
                    
                    nodes[i] = new BVHNodeGPU
                    {
                        AABB = aabb,
                        Parent = *intPtr++,
                        Left = *intPtr++,
                        Right = *intPtr++,
                        TriangleIndexStart = *intPtr++,
                        TriangleIndexCount = *intPtr++
                    };
                    
                    floatPtr = (float*)intPtr;
                }

                var tris = new Triangle[triCount];
                for (int i = 0; i < triCount; i++)
                {
                    tris[i] = new Triangle
                    {
                        A = new float3(*floatPtr++, *floatPtr++, *floatPtr++),
                        B = new float3(*floatPtr++, *floatPtr++, *floatPtr++),
                        C = new float3(*floatPtr++, *floatPtr++, *floatPtr++)
                    };
                }

                return new BVHDataGPU
                {
                    Nodes = nodes,
                    Triangles = tris
                };
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe BVHDataGPU[] FromBytesMany(byte[] bytes)
        {
            int offset = 0;
            var data = new List<BVHDataGPU>();
            
            while (offset < bytes.Length)
            {
                fixed (byte* ptr = bytes)
                {
                    int* intPtr = (int*)(ptr + offset);
                    int nodeCount = *intPtr++;
                    int triCount = *intPtr++;
                    
                    int totalSize = sizeof(int) * 2 + nodeCount * BVHNodeGPU.Size + triCount * Triangle.Size;
                    var bvhData = FromBytes(bytes[offset..(offset + totalSize)]);
                    
                    data.Add(bvhData);
                    offset += totalSize;
                }
            }
            
            return data.ToArray();
        }
    }
}