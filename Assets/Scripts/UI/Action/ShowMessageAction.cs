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

        ActionEnvironment actionEnv;

        public ShowMessageAction(string message, ActionEnvironment actionEnv,
            float x = 0, float y = -506, float width = 1680, float height = 400)
        {
            isCompleted = false;
            this.message = message;
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;

            this.actionEnv = actionEnv;
        }

        /// <inheritdoc/>
        public override bool Run()
        {
            isCompleted = true;
            if (message.Equals(""))
            {
                return true;
            }
            GameObject canvas = actionEnv.canvas;
            List<GameObject> windows = actionEnv.windows;

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

            return true;
        }
    }
}
