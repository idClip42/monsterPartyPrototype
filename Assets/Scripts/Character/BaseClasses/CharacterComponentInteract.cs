using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

#nullable enable

namespace MonsterParty
{
    [DisallowMultipleComponent]
    public abstract class CharacterComponentInteract : CharacterComponent
    {
        private IInteractible[] _interactibles = { };
        private IInteractible? _interactibleWithinReach = null;

        [SerializeField]
        private float _interactionDistance = 1.25f;

        [SerializeField]
        [Range(1, 20)]
        private float _gizmoMaxDistance = 5;

        public Vector3 ReferencePosition => this.transform.position + Vector3.up * 1.0f;

        public sealed override string DebugHeader => "Interaction";

        public sealed override void FillInDebugInfo(Dictionary<string, string> infoTarget)
        {
            infoTarget["Within Reach"] = _interactibleWithinReach != null ?
                _interactibleWithinReach.gameObject.name :
                "None";
        }

        protected sealed override void Awake()
        {
            base.Awake();

            _interactibles = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                .OfType<IInteractible>()
                .ToArray();
        }

        private void Update()
        {
            if (this.Character == null)
                throw new MonsterPartyNullReferenceException(this, "_characterBase");

            if (this.Character.State == Character.StateType.Player)
            {
                _interactibleWithinReach = GetInteractibleWithinReach();
                if (Input.GetButtonDown("Interact"))
                {
                    if (_interactibleWithinReach != null)
                    {
                        _interactibleWithinReach.DoInteraction(this.Character);
                        Debug.Log($"Character '{gameObject.name}' interacted with '{_interactibleWithinReach.gameObject.name}'.");
                    }
                }
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!enabled) return;
            if (this.Character == null) return;
            if (this.Character.State != Character.StateType.Player) return;

            if (_interactibleWithinReach != null)
            {
                using (new Handles.DrawingScope(Color.white))
                {
                    Handles.Label(
                        _interactibleWithinReach.InteractionWorldPosition + Vector3.up * 1.0f,
                        _interactibleWithinReach.GetInteractionName(this.Character)
                    );
                }
            }

            using (new Handles.DrawingScope(Color.white))
            {
                float gizmoMaxDistanceSqr = _gizmoMaxDistance * _gizmoMaxDistance;
                foreach (var interactible in _interactibles)
                {
                    if (interactible == null) throw new MonsterPartyNullReferenceException(this, "interactible");
                    if (interactible.gameObject == this.gameObject) continue;
                    if (interactible.IsInteractible(this.Character) == false) continue;
                    Vector3 diff = interactible.InteractionWorldPosition - ReferencePosition;
                    float distSqr = diff.sqrMagnitude;
                    if (distSqr > gizmoMaxDistanceSqr) continue;
                    Handles.DrawWireDisc(
                        interactible.InteractionWorldPosition,
                        Vector3.up,
                        _interactionDistance
                    );
                }
            }
        }
#endif

        private IInteractible? GetInteractibleWithinReach()
        {
            if (this.Character == null)
                throw new MonsterPartyNullReferenceException(this, "_characterBase");

            IInteractible? closest = null;
            float closestDistSq = float.MaxValue;

            foreach (var interactible in _interactibles)
            {
                if (interactible == null) continue;
                if (interactible.gameObject == this.gameObject) continue;
                if (interactible.IsInteractible(this.Character) == false) continue;
                Vector3 posDiff = interactible.InteractionWorldPosition - ReferencePosition;
                float distSq = posDiff.sqrMagnitude;
                if (distSq < closestDistSq)
                {
                    closestDistSq = distSq;
                    closest = interactible;
                }
            }

            if (Mathf.Sqrt(closestDistSq) > _interactionDistance)
                return null;

            return closest;
        }
    }
}