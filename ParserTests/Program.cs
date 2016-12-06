using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParserCombinators.RecursiveDescent.ObjectOriented;
//using ParserCombinators.RecursiveDescent.Functional;

namespace ParserTests
{
    class Program
    {
        static void Main(string[] args)
        {
            var num = (Parse.Literal("0") | Parse.Literal("1")).Select(int.Parse);
            var arith = Parse.Build<int>(e => num.Then(Parse.Literal("+")).Then(e).Select(x => x.Item2 + x.Item1.Item1)
                                            | num.Then(Parse.Literal("-")).Then(e).Select(x => x.Item1.Item1 - x.Item2)
                                            | num);
            var ans = arith.Parse("0+1-1+1+1", 0);
        }
    }
}
