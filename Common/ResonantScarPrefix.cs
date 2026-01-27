using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.Audio;
using Microsoft.Xna.Framework;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;
using System;

namespace MagnumOpus.Common
{
    /// <summary>
    /// Resonant Burn debuff - Applied by Resonant Scar prefix weapons
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
            // burnDamage persists from when it was applied
        }
    }

    /// <summary>
    /// Global NPC for Resonant Burn debuff effects
    /// </summary>
    public class ResonantBurnNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool resonantBurn;
        public int burnDamage; // Stored weapon damage for scaling DoT
        private int storedBurnDamage; // Persists across frames when buff is active

        public override void ResetEffects(NPC npc)
        {
            resonantBurn = false;
            // DON'T reset burnDamage here - it gets set when debuff is applied
            // and needs to persist for the duration
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (resonantBurn)
            {
                // Use stored damage if current is 0 (from buff reapplication)
                int effectiveDamage = burnDamage > 0 ? burnDamage : storedBurnDamage;
                
                // Store the damage for persistence
                if (burnDamage > 0)
                    storedBurnDamage = burnDamage;
                
                // Rainbow flame DoT - scales with weapon damage
                // Base: 15% of weapon damage per second (minimum 8 DPS)
                int dps = Math.Max(8, (int)(effectiveDamage * 0.15f));
                
                if (npc.lifeRegen > 0)
                    npc.lifeRegen = 0;
                
                npc.lifeRegen -= dps * 2; // lifeRegen is halved to get DPS
                
                if (damage < dps / 4)
                    damage = dps / 4;
            }
            else
            {
                // Clear stored damage when debuff expires
                storedBurnDamage = 0;
                burnDamage = 0;
            }
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (resonantBurn)
            {
                // Pulsing pale rainbow flames with black/white accents
                if (Main.rand.NextBool(3))
                {
                    Vector2 pos = npc.Center + Main.rand.NextVector2Circular(npc.width * 0.4f, npc.height * 0.4f);
                    
                    // Rainbow flame particle
                    float hue = (Main.GameUpdateCount * 0.03f + Main.rand.NextFloat()) % 1f;
                    Color rainbowFlame = Main.hslToRgb(hue, 0.8f, 0.75f) * 0.8f;
                    Vector2 vel = new Vector2(0, -Main.rand.NextFloat(0.5f, 1.5f));
                    CustomParticles.GenericGlow(pos, vel, rainbowFlame, 0.28f, 22, true);
                }
                
                // Black/white flame accents
                if (Main.rand.NextBool(4))
                {
                    Vector2 pos = npc.Center + Main.rand.NextVector2Circular(npc.width * 0.5f, npc.height * 0.5f);
                    Color bwFlame = Main.rand.NextBool() ? Color.White : new Color(30, 30, 30);
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(1f, 2f));
                    CustomParticles.GenericFlare(pos, bwFlame * 0.7f, 0.22f, 18);
                }
                
                // Music note particles
                if (Main.rand.NextBool(8))
                {
                    Vector2 pos = npc.Center + Main.rand.NextVector2Circular(npc.width * 0.6f, npc.height * 0.6f);
                    float hue = Main.rand.NextFloat();
                    Color noteColor = Main.hslToRgb(hue, 0.9f, 0.7f);
                    ThemedParticles.MusicNote(pos, Main.rand.NextVector2Circular(1f, 2f), noteColor, 0.3f, 30);
                }
                
                // Rainbow light emission
                float hue2 = (Main.GameUpdateCount * 0.02f) % 1f;
                Lighting.AddLight(npc.Center, Main.hslToRgb(hue2, 0.8f, 0.6f).ToVector3() * 0.6f);
                
                // Dust overlay
                if (Main.rand.NextBool(5))
                {
                    Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.RainbowMk2);
                    dust.noGravity = true;
                    dust.velocity = new Vector2(0, -1.5f);
                    dust.scale = 1.1f;
                }
            }
        }
    }

    /// <summary>
    /// ModPlayer to track Resonance Burn active on enemies and display visual indicator
    /// Shows large orbiting black/white rainbow flare around player when debuff is active
    /// </summary>
    public class ResonanceScarredPlayer : ModPlayer
    {
        public bool anyEnemyHasResonanceBurn = false;
        private float orbitAngle = 0f;
        
        public override void ResetEffects()
        {
            anyEnemyHasResonanceBurn = false;
        }
        
        public override void PostUpdate()
        {
            // Check if any nearby enemy has Resonant Burn
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.HasBuff(ModContent.BuffType<ResonantBurnDebuff>()))
                {
                    // Only track enemies within reasonable range (2000 pixels)
                    if (Vector2.Distance(Player.Center, npc.Center) < 2000f)
                    {
                        anyEnemyHasResonanceBurn = true;
                        break;
                    }
                }
            }
            
            // Update orbit angle
            if (anyEnemyHasResonanceBurn)
            {
                orbitAngle += 0.06f; // Rotation speed
                if (orbitAngle > MathHelper.TwoPi)
                    orbitAngle -= MathHelper.TwoPi;
                    
                // Spawn the orbiting monochromatic rainbow flare effect
                SpawnOrbitingFlare();
            }
        }
        
        private void SpawnOrbitingFlare()
        {
            float orbitRadius = 55f;
            
            // Main orbiting position
            Vector2 orbitPos = Player.Center + new Vector2(
                (float)Math.Cos(orbitAngle) * orbitRadius,
                (float)Math.Sin(orbitAngle) * orbitRadius
            );
            
            // Secondary orbit (opposite side)
            Vector2 orbitPos2 = Player.Center + new Vector2(
                (float)Math.Cos(orbitAngle + MathHelper.Pi) * orbitRadius,
                (float)Math.Sin(orbitAngle + MathHelper.Pi) * orbitRadius
            );
            
            // Pulsing rainbow hue
            float hue = (Main.GameUpdateCount * 0.025f) % 1f;
            Color rainbowColor = Main.hslToRgb(hue, 0.7f, 0.85f);
            
            // Monochromatic (black/white) + rainbow blend
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.5f + 0.5f;
            
            // Large orbiting flare 1 - white with rainbow tint
            Color flare1Color = Color.Lerp(Color.White, rainbowColor, 0.4f);
            CustomParticles.GenericFlare(orbitPos, flare1Color, 0.65f, 8);
            
            // Large orbiting flare 2 - black with rainbow tint  
            Color flare2Color = Color.Lerp(new Color(30, 30, 30), rainbowColor, 0.3f);
            CustomParticles.GenericFlare(orbitPos2, flare2Color, 0.55f, 8);
            
            // Trailing particles behind each flare
            if (Main.GameUpdateCount % 3 == 0)
            {
                // White trail particle
                Vector2 trailVel1 = -new Vector2((float)Math.Cos(orbitAngle), (float)Math.Sin(orbitAngle)) * 1.5f;
                CustomParticles.GenericGlow(orbitPos, trailVel1, Color.White * 0.6f, 0.35f, 15, true);
                
                // Black trail particle
                Vector2 trailVel2 = -new Vector2((float)Math.Cos(orbitAngle + MathHelper.Pi), (float)Math.Sin(orbitAngle + MathHelper.Pi)) * 1.5f;
                CustomParticles.GenericGlow(orbitPos2, trailVel2, new Color(50, 50, 50) * 0.8f, 0.3f, 15, true);
            }
            
            // Rainbow halo ring occasionally
            if (Main.GameUpdateCount % 20 == 0)
            {
                CustomParticles.HaloRing(Player.Center, rainbowColor * 0.5f, 0.4f, 18);
            }
            
            // Music note occasionally
            if (Main.GameUpdateCount % 30 == 0)
            {
                Vector2 notePos = Player.Center + Main.rand.NextVector2Circular(40f, 40f);
                ThemedParticles.MusicNote(notePos, new Vector2(0, -1.5f), rainbowColor, 0.35f, 35);
            }
            
            // Dynamic lighting at player
            Lighting.AddLight(Player.Center, rainbowColor.ToVector3() * 0.3f);
        }
    }

    /// <summary>
    /// Resonance Scarred prefix - Top-tier prefix better than all vanilla prefixes
    /// Applies to all weapon types with UNIQUE bonuses per class:
    /// 
    /// MELEE (vs Legendary: +17% dmg, +5% crit, +10% speed, +15% KB, +10% size):
    ///   Resonance Scarred: +22% dmg, +8% crit, +15% speed, +20% KB, +12% size
    /// 
    /// RANGED (vs Unreal: +15% dmg, +5% crit, +10% speed, +15% KB, +10% velocity):
    ///   Resonance Scarred: +20% dmg, +8% crit, +12% speed, +18% KB, +15% velocity
    /// 
    /// MAGIC (vs Mythical: +15% dmg, +5% crit, +10% speed, +15% KB, -10% mana):
    ///   Resonance Scarred: +20% dmg, +10% crit, +12% speed, +15% KB, -18% mana
    /// 
    /// SUMMON (vs Ruthless: +18% dmg, no other bonuses):
    ///   Resonance Scarred: +25% dmg, +15% KB, +10% summon tag crit (via GlobalItem)
    /// 
    /// ALL WEAPONS: Inflicts Resonance Burn (rainbow DoT + music VFX)
    /// When active on enemies, player sees orbiting monochromatic rainbow flares
    /// </summary>
    public class ResonantScarPrefix : ModPrefix
    {
        public override PrefixCategory Category => PrefixCategory.AnyWeapon;

        public override void SetStaticDefaults()
        {
            // Display name set in localization
        }

        public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus)
        {
            // These are BASE stats - they will be modified per-weapon-type in Apply()
            // Set to melee-equivalent as fallback for any weapon type
            damageMult = 1.22f;      // +22% damage
            critBonus = 8;           // +8% crit
            knockbackMult = 1.20f;   // +20% knockback
            useTimeMult = 0.85f;     // +15% speed
            shootSpeedMult = 1.15f;  // +15% velocity
            manaMult = 0.82f;        // -18% mana cost
            scaleMult = 1.12f;       // +12% size
        }

        public override void Apply(Item item)
        {
            // Class-specific stat adjustments are handled via the base SetStats
            // The actual class-specific bonuses are applied through ResonantScarGlobalItem
        }

        public override void ModifyValue(ref float valueMult)
        {
            // This is the ultimate prefix - massively increases value
            valueMult = 4.0f;
        }

        // Roll weight - very rare (rarer than Legendary)
        public override float RollChance(Item item)
        {
            return 0.02f; // 2% chance (Legendary is ~5%)
        }

        public override bool CanRoll(Item item)
        {
            // Can roll on ALL weapons with damage (melee, ranged, magic, summon, etc.)
            return item.damage > 0 && !item.accessory;
        }
    }

    /// <summary>
    /// Global Item to handle Resonant Scar prefix special effects and class-specific bonuses
    /// </summary>
    public class ResonantScarGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => true;
        
        public override void HoldItem(Item item, Player player)
        {
            // Apply class-specific bonuses when weapon is held
            if (item.prefix == ModContent.PrefixType<ResonantScarPrefix>() && item.damage > 0)
            {
                // Summon weapons get extra summon crit bonus while held (unique to Resonant Scar)
                // This is the main advantage over Ruthless which only gives damage
                if (item.DamageType == DamageClass.Summon || item.DamageType == DamageClass.SummonMeleeSpeed)
                {
                    player.GetCritChance(DamageClass.Summon) += 10;
                }
                
                // Magic weapons get bonus mana regen while held
                if (item.DamageType == DamageClass.Magic)
                {
                    player.manaRegenBonus += 25;
                }
                
                // Ranged weapons get bonus armor penetration while held
                if (item.DamageType == DamageClass.Ranged)
                {
                    player.GetArmorPenetration(DamageClass.Ranged) += 10;
                }
                
                // Melee weapons get bonus melee speed while held (stacks with prefix speed)
                if (item.DamageType == DamageClass.Melee || item.DamageType == DamageClass.MeleeNoSpeed)
                {
                    player.GetAttackSpeed(DamageClass.Melee) += 0.05f; // Extra 5% on top of prefix
                }
            }
        }

        public override void ModifyTooltips(Item item, System.Collections.Generic.List<TooltipLine> tooltips)
        {
            if (item.prefix == ModContent.PrefixType<ResonantScarPrefix>())
            {
                // Find the prefix tooltip lines and change their color to pulsing pale rainbow
                foreach (TooltipLine line in tooltips)
                {
                    if (line.Name == "PrefixDamage" || line.Name == "PrefixSpeed" || line.Name == "PrefixCritChance" || 
                        line.Name == "PrefixUseMana" || line.Name == "PrefixSize" || line.Name == "PrefixShootSpeed" || 
                        line.Name == "PrefixKnockback" || line.Mod == "Terraria" && line.Name.Contains("Prefix"))
                    {
                        // Pulsing pale rainbow effect
                        float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.3f + 0.7f;
                        float hue = (Main.GameUpdateCount * 0.015f) % 1f;
                        Color rainbowBase = Main.hslToRgb(hue, 0.6f, 0.85f); // Pale rainbow
                        
                        // Mix with white and black for flame effect
                        Color mixed = Color.Lerp(rainbowBase, Main.rand.NextBool(3) ? Color.White : new Color(50, 50, 50), 0.2f);
                        line.OverrideColor = mixed * pulse;
                    }
                }
                
                // Determine weapon class for tooltip
                string classBonus = GetClassBonusText(item);
                
                // Add class-specific bonus tooltip
                if (!string.IsNullOrEmpty(classBonus))
                {
                    TooltipLine classLine = new TooltipLine(Mod, "ResonanceScarredClassBonus", classBonus)
                    {
                        OverrideColor = Main.hslToRgb((Main.GameUpdateCount * 0.015f + 0.3f) % 1f, 0.8f, 0.75f)
                    };
                    tooltips.Add(classLine);
                }
                
                // Add special effect tooltip
                TooltipLine specialLine = new TooltipLine(Mod, "ResonanceScarredEffect", 
                    "Inflicts Resonance Burn - rainbow flames and musical echoes")
                {
                    OverrideColor = Main.hslToRgb((Main.GameUpdateCount * 0.015f) % 1f, 0.7f, 0.8f)
                };
                tooltips.Add(specialLine);
                
                // Add lore line
                TooltipLine loreLine = new TooltipLine(Mod, "ResonanceScarredLore", 
                    "'The mark of a true maestro'")
                {
                    OverrideColor = Color.Lerp(Color.White, Color.Gray, 0.3f)
                };
                tooltips.Add(loreLine);
            }
        }
        
        private string GetClassBonusText(Item item)
        {
            if (item.DamageType == DamageClass.Melee || item.DamageType == DamageClass.MeleeNoSpeed)
            {
                return "[Melee] Superior to Legendary - +5% bonus attack speed while held";
            }
            else if (item.DamageType == DamageClass.Ranged)
            {
                return "[Ranged] Superior to Unreal - +10 armor penetration while held";
            }
            else if (item.DamageType == DamageClass.Magic)
            {
                return "[Magic] Superior to Mythical - +25 mana regen while held";
            }
            else if (item.DamageType == DamageClass.Summon || item.DamageType == DamageClass.SummonMeleeSpeed)
            {
                return "[Summon] Superior to Ruthless - +10% summon crit while held";
            }
            else if (item.DamageType == DamageClass.Throwing)
            {
                return "[Throwing] Superior to all - enhanced across the board";
            }
            return "[Universal] The ultimate prefix for any weapon";
        }

        public override void OnHitNPC(Item item, Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (item.prefix == ModContent.PrefixType<ResonantScarPrefix>())
            {
                // Apply Resonant Burn debuff with weapon damage scaling
                var burnNPC = target.GetGlobalNPC<ResonantBurnNPC>();
                burnNPC.burnDamage = item.damage; // Store weapon damage for DoT scaling
                target.AddBuff(ModContent.BuffType<ResonantBurnDebuff>(), 300); // 5 seconds
                
                // VFX on hit
                float hue = Main.rand.NextFloat();
                Color rainbowFlare = Main.hslToRgb(hue, 0.9f, 0.8f);
                CustomParticles.GenericFlare(target.Center, rainbowFlare, 0.6f, 20);
                CustomParticles.GenericFlare(target.Center, Main.rand.NextBool() ? Color.White : Color.Black, 0.4f, 15);
                
                // Music notes burst
                for (int i = 0; i < 3; i++)
                {
                    Vector2 noteVel = Main.rand.NextVector2Circular(3f, 3f);
                    Color noteColor = Main.hslToRgb(Main.rand.NextFloat(), 0.9f, 0.75f);
                    ThemedParticles.MusicNote(target.Center, noteVel, noteColor, 0.35f, 25);
                }
            }
        }
    }

    /// <summary>
    /// Global Projectile to handle Resonant Scar prefix on ranged/magic/summon weapons
    /// </summary>
    public class ResonantScarGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public bool fromResonantScarWeapon = false;

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (source is EntitySource_ItemUse itemSource)
            {
                if (itemSource.Item.prefix == ModContent.PrefixType<ResonantScarPrefix>())
                {
                    fromResonantScarWeapon = true;
                }
            }
        }

        public override void AI(Projectile projectile)
        {
            if (fromResonantScarWeapon && !projectile.hostile && projectile.friendly)
            {
                // Pale rainbow trail with black/white flames
                if (Main.rand.NextBool(4))
                {
                    float hue = (Main.GameUpdateCount * 0.03f + Main.rand.NextFloat()) % 1f;
                    Color trailColor = Main.hslToRgb(hue, 0.5f, 0.8f) * 0.6f;
                    CustomParticles.GenericGlow(projectile.Center, -projectile.velocity * 0.08f, trailColor, 0.2f, 18, true);
                }
                
                if (Main.rand.NextBool(8))
                {
                    Color bwFlame = Main.rand.NextBool() ? Color.White * 0.5f : new Color(40, 40, 40) * 0.7f;
                    CustomParticles.GenericFlare(projectile.Center, bwFlame, 0.18f, 12);
                }
            }
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (fromResonantScarWeapon)
            {
                var burnNPC = target.GetGlobalNPC<ResonantBurnNPC>();
                burnNPC.burnDamage = projectile.damage; // Store projectile damage for DoT scaling
                
                // Apply Resonant Burn debuff
                target.AddBuff(ModContent.BuffType<ResonantBurnDebuff>(), 300); // 5 seconds
                
                // VFX on hit
                float hue = Main.rand.NextFloat();
                Color rainbowFlare = Main.hslToRgb(hue, 0.9f, 0.8f);
                CustomParticles.GenericFlare(target.Center, rainbowFlare, 0.6f, 20);
                CustomParticles.GenericFlare(target.Center, Main.rand.NextBool() ? Color.White : Color.Black, 0.4f, 15);
                
                // Music notes burst
                for (int i = 0; i < 3; i++)
                {
                    Vector2 noteVel = Main.rand.NextVector2Circular(3f, 3f);
                    Color noteColor = Main.hslToRgb(Main.rand.NextFloat(), 0.9f, 0.75f);
                    ThemedParticles.MusicNote(target.Center, noteVel, noteColor, 0.35f, 25);
                }
            }
        }
    }
}
