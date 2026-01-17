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
    /// Fate2Ranged - Cosmic Accelerator
    /// Fires slow rounds that accelerate, pierce enemies, and explode on walls/timeout.
    /// On explosion, spawns 3 homing cosmic rockets.
    /// </summary>
    public class Fate2Ranged : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.SniperRifle;
        
        public override void SetDefaults()
        {
            Item.damage = 580;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 58;
            Item.height = 22;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(gold: 55);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item40;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<AcceleratingCosmicRound>();
            Item.shootSpeed = 6f; // Starts slow
            Item.useAmmo = AmmoID.Bullet;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Fires slow rounds that accelerate as they travel"));
            tooltips.Add(new TooltipLine(Mod, "FateSpecial1", "Rounds pierce enemies and explode on impact with tiles"));
            tooltips.Add(new TooltipLine(Mod, "FateSpecial2", "Explosions spawn 3 homing cosmic rockets"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Time bends as the cosmos accelerates toward annihilation'")
            {
                OverrideColor = FateCosmicVFX.FateBrightRed
            });
        }
        
        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            return Main.rand.NextFloat() > 0.5f; // 50% ammo conservation
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Fire the accelerating round
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<AcceleratingCosmicRound>(), damage, knockback, player.whoAmI);
            
            // Track for star circle effect
            player.GetModPlayer<FateWeaponEffectPlayer>()?.OnFateWeaponAttack(player.Center);
            
            // Heavy muzzle flash
            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.Zero) * 40f;
            FateCosmicVFX.SpawnCosmicExplosion(muzzlePos, 0.5f);
            FateCosmicVFX.SpawnGlyphBurst(muzzlePos, 2, 4f, 0.3f);
            
            // Cosmic cloud at muzzle
            FateCosmicVFX.SpawnCosmicCloudTrail(muzzlePos, -velocity.SafeNormalize(Vector2.Zero) * 2f, 4);
            
            SoundEngine.PlaySound(SoundID.Item38 with { Pitch = -0.3f }, position);
            
            return false;
        }
    }
}
