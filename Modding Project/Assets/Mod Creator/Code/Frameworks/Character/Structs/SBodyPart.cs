using System;
using Code.Frameworks.Character.Enums;
using UnityEngine;

namespace Code.Frameworks.Character.Structs
{
	[Serializable]
	public struct SBodyPart
	{
		public EBodyPartType Type;
		public Transform Bone;
	}
}