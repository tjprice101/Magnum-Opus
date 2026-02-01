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
using static MagnumOpus.Common.Systems.ThemedParticles;
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
                var particle = new GenericGlowParticle(particlePos, particleVel, seasonColor * 0.45f, 0.2f, 18, true);
                MagnumParticleHandler.SpawnParticle(particle);
                
                // ☁ESPARKLE accent
                var sparkle = new SparkleParticle(particlePos, particleVel * 0.5f, seasonColor * 0.5f, 0.18f, 14);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // ☁EMUSICAL NOTATION - Conductor's notes! - VISIBLE SCALE 0.68f+
            if (Main.rand.NextBool(16))
            {
                int seasonIndex = (int)(Main.GameUpdateCount / 60) % 4;
                Color noteColor = seasonIndex switch { 0 => SpringPink, 1 => SummerGold, 2 => AutumnOrange, _ => WinterBlue };
                Vector2 notePos = player.Center + new Vector2(player.direction * 22f, -10f) + Main.rand.NextVector2Circular(6f, 6f);
                Vector2 noteVel = new Vector2(player.direction * 0.5f, -1.2f);
                ThemedParticles.MusicNote(notePos, noteVel, noteColor * 0.85f, 0.75f, 26);
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

            // Harmony Resonance visual when all 4 seasons present - with sparkles!
            if (hasSpring && hasSummer && hasAutumn && hasWinter && Main.rand.NextBool(8))
            {
                float angle = Main.GameUpdateCount * 0.03f;
                for (int i = 0; i < 4; i++)
                {
                    float harmonyAngle = angle + MathHelper.PiOver2 * i;
                    Vector2 harmonyPos = player.Center + harmonyAngle.ToRotationVector2() * 35f;
                    Color harmonyColor = i switch { 0 => SpringPink, 1 => SummerGold, 2 => AutumnOrange, _ => WinterBlue };
                    var harmonyParticle = new GenericGlowParticle(harmonyPos, Vector2.Zero, harmonyColor * 0.4f, 0.22f, 15, true);
                    MagnumParticleHandler.SpawnParticle(harmonyParticle);
                    
                    // ☁ESPARKLE at harmony points
                    var sparkle = new SparkleParticle(harmonyPos, Vector2.Zero, harmonyColor * 0.5f, 0.2f, 12);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
                
                // ☁EMUSICAL NOTATION - Harmony note at center! - VISIBLE SCALE 0.7f+
                if (Main.rand.NextBool(3))
                {
                    int noteIndex = Main.rand.Next(4);
                    Color noteColor = noteIndex switch { 0 => SpringPink, 1 => SummerGold, 2 => AutumnOrange, _ => WinterBlue };
                    ThemedParticles.MusicNote(player.Center, new Vector2(0, -1.5f), noteColor, 0.7f, 28);
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

                            // Coordination VFX on each minion with sparkles
                            Color minionColor = proj.type == ModContent.ProjectileType<Projectiles.SpringSpiritMinion>() ? SpringPink :
                                               proj.type == ModContent.ProjectileType<Projectiles.SummerSpiritMinion>() ? SummerGold :
                                               proj.type == ModContent.ProjectileType<Projectiles.AutumnSpiritMinion>() ? AutumnOrange : WinterBlue;
                            CustomParticles.GenericFlare(proj.Center, Color.White * 0.7f, 0.55f, 18);
                            CustomParticles.GenericFlare(proj.Center, minionColor, 0.52f, 16);
                            CustomParticles.HaloRing(proj.Center, minionColor * 0.55f, 0.35f, 14);
                            
                            // ☁ESPARKLE accent
                            var sparkle = new SparkleParticle(proj.Center, Main.rand.NextVector2Circular(2f, 2f), minionColor * 0.6f, 0.2f, 14);
                            MagnumParticleHandler.SpawnParticle(sparkle);
                        }
                    }
                }

                if (minionCount > 0)
                {
                    // Grand coordination burst at player - layered bloom with music notes!
                    CustomParticles.GenericFlare(player.Center, Color.White, 0.9f, 24);
                    CustomParticles.GenericFlare(player.Center, Color.White * 0.7f, 0.7f, 20);
                    for (int i = 0; i < 4; i++)
                    {
                        Color burstColor = i switch { 0 => SpringPink, 1 => SummerGold, 2 => AutumnOrange, _ => WinterBlue };
                        CustomParticles.HaloRing(player.Center, burstColor * 0.45f, 0.28f + i * 0.12f, 16 + i * 2);
                        
                        // ☁ESPARKLE at each color position
                        float sparkleAngle = MathHelper.PiOver2 * i;
                        Vector2 sparklePos = player.Center + sparkleAngle.ToRotationVector2() * 20f;
                        var sparkle = new SparkleParticle(sparklePos, sparkleAngle.ToRotationVector2() * 2f, burstColor * 0.6f, 0.22f, 16);
                        MagnumParticleHandler.SpawnParticle(sparkle);
                    }
                    
                    // ☁EMUSICAL NOTATION - Symphony coordination notes! - VISIBLE SCALE 0.72f+
                    for (int n = 0; n < 4; n++)
                    {
                        float noteAngle = MathHelper.PiOver2 * n + Main.rand.NextFloat(-0.3f, 0.3f);
                        Vector2 noteVel = noteAngle.ToRotationVector2() * Main.rand.NextFloat(2f, 3.5f);
                        Color noteColor = n switch { 0 => SpringPink, 1 => SummerGold, 2 => AutumnOrange, _ => WinterBlue };
                        ThemedParticles.MusicNote(player.Center, noteVel, noteColor, 0.72f, 30);
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
                        // Despawn VFX with sparkle
                        Color despawnColor = proj.type == ModContent.ProjectileType<Projectiles.SpringSpiritMinion>() ? SpringPink :
                                            proj.type == ModContent.ProjectileType<Projectiles.SummerSpiritMinion>() ? SummerGold :
                                            proj.type == ModContent.ProjectileType<Projectiles.AutumnSpiritMinion>() ? AutumnOrange : WinterBlue;
                        CustomParticles.GenericFlare(proj.Center, Color.White * 0.6f, 0.55f, 16);
                        CustomParticles.GenericFlare(proj.Center, despawnColor, 0.52f, 15);
                        CustomParticles.HaloRing(proj.Center, despawnColor * 0.45f, 0.32f, 12);
                        
                        // ☁ESPARKLE on despawn
                        var sparkle = new SparkleParticle(proj.Center, Main.rand.NextVector2Circular(1.5f, 1.5f), despawnColor * 0.55f, 0.2f, 12);
                        MagnumParticleHandler.SpawnParticle(sparkle);
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

            // Grand summon VFX - all 4 colors in a spectacular burst with music notes!
            CustomParticles.GenericFlare(spawnPos, Color.White, 1.2f, 28);
            CustomParticles.GenericFlare(spawnPos, Color.White * 0.8f, 0.9f, 24);
            for (int s = 0; s < 4; s++)
            {
                float angle = MathHelper.PiOver2 * s;
                Vector2 seasonSpawnPos = spawnPos + angle.ToRotationVector2() * 35f;
                
                CustomParticles.GenericFlare(seasonSpawnPos, Color.White * 0.7f, 0.7f, 22);
                CustomParticles.GenericFlare(seasonSpawnPos, seasonColors[s], 0.68f, 20);
                CustomParticles.HaloRing(seasonSpawnPos, seasonColors[s] * 0.55f, 0.42f, 16);

                for (int i = 0; i < 8; i++)
                {
                    float burstAngle = MathHelper.TwoPi * i / 8f;
                    Vector2 burstVel = burstAngle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                    var burst = new GenericGlowParticle(seasonSpawnPos, burstVel, seasonColors[s] * 0.55f, 0.25f, 16, true);
                    MagnumParticleHandler.SpawnParticle(burst);
                    
                    // ☁ESPARKLE accents
                    if (i % 2 == 0)
                    {
                        var sparkle = new SparkleParticle(seasonSpawnPos, burstVel * 0.6f, seasonColors[s] * 0.6f, 0.2f, 14);
                        MagnumParticleHandler.SpawnParticle(sparkle);
                    }
                }
                
                // ☁EMUSICAL NOTATION - Season note on summon! - VISIBLE SCALE 0.72f+
                Vector2 noteVel = new Vector2(0, -2f).RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f));
                ThemedParticles.MusicNote(seasonSpawnPos, noteVel, seasonColors[s], 0.72f, 30);

                // Spawn the spirit
                Projectile.NewProjectile(source, seasonSpawnPos, Vector2.Zero, minionTypes[s], damage, knockback, player.whoAmI);
            }

            // Harmony burst at center with sparkles and music notes
            for (int i = 0; i < 4; i++)
            {
                CustomParticles.HaloRing(spawnPos, seasonColors[i] * 0.4f, 0.28f + i * 0.14f, 14 + i * 3);
                
                // ☁ESPARKLE at harmony points
                float sparkleAngle = MathHelper.PiOver2 * i;
                Vector2 sparklePos = spawnPos + sparkleAngle.ToRotationVector2() * 15f;
                var sparkle = new SparkleParticle(sparklePos, sparkleAngle.ToRotationVector2() * 1.5f, seasonColors[i] * 0.55f, 0.2f, 14);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // ☁EMUSICAL NOTATION - Central harmony note! - VISIBLE SCALE 0.75f+
            ThemedParticles.MusicNote(spawnPos, new Vector2(0, -2f), Color.White * 0.9f, 0.75f, 32);

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
