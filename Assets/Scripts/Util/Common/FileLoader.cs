using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Util.Common
{
    /// <summary>
    /// �t�@�C���̓ǂݎ������b�v����N���X
    /// �Ή��FWin�AAndroid�AWebGL
    /// </summary>
    public class FileLoader
    {
        public static byte[] LoadSync(string path)
        {
            Debug.Log(path);
            byte[] res = null;

            using (var request = UnityWebRequest.Get(path))
            {
                request.timeout = 30;// �^�C���A�E�g����{�I��5�b�Őݒ�
                var async = request.SendWebRequest();

                while (true)
                {
                    if (request.result == UnityWebRequest.Result.ProtocolError
                        || request.result == UnityWebRequest.Result.ConnectionError)
                    {
                        //�G���[
                        Debug.LogError(request.error);
                        break;
                    }

                    if (async.isDone)
                    {
                        //����I��
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
