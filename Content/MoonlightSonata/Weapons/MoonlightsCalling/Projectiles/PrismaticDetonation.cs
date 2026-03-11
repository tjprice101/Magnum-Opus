using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Buffs;
using MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Dusts;
using MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Particles;
using MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Utilities.SerenadeUtils;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Projectiles
{
    /// <summary>
    /// PrismaticDetonation — the final explosion when the beam completes its 5th bounce.
    /// A massive prismatic explosion with:
    /// - Expanding refraction ripple ring
    /// - God-ray bloom bursts in all spectral colors
    /// - Screen shake
    /// - High AoE damage
    /// - Music note cascade
    /// 
    /// Purely visual + damage — no movement, 30-tick lifetime.
    /// </summary>
    public class PrismaticDetonation : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_1";

        private const int Lifetime = 30;
        private const float MaxRadius = 300f;

        public override void SetDefaults()
        {
            Projectile.width = (int)(MaxRadius * 2);
            Projectile.height = (int)(MaxRadius * 2);
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            Projectile.localAI[0]++;
            float t = Projectile.localAI[0] / (float)Lifetime;

            // Screen shake on spawn
            if (Projectile.localAI[0] == 1)
            {
                if (Projectile.owner == Main.myPlayer)
                {
                    Main.LocalPlayer.GetModPlayer<LaCampanella.Debuffs.ScreenShakePlayer>()?.AddShake(8f, 25);
                }
                SpawnDetonationVFX();
            }

            // Ongoing expanding bloom rings
            if (!Main.dedServ && Projectile.localAI[0] % 5 == 0)
            {
                float ringProgress = t;
                Color ringColor = GetBeamGradient(ringProgress);
                float ringScale = 1f + ringProgress * 4f;

                SerenadeParticleHandler.Spawn(new RefractionBloomParticle(
                    Projectile.Center, ringColor, ringScale, 25
                ));
            }

            // Ongoing spectral smoke
            if (!Main.dedServ && Projectile.localAI[0] % 3 == 0)
            {
                Vector2 offset = Main.rand.NextVector2Circular(MaxRadius * t, MaxRadius * t);
                Color smokeCol = GetBeamGradient(Main.rand.NextFloat()) * 0.5f;
                SerenadeParticleHandler.Spawn(new SerenadeMistParticle(
                    Projectile.Center + offset,
                    Main.rand.NextVector2Circular(1f, 1f),
                    smokeCol, 0.5f + Main.rand.NextFloat(0.5f),
                    40 + Main.rand.Next(20)
                ));
            }

            // Light
            float lightIntensity = (1f - t) * 2f;
            Lighting.AddLight(Projectile.Center, PrismViolet.ToVector3() * lightIntensity);
        }

        private void SpawnDetonationVFX()
        {
            if (Main.dedServ) return;

            // === FOUNDATION VFX: PrismaticMaskOrb (MaskFoundation RadialNoiseMaskShader) ===
            // Swirling noise sphere at detonation center — dramatic, prismatic.
            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center, Vector2.Zero,
                    ModContent.ProjectileType<PrismaticMaskOrb>(),
                    0, 0f, Projectile.owner,
                    ai0: 0.5f // mid spectral phase
                );
            }

            // === Central burst ===
            // 5-layer bloom cascade (same as Eternal Moon detonation, prismatic themed)
            for (int layer = 0; layer < 5; layer++)
            {
                float scale = 2f + layer * 0.8f;
                float alpha = (5 - layer) / 5f;
                Color bloomCol = GetSpectralColor(layer) * alpha;

                SerenadeParticleHandler.Spawn(new RefractionBloomParticle(
                    Projectile.Center, bloomCol, scale, 25 + layer * 5
                ));
            }

            // === Radial spectral spark explosion ===
            int sparkCount = 30;
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = MathHelper.TwoPi * i / sparkCount;
                float speed = 5f + Main.rand.NextFloat(4f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                Color col = GetSpectralColor(i % SpectralColors.Length);

                SerenadeParticleHandler.Spawn(new PrismaticSparkParticle(
                    Projectile.Center, vel, col, MoonWhite,
                    0.6f + Main.rand.NextFloat(0.4f), 30 + Main.rand.Next(20)
                ));
            }

            // === Prism shards radiating outward ===
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f + Main.rand.NextFloat(0.2f);
                Vector2 vel = angle.ToRotationVector2() * (3f + Main.rand.NextFloat(3f));
                Color col = GetSpectralColor(i % 7);
                SerenadeParticleHandler.Spawn(new PrismShardParticle(
                    Projectile.Center, vel, col, 0.5f + Main.rand.NextFloat(0.3f),
                    35 + Main.rand.Next(15)
                ));
            }

            // === Music note cascade ===
            for (int i = 0; i < 10; i++)
            {
                Vector2 noteVel = Main.rand.NextVector2CircularEdge(2f, 2f) + new Vector2(0, -1f);
                SerenadeParticleHandler.Spawn(new SpectralNoteParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(20, 20),
                    noteVel, 0.6f + Main.rand.NextFloat(0.3f),
                    80 + Main.rand.Next(40)
                ));
            }

            // === Dust explosion ===
            for (int i = 0; i < 25; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(8f, 8f);
                int d = Dust.NewDust(Projectile.Center - new Vector2(4), 8, 8,
                    ModContent.DustType<PrismaticDust>(), dustVel.X, dustVel.Y);
                Main.dust[d].scale = 1.5f;
                Main.dust[d].noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MusicalDissonance>(), 600); // 10s on detonation

            if (!Main.dedServ)
            {
                // Per-target spectral bloom
                SerenadeParticleHandler.Spawn(new RefractionBloomParticle(
                    target.Center, GetSpectralColor(Main.rand.Next(7)), 1.2f, 20
                ));
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Expanding circle collision
            float t = Projectile.localAI[0] / (float)Lifetime;
            float currentRadius = MaxRadius * ExpoOut(t);
            float dist = Vector2.Distance(Projectile.Center, targetHitbox.Center.ToVector2());
            return dist < currentRadius;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            // Draw expanding refraction ring (shader-driven ripple)
            DrawRefractionRing();
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

        private void DrawRefractionRing()
        {
            var tex = SerenadeTextures.SoftRadialBloom;
            if (tex == null) return;

            float t = Projectile.localAI[0] / (float)Lifetime;
            float scale = 0.015f + ExpoOut(t) * 0.092f;
            float alpha = (1f - t) * 0.6f;
            Color ringColor = GetBeamGradient(t) * alpha;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            var origin = tex.Size() * 0.5f;

            // Switch to additive blending for glow VFX (black-background textures)
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            // Outer ring glow
            Main.spriteBatch.Draw(tex, drawPos, null, ringColor * 0.4f, 0f, origin, scale * 1.3f, SpriteEffects.None, 0f);

            // Main ring
            Main.spriteBatch.Draw(tex, drawPos, null, ringColor, 0f, origin, scale, SpriteEffects.None, 0f);

            // White core flash (fades quickly)
            float coreAlpha = MathF.Max(0f, 1f - t * 4f);
            Main.spriteBatch.Draw(tex, drawPos, null, MoonWhite * coreAlpha, 0f, origin, scale * 0.3f, SpriteEffects.None, 0f);

            // Restore default blend state
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
