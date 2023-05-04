using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace UI.Action
{
    /// <summary>
    /// アクションを実行します
    /// 【暫定】アクションを実行する仕組みとオブジェクト情報の管理の両方が行われているので切り離す
    /// </summary>
    public class ActionProcessor : MonoBehaviour
    {
        [SerializeField]
        string[] scripts;
        int line;//現在読んでいるスクリプトの行数
        ActionEnvironment actionEnvironment;// イベントを実行するためのコマンドを保持
        List<UnityEvent> events;
        UI.Action.ActionBase currentAction;

        public static bool isProcessing = false;

        // Use this for initialization
        void Start()
        {
            line = 0;
            actionEnvironment = GameObject.Find("ActionEnvironment").GetComponent<ActionEnvironment>();
            events = new List<UnityEvent>();
            actionEnvironment.actNo = 0;
            actionEnvironment.IsCompleted = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (isProcessing && currentAction != null)
            {
                if (currentAction.Run())
                {
                    isProcessing = false;
                    currentAction = null;
                }
            }
        }

        public void StartActions(Map.EventObject eventObject)
        {
            isProcessing = true;
            Expression.Map.MapEvent.CommandVisitContext context
                = new Expression.Map.MapEvent.CommandVisitContext(actionEnvironment.Map.MapId, eventObject.EventData.Id);
            Map.EventActionFactory factory = new Map.EventActionFactory(actionEnvironment, context);
            currentAction = factory.CreateActionFrom(eventObject.EventData.PageData[0].CommandDataArray);
            currentAction.OnStart();
        }
    }
}
