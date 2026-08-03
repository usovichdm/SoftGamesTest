# Architecture

## Layers

| Layer | Assembly examples | Rule |
|-------|-------------------|------|
| Domain | `CardPile`, `EmojiParser`, `FireColorCycle` inside feature asmdefs | No Unity rendering/UI |
| Networking | `SoftGames.Networking` | Callbacks / results only |
| Presentation | feature `*Controller` / `*Presenter` / `*View` | Serialize refs, drive domain |
| Shared UI | `SoftGames.UI` | Reused chrome |
| Tests | `SoftGames.Tests.EditMode` | Edit Mode NUnit against domain |

## Ace of Shadows flow

1. Controller deals 144 ids into 4 `CardPile`s and spawns `CardView`s  
2. Every 1s `CardMoveScheduler.TryPlanMove` pops the source top only; push to the target waits until the tween completes  
3. `CardTween.MoveAsync` flies the matching view for 2s (concurrent UniTasks)  
4. Waves of 8 moves pause until `ActiveAnimations == 0` → idle message  

## Magic Words flow

```
DialogueMessage.Text
        ↓
   EmojiParser
        ↓
 List<DialogueToken>   (TextToken | EmojiToken)
        ↓
 DialogueRenderer
        ↓
 one wrapping TMP_Text (inline emoji sprites)
```

1. `MagicWordsApiClient` GET + `JsonUtility` parse  
2. Presenter builds avatar map (first valid URL wins)  
3. `DialogueMessage` → `EmojiParser` → tokens  
4. `DialogueRenderer` lays out TMP + sprite images  
5. `TextureDownloader` fills avatars or placeholder  

## Phoenix Flame flow

1. Button advances `FireColorCycle`  
2. Animator trigger blends Orange/Green/Blue clips  
3. Clips animate `FireColorChannels` RGB  
4. `FireParticleBinder` pushes color into particle modules  
