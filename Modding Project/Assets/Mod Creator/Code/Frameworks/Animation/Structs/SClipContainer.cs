using Code.Frameworks.Animation.Enums;
using System;
using UnityEngine;

namespace Code.Frameworks.Animation.Structs
{
    [Serializable]
	public struct SClipContainer
	{
        public EClipUsageFlags ClipUsageFlags;

        public EClipContainerType ClipContainerType;
        
        #region OneDimension

        [SerializeField]
		public string ParameterName;

        [SerializeField]
        public float ParameterInitialValue;

        [SerializeField]
		public float[] Thresholds;

        #endregion

        #region TwoDimensions

        [SerializeField]
        public string ParameterXName;

        [SerializeField]
        public string ParameterYName;

        [SerializeField]
        public Vector2 ParameterXYInitialValue;

        [SerializeField]
		public Vector2[] Positions;

		#endregion
		
		[SerializeField]
		public SClipDefinition[] ClipDefinitions;

		public SClipContainer Copy()
		{
			var container = new SClipContainer();
			container.ClipUsageFlags = ClipUsageFlags;
			container.ClipContainerType = ClipContainerType;
			
			container.ParameterName = ParameterName;
			container.ParameterInitialValue = ParameterInitialValue;
			
			container.Thresholds = new float[Thresholds.Length];
			
			for (var i = 0; i < Thresholds.Length; i++)
				container.Thresholds[i] = Thresholds[i];
			
			container.ParameterXName = ParameterXName;
			container.ParameterYName = ParameterYName;
			container.ParameterXYInitialValue = new Vector2(ParameterXYInitialValue.x, ParameterXYInitialValue.y);
			
			container.Positions = new Vector2[Positions.Length];
			
			for (var i = 0; i < Positions.Length; i++)
				container.Positions[i] = Positions[i];
			
			container.ClipDefinitions = new SClipDefinition[ClipDefinitions.Length];
			
			for (var i = 0; i < ClipDefinitions.Length; i++)
			{
				var current = ClipDefinitions[i];
				
				SStateIKDefinition[] copiedIKDefinitions = null;
				if (current.IKDefinitions != null)
				{
					copiedIKDefinitions = new SStateIKDefinition[current.IKDefinitions.Length];
					Array.Copy(current.IKDefinitions, copiedIKDefinitions, current.IKDefinitions.Length);
				}
				
				var copy = new SClipDefinition
				{
					AnimationClip = current.AnimationClip,
					CockTargetDefinition = current.CockTargetDefinition,
					IKDefinitions = copiedIKDefinitions
				};
				
				container.ClipDefinitions[i] = copy;
			}
			
			return container;
		}
	}
}