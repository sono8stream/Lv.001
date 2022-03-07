
namespace UI.Action
{
    /// <summary>
    /// アクションに割り付けられたIDを保持する
    /// </summary>
    public class ActionLabel
    {
        // 【暫定】とりあえず文字列で識別可能にする
        public string LabelName { get; private set; }

        public ActionLabel(string name)
        {
            LabelName = name;
        }
    }
}
