# Architecture notes

Living notes for decisions that are easy to regress. Folder layout and how to run the project live in the root README.

## Folder layout

Feature code, prefabs, and art live together under `Assets/Features/<Name>/`. Shared infrastructure (`Core`, `Networking`, `UI`) lives under `Assets/Common/Scripts/`.

## Boundaries

| Layer | Examples | Constraint |
|-------|----------|------------|
| Domain | `CardPile`, `CardMoveScheduler`, `DialogueResolver`, `EmojiParser` | No rendering / uGUI |
| Networking | `HttpJsonClient`, `TextureDownloader` | Results / callbacks only |
| Presentation | `*Controller`, `*Presenter`, `*View` | Serialized refs; drives domain |
| Shared UI | `SceneChrome`, `LoadingController`, `ResponsiveCanvas` | Cross-scene chrome |
| Tests | `SoftGames.*.Tests` beside each feature / Networking | Editor-only; domain + parse |

Feature assemblies do not reference each other. That keeps Ace compile surface free of Magic Words networking and lets Edit Mode tests take a narrow reference set.

## Ace of Shadows

Move planning and visuals must disagree briefly: a card has left the source stack in domain while its Transform is still mid-flight.

```
TryPlanMove
  pop source
  reserve landingSlot (= max occupied on target + 1)
  pending[target]++
    │
    ▼
  DOTween flight (2s) on FlightLayer
       │
       ├─ complete → NotifyMoveCompleted → Insert by reservation order
       └─ cancel   → AbortInFlightMove → restore source, never push target
                    (next plan skips still-occupied slots — no renumber mid-flight)
```

**Why reserve at plan time.** Completions are not FIFO. Appending on arrival reorders stacks when a later-started flight finishes first.

**Why skip sources with pending arrivals.** Taking the “top” under a landing card reads as a deal from under an arriving card.

**Pool.** Prewarm 144 through `ObjectPool` so deal does not hitch on Instantiate. Views are not `Release`d on scene unload: the host canvas is already destroying, and `SetParent` back to the pool host throws. Unity tears the card objects down with the hierarchy.

**FlightLayer.** Nested canvas + sorting override: dirty region stays on in-flight cards instead of the full pile canvas.

## Magic Words

```
GET JSON
  → MagicWordsParse.ParseJson / Validate
  → DialogueResolver (avatar map, first non-empty URL wins)
  → EmojiParser (EmojiCatalogAsset) → DialogueToken list
  → DialogueFeed (spawn lines, TextureDownloader, bind generation)
```

Presenter owns load cancellation and chrome (loading / error / retry). `DialogueFeed` owns the spawned line list, texture cache, and bind generation so Retry/Clear cannot race a previous `BindAsync` after `Yield`. Token→sprite mapping lives in `EmojiCatalogAsset` (serialized entries + TMP sprite asset), not `Resources`. Resolver stays pure so avatar merge rules stay in tests (`DialogueResolverTests`).

Failed downloads are sticky null until `Clear`—avoids hammering a bad URL every bind. `Clear` advances a generation so in-flight downloads cannot repopulate the cache after Retry.

## Phoenix Flame

```
FireColorId advance on controller
  → Animator SetInteger(ColorIndex) (+ Play snap on first frame)
  → FireColorChannels (animated RGB)
  → FireParticleBinder.LateUpdate → particle color over lifetime
```

Binder keeps gradient buffers and skips writes when the sampled colour is unchanged so Animator blends do not allocate every frame.

Initial colour snaps with `Play` + `SnapTo` so the first frame is not a blend from a default state.

## Async

- UniTask for scene-level waits, HTTP, and `await` on card flights.
- DOTween only for Ace motion (path / scale / tilt), owned by `CardTween` in the Ace feature. Kill on cancel maps to `OperationCanceledException` for the existing abort path.
- Magic Words bind uses a generation counter so `Clear` / Retry cannot race a previous `BindAsync` after `Yield`.
- No per-card `Update` loops.

## What we deliberately skipped

- DI frameworks — four scenes and explicit constructors are enough; revisit if wiring fans out.
- Addressables — single WebGL payload is still acceptable. Tuning lists that need authoring live in ScriptableObjects (e.g. `EmojiCatalogAsset`) instead.
