using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(PlainPCG))]
public class PlainPCGEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        PlainPCG plainPCG = (PlainPCG)target;
        if(GUILayout.Button("生成地图")){
            plainPCG.GenerateMap();
        }
    }
}
