using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;

namespace MagnumOpus.Content.Autumn.Bosses.Systems
{
    /// <summary>
    /// 4-phase sky for Autunno (Vivaldi's Autumn):
    ///   Phase 1 — Twilight Hunt: painted sunset sky, gentle spiral leaves drifting down
    ///   Phase 2 — Harvest Reaping: sky darkens to bruised amber, wind gusts, fog wisps
    ///   Phase 3 — Death of the Year: desaturated gray-violet, skeletal branches, ashen motes
    ///   Phase 4 — Funeral Pyre (enrage): near-black void, boss ember silhouette, fog constriction
    /// </summary>
    public class AutunnoSky : CustomSky
    {
        private bool _isActive;
        private float _opacity;

        // Boss state fed from Autunno.cs
        public static float BossLifeRatio = 1f;
        public static Vector2 BossCenter = Vector2.Zero;
        public static bool BossIsEnraged = false;
        public static int CurrentPhase = 1; // 1-4

        // Smooth phase blending
        private float _phaseBlend;
        private int _displayPhase = 1;

        // Falling leaves (Phases 1-3, distinct behavior per phase)
        private struct FallingLeaf
        {
            public Vector2 Position;
            public float FallSpeed;
            public float SwayAmplitude;
            public float SwayPhase;
            public float Scale;
            public Color Color;
            public float Rotation;
            public float RotSpeed;
            public float SpiralAngle;   // spiral orbit angle
            public float SpiralRadius;  // spiral orbit radius
        }

        private FallingLeaf[] _leaves;
        private const int MaxLeaves = 80;

        // Fog wisps (Phase 2+)
        private struct FogWisp
        {
            public Vector2 Position;
            public float DriftSpeed;
            public float Phase;
            public float Scale;
            public float Opacity;
        }

        private FogWisp[] _fogWisps;
        private const int MaxFog = 30;

        // Sky flash state
        private static float _flashIntensity;
        private static float _flashDecay;
        private static Color _flashColor;

        public override void OnLoad() { }

        public override void Activate(Vector2 position, params object[] args)
        {
            _isActive = true;
            Random rand = new Random();

            _leaves = new FallingLeaf[MaxLeaves];
            for (int i = 0; i < MaxLeaves; i++)
            {
                _leaves[i] = new FallingLeaf
                {
                    Position = new Vector2(
                        (float)rand.NextDouble() * Main.screenWidth,
                        (float)rand.NextDouble() * Main.screenHeight),
                    FallSpeed = 0.3f + (float)rand.NextDouble() * 1.0f,
                    SwayAmplitude = 15f + (float)rand.NextDouble() * 30f,
                    SwayPhase = (float)rand.NextDouble() * MathHelper.TwoPi,
                    Scale = 0.3f + (float)rand.NextDouble() * 0.7f,
                    Color = Color.Lerp(new Color(200, 120, 40), new Color(180, 160, 60),
                        (float)rand.NextDouble()),
                    Rotation = (float)rand.NextDouble() * MathHelper.TwoPi,
                    RotSpeed = ((float)rand.NextDouble() - 0.5f) * 0.06f,
                    SpiralAngle = (float)rand.NextDouble() * MathHelper.TwoPi,
                    SpiralRadius = 20f + (float)rand.NextDouble() * 40f
                };
            }

            _fogWisps = new FogWisp[MaxFog];
            for (int i = 0; i < MaxFog; i++)
            {
                _fogWisps[i] = new FogWisp
                {
                    Position = new Vector2(
                        (float)rand.NextDouble() * Main.screenWidth,
                        Main.screenHeight * 0.5f + (float)rand.NextDouble() * Main.screenHeight * 0.5f),
                    DriftSpeed = 0.2f + (float)rand.NextDouble() * 0.8f,
                    Phase = (float)rand.NextDouble() * MathHelper.TwoPi,
                    Scale = 40f + (float)rand.NextDouble() * 80f,
                    Opacity = 0.1f + (float)rand.NextDouble() * 0.15f
                };
            }
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
            float windStrength = _displayPhase switch
            {
                1 => 0.3f,
                2 => 1.2f,
                3 => 0.6f, // eerie stillness with occasional gusts
                _ => BossIsEnraged ? 2.0f : 0.4f
            };

            // Update falling leaves
            if (_leaves != null)
            {
                for (int i = 0; i < MaxLeaves; i++)
                {
                    float speedMult = 1f + (_displayPhase - 1) * 0.3f;

                    // Phase 1: gentle spiral descent
                    if (_displayPhase == 1)
                    {
                        _leaves[i].SpiralAngle += 0.008f + i * 0.0003f;
                        float spiralX = (float)Math.Cos(_leaves[i].SpiralAngle) * _leaves[i].SpiralRadius * 0.01f;
                        _leaves[i].Position.X += spiralX;
                        _leaves[i].Position.Y += _leaves[i].FallSpeed * 0.8f;
                    }
                    // Phase 2: wind-driven, chaotic
                    else if (_displayPhase == 2)
                    {
                        _leaves[i].Position.Y += _leaves[i].FallSpeed * speedMult;
                        _leaves[i].Position.X += (float)Math.Sin(time * 0.01f + _leaves[i].SwayPhase)
                            * _leaves[i].SwayAmplitude * 0.03f * windStrength;
                        // Wind gusts
                        _leaves[i].Position.X += windStrength * 0.8f *
                            (float)Math.Sin(time * 0.004f + i * 0.5f);
                    }
                    // Phase 3: slow, heavy, barely drifting — dead leaves
                    else if (_displayPhase == 3)
                    {
                        _leaves[i].Position.Y += _leaves[i].FallSpeed * 0.4f;
                        _leaves[i].Position.X += (float)Math.Sin(time * 0.003f + _leaves[i].SwayPhase)
                            * 3f;
                    }
                    // Phase 4/Enrage: leaves catch fire, rise upward briefly then vanish
                    else
                    {
                        _leaves[i].Position.Y -= _leaves[i].FallSpeed * 0.5f;
                        _leaves[i].Position.X += (float)Math.Sin(time * 0.015f + i) * 2f;
                    }

                    _leaves[i].Rotation += _leaves[i].RotSpeed * (1f + (_displayPhase - 1) * 0.3f);

                    // Wrap around
                    if (_leaves[i].Position.Y > Main.screenHeight + 20)
                    {
                        _leaves[i].Position.Y = -20;
                        _leaves[i].Position.X = Main.rand.NextFloat() * Main.screenWidth;
                    }
                    if (_leaves[i].Position.Y < -30)
                    {
                        _leaves[i].Position.Y = Main.screenHeight + 20;
                        _leaves[i].Position.X = Main.rand.NextFloat() * Main.screenWidth;
                    }
                    if (_leaves[i].Position.X < -30)
                        _leaves[i].Position.X = Main.screenWidth + 20;
                    if (_leaves[i].Position.X > Main.screenWidth + 30)
                        _leaves[i].Position.X = -20;
                }
            }

            // Update fog wisps (Phase 2+)
            if (_fogWisps != null && _displayPhase >= 2)
            {
                for (int i = 0; i < MaxFog; i++)
                {
                    _fogWisps[i].Position.X += _fogWisps[i].DriftSpeed * windStrength * 0.3f;
                    _fogWisps[i].Position.Y += (float)Math.Sin(time * 0.002f + _fogWisps[i].Phase) * 0.3f;

                    if (_fogWisps[i].Position.X > Main.screenWidth + _fogWisps[i].Scale)
                    {
                        _fogWisps[i].Position.X = -_fogWisps[i].Scale;
                        _fogWisps[i].Position.Y = Main.screenHeight * 0.4f + Main.rand.NextFloat() * Main.screenHeight * 0.6f;
                    }
                }
            }

            // Sky flash decay
            if (_flashIntensity > 0.01f)
                _flashIntensity *= _flashDecay;
            else
                _flashIntensity = 0f;
        }

        public override Color OnTileColor(Color inColor)
        {
            float eff = _opacity;
            if (_displayPhase <= 1)
            {
                // Warm golden sunset tint
                Color tint = new Color(240, 200, 140);
                return Color.Lerp(inColor, tint, eff * 0.3f);
            }
            else if (_displayPhase == 2)
            {
                // Darkening amber with brown undertone
                Color tint = new Color(180, 130, 70);
                return Color.Lerp(inColor, tint, eff * 0.35f);
            }
            else if (BossIsEnraged)
            {
                // Funeral Pyre: near-darkness, warm ember undertone
                Color dark = new Color(40, 20, 10);
                return Color.Lerp(inColor, dark, eff * 0.6f);
            }
            else
            {
                // Death of the Year: desaturated gray-violet
                Color withered = new Color(100, 80, 100);
                return Color.Lerp(inColor, withered, eff * 0.45f);
            }
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth < 0f || minDepth >= 0f) return;

            float eff = _opacity;
            if (eff < 0.01f) return;

            Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;

            if (BossIsEnraged)
                DrawPhase4FuneralPyre(spriteBatch, pixel, eff);
            else if (_displayPhase >= 3)
                DrawPhase3DeathOfTheYear(spriteBatch, pixel, eff);
            else if (_displayPhase == 2)
                DrawPhase2HarvestReaping(spriteBatch, pixel, eff);
            else
                DrawPhase1TwilightHunt(spriteBatch, pixel, eff);

            // Sky flash overlay
            if (_flashIntensity > 0.01f)
            {
                Color fc = _flashColor * (_flashIntensity * eff * 0.3f);
                fc.A = 0;
                spriteBatch.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), fc);
            }

            // Vignette (all phases, intensity varies)
            DrawVignette(spriteBatch, pixel, eff);
        }

        private void DrawPhase1TwilightHunt(SpriteBatch sb, Texture2D pixel, float eff)
        {
            // Painted sunset gradient: deep amber top → warm gold-orange horizon
            Color topColor = new Color(80, 35, 15) * eff * 0.7f;
            Color bottomColor = new Color(200, 120, 40) * eff * 0.7f;
            DrawGradientSky(sb, pixel, topColor, bottomColor);

            // Sunset glare at horizon
            DrawSunsetGlare(sb, pixel, eff, 0.2f);

            // Gentle spiral leaves
            DrawLeaves(sb, pixel, eff, 0.5f, false);
        }

        private void DrawPhase2HarvestReaping(SpriteBatch sb, Texture2D pixel, float eff)
        {
            // Bruised amber sky darkening
            Color topColor = new Color(55, 28, 12) * eff * 0.8f;
            Color bottomColor = new Color(140, 80, 25) * eff * 0.8f;
            DrawGradientSky(sb, pixel, topColor, bottomColor);

            // Dimmi sunset glare
            DrawSunsetGlare(sb, pixel, eff, 0.1f);

            // Wind-driven leaves
            DrawLeaves(sb, pixel, eff, 0.65f, false);

            // Rolling fog wisps
            DrawFogWisps(sb, pixel, eff, 0.5f);
        }

        private void DrawPhase3DeathOfTheYear(SpriteBatch sb, Texture2D pixel, float eff)
        {
            // Desaturated gray-violet sky — the color is draining from autumn
            Color topColor = new Color(35, 25, 40) * eff * 0.85f;
            Color bottomColor = new Color(80, 60, 75) * eff * 0.85f;
            DrawGradientSky(sb, pixel, topColor, bottomColor);

            // Dead leaves — desaturated, barely moving
            DrawLeaves(sb, pixel, eff, 0.35f, true);

            // Dense fog
            DrawFogWisps(sb, pixel, eff, 0.8f);

            // Ashen motes — pale gray flecks drifting
            DrawAshenMotes(sb, pixel, eff);
        }

        private void DrawPhase4FuneralPyre(SpriteBatch sb, Texture2D pixel, float eff)
        {
            // Near-black void
            Color darkSky = new Color(15, 8, 5) * eff * 0.92f;
            sb.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), darkSky);

            // Boss-centered ember glow — boss is the only light
            Vector2 bossScreen = BossCenter - Main.screenPosition;

            Color outerGlow = new Color(150, 50, 20) * eff * 0.12f;
            outerGlow.A = 0;
            sb.Draw(pixel, bossScreen, new Rectangle(0, 0, 1, 1), outerGlow, 0f,
                new Vector2(0.5f), 250f, SpriteEffects.None, 0f);

            Color innerGlow = new Color(255, 140, 40) * eff * 0.08f;
            innerGlow.A = 0;
            sb.Draw(pixel, bossScreen, new Rectangle(0, 0, 1, 1), innerGlow, 0f,
                new Vector2(0.5f), 120f, SpriteEffects.None, 0f);

            // Embers rising from boss (leaves that caught fire)
            if (_leaves != null)
            {
                for (int i = 0; i < MaxLeaves / 3; i++)
                {
                    float distToBoss = Vector2.Distance(_leaves[i].Position, bossScreen);
                    float proximity = MathHelper.SmoothStep(1f, 0f, MathHelper.Clamp(distToBoss / 300f, 0f, 1f));
                    if (proximity < 0.05f) continue;

                    // Ember orange-red color for burning leaves
                    Color ec = Color.Lerp(new Color(255, 100, 20), new Color(200, 50, 10),
                        (float)Math.Sin(i * 0.8f) * 0.5f + 0.5f) * eff * 0.6f * proximity;
                    ec.A = 0;
                    sb.Draw(pixel, _leaves[i].Position, new Rectangle(0, 0, 1, 1), ec, _leaves[i].Rotation,
                        new Vector2(0.5f), _leaves[i].Scale * 2.5f, SpriteEffects.None, 0f);
                }
            }

            // Constricting fog — fog closes in around boss, shrinking clear radius
            DrawConstrictionFog(sb, pixel, eff, bossScreen);
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

        private void DrawLeaves(SpriteBatch sb, Texture2D pixel, float eff, float visibility, bool desaturated)
        {
            if (_leaves == null || eff < 0.1f) return;

            for (int i = 0; i < MaxLeaves; i++)
            {
                Color c = _leaves[i].Color;
                if (desaturated)
                {
                    // Drain color — shift toward gray-brown
                    float gray = (c.R + c.G + c.B) / 3f / 255f;
                    c = Color.Lerp(c, new Color((int)(gray * 140), (int)(gray * 120), (int)(gray * 110)), 0.7f);
                }
                c *= eff * visibility;
                c.A = 0;

                float s = _leaves[i].Scale * (desaturated ? 0.8f : 1f);
                sb.Draw(pixel, _leaves[i].Position, new Rectangle(0, 0, 1, 1), c,
                    _leaves[i].Rotation, new Vector2(0.5f),
                    new Vector2(s * 4f, s * 2f), SpriteEffects.None, 0f);
            }
        }

        private void DrawFogWisps(SpriteBatch sb, Texture2D pixel, float eff, float density)
        {
            if (_fogWisps == null) return;
            float time = (float)Main.timeForVisualEffects;
            Color fogColor = _displayPhase >= 3 ? new Color(80, 70, 80) : new Color(120, 90, 50);

            for (int i = 0; i < MaxFog; i++)
            {
                float breathe = (float)Math.Sin(time * 0.003f + _fogWisps[i].Phase) * 0.3f + 0.7f;
                Color fc = fogColor * eff * _fogWisps[i].Opacity * density * breathe;
                fc.A = 0;
                sb.Draw(pixel, _fogWisps[i].Position, new Rectangle(0, 0, 1, 1), fc, 0f,
                    new Vector2(0.5f), _fogWisps[i].Scale, SpriteEffects.None, 0f);
            }
        }

        private void DrawAshenMotes(SpriteBatch sb, Texture2D pixel, float eff)
        {
            float time = (float)Main.timeForVisualEffects;

            for (int i = 0; i < 25; i++)
            {
                float px = (float)Math.Sin(time * 0.001f + i * 2.3f) * Main.screenWidth * 0.4f + Main.screenWidth * 0.5f;
                float py = (float)Math.Cos(time * 0.0008f + i * 1.7f) * Main.screenHeight * 0.3f + Main.screenHeight * 0.5f;
                float shimmer = (float)Math.Sin(time * 0.02f + i) * 0.3f + 0.7f;
                Color ashColor = new Color(160, 150, 140) * eff * 0.3f * shimmer;
                ashColor.A = 0;
                sb.Draw(pixel, new Vector2(px, py), new Rectangle(0, 0, 1, 1), ashColor, 0f,
                    new Vector2(0.5f), 1.5f + shimmer, SpriteEffects.None, 0f);
            }
        }

        private void DrawConstrictionFog(SpriteBatch sb, Texture2D pixel, float eff, Vector2 bossScreen)
        {
            // Dark fog that closes in — clear circle around boss shrinks over time
            float clearRadius = 180f;
            Color fogColor = new Color(10, 5, 3) * eff * 0.7f;
            fogColor.A = 0;

            // Draw fog rings from edge inward, stopping at clear radius
            int rings = 16;
            for (int r = 0; r < rings; r++)
            {
                float ringT = (float)r / rings;
                float insetFraction = ringT; // 0 = outer edge, 1 = center

                // Each ring gets stronger as it gets closer to center (past clear zone)
                float distFromCenter = (1f - insetFraction) * Math.Max(Main.screenWidth, Main.screenHeight) * 0.5f;
                if (distFromCenter < clearRadius) continue;

                float fogStrength = MathHelper.SmoothStep(0f, 1f,
                    MathHelper.Clamp((distFromCenter - clearRadius) / 200f, 0f, 1f));

                // Offset rings toward boss position to center the clear zone on the boss
                float ringRadius = distFromCenter;
                Color rc = fogColor * fogStrength * 0.4f;

                // Simple approach: draw from edges
                int inset = (int)((1f - ringT) * Math.Min(Main.screenWidth, Main.screenHeight) * 0.5f);
                if (inset < 1) inset = 1;

                sb.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, inset), rc);
                sb.Draw(pixel, new Rectangle(0, Main.screenHeight - inset, Main.screenWidth, inset), rc);
                sb.Draw(pixel, new Rectangle(0, inset, inset, Main.screenHeight - inset * 2), rc);
                sb.Draw(pixel, new Rectangle(Main.screenWidth - inset, inset, inset, Main.screenHeight - inset * 2), rc);
            }
        }

        private void DrawSunsetGlare(SpriteBatch sb, Texture2D pixel, float eff, float intensity)
        {
            Color glareColor = new Color(255, 180, 80) * intensity * eff;
            glareColor.A = 0;
            Vector2 glarePos = new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.85f);
            sb.Draw(pixel, glarePos, new Rectangle(0, 0, 1, 1), glareColor, 0f,
                new Vector2(0.5f), 120f, SpriteEffects.None, 0f);
        }

        private void DrawVignette(SpriteBatch sb, Texture2D pixel, float eff)
        {
            float vigStr = _displayPhase switch
            {
                1 => 0.1f,
                2 => 0.22f,
                3 => 0.35f,
                _ => BossIsEnraged ? 0.55f : 0.25f
            };

            Color vigColor = _displayPhase switch
            {
                1 => new Color(200, 120, 40),
                2 => new Color(150, 80, 25),
                3 => new Color(80, 60, 80),
                _ => new Color(40, 15, 5)
            };

            int ringCount = 8;
            for (int r = 0; r < ringCount; r++)
            {
                float ringT = (float)r / ringCount;
                float ringAlpha = eff * vigStr * ringT * ringT;
                Color rc = vigColor * ringAlpha;
                rc.A = 0;

                int inset = (int)((1f - ringT) * Math.Min(Main.screenWidth, Main.screenHeight) * 0.5f);
                if (inset < 1) inset = 1;

                sb.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, inset), rc);
                sb.Draw(pixel, new Rectangle(0, Main.screenHeight - inset, Main.screenWidth, inset), rc);
                sb.Draw(pixel, new Rectangle(0, inset, inset, Main.screenHeight - inset * 2), rc);
                sb.Draw(pixel, new Rectangle(Main.screenWidth - inset, inset, inset, Main.screenHeight - inset * 2), rc);
            }
        }

        public override float GetCloudAlpha() => 1f - _opacity * 0.5f;

        // ===== FLASH API =====
        // Twilight: warm orange glow
        public static void TriggerTwilightFlash(float intensity = 1f)
        {
            _flashIntensity = intensity;
            _flashDecay = 0.92f;
            _flashColor = new Color(200, 120, 40);
        }

        // Harvest: rich gold burst
        public static void TriggerHarvestFlash(float intensity = 1f)
        {
            _flashIntensity = intensity;
            _flashDecay = 0.90f;
            _flashColor = new Color(218, 165, 32);
        }

        // Withering: sickly brown-red
        public static void TriggerWitheringFlash(float intensity = 1f)
        {
            _flashIntensity = intensity;
            _flashDecay = 0.88f;
            _flashColor = new Color(150, 50, 30);
        }

        // Funeral: dim ember white
        public static void TriggerFuneralFlash(float intensity = 1f)
        {
            _flashIntensity = intensity;
            _flashDecay = 0.85f;
            _flashColor = new Color(255, 200, 140);
        }

        // Final death: pure white
        public static void TriggerFinalFlash(float intensity = 1f)
        {
            _flashIntensity = intensity;
            _flashDecay = 0.82f;
            _flashColor = Color.White;
        }
    }

    /// <summary>
    /// Companion ModSystem: ambient world particles during the Autunno boss fight
    /// and convenience flash method delegates.
    /// </summary>
    public class AutunnoSkySystem : ModSystem
    {
        public override void PostUpdateEverything()
        {
            if (Main.netMode == NetmodeID.Server) return;
            NPC boss = BossIndexTracker.GetActiveBoss(BossIndexTracker.Autunno);
            if (boss == null) return;

            float hpDrive = 1f - (boss.life / (float)boss.lifeMax);
            int phase = AutunnoSky.CurrentPhase;

            // Falling leaf dust (all phases, density scales)
            int leafChance = Math.Max(1, (int)(10 - hpDrive * 5 - (phase - 1) * 2));
            if (Main.rand.NextBool(leafChance))
            {
                Vector2 pos = Main.LocalPlayer.Center + Main.rand.NextVector2Circular(500f, 350f);
                int d = Dust.NewDust(pos, 4, 4, DustID.AmberBolt, Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(0.3f, 1.2f), 100, default, 0.8f);
                Main.dust[d].noGravity = true;
                Main.dust[d].fadeIn = 1.2f;
            }

            // Phase 2+: wind-blown embers & wisps
            if (phase >= 2 && Main.rand.NextBool(Math.Max(1, 6 - phase)))
            {
                Vector2 pos = Main.LocalPlayer.Center + Main.rand.NextVector2Circular(400f, 300f);
                int d = Dust.NewDust(pos, 4, 4, DustID.Torch, Main.rand.NextFloat(-0.5f, 0.5f), -1f, 120, default, 0.9f);
                Main.dust[d].noGravity = true;
            }

            // Phase 3+: ashen motes rising slowly
            if (phase >= 3 && Main.rand.NextBool(5))
            {
                Vector2 pos = Main.LocalPlayer.Bottom + new Vector2(Main.rand.NextFloat(-350f, 350f), Main.rand.NextFloat(-8f, 3f));
                int d = Dust.NewDust(pos, 4, 4, DustID.Smoke, Main.rand.NextFloat(-0.3f, 0.3f), -Main.rand.NextFloat(0.5f, 1.5f), 150, default, 0.7f);
                Main.dust[d].noGravity = true;
            }

            // Phase 4: ground-level ember sparks
            if (phase >= 4 && Main.rand.NextBool(3))
            {
                Vector2 pos = Main.LocalPlayer.Bottom + new Vector2(Main.rand.NextFloat(-300f, 300f), Main.rand.NextFloat(-5f, 5f));
                int d = Dust.NewDust(pos, 4, 4, DustID.Torch, Main.rand.NextFloat(-1.5f, 1.5f), -Main.rand.NextFloat(2f, 4f), 80, default, 1.2f);
                Main.dust[d].noGravity = true;
            }
        }

        // Convenience delegates for using static imports in AttackVFX
        public static void TriggerTwilightFlash(float intensity = 1f) => AutunnoSky.TriggerTwilightFlash(intensity);
        public static void TriggerHarvestFlash(float intensity = 1f) => AutunnoSky.TriggerHarvestFlash(intensity);
        public static void TriggerWitheringFlash(float intensity = 1f) => AutunnoSky.TriggerWitheringFlash(intensity);
        public static void TriggerFuneralFlash(float intensity = 1f) => AutunnoSky.TriggerFuneralFlash(intensity);
        public static void TriggerFinalFlash(float intensity = 1f) => AutunnoSky.TriggerFinalFlash(intensity);
    }
}