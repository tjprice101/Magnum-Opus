using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Nachtmusik.Debuffs;

namespace MagnumOpus.Content.Nachtmusik.Weapons.NebulasWhisper.Projectiles
{
    /// <summary>
    /// Fallback projectile for Nebula's Whisper (item now spawns orbs directly,
    /// but this remains for any edge cases or ammo-consumed projectiles).
    /// </summary>
    public class NebulaWhisperShot : ModProjectile
    {
        // Glowing nebula orb sprite — never weapon item texture
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            if (Main.rand.NextBool(3))
            {
                Color dustCol = Main.rand.NextBool() ? NachtmusikPalette.CosmicPurple : NachtmusikPalette.StarlitBlue;
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                    -Projectile.velocity * 0.1f, 0, dustCol, 0.7f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.15f, 0.1f, 0.3f);
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
                Color col = i % 2 == 0 ? NachtmusikPalette.CosmicPurple : NachtmusikPalette.RadianceGold;
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                    Main.rand.NextVector2CircularEdge(3f, 3f), 0, col, 0.5f);
                d.noGravity = true;
            }
        }
    }
}
