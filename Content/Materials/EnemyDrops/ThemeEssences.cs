using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Materials.EnemyDrops
{
    /// <summary>
    /// Lunar Essence - Post-Moon Lord theme material.
    /// Drops from Moonlight enemies (2%).
    /// </summary>
    public class LunarEssence : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
            ItemID.Sets.ItemNoGravity[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 3);
            Item.rare = ModContent.RarityType<MoonlightSonataRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The moon's quiet sorrow, crystallized'") { OverrideColor = new Color(140, 100, 200) });
        }

        public override void PostUpdate()
        {
            // Moonlight purple-blue glow
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.04f) * 0.2f + 0.6f;
            Lighting.AddLight(Item.Center, 0.4f * pulse, 0.35f * pulse, 0.7f * pulse);

            if (Main.rand.NextBool(18))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.PurpleTorch, 0f, -0.4f, 80, default, 0.6f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
        }
    }

    /// <summary>
    /// Valor Essence - Post-Moon Lord theme material.
    /// Drops from Eroica enemies (2%).
    /// </summary>
    public class ValorEssence : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
            ItemID.Sets.ItemNoGravity[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 3);
            Item.rare = ModContent.RarityType<EroicaRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The hero\'s triumphant spirit, distilled'") { OverrideColor = new Color(200, 50, 50) });
        }

        public override void PostUpdate()
        {
            // Heroic scarlet-gold glow
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.06f) * 0.2f + 0.65f;
            Lighting.AddLight(Item.Center, 0.75f * pulse, 0.45f * pulse, 0.2f * pulse);

            if (Main.rand.NextBool(18))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.GoldFlame, 0f, -0.5f, 60, default, 0.7f);
                dust.noGravity = true;
                dust.velocity *= 0.35f;
            }
        }
    }

    /// <summary>
    /// Bell Essence - Post-Moon Lord theme material.
    /// Drops from La Campanella enemies (2%).
    /// </summary>
    public class BellEssence : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
            ItemID.Sets.ItemNoGravity[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 3);
            Item.rare = ModContent.RarityType<LaCampanellaRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The infernal bell\'s resonance, captured'") { OverrideColor = new Color(255, 140, 40) });
        }

        public override void PostUpdate()
        {
            // Infernal bell orange-black glow
            float flicker = Main.rand.NextFloat(0.9f, 1.1f);
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.08f) * 0.2f + 0.55f;
            Lighting.AddLight(Item.Center, 0.8f * pulse * flicker, 0.45f * pulse * flicker, 0.1f * pulse * flicker);

            if (Main.rand.NextBool(15))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Torch, 0f, -0.6f, 80, default, 0.6f);
                dust.noGravity = true;
            }
        }
    }

    /// <summary>
    /// Mystery Essence - Post-Moon Lord theme material.
    /// Drops from Enigma enemies (2%).
    /// </summary>
    public class MysteryEssence : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
            ItemID.Sets.ItemNoGravity[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 3);
            Item.rare = ModContent.RarityType<EnigmaVariationsRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'An unknowable secret, given form'") { OverrideColor = new Color(140, 60, 200) });
        }

        public override void PostUpdate()
        {
            // Enigma purple-green glow
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.05f) * 0.25f + 0.5f;
            float shift = (float)System.Math.Sin(Main.GameUpdateCount * 0.02f);
            Lighting.AddLight(Item.Center, 0.4f * pulse, (0.3f + shift * 0.2f) * pulse, (0.55f - shift * 0.15f) * pulse);

            if (Main.rand.NextBool(18))
            {
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.GreenTorch;
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, dustType, 0f, -0.3f, 100, default, 0.55f);
                dust.noGravity = true;
            }
        }
    }

    /// <summary>
    /// Grace Essence - Post-Moon Lord theme material.
    /// Drops from Swan Lake enemies (2%).
    /// </summary>
    public class GraceEssence : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
            ItemID.Sets.ItemNoGravity[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 3);
            Item.rare = ModContent.RarityType<SwanRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Elegance incarnate, preserved eternally'") { OverrideColor = new Color(240, 240, 255) });
        }

        public override void PostUpdate()
        {
            // Swan Lake white-rainbow shimmer
            float hue = (Main.GameUpdateCount * 0.01f) % 1f;
            Color shimmer = Main.hslToRgb(hue, 0.3f, 0.8f);
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.04f) * 0.15f + 0.7f;
            Lighting.AddLight(Item.Center, shimmer.ToVector3() * pulse);

            if (Main.rand.NextBool(18))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Enchanted_Gold, 0f, -0.4f, 50, Color.White, 0.6f);
                dust.noGravity = true;
            }
        }
    }

    /// <summary>
    /// Fate Essence - Post-Moon Lord theme material.
    /// Drops from Fate enemies (2%).
    /// </summary>
    public class FateEssence : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
            ItemID.Sets.ItemNoGravity[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Destiny\'s celestial power, condensed'") { OverrideColor = new Color(180, 40, 80) });
        }

        public override void PostUpdate()
        {
            // Fate cosmic black-pink-crimson glow
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.06f) * 0.25f + 0.65f;
            float shift = (float)System.Math.Sin(Main.GameUpdateCount * 0.03f);
            Lighting.AddLight(Item.Center, (0.6f + shift * 0.2f) * pulse, 0.2f * pulse, (0.4f - shift * 0.1f) * pulse);

            if (Main.rand.NextBool(12))
            {
                Color[] colors = { new Color(180, 50, 100), new Color(255, 60, 80), Color.White };
                Color particleColor = colors[Main.rand.Next(colors.Length)];
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.PinkTorch, Main.rand.NextFloat(-0.5f, 0.5f), -0.5f, 60, particleColor, 0.7f);
                dust.noGravity = true;
            }
        }
    }

    /// <summary>
    /// Nachtmusik Essence - Post-Fate theme material.
    /// Drops from Nachtmusik enemies.
    /// </summary>
    public class NachtmusikEssence : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
            ItemID.Sets.ItemNoGravity[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 6);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A fragment of the night\'s serenade, still humming'") { OverrideColor = new Color(100, 120, 200) });
        }

        public override void PostUpdate()
        {
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.04f) * 0.2f + 0.55f;
            Lighting.AddLight(Item.Center, 0.3f * pulse, 0.35f * pulse, 0.7f * pulse);

            if (Main.rand.NextBool(16))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.BlueTorch, 0f, -0.4f, 80, default, 0.6f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
        }
    }

    /// <summary>
    /// Wrath Essence - Post-Fate theme material.
    /// Drops from Dies Irae enemies.
    /// </summary>
    public class WrathEssence : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
            ItemID.Sets.ItemNoGravity[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 7);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Fury unbound, judgment incarnate'") { OverrideColor = new Color(200, 50, 30) });
        }

        public override void PostUpdate()
        {
            float flicker = Main.rand.NextFloat(0.85f, 1.15f);
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.07f) * 0.25f + 0.6f;
            Lighting.AddLight(Item.Center, 0.8f * pulse * flicker, 0.2f * pulse * flicker, 0.1f * pulse * flicker);

            if (Main.rand.NextBool(12))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Torch, Main.rand.NextFloat(-0.3f, 0.3f), -0.6f, 60, default, 0.7f);
                dust.noGravity = true;
            }
        }
    }

    /// <summary>
    /// Joy Essence - Post-Fate theme material.
    /// Drops from Ode to Joy enemies.
    /// </summary>
    public class JoyEssence : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
            ItemID.Sets.ItemNoGravity[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 8);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Pure celebration, crystallized into golden light'") { OverrideColor = new Color(255, 200, 50) });
        }

        public override void PostUpdate()
        {
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.05f) * 0.2f + 0.65f;
            Lighting.AddLight(Item.Center, 0.8f * pulse, 0.65f * pulse, 0.2f * pulse);

            if (Main.rand.NextBool(14))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.GoldFlame, 0f, -0.5f, 50, default, 0.65f);
                dust.noGravity = true;
                dust.velocity *= 0.35f;
            }
        }
    }

    /// <summary>
    /// Lune Essence - Post-Fate theme material.
    /// Drops from Clair de Lune enemies.
    /// </summary>
    public class LuneEssence : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
            ItemID.Sets.ItemNoGravity[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Moonlit reverie made tangible, soft as a lullaby'") { OverrideColor = new Color(150, 200, 255) });
        }

        public override void PostUpdate()
        {
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.03f) * 0.2f + 0.6f;
            Lighting.AddLight(Item.Center, 0.45f * pulse, 0.6f * pulse, 0.8f * pulse);

            if (Main.rand.NextBool(18))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.BlueTorch, 0f, -0.3f, 90, new Color(150, 200, 255), 0.55f);
                dust.noGravity = true;
                dust.velocity *= 0.25f;
            }
        }
    }
}
