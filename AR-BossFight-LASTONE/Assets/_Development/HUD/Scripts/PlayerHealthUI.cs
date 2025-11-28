using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealthUI : MonoBehaviour
{
    public Image healthFill;

    [Range(0f, 100f)]
    public float currentHealth = 100f;

    void Start()
    {
        StartCoroutine(TestSequence());
    }

    IEnumerator TestSequence()
    {
        UpdateHealth(100);
        yield return new WaitForSeconds(1f);

        UpdateHealth(50);
        yield return new WaitForSeconds(1f);

        UpdateHealth(10);
    }

    public void UpdateHealth(float value)
    {
        currentHealth = Mathf.Clamp(value, 0f, 100f);
        healthFill.fillAmount = currentHealth / 100f;
        healthFill.color = Color.Lerp(Color.red, Color.green, currentHealth / 100f);
    }
}
