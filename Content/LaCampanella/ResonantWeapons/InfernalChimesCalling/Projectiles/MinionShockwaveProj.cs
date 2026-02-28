using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.InfernalChimesCalling.Utilities;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.InfernalChimesCalling.Particles;
using MagnumOpus.Content.LaCampanella.Debuffs;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.InfernalChimesCalling.Projectiles
{
    /// <summary>
    /// AoE shockwave explosion triggered every 5th minion hit.
    /// Expanding ring of fire + concussive blast that damages all enemies in radius.
    /// </summary>
    public class MinionShockwaveProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow";

        private const int Duration = 30;
        private const float MaxRadius = 250f;

        public override void SetDefaults()
        {
            Projectile.width = (int)(MaxRadius * 2);
            Projectile.height = (int)(MaxRadius * 2);
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = Duration;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1; // Hit once
        }

        public override void AI()
        {
            float progress = 1f - (float)Projectile.timeLeft / Duration;

            // Spawn frame VFX burst
            if (Projectile.timeLeft == Duration)
            {
                // Big shockwave pulse ring
                InfernalChimesParticleHandler.SpawnParticle(new ShockwavePulseParticle(
                    Projectile.Center, MaxRadius / 40f, Duration));

                // Bell ring pulse
                InfernalChimesParticleHandler.SpawnParticle(new BellRingPulseParticle(
                    Projectile.Center, MaxRadius, Duration - 5));

                // Musical notes burst outward
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi / 12f * i;
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(2f, 5f);
                    vel.Y -= 1f;
                    InfernalChimesParticleHandler.SpawnParticle(new MusicalChoirNoteParticle(
                        Projectile.Center + vel * 5f, vel, Main.rand.Next(50, 80)));
                }

                // Ember ring
                for (int i = 0; i < 20; i++)
                {
                    float angle = MathHelper.TwoPi / 20f * i + Main.rand.NextFloat(-0.1f, 0.1f);
                    InfernalChimesParticleHandler.SpawnParticle(new ChoirEmberParticle(
                        Projectile.Center, angle, 10f, 0.12f, Main.rand.Next(25, 45)));
                }
            }

            // Continuous ember spray during expansion
            if (Projectile.timeLeft > Duration / 2 && Projectile.timeLeft % 2 == 0)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = progress * MaxRadius * Main.rand.NextFloat(0.7f, 1.0f);
                Vector2 pos = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                InfernalChimesParticleHandler.SpawnParticle(new ChoirEmberParticle(
                    pos, angle, 3f, 0.05f, Main.rand.Next(15, 30)));
            }

            // Lighting
            float lightIntensity = (1f - progress) * 1.5f;
            Lighting.AddLight(Projectile.Center, InfernalChimesCallingUtils.ChoirPalette[2].ToVector3() * lightIntensity);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float progress = 1f - (float)Projectile.timeLeft / Duration;
            float currentRadius = MaxRadius * (float)Math.Sqrt(progress);
            float dist = Vector2.Distance(Projectile.Center, targetHitbox.Center.ToVector2());
            return dist <= currentRadius;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 3);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            float progress = 1f - (float)Projectile.timeLeft / Duration;
            float fade = 1f - progress;

            InfernalChimesParticleHandler.DrawAllParticles(sb);

            // Draw expanding shockwave ring using bloom texture
            var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow").Value;
            float ringRadius = MaxRadius * (float)Math.Sqrt(progress);
            float ringScale = ringRadius / (tex.Width * 0.5f);

            // Core blast
            Color coreColor = InfernalChimesCallingUtils.ShockwavePalette[0] * fade * 0.4f;
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null,
                coreColor, 0f, tex.Size() / 2f, ringScale * 0.8f, SpriteEffects.None, 0f);

            // Outer ring
            Color ringColor = InfernalChimesCallingUtils.ShockwavePalette[1] * fade * 0.3f;
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null,
                ringColor, 0f, tex.Size() / 2f, ringScale, SpriteEffects.None, 0f);

            // Hot center flash (early frames only)
            if (progress < 0.3f)
            {
                Color flashColor = InfernalChimesCallingUtils.ShockwavePalette[2] * (1f - progress / 0.3f) * 0.6f;
                sb.Draw(tex, Projectile.Center - Main.screenPosition, null,
                    flashColor, 0f, tex.Size() / 2f, 0.5f, SpriteEffects.None, 0f);
            }

            return false;
        }
    }
}
