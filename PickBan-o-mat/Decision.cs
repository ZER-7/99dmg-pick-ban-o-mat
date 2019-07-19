using System.Collections.Generic;

namespace PickBan_o_mat
{
    internal class Decision
    {
        public bool GoingFirst;
        public Map Map;
        public List<Map> Pool;
        public DecisionType Type;
    }

    internal enum DecisionType
    {
        Pick,
        Ban
    }
}