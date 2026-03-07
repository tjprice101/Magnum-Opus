using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.ResonantWeapons.RequiemOfReality.Utilities;
using MagnumOpus.Content.Fate.ResonantWeapons.RequiemOfReality.Particles;
using MagnumOpus.Content.Fate.Debuffs;

namespace MagnumOpus.Content.Fate.ResonantWeapons.RequiemOfReality.Projectiles
{
    /// <summary>
    /// RequiemRealityTear — Lingering reality rift zone spawned on 15% hit chance.
    /// Stays for 2 seconds dealing 30% weapon damage every 0.5s to enemies inside.
    /// Applies RealityFrayed debuff on contact.
    /// 
    /// Visual: Pulsating crimson-pink rift with FA Harmonic Resonance Wave Impact texture,
    /// surrounded by chromatic sparks and celestial glyph particles.
    /// </summary>
    public class RequiemRealityTear : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Fate/Impact Effects/FA Harmonic Resonance Wave Impact";

        private static Asset<Texture2D> _supernovaTex;
        private static Asset<Texture2D> _glowTex;
        private static Asset<Texture2D> _glyphTex;

        private const int LIFETIME = 120; // 2 seconds
        private const int HIT_INTERVAL = 30; // 0.5s between damage ticks
        private const float RADIUS = 55f;

        private int _hitTimer;
        private float _pulsePhase;

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = LIFETIME;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = HIT_INTERVAL;
            Projectile.alpha = 0;
        }

        public override void OnSpawn(IEntitySource source)
        {
            _hitTimer = 0;
            _pulsePhase = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;
            _hitTimer++;
            _pulsePhase += 0.08f;

            float lifeProgress = 1f - (float)Projectile.timeLeft / LIFETIME;

            // Fade in/out
            if (Projectile.timeLeft > LIFETIME - 10)
                Projectile.alpha = (int)(255 * (1f - (LIFETIME - Projectile.timeLeft) / 10f));
            else if (Projectile.timeLeft < 20)
                Projectile.alpha = (int)(255 * (1f - Projectile.timeLeft / 20f));
            else
                Projectile.alpha = 0;

            // Spawn ambient VFX
            if (!Main.dedServ)
            {
                SpawnAmbientVFX(lifeProgress);
            }

            // Pulsating light
            float pulseIntensity = 0.5f + 0.3f * MathF.Sin(_pulsePhase);
            Lighting.AddLight(Projectile.Center, RequiemUtils.BrightCrimson.ToVector3() * pulseIntensity);
        }

        private void SpawnAmbientVFX(float lifeProgress)
        {
            // Chromatic sparks around the tear
            if (Main.rand.NextBool(3))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = Main.rand.NextFloat(RADIUS * 0.5f, RADIUS);
                Vector2 sparkPos = Projectile.Center + angle.ToRotationVector2() * dist;
                Vector2 sparkVel = (Projectile.Center - sparkPos).SafeNormalize(Vector2.Zero) * 1.5f;

                Color sparkCol = Main.rand.NextBool() ? RequiemUtils.BrightCrimson : RequiemUtils.DarkPink;
                RequiemParticleHandler.SpawnParticle(new RequiemSparkParticle(
                    sparkPos, sparkVel, sparkCol, 0.15f, 10));
            }

            // Occasional glyph accent
            if (Main.rand.NextBool(12))
            {
                Vector2 glyphPos = Projectile.Center + Main.rand.NextVector2Circular(30f, 30f);
                Color glyphCol = RequiemUtils.PaletteLerp(Main.rand.NextFloat());
                RequiemParticleHandler.SpawnParticle(new RequiemGlyphParticle(
                    glyphPos, glyphCol, 0.2f, 20));
            }

            // Supernova core pulse every 15 frames
            if (_hitTimer % 15 == 0)
            {
                RequiemParticleHandler.SpawnParticle(new RequiemBloomFlare(
                    Projectile.Center, RequiemUtils.BrightCrimson * 0.6f, 0.25f, 10));
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply RealityFrayed DoT debuff
            target.AddBuff(ModContent.BuffType<RealityFrayed>(), 180); // 3 seconds
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 120);

            // Small impact VFX on each tick
            if (!Main.dedServ)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 sparkVel = Main.rand.NextVector2Circular(3f, 3f);
                    Color col = i == 0 ? RequiemUtils.SupernovaWhite : RequiemUtils.BrightCrimson;
                    RequiemParticleHandler.SpawnParticle(new RequiemSparkParticle(
                        target.Center, sparkVel, col, 0.18f, 8));
                }
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Circular hitbox
            float dist = Vector2.Distance(Projectile.Center, targetHitbox.Center.ToVector2());
            return dist < RADIUS + Math.Max(targetHitbox.Width, targetHitbox.Height) * 0.5f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;

            _supernovaTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Fate/Particles/FA Supernova Core");
            _glowTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow");
            _glyphTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Fate/Particles/FA Celestial Glyph");

            var sb = Main.spriteBatch;
            try
            {
            float opacity = 1f - Projectile.alpha / 255f;
            float pulse = 0.85f + 0.15f * MathF.Sin(_pulsePhase);
            float lifeProgress = 1f - (float)Projectile.timeLeft / LIFETIME;

            // Switch to Additive for all glow/bloom layers (black backgrounds invisible in additive)
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            // Layer 1: Wide soft glow underlayer
            if (_glowTex?.IsLoaded == true)
            {
                var glowOrigin = _glowTex.Value.Size() * 0.5f;
                sb.Draw(_glowTex.Value, Projectile.Center - Main.screenPosition, null,
                    (RequiemUtils.DarkPink with { A = 0 }) * 0.3f * opacity * pulse, 0f, glowOrigin,
                    0.55f * pulse, SpriteEffects.None, 0f);
                sb.Draw(_glowTex.Value, Projectile.Center - Main.screenPosition, null,
                    (RequiemUtils.BrightCrimson with { A = 0 }) * 0.2f * opacity, 0f, glowOrigin,
                    0.7f, SpriteEffects.None, 0f);
            }

            // Layer 2: Celestial glyph rotating slowly
            if (_glyphTex?.IsLoaded == true)
            {
                var glyphOrigin = _glyphTex.Value.Size() * 0.5f;
                float glyphRot = (float)Main.timeForVisualEffects * 0.015f;
                sb.Draw(_glyphTex.Value, Projectile.Center - Main.screenPosition, null,
                    (RequiemUtils.FatePurple with { A = 0 }) * 0.5f * opacity, glyphRot, glyphOrigin,
                    0.6f * pulse, SpriteEffects.None, 0f);
            }

            // Layer 3: Core rift texture (main Texture)
            var tex = ModContent.Request<Texture2D>(Texture);
            if (tex.IsLoaded)
            {
                var origin = tex.Value.Size() * 0.5f;
                float riftRot = (float)Main.timeForVisualEffects * -0.02f;
                sb.Draw(tex.Value, Projectile.Center - Main.screenPosition, null,
                    (RequiemUtils.BrightCrimson with { A = 0 }) * 0.7f * opacity * pulse, riftRot, origin,
                    0.5f * pulse, SpriteEffects.None, 0f);
                // Inner bright core
                sb.Draw(tex.Value, Projectile.Center - Main.screenPosition, null,
                    (RequiemUtils.SupernovaWhite with { A = 0 }) * 0.4f * opacity, riftRot + 0.5f, origin,
                    0.3f * pulse, SpriteEffects.None, 0f);
            }

            // Layer 4: Supernova core hotspot
            if (_supernovaTex?.IsLoaded == true)
            {
                var superOrigin = _supernovaTex.Value.Size() * 0.5f;
                float superPulse = 0.7f + 0.3f * MathF.Sin(_pulsePhase * 1.5f);
                sb.Draw(_supernovaTex.Value, Projectile.Center - Main.screenPosition, null,
                    (RequiemUtils.SupernovaWhite with { A = 0 }) * 0.5f * opacity * superPulse, 0f, superOrigin,
                    0.35f * superPulse, SpriteEffects.None, 0f);
            }

            }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            return false;
        }
    }
}
