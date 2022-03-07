using System.Collections.Generic;

namespace UI.Action
{
    /// <summary>
    /// 1つのイベント全体を保持するアクション
    /// アクションを複数個まとめ、さらに分岐などを制御できる
    /// Compositeパターンのようにふるまう
    /// </summary>
    public class EventAction : ActionBase
    {
        List<ActionBase> actions;
        // 現在アクションの保持用。制御中にジャンプしても参照を保持できる
        ActionBase currentAction;

        ActionControl control;

        public EventAction(List<ActionBase> actions, ActionControl control)
        {
            this.actions = actions;
            this.control = control;
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
            if (currentAction == null)
            {
                return true;
            }

            if (currentAction.Run())
            {
                currentAction.OnEnd();

                control.TransitToNext(actions);

                TryToStartCurrentAction();
            }

            return false;
        }

        private void TryToStartCurrentAction()
        {
            currentAction = null;
            if (control.CurrentActNo < actions.Count)
            {
                currentAction = actions[control.CurrentActNo];
                currentAction.OnStart();
            }
        }
    }
}
