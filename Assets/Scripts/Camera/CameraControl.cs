using System;
using UnityEngine;

#nullable enable

[RequireComponent(typeof(Camera))]
[DisallowMultipleComponent]
public class CameraControl : MonoBehaviour
{
    private enum State { Orbit, Transition };

    [SerializeField] 
    private LayerMask _collisionLayers;
    [SerializeField] 
    private LayerMask _navMeshRaycastLayers;

    [SerializeField]
    private float _horizontalSpeed = 10;
    [SerializeField]
    private float _verticalSpeed = 8;
    [SerializeField]
    private float _distanceFromTarget = 5;
    [SerializeField]
    private float _targetHeightOffset = 1;
    [SerializeField]
    private float _verticalMin = -80;
    [SerializeField]
    private float _verticalMax = 80;
    [SerializeField]
    private float _transitionSpeed = 20;

    private CharacterManager? _characterManager = null;
    private NavigationManager? _navManager = null;

    private State _currentState = State.Orbit;
    private float _horizontalAngle = 0;
    private float _verticalAngle = 0;

    private CameraControlTransition? _transitioner = null;
    private Action? _transitionEndCallback = null;

    private void Awake()
    {
        _characterManager = FindFirstObjectByType<CharacterManager>();
        if(_characterManager == null)
            throw new Exception("Could not find character manager");

        _navManager = FindFirstObjectByType<NavigationManager>();
        if(_navManager == null)
            throw new Exception("Could not find NavigationManager");

        _transitioner = new CameraControlTransition(this, _navManager);
    }

    private void Update()
    {
        if(_currentState == State.Orbit){
            OrbitControl();
        }
        else if(_currentState == State.Transition){
            TransitionUpdate();
        }
    }

    private void OrbitControl(){
        if(_characterManager == null)
            throw new Exception("Null _characterManager");
            
        // TODO: This is for the non-physics movement of the CharacterController
        // TODO: If and when this changes, this'll need to move to FixedUpdate()
        if(_characterManager.SelectedCharacter != null){
            float mouseX = Input.GetAxisRaw("Mouse X");
            float mouseY = Input.GetAxisRaw("Mouse Y");
            _horizontalAngle += mouseX * _horizontalSpeed;
            _verticalAngle += mouseY * _verticalSpeed;
            _verticalAngle = Mathf.Clamp(_verticalAngle, _verticalMin, _verticalMax);

            Vector3 direction = GetOffsetDirection();
            Vector3 positionOffset = direction * _distanceFromTarget;
            Vector3 characterAxis = GetCharacterAxis(_characterManager.SelectedCharacter);
            Vector3 targetPosition = characterAxis + positionOffset;

            // Raycast from the target position to the camera
            RaycastHit hit;
            if (Physics.Raycast(
                characterAxis, 
                direction, 
                out hit, 
                _distanceFromTarget,
                _collisionLayers
            )) {
                var prevTgtPos = targetPosition;
                
                // If the ray hits something, adjust the camera position
                targetPosition = hit.point + -direction * 0.2f; // Push the camera slightly in front of the hit point

                Debug.DrawLine(
                    prevTgtPos,
                    targetPosition
                );
            }

            // Set the camera's position to the calculated position
            this.transform.position = targetPosition;
            this.transform.forward = -direction;
        }
    }

    private void TransitionUpdate(){
        if(_transitioner == null)
            throw new Exception("Null _transitioner");
        if(_transitionEndCallback == null)
            throw new Exception("Null _transitionEndCallback");

        bool endTransition = _transitioner.MoveCamera(Time.deltaTime);

        if(endTransition){
            _transitioner.Clear();
            _transitionEndCallback();
            _transitionEndCallback = null;
            _currentState = State.Orbit;
        }
    }

    public void SendCameraToNewCharacter(Character target, Action arrivalCallback){
        if(_transitioner == null)
            throw new Exception("Null _transitioner");
        
        _currentState = State.Transition;
        _transitioner.Initialize(
            target,
            GetOffsetDirection() * _distanceFromTarget,
            _transitionSpeed,
            _navMeshRaycastLayers
        );
        _transitionEndCallback = arrivalCallback;
    }

    public Vector3 GetCharacterAxis(Character target){
        Vector3 characterAxis = target.transform.position +
            Vector3.up * _targetHeightOffset;
        return characterAxis;
    }

    private Vector3 GetOffsetDirection()
    {
        return Quaternion.Euler(_verticalAngle, _horizontalAngle, 0) * Vector3.forward;
    }

    public sealed override bool Equals(object other) => base.Equals(other);
    public sealed override int GetHashCode() => base.GetHashCode();
    public sealed override string ToString() => base.ToString();
}
