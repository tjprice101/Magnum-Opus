using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Content;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Debuffs;

namespace MagnumOpus.Content.Nachtmusik.Weapons.MidnightsCrescendo.Projectiles
{
    /// <summary>
    /// Crescendo Wave — crescent-shaped projectile released at 8+ crescendo stacks.
    /// A crescent arc of compressed starlight and music notes that sweeps forward,
    /// leaving a trail of musical dust in its wake.
    /// ai[0] = stack intensity (0-1), determines visual scale and brightness.
    /// </summary>
    public class CrescendoWaveProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/ImpactEffects/ImpactEllipse";

        private float Intensity => Projectile.ai[0];

        private Asset<Texture2D> _glowOrbTex;
        private Texture2D GlowOrb
        {
            get
            {
                _glowOrbTex ??= ModContent.Request<Texture2D>(
                    "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/GlowOrb",
                    AssetRequestMode.ImmediateLoad);
                return _glowOrbTex.Value;
            }
        }

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 45;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            // Rotate to face velocity
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Slight deceleration
            Projectile.velocity *= 0.98f;

            // Scale wave size with intensity
            float scale = 0.8f + Intensity * 0.6f;
            Projectile.scale = scale;

            // Grow hitbox with intensity
            Projectile.width = (int)(60 * scale);
            Projectile.height = (int)(30 * scale);

            // Per-frame trailing VFX: music notes + starlight dust in wake
            MidnightsCrescendoVFX.WaveTrailVFX(Projectile.Center, Projectile.velocity, Intensity);

            // Dynamic lighting
            float lightIntensity = 0.3f + Intensity * 0.4f;
            Lighting.AddLight(Projectile.Center, NachtmusikPalette.StarlitBlue.ToVector3() * lightIntensity);

            // Fade out near end of life
            if (Projectile.timeLeft < 12)
                Projectile.alpha = (int)(255 * (1f - Projectile.timeLeft / 12f));
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 300);
            MidnightsCrescendoVFX.WaveImpactVFX(target.Center, Intensity);
        }

        public override void OnKill(int timeLeft)
        {
            // Dissipation burst — wave fades into music notes
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * 2f;
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch, vel, 0,
                    NachtmusikPalette.StarlitBlue, 0.5f);
                d.noGravity = true;
            }
            CustomParticles.GenericMusicNotes(Projectile.Center, NachtmusikPalette.StarlitBlue, 3, 12f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;
            float rotation = Projectile.rotation;
            float fadeAlpha = 1f - Projectile.alpha / 255f;
            float scale = Projectile.scale;

            // Time-based shimmer
            float shimmer = 1f + MathF.Sin((float)Main.timeForVisualEffects * 0.12f) * 0.06f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Layer 1: Outer glow — deep blue crescent halo
            Color outerColor = NachtmusikPalette.DeepBlue with { A = 0 };
            sb.Draw(tex, pos, null, outerColor * 0.3f * fadeAlpha * Intensity,
                rotation, origin, scale * shimmer * 1.4f, SpriteEffects.None, 0f);

            // Layer 2: Main crescent body — starlit blue
            Color bodyColor = Color.Lerp(NachtmusikPalette.StarlitBlue, NachtmusikPalette.StarWhite, Intensity * 0.5f) with { A = 0 };
            sb.Draw(tex, pos, null, bodyColor * 0.6f * fadeAlpha,
                rotation, origin, scale * shimmer * 1.0f, SpriteEffects.None, 0f);

            // Layer 3: Bright core — twinkling white center
            Color coreColor = Color.Lerp(NachtmusikPalette.StarWhite, NachtmusikPalette.TwinklingWhite, Intensity) with { A = 0 };
            sb.Draw(tex, pos, null, coreColor * 0.5f * fadeAlpha,
                rotation, origin, scale * shimmer * 0.6f, SpriteEffects.None, 0f);

            // Layer 4: Soft bloom halo using GlowOrb
            if (GlowOrb != null)
            {
                Vector2 glowOrigin = new Vector2(GlowOrb.Width, GlowOrb.Height) * 0.5f;
                Color bloomColor = NachtmusikPalette.StarlitBlue with { A = 0 };
                sb.Draw(GlowOrb, pos, null, bloomColor * 0.2f * fadeAlpha * Intensity,
                    0f, glowOrigin, scale * 0.5f, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
