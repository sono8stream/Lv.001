using System.Collections.Generic;
using Expression.Common;
using Expression.Event;
using Expression.Map.MapEvent;
using UnityEngine;

namespace UI.Action
{
    /// <summary>
    /// 1つのコモンイベント全体を保持するアクション
    /// アクションを複数個まとめ、さらに分岐などを制御できる
    /// Compositeパターンのようにふるまう
    /// </summary>
    public class CommonEventAction : EventActionBase
    {
        private CommonEventId prevId, currentId;

        // 【暫定】文字列の戻り値渡しも後ほど対応
        private bool hasReturnValue;
        private IDataAccessorFactory<int> returnDestinationAccessorFactory;
        private IDataAccessorFactory<int> returnValueAccessorFactory;

        public CommonEventAction(CommonEventId eventId,
            EventCommandBase[] commands,
            ActionEnvironment actionEnv,
            CommandVisitContext context,
            bool hasReturnValue,
            IDataAccessorFactory<int> returnDestinationAccessorFactory,
            IDataAccessorFactory<int> returnValueAccessorFactory) : base(commands, actionEnv, context)
        {
            currentId = eventId;
            this.hasReturnValue = hasReturnValue;
            this.returnDestinationAccessorFactory = returnDestinationAccessorFactory;
            this.returnValueAccessorFactory = returnValueAccessorFactory;
        }

        /// <inheritdoc/>
        public override void OnStart()
        {
            prevId = context.CommonEventId;
            context.CommonEventId = currentId;

            base.OnStart();
        }

        /// <inheritdoc/>
        public override void OnEnd()
        {
            base.OnEnd();

            context.CommonEventId = prevId;

            // 戻り値を返す
            if (hasReturnValue)
            {
                var updater = new UpdaterInt(returnDestinationAccessorFactory,
                    returnValueAccessorFactory,
                    null,
                    OperatorType.NormalAssign,
                    OperatorType.Plus
                    );
                updater.Update(context);
            }
        }
    }
}
