namespace RootMotion.FinalIK
{
	[System.Serializable]
	public enum FullBodyBipedEffector {
		Body,
		LeftShoulder,
		RightShoulder,
		LeftThigh,
		RightThigh,
		LeftHand,
		RightHand,
		LeftFoot,
		RightFoot
	}

	[System.Serializable]
	public enum FullBodyBipedChain {
		LeftArm,
		RightArm,
		LeftLeg,
		RightLeg
	}
}