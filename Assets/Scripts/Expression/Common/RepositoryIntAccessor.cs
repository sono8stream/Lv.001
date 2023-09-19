using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expression.Common
{
    public class RepositoryIntAccessor : IDataAccessor<int>
    {
        private Infrastructure.IDataRepository repository;
        private Domain.Data.DataRef dataRef;

        public RepositoryIntAccessor(Infrastructure.IDataRepository repository, Domain.Data.DataRef dataRef)
        {
            this.repository = repository;
            this.dataRef = dataRef;
        }

        public int Get()
        {
            Domain.Data.DataField<int> field = repository.FindInt(dataRef);
            return field == null ? 0 : field.Val;
        }

        public void Set(int value)
        {
            repository.SetInt(dataRef, value);
        }

        public bool TestType(VariableType targetType)
        {
            Domain.Data.DataField<int> intField = repository.FindInt(dataRef);
            if (intField != null && targetType == VariableType.Number)
            {
                return true;
            }

            Domain.Data.DataField<string> stringField = repository.FindString(dataRef);
            if (stringField != null && targetType == VariableType.String)
            {
                return true;
            }

            return false;
        }
    }
}
