using System;
using System.Collections.Generic;
using UnityEngine;
using Tuple = Code.Tools.Tuple;

namespace Code.Frameworks.Shared.Structs
{
	[Serializable]
	public struct SVariant
	{
		[SerializeField]
		public string Name;
		
		[SerializeField]
		public List<Tuple.SerializableTuple<Transform, bool>> Transforms;
	}
}