using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.DiesIrae.Weapons.DeathTollingBell.Particles;
using MagnumOpus.Content.DiesIrae.Weapons.DeathTollingBell.Utilities;

namespace MagnumOpus.Content.DiesIrae.Weapons.DeathTollingBell
{
    /// <summary>
    /// Death Tolling Bell — A summon weapon that conjures a spectral bell of wrath.
    /// The bell hovers near the player and periodically tolls, releasing devastating
    /// concentric rings of crimson-gold shockwaves.
    ///
    /// Stats: 1450 damage, 22 mana, summon. 1 minion slot.
    /// Theme: Dies Irae — the funeral bell, the toll of judgment.
    /// </summary>
    public class DeathTollingBell : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 1450;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 22;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(gold: 35);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.DiesIraeRarity>();
            Item.UseSound = SoundID.Item44;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<Projectiles.BellTollingMinion>();
            Item.buffType = ModContent.BuffType<Buffs.DeathTollingBellBuff>();
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position = Main.MouseWorld;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Apply buff
            player.AddBuff(Item.buffType, 2);

            // Spawn minion
            int proj = Projectile.NewProjectile(source, position, Vector2.Zero, type, damage, knockback, player.whoAmI);

            // ── Summon VFX: burst at spawn point ──
            SpawnSummonVFX(position);

            return false;
        }

        private void SpawnSummonVFX(Vector2 pos)
        {
            // Crimson bloom flash
            BellParticleHandler.SpawnParticle(new BellBloomParticle(pos, BellUtils.TollCrimson, 2f, 25));
            BellParticleHandler.SpawnParticle(new BellBloomParticle(pos, BellUtils.BellWhite, 0.8f, 15));

            // Expanding summon ring
            BellParticleHandler.SpawnParticle(new TollRingParticle(pos, 80f, BellUtils.EmberOrange, 4f, 20));
            BellParticleHandler.SpawnParticle(new TollRingParticle(pos, 50f, BellUtils.BurningResonance, 3f, 15));

            // Music note cascade
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2.5f, -0.5f));
                Color c = BellUtils.MulticolorLerp(Main.rand.NextFloat(),
                    BellUtils.TollCrimson, BellUtils.EmberOrange, BellUtils.EchoGold);
                BellParticleHandler.SpawnParticle(new BellNoteParticle(pos, vel, c, 0.5f, 45));
            }

            // Smoke plume
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                BellParticleHandler.SpawnParticle(new BellSmokeParticle(pos, vel, 0.6f, 30));
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Summons a spectral bell of wrath that hovers near you"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "The bell periodically tolls, releasing concentric rings of devastating shockwaves"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Three rings of twelve toll waves strike with escalating fury"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'When the bell tolls, no prayer is answered — only judgment remains.'")
            {
                OverrideColor = new Color(200, 50, 30)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<Content.DiesIrae.ResonanceEnergies.DiesIraeResonantEnergy>(), 30)
                .AddIngredient(ModContent.ItemType<Content.DiesIrae.ResonanceEnergies.ResonantCoreOfDiesIrae>(), 12)
                .AddIngredient(ModContent.ItemType<Content.DiesIrae.HarmonicCores.HarmonicCoreOfDiesIrae>(), 2)
                .AddIngredient(ItemID.LunarBar, 20)
                .AddTile(ModContent.TileType<Content.Fate.CraftingStations.FatesCosmicAnvilTile>())
                .Register();
        }
    }
}
