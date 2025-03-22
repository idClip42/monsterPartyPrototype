using UnityEngine;
using UnityEditor;

#nullable enable

[CustomEditor(typeof(Character), true)] // 'true' makes this apply to subclasses as well
public class CharacterEditor : EntityEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // Get reference to the target script
        Character character = (Character)target;

        // Button to call Die() method
        GUI.enabled = character.Alive && Application.isPlaying;
        if (GUILayout.Button("Kill"))
        {
            character.Kill();
        }
        GUI.enabled = true;
    }
}
