using UnityEngine;
using UnityEditor;

#nullable enable

[CustomEditor(typeof(SimpleMonsterKillerMachine), true)] // 'true' makes this apply to subclasses as well
public class SimpleMonsterKillerMachineEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw default inspector properties
        DrawDefaultInspector();

        // Get reference to the target script
        SimpleMonsterKillerMachine machine = (SimpleMonsterKillerMachine)target;

        GUI.enabled = !machine.IsReady() && Application.isPlaying;
        if (GUILayout.Button("Force Activate"))
        {
            machine.ForceFillAllReceptacles();
        }
        GUI.enabled = true;
    }
}
