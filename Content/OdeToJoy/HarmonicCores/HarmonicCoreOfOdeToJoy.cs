using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;

namespace MagnumOpus.Content.OdeToJoy.HarmonicCores
{
    /// <summary>
    /// Harmonic Core of Ode to Joy - Tier 7 (Supreme)
    /// Unique Effect: Triumphant Chorus - Attacks have a chance to spawn joyous spirits
    /// that heal allies and debuff enemies
    /// </summary>
    public class HarmonicCoreOfOdeToJoy : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.scale = 1.25f;
            Item.maxStack = 1;
            Item.value = Item.sellPrice(gold: 120);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "HarmonicCoreType", "[Tier 7 Harmonic Core - Supreme]")
            {
                OverrideColor = new Color(76, 175, 80)
            });
            
            tooltips.Add(new TooltipLine(Mod, "HarmonicCore", "Equip in the Harmonic Core UI (opens with inventory)")
            {
                OverrideColor = new Color(76, 175, 80)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer1", " ") { OverrideColor = Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "DamageBonus", "+20% All Damage")
            {
                OverrideColor = new Color(120, 200, 120)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer2", " ") { OverrideColor = Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "UniqueHeader", "◁Triumphant Chorus")
            {
                OverrideColor = new Color(255, 215, 0)
            });
            tooltips.Add(new TooltipLine(Mod, "UniqueEffect1", "  10% chance on hit to spawn a joyous spirit")
            {
                OverrideColor = new Color(255, 225, 150)
            });
            tooltips.Add(new TooltipLine(Mod, "UniqueEffect2", "  Spirits heal nearby players for 15 HP")
            {
                OverrideColor = new Color(255, 225, 150)
            });
            tooltips.Add(new TooltipLine(Mod, "UniqueEffect3", "  Spirits inflict Confused and Slow on nearby enemies")
            {
                OverrideColor = new Color(255, 225, 150)
            });
            tooltips.Add(new TooltipLine(Mod, "UniqueEffect4", "  Enemies affected by spirits take 15% increased damage")
            {
                OverrideColor = new Color(255, 225, 150)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer3", " ") { OverrideColor = Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'All creatures become friends through joy'")
            {
                OverrideColor = new Color(255, 182, 193)
            });
        }

        public override void PostUpdate()
        {
            // Joyous nature glow
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.06f) * 0.2f + 0.8f;
            Lighting.AddLight(Item.Center, 0.45f * pulse, 0.9f * pulse, 0.5f * pulse);
            
            // Nature flare particles
            if (Main.GameUpdateCount % 18 == 0)
            {
                CustomParticles.GenericFlare(Item.Center, new Color(76, 175, 80) * 0.7f, 0.5f, 25);
                CustomParticles.GenericFlare(Item.Center, new Color(255, 215, 0) * 0.5f, 0.4f, 20);
            }
            
            // Nature particles
            if (Main.rand.NextBool(10))
            {
                Dust nature = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.JungleGrass, 0f, -1f, 100, default, 1.4f);
                nature.noGravity = true;
                nature.velocity *= 0.7f;
            }
            
            // Rose petals
            if (Main.rand.NextBool(20))
            {
                Dust petal = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.PinkTorch, 0f, 0f, 100, default, 1.1f);
                petal.noGravity = true;
                petal.velocity *= 0.4f;
            }
            
            // Golden sparkles
            if (Main.rand.NextBool(25))
            {
                Dust gold = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.GoldCoin, 0f, 0f, 100, default, 0.9f);
                gold.noGravity = true;
                gold.velocity *= 0.3f;
            }
        }
    }
}
