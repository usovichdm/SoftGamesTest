# SoftGames — Unity Developer Assignment

Unity 6 take-home covering three gameplay demos plus a shared main menu.

**Editor:** Unity `6000.3.13f1` (Unity 6)  
**Platform:** WebGL (also runs in Editor / Standalone)

## Tasks

### 1. Ace of Shadows
- 144 cards dealt across 4 overlapping piles
- Every **1 second** a top card starts moving to another pile
- Each flight lasts exactly **2 seconds** with easing + arc
- Multiple cards animate at once
- Live counters above each pile
- Status message when a wave’s animations all finish

### 2. Magic Words
- Loads dialogue from  
  `https://private-624120-softgamesassignment.apiary-mock.com/v3/magicwords`
- Resolves `{token}` placeholders to Unicode emoji
- Shows remote avatars with placeholders for missing/failed images
- Networking (`HttpJsonClient`, `TextureDownloader`) is separate from presentation; `MagicWordsApiClient.ParseJson` is pure and unit-tested

### 3. Phoenix Flame
- Looping multi-layer fire particle effect
- UI button cycles **Orange → Green → Blue → Orange**
- Color changes go through an **Animator Controller** with blended transitions
- `FireParticleBinder` applies animated RGB channels to the particles

## Controls / Navigation

Open **Main Menu** and pick a task. Each task scene has a **Menu** button (top-right) and an **FPS** counter (top-left).

## Project structure

```
Assets/
  Scenes/
  Scripts/
    Core/                 SoftGames.Core
    Utilities/            SoftGames.Utilities
    Animation/            SoftGames.Animation
    Networking/           SoftGames.Networking
    UI/                   SoftGames.UI
    Gameplay/
      AceOfShadows/       SoftGames.AceOfShadows
      MagicWords/         SoftGames.MagicWords
      PhoenixFlame/       SoftGames.PhoenixFlame
  Tests/
    EditMode/             SoftGames.Tests.EditMode
    Prefabs/
      UI/SceneChrome        Menu button + FPS (shared)
      AceOfShadows/
      MagicWords/
  Art/
  Documentation/
```

## Assemblies

Feature code is split into asmdefs so domains stay isolated and Edit Mode tests can reference only what they need.

| Assembly | Responsibility |
|----------|----------------|
| `SoftGames.Core` | Scene loading, shared colors |
| `SoftGames.Networking` | HTTP + texture download |
| `SoftGames.UI` | FPS, menu chrome, responsive canvas |
| `SoftGames.Animation` | Card tweens / easing |
| `SoftGames.AceOfShadows` | Card piles + move loop |
| `SoftGames.MagicWords` | Dialogue tokens, API parse, chat UI |
| `SoftGames.PhoenixFlame` | Fire cycle + particles |
| `SoftGames.Tests.EditMode` | NUnit Edit Mode tests |

## Tests

Edit Mode tests cover domain logic without entering Play Mode:

- `CardPile` / `CardMoveScheduler`
- `EmojiParser`
- `FireColorCycle`
- `HttpResult` (string Ok/Fail overload safety)
- `MagicWordsApiClient.ParseJson`

**Run:** Window → General → Test Runner → EditMode → Run All

Or batchmode:

```bash
Unity -batchmode -nographics -quit \
  -projectPath "$(pwd)" \
  -runTests -testPlatform EditMode \
  -testResults "$(pwd)/TestResults-EditMode.xml"
```

## Architecture notes

- **Domain logic** (`CardPile`, `CardMoveScheduler`, `FireColorCycle`, `EmojiParser`, DTOs) is plain C#
- **Async** flows use **UniTask** (`com.cysharp.unitask`) instead of coroutines
- **MonoBehaviours** wire Unity objects to those systems
- No DI frameworks, service locators, or giant managers
- Cached refs, pooled-friendly views, no per-card `Update()`

## Opening the project

1. Install Unity Hub + Unity **6000.3.13f1**
2. Open this folder as a project
3. Enter Play Mode on `Assets/Scenes/MainMenu.unity`

## WebGL build

1. In Unity Hub, install the **WebGL Build Support** module for `6000.3.13f1` (not present in this environment by default).  
2. File → Build Settings → **WebGL** → Switch Platform  
3. Ensure the four scenes are in Build Settings (MainMenu first)  
4. Build to e.g. `Builds/WebGL`  
5. Host the folder (itch.io, GitHub Pages, S3, nginx, etc.) and share the URL

> Hosted link: _add your deployment URL here after uploading the WebGL build._
