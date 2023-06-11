using Domain.Data;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure
{
    /// <summary>
    /// WolfRPG‚ÉŠÖ˜A‚·‚é’è”‚Ì•Û
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
                    return "CDatabase";
                case DatabaseType.User:
                    return "Database";
                default:
                    throw new System.Exception("–¢À‘•‚ÌƒGƒ‰[í•Ê");
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
