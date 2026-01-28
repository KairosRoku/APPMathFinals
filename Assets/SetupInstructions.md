# Unity Project Setup Guide - 2.5D Tile-Based Tower Defense

## 1. Scene Setup

### Main Camera
1. Set **Projection** to **Perspective** (Field of View ~60)
2. **Rotate** the camera: X = 45-60 degrees to look down at the ground
3. **Position**: Adjust Y and Z so you can see your tiles clearly

### Game Managers GameObject
1. Create empty GameObject named `_GameManagers`
2. Add these components:
   - `GameManager.cs`
   - `WaveManager.cs`
   - `BuildManager.cs`
   - `FusionManager.cs`
   - `InputManager.cs`
3. **Selection Icon Setup**:
   - Create a 3D Plane or Quad. 
   - Choose a semi-transparent color (e.g., light blue).
   - Set it to **Ignore Raycast** layer.
   - Save as prefab and drag into `BuildManager.SelectionIconPrefab`.
4. **Smoke VFX Setup**:
   - Create a 2D Sprite (Circle/Cloud).
   - Add `SmokeEffect.cs`.
   - Set **Duration**=1, **MaxScale**=3.
   - Save as prefab and drag into `BuildManager.SmokeVFXPrefab`.

### WaveManager Configuration
1. In Inspector, set **Waves** array size to 10
2. For each wave, assign:
   - **Enemy Prefab** (your enemy prefab)
   - **Rate**: 1.0 (spawn rate)
3. Set **Spawn Point**: Create an empty GameObject at the start of your path

### UI Setup
1. Create **Canvas** (Screen Space - Overlay)
2. Add **EventSystem** if not present
3. Create UI elements:

#### Text Elements (TextMeshPro)
- **Gold Text**: Right-click Canvas → UI → Text - TextMeshPro
  - Set default text to "100" (starting gold)
  - Name it "GoldText"
- **Health Text**: Same process, default "20"
- **Wave Text**: Default "Wave: 0"

#### Buttons
- **Start Wave Button**: 
  - Right-click Canvas → UI → Button - TextMeshPro
  - Name it "StartWaveButton"
  - Set text to "Start Wave"
  
- **Tower Shop Buttons** (Create 3):
  - **Fire Tower Button**: 
    - Add `DragTower.cs` component
    - In Inspector, create new `TowerBlueprint`:
      - **Prefab**: Your Fire Tower prefab
      - **Cost**: 100
  - **Ice Tower Button**:
    - Add `DragTower.cs` component
    - **Cost**: 150
  - **Lightning Tower Button**:
    - Add `DragTower.cs` component
    - **Cost**: 200

#### Panels
- **Pause Menu**: Panel (set inactive)
- **Game Over Panel**: Panel (set inactive)
- **Fusion Button**: Button (set inactive)

#### UI Manager
1. Create empty GameObject `_UIManager`
2. Add `GameUI.cs`
3. Link all references:
   - Gold Text
   - Health Text
   - Wave Text
   - Start Wave Button
   - Pause Menu
   - Game Over Panel

## 2. Environment Setup (2.5D Tiles)

### Path System
1. Create empty GameObject named `PathSystem`
2. Add `PathController.cs`
3. Create child empty GameObjects as waypoints:
   - Name them "Waypoint1", "Waypoint2", etc.
   - Position them along your desired path (Y = 0.5 to be above tiles)
4. Drag waypoints into `PathController.Waypoints` array

### Tower Nodes (Tiles)
**CRITICAL STEPS:**

1. Create a **3D Cube** (Right-click Hierarchy → 3D Object → Cube)
2. **Scale** it: X=1, Y=0.2, Z=1 (flat tile)
3. **MUST HAVE BoxCollider** (should be added automatically with Cube)
   - Check that **Is Trigger** is UNCHECKED
4. Add `Node.cs` component
5. In `Node.cs` Inspector:
   - **Hover Color**: Choose a highlight color (e.g., light green)
   - **Rend**: Drag the Cube's MeshRenderer component here (or leave empty to auto-detect)
6. Create a **Material** for the tile (green grass color)
7. Duplicate this tile to create your grid

### Path Tiles (Visual Only)
1. Create similar cubes for the path
2. Use different material (brown/dirt)
3. **DO NOT** add `Node.cs` to path tiles
4. Position them to form the enemy path

## 3. Prefabs

### Enemy Prefab (2.5D)
1. Create **Empty GameObject** (Parent)
2. Add `EnemyBase.cs` to parent
3. Configure stats:
   - **Max HP**: 100
   - **Speed**: 2-5 (depending on enemy type)
   - **Coin Value**: 10
   - **Type**: Grunt/Tank/Runner

4. **Child Sprite Object**:
   - Right-click parent → Create Empty
   - Add `SpriteRenderer` component
   - Add `Billboard.cs` component
   - Assign your enemy sprite
   - Position slightly above parent (Y = 0.5)

5. **Child Health Bar**:
   - Right-click parent → UI → Canvas
   - Set Canvas to **World Space**
   - Scale: 0.01, 0.01, 0.01
   - Position above enemy (Y = 1.0)
   - Right-click Canvas → UI → Scrollbar
   - Delete the "Sliding Area" child (not needed)
   - Set **Direction** to **Left To Right**
   - Set **Size** to 1 (full health)
   - Link this Scrollbar to `EnemyBase.HealthBar`

6. **Coin Prefab Reference**: Assign your coin prefab to `EnemyBase.CoinPrefab`

### Tower Prefabs

#### Fire Tower (Projectile + Burn DOT)
1. Create **2D Sprite** GameObject (or use your tower sprite)
2. Add `TowerBase.cs` component
3. **Configure TowerBase**:
   - **Range**: 3-5
   - **Damage**: 10
   - **Fire Rate**: 1
   - **Element**: **Fire**
   - **Burn Damage**: 2 (damage per second)
   - **Fire Point**: Create child empty GameObject, position it where projectiles spawn
   - **Projectile Prefab**: Drag your Projectile prefab here
   - **Laser Line**: Leave empty (not used for Fire)

#### Ice Tower (AOE Freeze Pulse)
1. Create **2D Sprite** GameObject (your tower sprite)
2. Add `TowerBase.cs` component
3. **Add Child Sprite for Pulse Effect** (NEW!):
   - Right-click tower → 2D Object → Sprite
   - Name it "IcePulse"
   - Add a **Sprite Renderer** (should be automatic)
   - Assign a **circle sprite** (white or light blue)
   - Set **Color** to light blue/cyan with some transparency
   - Set **Sorting Layer** to be behind the tower
   - **Disable** the Sprite Renderer (uncheck the box) - script will enable it
   - Position at Y = 0 (same height as tower)

4. **Configure TowerBase**:
   - **Range**: 3-4 (smaller range for balance)
   - **Damage**: 15
   - **Fire Rate**: 0.5 (slower attack)
   - **Element**: **Ice**
   - **Slow Amount**: 1.0 (100% slow = freeze)
   - **Slow Duration**: 1.0 (freeze for 1 second)
   - **Fire Point**: Create child empty GameObject (required even though no projectile)
   - **Projectile Prefab**: Leave empty (Ice uses AOE, not projectiles)
   - **Laser Line**: Leave empty
   - **Ice Pulse Sprite**: Drag the "IcePulse" child Sprite Renderer here

**How Ice Works**: When attacking, a blue circle pulses outward from the tower, damaging and freezing ALL enemies within range!

#### Lightning Tower (Chain Lightning + Line Visual)
**CRITICAL SETUP - Follow carefully!**

1. Create **2D Sprite** GameObject
2. Add `TowerBase.cs` component
3. **Add Line Renderer** component to the SAME GameObject
4. **Configure Line Renderer**:
   - **Positions**: Set size to 2
   - **Width**: 0.1 (or adjust to preference)
   - **Color**: Bright cyan/yellow (lightning color)
   - **Material**: Default-Line or create a glowing material
   - **Start Width**: 0.1
   - **End Width**: 0.05
   - **Use World Space**: Checked ✓
   - **Enabled**: Unchecked (script will enable it when shooting)

5. **Configure TowerBase**:
   - **Range**: 4-5
   - **Damage**: 20 (high single-target damage)
   - **Fire Rate**: 1.5 (slower but powerful)
   - **Element**: **Lightning**
   - **Fire Point**: Create child empty GameObject
   - **Projectile Prefab**: Leave empty (Lightning is instant hit)
   - **Laser Line**: **DRAG THE LINE RENDERER HERE** ← CRITICAL!
   - **Ice Pulse Sprite**: Leave empty (not used for Lightning)

**How Lightning Works**: 
- Instantly hits the target (no projectile travel time)
- Shows a lightning line from tower to the main target
- **Chains to up to 2 nearby enemies** within 3 units of the last hit enemy
- **Shows visual lines for each chain** (enemy-to-enemy lightning bolts!)
- Each chained enemy takes full damage
- Chains jump from enemy to enemy (not from tower)

**Troubleshooting Lightning**:
- **No visual line?** → Check that Line Renderer is assigned to "Laser Line" field
- **No chain damage?** → Ensure enemies are within 3 units of each other
- **Line stays visible?** → Make sure Line Renderer starts with "Enabled" unchecked

### Projectile Prefab
1. Create small sprite
2. Add `Projectile.cs`
3. Set **Speed**: 10

### Coin Prefab
1. Create sprite (yellow circle)
2. Add `Coin.cs`

### 4. VFX Setup
1. **Lightning Jitter**:
   - On your Lightning Tower prefab, add the `LightningBoltJitter.cs` component to the SAME GameObject as the Line Renderer.
   - Adjust **Segments** (5-10) and **Jitter Amount** (0.1 - 0.3).
2. **Impact Effects**:
   - Create 2D Sprite prefabs for each impact (Fire/Ice/Lightning).
   - Add `SimpleVFX.cs` to these prefabs.
   - Configure **Duration** (~0.3s) and **Scale/Color** lerps.
   - Assign these to the **Impact VFX Prefab** slot in your Tower (for Lightning/Ice) or Projectile (for Fire).

## 5. Critical Checklist

### Before Testing:
- [ ] All Node tiles have **BoxCollider** (not trigger)
- [ ] Camera can see the tiles clearly
- [ ] `_GameManagers` has all 5 manager scripts
- [ ] `GameUI` references are all linked
- [ ] Tower buttons have `DragTower.cs` with `TowerBlueprint` assigned
- [ ] Start Wave button is linked in `GameUI`
- [ ] `PathController.Waypoints` array is filled
- [ ] Enemy prefab has **Scrollbar** (or Image) assigned to `HealthBar` field
- [ ] **Lightning Towers** have `LightningBoltJitter.cs` component
- [ ] **Towers & Projectiles** have their **Impact VFX Prefab** slots assigned
- [ ] All prefabs are saved in Prefabs folder

### Tower-Specific Checklist:
- [ ] **Fire Tower**: Has Projectile Prefab assigned
- [ ] **Lightning Tower**: Has Line Renderer component on same GameObject
- [ ] **Lightning Tower**: Line Renderer is assigned to "Laser Line" field
- [ ] **Lightning Tower**: Line Renderer starts with "Enabled" unchecked
- [ ] **Ice Tower**: Element is set to "Ice" (not Fire or Lightning)
- [ ] All towers have a **Fire Point** child GameObject

### Testing Steps:
1. **Press Play**
2. **Check Console** for any errors
3. **Drag a tower button** - you should see debug logs:
   - "DragTower: Raycasting from..."
   - "DragTower: Hit [TileName]"
   - "BuildManager: Attempting to build..."
4. **Click Start Wave** - enemies should spawn
5. **Verify**:
   - Towers attack enemies
   - Enemies follow path
   - Health bars update
   - Coins fly to UI

## 5. Common Issues

### "Raycast hit nothing"
- **Solution**: Ensure tiles have BoxCollider and camera can raycast to them

### "Hit object has no Node component"
- **Solution**: Make sure you dropped tower on a tile with `Node.cs`, not a path tile

### "BuildManager.Instance is null"
- **Solution**: Ensure `BuildManager.cs` is on `_GameManagers` object

### Start Wave does nothing
- **Solution**: Check `GameUI.StartWaveButton` is assigned in Inspector

### Enemies don't move
- **Solution**: Verify `PathController.Waypoints` array is populated

---

## 6. Tower Attack Troubleshooting

### Lightning Tower - No Visual Line
**Symptoms**: Lightning tower damages enemies but no line appears

**Solutions**:
1. Check that **Line Renderer** component exists on the tower GameObject
2. Verify **Line Renderer** is assigned to the `Laser Line` field in `TowerBase`
3. Ensure Line Renderer **Positions** array size is set to 2
4. Make sure Line Renderer has a **Material** assigned (use Default-Line)
5. Check that Line Renderer **Width** is visible (try 0.1 or higher)

### Lightning Tower - No Chain Damage
**Symptoms**: Lightning only hits one enemy, doesn't chain to nearby enemies

**Solutions**:
1. Ensure multiple enemies are **within 3 units** of each other
2. Check Console for errors when tower shoots
3. Verify the tower's **Element** is set to "Lightning" (not Fire or Ice)
4. Test with enemies grouped closer together

**How to test**: Spawn multiple enemies and let them bunch up on the path. Lightning should hit 1 main target + up to 2 nearby enemies.

### Ice Tower - No Visible Attack
**Symptoms**: Ice tower seems to do nothing

**This is NORMAL!** Ice tower has no visual effect (no projectile, no line). 

**How to verify it's working**:
1. Watch enemy **health bars** - they should decrease when in range
2. Watch enemy **movement speed** - they should freeze (stop moving) briefly
3. Check that tower's **Element** is set to "Ice"
4. Verify **Range** is set (3-4 recommended)
5. Ice hits ALL enemies in range simultaneously

**Visual feedback ideas** (optional):
- Add a particle system that plays when tower shoots
- Add a blue circle sprite that pulses outward
- Tint enemies blue when frozen

### Fire Tower - Projectiles Don't Spawn
**Symptoms**: Fire tower doesn't shoot projectiles

**Solutions**:
1. Check that **Projectile Prefab** is assigned in `TowerBase`
2. Verify **Fire Point** child GameObject exists
3. Ensure tower's **Element** is set to "Fire"
4. Check that Projectile prefab has `Projectile.cs` script attached

### Towers Don't Attack At All
**Symptoms**: No tower shoots anything

**Solutions**:
1. Verify enemies are **within range** (check the red wire sphere in Scene view when tower is selected)
2. Ensure **Fire Rate** is not 0
3. Check that enemies have `EnemyBase.cs` component
4. Verify tower has `TowerBase.cs` component
5. Check Console for errors
