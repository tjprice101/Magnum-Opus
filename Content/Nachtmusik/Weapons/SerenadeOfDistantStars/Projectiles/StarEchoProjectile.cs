using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Debuffs;

namespace MagnumOpus.Content.Nachtmusik.Weapons.SerenadeOfDistantStars.Projectiles
{
    /// <summary>
    /// Star Echo Projectile — Simpler secondary homing star spawned by Star Memory.
    /// Small fading homing star that seeks an assigned target.
    /// ai[0] = target NPC index.
    /// </summary>
    public class StarEchoProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarSoft";

        private int TargetIndex => (int)Projectile.ai[0];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // === HOME TOWARD ASSIGNED TARGET ===
            if (TargetIndex >= 0 && TargetIndex < Main.maxNPCs)
            {
                NPC target = Main.npc[TargetIndex];
                if (target.active && target.CanBeChasedBy(Projectile))
                {
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 14f, 0.06f);
                }
            }

            // Fade alpha over lifetime
            float lifeProgress = 1f - (Projectile.timeLeft / 120f);
            Projectile.Opacity = MathHelper.Lerp(0.9f, 0.3f, lifeProgress);

            // === SUBTLE TRAIL — dimmer star dust ===
            if (Main.rand.NextBool(2))
            {
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(3f, 3f);
                Vector2 dustVel = -Projectile.velocity * 0.1f;
                Dust d = Dust.NewDustPerfect(dustPos, DustID.GoldFlame, dustVel, 0, default, 0.55f);
                d.noGravity = true;
                d.fadeIn = 0.6f;
            }

            // Occasional blue accent
            if (Main.rand.NextBool(5))
            {
                Dust b = Dust.NewDustPerfect(Projectile.Center, DustID.BlueTorch,
                    Main.rand.NextVector2Circular(0.5f, 0.5f), 0, default, 0.35f);
                b.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, NachtmusikPalette.StarGold.ToVector3() * 0.25f * Projectile.Opacity);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 180);
            if (target.TryGetGlobalNPC(out CelestialHarmonyNPC harmonyNPC))
                harmonyNPC.AddStack(target, 1);

            // Small cosmic flash
            CustomParticles.GenericFlare(target.Center, NachtmusikPalette.StarGold, 0.3f, 12);
            CustomParticles.GenericFlare(target.Center, NachtmusikPalette.StarWhite, 0.2f, 10);

            NachtmusikVFXLibrary.SpawnStarBurst(target.Center, 3, 0.25f);
        }

        public override void OnKill(int timeLeft)
        {
            // Small sparkle dissipation
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 vel = angle.ToRotationVector2() * 1.5f;
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame, vel, 0, default, 0.4f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 pos = Projectile.Center - Main.screenPosition;

            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.08f;

            // Subtle afterimage trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = 1f - (i / (float)Projectile.oldPos.Length);
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                Color afterColor = NachtmusikPalette.StarGold with { A = 0 } * progress * 0.25f * Projectile.Opacity;
                Main.spriteBatch.Draw(tex, drawPos, null, afterColor, Projectile.oldRot[i],
                    origin, 0.3f * progress, SpriteEffects.None, 0f);
            }

            // Dimmer/smaller star core — gold outer, white inner
            Main.spriteBatch.Draw(tex, pos, null,
                NachtmusikPalette.StarGold with { A = 0 } * 0.4f * Projectile.Opacity,
                Projectile.rotation, origin, 0.4f * pulse, SpriteEffects.None, 0f);

            Main.spriteBatch.Draw(tex, pos, null,
                NachtmusikPalette.StarWhite with { A = 0 } * 0.3f * Projectile.Opacity,
                Projectile.rotation, origin, 0.25f * pulse, SpriteEffects.None, 0f);

            // Nachtmusik theme star flare accent
            NachtmusikShaderManager.BeginAdditive(Main.spriteBatch);
            NachtmusikVFXLibrary.DrawThemeStarFlare(Main.spriteBatch, Projectile.Center, 1f, 0.5f);
            NachtmusikShaderManager.RestoreSpriteBatch(Main.spriteBatch);

            return false;
        }
    }
}
