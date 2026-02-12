using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Common.Systems.VFX;
using static MagnumOpus.Common.Systems.Particles.Particle;

namespace MagnumOpus.Content.TestWeapons._03_CosmicRendBlade
{
    /// <summary>
    /// 🌌 Cosmic Rend Blade — 4-step void combo swing projectile.
    /// Step 0: Warp Slash — quick, angular cut through space
    /// Step 1: Void Cleave — wide horizontal rip
    /// Step 2: Rift Tear — fierce slash launching 5 VoidShardProjectiles
    /// Step 3: Dimensional Severance — massive arc opening a VoidRiftProjectile + screen shake
    /// </summary>
    public class CosmicRendBladeSwing : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.StarWrath;

        #region === Combo Phase Definition ===

        private struct ComboPhase
        {
            public CurveSegment[] AnimCurves;
            public float MaxAngle;
            public int Duration;
            public float BladeLength;
            public bool FlipDirection;
            public float SquishRange;
            public float DamageMultiplier;
        }

        // Cosmic palette: deep violet → magenta → pink → white core
        private static readonly Color[] CosmicPalette = new Color[]
        {
            new Color(40, 10, 60),
            new Color(90, 20, 140),
            new Color(160, 60, 200),
            new Color(200, 80, 220),
            new Color(220, 140, 255),
            new Color(255, 220, 255)
        };

        // Step 0: Warp Slash — short, snappy angular cut
        private static readonly ComboPhase Phase0_WarpSlash = new ComboPhase
        {
            AnimCurves = new CurveSegment[]
            {
                new CurveSegment(EasingType.ExpIn, 0f, 0f, 0.15f),
                new CurveSegment(EasingType.ExpOut, 0.15f, 0.15f, 0.75f),
                new CurveSegment(EasingType.SineOut, 0.65f, 0.9f, 0.1f),
            },
            MaxAngle = MathHelper.ToRadians(115),
            Duration = 40,
            BladeLength = 140f,
            FlipDirection = false,
            SquishRange = 0.08f,
            DamageMultiplier = 0.85f,
        };

        // Step 1: Void Cleave — wide, sweeping horizontal
        private static readonly ComboPhase Phase1_VoidCleave = new ComboPhase
        {
            AnimCurves = new CurveSegment[]
            {
                new CurveSegment(EasingType.SineIn, 0f, 0f, 0.1f),
                new CurveSegment(EasingType.PolyOut, 0.1f, 0.1f, 0.8f, 3),
                new CurveSegment(EasingType.SineOut, 0.7f, 0.9f, 0.1f),
            },
            MaxAngle = MathHelper.ToRadians(170),
            Duration = 52,
            BladeLength = 160f,
            FlipDirection = true,
            SquishRange = 0.1f,
            DamageMultiplier = 1.0f,
        };

        // Step 2: Rift Tear — sharp upward rip, spawns void shards
        private static readonly ComboPhase Phase2_RiftTear = new ComboPhase
        {
            AnimCurves = new CurveSegment[]
            {
                new CurveSegment(EasingType.ExpIn, 0f, 0f, 0.2f),
                new CurveSegment(EasingType.CircOut, 0.2f, 0.2f, 0.7f),
                new CurveSegment(EasingType.SineOut, 0.7f, 0.9f, 0.1f),
            },
            MaxAngle = MathHelper.ToRadians(150),
            Duration = 46,
            BladeLength = 155f,
            FlipDirection = false,
            SquishRange = 0.09f,
            DamageMultiplier = 1.15f,
        };

        // Step 3: Dimensional Severance — massive devastating arc
        private static readonly ComboPhase Phase3_DimensionalSeverance = new ComboPhase
        {
            AnimCurves = new CurveSegment[]
            {
                new CurveSegment(EasingType.SineIn, 0f, 0f, 0.08f),
                new CurveSegment(EasingType.ExpIn, 0.08f, 0.08f, 0.22f),
                new CurveSegment(EasingType.ExpOut, 0.30f, 0.30f, 0.55f),
                new CurveSegment(EasingType.SineOut, 0.72f, 0.85f, 0.15f),
            },
            MaxAngle = MathHelper.ToRadians(210),
            Duration = 86,
            BladeLength = 185f,
            FlipDirection = true,
            SquishRange = 0.12f,
            DamageMultiplier = 1.45f,
        };

        private static readonly ComboPhase[] AllPhases = { Phase0_WarpSlash, Phase1_VoidCleave, Phase2_RiftTear, Phase3_DimensionalSeverance };

        #endregion

        #region === State ===

        private int comboStep;
        private ref ComboPhase CurrentPhase => ref AllPhases[Math.Clamp(comboStep, 0, AllPhases.Length - 1)];
        private int swingTimer;
        private float swingDirection;
        private float baseAngle;
        private bool initialized;
        private bool hasSpawnedSecondary;

        // Stasis for combo chaining
        private const int StasisDuration = 12;
        private int stasisTimer;
        public bool InPostSwingStasis => stasisTimer > 0 && swingTimer >= CurrentPhase.Duration;

        // Trail tracking
        private const int TrailLength = 60;
        private Vector2[] tipPositions = new Vector2[TrailLength];
        private float[] tipRotations = new float[TrailLength];
        private int trailHead = 0;
        private int trailCount = 0;

        #endregion

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.extraUpdates = 2;
            Projectile.localNPCHitCooldown = 24;
            Projectile.timeLeft = 9999;
            Projectile.alpha = 255;
        }

        public override bool ShouldUpdatePosition() => false;

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }

            if (!initialized)
            {
                comboStep = (int)Projectile.ai[0];
                swingDirection = player.direction;
                baseAngle = Projectile.velocity.ToRotation();
                initialized = true;
                SoundEngine.PlaySound(SoundID.Item71 with { Pitch = -0.1f + comboStep * 0.1f }, player.Center);
            }

            ref ComboPhase phase = ref CurrentPhase;

            if (swingTimer >= phase.Duration)
            {
                stasisTimer++;
                if (stasisTimer >= StasisDuration)
                    Projectile.Kill();
                return;
            }

            swingTimer++;
            float progress = (float)swingTimer / phase.Duration;

            // PiecewiseAnimation for swing arc
            float animProgress = PiecewiseAnimation(progress, phase.AnimCurves);
            float halfAngle = phase.MaxAngle * 0.5f;
            float dirMult = phase.FlipDirection ? -swingDirection : swingDirection;
            float currentAngle = baseAngle + (-halfAngle + phase.MaxAngle * animProgress) * dirMult;

            // Calculate blade tip
            float squish = 1f - phase.SquishRange * (float)Math.Sin(progress * MathHelper.Pi);
            float bladeLen = phase.BladeLength * squish;
            Vector2 tipPos = player.MountedCenter + currentAngle.ToRotationVector2() * bladeLen;

            // Store trail
            tipPositions[trailHead] = tipPos;
            tipRotations[trailHead] = currentAngle;
            trailHead = (trailHead + 1) % TrailLength;
            if (trailCount < TrailLength) trailCount++;

            // Update projectile hitbox
            Projectile.Center = player.MountedCenter + currentAngle.ToRotationVector2() * (bladeLen * 0.5f);
            Projectile.rotation = currentAngle;
            Projectile.Size = new Vector2(bladeLen, bladeLen * 0.35f);

            // Damage multiplier
            Projectile.damage = (int)(player.GetTotalDamage(DamageClass.MeleeNoSpeed).ApplyTo(player.HeldItem.damage) * phase.DamageMultiplier);

            // Player animation
            player.heldProj = Projectile.whoAmI;
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, currentAngle - MathHelper.PiOver2);

            // === VFX ===
            SpawnSwingParticles(player, tipPos, currentAngle, progress);

            // === Step 2: Void Shards at 68% progress ===
            if (comboStep == 2 && !hasSpawnedSecondary && progress >= 0.68f)
            {
                hasSpawnedSecondary = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    SoundEngine.PlaySound(SoundID.Item125 with { Pitch = 0.3f }, player.Center);
                    for (int i = 0; i < 5; i++)
                    {
                        float spreadAngle = currentAngle + MathHelper.ToRadians(-30 + 15 * i);
                        float speed = 11f + i * 1.5f;
                        Vector2 vel = spreadAngle.ToRotationVector2() * speed;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipPos, vel,
                            ModContent.ProjectileType<VoidShardProjectile>(),
                            (int)(Projectile.damage * 0.45f), Projectile.knockBack * 0.5f, Projectile.owner);
                    }
                }
            }

            // === Step 3: Void Rift at 86% progress ===
            if (comboStep == 3 && !hasSpawnedSecondary && progress >= 0.86f)
            {
                hasSpawnedSecondary = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.4f, Volume = 1.4f }, tipPos);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipPos, Vector2.Zero,
                        ModContent.ProjectileType<VoidRiftProjectile>(),
                        (int)(Projectile.damage * 0.6f), 0f, Projectile.owner);
                }
                var shakePlayer = player.GetModPlayer<global::MagnumOpus.Content.LaCampanella.Debuffs.ScreenShakePlayer>();
                shakePlayer?.AddShake(12f, 20);
            }
        }

        private void SpawnSwingParticles(Player player, Vector2 tipPos, float angle, float progress)
        {
            if (swingTimer % (Projectile.extraUpdates + 1) != 0) return;

            // Cosmic dust at blade tip
            if (Main.rand.NextBool(2))
            {
                Vector2 dustVel = (angle + MathHelper.PiOver2 * swingDirection).ToRotationVector2() * Main.rand.NextFloat(1f, 3f);
                Dust d = Dust.NewDustPerfect(tipPos, DustID.PurpleTorch, dustVel, 60, default, 1.4f);
                d.noGravity = true;
            }

            // Void mist particles along blade
            if (Main.rand.NextBool(3))
            {
                float bladeProgress = Main.rand.NextFloat(0.4f, 0.9f);
                Vector2 alongBlade = player.MountedCenter + angle.ToRotationVector2() * CurrentPhase.BladeLength * bladeProgress;
                Color cosmicColor = GetCosmicGradient(progress);
                var glow = new GenericGlowParticle(alongBlade, Main.rand.NextVector2Circular(0.8f, 0.8f),
                    cosmicColor * 0.7f, Main.rand.NextFloat(0.2f, 0.35f), Main.rand.Next(8, 14), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Step 3 extra: void energy sparks
            if (comboStep == 3 && Main.rand.NextBool(2))
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(2f, 2f);
                var spark = new GlowSparkParticle(tipPos, sparkVel, new Color(180, 100, 255),
                    Main.rand.NextFloat(0.3f, 0.5f), Main.rand.Next(8, 15));
                MagnumParticleHandler.SpawnParticle(spark);
            }

            Lighting.AddLight(tipPos, 0.3f, 0.1f, 0.5f);
        }

        private Color GetCosmicGradient(float progress)
        {
            float scaled = progress * (CosmicPalette.Length - 1);
            int idx = (int)scaled;
            int next = Math.Min(idx + 1, CosmicPalette.Length - 1);
            return Color.Lerp(CosmicPalette[idx], CosmicPalette[next], scaled - idx);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Cosmic spark burst on hit
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(5f, 5f);
                Color sparkColor = Color.Lerp(new Color(160, 60, 200), new Color(255, 180, 255), Main.rand.NextFloat());
                var spark = new GlowSparkParticle(target.Center, sparkVel, sparkColor, 0.35f, Main.rand.Next(8, 14));
                MagnumParticleHandler.SpawnParticle(spark);
            }
            for (int i = 0; i < 6; i++)
            {
                Dust d = Dust.NewDustPerfect(target.Center, DustID.PurpleTorch,
                    Main.rand.NextVector2Circular(5f, 5f), 0, default, 1.3f);
                d.noGravity = true;
            }

            // Step 3 slam: bloom ring on hit
            if (comboStep == 3)
            {
                var ring = new BloomRingParticle(target.Center, Vector2.Zero, new Color(200, 100, 255), 0.6f, 16);
                MagnumParticleHandler.SpawnParticle(ring);
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Player player = Main.player[Projectile.owner];
            float collisionPoint = 0f;
            Vector2 bladeEnd = player.MountedCenter + Projectile.rotation.ToRotationVector2() * CurrentPhase.BladeLength;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), player.MountedCenter, bladeEnd, CurrentPhase.BladeLength * 0.15f, ref collisionPoint);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (!initialized || swingTimer < 1) return false;

            SpriteBatch sb = Main.spriteBatch;
            Player player = Main.player[Projectile.owner];

            // --- Trail ---
            if (trailCount > 2)
            {
                int trailPoints = Math.Min(trailCount, TrailLength);
                Vector2[] positions = new Vector2[trailPoints];
                float[] rotations = new float[trailPoints];
                for (int i = 0; i < trailPoints; i++)
                {
                    int idx = ((trailHead - 1 - i) % TrailLength + TrailLength) % TrailLength;
                    positions[i] = tipPositions[idx];
                    rotations[i] = tipRotations[idx];
                }
                float trailWidth = CurrentPhase.BladeLength * 0.22f;
                Color trailPrimary = new Color(140, 50, 200);
                Color trailSecondary = new Color(220, 150, 255);
                CalamityStyleTrailRenderer.DrawTrailWithBloom(positions, rotations, CalamityStyleTrailRenderer.TrailStyle.Cosmic,
                    trailWidth, trailPrimary, trailSecondary, 0.8f, 2.0f);
            }

            // --- Blade glow ---
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = new Vector2(0, tex.Height * 0.5f);
            float bladeDraw = CurrentPhase.BladeLength / tex.Width;
            Vector2 drawPos = player.MountedCenter - Main.screenPosition;
            SpriteEffects flip = swingDirection < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            float progress = (float)swingTimer / CurrentPhase.Duration;

            SwingShaderSystem.BeginAdditive(sb);
            Color outerGlow = new Color(100, 30, 160, 0) * 0.35f;
            Color innerGlow = new Color(180, 80, 240, 0) * 0.5f;
            sb.Draw(tex, drawPos, null, outerGlow, Projectile.rotation, origin, new Vector2(bladeDraw * 1.1f, 1.1f), flip, 0f);
            sb.Draw(tex, drawPos, null, innerGlow, Projectile.rotation, origin, new Vector2(bladeDraw, 1f), flip, 0f);
            SwingShaderSystem.RestoreSpriteBatch(sb);

            // --- Main blade sprite ---
            sb.Draw(tex, drawPos, null, lightColor, Projectile.rotation, origin, new Vector2(bladeDraw, 1f), flip, 0f);

            // --- Lens flare at tip ---
            if (progress > 0.1f && progress < 0.9f)
            {
                Texture2D flare = Terraria.GameContent.TextureAssets.Extra[98].Value;
                Vector2 tipDraw = tipPositions[((trailHead - 1) % TrailLength + TrailLength) % TrailLength] - Main.screenPosition;
                Vector2 flareOrigin = flare.Size() * 0.5f;
                float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.12f;
                Color flareColor = new Color(200, 100, 255, 0) * 0.6f * pulse;
                sb.Draw(flare, tipDraw, null, flareColor, Main.GameUpdateCount * 0.06f, flareOrigin, 0.18f * pulse, SpriteEffects.None, 0f);
            }

            return false;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(comboStep);
            writer.Write(swingTimer);
            writer.Write(swingDirection);
            writer.Write(baseAngle);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            comboStep = reader.ReadInt32();
            swingTimer = reader.ReadInt32();
            swingDirection = reader.ReadSingle();
            baseAngle = reader.ReadSingle();
            initialized = true;
        }
    }
}
