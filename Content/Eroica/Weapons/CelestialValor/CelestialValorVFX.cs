using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Screen;
using MagnumOpus.Common.Systems.VFX.Trails;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor
{
    /// <summary>
    /// Static VFX helper for CelestialValor — all visual effects consolidated here.
    /// Game logic (projectile spawning, debuffs, AOE damage) stays in the original files.
    /// All VFX now routed through EroicaVFXLibrary for canonical palette + modern systems.
    /// </summary>
    public static class CelestialValorVFX
    {
        // ══════════════════════════════════════════════════════════════
        //  SWING PHASE VFX  (called from HandleComboSpecials)
        // ══════════════════════════════════════════════════════════════

        /// <summary>Phase 0 — quick opener VFX at blade tip.</summary>
        public static void SwingPhase0VFX(Vector2 tipPos, Vector2 swordDir)
        {
            EroicaVFXLibrary.BloomFlare(tipPos, EroicaPalette.Gold, 0.6f, 15);
            EroicaVFXLibrary.SpawnGradientHaloRings(tipPos, 2, 0.35f);
            EroicaVFXLibrary.SpawnDirectionalSparks(tipPos, swordDir, 5, 6f);
        }

        /// <summary>Phase 1 — main slash VFX with sakura petals.</summary>
        public static void SwingPhase1VFX(Vector2 tipPos, Vector2 swordDir)
        {
            EroicaVFXLibrary.HeroicImpact(tipPos, 0.8f);
            EroicaVFXLibrary.SpawnDirectionalSparks(tipPos, swordDir, 8, 7f);
            EroicaVFXLibrary.SpawnSakuraPetals(tipPos, 3, 30f);
        }

        /// <summary>Phase 2 — finisher VFX with full impact cascade + screen shake.</summary>
        public static void SwingPhase2VFX(Vector2 tipPos)
        {
            EroicaVFXLibrary.FinisherSlam(tipPos, 1.3f);
            EroicaVFXLibrary.SpawnSakuraPetals(tipPos, 6, 45f);
            EroicaVFXLibrary.SpawnMusicNotes(tipPos, 5, 35f);
        }

        // ══════════════════════════════════════════════════════════════
        //  SWING HIT IMPACT  (called from OnSwingHitNPC)
        // ══════════════════════════════════════════════════════════════

        /// <summary>On-hit VFX for melee swing (game logic like debuffs stays in Swing.cs).</summary>
        public static void SwingHitImpact(Vector2 center, int comboStep)
        {
            // Combo-scaled impact via EroicaVFXLibrary
            EroicaVFXLibrary.MeleeImpact(center, comboStep);

            // Extra sakura petals + music notes
            EroicaVFXLibrary.SpawnSakuraPetals(center, 2 + comboStep, 30f);
            EroicaVFXLibrary.SpawnMusicNotes(center, 3 + comboStep, 30f);

            Lighting.AddLight(center, EroicaPalette.Scarlet.ToVector3() * (0.8f + comboStep * 0.15f));
        }

        // ══════════════════════════════════════════════════════════════
        //  SWING TRAIL VFX  (called from DrawCustomVFX every frame)
        // ══════════════════════════════════════════════════════════════

        /// <summary>Per-frame swing trail: ember dust, sakura sparkles, valor shimmer, music notes, bloom.</summary>
        public static void DrawSwingTrailVFX(Vector2 tipPos, Vector2 ownerCenter, Vector2 swordDir, float progression, int comboStep)
        {
            if (progression <= 0.08f || progression >= 0.92f)
                return;

            // Dense ember trail using EroicaVFXLibrary swing dust
            EroicaVFXLibrary.SpawnSwingDust(tipPos, -swordDir, DustID.GoldFlame);

            // Sakura sparkles (1/3 chance)
            if (Main.rand.NextBool(3))
            {
                Vector2 sparklePos = tipPos + Main.rand.NextVector2Circular(8f, 8f);
                Dust s = Dust.NewDustPerfect(sparklePos, DustID.PinkFairy,
                    -swordDir * Main.rand.NextFloat(0.5f, 2f), 0, EroicaPalette.Sakura, 1.2f);
                s.noGravity = true;
            }

            // Valor shimmer (1/3 chance) — hue oscillation in scarlet-gold range
            if (Main.rand.NextBool(3))
            {
                Color valorColor = EroicaPalette.GetShimmer(Main.GameUpdateCount);
                Dust v = Dust.NewDustPerfect(tipPos, DustID.GoldFlame,
                    Main.rand.NextVector2Circular(1.5f, 1.5f), 0, valorColor, 1.3f);
                v.noGravity = true;
            }

            // Music notes (1/5 chance) — scarlet/gold hue band
            if (Main.rand.NextBool(5))
            {
                EroicaVFXLibrary.SpawnMusicNotes(tipPos, 1, 8f, 0.7f, 0.9f, 28);
            }

            // Contrast sparkle for dual-colour trail
            EroicaVFXLibrary.SpawnContrastSparkle(tipPos, -swordDir);

            // Bloom at blade tip
            float bloomOpacity = MathHelper.Clamp((progression - 0.08f) / 0.10f, 0f, 1f)
                               * MathHelper.Clamp((0.92f - progression) / 0.10f, 0f, 1f);
            if (bloomOpacity > 0f)
            {
                EroicaVFXLibrary.DrawComboBloom(tipPos, comboStep, 0.45f, bloomOpacity);
            }
        }

        // ══════════════════════════════════════════════════════════════
        //  PROJECTILE TRAIL VFX  (called from Projectile.AI)
        // ══════════════════════════════════════════════════════════════

        /// <summary>Per-frame projectile trail particles.</summary>
        public static void ProjectileTrailVFX(Projectile proj)
        {
            // Flame trail dust
            if (Main.rand.NextBool(2))
            {
                EroicaVFXLibrary.SpawnFlameTrailDust(proj.Center, proj.velocity);
            }

            // Valor sparkles
            if (Main.rand.NextBool(3))
            {
                EroicaVFXLibrary.SpawnValorSparkles(proj.Center, 1, 8f);
            }

            // Music notes (1/4 chance)
            if (Main.rand.NextBool(4))
            {
                EroicaVFXLibrary.SpawnMusicNotes(proj.Center, 1, 10f, 0.6f, 0.8f, 25);
            }

            EroicaVFXLibrary.AddPaletteLighting(proj.Center, 0.4f, 0.8f);
        }

        // ══════════════════════════════════════════════════════════════
        //  PROJECTILE HIT VFX  (called from Projectile.OnHitNPC)
        // ══════════════════════════════════════════════════════════════

        /// <summary>Bloom + impact when projectile hits an NPC.</summary>
        public static void ProjectileHitVFX(Vector2 center)
        {
            EroicaVFXLibrary.HeroicImpact(center, 0.8f);
            EroicaVFXLibrary.SpawnMusicNotes(center, 5, 32f, 0.7f, 1.0f, 30);
        }

        // ══════════════════════════════════════════════════════════════
        //  AOE EXPLOSION  (called from CreateAOEExplosion)
        // ══════════════════════════════════════════════════════════════

        /// <summary>Full AOE explosion VFX: sound, particles, dust rings, lightning.</summary>
        public static void AOEExplosion(Vector2 position)
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.3f, Volume = 0.5f }, position);

            // Heroic impact + screen distortion
            EroicaVFXLibrary.HeroicImpact(position, 1.5f);
            ScreenDistortionManager.TriggerRipple(position, EroicaPalette.Scarlet, 0.6f, 20);

            // Sun halo — expanding golden ring
            var sunHalo = new BloomRingParticle(position, Vector2.Zero,
                EroicaPalette.Gold * 0.8f, 0.8f, 30, 0.025f);
            MagnumParticleHandler.SpawnParticle(sunHalo);

            // Music note burst
            EroicaVFXLibrary.MusicalImpact(position, 1.2f, true);

            // Lightning bolts
            SpawnLightning(position);

            // Gold dust ring
            EroicaVFXLibrary.SpawnRadialDustBurst(position, 30, 10f, DustID.GoldFlame);

            // Scarlet dust ring
            EroicaVFXLibrary.SpawnRadialDustBurst(position, 25, 8f, DustID.CrimsonTorch);

            // Fire sparks (with gravity)
            for (int i = 0; i < 15; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                Dust.NewDustPerfect(position, DustID.Torch, vel, 0, default, 1.8f);
            }

            Lighting.AddLight(position, EroicaPalette.HotCore.ToVector3() * 1.5f);
        }

        // ══════════════════════════════════════════════════════════════
        //  LIGHTNING EFFECT  (sub-effect of AOE)
        // ══════════════════════════════════════════════════════════════

        /// <summary>Spawn 4 bolt lightning burst at position.</summary>
        public static void SpawnLightning(Vector2 position)
        {
            int boltCount = 4;
            for (int b = 0; b < boltCount; b++)
            {
                float baseAngle = MathHelper.TwoPi * b / boltCount + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 boltDirection = baseAngle.ToRotationVector2();
                Vector2 boltStart = position;

                int segments = Main.rand.Next(6, 10);
                for (int seg = 0; seg < segments; seg++)
                {
                    float segmentLength = Main.rand.NextFloat(15f, 25f);
                    float angleOffset = Main.rand.NextFloat(-0.5f, 0.5f);
                    boltDirection = boltDirection.RotatedBy(angleOffset);
                    Vector2 boltEnd = boltStart + boltDirection * segmentLength;

                    for (int p = 0; p < 5; p++)
                    {
                        float lerp = p / 5f;
                        Vector2 particlePos = Vector2.Lerp(boltStart, boltEnd, lerp);

                        if (p % 2 == 0)
                        {
                            Dust smoke = Dust.NewDustPerfect(particlePos, DustID.Smoke,
                                Main.rand.NextVector2Circular(1f, 1f), 200, Color.Black, 1.5f);
                            smoke.noGravity = true;
                        }
                        else
                        {
                            Dust crimson = Dust.NewDustPerfect(particlePos, DustID.CrimsonTorch,
                                Main.rand.NextVector2Circular(1f, 1f), 100, default, 1.8f);
                            crimson.noGravity = true;
                        }
                    }

                    boltStart = boltEnd;
                }
            }

            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.5f, Volume = 0.4f }, position);
        }

        // ══════════════════════════════════════════════════════════════
        //  DEATH FLASH  (called from Projectile.OnKill)
        // ══════════════════════════════════════════════════════════════

        /// <summary>Projectile death flash + sound.</summary>
        public static void DeathFlash(Vector2 center)
        {
            EroicaVFXLibrary.DeathHeroicFlash(center, 0.7f);
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f }, center);
        }

        // ══════════════════════════════════════════════════════════════
        //  PROJECTILE PREDRAW  (called from Projectile.PreDraw)
        // ══════════════════════════════════════════════════════════════

        /// <summary>Full projectile rendering: {A=0} bloom trail, bloom stack, main sprite.</summary>
        public static bool DrawProjectile(SpriteBatch sb, Projectile proj, ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[proj.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
            Vector2 projScreen = proj.Center - Main.screenPosition;

            // {A=0} bloom trail — replaces old prismatic gem trail + additive pass
            EroicaVFXLibrary.DrawProjectileTrail(sb, proj, EroicaPalette.Scarlet);

            // Afterimage trail with {A=0} additive colors
            for (int k = 0; k < proj.oldPos.Length; k++)
            {
                if (proj.oldPos[k] == Vector2.Zero) continue;

                Vector2 drawPos = proj.oldPos[k] - Main.screenPosition + new Vector2(proj.width / 2f, proj.height / 2f);
                float progress = (proj.oldPos.Length - k) / (float)proj.oldPos.Length;
                Color trailColor = Color.Lerp(EroicaPalette.DeepScarlet, EroicaPalette.Gold, progress);
                trailColor = (trailColor * progress) with { A = 0 };
                float scale = proj.scale * (0.5f + progress * 0.5f);

                sb.Draw(texture, drawPos, null, trailColor, proj.oldRot[k], drawOrigin, scale, SpriteEffects.None, 0f);
            }

            // {A=0} bloom stack — replaces old 4-offset glow
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.08f + 1f;
            Color outerGlow = (EroicaPalette.Scarlet with { A = 0 }) * 0.4f;
            Color midGlow = (EroicaPalette.Gold with { A = 0 }) * 0.35f;
            Color innerGlow = (EroicaPalette.HotCore with { A = 0 }) * 0.25f;

            sb.Draw(texture, projScreen, null, outerGlow, proj.rotation, drawOrigin,
                proj.scale * 1.25f * pulse, SpriteEffects.None, 0f);
            sb.Draw(texture, projScreen, null, midGlow, proj.rotation, drawOrigin,
                proj.scale * 1.12f * pulse, SpriteEffects.None, 0f);
            sb.Draw(texture, projScreen, null, innerGlow, proj.rotation, drawOrigin,
                proj.scale * 1.04f * pulse, SpriteEffects.None, 0f);

            // Main sprite
            Color mainColor = new Color(255, 240, 220, 220);
            sb.Draw(texture, projScreen, null, mainColor, proj.rotation, drawOrigin, proj.scale, SpriteEffects.None, 0f);

            return false;
        }
    }
}
