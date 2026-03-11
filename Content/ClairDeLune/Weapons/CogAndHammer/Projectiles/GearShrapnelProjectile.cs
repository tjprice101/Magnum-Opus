using MagnumOpus.Common;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.ClairDeLune;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.CogAndHammer.Projectiles
{
    /// <summary>
    /// Gear Shrapnel — brass gear fragment ejected from bomb detonations.
    /// Deals 30% bomb damage, short lifetime, spinning.
    /// 2 render passes: (1) GearSwing GearSwingTrail for spinning gear body,
    /// (2) Multi-scale bloom with gear teeth + core.
    /// Kept lightweight — many spawned from simultaneous detonations.
    /// </summary>
    public class GearShrapnelProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";
        private VertexStrip _vertexStrip;

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
            Projectile.timeLeft = 40;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            Projectile.rotation += 0.4f;
            Projectile.velocity *= 0.96f;
            Projectile.velocity.Y += 0.2f;

            // Spark trail
            if (Main.rand.NextBool(2))
            {
                Color sparkCol = Main.rand.NextBool(5)
                    ? ClairDeLunePalette.NightMist
                    : ClairDeLunePalette.MoonbeamGold;
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame,
                    -Projectile.velocity * 0.05f, 0, sparkCol, 0.4f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.MoonbeamGold.ToVector3() * 0.15f);
        }

        public override void OnKill(int timeLeft)
        {
            var flash = new GenericGlowParticle(Projectile.Center, Vector2.Zero,
                ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.3f, 0.08f, 5, true);
            MagnumParticleHandler.SpawnParticle(flash);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.ClairDeLune, ref _vertexStrip);

                // Gear shrapnel: brass gear spark
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    SamplerState.LinearClamp, DepthStencilState.None,
                    RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
                if (glow != null)
                {
                    Vector2 origin = glow.Size() / 2f;

                    sb.Draw(glow, drawPos, null,
                        (ClairDeLunePalette.ClockworkBrass with { A = 0 }) * 0.18f,
                        Projectile.rotation, origin, 0.03f, SpriteEffects.None, 0f);
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
    }
}
