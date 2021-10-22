using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace WolfConverter
{
    public class MapConverter : EditorWindow
    {
		private Object folder;

		[UnityEditor.MenuItem("Window/WolfConverter/MapConverter")]
		static void ShowMapConverter()
		{
			EditorWindow.GetWindow(typeof(MapConverter));
		}

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
			GUILayout.Label("Image Directory : ", GUILayout.Width(200));
			folder = EditorGUILayout.ObjectField(folder, typeof(UnityEngine.Object), false);
			GUILayout.EndHorizontal();
			EditorGUILayout.Space();
		}
    }

}

