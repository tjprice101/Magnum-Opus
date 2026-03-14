using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;

namespace MagnumOpus.Content.Summer.Bosses.Systems
{
    /// <summary>
    /// 4-phase sky for L'Estate (Vivaldi's Summer):
    ///   Phase 1 - Scorching Stillness: blazing gold sky, gentle ember drift
    ///   Phase 2 - Gathering Storm: sky darkens to deep amber, wind picks up
    ///   Phase 3 - Full Tempest: torrential solar rain, lightning flashes
    ///   Phase 4 - Solar Eclipse (enrage): near-darkness, boss is the light
    /// </summary>
    public class LEstateSky : CustomSky
    {
        private bool _isActive;
        private float _opacity;

        // Boss state fed from LEstate.cs
        public static float BossLifeRatio = 1f;
        public static Vector2 BossCenter = Vector2.Zero;
        public static bool BossIsEnraged = false;
        public static int CurrentPhase = 1; // 1-4

        // Phase transition interpolation
        private float _phaseBlend;
        private int _displayPhase = 1;

        // Ember motes (Phase 1-2)
        private struct EmberMote
        {
            public Vector2 Position;
            public float Speed;
            public float Phase;
            public float Scale;
            public Color Color;
        }
        private EmberMote[] _embers;
        private const int MaxEmbers = 60;

        // Solar rain (Phase 3)
        private struct SolarRain
        {
            public Vector2 Position;
            public float Speed;
            public float Length;
            public Color Color;
        }
        private SolarRain[] _rain;
        private const int MaxRain = 80;

        // Lightning flash state
        private float _lightningFlash;
        private float _lightningDecay;
        private int _nextLightningTimer;

        // Sky flash API
        private static float _flashIntensity;
        private static float _flashDecay;
        private static Color _flashColor;

        public override void OnLoad() { }

        public override void Activate(Vector2 position, params object[] args)
        {
            _isActive = true;
            Random rand = new Random();

            _embers = new EmberMote[MaxEmbers];
            for (int i = 0; i < MaxEmbers; i++)
            {
                _embers[i] = new EmberMote
                {
                    Position = new Vector2(
                        (float)rand.NextDouble() * Main.screenWidth,
                        (float)rand.NextDouble() * Main.screenHeight),
                    Speed = 0.3f + (float)rand.NextDouble() * 1.2f,
                    Phase = (float)rand.NextDouble() * MathHelper.TwoPi,
                    Scale = 0.3f + (float)rand.NextDouble() * 0.8f,
                    Color = Color.Lerp(new Color(255, 200, 50), new Color(255, 140, 40),
                        (float)rand.NextDouble())
                };
            }

            _rain = new SolarRain[MaxRain];
            for (int i = 0; i < MaxRain; i++)
            {
                _rain[i] = new SolarRain
                {
                    Position = new Vector2(
                        (float)rand.NextDouble() * Main.screenWidth,
                        (float)rand.NextDouble() * Main.screenHeight),
                    Speed = 8f + (float)rand.NextDouble() * 12f,
                    Length = 15f + (float)rand.NextDouble() * 25f,
                    Color = Color.Lerp(new Color(255, 200, 50), new Color(255, 250, 240),
                        (float)rand.NextDouble() * 0.5f)
                };
            }

            _nextLightningTimer = 120 + rand.Next(180);
        }

        public override void Deactivate(params object[] args) => _isActive = false;

        public override void Reset()
        {
            _isActive = false;
            _opacity = 0f;
            _displayPhase = 1;
            _phaseBlend = 0f;
        }

        public override bool IsActive() => _isActive || _opacity > 0.001f;

        public override void Update(GameTime gameTime)
        {
            _opacity = _isActive
                ? Math.Min(1f, _opacity + 0.02f)
                : Math.Max(0f, _opacity - 0.02f);

            // Smooth phase transitions
            if (_displayPhase != CurrentPhase)
            {
                _phaseBlend += 0.015f;
                if (_phaseBlend >= 1f)
                {
                    _displayPhase = CurrentPhase;
                    _phaseBlend = 0f;
                }
            }

            float time = (float)Main.timeForVisualEffects;
            float windStrength = _displayPhase >= 2 ? 1f + (_displayPhase - 2) * 1.5f : 0.3f;

            // Update embers (phases 1-3, fade out in 4)
            if (_embers != null)
            {
                for (int i = 0; i < MaxEmbers; i++)
                {
                    _embers[i].Position.Y -= _embers[i].Speed;
                    _embers[i].Position.X += (float)Math.Sin(time * 0.01f + _embers[i].Phase) * windStrength;

                    if (_embers[i].Position.Y < -20)
                    {
                        _embers[i].Position.Y = Main.screenHeight + 20;
                        _embers[i].Position.X = Main.rand.NextFloat() * Main.screenWidth;
                    }
                }
            }

            // Update solar rain (phase 3+)
            if (_rain != null && _displayPhase >= 3)
            {
                for (int i = 0; i < MaxRain; i++)
                {
                    _rain[i].Position.Y += _rain[i].Speed;
                    _rain[i].Position.X -= windStrength * 2f;

                    if (_rain[i].Position.Y > Main.screenHeight + 40)
                    {
                        _rain[i].Position.Y = -40f;
                        _rain[i].Position.X = Main.rand.NextFloat() * (Main.screenWidth + 200);
                    }
                }
            }

            // Lightning flashes (phase 3)
            if (_displayPhase >= 3 && !BossIsEnraged)
            {
                _nextLightningTimer--;
                if (_nextLightningTimer <= 0)
                {
                    _lightningFlash = 0.4f + Main.rand.NextFloat() * 0.4f;
                    _lightningDecay = 0.88f;
                    _nextLightningTimer = 40 + Main.rand.Next(120);
                }
            }
            _lightningFlash *= _lightningDecay;
            if (_lightningFlash < 0.01f) _lightningFlash = 0f;

            // Sky flash decay
            if (_flashIntensity > 0.01f)
                _flashIntensity *= _flashDecay;
            else
                _flashIntensity = 0f;
        }

        public override Color OnTileColor(Color inColor)
        {
            float eff = _opacity;
            if (_displayPhase <= 2)
            {
                Color tint = new Color(255, 230, 180);
                return Color.Lerp(inColor, tint, eff * 0.3f);
            }
            else if (BossIsEnraged)
            {
                // Eclipse: darken everything except near boss
                Color dark = new Color(30, 15, 5);
                return Color.Lerp(inColor, dark, eff * 0.6f);
            }
            else
            {
                Color stormTint = new Color(150, 90, 30);
                return Color.Lerp(inColor, stormTint, eff * 0.4f);
            }
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth < 0f || minDepth >= 0f) return;

            float eff = _opacity;
            if (eff < 0.01f) return;

            Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;

            if (BossIsEnraged)
                DrawPhase4Eclipse(spriteBatch, pixel, eff);
            else if (_displayPhase >= 3)
                DrawPhase3Tempest(spriteBatch, pixel, eff);
            else if (_displayPhase == 2)
                DrawPhase2Storm(spriteBatch, pixel, eff);
            else
                DrawPhase1Stillness(spriteBatch, pixel, eff);

            // Lightning flash overlay
            if (_lightningFlash > 0.01f)
            {
                Color flashC = new Color(255, 250, 230) * (_lightningFlash * eff);
                flashC.A = 0;
                spriteBatch.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), flashC);
            }

            // Sky flash API overlay
            if (_flashIntensity > 0.01f)
            {
                Color fc = _flashColor * (_flashIntensity * eff * 0.3f);
                fc.A = 0;
                spriteBatch.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), fc);
            }

            // Vignette (all phases, intensity varies)
            DrawVignette(spriteBatch, pixel, eff);
        }

        private void DrawPhase1Stillness(SpriteBatch sb, Texture2D pixel, float eff)
        {
            // Blazing gold gradient sky
            Color topColor = new Color(120, 80, 20) * eff * 0.7f;
            Color bottomColor = new Color(200, 140, 40) * eff * 0.7f;
            DrawGradientSky(sb, pixel, topColor, bottomColor);

            // Gentle ember drift
            DrawEmbers(sb, pixel, eff, 0.4f);

            // Solar glare at top-center
            DrawSolarGlare(sb, pixel, eff, 0.2f);
        }

        private void DrawPhase2Storm(SpriteBatch sb, Texture2D pixel, float eff)
        {
            // Darkening amber sky
            Color topColor = new Color(80, 50, 15) * eff * 0.8f;
            Color bottomColor = new Color(160, 100, 25) * eff * 0.8f;
            DrawGradientSky(sb, pixel, topColor, bottomColor);

            // Faster embers with wind
            DrawEmbers(sb, pixel, eff, 0.6f);

            // Dimmer solar glare
            DrawSolarGlare(sb, pixel, eff, 0.12f);
        }

        private void DrawPhase3Tempest(SpriteBatch sb, Texture2D pixel, float eff)
        {
            // Dark storm sky
            Color topColor = new Color(40, 25, 8) * eff * 0.85f;
            Color bottomColor = new Color(100, 60, 15) * eff * 0.85f;
            DrawGradientSky(sb, pixel, topColor, bottomColor);

            // Solar rain streaks
            if (_rain != null)
            {
                for (int i = 0; i < MaxRain; i++)
                {
                    Color rc = _rain[i].Color * eff * 0.5f;
                    rc.A = 0;
                    Vector2 start = _rain[i].Position;
                    Vector2 end = start + new Vector2(-3f, _rain[i].Length);
                    // Draw as thin line (1px wide rect)
                    float angle = (float)Math.Atan2(end.Y - start.Y, end.X - start.X);
                    float len = Vector2.Distance(start, end);
                    sb.Draw(pixel, start, new Rectangle(0, 0, 1, 1), rc, angle,
                        Vector2.Zero, new Vector2(len, 1.5f), SpriteEffects.None, 0f);
                }
            }

            // Turbulent embers
            DrawEmbers(sb, pixel, eff, 0.8f);
        }

        private void DrawPhase4Eclipse(SpriteBatch sb, Texture2D pixel, float eff)
        {
            // Near-black sky
            Color darkSky = new Color(15, 8, 3) * eff * 0.9f;
            sb.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), darkSky);

            // Boss-centered light: soft glow around boss screen position
            Vector2 bossScreen = BossCenter - Main.screenPosition;
            float glareScale = 200f;
            Color bossGlow = new Color(255, 250, 240) * eff * 0.15f;
            bossGlow.A = 0;
            sb.Draw(pixel, bossScreen, new Rectangle(0, 0, 1, 1), bossGlow, 0f,
                new Vector2(0.5f), glareScale, SpriteEffects.None, 0f);

            Color bossGlow2 = new Color(255, 200, 50) * eff * 0.1f;
            bossGlow2.A = 0;
            sb.Draw(pixel, bossScreen, new Rectangle(0, 0, 1, 1), bossGlow2, 0f,
                new Vector2(0.5f), glareScale * 1.8f, SpriteEffects.None, 0f);

            // Sparse embers near boss only
            if (_embers != null)
            {
                for (int i = 0; i < MaxEmbers / 3; i++)
                {
                    float distToBoss = Vector2.Distance(_embers[i].Position, bossScreen);
                    float proximity = MathHelper.SmoothStep(1f, 0f, MathHelper.Clamp(distToBoss / 300f, 0f, 1f));
                    if (proximity < 0.05f) continue;

                    Color ec = _embers[i].Color * eff * 0.5f * proximity;
                    ec.A = 0;
                    sb.Draw(pixel, _embers[i].Position, new Rectangle(0, 0, 1, 1), ec, 0f,
                        new Vector2(0.5f), _embers[i].Scale * 2.5f, SpriteEffects.None, 0f);
                }
            }
        }

        private void DrawGradientSky(SpriteBatch sb, Texture2D pixel, Color top, Color bottom)
        {
            for (int y = 0; y < Main.screenHeight; y += 4)
            {
                float t = (float)y / Main.screenHeight;
                Color lineColor = Color.Lerp(top, bottom, t);
                sb.Draw(pixel, new Rectangle(0, y, Main.screenWidth, 4), lineColor);
            }
        }

        private void DrawEmbers(SpriteBatch sb, Texture2D pixel, float eff, float visibility)
        {
            if (_embers == null) return;
            float time = (float)Main.timeForVisualEffects;

            for (int i = 0; i < MaxEmbers; i++)
            {
                float shimmer = (float)Math.Sin(time * 0.02f + i * 0.5f) * 0.3f + 0.7f;
                Color c = _embers[i].Color * eff * shimmer * visibility;
                c.A = 0;
                sb.Draw(pixel, _embers[i].Position, new Rectangle(0, 0, 1, 1), c, 0f,
                    new Vector2(0.5f), _embers[i].Scale * 3f, SpriteEffects.None, 0f);
            }
        }

        private void DrawSolarGlare(SpriteBatch sb, Texture2D pixel, float eff, float intensity)
        {
            Color glareColor = new Color(255, 240, 180) * intensity * eff;
            glareColor.A = 0;
            Vector2 glarePos = new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.12f);
            sb.Draw(pixel, glarePos, new Rectangle(0, 0, 1, 1), glareColor, 0f,
                new Vector2(0.5f), 100f, SpriteEffects.None, 0f);
        }

        private void DrawVignette(SpriteBatch sb, Texture2D pixel, float eff)
        {
            float vigStr = _displayPhase switch
            {
                1 => 0.1f,
                2 => 0.2f,
                3 => 0.3f,
                _ => BossIsEnraged ? 0.5f : 0.2f
            };

            Color vigColor = BossIsEnraged ? new Color(30, 15, 5) : new Color(180, 100, 20);
            int ringCount = 8;

            for (int r = 0; r < ringCount; r++)
            {
                float ringT = (float)r / ringCount;
                float ringAlpha = eff * vigStr * ringT * ringT;
                Color rc = vigColor * ringAlpha;
                rc.A = 0;

                int inset = (int)((1f - ringT) * Math.Min(Main.screenWidth, Main.screenHeight) * 0.5f);
                if (inset < 1) inset = 1;
                Rectangle outer = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);

                sb.Draw(pixel, new Rectangle(outer.X, outer.Y, outer.Width, inset), rc);
                sb.Draw(pixel, new Rectangle(outer.X, outer.Bottom - inset, outer.Width, inset), rc);
                sb.Draw(pixel, new Rectangle(outer.X, inset, inset, Main.screenHeight - inset * 2), rc);
                sb.Draw(pixel, new Rectangle(outer.Right - inset, inset, inset, Main.screenHeight - inset * 2), rc);
            }
        }

        public override float GetCloudAlpha() => 1f - _opacity * 0.5f;

        // Flash API
        public static void TriggerSolarFlash(float intensity = 1f)
        {
            _flashIntensity = intensity;
            _flashDecay = 0.92f;
            _flashColor = new Color(255, 200, 50);
        }

        public static void TriggerScorchFlash(float intensity = 1f)
        {
            _flashIntensity = intensity;
            _flashDecay = 0.90f;
            _flashColor = new Color(220, 60, 30);
        }

        public static void TriggerZenithFlash(float intensity = 1f)
        {
            _flashIntensity = intensity;
            _flashDecay = 0.88f;
            _flashColor = new Color(255, 250, 230);
        }

        public static void TriggerSupernovaFlash(float intensity = 1f)
        {
            _flashIntensity = intensity;
            _flashDecay = 0.85f;
            _flashColor = Color.White;
        }

        public static void TriggerEclipseFlash(float intensity = 1f)
        {
            _flashIntensity = intensity;
            _flashDecay = 0.82f;
            _flashColor = new Color(180, 100, 20);
        }
    }

    /// <summary>
    /// Companion ModSystem: ambient world particles during the boss fight.
    /// </summary>
    public class LEstateSkySystem : ModSystem
    {
        public override void PostUpdateEverything()
        {
            if (Main.netMode == NetmodeID.Server) return;
            NPC boss = BossIndexTracker.GetActiveBoss(BossIndexTracker.LEstate);
            if (boss == null) return;

            float hpDrive = 1f - (boss.life / (float)boss.lifeMax);
            int phase = LEstateSky.CurrentPhase;

            // Ember sparks (all phases, density scales)
            int emberChance = Math.Max(1, (int)(10 - hpDrive * 6 - (phase - 1) * 2));
            if (Main.rand.NextBool(emberChance))
            {
                Vector2 pos = Main.LocalPlayer.Center + Main.rand.NextVector2Circular(600f, 400f);
                int d = Dust.NewDust(pos, 4, 4, DustID.Torch, 0f, -1.5f, 100, default, 1.1f);
                Main.dust[d].noGravity = true;
                Main.dust[d].fadeIn = 1.2f;
            }

            // Phase 2+: rising heat wisps
            if (phase >= 2 && Main.rand.NextBool(Math.Max(1, 6 - phase)))
            {
                Vector2 pos = Main.LocalPlayer.Center + Main.rand.NextVector2Circular(500f, 350f);
                int d = Dust.NewDust(pos, 4, 4, DustID.OrangeTorch, Main.rand.NextFloat(-0.5f, 0.5f), -1f, 120, default, 0.9f);
                Main.dust[d].noGravity = true;
            }

            // Phase 3: ground-level fire sparks
            if (phase >= 3 && Main.rand.NextBool(4))
            {
                Vector2 pos = Main.LocalPlayer.Bottom + new Vector2(Main.rand.NextFloat(-400f, 400f), Main.rand.NextFloat(-10f, 5f));
                int d = Dust.NewDust(pos, 4, 4, DustID.Torch, Main.rand.NextFloat(-2f, 2f), -Main.rand.NextFloat(2f, 4f), 80, default, 1.3f);
                Main.dust[d].noGravity = true;
            }
        }

        // Convenience static flash methods that delegate to LEstateSky
        public static void TriggerSolarFlash(float intensity = 1f) => LEstateSky.TriggerSolarFlash(intensity);
        public static void TriggerScorchFlash(float intensity = 1f) => LEstateSky.TriggerScorchFlash(intensity);
        public static void TriggerZenithFlash(float intensity = 1f) => LEstateSky.TriggerZenithFlash(intensity);
        public static void TriggerSupernovaFlash(float intensity = 1f) => LEstateSky.TriggerSupernovaFlash(intensity);
        public static void TriggerEclipseFlash(float intensity = 1f) => LEstateSky.TriggerEclipseFlash(intensity);
    }
}
