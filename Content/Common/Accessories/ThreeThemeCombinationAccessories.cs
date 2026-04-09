using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
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
using MagnumOpus.Content.SwanLake.Debuffs;

namespace MagnumOpus.Content.Common.Accessories
{
    #region Trinity of Night - Moonlight + La Campanella + Enigma
    /// <summary>
    /// Three-Theme Combination: Moonlight Sonata + La Campanella + Enigma Variations
    /// From Sonata: +15% damage at night, +10% during day, -12% mana cost
    /// From Infernal Virtuoso: Fire/lava immunity, 8% Tolling Death on any weapon hits
    /// From Riddle of the Void: +15% all damage, 10% Paradox on any weapon hit
    /// Signature: "Nocturnal Trinity" — rotating 8s phases (Moon/Bell/Void), 50% stronger at night
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

            // From Sonata's Embrace: +15% damage at night, +10% during day, -12% mana cost
            if (isNight)
                player.GetDamage(DamageClass.Generic) += 0.15f;
            else
                player.GetDamage(DamageClass.Generic) += 0.10f;
            player.manaCost -= 0.12f;

            // From Infernal Virtuoso: fire/lava immunity (Tolling Death handled in OnHit)
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.lavaImmune = true;

            // From Riddle of the Void: +15% all damage (Paradox handled in OnHit)
            player.GetDamage(DamageClass.Generic) += 0.15f;

            // Nocturnal Trinity phase bonuses applied in PostUpdate
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color darkPurple = new Color(100, 50, 150);
            Color moonBlue = MoonlightColors.Purple;
            Color flameOrange = new Color(255, 140, 40);

            tooltips.Add(new TooltipLine(Mod, "Combo", "Combines: Moonlight Sonata + La Campanella + Enigma Variations")
            {
                OverrideColor = darkPurple
            });

            tooltips.Add(new TooltipLine(Mod, "SonataStats", "+15% damage at night, +10% during the day, -12% mana cost")
            {
                OverrideColor = moonBlue
            });

            tooltips.Add(new TooltipLine(Mod, "CampanellaStats", "Immunity to fire debuffs and lava, 8% Tolling Death on any weapon hit")
            {
                OverrideColor = flameOrange
            });

            tooltips.Add(new TooltipLine(Mod, "EnigmaStats", "+15% all damage, 10% Paradox on any weapon hit")
            {
                OverrideColor = EnigmaColors.GreenFlame
            });

            tooltips.Add(new TooltipLine(Mod, "Signature", "Nocturnal Trinity: Rotating 8-second empowerment phases")
            {
                OverrideColor = darkPurple
            });

            tooltips.Add(new TooltipLine(Mod, "Phase1", "Moon Phase: Magic costs no mana, +25% magic damage")
            {
                OverrideColor = moonBlue
            });

            tooltips.Add(new TooltipLine(Mod, "Phase2", "Bell Phase: All attacks echo 30% damage as fire, +2 minion slots")
            {
                OverrideColor = flameOrange
            });

            tooltips.Add(new TooltipLine(Mod, "Phase3", "Void Phase: Every hit applies Paradox, +20% all damage")
            {
                OverrideColor = EnigmaColors.GreenFlame
            });

            tooltips.Add(new TooltipLine(Mod, "NightBonus", "At night: All phases are 50% stronger")
            {
                OverrideColor = moonBlue
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Night speaks in three voices — the moon whispers, the bell tolls, and the void answers'")
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
        private int phaseTimer;
        private int currentPhase; // 0=Moon, 1=Bell, 2=Void

        private static readonly int PhaseDuration = 480; // 8 seconds at 60fps

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
            if (!trinityEquipped) return;

            bool isNight = !Main.dayTime;
            float nightMult = isNight ? 1.5f : 1.0f;

            phaseTimer++;
            if (phaseTimer >= PhaseDuration)
            {
                phaseTimer = 0;
                currentPhase = (currentPhase + 1) % 3;
            }

            switch (currentPhase)
            {
                case 0: // Moon Phase: free mana + magic damage
                    Player.manaCost -= 1.0f; // effectively free (stacks with -12% from base)
                    Player.GetDamage(DamageClass.Magic) += 0.25f * nightMult;
                    break;
                case 1: // Bell Phase: +2 minion slots (fire echo handled in OnHit)
                    Player.maxMinions += 2;
                    break;
                case 2: // Void Phase: +20% all damage (guaranteed Paradox handled in OnHit)
                    Player.GetDamage(DamageClass.Generic) += 0.20f * nightMult;
                    break;
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!trinityEquipped) return;
            HandleTrinityHit(target, damageDone, null);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!trinityEquipped || proj.owner != Player.whoAmI) return;
            HandleTrinityHit(target, damageDone, proj);
        }

        private void HandleTrinityHit(NPC target, int damageDone, Projectile proj)
        {
            bool isNight = !Main.dayTime;
            float nightMult = isNight ? 1.5f : 1.0f;

            // 8% Tolling Death on any weapon hit (from Infernal Virtuoso)
            if (Main.rand.NextFloat() < 0.08f)
            {
                int secondStrike = (int)(damageDone * 0.75f);
                if (secondStrike > 0 && Main.myPlayer == Player.whoAmI)
                    target.SimpleStrikeNPC(secondStrike, 0, false, 0, null, false, 0, true);
                target.AddBuff(BuffID.WitheredWeapon, 180);
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.5f, Volume = 0.6f }, target.Center);
            }

            // 10% Paradox on hit (from Riddle of the Void) — always during Void Phase
            bool applyParadox = currentPhase == 2 || Main.rand.NextFloat() < 0.10f;
            if (applyParadox)
            {
                int debuffId = ParadoxDebuffs[Main.rand.Next(ParadoxDebuffs.Length)];
                target.AddBuff(debuffId, 300);
            }

            // Bell Phase: 30% fire echo
            if (currentPhase == 1 && Main.myPlayer == Player.whoAmI)
            {
                int echoDmg = (int)(damageDone * 0.30f * nightMult);
                if (echoDmg > 0)
                {
                    target.SimpleStrikeNPC(echoDmg, 0, false, 0, null, false, 0, true);
                    target.AddBuff(BuffID.OnFire, 180);
                }
            }
        }
    }
    #endregion

    #region Adagio of Radiant Valor - Eroica + Moonlight + Swan Lake
    /// <summary>
    /// Three-Theme Combination: Eroica + Moonlight Sonata + Swan Lake
    /// From Hero's Symphony: 20% chance for melee double damage, +15% melee speed
    /// From Sonata: +15% damage at night, +18% crit at night
    /// From Swan's Diadem: +25% movement speed, Dying Swan's Grace (airborne weapon buff)
    /// Signature: "Radiant Crescendo" — consecutive hit stacking (max 20), tiered bonuses, Fortissimo burst
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
            modPlayer.adagioEquipped = true;

            bool isNight = !Main.dayTime;

            // From Hero's Symphony: +15% melee speed (double damage handled in ModifyHitNPC)
            player.GetAttackSpeed(DamageClass.Melee) += 0.15f;

            // From Sonata: +15% damage at night, +18% crit at night
            if (isNight)
            {
                player.GetDamage(DamageClass.Generic) += 0.15f;
                player.GetCritChance(DamageClass.Generic) += 18;
            }

            // From Swan's Diadem: +25% movement speed, Dying Swan's Grace (airborne weapon buff)
            player.moveSpeed += 0.25f;
            player.runAcceleration *= 1.25f;
            bool airborne = player.velocity.Y != 0 && !player.mount.Active;
            if (airborne)
            {
                player.GetDamage(DamageClass.Generic) += 0.08f;
            }

            // Radiant Crescendo attack speed bonus at 5+ stacks
            if (modPlayer.crescendoStacks >= 5)
                player.GetAttackSpeed(DamageClass.Generic) += 0.10f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color gold = new Color(255, 200, 80);
            Color moonSilver = MoonlightColors.Purple;
            Color rainbow = SwanColors.GetRainbow((float)(Main.GameUpdateCount % 300) / 300f);

            tooltips.Add(new TooltipLine(Mod, "Combo", "Combines: Eroica + Moonlight Sonata + Swan Lake")
            {
                OverrideColor = gold
            });

            tooltips.Add(new TooltipLine(Mod, "EroicaStats", "20% melee double damage chance, +15% melee attack speed")
            {
                OverrideColor = EroicaColors.Gold
            });

            tooltips.Add(new TooltipLine(Mod, "MoonlightStats", "+15% damage at night, +18 crit chance at night")
            {
                OverrideColor = moonSilver
            });

            tooltips.Add(new TooltipLine(Mod, "SwanStats", "+25% movement speed, Dying Swan's Grace when airborne")
            {
                OverrideColor = rainbow
            });

            tooltips.Add(new TooltipLine(Mod, "Signature", "Radiant Crescendo: Consecutive hits on same target build stacks (max 20)")
            {
                OverrideColor = gold
            });

            tooltips.Add(new TooltipLine(Mod, "Tier1", "5 stacks: +10% attack speed")
            {
                OverrideColor = gold
            });

            tooltips.Add(new TooltipLine(Mod, "Tier2", "10 stacks: Hits spawn moonlit sparkles (25% weapon damage AOE)")
            {
                OverrideColor = moonSilver
            });

            tooltips.Add(new TooltipLine(Mod, "Tier3", "15 stacks: Each hit heals 1% max HP")
            {
                OverrideColor = rainbow
            });

            tooltips.Add(new TooltipLine(Mod, "Tier4", "20 stacks: Fortissimo — next hit deals 5x damage, resets stacks")
            {
                OverrideColor = new Color(255, 240, 150)
            });

            tooltips.Add(new TooltipLine(Mod, "NightBonus", "At night: Stacks build 50% faster")
            {
                OverrideColor = moonSilver
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'The adagio swells — hero, moon, and swan rising together toward a single, radiant note'")
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
        public bool adagioEquipped;
        public int crescendoStacks;
        private int crescendoTarget = -1;
        private int crescendoDecayTimer;
        private static readonly int DecayTime = 180; // 3 seconds at 60fps

        public override void ResetEffects()
        {
            adagioEquipped = false;
        }

        public override void PostUpdate()
        {
            if (!adagioEquipped)
            {
                crescendoStacks = 0;
                crescendoTarget = -1;
                return;
            }

            if (crescendoDecayTimer > 0)
            {
                crescendoDecayTimer--;
                if (crescendoDecayTimer <= 0)
                {
                    crescendoStacks = 0;
                    crescendoTarget = -1;
                }
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!adagioEquipped) return;

            // 20% melee double damage (from Hero's Symphony)
            if (Player.HeldItem != null && Player.HeldItem.DamageType.CountsAsClass(DamageClass.Melee))
            {
                if (Main.rand.NextFloat() < 0.20f)
                    modifiers.FinalDamage *= 2f;
            }

            // Fortissimo at 20 stacks
            if (crescendoStacks >= 20 && target.whoAmI == crescendoTarget)
                modifiers.FinalDamage *= 5f;
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!adagioEquipped) return;
            TryApplyDyingSwanGrace(target);
            HandleCrescendoHit(target, damageDone);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!adagioEquipped || proj.owner != Player.whoAmI) return;
            TryApplyDyingSwanGrace(target);
            HandleCrescendoHit(target, damageDone);
        }

        private void TryApplyDyingSwanGrace(NPC target)
        {
            bool airborne = Player.velocity.Y != 0 && !Player.mount.Active;
            if (!airborne) return;
            if (target.HasBuff(ModContent.BuffType<OdilesBeauty>())) return;

            target.AddBuff(ModContent.BuffType<OdilesBeauty>(), 300);
            int weaponDamage = Player.HeldItem != null
                ? (int)Player.GetTotalDamage(Player.HeldItem.DamageType).ApplyTo(Player.HeldItem.damage)
                : 50;
            target.GetGlobalNPC<OdilesBeautyNPC>().SetDamage(weaponDamage);
        }

        private void HandleCrescendoHit(NPC target, int damageDone)
        {
            bool isNight = !Main.dayTime;

            // Track target — switch target resets stacks
            if (crescendoTarget != target.whoAmI)
            {
                crescendoStacks = 0;
                crescendoTarget = target.whoAmI;
            }

            // Check for Fortissimo reset BEFORE incrementing (the 5x was applied in ModifyHit)
            if (crescendoStacks >= 20)
            {
                crescendoStacks = 0;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 1.2f }, target.Center);
                return;
            }

            // Build stacks — night: 50% faster (every 2nd hit adds 2 instead of 1)
            int stackGain = 1;
            if (isNight && crescendoStacks % 2 == 0)
                stackGain = 2;
            crescendoStacks = Math.Min(crescendoStacks + stackGain, 20);
            crescendoDecayTimer = DecayTime;

            // Tier 2 (10+ stacks): moonlit sparkle AOE — 25% weapon damage to nearby enemies
            if (crescendoStacks >= 10 && Main.myPlayer == Player.whoAmI)
            {
                int sparkleDmg = (int)(damageDone * 0.25f);
                if (sparkleDmg > 0)
                {
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (npc.active && !npc.friendly && !npc.immortal && npc.whoAmI != target.whoAmI)
                        {
                            if (Vector2.Distance(npc.Center, target.Center) <= 150f)
                                npc.SimpleStrikeNPC(sparkleDmg, 0, false, 0, null, false, 0, true);
                        }
                    }
                }
            }

            // Tier 3 (15+ stacks): heal 1% max HP per hit
            if (crescendoStacks >= 15)
            {
                int healAmount = Math.Max(1, Player.statLifeMax2 / 100);
                Player.statLife = Math.Min(Player.statLife + healAmount, Player.statLifeMax2);
                Player.HealEffect(healAmount);
            }
        }
    }
    #endregion

    #region Requiem of the Enigmatic Flame - La Campanella + Enigma + Swan Lake
    /// <summary>
    /// Three-Theme Combination: La Campanella + Enigma Variations + Swan Lake
    /// From Infernal Virtuoso: Fire/lava immunity, 8% Tolling Death on any weapon hits
    /// From Riddle of the Void: +15% all damage, 10% Paradox on hit
    /// From Swan's Diadem: +25% movement speed, damage buff effectiveness +50%
    /// Signature: "Requiem's Judgment" — enemies with both Paradox + Tolling Death get Requiem's Mark;
    ///   +15% damage from all sources for 8s (does not stack)
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
            modPlayer.requiemEquipped = true;

            // From Infernal Virtuoso: fire/lava immunity (Tolling Death handled in OnHit)
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.lavaImmune = true;

            // From Riddle of the Void: +15% all damage (Paradox handled in OnHit)
            player.GetDamage(DamageClass.Generic) += 0.15f;

            // From Swan's Diadem: +25% movement speed, damage buff effectiveness +50%
            player.moveSpeed += 0.25f;
            player.runAcceleration *= 1.25f;
            // Approximate +50% buff effectiveness as flat damage (parent uses +18% for +80%)
            player.GetDamage(DamageClass.Generic) += 0.11f;
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

            tooltips.Add(new TooltipLine(Mod, "CampanellaStats", "Immunity to fire debuffs and lava, 8% Tolling Death on any weapon hit")
            {
                OverrideColor = flameOrange
            });

            tooltips.Add(new TooltipLine(Mod, "EnigmaStats", "+15% all damage, 10% chance on hit to apply Paradox debuff")
            {
                OverrideColor = greenFlame
            });

            tooltips.Add(new TooltipLine(Mod, "SwanStats", "+25% movement speed, damage buff effectiveness +50%")
            {
                OverrideColor = rainbow
            });

            tooltips.Add(new TooltipLine(Mod, "Signature", "Requiem's Judgment: Enemies with both Paradox and Tolling Death gain Requiem's Mark")
            {
                OverrideColor = new Color(200, 100, 50)
            });

            tooltips.Add(new TooltipLine(Mod, "MarkEffect", "Requiem's Mark: Enemy takes 15% more damage from all sources for 8 seconds (does not stack)")
            {
                OverrideColor = new Color(200, 100, 50)
            });

            tooltips.Add(new TooltipLine(Mod, "BuffBoost", "Swan Lake's damage buff effectiveness makes all combined buffs 50% more potent")
            {
                OverrideColor = rainbow
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'The requiem plays its final verse — fire, mystery, and grace united in one terrible prayer'")
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
        public bool requiemEquipped;

        // Track which NPCs have been hit by Paradox and Tolling Death from this accessory
        private HashSet<int> paradoxHitTargets = new HashSet<int>();
        private HashSet<int> tollingDeathHitTargets = new HashSet<int>();

        // Track Requiem's Mark — NPC index → timer (frames remaining)
        private Dictionary<int, int> requiemMarkTimers = new Dictionary<int, int>();

        private static readonly int MarkDuration = 480; // 8 seconds at 60fps

        private static readonly int[] ParadoxDebuffs = new int[]
        {
            BuffID.Confused, BuffID.Slow, BuffID.CursedInferno,
            BuffID.Ichor, BuffID.ShadowFlame, BuffID.Frostburn
        };

        public override void ResetEffects()
        {
            requiemEquipped = false;
        }

        public override void PostUpdate()
        {
            if (!requiemEquipped)
            {
                paradoxHitTargets.Clear();
                tollingDeathHitTargets.Clear();
                requiemMarkTimers.Clear();
                return;
            }

            // Decay mark timers
            var expired = requiemMarkTimers.Where(kvp => kvp.Value <= 0).Select(kvp => kvp.Key).ToList();
            foreach (int key in expired)
                requiemMarkTimers.Remove(key);

            foreach (int key in requiemMarkTimers.Keys.ToList())
                requiemMarkTimers[key]--;

            // Clean up stale NPC references
            paradoxHitTargets.RemoveWhere(id => id < 0 || id >= Main.maxNPCs || !Main.npc[id].active);
            tollingDeathHitTargets.RemoveWhere(id => id < 0 || id >= Main.maxNPCs || !Main.npc[id].active);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!requiemEquipped) return;

            // Requiem's Mark: +15% damage taken, Swan buff effectiveness +50% = +22.5% total
            if (requiemMarkTimers.ContainsKey(target.whoAmI))
                modifiers.FinalDamage *= 1.225f;
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!requiemEquipped) return;
            HandleRequiemHit(target, damageDone, null);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!requiemEquipped || proj.owner != Player.whoAmI) return;
            HandleRequiemHit(target, damageDone, proj);
        }

        private void HandleRequiemHit(NPC target, int damageDone, Projectile proj)
        {
            // 8% Tolling Death on any weapon hit (from Infernal Virtuoso)
            if (Main.rand.NextFloat() < 0.08f)
            {
                int secondStrike = (int)(damageDone * 0.75f);
                if (secondStrike > 0 && Main.myPlayer == Player.whoAmI)
                    target.SimpleStrikeNPC(secondStrike, 0, false, 0, null, false, 0, true);
                target.AddBuff(BuffID.WitheredWeapon, 180);
                tollingDeathHitTargets.Add(target.whoAmI);
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.5f, Volume = 0.6f }, target.Center);
            }

            // 10% Paradox on hit (from Riddle of the Void)
            if (Main.rand.NextFloat() < 0.10f)
            {
                int debuffId = ParadoxDebuffs[Main.rand.Next(ParadoxDebuffs.Length)];
                target.AddBuff(debuffId, 300);
                paradoxHitTargets.Add(target.whoAmI);
            }

            // Check for Requiem's Mark application — both Paradox AND Tolling Death on target (non-stacking)
            if (paradoxHitTargets.Contains(target.whoAmI) && tollingDeathHitTargets.Contains(target.whoAmI))
            {
                if (!requiemMarkTimers.ContainsKey(target.whoAmI))
                {
                    requiemMarkTimers[target.whoAmI] = MarkDuration;
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.3f, Volume = 0.8f }, target.Center);
                }
                // Does not stack — do not refresh timer if already marked
            }
        }
    }
    #endregion

    #region Complete Harmony - All 5 Themes
    /// <summary>
    /// All 5 Themes Combined: Moonlight Sonata + Eroica + La Campanella + Enigma Variations + Swan Lake
    /// From Sonata's Embrace: +15% damage at night, +10% day, -12% mana cost, magic Moonstruck
    /// From Hero's Symphony: 20% melee double damage, +15% melee speed, kills → Heroic Surge (+25% 5s)
    /// From Infernal Virtuoso: Fire/lava immunity, 8% Tolling Death on any hit, +1 minion slot
    /// From Riddle of the Void: +15% all damage, 10% Paradox on hit
    /// From Swan's Chromatic Diadem: +25% movement speed, +80% buff effectiveness, Dying Swan's Grace
    /// Signature: "Harmonic Convergence" — theme proc stacking (Harmonic Resonance) + Dissonance enemy debuff
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

            // === FROM SONATA'S EMBRACE ===
            if (isNight)
                player.GetDamage(DamageClass.Generic) += 0.15f;
            else
                player.GetDamage(DamageClass.Generic) += 0.10f;
            player.manaCost -= 0.12f;

            // === FROM HERO'S SYMPHONY ===
            player.GetAttackSpeed(DamageClass.Melee) += 0.15f;

            // === FROM INFERNAL VIRTUOSO ===
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.lavaImmune = true;
            player.maxMinions += 1;

            // === FROM RIDDLE OF THE VOID ===
            player.GetDamage(DamageClass.Generic) += 0.15f;

            // === FROM SWAN'S CHROMATIC DIADEM ===
            player.moveSpeed += 0.25f;
            player.runAcceleration *= 1.25f;
            // +80% buff effectiveness approximated as flat damage
            player.GetDamage(DamageClass.Generic) += 0.14f;

            // === HARMONIC RESONANCE TIER BONUSES ===
            if (modPlayer.harmonicResonanceStacks >= 1)
                player.endurance += 0.05f;
            if (modPlayer.harmonicResonanceStacks >= 3)
            {
                player.GetAttackSpeed(DamageClass.Generic) += 0.10f;
                player.lifeRegen += 5;
            }

            // Full Harmony buff
            if (modPlayer.fullHarmonyTimer > 0)
            {
                player.GetDamage(DamageClass.Generic) += 0.25f;
            }

            // Heroic Surge buff
            if (modPlayer.heroicSurgeTimer > 0)
                player.GetDamage(DamageClass.Generic) += 0.25f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color harmonyGold = new Color(255, 220, 100);
            Color moonSilver = new Color(150, 170, 220);
            Color eroicaGold = new Color(255, 200, 80);
            Color flameOrange = new Color(255, 140, 40);
            Color greenFlame = EnigmaColors.GreenFlame;
            Color rainbow = SwanColors.GetRainbow((float)(Main.GameUpdateCount % 300) / 300f);

            tooltips.Add(new TooltipLine(Mod, "Ultimate", "The complete harmony of all five themes")
            {
                OverrideColor = harmonyGold
            });

            tooltips.Add(new TooltipLine(Mod, "MoonlightStats", "+15% damage at night, +10% during the day, -12% mana cost, magic attacks inflict Moonstruck")
            {
                OverrideColor = moonSilver
            });

            tooltips.Add(new TooltipLine(Mod, "EroicaStats", "20% chance for melee attacks to deal double damage, +15% melee speed, kills trigger Heroic Surge (+25% damage, 5s)")
            {
                OverrideColor = eroicaGold
            });

            tooltips.Add(new TooltipLine(Mod, "CampanellaStats", "Immunity to fire debuffs and lava, 8% Tolling Death on any weapon hit, +1 minion slot")
            {
                OverrideColor = flameOrange
            });

            tooltips.Add(new TooltipLine(Mod, "EnigmaStats", "+15% all damage, 10% Paradox on hit")
            {
                OverrideColor = greenFlame
            });

            tooltips.Add(new TooltipLine(Mod, "SwanStats", "+25% movement speed, damage buff effectiveness +80%, Dying Swan's Grace airborne debuff")
            {
                OverrideColor = rainbow
            });

            tooltips.Add(new TooltipLine(Mod, "Signature", "Harmonic Convergence: Theme procs build Harmonic Resonance stacks (max 5)")
            {
                OverrideColor = harmonyGold
            });

            tooltips.Add(new TooltipLine(Mod, "Tier1", "1 stack: +5% damage reduction")
            {
                OverrideColor = harmonyGold
            });

            tooltips.Add(new TooltipLine(Mod, "Tier2", "3 stacks: +10% attack speed, +5 life regen")
            {
                OverrideColor = harmonyGold
            });

            tooltips.Add(new TooltipLine(Mod, "Tier3", "5 stacks: Full Harmony for 8s — +25% damage, +15% dodge, 0.5% max HP heal per hit")
            {
                OverrideColor = harmonyGold
            });

            tooltips.Add(new TooltipLine(Mod, "Dissonance", "Dissonance: Enemies with 3+ theme debuffs take 20% more damage for 8s (does not stack)")
            {
                OverrideColor = new Color(200, 100, 100)
            });

            tooltips.Add(new TooltipLine(Mod, "NightBonus", "At night: Harmonic Resonance decays slower (6s instead of 4s)")
            {
                OverrideColor = moonSilver
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Five voices. One harmony. The opus is complete.'")
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
        public int harmonicResonanceStacks;
        public int heroicSurgeTimer;
        public int fullHarmonyTimer;

        private int resonanceDecayTimer;
        private int fullHarmonyDodgeCooldown;

        // Track theme debuffs on enemies for Dissonance: NPC index → set of theme tags
        private Dictionary<int, HashSet<string>> enemyThemeDebuffs = new Dictionary<int, HashSet<string>>();
        // Dissonance timers: NPC index → frames remaining
        private Dictionary<int, int> dissonanceTimers = new Dictionary<int, int>();
        // Odile's Beauty cooldown per NPC (cannot reapply while active)
        private Dictionary<int, int> odilesBeautyCooldowns = new Dictionary<int, int>();

        private static readonly int FullHarmonyDuration = 480; // 8 seconds
        private static readonly int DissonanceDuration = 480; // 8 seconds

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
            if (!completeHarmonyEquipped)
            {
                harmonicResonanceStacks = 0;
                heroicSurgeTimer = 0;
                fullHarmonyTimer = 0;
                resonanceDecayTimer = 0;
                enemyThemeDebuffs.Clear();
                dissonanceTimers.Clear();
                odilesBeautyCooldowns.Clear();
                return;
            }

            bool isNight = !Main.dayTime;
            int decayInterval = isNight ? 360 : 240; // 6s night, 4s day

            // Decay Harmonic Resonance stacks
            if (harmonicResonanceStacks > 0)
            {
                resonanceDecayTimer++;
                if (resonanceDecayTimer >= decayInterval)
                {
                    harmonicResonanceStacks--;
                    resonanceDecayTimer = 0;
                }
            }

            // Decay heroic surge
            if (heroicSurgeTimer > 0) heroicSurgeTimer--;

            // Decay Full Harmony
            if (fullHarmonyTimer > 0) fullHarmonyTimer--;
            if (fullHarmonyDodgeCooldown > 0) fullHarmonyDodgeCooldown--;

            // Decay Dissonance timers
            var expiredDissonance = dissonanceTimers.Where(kvp => kvp.Value <= 0).Select(kvp => kvp.Key).ToList();
            foreach (int key in expiredDissonance)
                dissonanceTimers.Remove(key);
            foreach (int key in dissonanceTimers.Keys.ToList())
                dissonanceTimers[key]--;

            // Decay Odile's Beauty cooldowns
            var expiredOdiles = odilesBeautyCooldowns.Where(kvp => kvp.Value <= 0).Select(kvp => kvp.Key).ToList();
            foreach (int key in expiredOdiles)
                odilesBeautyCooldowns.Remove(key);
            foreach (int key in odilesBeautyCooldowns.Keys.ToList())
                odilesBeautyCooldowns[key]--;

            // Clean stale NPC entries
            enemyThemeDebuffs = enemyThemeDebuffs
                .Where(kvp => kvp.Key >= 0 && kvp.Key < Main.maxNPCs && Main.npc[kvp.Key].active)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!completeHarmonyEquipped) return;

            // Eroica: 20% melee double damage
            if (modifiers.DamageType == DamageClass.Melee && Main.rand.NextFloat() < 0.20f)
                modifiers.FinalDamage *= 2f;

            // Dissonance: +20% damage on enemies with 3+ theme debuffs
            if (dissonanceTimers.ContainsKey(target.whoAmI))
                modifiers.FinalDamage *= 1.20f;
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!completeHarmonyEquipped) return;
            HandleHarmonyHit(target, damageDone, null, item.DamageType);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!completeHarmonyEquipped || proj.owner != Player.whoAmI) return;
            HandleHarmonyHit(target, damageDone, proj, proj.DamageType);
        }

        private void HandleHarmonyHit(NPC target, int damageDone, Projectile proj, DamageClass damageType)
        {
            bool isMagic = damageType.CountsAsClass(DamageClass.Magic);

            // === MOONSTRUCK (magic attacks) ===
            if (isMagic)
            {
                target.AddBuff(BuffID.Slow, 180);
                target.AddBuff(BuffID.Ichor, 120);
                TrackThemeDebuff(target.whoAmI, "Moonstruck");
                AddResonanceStack();
            }

            // === TOLLING DEATH (8% any weapon hit) ===
            if (Main.rand.NextFloat() < 0.08f)
            {
                int secondStrike = (int)(damageDone * 0.75f);
                if (secondStrike > 0 && Main.myPlayer == Player.whoAmI)
                    target.SimpleStrikeNPC(secondStrike, 0, false, 0, null, false, 0, true);
                target.AddBuff(BuffID.WitheredWeapon, 180);
                TrackThemeDebuff(target.whoAmI, "TollingDeath");
                AddResonanceStack();
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.5f, Volume = 0.6f }, target.Center);
            }

            // === PARADOX (10% any hit) ===
            if (Main.rand.NextFloat() < 0.10f)
            {
                int debuffId = ParadoxDebuffs[Main.rand.Next(ParadoxDebuffs.Length)];
                target.AddBuff(debuffId, 300);
                TrackThemeDebuff(target.whoAmI, "Paradox");
                AddResonanceStack();
            }

            // === DYING SWAN'S GRACE (Odile's Beauty while airborne) ===
            if (Player.velocity.Y != 0 || Player.wingTime > 0)
            {
                if (!odilesBeautyCooldowns.ContainsKey(target.whoAmI))
                {
                    target.AddBuff(ModContent.BuffType<OdilesBeauty>(), 300);
                    odilesBeautyCooldowns[target.whoAmI] = 300; // 5s cooldown matching debuff duration
                    TrackThemeDebuff(target.whoAmI, "OdilesBeauty");
                    AddResonanceStack();
                }
            }

            // === CHECK FOR DISSONANCE ===
            CheckDissonance(target.whoAmI);

            // === FULL HARMONY HEAL ===
            if (fullHarmonyTimer > 0)
            {
                int healAmount = Player.statLifeMax2 / 200; // 0.5% max HP
                if (healAmount < 1) healAmount = 1;
                Player.Heal(healAmount);
            }

            // === CHECK FOR KILL → HEROIC SURGE ===
            if (target.life <= 0 && !target.immortal)
            {
                heroicSurgeTimer = 300; // 5 seconds
                AddResonanceStack();
            }
        }

        private void AddResonanceStack()
        {
            resonanceDecayTimer = 0; // Reset decay timer on proc
            harmonicResonanceStacks++;
            if (harmonicResonanceStacks >= 5)
            {
                // Trigger Full Harmony
                fullHarmonyTimer = FullHarmonyDuration;
                harmonicResonanceStacks = 0;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 0.8f }, Player.Center);
            }
            else if (harmonicResonanceStacks > 5)
            {
                harmonicResonanceStacks = 5;
            }
        }

        private void TrackThemeDebuff(int npcIndex, string themeTag)
        {
            if (!enemyThemeDebuffs.ContainsKey(npcIndex))
                enemyThemeDebuffs[npcIndex] = new HashSet<string>();
            enemyThemeDebuffs[npcIndex].Add(themeTag);
        }

        private void CheckDissonance(int npcIndex)
        {
            if (!enemyThemeDebuffs.ContainsKey(npcIndex)) return;
            if (enemyThemeDebuffs[npcIndex].Count >= 3 && !dissonanceTimers.ContainsKey(npcIndex))
            {
                dissonanceTimers[npcIndex] = DissonanceDuration;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.3f, Volume = 0.7f }, Main.npc[npcIndex].Center);
            }
            else if (enemyThemeDebuffs[npcIndex].Count >= 3 && dissonanceTimers.ContainsKey(npcIndex))
            {
                dissonanceTimers[npcIndex] = DissonanceDuration; // Refresh timer
            }
        }

        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (!completeHarmonyEquipped) return false;
            if (fullHarmonyTimer <= 0 || fullHarmonyDodgeCooldown > 0) return false;

            // Full Harmony: 15% dodge
            if (Main.rand.NextFloat() < 0.15f)
            {
                fullHarmonyDodgeCooldown = 60;
                Player.immune = true;
                Player.immuneTime = 30;
                return true;
            }

            return false;
        }
    }
    #endregion
}
