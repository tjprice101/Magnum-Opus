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
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.LaCampanella.Accessories;
using MagnumOpus.Content.LaCampanella.HarmonicCores;
using MagnumOpus.Content.EnigmaVariations.Accessories;
using MagnumOpus.Content.EnigmaVariations.ResonanceEnergies;
using MagnumOpus.Content.Spring.Accessories;
using MagnumOpus.Content.Spring.Materials;
using MagnumOpus.Content.Summer.Accessories;
using MagnumOpus.Content.Summer.Materials;
using MagnumOpus.Content.Winter.Accessories;
using MagnumOpus.Content.Winter.Materials;

namespace MagnumOpus.Content.Common.Accessories
{
    #region Spring's Moonlit Garden - Spring + Moonlight Sonata
    /// <summary>
    /// Season-Theme Hybrid: Bloom Crest + Sonata's Embrace
    /// From Bloom Crest: +6 life regen, +8 defense, +6% damage reduction, 60% thorns
    /// From Sonata's Embrace: +15% damage at night, +10% during the day, -12% mana cost
    /// Signature: "Moonlit Garden" — 12% Withered Root on hit (-10 def, slow), night heal,
    ///   kills during Withered Root → Verdant Renewal (stacking regen),
    ///   Withered Root + Moonstruck = +10% damage taken for 4s
    /// </summary>
    public class SpringsMoonlitGarden : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 1, gold: 50);
            Item.rare = ModContent.RarityType<MoonlightSonataRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<SpringsMoonlitGardenPlayer>();
            modPlayer.moonlitGardenEquipped = true;

            bool isNight = !Main.dayTime;

            // === FROM BLOOM CREST ===
            player.lifeRegen += 6;
            player.statDefense += 8;
            player.endurance += 0.06f;
            player.thorns = 0.6f;

            // === FROM SONATA'S EMBRACE ===
            if (isNight)
                player.GetDamage(DamageClass.Generic) += 0.15f;
            else
                player.GetDamage(DamageClass.Generic) += 0.10f;
            player.manaCost -= 0.12f;

            // === NIGHT BONUS ===
            if (isNight)
                player.lifeRegen += 8;

            // === VERDANT RENEWAL BUFF ===
            if (modPlayer.verdantRenewalStacks > 0)
                player.lifeRegen += 4 * modPlayer.verdantRenewalStacks;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<BloomCrest>()
                .AddIngredient<SonatasEmbrace>()
                .AddIngredient<VernalBar>(15)
                .AddIngredient<ResonantCoreOfMoonlightSonata>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color springPink = new Color(255, 183, 197);
            Color moonPurple = new Color(140, 100, 200);

            tooltips.Add(new TooltipLine(Mod, "Hybrid", "Bloom Crest + Sonata's Embrace")
            {
                OverrideColor = Color.Lerp(springPink, moonPurple, 0.5f)
            });
            tooltips.Add(new TooltipLine(Mod, "SpringStats", "+6 life regen, +8 defense, +6% damage reduction, 60% thorns")
            {
                OverrideColor = springPink
            });
            tooltips.Add(new TooltipLine(Mod, "MoonlightStats", "+15% damage at night, +10% during the day, -12% mana cost")
            {
                OverrideColor = moonPurple
            });
            tooltips.Add(new TooltipLine(Mod, "Signature", "Moonlit Garden: 12% chance on hit to apply Withered Root (slowed, -15 defense, 4s)")
            {
                OverrideColor = Color.Lerp(springPink, moonPurple, 0.4f)
            });
            tooltips.Add(new TooltipLine(Mod, "NightHeal", "At night: +8 life regen and 10% chance on hit to heal 8 HP")
            {
                OverrideColor = moonPurple
            });
            tooltips.Add(new TooltipLine(Mod, "VerdantRenewal", "Killing enemies with Withered Root grants Verdant Renewal: +4 life regen for 3s (stacks 3x)")
            {
                OverrideColor = springPink
            });
            tooltips.Add(new TooltipLine(Mod, "Synergy", "Enemies with both Withered Root and Moonstruck take 10% more damage for 4s (does not stack)")
            {
                OverrideColor = Color.Lerp(springPink, moonPurple, 0.7f)
            });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'In the garden where moonlight falls, even thorns bloom silver'")
            {
                OverrideColor = moonPurple
            });
        }
    }

    public class SpringsMoonlitGardenPlayer : ModPlayer
    {
        public bool moonlitGardenEquipped;
        public int verdantRenewalStacks;
        private int verdantRenewalTimer;
        private int healProcCooldown;

        // Track Withered Root: NPC index → timer (frames remaining)
        private Dictionary<int, int> witheredRootTimers = new Dictionary<int, int>();
        // Track dual-debuff bonus: NPC index → timer
        private Dictionary<int, int> dualDebuffTimers = new Dictionary<int, int>();

        private static readonly int WitheredRootDuration = 240; // 4 seconds
        private static readonly int DualDebuffDuration = 240; // 4 seconds

        public override void ResetEffects()
        {
            moonlitGardenEquipped = false;
        }

        public override void PostUpdate()
        {
            if (!moonlitGardenEquipped)
            {
                verdantRenewalStacks = 0;
                verdantRenewalTimer = 0;
                witheredRootTimers.Clear();
                dualDebuffTimers.Clear();
                return;
            }

            if (healProcCooldown > 0) healProcCooldown--;

            // Decay Verdant Renewal
            if (verdantRenewalStacks > 0)
            {
                verdantRenewalTimer--;
                if (verdantRenewalTimer <= 0)
                {
                    verdantRenewalStacks = 0;
                    verdantRenewalTimer = 0;
                }
            }

            // Decay Withered Root timers
            var expiredRoots = witheredRootTimers.Where(kvp => kvp.Value <= 0).Select(kvp => kvp.Key).ToList();
            foreach (int key in expiredRoots)
                witheredRootTimers.Remove(key);
            foreach (int key in witheredRootTimers.Keys.ToList())
                witheredRootTimers[key]--;

            // Decay dual debuff timers
            var expiredDual = dualDebuffTimers.Where(kvp => kvp.Value <= 0).Select(kvp => kvp.Key).ToList();
            foreach (int key in expiredDual)
                dualDebuffTimers.Remove(key);
            foreach (int key in dualDebuffTimers.Keys.ToList())
                dualDebuffTimers[key]--;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!moonlitGardenEquipped) return;

            // Enemies with both Withered Root + Moonstruck: +10% damage
            if (dualDebuffTimers.ContainsKey(target.whoAmI))
                modifiers.FinalDamage *= 1.10f;
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!moonlitGardenEquipped) return;
            HandleGardenHit(target, damageDone);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!moonlitGardenEquipped || proj.owner != Player.whoAmI) return;
            HandleGardenHit(target, damageDone);
        }

        private void HandleGardenHit(NPC target, int damageDone)
        {
            bool isNight = !Main.dayTime;

            // Withered Root: 12% chance on any hit
            if (Main.rand.NextFloat() < 0.12f)
            {
                // Apply Slow + Ichor (Ichor = -15 defense) for 4 seconds
                target.AddBuff(BuffID.Slow, WitheredRootDuration);
                target.AddBuff(BuffID.Ichor, WitheredRootDuration);
                witheredRootTimers[target.whoAmI] = WitheredRootDuration;
            }

            // Withered Root + Moonstruck synergy: triggers when enemy has both Slow+Ichor
            // (from Withered Root, Moonstruck, or any source) while Withered Root is active
            bool hasSlowAndIchor = target.HasBuff(BuffID.Slow) && target.HasBuff(BuffID.Ichor);
            if (witheredRootTimers.ContainsKey(target.whoAmI) && hasSlowAndIchor)
            {
                if (!dualDebuffTimers.ContainsKey(target.whoAmI))
                    dualDebuffTimers[target.whoAmI] = DualDebuffDuration;
                // Does not stack, does not refresh
            }

            // Night: 10% chance to heal 8 HP
            if (isNight && healProcCooldown <= 0 && Main.rand.NextFloat() < 0.10f)
            {
                healProcCooldown = 30;
                Player.Heal(8);
            }

            // Check for kill while Withered Root active → Verdant Renewal
            if (target.life <= 0 && !target.immortal && witheredRootTimers.ContainsKey(target.whoAmI))
            {
                verdantRenewalStacks = Math.Min(verdantRenewalStacks + 1, 3);
                verdantRenewalTimer = 180; // 3 seconds, resets on new stack
                witheredRootTimers.Remove(target.whoAmI);
            }
        }
    }
    #endregion

    #region Summer's Infernal Peak - Summer + La Campanella
    /// <summary>
    /// Season-Theme Hybrid: Radiant Crown + Infernal Virtuoso
    /// From Radiant Crown: +16% damage, +10 crit, +8 defense, attacks inflict On Fire!
    /// From Infernal Virtuoso: Fire/lava immunity, 8% Tolling Death on any hit, +1 minion slot
    /// Signature: "Infernal Peak" — Heat Intensity stacks on burning enemy hits (max 10),
    ///   5 stacks → +12% attack speed, 10 stacks → Solar Zenith (6s, +20% damage, Ichor on hit),
    ///   day doubles stack rate, Scorched debuff on burning enemies (+5% damage taken)
    /// </summary>
    public class SummersInfernalPeak : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 1, gold: 50);
            Item.rare = ModContent.RarityType<LaCampanellaRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<SummersInfernalPeakPlayer>();
            modPlayer.infernalPeakEquipped = true;

            // === FROM RADIANT CROWN ===
            player.GetDamage(DamageClass.Generic) += 0.16f;
            player.GetCritChance(DamageClass.Generic) += 10;
            player.statDefense += 8;
            player.magmaStone = true; // Attacks inflict On Fire!

            // === FROM INFERNAL VIRTUOSO ===
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.lavaImmune = true;
            player.maxMinions += 1;

            // === HEAT INTENSITY TIER BONUSES ===
            if (modPlayer.heatIntensityStacks >= 5)
                player.GetAttackSpeed(DamageClass.Generic) += 0.12f;

            // Solar Zenith buff
            if (modPlayer.solarZenithTimer > 0)
                player.GetDamage(DamageClass.Generic) += 0.20f;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<RadiantCrown>()
                .AddIngredient<InfernalVirtuoso>()
                .AddIngredient<SolsticeBar>(15)
                .AddIngredient<ResonantCoreOfLaCampanella>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color summerGold = new Color(255, 180, 50);
            Color flameOrange = new Color(255, 140, 40);

            tooltips.Add(new TooltipLine(Mod, "Hybrid", "Radiant Crown + Infernal Virtuoso")
            {
                OverrideColor = Color.Lerp(summerGold, flameOrange, 0.5f)
            });
            tooltips.Add(new TooltipLine(Mod, "SummerStats", "+16% damage, +10 crit, +8 defense, attacks inflict On Fire!")
            {
                OverrideColor = summerGold
            });
            tooltips.Add(new TooltipLine(Mod, "CampanellaStats", "Immunity to fire debuffs and lava, 8% Tolling Death on any weapon hit, +1 minion slot")
            {
                OverrideColor = flameOrange
            });
            tooltips.Add(new TooltipLine(Mod, "Signature", "Infernal Peak: Hitting burning enemies builds Heat Intensity stacks (max 10)")
            {
                OverrideColor = Color.Lerp(summerGold, flameOrange, 0.4f)
            });
            tooltips.Add(new TooltipLine(Mod, "Tier1", "5 stacks: +12% attack speed")
            {
                OverrideColor = Color.Lerp(summerGold, flameOrange, 0.4f)
            });
            tooltips.Add(new TooltipLine(Mod, "Tier2", "10 stacks: Solar Zenith for 6s — +20% all damage, all attacks apply Ichor. Stacks reset.")
            {
                OverrideColor = Color.Lerp(summerGold, flameOrange, 0.6f)
            });
            tooltips.Add(new TooltipLine(Mod, "DayBonus", "During the day: Heat Intensity stacks build twice as fast")
            {
                OverrideColor = summerGold
            });
            tooltips.Add(new TooltipLine(Mod, "Scorched", "Burning enemies you hit gain Scorched — taking 5% more damage for 3s (does not stack)")
            {
                OverrideColor = flameOrange
            });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'At the peak of summer, even the bell's toll melts in the heat'")
            {
                OverrideColor = flameOrange
            });
        }
    }

    public class SummersInfernalPeakPlayer : ModPlayer
    {
        public bool infernalPeakEquipped;
        public int heatIntensityStacks;
        public int solarZenithTimer;

        private int heatDecayTimer;
        // Scorched: NPC index → timer
        private Dictionary<int, int> scorchedTimers = new Dictionary<int, int>();

        private static readonly int ScorchedDuration = 180; // 3 seconds
        private static readonly int SolarZenithDuration = 360; // 6 seconds

        public override void ResetEffects()
        {
            infernalPeakEquipped = false;
        }

        public override void PostUpdate()
        {
            if (!infernalPeakEquipped)
            {
                heatIntensityStacks = 0;
                solarZenithTimer = 0;
                heatDecayTimer = 0;
                scorchedTimers.Clear();
                return;
            }

            // Decay Heat Intensity (1 stack per 2 seconds without hitting a burning enemy)
            if (heatIntensityStacks > 0)
            {
                heatDecayTimer++;
                if (heatDecayTimer >= 120) // 2 seconds
                {
                    heatIntensityStacks--;
                    heatDecayTimer = 0;
                }
            }

            if (solarZenithTimer > 0) solarZenithTimer--;

            // Decay Scorched timers
            var expiredScorched = scorchedTimers.Where(kvp => kvp.Value <= 0).Select(kvp => kvp.Key).ToList();
            foreach (int key in expiredScorched)
                scorchedTimers.Remove(key);
            foreach (int key in scorchedTimers.Keys.ToList())
                scorchedTimers[key]--;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!infernalPeakEquipped) return;

            // Scorched: +5% damage taken
            if (scorchedTimers.ContainsKey(target.whoAmI))
                modifiers.FinalDamage *= 1.05f;
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!infernalPeakEquipped) return;
            HandlePeakHit(target, damageDone, null);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!infernalPeakEquipped || proj.owner != Player.whoAmI) return;
            HandlePeakHit(target, damageDone, proj);
        }

        private void HandlePeakHit(NPC target, int damageDone, Projectile proj)
        {
            bool isDay = Main.dayTime;

            // === TOLLING DEATH (8% any weapon hit) ===
            if (Main.rand.NextFloat() < 0.08f)
            {
                int secondStrike = (int)(damageDone * 0.75f);
                if (secondStrike > 0 && Main.myPlayer == Player.whoAmI)
                    target.SimpleStrikeNPC(secondStrike, 0, false, 0, null, false, 0, true);
                target.AddBuff(BuffID.WitheredWeapon, 180);
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.5f, Volume = 0.6f }, target.Center);
            }

            // === HEAT INTENSITY (on burning enemies) ===
            if (target.HasBuff(BuffID.OnFire) || target.HasBuff(BuffID.OnFire3))
            {
                int stacksToAdd = isDay ? 2 : 1;
                heatIntensityStacks = Math.Min(heatIntensityStacks + stacksToAdd, 10);
                heatDecayTimer = 0; // Reset decay

                // At 10 stacks: trigger Solar Zenith
                if (heatIntensityStacks >= 10)
                {
                    solarZenithTimer = SolarZenithDuration;
                    heatIntensityStacks = 0;
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 0.8f }, Player.Center);
                }

                // Apply Scorched debuff to burning enemies (does not stack, refreshes)
                scorchedTimers[target.whoAmI] = ScorchedDuration;
            }

            // === SOLAR ZENITH: apply Ichor on all hits ===
            if (solarZenithTimer > 0)
                target.AddBuff(BuffID.Ichor, 180);
        }
    }
    #endregion

    #region Winter's Enigmatic Silence - Winter + Enigma Variations
    /// <summary>
    /// Season-Theme Hybrid: Glacial Heart + Riddle of the Void
    /// From Glacial Heart: +14 defense, +12% DR, +8% move speed, Frostburn, ice immunity
    /// From Riddle of the Void: +15% all damage, 10% Paradox on hit
    /// Signature: "Enigmatic Silence" — Frostburn + Paradox on same enemy → Frozen Paradox
    ///   (30% slow, +15% damage taken, 5s), applying grants Winter's Focus (+10% crit, +8% dmg, 4s),
    ///   below 50% HP: +20% DR and Paradox → 15%
    /// </summary>
    public class WintersEnigmaticSilence : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 1, gold: 50);
            Item.rare = ModContent.RarityType<EnigmaVariationsRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<WintersEnigmaticSilencePlayer>();
            modPlayer.enigmaticSilenceEquipped = true;

            // === FROM GLACIAL HEART ===
            player.statDefense += 14;
            player.endurance += 0.12f;
            player.moveSpeed += 0.08f;
            player.frostBurn = true;

            // Ice immunity
            player.buffImmune[BuffID.Frozen] = true;
            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.Frostburn] = true;

            // === FROM RIDDLE OF THE VOID ===
            player.GetDamage(DamageClass.Generic) += 0.15f;

            // === BELOW 50% HP BONUS ===
            if (player.statLife < player.statLifeMax2 / 2)
                player.endurance += 0.20f;

            // === WINTER'S FOCUS BUFF ===
            if (modPlayer.wintersFocusTimer > 0)
            {
                player.GetCritChance(DamageClass.Generic) += 10;
                player.GetDamage(DamageClass.Generic) += 0.08f;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<GlacialHeart>()
                .AddIngredient<RiddleOfTheVoid>()
                .AddIngredient<PermafrostBar>(15)
                .AddIngredient<ResonantCoreOfEnigma>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color winterBlue = new Color(150, 220, 255);
            Color enigmaPurple = new Color(140, 60, 200);

            tooltips.Add(new TooltipLine(Mod, "Hybrid", "Glacial Heart + Riddle of the Void")
            {
                OverrideColor = Color.Lerp(winterBlue, enigmaPurple, 0.5f)
            });
            tooltips.Add(new TooltipLine(Mod, "WinterStats", "+14 defense, +12% damage reduction, +8% move speed, attacks inflict Frostburn")
            {
                OverrideColor = winterBlue
            });
            tooltips.Add(new TooltipLine(Mod, "Immunities", "Immunity to Frozen, Chilled, and Frostburn")
            {
                OverrideColor = winterBlue
            });
            tooltips.Add(new TooltipLine(Mod, "EnigmaStats", "+15% all damage, 10% Paradox on hit")
            {
                OverrideColor = enigmaPurple
            });
            tooltips.Add(new TooltipLine(Mod, "Signature", "Enigmatic Silence: Enemies with both Frostburn and Paradox gain Frozen Paradox")
            {
                OverrideColor = Color.Lerp(winterBlue, enigmaPurple, 0.4f)
            });
            tooltips.Add(new TooltipLine(Mod, "FrozenParadox", "Frozen Paradox: 30% slower, takes 15% more damage for 5s (does not stack, refreshes)")
            {
                OverrideColor = Color.Lerp(winterBlue, enigmaPurple, 0.5f)
            });
            tooltips.Add(new TooltipLine(Mod, "WintersFocus", "Applying Frozen Paradox grants Winter's Focus: +10% crit, +8% damage for 4s")
            {
                OverrideColor = Color.Lerp(winterBlue, enigmaPurple, 0.6f)
            });
            tooltips.Add(new TooltipLine(Mod, "LowHP", "Below 50% HP: +20% damage reduction, Paradox chance increases to 15%")
            {
                OverrideColor = enigmaPurple
            });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The coldest silence hides the deepest mystery'")
            {
                OverrideColor = enigmaPurple
            });
        }
    }

    public class WintersEnigmaticSilencePlayer : ModPlayer
    {
        public bool enigmaticSilenceEquipped;
        public int wintersFocusTimer;

        // Frozen Paradox: NPC index → timer
        private Dictionary<int, int> frozenParadoxTimers = new Dictionary<int, int>();

        private static readonly int FrozenParadoxDuration = 300; // 5 seconds
        private static readonly int WintersFocusDuration = 240; // 4 seconds

        private static readonly int[] ParadoxDebuffs = new int[]
        {
            BuffID.Confused, BuffID.Slow, BuffID.CursedInferno,
            BuffID.Ichor, BuffID.ShadowFlame, BuffID.Frostburn
        };

        public override void ResetEffects()
        {
            enigmaticSilenceEquipped = false;
        }

        public override void PostUpdate()
        {
            if (!enigmaticSilenceEquipped)
            {
                wintersFocusTimer = 0;
                frozenParadoxTimers.Clear();
                return;
            }

            if (wintersFocusTimer > 0) wintersFocusTimer--;

            // Decay Frozen Paradox timers
            var expired = frozenParadoxTimers.Where(kvp => kvp.Value <= 0).Select(kvp => kvp.Key).ToList();
            foreach (int key in expired)
                frozenParadoxTimers.Remove(key);
            foreach (int key in frozenParadoxTimers.Keys.ToList())
                frozenParadoxTimers[key]--;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!enigmaticSilenceEquipped) return;

            // Frozen Paradox: +15% damage taken
            if (frozenParadoxTimers.ContainsKey(target.whoAmI))
                modifiers.FinalDamage *= 1.15f;
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!enigmaticSilenceEquipped) return;
            HandleSilenceHit(target, damageDone);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!enigmaticSilenceEquipped || proj.owner != Player.whoAmI) return;
            HandleSilenceHit(target, damageDone);
        }

        private void HandleSilenceHit(NPC target, int damageDone)
        {
            bool lowHP = Player.statLife < Player.statLifeMax2 / 2;
            float paradoxChance = lowHP ? 0.15f : 0.10f;

            // Paradox on hit
            if (Main.rand.NextFloat() < paradoxChance)
            {
                int debuffId = ParadoxDebuffs[Main.rand.Next(ParadoxDebuffs.Length)];
                target.AddBuff(debuffId, 300);
            }

            // Check for Frozen Paradox: enemy currently has Frostburn AND any active Paradox debuff
            bool hasActiveParadox = false;
            foreach (int debuffId in ParadoxDebuffs)
            {
                if (target.HasBuff(debuffId))
                {
                    hasActiveParadox = true;
                    break;
                }
            }

            if (hasActiveParadox && target.HasBuff(BuffID.Frostburn))
            {
                // Apply / refresh Frozen Paradox slow
                target.AddBuff(BuffID.Slow, FrozenParadoxDuration);
                frozenParadoxTimers[target.whoAmI] = FrozenParadoxDuration;

                // Grant / refresh Winter's Focus on every Frozen Paradox application
                wintersFocusTimer = WintersFocusDuration;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item30 with { Pitch = 0.3f, Volume = 0.6f }, Player.Center);
            }
        }
    }
    #endregion
}
