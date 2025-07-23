using System;
using System.Collections.Generic;
using Code.Frameworks.InteractionSystem.ScenePoints.Interfaces;
using UnityEngine;

namespace Code.Frameworks.InteractionSystem.Structs
{
    [Serializable]
    public struct SRunParameters
    {
	    public IInteractionScenePoint ScenePoint;
	    
		public GameObject Caller;
		public GameObject Trigger;
		public List<GameObject> Target;
	}
}