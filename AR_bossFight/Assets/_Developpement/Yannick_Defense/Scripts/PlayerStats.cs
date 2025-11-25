using UnityEngine;
using UnityEngine.Events;

public class PlayerStats : MonoBehaviour
{
    [Header("Configuration")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Events (Drag & Drop ici)")]
    // L'UI s'abonnera à cet event pour mettre à jour la barre de vie
    public UnityEvent<float> onHealthChanged;
    public UnityEvent onDeath;
    public UnityEvent onDamageTaken;

    void Start()
    {
        currentHealth = maxHealth;
        // Initialise l'UI au début
        onHealthChanged?.Invoke(1f);
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        // Calcul du pourcentage de vie (0.0 à 1.0)
        float healthPercent = (float)currentHealth / (float)maxHealth;

        onHealthChanged?.Invoke(healthPercent);
        onDamageTaken?.Invoke();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        float healthPercent = (float)currentHealth / (float)maxHealth;
        onHealthChanged?.Invoke(healthPercent);
    }

    private void Die()
    {
        Debug.Log("💀 Le joueur est mort !");
        onDeath?.Invoke();
    }
}