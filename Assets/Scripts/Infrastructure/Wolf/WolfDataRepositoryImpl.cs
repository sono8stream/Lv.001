using Domain.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure
{
    /// <summary>
    /// WolfRPG��DB��ǂݏo�����߂̃��|�W�g�������N���X
    /// �y�b��zGet/Set�̃��W�b�N���A�N�Z�b�T���ɂ�������Ă���̂ŁA���W�b�N���W�񂳂���B
    /// </summary>
    public class WolfDataRepositoryImpl
    {
        private Dictionary<DataRef, int> intDict;
        private Dictionary<DataRef, string> stringDict;

        public WolfDataRepositoryImpl(WolfConfig.DatabaseType dbType)
        {
            var loader = new Infrastructure.WolfDatabaseLoader();
            loader.LoadDatabase(dbType, out intDict, out stringDict);
        }

        public DataField<int> FindInt(DataRef dataRef)
        {
            // ���lDict�ɗv�f��������Ή������o���Ȃ��Bnull�n���h�����O�̓A�N�Z�b�T���ŏ���������B
            if (intDict.ContainsKey(dataRef))
            {
                int val = intDict[dataRef];
                return new DataField<int>(dataRef.FieldId, val);
            }
            else
            {
                return null;
            }
        }

        public void SetInt(DataRef dataRef, int value)
        {
            Debug.Log($"Set {value} to Table: {dataRef.TableId.Value}, Record: {dataRef.RecordId.Value}, Filed: {dataRef.FieldId.Value}");
            if (intDict.ContainsKey(dataRef))
            {
                intDict[dataRef] = value;
            }
            else if (stringDict.ContainsKey(dataRef))
            {
                stringDict[dataRef] = value.ToString();
            }
        }

        public DataField<string> FindString(DataRef dataRef)
        {
            // ������Dict�ɗv�f��������Ή������o���Ȃ��Bnull�n���h�����O�̓A�N�Z�b�T���ŏ���������B
            if (stringDict.ContainsKey(dataRef))
            {
                return new DataField<string>(dataRef.FieldId, stringDict[dataRef].ToString());
            }
            else
            {
                return null;
            }
        }

        public void SetString(DataRef dataRef, string value)
        {
            if (stringDict.ContainsKey(dataRef))
            {
                stringDict[dataRef] = value;
            }
        }
    }
}
