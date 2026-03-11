using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.OdeToJoy.Bosses.Systems
{
    /// <summary>
    /// Custom sky effect for the Ode to Joy boss fight.
    /// Phase 1: Sunlit garden with gentle drifting petals.
    /// Phase 2: Chromatic light show with radiant beams and blooming flower patterns.
    /// HP-driven intensity, distance falloff, and warm golden vignette.
    /// </summary>
    public class OdeToJoySky : CustomSky
    {
        private bool _isActive;
        private float _opacity;
        private float _chromaticIntensity;

        // Boss state
        private float _bossLifeRatio = 1f;
        private Vector2 _bossCenter;
        private bool _bossIsPhase2;
        private float _vignetteStrength;

        private struct GardenPetal
        {
            public Vector2 Position;
            public float Speed;
            public float Scale;
            public Color Color;
            public float Rotation;
            public float RotSpeed;
            public float SwayOffset;
        }

        private GardenPetal[] _petals;
        private const int MaxPetals = 50;

        private struct LightBeam
        {
            public float Angle;
            public float Width;
            public float Length;
            public Color Color;
            public float PulseOffset;
        }

        private LightBeam[] _beams;
        private const int MaxBeams = 6;

        // Static properties for companion system
        public static float BossLifeRatio { get; set; } = 1f;
        public static Vector2 BossCenter { get; set; }
        public static bool BossIsPhase2 { get; set; }

        public override void OnLoad() { }

        public override void Activate(Vector2 position, params object[] args)
        {
            _isActive = true;
            Random rand = new Random();
            _petals = new GardenPetal[MaxPetals];
            _beams = new LightBeam[MaxBeams];

            Color[] petalColors = {
                new Color(255, 200, 50),
                new Color(240, 160, 40),
                new Color(230, 130, 150),
                new Color(255, 240, 200)
            };

            for (int i = 0; i < MaxPetals; i++)
            {
                _petals[i] = new GardenPetal
                {
                    Position = new Vector2(
                        (float)rand.NextDouble() * Main.screenWidth,
                        (float)rand.NextDouble() * Main.screenHeight),
                    Speed = 0.3f + (float)rand.NextDouble() * 1.0f,
                    Scale = 0.3f + (float)rand.NextDouble() * 0.6f,
                    Color = petalColors[rand.Next(petalColors.Length)],
                    Rotation = (float)rand.NextDouble() * MathHelper.TwoPi,
                    RotSpeed = ((float)rand.NextDouble() - 0.5f) * 0.04f,
                    SwayOffset = (float)rand.NextDouble() * MathHelper.TwoPi
                };
            }

            for (int i = 0; i < MaxBeams; i++)
            {
                _beams[i] = new LightBeam
                {
                    Angle = (float)rand.NextDouble() * MathHelper.Pi * 0.6f - MathHelper.Pi * 0.3f,
                    Width = 20f + (float)rand.NextDouble() * 40f,
                    Length = Main.screenHeight * 0.8f + (float)rand.NextDouble() * Main.screenHeight * 0.4f,
                    Color = petalColors[rand.Next(petalColors.Length)],
                    PulseOffset = (float)rand.NextDouble() * MathHelper.TwoPi
                };
            }
        }

        public override void Deactivate(params object[] args)
        {
            _isActive = false;
        }

        public override void Reset()
        {
            _isActive = false;
            _opacity = 0f;
        }

        public override bool IsActive() => _isActive || _opacity > 0.001f;

        private float GetEffectiveIntensity()
        {
            if (Main.LocalPlayer == null) return 1f;
            float dist = Vector2.Distance(Main.LocalPlayer.Center, _bossCenter);
            return MathHelper.SmoothStep(1f, 0.2f, MathHelper.Clamp(dist / 3500f, 0f, 1f));
        }

        private void UpdateBossState()
        {
            NPC boss = BossIndexTracker.GetActiveBoss(BossIndexTracker.OdeToJoyConductor);
            if (boss != null)
            {
                _bossLifeRatio = boss.life / (float)boss.lifeMax;
                _bossCenter = boss.Center;
            }
            _bossIsPhase2 = BossIsPhase2;
        }

        public override void Update(GameTime gameTime)
        {
            if (_isActive)
                _opacity = Math.Min(1f, _opacity + 0.02f);
            else
                _opacity = Math.Max(0f, _opacity - 0.02f);

            UpdateBossState();

            // Phase 2 chromatic intensity
            float targetChromatic = _bossIsPhase2 ? 1f : 0f;
            _chromaticIntensity += (targetChromatic - _chromaticIntensity) * 0.02f;

            float hpDrive = 1f - _bossLifeRatio;

            // Vignette: gentle warm glow, stronger in Phase 2
            float targetVignette = _bossIsPhase2
                ? 0.25f + hpDrive * 0.2f
                : 0.1f + hpDrive * 0.12f;
            _vignetteStrength += (targetVignette - _vignetteStrength) * 0.03f;

            // Update petals - HP drives speed and density
            if (_petals != null)
            {
                float speedMult = 1f + hpDrive * 0.5f;
                for (int i = 0; i < MaxPetals; i++)
                {
                    _petals[i].Position.Y += _petals[i].Speed * speedMult;
                    _petals[i].Position.X += (float)Math.Sin(
                        Main.timeForVisualEffects * 0.008f + _petals[i].SwayOffset) * (0.5f + hpDrive * 0.3f);
                    _petals[i].Rotation += _petals[i].RotSpeed;

                    if (_petals[i].Position.Y > Main.screenHeight + 20)
                    {
                        _petals[i].Position.Y = -20;
                        _petals[i].Position.X = Main.rand.NextFloat() * Main.screenWidth;
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth >= 0f && minDepth < 0f)
            {
                Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
                float effectiveIntensity = GetEffectiveIntensity();
                float hpDrive = 1f - _bossLifeRatio;

                // Sky gradient
                Color topColor = Color.Lerp(new Color(80, 60, 20), new Color(100, 50, 70), _chromaticIntensity);
                Color bottomColor = Color.Lerp(new Color(120, 90, 30), new Color(150, 100, 80), _chromaticIntensity);
                topColor *= _opacity * (0.6f + hpDrive * 0.15f) * effectiveIntensity;
                bottomColor *= _opacity * (0.6f + hpDrive * 0.15f) * effectiveIntensity;

                for (int y = 0; y < Main.screenHeight; y += 4)
                {
                    float t = (float)y / Main.screenHeight;
                    Color lineColor = Color.Lerp(topColor, bottomColor, t);
                    spriteBatch.Draw(pixel, new Rectangle(0, y, Main.screenWidth, 4), lineColor);
                }

                // Light beams - HP drives brightness, Phase 2 adds chromatic shift
                if (_beams != null && _opacity > 0.1f)
                {
                    for (int i = 0; i < MaxBeams; i++)
                    {
                        float pulse = (float)Math.Sin(Main.timeForVisualEffects * 0.01f + _beams[i].PulseOffset);
                        float beamAlpha = _opacity * effectiveIntensity * (0.05f + pulse * 0.03f + _chromaticIntensity * 0.06f + hpDrive * 0.04f);
                        Color beamColor;

                        if (_chromaticIntensity > 0.3f)
                        {
                            float hue = ((float)Main.timeForVisualEffects * 0.002f + i * 0.15f) % 1f;
                            beamColor = Main.hslToRgb(hue, 0.7f, 0.6f) * beamAlpha;
                        }
                        else
                        {
                            beamColor = _beams[i].Color * beamAlpha;
                        }
                        beamColor.A = 0;

                        float x = Main.screenWidth * (0.15f + i * 0.14f);
                        spriteBatch.Draw(pixel,
                            new Vector2(x, 0),
                            new Rectangle(0, 0, 1, 1),
                            beamColor,
                            _beams[i].Angle,
                            Vector2.Zero,
                            new Vector2(_beams[i].Width, _beams[i].Length),
                            SpriteEffects.None, 0f);
                    }
                }

                // Floating petals - HP drives visibility and glow
                if (_petals != null && _opacity > 0.1f)
                {
                    float petalAlpha = 0.4f + hpDrive * 0.3f;

                    for (int i = 0; i < MaxPetals; i++)
                    {
                        Color c = _petals[i].Color * _opacity * petalAlpha * effectiveIntensity;

                        if (_chromaticIntensity > 0.3f)
                        {
                            float hue = ((float)Main.timeForVisualEffects * 0.003f + i * 0.05f) % 1f;
                            c = Main.hslToRgb(hue, 0.7f, 0.6f) * (_opacity * petalAlpha * _chromaticIntensity * effectiveIntensity);
                        }
                        c.A = 0;

                        float s = _petals[i].Scale * (1f + _chromaticIntensity * 0.3f + hpDrive * 0.2f);
                        spriteBatch.Draw(pixel,
                            _petals[i].Position,
                            new Rectangle(0, 0, 1, 1),
                            c,
                            _petals[i].Rotation,
                            new Vector2(0.5f),
                            s * 3f,
                            SpriteEffects.None, 0f);
                    }
                }

                // === WARM GOLDEN VIGNETTE ===
                if (_vignetteStrength > 0.01f && _opacity > 0.1f)
                {
                    float vigAlpha = _vignetteStrength * _opacity * effectiveIntensity;
                    Color vigBase = _bossIsPhase2
                        ? Color.Lerp(new Color(60, 40, 10), new Color(80, 30, 40), _chromaticIntensity)
                        : new Color(40, 30, 8);

                    int rings = _bossIsPhase2 ? 12 : 8;
                    for (int ring = 0; ring < rings; ring++)
                    {
                        float ringT = (float)ring / rings;
                        float edgeDist = 1f - ringT;
                        float ringAlpha = edgeDist * edgeDist * vigAlpha;
                        Color ringColor = vigBase * ringAlpha;

                        int thickness = Math.Max(1, (int)(Main.screenHeight * 0.05f * edgeDist));

                        spriteBatch.Draw(pixel, new Rectangle(0, ring * thickness / 2, Main.screenWidth, thickness), ringColor);
                        spriteBatch.Draw(pixel, new Rectangle(0, Main.screenHeight - ring * thickness / 2 - thickness, Main.screenWidth, thickness), ringColor);
                        spriteBatch.Draw(pixel, new Rectangle(ring * thickness / 2, 0, thickness, Main.screenHeight), ringColor);
                        spriteBatch.Draw(pixel, new Rectangle(Main.screenWidth - ring * thickness / 2 - thickness, 0, thickness, Main.screenHeight), ringColor);
                    }
                }
            }
        }

        public override Color OnTileColor(Color inColor)
        {
            float effectiveIntensity = GetEffectiveIntensity();
            float tintStrength = _bossIsPhase2 ? 0.3f : 0.22f;
            tintStrength *= effectiveIntensity;

            Color tint = Color.Lerp(Color.White,
                new Color(255, 230, 180), _opacity * tintStrength);
            return inColor.MultiplyRGBA(tint);
        }

        public override float GetCloudAlpha() => 1f - _opacity * 0.5f;
    }

    /// <summary>
    /// Companion system for OdeToJoySky.
    /// Flash APIs for attacks and ambient garden particles.
    /// </summary>
    public class OdeToJoySkySystem : ModSystem
    {
        private static float _flashIntensity;
        private static Color _flashColor;
        private static float _flashDecay;

        /// <summary>Warm golden flash for garden attacks.</summary>
        public static void TriggerGardenFlash(float intensity)
        {
            _flashIntensity = Math.Max(_flashIntensity, intensity);
            _flashColor = new Color(255, 200, 50);
            _flashDecay = 0.92f;
        }

        /// <summary>Rose-pink flash for petal attacks.</summary>
        public static void TriggerPetalFlash(float intensity)
        {
            _flashIntensity = Math.Max(_flashIntensity, intensity);
            _flashColor = new Color(230, 130, 150);
            _flashDecay = 0.90f;
        }

        /// <summary>Bright jubilant flash for chromatic/ultimate attacks.</summary>
        public static void TriggerJubilantFlash(float intensity)
        {
            _flashIntensity = Math.Max(_flashIntensity, intensity);
            _flashColor = new Color(255, 240, 200);
            _flashDecay = 0.88f;
        }

        /// <summary>Pure white flash for the final bloom moment.</summary>
        public static void TriggerEternalBloomFlash(float intensity)
        {
            _flashIntensity = Math.Max(_flashIntensity, intensity);
            _flashColor = Color.White;
            _flashDecay = 0.85f;
        }

        public static (float intensity, Color color) GetFlashState() => (_flashIntensity, _flashColor);

        public override void PostUpdateEverything()
        {
            _flashIntensity *= _flashDecay;
            if (_flashIntensity < 0.01f)
                _flashIntensity = 0f;

            if (Main.netMode == NetmodeID.Server) return;

            NPC boss = BossIndexTracker.GetActiveBoss(BossIndexTracker.OdeToJoyConductor);
            if (boss == null || !boss.active) return;

            float hpDrive = 1f - (boss.life / (float)boss.lifeMax);

            // Ambient golden pollen dust near boss
            if (Main.rand.NextBool(Math.Max(1, (int)(10 - hpDrive * 6))))
            {
                Vector2 dustPos = boss.Center + new Vector2(Main.rand.NextFloat(-250f, 250f), Main.rand.NextFloat(30f, 150f));
                Dust d = Dust.NewDustDirect(dustPos, 4, 4, DustID.GoldFlame, Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(0.5f, 2f), 0, default, 1.0f + hpDrive * 0.6f);
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }

            // Phase 2: chromatic sparkle dust
            if (OdeToJoySky.BossIsPhase2 && Main.rand.NextBool(Math.Max(1, (int)(8 - hpDrive * 4))))
            {
                Vector2 sparkPos = boss.Center + Main.rand.NextVector2Circular(180f, 180f);
                float hue = (float)(Main.timeForVisualEffects * 0.01) % 1f;
                Color rainbow = Main.hslToRgb(hue, 0.8f, 0.7f);
                Dust rd = Dust.NewDustDirect(sparkPos, 2, 2, DustID.RainbowTorch, 0f, -Main.rand.NextFloat(1f, 2.5f), 0, rainbow, 0.8f);
                rd.noGravity = true;
            }
        }
    }
}