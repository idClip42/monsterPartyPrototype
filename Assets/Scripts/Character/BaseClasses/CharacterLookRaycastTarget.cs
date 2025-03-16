using UnityEngine;
using UnityEditor;

#nullable enable

[DisallowMultipleComponent]
public class CharacterLookRaycastTarget : MonoBehaviour
{
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Color prevColor = Handles.color;
        Handles.color = Color.magenta;

        Handles.DrawWireCube(
            transform.position,
            Vector3.one * 0.1f
        );
        
        Handles.color = prevColor;
    }
#endif
}
