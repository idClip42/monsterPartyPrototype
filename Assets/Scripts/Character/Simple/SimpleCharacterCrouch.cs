using UnityEngine;

public class SimpleCharacterCrouch : CharacterCrouch
{
    private SimpleCharacter _characterBase;

    [SerializeField]
    [Range(0.1f, 0.7f)]
    private float _crouchHeightPercentage = 0.5f;

    private bool _isCrouching = false;
    public override bool IsCrouching => _isCrouching;

    void Awake()
    {
        _characterBase = GetComponent<SimpleCharacter>();
        if(_characterBase == null)
            throw new System.Exception($"Null character base on {this.gameObject.name}");
    }

    void Update()
    {
        if(_characterBase.State == State.Player){
            if(Input.GetButtonDown("Crouch")){
                ToggleCrouch();
            }
        }
    }

    private void ToggleCrouch(){
        this._isCrouching = !this._isCrouching; 

        float meshHeight = GetComponent<MeshRenderer>().bounds.extents.y;
        float startYScale = transform.localScale.y;
        float newHeight;
        float newYScale;

        if(this._isCrouching){
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
            (newYScale - startYScale) / 2,
            0
        );
    }
}
