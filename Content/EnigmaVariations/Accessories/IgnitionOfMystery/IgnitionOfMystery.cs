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
    /// Ignition of Mystery - Melee Accessory
    /// 
    /// "Mysteries Unveiled" - Every melee hit builds mystery stacks (up to 10).
    /// At max stacks, your next melee attack unleashes a massive eye burst that 
    /// marks all nearby enemies with "Watched" debuff (15% increased damage taken).
    /// Additionally, melee speed increases by 2% per stack (up to 20%).
    /// 
    /// Theme: The revelation of hidden truths through combat, 
    /// each strike bringing the unknown closer to light.
    /// </summary>
    public class IgnitionOfMystery : ModItem
    {
        // Enigma color palette
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreenFlame = new Color(50, 220, 100);
        
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
            modPlayer.hasIgnitionOfMystery = true;
            
            // Ambient visual effects
            if (!hideVisual)
            {
                // Mysterious flame particles around player - every 30 frames
                if (Main.GameUpdateCount % 30 == 0)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(25f, 35f);
                    float progress = Main.rand.NextFloat();
                    Color flameColor = EnigmaAccessoryPlayer.GetEnigmaGradient(progress);
                    
                    // Upward drifting flame
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -1.5f - Main.rand.NextFloat(0.5f));
                    CustomParticles.GenericGlow(player.Center + offset, flameColor * 0.6f, 0.25f, 20);
                }
                
                // Stack indicator visuals - more intense as stacks build
                if (modPlayer.mysteryStacks > 0)
                {
                    float intensity = (float)modPlayer.mysteryStacks / 10f;
                    
                    // Orbiting mystery flames
                    if (Main.rand.NextFloat() < intensity * 0.4f)
                    {
                        float orbitAngle = Main.GameUpdateCount * 0.03f + Main.rand.NextFloat() * MathHelper.TwoPi;
                        float orbitRadius = 30f + intensity * 15f;
                        Vector2 flamePos = player.Center + orbitAngle.ToRotationVector2() * orbitRadius;
                        
                        Color flameColor = EnigmaAccessoryPlayer.GetEnigmaGradient(intensity);
                        CustomParticles.GenericFlare(flamePos, flameColor, 0.25f + intensity * 0.2f, 12);
                    }
                    
                    // At high stacks, eyes begin to watch
                    if (modPlayer.mysteryStacks >= 6 && Main.rand.NextBool(15))
                    {
                        Vector2 eyePos = player.Center + Main.rand.NextVector2Circular(40f, 40f);
                        CustomParticles.EnigmaEyeGaze(eyePos, EnigmaGreenFlame * 0.5f, 0.25f * intensity, null);
                    }
                    
                    // Glyphs at max stacks
                    if (modPlayer.mysteryStacks >= 9 && Main.rand.NextBool(10))
                    {
                        CustomParticles.Glyph(player.Center + Main.rand.NextVector2Circular(35f, 35f), 
                            EnigmaPurple * 0.6f, 0.25f, -1);
                    }
                    
                    // Pulsing aura light based on stacks
                    float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.1f) * 0.2f + 0.8f;
                    Lighting.AddLight(player.Center, EnigmaAccessoryPlayer.GetEnigmaGradient(intensity).ToVector3() * 0.3f * intensity * pulse);
                }
                
                // Enigma aura - only spawn every 20 frames to reduce particle load
                if (Main.GameUpdateCount % 20 == 0)
                    ThemedParticles.EnigmaAura(player.Center, 35f, 0.3f);
            }
        }
        
        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "MysteryHeader", "Mysteries Unveiled:")
            {
                OverrideColor = EnigmaGreenFlame
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Melee attacks build mystery stacks (max 10)"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Each stack grants +2% melee speed (up to +20%)"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "At max stacks, unleash a devastating eye burst"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Marked enemies take 15% increased damage for 5 seconds"));
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The unknown fears those who seek it'")
            {
                OverrideColor = EnigmaPurple
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonanceEnergies.EnigmaResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<HarmonicCores.HarmonicCoreOfEnigma>(), 1)
                .AddIngredient(ItemID.MagmaStone)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
