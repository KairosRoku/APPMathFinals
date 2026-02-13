# Tower Defense Shader Setup Guide

I have added 4 high-quality HLSL shaders to your project. These use the Universal Render Pipeline (URP) and are designed for optimized, stylized performance.

## 1. Enemy Damage Flash (`EnemyDamageFlash.shader`)
**Purpose**: Handles the red flash when an enemy takes damage.
- **Setup**: 
  1. Create a new Material (`EnemyMaterial`) in `Assets/Materials`.
  2. Set the Shader to `Custom/EnemyDamageFlash`.
  3. Assign this material to your Enemy prefabs' Renderer.
- **Note**: The `EnemyBase.cs` script has been automatically updated to control the `_FlashAmount` and `_FlashColor` properties on this shader.

## 2. Fire Attack (`FireProjectile.shader`)
**Purpose**: Animated, glowing fire for projectiles.
- **Setup**:
  1. Create a Material (`FireMaterial`) using this shader.
  2. Assign a fire texture (or a generic noise texture).
  3. Set the **Intensity** to > 1 for a "Bloom" glow effect.
  4. Assign this to your Fire tower's projectile prefab.

## 3. Ice Pulse (`IcePulse.shader`)
**Purpose**: Procedural expand-and-fade ring effect for the Ice Tower AOE.
- **Setup**:
  1. Create a Material (`IcePulseMaterial`) using this shader.
  2. Set the **Color** to a light blue and **Rim Color** to white.
  3. Assign this material to the `IcePulseSprite` (or a Quad/Plane) on your Ice Tower.
- **Note**: This shader is procedural and handles the ring animation via the `_Time` variable and `_PulseSpeed`.

## 4. Lightning Attack (`LightningBolt.shader`)
**Purpose**: High-frequency jittering energy beam for Lightning towers.
- **Setup**:
  1. Create a Material (`LightningMaterial`) using this shader.
  2. Assign it to the **Line Renderer** component on your Lightning tower.
  3. Adjust the **Jitter** and **Speed** sliders in the material inspector to get the desired "unstable" look.

---

### Integration Notes
- All shaders are located in `Assets/Shaders/`.
- All scripts have been updated to recognize these new shader properties.
