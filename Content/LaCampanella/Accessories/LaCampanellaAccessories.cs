using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.LaCampanella.Debuffs;

namespace MagnumOpus.Content.LaCampanella.Accessories
{
    #region Chamber of Bellfire
    
    /// <summary>
    /// Chamber of Bellfire - Tier 1 La Campanella accessory.
    /// Grants fire resistance, bellfire aura that damages nearby enemies,
    /// and causes attacks to occasionally trigger bell explosions.
    /// </summary>
    public class ChamberOfBellfire : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 34;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 35);
            Item.rare = ModContent.RarityType<LaCampanellaRarity>();
            Item.defense = 6;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            ChamberOfBellfirePlayer modPlayer = player.GetModPlayer<ChamberOfBellfirePlayer>();
            modPlayer.chamberOfBellfireEquipped = true;
            
            // Fire resistance
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.OnFire3] = true;
            player.lavaImmune = true;
            
            // Damage boost to burning enemies
            player.GetDamage(DamageClass.Generic) += 0.12f;
            
            // === SIGNATURE ACCESSORY AURA - VIBRANT PARTICLES! ===
            if (!hideVisual)
            {
                ThemedParticles.LaCampanellaHoldAura(player.Center, 0.5f);
                
                // Bellfire aura (visual)
                if (Main.rand.NextBool(8))
                {
                    Vector2 offset = Main.rand.NextVector2Circular(60f, 60f);
                    Dust flame = Dust.NewDustPerfect(player.Center + offset, DustID.Torch,
                        Vector2.Zero, 100, ThemedParticles.CampanellaOrange, 1.5f);
                    flame.noGravity = true;
                }
            }
            
            Lighting.AddLight(player.Center, 0.4f, 0.2f, 0.05f);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color flameOrange = new Color(255, 140, 40);
            Color smokyBlack = new Color(50, 40, 45);
            
            tooltips.Add(new TooltipLine(Mod, "Stats", "+12% all damage, +6 defense")
            {
                OverrideColor = flameOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Aura", "Bellfire aura damages nearby enemies (25 damage every 0.5s)")
            {
                OverrideColor = flameOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect", "Every 10 hits triggers a bell explosion")
            {
                OverrideColor = smokyBlack
            });
            
            tooltips.Add(new TooltipLine(Mod, "Immunities", "Immunity to fire debuffs and lava")
            {
                OverrideColor = flameOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The chamber resonates with infernal flames'")
            {
                OverrideColor = new Color(200, 150, 100)
            });
        }
    }

    public class ChamberOfBellfirePlayer : ModPlayer
    {
        public bool chamberOfBellfireEquipped = false;
        private int bellExplosionTimer = 0;
        private int auraDamageTimer = 0;
        
        public override void ResetEffects()
        {
            chamberOfBellfireEquipped = false;
        }

        public override void PostUpdate()
        {
            if (!chamberOfBellfireEquipped) return;
            
            // Bellfire aura damage
            auraDamageTimer++;
            if (auraDamageTimer >= 30) // Every 0.5 seconds
            {
                auraDamageTimer = 0;
                DamageNearbyEnemies();
            }
        }

        private void DamageNearbyEnemies()
        {
            float auraRadius = 120f;
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                
                if (Vector2.Distance(Player.Center, npc.Center) <= auraRadius)
                {
                    // Small fire damage
                    npc.SimpleStrikeNPC(25, 0, false, 0f, null, false, 0f, true);
                    npc.GetGlobalNPC<ResonantTollNPC>().AddStacks(npc, 1);
                    
                    // Fire effect
                    for (int i = 0; i < 3; i++)
                    {
                        Dust flame = Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2Circular(npc.width * 0.3f, npc.height * 0.3f),
                            DustID.Torch, Main.rand.NextVector2Circular(2f, 2f), 100, new Color(255, 100, 0), 1.2f);
                        flame.noGravity = true;
                    }
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!chamberOfBellfireEquipped) return;
            
            // === BLACK SMOKE SPARKLE - SIGNATURE HIT ON ACCESSORY! ===
            Vector2 hitDir = (target.Center - Player.Center).SafeNormalize(Vector2.UnitX);
            ThemedParticles.LaCampanellaBlackSmokeSparkle(target.Center, 0.4f);
            
            // Chance for bell explosion
            bellExplosionTimer++;
            if (bellExplosionTimer >= 10) // Every 10 hits
            {
                bellExplosionTimer = 0;
                TriggerBellExplosion(target.Center);
            }
        }

        private void TriggerBellExplosion(Vector2 position)
        {
            // === GUTURAL BELL EXPLOSION SOUNDS ===
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = Main.rand.NextFloat(0.2f, 0.5f), Volume = 0.55f }, position);
            SoundEngine.PlaySound(SoundID.Item34 with { Pitch = Main.rand.NextFloat(0.1f, 0.4f), Volume = 0.35f }, position);
            
            // === MASSIVE BELL EXPLOSION EFFECTS WITH CUSTOM PARTICLES ===
            ThemedParticles.LaCampanellaBellChime(position, 1.2f);
            ThemedParticles.LaCampanellaShockwave(position, 0.8f);
            ThemedParticles.LaCampanellaSparks(position, Main.rand.NextVector2Unit(), 10, 7f);
            
            // === RADIAL FLARE BURST with GRADIENT ===
            for (int f = 0; f < 8; f++)
            {
                Vector2 flarePos = position + (MathHelper.TwoPi * f / 8).ToRotationVector2() * Main.rand.NextFloat(20f, 38f);
                float progress = (float)f / 8f;
                Color flareColor = Color.Lerp(Color.Lerp(ThemedParticles.CampanellaOrange, ThemedParticles.CampanellaYellow, progress * 2f),
                    ThemedParticles.CampanellaGold, Math.Max(0, progress * 2f - 1f));
                CustomParticles.GenericFlare(flarePos, flareColor, 0.45f, 14);
            }
            
            // === PRISMATIC SPARKLES ===
            ThemedParticles.LaCampanellaPrismaticBurst(position, 6, 0.7f);
            
            // Halo rings
            CustomParticles.HaloRing(position, ThemedParticles.CampanellaOrange, 0.45f, 15);
            CustomParticles.HaloRing(position, ThemedParticles.CampanellaYellow, 0.3f, 12);
            
            // Radial flares
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 flarePos = position + angle.ToRotationVector2() * Main.rand.NextFloat(25f, 40f);
                Color flareColor = Main.rand.NextBool() ? ThemedParticles.CampanellaOrange : ThemedParticles.CampanellaGold;
                CustomParticles.GenericFlare(flarePos, flareColor, 0.4f, 14);
            }
            
            // Music notes
            ThemedParticles.LaCampanellaMusicNotes(position, 4, 30f);
            
            // Screen shake
            Player.GetModPlayer<ScreenShakePlayer>()?.AddShake(4f, 8);
            
            Lighting.AddLight(position, 1f, 0.5f, 0.15f);
            
            // AOE damage
            float explosionRadius = 100f;
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                
                if (Vector2.Distance(position, npc.Center) <= explosionRadius)
                {
                    npc.SimpleStrikeNPC(75, 0, false, 0f, null, false, 0f, true);
                    npc.GetGlobalNPC<ResonantTollNPC>().AddStacks(npc, 1);
                    
                    // Hit effect on each enemy with flares
                    ThemedParticles.LaCampanellaSparks(npc.Center, (npc.Center - position).SafeNormalize(Vector2.UnitX), 4, 4f);
                    ThemedParticles.LaCampanellaPrismaticSparkles(npc.Center, 2, 0.4f);
                    CustomParticles.HaloRing(npc.Center, ThemedParticles.CampanellaOrange, 0.25f, 10);
                    // Flares around enemy
                    for (int f = 0; f < 3; f++)
                    {
                        Vector2 flarePos = npc.Center + (MathHelper.TwoPi * f / 3).ToRotationVector2() * 12f;
                        Color flareColor = Main.rand.NextBool() ? ThemedParticles.CampanellaYellow : ThemedParticles.CampanellaOrange;
                        CustomParticles.GenericFlare(flarePos, flareColor, 0.35f, 10);
                    }
                }
            }
        }
    }
    
    #endregion

    #region Campanella's Pyre Medallion
    
    /// <summary>
    /// Campanella's Pyre Medallion - La Campanella accessory.
    /// Enhances Resonant Toll debuff, grants crit chance against burning enemies,
    /// and leaves a trail of flame when dashing.
    /// </summary>
    public class CampanellasPyreMedallion : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 36;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 40);
            Item.rare = ModContent.RarityType<LaCampanellaRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CampanellasPyreMedallionPlayer modPlayer = player.GetModPlayer<CampanellasPyreMedallionPlayer>();
            modPlayer.pyreMedallionEquipped = true;
            
            // Crit chance boost
            player.GetCritChance(DamageClass.Generic) += 15f;
            
            // Attack speed
            player.GetAttackSpeed(DamageClass.Generic) += 0.08f;
            
            // === SIGNATURE ACCESSORY AURA - VIBRANT PARTICLES! ===
            if (!hideVisual)
            {
                ThemedParticles.LaCampanellaHoldAura(player.Center, 0.6f);
                
                // Visual
                if (Main.rand.NextBool(15))
                {
                    Dust glow = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(20f, 20f),
                        DustID.Torch, -player.velocity * 0.1f, 100, ThemedParticles.CampanellaYellow, 1f);
                    glow.noGravity = true;
                }
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color campanellaOrange = new Color(255, 140, 40);
            Color campanellaGold = new Color(255, 200, 80);
            Color campanellaYellow = new Color(255, 220, 100);

            tooltips.Add(new TooltipLine(Mod, "Tier", "Tier 2 La Campanella Accessory")
            {
                OverrideColor = campanellaOrange
            });
            tooltips.Add(new TooltipLine(Mod, "Stats", "+15% critical strike chance")
            {
                OverrideColor = campanellaGold
            });
            tooltips.Add(new TooltipLine(Mod, "Speed", "+8% attack speed")
            {
                OverrideColor = campanellaGold
            });
            tooltips.Add(new TooltipLine(Mod, "DamageBonus", "+20% damage against enemies with Resonant Toll stacks")
            {
                OverrideColor = campanellaYellow
            });
            tooltips.Add(new TooltipLine(Mod, "FlameTrail", "Leaves a trail of fire when dashing")
            {
                OverrideColor = campanellaOrange
            });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The pyre burns brightest for those who embrace the inferno'")
            {
                OverrideColor = Color.Lerp(campanellaOrange, Color.Black, 0.3f)
            });
        }
    }

    public class CampanellasPyreMedallionPlayer : ModPlayer
    {
        public bool pyreMedallionEquipped = false;
        private Vector2 lastPosition;
        
        public override void ResetEffects()
        {
            pyreMedallionEquipped = false;
        }

        public override void PostUpdate()
        {
            if (!pyreMedallionEquipped)
            {
                lastPosition = Player.Center;
                return;
            }
            
            // Flame trail when moving fast (dashing)
            float speed = Vector2.Distance(Player.Center, lastPosition);
            if (speed > 10f)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 trailPos = Vector2.Lerp(lastPosition, Player.Center, i / 2f);
                    
                    Dust flame = Dust.NewDustPerfect(trailPos + Main.rand.NextVector2Circular(5f, 5f),
                        DustID.Torch, Vector2.Zero, 100, new Color(255, 100, 0), 1.5f);
                    flame.noGravity = true;
                    flame.fadeIn = 0.3f;
                }
            }
            
            lastPosition = Player.Center;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!pyreMedallionEquipped) return;
            
            // Extra damage to enemies with Resonant Toll
            if (target.GetGlobalNPC<ResonantTollNPC>().Stacks > 0)
            {
                modifiers.FinalDamage += 0.2f; // +20% damage to afflicted enemies
            }
        }
    }
    
    #endregion

    #region Symphony of the Blazing Sanctuary
    
    /// <summary>
    /// Symphony of the Blazing Sanctuary - Tier 3 La Campanella accessory.
    /// Creates protective bell barrier on low health, grants regen near fire,
    /// and killing enemies creates healing fire pillars.
    /// </summary>
    public class SymphonyOfTheBlazingSanctuary : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<LaCampanellaRainbowRarity>();
            Item.defense = 10;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            BlazingSanctuaryPlayer modPlayer = player.GetModPlayer<BlazingSanctuaryPlayer>();
            modPlayer.blazingSanctuaryEquipped = true;
            
            // Life regen near fire sources (lava, fire blocks, etc.)
            player.lifeRegen += 4;
            
            // Max life boost
            player.statLifeMax2 += 40;
            
            // Defense when below half health
            if (player.statLife < player.statLifeMax2 / 2)
            {
                player.statDefense += 15;
            }
            
            // === SIGNATURE ACCESSORY AURA - VIBRANT PARTICLES! ===
            if (!hideVisual)
            {
                ThemedParticles.LaCampanellaHoldAura(player.Center, 0.7f);
                
                // Visual aura
                if (Main.rand.NextBool(10))
                {
                    float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                    Vector2 offset = angle.ToRotationVector2() * 40f;
                    
                    Dust orbit = Dust.NewDustPerfect(player.Center + offset, DustID.Torch,
                        angle.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * 2f, 100, ThemedParticles.CampanellaYellow, 1.3f);
                    orbit.noGravity = true;
                }
            }
            
            Lighting.AddLight(player.Center, 0.5f, 0.3f, 0.1f);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color campanellaOrange = new Color(255, 140, 40);
            Color campanellaGold = new Color(255, 200, 80);
            Color campanellaYellow = new Color(255, 220, 100);

            tooltips.Add(new TooltipLine(Mod, "Tier", "Tier 3 La Campanella Accessory")
            {
                OverrideColor = campanellaOrange
            });
            tooltips.Add(new TooltipLine(Mod, "Defense", "+10 defense (+15 when below 50% health)")
            {
                OverrideColor = campanellaGold
            });
            tooltips.Add(new TooltipLine(Mod, "MaxLife", "+40 maximum life")
            {
                OverrideColor = campanellaGold
            });
            tooltips.Add(new TooltipLine(Mod, "Regen", "+4 life regeneration")
            {
                OverrideColor = campanellaGold
            });
            tooltips.Add(new TooltipLine(Mod, "BellBarrier", "Protective bell barrier when below 30% health (70% damage reduction)")
            {
                OverrideColor = campanellaYellow
            });
            tooltips.Add(new TooltipLine(Mod, "Cooldown", "Bell barrier has a 30 second cooldown")
            {
                OverrideColor = Color.Gray
            });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Within the blazing sanctuary, even the flames sing prayers of protection'")
            {
                OverrideColor = Color.Lerp(campanellaOrange, Color.Black, 0.3f)
            });
        }
    }

    public class BlazingSanctuaryPlayer : ModPlayer
    {
        public bool blazingSanctuaryEquipped = false;
        private int barrierCooldown = 0;
        private bool barrierTriggeredThisHit = false;
        
        public override void ResetEffects()
        {
            blazingSanctuaryEquipped = false;
            barrierTriggeredThisHit = false;
        }

        public override void PostUpdate()
        {
            if (barrierCooldown > 0)
                barrierCooldown--;
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (!blazingSanctuaryEquipped) return;
            
            // Bell barrier on low health - 70% damage reduction
            if (Player.statLife < Player.statLifeMax2 * 0.3f && barrierCooldown <= 0)
            {
                modifiers.FinalDamage *= 0.3f; // 70% damage reduction
                barrierTriggeredThisHit = true;
            }
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            if (!blazingSanctuaryEquipped) return;
            
            // Trigger barrier visual/effects after damage is calculated
            if (barrierTriggeredThisHit && barrierCooldown <= 0)
            {
                TriggerBellBarrier();
                barrierCooldown = 1800; // 30 second cooldown
            }
        }

        private void TriggerBellBarrier()
        {
            // === EPIC BELL BARRIER SOUND STACK ===
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.4f, Volume = 0.85f }, Player.Center);
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = Main.rand.NextFloat(-0.1f, 0.2f), Volume = 0.65f }, Player.Center);
            SoundEngine.PlaySound(SoundID.Item34 with { Pitch = Main.rand.NextFloat(0.1f, 0.4f), Volume = 0.45f }, Player.Center);
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Pitch = 0.2f, Volume = 0.35f }, Player.Center);
            
            // === MASSIVE BARRIER VISUAL EFFECTS WITH CUSTOM PARTICLES ===
            ThemedParticles.LaCampanellaImpact(Player.Center, 3f);
            ThemedParticles.LaCampanellaBellChime(Player.Center, 2.5f);
            ThemedParticles.LaCampanellaShockwave(Player.Center, 2f);
            ThemedParticles.LaCampanellaMusicalImpact(Player.Center, 1.5f, true);
            
            // === GRAND IMPACT WITH ALL EFFECTS ===
            ThemedParticles.LaCampanellaGrandImpact(Player.Center, 2f);
            ThemedParticles.LaCampanellaHaloBurst(Player.Center, 1.5f);
            ThemedParticles.LaCampanellaSwordArcBurst(Player.Center, 8, 1f);
            ThemedParticles.LaCampanellaPrismaticBurst(Player.Center, 12, 1.1f);
            
            // === CRESCENT WAVE BARRIER ===
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                ThemedParticles.LaCampanellaCrescentWave(Player.Center, angle.ToRotationVector2(), 0.9f);
            }
            
            // Multiple massive halo rings
            for (int i = 0; i < 6; i++)
            {
                Color ringColor = i % 2 == 0 ? ThemedParticles.CampanellaOrange : ThemedParticles.CampanellaYellow;
                CustomParticles.HaloRing(Player.Center, ringColor, 0.6f + i * 0.2f, 22 + i * 4);
            }
            // Black shadow rings
            for (int i = 0; i < 3; i++)
            {
                CustomParticles.HaloRing(Player.Center, ThemedParticles.CampanellaBlack, 0.5f + i * 0.25f, 18 + i * 3);
            }
            
            // Radial flare burst with GRADIENT
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 flarePos = Player.Center + angle.ToRotationVector2() * Main.rand.NextFloat(60f, 100f);
                float progress = (float)i / 16f;
                Color flareColor = Color.Lerp(Color.Lerp(ThemedParticles.CampanellaOrange, ThemedParticles.CampanellaYellow, progress * 2f),
                    ThemedParticles.CampanellaGold, Math.Max(0, progress * 2f - 1f));
                CustomParticles.GenericFlare(flarePos, flareColor, 0.6f, 20);
            }
            
            // Ring of fire around player
            for (int i = 0; i < 36; i++)
            {
                float angle = MathHelper.TwoPi * i / 36f;
                Vector2 pos = Player.Center + angle.ToRotationVector2() * 80f;
                
                ThemedParticles.LaCampanellaSparks(pos, angle.ToRotationVector2().RotatedBy(MathHelper.PiOver2), 3, 5f);
                
                Dust flame = Dust.NewDustPerfect(pos, DustID.Torch,
                    angle.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * 3f, 100, new Color(255, 100, 0), 2f);
                flame.noGravity = true;
            }
            
            // Music notes explosion
            ThemedParticles.LaCampanellaMusicNotes(Player.Center, 12, 70f);
            
            // Screen shake
            Player.GetModPlayer<ScreenShakePlayer>()?.AddShake(10f, 18);
            
            Lighting.AddLight(Player.Center, 2f, 1f, 0.35f);
            
            // Knockback nearby enemies
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                
                if (Vector2.Distance(Player.Center, npc.Center) <= 150f)
                {
                    Vector2 knockback = (npc.Center - Player.Center).SafeNormalize(Vector2.UnitX) * 15f;
                    npc.velocity += knockback;
                    npc.GetGlobalNPC<ResonantTollNPC>().AddStacks(npc, 3);
                    
                    // Hit effects on each enemy with all effects
                    ThemedParticles.LaCampanellaSparks(npc.Center, (npc.Center - Player.Center).SafeNormalize(Vector2.UnitX), 6, 6f);
                    ThemedParticles.LaCampanellaBloomBurst(npc.Center, 0.5f);
                    ThemedParticles.LaCampanellaSwordArc(npc.Center, Main.rand.NextFloat(MathHelper.TwoPi).ToRotationVector2(), 0.5f);
                    ThemedParticles.LaCampanellaPrismaticSparkles(npc.Center, 3, 0.5f);
                    CustomParticles.HaloRing(npc.Center, ThemedParticles.CampanellaOrange, 0.35f, 12);
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!blazingSanctuaryEquipped) return;
            
            // === BLACK SMOKE SPARKLE - SIGNATURE HIT ON ACCESSORY! ===
            Vector2 hitDir = (target.Center - Player.Center).SafeNormalize(Vector2.UnitX);
            ThemedParticles.LaCampanellaBlackSmokeSparkle(target.Center, 0.5f);
            
            // Create healing fire pillar on kill
            if (target.life <= 0 && !target.SpawnedFromStatue)
            {
                SpawnHealingPillar(target.Center);
            }
        }

        private void SpawnHealingPillar(Vector2 position)
        {
            // === BELL CHIME SOUND ===
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = Main.rand.NextFloat(0.3f, 0.6f), Volume = 0.45f }, position);
            SoundEngine.PlaySound(SoundID.Item34 with { Pitch = Main.rand.NextFloat(0.2f, 0.5f), Volume = 0.3f }, position);
            
            // === FIRE PILLAR EFFECTS ===
            ThemedParticles.LaCampanellaBellChime(position, 0.8f);
            
            // Visual pillar with custom particles
            for (int i = 0; i < 20; i++)
            {
                Vector2 pillarPos = position + new Vector2(0, -i * 10f);
                Color color = Main.rand.NextBool() ? ThemedParticles.CampanellaOrange : ThemedParticles.CampanellaYellow;
                var glow = new GenericGlowParticle(pillarPos + Main.rand.NextVector2Circular(10f, 5f),
                    new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-3f, -1.5f)),
                    color, Main.rand.NextFloat(0.3f, 0.5f), Main.rand.Next(15, 30), true);
                MagnumParticleHandler.SpawnParticle(glow);
                
                Dust flame = Dust.NewDustPerfect(pillarPos,
                    DustID.Torch, new Vector2(Main.rand.NextFloat(-1f, 1f), -2f), 100, new Color(255, 150, 50), 2f);
                flame.noGravity = true;
            }
            
            // Halo rings at base
            CustomParticles.HaloRing(position, ThemedParticles.CampanellaOrange, 0.4f, 14);
            CustomParticles.HaloRing(position, ThemedParticles.CampanellaGold, 0.25f, 10);
            
            // Sparks rising
            for (int i = 0; i < 6; i++)
            {
                ThemedParticles.LaCampanellaSparks(position, new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f), 2, 5f);
            }
            
            // Heal player if nearby
            if (Vector2.Distance(Player.Center, position) <= 200f)
            {
                Player.Heal(15);
                
                // Healing effect
                CombatText.NewText(Player.Hitbox, new Color(255, 150, 100), 15, false, true);
            }
            
            Lighting.AddLight(position, 0.8f, 0.4f, 0.1f);
        }
    }
    
    #endregion

    #region Infernal Bell of the Maestro
    
    /// <summary>
    /// Infernal Bell of the Maestro - Tier 4/Ultimate La Campanella accessory.
    /// Combines all previous effects, grants fire mastery, periodic Grand Tolling AOE,
    /// and transforms player into a walking inferno during combat.
    /// </summary>
    public class InfernalBellOfTheMaestro : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 75);
            Item.rare = ModContent.RarityType<LaCampanellaRainbowRarity>();
            Item.defense = 15;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            InfernalMaestroPlayer modPlayer = player.GetModPlayer<InfernalMaestroPlayer>();
            modPlayer.infernalMaestroEquipped = true;
            
            // All immunities
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.OnFire3] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.lavaImmune = true;
            player.fireWalk = true;
            
            // Major stat boosts
            player.GetDamage(DamageClass.Generic) += 0.2f;
            player.GetCritChance(DamageClass.Generic) += 20f;
            player.GetAttackSpeed(DamageClass.Generic) += 0.15f;
            player.statLifeMax2 += 60;
            player.lifeRegen += 8;
            player.moveSpeed += 0.15f;
            
            // Fire mastery - attacks deal fire damage
            modPlayer.fireInfusionActive = true;
            
            // === SIGNATURE ACCESSORY AURA - ULTIMATE VIBRANT PARTICLES! ===
            if (!hideVisual)
            {
                ThemedParticles.LaCampanellaHoldAura(player.Center, 1f);
                
                // Infernal aura visual - Fire swirl
                for (int i = 0; i < 2; i++)
                {
                    float angle = Main.GameUpdateCount * 0.05f + i * MathHelper.Pi;
                    Vector2 offset = angle.ToRotationVector2() * (50f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 10f);
                    
                    Dust flame = Dust.NewDustPerfect(player.Center + offset, DustID.Torch,
                        angle.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * 3f, 100, ThemedParticles.CampanellaOrange, 2f);
                    flame.noGravity = true;
                }
                
                // Random sparks
                if (Main.rand.NextBool(3))
                {
                    Vector2 sparkPos = player.Center + Main.rand.NextVector2Circular(40f, 40f);
                    Dust spark = Dust.NewDustPerfect(sparkPos, DustID.Torch,
                        Main.rand.NextVector2Circular(2f, 2f), 100, ThemedParticles.CampanellaYellow, 1.5f);
                    spark.noGravity = true;
                }
            }
            
            Lighting.AddLight(player.Center, 0.8f, 0.4f, 0.15f);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color campanellaOrange = new Color(255, 140, 40);
            Color campanellaGold = new Color(255, 200, 80);
            Color campanellaYellow = new Color(255, 220, 100);

            tooltips.Add(new TooltipLine(Mod, "Tier", "Ultimate La Campanella Accessory")
            {
                OverrideColor = campanellaOrange
            });
            tooltips.Add(new TooltipLine(Mod, "Defense", "+15 defense")
            {
                OverrideColor = campanellaGold
            });
            tooltips.Add(new TooltipLine(Mod, "Damage", "+20% damage")
            {
                OverrideColor = campanellaGold
            });
            tooltips.Add(new TooltipLine(Mod, "Crit", "+20% critical strike chance")
            {
                OverrideColor = campanellaGold
            });
            tooltips.Add(new TooltipLine(Mod, "Speed", "+15% attack speed and movement speed")
            {
                OverrideColor = campanellaGold
            });
            tooltips.Add(new TooltipLine(Mod, "MaxLife", "+60 maximum life")
            {
                OverrideColor = campanellaGold
            });
            tooltips.Add(new TooltipLine(Mod, "Regen", "+8 life regeneration")
            {
                OverrideColor = campanellaGold
            });
            tooltips.Add(new TooltipLine(Mod, "GrandTolling", "Grand Tolling: Powerful bell explosion every 5 seconds")
            {
                OverrideColor = campanellaYellow
            });
            tooltips.Add(new TooltipLine(Mod, "FireInfusion", "Fire Mastery: Attacks deal additional fire damage")
            {
                OverrideColor = campanellaYellow
            });
            tooltips.Add(new TooltipLine(Mod, "AuraDamage", "Infernal aura damages nearby enemies")
            {
                OverrideColor = campanellaYellow
            });
            tooltips.Add(new TooltipLine(Mod, "Immunities", "Immunity to On Fire!, Burning, and lava")
            {
                OverrideColor = campanellaOrange
            });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The Maestro commands the inferno itself, conducting symphonies of destruction'")
            {
                OverrideColor = Color.Lerp(campanellaOrange, Color.Black, 0.3f)
            });
        }
    }

    public class InfernalMaestroPlayer : ModPlayer
    {
        public bool infernalMaestroEquipped = false;
        public bool fireInfusionActive = false;
        private int grandTollingTimer = 0;
        private int combatTimer = 0;
        private bool inCombat = false;
        
        public override void ResetEffects()
        {
            infernalMaestroEquipped = false;
            fireInfusionActive = false;
        }

        public override void PostUpdate()
        {
            if (!infernalMaestroEquipped) return;
            
            // Track combat state
            if (combatTimer > 0)
            {
                combatTimer--;
                inCombat = true;
            }
            else
            {
                inCombat = false;
            }
            
            // Grand Tolling timer
            grandTollingTimer++;
            if (grandTollingTimer >= 300) // Every 5 seconds
            {
                grandTollingTimer = 0;
                TriggerGrandTolling();
            }
            
            // Passive aura damage
            if (Main.GameUpdateCount % 15 == 0)
            {
                DamageNearbyEnemies();
            }
        }

        private void TriggerGrandTolling()
        {
            // === EPIC GRAND TOLLING SOUND STACK ===
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.6f, Volume = 0.85f }, Player.Center);
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = Main.rand.NextFloat(-0.3f, 0f), Volume = 0.6f }, Player.Center);
            SoundEngine.PlaySound(SoundID.Item34 with { Pitch = Main.rand.NextFloat(0f, 0.3f), Volume = 0.4f }, Player.Center);
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Pitch = 0.1f, Volume = 0.35f }, Player.Center);
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.2f, Volume = 0.3f }, Player.Center);
            
            // === MASSIVE GRAND TOLLING VISUAL EFFECTS WITH CUSTOM PARTICLES ===
            ThemedParticles.LaCampanellaImpact(Player.Center, 2.5f);
            ThemedParticles.LaCampanellaBellChime(Player.Center, 2.5f);
            ThemedParticles.LaCampanellaShockwave(Player.Center, 2f);
            ThemedParticles.LaCampanellaMusicalImpact(Player.Center, 1.8f, true);
            
            // === GRAND IMPACT - ULTIMATE EFFECTS ===
            ThemedParticles.LaCampanellaGrandImpact(Player.Center, 2.5f);
            ThemedParticles.LaCampanellaHaloBurst(Player.Center, 2f);
            ThemedParticles.LaCampanellaSwordArcBurst(Player.Center, 12, 1.3f);
            ThemedParticles.LaCampanellaPrismaticBurst(Player.Center, 18, 1.4f);
            
            // === CRESCENT WAVE EXPLOSION ===
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                ThemedParticles.LaCampanellaCrescentWave(Player.Center, angle.ToRotationVector2(), 1.1f);
            }
            
            // Multiple massive halo rings
            for (int i = 0; i < 7; i++)
            {
                Color ringColor = i % 2 == 0 ? ThemedParticles.CampanellaOrange : ThemedParticles.CampanellaYellow;
                CustomParticles.HaloRing(Player.Center, ringColor, 0.7f + i * 0.2f, 24 + i * 5);
            }
            // Black shadow rings
            for (int i = 0; i < 4; i++)
            {
                CustomParticles.HaloRing(Player.Center, ThemedParticles.CampanellaBlack, 0.55f + i * 0.25f, 20 + i * 4);
            }
            
            // Massive radial flare burst with GRADIENT
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Vector2 flarePos = Player.Center + angle.ToRotationVector2() * Main.rand.NextFloat(70f, 120f);
                float progress = (float)i / 20f;
                Color flareColor = Color.Lerp(Color.Lerp(ThemedParticles.CampanellaOrange, ThemedParticles.CampanellaYellow, progress * 2f),
                    ThemedParticles.CampanellaGold, Math.Max(0, progress * 2f - 1f));
                CustomParticles.GenericFlare(flarePos, flareColor, 0.65f, 22);
            }
            
            // Massive radial spark burst
            for (int i = 0; i < 32; i++)
            {
                float angle = MathHelper.TwoPi * i / 32f;
                ThemedParticles.LaCampanellaSparks(Player.Center, angle.ToRotationVector2(), 4, 10f);
            }
            
            // Music notes explosion
            ThemedParticles.LaCampanellaMusicNotes(Player.Center, 16, 80f);
            
            // Black smoke explosion
            for (int i = 0; i < 12; i++)
            {
                var smoke = new HeavySmokeParticle(Player.Center + Main.rand.NextVector2Circular(30f, 30f),
                    Main.rand.NextVector2Circular(4f, 4f) + new Vector2(0, -1.5f),
                    ThemedParticles.CampanellaBlack, Main.rand.Next(45, 70),
                    Main.rand.NextFloat(0.6f, 0.9f), 0.6f, 0.018f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Screen shake - DRAMATIC
            Player.GetModPlayer<ScreenShakePlayer>()?.AddShake(12f, 20);
            
            Lighting.AddLight(Player.Center, 2.2f, 1.1f, 0.4f);
            
            // AOE damage wave
            float waveRadius = 300f;
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                
                float distance = Vector2.Distance(Player.Center, npc.Center);
                if (distance <= waveRadius)
                {
                    int damage = (int)(100 * (1f - distance / waveRadius)); // Damage falloff with distance
                    npc.SimpleStrikeNPC(damage, 0, false, 0f, null, false, 0f, true);
                    npc.GetGlobalNPC<ResonantTollNPC>().AddStacks(npc, 2);
                    
                    // Effects on each hit enemy
                    ThemedParticles.LaCampanellaSparks(npc.Center, (npc.Center - Player.Center).SafeNormalize(Vector2.UnitX), 3, 3f);
                    ThemedParticles.LaCampanellaSwordArc(npc.Center, Main.rand.NextFloat(MathHelper.TwoPi).ToRotationVector2(), 0.4f);
                    ThemedParticles.LaCampanellaPrismaticSparkles(npc.Center, 2, 0.4f);
                }
            }
        }

        private void DamageNearbyEnemies()
        {
            float auraRadius = 150f;
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                
                if (Vector2.Distance(Player.Center, npc.Center) <= auraRadius)
                {
                    npc.SimpleStrikeNPC(30, 0, false, 0f, null, false, 0f, true);
                    npc.GetGlobalNPC<ResonantTollNPC>().AddStacks(npc, 1);
                    
                    // Mark as in combat
                    combatTimer = 180; // 3 seconds
                    
                    // ========================================
                    // AURA BURN EFFECTS - Passive bell damage
                    // ========================================
                    
                    // Bell burn sound
                    SoundEngine.PlaySound(SoundID.Item34 with { Pitch = Main.rand.NextFloat(0.2f, 0.6f), Volume = 0.2f }, npc.Center);
                    
                    // Fire particle burst
                    ThemedParticles.LaCampanellaSparks(npc.Center, Vector2.UnitY * -1f, 4, 3f);
                    
                    // Small halo ring
                    CustomParticles.HaloRing(npc.Center, ThemedParticles.CampanellaOrange, Main.rand.NextFloat(0.3f, 0.5f), 25);
                    
                    // Flame glow particle
                    var flameGlow = new GenericGlowParticle(npc.Center,
                        Main.rand.NextVector2Circular(2f, 2f) + Vector2.UnitY * -1.5f,
                        ThemedParticles.CampanellaOrange, Main.rand.NextFloat(0.4f, 0.6f), Main.rand.Next(15, 25));
                    MagnumParticleHandler.SpawnParticle(flameGlow);
                    
                    Lighting.AddLight(npc.Center, 0.8f, 0.4f, 0.1f);
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!infernalMaestroEquipped) return;
            
            // Mark as in combat
            combatTimer = 180;
            
            // Always apply Resonant Toll
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);
            
            // === BLACK SMOKE SPARKLE - ULTIMATE SIGNATURE HIT! ===
            Vector2 hitDir = (target.Center - Player.Center).SafeNormalize(Vector2.UnitX);
            ThemedParticles.LaCampanellaSignatureHit(target.Center, hitDir, 0.6f);
            
            // Fire burst on crit
            if (hit.Crit)
            {
                // ========================================
                // CRITICAL HIT - Chainsaw bell crit sound!
                // ========================================
                
                // Layered crit sound
                SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.5f, Volume = 0.5f }, target.Center);
                SoundEngine.PlaySound(SoundID.Item34 with { Pitch = Main.rand.NextFloat(0.1f, 0.4f), Volume = 0.35f }, target.Center);
                SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot with { Pitch = 0.3f, Volume = 0.25f }, target.Center);
                
                // Extra black smoke sparkle on crit!
                ThemedParticles.LaCampanellaBlackSmokeSparkle(target.Center, 0.8f);
                
                // Massive bloom burst
                ThemedParticles.LaCampanellaBloomBurst(target.Center, 1.2f);
                ThemedParticles.LaCampanellaSparks(target.Center, hit.HitDirection > 0 ? Vector2.UnitX : -Vector2.UnitX, 6, 4f);
                ThemedParticles.LaCampanellaImpact(target.Center, 1f);
                
                // Halo rings on crit
                CustomParticles.HaloRing(target.Center, ThemedParticles.CampanellaYellow, Main.rand.NextFloat(0.7f, 1f), 20);
                CustomParticles.HaloRing(target.Center, ThemedParticles.CampanellaOrange, Main.rand.NextFloat(0.5f, 0.8f), 25);
                
                // Radial flares
                for (int i = 0; i < 4; i++)
                {
                    float angle = MathHelper.TwoPi / 4 * i + Main.rand.NextFloat(-0.2f, 0.2f);
                    Vector2 flarePos = target.Center + angle.ToRotationVector2() * 20f;
                    CustomParticles.GenericFlare(flarePos, ThemedParticles.CampanellaGold, 0.6f, Main.rand.Next(8, 14));
                }
                
                // Small screen shake on crit
                Player.GetModPlayer<ScreenShakePlayer>()?.AddShake(4f, 6);
                
                Lighting.AddLight(target.Center, 1.4f, 0.7f, 0.2f);
                
                // Extra fire damage
                target.SimpleStrikeNPC(damageDone / 4, 0, false, 0f, null, false, 0f, true);
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!infernalMaestroEquipped) return;
            
            // Bonus damage to enemies with Resonant Toll
            int stacks = target.GetGlobalNPC<ResonantTollNPC>().Stacks;
            if (stacks > 0)
            {
                modifiers.FinalDamage += 0.05f * stacks; // +5% per stack
            }
        }
    }
    
    #endregion
}
