using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expression.Event
{
    public class CommonEventStringAccessor : Common.IDataAccessor<string>
    {
        private ICommonEventCommandsRepository repository;
        private CommonEventId eventId;
        private int variableId;

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
            if(variableId< eventData.NumberVariables.Length)
            {
                return eventData.NumberVariables[variableId].ToString();
            }
            else
            {
                int id = variableId - eventData.NumberVariables.Length;
                return eventData.StringVariables[id];
            }
        }

        public void Set(string value)
        {
            throw new NotImplementedException();
        }
    }
}
