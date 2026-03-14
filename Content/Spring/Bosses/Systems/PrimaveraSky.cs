using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ID;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Spring.Bosses.Systems
{
    /// <summary>
    /// Phase-driven sky for Primavera — Vivaldi's Spring.
    /// Phase 1: Dawn meadow (amber-pink, god rays, lazy petals)
    /// Phase 2: Spring rainstorm (green-gray, wind streaks, fast petals)
    /// Phase 3: Full bloom (vibrant hot pink, overwhelming petal density)
    /// Enrage: Nature's wrath (magenta-crimson pulse, hostile chaos)
    /// </summary>
    public class PrimaveraSky : CustomSky
    {
        private bool _isActive;
        private float _opacity;

        // Boss state (set from Primavera.cs — signatures preserved)
        public static float BossLifeRatio { get; set; } = 1f;
        public static Vector2 BossCenter { get; set; }
        public static bool BossIsEnraged { get; set; }

        // Smooth phase blend: 0=Dawn, 1=Storm, 2=FullBloom, 3=Enrage
        private float _phaseBlend;
        private Vector2 _currentWind;

        private struct SkyPetal
        {
            public Vector2 Position;
            public float DriftSpeed;
            public float SwayAmplitude;
            public float SwayPhase;
            public float Scale;
            public float Rotation;
            public float RotSpeed;
            public int Variant; // 0=small, 1=medium, 2=large, 3=glowy
            public float Brightness;
        }

        private SkyPetal[] _petals;
        private const int MaxPetals = 120;

        private struct WindStreak
        {
            public Vector2 Position;
            public float Speed;
            public float Length;
            public float Life;
            public float MaxLife;
        }

        private WindStreak[] _windStreaks;
        private const int MaxWindStreaks = 8;
        private Texture2D _glowTex;

        // Phase sky gradient colors
        private static readonly Color P1Top = new Color(255, 200, 100);
        private static readonly Color P1Bottom = new Color(255, 183, 197);
        private static readonly Color P2Top = new Color(140, 130, 140);
        private static readonly Color P2Bottom = new Color(100, 125, 105);
        private static readonly Color P3Top = new Color(255, 105, 180);
        private static readonly Color P3Bottom = new Color(255, 160, 200);
        private static readonly Color EnrTop = new Color(200, 0, 100);
        private static readonly Color EnrBottom = new Color(139, 0, 0);

        // Phase vignette colors
        private static readonly Color P1Vig = new Color(255, 215, 180);
        private static readonly Color P2Vig = new Color(112, 128, 144);
        private static readonly Color P3Vig = new Color(255, 105, 180);
        private static readonly Color EnrVig = new Color(255, 0, 100);

        // Phase petal colors (vibrant blooms with visual flare)
        private static readonly Color P1Petal = new Color(255, 200, 220);
        private static readonly Color P2Petal = new Color(255, 160, 190);
        private static readonly Color P3Petal = new Color(255, 100, 170);
        private static readonly Color EnrPetal = new Color(255, 0, 140);

        private float GetTargetBlend()
        {
            if (BossIsEnraged) return 3f;
            if (BossLifeRatio > 0.6f) return 0f;
            if (BossLifeRatio > 0.3f) return 1f;
            return 2f;
        }

        private int GetPhaseIndex()
        {
            if (BossIsEnraged) return 3;
            if (BossLifeRatio > 0.6f) return 0;
            if (BossLifeRatio > 0.3f) return 1;
            return 2;
        }

        private static Vector2 GetWindForPhase(float time, int phase) => phase switch
        {
            0 => new Vector2((float)Math.Sin(time * 0.03) * 0.6f + 0.4f, 0.2f),
            1 => new Vector2((float)(Math.Sin(time * 0.08) * 3.5 + Math.Cos(time * 0.13) * 2), 0.6f),
            2 => new Vector2((float)Math.Sin(time * 0.06) * 2.5f, (float)Math.Cos(time * 0.06) * 1.5f + 0.4f),
            _ => new Vector2(
                (float)(Math.Sin(time * 0.11) * 4 + Math.Cos(time * 0.19) * 3 + Math.Sin(time * 0.31) * 2),
                (float)(Math.Cos(time * 0.09) * 2 + Math.Sin(time * 0.23) * 1.5))
        };

        private float GetEffectiveIntensity(Vector2 screenCenter)
        {
            float dist = Vector2.Distance(screenCenter, BossCenter);
            return MathHelper.SmoothStep(1f, 0.15f, dist / 3500f);
        }

        public override void OnLoad() { }

        public override void Activate(Vector2 position, params object[] args)
        {
            _isActive = true;
            _phaseBlend = 0f;
            _currentWind = new Vector2(0.4f, 0.2f);

            try
            {
                _glowTex = ModContent.Request<Texture2D>(
                    "MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow64",
                    AssetRequestMode.ImmediateLoad).Value;
            }
            catch { _glowTex = null; }

            _petals = new SkyPetal[MaxPetals];
            Random rand = new Random();
            for (int i = 0; i < MaxPetals; i++)
            {
                _petals[i] = new SkyPetal
                {
                    Position = new Vector2(
                        (float)rand.NextDouble() * Main.screenWidth,
                        (float)rand.NextDouble() * Main.screenHeight),
                    DriftSpeed = 0.2f + (float)rand.NextDouble() * 0.8f,
                    SwayAmplitude = 15f + (float)rand.NextDouble() * 35f,
                    SwayPhase = (float)rand.NextDouble() * MathHelper.TwoPi,
                    Scale = 0.15f + (float)rand.NextDouble() * 0.65f,
                    Rotation = (float)rand.NextDouble() * MathHelper.TwoPi,
                    RotSpeed = ((float)rand.NextDouble() - 0.5f) * 0.05f,
                    Variant = rand.Next(4),
                    Brightness = 0.5f + (float)rand.NextDouble() * 0.5f
                };
            }

            _windStreaks = new WindStreak[MaxWindStreaks];
        }

        public override void Deactivate(params object[] args) { _isActive = false; }
        public override void Reset() { _isActive = false; _opacity = 0f; }
        public override bool IsActive() => _isActive || _opacity > 0.001f;

        public override void Update(GameTime gameTime)
        {
            if (_isActive)
                _opacity = Math.Min(1f, _opacity + 0.02f);
            else
                _opacity = Math.Max(0f, _opacity - 0.02f);

            NPC boss = BossIndexTracker.GetActiveBoss(BossIndexTracker.Primavera);
            if (boss == null) return;

            float time = (float)Main.timeForVisualEffects * 0.01f;
            int phase = GetPhaseIndex();
            float targetBlend = GetTargetBlend();

            // Smooth phase transition (~2 seconds)
            _phaseBlend = MathHelper.Lerp(_phaseBlend, targetBlend,
                Math.Abs(_phaseBlend - targetBlend) > 0.01f ? 0.008f : 1f);

            // Wind
            _currentWind = Vector2.Lerp(_currentWind, GetWindForPhase(time, phase), 0.02f);

            int activePetals = phase switch { 0 => 40, 1 => 65, 2 => MaxPetals, _ => 70 };
            float speedMult = phase switch { 0 => 0.7f, 1 => 1.4f, 2 => 1.0f, _ => 1.8f };
            float rotMult = phase switch { 0 => 0.8f, 1 => 1.5f, 2 => 1.2f, _ => 3.0f };

            if (_petals != null)
            {
                for (int i = 0; i < MaxPetals; i++)
                {
                    ref SkyPetal p = ref _petals[i];
                    p.Position.Y += p.DriftSpeed * speedMult;
                    p.Position.X += (float)Math.Sin(time * 6f + p.SwayPhase) * p.SwayAmplitude * 0.012f;
                    p.Position += _currentWind * 0.5f;
                    p.Rotation += p.RotSpeed * rotMult;

                    if (p.Position.Y > Main.screenHeight + 30)
                    {
                        if (i < activePetals)
                        {
                            p.Position.Y = -30;
                            p.Position.X = Main.rand.NextFloat() * Main.screenWidth;
                        }
                    }
                    if (p.Position.X > Main.screenWidth + 40) p.Position.X = -30;
                    if (p.Position.X < -40) p.Position.X = Main.screenWidth + 20;
                }
            }

            // Wind streaks (Phase 2+)
            if (_windStreaks != null && phase >= 1)
            {
                for (int i = 0; i < MaxWindStreaks; i++)
                {
                    ref WindStreak ws = ref _windStreaks[i];
                    if (ws.MaxLife > 0)
                    {
                        ws.Life += 1f;
                        ws.Position.X += ws.Speed;
                        if (ws.Life >= ws.MaxLife) ws.MaxLife = 0;
                    }
                    else if (Main.rand.NextBool(phase >= 3 ? 8 : (phase >= 2 ? 15 : 25)))
                    {
                        float windDir = _currentWind.X >= 0 ? 1f : -1f;
                        ws.Position = new Vector2(
                            windDir > 0 ? -100 : Main.screenWidth + 100,
                            Main.rand.NextFloat() * Main.screenHeight);
                        ws.Speed = (6f + Main.rand.NextFloat() * 4f) * windDir;
                        ws.Length = 80f + Main.rand.NextFloat() * 160f;
                        ws.Life = 0;
                        ws.MaxLife = 60f;
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth < 0f || minDepth >= 0f) return;

            Vector2 screenCenter = Main.screenPosition + new Vector2(Main.screenWidth, Main.screenHeight) / 2f;
            float proximity = GetEffectiveIntensity(screenCenter);
            float fade = _opacity * proximity;
            if (fade < 0.01f) return;

            Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
            float time = (float)Main.timeForVisualEffects * 0.01f;
            int phase = GetPhaseIndex();

            // ===== SKY GRADIENT =====
            Color topColor, bottomColor;
            if (_phaseBlend <= 1f)
            { topColor = Color.Lerp(P1Top, P2Top, _phaseBlend); bottomColor = Color.Lerp(P1Bottom, P2Bottom, _phaseBlend); }
            else if (_phaseBlend <= 2f)
            { float t = _phaseBlend - 1f; topColor = Color.Lerp(P2Top, P3Top, t); bottomColor = Color.Lerp(P2Bottom, P3Bottom, t); }
            else
            { float t = _phaseBlend - 2f; topColor = Color.Lerp(P3Top, EnrTop, t); bottomColor = Color.Lerp(P3Bottom, EnrBottom, t); }

            topColor *= fade * 0.55f;
            bottomColor *= fade * 0.55f;
            for (int y = 0; y < Main.screenHeight; y += 4)
            {
                float t = (float)y / Main.screenHeight;
                spriteBatch.Draw(pixel, new Rectangle(0, y, Main.screenWidth, 4),
                    Color.Lerp(topColor, bottomColor, t));
            }

            // ===== GOD RAYS (Phase 1 fading into Phase 2) =====
            if (_phaseBlend < 1.5f)
            {
                float godRayFade = Math.Max(0, 1f - _phaseBlend) * fade * 0.1f;
                if (godRayFade > 0.003f)
                {
                    for (int ray = 0; ray < 5; ray++)
                    {
                        float rayPhase = ray * 1.3f + time * 0.3f;
                        float rayX = (float)(Math.Sin(rayPhase) * 0.3 + 0.5) * Main.screenWidth;
                        float rayWidth = 35 + (float)Math.Sin(rayPhase * 0.7) * 12;
                        float flicker = 0.6f + (float)Math.Sin(rayPhase * 2) * 0.4f;
                        Color rayColor = new Color(255, 230, 180) * (godRayFade * flicker);
                        rayColor.A = 0;

                        for (int seg = 0; seg < Main.screenHeight; seg += 8)
                        {
                            float segT = (float)seg / Main.screenHeight;
                            float xOff = segT * 50f;
                            float w = rayWidth * (1f - segT * 0.3f);
                            Color c = rayColor * (1f - segT * 0.6f);
                            spriteBatch.Draw(pixel, new Rectangle(
                                (int)(rayX + xOff - w / 2), seg, Math.Max(1, (int)w), 8), c);
                        }
                    }
                }
            }

            // ===== WIND STREAKS (Phase 2+) =====
            if (_windStreaks != null && _phaseBlend > 0.5f)
            {
                float streakFade = Math.Min(1f, (_phaseBlend - 0.5f) * 2f) * fade;
                Color streakBase = phase >= 3
                    ? new Color(255, 0, 180)
                    : new Color(124, 252, 0);
                Color streakColor = streakBase * (streakFade * 0.1f);
                streakColor.A = 0;

                for (int i = 0; i < MaxWindStreaks; i++)
                {
                    ref WindStreak ws = ref _windStreaks[i];
                    if (ws.MaxLife <= 0) continue;
                    float alpha = (float)Math.Sin(ws.Life / ws.MaxLife * Math.PI);
                    Color c = streakColor * alpha;
                    int h = 2 + (int)(alpha * 3);
                    spriteBatch.Draw(pixel, new Rectangle(
                        (int)ws.Position.X, (int)ws.Position.Y, (int)ws.Length, h), c);
                }
            }

            // ===== ENRAGE CRIMSON PULSE =====
            if (_phaseBlend > 2.5f)
            {
                float enragePulse = (float)Math.Sin(time * 3f) * 0.5f + 0.5f;
                float pulseAlpha = (_phaseBlend - 2.5f) * 2f * fade * 0.12f * enragePulse;
                Color pulseColor = new Color(139, 0, 0) * pulseAlpha;
                pulseColor.A = 0;
                spriteBatch.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), pulseColor);
            }

            // ===== VIGNETTE =====
            Color vigCol;
            float vigStr;
            if (_phaseBlend <= 1f)
            { vigCol = Color.Lerp(P1Vig, P2Vig, _phaseBlend); vigStr = MathHelper.Lerp(0.08f, 0.14f, _phaseBlend); }
            else if (_phaseBlend <= 2f)
            { vigCol = Color.Lerp(P2Vig, P3Vig, _phaseBlend - 1f); vigStr = MathHelper.Lerp(0.14f, 0.18f, _phaseBlend - 1f); }
            else
            { vigCol = Color.Lerp(P3Vig, EnrVig, _phaseBlend - 2f); vigStr = MathHelper.Lerp(0.18f, 0.25f, _phaseBlend - 2f); }

            float vigOp = vigStr * fade;
            for (int ring = 0; ring < 10; ring++)
            {
                float rp = ring / 10f;
                float ra = rp * rp * vigOp;
                int mx = (int)(Main.screenWidth * 0.5f * (1f - rp));
                int my = (int)(Main.screenHeight * 0.5f * (1f - rp));
                Color vc = vigCol * ra; vc.A = 0;
                spriteBatch.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, Math.Max(1, my)), vc);
                spriteBatch.Draw(pixel, new Rectangle(0, Main.screenHeight - my, Main.screenWidth, Math.Max(1, my)), vc);
                spriteBatch.Draw(pixel, new Rectangle(0, 0, Math.Max(1, mx), Main.screenHeight), vc);
                spriteBatch.Draw(pixel, new Rectangle(Main.screenWidth - mx, 0, Math.Max(1, mx), Main.screenHeight), vc);
            }

            // ===== BLOSSOM PETALS (vibrant blooms with visual flare) =====
            if (_petals != null && fade > 0.1f)
            {
                Color petalBase;
                float petalVis;
                if (_phaseBlend <= 1f)
                { petalBase = Color.Lerp(P1Petal, P2Petal, _phaseBlend); petalVis = MathHelper.Lerp(0.35f, 0.55f, _phaseBlend); }
                else if (_phaseBlend <= 2f)
                { petalBase = Color.Lerp(P2Petal, P3Petal, _phaseBlend - 1f); petalVis = MathHelper.Lerp(0.55f, 0.85f, _phaseBlend - 1f); }
                else
                { petalBase = Color.Lerp(P3Petal, EnrPetal, _phaseBlend - 2f); petalVis = MathHelper.Lerp(0.85f, 0.7f, _phaseBlend - 2f); }

                int activePetals = phase switch { 0 => 40, 1 => 65, 2 => MaxPetals, _ => 70 };
                float pf = petalVis * fade;

                for (int i = 0; i < Math.Min(activePetals, MaxPetals); i++)
                {
                    ref SkyPetal p = ref _petals[i];
                    Color pc = Color.Lerp(petalBase, Color.White, p.Brightness * 0.35f);
                    float bs = p.Variant switch { 0 => p.Scale * 0.6f, 1 => p.Scale, 2 => p.Scale * 1.5f, _ => p.Scale * 0.8f };

                    // Glow layer for bright/glowy petals
                    if (_glowTex != null && (p.Variant == 3 || p.Brightness > 0.75f))
                    {
                        Color gc = pc * (pf * 0.25f * p.Brightness); gc.A = 0;
                        spriteBatch.Draw(_glowTex, p.Position, null, gc,
                            0f, _glowTex.Size() / 2f, bs * 2f, SpriteEffects.None, 0f);
                    }

                    // Petal shape (elongated bloom)
                    Color sc = pc * pf; sc.A = 0;
                    spriteBatch.Draw(pixel, p.Position, new Rectangle(0, 0, 1, 1), sc,
                        p.Rotation, new Vector2(0.5f), new Vector2(bs * 4f, bs * 2f),
                        SpriteEffects.None, 0f);

                    // Bright inner core
                    Color cc = Color.White * (pf * 0.3f * p.Brightness); cc.A = 0;
                    spriteBatch.Draw(pixel, p.Position, new Rectangle(0, 0, 1, 1), cc,
                        p.Rotation, new Vector2(0.5f), new Vector2(bs * 2f, bs),
                        SpriteEffects.None, 0f);
                }
            }

            // ===== PHASE 3+ LARGE OUT-OF-FOCUS BLOOM OVERLAYS =====
            if (_phaseBlend > 1.5f && _glowTex != null)
            {
                float bigAlpha = Math.Min(1f, (_phaseBlend - 1.5f) * 2f) * fade * 0.06f;
                for (int i = 0; i < 4; i++)
                {
                    float dx = (float)Math.Sin(time * 0.2f + i * 1.7f) * Main.screenWidth * 0.3f + Main.screenWidth * 0.5f;
                    float dy = (float)Math.Cos(time * 0.15f + i * 2.1f) * Main.screenHeight * 0.3f + Main.screenHeight * 0.5f;
                    Color bc = P3Petal * bigAlpha; bc.A = 0;
                    spriteBatch.Draw(_glowTex, new Vector2(dx, dy), null, bc,
                        time * 0.1f + i, _glowTex.Size() / 2f,
                        4f + (float)Math.Sin(time + i) * 0.5f, SpriteEffects.None, 0f);
                }
            }
        }

        public override Color OnTileColor(Color inColor)
        {
            Color tint;
            if (_phaseBlend <= 1f)
                tint = Color.Lerp(new Color(255, 245, 230), new Color(220, 235, 220), _phaseBlend);
            else if (_phaseBlend <= 2f)
                tint = Color.Lerp(new Color(220, 235, 220), new Color(255, 225, 240), _phaseBlend - 1f);
            else
                tint = Color.Lerp(new Color(255, 225, 240), new Color(255, 190, 210), _phaseBlend - 2f);

            return inColor.MultiplyRGBA(Color.Lerp(Color.White, tint, _opacity * 0.3f));
        }

        public override float GetCloudAlpha()
        {
            float cloudMod = _phaseBlend <= 1f ? 0.3f : (_phaseBlend <= 2f ? 0.6f : 0.2f);
            return 1f - _opacity * cloudMod;
        }
    }

    /// <summary>
    /// Companion ModSystem — flash APIs, phase-aware ambient world particles.
    /// </summary>
    public class PrimaveraSkySystem : ModSystem
    {
        private static float _flashIntensity;
        private static float _flashDecay;
        private static Color _flashColor;

        private static readonly Color CherryPink = new Color(255, 183, 197);
        private static readonly Color FreshGreen = new Color(124, 252, 0);
        private static readonly Color Lavender = new Color(181, 126, 220);
        private static readonly Color ViolentMagenta = new Color(255, 0, 255);

        public static void TriggerBlossomFlash(float intensity)
        {
            _flashColor = CherryPink;
            _flashIntensity = Math.Min(intensity, 0.92f);
            _flashDecay = 0.06f;
        }

        public static void TriggerGrowthFlash(float intensity)
        {
            _flashColor = FreshGreen;
            _flashIntensity = Math.Min(intensity, 0.90f);
            _flashDecay = 0.055f;
        }

        public static void TriggerVernalFlash(float intensity)
        {
            _flashColor = new Color(255, 240, 200);
            _flashIntensity = Math.Min(intensity, 0.88f);
            _flashDecay = 0.05f;
        }

        public static void TriggerRebirthFlash(float intensity)
        {
            _flashColor = Color.White;
            _flashIntensity = Math.Min(intensity, 0.85f);
            _flashDecay = 0.04f;
        }

        public static void TriggerStormFlash(float intensity)
        {
            _flashColor = Lavender;
            _flashIntensity = Math.Min(intensity, 0.80f);
            _flashDecay = 0.07f;
        }

        public static void TriggerWrathFlash(float intensity)
        {
            _flashColor = ViolentMagenta;
            _flashIntensity = Math.Min(intensity, 0.90f);
            _flashDecay = 0.05f;
        }

        public override void PostUpdateEverything()
        {
            if (_flashIntensity > 0.01f)
            {
                Lighting.GlobalBrightness = Math.Max(Lighting.GlobalBrightness, _flashIntensity * 0.3f);
                _flashIntensity -= _flashDecay;
            }

            NPC boss = BossIndexTracker.GetActiveBoss(BossIndexTracker.Primavera);
            if (boss == null || Main.netMode == NetmodeID.Server) return;

            float hpRatio = (float)boss.life / boss.lifeMax;
            bool enraged = PrimaveraSky.BossIsEnraged;
            int phase = enraged ? 3 : (hpRatio > 0.6f ? 0 : (hpRatio > 0.3f ? 1 : 2));

            // Phase-scaled ambient pollen
            int pollenChance = phase switch { 0 => 8, 1 => 5, 2 => 3, _ => 4 };
            if (Main.rand.NextBool(Math.Max(1, pollenChance)))
            {
                Vector2 pos = boss.Center + Main.rand.NextVector2Circular(300f, 300f);
                int dustType = phase >= 3 ? DustID.PinkTorch : DustID.YellowTorch;
                Dust d = Dust.NewDustPerfect(pos, dustType,
                    new Vector2(Main.rand.NextFloat(-0.4f, 0.6f), Main.rand.NextFloat(-0.2f, 0.6f)),
                    100, default, 0.6f + phase * 0.1f);
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }

            // Phase 2+: Wind-driven petal wisps
            if (phase >= 1 && Main.rand.NextBool(Math.Max(1, 8 - phase * 2)))
            {
                Vector2 pos = boss.Center + Main.rand.NextVector2Circular(250f, 250f);
                float windX = phase >= 3 ? Main.rand.NextFloat(-2f, 2f) : Main.rand.NextFloat(0.5f, 2f);
                Dust d = Dust.NewDustPerfect(pos, DustID.PinkTorch,
                    new Vector2(windX, -Main.rand.NextFloat(0.2f, 0.8f)), 80, default, 0.5f + phase * 0.1f);
                d.noGravity = true;
            }

            // Phase 3: Ground bloom sparkles
            if (phase >= 2 && Main.rand.NextBool(6))
            {
                Vector2 groundPos = boss.Center + new Vector2(Main.rand.NextFloat(-400f, 400f), 100f);
                Dust d = Dust.NewDustPerfect(groundPos, DustID.PinkFairy,
                    new Vector2(0, -Main.rand.NextFloat(0.5f, 1.5f)), 60, CherryPink, 0.8f);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }
        }
    }
}
