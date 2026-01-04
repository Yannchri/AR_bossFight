using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using UnityEngine;

public class MRUKLoading : MonoBehaviour
{
    [Header("MRUK Configuration")]
    [SerializeField] private MRUK mruk;

    [Header("UI")]
    [SerializeField] private GameObject loadingScreen;

    [Header("Boss Configuration")]
    [SerializeField] private GameObject bossPrefab; 
    [Tooltip("Ajustement vertical manuel")]
    [SerializeField] private float verticalOffset = 0.0f; 

    // --- NOUVEAU : Configuration des Potions ---
    [Header("Items Configuration")]
    [SerializeField] private GameObject potionPrefab; // Ton prefab de potion
    [SerializeField] private int numberOfPotions = 3; // Combien de potions ?
    [SerializeField] private float potionOffset = 0.1f; // Petit décalage pour ne pas qu'elle soit dans le sol
    [Header("Player Configuration")]
    [SerializeField] private GameObject player;

    private bool sceneHasBeenLoaded = false;
    private MRUKRoom currentRoom;

    void Start()
    {
        if (MRUK.Instance != null)
        {
            MRUK.Instance.RegisterSceneLoadedCallback(OnSceneLoaded);
        }
    }

    private void OnSceneLoaded()
    {
        if (sceneHasBeenLoaded) return;
        sceneHasBeenLoaded = true;

        currentRoom = MRUK.Instance?.GetCurrentRoom();

        if (currentRoom != null)
        {
            TeleportPlayerToFloor();
            StartCoroutine(SpawnBossFromPrefab());
            SpawnPotions(); // --- NOUVEAU : On lance le spawn des potions
            
            if (loadingScreen != null) loadingScreen.SetActive(false);
        }
    }
    private void TeleportPlayerToFloor()
{
    if (player == null || currentRoom == null) return;

    // On cherche une position sur le sol pour le joueur
    LabelFilter filter = new LabelFilter(MRUKAnchor.SceneLabels.FLOOR);
    if (currentRoom.GenerateRandomPositionOnSurface(MRUK.SurfaceType.FACING_UP, 0.5f, filter, out Vector3 spawnPos, out Vector3 spawnNormal))
    {
        // On désactive temporairement le CharacterController pour permettre la téléportation
        var controller = player.GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;

        player.transform.position = spawnPos + Vector3.up * 0.1f; // On le place juste un peu au-dessus du sol

        if (controller != null) controller.enabled = true;
        
        // On réactive la gravité du Rigidbody ici seulement !
        var rb = player.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false; 
    }
}

    // --- NOUVEAU : La fonction pour les potions ---
    private void SpawnPotions()
    {
        if (potionPrefab == null || currentRoom == null) return;

        // On cherche sur le SOL ou sur les TABLES (plus sympa en MR)
        LabelFilter filter = new LabelFilter(MRUKAnchor.SceneLabels.FLOOR | MRUKAnchor.SceneLabels.TABLE);

        for (int i = 0; i < numberOfPotions; i++)
        {
            // On essaie de trouver une position (100 essais max par potion pour éviter une boucle infinie)
            // minRadius: 0.1f (taille de la potion environ)
            bool foundPosition = currentRoom.GenerateRandomPositionOnSurface(
                MRUK.SurfaceType.FACING_UP, 
                0.1f,    
                filter, 
                out Vector3 spawnPos, 
                out Vector3 spawnNormal
            );

            if (foundPosition)
            {
                // Instantiation
                GameObject newPotion = Instantiate(potionPrefab);
                
                // Ajustement Hauteur (Simple)
                // On ajoute un petit offset pour être sûr qu'elle est posée "sur" la surface
                spawnPos.y += potionOffset;
                
                newPotion.transform.position = spawnPos;
                
                // Optionnel : Rotation aléatoire sur l'axe Y pour varier
                newPotion.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            }
        }
        Debug.Log($"{numberOfPotions} potions générées.");
    }

    private IEnumerator SpawnBossFromPrefab()
    {
        // ... (Ton code existant pour le boss reste identique ici) ...
        yield return new WaitForSeconds(0.2f);

        if (bossPrefab == null || currentRoom == null) yield break;

        LabelFilter filter = new LabelFilter(MRUKAnchor.SceneLabels.FLOOR); 

        bool foundPosition = currentRoom.GenerateRandomPositionOnSurface(
            MRUK.SurfaceType.FACING_UP, 
            0.5f,   
            filter, 
            out Vector3 spawnPos, 
            out Vector3 spawnNormal
        );

        if (foundPosition)
        {
            GameObject newBossInstance = Instantiate(bossPrefab);
            newBossInstance.SetActive(true);

            BossController bossScript = newBossInstance.GetComponent<BossController>();
            if (bossScript != null)
            {
                // bossScript.currentRoom = currentRoom; // Décommenter si nécessaire
            }

            float adjustment = verticalOffset;
            
            Collider bossCollider = newBossInstance.GetComponent<Collider>();
            if(bossCollider == null) bossCollider = newBossInstance.GetComponentInChildren<Collider>();

            if (bossCollider != null)
            {
                adjustment += bossCollider.bounds.extents.y;
            }

            spawnPos.y += adjustment;
            newBossInstance.transform.position = spawnPos;

            Vector3 lookAtPos = Camera.main.transform.position;
            lookAtPos.y = spawnPos.y;
            newBossInstance.transform.LookAt(lookAtPos);
        }
    }
}