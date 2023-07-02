using System.Collections.Generic;
using Expression.Map.MapEvent;

namespace UI.Action
{
    /// <summary>
    /// 1つのイベント全体を保持するアクション
    /// アクションを複数個まとめ、さらに分岐などを制御できる
    /// Compositeパターンのようにふるまう
    /// </summary>
    public class EventAction : ActionBase
    {
        EventCommandBase[] commands;

        // 現在アクションの保持用。制御中にジャンプしても参照を保持できる
        ActionBase currentAction;

        ActionControl control;
        Map.CommandActionFactory actionFactory;
        CommandVisitContext context;

        public EventAction(EventCommandBase[] commands,
            ActionEnvironment actionEnv,
            CommandVisitContext context)
        {
            this.commands = commands;
            this.control = new ActionControl();
            this.actionFactory = new Map.CommandActionFactory(actionEnv, context,control);
            this.context = context;
        }

        /// <inheritdoc/>
        public override void OnStart()
        {
            control.Initialize();
            TryToStartCurrentAction();
        }

        /// <inheritdoc/>
        public override bool Run()
        {
            while (currentAction != null && currentAction.Run())
            {
                currentAction.OnEnd();

                control.TransitToNext(commands);

                TryToStartCurrentAction();
            }

            if (currentAction == null)
            {
                // 実行できるアクションがないので終了とする
                return true;
            }
            else
            {
                // 実行できるアクションがあるので終了しない
                return false;
            }
        }

        private void TryToStartCurrentAction()
        {
            currentAction = null;
            if (control.CurrentActNo < commands.Length)
            {
                commands[control.CurrentActNo].Visit(actionFactory);
                currentAction = actionFactory.GeneratedAction;
                currentAction.OnStart();
            }
        }
    }
}
