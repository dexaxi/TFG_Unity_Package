namespace DUJAL.Systems.EXP
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "New Experience Mob", menuName = "DUJAL/Experience Mob")]
    public class ExperienceMobAsset : ScriptableObject
    {
        [Header("Experience Mob Settings")]
        public new string name;
        public float MobBaseExp;
    }
}