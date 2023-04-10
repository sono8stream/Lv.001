
namespace Expression.Common
{
    /// <summary>
    /// データから文字列情報を取得するためのインターフェース
    /// </summary>
    public class ConstStringGenerator : IStringGenerator
    {
        string str;

        public ConstStringGenerator(string str)
        {
            this.str = str;
        }
        
        public string GetString()
        {
            return str;
        }
    }
}
