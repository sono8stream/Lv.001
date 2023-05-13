
namespace Expression.Event
{
    /// <summary>
    /// コマンドに割り付けられたIDを保持する。コマンドリストの制御用に用いる
    /// </summary>
    public class CommandLabel
    {
        // 【暫定】とりあえず文字列で識別可能にする
        public string LabelName { get; private set; }

        public CommandLabel(string name)
        {
            LabelName = name;
        }
    }
}
