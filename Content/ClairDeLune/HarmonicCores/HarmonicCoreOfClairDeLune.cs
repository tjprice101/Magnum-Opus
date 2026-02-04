using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Content.ClairDeLune.Projectiles;
using MagnumOpus.Content.ClairDeLune.ResonanceEnergies;

namespace MagnumOpus.Content.ClairDeLune.HarmonicCores
{
    /// <summary>
    /// Harmonic Core of Clair de Lune - Tier 8 (Supreme Final)
    /// The ultimate Harmonic Core - FINAL BOSS tier
    /// Unique Effect: Temporal Overcharge - Attacks create clockwork time fractures
    /// that deal massive bonus damage and slow enemies through time
    /// </summary>
    public class HarmonicCoreOfClairDeLune : ModItem
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
            Item.value = Item.sellPrice(gold: 150); // Higher than Ode to Joy (120g)
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "HarmonicCoreType", "[Tier 8 Harmonic Core - Supreme Final]")
            {
                OverrideColor = ClairDeLuneColors.Crimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "HarmonicCore", "Equip in the Harmonic Core UI (opens with inventory)")
            {
                OverrideColor = ClairDeLuneColors.Crystal
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer1", " ") { OverrideColor = Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "DamageBonus", "+25% All Damage")
            {
                OverrideColor = new Color(255, 180, 180)
            });
            
            tooltips.Add(new TooltipLine(Mod, "CritBonus", "+8% Critical Strike Chance")
            {
                OverrideColor = new Color(255, 200, 200)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer2", " ") { OverrideColor = Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "UniqueHeader", "◁Temporal Overcharge")
            {
                OverrideColor = ClairDeLuneColors.Brass
            });
            tooltips.Add(new TooltipLine(Mod, "UniqueEffect1", "  15% chance on hit to create a temporal fracture")
            {
                OverrideColor = new Color(230, 200, 180)
            });
            tooltips.Add(new TooltipLine(Mod, "UniqueEffect2", "  Fractures deal 200% of weapon damage as clockwork energy")
            {
                OverrideColor = new Color(230, 200, 180)
            });
            tooltips.Add(new TooltipLine(Mod, "UniqueEffect3", "  Enemies struck by fractures are slowed through time for 5s")
            {
                OverrideColor = new Color(230, 200, 180)
            });
            tooltips.Add(new TooltipLine(Mod, "UniqueEffect4", "  Slowed enemies take 20% increased damage from all sources")
            {
                OverrideColor = new Color(230, 200, 180)
            });
            tooltips.Add(new TooltipLine(Mod, "UniqueEffect5", "  Critical hits always create fractures")
            {
                OverrideColor = new Color(255, 215, 150)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer3", " ") { OverrideColor = Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The gears of time bend to your will'")
            {
                OverrideColor = ClairDeLuneColors.MoonlightSilver
            });
        }

        public override void PostUpdate()
        {
            // Temporal clockwork glow
            float mechanicalPulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.2f + 0.8f;
            
            // Multi-color lighting - clockwork temporal palette
            Lighting.AddLight(Item.Center, ClairDeLuneColors.DarkGray.ToVector3() * 0.3f * mechanicalPulse);
            Lighting.AddLight(Item.Center, ClairDeLuneColors.Crimson.ToVector3() * 0.5f * mechanicalPulse);
            Lighting.AddLight(Item.Center, ClairDeLuneColors.Crystal.ToVector3() * 0.2f * mechanicalPulse);
            
            // Clockwork flare particles
            if (Main.GameUpdateCount % 15 == 0)
            {
                CustomParticles.GenericFlare(Item.Center, ClairDeLuneColors.Crimson * 0.7f, 0.55f, 28);
                CustomParticles.GenericFlare(Item.Center, ClairDeLuneColors.Crystal * 0.5f, 0.45f, 22);
            }
            
            // Gear dust
            if (Main.rand.NextBool(8))
            {
                Dust gear = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Silver, 0f, -0.5f, 100, ClairDeLuneColors.DarkGray, 1.2f);
                gear.noGravity = true;
                gear.velocity *= 0.6f;
            }
            
            // Crimson energy sparks
            if (Main.rand.NextBool(12))
            {
                Dust crimson = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.GemRuby, 0f, 0f, 80, ClairDeLuneColors.Crimson, 1.0f);
                crimson.noGravity = true;
                crimson.velocity *= 0.5f;
            }
            
            // Crystal shimmer
            if (Main.rand.NextBool(18))
            {
                Dust crystal = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.GemDiamond, 0f, 0f, 100, ClairDeLuneColors.Crystal, 0.9f);
                crystal.noGravity = true;
                crystal.velocity *= 0.4f;
            }
            
            // Brass lightning sparks
            if (Main.rand.NextBool(22))
            {
                Dust brass = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Enchanted_Gold, 0f, 0f, 100, ClairDeLuneColors.Brass, 0.8f);
                brass.noGravity = true;
                brass.velocity *= 0.3f;
            }
            
            // Occasional lightning spark effect
            if (Main.rand.NextBool(35))
            {
                Dust lightning = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Electric, Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f), 50, ClairDeLuneColors.ElectricBlue, 0.7f);
                lightning.noGravity = true;
            }
        }
    }
}
