using Util.Wolf;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Expression.Map.MapEvent.CommandFactory
{
    public class StringFactory
    {
        private List<Common.IDataAccessorFactory<string>> factories;

        public StringFactory(string text)
        {
            factories = CreateFactories(text);
        }

        List<Common.IDataAccessorFactory<string>> CreateFactories(string text)
        {
            var factories = new List<Common.IDataAccessorFactory<string>>();
            string constStr = "";

            // ���ꕶ�������o��
            // �y�b��z�Z���t�ϐ��Ăяo���ɂ����Ή����Ă��Ȃ��̂ŁA�C��
            var matches = Regex.Matches(text, @"\\c?self\[[0-9]+\]");
            int matchIndex = 0;

            for (int i = 0; i < text.Length; i++)
            {
                if (matchIndex < matches.Count && matches[matchIndex].Index == i)
                {
                    if (constStr.Length > 0)
                    {
                        factories.Add(
                            new Command.WolfStringAccessorFactory(true, constStr));
                        constStr = "";
                    }

                    // ���ꕶ���̏������s��
                    factories.Add(new Command.WolfStringAccessorFactory(
                        false, matches[matchIndex].Value));
                    i += matches[matchIndex].Value.Length - 1;
                    matchIndex++;
                }
                else
                {
                    constStr += text[i];
                }
            }
            if (constStr.Length > 0)
            {
                factories.Add(new Command.WolfStringAccessorFactory(
                    true, constStr));
            }
            return factories;
        }

        public string GenerateMessage(CommandVisitContext context)
        {
            string message = "";
            for (int i = 0; i < factories.Count; i++)
            {
                message += factories[i].Create(context).Get();
            }

            return message;
        }
    }
}