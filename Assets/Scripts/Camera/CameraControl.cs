using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraControl : MonoBehaviour
{
    private enum State { Orbit, Transition };

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

    private CharacterManager _characterManager;

    private State _currentState = State.Orbit;
    private float _horizontalAngle = 0;
    private float _verticalAngle = 0;

    private CameraControlTransition _transitioner;
    private Action _transitionEndCallback;

    void Awake()
    {
        _characterManager = FindFirstObjectByType<CharacterManager>();
        if(_characterManager == null)
            throw new Exception("Could not find character manager");

        _transitioner = new CameraControlTransition(this);
    }

    void Update()
    {
        if(_currentState == State.Orbit){
            OrbitControl();
        }
        else if(_currentState == State.Transition){
            bool endTransition = _transitioner.MoveCamera(Time.deltaTime);
            if(endTransition){
                _transitioner.Clear();
                _transitionEndCallback();
                _transitionEndCallback = null;
                _currentState = State.Orbit;
            }
        }
    }

    private void OrbitControl(){
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

            // TODO: If the player character has colliders on it,
            // TODO: This will be screwy. We'll need to figure out layers.

            // Raycast from the target position to the camera
            RaycastHit hit;
            if (Physics.Raycast(characterAxis, direction, out hit, _distanceFromTarget))
            {
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

    public void SendCameraToNewCharacter(CharacterBase target, Action arrivalCallback){
        _currentState = State.Transition;
        _transitioner.Initialize(
            target,
            GetOffsetDirection() * _distanceFromTarget,
            _transitionSpeed
        );
        _transitionEndCallback = arrivalCallback;
    }

    public Vector3 GetCharacterAxis(CharacterBase target){
        Vector3 characterAxis = target.transform.position +
            Vector3.up * _targetHeightOffset;
        return characterAxis;
    }

    private Vector3 GetOffsetDirection()
    {
        return Quaternion.Euler(_verticalAngle, _horizontalAngle, 0) * Vector3.forward;
    }
}
