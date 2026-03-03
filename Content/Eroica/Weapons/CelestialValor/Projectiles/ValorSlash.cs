using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor.Projectiles
{
    /// <summary>
    /// Crimson-gold slash arc projectile — travels outward from the player
    /// with a wide smear trail, bloom gradient, and musical sparks.
    /// </summary>
    public class ValorSlash : ModProjectile
    {
        private const int MaxLife = 25;

        public override void SetDefaults()
        {
            Projectile.width = 160;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = MaxLife;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.noEnchantmentVisuals = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity *= 0.97f;

            float progress = 1f - (float)Projectile.timeLeft / MaxLife;
            Projectile.Opacity = MathF.Max(0f, 1f - progress * 1.2f);

            // Spark trail
            if (Projectile.timeLeft % 3 == 0)
                EroicaVFXLibrary.SpawnDirectionalSparks(Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.UnitX), 2, 3f);

            EroicaVFXLibrary.AddPaletteLighting(Projectile.Center, 0.35f, 0.5f * Projectile.Opacity);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 180);
            EroicaVFXLibrary.MeleeImpact(target.Center, 1);
        }

        public override void OnKill(int timeLeft)
        {
            EroicaVFXLibrary.SpawnValorSparkles(Projectile.Center, 4, 15f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            float progress = 1f - (float)Projectile.timeLeft / MaxLife;
            float fade = Projectile.Opacity;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // ── Layer 1: Wide smear streak ──
            Texture2D streak = MagnumTextureRegistry.GetBeamStreak();
            if (streak != null)
            {
                Vector2 streakOrigin = new Vector2(streak.Width * 0.5f, streak.Height * 0.5f);

                // Outer scarlet smear
                Color outerColor = EroicaPalette.Scarlet with { A = 0 };
                sb.Draw(streak, drawPos, null, outerColor * (fade * 0.5f), Projectile.rotation, streakOrigin,
                    new Vector2(1.8f, 0.4f), SpriteEffects.None, 0f);

                // Inner gold core
                Color innerColor = EroicaPalette.Gold with { A = 0 };
                sb.Draw(streak, drawPos, null, innerColor * (fade * 0.6f), Projectile.rotation, streakOrigin,
                    new Vector2(1.4f, 0.2f), SpriteEffects.None, 0f);

                // Hot white center
                Color hotColor = EroicaPalette.HotCore with { A = 0 };
                sb.Draw(streak, drawPos, null, hotColor * (fade * 0.5f), Projectile.rotation, streakOrigin,
                    new Vector2(1.0f, 0.1f), SpriteEffects.None, 0f);
            }

            // ── Layer 2: Bloom at center ──
            EroicaVFXLibrary.DrawEroicaBloomStack(sb, Projectile.Center,
                EroicaPalette.Scarlet, EroicaPalette.Gold, 0.2f * fade, 0.6f * fade);

            return false;
        }
    }
}