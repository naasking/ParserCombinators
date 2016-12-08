using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserCombinators.LeftRecursive.Functional
{
    public struct Parser<T>
    {
        public Parser(Func<string, int, Result<T>> parse)
        {
            Parse = parse;
        }
        public Parser(Func<Parser<T>, Parser<T>> define) : this()
        {
            var stage1 = new HashSet<int>();
            var stage2 = new HashSet<int>();
            var memo = new Dictionary<int, Result<T>>();
            var parse = Parse;
            var delay = new Parser<T>((input, pos) =>
            {
                Result<T> r0;
                return memo.TryGetValue(pos, out r0) ? r0:
                       stage1.Add(pos)               ? memo[pos] = parse(input, pos):
                       stage2.Add(pos)               ? parse(input, pos):
                                                       new Result<T>(pos, "Recursive parse failed to make progress.");
            });
            parse = Parse = define(delay).Parse;
        }

        public Func<string, int, Result<T>> Parse { get; internal set; }

        public Result<T> BeginParse(string input)
        {
            return Parse(input, 0);
        }

        public Parser<Pair<T, T1>> Then<T1>(Parser<T1> p1)
        {
            var parse = Parse;
            return new Parser<Pair<T, T1>>((input, pos) =>
            {
                var r0 = parse(input, pos);
                if (r0.Failed) return new Result<Pair<T, T1>>(pos, r0.Error);
                var r1 = p1.Parse(input, r0.Pos);
                if (r1.Failed) return new Result<Pair<T, T1>>(pos, r1.Error);
                return new Result<Pair<T, T1>>(Pair.Create(r0.Value, r1.Value), r1.Pos);
            });
        }

        public Parser<T> Or(Parser<T> p1)
        {
            var parse = Parse;
            return new Parser<T>((input, pos) =>
            {
                var r0 = parse(input, pos);
                return r0.Failed ? p1.Parse(input, pos) : r0;
            });
        }

        public Parser<T1> Select< T1>(Func<T, T1> selector)
        {
            var parse = Parse;
            return new Parser<T1>((input, pos) =>
            {
                try
                {
                    var r0 = parse(input, pos);
                    return r0.Failed ? new Result<T1>(pos, r0.Error):
                                       new Result<T1>(selector(r0.Value), r0.Pos);
                }
                catch (Exception e)
                {
                    return new Result<T1>(pos, e.ToString());
                }
            });
        }

        public static Parser<T> operator |(Parser<T> left, Parser<T> right)
        {
            return left.Or(right);
        }
        public static implicit operator Parser<T>(Func<string, int, Result<T>> parse)
        {
            return new Parser<T>(parse);
        }
    }

    public static class Parse
    {
        public static Parser<string> Literal(string lit)
        {
            return new Parser<string>((input, pos) =>
                lit.Length + pos > input.Length     ? new Result<string>(pos, "End of stream"):
                Equals(input, pos, lit, lit.Length) ? new Result<string>(lit, pos + lit.Length):
                                                      new Result<string>(pos, "Expected literal '" + lit + "' but found '" + input.Substring(pos, lit.Length) + "'"));
        }
        
        public static Parser<T> Build<T>(Func<Parser<T>, Parser<T>> define)
        {
            return new Parser<T>(define);
        }
        
        static bool Equals(string x, int ix, string y, int count)
        {
            for (int iy = 0; iy < count; ++iy, ++ix)
            {
                if (x[ix] != y[iy]) return false;
            }
            return true;
        }
    }
}