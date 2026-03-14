using System;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.Nachtmusik.Weapons.NocturnalExecutioner.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Nachtmusik.Weapons.NocturnalExecutioner.Projectiles
{
    /// <summary>
    /// Homing spectral blade spawned during normal combo swings.
    /// Void-edged, trails cosmic purple dust, homes toward enemies within range.
    /// </summary>
    public class NocturnalBladeProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        private const float HomingRange = 400f;
        private const float HomingStrength = 0.04f;
        private const int Lifetime = 90;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.tileCollide = false;
            Projectile.penetrate = 2;
            Projectile.timeLeft = Lifetime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            // Gentle homing toward nearest enemy
            float timer = Lifetime - Projectile.timeLeft;
            if (timer > 10f) // Grace period before homing
            {
                int target = FindNearestEnemy(HomingRange);
                if (target >= 0)
                {
                    Vector2 toTarget = (Main.npc[target].Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity.SafeNormalize(Vector2.UnitX),
                        toTarget, HomingStrength).SafeNormalize(Vector2.UnitX) * Projectile.velocity.Length();
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Void trail dust
            if (!Main.dedServ && Main.rand.NextBool(2))
            {
                Color c = Color.Lerp(NachtmusikPalette.CosmicVoid, NachtmusikPalette.CosmicPurple,
                    Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch,
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    0, c, 0.7f);
                d.noGravity = true;
                d.fadeIn = 1f;
            }

            // Sparse gold accent
            if (!Main.dedServ && Main.rand.NextBool(6))
            {
                Dust g = Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame,
                    Main.rand.NextVector2Circular(1f, 1f), 0, NachtmusikPalette.StarGold, 0.4f);
                g.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, NachtmusikPalette.CosmicPurple.ToVector3() * 0.25f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Mini void implosion on hit
            int dustCount = 6;
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                Vector2 spawnPos = target.Center + angle.ToRotationVector2() * 20f;
                Vector2 vel = (target.Center - spawnPos).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(3f, 5f);
                Dust d = Dust.NewDustPerfect(spawnPos, DustID.PurpleTorch, vel, 0,
                    NachtmusikPalette.CosmicPurple, 0.8f);
                d.noGravity = true;
            }

            // Gold burst
            for (int i = 0; i < 3; i++)
            {
                Dust g = Dust.NewDustPerfect(target.Center, DustID.GoldFlame,
                    Main.rand.NextVector2Circular(3f, 3f), 0, NachtmusikPalette.StarGold, 0.6f);
                g.noGravity = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;
            for (int i = 0; i < 5; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch,
                    Main.rand.NextVector2Circular(3f, 3f), 0, NachtmusikPalette.Violet, 0.6f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
            if (glowTex == null) return false;

            Vector2 origin = glowTex.Size() * 0.5f;
            float fadeIn = Utils.GetLerpValue(Lifetime, Lifetime - 8, Projectile.timeLeft, true);
            float fadeOut = Utils.GetLerpValue(0, 10, Projectile.timeLeft, true);
            float alpha = fadeIn * fadeOut;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Trail afterimages
            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                float trailAlpha = alpha * (1f - i / (float)Projectile.oldPos.Length) * 0.4f;
                Color trailColor = NachtmusikPalette.Additive(NachtmusikPalette.CosmicPurple, trailAlpha);
                float trailScale = (1f - i / (float)Projectile.oldPos.Length) * 0.15f;
                sb.Draw(glowTex, trailPos, null, trailColor, Projectile.oldRot[i],
                    origin, new Vector2(trailScale * 2f, trailScale * 0.5f), SpriteEffects.None, 0f);
            }

            // Core void blade
            Vector2 pos = Projectile.Center - Main.screenPosition;
            float rot = Projectile.rotation;

            // Outer void glow
            Color outerColor = NachtmusikPalette.Additive(NachtmusikPalette.CosmicPurple, 0.4f * alpha);
            sb.Draw(glowTex, pos, null, outerColor, rot, origin,
                new Vector2(0.25f, 0.08f), SpriteEffects.None, 0f);

            // Inner violet
            Color innerColor = NachtmusikPalette.Additive(NachtmusikPalette.Violet, 0.6f * alpha);
            sb.Draw(glowTex, pos, null, innerColor, rot, origin,
                new Vector2(0.15f, 0.04f), SpriteEffects.None, 0f);

            // Gold edge
            Color edgeColor = NachtmusikPalette.Additive(NachtmusikPalette.StarGold, 0.3f * alpha);
            sb.Draw(glowTex, pos, null, edgeColor, rot, origin,
                new Vector2(0.18f, 0.02f), SpriteEffects.None, 0f);

            // White-hot core
            Color coreColor = NachtmusikPalette.Additive(NachtmusikPalette.TwinklingWhite, 0.5f * alpha);
            sb.Draw(glowTex, pos, null, coreColor, rot, origin,
                new Vector2(0.08f, 0.015f), SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        private int FindNearestEnemy(float range)
        {
            int closest = -1;
            float closestDist = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.dontTakeDamage)
                {
                    float dist = Projectile.Distance(npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = i;
                    }
                }
            }
            return closest;
        }
    }

    /// <summary>
    /// Void Guillotine — flat, dark rectangular blade defined by ABSENCE.
    /// Fired in 5-blade fan from Execution Fan right-click.
    /// ai[0]: 1 = max charge (homes briefly, larger impact).
    /// </summary>
    public class ExecutionFanBlade : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        private bool IsMaxCharge => Projectile.ai[0] >= 1f;
        private const int Lifetime = 120;
        private const float HomingRange = 500f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.tileCollide = false;
            Projectile.penetrate = 3;
            Projectile.timeLeft = Lifetime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            float timer = Lifetime - Projectile.timeLeft;

            // Max charge: brief homing after 20 ticks
            if (IsMaxCharge && timer > 20f && timer < 60f)
            {
                int target = FindNearestEnemy(HomingRange);
                if (target >= 0)
                {
                    Vector2 toTarget = (Main.npc[target].Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity.SafeNormalize(Vector2.UnitX),
                        toTarget, 0.06f).SafeNormalize(Vector2.UnitX) * Projectile.velocity.Length();
                }
            }

            // Spin rotation (void guillotine spinning)
            Projectile.rotation += 0.25f;

            // Dense void contrail
            if (!Main.dedServ)
            {
                for (int i = 0; i < 2; i++)
                {
                    Color c = Color.Lerp(NachtmusikPalette.CosmicVoid, NachtmusikPalette.CosmicPurple,
                        Main.rand.NextFloat(0.2f, 0.7f));
                    Dust d = Dust.NewDustPerfect(
                        Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                        DustID.PurpleTorch,
                        -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f),
                        0, c, 0.9f + (IsMaxCharge ? 0.3f : 0f));
                    d.noGravity = true;
                    d.fadeIn = 1.1f;
                }

                // Gold fire accents for max charge
                if (IsMaxCharge && Main.rand.NextBool(3))
                {
                    Dust g = Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame,
                        Main.rand.NextVector2Circular(2f, 2f), 0, NachtmusikPalette.StarGold, 0.6f);
                    g.noGravity = true;
                }
            }

            Lighting.AddLight(Projectile.Center,
                NachtmusikPalette.CosmicPurple.ToVector3() * (0.3f + (IsMaxCharge ? 0.15f : 0f)));
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Void implosion on hit — light collapses INWARD then detonates in gold
            float intensity = IsMaxCharge ? 1.4f : 1f;
            int implosionCount = (int)(8 * intensity);

            for (int i = 0; i < implosionCount; i++)
            {
                float angle = MathHelper.TwoPi * i / implosionCount;
                float dist = 25f * intensity;
                Vector2 spawnPos = target.Center + angle.ToRotationVector2() * dist;
                Vector2 vel = (target.Center - spawnPos).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(5f, 9f);

                Dust d = Dust.NewDustPerfect(spawnPos, DustID.PurpleTorch, vel, 0,
                    NachtmusikPalette.CosmicPurple, 1.0f * intensity);
                d.noGravity = true;
            }

            // Gold detonation
            int goldCount = (int)(5 * intensity);
            for (int i = 0; i < goldCount; i++)
            {
                Dust g = Dust.NewDustPerfect(target.Center, DustID.GoldFlame,
                    Main.rand.NextVector2Circular(5f, 5f) * intensity, 0,
                    Color.Lerp(NachtmusikPalette.StarGold, NachtmusikPalette.TwinklingWhite, Main.rand.NextFloat(0.3f, 1f)),
                    0.9f * intensity);
                g.noGravity = true;
            }

            // Bloom ring
            var ring = new BloomRingParticle(target.Center, Vector2.Zero,
                NachtmusikPalette.StarGold, 0.5f * intensity, 15);
            MagnumParticleHandler.SpawnParticle(ring);

            if (IsMaxCharge)
            {
                var innerRing = new BloomRingParticle(target.Center, Vector2.Zero,
                    NachtmusikPalette.CosmicPurple, 0.4f, 20);
                MagnumParticleHandler.SpawnParticle(innerRing);
            }

            // Build charge on kill (cosmetic dust burst)
            if (target.life <= 0)
            {
                for (int j = 0; j < 4; j++)
                {
                    Dust d = Dust.NewDustPerfect(target.Center, DustID.BlueTorch,
                        Main.rand.NextVector2Circular(4f, 4f), 0, default, 0.8f);
                    d.noGravity = true;
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;

            // Death implosion burst
            int count = IsMaxCharge ? 12 : 8;
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Color c = i % 2 == 0 ? NachtmusikPalette.CosmicPurple : NachtmusikPalette.Violet;
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch, vel, 0, c, 0.7f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
            if (glowTex == null) return false;

            Vector2 origin = glowTex.Size() * 0.5f;
            float fadeIn = Utils.GetLerpValue(Lifetime, Lifetime - 6, Projectile.timeLeft, true);
            float fadeOut = Utils.GetLerpValue(0, 12, Projectile.timeLeft, true);
            float alpha = fadeIn * fadeOut;
            float sizeScale = IsMaxCharge ? 1.3f : 1f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Trail afterimages — dark void streaks
            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                float trailAlpha = alpha * (1f - i / (float)Projectile.oldPos.Length) * 0.35f;
                Color trailColor = NachtmusikPalette.Additive(NachtmusikPalette.CosmicPurple, trailAlpha);
                float trailScale = (1f - i / (float)Projectile.oldPos.Length) * 0.12f * sizeScale;
                sb.Draw(glowTex, trailPos, null, trailColor, Projectile.oldRot[i],
                    origin, new Vector2(trailScale * 1.8f, trailScale * 1.2f), SpriteEffects.None, 0f);
            }

            Vector2 pos = Projectile.Center - Main.screenPosition;

            // Outer void rectangle shape (elongated for guillotine look)
            Color outerVoid = NachtmusikPalette.Additive(NachtmusikPalette.CosmicVoid, 0.5f * alpha);
            sb.Draw(glowTex, pos, null, outerVoid, Projectile.rotation, origin,
                new Vector2(0.20f, 0.14f) * sizeScale, SpriteEffects.None, 0f);

            // Purple edge glow
            Color purpleEdge = NachtmusikPalette.Additive(NachtmusikPalette.CosmicPurple, 0.4f * alpha);
            sb.Draw(glowTex, pos, null, purpleEdge, Projectile.rotation, origin,
                new Vector2(0.16f, 0.10f) * sizeScale, SpriteEffects.None, 0f);

            // Violet inner
            Color violetInner = NachtmusikPalette.Additive(NachtmusikPalette.Violet, 0.35f * alpha);
            sb.Draw(glowTex, pos, null, violetInner, Projectile.rotation + 0.1f, origin,
                new Vector2(0.12f, 0.07f) * sizeScale, SpriteEffects.None, 0f);

            // Gold edge (max charge gets brighter)
            if (IsMaxCharge)
            {
                float goldPulse = 1f + MathF.Sin((float)Main.timeForVisualEffects * 6f) * 0.15f;
                Color goldEdge = NachtmusikPalette.Additive(NachtmusikPalette.StarGold, 0.3f * alpha * goldPulse);
                sb.Draw(glowTex, pos, null, goldEdge, Projectile.rotation, origin,
                    new Vector2(0.22f, 0.03f) * sizeScale, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        private int FindNearestEnemy(float range)
        {
            int closest = -1;
            float closestDist = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.dontTakeDamage)
                {
                    float dist = Projectile.Distance(npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = i;
                    }
                }
            }
            return closest;
        }
    }
}
