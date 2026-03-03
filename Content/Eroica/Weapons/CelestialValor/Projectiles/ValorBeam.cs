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
    /// Heroic beam projectile — golden-crimson energy bolt with flame trail and bloom.
    /// Spawned by CelestialValorSwing during Ascending Valor and Crimson Legion phases.
    /// </summary>
    public class ValorBeam : ModProjectile
    {
        private const int TrailLength = 16;
        private Vector2[] trailPositions = new Vector2[TrailLength];
        private bool trailInitialized = false;

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 0;
            Projectile.timeLeft = 300;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Trail tracking
            if (!trailInitialized)
            {
                trailInitialized = true;
                for (int i = 0; i < TrailLength; i++)
                    trailPositions[i] = Projectile.Center;
            }
            for (int i = TrailLength - 1; i > 0; i--)
                trailPositions[i] = trailPositions[i - 1];
            trailPositions[0] = Projectile.Center;

            // Flame dust
            EroicaVFXLibrary.SpawnFlameTrailDust(Projectile.Center, Projectile.velocity);

            // Lighting
            EroicaVFXLibrary.AddPaletteLighting(Projectile.Center, 0.4f, 0.6f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 180);
            EroicaVFXLibrary.MeleeImpact(target.Center, 1);
        }

        public override void OnKill(int timeLeft)
        {
            EroicaVFXLibrary.SpawnRadialDustBurst(Projectile.Center, 8, 4f);
            EroicaVFXLibrary.SpawnValorSparkles(Projectile.Center, 5, 20f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            // ── Trail afterimages ──
            for (int i = TrailLength - 1; i > 0; i--)
            {
                float fade = 1f - (float)i / TrailLength;
                fade *= fade;
                Vector2 drawPos = trailPositions[i] - Main.screenPosition;

                Texture2D bloom = MagnumTextureRegistry.GetBloom();
                if (bloom == null) continue;
                Vector2 origin = bloom.Size() * 0.5f;

                Color trailColor = Color.Lerp(EroicaPalette.Scarlet, EroicaPalette.Gold, (float)i / TrailLength) with { A = 0 };
                sb.Draw(bloom, drawPos, null, trailColor * (fade * 0.4f), 0f, origin, 0.2f * fade, SpriteEffects.None, 0f);
            }

            // ── Core bloom ──
            EroicaVFXLibrary.DrawEroicaBloomStack(sb, Projectile.Center,
                EroicaPalette.DeepScarlet, EroicaPalette.Gold, 0.25f, 0.9f);

            // ── Directional streak ──
            Texture2D streak = MagnumTextureRegistry.GetBeamStreak();
            if (streak != null)
            {
                Vector2 streakDraw = Projectile.Center - Main.screenPosition;
                Vector2 streakOrigin = new Vector2(streak.Width * 0.5f, streak.Height * 0.5f);
                Color streakColor = EroicaPalette.Gold with { A = 0 };
                sb.Draw(streak, streakDraw, null, streakColor * 0.7f, Projectile.rotation, streakOrigin, new Vector2(0.6f, 0.2f), SpriteEffects.None, 0f);
            }

            return false;
        }
    }
}