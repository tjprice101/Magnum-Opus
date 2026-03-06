using System;
using MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Buffs;
using MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Utilities;
using MagnumOpus.Common.Systems.VFX.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Projectiles
{
    /// <summary>
    /// Verdict Bolt ? Homing judgment bolt fired 3 per swing.
    /// Tracks nearest enemy, leaves a crimson-gold trail, and spawns
    /// a SpectralVerdictSlash on impact.
    ///
    /// VFX: 3-layer additive fire orb + trailing ember dust + impact flash.
    /// </summary>
    public class VerdictBolt : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const float HomingRange = 600f;
        private const float HomingStrength = 0.08f;

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 180;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Homing toward nearest enemy
            NPC target = FindClosestNPC();
            if (target != null)
            {
                Vector2 desired = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * Projectile.velocity.Length();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, HomingStrength);
            }

            // Trail dust ? crimson + gold
            if (Main.rand.NextBool(2))
            {
                Vector2 dustVel = -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 2f)
                    + Main.rand.NextVector2Circular(0.8f, 0.8f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, dustVel, 0,
                    ExecutionersVerdictUtils.GetPaletteColor(Main.rand.NextFloat(0.3f, 0.7f)),
                    Main.rand.NextFloat(0.8f, 1.3f));
                d.noGravity = true;
                d.fadeIn = 1.0f;
            }

            // Gold accent spark
            if (Main.rand.NextBool(4))
            {
                Dust g = Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame,
                    Main.rand.NextVector2Circular(1f, 1f), 0,
                    ExecutionersVerdictUtils.JudgmentGold, 0.5f);
                g.noGravity = true;
            }

            // Dynamic lighting
            Lighting.AddLight(Projectile.Center, 0.7f, 0.2f, 0.05f);
        }

        private NPC FindClosestNPC()
        {
            NPC closest = null;
            float closestDist = HomingRange;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage)
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }
            return closest;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ExecutionBrand>(), 180);
            target.AddBuff(ModContent.BuffType<PyreImmolation>(), 120);

            // Spawn spectral verdict slash at impact
            if (Projectile.owner == Main.myPlayer)
            {
                float slashAngle = Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(),
                    target.Center, Vector2.Zero,
                    ModContent.ProjectileType<SpectralVerdictSlash>(),
                    (int)(Projectile.damage * 0.5f), 0f,
                    Projectile.owner, ai0: slashAngle);
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Death burst ? crimson/gold sparks
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, vel, 0,
                    ExecutionersVerdictUtils.GetPaletteColor(Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.8f, 1.2f));
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow == null) return false;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = glow.Size() * 0.5f;

            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.25f + Projectile.whoAmI * 0.7f) * 0.12f;
            float fadeMult = Projectile.timeLeft < 20 ? Projectile.timeLeft / 20f : 1f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);

            // Outer crimson halo
            Color outerColor = ExecutionersVerdictUtils.BloodCrimson * (0.3f * fadeMult);
            outerColor.A = 0;
            sb.Draw(glow, drawPos, null, outerColor, 0f, origin, 0.06f * pulse, SpriteEffects.None, 0f);

            // Mid gold body
            Color midColor = ExecutionersVerdictUtils.JudgmentGold * (0.5f * fadeMult);
            midColor.A = 0;
            sb.Draw(glow, drawPos, null, midColor, 0f, origin, 0.035f * pulse, SpriteEffects.None, 0f);

            // Hot white core
            Color coreColor = ExecutionersVerdictUtils.HellfireWhite * (0.65f * fadeMult);
            coreColor.A = 0;
            sb.Draw(glow, drawPos, null, coreColor, 0f, origin, 0.015f, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Spectral Verdict Slash ? Brief spectral sword arc spawned on bolt impact.
    /// Fades quickly, deals AoE damage in a small area.
    /// ai[0] = slash angle offset.
    /// </summary>
    public class SpectralVerdictSlash : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 25;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            // Stationary fading slash
            Projectile.velocity = Vector2.Zero;

            // Crimson slash dust line
            if (Projectile.timeLeft > 15)
            {
                float slashAngle = Projectile.ai[0];
                Vector2 dir = slashAngle.ToRotationVector2();
                for (int i = 0; i < 2; i++)
                {
                    float offset = Main.rand.NextFloat(-30f, 30f);
                    Vector2 pos = Projectile.Center + dir * offset;
                    Vector2 vel = dir.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-1f, 1f);
                    Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0,
                        ExecutionersVerdictUtils.GetPaletteColor(Main.rand.NextFloat(0.3f, 0.8f)), 1.0f);
                    d.noGravity = true;
                }
            }

            Lighting.AddLight(Projectile.Center, 0.5f, 0.15f, 0.05f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ExecutionBrand>(), 120);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow == null) return false;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = glow.Size() * 0.5f;

            float lifeFrac = Projectile.timeLeft / 25f;
            float alpha = lifeFrac > 0.6f ? 1f : lifeFrac / 0.6f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);

            // Slash arc glow ? stretched along slash angle
            float slashAngle = Projectile.ai[0];
            Color slashColor = ExecutionersVerdictUtils.CrimsonRed * (0.6f * alpha);
            slashColor.A = 0;
            sb.Draw(glow, drawPos, null, slashColor, slashAngle, origin,
                new Vector2(0.12f, 0.02f), SpriteEffects.None, 0f);

            // Gold core line
            Color goldColor = ExecutionersVerdictUtils.JudgmentGold * (0.4f * alpha);
            goldColor.A = 0;
            sb.Draw(glow, drawPos, null, goldColor, slashAngle, origin,
                new Vector2(0.08f, 0.01f), SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}