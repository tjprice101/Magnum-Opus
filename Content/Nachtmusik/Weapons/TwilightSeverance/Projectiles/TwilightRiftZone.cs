using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.Nachtmusik.Weapons.TwilightSeverance.Projectiles
{
    /// <summary>
    /// Twilight Rift Zone — stationary amplifier placed by right-click.
    /// Nachtmusik orbs passing through gain +40% damage and +50% speed.
    /// Lasts 1.5 seconds, 80px radius.
    /// </summary>
    public class TwilightRiftZone : ModProjectile
    {
        private const float Radius = 80f;
        private const int Duration = 90; // 1.5 seconds
        private int _timer;
        private float _seed;

        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";

        public override void SetDefaults()
        {
            Projectile.width = (int)(Radius * 2);
            Projectile.height = (int)(Radius * 2);
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = Duration;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            if (_timer == 0)
                _seed = Main.rand.NextFloat(100f);

            _timer++;
            Projectile.velocity = Vector2.Zero;

            float alpha = GetAlpha();

            // Amplify Nachtmusik orbs passing through
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (!proj.active || proj.owner != Projectile.owner) continue;
                if (proj.type != ModContent.ProjectileType<GenericHomingOrbChild>()) continue;
                if ((int)proj.localAI[0] != GenericHomingOrbChild.THEME_NACHTMUSIK) continue;

                float dist = Vector2.Distance(Projectile.Center, proj.Center);
                if (dist < Radius)
                {
                    // Use ai[2] to prevent double-boosting (0 = not boosted)
                    // GenericHomingOrbChild doesn't use ai[2], so it's safe
                    if (proj.ai[2] == 0f)
                    {
                        proj.velocity *= 1.5f; // +50% speed
                        proj.damage = (int)(proj.damage * 1.4f); // +40% damage
                        proj.ai[2] = 1f; // Mark as amplified
                    }
                }
            }

            // Ring dust at edges — Nachtmusik purple and gold
            if (Main.rand.NextBool(2))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 dustPos = Projectile.Center + angle.ToRotationVector2() * Radius * Main.rand.NextFloat(0.85f, 1.05f);
                Color dustColor = Main.rand.NextBool() ? NachtmusikPalette.CosmicPurple : NachtmusikPalette.RadianceGold;
                Dust d = Dust.NewDustPerfect(dustPos, DustID.WhiteTorch,
                    angle.ToRotationVector2() * 0.3f, 0, dustColor, 0.5f * alpha);
                d.noGravity = true;
            }

            // Pulsing core light
            float pulse = 0.7f + 0.3f * MathF.Sin((float)Main.timeForVisualEffects * 0.06f + _seed);
            Lighting.AddLight(Projectile.Center, new Vector3(0.3f, 0.2f, 0.5f) * alpha * pulse);
        }

        private float GetAlpha()
        {
            float fadeIn = MathHelper.Clamp(_timer / 10f, 0f, 1f);
            float fadeOut = Projectile.timeLeft < 20 ? Projectile.timeLeft / 20f : 1f;
            return fadeIn * fadeOut;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                float alpha = GetAlpha();
                float pulse = 0.85f + 0.15f * MathF.Sin((float)Main.timeForVisualEffects * 0.05f + _seed);
                float rotation = (float)Main.timeForVisualEffects * 0.02f;

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);

                Texture2D glow = ModContent.Request<Texture2D>(
                    "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                Vector2 origin = glow.Size() / 2f;
                float glowScale = Radius / (glow.Width * 0.5f) * 1.3f * pulse;

                // Outer purple glow
                sb.Draw(glow, drawPos, null, NachtmusikPalette.CosmicPurple * (0.15f * alpha), rotation, origin, glowScale, SpriteEffects.None, 0f);

                // Mid gold glow
                sb.Draw(glow, drawPos, null, NachtmusikPalette.RadianceGold * (0.12f * alpha), -rotation * 0.5f, origin, glowScale * 0.7f, SpriteEffects.None, 0f);

                // Inner white-blue core
                sb.Draw(glow, drawPos, null, NachtmusikPalette.StarWhite * (0.25f * alpha), 0f, origin, glowScale * 0.35f, SpriteEffects.None, 0f);
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
            if (Main.dedServ) return;
            // Death burst — purple and gold particles
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Color col = i % 2 == 0 ? NachtmusikPalette.CosmicPurple : NachtmusikPalette.RadianceGold;
                Dust d = Dust.NewDustPerfect(Projectile.Center + angle.ToRotationVector2() * Radius * 0.5f,
                    DustID.WhiteTorch, angle.ToRotationVector2() * 2f, 0, col, 0.6f);
                d.noGravity = true;
            }
        }
    }
}
