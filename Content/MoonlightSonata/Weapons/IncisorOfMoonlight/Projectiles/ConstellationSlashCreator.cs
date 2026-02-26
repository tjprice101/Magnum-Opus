using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Projectiles
{
    /// <summary>
    /// Timer/spawner projectile that periodically creates ConstellationSlash damage hitboxes
    /// at the target NPC's position. Spawned on dash-hit or beam-hit.
    /// ai[0] = target NPC whoAmI, ai[1] = spawn interval ticks.
    /// </summary>
    public class ConstellationSlashCreator : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/CircularMask";

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 45;
            Projectile.MaxUpdates = 2;
            Projectile.noEnchantmentVisuals = true;
        }

        public override bool? CanDamage() => false;

        public override void AI()
        {
            int targetIdx = (int)Projectile.ai[0];
            int spawnInterval = Projectile.ai[1] > 0 ? (int)Projectile.ai[1] : 20;

            if (!Main.npc.IndexInRange(targetIdx) || !Main.npc[targetIdx].active)
                return;

            NPC target = Main.npc[targetIdx];

            if (Projectile.timeLeft % spawnInterval == 0 && Main.myPlayer == Projectile.owner)
            {
                Vector2 slashDir = (Main.rand.NextVector2Unit()).SafeNormalize(Vector2.UnitX);
                Vector2 spawnPos = target.Center + Main.rand.NextVector2Circular(target.width * 0.3f, target.height * 0.3f);
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), spawnPos, slashDir,
                    ModContent.ProjectileType<ConstellationSlash>(), Projectile.damage, 0f, Projectile.owner);
            }
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }
}
