using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using MagnumOpus.Common.Systems.Particles;
using System;
using System.Collections.Generic;

namespace MagnumOpus.Common.Prefixes
{
    #region Shared Debuff System

    /// <summary>
    /// Resonant Burn debuff - Applied by all Resonance prefix weapons
    /// Creates rainbow + black/white flame effects and music note particles
    /// </summary>
    public class ResonantBurnDebuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.LongerExpertDebuff[Type] = false;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            var burnNPC = npc.GetGlobalNPC<ResonantBurnNPC>();
            burnNPC.resonantBurn = true;
        }
    }

    /// <summary>
    /// Global NPC for Resonant Burn debuff effects
    /// </summary>
    public class ResonantBurnNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool resonantBurn;
        public int burnDamage;
        private int storedBurnDamage;

        // Stacking system for Resonance Synergy accessories
        public const int MAX_STACKS = 5;
        public const int STACK_DECAY_TIME = 180; // 3 seconds
        public const int STACK_DECAY_INTERVAL = 60; // 1 second between stack decay

        public int burnStacks;
        public int stackDecayTimer;
        private int stackDecayInterval;
        private bool maxStacksTriggeredThisApplication; // Prevents multiple triggers per burn cycle

        /// <summary>
        /// Event fired when burn stacks reach maximum (5).
        /// Accessories hook into this for their max-stack bonuses.
        /// </summary>
        public static event Action<NPC, Player> OnMaxStacksReached;

        /// <summary>
        /// Adds a stack of Resonant Burn. Called on each hit from a Resonance weapon.
        /// </summary>
        public void AddStack(NPC npc, Player player)
        {
            int previousStacks = burnStacks;
            burnStacks = Math.Min(burnStacks + 1, MAX_STACKS);
            stackDecayTimer = STACK_DECAY_TIME;
            stackDecayInterval = STACK_DECAY_INTERVAL;

            // Trigger max stacks event once per burn application cycle
            if (burnStacks == MAX_STACKS && previousStacks < MAX_STACKS && !maxStacksTriggeredThisApplication)
            {
                maxStacksTriggeredThisApplication = true;
                OnMaxStacksReached?.Invoke(npc, player);
            }
        }

        /// <summary>
        /// Adds multiple stacks at once (e.g., for accessories that grant bonus stacks).
        /// </summary>
        public void AddStacks(NPC npc, Player player, int count)
        {
            for (int i = 0; i < count; i++)
            {
                AddStack(npc, player);
            }
        }

        /// <summary>
        /// Consumes all stacks and resets the max-trigger flag. Returns the stack count before reset.
        /// </summary>
        public int ConsumeStacks()
        {
            int consumed = burnStacks;
            burnStacks = 0;
            stackDecayTimer = 0;
            maxStacksTriggeredThisApplication = false;
            return consumed;
        }

        /// <summary>
        /// Returns true if this NPC has max (5) Resonant Burn stacks.
        /// </summary>
        public bool HasMaxStacks => burnStacks >= MAX_STACKS;

        public override void ResetEffects(NPC npc)
        {
            resonantBurn = false;
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (resonantBurn)
            {
                int effectiveDamage = burnDamage > 0 ? burnDamage : storedBurnDamage;

                if (burnDamage > 0)
                    storedBurnDamage = burnDamage;

                // Base DPS scales with stacks: 15% base + 3% per stack (max 30% at 5 stacks)
                float stackMultiplier = 1f + (burnStacks * 0.2f); // 20% more per stack
                int dps = Math.Max(8, (int)(effectiveDamage * 0.15f * stackMultiplier));

                if (npc.lifeRegen > 0)
                    npc.lifeRegen = 0;

                npc.lifeRegen -= dps * 2;

                if (damage < dps / 4)
                    damage = dps / 4;

                // Stack decay logic
                if (stackDecayTimer > 0)
                {
                    stackDecayTimer--;
                }
                else if (burnStacks > 0)
                {
                    // After decay timer expires, lose 1 stack per second
                    stackDecayInterval--;
                    if (stackDecayInterval <= 0)
                    {
                        burnStacks--;
                        stackDecayInterval = STACK_DECAY_INTERVAL;

                        // Reset max-trigger flag when stacks drop below max
                        if (burnStacks < MAX_STACKS)
                            maxStacksTriggeredThisApplication = false;
                    }
                }
            }
            else
            {
                storedBurnDamage = 0;
                burnDamage = 0;
                burnStacks = 0;
                stackDecayTimer = 0;
                maxStacksTriggeredThisApplication = false;
            }
        }

        public override void AI(NPC npc)
        {
            if (!resonantBurn) return;

            // Visual intensity scales with stacks (20% base + 16% per stack = 100% at 5 stacks)
            float stackIntensity = 0.2f + (burnStacks * 0.16f);

            // Spawn visual effects during AI update (more reliable than DrawEffects)
            // Rainbow flame particles - more frequent at higher stacks
            int flameChance = Math.Max(1, 3 - burnStacks / 2);
            if (Main.rand.NextBool(flameChance))
            {
                Vector2 pos = npc.Center + Main.rand.NextVector2Circular(npc.width * 0.4f, npc.height * 0.4f);
                float hue = (Main.GameUpdateCount * 0.03f + Main.rand.NextFloat()) % 1f;
                Color rainbowFlame = Main.hslToRgb(hue, 0.8f, 0.75f);

                Dust dust = Dust.NewDustPerfect(pos, DustID.RainbowMk2, new Vector2(0, -Main.rand.NextFloat(1f, 2.5f)));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1.2f, 1.8f) * (0.8f + stackIntensity * 0.4f);
                dust.color = rainbowFlame;
                dust.fadeIn = 0.5f;
            }

            // Black/white flame accents
            int bwChance = Math.Max(2, 4 - burnStacks / 2);
            if (Main.rand.NextBool(bwChance))
            {
                Vector2 pos = npc.Center + Main.rand.NextVector2Circular(npc.width * 0.5f, npc.height * 0.5f);
                bool isBlack = Main.rand.NextBool();

                Dust dust = Dust.NewDustPerfect(pos, isBlack ? DustID.Smoke : DustID.WhiteTorch,
                    new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(1.5f, 3f)));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1.5f, 2.2f) * (0.8f + stackIntensity * 0.4f);
                if (isBlack) dust.color = Color.Black;
                dust.alpha = isBlack ? 150 : 30;
            }

            // Music note particles (try custom, fallback to dust)
            int noteChance = Math.Max(3, 8 - burnStacks);
            if (Main.rand.NextBool(noteChance))
            {
                try
                {
                    Vector2 pos = npc.Center + Main.rand.NextVector2Circular(npc.width * 0.6f, npc.height * 0.6f);
                    float hue = Main.rand.NextFloat();
                    Color noteColor = Main.hslToRgb(hue, 0.9f, 0.7f);
                    ThemedParticles.MusicNote(pos, Main.rand.NextVector2Circular(1f, 2f), noteColor, 0.3f * (0.8f + stackIntensity * 0.4f), 30);
                }
                catch
                {
                    // Fallback to dust if custom particles fail
                    Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.MagicMirror);
                    dust.noGravity = true;
                    dust.velocity = Main.rand.NextVector2Circular(1f, 2f);
                    dust.scale = 1.0f;
                }
            }

            // Max stacks burst effect - pulsing ring when at 5 stacks
            if (burnStacks >= MAX_STACKS && Main.GameUpdateCount % 20 == 0)
            {
                try
                {
                    float hue = (Main.GameUpdateCount * 0.02f) % 1f;
                    Color burstColor = Main.hslToRgb(hue, 0.9f, 0.8f);
                    CustomParticles.HaloRing(npc.Center, burstColor, 0.6f, 15);

                    // Extra music note burst at max stacks
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 noteVel = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -1f);
                        ThemedParticles.MusicNote(npc.Center, noteVel, burstColor, 0.4f, 40);
                    }
                }
                catch { }
            }

            // Rainbow lighting - intensity scales with stacks
            float hue2 = (Main.GameUpdateCount * 0.02f) % 1f;
            Lighting.AddLight(npc.Center, Main.hslToRgb(hue2, 0.8f, 0.6f).ToVector3() * (0.4f + stackIntensity * 0.4f));
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (resonantBurn)
            {
                // Tint the NPC with subtle rainbow shimmer - more intense at higher stacks
                float stackIntensity = 0.1f + (burnStacks * 0.02f);
                float hue = (Main.GameUpdateCount * 0.02f + npc.whoAmI * 0.1f) % 1f;
                Color rainbowTint = Main.hslToRgb(hue, 0.5f + (burnStacks * 0.08f), 0.9f);
                drawColor = Color.Lerp(drawColor, rainbowTint, stackIntensity);
            }
        }
    }

    /// <summary>
    /// ModPlayer for visual indicator when Resonance Burn is active on enemies
    /// </summary>
    public class ResonancePrefixPlayer : ModPlayer
    {
        public bool anyEnemyHasResonanceBurn = false;
        private float orbitAngle = 0f;

        public override void ResetEffects()
        {
            anyEnemyHasResonanceBurn = false;
        }

        public override void PostUpdate()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.HasBuff(ModContent.BuffType<ResonantBurnDebuff>()))
                {
                    if (Vector2.Distance(Player.Center, npc.Center) < 2000f)
                    {
                        anyEnemyHasResonanceBurn = true;
                        break;
                    }
                }
            }

            if (anyEnemyHasResonanceBurn)
            {
                orbitAngle += 0.06f;
                if (orbitAngle > MathHelper.TwoPi)
                    orbitAngle -= MathHelper.TwoPi;

                SpawnOrbitingFlare();
            }
        }

        private void SpawnOrbitingFlare()
        {
            float orbitRadius = 55f;

            Vector2 orbitPos = Player.Center + new Vector2(
                (float)Math.Cos(orbitAngle) * orbitRadius,
                (float)Math.Sin(orbitAngle) * orbitRadius
            );

            Vector2 orbitPos2 = Player.Center + new Vector2(
                (float)Math.Cos(orbitAngle + MathHelper.Pi) * orbitRadius,
                (float)Math.Sin(orbitAngle + MathHelper.Pi) * orbitRadius
            );

            float hue = (Main.GameUpdateCount * 0.025f) % 1f;
            Color rainbowColor = Main.hslToRgb(hue, 0.7f, 0.85f);

            Color flare1Color = Color.Lerp(Color.White, rainbowColor, 0.4f);
            CustomParticles.GenericFlare(orbitPos, flare1Color, 0.65f, 8);

            Color flare2Color = Color.Lerp(new Color(30, 30, 30), rainbowColor, 0.3f);
            CustomParticles.GenericFlare(orbitPos2, flare2Color, 0.55f, 8);

            if (Main.GameUpdateCount % 3 == 0)
            {
                Vector2 trailVel1 = -new Vector2((float)Math.Cos(orbitAngle), (float)Math.Sin(orbitAngle)) * 1.5f;
                CustomParticles.GenericGlow(orbitPos, trailVel1, Color.White * 0.6f, 0.35f, 15, true);

                Vector2 trailVel2 = -new Vector2((float)Math.Cos(orbitAngle + MathHelper.Pi), (float)Math.Sin(orbitAngle + MathHelper.Pi)) * 1.5f;
                CustomParticles.GenericGlow(orbitPos2, trailVel2, new Color(50, 50, 50) * 0.8f, 0.3f, 15, true);
            }

            if (Main.GameUpdateCount % 20 == 0)
            {
                CustomParticles.HaloRing(Player.Center, rainbowColor * 0.5f, 0.4f, 18);
            }

            if (Main.GameUpdateCount % 30 == 0)
            {
                Vector2 notePos = Player.Center + Main.rand.NextVector2Circular(40f, 40f);
                ThemedParticles.MusicNote(notePos, new Vector2(0, -1.5f), rainbowColor, 0.35f, 35);
            }

            Lighting.AddLight(Player.Center, rainbowColor.ToVector3() * 0.3f);
        }
    }

    #endregion

    #region Prefix Helper

    /// <summary>
    /// Helper class to check if an item has any Resonance prefix
    /// </summary>
    public static class ResonancePrefixHelper
    {
        public static bool HasResonancePrefix(Item item)
        {
            return item.prefix == ModContent.PrefixType<ResonanceSlicedPrefix>() ||
                   item.prefix == ModContent.PrefixType<ResonanceSearedPrefix>() ||
                   item.prefix == ModContent.PrefixType<ResonancePiercedPrefix>() ||
                   item.prefix == ModContent.PrefixType<ResonanceBornPrefix>();
        }

        public static Color GetPulsingRainbowColor()
        {
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.3f + 0.7f;
            float hue = (Main.GameUpdateCount * 0.015f) % 1f;
            Color rainbowBase = Main.hslToRgb(hue, 0.6f, 0.85f);
            Color mixed = Color.Lerp(rainbowBase, Main.rand.NextBool(3) ? Color.White : new Color(50, 50, 50), 0.2f);
            return mixed * pulse;
        }

        public static void SpawnHitVFX(Vector2 position)
        {
            float hue = Main.rand.NextFloat();
            Color rainbowFlare = Main.hslToRgb(hue, 0.9f, 0.8f);
            CustomParticles.GenericFlare(position, rainbowFlare, 0.6f, 20);
            CustomParticles.GenericFlare(position, Main.rand.NextBool() ? Color.White : Color.Black, 0.4f, 15);

            for (int i = 0; i < 3; i++)
            {
                Vector2 noteVel = Main.rand.NextVector2Circular(3f, 3f);
                Color noteColor = Main.hslToRgb(Main.rand.NextFloat(), 0.9f, 0.75f);
                ThemedParticles.MusicNote(position, noteVel, noteColor, 0.35f, 25);
            }
        }

        public static void ApplyBurnDebuff(NPC target, int damage, Player player = null)
        {
            var burnNPC = target.GetGlobalNPC<ResonantBurnNPC>();
            burnNPC.burnDamage = damage;
            target.AddBuff(ModContent.BuffType<ResonantBurnDebuff>(), 300);

            // Add a stack when applying burn (player required for stack event)
            if (player != null)
            {
                burnNPC.AddStack(target, player);
            }
        }

        /// <summary>
        /// Gets the current burn stack count on an NPC.
        /// </summary>
        public static int GetBurnStacks(NPC npc)
        {
            if (!npc.active || npc.friendly)
                return 0;

            var burnNPC = npc.GetGlobalNPC<ResonantBurnNPC>();
            return burnNPC.burnStacks;
        }

        /// <summary>
        /// Checks if an NPC has maximum (5) burn stacks.
        /// </summary>
        public static bool HasMaxBurnStacks(NPC npc)
        {
            if (!npc.active || npc.friendly)
                return false;

            var burnNPC = npc.GetGlobalNPC<ResonantBurnNPC>();
            return burnNPC.HasMaxStacks;
        }

        /// <summary>
        /// Consumes all burn stacks from an NPC. Returns the stack count before reset.
        /// </summary>
        public static int ConsumeBurnStacks(NPC npc)
        {
            if (!npc.active || npc.friendly)
                return 0;

            var burnNPC = npc.GetGlobalNPC<ResonantBurnNPC>();
            return burnNPC.ConsumeStacks();
        }

        /// <summary>
        /// Checks if an NPC currently has the Resonant Burn debuff active.
        /// </summary>
        public static bool IsEnemyBurning(NPC npc)
        {
            return npc.active && !npc.friendly && npc.HasBuff(ModContent.BuffType<ResonantBurnDebuff>());
        }

        /// <summary>
        /// Counts the number of active enemies with Resonant Burn debuff.
        /// </summary>
        public static int CountBurningEnemies()
        {
            int count = 0;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (IsEnemyBurning(npc))
                    count++;
            }
            return count;
        }

        /// <summary>
        /// Calculates the total remaining DoT damage from Resonant Burn on an NPC.
        /// </summary>
        public static int GetBurnDamageRemaining(NPC npc)
        {
            if (!npc.active || npc.friendly)
                return 0;

            int buffIndex = npc.FindBuffIndex(ModContent.BuffType<ResonantBurnDebuff>());
            if (buffIndex < 0)
                return 0;

            var burnNPC = npc.GetGlobalNPC<ResonantBurnNPC>();
            if (!burnNPC.resonantBurn)
                return 0;

            int ticksRemaining = npc.buffTime[buffIndex];
            int dps = Math.Max(8, (int)(burnNPC.burnDamage * 0.15f));
            return (dps * ticksRemaining) / 60;
        }

        /// <summary>
        /// Extends the duration of Resonant Burn on a target NPC.
        /// </summary>
        public static void ExtendBurnDuration(NPC npc, int additionalTicks)
        {
            int buffIndex = npc.FindBuffIndex(ModContent.BuffType<ResonantBurnDebuff>());
            if (buffIndex >= 0)
            {
                npc.buffTime[buffIndex] += additionalTicks;
            }
        }

        /// <summary>
        /// Spreads Resonant Burn to a nearby enemy.
        /// </summary>
        public static bool SpreadBurnToNearby(Vector2 position, int damage, float range, NPC sourceNPC)
        {
            float closestDist = range;
            NPC closestNPC = null;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc == sourceNPC)
                    continue;
                if (IsEnemyBurning(npc))
                    continue;

                float dist = Vector2.Distance(position, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestNPC = npc;
                }
            }

            if (closestNPC != null)
            {
                ApplyBurnDebuff(closestNPC, damage);
                SpawnHitVFX(closestNPC.Center);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Consumes Resonant Burn on a target, dealing burst damage and removing the debuff.
        /// </summary>
        public static int ConsumeBurnForBurst(NPC npc, float burstMultiplier)
        {
            int remainingDamage = GetBurnDamageRemaining(npc);
            if (remainingDamage <= 0)
                return 0;

            int burstDamage = (int)(remainingDamage * burstMultiplier);

            int buffIndex = npc.FindBuffIndex(ModContent.BuffType<ResonantBurnDebuff>());
            if (buffIndex >= 0)
            {
                npc.DelBuff(buffIndex);
            }

            var burnNPC = npc.GetGlobalNPC<ResonantBurnNPC>();
            burnNPC.resonantBurn = false;
            burnNPC.burnDamage = 0;

            return burstDamage;
        }

        /// <summary>
        /// Increases the damage multiplier for Resonant Burn DoT.
        /// </summary>
        public static void AmplifyBurnDamage(NPC npc, float multiplier)
        {
            var burnNPC = npc.GetGlobalNPC<ResonantBurnNPC>();
            if (burnNPC.resonantBurn && burnNPC.burnDamage > 0)
            {
                burnNPC.burnDamage = (int)(burnNPC.burnDamage * multiplier);
            }
        }
    }

    #endregion

    #region Melee Prefix - Resonance Sliced

    /// <summary>
    /// Resonance Sliced - Superior to Legendary
    /// Legendary: +17% dmg, +5% crit, +10% speed, +15% KB, +10% size
    /// Sliced:    +22% dmg, +8% crit, +15% speed, +20% KB, +12% size, +5% bonus attack speed
    /// </summary>
    public class ResonanceSlicedPrefix : ModPrefix
    {
        public override PrefixCategory Category => PrefixCategory.Melee;

        public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult,
            ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus)
        {
            damageMult = 1.22f;      // +22% damage (vs Legendary's +17%)
            critBonus = 8;           // +8% crit (vs Legendary's +5%)
            knockbackMult = 1.20f;   // +20% knockback (vs Legendary's +15%)
            useTimeMult = 0.85f;     // +15% speed (vs Legendary's +10%)
            scaleMult = 1.12f;       // +12% size (vs Legendary's +10%)
        }

        public override void ModifyValue(ref float valueMult)
        {
            valueMult = 4.0f;
        }

        public override float RollChance(Item item)
        {
            return 0.02f;
        }

        public override bool CanRoll(Item item)
        {
            return item.damage > 0 && !item.accessory &&
                   (item.DamageType == DamageClass.Melee || item.DamageType == DamageClass.MeleeNoSpeed);
        }

        public override bool AllStatChangesHaveEffectOn(Item item)
        {
            return true;
        }
    }

    #endregion

    #region Magic Prefix - Resonance Seared

    /// <summary>
    /// Resonance Seared - Superior to Mythical
    /// Mythical: +15% dmg, +5% crit, +10% speed, +15% KB, -10% mana
    /// Seared:   +20% dmg, +10% crit, +12% speed, +15% KB, -18% mana, +25 mana regen
    /// </summary>
    public class ResonanceSearedPrefix : ModPrefix
    {
        public override PrefixCategory Category => PrefixCategory.Magic;

        public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult,
            ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus)
        {
            damageMult = 1.20f;      // +20% damage (vs Mythical's +15%)
            critBonus = 10;          // +10% crit (vs Mythical's +5%)
            knockbackMult = 1.15f;   // +15% knockback
            useTimeMult = 0.88f;     // +12% speed (vs Mythical's +10%)
            manaMult = 0.82f;        // -18% mana (vs Mythical's -10%)
        }

        public override void ModifyValue(ref float valueMult)
        {
            valueMult = 4.0f;
        }

        public override float RollChance(Item item)
        {
            return 0.02f;
        }

        public override bool CanRoll(Item item)
        {
            return item.damage > 0 && !item.accessory && item.DamageType == DamageClass.Magic;
        }

        public override bool AllStatChangesHaveEffectOn(Item item)
        {
            return true;
        }
    }

    #endregion

    #region Ranged Prefix - Resonance Pierced

    /// <summary>
    /// Resonance Pierced - Superior to Unreal
    /// Unreal:  +15% dmg, +5% crit, +10% speed, +15% KB, +10% velocity
    /// Pierced: +20% dmg, +8% crit, +12% speed, +18% KB, +15% velocity, +10 armor pen
    /// </summary>
    public class ResonancePiercedPrefix : ModPrefix
    {
        public override PrefixCategory Category => PrefixCategory.Ranged;

        public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult,
            ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus)
        {
            damageMult = 1.20f;      // +20% damage (vs Unreal's +15%)
            critBonus = 8;           // +8% crit (vs Unreal's +5%)
            knockbackMult = 1.18f;   // +18% knockback (vs Unreal's +15%)
            useTimeMult = 0.88f;     // +12% speed (vs Unreal's +10%)
            shootSpeedMult = 1.15f;  // +15% velocity (vs Unreal's +10%)
        }

        public override void ModifyValue(ref float valueMult)
        {
            valueMult = 4.0f;
        }

        public override float RollChance(Item item)
        {
            return 0.02f;
        }

        public override bool CanRoll(Item item)
        {
            return item.damage > 0 && !item.accessory && item.DamageType == DamageClass.Ranged;
        }

        public override bool AllStatChangesHaveEffectOn(Item item)
        {
            return true;
        }
    }

    #endregion

    #region Summon Prefix - Resonance Born

    /// <summary>
    /// Resonance Born - Superior to Ruthless
    /// Ruthless: +18% dmg, no other bonuses
    /// Born:     +25% dmg, +15% KB, +10% summon crit while held
    /// </summary>
    public class ResonanceBornPrefix : ModPrefix
    {
        // Summon uses AnyWeapon category but restricts via CanRoll
        public override PrefixCategory Category => PrefixCategory.AnyWeapon;

        public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult,
            ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus)
        {
            damageMult = 1.25f;      // +25% damage (vs Ruthless's +18%)
            knockbackMult = 1.15f;   // +15% knockback (Ruthless has none)
        }

        public override void ModifyValue(ref float valueMult)
        {
            valueMult = 4.0f;
        }

        public override float RollChance(Item item)
        {
            return 0.02f;
        }

        public override bool CanRoll(Item item)
        {
            return item.damage > 0 && !item.accessory &&
                   (item.DamageType == DamageClass.Summon || item.DamageType == DamageClass.SummonMeleeSpeed);
        }

        public override bool AllStatChangesHaveEffectOn(Item item)
        {
            return true;
        }
    }

    #endregion

    #region Global Item for Held Bonuses and Tooltips

    /// <summary>
    /// Handles held bonuses, tooltips, and on-hit effects for all Resonance prefixes
    /// </summary>
    public class ResonancePrefixGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => true;

        public override void HoldItem(Item item, Player player)
        {
            if (!ResonancePrefixHelper.HasResonancePrefix(item) || item.damage <= 0)
                return;

            // Melee: +5% bonus attack speed
            if (item.prefix == ModContent.PrefixType<ResonanceSlicedPrefix>())
            {
                player.GetAttackSpeed(DamageClass.Melee) += 0.05f;
            }
            // Magic: +25 mana regen
            else if (item.prefix == ModContent.PrefixType<ResonanceSearedPrefix>())
            {
                player.manaRegenBonus += 25;
            }
            // Ranged: +10 armor penetration
            else if (item.prefix == ModContent.PrefixType<ResonancePiercedPrefix>())
            {
                player.GetArmorPenetration(DamageClass.Ranged) += 10;
            }
            // Summon: +10% summon crit
            else if (item.prefix == ModContent.PrefixType<ResonanceBornPrefix>())
            {
                player.GetCritChance(DamageClass.Summon) += 10;
            }
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (!ResonancePrefixHelper.HasResonancePrefix(item))
                return;

            // Color prefix stat lines with pulsing rainbow
            foreach (TooltipLine line in tooltips)
            {
                if (line.Name == "PrefixDamage" || line.Name == "PrefixSpeed" ||
                    line.Name == "PrefixCritChance" || line.Name == "PrefixUseMana" ||
                    line.Name == "PrefixSize" || line.Name == "PrefixShootSpeed" ||
                    line.Name == "PrefixKnockback" || (line.Mod == "Terraria" && line.Name.Contains("Prefix")))
                {
                    line.OverrideColor = ResonancePrefixHelper.GetPulsingRainbowColor();
                }
            }

            // Add class-specific bonus tooltip
            string classBonus = GetClassBonusText(item);
            if (!string.IsNullOrEmpty(classBonus))
            {
                TooltipLine classLine = new TooltipLine(Mod, "ResonanceClassBonus", classBonus)
                {
                    OverrideColor = Main.hslToRgb((Main.GameUpdateCount * 0.015f + 0.3f) % 1f, 0.8f, 0.75f)
                };
                tooltips.Add(classLine);
            }

            // Add special effect tooltip
            TooltipLine specialLine = new TooltipLine(Mod, "ResonanceEffect",
                "Inflicts Resonance Burn - rainbow flames and musical echoes")
            {
                OverrideColor = Main.hslToRgb((Main.GameUpdateCount * 0.015f) % 1f, 0.7f, 0.8f)
            };
            tooltips.Add(specialLine);

            // Add lore line based on prefix type
            string lore = GetLoreText(item);
            TooltipLine loreLine = new TooltipLine(Mod, "ResonanceLore", lore)
            {
                OverrideColor = Color.Lerp(Color.White, Color.Gray, 0.3f)
            };
            tooltips.Add(loreLine);
        }

        private string GetClassBonusText(Item item)
        {
            if (item.prefix == ModContent.PrefixType<ResonanceSlicedPrefix>())
                return "[Melee] Superior to Legendary - +5% bonus attack speed while held";
            else if (item.prefix == ModContent.PrefixType<ResonanceSearedPrefix>())
                return "[Magic] Superior to Mythical - +25 mana regen while held";
            else if (item.prefix == ModContent.PrefixType<ResonancePiercedPrefix>())
                return "[Ranged] Superior to Unreal - +10 armor penetration while held";
            else if (item.prefix == ModContent.PrefixType<ResonanceBornPrefix>())
                return "[Summon] Superior to Ruthless - +10% summon crit while held";
            return "";
        }

        private string GetLoreText(Item item)
        {
            if (item.prefix == ModContent.PrefixType<ResonanceSlicedPrefix>())
                return "'Each cut echoes through eternity'";
            else if (item.prefix == ModContent.PrefixType<ResonanceSearedPrefix>())
                return "'Arcane fire that burns to the rhythm'";
            else if (item.prefix == ModContent.PrefixType<ResonancePiercedPrefix>())
                return "'No armor can silence this melody'";
            else if (item.prefix == ModContent.PrefixType<ResonanceBornPrefix>())
                return "'Spirits awakened by the conductor's will'";
            return "'The mark of a true maestro'";
        }

        public override void OnHitNPC(Item item, Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (ResonancePrefixHelper.HasResonancePrefix(item))
            {
                ResonancePrefixHelper.ApplyBurnDebuff(target, item.damage, player);
                ResonancePrefixHelper.SpawnHitVFX(target.Center);
            }
        }
    }

    #endregion

    #region Global Projectile for Ranged/Magic/Summon On-Hit

    /// <summary>
    /// Handles projectile trails and on-hit effects for Resonance prefix weapons
    /// </summary>
    public class ResonancePrefixGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public bool fromResonanceWeapon = false;
        private int ownerIndex = -1;

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (source is EntitySource_ItemUse itemSource)
            {
                if (ResonancePrefixHelper.HasResonancePrefix(itemSource.Item))
                {
                    fromResonanceWeapon = true;
                    ownerIndex = projectile.owner;
                }
            }
            else if (source is EntitySource_Parent parentSource && parentSource.Entity is Projectile parentProj)
            {
                // Inherit resonance from parent projectile (for split projectiles, etc.)
                var parentGlobal = parentProj.GetGlobalProjectile<ResonancePrefixGlobalProjectile>();
                if (parentGlobal.fromResonanceWeapon)
                {
                    fromResonanceWeapon = true;
                    ownerIndex = parentGlobal.ownerIndex;
                }
            }
        }

        public override void AI(Projectile projectile)
        {
            if (fromResonanceWeapon && !projectile.hostile && projectile.friendly)
            {
                // Pale rainbow trail
                if (Main.rand.NextBool(4))
                {
                    float hue = (Main.GameUpdateCount * 0.03f + Main.rand.NextFloat()) % 1f;
                    Color trailColor = Main.hslToRgb(hue, 0.5f, 0.8f) * 0.6f;
                    CustomParticles.GenericGlow(projectile.Center, -projectile.velocity * 0.08f, trailColor, 0.2f, 18, true);
                }

                // Black/white flame accents
                if (Main.rand.NextBool(8))
                {
                    Color bwFlame = Main.rand.NextBool() ? Color.White * 0.5f : new Color(40, 40, 40) * 0.7f;
                    CustomParticles.GenericFlare(projectile.Center, bwFlame, 0.18f, 12);
                }
            }
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (fromResonanceWeapon)
            {
                Player player = ownerIndex >= 0 && ownerIndex < Main.maxPlayers ? Main.player[ownerIndex] : null;
                ResonancePrefixHelper.ApplyBurnDebuff(target, projectile.damage, player);
                ResonancePrefixHelper.SpawnHitVFX(target.Center);
            }
        }
    }

    #endregion
}
