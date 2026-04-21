using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.OdeToJoy;
using MagnumOpus.Content.OdeToJoy.Systems;

namespace MagnumOpus.Content.OdeToJoy.Weapons.RoseThornChainsaw.Projectiles
{
    /// <summary>
    /// Rapid Streak Orb — Rose Thorn Chainsaw projectile.
    /// Fires in rapid bursts with short timeLeft (40 frames). No homing — pure directional.
    /// Right-click empowerment: pierce 2, wider spread, accelerates.
    /// ai[0] = IsEmpowered (0 or 1)
    /// </summary>
    public class RapidStreakOrbProjectile : ModProjectile
    {
        private bool IsEmpowered => Projectile.ai[0] > 0;
        private float _pulseTimer;
        private VertexStrip _strip;

        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = IsEmpowered ? 2 : 1;
            Projectile.timeLeft = 40;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            _pulseTimer += 0.15f;
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Empowered acceleration
            if (IsEmpowered && Projectile.velocity.Length() < 24f)
            {
                Projectile.velocity *= 1.01f;
            }

            // Trail dust - rose pink and golden
            if (Main.rand.NextBool(2))
            {
                Color dustCol = IsEmpowered
                    ? Color.Lerp(OdeToJoyPalette.RosePink, OdeToJoyPalette.GoldenPollen, Main.rand.NextFloat())
                    : OdeToJoyPalette.RosePink;
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(3f, 3f),
                    DustID.PinkTorch, -Projectile.velocity * 0.2f, 100, dustCol, 0.6f);
                d.noGravity = true;
            }

            // Petal sparkles
            if (Main.rand.NextBool(4))
            {
                Dust petal = Dust.NewDustPerfect(Projectile.Center, DustID.PinkFairy,
                    Main.rand.NextVector2Circular(1f, 1f), 80, OdeToJoyPalette.RosePink, 0.5f);
                petal.noGravity = true;
            }

            // Lighting
            float intensity = IsEmpowered ? 0.35f : 0.25f;
            Color lightCol = IsEmpowered ? OdeToJoyPalette.GoldenPollen : OdeToJoyPalette.RosePink;
            Lighting.AddLight(Projectile.Center, lightCol.ToVector3() * intensity);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Impact dust
            for (int i = 0; i < 5; i++)
            {
                Color col = Main.rand.NextBool() ? OdeToJoyPalette.RosePink : OdeToJoyPalette.PetalPink;
                Dust d = Dust.NewDustPerfect(target.Center, DustID.PinkTorch,
                    Main.rand.NextVector2Circular(4f, 4f), 100, col, 0.8f);
                d.noGravity = true;
            }

            // Empowered impact - golden burst
            if (IsEmpowered)
            {
                for (int i = 0; i < 4; i++)
                {
                    Dust spark = Dust.NewDustPerfect(target.Center, DustID.GoldFlame,
                        Main.rand.NextVector2Circular(2f, 2f), 0, OdeToJoyPalette.GoldenPollen, 0.7f);
                    spark.noGravity = true;
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 3; i++)
            {
                Color col = IsEmpowered
                    ? Color.Lerp(OdeToJoyPalette.RosePink, OdeToJoyPalette.GoldenPollen, Main.rand.NextFloat())
                    : OdeToJoyPalette.RosePink;
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.PinkTorch,
                    Main.rand.NextVector2Circular(2f, 2f), 100, col, 0.5f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            IncisorOrbRenderer.DrawOrbVisuals(Main.spriteBatch, Projectile, IncisorOrbRenderer.OdeToJoy, ref _strip);

            // Empowered indicator: golden ring + white-hot center dot when IsEmpowered
            if (IsEmpowered)
            {
                SpriteBatch sb = Main.spriteBatch;
                try
                {
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                        Main.DefaultSamplerState, DepthStencilState.None,
                        RasterizerState.CullCounterClockwise, null,
                        Main.GameViewMatrix.TransformationMatrix);

                    Texture2D glow = ModContent.Request<Texture2D>(
                        "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow",
                        ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                    Vector2 origin = glow.Size() / 2f;
                    Vector2 drawPos = Projectile.Center - Main.screenPosition;
                    float pulse = 0.85f + 0.15f * MathF.Sin(_pulseTimer * 3f);

                    sb.Draw(glow, drawPos, null, (OdeToJoyPalette.GoldenPollen with { A = 0 }) * 0.45f * pulse,
                        _pulseTimer, origin, 0.24f * Projectile.scale, SpriteEffects.None, 0f);
                    sb.Draw(glow, drawPos, null, (OdeToJoyPalette.WhiteBloom with { A = 0 }) * 0.65f,
                        0f, origin, 0.06f * Projectile.scale, SpriteEffects.None, 0f);
                }
                catch { }
                finally
                {
                    try { sb.End(); } catch { }
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                        DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }

            return false;
        }
    }
}
