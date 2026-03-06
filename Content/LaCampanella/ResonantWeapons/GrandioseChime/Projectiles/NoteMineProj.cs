using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Utilities;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Particles;
using MagnumOpus.Content.LaCampanella;
using MagnumOpus.Content.LaCampanella.Debuffs;
using MagnumOpus.Content.FoundationWeapons.ImpactFoundation;
using MagnumOpus.Content.FoundationWeapons.ExplosionParticlesFoundation;
using ReLogic.Content;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Projectiles
{
    /// <summary>
    /// Music note mine 遯ｶ繝ｻslow-drifting proximity mine deployed every 4th shot.
    /// Hovers in place, detonates when enemy approaches within radius. AoE explosion.
    /// </summary>
    public class NoteMineProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/MusicNoteWithSlashes";

        private const float DetonationRadius = 120f;
        private const int ArmTime = 20; // Frames before mine can detonate
        private float hoverAngle;

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 360; // 6 seconds
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1; // Hit once per enemy
        }

        public override void AI()
        {
            // Decelerate to hover
            Projectile.velocity *= 0.94f;
            hoverAngle += 0.04f;
            Projectile.position.Y += (float)Math.Sin(hoverAngle) * 0.3f;

            Projectile.rotation += 0.02f;

            // Proximity check after arming
            int age = 360 - Projectile.timeLeft;
            if (age >= ArmTime)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.CanBeChasedBy()) continue;
                    if (Vector2.Distance(Projectile.Center, npc.Center) <= DetonationRadius)
                    {
                        Detonate();
                        return;
                    }
                }
            }

            // Ambient glow pulse
            if (age > ArmTime && Main.rand.NextBool(8))
            {
                GrandioseChimeParticleHandler.SpawnParticle(new BurningNoteParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(10, 10),
                    Main.rand.NextVector2Circular(0.5f, 0.5f),
                    Main.rand.Next(15, 25)));
            }

            float glowPulse = age >= ArmTime
                ? 0.4f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.1f
                : 0.15f;
            Lighting.AddLight(Projectile.Center, GrandioseChimeUtils.MinePalette[1].ToVector3() * glowPulse);
        }

        private void Detonate()
        {
            // AoE damage
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy()) continue;
                if (Vector2.Distance(Projectile.Center, npc.Center) <= DetonationRadius * 1.2f)
                {
                    int dir = Projectile.Center.X < npc.Center.X ? 1 : -1;
                    npc.SimpleStrikeNPC(Projectile.damage, dir, false, Projectile.knockBack);
                    npc.GetGlobalNPC<ResonantTollNPC>().AddStacks(npc, 2);
                }
            }

            // Detonation VFX
            GrandioseChimeParticleHandler.SpawnParticle(new MineDetonationPulseParticle(
                Projectile.Center, DetonationRadius, 20));

            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(3f, 3f);
                GrandioseChimeParticleHandler.SpawnParticle(new BurningNoteParticle(
                    Projectile.Center, vel, Main.rand.Next(25, 40)));
            }

            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.3f, Volume = 0.7f }, Projectile.Center);

            // === FOUNDATION: RippleEffectProjectile — Mine detonation shockwave ===
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                ModContent.ProjectileType<RippleEffectProjectile>(),
                0, 0f, Projectile.owner, ai0: 1f);

            // === FOUNDATION: SparkExplosionProjectile — Mine detonation spark burst ===
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                ModContent.ProjectileType<SparkExplosionProjectile>(),
                0, 0f, Projectile.owner,
                ai0: (float)SparkMode.RadialScatter);

            Projectile.Kill();
        }

        public override bool? CanDamage() => false; // Damage is handled by Detonate()

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            var tex = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
            int age = 360 - Projectile.timeLeft;
            bool armed = age >= ArmTime;

            float pulse = armed
                ? 0.9f + (float)Math.Sin(Main.GameUpdateCount * 0.15f + Projectile.whoAmI) * 0.1f
                : 0.5f;

            Color mineColor = armed
                ? GrandioseChimeUtils.MinePalette[1] * pulse
                : GrandioseChimeUtils.MinePalette[0] * 0.5f;

            sb.Draw(tex, Projectile.Center - Main.screenPosition, null,
                mineColor, Projectile.rotation, tex.Size() / 2f, 0.6f, SpriteEffects.None, 0f);

            // Proximity detection ring (when armed)
            if (armed)
            {
                var bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow", AssetRequestMode.ImmediateLoad).Value;
                float ringAlpha = 0.1f + (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.05f;
                float ringScale = DetonationRadius / (bloomTex.Width * 0.5f);

                // Draw bloom ring + LC ring in Additive
                try { sb.End(); } catch { }
                try
                {
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                sb.Draw(bloomTex, Projectile.Center - Main.screenPosition, null,
                    GrandioseChimeUtils.MinePalette[2] * ringAlpha, 0f, bloomTex.Size() / 2f, ringScale, SpriteEffects.None, 0f);

                // LC Power Effect Ring - pulsing concentric ring on armed mine
                float minePulse = 0.5f + 0.5f * (float)Math.Sin(Main.GameUpdateCount * 0.12f + Projectile.whoAmI);
                LaCampanellaVFXLibrary.DrawPowerEffectRing(sb, Projectile.Center - Main.screenPosition,
                    0.2f * minePulse, (float)Main.GameUpdateCount * 0.03f,
                    0.2f * minePulse, LaCampanellaPalette.InfernalOrange);
                }
                catch { }
                finally
                {
                    try { sb.End(); } catch { }
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }
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