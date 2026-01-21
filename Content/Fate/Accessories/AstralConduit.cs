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
            
            // Celestial cosmic ambient particles
            if (!hideVisual && Main.rand.NextBool(6))
            {
                Vector2 offset = Main.rand.NextVector2Circular(25f, 25f);
                int dustType = Main.rand.NextBool() ? DustID.Enchanted_Pink : DustID.PurpleTorch;
                Dust dust = Dust.NewDustPerfect(player.Center + offset, dustType, 
                    new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1.5f), 100, default, 1.1f);
                dust.noGravity = true;
            }
            
            // Cosmic glyph particles
            if (!hideVisual && Main.rand.NextBool(15))
            {
                CustomParticles.Glyph(player.Center + Main.rand.NextVector2Circular(30f, 30f),
                    FateCosmicVFX.FateDarkPink, 0.25f, -1);
            }
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
            
            tooltips.Add(new TooltipLine(Mod, "CosmicFlare", "Magic attacks have a 15% chance to trigger cosmic flares")
            {
                OverrideColor = FateCosmicVFX.FateDarkPink
            });
            
            tooltips.Add(new TooltipLine(Mod, "ChainEffect", "Cosmic flares chain to up to 3 nearby enemies")
            {
                OverrideColor = FateCosmicVFX.FateBrightRed
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
        
        public override void ResetEffects()
        {
            hasAstralConduit = false;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!hasAstralConduit) return;
            if (!proj.CountsAsClass(DamageClass.Magic)) return;
            if (!Main.rand.NextBool(7)) return; // ~15% chance
            
            TriggerCosmicFlare(target, damageDone);
        }

        private void TriggerCosmicFlare(NPC target, int baseDamage)
        {
            // VFX at initial target
            FateCosmicVFX.SpawnCosmicExplosion(target.Center, 0.8f);
            CustomParticles.GlyphBurst(target.Center, FateCosmicVFX.FateDarkPink, 4, 3f);
            
            // Chain to nearby enemies
            int chainsRemaining = 3;
            float chainRange = 300f;
            int chainDamage = baseDamage / 3;
            NPC lastTarget = target;
            
            System.Collections.Generic.HashSet<int> hitNPCs = new() { target.whoAmI };
            
            for (int chain = 0; chain < chainsRemaining; chain++)
            {
                NPC nextTarget = null;
                float closestDist = chainRange;
                
                foreach (NPC npc in Main.npc)
                {
                    if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                    if (hitNPCs.Contains(npc.whoAmI)) continue;
                    
                    float dist = Vector2.Distance(lastTarget.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        nextTarget = npc;
                    }
                }
                
                if (nextTarget == null) break;
                
                // Draw lightning between targets
                FateCosmicVFX.DrawCosmicLightning(lastTarget.Center, nextTarget.Center, 8, 25f, 2, 0.4f);
                
                // Damage the next target
                if (Main.myPlayer == Player.whoAmI)
                {
                    Player.ApplyDamageToNPC(nextTarget, chainDamage, 0f, 0, false);
                }
                
                // VFX at chain target
                CustomParticles.GenericFlare(nextTarget.Center, FateCosmicVFX.FateBrightRed, 0.5f, 15);
                
                hitNPCs.Add(nextTarget.whoAmI);
                lastTarget = nextTarget;
            }
        }
    }
}
