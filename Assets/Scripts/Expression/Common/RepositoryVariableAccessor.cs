using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expression.Common
{
    public class RepositoryVariableAccessor : IDataAccessor
    {
        private Infrastructure.IDataRepository repository;
        private Domain.Data.DataRef dataRef;

        public RepositoryVariableAccessor(Infrastructure.IDataRepository repository, Domain.Data.DataRef dataRef)
        {
            this.repository = repository;
            this.dataRef = dataRef;
        }

        public int GetInt()
        {
            Domain.Data.DataField<int> intField = repository.FindInt(dataRef);
            if (intField != null)
            {
                return intField.Val;
            }

            Domain.Data.DataField<string> strField = repository.FindString(dataRef);
            if (strField != null && int.TryParse(strField.Val, out int res))
            {
                return res;
            }

            return 0;
        }

        public string GetString()
        {
            Domain.Data.DataField<int> intField = repository.FindInt(dataRef);
            if (intField != null)
            {
                return intField.Val.ToString();
            }

            Domain.Data.DataField<string> strField = repository.FindString(dataRef);
            if (strField != null)
            {
                return strField.Val;
            }

            return "";
        }

        public void SetInt(int value)
        {
            Domain.Data.DataField<int> intField = repository.FindInt(dataRef);
            if (intField != null)
            {
                intField.Val = value;
            }
        }

        public void SetString(string value)
        {
            Domain.Data.DataField<string> strField = repository.FindString(dataRef);
            if (strField != null)
            {
                strField.Val = value;
            }
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
