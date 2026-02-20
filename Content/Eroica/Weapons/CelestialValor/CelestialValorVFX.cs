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
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Content.Eroica;
using static MagnumOpus.Common.Systems.Particles.Particle;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor
{
    /// <summary>
    /// Static VFX helper for CelestialValor — all visual effects consolidated here.
    /// Game logic (projectile spawning, debuffs, AOE damage) stays in the original files.
    /// </summary>
    public static class CelestialValorVFX
    {
        // ── Color Shorthands ──────────────────────────────────────────
        private static Color EroicaScarlet => EroicaPalette.Scarlet;
        private static Color EroicaCrimson => EroicaPalette.BladeCrimson;
        private static Color EroicaGold => EroicaPalette.Gold;
        private static Color SakuraPink => EroicaPalette.Sakura;

        // ══════════════════════════════════════════════════════════════
        //  SWING PHASE VFX  (called from HandleComboSpecials)
        // ══════════════════════════════════════════════════════════════

        /// <summary>Phase 0 — quick opener VFX at blade tip.</summary>
        public static void SwingPhase0VFX(Vector2 tipPos, Vector2 swordDir)
        {
            CustomParticles.GenericFlare(tipPos, EroicaGold, 0.6f, 15);
            CustomParticles.HaloRing(tipPos, EroicaScarlet, 0.35f, 12);
            ThemedParticles.EroicaSparks(tipPos, swordDir, 5, 6f);
        }

        /// <summary>Phase 1 — main slash VFX with sakura petals.</summary>
        public static void SwingPhase1VFX(Vector2 tipPos, Vector2 swordDir)
        {
            CustomParticles.GenericFlare(tipPos, Color.White, 0.7f, 18);
            CustomParticles.GenericFlare(tipPos, EroicaScarlet, 0.55f, 16);
            CustomParticles.HaloRing(tipPos, EroicaCrimson, 0.4f, 14);
            ThemedParticles.EroicaSparks(tipPos, swordDir, 8, 7f);
            ThemedParticles.SakuraPetals(tipPos, 3, 30f);
        }

        /// <summary>Phase 2 — finisher VFX with full impact cascade + screen shake.</summary>
        public static void SwingPhase2VFX(Vector2 tipPos)
        {
            UnifiedVFX.Eroica.Impact(tipPos, 1.3f);
            CustomParticles.GenericFlare(tipPos, Color.White, 1.0f, 22);
            CustomParticles.GenericFlare(tipPos, EroicaGold, 0.8f, 20);

            for (int i = 0; i < 4; i++)
            {
                Color ringColor = Color.Lerp(EroicaScarlet, EroicaGold, i / 4f);
                CustomParticles.HaloRing(tipPos, ringColor, 0.35f + i * 0.1f, 14 + i * 2);
            }

            ThemedParticles.SakuraPetals(tipPos, 6, 45f);
            ThemedParticles.EroicaMusicNotes(tipPos, 5, 35f);
            MagnumScreenEffects.AddScreenShake(6f);
        }

        // ══════════════════════════════════════════════════════════════
        //  SWING HIT IMPACT  (called from OnSwingHitNPC)
        // ══════════════════════════════════════════════════════════════

        /// <summary>On-hit VFX for melee swing (game logic like debuffs stays in Swing.cs).</summary>
        public static void SwingHitImpact(Vector2 center, int comboStep)
        {
            float impactScale = 0.8f + comboStep * 0.25f;
            UnifiedVFX.Eroica.Impact(center, impactScale);

            int ringCount = 2 + comboStep;
            for (int ring = 0; ring < ringCount; ring++)
            {
                Color ringColor = Color.Lerp(EroicaScarlet, EroicaGold, (float)ring / ringCount);
                CustomParticles.HaloRing(center, ringColor, 0.3f + ring * 0.1f, 13 + ring * 2);
            }

            int dustCount = 8 + comboStep * 4;
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                int dustType = i % 2 == 0 ? DustID.GoldFlame : DustID.RedTorch;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Dust d = Dust.NewDustPerfect(center, dustType, vel, 0, default, 1.5f);
                d.noGravity = true;
            }

            ThemedParticles.SakuraPetals(center, 2 + comboStep, 30f);
            ThemedParticles.EroicaMusicNotes(center, 3 + comboStep, 30f);
            Lighting.AddLight(center, 1.0f, 0.5f, 0.25f);
        }

        // ══════════════════════════════════════════════════════════════
        //  SWING TRAIL VFX  (called from DrawCustomVFX every frame)
        // ══════════════════════════════════════════════════════════════

        /// <summary>Per-frame swing trail: ember dust, sakura sparkles, valor shimmer, music notes, bloom.</summary>
        public static void DrawSwingTrailVFX(Vector2 tipPos, Vector2 ownerCenter, Vector2 swordDir, float progression, int comboStep)
        {
            if (progression <= 0.08f || progression >= 0.92f)
                return;

            // ── Dense ember trail (2 per frame) ──
            for (int i = 0; i < 2; i++)
            {
                float dustProgress = Main.rand.NextFloat();
                Color dustColor = Color.Lerp(EroicaScarlet, EroicaGold, dustProgress);
                int dustType = dustProgress < 0.5f ? DustID.RedTorch : DustID.GoldFlame;
                Vector2 pos = Vector2.Lerp(ownerCenter, tipPos, Main.rand.NextFloat(0.4f, 1f));
                Vector2 vel = -swordDir * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(1f, 1f);
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, dustColor, 1.6f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // ── Sakura sparkles (1/3 chance) ──
            if (Main.rand.NextBool(3))
            {
                Vector2 sparklePos = tipPos + Main.rand.NextVector2Circular(8f, 8f);
                Vector2 sparkleVel = -swordDir * Main.rand.NextFloat(0.5f, 2f);
                Dust s = Dust.NewDustPerfect(sparklePos, DustID.PinkFairy, sparkleVel, 0, SakuraPink, 1.2f);
                s.noGravity = true;
            }

            // ── Valor shimmer (1/3 chance) ──
            if (Main.rand.NextBool(3))
            {
                float valorHue = 0.02f + Main.rand.NextFloat() * 0.1f;
                Color valorColor = Main.hslToRgb(valorHue, 1f, 0.65f);
                Dust v = Dust.NewDustPerfect(tipPos, DustID.GoldFlame, Main.rand.NextVector2Circular(1.5f, 1.5f), 0, valorColor, 1.3f);
                v.noGravity = true;
            }

            // ── Music notes (1/5 chance) ──
            if (Main.rand.NextBool(5))
            {
                float shimmer = 1f + MathF.Sin(Main.GameUpdateCount * 0.15f) * 0.12f;
                Vector2 noteVel = -swordDir * 1.5f + new Vector2(0, -0.5f);
                var note = new HueShiftingMusicNoteParticle(
                    tipPos, noteVel,
                    hueMin: 0.0f, hueMax: 0.12f,
                    saturation: 0.95f, luminosity: 0.60f,
                    scale: 0.75f * shimmer, lifetime: 28, hueSpeed: 0.02f);
                MagnumParticleHandler.SpawnParticle(note);
            }

            // ── Bloom at blade tip ──
            float bloomOpacity = MathHelper.Clamp((progression - 0.08f) / 0.10f, 0f, 1f)
                               * MathHelper.Clamp((0.92f - progression) / 0.10f, 0f, 1f);
            if (bloomOpacity > 0f)
            {
                BloomRenderer.DrawBloomStackAdditive(
                    tipPos, EroicaScarlet, EroicaGold,
                    scale: 0.45f + comboStep * 0.08f,
                    opacity: bloomOpacity);
            }
        }

        // ══════════════════════════════════════════════════════════════
        //  PROJECTILE TRAIL VFX  (called from Projectile.AI)
        // ══════════════════════════════════════════════════════════════

        /// <summary>Per-frame projectile trail particles.</summary>
        public static void ProjectileTrailVFX(Projectile proj)
        {
            // Sword arc wave (1/4 chance)
            if (Main.rand.NextBool(4))
            {
                CustomParticles.SwordArcWave(proj.Center, proj.velocity * 0.15f,
                    CustomParticleSystem.EroicaColors.Gold * 0.8f, 0.35f);
            }

            // Dust trail (1/3 chance)
            if (Main.rand.NextBool(3))
            {
                int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                Vector2 pos = new Vector2(
                    proj.position.X + Main.rand.Next(proj.width),
                    proj.position.Y + Main.rand.Next(proj.height));
                Vector2 vel = -proj.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, default, 1.2f);
                d.noGravity = true;
            }

            // Eroica music trail (1/2 chance)
            if (Main.rand.NextBool(2))
            {
                ThemedParticles.EroicaMusicTrail(proj.Center, proj.velocity);
            }

            // Eroica flare (1/3 chance)
            if (Main.rand.NextBool(3))
            {
                CustomParticles.EroicaFlare(proj.Center, 0.45f);
            }

            Lighting.AddLight(proj.Center, 1f, 0.6f, 0.3f);
        }

        // ══════════════════════════════════════════════════════════════
        //  PROJECTILE HIT VFX  (called from Projectile.OnHitNPC)
        // ══════════════════════════════════════════════════════════════

        /// <summary>Bloom + impact when projectile hits an NPC (game logic stays in Projectile.cs).</summary>
        public static void ProjectileHitVFX(Vector2 center)
        {
            EnhancedParticles.BloomFlare(center, Color.White, 0.7f, 20, 4, 1.0f);
            EnhancedParticles.BloomFlare(center, ThemedParticles.EroicaGold, 0.55f, 18, 3, 0.85f);
            UnifiedVFXBloom.Eroica.ImpactEnhanced(center, 0.8f);
            EnhancedThemedParticles.EroicaMusicNotesEnhanced(center, 5, 32f);
        }

        // ══════════════════════════════════════════════════════════════
        //  AOE EXPLOSION  (called from CreateAOEExplosion)
        // ══════════════════════════════════════════════════════════════

        /// <summary>Full AOE explosion VFX: sound, particles, dust rings, lightning.</summary>
        public static void AOEExplosion(Vector2 position)
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.3f, Volume = 0.5f }, position);

            CustomParticles.MagicSparkleFieldBurst(position, CustomParticleSystem.EroicaColors.Gold, 5, 30f);
            CustomParticles.SwordArcBurst(position, CustomParticleSystem.EroicaColors.Scarlet, 5, 0.4f);

            // Sun halo — expanding golden ring
            var sunHalo = new BloomRingParticle(position, Vector2.Zero, new Color(255, 220, 100) * 0.8f, 0.8f, 30, 0.025f);
            MagnumParticleHandler.SpawnParticle(sunHalo);

            CustomParticles.PrismaticSparkleBurst(position, CustomParticleSystem.EroicaColors.Gold, 8);
            SpawnLightning(position);
            ThemedParticles.EroicaMusicalImpact(position, 1.2f, true);

            // 30 gold dust ring
            for (int i = 0; i < 30; i++)
            {
                float angle = MathHelper.TwoPi * i / 30f;
                float speed = Main.rand.NextFloat(6f, 12f);
                Vector2 vel = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed;
                Dust d = Dust.NewDustPerfect(position, DustID.GoldFlame, vel, 0, default, 2.5f);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }

            // 25 scarlet dust ring
            for (int i = 0; i < 25; i++)
            {
                float angle = MathHelper.TwoPi * i / 25f;
                float speed = Main.rand.NextFloat(4f, 9f);
                Vector2 vel = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed;
                Dust d = Dust.NewDustPerfect(position, DustID.CrimsonTorch, vel, 0, default, 2.2f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // 15 fire sparks (with gravity)
            for (int i = 0; i < 15; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                Dust.NewDustPerfect(position, DustID.Torch, vel, 0, default, 1.8f);
            }

            Lighting.AddLight(position, 1.5f, 0.8f, 0.3f);
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
            DynamicParticleEffects.EroicaDeathHeroicFlash(center, 0.7f);
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f }, center);
        }

        // ══════════════════════════════════════════════════════════════
        //  PROJECTILE PREDRAW  (called from Projectile.PreDraw)
        // ══════════════════════════════════════════════════════════════

        /// <summary>Full projectile rendering: prismatic gem trail, glow layers, main sprite. Returns false to suppress default draw.</summary>
        public static bool DrawProjectile(SpriteBatch sb, Projectile proj, ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[proj.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);

            // ── Additive prismatic gem pass ──
            MagnumVFX.BeginAdditiveBlend(sb);
            MagnumVFX.DrawPrismaticGemTrail(sb, proj.oldPos, true, 0.4f, (float)proj.timeLeft);
            MagnumVFX.DrawEroicaPrismaticGem(sb, proj.Center, 0.7f, 0.9f, (float)proj.timeLeft);
            MagnumVFX.EndAdditiveBlend(sb);

            // ── Afterimage trail ──
            for (int k = 0; k < proj.oldPos.Length; k++)
            {
                if (proj.oldPos[k] == Vector2.Zero) continue;

                Vector2 drawPos = proj.oldPos[k] - Main.screenPosition + new Vector2(proj.width / 2f, proj.height / 2f);
                float progress = (proj.oldPos.Length - k) / (float)proj.oldPos.Length;
                Color trailColor = Color.Lerp(new Color(200, 50, 30, 80), new Color(255, 220, 100, 120), progress) * progress;
                float scale = proj.scale * (0.5f + progress * 0.5f);

                sb.Draw(texture, drawPos, null, trailColor, proj.oldRot[k], drawOrigin, scale, SpriteEffects.None, 0f);
            }

            // ── 4-offset glow ──
            Color glowColor = new Color(255, 200, 100, 0) * 0.5f;
            Vector2 projScreen = proj.Center - Main.screenPosition;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(4f, 0).RotatedBy(MathHelper.PiOver2 * i);
                sb.Draw(texture, projScreen + offset, null, glowColor, proj.rotation, drawOrigin, proj.scale * 1.2f, SpriteEffects.None, 0f);
            }

            // ── Main sprite ──
            Color mainColor = new Color(255, 240, 220, 220);
            sb.Draw(texture, projScreen, null, mainColor, proj.rotation, drawOrigin, proj.scale, SpriteEffects.None, 0f);

            return false;
        }
    }
}
