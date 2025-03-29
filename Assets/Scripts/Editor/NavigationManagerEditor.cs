using UnityEngine;
using UnityEditor;

#nullable enable

[CustomEditor(typeof(NavigationManager), true)]
public class NavigationManagerEditor : Editor {
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        DrawDefaultInspector();

        // Get reference to the target script
        NavigationManager manager = (NavigationManager)target;

        EditorGUILayout.LabelField($"Standing Nav Points: {manager.StandingNavPointsCount}", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Crouching Nav Points: {manager.CrouchingNavPointsCount}", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Only Crouching Nav Points: {manager.OnlyCrouchingNavPointsCount}", EditorStyles.boldLabel);

        if(GUILayout.Button("Refresh")){
            manager.Refresh();
        }
    }
}