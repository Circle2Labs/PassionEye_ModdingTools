using System;

namespace Code.Frameworks.Animation.Enums
{
	[Flags]
	public enum EAnimationUsageFlags
	{
		None = 0,
		CharacterMaker = 1,
		Studio = 2,
		HScene = 4,
		Interaction = 8,
		Object = 16,
		Flat = 32,
		Seated = 64,
		Wall = 128
	}
}