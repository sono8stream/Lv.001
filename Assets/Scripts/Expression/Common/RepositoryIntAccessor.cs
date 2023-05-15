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
            return field != null ? field.Val : 0;
        }

        public void Set(int value)
        {
            repository.SetInt(dataRef, value);
        }
    }
}
