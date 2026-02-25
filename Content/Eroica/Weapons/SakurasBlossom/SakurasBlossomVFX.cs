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
using static MagnumOpus.Common.Systems.VFX.GodRaySystem;

namespace MagnumOpus.Content.Eroica.Weapons.SakurasBlossom
{
    /// <summary>
    /// Static VFX helper for Sakura's Blossom — all visual effects consolidated here.
    /// Game logic (projectile spawning, debuffs, seeking crystals) stays in the original files.
    /// All VFX routed through EroicaVFXLibrary for canonical palette + modern systems.
    ///
    /// Sakura's Blossom is a 4-phase blooming combo sword. Each swing unfurls petals —
    /// delicate yet fierce, grace woven with heroic fire. The visual language is cherry
    /// blossoms in a spring storm: soft pinks erupting into golden pollen bursts.
    /// </summary>
    public static class SakurasBlossomVFX
    {
        // ══════════════════════════════════════════════════════════════
        //  SAKURA ACCENT PALETTE  (weapon-specific identity colors)
        // ══════════════════════════════════════════════════════════════

        /// <summary>Unopened bud, darkest petal — shadow undertone for trail ends.</summary>
        private static readonly Color BudCrimson = new Color(120, 25, 45);

        /// <summary>Full bloom sakura — primary readable pink.</summary>
        private static readonly Color BloomPink = new Color(255, 130, 165);

        /// <summary>Golden pollen motes — delegates to EroicaPalette canonical gold.</summary>
        private static readonly Color PollenGold = EroicaPalette.PollenGold;

        /// <summary>White-hot petal center — sforzando flare.</summary>
        private static readonly Color PetalWhite = new Color(255, 240, 235);

        /// <summary>New leaf accent — spring foliage undertone.</summary>
        private static readonly Color SpringGreen = new Color(140, 200, 120);

        /// <summary>Deep blossom heart — inner core of each petal.</summary>
        private static readonly Color BlossomCore = new Color(220, 80, 110);

        /// <summary>Floating petal edge — soft drifting petal trail color.</summary>
        private static readonly Color PetalDrifter = new Color(255, 180, 200);

        /// <summary>Sun-touched petal tip — warm highlight where light kisses blossom.</summary>
        private static readonly Color SunlitPetal = new Color(255, 220, 180);

        // ══════════════════════════════════════════════════════════════
        //  1. PER-PHASE SWING VFX  (called from HandleComboSpecials)
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Phase 0 — Petal Slash: quick opener with a scatter of cherry petals,
        /// a small bloom flare, and directional sparks in sakura hues.
        /// </summary>
        public static void PetalSlashVFX(Vector2 tipPos, Vector2 swordDir)
        {
            EroicaVFXLibrary.BloomFlare(tipPos, BloomPink, 0.55f, 14);
            EroicaVFXLibrary.SpawnGradientHaloRings(tipPos, 2, 0.3f);
            EroicaVFXLibrary.SpawnSakuraPetals(tipPos, 3, 25f);
            EroicaVFXLibrary.SpawnDirectionalSparks(tipPos, swordDir, 5, 5f);
            EroicaVFXLibrary.SpawnSakuraMusicNotes(tipPos, 1, 10f);

            Lighting.AddLight(tipPos, BloomPink.ToVector3() * 0.7f);
        }

        /// <summary>
        /// Phase 1 — Crimson Scatter: wider backhand scatter with dual-color petal burst
        /// (BudCrimson + BloomPink), directional sparks, and sakura music notes.
        /// </summary>
        public static void CrimsonScatterVFX(Vector2 tipPos, Vector2 swordDir)
        {
            EroicaVFXLibrary.HeroicImpact(tipPos, 0.8f);
            EroicaVFXLibrary.SpawnSakuraPetals(tipPos, 5, 30f);
            EroicaVFXLibrary.SpawnDirectionalSparks(tipPos, swordDir, 8, 6f);
            EroicaVFXLibrary.SpawnSakuraMusicNotes(tipPos, 2, 20f);

            // Dual-color petal dust — bud crimson mixed with bloom pink
            for (int i = 0; i < 4; i++)
            {
                Color petalCol = Main.rand.NextBool() ? BudCrimson : BloomPink;
                Vector2 vel = swordDir.RotatedBy(Main.rand.NextFloat(-0.6f, 0.6f)) * Main.rand.NextFloat(3f, 6f);
                Dust d = Dust.NewDustPerfect(tipPos, DustID.PinkTorch, vel, 0, petalCol, 1.4f);
                d.noGravity = true;
            }

            Lighting.AddLight(tipPos, BlossomCore.ToVector3() * 0.85f);
        }

        /// <summary>
        /// Phase 2 — Blossom Bloom: rising arc with pollen explosion, golden motes
        /// scattering upward, and expanding bloom rings around the blade tip.
        /// </summary>
        public static void BlossomBloomVFX(Vector2 tipPos)
        {
            EroicaVFXLibrary.BloomFlare(tipPos, PollenGold, 0.75f, 18);
            EroicaVFXLibrary.SpawnGradientHaloRings(tipPos, 4, 0.35f);
            EroicaVFXLibrary.SpawnSakuraPetals(tipPos, 8, 40f);
            EroicaVFXLibrary.SpawnMusicNotes(tipPos, 3, 25f);
            EroicaVFXLibrary.SpawnSakuraMusicNotes(tipPos, 2, 20f);

            // Golden pollen motes — scatter upward like spring wind
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-2f, 2f), -Main.rand.NextFloat(1.5f, 4f));
                Dust p = Dust.NewDustPerfect(tipPos + Main.rand.NextVector2Circular(12f, 12f),
                    DustID.GoldFlame, vel, 0, PollenGold, Main.rand.NextFloat(1.0f, 1.5f));
                p.noGravity = true;
            }

            // Expanding bloom ring — golden halo of pollen
            var bloomRing = new BloomRingParticle(tipPos, Vector2.Zero,
                PollenGold * 0.7f, 0.5f, 25, 0.06f);
            MagnumParticleHandler.SpawnParticle(bloomRing);

            Lighting.AddLight(tipPos, PollenGold.ToVector3() * 1.0f);
        }

        /// <summary>
        /// Phase 3 — Storm of Petals: FINALE. Massive sakura storm with screen shake,
        /// god rays, music note cascade, screen distortion ripple, and dense petal eruption.
        /// The sword's final movement — a full blossom unfurling.
        /// </summary>
        public static void StormOfPetalsVFX(Vector2 tipPos)
        {
            // Screen trauma — the climactic bloom
            MagnumScreenEffects.AddScreenShake(10f);
            ScreenDistortionManager.TriggerRipple(tipPos, BlossomCore, 0.9f, 28);

            // Massive bloom + halo cascade
            EroicaVFXLibrary.FinisherSlam(tipPos, 1.4f);
            EroicaVFXLibrary.SpawnSakuraPetals(tipPos, 14, 60f);

            // God rays — sakura blossoms radiate outward
            GodRaySystem.CreateBurst(tipPos, BlossomCore, 8, 110f, 40, GodRayStyle.Explosion, PollenGold);

            // Music note cascade — sakura hue range, the song of spring
            EroicaVFXLibrary.SpawnSakuraMusicNotes(tipPos, 8, 45f);
            EroicaVFXLibrary.SpawnMusicNotes(tipPos, 4, 35f, 0.9f, 1.2f, 40);

            // Pollen explosion ring
            var stormRing = new BloomRingParticle(tipPos, Vector2.Zero,
                PollenGold * 0.9f, 0.8f, 30, 0.12f);
            MagnumParticleHandler.SpawnParticle(stormRing);

            // Inner blossom core ring
            var coreRing = new BloomRingParticle(tipPos, Vector2.Zero,
                BlossomCore * 0.8f, 0.5f, 25, 0.08f);
            MagnumParticleHandler.SpawnParticle(coreRing);

            Lighting.AddLight(tipPos, PetalWhite.ToVector3() * 1.6f);
        }

        // ══════════════════════════════════════════════════════════════
        //  2. SWING HIT IMPACT  (called from OnSwingHitNPC)
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// On-hit VFX for melee swing — combo-scaled sakura impact with escalating
        /// petal bursts and sakura music notes. Phase 3 hits trigger screen trauma.
        /// Game logic (debuffs, seeking crystals) stays in Swing.cs.
        /// </summary>
        public static void SwingHitImpact(Vector2 center, int comboStep)
        {
            EroicaVFXLibrary.MeleeImpact(center, comboStep);
            EroicaVFXLibrary.SpawnSakuraPetals(center, 3 + comboStep * 2, 30f);
            EroicaVFXLibrary.SpawnSakuraMusicNotes(center, 2 + comboStep, 25f);

            // Phase 3 hits — screen trauma for the climactic blow
            if (comboStep >= 3)
            {
                MagnumScreenEffects.AddScreenShake(4f);
                ScreenDistortionManager.TriggerRipple(center, BlossomCore, 0.4f, 15);
                CustomParticles.EroicaFlare(center, 0.5f);
            }

            Lighting.AddLight(center, BloomPink.ToVector3() * (0.8f + comboStep * 0.15f));
        }

        // ══════════════════════════════════════════════════════════════
        //  3. PER-FRAME SWING TRAIL VFX  (called from DrawCustomVFX)
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Per-frame swing trail: dense sakura petal dust, golden pollen motes drifting
        /// upward, sakura blossom shimmer (hue oscillation via EroicaPalette.GetSakuraShimmer),
        /// sakura music notes, contrast sparkles, and blade-tip bloom glow.
        /// Skips rendering outside the active swing window (progression &lt; 0.08 or &gt; 0.92).
        /// </summary>
        public static void DrawSwingTrailVFX(Vector2 tipPos, Vector2 ownerCenter, Vector2 swordDir, float progression, int comboStep)
        {
            if (progression <= 0.08f || progression >= 0.92f)
                return;

            // Dense sakura petal trail — petals unfurl from the blade edge
            EroicaVFXLibrary.SpawnSwingDust(tipPos, -swordDir, DustID.PinkTorch);

            // Golden pollen motes drifting upward (1/3 chance)
            if (Main.rand.NextBool(3))
            {
                Vector2 pollenPos = tipPos + Main.rand.NextVector2Circular(10f, 10f);
                Dust pollen = Dust.NewDustPerfect(pollenPos, DustID.GoldFlame,
                    new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(0.5f, 2f)),
                    0, PollenGold, 1.1f);
                pollen.noGravity = true;
            }

            // Sakura blossom shimmer — hue oscillation in rose→magenta range (1/3 chance)
            if (Main.rand.NextBool(3))
            {
                Color shimmer = EroicaPalette.GetSakuraShimmer(Main.GameUpdateCount);
                Dust s = Dust.NewDustPerfect(tipPos, DustID.PinkFairy,
                    Main.rand.NextVector2Circular(1.5f, 1.5f), 0, shimmer, 1.2f);
                s.noGravity = true;
            }

            // Sakura music notes — song of blossoming spring (every 5th frame)
            if (Main.GameUpdateCount % 5 == 0)
            {
                EroicaVFXLibrary.SpawnSakuraMusicNotes(tipPos, 1, 8f);
            }

            // Contrast sparkle for dual-colour trail
            EroicaVFXLibrary.SpawnContrastSparkle(tipPos, -swordDir);

            // Spring-green leaf accent (rare, 1/8 chance)
            if (Main.rand.NextBool(8))
            {
                Dust leaf = Dust.NewDustPerfect(tipPos + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.GrassBlades, -swordDir * Main.rand.NextFloat(0.5f, 1.5f),
                    0, SpringGreen, 1.0f);
                leaf.noGravity = true;
            }

            // Blade-tip bloom glow — sakura blossom radiance
            float bloomOpacity = MathHelper.Clamp((progression - 0.08f) / 0.10f, 0f, 1f)
                               * MathHelper.Clamp((0.92f - progression) / 0.10f, 0f, 1f);
            if (bloomOpacity > 0f)
            {
                EroicaVFXLibrary.DrawComboBloom(tipPos, comboStep, 0.45f, bloomOpacity);
            }
        }

        // ══════════════════════════════════════════════════════════════
        //  4. SPECTRAL COPY VFX  (SakurasBlossomSpectral lifecycle)
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Spectral copy spawn VFX — bloom flash, halo rings, and sakura petals
        /// erupting when spectral blade copies materialize from the swing.
        /// </summary>
        public static void SpectralCopySpawnVFX(Vector2 pos, int copyCount)
        {
            float intensity = 0.5f + copyCount * 0.12f;
            EroicaVFXLibrary.BloomFlare(pos, BloomPink, intensity, 16);
            EroicaVFXLibrary.SpawnGradientHaloRings(pos, 2 + copyCount / 2, 0.3f);
            EroicaVFXLibrary.SpawnSakuraPetals(pos, 2 + copyCount, 25f + copyCount * 5f);
            EroicaVFXLibrary.SpawnSakuraMusicNotes(pos, 1 + copyCount / 2, 15f);

            Lighting.AddLight(pos, BloomPink.ToVector3() * (0.6f + copyCount * 0.1f));
        }

        /// <summary>
        /// Per-frame trail VFX for spectral blade copies — sakura flame dust,
        /// petal sparkles, and dynamic lighting following each phantom blade.
        /// </summary>
        public static void SpectralCopyTrailVFX(Projectile proj)
        {
            // Sakura flame dust trail
            if (Main.rand.NextBool(2))
            {
                Vector2 vel = -proj.velocity * 0.2f + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Color col = Color.Lerp(BloomPink, PollenGold, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(proj.Center, DustID.PinkTorch, vel, 0, col, 1.3f);
                d.noGravity = true;
            }

            // Petal sparkles (1/3 chance)
            if (Main.rand.NextBool(3))
            {
                Vector2 sparklePos = proj.Center + Main.rand.NextVector2Circular(8f, 8f);
                Dust s = Dust.NewDustPerfect(sparklePos, DustID.PinkFairy,
                    Main.rand.NextVector2Circular(1f, 1f), 0, PetalDrifter, 1.1f);
                s.noGravity = true;
            }

            // Sakura music notes (1/6 chance)
            if (Main.rand.NextBool(6))
            {
                EroicaVFXLibrary.SpawnSakuraMusicNotes(proj.Center, 1, 8f);
            }

            EroicaVFXLibrary.AddPaletteLighting(proj.Center, 0.5f, 0.6f);
        }

        /// <summary>
        /// Hit impact VFX for spectral blade copies — smaller sakura burst
        /// with petal scatter and music notes.
        /// </summary>
        public static void SpectralCopyHitVFX(Vector2 center)
        {
            EroicaVFXLibrary.HeroicImpact(center, 0.6f);
            EroicaVFXLibrary.SpawnSakuraPetals(center, 3, 20f);
            EroicaVFXLibrary.SpawnSakuraMusicNotes(center, 2, 18f);

            Lighting.AddLight(center, BloomPink.ToVector3() * 0.8f);
        }

        /// <summary>
        /// Death flash VFX for spectral blade copies — petal scatter and
        /// bloom flash as the phantom blade dissolves into cherry blossoms.
        /// </summary>
        public static void SpectralCopyDeathVFX(Vector2 center)
        {
            EroicaVFXLibrary.DeathHeroicFlash(center, 0.5f);
            EroicaVFXLibrary.SpawnSakuraPetals(center, 5, 30f);
            EroicaVFXLibrary.SpawnSakuraMusicNotes(center, 2, 20f);
            SoundEngine.PlaySound(SoundID.Item10 with { Pitch = 0.3f, Volume = 0.4f }, center);

            Lighting.AddLight(center, PetalDrifter.ToVector3() * 0.7f);
        }

        // ══════════════════════════════════════════════════════════════
        //  5. PROJECTILE PREDRAW  (called from Projectile.PreDraw)
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Full spectral copy rendering: {A=0} bloom trail in sakura colors,
        /// afterimage trail with BudCrimson→PollenGold gradient, 3-layer bloom
        /// stack (BlossomCore outer, BloomPink mid, PetalWhite core), and main
        /// sprite with sakura tint. Returns false to suppress vanilla drawing.
        /// </summary>
        public static bool DrawSpectralCopy(SpriteBatch sb, Projectile proj, ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[proj.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
            Vector2 projScreen = proj.Center - Main.screenPosition;

            // {A=0} bloom trail — sakura colors
            EroicaVFXLibrary.DrawProjectileTrail(sb, proj, BloomPink);

            // Afterimage trail with BudCrimson → PollenGold gradient
            for (int k = 0; k < proj.oldPos.Length; k++)
            {
                if (proj.oldPos[k] == Vector2.Zero) continue;

                Vector2 drawPos = proj.oldPos[k] - Main.screenPosition + new Vector2(proj.width / 2f, proj.height / 2f);
                float progress = (proj.oldPos.Length - k) / (float)proj.oldPos.Length;
                Color trailColor = Color.Lerp(BudCrimson, PollenGold, progress);
                trailColor = (trailColor * progress) with { A = 0 };
                float scale = proj.scale * (0.5f + progress * 0.5f);

                sb.Draw(texture, drawPos, null, trailColor, proj.oldRot[k], drawOrigin, scale, SpriteEffects.None, 0f);
            }

            // 3-layer {A=0} bloom stack — BlossomCore outer, BloomPink mid, PetalWhite core
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.08f + 1f;
            Color outerGlow = (BlossomCore with { A = 0 }) * 0.4f;
            Color midGlow = (BloomPink with { A = 0 }) * 0.35f;
            Color innerGlow = (PetalWhite with { A = 0 }) * 0.25f;

            sb.Draw(texture, projScreen, null, outerGlow, proj.rotation, drawOrigin,
                proj.scale * 1.25f * pulse, SpriteEffects.None, 0f);
            sb.Draw(texture, projScreen, null, midGlow, proj.rotation, drawOrigin,
                proj.scale * 1.12f * pulse, SpriteEffects.None, 0f);
            sb.Draw(texture, projScreen, null, innerGlow, proj.rotation, drawOrigin,
                proj.scale * 1.04f * pulse, SpriteEffects.None, 0f);

            // Main sprite with warm sakura tint
            Color mainColor = new Color(255, 235, 230, 220);
            sb.Draw(texture, projScreen, null, mainColor, proj.rotation, drawOrigin, proj.scale, SpriteEffects.None, 0f);

            return false;
        }

        // ══════════════════════════════════════════════════════════════
        //  6. AMBIENT HOLD VFX  (called from HoldItem)
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Gentle sakura aura while holding the weapon: orbiting petal motes,
        /// drifting pollen dust, subtle sakura music notes, spring-green leaf
        /// accents, and pulsing sakura light around the player.
        /// </summary>
        public static void HoldItemVFX(Player player)
        {
            if (Main.gameMenu) return;

            // Heroic ambient aura — pulsing embers
            EroicaVFXLibrary.SpawnHeroicAura(player.Center, 32f);

            // Orbiting sakura petal motes
            if (Main.rand.NextBool(15))
            {
                float angle = Main.GameUpdateCount * 0.03f;
                float radius = 30f + (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 10f;
                Vector2 flarePos = player.Center + angle.ToRotationVector2() * radius;
                EroicaVFXLibrary.SpawnSakuraMusicNotes(flarePos, 1, 5f);
            }

            // Drifting sakura petals
            if (Main.rand.NextBool(12))
            {
                EroicaVFXLibrary.SpawnSakuraPetals(player.Center + Main.rand.NextVector2Circular(25f, 25f), 1, 20f);
            }

            // Spring-green leaf accents (rare)
            if (Main.rand.NextBool(30))
            {
                Dust leaf = Dust.NewDustPerfect(
                    player.Center + Main.rand.NextVector2Circular(28f, 28f),
                    DustID.GrassBlades,
                    new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(0.3f, 0.8f)),
                    0, SpringGreen, 0.9f);
                leaf.noGravity = true;
            }

            // Pollen sparkles
            if (Main.rand.NextBool(15))
            {
                EroicaVFXLibrary.SpawnValorSparkles(player.Center + Main.rand.NextVector2Circular(25f, 25f), 1, 10f);
            }

            // Pulsing sakura light — pink to gold oscillation
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.9f;
            Vector3 lightCol = Color.Lerp(BloomPink, PollenGold, 0.4f).ToVector3();
            Lighting.AddLight(player.Center, lightCol * pulse * 0.55f);
        }
    }
}
