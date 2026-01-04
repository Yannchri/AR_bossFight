using UnityEngine;
using UnityEngine.UI; // <--- INDISPENSABLE pour l'UI
using UnityEngine.SceneManagement;

public class BossHealth : MonoBehaviour
{
    [Header("Santé")]
    public float maxHealth = 100f; // float c'est mieux pour les divisions
    private float currentHealth;
    private float initialBarWidth;

    [Header("Configuration UI")]
    public GameObject healthCanvas; // L'objet Canvas entier
    public Image healthBarImage;    // L'image ROUGE qui va diminuer

    [Header("Feedback")]
    public Renderer bossRenderer;
    private Color originalColor;
    private BossController bossController;

    void Start()
    {
        currentHealth = maxHealth;

        bossController = GetComponent<BossController>();

        if (bossRenderer != null)
            originalColor = bossRenderer.material.color;

        if (healthBarImage != null)
            initialBarWidth = healthBarImage.rectTransform.sizeDelta.x;

        UpdateHealthBar();
    }

    void Update()
    {
        // ASTUCE : On force la barre de vie à regarder la caméra du joueur
        // Sinon elle tourne avec le boss et on ne la voit plus !
        if (healthCanvas != null)
        {
            healthCanvas.transform.LookAt(Camera.main.transform);
        }
    }

    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        Debug.Log($"Boss prend {damage} dégâts. HP = {currentHealth}/{maxHealth}");
        
        // Mise à jour visuelle
        UpdateHealthBar();
        
        // Clignotement
        if (bossRenderer != null) StartCoroutine(FlashRed());

        if (currentHealth <= 0) Die();
    }

    // Méthode pour compatibilité avec les sorts (SpellProjectile utilise ApplyDamage)
    public void ApplyDamage(float damage)
    {
        TakeDamage(damage);
    }

    void UpdateHealthBar()
    {
        if (healthBarImage == null)
        {
            Debug.LogWarning("healthBarImage is null! Cannot update health bar.");
            return;
        }

        float healthPercentage = Mathf.Clamp01(currentHealth / maxHealth);

        RectTransform rt = healthBarImage.rectTransform;
        rt.sizeDelta = new Vector2(initialBarWidth * healthPercentage, rt.sizeDelta.y);

        Debug.Log($"Health bar updated (SLICED): {healthPercentage * 100f}% | width = {rt.sizeDelta.x}");
    }


    void Die()
    {
        Debug.Log("BOSS DIED");
        
        // Changer l'état du boss
        if (bossController != null) 
            bossController.ChangeState(BossController.BossState.Dead);
        
        // Déclencher la victoire du joueur
        if (GameManager.Instance != null)
        {
            Debug.Log("BOSS MORT - Changement d'état vers BossDead");
            GameManager.Instance.SetState(GameState.BossDead);
        }
        
        // Charger la scène de victoire
        SceneManager.LoadScene("Winner");
        
        Destroy(gameObject, 2f);
    }
    
    System.Collections.IEnumerator FlashRed()
    {
        if (bossRenderer != null)
        {
            bossRenderer.material.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            bossRenderer.material.color = originalColor;
        }
    }

    // Méthodes publiques utiles
    public float GetCurrentHealth() => currentHealth;
    public float GetHealthPercentage() => currentHealth / maxHealth;
}