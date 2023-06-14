namespace DUJAL.Systems.EXP
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Set desired multiplier as 100 * value.
    /// </summary>
    public enum ExperienceMultiplierEventType 
    {
        ExperienceMultiplierEventType_Default = 100,
        ExperienceMultiplierEventType_ExampleType1 = 200,
        ExperienceMultiplierEventType_ExampleType2 = 75,
        ExperienceMultiplierEventType_ExampleType3 = 25,
    }

    public class ExperienceMobHanlder : MonoBehaviour
    {
        public static ExperienceMobHanlder Instance { get; private set; }

        [SerializeField] private List<ExperienceMobAsset> _experienceMobList;

        public Dictionary<string, float> _experienceMobDictionary;

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

            _experienceMobDictionary = new Dictionary<string, float>();
            foreach (ExperienceMobAsset mob in _experienceMobList) 
            {
                _experienceMobDictionary[mob.name.ToLower()] = mob.MobBaseExp;
            }
        }

        /// <summary>
        /// Get experience points given by a given mob - A mob can be either an enemy or an event that grants Experience.
        /// </summary>
        /// <param name="name"> Name of the mob in the dictionary</param>
        /// <returns>Experience Points</returns>
        public float GetMobXp(string name) 
        {
            return _experienceMobDictionary[name.ToLower()];
        }
        
        /// <summary>
        /// Function to calculate Pondered xp that can be granted to the player.
        /// </summary>
        /// <param name="name"> Mob name whose xp will be consulter</param>
        /// <param name="multiplierType">Multiplier type that will be processed as the base xp multiplier, defaul 1</param>
        /// <param name="otherMultipliers">Other multipliears that can be added by the user. Default 1</param>
        /// <returns></returns>
        public float CalculatePonderedXp(string name, ExperienceMultiplierEventType multiplierType = ExperienceMultiplierEventType.ExperienceMultiplierEventType_Default, float otherMultipliers = 1)
        {
            float baseXp = GetMobXp(name);

            return baseXp * CalculateExperienceMultiplierEventMultiplier(multiplierType) * otherMultipliers;
        }

        /// <summary>
        /// Returns Experience Multiplier for each event type.
        /// </summary>
        private float CalculateExperienceMultiplierEventMultiplier(ExperienceMultiplierEventType eventType) 
        {
            return (float) eventType / 100f;
        }
    }
}