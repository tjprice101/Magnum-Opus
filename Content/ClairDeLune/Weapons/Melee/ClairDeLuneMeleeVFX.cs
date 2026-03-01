using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.ClairDeLune.Weapons.Melee
{
    // =========================================================================
    //  ChronologicalityVFX — Temporal Drill
    //  Shader: TemporalDrill.fx (TemporalDrillBore + TemporalDrillGlow)
    //  Identity: Time-rip spiraling bore — the drill tears through reality
    //  itself, leaving crimson temporal fractures in its wake.
    // =========================================================================
    public static class ChronologicalityVFX
    {
        public static void HoldItemVFX(Player player)
        {
            float time = (float)Main.timeForVisualEffects;

            // Converging temporal motes — particles spiral INWARD toward the player's hand
            if (Main.rand.NextBool(2))
            {
                float angle = time * 0.05f + Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = 25f + Main.rand.NextFloat(10f);
                Vector2 spawnPos = player.Center + angle.ToRotationVector2() * radius;
                Vector2 vel = (player.Center - spawnPos) * 0.06f; // Converge inward
                Dust d = Dust.NewDustPerfect(spawnPos, DustID.FireworkFountain_Red, vel, 0, default, 0.5f);
                d.noGravity = true;
                d.fadeIn = 0.6f;
            }

            // Orbiting clock-hand dots — two dots orbit the player at different speeds
            if (Main.rand.NextBool(3))
            {
                for (int i = 0; i < 2; i++)
                {
                    float orbitAngle = time * (0.035f + i * 0.015f) + i * MathHelper.Pi;
                    Vector2 orbitPos = player.Center + orbitAngle.ToRotationVector2() * (14f + i * 5f);
                    Color orbitCol = (i == 0) ? ClairDeLunePalette.TemporalCrimson : ClairDeLunePalette.PearlBlue;
                    var dot = new SparkleParticle(orbitPos, Vector2.Zero, orbitCol * 0.5f, 0.15f, 6);
                    MagnumParticleHandler.SpawnParticle(dot);
                }
            }

            if (Main.rand.NextBool(8))
            {
                Vector2 edgeOffset = Main.rand.NextVector2CircularEdge(18f, 18f);
                Dust pearl = Dust.NewDustPerfect(player.Center + edgeOffset, DustID.IceTorch,
                    edgeOffset * 0.015f, 0, default, 0.35f);
                pearl.noGravity = true;
            }

            ClairDeLuneVFXLibrary.AddClairDeLuneLight(player.Center, 0.25f);
        }

        public static void PreDrawInWorldBloom(SpriteBatch sb, Texture2D tex, Vector2 pos,
            Vector2 origin, float rotation, float scale)
        {
            if (ClairDeLuneShaderManager.HasTemporalDrill)
            {
                ClairDeLuneShaderManager.BeginShaderAdditive(sb);
                ClairDeLuneShaderManager.ApplyTemporalDrillGlow((float)Main.timeForVisualEffects * 0.03f);

                sb.Draw(tex, pos - Main.screenPosition, null,
                    ClairDeLunePalette.TemporalCrimson * 0.5f, rotation, origin, scale * 1.04f,
                    SpriteEffects.None, 0f);

                ClairDeLuneShaderManager.RestoreSpriteBatch(sb);
            }
            else
            {
                ClairDeLuneVFXLibrary.DrawClairDeLuneBloomStack(sb, pos,
                    ClairDeLunePalette.TemporalCrimson, ClairDeLunePalette.PearlWhite, scale * 0.3f, 0.4f);
            }
        }

        public static void DrillTrailVFX(Vector2 tipPos, Vector2 drillDirection)
        {
            Vector2 perpendicular = new Vector2(-drillDirection.Y, drillDirection.X);
            float time = (float)Main.timeForVisualEffects;

            // === SIGNATURE: SPIRALING TEMPORAL VORTEX ===
            // Particles orbit the drill axis in a tight helix, creating a bore-hole visual
            for (int i = 0; i < 3; i++)
            {
                float helixAngle = time * 0.15f + i * MathHelper.TwoPi / 3f;
                float helixRadius = 8f + (float)Math.Sin(time * 0.08f + i) * 3f;
                Vector2 helixOffset = perpendicular * (float)Math.Cos(helixAngle) * helixRadius
                    + Vector2.UnitY * (float)Math.Sin(helixAngle) * helixRadius * 0.5f;
                Vector2 helixVel = drillDirection * 2f + perpendicular * (float)Math.Sin(helixAngle) * 1.5f;

                // Alternating crimson-blue chromatic split (temporal rift appearance)
                Color vortexColor = (i % 2 == 0)
                    ? ClairDeLunePalette.PaletteLerp(ClairDeLunePalette.ChronologicalityBlade, 0.5f + (float)Math.Sin(time * 0.05f) * 0.2f)
                    : ClairDeLunePalette.TemporalCrimson;

                var vortexParticle = new GenericGlowParticle(
                    tipPos + helixOffset, helixVel,
                    vortexColor * 0.8f, 0.35f, 16, true);
                MagnumParticleHandler.SpawnParticle(vortexParticle);
            }

            // Core bore dust — dense corkscrew of crimson fire along drill axis
            Vector2 dustVel = perpendicular * Main.rand.NextFloat(-2f, 2f) + drillDirection * 2.5f;
            Dust d = Dust.NewDustPerfect(tipPos, DustID.FireworkFountain_Red, dustVel, 0, default, 0.9f);
            d.noGravity = true;
            d.fadeIn = 1.0f;

            // Temporal echo afterimage — fading ghost sparkle that persists briefly behind the tip
            if (Main.GameUpdateCount % 3 == 0)
            {
                Vector2 echoPos = tipPos - drillDirection * 20f;
                var echo = new SparkleParticle(
                    echoPos + Main.rand.NextVector2Circular(5f, 5f),
                    drillDirection * 0.3f,
                    ClairDeLunePalette.TemporalCrimson * 0.6f,
                    0.3f, 18);
                MagnumParticleHandler.SpawnParticle(echo);
            }

            // Pearl fracture sparks ejecting perpendicular — rift tear debris
            if (Main.GameUpdateCount % 2 == 0)
            {
                float ejectSide = Main.rand.NextBool() ? 1f : -1f;
                Vector2 ejectVel = perpendicular * ejectSide * Main.rand.NextFloat(2f, 4f)
                    + drillDirection * 0.5f;
                Dust pearl = Dust.NewDustPerfect(tipPos, DustID.IceTorch, ejectVel, 0, default, 0.55f);
                pearl.noGravity = true;
                pearl.fadeIn = 0.7f;
            }

            // Periodic music notes torn from the temporal wake
            if (Main.GameUpdateCount % 6 == 0)
                ClairDeLuneVFXLibrary.SpawnMusicNotes(tipPos, 1, 8f, 0.4f, 0.6f, 18);

            ClairDeLuneVFXLibrary.AddPaletteLighting(tipPos, 0.8f, 0.6f);
        }

        public static void DrillImpactVFX(Vector2 hitPos)
        {
            ClairDeLuneVFXLibrary.MeleeImpact(hitPos, 0);

            // === SIGNATURE: TEMPORAL RIFT SPIRAL BURST ===
            // Particles radiate outward in a SPIRAL pattern (not simple radial) — a corkscrew detonation
            int dustCount = 18;
            float spiralOffset = (float)Main.timeForVisualEffects * 0.12f;
            for (int i = 0; i < dustCount; i++)
            {
                float baseAngle = MathHelper.TwoPi * i / dustCount;
                float spiralAngle = baseAngle + spiralOffset + i * 0.15f; // Spiral twist
                float speed = 5f + Main.rand.NextFloat() * 4f;
                Vector2 vel = spiralAngle.ToRotationVector2() * speed;

                // Crimson-blue chromatic alternation
                bool isCrimson = (i % 3 == 0);
                Color impactColor = isCrimson
                    ? ClairDeLunePalette.TemporalCrimson
                    : ClairDeLunePalette.PaletteLerp(ClairDeLunePalette.ChronologicalityBlade, (float)i / dustCount);

                var burst = new GenericGlowParticle(hitPos, vel, impactColor * 0.9f, 0.4f, 18, true);
                MagnumParticleHandler.SpawnParticle(burst);

                // Inner dust ring
                int dustType = isCrimson ? DustID.FireworkFountain_Red : DustID.IceTorch;
                Dust d = Dust.NewDustPerfect(hitPos, dustType, vel * 0.7f, 0, default, 1.1f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // Temporal echo ring — fading concentric sparkle ripple
            for (int ring = 0; ring < 2; ring++)
            {
                float ringRadius = 15f + ring * 20f;
                int ringCount = 8 + ring * 4;
                for (int i = 0; i < ringCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / ringCount;
                    Vector2 ringPos = hitPos + angle.ToRotationVector2() * ringRadius;
                    var echo = new SparkleParticle(ringPos, angle.ToRotationVector2() * 2f,
                        ClairDeLunePalette.PearlWhite * (0.6f - ring * 0.2f), 0.25f, 14 + ring * 4);
                    MagnumParticleHandler.SpawnParticle(echo);
                }
            }

            ClairDeLuneVFXLibrary.SpawnPearlBurst(hitPos, 8, 4.5f, 0.3f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(hitPos, 3, 14f, 0.5f, 0.8f, 22);
            ClairDeLuneVFXLibrary.DrawBloom(hitPos, 0.5f, 0.8f);
            ClairDeLuneVFXLibrary.AddPaletteLighting(hitPos, 0.8f, 0.9f);
        }

        public static void CriticalDischargeVFX(Vector2 pos)
        {
            // === SIGNATURE: TEMPORAL OVERLOAD — reality cracks outward in a spiral shockwave ===
            float spiralSeed = (float)Main.timeForVisualEffects * 0.15f;

            // Double-spiral crimson detonation
            for (int arm = 0; arm < 2; arm++)
            {
                for (int i = 0; i < 12; i++)
                {
                    float angle = spiralSeed + arm * MathHelper.Pi + i * 0.35f;
                    float speed = 6f + i * 0.4f;
                    Vector2 vel = angle.ToRotationVector2() * speed;
                    Color col = Color.Lerp(ClairDeLunePalette.TemporalCrimson, ClairDeLunePalette.PearlWhite, (float)i / 12f);

                    var particle = new GenericGlowParticle(pos, vel, col * 0.9f, 0.45f, 20, true);
                    MagnumParticleHandler.SpawnParticle(particle);
                }
            }

            ClairDeLuneVFXLibrary.SpawnRadialDustBurst(pos, 20, 7f, DustID.FireworkFountain_Red);
            ClairDeLuneVFXLibrary.SpawnPearlBurst(pos, 12, 6f, 0.4f);
            ClairDeLuneVFXLibrary.SpawnGradientHaloRings(pos, 5, 0.45f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 5, 22f, 0.5f, 0.9f, 30);
            ClairDeLuneVFXLibrary.DrawBloom(pos, 0.9f, 1.1f);
            ClairDeLuneVFXLibrary.AddPaletteLighting(pos, 0.8f, 1.3f);
        }
    }

    // =========================================================================
    //  TemporalPiercerVFX — Crystal Lance
    //  Shader: CrystalLance.fx (CrystalLanceThrust + CrystalLanceShatter)
    //  Identity: Frost-crystal pierce — a frozen lance that crystallizes
    //  the air itself, shattering into ice shards on impact.
    // =========================================================================
    public static class TemporalPiercerVFX
    {
        public static void HoldItemVFX(Player player)
        {
            float time = (float)Main.timeForVisualEffects;

            // Frost crystal condensation — tiny angular sparkles forming near the lance like ice crystallizing
            if (Main.rand.NextBool(3))
            {
                // Hexagonal spawn pattern (6-fold symmetry)
                float hexAngle = (float)Math.Floor(Main.rand.NextFloat(6f)) * MathHelper.Pi / 3f;
                float hexDist = 15f + Main.rand.NextFloat(10f);
                Vector2 crystalPos = player.Center + hexAngle.ToRotationVector2() * hexDist;
                Color crystalCol = Color.Lerp(ClairDeLunePalette.MoonlitFrost, ClairDeLunePalette.PearlWhite,
                    Main.rand.NextFloat(0.3f, 0.8f));
                var crystal = new SparkleParticle(crystalPos, Vector2.UnitY * -0.3f,
                    crystalCol * 0.6f, 0.18f, 12);
                MagnumParticleHandler.SpawnParticle(crystal);
            }

            // Drifting frost motes — slow, gentle, cold
            if (Main.rand.NextBool(5))
            {
                Vector2 offset = Main.rand.NextVector2Circular(22f, 22f);
                Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.IceTorch,
                    Vector2.UnitY * -0.4f + Main.rand.NextVector2Circular(0.15f, 0.15f), 0, default, 0.4f);
                d.noGravity = true;
                d.fadeIn = 0.6f;
            }

            if (Main.rand.NextBool(10))
                ClairDeLuneVFXLibrary.SpawnPearlSparkle(player.Center, Main.rand.NextVector2Circular(1f, 1f));

            ClairDeLuneVFXLibrary.AddClairDeLuneLight(player.Center, 0.2f);
        }

        public static void PreDrawInWorldBloom(SpriteBatch sb, Texture2D tex, Vector2 pos,
            Vector2 origin, float rotation, float scale)
        {
            if (ClairDeLuneShaderManager.HasCrystalLance)
            {
                ClairDeLuneShaderManager.BeginShaderAdditive(sb);
                ClairDeLuneShaderManager.ApplyCrystalLanceShatter((float)Main.timeForVisualEffects * 0.03f);

                sb.Draw(tex, pos - Main.screenPosition, null,
                    ClairDeLunePalette.MoonlitFrost * 0.5f, rotation, origin, scale * 1.03f,
                    SpriteEffects.None, 0f);

                ClairDeLuneShaderManager.RestoreSpriteBatch(sb);
            }
            else
            {
                ClairDeLuneVFXLibrary.DrawClairDeLuneBloomStack(sb, pos,
                    ClairDeLunePalette.MoonlitFrost, ClairDeLunePalette.PearlBlue, scale * 0.25f, 0.35f);
            }
        }

        public static void ThrustTrailVFX(Vector2 tipPos, Vector2 thrustDir, bool isCharged)
        {
            float intensity = isCharged ? 1.5f : 1f;
            float time = (float)Main.timeForVisualEffects;
            Vector2 perpendicular = new Vector2(-thrustDir.Y, thrustDir.X);

            // === SIGNATURE: CRYSTALLINE FROST FACET FORMATION ===
            // Sharp angular particles form geometric crystal lattice along the thrust line
            for (int i = 0; i < (isCharged ? 3 : 2); i++)
            {
                // Angular facet: particles spawn at fixed angular offsets creating crystal geometry
                float facetAngle = (i * MathHelper.Pi / 3f) + time * 0.04f;
                float facetDist = 5f + (float)Math.Abs(Math.Sin(facetAngle * 3f)) * 6f;
                Vector2 facetOffset = perpendicular * (float)Math.Cos(facetAngle) * facetDist;
                Vector2 facetVel = thrustDir * (2f + Main.rand.NextFloat(1.5f)) * intensity;

                // Frost palette: moonlit frost -> pearl blue -> white
                Color crystalColor = ClairDeLunePalette.PaletteLerp(
                    ClairDeLunePalette.TemporalPiercerLance,
                    0.4f + (float)Math.Sin(time * 0.06f + i) * 0.2f);

                var facetParticle = new SparkleParticle(
                    tipPos + facetOffset, facetVel,
                    crystalColor * 0.85f, 0.28f * intensity, 14);
                MagnumParticleHandler.SpawnParticle(facetParticle);
            }

            // Core ice thrust — sharp concentrated line along thrust axis
            Vector2 dustVel = thrustDir * 3.5f * intensity + perpendicular * Main.rand.NextFloat(-0.3f, 0.3f);
            Dust d = Dust.NewDustPerfect(tipPos, DustID.IceTorch, dustVel, 0, default, 0.8f * intensity);
            d.noGravity = true;
            d.fadeIn = 0.9f;

            // Prismatic refraction sparkle at the lance tip — ice catching moonlight
            if (Main.GameUpdateCount % 2 == 0)
            {
                Color refractionCol = Color.Lerp(
                    ClairDeLunePalette.MoonlitFrost, ClairDeLunePalette.PearlWhite,
                    (float)Math.Sin(time * 0.1f) * 0.5f + 0.5f);
                var refract = new SparkleParticle(
                    tipPos + Main.rand.NextVector2Circular(3f, 3f),
                    thrustDir * 0.5f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    refractionCol, 0.22f * intensity, 10);
                MagnumParticleHandler.SpawnParticle(refract);
            }

            // Charged: extra frost crystal shards diverging outward
            if (isCharged && Main.GameUpdateCount % 2 == 0)
            {
                float shardSide = Main.rand.NextBool() ? 1f : -1f;
                Vector2 shardVel = perpendicular * shardSide * Main.rand.NextFloat(1.5f, 3f)
                    + thrustDir * 1f;
                Dust shard = Dust.NewDustPerfect(tipPos, DustID.GemDiamond, shardVel, 0, default, 0.5f);
                shard.noGravity = true;
                shard.fadeIn = 0.6f;

                ClairDeLuneVFXLibrary.SpawnPearlSparkle(tipPos, thrustDir);
            }

            if (Main.GameUpdateCount % 5 == 0)
                ClairDeLuneVFXLibrary.SpawnMusicNotes(tipPos, 1, 6f, 0.35f, 0.55f, 14);

            ClairDeLuneVFXLibrary.AddPaletteLighting(tipPos, 0.3f, 0.5f * intensity);
        }

        public static void ThrustImpactVFX(Vector2 hitPos, bool isCharged)
        {
            float intensity = isCharged ? 1.5f : 1f;
            ClairDeLuneVFXLibrary.MeleeImpact(hitPos, isCharged ? 2 : 0);

            // === SIGNATURE: ICE SHATTER BURST — geometric shard detonation ===
            // Shards radiate in angular patterns — like a crystal shattering on impact
            int shardCount = (int)(14 * intensity);
            for (int i = 0; i < shardCount; i++)
            {
                // Angular distribution — sharp 60° faceted pattern
                float baseAngle = MathHelper.TwoPi * i / shardCount;
                float facetSnap = (float)Math.Round(baseAngle / (MathHelper.Pi / 3f)) * (MathHelper.Pi / 3f);
                float angle = MathHelper.Lerp(baseAngle, facetSnap, 0.4f); // Partially snap to hexagonal grid
                float speed = (4f + Main.rand.NextFloat() * 3.5f) * intensity;
                Vector2 vel = angle.ToRotationVector2() * speed;

                // Crystal color — frost blue with occasional brilliant white shards
                Color shardCol = (i % 4 == 0)
                    ? ClairDeLunePalette.PearlWhite
                    : ClairDeLunePalette.PaletteLerp(ClairDeLunePalette.TemporalPiercerLance, (float)i / shardCount);

                // Crystal sparkle particles for the shards
                var shard = new SparkleParticle(hitPos, vel, shardCol * 0.9f, 0.3f * intensity, 16);
                MagnumParticleHandler.SpawnParticle(shard);

                // Underlying frost dust
                Dust d = Dust.NewDustPerfect(hitPos, DustID.IceTorch, vel * 0.8f, 0, default, 0.9f * intensity);
                d.noGravity = true;
                d.fadeIn = 1f;
            }

            // Hexagonal frost ring — 6-fold symmetry crystal pattern
            if (isCharged)
            {
                for (int i = 0; i < 6; i++)
                {
                    float hexAngle = MathHelper.TwoPi * i / 6f;
                    float hexRadius = 25f;
                    Vector2 hexPos = hitPos + hexAngle.ToRotationVector2() * hexRadius;
                    var hexSparkle = new SparkleParticle(
                        hexPos, hexAngle.ToRotationVector2() * 3f,
                        ClairDeLunePalette.MoonlitFrost, 0.35f, 20);
                    MagnumParticleHandler.SpawnParticle(hexSparkle);
                }

                ClairDeLuneVFXLibrary.SpawnGradientHaloRings(hitPos, 4, 0.4f);
                ClairDeLuneVFXLibrary.SpawnStarlitSparkles(hitPos, 8, 35f, 0.28f);
            }

            ClairDeLuneVFXLibrary.SpawnPearlBurst(hitPos, (int)(10 * intensity), 5f * intensity, 0.3f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(hitPos, 2 + (isCharged ? 2 : 0), 14f, 0.45f, 0.75f, 22);
            ClairDeLuneVFXLibrary.DrawBloom(hitPos, 0.4f * intensity, 0.8f);
            ClairDeLuneVFXLibrary.AddPaletteLighting(hitPos, 0.3f, 0.7f * intensity);
        }
    }

    // =========================================================================
    //  ClockworkHarmonyVFX — Gear Swing
    //  Shader: GearSwing.fx (GearSwingArc + GearSwingTrail)
    //  Identity: Music box pendulum — a grand brass pendulum that sweeps
    //  in rhythmic arcs, scattering gear-tooth sparks and golden notes.
    // =========================================================================
    public static class ClockworkHarmonyVFX
    {
        public static void HoldItemVFX(Player player)
        {
            float time = (float)Main.timeForVisualEffects;

            // Pendulum tick — a single brass spark that oscillates back and forth like a metronome
            float pendulumAngle = (float)Math.Sin(time * 0.04f) * 0.6f - MathHelper.PiOver2;
            float pendulumRadius = 18f;
            Vector2 pendulumPos = player.Center + pendulumAngle.ToRotationVector2() * pendulumRadius;
            if (Main.rand.NextBool(3))
            {
                var tick = new SparkleParticle(pendulumPos, Vector2.Zero,
                    ClairDeLunePalette.ClockworkBrass * 0.6f, 0.15f, 8);
                MagnumParticleHandler.SpawnParticle(tick);
            }

            // Warm brass ambient glow dust
            if (Main.rand.NextBool(5))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.GoldFlame,
                    Main.rand.NextVector2Circular(0.15f, 0.15f), 0, default, 0.35f);
                d.noGravity = true;
                d.fadeIn = 0.5f;
            }

            // Occasional music note drifting upward — music box idle
            if (Main.rand.NextBool(15))
            {
                var note = new HueShiftingMusicNoteParticle(
                    player.Center + Main.rand.NextVector2Circular(15f, 15f),
                    Vector2.UnitY * -0.8f + Main.rand.NextVector2Circular(0.3f, 0.3f),
                    0.10f, 0.15f, 0.6f, 0.55f,
                    Main.rand.NextFloat(0.4f, 0.6f), 30);
                MagnumParticleHandler.SpawnParticle(note);
            }

            if (Main.rand.NextBool(12))
                ClairDeLuneVFXLibrary.SpawnPearlShimmer(player.Center, 1, 20f, 0.15f);

            ClairDeLuneVFXLibrary.AddClairDeLuneLight(player.Center, 0.2f);
        }

        public static void PreDrawInWorldBloom(SpriteBatch sb, Texture2D tex, Vector2 pos,
            Vector2 origin, float rotation, float scale)
        {
            if (ClairDeLuneShaderManager.HasGearSwing)
            {
                ClairDeLuneShaderManager.BeginShaderAdditive(sb);
                ClairDeLuneShaderManager.ApplyGearSwingTrail((float)Main.timeForVisualEffects * 0.03f);

                sb.Draw(tex, pos - Main.screenPosition, null,
                    ClairDeLunePalette.ClockworkBrass * 0.5f, rotation, origin, scale * 1.03f,
                    SpriteEffects.None, 0f);

                ClairDeLuneShaderManager.RestoreSpriteBatch(sb);
            }
            else
            {
                ClairDeLuneVFXLibrary.DrawClairDeLuneBloomStack(sb, pos,
                    ClairDeLunePalette.ClockworkBrass, ClairDeLunePalette.MoonbeamGold, scale * 0.25f, 0.35f);
            }
        }

        public static void SwingTrailVFX(Vector2 tipPos, Vector2 swordDirection, int comboStep, int timer)
        {
            Vector2 perpendicular = new Vector2(-swordDirection.Y, swordDirection.X);
            float time = (float)Main.timeForVisualEffects;
            float comboIntensity = 1f + comboStep * 0.25f;

            // === SIGNATURE: PENDULUM ARC WITH INTERLOCKING GEAR-TOOTH SPARKS ===
            // The greatsword swings like a music box pendulum — wide, rhythmic, mechanical

            // Primary brass pendulum sweep — warm, wide, mechanical feeling
            for (int i = 0; i < 2; i++)
            {
                float arcSpread = Main.rand.NextFloat(-2f, 2f);
                Vector2 dustVel = perpendicular * arcSpread + swordDirection * (1.2f + i * 0.5f);
                Color brassGrad = ClairDeLunePalette.PaletteLerp(
                    ClairDeLunePalette.ClockworkHarmonyBlade,
                    0.3f + (float)Math.Sin(time * 0.04f + i) * 0.15f);
                var sweep = new GenericGlowParticle(
                    tipPos + Main.rand.NextVector2Circular(4f, 4f), dustVel,
                    brassGrad * 0.75f, 0.3f * comboIntensity, 16, true);
                MagnumParticleHandler.SpawnParticle(sweep);
            }

            // Gear-tooth sparkle notches — tiny angular sparks that create a sawtooth pattern along the arc
            if (timer % 2 == 0)
            {
                float toothPhase = time * 0.12f + timer * 0.5f;
                float toothSide = (float)Math.Sin(toothPhase) > 0 ? 1f : -1f;
                Vector2 toothVel = perpendicular * toothSide * 2.5f + swordDirection * 0.8f;
                Dust tooth = Dust.NewDustPerfect(tipPos, DustID.GoldFlame, toothVel, 0, default, 0.7f * comboIntensity);
                tooth.noGravity = true;
                tooth.fadeIn = 0.8f;

                // Counter-rotating pearl sparkle for gear interlocking visual
                Vector2 counterVel = perpendicular * -toothSide * 1.5f + swordDirection * 1.2f;
                Dust counter = Dust.NewDustPerfect(tipPos, DustID.IceTorch, counterVel, 0, default, 0.45f);
                counter.noGravity = true;
            }

            // Music box chime notes — scattered from the blade tip at musical intervals
            if (timer % 3 == 0)
            {
                Color noteColor = ClairDeLunePalette.GetClockworkGradient(Main.rand.NextFloat(0.4f, 0.8f));
                ClairDeLuneVFXLibrary.SpawnMusicNotes(tipPos, 1, 10f, 0.4f, 0.7f, 20);
            }

            // Combo escalation: brass-gold counter-rotating particle rings
            if (comboStep >= 1 && timer % 4 == 0)
            {
                float ringAngle = time * 0.06f * (comboStep % 2 == 0 ? 1f : -1f);
                float ringRadius = 12f + comboStep * 3f;
                Vector2 ringPos = tipPos + ringAngle.ToRotationVector2() * ringRadius;
                var orbiter = new SparkleParticle(
                    ringPos, swordDirection * 0.5f,
                    ClairDeLunePalette.MoonbeamGold * 0.8f, 0.2f * comboIntensity, 14);
                MagnumParticleHandler.SpawnParticle(orbiter);
            }

            if (comboStep >= 2 && timer % 5 == 0)
                ClairDeLuneVFXLibrary.SpawnPearlShimmer(tipPos, 1, 15f, 0.22f);

            ClairDeLuneVFXLibrary.AddPaletteLighting(tipPos, 0.6f, 0.45f * comboIntensity);
        }

        public static void SwingImpactVFX(Vector2 hitPos, int comboStep = 0)
        {
            float intensity = 1f + comboStep * 0.25f;
            ClairDeLuneVFXLibrary.MeleeImpact(hitPos, comboStep);

            // === SIGNATURE: CASCADING GEAR DETONATION — interlocking ring burst ===
            // Two counter-rotating rings of gear sparks expanding outward
            int ringCount = 10 + comboStep * 4;
            for (int ring = 0; ring < 2; ring++)
            {
                float ringDir = (ring == 0) ? 1f : -1f;
                float ringOffset = ring * MathHelper.Pi / ringCount;
                for (int i = 0; i < ringCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / ringCount + ringOffset;
                    float speed = (4f + Main.rand.NextFloat() * 2.5f) * intensity;
                    // Slight spiral for gear-meshing appearance
                    Vector2 vel = (angle + ringDir * 0.15f).ToRotationVector2() * speed;

                    Color gearCol = (ring == 0)
                        ? ClairDeLunePalette.GetClockworkGradient((float)i / ringCount)
                        : ClairDeLunePalette.PaletteLerp(ClairDeLunePalette.ClockworkHarmonyBlade, (float)i / ringCount);

                    int dustType = (i % 2 == 0) ? DustID.GoldFlame : DustID.IceTorch;
                    Dust d = Dust.NewDustPerfect(hitPos, dustType, vel, 0, default, 1f * intensity);
                    d.noGravity = true;
                    d.fadeIn = 1.1f;

                    // Glow particles for the outer ring only
                    if (ring == 0)
                    {
                        var glow = new GenericGlowParticle(hitPos, vel * 0.8f, gearCol * 0.7f, 0.25f, 14, true);
                        MagnumParticleHandler.SpawnParticle(glow);
                    }
                }
            }

            // Music box note cascade — a flurry of notes like a music box opening
            int noteCount = 2 + comboStep * 2;
            for (int i = 0; i < noteCount; i++)
            {
                float noteAngle = MathHelper.TwoPi * i / noteCount + Main.rand.NextFloat(0.3f);
                Vector2 noteVel = noteAngle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f) + Vector2.UnitY * -2f;
                Color noteCol = ClairDeLunePalette.GetClockworkGradient(Main.rand.NextFloat(0.3f, 0.9f));
                var note = new HueShiftingMusicNoteParticle(
                    hitPos + Main.rand.NextVector2Circular(10f, 10f), noteVel,
                    0.10f, 0.15f, 0.6f, 0.55f, // Brass-gold hue range
                    Main.rand.NextFloat(0.6f, 0.9f), 25);
                MagnumParticleHandler.SpawnParticle(note);
            }

            if (comboStep >= 1)
                ClairDeLuneVFXLibrary.SpawnPearlBurst(hitPos, 6 + comboStep * 2, 4f, 0.28f);

            if (comboStep >= 2)
                ClairDeLuneVFXLibrary.SpawnGradientHaloRings(hitPos, 4, 0.35f * intensity);

            ClairDeLuneVFXLibrary.DrawBloom(hitPos, 0.4f * intensity, 0.75f);
            ClairDeLuneVFXLibrary.AddPaletteLighting(hitPos, 0.6f, 0.7f * intensity);
        }

        public static void FinisherVFX(Vector2 pos, float intensity = 1f)
        {
            ClairDeLuneVFXLibrary.FinisherSlam(pos, intensity);

            // === SIGNATURE: GRAND CLOCKWORK CRESCENDO — a music box's final chord ===
            // Multi-ring gear detonation with ascending note fountain
            for (int ring = 0; ring < 3; ring++)
            {
                float ringDir = (ring % 2 == 0) ? 1f : -1f;
                int count = 10 + ring * 4;
                float speed = (5f + ring * 2f) * intensity;
                for (int i = 0; i < count; i++)
                {
                    float angle = MathHelper.TwoPi * i / count + ring * 0.2f;
                    Vector2 vel = (angle + ringDir * 0.1f).ToRotationVector2() * speed;
                    Color col = ClairDeLunePalette.GetClockworkGradient((float)i / count);
                    var glow = new GenericGlowParticle(pos, vel, col * 0.8f, 0.35f, 18 + ring * 2, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
            }

            // Ascending note fountain — notes shoot upward like a music box springing open
            for (int i = 0; i < 6; i++)
            {
                float spread = MathHelper.Lerp(-0.5f, 0.5f, (float)i / 5f);
                Vector2 noteVel = new Vector2(spread * 3f, -4f - Main.rand.NextFloat(3f)) * intensity;
                var note = new HueShiftingMusicNoteParticle(
                    pos + Main.rand.NextVector2Circular(8f, 4f), noteVel,
                    0.10f, 0.15f, 0.6f, 0.55f,
                    Main.rand.NextFloat(0.7f, 1f), 35);
                MagnumParticleHandler.SpawnParticle(note);
            }

            ClairDeLuneVFXLibrary.SpawnRadialDustBurst(pos, 20, 7f * intensity, DustID.GoldFlame);
            ClairDeLuneVFXLibrary.SpawnPearlBurst(pos, 12, 6f * intensity, 0.4f);
            ClairDeLuneVFXLibrary.SpawnGradientHaloRings(pos, 6, 0.45f * intensity);
            ClairDeLuneVFXLibrary.DrawBloom(pos, 0.8f * intensity, 1.1f);
            ClairDeLuneVFXLibrary.AddPaletteLighting(pos, 0.6f, 1.1f * intensity);
        }
    }
}
