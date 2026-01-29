using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Materials.Foundation;
using MagnumOpus.Content.Spring.Materials;
using MagnumOpus.Content.Summer.Materials;
using MagnumOpus.Content.Autumn.Materials;
using MagnumOpus.Content.Winter.Materials;
using MagnumOpus.Content.Seasons.Accessories;

namespace MagnumOpus.Content.Common.Accessories.MeleeChain
{
    #region Tier 1: Pre-Hardmode Foundation

    /// <summary>
    /// Resonant Rhythm Band - Base tier melee chain accessory.
    /// Enables the Resonance Combo System: hitting enemies builds Resonance stacks (max 10).
    /// Lose 1 stack every 2 seconds of not hitting.
    /// </summary>
    public class ResonantRhythmBand : ModItem
    {
        private static readonly Color BasePurple = new Color(180, 130, 255);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Blue;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var resonancePlayer = player.GetModPlayer<ResonanceComboPlayer>();
            resonancePlayer.hasResonantRhythmBand = true;
            
            // Ambient particles showing resonance level
            if (!hideVisual && resonancePlayer.resonanceStacks > 0 && Main.rand.NextBool(15))
            {
                float intensity = resonancePlayer.GetResonancePercent();
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(25f, 25f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(0.5f, 1f));
                CustomParticles.GenericGlow(pos, vel, BasePurple * intensity, 0.2f + intensity * 0.15f, 18, true);
                
                // Beat lines at high stacks
                if (resonancePlayer.resonanceStacks >= 7)
                {
                    CustomParticles.HaloRing(player.Center, BasePurple * 0.3f, 0.15f, 10);
                }
            }
            
            // Pulsing light based on stacks
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.15f + 0.2f;
            float stackIntensity = (float)resonancePlayer.resonanceStacks / 10f;
            Lighting.AddLight(player.Center, BasePurple.ToVector3() * pulse * stackIntensity);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var resonancePlayer = player.GetModPlayer<ResonanceComboPlayer>();
            
            tooltips.Add(new TooltipLine(Mod, "System", "Enables the Resonance Combo System")
            {
                OverrideColor = BasePurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Melee hits build Resonance stacks (max 10)")
            {
                OverrideColor = new Color(200, 180, 255)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Lose 1 stack every 2 seconds without hitting")
            {
                OverrideColor = new Color(180, 160, 220)
            });
            
            // Show current stacks if equipped
            if (resonancePlayer.hasResonantRhythmBand)
            {
                tooltips.Add(new TooltipLine(Mod, "Stacks", $"Current Resonance: {resonancePlayer.resonanceStacks}/{resonancePlayer.maxResonance}")
                {
                    OverrideColor = Color.Lerp(Color.Gray, BasePurple, resonancePlayer.GetResonancePercent())
                });
            }
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The rhythm of battle begins with a single beat'")
            {
                OverrideColor = new Color(150, 150, 150)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantCrystalShard>(10)
                .AddIngredient(ItemID.BandofRegeneration, 1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    /// <summary>
    /// Spring Tempo Charm - Post-Primavera tier.
    /// Increases max Resonance to 15. At 10+ stacks: +8% melee speed.
    /// </summary>
    public class SpringTempoCharm : ModItem
    {
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SpringGreen = new Color(144, 238, 144);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 3);
            Item.rare = ItemRarityID.Orange;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var resonancePlayer = player.GetModPlayer<ResonanceComboPlayer>();
            resonancePlayer.hasResonantRhythmBand = true;
            resonancePlayer.hasSpringTempoCharm = true;
            
            // Spring-themed particles
            if (!hideVisual && resonancePlayer.resonanceStacks > 0 && Main.rand.NextBool(12))
            {
                float intensity = resonancePlayer.GetResonancePercent();
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(28f, 28f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), -Main.rand.NextFloat(0.5f, 1.2f));
                Color particleColor = Color.Lerp(SpringPink, SpringGreen, Main.rand.NextFloat());
                CustomParticles.GenericGlow(pos, vel, particleColor * intensity, 0.22f + intensity * 0.12f, 22, true);
                
                // Cherry blossom at high stacks
                if (resonancePlayer.resonanceStacks >= 10 && Main.rand.NextBool(3))
                {
                    ThemedParticles.MusicNote(pos, vel * 0.6f, SpringPink, 0.25f, 20);
                }
            }
            
            // Pulsing spring light
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.15f + 0.25f;
            float stackIntensity = (float)resonancePlayer.resonanceStacks / 15f;
            Lighting.AddLight(player.Center, SpringPink.ToVector3() * pulse * stackIntensity);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var resonancePlayer = player.GetModPlayer<ResonanceComboPlayer>();
            
            tooltips.Add(new TooltipLine(Mod, "Upgrade", "Resonance Combo System (Upgraded)")
            {
                OverrideColor = SpringPink
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Max Resonance increased to 15")
            {
                OverrideColor = new Color(255, 200, 210)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "At 10+ stacks: +8% melee speed")
            {
                OverrideColor = SpringGreen
            });
            
            // Show current stacks if equipped
            if (resonancePlayer.hasSpringTempoCharm)
            {
                bool thresholdMet = resonancePlayer.resonanceStacks >= 10;
                tooltips.Add(new TooltipLine(Mod, "Stacks", $"Current Resonance: {resonancePlayer.resonanceStacks}/{resonancePlayer.maxResonance}")
                {
                    OverrideColor = thresholdMet ? SpringGreen : Color.Lerp(Color.Gray, SpringPink, resonancePlayer.GetResonancePercent())
                });
                
                if (thresholdMet)
                {
                    tooltips.Add(new TooltipLine(Mod, "Active", "✓ Speed bonus active!")
                    {
                        OverrideColor = SpringGreen
                    });
                }
            }
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Spring awakens the tempo of new beginnings'")
            {
                OverrideColor = new Color(150, 150, 150)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantRhythmBand>(1)
                .AddIngredient<VernalBar>(15)
                .AddIngredient<SpringResonantEnergy>(1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    #endregion

    #region Tier 2: Mid Pre-Hardmode (Post-L'Estate)

    /// <summary>
    /// Solar Crescendo Ring - Post-L'Estate tier.
    /// Max Resonance 20. At 15+ stacks: melee attacks inflict "Scorched" (fire DoT that stacks).
    /// </summary>
    public class SolarCrescendoRing : ModItem
    {
        private static readonly Color SummerOrange = new Color(255, 140, 0);
        private static readonly Color SummerGold = new Color(255, 215, 0);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.LightRed;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var resonancePlayer = player.GetModPlayer<ResonanceComboPlayer>();
            resonancePlayer.hasResonantRhythmBand = true;
            resonancePlayer.hasSpringTempoCharm = true;
            resonancePlayer.hasSolarCrescendoRing = true;
            
            // Solar flame particles
            if (!hideVisual && resonancePlayer.resonanceStacks > 0 && Main.rand.NextBool(10))
            {
                float intensity = resonancePlayer.GetResonancePercent();
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(1f, 2f));
                Color fireColor = Color.Lerp(SummerOrange, SummerGold, Main.rand.NextFloat());
                CustomParticles.GenericGlow(pos, vel, fireColor * intensity, 0.25f + intensity * 0.15f, 20, true);
                
                // Fire burst at threshold
                if (resonancePlayer.resonanceStacks >= 15 && Main.rand.NextBool(4))
                {
                    CustomParticles.GenericFlare(pos, SummerOrange, 0.3f, 12);
                }
            }
            
            // Warm light
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.07f) * 0.2f + 0.3f;
            float stackIntensity = (float)resonancePlayer.resonanceStacks / 20f;
            Lighting.AddLight(player.Center, SummerOrange.ToVector3() * pulse * stackIntensity);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var resonancePlayer = player.GetModPlayer<ResonanceComboPlayer>();
            
            tooltips.Add(new TooltipLine(Mod, "Upgrade", "Resonance Combo System (Solar)")
            {
                OverrideColor = SummerOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Max Resonance increased to 20")
            {
                OverrideColor = new Color(255, 180, 100)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "At 10+ stacks: +8% melee speed")
            {
                OverrideColor = new Color(255, 200, 150)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "At 15+ stacks: melee attacks inflict Scorched")
            {
                OverrideColor = SummerGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Scorched", "Scorched: Stacking fire damage over time")
            {
                OverrideColor = new Color(255, 100, 50)
            });
            
            // Show current stacks if equipped
            if (resonancePlayer.hasSolarCrescendoRing)
            {
                bool thresholdMet = resonancePlayer.resonanceStacks >= 15;
                tooltips.Add(new TooltipLine(Mod, "Stacks", $"Current Resonance: {resonancePlayer.resonanceStacks}/{resonancePlayer.maxResonance}")
                {
                    OverrideColor = thresholdMet ? SummerGold : Color.Lerp(Color.Gray, SummerOrange, resonancePlayer.GetResonancePercent())
                });
            }
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The summer sun fuels an ever-rising crescendo'")
            {
                OverrideColor = new Color(150, 150, 150)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SpringTempoCharm>(1)
                .AddIngredient<SolsticeBar>(15)
                .AddIngredient<SummerResonantEnergy>(1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    #endregion

    #region Tier 3: Early Hardmode (Post-Autunno)

    /// <summary>
    /// Harvest Rhythm Signet - Post-Autunno tier.
    /// Max Resonance 25. At 20+ stacks: 1% lifesteal. Losing stacks grants brief regen.
    /// </summary>
    public class HarvestRhythmSignet : ModItem
    {
        private static readonly Color AutumnOrange = new Color(255, 100, 30);
        private static readonly Color AutumnBrown = new Color(139, 69, 19);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 8);
            Item.rare = ItemRarityID.Pink;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var resonancePlayer = player.GetModPlayer<ResonanceComboPlayer>();
            resonancePlayer.hasResonantRhythmBand = true;
            resonancePlayer.hasSpringTempoCharm = true;
            resonancePlayer.hasSolarCrescendoRing = true;
            resonancePlayer.hasHarvestRhythmSignet = true;
            
            // Autumn leaf particles
            if (!hideVisual && resonancePlayer.resonanceStacks > 0 && Main.rand.NextBool(10))
            {
                float intensity = resonancePlayer.GetResonancePercent();
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(32f, 32f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(0.5f, 1.5f));
                Color leafColor = Color.Lerp(AutumnOrange, AutumnBrown, Main.rand.NextFloat());
                CustomParticles.GenericGlow(pos, vel, leafColor * intensity, 0.24f + intensity * 0.12f, 25, true);
                
                // Life energy at threshold
                if (resonancePlayer.resonanceStacks >= 20 && Main.rand.NextBool(5))
                {
                    CustomParticles.GenericGlow(player.Center, Vector2.UnitY * -1f, new Color(180, 255, 180) * 0.6f, 0.2f, 15, true);
                }
            }
            
            // Warm autumn light
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.15f + 0.3f;
            float stackIntensity = (float)resonancePlayer.resonanceStacks / 25f;
            Lighting.AddLight(player.Center, AutumnOrange.ToVector3() * pulse * stackIntensity);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var resonancePlayer = player.GetModPlayer<ResonanceComboPlayer>();
            
            tooltips.Add(new TooltipLine(Mod, "Upgrade", "Resonance Combo System (Harvest)")
            {
                OverrideColor = AutumnOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Max Resonance increased to 25")
            {
                OverrideColor = new Color(255, 150, 80)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "At 10+ stacks: +8% melee speed")
            {
                OverrideColor = new Color(255, 180, 120)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "At 15+ stacks: melee attacks inflict Scorched")
            {
                OverrideColor = new Color(255, 140, 0)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "At 20+ stacks: 1% lifesteal on melee hits")
            {
                OverrideColor = new Color(180, 255, 180)
            });
            
            // Show current stacks if equipped
            if (resonancePlayer.hasHarvestRhythmSignet)
            {
                bool thresholdMet = resonancePlayer.resonanceStacks >= 20;
                tooltips.Add(new TooltipLine(Mod, "Stacks", $"Current Resonance: {resonancePlayer.resonanceStacks}/{resonancePlayer.maxResonance}")
                {
                    OverrideColor = thresholdMet ? new Color(180, 255, 180) : Color.Lerp(Color.Gray, AutumnOrange, resonancePlayer.GetResonancePercent())
                });
            }
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The harvest reaps what the rhythm sows'")
            {
                OverrideColor = new Color(150, 150, 150)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SolarCrescendoRing>(1)
                .AddIngredient<HarvestBar>(20)
                .AddIngredient<AutumnResonantEnergy>(1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    #endregion

    #region Tier 4: Post-Mech (Post-L'Inverno)

    /// <summary>
    /// Permafrost Cadence Seal - Post-L'Inverno tier.
    /// Max Resonance 30. At 25+ stacks: hits freeze nearby enemies briefly.
    /// </summary>
    public class PermafrostCadenceSeal : ModItem
    {
        private static readonly Color WinterBlue = new Color(150, 220, 255);
        private static readonly Color WinterWhite = new Color(230, 240, 255);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 12);
            Item.rare = ItemRarityID.LightPurple;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var resonancePlayer = player.GetModPlayer<ResonanceComboPlayer>();
            resonancePlayer.hasResonantRhythmBand = true;
            resonancePlayer.hasSpringTempoCharm = true;
            resonancePlayer.hasSolarCrescendoRing = true;
            resonancePlayer.hasHarvestRhythmSignet = true;
            resonancePlayer.hasPermafrostCadenceSeal = true;
            
            // Frost particles
            if (!hideVisual && resonancePlayer.resonanceStacks > 0 && Main.rand.NextBool(9))
            {
                float intensity = resonancePlayer.GetResonancePercent();
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                Vector2 vel = Main.rand.NextVector2Circular(1f, 1f);
                Color frostColor = Color.Lerp(WinterBlue, WinterWhite, Main.rand.NextFloat());
                CustomParticles.GenericGlow(pos, vel, frostColor * intensity, 0.22f + intensity * 0.15f, 24, true);
                
                // Ice crystals at threshold
                if (resonancePlayer.resonanceStacks >= 25 && Main.rand.NextBool(4))
                {
                    CustomParticles.GenericFlare(pos, WinterWhite, 0.25f, 15);
                }
            }
            
            // Cold light
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.1f + 0.35f;
            float stackIntensity = (float)resonancePlayer.resonanceStacks / 30f;
            Lighting.AddLight(player.Center, WinterBlue.ToVector3() * pulse * stackIntensity);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var resonancePlayer = player.GetModPlayer<ResonanceComboPlayer>();
            
            tooltips.Add(new TooltipLine(Mod, "Upgrade", "Resonance Combo System (Permafrost)")
            {
                OverrideColor = WinterBlue
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Max Resonance increased to 30")
            {
                OverrideColor = new Color(180, 230, 255)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "At 10+ stacks: +8% melee speed")
            {
                OverrideColor = new Color(200, 235, 255)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "At 15+ stacks: melee attacks inflict Scorched")
            {
                OverrideColor = new Color(255, 140, 0)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "At 20+ stacks: 1% lifesteal on melee hits")
            {
                OverrideColor = new Color(180, 255, 180)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect5", "At 25+ stacks: melee hits freeze enemies briefly")
            {
                OverrideColor = WinterWhite
            });
            
            // Show current stacks if equipped
            if (resonancePlayer.hasPermafrostCadenceSeal)
            {
                bool thresholdMet = resonancePlayer.resonanceStacks >= 25;
                tooltips.Add(new TooltipLine(Mod, "Stacks", $"Current Resonance: {resonancePlayer.resonanceStacks}/{resonancePlayer.maxResonance}")
                {
                    OverrideColor = thresholdMet ? WinterWhite : Color.Lerp(Color.Gray, WinterBlue, resonancePlayer.GetResonancePercent())
                });
            }
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Winter's cadence freezes time itself'")
            {
                OverrideColor = new Color(150, 150, 150)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<HarvestRhythmSignet>(1)
                .AddIngredient<PermafrostBar>(25)
                .AddIngredient<WinterResonantEnergy>(1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    /// <summary>
    /// Vivaldi's Tempo Master - Post-Plantera tier (All Seasons).
    /// Max Resonance 40. Consume 30 stacks to unleash a seasonal elemental burst.
    /// </summary>
    public class VivaldisTempoMaster : ModItem
    {
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SummerOrange = new Color(255, 140, 0);
        private static readonly Color AutumnBrown = new Color(180, 100, 40);
        private static readonly Color WinterBlue = new Color(150, 220, 255);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 20);
            Item.rare = ItemRarityID.Lime;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var resonancePlayer = player.GetModPlayer<ResonanceComboPlayer>();
            resonancePlayer.hasResonantRhythmBand = true;
            resonancePlayer.hasSpringTempoCharm = true;
            resonancePlayer.hasSolarCrescendoRing = true;
            resonancePlayer.hasHarvestRhythmSignet = true;
            resonancePlayer.hasPermafrostCadenceSeal = true;
            resonancePlayer.hasVivaldisTempoMaster = true;
            
            // Get current season color based on time
            Color seasonColor = GetCurrentSeasonColor();
            
            // Seasonal particles
            if (!hideVisual && resonancePlayer.resonanceStacks > 0 && Main.rand.NextBool(8))
            {
                float intensity = resonancePlayer.GetResonancePercent();
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(38f, 38f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f));
                CustomParticles.GenericGlow(pos, vel, seasonColor * intensity, 0.26f + intensity * 0.14f, 26, true);
                
                // Music note at high stacks
                if (resonancePlayer.resonanceStacks >= 25 && Main.rand.NextBool(4))
                {
                    ThemedParticles.MusicNote(pos, vel * 0.5f, seasonColor, 0.3f, 22);
                }
            }
            
            // Rainbow-cycling light
            float hue = (Main.GameUpdateCount * 0.005f) % 1f;
            Color lightColor = Main.hslToRgb(hue, 0.7f, 0.5f);
            float stackIntensity = (float)resonancePlayer.resonanceStacks / 40f;
            Lighting.AddLight(player.Center, lightColor.ToVector3() * 0.4f * stackIntensity);
        }
        
        private Color GetCurrentSeasonColor()
        {
            // Cycle through seasons based on game time
            int season = (int)((Main.time / 15000) % 4);
            return season switch
            {
                0 => SpringPink,
                1 => SummerOrange,
                2 => AutumnBrown,
                _ => WinterBlue
            };
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var resonancePlayer = player.GetModPlayer<ResonanceComboPlayer>();
            
            // Cycling rainbow color for title
            float hue = (Main.GameUpdateCount * 0.01f) % 1f;
            Color titleColor = Main.hslToRgb(hue, 0.8f, 0.6f);
            
            tooltips.Add(new TooltipLine(Mod, "Upgrade", "Resonance Combo System (Four Seasons)")
            {
                OverrideColor = titleColor
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Max Resonance increased to 40")
            {
                OverrideColor = new Color(220, 200, 255)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Includes all previous tier bonuses")
            {
                OverrideColor = new Color(200, 180, 255)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Burst", "Consume 30 Resonance: Seasonal elemental burst")
            {
                OverrideColor = GetCurrentSeasonColor()
            });
            
            tooltips.Add(new TooltipLine(Mod, "BurstNote", "(Burst changes element based on current biome)")
            {
                OverrideColor = new Color(180, 180, 180)
            });
            
            // Show current stacks if equipped
            if (resonancePlayer.hasVivaldisTempoMaster)
            {
                bool canBurst = resonancePlayer.resonanceStacks >= 30 && !resonancePlayer.IsBurstOnCooldown;
                tooltips.Add(new TooltipLine(Mod, "Stacks", $"Current Resonance: {resonancePlayer.resonanceStacks}/{resonancePlayer.maxResonance}")
                {
                    OverrideColor = canBurst ? Color.Yellow : Color.Lerp(Color.Gray, GetCurrentSeasonColor(), resonancePlayer.GetResonancePercent())
                });
                
                if (canBurst)
                {
                    tooltips.Add(new TooltipLine(Mod, "Ready", "✓ Seasonal Burst ready!")
                    {
                        OverrideColor = Color.Yellow
                    });
                }
            }
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The Four Seasons unite under the maestro's baton'")
            {
                OverrideColor = new Color(150, 150, 150)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<PermafrostCadenceSeal>(1)
                .AddIngredient<CycleOfSeasons>(1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    #endregion
}
