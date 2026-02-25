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
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Fate.Debuffs;
using MagnumOpus.Content.Fate.Projectiles;

namespace MagnumOpus.Content.Fate.ResonantWeapons
{
    /// <summary>
    /// Destiny's Crescendo - The rising peak of fate's symphony.
    /// Summons a cosmic deity that rapidly slashes enemies and fires cosmic light beams.
    /// </summary>
    public class DestinysCrescendo : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/DestinysCrescendo";
        
        public override void SetDefaults()
        {
            Item.damage = 400;
            Item.DamageType = DamageClass.Summon;
            Item.width = 44;
            Item.height = 44;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(gold: 55);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.mana = 20;
            Item.shoot = ModContent.ProjectileType<CosmicDeityMinion>();
            Item.buffType = ModContent.BuffType<CosmicDeityBuff>();
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Summons a cosmic deity that rapidly slashes enemies"));
            tooltips.Add(new TooltipLine(Mod, "FateSpecial", "The deity periodically fires cosmic light beams"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'At the crescendo, even gods must answer the conductor's call'")
            {
                OverrideColor = FatePalette.BrightCrimson
            });
        }
        
        public override void HoldItem(Player player)
        {
            DestinysCrescendoVFX.HoldItemVFX(player);
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            
            // Spawn deity at cursor
            Vector2 spawnPos = Main.MouseWorld;
            Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback, player.whoAmI);
            
            // Track for star circle effect
            player.GetModPlayer<FateWeaponEffectPlayer>()?.OnFateWeaponAttack(player.Center);
            
            // Spawn cosmic summon VFX at summon location
            DestinysCrescendoVFX.SummonVFX(spawnPos);
            
            // === SEEKING CRYSTALS - DESTINY'S HERALD SHARDS ===
            // Release cosmic crystals that seek enemies upon deity summoning
            SeekingCrystalHelper.SpawnFateCrystals(
                source,
                spawnPos,
                Vector2.Zero, // Radiate outward
                (int)(damage * 0.3f),
                3f,
                player.whoAmI,
                5
            );
            
            return false;
        }
    }
}
