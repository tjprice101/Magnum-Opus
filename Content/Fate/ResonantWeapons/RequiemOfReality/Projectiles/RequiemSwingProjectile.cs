using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.ResonantWeapons.RequiemOfReality.Utilities;
using MagnumOpus.Content.Fate.ResonantWeapons.RequiemOfReality.Particles;
using MagnumOpus.Content.Fate.ResonantWeapons.RequiemOfReality.Primitives;
using MagnumOpus.Content.Fate.ResonantWeapons.RequiemOfReality.Shaders;
using MagnumOpus.Content.Fate.Debuffs;
using MagnumOpus.Content.Fate.ResonantWeapons.RequiemOfReality.Projectiles;

namespace MagnumOpus.Content.Fate.ResonantWeapons.RequiemOfReality.Projectiles
{
    /// <summary>
    /// Requiem of Reality — Main swing projectile.
    /// 
    /// ATTACK SYSTEM: 4-movement combo cycle (musical movements)
    ///   Movement I   (Adagio):    Slow wide horizontal sweep — wide arc, moderate damage
    ///   Movement II  (Allegro):   Fast diagonal upswing — narrow arc, fast
    ///   Movement III (Scherzo):   Spin slash — 270° whirlwind with particles
    ///   Movement IV  (Finale):    Overhead slam — narrow vertical, heavy damage + combo trigger
    ///
    /// 5-LAYER RENDERING (inspired by EternalMoon):
    ///   Layer 1: Wide cosmic glow underlayer (RequiemSwingGlow shader)
    ///   Layer 2: Core trail arc (RequiemSwingTrail shader + noise texture)
    ///   Layer 3: Constellation spark accents along the arc
    ///   Layer 4: UV-rotated weapon sprite + tip lens flare
    ///   Layer 5: Combo intensity aura (RequiemComboAura when combo >= 3)
    /// </summary>
    public class RequiemSwingProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/RequiemOfReality";

        // Swing arc parameters per movement (from Resonance Weapons Planning doc)
        private static readonly float[] ArcAngles = { 160f, 120f, 270f, 100f };  // Degrees
        private static readonly float[] SwingDurations = { 30f, 22f, 18f, 26f }; // Frames — Adagio(slow), Allegro(quick), Scherzo(wild), Finale(devastating)
        private static readonly float[] DamageMultipliers = { 1f, 0.9f, 0.8f, 1.5f };

        // Trail system
        private Vector2[] _trailPoints = new Vector2[24];
        private int _trailCount;
        private float _currentAngle;
        private float _startAngle;
        private int _direction; // 1 or -1
        private int _movement; // 0-3

        // Textures (lazy loaded)
        private static Asset<Texture2D> _noiseTex;
        private static Asset<Texture2D> _glowTex;
        private static Asset<Texture2D> _flareTex;
        private static Asset<Texture2D> _celestialGlyphTex;
        private static Asset<Texture2D> _supernovaTex;
        private static Asset<Texture2D> _fateTrailTex;
        private static Asset<Texture2D> _fateImpactTex;

        // Properties
        private Player Owner => Main.player[Projectile.owner];
        private float SwingProgress => Projectile.ai[1] / SwingDurations[_movement];
        private float ArcRadians => MathHelper.ToRadians(ArcAngles[_movement]);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 24;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1; // One hit per swing
            Projectile.extraUpdates = 1; // Smoother motion
        }

        public override void OnSpawn(IEntitySource source)
        {
            var rp = Owner.Requiem();
            _movement = rp.MusicalMovement;

            // Direction: alternate based on swing count, but Scherzo always clockwise
            _direction = _movement == 2 ? 1 : (rp.SwingCounter % 2 == 0 ? 1 : -1);

            // Start angle: aim toward mouse, offset by half arc
            Vector2 toMouse = (Main.MouseWorld - Owner.Center).SafeNormalize(Vector2.UnitX);
            _startAngle = toMouse.ToRotation() - ArcRadians * 0.5f * _direction;
            _currentAngle = _startAngle;

            // Set appropriate timeLeft
            Projectile.timeLeft = (int)(SwingDurations[_movement] * 2) + 4; // extra updates double it

            // Scale damage by movement multiplier
            Projectile.damage = (int)(Projectile.damage * DamageMultipliers[_movement]);
        }

        public override void AI()
        {
            // Keep attached to player
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;
            Projectile.Center = Owner.MountedCenter;

            float duration = SwingDurations[_movement] * 2f; // doubled by extraUpdates
            Projectile.ai[1]++;

            if (Projectile.ai[1] >= duration)
            {
                Projectile.Kill();
                return;
            }

            float progress = Projectile.ai[1] / duration;

            // Swing easing per movement (from Resonance Weapons Planning doc)
            float easedProgress;
            switch (_movement)
            {
                case 0: // Adagio: smooth, mournful — SineInOut
                    easedProgress = RequiemUtils.SineInOut(progress);
                    break;
                case 1: // Allegro: quick, desperate — QuadOut
                    easedProgress = RequiemUtils.QuadOut(progress);
                    break;
                case 2: // Scherzo: wild, spinning — ExpOut (fast start, decelerating)
                    easedProgress = RequiemUtils.ExpOut(progress);
                    break;
                case 3: // Finale: devastating overhead slam — BackOut overshoot
                    easedProgress = RequiemUtils.BackOut(progress);
                    break;
                default:
                    easedProgress = progress;
                    break;
            }

            // Calculate current angle
            _currentAngle = _startAngle + ArcRadians * easedProgress * _direction;

            // Weapon reach (longer on finale)
            float reach = _movement == 3 ? 95f : 78f;

            // Set rotation for collision and rendering
            Projectile.rotation = _currentAngle;
            Vector2 tipPos = Owner.MountedCenter + _currentAngle.ToRotationVector2() * reach;

            // Update trail
            if (_trailCount < _trailPoints.Length)
            {
                _trailPoints[_trailCount] = tipPos;
                _trailCount++;
            }
            else
            {
                Array.Copy(_trailPoints, 1, _trailPoints, 0, _trailPoints.Length - 1);
                _trailPoints[_trailPoints.Length - 1] = tipPos;
            }

            // Player direction
            Owner.ChangeDir(Math.Sign((_currentAngle.ToRotationVector2()).X));

            // Spawn particles along arc
            SpawnSwingParticles(tipPos, progress);

            // Lighting
            Color lightCol = RequiemUtils.PaletteLerp(progress);
            Lighting.AddLight(tipPos, lightCol.ToVector3() * 0.7f);

            // Sound at midpoint
            if ((int)Projectile.ai[1] == (int)(duration * 0.3f))
            {
                SoundStyle sound = _movement == 3
                    ? SoundID.Item71 with { Pitch = -0.3f, Volume = 0.9f }
                    : SoundID.Item1 with { Pitch = 0.3f + _movement * 0.15f, Volume = 0.7f };
                SoundEngine.PlaySound(sound, Owner.Center);
            }
        }

        private void SpawnSwingParticles(Vector2 tipPos, float progress)
        {
            if (Main.dedServ) return;

            var rp = Owner.Requiem();
            float intensity = 0.5f + rp.ComboIntensity * 0.5f;

            // Cosmic sparks at blade tip
            if (Main.rand.NextBool(2))
            {
                Vector2 sparkVel = (_currentAngle + MathHelper.PiOver2 * _direction).ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Color sparkCol = RequiemUtils.GetCosmicGradient(Main.rand.NextFloat());
                RequiemParticleHandler.SpawnParticle(new RequiemSparkParticle(
                    tipPos + Main.rand.NextVector2Circular(5f, 5f), sparkVel,
                    sparkCol, 0.25f * intensity, 14));
            }

            // Music notes scatter (more at higher combo)
            if (Main.rand.NextBool(rp.ComboIntensity > 0.5f ? 3 : 6))
            {
                Vector2 noteVel = Main.rand.NextVector2Circular(2f, 2f);
                noteVel.Y -= 1.5f; // Float upward
                Color noteCol = RequiemUtils.PaletteLerp(Main.rand.NextFloat(0.3f, 0.8f));
                RequiemParticleHandler.SpawnParticle(new RequiemNoteParticle(
                    tipPos + Main.rand.NextVector2Circular(10f, 10f), noteVel,
                    noteCol, 0.3f, 30));
            }

            // Nebula wisps during Scherzo spin
            if (_movement == 2 && Main.rand.NextBool(3))
            {
                Vector2 wispVel = Main.rand.NextVector2Circular(1f, 1f);
                Color wispCol = Color.Lerp(RequiemUtils.FatePurple, RequiemUtils.NebulaMist, Main.rand.NextFloat());
                RequiemParticleHandler.SpawnParticle(new RequiemNebulaWisp(
                    tipPos + Main.rand.NextVector2Circular(15f, 15f), wispVel,
                    wispCol, 0.2f, 40));
            }

            // Finale: heavy bloom flares near impact
            if (_movement == 3 && progress > 0.7f && Main.rand.NextBool(2))
            {
                Color flareCol = RequiemUtils.WithWhitePush(RequiemUtils.BrightCrimson, progress - 0.7f);
                RequiemParticleHandler.SpawnParticle(new RequiemBloomFlare(
                    tipPos, flareCol, 0.4f * intensity, 12));
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);

            // === SPECTRAL RESONANCE STACKING ===
            target.AddBuff(ModContent.BuffType<SpectralResonance>(), 120);
            var resonanceNPC = target.GetGlobalNPC<SpectralResonanceNPC>();
            bool maxReached = resonanceNPC.AddStack(damageDone);
            if (maxReached && !Main.dedServ)
            {
                // Visual indicator: bright crimson pulse ring at max stacks
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 vel = angle.ToRotationVector2() * 3f;
                    RequiemParticleHandler.SpawnParticle(new RequiemBloomFlare(
                        target.Center + vel * 5f, RequiemUtils.BrightCrimson, 0.3f, 18));
                }
            }

            // === REALITY TEAR (15% chance) ===
            if (Main.rand.NextFloat() < 0.15f)
            {
                // Spawn a reality tear projectile behind the enemy
                Vector2 tearPos = target.Center + Main.rand.NextVector2Circular(15f, 15f);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), tearPos, Vector2.Zero,
                    ModContent.ProjectileType<RequiemRealityTear>(),
                    (int)(Projectile.damage * 0.3f), 0f, Projectile.owner);

                // Chromatic spark burst at tear spawn
                if (!Main.dedServ)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        Color sparkCol = i % 3 == 0 ? RequiemUtils.BrightCrimson :
                                         i % 3 == 1 ? RequiemUtils.DarkPink : RequiemUtils.FatePurple;
                        Vector2 sparkVel = Main.rand.NextVector2Circular(4f, 4f);
                        RequiemParticleHandler.SpawnParticle(new RequiemSparkParticle(
                            tearPos, sparkVel, sparkCol, 0.2f, 12));
                    }
                }
            }

            SpawnImpactVFX(target.Center);
        }

        private void SpawnImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            var rp = Owner.Requiem();
            float intensity = 0.6f + rp.ComboIntensity * 0.4f;

            // Central bloom flash
            RequiemParticleHandler.SpawnParticle(new RequiemBloomFlare(
                pos, RequiemUtils.SupernovaWhite, 0.6f * intensity, 15));
            RequiemParticleHandler.SpawnParticle(new RequiemBloomFlare(
                pos, RequiemUtils.BrightCrimson, 0.45f * intensity, 12));

            // Radial spark burst (8-12 sparks)
            int sparkCount = 8 + (int)(rp.ComboIntensity * 4);
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = MathHelper.TwoPi * i / sparkCount + Main.rand.NextFloat(-0.1f, 0.1f);
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f) * intensity;
                Color sparkCol = RequiemUtils.GetCosmicGradient((float)i / sparkCount);
                RequiemParticleHandler.SpawnParticle(new RequiemSparkParticle(
                    pos, sparkVel, sparkCol, 0.3f * intensity, 16));
            }

            // Glyph accents (combo triggers extra glyphs)
            int glyphCount = 2 + (int)(rp.ComboIntensity * 3);
            for (int i = 0; i < glyphCount; i++)
            {
                Vector2 glyphPos = pos + Main.rand.NextVector2Circular(20f, 20f);
                Color glyphCol = RequiemUtils.PaletteLerp(Main.rand.NextFloat());
                RequiemParticleHandler.SpawnParticle(new RequiemGlyphParticle(
                    glyphPos, glyphCol, 0.28f * intensity, 25));
            }

            // Music notes on impact
            for (int i = 0; i < 3; i++)
            {
                Vector2 noteVel = Main.rand.NextVector2Circular(3f, 3f);
                noteVel.Y -= 2f;
                Color noteCol = RequiemUtils.PaletteLerp(Main.rand.NextFloat(0.3f, 0.8f));
                RequiemParticleHandler.SpawnParticle(new RequiemNoteParticle(
                    pos + Main.rand.NextVector2Circular(10f, 10f), noteVel,
                    noteCol, 0.35f, 28));
            }

            Lighting.AddLight(pos, RequiemUtils.BrightCrimson.ToVector3() * 1.0f * intensity);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Line collision from player center to blade tip
            float reach = _movement == 3 ? 95f : 78f;
            Vector2 start = Owner.MountedCenter;
            Vector2 end = start + _currentAngle.ToRotationVector2() * reach;
            float _ = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 30f, ref _);
        }

        // ======================== 5-LAYER RENDERING ========================

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ || _trailCount < 2) return false;

            // Lazy load textures
            _noiseTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/TileableFBMNoise");
            _glowTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow");
            _flareTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/StarFlare");
            _celestialGlyphTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Fate/Particles/FA Celestial Glyph");
            _supernovaTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Fate/Particles/FA Supernova Core");
            _fateTrailTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Fate/Trails and Ribbons/FA Basic Trail");
            _fateImpactTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Fate/Impact Effects/FA Harmonic Resonance Wave Impact");

            SpriteBatch sb = Main.spriteBatch;
            var rp = Owner.Requiem();
            float comboIntensity = rp.ComboIntensity;
            float progress = SwingProgress;

            try
            {
                // End SpriteBatch before GPU primitive trail draws
                sb.End();

                // GPU primitive layers (trail renderers use DrawUserIndexedPrimitives)
                DrawLayer1_CosmicGlow(sb, comboIntensity);
                DrawLayer2_CoreTrail(sb, comboIntensity);

                // Restart SpriteBatch for sprite-based layers
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                // Sprite-based layers (manage their own additive state changes)
                DrawLayer3_ConstellationSparks(sb, progress, comboIntensity);
                DrawLayer4_WeaponSprite(sb, lightColor);
                DrawLayer5_ComboAura(sb, comboIntensity);
            }
            catch
            {
                try
                {
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                        DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
                catch { }
            }

            // Theme accents (additive pass)
            try
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                RequiemUtils.DrawThemeAccents(sb, Projectile.Center, 1f, 0.4f);
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            catch { }

            return false;
        }

        /// <summary>Layer 1: Wide soft cosmic glow underlayer via shader.</summary>
        private void DrawLayer1_CosmicGlow(SpriteBatch sb, float combo)
        {
            var shader = RequiemShaderLoader.GetSwingGlow();
            if (shader == null || _trailCount < 2) return;

            try
            {
                shader.UseColor(RequiemUtils.CosmicVoid.ToVector3());
                shader.UseSecondaryColor(RequiemUtils.FatePurple.ToVector3());
                shader.Shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 2f);
                shader.Shader.Parameters["uOpacity"]?.SetValue(0.4f + combo * 0.2f);
                shader.Shader.Parameters["uIntensity"]?.SetValue(1.2f);
                shader.Shader.Parameters["uOverbrightMult"]?.SetValue(1.5f);

                RequiemTrailRenderer.RenderTrail(_trailPoints, new RequiemTrailSettings(
                    (p, _) => (40f + combo * 15f) * (1f - p * 0.5f), // Wide, tapers
                    (p) => Color.Lerp(RequiemUtils.Additive(RequiemUtils.FatePurple, 0.3f),
                                      RequiemUtils.Additive(RequiemUtils.CosmicVoid, 0.1f), p),
                    shader: shader), _trailCount, 3);
            }
            catch { }
        }

        /// <summary>Layer 2: Core trail arc with cosmic fire shader.</summary>
        private void DrawLayer2_CoreTrail(SpriteBatch sb, float combo)
        {
            var shader = RequiemShaderLoader.GetSwingTrail();
            if (shader == null || _trailCount < 2) return;

            try
            {
                if (_noiseTex?.Value != null)
                    shader.UseImage1(_noiseTex);

                shader.UseColor(RequiemUtils.BrightCrimson.ToVector3());
                shader.UseSecondaryColor(RequiemUtils.DarkPink.ToVector3());
                shader.Shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 3f);
                shader.Shader.Parameters["uOpacity"]?.SetValue(0.8f + combo * 0.2f);
                shader.Shader.Parameters["uIntensity"]?.SetValue(1.5f + combo * 0.5f);
                shader.Shader.Parameters["uOverbrightMult"]?.SetValue(1.8f);
                shader.Shader.Parameters["uScrollSpeed"]?.SetValue(1.2f);
                shader.Shader.Parameters["uNoiseScale"]?.SetValue(2.5f);
                shader.Shader.Parameters["uPhase"]?.SetValue(combo);
                shader.Shader.Parameters["uHasSecondaryTex"]?.SetValue(_noiseTex?.Value != null ? 1f : 0f);
                shader.Shader.Parameters["uSecondaryTexScale"]?.SetValue(3f);

                RequiemTrailRenderer.RenderTrail(_trailPoints, new RequiemTrailSettings(
                    (p, _) =>
                    {
                        float baseWidth = 22f + combo * 8f;
                        // Tapered: fat in middle, thin at ends
                        float taper = MathF.Sin(p * MathHelper.Pi);
                        return baseWidth * (0.3f + taper * 0.7f);
                    },
                    (p) =>
                    {
                        Color c = RequiemUtils.GetCosmicGradient(p);
                        float alpha = (1f - p * 0.6f);
                        return RequiemUtils.Additive(c, alpha);
                    },
                    shader: shader), _trailCount, 3);
            }
            catch { }
        }

        /// <summary>Layer 3: Ink droplet accents — bleeding splotches that drip and fade like spilled ink along the arc.</summary>
        private void DrawLayer3_ConstellationSparks(SpriteBatch sb, float progress, float combo)
        {
            if (_flareTex?.Value == null || _trailCount < 3) return;

            try
            {
                RequiemUtils.BeginAdditive(sb);

                var tex = _flareTex.Value;
                Vector2 origin = tex.Size() / 2f;
                float time = (float)Main.timeForVisualEffects;

                for (int i = 1; i < _trailCount - 1; i += 2)
                {
                    float t = (float)i / _trailCount;
                    float sparkAlpha = (1f - t) * (0.3f + combo * 0.4f);

                    // Slow, mournful pulse (not twinkling — dirge-like)
                    float dirge = MathF.Sin(time * 0.08f + i * 1.3f) * 0.2f + 0.8f;
                    sparkAlpha *= dirge;

                    if (sparkAlpha < 0.05f) continue;

                    // Ink-dark crimson splotches — darker than other weapons
                    Color inkColor = Color.Lerp(RequiemUtils.CosmicVoid, RequiemUtils.BrightCrimson, 0.3f + t * 0.3f);
                    Vector2 drawPos = _trailPoints[i] - Main.screenPosition;

                    // Gravity drip: ink droplets slowly drift downward over time
                    float dripOffset = MathF.Sin(i * 2.1f) * 3f + t * 5f;
                    drawPos.Y += dripOffset;

                    // Main ink splotch (larger, irregular scale for ink-blob feel)
                    float splotchScale = (0.2f + combo * 0.12f) * (0.8f + MathF.Sin(i * 3.7f) * 0.3f);
                    sb.Draw(tex, drawPos, null, RequiemUtils.Additive(inkColor, sparkAlpha * 0.8f),
                        i * 1.5f, origin, splotchScale, SpriteEffects.None, 0f);

                    // Faint crimson bleed halo around each splotch
                    sb.Draw(tex, drawPos, null, RequiemUtils.Additive(RequiemUtils.BrightCrimson, sparkAlpha * 0.25f),
                        i * 1.5f, origin, splotchScale * 1.8f, SpriteEffects.None, 0f);

                    // Every 3rd node: fading note silhouette (musical staff ghost)
                    if (i % 3 == 0)
                    {
                        float noteAlpha = sparkAlpha * 0.35f * (1f - t);
                        float noteScale = 0.1f + combo * 0.05f;
                        Color noteColor = RequiemUtils.Additive(RequiemUtils.FatePurple, noteAlpha);
                        // Slight rotation gives each "note" a hand-written feel
                        float noteRot = MathF.Sin(time * 0.05f + i * 2.3f) * 0.3f;
                        sb.Draw(tex, drawPos + new Vector2(0, -4f), null, noteColor,
                            noteRot, origin, noteScale, SpriteEffects.None, 0f);
                    }
                }

                RequiemUtils.EndAdditive(sb);
            }
            catch
            {
                try { RequiemUtils.EndAdditive(sb); } catch { }
            }
        }

        /// <summary>Layer 4: UV-rotated weapon sprite at current angle + tip lens flare.</summary>
        private void DrawLayer4_WeaponSprite(SpriteBatch sb, Color lightColor)
        {
            try
            {
                Texture2D weaponTex = TextureAssets.Projectile[Projectile.type].Value;
                Vector2 origin = weaponTex.Size() / 2f;
                float reach = _movement == 3 ? 95f : 78f;
                Vector2 drawPos = Owner.MountedCenter + _currentAngle.ToRotationVector2() * reach * 0.5f - Main.screenPosition;

                // Ghostly tint
                Color weaponColor = Color.Lerp(lightColor, RequiemUtils.ConstellationSilver, 0.3f);

                // Draw weapon with rotation
                float drawRotation = _currentAngle + MathHelper.PiOver4; // Diagonal alignment
                SpriteEffects fx = _direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                sb.Draw(weaponTex, drawPos, null, weaponColor, drawRotation, origin, Projectile.scale, fx, 0f);

                // Tip lens flare (additive)
                if (_glowTex?.Value != null)
                {
                    Vector2 tipPos = Owner.MountedCenter + _currentAngle.ToRotationVector2() * reach - Main.screenPosition;
                    float flarePulse = 0.8f + MathF.Sin((float)Main.timeForVisualEffects * 0.08f) * 0.2f;

                    RequiemUtils.BeginAdditive(sb);
                    sb.Draw(_glowTex.Value, tipPos, null, RequiemUtils.Additive(RequiemUtils.BrightCrimson, 0.5f * flarePulse),
                        0f, _glowTex.Value.Size() / 2f, 0.35f * flarePulse, SpriteEffects.None, 0f);
                    sb.Draw(_glowTex.Value, tipPos, null, RequiemUtils.Additive(RequiemUtils.SupernovaWhite, 0.3f * flarePulse),
                        0f, _glowTex.Value.Size() / 2f, 0.18f * flarePulse, SpriteEffects.None, 0f);
                    RequiemUtils.EndAdditive(sb);
                }
            }
            catch
            {
                try { RequiemUtils.EndAdditive(sb); } catch { }
            }
        }

        /// <summary>Layer 5: Combo aura — ink pooling effect: dark splotches that bleed outward from the player.</summary>
        private void DrawLayer5_ComboAura(SpriteBatch sb, float combo)
        {
            if (combo < 0.5f) return;
            if (_glowTex?.Value == null) return;

            try
            {
                float auraAlpha = (combo - 0.5f) * 2f; // 0→1 over 0.5→1.0
                float time = (float)Main.timeForVisualEffects;

                RequiemUtils.BeginAdditive(sb);

                Vector2 center = Owner.MountedCenter - Main.screenPosition;
                var tex = _glowTex.Value;
                Vector2 origin = tex.Size() / 2f;

                // Ink pool: asymmetric dark splotches that slowly bleed outward
                int splotchCount = 5;
                for (int s = 0; s < splotchCount; s++)
                {
                    // Each splotch has a unique angular offset and drift pattern
                    float baseAngle = s * MathHelper.TwoPi / splotchCount + MathF.Sin(s * 3.7f) * 0.5f;
                    float driftAngle = baseAngle + MathF.Sin(time * 0.008f + s * 1.9f) * 0.3f;
                    float driftRadius = 30f + auraAlpha * 25f + MathF.Sin(time * 0.012f + s * 2.3f) * 10f;

                    Vector2 splotchPos = center + driftAngle.ToRotationVector2() * driftRadius;
                    // Gravity drip: splotches drift downward slightly
                    splotchPos.Y += auraAlpha * 8f;

                    float splotchAlpha = auraAlpha * 0.12f;
                    // Dark crimson ink with void undertone
                    Color inkCol = Color.Lerp(RequiemUtils.CosmicVoid, RequiemUtils.BrightCrimson, 0.4f + s * 0.1f);
                    float splotchScale = (0.5f + s * 0.15f) * (0.9f + MathF.Sin(time * 0.01f + s) * 0.1f);

                    sb.Draw(tex, splotchPos, null, RequiemUtils.Additive(inkCol, splotchAlpha),
                        s * 1.2f, origin, splotchScale, SpriteEffects.None, 0f);
                }

                // Faint crimson haze at center (ink spreading from the weapon)
                float hazeAlpha = auraAlpha * 0.06f;
                sb.Draw(tex, center, null, RequiemUtils.Additive(RequiemUtils.BrightCrimson, hazeAlpha),
                    0f, origin, 0.7f, SpriteEffects.None, 0f);

                RequiemUtils.EndAdditive(sb);
            }
            catch
            {
                try { RequiemUtils.EndAdditive(sb); } catch { }
            }
        }
    }
}
