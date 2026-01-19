# Tower Defense Game Implementation Plan

## 1. Project Setup & Architecture
- [ ] **Folder Structure Creation**: Create `Scripts` (and subfolders: `Core`, `Towers`, `Enemies`, `UI`, `Environment`), `Prefabs`, `Materials`.
- [ ] **GameManager**: Singleton to manage Game State (Playing, Paused, GameOver), Player Health check, and Level loading.
- [ ] **Technical Constraint adherence**: 
    - No `Rigidbody2D` or `Collider2D` triggers for game logic if possible (use distance checks for range).
    - If collisions are needed (mouse clicks), use `Physics2D.Raycast` but avoid rigidbody dynamics.

## 2. Environment & Path System
- [ ] **Waypoint System**: Create `PathController` managing a list of `Transform` waypoints (Z=0).
- [ ] **Grid/Placement System**: Create `Node` script for tile locations where towers can be placed.
    - Logic for `IsOccupied`.
    - Logic for `Highlighting` on hover (Sprite Color Change).

## 3. Wave & Enemy System
- [ ] **EnemyBase Script**:
    - Stats: HP, Speed, KillReward.
    - Movement: `Update()` using `Vector3.MoveTowards` towards current Waypoint. **NO NavMesh, NO Physics for movement.**
    - Health Bar: World-space UI canvas (Screen Space Camera or Overlay) or Sprite-based.
    - Visuals: usage of `SpriteRenderer` and color flash logic.
- [ ] **Enemy Variations**: Grunt, Tank, Runner (Inheritance or ScriptableObjects).
- [ ] **WaveManager**:
    - List of Waves (struct/class).
    - Coroutine for spawning with delays.
    - Logic: Wave 1-10 scaling (5, 8, 12... 65 enemies).

## 4. Tower System (Core Mechanism)
- [ ] **TowerBase Script**:
    - Range check (Distance check in `Update` or `FixedUpdate` - low freq).
    - Attack Logic: Spawning Projectiles or Instant Hit (Ray).
    - Visuals: Attack FX.
- [ ] **Elemental Types**:
    - **Fire**: Apply DOT (Damage Over Time) component to enemy.
    - **Ice**: Apply Slow modifier to enemy movement script.
    - **Lightning**: Chain logic (find neighbors within range).
- [ ] **Fusion System**:
    - Input handling: Select Tower A, Select Tower B.
    - Validation: Are they compatible? Are they adjacent?
    - Logic: Remove A and B, Instantiate Combined Tower at B's position.
    - Combinations: Fire+Fire=Inferno, Ice+Ice=Glacier, etc.

## 5. Currency & Economy
- [ ] **CoinDrop**:
    - On Enemy Death -> Spawn Coin Visual at world pos.
    - Coroutine: Lerp Coin position from World Space to Screen Space (UI Coin Counter).
    - **Restriction**: No Physics. Pure Transform Lerp.
- [ ] **EconomyManager**: Track Gold. Handle increments after Coin finishes lerping.

## 6. UI & Player Feedback
- [ ] **In-Game HUD**:
    - Wave Counter.
    - Health Display.
    - Gold Counter (Lerp number increment).
- [ ] **Pause Menu**:
    - Scale in/out using `Mathf.Lerp`.
- [ ] **Damage Feedback**:
    - Camera Shake script (Manual position offset).
    - Full screen red vignette flash.

## 7. Deliverables & Polish
- [ ] **Main Scene**: Setup Waypoints, Nodes, Lighting, Camera.
- [ ] **Testing**: Verify 10 waves, Fusion logic, and Win/Loss states.
