using UnityEngine;
using TMPro; // Ou UnityEngine.UI si tu utilises l'ancien texte
using System.Collections;

public class DamageUI : MonoBehaviour
{
    public GameObject hitMessage; // Glisse le texte ici
    public GameObject blockMessage;

    public void ShowHitEffect()
    {
        StopAllCoroutines(); // Reset si on est touché plein de fois
        StartCoroutine(FlashMessage());
    }

    IEnumerator FlashMessage()
    {
        hitMessage.SetActive(true);
        yield return new WaitForSeconds(1.0f); // Reste affiché 1 seconde
        hitMessage.SetActive(false);
    }
}