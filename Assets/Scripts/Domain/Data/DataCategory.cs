using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Data
{
    /// <summary>
    /// 関連のあるDataNodeをひとまとめにするクラス
    /// </summary>
    public class DataCategory
    {
        public CategoryId Id { get; private set; }

        public Dictionary<NodeId, DataNode<int>> IntNodes { get; private set; }

        public Dictionary<NodeId,DataNode<string>> StringNodes { get; private set; }

        public DataCategory(CategoryId id, Dictionary<NodeId, DataNode<int>> intNodes, 
            Dictionary<NodeId, DataNode<string>> stringNodes)
        {
            Id = id;
            IntNodes = intNodes;
            StringNodes = stringNodes;
        }
    }
}
