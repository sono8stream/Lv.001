﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expression.Common
{
    /// <summary>
    /// 特定のデータを取得するためのインターフェース
    /// 【暫定】どのレイヤに置くべきか？固める。Infrastructureにも置けそう
    /// </summary>
    public interface IDataAccessor<T>
    {
        public T Get();

        public void Set(T value);
    }

}
