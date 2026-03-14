using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.Nachtmusik.Bosses.Systems
{
    /// <summary>
    /// Enhanced sky overlay for the Nachtmusik boss fight — 4 phases of nocturnal wonder.
    /// Phase 1: Planetarium — serene starlit dome, gentle indigo vignette.
    /// Phase 2: Cosmic Dance — rotating starfield, nebula tint intensifies.
    /// Phase 3: Celestial Crescendo — galaxy spiral visible, deep vignette.
    /// Phase 4: Supernova — blinding silver-white wash, aurora overlay.
    /// HP-driven intensity scaling on stars, vignette, tile tint.
    /// </summary>
    public class NachtmusikSkyEnhanced : CustomSky
    {
        private bool _isActive;
        private float _opacity;
        private float _phaseBlend; // 0-1 blending to current phase visual state

        // Boss state fed from NachtmusikSkySystem
        private float _bossLifeRatio = 1f;
        private Vector2 _bossCenter;
        private int _bossPhase = 1;
        private float _vignetteStrength;

        private struct NightStar
        {
            public Vector2 Position;
            public float Brightness;
            public float PulseSpeed;
            public float PulseOffset;
            public float Scale;
            public Color Tint;
        }

        private NightStar[] _stars;
        private const int MaxStars = 80;

        private struct ShootingStar
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Life;
            public float MaxLife;
            public Color Color;
        }

        private ShootingStar[] _shootingStars;
        private const int MaxShootingStars = 10;

        public override void OnLoad() { }

        public override void Activate(Vector2 position, params object[] args)
        {
            _isActive = true;
            Random rand = new Random();
            _stars = new NightStar[MaxStars];
            _shootingStars = new ShootingStar[MaxShootingStars];

            for (int i = 0; i < MaxStars; i++)
            {
                // Silver/blue palette — no gold
                Color tint = Color.Lerp(new Color(200, 215, 240), new Color(60, 100, 190),
                    (float)rand.NextDouble() * 0.4f);

                _stars[i] = new NightStar
                {
                    Position = new Vector2(
                        (float)rand.NextDouble() * Main.screenWidth,
                        (float)rand.NextDouble() * Main.screenHeight),
                    Brightness = 0.2f + (float)rand.NextDouble() * 0.8f,
                    PulseSpeed = 0.008f + (float)rand.NextDouble() * 0.02f,
                    PulseOffset = (float)rand.NextDouble() * MathHelper.TwoPi,
                    Scale = 0.2f + (float)rand.NextDouble() * 0.7f,
                    Tint = tint
                };
            }

            for (int i = 0; i < MaxShootingStars; i++)
                ResetShootingStar(ref _shootingStars[i], rand);
        }

        private void ResetShootingStar(ref ShootingStar star, Random rand)
        {
            star.Position = new Vector2(
                (float)rand.NextDouble() * Main.screenWidth,
                (float)rand.NextDouble() * Main.screenHeight * 0.5f);
            float angle = MathHelper.ToRadians(120f + (float)rand.NextDouble() * 60f);
            // Phase 3+ = faster shooting stars
            float phaseSpeed = _bossPhase >= 3 ? 6f : 0f;
            float speed = 2f + (float)rand.NextDouble() * 4f + phaseSpeed;
            star.Velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
            star.MaxLife = 30f + (float)rand.NextDouble() * 40f;
            star.Life = star.MaxLife;
            // Phase 4 = brighter shooting stars
            star.Color = _bossPhase >= 4
                ? Color.Lerp(new Color(245, 245, 255), new Color(200, 215, 240), (float)rand.NextDouble())
                : Color.Lerp(new Color(200, 215, 240), new Color(60, 100, 190), (float)rand.NextDouble() * 0.5f);
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
            if (_bossCenter == Vector2.Zero) return 1f;
            Vector2 screenCenter = Main.screenPosition + new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f;
            float dist = Vector2.Distance(screenCenter, _bossCenter);
            return MathHelper.SmoothStep(1f, 0.25f, MathHelper.Clamp(dist / 3500f, 0f, 1f));
        }

        private void UpdateBossState()
        {
            _bossLifeRatio = NachtmusikSkySystem.BossLifeRatio;
            _bossCenter = NachtmusikSkySystem.BossCenter;
            _bossPhase = NachtmusikSkySystem.BossPhase;
        }

        public override void Update(GameTime gameTime)
        {
            if (_isActive)
                _opacity = Math.Min(1f, _opacity + 0.02f);
            else
                _opacity = Math.Max(0f, _opacity - 0.02f);

            UpdateBossState();

            // Phase blend tracks current phase
            float targetBlend = (_bossPhase - 1) / 3f; // 0 for P1, 0.33 P2, 0.66 P3, 1.0 P4
            _phaseBlend += (targetBlend - _phaseBlend) * 0.02f;

            float hpDrive = 1f - _bossLifeRatio;

            // Vignette: escalates per phase
            float targetVignette = _bossPhase switch
            {
                4 => 0.1f + hpDrive * 0.15f,   // Supernova — lighter (sky is bright)
                3 => 0.3f + hpDrive * 0.2f,     // Crescendo — deep
                2 => 0.2f + hpDrive * 0.15f,    // Cosmic Dance
                _ => _opacity > 0.1f ? 0.12f + hpDrive * 0.1f : 0f  // Evening Star — subtle
            };
            _vignetteStrength += (targetVignette - _vignetteStrength) * 0.03f;

            // Update stars
            if (_stars != null)
            {
                float pulseMult = 1f + hpDrive * 0.6f + (_bossPhase >= 4 ? 0.5f : 0f);
                for (int i = 0; i < MaxStars; i++)
                {
                    float pulse = (float)Math.Sin(Main.timeForVisualEffects * _stars[i].PulseSpeed * pulseMult + _stars[i].PulseOffset);
                    _stars[i].Brightness = 0.2f + (pulse * 0.3f + 0.3f) + _phaseBlend * 0.2f + hpDrive * 0.2f;

                    // Phase 3+: stars shimmer more rapidly
                    if (_bossPhase >= 3)
                    {
                        float flicker = (float)Math.Sin(Main.timeForVisualEffects * 0.15f + i * 1.3f);
                        _stars[i].Brightness *= 0.75f + flicker * 0.25f;
                    }
                }
            }

            // Update shooting stars
            if (_shootingStars != null)
            {
                Random rand = new Random((int)(Main.timeForVisualEffects * 0.01f) + 42);
                for (int i = 0; i < MaxShootingStars; i++)
                {
                    _shootingStars[i].Position += _shootingStars[i].Velocity;
                    _shootingStars[i].Life -= 1f;
                    if (_shootingStars[i].Life <= 0 ||
                        _shootingStars[i].Position.X < -50 || _shootingStars[i].Position.X > Main.screenWidth + 50 ||
                        _shootingStars[i].Position.Y > Main.screenHeight + 50)
                    {
                        ResetShootingStar(ref _shootingStars[i], rand);
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth >= 0f && minDepth < 0f)
            {
                Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
                float hpDrive = 1f - _bossLifeRatio;

                // Sky gradient — phase-driven palette
                Color topColor, bottomColor;
                switch (_bossPhase)
                {
                    case 4: // Supernova — washed silver-white
                        float pulse4 = (float)Math.Sin(Main.timeForVisualEffects * 0.003f) * 0.1f;
                        topColor = Color.Lerp(new Color(15, 12, 40), new Color(100, 105, 130), 0.3f + pulse4);
                        bottomColor = Color.Lerp(new Color(20, 15, 50), new Color(80, 80, 110), 0.2f + pulse4);
                        break;
                    case 3: // Celestial Crescendo — deeper void
                        topColor = Color.Lerp(new Color(8, 6, 22), new Color(20, 18, 50), _phaseBlend);
                        bottomColor = Color.Lerp(new Color(15, 12, 40), new Color(35, 25, 65), _phaseBlend);
                        break;
                    case 2: // Cosmic Dance — subtle nebula tint
                        topColor = Color.Lerp(new Color(8, 6, 22), new Color(12, 10, 32), _phaseBlend);
                        bottomColor = Color.Lerp(new Color(15, 12, 40), new Color(25, 20, 55), _phaseBlend);
                        break;
                    default: // Evening Star — deep dark indigo
                        topColor = new Color(8, 6, 22);
                        bottomColor = new Color(15, 12, 40);
                        break;
                }
                topColor *= _opacity * 0.8f;
                bottomColor *= _opacity * 0.8f;

                for (int y = 0; y < Main.screenHeight; y += 4)
                {
                    float t = (float)y / Main.screenHeight;
                    Color lineColor = Color.Lerp(topColor, bottomColor, t);
                    spriteBatch.Draw(pixel, new Rectangle(0, y, Main.screenWidth, 4), lineColor);
                }

                float alpha = _opacity;

                // Stars
                if (_stars != null && alpha > 0.1f)
                {
                    float visibleFraction = 0.6f + hpDrive * 0.4f;
                    int visibleCount = (int)(MaxStars * visibleFraction);
                    for (int i = 0; i < visibleCount; i++)
                    {
                        Color c = _stars[i].Tint * (alpha * _stars[i].Brightness);
                        c.A = 0;
                        float s = _stars[i].Scale * (1f + _phaseBlend * 0.3f);
                        spriteBatch.Draw(pixel, _stars[i].Position, new Rectangle(0, 0, 1, 1),
                            c, 0f, new Vector2(0.5f), s * 2.5f, SpriteEffects.None, 0f);
                    }
                }

                // Shooting stars
                if (_shootingStars != null && alpha > 0.1f)
                {
                    for (int i = 0; i < MaxShootingStars; i++)
                    {
                        float lifeRatio = _shootingStars[i].Life / _shootingStars[i].MaxLife;
                        if (lifeRatio <= 0) continue;
                        Color c = _shootingStars[i].Color * (alpha * lifeRatio * 0.8f);
                        c.A = 0;
                        Vector2 dir = _shootingStars[i].Velocity;
                        dir.Normalize();
                        float rot = (float)Math.Atan2(dir.Y, dir.X);
                        float tailLen = 8f + (1f - lifeRatio) * 15f + _phaseBlend * 10f;

                        spriteBatch.Draw(pixel, _shootingStars[i].Position, new Rectangle(0, 0, 1, 1),
                            c, rot, Vector2.Zero, new Vector2(tailLen, 1.5f), SpriteEffects.None, 0f);
                    }
                }

                // Vignette — indigo edges, lighter in supernova
                if (_vignetteStrength > 0.01f)
                {
                    Color vignetteColor = _bossPhase >= 4
                        ? Color.Lerp(new Color(15, 12, 40), new Color(70, 40, 120), hpDrive * 0.3f)
                        : new Color(15, 12, 40);
                    int steps = 12;
                    for (int ring = 0; ring < steps; ring++)
                    {
                        float t = (float)ring / steps;
                        float ringAlpha = t * t * _vignetteStrength * _opacity;
                        Color c = vignetteColor * ringAlpha;
                        int inset = (int)((1f - t) * Math.Min(Main.screenWidth, Main.screenHeight) * 0.45f);
                        var rect = new Rectangle(inset, inset, Main.screenWidth - inset * 2, Main.screenHeight - inset * 2);
                        if (rect.Width > 0 && rect.Height > 0)
                            spriteBatch.Draw(pixel, rect, c);
                    }
                }
            }
        }

        public override Color OnTileColor(Color inColor)
        {
            float eff = GetEffectiveIntensity();
            // Phase-driven tinting: deeper indigo in early phases, silver wash in supernova
            float strength = _bossPhase >= 4 ? 0.25f : (0.25f + _phaseBlend * 0.15f);
            Color tintTarget = _bossPhase >= 4
                ? Color.Lerp(new Color(180, 195, 220), new Color(220, 220, 240), _phaseBlend)
                : Color.Lerp(new Color(160, 175, 210), new Color(120, 130, 190), _phaseBlend);
            Color tint = Color.Lerp(Color.White, tintTarget, _opacity * strength * eff);
            return inColor.MultiplyRGBA(tint);
        }

        public override float GetCloudAlpha() => 1f - _opacity * 0.7f;
    }

    /// <summary>
    /// Companion ModSystem for NachtmusikSkyEnhanced.
    /// Feeds boss state to sky, provides flash APIs and ambient world particles.
    /// Updated for 4-phase system: Evening Star → Cosmic Dance → Crescendo → Supernova.
    /// </summary>
    public class NachtmusikSkySystem : ModSystem
    {
        // Boss state — written by boss AI, read by sky
        public static float BossLifeRatio { get; set; } = 1f;
        public static Vector2 BossCenter { get; set; }
        public static int BossPhase { get; set; } = 1;

        // Flash state
        private static Color _flashColor;
        private static float _flashIntensity;
        private static float _flashDecay;

        /// <summary>Soft silver flash — gentle starlight impacts.</summary>
        public static void TriggerStarlightFlash(float intensity = 8f)
        {
            _flashColor = new Color(200, 215, 240);
            _flashIntensity = intensity;
            _flashDecay = 0.92f;
        }

        /// <summary>Cosmic blue flash — nebula pulse, constellation fire.</summary>
        public static void TriggerCosmicFlash(float intensity = 10f)
        {
            _flashColor = new Color(60, 100, 190);
            _flashIntensity = intensity;
            _flashDecay = 0.90f;
        }

        /// <summary>Prismatic white flash — starlight beam refractions, crescendo attacks.</summary>
        public static void TriggerPrismaticFlash(float intensity = 12f)
        {
            _flashColor = new Color(220, 225, 245);
            _flashIntensity = intensity;
            _flashDecay = 0.88f;
        }

        /// <summary>Blinding supernova flash — the night explodes.</summary>
        public static void TriggerSupernovaFlash(float intensity = 18f)
        {
            _flashColor = Color.White;
            _flashIntensity = intensity;
            _flashDecay = 0.85f;
        }

        public static (Color color, float intensity) GetFlashState() => (_flashColor, _flashIntensity);

        public override void PostUpdateEverything()
        {
            // Decay flash
            if (_flashIntensity > 0.05f)
                _flashIntensity *= _flashDecay;
            else
                _flashIntensity = 0f;

            // Ambient world particles near boss
            if (BossCenter != Vector2.Zero && !Main.dedServ && _flashIntensity > 1f)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 pos = BossCenter + Main.rand.NextVector2Circular(250f, 250f);
                    // Silver starlight dust, not gold
                    Color dustColor = BossPhase >= 4 ? new Color(245, 245, 255) : new Color(200, 215, 240);
                    int dustType = BossPhase >= 4 ? DustID.SilverCoin : DustID.BlueTorch;
                    var d = Dust.NewDustDirect(pos, 1, 1, dustType, 0f, -1f, 150, dustColor, 0.8f);
                    d.noGravity = true;
                    d.velocity *= 0.3f;
                }
            }
        }
    }
}
