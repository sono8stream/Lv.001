using Domain.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure
{
    /// <summary>
    /// WolfRPGのシステムDBを読み出すためのリポジトリ
    /// </summary>
    public class WolfSystemDataRepository : ISystemDataRepository
    {
        private WolfDataRepositoryImpl impl;

        public WolfSystemDataRepository()
        {
            impl = new WolfDataRepositoryImpl(WolfConfig.DatabaseType.System);
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
