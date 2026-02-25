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
            // +12% all damage
            player.GetDamage(DamageClass.Generic) += 0.12f;
            
            // Enable Paradox debuff mechanic
            player.GetModPlayer<PuzzleFragmentPlayer>().puzzleFragmentEquipped = true;
            
            // Ambient VFX - mysterious arcane particles
            if (!hideVisual && Main.rand.NextBool(12))
            {
                Vector2 offset = Main.rand.NextVector2Circular(25f, 25f);
                Color particleColor = Color.Lerp(EnigmaColors.DeepPurple, EnigmaColors.GreenFlame, Main.rand.NextFloat());
                
                CustomParticles.GenericFlare(player.Center + offset, particleColor * 0.6f, 0.25f, 15);
                
                // Occasional watching eye
                if (Main.rand.NextBool(5))
                {
                    CustomParticles.EnigmaEyeGaze(player.Center + offset, EnigmaColors.Purple, 0.3f, null);
                }
            }
            
            // Orbiting glyphs
            if (!hideVisual && Main.rand.NextBool(20))
            {
                float angle = Main.GameUpdateCount * 0.03f;
                Vector2 glyphPos = player.Center + angle.ToRotationVector2() * 35f;
                CustomParticles.Glyph(glyphPos, EnigmaColors.Purple * 0.5f, 0.3f, -1);
            }
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
            tooltips.Add(new TooltipLine(Mod, "DamageBoost", "+12% damage")
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
                OverrideColor = new Color(100, 100, 100)
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
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 offset = angle.ToRotationVector2() * 20f;
                    Color color = Color.Lerp(EnigmaColors.DeepPurple, EnigmaColors.GreenFlame, (float)i / 8f);
                    CustomParticles.GenericFlare(target.Center + offset, color, 0.35f, 15);
                }
                
                // Glyph burst
                CustomParticles.GlyphBurst(target.Center, EnigmaColors.Purple, 4, 3f);
                
                // Watching eye at center
                CustomParticles.EnigmaEyeGaze(target.Center, EnigmaColors.GreenFlame, 0.5f, null);
                
                // Halo ring
                CustomParticles.HaloRing(target.Center, EnigmaColors.Purple, 0.4f, 18);
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
            // +18% all damage
            player.GetDamage(DamageClass.Generic) += 0.18f;
            
            // +8% crit chance
            player.GetCritChance(DamageClass.Generic) += 8;
            
            // Enable Void Paradox mechanic
            player.GetModPlayer<RiddleOfTheVoidPlayer>().riddleOfTheVoidEquipped = true;
            
            // Elaborate ambient VFX - void energy swirling
            if (!hideVisual)
            {
                // Constant void particles
                if (Main.rand.NextBool(6))
                {
                    Vector2 offset = Main.rand.NextVector2Circular(40f, 40f);
                    Vector2 velocity = (player.Center - (player.Center + offset)).SafeNormalize(Vector2.Zero) * 1.5f;
                    Color color = Color.Lerp(EnigmaColors.VoidBlack, EnigmaColors.Purple, Main.rand.NextFloat());
                    
                    var particle = new GenericGlowParticle(
                        player.Center + offset, velocity,
                        color * 0.7f, 0.3f, 20, true);
                    MagnumParticleHandler.SpawnParticle(particle);
                }
                
                // Orbiting glyph circle
                if (Main.GameUpdateCount % 8 == 0)
                {
                    int glyphCount = 5;
                    for (int i = 0; i < glyphCount; i++)
                    {
                        float angle = Main.GameUpdateCount * 0.02f + MathHelper.TwoPi * i / glyphCount;
                        Vector2 glyphPos = player.Center + angle.ToRotationVector2() * 45f;
                        CustomParticles.Glyph(glyphPos, EnigmaColors.DeepPurple * 0.6f, 0.35f, -1);
                    }
                }
                
                // Watching eyes occasionally appear
                if (Main.rand.NextBool(25))
                {
                    Vector2 eyeOffset = Main.rand.NextVector2Circular(50f, 50f);
                    CustomParticles.EnigmaEyeGaze(player.Center + eyeOffset, EnigmaColors.GreenFlame, 0.4f, null);
                }
                
                // Void shimmer flares
                if (Main.rand.NextBool(15))
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 pos = player.Center + angle.ToRotationVector2() * Main.rand.NextFloat(20f, 50f);
                    CustomParticles.GenericFlare(pos, EnigmaColors.GreenFlame * 0.5f, 0.2f, 12);
                }
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<PuzzleFragment>()
                .AddIngredient<HarmonicCoreOfEnigma>(2)
                .AddIngredient<EnigmaResonantEnergy>(15)
                .AddIngredient<RemnantOfMysteries>(5)
                .AddIngredient(ItemID.FragmentNebula, 10)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "DamageBoost", "+18% damage, +8% critical strike chance")
            {
                OverrideColor = EnigmaColors.Purple
            });
            tooltips.Add(new TooltipLine(Mod, "Paradox", "12% chance on hit to apply 'Paradox' debuffs")
            {
                OverrideColor = EnigmaColors.GreenFlame
            });
            tooltips.Add(new TooltipLine(Mod, "Stacking", "Paradox can stack up to 5 times")
            {
                OverrideColor = EnigmaColors.DeepPurple
            });
            tooltips.Add(new TooltipLine(Mod, "VoidCollapse", "At 5 stacks, triggers 'Void Collapse' - massive damage explosion")
            {
                OverrideColor = EnigmaColors.GreenFlame
            });
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The answer was void all along'")
            {
                OverrideColor = new Color(100, 100, 100)
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
            
            // 12% chance for Paradox
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
                for (int i = 0; i < particleCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / particleCount;
                    Vector2 offset = angle.ToRotationVector2() * (15f + currentStacks * 5f);
                    float progress = (float)i / particleCount;
                    Color color = Color.Lerp(EnigmaColors.DeepPurple, EnigmaColors.GreenFlame, progress);
                    CustomParticles.GenericFlare(target.Center + offset, color, 0.3f + currentStacks * 0.05f, 18);
                }
                
                // Glyph circle intensity grows with stacks
                CustomParticles.GlyphBurst(target.Center, EnigmaColors.Purple, 3 + currentStacks, 3f + currentStacks * 0.5f);
                
                // Multiple watching eyes for higher stacks
                int eyeCount = Math.Min(currentStacks, 3);
                for (int i = 0; i < eyeCount; i++)
                {
                    Vector2 eyeOffset = Main.rand.NextVector2Circular(30f, 30f);
                    CustomParticles.EnigmaEyeGaze(target.Center + eyeOffset, EnigmaColors.GreenFlame, 0.45f, Player.Center);
                }
                
                // Halo with intensity
                CustomParticles.HaloRing(target.Center, EnigmaColors.Purple * (0.5f + currentStacks * 0.1f), 0.35f + currentStacks * 0.1f, 18);
                
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
            CustomParticles.GenericFlare(target.Center, Color.White, 1.5f, 30);
            CustomParticles.GenericFlare(target.Center, EnigmaColors.GreenFlame, 1.2f, 28);
            CustomParticles.GenericFlare(target.Center, EnigmaColors.DeepPurple, 1.0f, 25);
            
            // Phase 2: Cascading halo rings
            for (int ring = 0; ring < 10; ring++)
            {
                float progress = ring / 10f;
                Color ringColor = Color.Lerp(EnigmaColors.VoidBlack, EnigmaColors.GreenFlame, progress);
                CustomParticles.HaloRing(target.Center, ringColor, 0.4f + ring * 0.15f, 20 + ring * 3);
            }
            
            // Phase 3: Glyph spiral burst
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                float radius = 30f + i * 8f;
                Vector2 pos = target.Center + angle.ToRotationVector2() * radius;
                CustomParticles.Glyph(pos, EnigmaColors.Purple, 0.5f, -1);
            }
            
            // Phase 4: Eye formation watching outward
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 eyePos = target.Center + angle.ToRotationVector2() * 50f;
                Vector2 lookDir = angle.ToRotationVector2();
                CustomParticles.EnigmaEyeGaze(eyePos, EnigmaColors.GreenFlame, 0.6f, eyePos + lookDir * 100f);
            }
            
            // Phase 5: Particle explosion
            CustomParticles.ExplosionBurst(target.Center, EnigmaColors.Purple, 20, 12f);
            CustomParticles.ExplosionBurst(target.Center, EnigmaColors.GreenFlame, 15, 10f);
            
            // Deal massive damage (200% of base damage)
            if (Main.myPlayer == Player.whoAmI)
            {
                int voidDamage = (int)(baseDamage * 2.0f);
                
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
                            CustomParticles.GenericFlare(npc.Center, EnigmaColors.GreenFlame, 0.5f, 15);
                            CustomParticles.GlyphBurst(npc.Center, EnigmaColors.Purple, 3, 2f);
                        }
                    }
                }
            }
            
            // Screen shake
            MagnumScreenEffects.AddScreenShake(12f);
            
            // Sound effect
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.8f, Volume = 1.2f }, target.Center);
        }
    }
    #endregion
}
