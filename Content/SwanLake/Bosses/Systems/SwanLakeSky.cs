using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.SwanLake.Bosses.Systems
{
    /// <summary>
    /// Custom sky effect for the Swan Lake boss fight.
    /// Transitions between three moods:
    ///   Graceful: elegant silver moonlight, drifting feather motes
    ///   Tempest: stormy prismatic lightning flashes, swirling clouds
    ///   DyingSwan: monochrome fading to white, final rainbow shatter
    /// Uses BossIndexTracker.SwanLakeMood to determine current mood.
    /// </summary>
    public class SwanLakeSky : CustomSky
    {
        private bool _isActive;
        private float _opacity;
        private int _currentMood; // 0=Graceful, 1=Tempest, 2=DyingSwan
        private float _moodBlend; // 0-1 transition progress
        private float _intensity;

        // Boss state tracking
        private float _bossLifeRatio = 1f;
        private Vector2 _bossCenter;
        private bool _bossIsDyingSwan;

        private struct SkyFeather
        {
            public Vector2 Position;
            public float Speed;
            public float Scale;
            public float Rotation;
            public float RotSpeed;
            public float SwayPhase;
            public bool IsBlack; // black vs white feather
        }

        private SkyFeather[] _feathers;
        private const int MaxFeathers = 50;

        // Lightning flash for Tempest mood
        private float _lightningTimer;
        private float _lightningIntensity;

        // Vignette
        private float _vignetteStrength;

        /// <summary>
        /// Updates boss state for HP-driven effects. Called from companion ModSystem.
        /// </summary>
        public void UpdateBossState(float lifeRatio, Vector2 center, bool isDyingSwan)
        {
            _bossLifeRatio = lifeRatio;
            _bossCenter = center;
            _bossIsDyingSwan = isDyingSwan;
        }

        /// <summary>
        /// Distance-based intensity falloff. Full intensity near boss, fading to 0.3 at 3000px away.
        /// </summary>
        private float GetEffectiveIntensity()
        {
            if (_bossCenter == Vector2.Zero) return _opacity;
            float dist = Vector2.Distance(Main.LocalPlayer.Center, _bossCenter);
            float distFade = MathHelper.SmoothStep(1f, 0.3f, MathHelper.Clamp(dist / 3000f, 0f, 1f));
            return _opacity * distFade;
        }

        public override void OnLoad() { }

        public override void Activate(Vector2 position, params object[] args)
        {
            _isActive = true;
            _feathers = new SkyFeather[MaxFeathers];
            Random rand = new Random();

            for (int i = 0; i < MaxFeathers; i++)
            {
                _feathers[i] = new SkyFeather
                {
                    Position = new Vector2(
                        (float)rand.NextDouble() * Main.screenWidth,
                        (float)rand.NextDouble() * Main.screenHeight),
                    Speed = 0.2f + (float)rand.NextDouble() * 0.8f,
                    Scale = 0.3f + (float)rand.NextDouble() * 0.5f,
                    Rotation = (float)rand.NextDouble() * MathHelper.TwoPi,
                    RotSpeed = ((float)rand.NextDouble() - 0.5f) * 0.02f,
                    SwayPhase = (float)rand.NextDouble() * MathHelper.TwoPi,
                    IsBlack = rand.NextDouble() > 0.7 // 30% black feathers
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

        public override void Update(GameTime gameTime)
        {
            if (_isActive)
                _opacity = Math.Min(1f, _opacity + 0.02f);
            else
                _opacity = Math.Max(0f, _opacity - 0.02f);

            NPC boss = BossIndexTracker.GetActiveBoss(BossIndexTracker.SwanLakeFractal);
            if (boss != null)
            {
                float hpRatio = boss.life / (float)boss.lifeMax;
                _intensity = 1f - hpRatio;

                // Track mood transitions
                int targetMood = BossIndexTracker.SwanLakeMood;
                if (targetMood != _currentMood)
                {
                    _moodBlend += 0.01f;
                    if (_moodBlend >= 1f)
                    {
                        _currentMood = targetMood;
                        _moodBlend = 0f;
                    }
                }

                // Tempest lightning flashes — more frequent at lower HP
                if (_currentMood == 1)
                {
                    _lightningTimer -= 1f;
                    float lightningMinCooldown = MathHelper.Lerp(30f, 10f, _intensity);
                    float lightningMaxCooldown = MathHelper.Lerp(90f, 40f, _intensity);
                    if (_lightningTimer <= 0)
                    {
                        _lightningIntensity = 0.5f + Main.rand.NextFloat(0.5f);
                        _lightningTimer = lightningMinCooldown + Main.rand.NextFloat(lightningMaxCooldown - lightningMinCooldown);
                    }
                    _lightningIntensity *= 0.92f;
                }
                else
                {
                    _lightningIntensity *= 0.95f;
                }

                // Vignette — builds through fight, strongest in DyingSwan
                float targetVignette = _currentMood switch
                {
                    0 => 0.15f + _intensity * 0.15f,
                    1 => 0.35f + _intensity * 0.2f,
                    _ => 0.5f + _intensity * 0.3f
                };
                _vignetteStrength = MathHelper.Lerp(_vignetteStrength, targetVignette, 0.02f);

                // Update feather drift — HP-driven speed multiplier
                if (_feathers != null)
                {
                    float speedMult = _currentMood == 1 ? 2.5f : (_currentMood == 2 ? 0.5f : 1f);
                    speedMult *= MathHelper.Lerp(1f, 1.6f, _intensity);
                    for (int i = 0; i < MaxFeathers; i++)
                    {
                        _feathers[i].Position.Y += _feathers[i].Speed * speedMult;
                        float swaySpeed = MathHelper.Lerp(0.008f, 0.016f, _intensity);
                        _feathers[i].Position.X += (float)Math.Sin(Main.timeForVisualEffects * swaySpeed + _feathers[i].SwayPhase) * 0.5f * speedMult;
                        _feathers[i].Rotation += _feathers[i].RotSpeed * speedMult;

                        if (_feathers[i].Position.Y > Main.screenHeight + 20)
                        {
                            _feathers[i].Position.Y = -20;
                            _feathers[i].Position.X = Main.rand.NextFloat() * Main.screenWidth;
                        }
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth >= 0f && minDepth < 0f)
            {
                Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;

                // Mood-dependent sky colors
                Color topColor, bottomColor;

                switch (_currentMood)
                {
                    case 0: // Graceful: silver moonlight
                        topColor = Color.Lerp(new Color(20, 20, 35), new Color(40, 40, 60), _intensity);
                        bottomColor = Color.Lerp(new Color(50, 55, 70), new Color(100, 105, 130), _intensity);
                        break;
                    case 1: // Tempest: stormy dark
                        topColor = Color.Lerp(new Color(15, 15, 25), new Color(30, 20, 40), _intensity);
                        bottomColor = Color.Lerp(new Color(30, 30, 45), new Color(60, 50, 80), _intensity);
                        break;
                    default: // DyingSwan: fading to white
                        float fade = _intensity * 0.5f;
                        topColor = Color.Lerp(new Color(40, 40, 50), new Color(180, 180, 190), fade);
                        bottomColor = Color.Lerp(new Color(60, 60, 70), new Color(220, 220, 230), fade);
                        break;
                }

                topColor *= _opacity * 0.7f;
                bottomColor *= _opacity * 0.7f;

                // Draw gradient
                for (int y = 0; y < Main.screenHeight; y += 4)
                {
                    float t = (float)y / Main.screenHeight;
                    Color lineColor = Color.Lerp(topColor, bottomColor, t);
                    spriteBatch.Draw(pixel, new Rectangle(0, y, Main.screenWidth, 4), lineColor);
                }

                // Lightning flash overlay (Tempest)
                if (_lightningIntensity > 0.01f)
                {
                    float hue = ((float)Main.timeForVisualEffects * 0.01f) % 1f;
                    Color flashColor = Main.hslToRgb(hue, 0.3f, 0.9f) * _opacity * _lightningIntensity * 0.3f;
                    flashColor.A = 0;
                    spriteBatch.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), flashColor);
                }

                // DyingSwan white fade overlay
                if (_currentMood == 2 && _intensity > 0.5f)
                {
                    float whiteFade = (_intensity - 0.5f) * 2f * 0.15f;
                    Color whiteOverlay = Color.White * _opacity * whiteFade;
                    whiteOverlay.A = (byte)(whiteOverlay.A * 0.5f);
                    spriteBatch.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), whiteOverlay);
                }

                // Draw feather particles — more feathers appear at lower HP
                if (_feathers != null && _opacity > 0.1f)
                {
                    int visibleFeathers = (int)MathHelper.Lerp(MaxFeathers * 0.5f, MaxFeathers, _intensity);
                    for (int i = 0; i < visibleFeathers; i++)
                    {
                        Color c;
                        if (_currentMood == 2)
                        {
                            // DyingSwan: feathers fade to grey with faint prismatic shimmer
                            float prismatic = (float)Math.Sin(Main.timeForVisualEffects * 0.02f + i * 0.4f) * 0.1f;
                            c = Color.Lerp(new Color(150, 150, 155), new Color(200, 210, 240), Math.Max(0f, prismatic)) * _opacity * 0.5f;
                        }
                        else
                        {
                            c = _feathers[i].IsBlack
                                ? new Color(20, 20, 25) * _opacity * 0.5f
                                : new Color(230, 230, 240) * _opacity * 0.4f;
                        }
                        c.A = 0;
                        float s = _feathers[i].Scale * (1f + _intensity * 0.4f);
                        spriteBatch.Draw(pixel,
                            _feathers[i].Position,
                            new Rectangle(0, 0, 1, 1),
                            c,
                            _feathers[i].Rotation,
                            new Vector2(0.5f),
                            new Vector2(s * 2f, s * 4f), // Elongated like feathers
                            SpriteEffects.None, 0f);
                    }
                }

                // Vignette — darkens edges, intensifies through fight
                if (_vignetteStrength > 0.01f)
                {
                    float effectiveVignette = _vignetteStrength * GetEffectiveIntensity();
                    float vignetteSize = MathHelper.Lerp(400f, 550f, _intensity);
                    Vector2 screenCenter = new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f;

                    for (int ring = 0; ring < 12; ring++)
                    {
                        float ringT = ring / 12f;
                        float ringRadius = vignetteSize + ring * 60f;
                        float ringAlpha = ringT * ringT * effectiveVignette * 0.6f;

                        Color vignetteColor = _currentMood == 2
                            ? Color.Lerp(new Color(10, 10, 15), new Color(40, 40, 50), ringT) * ringAlpha
                            : new Color(5, 5, 10) * ringAlpha;

                        // Draw as 4-side edge rectangles
                        int thickness = 60 + ring * 40;
                        spriteBatch.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, thickness), vignetteColor);
                        spriteBatch.Draw(pixel, new Rectangle(0, Main.screenHeight - thickness, Main.screenWidth, thickness), vignetteColor);
                        spriteBatch.Draw(pixel, new Rectangle(0, 0, thickness, Main.screenHeight), vignetteColor);
                        spriteBatch.Draw(pixel, new Rectangle(Main.screenWidth - thickness, 0, thickness, Main.screenHeight), vignetteColor);
                    }
                }
            }
        }

        public override Color OnTileColor(Color inColor)
        {
            float eff = GetEffectiveIntensity();
            Color tint = _currentMood switch
            {
                // Graceful: cool silver moonlight wash
                0 => Color.Lerp(Color.White, new Color(190, 195, 220), eff * 0.45f),
                // Tempest: darker, stormy desaturation
                1 => Color.Lerp(Color.White, new Color(170, 170, 195), eff * 0.5f),
                // DyingSwan: fading toward monochrome white
                _ => Color.Lerp(Color.White, new Color(210, 210, 215), eff * 0.4f)
            };
            return inColor.MultiplyRGBA(tint);
        }

        public override float GetCloudAlpha() => 1f - _opacity * (_currentMood == 1 ? 0.8f : 0.5f);
    }

    /// <summary>
    /// Companion ModSystem for the Swan Lake sky. 
    /// Feeds boss state, provides screen flash APIs, and spawns ambient world particles.
    /// </summary>
    public class SwanLakeSkySystem : ModSystem
    {
        // Flash system
        private static float _flashTimer;
        private static float _flashDuration;
        private static Color _flashColor;

        // Boss state (set by boss AI loop)
        public static float BossLifeRatio { get; set; } = 1f;
        public static Vector2 BossCenter { get; set; }
        public static bool BossIsDyingSwan { get; set; }

        public static void TriggerWhiteFlash(float duration = 12f)
        {
            _flashTimer = duration;
            _flashDuration = duration;
            _flashColor = new Color(240, 240, 255);
        }

        public static void TriggerPrismaticFlash(float duration = 10f)
        {
            _flashTimer = duration;
            _flashDuration = duration;
            _flashColor = new Color(220, 230, 255);
        }

        public static void TriggerMonochromeFlash(float duration = 15f)
        {
            _flashTimer = duration;
            _flashDuration = duration;
            _flashColor = new Color(180, 180, 190);
        }

        public static void TriggerDeathFlash(float duration = 20f)
        {
            _flashTimer = duration;
            _flashDuration = duration;
            _flashColor = Color.White;
        }

        public override void PostUpdateEverything()
        {
            // Feed boss state to sky
            if (SkyManager.Instance["MagnumOpus:SwanLakeSky"] is SwanLakeSky sky)
            {
                sky.UpdateBossState(BossLifeRatio, BossCenter, BossIsDyingSwan);
            }

            // Flash timer
            if (_flashTimer > 0)
                _flashTimer--;

            // Ambient feather dust near boss
            if (BossCenter != Vector2.Zero && BossLifeRatio < 1f)
            {
                float hpDrive = 1f - BossLifeRatio;

                // Drifting white feather motes
                int featherChance = Math.Max(1, (int)MathHelper.Lerp(5, 2, hpDrive));
                if (Main.rand.NextBool(featherChance))
                {
                    Vector2 pos = BossCenter + Main.rand.NextVector2Circular(300f, 300f);
                    Dust d = Dust.NewDustPerfect(pos, Terraria.ID.DustID.SilverCoin, Main.rand.NextVector2Circular(0.5f, -1.5f) + new Vector2(0f, -0.5f), 120, Color.White, 0.6f);
                    d.noGravity = true;
                    d.fadeIn = 1.2f;
                }

                // DyingSwan: faint prismatic sparkle dust
                if (BossIsDyingSwan && Main.rand.NextBool(3))
                {
                    Vector2 pos = BossCenter + Main.rand.NextVector2Circular(200f, 200f);
                    float hue = Main.rand.NextFloat();
                    Color sparkleColor = Main.hslToRgb(hue, 0.6f, 0.9f);
                    Dust d = Dust.NewDustPerfect(pos, Terraria.ID.DustID.RainbowMk2, Main.rand.NextVector2Circular(1f, 1f), 80, sparkleColor, 0.5f);
                    d.noGravity = true;
                    d.fadeIn = 1f;
                }
            }
        }

        public override void ModifyScreenPosition()
        {
            // Screen flash rendering via UI
            if (_flashTimer > 0 && _flashDuration > 0)
            {
                float progress = _flashTimer / _flashDuration;
                float alpha = progress * progress * 0.25f;
                Terraria.Graphics.Effects.Filters.Scene.Activate("HeatDistortion", Vector2.Zero);
            }
        }

        /// <summary>
        /// Returns current flash info for additive overlay rendering by other systems.
        /// </summary>
        public static (Color color, float alpha) GetFlashState()
        {
            if (_flashTimer <= 0 || _flashDuration <= 0)
                return (Color.Transparent, 0f);
            float progress = _flashTimer / _flashDuration;
            return (_flashColor, progress * progress * 0.3f);
        }
    }
}
