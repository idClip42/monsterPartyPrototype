using System;
using UnityEngine;

#nullable enable

[RequireComponent(typeof(CharacterController))]
public class SimpleCharacterCrouch : CharacterCrouch
{
    private SimpleCharacter? _character = null;
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

    public override string DebugInfo { get {
        string result = base.DebugInfo;
        if(uncrouchHitInfo.collider != null)
            result += $" ({uncrouchHitInfo.collider.gameObject.name})";
        result += $" (Height: {CurrentHeight}m)";
        return result;
    }} 

    protected override void Awake()
    {
        base.Awake();
        
        _character = GetComponent<SimpleCharacter>();
        if(_character == null)
            throw new Exception($"Null character on {this.gameObject.name}");
        
        _characterController = GetComponent<CharacterController>();
        if(_characterController == null)
            throw new Exception($"Null character controller on {this.gameObject.name}");
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if(_character != null){
            Debug.DrawLine(
                transform.position,
                transform.position + Vector3.up * _character.ModelHeight * _crouchHeightPercentage,
                Color.green
            );
        }
    }
#endif

    protected override void EnableCrouch() => ToggleCrouch(true);

    protected override void DisableCrouch() => ToggleCrouch(false);

    private void ToggleCrouch(bool isCrouching){
        if(_character == null)
            throw new Exception($"Null character on {this.gameObject.name}");
        
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

        transform.localScale = new Vector3(
            transform.localScale.x,
            newYScale,
            transform.localScale.z
        );

        // TODO: Character shifts up 1/4 of their height on crouch
        // TODO: Character shifts down 1/4 of their height on uncrouch
    }

    protected override bool CanUncrouch()
    {
        if(_characterController == null)
            throw new Exception($"Null character controller on {this.gameObject.name}");
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
