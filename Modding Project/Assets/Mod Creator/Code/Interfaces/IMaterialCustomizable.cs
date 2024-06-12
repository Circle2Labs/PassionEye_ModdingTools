using System.Collections.Generic;
using UnityEngine;

namespace Code.Interfaces
{
	public interface IMaterialCustomizable
	{
		public Renderer[] Renderers { get; set; }
		
		public GameObject GetGameObject();

		public List<LODGroup> GetLODGroups();
		
		public int GetLOD(SkinnedMeshRenderer rend);
	}
}