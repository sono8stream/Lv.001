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

        public T Get()
        {
            return val;
        }

        public void Set(T value)
        {
            // 再代入不可なので何もしない
            return;
        }
    }

}
