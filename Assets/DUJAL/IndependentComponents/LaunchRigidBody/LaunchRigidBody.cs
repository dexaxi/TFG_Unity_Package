namespace DUJAL.IndependentComponents.LaunchRigidBody 
{
    using UnityEngine;

    public static class LaunchRigidBody
    {
        /// <summary>
        //  Static Function to add impulse to a Rigidbody2D, takes a rigidbody, Vector2 and a float Power, the Vector2 is normalized by the function. 
        /// </summary>
        public static void LaunchRigidBody2D(Rigidbody2D rigidbody, Vector2 launch, float power) 
        {
            rigidbody.AddForce(launch.normalized * power, ForceMode2D.Impulse);
        }

        public static void LaunchRigidBody2D(Rigidbody2D rigidbody, Vector2 launch, Vector2 power)
        {
            rigidbody.AddForce(new Vector2(launch.normalized.x * power.x, launch.normalized.y * power.y), ForceMode2D.Impulse);
        }

        /// <summary>
        //  Static Function to add impulse to a Rigidbody, takes a rigidbody, Vector3 and a float Power, the Vector3 is normalized by the function. 
        /// </summary>
        public static void LaunchRigidBody3D(Rigidbody rigidbody, Vector3 launch, float power) 
        {
            rigidbody.AddForce(launch.normalized * power, ForceMode.Impulse);
        }

        public static void LaunchRigidBody3D(Rigidbody rigidbody, Vector3 launch, Vector3 power)
        {
            rigidbody.AddForce(new Vector3(launch.normalized.x * power.x, launch.normalized.y * power.y, launch.normalized.z * power.z), ForceMode.Impulse);
        }
    }
}
