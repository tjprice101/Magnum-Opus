/* COMMENTED OUT — replaced by TheFinalFermata/TheFinalFermataItem.cs
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
    /// The Final Fermata - The last pause before eternal silence.
    /// Spawns 3 spectral Coda of Annihilation swords that orbit the player,
    /// then cast themselves at the nearest enemy and slash through twice.
    /// </summary>
    public class TheFinalFermata : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/TheFinalFermata";
        
        public override void SetDefaults()
        {
            Item.damage = 520;
            Item.DamageType = DamageClass.Magic;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 45;
            Item.useAnimation = 45;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 55);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item105 with { Pitch = 0.2f, Volume = 0.8f };
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.mana = 30;
            Item.shoot = ModContent.ProjectileType<FermataSpectralSword>();
            Item.shootSpeed = 1f;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Summons 3 spectral Coda blades that orbit you"));
            tooltips.Add(new TooltipLine(Mod, "FateSpecial", "The blades lock onto the nearest enemy and slash through twice"));
            tooltips.Add(new TooltipLine(Mod, "FateSpecial2", "Each blade deals massive damage on both passes"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'In the silence between notes, worlds are born and die'")
            {
                OverrideColor = FatePalette.BrightCrimson
            });
        }

        public override bool CanUseItem(Player player)
        {
            // Limit concurrent spectral swords
            return player.ownedProjectileCounts[ModContent.ProjectileType<FermataSpectralSword>()] < 6;
        }

        public override void HoldItem(Player player)
        {
            TheFinalFermataVFX.HoldItemVFX(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Track for star circle effect
            player.GetModPlayer<FateWeaponEffectPlayer>()?.OnFateWeaponAttack(player.Center);

            // Spawn 3 spectral Coda swords at different orbit positions
            for (int i = 0; i < 3; i++)
            {
                // Stagger spawn positions around player
                float spawnAngle = MathHelper.TwoPi * i / 3f;
                Vector2 spawnOffset = spawnAngle.ToRotationVector2() * 60f;
                Vector2 spawnPos = player.Center + spawnOffset;

                // ai[0] = phase (starts at 0 = Orbiting)
                // ai[1] = orbit index (0, 1, or 2)
                Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback, player.whoAmI, 0, i);

                // Spawn VFX per sword
                TheFinalFermataVFX.SwordSummonVFX(spawnPos);
            }

            // Dramatic sound
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 0.7f }, player.Center);

            return false;
        }
    }
}
*/
