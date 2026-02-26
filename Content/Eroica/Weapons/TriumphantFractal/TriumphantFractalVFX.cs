using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX.Screen;

namespace MagnumOpus.Content.Eroica.Weapons.TriumphantFractal
{
    /// <summary>
    /// Static VFX helper for TriumphantFractal — "Victory branches infinitely."
    /// Post-Moon Lord magic staff (518 dmg), fires 3 fractal projectiles in a spread.
    /// Identity: recursive mathematical beauty meets heroic triumph.
    /// Game logic stays in original files; all visuals consolidated here.
    /// All VFX routed through EroicaVFXLibrary for canonical palette + modern systems.
    /// </summary>
    public static class TriumphantFractalVFX
    {
        // ══════════════════════════════════════════════════════════════
        //  UNIQUE IDENTITY COLORS
        // ══════════════════════════════════════════════════════════════

        private static readonly Color FractalGold = new Color(255, 210, 80);       // Primary fractal energy
        private static readonly Color HexagonScarlet = new Color(200, 55, 45);     // Hexagonal frame color
        private static readonly Color InfinityWhite = new Color(255, 250, 240);    // White-hot convergence
        private static readonly Color CrystalCrimson = new Color(180, 40, 60);     // Crystal facet accent
        private static readonly Color GeometryViolet = new Color(160, 80, 180);    // Mathematical harmony
        private static readonly Color BranchGold = new Color(240, 190, 100);       // Fractal branch limbs
        private static readonly Color NodeCore = new Color(255, 240, 200);         // Fractal tree node centers
        private static readonly Color RecursionFade = new Color(120, 60, 80);      // Fading recursive depth

        // ══════════════════════════════════════════════════════════════
        //  1. CAST GEOMETRY VFX
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Hexagonal geometry burst on cast — rotating hex frame, inner triangle,
        /// connecting dust chains, central bloom, music notes, sakura petals, halo rings.
        /// </summary>
        public static void CastGeometryBurst(Vector2 position, float baseRotation)
        {
            SpawnHexagonalDust(position, 40f, baseRotation, FractalGold, 1.4f);
            SpawnTriangularDust(position, 22f, baseRotation + MathHelper.Pi / 6f, InfinityWhite * 0.9f, 1.2f);

            // Connecting lines between hex and tri points (dust particle chains)
            for (int i = 0; i < 6; i++)
            {
                float hexAngle = MathHelper.TwoPi * i / 6f + baseRotation;
                Vector2 hexPoint = position + hexAngle.ToRotationVector2() * 40f;
                float triAngle = MathHelper.TwoPi * (i % 3) / 3f + baseRotation + MathHelper.Pi / 6f;
                Vector2 triPoint = position + triAngle.ToRotationVector2() * 22f;

                for (int s = 1; s < 4; s++)
                {
                    float lerp = s / 4f;
                    Vector2 linkPos = Vector2.Lerp(hexPoint, triPoint, lerp);
                    Dust link = Dust.NewDustPerfect(linkPos, DustID.GoldFlame,
                        Vector2.Zero, 0, Color.Lerp(FractalGold, HexagonScarlet, lerp), 0.9f);
                    link.noGravity = true;
                }
            }

            // Central bloom, notes, petals, halos
            EroicaVFXLibrary.BloomFlare(position, InfinityWhite, 0.8f, 22);
            EroicaVFXLibrary.MusicNoteBurst(position, FractalGold, 4, 3f);
            EroicaVFXLibrary.SpawnSakuraPetals(position, 4, 45f);
            EroicaVFXLibrary.SpawnGradientHaloRings(position, 2, 0.5f);

            // Screen-space geometry lines — fractal branches fanning outward
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f + baseRotation;
                SpawnFractalBranch(position, angle.ToRotationVector2(), 3, FractalGold, HexagonScarlet);
            }

            Lighting.AddLight(position, InfinityWhite.ToVector3() * 1.2f);
        }

        // ══════════════════════════════════════════════════════════════
        //  2. FRACTAL PROJECTILE VFX
        // ══════════════════════════════════════════════════════════════

        /// <summary>Per-frame projectile trail: recursive branch dust, geometry node sparkles,
        /// hexagonal orbit motes, music notes, dynamic lighting.</summary>
        public static void ProjectileTrailVFX(Projectile proj)
        {
            Vector2 center = proj.Center;
            int frameCount = (int)proj.localAI[0];

            // Fractal branch splitting — spawns 2 sub-trails at 30deg every 8 frames
            if (frameCount % 8 == 0 && frameCount > 0)
            {
                Vector2 trailDir = Vector2.Normalize(proj.velocity);
                float branchAngle = MathHelper.ToRadians(30);
                SpawnFractalBranch(center, trailDir.RotatedBy(-branchAngle), 3, FractalGold, RecursionFade);
                SpawnFractalBranch(center, trailDir.RotatedBy(branchAngle), 3, FractalGold, RecursionFade);

                // Node sparkle at branch point
                Dust node = Dust.NewDustPerfect(center, DustID.GoldFlame,
                    Main.rand.NextVector2Circular(0.5f, 0.5f), 0, NodeCore, 1.5f);
                node.noGravity = true;
            }

            // Main trail dust
            if (Main.rand.NextBool(2))
            {
                Dust trail = Dust.NewDustPerfect(center, DustID.GoldFlame,
                    -proj.velocity * 0.15f + Main.rand.NextVector2Circular(0.5f, 0.5f), 0, FractalGold, 1.3f);
                trail.noGravity = true;
            }

            // Hexagonal orbit points (3 motes orbiting projectile)
            float orbitTime = Main.GameUpdateCount * 0.12f;
            for (int i = 0; i < 3; i++)
            {
                float angle = MathHelper.TwoPi * i / 3f + orbitTime;
                float radius = 12f + MathF.Sin(orbitTime * 2f + i) * 3f;
                Vector2 motePos = center + angle.ToRotationVector2() * radius;
                Dust mote = Dust.NewDustPerfect(motePos, DustID.GoldFlame,
                    proj.velocity * 0.5f, 0, Color.Lerp(FractalGold, HexagonScarlet, (float)i / 3f), 0.7f);
                mote.noGravity = true;
            }

            // Music notes (1/6 chance)
            if (Main.rand.NextBool(6))
                EroicaVFXLibrary.SpawnMusicNotes(center, 1, 10f, 0.6f, 0.8f, 25);

            // Dynamic lighting
            float pulse = 0.85f + MathF.Sin(Main.GlobalTimeWrappedHourly * 8f) * 0.15f;
            Lighting.AddLight(center, FractalGold.ToVector3() * 0.6f * pulse);
        }

        /// <summary>Fractal explosion on hit — hex burst, recursive sub-bursts,
        /// bloom cascade, halo rings, music notes, screen ripple.</summary>
        public static void ProjectileHitVFX(Vector2 hitPos)
        {
            SpawnHexagonalDust(hitPos, 30f, Main.rand.NextFloat(MathHelper.TwoPi), HexagonScarlet, 1.6f);
            RecursiveFractalBurst(hitPos, 0, 30f, Main.rand.NextFloat(MathHelper.TwoPi));

            // Bloom cascade (3 layers: RecursionFade outer, Scarlet mid, Gold inner)
            MagnumParticleHandler.SpawnParticle(new BloomRingParticle(
                hitPos, Vector2.Zero, RecursionFade * 0.8f, 0.6f, 25, 0.10f));
            MagnumParticleHandler.SpawnParticle(new BloomRingParticle(
                hitPos, Vector2.Zero, HexagonScarlet * 0.9f, 0.4f, 20, 0.07f));
            MagnumParticleHandler.SpawnParticle(new BloomRingParticle(
                hitPos, Vector2.Zero, FractalGold, 0.25f, 15, 0.04f));

            EroicaVFXLibrary.SpawnGradientHaloRings(hitPos, 4, 0.35f);
            EroicaVFXLibrary.SpawnMusicNotes(hitPos, 4, 30f, 0.7f, 1.0f, 30);
            ScreenDistortionManager.TriggerRipple(hitPos, HexagonScarlet, 0.5f, 18);
            Lighting.AddLight(hitPos, InfinityWhite.ToVector3() * 1.4f);
        }

        /// <summary>Fractal dissolution on death — geometry scatters, golden bloom, note pair.</summary>
        public static void ProjectileDeathVFX(Vector2 pos)
        {
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Dust scatter = Dust.NewDustPerfect(pos, DustID.GoldFlame,
                    vel, 0, Color.Lerp(FractalGold, RecursionFade, Main.rand.NextFloat()), 1.4f);
                scatter.noGravity = true;
                scatter.fadeIn = 0.8f;
            }

            EroicaVFXLibrary.DrawBloom(pos, 0.5f);
            EroicaVFXLibrary.BloomFlare(pos, FractalGold, 0.4f, 14);
            EroicaVFXLibrary.SpawnMusicNotes(pos, 2, 15f, 0.7f, 0.9f, 30);
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f }, pos);
            Lighting.AddLight(pos, FractalGold.ToVector3() * 0.8f);
        }

        // ══════════════════════════════════════════════════════════════
        //  3. RECURSIVE FRACTAL BURST
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Recursive method (max depth 3): at each depth, spawn geometry points in a
        /// hexagonal pattern; each point becomes center of a smaller hexagon.
        /// Colors fade with depth via RecursionFade. Dust alternates GoldFlame/CrimsonTorch.
        /// </summary>
        public static void RecursiveFractalBurst(Vector2 center, int depth, float radius, float baseAngle)
        {
            if (depth >= 3) return;

            int pointCount = (depth == 0) ? 6 : 3;
            float depthFade = 1f - depth * 0.3f;
            int dustType = (depth % 2 == 0) ? DustID.GoldFlame : DustID.CrimsonTorch;

            for (int i = 0; i < pointCount; i++)
            {
                float angle = MathHelper.TwoPi * i / pointCount + baseAngle;
                Vector2 point = center + angle.ToRotationVector2() * radius;

                Color pointColor = Color.Lerp(FractalGold, RecursionFade, depth / 3f);
                pointColor = Color.Lerp(pointColor, HexagonScarlet, (float)i / pointCount);
                float scale = (1.5f - depth * 0.3f) * depthFade;

                Dust d = Dust.NewDustPerfect(point, dustType,
                    (point - center) * 0.08f, 0, pointColor, scale);
                d.noGravity = true;

                // Node sparkle at branch convergence (depths 0-1 only)
                if (depth < 2)
                {
                    Dust node = Dust.NewDustPerfect(point, DustID.GoldFlame,
                        Vector2.Zero, 0, NodeCore * depthFade, 0.6f);
                    node.noGravity = true;
                }

                // Recurse — each point becomes center of a smaller hexagon
                RecursiveFractalBurst(point, depth + 1, radius * 0.45f,
                    baseAngle + MathHelper.Pi / pointCount);
            }
        }

        // ══════════════════════════════════════════════════════════════
        //  4. PROJECTILE PREDRAW
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Full fractal projectile rendering: {A=0} trail with FractalGold→HexagonScarlet
        /// gradient, geometry overlay, 4-layer bloom stack, pulsing rotation, counter-flares.
        /// </summary>
        public static bool DrawFractalProjectile(SpriteBatch sb, Projectile proj, ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[proj.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
            Vector2 projScreen = proj.Center - Main.screenPosition;

            // {A=0} bloom trail
            EroicaVFXLibrary.DrawProjectileTrail(sb, proj, FractalGold);

            // Shader-enhanced fractal trail pass
            {
                Texture2D shaderGlow = MagnumTextureRegistry.GetSoftGlow();
                EroicaShaderManager.BeginShaderAdditive(sb);
                EroicaShaderManager.ApplyTriumphantFractalProjectileTrail(Main.GlobalTimeWrappedHourly);
                Vector2 glowOrigin = shaderGlow.Size() * 0.5f;
                for (int k = 0; k < proj.oldPos.Length; k++)
                {
                    if (proj.oldPos[k] == Vector2.Zero) continue;
                    Vector2 shaderPos = proj.oldPos[k] - Main.screenPosition + new Vector2(proj.width / 2f, proj.height / 2f);
                    float shaderProgress = (proj.oldPos.Length - k) / (float)proj.oldPos.Length;
                    sb.Draw(shaderGlow, shaderPos, null, Color.White * shaderProgress * 0.55f, proj.oldRot[k],
                        glowOrigin, proj.scale * (0.4f + shaderProgress * 0.65f), SpriteEffects.None, 0f);
                }
                EroicaShaderManager.RestoreSpriteBatch(sb);
            }

            // Afterimage trail with FractalGold→HexagonScarlet gradient
            for (int k = 0; k < proj.oldPos.Length; k++)
            {
                if (proj.oldPos[k] == Vector2.Zero) continue;
                Vector2 drawPos = proj.oldPos[k] - Main.screenPosition
                    + new Vector2(proj.width / 2f, proj.height / 2f);
                float progress = (proj.oldPos.Length - k) / (float)proj.oldPos.Length;
                Color trailColor = (Color.Lerp(HexagonScarlet, FractalGold, progress) * progress) with { A = 0 };
                sb.Draw(texture, drawPos, null, trailColor, proj.oldRot[k],
                    drawOrigin, proj.scale * (0.5f + progress * 0.5f), SpriteEffects.None, 0f);
            }

            // Geometry overlay — hexagonal glow ring at projectile center
            float geoRotation = Main.GameUpdateCount * 0.06f;
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            if (bloom != null)
            {
                Vector2 bloomOrigin = bloom.Size() * 0.5f;
                Color hexGlow = (FractalGold with { A = 0 }) * 0.3f;
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f + geoRotation;
                    sb.Draw(bloom, projScreen + angle.ToRotationVector2() * 6f, null,
                        hexGlow, 0f, bloomOrigin, 0.12f, SpriteEffects.None, 0f);
                }
            }

            // 4-layer bloom stack (RecursionFade → HexagonScarlet → FractalGold → InfinityWhite)
            float pulse = MathF.Sin(Main.GameUpdateCount * 0.1f) * 0.08f + 1f;
            sb.Draw(texture, projScreen, null, (RecursionFade with { A = 0 }) * 0.35f,
                proj.rotation, drawOrigin, proj.scale * 1.35f * pulse, SpriteEffects.None, 0f);
            sb.Draw(texture, projScreen, null, (HexagonScarlet with { A = 0 }) * 0.4f,
                proj.rotation, drawOrigin, proj.scale * 1.22f * pulse, SpriteEffects.None, 0f);
            sb.Draw(texture, projScreen, null, (FractalGold with { A = 0 }) * 0.45f,
                proj.rotation, drawOrigin, proj.scale * 1.10f * pulse, SpriteEffects.None, 0f);
            sb.Draw(texture, projScreen, null, (InfinityWhite with { A = 0 }) * 0.5f,
                proj.rotation, drawOrigin, proj.scale * 1.02f * pulse, SpriteEffects.None, 0f);

            // Counter-rotating flares
            EroicaVFXLibrary.DrawCounterRotatingFlares(sb, proj.Center,
                0.4f, Main.GlobalTimeWrappedHourly, 0.7f);

            // Main sprite
            sb.Draw(texture, projScreen, null, new Color(255, 245, 230, 220),
                proj.rotation, drawOrigin, proj.scale, SpriteEffects.None, 0f);

            return false;
        }

        // ══════════════════════════════════════════════════════════════
        //  5. AMBIENT GEOMETRY
        // ══════════════════════════════════════════════════════════════

        /// <summary>4-point outer ring + 3-point inner triangle + connecting branches, lighting.</summary>
        public static void AmbientGeometryOrbit(Vector2 playerCenter, float timer)
        {
            // 4-point rotating outer ring
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f + timer;
                float radius = 38f + MathF.Sin(timer * 2f + i) * 6f;
                Vector2 pos = playerCenter + angle.ToRotationVector2() * radius;
                Dust d = Dust.NewDustPerfect(pos, DustID.GoldFlame,
                    Vector2.Zero, 0, Color.Lerp(FractalGold, HexagonScarlet, (float)i / 4f), 1.0f);
                d.noGravity = true;
            }

            // 3-point inner triangle (counter-rotating)
            for (int i = 0; i < 3; i++)
            {
                float angle = MathHelper.TwoPi * i / 3f + timer * 1.5f;
                Vector2 pos = playerCenter + angle.ToRotationVector2() * 15f;
                Dust d = Dust.NewDustPerfect(pos, DustID.GoldFlame,
                    Vector2.Zero, 0, NodeCore * 0.7f, 0.8f);
                d.noGravity = true;
            }

            // Connecting fractal branches between outer and inner geometry
            for (int i = 0; i < 3; i++)
            {
                Vector2 outerPos = playerCenter + (MathHelper.TwoPi * i / 3f + timer).ToRotationVector2() * 38f;
                Vector2 innerPos = playerCenter + (MathHelper.TwoPi * i / 3f + timer * 1.5f).ToRotationVector2() * 15f;
                Vector2 midPoint = Vector2.Lerp(innerPos, outerPos, 0.5f);
                Dust branch = Dust.NewDustPerfect(midPoint, DustID.GoldFlame,
                    Vector2.Zero, 0, BranchGold * 0.5f, 0.6f);
                branch.noGravity = true;
            }

            float pulse = 0.8f + MathF.Sin(timer * 2f) * 0.2f;
            Lighting.AddLight(playerCenter, FractalGold.ToVector3() * 0.4f * pulse);
        }

        /// <summary>Hold-item VFX — geometric aura: hex frame, branches, petals, notes, sparkles.</summary>
        public static void HoldItemVFX(Player player)
        {
            float time = Main.GameUpdateCount * 0.03f;

            // Rotating hexagonal frame (sparse — 1/12 chance per frame)
            if (Main.rand.NextBool(12))
                AmbientGeometryOrbit(player.Center, time);

            // Fractal branch particles (1/15 chance)
            if (Main.rand.NextBool(15))
            {
                float branchAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                SpawnFractalBranch(player.Center, branchAngle.ToRotationVector2(), 3, BranchGold, RecursionFade);
            }

            if (Main.rand.NextBool(20))
                EroicaVFXLibrary.SpawnSakuraPetals(player.Center, 1, 40f);
            if (Main.rand.NextBool(18))
                EroicaVFXLibrary.SpawnMusicNotes(player.Center, 1, 25f, 0.6f, 0.85f, 30);
            if (Main.rand.NextBool(12))
                EroicaVFXLibrary.SpawnValorSparkles(player.Center + Main.rand.NextVector2Circular(20f, 20f), 1, 10f);

            float pulse = MathF.Sin(Main.GameUpdateCount * 0.08f) * 0.1f + 0.9f;
            Lighting.AddLight(player.Center, FractalGold.ToVector3() * 0.5f * pulse);
        }

        // ══════════════════════════════════════════════════════════════
        //  6. GEOMETRY HELPERS
        // ══════════════════════════════════════════════════════════════

        /// <summary>6-point hexagonal dust spawn — vertices of a regular hexagon.</summary>
        public static void SpawnHexagonalDust(Vector2 center, float radius, float rotation,
            Color color, float scale)
        {
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f + rotation;
                Vector2 point = center + angle.ToRotationVector2() * radius;
                Dust d = Dust.NewDustPerfect(point, DustID.GoldFlame,
                    Vector2.Zero, 0, Color.Lerp(color, HexagonScarlet, i / 6f * 0.4f), scale);
                d.noGravity = true;
            }
        }

        /// <summary>3-point triangular dust spawn — vertices of an equilateral triangle.</summary>
        public static void SpawnTriangularDust(Vector2 center, float radius, float rotation,
            Color color, float scale)
        {
            for (int i = 0; i < 3; i++)
            {
                float angle = MathHelper.TwoPi * i / 3f + rotation;
                Vector2 point = center + angle.ToRotationVector2() * radius;
                Dust d = Dust.NewDustPerfect(point, DustID.GoldFlame,
                    Vector2.Zero, 0, color, scale);
                d.noGravity = true;
            }
        }

        /// <summary>Chain of dust particles forming a fractal branch with color interpolation.</summary>
        public static void SpawnFractalBranch(Vector2 start, Vector2 direction, int segments,
            Color startColor, Color endColor)
        {
            float segmentLength = 10f;
            Vector2 current = start;
            for (int i = 0; i < segments; i++)
            {
                float progress = (float)i / segments;
                current += direction * segmentLength;
                Dust d = Dust.NewDustPerfect(current, DustID.GoldFlame,
                    direction * 0.3f, 0, Color.Lerp(startColor, endColor, progress), 1.1f - progress * 0.4f);
                d.noGravity = true;
                d.fadeIn = 0.9f;
            }
        }
    }
}
