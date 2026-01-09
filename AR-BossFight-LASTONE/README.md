# AR Boss Fight

Jeu de combat en réalité augmentée pour Meta Quest où vous affrontez un boss en utilisant des sorts, un bouclier et des potions.

## 🎮 Concept

Combat de boss stationnaire en AR/VR où le joueur doit :

- **Esquiver** les zones d'attaque au sol (AoE)
- **Lancer des sorts** avec des gestes de main (hand tracking)
- **Se protéger** avec un bouclier physique
- **Utiliser des potions** pour se soigner

## 🛠️ Technologies

- **Unity** 6000.2.14f1
- **Meta XR SDK** 81.0.0
- **Universal Render Pipeline (URP)** 17.2.0
- **XR Interaction Toolkit** 3.2.2
- **XR Hands** 1.7.2 (Hand Tracking)
- **MRUK** (Mixed Reality Utility Kit)

## 📦 Installation

1. Cloner le repository
2. Ouvrir le projet avec Unity 6000.2.14f1
3. Vérifier que les packages Meta XR SDK sont installés
4. Connecter un Meta Quest et configurer les paramètres de build Android

## 🎯 Scènes principales

- **Menu.unity** - Menu principal
- **Main_Quest_Build.unity** - Scène de jeu principale
- **GameOver.unity** - Écran de défaite
- **Winner.unity** - Écran de victoire
- **HUD.unity** - Interface utilisateur

## 🧙 Systèmes de jeu

### Boss (Antoine)

- Système d'IA avec états (Idle, Chasing, Attacking, Stunned, Dead)
- Attaques AoE au sol
- Tir de boules de feu
- Intégration MRUK pour placement spatial

### Sorts du joueur (Johann)

- Détection de poses de main
- 4 types de sorts : Fireball, Ice Spike, Lightning Ray, Arcane Orb
- Système de projectiles

### Bouclier (Yannick)

- Bouclier physique pour bloquer les attaques
- UI de dégâts
- Système de tir du boss

### Potions (MaximusPrime)

- Système de versement de potion
- Mécanique de soin

## 👥 Développement

Projet développé par 4 membres, chaque système étant dans son propre dossier `Assets/_Development/`.

Pour plus de détails, consulter [PROTOCOLE.md](PROTOCOLE.md).

## 🎮 Contrôles

- **Hand Tracking** - Lancer des sorts avec des gestes
- **Contrôleurs Meta Quest** - Interactions alternatives
- **Mouvement physique** - Esquiver dans l'espace réel

## 📝 License

Projet académique - HES
