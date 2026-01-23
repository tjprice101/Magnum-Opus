using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using ReLogic.Content;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Enhanced custom particle system for MagnumOpus with 94 particle textures:
    /// - 7 EnergyFlare variants (intense bursts, explosions)
    /// - 4 SoftGlow variants (trails, auras, ambient)
    /// - 6 MusicNote variants (thematic musical effects)
    /// - 6 GlowingHalo variants (radiant auras, boss effects)
    /// - 4 ParticleTrail variants (motion trails, projectile paths)
    /// - 12 MagicSparkleField variants (buff indicators, magic auras, enchantments)
    /// - 15 PrismaticSparkle variants (gem effects, treasure highlights, magic sparkles)
    /// - 9 SwordArc variants (arcing projectile slashes, energy wave attacks)
    /// - 10 SwanFeather variants (Swan Lake exclusive - elegant floating feathers)
    /// - 8 EnigmaEye variants (Enigma theme - mysterious watching eyes, meaningful placement at impacts/targets)
    /// - 12 Glyph variants (universal arcane symbols - usable for all themes, debuff stacking, magic circles)
    /// All textures are white/grayscale and tinted at runtime.
    /// </summary>
    public class CustomParticleSystem : ModSystem
    {
        // EnergyFlare textures (7 variants)
        public static Asset<Texture2D>[] EnergyFlares { get; private set; } = new Asset<Texture2D>[7];
        // SoftGlow textures (4 variants)
        public static Asset<Texture2D>[] SoftGlows { get; private set; } = new Asset<Texture2D>[4];
        // MusicNote textures (6 variants)
        public static Asset<Texture2D>[] MusicNotes { get; private set; } = new Asset<Texture2D>[6];
        // GlowingHalo textures (6 variants)
        public static Asset<Texture2D>[] GlowingHalos { get; private set; } = new Asset<Texture2D>[6];
        // ParticleTrail textures (4 variants)
        public static Asset<Texture2D>[] ParticleTrails { get; private set; } = new Asset<Texture2D>[4];
        // MagicSparkleField textures (12 variants) - Buff indicators, magic auras, enchantments
        public static Asset<Texture2D>[] MagicSparkleFields { get; private set; } = new Asset<Texture2D>[12];
        // PrismaticSparkle textures (15 variants) - Gem effects, treasure highlights, magic sparkles
        public static Asset<Texture2D>[] PrismaticSparkles { get; private set; } = new Asset<Texture2D>[15];
        // SwordArc textures (9 variants) - Arcing projectile slashes, energy wave attacks
        public static Asset<Texture2D>[] SwordArcs { get; private set; } = new Asset<Texture2D>[9];
        // SwanFeather textures (10 variants) - Swan Lake exclusive feather effects
        public static Asset<Texture2D>[] SwanFeathers { get; private set; } = new Asset<Texture2D>[10];
        // EnigmaEye textures (8 variants) - Enigma theme mysterious watching eyes
        public static Asset<Texture2D>[] EnigmaEyes { get; private set; } = new Asset<Texture2D>[8];
        // Glyph textures (12 variants) - Universal arcane symbols for all themes
        public static Asset<Texture2D>[] Glyphs { get; private set; } = new Asset<Texture2D>[12];
        
        // Optimized particle pools - increased for particle-heavy boss fights
        private static List<CustomParticle> activeParticles = new List<CustomParticle>(1000);
        private static Queue<CustomParticle> particlePool = new Queue<CustomParticle>(400);
        
        public static bool TexturesLoaded { get; private set; } = false;
        public static int MaxParticles = 1200;
        
        public override void Load()
        {
            try
            {
                // Load EnergyFlare variants
                EnergyFlares[0] = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare", AssetRequestMode.ImmediateLoad);
                for (int i = 1; i < 7; i++)
                    EnergyFlares[i] = ModContent.Request<Texture2D>($"MagnumOpus/Assets/Particles/EnergyFlare{i + 1}", AssetRequestMode.ImmediateLoad);
                
                // Load SoftGlow variants
                SoftGlows[0] = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow", AssetRequestMode.ImmediateLoad);
                for (int i = 1; i < 4; i++)
                    SoftGlows[i] = ModContent.Request<Texture2D>($"MagnumOpus/Assets/Particles/SoftGlow{i + 1}", AssetRequestMode.ImmediateLoad);
                
                // Load MusicNote variants
                MusicNotes[0] = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/MusicNote", AssetRequestMode.ImmediateLoad);
                for (int i = 1; i < 6; i++)
                    MusicNotes[i] = ModContent.Request<Texture2D>($"MagnumOpus/Assets/Particles/MusicNote{i + 1}", AssetRequestMode.ImmediateLoad);
                
                // Load GlowingHalo variants (SKIP index 2 - GlowingHalo3.png is BANNED and DELETED)
                // Index mapping: 0=Halo1, 1=Halo2, 2=DELETED, 3=Halo4, 4=Halo5, 5=Halo6
                GlowingHalos[0] = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/GlowingHalo1", AssetRequestMode.ImmediateLoad);
                GlowingHalos[1] = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/GlowingHalo2", AssetRequestMode.ImmediateLoad);
                // GlowingHalos[2] - BANNED - GlowingHalo3.png has been DELETED from the mod
                GlowingHalos[2] = GlowingHalos[1]; // Fallback to Halo2 if code accidentally tries to use index 2
                GlowingHalos[3] = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/GlowingHalo4", AssetRequestMode.ImmediateLoad);
                GlowingHalos[4] = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/GlowingHalo5", AssetRequestMode.ImmediateLoad);
                GlowingHalos[5] = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/GlowingHalo6", AssetRequestMode.ImmediateLoad);
                
                // Load ParticleTrail variants
                for (int i = 0; i < 4; i++)
                    ParticleTrails[i] = ModContent.Request<Texture2D>($"MagnumOpus/Assets/Particles/ParticleTrail{i + 1}", AssetRequestMode.ImmediateLoad);
                
                // Load MagicSparkleField variants (12 total) - Buff indicators, magic auras
                for (int i = 0; i < 12; i++)
                    MagicSparkleFields[i] = ModContent.Request<Texture2D>($"MagnumOpus/Assets/Particles/MagicSparklField{i + 1}", AssetRequestMode.ImmediateLoad);
                
                // Load PrismaticSparkle variants (15 total) - Gem effects, treasure highlights
                for (int i = 0; i < 15; i++)
                    PrismaticSparkles[i] = ModContent.Request<Texture2D>($"MagnumOpus/Assets/Particles/PrismaticSparkle{i + 1}", AssetRequestMode.ImmediateLoad);
                
                // Load SwordArc variants (9 total) - Arcing projectile slashes
                for (int i = 0; i < 9; i++)
                    SwordArcs[i] = ModContent.Request<Texture2D>($"MagnumOpus/Assets/Particles/SwordArc{i + 1}", AssetRequestMode.ImmediateLoad);
                
                // Load SwanFeather variants (10 total) - Swan Lake exclusive feathers
                for (int i = 0; i < 10; i++)
                    SwanFeathers[i] = ModContent.Request<Texture2D>($"MagnumOpus/Assets/Particles/SwanFeather{i + 1}", AssetRequestMode.ImmediateLoad);
                
                // Load EnigmaEye variants (8 total) - Mysterious watching eyes for Enigma theme
                for (int i = 0; i < 8; i++)
                    EnigmaEyes[i] = ModContent.Request<Texture2D>($"MagnumOpus/Assets/Particles/EnigmaEye{i + 1}", AssetRequestMode.ImmediateLoad);
                
                // Load Glyph variants (12 total) - Universal arcane symbols for all themes
                for (int i = 0; i < 12; i++)
                    Glyphs[i] = ModContent.Request<Texture2D>($"MagnumOpus/Assets/Particles/Glyphs{i + 1}", AssetRequestMode.ImmediateLoad);
                
                TexturesLoaded = true;
            }
            catch { TexturesLoaded = false; }
        }
        
        public override void Unload()
        {
            activeParticles?.Clear();
            particlePool?.Clear();
            activeParticles = null;
            particlePool = null;
            TexturesLoaded = false;
        }
        
        public override void PostUpdateEverything()
        {
            for (int i = activeParticles.Count - 1; i >= 0; i--)
            {
                activeParticles[i].Update();
                if (activeParticles[i].IsDead)
                {
                    particlePool.Enqueue(activeParticles[i]);
                    activeParticles.RemoveAt(i);
                }
            }
        }
        
        public static CustomParticle GetParticle()
        {
            if (particlePool.Count > 0) return particlePool.Dequeue();
            return new CustomParticle();
        }
        
        public static void SpawnParticle(CustomParticle particle)
        {
            if (activeParticles.Count < MaxParticles)
                activeParticles.Add(particle);
        }
        
        public static Asset<Texture2D> RandomFlare() => EnergyFlares[Main.rand.Next(7)];
        public static Asset<Texture2D> RandomGlow() => SoftGlows[Main.rand.Next(4)];
        public static Asset<Texture2D> RandomNote() => MusicNotes[Main.rand.Next(6)];
        // BANNED: GlowingHalo3 (index 2) is excluded - it's the concentric ring texture
        // Only use indices 0, 1, 3, 4, 5
        public static Asset<Texture2D> RandomHalo()
        {
            int[] allowedIndices = { 0, 1, 3, 4, 5 };
            return GlowingHalos[allowedIndices[Main.rand.Next(allowedIndices.Length)]];
        }
        public static Asset<Texture2D> RandomTrail() => ParticleTrails[Main.rand.Next(4)];
        public static Asset<Texture2D> RandomSparkleField() => MagicSparkleFields[Main.rand.Next(12)];
        public static Asset<Texture2D> RandomPrismaticSparkle() => PrismaticSparkles[Main.rand.Next(15)];
        public static Asset<Texture2D> RandomSwordArc() => SwordArcs[Main.rand.Next(9)];
        public static Asset<Texture2D> RandomSwanFeather() => SwanFeathers[Main.rand.Next(10)];
        public static Asset<Texture2D> RandomEnigmaEye() => EnigmaEyes[Main.rand.Next(8)];
        public static Asset<Texture2D> RandomGlyph() => Glyphs[Main.rand.Next(12)];
        public static Asset<Texture2D> GetGlyph(int index) => Glyphs[Math.Clamp(index, 0, 11)];
        public static Asset<Texture2D> GetEnigmaEye(int index) => EnigmaEyes[Math.Clamp(index, 0, 7)];
        
        public static void DrawAllParticles(SpriteBatch spriteBatch)
        {
            if (!TexturesLoaded || activeParticles.Count == 0) return;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, 
                SamplerState.LinearClamp, DepthStencilState.None, 
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            foreach (var p in activeParticles) p.Draw(spriteBatch);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, 
                SamplerState.LinearClamp, DepthStencilState.None, 
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        
        // ================== COLOR PALETTES ==================
        public static class EroicaColors
        {
            public static Color Scarlet => new Color(255, 50, 50);
            public static Color Crimson => new Color(180, 30, 30);
            public static Color Gold => new Color(255, 200, 80);
            public static Color DarkGold => new Color(200, 150, 50);
            public static Color HotCore => new Color(255, 230, 180);
            public static Color Black => new Color(30, 20, 20);
            public static Color Random() => Main.rand.Next(5) switch { 0 => Scarlet, 1 => Crimson, 2 => Gold, 3 => DarkGold, _ => HotCore };
            public static Color Gradient(float t) => Color.Lerp(Scarlet, Gold, t);
        }
        
        public static class MoonlightColors
        {
            public static Color DeepPurple => new Color(80, 40, 140);
            public static Color Violet => new Color(140, 80, 200);
            public static Color Lavender => new Color(180, 150, 255);
            public static Color Silver => new Color(210, 205, 255);
            public static Color MoonWhite => new Color(240, 235, 255);
            public static Color IceBlue => new Color(160, 200, 255);
            public static Color Random() => Main.rand.Next(5) switch { 0 => DeepPurple, 1 => Violet, 2 => Lavender, 3 => Silver, _ => MoonWhite };
            public static Color Gradient(float t) => Color.Lerp(DeepPurple, IceBlue, t);
        }
        
        public static class SwanLakeColors
        {
            public static Color PureWhite => new Color(255, 255, 255);
            public static Color IcyBlue => new Color(180, 220, 255);
            public static Color Silver => new Color(220, 225, 235);
            public static Color PaleCyan => new Color(200, 240, 250);
            public static Color Frost => new Color(230, 245, 255);
            public static Color Random() => Main.rand.Next(4) switch { 0 => PureWhite, 1 => IcyBlue, 2 => Silver, _ => PaleCyan };
            public static Color Gradient(float t) => Color.Lerp(PureWhite, IcyBlue, t);
        }
        
        public static class DiesIraeColors
        {
            public static Color BloodRed => new Color(120, 20, 20);
            public static Color DarkCrimson => new Color(80, 10, 10);
            public static Color Ember => new Color(200, 80, 40);
            public static Color Ash => new Color(60, 50, 50);
            public static Color Hellfire => new Color(255, 100, 50);
            public static Color Random() => Main.rand.Next(4) switch { 0 => BloodRed, 1 => DarkCrimson, 2 => Ember, _ => Ash };
            public static Color Gradient(float t) => Color.Lerp(DarkCrimson, Hellfire, t);
        }
        
        public static class ClairDeLuneColors
        {
            public static Color SoftBlue => new Color(140, 170, 220);
            public static Color Moonbeam => new Color(200, 210, 240);
            public static Color Pearl => new Color(240, 240, 250);
            public static Color NightMist => new Color(100, 120, 160);
            public static Color DreamyBlue => new Color(170, 190, 230);
            public static Color Random() => Main.rand.Next(4) switch { 0 => SoftBlue, 1 => Moonbeam, 2 => Pearl, _ => NightMist };
            public static Color Gradient(float t) => Color.Lerp(NightMist, Pearl, t);
        }
        
        public static class LaCampanellaColors
        {
            public static Color BellGold => new Color(255, 215, 100);
            public static Color Bronze => new Color(180, 140, 80);
            public static Color Shimmer => new Color(255, 240, 180);
            public static Color Chime => new Color(255, 255, 220);
            public static Color Random() => Main.rand.Next(4) switch { 0 => BellGold, 1 => Bronze, 2 => Shimmer, _ => Chime };
            public static Color Gradient(float t) => Color.Lerp(Bronze, Chime, t);
        }
        
        public static class FateColors
        {
            public static Color Destiny => new Color(100, 50, 150);
            public static Color Cosmic => new Color(80, 100, 180);
            public static Color Starlight => new Color(200, 180, 255);
            public static Color Void => new Color(40, 30, 60);
            public static Color Random() => Main.rand.Next(4) switch { 0 => Destiny, 1 => Cosmic, 2 => Starlight, _ => Void };
            public static Color Gradient(float t) => Color.Lerp(Void, Starlight, t);
        }
    }
    
    /// <summary>Pooled particle with all properties for reuse.</summary>
    public class CustomParticle
    {
        public Vector2 Position, Velocity;
        public Color Color, SecondaryColor;
        public float Scale, Rotation, RotationSpeed, Gravity, Drag, ScaleVelocity;
        public int Lifetime, MaxLifetime;
        public Asset<Texture2D> Texture;
        public bool FadeOut, ScaleDown, UseGradient;
        public bool IsDead => Lifetime <= 0;
        
        public CustomParticle() { Reset(); }
        
        public void Reset()
        {
            Position = Velocity = Vector2.Zero;
            Color = SecondaryColor = Color.White;
            Scale = 1f; Rotation = 0f; RotationSpeed = 0f;
            Gravity = 0f; Drag = 0.98f; ScaleVelocity = 0f;
            Lifetime = MaxLifetime = 30;
            Texture = null;
            FadeOut = true; ScaleDown = false; UseGradient = false;
        }
        
        public CustomParticle Setup(Asset<Texture2D> tex, Vector2 pos, Vector2 vel, Color col, float scale, int life, 
            float rotSpeed = 0f, bool fade = true, bool shrink = false)
        {
            Texture = tex; Position = pos; Velocity = vel; Color = col;
            Scale = scale; Lifetime = MaxLifetime = life;
            RotationSpeed = rotSpeed; FadeOut = fade; ScaleDown = shrink;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            Gravity = 0f; Drag = 0.98f; UseGradient = false; ScaleVelocity = 0f;
            return this;
        }
        
        public CustomParticle WithGradient(Color secondary) { SecondaryColor = secondary; UseGradient = true; return this; }
        public CustomParticle WithGravity(float g) { Gravity = g; return this; }
        public CustomParticle WithDrag(float d) { Drag = d; return this; }
        public CustomParticle WithScaleVelocity(float sv) { ScaleVelocity = sv; return this; }
        
        public void Update()
        {
            Position += Velocity;
            Velocity *= Drag;
            Velocity.Y += Gravity;
            Rotation += RotationSpeed;
            Scale += ScaleVelocity;
            Lifetime--;
        }
        
        public void Draw(SpriteBatch sb)
        {
            if (Texture == null || !Texture.IsLoaded) return;
            var tex = Texture.Value;
            var origin = tex.Size() / 2f;
            var drawPos = Position - Main.screenPosition;
            float progress = 1f - (float)Lifetime / MaxLifetime;
            float alpha = FadeOut ? (1f - progress) : 1f;
            float curScale = ScaleDown ? Scale * (1f - progress * 0.5f) : Scale;
            Color drawColor = UseGradient ? Color.Lerp(Color, SecondaryColor, progress) : Color;
            sb.Draw(tex, drawPos, null, drawColor * alpha, Rotation, origin, curScale, SpriteEffects.None, 0f);
        }
    }
    
    /// <summary>Static helper for spawning themed particles easily.</summary>
    public static class CustomParticles
    {
        // ================== EROICA (Scarlet/Gold) ==================
        public static void EroicaImpactBurst(Vector2 pos, int count = 8)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomFlare(), pos, Vector2.Zero,
                CustomParticleSystem.EroicaColors.Gold, 0.6f, 25, 0.02f, true, true);
            CustomParticleSystem.SpawnParticle(p);
            for (int i = 0; i < count; i++)
            {
                var vel = Main.rand.NextVector2Circular(4f, 4f);
                var col = CustomParticleSystem.EroicaColors.Random();
                var particle = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomFlare(), pos, vel,
                    col, 0.2f + Main.rand.NextFloat(0.2f), 20 + Main.rand.Next(15), Main.rand.NextFloat(-0.1f, 0.1f), true, true);
                CustomParticleSystem.SpawnParticle(particle);
            }
        }
        
        public static void EroicaFlare(Vector2 pos, float scale = 0.5f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.SoftGlows[0], pos, Vector2.Zero,
                CustomParticleSystem.EroicaColors.Random(), scale, 20, 0f, true, false);
            CustomParticleSystem.SpawnParticle(p);
        }
        
        public static void EroicaBossAttack(Vector2 pos, int count = 15)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            var center = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.EnergyFlares[0], pos, Vector2.Zero,
                CustomParticleSystem.EroicaColors.HotCore, 1.2f, 35, 0.01f, true, true);
            CustomParticleSystem.SpawnParticle(center);
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                var vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (3f + Main.rand.NextFloat(2f));
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomFlare(), pos, vel,
                    CustomParticleSystem.EroicaColors.Random(), 0.3f + Main.rand.NextFloat(0.3f), 30 + Main.rand.Next(20),
                    Main.rand.NextFloat(-0.05f, 0.05f), true, true);
                CustomParticleSystem.SpawnParticle(p);
            }
            EroicaMusicNotes(pos, 5, 30f);
        }
        
        public static void EroicaMusicNotes(Vector2 pos, int count = 5, float spread = 30f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            for (int i = 0; i < count; i++)
            {
                var offset = Main.rand.NextVector2Circular(spread, spread);
                var vel = new Vector2(Main.rand.NextFloat(-1f, 1f), -1.5f - Main.rand.NextFloat(1f));
                var col = Main.rand.NextBool() ? CustomParticleSystem.EroicaColors.Gold : CustomParticleSystem.EroicaColors.Scarlet;
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomNote(), pos + offset, vel,
                    col, 0.25f + Main.rand.NextFloat(0.15f), 50 + Main.rand.Next(30), Main.rand.NextFloat(-0.03f, 0.03f), true, false)
                    .WithGravity(-0.03f).WithDrag(0.99f);
                CustomParticleSystem.SpawnParticle(p);
            }
        }
        
        public static void EroicaTrailFlare(Vector2 pos, Vector2 vel)
        {
            if (!CustomParticleSystem.TexturesLoaded || !Main.rand.NextBool(2)) return;
            var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomGlow(), pos, -vel * 0.1f,
                CustomParticleSystem.EroicaColors.Gradient(Main.rand.NextFloat()), 0.25f + Main.rand.NextFloat(0.15f), 15, 0f, true, false);
            CustomParticleSystem.SpawnParticle(p);
        }
        
        // ================== MOONLIGHT SONATA (Purple/Silver/IceBlue) ==================
        public static void MoonlightImpactBurst(Vector2 pos, int count = 8)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            var center = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomFlare(), pos, Vector2.Zero,
                CustomParticleSystem.MoonlightColors.MoonWhite, 0.5f, 25, 0.015f, true, true);
            CustomParticleSystem.SpawnParticle(center);
            for (int i = 0; i < count; i++)
            {
                var vel = Main.rand.NextVector2Circular(3.5f, 3.5f);
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomFlare(), pos, vel,
                    CustomParticleSystem.MoonlightColors.Random(), 0.15f + Main.rand.NextFloat(0.2f), 25 + Main.rand.Next(15),
                    Main.rand.NextFloat(-0.08f, 0.08f), true, true).WithGradient(CustomParticleSystem.MoonlightColors.IceBlue);
                CustomParticleSystem.SpawnParticle(p);
            }
        }
        
        public static void MoonlightFlare(Vector2 pos, float scale = 0.5f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.SoftGlows[2], pos, Vector2.Zero,
                CustomParticleSystem.MoonlightColors.Random(), scale, 22, 0f, true, false)
                .WithGradient(CustomParticleSystem.MoonlightColors.Silver);
            CustomParticleSystem.SpawnParticle(p);
        }
        
        public static void MoonlightMusicNotes(Vector2 pos, int count = 5, float spread = 30f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            for (int i = 0; i < count; i++)
            {
                var offset = Main.rand.NextVector2Circular(spread, spread);
                var vel = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), -1f - Main.rand.NextFloat(0.8f));
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomNote(), pos + offset, vel,
                    CustomParticleSystem.MoonlightColors.Random(), 0.2f + Main.rand.NextFloat(0.15f), 60 + Main.rand.Next(30),
                    Main.rand.NextFloat(-0.02f, 0.02f), true, false).WithGravity(-0.02f).WithDrag(0.995f)
                    .WithGradient(CustomParticleSystem.MoonlightColors.Silver);
                CustomParticleSystem.SpawnParticle(p);
            }
        }
        
        public static void MoonlightTrailFlare(Vector2 pos, Vector2 vel)
        {
            if (!CustomParticleSystem.TexturesLoaded || !Main.rand.NextBool(2)) return;
            var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomGlow(), pos, -vel * 0.1f,
                CustomParticleSystem.MoonlightColors.Gradient(Main.rand.NextFloat()), 0.2f + Main.rand.NextFloat(0.15f), 18, 0f, true, false);
            CustomParticleSystem.SpawnParticle(p);
        }
        
        public static void MoonlightBossAttack(Vector2 pos, int count = 15)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            var center = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.EnergyFlares[1], pos, Vector2.Zero,
                CustomParticleSystem.MoonlightColors.MoonWhite, 1.0f, 35, 0.01f, true, true);
            CustomParticleSystem.SpawnParticle(center);
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                var vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (2.5f + Main.rand.NextFloat(2f));
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomFlare(), pos, vel,
                    CustomParticleSystem.MoonlightColors.Random(), 0.25f + Main.rand.NextFloat(0.25f), 35 + Main.rand.Next(20),
                    Main.rand.NextFloat(-0.04f, 0.04f), true, true).WithGradient(CustomParticleSystem.MoonlightColors.IceBlue);
                CustomParticleSystem.SpawnParticle(p);
            }
            MoonlightMusicNotes(pos, 6, 35f);
        }
        
        // ================== SWAN LAKE (White/IcyBlue) ==================
        public static void SwanLakeImpactBurst(Vector2 pos, int count = 8)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            var center = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomFlare(), pos, Vector2.Zero,
                CustomParticleSystem.SwanLakeColors.PureWhite, 0.5f, 28, 0.01f, true, true);
            CustomParticleSystem.SpawnParticle(center);
            for (int i = 0; i < count; i++)
            {
                var vel = Main.rand.NextVector2Circular(3f, 3f);
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomFlare(), pos, vel,
                    CustomParticleSystem.SwanLakeColors.Random(), 0.15f + Main.rand.NextFloat(0.15f), 25 + Main.rand.Next(15),
                    Main.rand.NextFloat(-0.06f, 0.06f), true, true).WithGradient(CustomParticleSystem.SwanLakeColors.IcyBlue);
                CustomParticleSystem.SpawnParticle(p);
            }
        }
        
        public static void SwanLakeFlare(Vector2 pos, float scale = 0.5f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.SoftGlows[3], pos, Vector2.Zero,
                CustomParticleSystem.SwanLakeColors.Random(), scale, 25, 0f, true, false);
            CustomParticleSystem.SpawnParticle(p);
        }
        
        public static void SwanLakeMusicNotes(Vector2 pos, int count = 5, float spread = 30f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            for (int i = 0; i < count; i++)
            {
                var offset = Main.rand.NextVector2Circular(spread, spread);
                var vel = new Vector2(Main.rand.NextFloat(-0.6f, 0.6f), -0.8f - Main.rand.NextFloat(0.6f));
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomNote(), pos + offset, vel,
                    CustomParticleSystem.SwanLakeColors.Random(), 0.2f + Main.rand.NextFloat(0.1f), 70 + Main.rand.Next(30),
                    Main.rand.NextFloat(-0.015f, 0.015f), true, false).WithGravity(-0.015f).WithDrag(0.997f);
                CustomParticleSystem.SpawnParticle(p);
            }
        }
        
        public static void SwanLakeFeatherBurst(Vector2 pos, int count = 10)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.3f, 0.3f);
                var vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (2f + Main.rand.NextFloat(1.5f));
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomGlow(), pos, vel,
                    CustomParticleSystem.SwanLakeColors.Random(), 0.3f + Main.rand.NextFloat(0.2f), 40 + Main.rand.Next(20),
                    Main.rand.NextFloat(-0.02f, 0.02f), true, true).WithGravity(0.02f).WithDrag(0.97f);
                CustomParticleSystem.SpawnParticle(p);
            }
            SwanLakeMusicNotes(pos, 4, 25f);
        }
        
        // ================== DIES IRAE (Blood/Hellfire) ==================
        public static void DiesIraeImpactBurst(Vector2 pos, int count = 8)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            var center = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.EnergyFlares[2], pos, Vector2.Zero,
                CustomParticleSystem.DiesIraeColors.Hellfire, 0.6f, 25, 0.02f, true, true);
            CustomParticleSystem.SpawnParticle(center);
            for (int i = 0; i < count; i++)
            {
                var vel = Main.rand.NextVector2Circular(4.5f, 4.5f);
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomFlare(), pos, vel,
                    CustomParticleSystem.DiesIraeColors.Random(), 0.2f + Main.rand.NextFloat(0.2f), 22 + Main.rand.Next(12),
                    Main.rand.NextFloat(-0.1f, 0.1f), true, true).WithGradient(CustomParticleSystem.DiesIraeColors.Ash);
                CustomParticleSystem.SpawnParticle(p);
            }
        }
        
        public static void DiesIraeMusicNotes(Vector2 pos, int count = 5, float spread = 30f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            for (int i = 0; i < count; i++)
            {
                var offset = Main.rand.NextVector2Circular(spread, spread);
                var vel = new Vector2(Main.rand.NextFloat(-1.2f, 1.2f), -1.8f - Main.rand.NextFloat(1f));
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomNote(), pos + offset, vel,
                    CustomParticleSystem.DiesIraeColors.Random(), 0.25f + Main.rand.NextFloat(0.15f), 45 + Main.rand.Next(25),
                    Main.rand.NextFloat(-0.04f, 0.04f), true, false).WithGravity(-0.04f).WithDrag(0.98f);
                CustomParticleSystem.SpawnParticle(p);
            }
        }
        
        public static void DiesIraeHellfireBurst(Vector2 pos, int count = 12)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                var vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (4f + Main.rand.NextFloat(2f));
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomFlare(), pos, vel,
                    CustomParticleSystem.DiesIraeColors.Hellfire, 0.35f + Main.rand.NextFloat(0.25f), 28 + Main.rand.Next(15),
                    Main.rand.NextFloat(-0.06f, 0.06f), true, true).WithGradient(CustomParticleSystem.DiesIraeColors.BloodRed);
                CustomParticleSystem.SpawnParticle(p);
            }
            DiesIraeMusicNotes(pos, 4, 25f);
        }
        
        // ================== CLAIR DE LUNE (Soft Blue/Pearl) ==================
        public static void ClairDeLuneImpactBurst(Vector2 pos, int count = 8)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            var center = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomFlare(), pos, Vector2.Zero,
                CustomParticleSystem.ClairDeLuneColors.Pearl, 0.45f, 28, 0.01f, true, true);
            CustomParticleSystem.SpawnParticle(center);
            for (int i = 0; i < count; i++)
            {
                var vel = Main.rand.NextVector2Circular(2.5f, 2.5f);
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomFlare(), pos, vel,
                    CustomParticleSystem.ClairDeLuneColors.Random(), 0.12f + Main.rand.NextFloat(0.15f), 30 + Main.rand.Next(15),
                    Main.rand.NextFloat(-0.05f, 0.05f), true, true).WithGradient(CustomParticleSystem.ClairDeLuneColors.DreamyBlue);
                CustomParticleSystem.SpawnParticle(p);
            }
        }
        
        public static void ClairDeLuneMusicNotes(Vector2 pos, int count = 5, float spread = 30f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            for (int i = 0; i < count; i++)
            {
                var offset = Main.rand.NextVector2Circular(spread, spread);
                var vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -0.7f - Main.rand.NextFloat(0.5f));
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomNote(), pos + offset, vel,
                    CustomParticleSystem.ClairDeLuneColors.Random(), 0.18f + Main.rand.NextFloat(0.12f), 75 + Main.rand.Next(35),
                    Main.rand.NextFloat(-0.012f, 0.012f), true, false).WithGravity(-0.012f).WithDrag(0.998f);
                CustomParticleSystem.SpawnParticle(p);
            }
        }
        
        public static void ClairDeLuneDreamyBurst(Vector2 pos, int count = 10)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.4f, 0.4f);
                var vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (1.5f + Main.rand.NextFloat(1.5f));
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomGlow(), pos, vel,
                    CustomParticleSystem.ClairDeLuneColors.Random(), 0.25f + Main.rand.NextFloat(0.2f), 45 + Main.rand.Next(25),
                    Main.rand.NextFloat(-0.015f, 0.015f), true, false).WithGravity(-0.01f).WithDrag(0.985f);
                CustomParticleSystem.SpawnParticle(p);
            }
            ClairDeLuneMusicNotes(pos, 5, 30f);
        }
        
        // ================== LA CAMPANELLA (Bell Gold/Shimmer) ==================
        public static void LaCampanellaImpactBurst(Vector2 pos, int count = 8)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            var center = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.EnergyFlares[3], pos, Vector2.Zero,
                CustomParticleSystem.LaCampanellaColors.Chime, 0.55f, 25, 0.015f, true, true);
            CustomParticleSystem.SpawnParticle(center);
            for (int i = 0; i < count; i++)
            {
                var vel = Main.rand.NextVector2Circular(3.5f, 3.5f);
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomFlare(), pos, vel,
                    CustomParticleSystem.LaCampanellaColors.Random(), 0.15f + Main.rand.NextFloat(0.18f), 24 + Main.rand.Next(14),
                    Main.rand.NextFloat(-0.08f, 0.08f), true, true).WithGradient(CustomParticleSystem.LaCampanellaColors.Shimmer);
                CustomParticleSystem.SpawnParticle(p);
            }
        }
        
        public static void LaCampanellaMusicNotes(Vector2 pos, int count = 5, float spread = 30f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            for (int i = 0; i < count; i++)
            {
                var offset = Main.rand.NextVector2Circular(spread, spread);
                var vel = new Vector2(Main.rand.NextFloat(-0.9f, 0.9f), -1.2f - Main.rand.NextFloat(0.8f));
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomNote(), pos + offset, vel,
                    CustomParticleSystem.LaCampanellaColors.Random(), 0.22f + Main.rand.NextFloat(0.14f), 55 + Main.rand.Next(30),
                    Main.rand.NextFloat(-0.025f, 0.025f), true, false).WithGravity(-0.025f).WithDrag(0.992f);
                CustomParticleSystem.SpawnParticle(p);
            }
        }
        
        public static void LaCampanellaBellChime(Vector2 pos, int count = 12)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            // Radial shimmer
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                var vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (3f + Main.rand.NextFloat(1.5f));
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomFlare(), pos, vel,
                    CustomParticleSystem.LaCampanellaColors.BellGold, 0.2f + Main.rand.NextFloat(0.15f), 30 + Main.rand.Next(15),
                    Main.rand.NextFloat(-0.04f, 0.04f), true, true).WithGradient(CustomParticleSystem.LaCampanellaColors.Shimmer);
                CustomParticleSystem.SpawnParticle(p);
            }
            LaCampanellaMusicNotes(pos, 6, 35f);
        }
        
        // ================== FATE (Cosmic/Destiny) ==================
        public static void FateImpactBurst(Vector2 pos, int count = 8)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            var center = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.EnergyFlares[4], pos, Vector2.Zero,
                CustomParticleSystem.FateColors.Starlight, 0.5f, 28, 0.012f, true, true);
            CustomParticleSystem.SpawnParticle(center);
            for (int i = 0; i < count; i++)
            {
                var vel = Main.rand.NextVector2Circular(3f, 3f);
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomFlare(), pos, vel,
                    CustomParticleSystem.FateColors.Random(), 0.15f + Main.rand.NextFloat(0.18f), 28 + Main.rand.Next(15),
                    Main.rand.NextFloat(-0.06f, 0.06f), true, true).WithGradient(CustomParticleSystem.FateColors.Cosmic);
                CustomParticleSystem.SpawnParticle(p);
            }
        }
        
        public static void FateMusicNotes(Vector2 pos, int count = 5, float spread = 30f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            for (int i = 0; i < count; i++)
            {
                var offset = Main.rand.NextVector2Circular(spread, spread);
                var vel = new Vector2(Main.rand.NextFloat(-0.7f, 0.7f), -0.9f - Main.rand.NextFloat(0.7f));
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomNote(), pos + offset, vel,
                    CustomParticleSystem.FateColors.Random(), 0.2f + Main.rand.NextFloat(0.12f), 65 + Main.rand.Next(30),
                    Main.rand.NextFloat(-0.018f, 0.018f), true, false).WithGravity(-0.018f).WithDrag(0.995f);
                CustomParticleSystem.SpawnParticle(p);
            }
        }
        
        public static void FateCosmicBurst(Vector2 pos, int count = 12)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.2f, 0.2f);
                var vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (2.5f + Main.rand.NextFloat(2f));
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomFlare(), pos, vel,
                    CustomParticleSystem.FateColors.Destiny, 0.25f + Main.rand.NextFloat(0.2f), 35 + Main.rand.Next(20),
                    Main.rand.NextFloat(-0.03f, 0.03f), true, true).WithGradient(CustomParticleSystem.FateColors.Starlight);
                CustomParticleSystem.SpawnParticle(p);
            }
            FateMusicNotes(pos, 5, 30f);
        }
        
        // ================== GENERIC UTILITIES ==================
        public static void GenericFlare(Vector2 pos, Color col, float scale = 0.5f, int life = 25)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomFlare(), pos, Vector2.Zero,
                col, scale, life, 0.01f, true, true);
            CustomParticleSystem.SpawnParticle(p);
        }
        
        public static void GenericGlow(Vector2 pos, Color col, float scale = 0.5f, int life = 20)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomGlow(), pos, Vector2.Zero,
                col, scale, life, 0f, true, false);
            CustomParticleSystem.SpawnParticle(p);
        }
        
        /// <summary>
        /// GenericGlow with velocity and optional fade parameter.
        /// Used for trail effects and moving glow particles.
        /// </summary>
        public static void GenericGlow(Vector2 pos, Vector2 velocity, Color col, float scale = 0.5f, int life = 20, bool fade = false)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomGlow(), pos, velocity,
                col, scale, life, 0f, fade, false);
            CustomParticleSystem.SpawnParticle(p);
        }
        
        public static void GenericMusicNotes(Vector2 pos, Color col, int count = 3, float spread = 20f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            for (int i = 0; i < count; i++)
            {
                var offset = Main.rand.NextVector2Circular(spread, spread);
                var vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f - Main.rand.NextFloat(0.5f));
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomNote(), pos + offset, vel,
                    col, 0.2f + Main.rand.NextFloat(0.1f), 50 + Main.rand.Next(20), Main.rand.NextFloat(-0.02f, 0.02f), true, false)
                    .WithGravity(-0.02f).WithDrag(0.995f);
                CustomParticleSystem.SpawnParticle(p);
            }
        }
        
        public static void ExplosionBurst(Vector2 pos, Color col, int count = 12, float speed = 5f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            var center = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.EnergyFlares[0], pos, Vector2.Zero,
                Color.White, 0.8f, 15, 0f, true, true);
            CustomParticleSystem.SpawnParticle(center);
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.2f, 0.2f);
                var vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (speed + Main.rand.NextFloat(speed * 0.5f));
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomFlare(), pos, vel,
                    col, 0.25f + Main.rand.NextFloat(0.2f), 25 + Main.rand.Next(15), Main.rand.NextFloat(-0.05f, 0.05f), true, true)
                    .WithDrag(0.95f);
                CustomParticleSystem.SpawnParticle(p);
            }
        }
        
        public static void GlowTrail(Vector2 pos, Color col, float scale = 0.3f)
        {
            if (!CustomParticleSystem.TexturesLoaded || !Main.rand.NextBool(2)) return;
            var offset = Main.rand.NextVector2Circular(5f, 5f);
            var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomGlow(), pos + offset, Vector2.Zero,
                col, scale * (0.7f + Main.rand.NextFloat(0.6f)), 15 + Main.rand.Next(10), 0f, true, false);
            CustomParticleSystem.SpawnParticle(p);
        }
        
        public static void MusicalImpact(Vector2 pos, Color flareCol, Color noteCol, float intensity = 1f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            var center = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomFlare(), pos, Vector2.Zero,
                flareCol, 0.5f * intensity, 25, 0.02f, true, true);
            CustomParticleSystem.SpawnParticle(center);
            int noteCount = (int)(3 * intensity) + 2;
            for (int i = 0; i < noteCount; i++)
            {
                var offset = Main.rand.NextVector2Circular(20f * intensity, 20f * intensity);
                var vel = new Vector2(Main.rand.NextFloat(-1f, 1f), -1.5f - Main.rand.NextFloat(1f));
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomNote(), pos + offset, vel,
                    noteCol, 0.2f + Main.rand.NextFloat(0.15f), 45 + Main.rand.Next(25), Main.rand.NextFloat(-0.03f, 0.03f), true, false)
                    .WithGravity(-0.025f).WithDrag(0.995f);
                CustomParticleSystem.SpawnParticle(p);
            }
        }
        
        // =========================================
        // HALO EFFECTS - Radiant rings and auras
        // =========================================
        
        /// <summary>Spawn a pulsing halo ring around a position</summary>
        public static void HaloRing(Vector2 pos, Color col, float scale = 0.5f, int lifetime = 30)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomHalo(), pos, Vector2.Zero,
                col, scale, lifetime, 0.01f, true, true);
            CustomParticleSystem.SpawnParticle(p);
        }
        
        /// <summary>Eroica-themed expanding golden halo</summary>
        public static void EroicaHalo(Vector2 pos, float scale = 0.6f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            var col = Color.Lerp(CustomParticleSystem.EroicaColors.Scarlet, CustomParticleSystem.EroicaColors.Gold, Main.rand.NextFloat());
            var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.GlowingHalos[1], pos, Vector2.Zero,
                col, scale * (0.8f + Main.rand.NextFloat(0.4f)), 35, 0.015f, true, true)
                .WithScaleVelocity(0.02f);
            CustomParticleSystem.SpawnParticle(p);
        }
        
        /// <summary>Moonlight-themed ethereal silver halo</summary>
        public static void MoonlightHalo(Vector2 pos, float scale = 0.5f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            var col = Color.Lerp(CustomParticleSystem.MoonlightColors.DeepPurple, CustomParticleSystem.MoonlightColors.Silver, Main.rand.NextFloat());
            // BANNED: GlowingHalos[3] was the concentric ring texture - using GlowingHalos[1] instead
            var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.GlowingHalos[1], pos, Vector2.Zero,
                col, scale * (0.7f + Main.rand.NextFloat(0.5f)), 40, 0.008f, true, true)
                .WithScaleVelocity(0.015f);
            CustomParticleSystem.SpawnParticle(p);
        }
        
        /// <summary>Swan Lake pearlescent halo with chromatic shimmer</summary>
        public static void SwanLakeHalo(Vector2 pos, float scale = 0.6f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            var col = Color.Lerp(CustomParticleSystem.SwanLakeColors.PureWhite, CustomParticleSystem.SwanLakeColors.IcyBlue, Main.rand.NextFloat(0.3f));
            col = Color.Lerp(col, Color.White, 0.3f);
            var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.GlowingHalos[5], pos, Vector2.Zero,
                col, scale * (0.9f + Main.rand.NextFloat(0.3f)), 45, 0.005f, true, true)
                .WithScaleVelocity(0.025f);
            CustomParticleSystem.SpawnParticle(p);
        }
        
        /// <summary>Multi-layered halo burst for impacts</summary>
        public static void HaloBurst(Vector2 pos, Color primary, Color secondary, float intensity = 1f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            // Inner bright halo
            var inner = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomHalo(), pos, Vector2.Zero,
                Color.Lerp(primary, Color.White, 0.5f), 0.3f * intensity, 20, 0.02f, true, true)
                .WithScaleVelocity(0.04f);
            CustomParticleSystem.SpawnParticle(inner);
            // Outer expanding halo
            var outer = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomHalo(), pos, Vector2.Zero,
                secondary, 0.5f * intensity, 35, 0.01f, true, true)
                .WithScaleVelocity(0.03f);
            CustomParticleSystem.SpawnParticle(outer);
        }
        
        // =========================================
        // TRAIL EFFECTS - Motion trails and ribbons
        // =========================================
        
        /// <summary>Spawn a single trail particle following movement</summary>
        public static void TrailSegment(Vector2 pos, Vector2 velocity, Color col, float scale = 0.3f)
        {
            if (!CustomParticleSystem.TexturesLoaded || !Main.rand.NextBool(2)) return;
            var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomTrail(), pos, velocity * 0.1f,
                col, scale * (0.8f + Main.rand.NextFloat(0.4f)), 20 + Main.rand.Next(10), 0f, true, true)
                .WithDrag(0.92f);
            CustomParticleSystem.SpawnParticle(p);
        }
        
        /// <summary>Eroica-themed blazing trail effect</summary>
        public static void EroicaTrail(Vector2 pos, Vector2 velocity, float scale = 0.35f)
        {
            if (!CustomParticleSystem.TexturesLoaded || !Main.rand.NextBool(3)) return;
            var col = Color.Lerp(CustomParticleSystem.EroicaColors.Crimson, CustomParticleSystem.EroicaColors.Gold, Main.rand.NextFloat(0.7f));
            var offset = Main.rand.NextVector2Circular(3f, 3f);
            // Trail faces left in PNG - rotate to point opposite of travel direction (trail behind projectile)
            var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomTrail(), pos + offset, -velocity * 0.08f,
                col, scale * (0.7f + Main.rand.NextFloat(0.5f)), 18 + Main.rand.Next(12), 0f, true, true)
                .WithDrag(0.9f);
            p.Rotation = velocity.ToRotation() + MathHelper.Pi; // Point opposite of travel (trail behind)
            CustomParticleSystem.SpawnParticle(p);
        }
        
        /// <summary>Moonlight-themed ethereal wispy trail</summary>
        public static void MoonlightTrail(Vector2 pos, Vector2 velocity, float scale = 0.3f)
        {
            if (!CustomParticleSystem.TexturesLoaded || !Main.rand.NextBool(3)) return;
            var col = Color.Lerp(CustomParticleSystem.MoonlightColors.IceBlue, CustomParticleSystem.MoonlightColors.Silver, Main.rand.NextFloat());
            var offset = Main.rand.NextVector2Circular(4f, 4f);
            // Trail faces left in PNG - rotate to point opposite of travel direction (trail behind projectile)
            var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomTrail(), pos + offset, -velocity * 0.05f,
                col, scale * (0.6f + Main.rand.NextFloat(0.5f)), 25 + Main.rand.Next(15), 0f, true, true)
                .WithDrag(0.94f);
            p.Rotation = velocity.ToRotation() + MathHelper.Pi; // Point opposite of travel (trail behind)
            CustomParticleSystem.SpawnParticle(p);
        }
        
        /// <summary>Swan Lake chromatic ribbon trail</summary>
        public static void SwanLakeTrail(Vector2 pos, Vector2 velocity, float scale = 0.4f)
        {
            if (!CustomParticleSystem.TexturesLoaded || !Main.rand.NextBool(2)) return;
            // Chromatic color shift
            float hue = (float)(Main.GameUpdateCount % 360) / 360f + Main.rand.NextFloat(0.1f);
            var chromatic = Main.hslToRgb(hue, 0.6f, 0.85f);
            var col = Color.Lerp(CustomParticleSystem.SwanLakeColors.PureWhite, chromatic, 0.4f);
            var offset = Main.rand.NextVector2Circular(5f, 5f);
            // Trail faces left in PNG - rotate to point opposite of travel direction (trail behind projectile)
            var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomTrail(), pos + offset, -velocity * 0.06f,
                col, scale * (0.8f + Main.rand.NextFloat(0.4f)), 30 + Main.rand.Next(20), 0f, true, true)
                .WithDrag(0.93f);
            p.Rotation = velocity.ToRotation() + MathHelper.Pi; // Point opposite of travel (trail behind)
            CustomParticleSystem.SpawnParticle(p);
        }
        
        /// <summary>Dense trail burst for dashing/teleporting</summary>
        public static void TrailBurst(Vector2 pos, Vector2 direction, Color col, int count = 8, float spread = 0.5f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            for (int i = 0; i < count; i++)
            {
                float angle = direction.ToRotation() + Main.rand.NextFloat(-spread, spread);
                var vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (2f + Main.rand.NextFloat(3f));
                var offset = Main.rand.NextVector2Circular(8f, 8f);
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomTrail(), pos + offset, vel,
                    col * (0.7f + Main.rand.NextFloat(0.3f)), 0.25f + Main.rand.NextFloat(0.2f), 20 + Main.rand.Next(15), 
                    Main.rand.NextFloat(-0.03f, 0.03f), true, true)
                    .WithDrag(0.88f);
                CustomParticleSystem.SpawnParticle(p);
            }
        }
        
        // =========================================
        // COMBINED EFFECTS - Halos + Trails + More
        // =========================================
        
        /// <summary>Grand impact with halo ring and particle burst</summary>
        public static void GrandImpact(Vector2 pos, Color primary, Color secondary, float intensity = 1f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            // Central halo burst
            HaloBurst(pos, primary, secondary, intensity);
            // Surrounding particles
            int count = (int)(6 * intensity) + 4;
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.3f, 0.3f);
                var vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (3f + Main.rand.NextFloat(4f)) * intensity;
                var col = Color.Lerp(primary, secondary, Main.rand.NextFloat());
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomFlare(), pos, vel,
                    col, 0.2f + Main.rand.NextFloat(0.15f), 25 + Main.rand.Next(15), Main.rand.NextFloat(-0.04f, 0.04f), true, true)
                    .WithDrag(0.92f);
                CustomParticleSystem.SpawnParticle(p);
            }
        }
        
        /// <summary>Eroica grand finale - heroic burst with halos, trails, and notes</summary>
        public static void EroicaGrandFinale(Vector2 pos, float intensity = 1f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            // Expanding golden halos
            for (int i = 0; i < 3; i++)
            {
                var delay = i * 5;
                var haloCol = Color.Lerp(CustomParticleSystem.EroicaColors.Gold, CustomParticleSystem.EroicaColors.Scarlet, i * 0.3f);
                var halo = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomHalo(), pos, Vector2.Zero,
                    haloCol, (0.3f + i * 0.2f) * intensity, 30 + i * 10, 0.02f - i * 0.005f, true, true)
                    .WithScaleVelocity(0.025f + i * 0.01f);
                CustomParticleSystem.SpawnParticle(halo);
            }
            // Radiating trails
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                var vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 4f * intensity;
                var trailCol = Color.Lerp(CustomParticleSystem.EroicaColors.Crimson, CustomParticleSystem.EroicaColors.Gold, Main.rand.NextFloat());
                var trail = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomTrail(), pos, vel,
                    trailCol, 0.35f * intensity, 35, Main.rand.NextFloat(-0.02f, 0.02f), true, true)
                    .WithDrag(0.9f);
                CustomParticleSystem.SpawnParticle(trail);
            }
            // Musical notes rising
            EroicaMusicNotes(pos, (int)(4 * intensity) + 2);
        }
        
        /// <summary>Moonlight crescendo - ethereal burst with silver halos and wispy trails</summary>
        public static void MoonlightCrescendo(Vector2 pos, float intensity = 1f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            // Layered silver halos
            for (int i = 0; i < 2; i++)
            {
                var haloCol = Color.Lerp(CustomParticleSystem.MoonlightColors.Silver, CustomParticleSystem.MoonlightColors.DeepPurple, i * 0.4f);
                var halo = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomHalo(), pos, Vector2.Zero,
                    haloCol, (0.4f + i * 0.25f) * intensity, 40 + i * 15, 0.008f, true, true)
                    .WithScaleVelocity(0.02f);
                CustomParticleSystem.SpawnParticle(halo);
            }
            // Drifting ethereal trails
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f + Main.rand.NextFloat(-0.2f, 0.2f);
                var vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 2f * intensity;
                vel.Y -= 0.5f; // Slight upward drift
                var trailCol = Color.Lerp(CustomParticleSystem.MoonlightColors.IceBlue, CustomParticleSystem.MoonlightColors.Silver, Main.rand.NextFloat());
                var trail = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomTrail(), pos, vel,
                    trailCol, 0.3f * intensity, 45, Main.rand.NextFloat(-0.01f, 0.01f), true, true)
                    .WithDrag(0.95f).WithGravity(-0.01f);
                CustomParticleSystem.SpawnParticle(trail);
            }
            // Floating notes
            MoonlightMusicNotes(pos, (int)(3 * intensity) + 2);
        }
        
        /// <summary>Swan Lake aurora - pearlescent halos with chromatic shifting trails</summary>
        public static void SwanLakeAurora(Vector2 pos, float intensity = 1f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            // Pearlescent core halo
            var coreHalo = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomHalo(), pos, Vector2.Zero,
                Color.Lerp(CustomParticleSystem.SwanLakeColors.PureWhite, Color.White, 0.5f), 0.5f * intensity, 50, 0.005f, true, true)
                .WithScaleVelocity(0.03f);
            CustomParticleSystem.SpawnParticle(coreHalo);
            // Chromatic outer halos
            for (int i = 0; i < 3; i++)
            {
                float hue = (float)(Main.GameUpdateCount % 360) / 360f + i * 0.33f;
                var chromaCol = Main.hslToRgb(hue, 0.5f, 0.9f);
                var offset = Main.rand.NextVector2Circular(10f, 10f);
                var halo = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomHalo(), pos + offset, Vector2.Zero,
                    chromaCol * 0.7f, (0.3f + i * 0.1f) * intensity, 35, 0.01f, true, true)
                    .WithScaleVelocity(0.02f);
                CustomParticleSystem.SpawnParticle(halo);
            }
            // Graceful feather-like trails
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                var vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 1.5f * intensity;
                float trailHue = (float)(Main.GameUpdateCount % 360) / 360f + i * 0.125f;
                var trailCol = Color.Lerp(CustomParticleSystem.SwanLakeColors.PureWhite, Main.hslToRgb(trailHue, 0.4f, 0.85f), 0.3f);
                var trail = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomTrail(), pos, vel,
                    trailCol, 0.35f * intensity, 40, Main.rand.NextFloat(-0.008f, 0.008f), true, true)
                    .WithDrag(0.96f);
                CustomParticleSystem.SpawnParticle(trail);
            }
        }
        
        // ================== MAGIC SPARKLE FIELD EFFECTS ==================
        
        /// <summary>Spawn a magic sparkle field aura around a position - perfect for buff indicators</summary>
        public static void MagicSparkleFieldAura(Vector2 pos, Color color, float scale = 0.5f, int lifetime = 40)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomSparkleField(), pos, Vector2.Zero,
                color, scale, lifetime, Main.rand.NextFloat(-0.02f, 0.02f), true, true);
            CustomParticleSystem.SpawnParticle(p);
        }
        
        /// <summary>Spawn multiple sparkle fields in a circular pattern - enchantment activation</summary>
        public static void MagicSparkleFieldBurst(Vector2 pos, Color color, int count = 6, float radius = 30f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.1f, 0.1f);
                var offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius * Main.rand.NextFloat(0.5f, 1f);
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomSparkleField(), pos + offset, 
                    offset * 0.02f, color * Main.rand.NextFloat(0.7f, 1f), 0.3f + Main.rand.NextFloat(0.2f), 
                    30 + Main.rand.Next(20), Main.rand.NextFloat(-0.03f, 0.03f), true, true);
                CustomParticleSystem.SpawnParticle(p);
            }
        }
        
        /// <summary>Rising sparkle field column - buff activation/magic channeling</summary>
        public static void MagicSparkleFieldRising(Vector2 pos, Color color, int count = 8)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            for (int i = 0; i < count; i++)
            {
                var offset = new Vector2(Main.rand.NextFloat(-20f, 20f), Main.rand.NextFloat(-10f, 10f));
                var vel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -1f - Main.rand.NextFloat(0.5f));
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomSparkleField(), pos + offset, vel,
                    color * Main.rand.NextFloat(0.6f, 1f), 0.2f + Main.rand.NextFloat(0.2f), 35 + Main.rand.Next(15),
                    Main.rand.NextFloat(-0.02f, 0.02f), true, true).WithDrag(0.97f).WithGravity(-0.02f);
                CustomParticleSystem.SpawnParticle(p);
            }
        }
        
        // ================== PRISMATIC SPARKLE EFFECTS ==================
        
        /// <summary>Single prismatic sparkle - gem flash, treasure highlight</summary>
        public static void PrismaticSparkle(Vector2 pos, Color color, float scale = 0.4f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomPrismaticSparkle(), pos, Vector2.Zero,
                color, scale, 25 + Main.rand.Next(10), Main.rand.NextFloat(-0.05f, 0.05f), true, true);
            CustomParticleSystem.SpawnParticle(p);
        }
        
        /// <summary>Prismatic sparkle cluster - gem break, treasure discovery</summary>
        public static void PrismaticSparkleBurst(Vector2 pos, Color baseColor, int count = 10)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            for (int i = 0; i < count; i++)
            {
                var vel = Main.rand.NextVector2Circular(3f, 3f);
                float hueShift = Main.rand.NextFloat(-0.1f, 0.1f);
                var color = Main.hslToRgb((Main.rgbToHsl(baseColor).X + hueShift) % 1f, 0.8f, 0.9f);
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomPrismaticSparkle(), pos, vel,
                    color, 0.15f + Main.rand.NextFloat(0.2f), 20 + Main.rand.Next(15), 
                    Main.rand.NextFloat(-0.08f, 0.08f), true, true).WithDrag(0.93f);
                CustomParticleSystem.SpawnParticle(p);
            }
        }
        
        /// <summary>Rainbow prismatic effect - cycles through hues</summary>
        public static void PrismaticSparkleRainbow(Vector2 pos, int count = 12)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                var vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (2f + Main.rand.NextFloat(2f));
                float hue = (float)i / count;
                var color = Main.hslToRgb(hue, 1f, 0.7f);
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomPrismaticSparkle(), pos, vel,
                    color, 0.25f + Main.rand.NextFloat(0.15f), 30 + Main.rand.Next(15), 
                    Main.rand.NextFloat(-0.04f, 0.04f), true, true).WithDrag(0.95f);
                CustomParticleSystem.SpawnParticle(p);
            }
        }
        
        /// <summary>Ambient prismatic sparkles - floating gem dust</summary>
        public static void PrismaticSparkleAmbient(Vector2 pos, Color color, float radius = 40f, int count = 5)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            for (int i = 0; i < count; i++)
            {
                var offset = Main.rand.NextVector2Circular(radius, radius);
                var vel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-0.5f, -0.2f));
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomPrismaticSparkle(), pos + offset, vel,
                    color * Main.rand.NextFloat(0.5f, 1f), 0.1f + Main.rand.NextFloat(0.15f), 40 + Main.rand.Next(20),
                    Main.rand.NextFloat(-0.03f, 0.03f), true, false).WithDrag(0.98f);
                CustomParticleSystem.SpawnParticle(p);
            }
        }
        
        // ================== SWORD ARC EFFECTS ==================
        
        /// <summary>Single sword arc slash - projectile visual</summary>
        public static void SwordArcSlash(Vector2 pos, Vector2 direction, Color color, float scale = 0.5f, float rotation = 0f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomSwordArc(), pos, direction * 0.5f,
                color, scale, 20, 0f, true, false);
            p.Rotation = rotation != 0f ? rotation : direction.ToRotation();
            CustomParticleSystem.SpawnParticle(p);
        }
        
        /// <summary>Traveling sword arc wave - for projectile weapons</summary>
        public static void SwordArcWave(Vector2 pos, Vector2 velocity, Color color, float scale = 0.6f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomSwordArc(), pos, velocity,
                color, scale, 30, 0f, true, true);
            p.Rotation = velocity.ToRotation();
            p.Drag = 0.98f;
            CustomParticleSystem.SpawnParticle(p);
        }
        
        /// <summary>Sword arc burst - melee impact, omnidirectional slashes</summary>
        public static void SwordArcBurst(Vector2 pos, Color color, int count = 6, float scale = 0.4f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.2f, 0.2f);
                var vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (2f + Main.rand.NextFloat(1.5f));
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomSwordArc(), pos, vel,
                    color * Main.rand.NextFloat(0.8f, 1f), scale * Main.rand.NextFloat(0.8f, 1.2f), 
                    25 + Main.rand.Next(10), 0f, true, true);
                p.Rotation = angle;
                p.Drag = 0.92f;
                CustomParticleSystem.SpawnParticle(p);
            }
        }
        
        /// <summary>Crescent moon slash - classic arcing projectile</summary>
        public static void SwordArcCrescent(Vector2 pos, Vector2 direction, Color color, float scale = 0.7f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            // Main crescent
            var main = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.SwordArcs[0], pos, direction,
                color, scale, 35, 0f, true, true);
            main.Rotation = direction.ToRotation();
            main.Drag = 0.96f;
            CustomParticleSystem.SpawnParticle(main);
            
            // Trailing afterimage
            for (int i = 0; i < 3; i++)
            {
                var offset = -direction * (i + 1) * 5f;
                var trail = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomSwordArc(), pos + offset, direction * 0.3f,
                    color * (0.6f - i * 0.15f), scale * (0.9f - i * 0.1f), 25 - i * 5, 0f, true, true);
                trail.Rotation = direction.ToRotation();
                trail.Drag = 0.94f;
                CustomParticleSystem.SpawnParticle(trail);
            }
        }
        
        /// <summary>Double helix sword arcs - intertwined projectile slashes</summary>
        public static void SwordArcDoubleHelix(Vector2 pos, Vector2 direction, Color color1, Color color2, float scale = 0.5f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            float baseAngle = direction.ToRotation();
            for (int i = 0; i < 2; i++)
            {
                float offset = i == 0 ? 0.3f : -0.3f;
                var offsetDir = direction.RotatedBy(offset);
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomSwordArc(), pos, offsetDir * direction.Length(),
                    i == 0 ? color1 : color2, scale, 30, 0f, true, true);
                p.Rotation = baseAngle + offset;
                p.Drag = 0.97f;
                CustomParticleSystem.SpawnParticle(p);
            }
        }
        
        /// <summary>Spiraling sword arc vortex - spinning projectile attack</summary>
        public static void SwordArcVortex(Vector2 pos, Color color, int count = 4, float scale = 0.45f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            float baseRot = (float)(Main.GameUpdateCount % 360) * 0.1f;
            for (int i = 0; i < count; i++)
            {
                float angle = baseRot + MathHelper.TwoPi * i / count;
                var vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 3f;
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomSwordArc(), pos, vel,
                    color, scale, 25, 0.15f, true, true);
                p.Rotation = angle + MathHelper.PiOver2;
                p.Drag = 0.9f;
                CustomParticleSystem.SpawnParticle(p);
            }
        }
        
        // ================== SWAN LAKE FEATHER EFFECTS (EXCLUSIVE) ==================
        
        /// <summary>Single floating swan feather - elegant drift effect</summary>
        public static void SwanFeatherDrift(Vector2 pos, Color color, float scale = 0.4f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            var vel = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(0.3f, 1f));
            var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomSwanFeather(), pos, vel,
                color, scale, 80 + Main.rand.Next(40), Main.rand.NextFloat(-0.02f, 0.02f), true, false)
                .WithGravity(0.01f).WithDrag(0.98f);
            CustomParticleSystem.SpawnParticle(p);
        }
        
        /// <summary>Feather burst - elegant scatter for impacts and effects</summary>
        public static void SwanFeatherBurst(Vector2 pos, int count = 8, float scale = 0.35f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.3f, 0.3f);
                var vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (2f + Main.rand.NextFloat(1.5f));
                // Alternate between white and slight rainbow tint
                Color col = Main.rand.NextBool(3) 
                    ? Color.Lerp(CustomParticleSystem.SwanLakeColors.PureWhite, Main.hslToRgb(Main.rand.NextFloat(), 0.3f, 0.9f), 0.3f)
                    : CustomParticleSystem.SwanLakeColors.PureWhite;
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomSwanFeather(), pos, vel,
                    col, scale * Main.rand.NextFloat(0.8f, 1.2f), 60 + Main.rand.Next(30),
                    Main.rand.NextFloat(-0.03f, 0.03f), true, false)
                    .WithGravity(0.015f).WithDrag(0.96f);
                CustomParticleSystem.SpawnParticle(p);
            }
        }
        
        /// <summary>Feather trail - leaves behind floating feathers</summary>
        public static void SwanFeatherTrail(Vector2 pos, Vector2 velocity, float scale = 0.3f)
        {
            if (!CustomParticleSystem.TexturesLoaded || !Main.rand.NextBool(2)) return;
            var vel = -velocity * 0.2f + new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-0.3f, 0.3f));
            Color col = Main.rand.NextBool() ? CustomParticleSystem.SwanLakeColors.PureWhite : CustomParticleSystem.SwanLakeColors.Silver;
            var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomSwanFeather(), pos, vel,
                col, scale * Main.rand.NextFloat(0.7f, 1f), 50 + Main.rand.Next(30),
                Main.rand.NextFloat(-0.02f, 0.02f), true, false)
                .WithGravity(0.012f).WithDrag(0.97f);
            CustomParticleSystem.SpawnParticle(p);
        }
        
        /// <summary>Feather aura - gentle floating feathers around player/entity (smaller and more translucent)</summary>
        public static void SwanFeatherAura(Vector2 center, float radius = 40f, int count = 3)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            for (int i = 0; i < count; i++)
            {
                var offset = Main.rand.NextVector2Circular(radius, radius);
                var vel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-0.8f, -0.2f));
                // Chromatic feathers with pearlescent shimmer - more translucent
                float hue = (float)(Main.GameUpdateCount % 360) / 360f + Main.rand.NextFloat(0.1f);
                Color chromaCol = Main.hslToRgb(hue, 0.3f, 0.85f);
                Color col = Color.Lerp(CustomParticleSystem.SwanLakeColors.PureWhite, chromaCol, 0.25f) * 0.5f; // 50% opacity
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomSwanFeather(), center + offset, vel,
                    col, 0.12f + Main.rand.NextFloat(0.08f), 50 + Main.rand.Next(30), // Much smaller scale (0.12-0.2 vs 0.25-0.4)
                    Main.rand.NextFloat(-0.015f, 0.015f), true, false)
                    .WithGravity(-0.005f).WithDrag(0.99f);
                CustomParticleSystem.SpawnParticle(p);
            }
        }
        
        /// <summary>Feather explosion - dramatic burst for major impacts</summary>
        public static void SwanFeatherExplosion(Vector2 pos, int count = 15, float scale = 0.45f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.4f, 0.4f);
                float speed = 3f + Main.rand.NextFloat(4f);
                var vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                // Rainbow spectrum feathers
                float hue = (float)i / count;
                Color col = Color.Lerp(Color.White, Main.hslToRgb(hue, 0.7f, 0.85f), 0.4f);
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomSwanFeather(), pos, vel,
                    col, scale * Main.rand.NextFloat(0.8f, 1.3f), 70 + Main.rand.Next(40),
                    Main.rand.NextFloat(-0.04f, 0.04f), true, false)
                    .WithGravity(0.02f).WithDrag(0.94f);
                CustomParticleSystem.SpawnParticle(p);
            }
        }
        
        /// <summary>Black and white feather duality - alternating black/white feathers</summary>
        public static void SwanFeatherDuality(Vector2 pos, int count = 6, float scale = 0.35f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.2f, 0.2f);
                var vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (1.5f + Main.rand.NextFloat(1.5f));
                // Alternate black and white
                Color col = i % 2 == 0 ? Color.White : new Color(30, 30, 35);
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomSwanFeather(), pos, vel,
                    col, scale * Main.rand.NextFloat(0.9f, 1.1f), 55 + Main.rand.Next(25),
                    Main.rand.NextFloat(-0.025f, 0.025f), true, false)
                    .WithGravity(0.015f).WithDrag(0.96f);
                CustomParticleSystem.SpawnParticle(p);
            }
        }
        
        /// <summary>Feather spiral - elegant spiraling feather ascension</summary>
        public static void SwanFeatherSpiral(Vector2 pos, Color color, int count = 8)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            float baseRot = (float)(Main.GameUpdateCount % 360) * 0.05f;
            for (int i = 0; i < count; i++)
            {
                float angle = baseRot + MathHelper.TwoPi * i / count;
                float dist = 5f + i * 3f;
                var offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * dist;
                var vel = new Vector2((float)Math.Cos(angle + MathHelper.PiOver2), (float)Math.Sin(angle + MathHelper.PiOver2)) * 1f;
                vel.Y -= 1.5f; // Rising spiral
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomSwanFeather(), pos + offset, vel,
                    color * Main.rand.NextFloat(0.8f, 1f), 0.3f + Main.rand.NextFloat(0.15f), 50 + Main.rand.Next(20),
                    0.05f, true, false)
                    .WithGravity(-0.02f).WithDrag(0.97f);
                CustomParticleSystem.SpawnParticle(p);
            }
        }
        
        // ================== ENIGMA EYE EFFECTS (MYSTERIOUS WATCHING EYES) ==================
        // IMPORTANT: Eyes should have MEANINGFUL placements - at impact points, watching targets,
        // near enemies, NOT scattered randomly. They represent the unknown observing.
        
        /// <summary>Single mysterious eye - appears at impact point watching the struck target</summary>
        public static void EnigmaEyeGaze(Vector2 pos, Color color, float scale = 0.5f, Vector2? lookDirection = null)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomEnigmaEye(), pos, Vector2.Zero,
                color, scale, 45 + Main.rand.Next(20), 0f, true, true);
            // If look direction provided, rotate to face that direction
            if (lookDirection.HasValue && lookDirection.Value != Vector2.Zero)
                p.Rotation = lookDirection.Value.ToRotation();
            CustomParticleSystem.SpawnParticle(p);
        }
        
        /// <summary>Eye appears at impact watching the target - MEANINGFUL placement</summary>
        public static void EnigmaEyeImpact(Vector2 impactPos, Vector2 targetPos, Color color, float scale = 0.6f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            Vector2 lookDir = targetPos - impactPos;
            var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomEnigmaEye(), impactPos, Vector2.Zero,
                color, scale, 50 + Main.rand.Next(25), 0f, true, true);
            p.Rotation = lookDir != Vector2.Zero ? lookDir.ToRotation() : Main.rand.NextFloat(MathHelper.TwoPi);
            CustomParticleSystem.SpawnParticle(p);
            // Secondary flare behind the eye
            GenericFlare(impactPos, color * 0.5f, scale * 0.4f, 30);
        }
        
        /// <summary>Multiple eyes in formation watching a central point - for AOE effects</summary>
        public static void EnigmaEyeFormation(Vector2 centerPos, Color color, int count = 3, float radius = 50f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.1f, 0.1f);
                var offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.GetEnigmaEye(i % 8), centerPos + offset, Vector2.Zero,
                    color * Main.rand.NextFloat(0.8f, 1f), 0.4f + Main.rand.NextFloat(0.2f), 55 + Main.rand.Next(20), 0f, true, true);
                // Eyes look toward center
                p.Rotation = (-offset).ToRotation();
                CustomParticleSystem.SpawnParticle(p);
            }
        }
        
        /// <summary>Eye trail - eyes appear along path, each watching the next point</summary>
        public static void EnigmaEyeTrail(Vector2 pos, Vector2 velocity, Color color, float scale = 0.3f)
        {
            if (!CustomParticleSystem.TexturesLoaded || !Main.rand.NextBool(5)) return;
            var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomEnigmaEye(), pos, Vector2.Zero,
                color * 0.8f, scale, 40 + Main.rand.Next(20), 0f, true, true);
            p.Rotation = velocity.ToRotation();
            CustomParticleSystem.SpawnParticle(p);
        }
        
        /// <summary>Eye burst that all look outward - explosion of awareness</summary>
        public static void EnigmaEyeExplosion(Vector2 pos, Color color, int count = 6, float speed = 3f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            // Central intense flare
            GenericFlare(pos, color, 0.8f, 30);
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.15f, 0.15f);
                var vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (speed + Main.rand.NextFloat(speed * 0.3f));
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomEnigmaEye(), pos, vel,
                    color * Main.rand.NextFloat(0.8f, 1f), 0.35f + Main.rand.NextFloat(0.2f), 40 + Main.rand.Next(20), 0f, true, true)
                    .WithDrag(0.94f);
                p.Rotation = angle; // Eyes look in direction they're moving
                CustomParticleSystem.SpawnParticle(p);
            }
        }
        
        /// <summary>Orbiting eyes - eyes slowly orbit around an entity, always watching outward</summary>
        public static void EnigmaEyeOrbit(Vector2 center, Color color, int count = 4, float radius = 45f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            float baseAngle = (float)(Main.GameUpdateCount % 360) * 0.02f;
            for (int i = 0; i < count; i++)
            {
                float angle = baseAngle + MathHelper.TwoPi * i / count;
                var offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.GetEnigmaEye(i % 8), center + offset, Vector2.Zero,
                    color * Main.rand.NextFloat(0.7f, 1f), 0.3f + Main.rand.NextFloat(0.15f), 8, 0f, true, false);
                p.Rotation = angle; // Looking outward
                CustomParticleSystem.SpawnParticle(p);
            }
        }
        
        // ================== GLYPH EFFECTS (UNIVERSAL ARCANE SYMBOLS) ==================
        // Glyphs can be used for ANY theme. They represent arcane power, debuff stacking,
        // magic circles, enchantments, and mysterious runes.
        
        /// <summary>Single arcane glyph - generic magical symbol</summary>
        public static void Glyph(Vector2 pos, Color color, float scale = 0.5f, int glyphIndex = -1)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            var tex = glyphIndex >= 0 ? CustomParticleSystem.GetGlyph(glyphIndex) : CustomParticleSystem.RandomGlyph();
            var p = CustomParticleSystem.GetParticle().Setup(tex, pos, Vector2.Zero,
                color, scale, 40 + Main.rand.Next(20), Main.rand.NextFloat(-0.01f, 0.01f), true, true);
            CustomParticleSystem.SpawnParticle(p);
        }
        
        /// <summary>Glyph stack indicator - shows debuff/buff stacks with layered glyphs</summary>
        public static void GlyphStack(Vector2 pos, Color color, int stackCount, float baseScale = 0.3f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            int displayCount = Math.Min(stackCount, 5); // Max 5 visible glyphs
            for (int i = 0; i < displayCount; i++)
            {
                float angle = MathHelper.TwoPi * i / displayCount;
                float radius = 15f + displayCount * 3f;
                var offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                float individualScale = baseScale + (stackCount * 0.02f); // Bigger with more stacks
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.GetGlyph(i % 12), pos + offset, Vector2.Zero,
                    color * (0.7f + i * 0.06f), individualScale, 35 + i * 5, 0.02f + i * 0.005f, true, true);
                CustomParticleSystem.SpawnParticle(p);
            }
            // Central glow intensifies with stacks
            GenericGlow(pos, color, 0.3f + stackCount * 0.05f, 30);
        }
        
        /// <summary>Magic circle - rotating glyphs forming a protective/offensive circle</summary>
        public static void GlyphCircle(Vector2 pos, Color color, int count = 6, float radius = 40f, float rotationSpeed = 0.03f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            float baseAngle = (float)(Main.GameUpdateCount % 360) * rotationSpeed;
            for (int i = 0; i < count; i++)
            {
                float angle = baseAngle + MathHelper.TwoPi * i / count;
                var offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.GetGlyph(i % 12), pos + offset, Vector2.Zero,
                    color * Main.rand.NextFloat(0.8f, 1f), 0.35f + Main.rand.NextFloat(0.1f), 12, 0f, true, false);
                p.Rotation = angle + MathHelper.PiOver2;
                CustomParticleSystem.SpawnParticle(p);
            }
            // Central connecting halo
            HaloRing(pos, color * 0.4f, radius / 80f, 15);
        }
        
        /// <summary>Glyph burst - exploding arcane symbols</summary>
        public static void GlyphBurst(Vector2 pos, Color color, int count = 8, float speed = 3f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            // Central flare
            GenericFlare(pos, color, 0.7f, 25);
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.2f, 0.2f);
                var vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (speed + Main.rand.NextFloat(speed * 0.4f));
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomGlyph(), pos, vel,
                    color * Main.rand.NextFloat(0.75f, 1f), 0.3f + Main.rand.NextFloat(0.2f), 35 + Main.rand.Next(15),
                    Main.rand.NextFloat(-0.05f, 0.05f), true, true).WithDrag(0.92f);
                CustomParticleSystem.SpawnParticle(p);
            }
        }
        
        /// <summary>Glyph trail - glyphs left behind as projectile travels</summary>
        public static void GlyphTrail(Vector2 pos, Vector2 velocity, Color color, float scale = 0.25f)
        {
            if (!CustomParticleSystem.TexturesLoaded || !Main.rand.NextBool(4)) return;
            var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomGlyph(), pos, -velocity * 0.05f,
                color * 0.7f, scale, 30 + Main.rand.Next(15), Main.rand.NextFloat(-0.03f, 0.03f), true, true)
                .WithDrag(0.96f);
            CustomParticleSystem.SpawnParticle(p);
        }
        
        /// <summary>Glyph aura - floating glyphs around an entity for ambient magic</summary>
        public static void GlyphAura(Vector2 center, Color color, float radius = 35f, int count = 2)
        {
            if (!CustomParticleSystem.TexturesLoaded || !Main.rand.NextBool(4)) return;
            for (int i = 0; i < count; i++)
            {
                var offset = Main.rand.NextVector2Circular(radius, radius);
                var vel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-0.5f, -0.2f));
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomGlyph(), center + offset, vel,
                    color * Main.rand.NextFloat(0.4f, 0.7f), 0.15f + Main.rand.NextFloat(0.1f), 50 + Main.rand.Next(30),
                    Main.rand.NextFloat(-0.02f, 0.02f), true, true).WithDrag(0.98f).WithGravity(-0.005f);
                CustomParticleSystem.SpawnParticle(p);
            }
        }
        
        /// <summary>Glyph impact - meaningful glyph at point of impact</summary>
        public static void GlyphImpact(Vector2 pos, Color primary, Color secondary, float scale = 0.6f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            // Main glyph
            var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomGlyph(), pos, Vector2.Zero,
                primary, scale, 40, 0.01f, true, true).WithGradient(secondary);
            CustomParticleSystem.SpawnParticle(p);
            // Smaller supporting glyphs
            for (int i = 0; i < 3; i++)
            {
                float angle = MathHelper.TwoPi * i / 3f + Main.rand.NextFloat(-0.2f, 0.2f);
                var offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 25f;
                var small = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.RandomGlyph(), pos + offset, -offset * 0.02f,
                    secondary * 0.7f, scale * 0.5f, 30, Main.rand.NextFloat(-0.03f, 0.03f), true, true);
                CustomParticleSystem.SpawnParticle(small);
            }
            // Background halo
            HaloRing(pos, primary * 0.5f, scale * 0.6f, 25);
        }
        
        /// <summary>Multi-layered glyph tower - stacking visual for powerful effects</summary>
        public static void GlyphTower(Vector2 pos, Color color, int layers = 4, float baseScale = 0.4f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            for (int i = 0; i < layers; i++)
            {
                float yOffset = -i * 12f;
                float layerScale = baseScale * (1f - i * 0.15f);
                float alpha = 1f - i * 0.2f;
                var p = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.GetGlyph(i % 12), 
                    pos + new Vector2(0, yOffset), new Vector2(0, -0.3f),
                    color * alpha, layerScale, 40 + i * 8, 0.01f + i * 0.005f, true, true)
                    .WithGravity(-0.01f);
                CustomParticleSystem.SpawnParticle(p);
            }
        }
        
        // ================== ENIGMA THEME COLORS ==================
        public static class EnigmaColors
        {
            public static Color Black => new Color(15, 10, 20);
            public static Color DeepPurple => new Color(80, 20, 120);
            public static Color Purple => new Color(140, 60, 200);
            public static Color GreenFlame => new Color(50, 220, 100);
            public static Color DarkGreen => new Color(30, 100, 50);
            public static Color Random() => Main.rand.Next(5) switch { 0 => Black, 1 => DeepPurple, 2 => Purple, 3 => GreenFlame, _ => DarkGreen };
            public static Color Gradient(float t)
            {
                // Black → Purple → Green flame transition
                if (t < 0.5f)
                    return Color.Lerp(DeepPurple, Purple, t * 2f);
                return Color.Lerp(Purple, GreenFlame, (t - 0.5f) * 2f);
            }
        }
    }
}
