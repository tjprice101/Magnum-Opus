# Devourer of Gods VFX Design Document
## Analysis of InfernumMode's Devourer of Gods Visual Effects

---

## Overview

The Devourer of Gods features cosmic portal effects, reality-tearing visuals, worm segment transitions, and geometric laser wall patterns. This document details the DoGPortalShader, reality slice effects, and portal-based attack systems for adaptation into MagnumOpus.

---

## Core Portal System

### DoGPortalShader.fx - Swirl Effect

The signature cosmic portal shader with rotation matrix transformation:

```hlsl
// File: Assets/Effects/Primitives/DoGPortalShader.fx
sampler uImage0 : register(s0);
sampler noiseTexture : register(s1);

float globalTime;
float swirlIntensity;
float2 portalCenter;

float2x2 CreateRotationMatrix(float angle)
{
    float c = cos(angle);
    float s = sin(angle);
    return float2x2(c, -s, s, c);
}

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    // Calculate distance from portal center
    float2 centeredCoords = coords - portalCenter;
    float distance = length(centeredCoords);
    
    // Swirl rotation based on distance
    float swirlAngle = distance * swirlIntensity + globalTime * 2.0;
    float2x2 swirlRotationMatrix = CreateRotationMatrix(swirlAngle);
    
    // Apply swirl transformation
    float2 swirledCoords = mul(swirlRotationMatrix, centeredCoords) + portalCenter;
    
    // Sample noise for additional distortion
    float noise = tex2D(noiseTexture, swirledCoords * 0.5 + globalTime * 0.1).r;
    swirledCoords += noise * 0.03 * (1.0 - distance);
    
    // Sample base texture with swirled coordinates
    float4 baseColor = tex2D(uImage0, swirledCoords);
    
    // Edge fade for portal boundary
    float edgeFade = smoothstep(0.5, 0.3, distance);
    baseColor.a *= edgeFade;
    
    // Add cosmic color tint
    float3 cosmicTint = float3(0.5, 0.3, 0.8); // Purple-ish
    baseColor.rgb = lerp(baseColor.rgb, baseColor.rgb * cosmicTint, 0.3);
    
    return baseColor;
}

technique Technique1
{
    pass DoGPortalPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
```

---

## Reality Break Portal Laser Wall

### Additive Blend Portal Spawning

```csharp
// File: RealityBreakPortalLaserWall.cs (Lines 60-130)
public class RealityBreakPortalLaserWall : ModProjectile
{
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D portalTexture = InfernumTextureRegistry.LaserCircle.Value;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Vector2 origin = portalTexture.Size() * 0.5f;
        
        // Rotation for swirl effect
        float rotation = Main.GlobalTimeWrappedHourly * 3f * Projectile.ai[0]; // Direction-based rotation
        
        // Additive blend for cosmic glow
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, 
            SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, 
            null, Main.GameViewMatrix.TransformationMatrix);
        
        // Multi-layer portal drawing
        // Layer 1: Outer purple glow
        Color outerColor = new Color(120, 60, 180) * 0.5f * Projectile.Opacity;
        Main.spriteBatch.Draw(portalTexture, drawPosition, null, outerColor, 
            rotation, origin, Projectile.scale * 1.4f, 0, 0f);
        
        // Layer 2: Middle cosmic cyan
        Color middleColor = new Color(80, 200, 220) * 0.6f * Projectile.Opacity;
        Main.spriteBatch.Draw(portalTexture, drawPosition, null, middleColor, 
            -rotation * 0.7f, origin, Projectile.scale * 1.2f, 0, 0f);
        
        // Layer 3: Inner bright core
        Color innerColor = Color.White * 0.8f * Projectile.Opacity;
        Main.spriteBatch.Draw(portalTexture, drawPosition, null, innerColor, 
            rotation * 0.5f, origin, Projectile.scale * 0.9f, 0, 0f);
        
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, 
            SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, 
            null, Main.GameViewMatrix.TransformationMatrix);
        
        return false;
    }
    
    public override void AI()
    {
        // Spawn connecting laser between portal pairs
        if (PartnerPortal != null && Main.netMode != NetmodeID.MultiplayerClient)
        {
            SpawnConnectingLaser();
        }
        
        // Portal particles
        if (Main.rand.NextBool(3))
        {
            Vector2 particlePos = Projectile.Center + Main.rand.NextVector2Circular(30f, 30f);
            Vector2 particleVel = (Projectile.Center - particlePos).SafeNormalize(Vector2.Zero) * 2f;
            
            // Cosmic dust particle
            Dust dust = Dust.NewDustPerfect(particlePos, DustID.PurpleTorch, particleVel);
            dust.noGravity = true;
            dust.scale = Main.rand.NextFloat(0.8f, 1.4f);
        }
    }
}
```

---

## Charge Gate Portals

### Teleportation Entry/Exit Points

```csharp
// File: DoGChargeGate.cs (Lines 50-110)
public class DoGChargeGate : ModProjectile
{
    public const float MaxPortalScale = 1.5f;
    
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D gateTex = ModContent.Request<Texture2D>(Texture).Value;
        Vector2 drawPos = Projectile.Center - Main.screenPosition;
        Vector2 origin = gateTex.Size() * 0.5f;
        
        float rotation = Main.GlobalTimeWrappedHourly * 2f;
        float scale = Projectile.scale;
        
        // Apply DoG portal shader effect
        Effect portalShader = InfernumEffectsRegistry.DoGPortalShader;
        portalShader.Parameters["globalTime"].SetValue(Main.GlobalTimeWrappedHourly);
        portalShader.Parameters["swirlIntensity"].SetValue(5f);
        portalShader.Parameters["portalCenter"].SetValue(new Vector2(0.5f, 0.5f));
        
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, ...);
        
        portalShader.CurrentTechnique.Passes[0].Apply();
        
        // Draw swirling portal
        Main.spriteBatch.Draw(gateTex, drawPos, null, Color.White * Projectile.Opacity, 
            rotation, origin, scale, 0, 0f);
        
        Main.spriteBatch.End();
        
        // Secondary glow layer without shader
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, ...);
        
        Color glowColor = new Color(150, 80, 200) * 0.4f * Projectile.Opacity;
        Main.spriteBatch.Draw(gateTex, drawPos, null, glowColor, 
            -rotation * 0.5f, origin, scale * 1.3f, 0, 0f);
        
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, ...);
        
        return false;
    }
}
```

---

## Death Animation Telegraph

### BloomLineSmall Laser Effect

```csharp
// File: DoGDeathInfernum.cs (Lines 80-130)
public class DoGDeathInfernum : ModProjectile, IPixelPrimitiveDrawer
{
    public PrimitiveTrailCopy LaserDrawer;
    
    public float TelegraphWidthFunction(float completionRatio)
    {
        // Thin telegraph that pulses
        float pulse = Sin(Main.GlobalTimeWrappedHourly * 10f) * 0.2f + 0.8f;
        return 15f * pulse * Projectile.Opacity;
    }
    
    public Color TelegraphColorFunction(float completionRatio)
    {
        // Cosmic purple-cyan gradient
        Color purple = new Color(180, 80, 220);
        Color cyan = new Color(80, 200, 240);
        float gradient = Sin(completionRatio * Pi);
        return Color.Lerp(purple, cyan, gradient) * Projectile.Opacity;
    }
    
    public void DrawPixelPrimitives(SpriteBatch spriteBatch)
    {
        LaserDrawer ??= new PrimitiveTrailCopy(TelegraphWidthFunction, TelegraphColorFunction, 
            null, true, InfernumEffectsRegistry.GenericLaserVertexShader);
        
        InfernumEffectsRegistry.GenericLaserVertexShader.SetShaderTexture(
            InfernumTextureRegistry.BloomLineSmall);
        
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        float length = LaserLength * Projectile.scale;
        
        Vector2[] points = new Vector2[12];
        for (int i = 0; i < 12; i++)
        {
            points[i] = Projectile.Center + direction * (i / 11f) * length;
        }
        
        LaserDrawer.DrawPixelated(points, -Main.screenPosition, 25);
    }
    
    public override bool PreDraw(ref Color lightColor)
    {
        // Additional glow sprites
        Texture2D glowTex = InfernumTextureRegistry.BloomFlare.Value;
        Vector2 drawPos = Projectile.Center - Main.screenPosition;
        
        Main.spriteBatch.SetBlendState(BlendState.Additive);
        
        float rotation = Main.GlobalTimeWrappedHourly * 4f;
        Color glowColor = new Color(150, 100, 220) * 0.6f * Projectile.Opacity;
        
        Main.spriteBatch.Draw(glowTex, drawPos, null, glowColor, 
            rotation, glowTex.Size() * 0.5f, 0.5f, 0, 0f);
        
        Main.spriteBatch.SetBlendState(BlendState.AlphaBlend);
        
        return true; // Continue with primitive draw
    }
}
```

---

## Reality Slice Effect

### Void Tear Rendering

```csharp
// File: RealitySlice.cs (Lines 40-100)
public class RealitySlice : ModProjectile
{
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D sliceTex = ModContent.Request<Texture2D>(Texture).Value;
        Vector2 drawPos = Projectile.Center - Main.screenPosition;
        Vector2 origin = sliceTex.Size() * 0.5f;
        
        // RealityTearVertexShader for void effect
        Effect tearShader = InfernumEffectsRegistry.RealityTearVertexShader;
        tearShader.Parameters["globalTime"].SetValue(Main.GlobalTimeWrappedHourly);
        tearShader.Parameters["tearIntensity"].SetValue(Projectile.Opacity);
        
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, ...);
        
        tearShader.CurrentTechnique.Passes[0].Apply();
        
        // Draw reality tear
        Main.spriteBatch.Draw(sliceTex, drawPos, null, Color.White, 
            Projectile.rotation, origin, Projectile.scale, 0, 0f);
        
        Main.spriteBatch.End();
        
        // Edge glow effect
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, ...);
        
        Color edgeColor = new Color(180, 100, 220) * 0.5f * Projectile.Opacity;
        Main.spriteBatch.Draw(sliceTex, drawPos, null, edgeColor, 
            Projectile.rotation, origin, Projectile.scale * 1.1f, 0, 0f);
        
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, ...);
        
        return false;
    }
}
```

---

## Segment Behavior & Portal Transitions

### Worm Segment Alpha Fading

```csharp
// File: DoGSegmentBehaviorOverride.cs (Lines 100-160)
public class DoGSegmentBehaviorOverride : NPCBehaviorOverride
{
    // GeneralPortalIndex tracks which portal the segment is transitioning through
    public ref float GeneralPortalIndex => ref NPC.Infernum().ExtraAI[0];
    
    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D texture = TextureAssets.Npc[npc.type].Value;
        Vector2 drawPosition = npc.Center - screenPos;
        Vector2 origin = texture.Size() * 0.5f;
        
        // Calculate alpha based on portal transition
        float alpha = 1f;
        if (GeneralPortalIndex >= 0)
        {
            // Fade out when entering portal, fade in when exiting
            float transitionProgress = GetPortalTransitionProgress();
            alpha = GeneralPortalIndex % 2 == 0 ? 1f - transitionProgress : transitionProgress;
        }
        
        // Draw segment with calculated alpha
        Color segmentColor = drawColor * alpha;
        spriteBatch.Draw(texture, drawPosition, null, segmentColor, 
            npc.rotation, origin, npc.scale, 0, 0f);
        
        // Portal glow when transitioning
        if (GeneralPortalIndex >= 0 && alpha < 0.8f)
        {
            DrawPortalGlow(npc, spriteBatch, screenPos, 1f - alpha);
        }
        
        return false;
    }
    
    private void DrawPortalGlow(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, float intensity)
    {
        Texture2D glowTex = InfernumTextureRegistry.BloomCircle.Value;
        Vector2 drawPos = npc.Center - screenPos;
        
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, ...);
        
        Color glowColor = new Color(150, 100, 200) * intensity * 0.5f;
        spriteBatch.Draw(glowTex, drawPos, null, glowColor, 
            0f, glowTex.Size() * 0.5f, 1.5f, 0, 0f);
        
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, ...);
    }
}
```

---

## Phase 2 Head Special Attacks

### Laser Wall, Circular Burst, and Charge Gates

```csharp
// File: DoGPhase2HeadBehaviorOverride.cs (Attack sections)
public class DoGPhase2HeadBehaviorOverride : NPCBehaviorOverride
{
    public enum SpecialAttackState
    {
        LaserWalls,
        CircularLaserBurst,
        ChargeGates
    }
    
    // Laser Wall Pattern
    public void PerformLaserWallAttack(NPC npc)
    {
        if (AttackTimer == LaserWallSpawnTime)
        {
            // Spawn portal pairs for laser walls
            for (int i = 0; i < WallCount; i++)
            {
                float xOffset = (i - WallCount * 0.5f) * WallSpacing;
                Vector2 topPos = Target.Center + new Vector2(xOffset, -600f);
                Vector2 bottomPos = Target.Center + new Vector2(xOffset, 600f);
                
                // Create linked portal pair
                int topPortal = Projectile.NewProjectile(npc.GetSource_FromAI(), topPos, 
                    Vector2.Zero, ModContent.ProjectileType<RealityBreakPortalLaserWall>(), 
                    LaserDamage, 0f, Main.myPlayer, 1f, i); // ai[0] = direction, ai[1] = pair index
                
                int bottomPortal = Projectile.NewProjectile(npc.GetSource_FromAI(), bottomPos, 
                    Vector2.Zero, ModContent.ProjectileType<RealityBreakPortalLaserWall>(), 
                    LaserDamage, 0f, Main.myPlayer, -1f, i);
                
                // Link portals
                if (topPortal >= 0 && bottomPortal >= 0)
                {
                    Main.projectile[topPortal].ai[2] = bottomPortal;
                    Main.projectile[bottomPortal].ai[2] = topPortal;
                }
            }
        }
    }
    
    // Circular Laser Burst
    public void PerformCircularLaserBurst(NPC npc)
    {
        if (AttackTimer == BurstTelegraphTime)
        {
            // Radial laser pattern
            int laserCount = 16;
            for (int i = 0; i < laserCount; i++)
            {
                float angle = TwoPi * i / laserCount;
                Vector2 direction = angle.ToRotationVector2();
                
                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, 
                    direction * LaserSpeed, ModContent.ProjectileType<DoGDeathInfernum>(), 
                    LaserDamage, 0f, Main.myPlayer);
            }
            
            // Central burst VFX
            SpawnCircularBurstVFX(npc.Center);
        }
    }
    
    private void SpawnCircularBurstVFX(Vector2 center)
    {
        // Shockwave
        for (int i = 0; i < 24; i++)
        {
            float angle = TwoPi * i / 24f;
            Vector2 dustVel = angle.ToRotationVector2() * 8f;
            Dust dust = Dust.NewDustPerfect(center, DustID.PurpleTorch, dustVel);
            dust.noGravity = true;
            dust.scale = 2f;
        }
        
        // Screen effects would go here
    }
    
    // Charge Gate System
    public void PerformChargeGateAttack(NPC npc)
    {
        // Create entry and exit portals
        Vector2 entryPos = npc.Center + npc.velocity.SafeNormalize(Vector2.UnitX) * -200f;
        Vector2 exitPos = Target.Center + Target.velocity.SafeNormalize(Vector2.UnitX) * 300f;
        
        // Spawn gate projectiles
        int entryGate = Projectile.NewProjectile(npc.GetSource_FromAI(), entryPos, 
            Vector2.Zero, ModContent.ProjectileType<DoGChargeGate>(), 0, 0f, Main.myPlayer, 0f);
        int exitGate = Projectile.NewProjectile(npc.GetSource_FromAI(), exitPos, 
            Vector2.Zero, ModContent.ProjectileType<DoGChargeGate>(), 0, 0f, Main.myPlayer, 1f);
        
        // Link gates for teleportation
        if (entryGate >= 0 && exitGate >= 0)
        {
            Main.projectile[entryGate].ai[1] = exitGate;
            Main.projectile[exitGate].ai[1] = entryGate;
        }
    }
}
```

---

## Colosseum Portal (Reality Tear Usage)

### Large-Scale Portal Effect

```csharp
// File: ColosseumPortal.cs (Lines 50-90)
public class ColosseumPortal : ModProjectile
{
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D portalTex = ModContent.Request<Texture2D>(Texture).Value;
        Vector2 drawPos = Projectile.Center - Main.screenPosition;
        Vector2 origin = portalTex.Size() * 0.5f;
        
        // Apply RealityTearVertexShader for cosmic void look
        Effect tearShader = InfernumEffectsRegistry.RealityTearVertexShader;
        tearShader.Parameters["globalTime"].SetValue(Main.GlobalTimeWrappedHourly);
        tearShader.Parameters["voidIntensity"].SetValue(0.8f);
        tearShader.Parameters["edgeColor"].SetValue(new Color(180, 100, 255).ToVector4());
        
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, ...);
        
        tearShader.CurrentTechnique.Passes[0].Apply();
        
        // Main portal body
        float rotation = Main.GlobalTimeWrappedHourly * 0.5f;
        Main.spriteBatch.Draw(portalTex, drawPos, null, Color.White * Projectile.Opacity, 
            rotation, origin, Projectile.scale, 0, 0f);
        
        Main.spriteBatch.End();
        
        // Edge particles
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, ...);
        
        Texture2D glowTex = InfernumTextureRegistry.BloomCircle.Value;
        for (int i = 0; i < 12; i++)
        {
            float angle = TwoPi * i / 12f + Main.GlobalTimeWrappedHourly;
            Vector2 particleOffset = angle.ToRotationVector2() * Projectile.scale * 80f;
            Color particleColor = Color.Lerp(new Color(180, 100, 255), new Color(100, 200, 255), i / 11f) * 0.4f;
            
            Main.spriteBatch.Draw(glowTex, drawPos + particleOffset, null, particleColor * Projectile.Opacity, 
                0f, glowTex.Size() * 0.5f, 0.3f, 0, 0f);
        }
        
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, ...);
        
        return false;
    }
}
```

---

## Key Shaders and Textures

### Shaders Used

| Shader | Purpose | Key Parameters |
|--------|---------|----------------|
| `DoGPortalShader` | Swirling portal effect | GlobalTime, SwirlIntensity, PortalCenter |
| `RealityTearVertexShader` | Void/tear rendering | GlobalTime, TearIntensity, EdgeColor |
| `GenericLaserVertexShader` | Telegraph lasers | Texture |

### Textures Used

| Texture | Purpose |
|---------|---------|
| `LaserCircle` | Portal base texture |
| `BloomLineSmall` | Laser telegraph body |
| `BloomFlare` | Portal glow flare |
| `BloomCircle` | General glow orbs |

---

## Cosmic Color Palette

### Core Colors

```csharp
// Devourer of Gods Cosmic Palette
public static class DoGColors
{
    public static Color CosmicPurple = new Color(180, 80, 220);
    public static Color CosmicCyan = new Color(80, 200, 240);
    public static Color VoidDark = new Color(30, 20, 50);
    public static Color PortalEdge = new Color(150, 100, 200);
    public static Color PortalCore = Color.White;
    public static Color RealityTear = new Color(200, 120, 255);
}
```

---

## Adaptation Guidelines for MagnumOpus

### 1. Fate Theme Connection

DoG's cosmic void aligns perfectly with Fate's reality-bending aesthetic:

```csharp
// Fate portal effect adaptation
public void DrawFatePortal(Vector2 center, float scale, float opacity)
{
    float rotation = Main.GlobalTimeWrappedHourly * 2f;
    
    Main.spriteBatch.SetBlendState(BlendState.Additive);
    
    // Triple-layer portal with Fate colors
    Texture2D portalTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
    Vector2 origin = portalTex.Size() * 0.5f;
    
    // Outer: Dark pink
    Color outer = UnifiedVFX.Fate.DarkPink * 0.4f * opacity;
    Main.spriteBatch.Draw(portalTex, center, null, outer, rotation, origin, scale * 1.4f, 0, 0f);
    
    // Middle: Purple
    Color middle = UnifiedVFX.Fate.Purple * 0.5f * opacity;
    Main.spriteBatch.Draw(portalTex, center, null, middle, -rotation * 0.7f, origin, scale * 1.1f, 0, 0f);
    
    // Core: Bright
    Color core = Color.White * 0.7f * opacity;
    Main.spriteBatch.Draw(portalTex, center, null, core, rotation * 0.5f, origin, scale * 0.8f, 0, 0f);
    
    Main.spriteBatch.SetBlendState(BlendState.AlphaBlend);
}
```

### 2. Reality Slice for Fate Weapons

Adapt RealitySlice for Fate weapon effects:

```csharp
public void DrawFateRealitySlice(Vector2 position, float rotation, float scale, float opacity)
{
    // Draw dark void center
    Texture2D voidTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
    
    Main.spriteBatch.SetBlendState(BlendState.Additive);
    
    // Chromatic aberration effect - separate RGB
    Vector2 offset = new Vector2(3f * scale, 0);
    
    Main.spriteBatch.Draw(voidTex, position - offset, null, Color.Red * 0.3f * opacity, 
        rotation, voidTex.Size() * 0.5f, scale, 0, 0f);
    Main.spriteBatch.Draw(voidTex, position, null, Color.Green * 0.3f * opacity, 
        rotation, voidTex.Size() * 0.5f, scale, 0, 0f);
    Main.spriteBatch.Draw(voidTex, position + offset, null, Color.Blue * 0.3f * opacity, 
        rotation, voidTex.Size() * 0.5f, scale, 0, 0f);
    
    // Main slice
    Color sliceColor = UnifiedVFX.Fate.GetCosmicGradient(0.5f) * opacity;
    Main.spriteBatch.Draw(voidTex, position, null, sliceColor, 
        rotation, voidTex.Size() * 0.5f, scale * new Vector2(3f, 0.3f), 0, 0f);
    
    Main.spriteBatch.SetBlendState(BlendState.AlphaBlend);
}
```

### 3. Worm Segment Portal Transition

For multi-segment bosses with teleportation:

```csharp
public void DrawSegmentPortalTransition(NPC segment, float transitionProgress, Color themeColor)
{
    // Calculate fade alpha
    bool isExiting = segment.ai[2] > 0;
    float alpha = isExiting ? transitionProgress : (1f - transitionProgress);
    
    // Draw segment with fade
    Texture2D segmentTex = TextureAssets.Npc[segment.type].Value;
    Color segmentColor = Color.White * alpha;
    
    Main.spriteBatch.Draw(segmentTex, segment.Center - Main.screenPosition, null, segmentColor, 
        segment.rotation, segmentTex.Size() * 0.5f, segment.scale, 0, 0f);
    
    // Portal glow effect when fading
    if (alpha < 0.9f)
    {
        float glowIntensity = 1f - alpha;
        CustomParticles.GenericFlare(segment.Center, themeColor * glowIntensity, 0.6f * glowIntensity, 10);
        CustomParticles.HaloRing(segment.Center, themeColor * 0.5f * glowIntensity, 0.4f, 15);
    }
}
```

### 4. Circular Laser Burst Pattern

For radial attack patterns:

```csharp
public void SpawnCircularLaserBurst(Vector2 center, int laserCount, float laserSpeed, 
    int damage, Color themeColor)
{
    // Spawn radial lasers
    for (int i = 0; i < laserCount; i++)
    {
        float angle = TwoPi * i / laserCount;
        Vector2 direction = angle.ToRotationVector2();
        
        Projectile.NewProjectile(Entity.GetSource_FromAI(), center, 
            direction * laserSpeed, LaserProjectileType, damage, 0f, Main.myPlayer);
    }
    
    // Central burst VFX
    UnifiedVFX.Generic.Explosion(center, themeColor, Color.White, 2f);
    
    // Radial shockwave
    for (int i = 0; i < laserCount * 2; i++)
    {
        float angle = TwoPi * i / (laserCount * 2);
        Vector2 particlePos = center + angle.ToRotationVector2() * 50f;
        CustomParticles.GenericFlare(particlePos, themeColor, 0.5f, 15);
    }
    
    CustomParticles.HaloRing(center, themeColor, 1f, 25);
}
```

### 5. Laser Wall with Portals

For coordinated portal-based attacks:

```csharp
public class PortalLaserWallSystem
{
    public void SpawnLaserWall(Vector2 center, int wallCount, float spacing, float verticalOffset,
        int laserDamage, Color themeColor)
    {
        for (int i = 0; i < wallCount; i++)
        {
            float xOffset = (i - wallCount * 0.5f) * spacing;
            Vector2 topPos = center + new Vector2(xOffset, -verticalOffset);
            Vector2 bottomPos = center + new Vector2(xOffset, verticalOffset);
            
            // Create linked portal pair
            int topPortal = SpawnPortal(topPos, 1f); // Upward facing
            int bottomPortal = SpawnPortal(bottomPos, -1f); // Downward facing
            
            // Spawn connecting laser
            SpawnConnectingLaser(topPos, bottomPos, laserDamage, themeColor);
        }
    }
    
    private void SpawnConnectingLaser(Vector2 start, Vector2 end, int damage, Color color)
    {
        Vector2 direction = (end - start).SafeNormalize(Vector2.UnitY);
        float length = Vector2.Distance(start, end);
        
        Projectile.NewProjectile(Entity.GetSource_FromAI(), start, direction, 
            LaserProjectileType, damage, 0f, Main.myPlayer, length);
    }
}
```

---

## Key Takeaways

1. **Swirl Shader Math:** Rotation matrices with distance-based angle creates portal effect
2. **Triple-Layer Portals:** Three additive layers with different rotations and scales
3. **Segment Fading:** Alpha interpolation for portal transitions
4. **Laser Walls:** Linked portal pairs with connecting damage beams
5. **Reality Tears:** Specialized shader for void/slice effects
6. **Chromatic Separation:** RGB offset for reality-bending visual
7. **Circular Bursts:** Radial patterns with central shockwave VFX

---

## File References

- `Content/BehaviorOverrides/BossAIs/DoG/DoGPhase2HeadBehaviorOverride.cs`
- `Content/BehaviorOverrides/BossAIs/DoG/DoGSegmentBehaviorOverride.cs`
- `Content/BehaviorOverrides/BossAIs/DoG/RealityBreakPortalLaserWall.cs`
- `Content/BehaviorOverrides/BossAIs/DoG/DoGChargeGate.cs`
- `Content/BehaviorOverrides/BossAIs/DoG/DoGDeathInfernum.cs`
- `Content/BehaviorOverrides/BossAIs/DoG/RealitySlice.cs`
- `Content/BehaviorOverrides/BossAIs/DoG/ColosseumPortal.cs`
- `Assets/Effects/Primitives/DoGPortalShader.fx`
