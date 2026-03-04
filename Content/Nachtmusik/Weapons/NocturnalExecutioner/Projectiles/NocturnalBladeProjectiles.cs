using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Debuffs;
using MagnumOpus.Content.Nachtmusik.Weapons.NocturnalExecutioner.Utilities;
using static MagnumOpus.Common.Systems.Particles.Particle;

namespace MagnumOpus.Content.Nachtmusik.Weapons.NocturnalExecutioner.Projectiles
{
    /// <summary>
    /// Spectral blade projectile fired from Nocturnal Executioner's combo phases.
    /// Ghostly indigo blade that homes toward enemies with cosmic nebula trail.
    /// </summary>
    public class NocturnalBladeProjectile : ModProjectile
    {
        private static readonly Color DeepIndigo = new Color(40, 30, 100);
        private static readonly Color CosmicBlue = new Color(60, 80, 180);
        private static readonly Color StarlightSilver = new Color(180, 200, 230);
        private static readonly Color StellarWhite = new Color(240, 245, 255);

        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/QuarterNote";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            float time = (float)Main.timeForVisualEffects * 0.04f;

            // Gentle homing toward nearest enemy
            NPC target = FindClosestTarget(400f);
            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), 0.04f);
            }

            // Cosmic nebula dust trail — deep indigo to starlight
            if (Main.rand.NextBool(2))
            {
                float hue = 0.62f + (float)Math.Sin(time * 2f + Projectile.whoAmI * 0.5f) * 0.06f;
                Color trailColor = Main.hslToRgb(hue, 0.7f, 0.55f);
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.PurpleTorch, -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1f, 1f),
                    0, trailColor, 1.0f);
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }

            // Starlight silver sparkle accents
            if (Main.rand.NextBool(4))
            {
                Dust s = Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame,
                    Main.rand.NextVector2Circular(2f, 2f), 0, StarlightSilver, 0.6f);
                s.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, CosmicBlue.ToVector3() * 0.4f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 300);
            if (target.TryGetGlobalNPC(out CelestialHarmonyNPC harmonyNPC))
                harmonyNPC.AddStack(target, 1);

            // Cosmic impact flash
            CustomParticles.GenericFlare(target.Center, StarlightSilver, 0.5f, 12);
            CustomParticles.HaloRing(target.Center, CosmicBlue, 0.3f, 10);

            // Scattered starlight
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Dust d = Dust.NewDustPerfect(target.Center, DustID.PurpleTorch, vel, 0, 
                    Color.Lerp(DeepIndigo, StarlightSilver, Main.rand.NextFloat()), 1.1f);
                d.noGravity = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Dissipation burst
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch, vel, 0,
                    Color.Lerp(DeepIndigo, CosmicBlue, Main.rand.NextFloat()), 0.8f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Custom afterimage trail
            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = 1f - (float)i / Projectile.oldPos.Length;
                Color trailColor = Color.Lerp(DeepIndigo, CosmicBlue, progress) * progress * 0.5f;
                trailColor.A = 0;

                Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Main.EntitySpriteDraw(tex, drawPos, null, trailColor, Projectile.oldRot[i],
                    tex.Size() / 2f, Projectile.scale * progress, SpriteEffects.None, 0);
            }

            // Core glow
            {
                Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
                Vector2 pos = Projectile.Center - Main.screenPosition;
                Vector2 origin = tex.Size() / 2f;

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Main.EntitySpriteDraw(tex, pos, null, CosmicBlue with { A = 0 } * 0.6f,
                    Projectile.rotation, origin, Projectile.scale * 1.3f, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(tex, pos, null, StarlightSilver with { A = 0 } * 0.3f,
                    Projectile.rotation, origin, Projectile.scale * 1.1f, SpriteEffects.None, 0);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }

            // Nachtmusik theme star flare accent
            NachtmusikShaderManager.BeginAdditive(Main.spriteBatch);
            NachtmusikVFXLibrary.DrawThemeStarFlare(Main.spriteBatch, Projectile.Center, 1f, 0.5f);
            NachtmusikShaderManager.RestoreSpriteBatch(Main.spriteBatch);

            return true;
        }

        private NPC FindClosestTarget(float maxDistance)
        {
            NPC closest = null;
            float closestDist = maxDistance;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                {
                    float dist = npc.Distance(Projectile.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }
            return closest;
        }
    }

    /// <summary>
    /// Execution Fan Blade — fired in a 5-blade fan by the Nocturnal Executioner's alt-fire.
    /// At 100 charge, briefly homes toward enemies before convergence explosion.
    /// ai[0] = 1 for max-charge variant with homing.
    /// </summary>
    public class ExecutionFanBlade : ModProjectile
    {
        private static readonly Color DeepIndigo = new Color(40, 30, 100);
        private static readonly Color CosmicBlue = new Color(60, 80, 180);
        private static readonly Color StarlightSilver = new Color(180, 200, 230);
        private static readonly Color StellarWhite = new Color(240, 245, 255);

        private bool isMaxCharge => Projectile.ai[0] == 1f;

        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/QuarterNote";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            float time = (float)Main.timeForVisualEffects * 0.04f;

            // Max charge variant homes briefly after 20 ticks
            if (isMaxCharge && Projectile.timeLeft < 100 && Projectile.timeLeft > 60)
            {
                NPC target = FindClosestTarget(600f);
                if (target != null)
                {
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), 0.06f);
                }
            }

            // Dense cosmic trail — silver ghost blade look
            for (int i = 0; i < 2; i++)
            {
                Color trailColor = Color.Lerp(DeepIndigo, StellarWhite, Main.rand.NextFloat() * 0.7f);
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.PurpleTorch, -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1.5f, 1.5f),
                    0, trailColor, isMaxCharge ? 1.3f : 1.0f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // Starlight edge sparkles
            if (Main.rand.NextBool(3))
            {
                Dust s = Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame,
                    Main.rand.NextVector2Circular(3f, 3f), 0, StarlightSilver, 0.7f);
                s.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, StarlightSilver.ToVector3() * (isMaxCharge ? 0.5f : 0.35f));
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 480);
            if (target.TryGetGlobalNPC(out CelestialHarmonyNPC harmonyNPC))
                harmonyNPC.AddStack(target, 2);

            // Heavy cosmic impact
            CustomParticles.GenericFlare(target.Center, StellarWhite, 0.7f, 16);
            CustomParticles.HaloRing(target.Center, CosmicBlue, 0.4f, 14);
            NachtmusikVFXLibrary.SpawnTwinklingStars(target.Center, 3, 20f);

            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Dust d = Dust.NewDustPerfect(target.Center, DustID.PurpleTorch, vel, 0,
                    Color.Lerp(DeepIndigo, StarlightSilver, Main.rand.NextFloat()), 1.3f);
                d.noGravity = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Stellar fragment dissipation
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color c = Color.Lerp(DeepIndigo, StarlightSilver, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch, vel, 0, c, 1.0f);
                d.noGravity = true;
            }

            if (isMaxCharge)
            {
                NachtmusikVFXLibrary.SpawnStarBurst(Projectile.Center, 2, 0.8f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = tex.Size() / 2f;

            // Afterimage trail
            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = 1f - (float)i / Projectile.oldPos.Length;
                Color trailColor = Color.Lerp(DeepIndigo, StarlightSilver, progress) * progress * 0.4f;
                trailColor.A = 0;

                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Main.EntitySpriteDraw(tex, drawPos, null, trailColor, Projectile.oldRot[i],
                    origin, Projectile.scale * (0.7f + progress * 0.3f), SpriteEffects.None, 0);
            }

            // Core additive glow
            Vector2 pos = Projectile.Center - Main.screenPosition;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.1f;
            Main.EntitySpriteDraw(tex, pos, null, StarlightSilver with { A = 0 } * 0.5f,
                Projectile.rotation, origin, Projectile.scale * pulse * 1.4f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(tex, pos, null, CosmicBlue with { A = 0 } * 0.3f,
                Projectile.rotation, origin, Projectile.scale * pulse * 1.15f, SpriteEffects.None, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Nachtmusik theme star flare accent
            NachtmusikShaderManager.BeginAdditive(Main.spriteBatch);
            NachtmusikVFXLibrary.DrawThemeStarFlare(Main.spriteBatch, Projectile.Center, 1f, 0.5f);
            NachtmusikShaderManager.RestoreSpriteBatch(Main.spriteBatch);

            return true;
        }

        private NPC FindClosestTarget(float maxDistance)
        {
            NPC closest = null;
            float closestDist = maxDistance;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                {
                    float dist = npc.Distance(Projectile.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }
            return closest;
        }
    }
}
