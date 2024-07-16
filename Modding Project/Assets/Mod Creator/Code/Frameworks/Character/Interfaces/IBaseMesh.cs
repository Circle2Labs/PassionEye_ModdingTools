using Code.Frameworks.Character.Structs;
using Code.Frameworks.ForwardKinematics.Interfaces;
using Code.Frameworks.ForwardKinematics.Structs;

namespace Code.Frameworks.Character.Interfaces
{
	/// <summary>
	/// Interface defining common functionality for character base meshes.
	/// </summary>
	public interface IBaseMesh : ICharacterObject, IFKCompatible
	{
		public SAvatarData AvatarData { get; set; }
	}
}