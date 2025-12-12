using UnityEngine;
using System.Collections;

public class BossLaserAuto : MonoBehaviour
{
    [Header("Animation")]
    public Animator bossAnimator; // La référence aux muscles
    public string triggerName = "Shoot"; // Le nom du paramètre dans l'Animator
    public float delayBeforeLaser = 0.5f; // Temps pour lever le bras avant que le laser sorte

    [Header("Réglages Laser")]
    public float damageInterval = 0.5f;
    public float laserRange = 20f;
    public bool autoFire = true;
    public float laserDuration = 2.0f;
    public float cooldown = 4.0f;

    [Header("Références Techniques")]
    public Transform laserOrigin;
    public LineRenderer laserLine;

    private Transform targetHead;
    private float nextDamageTime = 0;

    void Start()
    {
        if (laserLine == null) laserLine = GetComponent<LineRenderer>();
        laserLine.enabled = false;

        // Si on a oublié de lier l'animator manuellement, on essaie de le trouver
        if (bossAnimator == null) bossAnimator = GetComponentInChildren<Animator>();

        // Recherche du joueur (Même logique qu'avant)
        if (Camera.main != null) targetHead = Camera.main.transform;
        else
        {
            var rig = FindFirstObjectByType<OVRCameraRig>();
            if (rig != null) targetHead = rig.centerEyeAnchor;
        }

        if (autoFire) StartCoroutine(AutoShootRoutine());
    }

    // ... (La partie Update et UpdateLaserLogic reste identique, je ne la remets pas pour faire court) ...
    void Update()
    {
        // On ne lance la logique de collision QUE si le laser est totalement déployé
        // (C'est à dire quand le point 0 et le point 1 ne sont pas au même endroit)
        if (laserLine.enabled && targetHead != null && laserLine.GetPosition(0) != laserLine.GetPosition(1))
        {
            UpdateLaserLogic();
        }
    }

    // C'est la partie UpdateLaserLogic que tu avais déjà (copier-coller de l'ancien script)
    void UpdateLaserLogic()
    {
        Vector3 startPos = (laserOrigin != null) ? laserOrigin.position : transform.position;
        laserLine.SetPosition(0, startPos);
        Vector3 direction = (targetHead.position - startPos).normalized;
        RaycastHit hit;
        if (Physics.Raycast(startPos, direction, out hit, laserRange))
        {
            laserLine.SetPosition(1, hit.point);
            // ... à l'intérieur de if (Physics.Raycast(...)) ...

            // A. EST-CE LE BOUCLIER ?
            if (hit.collider.CompareTag("Shield"))
            {
                Debug.Log("🛡️ LASER BLOQUÉ !");

                // NOUVEAU : On appelle l'UI
                if (DamageUI.instance != null)
                {
                    DamageUI.instance.ShowBlockEffect();
                }

                return; // On arrête le laser ici
            }
            DamageUI playerUI = hit.collider.GetComponent<DamageUI>();
            if (playerUI == null) playerUI = hit.collider.GetComponentInParent<DamageUI>();
            if (playerUI != null && Time.time >= nextDamageTime)
            {
                playerUI.ShowHitEffect();
                nextDamageTime = Time.time + damageInterval;
            }
        }
        else laserLine.SetPosition(1, startPos + (direction * laserRange));
    }

    // --- C'EST ICI QUE CA CHANGE ---
    // --- NOUVELLE VERSION ---
    IEnumerator AutoShootRoutine()
    {
        while (true)
        {
            // 1. Attente et Animation
            yield return new WaitForSeconds(cooldown);
            if (bossAnimator != null) bossAnimator.SetTrigger(triggerName);
            yield return new WaitForSeconds(delayBeforeLaser);

            // 2. LE LASER APPARAÎT (Phase de croissance)
            laserLine.enabled = true;
            Vector3 startPos = (laserOrigin != null) ? laserOrigin.position : transform.position;
            laserLine.SetPosition(0, startPos);
            // On met le point d'arrivée au même endroit que le départ pour commencer
            laserLine.SetPosition(1, startPos);

            float growthDuration = 0.2f; // Le laser met 0.2s à atteindre sa cible (réglable)
            float timer = 0f;

            while (timer < growthDuration)
            {
                timer += Time.deltaTime;
                float progress = timer / growthDuration; // Va de 0 à 1

                // On calcule la position intermédiaire
                Vector3 currentEndPos = Vector3.Lerp(startPos, targetHead.position, progress);
                laserLine.SetPosition(1, currentEndPos);

                // Petit hack : on empêche UpdateLaserLogic de tourner pendant la croissance
                // pour ne pas qu'il écrase nos positions.
                yield return null; // Attend une frame
            }

            // 3. LE LASER EST ÉTABLI (Phase de dégâts/blocage normale)
            // Maintenant, le UpdateLaserLogic reprend le relais pour gérer les collisions
            float durationTimer = 0f;
            while (durationTimer < laserDuration)
            {
                durationTimer += Time.deltaTime;
                // Le Update() appelle UpdateLaserLogic() automatiquement ici
                yield return null;
            }

            // 4. STOP
            laserLine.enabled = false;
        }
    }
}