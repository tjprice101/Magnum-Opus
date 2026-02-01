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
using static MagnumOpus.Common.Systems.ThemedParticles;

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

            // Ambient crescent moon particles
            if (!hideVisual && Main.rand.NextBool(10))
            {
                Vector2 offset = Main.rand.NextVector2Circular(28f, 28f);
                int dustType = DustID.PurpleTorch;
                Dust dust = Dust.NewDustPerfect(player.Center + offset, dustType,
                    new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1.5f, -0.5f)), 120, default, 1.0f);
                dust.noGravity = true;
            }

            // Floating nocturnal melody notes
            if (!hideVisual && Main.rand.NextBool(10))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(32f, 32f);
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), -Main.rand.NextFloat(0.3f, 0.5f)); // Rising like night whispers
                Color noteColor = Color.Lerp(new Color(100, 60, 180), new Color(80, 100, 200), Main.rand.NextFloat()) * 0.55f;
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.68f, 35);
            }
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

            tooltips.Add(new TooltipLine(Mod, "HarmonicWave", "Magic attacks have a 12% chance to release harmonic waves")
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
            CustomParticles.GenericFlare(initialTarget.Center, StarWhite, 0.8f, 18);
            CustomParticles.GenericFlare(initialTarget.Center, Gold, 0.6f, 16);
            CustomParticles.HaloRing(initialTarget.Center, Violet, 0.4f, 15);

            // Music note burst on harmonic wave proc
            ThemedParticles.MusicNoteBurst(initialTarget.Center, new Color(100, 60, 180), 4, 3.5f);

            // Star sparkle accents
            for (int i = 0; i < 3; i++)
            {
                var sparkle = new SparkleParticle(initialTarget.Center + Main.rand.NextVector2Circular(10f, 10f),
                    Main.rand.NextVector2Circular(2f, 2f), new Color(255, 250, 240) * 0.5f, 0.2f, 15);
                MagnumParticleHandler.SpawnParticle(sparkle);
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
                CustomParticles.GenericFlare(nextTarget.Center, bounceColor, 0.5f - bounce * 0.08f, 14);
                CustomParticles.HaloRing(nextTarget.Center, Violet * (1f - bounceProgress * 0.3f), 0.3f, 12);

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

            int segments = 10;
            for (int i = 0; i <= segments; i++)
            {
                float progress = (float)i / segments;
                
                // Quadratic bezier-like curve
                float arcOffset = (float)System.Math.Sin(progress * System.Math.PI) * arcHeight;
                Vector2 point = Vector2.Lerp(start, end, progress) + perpendicular * arcOffset;

                // Color gradient along arc
                Color arcColor = Color.Lerp(Gold, Violet, progress);
                float scale = 0.25f * (1f - System.Math.Abs(progress - 0.5f) * 0.5f);

                CustomParticles.GenericFlare(point, arcColor * 0.8f, scale, 8);
            }

            // Music note at midpoint
            Vector2 midpoint = Vector2.Lerp(start, end, 0.5f) + perpendicular * arcHeight;
            CustomParticles.GenericFlare(midpoint, StarWhite, 0.35f, 12);
        }
    }
}
