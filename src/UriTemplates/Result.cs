using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tavis.UriTemplates
{
    public class Result
    {
        private const string UriReservedSymbols = ":/?#[]@!$&'()*+,;=";
        private const string UriUnreservedSymbols = "-._~";

        private static readonly char[] HexDigits =
            {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};

        private readonly StringBuilder _result = new StringBuilder();

        public Result()
        {
            ParameterNames = new List<string>();
        }

        public bool ErrorDetected { get; set; }
        public List<string> ParameterNames { get; set; }

        public StringBuilder Append(char value)
        {
            return _result.Append(value);
        }

        public StringBuilder Append(string value)
        {
            return _result.Append(value);
        }

        public override string ToString()
        {
            return _result.ToString();
        }

        public void AppendName(string variable, OperatorInfo op, bool valueIsEmpty)
        {
            _result.Append(variable);
            if (valueIsEmpty)
            {
                _result.Append(op.IfEmpty);
            }
            else
            {
                _result.Append("=");
            }
        }


        public void AppendList(OperatorInfo op, bool explode, string variable, IList list)
        {
            foreach (var item in list)
            {
                if (op.Named && explode)
                {
                    _result.Append(variable);
                    _result.Append("=");
                }
                AppendValue(item.ToString(), 0, op.AllowReserved);

                _result.Append(explode ? op.Seperator : ',');
            }
            if (list.Count > 0)
            {
                _result.Remove(_result.Length - 1, 1);
            }
        }

        public void AppendDictionary(OperatorInfo op, bool explode, IDictionary<string, string> dictionary)
        {
            foreach (var key in dictionary.Keys)
            {
                _result.Append(Encode(key, op.AllowReserved));
                if (explode)
                {
                    _result.Append('=');
                }
                else
                {
                    _result.Append(',');
                }

                AppendValue(dictionary[key], 0, op.AllowReserved);

                if (explode)
                {
                    _result.Append(op.Seperator);
                }
                else
                {
                    _result.Append(',');
                }
            }
            if (dictionary.Any())
            {
                _result.Remove(_result.Length - 1, 1);
            }
        }

        public void AppendValue(string value, int prefixLength, bool allowReserved)
        {
            if (prefixLength != 0)
            {
                if (prefixLength < value.Length)
                {
                    value = value.Substring(0, prefixLength);
                }
            }

            _result.Append(Encode(value, allowReserved));
        }


        private static string Encode(string p, bool allowReserved)
        {
            var result = new StringBuilder();
            foreach (var c in p)
            {
                if (c >= 'A' && c <= 'z' //Alpha
                    || c >= '0' && c <= '9' // Digit
                    || UriUnreservedSymbols.IndexOf(c) !=
                    -1 // Unreserved symbols  - These should never be percent encoded
                    || allowReserved && UriReservedSymbols.IndexOf(c) != -1
                ) // Reserved symbols - should be included if requested (+)
                {
                    result.Append(c);
                }
                else
                {
                    var bytes = Encoding.UTF8.GetBytes(new[] {c});
                    foreach (var abyte in bytes)
                    {
                        result.Append(HexEscape(abyte));
                    }
                }
            }

            return result.ToString();
        }

        public static string HexEscape(byte i)
        {
            var esc = new char[3];
            esc[0] = '%';
            esc[1] = HexDigits[(i & 240) >> 4];
            esc[2] = HexDigits[i & 15];
            return new string(esc);
        }

        public static string HexEscape(char c)
        {
            var esc = new char[3];
            esc[0] = '%';
            esc[1] = HexDigits[(c & 240) >> 4];
            esc[2] = HexDigits[c & 15];
            return new string(esc);
        }
    }
}