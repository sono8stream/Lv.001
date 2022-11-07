using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UI.Action
{
    class ShowChoiceAction : ActionBase
    {
        int indentDepth;
        string[] choiceStrings;

        float x, y, width, height, textSizeY, margin;

        ActionEnvironment actionEnv;

        public ShowChoiceAction(int indentDepth, string[] choiceStrings, ActionEnvironment actionEnv,
            float x = 0, float y = -420)
        {
            this.indentDepth = indentDepth;
            this.choiceStrings = choiceStrings;
            this.x = x;
            this.y = y;

            textSizeY = 100;
            margin = 30;

            width = 500;
            height = textSizeY * choiceStrings.Length;

            this.actionEnv = actionEnv;

            UnityEngine.Assertions.Assert.IsNotNull(GameObject.FindObjectOfType<EventSystem>(),
                "EventSystemがシーン内で有効になっていません");
        }

        /// <inheritdoc/>
        public override bool Run()
        {
            // 【暫定】非常に長いので塊ごとに関数にまとめる

            // 選択肢ウィンドウを表示する
            GameObject choiceBoxOrigin = actionEnv.canvas.transform.Find("Choice Box").gameObject;//選択肢ウィンドウ
            GameObject choiceBox = UnityEngine.Object.Instantiate(choiceBoxOrigin);
            actionEnv.choiceBoxes.Add(choiceBox);

            choiceBox.transform.SetParent(actionEnv.canvas.transform);
            choiceBox.transform.localScale = Vector3.one;
            // 【暫定】システム変数を読み出して選択肢ウィンドウの位置を決める
            choiceBox.GetComponent<RectTransform>().localPosition 
                = new Vector2(x, y + (textSizeY / 2) * choiceStrings.Length);
            choiceBox.GetComponent<RectTransform>().sizeDelta = new Vector2(width, margin * 2 + height);
            choiceBox.SetActive(true);

            // 選択中項目を表示するビューの位置を調整
            GameObject selectBranchObject = choiceBox.transform.Find("Select Branch").gameObject;
            selectBranchObject.GetComponent<RectTransform>().sizeDelta = new Vector2(width, textSizeY);
            selectBranchObject.SetActive(false);

            // 選択肢の各要素を表示
            GameObject choiceButtonOrigin = actionEnv.canvas.transform.Find("Button Origin").gameObject;
            actionEnv.choiceButtons = new List<GameObject>();
            for (int i = 0; i < choiceStrings.Length; i++)
            {
                GameObject choiceButton = UnityEngine.Object.Instantiate(choiceButtonOrigin);
                actionEnv.choiceButtons.Add(UnityEngine.Object.Instantiate(choiceButtonOrigin));

                choiceButton.GetComponent<Text>().text = choiceStrings[i];

                choiceButton.transform.SetParent(choiceBox.transform);
                choiceButton.transform.localPosition
                    = new Vector2(0, /*actionEnv.choiceBoxes[boxNo].GetComponent<RectTransform>().sizeDelta.y / 2 - margin - textSizeY * (i + 0.5f)*/
                    height / 2 - height / choiceStrings.Length * (i + 0.5f));
                choiceButton.GetComponent<RectTransform>().sizeDelta = new Vector2(width - 50, textSizeY);
                choiceButton.transform.localScale = Vector3.one;

                // 【暫定】右詰めの文字列の内容や大きさを変える。未使用で今後使用する予定もないので廃止する
                Transform textObject = choiceButton.transform.Find("Text");
                textObject.GetComponent<Text>().text = "";
                textObject.GetComponent<RectTransform>().sizeDelta = choiceButton.GetComponent<RectTransform>().sizeDelta;

                choiceButton.SetActive(true);

                // ボタンへのイベント仕込み
                // 【暫定】本来はWaitForChoiceの責務か？そちらに分離する可能性あり
                int no = i;// ローカル変数に代入してインデックスをバインド
                choiceButton.GetComponent<Button>().onClick.AddListener(() => SetChoice(no));
                
                EventTrigger trigger = choiceButton.GetComponent<EventTrigger>();
                trigger.triggers = new List<EventTrigger.Entry>();
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerDown;    //イベントのタイプSelectが発生した際に
                Vector2 pos = choiceButton.GetComponent<RectTransform>().anchoredPosition;
                entry.callback.AddListener((x) => SetSelectBranch(pos, selectBranchObject));
                trigger.triggers.Add(entry);
            }
            actionEnv.ChoiceName = "";
            actionEnv.ChoiceNameSub = "";

            return true;
        }

        private void SetChoice(int choiceId)
        {
            // 選択肢番号1~10は分岐始点では2~11でナンバリングされる
            actionEnv.ChoiceName = $"{indentDepth}.{choiceId + 2}";
        }

        private void SetSelectBranch(Vector2 pos, GameObject branch)
        {
            branch.SetActive(true);
            branch.GetComponent<RectTransform>().anchoredPosition = pos;
            Debug.Log(pos);
        }
    }
}
