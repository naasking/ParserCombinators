using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserCombinators.LeftRecursive.ObjectOriented
{
    public abstract class Parser<T>
    {
        public abstract Result<T> Parse(string input, int pos);
        public virtual Result<T> BeginParse(string input)
        {
            return Parse(input, 0);
        }

        public Parser<Pair<T, T1>> Then<T1>(Parser<T1> p1)
        {
            return new Then<T, T1> { p0 = this, p1 = p1 };
        }

        public Parser<T> Or(Parser<T> p1)
        {
            return new Or<T> { p0 = this, p1 = p1 };
        }

        public Parser<T1> Select<T1>(Func<T, T1> selector)
        {
            return new Select<T, T1> { parser = this, selector = selector };
        }

        public static Parser<T> operator |(Parser<T> left, Parser<T> right)
        {
            return left.Or(right);
        }

        public abstract override string ToString();

        protected static Result<T> Fail(int pos, string error)
        {
            return new Result<T>(pos, error);
        }

        protected static Result<T> OK(T value, int pos)
        {
            return new Result<T>(value, pos);
        }
    }

    sealed class Lit : Parser<string>
    {
        internal string lit;
        public override Result<string> Parse(string input, int pos)
        {
            return lit.Length + pos > input.Length     ? Fail(pos, "End of stream"):
                   Equals(input, pos, lit, lit.Length) ? OK(lit, pos + lit.Length):
                                                         Fail(pos, "Expected literal '" + lit + "' but found '" + input.Substring(pos, lit.Length) + "'");
        }

        static bool Equals(string x, int ix, string y, int count)
        {
            for (int iy = 0; iy < count; ++iy, ++ix)
            {
                if (x[ix] != y[iy]) return false;
            }
            return true;
        }
        public override string ToString()
        {
            return '\'' + lit + '\'';
        }
    }

    sealed class Then<T0, T1> : Parser<Pair<T0, T1>>
    {
        internal Parser<T0> p0;
        internal Parser<T1> p1;

        public override Result<Pair<T0, T1>> Parse(string input, int pos)
        {
            var r0 = p0.Parse(input, pos);
            if (r0.Failed) return Fail(pos, r0.Error);
            var r1 = p1.Parse(input, r0.Pos);
            if (r1.Failed) return Fail(pos, r1.Error);
            return OK(Pair.Create(r0.Value, r1.Value), r1.Pos);
        }
        public override string ToString()
        {
            return p0.ToString() + ' ' + p1.ToString();
        }
    }

    sealed class Or<T> : Parser<T>
    {
        internal Parser<T> p0;
        internal Parser<T> p1;

        public override Result<T> Parse(string input, int pos)
        {
            var r0 = p0.Parse(input, pos);
            return r0.Failed ? p1.Parse(input, pos) : r0;
        }
        public override string ToString()
        {
            return p0.ToString() + " | " + p1.ToString();
        }
    }

    sealed class Select<T0, T1> : Parser<T1>
    {
        internal Parser<T0> parser;
        internal Func<T0, T1> selector;
        public override Result<T1> Parse(string input, int pos)
        {
            try
            {
                var r0 = parser.Parse(input, pos);
                return r0.Failed ? Fail(pos, r0.Error) : OK(selector(r0.Value), r0.Pos);
            }
            catch (Exception e)
            {
                return Fail(pos, e.ToString());
            }
        }
        public override string ToString()
        {
            return '(' + parser.ToString() + ")=>" + selector.Method.ReturnType.Name;// + "(" + (selector.Target?.ToString() ?? "") + ")";
        }
    }

    public sealed class Builder<T> : Parser<T>
    {
        HashSet<int> stage1 = new HashSet<int>();
        HashSet<int> stage2 = new HashSet<int>();
        Dictionary<int, Result<T>> memo = new Dictionary<int, Result<T>>();
        Parser<T> parser;
        public Builder(Func<Parser<T>, Parser<T>> build)
        {
            parser = build(this);
        }
        public override Result<T> Parse(string input, int pos)
        {
            Result<T> r0;
            return memo.TryGetValue(pos, out r0) ? r0:
                   stage1.Add(pos)               ? memo[pos] = parser.Parse(input, pos):
                   stage2.Add(pos)               ? parser.Parse(input, pos):
                                                   new Result<T>(pos, "Recursive parse failed to make progress.");
        }

        public override Result<T> BeginParse(string input)
        {
            return parser.Parse(input, 0);
        }

        public override string ToString()
        {
            return "e";
        }
        public string Describe()
        {
            return "e ::= " + parser.ToString();
        }
    }

    public static class Parse
    {
        public static Parser<string> Literal(string lit)
        {
            return new Lit { lit = lit };
        }
        public static Parser<T> Build<T>(Func<Parser<T>, Parser<T>> define)
        {
            return new Builder<T>(define);
        }
    }
}
