using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.EnigmaVariations.Accessories
{
    /// <summary>
    /// Pendant of a Thousand Puzzles - Mage Accessory
    /// 
    /// "Arcane Enigma" - Magic attacks have a 15% chance to spawn a "Puzzle Fragment".
    /// Collecting 5 fragments grants "Puzzle Mastery" buff for 8 seconds:
    /// +30% magic damage, -20% mana cost, and magic projectiles leave glowing glyph trails.
    /// Fragments orbit the player and can be collected by touching them.
    /// 
    /// Theme: The accumulation of arcane knowledge, 
    /// each fragment a piece of an unknowable puzzle.
    /// </summary>
    public class PendantOfAThousandPuzzles : ModItem
    {
        // Enigma color palette
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreenFlame = new Color(50, 220, 100);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 3);
            Item.rare = ModContent.RarityType<EnigmaVariationsRarity>();
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<EnigmaAccessoryPlayer>();
            modPlayer.hasPendantOfAThousandPuzzles = true;
            
            // Ambient visual effects
            if (!hideVisual)
            {
                // Gentle arcane glow around the pendant area - every 40 frames
                if (Main.GameUpdateCount % 40 == 0)
                {
                    Vector2 neckPos = player.Center + new Vector2(0, -15f);
                    Vector2 offset = Main.rand.NextVector2Circular(8f, 8f);
                    CustomParticles.GenericGlow(neckPos + offset, EnigmaPurple * 0.5f, 0.15f, 15);
                }
                
                // Fragment counter visual - glowing runes around player, every 30 frames
                if (modPlayer.puzzleFragments > 0 && Main.GameUpdateCount % 30 == 0)
                {
                    float baseAngle = Main.GameUpdateCount * 0.02f;
                    for (int i = 0; i < modPlayer.puzzleFragments; i++)
                    {
                        float angle = baseAngle + MathHelper.TwoPi * i / 5f;
                        Vector2 runePos = player.Center + angle.ToRotationVector2() * 45f;
                        
                        float progress = (float)i / 5f;
                        Color runeColor = EnigmaAccessoryPlayer.GetEnigmaGradient(progress);
                        CustomParticles.GenericFlare(runePos, runeColor * 0.6f, 0.2f, 10);
                    }
                }
                
                // Puzzle Mastery active visuals - DRAMATIC! (but performance optimized)
                if (modPlayer.puzzleMasteryActive)
                {
                    // Intense glyph aura - every 12 frames
                    if (Main.GameUpdateCount % 12 == 0)
                    {
                        float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                        float radius = 35f + Main.rand.NextFloat(20f);
                        Vector2 glyphPos = player.Center + angle.ToRotationVector2() * radius;
                        CustomParticles.Glyph(glyphPos, EnigmaPurple * 0.7f, 0.3f, -1);
                    }
                    
                    // Orbiting eyes - the puzzles watch back - every 25 frames
                    if (Main.GameUpdateCount % 25 == 0)
                    {
                        float eyeAngle = Main.GameUpdateCount * 0.05f + Main.rand.NextFloat() * MathHelper.TwoPi;
                        Vector2 eyePos = player.Center + eyeAngle.ToRotationVector2() * 55f;
                        CustomParticles.EnigmaEyeGaze(eyePos, EnigmaGreenFlame * 0.6f, 0.3f, 
                            (player.Center - eyePos).SafeNormalize(Vector2.UnitX));
                    }
                    
                    // Radiant green flame sparkles - every 15 frames
                    if (Main.GameUpdateCount % 15 == 0)
                    {
                        Vector2 sparkPos = player.Center + Main.rand.NextVector2Circular(40f, 40f);
                        CustomParticles.GenericFlare(sparkPos, EnigmaGreenFlame, 0.35f, 15);
                    }
                    
                    // Music notes floating upward
                    if (Main.rand.NextBool(8))
                    {
                        ThemedParticles.EnigmaMusicNotes(player.Center + Main.rand.NextVector2Circular(20f, 20f), 1, 10f);
                    }
                    
                    // Pulsing light
                    float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.15f) * 0.3f + 0.7f;
                    Lighting.AddLight(player.Center, EnigmaGreenFlame.ToVector3() * 0.5f * pulse);
                    
                    // Glyph trails from magic projectiles
                    foreach (Projectile proj in Main.projectile)
                    {
                        if (proj.active && proj.owner == player.whoAmI && proj.DamageType == DamageClass.Magic)
                        {
                            if (Main.rand.NextBool(4))
                            {
                                CustomParticles.GlyphTrail(proj.Center, proj.velocity, EnigmaPurple * 0.5f, 0.2f);
                            }
                        }
                    }
                }
                else
                {
                    // Normal ambient aura
                    ThemedParticles.EnigmaAura(player.Center, 30f, 0.2f);
                }
            }
        }
        
        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "ArcaneHeader", "Arcane Enigma:")
            {
                OverrideColor = EnigmaPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Magic attacks have 15% chance to spawn puzzle fragments"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Collect 5 fragments to activate Puzzle Mastery (8 seconds):"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "  +30% magic damage, -20% mana cost"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "  Magic projectiles leave glowing glyph trails"));
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A thousand answers to questions never asked'")
            {
                OverrideColor = EnigmaGreenFlame
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonanceEnergies.EnigmaResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<HarmonicCores.HarmonicCoreOfEnigma>(), 1)
                .AddIngredient(ItemID.CelestialEmblem)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
