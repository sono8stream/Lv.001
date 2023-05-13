
namespace UI.Action
{
    /// <summary>
    /// イベント内で実行するアクションのベースクラス
    /// </summary>
    public class ActionBase
    {
        /// <summary>
        /// 開始時に読み込む処理
        /// </summary>
        public virtual void OnStart()
        {

        }

        /// <summary>
        /// 処理本体
        /// </summary>
        /// <returns>イベント実行終了したか</returns>
        public virtual bool Run()
        {
            return true;
        }

        /// <summary>
        /// 終了時に読み込む処理
        /// </summary>
        /// <returns></returns>
        public virtual void OnEnd()
        {

        }
    }
}
