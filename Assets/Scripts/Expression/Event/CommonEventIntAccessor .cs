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
            if (variableId < eventData.NumberVariables.Length)
            {
                return eventData.NumberVariables[variableId];
            }

            // 文字列変数は無効
            return 0;
        }

        public void Set(int value)
        {
            var eventData = repository.GetEvent(eventId);
            if (variableId < eventData.NumberVariables.Length)
            {
                eventData.NumberVariables[variableId] = value;
            }
        }
    }
}
