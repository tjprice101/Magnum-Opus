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
using MagnumOpus.Content.DiesIrae.Weapons.DeathTollingBell.Buffs;

namespace MagnumOpus.Content.DiesIrae.Weapons.DeathTollingBell.Projectiles
{
    /// <summary>
    /// Death Tolling Bell minion — Spectral bell with concentric shockwaves.
    /// Hovers near player, periodically tolls releasing 3 rings of 12 shockwave projectiles.
    /// Each ring expands at different speeds for layered damage.
    /// </summary>
    public class BellTollingMinion : ModProjectile
    {
        private const int TollInterval = 180; // 3 seconds
        private const int WavesPerRing = 12;
        private const int RingCount = 3;

        private float hoverAngle;
        private float pulseTimer;
        private float swingAngle;
        private int tollTimer;
        private int ringDelayTimer;
        private int currentRing;
        private bool tolling;
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
            Projectile.width = 36;
            Projectile.height = 36;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!CheckActive(owner))
                return;

            hoverAngle += 0.025f;
            pulseTimer += 0.08f;
            tollTimer++;

            // Bell swing animation
            if (tolling)
            {
                swingAngle = MathF.Sin(pulseTimer * 8f) * 0.3f * (1f - (float)ringDelayTimer / 40f);
                ringDelayTimer++;

                // Fire rings with staggered timing
                if (ringDelayTimer == 1 || ringDelayTimer == 15 || ringDelayTimer == 30)
                {
                    if (Main.myPlayer == Projectile.owner)
                        FireRing(currentRing);
                    currentRing++;

                    // VFX per ring
                    DiesIraeVFXLibrary.SpawnWrathBurst(Projectile.Center, 8, 0.8f);
                }

                if (ringDelayTimer >= 40)
                {
                    tolling = false;
                    ringDelayTimer = 0;
                    currentRing = 0;
                }
            }
            else
            {
                // Gentle idle swing
                swingAngle = MathF.Sin(hoverAngle * 2f) * 0.1f;
            }

            // Check for toll
            NPC target = FindTarget(owner, 800f);
            if (target != null && tollTimer >= TollInterval && !tolling)
            {
                tollTimer = 0;
                tolling = true;
                ringDelayTimer = 0;
                currentRing = 0;

                // Toll sound
                SoundEngine.PlaySound(SoundID.Item119 with { Pitch = -0.5f, Volume = 1.0f }, Projectile.Center);
            }

            // Hover movement
            float hoverOffset = MathF.Sin(hoverAngle) * 20f;
            Vector2 idealPos = owner.Center + new Vector2(owner.direction * -50f, -70f + hoverOffset);
            Vector2 toIdeal = idealPos - Projectile.Center;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdeal * 0.08f, 0.06f);

            Projectile.rotation = swingAngle;

            // Ambient VFX
            if (Main.rand.NextBool(tolling ? 2 : 5))
            {
                Color dustCol = tolling
                    ? Color.Lerp(DiesIraePalette.JudgmentGold, DiesIraePalette.WrathWhite, Main.rand.NextFloat())
                    : Color.Lerp(DiesIraePalette.InfernalRed, DiesIraePalette.CharcoalBlack, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(18f, 18f),
                    DustID.Torch, new Vector2(0, -Main.rand.NextFloat(0.5f, 1.5f)), 0, dustCol, 0.7f);
                d.noGravity = true;
            }

            // Tolling sparks
            if (tolling && Main.rand.NextBool(2))
            {
                Dust spark = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(25f, 25f),
                    DustID.SolarFlare, Main.rand.NextVector2Circular(2f, 2f), 0, default, 0.8f);
                spark.noGravity = true;
            }

            // Lighting
            float intensity = tolling ? 0.7f : 0.4f;
            float pulse = 0.85f + 0.15f * MathF.Sin(pulseTimer * 2f);
            Lighting.AddLight(Projectile.Center, DiesIraePalette.JudgmentGold.ToVector3() * intensity * pulse);
        }

        private void FireRing(int ringIndex)
        {
            // Each ring has different speed and damage scaling
            float[] speeds = { 10f, 14f, 18f };
            float[] damageScales = { 1f, 0.8f, 0.6f };
            float speed = speeds[Math.Min(ringIndex, speeds.Length - 1)];
            float damageScale = damageScales[Math.Min(ringIndex, damageScales.Length - 1)];

            for (int i = 0; i < WavesPerRing; i++)
            {
                float angle = MathHelper.TwoPi * i / WavesPerRing + ringIndex * 0.15f;
                Vector2 vel = angle.ToRotationVector2() * speed;

                GenericHomingOrbChild.SpawnChild(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center, vel,
                    (int)(Projectile.damage * damageScale), Projectile.knockBack * 0.5f, Projectile.owner,
                    homingStrength: 0.03f + ringIndex * 0.02f,
                    behaviorFlags: GenericHomingOrbChild.FLAG_ACCELERATE,
                    themeIndex: GenericHomingOrbChild.THEME_DIESIRAE,
                    scaleMult: 0.9f - ringIndex * 0.1f,
                    timeLeft: 80 + ringIndex * 10);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 180);

            // Impact VFX
            for (int i = 0; i < 6; i++)
            {
                Color col = Color.Lerp(DiesIraePalette.InfernalRed, DiesIraePalette.JudgmentGold, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(target.Center, DustID.Torch,
                    Main.rand.NextVector2CircularEdge(4f, 4f), 0, col, 0.9f);
                d.noGravity = true;
            }
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<DeathTollingBellBuff>());
                return false;
            }
            if (owner.HasBuff(ModContent.BuffType<DeathTollingBellBuff>()))
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
            IncisorOrbRenderer.DrawOrbVisuals(Main.spriteBatch, Projectile, IncisorOrbRenderer.DiesIrae, ref _strip);

            // Bell-specific overlays: clapper when tolling + expanding toll rings
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                float pulse = 0.9f + 0.1f * MathF.Sin(pulseTimer * 2f);
                float tollPulse = tolling ? (1f + 0.3f * MathF.Sin(pulseTimer * 8f)) : 1f;
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

                // Bell clapper (swinging golden dot when tolling)
                if (tolling)
                {
                    Vector2 clapperOffset = new Vector2(MathF.Sin(swingAngle * 3f) * 8f, 10f);
                    float clapperScale = 0.08f * tollPulse;
                    sb.Draw(glow, drawPos + clapperOffset, null, (DiesIraePalette.JudgmentGold with { A = 0 }) * 0.8f,
                        0f, origin, clapperScale, SpriteEffects.None, 0f);
                }

                // Toll rings (expanding concentric shockwaves during toll)
                if (tolling && currentRing < RingCount)
                {
                    for (int r = 0; r <= currentRing; r++)
                    {
                        float ringProgress = (float)ringDelayTimer / 40f;
                        float ringRadius = (0.3f + ringProgress * 0.4f + r * 0.15f) * tollPulse;
                        float ringAlpha = (1f - ringProgress) * 0.3f;
                        Color ringCol = r == 0 ? DiesIraePalette.JudgmentGold
                            : r == 1 ? DiesIraePalette.InfernalRed
                            : DiesIraePalette.BloodRed;
                        sb.Draw(glow, drawPos, null, (ringCol with { A = 0 }) * ringAlpha,
                            pulseTimer + r * 0.5f, origin, ringRadius, SpriteEffects.None, 0f);
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
