using System;
using UnityEngine;

namespace Code.Frameworks.PhysicsSimulation
{
	[Serializable]
	public struct Simulation
	{
		[field: SerializeField]
		[field: Tooltip("Decides how this objects armature will be handled when preparing the simulation.")]
		public EBoneAssignmentMode BoneAssignmentMode { get; set; }

		[field: SerializeField]
		[field: Tooltip("SimulationData for this object")]
		public SimulationData[] SimulationData { get; set; }

		public Simulation Copy()
		{
			SimulationData[] simulationData = null;

			if(SimulationData != null)
			{
				simulationData = new SimulationData[SimulationData.Length];
				for (int i = 0; i < SimulationData.Length; i++)
				{
					SimulationData[i] = SimulationData[i].Copy();
				}
			}

			return new Simulation
			{
				BoneAssignmentMode = BoneAssignmentMode,
				SimulationData = simulationData
			};
		}
	}
}