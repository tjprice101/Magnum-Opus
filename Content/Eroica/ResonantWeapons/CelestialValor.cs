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
using MagnumOpus.Common.Systems;

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
            Item.damage = 320; // Balanced: ~1150 effective DPS with projectiles
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
            
            // Swing particles using themed system
            ThemedParticles.EroicaSparks(player.Center + towardsMouse * 40f, towardsMouse, 4 + swingCounter * 2, 6f);
            ThemedParticles.EroicaSparkles(player.Center + towardsMouse * 30f, swingCounter * 2, 20f);
            
            // Musical notes burst on swing!
            ThemedParticles.EroicaMusicNotes(player.Center + towardsMouse * 30f, swingCounter + 2, 25f);
            
            return false;
        }

        public override void HoldItem(Player player)
        {
            // Ambient particles while holding
            if (Main.rand.NextBool(6))
            {
                ThemedParticles.EroicaAura(player.Center, 30f);
            }
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // Swing trail particles
            if (Main.rand.NextBool(3))
            {
                Vector2 hitCenter = new Vector2(hitbox.X + hitbox.Width / 2, hitbox.Y + hitbox.Height / 2);
                ThemedParticles.EroicaTrail(hitCenter, player.velocity * 0.3f);
            }
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Impact effect
            ThemedParticles.EroicaImpact(target.Center, 1.2f);
            
            // Musical accidentals on hit
            ThemedParticles.EroicaAccidentals(target.Center, 2, 20f);
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
