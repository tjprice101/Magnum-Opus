using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Nachtmusik.Debuffs;

namespace MagnumOpus.Content.Nachtmusik.Weapons.StarweaversGrimoire.Projectiles
{
    /// <summary>
    /// Legacy orb projectile for Starweaver's Grimoire — kept for type compatibility.
    /// Item now spawns StarweaverNodeProjectile directly.
    /// Visually: a glowing gold-purple constellation orb.
    /// </summary>
    public class StarweaverOrbProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private static Asset<Texture2D> _bloomTex;
        private VertexStrip _strip;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation += 0.05f;

            if (Main.rand.NextBool(3))
            {
                Color dustCol = Main.rand.NextBool() ? NachtmusikPalette.RadianceGold : NachtmusikPalette.CosmicPurple;
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                    -Projectile.velocity * 0.1f, 0, dustCol, 0.7f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.2f, 0.15f, 0.3f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            IncisorOrbRenderer.DrawOrbVisuals(Main.spriteBatch, Projectile, IncisorOrbRenderer.Nachtmusik, ref _strip);
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
                Color col = i % 2 == 0 ? NachtmusikPalette.RadianceGold : NachtmusikPalette.CosmicPurple;
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                    Main.rand.NextVector2CircularEdge(3f, 3f), 0, col, 0.5f);
                d.noGravity = true;
            }
        }
    }
}
