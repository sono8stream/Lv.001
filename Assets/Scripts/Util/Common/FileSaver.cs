using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Util.Common
{
    /// <summary>
    /// ファイルの書き出しをラップするクラス
    /// </summary>
    public class FileSaver
    {
        /// <summary>
        /// ローカルセーブ
        /// 対応：Win、Android
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        public static void SaveLocalSync(string path, byte[] data)
        {
            string dirPath = System.IO.Path.GetDirectoryName(path);
            //ディレクトリが存在しているかの確認 なければ生成
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }


            using (var stream = File.Open(path, FileMode.OpenOrCreate))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(data);
                }
            }
        }

        /// <summary>
        /// アップロード
        /// 対応：WebGL
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        public static void UploadSync(string path, byte[] data)
        {
            using (var request = UnityWebRequest.Put(path, data))
            {
                request.timeout = 5;// タイムアウトを基本的に5秒で設定
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
                        Debug.Log("DONE!");
                        break;
                    }
                }

            }
        }
    }
}
