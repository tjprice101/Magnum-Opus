using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Trails;
using static MagnumOpus.Common.Systems.Particles.Particle;
using static MagnumOpus.Common.Systems.ThemedParticles;

namespace MagnumOpus.Content.Seasons.Weapons
{
    /// <summary>
    /// Swing projectile for the Four Seasons Blade.
    /// Each combo phase represents a different season with unique palette, trail, dust, and on-hit effects.
    /// Phase 0 = Spring (nature sweep), Phase 1 = Summer (blazing slash),
    /// Phase 2 = Autumn (heavy reap), Phase 3 = Winter (frost sweep).
    /// </summary>
    public sealed class FourSeasonsBladeSwing : MeleeSwingBase
    {
        public override string Texture => "MagnumOpus/Content/Seasons/Weapons/FourSeasonsBlade";

        #region Season Palettes

        private static readonly Color[] SpringPalette = new Color[]
        {
            new Color(60, 80, 40),       // [0] dark moss
            new Color(120, 180, 80),     // [1] fresh leaf
            new Color(255, 183, 197),    // [2] cherry blossom
            new Color(144, 238, 144),    // [3] spring green
            new Color(210, 255, 210),    // [4] pale bloom
            new Color(255, 250, 245)     // [5] white petal
        };

        private static readonly Color[] SummerPalette = new Color[]
        {
            new Color(100, 40, 0),       // [0] deep ember
            new Color(200, 100, 10),     // [1] amber
            new Color(255, 215, 0),      // [2] sunlight gold
            new Color(255, 180, 40),     // [3] solar flare
            new Color(255, 230, 100),    // [4] bright haze
            new Color(255, 250, 220)     // [5] white heat
        };

        private static readonly Color[] AutumnPalette = new Color[]
        {
            new Color(80, 40, 10),       // [0] dark bark
            new Color(139, 90, 43),      // [1] walnut
            new Color(255, 140, 50),     // [2] harvest orange
            new Color(220, 120, 40),     // [3] amber leaf
            new Color(255, 200, 100),    // [4] golden canopy
            new Color(255, 240, 200)     // [5] autumn mist
        };

        private static readonly Color[] WinterPalette = new Color[]
        {
            new Color(20, 40, 80),       // [0] deep midnight
            new Color(60, 120, 200),     // [1] cold cerulean
            new Color(150, 220, 255),    // [2] frost blue
            new Color(200, 240, 255),    // [3] ice crystal
            new Color(230, 248, 255),    // [4] pale frost
            new Color(240, 250, 255)     // [5] snow white
        };

        #endregion

        #region Quick Season Colors

        private static readonly Color[] SeasonPrimary =
        {
            new Color(255, 183, 197),    // Spring pink
            new Color(255, 215, 0),      // Summer gold
            new Color(255, 140, 50),     // Autumn orange
            new Color(150, 220, 255)     // Winter blue
        };

        private static readonly Color[] SeasonSecondary =
        {
            new Color(144, 238, 144),    // Spring green
            new Color(255, 140, 0),      // Summer orange
            new Color(139, 90, 43),      // Autumn brown
            new Color(240, 250, 255)     // Winter white
        };

        #endregion

        #region Combo Phases

        private static readonly ComboPhase[] Phases = new ComboPhase[]
        {
            // Phase 0: SPRING  Egentle flowing sweep (nature's breath)
            new ComboPhase(
                new CurveSegment[]
                {
                    new CurveSegment(EasingType.SineOut, 0f, -0.8f, 0.2f, 2),
                    new CurveSegment(EasingType.SineIn, 0.2f, -0.6f, 1.4f, 2),
                    new CurveSegment(EasingType.SineOut, 0.82f, 0.8f, 0.1f, 2)
                },
                MathHelper.PiOver2 * 1.4f, 52, 155f, false, 0.92f, 0.9f),

            // Phase 1: SUMMER  Eblazing fast horizontal slash (solar fury)
            new ComboPhase(
                new CurveSegment[]
                {
                    new CurveSegment(EasingType.PolyOut, 0f, -1f, 0.15f, 2),
                    new CurveSegment(EasingType.PolyIn, 0.15f, -0.85f, 1.75f, 3),
                    new CurveSegment(EasingType.PolyOut, 0.82f, 0.9f, 0.1f, 2)
                },
                MathHelper.PiOver2 * 1.6f, 45, 160f, true, 0.90f, 1.0f),

            // Phase 2: AUTUMN  Eheavy downward reaping strike (harvest's weight)
            new ComboPhase(
                new CurveSegment[]
                {
                    new CurveSegment(EasingType.PolyOut, 0f, -1.1f, 0.3f, 2),
                    new CurveSegment(EasingType.PolyIn, 0.3f, -0.8f, 1.7f, 3),
                    new CurveSegment(EasingType.PolyOut, 0.88f, 0.9f, 0.1f, 2)
                },
                MathHelper.PiOver2 * 1.8f, 55, 165f, false, 0.88f, 1.1f),

            // Phase 3: WINTER  Ewide frost sweep (winter's embrace)
            new ComboPhase(
                new CurveSegment[]
                {
                    new CurveSegment(EasingType.ExpOut, 0f, -1.2f, 0.25f, 2),
                    new CurveSegment(EasingType.PolyIn, 0.25f, -0.95f, 2.0f, 3),
                    new CurveSegment(EasingType.SineOut, 0.9f, 1.05f, 0.05f, 2)
                },
                MathHelper.PiOver2 * 2.0f, 58, 170f, true, 0.85f, 1.2f)
        };

        #endregion

        private int Season => Math.Clamp(ComboStep, 0, 3);

        protected override ComboPhase[] GetAllPhases() => Phases;

        protected override Color[] GetPalette()
        {
            return Season switch
            {
                0 => SpringPalette,
                1 => SummerPalette,
                2 => AutumnPalette,
                _ => WinterPalette
            };
        }

        protected override CalamityStyleTrailRenderer.TrailStyle GetTrailStyle()
        {
            return Season switch
            {
                0 => CalamityStyleTrailRenderer.TrailStyle.Nature,
                1 => CalamityStyleTrailRenderer.TrailStyle.Flame,
                2 => CalamityStyleTrailRenderer.TrailStyle.Nature,
                _ => CalamityStyleTrailRenderer.TrailStyle.Ice
            };
        }

        protected override string GetSmearTexturePath(int comboStep)
        {
            return comboStep switch
            {
                2 => "MagnumOpus/Assets/VFX Asset Library/ImpactEffects/ImpactEllipse",
                3 => "MagnumOpus/Assets/VFX Asset Library/ImpactEffects/ImpactEllipse",
                _ => "MagnumOpus/Assets/VFX Asset Library/ImpactEffects/ImpactEllipse"
            };
        }

        protected override string GetSmearGradientPath() => Math.Clamp(ComboStep, 0, 3) switch
        {
            0 => "MagnumOpus/Assets/VFX Asset Library/ColorGradients/EroicaGradientPALELUTandRAMP",     // Spring (pink/cherry blossom tones)
            1 => "MagnumOpus/Assets/VFX Asset Library/ColorGradients/LaCampanellaGradientLUTandRAMP",   // Summer (orange/gold fire)
            2 => "MagnumOpus/Assets/VFX Asset Library/ColorGradients/LaCampanellaGradientLUTandRAMP",   // Autumn (amber/harvest gold)
            3 => "MagnumOpus/Assets/VFX Asset Library/ColorGradients/ClairDeLuneGradientLUTandRAMP",    // Winter (ice blue/pearl)
            _ => "MagnumOpus/Assets/VFX Asset Library/ColorGradients/OdeToJoyGradientLUTandRAMP",       // Fallback (warm gold)
        };

        protected override int GetInitialDustType()
        {
            return Season switch
            {
                0 => DustID.GrassBlades,
                1 => DustID.Torch,
                2 => DustID.OrangeTorch,
                _ => DustID.IceTorch
            };
        }

        protected override int GetSecondaryDustType()
        {
            return Season switch
            {
                0 => DustID.Enchanted_Pink,
                1 => DustID.Enchanted_Gold,
                2 => DustID.AmberBolt,
                _ => DustID.Frost
            };
        }

        protected override Vector3 GetLightColor()
        {
            return SeasonPrimary[Season].ToVector3() * 0.6f;
        }

        protected override SoundStyle GetSwingSound()
        {
            return Season switch
            {
                0 => SoundID.Item71 with { Pitch = 0.1f },
                1 => SoundID.Item71 with { Pitch = 0.0f },
                2 => SoundID.Item71 with { Pitch = -0.2f },
                _ => SoundID.Item71 with { Pitch = -0.4f }
            };
        }

        // ═══ GPU Primitive Trail System (Incisor-style) ═══
        protected override SwingTrailMode GetTrailMode() => SwingTrailMode.GPUPrimitive;

        protected override MiscShaderData GetSlashShader()
            => GameShaders.Misc["MagnumOpus:IncisorSlash"];

        protected override void ConfigureSlashShader(MiscShaderData shader, bool isBloomPass)
        {
            if (shader == null) return;

            // Dynamic season-based shader configuration
            (Color primary, Color secondary, Color edge) = Season switch
            {
                0 => (  // Spring: cherry blossom pink core, deep rose secondary, spring green edge
                    isBloomPass ? new Color(180, 120, 150) : new Color(255, 183, 197),
                    new Color(120, 80, 100),
                    new Color(200, 255, 200)),
                1 => (  // Summer: sun gold core, deep amber secondary, sun orange edge
                    isBloomPass ? new Color(200, 140, 30) : new Color(255, 215, 0),
                    new Color(120, 50, 10),
                    new Color(255, 140, 0)),
                2 => (  // Autumn: harvest orange core, dark bark secondary, harvest gold edge
                    isBloomPass ? new Color(200, 110, 40) : new Color(255, 140, 50),
                    new Color(80, 40, 20),
                    new Color(218, 165, 32)),
                _ => (  // Winter: ice blue core, deep ocean secondary, frost white edge
                    isBloomPass ? new Color(100, 170, 220) : new Color(150, 220, 255),
                    new Color(30, 50, 100),
                    new Color(240, 250, 255))
            };

            shader.UseColor(primary);
            shader.UseSecondaryColor(secondary);
            shader.Shader.Parameters["fireColor"]?.SetValue(edge.ToVector3());
        }

        protected override void HandleComboSpecials()
        {
            if (hasSpawnedSpecial) return;

            // Season-specific blade-tip particles at ~75% swing progress
            if (Progression >= 0.75f)
            {
                hasSpawnedSpecial = true;
                Color primary = SeasonPrimary[Season];
                Vector2 tipPos = Owner.MountedCenter + SwordDirection * CurrentPhase.BladeLength;

                // Sparkle burst at blade tip
                for (int i = 0; i < 5; i++)
                {
                    Vector2 sparkleVel = Main.rand.NextVector2Circular(4f, 4f);
                    var spark = new SparkleParticle(tipPos, sparkleVel, primary, 0.4f, 20);
                    MagnumParticleHandler.SpawnParticle(spark);
                }

                // Music note from tip
                if (Main.rand.NextBool(2))
                {
                    float noteScale = Main.rand.NextFloat(0.7f, 0.9f);
                    ThemedParticles.MusicNote(tipPos, -SwordDirection * 2f, primary, noteScale, 30);
                }

                // Season-specific sub-effects
                switch (Season)
                {
                    case 0: // Spring  Epetal scatter
                        for (int i = 0; i < 3; i++)
                        {
                            Vector2 petalVel = -SwordDirection.RotatedByRandom(0.5f) * Main.rand.NextFloat(2f, 5f);
                            Dust d = Dust.NewDustPerfect(tipPos, DustID.PinkTorch, petalVel, 0, default, 1.2f);
                            d.noGravity = true;
                        }
                        break;
                    case 1: // Summer  Eember burst
                        for (int i = 0; i < 4; i++)
                        {
                            Vector2 emberVel = -SwordDirection.RotatedByRandom(0.4f) * Main.rand.NextFloat(3f, 7f);
                            Dust d = Dust.NewDustPerfect(tipPos, DustID.Torch, emberVel, 0, default, 1.4f);
                            d.noGravity = true;
                        }
                        break;
                    case 2: // Autumn  Eleaf drift
                        for (int i = 0; i < 3; i++)
                        {
                            Vector2 leafVel = -SwordDirection.RotatedByRandom(0.6f) * Main.rand.NextFloat(2f, 4f);
                            leafVel.Y -= 1f; // Slight upward drift
                            Dust d = Dust.NewDustPerfect(tipPos, DustID.AmberBolt, leafVel, 0, default, 1.3f);
                            d.noGravity = true;
                        }
                        break;
                    case 3: // Winter  Efrost shards
                        for (int i = 0; i < 4; i++)
                        {
                            Vector2 iceVel = -SwordDirection.RotatedByRandom(0.4f) * Main.rand.NextFloat(3f, 6f);
                            Dust d = Dust.NewDustPerfect(tipPos, DustID.Frost, iceVel, 0, default, 1.2f);
                            d.noGravity = true;
                        }
                        break;
                }
            }
        }

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int remainingDamageCount)
        {
            Player owner = Main.player[Projectile.owner];
            Color primary = SeasonPrimary[Season];
            Color secondary = SeasonSecondary[Season];

            switch (Season)
            {
                case 0: // Spring  Ehealing bloom + poison
                    owner.statLife = Math.Min(owner.statLife + 8, owner.statLifeMax2);
                    owner.HealEffect(8);
                    target.AddBuff(BuffID.Poisoned, 300);
                    for (int i = 0; i < 6; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                        var glow = new GenericGlowParticle(target.Center, vel, primary * 0.8f, 0.35f, 20, true);
                        MagnumParticleHandler.SpawnParticle(glow);
                    }
                    break;

                case 1: // Summer  Escorching blaze
                    target.AddBuff(BuffID.OnFire3, 300);
                    target.AddBuff(BuffID.Daybreak, 180);
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                        Dust d = Dust.NewDustPerfect(target.Center, DustID.Torch, vel, 0, default, 1.5f);
                        d.noGravity = true;
                    }
                    CustomParticles.GenericFlare(target.Center, primary, 0.6f, 15);
                    break;

                case 2: // Autumn  Eharvest lifesteal + cursed flames
                    int stolen = Math.Min(remainingDamageCount / 10, 15);
                    if (stolen > 0)
                    {
                        owner.statLife = Math.Min(owner.statLife + stolen, owner.statLifeMax2);
                        owner.HealEffect(stolen);
                    }
                    target.AddBuff(BuffID.CursedInferno, 300);
                    for (int i = 0; i < 6; i++)
                    {
                        Vector2 toOwner = (owner.Center - target.Center).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(3f, 7f);
                        var glow = new GenericGlowParticle(target.Center, toOwner, primary * 0.7f, 0.3f, 25, true);
                        MagnumParticleHandler.SpawnParticle(glow);
                    }
                    break;

                case 3: // Winter  Edeep freeze
                    target.AddBuff(BuffID.Frostburn2, 300);
                    if (Main.rand.NextBool(4))
                        target.AddBuff(BuffID.Frozen, 60);
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                        Dust d = Dust.NewDustPerfect(target.Center, DustID.Frost, vel, 0, default, 1.3f);
                        d.noGravity = true;
                    }
                    CustomParticles.GenericFlare(target.Center, primary, 0.5f, 18);
                    break;
            }

            // Universal gradient halo
            CustomParticles.HaloRing(target.Center, Color.Lerp(primary, secondary, 0.5f), 0.35f, 15);
        }

        protected override void DrawCustomVFX(SpriteBatch sb)
        {
            if (Progression < 0.08f || Progression > 0.95f) return;

            Color primary = SeasonPrimary[Season];
            Color secondary = SeasonSecondary[Season];
            // Per-season shadow color for atmospheric outer halo
            Color atmosphere = Season switch
            {
                0 => new Color(120, 80, 100),   // Spring: rose shadow
                1 => new Color(120, 50, 10),    // Summer: deep ember
                2 => new Color(80, 40, 10),     // Autumn: dark bark
                _ => new Color(20, 40, 80),     // Winter: deep midnight
            };

            Vector2 tipWorld = GetBladeTipPosition();
            Vector2 tipScreen = tipWorld - Main.screenPosition;
            Vector2 rootScreen = Owner.MountedCenter - Main.screenPosition;
            float phaseIntensity = 1f + ComboStep * 0.12f;
            float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 5f) * 0.10f;
            float swingFade = MathHelper.Clamp((Progression - 0.08f) / 0.08f, 0f, 1f)
                            * MathHelper.Clamp((0.95f - Progression) / 0.08f, 0f, 1f);

            // ═══ Switch to additive for layered season glow ═══
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
            Texture2D starTex = MagnumTextureRegistry.GetStar4Soft();
            if (glowTex != null)
            {
                Vector2 glowOrigin = glowTex.Size() / 2f;
                float baseScale = MathHelper.Min((0.18f + ComboStep * 0.05f) * pulse * phaseIntensity, 0.308f);

                // Layer 1: Wide atmospheric season haze (capped 300px max)
                sb.Draw(glowTex, tipScreen, null,
                    atmosphere with { A = 0 } * 0.2f * swingFade, 0f,
                    glowOrigin, baseScale * 1.9f, SpriteEffects.None, 0f);

                // Layer 2: Primary season color mid glow
                sb.Draw(glowTex, tipScreen, null,
                    primary with { A = 0 } * 0.45f * swingFade, 0f,
                    glowOrigin, baseScale * 1.6f, SpriteEffects.None, 0f);

                // Layer 3: Secondary season color inner glow
                sb.Draw(glowTex, tipScreen, null,
                    secondary with { A = 0 } * 0.6f * swingFade, 0f,
                    glowOrigin, baseScale * 0.9f, SpriteEffects.None, 0f);

                // Layer 4: White-hot core
                sb.Draw(glowTex, tipScreen, null,
                    Color.White with { A = 0 } * 0.75f * swingFade, 0f,
                    glowOrigin, baseScale * 0.3f, SpriteEffects.None, 0f);

                // Layer 5: Root glow — season-colored emanation at sword base
                float rootPulse = 0.6f + 0.4f * MathF.Sin(Main.GlobalTimeWrappedHourly * 2.5f);
                sb.Draw(glowTex, rootScreen, null,
                    secondary with { A = 0 } * 0.22f * swingFade * rootPulse, 0f,
                    glowOrigin, 0.12f * phaseIntensity, SpriteEffects.None, 0f);

                // Layer 6: Season transition shimmer at 45% blade (blends current+next season)
                Vector2 midScreen = Vector2.Lerp(rootScreen, tipScreen, 0.45f);
                int nextSeason = Math.Min(Season + 1, 3);
                Color transitionColor = Color.Lerp(primary, SeasonPrimary[nextSeason], 0.3f);
                sb.Draw(glowTex, midScreen, null,
                    transitionColor with { A = 0 } * 0.15f * swingFade, 0f,
                    glowOrigin, baseScale * 0.4f, SpriteEffects.None, 0f);
            }

            // Layer 7: Season star flare at tip
            if (starTex != null)
            {
                Vector2 starOrigin = starTex.Size() / 2f;
                float starRot = Main.GlobalTimeWrappedHourly * (2.5f + Season * 0.5f);
                float starScale = (0.07f + ComboStep * 0.02f) * pulse;
                sb.Draw(starTex, tipScreen, null,
                    primary with { A = 0 } * 0.6f * swingFade, starRot,
                    starOrigin, starScale, SpriteEffects.None, 0f);
                sb.Draw(starTex, tipScreen, null,
                    Color.White with { A = 0 } * 0.35f * swingFade, -starRot * 0.6f,
                    starOrigin, starScale * 0.5f, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // ═══ Season-specific blade dust ═══
            if (Progression > 0.12f && Progression < 0.88f)
            {
                float t = Main.rand.NextFloat(0.25f, 0.85f);
                Vector2 dustPos = Vector2.Lerp(Owner.MountedCenter, tipWorld, t);
                int dustType = Season switch
                {
                    0 => DustID.PinkTorch,
                    1 => DustID.SolarFlare,
                    2 => DustID.AmberBolt,
                    _ => DustID.IceTorch,
                };
                Vector2 drift = Season switch
                {
                    0 => new Vector2(Main.rand.NextFloat(-0.6f, 0.6f), Main.rand.NextFloat(-1f, -0.2f)),
                    1 => new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-1.8f, -0.5f)),
                    2 => new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(0.2f, 1.2f)),
                    _ => new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), Main.rand.NextFloat(-0.8f, 0.3f)),
                };
                Dust d = Dust.NewDustPerfect(dustPos, dustType, drift, 0,
                    Color.Lerp(primary, secondary, Main.rand.NextFloat()), 0.6f);
                d.noGravity = true;
                d.fadeIn = 0.4f;
            }

            // ═══ Music note particles ═══
            if (Main.rand.NextBool(4))
            {
                float hMin = Season switch { 0 => 0.25f, 1 => 0.04f, 2 => 0.06f, _ => 0.52f };
                float hMax = Season switch { 0 => 0.42f, 1 => 0.14f, 2 => 0.12f, _ => 0.68f };
                Vector2 noteVel = (Projectile.velocity.SafeNormalize(Vector2.UnitX) * -1.5f).RotatedByRandom(0.4);
                MagnumParticleHandler.SpawnParticle(new HueShiftingMusicNoteParticle(
                    tipWorld, noteVel, hMin, hMax, 0.85f, 0.60f, 0.75f, 25, 0.025f));
            }
        }
    }
}
