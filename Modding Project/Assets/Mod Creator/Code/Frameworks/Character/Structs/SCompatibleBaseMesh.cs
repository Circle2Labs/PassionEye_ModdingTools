using System;
using Code.Frameworks.Character.Flags;

namespace Code.Frameworks.Character.Structs
{
	[Serializable]
	public struct SCompatibleBaseMesh : IEquatable<SCompatibleBaseMesh>
	{
		public string GUID;
		public byte ID;

		public ECompatibleCharacterObjectType TypesCompatible;

		public string[] BoneAliasFrom;
		public string[] BoneAliasTo;

		public SCompatibleBaseMesh(Tuple<string, byte> identifier, ECompatibleCharacterObjectType typesCompatible = ECompatibleCharacterObjectType.None, string[] boneAliasFrom = null, string[] boneAliasTo = null)
		{
			GUID = identifier.Item1;
			ID = identifier.Item2;

			TypesCompatible = typesCompatible;
			BoneAliasFrom = boneAliasFrom;
			BoneAliasTo = boneAliasTo;
		}
		
		public static object[] ToObjects(SCompatibleBaseMesh[] compatibleBaseMeshes)
		{
			var outer = new object[compatibleBaseMeshes.Length];

			for (var i = 0; i < compatibleBaseMeshes.Length; i++)
			{
				var compatibleBaseMesh = compatibleBaseMeshes[i];
				
				var inner = new object[5];
				inner[0] = compatibleBaseMesh.GUID;
				inner[1] = compatibleBaseMesh.ID;
				inner[2] = compatibleBaseMesh.TypesCompatible;
				inner[3] = compatibleBaseMesh.BoneAliasFrom;
				inner[4] = compatibleBaseMesh.BoneAliasTo;

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

				if (obj.Length == 2)
				{
					// old data compat
					inner.TypesCompatible = ECompatibleCharacterObjectType.Clothing | ECompatibleCharacterObjectType.Accessory | ECompatibleCharacterObjectType.Hair | ECompatibleCharacterObjectType.Texture;
					
					inner.BoneAliasFrom = null;
					inner.BoneAliasTo = null;
				}
				else if (obj.Length == 5)
				{
					if (obj[2] is bool texturesCompatible)
					{
						// old data compat
						inner.TypesCompatible = ECompatibleCharacterObjectType.Clothing | ECompatibleCharacterObjectType.Accessory | ECompatibleCharacterObjectType.Hair | ECompatibleCharacterObjectType.Texture;

						if (!texturesCompatible)
							inner.TypesCompatible &= ~ECompatibleCharacterObjectType.Texture;
					}
					else
					{
						inner.TypesCompatible = (ECompatibleCharacterObjectType)obj[2];
					}
					
					inner.BoneAliasFrom = toStringArray(obj[3]);
					inner.BoneAliasTo = toStringArray(obj[4]);
				}

				outer[i] = inner;
			}
			
			return outer;
		}

		private static string[] toStringArray(object obj)
		{
			return obj is object[] objArray ? toStringArray(objArray) : null;
		}
		
		private static string[] toStringArray(object[] objArray)
		{
			if (objArray == null)
				return null;

			var strArray = new string[objArray.Length];

			for (var i = 0; i < objArray.Length; i++)
				strArray[i] = Convert.ToString(objArray[i]);
			
			return strArray;
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