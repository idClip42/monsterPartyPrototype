using UnityEngine;

#nullable enable

[RequireComponent(typeof(CharacterController))]
public class SimpleCharacterCrouch : CharacterCrouch
{
    private CharacterController? _characterController = null;

    [SerializeField]
    [Range(0.1f, 0.7f)]
    private float _crouchHeightPercentage = 0.5f;

    private float CurrentHeight => _characterController ? 
        _characterController.height * transform.localScale.y:
        -1;

    public override string DebugInfo => 
        base.DebugInfo + 
        $" (Height: {CurrentHeight}m)";

    protected override void Awake()
    {
        base.Awake();
        
        _characterController = GetComponent<CharacterController>();
        if(_characterController == null)
            throw new System.Exception($"Null character controller on {this.gameObject.name}");
    }

    protected override void EnableCrouch() => ToggleCrouch(true);

    protected override void DisableCrouch() => ToggleCrouch(false);

    private void ToggleCrouch(bool isCrouching){
        float meshHeight = GetComponent<MeshRenderer>().bounds.extents.y;
        float startYScale = transform.localScale.y;
        float newHeight;
        float newYScale;

        if(isCrouching){
            newYScale = startYScale * _crouchHeightPercentage;
            newHeight = meshHeight * _crouchHeightPercentage;
        }
        else {
            newYScale = startYScale / _crouchHeightPercentage;
            newHeight = meshHeight / _crouchHeightPercentage;
        }

        transform.localScale = new Vector3(
            transform.localScale.x,
            newYScale,
            transform.localScale.z
        );

        transform.position += new Vector3(
            0,
            (newHeight - meshHeight) / 2,
            0
        );
    }

    protected override bool CanUncrouch()
    {
        if(this.IsCrouching == false) return false;
        
        return true;
    }
}
