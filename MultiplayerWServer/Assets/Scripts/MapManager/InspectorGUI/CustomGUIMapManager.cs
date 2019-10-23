using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;

[CustomEditor(typeof(MapManager))]
public class CustomGUIMapManager : Editor
{
    public override void OnInspectorGUI()
    {
        MapManager myScript = (MapManager)target;
        SerializedObject serializedObj = new SerializedObject(myScript);
        List<string> thigsToExclude = new List<string>();

        DrawPropertiesExcluding(serializedObj,thigsToExclude.ToArray());
        if (GUILayout.Button("CreateNewMap"))
        {
            myScript.GenerateMap();
        }
        if (GUILayout.Button("DestroyCells"))
        {
            myScript.DestroyCells();
        }
        serializedObj.ApplyModifiedProperties();
    }
}
#endif