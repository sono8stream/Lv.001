using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Expression.Map.MapEvent.Command;

namespace UI.Action
{
    class ShowPictureAction : ActionBase
    {
        Texture2D texture;

        PicturePosPattern posPattern;
        float width, height;
        Vector2 pivot;

        ActionEnvironment actionEnv;

        Transform imageBox;

        public ShowPictureAction(Texture2D texture, ActionEnvironment actionEnv,
            PicturePosPattern posPattern, float width, float height,
            float pivotX, float pivotY)
        {
            this.texture = texture;
            this.posPattern = posPattern;
            this.width = width;
            this.height = height;
            this.pivot = new Vector2(pivotX, pivotY);

            this.actionEnv = actionEnv;
            imageBox = actionEnv.canvas.transform.Find("Image Box");
        }

        /// <inheritdoc/>
        public override bool Run()
        {
            GameObject image = new GameObject("image");

            // 親子関係
            image.transform.SetParent(imageBox, false);

            // 位置・回転・スケール・アンカーなど
            RectTransform rectTransform = image.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(texture.width, texture.height);
            rectTransform.localScale = Vector3.one;
            rectTransform.localPosition = Vector3.zero;

            // スプライト変更
            Image img = image.AddComponent<Image>();
            img.sprite = Sprite.Create(texture,
                new Rect(0, 0, texture.width, texture.height),
                pivot);
            img.rectTransform.position = GetPos();

            return true;
        }

        private Vector2 GetPos()
        {
            float x = 0;
            float y = 0;
            Vector2 canvasSize = actionEnv.canvas.GetComponent<RectTransform>().sizeDelta;
            switch (posPattern)
            {
                case PicturePosPattern.LeftTop:
                    x = 0;
                    y = 0;
                    break;
                case PicturePosPattern.LeftBottom:
                    x = 0;
                    y = canvasSize.y;
                    break;
                case PicturePosPattern.RightTop:
                    x = canvasSize.x;
                    y = 0;
                    break;
                case PicturePosPattern.RightBottom:
                    x = canvasSize.x;
                    y = canvasSize.y;
                    break;
                default:
                    x = 0;
                    y = 0;
                    break;
            }

            return new Vector2(x, y);
        }
    }
}
