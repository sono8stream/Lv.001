
namespace Expression.Common
{
    /// <summary>
    /// データから文字列情報を取得するためのインターフェース
    /// </summary>
    public class StringVariableStringGenerator : IStringGenerator
    {
        private IDataAccessor<string> accessor;

        public StringVariableStringGenerator(IDataAccessor<string> accessor)
        {
            this.accessor = accessor;
        }
        
        public string GetString()
        {
            return accessor.Get();
        }
    }
}
