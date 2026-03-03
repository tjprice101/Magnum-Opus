using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Eroica.Weapons.CelestialValor.Buffs;
using MagnumOpus.Content.Eroica.Weapons.CelestialValor.Projectiles;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor
{
    /// <summary>
    /// Celestial Valor swing projectile — Heroic Crescendo 4-phase combo.
    /// Phase 0: Resolute Strike — single overhead slash.
    /// Phase 1: Ascending Valor — upward diagonal, fires ValorBeam arcs.
    /// Phase 2: Crimson Legion — triple rapid strikes, spawns valor beam projectiles.
    /// Phase 3: Finale Fortissimo — 270° heroic slam with ValorBoom AoE.
    /// Valor Gauge builds on successive hits; at max the next Finale becomes Gloria.
    /// Hero's Resolve: below 30% HP, extra embers and +25% damage.
    /// </summary>
    public class CelestialValorSwing : ModProjectile
    {
        // ── AI state ──
        private ref float ComboPhase => ref Projectile.ai[0];
        private ref float PhaseTimer => ref Projectile.ai[1];

        private float swingRotation = 0f;
        private int swingDirection = 1;
        private bool initialized = false;
        private float valorGauge = 0f;
        private bool phaseProjectileSpawned = false;

        // ── Afterimage tracking ──
        private const int MaxAfterimages = 10;
        private float[] afterimageRotations = new float[MaxAfterimages];
        private int afterimageHead = 0;

        // ── Combo phase definitions (durations in AI ticks) ──
        private static readonly int[] PhaseDuration = { 20, 24, 30, 26 };
        private static readonly float[] ArcStart = { -2.0f, 0.8f, -1.6f, -2.8f };
        private static readonly float[] ArcEnd = { 0.8f, -1.6f, 1.6f, 2.0f };

        public override void SetDefaults()
        {
            Projectile.width = 120;
            Projectile.height = 120;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 9999;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.noEnchantmentVisuals = true;
            Projectile.ownerHitCheck = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!player.channel || player.dead || !player.active)
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 2;

            // ── Initialize on first tick ──
            if (!initialized)
            {
                initialized = true;
                swingDirection = player.direction;
                Projectile.direction = swingDirection;
                swingRotation = ArcStart[0] * swingDirection;
                for (int i = 0; i < MaxAfterimages; i++)
                    afterimageRotations[i] = swingRotation;
            }

            Projectile.Center = player.Center;

            // ── Swing interpolation ──
            int phaseIdx = (int)MathHelper.Clamp(ComboPhase, 0, 3);
            float duration = PhaseDuration[phaseIdx];
            float progress = MathHelper.Clamp(PhaseTimer / duration, 0f, 1f);
            float eased = EaseInOut(progress);

            float startAngle = ArcStart[phaseIdx] * swingDirection;
            float endAngle = ArcEnd[phaseIdx] * swingDirection;
            swingRotation = MathHelper.Lerp(startAngle, endAngle, eased);
            Projectile.rotation = swingRotation;

            // ── Afterimage record ──
            afterimageRotations[afterimageHead] = swingRotation;
            afterimageHead = (afterimageHead + 1) % MaxAfterimages;

            // ── Blade tip VFX ──
            float bladeLen = 95f;
            Vector2 tipDir = swingRotation.ToRotationVector2();
            Vector2 tipPos = player.Center + tipDir * bladeLen;

            bool heroResolve = player.statLife < player.statLifeMax2 * 0.3f;
            SpawnPerFrameVFX(tipPos, phaseIdx, heroResolve, player);

            // ── Phase-specific sub-projectile spawning at midpoint ──
            if (!phaseProjectileSpawned && progress > 0.5f)
            {
                phaseProjectileSpawned = true;
                SpawnPhaseProjectiles(player, tipPos, phaseIdx);
            }

            // ── Advance timer ──
            PhaseTimer++;

            if (PhaseTimer >= duration)
            {
                OnPhaseEnd(player, tipPos, phaseIdx);
                ComboPhase = (ComboPhase + 1) % 4;
                PhaseTimer = 0;
                swingDirection = -swingDirection;
                Projectile.direction = swingDirection;
                phaseProjectileSpawned = false;

                for (int i = 0; i < MaxAfterimages; i++)
                    afterimageRotations[i] = ArcStart[(int)ComboPhase] * swingDirection;
            }

            // ── Valor gauge decay ──
            valorGauge = Math.Max(0, valorGauge - 0.08f);

            // ── Player lock ──
            player.direction = Main.MouseWorld.X >= player.Center.X ? 1 : -1;
            player.heldProj = Projectile.whoAmI;
            player.itemAnimation = 2;
            player.itemTime = 2;
        }

        private static float EaseInOut(float t)
        {
            return t < 0.5f ? 2f * t * t : 1f - MathF.Pow(-2f * t + 2f, 2f) / 2f;
        }

        #region VFX Spawning

        private void SpawnPerFrameVFX(Vector2 tipPos, int phase, bool heroResolve, Player player)
        {
            // Blade tip dust every frame
            EroicaVFXLibrary.SpawnSwingDust(tipPos, -Projectile.rotation.ToRotationVector2());
            EroicaVFXLibrary.SpawnContrastSparkle(tipPos, -Projectile.rotation.ToRotationVector2());

            // Music notes periodically
            if ((int)PhaseTimer % 6 == 0)
                EroicaVFXLibrary.SpawnMusicNotes(tipPos, 1 + phase / 2, 12f, 0.7f, 1.0f, 28);

            // Valor sparkles on higher phases
            if (phase >= 1 && (int)PhaseTimer % 4 == 0)
                EroicaVFXLibrary.SpawnValorSparkles(tipPos, 2 + phase, 18f);

            // Dense sparks on phase 2+
            if (phase >= 2)
                EroicaVFXLibrary.SpawnDirectionalSparks(tipPos, Projectile.rotation.ToRotationVector2(), 2, 4f);

            // Hero's Resolve: extra rising embers
            if (heroResolve && Main.rand.NextBool(2))
                EroicaVFXLibrary.SpawnHeroicAura(player.Center, 35f);

            // Dynamic lighting
            EroicaVFXLibrary.AddPaletteLighting(tipPos, 0.3f + phase * 0.15f, 0.7f + phase * 0.1f);
        }

        private void SpawnPhaseProjectiles(Player player, Vector2 tipPos, int phase)
        {
            if (Main.myPlayer != Projectile.owner) return;

            Vector2 aimDir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);

            switch (phase)
            {
                case 1: // Ascending Valor: 1 ValorBeam
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.Center, aimDir * 14f,
                        ModContent.ProjectileType<ValorBeam>(), (int)(Projectile.damage * 0.5f),
                        Projectile.knockBack * 0.5f, Projectile.owner);
                    SoundEngine.PlaySound(SoundID.Item60 with { Pitch = 0.1f, Volume = 0.6f }, player.Center);
                    break;

                case 2: // Crimson Legion: 3 ValorBeams in spread
                    for (int i = -1; i <= 1; i++)
                    {
                        Vector2 dir = aimDir.RotatedBy(i * 0.18f) * 15f;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.Center, dir,
                            ModContent.ProjectileType<ValorBeam>(), (int)(Projectile.damage * 0.35f),
                            Projectile.knockBack * 0.3f, Projectile.owner);
                    }
                    SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.2f, Volume = 0.5f }, player.Center);
                    break;

                case 3: // Finale Fortissimo: ValorBoom AoE
                    float boomScale = valorGauge >= 95f ? 2f : 1f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipPos, Vector2.Zero,
                        ModContent.ProjectileType<ValorBoom>(), (int)(Projectile.damage * 0.7f * boomScale),
                        Projectile.knockBack, Projectile.owner);
                    SoundEngine.PlaySound(SoundID.Item62 with { Pitch = -0.3f }, player.Center);
                    break;
            }
        }

        private void OnPhaseEnd(Player player, Vector2 tipPos, int phase)
        {
            EroicaVFXLibrary.MeleeImpact(tipPos, phase);

            if (phase == 3)
            {
                float slamIntensity = valorGauge >= 95f ? 1.5f : 1f;
                EroicaVFXLibrary.FinisherSlam(tipPos, slamIntensity);

                if (valorGauge >= 95f) // Gloria!
                {
                    EroicaVFXLibrary.DeathHeroicFlash(tipPos, 1.2f);
                    EroicaVFXLibrary.SpawnSakuraPetals(tipPos, 18, 90f);
                    EroicaVFXLibrary.MusicNoteBurst(tipPos, EroicaPalette.Gold, 12, 6f);
                    valorGauge = 0f;
                }
            }
            else if (phase == 1)
            {
                EroicaVFXLibrary.Shockwave(tipPos, 0.5f);
            }
        }

        #endregion

        #region On Hit

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 240);
            target.AddBuff(ModContent.BuffType<HeroicBurn>(), 180);
            target.AddBuff(ModContent.BuffType<ValorStagger>(), 90);

            valorGauge = Math.Min(100f, valorGauge + 14f);

            int phase = (int)MathHelper.Clamp(ComboPhase, 0, 3);
            EroicaVFXLibrary.MeleeImpact(target.Center, phase);

            // Hero's Resolve bonus sparks
            Player player = Main.player[Projectile.owner];
            if (player.statLife < player.statLifeMax2 * 0.3f)
            {
                EroicaVFXLibrary.SpawnDirectionalSparks(target.Center,
                    (target.Center - player.Center).SafeNormalize(Vector2.UnitX), 5, 7f);
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // Hero's Resolve: +25% damage below 30% HP
            Player player = Main.player[Projectile.owner];
            if (player.statLife < player.statLifeMax2 * 0.3f)
                modifiers.FinalDamage *= 1.25f;

            // Scale damage with combo phase
            int phase = (int)MathHelper.Clamp(ComboPhase, 0, 3);
            modifiers.FinalDamage *= 1f + phase * 0.08f;
        }

        #endregion

        #region Rendering

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Player player = Main.player[Projectile.owner];

            Texture2D bladeTex = TextureAssets.Item[ModContent.ItemType<CelestialValor>()].Value;
            Vector2 bladeOrigin = new Vector2(0, bladeTex.Height);
            float bladeScale = 1.15f;
            SpriteEffects flip = swingDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;

            int phase = (int)MathHelper.Clamp(ComboPhase, 0, 3);
            float intensity = 0.6f + phase * 0.15f;
            bool heroResolve = player.statLife < player.statLifeMax2 * 0.3f;
            if (heroResolve) intensity += 0.2f;

            Vector2 playerDraw = player.Center - Main.screenPosition;
            float drawBaseRot = swingRotation + (swingDirection > 0 ? -MathHelper.PiOver4 : MathHelper.PiOver4 + MathHelper.Pi);

            // ── Layer 1: Afterimage trail ──
            DrawAfterimages(sb, bladeTex, bladeOrigin, bladeScale, flip, playerDraw, phase, drawBaseRot);

            // ── Layer 2: Main blade sprite ──
            Color bladeTint = Color.Lerp(lightColor, EroicaVFXLibrary.GetPaletteColor(0.35f + phase * 0.12f), 0.3f);
            sb.Draw(bladeTex, playerDraw, null, bladeTint, drawBaseRot, bladeOrigin, bladeScale, flip, 0f);

            // Inner glow layer on blade
            Color bladeGlow = EroicaVFXLibrary.GetPaletteColor(0.5f + phase * 0.1f) with { A = 0 };
            sb.Draw(bladeTex, playerDraw, null, bladeGlow * 0.3f, drawBaseRot, bladeOrigin, bladeScale * 1.03f, flip, 0f);

            // ── Layer 3: Bloom at blade tip ──
            DrawBladeTipBloom(sb, player, phase, intensity);

            // ── Layer 4: Valor gauge aura ──
            if (valorGauge > 40f)
                DrawValorAura(sb, player);

            return false;
        }

        private void DrawAfterimages(SpriteBatch sb, Texture2D tex, Vector2 origin, float scale,
            SpriteEffects flip, Vector2 playerDraw, int phase, float currentDrawRot)
        {
            int count = 4 + phase;

            for (int i = 0; i < count && i < MaxAfterimages; i++)
            {
                int idx = (afterimageHead - 2 - i + MaxAfterimages) % MaxAfterimages;
                float rot = afterimageRotations[idx];
                float drawRot = rot + (swingDirection > 0 ? -MathHelper.PiOver4 : MathHelper.PiOver4 + MathHelper.Pi);

                float fade = 1f - (float)(i + 1) / (count + 1);
                fade *= fade;

                float paletteT = 0.25f + phase * 0.12f + i * 0.06f;
                Color col = EroicaVFXLibrary.GetPaletteColor(paletteT) * (fade * 0.35f);
                col.A = 0;

                float afterScale = scale * (1f - i * 0.025f);
                sb.Draw(tex, playerDraw, null, col, drawRot, origin, afterScale, flip, 0f);
            }
        }

        private void DrawBladeTipBloom(SpriteBatch sb, Player player, int phase, float intensity)
        {
            float bladeLen = 95f;
            Vector2 tipPos = player.Center + swingRotation.ToRotationVector2() * bladeLen;

            EroicaVFXLibrary.DrawEroicaBloomStack(sb, tipPos,
                EroicaPalette.DeepScarlet, EroicaPalette.Gold,
                0.28f + phase * 0.07f, intensity);

            // Counter-rotating flares on phase 2+
            if (phase >= 2)
            {
                EroicaVFXLibrary.DrawCounterRotatingFlares(sb, tipPos,
                    0.22f + phase * 0.04f, (float)Main.GameUpdateCount * 0.05f, intensity * 0.6f);
            }
        }

        private void DrawValorAura(SpriteBatch sb, Player player)
        {
            float ratio = valorGauge / 100f;
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            if (bloom == null) return;

            Vector2 drawPos = player.Center - Main.screenPosition;
            Vector2 bloomOrigin = bloom.Size() * 0.5f;
            float pulse = 0.85f + MathF.Sin((float)Main.GameUpdateCount * 0.08f) * 0.15f;

            Color glowCol = Color.Lerp(EroicaPalette.Scarlet, EroicaPalette.Gold, ratio) with { A = 0 };
            sb.Draw(bloom, drawPos, null, glowCol * (0.12f * ratio * pulse), 0f, bloomOrigin,
                1.2f * ratio, SpriteEffects.None, 0f);
        }

        #endregion
    }
}