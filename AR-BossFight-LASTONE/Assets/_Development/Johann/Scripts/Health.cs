using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    private float _currentHealth;

    void Awake()
    {
        _currentHealth = maxHealth;
    }

    public void ApplyDamage(float amount)
    {
        _currentHealth -= amount;
        UnityEngine.Debug.Log($"{name} prend {amount} dégâts. HP = {_currentHealth}/{maxHealth}");

        if (_currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        UnityEngine.Debug.Log($"{name} est mort.");
        // Pour le cube de test :
        gameObject.SetActive(false);
        // Pour le boss plus tard, ils pourront mettre une anim, etc.
    }
}
