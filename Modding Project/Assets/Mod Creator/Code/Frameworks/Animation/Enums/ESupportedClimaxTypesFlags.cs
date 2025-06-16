using System;

namespace Code.Frameworks.Animation.Enums
{
	[Flags]
	public enum ESupportedClimaxTypesFlags
	{
		None = 0,
		Inside = 1, // Inside body like mouth or butt
		Outside = 2, // Outside body like stomach or floor
		NoLiquid = 4 // Things like kissing and touching
	}
}