# Rift

A first-person action game featuring both ranged and melee combat mechanics.

## Features

### Combat System
- **Dual Weapon System**
  - Ranged weapons with customizable fire rates and damage
  - Melee weapons with combo system
  - Smooth weapon switching
  - Ammo management system
  - Stamina-based combat actions

### Player Mechanics
- First-person camera controls
- Weapon aiming and shooting
- Melee attack combos
- Stamina management
- Health system
- Combat feedback system

### Enemy System
- Health management
- Damage reception
- Layer-based hit detection
- Combat feedback on hits

### Technical Features
- Optimized for WebGL
- Efficient hit detection system
- Smooth weapon animations
- Particle effects for combat feedback
- Responsive controls

## Controls

### Movement
- `W, A, S, D` - Move
- `Space` - Jump
- `Shift` - Sprint

### Combat
- `Left Mouse Button` - Attack/Shoot
- `R` - Reload (for ranged weapons)
- `Q` - Switch weapons

## Setup Instructions

### Prerequisites
- Unity 2022.3 LTS or newer
- Basic understanding of Unity's component system

### Project Structure
```
Assets/
├── Scripts/
│   ├── Weapons/
│   │   ├── GunController.cs
│   │   ├── MeleeController.cs
│   │   └── BaseWeapon.cs
│   ├── Player/
│   │   ├── PlayerController.cs
│   │   └── PlayerHealthController.cs
│   └── Enemy/
│       └── EnemyHealthController.cs
└── Prefabs/
    ├── Weapons/
    └── Effects/
```

### Required Setup
1. Player Setup
   - Create a player object with the "Player" tag
   - Add a "CameraHolder" as a child of the player
   - Add a Camera component to the CameraHolder

2. Weapon Setup
   - Attach GunController or MeleeController to weapon prefabs
   - Configure weapon parameters in the inspector
   - Set up appropriate animations

3. Enemy Setup
   - Place enemies in the "Enemies" layer
   - Add EnemyHealthController component
   - Configure health and damage parameters

## Development Notes

### WebGL Optimization
- Hit detection uses a buffer system for better performance
- Reduced particle counts for WebGL compatibility
- Optimized raycast system for melee attacks

### Known Issues
- None currently reported

## Contributing
1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License
This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments
- Unity Technologies for the game engine
- All contributors and testers 