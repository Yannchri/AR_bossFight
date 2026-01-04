using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    private float _currentHealth;

    [Header("Boss Detection")]
    [Tooltip("Si activé, la mort de ce GameObject déclenchera la victoire du joueur (pour le boss).")]
    public bool isBoss = false;

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
        
        // Si c'est le boss, on déclenche l'état de victoire
        if (isBoss)
        {
            if (GameManager.Instance != null)
            {
                UnityEngine.Debug.Log("BOSS MORT - Changement d'état vers BossDead");
                GameManager.Instance.SetState(GameState.BossDead);
            }
            
            // Informer le BossController si présent
            BossController bossController = GetComponent<BossController>();
            if (bossController != null)
            {
                bossController.ChangeState(BossController.BossState.Dead);
            }
        }
        
        // Pour les ennemis normaux, on désactive simplement
        gameObject.SetActive(false);
    }

    // Méthode publique pour obtenir la santé actuelle
    public float GetCurrentHealth()
    {
        return _currentHealth;
    }

    // Méthode publique pour obtenir le pourcentage de santé
    public float GetHealthPercentage()
    {
        return _currentHealth / maxHealth;
    }
}
