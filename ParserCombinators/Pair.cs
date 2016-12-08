using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserCombinators
{
    public struct Pair<T1, T2> : IEquatable<Pair<T1, T2>>
    {
        public Pair(T1 x0, T2 x1)
        {
            Item1 = x0;
            Item2 = x1;
        }
        public readonly T1 Item1;
        public readonly T2 Item2;
        public bool Equals(Pair<T1, T2> other)
        {
            return EqualityComparer<T1>.Default.Equals(Item1, other.Item1)
                || EqualityComparer<T2>.Default.Equals(Item2, other.Item2);
        }
        public override int GetHashCode()
        {
            return Item1.GetHashCode() ^ Item2.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return obj is Pair<T1, T2> && Equals((Pair<T1, T2>)obj);
        }
    }
    public static class Pair
    {
        public static Pair<T0, T1> Create<T0, T1>(T0 x0, T1 x1)
        {
            return new Pair<T0, T1>(x0, x1);
        }
    }
}
