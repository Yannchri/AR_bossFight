using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class BossHealth : MonoBehaviour
{
    [Header("Santé")]
    public float maxHealth = 100f;
    private float currentHealth;
    private float initialBarWidth;
    private bool isDead = false; // Sécurité pour ne pas mourir 2 fois

    [Header("Références (Auto-détectées si vides)")]
    public Animator animator;         // On va le trouver tout seul
    public BossController bossController; // Pour couper le cerveau

    [Header("Configuration UI")]
    public GameObject healthCanvas;
    public Image healthBarImage;

    [Header("Feedback Visuel")]
    public Renderer bossRenderer;
    private Color originalColor;

    void Start()
    {
        currentHealth = maxHealth;

        // --- 1. L'ASTUCE MAGIQUE ---
        // Si tu n'as pas rempli la case Animator, le script fouille dans les ENFANTS pour le trouver
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        // Idem pour le contrôleur
        if (bossController == null)
            bossController = GetComponent<BossController>();

        // Init visuelle
        if (bossRenderer != null) originalColor = bossRenderer.material.color;
        if (healthBarImage != null) initialBarWidth = healthBarImage.rectTransform.sizeDelta.x;

        UpdateHealthBar();
    }

    void Update()
    {
        if (healthCanvas != null && Camera.main != null)
        {
            healthCanvas.transform.LookAt(Camera.main.transform);
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return; // On ne tape pas un mort

        currentHealth -= damage;
        UpdateHealthBar();

        // Effet rouge
        if (bossRenderer != null) StartCoroutine(FlashRed());

        // Vérification de la mort
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Pour compatibilité avec tes sorts
    public void ApplyDamage(float damage) => TakeDamage(damage);

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("💀 LE BOSS EST MORT !");

        // 1. On lance l'animation (Grâce au GetComponentInChildren, ça marche !)
        if (animator != null)
            animator.SetTrigger("Die");

        // 2. On coupe le cerveau immédiatement
        if (bossController != null)
            bossController.ChangeState(BossController.BossState.Dead);

        // 3. On informe le GameManager (si tu en as un)
        // if (GameManager.Instance != null) GameManager.Instance.SetState(GameState.BossDead);

        // 4. On lance la séquence de fin (Attente + Victoire)
        StartCoroutine(VictorySequence());
    }

    IEnumerator VictorySequence()
    {
        // On attend 4 secondes (le temps que le boss tombe au sol)
        yield return new WaitForSeconds(4.0f);

        // On charge la scène de victoire
        SceneManager.LoadScene("Winner");
    }

    void UpdateHealthBar()
    {
        if (healthBarImage == null) return;
        float healthPercentage = Mathf.Clamp01(currentHealth / maxHealth);
        RectTransform rt = healthBarImage.rectTransform;
        rt.sizeDelta = new Vector2(initialBarWidth * healthPercentage, rt.sizeDelta.y);
    }

    IEnumerator FlashRed()
    {
        if (bossRenderer != null)
        {
            bossRenderer.material.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            bossRenderer.material.color = originalColor;
        }
    }
}