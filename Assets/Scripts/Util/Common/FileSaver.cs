using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Util.Common
{
    /// <summary>
    /// �t�@�C���̏����o�������b�v����N���X
    /// </summary>
    public class FileSaver
    {
        /// <summary>
        /// ���[�J���Z�[�u
        /// �Ή��FWin�AAndroid
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        public static void SaveLocalSync(string path, byte[] data)
        {
            string dirPath = System.IO.Path.GetDirectoryName(path);
            //�f�B���N�g�������݂��Ă��邩�̊m�F �Ȃ���ΐ���
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
        /// �A�b�v���[�h
        /// �Ή��FWebGL
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        public static void UploadSync(string path, byte[] data)
        {
            using (var request = UnityWebRequest.Put(path, data))
            {
                request.timeout = 5;// �^�C���A�E�g����{�I��5�b�Őݒ�
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
                        Debug.Log("DONE!");
                        break;
                    }
                }

            }
        }
    }
}
