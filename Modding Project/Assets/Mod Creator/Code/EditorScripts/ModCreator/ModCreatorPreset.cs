#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.EditorScripts.ModCreator
{
	[Serializable]
	public class ModCreatorPreset : MonoBehaviour
	{
		[SerializeField] 
		public string Name;
		[SerializeField] 
		public string ModId;
		[SerializeField] 
		public string Description;
		[SerializeField] 
		public string Version;
		[SerializeField] 
		public string Author;

		[SerializeField] 
		public string[] CompatibleVersions;
		[SerializeField] 
		public string[] Dependencies;

		[SerializeField]
		public List<UnityEngine.Object> Prefabs;
		[SerializeField]
		public List<Template> Templates;
	}
}
#endif