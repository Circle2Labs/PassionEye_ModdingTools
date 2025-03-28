using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Mathematics;

using Math = Code.Tools.Math;

namespace Code.Frameworks.RayTracing
{
    public static class RayUtils
    {
        public static Vector2 NormalizeUdim(this Vector2 vec)
        {
            //case < 0
            var x = vec.x < 0 ? vec.x + Mathf.Ceil(Mathf.Abs(vec.x)) : vec.x;
            var y = vec.y < 0 ? vec.y + Mathf.Ceil(Mathf.Abs(vec.y)) : vec.y;
            
            //case > 1
            x = x > 1 ? x - (int)x : x;
            y = y > 1 ? y - (int)y : y;
            
            return new Vector2(x, y);
        }
        
        // TODO: Create structs for methods parameters
        
        /// <summary>
        /// Creates an acceleration structure for a mesh, which is used for UV to 3D point conversion
        /// </summary>
        /// <param name="submeshOffsetStart"></param>
        /// <param name="submeshOffsetEnd"></param>
        /// <param name="bodyTriangles"></param>
        /// <param name="bodyUVs"></param>
        /// <param name="texSize"></param>
        /// <returns></returns>
        public static GeoHidingAccelStruct BakeAccelerationStructure(int submeshOffsetStart, int submeshOffsetEnd, 
                                                     int[] bodyTriangles, Vector2[] bodyUVs, int texSize)
        {
            var accelStruct = new GeoHidingAccelStruct(texSize);
            
            for (int i = submeshOffsetStart; i < submeshOffsetEnd; i += 3)
            {
                //gather UV coords
                var trisUvs = new Tuple<Vector2, Vector2, Vector2>(
                    bodyUVs[bodyTriangles[i]].NormalizeUdim(),
                    bodyUVs[bodyTriangles[i + 1]].NormalizeUdim(),
                    bodyUVs[bodyTriangles[i + 2]].NormalizeUdim()
                );

                // get the bounding box of the triangle
                var bb = Math.GetBoundingBox(trisUvs);
                // lets transform the bounding box to pixel space
                var bbPixel = new Vector4(bb.x * texSize, bb.y * texSize, bb.z * texSize, bb.w * texSize);
                //for each pixel inside the bounding box, we now need to run the triangle intersection test and set the value in the accelleration structure
                for (int x = (int)bbPixel.x; x < bbPixel.z; x++)
                {
                    for (int y = (int)bbPixel.y; y < bbPixel.w; y++)
                    {
                        var uv = new Vector2((float)x / texSize, (float)y / texSize);
                        Vector3? barycentric = Math.BarycentricCoords(uv, trisUvs);
                        if (barycentric == null) continue;
                        accelStruct.SetPosition(x, y, i);
                    }
                }
            }
            
            return accelStruct;
        }

        /// <summary>
        /// Generates a list of 3D points, triangle indices, barycentric coordinates and pixel coordinates for each pixel in the texture
        /// </summary>
        /// <param name="accelStruct"></param>
        /// <param name="texSize"></param>
        /// <param name="bodyTriangles"></param>
        /// <param name="bodyUVs"></param>
        /// <param name="bodyVertices"></param>
        /// <returns></returns>
        public static Tuple<Vector3, int, Vector3, Vector2Int>[] ScanUVs(GeoHidingAccelStruct accelStruct, int texSize, 
                                                    int[] bodyTriangles, Vector2[] bodyUVs, Vector3[] bodyVertices)
        {
            // <point3D, triangleIndex, barycentricCoords, pixelCoords>
            var points3D = new Tuple<Vector3, int, Vector3, Vector2Int>[texSize * texSize];
            
            // 2) for each pixel in the texture, get UV coordinates (maybe more than one)
            int parallelLenght = texSize * texSize;
            Parallel.For((long)0, parallelLenght, threadId =>
            {
                int x = (int)threadId % texSize;
                int y = (int)threadId / texSize;

                var uv = new Vector2((float)x / texSize, (float)y / texSize);
                var i = accelStruct.GetPosition(x, y);

                var trisUvs = new Tuple<Vector2, Vector2, Vector2>(bodyUVs[bodyTriangles[i]].NormalizeUdim(),
                    bodyUVs[bodyTriangles[i + 1]].NormalizeUdim(), bodyUVs[bodyTriangles[i + 2]].NormalizeUdim());

                Vector3? barycentric = Math.BarycentricCoords(uv, trisUvs);

                if (barycentric == null) return;

                lock (points3D)
                {
                    points3D[threadId] = new(barycentric.Value.x * bodyVertices[bodyTriangles[i]] +
                                             barycentric.Value.y * bodyVertices[bodyTriangles[i + 1]] +
                                             barycentric.Value.z * bodyVertices[bodyTriangles[i + 2]],
                        i, barycentric.Value, new Vector2Int(x, y));
                }
            });
            
            return points3D;
        }

        public static Ray[] CalculateRays(Tuple<Vector3, int, Vector3, Vector2Int>[] points3D, Vector3[] bodyNormals, 
                                            int[] bodyTriangles)
        {
            var rays = new Ray[points3D.Length];
            
            Parallel.For(0, points3D.Length, (threadId, loopState) =>
            {
                var point = points3D[threadId];
                if (point == null)
                    return;

                var triangleNormals = new Tuple<Vector3, Vector3, Vector3>(
                    bodyNormals[bodyTriangles[point.Item2]],
                    bodyNormals[bodyTriangles[point.Item2 + 1]],
                    bodyNormals[bodyTriangles[point.Item2 + 2]]
                );

                var normal = point.Item3.x * triangleNormals.Item1 +
                             point.Item3.y * triangleNormals.Item2 +
                             point.Item3.z * triangleNormals.Item3;

                normal = normal.normalized;

                var point3DWorld = point.Item1;
                //Math.TransformPoint(point.Item1, bodyPosition, bodyRotation, bodyScale);

                rays[threadId] = new Ray(point3DWorld, normal);
            });
            
            return rays;
        }
    }
}