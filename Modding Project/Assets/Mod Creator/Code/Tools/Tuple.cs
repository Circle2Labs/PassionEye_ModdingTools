using System;

namespace Code.Tools
{
	public static class Tuple
	{
		[Serializable]
		public struct SerializableTuple<A, B>
		{
			public SerializableTuple(A item1, B item2)
			{
				Item1 = item1;
				Item2 = item2;
			}

			public A Item1;
			public B Item2;
		}
		
		[Serializable]
		public struct SerializableTuple<A, B, C>
		{
			public SerializableTuple(A item1, B item2, C item3)
			{
				Item1 = item1;
				Item2 = item2;
				Item3 = item3;
			}
			
			public A Item1;
			public B Item2;
			public C Item3;
		}
		
		[Serializable]
		public struct SerializableTuple<A, B, C, D>
		{
			public SerializableTuple(A item1, B item2, C item3, D item4)
			{
				Item1 = item1;
				Item2 = item2;
				Item3 = item3;
				Item4 = item4;
			}
			
			public A Item1;
			public B Item2;
			public C Item3;
			public D Item4;
		}
	}
}