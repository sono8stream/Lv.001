using Expression.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expression.Event
{
    public class CommonEventIntAccessor : Common.IDataAccessor<int>
    {
        private ICommonEventCommandsRepository repository;
        private CommonEventId eventId;
        private int variableId;

        public CommonEventIntAccessor(CommonEventId eventId, int variableId)
        {
            this.repository = DI.DependencyInjector.It().CommonEventCommandsRepository;
            this.eventId = eventId;
            this.variableId = variableId;
        }

        public int Get()
        {
            var eventData = repository.GetEvent(eventId);

            int id = ToIntVariableIndex(variableId);
            if (id == -1)
            {
                // 文字列変数は無効
                return 0;
            }

            if (id < eventData.NumberVariables.Length)
            {
                return eventData.NumberVariables[id];
            }

            return 0;
        }

        public void Set(int value)
        {
            var eventData = repository.GetEvent(eventId);

            int id = ToIntVariableIndex(variableId);
            if (id == -1)
            {
                // 文字列変数は無効
                return;
            }

            if (id < eventData.NumberVariables.Length)
            {
                eventData.NumberVariables[id] = value;
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

            if (5 <= variableId && variableId <= 9)
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
    }
}
