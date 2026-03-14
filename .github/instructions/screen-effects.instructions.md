---
applyTo: "Common/Systems/VFX/Screen/**,Common/Systems/VFX/Boss/**,Effects/*Screen*,Effects/*Distortion*,Effects/*Aberration*"
---

# Screen Effects Conventions

## Safety Requirements

### Distortion Exclusion Zones

All screen distortion shaders MUST implement exclusion zones around UI elements:
- Health/mana bars (top-left)
- Inventory hotbar (bottom-center)
- Minimap (top-right)
- Boss health bar (bottom-center, when active)

Reference: Wrath of the Gods repo implements exclusion zone masking — search for distortion exclusion patterns there.

### Seizure & Accessibility Safety

- Screen flash maximum opacity: 70% (never pure white fullscreen)
- Flash duration maximum: 0.15s (3 frames at 60fps)
- Minimum gap between flashes: 0.5s
- Chromatic aberration maximum offset: 8 pixels
- Screen shake maximum displacement: 12 pixels (except boss death = 20px)
- All screen effects must decay smoothly (no abrupt on/off)

## Render Target Management

### RT Lifecycle

```
Allocate RT → Set as render target → Draw to RT → Restore backbuffer → Use RT as texture → Release RT
```

- Always restore the backbuffer after drawing to an RT
- Release RTs when no longer needed (don't hold across frames unnecessarily)
- Use `GraphicsDevice.SetRenderTarget(null)` to restore backbuffer
- Check `GraphicsDevice.IsDisposed` before RT operations

### RT Sizing

- Fullscreen effects: use `Main.screenWidth` × `Main.screenHeight`
- Localized effects: use smallest RT that covers the effect area
- For downsampled effects (blur passes): use half or quarter resolution

## Screen Shake Patterns

| Context | Intensity | Duration | Notes |
|---------|-----------|----------|-------|
| Light hit | 2-3px | 0.1s | Single impulse |
| Strong hit | 4-6px | 0.15s | Quick decay |
| Explosion | 6-10px | 0.3s | Rapid frequency |
| Boss phase change | 8-12px | 0.5-1.0s | Slow rumble |
| Boss death | 12-20px | 1.0-2.0s | Escalating then fade |

Shake should use dampened oscillation, not random jitter:
```csharp
float shake = amplitude * MathF.Sin(time * frequency) * MathF.Exp(-decay * time);
```

## Chromatic Aberration

- Red channel shifts right, blue channel shifts left (or along radial direction)
- Maximum offset: 8px for impacts, 4px for ambient
- Duration: 0.1-0.3s for impacts, continuous for boss phases at low offset (1-2px)
- Always centered on the effect source, not screen center

## Vignette

- Use for boss encounters, critical health, and atmosphere
- Per-theme edge color (never default to black for every theme)
- Intensity: 20-40% for atmosphere, 50-70% for critical moments
- Animate radius for breathing/pulsing feel

## Cinematic Timing Standards

For boss phase transitions and dramatic moments:
1. **Anticipation** (0.3-0.5s): Slow zoom, darken edges, subtle rumble
2. **Impact** (0.1-0.2s): Flash + shake + aberration spike
3. **Aftermath** (0.5-1.0s): Particles settle, effects fade, new phase visuals ramp in

Never skip the anticipation phase — it makes the impact feel earned.

## Performance Budgets

- Maximum simultaneous screen effects: 3 (e.g., shake + vignette + aberration)
- Fullscreen shader passes per frame: 2 maximum during gameplay, 4 during cinematics
- Distortion displacement map resolution: half-screen maximum
- All screen effects must have a graceful off-switch when FPS drops below 30
