using System;
using System.Collections.Generic;

namespace Code.Tools
{
	public static class Tuple
	{
		[Serializable]
		public struct SerializableTuple<A, B> : IEquatable<SerializableTuple<A, B>>
		{
			public SerializableTuple(A item1, B item2)
			{
				Item1 = item1;
				Item2 = item2;
			}

			public A Item1;
			public B Item2;
			
			public bool Equals(SerializableTuple<A, B> other)
			{
				return EqualityComparer<A>.Default.Equals(Item1, other.Item1) && EqualityComparer<B>.Default.Equals(Item2, other.Item2);
			}
			
			public override bool Equals(object obj)
			{
				return obj is SerializableTuple<A, B> other && Equals(other);
			}
			
			public override int GetHashCode()
			{
				unchecked
				{
					return (EqualityComparer<A>.Default.GetHashCode(Item1) * 397) ^ EqualityComparer<B>.Default.GetHashCode(Item2);
				}
			}
			
			public static bool operator ==(SerializableTuple<A, B> left, SerializableTuple<A, B> right)
			{
				return left.Equals(right);
			}
			
			public static bool operator !=(SerializableTuple<A, B> left, SerializableTuple<A, B> right)
			{
				return !left.Equals(right);
			}
		}
		
		[Serializable]
		public struct SerializableTuple<A, B, C> : IEquatable<SerializableTuple<A, B, C>>
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
			
			public bool Equals(SerializableTuple<A, B, C> other)
			{
				return EqualityComparer<A>.Default.Equals(Item1, other.Item1) && EqualityComparer<B>.Default.Equals(Item2, other.Item2) && EqualityComparer<C>.Default.Equals(Item3, other.Item3);
			}
			
			public override bool Equals(object obj)
			{
				return obj is SerializableTuple<A, B, C> other && Equals(other);
			}
			
			public override int GetHashCode()
			{
				unchecked
				{
					var hashCode = EqualityComparer<A>.Default.GetHashCode(Item1);
					hashCode = (hashCode * 397) ^ EqualityComparer<B>.Default.GetHashCode(Item2);
					hashCode = (hashCode * 397) ^ EqualityComparer<C>.Default.GetHashCode(Item3);
					return hashCode;
				}
			}
			
			public static bool operator ==(SerializableTuple<A, B, C> left, SerializableTuple<A, B, C> right)
			{
				return left.Equals(right);
			}
			
			public static bool operator !=(SerializableTuple<A, B, C> left, SerializableTuple<A, B, C> right)
			{
				return !left.Equals(right);
			}
		}
		
		[Serializable]
		public struct SerializableTuple<A, B, C, D> : IEquatable<SerializableTuple<A, B, C, D>>
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
			
			public bool Equals(SerializableTuple<A, B, C, D> other)
			{
				return EqualityComparer<A>.Default.Equals(Item1, other.Item1) && EqualityComparer<B>.Default.Equals(Item2, other.Item2) && EqualityComparer<C>.Default.Equals(Item3, other.Item3) && EqualityComparer<D>.Default.Equals(Item4, other.Item4);
			}
			
			public override bool Equals(object obj)
			{
				return obj is SerializableTuple<A, B, C, D> other && Equals(other);
			}
			
			public override int GetHashCode()
			{
				unchecked
				{
					var hashCode = EqualityComparer<A>.Default.GetHashCode(Item1);
					hashCode = (hashCode * 397) ^ EqualityComparer<B>.Default.GetHashCode(Item2);
					hashCode = (hashCode * 397) ^ EqualityComparer<C>.Default.GetHashCode(Item3);
					hashCode = (hashCode * 397) ^ EqualityComparer<D>.Default.GetHashCode(Item4);
					return hashCode;
				}
			}
			
			public static bool operator ==(SerializableTuple<A, B, C, D> left, SerializableTuple<A, B, C, D> right)
			{
				return left.Equals(right);
			}
			
			public static bool operator !=(SerializableTuple<A, B, C, D> left, SerializableTuple<A, B, C, D> right)
			{
				return !left.Equals(right);
			}
		}
	}
}