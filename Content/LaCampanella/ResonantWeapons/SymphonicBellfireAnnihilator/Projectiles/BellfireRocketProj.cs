using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.SymphonicBellfireAnnihilator.Utilities;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.SymphonicBellfireAnnihilator.Particles;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.SymphonicBellfireAnnihilator.Primitives;
using MagnumOpus.Content.LaCampanella.Debuffs;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.SymphonicBellfireAnnihilator.Projectiles
{
    /// <summary>
    /// Bellfire rocket  Ethe primary rocket projectile for SymphonicBellfireAnnihilator.
    /// ai[0]: 0 = normal, 1 = enhanced (shots 6-10), 2 = Grand Crescendo shot.
    /// Explodes on impact/enemy with AoE damage and fire trail.
    /// </summary>
    public class BellfireRocketProj : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RocketI;

        private List<Vector2> trailPositions = new List<Vector2>();
        private const int MaxTrailPoints = 16;
        private SymphonicBellfirePrimitiveRenderer trailRenderer;

        private int RocketTier => (int)Projectile.ai[0]; // 0=normal, 1=enhanced, 2=crescendo

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 150;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
        }

        public override void AI()
        {
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > MaxTrailPoints)
                trailPositions.RemoveAt(trailPositions.Count - 1);

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Scale VFX intensity by tier
            int trailFreq = RocketTier switch { 2 => 1, 1 => 2, _ => 3 };
            float exhaustStretch = RocketTier switch { 2 => 5f, 1 => 3.5f, _ => 2.5f };

            if (Main.rand.NextBool(trailFreq))
            {
                SymphonicBellfireParticleHandler.SpawnParticle(new RocketExhaustParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(4, 4),
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f),
                    exhaustStretch, Main.rand.Next(12, 22)));
            }

            // Musical notes for enhanced/crescendo
            if (RocketTier >= 1 && Main.rand.NextBool(8))
            {
                SymphonicBellfireParticleHandler.SpawnParticle(new SymphonicNoteParticle(
                    Projectile.Center, -Projectile.velocity * 0.04f + Main.rand.NextVector2Circular(1f, 1f),
                    Main.rand.Next(30, 50)));
            }

            float lightMul = RocketTier switch { 2 => 1.0f, 1 => 0.7f, _ => 0.4f };
            Lighting.AddLight(Projectile.Center, SymphonicBellfireUtils.RocketPalette[2].ToVector3() * lightMul);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            int stacks = RocketTier switch { 2 => 4, 1 => 2, _ => 1 };
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, stacks);
        }

        public override void OnKill(int timeLeft)
        {
            trailRenderer?.Dispose();
            trailRenderer = null;

            // Explosion
            float explosionSize = RocketTier switch { 2 => 3.5f, 1 => 2.5f, _ => 1.8f };
            int explosionDuration = RocketTier switch { 2 => 25, 1 => 18, _ => 12 };
            float explosionRadius = 80f + RocketTier * 40f;

            SymphonicBellfireParticleHandler.SpawnParticle(new ExplosionFireballParticle(
                Projectile.Center, explosionSize, explosionDuration));

            if (RocketTier >= 1)
            {
                SymphonicBellfireParticleHandler.SpawnParticle(new CrescendoWaveParticle(
                    Projectile.Center, explosionRadius, explosionDuration + 5));
            }

            // Shrapnel embers
            int shrapnelCount = 6 + RocketTier * 4;
            for (int i = 0; i < shrapnelCount; i++)
            {
                SymphonicBellfireParticleHandler.SpawnParticle(new RocketExhaustParticle(
                    Projectile.Center,
                    Main.rand.NextVector2Circular(6f, 6f),
                    Main.rand.NextFloat(2f, 4f),
                    Main.rand.Next(15, 30)));
            }

            // Musical notes on enhanced/crescendo
            if (RocketTier >= 1)
            {
                int noteCount = 4 + RocketTier * 3;
                for (int i = 0; i < noteCount; i++)
                {
                    Vector2 noteVel = Main.rand.NextVector2CircularEdge(3f, 3f) + new Vector2(0, -1f);
                    SymphonicBellfireParticleHandler.SpawnParticle(new SymphonicNoteParticle(
                        Projectile.Center, noteVel, Main.rand.Next(40, 65)));
                }
            }

            // AoE damage on explosion
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy()) continue;
                if (Vector2.Distance(Projectile.Center, npc.Center) <= explosionRadius)
                {
                    int dir = Projectile.Center.X < npc.Center.X ? 1 : -1;
                    int splashDmg = (int)(Projectile.damage * 0.5f);
                    npc.SimpleStrikeNPC(splashDmg, dir, false, Projectile.knockBack * 0.5f);
                    npc.GetGlobalNPC<ResonantTollNPC>().AddStacks(npc, 1);
                }
            }

            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            SymphonicBellfireParticleHandler.DrawAllParticles(sb);

            // Trail
            if (trailPositions.Count >= 2)
            {
                try
                {
                    trailRenderer ??= new SymphonicBellfirePrimitiveRenderer();
                    Color trailStart = RocketTier switch
                    {
                        2 => SymphonicBellfireUtils.CrescendoPalette[1],
                        1 => SymphonicBellfireUtils.RocketPalette[2],
                        _ => SymphonicBellfireUtils.RocketPalette[1]
                    };
                    float trailWidth = 10f + RocketTier * 5f;

                    var settings = new RocketTrailSettings
                    {
                        ColorStart = trailStart,
                        ColorEnd = SymphonicBellfireUtils.RocketPalette[0] * 0.3f,
                        Width = trailWidth,
                        BloomIntensity = 0.3f + RocketTier * 0.15f
                    };
                    trailRenderer.DrawTrail(sb, trailPositions, settings, Main.screenPosition);
                }
                catch { }
            }

            // Rocket sprite
            var tex = ModContent.Request<Texture2D>(Texture).Value;
            float glow = RocketTier switch { 2 => 0.6f, 1 => 0.35f, _ => 0.15f };
            Color drawColor = Color.Lerp(lightColor, Color.White, glow);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, drawColor,
                Projectile.rotation, tex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0f);

            // Glow overlay
            var bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow").Value;
            Color bloomCol = (RocketTier >= 2 ? SymphonicBellfireUtils.CrescendoPalette[1] : SymphonicBellfireUtils.RocketPalette[2]) * (glow * 0.4f);
            sb.Draw(bloomTex, Projectile.Center - Main.screenPosition, null,
                bloomCol, 0f, bloomTex.Size() / 2f, 0.2f + RocketTier * 0.08f, SpriteEffects.None, 0f);

            return false;
        }
    }
}
