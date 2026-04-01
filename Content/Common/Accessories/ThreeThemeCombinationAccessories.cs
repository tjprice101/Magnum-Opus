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
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.HarmonicCores;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.Eroica.HarmonicCores;
using EroicaColors = MagnumOpus.Common.Systems.CustomParticleSystem.EroicaColors;
using MagnumOpus.Content.Eroica;
using MagnumOpus.Content.LaCampanella.ResonanceEnergies;
using MagnumOpus.Content.LaCampanella.HarmonicCores;
using MagnumOpus.Content.EnigmaVariations.ResonanceEnergies;
using MagnumOpus.Content.EnigmaVariations.HarmonicCores;
using MagnumOpus.Content.SwanLake.ResonanceEnergies;
using MagnumOpus.Content.SwanLake.HarmonicCores;

namespace MagnumOpus.Content.Common.Accessories
{
    #region Trinity of Night - Moonlight + La Campanella + Enigma
    /// <summary>
    /// Phase 4 Three-Theme Combination: Moonlight Sonata + La Campanella + Enigma Variations
    /// Ultimate darkness theme combining lunar mysticism, infernal flames, and void mystery
    /// </summary>
    public class TrinityOfNight : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 120);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<TrinityOfNightPlayer>();
            modPlayer.trinityEquipped = true;
            
            bool isNight = !Main.dayTime;
            
            // Moonlight bonuses (enhanced at night)
            if (isNight)
            {
                player.GetDamage(DamageClass.Generic) += 0.22f;
                player.GetCritChance(DamageClass.Generic) += 25;
                player.statDefense += 15;
            }
            else
            {
                player.GetDamage(DamageClass.Generic) += 0.12f;
            }
            
            // La Campanella bonuses
            player.GetDamage(DamageClass.Magic) += 0.25f;
            player.GetCritChance(DamageClass.Magic) += 12;
            player.manaCost -= 0.15f;
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            
            // Enigma bonuses
            player.GetDamage(DamageClass.Generic) += 0.20f;
            player.GetCritChance(DamageClass.Generic) += 10;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color darkPurple = new Color(100, 50, 150);
            Color moonBlue = new Color(100, 150, 255);
            Color flameOrange = new Color(255, 140, 40);
            
            tooltips.Add(new TooltipLine(Mod, "Combo", "Combines: Moonlight Sonata + La Campanella + Enigma Variations")
            {
                OverrideColor = darkPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "MoonlightStats", "At night: +22% damage, +25 crit chance, +15 defense | Day: +12% damage")
            {
                OverrideColor = moonBlue
            });
            
            tooltips.Add(new TooltipLine(Mod, "CampanellaStats", "+25% magic damage, +12 magic crit, -15% mana cost")
            {
                OverrideColor = flameOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "EnigmaStats", "+20% all damage, +10 crit chance")
            {
                OverrideColor = EnigmaColors.GreenFlame
            });
            
            tooltips.Add(new TooltipLine(Mod, "Immunities", "Immunity to On Fire! and Burning")
            {
                OverrideColor = flameOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effects", "15% Paradox stacking (5 stacks = 3x dmg + 250 range AOE), 12% Bell ring stun")
            {
                OverrideColor = darkPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "BlueFireBonus", "+20% magic damage at night as blue fire")
            {
                OverrideColor = moonBlue
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Three dark powers united - lunar mysticism, infernal flames, and void mystery'")
            {
                OverrideColor = new Color(180, 150, 200)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<NocturneOfAzureFlames>()
                .AddIngredient<RiddleOfTheVoid>()
                .AddIngredient<HarmonicCoreOfMoonlightSonata>(20)
                .AddIngredient<HarmonicCoreOfLaCampanella>(20)
                .AddIngredient<HarmonicCoreOfEnigma>(20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class TrinityOfNightPlayer : ModPlayer
    {
        public bool trinityEquipped;
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
            trinityEquipped = false;
        }

        public override void PostUpdate()
        {
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

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!trinityEquipped) return;
            if (proj.owner != Player.whoAmI) return;
            
            bool isNight = !Main.dayTime;
            
            // Blue fire at night bonus
            if (isNight && DamageClass.Magic.CountsAsClass(proj.DamageType))
            {
                int bonusDamage = (int)(damageDone * 0.20f);
                target.SimpleStrikeNPC(bonusDamage, 0, false, 0, null, false, 0, true);
                
                Color blueFlame = new Color(100, 150, 255);
            }
            
            // Paradox (15%)
            if (Main.rand.NextFloat() < 0.15f)
            {
                int debuffId = ParadoxDebuffs[Main.rand.Next(ParadoxDebuffs.Length)];
                target.AddBuff(debuffId, 300);
                target.AddBuff(BuffID.OnFire, 240);
                
                if (!paradoxStacks.ContainsKey(target.whoAmI))
                    paradoxStacks[target.whoAmI] = 0;
                
                paradoxStacks[target.whoAmI]++;
                paradoxTimers[target.whoAmI] = 360;
                
                int stacks = paradoxStacks[target.whoAmI];
                
                // Trinity VFX - all three colors
                for (int i = 0; i < 9; i++)
                {
                    float angle = MathHelper.TwoPi * i / 9f;
                    Vector2 offset = angle.ToRotationVector2() * (18f + stacks * 3f);
                    
                    Color color;
                    if (i % 3 == 0)
                        color = isNight ? new Color(100, 150, 255) : MoonlightColors.Purple;
                    else if (i % 3 == 1)
                        color = CampanellaColors.Orange;
                    else
                        color = EnigmaColors.GreenFlame;
                    
                }
                
                
                // Void Collapse at 5 stacks
                if (stacks >= 5)
                {
                    TriggerTrinityCollapse(target, damageDone, isNight);
                    paradoxStacks[target.whoAmI] = 0;
                }
            }
            
            // Bell ring (12%)
            if (bellRingCooldown <= 0 && Main.rand.NextFloat() < 0.12f)
            {
                bellRingCooldown = 25;
                target.AddBuff(BuffID.Confused, 120);
                
                Color chimeColor = Color.Lerp(CampanellaColors.Orange, EnigmaColors.GreenFlame, 0.4f);
                
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item35 with { Pitch = -0.2f }, target.Center);
            }
        }

        private void TriggerTrinityCollapse(NPC target, int baseDamage, bool isNight)
        {
            // Trinity explosion - all three dark powers converge
            
            // Triple halos
            for (int ring = 0; ring < 12; ring++)
            {
                Color ringColor;
                if (ring % 3 == 0)
                    ringColor = MoonlightColors.Purple;
                else if (ring % 3 == 1)
                    ringColor = CampanellaColors.Orange;
                else
                    ringColor = EnigmaColors.GreenFlame;
                
            }
            
            // Massive glyph burst
            
            // Eye formation
            
            
            // Massive damage
            if (Main.myPlayer == Player.whoAmI)
            {
                int trinityDamage = (int)(baseDamage * 3.0f);
                target.SimpleStrikeNPC(trinityDamage, 0, false, 0, null, false, 0, true);
                
                float aoeRadius = 250f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.whoAmI != target.whoAmI && !npc.immortal)
                    {
                        if (Vector2.Distance(npc.Center, target.Center) <= aoeRadius)
                        {
                            npc.SimpleStrikeNPC(trinityDamage / 2, 0, false, 0, null, false, 0, true);
                            npc.AddBuff(BuffID.OnFire, 300);
                            npc.AddBuff(ParadoxDebuffs[Main.rand.Next(ParadoxDebuffs.Length)], 240);
                            
                        }
                    }
                }
            }
            
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.6f, Volume = 1.4f }, target.Center);
        }
    }
    #endregion

    #region Adagio of Radiant Valor - Eroica + Moonlight + Swan Lake
    /// <summary>
    /// Phase 4 Three-Theme Combination: Eroica + Moonlight Sonata + Swan Lake
    /// Ultimate noble theme combining valor, moonlight, and balletic grace
    /// </summary>
    public class AdagioOfRadiantValor : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 120);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<AdagioOfRadiantValorPlayer>();
            modPlayer.adagioOfRadiantValorEquipped = true;
            
            bool isNight = !Main.dayTime;
            
            // Eroica bonuses
            player.GetDamage(DamageClass.Melee) += 0.22f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.18f;
            player.GetCritChance(DamageClass.Melee) += 12;
            player.GetDamage(DamageClass.Generic) += 0.10f;
            
            // Moonlight bonuses
            if (isNight)
            {
                player.GetDamage(DamageClass.Generic) += 0.20f;
                player.GetCritChance(DamageClass.Generic) += 22;
                player.statDefense += 14;
            }
            else
            {
                player.GetDamage(DamageClass.Generic) += 0.10f;
            }
            
            // Swan Lake bonuses
            player.GetDamage(DamageClass.Generic) += 0.18f;
            player.GetCritChance(DamageClass.Generic) += 10;
            player.moveSpeed += 0.22f;
            player.runAcceleration *= 1.22f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color gold = new Color(255, 200, 80);
            Color moonSilver = new Color(200, 200, 230);
            Color rainbow = SwanColors.GetRainbow((float)(Main.GameUpdateCount % 300) / 300f);
            
            tooltips.Add(new TooltipLine(Mod, "Combo", "Combines: Eroica + Moonlight Sonata + Swan Lake")
            {
                OverrideColor = gold
            });
            
            tooltips.Add(new TooltipLine(Mod, "EroicaStats", "+22% melee damage, +18% melee attack speed, +12 melee crit, +10% all damage")
            {
                OverrideColor = EroicaColors.Gold
            });
            
            tooltips.Add(new TooltipLine(Mod, "MoonlightStats", "At night: +20% damage, +22 crit chance, +14 defense | Day: +10% damage")
            {
                OverrideColor = moonSilver
            });
            
            tooltips.Add(new TooltipLine(Mod, "SwanStats", "+18% all damage, +10 crit chance, +22% movement speed")
            {
                OverrideColor = rainbow
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effects", "Kills trigger 5-sec Heroic Surge (+30% damage), 14% dodge at night (10% day)")
            {
                OverrideColor = gold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Noble valor, moonlit grace, and balletic elegance united'")
            {
                OverrideColor = new Color(220, 200, 240)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<HerosSymphony>()
                .AddIngredient<ReverieOfTheSilverSwan>()
                .AddIngredient<HarmonicCoreOfEroica>(20)
                .AddIngredient<HarmonicCoreOfMoonlightSonata>(20)
                .AddIngredient<HarmonicCoreOfSwanLake>(20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class AdagioOfRadiantValorPlayer : ModPlayer
    {
        public bool adagioOfRadiantValorEquipped;
        private int heroicSurgeTimer;
        private int invulnFramesOnKill = 90;
        private int dodgeCooldown;

        public override void ResetEffects()
        {
            adagioOfRadiantValorEquipped = false;
        }

        public override void PostUpdate()
        {
            if (heroicSurgeTimer > 0)
            {
                heroicSurgeTimer--;
                Player.GetDamage(DamageClass.Generic) += 0.30f;
            }
            
            if (dodgeCooldown > 0) dodgeCooldown--;
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            HandleKill(target);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.owner == Player.whoAmI)
            {
                HandleKill(target);
            }
        }

        private void HandleKill(NPC target)
        {
            if (!adagioOfRadiantValorEquipped) return;
            
            if (target.life <= 0 && !target.immortal)
            {
                // Extended invulnerability
                Player.immune = true;
                Player.immuneTime = Math.Max(Player.immuneTime, invulnFramesOnKill);
                
                // Extended Heroic Surge
                heroicSurgeTimer = 360;
                
                // Noble kill VFX
                
                
                
                for (int i = 0; i < 8; i++)
                {
                    Color haloColor = Color.Lerp(
                        Color.Lerp(EroicaColors.Gold, MoonlightColors.Silver, i / 8f),
                        SwanColors.GetRainbow(i / 8f), 0.3f);
                }
            }
        }

        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (!adagioOfRadiantValorEquipped) return false;
            if (dodgeCooldown > 0) return false;
            
            bool isNight = !Main.dayTime;
            float dodgeChance = isNight ? 0.14f : 0.10f;
            
            if (Main.rand.NextFloat() < dodgeChance)
            {
                dodgeCooldown = 80;
                TriggerNobleGraceDodge();
                return true;
            }
            
            return false;
        }

        private void TriggerNobleGraceDodge()
        {
            // Noble grace dodge
            
            for (int i = 0; i < 9; i++)
            {
                Color haloColor;
                if (i % 3 == 0)
                    haloColor = EroicaColors.Gold;
                else if (i % 3 == 1)
                    haloColor = MoonlightColors.Silver;
                else
                    haloColor = SwanColors.GetRainbow(i / 9f);
                
            }
            
            
            
            Player.immune = true;
            Player.immuneTime = 30;
            
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.5f }, Player.Center);
        }
    }
    #endregion

    #region Requiem of the Enigmatic Flame - La Campanella + Enigma + Swan Lake
    /// <summary>
    /// Phase 4 Three-Theme Combination: La Campanella + Enigma Variations + Swan Lake
    /// Ultimate chaos theme combining fire, mystery, and grace
    /// </summary>
    public class RequiemOfTheEnigmaticFlame : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 120);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<RequiemOfTheEnigmaticFlamePlayer>();
            modPlayer.requiemOfTheEnigmaticFlameEquipped = true;
            
            // La Campanella bonuses
            player.GetDamage(DamageClass.Magic) += 0.25f;
            player.GetCritChance(DamageClass.Magic) += 12;
            player.manaCost -= 0.15f;
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            
            // Enigma bonuses
            player.GetDamage(DamageClass.Generic) += 0.20f;
            player.GetCritChance(DamageClass.Generic) += 10;
            
            // Swan Lake bonuses
            player.GetDamage(DamageClass.Generic) += 0.18f;
            player.GetCritChance(DamageClass.Generic) += 10;
            player.moveSpeed += 0.22f;
            player.runAcceleration *= 1.22f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color flameOrange = new Color(255, 140, 40);
            Color greenFlame = EnigmaColors.GreenFlame;
            Color rainbow = SwanColors.GetRainbow((float)(Main.GameUpdateCount % 300) / 300f);
            
            tooltips.Add(new TooltipLine(Mod, "Combo", "Combines: La Campanella + Enigma Variations + Swan Lake")
            {
                OverrideColor = flameOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "CampanellaStats", "+25% magic damage, +12 magic crit, -15% mana cost")
            {
                OverrideColor = flameOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "EnigmaStats", "+20% all damage, +10 crit chance")
            {
                OverrideColor = greenFlame
            });
            
            tooltips.Add(new TooltipLine(Mod, "SwanStats", "+18% all damage, +10 crit chance, +22% movement speed")
            {
                OverrideColor = rainbow
            });
            
            tooltips.Add(new TooltipLine(Mod, "Immunities", "Immunity to On Fire! and Burning")
            {
                OverrideColor = flameOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effects", "18% Paradox (magic) / 12% (other), 12% Bell ring stun (140 range, 60% AOE), 12% dodge")
            {
                OverrideColor = greenFlame
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Beautiful chaos - fire, mystery, and grace intertwined'")
            {
                OverrideColor = new Color(200, 180, 220)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<FantasiaOfBurningGrace>()
                .AddIngredient<RiddleOfTheVoid>()
                .AddIngredient<HarmonicCoreOfLaCampanella>(20)
                .AddIngredient<HarmonicCoreOfEnigma>(20)
                .AddIngredient<HarmonicCoreOfSwanLake>(20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class RequiemOfTheEnigmaticFlamePlayer : ModPlayer
    {
        public bool requiemOfTheEnigmaticFlameEquipped;
        private int bellRingCooldown;
        private int dodgeCooldown;
        private Dictionary<int, int> paradoxStacks = new Dictionary<int, int>();
        private Dictionary<int, int> paradoxTimers = new Dictionary<int, int>();
        
        private static readonly int[] ParadoxDebuffs = new int[]
        {
            BuffID.Confused, BuffID.Slow, BuffID.CursedInferno,
            BuffID.Ichor, BuffID.ShadowFlame, BuffID.Frostburn
        };

        public override void ResetEffects()
        {
            requiemOfTheEnigmaticFlameEquipped = false;
        }

        public override void PostUpdate()
        {
            if (bellRingCooldown > 0) bellRingCooldown--;
            if (dodgeCooldown > 0) dodgeCooldown--;
            
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

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!requiemOfTheEnigmaticFlameEquipped) return;
            if (proj.owner != Player.whoAmI) return;
            
            // Chaotic Paradox (18% for magic)
            float paradoxChance = DamageClass.Magic.CountsAsClass(proj.DamageType) ? 0.18f : 0.12f;
            
            if (Main.rand.NextFloat() < paradoxChance)
            {
                int debuffId = ParadoxDebuffs[Main.rand.Next(ParadoxDebuffs.Length)];
                target.AddBuff(debuffId, 300);
                target.AddBuff(BuffID.OnFire, 240);
                
                if (!paradoxStacks.ContainsKey(target.whoAmI))
                    paradoxStacks[target.whoAmI] = 0;
                
                paradoxStacks[target.whoAmI]++;
                paradoxTimers[target.whoAmI] = 360;
                
                int stacks = paradoxStacks[target.whoAmI];
                
                // Chaotic VFX
                for (int i = 0; i < 9 + stacks; i++)
                {
                    float angle = MathHelper.TwoPi * i / (9 + stacks);
                    Vector2 offset = angle.ToRotationVector2() * (18f + stacks * 3f);
                    
                    Color color;
                    if (i % 3 == 0)
                        color = CampanellaColors.Orange;
                    else if (i % 3 == 1)
                        color = EnigmaColors.GreenFlame;
                    else
                        color = SwanColors.GetRainbow((float)i / (9 + stacks));
                    
                }
                
                
                // Chaos Collapse at 5 stacks
                if (stacks >= 5)
                {
                    TriggerChaosCollapse(target, damageDone);
                    paradoxStacks[target.whoAmI] = 0;
                }
            }
            
            // Bell ring with rainbow fire (12%)
            if (bellRingCooldown <= 0 && Main.rand.NextFloat() < 0.12f)
            {
                bellRingCooldown = 25;
                target.AddBuff(BuffID.Confused, 120);
                
                
                // AOE chaos fire
                float aoeRadius = 140f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.whoAmI != target.whoAmI && !npc.immortal)
                    {
                        if (Vector2.Distance(npc.Center, target.Center) <= aoeRadius)
                        {
                            int aoeDamage = (int)(damageDone * 0.6f);
                            npc.SimpleStrikeNPC(aoeDamage, 0, false, 0, null, false, 0, true);
                            npc.AddBuff(BuffID.OnFire, 240);
                            npc.AddBuff(ParadoxDebuffs[Main.rand.Next(ParadoxDebuffs.Length)], 150);
                            
                        }
                    }
                }
                
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.2f }, target.Center);
            }
        }

        private void TriggerChaosCollapse(NPC target, int baseDamage)
        {
            // Beautiful chaos explosion
            
            // Rainbow chaos halos
            for (int ring = 0; ring < 12; ring++)
            {
                Color ringColor;
                if (ring % 3 == 0)
                    ringColor = CampanellaColors.Orange;
                else if (ring % 3 == 1)
                    ringColor = EnigmaColors.GreenFlame;
                else
                    ringColor = SwanColors.GetRainbow(ring / 12f);
                
            }
            
            // Burning feather explosion
            
            // Glyph spiral
            
            // Eye formation
            
            
            // Rainbow sparkle spiral
            
            // Massive damage
            if (Main.myPlayer == Player.whoAmI)
            {
                int chaosDamage = (int)(baseDamage * 3.0f);
                target.SimpleStrikeNPC(chaosDamage, 0, false, 0, null, false, 0, true);
                
                float aoeRadius = 260f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.whoAmI != target.whoAmI && !npc.immortal)
                    {
                        if (Vector2.Distance(npc.Center, target.Center) <= aoeRadius)
                        {
                            npc.SimpleStrikeNPC(chaosDamage / 2, 0, false, 0, null, false, 0, true);
                            npc.AddBuff(BuffID.OnFire, 360);
                            npc.AddBuff(ParadoxDebuffs[Main.rand.Next(ParadoxDebuffs.Length)], 240);
                            
                        }
                    }
                }
            }
            
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.3f, Volume = 1.4f }, target.Center);
        }

        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (!requiemOfTheEnigmaticFlameEquipped) return false;
            if (dodgeCooldown > 0) return false;
            
            if (Main.rand.NextFloat() < 0.12f)
            {
                dodgeCooldown = 80;
                
                // Chaotic dodge
                
                for (int i = 0; i < 9; i++)
                {
                    Color haloColor;
                    if (i % 3 == 0)
                        haloColor = CampanellaColors.Orange;
                    else if (i % 3 == 1)
                        haloColor = EnigmaColors.GreenFlame;
                    else
                        haloColor = SwanColors.GetRainbow(i / 9f);
                    
                }
                
                
                Player.immune = true;
                Player.immuneTime = 25;
                
                return true;
            }
            
            return false;
        }
    }
    #endregion

    #region Complete Harmony - All 5 Themes
    /// <summary>
    /// Phase 4 Ultimate Combination: All 5 Themes Combined
    /// Moonlight Sonata + Eroica + La Campanella + Enigma Variations + Swan Lake
    /// Ultimate musical achievement - all themes at full strength
    /// </summary>
    public class CompleteHarmony : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 200);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<CompleteHarmonyPlayer>();
            modPlayer.completeHarmonyEquipped = true;
            
            bool isNight = !Main.dayTime;
            
            // === ALL FIVE THEMES COMBINED ===
            
            // Moonlight Sonata
            if (isNight)
            {
                player.GetDamage(DamageClass.Generic) += 0.20f;
                player.GetCritChance(DamageClass.Generic) += 22;
                player.statDefense += 15;
            }
            else
            {
                player.GetDamage(DamageClass.Generic) += 0.12f;
            }
            
            // Eroica
            player.GetDamage(DamageClass.Melee) += 0.22f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.18f;
            player.GetCritChance(DamageClass.Melee) += 12;
            player.GetDamage(DamageClass.Generic) += 0.10f;
            
            // La Campanella
            player.GetDamage(DamageClass.Magic) += 0.25f;
            player.GetCritChance(DamageClass.Magic) += 12;
            player.manaCost -= 0.15f;
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            
            // Enigma Variations
            player.GetDamage(DamageClass.Generic) += 0.20f;
            player.GetCritChance(DamageClass.Generic) += 10;
            
            // Swan Lake
            player.GetDamage(DamageClass.Generic) += 0.18f;
            player.GetCritChance(DamageClass.Generic) += 10;
            player.moveSpeed += 0.25f;
            player.runAcceleration *= 1.25f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color harmonyGold = new Color(255, 220, 100);
            Color moonSilver = new Color(150, 170, 220);
            Color eroicaGold = new Color(255, 200, 80);
            Color flameOrange = new Color(255, 140, 40);
            Color greenFlame = EnigmaColors.GreenFlame;
            Color rainbow = SwanColors.GetRainbow((float)(Main.GameUpdateCount % 300) / 300f);
            
            tooltips.Add(new TooltipLine(Mod, "Ultimate", "The complete harmony of ALL FIVE THEMES")
            {
                OverrideColor = harmonyGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Combo", "Moonlight Sonata + Eroica + La Campanella + Enigma Variations + Swan Lake")
            {
                OverrideColor = new Color(200, 180, 255)
            });
            
            tooltips.Add(new TooltipLine(Mod, "MoonlightStats", "Night: +20% damage, +22 crit, +15 defense | Day: +12% damage")
            {
                OverrideColor = moonSilver
            });
            
            tooltips.Add(new TooltipLine(Mod, "EroicaStats", "+22% melee damage, +18% melee attack speed, +12 melee crit, +10% all damage")
            {
                OverrideColor = eroicaGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "CampanellaStats", "+25% magic damage, +12 magic crit, -15% mana cost")
            {
                OverrideColor = flameOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "EnigmaStats", "+20% all damage, +10 crit chance")
            {
                OverrideColor = greenFlame
            });
            
            tooltips.Add(new TooltipLine(Mod, "SwanStats", "+18% all damage, +10 crit, +25% movement speed")
            {
                OverrideColor = rainbow
            });
            
            tooltips.Add(new TooltipLine(Mod, "Immunities", "Immunity to On Fire! and Burning")
            {
                OverrideColor = flameOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "AllEffects", "Kills: 6-sec Heroic Surge (+30%), 18% Paradox, 15% Bell ring (160 AOE), 15%/12% dodge")
            {
                OverrideColor = harmonyGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'When all five movements converge, the symphony achieves perfect harmony'")
            {
                OverrideColor = new Color(255, 240, 200)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SonatasEmbrace>()
                .AddIngredient<HerosSymphony>()
                .AddIngredient<InfernalVirtuoso>()
                .AddIngredient<RiddleOfTheVoid>()
                .AddIngredient<SwansChromaticDiadem>()
                .AddIngredient<HarmonicCoreOfMoonlightSonata>(50)
                .AddIngredient<HarmonicCoreOfEroica>(50)
                .AddIngredient<HarmonicCoreOfLaCampanella>(50)
                .AddIngredient<HarmonicCoreOfEnigma>(50)
                .AddIngredient<HarmonicCoreOfSwanLake>(50)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class CompleteHarmonyPlayer : ModPlayer
    {
        public bool completeHarmonyEquipped;
        private int heroicSurgeTimer;
        private int invulnFramesOnKill = 90;
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
            completeHarmonyEquipped = false;
        }

        public override void PostUpdate()
        {
            if (heroicSurgeTimer > 0)
            {
                heroicSurgeTimer--;
                Player.GetDamage(DamageClass.Generic) += 0.30f;
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
            HandleHarmonyHit(target, damageDone, true);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.owner == Player.whoAmI)
            {
                HandleHarmonyHit(target, damageDone, DamageClass.Magic.CountsAsClass(proj.DamageType));
            }
        }

        private void HandleHarmonyHit(NPC target, int damageDone, bool isMagic)
        {
            if (!completeHarmonyEquipped) return;
            
            bool isNight = !Main.dayTime;
            
            // Blue fire at night (Moonlight + Campanella)
            if (isNight && isMagic)
            {
                int bonusDamage = (int)(damageDone * 0.20f);
                target.SimpleStrikeNPC(bonusDamage, 0, false, 0, null, false, 0, true);
                
                Color blueFlame = new Color(100, 150, 255);
            }
            
            // Paradox (18%)
            if (Main.rand.NextFloat() < 0.18f)
            {
                int debuffId = ParadoxDebuffs[Main.rand.Next(ParadoxDebuffs.Length)];
                target.AddBuff(debuffId, 360);
                target.AddBuff(BuffID.OnFire, 300);
                
                if (!paradoxStacks.ContainsKey(target.whoAmI))
                    paradoxStacks[target.whoAmI] = 0;
                
                paradoxStacks[target.whoAmI]++;
                paradoxTimers[target.whoAmI] = 420;
                
                int stacks = paradoxStacks[target.whoAmI];
                
                // Five-theme VFX
                Color[] colors = new Color[]
                {
                    MoonlightColors.Purple,
                    EroicaColors.Gold,
                    CampanellaColors.Orange,
                    EnigmaColors.GreenFlame,
                    SwanColors.GetRainbow(Main.rand.NextFloat())
                };
                
                
                
                // Ultimate Harmony Collapse at 5 stacks
                if (stacks >= 5)
                {
                    TriggerHarmonyCollapse(target, damageDone, isNight);
                    paradoxStacks[target.whoAmI] = 0;
                }
            }
            
            // Bell ring (15%)
            if (bellRingCooldown <= 0 && Main.rand.NextFloat() < 0.15f)
            {
                bellRingCooldown = 20;
                target.AddBuff(BuffID.Confused, 150);
                
                Color chimeColor = Color.Lerp(CampanellaColors.Orange, SwanColors.GetRainbow(Main.rand.NextFloat()), 0.5f);
                
                // AOE
                float aoeRadius = 160f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.whoAmI != target.whoAmI && !npc.immortal)
                    {
                        if (Vector2.Distance(npc.Center, target.Center) <= aoeRadius)
                        {
                            int aoeDamage = (int)(damageDone * 0.6f);
                            npc.SimpleStrikeNPC(aoeDamage, 0, false, 0, null, false, 0, true);
                            npc.AddBuff(BuffID.OnFire, 240);
                            npc.AddBuff(ParadoxDebuffs[Main.rand.Next(ParadoxDebuffs.Length)], 180);
                            
                        }
                    }
                }
                
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item35, target.Center);
            }
            
            // Check for kill
            if (target.life <= 0 && !target.immortal)
            {
                TriggerHeroicKill(target);
            }
        }

        private void TriggerHeroicKill(NPC killedTarget)
        {
            Player.immune = true;
            Player.immuneTime = Math.Max(Player.immuneTime, invulnFramesOnKill);
            heroicSurgeTimer = 360;
            
            // Five-theme kill VFX
            
            
            
            
            for (int i = 0; i < 10; i++)
            {
                Color[] colors = new Color[]
                {
                    MoonlightColors.Purple, EroicaColors.Gold,
                    CampanellaColors.Orange, EnigmaColors.GreenFlame, SwanColors.GetRainbow(i / 10f)
                };
            }
        }

        private void TriggerHarmonyCollapse(NPC target, int baseDamage, bool isNight)
        {
            // ULTIMATE HARMONY EXPLOSION
            
            Color[] themeColors = new Color[]
            {
                isNight ? new Color(100, 150, 255) : MoonlightColors.Purple,
                EroicaColors.Gold,
                CampanellaColors.Orange,
                EnigmaColors.GreenFlame,
                SwanColors.GetRainbow(0f)
            };
            
            
            // 15 halos cycling through all themes
            
            
            // Feather explosion
            
            // Glyph spiral
            
            // Eye formation
            
            
            // Rainbow sparkle explosion
            
            // MASSIVE damage
            if (Main.myPlayer == Player.whoAmI)
            {
                int harmonyDamage = (int)(baseDamage * 4.0f);
                target.SimpleStrikeNPC(harmonyDamage, 0, false, 0, null, false, 0, true);
                
                float aoeRadius = 300f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.whoAmI != target.whoAmI && !npc.immortal)
                    {
                        if (Vector2.Distance(npc.Center, target.Center) <= aoeRadius)
                        {
                            npc.SimpleStrikeNPC(harmonyDamage / 2, 0, false, 0, null, false, 0, true);
                            npc.AddBuff(BuffID.OnFire, 420);
                            npc.AddBuff(ParadoxDebuffs[Main.rand.Next(ParadoxDebuffs.Length)], 300);
                            
                        }
                    }
                }
            }
            
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.4f, Volume = 1.5f }, target.Center);
        }

        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (!completeHarmonyEquipped) return false;
            if (dodgeCooldown > 0) return false;
            
            bool isNight = !Main.dayTime;
            float dodgeChance = isNight ? 0.15f : 0.12f;
            
            if (Main.rand.NextFloat() < dodgeChance)
            {
                dodgeCooldown = 70;
                TriggerHarmonyDodge();
                return true;
            }
            
            return false;
        }

        private void TriggerHarmonyDodge()
        {
            // Ultimate harmony dodge
            
            Color[] themeColors = new Color[]
            {
                MoonlightColors.Purple,
                EroicaColors.Gold,
                CampanellaColors.Orange,
                EnigmaColors.GreenFlame,
                SwanColors.GetRainbow(0f)
            };
            
            
            
            
            
            
            // Deal dodge damage to nearby enemies
            if (Main.myPlayer == Player.whoAmI)
            {
                int dodgeDamage = 150 + (int)(Player.GetTotalDamage(DamageClass.Generic).ApplyTo(100) * 0.3f);
                float damageRadius = 200f;
                
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && !npc.immortal && !npc.dontTakeDamage)
                    {
                        if (Vector2.Distance(npc.Center, Player.Center) <= damageRadius)
                        {
                            npc.SimpleStrikeNPC(dodgeDamage, 0, false, 0, null, false, 0, true);
                        }
                    }
                }
            }
            
            Player.immune = true;
            Player.immuneTime = 35;
            
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 1.3f }, Player.Center);
        }
    }
    #endregion
}


