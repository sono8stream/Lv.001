using System;

namespace Expression.Common
{
    /// <summary>
    /// 定数を取得する
    /// </summary>
    public class ConstDataAccessor<T> : IDataAccessor<T>
    {
        private T val;

        public ConstDataAccessor(T val)
        {
            this.val = val;
        }

        public T Access()
        {
            return val;
        }
    }

}
