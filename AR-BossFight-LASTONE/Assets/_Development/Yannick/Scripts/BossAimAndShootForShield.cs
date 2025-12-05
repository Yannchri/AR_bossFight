using UnityEngine;
using System.Collections; // Nécessaire pour les Coroutines

public class BossAimAndShootForShield : MonoBehaviour
{
    [Header("Cibles et Visuels")]
    public Transform targetHead;
    public DamageUI damageUI;
    public LineRenderer laserLine;

    [Header("Animation")]
    public Animator bossAnimator; // GLISSE TON BOSS ICI
    public Transform laserOrigin; // Le point de départ du tir (le bâton ou l'œil)

    [Header("Réglages")]
    public float delayBeforeShot = 0.5f; // Temps pour synchroniser avec l'anim (à régler)

    void Update()
    {
        // 1. VISUEL : Le boss regarde le joueur
        if (targetHead != null)
        {
            // On tourne tout le boss vers le joueur
            transform.LookAt(new Vector3(targetHead.position.x, transform.position.y, targetHead.position.z));

            // Si tu as un objet "LaserOrigin" (ex: un oeil), tu peux le faire regarder aussi
            if (laserOrigin != null) laserOrigin.LookAt(targetHead);
        }

        // 2. TIR (Touche Entrée)
        if (Input.GetKeyDown(KeyCode.Return))
        {
            StartCoroutine(PrepareAttack());
        }
    }

    IEnumerator PrepareAttack()
    {
        // 1. On lance l'animation
        if (bossAnimator != null)
        {
            bossAnimator.SetTrigger("Shoot"); // Le nom du paramètre créé à l'étape 3
        }

        // 2. On attend que le bras se lève (règle le temps "delayBeforeShot" dans l'inspector)
        yield return new WaitForSeconds(delayBeforeShot);

        // 3. On tire le laser !
        ShootAtTarget();
    }

    void ShootAtTarget()
    {
        if (targetHead == null) return;

        laserLine.enabled = true;

        // POINT DE DÉPART : Priorité à laserOrigin, sinon le centre du boss
        Vector3 startPoint = (laserOrigin != null) ? laserOrigin.position : transform.position;

        laserLine.SetPosition(0, startPoint);

        Vector3 exactDirection = (targetHead.position - startPoint).normalized;
        RaycastHit hit;

        if (Physics.Raycast(startPoint, exactDirection, out hit, 100f))
        {
            laserLine.SetPosition(1, hit.point);

            if (hit.collider.CompareTag("Shield"))
            {
                Debug.Log("🛡️ BLOQUÉ par le bouclier !");
            }
            else if (hit.collider.CompareTag("Player"))
            {
                Debug.Log("🔥 TÊTE TOUCHÉE !");
                if (damageUI != null) damageUI.ShowHitEffect();
            }
        }
        else
        {
            laserLine.SetPosition(1, startPoint + exactDirection * 20f);
        }

        Invoke("HideLaser", 0.2f);
    }

    void HideLaser()
    {
        laserLine.enabled = false;
    }
}