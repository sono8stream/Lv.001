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
        int pictureId;
        Texture2D texture;

        PicturePivotPattern pivotPattern;
        float scale;
        Vector2 pos;

        ActionEnvironment actionEnv;

        Transform imageBox;

        public ShowPictureAction(int pictureId,Texture2D texture, ActionEnvironment actionEnv,
            PicturePivotPattern pivotPattern, float x, float y, float scale)
        {
            this.pictureId = pictureId;
            this.texture = texture;
            this.pivotPattern = pivotPattern;
            this.scale = scale;
            this.pos = new Vector2(x, y);

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
            rectTransform.pivot = GetPivot();
            rectTransform.localScale = Vector3.one * 6 * scale;
            rectTransform.localPosition = GetPos();

            // スプライト変更
            Image img = image.AddComponent<Image>();
            img.sprite = Sprite.Create(texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));

            actionEnv.RegisterImage(pictureId, img);

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
