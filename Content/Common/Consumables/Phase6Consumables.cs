using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Materials;
using MagnumOpus.Content.Fate.HarmonicCores;
using MagnumOpus.Content.Spring.Materials;
using MagnumOpus.Content.Summer.Materials;
using MagnumOpus.Content.Autumn.Materials;
using MagnumOpus.Content.Winter.Materials;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.LaCampanella.ResonanceEnergies;
using MagnumOpus.Content.EnigmaVariations.ResonanceEnergies;
using MagnumOpus.Content.SwanLake.ResonanceEnergies;
using MagnumOpus.Content.SwanLake.HarmonicCores;
using MagnumOpus.Content.Fate.ResonanceEnergies;

namespace MagnumOpus.Content.Common.Consumables
{
    // Fate theme colors for VFX
    internal static class FateColors
    {
        public static readonly Color White = new Color(255, 255, 255);
        public static readonly Color DarkPink = new Color(200, 80, 120);
        public static readonly Color Purple = new Color(140, 50, 160);
        public static readonly Color Crimson = new Color(180, 30, 60);
    }

    #region Harmonic Resonator Fragment - Crafting material for potions
    /// <summary>
    /// Phase 6: Base crafting material for MagnumOpus potions
    /// Dropped by all MagnumOpus enemies or crafted from resonant energies
    /// </summary>
    public class HarmonicResonatorFragment : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.maxStack = 999;
            Item.value = Item.sellPrice(silver: 50);
            Item.rare = ItemRarityID.Green;
            Item.material = true;
        }

        public override void AddRecipes()
        {
            // Can be crafted from any theme's materials
            CreateRecipe(5)
                .AddRecipeGroup("MagnumOpus:AnyResonantEnergy", 1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
    #endregion

    #region Minor Resonance Tonic - Basic potion
    /// <summary>
    /// Phase 6: Basic MagnumOpus potion - +10% damage for 8 minutes
    /// </summary>
    public class MinorResonanceTonic : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 24;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.consumable = true;
            Item.maxStack = 30;
            Item.value = Item.sellPrice(silver: 75);
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item3;
            Item.buffType = ModContent.BuffType<MinorResonanceBuff>();
            Item.buffTime = 60 * 60 * 8; // 8 minutes
        }

        public override void AddRecipes()
        {
            CreateRecipe(3)
                .AddIngredient<HarmonicResonatorFragment>(5)
                .AddIngredient(ItemID.BottledWater, 3)
                .AddIngredient(ItemID.Daybloom)
                .AddTile(TileID.Bottles)
                .Register();
        }
    }

    public class MinorResonanceBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.Regeneration;

        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Generic) += 0.10f;
            
            // Subtle harmonic glow
            if (Main.rand.NextBool(30))
            {
                Vector2 offset = Main.rand.NextVector2Circular(25f, 25f);
                CustomParticles.GenericFlare(player.Center + offset, new Color(200, 220, 255), 0.2f, 10);
            }
        }
    }
    #endregion

    #region Harmonic Elixir - Greater potion
    /// <summary>
    /// Phase 6: Greater MagnumOpus potion - +20% damage, +10% crit for 8 minutes
    /// </summary>
    public class HarmonicElixir : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 26;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.consumable = true;
            Item.maxStack = 30;
            Item.value = Item.sellPrice(gold: 1, silver: 50);
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.Item3;
            Item.buffType = ModContent.BuffType<HarmonicElixirBuff>();
            Item.buffTime = 60 * 60 * 8; // 8 minutes
        }

        public override void AddRecipes()
        {
            CreateRecipe(3)
                .AddIngredient<MinorResonanceTonic>(3)
                .AddIngredient<HarmonicResonatorFragment>(10)
                .AddIngredient(ItemID.BottledHoney, 3)
                .AddIngredient(ItemID.Moonglow)
                .AddIngredient(ItemID.Waterleaf)
                .AddTile(TileID.AlchemyTable)
                .Register();
        }
    }

    public class HarmonicElixirBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.ManaRegeneration;

        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Generic) += 0.20f;
            player.GetCritChance(DamageClass.Generic) += 10;
            
            // Harmonic resonance particles
            if (Main.rand.NextBool(20))
            {
                float angle = Main.GameUpdateCount * 0.05f;
                Vector2 pos = player.Center + angle.ToRotationVector2() * 30f;
                Color color = Color.Lerp(new Color(150, 180, 255), new Color(255, 200, 150), 
                    (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.5f + 0.5f);
                CustomParticles.GenericFlare(pos, color * 0.5f, 0.25f, 12);
            }
        }
    }
    #endregion

    #region Seasonal Draught - Season-enhancing potion
    /// <summary>
    /// Phase 6: Enhances current season bonus by +25% for 10 minutes
    /// </summary>
    public class SeasonalDraught : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 26;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.consumable = true;
            Item.maxStack = 30;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.LightPurple;
            Item.UseSound = SoundID.Item3;
            Item.buffType = ModContent.BuffType<SeasonalDraughtBuff>();
            Item.buffTime = 60 * 60 * 10; // 10 minutes
        }

        public override void AddRecipes()
        {
            CreateRecipe(2)
                .AddIngredient<HarmonicResonatorFragment>(15)
                .AddIngredient(ItemID.BottledHoney, 2)
                .AddIngredient(ItemID.Daybloom)
                .AddIngredient(ItemID.Fireblossom)
                .AddIngredient(ItemID.Shiverthorn)
                .AddIngredient(ItemID.Deathweed)
                .AddTile(TileID.AlchemyTable)
                .Register();
        }
    }

    public class SeasonalDraughtBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.Warmth;

        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // This will be checked by seasonal accessory code to provide +25% seasonal bonus
            player.GetModPlayer<SeasonalDraughtPlayer>().seasonalDraughtActive = true;
            
            // Season-colored particles
            if (Main.rand.NextBool(25))
            {
                Color seasonColor = GetCurrentSeasonColor();
                Vector2 offset = Main.rand.NextVector2Circular(30f, 30f);
                CustomParticles.GenericFlare(player.Center + offset, seasonColor * 0.6f, 0.25f, 15);
            }
        }

        private Color GetCurrentSeasonColor()
        {
            // Based on in-game time/date for season
            int dayOfYear = (int)(Main.time / 54000) % 4; // Simple 4-season cycle
            return dayOfYear switch
            {
                0 => new Color(255, 200, 220), // Spring pink
                1 => new Color(255, 180, 100), // Summer gold
                2 => new Color(200, 120, 80),  // Autumn brown
                _ => new Color(180, 220, 255)  // Winter ice
            };
        }
    }

    public class SeasonalDraughtPlayer : ModPlayer
    {
        public bool seasonalDraughtActive;

        public override void ResetEffects()
        {
            seasonalDraughtActive = false;
        }
    }
    #endregion

    #region Elixir of the Maestro - Master potion
    /// <summary>
    /// Phase 6: Master MagnumOpus potion - +35% damage, +20% crit, +15% speed for 10 minutes
    /// </summary>
    public class ElixirOfTheMaestro : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 28;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.consumable = true;
            Item.maxStack = 30;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Cyan;
            Item.UseSound = SoundID.Item3;
            Item.buffType = ModContent.BuffType<MaestroBuff>();
            Item.buffTime = 60 * 60 * 10; // 10 minutes
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<HarmonicElixir>(2)
                .AddIngredient<SeasonalDraught>(1)
                .AddIngredient<HarmonicResonatorFragment>(25)
                .AddIngredient(ItemID.SoulofLight, 5)
                .AddIngredient(ItemID.SoulofNight, 5)
                .AddTile(TileID.AlchemyTable)
                .Register();
        }
    }

    public class MaestroBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.Wrath;

        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Generic) += 0.35f;
            player.GetCritChance(DamageClass.Generic) += 20;
            player.GetAttackSpeed(DamageClass.Generic) += 0.15f;
            
            // Maestro's aura - musical notes and conductor's baton trails
            if (Main.rand.NextBool(12))
            {
                float angle = Main.GameUpdateCount * 0.03f + Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = 35f + Main.rand.NextFloat(15f);
                Vector2 pos = player.Center + angle.ToRotationVector2() * radius;
                
                // Rainbow musical glow
                float hue = (Main.GameUpdateCount * 0.01f + Main.rand.NextFloat()) % 1f;
                Color rainbowColor = Main.hslToRgb(hue, 0.9f, 0.7f);
                
                CustomParticles.GenericFlare(pos, rainbowColor * 0.6f, 0.3f, 15);
            }
            
            // Occasional music note
            if (Main.rand.NextBool(30))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(40f, 40f);
                Vector2 noteVel = new Vector2(0, -Main.rand.NextFloat(1f, 2f));
                ThemedParticles.MoonlightMusicNotes(notePos, 1, 10f);
            }
        }
    }
    #endregion

    #region Conductor's Insight - Informational item
    /// <summary>
    /// Phase 6: Accessory that displays detailed combat analysis
    /// Shows DPS, damage breakdown, and combo information
    /// </summary>
    public class ConductorsInsight : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ItemRarityID.Yellow;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<ConductorsInsightPlayer>();
            modPlayer.insightActive = true;
            
            // Subtle informational aura
            if (!hideVisual && Main.rand.NextBool(40))
            {
                float angle = Main.GameUpdateCount * 0.02f;
                Vector2 pos = player.Center + angle.ToRotationVector2() * 25f;
                CustomParticles.GenericFlare(pos, new Color(255, 230, 150) * 0.4f, 0.2f, 10);
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<HarmonicResonatorFragment>(30)
                .AddIngredient(ItemID.CrystalBall)
                .AddIngredient(ItemID.SoulofSight, 10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    public class ConductorsInsightPlayer : ModPlayer
    {
        public bool insightActive;
        public int totalDamageThisSecond;
        public int damageDisplayTimer;
        public float currentDPS;

        public override void ResetEffects()
        {
            insightActive = false;
        }

        public override void PostUpdate()
        {
            if (!insightActive) return;
            
            damageDisplayTimer++;
            if (damageDisplayTimer >= 60)
            {
                currentDPS = totalDamageThisSecond;
                totalDamageThisSecond = 0;
                damageDisplayTimer = 0;
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (insightActive) totalDamageThisSecond += damageDone;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (insightActive && proj.owner == Player.whoAmI) totalDamageThisSecond += damageDone;
        }
    }
    #endregion

    #region Fate's Blessing - Permanent stat upgrade from Fate boss
    /// <summary>
    /// Phase 6: Permanent +5% all damage, dropped once from Fate boss
    /// </summary>
    public class FatesBlessing : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.consumable = true;
            Item.maxStack = 1;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item119;
        }

        public override bool? UseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<FatesBlessingPlayer>();
            if (!modPlayer.hasFatesBlessing)
            {
                modPlayer.hasFatesBlessing = true;
                
                // Cosmic blessing VFX
                for (int i = 0; i < 20; i++)
                {
                    float angle = MathHelper.TwoPi * i / 20f;
                    Vector2 pos = player.Center + angle.ToRotationVector2() * 60f;
                    CustomParticles.GenericFlare(pos, FateColors.DarkPink, 0.5f, 25);
                    CustomParticles.Glyph(pos, FateColors.Purple, 0.4f, -1);
                }
                
                CustomParticles.HaloRing(player.Center, FateColors.White, 0.8f, 30);
                
                Main.NewText("The cosmos bestows its blessing upon you!", FateColors.DarkPink);
                return true;
            }
            
            Main.NewText("You have already received Fate's Blessing.", Color.Gray);
            return false;
        }
    }

    public class FatesBlessingPlayer : ModPlayer
    {
        public bool hasFatesBlessing;

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (hasFatesBlessing)
            {
                modifiers.FinalDamage *= 1.05f;
            }
        }

        public override void SaveData(Terraria.ModLoader.IO.TagCompound tag)
        {
            tag["hasFatesBlessing"] = hasFatesBlessing;
        }

        public override void LoadData(Terraria.ModLoader.IO.TagCompound tag)
        {
            hasFatesBlessing = tag.GetBool("hasFatesBlessing");
        }
    }
    #endregion

    #region Coda's Echo - Permanent upgrade after obtaining ultimate accessory
    /// <summary>
    /// Phase 6: Permanent +10% all damage, 25% drop from Fate boss after having Coda of Absolute Harmony
    /// </summary>
    public class CodasEcho : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.consumable = true;
            Item.maxStack = 1;
            Item.value = Item.sellPrice(platinum: 1);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item119;
        }

        public override bool? UseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<CodasEchoPlayer>();
            if (!modPlayer.hasCodasEcho)
            {
                modPlayer.hasCodasEcho = true;
                
                // Ultimate cosmic VFX
                for (int ring = 0; ring < 5; ring++)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 12f + ring * 0.2f;
                        float radius = 40f + ring * 20f;
                        Vector2 pos = player.Center + angle.ToRotationVector2() * radius;
                        
                        Color color = Color.Lerp(FateColors.White, FateColors.Crimson, ring / 5f);
                        CustomParticles.GenericFlare(pos, color, 0.4f + ring * 0.1f, 25 + ring * 5);
                    }
                }
                
                // Glyphs and halos
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 glyphPos = player.Center + angle.ToRotationVector2() * 80f;
                    CustomParticles.Glyph(glyphPos, FateColors.Purple, 0.5f, -1);
                }
                
                CustomParticles.HaloRing(player.Center, FateColors.White, 1.2f, 35);
                CustomParticles.HaloRing(player.Center, FateColors.DarkPink, 1.0f, 30);
                CustomParticles.HaloRing(player.Center, FateColors.Crimson, 0.8f, 25);
                
                Main.NewText("The Coda's eternal echo resonates within your soul!", FateColors.White);
                return true;
            }
            
            Main.NewText("The Coda already echoes within you.", Color.Gray);
            return false;
        }
    }

    public class CodasEchoPlayer : ModPlayer
    {
        public bool hasCodasEcho;

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (hasCodasEcho)
            {
                modifiers.FinalDamage *= 1.10f;
            }
        }

        public override void SaveData(Terraria.ModLoader.IO.TagCompound tag)
        {
            tag["hasCodasEcho"] = hasCodasEcho;
        }

        public override void LoadData(Terraria.ModLoader.IO.TagCompound tag)
        {
            hasCodasEcho = tag.GetBool("hasCodasEcho");
        }
    }
    #endregion

    #region Fate's Cosmic Brew - Ultimate consumable (once per day)
    /// <summary>
    /// Phase 6: Ultimate buff - All damage +50%, +30% crit, +25% speed for 15 minutes
    /// Can only be used once per in-game day
    /// </summary>
    public class FatesCosmicBrew : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 30;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.consumable = true;
            Item.maxStack = 10;
            Item.value = Item.sellPrice(gold: 20);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item3;
        }

        public override bool CanUseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<CosmicBrewPlayer>();
            if (modPlayer.cosmicBrewCooldown > 0)
            {
                Main.NewText("The cosmic energies haven't realigned yet...", Color.Gray);
                return false;
            }
            return true;
        }

        public override bool? UseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<CosmicBrewPlayer>();
            modPlayer.cosmicBrewCooldown = 60 * 60 * 24; // One in-game day (24 minutes real time)
            
            player.AddBuff(ModContent.BuffType<CosmicBrewBuff>(), 60 * 60 * 15); // 15 minutes
            
            // Cosmic transformation VFX
            for (int i = 0; i < 30; i++)
            {
                float angle = MathHelper.TwoPi * i / 30f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                
                Color color = Color.Lerp(FateColors.DarkPink, FateColors.Crimson, Main.rand.NextFloat());
                var particle = new GenericGlowParticle(player.Center, vel, color * 0.8f, 0.4f, 25, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }
            
            CustomParticles.GlyphBurst(player.Center, FateColors.Purple, 12, 6f);
            
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ElixirOfTheMaestro>(2)
                .AddIngredient<HarmonicCoreOfFate>(5)
                .AddIngredient(ItemID.FragmentSolar, 5)
                .AddIngredient(ItemID.FragmentVortex, 5)
                .AddIngredient(ItemID.FragmentNebula, 5)
                .AddIngredient(ItemID.FragmentStardust, 5)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class CosmicBrewBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.NebulaUpLife3;

        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Generic) += 0.50f;
            player.GetCritChance(DamageClass.Generic) += 30;
            player.GetAttackSpeed(DamageClass.Generic) += 0.25f;
            
            // Cosmic aura VFX
            if (Main.rand.NextBool(8))
            {
                float angle = Main.GameUpdateCount * 0.04f + Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = 40f + Main.rand.NextFloat(20f);
                Vector2 pos = player.Center + angle.ToRotationVector2() * radius;
                
                Color color = Color.Lerp(FateColors.DarkPink, FateColors.Crimson, Main.rand.NextFloat());
                CustomParticles.GenericFlare(pos, color * 0.6f, 0.35f, 18);
            }
            
            // Orbiting glyphs
            if (Main.rand.NextBool(25))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 pos = player.Center + angle.ToRotationVector2() * 50f;
                CustomParticles.Glyph(pos, FateColors.Purple * 0.5f, 0.35f, -1);
            }
            
            // Star sparkles
            if (Main.rand.NextBool(15))
            {
                Vector2 starPos = player.Center + Main.rand.NextVector2Circular(45f, 45f);
                CustomParticles.GenericFlare(starPos, FateColors.White * 0.7f, 0.25f, 12);
            }
        }
    }

    public class CosmicBrewPlayer : ModPlayer
    {
        public int cosmicBrewCooldown;

        public override void PostUpdate()
        {
            if (cosmicBrewCooldown > 0)
                cosmicBrewCooldown--;
        }

        public override void SaveData(Terraria.ModLoader.IO.TagCompound tag)
        {
            tag["cosmicBrewCooldown"] = cosmicBrewCooldown;
        }

        public override void LoadData(Terraria.ModLoader.IO.TagCompound tag)
        {
            cosmicBrewCooldown = tag.GetInt("cosmicBrewCooldown");
        }
    }
    #endregion

    #region Crystallized Harmony - Permanent Health Upgrade
    /// <summary>
    /// Phase 6: Consumable item that permanently transforms one health heart into a rainbow-shimmering version.
    /// Can be crafted and consumed multiple times to convert all 20 hearts one by one.
    /// Each use grants +5 max health permanently (up to 100 bonus, 20 uses total).
    /// </summary>
    public class CrystallizedHarmony : ModItem
    {
        public const int MaxUses = 20;
        public const int HealthPerUse = 5;

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 45;
            Item.useTime = 45;
            Item.consumable = true;
            Item.maxStack = 20;
            Item.value = Item.sellPrice(gold: 25);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item119;
        }

        public override bool CanUseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<CrystallizedHarmonyPlayer>();
            if (modPlayer.crystallizedHarmonyUses >= MaxUses)
            {
                Main.NewText("Your life force is already fully attuned to the harmonics.", new Color(255, 200, 220));
                return false;
            }
            return true;
        }

        public override bool? UseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<CrystallizedHarmonyPlayer>();
            
            modPlayer.crystallizedHarmonyUses++;
            int currentUses = modPlayer.crystallizedHarmonyUses;
            
            // Spectacular rainbow transformation VFX
            SpawnTransformationVFX(player, currentUses);
            
            // Immediately apply health bonus
            player.statLifeMax2 += HealthPerUse;
            player.statLife = Math.Min(player.statLife + HealthPerUse, player.statLifeMax2);
            
            // Healing effect
            player.HealEffect(HealthPerUse, true);
            
            Main.NewText($"Heart #{currentUses} resonates with eternal harmony! (+{HealthPerUse} max health)", 
                Color.Lerp(new Color(255, 150, 200), new Color(200, 255, 255), (float)currentUses / MaxUses));
            
            if (currentUses >= MaxUses)
            {
                Main.NewText("All hearts now shimmer with the music of existence!", new Color(255, 255, 200));
            }
            
            return true;
        }

        private void SpawnTransformationVFX(Player player, int heartNumber)
        {
            // Rainbow color cycling based on heart number
            float hueBase = (float)heartNumber / MaxUses;
            
            // Central burst
            CustomParticles.GenericFlare(player.Center, Color.White, 1.2f, 30);
            
            // Rainbow spiral burst
            for (int i = 0; i < 24; i++)
            {
                float angle = MathHelper.TwoPi * i / 24f;
                float hue = (hueBase + (float)i / 24f) % 1f;
                Color rainbowColor = Main.hslToRgb(hue, 1f, 0.65f);
                
                Vector2 offset = angle.ToRotationVector2() * 50f;
                CustomParticles.GenericFlare(player.Center + offset, rainbowColor, 0.5f, 25);
                
                // Outward particles
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                var particle = new GenericGlowParticle(player.Center, vel, rainbowColor * 0.8f, 0.35f, 30, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }
            
            // Seasonal color halos (representing all four seasons)
            Color[] seasonalColors = {
                new Color(255, 183, 197), // Spring pink
                new Color(255, 140, 40),  // Summer orange
                new Color(139, 90, 43),   // Autumn brown
                new Color(135, 206, 250)  // Winter blue
            };
            
            for (int s = 0; s < 4; s++)
            {
                float delay = s * 0.1f;
                CustomParticles.HaloRing(player.Center, seasonalColors[s], 0.6f + s * 0.15f, 20 + s * 5);
            }
            
            // Music notes spiral
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f + Main.GameUpdateCount * 0.05f;
                Vector2 notePos = player.Center + angle.ToRotationVector2() * 35f;
                ThemedParticles.MusicNote(notePos, angle.ToRotationVector2() * 2f, Main.hslToRgb((hueBase + i * 0.1f) % 1f, 1f, 0.7f), 0.4f, 35);
            }
            
            // Theme glyphs
            CustomParticles.GlyphBurst(player.Center, new Color(255, 200, 255), 6, 4f);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<BlossomEssence>(25)
                .AddIngredient<SolarEssence>(25)
                .AddIngredient<DecayEssence>(25)
                .AddIngredient<FrostEssence>(25)
                .AddIngredient<MoonlightsResonantEnergy>(10)
                .AddIngredient<EroicasResonantEnergy>(10)
                .AddIngredient<LaCampanellaResonantEnergy>(10)
                .AddIngredient<EnigmaResonantEnergy>(10)
                .AddIngredient<SwansResonanceEnergy>(10)
                .AddIngredient<FateResonantEnergy>(10)
                .AddIngredient(ItemID.LunarBar, 30)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }

        public override void PostUpdate()
        {
            // Rainbow prismatic glow when on ground
            float hue = (Main.GameUpdateCount * 0.02f) % 1f;
            Vector3 light = Main.hslToRgb(hue, 0.8f, 0.5f).ToVector3();
            Lighting.AddLight(Item.Center, light * 0.6f);
            
            if (Main.rand.NextBool(8))
            {
                float sparkleHue = Main.rand.NextFloat();
                Color sparkleColor = Main.hslToRgb(sparkleHue, 1f, 0.75f);
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.RainbowMk2, 0f, -0.5f, 100, sparkleColor, 0.6f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
        }
    }

    public class CrystallizedHarmonyPlayer : ModPlayer
    {
        public int crystallizedHarmonyUses;

        public override void ModifyMaxStats(out StatModifier health, out StatModifier mana)
        {
            health = StatModifier.Default;
            mana = StatModifier.Default;
            
            // Add permanent health bonus
            health = health.CombineWith(new StatModifier(1f, 1f, crystallizedHarmonyUses * CrystallizedHarmony.HealthPerUse, 0f));
        }

        public override void PostUpdate()
        {
            // Rainbow shimmer effect on hearts when fully upgraded
            if (crystallizedHarmonyUses >= CrystallizedHarmony.MaxUses && Main.rand.NextBool(60))
            {
                // Subtle ambient rainbow sparkle around player
                Vector2 sparklePos = Player.Center + Main.rand.NextVector2Circular(30f, 40f);
                float hue = Main.rand.NextFloat();
                CustomParticles.GenericFlare(sparklePos, Main.hslToRgb(hue, 0.9f, 0.7f) * 0.4f, 0.2f, 12);
            }
        }

        public override void SaveData(Terraria.ModLoader.IO.TagCompound tag)
        {
            tag["crystallizedHarmonyUses"] = crystallizedHarmonyUses;
        }

        public override void LoadData(Terraria.ModLoader.IO.TagCompound tag)
        {
            crystallizedHarmonyUses = tag.GetInt("crystallizedHarmonyUses");
        }
    }
    #endregion

    #region Arcane Harmonic Prism - Permanent Mana Upgrade
    /// <summary>
    /// Phase 6: Consumable item that permanently transforms one mana star into a rainbow-shimmering version.
    /// Can be crafted and consumed multiple times to convert all 10 mana stars one by one.
    /// Each use grants +20 max mana permanently (up to 200 bonus, 10 uses total).
    /// </summary>
    public class ArcaneHarmonicPrism : ModItem
    {
        public const int MaxUses = 10;
        public const int ManaPerUse = 20;

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 45;
            Item.useTime = 45;
            Item.consumable = true;
            Item.maxStack = 10;
            Item.value = Item.sellPrice(gold: 25);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item119;
        }

        public override bool CanUseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<ArcaneHarmonicPrismPlayer>();
            if (modPlayer.arcaneHarmonicPrismUses >= MaxUses)
            {
                Main.NewText("Your magical essence is already fully attuned to the arcane harmonics.", new Color(150, 200, 255));
                return false;
            }
            return true;
        }

        public override bool? UseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<ArcaneHarmonicPrismPlayer>();
            
            modPlayer.arcaneHarmonicPrismUses++;
            int currentUses = modPlayer.arcaneHarmonicPrismUses;
            
            // Spectacular arcane transformation VFX
            SpawnArcaneTransformationVFX(player, currentUses);
            
            // Immediately apply mana bonus
            player.statManaMax2 += ManaPerUse;
            player.statMana = Math.Min(player.statMana + ManaPerUse, player.statManaMax2);
            
            // Mana restore effect
            player.ManaEffect(ManaPerUse);
            
            Main.NewText($"Mana Star #{currentUses} resonates with arcane harmony! (+{ManaPerUse} max mana)", 
                Color.Lerp(new Color(100, 150, 255), new Color(200, 150, 255), (float)currentUses / MaxUses));
            
            if (currentUses >= MaxUses)
            {
                Main.NewText("All mana stars now shimmer with the arcane music of the cosmos!", new Color(200, 220, 255));
            }
            
            return true;
        }

        private void SpawnArcaneTransformationVFX(Player player, int starNumber)
        {
            // Arcane blue-violet color cycling based on star number
            float hueBase = 0.6f + (float)starNumber / MaxUses * 0.2f; // Blue to violet range
            
            // Central burst - arcane white-blue
            CustomParticles.GenericFlare(player.Center, new Color(200, 220, 255), 1.2f, 30);
            
            // Arcane spiral burst
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                float hue = (hueBase + (float)i / 20f * 0.15f) % 1f;
                Color arcaneColor = Main.hslToRgb(hue, 0.8f, 0.65f);
                
                Vector2 offset = angle.ToRotationVector2() * 45f;
                CustomParticles.GenericFlare(player.Center + offset, arcaneColor, 0.45f, 25);
                
                // Outward particles
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                var particle = new GenericGlowParticle(player.Center, vel, arcaneColor * 0.8f, 0.3f, 28, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }
            
            // Theme color halos (representing magical themes)
            Color[] themeColors = {
                new Color(138, 43, 226),  // Moonlight purple
                new Color(255, 140, 40),  // La Campanella orange
                new Color(140, 60, 200),  // Enigma purple
                new Color(200, 80, 120),  // Fate pink
                Color.White               // Swan Lake white
            };
            
            for (int t = 0; t < 5; t++)
            {
                CustomParticles.HaloRing(player.Center, themeColors[t], 0.5f + t * 0.12f, 18 + t * 4);
            }
            
            // Arcane glyphs orbit
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 glyphPos = player.Center + angle.ToRotationVector2() * 55f;
                CustomParticles.Glyph(glyphPos, new Color(150, 180, 255), 0.45f, -1);
            }
            
            // Music notes with arcane colors
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f + Main.GameUpdateCount * 0.04f;
                Vector2 notePos = player.Center + angle.ToRotationVector2() * 30f;
                Color noteColor = Color.Lerp(new Color(100, 150, 255), new Color(200, 150, 255), (float)i / 6f);
                ThemedParticles.MusicNote(notePos, angle.ToRotationVector2() * 1.5f, noteColor, 0.35f, 30);
            }
            
            // Star sparkles for mana theme
            for (int i = 0; i < 12; i++)
            {
                Vector2 starPos = player.Center + Main.rand.NextVector2Circular(50f, 50f);
                CustomParticles.GenericFlare(starPos, new Color(200, 220, 255) * 0.7f, 0.3f, 20);
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<BlossomEssence>(25)
                .AddIngredient<SolarEssence>(25)
                .AddIngredient<DecayEssence>(25)
                .AddIngredient<FrostEssence>(25)
                .AddIngredient<MoonlightsResonantEnergy>(10)
                .AddIngredient<EroicasResonantEnergy>(10)
                .AddIngredient<LaCampanellaResonantEnergy>(10)
                .AddIngredient<EnigmaResonantEnergy>(10)
                .AddIngredient<SwansResonanceEnergy>(10)
                .AddIngredient<FateResonantEnergy>(10)
                .AddIngredient(ItemID.LunarBar, 30)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }

        public override void PostUpdate()
        {
            // Arcane blue-violet prismatic glow when on ground
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.2f + 0.6f;
            Lighting.AddLight(Item.Center, 0.4f * pulse, 0.5f * pulse, 0.8f * pulse);
            
            if (Main.rand.NextBool(8))
            {
                Color arcaneColor = Color.Lerp(new Color(100, 150, 255), new Color(180, 130, 255), Main.rand.NextFloat());
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.BlueTorch, 0f, -0.5f, 100, arcaneColor, 0.6f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
        }
    }

    public class ArcaneHarmonicPrismPlayer : ModPlayer
    {
        public int arcaneHarmonicPrismUses;

        public override void ModifyMaxStats(out StatModifier health, out StatModifier mana)
        {
            health = StatModifier.Default;
            mana = StatModifier.Default;
            
            // Add permanent mana bonus
            mana = mana.CombineWith(new StatModifier(1f, 1f, arcaneHarmonicPrismUses * ArcaneHarmonicPrism.ManaPerUse, 0f));
        }

        public override void PostUpdate()
        {
            // Arcane shimmer effect on mana stars when fully upgraded
            if (arcaneHarmonicPrismUses >= ArcaneHarmonicPrism.MaxUses && Main.rand.NextBool(60))
            {
                // Subtle ambient arcane sparkle around player
                Vector2 sparklePos = Player.Center + Main.rand.NextVector2Circular(30f, 40f);
                Color arcaneColor = Color.Lerp(new Color(100, 150, 255), new Color(180, 130, 255), Main.rand.NextFloat());
                CustomParticles.GenericFlare(sparklePos, arcaneColor * 0.4f, 0.2f, 12);
            }
        }

        public override void SaveData(Terraria.ModLoader.IO.TagCompound tag)
        {
            tag["arcaneHarmonicPrismUses"] = arcaneHarmonicPrismUses;
        }

        public override void LoadData(Terraria.ModLoader.IO.TagCompound tag)
        {
            arcaneHarmonicPrismUses = tag.GetInt("arcaneHarmonicPrismUses");
        }
    }
    #endregion
}
