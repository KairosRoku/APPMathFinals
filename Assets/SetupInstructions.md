# Unity Project Setup Guide

## 1. Scene Setup
1.  **Main Camera**: Set Projection to *Orthographic*. Set Size to ~5-8 depending on your asset scale.
2.  **Game Managers**:
    *   Create an empty GameObject named `_GameManagers`.
    *   Add `GameManager.cs`, `WaveManager.cs`, `BuildManager.cs`, `FusionManager.cs` to it.
    *   **WaveManager**: Assign your Enemy Prefabs to the `Waves` array. (Size 10).
    *   **FusionManager**: Create your `FusionRecipe` entries (Fire+Fire=InfernoPrefab, etc.).
3.  **UI**:
    *   Create a Canvas (Screen Space - Overlay).
    *   Add **TextMeshPro - Text (UI)** elements for Gold, Health, Wave. Requires importing TMP Essentials.
    *   Add a Panel for "PauseMenu" (set inactive).
    *   Add a Panel for "GameOver" (set inactive).
    *   Add a Button for "FusionButton" (set inactive).
    *   Create an empty GameObject `_UIManager` and add `GameUI.cs`. Link all the Text/Panel references.
4.  **Environment**:
    *   Create empty GameObject `_Environment`.
    *   **Path**: Create empty `PathSystem` with `PathController.cs`. Create child objects as Waypoints. Drag these children into the `Waypoints` array in `PathController`.
    *   **Nodes**: Create your grid map using 2D Sprites. Add `Node.cs` and `BoxCollider2D` to each tile (set `IsTrigger` true for raycast detection).
    *   **Materials**: Create `Materials/EnemyMat` for color flashing.

## 2. Prefabs
1.  **Enemies**:
    *   Create a 2D Sprite GameObject.
    *   Add `EnemyBase.cs`.
    *   Add `CircleCollider2D` (Trigger) for projectile hit detection.
    *   **Child Object**: Create a World Space Canvas -> Image (Fill Type: Horizontal) for the Health Bar. Link this Image to `EnemyBase.HealthBarFill`.
2.  **Towers (Base)**:
    *   Create 2D Sprite.
    *   Add `TowerBase.cs`.
    *   Set `Element` (Fire/Ice/Lightning).
3.  **Towers (Fused)**:
    *   Duplicate Base towers, change Sprite and Element logic/Stats.
4.  **Projectile**:
    *   Create Sprite.
    *   Add `Projectile.cs`.
5.  **Coin**:
    *   Create Sprite (yellow circle).
    *   Add `Coin.cs`.

## 3. Important Connections
*   **BuildManager**: Needs `TowerBlueprint` list setup in any UI or internal logic (Currently simplified in code, you might need a Shop UI to call `SelectTowerToBuild`).
*   **Layer Collision**: Ensure your Raycasts work. If using 2D Physics Raycaster on Camera, ensure EventSystem is standard. 

## 4. Testing
1.  Press Play.
2.  Verify Waves spawn after 2s.
3.  Verify clicking Nodes builds towers (Implement a temporary 'Select Tower' button or keybind if Shop UI is missing).
4.  Verify Fusion appearing when selecting 2 compatible towers.
