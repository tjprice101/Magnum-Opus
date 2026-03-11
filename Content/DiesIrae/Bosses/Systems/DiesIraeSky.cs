using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.DiesIrae.Bosses.Systems
{
    /// <summary>
    /// Custom sky effect for the Dies Irae boss fight.
    /// Creates an apocalyptic hellfire atmosphere with blood-red sky,
    /// falling ash particles, crimson lightning flashes, and vignette.
    /// Escalates in intensity with HP — darker, more violent,
    /// more ash, more lightning as the Herald's wrath builds.
    /// </summary>
    public class DiesIraeSky : CustomSky
    {
        private bool _isActive;
        private float _opacity;
        private float _wrathIntensity;

        // Boss state for distance-based falloff
        private float _bossLifeRatio = 1f;
        private Vector2 _bossCenter;
        private bool _bossIsEnraged;
        private float _vignetteStrength;

        private struct AshParticle
        {
            public Vector2 Position;
            public float Speed;
            public float Scale;
            public float Rotation;
            public float RotSpeed;
            public float Drift;
        }

        private AshParticle[] _ash;
        private const int MaxAsh = 80;

        // Lightning flash
        private float _lightningTimer;
        private float _lightningBrightness;
        private float _lightningDecay;

        // Static boss state for companion system
        public static float BossLifeRatio { get; set; } = 1f;
        public static Vector2 BossCenter { get; set; }
        public static bool BossIsEnraged { get; set; }

        public override void OnLoad() { }

        public override void Activate(Vector2 position, params object[] args)
        {
            _isActive = true;
            Random rand = new Random();
            _ash = new AshParticle[MaxAsh];

            for (int i = 0; i < MaxAsh; i++)
            {
                _ash[i] = new AshParticle
                {
                    Position = new Vector2(
                        (float)rand.NextDouble() * Main.screenWidth,
                        (float)rand.NextDouble() * Main.screenHeight),
                    Speed = 0.4f + (float)rand.NextDouble() * 1.2f,
                    Scale = 0.2f + (float)rand.NextDouble() * 0.5f,
                    Rotation = (float)rand.NextDouble() * MathHelper.TwoPi,
                    RotSpeed = ((float)rand.NextDouble() - 0.5f) * 0.06f,
                    Drift = ((float)rand.NextDouble() - 0.5f) * 0.8f
                };
            }

            _lightningTimer = 0f;
            _lightningBrightness = 0f;
            _lightningDecay = 0.05f;
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

        /// <summary>
        /// Distance-based intensity falloff from boss center.
        /// Full intensity near boss, fading to 0.2 at 4000px.
        /// </summary>
        private float GetEffectiveIntensity()
        {
            if (Main.LocalPlayer == null) return 1f;
            float dist = Vector2.Distance(Main.LocalPlayer.Center, _bossCenter);
            return MathHelper.SmoothStep(1f, 0.2f, MathHelper.Clamp(dist / 4000f, 0f, 1f));
        }

        private void UpdateBossState()
        {
            NPC boss = BossIndexTracker.GetActiveBoss(BossIndexTracker.DiesIraeHerald);
            if (boss != null)
            {
                _bossLifeRatio = boss.life / (float)boss.lifeMax;
                _bossCenter = boss.Center;
            }

            _bossIsEnraged = BossIsEnraged;
        }

        public override void Update(GameTime gameTime)
        {
            if (_isActive)
                _opacity = Math.Min(1f, _opacity + 0.02f);
            else
                _opacity = Math.Max(0f, _opacity - 0.02f);

            UpdateBossState();

            // Wrath intensity from boss HP
            float targetWrath = 1f - _bossLifeRatio;
            _wrathIntensity += (targetWrath - _wrathIntensity) * 0.015f;

            // Vignette strength: enraged is heavier, scales with wrath
            float targetVignette = _bossIsEnraged
                ? 0.4f + _wrathIntensity * 0.3f
                : 0.15f + _wrathIntensity * 0.2f;
            _vignetteStrength += (targetVignette - _vignetteStrength) * 0.03f;

            // Update ash particles
            if (_ash != null)
            {
                float wrathSpeed = 1f + _wrathIntensity * 0.8f;
                for (int i = 0; i < MaxAsh; i++)
                {
                    _ash[i].Position.Y += _ash[i].Speed * wrathSpeed;
                    _ash[i].Position.X += _ash[i].Drift + (float)Math.Sin(Main.timeForVisualEffects * 0.01f + i * 0.5f) * 0.3f;
                    _ash[i].Rotation += _ash[i].RotSpeed;

                    if (_ash[i].Position.Y > Main.screenHeight + 20)
                    {
                        _ash[i].Position.Y = -20;
                        _ash[i].Position.X = Main.rand.NextFloat() * Main.screenWidth;
                    }
                    if (_ash[i].Position.X < -20)
                        _ash[i].Position.X = Main.screenWidth + 20;
                    if (_ash[i].Position.X > Main.screenWidth + 20)
                        _ash[i].Position.X = -20;
                }
            }

            // Lightning flashes - more frequent at high wrath, even more when enraged
            _lightningBrightness = Math.Max(0f, _lightningBrightness - _lightningDecay);
            _lightningTimer += 1f;
            float flashChance = _bossIsEnraged
                ? 60f - _wrathIntensity * 40f
                : 200f - _wrathIntensity * 150f;
            flashChance = Math.Max(10f, flashChance);

            if (_lightningTimer > flashChance && Main.rand.NextBool(Math.Max(1, (int)(flashChance * 0.5f))))
            {
                _lightningBrightness = 0.3f + _wrathIntensity * 0.5f;
                if (_bossIsEnraged)
                    _lightningBrightness += 0.15f;
                _lightningDecay = 0.02f + _wrathIntensity * 0.03f;
                _lightningTimer = 0f;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth >= 0f && minDepth < 0f)
            {
                Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
                float effectiveIntensity = GetEffectiveIntensity();
                float hpDrive = 1f - _bossLifeRatio;

                // Sky gradient - blood red overhead darkening to ashen black
                Color topColor = Color.Lerp(new Color(60, 10, 8), new Color(30, 5, 5), _wrathIntensity);
                Color bottomColor = Color.Lerp(new Color(25, 10, 5), new Color(15, 5, 3), _wrathIntensity);
                topColor *= _opacity * 0.85f * effectiveIntensity;
                bottomColor *= _opacity * 0.85f * effectiveIntensity;

                for (int y = 0; y < Main.screenHeight; y += 4)
                {
                    float t = (float)y / Main.screenHeight;
                    Color lineColor = Color.Lerp(topColor, bottomColor, t);
                    spriteBatch.Draw(pixel, new Rectangle(0, y, Main.screenWidth, 4), lineColor);
                }

                // Lightning flash overlay
                if (_lightningBrightness > 0.01f && _opacity > 0.1f)
                {
                    Color flashColor = Color.Lerp(
                        new Color(200, 50, 30),
                        new Color(255, 180, 50), _wrathIntensity * 0.4f)
                        * (_lightningBrightness * _opacity * effectiveIntensity);
                    flashColor.A = 0;
                    spriteBatch.Draw(pixel,
                        new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
                        flashColor);
                }

                // Falling ash - HP drives density & glow
                if (_ash != null && _opacity > 0.1f)
                {
                    float ashVisibility = 0.25f + hpDrive * 0.5f;

                    for (int i = 0; i < MaxAsh; i++)
                    {
                        float ashAlpha = _opacity * ashVisibility * effectiveIntensity;
                        Color ashColor = Color.Lerp(
                            new Color(80, 60, 50),
                            new Color(140, 40, 15), _wrathIntensity) * ashAlpha;

                        // Embers glow at high wrath - more of them glow as HP drops
                        bool isEmber = _wrathIntensity > 0.3f && i % Math.Max(1, 5 - (int)(hpDrive * 3f)) == 0;
                        if (isEmber)
                        {
                            float glowPulse = (float)Math.Sin(Main.timeForVisualEffects * 0.04f + i * 0.7f) * 0.5f + 0.5f;
                            ashColor = Color.Lerp(ashColor, new Color(220, 100, 30) * ashAlpha, glowPulse * _wrathIntensity);
                            ashColor.A = 0;
                        }

                        float s = _ash[i].Scale * (1f + _wrathIntensity * 0.4f);
                        spriteBatch.Draw(pixel,
                            _ash[i].Position,
                            new Rectangle(0, 0, 1, 1),
                            ashColor,
                            _ash[i].Rotation,
                            new Vector2(0.5f),
                            s * 2.5f,
                            SpriteEffects.None, 0f);
                    }
                }

                // Hellfire horizon glow - intensifies with wrath
                if (_wrathIntensity > 0.15f && _opacity > 0.1f)
                {
                    float glowAlpha = _wrathIntensity * _opacity * effectiveIntensity * 0.25f;
                    float pulse = (float)Math.Sin(Main.timeForVisualEffects * 0.008f) * 0.3f + 0.7f;
                    Color glowColor = Color.Lerp(
                        new Color(200, 30, 20),
                        new Color(255, 120, 30), _wrathIntensity * 0.5f) * (glowAlpha * pulse);
                    glowColor.A = 0;

                    int horizonY = (int)(Main.screenHeight * 0.65f);
                    for (int y = horizonY; y < Main.screenHeight; y += 4)
                    {
                        float t = (float)(y - horizonY) / (Main.screenHeight - horizonY);
                        Color lineGlow = glowColor * (1f - t * 0.4f);
                        spriteBatch.Draw(pixel, new Rectangle(0, y, Main.screenWidth, 4), lineGlow);
                    }
                }

                // === VIGNETTE - Blood-red darkening from edges ===
                if (_vignetteStrength > 0.01f && _opacity > 0.1f)
                {
                    float vigAlpha = _vignetteStrength * _opacity * effectiveIntensity;
                    Color vigBase = _bossIsEnraged
                        ? Color.Lerp(new Color(60, 5, 5), new Color(100, 15, 5), _wrathIntensity)
                        : Color.Lerp(new Color(30, 5, 5), new Color(60, 10, 5), _wrathIntensity);

                    int rings = _bossIsEnraged ? 14 : 10;
                    for (int ring = 0; ring < rings; ring++)
                    {
                        float ringT = (float)ring / rings;
                        float edgeDist = 1f - ringT;
                        float ringAlpha = edgeDist * edgeDist * vigAlpha;

                        Color ringColor = vigBase * ringAlpha;

                        int thickness = Math.Max(1, (int)(Main.screenHeight * 0.06f * edgeDist));

                        // Top edge
                        spriteBatch.Draw(pixel, new Rectangle(0, ring * thickness / 2, Main.screenWidth, thickness), ringColor);
                        // Bottom edge
                        spriteBatch.Draw(pixel, new Rectangle(0, Main.screenHeight - ring * thickness / 2 - thickness, Main.screenWidth, thickness), ringColor);
                        // Left edge
                        spriteBatch.Draw(pixel, new Rectangle(ring * thickness / 2, 0, thickness, Main.screenHeight), ringColor);
                        // Right edge
                        spriteBatch.Draw(pixel, new Rectangle(Main.screenWidth - ring * thickness / 2 - thickness, 0, thickness, Main.screenHeight), ringColor);
                    }
                }
            }
        }

        public override Color OnTileColor(Color inColor)
        {
            float effectiveIntensity = GetEffectiveIntensity();

            // Stronger tint: enraged pushes deeper into crimson
            float tintStrength = _bossIsEnraged ? 0.45f : 0.35f;
            tintStrength *= effectiveIntensity;

            Color tint = Color.Lerp(Color.White,
                Color.Lerp(new Color(200, 140, 120), new Color(160, 70, 50), _wrathIntensity),
                _opacity * tintStrength);
            return inColor.MultiplyRGBA(tint);
        }

        public override float GetCloudAlpha() => 1f - _opacity * 0.9f;
    }

    /// <summary>
    /// Companion system for DiesIraeSky. Provides static flash APIs
    /// for attacks and ambient hellfire world particles.
    /// </summary>
    public class DiesIraeSkySystem : ModSystem
    {
        // Flash state
        private static float _flashIntensity;
        private static Color _flashColor;
        private static float _flashDecay;

        /// <summary>Crimson lightning flash for standard fire attacks.</summary>
        public static void TriggerHellfireFlash(float intensity)
        {
            _flashIntensity = Math.Max(_flashIntensity, intensity);
            _flashColor = new Color(200, 30, 20);
            _flashDecay = 0.92f;
        }

        /// <summary>Ember-orange flash for wrath escalation moments.</summary>
        public static void TriggerWrathFlash(float intensity)
        {
            _flashIntensity = Math.Max(_flashIntensity, intensity);
            _flashColor = new Color(255, 100, 30);
            _flashDecay = 0.90f;
        }

        /// <summary>Blinding white-gold flash for judgment beams and ultimates.</summary>
        public static void TriggerJudgmentFlash(float intensity)
        {
            _flashIntensity = Math.Max(_flashIntensity, intensity);
            _flashColor = new Color(255, 220, 180);
            _flashDecay = 0.88f;
        }

        /// <summary>Maximum white flash for the final apocalypse moment.</summary>
        public static void TriggerApocalypseFlash(float intensity)
        {
            _flashIntensity = Math.Max(_flashIntensity, intensity);
            _flashColor = Color.White;
            _flashDecay = 0.85f;
        }

        public static (float intensity, Color color) GetFlashState() => (_flashIntensity, _flashColor);

        public override void PostUpdateEverything()
        {
            // Decay flash
            _flashIntensity *= _flashDecay;
            if (_flashIntensity < 0.01f)
                _flashIntensity = 0f;

            // Ambient hellfire world particles near boss
            if (Main.netMode == NetmodeID.Server) return;

            NPC boss = BossIndexTracker.GetActiveBoss(BossIndexTracker.DiesIraeHerald);
            if (boss == null || !boss.active) return;

            float hpDrive = 1f - (boss.life / (float)boss.lifeMax);

            // Ember dust rising from the ground near boss
            if (Main.rand.NextBool(Math.Max(1, (int)(8 - hpDrive * 5))))
            {
                Vector2 dustPos = boss.Center + new Vector2(Main.rand.NextFloat(-300f, 300f), Main.rand.NextFloat(50f, 200f));
                Dust d = Dust.NewDustDirect(dustPos, 4, 4, DustID.Torch, 0f, -Main.rand.NextFloat(1f, 3f), 0, default, 1.2f + hpDrive * 0.8f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // Smoke wisps at high wrath
            if (hpDrive > 0.4f && Main.rand.NextBool(Math.Max(1, (int)(12 - hpDrive * 8))))
            {
                Vector2 smokePos = boss.Center + Main.rand.NextVector2Circular(200f, 200f);
                Dust s = Dust.NewDustDirect(smokePos, 4, 4, DustID.Smoke, Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(0.5f, 2f), 100, default, 1.5f);
                s.noGravity = true;
            }
        }
    }
}