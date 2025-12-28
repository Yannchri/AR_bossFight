using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    private Image healthFill;

    void Awake()
    {
        healthFill = GetComponent<Image>();

        Debug.Log(
            "[PlayerHealthUI][Awake]\n" +
            "- GameObject: " + gameObject.name + "\n" +
            "- Has Image: " + (healthFill != null) + "\n" +
            "- Active Self: " + gameObject.activeSelf + "\n" +
            "- Active In Hierarchy: " + gameObject.activeInHierarchy + "\n" +
            "- Parent: " + transform.parent?.name + "\n" +
            "- Root: " + transform.root.name
        );

        if (healthFill != null)
        {
            Debug.Log(
                "[PlayerHealthUI][Awake][Image]\n" +
                "- Type: " + healthFill.type + "\n" +
                "- FillAmount: " + healthFill.fillAmount + "\n" +
                "- Color: " + healthFill.color + "\n" +
                "- Alpha: " + healthFill.color.a + "\n" +
                "- Enabled: " + healthFill.enabled
            );
        }
    }

    public void UpdateHealth(float current, float max)
    {
        Debug.Log(
            "[PlayerHealthUI][UpdateHealth]\n" +
            "- Current: " + current + "\n" +
            "- Max: " + max
        );

        if (healthFill == null)
        {
            Debug.LogError("[PlayerHealthUI] healthFill IS NULL");
            return;
        }

        float value = current / max;

        healthFill.fillAmount = value;

        Debug.Log(
            "[PlayerHealthUI][Apply]\n" +
            "- New FillAmount: " + healthFill.fillAmount + "\n" +
            "- Image Type: " + healthFill.type + "\n" +
            "- Color: " + healthFill.color + "\n" +
            "- Alpha: " + healthFill.color.a + "\n" +
            "- Enabled: " + healthFill.enabled
        );

    }
}
