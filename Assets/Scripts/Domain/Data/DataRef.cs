using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Data
{
    /// <summary>
    /// 特定のデータにアクセスするための情報を保持するクラス
    /// </summary>
    public class DataRef
    {
        private CategoryId categoryId;
        private NodeId nodeId;

        public DataRef(CategoryId categoryId, NodeId nodeId)
        {
            this.categoryId = categoryId;
            this.nodeId = nodeId;
        }
    }
}
