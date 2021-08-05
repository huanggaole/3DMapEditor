using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorUI : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        MapGenerator mg = (MapGenerator)target;
        if (GUILayout.Button("生成地图"))
        {
            mg.gennerator();
            EditorUtility.SetDirty(mg);
        }
        if (GUILayout.Button("清空地图"))
        {
            mg.clear();
            EditorUtility.SetDirty(mg);
        }
    }
}
