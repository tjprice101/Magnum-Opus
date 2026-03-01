using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.ClairDeLune.Weapons.Magic
{
    // =========================================================================
    //  ClockworkGrimoireVFX  EArcane Pages
    //  Shader: ArcanePages.fx (ArcanePageFlow + ArcanePageGlow)
    //  Identity: Flowing arcane script  Ean ancient tome whose pages
    //  spill luminous text into the air, each spell line a glowing phrase.
    // =========================================================================
    public static class ClockworkGrimoireVFX
    {
        public static void HoldItemVFX(Player player)
        {
            float time = (float)Main.timeForVisualEffects;

            // Floating runic glyphs  Eluminous text symbols orbit the player in a reading circle
            if (Main.rand.NextBool(2))
            {
                float glyphAngle = time * 0.025f + Main.rand.NextFloat(MathHelper.TwoPi);
                float glyphRadius = 22f + (float)Math.Sin(time * 0.03f) * 5f;
                Vector2 glyphPos = player.Center + glyphAngle.ToRotationVector2() * glyphRadius;
                Vector2 vel = (glyphAngle + MathHelper.PiOver2).ToRotationVector2() * 0.3f; // Tangential drift
                Color glyphCol = Color.Lerp(ClairDeLunePalette.SoftBlue, ClairDeLunePalette.PearlWhite,
                    (float)Math.Sin(time * 0.04f + glyphAngle) * 0.5f + 0.5f);
                Dust d = Dust.NewDustPerfect(glyphPos, DustID.MagicMirror, vel, 0, default, 0.45f);
                d.noGravity = true;
                d.fadeIn = 0.6f;
            }

            // Page-shimmer particles rising upward like luminous text evaporating
            if (Main.rand.NextBool(4))
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -Main.rand.NextFloat(0.5f, 1.2f));
                var shimmer = new SparkleParticle(
                    player.Center + Main.rand.NextVector2Circular(18f, 12f), vel,
                    ClairDeLunePalette.PearlBlue * 0.5f, 0.14f, 14);
                MagnumParticleHandler.SpawnParticle(shimmer);
            }

            ClairDeLuneVFXLibrary.AddClairDeLuneLight(player.Center, 0.25f);
        }

        public static void PreDrawInWorldBloom(SpriteBatch sb, Texture2D tex, Vector2 pos,
            Vector2 origin, float rotation, float scale)
        {
            if (ClairDeLuneShaderManager.HasArcanePages)
            {
                ClairDeLuneShaderManager.BeginShaderAdditive(sb);
                ClairDeLuneShaderManager.ApplyArcanePagesGlow((float)Main.timeForVisualEffects * 0.025f);

                sb.Draw(tex, pos - Main.screenPosition, null,
                    ClairDeLunePalette.SoftBlue * 0.5f, rotation, origin, scale * 1.04f,
                    SpriteEffects.None, 0f);

                ClairDeLuneShaderManager.RestoreSpriteBatch(sb);
            }
            else
            {
                ClairDeLuneVFXLibrary.DrawClairDeLuneBloomStack(sb, pos,
                    ClairDeLunePalette.SoftBlue, ClairDeLunePalette.PearlWhite, scale * 0.28f, 0.4f);
            }
        }

        public static void SpellCastVFX(Vector2 castPos, Vector2 castDirection)
        {
            float time = (float)Main.timeForVisualEffects;

            // === SIGNATURE: ARCANE PAGE BURST  Eluminous text pages scatter forward ===
            for (int i = 0; i < 10; i++)
            {
                float spread = Main.rand.NextFloat(-0.4f, 0.4f);
                Vector2 vel = castDirection.RotatedBy(spread) * (3f + Main.rand.NextFloat(2.5f));
                Color pageCol = Color.Lerp(ClairDeLunePalette.SoftBlue, ClairDeLunePalette.PearlWhite,
                    Main.rand.NextFloat(0.3f, 0.7f));
                Dust d = Dust.NewDustPerfect(castPos, DustID.MagicMirror, vel, 0, default, 0.75f);
                d.noGravity = true;
                d.fadeIn = 0.9f;

                // Layered glow particles interspersed  Earcane script energy
                if (i % 3 == 0)
                {
                    var glow = new GenericGlowParticle(castPos, vel * 0.7f,
                        pageCol * 0.7f, 0.25f, 16, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
            }

            // Pearl sparkle ring at cast origin
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f + time * 0.03f;
                Vector2 ringPos = castPos + angle.ToRotationVector2() * 12f;
                var sparkle = new SparkleParticle(ringPos, angle.ToRotationVector2() * 1.5f,
                    ClairDeLunePalette.PearlWhite * 0.6f, 0.2f, 12);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            ClairDeLuneVFXLibrary.SpawnPearlBurst(castPos, 5, 3.5f, 0.22f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(castPos, 2, 12f, 0.35f, 0.6f, 18);
            ClairDeLuneVFXLibrary.AddPaletteLighting(castPos, 0.3f, 0.55f);
        }

        public static void ProjectileTrailVFX(Vector2 projPos, Vector2 projVelocity)
        {
            Vector2 perpendicular = new Vector2(-projVelocity.Y, projVelocity.X);
            if (perpendicular != Vector2.Zero) perpendicular.Normalize();
            float time = (float)Main.timeForVisualEffects;

            // === SIGNATURE: ARCANE SCRIPT TRAIL  Eglowing rune characters tumble behind projectile ===
            // Dual-layer: core magic dust + floating sparkle glyphs

            // Core magic mirror line
            Dust d = Dust.NewDustPerfect(projPos, DustID.MagicMirror,
                perpendicular * Main.rand.NextFloat(-1.2f, 1.2f) + projVelocity * -0.12f, 0, default, 0.65f);
            d.noGravity = true;
            d.fadeIn = 0.8f;

            // Tumbling glyph sparkles  Ealternating sides like pages fluttering
            if (Main.GameUpdateCount % 2 == 0)
            {
                float side = (Main.GameUpdateCount % 4 < 2) ? 1f : -1f;
                Vector2 glyphPos = projPos + perpendicular * side * Main.rand.NextFloat(4f, 8f);
                Vector2 glyphVel = perpendicular * side * 0.5f + projVelocity * -0.08f;
                Color glyphCol = Color.Lerp(ClairDeLunePalette.SoftBlue, ClairDeLunePalette.PearlBlue,
                    (float)Math.Sin(time * 0.08f) * 0.5f + 0.5f);
                var glyph = new SparkleParticle(glyphPos, glyphVel, glyphCol * 0.7f, 0.18f, 12);
                MagnumParticleHandler.SpawnParticle(glyph);
            }

            // Soft pearl wake
            if (Main.GameUpdateCount % 3 == 0)
            {
                Dust trail = Dust.NewDustPerfect(projPos, DustID.IceTorch,
                    projVelocity * -0.06f, 0, default, 0.3f);
                trail.noGravity = true;
            }

            if (Main.GameUpdateCount % 7 == 0)
                ClairDeLuneVFXLibrary.SpawnMusicNotes(projPos, 1, 6f, 0.25f, 0.45f, 12);

            ClairDeLuneVFXLibrary.AddPaletteLighting(projPos, 0.3f, 0.38f);
        }

        public static void ProjectileImpactVFX(Vector2 hitPos)
        {
            ClairDeLuneVFXLibrary.ProjectileImpact(hitPos, 0);

            // === SIGNATURE: ARCANE SCATTER BURST  Erune fragments scatter like a shattered page ===
            for (int i = 0; i < 14; i++)
            {
                float angle = MathHelper.TwoPi * i / 14;
                float speed = 4f + Main.rand.NextFloat() * 3f;
                Vector2 vel = angle.ToRotationVector2() * speed;

                Color burstCol = (i % 3 == 0)
                    ? ClairDeLunePalette.PearlWhite
                    : Color.Lerp(ClairDeLunePalette.SoftBlue, ClairDeLunePalette.MidnightBlue, (float)i / 14);

                Dust d = Dust.NewDustPerfect(hitPos, DustID.MagicMirror, vel, 0, default, 0.9f);
                d.noGravity = true;
                d.fadeIn = 1f;

                // Arcane sparkle glyph layer
                if (i % 2 == 0)
                {
                    var glyph = new SparkleParticle(hitPos, vel * 0.6f, burstCol * 0.8f, 0.22f, 14);
                    MagnumParticleHandler.SpawnParticle(glyph);
                }
            }

            ClairDeLuneVFXLibrary.SpawnPearlBurst(hitPos, 8, 3.5f, 0.3f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(hitPos, 3, 14f, 0.4f, 0.7f, 22);
            ClairDeLuneVFXLibrary.DrawBloom(hitPos, 0.45f, 0.75f);
            ClairDeLuneVFXLibrary.AddPaletteLighting(hitPos, 0.3f, 0.65f);
        }
    }

    // =========================================================================
    //  OrreryOfDreamsVFX  ECelestial Orbit
    //  Shader: CelestialOrbit.fx (CelestialOrbitPath + CelestialOrbitCore)
    //  Identity: Dream planetarium  Eluminous orbs trace celestial paths
    //  around the caster, a miniature orrery of dreaming moons.
    // =========================================================================
    public static class OrreryOfDreamsVFX
    {
        public static void HoldItemVFX(Player player)
        {
            float time = (float)Main.timeForVisualEffects;

            // === SIGNATURE IDLE: MINIATURE ORRERY  E3 tiny orb-motes orbit at different speeds/radii ===
            for (int i = 0; i < 3; i++)
            {
                float orbitSpeed = 0.015f + i * 0.008f;
                float orbitRadius = 16f + i * 6f;
                // Elliptical orbits (wider horizontal than vertical)
                float orbAngle = time * orbitSpeed + i * MathHelper.TwoPi / 3f;
                Vector2 orbPos = player.Center + new Vector2(
                    (float)Math.Cos(orbAngle) * orbitRadius,
                    (float)Math.Sin(orbAngle) * orbitRadius * 0.55f);

                if (Main.rand.NextBool(3))
                {
                    Color orbCol = ClairDeLunePalette.PaletteLerp(
                        ClairDeLunePalette.OrreryOfDreamsOrbit, (float)i / 3f);
                    var orb = new GenericGlowParticle(orbPos, Vector2.Zero,
                        orbCol * 0.5f, 0.12f, 6, true);
                    MagnumParticleHandler.SpawnParticle(orb);
                }
            }

            // Gentle dream haze drift
            if (Main.rand.NextBool(6))
            {
                Vector2 driftVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.4f);
                Dust d = Dust.NewDustPerfect(
                    player.Center + Main.rand.NextVector2Circular(20f, 20f),
                    DustID.IceTorch, driftVel, 0, default, 0.3f);
                d.noGravity = true;
                d.fadeIn = 0.5f;
            }

            ClairDeLuneVFXLibrary.AmbientDreamyAura(player.Center, time);
            ClairDeLuneVFXLibrary.AddClairDeLuneLight(player.Center, 0.2f);
        }

        public static void PreDrawInWorldBloom(SpriteBatch sb, Texture2D tex, Vector2 pos,
            Vector2 origin, float rotation, float scale)
        {
            if (ClairDeLuneShaderManager.HasCelestialOrbit)
            {
                ClairDeLuneShaderManager.BeginShaderAdditive(sb);
                ClairDeLuneShaderManager.ApplyCelestialOrbitCore((float)Main.timeForVisualEffects * 0.02f);

                sb.Draw(tex, pos - Main.screenPosition, null,
                    ClairDeLunePalette.DreamHaze * 0.45f, rotation, origin, scale * 1.04f,
                    SpriteEffects.None, 0f);

                ClairDeLuneShaderManager.RestoreSpriteBatch(sb);
            }
            else
            {
                ClairDeLuneVFXLibrary.DrawClairDeLuneBloomStack(sb, pos,
                    ClairDeLunePalette.DreamHaze, ClairDeLunePalette.PearlBlue, scale * 0.25f, 0.35f);
            }
        }

        public static void OrbTrailVFX(Vector2 orbPos, Vector2 orbVelocity)
        {
            float time = (float)Main.timeForVisualEffects;

            // === SIGNATURE: CELESTIAL ORBIT WAKE  Eelliptical trail with stellar sparkle ===
            // Core dreamy trail dust
            Dust d = Dust.NewDustPerfect(orbPos, DustID.IceTorch,
                orbVelocity * -0.12f + Main.rand.NextVector2Circular(0.4f, 0.4f), 0, default, 0.55f);
            d.noGravity = true;
            d.fadeIn = 0.7f;

            // Stellar sparkle field  Etiny twinkling dots shed from the orb like stardust
            if (Main.GameUpdateCount % 2 == 0)
            {
                Color starCol = Color.Lerp(ClairDeLunePalette.DreamHaze, ClairDeLunePalette.PearlWhite,
                    (float)Math.Sin(time * 0.1f) * 0.5f + 0.5f);
                var star = new SparkleParticle(
                    orbPos + Main.rand.NextVector2Circular(5f, 5f),
                    orbVelocity * -0.05f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    starCol * 0.65f, 0.16f, 10);
                MagnumParticleHandler.SpawnParticle(star);
            }

            // Celestial glow  Esoft planetary trail
            if (Main.GameUpdateCount % 3 == 0)
            {
                Color planetCol = ClairDeLunePalette.PaletteLerp(
                    ClairDeLunePalette.OrreryOfDreamsOrbit,
                    (float)Math.Sin(time * 0.04f) * 0.3f + 0.5f);
                var planetGlow = new GenericGlowParticle(
                    orbPos, orbVelocity * -0.08f, planetCol * 0.5f, 0.2f, 12, true);
                MagnumParticleHandler.SpawnParticle(planetGlow);
            }

            if (Main.GameUpdateCount % 4 == 0)
                ClairDeLuneVFXLibrary.SpawnPearlSparkle(orbPos, orbVelocity * 0.1f);

            if (Main.GameUpdateCount % 8 == 0)
                ClairDeLuneVFXLibrary.SpawnMusicNotes(orbPos, 1, 6f, 0.25f, 0.4f, 10);

            ClairDeLuneVFXLibrary.AddPaletteLighting(orbPos, 0.3f, 0.35f);
        }

        public static void OrbImpactVFX(Vector2 hitPos)
        {
            ClairDeLuneVFXLibrary.ProjectileImpact(hitPos, 1);

            // === SIGNATURE: CELESTIAL NOVA  Econcentric planetary ring burst ===
            // Inner fast ring + outer slow ring (planetary collision look)
            int[] ringCounts = { 12, 8 };
            float[] ringSpeeds = { 5f, 3f };
            for (int ring = 0; ring < 2; ring++)
            {
                for (int i = 0; i < ringCounts[ring]; i++)
                {
                    float angle = MathHelper.TwoPi * i / ringCounts[ring] + ring * 0.3f;
                    Vector2 vel = angle.ToRotationVector2() * (ringSpeeds[ring] + Main.rand.NextFloat() * 2f);

                    Color orbitCol = (ring == 0)
                        ? ClairDeLunePalette.PaletteLerp(ClairDeLunePalette.OrreryOfDreamsOrbit, (float)i / ringCounts[ring])
                        : ClairDeLunePalette.DreamHaze;

                    Dust d = Dust.NewDustPerfect(hitPos, DustID.IceTorch, vel, 0, default, 0.85f);
                    d.noGravity = true;
                    d.fadeIn = 1f;

                    if (ring == 0 && i % 2 == 0)
                    {
                        var glow = new GenericGlowParticle(hitPos, vel * 0.7f, orbitCol * 0.7f, 0.25f, 16, true);
                        MagnumParticleHandler.SpawnParticle(glow);
                    }
                }
            }

            // Stellar sparkle scatter
            ClairDeLuneVFXLibrary.SpawnStarlitSparkles(hitPos, 6, 25f, 0.22f);
            ClairDeLuneVFXLibrary.SpawnPearlBurst(hitPos, 8, 3.5f, 0.28f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(hitPos, 2, 12f, 0.35f, 0.6f, 18);
            ClairDeLuneVFXLibrary.DrawBloom(hitPos, 0.4f, 0.65f);
            ClairDeLuneVFXLibrary.AddPaletteLighting(hitPos, 0.3f, 0.55f);
        }

        public static void OrbAmbientVFX(Vector2 orbPos)
        {
            float time = (float)Main.timeForVisualEffects;

            // Soft orbiting ring  Eelliptical halo around each orb
            if (Main.rand.NextBool(2))
            {
                float ringAngle = time * 0.04f;
                Vector2 ringOffset = new Vector2(
                    (float)Math.Cos(ringAngle) * 10f,
                    (float)Math.Sin(ringAngle) * 5f);
                Color haloCol = ClairDeLunePalette.DreamHaze * 0.4f;
                Dust d = Dust.NewDustPerfect(orbPos + ringOffset, DustID.IceTorch,
                    Vector2.Zero, 0, default, 0.2f);
                d.noGravity = true;
            }

            // Faint stellar twinkle
            if (Main.rand.NextBool(6))
            {
                var twinkle = new SparkleParticle(
                    orbPos + Main.rand.NextVector2Circular(6f, 6f), Vector2.Zero,
                    ClairDeLunePalette.PearlWhite * 0.35f, 0.1f, 8);
                MagnumParticleHandler.SpawnParticle(twinkle);
            }
        }
    }

    // =========================================================================
    //  RequiemOfTimeVFX  ETime Freeze Slash
    //  Shader: TimeFreezeSlash.fx (TimeFreezeSlash + TimeFreezeCrack)
    //  Identity: Reality-fracture sweep  Ea blade that cracks the fabric
    //  of time, leaving shattered temporal rifts in its swing path.
    // =========================================================================
    public static class RequiemOfTimeVFX
    {
        public static void HoldItemVFX(Player player)
        {
            float time = (float)Main.timeForVisualEffects;

            // Fractured reality motes  Etiny diamond-like shards drift near the player, hinting at cracked spacetime
            if (Main.rand.NextBool(2))
            {
                // Spawn at random angles but with a jagged, angular feel
                float snapAngle = (float)Math.Floor(Main.rand.NextFloat(8f)) * MathHelper.Pi / 4f;
                float dist = 16f + Main.rand.NextFloat(10f);
                Vector2 shardPos = player.Center + snapAngle.ToRotationVector2() * dist;
                Color shardCol = (Main.rand.NextBool(3)) ? ClairDeLunePalette.TemporalCrimson : ClairDeLunePalette.NightMist;
                Dust d = Dust.NewDustPerfect(shardPos, DustID.Clentaminator_Purple,
                    (shardPos - player.Center) * 0.01f, 0, default, 0.4f);
                d.noGravity = true;
                d.fadeIn = 0.5f;
            }

            // Occasional crimson rift spark  Ejagged energy flicker
            if (Main.rand.NextBool(6))
            {
                Vector2 riftOffset = Main.rand.NextVector2Circular(12f, 12f);
                var rift = new SparkleParticle(
                    player.Center + riftOffset, riftOffset * 0.03f,
                    ClairDeLunePalette.TemporalCrimson * 0.5f, 0.15f, 8);
                MagnumParticleHandler.SpawnParticle(rift);
            }

            ClairDeLuneVFXLibrary.AddClairDeLuneLight(player.Center, 0.25f);
        }

        public static void PreDrawInWorldBloom(SpriteBatch sb, Texture2D tex, Vector2 pos,
            Vector2 origin, float rotation, float scale)
        {
            if (ClairDeLuneShaderManager.HasTimeFreezeSlash)
            {
                ClairDeLuneShaderManager.BeginShaderAdditive(sb);
                ClairDeLuneShaderManager.ApplyTimeFreezeCrack((float)Main.timeForVisualEffects * 0.03f);

                sb.Draw(tex, pos - Main.screenPosition, null,
                    ClairDeLunePalette.TemporalCrimson * 0.4f, rotation, origin, scale * 1.04f,
                    SpriteEffects.None, 0f);

                ClairDeLuneShaderManager.RestoreSpriteBatch(sb);
            }
            else
            {
                ClairDeLuneVFXLibrary.DrawClairDeLuneBloomStack(sb, pos,
                    ClairDeLunePalette.TemporalCrimson, ClairDeLunePalette.NightMist, scale * 0.3f, 0.4f);
            }
        }

        public static void SwingTrailVFX(Vector2 tipPos, Vector2 swingDir)
        {
            Vector2 perpendicular = new Vector2(-swingDir.Y, swingDir.X);
            float time = (float)Main.timeForVisualEffects;

            // === SIGNATURE: REALITY-FRACTURE TRAIL  Ejagged crack lines radiate from the blade tip ===
            // Angular, sharp, broken  Enot smooth like the other weapons

            // Main void-purple sweep with angular jitter
            float jitter = (float)Math.Sin(time * 0.2f + tipPos.X * 0.01f) * 1.5f;
            Vector2 dustVel = perpendicular * (Main.rand.NextFloat(-2f, 2f) + jitter) + swingDir * 1.2f;
            Dust d = Dust.NewDustPerfect(tipPos, DustID.Clentaminator_Purple, dustVel, 0, default, 0.85f);
            d.noGravity = true;
            d.fadeIn = 0.9f;

            // Crack-line particles  Esharp linear streaks radiating 45° from swing direction
            if (Main.GameUpdateCount % 2 == 0)
            {
                float crackAngle = swingDir.ToRotation() + MathHelper.PiOver4 * (Main.rand.NextBool() ? 1f : -1f);
                Vector2 crackVel = crackAngle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color crackCol = Color.Lerp(ClairDeLunePalette.TemporalCrimson, ClairDeLunePalette.NightMist,
                    Main.rand.NextFloat(0.3f, 0.7f));
                var crack = new GenericGlowParticle(
                    tipPos + Main.rand.NextVector2Circular(3f, 3f), crackVel,
                    crackCol * 0.75f, 0.28f, 12, true);
                MagnumParticleHandler.SpawnParticle(crack);
            }

            // Crimson rift sparks
            if (Main.GameUpdateCount % 3 == 0)
            {
                Dust crimson = Dust.NewDustPerfect(tipPos, DustID.FireworkFountain_Red,
                    swingDir * 1.8f + Main.rand.NextVector2Circular(1f, 1f), 0, default, 0.55f);
                crimson.noGravity = true;
            }

            if (Main.GameUpdateCount % 5 == 0)
                ClairDeLuneVFXLibrary.SpawnMusicNotes(tipPos, 1, 8f, 0.3f, 0.5f, 14);

            ClairDeLuneVFXLibrary.AddPaletteLighting(tipPos, 0.8f, 0.55f);
        }

        public static void SwingImpactVFX(Vector2 hitPos)
        {
            ClairDeLuneVFXLibrary.MeleeImpact(hitPos, 1);

            // === SIGNATURE: SHATTERED GLASS PANEL BURST  Eangular fragments explode outward ===
            int dustCount = 18;
            for (int i = 0; i < dustCount; i++)
            {
                // Angular snapping  Efragments fly at multiples of 45° for a glass-shard pattern
                float baseAngle = MathHelper.TwoPi * i / dustCount;
                float snapAngle = (float)Math.Round(baseAngle / (MathHelper.PiOver4)) * MathHelper.PiOver4;
                float angle = MathHelper.Lerp(baseAngle, snapAngle, 0.5f);
                float speed = 5f + Main.rand.NextFloat() * 3f;
                Vector2 vel = angle.ToRotationVector2() * speed;

                int dustType = (i % 3 == 0) ? DustID.FireworkFountain_Red : DustID.Clentaminator_Purple;
                Dust d = Dust.NewDustPerfect(hitPos, dustType, vel, 0, default, 1.05f);
                d.noGravity = true;
                d.fadeIn = 1.1f;

                // Crack-line glow particles for the glass shard edges
                if (i % 2 == 0)
                {
                    Color crackCol = (i % 4 == 0)
                        ? ClairDeLunePalette.TemporalCrimson
                        : ClairDeLunePalette.NightMist;
                    var crackGlow = new GenericGlowParticle(
                        hitPos, vel * 0.6f, crackCol * 0.8f, 0.3f, 14, true);
                    MagnumParticleHandler.SpawnParticle(crackGlow);
                }
            }

            // Radiating crack lines  E4 cardinal directions with sparkle termination
            for (int cardDir = 0; cardDir < 4; cardDir++)
            {
                float crackDir = cardDir * MathHelper.PiOver2 + MathHelper.PiOver4;
                Vector2 crackEnd = hitPos + crackDir.ToRotationVector2() * 22f;
                var terminus = new SparkleParticle(crackEnd, crackDir.ToRotationVector2() * 2f,
                    ClairDeLunePalette.TemporalCrimson * 0.7f, 0.22f, 14);
                MagnumParticleHandler.SpawnParticle(terminus);
            }

            ClairDeLuneVFXLibrary.SpawnPearlBurst(hitPos, 8, 4f, 0.3f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(hitPos, 3, 14f, 0.45f, 0.7f, 22);
            ClairDeLuneVFXLibrary.DrawBloom(hitPos, 0.5f, 0.85f);
            ClairDeLuneVFXLibrary.AddPaletteLighting(hitPos, 0.8f, 0.75f);
        }

        public static void TimeFreezeReleaseVFX(Vector2 pos, float radius = 80f)
        {
            // === SIGNATURE: TEMPORAL COLLAPSE  Ereality implodes then detonates ===
            // Ring of shards CONVERGING inward (implosion), then radial outward burst

            // Implosion ring  Eparticles from the edge rushing inward
            int dustCount = 28;
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                Vector2 edgePos = pos + angle.ToRotationVector2() * radius;
                Vector2 vel = (pos - edgePos) * 0.05f;

                Color collapseCol = (i % 3 == 0)
                    ? ClairDeLunePalette.TemporalCrimson
                    : ClairDeLunePalette.NightMist;
                Dust d = Dust.NewDustPerfect(edgePos, DustID.Clentaminator_Purple, vel, 0, default, 1.2f);
                d.noGravity = true;
                d.fadeIn = 1.3f;

                // Crack energy glow converging
                if (i % 2 == 0)
                {
                    var converge = new GenericGlowParticle(edgePos, vel * 1.2f,
                        collapseCol * 0.8f, 0.35f, 16, true);
                    MagnumParticleHandler.SpawnParticle(converge);
                }
            }

            // Outward detonation  Ecrimson explosion from center
            ClairDeLuneVFXLibrary.SpawnRadialDustBurst(pos, 22, 9f, DustID.FireworkFountain_Red);

            // Reality-crack lines  E8 directions with graduated intensity
            for (int dir = 0; dir < 8; dir++)
            {
                float crackAngle = MathHelper.TwoPi * dir / 8f;
                for (int seg = 0; seg < 3; seg++)
                {
                    float dist = 18f + seg * 14f;
                    Vector2 crackPos = pos + crackAngle.ToRotationVector2() * dist;
                    var crackSpark = new SparkleParticle(crackPos, crackAngle.ToRotationVector2() * 2f,
                        ClairDeLunePalette.TemporalCrimson * (0.9f - seg * 0.2f), 0.28f, 18);
                    MagnumParticleHandler.SpawnParticle(crackSpark);
                }
            }

            ClairDeLuneVFXLibrary.SpawnGradientHaloRings(pos, 6, 0.55f);
            ClairDeLuneVFXLibrary.SpawnPearlBurst(pos, 12, 7f, 0.4f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 5, 24f, 0.5f, 1f, 32);
            ClairDeLuneVFXLibrary.DrawBloom(pos, 0.9f, 1.1f);
            ClairDeLuneVFXLibrary.AddPaletteLighting(pos, 0.8f, 1.3f);
        }
    }
}
