using UnityEngine;
using UnityEngine.UI;
using Expression.Map.MapEvent.Command;

namespace UI.Action
{
    class ShowMessageAsPictureAction : ActionBase
    {
        int pictureId;
        string message;

        PicturePivotPattern pivotPattern;
        Vector2 pos;

        ActionEnvironment actionEnv;

        Transform imageBox;
        Transform textOrigin;

        public ShowMessageAsPictureAction(int pictureId, string message, ActionEnvironment actionEnv,
            PicturePivotPattern pivotPattern, float x, float y)
        {
            this.pictureId = pictureId;
            this.message = message;
            this.pivotPattern = pivotPattern;
            pos = new Vector2(x, y);

            this.actionEnv = actionEnv;
            imageBox = actionEnv.canvas.transform.Find("Image Box");
            textOrigin = actionEnv.canvas.transform.Find("Text Origin");
        }

        /// <inheritdoc/>
        public override bool Run()
        {
            // Canvasに非アクティブで置いているオブジェクトをコピーして使用する
            GameObject textObject = Object.Instantiate(textOrigin).gameObject;

            // 親子関係
            textObject.transform.SetParent(imageBox, false);

            // 位置・回転・スケール・アンカーなど
            Vector2 canvasSize = actionEnv.canvas.GetComponent<RectTransform>().sizeDelta;
            RectTransform rectTransform = textObject.gameObject.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(canvasSize.x, canvasSize.y);
            rectTransform.pivot = GetPivot();
            rectTransform.localScale = Vector3.one;
            rectTransform.localPosition = GetPos();

            // スプライト変更
            Text text = textObject.AddComponent<Text>();
            text.text = message;

            actionEnv.RegisterPicture(pictureId, textObject);

            textObject.SetActive(true);

            return true;
        }

        private Vector2 GetPivot()
        {
            float x = 0;
            float y = 0;
            Vector2 canvasSize = actionEnv.canvas.GetComponent<RectTransform>().sizeDelta;
            switch (pivotPattern)
            {
                case PicturePivotPattern.LeftTop:
                    x = 0;
                    y = 1;
                    break;
                case PicturePivotPattern.LeftBottom:
                    x = 0;
                    y = 0;
                    break;
                case PicturePivotPattern.RightTop:
                    x = 1;
                    y = 1;
                    break;
                case PicturePivotPattern.RightBottom:
                    x = 1;
                    y = 0;
                    break;
                default:
                    x = 0;
                    y = 1;
                    break;
            }

            return new Vector2(x, y);
        }

        private Vector2 GetPos()
        {
            Vector2 canvasSize = actionEnv.canvas.GetComponent<RectTransform>().sizeDelta;
            float xLeft = -canvasSize.x * 0.5f;
            float yTop = canvasSize.y * 0.5f;

            return new Vector2(xLeft + pos.x, yTop - pos.y);
        }
    }
}
