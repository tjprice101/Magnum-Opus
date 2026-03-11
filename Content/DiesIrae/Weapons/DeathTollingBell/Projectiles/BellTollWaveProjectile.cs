using MagnumOpus.Content.DiesIrae.Weapons.DeathTollingBell.Buffs;
using MagnumOpus.Content.DiesIrae.Weapons.DeathTollingBell.Particles;
using MagnumOpus.Content.DiesIrae.Weapons.DeathTollingBell.Utilities;
using MagnumOpus.Content.DiesIrae;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.DeathTollingBell.Projectiles
{
    /// <summary>
    /// Bell Toll Wave — expanding concentric shockwave ring.
    /// Starts small, expands outward over 90 frames hitting enemies in the ring's path.
    /// Uses ImpactFoundation's RippleShader for concentric ring rendering.
    /// ai[0] = isFuneralMarch (0 or 1), ai[1] = ring spawn delay.
    /// </summary>
    public class BellTollWaveProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        // ═══════════════════════════════════════════════════════
        //  CONSTANTS
        // ═══════════════════════════════════════════════════════
        private const int MaxLifetime = 90;
        private const int FadeOutFrames = 20;
        private const float MaxRadiusPixels = 350f;
        private const float MaxDrawScale = 0.55f;

        // ═══════════════════════════════════════════════════════
        //  STATE
        // ═══════════════════════════════════════════════════════
        private int _timer;
        private float _seed;
        private bool IsFuneralMarch => Projectile.ai[0] > 0.5f;
        private float SpawnDelay => Projectile.ai[1];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 800;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1; // Unlimited penetration — hits all in radius
            Projectile.timeLeft = MaxLifetime + 10;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = MaxLifetime + 5; // Hit each NPC once per wave
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            // Staggered spawn delay (for multi-ring toll effects)
            if (_timer == 0)
            {
                _seed = Main.rand.NextFloat(100f);
                if (SpawnDelay > 0)
                {
                    Projectile.timeLeft += (int)(SpawnDelay * 60f);
                }
            }

            // Wait for stagger delay
            float effectiveTimer = _timer - SpawnDelay * 60f;
            if (effectiveTimer < 0)
            {
                _timer++;
                Projectile.velocity = Vector2.Zero;
                return;
            }

            _timer++;
            Projectile.velocity = Vector2.Zero;

            float progress = effectiveTimer / MaxLifetime;
            float currentRadius = MaxRadiusPixels * DeathTollingBellUtils.EaseOutCubic(Math.Min(progress, 1f));

            // Dynamically scale hitbox to match expanding ring
            int hitboxSize = (int)(currentRadius * 2f);
            hitboxSize = Math.Max(hitboxSize, 24);
            Projectile.Resize(hitboxSize, hitboxSize);

            // Lighting at the ring edge
            float brightness = 1f - Math.Clamp(progress, 0f, 1f);
            Lighting.AddLight(Projectile.Center, DeathTollingBellUtils.TollEmber.ToVector3() * brightness * 0.6f);

            // Ring-shaped ambient dust emission
            if (effectiveTimer < MaxLifetime - FadeOutFrames && Main.rand.NextBool(3) && !Main.dedServ)
            {
                float dustAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 dustPos = Projectile.Center + dustAngle.ToRotationVector2() * currentRadius;
                Vector2 dustVel = dustAngle.ToRotationVector2() * Main.rand.NextFloat(1f, 2.5f);
                Color col = Color.Lerp(DeathTollingBellUtils.TollEmber, DeathTollingBellUtils.TollGold, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(dustPos, DustID.Torch, dustVel, 0, default, 0.7f);
                d.noGravity = true;
                d.fadeIn = 0.3f;
            }

            // Kill after full expansion + fade
            if (effectiveTimer >= MaxLifetime + FadeOutFrames)
                Projectile.Kill();
        }

        /// <summary>
        /// Only hit enemies within the expanding ring's radius.
        /// Uses circular collision instead of rectangular hitbox.
        /// </summary>
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float effectiveTimer = _timer - SpawnDelay * 60f;
            if (effectiveTimer < 0) return false;

            float progress = effectiveTimer / MaxLifetime;
            float currentRadius = MaxRadiusPixels * DeathTollingBellUtils.EaseOutCubic(Math.Min(progress, 1f));

            // Ring-shaped collision — enemies must be near the ring's current radius
            Vector2 targetCenter = targetHitbox.Center.ToVector2();
            float dist = Vector2.Distance(Projectile.Center, targetCenter);
            float ringThickness = IsFuneralMarch ? 60f : 40f;

            return dist >= currentRadius - ringThickness && dist <= currentRadius + ringThickness;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Tolled stack escalation
            var globalNPC = target.GetGlobalNPC<DeathTollingBellGlobalNPC>();
            globalNPC.IncrementTolledStack(target);

            // VFX on hit — ember burst at enemy position
            if (!Main.dedServ)
            {
                DeathTollingBellUtils.DoTollWaveDust(target.Center, 15f, IsFuneralMarch);

                // Dies Irae VFX: tolling bell impact — music notes (thematic for a bell!)
                DiesIraeVFXLibrary.MeleeImpact(target.Center, 0);
                DiesIraeVFXLibrary.SpawnMusicNotes(target.Center, 2, 2.5f, 0.7f, 0.8f, 35);

                if (IsFuneralMarch)
                {
                    DiesIraeVFXLibrary.SpawnJudgmentRings(target.Center, 2, 0.3f);
                }
            }
        }

        // ═══════════════════════════════════════════════════════
        //  RENDERING — Shader-driven expanding rings
        // ═══════════════════════════════════════════════════════

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            float effectiveTimer = _timer - SpawnDelay * 60f;
            if (effectiveTimer < 0) return false;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float progress = effectiveTimer / MaxLifetime;
            float fadeAlpha = effectiveTimer > (MaxLifetime - FadeOutFrames)
                ? (MaxLifetime + FadeOutFrames - effectiveTimer) / (float)(FadeOutFrames * 2)
                : Math.Clamp(effectiveTimer / 5f, 0f, 1f);

            float expandScale = MathHelper.Lerp(0.05f, MaxDrawScale, DeathTollingBellUtils.EaseOutCubic(Math.Min(progress, 1f)));
            int ringCount = IsFuneralMarch ? 6 : 3;

            // ── LAYER 1: Initial impact flash ──
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.ZoomMatrix);

            if (effectiveTimer < 15)
            {
                float flashAlpha = 1f - (effectiveTimer / 15f);
                flashAlpha *= flashAlpha;

                Texture2D softGlow = BellTextures.SoftGlow;
                if (softGlow != null)
                {
                    Vector2 glowOrigin = softGlow.Size() / 2f;
                    // Bright gold-white flash
                    sb.Draw(softGlow, drawPos, null, DeathTollingBellUtils.TollGold * (flashAlpha * 0.7f),
                        0f, glowOrigin, 0.12f, SpriteEffects.None, 0f);
                    // Wide crimson flash
                    sb.Draw(softGlow, drawPos, null, DeathTollingBellUtils.TollCrimson * (flashAlpha * 0.35f),
                        0f, glowOrigin, 0.25f, SpriteEffects.None, 0f);
                }
            }

            // ── LAYER 2: Shader-driven ripple rings ──
            DeathTollingBellUtils.DrawTollWaveShader(sb, Projectile.Center, progress,
                fadeAlpha, expandScale, ringCount, IsFuneralMarch, _seed);

            // ── LAYER 3: DI-specific ring overlay (Power Effect Ring) ──
            Texture2D powerRing = BellTextures.DIPowerEffectRing;
            if (powerRing != null)
            {
                Vector2 ringOrigin = powerRing.Size() / 2f;
                float ringScale = expandScale * 0.7f;
                Color ringColor = IsFuneralMarch ? DeathTollingBellUtils.TollGold : DeathTollingBellUtils.TollEmber;
                sb.Draw(powerRing, drawPos, null, ringColor * fadeAlpha * 0.3f,
                    effectiveTimer * 0.01f, ringOrigin, ringScale, SpriteEffects.None, 0f);
            }

            // ── LAYER 4: Outer bloom haze ──
            Texture2D glowOrb = BellTextures.GlowOrb ?? BellTextures.SoftRadialBloom;
            if (glowOrb != null)
            {
                Vector2 orbOrigin = glowOrb.Size() / 2f;
                float outerPulse = 0.9f + 0.1f * MathF.Sin(effectiveTimer * 0.15f);
                Color outerColor = DeathTollingBellUtils.TollCrimson;
                sb.Draw(glowOrb, drawPos, null, outerColor * (0.12f * fadeAlpha * outerPulse),
                    0f, orbOrigin, expandScale * 0.6f, SpriteEffects.None, 0f);
            }

            // ── RESTORE SpriteBatch ──

            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }
    }
}