using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using MagnumOpus.Content.Spring.Materials;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;
using static MagnumOpus.Common.Systems.ThemedParticles;

namespace MagnumOpus.Content.Spring.Accessories
{
    /// <summary>
    /// Petal Shield - Defensive Spring accessory
    /// Post-WoF tier, provides defense and damage reduction when hit
    /// </summary>
    public class PetalShield : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(gold: 3);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.statDefense += 8;
            player.endurance += 0.06f; // 6% damage reduction
        }

        public override void HoldItem(Player player)
        {
            // Ambient petal aura - every 20 frames
            if ((int)Main.GameUpdateCount % 20 == 0)
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(25f, 25f);
                Vector2 vel = (player.Center - pos).SafeNormalize(Vector2.Zero) * 0.5f;
                Dust d = Dust.NewDustPerfect(pos, DustID.PinkFairy, vel, 0, new Color(255, 183, 197), 0.7f);
                d.noGravity = true;
            }

            // Pulsing lighting
            float pulse = 0.3f + 0.2f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f);
            Lighting.AddLight(player.Center, new Color(255, 183, 197).ToVector3() * pulse);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color springPink = new Color(255, 183, 197);
            Color springGreen = new Color(144, 238, 144);
            
            tooltips.Add(new TooltipLine(Mod, "Season", "Spring Accessory") { OverrideColor = springPink });
            tooltips.Add(new TooltipLine(Mod, "Defense", "+8 defense") { OverrideColor = springGreen });
            tooltips.Add(new TooltipLine(Mod, "DR", "+6% damage reduction") { OverrideColor = springGreen });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Petals form an impenetrable shield of renewal'") { OverrideColor = Color.Lerp(springPink, Color.White, 0.3f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SpringResonantEnergy>(), 2)
                .AddIngredient(ModContent.ItemType<BlossomEssence>(), 4)
                .AddIngredient(ModContent.ItemType<PetalOfRebirth>(), 10)
                .AddIngredient(ItemID.CobaltShield, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    
    /// <summary>
    /// Growth Band - Regeneration Spring accessory
    /// Post-WoF tier, provides life regen and mana regen
    /// </summary>
    public class GrowthBand : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.accessory = true;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(gold: 3);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.lifeRegen += 3;
            player.manaRegen += 2;
            player.GetDamage(DamageClass.Generic) += 0.04f; // 4% damage boost
        }

        public override void HoldItem(Player player)
        {
            // Ambient growth leaf aura - every 25 frames
            if ((int)Main.GameUpdateCount % 25 == 0)
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -0.8f);
                Dust d = Dust.NewDustPerfect(pos, DustID.Grass, vel, 0, new Color(144, 238, 144), 0.8f);
                d.noGravity = true;
            }

            // Pulsing growth aura lighting
            float pulse = 0.2f + 0.15f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2.5f);
            Lighting.AddLight(player.Center, new Color(144, 238, 144).ToVector3() * pulse);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color springPink = new Color(255, 183, 197);
            Color springGreen = new Color(144, 238, 144);
            
            tooltips.Add(new TooltipLine(Mod, "Season", "Spring Accessory") { OverrideColor = springPink });
            tooltips.Add(new TooltipLine(Mod, "Regen", "+3 life regeneration") { OverrideColor = springGreen });
            tooltips.Add(new TooltipLine(Mod, "ManaRegen", "+2 mana regeneration") { OverrideColor = springGreen });
            tooltips.Add(new TooltipLine(Mod, "Damage", "+4% damage") { OverrideColor = springGreen });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Life flows through the wearer like sap through a tree'") { OverrideColor = Color.Lerp(springGreen, Color.White, 0.3f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SpringResonantEnergy>(), 2)
                .AddIngredient(ModContent.ItemType<BlossomEssence>(), 4)
                .AddIngredient(ModContent.ItemType<PetalOfRebirth>(), 10)
                .AddIngredient(ItemID.BandofRegeneration, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    
    /// <summary>
    /// Bloom Crest - Offensive Spring accessory
    /// Post-WoF tier, boosts damage and crit when moving
    /// </summary>
    public class BloomCrest : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.accessory = true;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(gold: 3, silver: 50);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Generic) += 0.06f; // 6% base damage
            
            // Bonus when moving
            if (player.velocity.Length() > 3f)
            {
                player.GetDamage(DamageClass.Generic) += 0.04f; // +4% when moving fast
                player.GetCritChance(DamageClass.Generic) += 5;
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color springPink = new Color(255, 183, 197);
            Color springGreen = new Color(144, 238, 144);
            
            tooltips.Add(new TooltipLine(Mod, "Season", "Spring Accessory") { OverrideColor = springPink });
            tooltips.Add(new TooltipLine(Mod, "Damage", "+6% damage (base)") { OverrideColor = springGreen });
            tooltips.Add(new TooltipLine(Mod, "Moving", "While moving: +4% additional damage, +5% crit chance") { OverrideColor = springPink });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The crest blooms brighter with each step forward'") { OverrideColor = Color.Lerp(springPink, Color.White, 0.3f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SpringResonantEnergy>(), 2)
                .AddIngredient(ModContent.ItemType<BlossomEssence>(), 4)
                .AddIngredient(ModContent.ItemType<PetalOfRebirth>(), 12)
                .AddIngredient(ItemID.WarriorEmblem, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    /// <summary>
    /// Spring accessories VFX player hook
    /// </summary>
    public class SpringAccessoriesPlayer : ModPlayer
    {
        public bool petalShieldEquipped = false;
        public bool growthBandEquipped = false;

        public override void ResetEffects()
        {
            petalShieldEquipped = false;
            growthBandEquipped = false;
        }

        public override void UpdateEquips()
        {
            if (Player.HasItem(ModContent.ItemType<PetalShield>()))
                petalShieldEquipped = true;
            if (Player.HasItem(ModContent.ItemType<GrowthBand>()))
                growthBandEquipped = true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Color springPink = new Color(255, 183, 197);
            Color springGreen = new Color(144, 238, 144);

            if (petalShieldEquipped)
            {
                // 3-layer flash cascade
                CustomParticles.GenericFlare(target.Center, Color.White, 0.6f, 20);
                CustomParticles.GenericFlare(target.Center, springPink, 0.5f, 18);
                CustomParticles.GenericFlare(target.Center, springGreen, 0.42f, 16);

                // 6-point petal burst
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(2.5f, 4f);
                    Dust d = Dust.NewDustPerfect(target.Center, DustID.PinkFairy, burstVel, 0, springPink, 0.8f);
                    d.noGravity = true;
                }

                // 2 halo rings
                CustomParticles.HaloRing(target.Center, springPink, 0.4f, 16);
                CustomParticles.HaloRing(target.Center, springGreen * 0.7f, 0.32f, 14);

                // Music notes
                ThemedParticles.MusicNote(target.Center, Vector2.Zero, springPink, 0.7f, 30);
                ThemedParticles.MusicNote(target.Center + new Vector2(10, 0), Vector2.Zero, springGreen, 0.7f, 32);

                Lighting.AddLight(target.Center, springPink.ToVector3() * 0.5f);
            }

            if (growthBandEquipped)
            {
                // 3-layer flash cascade with growth colors
                CustomParticles.GenericFlare(target.Center, Color.White, 0.5f, 18);
                CustomParticles.GenericFlare(target.Center, springGreen, 0.42f, 16);
                CustomParticles.GenericFlare(target.Center, springPink, 0.35f, 14);

                // 8-point leaf burst
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4.5f);
                    Dust d = Dust.NewDustPerfect(target.Center, DustID.Grass, burstVel, 0, springGreen, 0.85f);
                    d.noGravity = true;
                }

                // 2 halo rings for vitality
                CustomParticles.HaloRing(target.Center, springGreen, 0.38f, 16);
                CustomParticles.HaloRing(target.Center, springPink * 0.7f, 0.3f, 14);

                // Music notes for life theme
                ThemedParticles.MusicNote(target.Center + new Vector2(-10, 0), Vector2.Zero, springGreen, 0.72f, 32);
                ThemedParticles.MusicNote(target.Center + new Vector2(10, 0), Vector2.Zero, springPink, 0.72f, 34);

                Lighting.AddLight(target.Center, springGreen.ToVector3() * 0.55f);
            }
        }
    }
}
