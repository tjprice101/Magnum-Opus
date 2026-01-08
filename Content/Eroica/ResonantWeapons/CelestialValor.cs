using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
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
    /// Celestial Valor - A mighty greatsword infused with heroic spirit.
    /// Swing 1: fires 1 projectile, Swing 2: fires 2, Swing 3: fires 3, then repeats.
    /// Rainbow rarity, drops from Eroica, God of Valor.
    /// Uses 6x6 sprite sheet for swing animation (horizontal sprite).
    /// </summary>
    public class CelestialValor : ModItem
    {
        private int swingCounter = 0;
        
        // 6x6 sprite sheet animation
        private const int FrameColumns = 6;
        private const int FrameRows = 6;
        private const int TotalFrames = 36;
        
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 342;
            Item.DamageType = DamageClass.Melee;
            Item.width = 80;
            Item.height = 80;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 7.5f;
            Item.value = Item.sellPrice(gold: 45);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<CelestialValorProjectile>();
            Item.shootSpeed = 14f;
            Item.noMelee = false;
            Item.maxStack = 1;
            Item.scale = 1.3f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Increment swing counter (1, 2, 3, then reset to 1)
            swingCounter++;
            if (swingCounter > 3)
                swingCounter = 1;
            
            // Get direction toward mouse
            Vector2 towardsMouse = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            
            // Fire projectiles based on swing count
            int projectileCount = swingCounter;
            float spreadAngle = projectileCount > 1 ? MathHelper.ToRadians(15f) : 0f;
            
            for (int i = 0; i < projectileCount; i++)
            {
                float angleOffset = 0f;
                if (projectileCount > 1)
                {
                    // Spread projectiles evenly
                    angleOffset = spreadAngle * ((float)i / (projectileCount - 1) - 0.5f) * 2f;
                }
                
                Vector2 projectileVelocity = towardsMouse.RotatedBy(angleOffset) * Item.shootSpeed;
                Projectile.NewProjectile(source, player.Center, projectileVelocity, type, (int)(damage * 0.88f), knockback, player.whoAmI);
            }
            
            // Swing particles - red and gold, more for higher swing count
            int particleCount = 8 + swingCounter * 3;
            for (int i = 0; i < particleCount; i++)
            {
                int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                Vector2 dustVel = towardsMouse.RotatedByRandom(0.8f) * Main.rand.NextFloat(4f, 10f);
                Dust swing = Dust.NewDustDirect(player.Center + towardsMouse * 40f, 1, 1, dustType, dustVel.X, dustVel.Y, 100, default, 1.5f);
                swing.noGravity = true;
            }
            
            return false;
        }

        public override void HoldItem(Player player)
        {
            // Dark red and gold particles while holding
            if (Main.rand.NextBool(2))
            {
                Vector2 offset = Main.rand.NextVector2Circular(25f, 25f);
                int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.Torch;
                Dust particle = Dust.NewDustDirect(player.Center + offset, 1, 1, dustType, 0f, -1.5f, 150, default, 1.1f);
                particle.noGravity = true;
                particle.velocity *= 0.4f;
                if (dustType == DustID.Torch)
                    particle.color = new Color(139, 0, 0); // Dark red
            }
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // Swing trail particles - dark red and gold
            if (Main.rand.NextBool(2))
            {
                int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.Torch;
                Dust trail = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, 
                    dustType, player.velocity.X * 0.2f, player.velocity.Y * 0.2f, 100, default, 1.4f);
                trail.noGravity = true;
                if (dustType == DustID.Torch)
                    trail.color = new Color(139, 0, 0); // Dark red
            }
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Impact effect
            for (int i = 0; i < 8; i++)
            {
                int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                Dust impact = Dust.NewDustDirect(target.position, target.width, target.height, 
                    dustType, 0f, 0f, 100, default, 1.5f);
                impact.noGravity = true;
                impact.velocity = Main.rand.NextVector2Circular(5f, 5f);
            }
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            // Use the inventory image for inventory display
            return true;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "EroicaWeapon", "A blade forged from the valor of countless heroes")
            {
                OverrideColor = new Color(255, 200, 100)
            });
            tooltips.Add(new TooltipLine(Mod, "SwingCombo", "Fires 1/2/3 projectiles on consecutive swings")
            {
                OverrideColor = new Color(255, 150, 100)
            });
        }
    }
}
