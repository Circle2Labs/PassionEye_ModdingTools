using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Tools
{
	public static class Object
	{
		public static string GetGameObjectPath(Transform transform, Transform startFrom = null)
		{
			var path = transform.name;
			while (transform.parent != startFrom && transform != startFrom)
			{
				transform = transform.parent;
				path = transform.name + "/" + path;
			}
			return path;
		}

		public static Scene GetSceneBelow(Vector3 point)
		{
			if (Physics.Raycast(point, Vector3.down, out var hit, float.MaxValue, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore))
				return hit.transform.gameObject.scene;

			Debug.LogWarning($"[Object] Failed to grab scene below point {point}");
			return default;
		}
	}
}