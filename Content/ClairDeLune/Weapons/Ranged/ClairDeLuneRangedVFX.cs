using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.ClairDeLune.Weapons.Ranged
{
    // =========================================================================
    //  StarfallWhisperVFX — Starfall Trail
    //  Shader: StarfallTrail.fx (StarfallBolt + StarfallWake)
    //  Identity: Falling star bolt — each shot is a tiny meteor falling
    //  from a dream sky, trailing luminous stardust in its wake.
    // =========================================================================
    public static class StarfallWhisperVFX
    {
        public static void HoldItemVFX(Player player)
        {
            if (Main.rand.NextBool(4))
            {
                Vector2 offset = Main.rand.NextVector2Circular(22f, 22f);
                Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.IceTorch,
                    new Vector2(0f, -Main.rand.NextFloat(0.3f, 0.8f)), 0, default, 0.35f);
                d.noGravity = true;
                d.fadeIn = 0.5f;
            }

            if (Main.rand.NextBool(8))
                ClairDeLuneVFXLibrary.SpawnStarlitSparkles(player.Center, 1, 25f, 0.12f);

            ClairDeLuneVFXLibrary.AddClairDeLuneLight(player.Center, 0.2f);
        }

        public static void PreDrawInWorldBloom(SpriteBatch sb, Texture2D tex, Vector2 pos,
            Vector2 origin, float rotation, float scale)
        {
            if (ClairDeLuneShaderManager.HasStarfallTrail)
            {
                ClairDeLuneShaderManager.BeginShaderAdditive(sb);
                ClairDeLuneShaderManager.ApplyStarfallWake((float)Main.timeForVisualEffects * 0.03f);

                sb.Draw(tex, pos - Main.screenPosition, null,
                    ClairDeLunePalette.StarlightSilver * 0.5f, rotation, origin, scale * 1.03f,
                    SpriteEffects.None, 0f);

                ClairDeLuneShaderManager.RestoreSpriteBatch(sb);
            }
            else
            {
                ClairDeLuneVFXLibrary.DrawClairDeLuneBloomStack(sb, pos,
                    ClairDeLunePalette.StarlightSilver, ClairDeLunePalette.PearlBlue, scale * 0.25f, 0.35f);
            }
        }

        public static void BoltTrailVFX(Vector2 boltPos, Vector2 boltVelocity)
        {
            Vector2 perpendicular = new Vector2(-boltVelocity.Y, boltVelocity.X);
            perpendicular.Normalize();

            Dust d = Dust.NewDustPerfect(boltPos, DustID.IceTorch,
                boltVelocity * -0.08f + perpendicular * Main.rand.NextFloat(-0.8f, 0.8f),
                0, default, 0.6f);
            d.noGravity = true;
            d.fadeIn = 0.7f;

            if (Main.GameUpdateCount % 2 == 0)
            {
                Dust sparkle = Dust.NewDustPerfect(boltPos, DustID.SilverFlame,
                    boltVelocity * -0.12f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    0, default, 0.35f);
                sparkle.noGravity = true;
            }

            if (Main.GameUpdateCount % 6 == 0)
                ClairDeLuneVFXLibrary.SpawnMusicNotes(boltPos, 1, 6f, 0.2f, 0.4f, 10);

            ClairDeLuneVFXLibrary.AddPaletteLighting(boltPos, 0.3f, 0.3f);
        }

        public static void BoltImpactVFX(Vector2 hitPos)
        {
            ClairDeLuneVFXLibrary.ProjectileImpact(hitPos, 0);

            int dustCount = 14;
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                float speed = 4f + Main.rand.NextFloat() * 3f;
                Vector2 vel = angle.ToRotationVector2() * speed;
                int dustType = (i % 3 == 0) ? DustID.SilverFlame : DustID.IceTorch;
                Dust d = Dust.NewDustPerfect(hitPos, dustType, vel, 0, default, 0.9f);
                d.noGravity = true;
                d.fadeIn = 1f;
            }

            ClairDeLuneVFXLibrary.SpawnPearlBurst(hitPos, 8, 4f, 0.3f);
            ClairDeLuneVFXLibrary.SpawnStarlitSparkles(hitPos, 5, 25f, 0.2f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(hitPos, 2, 12f, 0.35f, 0.65f, 18);
            ClairDeLuneVFXLibrary.DrawBloom(hitPos, 0.4f, 0.7f);
            ClairDeLuneVFXLibrary.AddPaletteLighting(hitPos, 0.3f, 0.5f);
        }

        public static void ChargeUpVFX(Vector2 muzzlePos, float chargeProgress)
        {
            float intensity = MathHelper.Clamp(chargeProgress, 0f, 1f);
            int dustCount = (int)(3 * intensity) + 1;

            for (int i = 0; i < dustCount; i++)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float radius = 30f * (1f - intensity) + 5f;
                Vector2 dustPos = muzzlePos + angle.ToRotationVector2() * radius;
                Vector2 vel = (muzzlePos - dustPos) * 0.08f * intensity;
                Dust d = Dust.NewDustPerfect(dustPos, DustID.IceTorch, vel, 0, default, 0.5f * intensity);
                d.noGravity = true;
                d.fadeIn = 0.6f;
            }

            if (intensity > 0.6f)
                ClairDeLuneVFXLibrary.SpawnPearlSparkle(muzzlePos, Vector2.Zero);

            ClairDeLuneVFXLibrary.AddPaletteLighting(muzzlePos, 0.3f, 0.3f * intensity);
        }
    }

    // =========================================================================
    //  MidnightMechanismVFX — Gatling Blur
    //  Shader: GatlingBlur.fx (GatlingBarrelBlur + GatlingMuzzle)
    //  Identity: Clockwork barrel blur — rapid-fire mechanical mayhem,
    //  brass-gold muzzle flashes and spinning barrel motion blur.
    // =========================================================================
    public static class MidnightMechanismVFX
    {
        public static void HoldItemVFX(Player player)
        {
            if (Main.rand.NextBool(4))
            {
                Vector2 offset = Main.rand.NextVector2Circular(18f, 18f);
                Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.GoldFlame,
                    Main.rand.NextVector2Circular(0.15f, 0.15f), 0, default, 0.35f);
                d.noGravity = true;
                d.fadeIn = 0.5f;
            }

            ClairDeLuneVFXLibrary.AmbientClockworkAura(player.Center, (float)Main.timeForVisualEffects);
            ClairDeLuneVFXLibrary.AddClairDeLuneLight(player.Center, 0.2f);
        }

        public static void PreDrawInWorldBloom(SpriteBatch sb, Texture2D tex, Vector2 pos,
            Vector2 origin, float rotation, float scale)
        {
            if (ClairDeLuneShaderManager.HasGatlingBlur)
            {
                ClairDeLuneShaderManager.BeginShaderAdditive(sb);
                ClairDeLuneShaderManager.ApplyGatlingMuzzle((float)Main.timeForVisualEffects * 0.04f);

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

        public static void BulletTrailVFX(Vector2 bulletPos, Vector2 bulletVelocity)
        {
            Vector2 perpendicular = new Vector2(-bulletVelocity.Y, bulletVelocity.X);
            perpendicular.Normalize();

            Dust d = Dust.NewDustPerfect(bulletPos, DustID.GoldFlame,
                bulletVelocity * -0.05f + perpendicular * Main.rand.NextFloat(-0.5f, 0.5f),
                0, default, 0.5f);
            d.noGravity = true;
            d.fadeIn = 0.6f;

            if (Main.GameUpdateCount % 3 == 0)
            {
                Dust spark = Dust.NewDustPerfect(bulletPos, DustID.IceTorch,
                    bulletVelocity * -0.1f, 0, default, 0.3f);
                spark.noGravity = true;
            }

            ClairDeLuneVFXLibrary.AddPaletteLighting(bulletPos, 0.6f, 0.25f);
        }

        public static void BulletImpactVFX(Vector2 hitPos)
        {
            ClairDeLuneVFXLibrary.ProjectileImpact(hitPos, 0);

            int dustCount = 10;
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                Vector2 vel = angle.ToRotationVector2() * (3.5f + Main.rand.NextFloat() * 2f);
                int dustType = (i % 2 == 0) ? DustID.GoldFlame : DustID.IceTorch;
                Dust d = Dust.NewDustPerfect(hitPos, dustType, vel, 0, default, 0.7f);
                d.noGravity = true;
                d.fadeIn = 0.9f;
            }

            ClairDeLuneVFXLibrary.SpawnPearlBurst(hitPos, 4, 2.5f, 0.2f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(hitPos, 1, 8f, 0.3f, 0.5f, 12);
            ClairDeLuneVFXLibrary.DrawBloom(hitPos, 0.3f, 0.5f);
            ClairDeLuneVFXLibrary.AddPaletteLighting(hitPos, 0.6f, 0.4f);
        }

        public static void SpinUpVFX(Vector2 barrelPos, float spinProgress)
        {
            float intensity = MathHelper.Clamp(spinProgress, 0f, 1f);

            if (Main.rand.NextBool(Math.Max(1, (int)(6 * (1f - intensity)))))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 sparkPos = barrelPos + angle.ToRotationVector2() * (8f + 6f * intensity);
                Vector2 vel = angle.ToRotationVector2() * 2f * intensity;
                Dust d = Dust.NewDustPerfect(sparkPos, DustID.GoldFlame, vel, 0, default, 0.5f * intensity);
                d.noGravity = true;
            }

            if (intensity > 0.5f && Main.GameUpdateCount % 4 == 0)
                ClairDeLuneVFXLibrary.SpawnMusicNotes(barrelPos, 1, 8f, 0.3f, 0.5f * intensity, 12);

            ClairDeLuneVFXLibrary.AddPaletteLighting(barrelPos, 0.6f, 0.3f * intensity);
        }
    }

    // =========================================================================
    //  CogAndHammerVFX — Singularity Pull
    //  Shader: SingularityPull.fx (SingularityVortex + SingularityCore)
    //  Identity: Temporal singularity collapse — bombs that collapse into
    //  miniature temporal singularities, pulling nearby matter inward.
    // =========================================================================
    public static class CogAndHammerVFX
    {
        public static void HoldItemVFX(Player player)
        {
            if (Main.rand.NextBool(3))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.IceTorch,
                    (player.Center - (player.Center + offset)) * 0.02f, 0, default, 0.4f);
                d.noGravity = true;
                d.fadeIn = 0.5f;
            }

            if (Main.rand.NextBool(8))
            {
                Dust brass = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(16f, 16f),
                    DustID.GoldFlame, Vector2.Zero, 0, default, 0.25f);
                brass.noGravity = true;
            }

            ClairDeLuneVFXLibrary.AddClairDeLuneLight(player.Center, 0.22f);
        }

        public static void PreDrawInWorldBloom(SpriteBatch sb, Texture2D tex, Vector2 pos,
            Vector2 origin, float rotation, float scale)
        {
            if (ClairDeLuneShaderManager.HasSingularityPull)
            {
                ClairDeLuneShaderManager.BeginShaderAdditive(sb);
                ClairDeLuneShaderManager.ApplySingularityCore((float)Main.timeForVisualEffects * 0.03f);

                sb.Draw(tex, pos - Main.screenPosition, null,
                    ClairDeLunePalette.DeepNight * 0.5f, rotation, origin, scale * 1.05f,
                    SpriteEffects.None, 0f);

                ClairDeLuneShaderManager.RestoreSpriteBatch(sb);
            }
            else
            {
                ClairDeLuneVFXLibrary.DrawClairDeLuneBloomStack(sb, pos,
                    ClairDeLunePalette.DeepNight, ClairDeLunePalette.PearlBlue, scale * 0.3f, 0.4f);
            }
        }

        public static void BombTrailVFX(Vector2 bombPos, Vector2 bombVelocity)
        {
            Dust d = Dust.NewDustPerfect(bombPos, DustID.IceTorch,
                bombVelocity * -0.08f + Main.rand.NextVector2Circular(0.6f, 0.6f),
                0, default, 0.55f);
            d.noGravity = true;
            d.fadeIn = 0.7f;

            if (Main.GameUpdateCount % 2 == 0)
            {
                Dust brass = Dust.NewDustPerfect(bombPos, DustID.GoldFlame,
                    bombVelocity * -0.12f, 0, default, 0.35f);
                brass.noGravity = true;
            }

            if (Main.GameUpdateCount % 8 == 0)
                ClairDeLuneVFXLibrary.SpawnMusicNotes(bombPos, 1, 6f, 0.2f, 0.4f, 10);

            ClairDeLuneVFXLibrary.AddPaletteLighting(bombPos, 0.3f, 0.3f);
        }

        public static void BombDetonationVFX(Vector2 detonationPos, float radius = 100f)
        {
            int outerCount = 20;
            for (int i = 0; i < outerCount; i++)
            {
                float angle = MathHelper.TwoPi * i / outerCount;
                Vector2 edgePos = detonationPos + angle.ToRotationVector2() * radius;
                Vector2 vel = (detonationPos - edgePos) * 0.06f;
                Dust d = Dust.NewDustPerfect(edgePos, DustID.IceTorch, vel, 0, default, 1.1f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            int innerCount = 14;
            for (int i = 0; i < innerCount; i++)
            {
                float angle = MathHelper.TwoPi * i / innerCount;
                Vector2 vel = angle.ToRotationVector2() * (6f + Main.rand.NextFloat() * 3f);
                int dustType = (i % 3 == 0) ? DustID.GoldFlame : DustID.IceTorch;
                Dust d = Dust.NewDustPerfect(detonationPos, dustType, vel, 0, default, 1f);
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }

            ClairDeLuneVFXLibrary.SpawnRadialDustBurst(detonationPos, 18, 7f, DustID.IceTorch);
            ClairDeLuneVFXLibrary.SpawnPearlBurst(detonationPos, 12, 5f, 0.4f);
            ClairDeLuneVFXLibrary.SpawnGradientHaloRings(detonationPos, 5, 0.5f);
            ClairDeLuneVFXLibrary.SpawnConvergingMist(detonationPos, 8, radius * 0.7f, 0.5f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(detonationPos, 4, 20f, 0.5f, 1f, 30);
            ClairDeLuneVFXLibrary.DrawBloom(detonationPos, 0.8f, 1f);
            ClairDeLuneVFXLibrary.AddPaletteLighting(detonationPos, 0.3f, 1.2f);
        }
    }
}
