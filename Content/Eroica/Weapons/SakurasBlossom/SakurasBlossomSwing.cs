using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Eroica.Projectiles;
using MagnumOpus.Content.Eroica.Weapons.SakurasBlossom.Buffs;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace MagnumOpus.Content.Eroica.Weapons.SakurasBlossom
{
    /// <summary>
    /// Sakura's Blossom swing projectile — Petal Dance 3-phase flowing combo.
    /// Phase 0: First Petal — graceful horizontal sweep with petal trail.
    /// Phase 1: Scattered Petals — cross-slash spawns 8 homing petal projectiles.
    /// Phase 2: Final Bloom — upward flourish + downward finisher, 360° petal burst.
    /// Blossom Counter: perfect-timing reflects enemy projectiles as homing petals.
    /// Sakura Meditation: hold without target for 1.5s, next swing 2x arc range.
    /// </summary>
    public class SakurasBlossomSwing : ModProjectile
    {
        // ── AI state ──
        private ref float ComboPhase => ref Projectile.ai[0];
        private ref float PhaseTimer => ref Projectile.ai[1];

        private float swingRotation = 0f;
        private int swingDirection = 1;
        private bool initialized = false;
        private bool phaseProjectileSpawned = false;
        private float meditationTimer = 0f;
        private bool meditationCharged = false;

        // ── Afterimage tracking ──
        private const int MaxAfterimages = 10;
        private float[] afterimageRotations = new float[MaxAfterimages];
        private int afterimageHead = 0;

        // ── Phase definitions ──
        private static readonly int[] PhaseDuration = { 18, 22, 26 };
        private static readonly float[] ArcStart = { -1.8f, 1.2f, -2.4f };
        private static readonly float[] ArcEnd = { 1.2f, -1.5f, 2.0f };

        public override void SetDefaults()
        {
            Projectile.width = 110;
            Projectile.height = 110;
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

            // ── Meditation check (no nearby enemies + holding) ──
            if (!meditationCharged)
            {
                bool enemiesNearby = false;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (Main.npc[i].active && !Main.npc[i].friendly && Main.npc[i].CanBeChasedBy()
                        && Vector2.Distance(player.Center, Main.npc[i].Center) < 400f)
                    {
                        enemiesNearby = true;
                        break;
                    }
                }
                if (!enemiesNearby)
                {
                    meditationTimer++;
                    if (meditationTimer >= 90) // 1.5 seconds
                    {
                        meditationCharged = true;
                        EroicaVFXLibrary.HaloBurst(player.Center, 0.8f);
                        SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.4f, Volume = 0.5f }, player.Center);
                    }
                }
                else
                {
                    meditationTimer = 0f;
                }
            }

            // ── Swing interpolation ──
            int phaseIdx = (int)MathHelper.Clamp(ComboPhase, 0, 2);
            float duration = PhaseDuration[phaseIdx];
            float progress = MathHelper.Clamp(PhaseTimer / duration, 0f, 1f);
            float eased = EaseFlowing(progress);

            float arcScale = meditationCharged ? 1.4f : 1f;
            float startAngle = ArcStart[phaseIdx] * swingDirection * arcScale;
            float endAngle = ArcEnd[phaseIdx] * swingDirection * arcScale;
            swingRotation = MathHelper.Lerp(startAngle, endAngle, eased);
            Projectile.rotation = swingRotation;

            // Afterimage
            afterimageRotations[afterimageHead] = swingRotation;
            afterimageHead = (afterimageHead + 1) % MaxAfterimages;

            // ── Blade tip ──
            float bladeLen = 88f;
            Vector2 tipDir = swingRotation.ToRotationVector2();
            Vector2 tipPos = player.Center + tipDir * bladeLen;

            SpawnPerFrameVFX(tipPos, phaseIdx, player);

            // ── Phase projectiles at midpoint ──
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
                ComboPhase = (ComboPhase + 1) % 3;
                PhaseTimer = 0;
                swingDirection = -swingDirection;
                Projectile.direction = swingDirection;
                phaseProjectileSpawned = false;
                if (meditationCharged) meditationCharged = false;

                for (int i = 0; i < MaxAfterimages; i++)
                    afterimageRotations[i] = ArcStart[(int)ComboPhase] * swingDirection;
            }

            // ── Player lock ──
            player.direction = Main.MouseWorld.X >= player.Center.X ? 1 : -1;
            player.heldProj = Projectile.whoAmI;
            player.itemAnimation = 2;
            player.itemTime = 2;
        }

        private static float EaseFlowing(float t)
        {
            // Smooth flowing ease — gentler than standard for sakura grace
            return MathF.Sin(t * MathHelper.PiOver2);
        }

        #region VFX Spawning

        private void SpawnPerFrameVFX(Vector2 tipPos, int phase, Player player)
        {
            // Sakura petals from blade
            if ((int)PhaseTimer % 3 == 0)
                EroicaVFXLibrary.SpawnSakuraPetals(tipPos, 1 + phase, 15f);

            // Sakura music notes
            if ((int)PhaseTimer % 7 == 0)
                EroicaVFXLibrary.SpawnSakuraMusicNotes(tipPos, 1, 12f);

            // Soft sparkles
            if ((int)PhaseTimer % 4 == 0)
                EroicaVFXLibrary.SpawnValorSparkles(tipPos, 1 + phase, 12f);

            // Meditation aura — glowing rings while charged
            if (meditationCharged && (int)PhaseTimer % 8 == 0)
            {
                var ring = new BloomRingParticle(player.Center, Vector2.Zero, EroicaPalette.Sakura, 0.4f, 25, 0.06f);
                MagnumParticleHandler.SpawnParticle(ring);
            }

            // Dynamic lighting
            EroicaVFXLibrary.AddPaletteLighting(tipPos, 0.6f + phase * 0.1f, 0.5f + phase * 0.1f);
        }

        private void SpawnPhaseProjectiles(Player player, Vector2 tipPos, int phase)
        {
            if (Main.myPlayer != Projectile.owner) return;

            if (phase == 1) // Scattered Petals: 8 homing spectral copies
            {
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 dir = angle.ToRotationVector2() * 5f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipPos, dir,
                        ModContent.ProjectileType<SakurasBlossomSpectral>(),
                        (int)(Projectile.damage * 0.3f), Projectile.knockBack * 0.3f, Projectile.owner);
                }
                SoundEngine.PlaySound(SoundID.Item43 with { Pitch = 0.3f, Volume = 0.6f }, player.Center);
            }
        }

        private void OnPhaseEnd(Player player, Vector2 tipPos, int phase)
        {
            EroicaVFXLibrary.MeleeImpact(tipPos, phase);

            if (phase == 2) // Final Bloom — massive petal burst
            {
                EroicaVFXLibrary.FinisherSlam(tipPos, 0.8f);
                EroicaVFXLibrary.SpawnSakuraPetals(tipPos, 15, 80f);
                EroicaVFXLibrary.SpawnSakuraMusicNotes(tipPos, 5, 40f);
                EroicaVFXLibrary.HaloBurst(tipPos, 1f);
            }
            else
            {
                EroicaVFXLibrary.SpawnSakuraPetals(tipPos, 5 + phase * 3, 40f);
            }
        }

        #endregion

        #region On Hit

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 240);
            target.AddBuff(ModContent.BuffType<SakuraBlight>(), 180);
            target.AddBuff(ModContent.BuffType<PetalWound>(), 120);

            int phase = (int)MathHelper.Clamp(ComboPhase, 0, 2);
            EroicaVFXLibrary.MeleeImpact(target.Center, phase);
            EroicaVFXLibrary.SpawnSakuraPetals(target.Center, 4 + phase * 2, 30f);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // Meditation charged: 2x damage on next swing
            if (meditationCharged)
                modifiers.FinalDamage *= 2f;

            int phase = (int)MathHelper.Clamp(ComboPhase, 0, 2);
            modifiers.FinalDamage *= 1f + phase * 0.1f;
        }

        #endregion

        #region Rendering

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Player player = Main.player[Projectile.owner];

            Texture2D bladeTex = TextureAssets.Item[ModContent.ItemType<SakurasBlossom>()].Value;
            Vector2 bladeOrigin = new Vector2(0, bladeTex.Height);
            float bladeScale = 1.1f * (meditationCharged ? 1.15f : 1f);
            SpriteEffects flip = swingDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;

            int phase = (int)MathHelper.Clamp(ComboPhase, 0, 2);
            Vector2 playerDraw = player.Center - Main.screenPosition;
            float drawBaseRot = swingRotation + (swingDirection > 0 ? -MathHelper.PiOver4 : MathHelper.PiOver4 + MathHelper.Pi);

            // ── Layer 1: Sakura petal afterimage trail ──
            DrawAfterimages(sb, bladeTex, bladeOrigin, bladeScale, flip, playerDraw, phase, drawBaseRot);

            // ── Layer 2: Main blade sprite ──
            float paletteT = 0.4f + phase * 0.1f;
            Color bladeTint = Color.Lerp(lightColor, EroicaPalette.PaletteLerp(EroicaPalette.SakurasBlossomBlade, paletteT), 0.3f);
            sb.Draw(bladeTex, playerDraw, null, bladeTint, drawBaseRot, bladeOrigin, bladeScale, flip, 0f);

            // Inner sakura glow
            Color bladeGlow = EroicaPalette.Sakura with { A = 0 };
            sb.Draw(bladeTex, playerDraw, null, bladeGlow * 0.25f, drawBaseRot, bladeOrigin, bladeScale * 1.03f, flip, 0f);

            // ── Layer 3: Bloom at blade tip ──
            DrawBladeTipBloom(sb, player, phase);

            // ── Layer 4: Meditation aura ──
            if (meditationCharged)
                DrawMeditationGlow(sb, player);

            return false;
        }

        private void DrawAfterimages(SpriteBatch sb, Texture2D tex, Vector2 origin, float scale,
            SpriteEffects flip, Vector2 playerDraw, int phase, float currentDrawRot)
        {
            int count = 5 + phase;

            for (int i = 0; i < count && i < MaxAfterimages; i++)
            {
                int idx = (afterimageHead - 2 - i + MaxAfterimages) % MaxAfterimages;
                float rot = afterimageRotations[idx];
                float drawRot = rot + (swingDirection > 0 ? -MathHelper.PiOver4 : MathHelper.PiOver4 + MathHelper.Pi);

                float fade = 1f - (float)(i + 1) / (count + 1);
                fade *= fade;

                // Sakura color gradient for afterimages
                Color col = Color.Lerp(EroicaPalette.Sakura, EroicaPalette.PollenGold, (float)i / count) * (fade * 0.3f);
                col.A = 0;

                sb.Draw(tex, playerDraw, null, col, drawRot, origin, scale * (1f - i * 0.02f), flip, 0f);
            }
        }

        private void DrawBladeTipBloom(SpriteBatch sb, Player player, int phase)
        {
            float bladeLen = 88f;
            Vector2 tipPos = player.Center + swingRotation.ToRotationVector2() * bladeLen;

            // Sakura-themed bloom: pink outer, gold inner
            EroicaVFXLibrary.DrawEroicaBloomStack(sb, tipPos,
                EroicaPalette.Sakura, EroicaPalette.PollenGold,
                0.25f + phase * 0.06f, 0.7f + phase * 0.1f);
        }

        private void DrawMeditationGlow(SpriteBatch sb, Player player)
        {
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            if (bloom == null) return;

            Vector2 drawPos = player.Center - Main.screenPosition;
            Vector2 bloomOrigin = bloom.Size() * 0.5f;
            float pulse = 0.8f + MathF.Sin((float)Main.GameUpdateCount * 0.06f) * 0.2f;

            Color meditColor = EroicaPalette.Sakura with { A = 0 };
            sb.Draw(bloom, drawPos, null, meditColor * (0.18f * pulse), 0f, bloomOrigin,
                1.5f, SpriteEffects.None, 0f);

            Color innerColor = EroicaPalette.PollenGold with { A = 0 };
            sb.Draw(bloom, drawPos, null, innerColor * (0.12f * pulse), 0f, bloomOrigin,
                0.8f, SpriteEffects.None, 0f);
        }

        #endregion
    }
}