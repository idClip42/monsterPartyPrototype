using UnityEditor;
using UnityEngine;

#nullable enable

namespace MonsterParty
{
    public class Door : MonoBehaviour, IInteractible
    {
        private enum DoorState { Closed, OpenForward, OpenBackward }

        [SerializeField]
        private Transform? _axis;

        [SerializeField]
        private Transform? _interactor;

        [SerializeField]
        private OcclusionPortal? _occlusionPortal;

        [SerializeField]
        private DoorState _startState = DoorState.Closed;

        [SerializeField]
        private AudioSource? _doorOpenSoundEffect = null;
        [SerializeField]
        private AudioSource? _doorCloseSoundEffect = null;

        public Vector3 InteractionWorldPosition => _interactor ? _interactor.position : transform.position;

        private Quaternion _baseRotation;
        private DoorState _state = DoorState.Closed;

        public bool IsOpen
        {
            get
            {
                if (this._state == DoorState.Closed)
                    return false;
                return true;
            }
        }

        void Awake()
        {
            if (_axis == null)
                throw new MonsterPartyNullReferenceException(this, nameof(_axis));
            if (_interactor == null)
                throw new MonsterPartyNullReferenceException(this, nameof(_interactor));
            if (_occlusionPortal == null)
                throw new MonsterPartyNullReferenceException(this, nameof(_occlusionPortal));

            _baseRotation = _axis.rotation;

            SetDoorState(_startState);
        }

        public bool IsInteractible(Character interactor)
        {
            return true;
        }

        public void DoInteraction(Character interactor)
        {
            if (_state == DoorState.Closed)
            {
                OpenDoor(interactor.transform);
            }
            else
            {
                CloseDoor();
            }
        }

        public string GetInteractionName(Character interactor)
        {
            if (_state == DoorState.Closed)
                return "Open";
            else
                return "Close";
        }

        public void OpenDoor(Transform opener)
        {
            if (_axis == null)
                throw new MonsterPartyNullReferenceException(this, nameof(_axis));

            Vector3 doorToChar = opener.position - this.transform.position;
            Vector3 doorForward = _axis.forward;
            float dotProduct = Vector3.Dot(doorToChar, doorForward);
            if (dotProduct > 0)
            {
                SetDoorState(DoorState.OpenBackward);
            }
            else
            {
                SetDoorState(DoorState.OpenForward);
            }

            if (_doorOpenSoundEffect)
                _doorOpenSoundEffect.Play();
            else
                Debug.LogWarning("Missing door open sound effect");

            Debug.Log($"Door '{gameObject.name}' opened.");
        }

        public void CloseDoor()
        {
            SetDoorState(DoorState.Closed);

            if (_doorCloseSoundEffect)
                _doorCloseSoundEffect.Play();
            else
                Debug.LogWarning("Missing door close sound effect");

            Debug.Log($"Door '{gameObject.name}' closed.");
        }

        private void SetDoorState(DoorState newState)
        {
            if (_axis == null)
                throw new MonsterPartyNullReferenceException(this, nameof(_axis));
            if (_baseRotation == null)
                throw new MonsterPartyNullReferenceException(this, nameof(_baseRotation));
            if (_occlusionPortal == null)
                throw new MonsterPartyNullReferenceException(this, nameof(_occlusionPortal));

            switch (newState)
            {
                case DoorState.Closed:
                    _axis.rotation = _baseRotation;
                    _occlusionPortal.open = false;
                    break;
                case DoorState.OpenForward:
                    _axis.rotation = _baseRotation * Quaternion.Euler(0, 90, 0);
                    _occlusionPortal.open = true;
                    break;
                case DoorState.OpenBackward:
                    _axis.rotation = _baseRotation * Quaternion.Euler(0, -90, 0);
                    _occlusionPortal.open = true;
                    break;
                default:
                    throw new MonsterPartyUnhandledEnumException<DoorState>(newState);
            }
            ;

            _state = newState;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_axis == null || _interactor == null)
            {
                using (new Handles.DrawingScope(Color.red))
                {
                    Handles.DrawSolidDisc(transform.position, Vector3.up, 1);
                }
                return;
            }

            Quaternion rotation;
            if (Application.IsPlaying(this))
                rotation = _baseRotation;
            else
                rotation = _axis.rotation;
            Vector3 rotationDirection = rotation * Vector3.back; // ??? Why is "back" the correct one?

            using (new Handles.DrawingScope())
            {
                Handles.DrawWireArc(
                    _axis.position,
                    Vector3.up,
                    rotationDirection,
                    180,
                    1
                );
                Handles.DrawLine(
                    _axis.position,
                    _axis.position + Vector3.up * 2
                );

                Handles.DrawWireCube(_interactor.position, Vector3.one * 0.1f);

                if (Application.IsPlaying(this) == false)
                {
                    Handles.Label(
                        transform.position,
                        _startState.ToString()
                    );
                }
            }
        }
#endif

        public sealed override bool Equals(object other) => base.Equals(other);
        public sealed override int GetHashCode() => base.GetHashCode();
        public sealed override string ToString() => base.ToString();
    }
}