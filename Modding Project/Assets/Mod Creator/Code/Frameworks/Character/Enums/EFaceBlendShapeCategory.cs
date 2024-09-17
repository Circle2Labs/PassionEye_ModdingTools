namespace Code.Frameworks.Character.Enums
{
	public enum EFaceBlendShapeCategory
	{
		None,
		Face, // face depth, size, height, width
		Cheeks, // cheekbone width, depth && cheek vertical, depth, width
		Ears, // ears size, shape, rotation, scale, offset
		Jaw, // jaw vertical, depth, width, shape
		Chin, // chin vertical, depth, width, tip width, scale
		Eyes, // eyelid 1 2 3 4 5 6 shape, rotation && eye vertical spacing, rotation, scale
		Nose, // tip vertical, depth, shape && bridge vertical shape, horizontal shape, vertical position
		Mouth, // mouth vertical position, width, depth && upper lip size, lower lip size && corner mouth shape
		Expression,
		Eyebrows,
		UpperEyelids,
		LowerEyelids
	}
}