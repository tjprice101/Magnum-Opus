using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.DiesIrae;
using MagnumOpus.Content.DiesIrae.Weapons.WrathfulContract.Buffs;
using MagnumOpus.Content.DiesIrae.Systems;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathfulContract.Projectiles
{
    /// <summary>
    /// Wrathful Contract demon minion — Blood-Bound Minion pattern.
    /// Base: Fire homing orb every 60 frames
    /// Ramp: Fire rate accelerates to 30 frames over 10 seconds
    /// After 3 kills: Frenzy (5s) — double fire rate, orbs gain pierce
    /// Below 10% player HP: Breach — additional massive orb every 2 seconds
    /// </summary>
    public class WrathDemonMinion : ModProjectile
    {
        private const int BaseFireRate = 60;
        private const int MinFireRate = 30;
        private const int RampDuration = 600; // 10 seconds
        private const int FrenzyDuration = 300; // 5 seconds
        private const int BreachInterval = 120; // 2 seconds
        private const int KillsForFrenzy = 3;

        private float hoverAngle;
        private int fireTimer = 0;
        private int rampTimer = 0;
        private int breachTimer = 0;
        private float pulseTimer = 0f;
        private VertexStrip _strip;

        // State accessed via owner's ModPlayer
        private DiesIraeCombatPlayer CombatPlayer => Main.player[Projectile.owner].GetModPlayer<DiesIraeCombatPlayer>();

        private bool InFrenzy => CombatPlayer.WrathfulContractFrenzyTimer > 0;
        private bool InBreach => Main.player[Projectile.owner].statLife <= Main.player[Projectile.owner].statLifeMax2 * 0.1f;

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
            Projectile.width = 32;
            Projectile.height = 32;
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
            pulseTimer += 0.06f;
            rampTimer++;
            fireTimer++;
            breachTimer++;

            NPC target = FindTarget(owner, 800f);

            // Movement
            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                float speed = InFrenzy ? 18f : 14f;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * speed, 0.1f);
            }
            else
            {
                float hoverOffset = MathF.Sin(hoverAngle) * 30f;
                Vector2 idealPos = owner.Center + new Vector2(owner.direction * -60f, -50f + hoverOffset);
                Vector2 toIdeal = idealPos - Projectile.Center;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdeal * 0.1f, 0.08f);
            }

            Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;

            // Calculate current fire rate
            float rampProgress = Math.Clamp((float)rampTimer / RampDuration, 0f, 1f);
            int currentFireRate = (int)MathHelper.Lerp(BaseFireRate, MinFireRate, rampProgress);
            if (InFrenzy) currentFireRate /= 2;

            // Fire orbs
            if (target != null && fireTimer >= currentFireRate && Main.myPlayer == Projectile.owner)
            {
                fireTimer = 0;
                FireOrb(target, InFrenzy);
            }

            // Breach mode: additional massive orb
            if (InBreach && breachTimer >= BreachInterval && target != null && Main.myPlayer == Projectile.owner)
            {
                breachTimer = 0;
                FireBreachOrb(target);
            }

            // Ambient VFX
            if (Main.rand.NextBool(4))
            {
                Color dustCol = InFrenzy ? DiesIraePalette.WrathWhite
                    : InBreach ? DiesIraePalette.BloodRed
                    : DiesIraePalette.InfernalRed;
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(16f, 16f),
                    DustID.Torch, -Projectile.velocity * 0.1f, 0, dustCol, 0.8f);
                d.noGravity = true;
            }

            // Extra frenzy sparks
            if (InFrenzy && Main.rand.NextBool(2))
            {
                Dust spark = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f),
                    DustID.SolarFlare, Main.rand.NextVector2Circular(2f, 2f), 0, default, 0.6f);
                spark.noGravity = true;
            }

            // Lighting
            float intensity = InFrenzy ? 0.8f : InBreach ? 0.6f : 0.4f;
            float pulse = 0.85f + 0.15f * MathF.Sin(pulseTimer);
            Lighting.AddLight(Projectile.Center, DiesIraePalette.InfernalRed.ToVector3() * intensity * pulse);
        }

        private void FireOrb(NPC target, bool frenzy)
        {
            Vector2 vel = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 12f;
            int flags = frenzy ? GenericHomingOrbChild.FLAG_PIERCE | GenericHomingOrbChild.FLAG_ACCELERATE
                : GenericHomingOrbChild.FLAG_ACCELERATE;

            GenericHomingOrbChild.SpawnChild(
                Projectile.GetSource_FromThis(),
                Projectile.Center, vel,
                Projectile.damage, Projectile.knockBack, Projectile.owner,
                homingStrength: 0.08f,
                behaviorFlags: flags,
                themeIndex: GenericHomingOrbChild.THEME_DIESIRAE,
                scaleMult: frenzy ? 1.1f : 1f,
                timeLeft: 90);

            SoundEngine.PlaySound(SoundID.Item8 with { Pitch = frenzy ? 0.2f : 0f, Volume = 0.6f }, Projectile.Center);
        }

        private void FireBreachOrb(NPC target)
        {
            Vector2 vel = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 14f;

            GenericHomingOrbChild.SpawnChild(
                Projectile.GetSource_FromThis(),
                Projectile.Center, vel,
                Projectile.damage * 2, Projectile.knockBack * 2, Projectile.owner,
                homingStrength: 0.14f,
                behaviorFlags: GenericHomingOrbChild.FLAG_PIERCE | GenericHomingOrbChild.FLAG_ACCELERATE,
                themeIndex: GenericHomingOrbChild.THEME_DIESIRAE,
                scaleMult: 1.8f,
                timeLeft: 120);

            SoundEngine.PlaySound(SoundID.Item73 with { Pitch = -0.4f, Volume = 0.8f }, Projectile.Center);

            // Breach flash VFX
            DiesIraeVFXLibrary.SpawnWrathBurst(Projectile.Center, 6, 0.6f);
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<WrathfulContractBuff>());
                return false;
            }
            if (owner.HasBuff(ModContent.BuffType<WrathfulContractBuff>()))
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

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Track kills for Frenzy
            if (target.life <= 0)
            {
                CombatPlayer.WrathfulContractKillCount++;
                if (CombatPlayer.WrathfulContractKillCount >= KillsForFrenzy)
                {
                    CombatPlayer.WrathfulContractFrenzyTimer = FrenzyDuration;
                    CombatPlayer.WrathfulContractKillCount = 0;
                    SoundEngine.PlaySound(SoundID.Item119 with { Pitch = -0.2f, Volume = 0.9f }, Projectile.Center);
                    DiesIraeVFXLibrary.FinisherSlam(Projectile.Center, 0.7f);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            IncisorOrbRenderer.DrawOrbVisuals(Main.spriteBatch, Projectile, IncisorOrbRenderer.DiesIrae, ref _strip);

            // State overlays: frenzy ring + kill-progress dots
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

                // Frenzy ring
                if (InFrenzy)
                {
                    float ringScale = 0.6f + 0.1f * MathF.Sin(pulseTimer * 6f);
                    sb.Draw(glow, drawPos, null, (DiesIraePalette.HellfireGold with { A = 0 }) * 0.3f,
                        pulseTimer, origin, ringScale, SpriteEffects.None, 0f);
                }
                else
                {
                    // Kill-progress indicator: orbiting dots (0→1→2→3 = next kill triggers Frenzy)
                    int displayKills = Math.Min(CombatPlayer.WrathfulContractKillCount, KillsForFrenzy);
                    for (int i = 0; i < displayKills; i++)
                    {
                        float dotAngle = pulseTimer * 1.5f + i * MathHelper.TwoPi / KillsForFrenzy;
                        Vector2 dotPos = drawPos + new Vector2(MathF.Cos(dotAngle), MathF.Sin(dotAngle)) * 30f;
                        Color dotCol = i == 0 ? DiesIraePalette.JudgmentGold
                            : i == 1 ? DiesIraePalette.EmberOrange
                            : DiesIraePalette.WrathWhite;
                        sb.Draw(glow, dotPos, null, (dotCol with { A = 0 }) * 0.9f,
                            0f, origin, 0.10f, SpriteEffects.None, 0f);
                        sb.Draw(glow, dotPos, null, (dotCol with { A = 0 }) * 0.35f,
                            0f, origin, 0.20f, SpriteEffects.None, 0f);
                    }
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
