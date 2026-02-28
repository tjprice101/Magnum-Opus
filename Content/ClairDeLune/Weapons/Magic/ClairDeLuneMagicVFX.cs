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
    //  ClockworkGrimoireVFX — Arcane Pages
    //  Shader: ArcanePages.fx (ArcanePageFlow + ArcanePageGlow)
    //  Identity: Flowing arcane script — an ancient tome whose pages
    //  spill luminous text into the air, each spell line a glowing phrase.
    // =========================================================================
    public static class ClockworkGrimoireVFX
    {
        public static void HoldItemVFX(Player player)
        {
            if (Main.rand.NextBool(3))
            {
                Vector2 offset = Main.rand.NextVector2Circular(24f, 24f);
                Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.MagicMirror,
                    offset * -0.02f, 0, default, 0.4f);
                d.noGravity = true;
                d.fadeIn = 0.6f;
            }

            if (Main.rand.NextBool(6))
            {
                Vector2 vel = new Vector2(0f, -Main.rand.NextFloat(0.5f, 1.2f));
                Dust rune = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(18f, 18f),
                    DustID.IceTorch, vel, 0, default, 0.3f);
                rune.noGravity = true;
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
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = castDirection * 4f + Main.rand.NextVector2Circular(2f, 2f);
                Dust d = Dust.NewDustPerfect(castPos, DustID.MagicMirror, vel, 0, default, 0.7f);
                d.noGravity = true;
                d.fadeIn = 0.9f;
            }

            ClairDeLuneVFXLibrary.SpawnPearlBurst(castPos, 4, 3f, 0.2f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(castPos, 2, 12f, 0.35f, 0.6f, 18);
            ClairDeLuneVFXLibrary.AddPaletteLighting(castPos, 0.3f, 0.5f);
        }

        public static void ProjectileTrailVFX(Vector2 projPos, Vector2 projVelocity)
        {
            Vector2 perpendicular = new Vector2(-projVelocity.Y, projVelocity.X);
            perpendicular.Normalize();

            Dust d = Dust.NewDustPerfect(projPos, DustID.MagicMirror,
                perpendicular * Main.rand.NextFloat(-1f, 1f) + projVelocity * -0.1f, 0, default, 0.6f);
            d.noGravity = true;
            d.fadeIn = 0.8f;

            if (Main.GameUpdateCount % 3 == 0)
            {
                Dust trail = Dust.NewDustPerfect(projPos, DustID.IceTorch,
                    projVelocity * -0.05f, 0, default, 0.3f);
                trail.noGravity = true;
            }

            if (Main.GameUpdateCount % 6 == 0)
                ClairDeLuneVFXLibrary.SpawnMusicNotes(projPos, 1, 6f, 0.25f, 0.45f, 12);

            ClairDeLuneVFXLibrary.AddPaletteLighting(projPos, 0.3f, 0.35f);
        }

        public static void ProjectileImpactVFX(Vector2 hitPos)
        {
            ClairDeLuneVFXLibrary.ProjectileImpact(hitPos, 0);

            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12;
                Vector2 vel = angle.ToRotationVector2() * (5f + Main.rand.NextFloat() * 2f);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.MagicMirror, vel, 0, default, 0.9f);
                d.noGravity = true;
                d.fadeIn = 1f;
            }

            ClairDeLuneVFXLibrary.SpawnPearlBurst(hitPos, 8, 3.5f, 0.3f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(hitPos, 2, 14f, 0.4f, 0.7f, 22);
            ClairDeLuneVFXLibrary.DrawBloom(hitPos, 0.4f, 0.7f);
            ClairDeLuneVFXLibrary.AddPaletteLighting(hitPos, 0.3f, 0.6f);
        }
    }

    // =========================================================================
    //  OrreryOfDreamsVFX — Celestial Orbit
    //  Shader: CelestialOrbit.fx (CelestialOrbitPath + CelestialOrbitCore)
    //  Identity: Dream planetarium — luminous orbs trace celestial paths
    //  around the caster, a miniature orrery of dreaming moons.
    // =========================================================================
    public static class OrreryOfDreamsVFX
    {
        public static void HoldItemVFX(Player player)
        {
            if (Main.rand.NextBool(4))
            {
                float angle = (float)Main.timeForVisualEffects * 0.02f + Main.rand.NextFloat() * MathHelper.TwoPi;
                float radius = 20f + Main.rand.NextFloat() * 10f;
                Vector2 orbPos = player.Center + angle.ToRotationVector2() * radius;
                Dust d = Dust.NewDustPerfect(orbPos, DustID.IceTorch,
                    Vector2.Zero, 0, default, 0.35f);
                d.noGravity = true;
                d.fadeIn = 0.5f;
            }

            ClairDeLuneVFXLibrary.AmbientDreamyAura(player.Center, (float)Main.timeForVisualEffects);
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
            Dust d = Dust.NewDustPerfect(orbPos, DustID.IceTorch,
                orbVelocity * -0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f), 0, default, 0.55f);
            d.noGravity = true;
            d.fadeIn = 0.7f;

            if (Main.GameUpdateCount % 4 == 0)
                ClairDeLuneVFXLibrary.SpawnPearlSparkle(orbPos, orbVelocity * 0.1f);

            if (Main.GameUpdateCount % 8 == 0)
                ClairDeLuneVFXLibrary.SpawnMusicNotes(orbPos, 1, 6f, 0.25f, 0.4f, 10);

            ClairDeLuneVFXLibrary.AddPaletteLighting(orbPos, 0.3f, 0.3f);
        }

        public static void OrbImpactVFX(Vector2 hitPos)
        {
            ClairDeLuneVFXLibrary.ProjectileImpact(hitPos, 1);

            int dustCount = 10;
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                Vector2 vel = angle.ToRotationVector2() * (3f + Main.rand.NextFloat() * 2f);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.IceTorch, vel, 0, default, 0.8f);
                d.noGravity = true;
                d.fadeIn = 1f;
            }

            ClairDeLuneVFXLibrary.SpawnPearlBurst(hitPos, 6, 3f, 0.25f);
            ClairDeLuneVFXLibrary.SpawnStarlitSparkles(hitPos, 4, 20f, 0.2f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(hitPos, 2, 12f, 0.35f, 0.6f, 18);
            ClairDeLuneVFXLibrary.DrawBloom(hitPos, 0.35f, 0.6f);
            ClairDeLuneVFXLibrary.AddPaletteLighting(hitPos, 0.3f, 0.5f);
        }

        public static void OrbAmbientVFX(Vector2 orbPos)
        {
            if (Main.rand.NextBool(3))
            {
                float ring = (float)Main.timeForVisualEffects * 0.03f;
                Vector2 ringOffset = ring.ToRotationVector2() * 8f;
                Dust d = Dust.NewDustPerfect(orbPos + ringOffset, DustID.IceTorch,
                    Vector2.Zero, 0, default, 0.25f);
                d.noGravity = true;
            }
        }
    }

    // =========================================================================
    //  RequiemOfTimeVFX — Time Freeze Slash
    //  Shader: TimeFreezeSlash.fx (TimeFreezeSlash + TimeFreezeCrack)
    //  Identity: Reality-fracture sweep — a blade that cracks the fabric
    //  of time, leaving shattered temporal rifts in its swing path.
    // =========================================================================
    public static class RequiemOfTimeVFX
    {
        public static void HoldItemVFX(Player player)
        {
            if (Main.rand.NextBool(3))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.Clentaminator_Purple,
                    offset * 0.02f, 0, default, 0.4f);
                d.noGravity = true;
                d.fadeIn = 0.5f;
            }

            if (Main.rand.NextBool(5))
            {
                Dust crimson = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(18f, 18f),
                    DustID.FireworkFountain_Red, Vector2.Zero, 0, default, 0.3f);
                crimson.noGravity = true;
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

            Vector2 dustVel = perpendicular * Main.rand.NextFloat(-2f, 2f) + swingDir * 1f;
            Dust d = Dust.NewDustPerfect(tipPos, DustID.Clentaminator_Purple, dustVel, 0, default, 0.8f);
            d.noGravity = true;
            d.fadeIn = 0.9f;

            if (Main.GameUpdateCount % 3 == 0)
            {
                Dust crimson = Dust.NewDustPerfect(tipPos, DustID.FireworkFountain_Red,
                    swingDir * 1.5f + Main.rand.NextVector2Circular(0.8f, 0.8f), 0, default, 0.5f);
                crimson.noGravity = true;
            }

            if (Main.GameUpdateCount % 5 == 0)
                ClairDeLuneVFXLibrary.SpawnMusicNotes(tipPos, 1, 8f, 0.3f, 0.5f, 14);

            ClairDeLuneVFXLibrary.AddPaletteLighting(tipPos, 0.8f, 0.5f);
        }

        public static void SwingImpactVFX(Vector2 hitPos)
        {
            ClairDeLuneVFXLibrary.MeleeImpact(hitPos, 1);

            int dustCount = 16;
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                Vector2 vel = angle.ToRotationVector2() * (5f + Main.rand.NextFloat() * 2f);
                int dustType = (i % 3 == 0) ? DustID.FireworkFountain_Red : DustID.Clentaminator_Purple;
                Dust d = Dust.NewDustPerfect(hitPos, dustType, vel, 0, default, 1f);
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }

            ClairDeLuneVFXLibrary.SpawnPearlBurst(hitPos, 8, 4f, 0.3f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(hitPos, 2, 14f, 0.45f, 0.7f, 22);
            ClairDeLuneVFXLibrary.DrawBloom(hitPos, 0.5f, 0.8f);
            ClairDeLuneVFXLibrary.AddPaletteLighting(hitPos, 0.8f, 0.7f);
        }

        public static void TimeFreezeReleaseVFX(Vector2 pos, float radius = 80f)
        {
            int dustCount = 24;
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                Vector2 edgePos = pos + angle.ToRotationVector2() * radius;
                Vector2 vel = (pos - edgePos) * 0.04f;
                Dust d = Dust.NewDustPerfect(edgePos, DustID.Clentaminator_Purple, vel, 0, default, 1.2f);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }

            ClairDeLuneVFXLibrary.SpawnRadialDustBurst(pos, 20, 8f, DustID.FireworkFountain_Red);
            ClairDeLuneVFXLibrary.SpawnGradientHaloRings(pos, 5, 0.5f);
            ClairDeLuneVFXLibrary.SpawnPearlBurst(pos, 10, 6f, 0.4f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 4, 20f, 0.5f, 1f, 30);
            ClairDeLuneVFXLibrary.DrawBloom(pos, 0.8f, 1f);
            ClairDeLuneVFXLibrary.AddPaletteLighting(pos, 0.8f, 1.2f);
        }
    }
}
