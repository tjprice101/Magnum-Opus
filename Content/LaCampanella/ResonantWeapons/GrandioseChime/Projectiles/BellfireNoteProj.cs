using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Utilities;
using MagnumOpus.Common.Systems;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Particles;
using MagnumOpus.Content.LaCampanella.Debuffs;
using ReLogic.Content;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Projectiles
{
    /// <summary>
    /// Bellfire barrage note 遯ｶ繝ｻone of 7 burning note projectiles spread on every 3rd shot.
    /// Slower than main beam, deals AoE on impact, leaves burning trail.
    /// </summary>
    public class BellfireNoteProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/CursiveMusicNote";

        private float spinRotation;

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 80;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            spinRotation += 0.15f;
            Projectile.rotation = spinRotation;

            // Slow deceleration
            Projectile.velocity *= 0.985f;

            // Burning trail particles
            if (Main.rand.NextBool(3))
            {
                GrandioseChimeParticleHandler.SpawnParticle(new BurningNoteParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(5, 5),
                    -Projectile.velocity * 0.05f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    Main.rand.Next(15, 30)));
            }

            Lighting.AddLight(Projectile.Center, GrandioseChimeUtils.BarragePalette[1].ToVector3() * 0.4f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);
        }

        public override void OnKill(int timeLeft)
        {
            // Small explosion burst
            for (int i = 0; i < 5; i++)
            {
                GrandioseChimeParticleHandler.SpawnParticle(new BurningNoteParticle(
                    Projectile.Center, Main.rand.NextVector2Circular(3f, 3f),
                    Main.rand.Next(15, 25)));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                var tex = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad)?.Value;
                if (tex == null) return false;
                float pulse = 0.9f + (float)Math.Sin(Main.GameUpdateCount * 0.2f + Projectile.whoAmI) * 0.1f;
                Color noteColor = GrandioseChimeUtils.BarragePalette[Main.rand.Next(3)] * pulse;

                sb.Draw(tex, Projectile.Center - Main.screenPosition, null,
                    noteColor, Projectile.rotation, tex.Size() / 2f, 0.55f, SpriteEffects.None, 0f);

                // Fire glow behind (additive so black background disappears)
                var bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow", AssetRequestMode.ImmediateLoad)?.Value;
                if (bloomTex != null)
                {
                    try { sb.End(); } catch { }
                    try
                    {
                        sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                            Main.DefaultSamplerState, DepthStencilState.None,
                            Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                        
                        // Graduated orb bloom head
                        MagnumVFX.DrawGraduatedOrbHead(sb, Projectile.Center - Main.screenPosition, 
                            LaCampanellaPalette.InfernalOrange, LaCampanellaPalette.FlameYellow, 0.7f);
                    }
                    catch { }
                    finally
                    {
                        try { sb.End(); } catch { }
                        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                            Main.DefaultSamplerState, DepthStencilState.None,
                            Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                    }
                }
            }
            catch
            {
                try
                {
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                        Main.DefaultSamplerState, DepthStencilState.None,
                        Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
                catch { }
            }
            return false;
        }
    }
}