using UnityEngine;
using UnityEditor;

#nullable enable

[CustomEditor(typeof(NavigationManager), true)]
public class NavigationManagerEditor : Editor {
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // Get reference to the target script
        NavigationManager manager = (NavigationManager)target;

        EditorGUILayout.LabelField($"Nav Points: {manager.NavPointsCount}", EditorStyles.boldLabel);

        if(GUILayout.Button("Refresh")){
            manager.Refresh();
        }
    }
}