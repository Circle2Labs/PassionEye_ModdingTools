using Code.Frameworks.Character.Structs;

namespace Code.Frameworks.Character.Interfaces
{
	/// <summary>
	/// Interface defining common functionality for character base meshes.
	/// </summary>
	public interface IBaseMesh : ICharacterObject
	{
		public SAvatarData AvatarData { get; set; }
	}
}