using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using MagnumOpus.Content.Winter.Materials;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;
using static MagnumOpus.Common.Systems.ThemedParticles;

namespace MagnumOpus.Content.Winter.Accessories
{
    /// <summary>
    /// Frostbite Amulet - Offense Winter accessory
    /// Post-Mechanical Bosses tier, applies Frostburn to enemies
    /// </summary>
    public class FrostbiteAmulet : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
            Item.rare = ItemRarityID.Yellow;
            Item.value = Item.sellPrice(gold: 10);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Generic) += 0.1f; // 10% damage boost
            player.frostBurn = true; // Inflicts Frostburn
        }

        public override void HoldItem(Player player)
        {
            // Ambient frost crystals - every 20 frames
            if ((int)Main.GameUpdateCount % 20 == 0)
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(26f, 26f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -0.3f);
                Dust d = Dust.NewDustPerfect(pos, DustID.Frost, vel, 0, new Color(150, 220, 255), 0.8f);
                d.noGravity = true;
            }

            // Pulsing frost lighting
            float pulse = 0.25f + 0.15f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f);
            Lighting.AddLight(player.Center, new Color(150, 220, 255).ToVector3() * pulse);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color winterBlue = new Color(173, 216, 230);
            Color winterCyan = new Color(0, 255, 255);
            
            tooltips.Add(new TooltipLine(Mod, "Season", "Winter Accessory") { OverrideColor = winterBlue });
            tooltips.Add(new TooltipLine(Mod, "Damage", "+10% damage") { OverrideColor = winterCyan });
            tooltips.Add(new TooltipLine(Mod, "Frostburn", "Attacks inflict Frostburn") { OverrideColor = winterBlue });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The cold burns deeper than any flame'") { OverrideColor = Color.Lerp(winterBlue, Color.White, 0.3f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<WinterResonantEnergy>(), 2)
                .AddIngredient(ModContent.ItemType<FrostEssence>(), 4)
                .AddIngredient(ModContent.ItemType<ShardOfStillness>(), 12)
                .AddIngredient(ItemID.FrostCore, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    
    /// <summary>
    /// Stillness Shrine - Defense/Regen Winter accessory
    /// Post-Mechanical Bosses tier, provides defense and regen when stationary
    /// </summary>
    public class StillnessShrine : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ItemRarityID.Yellow;
            Item.value = Item.sellPrice(gold: 10);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.statDefense += 8;
            player.lifeRegen += 2;

            // Bonus when stationary
            if (player.velocity.Length() < 1f)
            {
                player.statDefense += 8; // +8 more when still (16 total)
                player.lifeRegen += 4; // +4 more regen when still
                player.endurance += 0.1f; // 10% damage reduction when still
            }
        }

        public override void HoldItem(Player player)
        {
            // Ambient frost aura - every 18 frames
            if ((int)Main.GameUpdateCount % 18 == 0)
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                Vector2 vel = (player.Center - pos).SafeNormalize(Vector2.Zero) * 0.4f;
                Dust d = Dust.NewDustPerfect(pos, DustID.Frost, vel, 0, new Color(200, 230, 255), 0.8f);
                d.noGravity = true;
            }

            // Stronger glow when stationary
            float pulse = 0.3f + 0.15f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2.5f);
            if (player.velocity.Length() < 1f)
            {
                pulse += 0.2f; // Enhanced glow when still
            }
            Lighting.AddLight(player.Center, new Color(200, 230, 255).ToVector3() * pulse);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color winterBlue = new Color(173, 216, 230);
            Color winterWhite = new Color(200, 230, 255);
            
            tooltips.Add(new TooltipLine(Mod, "Season", "Winter Accessory") { OverrideColor = winterBlue });
            tooltips.Add(new TooltipLine(Mod, "Base", "+8 defense, +2 life regeneration") { OverrideColor = winterWhite });
            tooltips.Add(new TooltipLine(Mod, "Stationary", "While stationary: +8 additional defense, +4 life regen, +10% DR") { OverrideColor = winterBlue });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'In perfect stillness, the cold becomes a shield'") { OverrideColor = Color.Lerp(winterWhite, Color.White, 0.3f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<WinterResonantEnergy>(), 2)
                .AddIngredient(ModContent.ItemType<FrostEssence>(), 4)
                .AddIngredient(ModContent.ItemType<ShardOfStillness>(), 12)
                .AddIngredient(ItemID.FrozenTurtleShell, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    
    /// <summary>
    /// Glacial Heart - Ultimate Winter accessory
    /// Post-Mechanical Bosses tier, provides immunity to frost and significant bonuses
    /// </summary>
    public class GlacialHeart : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
            Item.rare = ItemRarityID.Cyan;
            Item.value = Item.sellPrice(gold: 12);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // Immunity to frost effects
            player.buffImmune[BuffID.Frozen] = true;
            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            
            // Combat bonuses
            player.GetDamage(DamageClass.Generic) += 0.08f;
            player.GetCritChance(DamageClass.Generic) += 6;
            player.statDefense += 6;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color winterBlue = new Color(173, 216, 230);
            Color winterCyan = new Color(0, 255, 255);
            
            tooltips.Add(new TooltipLine(Mod, "Season", "Winter Accessory") { OverrideColor = winterBlue });
            tooltips.Add(new TooltipLine(Mod, "Stats", "+8% damage, +6% crit, +6 defense") { OverrideColor = winterCyan });
            tooltips.Add(new TooltipLine(Mod, "Immunity", "Immune to Frozen, Chilled, and Frostburn") { OverrideColor = winterBlue });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A heart of ice beats eternal'") { OverrideColor = Color.Lerp(winterCyan, Color.White, 0.3f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<WinterResonantEnergy>(), 3)
                .AddIngredient(ModContent.ItemType<FrostEssence>(), 6)
                .AddIngredient(ModContent.ItemType<ShardOfStillness>(), 18)
                .AddIngredient(ItemID.FrozenTurtleShell, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    /// <summary>
    /// Winter accessories VFX player hook
    /// </summary>
    public class WinterAccessoriesPlayer : ModPlayer
    {
        public bool frostbiteAmuletEquipped = false;
        public bool stillnessShrineEquipped = false;

        public override void ResetEffects()
        {
            frostbiteAmuletEquipped = false;
            stillnessShrineEquipped = false;
        }

        public override void UpdateEquips()
        {
            if (Player.HasItem(ModContent.ItemType<FrostbiteAmulet>()))
                frostbiteAmuletEquipped = true;
            if (Player.HasItem(ModContent.ItemType<StillnessShrine>()))
                stillnessShrineEquipped = true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Color winterBlue = new Color(150, 220, 255);
            Color winterWhite = new Color(240, 250, 255);

            if (frostbiteAmuletEquipped)
            {
                // 3-layer flash cascade
                CustomParticles.GenericFlare(target.Center, Color.White, 0.62f, 20);
                CustomParticles.GenericFlare(target.Center, winterBlue, 0.52f, 18);
                CustomParticles.GenericFlare(target.Center, winterWhite, 0.42f, 16);

                // 6-point frost shard burst
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                    Dust d = Dust.NewDustPerfect(target.Center, DustID.Frost, burstVel, 0, winterBlue, 0.85f);
                    d.noGravity = true;
                }

                // 2 halo rings
                CustomParticles.HaloRing(target.Center, winterBlue, 0.38f, 16);
                CustomParticles.HaloRing(target.Center, winterWhite * 0.8f, 0.3f, 14);

                // Music notes
                ThemedParticles.MusicNote(target.Center + new Vector2(-8, 0), Vector2.Zero, winterBlue, 0.7f, 30);
                ThemedParticles.MusicNote(target.Center + new Vector2(8, 0), Vector2.Zero, winterWhite, 0.7f, 32);

                Lighting.AddLight(target.Center, winterBlue.ToVector3() * 0.5f);
            }

            if (stillnessShrineEquipped)
            {
                Color winterBlue2 = new Color(173, 216, 230);
                Color winterWhite2 = new Color(200, 230, 255);

                // 3-layer flash cascade
                CustomParticles.GenericFlare(target.Center, Color.White, 0.58f, 18);
                CustomParticles.GenericFlare(target.Center, winterWhite2, 0.48f, 16);
                CustomParticles.GenericFlare(target.Center, winterBlue2, 0.4f, 14);

                // 7-point defensive burst
                for (int i = 0; i < 7; i++)
                {
                    float angle = MathHelper.TwoPi * i / 7f;
                    Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 3.5f);
                    Dust d = Dust.NewDustPerfect(target.Center, DustID.Frost, burstVel, 0, winterBlue2, 0.8f);
                    d.noGravity = true;
                }

                // 2 halo rings
                CustomParticles.HaloRing(target.Center, winterWhite2, 0.36f, 16);
                CustomParticles.HaloRing(target.Center, winterBlue2 * 0.8f, 0.28f, 14);

                // Music notes
                ThemedParticles.MusicNote(target.Center + new Vector2(-10, 0), Vector2.Zero, winterBlue2, 0.72f, 32);
                ThemedParticles.MusicNote(target.Center + new Vector2(10, 0), Vector2.Zero, winterWhite2, 0.72f, 34);

                Lighting.AddLight(target.Center, winterWhite2.ToVector3() * 0.5f);
            }
        }
    }
}
