using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Content.Nachtmusik.ResonanceEnergies;
using MagnumOpus.Content.Nachtmusik.HarmonicCores;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Nachtmusik;

namespace MagnumOpus.Content.Nachtmusik.Accessories
{
    /// <summary>
    /// Moonlit Serenade Pendant - Magic accessory for Nachtmusik theme.
    /// A crescent moon pendant infused with the Queen's nocturnal magic.
    /// Magic attacks have a chance to release harmonic waves that bounce between enemies.
    /// </summary>
    public class MoonlitSerenadePendant : ModItem
    {
        // Nachtmusik colors
        private static readonly Color DeepPurple = new Color(45, 27, 78);    // #2D1B4E
        private static readonly Color Gold = new Color(255, 215, 0);          // #FFD700
        private static readonly Color Violet = new Color(123, 104, 238);      // #7B68EE
        private static readonly Color StarWhite = new Color(255, 255, 255);   // #FFFFFF

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<MoonlitSerenadePendantPlayer>();
            modPlayer.hasMoonlitSerenadePendant = true;

            // +35% magic damage - POST-FATE ULTIMATE
            player.GetDamage(DamageClass.Magic) += 0.35f;

            // +50 mana regeneration - POST-FATE ULTIMATE
            player.manaRegenBonus += 50;

            // -20% mana cost - POST-FATE ULTIMATE
            player.manaCost -= 0.20f;

            // +15% magic crit - POST-FATE ULTIMATE
            player.GetCritChance(DamageClass.Magic) += 15;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "MagicBoost", "+35% magic damage")
            {
                OverrideColor = Violet
            });

            tooltips.Add(new TooltipLine(Mod, "ManaRegen", "+50 mana regeneration")
            {
                OverrideColor = Color.Lerp(Violet, StarWhite, 0.5f)
            });

            tooltips.Add(new TooltipLine(Mod, "ManaCost", "-20% mana cost")
            {
                OverrideColor = DeepPurple
            });

            tooltips.Add(new TooltipLine(Mod, "CritBoost", "+15% magic critical strike chance")
            {
                OverrideColor = Gold
            });

            tooltips.Add(new TooltipLine(Mod, "HarmonicWave", "Magic attacks have a 12.5% chance to release harmonic waves")
            {
                OverrideColor = Color.Lerp(DeepPurple, Gold, 0.4f)
            });

            tooltips.Add(new TooltipLine(Mod, "WaveBounce", "Waves bounce between up to 4 enemies, dealing 50% damage per bounce")
            {
                OverrideColor = Color.Lerp(Violet, Gold, 0.5f)
            });

            tooltips.Add(new TooltipLine(Mod, "Flavor", "'Her serenade carries on the moonlight, touching all who hear'")
            {
                OverrideColor = Color.Lerp(DeepPurple, Violet, 0.3f)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<NachtmusikResonantEnergy>(), 12)
                .AddIngredient(ModContent.ItemType<NachtmusikResonantCore>(), 4)
                .AddIngredient(ItemID.FragmentNebula, 8)
                .AddIngredient(ItemID.LunarBar, 8)
                .AddIngredient(ItemID.FallenStar, 15)
                .AddIngredient(ModContent.ItemType<ShardOfNachtmusiksTempo>(), 5)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    public class MoonlitSerenadePendantPlayer : ModPlayer
    {
        public bool hasMoonlitSerenadePendant = false;

        // Nachtmusik colors
        private static readonly Color DeepPurple = new Color(45, 27, 78);
        private static readonly Color Gold = new Color(255, 215, 0);
        private static readonly Color Violet = new Color(123, 104, 238);
        private static readonly Color StarWhite = new Color(255, 255, 255);

        public override void ResetEffects()
        {
            hasMoonlitSerenadePendant = false;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!hasMoonlitSerenadePendant) return;
            if (!proj.CountsAsClass(DamageClass.Magic)) return;
            if (!Main.rand.NextBool(8)) return; // ~12% chance

            TriggerHarmonicWave(target, damageDone);
        }

        private void TriggerHarmonicWave(NPC initialTarget, int baseDamage)
        {
            // Initial wave burst at target

            // Music note burst on harmonic wave proc

            // Star sparkle accents
            for (int i = 0; i < 3; i++)
            {
            }

            // Bouncing wave effect
            int maxBounces = 4;
            float bounceRange = 350f;
            int currentDamage = baseDamage;
            NPC lastTarget = initialTarget;

            System.Collections.Generic.HashSet<int> hitNPCs = new() { initialTarget.whoAmI };

            for (int bounce = 0; bounce < maxBounces; bounce++)
            {
                NPC nextTarget = null;
                float closestDist = bounceRange;

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

                // Draw harmonic wave arc between targets
                DrawHarmonicWaveArc(lastTarget.Center, nextTarget.Center, bounce);

                // Calculate damage with falloff
                currentDamage = (int)(currentDamage * 0.5f);
                if (currentDamage < 1) currentDamage = 1;

                // Apply damage
                if (Main.myPlayer == Player.whoAmI)
                {
                    Player.ApplyDamageToNPC(nextTarget, currentDamage, 0f, 0, false);
                }

                // VFX at bounce target
                float bounceProgress = (float)bounce / maxBounces;
                Color bounceColor = Color.Lerp(Gold, DeepPurple, bounceProgress);

                hitNPCs.Add(nextTarget.whoAmI);
                lastTarget = nextTarget;
            }
        }

        private void DrawHarmonicWaveArc(Vector2 start, Vector2 end, int bounceIndex)
        {
            // Draw a curved arc of particles between targets
            Vector2 direction = end - start;
            float distance = direction.Length();
            direction.Normalize();

            // Perpendicular offset for arc
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
            float arcHeight = distance * 0.25f * (bounceIndex % 2 == 0 ? 1f : -1f);

            // Music note at midpoint
            Vector2 midpoint = Vector2.Lerp(start, end, 0.5f) + perpendicular * arcHeight;
        }
    }
}
