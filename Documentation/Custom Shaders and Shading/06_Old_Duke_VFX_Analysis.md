# Old Duke VFX Analysis

> **Deep-dive into Old Duke's comprehensive VFX systems: acid fire, bile metaballs, nuclear hurricane, and environmental effects.**
> 
> Old Duke represents one of the most visually intensive bosses in FargosSoulsDLC.

---

## Old Duke Overview

The Old Duke fight features multiple interconnected VFX systems:

| System | Purpose | Key Effects |
|--------|---------|-------------|
| **Fire Particles** | High-performance acid flames | FastParticle system, shader animation |
| **Bile Metaballs** | Slimy blob projectiles | Metaball merging, dissolve shader |
| **Nuclear Hurricane** | Phase 2 vortex attack | Cylinder meshes, multi-shader layers |
| **Environmental** | Rain, sky, water tint | Screen filters, color grading |

---

## Fire Particle System

### OldDukeFireParticleSystemManager

**Purpose:** Manages thousands of fire particles with optimal performance

```csharp
public class OldDukeFireParticleSystemManager : ModSystem
{
    // Performance: Use arrays instead of lists where possible
    private static FastParticle[] particles = new FastParticle[10000];
    private static int particleCount = 0;
    
    // Texture animation frames
    private static Texture2D fireTextureA;
    private static Texture2D fireTextureB;
    
    public override void Load()
    {
        fireTextureA = ModContent.Request<Texture2D>("FargowiltasCrossmod/Assets/Textures/FireParticleA").Value;
        fireTextureB = ModContent.Request<Texture2D>("FargowiltasCrossmod/Assets/Textures/FireParticleB").Value;
    }
    
    public static void SpawnParticle(Vector2 position, Vector2 velocity, Color color, float scale)
    {
        if (particleCount >= particles.Length)
            return;
        
        particles[particleCount] = new FastParticle
        {
            Position = position,
            Velocity = velocity,
            Color = color,
            Scale = scale,
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
            Time = 0,
            Lifetime = Main.rand.Next(20, 40),
            TextureVariant = Main.rand.NextBool() ? 0 : 1
        };
        particleCount++;
    }
    
    public override void PostUpdateDusts()
    {
        // Update all particles with minimal overhead
        int writeIndex = 0;
        for (int i = 0; i < particleCount; i++)
        {
            ref FastParticle p = ref particles[i];
            p.Time++;
            
            if (p.Time < p.Lifetime)
            {
                // Physics
                p.Position += p.Velocity;
                p.Velocity *= 0.96f;
                p.Velocity.Y -= 0.08f; // Rise
                p.Velocity.X += Main.rand.NextFloat(-0.1f, 0.1f); // Drift
                p.Rotation += p.Velocity.Length() * 0.03f;
                
                // Keep particle
                particles[writeIndex] = p;
                writeIndex++;
            }
        }
        particleCount = writeIndex;
    }
}
```

### Fire Particle Drawing with Shader

```csharp
public void DrawFireParticles(SpriteBatch spriteBatch)
{
    if (particleCount == 0)
        return;
    
    // Begin additive blending for glow
    spriteBatch.End();
    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
    
    // Apply fire dissolve shader
    ManagedShader shader = ShaderManager.GetShader("FargowiltasCrossmod.FireParticleDissolveShader");
    shader.SetTexture(fireTextureA, 1, SamplerState.LinearWrap);
    shader.SetTexture(fireTextureB, 2, SamplerState.LinearWrap);
    shader.SetTexture(NoiseTexturesRegistry.FireParticleA.Value, 3, SamplerState.LinearWrap);
    shader.TrySetParameter("turbulence", 0.3f);
    shader.TrySetParameter("initialGlowIntensity", 1.5f);
    shader.TrySetParameter("initialGlowDuration", 0.2f);
    shader.TrySetParameter("pixelationLevel", 4f);
    shader.Apply();
    
    // Batch draw all particles
    for (int i = 0; i < particleCount; i++)
    {
        ref FastParticle p = ref particles[i];
        float progress = p.Time / (float)p.Lifetime;
        
        // Select texture based on variant
        Texture2D tex = p.TextureVariant == 0 ? fireTextureA : fireTextureB;
        
        // Calculate draw parameters
        float alpha = 1f - progress;
        float scale = p.Scale * (1f + progress * 0.5f); // Expand as it fades
        Color drawColor = p.Color * alpha;
        
        Vector2 drawPos = p.Position - Main.screenPosition;
        spriteBatch.Draw(tex, drawPos, null, drawColor, p.Rotation, 
            tex.Size() * 0.5f, scale, 0, 0f);
    }
    
    // Reset to normal blending
    spriteBatch.End();
    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
}
```

### Fire Color Palette

```csharp
public static class OldDukeFireColors
{
    // Acid green fire palette
    public static Color BrightGreen = new Color(100, 255, 50);
    public static Color MediumGreen = new Color(50, 200, 30);
    public static Color DarkGreen = new Color(20, 100, 10);
    
    // Rage mode - more yellow
    public static Color RageBright = new Color(200, 255, 50);
    public static Color RageMedium = new Color(150, 200, 30);
    
    public static Color GetRandomFireColor(bool enraged = false)
    {
        float t = Main.rand.NextFloat();
        if (enraged)
            return Color.Lerp(RageMedium, RageBright, t);
        return Color.Lerp(DarkGreen, BrightGreen, t);
    }
}
```

---

## Bile Metaball System

### BileMetaball Class

```csharp
public class BileMetaball : MetaballType
{
    public override string MetaballTexture => "FargowiltasCrossmod/Assets/Textures/MetaballCircle";
    
    private List<BileBlob> blobs = new();
    
    public struct BileBlob
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Size;
        public int Time;
        public int Lifetime;
        public float Darkening; // 0 = normal, 1 = very dark
    }
    
    public static void SpawnBlob(Vector2 position, Vector2 velocity, float size, int lifetime, float darkening = 0f)
    {
        Instance.blobs.Add(new BileBlob
        {
            Position = position,
            Velocity = velocity,
            Size = size,
            Time = 0,
            Lifetime = lifetime,
            Darkening = darkening
        });
    }
    
    public override void Update()
    {
        for (int i = blobs.Count - 1; i >= 0; i--)
        {
            var b = blobs[i];
            b.Time++;
            
            // Physics
            b.Position += b.Velocity;
            b.Velocity.Y += 0.2f; // Heavy gravity
            b.Velocity *= 0.98f;
            
            // Tile collision - splat
            if (Collision.SolidCollision(b.Position - new Vector2(b.Size * 8), (int)(b.Size * 16), (int)(b.Size * 16)))
            {
                // Spawn splatter particles
                SpawnSplatter(b.Position, b.Size);
                blobs.RemoveAt(i);
                continue;
            }
            
            if (b.Time >= b.Lifetime)
                blobs.RemoveAt(i);
            else
                blobs[i] = b;
        }
    }
    
    private void SpawnSplatter(Vector2 position, float size)
    {
        // Smaller blobs spray out
        for (int i = 0; i < 6; i++)
        {
            Vector2 vel = Main.rand.NextVector2CircularEdge(4f, 4f);
            vel.Y = -Math.Abs(vel.Y) * 1.5f; // Upward bias
            
            SpawnBlob(position, vel, size * 0.3f, 30, 0.3f);
        }
    }
    
    public override void DrawToTarget(SpriteBatch spriteBatch)
    {
        Texture2D circle = ModContent.Request<Texture2D>(MetaballTexture).Value;
        
        foreach (var b in blobs)
        {
            float progress = b.Time / (float)b.Lifetime;
            float drawSize = b.Size * (1f - progress * 0.2f);
            
            // Encode data in color channels for shader:
            // R = lifetime ratio (for color gradient)
            // G = darkening factor
            // B = dissolve interpolant (starts at 0.7 progress)
            float dissolve = progress > 0.7f ? (progress - 0.7f) / 0.3f : 0f;
            
            Color dataColor = new Color(
                (byte)(progress * 255),
                (byte)(b.Darkening * 255),
                (byte)(dissolve * 255),
                255);
            
            Vector2 drawPos = b.Position - Main.screenPosition;
            spriteBatch.Draw(circle, drawPos, null, dataColor,
                0f, circle.Size() * 0.5f, drawSize, 0, 0f);
        }
    }
    
    public override void DrawAfterTiles(SpriteBatch spriteBatch)
    {
        ManagedShader shader = ShaderManager.GetShader("FargowiltasCrossmod.OldDukeBileMetaballShader");
        
        // Set bile color gradient (gross green)
        Vector3[] bileGradient = {
            new Color(80, 180, 60).ToVector3(),   // Bright
            new Color(50, 120, 40).ToVector3(),   // Medium
            new Color(30, 80, 20).ToVector3(),    // Dark
            new Color(20, 50, 15).ToVector3(),    // Very dark
        };
        shader.TrySetParameter("gradient", bileGradient);
        shader.TrySetParameter("gradientCount", 4f);
        shader.TrySetParameter("dissolvePersistence", 0.3f);
        shader.TrySetParameter("maxDistortionOffset", new Vector2(0.02f, 0.02f));
        
        // Set textures
        shader.SetTexture(OverlayTexture, 1, SamplerState.LinearWrap);
        shader.SetTexture(NoiseTexturesRegistry.BubblyNoise.Value, 2, SamplerState.LinearWrap);
        shader.SetTexture(BloodTexture, 3, SamplerState.LinearWrap);
        
        // Draw metaball render target with shader
        spriteBatch.PrepareForShaders();
        shader.Apply();
        spriteBatch.Draw(MetaballTarget, Vector2.Zero, Color.White);
        spriteBatch.ResetToDefault();
    }
}
```

### Bile Metaball Shader (HLSL)

```hlsl
sampler metaballContents : register(s0);
sampler overlayTexture : register(s1);
sampler dissolveNoiseTexture : register(s2);
sampler bloodTexture : register(s3);

float gradientCount;
float3 gradient[8];
float dissolvePersistence;
float2 maxDistortionOffset;
float2 screenSize;
float2 layerSize;
float2 layerOffset;

float3 PaletteLerp(float interpolant)
{
    int startIndex = clamp(interpolant * gradientCount, 0, gradientCount - 1);
    int endIndex = clamp(startIndex + 1, 0, gradientCount - 1);
    return lerp(gradient[startIndex], gradient[endIndex], frac(interpolant * gradientCount));
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Convert to world UV
    float2 worldUV = (coords + layerOffset) * screenSize / layerSize;
    
    // Sample metaball data
    float4 metaballData = tex2D(metaballContents, coords);
    float lifetimeRatio = metaballData.r;
    float darkening = metaballData.g;
    float dissolveInterpolant = metaballData.b;
    
    // Early out if no metaball here
    if (metaballData.a < 0.01)
        return 0;
    
    // Calculate distance from blob center for effects
    float2 gradient = float2(
        tex2D(metaballContents, coords + float2(0.01, 0)).a - 
        tex2D(metaballContents, coords - float2(0.01, 0)).a,
        tex2D(metaballContents, coords + float2(0, 0.01)).a - 
        tex2D(metaballContents, coords - float2(0, 0.01)).a
    );
    float edgeFactor = length(gradient);
    
    // Dissolve with noise
    float dissolveNoise = tex2D(dissolveNoiseTexture, worldUV * 2);
    float dissolveOpacity = smoothstep(0, dissolvePersistence, dissolveNoise - dissolveInterpolant);
    
    // Edge distortion for goopy look
    float2 distortionOffset = gradient * maxDistortionOffset;
    float2 distortedWorldUV = worldUV + distortionOffset;
    
    // Sample overlay texture for surface detail
    float overlay = tex2D(overlayTexture, distortedWorldUV * 3).r;
    
    // Get color from palette based on lifetime and darkening
    float colorIndex = lifetimeRatio * 0.7 + darkening * 0.3;
    float3 bileColor = PaletteLerp(colorIndex);
    
    // Add overlay detail
    bileColor *= 0.8 + overlay * 0.4;
    
    // Edge highlight
    bileColor += edgeFactor * 0.3;
    
    return float4(bileColor, metaballData.a * dissolveOpacity);
}
```

---

## Nuclear Hurricane System

The Nuclear Hurricane is a multi-layered vortex attack using cylinder mesh rendering.

### Hurricane Component Structure

| Layer | Shader | Purpose |
|-------|--------|---------|
| Core | NuclearHurricaneCoreShader | Inner swirling mass |
| Glow | NuclearHurricaneGlowShader | Outer glow ring |
| Foam | NuclearHurricaneFoamShader | Frothy surface detail |
| Extremes | NuclearHurricaneExtremesShader | Top/bottom caps |

### NuclearHurricane Projectile

```csharp
public class OldDukeNuclearHurricane : ModProjectile
{
    // Cylinder mesh vertices
    private VertexPositionColorTexture[] coreVertices;
    private VertexPositionColorTexture[] glowVertices;
    private VertexPositionColorTexture[] foamVertices;
    
    private const int CylinderSegments = 32;
    private const int CylinderRings = 16;
    
    public override void OnSpawn(IEntitySource source)
    {
        // Generate cylinder meshes
        GenerateCylinderMesh(ref coreVertices, CoreRadius);
        GenerateCylinderMesh(ref glowVertices, GlowRadius);
        GenerateCylinderMesh(ref foamVertices, FoamRadius);
    }
    
    private void GenerateCylinderMesh(ref VertexPositionColorTexture[] vertices, float radius)
    {
        vertices = new VertexPositionColorTexture[CylinderSegments * CylinderRings * 6];
        int index = 0;
        
        for (int ring = 0; ring < CylinderRings; ring++)
        {
            float y1 = ring / (float)CylinderRings;
            float y2 = (ring + 1) / (float)CylinderRings;
            
            for (int seg = 0; seg < CylinderSegments; seg++)
            {
                float angle1 = MathHelper.TwoPi * seg / CylinderSegments;
                float angle2 = MathHelper.TwoPi * (seg + 1) / CylinderSegments;
                
                // Two triangles per quad
                // Triangle 1
                vertices[index++] = CreateVertex(angle1, y1, radius);
                vertices[index++] = CreateVertex(angle2, y1, radius);
                vertices[index++] = CreateVertex(angle1, y2, radius);
                
                // Triangle 2
                vertices[index++] = CreateVertex(angle2, y1, radius);
                vertices[index++] = CreateVertex(angle2, y2, radius);
                vertices[index++] = CreateVertex(angle1, y2, radius);
            }
        }
    }
    
    private VertexPositionColorTexture CreateVertex(float angle, float y, float radius)
    {
        // Apply QuadraticBump for cylinder shape
        float bump = y * (4f - y * 4f); // QuadraticBump
        float adjustedRadius = radius * (1f + bump * 0.3f); // Bulge in middle
        
        return new VertexPositionColorTexture(
            new Vector3(MathF.Cos(angle) * adjustedRadius, y * HurricaneHeight, MathF.Sin(angle) * adjustedRadius),
            Color.White,
            new Vector2(angle / MathHelper.TwoPi, y)
        );
    }
    
    public override void AI()
    {
        // Spin
        Projectile.rotation += 0.05f;
        
        // Scale up/down based on lifetime
        float lifeProgress = Projectile.timeLeft / (float)MaxLifetime;
        float scale = lifeProgress < 0.1f ? lifeProgress / 0.1f : 
                     (lifeProgress > 0.9f ? (1f - lifeProgress) / 0.1f : 1f);
        
        Projectile.scale = scale;
        
        // Spawn edge particles
        SpawnHurricaneParticles();
        
        // Suction effect on nearby entities
        ApplySuctionEffect();
    }
    
    private void SpawnHurricaneParticles()
    {
        // Fire particles spiral around
        float spawnAngle = Main.GlobalTimeWrappedHourly * 5f;
        for (int i = 0; i < 3; i++)
        {
            float angle = spawnAngle + MathHelper.TwoPi * i / 3f;
            float radius = HurricaneRadius * Main.rand.NextFloat(0.8f, 1.2f);
            float height = Main.rand.NextFloat() * HurricaneHeight;
            
            Vector2 spawnPos = Projectile.Center + new Vector2(MathF.Cos(angle) * radius, -height);
            Vector2 velocity = new Vector2(-MathF.Sin(angle), -0.5f) * 3f;
            
            OldDukeFireParticleSystemManager.SpawnParticle(spawnPos, velocity,
                OldDukeFireColors.GetRandomFireColor(), 0.8f);
        }
        
        // Debris sucked in
        if (Main.rand.NextBool(3))
        {
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 debrisPos = Projectile.Center + angle.ToRotationVector2() * (HurricaneRadius + 100f);
            Vector2 debrisVel = (Projectile.Center - debrisPos).SafeNormalize(Vector2.Zero) * 5f;
            
            GeneralParticleHandler.SpawnParticle(new HeavyStoneDebrisParticle(
                debrisPos, debrisVel, Color.Gray, 0.5f, 60));
        }
    }
}
```

### Hurricane Drawing System

```csharp
public override bool PreDraw(ref Color lightColor)
{
    // Render in 3D
    GraphicsDevice device = Main.graphics.GraphicsDevice;
    
    // Set up view/projection matrices
    Matrix view = Matrix.CreateLookAt(
        new Vector3(0, -200, 400), // Camera position
        Vector3.Zero,             // Look at center
        Vector3.Up);
    
    Matrix projection = Matrix.CreatePerspectiveFieldOfView(
        MathHelper.PiOver4, 
        device.Viewport.AspectRatio, 
        1f, 10000f);
    
    Matrix world = Matrix.CreateRotationY(Projectile.rotation) *
                   Matrix.CreateScale(Projectile.scale) *
                   Matrix.CreateTranslation(Projectile.Center.X - Main.screenPosition.X, 
                                           Projectile.Center.Y - Main.screenPosition.Y, 0);
    
    // Draw each layer with its shader
    DrawCylinderLayer(coreVertices, "NuclearHurricaneCoreShader", world, view, projection);
    DrawCylinderLayer(glowVertices, "NuclearHurricaneGlowShader", world, view, projection);
    DrawCylinderLayer(foamVertices, "NuclearHurricaneFoamShader", world, view, projection);
    
    return false;
}

private void DrawCylinderLayer(VertexPositionColorTexture[] vertices, string shaderName, 
    Matrix world, Matrix view, Matrix projection)
{
    ManagedShader shader = ShaderManager.GetShader($"FargowiltasCrossmod.{shaderName}");
    
    shader.TrySetParameter("uWorldViewProjection", world * view * projection);
    shader.TrySetParameter("localTime", Main.GlobalTimeWrappedHourly);
    shader.TrySetParameter("wavinessFactor", 0.1f);
    shader.TrySetParameter("maxBumpSquish", 0.8f);
    
    // Set noise textures
    shader.SetTexture(NoiseTexturesRegistry.PerlinNoise.Value, 1, SamplerState.LinearWrap);
    shader.SetTexture(NoiseTexturesRegistry.BubblyNoise.Value, 2, SamplerState.LinearWrap);
    
    shader.Apply();
    
    // Draw the cylinder
    Main.graphics.GraphicsDevice.DrawUserPrimitives(
        PrimitiveType.TriangleList, vertices, 0, vertices.Length / 3);
}
```

### Nuclear Hurricane Shader (Core)

```hlsl
float4x4 uWorldViewProjection;
float localTime;
float wavinessFactor;
float maxBumpSquish;

sampler noiseTexture : register(s1);
sampler detailTexture : register(s2);

float QuadraticBump(float x)
{
    return x * (4 - x * 4);
}

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output;
    
    float2 coords = input.TextureCoordinates;
    float bump = QuadraticBump(coords.y);
    
    // Wavy distortion
    float wave = sin(coords.y * 12.566 + localTime * 3) * wavinessFactor;
    input.Position.x += wave * bump;
    input.Position.z += cos(coords.y * 12.566 + localTime * 3) * wavinessFactor * bump;
    
    // Squish at peak
    float squish = lerp(1, maxBumpSquish, bump);
    input.Position.x *= squish;
    input.Position.z *= squish;
    
    output.Position = mul(input.Position, uWorldViewProjection);
    output.Color = input.Color;
    output.TextureCoordinates = coords;
    
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TextureCoordinates;
    
    // Scrolling noise for swirl effect
    float2 scrolledCoords = coords + float2(localTime * 0.5, localTime * 0.2);
    float noise = tex2D(noiseTexture, scrolledCoords * float2(3, 1)).r;
    float detail = tex2D(detailTexture, scrolledCoords * float2(5, 2)).r;
    
    // Color gradient (green hurricane)
    float3 innerColor = float3(0.2, 0.8, 0.3);  // Bright green
    float3 outerColor = float3(0.1, 0.4, 0.15); // Dark green
    float3 hurricaneColor = lerp(outerColor, innerColor, noise * detail);
    
    // Edge fade
    float edgeFade = QuadraticBump(coords.y) * 0.8 + 0.2;
    
    // Add glow
    float glow = pow(noise, 2) * 0.5;
    hurricaneColor += glow;
    
    return float4(hurricaneColor, edgeFade);
}
```

---

## Environmental Effects

### Acid Rain Filter

```csharp
public class OldDukeRainSystem : ModSystem
{
    private static bool rainActive = false;
    private static float rainIntensity = 0f;
    
    public static void StartRain(float intensity)
    {
        rainActive = true;
        rainIntensity = intensity;
    }
    
    public override void PostDrawTiles()
    {
        if (!rainActive || rainIntensity <= 0f)
            return;
        
        ManagedShader shader = ShaderManager.GetShader("FargowiltasCrossmod.OldDukeRainShader");
        
        shader.TrySetParameter("rainAngle", 0.3f); // Slight angle
        shader.TrySetParameter("rainOpacity", rainIntensity);
        shader.TrySetParameter("time", Main.GlobalTimeWrappedHourly);
        shader.TrySetParameter("zoom", Main.GameViewMatrix.Zoom.X);
        shader.TrySetParameter("rainColor", new Color(80, 200, 80, 100).ToVector4()); // Green tint
        
        shader.SetTexture(NoiseTexturesRegistry.PerlinNoise.Value, 1, SamplerState.LinearWrap);
        
        Main.spriteBatch.PrepareForShaders();
        shader.Apply();
        Main.spriteBatch.Draw(Main.screenTarget, Vector2.Zero, Color.White);
        Main.spriteBatch.ResetToDefault();
    }
}
```

### Sky Darkening

```csharp
public class OldDukeSky : CustomSky
{
    private float intensity = 0f;
    
    public override void Update(GameTime gameTime)
    {
        // Gradually increase intensity during fight
        if (OldDukeBossActive)
            intensity = Math.Min(intensity + 0.01f, 1f);
        else
            intensity = Math.Max(intensity - 0.02f, 0f);
    }
    
    public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
    {
        if (intensity <= 0f)
            return;
        
        // Dark green overlay
        Color skyColor = new Color(20, 50, 30) * intensity * 0.7f;
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, 
            new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
            skyColor);
        
        // Toxic clouds
        DrawToxicClouds(spriteBatch);
    }
    
    private void DrawToxicClouds(SpriteBatch spriteBatch)
    {
        Texture2D cloudTex = ModContent.Request<Texture2D>("Path/To/CloudTexture").Value;
        
        for (int i = 0; i < 5; i++)
        {
            float scroll = Main.GlobalTimeWrappedHourly * 0.1f + i * 0.3f;
            float yOffset = MathF.Sin(scroll * 2f) * 20f;
            
            Vector2 cloudPos = new Vector2(
                (scroll * Main.screenWidth) % (Main.screenWidth + cloudTex.Width) - cloudTex.Width,
                50 + i * 80 + yOffset
            );
            
            Color cloudColor = new Color(30, 80, 40) * intensity * (0.3f + i * 0.1f);
            spriteBatch.Draw(cloudTex, cloudPos, cloudColor);
        }
    }
    
    public override Color OnTileColor(Color inColor)
    {
        // Tint all tiles slightly green
        return Color.Lerp(inColor, new Color(150, 255, 150), intensity * 0.3f);
    }
}
```

### Water Tint Effect

```csharp
// Applied to water in Old Duke's arena
public class ToxicWaterTintSystem : ModSystem
{
    public override void ModifyLightingBrightness(ref float scale)
    {
        if (!InOldDukeArena)
            return;
        
        // Reduce light slightly for ominous feel
        scale *= 0.9f;
    }
    
    public override void PostDrawTiles()
    {
        if (!InOldDukeArena)
            return;
        
        // Draw toxic glow on water surface
        foreach (var waterTile in GetWaterSurfaceTiles())
        {
            Vector2 worldPos = waterTile.ToWorldCoordinates();
            Vector2 screenPos = worldPos - Main.screenPosition;
            
            // Bubbling effect
            float bubble = MathF.Sin(Main.GlobalTimeWrappedHourly * 3f + waterTile.X * 0.5f) * 0.3f + 0.7f;
            
            Texture2D glow = MiscTexturesRegistry.BloomCircleSmall.Value;
            Main.spriteBatch.Draw(glow, screenPos, null,
                new Color(50, 150, 50) with { A = 0 } * bubble * 0.3f,
                0f, glow.Size() * 0.5f, 0.5f, 0, 0f);
        }
    }
}
```

---

## Old Duke Attack VFX Patterns

### Charge Attack

```csharp
public void ChargeVFX(Vector2 startPos, Vector2 endPos, float progress)
{
    // Trail fire
    Vector2 currentPos = Vector2.Lerp(startPos, endPos, progress);
    for (int i = 0; i < 5; i++)
    {
        Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
        OldDukeFireParticleSystemManager.SpawnParticle(
            currentPos + offset,
            -NPC.velocity * 0.2f + Main.rand.NextVector2Circular(2f, 2f),
            OldDukeFireColors.GetRandomFireColor(),
            Main.rand.NextFloat(0.5f, 1f));
    }
    
    // Bile drips
    if (Main.rand.NextBool(5))
    {
        BileMetaball.SpawnBlob(
            currentPos + Main.rand.NextVector2Circular(30f, 30f),
            Main.rand.NextVector2Circular(3f, 3f),
            Main.rand.NextFloat(0.3f, 0.6f),
            40);
    }
}
```

### Vomit Attack

```csharp
public void VomitAttackVFX(Vector2 mouthPos, Vector2 direction)
{
    // Main bile stream
    for (int i = 0; i < 8; i++)
    {
        float spread = Main.rand.NextFloat(-0.3f, 0.3f);
        Vector2 bileVel = direction.RotatedBy(spread) * Main.rand.NextFloat(8f, 15f);
        
        BileMetaball.SpawnBlob(
            mouthPos + Main.rand.NextVector2Circular(10f, 10f),
            bileVel,
            Main.rand.NextFloat(0.5f, 1f),
            60,
            Main.rand.NextFloat(0f, 0.3f));
    }
    
    // Fire spray
    for (int i = 0; i < 10; i++)
    {
        float spread = Main.rand.NextFloat(-0.4f, 0.4f);
        Vector2 fireVel = direction.RotatedBy(spread) * Main.rand.NextFloat(6f, 12f);
        
        OldDukeFireParticleSystemManager.SpawnParticle(
            mouthPos,
            fireVel,
            OldDukeFireColors.GetRandomFireColor(),
            Main.rand.NextFloat(0.8f, 1.5f));
    }
    
    // Glow at mouth
    GeneralParticleHandler.SpawnParticle(new StrongBloom(
        mouthPos, Vector2.Zero, new Color(100, 200, 80), 2f, 10));
}
```

### Death Explosion

```csharp
public void DeathExplosionVFX()
{
    Vector2 center = NPC.Center;
    
    // Massive bile burst
    for (int i = 0; i < 50; i++)
    {
        float angle = MathHelper.TwoPi * i / 50f;
        Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 20f);
        
        BileMetaball.SpawnBlob(center, vel, Main.rand.NextFloat(0.5f, 1.5f), 80);
    }
    
    // Fire explosion
    for (int i = 0; i < 100; i++)
    {
        Vector2 vel = Main.rand.NextVector2CircularEdge(15f, 15f);
        OldDukeFireParticleSystemManager.SpawnParticle(
            center + Main.rand.NextVector2Circular(30f, 30f),
            vel,
            OldDukeFireColors.GetRandomFireColor(enraged: true),
            Main.rand.NextFloat(1f, 2f));
    }
    
    // Flash
    GeneralParticleHandler.SpawnParticle(new DirectionalStrongBloom(
        center, Vector2.Zero, new Color(100, 255, 100), 5f, 30));
    
    // Screen shake
    ScreenShakeSystem.StartShake(30f, shakeStrengthDissipationIncrement: 0.5f);
    
    // Spawn lingering toxic cloud
    Projectile.NewProjectile(source, center, Vector2.Zero,
        ModContent.ProjectileType<OldDukeDeathCloud>(), 0, 0);
}
```

---

## MagnumOpus Adaptation: Old Duke Style Effects

### Theme Mappings

| Old Duke Element | MagnumOpus Theme | Adaptation |
|------------------|------------------|------------|
| Acid Green Fire | Enigma Variations | Purple/green eerie flames |
| Bile Metaballs | None (unique) | Could use for Enigma mystery blobs |
| Hurricane Vortex | Fate | Cosmic vortex with star particles |
| Toxic Rain | Enigma | Mystery rain with question marks |

### Enigma Fire Adaptation

```csharp
public static class EnigmaFireParticles
{
    public static Color BrightPurple = new Color(180, 100, 255);
    public static Color EerieGreen = new Color(50, 220, 100);
    public static Color VoidBlack = new Color(20, 10, 30);
    
    public static void SpawnEnigmaFlame(Vector2 position, Vector2 velocity)
    {
        // Use same high-performance system
        EnigmaFireParticleSystemManager.SpawnParticle(
            position, velocity,
            Color.Lerp(BrightPurple, EerieGreen, Main.rand.NextFloat()),
            Main.rand.NextFloat(0.6f, 1.2f));
    }
}
```

---

*Extracted from FargosSoulsDLC for MagnumOpus VFX development reference.*
