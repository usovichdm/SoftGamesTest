# Architecture

## Layers

| Layer | Assembly examples | Rule |
|-------|-------------------|------|
| Domain | `CardPile`, `CardMoveScheduler`, `DialogueResolver`, `EmojiParser`, `FireColorCycle` | No Unity rendering/UI |
| Networking | `SoftGames.Networking` | Callbacks / results only |
| Presentation | feature `*Controller` / `*Presenter` / `*View` | Serialize refs, drive domain |
| Shared UI | `SoftGames.UI` | Reused chrome |
| Tests | `SoftGames.Tests.EditMode` | Edit Mode NUnit against domain |

## Ace of Shadows flow

1. Controller prewarms a `CardViewPool` (144), deals ids into 4 `CardPile`s, and rents `CardView`s  
2. Every 1s `CardMoveScheduler.TryPlanMove` pops the source top only and **reserves a landing slot** on the target  
3. Target push waits until the tween completes; insert uses reserved slot order so out-of-order landings stay correct  
4. `CardTween.MoveAsync` flies the matching view for 2s (concurrent UniTasks)  
5. Waves of 8 moves pause until `ActiveAnimations == 0` → idle message  
6. Cancelled flights call `AbortInFlightMove` (restore source, release reservation, never push target)  
7. Visible pile counts = domain count + pending arrivals  
8. On destroy, views return to the pool (then disposed) so scene churn does not leave orphan Instantiates

## Magic Words flow

```
MagicWordsResponse
        ↓
 DialogueResolver  (avatar map + lines)
        ↓
 DialogueMessage → EmojiParser → tokens
        ↓
 DialogueRenderer → TMP (+ inline emoji sprites)
```

1. `MagicWordsApiClient` GET + `JsonUtility` parse  
2. `DialogueResolver` builds avatar map (first non-empty URL wins) and resolved lines  
3. Presenter binds views; `TextureDownloader` coalesces in-flight URL downloads  
4. Loading overlay via `LoadingController` prefab; Retry on network failure  

## Phoenix Flame flow

1. Button advances `FireColorCycle`  
2. Animator trigger blends Orange/Green/Blue clips  
3. Clips animate `FireColorChannels` RGB  
4. `FireParticleBinder` pushes color into particle modules (cached gradients, no GC per frame)  
