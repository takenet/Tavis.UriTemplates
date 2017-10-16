using System.Text;

namespace Tavis.UriTemplates
{
    public class VarSpec
    {
        public bool Explode = false;
        public bool First = true;
        public int PrefixLength = 0;
        public StringBuilder VarName = new StringBuilder();

        public VarSpec(OperatorInfo operatorInfo)
        {
            OperatorInfo = operatorInfo;
        }

        public OperatorInfo OperatorInfo { get; }

        public override string ToString()
        {
            return (First ? OperatorInfo.First : "") +
                   VarName
                   + (Explode ? "*" : "")
                   + (PrefixLength > 0 ? ":" + PrefixLength : "");
        }
    }
}