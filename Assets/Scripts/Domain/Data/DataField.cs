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
    public class DataField<T>
    {
        public T Val { get; private set; }

        public FieldId Id { get; private set; }

        public DataField(FieldId id, T value)
        {
            this.Id = id;
            this.Val = value;
        }
    }

}
