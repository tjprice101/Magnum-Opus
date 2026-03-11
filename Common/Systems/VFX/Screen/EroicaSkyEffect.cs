using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using MagnumOpus.Content.Eroica.Bosses;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Calamity-tier custom sky for the Eroica boss fight.
    /// Implements: SCal-style cinder particles with depth/wave motion,
    /// Yharon-style world lighting tint, boss-health-driven intensity scaling,
    /// distance-based falloff, phase-specific color shifts, and attack flash system.
    /// </summary>
    public class EroicaSkyEffect : CustomSky
    {
        // --- Cinder particle (SCal pattern) ---
        private class Cinder
        {
            public int Time;
            public int Lifetime;
            public int Identity;
            public float Scale;
            public float Depth;
            public float Opacity;
            public Color DrawColor;
            public Vector2 Center;
            public Vector2 Velocity;
            public int Type; // 0=ember, 1=sakura, 2=sparkle

            public Cinder(int lifetime, int identity, float depth, Color color, Vector2 position, Vector2 velocity, int type)
            {
                Lifetime = lifetime;
                Identity = identity;
                Depth = depth;
                DrawColor = color;
                Center = position;
                Velocity = velocity;
                Type = type;
            }
        }

        // --- State ---
        private bool isActive;
        private float intensity;
        private int eroicaIndex = -1;
        private readonly List<Cinder> cinders = new();

        // Boss-driven state (updated by EroicaSkySystem)
        internal float BossLifeRatio = 1f;
        internal int BossPhaseIndex; // 0=phase1, 1=phase2A, 2=phase2B, 3=phase2C
        internal bool BossEnraged;

        // Flash system
        private float flashIntensity;
        private Color flashColor = Color.White;

        // --- Theme palette ---
        private static readonly Color SkyBlack = new(8, 2, 4);
        private static readonly Color SkyScarlet = new(120, 25, 30);
        private static readonly Color SkyCrimson = new(160, 20, 15);
        private static readonly Color EmberGold = new(255, 200, 50);
        private static readonly Color EmberOrange = new(255, 140, 30);
        private static readonly Color SakuraPink = new(255, 150, 180);
        private static readonly Color ValorWhite = new(255, 240, 220);

        // --- Configuration ---
        private const int MaxCinders = 200;

        #region Cinder Spawning & Speed (SCal-inspired health-driven scaling)

        private int CinderReleaseChance
        {
            get
            {
                if (BossLifeRatio <= 0.15f) return 2;  // Overwhelming at death
                if (BossLifeRatio <= 0.4f) return 4;   // Intense at low HP
                if (BossLifeRatio <= 0.7f) return 7;   // Moderate mid-fight
                return 12;                               // Subtle at full HP
            }
        }

        private float CinderSpeed
        {
            get
            {
                if (BossLifeRatio <= 0.15f) return 8f;
                if (BossLifeRatio <= 0.4f) return 5.5f;
                if (BossLifeRatio <= 0.7f) return 3.8f;
                return 2.5f;
            }
        }

        private Color SelectCinderColor()
        {
            // Phase-driven color shifts
            if (BossEnraged)
                return Color.Lerp(SkyCrimson, new Color(255, 60, 20), Main.rand.NextFloat());

            return BossPhaseIndex switch
            {
                >= 3 => Color.Lerp(SkyCrimson, EmberOrange, Main.rand.NextFloat()),    // Phase 2C: crimson/orange
                2 => Color.Lerp(new Color(200, 50, 50), EmberGold, Main.rand.NextFloat()), // Phase 2B: scarlet/gold
                1 => Color.Lerp(EmberGold, EmberOrange, Main.rand.NextFloat()),         // Phase 2A: gold/orange
                _ => Color.Lerp(EmberGold, ValorWhite, Main.rand.NextFloat() * 0.4f)    // Phase 1: warm gold
            };
        }

        #endregion

        #region Core Sky Methods

        public override void Update(GameTime gameTime)
        {
            if (isActive && intensity < 1f)
                intensity += 0.008f;
            else if (!isActive && intensity > 0f)
                intensity -= 0.015f;

            intensity = MathHelper.Clamp(intensity, 0f, 1f);

            // Decay flash
            flashIntensity *= 0.88f;
            if (flashIntensity < 0.005f) flashIntensity = 0f;

            // Spawn cinders (SCal pattern)
            if (isActive && intensity > 0.2f && cinders.Count < MaxCinders)
            {
                if (Main.rand.NextBool(Math.Max(1, CinderReleaseChance)))
                    SpawnCinder();

                // Sakura petals at lower HP
                if (BossLifeRatio < 0.7f && Main.rand.NextBool(Math.Max(2, CinderReleaseChance + 4)))
                    SpawnSakuraCinder();
            }

            // Update cinders (SCal wave-motion pattern)
            float speed = CinderSpeed;
            for (int i = 0; i < cinders.Count; i++)
            {
                var c = cinders[i];
                c.Time++;

                // Opacity curve: fade in → sustain → fade out
                if (c.Opacity < 1f && c.Time < c.Lifetime / 3)
                    c.Opacity = Math.Min(c.Opacity + 0.06f, 1f);
                c.Scale = Utils.GetLerpValue(c.Lifetime, c.Lifetime / 3, c.Time, true) * 1.5f;
                c.Scale *= MathHelper.Lerp(0.6f, 1.1f, c.Identity % 7f / 7f);

                // Wave motion (SCal-style sinusoidal drift)
                Vector2 idealVelocity = -Vector2.UnitY.RotatedBy(
                    MathHelper.Lerp(-0.7f, 0.7f, (float)Math.Sin(c.Time / 32f + c.Identity) * 0.5f + 0.5f)) * speed;
                float interpolant = MathHelper.Lerp(0.01f, 0.06f, Utils.GetLerpValue(30f, 120f, c.Time, true));
                c.Velocity = Vector2.Lerp(c.Velocity, idealVelocity, interpolant);
                if (c.Velocity.Length() > 0.01f)
                    c.Velocity = c.Velocity.SafeNormalize(-Vector2.UnitY) * speed;

                c.Center += c.Velocity;
            }

            // Remove dead cinders
            cinders.RemoveAll(c => c.Time >= c.Lifetime);
        }

        private void SpawnCinder()
        {
            int lifetime = Main.rand.Next(360, 600);
            float depth = Main.rand.NextFloat(1.5f, 5f);
            Vector2 spawnPos = Main.screenPosition + new Vector2(
                Main.screenWidth * Main.rand.NextFloat(-0.1f, 1.1f),
                Main.screenHeight * Main.rand.NextFloat(0.9f, 1.3f));
            Vector2 velocity = -Vector2.UnitY.RotatedByRandom(0.8f) * Main.rand.NextFloat(0.5f, 1.5f);
            cinders.Add(new Cinder(lifetime, cinders.Count, depth, SelectCinderColor(), spawnPos, velocity, 0));
        }

        private void SpawnSakuraCinder()
        {
            int lifetime = Main.rand.Next(300, 550);
            float depth = Main.rand.NextFloat(2f, 6f);
            Vector2 spawnPos = Main.screenPosition + new Vector2(
                Main.screenWidth * Main.rand.NextFloat(-0.15f, 1.15f),
                Main.rand.NextBool() ? -Main.rand.NextFloat(20, 80) : Main.screenHeight * Main.rand.NextFloat(0.85f, 1.2f));
            Vector2 velocity = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(-1.5f, 0.5f));
            Color pink = Color.Lerp(SakuraPink, new Color(255, 200, 210), Main.rand.NextFloat());
            cinders.Add(new Cinder(lifetime, cinders.Count, depth, pink, spawnPos, velocity, 1));
        }

        /// <summary>
        /// Yharon-style world lighting tint. Shifts all tile colors toward a warm heroic tone.
        /// Intensity scales with distance from boss and boss HP.
        /// </summary>
        public override Color OnTileColor(Color inColor)
        {
            float eff = GetEffectiveIntensity();
            if (eff <= 0f) return inColor;

            // Warm golden-scarlet tint that deepens as boss HP drops
            Vector4 tint = BossLifeRatio > 0.4f
                ? new Vector4(1f, 0.85f, 0.6f, 1f)     // Warm gold tint (high HP)
                : new Vector4(0.9f, 0.55f, 0.4f, 1f);  // Scarlet tint (low HP)

            return new Color(Vector4.Lerp(inColor.ToVector4(), tint * inColor.ToVector4(), eff * 0.5f));
        }

        private float GetEffectiveIntensity()
        {
            // Distance-based falloff (Yharon pattern)
            if (UpdateEroicaIndex() && eroicaIndex >= 0)
            {
                float dist = Vector2.Distance(Main.LocalPlayer.Center, Main.npc[eroicaIndex].Center);
                return (1f - Utils.SmoothStep(2500f, 5500f, dist)) * intensity;
            }
            return intensity * 0.4f;
        }

        private bool UpdateEroicaIndex()
        {
            int eroicaType = ModContent.NPCType<EroicasRetribution>();
            if (eroicaIndex >= 0 && eroicaIndex < Main.maxNPCs &&
                Main.npc[eroicaIndex].active && Main.npc[eroicaIndex].type == eroicaType)
                return true;

            eroicaIndex = NPC.FindFirstNPC(eroicaType);
            return eroicaIndex != -1;
        }

        /// <summary>Triggers a screen flash. Call from boss attacks for dramatic punctuation.</summary>
        public void TriggerFlash(float strength, Color? color = null)
        {
            flashIntensity = Math.Max(flashIntensity, Math.Min(strength, 1f));
            flashColor = color ?? ValorWhite;
        }

        #endregion

        #region Drawing

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth >= 0 && minDepth < 0)
            {
                float eff = GetEffectiveIntensity();
                if (eff <= 0.01f) return;

                Texture2D pixel = MagnumTextureRegistry.GetPixelTexture();
                if (pixel == null) return;

                // --- Background gradient (phase-color-driven) ---
                DrawSkyGradient(spriteBatch, pixel, eff);

                // --- Cinder particles ---
                DrawCinders(spriteBatch, pixel, eff);

                // --- Vignette: scarlet-tinted cinematic edges ---
                DrawVignette(spriteBatch, pixel, eff);

                // --- Attack flash overlay ---
                if (flashIntensity > 0.01f)
                    DrawFlashOverlay(spriteBatch, pixel, eff);
            }
        }

        private void DrawSkyGradient(SpriteBatch spriteBatch, Texture2D pixel, float eff)
        {
            // Breathing pulse tied to boss HP (faster pulse at lower HP)
            float pulseSpeed = MathHelper.Lerp(0.015f, 0.04f, 1f - BossLifeRatio);
            float pulse = (float)Math.Sin(Main.GlobalTimeWrappedHourly * pulseSpeed * 60f) * 0.08f;

            // Phase-driven sky color: gold → scarlet → crimson
            Color skyTop = BossPhaseIndex switch
            {
                >= 3 => Color.Lerp(SkyCrimson, new Color(180, 30, 10), pulse + 0.5f),
                2 => Color.Lerp(SkyScarlet, SkyCrimson, pulse + 0.3f),
                _ => Color.Lerp(SkyScarlet, new Color(140, 40, 25), pulse)
            };

            // Draw in 8px bands (2x fewer draw calls than 4px)
            for (int y = 0; y < Main.screenHeight; y += 8)
            {
                float t = (float)y / Main.screenHeight;
                // Bottom = dark, top = colored (embers rise)
                Color band = Color.Lerp(SkyBlack, skyTop, (1f - t) * (1f - t));
                // Slight horizontal wave for visual depth
                float wave = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 0.6f + t * 4f) * 0.03f;
                band = Color.Lerp(band, skyTop, wave);

                spriteBatch.Draw(pixel, new Rectangle(0, y, Main.screenWidth, 8), band * eff);
            }
        }

        private void DrawCinders(SpriteBatch spriteBatch, Texture2D pixel, float eff)
        {
            Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
            bool hasGlow = glowTex != null;
            Vector2 glowOrigin = hasGlow ? new Vector2(glowTex.Width * 0.5f, glowTex.Height * 0.5f) : Vector2.Zero;

            foreach (var c in cinders)
            {
                Vector2 drawPos = c.Center - Main.screenPosition;
                // Depth-based parallax offset (SCal pattern)
                drawPos += (Main.screenPosition - Main.LocalPlayer.Center) * (1f / c.Depth);

                if (drawPos.X < -80 || drawPos.X > Main.screenWidth + 80 ||
                    drawPos.Y < -80 || drawPos.Y > Main.screenHeight + 80)
                    continue;

                float alpha = c.Opacity * eff;
                Color col = c.DrawColor * alpha;
                col.A = 0; // Additive rendering (black = invisible)

                float size = c.Scale * MathHelper.Lerp(0.6f, 1.2f, 1f / c.Depth);

                if (c.Type == 1) // Sakura petal: gentle rotation + transparency
                {
                    float rot = c.Time * 0.03f + c.Identity * 0.5f;
                    if (hasGlow)
                    {
                        float petalScale = size * 0.04f;
                        spriteBatch.Draw(glowTex, drawPos, null, col * 0.7f, rot, glowOrigin, petalScale, SpriteEffects.None, 0f);
                        spriteBatch.Draw(glowTex, drawPos, null, col * 0.3f, rot, glowOrigin, petalScale * 1.8f, SpriteEffects.None, 0f);
                    }
                    else
                    {
                        int s = Math.Max(2, (int)(size * 3));
                        spriteBatch.Draw(pixel, new Rectangle((int)(drawPos.X - s / 2), (int)(drawPos.Y - s / 2), s, s), col * 0.6f);
                    }
                }
                else // Ember or sparkle: multi-layer soft glow
                {
                    if (hasGlow)
                    {
                        float baseScale = size * 0.035f;
                        // Core (bright, tight)
                        spriteBatch.Draw(glowTex, drawPos, null, col, 0f, glowOrigin, baseScale * 0.4f, SpriteEffects.None, 0f);
                        // Mid glow
                        spriteBatch.Draw(glowTex, drawPos, null, col * 0.5f, 0f, glowOrigin, baseScale, SpriteEffects.None, 0f);
                        // Outer haze
                        spriteBatch.Draw(glowTex, drawPos, null, col * 0.2f, 0f, glowOrigin, baseScale * 2f, SpriteEffects.None, 0f);
                    }
                    else
                    {
                        int s = Math.Max(2, (int)(size * 4));
                        for (int layer = 2; layer >= 0; layer--)
                        {
                            int ls = s * (1 + layer);
                            float lo = 0.3f / (layer + 1);
                            spriteBatch.Draw(pixel, new Rectangle((int)(drawPos.X - ls / 2), (int)(drawPos.Y - ls / 2), ls, ls), col * lo);
                        }
                    }
                }
            }
        }

        private void DrawVignette(SpriteBatch spriteBatch, Texture2D pixel, float eff)
        {
            // Scarlet-tinted vignette that deepens at low HP
            float vignetteStrength = MathHelper.Lerp(0.3f, 0.65f, 1f - BossLifeRatio) * eff;
            Color vignetteBase = BossLifeRatio < 0.4f
                ? Color.Lerp(Color.Black, new Color(40, 5, 0), 0.3f)
                : Color.Black;

            int size = (int)(Main.screenHeight * 0.2f);
            for (int i = 0; i < size; i += 2)
            {
                float t = 1f - (float)i / size;
                float opacity = t * t * vignetteStrength; // Quadratic falloff
                Color c = vignetteBase * opacity;

                // All four edges in 2px bands
                spriteBatch.Draw(pixel, new Rectangle(0, i, Main.screenWidth, 2), c);
                spriteBatch.Draw(pixel, new Rectangle(0, Main.screenHeight - i - 2, Main.screenWidth, 2), c);
                spriteBatch.Draw(pixel, new Rectangle(i, 0, 2, Main.screenHeight), c * 0.7f);
                spriteBatch.Draw(pixel, new Rectangle(Main.screenWidth - i - 2, 0, 2, Main.screenHeight), c * 0.7f);
            }
        }

        private void DrawFlashOverlay(SpriteBatch spriteBatch, Texture2D pixel, float eff)
        {
            Color c = flashColor * (flashIntensity * eff * 0.6f);
            c.A = 0;
            spriteBatch.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), c);
        }

        #endregion

        #region Lifecycle

        public override float GetCloudAlpha() => 1f - intensity * 0.95f;

        public override void Activate(Vector2 position, params object[] args)
        {
            isActive = true;
            cinders.Clear();
        }

        public override void Deactivate(params object[] args) => isActive = false;

        public override void Reset()
        {
            isActive = false;
            intensity = 0f;
            flashIntensity = 0f;
            BossLifeRatio = 1f;
            BossPhaseIndex = 0;
            BossEnraged = false;
            cinders.Clear();
        }

        public override bool IsActive() => isActive || intensity > 0f;

        #endregion
    }

    /// <summary>
    /// ModSystem that drives the Eroica sky: finds the boss, feeds it health/phase data,
    /// and spawns world-space dust + custom particles scaled to fight intensity.
    /// </summary>
    public class EroicaSkySystem : ModSystem
    {
        private static bool skyRegistered;
        private static bool lastEroicaActive;
        private static EroicaSkyEffect skyInstance;
        private static int flashCooldown;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                skyInstance = new EroicaSkyEffect();
                SkyManager.Instance["MagnumOpus:EroicaSky"] = skyInstance;
                skyRegistered = true;
            }
        }

        public override void Unload()
        {
            skyRegistered = false;
            skyInstance = null;
        }

        public override void PostUpdateWorld()
        {
            if (Main.dedServ || !skyRegistered) return;

            if (flashCooldown > 0) flashCooldown--;

            bool active = FindEroica(out NPC boss);

            // Feed boss state into sky
            if (boss != null && skyInstance != null)
            {
                skyInstance.BossLifeRatio = (float)boss.life / boss.lifeMax;

                float lifeRatio = skyInstance.BossLifeRatio;
                if (lifeRatio > 0.7f) skyInstance.BossPhaseIndex = 1;
                else if (lifeRatio > 0.4f) skyInstance.BossPhaseIndex = 2;
                else skyInstance.BossPhaseIndex = 3;

                // Detect enrage from AI state (ai[0] == 5 is Enraged in EroicasRetribution)
                skyInstance.BossEnraged = boss.ai[0] == 5f;
            }

            // Activate/deactivate
            if (active && !lastEroicaActive)
                SkyManager.Instance.Activate("MagnumOpus:EroicaSky");
            else if (!active && lastEroicaActive)
            {
                SkyManager.Instance.Deactivate("MagnumOpus:EroicaSky");
                if (skyInstance != null)
                {
                    skyInstance.BossLifeRatio = 1f;
                    skyInstance.BossPhaseIndex = 0;
                    skyInstance.BossEnraged = false;
                }
            }

            lastEroicaActive = active;

            // World-space dust and particles
            if (active && boss != null)
                SpawnWorldDust(boss);
        }

        /// <summary>
        /// Triggers a screen flash on the sky overlay.
        /// Call from attack VFX for dramatic punctuation on big hits.
        /// </summary>
        public static void TriggerAttackFlash(float strength = 0.8f, Color? color = null)
        {
            if (flashCooldown <= 0 && skyInstance != null)
            {
                skyInstance.TriggerFlash(strength, color);
                flashCooldown = 8;
            }
        }

        /// <summary>Triggers a golden flash specifically for attack release moments.</summary>
        public static void TriggerGoldenFlash(float strength = 0.6f)
            => TriggerAttackFlash(strength, new Color(255, 220, 140));

        /// <summary>Triggers a scarlet flash for high-damage impacts.</summary>
        public static void TriggerScarletFlash(float strength = 0.5f)
            => TriggerAttackFlash(strength, new Color(200, 50, 50));

        private static bool FindEroica(out NPC boss)
        {
            boss = null;
            int type = ModContent.NPCType<EroicasRetribution>();
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].type == type)
                {
                    boss = Main.npc[i];
                    return true;
                }
            }
            return false;
        }

        private void SpawnWorldDust(NPC boss)
        {
            Player player = Main.LocalPlayer;
            float lifeRatio = (float)boss.life / boss.lifeMax;

            // Golden ember dust — spawn rate scales with missing HP
            int emberChance = (int)MathHelper.Lerp(1, 4, lifeRatio);
            if (Main.rand.NextBool(Math.Max(1, emberChance)))
            {
                Vector2 pos = player.Center + new Vector2(Main.rand.NextFloat(-1400, 1400), Main.rand.NextFloat(-500, 400));
                Dust d = Dust.NewDustDirect(pos, 1, 1, DustID.GoldFlame, 0f, 0f, 100, default, Main.rand.NextFloat(1.5f, 2.5f));
                d.noGravity = true;
                d.velocity = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), Main.rand.NextFloat(-2f, -0.5f));
                d.fadeIn = 1.5f;
            }

            // Scarlet dust — increases at lower HP
            int scarletChance = lifeRatio < 0.4f ? 2 : 6;
            if (Main.rand.NextBool(scarletChance))
            {
                Vector2 pos = player.Center + new Vector2(Main.rand.NextFloat(-1200, 1200), Main.rand.NextFloat(-400, 300));
                Dust d = Dust.NewDustDirect(pos, 1, 1, DustID.CrimsonTorch, 0f, 0f, 120, default, Main.rand.NextFloat(1.2f, 2f));
                d.noGravity = true;
                d.velocity = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(-1.8f, -0.3f));
            }

            // Custom bloom particles via MagnumParticleHandler
            if (Main.rand.NextBool(Math.Max(2, (int)(10 * lifeRatio))))
            {
                Vector2 pos = player.Center + new Vector2(Main.rand.NextFloat(-900, 900), Main.rand.NextFloat(-600, 200));
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), Main.rand.NextFloat(-1.2f, -0.2f));
                Color col = Color.Lerp(new Color(255, 200, 80), new Color(255, 140, 30), Main.rand.NextFloat());
                var bloom = new Particles.BloomParticle(pos, vel, col, Main.rand.NextFloat(0.3f, 0.8f), Main.rand.Next(80, 160));
                Particles.MagnumParticleHandler.SpawnParticle(bloom);
            }

            // Sparkle particles (less frequent)
            if (Main.rand.NextBool(14))
            {
                Vector2 pos = player.Center + new Vector2(Main.rand.NextFloat(-1000, 1000), Main.rand.NextFloat(-700, 100));
                Color col = Color.Lerp(Color.Gold, Color.White, Main.rand.NextFloat(0.3f, 0.7f));
                var sparkle = new Particles.SparkleParticle(pos, Main.rand.NextVector2Circular(0.3f, 0.3f), col, Main.rand.NextFloat(0.5f, 1.2f), Main.rand.Next(50, 110));
                Particles.MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Enraged: heavy crimson glow particles
            if (boss.ai[0] == 5f && Main.rand.NextBool(2))
            {
                Vector2 pos = player.Center + new Vector2(Main.rand.NextFloat(-500, 500), Main.rand.NextFloat(-350, 200));
                Color col = Color.Lerp(new Color(200, 50, 50), new Color(255, 80, 40), Main.rand.NextFloat());
                var glow = new Particles.GlowSparkParticle(pos, new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1.5f, 0.5f)), col, Main.rand.NextFloat(0.4f, 1f), Main.rand.Next(35, 70));
                Particles.MagnumParticleHandler.SpawnParticle(glow);
            }
        }
    }
}
