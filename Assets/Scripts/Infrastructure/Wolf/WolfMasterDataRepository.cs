using Domain.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure
{
    /// <summary>
    /// WolfRPGのシステム変数を読み出すためのリポジトリ
    /// </summary>
    public class WolfMasterDataRepository : IMasterDataRepository
    {
        private WolfDataRepositoryImpl impl;

        public WolfMasterDataRepository()
        {
            impl = new WolfDataRepositoryImpl(WolfConfig.DatabaseType.User);
        }

        public DataField<int> FindInt(DataRef dataRef)
        {
            return impl.FindInt(dataRef);
        }

        public void SetInt(DataRef dataRef, int value)
        {
            impl.SetInt(dataRef, value);
        }

        public DataField<string> FindString(DataRef dataRef)
        {
            return impl.FindString(dataRef);
        }

        public void SetString(DataRef dataRef, string value)
        {
            impl.SetString(dataRef, value);
        }
    }
}
