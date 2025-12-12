using UnityEngine;
using System.Collections;
using TMPro; // Si tu utilises TextMeshPro

public class DamageUI : MonoBehaviour
{
    [Header("Messages UI")]
    public GameObject hitMessage;   // Le texte "AIE !"
    public GameObject blockMessage; // Le texte "BLOQUÉ !"

    // Singleton pour accès facile depuis partout
    public static DamageUI instance;

    void Awake()
    {
        instance = this;
    }

    public void ShowHitEffect()
    {
        StopAllCoroutines();
        StartCoroutine(FlashMessage(hitMessage));
    }

    public void ShowBlockEffect()
    {
        StopAllCoroutines();
        StartCoroutine(FlashMessage(blockMessage));
    }

    IEnumerator FlashMessage(GameObject messageObj)
    {
        if (messageObj != null)
        {
            messageObj.SetActive(true);
            yield return new WaitForSeconds(1.0f); // Affiche 1 seconde
            messageObj.SetActive(false);
        }
    }
}