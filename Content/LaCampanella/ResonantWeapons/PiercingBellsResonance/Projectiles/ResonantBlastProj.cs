using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Audio;
using Terraria.ModLoader;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.PiercingBellsResonance.Utilities;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.PiercingBellsResonance.Particles;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.PiercingBellsResonance.Primitives;
using MagnumOpus.Content.LaCampanella.Debuffs;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.PiercingBellsResonance.Projectiles
{
    /// <summary>
    /// Resonant Blast  Ethe powerful 20th-shot projectile.
    /// Larger, faster, penetrating shot that on impact/expiry spawns:
    /// - 4 homing ResonantNoteProj (music notes that seek enemies)
    /// - 3 seeking SeekingCrystalProj (crystal shards with aura)
    /// </summary>
    public class ResonantBlastProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow";

        private List<Vector2> trailPositions = new List<Vector2>();
        private const int MaxTrailPoints = 18;
        private PiercingBellsPrimitiveRenderer trailRenderer;
        private bool hasExploded;

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > MaxTrailPoints)
                trailPositions.RemoveAt(trailPositions.Count - 1);

            // Intense fire trail
            if (Main.rand.NextBool(2))
            {
                PiercingBellsParticleHandler.SpawnParticle(new BulletTracerParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(8, 8),
                    -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(2f, 2f),
                    Main.rand.Next(15, 25)));
            }

            // Musical note trail
            if (Main.rand.NextBool(6))
            {
                PiercingBellsParticleHandler.SpawnParticle(new ResonantNoteParticle(
                    Projectile.Center, -Projectile.velocity * 0.03f + Main.rand.NextVector2Circular(1f, 1f),
                    Main.rand.Next(30, 50)));
            }

            Projectile.rotation = Projectile.velocity.ToRotation();
            Lighting.AddLight(Projectile.Center, PiercingBellsResonanceUtils.ResonancePalette[2].ToVector3() * 0.8f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 2);

            // Spawn sub-projectiles on first hit
            if (!hasExploded)
            {
                SpawnSubProjectiles(target.Center);
                hasExploded = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            trailRenderer?.Dispose();
            trailRenderer = null;

            if (!hasExploded)
                SpawnSubProjectiles(Projectile.Center);

            // Explosion VFX
            PiercingBellsParticleHandler.SpawnParticle(new ResonantBlastFlashParticle(
                Projectile.Center, 3f, 20));

            for (int i = 0; i < 10; i++)
            {
                PiercingBellsParticleHandler.SpawnParticle(new BulletTracerParticle(
                    Projectile.Center, Main.rand.NextVector2Circular(5f, 5f),
                    Main.rand.Next(15, 30)));
            }

            for (int i = 0; i < 8; i++)
            {
                Vector2 noteVel = Main.rand.NextVector2CircularEdge(3f, 3f);
                PiercingBellsParticleHandler.SpawnParticle(new ResonantNoteParticle(
                    Projectile.Center, noteVel, Main.rand.Next(40, 70)));
            }

            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
        }

        private void SpawnSubProjectiles(Vector2 center)
        {
            int baseDmg = Projectile.damage / 3;

            // 4 homing music note projectiles
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi / 4f * i + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 8f;
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), center, vel,
                    ModContent.ProjectileType<ResonantNoteProj>(), baseDmg, 2f, Projectile.owner);
            }

            // 3 seeking crystal projectiles
            for (int i = 0; i < 3; i++)
            {
                float angle = MathHelper.TwoPi / 3f * i + MathHelper.Pi / 6f;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 6f;
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), center, vel,
                    ModContent.ProjectileType<SeekingCrystalProj>(), (int)(baseDmg * 0.8f), 3f, Projectile.owner);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            PiercingBellsParticleHandler.DrawAllParticles(sb);

            // Trail
            if (trailPositions.Count >= 2)
            {
                try
                {
                    trailRenderer ??= new PiercingBellsPrimitiveRenderer();
                    var settings = new BulletTrailSettings
                    {
                        ColorStart = PiercingBellsResonanceUtils.ResonancePalette[2],
                        ColorEnd = PiercingBellsResonanceUtils.ResonancePalette[0] * 0.3f,
                        Width = 14f,
                        BloomIntensity = 0.5f
                    };
                    trailRenderer.DrawTrail(sb, trailPositions, settings, Main.screenPosition);
                }
                catch { }
            }

            // Draw glowing orb core
            var tex = ModContent.Request<Texture2D>(Texture).Value;
            float pulse = 0.8f + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.2f;
            Color coreColor = PiercingBellsResonanceUtils.ResonancePalette[3] * pulse;
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null,
                coreColor, 0f, tex.Size() / 2f, 0.3f * pulse, SpriteEffects.None, 0f);

            // Outer glow
            Color outerColor = PiercingBellsResonanceUtils.ResonancePalette[1] * 0.4f;
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null,
                outerColor, 0f, tex.Size() / 2f, 0.5f, SpriteEffects.None, 0f);

            return false;
        }
    }
}
