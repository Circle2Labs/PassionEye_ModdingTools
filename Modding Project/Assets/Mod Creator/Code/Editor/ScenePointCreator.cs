using Code.Frameworks.InteractionSystem.Database.Enums;
using Code.Frameworks.InteractionSystem.ScenePoints;
using Code.Frameworks.InteractionSystem.ScenePoints.Base;
using Code.Frameworks.InteractionSystem.ScenePoints.Interaction;
using Code.Frameworks.InteractionSystem.ScenePoints.Interfaces;
using UnityEditor;
using UnityEngine;

namespace Code.Editor
{
    public static class ScenePointCreator
	{
		#region Scene Points

		[MenuItem("GameObject/ScenePoint/CharacterLocation Point", false)]
		public static void CreateCharacterLocation(MenuCommand cmd) => createScenePoint<CharacterLocationScenePoint>(cmd);

        #endregion

        #region Interaction Scene Points
        [MenuItem("GameObject/ScenePoint/Door Point", false)]
        public static void CreateDoor(MenuCommand cmd) => createInteractionScenePoint<DoorScenePoint>(cmd, EInteractionIdentifier.Door, false);
        
		[MenuItem("GameObject/ScenePoint/LightSwitch Point", false)]
		public static void CreateLightSwitch(MenuCommand cmd) => createInteractionScenePoint<LightSwitchScenePoint>(cmd, EInteractionIdentifier.LightSwitch, false);

        #endregion

        private static void createScenePoint<T>(MenuCommand menuCommand) where T : MonoBehaviour, IScenePoint
        {
            var go = new GameObject($"{typeof(T).Name} Point");
            go.AddComponent<T>();

            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }
        
        private static void createInteractionScenePoint<T>(MenuCommand menuCommand, EInteractionIdentifier identifier, bool sameObject) where T : MonoBehaviour, IInteractionScenePoint
        {
	        GameObject go;

	        if (sameObject)
	        {
		        go = Selection.activeGameObject;

		        if (go == null)
		        {
			        Debug.LogError("No GameObject selected");
			        return;
		        }
	        }
	        else
	        {
		        go = new GameObject($"{identifier.ToString()} Point");
		        
		        // probably should have a size parameter instead of defaulting
		        var collider = go.AddComponent<BoxCollider>();
		        collider.isTrigger = true;
		        
		        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
	        }
	        
            var scenePoint = go.AddComponent<T>();
            scenePoint.InteractionIdentifier = identifier;

            // for some reason the above method resets the layer
            go.layer = LayerMask.NameToLayer("Interaction");
            
            if (!sameObject)
            {
	            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
	            Selection.activeObject = go;
            }
        }
    }
}