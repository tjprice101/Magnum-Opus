using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
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
    /// Features Calamity-inspired visual effects with glowing outline and pulsing aura.
    /// </summary>
    public class CelestialValor : ModItem
    {
        private int swingCounter = 0;
        private float visualTimer = 0f;
        
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
            
            // Heroic sword arc based on swing count - escalating visuals
            Vector2 arcPos = player.Center + towardsMouse * 40f;
            if (swingCounter == 1)
            {
                // Single crescent slash
                CustomParticles.SwordArcSlash(arcPos, towardsMouse, CustomParticleSystem.EroicaColors.Gold, 0.5f);
            }
            else if (swingCounter == 2)
            {
                // Double helix intertwined slashes - scarlet and gold
                CustomParticles.SwordArcDoubleHelix(arcPos, towardsMouse * 4f, CustomParticleSystem.EroicaColors.Scarlet, CustomParticleSystem.EroicaColors.Gold, 0.45f);
            }
            else
            {
                // Triple burst - heroic finale
                CustomParticles.SwordArcBurst(arcPos, CustomParticleSystem.EroicaColors.Gold, 4, 0.4f);
            }
            
            // Swing particles - reduced for cleaner look
            ThemedParticles.EroicaSparks(arcPos, towardsMouse, 3 + swingCounter, 5f);
            
            // Musical notes burst on swing
            CustomParticles.EroicaMusicNotes(player.Center + towardsMouse * 30f, swingCounter + 1, 22f);
            
            return false;
        }

        public override void HoldItem(Player player)
        {
            // Increment visual timer for pulsing effects
            visualTimer += 1f;
            
            // Heroic magic sparkle field aura - valor's presence
            if (Main.rand.NextBool(10))
            {
                Vector2 offset = Main.rand.NextVector2Circular(35f, 35f);
                CustomParticles.MagicSparkleFieldAura(player.Center + offset, CustomParticleSystem.EroicaColors.Gold * 0.5f, 0.35f, 25);
            }
            
            // Occasional prismatic sparkle - heroic gleam
            if (Main.rand.NextBool(12))
            {
                CustomParticles.PrismaticSparkle(player.Center + Main.rand.NextVector2Circular(25f, 25f), CustomParticleSystem.EroicaColors.DarkGold, 0.2f);
            }
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // Swing trail with occasional sword arc slash
            if (Main.rand.NextBool(4))
            {
                Vector2 hitCenter = new Vector2(hitbox.X + hitbox.Width / 2, hitbox.Y + hitbox.Height / 2);
                Vector2 swingDir = (hitCenter - player.Center).SafeNormalize(Vector2.UnitX);
                CustomParticles.SwordArcSlash(hitCenter, swingDir, CustomParticleSystem.EroicaColors.Scarlet * 0.7f, 0.3f, swingDir.ToRotation());
            }
            
            // Themed trail particles
            if (Main.rand.NextBool(3))
            {
                Vector2 hitCenter = new Vector2(hitbox.X + hitbox.Width / 2, hitbox.Y + hitbox.Height / 2);
                ThemedParticles.EroicaTrail(hitCenter, player.velocity * 0.25f);
            }
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Heroic impact - golden prismatic burst
            CustomParticles.PrismaticSparkleBurst(target.Center, CustomParticleSystem.EroicaColors.Gold, 6);
            
            // Single elegant halo ring
            CustomParticles.EroicaHalo(target.Center, 0.6f);
            
            // Themed impact
            ThemedParticles.EroicaImpact(target.Center, 1.0f);
            
            // Musical accidentals on hit
            CustomParticles.EroicaMusicNotes(target.Center, 2, 18f);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            // Draw glowing backlight effect when dropped in world
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            // Calculate pulse
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.15f + 1f;
            
            // Begin additive blending for glow
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Outer scarlet glow
            spriteBatch.Draw(texture, position, null, new Color(255, 80, 60) * 0.4f, rotation, origin, scale * pulse * 1.3f, SpriteEffects.None, 0f);
            
            // Inner golden glow
            spriteBatch.Draw(texture, position, null, new Color(255, 200, 100) * 0.3f, rotation, origin, scale * pulse * 1.15f, SpriteEffects.None, 0f);
            
            // Return to normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Add lighting
            Lighting.AddLight(Item.Center, 0.8f, 0.4f, 0.2f);
            
            return true; // Draw the normal sprite too
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
