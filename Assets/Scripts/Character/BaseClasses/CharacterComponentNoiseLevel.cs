using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

#nullable enable

[DisallowMultipleComponent]
public abstract class CharacterComponentNoiseLevel : CharacterComponent
{
    public abstract float CurrentNoiseRadius { get; }

    public sealed override string DebugHeader => "Noise";

    public override void FillInDebugInfo(Dictionary<string, string> infoTarget)
    {
        infoTarget["Noise Radius"] = $"{CurrentNoiseRadius:F2}m";
    }

#if UNITY_EDITOR
    private void OnDrawGizmos() {        
        const float ARC_DEGREES = 80;
        const int INTERVAL = 5;
        using(new Handles.DrawingScope(Color.cyan)){
            if(CurrentNoiseRadius > 0){
                Vector3 origin = transform.position;
                Vector3[] directions = new Vector3[]{
                    Vector3.forward,
                    Vector3.right,
                    Vector3.back,
                    Vector3.left
                };
                foreach(var dir in directions){
                    for(int i = INTERVAL; i < CurrentNoiseRadius; i += INTERVAL){
                        Handles.DrawWireArc(
                            origin,
                            Vector3.up,
                            Quaternion.Euler(0, -ARC_DEGREES/2, 0) * dir,
                            ARC_DEGREES,
                            i
                        );
                    }
                    Handles.DrawWireArc(
                        origin,
                        Vector3.up,
                        Quaternion.Euler(0, -ARC_DEGREES/2, 0) * dir,
                        ARC_DEGREES,
                        CurrentNoiseRadius
                    );
                }    
            }
        }
    }
#endif
}