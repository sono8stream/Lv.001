using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Util.Common
{
    /// <summary>
    /// ファイルの読み取りをラップするクラス
    /// 対応：Win、Android、WebGL
    /// </summary>
    public class FileLoader
    {
        public static byte[] LoadSync(string path)
        {
            Debug.Log(path);
            byte[] res = null;

            using (var request = UnityWebRequest.Get(path))
            {
                request.timeout = 30;// タイムアウトを基本的に5秒で設定
                var async = request.SendWebRequest();

                while (true)
                {
                    if (request.result == UnityWebRequest.Result.ProtocolError
                        || request.result == UnityWebRequest.Result.ConnectionError)
                    {
                        //エラー
                        Debug.LogError(request.error);
                        break;
                    }

                    if (async.isDone)
                    {
                        //正常終了
                        Debug.Log($"Load DONE! for ${path}");
                        break;
                    }
                }

                res = async.webRequest.downloadHandler.data;
            }

            return res;
        }
    }
}
