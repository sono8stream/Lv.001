using System.Collections.Generic;

namespace UI.Action
{
    /// <summary>
    /// アクションを複数個まとめたアクション
    /// Compositeパターンのようにふるまう
    /// 制御機能は持たず、あくまで一連の処理をまとめ、逐次実行するだけ
    /// </summary>
    public class MultiAction : ActionBase
    {
        List<ActionBase> actions;
        int currentActionNo;
        // 現在アクションの保持用。制御中にジャンプしても参照を保持できる
        ActionBase currentAction;

        public MultiAction(List<ActionBase> actions)
        {
            this.actions = actions;
        }

        /// <inheritdoc/>
        public override void OnStart()
        {
            currentActionNo = 0;
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

                currentActionNo++;

                TryToStartCurrentAction();
            }

            return false;
        }

        private void TryToStartCurrentAction()
        {
            currentAction = null;
            if (currentActionNo < actions.Count)
            {
                currentAction = actions[currentActionNo];
                currentAction.OnStart();
            }
        }
    }
}
