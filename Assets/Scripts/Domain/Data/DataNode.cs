using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Data
{
    /// <summary>
    /// 特定の変数を格納する
    /// </summary>
    /// <typeparam name="T">変数として管理する型</typeparam>
    public class DataNode<T>
    {
        public T Val { get; private set; }

        public NodeId Id { get; private set; }

        public DataNode(T value, NodeId id)
        {
            this.Val = value;
            this.Id = id;
        }
    }

}
