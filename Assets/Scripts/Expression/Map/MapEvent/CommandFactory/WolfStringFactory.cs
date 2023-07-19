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
                message += Create(parts[i], context).Get();
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

        private Common.IDataAccessor<string> Create(string str, CommandVisitContext context)
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
                    return new Common.ConstDataAccessor<string>(str);
                }

                var repository = DI.DependencyInjector.It().MapEventStateRpository;
                Domain.Data.DataRef dataRef = new Domain.Data.DataRef(
                    new Domain.Data.TableId(context.MapId.Value, ""),
                    new Domain.Data.RecordId(context.EventId.Value, ""),
                    new Domain.Data.FieldId(fieldId, "")
                    );
                return new Common.RepositoryStringAccessor(repository, dataRef);
            }
            else if (str.StartsWith("cself["))
            {
                // ���s���̃R�����C�x���g�̃Z���t�ϐ��Ăяo��
                // []�����擾
                string fieldStr = str.Substring("cself[".Length, str.Length - "cself[]".Length);
                if (!int.TryParse(fieldStr, out int variableId))
                {
                    // ID�ϊ��ł��Ȃ��ꍇ�͒萔�Ƃ��ĕԂ�
                    return new Common.ConstDataAccessor<string>(str);
                }
                if (context.CommonEventId == null)
                {
                    // �R�����C�x���g����Ăяo����Ă��Ȃ��ꍇ��0��Ԃ�
                    return new Common.ConstDataAccessor<string>("0");
                }

                return new Event.CommonEventStringAccessor(
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
                    return new Common.ConstDataAccessor<string>(str);
                }
                if (context.CommonEventId == null)
                {
                    // �R�����C�x���g����Ăяo����Ă��Ȃ��ꍇ��0��Ԃ�
                    return new Common.ConstDataAccessor<string>("0");
                }

                return new Event.CommonEventStringAccessor(
                    new Event.CommonEventId(context.CommonEventId.Value), variableId);
            }

            // ��������ȊO�̏ꍇ�A�萔���擾
            return new Common.ConstDataAccessor<string>(str);
        }
    }
}
