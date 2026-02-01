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
using MagnumOpus.Content.LaCampanella.Accessories;
using MagnumOpus.Content.EnigmaVariations.Accessories;
using MagnumOpus.Content.SwanLake.Accessories;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.HarmonicCores;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.Eroica.HarmonicCores;
using MagnumOpus.Content.LaCampanella.ResonanceEnergies;
using MagnumOpus.Content.LaCampanella.HarmonicCores;
using MagnumOpus.Content.EnigmaVariations.ResonanceEnergies;
using MagnumOpus.Content.EnigmaVariations.HarmonicCores;
using MagnumOpus.Content.SwanLake.ResonanceEnergies;
using MagnumOpus.Content.SwanLake.HarmonicCores;

namespace MagnumOpus.Content.Common.Accessories
{
    #region Nocturne of Azure Flames - Moonlight + La Campanella
    /// <summary>
    /// Phase 4 Two-Theme Combination: Moonlight Sonata + La Campanella
    /// Combines lunar mysticism with infernal flames
    /// Fire burns blue at night for extra damage
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
            modPlayer.nocturneOfAzureFlamesEquipped = true;
            
            bool isNight = !Main.dayTime;
            
            // Moonlight bonuses
            if (isNight)
            {
                player.GetDamage(DamageClass.Generic) += 0.18f;
                player.GetCritChance(DamageClass.Generic) += 20;
                player.statDefense += 12;
            }
            else
            {
                player.GetDamage(DamageClass.Generic) += 0.10f;
            }
            
            // Campanella bonuses - magic damage and mana
            player.GetDamage(DamageClass.Magic) += 0.22f;
            player.GetCritChance(DamageClass.Magic) += 10;
            player.manaCost -= 0.12f;
            
            // Fire immunity from Campanella
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            
            // Blue fire at night (handled in player class)
            
            // Ambient VFX - Fusion of moonlight and flames
            if (!hideVisual)
            {
                Color flameColor = isNight ? new Color(100, 150, 255) : CampanellaColors.Orange;
                
                // Lunar fire particles
                if (Main.rand.NextBool(8))
                {
                    Vector2 offset = Main.rand.NextVector2Circular(35f, 35f);
                    Vector2 velocity = new Vector2(0, -Main.rand.NextFloat(1f, 2f));
                    
                    var flame = new GenericGlowParticle(
                        player.Center + offset, velocity,
                        flameColor * 0.7f, 0.35f, 20, true);
                    MagnumParticleHandler.SpawnParticle(flame);
                }
                
                // Crescent moon flares at night
                if (isNight && Main.rand.NextBool(15))
                {
                    float angle = Main.GameUpdateCount * 0.03f;
                    Vector2 moonPos = player.Center + angle.ToRotationVector2() * 40f;
                    CustomParticles.GenericFlare(moonPos, MoonlightColors.Silver, 0.3f, 15);
                }
                
                // Fire wisps
                if (Main.rand.NextBool(12))
                {
                    Vector2 wispPos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                    CustomParticles.GenericFlare(wispPos, flameColor * 0.6f, 0.25f, 12);
                }
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            bool isNight = !Main.dayTime;
            Color moonBlue = new Color(100, 150, 255);
            Color flameOrange = new Color(255, 140, 40);
            
            tooltips.Add(new TooltipLine(Mod, "Combo", "Combines: Moonlight Sonata + La Campanella")
            {
                OverrideColor = moonBlue
            });
            
            tooltips.Add(new TooltipLine(Mod, "MoonlightStats", "At night: +18% damage, +20 crit, +12 defense | Day: +10% damage")
            {
                OverrideColor = MoonlightColors.Silver
            });
            
            tooltips.Add(new TooltipLine(Mod, "CampanellaStats", "+22% magic damage, +10 magic crit, -12% mana cost")
            {
                OverrideColor = flameOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Special", "Fire burns blue at night, dealing 15% bonus damage")
            {
                OverrideColor = moonBlue
            });
            
            tooltips.Add(new TooltipLine(Mod, "Immunities", "Immunity to On Fire! and Burning")
            {
                OverrideColor = flameOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Azure flames dance beneath the moonlit sky'")
            {
                OverrideColor = new Color(150, 180, 220)
            });
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
        public bool nocturneOfAzureFlamesEquipped;
        private int bellRingCooldown;

        public override void ResetEffects()
        {
            nocturneOfAzureFlamesEquipped = false;
        }

        public override void PostUpdate()
        {
            if (bellRingCooldown > 0) bellRingCooldown--;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!nocturneOfAzureFlamesEquipped) return;
            if (proj.owner != Player.whoAmI) return;
            
            bool isNight = !Main.dayTime;
            
            // Blue fire at night does bonus damage
            if (isNight && DamageClass.Magic.CountsAsClass(proj.DamageType))
            {
                // 15% bonus as blue fire damage
                int bonusDamage = (int)(damageDone * 0.15f);
                target.SimpleStrikeNPC(bonusDamage, 0, false, 0, null, false, 0, true);
                
                // Blue fire VFX
                Color blueFlame = new Color(100, 150, 255);
                CustomParticles.GenericFlare(target.Center, blueFlame, 0.5f, 15);
                
                for (int i = 0; i < 4; i++)
                {
                    Vector2 flameVel = Main.rand.NextVector2Circular(3f, 3f) + new Vector2(0, -2f);
                    var flame = new GenericGlowParticle(target.Center, flameVel, blueFlame * 0.7f, 0.3f, 18, true);
                    MagnumParticleHandler.SpawnParticle(flame);
                }
            }
            
            // Bell ring stun from Campanella (10% at night, 8% during day)
            float stunChance = isNight ? 0.10f : 0.08f;
            if (bellRingCooldown <= 0 && Main.rand.NextFloat() < stunChance)
            {
                bellRingCooldown = 30;
                target.AddBuff(BuffID.Confused, 90);
                
                // Bell chime VFX
                Color chimeColor = isNight ? new Color(100, 150, 255) : CampanellaColors.Orange;
                CustomParticles.GenericFlare(target.Center, chimeColor, 0.6f, 18);
                CustomParticles.HaloRing(target.Center, chimeColor, 0.4f, 15);
                
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item35 with { Pitch = isNight ? 0.5f : 0f }, target.Center);
            }
        }
    }
    #endregion

    #region Valse Macabre - Eroica + Enigma Variations
    /// <summary>
    /// Phase 4 Two-Theme Combination: Eroica + Enigma Variations
    /// Combines heroic valor with mysterious paradoxes
    /// Heroic kills spread Paradox debuffs to nearby enemies
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
            
            // Eroica bonuses - melee
            player.GetDamage(DamageClass.Melee) += 0.20f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.15f;
            player.GetCritChance(DamageClass.Melee) += 10;
            player.GetDamage(DamageClass.Generic) += 0.08f;
            
            // Enigma bonuses - all damage
            player.GetDamage(DamageClass.Generic) += 0.18f;
            player.GetCritChance(DamageClass.Generic) += 8;
            
            // Ambient VFX - Sakura petals transforming into question marks
            if (!hideVisual)
            {
                // Sakura-mystery fusion particles
                if (Main.rand.NextBool(10))
                {
                    Vector2 offset = Main.rand.NextVector2Circular(40f, 40f);
                    Color color = Color.Lerp(EroicaColors.Sakura, 
                        EnigmaColors.Purple, Main.rand.NextFloat());
                    
                    CustomParticles.GenericFlare(player.Center + offset, color * 0.6f, 0.28f, 15);
                }
                
                // Orbiting glyphs with golden edges
                if (Main.rand.NextBool(18))
                {
                    float angle = Main.GameUpdateCount * 0.025f;
                    Vector2 glyphPos = player.Center + angle.ToRotationVector2() * 45f;
                    Color glyphColor = Color.Lerp(EroicaColors.Gold, EnigmaColors.DeepPurple, 0.5f);
                    CustomParticles.Glyph(glyphPos, glyphColor * 0.5f, 0.35f, -1);
                }
                
                // Occasional watching eye with heroic glow
                if (Main.rand.NextBool(25))
                {
                    Vector2 eyePos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                    CustomParticles.EnigmaEyeGaze(eyePos, EroicaColors.Gold, 0.35f, null);
                }
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color eroicaGold = new Color(255, 200, 80);
            Color enigmaPurple = EnigmaColors.Purple;
            
            tooltips.Add(new TooltipLine(Mod, "Combo", "Combines: Eroica + Enigma Variations")
            {
                OverrideColor = eroicaGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "EroicaStats", "+20% melee damage, +15% melee attack speed, +10 melee crit, +8% all damage")
            {
                OverrideColor = EroicaColors.Gold
            });
            
            tooltips.Add(new TooltipLine(Mod, "EnigmaStats", "+18% all damage, +8 crit chance")
            {
                OverrideColor = EnigmaColors.GreenFlame
            });
            
            tooltips.Add(new TooltipLine(Mod, "Special", "Melee kills spread Paradox debuffs to nearby enemies")
            {
                OverrideColor = enigmaPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effects", "Heroic surge on kills, Paradox stacking on all hits")
            {
                OverrideColor = eroicaGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A dance of heroism and mystery'")
            {
                OverrideColor = new Color(200, 180, 160)
            });
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
        private int heroicSurgeTimer;
        private int invulnFramesOnKill = 60; // 1 second
        
        private static readonly int[] ParadoxDebuffs = new int[]
        {
            BuffID.Confused,
            BuffID.Slow,
            BuffID.CursedInferno,
            BuffID.Ichor,
            BuffID.ShadowFlame
        };

        public override void ResetEffects()
        {
            valseMacabreEquipped = false;
        }

        public override void PostUpdate()
        {
            if (heroicSurgeTimer > 0)
            {
                heroicSurgeTimer--;
                // Heroic Surge active
                Player.GetDamage(DamageClass.Generic) += 0.25f;
                
                // Golden-purple aura while active
                if (Main.rand.NextBool(4))
                {
                    Vector2 pos = Player.Center + Main.rand.NextVector2Circular(25f, 25f);
                    Color color = Color.Lerp(EroicaColors.Gold, EnigmaColors.Purple, Main.rand.NextFloat());
                    CustomParticles.GenericFlare(pos, color, 0.3f, 12);
                }
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            HandleValseMacabreHit(target, damageDone);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.owner == Player.whoAmI)
            {
                HandleValseMacabreHit(target, damageDone);
            }
        }

        private void HandleValseMacabreHit(NPC target, int damageDone)
        {
            if (!valseMacabreEquipped) return;
            
            // 12% Paradox chance
            if (Main.rand.NextFloat() < 0.12f)
            {
                int debuffId = ParadoxDebuffs[Main.rand.Next(ParadoxDebuffs.Length)];
                target.AddBuff(debuffId, 240);
                
                // Heroic-mystery VFX
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    Vector2 offset = angle.ToRotationVector2() * 20f;
                    Color color = Color.Lerp(EroicaColors.Scarlet, EnigmaColors.GreenFlame, (float)i / 6f);
                    CustomParticles.GenericFlare(target.Center + offset, color, 0.35f, 15);
                }
                CustomParticles.GlyphBurst(target.Center, EroicaColors.Gold, 4, 3f);
            }
            
            // Check for kill
            if (target.life <= 0 && !target.immortal)
            {
                TriggerHeroicKill(target);
            }
        }

        private void TriggerHeroicKill(NPC killedTarget)
        {
            // Brief invulnerability
            Player.immune = true;
            Player.immuneTime = Math.Max(Player.immuneTime, invulnFramesOnKill);
            
            // Trigger Heroic Surge
            heroicSurgeTimer = 300; // 5 seconds
            
            // SPREAD PARADOX to nearby enemies!
            float spreadRadius = 300f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.immortal && npc.whoAmI != killedTarget.whoAmI)
                {
                    if (Vector2.Distance(npc.Center, killedTarget.Center) <= spreadRadius)
                    {
                        // Apply random Paradox debuff
                        int debuffId = ParadoxDebuffs[Main.rand.Next(ParadoxDebuffs.Length)];
                        npc.AddBuff(debuffId, 180);
                        
                        // Spread VFX
                        Vector2 direction = (npc.Center - killedTarget.Center).SafeNormalize(Vector2.Zero);
                        for (int j = 0; j < 4; j++)
                        {
                            Vector2 pos = Vector2.Lerp(killedTarget.Center, npc.Center, j / 4f);
                            Color color = Color.Lerp(EnigmaColors.Purple, EnigmaColors.GreenFlame, j / 4f);
                            CustomParticles.GenericFlare(pos, color, 0.25f, 10);
                        }
                        
                        CustomParticles.EnigmaEyeGaze(npc.Center, EnigmaColors.GreenFlame, 0.4f, null);
                    }
                }
            }
            
            // Kill VFX burst
            CustomParticles.GenericFlare(killedTarget.Center, Color.White, 1.0f, 25);
            CustomParticles.GenericFlare(killedTarget.Center, EroicaColors.Gold, 0.8f, 22);
            ThemedParticles.SakuraPetals(killedTarget.Center, 8, 40f);
            CustomParticles.GlyphBurst(killedTarget.Center, EnigmaColors.Purple, 6, 5f);
            
            for (int i = 0; i < 6; i++)
            {
                CustomParticles.HaloRing(killedTarget.Center, 
                    Color.Lerp(EroicaColors.Scarlet, EnigmaColors.GreenFlame, i / 6f), 
                    0.3f + i * 0.1f, 15 + i * 2);
            }
        }
    }
    #endregion

    #region Reverie of the Silver Swan - Moonlight + Swan Lake
    /// <summary>
    /// Phase 4 Two-Theme Combination: Moonlight Sonata + Swan Lake
    /// Combines lunar mysticism with balletic grace
    /// Dancing under moonlight grants powerful buffs
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
            modPlayer.reverieOfTheSilverSwanEquipped = true;
            
            bool isNight = !Main.dayTime;
            
            // Moonlight bonuses
            if (isNight)
            {
                player.GetDamage(DamageClass.Generic) += 0.18f;
                player.GetCritChance(DamageClass.Generic) += 20;
                player.statDefense += 12;
            }
            else
            {
                player.GetDamage(DamageClass.Generic) += 0.10f;
            }
            
            // Swan Lake bonuses
            player.GetDamage(DamageClass.Generic) += 0.16f;
            player.GetCritChance(DamageClass.Generic) += 8;
            player.moveSpeed += 0.20f;
            player.runAcceleration *= 1.20f;
            
            // Night dancing bonus (moving at night gives extra buffs)
            if (isNight && player.velocity.Length() > 3f)
            {
                player.GetDamage(DamageClass.Generic) += 0.05f;
                player.GetCritChance(DamageClass.Generic) += 5;
                
                // Dancing VFX
                if (Main.rand.NextBool(6))
                {
                    Color danceColor = Color.Lerp(MoonlightColors.Purple, SwanColors.GetRainbow(Main.rand.NextFloat()), 0.5f);
                    CustomParticles.GenericFlare(player.Center, danceColor * 0.5f, 0.25f, 12);
                }
            }
            
            // Ambient VFX - Moonlit swan feathers
            if (!hideVisual)
            {
                // Feathers with lunar glow
                if (Main.rand.NextBool(12))
                {
                    Vector2 featherPos = player.Center + new Vector2(Main.rand.NextFloat(-30f, 30f), -15f);
                    Color featherColor = isNight ? MoonlightColors.Silver : SwanColors.White;
                    CustomParticles.SwanFeatherDrift(featherPos, featherColor, 0.4f);
                }
                
                // Rainbow-lunar sparkles
                if (Main.rand.NextBool(10))
                {
                    Vector2 sparklePos = player.Center + Main.rand.NextVector2Circular(40f, 40f);
                    Color sparkleColor = Color.Lerp(MoonlightColors.Purple, SwanColors.GetRainbow(Main.rand.NextFloat()), 0.6f);
                    
                    var sparkle = new SparkleParticle(sparklePos, Main.rand.NextVector2Circular(1f, 1f),
                        sparkleColor, 0.35f, 18);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
                
                // Crescent-feather fusion at night
                if (isNight && Main.rand.NextBool(20))
                {
                    float angle = Main.GameUpdateCount * 0.02f;
                    Vector2 pos = player.Center + angle.ToRotationVector2() * 45f;
                    CustomParticles.GenericFlare(pos, Color.Lerp(MoonlightColors.LightBlue, SwanColors.Silver, 0.5f), 0.3f, 15);
                }
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            bool isNight = !Main.dayTime;
            Color moonSilver = MoonlightColors.Silver;
            Color rainbow = SwanColors.GetRainbow((float)(Main.GameUpdateCount % 300) / 300f);
            
            tooltips.Add(new TooltipLine(Mod, "Combo", "Combines: Moonlight Sonata + Swan Lake")
            {
                OverrideColor = moonSilver
            });
            
            tooltips.Add(new TooltipLine(Mod, "MoonlightStats", "At night: +18% damage, +20 crit, +12 defense | Day: +10% damage")
            {
                OverrideColor = MoonlightColors.LightBlue
            });
            
            tooltips.Add(new TooltipLine(Mod, "SwanStats", "+16% all damage, +8 crit, +20% movement speed")
            {
                OverrideColor = rainbow
            });
            
            tooltips.Add(new TooltipLine(Mod, "Special", "Moving at night: +5% damage, +5 crit")
            {
                OverrideColor = moonSilver
            });
            
            tooltips.Add(new TooltipLine(Mod, "Dodge", isNight ? "12% chance to dodge attacks (night)" : "8% chance to dodge attacks (day)")
            {
                OverrideColor = rainbow
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Moonlit feathers dance across the water's surface'")
            {
                OverrideColor = new Color(200, 210, 240)
            });
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
        public bool reverieOfTheSilverSwanEquipped;
        private int dodgeCooldown;

        public override void ResetEffects()
        {
            reverieOfTheSilverSwanEquipped = false;
        }

        public override void PostUpdate()
        {
            if (dodgeCooldown > 0) dodgeCooldown--;
        }

        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (!reverieOfTheSilverSwanEquipped) return false;
            if (dodgeCooldown > 0) return false;
            
            bool isNight = !Main.dayTime;
            float dodgeChance = isNight ? 0.12f : 0.08f; // Higher chance at night
            
            if (Main.rand.NextFloat() < dodgeChance)
            {
                dodgeCooldown = 90;
                TriggerGracefulDodge();
                return true;
            }
            
            return false;
        }

        private void TriggerGracefulDodge()
        {
            // Moonlit ballet dodge VFX
            
            // Central flash
            CustomParticles.GenericFlare(Player.Center, Color.White, 1.2f, 25);
            CustomParticles.GenericFlare(Player.Center, MoonlightColors.Purple, 0.9f, 22);
            
            // Rainbow-lunar halos
            for (int i = 0; i < 6; i++)
            {
                Color haloColor = Color.Lerp(MoonlightColors.Silver, SwanColors.GetRainbow(i / 6f), 0.5f);
                CustomParticles.HaloRing(Player.Center, haloColor, 0.35f + i * 0.1f, 15 + i * 2);
            }
            
            // Feather burst
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 featherPos = Player.Center + angle.ToRotationVector2() * 25f;
                Color featherColor = i % 2 == 0 ? SwanColors.White : MoonlightColors.Silver;
                CustomParticles.SwanFeatherDrift(featherPos, featherColor, 0.45f);
            }
            
            // Prismatic sparkle spiral
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 sparklePos = Player.Center + angle.ToRotationVector2() * 35f;
                Color sparkleColor = SwanColors.GetRainbow((float)i / 8f);
                
                var sparkle = new SparkleParticle(sparklePos, angle.ToRotationVector2() * 3f,
                    sparkleColor, 0.4f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Brief invulnerability
            Player.immune = true;
            Player.immuneTime = 20;
            
            // Sound
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.7f }, Player.Center);
        }
    }
    #endregion

    #region Fantasia of Burning Grace - La Campanella + Swan Lake
    /// <summary>
    /// Phase 4 Two-Theme Combination: La Campanella + Swan Lake
    /// Combines infernal flames with balletic grace
    /// Fire trails shimmer with rainbow colors
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
            modPlayer.fantasiaOfBurningGraceEquipped = true;
            
            // La Campanella bonuses
            player.GetDamage(DamageClass.Magic) += 0.22f;
            player.GetCritChance(DamageClass.Magic) += 10;
            player.manaCost -= 0.12f;
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            
            // Swan Lake bonuses
            player.GetDamage(DamageClass.Generic) += 0.16f;
            player.GetCritChance(DamageClass.Generic) += 8;
            player.moveSpeed += 0.20f;
            player.runAcceleration *= 1.20f;
            
            // Ambient VFX - Rainbow fire feathers
            if (!hideVisual)
            {
                // Prismatic fire particles
                if (Main.rand.NextBool(8))
                {
                    Vector2 offset = Main.rand.NextVector2Circular(35f, 35f);
                    Vector2 velocity = new Vector2(0, -Main.rand.NextFloat(1f, 2.5f));
                    Color flameColor = Color.Lerp(CampanellaColors.Orange, 
                        SwanColors.GetRainbow(Main.rand.NextFloat()), 0.4f);
                    
                    var flame = new GenericGlowParticle(
                        player.Center + offset, velocity,
                        flameColor * 0.7f, 0.35f, 20, true);
                    MagnumParticleHandler.SpawnParticle(flame);
                }
                
                // Fire-feathers
                if (Main.rand.NextBool(12))
                {
                    Vector2 featherPos = player.Center + new Vector2(Main.rand.NextFloat(-30f, 30f), -15f);
                    Color featherColor = Color.Lerp(SwanColors.White, CampanellaColors.Orange, Main.rand.NextFloat(0.3f, 0.7f));
                    CustomParticles.SwanFeatherDrift(featherPos, featherColor, 0.4f);
                }
                
                // Rainbow sparkles in flames
                if (Main.rand.NextBool(15))
                {
                    Vector2 sparklePos = player.Center + Main.rand.NextVector2Circular(40f, 40f);
                    
                    var sparkle = new SparkleParticle(sparklePos, new Vector2(0, -1.5f),
                        SwanColors.GetRainbow(Main.rand.NextFloat()), 0.3f, 15);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color flameOrange = new Color(255, 140, 40);
            Color rainbow = SwanColors.GetRainbow((float)(Main.GameUpdateCount % 300) / 300f);
            
            tooltips.Add(new TooltipLine(Mod, "Combo", "Combines: La Campanella + Swan Lake")
            {
                OverrideColor = flameOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "CampanellaStats", "+22% magic damage, +10 magic crit, -12% mana cost")
            {
                OverrideColor = flameOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "SwanStats", "+16% all damage, +8 crit, +20% movement speed")
            {
                OverrideColor = rainbow
            });
            
            tooltips.Add(new TooltipLine(Mod, "Immunities", "Immunity to On Fire! and Burning")
            {
                OverrideColor = flameOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effects", "Bell ring AOE on magic crits, Feather dodge chance")
            {
                OverrideColor = rainbow
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Fire trails shimmer with rainbow colors in a burning ballet'")
            {
                OverrideColor = new Color(255, 200, 180)
            });
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
        public bool fantasiaOfBurningGraceEquipped;
        private int bellRingCooldown;
        private int dodgeCooldown;

        public override void ResetEffects()
        {
            fantasiaOfBurningGraceEquipped = false;
        }

        public override void PostUpdate()
        {
            if (bellRingCooldown > 0) bellRingCooldown--;
            if (dodgeCooldown > 0) dodgeCooldown--;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!fantasiaOfBurningGraceEquipped) return;
            if (proj.owner != Player.whoAmI) return;
            
            // Rainbow fire trail on magic projectiles
            if (DamageClass.Magic.CountsAsClass(proj.DamageType))
            {
                // Leave rainbow fire particles
                if (Main.rand.NextBool(3))
                {
                    Color trailColor = Color.Lerp(CampanellaColors.Orange, 
                        SwanColors.GetRainbow(Main.rand.NextFloat()), 0.5f);
                    CustomParticles.GenericFlare(proj.Center, trailColor, 0.3f, 12);
                }
            }
            
            // Bell ring stun (10%)
            if (bellRingCooldown <= 0 && Main.rand.NextFloat() < 0.10f)
            {
                bellRingCooldown = 30;
                target.AddBuff(BuffID.Confused, 90);
                
                // Rainbow bell chime
                CustomParticles.GenericFlare(target.Center, SwanColors.GetRainbow(Main.rand.NextFloat()), 0.6f, 18);
                CustomParticles.HaloRing(target.Center, CampanellaColors.Orange, 0.4f, 15);
                
                // AoE rainbow fire
                float aoeRadius = 120f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.whoAmI != target.whoAmI && !npc.immortal)
                    {
                        if (Vector2.Distance(npc.Center, target.Center) <= aoeRadius)
                        {
                            int aoeDamage = (int)(damageDone * 0.5f);
                            npc.SimpleStrikeNPC(aoeDamage, 0, false, 0, null, false, 0, true);
                            npc.AddBuff(BuffID.OnFire, 180);
                            
                            CustomParticles.GenericFlare(npc.Center, SwanColors.GetRainbow(Main.rand.NextFloat()), 0.4f, 12);
                        }
                    }
                }
                
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item35, target.Center);
            }
        }

        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (!fantasiaOfBurningGraceEquipped) return false;
            if (dodgeCooldown > 0) return false;
            
            if (Main.rand.NextFloat() < 0.10f)
            {
                dodgeCooldown = 100;
                TriggerFantasiaDodge();
                return true;
            }
            
            return false;
        }

        private void TriggerFantasiaDodge()
        {
            // Fiery rainbow burst
            CustomParticles.GenericFlare(Player.Center, Color.White, 1.2f, 25);
            
            for (int i = 0; i < 8; i++)
            {
                Color burstColor = Color.Lerp(CampanellaColors.Orange, SwanColors.GetRainbow(i / 8f), 0.5f);
                CustomParticles.HaloRing(Player.Center, burstColor, 0.35f + i * 0.1f, 15 + i * 2);
            }
            
            // Burning feathers
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 featherPos = Player.Center + angle.ToRotationVector2() * 30f;
                Color featherColor = Color.Lerp(SwanColors.White, CampanellaColors.Orange, Main.rand.NextFloat());
                CustomParticles.SwanFeatherDrift(featherPos, featherColor, 0.5f);
            }
            
            Player.immune = true;
            Player.immuneTime = 25;
            
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f }, Player.Center);
        }
    }
    #endregion

    #region Triumphant Arabesque - Eroica + Swan Lake
    /// <summary>
    /// Phase 4 Two-Theme Combination: Eroica + Swan Lake
    /// Combines heroic valor with balletic grace
    /// Graceful kills trigger Heroic Surges
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
            modPlayer.triumphantArabesqueEquipped = true;
            
            // Eroica bonuses
            player.GetDamage(DamageClass.Melee) += 0.20f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.15f;
            player.GetCritChance(DamageClass.Melee) += 10;
            player.GetDamage(DamageClass.Generic) += 0.08f;
            
            // Swan Lake bonuses
            player.GetDamage(DamageClass.Generic) += 0.16f;
            player.GetCritChance(DamageClass.Generic) += 8;
            player.moveSpeed += 0.20f;
            player.runAcceleration *= 1.20f;
            
            // Ambient VFX - Sakura petals meeting swan feathers
            if (!hideVisual)
            {
                // Sakura-feather fusion
                if (Main.rand.NextBool(10))
                {
                    Vector2 petalPos = player.Center + new Vector2(Main.rand.NextFloat(-35f, 35f), -15f);
                    Color petalColor = Main.rand.NextBool() ? 
                        EroicaColors.Sakura : SwanColors.White;
                    
                    if (Main.rand.NextBool())
                        ThemedParticles.SakuraPetals(petalPos, 1, 10f);
                    else
                        CustomParticles.SwanFeatherDrift(petalPos, petalColor, 0.4f);
                }
                
                // Golden-rainbow sparkles
                if (Main.rand.NextBool(12))
                {
                    Vector2 sparklePos = player.Center + Main.rand.NextVector2Circular(40f, 40f);
                    Color sparkleColor = Color.Lerp(EroicaColors.Gold, SwanColors.GetRainbow(Main.rand.NextFloat()), 0.5f);
                    
                    var sparkle = new SparkleParticle(sparklePos, Main.rand.NextVector2Circular(1f, 1f),
                        sparkleColor, 0.35f, 18);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color eroicaGold = new Color(255, 200, 80);
            Color rainbow = SwanColors.GetRainbow((float)(Main.GameUpdateCount % 300) / 300f);
            
            tooltips.Add(new TooltipLine(Mod, "Combo", "Combines: Eroica + Swan Lake")
            {
                OverrideColor = eroicaGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "EroicaStats", "+20% melee damage, +15% melee attack speed, +10 melee crit, +8% all damage")
            {
                OverrideColor = EroicaColors.Gold
            });
            
            tooltips.Add(new TooltipLine(Mod, "SwanStats", "+16% all damage, +8 crit, +20% movement speed")
            {
                OverrideColor = rainbow
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effects", "Melee kills trigger Heroic Surge (+25% damage), Feather dodge chance")
            {
                OverrideColor = eroicaGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Sakura petals and swan feathers dance as one'")
            {
                OverrideColor = new Color(255, 200, 220)
            });
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
        public bool triumphantArabesqueEquipped;
        private int heroicSurgeTimer;
        private int invulnFramesOnKill = 60;
        private int dodgeCooldown;

        public override void ResetEffects()
        {
            triumphantArabesqueEquipped = false;
        }

        public override void PostUpdate()
        {
            if (heroicSurgeTimer > 0)
            {
                heroicSurgeTimer--;
                Player.GetDamage(DamageClass.Generic) += 0.25f;
                
                // Golden-rainbow aura
                if (Main.rand.NextBool(4))
                {
                    Vector2 pos = Player.Center + Main.rand.NextVector2Circular(25f, 25f);
                    Color color = Color.Lerp(EroicaColors.Gold, SwanColors.GetRainbow(Main.rand.NextFloat()), 0.5f);
                    CustomParticles.GenericFlare(pos, color, 0.3f, 12);
                }
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
            if (!triumphantArabesqueEquipped) return;
            
            if (target.life <= 0 && !target.immortal)
            {
                // Invulnerability
                Player.immune = true;
                Player.immuneTime = Math.Max(Player.immuneTime, invulnFramesOnKill);
                
                // Heroic Surge
                heroicSurgeTimer = 300;
                
                // Graceful heroic kill VFX
                CustomParticles.GenericFlare(target.Center, Color.White, 1.0f, 25);
                CustomParticles.GenericFlare(target.Center, EroicaColors.Gold, 0.8f, 22);
                
                ThemedParticles.SakuraPetals(target.Center, 6, 35f);
                
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 featherPos = target.Center + angle.ToRotationVector2() * 25f;
                    CustomParticles.SwanFeatherDrift(featherPos, SwanColors.White, 0.45f);
                }
                
                for (int i = 0; i < 5; i++)
                {
                    Color haloColor = Color.Lerp(EroicaColors.Gold, SwanColors.GetRainbow(i / 5f), 0.5f);
                    CustomParticles.HaloRing(target.Center, haloColor, 0.35f + i * 0.1f, 15 + i * 2);
                }
            }
        }

        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (!triumphantArabesqueEquipped) return false;
            if (dodgeCooldown > 0) return false;
            
            if (Main.rand.NextFloat() < 0.10f)
            {
                dodgeCooldown = 100;
                
                // Graceful dodge with sakura and feathers
                CustomParticles.GenericFlare(Player.Center, Color.White, 1.0f, 20);
                
                for (int i = 0; i < 6; i++)
                {
                    CustomParticles.HaloRing(Player.Center, 
                        Color.Lerp(EroicaColors.Gold, SwanColors.GetRainbow(i / 6f), 0.5f), 
                        0.3f + i * 0.1f, 12 + i * 2);
                }
                
                ThemedParticles.SakuraPetals(Player.Center, 8, 40f);
                
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    CustomParticles.SwanFeatherDrift(Player.Center + angle.ToRotationVector2() * 20f, SwanColors.White, 0.4f);
                }
                
                Player.immune = true;
                Player.immuneTime = 20;
                
                return true;
            }
            
            return false;
        }
    }
    #endregion

    #region Inferno of Lost Shadows - La Campanella + Enigma Variations
    /// <summary>
    /// Phase 4 Two-Theme Combination: La Campanella + Enigma Variations
    /// Combines infernal flames with mysterious paradoxes
    /// Fire inflicts random Paradox debuffs
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
            modPlayer.infernoOfLostShadowsEquipped = true;
            
            // La Campanella bonuses
            player.GetDamage(DamageClass.Magic) += 0.22f;
            player.GetCritChance(DamageClass.Magic) += 10;
            player.manaCost -= 0.12f;
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            
            // Enigma bonuses
            player.GetDamage(DamageClass.Generic) += 0.18f;
            player.GetCritChance(DamageClass.Generic) += 8;
            
            // Ambient VFX - Fire burning with void edges
            if (!hideVisual)
            {
                // Void fire particles
                if (Main.rand.NextBool(8))
                {
                    Vector2 offset = Main.rand.NextVector2Circular(35f, 35f);
                    Vector2 velocity = new Vector2(0, -Main.rand.NextFloat(1f, 2.5f));
                    Color flameColor = Color.Lerp(CampanellaColors.Orange, 
                        EnigmaColors.GreenFlame, Main.rand.NextFloat(0.2f, 0.5f));
                    
                    var flame = new GenericGlowParticle(
                        player.Center + offset, velocity,
                        flameColor * 0.7f, 0.35f, 20, true);
                    MagnumParticleHandler.SpawnParticle(flame);
                }
                
                // Enigma glyphs in fire
                if (Main.rand.NextBool(18))
                {
                    float angle = Main.GameUpdateCount * 0.025f;
                    Vector2 glyphPos = player.Center + angle.ToRotationVector2() * 40f;
                    CustomParticles.Glyph(glyphPos, Color.Lerp(CampanellaColors.Orange, EnigmaColors.Purple, 0.5f) * 0.5f, 0.3f, -1);
                }
                
                // Watching eyes in flames
                if (Main.rand.NextBool(25))
                {
                    Vector2 eyePos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                    CustomParticles.EnigmaEyeGaze(eyePos, EnigmaColors.GreenFlame, 0.35f, null);
                }
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color flameOrange = new Color(255, 140, 40);
            Color greenFlame = EnigmaColors.GreenFlame;
            
            tooltips.Add(new TooltipLine(Mod, "Combo", "Combines: La Campanella + Enigma Variations")
            {
                OverrideColor = flameOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "CampanellaStats", "+22% magic damage, +10 magic crit, -12% mana cost")
            {
                OverrideColor = flameOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "EnigmaStats", "+18% all damage, +8 crit chance")
            {
                OverrideColor = greenFlame
            });
            
            tooltips.Add(new TooltipLine(Mod, "Immunities", "Immunity to On Fire! and Burning")
            {
                OverrideColor = flameOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effects", "Fire inflicts random Paradox debuffs, Bell ring AOE on crits")
            {
                OverrideColor = greenFlame
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Flames that burn with questions, shadows that whisper riddles'")
            {
                OverrideColor = new Color(180, 140, 100)
            });
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
        public bool infernoOfLostShadowsEquipped;
        private int bellRingCooldown;
        private Dictionary<int, int> paradoxStacks = new Dictionary<int, int>();
        private Dictionary<int, int> paradoxTimers = new Dictionary<int, int>();
        
        private static readonly int[] ParadoxDebuffs = new int[]
        {
            BuffID.Confused,
            BuffID.Slow,
            BuffID.CursedInferno,
            BuffID.Ichor,
            BuffID.ShadowFlame,
            BuffID.Frostburn
        };

        public override void ResetEffects()
        {
            infernoOfLostShadowsEquipped = false;
        }

        public override void PostUpdate()
        {
            if (bellRingCooldown > 0) bellRingCooldown--;
            
            // Decay paradox stacks
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
            if (!infernoOfLostShadowsEquipped) return;
            if (proj.owner != Player.whoAmI) return;
            
            // Fire inflicts random Paradox (15% for magic, 10% otherwise)
            float paradoxChance = DamageClass.Magic.CountsAsClass(proj.DamageType) ? 0.15f : 0.10f;
            
            if (Main.rand.NextFloat() < paradoxChance)
            {
                int debuffId = ParadoxDebuffs[Main.rand.Next(ParadoxDebuffs.Length)];
                target.AddBuff(debuffId, 240);
                target.AddBuff(BuffID.OnFire, 180);
                
                // Track stacks
                if (!paradoxStacks.ContainsKey(target.whoAmI))
                    paradoxStacks[target.whoAmI] = 0;
                
                paradoxStacks[target.whoAmI]++;
                paradoxTimers[target.whoAmI] = 300;
                
                int stacks = paradoxStacks[target.whoAmI];
                
                // Void fire VFX
                for (int i = 0; i < 6 + stacks; i++)
                {
                    float angle = MathHelper.TwoPi * i / (6 + stacks);
                    Vector2 offset = angle.ToRotationVector2() * (15f + stacks * 3f);
                    Color color = Color.Lerp(CampanellaColors.Orange, EnigmaColors.GreenFlame, (float)i / (6 + stacks));
                    CustomParticles.GenericFlare(target.Center + offset, color, 0.3f + stacks * 0.03f, 15);
                }
                
                CustomParticles.GlyphBurst(target.Center, EnigmaColors.Purple, 2 + stacks, 2f + stacks * 0.3f);
                
                // Void Collapse at 5 stacks
                if (stacks >= 5)
                {
                    TriggerVoidFlameCollapse(target, damageDone);
                    paradoxStacks[target.whoAmI] = 0;
                }
            }
            
            // Bell ring stun (10%)
            if (bellRingCooldown <= 0 && Main.rand.NextFloat() < 0.10f)
            {
                bellRingCooldown = 30;
                target.AddBuff(BuffID.Confused, 90);
                
                // Void bell chime
                Color chimeColor = Color.Lerp(CampanellaColors.Orange, EnigmaColors.GreenFlame, 0.4f);
                CustomParticles.GenericFlare(target.Center, chimeColor, 0.6f, 18);
                CustomParticles.HaloRing(target.Center, EnigmaColors.Purple, 0.4f, 15);
                
                // AoE Paradox + fire
                float aoeRadius = 120f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.whoAmI != target.whoAmI && !npc.immortal)
                    {
                        if (Vector2.Distance(npc.Center, target.Center) <= aoeRadius)
                        {
                            int aoeDamage = (int)(damageDone * 0.5f);
                            npc.SimpleStrikeNPC(aoeDamage, 0, false, 0, null, false, 0, true);
                            npc.AddBuff(BuffID.OnFire, 180);
                            
                            int debuffId = ParadoxDebuffs[Main.rand.Next(ParadoxDebuffs.Length)];
                            npc.AddBuff(debuffId, 120);
                            
                            CustomParticles.GenericFlare(npc.Center, EnigmaColors.GreenFlame, 0.4f, 12);
                        }
                    }
                }
                
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item35 with { Pitch = -0.3f }, target.Center);
            }
        }

        private void TriggerVoidFlameCollapse(NPC target, int baseDamage)
        {
            // Massive void fire explosion
            CustomParticles.GenericFlare(target.Center, Color.White, 1.5f, 30);
            CustomParticles.GenericFlare(target.Center, CampanellaColors.Orange, 1.2f, 28);
            CustomParticles.GenericFlare(target.Center, EnigmaColors.GreenFlame, 1.0f, 25);
            
            for (int ring = 0; ring < 10; ring++)
            {
                Color ringColor = Color.Lerp(CampanellaColors.Orange, EnigmaColors.GreenFlame, ring / 10f);
                CustomParticles.HaloRing(target.Center, ringColor, 0.4f + ring * 0.15f, 20 + ring * 3);
            }
            
            // Glyph spiral
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                float radius = 30f + i * 8f;
                Vector2 pos = target.Center + angle.ToRotationVector2() * radius;
                CustomParticles.Glyph(pos, EnigmaColors.Purple, 0.5f, -1);
            }
            
            // Eye formation
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 eyePos = target.Center + angle.ToRotationVector2() * 45f;
                CustomParticles.EnigmaEyeGaze(eyePos, EnigmaColors.GreenFlame, 0.55f, target.Center);
            }
            
            CustomParticles.ExplosionBurst(target.Center, CampanellaColors.Orange, 18, 12f);
            CustomParticles.ExplosionBurst(target.Center, EnigmaColors.GreenFlame, 12, 10f);
            
            // Deal massive damage
            if (Main.myPlayer == Player.whoAmI)
            {
                int voidDamage = (int)(baseDamage * 2.5f);
                target.SimpleStrikeNPC(voidDamage, 0, false, 0, null, false, 0, true);
                
                // AOE
                float aoeRadius = 200f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.whoAmI != target.whoAmI && !npc.immortal)
                    {
                        if (Vector2.Distance(npc.Center, target.Center) <= aoeRadius)
                        {
                            npc.SimpleStrikeNPC(voidDamage / 2, 0, false, 0, null, false, 0, true);
                            npc.AddBuff(BuffID.OnFire, 300);
                            npc.AddBuff(ParadoxDebuffs[Main.rand.Next(ParadoxDebuffs.Length)], 180);
                            
                            CustomParticles.GenericFlare(npc.Center, EnigmaColors.GreenFlame, 0.5f, 15);
                        }
                    }
                }
            }
            
            MagnumScreenEffects.AddScreenShake(12f);
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.5f, Volume = 1.3f }, target.Center);
        }
    }
    #endregion
}

