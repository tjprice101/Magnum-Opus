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
using static MagnumOpus.Common.Systems.ThemedParticles;
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
            Item.damage = 1850; // POST-FATE ULTIMATE - 37%+ above Fate tier (Coda=1350)
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
                
                // Execution Strike VFX - trust the explosion
                NachtmusikCosmicVFX.SpawnCelestialExplosion(player.Center + direction * 50f, 1.5f);
                MagnumScreenEffects.AddScreenShake(10f);
                
                // Single music note burst
                ThemedParticles.MusicNoteBurst(player.Center, new Color(80, 100, 200), 5, 4f);
                
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
            Vector2 hitCenter = hitbox.Center.ToVector2();
            
            // === SPECTACULAR SWING SYSTEM - ULTIMATE TIER (9+ arcs + constellation + cosmic clouds + seeking shimmers) ===
            SpectacularMeleeSwing.OnSwing(player, hitbox, NachtmusikCosmicVFX.DeepPurple, NachtmusikCosmicVFX.Gold, 
                SpectacularMeleeSwing.SwingTier.Ultimate, SpectacularMeleeSwing.WeaponTheme.Nachtmusik);
            
            // === IRIDESCENT WINGSPAN-STYLE HEAVY DUST TRAILS ===
            // Heavy purple dust trail #1
            float trailProgress1 = Main.rand.NextFloat();
            Color purpleGradient = Color.Lerp(NachtmusikCosmicVFX.DeepPurple, NachtmusikCosmicVFX.NightBlue, trailProgress1);
            Dust heavyPurple = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, 
                DustID.PurpleTorch, player.velocity.X * 0.3f, player.velocity.Y * 0.3f, 100, purpleGradient, 1.6f);
            heavyPurple.noGravity = true;
            heavyPurple.fadeIn = 1.5f;
            heavyPurple.velocity = heavyPurple.velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(1.3f, 2.0f);
            
            // Heavy gold dust trail #2
            float trailProgress2 = Main.rand.NextFloat();
            Color goldGradient = Color.Lerp(NachtmusikCosmicVFX.Gold, NachtmusikCosmicVFX.StarWhite, trailProgress2);
            Dust heavyGold = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, 
                DustID.Enchanted_Gold, player.velocity.X * 0.25f, player.velocity.Y * 0.25f, 80, goldGradient, 1.5f);
            heavyGold.noGravity = true;
            heavyGold.fadeIn = 1.4f;
            heavyGold.velocity = heavyGold.velocity.RotatedByRandom(0.4f) * Main.rand.NextFloat(1.2f, 1.8f);
            
            // === CONTRASTING STAR SPARKLES (1-in-2) ===
            if (Main.rand.NextBool(2))
            {
                Vector2 sparklePos = new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height));
                CustomParticles.PrismaticSparkle(sparklePos, NachtmusikCosmicVFX.StarWhite, 0.4f);
                
                Dust starDust = Dust.NewDustDirect(sparklePos, 1, 1, DustID.GoldFlame, 0f, 0f, 100, default, 1.0f);
                starDust.noGravity = true;
                starDust.velocity = Main.rand.NextVector2Circular(2.5f, 2.5f);
            }
            
            // === CELESTIAL SHIMMER TRAIL (Main.hslToRgb in purple-gold range) (1-in-3) ===
            if (Main.rand.NextBool(3))
            {
                // Celestial shimmer - cycling through purple to gold hues
                float celestialHue = Main.rand.NextBool() ? (0.73f + (Main.GameUpdateCount * 0.015f % 0.12f)) : (0.12f + (Main.GameUpdateCount * 0.015f % 0.08f));
                Color shimmerColor = Main.hslToRgb(celestialHue, 0.9f, 0.8f);
                Vector2 shimmerPos = new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height));
                CustomParticles.GenericFlare(shimmerPos, shimmerColor, 0.45f, 14);
            }
            
            // === PEARLESCENT COSMIC EFFECTS (1-in-4) ===
            if (Main.rand.NextBool(4))
            {
                Vector2 pearlPos = new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height));
                float pearlShift = (Main.GameUpdateCount * 0.02f) % 1f;
                Color pearlColor = Color.Lerp(Color.Lerp(NachtmusikCosmicVFX.Gold, NachtmusikCosmicVFX.StarWhite, pearlShift), 
                    NachtmusikCosmicVFX.DeepPurple, (float)Math.Sin(pearlShift * MathHelper.TwoPi) * 0.3f + 0.3f);
                CustomParticles.GenericFlare(pearlPos, pearlColor * 0.85f, 0.38f, 16);
            }
            
            // === FREQUENT FLARES (1-in-2) ===
            if (Main.rand.NextBool(2))
            {
                Vector2 flarePos = new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height));
                Color flareColor = NachtmusikCosmicVFX.GetCelestialGradient(Main.rand.NextFloat());
                CustomParticles.GenericFlare(flarePos, flareColor, 0.4f, 12);
            }
            
            // Cosmic swing trail
            if (Main.rand.NextBool())
            {
                Vector2 trailPos = hitCenter + Main.rand.NextVector2Circular(hitbox.Width / 3f, hitbox.Height / 3f);
                Color trailColor = NachtmusikCosmicVFX.GetCelestialGradient(Main.rand.NextFloat());
                
                var trail = new GenericGlowParticle(trailPos, player.velocity * 0.3f + Main.rand.NextVector2Circular(1f, 1f),
                    trailColor * 0.75f, 0.4f, 18, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // === MUSIC NOTES (1-in-6) - VISIBLE SCALE 0.85f+ ===
            if (Main.rand.NextBool(6))
            {
                Vector2 notePos = hitCenter + Main.rand.NextVector2Circular(20f, 20f);
                Color noteColor = Color.Lerp(NachtmusikCosmicVFX.Gold, NachtmusikCosmicVFX.DeepPurple, Main.rand.NextFloat());
                ThemedParticles.MusicNote(notePos, Main.rand.NextVector2Circular(2f, 2f), noteColor, 0.9f, 28);
                
                // Celestial sparkle accent
                var sparkle = new SparkleParticle(notePos, Main.rand.NextVector2Circular(1.5f, 1.5f), NachtmusikCosmicVFX.StarWhite * 0.6f, 0.28f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }
        
        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Build execution charge on hit
            executionCharge = Math.Min(100, executionCharge + 8);
            
            // Apply Celestial Harmony
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 480);
            target.GetGlobalNPC<CelestialHarmonyNPC>().AddStack(target, 2);
            
            // GRAND CELESTIAL IMPACT - signature Nachtmusik effect with StarBurst and ShatteredStarlight
            NachtmusikCosmicVFX.SpawnGrandCelestialImpact(target.Center, 1.2f);
            
            // Glyph burst on crit + seeking crystals
            if (hit.Crit)
            {
                // Additional star burst explosion on crit
                NachtmusikCosmicVFX.SpawnStarBurstImpact(target.Center, 1.4f, 5);
                NachtmusikCosmicVFX.SpawnGlyphBurst(target.Center, 6, 5f, 0.4f);
                executionCharge = Math.Min(100, executionCharge + 15);
                
                // Spawn seeking crystals on crit - POST-FATE ULTIMATE power
                SeekingCrystalHelper.SpawnNachtmusikCrystals(
                    player.GetSource_ItemUse(Item),
                    target.Center,
                    (Main.MouseWorld - target.Center).SafeNormalize(Vector2.UnitX) * 8f,
                    (int)(damageDone * 0.3f),
                    Item.knockBack * 0.5f,
                    player.whoAmI,
                    6); // 6 crystals for ultimate tier
            }
        }
        
        public override void HoldItem(Player player)
        {
            // Execution charge indicator - subtle orbiting particle
            if (executionCharge >= 50 && Main.rand.NextBool(12))
            {
                float angle = Main.GameUpdateCount * 0.05f;
                Vector2 particlePos = player.Center + angle.ToRotationVector2() * 40f;
                Color chargeColor = Color.Lerp(NachtmusikCosmicVFX.DeepPurple, NachtmusikCosmicVFX.Gold, executionCharge / 100f);
                CustomParticles.GenericFlare(particlePos, chargeColor, 0.25f, 10);
            }
            
            // Sparse ambient music note
            if (Main.rand.NextBool(25))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                Vector2 noteVel = new Vector2(0, -0.4f);
                Color noteColor = Color.Lerp(new Color(100, 60, 180), new Color(80, 100, 200), Main.rand.NextFloat()) * 0.6f;
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.7f, 35);
            }
            
            Lighting.AddLight(player.Center, NachtmusikCosmicVFX.DeepPurple.ToVector3() * 0.3f);
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
            Item.damage = 1200; // Base damage - scales to 3200+ with full crescendo
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
            Vector2 hitCenter = hitbox.Center.ToVector2();
            
            // === SPECTACULAR SWING SYSTEM - ENDGAME TIER (7-8 layered arcs + celestial effects) ===
            SpectacularMeleeSwing.OnSwing(player, hitbox, NachtmusikCosmicVFX.DeepPurple, NachtmusikCosmicVFX.Gold, 
                SpectacularMeleeSwing.SwingTier.Endgame, SpectacularMeleeSwing.WeaponTheme.Nachtmusik);
            
            // Swing trail - single particle, intensity scales with stacks
            if (Main.rand.NextBool(2))
            {
                Vector2 trailPos = hitCenter + Main.rand.NextVector2Circular(hitbox.Width / 3f, hitbox.Height / 3f);
                Color trailColor = Color.Lerp(NachtmusikCosmicVFX.DeepPurple, NachtmusikCosmicVFX.Gold, stackPercent);
                var trail = new GenericGlowParticle(trailPos, player.velocity * 0.2f, trailColor * 0.6f, 0.25f + stackPercent * 0.15f, 12, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // Sparse music note at higher stacks
            if (crescendoStacks >= 8 && Main.rand.NextBool(5))
            {
                ThemedParticles.MusicNote(hitCenter, Main.rand.NextVector2Circular(1.5f, 1.5f), NachtmusikCosmicVFX.Gold, 0.7f, 18);
            }
            
            // Dust intensity scales with stacks
            if (Main.rand.NextBool(3 - (int)(stackPercent * 2)))
            {
                Dust dust = Dust.NewDustPerfect(hitCenter, DustID.GoldFlame, Main.rand.NextVector2Circular(2f + stackPercent * 2f, 2f + stackPercent * 2f), 0, default, 0.9f + stackPercent * 0.4f);
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
                
                // Stack gain VFX with shattered starlight
                if (crescendoStacks % 3 == 0)
                {
                    NachtmusikCosmicVFX.SpawnMusicNoteBurst(target.Center, 3, 3f);
                    NachtmusikCosmicVFX.SpawnShatteredStarlightBurst(target.Center, 6, 4f, 0.6f, true);
                }
            }
            
            // Apply Celestial Harmony
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 360);
            int stacksToAdd = 1 + crescendoStacks / 5;
            target.GetGlobalNPC<CelestialHarmonyNPC>().AddStack(target, stacksToAdd);
            
            // Impact VFX scales with crescendo - use StarBurst for high stacks
            float stackPercent = (float)crescendoStacks / MaxStacks;
            if (stackPercent > 0.5f)
            {
                // High crescendo = star burst impact
                NachtmusikCosmicVFX.SpawnStarBurstImpact(target.Center, 0.6f + stackPercent * 0.8f, 2 + (int)(stackPercent * 3));
                NachtmusikCosmicVFX.SpawnShatteredStarlightBurst(target.Center, (int)(4 + stackPercent * 8), 5f + stackPercent * 4f, stackPercent, true);
            }
            else
            {
                NachtmusikCosmicVFX.SpawnCelestialImpact(target.Center, 0.8f + stackPercent * 0.6f);
            }
            
            // Max stack explosion with GRAND impact + seeking crystals
            if (crescendoStacks == MaxStacks && hit.Crit)
            {
                NachtmusikCosmicVFX.SpawnGrandCelestialImpact(target.Center, 1.5f);
                MagnumScreenEffects.AddScreenShake(8f);
                
                // Spawn seeking crystals at full crescendo crit - powerful burst
                SeekingCrystalHelper.SpawnNachtmusikCrystals(
                    player.GetSource_ItemUse(Item),
                    target.Center,
                    (Main.MouseWorld - target.Center).SafeNormalize(Vector2.UnitX) * 10f,
                    (int)(damageDone * 0.25f),
                    Item.knockBack * 0.5f,
                    player.whoAmI,
                    5); // 5 crystals at max crescendo
            }
            // Also spawn crystals periodically at high stacks
            else if (crescendoStacks >= 10 && Main.rand.NextBool(4))
            {
                SeekingCrystalHelper.SpawnNachtmusikCrystals(
                    player.GetSource_ItemUse(Item),
                    target.Center,
                    (Main.MouseWorld - target.Center).SafeNormalize(Vector2.UnitX) * 8f,
                    (int)(damageDone * 0.15f),
                    Item.knockBack * 0.4f,
                    player.whoAmI,
                    3); // 3 crystals at high stacks
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
            
            float stackPercent = (float)crescendoStacks / MaxStacks;
            
            // Subtle crescendo aura at high stacks only
            if (crescendoStacks >= 10 && Main.rand.NextBool(15))
            {
                float angle = Main.GameUpdateCount * 0.04f;
                Vector2 auraPos = player.Center + angle.ToRotationVector2() * 35f;
                Color auraColor = Color.Lerp(NachtmusikCosmicVFX.Violet, NachtmusikCosmicVFX.Gold, stackPercent);
                CustomParticles.GenericFlare(auraPos, auraColor, 0.2f, 10);
            }
            
            // Sparse ambient music note
            if (Main.rand.NextBool(25))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                Color noteColor = Color.Lerp(NachtmusikCosmicVFX.Violet, NachtmusikCosmicVFX.Gold, stackPercent) * 0.6f;
                ThemedParticles.MusicNote(notePos, new Vector2(0, -0.4f), noteColor, 0.7f, 35);
            }
            
            Lighting.AddLight(player.Center, NachtmusikCosmicVFX.Violet.ToVector3() * 0.3f);
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
    /// DAMAGE: 980 (fast attack speed - POST-FATE ULTIMATE)
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
            Item.damage = 1450; // POST-FATE ULTIMATE - 48%+ above Fate tier
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
                
                // Dimension Sever VFX - trust the explosion
                NachtmusikCosmicVFX.SpawnCelestialExplosion(player.Center + velocity * 2f, 1.8f);
                NachtmusikCosmicVFX.SpawnGlyphBurst(player.Center, 4, 5f, 0.4f);
                
                // Single music note burst
                ThemedParticles.MusicNoteBurst(player.Center, new Color(80, 100, 200), 6, 5f);
                
                MagnumScreenEffects.AddScreenShake(10f);
                
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
            
            // === SPECTACULAR SWING SYSTEM - ULTIMATE TIER (10+ layered arcs + cosmic severity) ===
            SpectacularMeleeSwing.OnSwing(player, hitbox, NachtmusikCosmicVFX.DeepPurple, NachtmusikCosmicVFX.Gold, 
                SpectacularMeleeSwing.SwingTier.Ultimate, SpectacularMeleeSwing.WeaponTheme.Nachtmusik);
            
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
            
            // Music notes in swing trail
            if (Main.rand.NextBool(4))
            {
                ThemedParticles.MusicNote(center, Main.rand.NextVector2Circular(2f, 2f), new Color(100, 60, 180) * 0.8f, 0.7f, 18);
                
                // Star sparkle accent
                var sparkle = new SparkleParticle(center, Main.rand.NextVector2Circular(1.5f, 1.5f), new Color(255, 250, 240) * 0.5f, 0.18f, 14);
                MagnumParticleHandler.SpawnParticle(sparkle);
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
            
            // Fast slash VFX with shattered starlight fragments
            NachtmusikCosmicVFX.SpawnCelestialImpact(target.Center, 0.5f);
            NachtmusikCosmicVFX.SpawnShatteredStarlightBurst(target.Center, 5, 6f, 0.4f, false); // No gravity for fast effect
            
            // Build twilight charge faster on crits - add star burst + seeking crystals
            if (hit.Crit)
            {
                twilightCharge = Math.Min(MaxTwilightCharge, twilightCharge + 8);
                NachtmusikCosmicVFX.SpawnStarBurstImpact(target.Center, 0.8f, 2);
                NachtmusikCosmicVFX.SpawnMusicNoteBurst(target.Center, 3, 25f);
                
                // Fast katana spawns 4 seeking crystals on crit
                SeekingCrystalHelper.SpawnNachtmusikCrystals(
                    player.GetSource_ItemUse(Item),
                    target.Center,
                    (Main.MouseWorld - target.Center).SafeNormalize(Vector2.UnitX) * 10f,
                    (int)(damageDone * 0.2f),
                    Item.knockBack * 0.4f,
                    player.whoAmI,
                    4); // 4 crystals for fast weapon
            }
        }
        
        public override void HoldItem(Player player)
        {
            // Decay charge when not attacking
            if (player.itemAnimation == 0 && twilightCharge > 0)
            {
                twilightCharge = Math.Max(0, twilightCharge - 1);
            }
            
            // Subtle charge indicator when fully charged
            if (twilightCharge >= MaxTwilightCharge && Main.rand.NextBool(15))
            {
                float angle = Main.GameUpdateCount * 0.06f;
                Vector2 orbitPos = player.Center + angle.ToRotationVector2() * 30f;
                CustomParticles.GenericFlare(orbitPos, NachtmusikCosmicVFX.Gold, 0.25f, 10);
            }
            
            // Sparse ambient music note
            if (Main.rand.NextBool(25))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                Color noteColor = Color.Lerp(NachtmusikCosmicVFX.DuskViolet, NachtmusikCosmicVFX.StarGold, Main.rand.NextFloat()) * 0.6f;
                ThemedParticles.MusicNote(notePos, new Vector2(0, -0.4f), noteColor, 0.7f, 35);
            }
            
            Lighting.AddLight(player.Center, NachtmusikCosmicVFX.Violet.ToVector3() * 0.25f);
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
