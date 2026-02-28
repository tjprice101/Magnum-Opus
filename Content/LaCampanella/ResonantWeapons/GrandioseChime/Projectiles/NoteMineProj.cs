using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Utilities;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Particles;
using MagnumOpus.Content.LaCampanella.Debuffs;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Projectiles
{
    /// <summary>
    /// Music note mine  Eslow-drifting proximity mine deployed every 4th shot.
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
            Projectile.Kill();
        }

        public override bool? CanDamage() => false; // Damage is handled by Detonate()

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            var tex = ModContent.Request<Texture2D>(Texture).Value;
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
                var bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow").Value;
                float ringAlpha = 0.1f + (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.05f;
                float ringScale = DetonationRadius / (bloomTex.Width * 0.5f);
                sb.Draw(bloomTex, Projectile.Center - Main.screenPosition, null,
                    GrandioseChimeUtils.MinePalette[2] * ringAlpha, 0f, bloomTex.Size() / 2f, ringScale, SpriteEffects.None, 0f);
            }

            return false;
        }
    }
}
