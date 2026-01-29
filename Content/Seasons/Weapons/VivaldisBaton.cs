using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Spring.Materials;
using MagnumOpus.Content.Summer.Materials;
using MagnumOpus.Content.Autumn.Materials;
using MagnumOpus.Content.Winter.Materials;
using MagnumOpus.Content.Spring.Weapons;
using MagnumOpus.Content.Summer.Weapons;
using MagnumOpus.Content.Autumn.Weapons;
using MagnumOpus.Content.Winter.Weapons;

namespace MagnumOpus.Content.Seasons.Weapons
{
    /// <summary>
    /// Vivaldi's Baton - Ultimate Vivaldi Summon Weapon
    /// Post-Moon Lord summon staff that summons seasonal spirits
    /// 
    /// MECHANICS:
    /// - Left Click: Summon Season Spirit - Cycles through seasonal minion types
    /// - Right Click: Symphony Coordination - Conducts all minions for synchronized attack
    /// - Each season has a unique minion with different abilities
    /// - Having all 4 seasons creates Harmony Resonance bonus
    /// 
    /// SEASONAL MINIONS:
    /// - Spring Spirit: Fast, supportive, heals on hit
    /// - Summer Spirit: Aggressive, fires solar bolts
    /// - Autumn Spirit: Area control, life drain aura
    /// - Winter Spirit: Defensive, slows and freezes enemies
    /// 
    /// CRAFTING: All 4 seasonal bars + all 4 resonant energies + Stardust Fragments @ Lunar Crafting Station
    /// </summary>
    public class VivaldisBaton : ModItem
    {
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SummerGold = new Color(255, 215, 0);
        private static readonly Color AutumnOrange = new Color(255, 140, 50);
        private static readonly Color WinterBlue = new Color(150, 220, 255);

        public override void SetDefaults()
        {
            Item.damage = 145;
            Item.DamageType = DamageClass.Summon;
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(gold: 35);
            Item.rare = ItemRarityID.Red;
            Item.shoot = ModContent.ProjectileType<Projectiles.SpringSpiritMinion>();
            Item.buffType = ModContent.BuffType<VivaldiConductorBuff>();
            Item.mana = 12;
            Item.UseSound = SoundID.Item44;
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Right click - Symphony Coordination
                Item.useTime = 45;
                Item.useAnimation = 45;
                Item.mana = 30;
            }
            else
            {
                // Left click - Summon
                Item.useTime = 30;
                Item.useAnimation = 30;
                Item.mana = 12;
            }

            return base.CanUseItem(player);
        }

        public override void HoldItem(Player player)
        {
            // Ambient conductor particles - cycle through all 4 seasons
            if (Main.rand.NextBool(12))
            {
                int seasonIndex = (int)(Main.GameUpdateCount / 60) % 4;
                Color seasonColor = seasonIndex switch { 0 => SpringPink, 1 => SummerGold, 2 => AutumnOrange, _ => WinterBlue };
                Vector2 particlePos = player.Center + new Vector2(player.direction * 18f, -6f) + Main.rand.NextVector2Circular(10f, 10f);
                Vector2 particleVel = new Vector2(0, Main.rand.NextFloat(-2f, -0.5f));
                var particle = new GenericGlowParticle(particlePos, particleVel, seasonColor * 0.4f, 0.18f, 18, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // Check for Harmony Resonance (all 4 seasons present)
            bool hasSpring = false, hasSummer = false, hasAutumn = false, hasWinter = false;
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.owner == player.whoAmI)
                {
                    if (proj.type == ModContent.ProjectileType<Projectiles.SpringSpiritMinion>()) hasSpring = true;
                    else if (proj.type == ModContent.ProjectileType<Projectiles.SummerSpiritMinion>()) hasSummer = true;
                    else if (proj.type == ModContent.ProjectileType<Projectiles.AutumnSpiritMinion>()) hasAutumn = true;
                    else if (proj.type == ModContent.ProjectileType<Projectiles.WinterSpiritMinion>()) hasWinter = true;
                }
            }

            // Harmony Resonance visual when all 4 seasons present
            if (hasSpring && hasSummer && hasAutumn && hasWinter && Main.rand.NextBool(8))
            {
                float angle = Main.GameUpdateCount * 0.03f;
                for (int i = 0; i < 4; i++)
                {
                    float harmonyAngle = angle + MathHelper.PiOver2 * i;
                    Vector2 harmonyPos = player.Center + harmonyAngle.ToRotationVector2() * 35f;
                    Color harmonyColor = i switch { 0 => SpringPink, 1 => SummerGold, 2 => AutumnOrange, _ => WinterBlue };
                    var harmonyParticle = new GenericGlowParticle(harmonyPos, Vector2.Zero, harmonyColor * 0.35f, 0.2f, 15, true);
                    MagnumParticleHandler.SpawnParticle(harmonyParticle);
                }
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                // Right click - Symphony Coordination
                // All minions perform synchronized attack toward cursor
                Vector2 targetPos = Main.MouseWorld;
                int minionCount = 0;

                foreach (Projectile proj in Main.projectile)
                {
                    if (proj.active && proj.owner == player.whoAmI && proj.minion)
                    {
                        // Check if it's a Vivaldi minion
                        if (proj.type == ModContent.ProjectileType<Projectiles.SpringSpiritMinion>() ||
                            proj.type == ModContent.ProjectileType<Projectiles.SummerSpiritMinion>() ||
                            proj.type == ModContent.ProjectileType<Projectiles.AutumnSpiritMinion>() ||
                            proj.type == ModContent.ProjectileType<Projectiles.WinterSpiritMinion>())
                        {
                            // Trigger synchronized attack (stored in ai[1])
                            proj.ai[1] = 60; // 60 frames of coordinated attack
                            proj.netUpdate = true;
                            minionCount++;

                            // Coordination VFX on each minion
                            Color minionColor = proj.type == ModContent.ProjectileType<Projectiles.SpringSpiritMinion>() ? SpringPink :
                                               proj.type == ModContent.ProjectileType<Projectiles.SummerSpiritMinion>() ? SummerGold :
                                               proj.type == ModContent.ProjectileType<Projectiles.AutumnSpiritMinion>() ? AutumnOrange : WinterBlue;
                            CustomParticles.GenericFlare(proj.Center, minionColor, 0.5f, 16);
                            CustomParticles.HaloRing(proj.Center, minionColor * 0.5f, 0.3f, 14);
                        }
                    }
                }

                if (minionCount > 0)
                {
                    // Grand coordination burst at player
                    CustomParticles.GenericFlare(player.Center, Color.White, 0.7f, 20);
                    for (int i = 0; i < 4; i++)
                    {
                        Color burstColor = i switch { 0 => SpringPink, 1 => SummerGold, 2 => AutumnOrange, _ => WinterBlue };
                        CustomParticles.HaloRing(player.Center, burstColor * 0.4f, 0.25f + i * 0.1f, 16 + i * 2);
                    }

                    SoundEngine.PlaySound(SoundID.Item4, player.Center);
                }

                return false;
            }

            // Left click - Summon ALL 4 seasonal spirits at once (or despawn if already summoned)
            // Check if spirits already exist
            bool hasSpirits = false;
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.owner == player.whoAmI &&
                    (proj.type == ModContent.ProjectileType<Projectiles.SpringSpiritMinion>() ||
                     proj.type == ModContent.ProjectileType<Projectiles.SummerSpiritMinion>() ||
                     proj.type == ModContent.ProjectileType<Projectiles.AutumnSpiritMinion>() ||
                     proj.type == ModContent.ProjectileType<Projectiles.WinterSpiritMinion>()))
                {
                    hasSpirits = true;
                    break;
                }
            }

            if (hasSpirits)
            {
                // Despawn all spirits
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj.active && proj.owner == player.whoAmI &&
                        (proj.type == ModContent.ProjectileType<Projectiles.SpringSpiritMinion>() ||
                         proj.type == ModContent.ProjectileType<Projectiles.SummerSpiritMinion>() ||
                         proj.type == ModContent.ProjectileType<Projectiles.AutumnSpiritMinion>() ||
                         proj.type == ModContent.ProjectileType<Projectiles.WinterSpiritMinion>()))
                    {
                        // Despawn VFX
                        Color despawnColor = proj.type == ModContent.ProjectileType<Projectiles.SpringSpiritMinion>() ? SpringPink :
                                            proj.type == ModContent.ProjectileType<Projectiles.SummerSpiritMinion>() ? SummerGold :
                                            proj.type == ModContent.ProjectileType<Projectiles.AutumnSpiritMinion>() ? AutumnOrange : WinterBlue;
                        CustomParticles.GenericFlare(proj.Center, despawnColor, 0.5f, 15);
                        CustomParticles.HaloRing(proj.Center, despawnColor * 0.4f, 0.3f, 12);
                        proj.Kill();
                    }
                }
                player.ClearBuff(Item.buffType);
                SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.5f }, player.Center);
                return false;
            }

            // Summon all 4 spirits!
            player.AddBuff(Item.buffType, 2);

            Vector2 spawnPos = Main.MouseWorld;
            Color[] seasonColors = { SpringPink, SummerGold, AutumnOrange, WinterBlue };
            int[] minionTypes = {
                ModContent.ProjectileType<Projectiles.SpringSpiritMinion>(),
                ModContent.ProjectileType<Projectiles.SummerSpiritMinion>(),
                ModContent.ProjectileType<Projectiles.AutumnSpiritMinion>(),
                ModContent.ProjectileType<Projectiles.WinterSpiritMinion>()
            };

            // Grand summon VFX - all 4 colors in a spectacular burst
            CustomParticles.GenericFlare(spawnPos, Color.White, 1.0f, 25);
            for (int s = 0; s < 4; s++)
            {
                float angle = MathHelper.PiOver2 * s;
                Vector2 seasonSpawnPos = spawnPos + angle.ToRotationVector2() * 35f;
                
                CustomParticles.GenericFlare(seasonSpawnPos, seasonColors[s], 0.65f, 20);
                CustomParticles.HaloRing(seasonSpawnPos, seasonColors[s] * 0.5f, 0.4f, 16);

                for (int i = 0; i < 8; i++)
                {
                    float burstAngle = MathHelper.TwoPi * i / 8f;
                    Vector2 burstVel = burstAngle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                    var burst = new GenericGlowParticle(seasonSpawnPos, burstVel, seasonColors[s] * 0.5f, 0.22f, 16, true);
                    MagnumParticleHandler.SpawnParticle(burst);
                }

                // Spawn the spirit
                Projectile.NewProjectile(source, seasonSpawnPos, Vector2.Zero, minionTypes[s], damage, knockback, player.whoAmI);
            }

            // Harmony burst at center
            for (int i = 0; i < 4; i++)
            {
                CustomParticles.HaloRing(spawnPos, seasonColors[i] * 0.35f, 0.25f + i * 0.12f, 14 + i * 3);
            }

            SoundEngine.PlaySound(SoundID.Item44 with { Volume = 1.2f }, spawnPos);

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            // Check how many minion slots player has for display
            Player player = Main.LocalPlayer;
            float damageBonus = Math.Min(3f, 1f + (player.maxMinions - 1) * 0.15f);
            float speedBonus = Math.Min(2f, 1f + (player.maxMinions - 1) * 0.08f);

            tooltips.Add(new TooltipLine(Mod, "Summon", "Summons all four Seasonal Spirits at once") { OverrideColor = new Color(255, 220, 180) });
            tooltips.Add(new TooltipLine(Mod, "MinionBonus", $"Spirit power scales with minion slots ({player.maxMinions} slots = {damageBonus:F1}x damage, {speedBonus:F1}x speed)") { OverrideColor = new Color(180, 220, 255) });
            tooltips.Add(new TooltipLine(Mod, "RightClick", "Right click triggers Symphony Coordination - all spirits attack in unison"));
            tooltips.Add(new TooltipLine(Mod, "Harmony", "All 4 seasons together grant Harmony Resonance (+20% minion damage)") { OverrideColor = new Color(255, 220, 180) });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The conductor who commands the seasons themselves'") { OverrideColor = new Color(180, 150, 255) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                // Combine the 4 lower-tier seasonal summon weapons
                .AddIngredient(ModContent.ItemType<PrimaverasBloom>(), 1)
                .AddIngredient(ModContent.ItemType<SolarCrest>(), 1)
                .AddIngredient(ModContent.ItemType<DecayBell>(), 1)
                .AddIngredient(ModContent.ItemType<FrozenHeart>(), 1)
                // Plus 10 of each Seasonal Resonant Energy
                .AddIngredient(ModContent.ItemType<SpringResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<SummerResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<AutumnResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<WinterResonantEnergy>(), 10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    /// <summary>
    /// Vivaldi Conductor Buff - Grants Harmony Resonance when all 4 seasons present
    /// </summary>
    public class VivaldiConductorBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.Swiftness;
        
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Projectiles.SpringSpiritMinion>()] > 0 ||
                player.ownedProjectileCounts[ModContent.ProjectileType<Projectiles.SummerSpiritMinion>()] > 0 ||
                player.ownedProjectileCounts[ModContent.ProjectileType<Projectiles.AutumnSpiritMinion>()] > 0 ||
                player.ownedProjectileCounts[ModContent.ProjectileType<Projectiles.WinterSpiritMinion>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
                return;
            }

            // Check for Harmony Resonance
            bool hasSpring = player.ownedProjectileCounts[ModContent.ProjectileType<Projectiles.SpringSpiritMinion>()] > 0;
            bool hasSummer = player.ownedProjectileCounts[ModContent.ProjectileType<Projectiles.SummerSpiritMinion>()] > 0;
            bool hasAutumn = player.ownedProjectileCounts[ModContent.ProjectileType<Projectiles.AutumnSpiritMinion>()] > 0;
            bool hasWinter = player.ownedProjectileCounts[ModContent.ProjectileType<Projectiles.WinterSpiritMinion>()] > 0;

            if (hasSpring && hasSummer && hasAutumn && hasWinter)
            {
                // Harmony Resonance - +20% minion damage
                player.GetDamage(DamageClass.Summon) += 0.2f;
            }
        }
    }
}
