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
using MagnumOpus.Content.Eroica.Accessories.Shared;
using MagnumOpus.Content.LaCampanella.Accessories;
using MagnumOpus.Content.EnigmaVariations.Accessories;
using MagnumOpus.Content.SwanLake.Accessories;
using MagnumOpus.Content.Fate.Accessories;
using EroicaColors = MagnumOpus.Common.Systems.CustomParticleSystem.EroicaColors;
using MagnumOpus.Content.Eroica;
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
            player.GetDamage(DamageClass.Melee) += 0.10f;
            player.GetDamage(DamageClass.Ranged) += 0.10f;
            player.GetDamage(DamageClass.Magic) += 0.10f;
            player.GetDamage(DamageClass.Summon) += 0.10f;
            player.statDefense += 20;
            player.statLifeMax2 += 60;
            player.statManaMax2 += 80;
            player.lifeRegen += 8;
            player.manaRegen += 4;
            player.endurance += 0.10f;
            player.moveSpeed += 0.15f;
            player.maxMinions += 1;
            player.maxTurrets += 1;
            player.magmaStone = true;
            player.frostBurn = true;
            player.thorns = 1.5f;
            
            // === THEME BONUSES (from Complete Harmony) ===
            // Moonlight
            if (isNight)
            {
                player.GetCritChance(DamageClass.Melee) += 6;
                player.GetCritChance(DamageClass.Ranged) += 6;
                player.GetCritChance(DamageClass.Magic) += 6;
                player.GetCritChance(DamageClass.Summon) += 6;
                player.statDefense += 15;
                player.moveSpeed += 0.08f;
            }
            
            // Eroica
            player.GetDamage(DamageClass.Melee) += 0.14f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.15f;
            player.GetCritChance(DamageClass.Melee) += 10;
            player.GetArmorPenetration(DamageClass.Melee) += 12;
            
            // La Campanella
            player.GetDamage(DamageClass.Magic) += 0.16f;
            player.GetCritChance(DamageClass.Magic) += 10;
            player.manaCost -= 0.10f;
            
            // Enigma
            player.GetDamage(DamageClass.Ranged) += 0.14f;
            player.GetCritChance(DamageClass.Ranged) += 8;
            player.ammoCost80 = true;
            
            // Swan Lake
            player.GetDamage(DamageClass.Summon) += 0.14f;
            player.GetCritChance(DamageClass.Summon) += 8;
            player.whipRangeMultiplier += 0.12f;
            player.moveSpeed += 0.25f;
            
            // Immunities
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.buffImmune[BuffID.Frozen] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.Poisoned] = true;
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

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color opusColor = new Color(200, 100, 255);
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Seasonal: +20% damage, +15 crit, +12% attack speed, +20 defense, +15% movement")
            {
                OverrideColor = opusColor
            });
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+60 max life, +80 max mana, +1 minion, +1 sentry")
            {
                OverrideColor = opusColor
            });
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Theme bonuses: Eroica (melee speed/armor penetration), Campanella (magic power/mana efficiency), Enigma (ranged + ammo economy), Swan (summon + whip reach)")
            {
                OverrideColor = opusColor
            });
            tooltips.Add(new TooltipLine(Mod, "Effect4", "+8 life regen, +4 mana regen, +12% damage reduction, 150% thorns")
            {
                OverrideColor = opusColor
            });
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Paradox stacks trigger grand explosions, bell ring AOE, inflicts fire and frostburn")
            {
                OverrideColor = opusColor
            });
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Immunity to fire, frost, and poison effects")
            {
                OverrideColor = opusColor
            });
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
                Player.GetAttackSpeed(DamageClass.Melee) += 0.10f;
                Player.GetAttackSpeed(DamageClass.Ranged) += 0.08f;
                Player.GetAttackSpeed(DamageClass.Magic) += 0.08f;
                Player.GetAttackSpeed(DamageClass.Summon) += 0.10f;
                Player.endurance += 0.06f;
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
            player.GetDamage(DamageClass.Melee) += 0.14f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.18f;
            player.GetCritChance(DamageClass.Melee) += 10;
            player.GetArmorPenetration(DamageClass.Melee) += 15;
            
            // === CONSTELLATION COMPASS (Ranged) ===
            player.GetDamage(DamageClass.Ranged) += 0.16f;
            player.GetCritChance(DamageClass.Ranged) += 10;
            player.GetArmorPenetration(DamageClass.Ranged) += 14;
            player.ammoBox = true;
            
            // === ASTRAL CONDUIT (Magic) ===
            player.GetDamage(DamageClass.Magic) += 0.16f;
            player.GetCritChance(DamageClass.Magic) += 8;
            player.manaCost -= 0.14f;
            player.manaRegen += 6;
            player.statManaMax2 += 60;
            
            // === MACHINATION OF THE EVENT HORIZON (Mobility) ===
            player.moveSpeed += 0.30f;
            player.runAcceleration *= 1.35f;
            player.maxRunSpeed *= 1.25f;
            player.wingTimeMax += 60;
            player.noFallDmg = true;
            player.endurance += 0.06f;
            player.statLifeMax2 += 40;
            
            // === ORRERY OF INFINITE ORBITS (Summon) ===
            player.maxMinions += 3;
            player.maxTurrets += 1;
            player.GetDamage(DamageClass.Summon) += 0.15f;
            player.whipRangeMultiplier += 0.15f;
            
            // Cosmic immunities
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.CursedInferno] = true;
            player.buffImmune[BuffID.ShadowFlame] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Confused] = true;
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

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color cosmicColor = new Color(180, 80, 220);
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Combines all five Fate vanilla accessory upgrades")
            {
                OverrideColor = cosmicColor
            });
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Melee/ranged/magic/summon each gain tuned class-specific bonuses")
            {
                OverrideColor = cosmicColor
            });
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+3 minions, +1 sentry, +30% movement, +60 wing time, no fall damage")
            {
                OverrideColor = cosmicColor
            });
            tooltips.Add(new TooltipLine(Mod, "Effect4", "+40 max life, +60 max mana, +6% damage reduction, Ammo Box and mana economy")
            {
                OverrideColor = cosmicColor
            });
            tooltips.Add(new TooltipLine(Mod, "Effect5", "12% cosmic dodge chance with damage burst")
            {
                OverrideColor = cosmicColor
            });
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
            player.GetDamage(DamageClass.Melee) += 0.10f;
            player.GetDamage(DamageClass.Ranged) += 0.08f;
            player.GetDamage(DamageClass.Magic) += 0.08f;
            player.GetDamage(DamageClass.Summon) += 0.08f;
            player.statDefense += 22;
            player.statLifeMax2 += 40;
            player.statManaMax2 += 40;
            player.lifeRegen += 8;
            player.manaRegen += 5;
            player.endurance += 0.10f;
            player.moveSpeed += 0.18f;
            player.maxMinions += 1;
            
            // Chronometer bonuses
            player.GetDamage(DamageClass.Melee) += 0.14f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.16f;
            player.GetCritChance(DamageClass.Melee) += 8;
            player.GetArmorPenetration(DamageClass.Melee) += 10;
            
            // Elemental
            player.magmaStone = true;
            player.frostBurn = true;
            player.thorns = 1.5f;
            
            // Immunities
            player.buffImmune[BuffID.Frozen] = true;
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Chilled] = true;
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

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color destinyColor = new Color(200, 150, 180);
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Combines Vivaldi's Masterwork with Paradox Chronometer")
            {
                OverrideColor = destinyColor
            });
            tooltips.Add(new TooltipLine(Mod, "Effect2", "All-class seasonal support: +defense, regen, movement, and durability")
            {
                OverrideColor = destinyColor
            });
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Chronometer: +18% melee damage, +20% melee speed, +10 melee crit, +12 armor penetration")
            {
                OverrideColor = destinyColor
            });
            tooltips.Add(new TooltipLine(Mod, "Effect4", "+40 max life, +40 max mana, +1 minion, +10% damage reduction, 150% thorns")
            {
                OverrideColor = destinyColor
            });
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Temporal echoes on every 7th melee strike, 8% lifesteal (max 18 HP)")
            {
                OverrideColor = destinyColor
            });
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Inflicts On Fire and Frostburn, immunity to Frozen, On Fire, Frostburn, Chilled")
            {
                OverrideColor = destinyColor
            });
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
                player.GetCritChance(DamageClass.Melee) += 6;
                player.GetCritChance(DamageClass.Ranged) += 6;
                player.GetCritChance(DamageClass.Magic) += 6;
                player.GetCritChance(DamageClass.Summon) += 6;
                player.statDefense += 15;
            }
            
            player.GetDamage(DamageClass.Melee) += 0.12f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.15f;
            player.GetDamage(DamageClass.Ranged) += 0.12f;
            player.GetCritChance(DamageClass.Ranged) += 6;
            player.ammoCost80 = true;
            player.GetDamage(DamageClass.Magic) += 0.14f;
            player.manaCost -= 0.10f;
            player.GetDamage(DamageClass.Summon) += 0.12f;
            player.maxMinions += 1;
            
            // Event Horizon bonuses
            player.moveSpeed += 0.35f;
            player.runAcceleration *= 1.4f;
            player.maxRunSpeed *= 1.3f;
            player.wingTimeMax += 80;
            player.noFallDmg = true;
            player.endurance += 0.08f;
            
            // Immunities
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.buffImmune[BuffID.Confused] = true;
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

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            float hue = (Main.GameUpdateCount * 0.01f) % 1f;
            Color wandererColor = Main.hslToRgb(hue, 0.8f, 0.6f);
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Combines Complete Harmony with Machination of the Event Horizon")
            {
                OverrideColor = wandererColor
            });
            tooltips.Add(new TooltipLine(Mod, "Effect2", "All five theme bonuses tuned for class utility and night crit support")
            {
                OverrideColor = wandererColor
            });
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+35% movement, +30% run speed, +80 wing time, no fall damage")
            {
                OverrideColor = wandererColor
            });
            tooltips.Add(new TooltipLine(Mod, "Effect4", "+8% damage reduction, ammo economy, mana efficiency, +1 minion")
            {
                OverrideColor = wandererColor
            });
            tooltips.Add(new TooltipLine(Mod, "Effect5", "14% dodge chance with multi-theme burst")
            {
                OverrideColor = wandererColor
            });
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
                player.GetDamage(DamageClass.Summon) += 0.12f;
                player.GetCritChance(DamageClass.Summon) += 6;
            }
            
            player.GetDamage(DamageClass.Summon) += 0.24f;
            player.GetCritChance(DamageClass.Summon) += 8;
            
            // Orrery bonuses (enhanced)
            player.maxMinions += 4;
            player.maxTurrets += 1;
            player.whipRangeMultiplier += 0.20f;
            player.statManaMax2 += 60;
            player.manaCost -= 0.10f;
            player.lifeRegen += 3;
            
            // Minion theme abilities via player hook
            
            // Immunities
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
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

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color summonColor = new Color(150, 100, 200);
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Combines Complete Harmony with Orrery of Infinite Orbits")
            {
                OverrideColor = summonColor
            });
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Summoner-focused scaling: +24% summon damage, +8 summon crit, strong night bonus")
            {
                OverrideColor = summonColor
            });
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+4 minions, +1 sentry, +20% whip range")
            {
                OverrideColor = summonColor
            });
            tooltips.Add(new TooltipLine(Mod, "Effect4", "+60 max mana, 10% mana cost reduction, +3 life regen")
            {
                OverrideColor = summonColor
            });
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Night damage, fire debuffs, confusion, and prismatic sparkles")
            {
                OverrideColor = summonColor
            });
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
