namespace DUJAL.MovementComponents 
{
    using System.Linq;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.Controls;

    public class MovementComponent : MonoBehaviour
    {
        public MovementInput MovementMap { get; protected set; }
        public bool UseMouse { get; private set; }
        public Vector2 MovementInput;

        protected void HandleDeviceChange()
        {
            if ((Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
                || (Mouse.current.delta.ReadValue() != Vector2.zero || Mouse.current.leftButton.wasPressedThisFrame))
            {
                UseMouse = true;
            }
            else if (Gamepad.current != null &&
                (Gamepad.current.allControls.Any(x => x is ButtonControl button && x.IsPressed() && !x.synthetic)
                || Gamepad.current.allControls.Any(y => y is StickControl stick && y.IsActuated() && !y.synthetic)))
            {
                UseMouse = false;
            }
        }
    }
}
