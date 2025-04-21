using UnityEngine;
using UnityEditor;

#nullable enable

namespace MonsterParty
{
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
            EditorGUILayout.LabelField(entity.GetDebugInfoString(), EditorStyles.wordWrappedMiniLabel);

            // Button to call Kill() method
            GUI.enabled = entity.Alive && Application.isPlaying;
            if (GUILayout.Button("Kill"))
            {
                entity.Kill();
            }
            GUI.enabled = true;

            // Button to call Resurrect() method
            GUI.enabled = !entity.Alive && Application.isPlaying;
            if (GUILayout.Button("Resurrect"))
            {
                entity.Resurrect();
            }
            GUI.enabled = true;
        }
    }
}