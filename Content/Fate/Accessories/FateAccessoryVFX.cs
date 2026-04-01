using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Fate.Accessories
{
    /// <summary>
    /// Centralized VFX for all Fate accessories.
    /// Each accessory has its own section with dedicated methods.
    /// All colors come from FatePalette, all shared effects from FateVFXLibrary.
    /// </summary>
    public static class FateAccessoryVFX
    {
        // ===================================================================
        //  ASTRAL CONDUIT  (Magic — cosmic flare chains)
        // ===================================================================

        /// <summary>
        /// Ambient particles around the player while Astral Conduit is equipped.
        /// Enchanted motes + occasional cosmic glyph.
        /// </summary>
        public static void AstralConduitAmbientVFX(Player player)
        {
            if (Main.dedServ) return;

            // Enchanted pink/purple motes orbiting
            if (Main.rand.NextBool(6))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 dustPos = player.Center + angle.ToRotationVector2() * Main.rand.NextFloat(20f, 45f);
                Vector2 vel = angle.ToRotationVector2() * 1.5f;
                Color dustColor = Color.Lerp(FatePalette.DarkPink, FatePalette.FatePurple, Main.rand.NextFloat());

            }

            // Occasional cosmic glyph
            if (Main.rand.NextBool(15))
            {
                Vector2 glyphPos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
            }

            // Subtle ambient light
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.1f + 0.9f;
            Lighting.AddLight(player.Center, FatePalette.DarkPink.ToVector3() * pulse * 0.25f);
        }

        /// <summary>
        /// Cosmic flare chain VFX — triggered on magic hit (chaining effect).
        /// </summary>
        public static void AstralConduitFlareVFX(Vector2 sourcePos)
        {
            if (Main.dedServ) return;

            FateVFXLibrary.ProjectileImpact(sourcePos, 0.5f);
            FateVFXLibrary.SpawnGlyphBurst(sourcePos, 4, 4f);
            FateVFXLibrary.SpawnMusicNotes(sourcePos, 2, 20f);
            Lighting.AddLight(sourcePos, FatePalette.BrightCrimson.ToVector3() * 0.8f);
        }

        /// <summary>
        /// Lightning chain between cosmic flare targets.
        /// </summary>
        public static void AstralConduitChainVFX(Vector2 from, Vector2 to)
        {
            if (Main.dedServ) return;

            FateVFXLibrary.DrawCosmicLightning(from, to, 8, 25f,
                FatePalette.DarkPink, FatePalette.StarGold);
            Lighting.AddLight(to, FatePalette.BrightCrimson.ToVector3() * 0.6f);
        }

        // ===================================================================
        //  CONSTELLATION COMPASS  (Ranged — starburst on crit)
        // ===================================================================

        /// <summary>
        /// Ambient particles while Constellation Compass is equipped.
        /// Cardinal star points + occasional central flare.
        /// </summary>
        public static void ConstellationCompassAmbientVFX(Player player)
        {
            if (Main.dedServ) return;

            // Cardinal star points
            if (Main.rand.NextBool(8))
            {
                float baseAngle = Main.GameUpdateCount * 0.03f;
                int point = Main.rand.Next(4);
                float starAngle = baseAngle + MathHelper.PiOver2 * point;
                Vector2 starPos = player.Center + starAngle.ToRotationVector2() * 35f;

            }

            // Occasional central flare
            if (Main.rand.NextBool(20))
            {
            }

            Lighting.AddLight(player.Center, FatePalette.StarGold.ToVector3() * 0.2f);
        }

        /// <summary>
        /// Constellation starburst on ranged critical hit.
        /// 5-point star formation with connecting lines.
        /// </summary>
        public static void ConstellationCompassStarburstVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Central explosion
            FateVFXLibrary.ProjectileImpact(pos, 0.6f);

            // 5-point star formation
            Vector2[] starPoints = new Vector2[5];
            float baseAngle = Main.rand.NextFloat(MathHelper.TwoPi);
            for (int i = 0; i < 5; i++)
            {
                float angle = baseAngle + MathHelper.TwoPi * i / 5f;
                starPoints[i] = pos + angle.ToRotationVector2() * 45f;

            }

            // Connect star points with constellation lines
            for (int i = 0; i < 5; i++)
            {
                int next = (i + 2) % 5; // Pentagram pattern
                FateVFXLibrary.SpawnConstellationLine(starPoints[i], starPoints[next],
                    FatePalette.ConstellationSilver * 0.5f);
            }

            // Central flash
            FateVFXLibrary.SpawnMusicNotes(pos, 3, 30f);

            Lighting.AddLight(pos, FatePalette.StarGold.ToVector3() * 1.2f);
        }

        /// <summary>
        /// Flare on bonus damage dealt to starburst-marked enemies.
        /// </summary>
        public static void ConstellationCompassBonusDamageVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            Lighting.AddLight(pos, FatePalette.BrightCrimson.ToVector3() * 0.5f);
        }

        // ===================================================================
        //  MACHINATION OF THE EVENT HORIZON  (Utility — phase dodge)
        // ===================================================================

        /// <summary>
        /// Ambient accretion disk + gravitational lensing + event horizon glyphs.
        /// </summary>
        public static void EventHorizonAmbientVFX(Player player)
        {
            if (Main.dedServ) return;

            // Accretion disk particles
            if (Main.rand.NextBool(5))
            {
                float angle = Main.GameUpdateCount * 0.04f + Main.rand.NextFloat(MathHelper.Pi);
                float radius = 25f + Main.rand.NextFloat(15f);
                Vector2 diskPos = player.Center + angle.ToRotationVector2() * radius;
                Vector2 vel = angle.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * 1.5f;
                Color diskColor = Color.Lerp(FatePalette.CosmicVoid, FatePalette.DarkPink, Main.rand.NextFloat());

            }

            // Gravitational lensing star sparkles
            if (Main.rand.NextBool(12))
            {
                FateVFXLibrary.SpawnStarSparkles(player.Center, 1, 8f, 0.15f);
            }

            // Event horizon glyphs
            if (Main.rand.NextBool(18))
            {
                float glyphAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 glyphPos = player.Center + glyphAngle.ToRotationVector2() * 35f;
            }

            Lighting.AddLight(player.Center, FatePalette.FatePurple.ToVector3() * 0.2f);
        }

        /// <summary>
        /// Phase dodge VFX — cosmic explosion + warp particles + afterimage.
        /// </summary>
        public static void EventHorizonPhaseVFX(Vector2 playerCenter)
        {
            if (Main.dedServ) return;

            // Cosmic phase explosion
            FateVFXLibrary.ProjectileImpact(playerCenter, 0.6f);

            // Warp halo rings

            // Void warp particles radiating outward

            // Star sparkles at dodge destination
            FateVFXLibrary.SpawnStarSparkles(playerCenter, 8, 35f);

            // Glyph burst
            FateVFXLibrary.SpawnGlyphBurst(playerCenter, 6, 5f);
            FateVFXLibrary.SpawnMusicNotes(playerCenter, 2, 25f);

            Lighting.AddLight(playerCenter, FatePalette.FatePurple.ToVector3() * 1.0f);
        }

        // ===================================================================
        //  ORRERY OF INFINITE ORBITS  (Summon — minion empowerment)
        // ===================================================================

        /// <summary>
        /// Ambient orbiting planet particles + central star glow.
        /// </summary>
        public static void OrreryOrbitVFX(Player player)
        {
            if (Main.dedServ) return;

            // 3 orbiting planet particles at different speeds and radii
            Color[] planetColors = { FatePalette.DarkPink, FatePalette.BrightCrimson, FatePalette.FatePurple };
            float[] orbitSpeeds = { 0.02f, 0.035f, 0.05f };
            float[] orbitRadii = { 30f, 42f, 55f };

            for (int p = 0; p < 3; p++)
            {
                if (Main.GameUpdateCount % 4 != (uint)p) continue;

                float angle = Main.GameUpdateCount * orbitSpeeds[p];
                Vector2 planetPos = player.Center + angle.ToRotationVector2() * orbitRadii[p];

            }

            // Central star glow
            if (Main.rand.NextBool(10))
            {
            }

            Lighting.AddLight(player.Center, FatePalette.StarGold.ToVector3() * 0.2f);
        }

        /// <summary>
        /// Empowerment activation burst on a minion.
        /// </summary>
        public static void OrreryEmpowermentVFX(Vector2 minionPos)
        {
            if (Main.dedServ) return;

            FateVFXLibrary.ProjectileImpact(minionPos, 0.5f);
            FateVFXLibrary.SpawnGlyphBurst(minionPos, 4, 4f);
            FateVFXLibrary.SpawnMusicNotes(minionPos, 2, 20f);
            Lighting.AddLight(minionPos, FatePalette.BrightCrimson.ToVector3() * 0.8f);
        }

        /// <summary>
        /// Ongoing empowerment glow on the active empowered minion.
        /// </summary>
        public static void OrreryEmpoweredMinionVFX(Vector2 minionPos)
        {
            if (Main.dedServ) return;

            if (Main.rand.NextBool(4))
            {
            }

            // Orbiting star accent
            if (Main.rand.NextBool(8))
            {
                float starAngle = Main.GameUpdateCount * 0.08f;
                Vector2 starPos = minionPos + starAngle.ToRotationVector2() * 15f;
            }

            Lighting.AddLight(minionPos, FatePalette.DarkPink.ToVector3() * 0.4f);
        }

        /// <summary>
        /// Extra flare on empowered minion hit.
        /// </summary>
        public static void OrreryEmpoweredHitVFX(Vector2 hitPos)
        {
            if (Main.dedServ) return;

            Lighting.AddLight(hitPos, FatePalette.BrightCrimson.ToVector3() * 0.5f);
        }

        // ===================================================================
        //  PARADOX CHRONOMETER  (Melee — temporal echo)
        // ===================================================================

        /// <summary>
        /// Ambient clock-hand rotating particles + temporal glyphs.
        /// </summary>
        public static void ParadoxChronometerAmbientVFX(Player player)
        {
            if (Main.dedServ) return;

            // Hour hand particle (slow rotation)
            if (Main.GameUpdateCount % 6 == 0)
            {
                float hourAngle = Main.GameUpdateCount * 0.015f;
                Vector2 hourPos = player.Center + hourAngle.ToRotationVector2() * 25f;
            }

            // Minute hand particle (faster rotation)
            if (Main.GameUpdateCount % 4 == 0)
            {
                float minuteAngle = Main.GameUpdateCount * 0.04f;
                Vector2 minutePos = player.Center + minuteAngle.ToRotationVector2() * 35f;
            }

            // Temporal glyphs
            if (Main.rand.NextBool(14))
            {
                float glyphAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 glyphPos = player.Center + glyphAngle.ToRotationVector2() * 30f;
            }

            Lighting.AddLight(player.Center, FatePalette.DarkPink.ToVector3() * 0.2f);
        }

        /// <summary>
        /// Counter progress particles near hit threshold (visual anticipation).
        /// </summary>
        public static void ParadoxCounterProgressVFX(Vector2 hitPos, float intensity)
        {
            if (Main.dedServ) return;

            Lighting.AddLight(hitPos, FatePalette.DarkPink.ToVector3() * intensity * 0.4f);
        }

        /// <summary>
        /// Temporal echo activation VFX — afterimage + lightning + glyph burst.
        /// </summary>
        public static void ParadoxTemporalEchoVFX(Vector2 playerCenter)
        {
            if (Main.dedServ) return;

            // Afterimage trail

            // Glyph burst
            FateVFXLibrary.SpawnGlyphBurst(playerCenter, 6, 5f);

            // Halo rings

            // Temporal lightning arcs
            for (int i = 0; i < 3; i++)
            {
                float angle = MathHelper.TwoPi * i / 3f + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 endPos = playerCenter + angle.ToRotationVector2() * Main.rand.NextFloat(40f, 60f);
                FateVFXLibrary.DrawCosmicLightning(playerCenter, endPos, 5, 15f,
                    FatePalette.BrightCrimson, FatePalette.StarGold);
            }

            FateVFXLibrary.SpawnMusicNotes(playerCenter, 3, 30f);

            Lighting.AddLight(playerCenter, FatePalette.BrightCrimson.ToVector3() * 1.0f);
        }

        // ===================================================================
        //  SHARED ACCESSORY HELPERS
        // ===================================================================

        /// <summary>
        /// Generic Fate accessory aura for use by any Fate accessory.
        /// Subtle cosmic particles + ambient light.
        /// </summary>
        public static void GenericFateAuraVFX(Player player, float intensity = 0.3f)
        {
            if (Main.dedServ) return;

            if (Main.rand.NextBool(10))
            {
                Vector2 offset = Main.rand.NextVector2Circular(35f, 35f);
                Color auraColor = FatePalette.GetCosmicGradient(Main.rand.NextFloat()) * intensity;
            }

            FateVFXLibrary.AddFateLight(player.Center, intensity * 0.3f);
        }

        /// <summary>
        /// Standard Fate accessory PreDrawInWorld bloom layers.
        /// </summary>
        public static void DrawWorldItemBloom(SpriteBatch sb, Texture2D texture,
            Vector2 position, Vector2 origin, float rotation, float scale)
        {
            if (Main.dedServ) return;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.08f + 1f;
            FatePalette.DrawItemBloom(sb, texture, position, origin, rotation, scale, pulse);
        }
    }
}
