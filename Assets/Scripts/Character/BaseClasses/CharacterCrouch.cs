using UnityEngine;

public abstract class CharacterCrouch : MonoBehaviour
{
    private Character _characterBase;

    private bool _isCrouching = false;
    public bool IsCrouching => _isCrouching;

    void Awake()
    {
        _characterBase = GetComponent<Character>();
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
        if(this._isCrouching) EnableCrouch();
        else DisableCrouch();
    }

    protected abstract void EnableCrouch();
    protected abstract void DisableCrouch();
}
