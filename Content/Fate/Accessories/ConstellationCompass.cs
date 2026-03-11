using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.Fate;
using MagnumOpus.Content.Fate.ResonanceEnergies;
using MagnumOpus.Content.Fate.HarmonicCores;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;

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

            tooltips.Add(new TooltipLine(Mod, "HomingEffect", "Ranged projectiles gain slight homing toward nearby enemies")
            {
                OverrideColor = FatePalette.DarkPink
            });

            tooltips.Add(new TooltipLine(Mod, "StarburstEffect", "Critical hits create constellation starbursts")
            {
                OverrideColor = FatePalette.BrightCrimson
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
            
            TriggerConstellationStarburst(target.Center, damageDone);
        }

        private void TriggerConstellationStarburst(Vector2 position, int damage)
        {
            // Constellation starburst VFX
            FateAccessoryVFX.ConstellationCompassStarburstVFX(position);

            // Bonus damage to nearby enemies from starburst
            if (Main.myPlayer == Player.whoAmI)
            {
                int burstDamage = damage / 4;
                float burstRange = 120f;

                foreach (NPC npc in Main.npc)
                {
                    if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;

                    float dist = Vector2.Distance(position, npc.Center);
                    if (dist < burstRange)
                    {
                        Player.ApplyDamageToNPC(npc, burstDamage, 0f, 0, false);
                        FateAccessoryVFX.ConstellationCompassBonusDamageVFX(npc.Center);
                    }
                }
            }
        }
    }

    public class ConstellationCompassGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        
        public override void AI(Projectile projectile)
        {
            if (projectile.owner < 0 || projectile.owner >= Main.maxPlayers) return;
            
            Player player = Main.player[projectile.owner];
            if (!player.active) return;
            
            var modPlayer = player.GetModPlayer<ConstellationCompassPlayer>();
            if (!modPlayer.hasConstellationCompass) return;
            
            if (!projectile.CountsAsClass(DamageClass.Ranged)) return;
            if (!projectile.friendly || projectile.hostile) return;
            if (projectile.minion || projectile.sentry) return;
            
            // Slight homing effect
            float homingRange = 200f;
            float homingStrength = 0.02f;
            
            NPC closestNPC = null;
            float closestDist = homingRange;
            
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                
                float dist = Vector2.Distance(projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestNPC = npc;
                }
            }
            
            if (closestNPC != null)
            {
                Vector2 direction = (closestNPC.Center - projectile.Center).SafeNormalize(Vector2.Zero);
                projectile.velocity = Vector2.Lerp(projectile.velocity, 
                    direction * projectile.velocity.Length(), homingStrength);
            }
        }
    }
}
