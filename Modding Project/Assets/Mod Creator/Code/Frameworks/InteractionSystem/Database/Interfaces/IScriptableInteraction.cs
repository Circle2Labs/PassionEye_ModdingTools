using System.Collections.Generic;
using Code.Frameworks.InteractionSystem.Database.Enums;
using Code.Frameworks.InteractionSystem.Database.Structs;

namespace Code.Frameworks.InteractionSystem.Database.Interfaces
{
    public interface IScriptableInteraction
	{
		/// <summary>
		/// Identifier used to grab and point to the correct scriptable interaction
		/// </summary>
		public EInteractionIdentifier Identifier { get; set; }

		/// <summary>
		/// Type of the Monobehaviour that is created for this interaction
		/// </summary>
		public SInteractionMonoType InteractionMonoType { get; set; }

		/// <summary>
		/// Interaction dependencies required for this interaction to be available
		/// </summary>
		public List<EInteractionIdentifier> Dependencies { get; set; }
		
		/// <summary>
		/// Should this interaction be instantiated instantly once and then shared between runs, reusing logic and data
		/// Setting it to false would create a new instance for every run, separating logic and data
		/// A SharedInstance interaction calls Setup with parameters only once when it is initially instantiated
		/// </summary>
		public bool SharedInstance { get; set; }
		
		/// <summary>
		/// Should this interaction end immediately after Run finishes
		/// 
		/// Set it to false if you intend the interaction to run over multiple frames, make sure to call End manually after it finishes
		/// Setting it to true will make the interaction manager call End immediately after Run
		/// </summary>
		public bool IsEndDelayed { get; set; }
		
		/// <summary>
		/// Should this interaction be instantiable/runnable if it is marked busy
		/// 
		/// If SharedInstance is false
		/// Setting it to false would prevent a new instance from being created and ran until the current instance is finished
		/// 
		/// If SharedInstance is true
		/// Setting it to false would prevent it from being ran again until it is finished
		/// </summary>
		public bool AllowInteractionWhenBusy { get; set; }

        /// <summary>
        /// While the interaction is running, should it block other interactions from running?
		/// 
		/// Only affects displayed interactions on the action wheel.
        /// </summary>
        public bool BlockOthers { get; set; }

        /// <summary>
        /// Name of the category
        /// </summary>
        public string CategoryName { get; set; }
		
		/// <summary>
		/// Description of the category
		/// </summary>
		public string CategoryDescription { get; set; }
		
		/// <summary>
		/// Name of the interaction
		/// </summary>
		public string Name { get; set; }
		
		/// <summary>
		/// Description of the interaction
		/// </summary>
		public string Description { get; set; }
	}
}