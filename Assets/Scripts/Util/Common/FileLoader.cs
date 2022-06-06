using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Util.Common
{
    /// <summary>
    /// �t�@�C���̓ǂݎ������b�v����N���X
    /// </summary>
    public class FileLoader
    {
        public static byte[] LoadSync(string path)
        {
            byte[] res = null;

            using (var request = UnityWebRequest.Get(path))
            {
                var async = request.SendWebRequest();

                while (true)
                {
                    if (request.isHttpError || request.isNetworkError)
                    {
                        //�G���[
                        Debug.LogError(request.error);
                        break;
                    }

                    if (async.isDone)
                    {
                        //����I��
                        Debug.Log("DONE!");
                        break;
                    }
                }

                res = async.webRequest.downloadHandler.data;
            }

            // Windows�̂ݓ���
            /*
            using (var fs = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                res = new byte[fs.Length];
                fs.Read(res, 0, res.Length);
            }
            */

            return res;
        }
    }
}
