using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserCombinators
{
    public struct Result<T>
    {
        public Result(T value, int pos) : this()
        {
            Value = value;
            Pos = pos;
        }
        public Result(int pos, string error) : this()
        {
            Error = error;
            Pos = pos;
        }
        public T Value { get; internal set; }
        public string Error { get; internal set; }
        public int Pos { get; internal set; }
        public bool Failed
        {
            get { return Error != null; }
        }
        public bool TryGetValue(out T value, out int pos)
        {
            value = Value;
            pos = Pos;
            return Error == null;
        }
    }
}
