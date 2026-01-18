using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Fate.Debuffs;
using MagnumOpus.Content.Fate.Projectiles;

namespace MagnumOpus.Content.Fate.ResonantWeapons
{
    /// <summary>
    /// Fate2Magic - Spectral Blade Storm
    /// Spawns random spectral sword blades that spiral toward the cursor and explode on contact.
    /// </summary>
    public class Fate2Magic : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.RazorbladeTyphoon;
        
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
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Spawns spectral blades that spiral toward the cursor"));
            tooltips.Add(new TooltipLine(Mod, "FateSpecial", "Blades explode on contact with enemies"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A tempest of blades from beyond the veil'")
            {
                OverrideColor = FateCosmicVFX.FateBrightRed
            });
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
            
            // Spawn VFX at blade origin
            FateCosmicVFX.SpawnGlyphBurst(spawnPos, 1, 3f, 0.2f);
            
            // Occasional cosmic spark at player
            if (Main.rand.NextBool(3))
            {
                var spark = new GlowSparkParticle(player.Center + Main.rand.NextVector2Circular(20f, 20f),
                    Main.rand.NextVector2Circular(2f, 2f), FateCosmicVFX.FateDarkPink, 0.2f, 10);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            return false;
        }
    }
}
