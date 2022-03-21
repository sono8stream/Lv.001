using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expression.Common
{
    /// <summary>
    /// 特定のデータを取得するためのインターフェース
    /// </summary>
    interface IDataAccessor<T>
    {
        public T Access();
    }

}
