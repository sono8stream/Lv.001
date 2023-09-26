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
    public interface IDataAccessor
    {
        public int GetInt();

        public string GetString();

        public void SetInt(int value);

        public void SetString(string value);

        public bool TestType(VariableType targetType);
    }

    public enum VariableType
    {
        Number,
        String,
    }
}
