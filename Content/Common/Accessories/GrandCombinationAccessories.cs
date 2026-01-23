using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.MoonlightSonata.Accessories;
using MagnumOpus.Content.Eroica.Accessories;
using MagnumOpus.Content.LaCampanella.Accessories;
using MagnumOpus.Content.EnigmaVariations.Accessories;
using MagnumOpus.Content.SwanLake.Accessories;
using MagnumOpus.Content.Fate.Accessories;
using MagnumOpus.Content.Fate.HarmonicCores;
using MagnumOpus.Content.Fate.ResonanceEnergies;
using MagnumOpus.Content.Fate;
using MagnumOpus.Content.Seasons.Accessories;
using MagnumOpus.Content.Spring.Materials;
using MagnumOpus.Content.Summer.Materials;
using MagnumOpus.Content.Autumn.Materials;
using MagnumOpus.Content.Winter.Materials;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.LaCampanella.ResonanceEnergies;
using MagnumOpus.Content.EnigmaVariations.ResonanceEnergies;
using MagnumOpus.Content.SwanLake.ResonanceEnergies;

namespace MagnumOpus.Content.Common.Accessories
{
    #region Opus of Four Movements - Seasons + Themes Combined
    /// <summary>
    /// Phase 5 Grand Combination: Complete Harmony + Vivaldi's Masterwork + All Resonant Energies
    /// ALL seasons AND all themes combined - ultimate pre-Fate musical achievement
    /// </summary>
    public class OpusOfFourMovements : ModItem
    {
        // Season colors
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SummerGold = new Color(255, 180, 50);
        private static readonly Color AutumnOrange = new Color(200, 100, 30);
        private static readonly Color WinterBlue = new Color(150, 220, 255);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 3);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<OpusOfFourMovementsPlayer>();
            modPlayer.opusEquipped = true;
            
            bool isNight = !Main.dayTime;
            
            // === SEASONAL BONUSES (from Vivaldi's Masterwork) ===
            player.GetDamage(DamageClass.Generic) += 0.20f;
            player.GetCritChance(DamageClass.Generic) += 15;
            player.GetAttackSpeed(DamageClass.Generic) += 0.12f;
            player.statDefense += 20;
            player.lifeRegen += 8;
            player.manaRegen += 4;
            player.endurance += 0.12f;
            player.moveSpeed += 0.15f;
            player.magmaStone = true;
            player.frostBurn = true;
            player.thorns = 1.5f;
            
            // === THEME BONUSES (from Complete Harmony) ===
            // Moonlight
            if (isNight)
            {
                player.GetDamage(DamageClass.Generic) += 0.20f;
                player.GetCritChance(DamageClass.Generic) += 22;
                player.statDefense += 15;
            }
            
            // Eroica
            player.GetDamage(DamageClass.Melee) += 0.22f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.18f;
            player.GetCritChance(DamageClass.Melee) += 12;
            
            // La Campanella
            player.GetDamage(DamageClass.Magic) += 0.25f;
            player.GetCritChance(DamageClass.Magic) += 12;
            player.manaCost -= 0.15f;
            
            // Enigma
            player.GetDamage(DamageClass.Generic) += 0.20f;
            player.GetCritChance(DamageClass.Generic) += 10;
            
            // Swan Lake
            player.GetDamage(DamageClass.Generic) += 0.18f;
            player.GetCritChance(DamageClass.Generic) += 10;
            player.moveSpeed += 0.25f;
            
            // Immunities
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.buffImmune[BuffID.Frozen] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.Poisoned] = true;
            
            // Ambient VFX - 9 elements unified
            if (!hideVisual)
            {
                // Orbiting seasonal + theme particles
                if (Main.GameUpdateCount % 6 == 0)
                {
                    float baseAngle = Main.GameUpdateCount * 0.012f;
                    
                    // Inner ring - 4 seasons
                    Color[] seasonColors = { SpringPink, SummerGold, AutumnOrange, WinterBlue };
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = baseAngle + MathHelper.TwoPi * i / 4f;
                        Vector2 pos = player.Center + angle.ToRotationVector2() * 45f;
                        CustomParticles.GenericFlare(pos, seasonColors[i], 0.3f, 14);
                    }
                    
                    // Outer ring - 5 themes
                    Color[] themeColors = {
                        MoonlightColors.Purple,
                        EroicaColors.Gold,
                        CampanellaColors.Orange,
                        EnigmaColors.GreenFlame,
                        SwanColors.GetRainbow(Main.rand.NextFloat())
                    };
                    for (int i = 0; i < 5; i++)
                    {
                        float angle = -baseAngle * 0.8f + MathHelper.TwoPi * i / 5f;
                        Vector2 pos = player.Center + angle.ToRotationVector2() * 65f;
                        CustomParticles.GenericFlare(pos, themeColors[i], 0.28f, 14);
                    }
                }
                
                // Theme-specific particles
                if (Main.rand.NextBool(6))
                {
                    int choice = Main.rand.Next(9);
                    Vector2 particlePos = player.Center + Main.rand.NextVector2Circular(50f, 50f);
                    
                    switch (choice)
                    {
                        case 0: // Spring
                            CustomParticles.GenericGlow(particlePos, new Vector2(0, -1f), SpringPink * 0.7f, 0.25f, 20, true);
                            break;
                        case 1: // Summer
                            CustomParticles.GenericGlow(particlePos, new Vector2(0, -1.5f), SummerGold * 0.7f, 0.25f, 18, true);
                            break;
                        case 2: // Autumn
                            CustomParticles.GenericGlow(particlePos, new Vector2(Main.rand.NextFloat(-1f, 1f), 0.5f), AutumnOrange * 0.7f, 0.25f, 22, true);
                            break;
                        case 3: // Winter
                            CustomParticles.GenericGlow(particlePos, Main.rand.NextVector2Circular(1f, 1f), WinterBlue * 0.7f, 0.25f, 24, true);
                            break;
                        case 4: // Moonlight
                            CustomParticles.GenericFlare(particlePos, MoonlightColors.Silver, 0.25f, 14);
                            break;
                        case 5: // Eroica
                            if (Main.rand.NextBool())
                                ThemedParticles.SakuraPetals(particlePos, 1, 8f);
                            break;
                        case 6: // Campanella
                            var flame = new GenericGlowParticle(particlePos, new Vector2(0, -1.5f), CampanellaColors.Orange * 0.6f, 0.28f, 18, true);
                            MagnumParticleHandler.SpawnParticle(flame);
                            break;
                        case 7: // Enigma
                            CustomParticles.Glyph(particlePos, EnigmaColors.DeepPurple * 0.5f, 0.28f, -1);
                            break;
                        case 8: // Swan Lake
                            CustomParticles.SwanFeatherDrift(particlePos, Main.rand.NextBool() ? SwanColors.White : SwanColors.Black, 0.32f);
                            break;
                    }
                }
                
                // Grand light cycling through all colors
                float hue = (Main.GameUpdateCount * 0.004f) % 1f;
                Color lightColor = Main.hslToRgb(hue, 0.85f, 0.65f);
                Lighting.AddLight(player.Center, lightColor.ToVector3() * 0.7f);
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<CompleteHarmony>()
                .AddIngredient<VivaldisMasterwork>()
                .AddIngredient<MoonlightsResonantEnergy>()
                .AddIngredient<EroicasResonantEnergy>()
                .AddIngredient<LaCampanellaResonantEnergy>()
                .AddIngredient<EnigmaResonantEnergy>()
                .AddIngredient<SwansResonanceEnergy>()
                .AddIngredient<DormantSpringCore>()
                .AddIngredient<DormantSummerCore>()
                .AddIngredient<DormantAutumnCore>()
                .AddIngredient<DormantWinterCore>()
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class OpusOfFourMovementsPlayer : ModPlayer
    {
        public bool opusEquipped;
        private int heroicSurgeTimer;
        private int invulnFramesOnKill = 100;
        private int dodgeCooldown;
        private int bellRingCooldown;
        private Dictionary<int, int> paradoxStacks = new Dictionary<int, int>();
        private Dictionary<int, int> paradoxTimers = new Dictionary<int, int>();
        
        private static readonly int[] ParadoxDebuffs = new int[]
        {
            BuffID.Confused, BuffID.Slow, BuffID.CursedInferno,
            BuffID.Ichor, BuffID.ShadowFlame, BuffID.Frostburn
        };

        public override void ResetEffects()
        {
            opusEquipped = false;
        }

        public override void PostUpdate()
        {
            if (heroicSurgeTimer > 0)
            {
                heroicSurgeTimer--;
                Player.GetDamage(DamageClass.Generic) += 0.35f;
            }
            
            if (dodgeCooldown > 0) dodgeCooldown--;
            if (bellRingCooldown > 0) bellRingCooldown--;
            
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
            HandleOpusHit(target, damageDone, false);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.owner == Player.whoAmI)
            {
                HandleOpusHit(target, damageDone, DamageClass.Magic.CountsAsClass(proj.DamageType));
            }
        }

        private void HandleOpusHit(NPC target, int damageDone, bool isMagic)
        {
            if (!opusEquipped) return;
            
            bool isNight = !Main.dayTime;
            
            // Blue fire at night
            if (isNight && isMagic)
            {
                int bonusDamage = (int)(damageDone * 0.22f);
                target.SimpleStrikeNPC(bonusDamage, 0, false, 0, null, false, 0, true);
            }
            
            // Paradox (20%)
            if (Main.rand.NextFloat() < 0.20f)
            {
                int debuffId = ParadoxDebuffs[Main.rand.Next(ParadoxDebuffs.Length)];
                target.AddBuff(debuffId, 420);
                target.AddBuff(BuffID.OnFire, 360);
                target.AddBuff(BuffID.Frostburn, 300);
                
                if (!paradoxStacks.ContainsKey(target.whoAmI))
                    paradoxStacks[target.whoAmI] = 0;
                
                paradoxStacks[target.whoAmI]++;
                paradoxTimers[target.whoAmI] = 480;
                
                if (paradoxStacks[target.whoAmI] >= 5)
                {
                    TriggerOpusCollapse(target, damageDone, isNight);
                    paradoxStacks[target.whoAmI] = 0;
                }
            }
            
            // Bell ring (16%)
            if (bellRingCooldown <= 0 && Main.rand.NextFloat() < 0.16f)
            {
                bellRingCooldown = 18;
                target.AddBuff(BuffID.Confused, 180);
                
                float aoeRadius = 180f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.whoAmI != target.whoAmI && !npc.immortal)
                    {
                        if (Vector2.Distance(npc.Center, target.Center) <= aoeRadius)
                        {
                            int aoeDamage = (int)(damageDone * 0.65f);
                            npc.SimpleStrikeNPC(aoeDamage, 0, false, 0, null, false, 0, true);
                            npc.AddBuff(BuffID.OnFire, 300);
                            npc.AddBuff(BuffID.Frostburn, 240);
                        }
                    }
                }
            }
            
            // Lifesteal (8%)
            if (Main.rand.NextFloat() < 0.08f)
            {
                int healAmount = Math.Max(1, Math.Min((int)(damageDone * 0.08f), 20));
                Player.Heal(healAmount);
            }
            
            // Check kill
            if (target.life <= 0 && !target.immortal)
            {
                Player.immune = true;
                Player.immuneTime = Math.Max(Player.immuneTime, invulnFramesOnKill);
                heroicSurgeTimer = 420;
            }
        }

        private void TriggerOpusCollapse(NPC target, int baseDamage, bool isNight)
        {
            // GRAND OPUS EXPLOSION
            CustomParticles.GenericFlare(target.Center, Color.White, 2.5f, 45);
            
            Color[] allColors = {
                new Color(255, 183, 197), // Spring
                new Color(255, 180, 50),  // Summer
                new Color(200, 100, 30),  // Autumn
                new Color(150, 220, 255), // Winter
                MoonlightColors.Purple,
                EroicaColors.Gold,
                CampanellaColors.Orange,
                EnigmaColors.GreenFlame,
                SwanColors.GetRainbow(0f)
            };
            
            for (int i = 0; i < 9; i++)
            {
                CustomParticles.GenericFlare(target.Center, allColors[i], 1.8f - i * 0.15f, 38 - i * 2);
            }
            
            for (int ring = 0; ring < 18; ring++)
            {
                CustomParticles.HaloRing(target.Center, allColors[ring % 9], 0.4f + ring * 0.12f, 22 + ring * 2);
            }
            
            ThemedParticles.SakuraPetals(target.Center, 18, 70f);
            
            for (int i = 0; i < 24; i++)
            {
                float angle = MathHelper.TwoPi * i / 24f;
                Vector2 featherPos = target.Center + angle.ToRotationVector2() * 55f;
                CustomParticles.SwanFeatherDrift(featherPos, i % 2 == 0 ? SwanColors.White : MoonlightColors.Silver, 0.6f);
            }
            
            for (int i = 0; i < 30; i++)
            {
                float angle = MathHelper.TwoPi * i / 30f;
                float radius = 45f + i * 6f;
                Vector2 pos = target.Center + angle.ToRotationVector2() * radius;
                CustomParticles.Glyph(pos, EnigmaColors.Purple, 0.6f, -1);
            }
            
            for (int i = 0; i < 9; i++)
            {
                CustomParticles.ExplosionBurst(target.Center, allColors[i], 15, 12f - i * 0.5f);
            }
            
            if (Main.myPlayer == Player.whoAmI)
            {
                int opusDamage = (int)(baseDamage * 5.0f);
                target.SimpleStrikeNPC(opusDamage, 0, false, 0, null, false, 0, true);
                
                float aoeRadius = 350f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.whoAmI != target.whoAmI && !npc.immortal)
                    {
                        if (Vector2.Distance(npc.Center, target.Center) <= aoeRadius)
                        {
                            npc.SimpleStrikeNPC(opusDamage / 2, 0, false, 0, null, false, 0, true);
                            npc.AddBuff(BuffID.OnFire, 480);
                            npc.AddBuff(BuffID.Frostburn, 420);
                            npc.AddBuff(ParadoxDebuffs[Main.rand.Next(ParadoxDebuffs.Length)], 360);
                        }
                    }
                }
            }
            
            MagnumScreenEffects.AddScreenShake(20f);
        }

        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (!opusEquipped) return false;
            if (dodgeCooldown > 0) return false;
            
            bool isNight = !Main.dayTime;
            float dodgeChance = isNight ? 0.18f : 0.14f;
            
            if (Main.rand.NextFloat() < dodgeChance)
            {
                dodgeCooldown = 60;
                
                CustomParticles.GenericFlare(Player.Center, Color.White, 1.8f, 32);
                
                Color[] colors = {
                    MoonlightColors.Purple, EroicaColors.Gold,
                    CampanellaColors.Orange, EnigmaColors.GreenFlame, SwanColors.GetRainbow(0f)
                };
                
                for (int i = 0; i < 5; i++)
                    CustomParticles.GenericFlare(Player.Center, colors[i], 1.2f - i * 0.15f, 28 - i * 2);
                
                for (int i = 0; i < 12; i++)
                    CustomParticles.HaloRing(Player.Center, colors[i % 5], 0.35f + i * 0.08f, 14 + i * 2);
                
                // Dodge damage
                if (Main.myPlayer == Player.whoAmI)
                {
                    int dodgeDamage = 200 + (int)(Player.GetTotalDamage(DamageClass.Generic).ApplyTo(100) * 0.4f);
                    float damageRadius = 250f;
                    
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (npc.active && !npc.friendly && !npc.immortal && !npc.dontTakeDamage)
                        {
                            if (Vector2.Distance(npc.Center, Player.Center) <= damageRadius)
                                npc.SimpleStrikeNPC(dodgeDamage, 0, false, 0, null, false, 0, true);
                        }
                    }
                }
                
                Player.immune = true;
                Player.immuneTime = 40;
                
                return true;
            }
            
            return false;
        }
    }
    #endregion

    #region Cosmic Warden's Regalia - All 5 Fate Accessories Combined
    /// <summary>
    /// Phase 5 Grand Combination: All 5 Fate Vanilla Upgrade Accessories
    /// Ultimate cosmic authority - ALL Fate accessory bonuses combined
    /// </summary>
    public class CosmicWardensRegalia : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 4);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<CosmicWardensRegaliaPlayer>();
            modPlayer.regaliaEquipped = true;
            
            // === PARADOX CHRONOMETER (Melee) ===
            player.GetDamage(DamageClass.Melee) += 0.20f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.22f;
            player.GetCritChance(DamageClass.Melee) += 12;
            
            // === CONSTELLATION COMPASS (Ranged) ===
            player.GetDamage(DamageClass.Ranged) += 0.28f;
            player.GetCritChance(DamageClass.Ranged) += 15;
            
            // === ASTRAL CONDUIT (Magic) ===
            player.GetDamage(DamageClass.Magic) += 0.28f;
            player.GetCritChance(DamageClass.Magic) += 12;
            player.manaCost -= 0.22f;
            player.manaRegen += 5;
            
            // === MACHINATION OF THE EVENT HORIZON (Mobility) ===
            player.moveSpeed += 0.30f;
            player.runAcceleration *= 1.35f;
            player.maxRunSpeed *= 1.25f;
            player.wingTimeMax += 60;
            player.noFallDmg = true;
            
            // === ORRERY OF INFINITE ORBITS (Summon) ===
            player.maxMinions += 4;
            player.GetDamage(DamageClass.Summon) += 0.25f;
            
            // Cosmic immunities
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.CursedInferno] = true;
            player.buffImmune[BuffID.ShadowFlame] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Confused] = true;
            
            // Ambient VFX - All 5 Fate accessories represented
            if (!hideVisual)
            {
                // Cosmic orbit system
                if (Main.GameUpdateCount % 8 == 0)
                {
                    float baseAngle = Main.GameUpdateCount * 0.015f;
                    
                    // 5 cosmic elements orbit
                    for (int i = 0; i < 5; i++)
                    {
                        float angle = baseAngle + MathHelper.TwoPi * i / 5f;
                        Vector2 pos = player.Center + angle.ToRotationVector2() * 55f;
                        
                        Color orbitColor;
                        switch (i)
                        {
                            case 0: orbitColor = FateCosmicVFX.FateDarkPink; break; // Chronometer
                            case 1: orbitColor = FateCosmicVFX.FateBrightRed; break; // Compass
                            case 2: orbitColor = FateCosmicVFX.FatePurple; break; // Conduit
                            case 3: orbitColor = FateCosmicVFX.FateWhite; break; // Event Horizon
                            default: orbitColor = Color.Lerp(FateCosmicVFX.FateDarkPink, FateCosmicVFX.FateBrightRed, 0.5f); break; // Orrery
                        }
                        
                        CustomParticles.GenericFlare(pos, orbitColor, 0.35f, 14);
                    }
                }
                
                // Clock hands from Chronometer
                float handAngle = Main.GameUpdateCount * 0.05f;
                if (Main.rand.NextBool(12))
                {
                    Vector2 handPos = player.Center + handAngle.ToRotationVector2() * 25f;
                    CustomParticles.GenericFlare(handPos, FateCosmicVFX.FateDarkPink, 0.25f, 10);
                }
                
                // Star particles from Compass
                if (Main.rand.NextBool(10))
                {
                    Vector2 starPos = player.Center + Main.rand.NextVector2Circular(45f, 45f);
                    CustomParticles.GenericFlare(starPos, FateCosmicVFX.FateWhite, 0.3f, 12);
                }
                
                // Cosmic glyphs
                if (Main.rand.NextBool(18))
                {
                    Vector2 glyphPos = player.Center + Main.rand.NextVector2Circular(50f, 50f);
                    CustomParticles.Glyph(glyphPos, FateCosmicVFX.FatePurple * 0.6f, 0.32f, -1);
                }
                
                // Cosmic light
                float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.03f) * 0.2f + 0.8f;
                Lighting.AddLight(player.Center, FateCosmicVFX.FateDarkPink.ToVector3() * pulse * 0.6f);
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ParadoxChronometer>()
                .AddIngredient<ConstellationCompass>()
                .AddIngredient<AstralConduit>()
                .AddIngredient<MachinationoftheEventHorizon>()
                .AddIngredient<OrreryofInfiniteOrbits>()
                .AddIngredient<HarmonicCoreOfFate>(50)
                .AddIngredient<MoonlightsResonantEnergy>()
                .AddIngredient<EroicasResonantEnergy>()
                .AddIngredient<LaCampanellaResonantEnergy>()
                .AddIngredient<EnigmaResonantEnergy>()
                .AddIngredient<SwansResonanceEnergy>()
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class CosmicWardensRegaliaPlayer : ModPlayer
    {
        public bool regaliaEquipped;
        private int meleeStrikeCount;
        private int dashCooldown;
        private int cosmicBurstCooldown;

        public override void ResetEffects()
        {
            regaliaEquipped = false;
        }

        public override void PostUpdate()
        {
            if (dashCooldown > 0) dashCooldown--;
            if (cosmicBurstCooldown > 0) cosmicBurstCooldown--;
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            HandleRegaliaHit(target, damageDone, true);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.owner == Player.whoAmI)
            {
                bool isMelee = DamageClass.Melee.CountsAsClass(proj.DamageType);
                HandleRegaliaHit(target, damageDone, isMelee);
            }
        }

        private void HandleRegaliaHit(NPC target, int damageDone, bool isMelee)
        {
            if (!regaliaEquipped) return;
            
            // Temporal Echo (melee, every 6th hit)
            if (isMelee)
            {
                meleeStrikeCount++;
                if (meleeStrikeCount >= 6)
                {
                    meleeStrikeCount = 0;
                    int echoDamage = (int)(damageDone * 0.8f);
                    target.SimpleStrikeNPC(echoDamage, 0, false, 0, null, false, 0, true);
                    
                    CustomParticles.GenericFlare(target.Center, FateCosmicVFX.FateDarkPink, 0.8f, 20);
                    CustomParticles.HaloRing(target.Center, FateCosmicVFX.FatePurple, 0.5f, 16);
                    CustomParticles.GlyphBurst(target.Center, FateCosmicVFX.FatePurple, 5, 4f);
                }
            }
            
            // Constellation Mark (all hits)
            if (Main.rand.NextFloat() < 0.15f)
            {
                // Mark nearby enemies for bonus damage
                float markRadius = 300f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && !npc.immortal)
                    {
                        if (Vector2.Distance(npc.Center, target.Center) <= markRadius)
                        {
                            npc.AddBuff(BuffID.Ichor, 300); // Reduced defense
                            CustomParticles.GenericFlare(npc.Center, FateCosmicVFX.FateWhite, 0.4f, 12);
                        }
                    }
                }
            }
            
            // Cosmic Mana Burst (magic, when mana low)
            if (Player.statMana < Player.statManaMax2 * 0.3f && cosmicBurstCooldown <= 0)
            {
                cosmicBurstCooldown = 300;
                Player.statMana = Math.Min(Player.statMana + 100, Player.statManaMax2);
                
                CustomParticles.GenericFlare(Player.Center, FateCosmicVFX.FatePurple, 0.9f, 22);
                for (int i = 0; i < 6; i++)
                {
                    CustomParticles.HaloRing(Player.Center, Color.Lerp(FateCosmicVFX.FateDarkPink, FateCosmicVFX.FatePurple, i / 6f), 
                        0.3f + i * 0.1f, 14 + i * 2);
                }
            }
        }

        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (!regaliaEquipped) return false;
            if (dashCooldown > 0) return false;
            
            // Event Horizon dodge (12% chance)
            if (Main.rand.NextFloat() < 0.12f)
            {
                dashCooldown = 180;
                
                // Brief invulnerability dash
                Player.immune = true;
                Player.immuneTime = 45;
                
                // Cosmic dash VFX
                CustomParticles.GenericFlare(Player.Center, Color.White, 1.5f, 28);
                CustomParticles.GenericFlare(Player.Center, FateCosmicVFX.FateDarkPink, 1.2f, 25);
                
                for (int i = 0; i < 8; i++)
                {
                    CustomParticles.HaloRing(Player.Center, Color.Lerp(FateCosmicVFX.FateDarkPink, FateCosmicVFX.FateBrightRed, i / 8f),
                        0.4f + i * 0.1f, 16 + i * 2);
                }
                
                CustomParticles.GlyphBurst(Player.Center, FateCosmicVFX.FatePurple, 8, 5f);
                
                return true;
            }
            
            return false;
        }
    }
    #endregion

    #region Seasonal Destiny - Seasons + Fate Time
    /// <summary>
    /// Phase 5: Vivaldi's Masterwork + Paradox Chronometer + Fate Cores
    /// All seasons + cosmic time manipulation
    /// </summary>
    public class SeasonalDestiny : ModItem
    {
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SummerGold = new Color(255, 180, 50);
        private static readonly Color AutumnOrange = new Color(200, 100, 30);
        private static readonly Color WinterBlue = new Color(150, 220, 255);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 2);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<SeasonalDestinyPlayer>();
            modPlayer.seasonalDestinyEquipped = true;
            
            // Vivaldi bonuses
            player.GetDamage(DamageClass.Generic) += 0.22f;
            player.GetCritChance(DamageClass.Generic) += 16;
            player.GetAttackSpeed(DamageClass.Generic) += 0.14f;
            player.statDefense += 22;
            player.lifeRegen += 8;
            player.manaRegen += 5;
            player.endurance += 0.14f;
            player.moveSpeed += 0.18f;
            
            // Chronometer bonuses
            player.GetDamage(DamageClass.Melee) += 0.18f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.20f;
            player.GetCritChance(DamageClass.Melee) += 10;
            
            // Elemental
            player.magmaStone = true;
            player.frostBurn = true;
            player.thorns = 1.5f;
            
            // Immunities
            player.buffImmune[BuffID.Frozen] = true;
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Chilled] = true;
            
            // Ambient VFX
            if (!hideVisual)
            {
                // Seasonal cycle with cosmic overlay
                int season = (int)((Main.time / 10000) % 4);
                Color seasonColor;
                switch (season)
                {
                    case 0: seasonColor = SpringPink; break;
                    case 1: seasonColor = SummerGold; break;
                    case 2: seasonColor = AutumnOrange; break;
                    default: seasonColor = WinterBlue; break;
                }
                
                // Orbiting particles
                if (Main.GameUpdateCount % 10 == 0)
                {
                    float baseAngle = Main.GameUpdateCount * 0.018f;
                    
                    // 4 seasons
                    Color[] sColors = { SpringPink, SummerGold, AutumnOrange, WinterBlue };
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = baseAngle + MathHelper.TwoPi * i / 4f;
                        Vector2 pos = player.Center + angle.ToRotationVector2() * 45f;
                        CustomParticles.GenericFlare(pos, sColors[i], 0.3f, 14);
                    }
                }
                
                // Clock hands overlay
                float handAngle = Main.GameUpdateCount * 0.05f;
                if (Main.rand.NextBool(10))
                {
                    Vector2 hourPos = player.Center + (handAngle * 0.1f).ToRotationVector2() * 18f;
                    CustomParticles.GenericFlare(hourPos, FateCosmicVFX.FateDarkPink, 0.25f, 10);
                    
                    Vector2 minPos = player.Center + handAngle.ToRotationVector2() * 30f;
                    CustomParticles.GenericFlare(minPos, FateCosmicVFX.FateBrightRed, 0.22f, 10);
                }
                
                // Cosmic glyphs
                if (Main.rand.NextBool(20))
                {
                    Vector2 glyphPos = player.Center + Main.rand.NextVector2Circular(40f, 40f);
                    CustomParticles.Glyph(glyphPos, Color.Lerp(seasonColor, FateCosmicVFX.FatePurple, 0.5f) * 0.5f, 0.3f, -1);
                }
                
                // Seasonal particles
                if (Main.rand.NextBool(8))
                {
                    Vector2 particlePos = player.Center + Main.rand.NextVector2Circular(45f, 45f);
                    Vector2 vel = season == 2 ? new Vector2(Main.rand.NextFloat(-1f, 1f), 0.5f) : new Vector2(0, -1f);
                    CustomParticles.GenericGlow(particlePos, vel, seasonColor * 0.7f, 0.28f, 22, true);
                }
                
                // Dual light
                float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.035f) * 0.3f + 0.7f;
                Color lightColor = Color.Lerp(seasonColor, FateCosmicVFX.FateDarkPink, 0.4f);
                Lighting.AddLight(player.Center, lightColor.ToVector3() * pulse * 0.55f);
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<VivaldisMasterwork>()
                .AddIngredient<ParadoxChronometer>()
                .AddIngredient<HarmonicCoreOfFate>(30)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class SeasonalDestinyPlayer : ModPlayer
    {
        public bool seasonalDestinyEquipped;
        private int meleeStrikeCount;
        private int lifestealTimer;

        public override void ResetEffects()
        {
            seasonalDestinyEquipped = false;
        }

        public override void PostUpdate()
        {
            if (lifestealTimer > 0) lifestealTimer--;
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            HandleDestinyHit(target, damageDone, true);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.owner == Player.whoAmI)
            {
                HandleDestinyHit(target, damageDone, DamageClass.Melee.CountsAsClass(proj.DamageType));
            }
        }

        private void HandleDestinyHit(NPC target, int damageDone, bool isMelee)
        {
            if (!seasonalDestinyEquipped) return;
            
            // Temporal Echo (melee, every 7th)
            if (isMelee)
            {
                meleeStrikeCount++;
                if (meleeStrikeCount >= 7)
                {
                    meleeStrikeCount = 0;
                    int echoDamage = (int)(damageDone * 0.75f);
                    target.SimpleStrikeNPC(echoDamage, 0, false, 0, null, false, 0, true);
                    
                    CustomParticles.GenericFlare(target.Center, FateCosmicVFX.FateDarkPink, 0.7f, 18);
                    CustomParticles.HaloRing(target.Center, FateCosmicVFX.FatePurple, 0.45f, 14);
                }
            }
            
            // Seasonal lifesteal (8%)
            if (lifestealTimer <= 0 && Main.rand.NextFloat() < 0.08f)
            {
                lifestealTimer = 30;
                int healAmount = Math.Max(1, Math.Min((int)(damageDone * 0.08f), 18));
                Player.Heal(healAmount);
            }
        }
    }
    #endregion

    #region Theme Wanderer - Complete Harmony + Mobility
    /// <summary>
    /// Phase 5: Complete Harmony + Machination of the Event Horizon + Fate Cores
    /// All five themes + cosmic mobility
    /// </summary>
    public class ThemeWanderer : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 2);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<ThemeWandererPlayer>();
            modPlayer.themeWandererEquipped = true;
            
            bool isNight = !Main.dayTime;
            
            // Complete Harmony bonuses (condensed)
            if (isNight)
            {
                player.GetDamage(DamageClass.Generic) += 0.20f;
                player.GetCritChance(DamageClass.Generic) += 22;
                player.statDefense += 15;
            }
            
            player.GetDamage(DamageClass.Melee) += 0.22f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.18f;
            player.GetDamage(DamageClass.Magic) += 0.25f;
            player.manaCost -= 0.15f;
            player.GetDamage(DamageClass.Generic) += 0.38f;
            player.GetCritChance(DamageClass.Generic) += 20;
            
            // Event Horizon bonuses
            player.moveSpeed += 0.35f;
            player.runAcceleration *= 1.4f;
            player.maxRunSpeed *= 1.3f;
            player.wingTimeMax += 80;
            player.noFallDmg = true;
            
            // Immunities
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.buffImmune[BuffID.Confused] = true;
            
            // Ambient VFX
            if (!hideVisual)
            {
                // Theme trail while moving
                if (player.velocity.Length() > 3f)
                {
                    if (Main.rand.NextBool(3))
                    {
                        Color[] themeColors = {
                            MoonlightColors.Purple,
                            EroicaColors.Gold,
                            CampanellaColors.Orange,
                            EnigmaColors.GreenFlame,
                            SwanColors.GetRainbow(Main.rand.NextFloat())
                        };
                        
                        Color trailColor = themeColors[Main.rand.Next(5)];
                        Vector2 trailPos = player.Center - player.velocity * 0.2f + Main.rand.NextVector2Circular(15f, 15f);
                        CustomParticles.GenericGlow(trailPos, -player.velocity * 0.05f, trailColor * 0.6f, 0.3f, 18, true);
                    }
                }
                
                // Five theme orbit
                if (Main.GameUpdateCount % 8 == 0)
                {
                    float baseAngle = Main.GameUpdateCount * 0.02f;
                    Color[] tColors = {
                        MoonlightColors.Purple, EroicaColors.Gold,
                        CampanellaColors.Orange, EnigmaColors.GreenFlame, SwanColors.GetRainbow(0f)
                    };
                    
                    for (int i = 0; i < 5; i++)
                    {
                        float angle = baseAngle + MathHelper.TwoPi * i / 5f;
                        Vector2 pos = player.Center + angle.ToRotationVector2() * 50f;
                        CustomParticles.GenericFlare(pos, tColors[i], 0.28f, 12);
                    }
                }
                
                // Event Horizon glow
                if (Main.rand.NextBool(15))
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 pos = player.Center + angle.ToRotationVector2() * 35f;
                    CustomParticles.GenericFlare(pos, FateCosmicVFX.FateDarkPink, 0.25f, 10);
                }
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<CompleteHarmony>()
                .AddIngredient<MachinationoftheEventHorizon>()
                .AddIngredient<HarmonicCoreOfFate>(30)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class ThemeWandererPlayer : ModPlayer
    {
        public bool themeWandererEquipped;
        private int dashCooldown;
        private int bellRingCooldown;
        private Dictionary<int, int> paradoxStacks = new Dictionary<int, int>();

        public override void ResetEffects()
        {
            themeWandererEquipped = false;
        }

        public override void PostUpdate()
        {
            if (dashCooldown > 0) dashCooldown--;
            if (bellRingCooldown > 0) bellRingCooldown--;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!themeWandererEquipped) return;
            if (proj.owner != Player.whoAmI) return;
            
            // Theme effects (15% each)
            if (Main.rand.NextFloat() < 0.15f)
            {
                target.AddBuff(BuffID.OnFire, 300);
            }
            
            if (Main.rand.NextFloat() < 0.15f && bellRingCooldown <= 0)
            {
                bellRingCooldown = 30;
                target.AddBuff(BuffID.Confused, 90);
                CustomParticles.HaloRing(target.Center, CampanellaColors.Orange, 0.45f, 14);
            }
            
            if (Main.rand.NextFloat() < 0.12f)
            {
                int[] debuffs = { BuffID.Confused, BuffID.Slow, BuffID.CursedInferno };
                target.AddBuff(debuffs[Main.rand.Next(3)], 180);
            }
        }

        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (!themeWandererEquipped) return false;
            if (dashCooldown > 0) return false;
            
            if (Main.rand.NextFloat() < 0.14f)
            {
                dashCooldown = 150;
                
                CustomParticles.GenericFlare(Player.Center, Color.White, 1.4f, 25);
                
                Color[] colors = {
                    MoonlightColors.Purple, EroicaColors.Gold,
                    CampanellaColors.Orange, EnigmaColors.GreenFlame, SwanColors.GetRainbow(0f)
                };
                
                for (int i = 0; i < 8; i++)
                {
                    CustomParticles.HaloRing(Player.Center, colors[i % 5], 0.35f + i * 0.08f, 14 + i * 2);
                }
                
                Player.immune = true;
                Player.immuneTime = 35;
                
                return true;
            }
            
            return false;
        }
    }
    #endregion

    #region Summoner's Magnum Opus - Complete Harmony + Summons
    /// <summary>
    /// Phase 5: Complete Harmony + Orrery of Infinite Orbits + Fate Cores
    /// All themes + ultimate summoning, minions gain theme abilities
    /// </summary>
    public class SummonersMagnumOpus : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 2);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<SummonersMagnumOpusPlayer>();
            modPlayer.summonOpusEquipped = true;
            
            bool isNight = !Main.dayTime;
            
            // Complete Harmony bonuses (condensed)
            if (isNight)
            {
                player.GetDamage(DamageClass.Generic) += 0.20f;
                player.GetCritChance(DamageClass.Generic) += 20;
            }
            
            player.GetDamage(DamageClass.Generic) += 0.35f;
            player.GetCritChance(DamageClass.Generic) += 18;
            
            // Orrery bonuses (enhanced)
            player.maxMinions += 5;
            player.GetDamage(DamageClass.Summon) += 0.30f;
            player.GetCritChance(DamageClass.Summon) += 10;
            
            // Minion theme abilities via player hook
            
            // Immunities
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            
            // Ambient VFX
            if (!hideVisual)
            {
                // Orrery orbits with theme colors
                if (Main.GameUpdateCount % 8 == 0)
                {
                    float baseAngle = Main.GameUpdateCount * 0.015f;
                    
                    // Inner orbit - orrery planets
                    for (int i = 0; i < 3; i++)
                    {
                        float angle = baseAngle + MathHelper.TwoPi * i / 3f;
                        Vector2 pos = player.Center + angle.ToRotationVector2() * 35f;
                        Color orbColor = Color.Lerp(FateCosmicVFX.FateDarkPink, FateCosmicVFX.FatePurple, (float)i / 3f);
                        CustomParticles.GenericFlare(pos, orbColor, 0.3f, 12);
                    }
                    
                    // Outer orbit - 5 themes
                    Color[] themeColors = {
                        MoonlightColors.Purple, EroicaColors.Gold,
                        CampanellaColors.Orange, EnigmaColors.GreenFlame, SwanColors.GetRainbow(0f)
                    };
                    
                    for (int i = 0; i < 5; i++)
                    {
                        float angle = -baseAngle * 0.7f + MathHelper.TwoPi * i / 5f;
                        Vector2 pos = player.Center + angle.ToRotationVector2() * 55f;
                        CustomParticles.GenericFlare(pos, themeColors[i], 0.25f, 12);
                    }
                }
                
                // Summon sparkles
                if (Main.rand.NextBool(12))
                {
                    Vector2 sparklePos = player.Center + Main.rand.NextVector2Circular(45f, 45f);
                    var sparkle = new SparkleParticle(sparklePos, Main.rand.NextVector2Circular(1f, 1f),
                        SwanColors.GetRainbow(Main.rand.NextFloat()), 0.3f, 16);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<CompleteHarmony>()
                .AddIngredient<OrreryofInfiniteOrbits>()
                .AddIngredient<HarmonicCoreOfFate>(30)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class SummonersMagnumOpusPlayer : ModPlayer
    {
        public bool summonOpusEquipped;
        private int themeEffectCooldown;

        public override void ResetEffects()
        {
            summonOpusEquipped = false;
        }

        public override void PostUpdate()
        {
            if (themeEffectCooldown > 0) themeEffectCooldown--;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!summonOpusEquipped) return;
            if (proj.owner != Player.whoAmI) return;
            
            // Minion theme attacks
            if (proj.minion || proj.sentry || DamageClass.Summon.CountsAsClass(proj.DamageType))
            {
                if (themeEffectCooldown <= 0)
                {
                    themeEffectCooldown = 15;
                    
                    // Random theme effect
                    int effect = Main.rand.Next(5);
                    switch (effect)
                    {
                        case 0: // Moonlight
                            if (!Main.dayTime)
                            {
                                int bonusDamage = (int)(damageDone * 0.15f);
                                target.SimpleStrikeNPC(bonusDamage, 0, false, 0, null, false, 0, true);
                                CustomParticles.GenericFlare(target.Center, MoonlightColors.Purple, 0.4f, 12);
                            }
                            break;
                            
                        case 1: // Eroica
                            target.AddBuff(BuffID.Ichor, 120);
                            CustomParticles.GenericFlare(target.Center, EroicaColors.Gold, 0.4f, 12);
                            break;
                            
                        case 2: // La Campanella
                            target.AddBuff(BuffID.OnFire, 180);
                            CustomParticles.GenericFlare(target.Center, CampanellaColors.Orange, 0.4f, 12);
                            break;
                            
                        case 3: // Enigma
                            int[] debuffs = { BuffID.Confused, BuffID.Slow };
                            target.AddBuff(debuffs[Main.rand.Next(2)], 120);
                            CustomParticles.GenericFlare(target.Center, EnigmaColors.GreenFlame, 0.4f, 12);
                            break;
                            
                        case 4: // Swan Lake
                            CustomParticles.SwanFeatherDrift(target.Center, SwanColors.White, 0.4f);
                            var sparkle = new SparkleParticle(target.Center, Main.rand.NextVector2Circular(2f, 2f),
                                SwanColors.GetRainbow(Main.rand.NextFloat()), 0.35f, 14);
                            MagnumParticleHandler.SpawnParticle(sparkle);
                            break;
                    }
                }
            }
        }
    }
    #endregion
}
