# SoftGames

Menu-driven Unity client with three interactive scenes: concurrent card redistribution (Ace of Shadows), remote dialogue with emoji tokens and avatars (Magic Words), and an Animator-driven fire colour cycle (Phoenix Flame). Primary ship target is WebGL.

Live build: https://usovichdm.github.io/SoftGamesTest/

## Requirements

| Item | Value |
|------|--------|
| Unity | `6000.3.13f1` |
| Target | WebGL (Editor / Standalone also work) |
| Input | New Input System only (`activeInputHandler: 1`) |
| UI | uGUI + TextMesh Pro |

**Packages** (`Packages/manifest.json`):

- `com.cysharp.unitask` `2.5.10` (git)
- `com.unity.nuget.newtonsoft-json` `3.2.1`
- `com.unity.inputsystem` `1.14.0`
- `com.unity.ugui` `2.0.0`
- `com.unity.test-framework` `1.4.6`
- `com.unity.ide.rider` `3.0.40`

**Vendored:** DOTween `1.3.030` under `Assets/Plugins/Demigiant/DOTween` (DLL + Modules asmdef).

WebGL Build Support for `6000.3.13f1` must be installed in Hub.

## Project Structure

```
Assets/
  Scenes/                      Build-order scenes (MainMenu first)
  Features/                    Feature slices (scripts + prefabs + art)
    AceOfShadows/              SoftGames.AceOfShadows (+ Prefabs/, Art/, Scripts/Tests/)
    MagicWords/                SoftGames.MagicWords (+ Prefabs/, Scripts/Tests/)
    PhoenixFlame/              SoftGames.PhoenixFlame (+ Art/, Scripts/Tests/)
  Common/Scripts/
    Core/                      SoftGames.Core — scene load, shared colours
    Networking/                SoftGames.Networking — HTTP + texture cache (+ Tests/)
    UI/                        SoftGames.UI — chrome, loading, responsive canvas
                               (+ Prefabs/, Art/)
  Resources/                   DOTween settings (vendor)
  Plugins/Demigiant/           DOTween
  Editor/                      WebGLBuilder
  Documentation/               Design notes that outgrew the README
```

Feature folders own their prefabs and art. Assemblies keep feature domains and Edit Mode tests from dragging in unrelated Unity UI types. Shared infrastructure (`Core`, `Networking`, `UI`) is referenced intentionally; features do not reference each other.

Edit Mode tests sit in `Scripts/Tests/` (features) or `Networking/Tests/`, each with its own Editor-only asmdef (`*.Tests`).

## Architecture

Logic that can be wrong without a scene (pile rules, move planning, emoji split, fire cycle, JSON parse) lives in plain C#. MonoBehaviours hold serialized refs, start loops, and push results into Views.

**Ace of Shadows** is the concurrency-sensitive path. Domain (`CardPile`, `CardMoveScheduler`) owns stack order and in-flight reservations. Presentation owns pooling, flight parenting, and DOTween. That split exists because flights take 2s and can finish out of order; counting “top of pile” from the Transform hierarchy would race.

**Magic Words** pipelines as: HTTP → parse → `DialogueResolver` (avatars + lines) → `EmojiParser` tokens → line views + `TextureDownloader`. Transport and UI stay in different assemblies so parse/resolve can be Edit Mode tested without Play Mode.

**Phoenix Flame** drives colour through Animator blends into `FireColorChannels`; `FireParticleBinder` samples those channels in `LateUpdate` and writes particle gradients. Domain only advances the orange → green → blue cycle.

No DI container. Scene wiring is Inspector + thin constructors in `Awake`. Async work uses UniTask + `CancellationToken` (linked to `destroyCancellationToken` where relevant).

## Scenes

| Scene | Role |
|-------|------|
| `MainMenu` | Entry. Buttons load the three feature scenes via `SceneLoader`. |
| `AceOfShadows` | 144-card deal, timed moves, pile counters, idle banner. |
| `MagicWords` | Fetches dialogue, scroll list, retry / loading overlay. |
| `PhoenixFlame` | Fire particles + colour cycle button. |

Build Settings order matches the table; `MainMenu` is index 0.

Each feature scene includes shared `SceneChrome` (Menu → MainMenu, FPS readout).

## Features

### Ace of Shadows

- Deals 144 card ids across four `CardPile`s; rents `CardView`s from a prewarmed pool (capacity 144). Scene unload does not return views to the pool (dying canvas / `SetParent`).
- Every 1s plans a move: pop source top, reserve a landing slot on the target, start a 2s flight.
- Visible counter = cards already in the pile (updates when a card leaves or lands).
- Waves of 8 moves wait until `IsIdle`, then `ScreenMessage` shows idle text.
- Suit marks are sprites (`Features/AceOfShadows/Art/CardSuits`); LiberationSans has no ♠♥♦♣ glyphs.
- In-flight cards reparent under a nested-canvas `FlightLayer` so rebuilds stay off the pile stacks.

### Magic Words

- GET `https://private-624120-softgamesassignment.apiary-mock.com/v3/magicwords` (override on the presenter).
- `{token}` placeholders map through `EmojiCatalogAsset` (ScriptableObject + TMP sprite asset); unknown tokens remain literal text.
- Avatar URLs download once per URL (in-flight coalesced); failures remember null until `Clear`.
- Offline / HTTP / empty dialogue surface an error + Retry; loading uses `LoadingOverlay` prefab.
- `DialogueFeed` bind generation drops work from a cancelled Retry so spawn/avatar apply cannot race `Clear`.

### Phoenix Flame

- Button advances flame colour (`Orange → Green → Blue`) with a short re-click lock.
- Animator `ColorIndex` int drives Orange / Green / Blue blends; binder skips particle writes when colour is unchanged.

## Gameplay Flow

1. Player opens `MainMenu`, picks a scene (`MainMenuController` → `SceneLoader`, single-mode load).
2. Feature scene runs its own `Start` / load path. Menu returns to `MainMenu` via chrome.
3. **Ace:** `AceOfShadowsController` builds domain, spawns cards, starts `MoveLoopAsync`. Each planned move fires `AceBoard.FlyAsync` (Forget); completion inserts at the reserved slot; cancel / destroy aborts via `AbortInFlightMove` and reattaches to source.
4. **Magic Words:** `MagicWordsPresenter.Load` cancels any prior request, clears the feed, fetches, resolves, binds. Destroy cancels and clears the feed (textures included).
5. **Phoenix:** Click → cycle advance → `ColorIndex` → channels → binder.

## Technical Notes

**Landing reservation (Ace).** `TryPlanMove` reserves `landingSlot = max(occupied slots on target)+1` (landed + in-flight) and increments pending arrivals before the tween. `NotifyMoveCompleted` inserts by reserved order (`CountCardsWithLowerSlot`), not “append on arrival”. Cancel leaves a hole rather than renumbering in-flight cards (their flight destination is already fixed); the next reservation skips occupied slots so ids never collide. Sources with pending landings are skipped so a card is not dealt out from under an arriving one.

**Abort.** Cancelled UniTasks / killed DOTween sequences must not push the target. `AbortInFlightMove` restores the source and frees the animation slot. On scene unload the board does not `Release` pooled cards back to the host (canvas already tearing down).

**CardTween.** DOTween Sequence: Catmull-Rom path, scale breathe, Z tilt; duration defaults to 2s. Cancel → `Kill` → `OperationCanceledException`. `DOKill` on the transform before starting avoids leftover tweens after pool reuse.

**Canvas cost.** Flight layer uses `overrideSorting`. Card raycasts stay off. Relayout is skipped for top-only deals where stack slots remain valid.

**Textures.** `TextureDownloader.Clear` bumps a generation, destroys cached `Texture2D`s, and cancels in-flight waiters. Completions from older generations destroy their texture and do not re-enter the cache—safe to call on Retry after views release sprites.

**Serialized refs.** Required Inspector fields are not null-checked in feature code—missing wiring should fail loudly (see team rule on serialized refs).

**Input.** Scenes use `InputSystemUIInputModule` + package DefaultInputActions. Legacy Input Manager alone will not drive UI.

**FPS.** `ResponsiveCanvas` sets `Application.targetFrameRate = 60` and applies safe-area padding on size / safe-area changes (no per-frame poll).

## Build & Run

**Editor**

1. Open the project folder in Hub with Unity `6000.3.13f1`.
2. Play `Assets/Scenes/MainMenu.unity`.

**Edit Mode tests**

Window → General → Test Runner → EditMode → Run All

```bash
Unity -batchmode -nographics -quit \
  -projectPath "$(pwd)" \
  -runTests -testPlatform EditMode \
  -testResults "$(pwd)/TestResults-EditMode.xml"
```

Covered today (colocated under each feature’s `Scripts/Tests/` and `Networking/Tests/`):

- Ace: `CardPile`, `CardMoveScheduler` (incl. abort / slot reuse after cancel)
- Magic Words: `DialogueResolver`, `EmojiParser`, `MagicWordsParse.ParseJson`
- Networking: `HttpResult` Ok/Fail, `DeserializeJson`, `TextureDownloader` Clear/generation

**WebGL**

Menu: SoftGames → Build WebGL → output `Builds/WebGL` (gzip + decompression fallback).

CLI:

```bash
Unity -batchmode -nographics -quit \
  -projectPath "$(pwd)" \
  -executeMethod SoftGames.EditorTools.WebGLBuilder.BuildCli
```

Do not run batchmode against a project that already has an Editor lock on the same path. Host `Builds/WebGL` as static files (GitHub Pages is what we use for the link above). Include `.nojekyll` if deploying to Pages.

## Future Improvements

- Disk / LRU policy for `TextureDownloader` if avatar sets grow.
- Addressables (or equivalent) if art and dialogue assets stop fitting a single WebGL payload.
- Light DI scope per scene once feature count outgrows Inspector wiring.
- CI: Edit Mode tests + WebGL build on PR.
- Ace: hard cap on simultaneous flights / non-uGUI cards if pile sizes or concurrency increase an order of magnitude.
