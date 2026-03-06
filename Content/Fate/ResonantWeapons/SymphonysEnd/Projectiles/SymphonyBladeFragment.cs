using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.Debuffs;

namespace MagnumOpus.Content.Fate.ResonantWeapons.SymphonysEnd
{
    /// <summary>
    /// Blade fragment — spawned when a <see cref="SymphonySpiralBlade"/> shatters.
    /// Smaller, faster decay, scatters outward with slight gravity.
    /// </summary>
    public class SymphonyBladeFragment : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/SymphonysEnd";

        private const int MaxLifetime = 45; // 0.75 seconds

        // ─── Setup ────────────────────────────────────────────────

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width  = 16;
            Projectile.height = 16;
            Projectile.friendly    = true;
            Projectile.DamageType  = DamageClass.Magic;
            Projectile.penetrate   = 1;
            Projectile.timeLeft    = MaxLifetime;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown  = -1; // one hit only
        }

        // ─── AI: Scatter + Gravity ────────────────────────────────

        public override void AI()
        {
            float age = 1f - (float)Projectile.timeLeft / MaxLifetime;

            // Decelerate + subtle gravity
            Projectile.velocity *= 0.96f;
            Projectile.velocity.Y += 0.1f;

            // Tumble rotation
            Projectile.rotation += Projectile.velocity.X * 0.05f;

            // ─── VFX ──────────────────────────────────────────────
            if (!Main.dedServ)
            {
                if (Main.rand.NextBool(2))
                {
                    Color sparkCol = SymphonyUtils.GetSymphonyGradient(Main.rand.NextFloat());
                    SymphonyParticleHandler.Spawn(SymphonyParticleFactory.Spark(
                        Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                        -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 3f),
                        sparkCol * 0.8f,
                        0.05f, 10));
                }

                Color light = Color.Lerp(SymphonyUtils.DiscordRed, SymphonyUtils.SymphonyPink, age);
                Lighting.AddLight(Projectile.Center, light.ToVector3() * 0.3f);
            }
        }

        // ─── Combat ───────────────────────────────────────────────

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 120);

            if (!Main.dedServ)
            {
                SymphonyParticleHandler.SpawnBurst(Projectile.Center, 6, 3f, 0.08f,
                    SymphonyUtils.DiscordRed, SymphonyParticleType.Shard, 12);
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (!Main.dedServ)
            {
                SymphonyParticleHandler.SpawnBurst(Projectile.Center, 4, 2f, 0.1f,
                    SymphonyUtils.SymphonyPink, SymphonyParticleType.Glow, 15);
            }
        }

        // ─── Custom Drawing ───────────────────────────────────────

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {

            // End default batch
            sb.End();

            // Draw fragment trail
            Effect shader = SymphonyShaderLoader.SymphonyFragmentTrail;
            if (shader != null)
            {
                try { shader.CurrentTechnique = shader.Techniques["FragmentTrail"]; }
                catch { }
            }

            SymphonyTrailRenderer.DrawTrail(
                Projectile.oldPos,
                Projectile.Size * 0.5f,
                SymphonyTrailSettings.Fragment,
                shader);

            // Additive blade sprite
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);

            Texture2D tex   = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin  = tex.Size() * 0.5f;
            float scale     = 0.3f;

            sb.Draw(tex, drawPos, null,
                SymphonyUtils.Additive(SymphonyUtils.DiscordRed, 0.5f),
                Projectile.rotation, origin, scale * 1.2f, SpriteEffects.None, 0f);

            sb.Draw(tex, drawPos, null,
                SymphonyUtils.Additive(SymphonyUtils.FinalWhite, 0.6f),
                Projectile.rotation, origin, scale, SpriteEffects.None, 0f);

            sb.End();

            }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null,
                    Main.GameViewMatrix.TransformationMatrix);
            }
            return false;
        }
    }
}
