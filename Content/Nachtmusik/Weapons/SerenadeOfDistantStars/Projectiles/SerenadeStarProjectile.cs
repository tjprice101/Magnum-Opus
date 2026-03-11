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

            // === SPARKLE ACCENTS (palette-ramped) ===
            if (Main.rand.NextBool(4))
                NachtmusikVFXLibrary.SpawnGradientSparkles(Projectile.Center, Projectile.velocity, 1, 0.25f, 16, 6f);

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

            // Palette-ramped sparkle explosion
            NachtmusikVFXLibrary.SpawnGradientSparkleExplosion(target.Center, 8, 5f, 0.3f);

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
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.Nachtmusik, ref _vertexStrip);

                // Serenade accent: StarHomingTrail shader-driven rhythm flare
                float time = (float)Main.timeForVisualEffects * 0.03f;
                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
                if (glow != null && NachtmusikShaderManager.HasStarHomingTrail)
                {
                    Vector2 origin = glow.Size() / 2f;
                    float rhythmScale = MathHelper.Clamp(RhythmStacks, 1f, 3f);
                    float pulse = 0.8f + 0.2f * MathF.Sin((float)Main.timeForVisualEffects * 0.12f * rhythmScale);
                    float velRot = Projectile.velocity.ToRotation();

                    NachtmusikShaderManager.BeginShaderAdditive(sb);

                    // Rhythm-scaled directional flare with StarHomingTrail shader
                    NachtmusikShaderManager.ApplyStarHomingTrail(time);
                    float intensity = 0.18f + 0.12f * (rhythmScale - 1f);
                    sb.Draw(glow, drawPos, null,
                        (NachtmusikPalette.StarGold with { A = 0 }) * intensity * pulse,
                        velRot, origin, new Vector2(0.12f + 0.04f * rhythmScale, 0.03f), SpriteEffects.None, 0f);

                    // Serenade glow pass — perpendicular shimmer
                    NachtmusikShaderManager.ApplyStarHomingTrailGlow(time);
                    sb.Draw(glow, drawPos, null,
                        (NachtmusikPalette.SerenadeGlow with { A = 0 }) * 0.18f * pulse,
                        velRot + MathHelper.PiOver2, origin, new Vector2(0.06f * rhythmScale, 0.02f), SpriteEffects.None, 0f);

                    // NK Lens Flare accent on high rhythm
                    if (rhythmScale > 1.5f)
                    {
                        Texture2D flareTex = NachtmusikThemeTextures.NKLensFlare?.Value;
                        if (flareTex != null)
                        {
                            float flareAlpha = (rhythmScale - 1.5f) / 1.5f;
                            Vector2 flareOrigin = flareTex.Size() / 2f;
                            Color flareColor = NachtmusikPalette.StarGold with { A = 0 } * 0.2f * flareAlpha * pulse;
                            sb.Draw(flareTex, drawPos, null, flareColor,
                                time * 0.4f, flareOrigin, 0.04f * pulse, SpriteEffects.None, 0f);
                        }
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

                    Vector2 origin = glow.Size() / 2f;
                    float rhythmScale = MathHelper.Clamp(RhythmStacks, 1f, 3f);
                    float pulse = 0.8f + 0.2f * MathF.Sin((float)Main.timeForVisualEffects * 0.12f * rhythmScale);
                    float velRot = Projectile.velocity.ToRotation();

                    sb.Draw(glow, drawPos, null,
                        (NachtmusikPalette.StarGold with { A = 0 }) * (0.15f + 0.1f * (rhythmScale - 1f)) * pulse,
                        velRot, origin, new Vector2(0.1f + 0.04f * rhythmScale, 0.03f), SpriteEffects.None, 0f);
                    sb.Draw(glow, drawPos, null,
                        (NachtmusikPalette.SerenadeGlow with { A = 0 }) * 0.15f * pulse,
                        velRot + MathHelper.PiOver2, origin, new Vector2(0.06f * rhythmScale, 0.02f), SpriteEffects.None, 0f);
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
    }
}
