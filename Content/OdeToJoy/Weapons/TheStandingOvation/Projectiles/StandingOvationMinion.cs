using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy;
using MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation.Buffs;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation.Projectiles
{
    public class StandingOvationMinion : ModProjectile
    {
        private float hoverAngle;
        private float pulseTimer;
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

            if (Main.rand.NextBool(4))
            {
                Color dustCol = Main.rand.NextBool() ? OdeToJoyPalette.RosePink : OdeToJoyPalette.PetalPink;
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), DustID.Flare,
                    Main.rand.NextVector2Circular(0.4f, 0.4f), 0, dustCol, 0.45f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, OdeToJoyPalette.RosePink.ToVector3() * 0.3f);
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<StandingOvationBuff>());
                return false;
            }
            if (owner.HasBuff(ModContent.BuffType<StandingOvationBuff>()))
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

        public override bool PreDraw(ref Color lightColor)
        {
            IncisorOrbRenderer.DrawOrbVisuals(Main.spriteBatch, Projectile, IncisorOrbRenderer.OdeToJoy, ref _strip);

            // Rose bloom overlay: 4 petal-like blooms at cardinal angles, slowly rotating
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                Vector2 drawPos = Projectile.Center - Main.screenPosition;

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);

                Texture2D glow = ModContent.Request<Texture2D>(
                    "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                Vector2 origin = glow.Size() / 2f;

                for (int i = 0; i < 4; i++)
                {
                    float petalAngle = pulseTimer * 0.4f + i * MathHelper.TwoPi / 4f;
                    Vector2 petalPos = drawPos + new Vector2(MathF.Cos(petalAngle), MathF.Sin(petalAngle)) * 18f;
                    float petalPulse = 0.8f + 0.2f * MathF.Sin(pulseTimer * 2.5f + i * MathHelper.TwoPi / 4f);
                    sb.Draw(glow, petalPos, null, (OdeToJoyPalette.RosePink with { A = 0 }) * 0.40f * petalPulse,
                        0f, origin, 0.22f, SpriteEffects.None, 0f);
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
    }
}
