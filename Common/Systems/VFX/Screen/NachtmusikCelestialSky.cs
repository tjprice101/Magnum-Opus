using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using MagnumOpus.Content.Nachtmusik.Bosses;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// NACHTMUSIK CELESTIAL SKY — Mozart's serenade to the night sky.
    /// A 4-phase planetarium that transforms from serene evening star to blinding supernova.
    ///
    /// Phase 1 — Evening Star: Planetarium dome. Constellations form as connect-the-dot star
    ///   particles. Gentle starlight dust drifts down.
    /// Phase 2 — Cosmic Dance: Constellations rotate and orbit. Nebula clouds of deep indigo
    ///   and cosmic blue form, pulsing with internal starlight.
    /// Phase 3 — Celestial Crescendo: Entire sky becomes a slowly rotating galaxy.
    ///   Prismatic refraction trails, comet orbits.
    /// Phase 4 (Enrage) — Supernova: Serene night explodes into blinding stellar radiance.
    ///   Permanent aurora-trail afterimages across the sky.
    ///
    /// Palette: deep indigo, starlight silver, cosmic blue, nebula purple, white radiance.
    /// </summary>
    public class NachtmusikCelestialSky : CustomSky
    {
        private bool isActive = false;
        private float intensity = 0f;
        private int animationTimer = 0;

        // Phase tracking (1=evening star, 2=cosmic dance, 3=crescendo, 4=supernova)
        private int currentPhase = 1;
        private float phaseTransition = 0f;
        private float galaxyRotation = 0f;

        // Flash overlay
        private float flashIntensity = 0f;
        private Color flashColor = Color.White;

        // ── Constellation System ──
        private List<Constellation> constellations = new List<Constellation>();
        private bool constellationsInitialized = false;
        private float constellationOrbitAngle = 0f;

        private struct ConstellationStar
        {
            public Vector2 RelativePosition;
            public float Brightness;
            public float TwinkleOffset;
            public float Scale;
        }

        private struct Constellation
        {
            public ConstellationStar[] Stars;
            public int[] ConnectionA;
            public int[] ConnectionB;
            public float FormProgress;
            public float Depth;
            public Vector2 Pivot;
        }

        // ── Star Dust Particles ──
        private List<StarDust> starDustParticles = new List<StarDust>();
        private const int MaxStarDust = 180;

        private struct StarDust
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float Opacity;
            public Color Tint;
            public int Lifetime;
            public int TimeAlive;
            public float TwinklePhase;
            public int Type; // 0=tiny star, 1=starlight dust, 2=comet tail, 3=aurora wisp
        }

        // ── Nebula Clouds (Phase 2+) ──
        private struct NebulaCloud
        {
            public Vector2 Position;
            public float Scale;
            public float Rotation;
            public float PulsePhase;
            public Color BaseColor;
            public float Opacity;
        }
        private List<NebulaCloud> nebulaClouds = new List<NebulaCloud>();

        // ── Aurora Trails (Enrage) ──
        private List<AuroraTrail> auroraTrails = new List<AuroraTrail>();
        private const int MaxAuroraTrails = 24;

        private struct AuroraTrail
        {
            public Vector2 Position;
            public float Width;
            public float Opacity;
            public Color Color;
            public float WaveOffset;
        }

        // ── Theme Colors ──
        private static readonly Color DeepIndigo = new Color(25, 20, 65);
        private static readonly Color StarlightSilver = new Color(200, 215, 240);
        private static readonly Color CosmicBlue = new Color(60, 100, 190);
        private static readonly Color NebulaPurple = new Color(70, 40, 120);
        private static readonly Color WhiteRadiance = new Color(245, 245, 255);
        private static readonly Color DarkSky = new Color(8, 6, 22);
        private static readonly Color MidnightIndigo = new Color(15, 12, 40);

        public override void OnLoad()
        {
            constellations = new List<Constellation>();
            starDustParticles = new List<StarDust>();
            nebulaClouds = new List<NebulaCloud>();
            auroraTrails = new List<AuroraTrail>();
            constellationsInitialized = false;
        }
        
        // ═══════ CONSTELLATION GENERATION ═══════

        private void InitializeConstellations()
        {
            constellations.Clear();
            int count = 6 + Main.rand.Next(3);
            for (int c = 0; c < count; c++)
            {
                int starCount = 4 + Main.rand.Next(5);
                var stars = new ConstellationStar[starCount];
                Vector2 center = new Vector2(
                    0.08f + Main.rand.NextFloat() * 0.84f,
                    0.05f + Main.rand.NextFloat() * 0.45f
                );

                for (int s = 0; s < starCount; s++)
                {
                    stars[s] = new ConstellationStar
                    {
                        RelativePosition = center + new Vector2(
                            Main.rand.NextFloat(-0.08f, 0.08f),
                            Main.rand.NextFloat(-0.06f, 0.06f)
                        ),
                        Brightness = 0.5f + Main.rand.NextFloat() * 0.5f,
                        TwinkleOffset = Main.rand.NextFloat(MathHelper.TwoPi),
                        Scale = 0.4f + Main.rand.NextFloat() * 0.8f
                    };
                }

                var conA = new List<int>();
                var conB = new List<int>();
                for (int s = 1; s < starCount; s++)
                {
                    conA.Add(s - 1);
                    conB.Add(s);
                }
                int branches = Main.rand.Next(1, 3);
                for (int b = 0; b < branches && starCount > 3; b++)
                {
                    int a = Main.rand.Next(starCount);
                    int bIdx = Main.rand.Next(starCount);
                    if (a != bIdx)
                    {
                        conA.Add(a);
                        conB.Add(bIdx);
                    }
                }

                constellations.Add(new Constellation
                {
                    Stars = stars,
                    ConnectionA = conA.ToArray(),
                    ConnectionB = conB.ToArray(),
                    FormProgress = 0f,
                    Depth = 0.3f + Main.rand.NextFloat() * 0.5f,
                    Pivot = center
                });
            }
            constellationsInitialized = true;
        }

        // ═══════ PHASE CONTROL ═══════

        public void SetPhase(int phase)
        {
            if (phase != currentPhase)
            {
                currentPhase = Math.Clamp(phase, 1, 4);
                phaseTransition = 0f;
                if (phase == 2) SpawnNebulaClouds();
                if (phase == 4) TriggerFlash(1.5f, WhiteRadiance);
            }
        }

        // Legacy compatibility API used by older boss code.
        public void ActivatePhase2()
        {
            SetPhase(2);
        }

        public void TriggerFlash(float strength = 1f, Color? color = null)
        {
            flashIntensity = Math.Max(flashIntensity, strength);
            flashColor = color ?? WhiteRadiance;
        }

        public void AddAuroraTrail(Vector2 worldPos)
        {
            if (auroraTrails.Count >= MaxAuroraTrails)
                auroraTrails.RemoveAt(0);

            float hue = Main.rand.NextFloat();
            Color auroraColor;
            if (hue < 0.33f)
                auroraColor = Color.Lerp(CosmicBlue, NebulaPurple, hue * 3f);
            else if (hue < 0.66f)
                auroraColor = Color.Lerp(NebulaPurple, StarlightSilver, (hue - 0.33f) * 3f);
            else
                auroraColor = Color.Lerp(StarlightSilver, CosmicBlue, (hue - 0.66f) * 3f);

            auroraTrails.Add(new AuroraTrail
            {
                Position = worldPos,
                Width = 30f + Main.rand.NextFloat() * 60f,
                Opacity = 0.7f,
                Color = auroraColor,
                WaveOffset = Main.rand.NextFloat(MathHelper.TwoPi)
            });
        }

        private void SpawnNebulaClouds()
        {
            nebulaClouds.Clear();
            int count = 5 + Main.rand.Next(3);
            for (int i = 0; i < count; i++)
            {
                Color baseCol = Main.rand.NextBool()
                    ? Color.Lerp(DeepIndigo, NebulaPurple, Main.rand.NextFloat())
                    : Color.Lerp(CosmicBlue, DeepIndigo, Main.rand.NextFloat());

                nebulaClouds.Add(new NebulaCloud
                {
                    Position = new Vector2(Main.rand.NextFloat(0.05f, 0.95f), Main.rand.NextFloat(0.1f, 0.7f)),
                    Scale = 60f + Main.rand.NextFloat() * 80f,
                    Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
                    PulsePhase = Main.rand.NextFloat(MathHelper.TwoPi),
                    BaseColor = baseCol,
                    Opacity = 0f
                });
            }
        }

        // ═══════ UPDATE ═══════

        public override void Update(GameTime gameTime)
        {
            animationTimer++;
            if (!constellationsInitialized) InitializeConstellations();

            if (isActive && intensity < 1f) intensity += 0.015f;
            else if (!isActive && intensity > 0f) intensity -= 0.025f;
            intensity = MathHelper.Clamp(intensity, 0f, 1f);

            if (phaseTransition < 1f)
                phaseTransition = Math.Min(1f, phaseTransition + 0.012f);

            flashIntensity *= 0.88f;
            if (flashIntensity < 0.01f) flashIntensity = 0f;

            UpdateConstellations();
            UpdateStarDust();
            UpdateNebulaClouds();
            UpdateAuroraTrails();
            if (currentPhase >= 3)
            {
                float speed = currentPhase >= 4 ? 0.004f : 0.0015f;
                galaxyRotation += speed;
            }
        }

        private void UpdateConstellations()
        {
            float targetForm = currentPhase >= 1 ? 1f : 0f;
            for (int i = 0; i < constellations.Count; i++)
            {
                var c = constellations[i];
                c.FormProgress += (targetForm - c.FormProgress) * 0.008f;
                constellations[i] = c;
            }
            if (currentPhase >= 2)
            {
                float orbitSpeed = currentPhase >= 3 ? 0.003f : 0.001f;
                constellationOrbitAngle += orbitSpeed;
            }
        }

        private void UpdateStarDust()
        {
            for (int i = starDustParticles.Count - 1; i >= 0; i--)
            {
                var p = starDustParticles[i];
                p.Position += p.Velocity;
                p.TimeAlive++;
                p.TwinklePhase += 0.08f;

                float lifeProgress = (float)p.TimeAlive / p.Lifetime;
                if (lifeProgress < 0.1f) p.Opacity = lifeProgress / 0.1f;
                else if (lifeProgress > 0.8f) p.Opacity = 1f - (lifeProgress - 0.8f) / 0.2f;
                else p.Opacity = 1f;

                if (p.Type == 0) p.Opacity *= 0.65f + (float)Math.Sin(p.TwinklePhase) * 0.35f;
                starDustParticles[i] = p;
                if (p.TimeAlive >= p.Lifetime || p.Opacity <= 0f)
                    starDustParticles.RemoveAt(i);
            }

            if (!isActive || intensity < 0.2f || starDustParticles.Count >= MaxStarDust) return;

            int spawnRate = currentPhase switch { 4 => 1, 3 => 2, 2 => 3, _ => 5 };
            if (Main.rand.NextBool(spawnRate)) SpawnStarDustParticle(0);
            if (Main.rand.NextBool(spawnRate + 2)) SpawnStarDustParticle(1);
            if (currentPhase >= 3 && Main.rand.NextBool(15)) SpawnStarDustParticle(2);
            if (currentPhase >= 4 && Main.rand.NextBool(4)) SpawnStarDustParticle(3);
        }

        private void SpawnStarDustParticle(int type)
        {
            float spawnX = Main.screenPosition.X + Main.rand.NextFloat(-80, Main.screenWidth + 80);
            float spawnY; Vector2 velocity; float scale; Color tint; int lifetime;

            switch (type)
            {
                case 0:
                    spawnY = Main.screenPosition.Y + Main.rand.NextFloat(-30, Main.screenHeight * 0.6f);
                    velocity = new Vector2(Main.rand.NextFloat(-0.05f, 0.05f), Main.rand.NextFloat(0.02f, 0.1f));
                    scale = Main.rand.NextFloat(0.3f, 1.0f);
                    tint = Color.Lerp(StarlightSilver, WhiteRadiance, Main.rand.NextFloat(0.5f));
                    lifetime = Main.rand.Next(250, 500);
                    break;
                case 1:
                    spawnY = Main.screenPosition.Y - 20f;
                    velocity = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(0.3f, 0.8f));
                    scale = Main.rand.NextFloat(0.5f, 1.5f);
                    tint = Color.Lerp(StarlightSilver, CosmicBlue, Main.rand.NextFloat(0.4f));
                    lifetime = Main.rand.Next(200, 400);
                    break;
                case 2:
                    spawnX = Main.screenPosition.X + Main.rand.NextFloat(-50, Main.screenWidth * 0.3f);
                    spawnY = Main.screenPosition.Y - 30f;
                    float cAngle = Main.rand.NextFloat(MathHelper.PiOver4 * 0.5f, MathHelper.PiOver4 * 1.5f);
                    float cSpeed = Main.rand.NextFloat(6f, 12f);
                    velocity = new Vector2((float)Math.Cos(cAngle), (float)Math.Sin(cAngle)) * cSpeed;
                    scale = Main.rand.NextFloat(1.5f, 3f);
                    tint = Color.Lerp(WhiteRadiance, StarlightSilver, Main.rand.NextFloat(0.3f));
                    lifetime = Main.rand.Next(40, 80);
                    break;
                default:
                    spawnY = Main.screenPosition.Y + Main.rand.NextFloat(0, Main.screenHeight);
                    velocity = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-0.3f, 0.3f));
                    scale = Main.rand.NextFloat(3f, 7f);
                    float hue = Main.rand.NextFloat();
                    tint = hue < 0.5f ? Color.Lerp(CosmicBlue, NebulaPurple, hue * 2f)
                                      : Color.Lerp(NebulaPurple, StarlightSilver, (hue - 0.5f) * 2f);
                    lifetime = Main.rand.Next(100, 250);
                    break;
            }

            starDustParticles.Add(new StarDust
            {
                Position = new Vector2(spawnX, spawnY), Velocity = velocity,
                Scale = scale, Opacity = 0f, Tint = tint,
                Lifetime = lifetime, TimeAlive = 0,
                TwinklePhase = Main.rand.NextFloat(MathHelper.TwoPi), Type = type
            });
        }

        private void UpdateNebulaClouds()
        {
            float targetOpacity = currentPhase >= 2 ? 1f : 0f;
            for (int i = 0; i < nebulaClouds.Count; i++)
            {
                var cloud = nebulaClouds[i];
                cloud.Opacity += (targetOpacity - cloud.Opacity) * 0.015f;
                cloud.Rotation += 0.0005f;
                cloud.PulsePhase += 0.02f;
                nebulaClouds[i] = cloud;
            }
        }

        private void UpdateAuroraTrails()
        {
            float fadeSpeed = currentPhase >= 4 ? 0.001f : 0.005f;
            for (int i = auroraTrails.Count - 1; i >= 0; i--)
            {
                var trail = auroraTrails[i];
                trail.Opacity -= fadeSpeed;
                trail.WaveOffset += 0.03f;
                auroraTrails[i] = trail;
                if (trail.Opacity <= 0f) auroraTrails.RemoveAt(i);
            }
        }

        // ═══════ DRAW ═══════

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (intensity <= 0f) return;
            if (maxDepth >= 0 && minDepth < 0)
            {
                Texture2D pixel = MagnumTextureRegistry.GetPixelTexture();
                if (pixel == null) return;

                DrawSkyGradient(spriteBatch, pixel);
                if (currentPhase >= 3) DrawGalaxySpiral(spriteBatch, pixel);
                if (currentPhase >= 2) DrawNebulaClouds(spriteBatch, pixel);
                DrawConstellations(spriteBatch, pixel);
                DrawStarDustParticles(spriteBatch, pixel);
                if (currentPhase >= 4 && auroraTrails.Count > 0) DrawAuroraTrails(spriteBatch, pixel);
                if (currentPhase >= 4) DrawSupernovaRadiance(spriteBatch, pixel);
                DrawVignette(spriteBatch, pixel);

                if (flashIntensity > 0f)
                {
                    Color flashOverlay = flashColor * flashIntensity * intensity * 0.6f;
                    spriteBatch.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), flashOverlay);
                }
            }
        }

        private void DrawSkyGradient(SpriteBatch spriteBatch, Texture2D pixel)
        {
            Color topBase, bottomBase;
            switch (currentPhase)
            {
                case 4:
                    float supernovaPulse = (float)Math.Sin(animationTimer * 0.03f) * 0.15f;
                    topBase = Color.Lerp(
                        DarkSky,
                        Color.Lerp(MidnightIndigo, StarlightSilver, 0.15f + supernovaPulse),
                        phaseTransition);
                    bottomBase = Color.Lerp(
                        MidnightIndigo,
                        Color.Lerp(DeepIndigo, WhiteRadiance, 0.1f + supernovaPulse * 0.5f),
                        phaseTransition);
                    break;
                case 3:
                    topBase = Color.Lerp(DarkSky, DeepIndigo, 0.3f * phaseTransition);
                    bottomBase = Color.Lerp(MidnightIndigo, NebulaPurple, 0.15f * phaseTransition);
                    break;
                case 2:
                    topBase = Color.Lerp(DarkSky, new Color(12, 10, 32), phaseTransition);
                    bottomBase = Color.Lerp(MidnightIndigo, DeepIndigo, 0.3f * phaseTransition);
                    break;
                default:
                    topBase = DarkSky;
                    bottomBase = MidnightIndigo;
                    break;
            }

            for (int y = 0; y < Main.screenHeight; y += 3)
            {
                float t = (float)y / Main.screenHeight;
                Color lineColor = Color.Lerp(topBase, bottomBase, t) * intensity;
                spriteBatch.Draw(pixel, new Rectangle(0, y, Main.screenWidth, 4), lineColor);
            }
        }

        private void DrawConstellations(SpriteBatch spriteBatch, Texture2D pixel)
        {
            float time = animationTimer * 0.04f;
            foreach (var constellation in constellations)
            {
                if (constellation.FormProgress < 0.01f) continue;
                float formAlpha = constellation.FormProgress * intensity;

                Vector2[] screenPositions = new Vector2[constellation.Stars.Length];
                for (int s = 0; s < constellation.Stars.Length; s++)
                {
                    Vector2 relPos = constellation.Stars[s].RelativePosition;
                    if (currentPhase >= 2)
                    {
                        Vector2 offset = relPos - constellation.Pivot;
                        float cos = (float)Math.Cos(constellationOrbitAngle * constellation.Depth);
                        float sin = (float)Math.Sin(constellationOrbitAngle * constellation.Depth);
                        offset = new Vector2(offset.X * cos - offset.Y * sin, offset.X * sin + offset.Y * cos);
                        relPos = constellation.Pivot + offset;
                    }
                    screenPositions[s] = new Vector2(relPos.X * Main.screenWidth, relPos.Y * Main.screenHeight);
                }

                // Connection lines
                for (int e = 0; e < constellation.ConnectionA.Length; e++)
                {
                    int a = constellation.ConnectionA[e];
                    int b = constellation.ConnectionB[e];
                    if (a >= screenPositions.Length || b >= screenPositions.Length) continue;
                    Vector2 from = screenPositions[a];
                    Vector2 diff = screenPositions[b] - from;
                    float length = diff.Length();
                    if (length < 1f) continue;
                    float angle = (float)Math.Atan2(diff.Y, diff.X);
                    Color lineColor = StarlightSilver * (formAlpha * 0.25f);
                    lineColor.A = 0;
                    spriteBatch.Draw(pixel, from, new Rectangle(0, 0, 1, 1), lineColor, angle, Vector2.Zero,
                        new Vector2(length, 1f), SpriteEffects.None, 0f);
                }

                // Star points
                for (int s = 0; s < constellation.Stars.Length; s++)
                {
                    var star = constellation.Stars[s];
                    Vector2 pos = screenPositions[s];
                    float twinkle = (float)Math.Sin(time + star.TwinkleOffset) * 0.3f + 0.7f;
                    float starAlpha = formAlpha * star.Brightness * twinkle;
                    Color starColor = StarlightSilver * starAlpha;
                    starColor.A = 0;

                    float coreSize = star.Scale * 2.5f;
                    spriteBatch.Draw(pixel, pos, new Rectangle(0, 0, 1, 1), starColor, 0f, new Vector2(0.5f),
                        coreSize, SpriteEffects.None, 0f);

                    if (star.Scale > 0.6f)
                    {
                        float flareLen = star.Scale * 5f * twinkle;
                        Color flareColor = starColor * 0.5f;
                        spriteBatch.Draw(pixel, new Rectangle((int)(pos.X - flareLen), (int)pos.Y, (int)(flareLen * 2), 1), flareColor);
                        spriteBatch.Draw(pixel, new Rectangle((int)pos.X, (int)(pos.Y - flareLen), 1, (int)(flareLen * 2)), flareColor);
                    }
                }
            }
        }

        private void DrawGalaxySpiral(SpriteBatch spriteBatch, Texture2D pixel)
        {
            Vector2 center = new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.45f);
            float maxRadius = Math.Min(Main.screenWidth, Main.screenHeight) * 0.45f;
            float blend = currentPhase >= 3 ? phaseTransition : 0f;

            for (int arm = 0; arm < 2; arm++)
            {
                float armOffset = arm * MathHelper.Pi;
                for (int i = 0; i < 120; i++)
                {
                    float t = i / 120f;
                    float spiralAngle = galaxyRotation + armOffset + t * MathHelper.TwoPi * 2.5f;
                    float radius = t * maxRadius;
                    float x = center.X + (float)Math.Cos(spiralAngle) * radius;
                    float y = center.Y + (float)Math.Sin(spiralAngle) * radius * 0.5f;
                    float armWidth = 8f + (1f - t) * 25f;
                    float brightness = (1f - t * 0.7f) * blend * intensity * 0.12f;
                    Color armColor = Color.Lerp(CosmicBlue, NebulaPurple, t) * brightness;
                    armColor.A = 0;
                    spriteBatch.Draw(pixel,
                        new Rectangle((int)(x - armWidth * 0.5f), (int)(y - armWidth * 0.25f), (int)armWidth, (int)(armWidth * 0.5f)),
                        armColor);
                }
            }

            float corePulse = 0.85f + 0.15f * (float)Math.Sin(animationTimer * 0.025f);
            for (int layer = 5; layer > 0; layer--)
            {
                float layerT = layer / 5f;
                float coreRadius = 40f * layerT * corePulse;
                Color coreColor = Color.Lerp(WhiteRadiance, CosmicBlue, layerT) * (blend * intensity * 0.08f * (1f - layerT));
                coreColor.A = 0;
                spriteBatch.Draw(pixel,
                    new Rectangle((int)(center.X - coreRadius), (int)(center.Y - coreRadius * 0.5f),
                        (int)(coreRadius * 2), (int)coreRadius),
                    coreColor);
            }
        }

        private void DrawNebulaClouds(SpriteBatch spriteBatch, Texture2D pixel)
        {
            foreach (var cloud in nebulaClouds)
            {
                if (cloud.Opacity < 0.01f) continue;
                Vector2 pos = new Vector2(cloud.Position.X * Main.screenWidth, cloud.Position.Y * Main.screenHeight);
                float pulse = 0.6f + 0.4f * (float)Math.Sin(cloud.PulsePhase);
                float alpha = cloud.Opacity * intensity * 0.1f * pulse;

                for (int layer = 8; layer > 0; layer--)
                {
                    float layerT = layer / 8f;
                    float radius = cloud.Scale * layerT;
                    Color layerColor = cloud.BaseColor * (alpha * (1f - layerT));
                    layerColor.A = 0;
                    spriteBatch.Draw(pixel,
                        new Rectangle((int)(pos.X - radius), (int)(pos.Y - radius * 0.6f), (int)(radius * 2), (int)(radius * 1.2f)),
                        layerColor);
                }

                Color starlightColor = StarlightSilver * (alpha * 2f * pulse);
                starlightColor.A = 0;
                spriteBatch.Draw(pixel, pos, new Rectangle(0, 0, 1, 1), starlightColor, 0f, new Vector2(0.5f),
                    3f * pulse, SpriteEffects.None, 0f);
            }
        }

        private void DrawStarDustParticles(SpriteBatch spriteBatch, Texture2D pixel)
        {
            foreach (var p in starDustParticles)
            {
                Vector2 drawPos = p.Position - Main.screenPosition;
                Color drawColor = p.Tint * p.Opacity * intensity;
                drawColor.A = 0;

                switch (p.Type)
                {
                    case 0: // Tiny twinkling star
                        float starSize = p.Scale * 2f;
                        spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), drawColor, 0f, new Vector2(0.5f),
                            starSize, SpriteEffects.None, 0f);
                        if (p.Scale > 0.6f)
                        {
                            float crossLen = p.Scale * 3f;
                            Color crossColor = drawColor * 0.4f;
                            spriteBatch.Draw(pixel, new Rectangle((int)(drawPos.X - crossLen), (int)drawPos.Y, (int)(crossLen * 2), 1), crossColor);
                            spriteBatch.Draw(pixel, new Rectangle((int)drawPos.X, (int)(drawPos.Y - crossLen), 1, (int)(crossLen * 2)), crossColor);
                        }
                        break;
                    case 1: // Starlight dust
                        float dustLen = p.Scale * 4f;
                        float dustAngle = (float)Math.Atan2(p.Velocity.Y, p.Velocity.X);
                        spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), drawColor * 0.8f, dustAngle, Vector2.Zero,
                            new Vector2(dustLen, 1f), SpriteEffects.None, 0f);
                        spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), drawColor, 0f, new Vector2(0.5f),
                            p.Scale * 1.5f, SpriteEffects.None, 0f);
                        break;
                    case 2: // Comet tail
                        Vector2 trailDir = p.Velocity.SafeNormalize(Vector2.UnitY);
                        spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), drawColor, 0f, new Vector2(0.5f),
                            p.Scale * 3f, SpriteEffects.None, 0f);
                        float trailLen = p.Scale * 35f;
                        for (int i = 0; i < 15; i++)
                        {
                            float t = i / 15f;
                            Vector2 point = drawPos - trailDir * trailLen * t;
                            Color pointColor = drawColor * (1f - t) * 0.7f;
                            spriteBatch.Draw(pixel, point, new Rectangle(0, 0, 1, 1), pointColor, 0f, new Vector2(0.5f),
                                Math.Max(1f, p.Scale * (1f - t * 0.6f)), SpriteEffects.None, 0f);
                        }
                        break;
                    case 3: // Aurora wisp
                        float wispSize = p.Scale * 15f;
                        for (int layer = 4; layer > 0; layer--)
                        {
                            float lt = layer / 4f;
                            Color layerColor = drawColor * (0.15f * (1f - lt));
                            spriteBatch.Draw(pixel,
                                new Rectangle((int)(drawPos.X - wispSize * lt), (int)(drawPos.Y - wispSize * lt * 0.3f),
                                    (int)(wispSize * lt * 2), (int)(wispSize * lt * 0.6f)),
                                layerColor);
                        }
                        break;
                }
            }
        }

        private void DrawAuroraTrails(SpriteBatch spriteBatch, Texture2D pixel)
        {
            foreach (var trail in auroraTrails)
            {
                Vector2 drawPos = trail.Position - Main.screenPosition;
                float alpha = trail.Opacity * intensity;
                Color trailColor = trail.Color * (alpha * 0.3f);
                trailColor.A = 0;

                for (int x = -60; x < 60; x += 3)
                {
                    float wave = (float)Math.Sin(trail.WaveOffset + x * 0.05f) * 15f;
                    float segAlpha = 1f - Math.Abs(x) / 60f;
                    spriteBatch.Draw(pixel,
                        new Rectangle((int)(drawPos.X + x - 1), (int)(drawPos.Y + wave - trail.Width * 0.15f),
                            4, (int)(trail.Width * 0.3f)),
                        trailColor * segAlpha);
                }
            }
        }

        private void DrawSupernovaRadiance(SpriteBatch spriteBatch, Texture2D pixel)
        {
            float pulse = 0.5f + 0.5f * (float)Math.Sin(animationTimer * 0.04f);
            float radianceAlpha = phaseTransition * intensity * 0.06f * pulse;
            Color radianceColor = WhiteRadiance * radianceAlpha;
            radianceColor.A = 0;
            spriteBatch.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), radianceColor);

            Vector2 center = new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.45f);
            for (int ring = 5; ring > 0; ring--)
            {
                float ringT = ring / 5f;
                float radius = 150f * ringT * (1f + pulse * 0.3f);
                Color ringColor = Color.Lerp(WhiteRadiance, StarlightSilver, ringT)
                    * (phaseTransition * intensity * 0.04f * (1f - ringT));
                ringColor.A = 0;
                spriteBatch.Draw(pixel,
                    new Rectangle((int)(center.X - radius), (int)(center.Y - radius), (int)(radius * 2), (int)(radius * 2)),
                    ringColor);
            }
        }

        private void DrawVignette(SpriteBatch spriteBatch, Texture2D pixel)
        {
            float vignetteStrength = currentPhase switch { 4 => 0.15f, 3 => 0.35f, 2 => 0.25f, _ => 0.18f };
            Color vignetteColor = currentPhase >= 4 ? NebulaPurple : DeepIndigo;

            for (int ring = 0; ring < 10; ring++)
            {
                float t = ring / 10f;
                float ringAlpha = t * t * vignetteStrength * intensity;
                int inset = (int)((1f - t) * Math.Min(Main.screenWidth, Main.screenHeight) * 0.45f);
                var rect = new Rectangle(inset, inset, Main.screenWidth - inset * 2, Main.screenHeight - inset * 2);
                if (rect.Width > 0 && rect.Height > 0)
                    spriteBatch.Draw(pixel, rect, vignetteColor * ringAlpha);
            }
        }

        // ═══════ LIFECYCLE ═══════

        public override void Activate(Vector2 position, params object[] args)
        {
            isActive = true;
        }

        public override void Deactivate(params object[] args)
        {
            isActive = false;
            currentPhase = 1;
            phaseTransition = 0f;
        }

        public override void Reset()
        {
            isActive = false;
            intensity = 0f;
            currentPhase = 1;
            phaseTransition = 0f;
            starDustParticles.Clear();
            nebulaClouds.Clear();
            auroraTrails.Clear();
            constellationsInitialized = false;
            galaxyRotation = 0f;
            constellationOrbitAngle = 0f;
        }

        public override bool IsActive()
        {
            return isActive || intensity > 0f;
        }
    }

    /// <summary>
    /// Registers the Nachtmusik celestial sky effect with tModLoader
    /// </summary>
    public class NachtmusikCelestialSkyLoader : ModSystem
    {
        public override void Load()
        {
            if (!Main.dedServ)
            {
                SkyManager.Instance["MagnumOpus:NachtmusikCelestialSky"] = new NachtmusikCelestialSky();
            }
        }

        public override void Unload()
        {
            if (!Main.dedServ)
            {
                // SkyManager handles cleanup
            }
        }
    }
}
