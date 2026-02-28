using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Utilities;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Particles;
using MagnumOpus.Content.LaCampanella.Debuffs;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Projectiles
{
    /// <summary>
    /// Kill echo projectile  Espawns at enemy death location, fires burst of homing echo shards.
    /// Spectral afterimage that replays the killing blow.
    /// </summary>
    public class KillEchoProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow";

        private const int Duration = 25;
        private const float EchoRadius = 180f;
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

            // Frame 5: radial burst damage
            if (age == 5 && !hasDetonated)
            {
                hasDetonated = true;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.CanBeChasedBy()) continue;
                    if (Vector2.Distance(Projectile.Center, npc.Center) <= EchoRadius)
                    {
                        int dir = Projectile.Center.X < npc.Center.X ? 1 : -1;
                        npc.SimpleStrikeNPC(Projectile.damage, dir, false, Projectile.knockBack);
                        npc.GetGlobalNPC<ResonantTollNPC>().AddStacks(npc, 1);
                    }
                }

                // Echo burst VFX
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi / 6f * i;
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 4f;
                    GrandioseChimeParticleHandler.SpawnParticle(new BurningNoteParticle(
                        Projectile.Center, vel, Main.rand.Next(25, 40)));
                }
            }

            // Fading echo glow
            float intensity = (1f - (float)age / Duration) * 0.6f;
            Lighting.AddLight(Projectile.Center, GrandioseChimeUtils.EchoPalette[0].ToVector3() * intensity);
        }

        public override bool? CanDamage() => false; // Damage handled manually

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            var tex = ModContent.Request<Texture2D>(Texture).Value;
            int age = Duration - Projectile.timeLeft;
            float progress = (float)age / Duration;
            float fade = 1f - progress;

            // Expanding echo ring
            float ringScale = EchoRadius * (float)Math.Sqrt(progress) / (tex.Width * 0.5f);
            Color ringColor = GrandioseChimeUtils.EchoPalette[0] * fade * 0.25f;
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null,
                ringColor, 0f, tex.Size() / 2f, ringScale, SpriteEffects.None, 0f);

            // Core flash
            if (progress < 0.3f)
            {
                Color flash = GrandioseChimeUtils.EchoPalette[1] * (1f - progress / 0.3f) * 0.5f;
                sb.Draw(tex, Projectile.Center - Main.screenPosition, null,
                    flash, 0f, tex.Size() / 2f, 0.3f, SpriteEffects.None, 0f);
            }

            return false;
        }
    }
}
