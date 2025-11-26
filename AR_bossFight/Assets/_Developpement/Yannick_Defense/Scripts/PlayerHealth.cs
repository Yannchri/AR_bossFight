using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Configuration")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Dépendances")]
    public ShieldController shieldController; // Pour vérifier si on bloque

    [Header("Events (Pour l'UI)")]
    // Cet event enverra le % de vie (entre 0 et 1) à la barre de vie
    public UnityEvent<float> onHealthChanged;
    public UnityEvent onDamageTaken;
    public UnityEvent onDeath;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    // Fonction à appeler pour blesser le joueur
    public void TakeDamage(int damageAmount)
    {
        // 1. Vérifier le bouclier
        if (shieldController != null && shieldController.IsBlocking())
        {
            Debug.Log("🛡️ BLOQUÉ ! Pas de dégâts.");
            return; // On arrête tout, pas de dégâts
        }

        // 2. Appliquer les dégâts
        currentHealth -= damageAmount;
        Debug.Log($"Aïe ! Reste {currentHealth} PV");

        // 3. Feedback
        onDamageTaken?.Invoke();
        UpdateUI();

        // 4. Vérifier la mort
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    public void Heal(int healAmount)
    {
        currentHealth += healAmount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Convertit la vie en pourcentage (ex: 80/100 = 0.8)
        float ratio = (float)currentHealth / maxHealth;
        onHealthChanged?.Invoke(ratio);
    }

    private void Die()
    {
        Debug.Log("💀 GAME OVER");
        onDeath?.Invoke();
    }
}