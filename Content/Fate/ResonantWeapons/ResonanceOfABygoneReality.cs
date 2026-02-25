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
    /// Resonance of a Bygone Reality - Echoes of a universe that once was.
    /// A rapid-fire cosmic gun. Every 5th hit spawns a spectral blade that slashes enemies for 3 seconds.
    /// </summary>
    public class ResonanceOfABygoneReality : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/ResonanceOfABygoneReality";
        
        public override void SetDefaults()
        {
            Item.damage = 400;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 50;
            Item.height = 26;
            Item.useTime = 6;
            Item.useAnimation = 6;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 2f;
            Item.value = Item.sellPrice(gold: 52);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<FateRapidBullet>();
            Item.shootSpeed = 18f;
            Item.useAmmo = AmmoID.Bullet;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Rapid-fire cosmic bullets"));
            tooltips.Add(new TooltipLine(Mod, "FateSpecial", "Every 5th hit summons a spectral blade that slashes for 3 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Memories of a world erased from existence'")
            {
                OverrideColor = FatePalette.BrightCrimson
            });
        }

        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            return Main.rand.NextFloat() > 0.4f; // 40% chance to not consume
        }

        public override void HoldItem(Player player)
        {
            ResonanceOfABygoneRealityVFX.HoldItemVFX(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Spawn rapid bullet with slight spread
            Vector2 bulletVel = velocity.RotatedByRandom(0.05f);
            Projectile.NewProjectile(source, position, bulletVel, ModContent.ProjectileType<FateRapidBullet>(), damage, knockback, player.whoAmI);

            // Track for star circle effect
            player.GetModPlayer<FateWeaponEffectPlayer>()?.OnFateWeaponAttack(player.Center);

            // Muzzle flash VFX
            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.Zero) * 30f;
            ResonanceOfABygoneRealityVFX.MuzzleFlashVFX(muzzlePos, velocity.SafeNormalize(Vector2.UnitX));

            return false;
        }
    }
}
