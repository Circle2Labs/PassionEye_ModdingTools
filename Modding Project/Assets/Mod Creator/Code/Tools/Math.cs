using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Code.Tools
{
	public static class Math
	{
		public static float Remap(float value, float minfrom, float maxfrom, float minto, float maxto)
		{
			return minto + (value - minfrom) * (maxto - minto) / (maxfrom - minfrom);
		}

		public static float EaseInOutQuad(float t, float b, float c, float d)
		{
			t /= d / 2;
			if (t < 1) return c / 2 * t * t + b;
			t--;
			return -c / 2 * (t * (t - 2) - 1) + b;
		}
		
		#region BezierImplementation
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float QuadraticBezier(float a, float b, float c, float t) 
			=> (1 - t) * ((1 - t) * a + (t * b)) + t * ((1 - t) * b + (t * c));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float CubicBezier(float a, float b, float c, float d, float t) 
			=> (1-t)*QuadraticBezier(a, b, c, t) + t*QuadraticBezier(b, c, d, t);
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 QuadraticBezier(Vector2 a, Vector2 b, Vector2 c, float t) 
			=> (1 - t) * ((1 - t) * a + (t * b)) + t * ((1 - t) * b + (t * c));
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 CubicBezier(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t) 
			=> (1-t)*QuadraticBezier(a, b, c, t) + t*QuadraticBezier(b, c, d, t);
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 QuadraticBezier(Vector3 a, Vector3 b, Vector3 c, float t) 
			=> (1 - t) * ((1 - t) * a + (t * b)) + t * ((1 - t) * b + (t * c));
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 CubicBezier(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t) 
			=> (1-t)*QuadraticBezier(a, b, c, t) + t*QuadraticBezier(b, c, d, t);
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector4 QuadraticBezier(Vector4 a, Vector4 b, Vector4 c, float t) 
			=> (1 - t) * ((1 - t) * a + (t * b)) + t * ((1 - t) * b + (t * c));
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector4 CubicBezier(Vector4 a, Vector4 b, Vector4 c, Vector4 d, float t) 
			=> (1-t)*QuadraticBezier(a, b, c, t) + t*QuadraticBezier(b, c, d, t);
		
		static float3x3 QUADRATIC_B_MAT = new float3x3( 1, -2, 1, 0, 2, -2, 0, 0, 1);
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float QuadraticBezierMatrix(float a, float b, float c, float t) {
			float3x3 matX = math.transpose(QUADRATIC_B_MAT);
			matX[0] *= a;
			matX[1] *= b;
			matX[2] *= c;

			float3 powerSeries = new float3(1, t, t * t);
			
			float result;
			float3 resultX = math.mul(powerSeries, matX); 
			
			return resultX.x + resultX.y + resultX.z;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float2 QuadraticBezierMatrix(float2 a, float2 b, float2 c, float t) {
			float3x3 matX = math.transpose(QUADRATIC_B_MAT);
			matX[0] *= a.x;
			matX[1] *= b.x;
			matX[2] *= c.x;
    
			float3x3 matY = math.transpose(QUADRATIC_B_MAT);
			matY[0] *= a.y;
			matY[1] *= b.y;
			matY[2] *= c.y;

			float3 powerSeries = new float3(1, t, t * t);
			
			float2 result;
			float3 resultX = math.mul(powerSeries, matX); 
			float3 resultY = math.mul(powerSeries, matY);
    
			result.x = resultX.x + resultX.y + resultX.z;
			result.y = resultY.x + resultY.y + resultY.z;
			return result;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float3 QuadraticBezierMatrix(float3 a, float3 b, float3 c, float t) {
			float3x3 matX = math.transpose(QUADRATIC_B_MAT);
			matX[0] *= a.x;
			matX[1] *= b.x;
			matX[2] *= c.x;
    
			float3x3 matY = math.transpose(QUADRATIC_B_MAT);
			matY[0] *= a.y;
			matY[1] *= b.y;
			matY[2] *= c.y;

			float3x3 matZ = math.transpose(QUADRATIC_B_MAT);
			matZ[0] *= a.z;
			matZ[1] *= b.z;
			matZ[2] *= c.z;

			float3 powerSeries = new float3(1, t, t * t);
			
			float3 result;
			float3 resultX = math.mul(powerSeries, matX); 
			float3 resultY = math.mul(powerSeries, matY);
			float3 resultZ = math.mul(powerSeries, matZ);
    
			result.x = resultX.x + resultX.y + resultX.z;
			result.y = resultY.x + resultY.y + resultY.z;
			result.z = resultZ.x + resultZ.y + resultZ.z;
			return result;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float4 QuadraticBezierMatrix(float4 a, float4 b, float4 c, float t) {
			float3x3 matX = math.transpose(QUADRATIC_B_MAT);
			matX[0] *= a.x;
			matX[1] *= b.x;
			matX[2] *= c.x;
    
			float3x3 matY = math.transpose(QUADRATIC_B_MAT);
			matY[0] *= a.y;
			matY[1] *= b.y;
			matY[2] *= c.y;

			float3x3 matZ = math.transpose(QUADRATIC_B_MAT);
			matZ[0] *= a.z;
			matZ[1] *= b.z;
			matZ[2] *= c.z;

			float3x3 matW = math.transpose(QUADRATIC_B_MAT);
			matW[0] *= a.w;
			matW[1] *= b.w;
			matW[2] *= c.w;

			float3 powerSeries = new float3(1, t, t * t);
			
			float4 result;
			float3 resultX = math.mul(powerSeries, matX); 
			float3 resultY = math.mul(powerSeries, matY);
			float3 resultZ = math.mul(powerSeries, matZ);
			float3 resultW = math.mul(powerSeries, matW);
    
			result.x = resultX.x + resultX.y + resultX.z;
			result.y = resultY.x + resultY.y + resultY.z;
			result.z = resultZ.x + resultZ.y + resultZ.z;
			result.w = resultW.x + resultW.y + resultW.z;
			return result;
		}

		public static float4x4 CUBIC_B_MAT = new  float4x4(1, 0, 0, 0, -3, 3, 0, 0, 3, -6, 3, 0, -1, 3, -3, 1);
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float CubicBezierMatrix(float a, float b, float c, float d, float t)
		{
			float4x4 matX = math.transpose(CUBIC_B_MAT);
			matX[0] *= a;
			matX[1] *= b;
			matX[2] *= c;
			matX[3] *= d;
			
			float4 powerSeries = new float4(1, t, t * t, t*t*t);
			
			float4 resultX = math.mul(powerSeries, matX);
			
			return resultX.x + resultX.y + resultX.z + resultX.w;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float2 CubicBezierMatrix(float2 a, float2 b, float2 c, float2 d, float t)
		{
			float4x4 matX = math.transpose(CUBIC_B_MAT);
			matX[0] *= a.x;
			matX[1] *= b.x;
			matX[2] *= c.x;
			matX[3] *= d.x;
			
			float4x4 matY = math.transpose(CUBIC_B_MAT);
			matY[0] *= a.y;
			matY[1] *= b.y;
			matY[2] *= c.y;
			matY[3] *= d.y;
			
			float4 powerSeries = new float4(1, t, t * t, t*t*t);
			
			float2 result;
			float4 resultX = math.mul(powerSeries, matX);
			float4 resultY = math.mul(powerSeries, matY);
			
			result.x = resultX.x + resultX.y + resultX.z + resultX.w;
			result.y = resultY.x + resultY.y + resultY.z + resultY.w;
			
			return result;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float3 CubicBezierMatrix(float3 a, float3 b, float3 c, float3 d, float t)
		{
			float4x4 matX = math.transpose(CUBIC_B_MAT);
			matX[0] *= a.x;
			matX[1] *= b.x;
			matX[2] *= c.x;
			matX[3] *= d.x;
			
			float4x4 matY = math.transpose(CUBIC_B_MAT);
			matY[0] *= a.y;
			matY[1] *= b.y;
			matY[2] *= c.y;
			matY[3] *= d.y;
			
			float4x4 matZ = math.transpose(CUBIC_B_MAT);
			matZ[0] *= a.z;
			matZ[1] *= b.z;
			matZ[2] *= c.z;
			matZ[3] *= d.z;
			
			float4 powerSeries = new float4(1, t, t * t, t*t*t);
			
			float3 result;
			float4 resultX = math.mul(powerSeries, matX);
			float4 resultY = math.mul(powerSeries, matY);
			float4 resultZ = math.mul(powerSeries, matZ);
			
			result.x = resultX.x + resultX.y + resultX.z + resultX.w;
			result.y = resultY.x + resultY.y + resultY.z + resultY.w;
			result.z = resultZ.x + resultZ.y + resultZ.z + resultZ.w;
			
			return result;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float4 CubicBezierMatrix(float4 a, float4 b, float4 c, float4 d, float t)
		{
			float4x4 matX = math.transpose(CUBIC_B_MAT);
			matX[0] *= a.x;
			matX[1] *= b.x;
			matX[2] *= c.x;
			matX[3] *= d.x;
			
			float4x4 matY = math.transpose(CUBIC_B_MAT);
			matY[0] *= a.y;
			matY[1] *= b.y;
			matY[2] *= c.y;
			matY[3] *= d.y;
			
			float4x4 matZ = math.transpose(CUBIC_B_MAT);
			matZ[0] *= a.z;
			matZ[1] *= b.z;
			matZ[2] *= c.z;
			matZ[3] *= d.z;
			
			float4x4 matW = math.transpose(CUBIC_B_MAT);
			matW[0] *= a.w;
			matW[1] *= b.w;
			matW[2] *= c.w;
			matW[3] *= d.w;
			
			float4 powerSeries = new float4(1, t, t * t, t*t*t);
			
			float4 result;
			float4 resultX = math.mul(powerSeries, matX);
			float4 resultY = math.mul(powerSeries, matY);
			float4 resultZ = math.mul(powerSeries, matZ);
			float4 resultW = math.mul(powerSeries, matW);
			
			result.x = resultX.x + resultX.y + resultX.z + resultX.w;
			result.y = resultY.x + resultY.y + resultY.z + resultY.w;
			result.z = resultZ.x + resultZ.y + resultZ.z + resultZ.w;
			result.w = resultW.x + resultW.y + resultW.z + resultW.w;
			
			return result;
		}
		
		#endregion
		
		
		#region 2D Math
		/// <summary>
        /// Get the bounding box of the triangle provided by the 3 points in UV coords
        /// </summary>
        /// <param name="points">Tuple of three UV coords</param>
        /// <returns><see cref="Vector4"/> containing minX, minY, maxX, maxY</returns>
        public static Vector4 GetBoundingBox(Tuple<Vector2, Vector2, Vector2> points)
        {
            float minX = Mathf.Min(points.Item1.x, points.Item2.x, points.Item3.x);
            float minY = Mathf.Min(points.Item1.y, points.Item2.y, points.Item3.y);
            float maxX = Mathf.Max(points.Item1.x, points.Item2.x, points.Item3.x);
            float maxY = Mathf.Max(points.Item1.y, points.Item2.y, points.Item3.y);
            return new Vector4(minX, minY, maxX, maxY);
        }
		
        /// <summary>
        /// Calculate the area of a triangle defined by three points
        /// </summary>
        /// <param name="pointA"></param>
        /// <param name="pointB"></param>
        /// <param name="pointC"></param>
        /// <returns></returns>
        public static float Area(Vector2 pointA, Vector2 pointB, Vector2 pointC)
        {
            Vector2 v1 = pointA - pointC;
            Vector2 v2 = pointB - pointC;
            return (v1.x * v2.y - v1.y * v2.x) / 2;
        }
        
        /// <summary>
        /// Calculates if uv is inside the supplied triangle
        /// </summary>
        /// <param name="uv">point to test</param>
        /// <param name="points">tuple of 3 points which represents the triangle to test uv point after</param>
        /// <returns>null if the point is outside, otherwise returns the coordinates in object space </returns>
        public static Vector3? BarycentricCoords(Vector2 uv, Tuple<Vector2, Vector2, Vector2> points)
        {
            var a = Area(points.Item1, points.Item2, points.Item3);
            if (a == 0)
                return null;
            
            var a1 = Area(points.Item2, points.Item3, uv) / a;
            if (a1 < 0)
                return null;
            
            var a2 = Area(points.Item3, points.Item1, uv) / a;
            if (a2 < 0)
                return null;
            
            var a3 = Area(points.Item1, points.Item2, uv) / a;
            if (a3 < 0)
                return null;
            
            return new Vector3(a1, a2, a3);
        }
        
        #endregion
        
        #region transform
        
        public static Vector3 TransformDirection(Vector3 dir, Quaternion rotation)
        {
	        return rotation * dir;
        }
        
        public static Vector3 TransformPoint(Vector3 point, Vector3 position, Quaternion rotation, Vector3 scale)
        {
	        return position + rotation * Vector3.Scale(point, scale);
        }
        
        #endregion
	}
}