namespace Code.Frameworks.Character.Enums
{
	/// <summary>
	/// Accessory visibility values for <see cref="Interfaces.IAccessory"/> items.
	/// 
	/// Explanation of values:
	/// Essential - shown by default unless removed manually. Used for essential accs like tattoos, hair additions
	/// Important - shown by default unless removed manually
	/// Unimportant - automatically hidden in places like showers
	/// </summary>
	public enum EAccessoryVisibility
	{
		Essential,
		Important,
		Unimportant
	}
}