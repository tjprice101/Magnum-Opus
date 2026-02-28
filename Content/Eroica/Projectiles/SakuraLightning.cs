using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using MagnumOpus.Content.Eroica.Weapons.PiercingLightOfTheSakura.Utilities;
using MagnumOpus.Content.Eroica.Weapons.PiercingLightOfTheSakura.Particles;
using MagnumOpus.Content.Eroica.Weapons.PiercingLightOfTheSakura.Dusts;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Spiral lightning explosion effect spawned by Piercing Light of the Sakura projectile.
    /// Self-contained VFX: expanding ring of lightning sparks + central bloom + particle burst.
    /// </summary>
    public class SakuraLightning : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/SandboxLastPrism/Flare/flare_16";

        private bool initialized = false;
        private float spiralAngle = 0f;
        private int spiralCounter = 0;

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 45;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.light = 0.6f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            if (!initialized)
            {
                initialized = true;
                SoundEngine.PlaySound(SoundID.Item74 with { Pitch = 0.3f, Volume = 0.8f }, Projectile.Center);
                CreateInitialBurst();
            }

            Projectile.velocity = Vector2.Zero;

            // Spawn spiral lightning sparks
            spiralCounter++;
            if (spiralCounter % 2 == 0 && Projectile.timeLeft > 10)
            {
                spiralAngle += MathHelper.ToRadians(45);
                float radius = (45 - Projectile.timeLeft) * 1.5f;
                Vector2 sparkPos = Projectile.Center + spiralAngle.ToRotationVector2() * radius;
                Vector2 sparkVel = (spiralAngle + MathHelper.PiOver2).ToRotationVector2() * Main.rand.NextFloat(1f, 3f);

                PiercingParticleHandler.SpawnParticle(new LightningSparkParticle(
                    sparkPos,
                    sparkVel,
                    Color.Lerp(PiercingUtils.LightningCore, PiercingUtils.LightGold, Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.4f, 0.8f),
                    Main.rand.Next(6, 14)
                ));
            }
        }

        private void CreateInitialBurst()
        {
            // Central impact flash
            Color flashColor = PiercingUtils.BrilliantWhite;
            flashColor.A = 0;
            PiercingParticleHandler.SpawnParticle(new CrescendoFlashParticle(
                Projectile.Center,
                Vector2.Zero,
                flashColor,
                0.9f,
                12
            ));

            // Radial lightning spark burst
            int sparkCount = 16;
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = MathHelper.TwoPi * i / sparkCount + Main.rand.NextFloatDirection() * 0.15f;
                float speed = Main.rand.NextFloat(4f, 10f);
                PiercingParticleHandler.SpawnParticle(new LightningSparkParticle(
                    Projectile.Center,
                    angle.ToRotationVector2() * speed,
                    Color.Lerp(PiercingUtils.LightningCore, PiercingUtils.LightningEdge, Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.5f, 0.9f),
                    Main.rand.Next(8, 18)
                ));
            }

            // Dust ring
            for (int i = 0; i < 12; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(6f, 6f);
                int dust = Dust.NewDust(Projectile.Center - new Vector2(4), 8, 8,
                    ModContent.DustType<PiercingLightDust>(), dustVel.X, dustVel.Y, 0,
                    PiercingUtils.LightGold, Main.rand.NextFloat(0.8f, 1.3f));
                Main.dust[dust].noGravity = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.2f, Volume = 0.7f }, Projectile.Center);

            // Final spark burst
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                PiercingParticleHandler.SpawnParticle(new LightningSparkParticle(
                    Projectile.Center,
                    angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f),
                    PiercingUtils.LightGold,
                    Main.rand.NextFloat(0.3f, 0.6f),
                    Main.rand.Next(6, 12)
                ));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float lifeProgress = 1f - (float)Projectile.timeLeft / 45f;

            // Expanding lightning ring — fade in then out
            float ringAlpha = lifeProgress < 0.3f ? lifeProgress / 0.3f : (1f - lifeProgress) / 0.7f;
            float ringScale = 0.4f + lifeProgress * 1.8f;

            Color ringColor = Color.Lerp(PiercingUtils.LightningCore, PiercingUtils.LightGold, lifeProgress * 0.5f);
            ringColor.A = 0;

            // Lightning texture with additive blend
            PiercingUtils.EnterShaderRegion(sb);

            // Main lightning burst
            sb.Draw(tex, drawPos, null, ringColor * ringAlpha * 0.8f,
                spiralAngle, origin, ringScale * Projectile.scale, SpriteEffects.None, 0f);

            // Rotated copy for fuller coverage
            sb.Draw(tex, drawPos, null, ringColor * ringAlpha * 0.5f,
                spiralAngle + MathHelper.PiOver4, origin, ringScale * Projectile.scale * 0.9f, SpriteEffects.None, 0f);

            // Central bright core
            Color coreColor = PiercingUtils.BrilliantWhite;
            coreColor.A = 0;
            float coreScale = (1f - lifeProgress) * 0.6f;
            sb.Draw(tex, drawPos, null, coreColor * ringAlpha * 0.6f,
                0f, origin, coreScale, SpriteEffects.None, 0f);

            PiercingUtils.ExitShaderRegion(sb);

            return false;
        }
    }
}
