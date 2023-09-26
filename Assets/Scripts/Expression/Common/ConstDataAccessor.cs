using System;

namespace Expression.Common
{
    /// <summary>
    /// 定数を取得する
    /// </summary>
    public class ConstDataAccessor : IDataAccessor
    {
        private int intVal;
        private string strVal;
        private bool isNumber;

        public ConstDataAccessor(int intVal)
        {
            this.intVal = intVal;
            this.isNumber = true;
        }

        public ConstDataAccessor(string strVal)
        {
            this.strVal = strVal;
            this.isNumber = false;
        }

        public int GetInt()
        {
            if (isNumber)
            {
                return intVal;
            }
            else
            {
                if (int.TryParse(strVal, out int res))
                {
                    return res;
                }
                return 0;
            }
        }

        public string GetString()
        {
            if (isNumber)
            {
                return intVal.ToString();
            }
            else
            {
                return strVal;
            }
        }

        public void SetInt(int value)
        {
            // 代入不可なので何もしない
            return;
        }

        public void SetString(string value)
        {
            // 代入不可なので何もしない
            return;
        }

        public bool TestType(VariableType targetType)
        {
            if(targetType== VariableType.Number)
            {
                return isNumber;
            }
            else if (targetType == VariableType.String)
            {
                return !isNumber;
            }

            throw new Exception("想定外の型");
        }
    }
}
