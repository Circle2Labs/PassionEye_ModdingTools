using Code.Frameworks.H.Enums;
using Code.Frameworks.H.Structs;

namespace Code.Frameworks.H.Interfaces
{
	public interface IHLocation
	{
		public string Name { get; set; }
		public ELocationType Type { get; set; }
	}
}