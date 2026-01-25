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
}
