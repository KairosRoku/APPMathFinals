# Tower Defense Game Setup Guide

Welcome to the **Tower Defense Game** project! This detailed guide will walk you through setting up the development environment, opening the project, and understanding the core structure.

## 1. Prerequisites

Before you begin, ensure you have the following installed:

*   **Unity Hub**: [Download Here](https://unity.com/download)
*   **Unity Editor Version**: `6000.3.2f1` (Unity 6 Preview/Beta series)
    *   *Note: If you do not have this exact version, Unity Hub should prompt you to install it when you add the project.*
*   **Visual Studio 2022** (or your preferred IDE like VS Code or JetBrains Rider) with **Game Development with Unity** workload installed.

## 2. Installation & Setup

### Step 1: Get the Project
If you haven't already, download or clone the repository to your local machine.

### Step 2: Add to Unity Hub
1.  Open **Unity Hub**.
2.  Click on the **Projects** tab.
3.  Click the **Add** button (or "Add project from disk").
4.  Navigate to the root folder of this repository (where this `SETUP.md` file is located).
5.  Select the **Folder** (e.g., `TowerDefenseGame`).

### Step 3: Open the Project
1.  In Unity Hub, click on the project name you just added.
2.  Wait for Unity to import assets and compile scripts. This may take a few minutes for the first launch.

## 3. Running the Game

1.  Once the Unity Editor is open, go to the **Project** window (usually at the bottom).
2.  Navigate to `Assets > Scenes`.
3.  Double-click **SampleScene** to open it. (This is currently the main gameplay scene).
4.  Click the **Play** button (▶) at the top center of the editor to start the game.

## 4. Project Structure Overview

Here is a quick tour of the key folders in `Assets/`:

*   **`Scripts/`**: Contains all the C# source code.
    *   **`Core/`**:
        *   `GameManager.cs`: Manages global game state (Play, Pause, Game Over).
        *   `WaveManager.cs`: Handles enemy spawning and wave logic.
        *   `BuildManager.cs`: Handles tower placement logic.
        *   `FusionManager.cs`: Logic for combining towers.
        *   `Enums.cs`: centralized enum definitions (ElementTypes, etc).
    *   **`Environment/`**:
        *   `PathController.cs`: Manages waypoints for enemy movement.
        *   `Node.cs`: Represents grid tiles for tower placement.
        *   `Coin.cs`: Logic for currency visual/collectible.
    *   **`Enemies/`**:
        *   `EnemyBase.cs`: Base class for enemy logic, movement, and health.
    *   **`Towers/`**:
        *   `TowerBase.cs`: Base class for tower attack logic and range.
        *   `Projectile.cs`: Logic for tower projectiles.
    *   **`UI/`**:
        *   `UIManager.cs`: Main UI orchestrator.
        *   `GameUI.cs`: HUD elements (Health, Waves, Gold).
        *   `ShopUI.cs`: Tower purchase interface.
        *   `CameraShake.cs`: Visual feedback for damage/impact.
*   **`Scenes/`**: Contains the game levels (`SampleScene`).
*   **`implementation_plan.md`**: Tracks the specialized requirements and development roadmap.

## 5. Development Notes

*   **Physics Restrictions**: This project intentionally avoids Unity's built-in 2D Physics engine (`Rigidbody2D`, collision callbacks) for checking ranges and movement to follow specific technical constraints. Movement is handled via `Vector3.MoveTowards`, and ranges are checked via `Vector3.Distance`.
*   **Visual Effects**: All animations use code-based tweening (Lerp) or coroutines instead of external animation libraries.
*   **Inputs**: The project uses the new Input System (see `InputSystem_Actions.inputactions`).

## 6. Troubleshooting

*   **"Scene not found" or empty scene**: Ensure you have double-clicked `SampleScene` in the `Scenes` folder.
*   **Compilation Errors**: If you see red error messages in the Console upon opening, try **Right-click > Reimport All** in the Project window, or check that your IDE has the correct .NET SDKs installed.
