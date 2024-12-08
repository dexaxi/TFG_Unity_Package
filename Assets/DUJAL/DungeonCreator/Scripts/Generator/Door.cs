namespace DUJAL.DungeonCreator
{
    using DungeonCreator.Types;
    using UnityEngine;

    public class Door : MonoBehaviour
    {
        private MeshRenderer _renderer;

        private Collider _collider;

        public Direction.CardinalDirection Direction;

        private void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();
            _collider = GetComponent<Collider>();
        }

        public void EnableDoor()
        {
            ToggleDoor(true);
        }

        public void DisableDoor()
        {
            ToggleDoor(false);
        }

        public void ToggleDoor(bool enabled)
        {
            if (_renderer != null) _renderer.enabled = enabled;
            if (_collider != null) _collider.enabled = enabled;
        }
    }
}
