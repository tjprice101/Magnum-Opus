using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Fate;
using MagnumOpus.Content.Fate.ResonantWeapons;

namespace MagnumOpus.Content.Common.Accessories
{
    /// <summary>
    /// Coda of Absolute Harmony - THE ULTIMATE ACCESSORY
    /// Phase 5 Ultimate: All themes, all seasons, all Fate power combined
    /// Requires sacrificing the Coda of Annihilation weapon itself
    /// The pinnacle of MagnumOpus accessory progression
    /// </summary>
    public class CodaOfAbsoluteHarmony : ModItem
    {
        // All theme colors
        private static readonly Color MoonlightPurple = new Color(138, 43, 226);
        private static readonly Color MoonlightSilver = new Color(220, 220, 235);
        private static readonly Color EroicaGold = new Color(255, 200, 80);
        private static readonly Color EroicaScarlet = new Color(200, 50, 50);
        private static readonly Color CampanellaOrange = new Color(255, 140, 40);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        private static readonly Color SwanWhite = new Color(255, 255, 255);
        private static readonly Color FateDarkPink = new Color(200, 80, 120);
        private static readonly Color FateBrightRed = new Color(255, 60, 80);
        
        // Season colors
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SummerGold = new Color(255, 180, 50);
        private static readonly Color AutumnOrange = new Color(200, 100, 30);
        private static readonly Color WinterBlue = new Color(150, 220, 255);
        
        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 42;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 10);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<CodaOfAbsoluteHarmonyPlayer>();
            modPlayer.codaEquipped = true;
            
            bool isNight = !Main.dayTime;
            
            // ============================================
            // === THE ULTIMATE ACCESSORY - ALL BONUSES ===
            // ============================================
            
            // === GLOBAL STATS ===
            player.GetDamage(DamageClass.Generic) += 0.40f;
            player.GetCritChance(DamageClass.Generic) += 30;
            player.GetAttackSpeed(DamageClass.Generic) += 0.20f;
            player.statDefense += 35;
            player.lifeRegen += 15;
            player.manaRegen += 10;
            player.endurance += 0.18f;
            player.moveSpeed += 0.30f;
            
            // === MOONLIGHT SONATA (Night power) ===
            if (isNight)
            {
                player.GetDamage(DamageClass.Generic) += 0.25f;
                player.GetCritChance(DamageClass.Generic) += 25;
                player.statDefense += 20;
            }
            
            // === EROICA (Melee mastery) ===
            player.GetDamage(DamageClass.Melee) += 0.25f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.22f;
            player.GetCritChance(DamageClass.Melee) += 15;
            
            // === LA CAMPANELLA (Magic mastery) ===
            player.GetDamage(DamageClass.Magic) += 0.30f;
            player.GetCritChance(DamageClass.Magic) += 15;
            player.manaCost -= 0.25f;
            
            // === ENIGMA (Chaos power) ===
            player.GetDamage(DamageClass.Generic) += 0.22f;
            
            // === SWAN LAKE (Grace) ===
            player.GetDamage(DamageClass.Generic) += 0.22f;
            player.moveSpeed += 0.30f;
            player.maxRunSpeed *= 1.3f;
            
            // === RANGED (Constellation Compass) ===
            player.GetDamage(DamageClass.Ranged) += 0.30f;
            player.GetCritChance(DamageClass.Ranged) += 18;
            
            // === SUMMON (Orrery) ===
            player.maxMinions += 6;
            player.GetDamage(DamageClass.Summon) += 0.30f;
            
            // === MOBILITY (Event Horizon) ===
            player.wingTimeMax += 120;
            player.noFallDmg = true;
            player.runAcceleration *= 1.5f;
            
            // === ELEMENTAL ===
            player.magmaStone = true;
            player.frostBurn = true;
            player.thorns = 2f;
            
            // === IMMUNITIES ===
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.buffImmune[BuffID.CursedInferno] = true;
            player.buffImmune[BuffID.ShadowFlame] = true;
            player.buffImmune[BuffID.Frozen] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.Confused] = true;
            player.buffImmune[BuffID.Poisoned] = true;
            player.buffImmune[BuffID.Venom] = true;
            player.buffImmune[BuffID.Slow] = true;
            player.buffImmune[BuffID.Darkness] = true;
            player.buffImmune[BuffID.Silenced] = true;
            
            // ============================================
            // === ULTIMATE AMBIENT VFX ===
            // ============================================
            if (!hideVisual)
            {
                // GRAND ORBITAL SYSTEM - 14 elements
                if (Main.GameUpdateCount % 4 == 0)
                {
                    float baseAngle = Main.GameUpdateCount * 0.008f;
                    
                    // Inner ring - 5 themes
                    Color[] themeColors = {
                        MoonlightPurple, EroicaGold, CampanellaOrange,
                        EnigmaPurple, SwanWhite
                    };
                    
                    for (int i = 0; i < 5; i++)
                    {
                        float angle = baseAngle + MathHelper.TwoPi * i / 5f;
                        float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.03f + i * 0.7f) * 4f;
                        Vector2 pos = player.Center + angle.ToRotationVector2() * (35f + pulse);
                        CustomParticles.GenericFlare(pos, themeColors[i], 0.38f, 14);
                    }
                    
                    // Middle ring - 4 seasons
                    Color[] seasonColors = { SpringPink, SummerGold, AutumnOrange, WinterBlue };
                    
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = -baseAngle * 1.2f + MathHelper.TwoPi * i / 4f;
                        float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f + i * 0.8f) * 5f;
                        Vector2 pos = player.Center + angle.ToRotationVector2() * (55f + pulse);
                        CustomParticles.GenericFlare(pos, seasonColors[i], 0.35f, 14);
                    }
                    
                    // Outer ring - 3 Fate cosmic points
                    Color[] fateColors = { FateDarkPink, FateBrightRed, Color.White };
                    
                    for (int i = 0; i < 3; i++)
                    {
                        float angle = baseAngle * 0.6f + MathHelper.TwoPi * i / 3f;
                        Vector2 pos = player.Center + angle.ToRotationVector2() * 75f;
                        CustomParticles.GenericFlare(pos, fateColors[i], 0.32f, 14);
                        
                        // Cosmic glyphs at fate points
                        if (Main.rand.NextBool(3))
                            CustomParticles.Glyph(pos, FateDarkPink * 0.5f, 0.3f, -1);
                    }
                }
                
                // Theme-specific ambient particles
                if (Main.rand.NextBool(4))
                {
                    int choice = Main.rand.Next(14);
                    Vector2 particlePos = player.Center + Main.rand.NextVector2Circular(60f, 60f);
                    
                    switch (choice)
                    {
                        case 0: // Moonlight
                            CustomParticles.GenericGlow(particlePos, new Vector2(0, -0.8f), MoonlightSilver * 0.6f, 0.25f, 22, true);
                            break;
                        case 1: // Eroica
                            ThemedParticles.SakuraPetals(particlePos, 1, 8f);
                            break;
                        case 2: // La Campanella
                            var flame = new GenericGlowParticle(particlePos, new Vector2(0, -1.5f), CampanellaOrange * 0.6f, 0.28f, 18, true);
                            MagnumParticleHandler.SpawnParticle(flame);
                            break;
                        case 3: // Enigma
                            CustomParticles.Glyph(particlePos, EnigmaPurple * 0.5f, 0.28f, -1);
                            break;
                        case 4: // Swan Lake
                            CustomParticles.SwanFeatherDrift(particlePos, Main.rand.NextBool() ? SwanWhite : Color.Black, 0.32f);
                            break;
                        case 5: // Spring
                            CustomParticles.GenericGlow(particlePos, new Vector2(0, -1f), SpringPink * 0.6f, 0.25f, 20, true);
                            break;
                        case 6: // Summer
                            CustomParticles.GenericGlow(particlePos, new Vector2(0, -1.5f), SummerGold * 0.6f, 0.25f, 18, true);
                            break;
                        case 7: // Autumn
                            CustomParticles.GenericGlow(particlePos, new Vector2(Main.rand.NextFloat(-1f, 1f), 0.5f), AutumnOrange * 0.6f, 0.25f, 22, true);
                            break;
                        case 8: // Winter
                            var sparkle = new SparkleParticle(particlePos, new Vector2(0, 0.5f), WinterBlue, 0.25f, 22);
                            MagnumParticleHandler.SpawnParticle(sparkle);
                            break;
                        case 9: // Fate Glyph
                            CustomParticles.Glyph(particlePos, FateDarkPink * 0.5f, 0.3f, -1);
                            break;
                        case 10: // Fate Star
                            CustomParticles.GenericFlare(particlePos, Color.White, 0.3f, 12);
                            break;
                        case 11: // Rainbow sparkle
                            float rainbowHue = Main.rand.NextFloat();
                            var rainbow = new SparkleParticle(particlePos, Main.rand.NextVector2Circular(1f, 1f),
                                Main.hslToRgb(rainbowHue, 1f, 0.8f), 0.28f, 18);
                            MagnumParticleHandler.SpawnParticle(rainbow);
                            break;
                        case 12: // Smoke
                            var smoke = new HeavySmokeParticle(particlePos, new Vector2(0, -0.8f),
                                Color.DarkGray, 20, 0.2f, 0.35f, 0.012f, false);
                            MagnumParticleHandler.SpawnParticle(smoke);
                            break;
                        case 13: // Music note
                            ThemedParticles.MusicNote(particlePos, new Vector2(0, -1f), 
                                Main.hslToRgb(Main.rand.NextFloat(), 0.9f, 0.75f), 0.35f, 22);
                            break;
                    }
                }
                
                // Grand cosmic light cycling through all elements
                float hue = (Main.GameUpdateCount * 0.003f) % 1f;
                Color lightColor = Main.hslToRgb(hue, 0.9f, 0.7f);
                float lightPulse = (float)Math.Sin(Main.GameUpdateCount * 0.02f) * 0.2f + 0.8f;
                Lighting.AddLight(player.Center, lightColor.ToVector3() * lightPulse * 0.8f);
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<OpusOfFourMovements>()
                .AddIngredient<CosmicWardensRegalia>()
                .AddIngredient<SpringsMoonlitGarden>()
                .AddIngredient<SummersInfernalPeak>()
                .AddIngredient<WintersEnigmaticSilence>()
                .AddIngredient<CodaOfAnnihilation>() // CONSUMED
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class CodaOfAbsoluteHarmonyPlayer : ModPlayer
    {
        public bool codaEquipped;
        private int heroicSurgeTimer;
        private int invulnFramesOnKill = 120;
        private int dodgeCooldown;
        private int bellRingCooldown;
        private int temporalEchoCooldown;
        private int cosmicBurstCooldown;
        private int meleeStrikeCount;
        private Dictionary<int, int> paradoxStacks = new Dictionary<int, int>();
        private Dictionary<int, int> paradoxTimers = new Dictionary<int, int>();
        
        // All debuffs from all sources
        private static readonly int[] AllDebuffs = new int[]
        {
            BuffID.Confused, BuffID.Slow, BuffID.CursedInferno,
            BuffID.Ichor, BuffID.ShadowFlame, BuffID.Frostburn,
            BuffID.OnFire, BuffID.Poisoned, BuffID.Venom
        };

        public override void ResetEffects()
        {
            codaEquipped = false;
        }

        public override void PostUpdate()
        {
            if (heroicSurgeTimer > 0)
            {
                heroicSurgeTimer--;
                Player.GetDamage(DamageClass.Generic) += 0.40f;
            }
            
            if (dodgeCooldown > 0) dodgeCooldown--;
            if (bellRingCooldown > 0) bellRingCooldown--;
            if (temporalEchoCooldown > 0) temporalEchoCooldown--;
            if (cosmicBurstCooldown > 0) cosmicBurstCooldown--;
            
            List<int> toRemove = new List<int>();
            foreach (var kvp in paradoxTimers)
            {
                paradoxTimers[kvp.Key]--;
                if (paradoxTimers[kvp.Key] <= 0)
                    toRemove.Add(kvp.Key);
            }
            foreach (int key in toRemove)
            {
                paradoxTimers.Remove(key);
                paradoxStacks.Remove(key);
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            HandleCodaHit(target, damageDone, true, DamageClass.Magic.CountsAsClass(item.DamageType));
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.owner == Player.whoAmI)
            {
                bool isMelee = DamageClass.Melee.CountsAsClass(proj.DamageType);
                bool isMagic = DamageClass.Magic.CountsAsClass(proj.DamageType);
                HandleCodaHit(target, damageDone, isMelee, isMagic);
            }
        }

        private void HandleCodaHit(NPC target, int damageDone, bool isMelee, bool isMagic)
        {
            if (!codaEquipped) return;
            
            bool isNight = !Main.dayTime;
            
            // === MOONLIGHT: Blue fire at night ===
            if (isNight && isMagic)
            {
                int bonusDamage = (int)(damageDone * 0.25f);
                target.SimpleStrikeNPC(bonusDamage, 0, false, 0, null, false, 0, true);
            }
            
            // === EROICA: Temporal Echo every 5th melee hit ===
            if (isMelee)
            {
                meleeStrikeCount++;
                if (meleeStrikeCount >= 5)
                {
                    meleeStrikeCount = 0;
                    int echoDamage = (int)(damageDone * 1.0f);
                    target.SimpleStrikeNPC(echoDamage, 0, false, 0, null, false, 0, true);
                    
                    CustomParticles.GenericFlare(target.Center, new Color(200, 80, 120), 0.85f, 22);
                    CustomParticles.HaloRing(target.Center, new Color(140, 50, 160), 0.55f, 18);
                    CustomParticles.GlyphBurst(target.Center, new Color(140, 50, 160), 6, 4f);
                }
            }
            
            // === LA CAMPANELLA: Bell ring AOE (20%) ===
            if (bellRingCooldown <= 0 && Main.rand.NextFloat() < 0.20f)
            {
                bellRingCooldown = 15;
                target.AddBuff(BuffID.Confused, 240);
                
                float aoeRadius = 220f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.whoAmI != target.whoAmI && !npc.immortal)
                    {
                        if (Vector2.Distance(npc.Center, target.Center) <= aoeRadius)
                        {
                            int aoeDamage = (int)(damageDone * 0.75f);
                            npc.SimpleStrikeNPC(aoeDamage, 0, false, 0, null, false, 0, true);
                            npc.AddBuff(BuffID.OnFire, 360);
                            npc.AddBuff(BuffID.Frostburn, 300);
                        }
                    }
                }
                
                CustomParticles.GenericFlare(target.Center, Color.White, 1.1f, 22);
                CustomParticles.GenericFlare(target.Center, new Color(255, 140, 40), 0.9f, 20);
                for (int i = 0; i < 6; i++)
                    CustomParticles.HaloRing(target.Center, Color.Lerp(new Color(255, 140, 40), new Color(255, 200, 80), i / 6f), 0.4f + i * 0.12f, 16 + i * 2);
            }
            
            // === ENIGMA: Paradox stacking (25%) ===
            if (Main.rand.NextFloat() < 0.25f)
            {
                int debuffId = AllDebuffs[Main.rand.Next(AllDebuffs.Length)];
                target.AddBuff(debuffId, 480);
                target.AddBuff(BuffID.OnFire, 420);
                target.AddBuff(BuffID.Frostburn, 360);
                
                if (!paradoxStacks.ContainsKey(target.whoAmI))
                    paradoxStacks[target.whoAmI] = 0;
                
                paradoxStacks[target.whoAmI]++;
                paradoxTimers[target.whoAmI] = 540;
                
                // ABSOLUTE HARMONY COLLAPSE at 5 stacks
                if (paradoxStacks[target.whoAmI] >= 5)
                {
                    TriggerAbsoluteHarmonyCollapse(target, damageDone, isNight);
                    paradoxStacks[target.whoAmI] = 0;
                }
            }
            
            // === SWAN LAKE: Rainbow sparkle ===
            if (Main.rand.NextBool(4))
            {
                float hue = Main.rand.NextFloat();
                var sparkle = new SparkleParticle(target.Center, Main.rand.NextVector2Circular(3f, 3f),
                    Main.hslToRgb(hue, 1f, 0.8f), 0.35f, 16);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // === SEASONS: All elemental effects ===
            target.AddBuff(BuffID.OnFire, 300);
            target.AddBuff(BuffID.Frostburn, 240);
            target.AddBuff(BuffID.Poisoned, 300);
            
            // === LIFESTEAL (12%) ===
            if (Main.rand.NextFloat() < 0.12f)
            {
                int healAmount = Math.Max(1, Math.Min((int)(damageDone * 0.10f), 30));
                Player.Heal(healAmount);
            }
            
            // === COSMIC MANA BURST ===
            if (Player.statMana < Player.statManaMax2 * 0.3f && cosmicBurstCooldown <= 0)
            {
                cosmicBurstCooldown = 240;
                Player.statMana = Math.Min(Player.statMana + 150, Player.statManaMax2);
                
                CustomParticles.GenericFlare(Player.Center, new Color(140, 50, 160), 1.0f, 24);
                for (int i = 0; i < 8; i++)
                    CustomParticles.HaloRing(Player.Center, Color.Lerp(new Color(200, 80, 120), new Color(140, 50, 160), i / 8f), 0.35f + i * 0.1f, 16 + i * 2);
            }
            
            // Check kill for heroic surge
            if (target.life <= 0 && !target.immortal)
            {
                Player.immune = true;
                Player.immuneTime = Math.Max(Player.immuneTime, invulnFramesOnKill);
                heroicSurgeTimer = 480;
                
                // Kill explosion
                CustomParticles.GenericFlare(target.Center, Color.White, 1.2f, 28);
                CustomParticles.ExplosionBurst(target.Center, Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.75f), 18, 10f);
            }
        }

        private void TriggerAbsoluteHarmonyCollapse(NPC target, int baseDamage, bool isNight)
        {
            // ================================================
            // === ABSOLUTE HARMONY COLLAPSE - ULTIMATE VFX ===
            // ================================================
            
            // Central flash cascade - ALL colors
            CustomParticles.GenericFlare(target.Center, Color.White, 3.0f, 50);
            
            Color[] allColors = {
                new Color(255, 183, 197), // Spring
                new Color(255, 180, 50),  // Summer
                new Color(200, 100, 30),  // Autumn
                new Color(150, 220, 255), // Winter
                new Color(138, 43, 226),  // Moonlight
                new Color(255, 200, 80),  // Eroica Gold
                new Color(200, 50, 50),   // Eroica Scarlet
                new Color(255, 140, 40),  // La Campanella
                new Color(140, 60, 200),  // Enigma Purple
                new Color(50, 220, 100),  // Enigma Green
                Color.White,              // Swan Lake White
                new Color(200, 80, 120),  // Fate Pink
                new Color(255, 60, 80),   // Fate Red
            };
            
            // Cascading flares
            for (int i = 0; i < allColors.Length; i++)
            {
                CustomParticles.GenericFlare(target.Center, allColors[i], 2.2f - i * 0.12f, 45 - i * 2);
            }
            
            // Mega halo cascade - 24 rings
            for (int ring = 0; ring < 24; ring++)
            {
                CustomParticles.HaloRing(target.Center, allColors[ring % allColors.Length], 0.5f + ring * 0.15f, 25 + ring * 2);
            }
            
            // Themed particle bursts
            ThemedParticles.SakuraPetals(target.Center, 25, 90f);
            
            for (int i = 0; i < 32; i++)
            {
                float angle = MathHelper.TwoPi * i / 32f;
                Vector2 featherPos = target.Center + angle.ToRotationVector2() * 70f;
                CustomParticles.SwanFeatherDrift(featherPos, i % 2 == 0 ? Color.White : new Color(220, 220, 235), 0.7f);
            }
            
            for (int i = 0; i < 40; i++)
            {
                float angle = MathHelper.TwoPi * i / 40f;
                float radius = 50f + i * 7f;
                Vector2 pos = target.Center + angle.ToRotationVector2() * radius;
                CustomParticles.Glyph(pos, allColors[i % allColors.Length], 0.7f, -1);
            }
            
            // Rainbow sparkle explosion
            for (int i = 0; i < 30; i++)
            {
                float hue = (float)i / 30f;
                Vector2 sparklePos = target.Center + Main.rand.NextVector2Circular(80f, 80f);
                var sparkle = new SparkleParticle(sparklePos, Main.rand.NextVector2Circular(5f, 5f),
                    Main.hslToRgb(hue, 1f, 0.85f), 0.5f, 25);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Multi-color explosion bursts
            for (int i = 0; i < allColors.Length; i++)
            {
                CustomParticles.ExplosionBurst(target.Center, allColors[i], 18, 14f - i * 0.3f);
            }
            
            // Music notes spiraling outward
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f + Main.GameUpdateCount * 0.05f;
                Vector2 notePos = target.Center + angle.ToRotationVector2() * 50f;
                ThemedParticles.MusicNote(notePos, angle.ToRotationVector2() * 3f, allColors[i % allColors.Length], 0.5f, 30);
            }
            
            // ULTIMATE DAMAGE
            if (Main.myPlayer == Player.whoAmI)
            {
                int harmonyDamage = (int)(baseDamage * 7.0f); // 700% damage!
                target.SimpleStrikeNPC(harmonyDamage, 0, false, 0, null, false, 0, true);
                
                float aoeRadius = 450f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.whoAmI != target.whoAmI && !npc.immortal)
                    {
                        if (Vector2.Distance(npc.Center, target.Center) <= aoeRadius)
                        {
                            npc.SimpleStrikeNPC(harmonyDamage / 2, 0, false, 0, null, false, 0, true);
                            
                            // Apply ALL debuffs
                            foreach (int debuff in AllDebuffs)
                                npc.AddBuff(debuff, 600);
                        }
                    }
                }
            }
            
            MagnumScreenEffects.AddScreenShake(25f);
        }

        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (!codaEquipped) return false;
            if (dodgeCooldown > 0) return false;
            
            bool isNight = !Main.dayTime;
            float dodgeChance = isNight ? 0.22f : 0.18f;
            
            if (Main.rand.NextFloat() < dodgeChance)
            {
                dodgeCooldown = 45;
                
                // ULTIMATE DODGE VFX
                CustomParticles.GenericFlare(Player.Center, Color.White, 2.2f, 38);
                
                Color[] themeColors = {
                    new Color(138, 43, 226), new Color(255, 200, 80),
                    new Color(255, 140, 40), new Color(140, 60, 200), Color.White
                };
                
                for (int i = 0; i < 5; i++)
                    CustomParticles.GenericFlare(Player.Center, themeColors[i], 1.5f - i * 0.2f, 32 - i * 3);
                
                for (int i = 0; i < 18; i++)
                {
                    float hue = (float)i / 18f;
                    CustomParticles.HaloRing(Player.Center, Main.hslToRgb(hue, 1f, 0.75f), 0.4f + i * 0.1f, 16 + i * 2);
                }
                
                // Dodge damage
                if (Main.myPlayer == Player.whoAmI)
                {
                    int dodgeDamage = 400 + (int)(Player.GetTotalDamage(DamageClass.Generic).ApplyTo(100) * 0.6f);
                    float damageRadius = 350f;
                    
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (npc.active && !npc.friendly && !npc.immortal && !npc.dontTakeDamage)
                        {
                            if (Vector2.Distance(npc.Center, Player.Center) <= damageRadius)
                            {
                                npc.SimpleStrikeNPC(dodgeDamage, 0, false, 0, null, false, 0, true);
                                npc.AddBuff(BuffID.OnFire, 360);
                                npc.AddBuff(BuffID.Frostburn, 300);
                            }
                        }
                    }
                }
                
                Player.immune = true;
                Player.immuneTime = 50;
                
                return true;
            }
            
            return false;
        }
    }
}
