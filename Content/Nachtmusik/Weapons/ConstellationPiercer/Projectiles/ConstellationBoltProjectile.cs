using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.Graphics;
using MagnumOpus.Common;
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
        private VertexStrip _vertexStrip;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
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
                    Color moteColor = Color.Lerp(NachtmusikPalette.ConstellationPiercerShot[1], NachtmusikPalette.ConstellationPiercerShot[4], i / 3f);
                    CustomParticles.GenericFlare(motePos, moteColor, 0.2f, 10);
                }
            }

            // === FLARE OSCILLATION ===
            if (Main.rand.NextBool(2))
            {
                Color flareColor = Color.Lerp(NachtmusikPalette.ConstellationPiercerShot[1], NachtmusikPalette.ConstellationPiercerShot[3], Main.rand.NextFloat());
                CustomParticles.GenericFlare(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    flareColor, 0.3f, 14);
            }

            // Palette-ramped trail sparkles
            if (Main.rand.NextBool(3))
                NachtmusikVFXLibrary.SpawnGradientSparkles(Projectile.Center, Projectile.velocity, 1, 0.25f, 16, 6f);

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

            // === SPARKLE SCATTER (palette-ramped) ===
            NachtmusikVFXLibrary.SpawnGradientSparkleExplosion(target.Center, 8, 5f, 0.3f);

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
                        Color lineColor = Color.Lerp(NachtmusikPalette.ConstellationPiercerShot[1], NachtmusikPalette.ConstellationPiercerShot[4], i / 7f);
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
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                // Layer 1: Shader-driven beam trail + enhanced bloom head via IncisorOrbRenderer
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.Nachtmusik, ref _vertexStrip);

                // Layer 2: StarChainBeam shader-driven constellation crosshair
                float time = (float)Main.timeForVisualEffects * 0.03f;
                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                float life = MathHelper.Clamp((float)Projectile.timeLeft / 200f, 0f, 1f);
                float chainFade = 1f - chainCount * 0.15f;
                float pulse = 0.85f + 0.15f * MathF.Sin(Main.GameUpdateCount * 0.12f);

                Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
                if (glow != null && NachtmusikShaderManager.HasStarChainBeam)
                {
                    NachtmusikShaderManager.BeginShaderAdditive(sb);
                    NachtmusikShaderManager.ApplyStarChainBeam(time);

                    float crossRot = Projectile.velocity.ToRotation();
                    Color crossColor = NachtmusikPalette.ConstellationPiercerShot[3] with { A = 0 } * 0.35f * life * chainFade * pulse;
                    sb.Draw(glow, drawPos, null, crossColor,
                        crossRot, glow.Size() * 0.5f, new Vector2(0.18f, 0.04f), SpriteEffects.None, 0f);
                    sb.Draw(glow, drawPos, null, crossColor,
                        crossRot + MathHelper.PiOver2, glow.Size() * 0.5f, new Vector2(0.18f, 0.04f), SpriteEffects.None, 0f);

                    // Inner core glow pass
                    NachtmusikShaderManager.ApplyStarChainBeamGlow(time);
                    Color coreColor = NachtmusikPalette.ConstellationPiercerShot[5] with { A = 0 } * 0.2f * life * chainFade;
                    sb.Draw(glow, drawPos, null, coreColor,
                        0f, glow.Size() * 0.5f, 0.05f * pulse, SpriteEffects.None, 0f);

                    // NK Lens Flare accent at head
                    Texture2D flareTex = NachtmusikThemeTextures.NKLensFlare?.Value;
                    if (flareTex != null)
                    {
                        Vector2 flareOrigin = flareTex.Size() / 2f;
                        Color flareColor = NachtmusikPalette.ConstellationPiercerShot[2] with { A = 0 } * 0.25f * life * chainFade * pulse;
                        sb.Draw(flareTex, drawPos, null, flareColor,
                            time * 0.6f, flareOrigin, 0.06f * pulse, SpriteEffects.None, 0f);
                    }

                    NachtmusikShaderManager.RestoreSpriteBatch(sb);
                }
                else if (glow != null)
                {
                    // Fallback without shader
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                        SamplerState.LinearClamp, DepthStencilState.None,
                        RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                    float crossRot = Projectile.velocity.ToRotation();
                    sb.Draw(glow, drawPos, null,
                        (NachtmusikPalette.StarGold with { A = 0 }) * 0.3f * life * chainFade * pulse,
                        crossRot, glow.Size() * 0.5f, new Vector2(0.15f, 0.04f), SpriteEffects.None, 0f);
                    sb.Draw(glow, drawPos, null,
                        (NachtmusikPalette.StarGold with { A = 0 }) * 0.3f * life * chainFade * pulse,
                        crossRot + MathHelper.PiOver2, glow.Size() * 0.5f, new Vector2(0.15f, 0.04f), SpriteEffects.None, 0f);
                }
            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

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
