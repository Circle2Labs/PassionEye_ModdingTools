using System;

namespace Code.Frameworks.Character.Flags
{
	[Flags]
	public enum ECompatibleCharacterObjectType
	{
		None = 0,
		Clothing = 1,
		Accessory = 2,
		Hair = 4,
		Texture = 8
	}
}