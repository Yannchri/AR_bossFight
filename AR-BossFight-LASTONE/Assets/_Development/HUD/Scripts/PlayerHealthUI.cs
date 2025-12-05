using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public Image healthFill;

    public void UpdateHealth(float current, float max)
    {
        if (!healthFill) return;

        float ratio = current / max;
        healthFill.fillAmount = ratio;
        healthFill.color = Color.Lerp(Color.red, Color.green, ratio);
    }
}
