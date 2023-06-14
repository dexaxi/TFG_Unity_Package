namespace DUJAL.Systems.EXP 
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;

    public enum ExpType 
    {
        ExpType_Cubic,
        ExpType_Quadratic,
        ExpType_Parabolic,
        ExpType_Linear
    }

    public class Experience : MonoBehaviour
    {
        public const float XP_RATE_TOP_BOUND = 3f;

        [Header("Exp Settings")]
        [ContextMenuItem("Print All Level Values", "PrintLevel100Roadmap")]
        [SerializeField] public ExpType ExpGainType;
        
        [Tooltip("Experience Gain rate, a higher one will mean less necessary XP to get to max level")]
        [SerializeField] [Range(0.1f, XP_RATE_TOP_BOUND)]private float _xpRate;
        [SerializeField] [Range(1f, 1000f)] private int _minLevel;        
        [ContextMenuItem("Print Max XP", "PrintMaxXp")] 
        [SerializeField] [Range(1f, 1000f)] private int _maxLevel;
        

        [Header("Events")]
        [Space(4)]
        public UnityEvent OnExpGain;
        public UnityEvent OnLevelUp;
        public UnityEvent OnMaxLevelReached;

        private int  _currentLevel;

        private float _currentExp;
        private float _totalExp;
        private float _currentLevelUpThreshold;
        private float _auxRemainingExp;
        

        public float GetCurrentExp() { return _currentExp; }
        public float GetTotalExp() { return _totalExp; }
        public float GetCurrentLevel() { return _currentLevel; }
        public float GetMaxLevel() { return _maxLevel; }
        public float GetMaxExp() { return CalculateTotalXpForLevel(_maxLevel); }
        public float GetMinLevel() { return _minLevel; }
        public float GetLevelUpThreshhold() { return _currentLevelUpThreshold; }

        private void Start()
        {
            if (_currentLevel < _minLevel) _currentLevel = _minLevel;
            _currentLevelUpThreshold = CalculateLevelUpThreshold(_currentLevel);
        }

        private void LevelUp(int amount = 1)
        {
            if (amount == 0) return;
            
            //Update level by amount
            _currentLevel += amount;
            _currentLevelUpThreshold = CalculateLevelUpThreshold(_currentLevel);
            OnLevelUp.Invoke();

            // if max level, don't level up
            if (_currentLevel >= _maxLevel) 
            {
                _currentLevel = _maxLevel;
                OnMaxLevelReached.Invoke();
                return;
            }
            
            // If grant exp granted more exp than the current level allows, it is stored on _auxRemainingExp, and is then granted
            if (_auxRemainingExp != 0) 
            {
                GrantExp(_auxRemainingExp);
            }
        }

        /// <summary>
        /// Public method to grant xp
        /// </summary>
        /// <param name="amount">XP Quantity</param>
        public void GrantExp(float amount) 
        {
            //We update _totalExp and fire event
            _totalExp += amount;
            OnExpGain.Invoke();

            //If we can level up, we do, but first we store any possible remainding exp that should be granted after level up.
            //If we know that we're going to level up, currentExp = current theshold, this way we prevent bugs on UI in the
            //case that we grant more exp than the thershold. Then we level up. If we dont, we just add the exp.
            if (_currentExp + amount >= _currentLevelUpThreshold)
            {
                _auxRemainingExp = (_currentExp + amount) - _currentLevelUpThreshold;
                _currentExp = _currentLevelUpThreshold;
                _currentExp = _currentLevel == _maxLevel ? _currentLevelUpThreshold : 0;
                LevelUp();
            }
            else _currentExp += amount;
        }

        /// <summary>
        /// Function to level up exactly once, granting the exact amount so that there is no remainder
        /// </summary>
        public void GrantExpForOneLevel() 
        {
            GrantExp(_currentLevelUpThreshold - _currentExp);
        }

        /// <summary>
        /// Depending on what ExpGainType is selected, the exp gain for this component will be linear, quadratic, cubic or parabolic
        /// </summary>
        /// <param name="level"> Level you want to calculate the threshold for</param>
        /// <returns>Updated Threshold for current level</returns>
        private float CalculateLevelUpThreshold(float level) 
        {
            if (level == 1) return CalculateTotalXpForLevel(2);
            return CalculateTotalXpForLevel(level + 1) - CalculateTotalXpForLevel(level);
        }

        /// <summary>
        /// Function to calculate total xp for level
        /// </summary>
        /// <param name="level"> Level you want to calculate the total xp for</param>
        /// <returns>Total xp required for level</returns>
        public float CalculateTotalXpForLevel(float level) 
        {
            if (level < _minLevel) return 0;
            float totalXpForNewLevel;
            switch (ExpGainType)
            {
                case ExpType.ExpType_Linear:
                    totalXpForNewLevel = (XP_RATE_TOP_BOUND - _xpRate) * (level);
                    break;

                case ExpType.ExpType_Quadratic:
                    totalXpForNewLevel = (XP_RATE_TOP_BOUND - _xpRate) * (level * level);
                    break;

                case ExpType.ExpType_Cubic:
                default:
                    totalXpForNewLevel = (XP_RATE_TOP_BOUND - _xpRate) * (level * level * level);
                    break;

                case ExpType.ExpType_Parabolic:
                    totalXpForNewLevel = (XP_RATE_TOP_BOUND - _xpRate) * (1.2f * (level * level * level) - 15 * (level * level) + 100 * level - 140);
                    break;
            }
            return RoundToTwoDP(totalXpForNewLevel);
        }

        /// <summary>
        /// Set this entity to min level
        /// </summary>
        public void ResetEntityExp() 
        {
            _currentExp = 0;
            _currentLevel = _minLevel;
            _totalExp = 0;
            _currentLevelUpThreshold = CalculateLevelUpThreshold(_currentLevel);
        }

        private float RoundToTwoDP(float input) 
        {
            return (float) Math.Round(input, 2);
        }
        
        //DEBUG
        private void PrintLevel100Roadmap()
        {
            while (_currentLevel < _maxLevel) 
            { 
                Debug.Log("Current Level: " + _currentLevel + " Total Exp: " + _totalExp + " CurrentXp:" + _currentExp + " Current threhshold: " + _currentLevelUpThreshold);
                GrantExpForOneLevel();
            }
            Debug.Log("Current Level: " + _currentLevel + " Total Exp: " + _totalExp + " CurrentXp:" + _currentExp);
            //ResetEntityExp();
        }

        private void PrintMaxXp() 
        {
            Debug.Log(GetMaxExp());
        }
    }
}
