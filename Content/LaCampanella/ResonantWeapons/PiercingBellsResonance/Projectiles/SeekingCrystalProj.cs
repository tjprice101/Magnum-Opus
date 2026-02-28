using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.PiercingBellsResonance.Utilities;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.PiercingBellsResonance.Particles;
using MagnumOpus.Content.LaCampanella.Debuffs;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.PiercingBellsResonance.Projectiles
{
    /// <summary>
    /// Seeking crystal sub-projectile spawned by ResonantBlastProj.
    /// Crystalline fire shard that aggressively homes toward enemies with a glowing aura.
    /// </summary>
    public class SeekingCrystalProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarSoft";

        private const float HomingRange = 600f;
        private const float HomingStrength = 0.08f;
        private float crystalRotation;

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 150;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            crystalRotation += 0.12f;

            // Aggressive homing
            int target = FindClosestEnemy();
            if (target >= 0)
            {
                NPC npc = Main.npc[target];
                Vector2 toTarget = Vector2.Normalize(npc.Center - Projectile.Center);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 14f, HomingStrength);
            }
            else
            {
                // Drift if no target
                Projectile.velocity *= 0.98f;
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Crystal ember trail
            if (Main.rand.NextBool(3))
            {
                PiercingBellsParticleHandler.SpawnParticle(new BulletTracerParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(5, 5),
                    -Projectile.velocity * 0.04f + Main.rand.NextVector2Circular(0.8f, 0.8f),
                    Main.rand.Next(10, 20)));
            }

            Lighting.AddLight(Projectile.Center, PiercingBellsResonanceUtils.CrystalPalette[1].ToVector3() * 0.5f);
        }

        private int FindClosestEnemy()
        {
            int closest = -1;
            float closestDist = HomingRange;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy()) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = i;
                }
            }
            return closest;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);
        }

        public override void OnKill(int timeLeft)
        {
            // Crystal shatter
            for (int i = 0; i < 6; i++)
            {
                PiercingBellsParticleHandler.SpawnParticle(new BulletTracerParticle(
                    Projectile.Center, Main.rand.NextVector2Circular(4f, 4f),
                    Main.rand.Next(10, 20)));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            var tex = ModContent.Request<Texture2D>(Texture).Value;

            // Rotating crystal with color pulse
            float pulse = 0.85f + (float)Math.Sin(Main.GameUpdateCount * 0.2f + Projectile.whoAmI * 2f) * 0.15f;
            Color crystalColor = PiercingBellsResonanceUtils.MulticolorLerp(
                (float)(Math.Sin(Main.GameUpdateCount * 0.1f + Projectile.whoAmI) * 0.5f + 0.5f),
                PiercingBellsResonanceUtils.CrystalPalette);

            sb.Draw(tex, Projectile.Center - Main.screenPosition, null,
                crystalColor * pulse, crystalRotation, tex.Size() / 2f, 0.5f * pulse, SpriteEffects.None, 0f);

            // Crystal aura glow
            var bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow").Value;
            Color auraColor = PiercingBellsResonanceUtils.CrystalPalette[2] * 0.2f;
            sb.Draw(bloomTex, Projectile.Center - Main.screenPosition, null,
                auraColor, 0f, bloomTex.Size() / 2f, 0.25f, SpriteEffects.None, 0f);

            return false;
        }
    }
}
