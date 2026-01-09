using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Content.Eroica.Projectiles;
using MagnumOpus.Common;

namespace MagnumOpus.Content.Eroica.ResonantWeapons
{
    /// <summary>
    /// Piercing Light of the Sakura - A fast-firing rifle that channels the essence of valor.
    /// Every 10th shot fires a special sakura projectile that calls down black, gold, and red lightning.
    /// Rainbow rarity, drops from Eroica, God of Valor.
    /// </summary>
    public class PiercingLightOfTheSakura : ModItem
    {
        private int shotCounter = 0;
        
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 155; // Balanced: ~1163 DPS (155 × 60/8)
            Item.DamageType = DamageClass.Ranged;
            Item.width = 64;
            Item.height = 24;
            Item.useTime = 8; // Very fast firing
            Item.useAnimation = 8;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 2.5f;
            Item.value = Item.sellPrice(gold: 40);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.Bullet;
            Item.shootSpeed = 18f;
            Item.useAmmo = AmmoID.Bullet;
            Item.noMelee = true;
            Item.maxStack = 1;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            shotCounter++;
            
            // Muzzle flash particles
            for (int i = 0; i < 3; i++)
            {
                int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                Dust flash = Dust.NewDustDirect(position, 1, 1, dustType, velocity.X * 0.2f, velocity.Y * 0.2f, 100, default, 1.2f);
                flash.noGravity = true;
            }
            
            // Every 10th shot, fire the special sakura lightning projectile
            if (shotCounter >= 10)
            {
                shotCounter = 0;
                
                // Fire the special projectile instead of normal bullet
                Projectile.NewProjectile(source, position, velocity * 1.2f, 
                    ModContent.ProjectileType<PiercingLightOfTheSakuraProjectile>(), 
                    (int)(damage * 2.5f), knockback * 2f, player.whoAmI);
                
                // Special firing effect
                SoundEngine.PlaySound(SoundID.Item125 with { Pitch = 0.3f, Volume = 0.8f }, position);
                
                for (int i = 0; i < 12; i++)
                {
                    int dustType = i % 3 == 0 ? DustID.Shadowflame : (i % 3 == 1 ? DustID.GoldFlame : DustID.CrimsonTorch);
                    Dust special = Dust.NewDustDirect(position, 1, 1, dustType, 0f, 0f, 100, default, 1.8f);
                    special.noGravity = true;
                    special.velocity = velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.5f) * Main.rand.NextFloat(3f, 8f);
                }
                
                return false; // Don't fire normal bullet
            }
            
            // Normal bullet with tracer effect - spawn black bullet
            int proj = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            // Make bullet appear darker with black trail particles
            if (proj >= 0 && proj < Main.maxProjectiles)
            {
                Main.projectile[proj].alpha = 200; // Darken the bullet
            }
            
            // Add black tracer particles to normal shots
            for (int i = 0; i < 2; i++)
            {
                Dust tracer = Dust.NewDustDirect(position, 1, 1, DustID.Smoke, velocity.X * 0.3f, velocity.Y * 0.3f, 200, Color.Black, 0.7f);
                tracer.noGravity = true;
                tracer.color = Color.Black;
            }
            
            return false;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-8f, 0f);
        }

        public override void HoldItem(Player player)
        {
            // Dark red and gold particles while holding
            if (Main.rand.NextBool(3))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.Torch;
                Dust particle = Dust.NewDustDirect(player.Center + offset, 1, 1, dustType, 0f, -1f, 150, default, 0.9f);
                particle.noGravity = true;
                particle.velocity *= 0.3f;
                if (dustType == DustID.Torch)
                    particle.color = new Color(139, 0, 0); // Dark red
            }
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            // Make normal bullets black by using a custom projectile color
            // This is handled in Shoot method by setting bullet alpha/color
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "EroicaWeapon", "The light of fallen heroes guides each shot")
            {
                OverrideColor = new Color(255, 200, 100)
            });
        }

        // No recipe - drops from boss
    }
}
