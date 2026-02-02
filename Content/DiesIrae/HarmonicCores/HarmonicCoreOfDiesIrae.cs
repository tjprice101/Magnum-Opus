using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using MagnumOpus.Content.DiesIrae.ResonanceEnergies;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common;

namespace MagnumOpus.Content.DiesIrae.HarmonicCores
{
    /// <summary>
    /// Harmonic Core of Dies Irae - Tier 7 (Supreme)
    /// Unique Effect: Final Judgment - Critical hits condemn enemies, causing them to take 
    /// greatly increased damage and eventually explode in hellfire
    /// </summary>
    public class HarmonicCoreOfDiesIrae : ModItem
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
            Item.value = Item.sellPrice(gold: 100);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "HarmonicCoreType", "[Tier 7 Harmonic Core - Supreme]")
            {
                OverrideColor = new Color(139, 0, 0)
            });
            
            tooltips.Add(new TooltipLine(Mod, "HarmonicCore", "Equip in the Harmonic Core UI (opens with inventory)")
            {
                OverrideColor = new Color(139, 0, 0)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer1", " ") { OverrideColor = Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "DamageBonus", "+18% All Damage")
            {
                OverrideColor = new Color(120, 200, 120)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer2", " ") { OverrideColor = Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "UniqueHeader", "◁Final Judgment")
            {
                OverrideColor = new Color(255, 100, 0)
            });
            tooltips.Add(new TooltipLine(Mod, "UniqueEffect1", "  Critical hits condemn enemies for 5 seconds")
            {
                OverrideColor = new Color(255, 150, 100)
            });
            tooltips.Add(new TooltipLine(Mod, "UniqueEffect2", "  Condemned enemies take 25% increased damage")
            {
                OverrideColor = new Color(255, 150, 100)
            });
            tooltips.Add(new TooltipLine(Mod, "UniqueEffect3", "  When condemnation expires, enemies explode in hellfire")
            {
                OverrideColor = new Color(255, 150, 100)
            });
            tooltips.Add(new TooltipLine(Mod, "UniqueEffect4", "  Hellfire explosion deals 50% of all damage taken while condemned")
            {
                OverrideColor = new Color(255, 150, 100)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer3", " ") { OverrideColor = Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'None escape the day of wrath'")
            {
                OverrideColor = new Color(139, 0, 0)
            });
        }

        public override void PostUpdate()
        {
            // Powerful infernal glow
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.06f) * 0.2f + 0.8f;
            Lighting.AddLight(Item.Center, 0.9f * pulse, 0.25f * pulse, 0.1f * pulse);
            
            // Infernal flare particles
            if (Main.GameUpdateCount % 18 == 0)
            {
                CustomParticles.GenericFlare(Item.Center, new Color(255, 100, 0) * 0.7f, 0.5f, 25);
                CustomParticles.GenericFlare(Item.Center, new Color(139, 0, 0) * 0.5f, 0.4f, 20);
            }
            
            // Fire particles
            if (Main.rand.NextBool(10))
            {
                Dust fire = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Torch, 0f, -1f, 100, default, 1.4f);
                fire.noGravity = true;
                fire.velocity *= 0.7f;
            }
            
            // Blood red embers
            if (Main.rand.NextBool(20))
            {
                Dust ember = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.CrimsonTorch, 0f, 0f, 100, default, 1.1f);
                ember.noGravity = true;
                ember.velocity *= 0.4f;
            }
        }
    }
}
