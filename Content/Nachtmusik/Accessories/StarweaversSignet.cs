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
    /// Starweaver's Signet - Melee accessory for Nachtmusik theme.
    /// A golden ring set with a deep purple starfield gem, empowering melee strikes with cosmic might.
    /// Melee attacks summon starfall impacts on critical hits.
    /// </summary>
    public class StarweaversSignet : ModItem
    {
        // Nachtmusik colors
        private static readonly Color DeepPurple = new Color(45, 27, 78);    // #2D1B4E
        private static readonly Color Gold = new Color(255, 215, 0);          // #FFD700
        private static readonly Color Violet = new Color(123, 104, 238);      // #7B68EE
        private static readonly Color StarWhite = new Color(255, 255, 255);   // #FFFFFF

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<StarweaversSignetPlayer>();
            modPlayer.hasStarweaversSignet = true;

            // +38% melee damage - POST-FATE ULTIMATE
            player.GetDamage(DamageClass.Melee) += 0.38f;

            // +25% melee attack speed - POST-FATE ULTIMATE
            player.GetAttackSpeed(DamageClass.Melee) += 0.25f;

            // +18% melee crit - POST-FATE ULTIMATE
            player.GetCritChance(DamageClass.Melee) += 18;

            // Ambient star particles around player
            if (!hideVisual && Main.rand.NextBool(8))
            {
                Vector2 offset = Main.rand.NextVector2Circular(30f, 30f);
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.GoldFlame;
                Dust dust = Dust.NewDustPerfect(player.Center + offset, dustType,
                    new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -1f), 150, default, 0.9f);
                dust.noGravity = true;
            }

            // Occasional golden star sparkle
            if (!hideVisual && Main.rand.NextBool(20))
            {
                Vector2 sparklePos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                CustomParticles.GenericFlare(sparklePos, Gold * 0.7f, 0.25f, 15);
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
            tooltips.Add(new TooltipLine(Mod, "MeleeBoost", "+38% melee damage")
            {
                OverrideColor = Gold
            });

            tooltips.Add(new TooltipLine(Mod, "AttackSpeed", "+25% melee attack speed")
            {
                OverrideColor = Violet
            });

            tooltips.Add(new TooltipLine(Mod, "CritBoost", "+18% melee critical strike chance")
            {
                OverrideColor = StarWhite
            });

            tooltips.Add(new TooltipLine(Mod, "StarfallEffect", "Critical strikes summon a starfall impact")
            {
                OverrideColor = DeepPurple
            });

            tooltips.Add(new TooltipLine(Mod, "StarfallDamage", "Starfall deals 75% of the hit's damage to nearby enemies")
            {
                OverrideColor = Color.Lerp(DeepPurple, Gold, 0.5f)
            });

            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The stars weave through your blade, each strike a constellation born'")
            {
                OverrideColor = Color.Lerp(Violet, StarWhite, 0.3f)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<NachtmusikResonantEnergy>(), 12)
                .AddIngredient(ModContent.ItemType<NachtmusikResonantCore>(), 4)
                .AddIngredient(ItemID.FragmentSolar, 8)
                .AddIngredient(ItemID.LunarBar, 8)
                .AddIngredient(ItemID.FallenStar, 15)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    public class StarweaversSignetPlayer : ModPlayer
    {
        public bool hasStarweaversSignet = false;

        // Nachtmusik colors
        private static readonly Color DeepPurple = new Color(45, 27, 78);
        private static readonly Color Gold = new Color(255, 215, 0);
        private static readonly Color Violet = new Color(123, 104, 238);
        private static readonly Color StarWhite = new Color(255, 255, 255);

        public override void ResetEffects()
        {
            hasStarweaversSignet = false;
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!hasStarweaversSignet) return;
            if (!item.CountsAsClass(DamageClass.Melee)) return;
            if (!hit.Crit) return;

            TriggerStarfallImpact(target, damageDone);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!hasStarweaversSignet) return;
            if (!proj.CountsAsClass(DamageClass.Melee)) return;
            if (!hit.Crit) return;

            TriggerStarfallImpact(target, damageDone);
        }

        private void TriggerStarfallImpact(NPC target, int baseDamage)
        {
            // Visual impact at target
            // Central flash
            CustomParticles.GenericFlare(target.Center, StarWhite, 0.9f, 20);
            CustomParticles.GenericFlare(target.Center, Gold, 0.7f, 18);
            CustomParticles.GenericFlare(target.Center, DeepPurple, 0.5f, 16);

            // Music note burst on starfall impact
            ThemedParticles.MusicNoteBurst(target.Center, new Color(100, 60, 180), 4, 3.5f);

            // Star sparkle accents
            for (int i = 0; i < 3; i++)
            {
                var sparkle = new SparkleParticle(target.Center + Main.rand.NextVector2Circular(10f, 10f),
                    Main.rand.NextVector2Circular(2f, 2f), new Color(255, 250, 240) * 0.5f, 0.2f, 15);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Starfall streaks from above
            for (int i = 0; i < 5; i++)
            {
                float xOffset = Main.rand.NextFloat(-50f, 50f);
                Vector2 startPos = target.Center + new Vector2(xOffset, -150f);
                Vector2 endPos = target.Center + new Vector2(xOffset * 0.3f, 0f);

                // Draw star trail
                for (int j = 0; j < 8; j++)
                {
                    float progress = j / 8f;
                    Vector2 trailPos = Vector2.Lerp(startPos, endPos, progress);
                    Color trailColor = Color.Lerp(Gold, DeepPurple, progress);
                    CustomParticles.GenericFlare(trailPos, trailColor * (1f - progress * 0.5f), 0.3f - progress * 0.2f, 10);
                }
            }

            // Expanding halo rings
            for (int ring = 0; ring < 4; ring++)
            {
                float ringProgress = ring / 4f;
                Color ringColor = Color.Lerp(Gold, Violet, ringProgress);
                CustomParticles.HaloRing(target.Center, ringColor, 0.3f + ring * 0.12f, 15 + ring * 3);
            }

            // Star burst particles
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 burstDir = angle.ToRotationVector2();
                Vector2 sparkPos = target.Center + burstDir * 25f;
                CustomParticles.GenericFlare(sparkPos, Gold, 0.35f, 12);
            }

            // Damage nearby enemies (75% of hit damage)
            int starfallDamage = (int)(baseDamage * 0.75f);
            float aoeRadius = 120f;

            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                if (npc.whoAmI == target.whoAmI) continue;

                float dist = Vector2.Distance(target.Center, npc.Center);
                if (dist < aoeRadius)
                {
                    if (Main.myPlayer == Player.whoAmI)
                    {
                        Player.ApplyDamageToNPC(npc, starfallDamage, 0f, 0, false);
                    }

                    // VFX on hit enemy
                    CustomParticles.GenericFlare(npc.Center, Gold, 0.4f, 12);
                    CustomParticles.HaloRing(npc.Center, DeepPurple, 0.25f, 10);
                }
            }

            // Dynamic lighting
            Lighting.AddLight(target.Center, Gold.ToVector3() * 1.2f);
        }
    }
}
