# PROTOCOLE DE DÉVELOPPEMENT - AR Boss Fight

## 📋 INFORMATIONS GÉNÉRALES

### Projet

- **Nom** : AR-BossFight-LASTONE
- **Type** : Jeu de combat en Réalité Augmentée (AR)
- **Moteur** : Unity 6000.2.14f1
- **Pipeline de rendu** : Universal Render Pipeline (URP)
- **Plateforme cible** : Meta Quest (VR/AR)
- **Repository** : AR_bossFight (Owner: Yannchri, Branch: main)

### Équipe de développement

Le projet est divisé en 4 dossiers de développement par membre :

- **Antoine** : Boss, attaques AoE, système MRUK
- **Johann** : Système de sorts du joueur, détection de poses de main
- **Yannick** : Système de bouclier, UI de dégâts, tir du boss
- **MaximusPrime** : Système de potion et mécanique de versement

---

## 🎮 CONCEPT DU JEU

### Objectif

Combat de boss en réalité augmentée où le joueur doit :

1. **Esquiver** les attaques AoE (zones au sol)
2. **Lancer des sorts** avec des gestes de main
3. **Se protéger** avec un bouclier physique
4. **Utiliser des potions** pour se soigner

### Gameplay principal

- Le joueur est immobile (position de la caméra AR)
- Le boss regarde toujours le joueur
- Combat basé sur les réflexes et la stratégie
- Interactions via hand tracking et contrôleurs Meta Quest

---

## 🏗️ ARCHITECTURE DU PROJET

### Structure des dossiers

```
Assets/
  _Development/
    ├── Antoine/      # Boss & Attaques
    ├── Johann/       # Sorts & Mains
    ├── Yannick/      # Bouclier & UI
    └── MaximusPrime/ # Potions
  Scenes/
    ├── Main_Quest_Build.unity    (Scène principale)
    ├── Johann_PlayerSpellsLab.unity
    ├── Yannick_shield.unity
    └── Totti_Gym.unity
  MetaXR/           # SDK Meta Quest
  XRI/              # XR Interaction Toolkit
  Settings/         # Configuration Unity
```

### Technologies utilisées

- **Meta XR SDK** : Hand tracking, contrôleurs Quest
- **Meta XR MRUtilityKit (MRUK)** : Scan de pièce et ancrage spatial
- **Unity XR Interaction Toolkit**
- **TextMesh Pro** : UI
- **Input System** : Nouveau système d'input Unity

---

## 🎯 SYSTÈMES CLÉS

### 1. BOSS (`BossController.cs`)

**Localisation** : `Assets/_Development/Antoine/Boss/`

**État du boss** :

```csharp
enum BossState { Idle, Chasing, Attacking, Stunned, Dead }
```

**Comportement** :

- **Rotation permanente** vers le joueur (LookAt sur playerHead)
- **Attaque cyclique** : Cooldown → Casting → Spawn zone AoE → Reset
- **Coroutine** pour gérer la séquence d'attaque (`AttackRoutine`)

**Paramètres clés** :

- `attackCooldown` : Temps entre deux attaques (défaut: 3s)
- `zoneAttackPrefab` : Prefab de l'AoE
- `playerHead` : Référence à la caméra principale

---

### 2. ATTAQUES AOE (`CircleZoneAttack.cs`)

**Localisation** : `Assets/_Development/Antoine/Attacks/CircleZoneAttack/`

**Séquence** :

1. **Warning** (2s) : Zone rouge au sol pour prévenir
2. **Explosion** (0.5s) : Dégâts dans un rayon
3. **Autodestruction** : Destroy de l'objet

**Détection des dégâts** :

```csharp
Physics.OverlapSphere(position, rayonDeDegats)
```

**Tags utilisés** : "Player", "Shield"

---

### 3. SYSTÈME DE SORTS (`PlayerSpellCaster.cs`)

**Localisation** : `Assets/_Development/Johann/Scripts/`

**Types de sorts** :

```csharp
enum SpellType {
  Fireball,      // Main ouverte
  IceSpike,      // Poing fermé
  Fireball,      // Index pointé
  ArcaneOrb      // Majeur levé
}
```

**Mode actuel** :

- **Debug clavier** : Touches 1-4 pour tester les sorts
- **À implémenter** : Détection via `HandPoseReader` (XR Hands)

**Paramètres** :

- `projectileSpeed` : 10f
- `castCooldown` : 0.3s
- Prefabs pour chaque sort

---

### 4. BOUCLIER (`MetaShieldController.cs`)

**Localisation** : `Assets/_Development/Yannick/Scripts/`

**Activation du bouclier** :

- **Clavier** : Touche S (debug)
- **Contrôleur** : Grip > 0.8
- **Hand tracking** : Pincement majeur/annulaire avec hysteresis

**Hysteresis** :

- `pinchThresholdOn` : 0.5 (activation)
- `pinchThresholdOff` : 0.4 (désactivation)
- Évite le clignotement du bouclier

**Collision** : Tag "Shield" pour bloquer les projectiles

---

### 5. TIR DU BOSS (`BossAimAndShootForShield.cs`)

**Localisation** : `Assets/_Development/Yannick/Scripts/`

**Mécanique** :

- Calcul de direction exacte vers la tête du joueur
- `Physics.Raycast` dans cette direction
- Détection : Shield (bloqué) ou Player (touché)
- Feedback visuel : `LineRenderer` pour le laser

**Activation** : Touche Entrée (debug)

---

### 6. POTIONS (`PotionPourDetector.cs`)

**Localisation** : `Assets/_Development/MaximusPrime/Scripts/`

**Mécanique de versement** :

- Détection de l'angle de la bouteille (> 60°)
- Spawn de gouttelettes à intervalles réguliers
- Drainage du liquide (max: 100, drain: 2 par goutte)
- Arrêt automatique quand vide

---

### 7. UI DE DÉGÂTS (`DamageUI.cs`)

**Localisation** : `Assets/_Development/Yannick/Scripts/`

**Feedback** :

- Message "HIT" qui apparaît 1 seconde
- Possibilité de message "BLOCK"
- Utilise TextMesh Pro

---

### 8. MRUK - CHARGEMENT DE SCÈNE (`MRUKLoading.cs`)

**Localisation** : `Assets/_Development/Antoine/Utils/`

**État** : En cours de développement
**But** : Gérer le chargement du scan de pièce Meta Quest

---

### 9. UTILITAIRES

#### `EditorCameraMover.cs`

- Déplacement de caméra en éditeur uniquement
- **Clic droit** : Rotation
- **ZQSD/WASD** : Déplacement
- **Shift** : Sprint
- Utilise le nouveau Input System

#### `SelfDestruct.cs`

- Destruction automatique après un délai
- Utilisé pour les projectiles

---

## 🔧 CONVENTIONS DE CODE

### Naming

- **Classes** : PascalCase (`BossController`)
- **Variables publiques** : camelCase (`playerHead`)
- **Variables privées** : camelCase avec `_` (`_lastCastTime`)
- **Constantes** : camelCase pour les serialized fields

### Tags Unity essentiels

- `"Player"` : Joueur / Caméra
- `"Shield"` : Bouclier physique
- `"Boss"` : Boss ennemi

### Layers (à définir si besoin)

---

## 🎨 ASSETS & PREFABS

### Prefabs identifiés

- `Boss_AOE_Attack` : Zone d'attaque circulaire
  - `WarningCircle` : Visuel d'avertissement
  - `ExplosionEffect` : Visuel d'explosion
- Prefabs de sorts : Fireball, IceSpike, LightningRay, ArcaneOrb
- `dropletPrefab` : Gouttelettes de potion
- `shieldVisual` : Visuel du bouclier

---

## 🐛 POINTS D'ATTENTION

### Systèmes en développement

1. **HandPoseReader** : Retourne actuellement `None`, à implémenter avec XR Hands
2. **MRUKLoading** : Script incomplet
3. **Détection de poses** : Actuellement en mode debug clavier

### Debug disponible

- **Sorts** : Touches 1-4
- **Bouclier** : Touche S
- **Tir du boss** : Touche Entrée
- **Caméra éditeur** : Clic droit + ZQSD

### Configuration Meta Quest

- Le projet utilise le SDK Meta XR
- Hand tracking configuré
- Support OVRHand et OVRInput

---

## 📝 WORKFLOW DE DÉVELOPPEMENT

### Avant de coder

1. Vérifier le dossier du développeur concerné
2. Identifier les dépendances avec les autres systèmes
3. Respecter les tags et layers existants

### Pendant le développement

1. Utiliser les **Coroutines** pour les séquences temporelles
2. Toujours vérifier les références null
3. Ajouter des logs Debug pour le suivi
4. Prévoir un mode debug clavier quand possible

### Tests

1. **Mode éditeur** : Utiliser les touches debug
2. **Sur Quest** : Tester hand tracking et contrôleurs
3. **Vérifier** : Collisions, tags, et raycast

---

## 💡 CONSEILS POUR L'IA

### Lors de modifications de code

1. **Toujours lire le fichier complet** avant de modifier
2. **Préserver les commentaires** français existants
3. **Respecter le style** : Des commentaires bref et clairs
4. **Tester les dépendances** : Vérifier les références entre scripts
5. **Toujours faire un plan d'action détaillé avant de modifier des fichiers**
6. **Poser des questions** si des informations manquent
7. **Ne pas inventer de fonctionnalités** non spécifiées
8. **Modifier un fichier seulement** si je te dis : "Je valide de le plan d'action"

### Structure typique d'un script

```csharp
using UnityEngine;

public class MonScript : MonoBehaviour
{
    [Header("Section")]
    public Type variable;

    private Type _privateVar;

    void Start() { }
    void Update() { }

    // Méthodes avec commentaires explicatifs
}
```

## 📞 RÉFÉRENCE RAPIDE

### Fichiers critiques

- `BossController.cs` : Cerveau du boss
- `PlayerSpellCaster.cs` : Système de magie
- `MetaShieldController.cs` : Défense du joueur
- `CircleZoneAttack.cs` : Attaques AoE

### Scènes principales

- `Main_Quest_Build.unity` : Build principal
- `*_Lab.unity` / `*_Gym.unity` : Scènes de test individuelles

### SDK & Packages

- Meta XR SDK (OVRHand, OVRInput)
- XR Interaction Toolkit
- Unity Input System
- TextMesh Pro
- URP

---

**Date de création** : 5 décembre 2025  
**Dernière mise à jour** : 5 décembre 2025  
**Version Unity** : 6000.2.14f1
