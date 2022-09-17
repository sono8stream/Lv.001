using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Util.Common
{
    /// <summary>
    /// ファイルの書き出しをラップするクラス
    /// </summary>
    public class FileSaver
    {
        public static void SaveSync(string path, byte[] data)
        {
            using (var request = UnityWebRequest.Put(path,data))
            {
                var async = request.SendWebRequest();

                while (true)
                {
                    if (request.result==UnityWebRequest.Result.ProtocolError
                        ||request.result==UnityWebRequest.Result.ConnectionError)
                    {
                        //エラー
                        Debug.LogError(request.error);
                        break;
                    }

                    if (async.isDone)
                    {
                        //正常終了
                        Debug.Log("DONE!");
                        break;
                    }
                }

            }
        }
    }
}
