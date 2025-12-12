using UnityEngine;

public class BossPositioner : MonoBehaviour
{
    public float distanceDevantJoueur = 2.0f; // 2 mètres par défaut

    void Start() // Se lance tout au début, avant que le boss n'attaque
    {
        Repositionner();
    }

    void Repositionner()
    {
        // 1. Trouver la tête du joueur (Caméra)
        Transform playerHead = Camera.main.transform;

        // Si Camera.main ne marche pas (fréquent avec Meta), on cherche le rig OVR
        if (playerHead == null)
        {
            var rig = FindFirstObjectByType<OVRCameraRig>();
            if (rig != null) playerHead = rig.centerEyeAnchor;
        }

        if (playerHead != null)
        {
            // 2. Calculer la direction où le joueur regarde
            Vector3 regard = playerHead.forward;
            regard.y = 0; // On aplatit (Y=0) pour pas que le boss s'envole au plafond
            regard.Normalize();

            // 3. Calculer la nouvelle position : Tête + (Direction * 2m)
            Vector3 nouvellePosition = playerHead.position + (regard * distanceDevantJoueur);

            // 4. On s'assure que le Boss reste au sol (on garde son Y actuel ou on le met à 0)
            nouvellePosition.y = transform.position.y;

            // 5. On applique la téléportation
            transform.position = nouvellePosition;

            // 6. On le force à regarder le joueur tout de suite
            Vector3 lookTarget = new Vector3(playerHead.position.x, transform.position.y, playerHead.position.z);
            transform.LookAt(lookTarget);
        }
    }
}