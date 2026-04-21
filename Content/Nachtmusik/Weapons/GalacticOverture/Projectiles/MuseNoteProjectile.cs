using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Nachtmusik.Debuffs;

namespace MagnumOpus.Content.Nachtmusik.Weapons.GalacticOverture.Projectiles
{
    /// <summary>
    /// Legacy MuseNoteProjectile — kept for type compatibility.
    /// Muse now fires GenericHomingOrbChild directly via note cycling.
    /// Visually: a glowing music note that drifts with gravity, leaving a golden trail.
    /// </summary>
    public class MuseNoteProjectile : ModProjectile
    {
        // Actual music note sprite instead of weapon item sprite
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/CursiveMusicNote";

        private static Asset<Texture2D> _bloomTex;
        private VertexStrip _strip;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            // Gentle gravity arc
            Projectile.velocity.Y += 0.04f;
            // Spin like a falling note
            Projectile.rotation += 0.05f;

            if (Main.rand.NextBool(4))
            {
                Color dustCol = Main.rand.NextBool() ? NachtmusikPalette.RadianceGold : NachtmusikPalette.CosmicPurple;
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.3f, 0.3f),
                    0, dustCol, 0.5f);
                d.noGravity = true;
                d.fadeIn = 0.2f;
            }

            Lighting.AddLight(Projectile.Center, NachtmusikPalette.RadianceGold.ToVector3() * 0.2f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            IncisorOrbRenderer.DrawOrbVisuals(Main.spriteBatch, Projectile, IncisorOrbRenderer.Nachtmusik, ref _strip);

            // Draw the music note sprite on top as an overlay in AlphaBlend (already restored by DrawOrbVisuals)
            _bloomTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/CursiveMusicNote", AssetRequestMode.ImmediateLoad);
            if (_bloomTex?.Value != null)
            {
                Main.spriteBatch.Draw(_bloomTex.Value,
                    Projectile.Center - Main.screenPosition,
                    null,
                    NachtmusikPalette.StarWhite * 0.9f,
                    Projectile.rotation,
                    _bloomTex.Value.Size() / 2f,
                    Projectile.scale * 0.7f, SpriteEffects.None, 0f);
            }

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 600);
            target.GetGlobalNPC<CelestialHarmonyNPC>().AddStack(target, 1);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                Color col = i % 2 == 0 ? NachtmusikPalette.RadianceGold : NachtmusikPalette.StarlitBlue;
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                    Main.rand.NextVector2CircularEdge(2f, 2f), 0, col, 0.45f);
                d.noGravity = true;
            }
        }
    }
}
