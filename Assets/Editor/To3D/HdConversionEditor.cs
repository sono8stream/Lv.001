using UnityEngine;
using UnityEditor;
using System.IO;

namespace Hd2d
{
#if UNITY_EDITOR
    public class HdConversionEditor : EditorWindow
    {
        Material mat = null;
        Vector2Int offset = Vector2Int.zero;
        MapBlockType blockType;

        [MenuItem("Window/Hd2dConverter/TileEditor")]
        static void Show()
        {
            GetWindow(typeof(HdConversionEditor));
        }

        private void OnGUI()
        {
            mat = EditorGUILayout.ObjectField("Material", mat, typeof(Material), false) as Material;
            blockType = (MapBlockType)EditorGUILayout.EnumPopup("MapBlockType", blockType);
            offset = EditorGUILayout.Vector2IntField("Offset", offset);
            if (GUILayout.Button("Generate quads"))
            {
                GenerateMapObject();
            }
        }

        private void GenerateMapObject()
        {
            GameObject ob = new GameObject("Cube");
            Hd2dBlock block = null;
            Vector2Int[] offsets = null;
            switch (blockType)
            {
                case MapBlockType.Cube:
                    {
                        int meshCount = 6;
                        offsets = new Vector2Int[meshCount];
                        for (int i = 0; i < meshCount; i++)
                        {
                            offsets[i] = offset;
                        }
                        block = ob.AddComponent<Hd2dCube>();
                    }
                    break;
                case MapBlockType.Slope:
                    {
                        int meshCount = 5;
                        offsets = new Vector2Int[meshCount];
                        for (int i = 0; i < meshCount; i++)
                        {
                            offsets[i] = offset;
                        }
                        block = ob.AddComponent<Hd2dSlope>();
                    }
                    break;
                default:
                    break;
            }
            block?.Initialize(mat, offsets, Vector3Int.one);

            //obj.GetComponent<Renderer>().material=

            /*
            var filePath = "Assets/Editor/To3D/GenerateTest/material.mat";

            // フォルダを作成
            var folderPath = Path.GetDirectoryName(filePath);
            CreateFolder(folderPath);

            // アセットのパスを作成
            var assetPath = AssetDatabase.GenerateUniqueAssetPath(filePath);

            Material material = new Material(Shader.Find("Standard"));
            AssetDatabase.CreateAsset(material, assetPath);
            AssetDatabase.Refresh();
            */
        }

        /// <summary>
        /// 指定されたパスのフォルダを生成する
        /// </summary>
        /// <param name="path">フォルダパス（例: Assets/Sample/FolderName）</param>
        private static void CreateFolder(string path)
        {
            var target = "";
            var splitChars = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
            foreach (var dir in path.Split(splitChars))
            {
                var parent = target;
                target = Path.Combine(target, dir);
                if (!AssetDatabase.IsValidFolder(target))
                {
                    AssetDatabase.CreateFolder(parent, dir);
                }
            }
        }
    }
#endif
}