using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using MagnumOpus.Content.Eroica;
using MagnumOpus.Content.FoundationWeapons.SwordSmearFoundation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor
{
    /// <summary>
    /// Heroic flame projectile - thrown melee projectile with flame trail,
    /// afterimage chain, and crimson-gold bloom overlay.
    /// 
    /// ARCHITECTURE: Built on Foundation rendering (SMFTextures).
    /// - Trail: Afterimage chain using position buffer + SoftGlow/PointBloom bloom per point
    /// - Body: Multi-scale bloom stack (outer haze, main glow, hot core)
    /// - Accent: StarFlare rotating flare at head
    /// </summary>
    public class CelestialValorProjectile : ModProjectile
    {
        private const int TrailLength = 12;
        private Vector2[] trailPositions = new Vector2[TrailLength];
        private bool trailInitialized = false;

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Trail tracking
            if (!trailInitialized)
            {
                trailInitialized = true;
                for (int i = 0; i < TrailLength; i++)
                    trailPositions[i] = Projectile.Center;
            }
            for (int i = TrailLength - 1; i > 0; i--)
                trailPositions[i] = trailPositions[i - 1];
            trailPositions[0] = Projectile.Center;

            // Flame trail dust
            EroicaVFXLibrary.SpawnFlameTrailDust(Projectile.Center, Projectile.velocity);

            // Gravity
            Projectile.velocity.Y += 0.05f;

            // Lighting
            EroicaVFXLibrary.AddPaletteLighting(Projectile.Center, 0.4f, 0.7f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 180);
            EroicaVFXLibrary.MeleeImpact(target.Center, 1);
            EroicaVFXLibrary.SpawnDirectionalSparks(target.Center,
                (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX), 4, 5f);
        }

        public override void OnKill(int timeLeft)
        {
            EroicaVFXLibrary.SpawnRadialDustBurst(Projectile.Center, 10, 5f);
            EroicaVFXLibrary.SpawnValorSparkles(Projectile.Center, 6, 25f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            // Foundation textures
            Texture2D softGlow = SMFTextures.SoftGlow.Value;
            Texture2D pointBloom = SMFTextures.PointBloom.Value;
            Texture2D starFlare = SMFTextures.StarFlare.Value;

            Vector2 glowOrigin = softGlow.Size() / 2f;
            Vector2 pointOrigin = pointBloom.Size() / 2f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            // „Ÿ„Ÿ LAYER 1: Trail afterimage bloom chain (Foundation pattern) „Ÿ„Ÿ
            for (int i = TrailLength - 1; i > 0; i--)
            {
                float progress = (float)i / TrailLength;
                float headStrength = 1f - progress;
                float fade = headStrength * headStrength;
                if (fade < 0.02f) continue;

                Vector2 drawPos = trailPositions[i] - Main.screenPosition;

                // Velocity-aligned stretch
                Vector2 vel = i + 1 < TrailLength ?
                    trailPositions[i] - trailPositions[i + 1] : Projectile.velocity;
                float rot = vel.ToRotation() + MathHelper.PiOver2;
                float scale = MathHelper.Lerp(2f, 10f, headStrength) / softGlow.Width;
                float stretchX = scale;
                float stretchY = scale * 2.2f;

                // Outer scarlet haze
                Color outerCol = (EroicaPalette.Scarlet with { A = 0 }) * (fade * 0.3f);
                sb.Draw(softGlow, drawPos, null, outerCol, rot, glowOrigin,
                    new Vector2(stretchX * 1.6f, stretchY * 1.3f), SpriteEffects.None, 0f);

                // Body gold glow
                Color bodyCol = Color.Lerp(Color.White, EroicaPalette.Gold, progress) with { A = 0 };
                sb.Draw(softGlow, drawPos, null, bodyCol * (fade * 0.45f), rot, glowOrigin,
                    new Vector2(stretchX, stretchY), SpriteEffects.None, 0f);

                // Hot core
                Color coreCol = (Color.White with { A = 0 }) * (fade * 0.35f * headStrength);
                sb.Draw(pointBloom, drawPos, null, coreCol, rot, pointOrigin,
                    new Vector2(stretchX * 0.4f, stretchY * 0.6f), SpriteEffects.None, 0f);
            }

            // „Ÿ„Ÿ LAYER 2: Head bloom stack „Ÿ„Ÿ
            Vector2 headPos = Projectile.Center - Main.screenPosition;

            // Wide ambient
            sb.Draw(softGlow, headPos, null,
                (EroicaPalette.Scarlet with { A = 0 }) * 0.25f, 0f,
                glowOrigin, 0.35f, SpriteEffects.None, 0f);

            // Main body
            sb.Draw(softGlow, headPos, null,
                (EroicaPalette.Gold with { A = 0 }) * 0.5f, 0f,
                glowOrigin, 0.18f, SpriteEffects.None, 0f);

            // Hot core
            sb.Draw(pointBloom, headPos, null,
                (EroicaPalette.HotCore with { A = 0 }) * 0.4f, 0f,
                pointOrigin, 0.08f, SpriteEffects.None, 0f);

            // „Ÿ„Ÿ LAYER 3: Rotating flare „Ÿ„Ÿ
            float flareRot = (float)Main.GameUpdateCount * 0.08f;
            sb.Draw(starFlare, headPos, null,
                (EroicaPalette.Gold with { A = 0 }) * 0.4f, flareRot,
                starFlare.Size() / 2f, 0.18f, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
