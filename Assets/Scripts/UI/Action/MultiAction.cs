using System.Collections.Generic;

namespace UI.Action
{
    /// <summary>
    /// アクションを複数個まとめたアクション
    /// Compositeパターンのようにふるまう
    /// </summary>
    public class MultiAction : ActionBase
    {
        List<ActionBase> actions;
        int currentActNo;
        // 現在アクションの保持用。制御中にジャンプしても参照を保持できる
        ActionBase currentAction;

        ActionControlInfo controlInfo;

        public MultiAction(List<ActionBase> actions)
        {
            this.actions = actions;
            controlInfo = new ActionControlInfo();
        }

        /// <inheritdoc/>
        public override void OnStart()
        {
            currentActNo = 0;

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

                TransitToNext();

                TryToStartCurrentAction();
            }

            return false;
        }

        private void TryToStartCurrentAction()
        {
            currentAction = null;
            if (currentActNo < actions.Count)
            {
                currentAction = actions[currentActNo];
                currentAction.OnStart();
            }
        }

        /// <summary>
        /// 次のアクションまで遷移させる
        /// </summary>
        void TransitToNext()
        {
            if (controlInfo.IsSkipMode)
            {
                // スキップ要求があるのでラベルまでスキップさせる
                for (int i = 0; i < actions.Count; i++)
                {
                    if (actions[i].VerifyLabel(controlInfo.SkipLabel))
                    {
                        currentActNo = i;
                        break;
                    }
                }
            }
            else
            {
                currentActNo++;
            }
        }
    }
}
