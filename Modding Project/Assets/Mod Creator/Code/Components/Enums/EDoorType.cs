namespace Code.Components.Enums
{
    public enum EDoorType
    {
        /// <summary>
        /// Door rotates around its local Y axis to open in the local +Z (forward) direction.
        /// </summary>
        SwingAroundYTowardsPositiveZ,

        /// <summary>
        /// Door rotates around its local Y axis to open in the local -Z (backward) direction.
        /// </summary>
        SwingAroundYTowardsNegativeZ,

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
        SlideAlongNegativeZ
    }
}
