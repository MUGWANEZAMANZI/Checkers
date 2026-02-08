# Checkers — Unit 6

This README explains how to clone and open the project (Unity 6) in Unity, minimal third-party package expectations, and a short overview of programming principles and heuristics used to make gameplay smooth.

## Required Unity Version
- Project was created with Unity 2023.4 (Editor version: 6000.0.66f1). Use the same or a compatible LTS release (2023.4.x recommended) in Unity Hub.

## Clone the repository
1. Clone the repo:

```bash
git clone https://github.com/MUGWANEZAMANZI/Checkers.git
cd Checkers
```

2. Open the project in Unity Hub:
- In Unity Hub, click "Add" and select the project's folder (the directory that contains `Assets`, `ProjectSettings`, etc.).
- Open the project with the Unity 6.0 editor.

Unity will import assets and compile scripts. Wait for the Editor to finish the import process before pressing Play.

## Minimal third-party packages
This project uses Unity built-in features and minimal external packages. Confirm the following packages (if any) in `Packages/manifest.json`:
- `com.unity.textmeshpro` (TextMeshPro) — necessary for UI text.
- Input System: The project uses the new Input System, enable it in Player Settings or install `com.unity.inputsystem`.

If any package is missing Unity will prompt to install it when the project opens. No other third-party assets are required for core gameplay.

## Quick-run steps
1. Open the scene used for the game. Look under `Assets/Scenes` — open the `SampleScene`.
2. Inspect the `GameManager` (on the root GameObject or `GameManager` prefab) to ensure `AIController`, `PlayerController`, and `UIManager` are present.
3. In the `UIManager` inspector, assign SFX and background music clips (optional) and the UI `TextMeshProUGUI` references for the piece counters.
4. Press Play. Use the Start/Play button to begin the game.

## Audio notes (troubleshooting)
- Unity requires exactly one `AudioListener` active in the scene (usually on the Main Camera). The project adds an `AudioListener` at runtime if missing, but you should verify the Main Camera has one.
- Assign `playerKillSfx`, `aiKillSfx`, and `dangerSfx` in `UIManager` to hear capture sounds.
- Background music volume is intentionally low by default; SFX is louder so captures feel impactful.

## Programming principles & heuristics used for smooth gameplay
The project focuses on clarity and responsiveness. Key practices applied:

- Single Responsibility & Modularity:
  - `BoardManager` handles board state, tilemap, and spawning pieces.
  - `PlayerController` handles input and player moves.
  - `AIController` evaluates and executes AI moves.
  - `UIManager` handles UI and audio.
  - `GameManager` coordinates game state transitions.

- Defensive programming & diagnostics:
  - Null checks for components (e.g., `boardManager`, `tileMap`) before using them.
  - Debug logs at major steps to aid development and reproduce issues.
  - Runtime checks (AudioListener auto-add) so common setup issues don't block playtesting.

- Clear data model:
  - A `Cell[,] grids` array in `BoardManager` tracks occupancy and playability.
  - Pieces are tracked by `occupant` references and `CheckerPiece` components.

- Smooth UX heuristics:
  - Short delays for AI turns (`WaitForSeconds`) to make AI actions feel deliberate but snappy.
  - Mandatory capture rules: the code checks for available captures and enforces them for the player/AI.
  - Multi-jump capture detection so chains execute correctly and feel natural.
  - Audio feedback: louder SFX for kills, subtle background music for ambiance.
  - Visual alignment: using Tilemap cell centers keeps pieces neatly placed and movement consistent.

- AI heuristics (in `AIController`):
  - Prioritize capture moves (+100 per capture) so AI behaves aggressively when possible.
  - Prefer kinging moves (+50) and safer moves (+10) to avoid immediate recapture.
  - Evaluate spatial heuristics: center control, nearby allies, and protecting back row to produce believable play.
  - For harder difficulty, the AI evaluates and chooses moves using a simple heuristic scoring system.

## Recommended editor settings
- In Project Settings > Player, ensure the correct Scripting Backend (IL2CPP or Mono) is selected for your target platform.
- If using the new Input System, enable it and restart the editor when prompted.
- Set the Editor Layout to a comfortable configuration (Scene, Game, Inspector, Console) for debugging.

## Common troubleshooting
- Silent audio: Ensure audio clips are assigned and exactly one `AudioListener` is active.
- Missing TextMeshPro text: Import TextMeshPro Essentials when prompted.
- Scripts failing to compile: open the Console, resolve the first listed error and recompile.

## Extending / Modifying
- Volumes: `UIManager` exposes volume scaling in code; consider exposing public fields for `musicVolume` and `sfxVolume` to tweak in the Inspector.
- Add an options menu with runtime sliders for master/music/SFX volumes.
- Improve AI by adding limited-depth lookahead or alpha-beta pruning for stronger play.
