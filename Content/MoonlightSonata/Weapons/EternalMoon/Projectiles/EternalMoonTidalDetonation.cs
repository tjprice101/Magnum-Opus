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
    /// 
    /// REWORKED FOUNDATION ARCHITECTURE:
    /// - XSlashFoundation: TidalXSlashEffect spawned for the blazing X-shaped cross VFX
    /// - ImpactFoundation: TidalRippleEffect spawned for expanding shockwave rings
    /// - ThinSlashFoundation: TidalThinSlash spawned for razor-thin impact marks
    /// - Existing particle cascade (bloom, sparks, smoke, notes) retained for layered richness
    /// 
    /// The detonation itself handles damage, screen shake, and particle spawning.
    /// Foundation VFX projectiles handle shader-driven rendering.
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
            Player owner = Main.player[Projectile.owner];
            var emPlayer = owner.EternalMoon();
            float tidalMult = emPlayer.TidalPhaseMultiplier;

            // === FOUNDATION VFX: XSlashEffect — Blazing tidal X-cross ===
            if (Main.myPlayer == Projectile.owner)
            {
                float impactAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Projectile.NewProjectile(Projectile.GetSource_FromAI(),
                    Projectile.Center, Vector2.Zero,
                    ModContent.ProjectileType<TidalXSlashEffect>(),
                    0, 0f, Projectile.owner, impactAngle, tidalMult);
            }

            // === FOUNDATION VFX: RippleEffect — Expanding shockwave rings ===
            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromAI(),
                    Projectile.Center, Vector2.Zero,
                    ModContent.ProjectileType<TidalRippleEffect>(),
                    0, 0f, Projectile.owner, tidalMult);
            }

            // === FOUNDATION VFX: ThinSlash marks — 3 radial cuts through the detonation ===
            if (Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < 3; i++)
                {
                    float slashAngle = MathHelper.TwoPi * i / 3f + Main.rand.NextFloat(-0.2f, 0.2f);
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(),
                        Projectile.Center, Vector2.Zero,
                        ModContent.ProjectileType<TidalThinSlash>(),
                        0, 0f, Projectile.owner, slashAngle, 0f); // style 0 = Ice Cyan
                }
            }

            // === GRAVITATIONAL PULL at detonation center ===
            emPlayer.StartGravitationalPull(Projectile.Center);

            // Tidal Phase Ring — shows what phase the detonation occurred at
            if (emPlayer.TidalPhase >= 1)
            {
                Color phaseColor = EternalMoonPlayer.TidalPhaseColors[emPlayer.TidalPhase];
                LunarParticleHandler.SpawnParticle(new TidalPhaseRingParticle(
                    Projectile.Center, 1.5f * tidalMult, phaseColor, 35));
                LunarParticleHandler.SpawnParticle(new TidalPhaseRingParticle(
                    Projectile.Center, 2f * tidalMult, phaseColor * 0.5f, 45));
            }

            // Bloom cascade — scaled by tidal phase (reduced intensity)
            LunarParticleHandler.SpawnParticle(new LunarBloomParticle(Projectile.Center, 1.2f * tidalMult, EternalMoonUtils.MoonWhite, 20, 0.08f));
            LunarParticleHandler.SpawnParticle(new LunarBloomParticle(Projectile.Center, 1.5f * tidalMult, EternalMoonUtils.CrescentGlow, 25, 0.06f));
            LunarParticleHandler.SpawnParticle(new LunarBloomParticle(Projectile.Center, 1.8f * tidalMult, EternalMoonUtils.IceBlue, 30, 0.05f));
            LunarParticleHandler.SpawnParticle(new LunarBloomParticle(Projectile.Center, 2.2f * tidalMult, EternalMoonUtils.Violet, 35, 0.04f));
            LunarParticleHandler.SpawnParticle(new LunarBloomParticle(Projectile.Center, 2.5f * tidalMult, EternalMoonUtils.DarkPurple, 45, 0.035f));

            // Radial crescent spark explosion (count scales with tidal phase)
            int sparkCount = 20 + emPlayer.TidalPhase * 5;
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = MathHelper.TwoPi * i / sparkCount + Main.rand.NextFloat(-0.1f, 0.1f);
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(8f, 16f) * tidalMult;
                Color sparkColor = EternalMoonUtils.MulticolorLerp(Main.rand.NextFloat(),
                    EternalMoonUtils.IceBlue, EternalMoonUtils.CrescentGlow, EternalMoonUtils.MoonWhite);
                LunarParticleHandler.SpawnParticle(new CrescentSparkParticle(
                    Projectile.Center, sparkVel, Main.rand.NextFloat(0.6f, 1.2f),
                    sparkColor, Main.rand.Next(20, 35)));
            }

            // Wave spray ring — water-like spray radiating outward
            int sprayCount = 12 + emPlayer.TidalPhase * 4;
            for (int i = 0; i < sprayCount; i++)
            {
                float angle = MathHelper.TwoPi * i / sprayCount + Main.rand.NextFloat(-0.15f, 0.15f);
                Vector2 sprayVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 12f);
                Color sprayColor = Color.Lerp(EternalMoonUtils.MoonWhite, EternalMoonUtils.IceBlue, Main.rand.NextFloat());
                LunarParticleHandler.SpawnParticle(new WaveSprayParticle(
                    Projectile.Center + angle.ToRotationVector2() * 15f, sprayVel,
                    Main.rand.NextFloat(0.3f, 0.7f), sprayColor, Main.rand.Next(15, 25)));
            }

            // Tidal droplets falling from blast — gravity-affected water drops
            for (int i = 0; i < 10 + emPlayer.TidalPhase * 3; i++)
            {
                Vector2 dropVel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(3f, 8f);
                dropVel.Y -= Main.rand.NextFloat(2f, 5f); // Bias upward first
                Color dropColor = Color.Lerp(EternalMoonUtils.IceBlue, EternalMoonUtils.MoonWhite, Main.rand.NextFloat(0.3f));
                LunarParticleHandler.SpawnParticle(new TidalDropletParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(30f, 30f), dropVel,
                    Main.rand.NextFloat(0.3f, 0.6f), dropColor, Main.rand.Next(25, 45)));
            }

            // Moon glint sparkles scattered across the blast
            for (int i = 0; i < 8; i++)
            {
                Vector2 glintPos = Projectile.Center + Main.rand.NextVector2Circular(MaxExplosionRadius * 0.5f, MaxExplosionRadius * 0.5f);
                LunarParticleHandler.SpawnParticle(new MoonGlintParticle(
                    glintPos, Main.rand.NextFloat(0.3f, 0.6f), EternalMoonUtils.MoonWhite, Main.rand.Next(15, 30)));
            }

            // Gravity well motes spiraling inward at detonation point
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 motePos = Projectile.Center + angle.ToRotationVector2() * Main.rand.NextFloat(80f, 140f);
                LunarParticleHandler.SpawnParticle(new GravityWellMoteParticle(
                    motePos, Projectile.Center, Main.rand.NextFloat(0.3f, 0.5f),
                    Color.Lerp(EternalMoonUtils.Violet, EternalMoonUtils.IceBlue, Main.rand.NextFloat()),
                    Main.rand.Next(25, 40)));
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

            // Music note cascade — rising from the destruction (more at higher tidal phase)
            int noteCount = 8 + emPlayer.TidalPhase * 2;
            for (int i = 0; i < noteCount; i++)
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

            SpriteBatch sb = Main.spriteBatch;
            try
            {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Switch to additive for all detonation rendering
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            // Expanding bloom rings (original system, now complementing Foundation VFX)
            for (int ring = 0; ring < 3; ring++)
            {
                float ringProgress = Math.Clamp(progress - ring * 0.1f, 0f, 1f);
                float ringScale = ringProgress * 3f * (1 + ring * 0.3f);
                float ringOpacity = (1f - ringProgress) * 0.35f;

                Color ringColor = ring switch
                {
                    0 => EternalMoonUtils.MoonWhite,
                    1 => EternalMoonUtils.IceBlue,
                    _ => EternalMoonUtils.Violet
                };
                ringColor.A = 0;

                sb.Draw(texture, drawPos, null,
                    ringColor * ringOpacity, 0f, texture.Size() / 2f, ringScale, SpriteEffects.None, 0f);
            }

            // Central glow pulse that fades with the detonation
            float coreAlpha = (1f - progress * progress) * 0.6f;
            sb.Draw(texture, drawPos, null,
                EternalMoonUtils.IceBlue with { A = 0 } * coreAlpha,
                0f, texture.Size() / 2f, 0.8f + progress * 0.5f, SpriteEffects.None, 0f);

            // Restore standard blend state
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

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
