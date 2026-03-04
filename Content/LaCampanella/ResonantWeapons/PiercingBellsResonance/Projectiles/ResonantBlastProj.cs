using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.PiercingBellsResonance.Utilities;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.PiercingBellsResonance.Particles;
using MagnumOpus.Content.LaCampanella;
using MagnumOpus.Content.LaCampanella.Debuffs;
using MagnumOpus.Content.FoundationWeapons.ImpactFoundation;
using MagnumOpus.Content.FoundationWeapons.ExplosionParticlesFoundation;
using ReLogic.Content;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.PiercingBellsResonance.Projectiles
{
    /// <summary>
    /// Resonant Blast 遯ｶ繝ｻdetonation AoE spawned at enemy position when alt-fire triggers.
    /// ai[0] = marker count consumed, ai[1] = 1 if Perfect Pitch.
    /// Expanding bell-shaped shockwave that damages all enemies in radius.
    /// Perfect Pitch (exactly 5 markers): 2x damage + applies Resonant Silence (enemies can't attack for 1s).
    /// </summary>
    public class ResonantBlastProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";

        private const int Duration = 30;
        private const float BaseRadius = 120f;

        private int MarkerCount => (int)Projectile.ai[0];
        private bool IsPerfectPitch => Projectile.ai[1] > 0f;
        private float DetonationRadius => BaseRadius + MarkerCount * 20f;
        private bool hasDetonated;

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Duration;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            int age = Duration - Projectile.timeLeft;

            // Detonate on frame 5
            if (age == 5 && !hasDetonated)
            {
                hasDetonated = true;
                float radius = DetonationRadius;

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.CanBeChasedBy()) continue;
                    if (Vector2.Distance(Projectile.Center, npc.Center) > radius) continue;

                    int dir = Projectile.Center.X < npc.Center.X ? 1 : -1;
                    npc.SimpleStrikeNPC(Projectile.damage, dir, false, Projectile.knockBack);
                    npc.GetGlobalNPC<ResonantTollNPC>().AddStacks(npc, MarkerCount);

                    // Perfect Pitch: Resonant Silence 遯ｶ繝ｻstun 1s (confuse debuff as proxy)
                    if (IsPerfectPitch)
                    {
                        npc.AddBuff(BuffID.Confused, 60);
                    }
                }

                // VFX burst
                int burstCount = IsPerfectPitch ? 18 : 10;
                for (int i = 0; i < burstCount; i++)
                {
                    float angle = MathHelper.TwoPi / burstCount * i;
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(2f, 5f);
                    PiercingBellsParticleHandler.SpawnParticle(new ResonantNoteParticle(
                        Projectile.Center + vel * 5f, vel, Main.rand.Next(40, 70)));
                }

                PiercingBellsParticleHandler.SpawnParticle(new ResonantBlastFlashParticle(
                    Projectile.Center, IsPerfectPitch ? 4f : 2.5f, IsPerfectPitch ? 20 : 15));

                SoundEngine.PlaySound(SoundID.Item14 with { Pitch = IsPerfectPitch ? 0.5f : 0.2f, Volume = IsPerfectPitch ? 1.2f : 0.9f }, Projectile.Center);

                // === FOUNDATION: RippleEffectProjectile — Resonant Detonation shockwave ===
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                    ModContent.ProjectileType<RippleEffectProjectile>(),
                    0, 0f, Projectile.owner);

                // === FOUNDATION: SparkExplosionProjectile — Detonation spark burst (scales with markers) ===
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                    ModContent.ProjectileType<SparkExplosionProjectile>(),
                    0, 0f, Projectile.owner,
                    ai0: (float)SparkMode.RadialScatter);

                // === FOUNDATION: DamageZoneProjectile — Resonant Detonation zone (Perfect Pitch only) ===
                if (IsPerfectPitch)
                {
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                        ModContent.ProjectileType<DamageZoneProjectile>(),
                        (int)(Projectile.damage * 0.3f), 0f, Projectile.owner);
                }
            }

            // Light
            float fade = (float)Projectile.timeLeft / Duration;
            float lightIntensity = fade * (IsPerfectPitch ? 1.2f : 0.7f);
            Lighting.AddLight(Projectile.Center, PiercingBellsResonanceUtils.ResonancePalette[2].ToVector3() * lightIntensity);
        }

        public override bool? CanDamage() => false; // Damage handled manually

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            var tex = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
            int age = Duration - Projectile.timeLeft;
            float progress = (float)age / Duration;
            float fade = 1f - progress;

            Vector2 screenPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;

            // Expanding detonation ring
            float ringRadius = DetonationRadius * (float)Math.Sqrt(progress);
            float ringScale = ringRadius / (tex.Width * 0.5f);

            try { sb.End(); } catch { }
            try
            {
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Color ringColor = IsPerfectPitch
                ? Color.Lerp(new Color(255, 255, 200), new Color(255, 200, 60), progress) * fade * 0.4f
                : PiercingBellsResonanceUtils.ResonancePalette[2] * fade * 0.3f;
            sb.Draw(tex, screenPos, null, ringColor, 0f, origin, ringScale, SpriteEffects.None, 0f);

            // Inner blast core
            if (progress < 0.4f)
            {
                float coreFade = 1f - progress / 0.4f;
                Color coreColor = IsPerfectPitch
                    ? new Color(255, 255, 220) * coreFade * 0.6f
                    : PiercingBellsResonanceUtils.ResonancePalette[3] * coreFade * 0.5f;
                sb.Draw(tex, screenPos, null, coreColor, 0f, origin, 0.4f * coreFade, SpriteEffects.None, 0f);
            }

            // Outer glow
            Color outerColor = PiercingBellsResonanceUtils.ResonancePalette[1] * fade * 0.2f;
            sb.Draw(tex, screenPos, null, outerColor, 0f, origin, ringScale * 1.2f, SpriteEffects.None, 0f);
            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }

            } // end outer try
            catch
            {
                try
                {
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }
                catch { }
            }
            return false;
        }
    }
}