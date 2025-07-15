using System;

namespace Code.Frameworks.InteractionSystem.ScenePoints.Enums
{
	[Serializable]
	[Flags]
	public enum EUsageOptionFlags
	{
		None,
		Everything,
		// Used as starting values like where something is placed by default 
		Initial,
		// Used in settings scenes like FreeHSettingsScene
		BeforePlay,
		// Used during gameplay like HScene and CharacterMakerScene
		DuringPlay,
	}
}