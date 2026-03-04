using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Debuffs;
using MagnumOpus.Content.Nachtmusik.Weapons.NebulasWhisper.Utilities;

namespace MagnumOpus.Content.Nachtmusik.Weapons.NebulasWhisper.Projectiles
{
    /// <summary>
    /// Nebula Whisper Shot — Expanding nebula projectile that grows as it travels.
    /// Phases through first 48 pixels of walls. Scale grows from 0.5 to 2.0.
    /// Slows as it expands. Leaves residue puffs. ai[0] == 1 = Whisper Storm mode.
    /// </summary>
    public class NebulaWhisperShot : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/WideSoftEllipse";

        private int phaseTicks = 0;
        private const int PhasePixelDistance = 48; // 3 tiles
        private float currentScale = 0.5f;
        private bool IsStormMode => Projectile.ai[0] == 1f;

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            int ticksAlive = 120 - Projectile.timeLeft;

            if (IsStormMode)
            {
                // Storm mode: stationary expanding implosion-explosion
                StormAI(ticksAlive);
                return;
            }

            // === PHASE THROUGH WALLS for first 48 pixels ===
            phaseTicks++;
            float distTraveled = phaseTicks * Projectile.velocity.Length();
            // After phasing distance, enable tile collision
            if (distTraveled > PhasePixelDistance && !Projectile.tileCollide)
            {
                Projectile.tileCollide = true;
            }

            // === EXPAND OVER TIME ===
            currentScale = MathHelper.Lerp(0.5f, 2.0f, MathHelper.Clamp(ticksAlive / 80f, 0f, 1f));

            // Grow hitbox width
            int newWidth = (int)MathHelper.Lerp(20f, 60f, MathHelper.Clamp(ticksAlive / 80f, 0f, 1f));
            Projectile.width = newWidth;
            Projectile.height = newWidth;

            // === SLOW AS IT GROWS (after 30 ticks) ===
            if (ticksAlive > 30)
            {
                Projectile.velocity *= 0.985f;
            }

            Projectile.rotation += 0.02f;

            // === DENSE BILLOWING NEBULA TRAIL ===
            for (int i = 0; i < 3; i++)
            {
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(
                    Projectile.width * 0.4f, Projectile.height * 0.4f);
                Vector2 dustVel = -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(2f, 2f);

                // Alternating deep indigo, cosmic blue, violet smoke
                int dustType;
                switch (i % 3)
                {
                    case 0: dustType = DustID.PurpleTorch; break;
                    case 1: dustType = DustID.BlueTorch; break;
                    default: dustType = DustID.PinkTorch; break;
                }

                Dust d = Dust.NewDustPerfect(dustPos, dustType, dustVel, 80, default, 1.1f * currentScale * 0.5f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // === RESIDUE PUFFS that linger ===
            if (ticksAlive % 12 == 0)
            {
                NebulasWhisperVFX.NebulaResidueVFX(Projectile.Center);
            }

            // === COSMIC MOTES ===
            if (Main.rand.NextBool(3))
            {
                var glow = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(Projectile.width * 0.3f, Projectile.height * 0.3f),
                    Main.rand.NextVector2Circular(0.5f, 0.5f),
                    NachtmusikPalette.CosmicPurple * 0.6f, 0.2f * currentScale * 0.5f, 20, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            Lighting.AddLight(Projectile.Center, NachtmusikPalette.CosmicPurple.ToVector3() * 0.4f * currentScale * 0.5f);
        }

        private void StormAI(int ticksAlive)
        {
            // Storm mode: stationary expanding damage field
            currentScale = MathHelper.Lerp(1f, 3.5f, MathHelper.Clamp(ticksAlive / 60f, 0f, 1f));
            int stormRadius = (int)MathHelper.Lerp(40f, 120f, MathHelper.Clamp(ticksAlive / 60f, 0f, 1f));
            Projectile.width = stormRadius;
            Projectile.height = stormRadius;
            Projectile.velocity = Vector2.Zero;
            Projectile.rotation += 0.04f;

            // Inward-streaming particles
            for (int i = 0; i < 5; i++)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = stormRadius * 1.5f + Main.rand.NextFloat() * 40f;
                Vector2 spawnPos = Projectile.Center + angle.ToRotationVector2() * dist;
                Vector2 vel = (Projectile.Center - spawnPos).SafeNormalize(Vector2.Zero) * (3f + Main.rand.NextFloat() * 2f);

                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.BlueTorch;
                Dust d = Dust.NewDustPerfect(spawnPos, dustType, vel, 60, default, 1.3f);
                d.noGravity = true;
                d.fadeIn = 1.0f;
            }

            // Core nebula churn
            for (int i = 0; i < 3; i++)
            {
                Vector2 churnPos = Projectile.Center + Main.rand.NextVector2Circular(stormRadius * 0.5f, stormRadius * 0.5f);
                Dust c = Dust.NewDustPerfect(churnPos, DustID.PinkTorch,
                    Main.rand.NextVector2Circular(1.5f, 1.5f), 50, default, 1.0f);
                c.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, NachtmusikPalette.Violet.ToVector3() * 0.6f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 360);
            if (target.TryGetGlobalNPC(out CelestialHarmonyNPC harmonyNPC))
                harmonyNPC.AddStack(target, 1);

            // Nebula burst cloud on hit
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * (2f + Main.rand.NextFloat() * 2f);
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.PinkTorch;
                Dust d = Dust.NewDustPerfect(target.Center, dustType, vel, 60, default, 0.9f);
                d.noGravity = true;
            }

            // Nebula slow indicator flash
            CustomParticles.GenericFlare(target.Center, NachtmusikPalette.CosmicPurple, 0.4f, 14);
            CustomParticles.GenericFlare(target.Center, NachtmusikPalette.Violet, 0.3f, 12);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 pos = Projectile.Center - Main.screenPosition;

            float time = (float)Main.timeForVisualEffects * 0.03f;
            float breathe = 1f + MathF.Sin(time * 1.5f) * 0.08f;

            // Opacity decreases as size increases
            float baseOpacity = MathHelper.Lerp(0.7f, 0.25f, MathHelper.Clamp((currentScale - 0.5f) / 1.5f, 0f, 1f));

            // ═══════════════════════════════════════════════════════════════
            //  SHADER LAYER: Serenade nebula aura — swirling cosmic presence
            //  Nebula shots don't have trails, but the expanding cloud IS the VFX
            // ═══════════════════════════════════════════════════════════════
            if (NachtmusikShaderManager.HasSerenade)
            {
                Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
                if (glowTex != null)
                {
                    float phase = IsStormMode ? 0.85f : MathHelper.Clamp(currentScale / 2f, 0f, 1f);

                    NachtmusikShaderManager.BeginShaderAdditive(Main.spriteBatch);
                    NachtmusikShaderManager.ApplySerenade(time, NachtmusikPalette.CosmicPurple,
                        NachtmusikPalette.Violet, phase: phase);

                    float auraScale = currentScale * 0.35f * breathe;
                    Main.spriteBatch.Draw(glowTex, pos, null,
                        NachtmusikPalette.CosmicPurple with { A = 0 } * baseOpacity * 0.4f,
                        Projectile.rotation * 0.5f, glowTex.Size() / 2f, auraScale, SpriteEffects.None, 0f);

                    // Storm mode: extra vortex aura
                    if (IsStormMode)
                    {
                        Main.spriteBatch.Draw(glowTex, pos, null,
                            NachtmusikPalette.Violet with { A = 0 } * 0.25f,
                            -Projectile.rotation, glowTex.Size() / 2f, auraScale * 1.3f, SpriteEffects.None, 0f);
                    }

                    NachtmusikShaderManager.RestoreSpriteBatch(Main.spriteBatch);
                }
            }

            // ═══════════════════════════════════════════════════════════════
            //  BLOOM LAYER: Multi-layer additive nebula rendering — palette-driven
            // ═══════════════════════════════════════════════════════════════

            // Deep cosmic purple outer haze
            Main.spriteBatch.Draw(tex, pos, null,
                NachtmusikPalette.CosmicPurple with { A = 0 } * baseOpacity * 0.6f,
                Projectile.rotation, origin, currentScale * breathe * 1.3f, SpriteEffects.None, 0f);

            // Violet mid-body
            float violetShift = MathF.Sin(time * 2f) * 0.15f + 0.85f;
            Main.spriteBatch.Draw(tex, pos, null,
                NachtmusikPalette.Violet with { A = 0 } * baseOpacity * 0.5f * violetShift,
                Projectile.rotation * 0.7f, origin, currentScale * breathe * 1.0f, SpriteEffects.None, 0f);

            // Deep indigo inner swirl
            Main.spriteBatch.Draw(tex, pos, null,
                NachtmusikPalette.DeepBlue with { A = 0 } * baseOpacity * 0.45f,
                -Projectile.rotation * 0.5f, origin, currentScale * breathe * 0.7f, SpriteEffects.None, 0f);

            // Star white shimmer core
            float corePulse = MathF.Sin(time * 3.5f) * 0.5f + 0.5f;
            Main.spriteBatch.Draw(tex, pos, null,
                NachtmusikPalette.StarWhite with { A = 0 } * baseOpacity * 0.35f * corePulse,
                0f, origin, currentScale * breathe * 0.4f, SpriteEffects.None, 0f);

            // Extra bloom halo for Storm mode
            if (IsStormMode)
            {
                Texture2D bloomTex = MagnumTextureRegistry.GetSoftGlow();
                if (bloomTex != null)
                {
                    NachtmusikShaderManager.BeginAdditive(Main.spriteBatch);
                    Main.spriteBatch.Draw(bloomTex, pos, null,
                        NachtmusikPalette.Violet with { A = 0 } * 0.2f,
                        0f, bloomTex.Size() / 2f, currentScale * 0.5f * breathe, SpriteEffects.None, 0f);
                    NachtmusikShaderManager.RestoreSpriteBatch(Main.spriteBatch);
                }
            }

            // Nachtmusik theme star flare accent
            NachtmusikShaderManager.BeginAdditive(Main.spriteBatch);
            NachtmusikVFXLibrary.DrawThemeStarFlare(Main.spriteBatch, Projectile.Center, 1f, 0.5f);
            NachtmusikShaderManager.RestoreSpriteBatch(Main.spriteBatch);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // Final nebula dispersal
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * (2f + Main.rand.NextFloat() * 3f);
                int dustType = i % 2 == 0 ? DustID.PurpleTorch : DustID.BlueTorch;
                Dust d = Dust.NewDustPerfect(Projectile.Center, dustType, vel, 80, default, 0.8f);
                d.noGravity = true;
            }

            NachtmusikVFXLibrary.DrawBloom(Projectile.Center, 0.3f * currentScale, 0.5f);
        }
    }
}
