using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.Graphics;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Debuffs;

namespace MagnumOpus.Content.Nachtmusik.Weapons.SerenadeOfDistantStars.Projectiles
{
    /// <summary>
    /// Star Echo Projectile — Simpler secondary homing star spawned by Star Memory.
    /// Small fading homing star that seeks an assigned target.
    /// ai[0] = target NPC index.
    /// </summary>
    public class StarEchoProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarSoft";

        private VertexStrip _vertexStrip;
        private int TargetIndex => (int)Projectile.ai[0];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // === HOME TOWARD ASSIGNED TARGET ===
            if (TargetIndex >= 0 && TargetIndex < Main.maxNPCs)
            {
                NPC target = Main.npc[TargetIndex];
                if (target.active && target.CanBeChasedBy(Projectile))
                {
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 14f, 0.06f);
                }
            }

            // Fade alpha over lifetime
            float lifeProgress = 1f - (Projectile.timeLeft / 120f);
            Projectile.Opacity = MathHelper.Lerp(0.9f, 0.3f, lifeProgress);

            // === SUBTLE TRAIL — dimmer star dust ===
            if (Main.rand.NextBool(2))
            {
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(3f, 3f);
                Vector2 dustVel = -Projectile.velocity * 0.1f;
                Dust d = Dust.NewDustPerfect(dustPos, DustID.GoldFlame, dustVel, 0, default, 0.55f);
                d.noGravity = true;
                d.fadeIn = 0.6f;
            }

            // Occasional blue accent
            if (Main.rand.NextBool(5))
            {
                Dust b = Dust.NewDustPerfect(Projectile.Center, DustID.BlueTorch,
                    Main.rand.NextVector2Circular(0.5f, 0.5f), 0, default, 0.35f);
                b.noGravity = true;
            }

            // Palette-ramped trail sparkles (subtle — echo is small)
            if (Main.rand.NextBool(6))
                NachtmusikVFXLibrary.SpawnGradientSparkles(Projectile.Center, Projectile.velocity, 1, 0.15f, 14, 4f);

            Lighting.AddLight(Projectile.Center, NachtmusikPalette.StarGold.ToVector3() * 0.25f * Projectile.Opacity);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 180);
            if (target.TryGetGlobalNPC(out CelestialHarmonyNPC harmonyNPC))
                harmonyNPC.AddStack(target, 1);

            // Small cosmic flash
            CustomParticles.GenericFlare(target.Center, NachtmusikPalette.StarGold, 0.3f, 12);
            CustomParticles.GenericFlare(target.Center, NachtmusikPalette.StarWhite, 0.2f, 10);

            NachtmusikVFXLibrary.SpawnStarBurst(target.Center, 3, 0.25f);

            // Small palette-ramped sparkle burst
            NachtmusikVFXLibrary.SpawnGradientSparkleExplosion(target.Center, 4, 3.5f, 0.2f, 16);
        }

        public override void OnKill(int timeLeft)
        {
            // Small sparkle dissipation
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 vel = angle.ToRotationVector2() * 1.5f;
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame, vel, 0, default, 0.4f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.Nachtmusik, ref _vertexStrip);

                // === STAR ECHO VFX: StarHomingTrail shader + NK texture accents ===
                float time = (float)Main.timeForVisualEffects * 0.03f;
                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
                float life = MathHelper.Clamp((float)Projectile.timeLeft / 60f, 0f, 1f);
                float expandScale = 0.04f + (1f - life) * 0.04f;

                if (glow != null && NachtmusikShaderManager.HasStarHomingTrail)
                {
                    Vector2 origin = glow.Size() / 2f;

                    NachtmusikShaderManager.BeginShaderAdditive(sb);

                    // Main echo aura — full StarHomingTrail shader
                    NachtmusikShaderManager.ApplyStarHomingTrail(time);
                    sb.Draw(glow, drawPos, null,
                        (NachtmusikPalette.ConstellationBlue with { A = 0 }) * 0.3f * life,
                        0f, origin, expandScale * 1.2f, SpriteEffects.None, 0f);

                    // Glow pass — tighter golden echo pulse
                    NachtmusikShaderManager.ApplyStarHomingTrailGlow(time);
                    sb.Draw(glow, drawPos, null,
                        (NachtmusikPalette.StarGold with { A = 0 }) * 0.25f * life,
                        0f, origin, expandScale, SpriteEffects.None, 0f);

                    // Inner echo core — tighter, brighter
                    sb.Draw(glow, drawPos, null,
                        NachtmusikPalette.StarWhite with { A = 0 } * 0.15f * life,
                        0f, origin, expandScale * 0.4f, SpriteEffects.None, 0f);

                    NachtmusikShaderManager.RestoreSpriteBatch(sb);

                    // NK Lens Flare accent — rotates with time, fading with life
                    Texture2D flareTex = NachtmusikThemeTextures.NKLensFlare?.Value;
                    if (flareTex != null)
                    {
                        NachtmusikShaderManager.BeginAdditive(sb);
                        sb.Draw(flareTex, drawPos, null,
                            (NachtmusikPalette.StarGold with { A = 0 }) * 0.12f * life,
                            time * 0.8f, flareTex.Size() / 2f, expandScale * 0.5f, SpriteEffects.None, 0f);
                        NachtmusikShaderManager.RestoreSpriteBatch(sb);
                    }
                }
                else if (glow != null)
                {
                    // Fallback without shader — TrueAdditive bloom
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                        SamplerState.LinearClamp, DepthStencilState.None,
                        RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                    Vector2 origin = glow.Size() / 2f;
                    sb.Draw(glow, drawPos, null,
                        (NachtmusikPalette.ConstellationBlue with { A = 0 }) * 0.2f * life,
                        0f, origin, expandScale * 1.1f, SpriteEffects.None, 0f);
                    sb.Draw(glow, drawPos, null,
                        (NachtmusikPalette.StarGold with { A = 0 }) * 0.15f * life,
                        0f, origin, expandScale * 0.6f, SpriteEffects.None, 0f);
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
    }
}
