using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using MagnumOpus.Content.FoundationWeapons.RibbonFoundation;
using ReLogic.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor
{
    /// <summary>
    /// Heroic flame projectile — thrown melee projectile with flame trail,
    /// afterimage chain, and crimson-gold bloom overlay.
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

            // ── Layer 1: Trail afterimage chain ──
            for (int i = TrailLength - 1; i > 0; i--)
            {
                float fade = 1f - (float)i / TrailLength;
                fade *= fade;
                Vector2 drawPos = trailPositions[i] - Main.screenPosition;

                Texture2D bloom = MagnumTextureRegistry.GetBloom();
                if (bloom == null) continue;
                Vector2 origin = bloom.Size() * 0.5f;

                Color trailColor = Color.Lerp(EroicaPalette.Gold, EroicaPalette.DeepScarlet, (float)i / TrailLength) with { A = 0 };
                sb.Draw(bloom, drawPos, null, trailColor * (fade * 0.35f), 0f, origin, 0.25f * fade + 0.1f, SpriteEffects.None, 0f);
            }

            // ── Layer 1b: Pure Bloom Ribbon (RibbonFoundation Mode 1) ──
            DrawPureBloomRibbon(sb);

            // ── Layer 2: Core bloom at projectile center ──
            EroicaVFXLibrary.DrawEroicaBloomStack(sb, Projectile.Center,
                EroicaPalette.Scarlet, EroicaPalette.Gold, 0.3f, 0.85f);

            // ── Layer 3: Rotating flare ──
            Texture2D flare = MagnumTextureRegistry.GetFlare();
            if (flare != null)
            {
                Vector2 flarePos = Projectile.Center - Main.screenPosition;
                Vector2 flareOrigin = flare.Size() * 0.5f;
                float rot = (float)Main.GameUpdateCount * 0.08f;
                Color flareCol = EroicaPalette.Gold with { A = 0 };
                sb.Draw(flare, flarePos, null, flareCol * 0.4f, rot, flareOrigin, 0.25f, SpriteEffects.None, 0f);
            }

            // Eroica theme accent
            EroicaVFXLibrary.BeginEroicaAdditive(sb);
            EroicaVFXLibrary.DrawThemeSakuraAccent(sb, Projectile.Center, 1f, 0.5f);
            EroicaVFXLibrary.EndEroicaAdditive(sb);

            return false;
        }

        /// <summary>
        /// RibbonFoundation Mode 1 (Pure Bloom) — velocity-stretched bloom sprites along trail.
        /// Gold → crimson gradient, 3-layer per point (outer haze, body, hot core).
        /// </summary>
        private void DrawPureBloomRibbon(SpriteBatch sb)
        {
            int validCount = 0;
            for (int i = 0; i < TrailLength; i++)
            {
                if (trailPositions[i] != Vector2.Zero) validCount++;
                else break;
            }
            if (validCount < 3) return;

            Texture2D bloomTex = RBFTextures.SoftGlowBright.Value;
            Texture2D coreTex = RBFTextures.PointBloom.Value;
            if (bloomTex == null || coreTex == null) return;

            Vector2 bloomOrigin = bloomTex.Size() / 2f;
            Vector2 coreOrigin = coreTex.Size() / 2f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
            try
            {
            for (int i = 0; i < validCount; i++)
            {
                float progress = (float)i / validCount;
                float headStrength = 1f - progress;
                float fade = headStrength * headStrength;
                if (fade < 0.01f) continue;

                float width = MathHelper.Lerp(2f, 10f, headStrength);
                float scale = width / bloomTex.Width;

                Vector2 vel = i + 1 < validCount ? trailPositions[i] - trailPositions[i + 1] : Projectile.velocity;
                float rot = vel.ToRotation() + MathHelper.PiOver2;

                Vector2 pos = trailPositions[i] - Main.screenPosition;
                float stretchX = scale;
                float stretchY = scale * 2.2f;

                Color outerColor = EroicaPalette.Scarlet with { A = 0 } * (fade * 0.3f);
                sb.Draw(bloomTex, pos, null, outerColor, rot, bloomOrigin,
                    new Vector2(stretchX * 1.6f, stretchY * 1.3f), SpriteEffects.None, 0f);

                Color bodyColor = Color.Lerp(Color.White, EroicaPalette.Gold, progress) with { A = 0 } * (fade * 0.45f);
                sb.Draw(bloomTex, pos, null, bodyColor, rot, bloomOrigin,
                    new Vector2(stretchX, stretchY), SpriteEffects.None, 0f);

                Color coreColor = Color.White with { A = 0 } * (fade * 0.35f * headStrength);
                sb.Draw(coreTex, pos, null, coreColor, rot, coreOrigin,
                    new Vector2(stretchX * 0.4f, stretchY * 0.6f), SpriteEffects.None, 0f);
            }
            }
            finally
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);
            }
        }
    }
}