using UnityEngine;

public class BossAimAndShootForShield : MonoBehaviour
{
    public Transform targetHead; // Ta Main Camera
    public DamageUI damageUI;    // Ton Canvas
    public LineRenderer laserLine;

    void Update()
    {
        // 1. VISUEL : Le boss regarde le joueur (pour faire joli)
        if (targetHead != null)
        {
            transform.LookAt(targetHead);
        }

        // 2. TIR (Touche Entrée)
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ShootAtTarget();
        }
    }

    void ShootAtTarget()
    {
        if (targetHead == null) return;

        laserLine.enabled = true;

        // POINT DE DÉPART : Le centre du Boss (ou un peu devant)
        Vector3 startPoint = transform.position;
        laserLine.SetPosition(0, startPoint);

        // --- LA MAGIE MATHEMATIQUE ---
        // On calcule le vecteur exact qui relie le Boss à la Tête
        // Direction = Destination - Départ
        Vector3 exactDirection = (targetHead.position - startPoint).normalized;

        RaycastHit hit;

        // On tire le rayon dans cette Direction Exacte (pas juste "devant le boss")
        if (Physics.Raycast(startPoint, exactDirection, out hit, 100f))
        {
            laserLine.SetPosition(1, hit.point);

            // --- ANALYSE ---
            if (hit.collider.CompareTag("Shield"))
            {
                Debug.Log("🛡️ BLOQUÉ par le bouclier !");
            }
            else if (hit.collider.CompareTag("Player"))
            {
                Debug.Log("🔥 TÊTE TOUCHÉE (En plein dans le mille) !");
                if (damageUI != null) damageUI.ShowHitEffect();
            }
            else
            {
                Debug.Log("⚠️ Touché obstacle : " + hit.collider.name);
            }
        }
        else
        {
            // Si on ne touche rien (trop loin ?)
            laserLine.SetPosition(1, startPoint + exactDirection * 20f);
        }

        Invoke("HideLaser", 0.2f);
    }

    void HideLaser()
    {
        laserLine.enabled = false;
    }
}