using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Code.Frameworks.RayTracing
{
    public class Raycaster
    {
        private static float epsilon = Mathf.Epsilon;
        
        private Triangle[] triangles;

        public  BVHNode BvhRoot { get; private set; }

        public Raycaster() => triangles = Array.Empty<Triangle>();

        public Raycaster AddMesh(Mesh mesh, Transform transform)
        {
            int[]          indices    = mesh.triangles;
            int            trisCount  = indices.Length / 3;
            var            meshTri    = new Triangle[trisCount];
            RigidTransform rTransform = new RigidTransform(transform.rotation, transform.position);
            float3         scale      = transform.lossyScale;
            
            
            for (int i = 0; i < indices.Length; i += 3)
            {
                meshTri[i / 3] = new Triangle(
                    math.transform(rTransform, scale * mesh.vertices[indices[i]]),
                    math.transform(rTransform, scale * mesh.vertices[indices[i + 1]]),
                    math.transform(rTransform, scale * mesh.vertices[indices[i + 2]])
                );
            }

            triangles = triangles.Concat(meshTri).ToArray();
            return this;
        }

        public bool[] CastRays(Ray[] rays, float maxDistance = float.MaxValue)
        {
            // 1) Create a new texture with the same size as the rays texture
            var sideLength = (int)Mathf.Sqrt(rays.Length);
            var hitMap = new bool[sideLength * sideLength];
            
            // 2) Loop through all pixels in the rays texture
            Parallel.For(0, sideLength * sideLength, (id) => hitMap[id] = CastRay(rays[id], out _, maxDistance));

            return hitMap;
        }
        
        private Triangle[] TraverseBVH(Ray ray, BVHNode bvh, float maxDistance = float.MaxValue)
        {
            if (bvh.IsLeafNode) return bvh.Triangles;
            
            Triangle[] tris = Array.Empty<Triangle>();

            float t;
            
            if (RayAABBIntersection(ray, bvh.Left.AABB, out t))
                if(t <= maxDistance)
                    tris = TraverseBVH(ray, bvh.Left);
            
            if (RayAABBIntersection(ray, bvh.Right.AABB, out t))
                if(t <= maxDistance)
                    tris = tris.Concat(TraverseBVH(ray, bvh.Right)).ToArray();
            
            return tris;
        }

        public bool CastRay(Ray ray, out float3 hitPos, float maxDistance = float.MaxValue) {
            hitPos = float.MaxValue;
            if ((math.abs(math.length(ray.Direction)) <= epsilon) || (triangles.Length == 0)) return false;

            bool hit = false;
            
            var tris = TraverseBVH(ray, BvhRoot, maxDistance);
            
            // Use thread-local variables for each thread's local hit position and t value
            float3 hitPosTemp = float.MaxValue;
            float minT = float.MaxValue;

            Parallel.ForEach(tris, (triangle, loopState) =>
            {
                float t;

                if (RayTriangleIntersection(ray, triangle, out t, maxDistance))
                {
                    float3 currentHitPos = ray.Origin + ray.Direction * t;

                    // Check if this is the closest hit
                    if (t < minT)
                    {
                        minT = t;
                        hitPosTemp = currentHitPos;
                        hit = true;
                    }
                }
            });

            hitPos = hitPosTemp;

            return hit;
        }
        
        /// <summary>
        /// Calculate the intersection between a ray and a triangle
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="triangle"></param>
        /// <param name="t">Hit distance</param>
        /// <returns> true if ray intersects the triangle</returns>
        bool RayTriangleIntersection(Ray ray, Triangle triangle, out float t, float maxDistance = float.MaxValue)
        {
            t = -1f;
            
            float3 edge1 = triangle.B - triangle.A;
            float3 edge2 = triangle.C - triangle.A;
            float3 crossE2 = math.cross(ray.Direction, edge2);
            float det = math.dot(edge1, crossE2);
            
            if(det > -epsilon && det < epsilon) 
                return false;
            
            float  invDet = 1.0f / det;
            float3 s      = ray.Origin - triangle.A;
            float  u      = invDet * math.dot(s, crossE2);

            if (u < 0 || u > 1)
                return false;

            float3 crossE1 = math.cross(s, edge1);
            float v = invDet * math.dot(ray.Direction, crossE1);

            if (v < 0 || u + v > 1)
                return false;

            t = invDet * math.dot(edge2, crossE1);

            return t < epsilon && t <= maxDistance;
        }
        
        /// <summary>
        /// Calculate the intersection between a ray and an AABB
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="aabb"></param>
        /// <param name="t">Hit distance</param>
        /// <returns> true if ray intersects the AABB</returns>
        bool RayAABBIntersection(Ray ray, AABB aabb, out float t)
        {
            t = -1f;
            if (aabb.WithinBounds(ray.Origin)) return true;
                
            float tmin = float.MinValue;
            float tmax = float.MaxValue;
            
            float3 invD = math.rcp(ray.Direction);
            float3 t0s  = (aabb.Min - ray.Origin) * invD;
            float3 t1s  = (aabb.Max - ray.Origin) * invD;
    
            float3 tsmaller = math.min(t0s, t1s);
            float3 tbigger  = math.max(t0s, t1s);
    
            tmin = math.max(tmin, math.max(tsmaller[0], math.max(tsmaller[1], tsmaller[2])));
            tmax = math.min(tmax, math.min(tbigger[0],math. min(tbigger[1], tbigger[2])));

            if (tmin < tmax)
            {
                t = tmin;
                return true;
            }
            
            return false;
        }
        
        bool RayOBBIntersection(Ray ray, OBB obb)
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// Computes the bounding box of a set of triangles
        /// </summary>
        /// <param name="triangles"></param>
        /// <returns></returns>
        public static AABB ComputeBounds(Triangle[] triangles)
        {
            float3 min = new float3(float.MaxValue, float.MaxValue, float.MaxValue);
            float3 max = new float3(float.MinValue, float.MinValue, float.MinValue);

            foreach (var triangle in triangles)
            {
                min = math.min(min, math.min(triangle.A, math.min(triangle.B, triangle.C)));
                max = math.max(max, math.max(triangle.A, math.max(triangle.B, triangle.C)));
            }

            return new AABB(min, max);
        }
        
        public void BuildBVH(int maxDepth = 7) {
            
            BVHNode bvh = new() {
                Triangles = triangles,
                AABB      = ComputeBounds(triangles)
            };

            SplitBVH(bvh, 0, maxDepth);
            
            BvhRoot = bvh;
            
            CleanupNode(BvhRoot);
        }

        public void CleanupNode(BVHNode node) {
            if (node.IsLeafNode) return;
            node.Triangles = Array.Empty<Triangle>();
            CleanupNode(node.Left);
            CleanupNode(node.Right);
        }
        
        
        static void SplitBVH(BVHNode parent, int depth, int maxDepth) {
            if (depth >= maxDepth) return;
            if (parent.Triangles.Length == 0) return;
            
            Axis    splitAxis = parent.AABB.LongestAxis();
            BVHNode left      = new BVHNode();
            BVHNode right     = new BVHNode();
            
            foreach (var triangle in parent.Triangles)
                if (parent.AABB.IsLeftOfPlane(splitAxis, triangle.A) || 
                    parent.AABB.IsLeftOfPlane(splitAxis, triangle.B) ||
                    parent.AABB.IsLeftOfPlane(splitAxis, triangle.C))
                    left.Triangles = left.Triangles.Concat(new[] {triangle}).ToArray();
                else
                    right.Triangles = right.Triangles.Concat(new[] {triangle}).ToArray();
            
            left.AABB   = ComputeBounds(left.Triangles);
            parent.Left = left;
            SplitBVH(left, depth+1, maxDepth);
            
            right.AABB   = ComputeBounds(right.Triangles);
            parent.Right = right;
            SplitBVH(right, depth+1, maxDepth);
        }
    }
}