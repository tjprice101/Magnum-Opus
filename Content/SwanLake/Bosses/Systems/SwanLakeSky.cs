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
    /// Custom sky for the Monochromatic Fractal boss fight — Swan Lake's dying grace.
    /// 
    /// Phase 1 (White Swan): Stark void-black sky with precisely drifting white feathers.
    ///          Clean, geometric. Moonlit silence. White-on-black contrast.
    /// Phase 2 (Black Swan): Sky begins cracking — white fracture lines appear.
    ///          Black feathers emerge mirroring white. Faint prismatic bleed at cracks.
    ///          Sky fragments begin to form kaleidoscope geometry.
    /// Phase 3 (Duality War): Full kaleidoscope fractal pattern — sky is tiled/mirrored
    ///          geometric fragments rotating slowly. Black and white alternate in panels.
    ///          The fractal rotates and fragments.
    /// Enrage (Death of Swan): Color drains from sky. Feathers slow to near-stillness.
    ///          Kaleidoscope dims. Only faint desperate prismatic flickers survive.
    /// </summary>
    public class SwanLakeSky : CustomSky
    {
        private bool _isActive;
        private float _opacity;
        private int _currentMood; // 0=Graceful, 1=Tempest, 2=DyingSwan
        private float _intensity;

        // Boss state tracking
        private float _bossLifeRatio = 1f;
        private Vector2 _bossCenter;

        // Feather particles
        private struct SkyFeather
        {
            public Vector2 Position;
            public float Speed;
            public float Scale;
            public float Rotation;
            public float RotSpeed;
            public float SwayPhase;
            public bool IsBlack;
        }

        private SkyFeather[] _feathers;
        private const int MaxFeathers = 60;

        // Fractal/kaleidoscope state
        private float _fractalCrackProgress; // 0=none, 1=full kaleidoscope
        private float _kaleidoscopeRotation;
        private float _colorDrain; // 0=full color, 1=completely drained

        // Vignette
        private float _vignetteStrength;

        public void UpdateBossState(float lifeRatio, Vector2 center, bool isDyingSwan)
        {
            _bossLifeRatio = lifeRatio;
            _bossCenter = center;
        }

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
                    Speed = 0.15f + (float)rand.NextDouble() * 0.6f,
                    Scale = 0.3f + (float)rand.NextDouble() * 0.4f,
                    Rotation = (float)rand.NextDouble() * MathHelper.TwoPi,
                    RotSpeed = ((float)rand.NextDouble() - 0.5f) * 0.015f,
                    SwayPhase = (float)rand.NextDouble() * MathHelper.TwoPi,
                    IsBlack = false // Phase 1: all white. Black emerge in Phase 2.
                };
            }
        }

        public override void Deactivate(params object[] args) => _isActive = false;
        public override void Reset() { _isActive = false; _opacity = 0f; }
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

                int targetMood = BossIndexTracker.SwanLakeMood;
                _currentMood = targetMood;

                // Phase-driven fractal crack progression
                // Phase 1 (>60%): 0 cracks. Phase 2 (60-45%): cracks emerge 0->0.4.
                // Phase 3 (45-30%): full kaleidoscope 0.4->1.0. Enrage (<30%): holds at 1.0 but dims.
                float targetCrack;
                if (hpRatio > 0.6f)
                    targetCrack = 0f;
                else if (hpRatio > 0.45f)
                    targetCrack = MathHelper.Lerp(0f, 0.4f, (0.6f - hpRatio) / 0.15f);
                else if (hpRatio > 0.3f)
                    targetCrack = MathHelper.Lerp(0.4f, 1f, (0.45f - hpRatio) / 0.15f);
                else
                    targetCrack = 1f;
                _fractalCrackProgress = MathHelper.Lerp(_fractalCrackProgress, targetCrack, 0.015f);

                // Kaleidoscope rotation — Phase 3+ only, speeds up at lower HP
                if (_fractalCrackProgress > 0.4f)
                {
                    float rotSpeed = MathHelper.Lerp(0.0005f, 0.002f, (_fractalCrackProgress - 0.4f) / 0.6f);
                    _kaleidoscopeRotation += rotSpeed;
                }

                // Color drain — Enrage phase only
                float targetDrain = hpRatio < 0.3f ? MathHelper.Lerp(0f, 0.85f, (0.3f - hpRatio) / 0.3f) : 0f;
                _colorDrain = MathHelper.Lerp(_colorDrain, targetDrain, 0.01f);

                // Phase-dependent feather black ratio
                float blackRatio = hpRatio > 0.6f ? 0f : // Phase 1: all white
                                   hpRatio > 0.45f ? MathHelper.Lerp(0f, 0.5f, (0.6f - hpRatio) / 0.15f) : // Phase 2: black emerging
                                   0.5f; // Phase 3+: equal

                // Vignette — builds through phases
                float targetVignette = _currentMood switch
                {
                    0 => 0.1f + _intensity * 0.1f,
                    1 => 0.25f + _intensity * 0.15f,
                    _ => 0.35f + _intensity * 0.25f
                };
                _vignetteStrength = MathHelper.Lerp(_vignetteStrength, targetVignette, 0.02f);

                // Update feathers
                if (_feathers != null)
                {
                    // Enrage: slow to half speed (beauty in slow motion)
                    float speedMult = hpRatio < 0.3f ? 0.35f :
                                      _currentMood == 1 ? 1.5f : 1f;

                    for (int i = 0; i < MaxFeathers; i++)
                    {
                        // Phase 2+: convert some feathers to black
                        if (i < MaxFeathers * blackRatio && !_feathers[i].IsBlack)
                            _feathers[i].IsBlack = true;

                        _feathers[i].Position.Y += _feathers[i].Speed * speedMult;
                        float swaySpeed = MathHelper.Lerp(0.006f, 0.012f, _intensity);
                        _feathers[i].Position.X += (float)Math.Sin(Main.timeForVisualEffects * swaySpeed + _feathers[i].SwayPhase) * 0.4f * speedMult;
                        _feathers[i].Rotation += _feathers[i].RotSpeed * speedMult;

                        if (_feathers[i].Position.Y > Main.screenHeight + 20)
                        {
                            _feathers[i].Position.Y = -20;
                            _feathers[i].Position.X = Main.rand.NextFloat() * Main.screenWidth;
                        }

                        // Black feathers in Phase 2+ also drift upward (mirror)
                        if (_feathers[i].IsBlack && _fractalCrackProgress > 0.1f)
                        {
                            _feathers[i].Position.Y -= _feathers[i].Speed * speedMult * 0.6f;
                            if (_feathers[i].Position.Y < -20)
                            {
                                _feathers[i].Position.Y = Main.screenHeight + 20;
                                _feathers[i].Position.X = Main.rand.NextFloat() * Main.screenWidth;
                            }
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
                float eff = GetEffectiveIntensity();

                // === PHASE 1: Stark void-black background ===
                // Pure black — the void stage upon which the white swan dances
                Color skyBlack = Color.Black * _opacity * 0.85f;
                spriteBatch.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), skyBlack);

                // === PHASE 2+: Fracture lines / kaleidoscope ===
                if (_fractalCrackProgress > 0.01f)
                {
                    DrawFractalCracks(spriteBatch, pixel, eff);
                }

                // === KALEIDOSCOPE GEOMETRY (Phase 3+) ===
                if (_fractalCrackProgress > 0.4f)
                {
                    DrawKaleidoscope(spriteBatch, pixel, eff);
                }

                // === FEATHER PARTICLES ===
                DrawFeathers(spriteBatch, pixel, eff);

                // === ENRAGE: Color drain overlay ===
                if (_colorDrain > 0.01f)
                {
                    // Gray wash that drains all remaining color
                    Color drainOverlay = new Color(120, 120, 125) * _colorDrain * _opacity * 0.3f;
                    drainOverlay.A = (byte)(drainOverlay.A * 0.4f);
                    spriteBatch.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), drainOverlay);
                }

                // === VIGNETTE ===
                if (_vignetteStrength > 0.01f)
                {
                    DrawVignette(spriteBatch, pixel, eff);
                }
            }
        }

        /// <summary>
        /// Phase 2: White fracture lines crack across the void-black sky.
        /// Lines of pure white that split the darkness, with faint prismatic bleed at edges.
        /// </summary>
        private void DrawFractalCracks(SpriteBatch spriteBatch, Texture2D pixel, float eff)
        {
            float crackAlpha = _fractalCrackProgress * eff * _opacity;
            float time = (float)Main.timeForVisualEffects;
            int crackCount = (int)(_fractalCrackProgress * 12);
            Vector2 screenCenter = new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f;

            for (int i = 0; i < crackCount; i++)
            {
                // Deterministic crack angles radiating from center
                float baseAngle = MathHelper.TwoPi * i / 12f + _kaleidoscopeRotation;
                float crackLength = 200f + (i % 3) * 150f;

                // White crack line
                Vector2 crackDir = new Vector2((float)Math.Cos(baseAngle), (float)Math.Sin(baseAngle));
                for (int seg = 0; seg < (int)(crackLength / 6f); seg++)
                {
                    Vector2 segPos = screenCenter + crackDir * seg * 6f;
                    // Add slight jaggedness
                    float jitter = (float)Math.Sin(seg * 0.8f + i * 2.3f + time * 0.003f) * 3f;
                    segPos += crackDir.RotatedBy(MathHelper.PiOver2) * jitter;

                    if (segPos.X < -10 || segPos.X > Main.screenWidth + 10 ||
                        segPos.Y < -10 || segPos.Y > Main.screenHeight + 10) continue;

                    // White core
                    Color crackColor = new Color(240, 240, 255) * crackAlpha * 0.6f;
                    crackColor.A = 0;
                    spriteBatch.Draw(pixel, new Rectangle((int)segPos.X - 1, (int)segPos.Y - 1, 3, 3), crackColor);

                    // Faint prismatic bleed at crack edges (Phase 2+)
                    if (_fractalCrackProgress > 0.2f && seg % 5 == 0)
                    {
                        float hue = (seg * 0.05f + time * 0.002f + i * 0.3f) % 1f;
                        Color prism = Main.hslToRgb(hue, 0.8f, 0.7f) * crackAlpha * 0.2f;
                        prism.A = 0;
                        spriteBatch.Draw(pixel, new Rectangle((int)segPos.X - 3, (int)segPos.Y - 3, 7, 7), prism);
                    }
                }
            }
        }

        /// <summary>
        /// Phase 3: Full kaleidoscope fractal pattern — mirrored geometric panels
        /// of alternating black and white rotating slowly across the sky.
        /// </summary>
        private void DrawKaleidoscope(SpriteBatch spriteBatch, Texture2D pixel, float eff)
        {
            float kaleidoAlpha = MathHelper.Clamp((_fractalCrackProgress - 0.4f) / 0.6f, 0f, 1f);
            // Dim during enrage — color/brightness drains
            float drainMult = 1f - _colorDrain * 0.7f;
            kaleidoAlpha *= eff * _opacity * 0.35f * drainMult;

            if (kaleidoAlpha < 0.01f) return;

            Vector2 center = new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f;
            int segments = 8;
            float segmentAngle = MathHelper.TwoPi / segments;

            // Draw alternating black/white triangular segments radiating from center
            for (int seg = 0; seg < segments; seg++)
            {
                float angle1 = seg * segmentAngle + _kaleidoscopeRotation;
                float angle2 = angle1 + segmentAngle;
                bool isWhiteSegment = seg % 2 == 0;

                Color segColor = isWhiteSegment
                    ? new Color(220, 220, 235) * kaleidoAlpha
                    : new Color(10, 10, 15) * kaleidoAlpha;
                segColor.A = 0;

                // Draw as radiating bands
                float maxRadius = Math.Max(Main.screenWidth, Main.screenHeight) * 0.7f;
                for (float r = 80f; r < maxRadius; r += 20f)
                {
                    float midAngle = (angle1 + angle2) * 0.5f;
                    Vector2 bandPos = center + new Vector2((float)Math.Cos(midAngle), (float)Math.Sin(midAngle)) * r;

                    if (bandPos.X < -40 || bandPos.X > Main.screenWidth + 40 ||
                        bandPos.Y < -40 || bandPos.Y > Main.screenHeight + 40) continue;

                    // Band thickness decreases with distance
                    float thickness = MathHelper.Lerp(12f, 2f, r / maxRadius);
                    float arcWidth = r * segmentAngle * 0.6f;

                    // Vary panel opacity for fragmented look
                    float fragmentation = (float)Math.Sin(r * 0.02f + seg * 1.5f + (float)Main.timeForVisualEffects * 0.001f);
                    float panelAlpha = 0.5f + fragmentation * 0.3f;

                    Color panelColor = segColor * panelAlpha;
                    spriteBatch.Draw(pixel,
                        new Rectangle((int)(bandPos.X - arcWidth * 0.5f), (int)(bandPos.Y - thickness * 0.5f),
                            (int)arcWidth, (int)thickness),
                        panelColor);
                }

                // Prismatic edge between segments — only at destruction boundaries
                if (_fractalCrackProgress > 0.6f)
                {
                    float edgeHue = (seg * 0.125f + (float)Main.timeForVisualEffects * 0.003f) % 1f;
                    Color edgeColor = Main.hslToRgb(edgeHue, 0.9f, 0.8f) * kaleidoAlpha * 0.4f;
                    edgeColor.A = 0;

                    for (float r = 60f; r < maxRadius * 0.8f; r += 15f)
                    {
                        Vector2 edgePos = center + new Vector2((float)Math.Cos(angle1), (float)Math.Sin(angle1)) * r;
                        if (edgePos.X >= 0 && edgePos.X <= Main.screenWidth &&
                            edgePos.Y >= 0 && edgePos.Y <= Main.screenHeight)
                        {
                            spriteBatch.Draw(pixel, new Rectangle((int)edgePos.X - 1, (int)edgePos.Y - 1, 3, 3), edgeColor);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Draw sky feather particles — phase-aware coloring and behavior.
        /// </summary>
        private void DrawFeathers(SpriteBatch spriteBatch, Texture2D pixel, float eff)
        {
            if (_feathers == null || _opacity < 0.1f) return;

            int visibleFeathers = (int)MathHelper.Lerp(MaxFeathers * 0.4f, MaxFeathers, _intensity);
            for (int i = 0; i < visibleFeathers && i < MaxFeathers; i++)
            {
                Color c;
                if (_colorDrain > 0.3f)
                {
                    // Enrage: feathers drain to uniform gray — beauty fading
                    float gray = MathHelper.Lerp(0.5f, 0.35f, _colorDrain);
                    c = new Color(gray, gray, gray + 0.02f) * _opacity * 0.5f;
                }
                else if (_feathers[i].IsBlack)
                {
                    c = new Color(15, 15, 20) * _opacity * 0.5f;
                }
                else
                {
                    c = new Color(235, 235, 245) * _opacity * 0.4f;
                }
                c.A = 0;

                float s = _feathers[i].Scale * (1f + _intensity * 0.3f);
                spriteBatch.Draw(pixel,
                    _feathers[i].Position,
                    new Rectangle(0, 0, 1, 1),
                    c,
                    _feathers[i].Rotation,
                    new Vector2(0.5f),
                    new Vector2(s * 2f, s * 4.5f), // Elongated feather shape
                    SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Darkens edges of screen — builds through the fight.
        /// </summary>
        private void DrawVignette(SpriteBatch spriteBatch, Texture2D pixel, float eff)
        {
            float effectiveVignette = _vignetteStrength * eff;
            for (int ring = 0; ring < 10; ring++)
            {
                float ringT = ring / 10f;
                float ringAlpha = ringT * ringT * effectiveVignette * 0.5f;

                Color vignetteColor = new Color(2, 2, 5) * ringAlpha;
                int thickness = 50 + ring * 35;
                spriteBatch.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, thickness), vignetteColor);
                spriteBatch.Draw(pixel, new Rectangle(0, Main.screenHeight - thickness, Main.screenWidth, thickness), vignetteColor);
                spriteBatch.Draw(pixel, new Rectangle(0, 0, thickness, Main.screenHeight), vignetteColor);
                spriteBatch.Draw(pixel, new Rectangle(Main.screenWidth - thickness, 0, thickness, Main.screenHeight), vignetteColor);
            }
        }

        public override Color OnTileColor(Color inColor)
        {
            float eff = GetEffectiveIntensity();
            Color tint;
            if (_colorDrain > 0.1f)
            {
                // Enrage: tiles drain toward monochrome gray
                float gray = MathHelper.Lerp(0.85f, 0.55f, _colorDrain);
                tint = Color.Lerp(Color.White, new Color(gray, gray, gray), eff * 0.5f);
            }
            else
            {
                // Normal: cool silver moonlight wash — stark, clean
                tint = Color.Lerp(Color.White, new Color(200, 200, 215), eff * 0.4f);
            }
            return inColor.MultiplyRGBA(tint);
        }

        public override float GetCloudAlpha() => 1f - _opacity * 0.7f;
    }

    /// <summary>
    /// Companion ModSystem for the Swan Lake sky.
    /// Feeds boss state, provides screen flash APIs, and spawns ambient world particles.
    /// </summary>
    public class SwanLakeSkySystem : ModSystem
    {
        private static float _flashTimer;
        private static float _flashDuration;
        private static Color _flashColor;

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
            if (SkyManager.Instance["MagnumOpus:SwanLakeSky"] is SwanLakeSky sky)
            {
                sky.UpdateBossState(BossLifeRatio, BossCenter, BossIsDyingSwan);
            }

            if (_flashTimer > 0)
                _flashTimer--;

            // Phase-aware ambient feather dust near boss
            if (BossCenter != Vector2.Zero && BossLifeRatio < 1f)
            {
                float hpDrive = 1f - BossLifeRatio;

                if (BossIsDyingSwan)
                {
                    // Enrage: slow gray feathers drifting down, mourning
                    if (Main.rand.NextBool(3))
                    {
                        Vector2 pos = BossCenter + Main.rand.NextVector2Circular(250f, 250f);
                        Dust d = Dust.NewDustPerfect(pos, Terraria.ID.DustID.SilverCoin,
                            new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextFloat(0.05f, 0.2f)),
                            100, new Color(180, 180, 190), 0.4f);
                        d.noGravity = true;
                        d.fadeIn = 1.5f;
                    }

                    // Desperate prismatic sparkle — rare
                    if (Main.rand.NextBool(12))
                    {
                        Vector2 pos = BossCenter + Main.rand.NextVector2Circular(150f, 150f);
                        float hue = Main.rand.NextFloat();
                        Color sparkleColor = Main.hslToRgb(hue, 0.9f, 0.85f);
                        Dust d = Dust.NewDustPerfect(pos, Terraria.ID.DustID.RainbowMk2,
                            Main.rand.NextVector2Circular(0.5f, 0.5f), 80, sparkleColor, 0.35f);
                        d.noGravity = true;
                    }
                }
                else
                {
                    // Normal: white feather motes drifting
                    int featherChance = Math.Max(1, (int)MathHelper.Lerp(5, 2, hpDrive));
                    if (Main.rand.NextBool(featherChance))
                    {
                        Vector2 pos = BossCenter + Main.rand.NextVector2Circular(300f, 300f);
                        Dust d = Dust.NewDustPerfect(pos, Terraria.ID.DustID.WhiteTorch,
                            new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-0.5f, 0.5f)),
                            0, Color.White, 0.5f);
                        d.noGravity = true;
                        d.fadeIn = 1.2f;
                    }

                    // Phase 2+: black feathers mirroring
                    if (BossLifeRatio < 0.6f && Main.rand.NextBool(featherChance + 1))
                    {
                        Vector2 pos = BossCenter + Main.rand.NextVector2Circular(300f, 300f);
                        Dust d = Dust.NewDustPerfect(pos, Terraria.ID.DustID.Shadowflame,
                            new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-0.5f, 0.5f)),
                            120, new Color(15, 15, 20), 0.45f);
                        d.noGravity = true;
                    }
                }
            }
        }

        public override void ModifyScreenPosition()
        {
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
