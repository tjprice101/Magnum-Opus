using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.DiesIrae.Weapons.ChainOfJudgment.Utilities;
using MagnumOpus.Content.DiesIrae.Weapons.ChainOfJudgment.Particles;

namespace MagnumOpus.Content.DiesIrae.Weapons.ChainOfJudgment.Projectiles
{
    /// <summary>
    /// Judgment Chain Projectile — A blazing spectral chain that spins and ricochets between enemies.
    /// Bounces up to 4 times, each bounce causes an explosion. Returns to player after final bounce.
    ///
    /// VFX Layers:
    /// - Core: Spinning chain body with molten glow
    /// - Trail: Chain-link ghost afterimages trailing behind
    /// - Bounce: Bloom flash + spark shower + smoke burst + music notes
    /// - Return: Fading trail as it flies back
    ///
    /// ai[0] = bounces remaining (starts at 4)
    /// ai[1] = return state (0 = attacking, 1 = returning)
    /// </summary>
    public class JudgmentChainProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private static Asset<Texture2D> bloomTexture;
        private static Asset<Texture2D> glowTexture;

        // ─── Trail cache ───
        private const int TrailLength = 12;
        private Vector2[] trailCache = new Vector2[TrailLength];
        private float[] trailRotations = new float[TrailLength];

        private ref float BouncesRemaining => ref Projectile.ai[0];
        private ref float ReturnState => ref Projectile.ai[1];

        private const int MaxBounces = 4;
        private const float ReturnSpeed = 22f;
        private const float SearchRadius = 600f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            // Initialize bounces
            if (BouncesRemaining == 0 && ReturnState == 0 && Projectile.timeLeft == 300)
                BouncesRemaining = MaxBounces;

            UpdateTrailCache();

            // Spinning rotation
            Projectile.rotation += 0.3f;

            if (ReturnState == 1)
            {
                // Returning to player
                Player owner = Main.player[Projectile.owner];
                Vector2 toOwner = (owner.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toOwner * ReturnSpeed, 0.15f);

                if (Vector2.Distance(Projectile.Center, owner.Center) < 40f)
                    Projectile.Kill();

                SpawnReturnTrail();
            }
            else
            {
                // Attacking: seek next target
                float speed = Projectile.velocity.Length();
                if (speed < 14f)
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 16f;

                // Gentle homing to nearest enemy
                NPC target = FindNearestEnemy(SearchRadius);
                if (target != null)
                {
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 16f, 0.06f);
                }

                SpawnAttackTrail();
            }

            Lighting.AddLight(Projectile.Center, ChainUtils.HellfireChain.ToVector3() * 0.5f);
        }

        private void UpdateTrailCache()
        {
            for (int i = TrailLength - 1; i > 0; i--)
            {
                trailCache[i] = trailCache[i - 1];
                trailRotations[i] = trailRotations[i - 1];
            }
            trailCache[0] = Projectile.Center;
            trailRotations[0] = Projectile.rotation;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Bounce explosion VFX
            SpawnBounceVFX(target.Center);

            // Apply infernal debuffs
            target.AddBuff(BuffID.OnFire3, 300);

            BouncesRemaining--;

            if (BouncesRemaining <= 0)
            {
                // Enter return phase
                ReturnState = 1;
                Player owner = Main.player[Projectile.owner];
                Projectile.velocity = (owner.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * ReturnSpeed;
            }
            else
            {
                // Bounce: redirect to next enemy
                NPC nextTarget = FindNearestEnemyExcluding(SearchRadius, target.whoAmI);
                if (nextTarget != null)
                {
                    Vector2 toNext = (nextTarget.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    Projectile.velocity = toNext * 18f;
                }
                else
                {
                    // No target, reflect with random angle
                    Projectile.velocity = Projectile.velocity.RotatedByRandom(MathHelper.PiOver4) * 1.1f;
                }

                SoundEngine.PlaySound(SoundID.Item153 with { Pitch = 0.1f + (MaxBounces - BouncesRemaining) * 0.1f, Volume = 0.7f }, Projectile.Center);
            }
        }

        private void SpawnBounceVFX(Vector2 pos)
        {
            float bounceProgress = 1f - BouncesRemaining / (float)MaxBounces;

            // Bloom flash — gets bigger each bounce
            float bloomScale = 1.2f + bounceProgress * 1.5f;
            Color bloomColor = ChainUtils.GetChainColor(bounceProgress);
            ChainParticleHandler.Spawn(new ChainBloomParticle(pos, bloomColor, bloomScale, 18));
            ChainParticleHandler.Spawn(new ChainBloomParticle(pos, ChainUtils.AshWhite, bloomScale * 0.4f, 12));

            // Spark shower — 8-15 sparks
            int sparkCount = 8 + (int)(bounceProgress * 7);
            for (int i = 0; i < sparkCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f) + Projectile.velocity * 0.1f;
                Color c = ChainUtils.MulticolorLerp(Main.rand.NextFloat(),
                    ChainUtils.ChainCrimson, ChainUtils.MoltenLink, ChainUtils.HellfireChain);
                ChainParticleHandler.Spawn(new ChainSparkParticle(pos, vel, c, 0.2f + bounceProgress * 0.15f, 20));
            }

            // Smoke burst
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2.5f, 2.5f);
                ChainParticleHandler.Spawn(new ChainSmokeParticle(pos, vel, 0.5f + bounceProgress * 0.3f, 30));
            }

            // Chain link ghosts
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                ChainParticleHandler.Spawn(new ChainLinkGhostParticle(pos, vel, Main.rand.NextFloat(MathHelper.TwoPi),
                    ChainUtils.GetChainColor(Main.rand.NextFloat()), 0.4f, 25));
            }

            // Music notes — more on later bounces
            int noteCount = 2 + (int)(bounceProgress * 4);
            for (int i = 0; i < noteCount; i++)
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2.5f, -0.5f));
                Color c = ChainUtils.MulticolorLerp(Main.rand.NextFloat(),
                    ChainUtils.ChainCrimson, ChainUtils.HellfireChain, ChainUtils.WhiteHot);
                ChainParticleHandler.Spawn(new ChainNoteParticle(pos, vel, c, 0.4f + bounceProgress * 0.2f, 40));
            }
        }

        private void SpawnAttackTrail()
        {
            // Chain link ghost trail
            if (Main.rand.NextBool(2))
            {
                Vector2 off = Main.rand.NextVector2Circular(5f, 5f);
                ChainParticleHandler.Spawn(new ChainLinkGhostParticle(
                    Projectile.Center + off, -Projectile.velocity * 0.05f, Projectile.rotation,
                    ChainUtils.MoltenLink, 0.3f, 12));
            }

            // Ember sparks
            if (Main.rand.NextBool(3))
            {
                Vector2 vel = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f);
                ChainParticleHandler.Spawn(new ChainSparkParticle(Projectile.Center, vel,
                    ChainUtils.HellfireChain, 0.15f, 12));
            }
        }

        private void SpawnReturnTrail()
        {
            // Fading ember trail on return
            if (Main.rand.NextBool(2))
            {
                Vector2 vel = -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Color c = ChainUtils.MulticolorLerp(Main.rand.NextFloat(), ChainUtils.IronBlack, ChainUtils.ChainCrimson);
                ChainParticleHandler.Spawn(new ChainSparkParticle(Projectile.Center, vel, c, 0.1f, 10));
            }
        }

        private NPC FindNearestEnemy(float range)
        {
            NPC best = null;
            float closest = range * range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                float distSq = Vector2.DistanceSquared(Projectile.Center, npc.Center);
                if (distSq < closest) { closest = distSq; best = npc; }
            }
            return best;
        }

        private NPC FindNearestEnemyExcluding(float range, int excludeId)
        {
            NPC best = null;
            float closest = range * range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || i == excludeId) continue;
                float distSq = Vector2.DistanceSquared(Projectile.Center, npc.Center);
                if (distSq < closest) { closest = distSq; best = npc; }
            }
            return best;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawChainTrail();
            DrawChainBody(lightColor);
            DrawChainGlow();
            return false;
        }

        private void DrawChainTrail()
        {
            glowTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
            if (!glowTexture.IsLoaded) return;
            var tex = glowTexture.Value;

            float bounceProgress = 1f - BouncesRemaining / (float)MaxBounces;
            float heatMultiplier = 1f + bounceProgress * 1.5f; // Escalates with bounces

            for (int i = 1; i < TrailLength; i++)
            {
                if (trailCache[i] == Vector2.Zero) continue;
                float progress = i / (float)TrailLength;
                float alpha = (1f - progress) * 0.5f * heatMultiplier;
                Color c = ChainUtils.GetChainColor(progress * 0.7f);

                // Directional stretched afterimage — aligned to trail segment for chain-link feel
                float segRot = trailRotations[i];
                float baseScale = (1f - progress) * 0.4f;

                // Outer glow layer — elongated along rotation
                Main.EntitySpriteDraw(tex, trailCache[i] - Main.screenPosition, null, ChainUtils.Additive(c, alpha * 0.6f),
                    segRot, tex.Size() / 2f, new Vector2(baseScale * 2.5f, baseScale * 0.4f), SpriteEffects.None, 0);

                // Inner hot core — brighter, tighter, escalates with bounces
                Color hotCore = Color.Lerp(ChainUtils.MoltenLink, ChainUtils.WhiteHot, bounceProgress * 0.5f);
                Main.EntitySpriteDraw(tex, trailCache[i] - Main.screenPosition, null, ChainUtils.Additive(hotCore, alpha * 0.3f * heatMultiplier),
                    segRot, tex.Size() / 2f, new Vector2(baseScale * 1.5f, baseScale * 0.2f), SpriteEffects.None, 0);

                // Chain link connector glow between segments (every other point)
                if (i < TrailLength - 1 && trailCache[i + 1] != Vector2.Zero && i % 2 == 0)
                {
                    Vector2 mid = (trailCache[i] + trailCache[i + 1]) / 2f;
                    float linkRot = (trailCache[i] - trailCache[i + 1]).ToRotation() + MathHelper.PiOver2;
                    float linkAlpha = alpha * 0.2f;
                    Main.EntitySpriteDraw(tex, mid - Main.screenPosition, null, ChainUtils.Additive(ChainUtils.HellfireChain, linkAlpha),
                        linkRot, tex.Size() / 2f, new Vector2(baseScale * 0.8f, baseScale * 0.15f), SpriteEffects.None, 0);
                }
            }
        }

        private void DrawChainBody(Color lightColor)
        {
            var tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;
            Main.EntitySpriteDraw(tex, drawPos, null, lightColor, Projectile.rotation, origin, 1f, SpriteEffects.None, 0);
        }

        private void DrawChainGlow()
        {
            bloomTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            if (!bloomTexture.IsLoaded) return;
            var tex = bloomTexture.Value;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = 0.7f + 0.2f * (float)Math.Sin(Main.GameUpdateCount * 0.15f);
            float bounceProgress = 1f - BouncesRemaining / (float)MaxBounces;
            float heatMultiplier = 1f + bounceProgress * 0.8f;

            // Core glow — escalates with bounces
            Main.EntitySpriteDraw(tex, drawPos, null, ChainUtils.Additive(ChainUtils.MoltenLink, 0.3f * pulse * heatMultiplier),
                0f, tex.Size() / 2f, 0.8f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(tex, drawPos, null, ChainUtils.Additive(ChainUtils.WhiteHot, 0.15f * pulse * heatMultiplier),
                0f, tex.Size() / 2f, 0.4f, SpriteEffects.None, 0);

            // Bounce heat ring — appears after first bounce, pulsing
            if (bounceProgress > 0)
            {
                float ringPulse = 0.8f + 0.3f * (float)Math.Sin(Main.GameUpdateCount * 0.3f);
                Color ringColor = ChainUtils.Additive(ChainUtils.HellfireChain, 0.12f * bounceProgress * ringPulse);
                Main.EntitySpriteDraw(tex, drawPos, null, ringColor, 0f, tex.Size() / 2f, 1.2f + bounceProgress * 0.3f, SpriteEffects.None, 0);
            }

            // Spinning cross-flare on later bounces (signature chain judgment look)
            if (bounceProgress >= 0.5f)
            {
                glowTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
                if (glowTexture.IsLoaded)
                {
                    var pb = glowTexture.Value;
                    float crossAlpha = (bounceProgress - 0.5f) * 0.4f;
                    Color crossColor = ChainUtils.Additive(ChainUtils.WhiteHot, crossAlpha);
                    Main.EntitySpriteDraw(pb, drawPos, null, crossColor, Projectile.rotation, pb.Size() / 2f,
                        new Vector2(0.1f, 0.6f), SpriteEffects.None, 0);
                    Main.EntitySpriteDraw(pb, drawPos, null, crossColor, Projectile.rotation + MathHelper.PiOver2, pb.Size() / 2f,
                        new Vector2(0.1f, 0.6f), SpriteEffects.None, 0);
                }
            }
        }
    }
}
