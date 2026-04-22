#ifndef RAYCAST_HLSL
#define RAYCAST_HLSL

#include "RayData.hlsl"

RayData RayCast(Ray ray, float distance, float3 vertices[3])
{
    ray.direction = normalize(ray.direction);

    RayData result = {
        ray,
        distance,
        float3 (0,0,0),
        float3(0, 0, 0),
        0.0f,
        0
    };

    Triangle tris = {
        vertices[0],
        vertices[1],
        vertices[2]
    };

    float3 edge1 = tris.b - tris.a;
    float3 edge2 = tris.c - tris.a;

    float3 p = cross(ray.direction, edge2);
    float det = dot(edge1, p);

    if (det > -0.000001f && det < 0.000001f)
        return result;

    float invDet = 1.0f / det;

    float3 t = ray.origin - tris.a;
    float u = dot(t, p) * invDet;

    if (u < 0.0f || u > 1.0f)
        return result;

    float3 q = cross(t, edge1);
    float v = dot(ray.direction, q) * invDet;

    if (v < 0.0f || u + v > 1.0f)
        return result;

    float distanceToHit = dot(edge2, q) * invDet;

    if (distanceToHit > 0.0f && distanceToHit <= distance)
    {
        result.hitPoint = ray.origin + ray.direction * distanceToHit;
        result.hitNormal = normalize(cross(edge1, edge2));
        result.hitDistance = distanceToHit;
    }

    return result;
}

RayData RayAABBIntersection(Ray ray, AABB aabb){
    RayData result;
    result.ray = ray;
    result.distance = 0.0f;
    result.hitPoint = float3(0, 0, 0);
    result.hitNormal = float3(0, 0, 0);
    result.hitDistance = 0.0f;
    result.colorHint = 0;
    
    float3 invDir = rcp(ray.direction);
    float3 t0s = (aabb.min - ray.origin) * invDir;
    float3 t1s = (aabb.max - ray.origin) * invDir;

    float3 tmin = min(t0s, t1s);
    float3 tmax = max(t0s, t1s);

    float tminMax = max(tmin.x, max(tmin.y, tmin.z));
    float tmaxMin = min(tmax.x, min(tmax.y, tmax.z));

    if (tminMax > tmaxMin)
        return result;

    float distanceToHit = tminMax > 0.0f ? tminMax : tmaxMin;

    result.hitPoint = ray.origin + ray.direction * distanceToHit;
    result.hitNormal = normalize(result.hitPoint - aabb.min);
    result.hitDistance = distanceToHit;

    return result;
}

/*
RayData RayBVHIntersection(Ray ray, StructuredBuffer<BVHNode> bvhNodes, StructuredBuffer<Triangle> triangles, int nodeIndex)
{
    RayData result;
    result.ray = ray;
    result.distance = 0.0f;
    result.hitPoint = float3(0, 0, 0);
    result.hitNormal = float3(0, 0, 0);
    result.hitDistance = 0.0f;
    result.colorHint = 0;

    if (nodeIndex == -1)
        return result;

    BVHNode node = bvhNodes[nodeIndex];

    if (node.isLeaf)
    {
        for (int i = node.triangleIndexStart; i < node.triangleIndexStart + node.triangleIndexCount; i++)
        {
            Triangle tri = triangles[i];
            float3 vertices[3] = {tri.a, tri.b, tri.c};
            RayData hit = RayCast(ray, result.hitDistance, vertices);

            if (hit.hitDistance > 0.0f && hit.hitDistance < result.hitDistance)
                result = hit;
        }
    }
    else
    {
        RayData left = RayBVHIntersection(ray, bvhNodes, triangles, node.left);
        RayData right = RayBVHIntersection(ray, bvhNodes, triangles, node.right);

        if (left.hitDistance > 0.0f && left.hitDistance < result.hitDistance)
            result = left;

        if (right.hitDistance > 0.0f && right.hitDistance < result.hitDistance)
            result = right;
    }

    return result;
}*/

RayData RayBVHIntersection(Ray ray, float distance, StructuredBuffer<BVHNode> bvhNodes, StructuredBuffer<Triangle> triangles, int rootIndex) {

    /*ray.origin += -ray.direction * 0.005f;
    distance += 0.005f;*/

    RayData result;
    result.ray = ray;
    result.distance = 0.0f;
    result.hitPoint = float3(0, 0, 0);
    result.hitNormal = float3(0, 0, 0);
    result.hitDistance = 999999.0f;
    result.colorHint = 0;

    if (rootIndex == -1) {
        return result;
    }

    int stack[2048];
    int stackSize = 0;

    if(RayAABBIntersection(ray, bvhNodes[rootIndex].aabb).hitDistance <= 0.0f) {
        result.hitDistance = -1.0f;
        return result;
    }

    stack[stackSize++] = rootIndex;

    while (stackSize > 0) {
        int nodeIndex = stack[--stackSize];

        if (nodeIndex == -1) {
            continue;
        }
        
        BVHNode node = bvhNodes[nodeIndex];

        if (node.triangleIndexStart != -1) {
            for (int i = node.triangleIndexStart; i < node.triangleIndexStart + node.triangleIndexCount; i++) {
                Triangle tri = triangles[i];
                float3 vertices[3] = {tri.a, tri.b, tri.c};
                RayData hit = RayCast(ray, distance, vertices);
                if (hit.hitDistance > 0.0f && hit.hitDistance < result.hitDistance) {
                    result = hit;
                }
            }
        } else {
            if (node.left != -1 && RayAABBIntersection(ray, bvhNodes[node.left].aabb).hitDistance > 0.0f) {
                stack[stackSize++] = node.left;
            }
            if (node.right != -1 && RayAABBIntersection(ray, bvhNodes[node.right].aabb).hitDistance > 0.0f) {
                stack[stackSize++] = node.right;
            }
        }
    }

    if (result.hitDistance < 999999.0f) {
        result.hitPoint = ray.origin + ray.direction * result.hitDistance;
        result.hitNormal = normalize(result.hitNormal);
    } else {
        result.hitDistance = -1.0f;
    }
    
    return result;
}


#endif // RAYCAST_HLSL