using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Tools
{
    /// <summary>
    /// Held projectile for Wrath's Drill - uses vanilla aiStyle 20 for drill behavior.
    /// </summary>
    public class WrathsDrillProjectile : ModProjectile
    {
        // Use the drill item texture
        public override string Texture => "MagnumOpus/Content/DiesIrae/Tools/WrathsDrill";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.ownerHitCheck = true;
            Projectile.aiStyle = 20; // Vanilla drill AI - handles jitter, cursor pointing, sound
            Projectile.hide = true;
        }
    }
}

