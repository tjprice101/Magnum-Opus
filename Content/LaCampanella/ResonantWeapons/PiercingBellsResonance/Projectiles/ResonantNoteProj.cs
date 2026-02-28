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
    /// Homing music note sub-projectile spawned by ResonantBlastProj.
    /// Seeks nearest enemy with gentle homing. Musical identity projectile.
    /// </summary>
    public class ResonantNoteProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/MusicNote";

        private const float HomingRange = 500f;
        private const float HomingStrength = 0.06f;
        private int noteFrame;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            // Homing
            int target = FindClosestEnemy();
            if (target >= 0)
            {
                NPC npc = Main.npc[target];
                Vector2 toTarget = Vector2.Normalize(npc.Center - Projectile.Center);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 12f, HomingStrength);
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Sparkle trail
            if (Main.rand.NextBool(3))
            {
                PiercingBellsParticleHandler.SpawnParticle(new BulletTracerParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(4, 4),
                    -Projectile.velocity * 0.03f,
                    Main.rand.Next(8, 15)));
            }

            Lighting.AddLight(Projectile.Center, PiercingBellsResonanceUtils.ResonancePalette[2].ToVector3() * 0.4f);
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
            for (int i = 0; i < 4; i++)
            {
                PiercingBellsParticleHandler.SpawnParticle(new BulletTracerParticle(
                    Projectile.Center, Main.rand.NextVector2Circular(3f, 3f),
                    Main.rand.Next(10, 18)));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            var tex = ModContent.Request<Texture2D>(Texture).Value;
            float pulse = 0.9f + (float)Math.Sin(Main.GameUpdateCount * 0.15f + Projectile.whoAmI) * 0.1f;
            Color drawColor = PiercingBellsResonanceUtils.ResonancePalette[2] * pulse;

            sb.Draw(tex, Projectile.Center - Main.screenPosition, null,
                drawColor, Projectile.rotation, tex.Size() / 2f, 0.6f * pulse, SpriteEffects.None, 0f);

            // Bloom behind note
            var bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow").Value;
            sb.Draw(bloomTex, Projectile.Center - Main.screenPosition, null,
                PiercingBellsResonanceUtils.ResonancePalette[1] * 0.25f, 0f, bloomTex.Size() / 2f, 0.2f, SpriteEffects.None, 0f);

            return false;
        }
    }
}
