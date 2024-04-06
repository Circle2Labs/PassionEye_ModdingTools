using System;
using UnityEngine.Events;

namespace Code.Managers.Events
{
	[Serializable]
	public class LanguageChangedEvent : UnityEvent<string, string> { }
}