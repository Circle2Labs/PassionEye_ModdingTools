using System;

namespace Code.Frameworks.InteractionSystem.Database.Enums
{
	[Serializable]
	public enum EInteractionIdentifier
	{
		Undefined = 0,
		ExitToMainMenu = 1,
		LightSwitch = 2,
		Force = 3,
		Kick = 4,
		Follow = 5,
		H = 6,
		Undress = 7,
		Dress = 8,
		CharacterAnimation = 9,
		SyncedCharacterAnimation = 10,
		Door = 11,
		NPCDialog = 12,
		Item = 13,
		VendingMachine = 14,
		Seat = 15,
		NPCIdle = 16
	}
}