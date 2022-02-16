using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Action
{
    class ShowMessageAction : ActionBase
    {
        bool isCompleted;
        string message;

        float x, y, width, height;

        EventCommands commands;

        public ShowMessageAction(string message, EventCommands commands,
            float x = 0, float y = -690, float width = 1060, float height = 500)
        {
            isCompleted = false;
            this.message = message;
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;

            this.commands = commands;
        }

        /// <inheritdoc/>
        public override bool Run()
        {
            isCompleted = true;
            Debug.Log(message);
            if (message.Equals(""))
            {
                return true;
            }
            GameObject canvas = commands.canvas;
            List<GameObject> windows = commands.windows;

            GameObject messageBox = canvas.transform.Find("Message Box").gameObject;
            windows.Add(UnityEngine.Object.Instantiate(messageBox));
            GameObject win = windows[windows.Count - 1];
            GameObject text = win.transform.Find("Text").gameObject;
            text.GetComponent<Text>().text = message;
            win.transform.SetParent(canvas.transform);
            win.GetComponent<RectTransform>().localScale = Vector3.one;
            win.SetActive(true);
            win.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
            win.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
            text.GetComponent<RectTransform>().sizeDelta = new Vector2(width - 100, height - 50);
            text.SetActive(true);

            // 【暫定】isCompletedをEventCommandsから切り離す
            commands.IsCompleted = true;
            return true;
        }
    }
}
