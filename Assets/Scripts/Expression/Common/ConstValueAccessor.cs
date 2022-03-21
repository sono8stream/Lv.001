using Domain.Data;
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
    class ConstValueAccessor : IDataAccessor<int>
    {
        private int value;

        public ConstValueAccessor(int val)
        {
            value = val;
        }


        public int Access()
        {
            return value;
        }
    }
}
