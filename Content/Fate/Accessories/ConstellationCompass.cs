using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.Fate.ResonanceEnergies;
using MagnumOpus.Content.Fate.HarmonicCores;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;

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
            
            // Celestial compass ambient particles
            if (!hideVisual && Main.rand.NextBool(7))
            {
                float angle = Main.GameUpdateCount * 0.03f;
                for (int i = 0; i < 4; i++)
                {
                    float starAngle = angle + MathHelper.PiOver2 * i;
                    Vector2 starPos = player.Center + starAngle.ToRotationVector2() * 25f;
                    
                    if (Main.rand.NextBool(4))
                    {
                        Dust dust = Dust.NewDustPerfect(starPos, DustID.Enchanted_Pink, 
                            Vector2.Zero, 100, default, 0.6f);
                        dust.noGravity = true;
                    }
                }
            }
            
            // Star sparkle particles
            if (!hideVisual && Main.rand.NextBool(12))
            {
                CustomParticles.GenericFlare(player.Center + Main.rand.NextVector2Circular(30f, 30f),
                    FateCosmicVFX.FateWhite, 0.2f, 10);
            }
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
                OverrideColor = FateCosmicVFX.FateDarkPink
            });
            
            tooltips.Add(new TooltipLine(Mod, "StarburstEffect", "Critical hits create constellation starbursts")
            {
                OverrideColor = FateCosmicVFX.FateBrightRed
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
            FateCosmicVFX.SpawnCosmicExplosion(position, 0.6f);
            
            // Star pattern burst
            int starPoints = 5;
            for (int i = 0; i < starPoints; i++)
            {
                float angle = MathHelper.TwoPi * i / starPoints;
                Vector2 starPos = position + angle.ToRotationVector2() * 40f;
                
                CustomParticles.GenericFlare(starPos, FateCosmicVFX.FateWhite, 0.4f, 18);
                CustomParticles.GenericFlare(starPos, FateCosmicVFX.FateDarkPink * 0.8f, 0.3f, 15);
                
                // Connect stars with faint lines (constellation effect)
                int nextPoint = (i + 2) % starPoints; // Skip one for star pattern
                float nextAngle = MathHelper.TwoPi * nextPoint / starPoints;
                Vector2 nextPos = position + nextAngle.ToRotationVector2() * 40f;
                
                FateCosmicVFX.DrawCosmicLightning(starPos, nextPos, 4, 8f, 0, 0.3f);
            }
            
            // Central flare
            CustomParticles.GenericFlare(position, Color.White, 0.7f, 20);
            CustomParticles.HaloRing(position, FateCosmicVFX.FateDarkPink, 0.4f, 15);
            
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
                        CustomParticles.GenericFlare(npc.Center, FateCosmicVFX.FateBrightRed * 0.7f, 0.35f, 12);
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
