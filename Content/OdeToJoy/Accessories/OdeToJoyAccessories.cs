using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;
using MagnumOpus.Content.OdeToJoy.HarmonicCores;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Common.Systems;
using MagnumOpus.Content.OdeToJoy;
using MagnumOpus.Content.Materials.EnemyDrops;

namespace MagnumOpus.Content.OdeToJoy.Accessories
{
    /// <summary>
    /// The Flowering Coda - Magic class accessory
    /// Boosts magic damage, causes spells to bloom with healing energy
    /// Post-Dies Irae tier (stronger than Dies Irae)
    /// </summary>
    public class TheFloweringCoda : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 34;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 3);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+60% magic damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+35% magic critical strike chance"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Magic attacks create blooming petals that heal 3% of damage dealt"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "-30% mana cost"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Every 10th magic hit creates a joyous bloom explosion"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The final flourish of nature's triumphant symphony'") 
            { 
                OverrideColor = OdeToJoyPalette.VerdantGreen 
            });
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Magic) += 0.60f; // POST-DIES IRAE (Dies Irae: 0.45f)
            player.GetCritChance(DamageClass.Magic) += 35;
            player.manaCost -= 0.30f;
            
            player.GetModPlayer<FloweringCodaPlayer>().floweringCodaActive = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfOdeToJoy>(), 18)
                .AddIngredient(ModContent.ItemType<OdeToJoyResonantEnergy>(), 12)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfOdeToJoy>(), 1)
                .AddIngredient(ModContent.ItemType<JoyEssence>(), 8)
                .AddIngredient(ModContent.ItemType<ShardOfOdeToJoysTempo>(), 5)
                .AddIngredient(ItemID.LunarBar, 15)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    public class FloweringCodaPlayer : ModPlayer
    {
        public bool floweringCodaActive = false;
        public int magicHitCounter = 0;

        public override void ResetEffects()
        {
            floweringCodaActive = false;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (floweringCodaActive && proj.DamageType == DamageClass.Magic && proj.friendly)
            {
                // 3% healing
                int healAmount = (int)(damageDone * 0.03f);
                if (healAmount > 0)
                {
                    Player.Heal(healAmount);
                }
                
                // Every 10th hit: joyous bloom explosion
                magicHitCounter++;
                if (magicHitCounter >= 10)
                {
                    magicHitCounter = 0;
                    
                    // Deal bonus damage in area
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (npc.active && npc.CanBeChasedBy(proj) && Vector2.Distance(npc.Center, target.Center) < 150f)
                        {
                            Player.ApplyDamageToNPC(npc, damageDone / 3, 0f, 0, false);
                        }
                    }

                    // Bloom trigger VFX: green sparkle dust
                    for (int d = 0; d < 3; d++)
                    {
                        Dust dust = Dust.NewDustDirect(target.Center + Main.rand.NextVector2Circular(14f, 14f),
                            0, 0, DustID.GreenFairy, Main.rand.NextFloat(-1f, 1f), -1.5f, 100, default, 1.1f);
                        dust.noGravity = true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// The Verdant Refrain - Summoner class accessory
    /// Boosts summon damage and adds nature-themed effects
    /// Post-Dies Irae tier
    /// </summary>
    public class TheVerdantRefrain : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 3);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+70% summon damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+4 max minions"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Minions slow enemies on hit"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Minion hits may plant healing flowers that restore health nearby"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "+25% whip speed and range"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The endless melody of nature's eternal chorus'") 
            { 
                OverrideColor = OdeToJoyPalette.VerdantGreen 
            });
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Summon) += 0.70f; // POST-DIES IRAE (Dies Irae: 0.55f)
            player.maxMinions += 4;
            player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += 0.25f;
            player.whipRangeMultiplier += 0.25f;
            
            player.GetModPlayer<VerdantRefrainPlayer>().verdantRefrainActive = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfOdeToJoy>(), 18)
                .AddIngredient(ModContent.ItemType<OdeToJoyResonantEnergy>(), 12)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfOdeToJoy>(), 1)
                .AddIngredient(ModContent.ItemType<JoyEssence>(), 8)
                .AddIngredient(ModContent.ItemType<ShardOfOdeToJoysTempo>(), 5)
                .AddIngredient(ItemID.LunarBar, 15)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    public class VerdantRefrainPlayer : ModPlayer
    {
        public bool verdantRefrainActive = false;

        // Healing flower tracking
        private Vector2[] flowerPositions = new Vector2[3];
        private int[] flowerTimers = new int[3];
        private int activeFlowerCount = 0;

        public override void ResetEffects()
        {
            verdantRefrainActive = false;
        }

        public override void PostUpdate()
        {
            // Update healing flowers
            for (int i = 0; i < 3; i++)
            {
                if (flowerTimers[i] > 0)
                {
                    flowerTimers[i]--;

                    // Heal 2 HP/sec while player is within 200px
                    if (flowerTimers[i] % 30 == 0 && Vector2.Distance(Player.Center, flowerPositions[i]) < 200f)
                    {
                        Player.Heal(1);
                    }

                    // Ambient flower dust every 30 frames
                    if (flowerTimers[i] % 30 == 0)
                    {
                        Dust dust = Dust.NewDustDirect(flowerPositions[i] + Main.rand.NextVector2Circular(8f, 8f),
                            0, 0, DustID.Grass, 0f, -0.5f, 100, default, 0.7f);
                        dust.noGravity = true;
                    }

                    if (flowerTimers[i] <= 0)
                        activeFlowerCount = System.Math.Max(0, activeFlowerCount - 1);
                }
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (verdantRefrainActive && proj.minion && proj.friendly)
            {
                // Slow enemies
                target.AddBuff(BuffID.Slow, 180); // 3 seconds

                // 15% chance to plant a healing flower
                if (Main.rand.NextFloat() < 0.15f && activeFlowerCount < 3)
                {
                    // Find an available slot
                    for (int i = 0; i < 3; i++)
                    {
                        if (flowerTimers[i] <= 0)
                        {
                            flowerPositions[i] = target.Center;
                            flowerTimers[i] = 300; // 5 seconds
                            activeFlowerCount++;

                            // Spawn VFX: green + pink upward dust
                            for (int d = 0; d < 3; d++)
                            {
                                int dustType = d < 2 ? DustID.Grass : DustID.PinkFairy;
                                Dust dust = Dust.NewDustDirect(target.Center + Main.rand.NextVector2Circular(10f, 10f),
                                    0, 0, dustType, 0f, -1.5f, 100, default, 0.9f);
                                dust.noGravity = true;
                            }
                            break;
                        }
                    }
                }

                // On-hit VFX: 1 green leaf dust
                if (Main.rand.NextBool(3))
                {
                    Dust dust = Dust.NewDustDirect(target.Center + Main.rand.NextVector2Circular(8f, 8f),
                        0, 0, DustID.Grass, 0f, -0.5f, 100, default, 0.7f);
                    dust.noGravity = true;
                }
            }
        }
    }

    /// <summary>
    /// Conductor's Corsage - Melee class accessory
    /// Boosts melee damage and adds blooming lifesteal
    /// Post-Dies Irae tier
    /// </summary>
    public class ConductorsCorsage : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 36;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 3);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+65% melee damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+40% melee speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Melee hits build towards a triumphant crescendo"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Every 8th hit releases a golden shockwave that heals"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Critical strikes heal nearby allies"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "+20 defense while attacking"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The flower that blooms upon the conductor's heart, spreading joy to all'") 
            { 
                OverrideColor = OdeToJoyPalette.RosePink 
            });
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Melee) += 0.65f; // POST-DIES IRAE (Dies Irae: 0.50f)
            player.GetAttackSpeed(DamageClass.Melee) += 0.40f;
            
            player.GetModPlayer<ConductorsCorsagePlayer>().conductorsCorsageActive = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfOdeToJoy>(), 18)
                .AddIngredient(ModContent.ItemType<OdeToJoyResonantEnergy>(), 12)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfOdeToJoy>(), 1)
                .AddIngredient(ModContent.ItemType<JoyEssence>(), 8)
                .AddIngredient(ModContent.ItemType<ShardOfOdeToJoysTempo>(), 5)
                .AddIngredient(ItemID.LunarBar, 15)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    public class ConductorsCorsagePlayer : ModPlayer
    {
        public bool conductorsCorsageActive = false;
        public int attackTimer = 0;
        public int crescendoCounter = 0;

        public override void ResetEffects()
        {
            conductorsCorsageActive = false;
        }

        public override void PostUpdateEquips()
        {
            // +20 defense while attacking (recent attack within 60 frames)
            if (conductorsCorsageActive && attackTimer > 0)
            {
                Player.statDefense += 20;
                attackTimer--;
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (conductorsCorsageActive && item.DamageType == DamageClass.Melee)
            {
                ProcessMeleeHit(target, hit, damageDone);
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (conductorsCorsageActive && proj.DamageType == DamageClass.Melee && proj.friendly)
            {
                ProcessMeleeHit(target, hit, damageDone);
            }
        }

        private void ProcessMeleeHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            attackTimer = 60;

            // On-hit VFX: 1 warm gold dust per hit
            Dust dust = Dust.NewDustDirect(target.Center + Main.rand.NextVector2Circular(10f, 10f),
                0, 0, DustID.GoldFlame, 0f, -0.5f, 100, default, 0.8f);
            dust.noGravity = true;

            // Crescendo counter
            crescendoCounter++;
            if (crescendoCounter >= 8)
            {
                crescendoCounter = 0;

                // Golden shockwave: AoE damage in 200px
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && npc.CanBeChasedBy() && Vector2.Distance(npc.Center, target.Center) < 200f)
                    {
                        Player.ApplyDamageToNPC(npc, damageDone * 3 / 10, 0f, 0, false);
                    }
                }

                // Heal player
                Player.Heal(25);

                // Shockwave VFX: gold dust expanding outward
                for (int d = 0; d < 5; d++)
                {
                    Vector2 vel = Main.rand.NextVector2CircularEdge(4f, 4f);
                    Dust shockDust = Dust.NewDustDirect(target.Center, 0, 0,
                        DustID.GoldFlame, vel.X, vel.Y, 80, default, 1.3f);
                    shockDust.noGravity = true;
                }
                Lighting.AddLight(target.Center, 1.0f, 0.85f, 0.3f);
            }

            // Critical strikes heal nearby allies (multiplayer)
            if (hit.Crit)
            {
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player ally = Main.player[i];
                    if (ally.active && !ally.dead && ally.whoAmI != Player.whoAmI &&
                        Vector2.Distance(ally.Center, target.Center) < 200f)
                    {
                        int allyHeal = (int)(damageDone * 0.05f);
                        if (allyHeal > 0)
                            ally.Heal(allyHeal);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Symphony of Blossoms - Ranged class accessory
    /// Boosts ranged damage and adds blooming mark mechanic
    /// Post-Dies Irae tier
    /// </summary>
    public class SymphonyOfBlossoms : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 3);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+65% ranged damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+40% ranged critical strike chance"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Ranged attacks slow enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Every 5th shot fires a volley of piercing thorns"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "20% chance to not consume ammo"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A symphony played in petals and thorns, celebrating nature's triumph'") 
            { 
                OverrideColor = OdeToJoyPalette.RosePink 
            });
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Ranged) += 0.65f; // POST-DIES IRAE (Dies Irae: 0.50f)
            player.GetCritChance(DamageClass.Ranged) += 40;
            player.ammoCost80 = true; // 20% chance to not consume ammo
            
            player.GetModPlayer<SymphonyOfBlossomsPlayer>().symphonyActive = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfOdeToJoy>(), 18)
                .AddIngredient(ModContent.ItemType<OdeToJoyResonantEnergy>(), 12)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfOdeToJoy>(), 1)
                .AddIngredient(ModContent.ItemType<JoyEssence>(), 8)
                .AddIngredient(ModContent.ItemType<ShardOfOdeToJoysTempo>(), 5)
                .AddIngredient(ItemID.LunarBar, 15)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    public class SymphonyOfBlossomsPlayer : ModPlayer
    {
        public bool symphonyActive = false;
        public int shotCounter = 0;

        public override void ResetEffects()
        {
            symphonyActive = false;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (symphonyActive && proj.DamageType == DamageClass.Ranged && proj.friendly && !proj.minion)
            {
                // Slow on hit
                target.AddBuff(BuffID.Slow, 360); // 6 seconds

                // On-hit VFX: rose-pink dust
                if (Main.rand.NextBool(3))
                {
                    Dust dust = Dust.NewDustDirect(target.Center + Main.rand.NextVector2Circular(8f, 8f),
                        0, 0, DustID.PinkFairy, 0f, -0.5f, 100, default, 0.8f);
                    dust.noGravity = true;
                }

                // Shot counter for thorn volley
                shotCounter++;
                if (shotCounter >= 5)
                {
                    shotCounter = 0;

                    // Fire 3 piercing seed projectiles
                    if (Main.myPlayer == Player.whoAmI)
                    {
                        Vector2 baseDir = (target.Center - Player.Center).SafeNormalize(Vector2.UnitX);
                        for (int i = -1; i <= 1; i++)
                        {
                            Vector2 direction = baseDir.RotatedBy(i * 0.15f) * 14f;
                            Projectile.NewProjectile(
                                Player.GetSource_Accessory(Player.armor.FirstOrDefault(a => a.type == ModContent.ItemType<SymphonyOfBlossoms>())),
                                Player.Center, direction,
                                ProjectileID.Seed,
                                (int)(damageDone * 0.4f),
                                2f,
                                Player.whoAmI
                            );
                        }
                    }
                }
            }
        }
    }
}
