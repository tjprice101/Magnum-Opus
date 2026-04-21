using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.ClairDeLune;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.GearDrivenArbiter.Projectiles
{
    /// <summary>
    /// Gear-Driven Arbiter Minion — Verdict Stacking system.
    /// Minion fires homing orbs. Each hit same enemy = 1 Verdict stack (max 8).
    /// At thresholds: 2 stacks (0.06→0.10 homing), 4 stacks (+pierce 1), 6 stacks (+50% damage), 8 stacks (2x scale, 5x damage).
    /// </summary>
    public class ArbiterMinionProjectile : ModProjectile
    {
        private float hoverAngle;
        private int fireTimer = 0;
        private float pulseTimer = 0f;
        private int _lastKnownStacks = 0;
        private VertexStrip _strip;

        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        public override void SetStaticDefaults()
        {
            Main.projPet[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!CheckActive(owner))
                return;

            hoverAngle += 0.03f;
            fireTimer++;
            pulseTimer += 0.05f;

            NPC target = FindTarget(owner, 700f);

            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 14f, 0.08f);
            }
            else
            {
                float hoverOffset = (float)Math.Sin(hoverAngle) * 30f;
                Vector2 idealPos = owner.Center + new Vector2(owner.direction * -60f, -50f + hoverOffset);
                Vector2 toIdeal = idealPos - Projectile.Center;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdeal * 0.1f, 0.08f);
            }

            Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;

            // Fire orb every 50 frames
            if (fireTimer % 50 == 0 && target != null && Main.myPlayer == owner.whoAmI)
            {
                var globalNPC = target.GetGlobalNPC<ClairDeLuneGlobalNPC>();
                int stacks = globalNPC.ArbiterVerdictStacks;
                _lastKnownStacks = stacks;

                // Determine homing and flags based on verdict stacks
                float homingStrength = 0.04f;
                int flags = 0;
                float damageMultiplier = 1f;
                float scaleMultiplier = 1f;

                if (stacks >= 2)
                    homingStrength = 0.10f;
                if (stacks >= 4)
                    flags |= GenericHomingOrbChild.FLAG_PIERCE;
                if (stacks >= 6)
                    damageMultiplier = 1.5f;
                if (stacks >= 8)
                {
                    damageMultiplier = 5f;
                    scaleMultiplier = 2f;
                }

                Vector2 orbVel = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 10f;

                GenericHomingOrbChild.SpawnChild(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center, orbVel,
                    (int)(Projectile.damage * damageMultiplier), Projectile.knockBack, Projectile.owner,
                    homingStrength: homingStrength,
                    behaviorFlags: flags,
                    themeIndex: GenericHomingOrbChild.THEME_CLAIRDELUNE,
                    scaleMult: scaleMultiplier * 0.9f, timeLeft: 90);

                // Increment stack
                globalNPC.ArbiterVerdictStacks = Math.Min(8, stacks + 1);
                globalNPC.VerdictStackTimer = 180; // Reset decay timer
            }

            // Dust trail
            if (Main.rand.NextBool(4))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch,
                    -Projectile.velocity * 0.1f, 0, ClairDeLunePalette.ClockworkBrass, 0.6f);
                d.noGravity = true;
            }

            ClairDeLuneVFXLibrary.AddClairDeLuneLight(Projectile.Center, 0.6f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // IncisorOrb shader trail (shows chase path) + 5-layer palette-cycling bloom head
            IncisorOrbRenderer.DrawOrbVisuals(Main.spriteBatch, Projectile, IncisorOrbRenderer.ClairDeLune, ref _strip);

            // Verdict stack ring: one dot per stack (0–8), color by tier
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                float pulse = 0.82f + 0.18f * MathF.Sin(pulseTimer);
                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                float t = (float)Main.timeForVisualEffects;

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);

                Texture2D bloom = ModContent.Request<Texture2D>(
                    "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                Vector2 origin = bloom.Size() / 2f;

                for (int s = 0; s < _lastKnownStacks; s++)
                {
                    float angle = t * 0.040f * 60f + s * MathHelper.TwoPi / 8f;
                    Color dotColor = s < 2 ? ClairDeLunePalette.ClockworkBrass
                                   : s < 4 ? ClairDeLunePalette.PearlBlue
                                   : s < 6 ? ClairDeLunePalette.TemporalCrimson
                                   : ClairDeLunePalette.WhiteHot;
                    float dotSize = 0.27f + (s >= 6 ? 0.14f : 0f);
                    Vector2 dotPos = drawPos + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 30f;
                    sb.Draw(bloom, dotPos, null,
                        (dotColor with { A = 0 }) * 0.62f * pulse, 0f, origin,
                        dotSize, SpriteEffects.None, 0f);
                }

                // Max-stack (8) judgment ring
                if (_lastKnownStacks >= 8)
                {
                    float ringPulse = 0.5f + 0.5f * MathF.Sin(t * 0.30f);
                    sb.Draw(bloom, drawPos, null,
                        (ClairDeLunePalette.WhiteHot with { A = 0 }) * 0.42f * ringPulse, 0f, origin,
                        Projectile.scale * 2.3f * ringPulse, SpriteEffects.None, 0f);
                }
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

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<GearDrivenArbiterBuff>());
                return false;
            }
            if (owner.HasBuff(ModContent.BuffType<GearDrivenArbiterBuff>()))
                Projectile.timeLeft = 2;
            return true;
        }

        private NPC FindTarget(Player owner, float range)
        {
            if (owner.HasMinionAttackTargetNPC)
            {
                NPC manual = Main.npc[owner.MinionAttackTargetNPC];
                if (manual.active && manual.CanBeChasedBy(Projectile) && Vector2.Distance(owner.Center, manual.Center) < range * 1.5f)
                    return manual;
            }
            NPC closest = null;
            float closestDist = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy(Projectile))
                {
                    float dist = Vector2.Distance(owner.Center, npc.Center);
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
