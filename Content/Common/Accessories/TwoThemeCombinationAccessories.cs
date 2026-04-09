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
    #region Nocturne of Azure Flames - Moonlight + La Campanella
    /// <summary>
    /// Two-Theme Combination: Moonlight Sonata + La Campanella
    /// From Sonata: +15% damage at night, +10% during day, -12% mana cost
    /// From Campanella: Fire/lava immunity, +1 minion slot
    /// Signature: "Azure Immolation" — blue fire DoT, doubled and spreading at night
    /// </summary>
    public class NocturneOfAzureFlames : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 75);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<NocturneOfAzureFlamesPlayer>();
            modPlayer.nocturneEquipped = true;

            // From Sonata's Embrace: +15% damage at night, +10% during day, -12% mana cost
            if (!Main.dayTime)
                player.GetDamage(DamageClass.Generic) += 0.15f;
            else
                player.GetDamage(DamageClass.Generic) += 0.10f;
            player.manaCost -= 0.12f;

            // From Infernal Virtuoso: fire/lava immunity, +1 minion slot
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.lavaImmune = true;
            player.maxMinions++;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color moonBlue = MoonlightColors.LightBlue;
            Color flameOrange = new Color(255, 140, 40);
            Color azureBlue = new Color(100, 150, 255);

            tooltips.Add(new TooltipLine(Mod, "Combo", "Combines: Moonlight Sonata + La Campanella")
            { OverrideColor = azureBlue });

            tooltips.Add(new TooltipLine(Mod, "SonataStats", "+15% damage at night, +10% during the day, -12% mana cost")
            { OverrideColor = moonBlue });

            tooltips.Add(new TooltipLine(Mod, "CampanellaStats", "Immunity to fire debuffs and lava, +1 minion slot")
            { OverrideColor = flameOrange });

            tooltips.Add(new TooltipLine(Mod, "Signature", "10% chance for magic and summon attacks to inflict 'Azure Immolation'")
            { OverrideColor = azureBlue });

            tooltips.Add(new TooltipLine(Mod, "SignatureDesc", "Azure Immolation: blue fire dealing 8% magic damage/sec for 4s")
            { OverrideColor = azureBlue });

            tooltips.Add(new TooltipLine(Mod, "NightBonus", "At night: Azure Immolation damage doubled, spreads on kill")
            { OverrideColor = moonBlue });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Where moonlight meets flame, the fire burns blue — cold and beautiful and without mercy'")
            { OverrideColor = new Color(150, 180, 220) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SonatasEmbrace>()
                .AddIngredient<InfernalVirtuoso>()
                .AddIngredient<HarmonicCoreOfMoonlightSonata>(15)
                .AddIngredient<HarmonicCoreOfLaCampanella>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class NocturneOfAzureFlamesPlayer : ModPlayer
    {
        public bool nocturneEquipped;
        private Dictionary<int, int> azureImmolationTimers = new Dictionary<int, int>();

        public override void ResetEffects()
        {
            nocturneEquipped = false;
        }

        public override void PostUpdate()
        {
            if (!nocturneEquipped) return;

            // Tick Azure Immolation DoT on afflicted enemies
            List<int> expired = new List<int>();
            foreach (var kvp in azureImmolationTimers.ToList())
            {
                int npcIdx = kvp.Key;
                int remaining = kvp.Value - 1;

                if (remaining <= 0 || npcIdx < 0 || npcIdx >= Main.maxNPCs || !Main.npc[npcIdx].active)
                {
                    expired.Add(npcIdx);
                    continue;
                }

                azureImmolationTimers[npcIdx] = remaining;

                // Tick damage once per second (every 60 frames)
                if (remaining % 60 == 0 && Main.myPlayer == Player.whoAmI)
                {
                    float magicDmg = Player.GetTotalDamage(DamageClass.Magic).ApplyTo(100);
                    int dotDamage = (int)(magicDmg * 0.08f);
                    bool isNight = !Main.dayTime;
                    if (isNight) dotDamage *= 2;
                    if (dotDamage > 0)
                        Main.npc[npcIdx].SimpleStrikeNPC(dotDamage, 0, false, 0, null, false, 0, true);
                }
            }
            foreach (int key in expired)
                azureImmolationTimers.Remove(key);
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            TryApplyAzureImmolation(target, item.DamageType);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.owner != Player.whoAmI) return;
            TryApplyAzureImmolation(target, proj.DamageType);
        }

        private void TryApplyAzureImmolation(NPC target, DamageClass damageType)
        {
            if (!nocturneEquipped) return;

            // Only magic and summon attacks
            bool isMagicOrSummon = damageType.CountsAsClass(DamageClass.Magic)
                || damageType.CountsAsClass(DamageClass.Summon);
            if (!isMagicOrSummon) return;

            if (Main.rand.NextFloat() < 0.10f)
            {
                azureImmolationTimers[target.whoAmI] = 240; // 4 seconds

                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item45 with { Pitch = 0.4f, Volume = 0.5f }, target.Center);
            }

            // Night spread on kill
            if (target.life <= 0 && !target.immortal && !Main.dayTime)
            {
                float spreadRange = 50f * 16f; // 50 blocks
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && !npc.immortal && npc.whoAmI != target.whoAmI)
                    {
                        if (Vector2.Distance(npc.Center, target.Center) <= spreadRange)
                            azureImmolationTimers[npc.whoAmI] = 240;
                    }
                }
            }
        }
    }
    #endregion

    #region Valse Macabre - Eroica + Enigma Variations
    /// <summary>
    /// Two-Theme Combination: Eroica + Enigma Variations
    /// From Hero's Symphony: 20% chance melee double damage, +15% melee speed
    /// From Riddle of the Void: +15% all damage, 10% Paradox on hit
    /// Signature: "Dance of Death" — on kill: 6s buff, ignore 20% def, every 3rd hit +50% phantom strike
    /// </summary>
    public class ValseMacabre : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 75);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<ValseMacabrePlayer>();
            modPlayer.valseMacabreEquipped = true;

            // From Hero's Symphony: +15% melee speed (double damage handled in ModifyHitNPC)
            player.GetAttackSpeed(DamageClass.Melee) += 0.15f;

            // From Riddle of the Void: +15% all damage (Paradox handled in OnHit)
            player.GetDamage(DamageClass.Generic) += 0.15f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color eroicaGold = EroicaColors.Gold;
            Color enigmaPurple = EnigmaColors.Purple;
            Color valseMix = new Color(200, 130, 130);

            tooltips.Add(new TooltipLine(Mod, "Combo", "Combines: Eroica + Enigma Variations")
            { OverrideColor = valseMix });

            tooltips.Add(new TooltipLine(Mod, "EroicaStats", "20% chance for melee attacks to deal double damage, +15% melee speed")
            { OverrideColor = eroicaGold });

            tooltips.Add(new TooltipLine(Mod, "EnigmaStats", "+15% all damage, 10% chance on hit to apply Paradox debuff")
            { OverrideColor = EnigmaColors.GreenFlame });

            tooltips.Add(new TooltipLine(Mod, "Signature", "Killing an enemy triggers 'Dance of Death' for 6 seconds")
            { OverrideColor = enigmaPurple });

            tooltips.Add(new TooltipLine(Mod, "SignatureDesc", "Dance of Death: attacks ignore 20% defense, every 3rd hit deals +50% damage")
            { OverrideColor = enigmaPurple });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'The waltz quickens — one partner leads with glory, the other with oblivion'")
            { OverrideColor = new Color(200, 180, 160) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<HerosSymphony>()
                .AddIngredient<RiddleOfTheVoid>()
                .AddIngredient<HarmonicCoreOfEroica>(15)
                .AddIngredient<HarmonicCoreOfEnigma>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class ValseMacabrePlayer : ModPlayer
    {
        public bool valseMacabreEquipped;
        private int danceOfDeathTimer;
        private int hitCounter;

        private static readonly int[] ParadoxDebuffs = new int[]
        {
            BuffID.Confused, BuffID.Slow, BuffID.CursedInferno,
            BuffID.Ichor, BuffID.ShadowFlame
        };

        public override void ResetEffects()
        {
            valseMacabreEquipped = false;
        }

        public override void PostUpdate()
        {
            if (danceOfDeathTimer > 0) danceOfDeathTimer--;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!valseMacabreEquipped) return;

            // 20% chance melee double damage (from Hero's Symphony)
            if (Player.HeldItem != null && Player.HeldItem.DamageType.CountsAsClass(DamageClass.Melee))
            {
                if (Main.rand.NextFloat() < 0.20f)
                    modifiers.FinalDamage *= 2f;
            }

            // Dance of Death: ignore 20% defense
            if (danceOfDeathTimer > 0)
                modifiers.ArmorPenetration += target.defense * 0.20f;
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            HandleHit(target, damageDone);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.owner == Player.whoAmI)
                HandleHit(target, damageDone);
        }

        private void HandleHit(NPC target, int damageDone)
        {
            if (!valseMacabreEquipped) return;

            // 10% Paradox chance (from Riddle of the Void)
            if (Main.rand.NextFloat() < 0.10f)
            {
                int debuffId = ParadoxDebuffs[Main.rand.Next(ParadoxDebuffs.Length)];
                target.AddBuff(debuffId, 240);
            }

            // Dance of Death phantom strike: every 3rd hit
            if (danceOfDeathTimer > 0)
            {
                hitCounter++;
                if (hitCounter >= 3)
                {
                    hitCounter = 0;
                    int phantomDamage = (int)(damageDone * 0.50f);
                    if (phantomDamage > 0 && Main.myPlayer == Player.whoAmI)
                        target.SimpleStrikeNPC(phantomDamage, 0, false, 0, null, false, 0, true);

                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.3f, Volume = 0.6f }, target.Center);
                }
            }

            // Kill check → trigger Dance of Death
            if (target.life <= 0 && !target.immortal)
            {
                danceOfDeathTimer = 360; // 6 seconds
                hitCounter = 0;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item119 with { Pitch = -0.2f, Volume = 0.7f }, target.Center);
            }
        }
    }
    #endregion

    #region Reverie of the Silver Swan - Moonlight + Swan Lake
    /// <summary>
    /// Two-Theme Combination: Moonlight Sonata + Swan Lake
    /// From Sonata: +15% damage at night, +10% during day, +18% crit at night, +15% crit during day
    /// From Swan's Diadem: +25% movement speed, +12% damage
    /// Signature: "Moonlit Reverie" — stacking movement buff, 10 stacks = Silver Cascade burst
    /// </summary>
    public class ReverieOfTheSilverSwan : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 75);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<ReverieOfTheSilverSwanPlayer>();
            modPlayer.reverieEquipped = true;

            // From Sonata's Embrace: +15% damage at night, +10% during day, +18% crit at night, +15% crit during day
            if (!Main.dayTime)
            {
                player.GetDamage(DamageClass.Generic) += 0.15f;
                player.GetCritChance(DamageClass.Generic) += 18;
            }
            else
            {
                player.GetDamage(DamageClass.Generic) += 0.10f;
                player.GetCritChance(DamageClass.Generic) += 15;
            }

            // From Swan's Chromatic Diadem: +25% movement speed, +12% damage
            player.moveSpeed += 0.25f;
            player.GetDamage(DamageClass.Generic) += 0.12f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color moonSilver = MoonlightColors.Silver;
            Color rainbow = SwanColors.GetRainbow((float)(Main.GameUpdateCount % 300) / 300f);
            Color reverieMix = new Color(200, 210, 240);

            tooltips.Add(new TooltipLine(Mod, "Combo", "Combines: Moonlight Sonata + Swan Lake")
            { OverrideColor = reverieMix });

            tooltips.Add(new TooltipLine(Mod, "SonataStats", "+15% damage at night, +10% during the day, +18% crit at night, +15% crit during the day")
            { OverrideColor = MoonlightColors.LightBlue });

            tooltips.Add(new TooltipLine(Mod, "SwanStats", "+25% movement speed, +12% damage")
            { OverrideColor = rainbow });

            tooltips.Add(new TooltipLine(Mod, "Signature", "While moving, gain 1 'Moonlit Reverie' stack per second (max 10)")
            { OverrideColor = moonSilver });

            tooltips.Add(new TooltipLine(Mod, "SignatureDesc", "Each stack: +1% dodge chance; at night stacks build 2x faster")
            { OverrideColor = moonSilver });

            tooltips.Add(new TooltipLine(Mod, "Finisher", "At 10 stacks: next hit triggers Silver Cascade (damage echoes to all enemies in range)")
            { OverrideColor = rainbow });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'The silver swan glides through a dream of moonlight, untouchable and radiant'")
            { OverrideColor = reverieMix });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SonatasEmbrace>()
                .AddIngredient<SwansChromaticDiadem>()
                .AddIngredient<HarmonicCoreOfMoonlightSonata>(15)
                .AddIngredient<HarmonicCoreOfSwanLake>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class ReverieOfTheSilverSwanPlayer : ModPlayer
    {
        public bool reverieEquipped;
        private int reverieStacks;
        private int stackBuildTimer;
        private bool silverCascadeReady;

        public override void ResetEffects()
        {
            reverieEquipped = false;
        }

        public override void PostUpdate()
        {
            if (!reverieEquipped)
            {
                reverieStacks = 0;
                silverCascadeReady = false;
                return;
            }

            // Build stacks while moving
            bool isMoving = Player.velocity.Length() > 1f;
            bool isNight = !Main.dayTime;
            int buildRate = isNight ? 30 : 60; // 2x faster at night

            if (isMoving)
            {
                stackBuildTimer++;
                if (stackBuildTimer >= buildRate && reverieStacks < 10)
                {
                    stackBuildTimer = 0;
                    reverieStacks++;
                    if (reverieStacks >= 10)
                        silverCascadeReady = true;
                }
            }
            else
            {
                stackBuildTimer = 0;
            }
        }

        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (!reverieEquipped || reverieStacks <= 0) return false;

            float dodgeChance = reverieStacks * 0.01f;
            if (Main.rand.NextFloat() < dodgeChance)
            {
                Player.immune = true;
                Player.immuneTime = 20;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.7f }, Player.Center);
                return true;
            }

            return false;
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            TrySilverCascade(target, damageDone);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.owner == Player.whoAmI)
                TrySilverCascade(target, damageDone);
        }

        private void TrySilverCascade(NPC target, int damageDone)
        {
            if (!reverieEquipped || !silverCascadeReady) return;

            silverCascadeReady = false;
            reverieStacks = 0;
            stackBuildTimer = 0;

            // Silver Cascade: echo the damage dealt to all enemies in range
            int cascadeDamage = damageDone;
            if (cascadeDamage > 0 && Main.myPlayer == Player.whoAmI)
            {
                float cascadeRange = 200f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && !npc.immortal && npc.whoAmI != target.whoAmI)
                    {
                        if (Vector2.Distance(npc.Center, target.Center) <= cascadeRange)
                            npc.SimpleStrikeNPC(cascadeDamage, 0, false, 0, null, false, 0, true);
                    }
                }
            }

            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.5f, Volume = 0.8f }, target.Center);
        }
    }
    #endregion

    #region Fantasia of Burning Grace - La Campanella + Swan Lake
    /// <summary>
    /// Two-Theme Combination: La Campanella + Swan Lake
    /// From Campanella: Fire/lava immunity, 8% whip Tolling Death
    /// From Swan's Diadem: +20% movement speed, damage buff effectiveness +50%
    /// Signature: "Burning Pas de Deux" — speed-based proximity debuff, death explosions
    /// </summary>
    public class FantasiaOfBurningGrace : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 75);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<FantasiaOfBurningGracePlayer>();
            modPlayer.fantasiaEquipped = true;

            // From Infernal Virtuoso: fire/lava immunity (Tolling Death handled in OnHit)
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.lavaImmune = true;

            // From Swan's Chromatic Diadem: +20% movement speed, damage buff effectiveness +50%
            player.moveSpeed += 0.20f;
            // Approximate +50% buff effectiveness as flat damage (parent uses +18% for +80%)
            player.GetDamage(DamageClass.Generic) += 0.11f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color flameOrange = new Color(255, 140, 40);
            Color rainbow = SwanColors.GetRainbow((float)(Main.GameUpdateCount % 300) / 300f);
            Color fantasiaMix = new Color(255, 200, 180);

            tooltips.Add(new TooltipLine(Mod, "Combo", "Combines: La Campanella + Swan Lake")
            { OverrideColor = fantasiaMix });

            tooltips.Add(new TooltipLine(Mod, "CampanellaStats", "Immunity to fire debuffs and lava, 8% whip Tolling Death chance")
            { OverrideColor = flameOrange });

            tooltips.Add(new TooltipLine(Mod, "SwanStats", "+20% movement speed, damage buff effectiveness +50%")
            { OverrideColor = rainbow });

            tooltips.Add(new TooltipLine(Mod, "Signature", "Moving above 75% max speed inflicts 'Burning Pas de Deux' on nearby enemies")
            { OverrideColor = flameOrange });

            tooltips.Add(new TooltipLine(Mod, "SignatureDesc", "Enemies take 3% weapon damage/sec and -10% move speed for 3s (250 range)")
            { OverrideColor = flameOrange });

            tooltips.Add(new TooltipLine(Mod, "DeathEffect", "Debuffed enemies explode on death (150% weapon damage, 120 range)")
            { OverrideColor = rainbow });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Two dancers — one of flame, one of feathers — and neither knows who leads'")
            { OverrideColor = fantasiaMix });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<InfernalVirtuoso>()
                .AddIngredient<SwansChromaticDiadem>()
                .AddIngredient<HarmonicCoreOfLaCampanella>(15)
                .AddIngredient<HarmonicCoreOfSwanLake>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class FantasiaOfBurningGracePlayer : ModPlayer
    {
        public bool fantasiaEquipped;
        private HashSet<int> burningPasTargets = new HashSet<int>();
        private int proximityTickTimer;

        public override void ResetEffects()
        {
            fantasiaEquipped = false;
        }

        public override void PostUpdate()
        {
            if (!fantasiaEquipped) return;

            // Check speed threshold (75% of max move speed)
            float maxSpeed = Player.maxRunSpeed * Player.moveSpeed;
            bool fastEnough = Player.velocity.Length() > maxSpeed * 0.75f;

            proximityTickTimer++;
            if (proximityTickTimer < 60) return; // Tick once per second
            proximityTickTimer = 0;

            if (!fastEnough) return;

            // Apply Burning Pas de Deux to enemies within 250 range
            float range = 250f;
            float weaponDmg = Player.HeldItem != null
                ? Player.GetTotalDamage(Player.HeldItem.DamageType).ApplyTo(Player.HeldItem.damage)
                : 50f;
            int tickDamage = Math.Max(1, (int)(weaponDmg * 0.03f));

            burningPasTargets.Clear();
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.immortal && !npc.dontTakeDamage)
                {
                    if (Vector2.Distance(npc.Center, Player.Center) <= range)
                    {
                        burningPasTargets.Add(npc.whoAmI);
                        npc.AddBuff(BuffID.Slow, 180); // 3 seconds slow
                        if (Main.myPlayer == Player.whoAmI)
                            npc.SimpleStrikeNPC(tickDamage, 0, false, 0, null, false, 0, true);
                    }
                }
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!fantasiaEquipped || proj.owner != Player.whoAmI) return;

            // 8% Tolling Death on whip hits (from Infernal Virtuoso)
            if (ProjectileID.Sets.IsAWhip[proj.type] && Main.rand.NextFloat() < 0.08f)
            {
                int secondStrike = (int)(damageDone * 0.75f);
                if (secondStrike > 0 && Main.myPlayer == Player.whoAmI)
                    target.SimpleStrikeNPC(secondStrike, 0, false, 0, null, false, 0, true);
                target.AddBuff(BuffID.WitheredWeapon, 180);
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.5f, Volume = 0.6f }, target.Center);
            }

            // Death explosion for debuffed enemies
            CheckDeathExplosion(target, damageDone);
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!fantasiaEquipped) return;
            CheckDeathExplosion(target, damageDone);
        }

        private void CheckDeathExplosion(NPC target, int damageDone)
        {
            if (target.life > 0 || target.immortal) return;
            if (!burningPasTargets.Contains(target.whoAmI)) return;

            // Graceful fire explosion: 150% weapon damage, 120 range
            int explosionDmg = (int)(damageDone * 1.5f);
            if (explosionDmg > 0 && Main.myPlayer == Player.whoAmI)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && !npc.immortal && npc.whoAmI != target.whoAmI)
                    {
                        if (Vector2.Distance(npc.Center, target.Center) <= 120f)
                            npc.SimpleStrikeNPC(explosionDmg, 0, false, 0, null, false, 0, true);
                    }
                }
            }

            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item74 with { Pitch = 0.2f }, target.Center);
        }
    }
    #endregion

    #region Triumphant Arabesque - Eroica + Swan Lake
    /// <summary>
    /// Two-Theme Combination: Eroica + Swan Lake
    /// From Hero's Symphony: +18% melee damage, kills trigger Heroic Surge (+20% damage, 4s)
    /// From Swan's Diadem: +25% movement speed, Dying Swan's Grace (airborne weapon buff)
    /// Signature: "Valor of the Radiant Swan" — airborne triggers 10s buff: +30% attack speed, +15% reach, 0.5% HP heal per hit
    /// </summary>
    public class TriumphantArabesque : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 75);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<TriumphantArabesquePlayer>();
            modPlayer.arabesqueEquipped = true;

            // From Hero's Symphony: +18% melee damage (Heroic Surge handled in OnHit)
            player.GetDamage(DamageClass.Melee) += 0.18f;

            // From Swan's Chromatic Diadem: +25% movement speed
            player.moveSpeed += 0.25f;

            // Valor of the Radiant Swan: timed buff from airborne
            if (modPlayer.valorBuffTimer > 0)
            {
                player.GetAttackSpeed(DamageClass.Generic) += 0.30f;
                // +15% melee reach approximated as melee damage bonus
                player.GetDamage(DamageClass.Melee) += 0.08f;
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color eroicaGold = EroicaColors.Gold;
            Color rainbow = SwanColors.GetRainbow((float)(Main.GameUpdateCount % 300) / 300f);
            Color arabesqueMix = new Color(255, 220, 200);

            tooltips.Add(new TooltipLine(Mod, "Combo", "Combines: Eroica + Swan Lake")
            { OverrideColor = arabesqueMix });

            tooltips.Add(new TooltipLine(Mod, "EroicaStats", "+18% melee damage, kills trigger Heroic Surge (+20% damage, 4s)")
            { OverrideColor = eroicaGold });

            tooltips.Add(new TooltipLine(Mod, "SwanStats", "+25% movement speed, Dying Swan's Grace when airborne")
            { OverrideColor = rainbow });

            tooltips.Add(new TooltipLine(Mod, "Signature", "Airborne triggers 'Valor of the Radiant Swan' for 10 seconds")
            { OverrideColor = eroicaGold });

            tooltips.Add(new TooltipLine(Mod, "ValorEffect", "+30% attack speed, +15% melee reach, all attacks heal 0.5% max HP")
            { OverrideColor = rainbow });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'The hero leaps — and for a moment, gravity remembers that it serves the brave'")
            { OverrideColor = arabesqueMix });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<HerosSymphony>()
                .AddIngredient<SwansChromaticDiadem>()
                .AddIngredient<HarmonicCoreOfEroica>(15)
                .AddIngredient<HarmonicCoreOfSwanLake>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class TriumphantArabesquePlayer : ModPlayer
    {
        public bool arabesqueEquipped;
        public int valorBuffTimer;
        private int heroicSurgeTimer;

        public override void ResetEffects()
        {
            arabesqueEquipped = false;
        }

        public override void PostUpdate()
        {
            if (!arabesqueEquipped)
            {
                valorBuffTimer = 0;
                return;
            }

            if (heroicSurgeTimer > 0)
            {
                heroicSurgeTimer--;
                Player.GetDamage(DamageClass.Generic) += 0.20f;
            }

            if (valorBuffTimer > 0)
                valorBuffTimer--;

            // Trigger Valor of the Radiant Swan when airborne
            bool airborne = Player.velocity.Y != 0 && !Player.mount.Active;
            if (airborne)
                valorBuffTimer = 600; // 10 seconds
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            HandleHit(target, damageDone);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.owner == Player.whoAmI)
                HandleHit(target, damageDone);
        }

        private void HandleHit(NPC target, int damageDone)
        {
            if (!arabesqueEquipped) return;

            // Dying Swan's Grace: apply Odile's Beauty DoT when airborne (non-stacking)
            bool airborne = Player.velocity.Y != 0 && !Player.mount.Active;
            if (airborne && !target.HasBuff(ModContent.BuffType<OdilesBeauty>()))
            {
                target.AddBuff(ModContent.BuffType<OdilesBeauty>(), 300);
                int weaponDamage = Player.HeldItem != null
                    ? (int)Player.GetTotalDamage(Player.HeldItem.DamageType).ApplyTo(Player.HeldItem.damage)
                    : 50;
                target.GetGlobalNPC<OdilesBeautyNPC>().SetDamage(weaponDamage);
            }

            // Valor of the Radiant Swan: heal 0.5% max HP per hit
            if (valorBuffTimer > 0)
            {
                int healAmount = Math.Max(1, Player.statLifeMax2 / 200);
                Player.statLife = Math.Min(Player.statLife + healAmount, Player.statLifeMax2);
                Player.HealEffect(healAmount);
            }

            // Kill → Heroic Surge (+20% damage, 4s)
            if (target.life <= 0 && !target.immortal)
            {
                heroicSurgeTimer = 240;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item119 with { Pitch = 0.2f, Volume = 0.6f }, target.Center);
            }
        }
    }
    #endregion

    #region Inferno of Lost Shadows - La Campanella + Enigma Variations
    /// <summary>
    /// Two-Theme Combination: La Campanella + Enigma Variations
    /// From Campanella: Fire/lava immunity, +1 minion slot
    /// From Riddle of the Void: +15% all damage, 10% Paradox on hit
    /// Signature: "Void Immolation" — 5% chance to inflict for 5s (no stack), enemy takes 2% more damage,
    ///   +0.2% per additional hit, cap 30%
    /// </summary>
    public class InfernoOfLostShadows : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 75);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<InfernoOfLostShadowsPlayer>();
            modPlayer.infernoEquipped = true;

            // From Infernal Virtuoso: fire/lava immunity, +1 minion slot
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.lavaImmune = true;
            player.maxMinions++;

            // From Riddle of the Void: +15% all damage (Paradox handled in OnHit)
            player.GetDamage(DamageClass.Generic) += 0.15f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color flameOrange = new Color(255, 140, 40);
            Color greenFlame = EnigmaColors.GreenFlame;
            Color infernoMix = new Color(200, 120, 60);

            tooltips.Add(new TooltipLine(Mod, "Combo", "Combines: La Campanella + Enigma Variations")
            { OverrideColor = infernoMix });

            tooltips.Add(new TooltipLine(Mod, "CampanellaStats", "Immunity to fire debuffs and lava, +1 minion slot")
            { OverrideColor = flameOrange });

            tooltips.Add(new TooltipLine(Mod, "EnigmaStats", "+15% all damage, 10% chance on hit to apply Paradox debuff")
            { OverrideColor = greenFlame });

            tooltips.Add(new TooltipLine(Mod, "Signature", "5% chance on hit to inflict 'Void Immolation' for 5 seconds (cannot stack)")
            { OverrideColor = infernoMix });

            tooltips.Add(new TooltipLine(Mod, "StackEffect", "Void Immolation: enemy takes 2% more damage from all sources")
            { OverrideColor = greenFlame });

            tooltips.Add(new TooltipLine(Mod, "Escalation", "Additional hits increase damage taken by 0.2% (cap: 30%)")
            { OverrideColor = flameOrange });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'The inferno swallowed the shadows, and the shadows swallowed back'")
            { OverrideColor = new Color(180, 140, 100) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<InfernalVirtuoso>()
                .AddIngredient<RiddleOfTheVoid>()
                .AddIngredient<HarmonicCoreOfLaCampanella>(15)
                .AddIngredient<HarmonicCoreOfEnigma>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class InfernoOfLostShadowsPlayer : ModPlayer
    {
        public bool infernoEquipped;

        // Void Immolation tracking: NPC index → { timer, bonusDamagePercent }
        private Dictionary<int, int> voidImmolationTimers = new Dictionary<int, int>();
        private Dictionary<int, float> voidImmolationDamageBonus = new Dictionary<int, float>();

        private static readonly int[] ParadoxDebuffs = new int[]
        {
            BuffID.Confused, BuffID.Slow, BuffID.CursedInferno,
            BuffID.Ichor, BuffID.ShadowFlame, BuffID.Frostburn
        };

        public override void ResetEffects()
        {
            infernoEquipped = false;
        }

        public override void PostUpdate()
        {
            // Decay void immolation timers
            List<int> expired = new List<int>();
            foreach (var kvp in voidImmolationTimers.ToList())
            {
                voidImmolationTimers[kvp.Key] = kvp.Value - 1;
                if (voidImmolationTimers[kvp.Key] <= 0)
                    expired.Add(kvp.Key);
            }
            foreach (int key in expired)
            {
                voidImmolationTimers.Remove(key);
                voidImmolationDamageBonus.Remove(key);
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            HandleHit(target, damageDone);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.owner == Player.whoAmI)
                HandleHit(target, damageDone);
        }

        private void HandleHit(NPC target, int damageDone)
        {
            if (!infernoEquipped) return;

            // 10% Paradox on hit (from Riddle of the Void)
            if (Main.rand.NextFloat() < 0.10f)
            {
                int debuffId = ParadoxDebuffs[Main.rand.Next(ParadoxDebuffs.Length)];
                target.AddBuff(debuffId, 240);
            }

            // If enemy already has Void Immolation, escalate damage bonus (+0.2% per hit, cap 30%)
            if (voidImmolationTimers.ContainsKey(target.whoAmI))
            {
                float currentBonus = voidImmolationDamageBonus.GetValueOrDefault(target.whoAmI, 0.02f);
                voidImmolationDamageBonus[target.whoAmI] = Math.Min(currentBonus + 0.002f, 0.30f);
                return; // Don't re-roll for new application while active
            }

            // 5% chance to inflict Void Immolation for 5 seconds (cannot stack)
            if (Main.rand.NextFloat() < 0.05f)
            {
                voidImmolationTimers[target.whoAmI] = 300; // 5 seconds
                voidImmolationDamageBonus[target.whoAmI] = 0.02f; // Start at 2%
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.3f, Volume = 0.6f }, target.Center);
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!infernoEquipped) return;

            // Void Immolation: enemy takes more damage from all sources
            if (voidImmolationDamageBonus.TryGetValue(target.whoAmI, out float bonus) && bonus > 0f)
                modifiers.FinalDamage *= 1f + bonus;
        }
    }
    #endregion
}
