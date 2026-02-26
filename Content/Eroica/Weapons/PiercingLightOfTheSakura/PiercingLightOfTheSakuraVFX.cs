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

namespace MagnumOpus.Content.Eroica.Weapons.PiercingLightOfTheSakura
{
    /// <summary>
    /// Static VFX helper for Piercing Light of the Sakura — precision ranged weapon
    /// that builds crescendo. Nine steady tracer rounds accumulate orbiting charge
    /// points. On the tenth, the energy detonates into sakura lightning + seeking crystals.
    /// Game logic stays in the item/projectile files; all VFX routed through EroicaVFXLibrary.
    /// </summary>
    public static class PiercingLightOfTheSakuraVFX
    {
        // ══════════════════════════════════════════════════════════════
        //  PIERCING LIGHT ACCENT PALETTE  (weapon-specific identity colors)
        // ══════════════════════════════════════════════════════════════

        private static readonly Color TracerScarlet    = new Color(200, 50, 50);    // Standard tracer rounds
        private static readonly Color ChargingGold     = new Color(255, 200, 80);   // Building charge energy
        private static readonly Color PierceSakura     = new Color(255, 140, 170);  // Sakura-tinted piercing light
        private static readonly Color BurstWhite       = new Color(255, 250, 245);  // Full charge burst
        private static readonly Color OrbitalEmber     = new Color(220, 120, 60);   // Orbiting charge mote
        private static readonly Color ConvergenceGlow  = new Color(255, 220, 150);  // Convergence warning glow
        private static readonly Color LightningPink    = new Color(255, 100, 150);  // Sakura lightning color
        private static readonly Color CrystalFacet     = new Color(200, 160, 120);  // Seeking crystal accent

        // ══════════════════════════════════════════════════════════════
        //  1. CHARGE ORBIT VFX  (9-point buildup around gun tip)
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// 9-point orbiting charge buildup. Each shot adds a visible point
        /// (TracerScarlet→ChargingGold). Core at 30%+, convergence warning
        /// at shots 7-9, halo ring pulse at 60%+.
        /// </summary>
        public static void ChargeOrbitVFX(Vector2 gunTip, int shotCounter, float rotationSpeed)
        {
            if (shotCounter <= 0) return;
            float chargeProgress = shotCounter / 9f;
            float orbitRadius = 18f + chargeProgress * 8f;

            for (int i = 0; i < shotCounter; i++)
            {
                float angle = rotationSpeed + MathHelper.TwoPi * i / 9f;
                Vector2 orbPos = gunTip + angle.ToRotationVector2() * orbitRadius;
                Color orbColor = Color.Lerp(TracerScarlet, ChargingGold, i / 8f);
                float orbPulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f + i * 0.5f) * 0.1f + 1f;
                float orbScale = (0.6f + chargeProgress * 0.5f) * orbPulse;
                Dust d = Dust.NewDustPerfect(orbPos, DustID.GoldFlame, Vector2.Zero, 0, orbColor, orbScale);
                d.noGravity = true;

                if (Main.rand.NextBool(3))
                {
                    Vector2 trailVel = (gunTip - orbPos).SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2) * 0.8f;
                    Dust trail = Dust.NewDustPerfect(orbPos, DustID.Torch, trailVel, 0, OrbitalEmber, 0.7f);
                    trail.noGravity = true;
                }
            }

            // Central charge core grows from 30%+
            if (chargeProgress > 0.3f)
            {
                float coreIntensity = (chargeProgress - 0.3f) / 0.7f;
                Color coreColor = Color.Lerp(ChargingGold, BurstWhite, coreIntensity * 0.5f);
                Dust core = Dust.NewDustPerfect(gunTip, DustID.GoldFlame, Vector2.Zero, 0, coreColor, 0.8f + coreIntensity);
                core.noGravity = true;

                if (chargeProgress > 0.6f && Main.rand.NextBool(8))
                {
                    var ring = new BloomRingParticle(gunTip, Vector2.Zero,
                        ConvergenceGlow * 0.6f, 0.15f + coreIntensity * 0.12f, 18, 0.06f);
                    MagnumParticleHandler.SpawnParticle(ring);
                }
            }

            // Convergence warning at shots 7-9
            if (shotCounter >= 7)
            {
                float urgency = (shotCounter - 6) / 3f;
                if (Main.rand.NextBool(Math.Max(2, (int)(6 - urgency * 4))))
                {
                    Vector2 spawnPos = gunTip + Main.rand.NextVector2CircularEdge(35f, 35f);
                    Vector2 convergeVel = (gunTip - spawnPos).SafeNormalize(Vector2.Zero) * (2f + urgency * 2f);
                    var p = new GenericGlowParticle(spawnPos, convergeVel,
                        Color.Lerp(PierceSakura, ConvergenceGlow, urgency), 0.2f, 12, true);
                    MagnumParticleHandler.SpawnParticle(p);
                }
            }

            float lightIntensity = 0.3f + chargeProgress * 0.5f;
            Lighting.AddLight(gunTip, Color.Lerp(TracerScarlet, ChargingGold, chargeProgress).ToVector3() * lightIntensity);
        }

        // ══════════════════════════════════════════════════════════════
        //  2. MUZZLE FLASH VFX  (normal tracer vs. charged burst)
        // ══════════════════════════════════════════════════════════════

        /// <summary>Standard tracer flash: gradient dust, single halo, directional sparks, tracer dust.</summary>
        public static void NormalShotFlash(Vector2 muzzlePos, Vector2 velocity)
        {
            Vector2 shotDir = velocity.SafeNormalize(Vector2.UnitX);

            for (int i = 0; i < 5; i++)
            {
                float progress = i / 4f;
                Color flashColor = Color.Lerp(EroicaPalette.DeepScarlet, ChargingGold, progress);
                Vector2 vel = shotDir * Main.rand.NextFloat(2f, 5f) + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Dust d = Dust.NewDustPerfect(muzzlePos, DustID.GoldFlame, vel, 0, flashColor, 1.0f + progress * 0.4f);
                d.noGravity = true;
            }
            EroicaVFXLibrary.SpawnGradientHaloRings(muzzlePos, 1, 0.25f);
            EroicaVFXLibrary.SpawnDirectionalSparks(muzzlePos, shotDir, 3, 4f);

            for (int i = 0; i < 3; i++)
            {
                Color tracerCol = Color.Lerp(TracerScarlet, EroicaPalette.Crimson, i / 2f);
                Dust td = Dust.NewDustPerfect(muzzlePos, DustID.GoldFlame, Main.rand.NextVector2Circular(1f, 1f), 0, tracerCol, 0.9f);
                td.noGravity = true;
            }
            Lighting.AddLight(muzzlePos, TracerScarlet.ToVector3() * 0.6f);
        }

        /// <summary>
        /// FULL BURST: massive bloom, 3-ring halo cascade, music note burst,
        /// sakura petal explosion, 8-count spark spray, screen trauma, god rays, screen ripple.
        /// </summary>
        public static void ChargedShotFlash(Vector2 muzzlePos, Vector2 velocity)
        {
            Vector2 shotDir = velocity.SafeNormalize(Vector2.UnitX);

            EroicaVFXLibrary.BloomFlare(muzzlePos, BurstWhite, 0.9f, 16);
            EroicaVFXLibrary.SpawnGradientHaloRings(muzzlePos, 3, 0.5f);
            var outerRing = new BloomRingParticle(muzzlePos, Vector2.Zero, PierceSakura * 0.8f, 0.7f, 28, 0.10f);
            MagnumParticleHandler.SpawnParticle(outerRing);

            EroicaVFXLibrary.MusicNoteBurst(muzzlePos, ChargingGold, 8, 4f);
            EroicaVFXLibrary.SpawnSakuraMusicNotes(muzzlePos, 4, 35f);
            EroicaVFXLibrary.SpawnSakuraPetals(muzzlePos, 12, 50f);
            EroicaVFXLibrary.SpawnDirectionalSparks(muzzlePos, shotDir, 8, 8f);

            MagnumScreenEffects.AddScreenShake(8f);
            ScreenDistortionManager.TriggerRipple(muzzlePos, PierceSakura, 0.7f, 22);
            GodRaySystem.CreateBurst(muzzlePos, LightningPink, 6, 90f, 35,
                GodRaySystem.GodRayStyle.Explosion, ChargingGold);

            SoundEngine.PlaySound(SoundID.Item125 with { Pitch = 0.3f, Volume = 0.8f }, muzzlePos);
            Lighting.AddLight(muzzlePos, BurstWhite.ToVector3() * 1.5f);
        }

        // ══════════════════════════════════════════════════════════════
        //  3. SAKURA LIGHTNING PROJECTILE  (10th-shot special)
        // ══════════════════════════════════════════════════════════════

        /// <summary>Per-frame: zigzag lightning dust chain, sakura petals at nodes, 3 orbiting motes, music notes.</summary>
        public static void LightningTrailVFX(Projectile proj)
        {
            Vector2 trailDir = -proj.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 perp = trailDir.RotatedBy(MathHelper.PiOver2);
            Vector2 chainPos = proj.Center;

            for (int s = 0; s < 6; s++)
            {
                float zigzag = ((s % 2 == 0) ? 1f : -1f) * Main.rand.NextFloat(4f, 8f);
                chainPos += trailDir * Main.rand.NextFloat(6f, 10f) + perp * zigzag;
                Dust ld = Dust.NewDustPerfect(chainPos, DustID.PinkTorch,
                    Main.rand.NextVector2Circular(0.5f, 0.5f), 0, LightningPink, 1.4f);
                ld.noGravity = true;
                if (s % 2 == 0) EroicaVFXLibrary.SpawnSakuraPetals(chainPos, 1, 8f);
            }

            float orbitAngle = Main.GameUpdateCount * 0.12f;
            for (int m = 0; m < 3; m++)
            {
                float angle = orbitAngle + MathHelper.TwoPi * m / 3f;
                Vector2 motePos = proj.Center + angle.ToRotationVector2() * 14f;
                Color moteCol = Color.Lerp(OrbitalEmber, LightningPink,
                    (float)Math.Sin(Main.GameUpdateCount * 0.08f + m) * 0.5f + 0.5f);
                Dust md = Dust.NewDustPerfect(motePos, DustID.PinkTorch, proj.velocity * 0.3f, 0, moteCol, 0.9f);
                md.noGravity = true;
            }

            if (Main.rand.NextBool(5)) EroicaVFXLibrary.SpawnSakuraMusicNotes(proj.Center, 1, 12f);
            Lighting.AddLight(proj.Center, LightningPink.ToVector3() * 1.0f);
        }

        /// <summary>Powerful hit: 8-bolt lightning burst, sakura explosion, bloom cascade, screen distortion.</summary>
        public static void LightningHitVFX(Vector2 hitPos)
        {
            for (int b = 0; b < 8; b++)
            {
                float baseAngle = MathHelper.TwoPi * b / 8f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 boltDir = baseAngle.ToRotationVector2();
                Vector2 boltPos = hitPos;
                for (int seg = 0; seg < Main.rand.Next(4, 7); seg++)
                {
                    boltDir = boltDir.RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f));
                    boltPos += boltDir * Main.rand.NextFloat(10f, 18f);
                    Dust bolt = Dust.NewDustPerfect(boltPos, DustID.PinkTorch,
                        Main.rand.NextVector2Circular(0.8f, 0.8f), 0, LightningPink, 1.5f);
                    bolt.noGravity = true;
                }
            }

            EroicaVFXLibrary.SpawnSakuraPetals(hitPos, 10, 45f);
            EroicaVFXLibrary.BloomFlare(hitPos, PierceSakura, 0.8f, 18);
            EroicaVFXLibrary.SpawnGradientHaloRings(hitPos, 4, 0.4f);
            var impactRing = new BloomRingParticle(hitPos, Vector2.Zero, LightningPink * 0.7f, 0.6f, 25, 0.09f);
            MagnumParticleHandler.SpawnParticle(impactRing);

            EroicaVFXLibrary.SpawnSakuraMusicNotes(hitPos, 4, 30f);
            EroicaVFXLibrary.SpawnMusicNotes(hitPos, 3, 25f);
            MagnumScreenEffects.AddScreenShake(5f);
            ScreenDistortionManager.TriggerRipple(hitPos, LightningPink, 0.5f, 18);

            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.4f, Volume = 0.6f }, hitPos);
            Lighting.AddLight(hitPos, BurstWhite.ToVector3() * 1.3f);
        }

        /// <summary>Lightning dissipation: scattered sparks, fading arcs, final bloom.</summary>
        public static void LightningDeathVFX(Vector2 pos)
        {
            for (int i = 0; i < 12; i++)
            {
                Color col = Color.Lerp(LightningPink, PierceSakura, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.PinkTorch,
                    Main.rand.NextVector2Circular(5f, 5f), 0, col, Main.rand.NextFloat(1.0f, 1.5f));
                d.noGravity = true;
            }

            for (int a = 0; a < 4; a++)
            {
                Vector2 arcDir = (MathHelper.TwoPi * a / 4f + Main.rand.NextFloat(-0.4f, 0.4f)).ToRotationVector2();
                Vector2 arcPos = pos;
                for (int seg = 0; seg < 3; seg++)
                {
                    arcDir = arcDir.RotatedBy(Main.rand.NextFloat(-0.6f, 0.6f));
                    arcPos += arcDir * Main.rand.NextFloat(8f, 14f);
                    Dust arc = Dust.NewDustPerfect(arcPos, DustID.PinkTorch,
                        Main.rand.NextVector2Circular(0.3f, 0.3f), 80, LightningPink * 0.7f, 1.1f);
                    arc.noGravity = true;
                }
            }

            EroicaVFXLibrary.DeathHeroicFlash(pos, 0.5f);
            EroicaVFXLibrary.SpawnSakuraPetals(pos, 5, 30f);
            SoundEngine.PlaySound(SoundID.Item10 with { Pitch = 0.2f, Volume = 0.4f }, pos);
            Lighting.AddLight(pos, PierceSakura.ToVector3() * 0.8f);
        }

        // ══════════════════════════════════════════════════════════════
        //  4. SEEKING CRYSTAL VFX  (companion projectiles from burst)
        // ══════════════════════════════════════════════════════════════

        /// <summary>Per-frame: crystal sparkle trail, gentle CrystalFacet glow, subtle music notes.</summary>
        public static void CrystalTrailVFX(Projectile proj)
        {
            if (Main.rand.NextBool(2))
            {
                Vector2 vel = -proj.velocity * 0.15f + Main.rand.NextVector2Circular(0.4f, 0.4f);
                Color col = Color.Lerp(CrystalFacet, ChargingGold, Main.rand.NextFloat(0.3f));
                Dust d = Dust.NewDustPerfect(proj.Center, DustID.GoldFlame, vel, 0, col, 0.9f);
                d.noGravity = true;
            }
            if (Main.rand.NextBool(3))
            {
                Dust s = Dust.NewDustPerfect(proj.Center + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.GoldFlame, Main.rand.NextVector2Circular(0.5f, 0.5f), 0, CrystalFacet, 0.8f);
                s.noGravity = true;
            }
            if (Main.rand.NextBool(8)) EroicaVFXLibrary.SpawnMusicNotes(proj.Center, 1, 8f, 0.6f, 0.8f, 25);
            Lighting.AddLight(proj.Center, CrystalFacet.ToVector3() * 0.4f);
        }

        /// <summary>Crystal impact: small bloom, faceted dust burst, single music note.</summary>
        public static void CrystalHitVFX(Vector2 pos)
        {
            EroicaVFXLibrary.BloomFlare(pos, CrystalFacet, 0.4f, 12);
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color col = Color.Lerp(CrystalFacet, ChargingGold, i / 7f);
                Dust d = Dust.NewDustPerfect(pos, DustID.GoldFlame, vel, 0, col, 1.0f);
                d.noGravity = true;
            }
            EroicaVFXLibrary.SpawnMusicNotes(pos, 1, 12f, 0.7f, 0.9f, 28);
            Lighting.AddLight(pos, CrystalFacet.ToVector3() * 0.6f);
        }

        // ══════════════════════════════════════════════════════════════
        //  5. PROJECTILE PREDRAW  (custom rendering for special projectiles)
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Lightning rendering: {A=0} zigzag trail (LightningPink→BurstWhite),
        /// 4-layer bloom stack (PierceSakura/ChargingGold/LightningPink/BurstWhite),
        /// main sprite with electric glow. Returns false to suppress vanilla.
        /// Accepts optional sourceRect/drawOrigin for sprite-sheet animation.
        /// </summary>
        public static bool DrawLightningProjectile(SpriteBatch sb, Projectile proj,
            Rectangle? sourceRect = null, Vector2? customOrigin = null, Color? lightColorRef = null)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[proj.type].Value;
            Rectangle frame = sourceRect ?? new Rectangle(0, 0, texture.Width, texture.Height);
            Vector2 drawOrigin = customOrigin ?? new Vector2(frame.Width / 2f, frame.Height / 2f);
            Vector2 projScreen = proj.Center - Main.screenPosition;

            // {A=0} zigzag trail — electric bolt afterimage
            for (int k = 0; k < proj.oldPos.Length; k++)
            {
                if (proj.oldPos[k] == Vector2.Zero) continue;
                Vector2 drawPos = proj.oldPos[k] - Main.screenPosition + new Vector2(proj.width / 2f, proj.height / 2f);
                float progress = (proj.oldPos.Length - k) / (float)proj.oldPos.Length;

                float zigzag = (float)Math.Sin(k * 1.8f + Main.GameUpdateCount * 0.3f) * 4f * (1f - progress);
                Vector2 perpDir = (proj.oldPos[k] - proj.Center).SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.PiOver2);
                drawPos += perpDir * zigzag;

                Color trailColor = (Color.Lerp(LightningPink, BurstWhite, progress * 0.6f) * progress) with { A = 0 };
                sb.Draw(texture, drawPos, frame, trailColor, proj.oldRot[k], drawOrigin,
                    proj.scale * (0.4f + progress * 0.6f), SpriteEffects.None, 0f);
            }

            // Shader-enhanced lightning trail pass
            {
                Texture2D shaderGlow = MagnumTextureRegistry.GetSoftGlow();
                EroicaShaderManager.BeginShaderAdditive(sb);
                EroicaShaderManager.ApplyPiercingLightLightningTrail(Main.GlobalTimeWrappedHourly, 1f);
                Vector2 glowOrigin = shaderGlow.Size() * 0.5f;
                for (int k = 0; k < proj.oldPos.Length; k++)
                {
                    if (proj.oldPos[k] == Vector2.Zero) continue;
                    Vector2 shaderPos = proj.oldPos[k] - Main.screenPosition + new Vector2(proj.width / 2f, proj.height / 2f);
                    float shaderProgress = (proj.oldPos.Length - k) / (float)proj.oldPos.Length;
                    float shaderZigzag = (float)Math.Sin(k * 1.8f + Main.GameUpdateCount * 0.3f) * 3f * (1f - shaderProgress);
                    Vector2 shaderPerp = (proj.oldPos[k] - proj.Center).SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.PiOver2);
                    shaderPos += shaderPerp * shaderZigzag;
                    sb.Draw(shaderGlow, shaderPos, null, Color.White * shaderProgress * 0.55f, proj.oldRot[k],
                        glowOrigin, proj.scale * (0.35f + shaderProgress * 0.6f), SpriteEffects.None, 0f);
                }
                EroicaShaderManager.RestoreSpriteBatch(sb);
            }

            // 4-layer {A=0} bloom stack
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.1f + 1f;
            sb.Draw(texture, projScreen, frame, (PierceSakura with { A = 0 }) * 0.35f, proj.rotation, drawOrigin,
                proj.scale * 1.35f * pulse, SpriteEffects.None, 0f);
            sb.Draw(texture, projScreen, frame, (ChargingGold with { A = 0 }) * 0.30f, proj.rotation, drawOrigin,
                proj.scale * 1.20f * pulse, SpriteEffects.None, 0f);
            sb.Draw(texture, projScreen, frame, (LightningPink with { A = 0 }) * 0.40f, proj.rotation, drawOrigin,
                proj.scale * 1.10f * pulse, SpriteEffects.None, 0f);
            sb.Draw(texture, projScreen, frame, (BurstWhite with { A = 0 }) * 0.30f, proj.rotation, drawOrigin,
                proj.scale * 1.03f * pulse, SpriteEffects.None, 0f);

            // Main sprite with electric glow tint
            sb.Draw(texture, projScreen, frame, new Color(255, 230, 240, 210), proj.rotation, drawOrigin,
                proj.scale, SpriteEffects.None, 0f);
            return false;
        }

        /// <summary>Crystal rendering: faceted afterimage trail, gentle 2-layer bloom. Returns false.</summary>
        public static bool DrawCrystalProjectile(SpriteBatch sb, Projectile proj, ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[proj.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
            Vector2 projScreen = proj.Center - Main.screenPosition;

            for (int k = 0; k < proj.oldPos.Length; k++)
            {
                if (proj.oldPos[k] == Vector2.Zero) continue;
                Vector2 drawPos = proj.oldPos[k] - Main.screenPosition + new Vector2(proj.width / 2f, proj.height / 2f);
                float progress = (proj.oldPos.Length - k) / (float)proj.oldPos.Length;
                Color trailColor = (Color.Lerp(CrystalFacet, ChargingGold, progress * 0.4f) * progress * 0.6f) with { A = 0 };
                sb.Draw(texture, drawPos, null, trailColor, proj.oldRot[k], drawOrigin,
                    proj.scale * (0.5f + progress * 0.5f), SpriteEffects.None, 0f);
            }

            // Shader-enhanced crystal trail pass
            {
                Texture2D shaderGlow = MagnumTextureRegistry.GetSoftGlow();
                EroicaShaderManager.BeginShaderAdditive(sb);
                EroicaShaderManager.ApplyPiercingLightCrescendoCharge(Main.GlobalTimeWrappedHourly, 0.8f);
                Vector2 glowOrigin = shaderGlow.Size() * 0.5f;
                for (int k = 0; k < proj.oldPos.Length; k++)
                {
                    if (proj.oldPos[k] == Vector2.Zero) continue;
                    Vector2 shaderPos = proj.oldPos[k] - Main.screenPosition + new Vector2(proj.width / 2f, proj.height / 2f);
                    float shaderProgress = (proj.oldPos.Length - k) / (float)proj.oldPos.Length;
                    sb.Draw(shaderGlow, shaderPos, null, Color.White * shaderProgress * 0.35f, proj.oldRot[k],
                        glowOrigin, proj.scale * (0.3f + shaderProgress * 0.4f), SpriteEffects.None, 0f);
                }
                EroicaShaderManager.RestoreSpriteBatch(sb);
            }

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.06f + 1f;
            sb.Draw(texture, projScreen, null, (CrystalFacet with { A = 0 }) * 0.30f, proj.rotation, drawOrigin,
                proj.scale * 1.18f * pulse, SpriteEffects.None, 0f);
            sb.Draw(texture, projScreen, null, (ChargingGold with { A = 0 }) * 0.25f, proj.rotation, drawOrigin,
                proj.scale * 1.06f * pulse, SpriteEffects.None, 0f);

            sb.Draw(texture, projScreen, null, new Color(255, 245, 235, 230), proj.rotation, drawOrigin,
                proj.scale, SpriteEffects.None, 0f);
            return false;
        }

        // ══════════════════════════════════════════════════════════════
        //  6. AMBIENT HOLD VFX  (charge-reactive while weapon is held)
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Charge-reactive ambient VFX: sakura petals scaling with charge,
        /// music note crescendo, charge orbit geometry, dynamic lighting.
        /// </summary>
        public static void HoldItemVFX(Player player, int shotCounter)
        {
            if (Main.gameMenu) return;
            float chargeProgress = shotCounter / 9f;
            Vector2 gunTip = player.Center + new Vector2(45f * player.direction, -3f);

            // Charge orbit geometry
            if (shotCounter > 0)
                ChargeOrbitVFX(gunTip, shotCounter, Main.GameUpdateCount * 0.04f);

            // Sakura petals — frequency scales with charge
            int petalChance = Math.Max(4, (int)(12 - chargeProgress * 8));
            if (Main.rand.NextBool(petalChance))
                EroicaVFXLibrary.SpawnSakuraPetals(player.Center + Main.rand.NextVector2Circular(20f, 20f), 1, 15f);

            // Music note crescendo
            if (chargeProgress > 0.5f && Main.rand.NextBool(15))
                EroicaVFXLibrary.SpawnMusicNotes(gunTip, 1, 20f);
            if (chargeProgress > 0.7f && Main.rand.NextBool(10))
                EroicaVFXLibrary.SpawnSakuraMusicNotes(gunTip, 1, 15f);

            // Heroic ambient aura at high charge
            if (chargeProgress > 0.3f)
                EroicaVFXLibrary.SpawnHeroicAura(player.Center, 28f + chargeProgress * 10f);

            // Dynamic lighting from charge level
            float lightIntensity = 0.25f + chargeProgress * 0.45f;
            Lighting.AddLight(gunTip, Color.Lerp(TracerScarlet, ChargingGold, chargeProgress).ToVector3() * lightIntensity);
        }
    }
}
