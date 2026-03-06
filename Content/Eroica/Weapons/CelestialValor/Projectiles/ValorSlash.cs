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

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor.Projectiles
{
    /// <summary>
    /// Crimson-gold slash arc projectile - travels outward from the player
    /// with a directional smear trail, bloom gradient, and musical sparks.
    /// 
    /// ARCHITECTURE: Built on Foundation rendering (SMFTextures).
    /// - Streak: Smear texture from SMFTextures.SwordArcSmear, 3-layer (outer/body/core)
    /// - Bloom: Foundation SoftGlow + StarFlare at center
    /// - Sparks: Eroica directional spark dust via EroicaVFXLibrary
    /// </summary>
    public class ValorSlash : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int MaxLife = 25;

        public override void SetDefaults()
        {
            Projectile.width = 160;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = MaxLife;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.noEnchantmentVisuals = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity *= 0.97f;

            float progress = 1f - (float)Projectile.timeLeft / MaxLife;
            Projectile.Opacity = MathF.Max(0f, 1f - progress * 1.2f);

            // Spark trail
            if (Projectile.timeLeft % 3 == 0)
                EroicaVFXLibrary.SpawnDirectionalSparks(
                    Projectile.Center,
                    Projectile.velocity.SafeNormalize(Vector2.UnitX), 2, 3f);

            EroicaVFXLibrary.AddPaletteLighting(Projectile.Center, 0.35f, 0.5f * Projectile.Opacity);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 180);
            EroicaVFXLibrary.MeleeImpact(target.Center, 1);
        }

        public override void OnKill(int timeLeft)
        {
            EroicaVFXLibrary.SpawnValorSparkles(Projectile.Center, 4, 15f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            float progress = 1f - (float)Projectile.timeLeft / MaxLife;
            float fade = Projectile.Opacity;
            if (fade < 0.02f) return false;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // „Ÿ„Ÿ Foundation rendering: SMFTextures „Ÿ„Ÿ
            Texture2D smearTex = SMFTextures.SwordArcSmear.Value;
            Texture2D softGlow = SMFTextures.SoftGlow.Value;
            Texture2D starFlare = SMFTextures.StarFlare.Value;

            Vector2 smearOrigin = smearTex.Size() / 2f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            // Layer 1: Outer scarlet smear
            sb.Draw(smearTex, drawPos, null,
                (EroicaPalette.Scarlet with { A = 0 }) * (fade * 0.5f),
                Projectile.rotation, smearOrigin,
                new Vector2(1.8f, 0.4f), SpriteEffects.None, 0f);

            // Layer 2: Inner gold body
            sb.Draw(smearTex, drawPos, null,
                (EroicaPalette.Gold with { A = 0 }) * (fade * 0.6f),
                Projectile.rotation, smearOrigin,
                new Vector2(1.4f, 0.2f), SpriteEffects.None, 0f);

            // Layer 3: Hot white core
            sb.Draw(smearTex, drawPos, null,
                (EroicaPalette.HotCore with { A = 0 }) * (fade * 0.5f),
                Projectile.rotation, smearOrigin,
                new Vector2(1.0f, 0.1f), SpriteEffects.None, 0f);

            // Layer 4: Center bloom (Foundation SoftGlow)
            sb.Draw(softGlow, drawPos, null,
                (EroicaPalette.Scarlet with { A = 0 }) * (fade * 0.2f), 0f,
                softGlow.Size() / 2f, 0.3f, SpriteEffects.None, 0f);

            sb.Draw(softGlow, drawPos, null,
                (EroicaPalette.Gold with { A = 0 }) * (fade * 0.4f), 0f,
                softGlow.Size() / 2f, 0.15f, SpriteEffects.None, 0f);

            // Layer 5: Star flare accent
            sb.Draw(starFlare, drawPos, null,
                (EroicaPalette.Gold with { A = 0 }) * (fade * 0.35f),
                Projectile.rotation, starFlare.Size() / 2f,
                0.15f, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
