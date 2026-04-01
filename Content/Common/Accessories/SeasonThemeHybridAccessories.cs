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
            tooltips.Add(new TooltipLine(Mod, "Effect2", "At night: +22% damage, +18 crit, +12 defense, +15% move speed")
            {
                OverrideColor = MoonlightPurple
            });
            tooltips.Add(new TooltipLine(Mod, "Effect2b", "During day: +10% damage, +8 crit")
            {
                OverrideColor = Color.Lerp(SpringPink, MoonlightPurple, 0.4f)
            });
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+12% magic damage, +4 mana regen, +8 life regen at night")
            {
                OverrideColor = Color.Lerp(SpringPink, MoonlightPurple, 0.3f)
            });
            tooltips.Add(new TooltipLine(Mod, "Effect4", "12% chance to confuse enemies at night, 10%/6% chance to heal (8/5 HP), 8% chance to poison")
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
            }
            
            // Bloom healing (8% chance, enhanced at night)
            if (healProcCooldown <= 0 && Main.rand.NextFloat() < (isNight ? 0.10f : 0.06f))
            {
                healProcCooldown = 45;
                int healAmount = isNight ? 8 : 5;
                Player.Heal(healAmount);
                
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
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+16% damage, +10 crit, +8 defense, attacks inflict fire")
            {
                OverrideColor = SummerGold
            });
            tooltips.Add(new TooltipLine(Mod, "Effect1b", "During day: +8% bonus damage")
            {
                OverrideColor = SummerGold
            });
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+22% magic damage, +14 magic crit, -15% mana cost, +4 mana regen")
            {
                OverrideColor = CampanellaOrange
            });
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Immunity to On Fire, Burning, and lava")
            {
                OverrideColor = Color.Lerp(SummerGold, CampanellaOrange, 0.3f)
            });
            tooltips.Add(new TooltipLine(Mod, "Effect4", "15% chance Bell Chime stuns with fire AOE (150 range, 40% damage), 10% Solar Burst during day")
            {
                OverrideColor = Color.Lerp(SummerGold, CampanellaOrange, 0.7f)
            });
            tooltips.Add(new TooltipLine(Mod, "Effect5", "When hit, nearby enemies within 120 range catch fire")
            {
                OverrideColor = Color.Lerp(SummerGold, CampanellaOrange, 0.5f)
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
                
            }
            
            // Solar Burst during day (10%)
            if (Main.dayTime && solarBurstCooldown <= 0 && Main.rand.NextFloat() < 0.10f)
            {
                solarBurstCooldown = 60;
                
                // Extra solar damage
                int solarDamage = (int)(damageDone * 0.3f);
                target.SimpleStrikeNPC(solarDamage, 0, false, 0, null, false, 0, true);
                
                // Solar burst VFX
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
            tooltips.Add(new TooltipLine(Mod, "Effect3", "15% chance per hit to apply Paradox debuffs and slow enemies")
            {
                OverrideColor = Color.Lerp(WinterBlue, EnigmaPurple, 0.3f)
            });
            tooltips.Add(new TooltipLine(Mod, "Effect4", "At 4 stacks: Frozen Void Collapse deals 2.5x damage in 200 range (50% AOE)")
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
            }
            
            // Deep freeze chance (8%)
            if (freezeCooldown <= 0 && Main.rand.NextFloat() < 0.08f)
            {
                freezeCooldown = 120;
                target.AddBuff(BuffID.Frozen, 90); // Brief freeze
                
                // Freeze VFX
            }
        }

        private void TriggerFrozenVoidCollapse(NPC target, int baseDamage)
        {
            // FROZEN VOID COLLAPSE VFX
            
            // Ice shatter burst
            
            // Enigma glyphs
            
            // Halos
            
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
        }
    }
    #endregion
}
