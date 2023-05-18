using Domain.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure
{
    /// <summary>
    /// WolfRPGの可変DBを読み出すためのリポジトリ
    /// </summary>
    public class WolfPlayDataRepository : IPlayDataRepository
    {
        private WolfDataRepositoryImpl impl;

        public WolfPlayDataRepository()
        {
            impl = new WolfDataRepositoryImpl("CDatabase");
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
