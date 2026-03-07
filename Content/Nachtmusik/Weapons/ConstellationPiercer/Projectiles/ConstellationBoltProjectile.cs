using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Debuffs;
using MagnumOpus.Content.Nachtmusik.Weapons.ConstellationPiercer.Utilities;

namespace MagnumOpus.Content.Nachtmusik.Weapons.ConstellationPiercer.Projectiles
{
    /// <summary>
    /// Constellation Bolt — Piercing star bolt that chains between enemies.
    /// Marks each struck enemy as a Star Point. Gentle homing after 15 ticks.
    /// Dense cosmic dust trail with orbiting constellation motes and dot-spaced star points.
    /// </summary>
    public class ConstellationBoltProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarSoft";

        // Nachtmusik hue range — deep indigo to starlight silver spectrum
        private const float HueMin = 0.60f;
        private const float HueMax = 0.72f;

        private int chainCount = 0;
        private const int MaxChains = 4;
        private readonly List<int> hitEnemies = new List<int>();

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 4;
            Projectile.timeLeft = 200;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            int ticksAlive = 200 - Projectile.timeLeft;

            // === GENTLE HOMING AFTER 15 TICKS ===
            if (ticksAlive > 15)
            {
                NPC target = FindClosestTarget(600f);
                if (target != null)
                {
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), 0.04f);
                }
            }

            // === DENSE COSMIC DUST TRAIL — alternating Deep Indigo and Starlight Silver ===
            for (int i = 0; i < 2; i++)
            {
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(6f, 6f);
                Vector2 dustVel = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                bool isIndigo = i % 2 == 0;
                int dustType = isIndigo ? DustID.PurpleTorch : DustID.BlueTorch;
                Dust dust = Dust.NewDustPerfect(dustPos, dustType, dustVel, 0, default, 1.2f);
                dust.noGravity = true;
                dust.fadeIn = 1.1f;
            }

            // === STARLIGHT SILVER SPARKLE ACCENTS ===
            if (Main.rand.NextBool(2))
            {
                Vector2 sparkPos = Projectile.Center + Main.rand.NextVector2Circular(5f, 5f);
                Dust silver = Dust.NewDustPerfect(sparkPos, DustID.SilverCoin,
                    -Projectile.velocity * 0.08f, 0, NachtmusikPalette.StarWhite, 0.9f);
                silver.noGravity = true;
            }

            // === DOT-SPACED STAR POINT PARTICLES — lingering every 8 ticks ===
            if (ticksAlive % 8 == 0)
            {
                var starPoint = new GenericGlowParticle(
                    Projectile.Center, Vector2.Zero,
                    NachtmusikPalette.StarlightCore * 0.7f, 0.25f, 40, true);
                MagnumParticleHandler.SpawnParticle(starPoint);
            }

            // === 4 ORBITING CONSTELLATION MOTES ===
            if (ticksAlive % 5 == 0)
            {
                float orbitAngle = Main.GameUpdateCount * 0.08f;
                for (int i = 0; i < 4; i++)
                {
                    float angle = orbitAngle + MathHelper.TwoPi * i / 4f;
                    Vector2 motePos = Projectile.Center + angle.ToRotationVector2() * 14f;
                    float hue = HueMin + (i / 4f) * (HueMax - HueMin);
                    Color moteColor = Main.hslToRgb(hue, 0.88f, 0.78f);
                    CustomParticles.GenericFlare(motePos, moteColor, 0.2f, 10);
                }
            }

            // === FLARE OSCILLATION ===
            if (Main.rand.NextBool(2))
            {
                float hue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color flareColor = Main.hslToRgb(hue, 0.9f, 0.75f);
                CustomParticles.GenericFlare(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    flareColor, 0.3f, 14);
            }

            Lighting.AddLight(Projectile.Center, NachtmusikPalette.ConstellationBlue.ToVector3() * 0.6f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply CelestialHarmony debuff
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 300);
            if (target.TryGetGlobalNPC(out CelestialHarmonyNPC harmonyNPC))
                harmonyNPC.AddStack(target, 1);

            // === 3-LAYER IMPACT FLASH ===
            CustomParticles.GenericFlare(target.Center, Color.White, 0.55f, 18);
            CustomParticles.GenericFlare(target.Center, NachtmusikPalette.ConstellationBlue, 0.45f, 16);
            CustomParticles.GenericFlare(target.Center, NachtmusikPalette.StarGold, 0.35f, 14);

            // === STAR CHAIN BURST — music notes scatter ===
            NachtmusikVFXLibrary.SpawnMusicNotes(target.Center, 3, 14f, 0.5f, 0.8f, 24);

            // === SPARKLE SCATTER ===
            for (int s = 0; s < 5; s++)
            {
                float angle = MathHelper.TwoPi * s / 5f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                var sparkle = new SparkleParticle(target.Center, sparkVel,
                    NachtmusikPalette.StarWhite, 0.3f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Star Point creation VFX
            ConstellationPiercerVFX.StarPointCreationVFX(target.Center);

            hitEnemies.Add(target.whoAmI);

            // === CHAIN TO NEXT ENEMY within 300f ===
            if (chainCount < MaxChains)
            {
                NPC nextTarget = FindNextChainTarget(target.Center, 300f);
                if (nextTarget != null)
                {
                    chainCount++;
                    Vector2 toNext = (nextTarget.Center - target.Center).SafeNormalize(Vector2.UnitX);
                    Projectile.velocity = toNext * Projectile.velocity.Length() * 0.95f;
                    Projectile.Center = target.Center;

                    // Constellation line VFX between star points
                    ConstellationPiercerVFX.ConstellationLineVFX(target.Center, nextTarget.Center);

                    // Chain line glow particles
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 linePos = Vector2.Lerp(target.Center, target.Center + toNext * 60f, i / 8f);
                        float hue = HueMin + (i / 8f) * (HueMax - HueMin);
                        Color lineColor = Main.hslToRgb(hue, 0.88f, 0.75f);
                        var line = new GenericGlowParticle(linePos, Vector2.Zero,
                            lineColor * 0.8f, 0.22f, 12, true);
                        MagnumParticleHandler.SpawnParticle(line);
                    }
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            // === STAR SPARKLE DISSIPATION BURST ===
            NachtmusikVFXLibrary.SpawnShatteredStarlight(Projectile.Center, 4, 3.5f, 0.6f, true);
            NachtmusikVFXLibrary.SpawnStarBurst(Projectile.Center, 6, 0.35f);

            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.BlueTorch, vel, 0, default, 0.7f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, NachtmusikPalette.ConstellationBlue.ToVector3() * 0.8f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = tex.Size() * 0.5f;

            float time = (float)Main.timeForVisualEffects * 0.03f;

            // ═══════════════════════════════════════════════════════════════
            //  SHADER LAYER 1: StarChainBeam GPU trail
            //  Constellation-blue piercing bolt trail with cosmic style
            // ═══════════════════════════════════════════════════════════════
            {
                int validCount = 0;
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] != Vector2.Zero) validCount++;
                    else break;
                }

                if (validCount > 2)
                {
                    var trailPositions = new Vector2[validCount];
                    for (int i = 0; i < validCount; i++)
                        trailPositions[i] = Projectile.oldPos[i] + Projectile.Size * 0.5f;

                    CalamityStyleTrailRenderer.DrawDualLayerTrail(
                        trailPositions, null, CalamityStyleTrailRenderer.TrailStyle.Cosmic,
                        9f, NachtmusikPalette.ConstellationBlue * 0.45f, NachtmusikPalette.StarWhite * 0.35f,
                        0.5f, bodyOverbright: 2.5f, coreOverbright: 4.5f, coreWidthRatio: 0.3f);
                }
            }

            // ═══════════════════════════════════════════════════════════════
            //  BLOOM LAYER: Multi-scale additive star core + bloom halo
            // ═══════════════════════════════════════════════════════════════
            {
                Vector2 pos = Projectile.Center - Main.screenPosition;
                float pulse = 1f + MathF.Sin(time * 3f) * 0.1f;

                NachtmusikShaderManager.BeginAdditive(Main.spriteBatch);

                // Cosmic Blue outer
                Main.spriteBatch.Draw(tex, pos, null,
                    NachtmusikPalette.ConstellationBlue with { A = 0 } * 0.5f,
                    Projectile.rotation, origin, 0.8f * pulse, SpriteEffects.None, 0f);

                // Starlight Silver mid
                Main.spriteBatch.Draw(tex, pos, null,
                    NachtmusikPalette.StarWhite with { A = 0 } * 0.45f,
                    Projectile.rotation, origin, 0.55f * pulse, SpriteEffects.None, 0f);

                // Stellar White core
                Main.spriteBatch.Draw(tex, pos, null,
                    NachtmusikPalette.StarlightCore with { A = 0 } * 0.4f,
                    Projectile.rotation, origin, 0.35f * pulse, SpriteEffects.None, 0f);

                // Bloom halo from texture registry
                Texture2D bloomTex = MagnumTextureRegistry.GetSoftGlow();
                if (bloomTex != null)
                {
                    Vector2 bloomOrigin = bloomTex.Size() / 2f;
                    Main.spriteBatch.Draw(bloomTex, pos, null,
                        NachtmusikPalette.ConstellationBlue with { A = 0 } * 0.2f,
                        0f, bloomOrigin, 0.25f * pulse, SpriteEffects.None, 0f);
                }

                // Star flare accent
                Texture2D flareTex = MagnumTextureRegistry.GetRadialBloom();
                if (flareTex != null)
                {
                    Main.spriteBatch.Draw(flareTex, pos, null,
                        NachtmusikPalette.StarGold with { A = 0 } * 0.15f,
                        time * 0.4f, flareTex.Size() / 2f, 0.12f * pulse, SpriteEffects.None, 0f);
                }

                NachtmusikShaderManager.RestoreSpriteBatch(Main.spriteBatch);
            }

            // Nachtmusik theme star flare accent
            NachtmusikShaderManager.BeginAdditive(Main.spriteBatch);
            NachtmusikVFXLibrary.DrawThemeStarFlare(Main.spriteBatch, Projectile.Center, 1f, 0.5f);
            NachtmusikShaderManager.RestoreSpriteBatch(Main.spriteBatch);

            return false;
        }

        private NPC FindClosestTarget(float range)
        {
            NPC closest = null;
            float closestDist = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy(Projectile))
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }
            return closest;
        }

        private NPC FindNextChainTarget(Vector2 from, float range)
        {
            NPC closest = null;
            float closestDist = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (hitEnemies.Contains(i)) continue;
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy(Projectile))
                {
                    float dist = Vector2.Distance(from, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }
            return closest;
        }
    }
}
