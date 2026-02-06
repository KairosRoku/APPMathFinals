# UI Implementation Guide: Menus

This guide outlines how to set up the newly created Menu systems in Unity.

## 1. Scene Setup
### Main Menu Scene
1. Create a new scene named **"MainMenu"**.
2. Add a `Canvas` with a `CanvasScaler` set to **Scale With Screen Size**.
3. Create a GameObject named **"UIController"** and attach the `MainMenuController` script.
4. Create two Panels under the Canvas:
   - **MainPanel**: Contains title, "Play", "Settings", and "Quit" buttons.
   - **SettingsPanel**: Contains volume sliders, quality dropdown, and a "Back" button.
5. Link these panels to the `MainMenuController` inspector fields.
6. Connect button `OnClick` events to `MainMenuController` functions:
   - Play -> `StartGame()`
   - Settings -> `ShowSettings()`
   - Quit -> `QuitGame()`
   - Back (in sub-panel) -> `GoBack()`

### Game Scene
1. Create a **SettingsPanel** under your existing `GameUI` Canvas. This should be a child of (or adjacent to) your Pause Menu.
2. In the `GameUI` inspector, link the following panels:
   - `PauseMenuPanel`
   - `SettingsPanel`
   - `GameOverPanel` (New: Add a "Try Again" button calling `RestartLevel()` and a "Menu" button calling `BackToMainMenu()`)
   - `VictoryPanel` (New: Add a "Play Again" button calling `RestartLevel()` and a "Menu" button calling `BackToMainMenu()`)
3. Add a "Settings" button to the Pause Menu that calls `GameUI.OpenSettings()`.
4. Add a "Restart" button to the Pause Menu that calls `GameUI.RestartLevel()`.
5. Add a "Back to Main Menu" button that calls `GameUI.BackToMainMenu()`.
6. **Add a "Return to Game" button** in the **SettingsPanel** that calls `GameUI.Resume()`. This allows players to jump straight back into the action without going back through the Pause Menu.
7. Add a "Back" button in the **SettingsPanel** that calls `GameUI.CloseSettings()` to return to the Pause Menu.

## 2. Settings Manager (Persistent)
1. In the **MainMenu** scene, create a GameObject named **"SettingsManager"**.
2. Attach the `SettingsManager` script.
3. This object will persist through scenes (`DontDestroyOnLoad`).
4. Link sliders in the **SettingsPanel** to `SettingsManager` functions:
   - Master Slider -> `SetMasterVolume`
   - Music Slider -> `SetMusicVolume`
   - SFX Slider -> `SetSFXVolume`

## 3. Build Settings
1. Go to **File -> Build Settings**.
2. Add both scenes:
   - `MainMenu` (Index 0)
   - `SampleScene` (Index 1)

## 4. Input
- Press **Escape** during gameplay to toggle the Pause Menu.
