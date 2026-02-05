using Code.Frameworks.InteractionSystem.Structs;

namespace Code.Frameworks.InteractionSystem.Interactions
{
	public interface IBaseInteraction
	{
		public SRunParameters RunParameters { get; set; }

		/// <summary>
		/// An interaction declared as running is marked as 'active' or 'currently playing'.
		/// </summary>
		public bool IsPlaying { get; set; }
		
		public bool IsApproaching { get; set; }

		/// <summary>
		/// After approaching finishes, should the interaction be started
		/// </summary>
		public bool RunAfterApproach { get; set; }
        
		public void Run(SRunParameters parameters);
		public void End();
		public void Cancel();

		public void Setup(object[] parameters);
	}
}