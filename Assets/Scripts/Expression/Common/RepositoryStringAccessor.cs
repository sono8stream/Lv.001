using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expression.Common
{
    public class RepositoryStringAccessor : IDataAccessor<string>
    {
        private Infrastructure.IDataRepository repository;
        private Domain.Data.DataRef dataRef;

        public RepositoryStringAccessor(Infrastructure.IDataRepository repository, Domain.Data.DataRef dataRef)
        {
            this.repository = repository;
            this.dataRef = dataRef;
        }

        public string Get()
        {
            Domain.Data.DataField<string> field = repository.FindString(dataRef);
            return field != null ? field.Val : "";
        }

        public void Set(string value)
        {
            repository.SetString(dataRef, value);
        }
    }
}
