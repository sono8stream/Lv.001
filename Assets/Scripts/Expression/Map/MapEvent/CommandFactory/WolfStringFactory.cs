using Util.Wolf;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Expression.Map.MapEvent.CommandFactory
{
    public class WolfStringFactory : IStringFactory
    {
        private string[] parts;

        public WolfStringFactory(string text)
        {
            parts = CreateParts(text);
        }

        string[] CreateParts(string text)
        {
            var matches = Regex.Matches(text, BuildPattern());
            return Regex.Split(text, BuildPattern());
        }

        public string GenerateMessage(CommandVisitContext context)
        {
            string message = "";
            for (int i = 0; i < parts.Length; i++)
            {
                message += Create(parts[i], context).GetString();
            }

            return message;
        }

        private string BuildPattern()
        {
            // ���ꕶ�������o��
            // �y�b��z�ꕔ�̓��ꕶ���ɂ����Ή����Ă��Ȃ��̂ŁA�����C��
            string[] blocks = new string[] {
                @"self\[\d+]",// �}�b�v�C�x���g�̃Z���t�ϐ�
                @"cself\[\d+]",// �R�����C�x���g�̃Z���t�ϐ�
                @"udb\[\d+:\d+:\d+]",// �}�X�^�f�[�^�x�[�X
                @"cdb\[\d+:\d+:\d+]",// �σf�[�^�x�[�X
                @"sdb\[\d+:\d+:\d+]",// �V�X�e���f�[�^�x�[�X
            };
            return @"\\(" + string.Join("|", blocks) + ")";
        }

        private Common.IDataAccessor Create(string str, CommandVisitContext context)
        {
            // ���̂܂ܒl���g�p����ꍇ

            if (str.StartsWith("self["))
            {
                // ���s���̃}�b�v�C�x���g�̃Z���t�ϐ��Ăяo��
                // []�����擾
                string fieldStr = str.Substring("self[".Length, str.Length - "self[]".Length);
                if (!int.TryParse(fieldStr, out int fieldId))
                {
                    // ID�ϊ��ł��Ȃ��ꍇ�͒萔�Ƃ��ĕԂ�
                    return new Common.ConstDataAccessor(str);
                }

                var repository = DI.DependencyInjector.It().MapEventStateRpository;
                Domain.Data.DataRef dataRef = new Domain.Data.DataRef(
                    new Domain.Data.TableId(context.MapId.Value, ""),
                    new Domain.Data.RecordId(context.EventId.Value, ""),
                    new Domain.Data.FieldId(fieldId, "")
                    );
                return new Common.RepositoryVariableAccessor(repository, dataRef);
            }
            else if (str.StartsWith("cself["))
            {
                // ���s���̃R�����C�x���g�̃Z���t�ϐ��Ăяo��
                // []�����擾
                string fieldStr = str.Substring("cself[".Length, str.Length - "cself[]".Length);
                if (!int.TryParse(fieldStr, out int variableId))
                {
                    // ID�ϊ��ł��Ȃ��ꍇ�͒萔�Ƃ��ĕԂ�
                    return new Common.ConstDataAccessor(str);
                }
                if (context.CommonEventId == null)
                {
                    // �R�����C�x���g����Ăяo����Ă��Ȃ��ꍇ��0��Ԃ�
                    return new Common.ConstDataAccessor("0");
                }

                return new Event.CommonEventVariableAccessor(
                    new Event.CommonEventId(context.CommonEventId.Value), variableId);
            }
            else if (str.StartsWith("sdb["))
            {
                // �V�X�e��DB�̕ϐ��Ăяo��
                // []�����擾
                string fieldStr = str.Substring("sdb[".Length, str.Length - "sdb[]".Length);
                if (int.TryParse(fieldStr, out int variableId))
                {
                    // ID�ϊ��ł��Ȃ��ꍇ�͒萔�Ƃ��ĕԂ�
                    return new Common.ConstDataAccessor(str);
                }
                if (context.CommonEventId == null)
                {
                    // �R�����C�x���g����Ăяo����Ă��Ȃ��ꍇ��0��Ԃ�
                    return new Common.ConstDataAccessor("0");
                }

                return new Event.CommonEventVariableAccessor(
                    new Event.CommonEventId(context.CommonEventId.Value), variableId);
            }

            // ��������ȊO�̏ꍇ�A�萔���擾
            return new Common.ConstDataAccessor(str);
        }
    }

    internal class StringBlockFactory2
    {
        private string text;
        private HashSet<string> patterns;

        public StringBlockFactory2(string text)
        {
            this.text = text;
            patterns = BuildPatterns();
        }

        public string GenerateMessage(CommandVisitContext context)
        {
            int index = 0;

            // �ċA�I��\XXX[]�Ƃ�����������\�������Ȃ���擾����B
            // \XXX[]hoge\YYY[]�Ƃ����悤�ɕ���ŕ�������\������Ă���ꍇ������̂ŁA]�ŋ�؂�Ȃ���擾���Ă���
            IStringBlock block = CreateBlock(text, ref index, 0, context);
            return block.GetMessaage();
        }

        IStringBlock CreateBlock(string text, ref int index, int depth, CommandVisitContext context)
        {
            // �����̈ʒu�����߂�B����q�Ȃ珉�߂Č����]�̎�O�܂ł����A����q�ɂȂ��Ă��Ȃ��ꍇ�͖����܂œǂށB
            int lastPos = text.Length;
            if (depth > 0)
            {
                lastPos = text.IndexOf(']', index);
                if (lastPos == -1)
                {
                    lastPos = text.Length;
                }
            }

            // �u���b�N��ǂݑ�����
            List<IStringBlock> blocks = new List<IStringBlock>();
            while (index < lastPos)
            {
                int specialStartI = text.IndexOf('\\', index);
                if (specialStartI == -1 || specialStartI >= lastPos)
                {
                    // ���ꕶ�����Ȃ���΂��̂܂ܕԂ�
                    blocks.Add(new ConstStringBlock(text.Substring(specialStartI)));
                }

                int blockStartI = text.IndexOf('[', specialStartI);

                if (blockStartI == -1 || blockStartI >= lastPos)
                {
                    // ���ꕶ�����������Ȃ������ꍇ�A����������̂܂ܕԂ��B
                    blocks.Add(new ConstStringBlock(text.Substring(index, lastPos - index)));
                    break;
                }

                string functionString = text.Substring(specialStartI, blockStartI - specialStartI);
                if (patterns.Contains(functionString))
                {
                    // ���ꕶ���Ȃ̂ŁA�����u���b�N���擾�B
                    IStringBlock child = CreateBlock(text, ref blockStartI, context);
                    // ���Z�ǂݎ����]�����Ȃ���Γ��ꕶ���Ƃ݂Ȃ��Ȃ��B
                    if (text[index] == ']')
                    {
                        return new SpecialStringBock(functionString, context, child);
                    }
                    else
                    {
                        // �u���b�N�ɂȂ��Ă��Ȃ��ꍇ�͂��̂܂ܑ�������
                        blocks.Add(new ConstStringBlock($"\\{functionString}[]"));
                    }
                }
            }

            if (blocks.Count == 1)
            {
                return blocks[0];
            }
            else
            {
                return new MultiStringBock(blocks);
            }
        }

        private HashSet<string> BuildPatterns()
        {
            // �������₷
            return new HashSet<string>
            {
                "self",
                "cself",
                "sdb",
            };
        }
    }

    internal interface IStringBlock
    {

        public string GetMessaage();
    }

    internal class MultiStringBock : IStringBlock
    {
        private List<IStringBlock> children;

        public MultiStringBock(List<IStringBlock> children) : base()
        {
            this.children = children;
        }

        public string GetMessaage()
        {
            string message = "";
            for (int i = 0; i < children.Count; i++)
            {
                message += children[i].GetMessaage();
            }

            return message;
        }
    }

    internal class ConstStringBlock : IStringBlock
    {
        private string part;

        public ConstStringBlock(string part) : base()
        {
            this.part = part;
        }

        public string GetMessaage()
        {
            return part;
        }
    }

    internal class SpecialStringBock : IStringBlock
    {
        private IStringBlock child;
        private string functionString;
        private CommandVisitContext context;

        public SpecialStringBock(string functionString,
            CommandVisitContext context, IStringBlock child) : base()
        {
            this.functionString = functionString;
            this.child = child;
        }

        public string GetMessaage()
        {
            string message = "";
            string childStr = child.GetMessaage();
            if (functionString.Equals("self"))
            {
                // ���s���̃}�b�v�C�x���g�̃Z���t�ϐ��Ăяo��
                if (!int.TryParse(childStr, out int fieldId))
                {
                    // ID�ϊ��ł��Ȃ��ꍇ�͒萔�Ƃ��ĕԂ�
                    message += childStr;
                }

                var repository = DI.DependencyInjector.It().MapEventStateRpository;
                Domain.Data.DataRef dataRef = new Domain.Data.DataRef(
                    new Domain.Data.TableId(context.MapId.Value, ""),
                    new Domain.Data.RecordId(context.EventId.Value, ""),
                    new Domain.Data.FieldId(fieldId, "")
                    );

                message += new Common.RepositoryVariableAccessor(repository, dataRef).GetString();
            }
            else if (functionString.Equals("cself"))
            {
                // ���s���̃R�����C�x���g�̃Z���t�ϐ��Ăяo��
                if (!int.TryParse(childStr, out int variableId))
                {
                    // ID�ϊ��ł��Ȃ��ꍇ�͒萔�Ƃ��ĕԂ�
                    message += childStr;
                }
                if (context.CommonEventId == null)
                {
                    // �R�����C�x���g����Ăяo����Ă��Ȃ��ꍇ��0��Ԃ�
                    message += "0";
                }

                message += new Event.CommonEventVariableAccessor(
                    new Event.CommonEventId(context.CommonEventId.Value), variableId).GetString();
            }
            else if (functionString.Equals("sdb"))
            {
                // �V�X�e��DB�̕ϐ��Ăяo���B\sdb[A:B:C]�Ń^�C�vA�ԁE�f�[�^B�ԁE����C�Ԃ��Ăяo���B
                string[] vars = childStr.Split(':');
                if (vars.Length != 3)
                {
                    return childStr;
                }

                if (!(int.TryParse(vars[0], out int tableId)
                    && int.TryParse(vars[1], out int recordId)
                    && int.TryParse(vars[2], out int fieldId)))
                {
                    // ID�ϊ��ł��Ȃ��ꍇ�͒萔�Ƃ��ĕԂ�
                    return childStr;
                }

                var repository = DI.DependencyInjector.It().SystemDataRepository;
                Domain.Data.DataRef dataRef = new Domain.Data.DataRef(
                    new Domain.Data.TableId(tableId, ""),
                    new Domain.Data.RecordId(recordId, ""),
                    new Domain.Data.FieldId(fieldId, "")
                    );

                message += new Common.RepositoryVariableAccessor(repository, dataRef).GetString();
            }
            else
            {
                // �����ł��Ȃ��ꍇ�͂����ԋp����
                message += $"\\{functionString}[{childStr}]";
            }

            return message;
        }
    }
}
