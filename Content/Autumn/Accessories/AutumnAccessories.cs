using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using MagnumOpus.Content.Autumn.Materials;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;
using static MagnumOpus.Common.Systems.ThemedParticles;

namespace MagnumOpus.Content.Autumn.Accessories
{
    /// <summary>
    /// Reaper's Charm - Life Steal Autumn accessory
    /// Post-Wall of Flesh tier, provides life steal on hits
    /// </summary>
    public class ReapersCharm : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.accessory = true;
            Item.rare = ItemRarityID.Lime;
            Item.value = Item.sellPrice(gold: 7);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Generic) += 0.07f;
            
            // Life steal handled via OnHit
            player.GetModPlayer<ReapersCharmPlayer>().reapersCharmEquipped = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color autumnOrange = new Color(255, 100, 30);
            Color autumnBrown = new Color(139, 69, 19);
            
            tooltips.Add(new TooltipLine(Mod, "Season", "Autumn Accessory") { OverrideColor = autumnOrange });
            tooltips.Add(new TooltipLine(Mod, "Damage", "+7% damage") { OverrideColor = autumnBrown });
            tooltips.Add(new TooltipLine(Mod, "LifeSteal", "20% chance to life steal 4% of damage dealt (max 8 HP)") { OverrideColor = autumnOrange });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The reaper claims a portion of every soul'") { OverrideColor = Color.Lerp(autumnBrown, Color.Black, 0.3f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<AutumnResonantEnergy>(), 2)
                .AddIngredient(ModContent.ItemType<DecayEssence>(), 4)
                .AddIngredient(ModContent.ItemType<LeafOfEnding>(), 12)
                .AddIngredient(ItemID.Megashark, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    
    public class ReapersCharmPlayer : ModPlayer
    {
        public bool reapersCharmEquipped = false;
        
        public override void ResetEffects()
        {
            reapersCharmEquipped = false;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (reapersCharmEquipped && Main.rand.NextBool(5)) // 20% chance
            {
                int healAmount = (int)(damageDone * 0.04f); // 4% lifesteal
                healAmount = System.Math.Max(1, System.Math.Min(healAmount, 8)); // Cap at 8 HP
                Player.Heal(healAmount);
                
                // Lifesteal VFX
                CustomParticles.GenericFlare(target.Center, new Color(180, 80, 40), 0.4f, 15);
                
                // Music note burst for soul reap
                ThemedParticles.MusicNoteBurst(target.Center, new Color(139, 90, 43), 3, 2.5f);
                
                // Reaper glyphs
                CustomParticles.GlyphBurst(target.Center, new Color(139, 90, 43), 2, 2f);
                
                // Sparkle accent
                var sparkle = new SparkleParticle(target.Center + Main.rand.NextVector2Circular(8f, 8f),
                    Main.rand.NextVector2Circular(1.5f, 1.5f), new Color(218, 165, 32) * 0.4f, 0.15f, 12);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }
    }
    
    /// <summary>
    /// Twilight Ring - Crit-based Autumn accessory
    /// Post-Wall of Flesh tier, boosts crit chance and crit damage
    /// </summary>
    public class TwilightRing : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.accessory = true;
            Item.rare = ItemRarityID.Lime;
            Item.value = Item.sellPrice(gold: 7);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetCritChance(DamageClass.Generic) += 10;

            // Twilight glow effect based on time of day
            float twilightMult = 1f;
            if (!Main.dayTime && Main.time < 16200) // Early night (sunset)
                twilightMult = 1.3f;
            else if (Main.dayTime && Main.time > 38000) // Late day (approaching sunset)
                twilightMult = 1.2f;

            player.GetDamage(DamageClass.Generic) += 0.05f * twilightMult;
        }

        public override void HoldItem(Player player)
        {
            // Twilight aura particles - every 22 frames
            if ((int)Main.GameUpdateCount % 22 == 0)
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(28f, 28f);
                Vector2 vel = (player.Center - pos).SafeNormalize(Vector2.Zero) * 0.3f;
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, new Color(128, 64, 96), 0.75f);
                d.noGravity = true;
            }

            // Pulsing crit aura lighting
            float pulse = 0.25f + 0.15f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f);
            Lighting.AddLight(player.Center, new Color(128, 64, 96).ToVector3() * pulse);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color autumnOrange = new Color(255, 100, 30);
            Color twilightPurple = new Color(128, 64, 96);
            
            tooltips.Add(new TooltipLine(Mod, "Season", "Autumn Accessory") { OverrideColor = autumnOrange });
            tooltips.Add(new TooltipLine(Mod, "Crit", "+10% critical strike chance") { OverrideColor = twilightPurple });
            tooltips.Add(new TooltipLine(Mod, "Damage", "+5% damage (boosted during twilight hours)") { OverrideColor = twilightPurple });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The boundary between day and night holds great power'") { OverrideColor = Color.Lerp(autumnOrange, twilightPurple, 0.5f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<AutumnResonantEnergy>(), 2)
                .AddIngredient(ModContent.ItemType<DecayEssence>(), 4)
                .AddIngredient(ModContent.ItemType<LeafOfEnding>(), 12)
                .AddIngredient(ItemID.MoonStone, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    
    /// <summary>
    /// Harvest Mantle - Tank Autumn accessory
    /// Post-Wall of Flesh tier, provides significant defense and thorns
    /// </summary>
    public class HarvestMantle : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ItemRarityID.Yellow;
            Item.value = Item.sellPrice(gold: 8);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.statDefense += 12;
            player.endurance += 0.08f; // 8% damage reduction
            player.thorns = 1f; // Thorns damage
        }

        public override void HoldItem(Player player)
        {
            // Ambient harvest aura - every 20 frames
            if ((int)Main.GameUpdateCount % 20 == 0)
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(32f, 32f);
                Vector2 vel = Main.rand.NextVector2Circular(1.5f, 1.5f) - new Vector2(0, 0.5f);
                int dustType = Main.rand.NextBool() ? DustID.Torch : DustID.Copper;
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, new Color(218, 165, 32), 0.8f);
                d.noGravity = true;
            }

            // Pulsing defensive aura lighting
            float pulse = 0.3f + 0.15f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2.5f);
            Lighting.AddLight(player.Center, new Color(218, 165, 32).ToVector3() * pulse);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color autumnOrange = new Color(255, 100, 30);
            Color harvestGold = new Color(218, 165, 32);

            tooltips.Add(new TooltipLine(Mod, "Season", "Autumn Accessory") { OverrideColor = autumnOrange });
            tooltips.Add(new TooltipLine(Mod, "Defense", "+12 defense") { OverrideColor = harvestGold });
            tooltips.Add(new TooltipLine(Mod, "DR", "+8% damage reduction") { OverrideColor = harvestGold });
            tooltips.Add(new TooltipLine(Mod, "Thorns", "Attackers take 10 damage per hit") { OverrideColor = autumnOrange });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The harvest provides both bounty and protection'") { OverrideColor = Color.Lerp(harvestGold, Color.White, 0.3f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<AutumnResonantEnergy>(), 3)
                .AddIngredient(ModContent.ItemType<DecayEssence>(), 6)
                .AddIngredient(ModContent.ItemType<LeafOfEnding>(), 15)
                .AddIngredient(ItemID.PaladinsShield, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    /// <summary>
    /// Autumn accessories VFX player hook for TwilightRing and HarvestMantle
    /// </summary>
    public class AutumnAccessoriesPlayer : ModPlayer
    {
        public bool twilightRingEquipped = false;
        public bool harvestMantleEquipped = false;

        public override void ResetEffects()
        {
            twilightRingEquipped = false;
            harvestMantleEquipped = false;
        }

        public override void UpdateEquips()
        {
            if (Player.HasItem(ModContent.ItemType<TwilightRing>()))
                twilightRingEquipped = true;
            if (Player.HasItem(ModContent.ItemType<HarvestMantle>()))
                harvestMantleEquipped = true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Color autumnOrange = new Color(255, 100, 30);
            Color twilightPurple = new Color(128, 64, 96);
            Color harvestGold = new Color(218, 165, 32);

            if (twilightRingEquipped)
            {
                // Enhanced flare on crit
                if (hit.Crit)
                {
                    CustomParticles.GenericFlare(target.Center, Color.Yellow, 0.65f, 22);
                    CustomParticles.GenericFlare(target.Center, autumnOrange, 0.55f, 20);
                    CustomParticles.GenericFlare(target.Center, twilightPurple, 0.45f, 18);

                    // 8-point crit burst
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 8f;
                        Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(2.5f, 5f);
                        Dust d = Dust.NewDustPerfect(target.Center, DustID.Torch, burstVel, 0, Color.Yellow, 0.9f);
                        d.noGravity = true;
                    }
                }
                else
                {
                    // Standard impact
                    CustomParticles.GenericFlare(target.Center, Color.White, 0.5f, 18);
                    CustomParticles.GenericFlare(target.Center, twilightPurple, 0.4f, 16);
                    CustomParticles.GenericFlare(target.Center, autumnOrange, 0.32f, 14);

                    // 6-point burst
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 6f;
                        Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                        Dust d = Dust.NewDustPerfect(target.Center, DustID.Torch, burstVel, 0, twilightPurple, 0.8f);
                        d.noGravity = true;
                    }
                }

                // 2 halo rings
                CustomParticles.HaloRing(target.Center, twilightPurple, 0.38f, 16);
                CustomParticles.HaloRing(target.Center, autumnOrange * 0.75f, 0.3f, 14);

                // Music notes
                ThemedParticles.MusicNote(target.Center + new Vector2(-8, 0), Vector2.Zero, twilightPurple, 0.72f, 32);
                ThemedParticles.MusicNote(target.Center + new Vector2(8, 0), Vector2.Zero, autumnOrange, 0.72f, 34);

                Lighting.AddLight(target.Center, twilightPurple.ToVector3() * 0.5f);
            }

            if (harvestMantleEquipped)
            {
                // 3-layer flash cascade
                CustomParticles.GenericFlare(target.Center, Color.White, 0.6f, 20);
                CustomParticles.GenericFlare(target.Center, harvestGold, 0.5f, 18);
                CustomParticles.GenericFlare(target.Center, autumnOrange, 0.4f, 16);

                // 7-point thorns burst
                for (int i = 0; i < 7; i++)
                {
                    float angle = MathHelper.TwoPi * i / 7f;
                    Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                    Dust d = Dust.NewDustPerfect(target.Center, Main.rand.NextBool() ? DustID.Torch : DustID.Copper, burstVel, 0, harvestGold, 0.85f);
                    d.noGravity = true;
                }

                // 2 halo rings
                CustomParticles.HaloRing(target.Center, harvestGold, 0.4f, 16);
                CustomParticles.HaloRing(target.Center, autumnOrange * 0.7f, 0.32f, 14);

                // Music notes
                ThemedParticles.MusicNote(target.Center + new Vector2(-10, 0), Vector2.Zero, harvestGold, 0.7f, 32);
                ThemedParticles.MusicNote(target.Center + new Vector2(10, 0), Vector2.Zero, autumnOrange, 0.7f, 34);

                Lighting.AddLight(target.Center, harvestGold.ToVector3() * 0.55f);
            }
        }
    }
}
