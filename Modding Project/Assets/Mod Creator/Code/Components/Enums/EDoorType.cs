namespace Code.Components.Enums
{
	public enum EDoorType
	{
		/// <summary>
		/// Door rotates around its local Y axis to the positive direction.
		/// </summary>
		SwingAroundYPositive,

		/// <summary>
		/// Door rotates around its local Y axis to the negative direction.
		/// </summary>
		SwingAroundYNegative,

		/// <summary>
		/// Door slides along its local +X (right) axis.
		/// </summary>
		SlideAlongPositiveX,

		/// <summary>
		/// Door slides along its local -X (left) axis.
		/// </summary>
		SlideAlongNegativeX,

		/// <summary>
		/// Door slides along its local +Z (forward) axis.
		/// </summary>
		SlideAlongPositiveZ,

		/// <summary>
		/// Door slides along its local -Z (backward) axis.
		/// </summary>
		SlideAlongNegativeZ,
        
		/// <summary>
		/// Door rotates around its local X axis to the positive direction.
		/// </summary>
		SwingAroundXPositive,

		/// <summary>
		/// Door rotates around its local X axis to the negative direction.
		/// </summary>
		SwingAroundXNegative,
        
		/// <summary>
		/// Door rotates around its local Z axis to the positive direction.
		/// </summary>
		SwingAroundZPositive,

		/// <summary>
		/// Door rotates around its local Z axis to the negative direction.
		/// </summary>
		SwingAroundZNegative,
	}
}