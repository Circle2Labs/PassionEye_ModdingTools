using System;

namespace Code.Frameworks.InteractionSystem.ScenePoints.Enums
{
	/// <summary>
	/// Instantiation logic that runs when the scene is loaded, allowing given scenepoint to be copied over to characters or left be as it is
	/// This is useful in cases where you have a character scenepoint that needs additional configuration done in editor
	/// </summary>
	[Flags]
	public enum EInstantiationFlags
	{
		// Keep the scenepoint as it is
		None = 0,
		
		// Clone the scenepoint, add it to all male characters and disable the original
		Male = 1,
		
		// Clone the scenepoint, add it to all female characters and disable the original
		Female = 2
	}
}