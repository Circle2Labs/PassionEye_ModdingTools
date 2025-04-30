using UnityEngine;

namespace Code.Frameworks.RayTracing
{
    public struct GeoHidingAccelStruct
    {
        private int resolution;
        private int[] data;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GeoHidingAccelStruct"/> struct. 
        /// </summary>
        /// <param name="resolution">Resolution of the accelleration data.</param>
        public GeoHidingAccelStruct(int resolution)
        {
            this.resolution = resolution;
            data = new int[resolution*resolution];
        }
        
        /// <summary>
        /// Returns the triangle index at the given position.
        /// </summary>
        /// <param name="x">x coord of the UV</param>
        /// <param name="y">y coord of the UV</param>
        /// <returns></returns>
        public int GetPosition(int x, int y)
        {
            return data[x * resolution + y];
        }
        
        public int GetPosition(float x, float y)
        {
            return data[(int)(resolution*x * resolution*y)];
        }
        
        /// <summary>
        /// Sets the triangle index at the given position.
        /// </summary>
        /// <param name="x">x coord of the UV</param>
        /// <param name="y">y coord of the UV</param>
        /// <param name="value">triangle index to store</param>
        public void SetPosition(int x, int y, int value)
        {
            data[x * resolution + y] = value;
        }
        
        /// <summary>
        /// Wipes all data in the structure.
        /// </summary>
        public void Clear()
        {
            data = new int[data.Length];
        }

        public static Color Hash(int value)
        {
            if(value == 0)
                return Color.black;

            var frac = (double)value / 17 * 7907;
            frac -= (int)frac;
            
            
            return Color.HSVToRGB((float)frac, 1, 1);
        }
        
        public Color GetColorAtPos(int x, int y)
        {
            try{
                return Hash(data[x * resolution + y]);
            } catch {
                return Color.white;
            }
        }
    }
}