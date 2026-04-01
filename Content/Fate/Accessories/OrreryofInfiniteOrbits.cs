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
    /// Orrery of Infinite Orbits - Summon accessory for Fate theme.
    /// A cosmic planetarium that enhances minion capabilities.
    /// Summons orbit around the player and gain cosmic empowerment.
    /// </summary>
    public class OrreryofInfiniteOrbits : ModItem
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
            var modPlayer = player.GetModPlayer<OrreryPlayer>();
            modPlayer.hasOrrery = true;
            
            // +22% summon damage
            player.GetDamage(DamageClass.Summon) += 0.22f;
            
            // +1 max minion
            player.maxMinions += 1;
            
            // +10% minion knockback
            player.GetKnockback(DamageClass.Summon) += 0.10f;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "SummonBoost", "+22% summon damage")
            {
                OverrideColor = new Color(100, 200, 255)
            });

            tooltips.Add(new TooltipLine(Mod, "MinionSlot", "+1 max minion")
            {
                OverrideColor = new Color(120, 220, 255)
            });

            tooltips.Add(new TooltipLine(Mod, "KnockbackBoost", "+10% minion knockback")
            {
                OverrideColor = new Color(140, 200, 255)
            });

            tooltips.Add(new TooltipLine(Mod, "CosmicEmpower", "Minions periodically gain Cosmic Empowerment")
            {
                OverrideColor = FatePalette.DarkPink
            });

            tooltips.Add(new TooltipLine(Mod, "EmpowerEffect", "Empowered minion attacks deal 50% bonus damage")
            {
                OverrideColor = FatePalette.BrightCrimson
            });

            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The universe itself serves at your command'")
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
                .AddIngredient(ItemID.FragmentStardust, 8)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }

    public class OrreryPlayer : ModPlayer
    {
        public bool hasOrrery = false;
        public int empowermentTimer = 0;
        public int empoweredMinionIndex = -1;
        
        private const int EmpowermentInterval = 300; // 5 seconds
        private const int EmpowermentDuration = 120; // 2 seconds of empowerment
        
        public override void ResetEffects()
        {
            hasOrrery = false;
        }

        public override void PostUpdate()
        {
            if (!hasOrrery) 
            {
                empowermentTimer = 0;
                empoweredMinionIndex = -1;
                return;
            }
            
            empowermentTimer++;
            
            // Every 5 seconds, empower a random minion
            if (empowermentTimer >= EmpowermentInterval)
            {
                empowermentTimer = 0;
                EmpowerRandomMinion();
            }
            
        }

        private void EmpowerRandomMinion()
        {
            // Find all player's minions
            System.Collections.Generic.List<int> minionIndices = new();

            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.owner == Player.whoAmI && proj.minion)
                {
                    minionIndices.Add(proj.whoAmI);
                }
            }

            if (minionIndices.Count == 0) return;

            // Pick random minion
            empoweredMinionIndex = minionIndices[Main.rand.Next(minionIndices.Count)];

            // Empowerment VFX burst
            Projectile minion = Main.projectile[empoweredMinionIndex];
            FateAccessoryVFX.OrreryEmpowermentVFX(minion.Center);
        }
    }

    public class OrreryGlobalProjectile : GlobalProjectile
    {
        public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (projectile.owner < 0 || projectile.owner >= Main.maxPlayers) return;
            if (!projectile.minion) return;

            Player player = Main.player[projectile.owner];
            var modPlayer = player.GetModPlayer<OrreryPlayer>();

            if (!modPlayer.hasOrrery) return;

            // Check if this is the empowered minion during empowerment window
            if (projectile.whoAmI == modPlayer.empoweredMinionIndex &&
                modPlayer.empowermentTimer < 120) // Within empowerment duration
            {
                // +50% damage for empowered minion
                modifiers.FinalDamage *= 1.5f;

                // Extra VFX on empowered hit
                FateAccessoryVFX.OrreryEmpoweredHitVFX(target.Center);
            }
        }
    }
}
