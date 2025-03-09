using UnityEngine;

#nullable enable

public abstract class CharacterCrouch : MonoBehaviour, ICharacterComponent
{
    private Character? _characterBase = null;

    private bool _isCrouching = false;
    public bool IsCrouching => _isCrouching;
    private bool _canUncrouch = false;

    public string DebugName => "Crouch";
    public virtual string DebugInfo => _isCrouching ? 
        $"Crouching ({(_canUncrouch ? "Can Stand" : "Can't Stand")})" : 
        "Standing";

    protected virtual void Awake()
    {
        _characterBase = GetComponent<Character>();
        if(_characterBase == null)
            throw new System.Exception($"Null character base on {this.gameObject.name}");
    }

    void Update()
    {
        if(_characterBase == null) throw new System.Exception("Null _characterBase");
        
        _canUncrouch = CanUncrouch();
        
        if(_characterBase.State == State.Player){
            if(Input.GetButtonDown("Crouch")){
                ToggleCrouch();
            }
        }
    }

    private void ToggleCrouch(){
        if(this._isCrouching && !_canUncrouch)
            return;

        this._isCrouching = !this._isCrouching; 
        if(this._isCrouching) 
            EnableCrouch();
        else 
            DisableCrouch();
    }

    protected abstract void EnableCrouch();
    protected abstract void DisableCrouch();

    protected abstract bool CanUncrouch();
}
