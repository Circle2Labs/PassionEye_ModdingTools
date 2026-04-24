#ifndef RAYDATA_HLSL
#define RAYDATA_HLSL

struct Ray{
    float3 origin;
    float3 direction;
};

struct Triangle{
    float3 a;
    float3 b;
    float3 c;
};

struct RayData
{
    Ray ray;
    float distance;
    float3 hitPoint;
    float3 hitNormal;
    float hitDistance;
    int colorHint; //TODO: remove this
};

struct AABB {
    float3 min;
    float3 max;
};

struct BVHNode{
    AABB aabb;
    int parent;
    int left;
    int right;
    int triangleIndexStart;
    int triangleIndexCount;
};

#endif // RAYDATA_HLSL