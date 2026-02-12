using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using static MagnumOpus.Common.Systems.Particles.Particle;

namespace MagnumOpus.Content.TestWeapons._05_ArcaneHarmonics
{
    /// <summary>
    /// 🎵 Arcane Harmonics 5-Step Combo Swing Projectile
    /// Step 0: Prelude Strike — quick horizontal opening slash
    /// Step 1: Staccato Rend — sharp reverse cut with staccato snap
    /// Step 2: Crescendo Arc — wide sweeping arc, spawns 3 NoteProjectiles at 62%
    /// Step 3: Fortissimo Cleave — heavy downward strike, spawns HarmonicRingProjectile at 70%
    /// Step 4: Grand Finale — massive slam, spawns SymphonyBurstProjectile + ResonanceFieldProjectile at 86% + screen shake
    ///
    /// Uses EnhancedTrailRenderer (RenderMultiPassTrail) instead of CalamityStyleTrailRenderer.
    /// Uses HueShiftingMusicNoteParticle for unique music-themed particle VFX.
    /// </summary>
    public class ArcaneHarmonicsSwing : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.EnchantedSword;

        #region ComboPhase Definition
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

        // Step 0: Prelude Strike — quick horizontal slash (opening note)
        private static readonly ComboPhase Phase0_PreludeStrike = new ComboPhase
        {
            AnimCurves = new CurveSegment[]
            {
                new CurveSegment(EasingType.SineOut, 0f, -1f, 0.3f),
                new CurveSegment(EasingType.PolyIn, 0.2f, -0.7f, 1.5f, 3),
                new CurveSegment(EasingType.PolyOut, 0.8f, 0.8f, 0.2f, 2)
            },
            MaxAngle = MathHelper.Pi * 0.6f,
            Duration = 40,
            BladeLength = 140f,
            FlipDirection = false,
            SquishRange = 0.14f,
            DamageMultiplier = 0.85f
        };

        // Step 1: Staccato Rend — sharp reverse cut (accented beat, flipped)
        private static readonly ComboPhase Phase1_StaccatoRend = new ComboPhase
        {
            AnimCurves = new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -1f, 0.35f, 2),
                new CurveSegment(EasingType.ExpIn, 0.2f, -0.65f, 1.6f),
                new CurveSegment(EasingType.PolyOut, 0.78f, 0.95f, 0.05f, 2)
            },
            MaxAngle = MathHelper.Pi * 0.7f,
            Duration = 38,
            BladeLength = 145f,
            FlipDirection = true,
            SquishRange = 0.12f,
            DamageMultiplier = 0.95f
        };

        // Step 2: Crescendo Arc — wide sweeping arc, spawns NoteProjectiles at 62%
        private static readonly ComboPhase Phase2_CrescendoArc = new ComboPhase
        {
            AnimCurves = new CurveSegment[]
            {
                new CurveSegment(EasingType.SineOut, 0f, -1f, 0.25f),
                new CurveSegment(EasingType.SineIn, 0.18f, -0.75f, 1.3f),
                new CurveSegment(EasingType.PolyOut, 0.75f, 0.55f, 0.45f, 2)
            },
            MaxAngle = MathHelper.Pi * 0.92f,
            Duration = 50,
            BladeLength = 160f,
            FlipDirection = false,
            SquishRange = 0.18f,
            DamageMultiplier = 1.05f
        };

        // Step 3: Fortissimo Cleave — heavy downward strike, spawns HarmonicRingProjectile at 70%
        private static readonly ComboPhase Phase3_FortissimoCleave = new ComboPhase
        {
            AnimCurves = new CurveSegment[]
            {
                new CurveSegment(EasingType.SineOut, 0f, -1f, 0.22f),
                new CurveSegment(EasingType.ExpIn, 0.16f, -0.78f, 1.8f),
                new CurveSegment(EasingType.PolyOut, 0.72f, 1.02f, -0.02f, 2)
            },
            MaxAngle = MathHelper.Pi * 0.85f,
            Duration = 52,
            BladeLength = 155f,
            FlipDirection = true,
            SquishRange = 0.2f,
            DamageMultiplier = 1.2f
        };

        // Step 4: Grand Finale — massive slam, spawns SymphonyBurst + ResonanceField at 86%
        private static readonly ComboPhase Phase4_GrandFinale = new ComboPhase
        {
            AnimCurves = new CurveSegment[]
            {
                new CurveSegment(EasingType.SineOut, 0f, -1f, 0.18f),
                new CurveSegment(EasingType.PolyIn, 0.12f, -0.82f, 0.45f, 2),
                new CurveSegment(EasingType.ExpIn, 0.4f, -0.37f, 1.6f),
                new CurveSegment(EasingType.PolyOut, 0.88f, 1.23f, -0.23f, 2)
            },
            MaxAngle = MathHelper.Pi * 1.15f,
            Duration = 86,
            BladeLength = 185f,
            FlipDirection = false,
            SquishRange = 0.25f,
            DamageMultiplier = 1.5f
        };

        private static readonly ComboPhase[] Phases =
        {
            Phase0_PreludeStrike, Phase1_StaccatoRend, Phase2_CrescendoArc,
            Phase3_FortissimoCleave, Phase4_GrandFinale
        };
        #endregion

        #region Arcane Palette
        private static readonly Color[] ArcanePalette = new Color[]
        {
            new Color(40, 20, 100),
            new Color(80, 40, 180),
            new Color(130, 70, 220),
            new Color(180, 120, 240),
            new Color(220, 180, 255),
            new Color(255, 240, 255)
        };

        private Color GetPaletteColor(float t)
        {
            float scaled = t * (ArcanePalette.Length - 1);
            int idx = Math.Clamp((int)scaled, 0, ArcanePalette.Length - 2);
            return Color.Lerp(ArcanePalette[idx], ArcanePalette[idx + 1], scaled - idx);
        }
        #endregion

        #region State
        private const int TrailLength = 60;
        private const int StasisDuration = 12;

        private Vector2[] tipPositions = new Vector2[TrailLength];
        private float[] tipRotations = new float[TrailLength];
        private int trailIndex = 0;
        private bool hasSpawnedSecondary = false;
        private int currentStep = 0;

        private Player Owner => Main.player[Projectile.owner];
        private ComboPhase ActivePhase => Phases[Math.Clamp(currentStep, 0, Phases.Length - 1)];
        public int SwingTime { get => (int)Projectile.localAI[0]; set => Projectile.localAI[0] = value; }
        public float SquishFactor { get => Projectile.localAI[1]; set => Projectile.localAI[1] = value; }
        private float Timer => SwingTime - Projectile.timeLeft;
        private float Progression => SwingTime > 0 ? Timer / SwingTime : 0f;
        public bool InPostSwingStasis { get; set; }
        private int Direction => Math.Sign(Projectile.velocity.X) <= 0 ? -1 : 1;
        private float BaseRotation => Projectile.velocity.ToRotation();
        #endregion

        #region Animation
        private float SwingAngleAtProgress(float progress)
        {
            var phase = ActivePhase;
            return phase.MaxAngle * PiecewiseAnimation(progress, phase.AnimCurves)
                   * (phase.FlipDirection ? -1f : 1f);
        }

        private float SwordRotation => BaseRotation + SwingAngleAtProgress(Progression) * Direction;

        private float SquishAtProgress(float progress)
        {
            float shift = Math.Abs(SwingAngleAtProgress(progress));
            float range = ActivePhase.SquishRange;
            return MathHelper.Lerp(1f + range * 0.6f, 1f - range, (float)Math.Abs(Math.Sin(shift)));
        }

        private Vector2 SwordDirection => SwordRotation.ToRotationVector2() * SquishAtProgress(Progression);
        #endregion

        #region Setup
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = TrailLength;
        }

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 9999;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.MaxUpdates = 3;
            Projectile.localNPCHitCooldown = Projectile.MaxUpdates * 8;
            Projectile.noEnchantmentVisuals = true;
        }
        #endregion

        public override bool ShouldUpdatePosition() => false;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            Vector2 start = Owner.RotatedRelativePoint(Owner.MountedCenter, true);
            Vector2 end = start + SwordDirection * ActivePhase.BladeLength * Projectile.scale;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 40f, ref _);
        }

        public override void AI()
        {
            Player player = Owner;
            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }

            // Initialize on first real frame
            if (SwingTime == 0)
            {
                currentStep = Math.Clamp((int)Projectile.ai[0], 0, Phases.Length - 1);
                SwingTime = ActivePhase.Duration * Projectile.MaxUpdates;
                Projectile.timeLeft = SwingTime + StasisDuration * Projectile.MaxUpdates;
                SquishFactor = 1f - ActivePhase.SquishRange;

                // Pitch rises with combo step for musical escalation
                float pitch = -0.2f + currentStep * 0.12f;
                SoundEngine.PlaySound(SoundID.Item71 with { Pitch = pitch }, player.Center);
            }

            // Post-swing stasis
            if (Timer >= SwingTime)
            {
                InPostSwingStasis = true;
                player.heldProj = Projectile.whoAmI;
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, SwordRotation - MathHelper.PiOver2);
                return;
            }

            InPostSwingStasis = false;
            float progress = Progression;
            Projectile.damage = (int)(player.GetWeaponDamage(player.HeldItem) * ActivePhase.DamageMultiplier);

            // Player attachment
            Projectile.Center = player.RotatedRelativePoint(player.MountedCenter, true);
            player.heldProj = Projectile.whoAmI;
            player.ChangeDir(Direction);
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, SwordRotation - MathHelper.PiOver2);

            // Trail tracking
            Vector2 tipWorld = Projectile.Center + SwordDirection * ActivePhase.BladeLength * Projectile.scale;
            trailIndex = (trailIndex + 1) % TrailLength;
            tipPositions[trailIndex] = tipWorld;
            tipRotations[trailIndex] = SwordRotation;

            // VFX — arcane dust + music notes
            if (!Main.dedServ && Timer % Projectile.MaxUpdates == 0)
                SpawnSwingParticles(tipWorld, progress);

            // Step 2: Spawn orbiting note projectiles at 62%
            if (currentStep == 2 && !hasSpawnedSecondary && progress >= 0.62f)
            {
                hasSpawnedSecondary = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    SoundEngine.PlaySound(SoundID.Item26 with { Pitch = 0.4f }, tipWorld);
                    for (int i = 0; i < 3; i++)
                    {
                        float spread = MathHelper.ToRadians(-20f + 20f * i);
                        Vector2 noteVel = SwordDirection.RotatedBy(spread) * Main.rand.NextFloat(7f, 10f);
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipWorld, noteVel,
                            ModContent.ProjectileType<NoteProjectile>(),
                            (int)(Projectile.damage * 0.4f), Projectile.knockBack * 0.3f, Projectile.owner);
                    }
                }
            }

            // Step 3: Spawn harmonic ring at 70%
            if (currentStep == 3 && !hasSpawnedSecondary && progress >= 0.70f)
            {
                hasSpawnedSecondary = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.1f, Volume = 1.1f }, tipWorld);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipWorld, Vector2.Zero,
                        ModContent.ProjectileType<HarmonicRingProjectile>(),
                        (int)(Projectile.damage * 0.5f), Projectile.knockBack * 0.4f, Projectile.owner);
                }
            }

            // Step 4 (Grand Finale): Spawn SymphonyBurst + ResonanceField at 86%
            if (currentStep == 4 && !hasSpawnedSecondary && progress >= 0.86f)
            {
                hasSpawnedSecondary = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.2f, Volume = 1.3f }, tipWorld);

                    // Symphony burst — massive AoE explosion
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipWorld, Vector2.Zero,
                        ModContent.ProjectileType<SymphonyBurstProjectile>(),
                        (int)(Projectile.damage * 0.65f), Projectile.knockBack * 0.6f, Projectile.owner);

                    // Resonance field — persistent damage zone
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipWorld, Vector2.Zero,
                        ModContent.ProjectileType<ResonanceFieldProjectile>(),
                        (int)(Projectile.damage * 0.3f), 0f, Projectile.owner);
                }

                // Screen shake for grand finale
                if (player.TryGetModPlayer(out global::MagnumOpus.Content.LaCampanella.Debuffs.ScreenShakePlayer shakePlayer))
                    shakePlayer.AddShake(12f, 22);
            }

            Projectile.rotation = SwordRotation;
        }

        /// <summary>
        /// Spawns arcane-themed swing VFX: purple dust, gem sparkles, glow particles, and music notes.
        /// </summary>
        private void SpawnSwingParticles(Vector2 tipWorld, float progress)
        {
            // Purple torch dust — 2 per frame for dense trail
            for (int i = 0; i < 2; i++)
            {
                Vector2 dustPos = tipWorld + Main.rand.NextVector2Circular(10f, 10f);
                Vector2 dustVel = -SwordDirection * Main.rand.NextFloat(0.5f, 2f);
                Dust d = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch, dustVel, 0,
                    GetPaletteColor(progress), Main.rand.NextFloat(1.1f, 1.6f));
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // Gem sparkles (amethyst) — contrasting accents
            if (Main.rand.NextBool(2))
            {
                Dust gem = Dust.NewDustPerfect(tipWorld + Main.rand.NextVector2Circular(12f, 12f),
                    DustID.GemAmethyst, -SwordDirection * 0.8f, 0, Color.White, 0.9f);
                gem.noGravity = true;
            }

            // Glow trail particles
            if (Main.rand.NextBool(3))
            {
                Color glowColor = GetPaletteColor(progress) * 0.6f;
                var glow = new GenericGlowParticle(tipWorld, -SwordDirection * 0.5f,
                    glowColor, Main.rand.NextFloat(0.15f, 0.3f), Main.rand.Next(8, 14), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Music note particles — hue-shifting notes orbit the blade trail
            if (Main.rand.NextBool(4))
            {
                Vector2 noteVel = -SwordDirection * 0.3f + Main.rand.NextVector2Circular(0.8f, 0.8f);
                // Arcane hue range: purple to blue (0.7 to 0.85)
                var musicNote = new HueShiftingMusicNoteParticle(
                    tipWorld + Main.rand.NextVector2Circular(8f, 8f),
                    noteVel,
                    hueMin: 0.7f, hueMax: 0.85f,
                    saturation: 0.9f, luminosity: 0.7f,
                    scale: Main.rand.NextFloat(0.7f, 1.0f),
                    lifetime: Main.rand.Next(20, 35),
                    hueSpeed: 0.025f);
                MagnumParticleHandler.SpawnParticle(musicNote);
            }

            // Extra glow sparks on steps 3-4 for escalating intensity
            if (currentStep >= 3 && Main.rand.NextBool(3))
            {
                Vector2 sparkVel = -SwordDirection * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Color sparkColor = Color.Lerp(new Color(180, 120, 240), new Color(255, 220, 255), Main.rand.NextFloat());
                var spark = new GlowSparkParticle(tipWorld, sparkVel, sparkColor,
                    Main.rand.NextFloat(0.12f, 0.22f), Main.rand.Next(6, 12));
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.dedServ) return;

            // Arcane sparks on hit
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(5f, 5f);
                Color sparkColor = Color.Lerp(new Color(100, 50, 200), new Color(200, 160, 255), Main.rand.NextFloat());
                var spark = new GlowSparkParticle(target.Center, sparkVel, sparkColor,
                    Main.rand.NextFloat(0.15f, 0.3f), Main.rand.Next(8, 14));
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Purple dust burst
            for (int i = 0; i < 5; i++)
            {
                Dust d = Dust.NewDustPerfect(target.Center, DustID.PurpleTorch,
                    Main.rand.NextVector2Circular(5f, 5f), 0, default, 1.3f);
                d.noGravity = true;
            }

            // Music note on hit (visible scale)
            if (Main.rand.NextBool(2))
            {
                var hitNote = new HueShiftingMusicNoteParticle(
                    target.Center, Main.rand.NextVector2Circular(2f, 2f),
                    0.7f, 0.85f, 0.9f, 0.75f,
                    Main.rand.NextFloat(0.8f, 1.1f), Main.rand.Next(18, 28));
                MagnumParticleHandler.SpawnParticle(hitNote);
            }

            // Bloom ring on finisher steps (3 and 4)
            if (currentStep >= 3)
            {
                var ring = new BloomRingParticle(target.Center, Vector2.Zero, new Color(160, 100, 240), 0.6f, 14);
                MagnumParticleHandler.SpawnParticle(ring);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ || SwingTime <= 0) return false;

            SpriteBatch sb = Main.spriteBatch;
            Player player = Owner;
            Vector2 mountedCenter = player.RotatedRelativePoint(player.MountedCenter, true);

            // Build ordered trail arrays from circular buffer
            Vector2[] orderedPositions = new Vector2[TrailLength];
            for (int i = 0; i < TrailLength; i++)
            {
                int idx = (trailIndex - i + TrailLength * 2) % TrailLength;
                orderedPositions[i] = tipPositions[idx];
            }

            // Arcane trail via EnhancedTrailRenderer (RenderMultiPassTrail)
            float trailWidth = ActivePhase.BladeLength * 0.22f;
            try
            {
                EnhancedTrailRenderer.RenderMultiPassTrail(
                    orderedPositions,
                    widthFunc: EnhancedTrailRenderer.LinearTaper(trailWidth),
                    colorFunc: completionRatio =>
                    {
                        float fade = 1f - completionRatio;
                        return GetPaletteColor(completionRatio) * fade * 0.85f;
                    },
                    bloomMultiplier: 2.8f,
                    coreMultiplier: 0.35f);
            }
            catch { }

            // Blade glow rendering
            Texture2D bladeTex = TextureAssets.Projectile[Type].Value;
            Vector2 origin = new Vector2(0, bladeTex.Height * 0.5f);
            float rot = SwordRotation;
            float bladeScale = ActivePhase.BladeLength / bladeTex.Width * Projectile.scale;
            SpriteEffects flip = Direction < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            Vector2 drawPos = mountedCenter - Main.screenPosition;

            SwingShaderSystem.BeginAdditive(sb);

            // Outer arcane glow
            Color outerGlow = new Color(80, 40, 180, 0) * 0.3f;
            sb.Draw(bladeTex, drawPos, null, outerGlow, rot, origin, bladeScale * 1.18f, flip, 0f);

            // Inner arcane glow
            Color innerGlow = new Color(180, 140, 240, 0) * 0.5f;
            sb.Draw(bladeTex, drawPos, null, innerGlow, rot, origin, bladeScale, flip, 0f);

            // Lens flare at blade tip — musical pulse
            float progress = Progression;
            float flareIntensity = (float)Math.Sin(progress * MathHelper.Pi) * 0.75f;
            if (flareIntensity > 0.05f)
            {
                Texture2D flareTex = TextureAssets.Extra[98].Value;
                Vector2 tipScreen = (mountedCenter + SwordDirection * ActivePhase.BladeLength * Projectile.scale) - Main.screenPosition;
                Vector2 flareOrigin = flareTex.Size() * 0.5f;
                float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.14f;
                Color flareColor = GetPaletteColor(progress) with { A = 0 } * flareIntensity;

                // Spinning flare layer
                sb.Draw(flareTex, tipScreen, null, flareColor * 0.55f, Main.GameUpdateCount * 0.04f,
                    flareOrigin, 0.38f * pulse, SpriteEffects.None, 0f);
                // Counter-spinning secondary layer
                sb.Draw(flareTex, tipScreen, null, new Color(200, 180, 255, 0) * flareIntensity * 0.3f,
                    -Main.GameUpdateCount * 0.025f, flareOrigin, 0.22f * pulse, SpriteEffects.None, 0f);
                // White core
                sb.Draw(flareTex, tipScreen, null, new Color(255, 255, 255, 0) * flareIntensity * 0.4f, 0f,
                    flareOrigin, 0.12f * pulse, SpriteEffects.None, 0f);
            }

            SwingShaderSystem.RestoreSpriteBatch(sb);

            return false;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(currentStep);
            writer.Write(hasSpawnedSecondary);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            currentStep = reader.ReadInt32();
            hasSpawnedSecondary = reader.ReadBoolean();
        }
    }
}
