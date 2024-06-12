using Code.Tools;
using UnityEditor;
using UnityEngine;

namespace Code.Editor.CustomPropertyDrawers
{
    //https://medium.com/@obaynaeem/bounded-grid-for-animation-curves-in-unity-e3b14f99e372

    [CustomPropertyDrawer(typeof(BoundedCurve))]
    public class BoundedCurvePropertyDrawer : PropertyDrawer
    {
        private bool isBoundsChecked;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            BoundedCurve attr = (BoundedCurve)attribute;

            EditorGUI.BeginProperty(position, label, property);

            if (!Validate(property.type))
            {
                EditorGUILayout.HelpBox("Bounded Curve Attribute Can Be Used Only On Animation Curves", MessageType.Error);
                return;
            }
            if (!isBoundsChecked)
            {
                Validate(property, attr.bounds);
                isBoundsChecked = true;
            }

            property.animationCurveValue = EditorGUI.CurveField(position, label, property.animationCurveValue, Color.green, attr.bounds);

            EditorGUI.EndProperty();
        }

        private bool Validate(string fieldType) => fieldType == nameof(AnimationCurve);

        /// <summary>
        /// validate the first and last keyframe on the curve
        /// and reset the curve if the validation failed
        /// </summary>
        private void Validate(SerializedProperty property, Rect range)
        {
            AnimationCurve curve = property.animationCurveValue;
            int curveLength = curve.length;

            if (curveLength < 1)
                return;

            if (!curve.keys[0].ValidateKeyFrameCor(range.min, range.max) ||
                !curve.keys[curveLength - 1].ValidateKeyFrameCor(range.min, range.max))
            {
                property.animationCurveValue = AnimationCurve.Constant(range.xMin, range.xMax, range.yMax);
            }
        }
    }
    public static class KeyframeExtentions
    {
        /// <summary>
        /// checks if a keyframes' value is within bounds
        /// </summary>
        /// <returns>true if the keyframe is within the bounds</returns>
        public static bool ValidateKeyFrameCor(this Keyframe keyframe, Vector2 minPoint, Vector2 MaxPoint)
        {
            if (keyframe.value >= minPoint.y && keyframe.value <= MaxPoint.y &&
                keyframe.time >= minPoint.x && keyframe.time <= MaxPoint.x)
                return true;


            return false;

        }
    }
}
