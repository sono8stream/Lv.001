
namespace Expression.Common
{
    /// <summary>
    /// データから文字列情報を取得するためのインターフェース
    /// </summary>
    public class IntVariableStringGenerator : IStringGenerator
    {
        private IDataAccessor<int> accessor;

        public IntVariableStringGenerator(IDataAccessor<int> accessor)
        {
            this.accessor = accessor;
        }
        
        public string GetString()
        {
            int num = accessor.Get();
            return num.ToString();
        }
    }
}
