using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.Fate;
using MagnumOpus.Content.Fate.ResonanceEnergies;
using MagnumOpus.Content.Fate.HarmonicCores;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Content.Materials.EnemyDrops;

namespace MagnumOpus.Content.Fate.Accessories
{
    /// <summary>
    /// Astral Conduit - Magic accessory for Fate theme.
    /// Channels cosmic energy to amplify magic damage and mana regeneration.
    /// Every magic attack has a chance to trigger a cosmic flare that chains to nearby enemies.
    /// </summary>
    public class AstralConduit : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 3);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<AstralConduitPlayer>();
            modPlayer.hasAstralConduit = true;
            
            // +20% magic damage
            player.GetDamage(DamageClass.Magic) += 0.20f;
            
            // +15% mana regeneration
            player.manaRegenBonus += 25;
            
            // Reduced mana cost
            player.manaCost -= 0.10f;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "MagicBoost", "+20% magic damage")
            {
                OverrideColor = new Color(180, 100, 255)
            });

            tooltips.Add(new TooltipLine(Mod, "ManaRegen", "+25 mana regeneration")
            {
                OverrideColor = new Color(100, 150, 255)
            });

            tooltips.Add(new TooltipLine(Mod, "ManaCost", "-10% mana cost")
            {
                OverrideColor = new Color(120, 180, 255)
            });

            tooltips.Add(new TooltipLine(Mod, "AstralResonance", "Magic crits grant 'Astral Resonance' for 3s (+8% magic damage, +5% magic crit)")
            {
                OverrideColor = FatePalette.DarkPink
            });

            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The stars themselves bend to your will'")
            {
                OverrideColor = new Color(255, 150, 180)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<FateResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfFate>(), 5)
                .AddIngredient(ModContent.ItemType<FateEssence>(), 5)
                .AddIngredient(ModContent.ItemType<ShardOfFatesTempo>(), 5)
                .AddIngredient(ItemID.SoulofSight, 10)
                .AddIngredient(ItemID.FragmentNebula, 8)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }

    public class AstralConduitPlayer : ModPlayer
    {
        public bool hasAstralConduit = false;
        public int astralResonanceTimer = 0;
        
        private const int AstralResonanceDuration = 180; // 3 seconds
        
        public override void ResetEffects()
        {
            hasAstralConduit = false;
        }

        public override void PostUpdateEquips()
        {
            if (!hasAstralConduit)
            {
                astralResonanceTimer = 0;
                return;
            }

            if (astralResonanceTimer > 0)
            {
                astralResonanceTimer--;
                Player.GetDamage(DamageClass.Magic) += 0.08f;
                Player.GetCritChance(DamageClass.Magic) += 5;
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!hasAstralConduit) return;
            if (!proj.CountsAsClass(DamageClass.Magic)) return;
            if (!hit.Crit) return;
            
            astralResonanceTimer = AstralResonanceDuration;
        }
    }
}
