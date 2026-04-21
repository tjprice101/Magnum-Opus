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

namespace MagnumOpus.Content.ClairDeLune.Weapons.AutomatonsTuningFork.Projectiles
{
    /// <summary>
    /// Automaton's Tuning Fork Minion — Frequency Modes support.
    /// Right-click cycles 4 frequencies: A (pierce 2, -20% speed), C (+40% speed, no homing),
    /// E (split on hit, 60% damage), G (slow, decelerate, become zone on expiry).
    /// Perfect Resonance (all 4 in 10s): all properties at once for 5s.
    /// </summary>
    public class AutomatonMinionProjectile : ModProjectile
    {
        private float hoverAngle;
        private int fireTimer = 0;
        private float pulseTimer = 0f;
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
            pulseTimer += 0.06f;

            var combatPlayer = owner.GetModPlayer<ClairDeLuneCombatPlayer>();
            int currentFreq = combatPlayer.AutomatonFrequency;

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

            // Fire orbs based on frequency
            if (fireTimer % 40 == 0 && target != null && Main.myPlayer == owner.whoAmI)
            {
                FireFrequencyOrb(owner, target, currentFreq);
            }

            // Dust trail
            if (Main.rand.NextBool(4))
            {
                Color trailColor = currentFreq switch
                {
                    0 => ClairDeLunePalette.PearlBlue,      // A
                    1 => ClairDeLunePalette.SoftBlue,       // C
                    2 => ClairDeLunePalette.TemporalCrimson, // E
                    3 => ClairDeLunePalette.MoonlitFrost,   // G
                    _ => ClairDeLunePalette.PearlBlue
                };
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch,
                    -Projectile.velocity * 0.1f, 0, trailColor, 0.6f);
                d.noGravity = true;
            }

            ClairDeLuneVFXLibrary.AddClairDeLuneLight(Projectile.Center, 0.6f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // IncisorOrb shader trail (shows chase path) + 5-layer palette-cycling bloom head
            IncisorOrbRenderer.DrawOrbVisuals(Main.spriteBatch, Projectile, IncisorOrbRenderer.ClairDeLune, ref _strip);

            // Frequency-coded orbiting satellites (A=1, C=2, E=3, G=4)
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                Player owner = Main.player[Projectile.owner];
                var combatPlayer = owner.GetModPlayer<ClairDeLuneCombatPlayer>();
                int freq = Math.Max(0, Math.Min(3, combatPlayer.AutomatonFrequency));

                Color[] freqColors = {
                    ClairDeLunePalette.PearlBlue,        // A
                    ClairDeLunePalette.SoftBlue,         // C
                    ClairDeLunePalette.TemporalCrimson,  // E
                    ClairDeLunePalette.MoonlitFrost,     // G
                };
                int[] satCounts   = { 1, 2, 3, 4 };
                float[] satSpeeds = { 0.040f, 0.070f, 0.055f, 0.030f };

                Color freqColor = freqColors[freq];
                float orbitSpeed = satSpeeds[freq];
                int satCount = satCounts[freq];
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

                for (int s = 0; s < satCount; s++)
                {
                    float angle = t * orbitSpeed * 60f + s * MathHelper.TwoPi / satCount;
                    Vector2 satPos = drawPos + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 28f;
                    sb.Draw(bloom, satPos, null,
                        (freqColor with { A = 0 }) * 0.58f * pulse, 0f, origin,
                        0.28f, SpriteEffects.None, 0f);
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

        private void FireFrequencyOrb(Player owner, NPC target, int frequency)
        {
            int flags = 0;
            float homing = 0.06f;
            float speed = 10f;
            int timeLeft = 90;

            switch (frequency)
            {
                case 0: // A: pierce 2, -20% speed
                    flags = GenericHomingOrbChild.FLAG_PIERCE;
                    speed = 8f;
                    break;

                case 1: // C: +40% speed, no homing
                    homing = 0f;
                    speed = 14f;
                    break;

                case 2: // E: split on hit, 60% damage
                    flags = GenericHomingOrbChild.FLAG_ZONE_ON_KILL;
                    homing = 0.06f;
                    break;

                case 3: // G: slow, decelerate
                    flags = GenericHomingOrbChild.FLAG_DECELERATE;
                    homing = 0.04f;
                    speed = 8f;
                    break;
            }

            Vector2 orbVel = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * speed;

            GenericHomingOrbChild.SpawnChild(
                Projectile.GetSource_FromThis(),
                Projectile.Center, orbVel,
                Projectile.damage, Projectile.knockBack, Projectile.owner,
                homingStrength: homing,
                behaviorFlags: flags,
                themeIndex: GenericHomingOrbChild.THEME_CLAIRDELUNE,
                scaleMult: 0.9f, timeLeft: timeLeft);
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<AutomatonsTuningForkBuff>());
                return false;
            }
            if (owner.HasBuff(ModContent.BuffType<AutomatonsTuningForkBuff>()))
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
