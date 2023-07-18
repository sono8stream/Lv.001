﻿using System.Collections.Generic;
using Expression.Event;
using Expression.Map.MapEvent;
using UnityEngine;

namespace UI.Action
{
    /// <summary>
    /// 1つのイベント全体を保持するアクション
    /// アクションを複数個まとめ、さらに分岐などを制御できる
    /// Compositeパターンのようにふるまう
    /// </summary>
    public class EventActionBase : ActionBase
    {
        EventCommandBase[] commands;

        // 現在アクションの保持用。制御中にジャンプしても参照を保持できる
        ActionBase currentAction;

        ActionControl control;
        Map.CommandActionFactory actionFactory;
        protected CommandVisitContext context;

        CommonEventId parentId;

        public EventActionBase(EventCommandBase[] commands,
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
            try
            {
                int perFrame = 100;
                for (int i = 0; i < perFrame; i++)
                {
                    if (currentAction != null && currentAction.Run())
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
                }
                // 実行できるアクションがあるので終了しない
                return false;
            }
            catch (System.Exception e)
            {
                throw e;
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
                Debug.Log($"Start script of MapId: {context.MapId.Value}, EventId: {context.EventId.Value}, CommonId: {context.CommonEventId?.Value}, Line: {control.CurrentActNo}");
            }
        }
    }
}