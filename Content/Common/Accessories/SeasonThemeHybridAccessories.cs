using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.MoonlightSonata.Accessories;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.LaCampanella.Accessories;
using MagnumOpus.Content.LaCampanella.HarmonicCores;
using MagnumOpus.Content.EnigmaVariations.Accessories;
using MagnumOpus.Content.EnigmaVariations.ResonanceEnergies;
using MagnumOpus.Content.Spring.Accessories;
using MagnumOpus.Content.Spring.Materials;
using MagnumOpus.Content.Summer.Accessories;
using MagnumOpus.Content.Summer.Materials;
using MagnumOpus.Content.Winter.Accessories;
using MagnumOpus.Content.Winter.Materials;

namespace MagnumOpus.Content.Common.Accessories
{
    #region Spring's Moonlit Garden - Spring + Moonlight Sonata
    /// <summary>
    /// Phase 5 Season-Theme Hybrid: Bloom Crest + Sonata's Embrace
    /// The moon's gentle light nurtures nocturnal blossoms in an eternal garden
    /// </summary>
    public class SpringsMoonlitGarden : ModItem
    {
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color MoonlightPurple = new Color(138, 43, 226);
        private static readonly Color MoonlightSilver = new Color(220, 220, 235);
        private static readonly Color NightBlossomBlue = new Color(160, 140, 220);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 1, gold: 50);
            Item.rare = ModContent.RarityType<MoonlightSonataRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<SpringsMoonlitGardenPlayer>();
            modPlayer.moonlitGardenEquipped = true;
            
            bool isNight = !Main.dayTime;
            
            // === SPRING BONUSES (Bloom Crest) ===
            player.lifeRegen += 6;
            player.statDefense += 8;
            player.endurance += 0.06f;
            player.thorns = 0.6f;
            
            // === MOONLIGHT BONUSES (Sonata's Embrace) ===
            if (isNight)
            {
                player.GetDamage(DamageClass.Generic) += 0.22f;
                player.GetCritChance(DamageClass.Generic) += 18;
                player.statDefense += 12;
                player.moveSpeed += 0.15f;
            }
            else
            {
                player.GetDamage(DamageClass.Generic) += 0.10f;
                player.GetCritChance(DamageClass.Generic) += 8;
            }
            
            player.GetDamage(DamageClass.Magic) += 0.12f;
            player.manaRegen += 4;
            
            // === HYBRID BONUS: Moonlit Garden ===
            // Enhanced life regen at night
            if (isNight)
            {
                player.lifeRegen += 8;
            }
            
            // Ambient VFX
            if (!hideVisual)
            {
                // Night blossoms - moon-kissed flowers
                if (isNight)
                {
                    // Orbital moonlit petals
                    if (Main.GameUpdateCount % 10 == 0)
                    {
                        float baseAngle = Main.GameUpdateCount * 0.015f;
                        for (int i = 0; i < 5; i++)
                        {
                            float angle = baseAngle + MathHelper.TwoPi * i / 5f;
                            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f + i) * 5f;
                            Vector2 pos = player.Center + angle.ToRotationVector2() * (42f + pulse);
                            
                            Color petalColor = Color.Lerp(SpringPink, NightBlossomBlue, (float)i / 5f);
                            CustomParticles.GenericFlare(pos, petalColor, 0.28f, 14);
                        }
                    }
                    
                    // Floating moon dust among flowers
                    if (Main.rand.NextBool(8))
                    {
                        Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(45f, 45f);
                        Color dustColor = Main.rand.NextBool() ? MoonlightSilver : NightBlossomBlue;
                        CustomParticles.GenericGlow(dustPos, new Vector2(0, -0.8f), dustColor * 0.65f, 0.22f, 24, true);
                    }
                    
                    // Sparkles like moonlight on dew
                    if (Main.rand.NextBool(12))
                    {
                        Vector2 sparklePos = player.Center + Main.rand.NextVector2Circular(40f, 40f);
                        var sparkle = new SparkleParticle(sparklePos, new Vector2(0, -0.5f), MoonlightSilver, 0.25f, 18);
                        MagnumParticleHandler.SpawnParticle(sparkle);
                    }
                }
                else
                {
                    // Daytime - gentle spring essence
                    if (Main.rand.NextBool(12))
                    {
                        Vector2 pos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                        CustomParticles.GenericGlow(pos, new Vector2(0, -1f), SpringPink * 0.5f, 0.22f, 20, true);
                    }
                }
                
                // Garden light
                Color lightColor = isNight ? Color.Lerp(MoonlightPurple, NightBlossomBlue, 0.5f) : SpringPink;
                float lightIntensity = isNight ? 0.5f : 0.35f;
                Lighting.AddLight(player.Center, lightColor.ToVector3() * lightIntensity);
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<BloomCrest>()
                .AddIngredient<SonatasEmbrace>()
                .AddIngredient<VernalBar>(15)
                .AddIngredient<ResonantCoreOfMoonlightSonata>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color SpringPink = new Color(255, 183, 197);
            Color MoonlightPurple = new Color(138, 43, 226);

            tooltips.Add(new TooltipLine(Mod, "Hybrid", "Bloom Crest + Sonata's Embrace")
            {
                OverrideColor = Color.Lerp(SpringPink, MoonlightPurple, 0.5f)
            });
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+6 life regen, +8 defense, +6% damage reduction, 60% thorns")
            {
                OverrideColor = SpringPink
            });
            tooltips.Add(new TooltipLine(Mod, "Effect2", "At night: +22% damage, +18% crit, +12 defense, +15% move speed")
            {
                OverrideColor = MoonlightPurple
            });
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+12% magic damage, +4 mana regen, +8 life regen at night")
            {
                OverrideColor = Color.Lerp(SpringPink, MoonlightPurple, 0.3f)
            });
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Hits can inflict Moonstruck and heal you, thorns poison enemies")
            {
                OverrideColor = Color.Lerp(SpringPink, MoonlightPurple, 0.7f)
            });
        }
    }

    public class SpringsMoonlitGardenPlayer : ModPlayer
    {
        public bool moonlitGardenEquipped;
        private int healProcCooldown;

        public override void ResetEffects()
        {
            moonlitGardenEquipped = false;
        }

        public override void PostUpdate()
        {
            if (healProcCooldown > 0) healProcCooldown--;
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            HandleGardenHit(target, damageDone);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.owner == Player.whoAmI)
                HandleGardenHit(target, damageDone);
        }

        private void HandleGardenHit(NPC target, int damageDone)
        {
            if (!moonlitGardenEquipped) return;
            
            bool isNight = !Main.dayTime;
            
            // Moonstruck at night (12%)
            if (isNight && Main.rand.NextFloat() < 0.12f)
            {
                target.AddBuff(BuffID.Confused, 150);
                CustomParticles.GenericFlare(target.Center, new Color(138, 43, 226), 0.5f, 14);
            }
            
            // Bloom healing (8% chance, enhanced at night)
            if (healProcCooldown <= 0 && Main.rand.NextFloat() < (isNight ? 0.10f : 0.06f))
            {
                healProcCooldown = 45;
                int healAmount = isNight ? 8 : 5;
                Player.Heal(healAmount);
                
                CustomParticles.GenericFlare(Player.Center, new Color(255, 183, 197), 0.5f, 16);
            }
            
            // Thorns on hit
            if (Main.rand.NextFloat() < 0.08f)
            {
                target.AddBuff(BuffID.Poisoned, 180);
            }
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            if (!moonlitGardenEquipped) return;
            
            bool isNight = !Main.dayTime;
            
            // Petal barrier when hurt at night
            if (isNight)
            {
                // Burst of moonlit petals
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 pos = Player.Center + angle.ToRotationVector2() * 25f;
                    Color petalColor = Color.Lerp(new Color(255, 183, 197), new Color(160, 140, 220), Main.rand.NextFloat());
                    CustomParticles.GenericGlow(pos, angle.ToRotationVector2() * 2f, petalColor, 0.35f, 20, true);
                }
            }
        }
    }
    #endregion

    #region Summer's Infernal Peak - Summer + La Campanella
    /// <summary>
    /// Phase 5 Season-Theme Hybrid: Radiant Crown + Infernal Virtuoso
    /// The scorching summer sun meets the flames of virtuosic passion
    /// </summary>
    public class SummersInfernalPeak : ModItem
    {
        private static readonly Color SummerGold = new Color(255, 180, 50);
        private static readonly Color CampanellaOrange = new Color(255, 140, 40);
        private static readonly Color SolarFlare = new Color(255, 220, 100);
        private static readonly Color InfernalRed = new Color(255, 80, 40);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 1, gold: 50);
            Item.rare = ModContent.RarityType<LaCampanellaRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<SummersInfernalPeakPlayer>();
            modPlayer.infernalPeakEquipped = true;
            
            // === SUMMER BONUSES (Radiant Crown) ===
            player.GetDamage(DamageClass.Generic) += 0.16f;
            player.GetCritChance(DamageClass.Generic) += 10;
            player.statDefense += 8;
            player.magmaStone = true;
            
            // === LA CAMPANELLA BONUSES (Infernal Virtuoso) ===
            player.GetDamage(DamageClass.Magic) += 0.22f;
            player.GetCritChance(DamageClass.Magic) += 14;
            player.manaCost -= 0.15f;
            player.manaRegen += 4;
            
            // Fire immunity
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.lavaImmune = true;
            
            // === HYBRID BONUS: Solar Flare ===
            // Fire damage bonus during day
            if (Main.dayTime)
            {
                player.GetDamage(DamageClass.Generic) += 0.08f;
            }
            
            // Ambient VFX
            if (!hideVisual)
            {
                // Orbital flame crown
                if (Main.GameUpdateCount % 8 == 0)
                {
                    float baseAngle = Main.GameUpdateCount * 0.02f;
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = baseAngle + MathHelper.TwoPi * i / 6f;
                        float flicker = Main.rand.NextFloat(0.8f, 1.2f);
                        Vector2 pos = player.Center + angle.ToRotationVector2() * (45f * flicker);
                        
                        Color flameColor = Color.Lerp(SummerGold, CampanellaOrange, (float)i / 6f);
                        CustomParticles.GenericFlare(pos, flameColor, 0.32f * flicker, 12);
                    }
                }
                
                // Rising heat distortion particles
                if (Main.rand.NextBool(5))
                {
                    Vector2 heatPos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                    Color heatColor = Main.rand.NextBool() ? SummerGold : CampanellaOrange;
                    var heat = new GenericGlowParticle(heatPos, new Vector2(0, -2f), heatColor * 0.6f, 0.28f, 22, true);
                    MagnumParticleHandler.SpawnParticle(heat);
                }
                
                // Solar flare sparks during day
                if (Main.dayTime && Main.rand.NextBool(10))
                {
                    Vector2 sparkPos = player.Center + Main.rand.NextVector2Circular(40f, 40f);
                    CustomParticles.GenericFlare(sparkPos, SolarFlare, 0.35f, 10);
                }
                
                // Smoke tendrils
                if (Main.rand.NextBool(15))
                {
                    Vector2 smokePos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                    var smoke = new HeavySmokeParticle(smokePos, new Vector2(0, -1f), 
                        Color.DarkGray, 25, 0.25f, 0.4f, 0.015f, false);
                    MagnumParticleHandler.SpawnParticle(smoke);
                }
                
                // Infernal glow
                float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.2f + 0.8f;
                Color lightColor = Color.Lerp(SummerGold, CampanellaOrange, 0.5f);
                Lighting.AddLight(player.Center, lightColor.ToVector3() * pulse * 0.6f);
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<RadiantCrown>()
                .AddIngredient<InfernalVirtuoso>()
                .AddIngredient<SolsticeBar>(15)
                .AddIngredient<ResonantCoreOfLaCampanella>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color SummerGold = new Color(255, 180, 50);
            Color CampanellaOrange = new Color(255, 140, 40);

            tooltips.Add(new TooltipLine(Mod, "Hybrid", "Radiant Crown + Infernal Virtuoso")
            {
                OverrideColor = Color.Lerp(SummerGold, CampanellaOrange, 0.5f)
            });
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+16% damage, +10% crit, +8 defense, attacks inflict fire")
            {
                OverrideColor = SummerGold
            });
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+22% magic damage, +14% magic crit, -15% mana cost, +4 mana regen")
            {
                OverrideColor = CampanellaOrange
            });
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Immunity to On Fire, Burning, and lava")
            {
                OverrideColor = Color.Lerp(SummerGold, CampanellaOrange, 0.3f)
            });
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Bell Chime stuns enemies with fire AOE, Solar Burst during daytime")
            {
                OverrideColor = Color.Lerp(SummerGold, CampanellaOrange, 0.7f)
            });
        }
    }

    public class SummersInfernalPeakPlayer : ModPlayer
    {
        public bool infernalPeakEquipped;
        private int bellChimeCooldown;
        private int solarBurstCooldown;

        public override void ResetEffects()
        {
            infernalPeakEquipped = false;
        }

        public override void PostUpdate()
        {
            if (bellChimeCooldown > 0) bellChimeCooldown--;
            if (solarBurstCooldown > 0) solarBurstCooldown--;
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            HandlePeakHit(target, damageDone);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.owner == Player.whoAmI)
                HandlePeakHit(target, damageDone);
        }

        private void HandlePeakHit(NPC target, int damageDone)
        {
            if (!infernalPeakEquipped) return;
            
            // Extended fire duration (always)
            target.AddBuff(BuffID.OnFire, 420); // 7 seconds
            
            // Bell Chime stun (15%)
            if (bellChimeCooldown <= 0 && Main.rand.NextFloat() < 0.15f)
            {
                bellChimeCooldown = 25;
                target.AddBuff(BuffID.Confused, 120);
                
                // Fire AOE from chime
                float aoeRadius = 150f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.whoAmI != target.whoAmI && !npc.immortal)
                    {
                        if (Vector2.Distance(npc.Center, target.Center) <= aoeRadius)
                        {
                            int aoeDamage = (int)(damageDone * 0.4f);
                            npc.SimpleStrikeNPC(aoeDamage, 0, false, 0, null, false, 0, true);
                            npc.AddBuff(BuffID.OnFire, 300);
                        }
                    }
                }
                
                // Bell chime VFX
                CustomParticles.GenericFlare(target.Center, Color.White, 0.9f, 20);
                CustomParticles.GenericFlare(target.Center, new Color(255, 180, 50), 0.75f, 18);
                
                for (int i = 0; i < 5; i++)
                {
                    CustomParticles.HaloRing(target.Center, 
                        Color.Lerp(new Color(255, 180, 50), new Color(255, 140, 40), i / 5f), 
                        0.35f + i * 0.1f, 14 + i * 2);
                }
            }
            
            // Solar Burst during day (10%)
            if (Main.dayTime && solarBurstCooldown <= 0 && Main.rand.NextFloat() < 0.10f)
            {
                solarBurstCooldown = 60;
                
                // Extra solar damage
                int solarDamage = (int)(damageDone * 0.3f);
                target.SimpleStrikeNPC(solarDamage, 0, false, 0, null, false, 0, true);
                
                // Solar burst VFX
                CustomParticles.GenericFlare(target.Center, new Color(255, 220, 100), 0.8f, 18);
                CustomParticles.ExplosionBurst(target.Center, new Color(255, 180, 50), 10, 7f);
            }
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            if (!infernalPeakEquipped) return;
            
            // Fire retaliation
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.immortal)
                {
                    if (Vector2.Distance(npc.Center, Player.Center) <= 120f)
                    {
                        npc.AddBuff(BuffID.OnFire, 300);
                    }
                }
            }
            
            // Fire burst VFX
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 pos = Player.Center + angle.ToRotationVector2() * 20f;
                var flame = new GenericGlowParticle(pos, angle.ToRotationVector2() * 3f, 
                    new Color(255, 140, 40) * 0.7f, 0.4f, 18, true);
                MagnumParticleHandler.SpawnParticle(flame);
            }
        }
    }
    #endregion

    #region Winter's Enigmatic Silence - Winter + Enigma Variations
    /// <summary>
    /// Phase 5 Season-Theme Hybrid: Glacial Heart + Riddle of the Void
    /// The frozen stillness of winter conceals unknowable mysteries
    /// </summary>
    public class WintersEnigmaticSilence : ModItem
    {
        private static readonly Color WinterBlue = new Color(150, 220, 255);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color FrozenVoid = new Color(100, 140, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 1, gold: 50);
            Item.rare = ModContent.RarityType<EnigmaVariationsRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<WintersEnigmaticSilencePlayer>();
            modPlayer.enigmaticSilenceEquipped = true;
            
            // === WINTER BONUSES (Glacial Heart) ===
            player.statDefense += 14;
            player.endurance += 0.12f;
            player.moveSpeed += 0.08f;
            player.frostBurn = true;
            
            // Ice immunity
            player.buffImmune[BuffID.Frozen] = true;
            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            
            // === ENIGMA BONUSES (Riddle of the Void) ===
            player.GetDamage(DamageClass.Generic) += 0.18f;
            player.GetCritChance(DamageClass.Generic) += 12;
            
            // === HYBRID BONUS: Frozen Enigma ===
            // Void damage freezes enemies
            // Combined debuff application handled in player class
            
            // Ambient VFX
            if (!hideVisual)
            {
                // Frozen enigma orbit
                if (Main.GameUpdateCount % 10 == 0)
                {
                    float baseAngle = Main.GameUpdateCount * 0.012f;
                    
                    // Inner orbit - ice crystals
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = baseAngle + MathHelper.TwoPi * i / 4f;
                        Vector2 pos = player.Center + angle.ToRotationVector2() * 35f;
                        CustomParticles.GenericFlare(pos, WinterBlue, 0.28f, 14);
                    }
                    
                    // Outer orbit - enigma glyphs
                    for (int i = 0; i < 3; i++)
                    {
                        float angle = -baseAngle * 1.5f + MathHelper.TwoPi * i / 3f;
                        Vector2 pos = player.Center + angle.ToRotationVector2() * 55f;
                        CustomParticles.Glyph(pos, EnigmaPurple * 0.6f, 0.3f, -1);
                    }
                }
                
                // Frozen void mist
                if (Main.rand.NextBool(6))
                {
                    Vector2 mistPos = player.Center + Main.rand.NextVector2Circular(40f, 40f);
                    Color mistColor = Color.Lerp(WinterBlue, FrozenVoid, Main.rand.NextFloat()) * 0.5f;
                    CustomParticles.GenericGlow(mistPos, Main.rand.NextVector2Circular(1f, 1f), mistColor, 0.25f, 26, true);
                }
                
                // Occasional enigma eyes in the frost
                if (Main.rand.NextBool(25))
                {
                    Vector2 eyePos = player.Center + Main.rand.NextVector2Circular(50f, 50f);
                    CustomParticles.EnigmaEyeGaze(eyePos, EnigmaPurple * 0.5f, 0.35f, null);
                }
                
                // Snowflake sparkles
                if (Main.rand.NextBool(10))
                {
                    Vector2 snowPos = player.Center + Main.rand.NextVector2Circular(45f, 45f);
                    var sparkle = new SparkleParticle(snowPos, new Vector2(0, 0.5f), WinterBlue, 0.25f, 20);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
                
                // Dual-tone light
                float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.025f) * 0.2f + 0.8f;
                Color lightColor = Color.Lerp(WinterBlue, EnigmaPurple, 0.4f);
                Lighting.AddLight(player.Center, lightColor.ToVector3() * pulse * 0.5f);
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<GlacialHeart>()
                .AddIngredient<RiddleOfTheVoid>()
                .AddIngredient<PermafrostBar>(15)
                .AddIngredient<ResonantCoreOfEnigma>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color WinterBlue = new Color(150, 220, 255);
            Color EnigmaPurple = new Color(140, 60, 200);

            tooltips.Add(new TooltipLine(Mod, "Hybrid", "Glacial Heart + Riddle of the Void")
            {
                OverrideColor = Color.Lerp(WinterBlue, EnigmaPurple, 0.5f)
            });
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+14 defense, +12% damage reduction, +8% move speed, attacks inflict frostburn")
            {
                OverrideColor = WinterBlue
            });
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+18% damage, +12% crit, immunity to Frozen, Chilled, and Frostburn")
            {
                OverrideColor = EnigmaPurple
            });
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Paradox stacks apply random debuffs and slow enemies")
            {
                OverrideColor = Color.Lerp(WinterBlue, EnigmaPurple, 0.3f)
            });
            tooltips.Add(new TooltipLine(Mod, "Effect4", "At 4 stacks: Frozen Void Collapse deals massive AOE damage")
            {
                OverrideColor = Color.Lerp(WinterBlue, EnigmaPurple, 0.7f)
            });
        }
    }

    public class WintersEnigmaticSilencePlayer : ModPlayer
    {
        public bool enigmaticSilenceEquipped;
        private Dictionary<int, int> paradoxStacks = new Dictionary<int, int>();
        private Dictionary<int, int> paradoxTimers = new Dictionary<int, int>();
        private int freezeCooldown;
        
        private static readonly int[] ParadoxDebuffs = new int[]
        {
            BuffID.Confused, BuffID.Slow, BuffID.Frostburn
        };

        public override void ResetEffects()
        {
            enigmaticSilenceEquipped = false;
        }

        public override void PostUpdate()
        {
            if (freezeCooldown > 0) freezeCooldown--;
            
            // Decay paradox stacks
            List<int> toRemove = new List<int>();
            foreach (var kvp in paradoxTimers)
            {
                paradoxTimers[kvp.Key]--;
                if (paradoxTimers[kvp.Key] <= 0)
                    toRemove.Add(kvp.Key);
            }
            foreach (int key in toRemove)
            {
                paradoxTimers.Remove(key);
                paradoxStacks.Remove(key);
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            HandleSilenceHit(target, damageDone);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.owner == Player.whoAmI)
                HandleSilenceHit(target, damageDone);
        }

        private void HandleSilenceHit(NPC target, int damageDone)
        {
            if (!enigmaticSilenceEquipped) return;
            
            // Always apply frostburn
            target.AddBuff(BuffID.Frostburn, 300);
            
            // Paradox stacking (15%)
            if (Main.rand.NextFloat() < 0.15f)
            {
                int debuffId = ParadoxDebuffs[Main.rand.Next(ParadoxDebuffs.Length)];
                target.AddBuff(debuffId, 360);
                
                // Frost debuff
                target.AddBuff(BuffID.Slow, 240);
                
                if (!paradoxStacks.ContainsKey(target.whoAmI))
                    paradoxStacks[target.whoAmI] = 0;
                
                paradoxStacks[target.whoAmI]++;
                paradoxTimers[target.whoAmI] = 420;
                
                // At 4 stacks: Frozen Void Collapse
                if (paradoxStacks[target.whoAmI] >= 4)
                {
                    TriggerFrozenVoidCollapse(target, damageDone);
                    paradoxStacks[target.whoAmI] = 0;
                }
                
                // VFX for stack
                CustomParticles.GenericFlare(target.Center, new Color(140, 60, 200), 0.45f, 14);
                CustomParticles.Glyph(target.Center, new Color(100, 140, 200), 0.4f, -1);
            }
            
            // Deep freeze chance (8%)
            if (freezeCooldown <= 0 && Main.rand.NextFloat() < 0.08f)
            {
                freezeCooldown = 120;
                target.AddBuff(BuffID.Frozen, 90); // Brief freeze
                
                // Freeze VFX
                CustomParticles.GenericFlare(target.Center, new Color(150, 220, 255), 0.7f, 18);
                for (int i = 0; i < 6; i++)
                {
                    CustomParticles.HaloRing(target.Center, new Color(150, 220, 255) * (1f - i * 0.1f), 
                        0.3f + i * 0.08f, 14 + i * 2);
                }
            }
        }

        private void TriggerFrozenVoidCollapse(NPC target, int baseDamage)
        {
            // FROZEN VOID COLLAPSE VFX
            CustomParticles.GenericFlare(target.Center, Color.White, 1.8f, 35);
            CustomParticles.GenericFlare(target.Center, new Color(100, 140, 200), 1.5f, 32);
            CustomParticles.GenericFlare(target.Center, new Color(140, 60, 200), 1.2f, 30);
            
            // Ice shatter burst
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 shardPos = target.Center + angle.ToRotationVector2() * 30f;
                Vector2 shardVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                
                Color shardColor = i % 2 == 0 ? new Color(150, 220, 255) : new Color(100, 140, 200);
                CustomParticles.GenericGlow(shardPos, shardVel, shardColor, 0.35f, 22, true);
            }
            
            // Enigma glyphs
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 glyphPos = target.Center + angle.ToRotationVector2() * Main.rand.NextFloat(40f, 70f);
                CustomParticles.Glyph(glyphPos, new Color(140, 60, 200), 0.5f, -1);
            }
            
            // Halos
            for (int i = 0; i < 8; i++)
            {
                Color haloColor = Color.Lerp(new Color(150, 220, 255), new Color(140, 60, 200), i / 8f);
                CustomParticles.HaloRing(target.Center, haloColor, 0.4f + i * 0.1f, 18 + i * 2);
            }
            
            // Damage
            if (Main.myPlayer == Player.whoAmI)
            {
                int collapseDamage = (int)(baseDamage * 2.5f);
                target.SimpleStrikeNPC(collapseDamage, 0, false, 0, null, false, 0, true);
                target.AddBuff(BuffID.Frozen, 120);
                target.AddBuff(BuffID.Frostburn, 480);
                
                // AOE damage
                float aoeRadius = 200f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.whoAmI != target.whoAmI && !npc.immortal)
                    {
                        if (Vector2.Distance(npc.Center, target.Center) <= aoeRadius)
                        {
                            npc.SimpleStrikeNPC(collapseDamage / 2, 0, false, 0, null, false, 0, true);
                            npc.AddBuff(BuffID.Frostburn, 360);
                            npc.AddBuff(BuffID.Slow, 300);
                        }
                    }
                }
            }
            
            MagnumScreenEffects.AddScreenShake(12f);
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            if (!enigmaticSilenceEquipped) return;
            
            // Frost barrier
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly)
                {
                    if (Vector2.Distance(npc.Center, Player.Center) <= 100f)
                    {
                        npc.AddBuff(BuffID.Slow, 180);
                        npc.AddBuff(BuffID.Frostburn, 240);
                    }
                }
            }
            
            // Frost burst VFX
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 pos = Player.Center + angle.ToRotationVector2() * 20f;
                CustomParticles.GenericGlow(pos, angle.ToRotationVector2() * 2f, 
                    new Color(150, 220, 255) * 0.7f, 0.35f, 18, true);
            }
        }
    }
    #endregion
}
