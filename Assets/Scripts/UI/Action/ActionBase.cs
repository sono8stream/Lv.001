
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

        /// <summary>
        /// 自分自身が指定されたラベルと対応しているかを判定
        /// </summary>
        /// <param name="label">チェックしたいラベル</param>
        /// <returns></returns>
        public virtual bool VerifyLabel(ActionLabel label)
        {
            return false;
        }
    }
}
