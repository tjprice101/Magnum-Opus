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
    /// Fate1Summon - Cosmic Deity Staff
    /// Summons a cosmic deity that rapidly slashes enemies and fires cosmic light beams.
    /// </summary>
    public class Fate1Summon : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.StardustDragonStaff;
        
        public override void SetDefaults()
        {
            Item.damage = 320;
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
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The cosmos made manifest, a god of stars at your command'")
            {
                OverrideColor = FateCosmicVFX.FateBrightRed
            });
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            
            // Spawn deity at cursor
            Vector2 spawnPos = Main.MouseWorld;
            Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback, player.whoAmI);
            
            // Track for star circle effect
            player.GetModPlayer<FateWeaponEffectPlayer>()?.OnFateWeaponAttack(player.Center);
            
            // Spawn cosmic explosion effect at summon location
            FateCosmicVFX.SpawnCosmicExplosion(spawnPos, 1.2f);
            FateCosmicVFX.SpawnGlyphBurst(spawnPos, 6, 6f, 0.5f);
            
            // Star particles for celestial appearance
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 starVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                var star = new GlowSparkParticle(spawnPos, starVel, FateCosmicVFX.FateWhite, 0.3f, 20);
                MagnumParticleHandler.SpawnParticle(star);
            }
            
            return false;
        }
    }
}
