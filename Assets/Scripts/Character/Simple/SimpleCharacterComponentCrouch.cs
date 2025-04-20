using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#nullable enable

[RequireComponent(typeof(SimpleCharacter))]
[RequireComponent(typeof(CharacterController))]
public class SimpleCharacterComponentCrouch : CharacterComponentCrouch
{
    private SimpleCharacter? _simpleCharacter = null;
    private CharacterController? _characterController = null;

    [SerializeField]
    [Range(0.1f, 0.7f)]
    private float _crouchHeightPercentage = 0.5f;

    [SerializeField]
    [Range(0.1f, 1f)]
    private float _uncrouchRadiusPercentage = 0.9f;

    private float CurrentHeight => _characterController ? 
        _characterController.height * transform.localScale.y:
        -1;

    private float StandingHeight => _characterController ? 
        _characterController.height :
        -1;

    private RaycastHit uncrouchHitInfo;

    public sealed override void FillInDebugInfo(Dictionary<string, string> infoTarget)
    {
        base.FillInDebugInfo(infoTarget);
        if(uncrouchHitInfo.collider != null)
            infoTarget["Collider Above"] = uncrouchHitInfo.collider.gameObject.name;
        infoTarget["Height"] = $"{CurrentHeight:F2}m";
    }

    protected override void Awake()
    {
        base.Awake();
        
        _simpleCharacter = GetComponent<SimpleCharacter>();
        if(_simpleCharacter == null)
            throw new MonsterPartyException($"Null character on {this.gameObject.name}");
        
        _characterController = GetComponent<CharacterController>();
        if(_characterController == null)
            throw new MonsterPartyException($"Null character controller on {this.gameObject.name}");
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        using(new Handles.DrawingScope(Color.green)){
            if(_simpleCharacter != null){
                Handles.DrawLine(
                    transform.position,
                    transform.position + Vector3.up * _simpleCharacter.ModelHeight * _crouchHeightPercentage            );
            }
        }
    }
#endif

    protected sealed override void EnableCrouch() => ToggleCrouch(true);

    protected sealed override void DisableCrouch() => ToggleCrouch(false);

    private void ToggleCrouch(bool isCrouching){
        if(_simpleCharacter == null)
            throw new MonsterPartyException($"Null character on {this.gameObject.name}");
        if(_characterController == null)
            throw new MonsterPartyException($"Null character controller on {this.gameObject.name}");
        
        // float meshHeight = _character.ModelHeight;
        float startYScale = transform.localScale.y;
        // float newHeight;
        float newYScale;

        if(isCrouching){
            newYScale = startYScale * _crouchHeightPercentage;
            // newHeight = meshHeight * _crouchHeightPercentage;
        }
        else {
            newYScale = startYScale / _crouchHeightPercentage;
            // newHeight = meshHeight / _crouchHeightPercentage;
        }

        // Turn off character controller while transforming.
        // (And store the initial state so we can restore it later.)
        bool preControllerState = _characterController.enabled;
        _characterController.enabled = false;

        // Squash the character.
        transform.localScale = new Vector3(
            transform.localScale.x,
            newYScale,
            transform.localScale.z
        );

        // Restore the character controller
        // (back to what it was)
        // now that transforming is done.
        _characterController.enabled = preControllerState;
    }

    protected sealed override bool CanUncrouch()
    {
        if(_characterController == null)
            throw new MonsterPartyException($"Null character controller on {this.gameObject.name}");
        if(this.IsCrouching == false) return false;
        
        float radius = _characterController.radius * _uncrouchRadiusPercentage;
        Vector3 basePosition = transform.position /* - Vector3.up * CurrentHeight / 2 */;
        Vector3 sphereCastStart = basePosition + Vector3.up * radius;
        float castDistance = StandingHeight - radius * 2;

        bool collision = Physics.SphereCast(
            sphereCastStart,
            radius,
            Vector3.up,
            out uncrouchHitInfo,
            castDistance
        );

        if(collision){
            Debug.DrawLine(sphereCastStart, uncrouchHitInfo.point, Color.yellow);
        }
        else {
            Debug.DrawLine(sphereCastStart, sphereCastStart + Vector3.up * castDistance, Color.white);
        }

        return !collision;
    }
}
