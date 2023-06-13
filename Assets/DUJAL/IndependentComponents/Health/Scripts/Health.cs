namespace DUJAL.IndependentComponents.Health 
{
    using UnityEngine;
    using UnityEngine.Events;

    public class Health : MonoBehaviour
    {
        [Header("Health Parameters")]
        [SerializeField] private float _maxHealth;
        [SerializeField] private float _minHealth;
        [SerializeField] private bool _healOnStart;
        
        [Header("Events")]
        [Space(4)]
        public UnityEvent OnHealthUpdated;
        public UnityEvent OnDamageDealt;
        public UnityEvent OnHeal;
        public UnityEvent OnFullHeal;
        public UnityEvent OnDeath;
        
        private float _currentHealth;

        private void Start()
        {
            if(_healOnStart) HealMaxHealth();
        }

        public float GetHealth() { return _currentHealth; }
        public float GetMaxHealth() { return _maxHealth; }
        public float GetMinHealth() { return _minHealth; }
        public float GetHealthPercent() { return (_currentHealth  - _minHealth)/ (_maxHealth - _minHealth) * 100; }
        public float GetHealthDecimal() { return (_currentHealth  - _minHealth) / (_maxHealth - _minHealth); }

        public void SetMaxHealth(float newHealth) 
        {
            _maxHealth = newHealth;
        } 
        
        public void SetMinHealth(float newHealth) 
        {
            _minHealth = newHealth;
        }

        public void DealDamage(float damage) 
        {
            _currentHealth -= damage;
            OnHealthUpdated.Invoke();
            OnDamageDealt.Invoke();
            if (_currentHealth <= _minHealth)
            {
                OnDeath.Invoke();
                _currentHealth = _minHealth;
            }
        }

        public void DealOneDamage() 
        {
            DealDamage(1);
        }

        public void Kill() 
        {
            DealDamage(_maxHealth);
        }

        public void Heal(float heal) 
        {
            _currentHealth += heal;
            OnHeal.Invoke();
            OnHealthUpdated.Invoke();
            if (_currentHealth >= _maxHealth) 
            {
                OnFullHeal.Invoke();
                _currentHealth = _maxHealth;
            }
        }

        public void HealMaxHealth() 
        {
            Heal(_maxHealth);
        }
    }
}
