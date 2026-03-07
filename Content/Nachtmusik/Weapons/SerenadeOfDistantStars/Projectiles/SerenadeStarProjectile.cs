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
using MagnumOpus.Content.Nachtmusik.Weapons.SerenadeOfDistantStars.Utilities;

namespace MagnumOpus.Content.Nachtmusik.Weapons.SerenadeOfDistantStars.Projectiles
{
    /// <summary>
    /// Serenade Star Projectile — Homing star with rhythm-scaling homing strength.
    /// Strong homing after 30 tick delay. Trail cache 16, trailing mode 2.
    /// Star Memory: tracks enemies passed within 5 tiles (up to 4), spawns echoes on kill/expire.
    /// ai[0] = rhythm stack count at time of fire.
    /// </summary>
    public class SerenadeStarProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarSoft";

        private const float BaseHomingStrength = 0.04f;
        private const float HomingPerStack = 0.01f;
        private const float MaxRange = 1280f; // 80 tiles
        private const float MemoryRange = 80f; // 5 tiles
        private const int MaxMemories = 4;

        private readonly List<int> rememberedEnemies = new List<int>();
        private int RhythmStacks => (int)Projectile.ai[0];

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
            Projectile.penetrate = 2;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            int ticksAlive = 300 - Projectile.timeLeft;

            float homingStrength = BaseHomingStrength + HomingPerStack * RhythmStacks;

            // === STRONG HOMING after 30 tick delay ===
            if (ticksAlive > 30)
            {
                NPC target = FindClosestTarget(MaxRange);
                if (target != null)
                {
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    float targetSpeed = RhythmStacks >= 5 ? 16f : Projectile.velocity.Length();
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * targetSpeed, homingStrength);
                }
            }

            // === STAR MEMORY — track enemies passed within 5 tiles ===
            if (rememberedEnemies.Count < MaxMemories)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (rememberedEnemies.Contains(i)) continue;
                    NPC npc = Main.npc[i];
                    if (npc.active && npc.CanBeChasedBy(Projectile) &&
                        Vector2.Distance(Projectile.Center, npc.Center) < MemoryRange)
                    {
                        rememberedEnemies.Add(i);
                        if (rememberedEnemies.Count >= MaxMemories) break;
                    }
                }
            }

            // === DENSE DUST TRAIL — alternating star gold and cosmic blue ===
            for (int i = 0; i < 2; i++)
            {
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(5f, 5f);
                Vector2 dustVel = -Projectile.velocity * 0.12f + Main.rand.NextVector2Circular(1f, 1f);
                int dustType = i % 2 == 0 ? DustID.GoldFlame : DustID.BlueTorch;
                Dust d = Dust.NewDustPerfect(dustPos, dustType, dustVel, 0, default, 1.0f);
                d.noGravity = true;
                d.fadeIn = 1.0f;
            }

            // === EXTENDED PERSISTENCE SHIMMER TRAIL ===
            if (Main.rand.NextBool(2))
            {
                var glow = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    -Projectile.velocity * 0.06f,
                    NachtmusikPalette.StarGold * 0.5f, 0.2f, 22, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // === 4-POINT CONSTELLATION MOTES ===
            if (ticksAlive % 6 == 0)
            {
                float orbitAngle = Main.GameUpdateCount * 0.07f;
                for (int i = 0; i < 4; i++)
                {
                    float angle = orbitAngle + MathHelper.TwoPi * i / 4f;
                    Vector2 motePos = Projectile.Center + angle.ToRotationVector2() * 12f;
                    Color moteColor = Color.Lerp(NachtmusikPalette.StarGold, NachtmusikPalette.StarlitBlue, i / 4f);
                    CustomParticles.GenericFlare(motePos, moteColor, 0.18f, 10);
                }
            }

            // === MUSIC NOTES every 8 ticks ===
            if (ticksAlive % 8 == 0)
            {
                NachtmusikVFXLibrary.SpawnMusicNotes(Projectile.Center, 1, 8f, 0.4f, 0.7f, 20);
            }

            // === SPARKLE ACCENTS ===
            if (Main.rand.NextBool(4))
            {
                var sparkle = new SparkleParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    -Projectile.velocity * 0.04f,
                    NachtmusikPalette.StarWhite, 0.25f, 16);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            Lighting.AddLight(Projectile.Center, NachtmusikPalette.StarGold.ToVector3() * 0.5f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 360);
            if (target.TryGetGlobalNPC(out CelestialHarmonyNPC harmonyNPC))
                harmonyNPC.AddStack(target, 1);

            // === 3-LAYER FLASH ===
            CustomParticles.GenericFlare(target.Center, Color.White, 0.55f, 18);
            CustomParticles.GenericFlare(target.Center, NachtmusikPalette.StarGold, 0.45f, 16);
            CustomParticles.GenericFlare(target.Center, NachtmusikPalette.StarlitBlue, 0.35f, 14);

            // Star burst on hit
            NachtmusikVFXLibrary.SpawnStarBurst(target.Center, 6, 0.35f);
            NachtmusikVFXLibrary.SpawnMusicNotes(target.Center, 3, 14f, 0.5f, 0.8f, 24);

            // Fire echoes at remembered enemies on hit
            SpawnStarEchoes(target);
        }

        public override void OnKill(int timeLeft)
        {
            // Fire echoes at remembered enemies on expire too
            SpawnStarEchoes(null);

            // Star sparkle dissipation
            NachtmusikVFXLibrary.SpawnShatteredStarlight(Projectile.Center, 4, 3f, 0.5f, true);

            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame, vel, 0, default, 0.7f);
                d.noGravity = true;
            }
        }

        private void SpawnStarEchoes(NPC hitTarget)
        {
            foreach (int npcIndex in rememberedEnemies)
            {
                NPC remembered = Main.npc[npcIndex];
                if (remembered.active && remembered.CanBeChasedBy(Projectile) &&
                    (hitTarget == null || remembered.whoAmI != hitTarget.whoAmI))
                {
                    Vector2 toRemembered = (remembered.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    int echoDamage = (int)(Projectile.damage * 0.4f);

                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        toRemembered * 12f,
                        ModContent.ProjectileType<StarEchoProjectile>(),
                        echoDamage,
                        Projectile.knockBack * 0.3f,
                        Projectile.owner,
                        ai0: npcIndex);

                    SerenadeOfDistantStarsVFX.StarMemoryEchoVFX(Projectile.Center);
                }
            }
            rememberedEnemies.Clear();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 pos = Projectile.Center - Main.screenPosition;

            float time = (float)Main.timeForVisualEffects * 0.03f;
            float pulse = 1f + MathF.Sin(time * 3f) * 0.1f;
            bool maxStacks = RhythmStacks >= 5;

            // ═══════════════════════════════════════════════════════════════
            //  SHADER LAYER 1: StarHomingTrail — GPU-driven star trail
            //  Replaces the old afterimage loop with a proper primitive trail
            // ═══════════════════════════════════════════════════════════════
            if (Projectile.oldPos.Length > 2)
            {
                // Build trail positions from old position cache
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

                    float trailWidth = maxStacks ? 12f : 8f;
                    CalamityStyleTrailRenderer.DrawDualLayerTrail(
                        trailPositions, null, CalamityStyleTrailRenderer.TrailStyle.Cosmic,
                        trailWidth, NachtmusikPalette.StarGold * 0.5f, NachtmusikPalette.StarlitBlue * 0.4f,
                        maxStacks ? 0.7f : 0.5f,
                        bodyOverbright: 2.5f, coreOverbright: 4f, coreWidthRatio: 0.35f);
                }
            }

            // ═══════════════════════════════════════════════════════════════
            //  SHADER LAYER 2: Serenade aura glow at projectile center
            // ═══════════════════════════════════════════════════════════════
            if (NachtmusikShaderManager.HasSerenade)
            {
                Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
                if (glowTex != null)
                {
                    NachtmusikShaderManager.BeginShaderAdditive(Main.spriteBatch);
                    NachtmusikShaderManager.ApplySerenade(time, NachtmusikPalette.StarGold,
                        NachtmusikPalette.StarlitBlue, phase: (float)(Main.timeForVisualEffects * 0.01f) % 1f);

                    float auraScale = (maxStacks ? 0.4f : 0.28f) * pulse;
                    Main.spriteBatch.Draw(glowTex, pos, null,
                        NachtmusikPalette.StarGold with { A = 0 } * 0.35f,
                        0f, glowTex.Size() / 2f, auraScale, SpriteEffects.None, 0f);

                    NachtmusikShaderManager.RestoreSpriteBatch(Main.spriteBatch);
                }
            }

            // ═══════════════════════════════════════════════════════════════
            //  BLOOM LAYER: Multi-scale additive star core + corona
            // ═══════════════════════════════════════════════════════════════
            NachtmusikShaderManager.BeginAdditive(Main.spriteBatch);

            // Star gold warm outer
            Main.spriteBatch.Draw(tex, pos, null,
                NachtmusikPalette.StarGold with { A = 0 } * 0.5f,
                Projectile.rotation, origin, 0.7f * pulse, SpriteEffects.None, 0f);

            // Starlit blue mid
            Main.spriteBatch.Draw(tex, pos, null,
                NachtmusikPalette.StarlitBlue with { A = 0 } * 0.4f,
                Projectile.rotation, origin, 0.5f * pulse, SpriteEffects.None, 0f);

            // White core
            Main.spriteBatch.Draw(tex, pos, null,
                NachtmusikPalette.StarWhite with { A = 0 } * 0.4f,
                Projectile.rotation, origin, 0.35f * pulse, SpriteEffects.None, 0f);

            // Bloom halo from texture registry
            Texture2D bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex != null)
            {
                Vector2 bloomOrigin = bloomTex.Size() / 2f;
                float bloomScale = maxStacks ? 0.35f : 0.22f;
                Main.spriteBatch.Draw(bloomTex, pos, null,
                    NachtmusikPalette.StarGold with { A = 0 } * 0.25f,
                    0f, bloomOrigin, bloomScale * pulse, SpriteEffects.None, 0f);
            }

            // Corona rays at 5 stacks
            if (maxStacks)
            {
                for (int i = 0; i < 4; i++)
                {
                    float rayAngle = time * 0.5f + MathHelper.PiOver2 * i;
                    Vector2 rayOffset = rayAngle.ToRotationVector2() * 4f;

                    Main.spriteBatch.Draw(tex, pos + rayOffset, null,
                        NachtmusikPalette.RadianceGold with { A = 0 } * 0.25f,
                        Projectile.rotation + rayAngle, origin, 0.9f * pulse, SpriteEffects.None, 0f);
                }

                // Star flare at max stacks
                Texture2D flareTex = MagnumTextureRegistry.GetRadialBloom();
                if (flareTex != null)
                {
                    Main.spriteBatch.Draw(flareTex, pos, null,
                        NachtmusikPalette.StarGold with { A = 0 } * 0.2f,
                        time * 0.3f, flareTex.Size() / 2f, 0.12f * pulse, SpriteEffects.None, 0f);
                }
            }

            NachtmusikShaderManager.RestoreSpriteBatch(Main.spriteBatch);

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
    }
}
