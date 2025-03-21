using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Entity), true)] // 'true' makes this apply to subclasses as well
public class EntityEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw default inspector properties
        DrawDefaultInspector();

        // Get reference to the target script
        Entity entity = (Entity)target;

        // Display DebugValues in a read-only text area
        EditorGUILayout.LabelField("Debug Info", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(entity.DebugInfoString, EditorStyles.wordWrappedMiniLabel);
    }
}
