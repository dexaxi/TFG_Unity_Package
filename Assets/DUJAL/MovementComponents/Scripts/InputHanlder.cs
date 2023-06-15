namespace DUJAL.MovementComponents
{
    using UnityEngine;

    public class InputHanlder : MonoBehaviour
    {
        public static InputHanlder Instance { get; private set;}
        private void Awake()
        {
            //Make Singleton
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        public void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void UnlockCursor() 
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void LockInput() 
        {
            MovementComponent[] movementComponents = FindObjectsOfType<MovementComponent>();
            foreach (MovementComponent m in movementComponents)
            {
                m.MovementMap.Disable();
            }
        }

        public void UnlockInput() 
        {
            MovementComponent[] movementComponents = FindObjectsOfType<MovementComponent>();
            foreach (MovementComponent m in movementComponents) 
            {
                m.MovementMap.Enable();
            }
        }
    }
}
