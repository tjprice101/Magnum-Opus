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
    /// Constellation Quiver - Ranged accessory for Nachtmusik theme.
    /// A celestial quiver that enhances ranged attacks with starlight power.
    /// Projectiles leave starlight trails and have a chance to split into constellation shards.
    /// </summary>
    public class ConstellationQuiver : ModItem
    {
        // Placeholder texture until custom art is ready
        public override string Texture => "Terraria/Images/Item_" + ItemID.EndlessQuiver;
        
        // Nachtmusik colors
        private static readonly Color DeepPurple = new Color(45, 27, 78);
        private static readonly Color Gold = new Color(255, 215, 0);
        private static readonly Color Violet = new Color(123, 104, 238);
        private static readonly Color StarWhite = new Color(255, 255, 255);

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
            var modPlayer = player.GetModPlayer<ConstellationQuiverPlayer>();
            modPlayer.hasConstellationQuiver = true;

            // +40% ranged damage - POST-FATE ULTIMATE
            player.GetDamage(DamageClass.Ranged) += 0.40f;

            // +20% ranged attack speed - POST-FATE ULTIMATE
            player.GetAttackSpeed(DamageClass.Ranged) += 0.20f;

            // +18% ranged crit - POST-FATE ULTIMATE
            player.GetCritChance(DamageClass.Ranged) += 18;

            // +15% ammo conservation - POST-FATE ULTIMATE
            player.ammoCost75 = true;

            // Ambient starlight particles
            if (!hideVisual && Main.rand.NextBool(10))
            {
                Vector2 offset = Main.rand.NextVector2Circular(28f, 28f);
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.GoldFlame;
                Dust dust = Dust.NewDustPerfect(player.Center + offset, dustType,
                    new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.8f), 120, default, 0.9f);
                dust.noGravity = true;
            }

            // Floating nocturnal melody notes
            if (!hideVisual && Main.rand.NextBool(12))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(32f, 32f);
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), -Main.rand.NextFloat(0.3f, 0.5f));
                Color noteColor = Color.Lerp(new Color(100, 60, 180), new Color(80, 100, 200), Main.rand.NextFloat()) * 0.55f;
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.68f, 35);
            }
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "RangedBoost", "+40% ranged damage")
            {
                OverrideColor = Gold
            });

            tooltips.Add(new TooltipLine(Mod, "AttackSpeed", "+20% ranged attack speed")
            {
                OverrideColor = Violet
            });

            tooltips.Add(new TooltipLine(Mod, "CritBoost", "+18% ranged critical strike chance")
            {
                OverrideColor = StarWhite
            });

            tooltips.Add(new TooltipLine(Mod, "AmmoConserve", "25% chance to not consume ammo")
            {
                OverrideColor = DeepPurple
            });

            tooltips.Add(new TooltipLine(Mod, "StarSplit", "Ranged criticals have a 20% chance to split into 3 starlight shards")
            {
                OverrideColor = Color.Lerp(DeepPurple, Gold, 0.4f)
            });

            tooltips.Add(new TooltipLine(Mod, "ShardDamage", "Starlight shards deal 40% of the original damage")
            {
                OverrideColor = Color.Lerp(Violet, Gold, 0.5f)
            });

            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The stars themselves become your ammunition'")
            {
                OverrideColor = Color.Lerp(DeepPurple, Violet, 0.3f)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<NachtmusikResonantEnergy>(), 12)
                .AddIngredient(ModContent.ItemType<NachtmusikResonantCore>(), 4)
                .AddIngredient(ItemID.FragmentVortex, 8)
                .AddIngredient(ItemID.LunarBar, 8)
                .AddIngredient(ItemID.FallenStar, 15)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    public class ConstellationQuiverPlayer : ModPlayer
    {
        public bool hasConstellationQuiver = false;

        // Nachtmusik colors
        private static readonly Color DeepPurple = new Color(45, 27, 78);
        private static readonly Color Gold = new Color(255, 215, 0);
        private static readonly Color Violet = new Color(123, 104, 238);
        private static readonly Color StarWhite = new Color(255, 255, 255);

        public override void ResetEffects()
        {
            hasConstellationQuiver = false;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!hasConstellationQuiver) return;
            if (!proj.CountsAsClass(DamageClass.Ranged)) return;
            if (!hit.Crit) return;
            if (!Main.rand.NextBool(5)) return; // 20% chance

            SpawnStarlightShards(target, damageDone);
        }

        private void SpawnStarlightShards(NPC target, int baseDamage)
        {
            // Visual impact
            CustomParticles.GenericFlare(target.Center, StarWhite, 0.8f, 18);
            CustomParticles.GenericFlare(target.Center, Gold, 0.6f, 15);
            
            // Star burst
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 sparkPos = target.Center + angle.ToRotationVector2() * 20f;
                CustomParticles.GenericFlare(sparkPos, Violet * 0.7f, 0.3f, 12);
            }

            // Music note accent
            ThemedParticles.MusicNoteBurst(target.Center, Violet * 0.6f, 3, 3f);

            // Find nearby enemies for shards to target
            int shardDamage = (int)(baseDamage * 0.4f);
            int shardsSpawned = 0;
            
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                if (npc.whoAmI == target.whoAmI) continue;
                if (Vector2.Distance(npc.Center, target.Center) > 400f) continue;
                if (shardsSpawned >= 3) break;
                
                // Deal damage to nearby enemy
                Player.ApplyDamageToNPC(npc, shardDamage, 0f, 0, false);
                
                // Visual trail from original target to this enemy
                Vector2 direction = npc.Center - target.Center;
                float distance = direction.Length();
                direction.Normalize();
                
                for (int j = 0; j < (int)(distance / 20f); j++)
                {
                    Vector2 trailPos = target.Center + direction * (j * 20f);
                    Color trailColor = Color.Lerp(Gold, Violet, j / (distance / 20f));
                    CustomParticles.GenericFlare(trailPos, trailColor * 0.6f, 0.25f, 10);
                }
                
                // Impact on target enemy
                CustomParticles.GenericFlare(npc.Center, StarWhite * 0.7f, 0.5f, 12);
                CustomParticles.HaloRing(npc.Center, Gold * 0.5f, 0.3f, 10);
                
                shardsSpawned++;
            }

            // If no other enemies, just spawn visual shards
            if (shardsSpawned == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    float angle = MathHelper.TwoPi * i / 3f + Main.rand.NextFloat(-0.3f, 0.3f);
                    Vector2 shardVel = angle.ToRotationVector2() * 8f;
                    
                    for (int j = 0; j < 5; j++)
                    {
                        Vector2 shardPos = target.Center + shardVel * (j * 5f);
                        Color shardColor = Color.Lerp(Gold, Violet, j / 5f);
                        CustomParticles.GenericFlare(shardPos, shardColor * 0.5f, 0.2f, 8);
                    }
                }
            }
        }
    }
}
