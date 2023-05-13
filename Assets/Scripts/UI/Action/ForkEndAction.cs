
namespace UI.Action
{
    /// <summary>
    /// 分岐始点を示すアクション
    /// </summary>
    public class ForkEndAction : ActionBase
    {
        string labelString;

        public ForkEndAction(string labelString)
        {
            this.labelString = labelString;
        }
    }
}
