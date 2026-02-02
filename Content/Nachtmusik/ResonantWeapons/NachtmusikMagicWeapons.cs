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
    /// Starweaver's Grimoire - A tome that weaves constellations of destruction.
    /// Fires cosmic orbs that create mini-explosions along their path.
    /// DAMAGE: 450 base + periodic explosions
    /// </summary>
    public class StarweaversGrimoire : ModItem
    {
        private int constellationCharge = 0;
        
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }
        
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 38;
            Item.damage = 1080; // POST-FATE ULTIMATE MAGIC - 38%+ above Fate tier
            Item.DamageType = DamageClass.Magic;
            Item.mana = 12;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(gold: 46);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.UseSound = SoundID.Item8;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<StarweaverOrbProjectile>();
            Item.shootSpeed = 12f;
            Item.crit = 16;
            Item.staff[Item.type] = true;
        }
        
        public override bool AltFunctionUse(Player player) => true;
        
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Constellation Burst - requires full charge
                if (constellationCharge < 100)
                    return false;
                    
                Item.mana = 40;
                Item.useTime = 35;
                Item.useAnimation = 35;
            }
            else
            {
                Item.mana = 12;
                Item.useTime = 18;
                Item.useAnimation = 18;
            }
            return base.CanUseItem(player);
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 direction = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            
            if (player.altFunctionUse == 2 && constellationCharge >= 100)
            {
                // Constellation Burst - fire a massive barrage
                constellationCharge = 0;
                
                // Fire 12 orbs in a complex pattern
                for (int wave = 0; wave < 3; wave++)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        float angleOffset = MathHelper.ToRadians(-30f + 20f * i);
                        float speedMod = 1f - wave * 0.15f;
                        Vector2 orbVel = direction.RotatedBy(angleOffset) * 14f * speedMod;
                        
                        // Stagger spawns for wave effect
                        Vector2 spawnPos = player.Center + direction * (20f + wave * 15f);
                        
                        Projectile.NewProjectile(source, spawnPos, orbVel, type, (int)(damage * 1.5f), knockback, player.whoAmI);
                    }
                }
                
                // === SPAWN SEEKING CRYSTALS - CONSTELLATION BURST FINALE ===
                // Massive burst of 8 seeking crystals that spread out and home to enemies
                SeekingCrystalHelper.SpawnNachtmusikCrystals(
                    source,
                    player.Center + direction * 40f,
                    direction * 10f,
                    (int)(damage * 0.6f),
                    knockback,
                    player.whoAmI,
                    8  // 8 crystals for constellation burst
                );
                
                // Constellation Burst VFX - trust GrandCelestialImpact
                NachtmusikCosmicVFX.SpawnGrandCelestialImpact(player.Center + direction * 40f, 1.5f);
                NachtmusikCosmicVFX.SpawnConstellationCircle(player.Center, 50f, 8, 0.4f);
                MagnumScreenEffects.AddScreenShake(8f);
                
                // Single music note burst
                ThemedParticles.MusicNoteBurst(player.Center, new Color(80, 100, 200), 5, 4f);
                
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 1f }, player.Center);
            }
            else
            {
                // Normal shot - single orb
                Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
                
                // Build constellation charge
                constellationCharge = Math.Min(100, constellationCharge + 8);
                
                // Cast VFX with shattered starlight accents
                CustomParticles.GenericFlare(position + direction * 20f, NachtmusikCosmicVFX.Violet, 0.4f, 12);
                if (Main.rand.NextBool(3))
                {
                    NachtmusikCosmicVFX.SpawnShatteredStarlightBurst(position + direction * 15f, 3, 3f, 0.3f, false);
                }
                
                // Music note on cast
                ThemedParticles.MusicNote(position + direction * 15f, direction * 1.5f, new Color(100, 60, 180) * 0.8f, 0.7f, 25);
            }
            
            return false;
        }
        
        public override void HoldItem(Player player)
        {
            float chargePercent = constellationCharge / 100f;
            
            // Subtle charge indicator at high charge
            if (constellationCharge >= 50 && Main.rand.NextBool(15))
            {
                float angle = Main.GameUpdateCount * 0.04f;
                Vector2 pointPos = player.Center + angle.ToRotationVector2() * 35f;
                Color pointColor = Color.Lerp(NachtmusikCosmicVFX.DeepPurple, NachtmusikCosmicVFX.StarWhite, chargePercent);
                CustomParticles.GenericFlare(pointPos, pointColor, 0.18f, 8);
            }
            
            // Sparse ambient music note
            if (Main.rand.NextBool(25))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                Color noteColor = Color.Lerp(NachtmusikCosmicVFX.DeepPurple, NachtmusikCosmicVFX.StarWhite, chargePercent) * 0.5f;
                ThemedParticles.MusicNote(notePos, new Vector2(0, -0.4f), noteColor, 0.65f, 30);
            }
            
            Lighting.AddLight(player.Center, NachtmusikCosmicVFX.Violet.ToVector3() * 0.25f);
        }
        
        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Orb", "Fires cosmic orbs that create mini-explosions along their path"));
            tooltips.Add(new TooltipLine(Mod, "Charge", $"Constellation Charge: {constellationCharge}/100"));
            tooltips.Add(new TooltipLine(Mod, "Burst", "Right-click at full charge: Constellation Burst"));
            tooltips.Add(new TooltipLine(Mod, "Debuff", "Inflicts heavy Celestial Harmony stacks"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Written in stardust, bound in eternity'")
            {
                OverrideColor = NachtmusikCosmicVFX.Violet
            });
        }
    }
    
    /// <summary>
    /// Requiem of the Cosmos - The ultimate magic weapon from Nachtmusik.
    /// Fires devastating cosmic beams that pierce all enemies.
    /// DAMAGE: 580 with infinite pierce
    /// </summary>
    public class RequiemOfTheCosmos : ModItem
    {
        private int requiemTimer = 0;
        private bool isChanneling = false;
        
        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 42;
            Item.damage = 1320; // POST-FATE ULTIMATE MAGIC - 40%+ above Fate tier
            Item.DamageType = DamageClass.Magic;
            Item.mana = 18;
            Item.useTime = 8;
            Item.useAnimation = 8;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 52);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.UseSound = SoundID.Item12;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<CosmicRequiemBeamProjectile>();
            Item.shootSpeed = 22f;
            Item.crit = 22;
            Item.channel = true;
            Item.staff[Item.type] = true;
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            requiemTimer++;
            isChanneling = true;
            
            Vector2 direction = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            
            // Fire beam
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            
            // Channeling intensifies over time
            float intensity = Math.Min(1f, requiemTimer / 60f);
            
            // Fire additional beams at higher intensity
            if (requiemTimer % 4 == 0 && intensity > 0.3f)
            {
                float angleOffset = MathHelper.ToRadians(Main.rand.NextFloat(-8f, 8f) * intensity);
                Vector2 sideVel = velocity.RotatedBy(angleOffset);
                Projectile.NewProjectile(source, position, sideVel, type, (int)(damage * 0.6f), knockback * 0.5f, player.whoAmI);
            }
            
            // Grand finale at peak intensity
            if (requiemTimer % 45 == 0 && intensity >= 1f)
            {
                // Burst of 8 beams
                for (int i = 0; i < 8; i++)
                {
                    float burstAngle = MathHelper.TwoPi * i / 8f;
                    Vector2 burstVel = burstAngle.ToRotationVector2() * 18f;
                    Projectile.NewProjectile(source, player.Center, burstVel, type, (int)(damage * 0.8f), knockback, player.whoAmI);
                }
                
                // Grand finale - trust GrandCelestialImpact
                NachtmusikCosmicVFX.SpawnGrandCelestialImpact(player.Center, 1.2f);
                MagnumScreenEffects.AddScreenShake(6f);
                
                // Single music note burst
                ThemedParticles.MusicNoteBurst(player.Center, new Color(100, 60, 180), 6, 5f);
            }
            
            // Channeling VFX with star trail effects
            CustomParticles.GenericFlare(position + direction * 25f, NachtmusikCosmicVFX.Violet, 0.35f + intensity * 0.2f, 8);
            NachtmusikCosmicVFX.SpawnStarTrailEffect(position + direction * 20f, velocity, 0.3f + intensity * 0.3f);
            
            // Music note on cast
            ThemedParticles.MusicNote(position + direction * 15f, direction * 1.5f, new Color(100, 60, 180) * 0.8f, 0.7f, 25);
            
            if (Main.rand.NextBool(3))
            {
                Color trailColor = NachtmusikCosmicVFX.GetCelestialGradient(Main.rand.NextFloat());
                var trail = new GenericGlowParticle(position + Main.rand.NextVector2Circular(15f, 15f),
                    direction * 2f + Main.rand.NextVector2Circular(1f, 1f), trailColor * 0.6f, 0.2f, 12, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            return false;
        }
        
        public override void UpdateInventory(Player player)
        {
            // Reset channel timer when not using
            if (!player.channel || player.HeldItem.type != Item.type)
            {
                if (isChanneling && requiemTimer > 30)
                {
                    // Release burst when stopping channel
                    Vector2 direction = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
                    for (int i = 0; i < 5; i++)
                    {
                        float angle = direction.ToRotation() + MathHelper.ToRadians(-20f + 10f * i);
                        Vector2 burstVel = angle.ToRotationVector2() * 20f;
                        Projectile.NewProjectile(player.GetSource_FromThis(), player.Center, burstVel,
                            ModContent.ProjectileType<CosmicRequiemBeamProjectile>(), (int)(Item.damage * player.GetDamage(DamageClass.Magic).Multiplicative), Item.knockBack, player.whoAmI);
                    }
                    
                    NachtmusikCosmicVFX.SpawnCelestialImpact(player.Center + direction * 30f, 1f);
                }
                
                requiemTimer = 0;
                isChanneling = false;
            }
        }
        
        public override void HoldItem(Player player)
        {
            float intensity = Math.Min(1f, requiemTimer / 60f);
            
            // Subtle channeling aura at high intensity
            if (isChanneling && intensity > 0.5f && Main.rand.NextBool(15))
            {
                float angle = Main.GameUpdateCount * 0.06f;
                Vector2 auraPos = player.Center + angle.ToRotationVector2() * 35f;
                Color auraColor = NachtmusikCosmicVFX.GetCelestialGradient(Main.rand.NextFloat());
                CustomParticles.GenericFlare(auraPos, auraColor, 0.2f, 10);
            }
            
            // Sparse ambient music note
            if (Main.rand.NextBool(25))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                Color noteColor = NachtmusikCosmicVFX.GetCelestialGradient(Main.rand.NextFloat()) * 0.5f;
                ThemedParticles.MusicNote(notePos, new Vector2(0, -0.4f), noteColor, 0.65f, 30);
            }
            
            Lighting.AddLight(player.Center, NachtmusikCosmicVFX.Violet.ToVector3() * (0.25f + intensity * 0.2f));
        }
        
        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Beam", "Fires piercing cosmic beams that hit all enemies"));
            tooltips.Add(new TooltipLine(Mod, "Channel", "Hold to channel - intensity builds over time"));
            tooltips.Add(new TooltipLine(Mod, "Burst", "At peak intensity, periodically fires beam bursts"));
            tooltips.Add(new TooltipLine(Mod, "Release", "Releasing channel fires a final burst"));
            tooltips.Add(new TooltipLine(Mod, "Debuff", "Inflicts heavy Celestial Harmony stacks"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The final movement plays as the universe falls silent'")
            {
                OverrideColor = NachtmusikCosmicVFX.DeepPurple
            });
        }
    }
}
