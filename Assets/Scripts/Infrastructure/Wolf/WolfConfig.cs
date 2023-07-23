using Domain.Data;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure
{
    /// <summary>
    /// WolfRPGに関連する定数の保持
    /// </summary>
    public class WolfConfig
    {
        public static string GetDbProjectPath(DatabaseType type)
        {
            return $"{Application.streamingAssetsPath}/Data/BasicData/{GetDatabaseName(type)}.project";
        }

        public static string GetDbDatPath(DatabaseType type)
        {
            return $"{Application.streamingAssetsPath}/Data/BasicData/{GetDatabaseName(type)}.dat";
        }

        private static string GetDatabaseName(DatabaseType type)
        {
            switch (type)
            {
                case DatabaseType.System:
                    return "SysDatabase";
                case DatabaseType.Changable:
                    return "CDataBase";
                case DatabaseType.User:
                    return "DataBase";
                default:
                    throw new System.Exception("未実装のエラー種別");
            }
        }

        public enum DatabaseType
        {
            System, 
            Changable, 
            User
        }
    }
}
