using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Nachtmusik.Debuffs;
using MagnumOpus.Content.Nachtmusik.Projectiles;

namespace MagnumOpus.Content.Nachtmusik.ResonantWeapons
{
    /// <summary>
    /// Nocturnal Executioner - The ultimate melee weapon from Nachtmusik.
    /// Massive cosmic blade that shreds enemies with spectral slashes.
    /// DAMAGE: 920 (higher than Fate weapons ~780-850)
    /// </summary>
    public class NocturnalExecutioner : ModItem
    {
        private int comboCounter = 0;
        private int executionCharge = 0;
        
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }
        
        public override void SetDefaults()
        {
            Item.width = 70;
            Item.height = 70;
            Item.damage = 920; // POST-FATE tier damage
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 7.5f;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.UseSound = SoundID.Item71;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.shoot = ModContent.ProjectileType<NocturnalBladeProjectile>();
            Item.shootSpeed = 15f;
            Item.scale = 1.5f;
            Item.crit = 18;
        }
        
        public override bool AltFunctionUse(Player player) => true;
        
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Execution Strike - costs 50 execution charge
                if (executionCharge < 50)
                    return false;
                    
                Item.useTime = 30;
                Item.useAnimation = 30;
                Item.UseSound = SoundID.Item122;
            }
            else
            {
                Item.useTime = 16;
                Item.useAnimation = 16;
                Item.UseSound = SoundID.Item71;
            }
            return base.CanUseItem(player);
        }
        
        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            // Execution Strike does 2.5x damage
            if (player.altFunctionUse == 2)
            {
                damage *= 2.5f;
            }
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2 && executionCharge >= 50)
            {
                // Execution Strike - massive spectral blade
                executionCharge -= 50;
                
                Vector2 direction = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
                
                // Fire 5 spectral blades in a fan
                for (int i = -2; i <= 2; i++)
                {
                    float angleOffset = MathHelper.ToRadians(12f * i);
                    Vector2 adjustedVel = direction.RotatedBy(angleOffset) * 18f;
                    Projectile.NewProjectile(source, player.Center, adjustedVel, type, damage, knockback, player.whoAmI);
                }
                
                // Massive VFX burst
                NachtmusikCosmicVFX.SpawnCelestialExplosion(player.Center + direction * 50f, 1.5f);
                MagnumScreenEffects.AddScreenShake(12f);
                
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.3f, Volume = 1.2f }, player.Center);
                
                return false;
            }
            else
            {
                // Normal swing - fire a blade every 3rd hit
                comboCounter++;
                if (comboCounter >= 3)
                {
                    comboCounter = 0;
                    Vector2 direction = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
                    Projectile.NewProjectile(source, player.Center, direction * velocity.Length(), type, damage / 2, knockback, player.whoAmI);
                }
                
                return false;
            }
        }
        
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // Cosmic swing trail
            if (Main.rand.NextBool())
            {
                Vector2 trailPos = hitbox.Center.ToVector2() + Main.rand.NextVector2Circular(hitbox.Width / 3f, hitbox.Height / 3f);
                Color trailColor = NachtmusikCosmicVFX.GetCelestialGradient(Main.rand.NextFloat());
                
                var trail = new GenericGlowParticle(trailPos, player.velocity * 0.3f + Main.rand.NextVector2Circular(1f, 1f),
                    trailColor * 0.7f, 0.35f, 15, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // Music notes in trail
            if (Main.rand.NextBool(4))
            {
                Vector2 notePos = hitbox.Center.ToVector2() + Main.rand.NextVector2Circular(20f, 20f);
                ThemedParticles.MusicNote(notePos, Main.rand.NextVector2Circular(2f, 2f), NachtmusikCosmicVFX.Gold, 0.3f, 20);
            }
            
            // Dust
            Dust dust = Dust.NewDustPerfect(hitbox.Center.ToVector2(), DustID.PurpleTorch, Main.rand.NextVector2Circular(3f, 3f), 0, default, 1.1f);
            dust.noGravity = true;
        }
        
        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Build execution charge on hit
            executionCharge = Math.Min(100, executionCharge + 8);
            
            // Apply Celestial Harmony
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 480);
            target.GetGlobalNPC<CelestialHarmonyNPC>().AddStack(target, 2);
            
            // Impact VFX
            NachtmusikCosmicVFX.SpawnCelestialImpact(target.Center, 1.2f);
            
            // Glyph burst on crit
            if (hit.Crit)
            {
                NachtmusikCosmicVFX.SpawnGlyphBurst(target.Center, 6, 5f, 0.4f);
                executionCharge = Math.Min(100, executionCharge + 15);
            }
        }
        
        public override void HoldItem(Player player)
        {
            // Execution charge indicator
            if (executionCharge > 0)
            {
                float chargePercent = executionCharge / 100f;
                
                // Orbiting charge particles
                if (Main.rand.NextBool((int)(8 - chargePercent * 5)))
                {
                    float angle = Main.GameUpdateCount * 0.05f;
                    for (int i = 0; i < (int)(chargePercent * 3) + 1; i++)
                    {
                        float particleAngle = angle + MathHelper.TwoPi * i / 3f;
                        Vector2 particlePos = player.Center + particleAngle.ToRotationVector2() * (35f + chargePercent * 15f);
                        Color chargeColor = Color.Lerp(NachtmusikCosmicVFX.DeepPurple, NachtmusikCosmicVFX.Gold, chargePercent);
                        CustomParticles.GenericFlare(particlePos, chargeColor, 0.2f + chargePercent * 0.2f, 10);
                    }
                }
                
                // Full charge indicator
                if (executionCharge >= 50 && Main.rand.NextBool(5))
                {
                    NachtmusikCosmicVFX.SpawnGlyphBurst(player.Center, 1, 3f, 0.25f);
                }
            }
            
            Lighting.AddLight(player.Center, NachtmusikCosmicVFX.DeepPurple.ToVector3() * 0.4f);
        }
        
        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Combo", "Every 3rd swing fires a spectral blade"));
            tooltips.Add(new TooltipLine(Mod, "Execution", $"Right-click: Execution Strike (requires 50 charge, current: {executionCharge}/100)"));
            tooltips.Add(new TooltipLine(Mod, "Charge", "Build charge by hitting enemies, crits give bonus charge"));
            tooltips.Add(new TooltipLine(Mod, "Debuff", "Inflicts Celestial Harmony - stacking cosmic damage over time"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The final note of existence'")
            {
                OverrideColor = NachtmusikCosmicVFX.Gold
            });
        }
    }
    
    /// <summary>
    /// Midnight's Crescendo - A blade that builds power with each swing.
    /// Each consecutive hit increases damage until you stop attacking.
    /// DAMAGE: 680 base, scales up to 1800+ with full crescendo
    /// </summary>
    public class MidnightsCrescendo : ModItem
    {
        private int crescendoStacks = 0;
        private int decayTimer = 0;
        private const int MaxStacks = 15;
        private const int DecayTime = 90; // 1.5 seconds to decay
        
        public override void SetDefaults()
        {
            Item.width = 60;
            Item.height = 60;
            Item.damage = 680; // Base damage - scales with crescendo
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 14;
            Item.useAnimation = 14;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(gold: 45);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.shoot = ModContent.ProjectileType<CrescendoWaveProjectile>();
            Item.shootSpeed = 12f;
            Item.scale = 1.3f;
            Item.crit = 15;
        }
        
        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            // Each stack adds 12% damage
            float crescendoBonus = 1f + (crescendoStacks * 0.12f);
            damage *= crescendoBonus;
        }
        
        public override void ModifyWeaponCrit(Player player, ref float crit)
        {
            // Each stack adds 2% crit
            crit += crescendoStacks * 2f;
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Fire crescendo wave at high stacks
            if (crescendoStacks >= 8)
            {
                Vector2 direction = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
                Projectile.NewProjectile(source, player.Center, direction * 14f, type, damage, knockback, player.whoAmI);
                
                // Big VFX for crescendo release
                NachtmusikCosmicVFX.SpawnCelestialExplosion(player.Center + direction * 40f, 0.8f + crescendoStacks * 0.05f);
            }
            
            return false;
        }
        
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            float stackPercent = (float)crescendoStacks / MaxStacks;
            
            // Intensifying swing effects based on stacks
            int particleCount = 1 + (int)(stackPercent * 4);
            for (int i = 0; i < particleCount; i++)
            {
                Vector2 trailPos = hitbox.Center.ToVector2() + Main.rand.NextVector2Circular(hitbox.Width / 2f, hitbox.Height / 2f);
                Color trailColor = Color.Lerp(NachtmusikCosmicVFX.DeepPurple, NachtmusikCosmicVFX.Gold, stackPercent);
                
                var trail = new GenericGlowParticle(trailPos, player.velocity * 0.2f + Main.rand.NextVector2Circular(1.5f, 1.5f),
                    trailColor * (0.5f + stackPercent * 0.4f), 0.25f + stackPercent * 0.2f, 12, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // Music notes at higher stacks
            if (crescendoStacks >= 5 && Main.rand.NextBool(3))
            {
                ThemedParticles.MusicNote(hitbox.Center.ToVector2(), Main.rand.NextVector2Circular(2f, 2f), 
                    NachtmusikCosmicVFX.Gold, 0.25f + stackPercent * 0.15f, 18);
            }
            
            // Dust intensity scales with stacks
            if (Main.rand.NextBool(3 - (int)(stackPercent * 2)))
            {
                Dust dust = Dust.NewDustPerfect(hitbox.Center.ToVector2(), DustID.GoldFlame, Main.rand.NextVector2Circular(2f + stackPercent * 2f, 2f + stackPercent * 2f), 0, default, 0.9f + stackPercent * 0.4f);
                dust.noGravity = true;
            }
        }
        
        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Build crescendo stacks
            if (crescendoStacks < MaxStacks)
            {
                crescendoStacks++;
                decayTimer = DecayTime;
                
                // Stack gain VFX
                if (crescendoStacks % 3 == 0)
                {
                    NachtmusikCosmicVFX.SpawnMusicNoteBurst(target.Center, 3, 3f);
                }
            }
            
            // Apply Celestial Harmony
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 360);
            int stacksToAdd = 1 + crescendoStacks / 5;
            target.GetGlobalNPC<CelestialHarmonyNPC>().AddStack(target, stacksToAdd);
            
            // Impact VFX scales with crescendo
            float stackPercent = (float)crescendoStacks / MaxStacks;
            NachtmusikCosmicVFX.SpawnCelestialImpact(target.Center, 0.8f + stackPercent * 0.6f);
            
            // Max stack explosion
            if (crescendoStacks == MaxStacks && hit.Crit)
            {
                NachtmusikCosmicVFX.SpawnCelestialExplosion(target.Center, 1.3f);
                MagnumScreenEffects.AddScreenShake(8f);
            }
        }
        
        public override void UpdateInventory(Player player)
        {
            // Decay stacks when not attacking
            if (decayTimer > 0)
            {
                decayTimer--;
            }
            else if (crescendoStacks > 0)
            {
                crescendoStacks--;
                decayTimer = 15; // Slow decay, one stack every 15 frames
            }
        }
        
        public override void HoldItem(Player player)
        {
            // Reset decay timer while holding
            if (player.itemAnimation > 0)
            {
                decayTimer = DecayTime;
            }
            
            // Crescendo aura based on stacks
            float stackPercent = (float)crescendoStacks / MaxStacks;
            if (crescendoStacks > 0 && Main.rand.NextBool((int)(6 - stackPercent * 4) + 1))
            {
                float angle = Main.GameUpdateCount * 0.03f * (1f + stackPercent);
                Vector2 auraPos = player.Center + angle.ToRotationVector2() * (25f + stackPercent * 20f);
                Color auraColor = Color.Lerp(NachtmusikCosmicVFX.Violet, NachtmusikCosmicVFX.Gold, stackPercent);
                CustomParticles.GenericFlare(auraPos, auraColor, 0.2f + stackPercent * 0.15f, 10);
            }
            
            // Max stack indicator
            if (crescendoStacks >= MaxStacks && Main.rand.NextBool(8))
            {
                NachtmusikCosmicVFX.SpawnGlyphBurst(player.Center, 1, 2f, 0.2f);
            }
            
            Lighting.AddLight(player.Center, NachtmusikCosmicVFX.Violet.ToVector3() * (0.3f + stackPercent * 0.4f));
        }
        
        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            float damageBonus = crescendoStacks * 12;
            float critBonus = crescendoStacks * 2;
            
            tooltips.Add(new TooltipLine(Mod, "Crescendo", $"Crescendo Stacks: {crescendoStacks}/{MaxStacks}"));
            tooltips.Add(new TooltipLine(Mod, "Bonus", $"Current Bonus: +{damageBonus}% damage, +{critBonus}% crit"));
            tooltips.Add(new TooltipLine(Mod, "Mechanic", "Build crescendo by hitting enemies consecutively"));
            tooltips.Add(new TooltipLine(Mod, "Wave", "At 8+ stacks, swings release crescendo waves"));
            tooltips.Add(new TooltipLine(Mod, "Debuff", "Inflicts Celestial Harmony - more stacks = more debuff stacks"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Each swing writes another verse of the cosmic symphony'")
            {
                OverrideColor = NachtmusikCosmicVFX.Violet
            });
        }
    }
    
    /// <summary>
    /// Twilight Severance - A blade that severs the boundary between day and night.
    /// Ultra-fast katana-style weapon with dimension-slicing attacks.
    /// DAMAGE: 750 (fast attack speed compensates)
    /// </summary>
    public class TwilightSeverance : ModItem
    {
        private int slashCombo = 0;
        private int twilightCharge = 0;
        private const int MaxTwilightCharge = 100;
        
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }
        
        public override void SetDefaults()
        {
            Item.width = 58;
            Item.height = 58;
            Item.damage = 750;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 8; // Ultra-fast katana
            Item.useAnimation = 8;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 45);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.shoot = ModContent.ProjectileType<TwilightSlashProjectile>();
            Item.shootSpeed = 18f;
            Item.scale = 1.3f;
            Item.crit = 25; // High crit for katana
        }
        
        public override bool AltFunctionUse(Player player) => twilightCharge >= MaxTwilightCharge;
        
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2 && twilightCharge >= MaxTwilightCharge)
            {
                Item.useTime = 25;
                Item.useAnimation = 25;
                Item.UseSound = SoundID.Item71;
            }
            else
            {
                Item.useTime = 8;
                Item.useAnimation = 8;
                Item.UseSound = SoundID.Item1;
            }
            return base.CanUseItem(player);
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Alt-click: Twilight Dimension Sever
            if (player.altFunctionUse == 2 && twilightCharge >= MaxTwilightCharge)
            {
                twilightCharge = 0;
                
                // Spawn dimension-severing slash
                for (int i = -2; i <= 2; i++)
                {
                    Vector2 slashVel = velocity.RotatedBy(MathHelper.ToRadians(i * 12));
                    Projectile.NewProjectile(source, position, slashVel * 1.5f, type, damage * 3, knockback * 2f, player.whoAmI, 1f);
                }
                
                // Massive VFX
                NachtmusikCosmicVFX.SpawnCelestialExplosion(player.Center + velocity * 2f, 2f);
                NachtmusikCosmicVFX.SpawnGlyphBurst(player.Center, 8, 6f, 0.5f);
                
                // Screen flash
                MagnumScreenEffects.AddScreenShake(12f);
                
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 1.2f }, player.Center);
                
                return false;
            }
            
            // Normal rapid slashes
            slashCombo++;
            twilightCharge = Math.Min(MaxTwilightCharge, twilightCharge + 5);
            
            // Every 3rd slash is a double slash
            if (slashCombo % 3 == 0)
            {
                Vector2 perpVel = new Vector2(-velocity.Y, velocity.X).SafeNormalize(Vector2.Zero) * velocity.Length() * 0.5f;
                Projectile.NewProjectile(source, position, velocity + perpVel, type, damage, knockback, player.whoAmI);
                Projectile.NewProjectile(source, position, velocity - perpVel, type, damage, knockback, player.whoAmI);
                
                NachtmusikCosmicVFX.SpawnCelestialImpact(position, 0.6f);
            }
            
            // VFX trail
            NachtmusikCosmicVFX.SpawnRadiantBeamTrail(position, velocity, 0.4f);
            
            return true;
        }
        
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // Twilight slash trail
            Vector2 center = hitbox.Center.ToVector2();
            
            if (Main.rand.NextBool(2))
            {
                Color slashColor = Color.Lerp(NachtmusikCosmicVFX.DeepPurple, NachtmusikCosmicVFX.Gold, Main.rand.NextFloat());
                var glow = new GenericGlowParticle(center, Main.rand.NextVector2Circular(3f, 3f), slashColor, 0.3f, 12, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Twilight charge indicator
            if (twilightCharge >= MaxTwilightCharge && Main.rand.NextBool(3))
            {
                CustomParticles.GenericFlare(center, NachtmusikCosmicVFX.Gold, 0.35f, 8);
            }
            
            Lighting.AddLight(center, NachtmusikCosmicVFX.Violet.ToVector3() * 0.5f);
        }
        
        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Celestial Harmony
            int stacks = 1 + (hit.Crit ? 1 : 0);
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 300);
            
            if (target.TryGetGlobalNPC(out CelestialHarmonyNPC harmonyNPC))
            {
                harmonyNPC.AddStack(target, stacks);
            }
            
            // Fast slash VFX
            NachtmusikCosmicVFX.SpawnCelestialImpact(target.Center, 0.5f);
            
            // Build twilight charge faster on crits
            if (hit.Crit)
            {
                twilightCharge = Math.Min(MaxTwilightCharge, twilightCharge + 8);
                NachtmusikCosmicVFX.SpawnMusicNoteBurst(target.Center, 3, 25f);
            }
        }
        
        public override void HoldItem(Player player)
        {
            // Decay charge when not attacking
            if (player.itemAnimation == 0 && twilightCharge > 0)
            {
                twilightCharge = Math.Max(0, twilightCharge - 1);
            }
            
            // Visual indicator when fully charged
            if (twilightCharge >= MaxTwilightCharge)
            {
                if (Main.rand.NextBool(6))
                {
                    float angle = Main.GameUpdateCount * 0.08f;
                    Vector2 orbitPos = player.Center + angle.ToRotationVector2() * 30f;
                    CustomParticles.GenericFlare(orbitPos, NachtmusikCosmicVFX.Gold, 0.3f, 10);
                }
                
                Lighting.AddLight(player.Center, NachtmusikCosmicVFX.Gold.ToVector3() * 0.4f);
            }
        }
        
        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            float chargePercent = (twilightCharge / (float)MaxTwilightCharge) * 100f;
            
            tooltips.Add(new TooltipLine(Mod, "Charge", $"Twilight Charge: {chargePercent:F0}%"));
            tooltips.Add(new TooltipLine(Mod, "Mechanic", "Ultra-fast slashes build Twilight Charge"));
            tooltips.Add(new TooltipLine(Mod, "Special", "Right-click at full charge: Dimension Sever (5 slashes, 3x damage)"));
            tooltips.Add(new TooltipLine(Mod, "Combo", "Every 3rd slash is a double strike"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Where twilight falls, reality parts'")
            {
                OverrideColor = NachtmusikCosmicVFX.Gold
            });
        }
    }
}
