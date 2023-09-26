using Expression.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expression.Event
{
    public class CommonEventVariableAccessor : Common.IDataAccessor
    {
        private const int MinStringVariableId = 5;
        private const int MaxStringVariableId = 9;

        private ICommonEventCommandsRepository repository;
        private CommonEventId eventId;
        private int variableId;

        public CommonEventVariableAccessor(CommonEventId eventId, int variableId)
        {
            this.repository = DI.DependencyInjector.It().CommonEventCommandsRepository;
            this.eventId = eventId;
            this.variableId = variableId;
        }

        public int GetInt()
        {
            var eventData = repository.GetEvent(eventId);

            int intId = ToIntVariableIndex(variableId);
            if (intId >= 0 && intId < eventData.NumberVariables.Length)
            {
                return eventData.NumberVariables[intId];
            }

            // 文字列は数値化できるなら返す
            int stringId = ToStringVariableIndex(variableId);
            if (stringId != -1&&int.TryParse(eventData.StringVariables[stringId],out int res))
            {
                return res;
            }

            return 0;
        }

        public string GetString()
        {
            var eventData = repository.GetEvent(eventId);
            int stringId = ToStringVariableIndex(variableId);
            if (stringId != -1)
            {
                return eventData.StringVariables[stringId];
            }

            // 数値は文字列にして返す
            int intId = ToIntVariableIndex(variableId);
            if (intId >= 0 && intId < eventData.NumberVariables.Length)
            {
                return eventData.NumberVariables[intId].ToString();
            }

            return "";
        }

        public void SetInt(int value)
        {
            var eventData = repository.GetEvent(eventId);

            int intId = ToIntVariableIndex(variableId);
            if (intId >= 0 && intId < eventData.NumberVariables.Length)
            {
                eventData.NumberVariables[intId] = value;
            }

            // 文字列には数値を文字列化して代入
            int stringId = ToStringVariableIndex(variableId);
            if (stringId != -1)
            {
                eventData.StringVariables[stringId] = value.ToString();
            }
        }

        public void SetString(string value)
        {
            var eventData = repository.GetEvent(eventId);
            int stringId = ToStringVariableIndex(variableId);
            if (stringId != -1)
            {
                eventData.StringVariables[stringId] = value;
            }

            int intId = ToIntVariableIndex(variableId);
            if (intId >= 0 && intId < eventData.NumberVariables.Length)
            {
                if (int.TryParse(value,out int res))
                {
                    eventData.NumberVariables[intId] = res;
                }
            }
        }

        public bool TestType(VariableType targetType)
        {
            int id = ToIntVariableIndex(variableId);
            if (id == -1)
            {
                return targetType == VariableType.String;
            }
            else
            {
                return targetType == VariableType.Number;
            }
        }

        private int ToIntVariableIndex(int variableId)
        {
            // 【暫定】本来はVariableIdクラスを作り、その中に隠蔽すべきロジック

            if (variableId < 0 || (MinStringVariableId <= variableId && variableId <= MaxStringVariableId))
            {
                // 文字列は無効な値を返す
                return -1;
            }

            if (variableId <= 4)
            {
                return variableId;
            }
            else
            {
                return variableId - 5;
            }
        }

        private int ToStringVariableIndex(int variableId)
        {
            // 【暫定】本来はVariableIdクラスを作り、その中に隠蔽すべきロジック
            if (MinStringVariableId <= variableId && variableId <= MaxStringVariableId)
            {
                // 文字列ならインデックスを返す
                return variableId - MinStringVariableId;
            }

            return -1;
        }
    }
}
