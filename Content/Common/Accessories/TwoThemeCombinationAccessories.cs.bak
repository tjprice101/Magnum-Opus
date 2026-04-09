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
using MagnumOpus.Content.Eroica.Accessories;
using MagnumOpus.Content.Eroica.Accessories.Shared;
using MagnumOpus.Content.LaCampanella.Accessories;
using MagnumOpus.Content.EnigmaVariations.Accessories;
using MagnumOpus.Content.SwanLake.Accessories;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.HarmonicCores;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.Eroica.HarmonicCores;
using EroicaColors = MagnumOpus.Common.Systems.CustomParticleSystem.EroicaColors;
using MagnumOpus.Content.Eroica;
using MagnumOpus.Content.LaCampanella.ResonanceEnergies;
using MagnumOpus.Content.LaCampanella.HarmonicCores;
using MagnumOpus.Content.EnigmaVariations.ResonanceEnergies;
using MagnumOpus.Content.EnigmaVariations.HarmonicCores;
using MagnumOpus.Content.SwanLake.ResonanceEnergies;
using MagnumOpus.Content.SwanLake.HarmonicCores;

namespace MagnumOpus.Content.Common.Accessories
{
    #region Nocturne of Azure Flames - Moonlight + La Campanella
    /// <summary>
    /// Phase 4 Two-Theme Combination: Moonlight Sonata + La Campanella
    /// Combines lunar mysticism with infernal flames
    /// Fire burns blue at night for extra damage
    /// </summary>
    public class NocturneOfAzureFlames : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 75);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<NocturneOfAzureFlamesPlayer>();
            modPlayer.nocturneOfAzureFlamesEquipped = true;
            
            bool isNight = !Main.dayTime;
            
            // Moonlight bonuses
            if (isNight)
            {
                player.GetDamage(DamageClass.Generic) += 0.18f;
                player.GetCritChance(DamageClass.Generic) += 20;
                player.statDefense += 12;
            }
            else
            {
                player.GetDamage(DamageClass.Generic) += 0.10f;
            }
            
            // Campanella bonuses - magic damage and mana
            player.GetDamage(DamageClass.Magic) += 0.22f;
            player.GetCritChance(DamageClass.Magic) += 10;
            player.manaCost -= 0.12f;
            
            // Fire immunity from Campanella
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            bool isNight = !Main.dayTime;
            Color moonBlue = new Color(100, 150, 255);
            Color flameOrange = new Color(255, 140, 40);
            
            tooltips.Add(new TooltipLine(Mod, "Combo", "Combines: Moonlight Sonata + La Campanella")
            {
                OverrideColor = moonBlue
            });
            
            tooltips.Add(new TooltipLine(Mod, "MoonlightStats", "At night: +18% damage, +20 crit, +12 defense | Day: +10% damage")
            {
                OverrideColor = MoonlightColors.Silver
            });
            
            tooltips.Add(new TooltipLine(Mod, "CampanellaStats", "+22% magic damage, +10 magic crit, -12% mana cost")
            {
                OverrideColor = flameOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Special", "Fire burns blue at night, dealing 15% bonus damage")
            {
                OverrideColor = moonBlue
            });
            
            tooltips.Add(new TooltipLine(Mod, "Immunities", "Immunity to On Fire! and Burning")
            {
                OverrideColor = flameOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Azure flames dance beneath the moonlit sky'")
            {
                OverrideColor = new Color(150, 180, 220)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SonatasEmbrace>()
                .AddIngredient<InfernalVirtuoso>()
                .AddIngredient<HarmonicCoreOfMoonlightSonata>(15)
                .AddIngredient<HarmonicCoreOfLaCampanella>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class NocturneOfAzureFlamesPlayer : ModPlayer
    {
        public bool nocturneOfAzureFlamesEquipped;
        private int bellRingCooldown;

        public override void ResetEffects()
        {
            nocturneOfAzureFlamesEquipped = false;
        }

        public override void PostUpdate()
        {
            if (bellRingCooldown > 0) bellRingCooldown--;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!nocturneOfAzureFlamesEquipped) return;
            if (proj.owner != Player.whoAmI) return;
            
            bool isNight = !Main.dayTime;
            
            // Blue fire at night does bonus damage
            if (isNight && DamageClass.Magic.CountsAsClass(proj.DamageType))
            {
                // 15% bonus as blue fire damage
                int bonusDamage = (int)(damageDone * 0.15f);
                target.SimpleStrikeNPC(bonusDamage, 0, false, 0, null, false, 0, true);
                
                // Blue fire VFX
                Color blueFlame = new Color(100, 150, 255);
                
            }
            
            // Bell ring stun from Campanella (10% at night, 8% during day)
            float stunChance = isNight ? 0.10f : 0.08f;
            if (bellRingCooldown <= 0 && Main.rand.NextFloat() < stunChance)
            {
                bellRingCooldown = 30;
                target.AddBuff(BuffID.Confused, 90);
                
                // Bell chime VFX
                Color chimeColor = isNight ? new Color(100, 150, 255) : CampanellaColors.Orange;
                
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item35 with { Pitch = isNight ? 0.5f : 0f }, target.Center);
            }
        }
    }
    #endregion

    #region Valse Macabre - Eroica + Enigma Variations
    /// <summary>
    /// Phase 4 Two-Theme Combination: Eroica + Enigma Variations
    /// Combines heroic valor with mysterious paradoxes
    /// Heroic kills spread Paradox debuffs to nearby enemies
    /// </summary>
    public class ValseMacabre : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 75);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<ValseMacabrePlayer>();
            modPlayer.valseMacabreEquipped = true;
            
            // Eroica bonuses - melee
            player.GetDamage(DamageClass.Melee) += 0.20f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.15f;
            player.GetCritChance(DamageClass.Melee) += 10;
            player.GetDamage(DamageClass.Generic) += 0.08f;
            
            // Enigma bonuses - all damage
            player.GetDamage(DamageClass.Generic) += 0.18f;
            player.GetCritChance(DamageClass.Generic) += 8;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color eroicaGold = new Color(255, 200, 80);
            Color enigmaPurple = EnigmaColors.Purple;
            
            tooltips.Add(new TooltipLine(Mod, "Combo", "Combines: Eroica + Enigma Variations")
            {
                OverrideColor = eroicaGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "EroicaStats", "+20% melee damage, +15% melee attack speed, +10 melee crit, +8% all damage")
            {
                OverrideColor = EroicaColors.Gold
            });
            
            tooltips.Add(new TooltipLine(Mod, "EnigmaStats", "+18% all damage, +8 crit chance")
            {
                OverrideColor = EnigmaColors.GreenFlame
            });
            
            tooltips.Add(new TooltipLine(Mod, "Special", "Melee kills spread Paradox debuffs to nearby enemies")
            {
                OverrideColor = enigmaPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effects", "Kills trigger 5-second Heroic Surge (+25% damage) and spread Paradox to nearby enemies")
            {
                OverrideColor = eroicaGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Paradox", "12% chance per hit to apply random Paradox debuff")
            {
                OverrideColor = enigmaPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A dance of heroism and mystery'")
            {
                OverrideColor = new Color(200, 180, 160)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<HerosSymphony>()
                .AddIngredient<RiddleOfTheVoid>()
                .AddIngredient<HarmonicCoreOfEroica>(15)
                .AddIngredient<HarmonicCoreOfEnigma>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class ValseMacabrePlayer : ModPlayer
    {
        public bool valseMacabreEquipped;
        private int heroicSurgeTimer;
        private int invulnFramesOnKill = 60; // 1 second
        
        private static readonly int[] ParadoxDebuffs = new int[]
        {
            BuffID.Confused,
            BuffID.Slow,
            BuffID.CursedInferno,
            BuffID.Ichor,
            BuffID.ShadowFlame
        };

        public override void ResetEffects()
        {
            valseMacabreEquipped = false;
        }

        public override void PostUpdate()
        {
            if (heroicSurgeTimer > 0)
            {
                heroicSurgeTimer--;
                // Heroic Surge active
                Player.GetDamage(DamageClass.Generic) += 0.25f;
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            HandleValseMacabreHit(target, damageDone);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.owner == Player.whoAmI)
            {
                HandleValseMacabreHit(target, damageDone);
            }
        }

        private void HandleValseMacabreHit(NPC target, int damageDone)
        {
            if (!valseMacabreEquipped) return;
            
            // 12% Paradox chance
            if (Main.rand.NextFloat() < 0.12f)
            {
                int debuffId = ParadoxDebuffs[Main.rand.Next(ParadoxDebuffs.Length)];
                target.AddBuff(debuffId, 240);
                
                // Heroic-mystery VFX
            }
            
            // Check for kill
            if (target.life <= 0 && !target.immortal)
            {
                TriggerHeroicKill(target);
            }
        }

        private void TriggerHeroicKill(NPC killedTarget)
        {
            // Brief invulnerability
            Player.immune = true;
            Player.immuneTime = Math.Max(Player.immuneTime, invulnFramesOnKill);
            
            // Trigger Heroic Surge
            heroicSurgeTimer = 300; // 5 seconds
            
            // SPREAD PARADOX to nearby enemies!
            float spreadRadius = 300f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.immortal && npc.whoAmI != killedTarget.whoAmI)
                {
                    if (Vector2.Distance(npc.Center, killedTarget.Center) <= spreadRadius)
                    {
                        // Apply random Paradox debuff
                        int debuffId = ParadoxDebuffs[Main.rand.Next(ParadoxDebuffs.Length)];
                        npc.AddBuff(debuffId, 180);
                        
                        // Spread VFX
                        Vector2 direction = (npc.Center - killedTarget.Center).SafeNormalize(Vector2.Zero);
                        
                    }
                }
            }
            
            // Kill VFX burst
            
        }
    }
    #endregion

    #region Reverie of the Silver Swan - Moonlight + Swan Lake
    /// <summary>
    /// Phase 4 Two-Theme Combination: Moonlight Sonata + Swan Lake
    /// Combines lunar mysticism with balletic grace
    /// Dancing under moonlight grants powerful buffs
    /// </summary>
    public class ReverieOfTheSilverSwan : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 75);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<ReverieOfTheSilverSwanPlayer>();
            modPlayer.reverieOfTheSilverSwanEquipped = true;
            
            bool isNight = !Main.dayTime;
            
            // Moonlight bonuses
            if (isNight)
            {
                player.GetDamage(DamageClass.Generic) += 0.18f;
                player.GetCritChance(DamageClass.Generic) += 20;
                player.statDefense += 12;
            }
            else
            {
                player.GetDamage(DamageClass.Generic) += 0.10f;
            }
            
            // Swan Lake bonuses
            player.GetDamage(DamageClass.Generic) += 0.16f;
            player.GetCritChance(DamageClass.Generic) += 8;
            player.moveSpeed += 0.20f;
            player.runAcceleration *= 1.20f;
            
            // Night dancing bonus (moving at night gives extra buffs)
            if (isNight && player.velocity.Length() > 3f)
            {
                player.GetDamage(DamageClass.Generic) += 0.05f;
                player.GetCritChance(DamageClass.Generic) += 5;
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            bool isNight = !Main.dayTime;
            Color moonSilver = MoonlightColors.Silver;
            Color rainbow = SwanColors.GetRainbow((float)(Main.GameUpdateCount % 300) / 300f);
            
            tooltips.Add(new TooltipLine(Mod, "Combo", "Combines: Moonlight Sonata + Swan Lake")
            {
                OverrideColor = moonSilver
            });
            
            tooltips.Add(new TooltipLine(Mod, "MoonlightStats", "At night: +18% damage, +20 crit, +12 defense | Day: +10% damage")
            {
                OverrideColor = MoonlightColors.LightBlue
            });
            
            tooltips.Add(new TooltipLine(Mod, "SwanStats", "+16% all damage, +8 crit, +20% movement speed")
            {
                OverrideColor = rainbow
            });
            
            tooltips.Add(new TooltipLine(Mod, "Special", "Moving at night: +5% damage, +5 crit")
            {
                OverrideColor = moonSilver
            });
            
            tooltips.Add(new TooltipLine(Mod, "Dodge", isNight ? "12% chance to dodge attacks (night)" : "8% chance to dodge attacks (day)")
            {
                OverrideColor = rainbow
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Moonlit feathers dance across the water's surface'")
            {
                OverrideColor = new Color(200, 210, 240)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SonatasEmbrace>()
                .AddIngredient<SwansChromaticDiadem>()
                .AddIngredient<HarmonicCoreOfMoonlightSonata>(15)
                .AddIngredient<HarmonicCoreOfSwanLake>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class ReverieOfTheSilverSwanPlayer : ModPlayer
    {
        public bool reverieOfTheSilverSwanEquipped;
        private int dodgeCooldown;

        public override void ResetEffects()
        {
            reverieOfTheSilverSwanEquipped = false;
        }

        public override void PostUpdate()
        {
            if (dodgeCooldown > 0) dodgeCooldown--;
        }

        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (!reverieOfTheSilverSwanEquipped) return false;
            if (dodgeCooldown > 0) return false;
            
            bool isNight = !Main.dayTime;
            float dodgeChance = isNight ? 0.12f : 0.08f; // Higher chance at night
            
            if (Main.rand.NextFloat() < dodgeChance)
            {
                dodgeCooldown = 90;
                TriggerGracefulDodge();
                return true;
            }
            
            return false;
        }

        private void TriggerGracefulDodge()
        {
            // Moonlit ballet dodge VFX
            
            // Central flash
            
            // Rainbow-lunar halos
            
            // Feather burst
            
            // Prismatic sparkle spiral
            
            // Brief invulnerability
            Player.immune = true;
            Player.immuneTime = 20;
            
            // Sound
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.7f }, Player.Center);
        }
    }
    #endregion

    #region Fantasia of Burning Grace - La Campanella + Swan Lake
    /// <summary>
    /// Phase 4 Two-Theme Combination: La Campanella + Swan Lake
    /// Combines infernal flames with balletic grace
    /// Fire trails shimmer with rainbow colors
    /// </summary>
    public class FantasiaOfBurningGrace : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 75);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<FantasiaOfBurningGracePlayer>();
            modPlayer.fantasiaOfBurningGraceEquipped = true;
            
            // La Campanella bonuses
            player.GetDamage(DamageClass.Magic) += 0.22f;
            player.GetCritChance(DamageClass.Magic) += 10;
            player.manaCost -= 0.12f;
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            
            // Swan Lake bonuses
            player.GetDamage(DamageClass.Generic) += 0.16f;
            player.GetCritChance(DamageClass.Generic) += 8;
            player.moveSpeed += 0.20f;
            player.runAcceleration *= 1.20f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color flameOrange = new Color(255, 140, 40);
            Color rainbow = SwanColors.GetRainbow((float)(Main.GameUpdateCount % 300) / 300f);
            
            tooltips.Add(new TooltipLine(Mod, "Combo", "Combines: La Campanella + Swan Lake")
            {
                OverrideColor = flameOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "CampanellaStats", "+22% magic damage, +10 magic crit, -12% mana cost")
            {
                OverrideColor = flameOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "SwanStats", "+16% all damage, +8 crit, +20% movement speed")
            {
                OverrideColor = rainbow
            });
            
            tooltips.Add(new TooltipLine(Mod, "Immunities", "Immunity to On Fire! and Burning")
            {
                OverrideColor = flameOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effects", "10% chance Bell ring stun with fire AOE (120 range, 50% damage), 10% dodge chance")
            {
                OverrideColor = rainbow
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Fire trails shimmer with rainbow colors in a burning ballet'")
            {
                OverrideColor = new Color(255, 200, 180)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<InfernalVirtuoso>()
                .AddIngredient<SwansChromaticDiadem>()
                .AddIngredient<HarmonicCoreOfLaCampanella>(15)
                .AddIngredient<HarmonicCoreOfSwanLake>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class FantasiaOfBurningGracePlayer : ModPlayer
    {
        public bool fantasiaOfBurningGraceEquipped;
        private int bellRingCooldown;
        private int dodgeCooldown;

        public override void ResetEffects()
        {
            fantasiaOfBurningGraceEquipped = false;
        }

        public override void PostUpdate()
        {
            if (bellRingCooldown > 0) bellRingCooldown--;
            if (dodgeCooldown > 0) dodgeCooldown--;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!fantasiaOfBurningGraceEquipped) return;
            if (proj.owner != Player.whoAmI) return;
            
            // Rainbow fire trail on magic projectiles
            if (DamageClass.Magic.CountsAsClass(proj.DamageType))
            {
                // Leave rainbow fire particles
                if (Main.rand.NextBool(3))
                {
                    Color trailColor = Color.Lerp(CampanellaColors.Orange, 
                        SwanColors.GetRainbow(Main.rand.NextFloat()), 0.5f);
                }
            }
            
            // Bell ring stun (10%)
            if (bellRingCooldown <= 0 && Main.rand.NextFloat() < 0.10f)
            {
                bellRingCooldown = 30;
                target.AddBuff(BuffID.Confused, 90);
                
                // Rainbow bell chime
                
                // AoE rainbow fire
                float aoeRadius = 120f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.whoAmI != target.whoAmI && !npc.immortal)
                    {
                        if (Vector2.Distance(npc.Center, target.Center) <= aoeRadius)
                        {
                            int aoeDamage = (int)(damageDone * 0.5f);
                            npc.SimpleStrikeNPC(aoeDamage, 0, false, 0, null, false, 0, true);
                            npc.AddBuff(BuffID.OnFire, 180);
                            
                        }
                    }
                }
                
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item35, target.Center);
            }
        }

        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (!fantasiaOfBurningGraceEquipped) return false;
            if (dodgeCooldown > 0) return false;
            
            if (Main.rand.NextFloat() < 0.10f)
            {
                dodgeCooldown = 100;
                TriggerFantasiaDodge();
                return true;
            }
            
            return false;
        }

        private void TriggerFantasiaDodge()
        {
            // Fiery rainbow burst
            
            
            // Burning feathers
            
            Player.immune = true;
            Player.immuneTime = 25;
            
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f }, Player.Center);
        }
    }
    #endregion

    #region Triumphant Arabesque - Eroica + Swan Lake
    /// <summary>
    /// Phase 4 Two-Theme Combination: Eroica + Swan Lake
    /// Combines heroic valor with balletic grace
    /// Graceful kills trigger Heroic Surges
    /// </summary>
    public class TriumphantArabesque : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 75);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<TriumphantArabesquePlayer>();
            modPlayer.triumphantArabesqueEquipped = true;
            
            // Eroica bonuses
            player.GetDamage(DamageClass.Melee) += 0.20f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.15f;
            player.GetCritChance(DamageClass.Melee) += 10;
            player.GetDamage(DamageClass.Generic) += 0.08f;
            
            // Swan Lake bonuses
            player.GetDamage(DamageClass.Generic) += 0.16f;
            player.GetCritChance(DamageClass.Generic) += 8;
            player.moveSpeed += 0.20f;
            player.runAcceleration *= 1.20f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color eroicaGold = new Color(255, 200, 80);
            Color rainbow = SwanColors.GetRainbow((float)(Main.GameUpdateCount % 300) / 300f);
            
            tooltips.Add(new TooltipLine(Mod, "Combo", "Combines: Eroica + Swan Lake")
            {
                OverrideColor = eroicaGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "EroicaStats", "+20% melee damage, +15% melee attack speed, +10 melee crit, +8% all damage")
            {
                OverrideColor = EroicaColors.Gold
            });
            
            tooltips.Add(new TooltipLine(Mod, "SwanStats", "+16% all damage, +8 crit, +20% movement speed")
            {
                OverrideColor = rainbow
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effects", "Melee kills trigger 5-second Heroic Surge (+25% damage), 10% dodge chance")
            {
                OverrideColor = eroicaGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Sakura petals and swan feathers dance as one'")
            {
                OverrideColor = new Color(255, 200, 220)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<HerosSymphony>()
                .AddIngredient<SwansChromaticDiadem>()
                .AddIngredient<HarmonicCoreOfEroica>(15)
                .AddIngredient<HarmonicCoreOfSwanLake>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class TriumphantArabesquePlayer : ModPlayer
    {
        public bool triumphantArabesqueEquipped;
        private int heroicSurgeTimer;
        private int invulnFramesOnKill = 60;
        private int dodgeCooldown;

        public override void ResetEffects()
        {
            triumphantArabesqueEquipped = false;
        }

        public override void PostUpdate()
        {
            if (heroicSurgeTimer > 0)
            {
                heroicSurgeTimer--;
                Player.GetDamage(DamageClass.Generic) += 0.25f;
            }
            
            if (dodgeCooldown > 0) dodgeCooldown--;
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            HandleKill(target);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.owner == Player.whoAmI)
            {
                HandleKill(target);
            }
        }

        private void HandleKill(NPC target)
        {
            if (!triumphantArabesqueEquipped) return;
            
            if (target.life <= 0 && !target.immortal)
            {
                // Invulnerability
                Player.immune = true;
                Player.immuneTime = Math.Max(Player.immuneTime, invulnFramesOnKill);
                
                // Heroic Surge
                heroicSurgeTimer = 300;
                
                // Graceful heroic kill VFX
                
                
                
            }
        }

        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (!triumphantArabesqueEquipped) return false;
            if (dodgeCooldown > 0) return false;
            
            if (Main.rand.NextFloat() < 0.10f)
            {
                dodgeCooldown = 100;
                
                // Graceful dodge with sakura and feathers
                
                
                
                
                Player.immune = true;
                Player.immuneTime = 20;
                
                return true;
            }
            
            return false;
        }
    }
    #endregion

    #region Inferno of Lost Shadows - La Campanella + Enigma Variations
    /// <summary>
    /// Phase 4 Two-Theme Combination: La Campanella + Enigma Variations
    /// Combines infernal flames with mysterious paradoxes
    /// Fire inflicts random Paradox debuffs
    /// </summary>
    public class InfernoOfLostShadows : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 75);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<InfernoOfLostShadowsPlayer>();
            modPlayer.infernoOfLostShadowsEquipped = true;
            
            // La Campanella bonuses
            player.GetDamage(DamageClass.Magic) += 0.22f;
            player.GetCritChance(DamageClass.Magic) += 10;
            player.manaCost -= 0.12f;
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            
            // Enigma bonuses
            player.GetDamage(DamageClass.Generic) += 0.18f;
            player.GetCritChance(DamageClass.Generic) += 8;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color flameOrange = new Color(255, 140, 40);
            Color greenFlame = EnigmaColors.GreenFlame;
            
            tooltips.Add(new TooltipLine(Mod, "Combo", "Combines: La Campanella + Enigma Variations")
            {
                OverrideColor = flameOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "CampanellaStats", "+22% magic damage, +10 magic crit, -12% mana cost")
            {
                OverrideColor = flameOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "EnigmaStats", "+18% all damage, +8 crit chance")
            {
                OverrideColor = greenFlame
            });
            
            tooltips.Add(new TooltipLine(Mod, "Immunities", "Immunity to On Fire! and Burning")
            {
                OverrideColor = flameOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effects", "15% Paradox chance (magic), 10% (other); 10% Bell ring stun (120 range, 50% AOE)")
            {
                OverrideColor = greenFlame
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Flames that burn with questions, shadows that whisper riddles'")
            {
                OverrideColor = new Color(180, 140, 100)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<InfernalVirtuoso>()
                .AddIngredient<RiddleOfTheVoid>()
                .AddIngredient<HarmonicCoreOfLaCampanella>(15)
                .AddIngredient<HarmonicCoreOfEnigma>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class InfernoOfLostShadowsPlayer : ModPlayer
    {
        public bool infernoOfLostShadowsEquipped;
        private int bellRingCooldown;
        private Dictionary<int, int> paradoxStacks = new Dictionary<int, int>();
        private Dictionary<int, int> paradoxTimers = new Dictionary<int, int>();
        
        private static readonly int[] ParadoxDebuffs = new int[]
        {
            BuffID.Confused,
            BuffID.Slow,
            BuffID.CursedInferno,
            BuffID.Ichor,
            BuffID.ShadowFlame,
            BuffID.Frostburn
        };

        public override void ResetEffects()
        {
            infernoOfLostShadowsEquipped = false;
        }

        public override void PostUpdate()
        {
            if (bellRingCooldown > 0) bellRingCooldown--;
            
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

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!infernoOfLostShadowsEquipped) return;
            if (proj.owner != Player.whoAmI) return;
            
            // Fire inflicts random Paradox (15% for magic, 10% otherwise)
            float paradoxChance = DamageClass.Magic.CountsAsClass(proj.DamageType) ? 0.15f : 0.10f;
            
            if (Main.rand.NextFloat() < paradoxChance)
            {
                int debuffId = ParadoxDebuffs[Main.rand.Next(ParadoxDebuffs.Length)];
                target.AddBuff(debuffId, 240);
                target.AddBuff(BuffID.OnFire, 180);
                
                // Track stacks
                if (!paradoxStacks.ContainsKey(target.whoAmI))
                    paradoxStacks[target.whoAmI] = 0;
                
                paradoxStacks[target.whoAmI]++;
                paradoxTimers[target.whoAmI] = 300;
                
                int stacks = paradoxStacks[target.whoAmI];
                
                // Void fire VFX
                
                
                // Void Collapse at 5 stacks
                if (stacks >= 5)
                {
                    TriggerVoidFlameCollapse(target, damageDone);
                    paradoxStacks[target.whoAmI] = 0;
                }
            }
            
            // Bell ring stun (10%)
            if (bellRingCooldown <= 0 && Main.rand.NextFloat() < 0.10f)
            {
                bellRingCooldown = 30;
                target.AddBuff(BuffID.Confused, 90);
                
                // Void bell chime
                Color chimeColor = Color.Lerp(CampanellaColors.Orange, EnigmaColors.GreenFlame, 0.4f);
                
                // AoE Paradox + fire
                float aoeRadius = 120f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.whoAmI != target.whoAmI && !npc.immortal)
                    {
                        if (Vector2.Distance(npc.Center, target.Center) <= aoeRadius)
                        {
                            int aoeDamage = (int)(damageDone * 0.5f);
                            npc.SimpleStrikeNPC(aoeDamage, 0, false, 0, null, false, 0, true);
                            npc.AddBuff(BuffID.OnFire, 180);
                            
                            int debuffId = ParadoxDebuffs[Main.rand.Next(ParadoxDebuffs.Length)];
                            npc.AddBuff(debuffId, 120);
                            
                        }
                    }
                }
                
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item35 with { Pitch = -0.3f }, target.Center);
            }
        }

        private void TriggerVoidFlameCollapse(NPC target, int baseDamage)
        {
            // Massive void fire explosion
            
            
            // Glyph spiral
            
            // Eye formation
            
            
            // Deal massive damage
            if (Main.myPlayer == Player.whoAmI)
            {
                int voidDamage = (int)(baseDamage * 2.5f);
                target.SimpleStrikeNPC(voidDamage, 0, false, 0, null, false, 0, true);
                
                // AOE
                float aoeRadius = 200f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.whoAmI != target.whoAmI && !npc.immortal)
                    {
                        if (Vector2.Distance(npc.Center, target.Center) <= aoeRadius)
                        {
                            npc.SimpleStrikeNPC(voidDamage / 2, 0, false, 0, null, false, 0, true);
                            npc.AddBuff(BuffID.OnFire, 300);
                            npc.AddBuff(ParadoxDebuffs[Main.rand.Next(ParadoxDebuffs.Length)], 180);
                            
                        }
                    }
                }
            }
            
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.5f, Volume = 1.3f }, target.Center);
        }
    }
    #endregion
}

