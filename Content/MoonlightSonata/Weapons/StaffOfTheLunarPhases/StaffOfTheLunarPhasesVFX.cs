using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Common.Systems.VFX.Optimization;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.MoonlightSonata;
using MagnumOpus.Content.MoonlightSonata.Dusts;
using MagnumOpus.Content.MoonlightSonata.Minions;
using MagnumOpus.Content.SandboxLastPrism.Systems;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.StaffOfTheLunarPhases
{
    /// <summary>
    /// Overhauled VFX helper for Staff of the Lunar Phases — "The Conductor's Baton".
    /// Since this staff summons the Goliath of Moonlight, its visual identity channels
    /// cosmic gravitational energy — spiraling gravity particles, orbiting crescents,
    /// and a summoning ritual that tears open a gravitational rift.
    ///
    /// Shader Integration:
    ///   GravitationalRift.fx — Spiral vortex aura during Conductor Mode hold
    ///   SummonCircle.fx     — Rotating sigil during summoning ritual
    ///
    /// Visual Identity: Cosmic conductor — channeling gravitational forces through a baton.
    /// - GravityWellDust: Spiraling particles drawn toward the staff/player (gravitational focus)
    /// - LunarMote: Orbiting crescent moons showing the lunar cycle
    /// - StarPointDust: Bright star-core sparks crackling from the baton tip
    /// - ResonantPulseDust: Expanding gravitational shockwaves on summon
    ///
    /// All colors reference GoliathVFX cosmic palette for visual consistency
    /// with the summoned Goliath of Moonlight.
    /// </summary>
    public static class StaffOfTheLunarPhasesVFX
    {
        // Per-weapon accent colors — conductor's baton theme layered over cosmic palette
        public static readonly Color BatonGlowColor = new Color(190, 160, 255);
        private static readonly Color RitualFlash = new Color(220, 200, 255);
        private static readonly Color PhaseShift = new Color(160, 120, 240);
        private static readonly Color ConductorAura = new Color(140, 100, 220);

        // =====================================================================
        //  HOLD ITEM VFX — gravitational focus, orbiting crescents, star sparks
        // =====================================================================

        /// <summary>
        /// Per-frame VFX while the staff is held. Produces:
        /// - GravityWellDust spiraling toward the player (gravitational focus point)
        /// - LunarMote orbiting crescents (3 moons in slow orbit — lunar phases)
        /// - StarPointDust baton-tip sparks (cosmic energy discharge)
        /// - Music note orbits (conductor's motif)
        /// - Prismatic sparkle aura (cosmic shimmer)
        /// - GravitationalRift shader aura when Conductor Mode is active
        /// </summary>
        public static void HoldItemVFX(Player player, bool conductorMode)
        {
            if (Main.dedServ) return;

            float time = Main.GameUpdateCount * 0.04f;

            // GravityWellDust — spiraling toward player (gravitational focus point)
            if (Main.GameUpdateCount % 5 == 0)
            {
                float spawnAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                float spawnRadius = 38f + Main.rand.NextFloat(15f);
                Vector2 spawnPos = player.Center + spawnAngle.ToRotationVector2() * spawnRadius;
                Color wellColor = Color.Lerp(GoliathVFX.CosmicVoid, GoliathVFX.GravityWell, Main.rand.NextFloat(0.7f));
                Dust well = Dust.NewDustPerfect(spawnPos,
                    ModContent.DustType<GravityWellDust>(),
                    Vector2.Zero, 0, wellColor, 0.18f);
                well.customData = new GravityWellBehavior
                {
                    GravityCenter = player.Center,
                    PullStrength = 0.05f,
                    SpiralSpeed = 0.3f,
                    BaseScale = 0.18f,
                    Lifetime = 26,
                    VelocityDecay = 0.97f
                };
            }

            // LunarMote — 3 orbiting crescent moons (the lunar phases)
            if (Main.rand.NextBool(10))
            {
                float orbitAngle = time * 0.8f;
                for (int i = 0; i < 3; i++)
                {
                    float moteAngle = orbitAngle + MathHelper.TwoPi * i / 3f;
                    float radius = 26f + MathF.Sin(Main.GameUpdateCount * 0.03f + i * 0.9f) * 5f;
                    Color moteColor = Color.Lerp(GoliathVFX.EnergyTendril, GoliathVFX.StarCore, (float)i / 3f);
                    Dust mote = Dust.NewDustPerfect(player.Center,
                        ModContent.DustType<LunarMote>(),
                        Vector2.Zero, 0, moteColor, 0.22f);
                    mote.customData = new LunarMoteBehavior(player.Center, moteAngle)
                    {
                        OrbitRadius = radius,
                        OrbitSpeed = 0.025f,
                        Lifetime = 28,
                        FadePower = 0.93f
                    };
                }
            }

            // StarPointDust — baton tip sparks (cosmic energy crackling)
            if (Main.rand.NextBool(6))
            {
                Vector2 tipOffset = new Vector2(player.direction * 16f, -18f);
                Vector2 tipPos = player.Center + tipOffset;
                Color sparkColor = Color.Lerp(GoliathVFX.NebulaPurple, GoliathVFX.StarCore, Main.rand.NextFloat(0.5f));
                Dust star = Dust.NewDustPerfect(tipPos + Main.rand.NextVector2Circular(4f, 4f),
                    ModContent.DustType<StarPointDust>(),
                    new Vector2(0, -0.4f) + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    0, sparkColor, 0.15f);
                star.customData = new StarPointBehavior
                {
                    RotationSpeed = 0.08f,
                    TwinkleFrequency = 0.4f,
                    Lifetime = 22,
                    FadeStartTime = 8
                };
            }

            // Prismatic sparkle aura — cosmic shimmer
            if (Main.rand.NextBool(5))
            {
                Vector2 offset = Main.rand.NextVector2Circular(28f, 28f);
                Color gradientColor = Color.Lerp(GoliathVFX.NebulaPurple, GoliathVFX.EnergyTendril, Main.rand.NextFloat());
                CustomParticles.PrismaticSparkle(player.Center + offset, gradientColor * 0.6f, 0.22f);

                var sparkle = new SparkleParticle(player.Center + offset, Main.rand.NextVector2Circular(0.5f, 0.5f),
                    (GoliathVFX.StarCore with { A = 0 }) * 0.4f, 0.18f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Conductor's baton orbiting music notes
            if (Main.rand.NextBool(8))
            {
                float noteOrbit = Main.GameUpdateCount * 0.06f;
                for (int i = 0; i < 2; i++)
                {
                    float noteAngle = noteOrbit + MathHelper.Pi * i;
                    Vector2 notePos = player.Center + noteAngle.ToRotationVector2() * 22f;
                    MoonlightVFXLibrary.SpawnMusicNotes(notePos, 1, 2f, 0.75f, 0.9f, 35);

                    CustomParticles.PrismaticSparkle(notePos + Main.rand.NextVector2Circular(4f, 4f),
                        (GoliathVFX.EnergyTendril with { A = 0 }) * 0.4f, 0.15f);
                }
            }

            // === CONDUCTOR MODE — Enhanced aura with shader-driven gravitational rift ===
            if (conductorMode)
            {
                // Additional GravityWellDust — denser spiral around baton tip
                if (Main.GameUpdateCount % 3 == 0)
                {
                    Vector2 tipPos = player.Center + new Vector2(player.direction * 16f, -18f);
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float radius = 15f + Main.rand.NextFloat(10f);
                    Vector2 spawnPos = tipPos + angle.ToRotationVector2() * radius;
                    Color conductColor = Color.Lerp(ConductorAura, GoliathVFX.StarCore, Main.rand.NextFloat(0.4f));
                    Dust well = Dust.NewDustPerfect(spawnPos,
                        ModContent.DustType<GravityWellDust>(),
                        Vector2.Zero, 0, conductColor, 0.2f);
                    well.customData = new GravityWellBehavior
                    {
                        GravityCenter = tipPos,
                        PullStrength = 0.1f,
                        SpiralSpeed = 0.5f,
                        BaseScale = 0.2f,
                        Lifetime = 18,
                        VelocityDecay = 0.96f
                    };
                }

                // ResonantPulseDust conductor pulse — rhythmic waves from baton
                if (Main.GameUpdateCount % 30 == 0)
                {
                    Vector2 tipPos = player.Center + new Vector2(player.direction * 16f, -18f);
                    Color pulseColor = Color.Lerp(GoliathVFX.GravityWell, BatonGlowColor, 0.5f);
                    Dust pulse = Dust.NewDustPerfect(tipPos,
                        ModContent.DustType<ResonantPulseDust>(),
                        Vector2.Zero, 0, pulseColor, 0.2f);
                    pulse.customData = new ResonantPulseBehavior
                    {
                        ExpansionRate = 0.035f,
                        ExpansionDecay = 0.94f,
                        Lifetime = 16,
                        PulseFrequency = 0.3f
                    };
                }

                // Extra StarPointDust — conductor energy discharge
                if (Main.rand.NextBool(4))
                {
                    Vector2 tipPos = player.Center + new Vector2(player.direction * 16f, -18f);
                    Vector2 toCursor = (Main.MouseWorld - tipPos).SafeNormalize(Vector2.UnitY);
                    Color arcColor = Color.Lerp(BatonGlowColor, GoliathVFX.StarCore, Main.rand.NextFloat(0.5f));
                    Dust star = Dust.NewDustPerfect(tipPos + Main.rand.NextVector2Circular(6f, 6f),
                        ModContent.DustType<StarPointDust>(),
                        toCursor * Main.rand.NextFloat(1f, 3f),
                        0, arcColor, 0.18f);
                    star.customData = new StarPointBehavior
                    {
                        RotationSpeed = 0.12f,
                        TwinkleFrequency = 0.6f,
                        Lifetime = 16,
                        FadeStartTime = 5
                    };
                }

                // Conductor mode glow — brighter, more intense than standard
                float conductorPulse = MathF.Sin(Main.GameUpdateCount * 0.08f) * 0.2f + 1.1f;
                Lighting.AddLight(player.Center, BatonGlowColor.ToVector3() * conductorPulse * 0.6f);
            }

            // Cosmic gravitational glow — pulsing presence
            float pulse = MathF.Sin(Main.GameUpdateCount * 0.05f) * 0.15f + 0.95f;
            Color lightColor = Color.Lerp(GoliathVFX.GravityWell, GoliathVFX.NebulaPurple,
                MathF.Sin(Main.GameUpdateCount * 0.03f) * 0.5f + 0.5f);
            Lighting.AddLight(player.Center, lightColor.ToVector3() * pulse * 0.55f);
        }

        // =====================================================================
        //  CONDUCTOR MODE TOGGLE VFX
        // =====================================================================

        /// <summary>
        /// VFX burst when toggling Conductor Mode on/off.
        /// ON:  Gravitational implosion — particles converge, then pulse outward.
        /// OFF: Dispersal burst — particles scatter.
        /// </summary>
        public static void ConductorModeToggleVFX(Vector2 batonTip, bool turningOn)
        {
            if (Main.dedServ) return;

            if (turningOn)
            {
                // Gravitational convergence flash
                CustomParticles.GenericFlare(batonTip, BatonGlowColor, 0.7f, 18);
                CustomParticles.GenericFlare(batonTip, GoliathVFX.StarCore, 0.5f, 16);

                // GravityWellDust converging inward — rift forming at baton tip
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 startPos = batonTip + angle.ToRotationVector2() * 35f;
                    Color wellColor = Color.Lerp(GoliathVFX.GravityWell, ConductorAura, (float)i / 8f);
                    Dust well = Dust.NewDustPerfect(startPos,
                        ModContent.DustType<GravityWellDust>(),
                        Vector2.Zero, 0, wellColor, 0.25f);
                    well.customData = new GravityWellBehavior
                    {
                        GravityCenter = batonTip,
                        PullStrength = 0.15f,
                        SpiralSpeed = 0.6f,
                        BaseScale = 0.25f,
                        Lifetime = 20,
                        VelocityDecay = 0.95f
                    };
                }

                // StarPointDust starburst — activation sparks
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                    Color sparkColor = Color.Lerp(GoliathVFX.StarCore, BatonGlowColor, Main.rand.NextFloat());
                    Dust star = Dust.NewDustPerfect(batonTip,
                        ModContent.DustType<StarPointDust>(),
                        sparkVel, 0, sparkColor, 0.2f);
                    star.customData = new StarPointBehavior
                    {
                        RotationSpeed = 0.15f,
                        TwinkleFrequency = 0.5f,
                        Lifetime = 16,
                        FadeStartTime = 4
                    };
                }

                // ResonantPulseDust activation ring
                Dust activePulse = Dust.NewDustPerfect(batonTip,
                    ModContent.DustType<ResonantPulseDust>(),
                    Vector2.Zero, 0, BatonGlowColor, 0.22f);
                activePulse.customData = new ResonantPulseBehavior
                {
                    ExpansionRate = 0.04f,
                    ExpansionDecay = 0.93f,
                    Lifetime = 16,
                    PulseFrequency = 0.25f
                };

                // Music notes — conductor beginning the piece
                MoonlightVFXLibrary.SpawnMusicNotes(batonTip, 4, 25f, 0.8f, 1.0f, 30);
            }
            else
            {
                // Dispersal burst — conductor lowering baton
                CustomParticles.GenericFlare(batonTip, PhaseShift, 0.4f, 14);

                // GravityWellDust scattering outward — rift closing
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                    Color wellColor = Color.Lerp(GoliathVFX.GravityWell, GoliathVFX.CosmicVoid, Main.rand.NextFloat());
                    Dust well = Dust.NewDustPerfect(batonTip,
                        ModContent.DustType<GravityWellDust>(),
                        vel, 0, wellColor, 0.18f);
                    well.customData = new GravityWellBehavior
                    {
                        GravityCenter = Vector2.Zero,
                        PullStrength = 0f,
                        BaseScale = 0.18f,
                        Lifetime = 18,
                        VelocityDecay = 0.95f
                    };
                }

                // Fading music notes
                MoonlightVFXLibrary.SpawnMusicNotes(batonTip, 2, 15f, 0.6f, 0.8f, 25);
            }

            Lighting.AddLight(batonTip, BatonGlowColor.ToVector3() * 0.8f);
        }

        // =====================================================================
        //  PREDRAW BLOOM — 5-layer {A=0} bloom for world item rendering
        // =====================================================================

        /// <summary>
        /// 5-layer PreDrawInWorld bloom for the staff item lying in the world.
        /// Uses {A=0} alpha trick for additive rendering under AlphaBlend.
        /// Cosmic palette — outer void → event horizon → nebula → gravity well → star core.
        /// </summary>
        public static void DrawWorldItemBloom(SpriteBatch sb, Texture2D texture,
            Vector2 position, Vector2 origin, float rotation, float scale)
        {
            float pulse = 1f + MathF.Sin(Main.GameUpdateCount * 0.04f) * 0.12f;

            // Layer 1: Outer cosmic void aura (massive, barely visible)
            sb.Draw(texture, position, null,
                (GoliathVFX.CosmicVoid with { A = 0 }) * 0.20f,
                rotation, origin, scale * pulse * 1.45f, SpriteEffects.None, 0f);

            // Layer 2: Event horizon ring
            sb.Draw(texture, position, null,
                (GoliathVFX.EventHorizon with { A = 0 }) * 0.25f,
                rotation, origin, scale * pulse * 1.30f, SpriteEffects.None, 0f);

            // Layer 3: Mid nebula purple glow
            sb.Draw(texture, position, null,
                (GoliathVFX.NebulaPurple with { A = 0 }) * 0.30f,
                rotation, origin, scale * pulse * 1.15f, SpriteEffects.None, 0f);

            // Layer 4: Inner gravity well
            sb.Draw(texture, position, null,
                (GoliathVFX.GravityWell with { A = 0 }) * 0.35f,
                rotation, origin, scale * pulse * 1.05f, SpriteEffects.None, 0f);

            // Layer 5: Star core center
            sb.Draw(texture, position, null,
                (GoliathVFX.StarCore with { A = 0 }) * 0.22f,
                rotation, origin, scale * pulse, SpriteEffects.None, 0f);

            Lighting.AddLight(position + Main.screenPosition, GoliathVFX.NebulaPurple.ToVector3() * 0.45f);
        }

        // =====================================================================
        //  SUMMONING RITUAL VFX — cosmic rift when Goliath is summoned
        // =====================================================================

        /// <summary>
        /// Grand summoning ritual VFX at the spawn position.
        /// Tears open a gravitational rift — GravityWellDust imploding spiral,
        /// then explosive outward burst with ResonantPulseDust shockwaves,
        /// StarPointDust radial sparks, LunarMote crescent ejection,
        /// GodRay burst, screen distortion, chromatic aberration.
        ///
        /// Shader Integration: SummonCircle shader overlay during the ritual
        /// for a rotating lunar phase sigil at the summoning point.
        /// </summary>
        public static void SummoningRitualVFX(Vector2 position)
        {
            if (Main.dedServ) return;

            // === Central flash cascade — cosmic flare ===
            CustomParticles.GenericFlare(position, Color.White, 1.1f, 22);
            CustomParticles.GenericFlare(position, GoliathVFX.StarCore, 0.9f, 20);
            CustomParticles.GenericFlare(position, GoliathVFX.NebulaPurple, 0.7f, 18);
            CustomParticles.GenericFlare(position, GoliathVFX.CosmicVoid, 0.5f, 24);

            // === Magic circle — 6 GravityWellDust glyph points converging ===
            float magicCircleAngle = Main.GameUpdateCount * 0.05f;
            for (int i = 0; i < 6; i++)
            {
                float glyphAngle = magicCircleAngle + MathHelper.TwoPi * i / 6f;
                Vector2 glyphPos = position + glyphAngle.ToRotationVector2() * 55f;
                Color glyphColor = Color.Lerp(GoliathVFX.GravityWell, GoliathVFX.EnergyTendril, (float)i / 6f);

                // Glyph flare at each summoning circle point
                CustomParticles.GenericFlare(glyphPos, glyphColor, 0.45f, 18);

                // GravityWellDust converging from glyph points toward center
                Dust well = Dust.NewDustPerfect(glyphPos,
                    ModContent.DustType<GravityWellDust>(),
                    Vector2.Zero, 0, glyphColor, 0.25f);
                well.customData = new GravityWellBehavior
                {
                    GravityCenter = position,
                    PullStrength = 0.12f,
                    SpiralSpeed = 0.5f,
                    BaseScale = 0.25f,
                    Lifetime = 24,
                    VelocityDecay = 0.95f
                };
            }

            // === 8 lunar phase symbols — LunarMote crescents at cardinal+ordinal positions ===
            for (int i = 0; i < 8; i++)
            {
                float phaseAngle = MathHelper.TwoPi * i / 8f;
                Vector2 phasePos = position + phaseAngle.ToRotationVector2() * 38f;
                float progress = (float)i / 8f;
                Color phaseColor = Color.Lerp(GoliathVFX.EnergyTendril, GoliathVFX.StarCore, progress);

                // Each moon phase as a LunarMote orbiting outward
                Dust mote = Dust.NewDustPerfect(phasePos,
                    ModContent.DustType<LunarMote>(),
                    phaseAngle.ToRotationVector2() * 1.5f,
                    0, phaseColor, 0.3f);
                mote.customData = new LunarMoteBehavior(position, phaseAngle)
                {
                    OrbitRadius = 38f + i * 4f,
                    OrbitSpeed = 0.05f + i * 0.005f,
                    Lifetime = 28,
                    FadePower = 0.92f
                };
            }

            // === GravityWellDust radial explosion — rift opening outward ===
            for (int i = 0; i < 14; i++)
            {
                float angle = MathHelper.TwoPi * i / 14f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 10f);
                Color wellColor = GoliathVFX.GetCosmicColor((float)i / 14f, 0.8f);
                Dust well = Dust.NewDustPerfect(position,
                    ModContent.DustType<GravityWellDust>(),
                    vel, 0, wellColor, 0.3f);
                well.customData = new GravityWellBehavior
                {
                    GravityCenter = Vector2.Zero,
                    PullStrength = 0f,
                    SpiralSpeed = 0f,
                    BaseScale = 0.3f,
                    Lifetime = 26,
                    VelocityDecay = 0.94f
                };
            }

            // === StarPointDust radial spark burst — cosmic energy discharge ===
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 9f);
                Color sparkColor = Color.Lerp(GoliathVFX.StarCore, GoliathVFX.EnergyTendril, (float)i / 10f);
                Dust star = Dust.NewDustPerfect(position,
                    ModContent.DustType<StarPointDust>(),
                    sparkVel, 0, sparkColor, 0.22f);
                star.customData = new StarPointBehavior
                {
                    RotationSpeed = 0.14f,
                    TwinkleFrequency = 0.5f,
                    Lifetime = 20,
                    FadeStartTime = 6
                };
            }

            // === ResonantPulseDust gravitational shockwaves — 3 expanding rings ===
            for (int ring = 0; ring < 3; ring++)
            {
                float ringProgress = ring / 3f;
                Color ringColor = Color.Lerp(GoliathVFX.EventHorizon, GoliathVFX.NebulaPurple, ringProgress);
                Dust pulse = Dust.NewDustPerfect(position,
                    ModContent.DustType<ResonantPulseDust>(),
                    Vector2.Zero, 0, ringColor,
                    0.28f + ring * 0.07f);
                pulse.customData = new ResonantPulseBehavior
                {
                    ExpansionRate = 0.045f + ring * 0.012f,
                    ExpansionDecay = 0.93f,
                    Lifetime = 18 + ring * 4,
                    PulseFrequency = 0.2f
                };
            }

            // === Halo ring cascade — cosmic halos ===
            for (int ring = 0; ring < 4; ring++)
            {
                Color ringColor = Color.Lerp(GoliathVFX.GravityWell, GoliathVFX.StarCore, ring / 4f);
                CustomParticles.HaloRing(position, ringColor, 0.5f + ring * 0.18f, 20 + ring * 5);
            }

            // === GodRay burst — summoning rift opening ===
            GodRaySystem.CreateBurst(position, GoliathVFX.NebulaPurple, 8, 80f, 25,
                GodRaySystem.GodRayStyle.Explosion, GoliathVFX.StarCore);

            // === Screen effects ===
            if (AdaptiveQualityManager.Instance?.CurrentQuality >= AdaptiveQualityManager.QualityLevel.Medium)
            {
                ScreenDistortionManager.TriggerRipple(position, GoliathVFX.NebulaPurple, 0.5f, 22);
                MagnumScreenEffects.AddScreenShake(3f);
            }

            // === Chromatic aberration — rift tearing space ===
            try
            {
                SLPFlashSystem.SetCAFlashEffect(
                    intensity: 0.15f,
                    lifetime: 12,
                    whiteIntensity: 0.4f,
                    distanceMult: 0.3f,
                    moveIn: true);
            }
            catch { }

            // === Music notes — the summoning song ===
            MoonlightVFXLibrary.SpawnMusicNotes(position, 8, 50f, 0.8f, 1.1f, 35);

            // Intense light
            Lighting.AddLight(position, GoliathVFX.StarCore.ToVector3() * 1.5f);
        }

        // =====================================================================
        //  SHADER OVERLAY RENDERING — GravitationalRift + SummonCircle
        // =====================================================================

        /// <summary>
        /// Draws a GravitationalRift shader overlay around the specified world position.
        /// Used during Conductor Mode hold and Goliath beam charging.
        /// Call within an Immediate-mode SpriteBatch (use BeginShaderBatch/RestoreDefaultBatch).
        /// </summary>
        public static void DrawGravitationalRiftOverlay(SpriteBatch sb, Vector2 worldPos, float riftPhase, float scale = 1f)
        {
            if (sb == null || !MoonlightSonataShaderManager.HasGravitationalRift) return;

            var bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex == null) return;

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = bloomTex.Size() * 0.5f;
            float time = Main.GlobalTimeWrappedHourly;

            // Bind noise texture for enhanced distortion
            MoonlightSonataShaderManager.BindCosmicNoiseTexture(Main.graphics.GraphicsDevice);

            // Apply shader
            MoonlightSonataShaderManager.ApplyGoliathGravitationalRift(time, riftPhase);

            // Draw the shader-processed overlay
            sb.Draw(bloomTex, drawPos, null, Color.White, 0f, origin, 2.5f * scale, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draws a SummonCircle shader overlay at the specified world position.
        /// Used during the summoning ritual sequence.
        /// Call within an Immediate-mode SpriteBatch.
        /// </summary>
        public static void DrawSummonCircleOverlay(SpriteBatch sb, Vector2 worldPos, float ritualPhase, float scale = 1f)
        {
            if (sb == null || !MoonlightSonataShaderManager.HasSummonCircle) return;

            var bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex == null) return;

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = bloomTex.Size() * 0.5f;
            float time = Main.GlobalTimeWrappedHourly;

            // Bind noise texture
            MoonlightSonataShaderManager.BindCosmicNoiseTexture(Main.graphics.GraphicsDevice);

            // Apply shader
            MoonlightSonataShaderManager.ApplyGoliathSummonCircle(time, ritualPhase);

            // Draw the shader-processed overlay
            sb.Draw(bloomTex, drawPos, null, Color.White, 0f, origin, 3f * scale, SpriteEffects.None, 0f);
        }
    }
}
