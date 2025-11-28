using UnityEngine;
using System.Collections;

public class BossAttackTester : MonoBehaviour
{
    [Header("Cible")]
    public Transform playerHead;
    [Header("Visuel")]
    public LineRenderer laserLine;

    void Update()
    {
        // 1. VISÉE : Le trait vert dans la vue "Scene" montre où le boss regarde
        if (playerHead != null)
        {
            transform.LookAt(playerHead);
            // Dessine un trait vert visible seulement dans l'onglet SCENE
            Debug.DrawRay(transform.position, transform.forward * 20f, Color.green);
        }

        // 2. INPUTS
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("🟢 Touche ENTRÉE reçue ! Je lance l'attaque normale.");
            StartCoroutine(FireLaser(false));
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("🟣 Touche ESPACE reçue ! Je lance l'attaque perçante.");
            StartCoroutine(FireLaser(true));
        }
    }

    IEnumerator FireLaser(bool isUnblockable)
    {
        laserLine.enabled = true;
        laserLine.SetPosition(0, transform.position);

        // Couleur : Rouge (Normal) ou Magenta (Perçant)
        laserLine.material.color = isUnblockable ? Color.magenta : Color.red;

        RaycastHit hit;

        // On lance le rayon
        if (Physics.Raycast(transform.position, transform.forward, out hit, 20f))
        {
            Debug.Log("🎯 J'AI TOUCHÉ : " + hit.collider.name); // <--- QU'EST-CE QU'ON TOUCHE ?

            laserLine.SetPosition(1, hit.point);

            if (hit.collider.CompareTag("Shield"))
            {
                if (isUnblockable) Debug.Log("😈 PERÇANT traverse bouclier !");
                else Debug.Log("🛡️ BLOQUÉ !");
            }
            else if (hit.collider.CompareTag("Player"))
            {
                Debug.Log("🔥 JOUEUR TOUCHÉ !");
            }
            else
            {
                Debug.Log("⚠️ J'ai touché un objet sans Tag (Mur ?) : " + hit.collider.name);
            }
        }
        else
        {
            // Si on ne touche RIEN
            Debug.Log("💨 TIR DANS LE VIDE ! (Pas de collision détectée)");
            laserLine.SetPosition(1, transform.position + transform.forward * 20f);
        }

        yield return new WaitForSeconds(0.5f);
        laserLine.enabled = false;
    }
}