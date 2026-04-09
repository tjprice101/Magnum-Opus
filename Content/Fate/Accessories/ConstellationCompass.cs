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
    /// Constellation Compass - Ranged accessory for Fate theme.
    /// Guides projectiles toward their cosmic destiny with enhanced homing.
    /// Increases projectile velocity and grants piercing to ranged attacks.
    /// </summary>
    public class ConstellationCompass : ModItem
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
            var modPlayer = player.GetModPlayer<ConstellationCompassPlayer>();
            modPlayer.hasConstellationCompass = true;
            
            // +18% ranged damage
            player.GetDamage(DamageClass.Ranged) += 0.18f;
            
            // +15% ranged critical strike chance
            player.GetCritChance(DamageClass.Ranged) += 15;
            
            // Increased projectile speed
            player.GetAttackSpeed(DamageClass.Ranged) += 0.12f;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "RangedBoost", "+18% ranged damage")
            {
                OverrideColor = new Color(255, 180, 100)
            });

            tooltips.Add(new TooltipLine(Mod, "CritBoost", "+15% ranged critical strike chance")
            {
                OverrideColor = new Color(255, 200, 120)
            });

            tooltips.Add(new TooltipLine(Mod, "SpeedBoost", "+12% ranged attack speed")
            {
                OverrideColor = new Color(255, 220, 140)
            });

            tooltips.Add(new TooltipLine(Mod, "ConstellationMark", "Ranged crits apply 'Constellation Mark' on enemy for 3s (+10% ranged damage taken)")
            {
                OverrideColor = FatePalette.DarkPink
            });

            tooltips.Add(new TooltipLine(Mod, "Flavor", "'Every shot follows the path written in the stars'")
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
                .AddIngredient(ItemID.FragmentVortex, 8)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }

    public class ConstellationCompassPlayer : ModPlayer
    {
        public bool hasConstellationCompass = false;
        
        public override void ResetEffects()
        {
            hasConstellationCompass = false;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!hasConstellationCompass) return;
            if (!proj.CountsAsClass(DamageClass.Ranged)) return;
            if (!hit.Crit) return;
            
            // Apply Constellation Mark
            var npcData = target.GetGlobalNPC<ConstellationMarkNPC>();
            npcData.constellationMarkTimer = 180; // 3 seconds
            npcData.markOwner = Player.whoAmI;
        }
    }

    public class ConstellationMarkNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        
        public int constellationMarkTimer = 0;
        public int markOwner = -1;
        
        public override void ResetEffects(NPC npc)
        {
            if (constellationMarkTimer > 0)
                constellationMarkTimer--;
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (constellationMarkTimer <= 0) return;
            if (!projectile.CountsAsClass(DamageClass.Ranged)) return;
            
            modifiers.FinalDamage *= 1.10f; // +10% ranged damage taken
        }
    }
}
