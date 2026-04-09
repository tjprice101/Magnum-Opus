using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.EnigmaVariations.ResonanceEnergies;
using MagnumOpus.Content.EnigmaVariations.HarmonicCores;
using MagnumOpus.Content.Materials.EnemyDrops;

namespace MagnumOpus.Content.EnigmaVariations.Accessories
{
    /// <summary>
    /// Enigma theme color constants - void mystery, purple ↁEgreen gradient
    /// </summary>
    public static class EnigmaColors
    {
        public static readonly Color VoidBlack = new Color(15, 10, 20);
        public static readonly Color DeepPurple = new Color(80, 20, 120);
        public static readonly Color Purple = new Color(140, 60, 200);
        public static readonly Color GreenFlame = new Color(50, 220, 100);
        public static readonly Color DarkGreen = new Color(30, 100, 50);
    }

    #region Puzzle Fragment
    /// <summary>
    /// Phase 3 Enigma Tier 1 Accessory - Post-Moon Lord
    /// +12% all damage
    /// 8% chance on hit to apply "Paradox" - a random debuff (Confused, Slow, Cursed Inferno, Ichor)
    /// Mysterious, arcane energy with watching eyes
    /// </summary>
    public class PuzzleFragment : ModItem
    {
        public override string Texture => "MagnumOpus/Content/EnigmaVariations/Accessories/PendantOfAThousandPuzzles/PuzzleFragment";

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 25);
            Item.rare = ModContent.RarityType<EnigmaVariationsRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // +20% all damage
            player.GetDamage(DamageClass.Generic) += 0.20f;
            
            // Enable Paradox debuff mechanic
            player.GetModPlayer<PuzzleFragmentPlayer>().puzzleFragmentEquipped = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantCoreOfEnigma>(3)
                .AddIngredient<EnigmaResonantEnergy>(8)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "DamageBoost", "+20% damage")
            {
                OverrideColor = EnigmaColors.Purple
            });
            tooltips.Add(new TooltipLine(Mod, "Paradox", "8% chance on hit to apply 'Paradox' - a random debuff")
            {
                OverrideColor = EnigmaColors.GreenFlame
            });
            tooltips.Add(new TooltipLine(Mod, "Debuffs", "Paradox: Confused, Slow, Cursed Inferno, or Ichor")
            {
                OverrideColor = EnigmaColors.DeepPurple
            });
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'A piece of a puzzle no one was meant to solve'")
            {
                OverrideColor = new Color(140, 60, 200)
            });
        }
    }

    public class PuzzleFragmentPlayer : ModPlayer
    {
        public bool puzzleFragmentEquipped;
        
        // Debuff IDs for Paradox effect
        private static readonly int[] ParadoxDebuffs = new int[]
        {
            BuffID.Confused,
            BuffID.Slow,
            BuffID.CursedInferno,
            BuffID.Ichor
        };

        public override void ResetEffects()
        {
            puzzleFragmentEquipped = false;
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            TryApplyParadox(target);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.owner == Player.whoAmI)
            {
                TryApplyParadox(target);
            }
        }

        private void TryApplyParadox(NPC target)
        {
            if (!puzzleFragmentEquipped) return;
            if (target.immortal || target.dontTakeDamage) return;
            
            // 8% chance for Paradox
            if (Main.rand.NextFloat() < 0.08f)
            {
                // Apply random debuff
                int debuffId = ParadoxDebuffs[Main.rand.Next(ParadoxDebuffs.Length)];
                target.AddBuff(debuffId, 180); // 3 seconds
                
                // Mysterious VFX on Paradox application
                
                // Glyph burst
                
                // Watching eye at center
                
                // Halo ring
            }
        }
    }
    #endregion

    #region Riddle of the Void
    /// <summary>
    /// Phase 3 Enigma Tier 2 Accessory - Post-Moon Lord (Combination)
    /// Includes all Puzzle Fragment benefits maximized:
    /// +18% all damage
    /// 12% Paradox chance
    /// Paradox can now stack up to 5 times
    /// At 5 stacks, triggers "Void Collapse" - massive damage explosion
    /// Void energy constantly swirls around player
    /// </summary>
    public class RiddleOfTheVoid : ModItem
    {
        public override string Texture => "MagnumOpus/Content/EnigmaVariations/Accessories/RiddlemastersCauldron/RiddleOfTheVoid";

        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 36;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<EnigmaVariationsRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // +20% all damage
            player.GetDamage(DamageClass.Generic) += 0.20f;
            
            // +8% crit chance
            player.GetCritChance(DamageClass.Generic) += 8;
            
            // Enable Void Paradox mechanic
            player.GetModPlayer<RiddleOfTheVoidPlayer>().riddleOfTheVoidEquipped = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<PuzzleFragment>()
                .AddIngredient<HarmonicCoreOfEnigma>(2)
                .AddIngredient<EnigmaResonantEnergy>(15)
                .AddIngredient<RemnantOfMysteries>(5)
                .AddIngredient<MysteryEssence>(10)
                .AddIngredient(ItemID.FragmentNebula, 10)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "DamageBoost", "+20% damage, +8% critical strike chance")
            {
                OverrideColor = EnigmaColors.Purple
            });
            tooltips.Add(new TooltipLine(Mod, "Paradox", "12% chance on hit to apply 'Paradox Rush' debuffs")
            {
                OverrideColor = EnigmaColors.GreenFlame
            });
            tooltips.Add(new TooltipLine(Mod, "Stacking", "Paradox Rush stacks up to 5 times")
            {
                OverrideColor = EnigmaColors.DeepPurple
            });
            tooltips.Add(new TooltipLine(Mod, "VoidCollapse", "At 5 stacks, triggers 'Void Collapse' dealing 2% of boss current HP")
            {
                OverrideColor = EnigmaColors.GreenFlame
            });
            tooltips.Add(new TooltipLine(Mod, "ParadoxChance", "2% chance on hit to apply 'Paradox' - random debuff")
            {
                OverrideColor = EnigmaColors.Purple
            });
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The answer was void all along'")
            {
                OverrideColor = new Color(140, 60, 200)
            });
        }
    }

    public class RiddleOfTheVoidPlayer : ModPlayer
    {
        public bool riddleOfTheVoidEquipped;
        
        // Debuff IDs for enhanced Paradox effect
        private static readonly int[] ParadoxDebuffs = new int[]
        {
            BuffID.Confused,
            BuffID.Slow,
            BuffID.CursedInferno,
            BuffID.Ichor,
            BuffID.ShadowFlame, // Added for enhanced version
            BuffID.Frostburn   // Added for enhanced version
        };
        
        // Track Paradox stacks per NPC
        private Dictionary<int, int> paradoxStacks = new Dictionary<int, int>();
        private Dictionary<int, int> paradoxTimers = new Dictionary<int, int>();

        public override void ResetEffects()
        {
            riddleOfTheVoidEquipped = false;
        }

        public override void PostUpdate()
        {
            // Decay paradox stacks over time
            List<int> toRemove = new List<int>();
            foreach (var kvp in paradoxTimers)
            {
                paradoxTimers[kvp.Key]--;
                if (paradoxTimers[kvp.Key] <= 0)
                {
                    toRemove.Add(kvp.Key);
                }
            }
            foreach (int key in toRemove)
            {
                paradoxTimers.Remove(key);
                paradoxStacks.Remove(key);
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            TryApplyVoidParadox(target, damageDone);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.owner == Player.whoAmI)
            {
                TryApplyVoidParadox(target, damageDone);
            }
        }

        private void TryApplyVoidParadox(NPC target, int damageDone)
        {
            if (!riddleOfTheVoidEquipped) return;
            if (target.immortal || target.dontTakeDamage) return;
            
            // 2% chance for basic Paradox (random debuff, separate from Rush)
            if (Main.rand.NextFloat() < 0.02f)
            {
                int basicDebuff = ParadoxDebuffs[Main.rand.Next(4)]; // Only first 4 (Confused, Slow, Cursed Inferno, Ichor)
                target.AddBuff(basicDebuff, 180);
            }
            
            // 12% chance for Paradox Rush (stacking)
            if (Main.rand.NextFloat() < 0.12f)
            {
                // Apply random debuff
                int debuffId = ParadoxDebuffs[Main.rand.Next(ParadoxDebuffs.Length)];
                target.AddBuff(debuffId, 240); // 4 seconds
                
                // Track stacks
                if (!paradoxStacks.ContainsKey(target.whoAmI))
                {
                    paradoxStacks[target.whoAmI] = 0;
                }
                
                paradoxStacks[target.whoAmI]++;
                paradoxTimers[target.whoAmI] = 300; // 5 second stack duration
                
                int currentStacks = paradoxStacks[target.whoAmI];
                
                // VFX based on stack count
                int particleCount = 6 + currentStacks * 2;
                
                // Glyph circle intensity grows with stacks
                
                // Multiple watching eyes for higher stacks
                int eyeCount = Math.Min(currentStacks, 3);
                
                // Halo with intensity
                
                // VOID COLLAPSE at 5 stacks!
                if (currentStacks >= 5)
                {
                    TriggerVoidCollapse(target, damageDone);
                    paradoxStacks[target.whoAmI] = 0; // Reset stacks
                }
            }
        }

        private void TriggerVoidCollapse(NPC target, int baseDamage)
        {
            // Massive VFX explosion
            
            // Phase 1: Central void flash
            
            // Phase 2: Cascading halo rings
            
            // Phase 3: Glyph spiral burst
            
            // Phase 4: Eye formation watching outward
            
            // Phase 5: Particle explosion
            
            // Deal 2% of boss current HP as damage
            if (Main.myPlayer == Player.whoAmI)
            {
                int voidDamage = Math.Max(50, (int)(target.life * 0.02f));
                
                // Deal damage to main target
                target.SimpleStrikeNPC(voidDamage, 0, false, 0, null, false, 0, true);
                
                // AOE damage to nearby enemies
                float aoeRadius = 200f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.whoAmI != target.whoAmI && !npc.immortal)
                    {
                        if (Vector2.Distance(npc.Center, target.Center) <= aoeRadius)
                        {
                            npc.SimpleStrikeNPC(voidDamage / 2, 0, false, 0, null, false, 0, true);
                            
                            // VFX on hit targets
                        }
                    }
                }
            }
            
            // Screen shake
            
            // Sound effect
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.8f, Volume = 1.2f }, target.Center);
        }
    }
    #endregion
}
