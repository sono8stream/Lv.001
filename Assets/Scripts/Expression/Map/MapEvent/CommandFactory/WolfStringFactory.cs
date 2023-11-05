using Util.Wolf;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Expression.Map.MapEvent.CommandFactory
{
    public class WolfStringFactory : IStringFactory
    {
        private string[] parts;
        private StringBlockFactory2 blockFactory;

        public WolfStringFactory(string text)
        {
            parts = CreateParts(text);
            blockFactory = new StringBlockFactory2(text);
        }

        string[] CreateParts(string text)
        {
            var matches = Regex.Matches(text, BuildPattern());
            return Regex.Split(text, BuildPattern());
        }

        public string GenerateMessage(CommandVisitContext context)
        {
            return blockFactory.GenerateMessage(context);

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
            List<IStringBlock> blocks = new List<IStringBlock>();
            string tmp = "";
            bool isSpecial = false;

            for (; index < text.Length; index++)
            {
                // �O���珇�ɓǂ�ł���
                if (text[index] == '\\')
                {
                    isSpecial = true;
                    // ���ꕶ�����n�܂�̂ŁA��U�����܂ł̕��������ŋl�߂Ă���
                    blocks.Add(new ConstStringBlock(tmp));
                    tmp = "";
                }
                else if (text[index] == '[')
                {
                    // ���ꕶ���̏I���Ȃ̂ŁA����q����B
                    if (isSpecial)
                    {
                        if (patterns.Contains(tmp))
                        {
                            // ���ꕶ���Ȃ̂ŁA�����u���b�N���擾�B
                            index++;
                            IStringBlock child = CreateBlock(text, ref index, depth + 1, context);
                            // �u���b�N�ǂݎ���̈ʒu��]�����Ȃ���Γ��ꕶ���Ƃ݂Ȃ��Ȃ��B
                            if (index < text.Length && text[index] == ']')
                            {
                                blocks.Add(new SpecialStringBlock(tmp, context, child));
                            }
                            else
                            {
                                // ���ꕶ���̃u���b�N�ɂȂ��Ă��Ȃ��ꍇ�͒ʏ�̕�����Ƃ��đ�������
                                List<IStringBlock> blocksTmp = new List<IStringBlock>();
                                blocksTmp.Add(new ConstStringBlock($"\\{tmp}["));
                                blocksTmp.Add(child);
                                if (index < text.Length)
                                {
                                    blocksTmp.Add(new ConstStringBlock($"{text[index]}"));
                                }
                                blocks.Add(new MultiStringBlock(blocksTmp));
                            }
                        }
                        else
                        {
                            // ���߉\�ȓ��ꕶ���ł͂Ȃ����A����q�\���ɂ͂Ȃ��Ă���̂Ńn���h�����O���Ă����B
                            index++;
                            List<IStringBlock> blocksTmp = new List<IStringBlock>();
                            blocksTmp.Add(new ConstStringBlock($"\\{tmp}["));
                            IStringBlock child = CreateBlock(text, ref index, depth + 1, context);
                            blocksTmp.Add(child);
                            if (index < text.Length)
                            {
                                blocksTmp.Add(new ConstStringBlock($"{text[index]}"));
                            }
                            blocks.Add(new MultiStringBlock(blocksTmp));
                        }
                        tmp = "";
                        isSpecial = false;
                    }
                    else
                    {
                        // ���ꕶ���łȂ����ʂ͑��̕����Ɠ��l�ɏ����B
                        tmp += text[index];
                    }
                }
                else if (text[index] == ']')
                {
                    if (depth > 0)
                    {
                        // �l�X�g���Ă���Ȃ��ʂ���̌Ăяo���Ŋ��ʂ���铮��Ȃ̂ŁA��ʂɕԂ�
                        if (!string.IsNullOrEmpty(tmp))
                        {
                            blocks.Add(new ConstStringBlock(tmp));
                        }
                        return new MultiStringBlock(blocks);
                    }
                    else
                    {
                        // �l�X�g���Ă��Ȃ���ԂŊ��ʂ���铮��͓��ꕶ���ł͂Ȃ��͂��Ȃ̂ŁA�ʏ�ʂ菈���B
                        tmp += text[index];
                    }
                }
                else
                {
                    tmp += text[index];
                }
            }

            if (!string.IsNullOrEmpty(tmp))
            {
                blocks.Add(new ConstStringBlock(tmp));
            }

            return new MultiStringBlock(blocks);
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

    internal class MultiStringBlock : IStringBlock
    {
        private List<IStringBlock> children;

        public MultiStringBlock(List<IStringBlock> children) : base()
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

    internal class SpecialStringBlock : IStringBlock
    {
        private IStringBlock child;
        private string functionString;
        private CommandVisitContext context;

        public SpecialStringBlock(string functionString,
            CommandVisitContext context, IStringBlock child) : base()
        {
            this.functionString = functionString;
            this.child = child;
            this.context = context;
        }

        public string GetMessaage()
        {
            string childStr = child.GetMessaage();
            if (functionString.Equals("self"))
            {
                // ���s���̃}�b�v�C�x���g�̃Z���t�ϐ��Ăяo��
                if (!int.TryParse(childStr, out int fieldId))
                {
                    // ID�ϊ��ł��Ȃ��ꍇ�͒萔�Ƃ��ĕԂ�
                    return $"\\{functionString}[{childStr}]";
                }

                var repository = DI.DependencyInjector.It().MapEventStateRpository;
                Domain.Data.DataRef dataRef = new Domain.Data.DataRef(
                    new Domain.Data.TableId(context.MapId.Value, ""),
                    new Domain.Data.RecordId(context.EventId.Value, ""),
                    new Domain.Data.FieldId(fieldId, "")
                    );

                return new Common.RepositoryVariableAccessor(repository, dataRef).GetString();
            }
            else if (functionString.Equals("cself"))
            {
                // ���s���̃R�����C�x���g�̃Z���t�ϐ��Ăяo��
                if (!int.TryParse(childStr, out int variableId))
                {
                    // ID�ϊ��ł��Ȃ��ꍇ�͒萔�Ƃ��ĕԂ�
                    return $"\\{functionString}[{childStr}]";
                }

                if (context.CommonEventId == null)
                {
                    // �R�����C�x���g����Ăяo����Ă��Ȃ��ꍇ��0��Ԃ�
                    return "0";
                }

                return new Event.CommonEventVariableAccessor(
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

                return new Common.RepositoryVariableAccessor(repository, dataRef).GetString();
            }
            else
            {
                // �����ł��Ȃ��ꍇ�͂����ԋp����
                return $"\\{functionString}[{childStr}]";
            }
        }
    }
}
