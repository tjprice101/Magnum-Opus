using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Content.Nachtmusik.ResonanceEnergies;
using MagnumOpus.Content.Nachtmusik.HarmonicCores;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Nachtmusik.Accessories
{
    /// <summary>
    /// Radiance of the Night Queen - Universal accessory for Nachtmusik theme.
    /// A crystalline emblem containing the Queen's blessing, empowering all forms of combat.
    /// Periodically releases a nova of starlight that empowers the player and damages enemies.
    /// </summary>
    public class RadianceOfTheNightQueen : ModItem
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
            Item.value = Item.buyPrice(platinum: 4);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<RadianceOfTheNightQueenPlayer>();
            modPlayer.hasRadianceOfTheNightQueen = true;

            // +15% all damage
            player.GetDamage(DamageClass.Generic) += 0.15f;

            // +8% critical strike chance (all classes)
            player.GetCritChance(DamageClass.Generic) += 8;

            // +10% movement speed
            player.moveSpeed += 0.10f;

            // +1 minion slot
            player.maxMinions += 1;

            // Ambient radiant particles
            if (!hideVisual && Main.rand.NextBool(8))
            {
                Vector2 offset = Main.rand.NextVector2Circular(35f, 35f);
                float angle = offset.ToRotation();
                Vector2 velocity = angle.ToRotationVector2() * 0.5f;
                
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.GoldFlame;
                Dust dust = Dust.NewDustPerfect(player.Center + offset, dustType, velocity, 100, default, 1.0f);
                dust.noGravity = true;
            }

            // Orbiting golden stars
            if (!hideVisual && Main.rand.NextBool(15))
            {
                float orbitAngle = Main.GameUpdateCount * 0.03f + Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 orbitPos = player.Center + orbitAngle.ToRotationVector2() * 40f;
                CustomParticles.GenericFlare(orbitPos, Gold * 0.6f, 0.2f, 12);
            }
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "AllDamage", "+15% damage")
            {
                OverrideColor = Gold
            });

            tooltips.Add(new TooltipLine(Mod, "AllCrit", "+8% critical strike chance")
            {
                OverrideColor = StarWhite
            });

            tooltips.Add(new TooltipLine(Mod, "MoveSpeed", "+10% movement speed")
            {
                OverrideColor = Violet
            });

            tooltips.Add(new TooltipLine(Mod, "MinionSlot", "+1 max minion")
            {
                OverrideColor = DeepPurple
            });

            tooltips.Add(new TooltipLine(Mod, "RadianceNova", "Every 10 seconds, releases a nova of starlight")
            {
                OverrideColor = Color.Lerp(DeepPurple, Gold, 0.4f)
            });

            tooltips.Add(new TooltipLine(Mod, "NovaEffect", "The nova grants +25% damage for 4 seconds and damages nearby enemies")
            {
                OverrideColor = Color.Lerp(Violet, Gold, 0.5f)
            });

            tooltips.Add(new TooltipLine(Mod, "Flavor", "'Blessed by the Queen herself, her radiance flows through you'")
            {
                OverrideColor = Color.Lerp(DeepPurple, Violet, 0.3f)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<NachtmusikResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<NachtmusikResonantCore>(), 6)
                .AddIngredient(ItemID.FragmentSolar, 5)
                .AddIngredient(ItemID.FragmentNebula, 5)
                .AddIngredient(ItemID.FragmentVortex, 5)
                .AddIngredient(ItemID.FragmentStardust, 5)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }

    public class RadianceOfTheNightQueenPlayer : ModPlayer
    {
        public bool hasRadianceOfTheNightQueen = false;
        private int novaTimer = 0;
        private const int NovaCooldown = 600; // 10 seconds
        private const int BuffDuration = 240; // 4 seconds

        // Nachtmusik colors
        private static readonly Color DeepPurple = new Color(45, 27, 78);
        private static readonly Color Gold = new Color(255, 215, 0);
        private static readonly Color Violet = new Color(123, 104, 238);
        private static readonly Color StarWhite = new Color(255, 255, 255);

        public override void ResetEffects()
        {
            hasRadianceOfTheNightQueen = false;
        }

        public override void PostUpdate()
        {
            if (!hasRadianceOfTheNightQueen)
            {
                novaTimer = 0;
                return;
            }

            novaTimer++;

            // Visual buildup as nova approaches
            if (novaTimer > NovaCooldown - 60 && novaTimer % 10 == 0)
            {
                float progress = (novaTimer - (NovaCooldown - 60)) / 60f;
                int particleCount = (int)(3 + progress * 5);
                
                for (int i = 0; i < particleCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / particleCount + Main.GameUpdateCount * 0.05f;
                    float radius = 60f * (1f - progress * 0.5f);
                    Vector2 pos = Player.Center + angle.ToRotationVector2() * radius;
                    Color color = Color.Lerp(DeepPurple, Gold, progress);
                    CustomParticles.GenericFlare(pos, color, 0.25f + progress * 0.15f, 10);
                }
            }

            if (novaTimer >= NovaCooldown)
            {
                TriggerRadianceNova();
                novaTimer = 0;
            }
        }

        private void TriggerRadianceNova()
        {
            // Grant damage buff
            Player.AddBuff(ModContent.BuffType<QueensRadianceBuff>(), BuffDuration);

            // Massive visual nova
            // Central burst
            CustomParticles.GenericFlare(Player.Center, StarWhite, 1.5f, 25);
            CustomParticles.GenericFlare(Player.Center, Gold, 1.2f, 22);
            CustomParticles.GenericFlare(Player.Center, Violet, 0.9f, 20);

            // Expanding halo rings
            for (int ring = 0; ring < 6; ring++)
            {
                float ringProgress = ring / 6f;
                Color ringColor = Color.Lerp(Gold, DeepPurple, ringProgress);
                CustomParticles.HaloRing(Player.Center, ringColor, 0.4f + ring * 0.15f, 18 + ring * 3);
            }

            // Radial star burst
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 burstDir = angle.ToRotationVector2();
                
                // Star streak
                for (int j = 0; j < 6; j++)
                {
                    float dist = 20f + j * 25f;
                    Vector2 pos = Player.Center + burstDir * dist;
                    float progress = j / 6f;
                    Color starColor = Color.Lerp(StarWhite, Gold, progress);
                    CustomParticles.GenericFlare(pos, starColor * (1f - progress * 0.3f), 0.35f - progress * 0.15f, 15);
                }
            }

            // Damage nearby enemies
            float novaRadius = 300f;
            int novaDamage = (int)(Player.GetTotalDamage(DamageClass.Generic).ApplyTo(150));

            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;

                float dist = Vector2.Distance(Player.Center, npc.Center);
                if (dist < novaRadius)
                {
                    if (Main.myPlayer == Player.whoAmI)
                    {
                        // Damage falloff based on distance
                        float falloff = 1f - (dist / novaRadius) * 0.5f;
                        int finalDamage = (int)(novaDamage * falloff);
                        Player.ApplyDamageToNPC(npc, finalDamage, 0f, 0, false);
                    }

                    // VFX on hit enemy
                    CustomParticles.GenericFlare(npc.Center, Gold, 0.5f, 15);
                    CustomParticles.HaloRing(npc.Center, Violet, 0.3f, 12);
                }
            }

            // Dynamic lighting pulse
            Lighting.AddLight(Player.Center, Gold.ToVector3() * 2f);
        }
    }

    /// <summary>
    /// Buff granted by Radiance of the Night Queen nova.
    /// </summary>
    public class QueensRadianceBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.Shine;

        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // +25% damage during radiance
            player.GetDamage(DamageClass.Generic) += 0.25f;
        }
    }
}
