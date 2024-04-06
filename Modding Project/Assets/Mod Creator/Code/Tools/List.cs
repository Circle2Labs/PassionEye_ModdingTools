using System.Collections.Generic;

namespace Code.Tools
{
	public static class List
	{
		public static void AddUnique<T>(this IList<T> instance, T item)
		{
			if (!instance.Contains(item))
				instance.Add(item);
		}
	}
}