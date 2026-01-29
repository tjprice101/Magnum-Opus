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

namespace MagnumOpus.Content.Nachtmusik.Accessories
{
    /// <summary>
    /// Nocturne's Embrace - Summoner accessory for Nachtmusik theme.
    /// A star-shaped badge that channels the Queen's symphonic command over celestial minions.
    /// Minions gain bonus damage and periodically perform coordinated constellation strikes.
    /// </summary>
    public class NocturnesEmbrace : ModItem
    {
        // Nachtmusik colors
        private static readonly Color DeepPurple = new Color(45, 27, 78);    // #2D1B4E
        private static readonly Color Gold = new Color(255, 215, 0);          // #FFD700
        private static readonly Color Violet = new Color(123, 104, 238);      // #7B68EE
        private static readonly Color StarWhite = new Color(255, 255, 255);   // #FFFFFF

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<NocturnesEmbracePlayer>();
            modPlayer.hasNocturnesEmbrace = true;

            // +25% summon damage
            player.GetDamage(DamageClass.Summon) += 0.25f;

            // +2 minion slots
            player.maxMinions += 2;

            // +10% minion knockback
            player.GetKnockback(DamageClass.Summon) += 0.10f;

            // Minions inflict a stacking debuff
            // (handled in ModPlayer)

            // Ambient conductor particles
            if (!hideVisual && Main.rand.NextBool(10))
            {
                Vector2 offset = Main.rand.NextVector2Circular(25f, 25f);
                int dustType = DustID.PurpleTorch;
                Dust dust = Dust.NewDustPerfect(player.Center + offset, dustType,
                    new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.8f), 120, default, 0.9f);
                dust.noGravity = true;
            }

            // Orbiting constellation points
            if (!hideVisual && Main.rand.NextBool(20))
            {
                float orbitAngle = Main.GameUpdateCount * 0.025f;
                for (int i = 0; i < 3; i++)
                {
                    float angle = orbitAngle + MathHelper.TwoPi * i / 3f;
                    Vector2 starPos = player.Center + angle.ToRotationVector2() * 35f;
                    CustomParticles.GenericFlare(starPos, Gold * 0.5f, 0.18f, 10);
                }
            }
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "SummonBoost", "+25% summon damage")
            {
                OverrideColor = Gold
            });

            tooltips.Add(new TooltipLine(Mod, "MinionSlots", "+2 max minions")
            {
                OverrideColor = Violet
            });

            tooltips.Add(new TooltipLine(Mod, "Knockback", "+10% minion knockback")
            {
                OverrideColor = DeepPurple
            });

            tooltips.Add(new TooltipLine(Mod, "ConstellationStrike", "Every 8 seconds, minions perform a constellation strike")
            {
                OverrideColor = Color.Lerp(DeepPurple, Gold, 0.4f)
            });

            tooltips.Add(new TooltipLine(Mod, "StrikeEffect", "Constellation strikes deal 200% minion damage to all enemies near your minions")
            {
                OverrideColor = Color.Lerp(Violet, Gold, 0.5f)
            });

            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The night's orchestra answers to your command'")
            {
                OverrideColor = Color.Lerp(DeepPurple, Violet, 0.3f)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<NachtmusikResonantEnergy>(), 12)
                .AddIngredient(ModContent.ItemType<NachtmusikResonantCore>(), 4)
                .AddIngredient(ItemID.FragmentStardust, 10)
                .AddIngredient(ItemID.LunarBar, 8)
                .AddIngredient(ItemID.FallenStar, 20)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    public class NocturnesEmbracePlayer : ModPlayer
    {
        public bool hasNocturnesEmbrace = false;
        private int strikeTimer = 0;
        private const int StrikeCooldown = 480; // 8 seconds

        // Nachtmusik colors
        private static readonly Color DeepPurple = new Color(45, 27, 78);
        private static readonly Color Gold = new Color(255, 215, 0);
        private static readonly Color Violet = new Color(123, 104, 238);
        private static readonly Color StarWhite = new Color(255, 255, 255);

        public override void ResetEffects()
        {
            hasNocturnesEmbrace = false;
        }

        public override void PostUpdate()
        {
            if (!hasNocturnesEmbrace)
            {
                strikeTimer = 0;
                return;
            }

            strikeTimer++;

            // Visual buildup as strike approaches
            if (strikeTimer > StrikeCooldown - 40 && strikeTimer % 8 == 0)
            {
                float progress = (strikeTimer - (StrikeCooldown - 40)) / 40f;
                
                // Particles around each minion
                foreach (Projectile proj in Main.projectile)
                {
                    if (!proj.active || proj.owner != Player.whoAmI || !proj.minion) continue;

                    int particleCount = (int)(2 + progress * 3);
                    for (int i = 0; i < particleCount; i++)
                    {
                        float angle = MathHelper.TwoPi * i / particleCount + Main.GameUpdateCount * 0.08f;
                        float radius = 25f * (1f - progress * 0.3f);
                        Vector2 pos = proj.Center + angle.ToRotationVector2() * radius;
                        Color color = Color.Lerp(Violet, Gold, progress);
                        CustomParticles.GenericFlare(pos, color, 0.2f + progress * 0.1f, 8);
                    }
                }
            }

            if (strikeTimer >= StrikeCooldown)
            {
                TriggerConstellationStrike();
                strikeTimer = 0;
            }
        }

        private void TriggerConstellationStrike()
        {
            // Gather all minion positions
            System.Collections.Generic.List<Vector2> minionPositions = new();
            
            foreach (Projectile proj in Main.projectile)
            {
                if (!proj.active || proj.owner != Player.whoAmI || !proj.minion) continue;
                minionPositions.Add(proj.Center);
            }

            if (minionPositions.Count == 0) return;

            // Draw constellation lines between minions
            if (minionPositions.Count > 1)
            {
                for (int i = 0; i < minionPositions.Count; i++)
                {
                    int next = (i + 1) % minionPositions.Count;
                    DrawConstellationLine(minionPositions[i], minionPositions[next]);
                }
            }

            // Calculate strike damage (200% of base summon damage)
            int baseDamage = (int)(Player.GetTotalDamage(DamageClass.Summon).ApplyTo(100) * 2f);
            float strikeRadius = 150f;

            // Strike at each minion position
            foreach (Vector2 minionPos in minionPositions)
            {
                // Visual burst at minion
                CustomParticles.GenericFlare(minionPos, StarWhite, 0.8f, 18);
                CustomParticles.GenericFlare(minionPos, Gold, 0.6f, 16);
                
                // Expanding star burst
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 burstDir = angle.ToRotationVector2();
                    for (int j = 0; j < 4; j++)
                    {
                        float dist = 15f + j * 20f;
                        Vector2 pos = minionPos + burstDir * dist;
                        float progress = j / 4f;
                        Color starColor = Color.Lerp(Gold, DeepPurple, progress);
                        CustomParticles.GenericFlare(pos, starColor * (1f - progress * 0.4f), 0.25f - progress * 0.1f, 12);
                    }
                }

                // Halo rings
                for (int ring = 0; ring < 3; ring++)
                {
                    float ringProgress = ring / 3f;
                    Color ringColor = Color.Lerp(Gold, Violet, ringProgress);
                    CustomParticles.HaloRing(minionPos, ringColor, 0.3f + ring * 0.1f, 14 + ring * 2);
                }

                // Damage enemies near this minion
                foreach (NPC npc in Main.npc)
                {
                    if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;

                    float dist = Vector2.Distance(minionPos, npc.Center);
                    if (dist < strikeRadius)
                    {
                        if (Main.myPlayer == Player.whoAmI)
                        {
                            // Damage falloff based on distance
                            float falloff = 1f - (dist / strikeRadius) * 0.4f;
                            int finalDamage = (int)(baseDamage * falloff);
                            Player.ApplyDamageToNPC(npc, finalDamage, 2f, Player.direction, false);
                        }

                        // VFX on hit enemy
                        CustomParticles.GenericFlare(npc.Center, Gold, 0.4f, 12);
                    }
                }

                // Dynamic lighting
                Lighting.AddLight(minionPos, Gold.ToVector3() * 1.5f);
            }
        }

        private void DrawConstellationLine(Vector2 start, Vector2 end)
        {
            Vector2 direction = end - start;
            float distance = direction.Length();
            direction.Normalize();

            int segments = (int)(distance / 20f);
            for (int i = 0; i <= segments; i++)
            {
                float progress = (float)i / segments;
                Vector2 point = Vector2.Lerp(start, end, progress);

                // Color gradient along line
                Color lineColor = Color.Lerp(Gold, Violet, progress);
                float scale = 0.2f * (1f - System.Math.Abs(progress - 0.5f) * 0.3f);

                CustomParticles.GenericFlare(point, lineColor * 0.7f, scale, 10);
            }

            // Star at each end
            CustomParticles.GenericFlare(start, StarWhite, 0.3f, 12);
            CustomParticles.GenericFlare(end, StarWhite, 0.3f, 12);
        }
    }
}
