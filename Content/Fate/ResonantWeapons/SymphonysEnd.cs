using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Fate.Debuffs;
using MagnumOpus.Content.Fate.Projectiles;

namespace MagnumOpus.Content.Fate.ResonantWeapons
{
    /// <summary>
    /// Symphony's End - Where all melodies find their conclusion.
    /// Spawns random spectral sword blades that spiral toward the cursor and explode on contact.
    /// Enhanced with cosmic VFX, glowing world sprite, and musical identity.
    /// </summary>
    public class SymphonysEnd : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/SymphonysEnd";
        
        public override void SetDefaults()
        {
            Item.damage = 500;
            Item.DamageType = DamageClass.Magic;
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 8;
            Item.useAnimation = 8;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 55);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item8;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.mana = 8;
            Item.shoot = ModContent.ProjectileType<SpiralingSpectralBlade>();
            Item.shootSpeed = 10f;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Spawns aggressive spectral blades that hunt enemies"));
            tooltips.Add(new TooltipLine(Mod, "FateSpecial", "Blades dash at targets rapidly and explode on contact"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Every song must end, but this ending reshapes the cosmos'")
            {
                OverrideColor = FatePalette.BrightCrimson
            });
        }
        
        public override void HoldItem(Player player)
        {
            SymphonysEndVFX.HoldItemVFX(player);
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Track for star circle effect
            player.GetModPlayer<FateWeaponEffectPlayer>()?.OnFateWeaponAttack(player.Center);
            
            // Spawn blade at random offset from player
            Vector2 spawnOffset = Main.rand.NextVector2CircularEdge(60f, 60f);
            Vector2 spawnPos = player.Center + spawnOffset;
            
            // Direction toward cursor with spiral component
            Vector2 toCursor = (Main.MouseWorld - spawnPos).SafeNormalize(Vector2.UnitX);
            float spiralAngle = Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4);
            Vector2 spiralVel = toCursor.RotatedBy(spiralAngle) * velocity.Length();
            
            Projectile.NewProjectile(source, spawnPos, spiralVel, type, damage, knockback, player.whoAmI, 
                Main.MouseWorld.X, Main.MouseWorld.Y);
            
            // === ENHANCED SPAWN VFX ===
            SymphonysEndVFX.BladeSpawnVFX(spawnPos, spiralVel.SafeNormalize(Vector2.UnitX));
            
            return false;
        }
        
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            // === COSMIC GLOWING WORLD SPRITE ===
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            // Additive bloom layers via VFX helper
            FateVFXLibrary.BeginFateAdditive(spriteBatch);
            SymphonysEndVFX.PreDrawInWorldBloom(spriteBatch, texture, position, origin, rotation, scale);
            FateVFXLibrary.EndFateAdditive(spriteBatch);

            // Draw main sprite
            spriteBatch.Draw(texture, position, null, lightColor, rotation, origin, scale, SpriteEffects.None, 0f);

            // Cosmic world light
            Lighting.AddLight(Item.Center, FatePalette.DarkPink.ToVector3() * 0.6f);

            return false;
        }
        
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            // Subtle pulse effect in inventory for visual feedback
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.08f + 1f;
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            
            // Draw glow behind
            spriteBatch.Draw(texture, position, frame, FatePalette.DarkPink * 0.25f, 0f, origin, scale * pulse * 1.1f, SpriteEffects.None, 0f);
            
            // Draw main item
            spriteBatch.Draw(texture, position, frame, drawColor, 0f, origin, scale * pulse, SpriteEffects.None, 0f);
            
            return false;
        }
    }
}
