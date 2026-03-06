using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.DiesIrae;
using MagnumOpus.Content.DiesIrae.Weapons.StaffOfFinalJudgement.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.StaffOfFinalJudgement.Projectiles
{
    /// <summary>
    /// Floating Ignition Mine — divine fire mine with state machine.
    /// States: 0 = Unarmed (1s), 1 = Armed (waiting), 2 = Near-trigger (enemy within 3 tiles), Detonate on contact.
    /// Mines connect with purgatory field lines when 4+ exist.
    /// Chain detonation when adjacent mines within 5 tiles are hit.
    /// Judgment Storm: 3+ detonating within 1s → massive fire rain.
    /// </summary>
    public class FloatingIgnitionProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        // State tracking
        private int MineState
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        private float StateTimer
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        private const float ArmDelay = 60f; // 1 second to arm
        private const float TriggerRange = 48f; // 3 tiles
        private const float ChainRange = 80f; // 5 tiles for chain detonation
        private const float FieldRange = 160f; // 10 tiles for purgatory field connections
        private const int MaxMines = 8;

        // Static tracking for Judgment Storm (3+ detonations within 1s)
        private static int recentDetonations = 0;
        private static int detonationCooldown = 0;

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 900; // 15 seconds before auto-detonate
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            StateTimer++;

            // Static detonation counter cooldown
            if (Projectile.whoAmI == 0) // Only one projectile manages the counter
            {
                if (detonationCooldown > 0)
                {
                    detonationCooldown--;
                    if (detonationCooldown <= 0)
                    {
                        recentDetonations = 0;
                    }
                }
            }

            switch (MineState)
            {
                case 0: // Unarmed — travel to position, then hover
                    // Decelerate to a stop
                    Projectile.velocity *= 0.94f;
                    if (StateTimer >= ArmDelay)
                    {
                        MineState = 1;
                        Projectile.velocity = Vector2.Zero;
                        Projectile.netUpdate = true;
                    }
                    break;

                case 1: // Armed — hover in place, check for enemies
                    Projectile.velocity = Vector2.Zero;

                    // Gentle bob
                    float bob = (float)Math.Sin(StateTimer * 0.05f) * 0.3f;
                    Projectile.position.Y += bob;

                    // Check for nearby enemies — trigger zone
                    if (FindNearestEnemy(TriggerRange) != null)
                    {
                        MineState = 2;
                        Projectile.netUpdate = true;
                    }
                    break;

                case 2: // Near-trigger — pulsing, about to detonate
                    Projectile.velocity = Vector2.Zero;

                    // Brief trigger delay (15 frames)
                    if (StateTimer - ArmDelay > 15f || Projectile.ai[0] == 2)
                    {
                        // Detonate!
                        Detonate();
                        Projectile.Kill();
                        return;
                    }
                    break;
            }

            // Dust based on state
            if (!Main.dedServ)
            {
                if (MineState == 0 && Main.rand.NextBool(4))
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch,
                        Main.rand.NextVector2Circular(0.5f, 0.5f), 0, default, 0.6f);
                    d.noGravity = true;
                }
                else if (MineState == 1 && Main.rand.NextBool(3))
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f),
                        DustID.Torch, new Vector2(0, -0.5f), 0, default, 0.8f);
                    d.noGravity = true;
                }
                else if (MineState == 2)
                {
                    // Aggressive sparking
                    for (int i = 0; i < 2; i++)
                    {
                        Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                            DustID.GoldFlame, Main.rand.NextVector2Circular(2f, 2f), 0, default, 1f);
                        d.noGravity = true;
                    }
                }
            }

            // Lighting
            float lightLevel = MineState switch { 0 => 0.3f, 1 => 0.5f, _ => 0.8f };
            Lighting.AddLight(Projectile.Center, lightLevel, lightLevel * 0.3f, 0.05f);
        }

        private void Detonate()
        {
            // VFX
            StaffOfFinalJudgementUtils.DoDetonation(Projectile.Center);

            // Track for Judgment Storm
            recentDetonations++;
            detonationCooldown = 60; // 1s window

            if (recentDetonations >= 3)
            {
                StaffOfFinalJudgementUtils.DoJudgmentStorm(Projectile.Center);
                recentDetonations = 0;
            }

            // Chain detonation — trigger nearby armed mines
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile other = Main.projectile[i];
                if (other.active && other.whoAmI != Projectile.whoAmI &&
                    other.type == Projectile.type && other.owner == Projectile.owner)
                {
                    float dist = Vector2.Distance(Projectile.Center, other.Center);
                    if (dist < ChainRange && other.ai[0] >= 1) // Must be armed
                    {
                        // Force detonation of adjacent mine
                        other.ai[0] = 2; // Set to trigger state
                        other.ai[1] = ArmDelay + 20f; // Immediate trigger
                        other.netUpdate = true;
                    }
                }
            }

            // Damage in radius
            float blastRadius = 64f; // 4 tiles
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                {
                    if (Vector2.Distance(Projectile.Center, npc.Center) < blastRadius)
                    {
                        npc.AddBuff(BuffID.OnFire3, 300);
                    }
                }
            }
        }

        private NPC FindNearestEnemy(float range)
        {
            NPC closest = null;
            float minDist = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closest = npc;
                    }
                }
            }
            return closest;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 240);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Draw purgatory field connections to other mines
            int mineCount = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile other = Main.projectile[i];
                if (other.active && other.type == Projectile.type && other.owner == Projectile.owner)
                {
                    mineCount++;
                    if (other.whoAmI != Projectile.whoAmI && other.ai[0] >= 1) // Both armed
                    {
                        float dist = Vector2.Distance(Projectile.Center, other.Center);
                        if (dist < FieldRange && MineState >= 1)
                        {
                            StaffOfFinalJudgementUtils.DrawFieldLine(sb, Projectile.Center, other.Center, StateTimer);
                        }
                    }
                }
            }

            // Draw mine body
            StaffOfFinalJudgementUtils.DrawMineBody(sb, Projectile.Center, MineState, StateTimer);

            // Dies Irae theme accent layer
            StaffOfFinalJudgementUtils.DrawThemeAccents(sb, Projectile.Center, 1f, 0.6f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}