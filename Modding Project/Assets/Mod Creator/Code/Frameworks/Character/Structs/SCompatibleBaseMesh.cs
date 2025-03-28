using System;

namespace Code.Frameworks.Character.Structs
{
	[Serializable]
	public struct SCompatibleBaseMesh : IEquatable<SCompatibleBaseMesh>
	{
		public string GUID;
		public byte ID;

		public SCompatibleBaseMesh(Tuple<string, byte> identifier)
		{
			GUID = identifier.Item1;
			ID = identifier.Item2;
		}
		
		public static object[] ToObjects(SCompatibleBaseMesh[] compatibleBaseMeshes)
		{
			var outer = new object[compatibleBaseMeshes.Length];

			for (var i = 0; i < compatibleBaseMeshes.Length; i++)
			{
				var compatibleBaseMesh = compatibleBaseMeshes[i];
				
				var inner = new object[2];
				inner[0] = compatibleBaseMesh.GUID;
				inner[1] = compatibleBaseMesh.ID;

				outer[i] = inner;
			}
			
			return outer;
		}

		public static SCompatibleBaseMesh[] FromObjects(object[] objects)
		{
			var outer = new SCompatibleBaseMesh[objects.Length];

			for (var i = 0; i < objects.Length; i++)
			{
				var obj = (object[])objects[i];

				var inner = new SCompatibleBaseMesh();
				inner.GUID = (string)obj[0];
				inner.ID = (byte)obj[1];

				outer[i] = inner;
			}
			
			return outer;
		}
		
		public bool Equals(SCompatibleBaseMesh other)
		{
			return GUID == other.GUID && ID == other.ID;
		}
		
		public override bool Equals(object obj)
		{
			return obj is SCompatibleBaseMesh other && Equals(other);
		}
		
		public override int GetHashCode()
		{
			unchecked
			{
				return ((GUID != null ? GUID.GetHashCode() : 0) * 397) ^ ID.GetHashCode();
			}
		}
	}
}