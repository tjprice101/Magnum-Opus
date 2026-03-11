using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Utilities;
using MagnumOpus.Common.Systems;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Particles;
using MagnumOpus.Content.LaCampanella.Debuffs;
using MagnumOpus.Content.FoundationWeapons.ImpactFoundation;
using ReLogic.Content;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Projectiles
{
    /// <summary>
    /// Kill Echo 遯ｶ繝ｻspawns at enemy death location, seeks nearest enemy within range,
    /// deals 60% of killing blow damage, then chains again (up to 3 total chains).
    /// ai[0] = chain range, ai[1] = current chain depth.
    /// </summary>
    public class KillEchoProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";

        private const int SeekDuration = 15; // Frames to find and strike target
        private int targetNPC = -1;
        private bool hasStruck;
        private float ChainRange => Projectile.ai[0];
        private int ChainDepth => (int)Projectile.ai[1];
        private VertexStrip _strip;

        public override void SetStaticDefaults()
        {
            Terraria.ID.ProjectileID.Sets.TrailCacheLength[Type] = 16;
            Terraria.ID.ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 30;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            int age = 30 - Projectile.timeLeft;

            // Find target on first frame
            if (age == 0)
            {
                targetNPC = FindNearestEnemy();
                if (targetNPC < 0)
                {
                    Projectile.Kill();
                    return;
                }
            }

            // Home toward target rapidly
            if (targetNPC >= 0 && targetNPC < Main.maxNPCs && Main.npc[targetNPC].active)
            {
                NPC target = Main.npc[targetNPC];
                Vector2 toTarget = target.Center - Projectile.Center;
                float dist = toTarget.Length();

                if (dist > 4f)
                {
                    Projectile.velocity = Vector2.Normalize(toTarget) * Math.Min(dist, 20f);
                }
            }

            // Echo trail particles
            if (Main.rand.NextBool(2))
            {
                GrandioseChimeParticleHandler.SpawnParticle(new BurningNoteParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(4, 4),
                    -Projectile.velocity * 0.05f,
                    Main.rand.Next(10, 18)));
            }

            Lighting.AddLight(Projectile.Center, GrandioseChimeUtils.EchoPalette[0].ToVector3() * 0.5f);
        }

        private int FindNearestEnemy()
        {
            int closest = -1;
            float closestDist = ChainRange;
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
            hasStruck = true;

            // VFX at strike point
            GrandioseChimeParticleHandler.SpawnParticle(new KillEchoParticle(target.Center, 1.5f, 12));

            // === FOUNDATION: RippleEffectProjectile — Kill Echo chain terminus ring ===
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), target.Center, Vector2.Zero,
                ModContent.ProjectileType<RippleEffectProjectile>(),
                0, 0f, Projectile.owner, ai0: 1f);

            // If this kill echo killed the target, chain further
            if (target.life <= 0 && ChainDepth < 2)
            {
                // Chain again from this position
                int nextDmg = (int)(Projectile.damage * 0.6f);
                if (nextDmg > 0)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), target.Center, Vector2.Zero,
                        ModContent.ProjectileType<KillEchoProj>(), nextDmg, 4f, Projectile.owner,
                        ai0: ChainRange, ai1: ChainDepth + 1);
                }

                // If this was chain depth 2 (the final chain completing a full 3-chain),
                // register a full chain kill for Grandiose Crescendo
                if (ChainDepth + 1 >= 2)
                {
                    Player owner = Main.player[Projectile.owner];
                    var modPlayer = owner.GetModPlayer<GrandioseChimePlayer>();
                    modPlayer.RegisterFullChainKill();
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            IncisorOrbRenderer.DrawOrbVisuals(Main.spriteBatch, Projectile, IncisorOrbRenderer.LaCampanella, ref _strip);
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
    }
}
