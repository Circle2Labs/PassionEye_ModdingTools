using System;
using UnityEngine;

namespace Code.Tools
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class BoundedCurve : PropertyAttribute
    {
        public readonly Rect bounds;

        public BoundedCurve(float width, float height)
        {
            bounds = Validate(0, 0, width, height);
        }

        public BoundedCurve(float x, float y, float width, float height)
        {
            bounds = Validate(x, y, width, height);
        }

        private Rect Validate(float x, float y, float width, float height)
        {
            if (width < 0)
            {
                x += width;
                width = Mathf.Abs(width);
            }

            if (height < 0)
            {
                y += height;
                height = Mathf.Abs(height);
            }

            return new Rect(x, y, width, height);
        }
    }
}
