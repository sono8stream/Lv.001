using Domain.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure
{
    /// <summary>
    /// WolfRPG�̉�DB��ǂݏo�����߂̃��|�W�g��
    /// </summary>
    public class WolfPlayDataRepository : IPlayDataRepository
    {
        public DataRecord Find(RecordId id)
        {
            return null;
        }

        public DataField<int> FindInt(DataRef dataRef)
        {
            throw new System.NotImplementedException();
        }

        public DataField<string> FindString(DataRef dataRef)
        {
            throw new System.NotImplementedException();
        }
    }
}
