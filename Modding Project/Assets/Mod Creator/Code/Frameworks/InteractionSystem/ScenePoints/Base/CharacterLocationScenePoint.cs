using UnityEngine;

namespace Code.Frameworks.InteractionSystem.ScenePoints.Base
{
	public class CharacterLocationScenePoint : BaseScenePoint
	{ 
		[SerializeField]
		public int CharacterIndex;
				
#if UNITY_EDITOR
		public void OnDrawGizmos()
		{
			Gizmos.matrix = transform.localToWorldMatrix;
			
			var boxSize = new Vector3(0.3f, 1.56f, 0.3f);
			
			Gizmos.DrawWireCube(new Vector3(0f, boxSize.y / 2f, 0f), boxSize);
			
			Gizmos.DrawLine(Vector3.zero, new Vector3(0f, 0f, 0.15f));
			
			Gizmos.DrawLine(new Vector3(0f, 0f, 0.15f), new Vector3(0.15f, 0f, 0f));
			Gizmos.DrawLine(new Vector3(0f, 0f, 0.15f), new Vector3(-0.15f, 0f, 0f));
		}
#endif
	}
}