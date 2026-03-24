using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using MagnumOpus.Content.Summer.Materials;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;
using static MagnumOpus.Common.Systems.ThemedParticles;

namespace MagnumOpus.Content.Summer.Accessories
{
    /// <summary>
    /// Sunfire Pendant - Offensive Summer accessory
    /// Post-Mech tier, provides fire damage boost and inflicts On Fire
    /// </summary>
    public class SunfirePendant : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(gold: 5);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Generic) += 0.08f; // 8% damage boost
            player.magmaStone = true; // Inflicts On Fire on melee hits
        }

        public override void HoldItem(Player player)
        {
            // Ambient fire aura - every 18 frames
            if ((int)Main.GameUpdateCount % 18 == 0)
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(28f, 28f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), -1.2f - Main.rand.NextFloat(0.3f));
                Dust d = Dust.NewDustPerfect(pos, DustID.SolarFlare, vel, 0, new Color(255, 140, 0), 0.8f);
                d.noGravity = true;
            }

            // Pulsing fire lighting
            float pulse = 0.4f + 0.25f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4f);
            Lighting.AddLight(player.Center, new Color(255, 140, 0).ToVector3() * pulse);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color summerOrange = new Color(255, 140, 0);
            Color summerGold = new Color(255, 215, 0);
            
            tooltips.Add(new TooltipLine(Mod, "Season", "Summer Accessory") { OverrideColor = summerOrange });
            tooltips.Add(new TooltipLine(Mod, "Damage", "+8% damage") { OverrideColor = summerGold });
            tooltips.Add(new TooltipLine(Mod, "OnFire", "Melee attacks inflict On Fire!") { OverrideColor = summerOrange });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The sun's fury burns within'") { OverrideColor = Color.Lerp(summerOrange, Color.Yellow, 0.3f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SummerResonantEnergy>(), 2)
                .AddIngredient(ModContent.ItemType<SolarEssence>(), 4)
                .AddIngredient(ModContent.ItemType<EmberOfIntensity>(), 12)
                .AddIngredient(ItemID.MagmaStone, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    
    /// <summary>
    /// Zenith Band - Speed/Attack Summer accessory
    /// Post-Mech tier, boosts attack speed and movement
    /// </summary>
    public class ZenithBand : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.accessory = true;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(gold: 5);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetAttackSpeed(DamageClass.Generic) += 0.1f; // 10% attack speed
            player.moveSpeed += 0.12f; // 12% movement speed
            player.accRunSpeed += 1.5f;
        }

        public override void HoldItem(Player player)
        {
            // Speed aura with swirling particles - every 22 frames
            if ((int)Main.GameUpdateCount % 22 == 0)
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(32f, 32f);
                Vector2 vel = (player.Center - pos).SafeNormalize(Vector2.Zero) * 0.3f + new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-0.5f, 0.5f));
                Dust d = Dust.NewDustPerfect(pos, DustID.GoldCoin, vel, 0, new Color(255, 215, 0), 0.75f);
                d.noGravity = true;
            }

            // Pulsing speed aura lighting
            float pulse = 0.35f + 0.2f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3.5f);
            Lighting.AddLight(player.Center, new Color(255, 215, 0).ToVector3() * pulse);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color summerOrange = new Color(255, 140, 0);
            Color summerGold = new Color(255, 215, 0);
            
            tooltips.Add(new TooltipLine(Mod, "Season", "Summer Accessory") { OverrideColor = summerOrange });
            tooltips.Add(new TooltipLine(Mod, "Speed", "+10% attack speed") { OverrideColor = summerGold });
            tooltips.Add(new TooltipLine(Mod, "MoveSpeed", "+12% movement speed") { OverrideColor = summerGold });
            tooltips.Add(new TooltipLine(Mod, "RunSpeed", "Increased run acceleration") { OverrideColor = summerGold });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'At the zenith of summer, speed becomes light'") { OverrideColor = Color.Lerp(summerGold, Color.White, 0.3f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SummerResonantEnergy>(), 2)
                .AddIngredient(ModContent.ItemType<SolarEssence>(), 4)
                .AddIngredient(ModContent.ItemType<EmberOfIntensity>(), 12)
                .AddIngredient(ItemID.Aglet, 1)
                .AddIngredient(ItemID.AnkletoftheWind, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    
    /// <summary>
    /// Radiant Crown - Ultimate Summer accessory
    /// Post-Mech tier, provides significant combat bonuses during daytime
    /// </summary>
    public class RadiantCrown : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 26;
            Item.accessory = true;
            Item.rare = ItemRarityID.LightPurple;
            Item.value = Item.sellPrice(gold: 6);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // Base stats
            player.GetDamage(DamageClass.Generic) += 0.06f;
            player.statDefense += 5;
            
            // Daytime bonus
            if (Main.dayTime)
            {
                player.GetDamage(DamageClass.Generic) += 0.06f; // +6% more during day (12% total)
                player.GetCritChance(DamageClass.Generic) += 8;
                player.lifeRegen += 2;
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color summerOrange = new Color(255, 140, 0);
            Color summerGold = new Color(255, 215, 0);
            
            tooltips.Add(new TooltipLine(Mod, "Season", "Summer Accessory") { OverrideColor = summerOrange });
            tooltips.Add(new TooltipLine(Mod, "Base", "+6% damage, +5 defense") { OverrideColor = summerGold });
            tooltips.Add(new TooltipLine(Mod, "DayBonus", "During daytime: +6% additional damage, +8% crit, +2 life regen") { OverrideColor = summerOrange });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The crown blazes with the power of the midday sun'") { OverrideColor = Color.Lerp(summerGold, Color.White, 0.3f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SummerResonantEnergy>(), 3)
                .AddIngredient(ModContent.ItemType<SolarEssence>(), 6)
                .AddIngredient(ModContent.ItemType<EmberOfIntensity>(), 15)
                .AddIngredient(ItemID.SunStone, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    /// <summary>
    /// Summer accessories VFX player hook
    /// </summary>
    public class SummerAccessoriesPlayer : ModPlayer
    {
        public bool sunfirePendantEquipped = false;
        public bool zenithBandEquipped = false;

        public override void ResetEffects()
        {
            sunfirePendantEquipped = false;
            zenithBandEquipped = false;
        }

        public override void UpdateEquips()
        {
            if (Player.HasItem(ModContent.ItemType<SunfirePendant>()))
                sunfirePendantEquipped = true;
            if (Player.HasItem(ModContent.ItemType<ZenithBand>()))
                zenithBandEquipped = true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Color summerOrange = new Color(255, 140, 0);
            Color summerGold = new Color(255, 215, 0);

            if (sunfirePendantEquipped)
            {
                // 3-layer flash cascade
                CustomParticles.GenericFlare(target.Center, Color.White, 0.65f, 22);
                CustomParticles.GenericFlare(target.Center, summerGold, 0.55f, 20);
                CustomParticles.GenericFlare(target.Center, summerOrange, 0.45f, 18);

                // 6-point fire burst
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(2.5f, 4.5f);
                    Dust d = Dust.NewDustPerfect(target.Center, DustID.SolarFlare, burstVel, 0, summerOrange, 0.9f);
                    d.noGravity = true;
                }

                // 2 halo rings
                CustomParticles.HaloRing(target.Center, summerGold, 0.42f, 16);
                CustomParticles.HaloRing(target.Center, summerOrange * 0.8f, 0.35f, 14);

                // Music notes
                ThemedParticles.MusicNote(target.Center + new Vector2(-8, 0), Vector2.Zero, summerGold, 0.7f, 32);
                ThemedParticles.MusicNote(target.Center + new Vector2(8, 0), Vector2.Zero, summerOrange, 0.7f, 34);

                Lighting.AddLight(target.Center, summerGold.ToVector3() * 0.6f);
            }

            if (zenithBandEquipped)
            {
                // 3-layer flash cascade
                CustomParticles.GenericFlare(target.Center, Color.White, 0.6f, 20);
                CustomParticles.GenericFlare(target.Center, summerGold, 0.48f, 18);
                CustomParticles.GenericFlare(target.Center, summerOrange, 0.4f, 16);

                // 8-point speed burst
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                    Dust d = Dust.NewDustPerfect(target.Center, DustID.GoldCoin, burstVel, 0, summerGold, 0.85f);
                    d.noGravity = true;
                }

                // 2 halo rings
                CustomParticles.HaloRing(target.Center, summerGold, 0.4f, 16);
                CustomParticles.HaloRing(target.Center, summerOrange * 0.75f, 0.33f, 14);

                // Music notes
                ThemedParticles.MusicNote(target.Center + new Vector2(-10, 0), Vector2.Zero, summerGold, 0.75f, 32);
                ThemedParticles.MusicNote(target.Center + new Vector2(10, 0), Vector2.Zero, summerOrange, 0.75f, 34);

                Lighting.AddLight(target.Center, summerGold.ToVector3() * 0.55f);
            }
        }
    }
}
