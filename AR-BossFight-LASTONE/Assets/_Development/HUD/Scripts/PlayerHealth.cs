using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance; // 🔑 LIGNE MANQUANTE

    public float maxHealth = 100f;
    public float currentHealth;

    [Header("UI")]
    [SerializeField] PlayerHealthUI healthUI;
    [SerializeField] Transform playerHead;

    void Awake()
    {
        Instance = this;

        if (playerHead == null && Camera.main != null)
            playerHead = Camera.main.transform;

        if (healthUI == null)
            healthUI = FindObjectOfType<PlayerHealthUI>();

        Debug.Log("HealthUI found: " + (healthUI != null));

    }

    public void RegisterUI(PlayerHealthUI ui)
    {
        healthUI = ui;
        UpdateUI();
        Debug.Log("PlayerHealthUI registered");
    }

    void Start()
    {
        currentHealth = maxHealth;
    }

    public Vector3 GetPlayerPosition()
    {
        return playerHead != null ? playerHead.position : transform.position;
    }

    public void TakeDamage(float damage)
    {
        Debug.Log($"[PlayerHealth] TakeDamage({damage}) BEFORE = {currentHealth}");

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log($"[PlayerHealth] AFTER = {currentHealth}");

        UpdateUI();

        if (currentHealth <= 0)
            Die();
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
        UpdateUI();
    }

    void UpdateUI()
    {
        if (healthUI == null)
        {
            Debug.LogWarning("PlayerHealthUI NOT assigned");
            return;
        }

        healthUI.UpdateHealth(currentHealth, maxHealth);
    }

    void Die()
    {
        Debug.Log("<color=red>Player Dead</color>");
    }
}
