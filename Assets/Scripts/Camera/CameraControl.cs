using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraControl : MonoBehaviour
{
    [SerializeField]
    private float horizontalSpeed = 10;
    [SerializeField]
    private float verticalSpeed = 8;
    [SerializeField]
    private float distanceFromTarget = 5;
    [SerializeField]
    private float targetHeightOffset = 1;
    [SerializeField]
    private float verticalMin = -80;
    [SerializeField]
    private float verticalMax = 80;

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
        _horizontalAngle += mouseX * horizontalSpeed;
        _verticalAngle += mouseY * verticalSpeed;
        _verticalAngle = Mathf.Clamp(_verticalAngle, verticalMin, verticalMax);

        // TODO: This is for the non-physics movement of the CharacterController
        // TODO: If and when this changes, this'll need to move to FixedUpdate()
        if(_characterManager.SelectedCharacter != null){
            Vector3 direction = GetDirectionFromYawPitch();
            Vector3 positionOffset = direction * distanceFromTarget;
            Vector3 position = _characterManager.SelectedCharacter.transform.position +
                Vector3.up * targetHeightOffset +
                positionOffset;
            this.transform.position = position;
            this.transform.forward = -direction;
        }
    }

    Vector3 GetDirectionFromYawPitch()
    {
        return Quaternion.Euler(_verticalAngle, _horizontalAngle, 0) * Vector3.forward;
    }
}
