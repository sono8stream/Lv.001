using Expression.Common;
using System;

namespace Expression.Event
{
    public class CommonEventStringAccessor : Common.IDataAccessor<string>
    {
        private ICommonEventCommandsRepository repository;
        private CommonEventId eventId;
        private int variableId;

        private int minStringVariableId = 5;
        private int maxStringVariableId = 9;

        public CommonEventStringAccessor(CommonEventId eventId, int variableId)
        {
            this.repository = DI.DependencyInjector.It().CommonEventCommandsRepository;
            this.eventId = eventId;
            this.variableId = variableId;
        }

        public string Get()
        {
            var eventData = repository.GetEvent(eventId);
            // 【暫定】実際にはIDが5~9のもののみが文字列型になる。呼び出し側が数値化文字列化を区別しなくて済むよう、レコード側に複数の取り出し方を実装する
            if (variableId < minStringVariableId || variableId > maxStringVariableId)
            {
                int numberVariableId = variableId > maxStringVariableId ? variableId - (maxStringVariableId - minStringVariableId + 1) : variableId;
                return eventData.NumberVariables[numberVariableId].ToString();
            }
            else
            {
                int stringVariableId = variableId - minStringVariableId;
                return eventData.StringVariables[stringVariableId];
            }
        }

        public void Set(string value)
        {
            throw new NotImplementedException();
        }

        public bool TestType(VariableType targetType)
        {
            int id = ToStringVariableIndex(variableId);
            if (id == -1)
            {
                return targetType == VariableType.String;
            }
            else
            {
                return targetType == VariableType.Number;
            }
        }

        private int ToStringVariableIndex(int variableId)
        {
            // 【暫定】本来はVariableIdクラスを作り、その中に隠蔽すべきロジック
            if (minStringVariableId <= variableId && variableId <= maxStringVariableId)
            {
                // 文字列ならインデックスを返す
                return variableId - minStringVariableId;
            }

            return -1;
        }
    }
}
