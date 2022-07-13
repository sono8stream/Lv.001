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
        private const int CHIP_COUNT = 256;// ���Ō��ߑł�
        private Vector3[] offsets = new Vector3[CHIP_COUNT];
        private Vector2 scrollPos = Vector2.zero;

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

            using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPos))
            {
                scrollPos = scrollView.scrollPosition;

                for (int i = 0; i < CHIP_COUNT; i++)
                {
                    offsets[i] = EditorGUILayout.Vector3Field("Sample", offsets[i]);
                }
            }
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
                    Hd2dTileInfo[] tileInfoArray = offsets.Select(offset => new Hd2dTileInfo(offset)).ToArray();
                    WolfHd2dMapFactory creator = new WolfHd2dMapFactory(id, tileInfoArray);
                    creator.Create(filePaths[curIndex]);
                }
            }
        }
    }
#endif
}
