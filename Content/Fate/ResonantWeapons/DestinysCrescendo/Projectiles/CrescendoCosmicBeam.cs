using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.Debuffs;
using ReLogic.Content;

namespace MagnumOpus.Content.Fate.ResonantWeapons.DestinysCrescendo
{
    /// <summary>
    /// Cosmic beam fired by the CrescendoDeityMinion.
    /// 3 penetrate, 90 life, gradient trail, multi-layer bloom PreDraw.
    /// Applies DestinyCollapse debuff on hit.
    /// 
    /// Texture: MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarHard
    /// ZERO shared VFX system references.
    /// </summary>
    public class CrescendoCosmicBeam : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarHard";

        private float pulsePhase = 0f;

        /// <summary>Escalation Phase (0-3) passed from CrescendoDeityMinion via ai[0].</summary>
        private int Phase => (int)Projectile.ai[0];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 25;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        /// <summary>Scale penetration with phase after defaults are set (called from AI first tick).</summary>
        private bool _phaseApplied = false;
        private void ApplyPhaseScaling()
        {
            if (_phaseApplied) return;
            _phaseApplied = true;
            // Phase 0: 3 pen, Phase 1: 4, Phase 2: 5, Phase 3: 7
            Projectile.penetrate += Phase;
            if (Phase >= 3) Projectile.penetrate += 1;
            // Wider hitbox at higher phases
            int sizeBonus = Phase * 4;
            Projectile.width += sizeBonus;
            Projectile.height += sizeBonus;
        }

        public override void AI()
        {
            ApplyPhaseScaling();

            Projectile.rotation = Projectile.velocity.ToRotation();
            pulsePhase += 0.18f;

            if (Main.dedServ) return;

            float phaseIntensity = 1f + Phase * 0.35f; // VFX density multiplier

            // === COSMIC BEAM TRAIL VFX (scaled with phase) ===

            // Gradient glow particles trailing behind the beam
            if (Main.rand.NextFloat() < 0.5f * phaseIntensity)
            {
                Color trailColor = CrescendoUtils.GetCrescendoGradient(Main.rand.NextFloat(0.3f, 0.8f));
                Vector2 vel = -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(1f, 1f);
                CrescendoParticleHandler.Spawn(CrescendoParticleFactory.OrbGlow(Projectile.Center, vel, trailColor * (0.5f + Phase * 0.1f), 0.16f + Phase * 0.04f, 14));
            }

            // Star sparkles in trail 遯ｶ繝ｻmore at higher phases
            if (Main.rand.NextFloat() < 0.33f * phaseIntensity)
            {
                Vector2 sparkPos = Projectile.Center + Main.rand.NextVector2Circular(12f + Phase * 4f, 12f + Phase * 4f);
                Color sparkCol = Main.rand.NextBool(2) ? CrescendoUtils.StarGold : CrescendoUtils.CelestialWhite;
                CrescendoParticleHandler.Spawn(CrescendoParticleFactory.DivineSpark(sparkPos,
                    Main.rand.NextVector2Circular(1.5f, 1.5f), sparkCol * (0.4f + Phase * 0.1f), 0.12f + Phase * 0.03f, 12));
            }

            // Cosmic cloud wisps along beam path
            if (Main.rand.NextFloat() < 0.33f * phaseIntensity * 0.4f)
            {
                CrescendoParticleFactory.SpawnAuraWisps(Projectile.Center, 1, 10f + Phase * 3f);
            }

            // Glyph accents 遯ｶ繝ｻmore frequent at higher phases
            if (Main.rand.NextFloat() < (0.125f + Phase * 0.05f))
            {
                CrescendoParticleHandler.Spawn(CrescendoParticleFactory.GlyphCircle(Projectile.Center, CrescendoUtils.DeityPurple * (0.5f + Phase * 0.1f), 0.2f + Phase * 0.05f, 16));
            }

            // Cosmic music notes 遯ｶ繝ｻthe beam sings louder at higher phases
            if (Main.rand.NextFloat() < (0.17f + Phase * 0.06f))
            {
                CrescendoParticleFactory.SpawnCosmicNotes(Projectile.Center, 1, 8f + Phase * 2f);
            }

            Lighting.AddLight(Projectile.Center, CrescendoUtils.CelestialWhite.ToVector3() * (1.3f + Phase * 0.3f));
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 180);

            // Enhanced impact burst
            CrescendoParticleHandler.SpawnBurst(target.Center, 10, 5f, 0.2f, CrescendoUtils.DivineCrimson, CrescendoParticleType.DivineSpark, 16);
            CrescendoParticleHandler.SpawnBurst(target.Center, 4, 3f, 0.25f, CrescendoUtils.StarGold, CrescendoParticleType.GlyphCircle, 20);
            CrescendoParticleFactory.SpawnCosmicNotes(target.Center, 3, 18f);

            // Flash
            CrescendoParticleHandler.Spawn(CrescendoParticleFactory.BeamFlare(target.Center, Vector2.Zero, CrescendoUtils.CelestialWhite, 0.5f, 12));
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;

            // Death explosion
            CrescendoParticleHandler.SpawnBurst(Projectile.Center, 12, 6f, 0.22f, CrescendoUtils.CrescendoPink, CrescendoParticleType.DivineSpark, 18);
            CrescendoParticleHandler.SpawnBurst(Projectile.Center, 5, 3f, 0.3f, CrescendoUtils.StarGold, CrescendoParticleType.GlyphCircle, 22);
            CrescendoParticleFactory.SpawnCosmicNotes(Projectile.Center, 4, 20f);

            // Central flash
            CrescendoParticleHandler.Spawn(CrescendoParticleFactory.BeamFlare(Projectile.Center, Vector2.Zero, CrescendoUtils.CelestialWhite, 0.6f, 14));

            SoundEngine.PlaySound(SoundID.Item10 with { Pitch = 0.5f, Volume = 0.5f }, Projectile.Center);
        }

        // 隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨翫・PREDRAW 遯ｶ繝ｻGRADIENT TRAIL + MULTI-LAYER BLOOM 隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨翫・

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;

            try
            {
                Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarHard", AssetRequestMode.ImmediateLoad).Value;
                Vector2 origin = tex.Size() / 2f;

                float pulse = 1f + MathF.Sin(pulsePhase) * 0.15f;
                float phaseScale = 1f + Phase * 0.2f; // Beams visually larger at higher phases
                float phaseGlow = 1f + Phase * 0.15f; // Brighter glow at higher phases

                // === PASS 1: Gradient trail from oldPos ===
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero) continue;
                    float progress = (float)i / Projectile.oldPos.Length;
                    Color trailColor = CrescendoUtils.GetCrescendoGradient(0.2f + progress * 0.6f) * (1f - progress) * 0.6f * phaseGlow;
                    Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                    float trailScale = (1f - progress * 0.4f) * 0.7f * pulse * phaseScale;
                    spriteBatch.Draw(tex, trailPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
                }

                // === PASS 2: Multi-layer additive bloom (phase-scaled) ===
                CrescendoUtils.BeginAdditive(spriteBatch);

                Vector2 drawPos = Projectile.Center - Main.screenPosition;

                // Layer 1: Deity purple outer glow
                spriteBatch.Draw(tex, drawPos, null, CrescendoUtils.DeityPurple * (0.4f * phaseGlow), Projectile.rotation, origin, 1.2f * pulse * phaseScale, SpriteEffects.None, 0f);

                // Layer 2: Divine crimson mid
                spriteBatch.Draw(tex, drawPos, null, CrescendoUtils.DivineCrimson * (0.5f * phaseGlow), Projectile.rotation, origin, 0.9f * pulse * phaseScale, SpriteEffects.None, 0f);

                // Layer 3: Crescendo pink inner
                spriteBatch.Draw(tex, drawPos, null, CrescendoUtils.CrescendoPink * (0.6f * phaseGlow), Projectile.rotation, origin, 0.6f * pulse * phaseScale, SpriteEffects.None, 0f);

                // Layer 4: Celestial white hot core
                spriteBatch.Draw(tex, drawPos, null, CrescendoUtils.CelestialWhite * (0.85f * phaseGlow), Projectile.rotation, origin, 0.35f * pulse * phaseScale, SpriteEffects.None, 0f);

                CrescendoUtils.BeginAlpha(spriteBatch);
            }
            catch
            {
                try
                {
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                        DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
                catch { }
            }

            return false;
        }
    }
}