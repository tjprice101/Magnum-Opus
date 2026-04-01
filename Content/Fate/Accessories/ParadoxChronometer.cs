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
    /// Paradox Chronometer - Melee accessory for Fate theme.
    /// Manipulates the flow of time to enhance melee combat.
    /// Every 7th melee strike triggers a temporal echo that repeats the attack.
    /// </summary>
    public class ParadoxChronometer : ModItem
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
            var modPlayer = player.GetModPlayer<ParadoxChronometerPlayer>();
            modPlayer.hasParadoxChronometer = true;
            
            // +18% melee damage
            player.GetDamage(DamageClass.Melee) += 0.18f;
            
            // +20% melee speed
            player.GetAttackSpeed(DamageClass.Melee) += 0.20f;
            
            // +10% melee critical strike chance
            player.GetCritChance(DamageClass.Melee) += 10;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "MeleeBoost", "+18% melee damage")
            {
                OverrideColor = new Color(255, 150, 100)
            });

            tooltips.Add(new TooltipLine(Mod, "SpeedBoost", "+20% melee speed")
            {
                OverrideColor = new Color(255, 180, 120)
            });

            tooltips.Add(new TooltipLine(Mod, "CritBoost", "+10% melee critical strike chance")
            {
                OverrideColor = new Color(255, 200, 140)
            });

            tooltips.Add(new TooltipLine(Mod, "TemporalEcho", "Every 7th melee strike triggers a temporal echo")
            {
                OverrideColor = FatePalette.DarkPink
            });

            tooltips.Add(new TooltipLine(Mod, "EchoEffect", "Temporal echoes repeat the strike for 75% damage")
            {
                OverrideColor = FatePalette.BrightCrimson
            });

            tooltips.Add(new TooltipLine(Mod, "Flavor", "'Time bends to the rhythm of your blade'")
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
                .AddIngredient(ItemID.SoulofMight, 10)
                .AddIngredient(ItemID.FragmentSolar, 8)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }

    public class ParadoxChronometerPlayer : ModPlayer
    {
        public bool hasParadoxChronometer = false;
        public int meleeStrikeCounter = 0;
        
        private const int StrikesForEcho = 7;
        
        public override void ResetEffects()
        {
            hasParadoxChronometer = false;
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!hasParadoxChronometer) return;
            if (!item.CountsAsClass(DamageClass.Melee)) return;
            
            ProcessMeleeHit(target, damageDone, hit.Crit);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!hasParadoxChronometer) return;
            if (!proj.CountsAsClass(DamageClass.Melee)) return;
            if (proj.minion || proj.sentry) return;
            
            ProcessMeleeHit(target, damageDone, hit.Crit);
        }

        private void ProcessMeleeHit(NPC target, int damage, bool wasCrit)
        {
            meleeStrikeCounter++;

            // Show counter progress with subtle particles
            if (meleeStrikeCounter >= StrikesForEcho - 2)
            {
                float intensity = (float)(meleeStrikeCounter - (StrikesForEcho - 3)) / 3f;
                FateAccessoryVFX.ParadoxCounterProgressVFX(target.Center, intensity);
            }

            if (meleeStrikeCounter >= StrikesForEcho)
            {
                meleeStrikeCounter = 0;
                TriggerTemporalEcho(target, damage, wasCrit);
            }
        }

        private void TriggerTemporalEcho(NPC target, int originalDamage, bool wasCrit)
        {
            if (!target.active) return;

            // Temporal echo VFX
            FateAccessoryVFX.ParadoxTemporalEchoVFX(target.Center);

            // Apply echo damage (75% of original)
            if (Main.myPlayer == Player.whoAmI)
            {
                int echoDamage = (int)(originalDamage * 0.75f);
                Player.ApplyDamageToNPC(target, echoDamage, 0f, 0, wasCrit);
            }

            // Sound effect
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.5f, Volume = 0.6f }, target.Center);
        }
    }
}
