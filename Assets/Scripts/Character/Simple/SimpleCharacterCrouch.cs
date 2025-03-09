using UnityEngine;

#nullable enable

public class SimpleCharacterCrouch : CharacterCrouch
{
    [SerializeField]
    [Range(0.1f, 0.7f)]
    private float _crouchHeightPercentage = 0.5f;

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
}
