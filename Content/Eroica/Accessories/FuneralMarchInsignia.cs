using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.Eroica.Enemies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Eroica.Accessories
{
    /// <summary>
    /// Funeral March Insignia - Mage accessory.
    /// Heroic Encore: Taking fatal damage consumes ALL mana, grants 3s invulnerability + 2x magic damage (180s cooldown).
    /// Mana regeneration triples when below 20% mana.
    /// </summary>
    public class FuneralMarchInsignia : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 3);
            Item.rare = ModContent.RarityType<EroicaRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<EroicaAccessoryPlayer>();
            modPlayer.hasFuneralMarchInsignia = true;
            
            // Ambient particles - funeral march theme with dark flames
            if (!hideVisual && Main.rand.NextBool(7))
            {
                // Dark purple/crimson flame
                Vector2 offset = Main.rand.NextVector2Circular(25f, 25f);
                int dustType = Main.rand.NextBool() ? DustID.CrimsonTorch : DustID.Shadowflame;
                Dust flame = Dust.NewDustPerfect(player.Center + offset, dustType, 
                    new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1.5f), 100, default, 1.1f);
                flame.noGravity = true;
            }
            
            // Heroic Encore active visual - brilliant prismatic gem effect
            if (modPlayer.heroicEncoreActive)
            {
                // Intense golden/red aura
                Lighting.AddLight(player.Center, 1f, 0.6f, 0.2f);
                
                // Eroica themed impact burst for dramatic effect
                if (Main.rand.NextBool(3))
                {
                    ThemedParticles.EroicaSparkles(player.Center, 5, 40f);
                }
                
                if (Main.rand.NextBool(2))
                {
                    // Protective flame ring
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 ringPos = player.Center + new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle)) * 40f;
                    Dust ring = Dust.NewDustPerfect(ringPos, DustID.GoldCoin, Vector2.Zero, 0, default, 1.5f);
                    ring.noGravity = true;
                }
            }
            else if (!hideVisual)
            {
                // Normal state - subtle Eroica aura
                ThemedParticles.EroicaAura(player.Center, 25f);
            }
            
            // Low mana visual (when mana regen is tripled)
            float manaPercent = (float)player.statMana / player.statManaMax2;
            if (!hideVisual && manaPercent < 0.2f && Main.rand.NextBool(4))
            {
                // Desperate energy gathering
                Vector2 offset = Main.rand.NextVector2Circular(40f, 40f);
                Dust mana = Dust.NewDustPerfect(player.Center + offset, DustID.Smoke, 
                    (player.Center - (player.Center + offset)).SafeNormalize(Vector2.Zero) * 2f, 100, Color.Black, 1f);
                mana.noGravity = true;
            }
            
            // Cooldown visual indicator (subtle when on cooldown)
            if (!hideVisual && modPlayer.heroicEncoreCooldown > 0 && Main.rand.NextBool(30))
            {
                Dust cooldown = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(20f, 20f),
                    DustID.Smoke, new Vector2(0, -0.5f), 150, Color.Gray, 0.8f);
                cooldown.noGravity = true;
            }
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            var player = Main.LocalPlayer;
            var modPlayer = player.GetModPlayer<EroicaAccessoryPlayer>();
            
            tooltips.Add(new TooltipLine(Mod, "EncoreHeader", "Heroic Encore:")
            {
                OverrideColor = new Color(200, 100, 150)
            });
            
            tooltips.Add(new TooltipLine(Mod, "DeathPrevention", "Fatal damage instead consumes ALL mana")
            {
                OverrideColor = new Color(255, 100, 100)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Invulnerability", "Grants 3 seconds of invulnerability and doubled magic damage")
            {
                OverrideColor = new Color(255, 200, 100)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Cooldown", "180 second cooldown")
            {
                OverrideColor = new Color(180, 180, 180)
            });
            
            tooltips.Add(new TooltipLine(Mod, "ManaRegen", "Mana regeneration triples when below 20% mana")
            {
                OverrideColor = new Color(150, 150, 255)
            });
            
            // Show current cooldown if active
            if (modPlayer.heroicEncoreCooldown > 0)
            {
                int secondsLeft = modPlayer.heroicEncoreCooldown / 60;
                tooltips.Add(new TooltipLine(Mod, "CooldownTimer", $"Cooldown: {secondsLeft}s remaining")
                {
                    OverrideColor = new Color(255, 100, 100)
                });
            }
            else
            {
                tooltips.Add(new TooltipLine(Mod, "Ready", "Ready!")
                {
                    OverrideColor = new Color(100, 255, 100)
                });
            }
            
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The march continues, even beyond death'")
            {
                OverrideColor = new Color(120, 80, 100)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<EroicasResonantEnergy>(), 5)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfEroica>(), 5)
                .AddIngredient(ModContent.ItemType<ShardOfTriumphsTempo>(), 5)
                .AddIngredient(ItemID.SoulofFright, 5)
                .AddIngredient(ItemID.SoulofMight, 15)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }
}
