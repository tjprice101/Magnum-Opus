using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.OdeToJoy;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Buffs;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Dusts;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Projectiles
{
    /// <summary>
    /// Thorn Lash — V-pattern projectile from Phase 2.
    /// Embeds in enemies, dealing Rose Thorn Bleed DoT.
    /// VFX: CellularCrack-textured thorn sprite with Petal Pink trailing sparks.
    /// </summary>
    public class ThornLashProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int MaxLifetime = 50;
        private const int FadeInFrames = 5;
        private const int FadeOutFrames = 10;
        private int timer;
        private float seed;
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
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 2;
            Projectile.timeLeft = MaxLifetime;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            if (timer == 0)
                seed = Main.rand.NextFloat(100f);

            timer++;
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Slight deceleration
            if (Projectile.velocity.Length() > 4f)
                Projectile.velocity *= 0.97f;

            // Lighting
            float alpha = GetAlpha();
            Lighting.AddLight(Projectile.Center,
                ThornboundTextures.RadiantAmber.ToVector3() * 0.4f * alpha);

            // Trailing sparks — ThornburstDust fragments shed from the lash
            if (timer % 2 == 0)
            {
                Vector2 vel = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f);
                Color col = Color.Lerp(ThornboundTextures.PetalPink, ThornboundTextures.RadiantAmber,
                    Main.rand.NextFloat());
                Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    ModContent.DustType<ThornburstDust>(), vel, newColor: col,
                    Scale: Main.rand.NextFloat(0.6f, 1.2f));
            }
        }

        private float GetAlpha()
        {
            float fadeIn = MathHelper.Clamp(timer / (float)FadeInFrames, 0f, 1f);
            float fadeOut = Projectile.timeLeft < FadeOutFrames
                ? Projectile.timeLeft / (float)FadeOutFrames
                : 1f;
            return fadeIn * fadeOut;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Rose Thorn Bleed — embeds, stacks
            target.AddBuff(ModContent.BuffType<RoseThornBleedDebuff>(), 240);

            // Build Reckoning Charge for thorn embed
            Player owner = Main.player[Projectile.owner];
            var tbp = owner.GetModPlayer<ThornboundPlayer>();
            tbp.AddThornEmbedCharge();

            // Embed impact burst — ThornburstDust fragments + sparkle
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Color col = Color.Lerp(ThornboundTextures.RoseShadow, ThornboundTextures.RadiantAmber,
                    Main.rand.NextFloat());
                Dust.NewDustPerfect(target.Center, ModContent.DustType<ThornburstDust>(), vel,
                    newColor: col, Scale: Main.rand.NextFloat(1.0f, 1.8f));
            }

            // Sparkle burst on embed
            OdeToJoyVFXLibrary.SpawnGardenSparkleExplosion(target.Center, 4, 3f, 0.15f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.OdeToJoy, ref _vertexStrip);

                // Thorn Lash accent: vine-green directional streak
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    SamplerState.LinearClamp, DepthStencilState.None,
                    RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
                if (glow != null)
                {
                    Vector2 origin = glow.Size() / 2f;
                    float velRot = Projectile.velocity.ToRotation();
                    float alpha = GetAlpha();

                    sb.Draw(glow, drawPos, null,
                        (OdeToJoyPalette.LeafGreen with { A = 0 }) * 0.22f * alpha,
                        velRot, origin, new Vector2(0.1f, 0.022f), SpriteEffects.None, 0f);
                }

                sb.End();
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

        public override void OnKill(int timeLeft)
        {
            // Thorn fragment burst on death
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<ThornburstDust>(), vel,
                    newColor: ThornboundTextures.RoseShadow,
                    Scale: Main.rand.NextFloat(0.7f, 1.4f));
            }
        }
    }
}
