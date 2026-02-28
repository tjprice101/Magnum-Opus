using System;
using MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Buffs;
using MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Particles;
using MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Projectiles
{
    /// <summary>
    /// Tidal Detonation — a massive area-of-effect explosion spawned on Full Moon empowered hit.
    /// A sphere of crushing tidal moonlight that expands outward, dealing heavy damage to all
    /// nearby enemies. Features layered visual effects:
    ///   - Expanding bloom rings in the lunar palette
    ///   - Cascading crescent sparks radiating outward  
    ///   - Heavy tidal smoke lingering in the aftermath
    ///   - Music note particles floating upward through the destruction
    ///   - Screen shake at detonation
    /// </summary>
    public class EternalMoonTidalDetonation : ModProjectile
    {
        private const int DetonationLifetime = 30;
        private const float MaxExplosionRadius = 250f;
        private bool _initialBurstDone;

        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom";

        public override void SetDefaults()
        {
            Projectile.width = (int)(MaxExplosionRadius * 2);
            Projectile.height = (int)(MaxExplosionRadius * 2);
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = DetonationLifetime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = DetonationLifetime;
            Projectile.Opacity = 1f;
        }

        public override void AI()
        {
            float progress = 1f - (Projectile.timeLeft / (float)DetonationLifetime);

            // Initial burst on first frame
            if (!_initialBurstDone)
            {
                _initialBurstDone = true;
                SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.9f, Pitch = -0.5f }, Projectile.Center);
                SoundEngine.PlaySound(SoundID.Item105 with { Volume = 0.4f, Pitch = 0.3f }, Projectile.Center);

                // Screen shake
                if (Main.LocalPlayer.Distance(Projectile.Center) < 1500f)
                {
                    float shakeIntensity = MathHelper.Clamp(1f - Main.LocalPlayer.Distance(Projectile.Center) / 1500f, 0f, 1f);
                    Main.LocalPlayer.velocity += Main.rand.NextVector2Circular(3f, 3f) * shakeIntensity;
                }

                if (!Main.dedServ)
                    SpawnInitialBurst();
            }

            // Ongoing effects
            Projectile.Opacity = 1f - (float)Math.Pow(progress, 2);
            Projectile.scale = 0.5f + progress * 1.5f;

            // Ongoing smoke
            if (Main.rand.NextBool(3) && !Main.dedServ && progress < 0.7f)
            {
                Vector2 smokePos = Projectile.Center + Main.rand.NextVector2Circular(MaxExplosionRadius * progress, MaxExplosionRadius * progress);
                Vector2 smokeVel = (smokePos - Projectile.Center).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 2f);
                LunarParticleHandler.SpawnParticle(new TidalSmokeParticle(
                    smokePos, smokeVel, Main.rand.NextFloat(0.2f, 0.5f),
                    Color.Lerp(EternalMoonUtils.DarkPurple, EternalMoonUtils.NightPurple, Main.rand.NextFloat()),
                    Main.rand.Next(50, 80)));
            }

            // Moonlight
            float lightIntensity = Projectile.Opacity * 1.5f;
            Lighting.AddLight(Projectile.Center, EternalMoonUtils.IceBlue.ToVector3() * lightIntensity);
        }

        private void SpawnInitialBurst()
        {
            // Large bloom cascade
            LunarParticleHandler.SpawnParticle(new LunarBloomParticle(Projectile.Center, 2f, EternalMoonUtils.MoonWhite, 25, 0.1f));
            LunarParticleHandler.SpawnParticle(new LunarBloomParticle(Projectile.Center, 2.5f, EternalMoonUtils.CrescentGlow, 30, 0.08f));
            LunarParticleHandler.SpawnParticle(new LunarBloomParticle(Projectile.Center, 3f, EternalMoonUtils.IceBlue, 35, 0.06f));
            LunarParticleHandler.SpawnParticle(new LunarBloomParticle(Projectile.Center, 3.5f, EternalMoonUtils.Violet, 40, 0.05f));
            LunarParticleHandler.SpawnParticle(new LunarBloomParticle(Projectile.Center, 4f, EternalMoonUtils.DarkPurple, 50, 0.04f));

            // Radial crescent spark explosion (20 sparks)
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f + Main.rand.NextFloat(-0.1f, 0.1f);
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(8f, 16f);
                Color sparkColor = EternalMoonUtils.MulticolorLerp(Main.rand.NextFloat(),
                    EternalMoonUtils.IceBlue, EternalMoonUtils.CrescentGlow, EternalMoonUtils.MoonWhite);
                LunarParticleHandler.SpawnParticle(new CrescentSparkParticle(
                    Projectile.Center, sparkVel, Main.rand.NextFloat(0.6f, 1.2f),
                    sparkColor, Main.rand.Next(20, 35)));
            }

            // Tidal mote ring
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 moteVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                LunarParticleHandler.SpawnParticle(new TidalMoteParticle(
                    Projectile.Center + angle.ToRotationVector2() * 20f, moteVel,
                    Main.rand.NextFloat(0.4f, 0.8f), EternalMoonUtils.IceBlue,
                    Main.rand.Next(35, 55)));
            }

            // Music note cascade — rising from the destruction
            for (int i = 0; i < 8; i++)
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3.5f, -1f));
                LunarParticleHandler.SpawnParticle(new LunarNoteParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(40f, 40f),
                    noteVel, Main.rand.NextFloat(0.4f, 0.8f), Main.rand.Next(60, 90)));
            }

            // Heavy tidal smoke ring
            for (int i = 0; i < 12; i++)
            {
                Vector2 smokeVel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(1f, 3f);
                LunarParticleHandler.SpawnParticle(new TidalSmokeParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(50f, 50f),
                    smokeVel, Main.rand.NextFloat(0.3f, 0.7f),
                    Color.Lerp(EternalMoonUtils.DarkPurple, EternalMoonUtils.NightPurple, Main.rand.NextFloat()),
                    Main.rand.Next(70, 110)));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            float progress = 1f - (Projectile.timeLeft / (float)DetonationLifetime);

            // Expanding rings
            for (int ring = 0; ring < 3; ring++)
            {
                float ringProgress = Math.Clamp(progress - ring * 0.1f, 0f, 1f);
                float ringScale = ringProgress * 3f * (1 + ring * 0.3f);
                float ringOpacity = (1f - ringProgress) * 0.4f;

                Color ringColor = ring switch
                {
                    0 => EternalMoonUtils.MoonWhite,
                    1 => EternalMoonUtils.IceBlue,
                    _ => EternalMoonUtils.Violet
                };
                ringColor.A = 0;

                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null,
                    ringColor * ringOpacity, 0f, texture.Size() / 2f, ringScale, SpriteEffects.None, 0f);
            }

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<TidalDrowning>(), 300);

            if (!Main.dedServ)
            {
                // Small spark burst per enemy hit
                for (int i = 0; i < 4; i++)
                {
                    Vector2 sparkVel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(3f, 6f);
                    LunarParticleHandler.SpawnParticle(new CrescentSparkParticle(
                        target.Center, sparkVel, Main.rand.NextFloat(0.3f, 0.6f),
                        EternalMoonUtils.CrescentGlow, 15));
                }
            }
        }
    }
}
