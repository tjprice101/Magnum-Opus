using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.SymphonicBellfireAnnihilator.Utilities;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.SymphonicBellfireAnnihilator.Particles;
using MagnumOpus.Content.LaCampanella.Debuffs;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.SymphonicBellfireAnnihilator.Projectiles
{
    /// <summary>
    /// Grand Crescendo Wave  Emassive AoE explosion triggered every 3rd volley completion.
    /// Expanding ring of divine fire that damages all enemies in massive radius.
    /// </summary>
    public class GrandCrescendoWaveProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow";

        private const int Duration = 40;
        private const float MaxRadius = 400f;

        public override void SetDefaults()
        {
            Projectile.width = (int)(MaxRadius * 2);
            Projectile.height = (int)(MaxRadius * 2);
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = Duration;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            float progress = 1f - (float)Projectile.timeLeft / Duration;

            // Spawn VFX
            if (Projectile.timeLeft == Duration)
            {
                // Massive crescendo ring
                SymphonicBellfireParticleHandler.SpawnParticle(new CrescendoWaveParticle(
                    Projectile.Center, MaxRadius, Duration - 5));

                // Inner flash
                SymphonicBellfireParticleHandler.SpawnParticle(new ExplosionFireballParticle(
                    Projectile.Center, 4f, Duration / 2));

                // Musical note orchestra burst
                for (int i = 0; i < 16; i++)
                {
                    float angle = MathHelper.TwoPi / 16f * i;
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(3f, 7f);
                    SymphonicBellfireParticleHandler.SpawnParticle(new SymphonicNoteParticle(
                        Projectile.Center + vel * 5f, vel, Main.rand.Next(50, 90)));
                }

                // Radial ember shower
                for (int i = 0; i < 30; i++)
                {
                    SymphonicBellfireParticleHandler.SpawnParticle(new RocketExhaustParticle(
                        Projectile.Center,
                        Main.rand.NextVector2Circular(8f, 8f),
                        Main.rand.NextFloat(3f, 6f),
                        Main.rand.Next(20, 40)));
                }

                SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.3f, Volume = 1.2f }, Projectile.Center);
            }

            // Continuous ember ring at edge
            if (Projectile.timeLeft > Duration / 2 && Projectile.timeLeft % 2 == 0)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = progress * MaxRadius;
                Vector2 pos = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                SymphonicBellfireParticleHandler.SpawnParticle(new RocketExhaustParticle(
                    pos, Main.rand.NextVector2Circular(2f, 2f), 2f, Main.rand.Next(10, 25)));
            }

            // Lighting
            float intensity = (1f - progress) * 2f;
            Lighting.AddLight(Projectile.Center, SymphonicBellfireUtils.CrescendoPalette[1].ToVector3() * intensity);
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
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 5);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            float progress = 1f - (float)Projectile.timeLeft / Duration;
            float fade = 1f - progress;

            SymphonicBellfireParticleHandler.DrawAllParticles(sb);

            var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow").Value;
            float ringRadius = MaxRadius * (float)Math.Sqrt(progress);
            float ringScale = ringRadius / (tex.Width * 0.5f);

            // Outer wave
            Color outerColor = SymphonicBellfireUtils.CrescendoPalette[0] * fade * 0.3f;
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null,
                outerColor, 0f, tex.Size() / 2f, ringScale, SpriteEffects.None, 0f);

            // Inner blaze
            Color innerColor = SymphonicBellfireUtils.CrescendoPalette[1] * fade * 0.4f;
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null,
                innerColor, 0f, tex.Size() / 2f, ringScale * 0.6f, SpriteEffects.None, 0f);

            // Center divine flash
            if (progress < 0.25f)
            {
                Color flash = SymphonicBellfireUtils.CrescendoPalette[2] * (1f - progress / 0.25f) * 0.7f;
                sb.Draw(tex, Projectile.Center - Main.screenPosition, null,
                    flash, 0f, tex.Size() / 2f, 0.6f, SpriteEffects.None, 0f);
            }

            return false;
        }
    }
}
