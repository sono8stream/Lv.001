using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Expression;
using Expression.Map;

namespace Hd2d
{
    // �y�b��zAssembly Info��؂蕪����ifdef���폜
#if UNITY_EDITOR
    public class Hd2dMapConverter : EditorWindow
    {
        private Material mat = null;
        private int mapDataIndex = -1;

        [MenuItem("Window/Hd2dConverter/WolfConverter")]
        static void ShowMapConverter()
        {
            GetWindow(typeof(Hd2dMapConverter));
        }

        private void OnGUI()
        {
            // GUI
            ShowMapFilePullDown();

            mat = EditorGUILayout.ObjectField("Material", mat, typeof(Material), false) as Material;
            /*
            if (GUILayout.Button("Generate quads"))
            {
                GenerateMapObject();
            }
            */
        }

        private void ShowMapFilePullDown()
        {
            // �v���_�E�����j���[�ɓo�^���镶����z��
            string path = $"{Application.streamingAssetsPath}/Data/MapData";
            string[] filePaths = System.IO.Directory.GetFiles(path, "*.mps");
            string[] displayOptions = filePaths.Select(a => a.Replace($"{path}\\", "")).ToArray();

            // �v���_�E�����j���[�̍쐬
            var curIndex = displayOptions.Length > 0
            ? EditorGUILayout.Popup("MapData", mapDataIndex, displayOptions)
                : -1;

            // �`�F�b�N�I����
            if (EditorGUI.EndChangeCheck())
            {
                if (mapDataIndex != curIndex)
                {
                    mapDataIndex = curIndex;
                    MapId id = new MapId(mapDataIndex);

                    string dirPath = $"{Application.streamingAssetsPath}/Data/MapData/SampleMapA.mps";
                    Wolhd2dfMapCreator creator = new Wolhd2dfMapCreator(null);
                    creator.Create(dirPath);
                }
            }
        }
    }
#endif
}
