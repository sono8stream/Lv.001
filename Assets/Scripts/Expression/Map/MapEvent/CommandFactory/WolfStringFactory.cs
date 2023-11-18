using Util.Wolf;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using static Infrastructure.WolfConfig;

namespace Expression.Map.MapEvent.CommandFactory
{
    public class WolfStringFactory : IStringFactory
    {
        private string text;
        private HashSet<string> patterns;

        public WolfStringFactory(string text)
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
                                AddSpecialBlock(blocks, tmp, context, child);
                            }
                            else
                            {
                                // ���ꕶ���̃u���b�N�ɂȂ��Ă��Ȃ��ꍇ�i�����ʂ������ꍇ�Ȃǁj�͒ʏ�̕�����Ƃ��đ�������
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
                "cdb",
                "udb",
            };
        }

        private void AddSpecialBlock(List<IStringBlock> blocks, string functionString, CommandVisitContext context, IStringBlock child)
        {
            // �t�@�N�g���N���X������ĈϏ������ق�������
            if (functionString.Equals("self"))
            {
                blocks.Add(new SelfVariableAccessStringBlock(context, child));
            }
            else if (functionString.Equals("sdb"))
            {
                blocks.Add(new DatabaseAccessStringBlock(Infrastructure.WolfConfig.DatabaseType.System, functionString, context, child));
            }
            else if (functionString.Equals("cdb"))
            {
                blocks.Add(new DatabaseAccessStringBlock(Infrastructure.WolfConfig.DatabaseType.Changable, functionString, context, child));
            }
            else if (functionString.Equals("udb"))
            {
                blocks.Add(new DatabaseAccessStringBlock(Infrastructure.WolfConfig.DatabaseType.User, functionString, context, child));
            }
            else
            {
                blocks.Add(new SpecialStringBlock(functionString, context, child));
            }
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
        protected string functionString;
        protected IStringBlock child;
        protected CommandVisitContext context;

        public SpecialStringBlock(string functionString,
            CommandVisitContext context, IStringBlock child) : base()
        {
            this.functionString = functionString;
            this.child = child;
            this.context = context;
        }

        public virtual string GetMessaage()
        {
            // �ŏI�I�Ɍʂ�StringBlock�ɕ������Aif����o�ł���
            string childStr = child.GetMessaage();
            if (functionString.Equals("cself"))
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
            else
            {
                // �����ł��Ȃ��ꍇ�͂����ԋp����
                return $"\\{functionString}[{childStr}]";
            }
        }
    }

    internal class SelfVariableAccessStringBlock : SpecialStringBlock
    {
        public SelfVariableAccessStringBlock(CommandVisitContext context, IStringBlock child)
            : base("self", context, child)
        {
        }

        public override string GetMessaage()
        {
            string childStr = child.GetMessaage();

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
    }

    internal class DatabaseAccessStringBlock : SpecialStringBlock
    {
        Infrastructure.WolfConfig.DatabaseType databaseType;

        public DatabaseAccessStringBlock(Infrastructure.WolfConfig.DatabaseType databaseType,
            string functionString, CommandVisitContext context, IStringBlock child)
            : base(functionString, context, child)
        {
            this.databaseType = databaseType;
        }

        public override string GetMessaage()
        {
            string childStr = child.GetMessaage();
            string originalString = $"\\{functionString}[{childStr}]";

            // DB�̕ϐ��Ăяo���B\sdb[A:B:C]�Ń^�C�vA�ԁE�f�[�^B�ԁE����C�Ԃ��Ăяo���B
            string[] vars = childStr.Split(':');
            if (vars.Length != 3)
            {
                return originalString;
            }

            if (!(int.TryParse(vars[0], out int tableId)
                && int.TryParse(vars[1], out int recordId)
                && int.TryParse(vars[2], out int fieldId)))
            {
                // ID�ϊ��ł��Ȃ��ꍇ�͒萔�Ƃ��ĕԂ�
                return originalString;
            }

            var accessor = new Command.WolfRepositoryAccessorFactory(databaseType, tableId, recordId, fieldId);
            return accessor.GetString(context);
        }
    }
}
