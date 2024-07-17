using Code.Frameworks.ForwardKinematics.Structs;

namespace Code.Frameworks.ForwardKinematics.Interfaces
{
	public interface IFKCompatible
	{
		public SFKData FKData { get; set; }
	}
}