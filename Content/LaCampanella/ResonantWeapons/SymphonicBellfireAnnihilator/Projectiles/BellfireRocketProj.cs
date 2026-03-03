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
    /// Bellfire Rocket — alt-fire rapid arcing rockets that leave fire patches on impact.
    /// Simplified from tier system. Arcs slightly downward due to gravity.
    /// On kill, registers rocket kill for Bellfire Crescendo buff stacking.
    /// </summary>
    public class BellfireRocketProj : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RocketI;

        private List<Vector2> trailPositions = new List<Vector2>();
        private const int MaxTrailPoints = 16;
        private SymphonicBellfirePrimitiveRenderer trailRenderer;

        private const float FirePatchDuration = 90; // 1.5 seconds
        private const float ExplosionRadius = 80f;

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
            // Record trail
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > MaxTrailPoints)
                trailPositions.RemoveAt(trailPositions.Count - 1);

            // Slight arc (gravity)
            Projectile.velocity.Y += 0.12f;

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Exhaust trail particles
            if (Main.rand.NextBool(2))
            {
                SymphonicBellfireParticleHandler.SpawnParticle(new RocketExhaustParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(4, 4),
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f),
                    2.5f, Main.rand.Next(12, 22)));
            }

            // Lighting
            Lighting.AddLight(Projectile.Center, SymphonicBellfireUtils.RocketPalette[2].ToVector3() * 0.5f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);

            // Register rocket kill for buff stacking
            if (target.life <= 0)
            {
                Player owner = Main.player[Projectile.owner];
                var modPlayer = owner.GetModPlayer<SymphonicBellfirePlayer>();
                modPlayer.RegisterRocketKill();
            }
        }

        public override void OnKill(int timeLeft)
        {
            trailRenderer?.Dispose();
            trailRenderer = null;

            // Explosion VFX
            SymphonicBellfireParticleHandler.SpawnParticle(new ExplosionFireballParticle(
                Projectile.Center, 2f, 15));

            // Shrapnel embers
            for (int i = 0; i < 8; i++)
            {
                SymphonicBellfireParticleHandler.SpawnParticle(new RocketExhaustParticle(
                    Projectile.Center,
                    Main.rand.NextVector2Circular(5f, 5f),
                    Main.rand.NextFloat(2f, 3.5f),
                    Main.rand.Next(12, 25)));
            }

            // AoE splash damage
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy()) continue;
                if (Vector2.Distance(Projectile.Center, npc.Center) <= ExplosionRadius)
                {
                    int dir = Projectile.Center.X < npc.Center.X ? 1 : -1;
                    int splashDmg = (int)(Projectile.damage * 0.4f);
                    npc.SimpleStrikeNPC(splashDmg, dir, false, Projectile.knockBack * 0.4f);
                    npc.GetGlobalNPC<ResonantTollNPC>().AddStacks(npc, 1);
                }
            }

            // Fire patch — lingering damage zone (vanilla fire dust)
            SpawnFirePatch();

            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.2f, Volume = 0.7f }, Projectile.Center);
        }

        private void SpawnFirePatch()
        {
            // Create lingering fire visual (vanilla dust-based, 1.5s)
            Vector2 patchCenter = Projectile.Center;
            int patchDustCount = 12;
            for (int i = 0; i < patchDustCount; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(30f, 10f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -Main.rand.NextFloat(0.5f, 1.5f));
                Dust d = Dust.NewDustPerfect(patchCenter + offset, DustID.Torch, vel, 0,
                    SymphonicBellfireUtils.GetRocketFlicker(Main.rand.NextFloat()),
                    Main.rand.NextFloat(1f, 1.8f));
                d.noGravity = true;
                d.fadeIn = 1.5f;
            }

            // Smoke puffs
            for (int i = 0; i < 4; i++)
            {
                Vector2 smokeVel = new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(1f, 3f));
                Dust d = Dust.NewDustPerfect(patchCenter + Main.rand.NextVector2Circular(20f, 5f),
                    DustID.Smoke, smokeVel, 100, new Color(30, 20, 15), 2f);
                d.noGravity = true;
            }
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
                    var settings = new RocketTrailSettings
                    {
                        ColorStart = SymphonicBellfireUtils.RocketPalette[2],
                        ColorEnd = SymphonicBellfireUtils.RocketPalette[0] * 0.3f,
                        Width = 10f,
                        BloomIntensity = 0.3f
                    };
                    trailRenderer.DrawTrail(sb, trailPositions, settings, Main.screenPosition);
                }
                catch { }
            }

            // Rocket sprite
            Texture2D tex = null;
            try { tex = ModContent.Request<Texture2D>(Texture, ReLogic.Content.AssetRequestMode.ImmediateLoad)?.Value; }
            catch { }
            if (tex != null)
            {
                Color drawColor = Color.Lerp(lightColor, Color.White, 0.2f);
                sb.Draw(tex, Projectile.Center - Main.screenPosition, null, drawColor,
                    Projectile.rotation, tex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0f);
            }

            // Bloom orb
            Texture2D bloomTex = null;
            try
            {
                bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad)?.Value;
            }
            catch { }
            if (bloomTex != null)
            {
                Color bloomCol = SymphonicBellfireUtils.RocketPalette[2] * 0.15f;
                sb.Draw(bloomTex, Projectile.Center - Main.screenPosition, null,
                    bloomCol, 0f, bloomTex.Size() / 2f, 0.2f, SpriteEffects.None, 0f);
            }

            return false;
        }
    }
}
