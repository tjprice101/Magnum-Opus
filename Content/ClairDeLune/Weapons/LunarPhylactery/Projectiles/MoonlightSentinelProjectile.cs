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

namespace MagnumOpus.Content.ClairDeLune.Weapons.LunarPhylactery.Projectiles
{
    /// <summary>
    /// Lunar Phylactery Soul-Linked Sentinel — Summoner minion.
    /// Crystal minion fires homing orbs. Soul-Link: damage scales with low player HP (0.04 homing at full, 0.14 at 20% HP).
    /// Fires 3-orb burst targeting same enemy every 60f.
    /// </summary>
    public class MoonlightSentinelProjectile : ModProjectile
    {
        private float hoverAngle;
        private int fireTimer = 0;
        private NPC lastTarget = null;
        private float pulseTimer = 0f;
        private float _hpFraction = 1f;
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

            NPC target = FindTarget(owner, 700f);

            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 14f, 0.08f);
                lastTarget = target;
            }
            else
            {
                float hoverOffset = (float)Math.Sin(hoverAngle) * 30f;
                Vector2 idealPos = owner.Center + new Vector2(owner.direction * -60f, -50f + hoverOffset);
                Vector2 toIdeal = idealPos - Projectile.Center;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdeal * 0.1f, 0.08f);
            }

            Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;

            // Soul-Link homing calculation: scales from 0.04 at full HP to 0.14 at 20% HP
            float hpPercent = owner.statLife / (float)owner.statLifeMax;
            _hpFraction = hpPercent;
            pulseTimer += 0.05f;
            float homingStrength = MathHelper.Lerp(0.14f, 0.04f, hpPercent);

            // Fire 3-orb burst every 60 frames
            if (fireTimer % 60 == 0 && Main.myPlayer == owner.whoAmI)
            {
                if (lastTarget != null || target != null)
                {
                    NPC fireTarget = target ?? lastTarget;
                    for (int i = 0; i < 3; i++)
                    {
                        float angle = (i - 1) * 0.4f;
                        Vector2 orbVel = (fireTarget.Center - Projectile.Center).SafeNormalize(Vector2.UnitX).RotatedBy(angle) * 10f;

                        GenericHomingOrbChild.SpawnChild(
                            Projectile.GetSource_FromThis(),
                            Projectile.Center, orbVel,
                            Projectile.damage, Projectile.knockBack, Projectile.owner,
                            homingStrength: homingStrength,
                            behaviorFlags: 0,
                            themeIndex: GenericHomingOrbChild.THEME_CLAIRDELUNE,
                            scaleMult: 0.9f, timeLeft: 90);
                    }

                    ClairDeLuneVFXLibrary.SpawnMusicNotes(Projectile.Center, 2, 15f, 0.7f, 0.9f, 30);
                }
            }

            // Dust trail
            if (Main.rand.NextBool(4))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch,
                    -Projectile.velocity * 0.1f, 0, ClairDeLunePalette.PearlBlue, 0.6f);
                d.noGravity = true;
            }

            ClairDeLuneVFXLibrary.AddMoonbeamLight(Projectile.Center, fireTimer * 0.01f, 0.6f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // IncisorOrb shader trail (shows chase path) + 5-layer palette-cycling bloom head
            IncisorOrbRenderer.DrawOrbVisuals(Main.spriteBatch, Projectile, IncisorOrbRenderer.ClairDeLune, ref _strip);

            // HP soul-link aura: crimson glow grows as player HP falls
            float soulAwakening = MathF.Pow(1f - MathHelper.Clamp(_hpFraction, 0f, 1f), 1.5f);
            if (soulAwakening > 0.05f)
            {
                SpriteBatch sb = Main.spriteBatch;
                try
                {
                    Vector2 drawPos = Projectile.Center - Main.screenPosition;
                    float pulseSpeed = 0.08f + soulAwakening * 0.22f;
                    float pulse = 0.75f + 0.25f * MathF.Sin(pulseTimer * pulseSpeed * 20f);

                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                        Main.DefaultSamplerState, DepthStencilState.None,
                        RasterizerState.CullCounterClockwise, null,
                        Main.GameViewMatrix.TransformationMatrix);

                    Texture2D bloom = ModContent.Request<Texture2D>(
                        "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom",
                        ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                    Vector2 origin = bloom.Size() / 2f;

                    sb.Draw(bloom, drawPos, null,
                        (ClairDeLunePalette.TemporalCrimson with { A = 0 }) * (0.52f * soulAwakening) * pulse, 0f, origin,
                        Projectile.scale * (2.4f + soulAwakening * 0.9f) * pulse, SpriteEffects.None, 0f);
                }
                catch { }
                finally
                {
                    try { sb.End(); } catch { }
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                        DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }
            return false;
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<LunarPhylacteryBuff>());
                return false;
            }
            if (owner.HasBuff(ModContent.BuffType<LunarPhylacteryBuff>()))
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
