using Code.Frameworks.Shared.Structs;

namespace Code.Frameworks.Shared.Interfaces
{
	public interface IHasVariants
	{
		public SVariant[] Variants { get; set; }
        
		public int Variant { get; }
		
		public void SetVariant(int variant);
	}
}