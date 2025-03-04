using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraControl : MonoBehaviour
{
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

    private CharacterManager _characterManager;

    private float _horizontalAngle = 0;
    private float _verticalAngle = 0;

    void Awake()
    {
        _characterManager = FindFirstObjectByType<CharacterManager>();
        if(_characterManager == null)
            throw new System.Exception("Could not find character manager");
    }

    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");
        _horizontalAngle += mouseX * _horizontalSpeed;
        _verticalAngle += mouseY * _verticalSpeed;
        _verticalAngle = Mathf.Clamp(_verticalAngle, _verticalMin, _verticalMax);

        // TODO: This is for the non-physics movement of the CharacterController
        // TODO: If and when this changes, this'll need to move to FixedUpdate()
        if(_characterManager.SelectedCharacter != null){
            Vector3 direction = GetDirectionFromYawPitch();
            Vector3 positionOffset = direction * _distanceFromTarget;
            Vector3 characterAxis = _characterManager.SelectedCharacter.transform.position +
                Vector3.up * _targetHeightOffset;
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

    Vector3 GetDirectionFromYawPitch()
    {
        return Quaternion.Euler(_verticalAngle, _horizontalAngle, 0) * Vector3.forward;
    }
}
