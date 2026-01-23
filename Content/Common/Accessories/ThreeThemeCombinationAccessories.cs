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
    #region Trinity of Night - Moonlight + La Campanella + Enigma
    /// <summary>
    /// Phase 4 Three-Theme Combination: Moonlight Sonata + La Campanella + Enigma Variations
    /// Ultimate darkness theme combining lunar mysticism, infernal flames, and void mystery
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
            
            // Moonlight bonuses (enhanced at night)
            if (isNight)
            {
                player.GetDamage(DamageClass.Generic) += 0.22f;
                player.GetCritChance(DamageClass.Generic) += 25;
                player.statDefense += 15;
            }
            else
            {
                player.GetDamage(DamageClass.Generic) += 0.12f;
            }
            
            // La Campanella bonuses
            player.GetDamage(DamageClass.Magic) += 0.25f;
            player.GetCritChance(DamageClass.Magic) += 12;
            player.manaCost -= 0.15f;
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            
            // Enigma bonuses
            player.GetDamage(DamageClass.Generic) += 0.20f;
            player.GetCritChance(DamageClass.Generic) += 10;
            
            // Ambient VFX - Three dark powers combined
            if (!hideVisual)
            {
                // Triple-color dark flames
                if (Main.rand.NextBool(6))
                {
                    Vector2 offset = Main.rand.NextVector2Circular(40f, 40f);
                    Vector2 velocity = new Vector2(0, -Main.rand.NextFloat(1.5f, 3f));
                    
                    // Cycle through three colors
                    Color flameColor;
                    int colorPhase = (int)((Main.GameUpdateCount / 20) % 3);
                    if (colorPhase == 0)
                        flameColor = isNight ? new Color(100, 150, 255) : MoonlightColors.Purple;
                    else if (colorPhase == 1)
                        flameColor = CampanellaColors.Orange;
                    else
                        flameColor = EnigmaColors.GreenFlame;
                    
                    var flame = new GenericGlowParticle(
                        player.Center + offset, velocity,
                        flameColor * 0.7f, 0.4f, 22, true);
                    MagnumParticleHandler.SpawnParticle(flame);
                }
                
                // Orbiting trinity particles
                if (Main.GameUpdateCount % 10 == 0)
                {
                    float baseAngle = Main.GameUpdateCount * 0.02f;
                    
                    // Moon
                    Vector2 moonPos = player.Center + baseAngle.ToRotationVector2() * 50f;
                    CustomParticles.GenericFlare(moonPos, MoonlightColors.Silver, 0.3f, 12);
                    
                    // Flame
                    Vector2 flamePos = player.Center + (baseAngle + MathHelper.TwoPi / 3f).ToRotationVector2() * 50f;
                    CustomParticles.GenericFlare(flamePos, CampanellaColors.Orange, 0.3f, 12);
                    
                    // Void
                    Vector2 voidPos = player.Center + (baseAngle + MathHelper.TwoPi * 2f / 3f).ToRotationVector2() * 50f;
                    CustomParticles.GenericFlare(voidPos, EnigmaColors.GreenFlame, 0.3f, 12);
                }
                
                // Glyphs and eyes
                if (Main.rand.NextBool(20))
                {
                    Vector2 glyphPos = player.Center + Main.rand.NextVector2Circular(45f, 45f);
                    CustomParticles.Glyph(glyphPos, EnigmaColors.DeepPurple * 0.5f, 0.35f, -1);
                }
                
                if (Main.rand.NextBool(30))
                {
                    Vector2 eyePos = player.Center + Main.rand.NextVector2Circular(40f, 40f);
                    CustomParticles.EnigmaEyeGaze(eyePos, EnigmaColors.GreenFlame, 0.35f, null);
                }
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<LunarFlames>()
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
        private int bellRingCooldown;
        private Dictionary<int, int> paradoxStacks = new Dictionary<int, int>();
        private Dictionary<int, int> paradoxTimers = new Dictionary<int, int>();
        
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
            if (bellRingCooldown > 0) bellRingCooldown--;
            
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
            if (!trinityEquipped) return;
            if (proj.owner != Player.whoAmI) return;
            
            bool isNight = !Main.dayTime;
            
            // Blue fire at night bonus
            if (isNight && DamageClass.Magic.CountsAsClass(proj.DamageType))
            {
                int bonusDamage = (int)(damageDone * 0.20f);
                target.SimpleStrikeNPC(bonusDamage, 0, false, 0, null, false, 0, true);
                
                Color blueFlame = new Color(100, 150, 255);
                CustomParticles.GenericFlare(target.Center, blueFlame, 0.5f, 15);
            }
            
            // Paradox (15%)
            if (Main.rand.NextFloat() < 0.15f)
            {
                int debuffId = ParadoxDebuffs[Main.rand.Next(ParadoxDebuffs.Length)];
                target.AddBuff(debuffId, 300);
                target.AddBuff(BuffID.OnFire, 240);
                
                if (!paradoxStacks.ContainsKey(target.whoAmI))
                    paradoxStacks[target.whoAmI] = 0;
                
                paradoxStacks[target.whoAmI]++;
                paradoxTimers[target.whoAmI] = 360;
                
                int stacks = paradoxStacks[target.whoAmI];
                
                // Trinity VFX - all three colors
                for (int i = 0; i < 9; i++)
                {
                    float angle = MathHelper.TwoPi * i / 9f;
                    Vector2 offset = angle.ToRotationVector2() * (18f + stacks * 3f);
                    
                    Color color;
                    if (i % 3 == 0)
                        color = isNight ? new Color(100, 150, 255) : MoonlightColors.Purple;
                    else if (i % 3 == 1)
                        color = CampanellaColors.Orange;
                    else
                        color = EnigmaColors.GreenFlame;
                    
                    CustomParticles.GenericFlare(target.Center + offset, color, 0.35f, 16);
                }
                
                CustomParticles.GlyphBurst(target.Center, EnigmaColors.Purple, 3 + stacks, 3f);
                
                // Void Collapse at 5 stacks
                if (stacks >= 5)
                {
                    TriggerTrinityCollapse(target, damageDone, isNight);
                    paradoxStacks[target.whoAmI] = 0;
                }
            }
            
            // Bell ring (12%)
            if (bellRingCooldown <= 0 && Main.rand.NextFloat() < 0.12f)
            {
                bellRingCooldown = 25;
                target.AddBuff(BuffID.Confused, 120);
                
                Color chimeColor = Color.Lerp(CampanellaColors.Orange, EnigmaColors.GreenFlame, 0.4f);
                CustomParticles.GenericFlare(target.Center, chimeColor, 0.7f, 20);
                CustomParticles.HaloRing(target.Center, MoonlightColors.Purple, 0.5f, 16);
                
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item35 with { Pitch = -0.2f }, target.Center);
            }
        }

        private void TriggerTrinityCollapse(NPC target, int baseDamage, bool isNight)
        {
            // Trinity explosion - all three dark powers converge
            CustomParticles.GenericFlare(target.Center, Color.White, 1.8f, 35);
            CustomParticles.GenericFlare(target.Center, isNight ? new Color(100, 150, 255) : MoonlightColors.Purple, 1.4f, 32);
            CustomParticles.GenericFlare(target.Center, CampanellaColors.Orange, 1.2f, 30);
            CustomParticles.GenericFlare(target.Center, EnigmaColors.GreenFlame, 1.0f, 28);
            
            // Triple halos
            for (int ring = 0; ring < 12; ring++)
            {
                Color ringColor;
                if (ring % 3 == 0)
                    ringColor = MoonlightColors.Purple;
                else if (ring % 3 == 1)
                    ringColor = CampanellaColors.Orange;
                else
                    ringColor = EnigmaColors.GreenFlame;
                
                CustomParticles.HaloRing(target.Center, ringColor, 0.4f + ring * 0.12f, 20 + ring * 2);
            }
            
            // Massive glyph burst
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                float radius = 35f + i * 6f;
                Vector2 pos = target.Center + angle.ToRotationVector2() * radius;
                CustomParticles.Glyph(pos, EnigmaColors.Purple, 0.55f, -1);
            }
            
            // Eye formation
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 eyePos = target.Center + angle.ToRotationVector2() * 55f;
                CustomParticles.EnigmaEyeGaze(eyePos, EnigmaColors.GreenFlame, 0.6f, target.Center);
            }
            
            CustomParticles.ExplosionBurst(target.Center, MoonlightColors.Purple, 15, 12f);
            CustomParticles.ExplosionBurst(target.Center, CampanellaColors.Orange, 15, 11f);
            CustomParticles.ExplosionBurst(target.Center, EnigmaColors.GreenFlame, 15, 10f);
            
            // Massive damage
            if (Main.myPlayer == Player.whoAmI)
            {
                int trinityDamage = (int)(baseDamage * 3.0f);
                target.SimpleStrikeNPC(trinityDamage, 0, false, 0, null, false, 0, true);
                
                float aoeRadius = 250f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.whoAmI != target.whoAmI && !npc.immortal)
                    {
                        if (Vector2.Distance(npc.Center, target.Center) <= aoeRadius)
                        {
                            npc.SimpleStrikeNPC(trinityDamage / 2, 0, false, 0, null, false, 0, true);
                            npc.AddBuff(BuffID.OnFire, 300);
                            npc.AddBuff(ParadoxDebuffs[Main.rand.Next(ParadoxDebuffs.Length)], 240);
                            
                            CustomParticles.GenericFlare(npc.Center, EnigmaColors.GreenFlame, 0.6f, 18);
                        }
                    }
                }
            }
            
            MagnumScreenEffects.AddScreenShake(15f);
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.6f, Volume = 1.4f }, target.Center);
        }
    }
    #endregion

    #region Heroic Grace - Eroica + Moonlight + Swan Lake
    /// <summary>
    /// Phase 4 Three-Theme Combination: Eroica + Moonlight Sonata + Swan Lake
    /// Ultimate noble theme combining valor, moonlight, and balletic grace
    /// </summary>
    public class HeroicGrace : ModItem
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
            var modPlayer = player.GetModPlayer<HeroicGracePlayer>();
            modPlayer.heroicGraceEquipped = true;
            
            bool isNight = !Main.dayTime;
            
            // Eroica bonuses
            player.GetDamage(DamageClass.Melee) += 0.22f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.18f;
            player.GetCritChance(DamageClass.Melee) += 12;
            player.GetDamage(DamageClass.Generic) += 0.10f;
            
            // Moonlight bonuses
            if (isNight)
            {
                player.GetDamage(DamageClass.Generic) += 0.20f;
                player.GetCritChance(DamageClass.Generic) += 22;
                player.statDefense += 14;
            }
            else
            {
                player.GetDamage(DamageClass.Generic) += 0.10f;
            }
            
            // Swan Lake bonuses
            player.GetDamage(DamageClass.Generic) += 0.18f;
            player.GetCritChance(DamageClass.Generic) += 10;
            player.moveSpeed += 0.22f;
            player.runAcceleration *= 1.22f;
            
            // Ambient VFX - Noble trinity
            if (!hideVisual)
            {
                // Sakura-feather-moonlight fusion
                if (Main.rand.NextBool(8))
                {
                    Vector2 pos = player.Center + new Vector2(Main.rand.NextFloat(-35f, 35f), -15f);
                    
                    int choice = Main.rand.Next(3);
                    if (choice == 0)
                        ThemedParticles.SakuraPetals(pos, 1, 8f);
                    else if (choice == 1)
                        CustomParticles.SwanFeatherDrift(pos, SwanColors.White, 0.4f);
                    else
                        CustomParticles.GenericFlare(pos, MoonlightColors.Silver, 0.25f, 12);
                }
                
                // Orbiting noble particles
                if (Main.GameUpdateCount % 12 == 0)
                {
                    float baseAngle = Main.GameUpdateCount * 0.02f;
                    
                    Vector2 pos1 = player.Center + baseAngle.ToRotationVector2() * 45f;
                    CustomParticles.GenericFlare(pos1, EroicaColors.Gold, 0.28f, 12);
                    
                    Vector2 pos2 = player.Center + (baseAngle + MathHelper.TwoPi / 3f).ToRotationVector2() * 45f;
                    CustomParticles.GenericFlare(pos2, MoonlightColors.Silver, 0.28f, 12);
                    
                    Vector2 pos3 = player.Center + (baseAngle + MathHelper.TwoPi * 2f / 3f).ToRotationVector2() * 45f;
                    CustomParticles.GenericFlare(pos3, SwanColors.GetRainbow(Main.rand.NextFloat()), 0.28f, 12);
                }
                
                // Rainbow sparkles
                if (Main.rand.NextBool(12))
                {
                    Vector2 sparklePos = player.Center + Main.rand.NextVector2Circular(40f, 40f);
                    Color sparkleColor = Color.Lerp(
                        Color.Lerp(EroicaColors.Gold, MoonlightColors.Silver, 0.5f),
                        SwanColors.GetRainbow(Main.rand.NextFloat()), 0.4f);
                    
                    var sparkle = new SparkleParticle(sparklePos, Main.rand.NextVector2Circular(1f, 1f),
                        sparkleColor, 0.35f, 18);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<HerosSymhpony>()
                .AddIngredient<GracefulSonata>()
                .AddIngredient<HarmonicCoreOfEroica>(20)
                .AddIngredient<HarmonicCoreOfMoonlightSonata>(20)
                .AddIngredient<HarmonicCoreOfSwanLake>(20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class HeroicGracePlayer : ModPlayer
    {
        public bool heroicGraceEquipped;
        private int heroicSurgeTimer;
        private int invulnFramesOnKill = 90;
        private int dodgeCooldown;

        public override void ResetEffects()
        {
            heroicGraceEquipped = false;
        }

        public override void PostUpdate()
        {
            if (heroicSurgeTimer > 0)
            {
                heroicSurgeTimer--;
                Player.GetDamage(DamageClass.Generic) += 0.30f;
                
                // Triple noble aura
                if (Main.rand.NextBool(3))
                {
                    Vector2 pos = Player.Center + Main.rand.NextVector2Circular(25f, 25f);
                    Color color = Color.Lerp(
                        Color.Lerp(EroicaColors.Gold, MoonlightColors.Silver, Main.rand.NextFloat()),
                        SwanColors.GetRainbow(Main.rand.NextFloat()), 0.3f);
                    CustomParticles.GenericFlare(pos, color, 0.32f, 12);
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
            if (!heroicGraceEquipped) return;
            
            if (target.life <= 0 && !target.immortal)
            {
                // Extended invulnerability
                Player.immune = true;
                Player.immuneTime = Math.Max(Player.immuneTime, invulnFramesOnKill);
                
                // Extended Heroic Surge
                heroicSurgeTimer = 360;
                
                // Noble kill VFX
                CustomParticles.GenericFlare(target.Center, Color.White, 1.2f, 28);
                CustomParticles.GenericFlare(target.Center, EroicaColors.Gold, 1.0f, 25);
                CustomParticles.GenericFlare(target.Center, MoonlightColors.Silver, 0.8f, 22);
                
                ThemedParticles.SakuraPetals(target.Center, 10, 45f);
                
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f;
                    Vector2 featherPos = target.Center + angle.ToRotationVector2() * 30f;
                    CustomParticles.SwanFeatherDrift(featherPos, i % 2 == 0 ? SwanColors.White : MoonlightColors.Silver, 0.5f);
                }
                
                for (int i = 0; i < 8; i++)
                {
                    Color haloColor = Color.Lerp(
                        Color.Lerp(EroicaColors.Gold, MoonlightColors.Silver, i / 8f),
                        SwanColors.GetRainbow(i / 8f), 0.3f);
                    CustomParticles.HaloRing(target.Center, haloColor, 0.35f + i * 0.1f, 16 + i * 2);
                }
            }
        }

        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (!heroicGraceEquipped) return false;
            if (dodgeCooldown > 0) return false;
            
            bool isNight = !Main.dayTime;
            float dodgeChance = isNight ? 0.14f : 0.10f;
            
            if (Main.rand.NextFloat() < dodgeChance)
            {
                dodgeCooldown = 80;
                TriggerNobleGraceDodge();
                return true;
            }
            
            return false;
        }

        private void TriggerNobleGraceDodge()
        {
            // Noble grace dodge
            CustomParticles.GenericFlare(Player.Center, Color.White, 1.4f, 28);
            CustomParticles.GenericFlare(Player.Center, EroicaColors.Gold, 1.1f, 25);
            CustomParticles.GenericFlare(Player.Center, MoonlightColors.Silver, 0.9f, 22);
            
            for (int i = 0; i < 9; i++)
            {
                Color haloColor;
                if (i % 3 == 0)
                    haloColor = EroicaColors.Gold;
                else if (i % 3 == 1)
                    haloColor = MoonlightColors.Silver;
                else
                    haloColor = SwanColors.GetRainbow(i / 9f);
                
                CustomParticles.HaloRing(Player.Center, haloColor, 0.35f + i * 0.1f, 14 + i * 2);
            }
            
            ThemedParticles.SakuraPetals(Player.Center, 8, 40f);
            
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                CustomParticles.SwanFeatherDrift(Player.Center + angle.ToRotationVector2() * 25f, SwanColors.White, 0.45f);
            }
            
            Player.immune = true;
            Player.immuneTime = 30;
            
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.5f }, Player.Center);
        }
    }
    #endregion

    #region Blazing Enigma - La Campanella + Enigma + Swan Lake
    /// <summary>
    /// Phase 4 Three-Theme Combination: La Campanella + Enigma Variations + Swan Lake
    /// Ultimate chaos theme combining fire, mystery, and grace
    /// </summary>
    public class BlazingEnigma : ModItem
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
            var modPlayer = player.GetModPlayer<BlazingEnigmaPlayer>();
            modPlayer.blazingEnigmaEquipped = true;
            
            // La Campanella bonuses
            player.GetDamage(DamageClass.Magic) += 0.25f;
            player.GetCritChance(DamageClass.Magic) += 12;
            player.manaCost -= 0.15f;
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            
            // Enigma bonuses
            player.GetDamage(DamageClass.Generic) += 0.20f;
            player.GetCritChance(DamageClass.Generic) += 10;
            
            // Swan Lake bonuses
            player.GetDamage(DamageClass.Generic) += 0.18f;
            player.GetCritChance(DamageClass.Generic) += 10;
            player.moveSpeed += 0.22f;
            player.runAcceleration *= 1.22f;
            
            // Ambient VFX - Beautiful chaos
            if (!hideVisual)
            {
                // Chaotic triple-color particles
                if (Main.rand.NextBool(6))
                {
                    Vector2 offset = Main.rand.NextVector2Circular(40f, 40f);
                    Vector2 velocity = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -1f);
                    
                    Color color;
                    int choice = Main.rand.Next(3);
                    if (choice == 0)
                        color = CampanellaColors.Orange;
                    else if (choice == 1)
                        color = EnigmaColors.GreenFlame;
                    else
                        color = SwanColors.GetRainbow(Main.rand.NextFloat());
                    
                    var particle = new GenericGlowParticle(
                        player.Center + offset, velocity,
                        color * 0.7f, 0.35f, 20, true);
                    MagnumParticleHandler.SpawnParticle(particle);
                }
                
                // Burning feathers
                if (Main.rand.NextBool(12))
                {
                    Vector2 featherPos = player.Center + new Vector2(Main.rand.NextFloat(-35f, 35f), -15f);
                    Color featherColor = Color.Lerp(SwanColors.White, CampanellaColors.Orange, Main.rand.NextFloat(0.2f, 0.6f));
                    CustomParticles.SwanFeatherDrift(featherPos, featherColor, 0.4f);
                }
                
                // Mystery glyphs
                if (Main.rand.NextBool(18))
                {
                    float angle = Main.GameUpdateCount * 0.025f;
                    Vector2 glyphPos = player.Center + angle.ToRotationVector2() * 45f;
                    CustomParticles.Glyph(glyphPos, EnigmaColors.DeepPurple * 0.5f, 0.35f, -1);
                }
                
                // Rainbow sparkles
                if (Main.rand.NextBool(14))
                {
                    Vector2 sparklePos = player.Center + Main.rand.NextVector2Circular(40f, 40f);
                    
                    var sparkle = new SparkleParticle(sparklePos, new Vector2(0, -1.5f),
                        SwanColors.GetRainbow(Main.rand.NextFloat()), 0.32f, 15);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
                
                // Watching eyes
                if (Main.rand.NextBool(28))
                {
                    Vector2 eyePos = player.Center + Main.rand.NextVector2Circular(40f, 40f);
                    CustomParticles.EnigmaEyeGaze(eyePos, EnigmaColors.GreenFlame, 0.35f, null);
                }
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<BlazingSwan>()
                .AddIngredient<RiddleOfTheVoid>()
                .AddIngredient<HarmonicCoreOfLaCampanella>(20)
                .AddIngredient<HarmonicCoreOfEnigma>(20)
                .AddIngredient<HarmonicCoreOfSwanLake>(20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class BlazingEnigmaPlayer : ModPlayer
    {
        public bool blazingEnigmaEquipped;
        private int bellRingCooldown;
        private int dodgeCooldown;
        private Dictionary<int, int> paradoxStacks = new Dictionary<int, int>();
        private Dictionary<int, int> paradoxTimers = new Dictionary<int, int>();
        
        private static readonly int[] ParadoxDebuffs = new int[]
        {
            BuffID.Confused, BuffID.Slow, BuffID.CursedInferno,
            BuffID.Ichor, BuffID.ShadowFlame, BuffID.Frostburn
        };

        public override void ResetEffects()
        {
            blazingEnigmaEquipped = false;
        }

        public override void PostUpdate()
        {
            if (bellRingCooldown > 0) bellRingCooldown--;
            if (dodgeCooldown > 0) dodgeCooldown--;
            
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
            if (!blazingEnigmaEquipped) return;
            if (proj.owner != Player.whoAmI) return;
            
            // Chaotic Paradox (18% for magic)
            float paradoxChance = DamageClass.Magic.CountsAsClass(proj.DamageType) ? 0.18f : 0.12f;
            
            if (Main.rand.NextFloat() < paradoxChance)
            {
                int debuffId = ParadoxDebuffs[Main.rand.Next(ParadoxDebuffs.Length)];
                target.AddBuff(debuffId, 300);
                target.AddBuff(BuffID.OnFire, 240);
                
                if (!paradoxStacks.ContainsKey(target.whoAmI))
                    paradoxStacks[target.whoAmI] = 0;
                
                paradoxStacks[target.whoAmI]++;
                paradoxTimers[target.whoAmI] = 360;
                
                int stacks = paradoxStacks[target.whoAmI];
                
                // Chaotic VFX
                for (int i = 0; i < 9 + stacks; i++)
                {
                    float angle = MathHelper.TwoPi * i / (9 + stacks);
                    Vector2 offset = angle.ToRotationVector2() * (18f + stacks * 3f);
                    
                    Color color;
                    if (i % 3 == 0)
                        color = CampanellaColors.Orange;
                    else if (i % 3 == 1)
                        color = EnigmaColors.GreenFlame;
                    else
                        color = SwanColors.GetRainbow((float)i / (9 + stacks));
                    
                    CustomParticles.GenericFlare(target.Center + offset, color, 0.32f + stacks * 0.03f, 16);
                }
                
                CustomParticles.GlyphBurst(target.Center, EnigmaColors.Purple, 3 + stacks, 3f);
                
                // Chaos Collapse at 5 stacks
                if (stacks >= 5)
                {
                    TriggerChaosCollapse(target, damageDone);
                    paradoxStacks[target.whoAmI] = 0;
                }
            }
            
            // Bell ring with rainbow fire (12%)
            if (bellRingCooldown <= 0 && Main.rand.NextFloat() < 0.12f)
            {
                bellRingCooldown = 25;
                target.AddBuff(BuffID.Confused, 120);
                
                CustomParticles.GenericFlare(target.Center, SwanColors.GetRainbow(Main.rand.NextFloat()), 0.7f, 20);
                CustomParticles.HaloRing(target.Center, CampanellaColors.Orange, 0.5f, 16);
                
                // AOE chaos fire
                float aoeRadius = 140f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.whoAmI != target.whoAmI && !npc.immortal)
                    {
                        if (Vector2.Distance(npc.Center, target.Center) <= aoeRadius)
                        {
                            int aoeDamage = (int)(damageDone * 0.6f);
                            npc.SimpleStrikeNPC(aoeDamage, 0, false, 0, null, false, 0, true);
                            npc.AddBuff(BuffID.OnFire, 240);
                            npc.AddBuff(ParadoxDebuffs[Main.rand.Next(ParadoxDebuffs.Length)], 150);
                            
                            CustomParticles.GenericFlare(npc.Center, SwanColors.GetRainbow(Main.rand.NextFloat()), 0.45f, 14);
                        }
                    }
                }
                
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.2f }, target.Center);
            }
        }

        private void TriggerChaosCollapse(NPC target, int baseDamage)
        {
            // Beautiful chaos explosion
            CustomParticles.GenericFlare(target.Center, Color.White, 1.8f, 35);
            CustomParticles.GenericFlare(target.Center, CampanellaColors.Orange, 1.4f, 32);
            CustomParticles.GenericFlare(target.Center, EnigmaColors.GreenFlame, 1.2f, 30);
            
            // Rainbow chaos halos
            for (int ring = 0; ring < 12; ring++)
            {
                Color ringColor;
                if (ring % 3 == 0)
                    ringColor = CampanellaColors.Orange;
                else if (ring % 3 == 1)
                    ringColor = EnigmaColors.GreenFlame;
                else
                    ringColor = SwanColors.GetRainbow(ring / 12f);
                
                CustomParticles.HaloRing(target.Center, ringColor, 0.4f + ring * 0.12f, 20 + ring * 2);
            }
            
            // Burning feather explosion
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 featherPos = target.Center + angle.ToRotationVector2() * 40f;
                Color featherColor = Color.Lerp(SwanColors.White, CampanellaColors.Orange, Main.rand.NextFloat());
                CustomParticles.SwanFeatherDrift(featherPos, featherColor, 0.55f);
            }
            
            // Glyph spiral
            for (int i = 0; i < 18; i++)
            {
                float angle = MathHelper.TwoPi * i / 18f;
                float radius = 35f + i * 6f;
                Vector2 pos = target.Center + angle.ToRotationVector2() * radius;
                CustomParticles.Glyph(pos, EnigmaColors.Purple, 0.5f, -1);
            }
            
            // Eye formation
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 eyePos = target.Center + angle.ToRotationVector2() * 50f;
                CustomParticles.EnigmaEyeGaze(eyePos, EnigmaColors.GreenFlame, 0.55f, target.Center);
            }
            
            CustomParticles.ExplosionBurst(target.Center, CampanellaColors.Orange, 18, 12f);
            CustomParticles.ExplosionBurst(target.Center, EnigmaColors.GreenFlame, 15, 10f);
            
            // Rainbow sparkle spiral
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Vector2 sparklePos = target.Center + angle.ToRotationVector2() * 55f;
                
                var sparkle = new SparkleParticle(sparklePos, angle.ToRotationVector2() * 4f,
                    SwanColors.GetRainbow((float)i / 20f), 0.5f, 22);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Massive damage
            if (Main.myPlayer == Player.whoAmI)
            {
                int chaosDamage = (int)(baseDamage * 3.0f);
                target.SimpleStrikeNPC(chaosDamage, 0, false, 0, null, false, 0, true);
                
                float aoeRadius = 260f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.whoAmI != target.whoAmI && !npc.immortal)
                    {
                        if (Vector2.Distance(npc.Center, target.Center) <= aoeRadius)
                        {
                            npc.SimpleStrikeNPC(chaosDamage / 2, 0, false, 0, null, false, 0, true);
                            npc.AddBuff(BuffID.OnFire, 360);
                            npc.AddBuff(ParadoxDebuffs[Main.rand.Next(ParadoxDebuffs.Length)], 240);
                            
                            CustomParticles.GenericFlare(npc.Center, SwanColors.GetRainbow(Main.rand.NextFloat()), 0.6f, 18);
                        }
                    }
                }
            }
            
            MagnumScreenEffects.AddScreenShake(15f);
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.3f, Volume = 1.4f }, target.Center);
        }

        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (!blazingEnigmaEquipped) return false;
            if (dodgeCooldown > 0) return false;
            
            if (Main.rand.NextFloat() < 0.12f)
            {
                dodgeCooldown = 80;
                
                // Chaotic dodge
                CustomParticles.GenericFlare(Player.Center, Color.White, 1.3f, 25);
                
                for (int i = 0; i < 9; i++)
                {
                    Color haloColor;
                    if (i % 3 == 0)
                        haloColor = CampanellaColors.Orange;
                    else if (i % 3 == 1)
                        haloColor = EnigmaColors.GreenFlame;
                    else
                        haloColor = SwanColors.GetRainbow(i / 9f);
                    
                    CustomParticles.HaloRing(Player.Center, haloColor, 0.35f + i * 0.1f, 14 + i * 2);
                }
                
                for (int i = 0; i < 10; i++)
                {
                    float angle = MathHelper.TwoPi * i / 10f;
                    Color featherColor = Color.Lerp(SwanColors.White, CampanellaColors.Orange, Main.rand.NextFloat());
                    CustomParticles.SwanFeatherDrift(Player.Center + angle.ToRotationVector2() * 25f, featherColor, 0.45f);
                }
                
                Player.immune = true;
                Player.immuneTime = 25;
                
                return true;
            }
            
            return false;
        }
    }
    #endregion

    #region Complete Harmony - All 5 Themes
    /// <summary>
    /// Phase 4 Ultimate Combination: All 5 Themes Combined
    /// Moonlight Sonata + Eroica + La Campanella + Enigma Variations + Swan Lake
    /// Ultimate musical achievement - all themes at full strength
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
            
            // === ALL FIVE THEMES COMBINED ===
            
            // Moonlight Sonata
            if (isNight)
            {
                player.GetDamage(DamageClass.Generic) += 0.20f;
                player.GetCritChance(DamageClass.Generic) += 22;
                player.statDefense += 15;
            }
            else
            {
                player.GetDamage(DamageClass.Generic) += 0.12f;
            }
            
            // Eroica
            player.GetDamage(DamageClass.Melee) += 0.22f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.18f;
            player.GetCritChance(DamageClass.Melee) += 12;
            player.GetDamage(DamageClass.Generic) += 0.10f;
            
            // La Campanella
            player.GetDamage(DamageClass.Magic) += 0.25f;
            player.GetCritChance(DamageClass.Magic) += 12;
            player.manaCost -= 0.15f;
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            
            // Enigma Variations
            player.GetDamage(DamageClass.Generic) += 0.20f;
            player.GetCritChance(DamageClass.Generic) += 10;
            
            // Swan Lake
            player.GetDamage(DamageClass.Generic) += 0.18f;
            player.GetCritChance(DamageClass.Generic) += 10;
            player.moveSpeed += 0.25f;
            player.runAcceleration *= 1.25f;
            
            // Ambient VFX - All five themes harmonized
            if (!hideVisual)
            {
                // Orbiting five-theme particles
                if (Main.GameUpdateCount % 8 == 0)
                {
                    float baseAngle = Main.GameUpdateCount * 0.015f;
                    
                    Color[] themeColors = new Color[]
                    {
                        MoonlightColors.Purple,
                        EroicaColors.Gold,
                        CampanellaColors.Orange,
                        EnigmaColors.GreenFlame,
                        SwanColors.GetRainbow(Main.rand.NextFloat())
                    };
                    
                    for (int i = 0; i < 5; i++)
                    {
                        float angle = baseAngle + MathHelper.TwoPi * i / 5f;
                        Vector2 pos = player.Center + angle.ToRotationVector2() * 55f;
                        CustomParticles.GenericFlare(pos, themeColors[i], 0.35f, 14);
                    }
                }
                
                // Theme-specific particles
                if (Main.rand.NextBool(8))
                {
                    int themeChoice = Main.rand.Next(5);
                    Vector2 particlePos = player.Center + Main.rand.NextVector2Circular(45f, 45f);
                    
                    switch (themeChoice)
                    {
                        case 0: // Moonlight
                            CustomParticles.GenericFlare(particlePos, MoonlightColors.Silver, 0.28f, 14);
                            break;
                        case 1: // Eroica
                            if (Main.rand.NextBool())
                                ThemedParticles.SakuraPetals(particlePos, 1, 8f);
                            else
                                CustomParticles.GenericFlare(particlePos, EroicaColors.Gold, 0.28f, 14);
                            break;
                        case 2: // La Campanella
                            Vector2 flameVel = new Vector2(0, -Main.rand.NextFloat(1f, 2f));
                            var flame = new GenericGlowParticle(particlePos, flameVel, 
                                CampanellaColors.Orange * 0.7f, 0.32f, 18, true);
                            MagnumParticleHandler.SpawnParticle(flame);
                            break;
                        case 3: // Enigma
                            CustomParticles.Glyph(particlePos, EnigmaColors.DeepPurple * 0.5f, 0.3f, -1);
                            break;
                        case 4: // Swan Lake
                            CustomParticles.SwanFeatherDrift(particlePos, Main.rand.NextBool() ? SwanColors.White : SwanColors.Black, 0.35f);
                            break;
                    }
                }
                
                // Rainbow sparkles
                if (Main.rand.NextBool(10))
                {
                    Vector2 sparklePos = player.Center + Main.rand.NextVector2Circular(50f, 50f);
                    
                    var sparkle = new SparkleParticle(sparklePos, Main.rand.NextVector2Circular(1f, 1f),
                        SwanColors.GetRainbow(Main.rand.NextFloat()), 0.35f, 18);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
                
                // Watching eyes occasionally
                if (Main.rand.NextBool(35))
                {
                    Vector2 eyePos = player.Center + Main.rand.NextVector2Circular(45f, 45f);
                    CustomParticles.EnigmaEyeGaze(eyePos, EnigmaColors.GreenFlame, 0.35f, null);
                }
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SonatasEmbrace>()
                .AddIngredient<HerosSymhpony>()
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
        private int heroicSurgeTimer;
        private int invulnFramesOnKill = 90;
        private int dodgeCooldown;
        private int bellRingCooldown;
        private Dictionary<int, int> paradoxStacks = new Dictionary<int, int>();
        private Dictionary<int, int> paradoxTimers = new Dictionary<int, int>();
        
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
            if (heroicSurgeTimer > 0)
            {
                heroicSurgeTimer--;
                Player.GetDamage(DamageClass.Generic) += 0.30f;
                
                // Five-theme aura
                if (Main.rand.NextBool(3))
                {
                    Vector2 pos = Player.Center + Main.rand.NextVector2Circular(28f, 28f);
                    Color[] colors = new Color[]
                    {
                        MoonlightColors.Purple,
                        EroicaColors.Gold,
                        CampanellaColors.Orange,
                        EnigmaColors.GreenFlame,
                        SwanColors.GetRainbow(Main.rand.NextFloat())
                    };
                    CustomParticles.GenericFlare(pos, colors[Main.rand.Next(5)], 0.32f, 12);
                }
            }
            
            if (dodgeCooldown > 0) dodgeCooldown--;
            if (bellRingCooldown > 0) bellRingCooldown--;
            
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

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            HandleHarmonyHit(target, damageDone, true);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.owner == Player.whoAmI)
            {
                HandleHarmonyHit(target, damageDone, DamageClass.Magic.CountsAsClass(proj.DamageType));
            }
        }

        private void HandleHarmonyHit(NPC target, int damageDone, bool isMagic)
        {
            if (!completeHarmonyEquipped) return;
            
            bool isNight = !Main.dayTime;
            
            // Blue fire at night (Moonlight + Campanella)
            if (isNight && isMagic)
            {
                int bonusDamage = (int)(damageDone * 0.20f);
                target.SimpleStrikeNPC(bonusDamage, 0, false, 0, null, false, 0, true);
                
                Color blueFlame = new Color(100, 150, 255);
                CustomParticles.GenericFlare(target.Center, blueFlame, 0.5f, 15);
            }
            
            // Paradox (18%)
            if (Main.rand.NextFloat() < 0.18f)
            {
                int debuffId = ParadoxDebuffs[Main.rand.Next(ParadoxDebuffs.Length)];
                target.AddBuff(debuffId, 360);
                target.AddBuff(BuffID.OnFire, 300);
                
                if (!paradoxStacks.ContainsKey(target.whoAmI))
                    paradoxStacks[target.whoAmI] = 0;
                
                paradoxStacks[target.whoAmI]++;
                paradoxTimers[target.whoAmI] = 420;
                
                int stacks = paradoxStacks[target.whoAmI];
                
                // Five-theme VFX
                Color[] colors = new Color[]
                {
                    MoonlightColors.Purple,
                    EroicaColors.Gold,
                    CampanellaColors.Orange,
                    EnigmaColors.GreenFlame,
                    SwanColors.GetRainbow(Main.rand.NextFloat())
                };
                
                for (int i = 0; i < 10 + stacks; i++)
                {
                    float angle = MathHelper.TwoPi * i / (10 + stacks);
                    Vector2 offset = angle.ToRotationVector2() * (20f + stacks * 3f);
                    CustomParticles.GenericFlare(target.Center + offset, colors[i % 5], 0.35f, 18);
                }
                
                CustomParticles.GlyphBurst(target.Center, EnigmaColors.Purple, 4 + stacks, 3.5f);
                
                // Ultimate Harmony Collapse at 5 stacks
                if (stacks >= 5)
                {
                    TriggerHarmonyCollapse(target, damageDone, isNight);
                    paradoxStacks[target.whoAmI] = 0;
                }
            }
            
            // Bell ring (15%)
            if (bellRingCooldown <= 0 && Main.rand.NextFloat() < 0.15f)
            {
                bellRingCooldown = 20;
                target.AddBuff(BuffID.Confused, 150);
                
                Color chimeColor = Color.Lerp(CampanellaColors.Orange, SwanColors.GetRainbow(Main.rand.NextFloat()), 0.5f);
                CustomParticles.GenericFlare(target.Center, chimeColor, 0.8f, 22);
                CustomParticles.HaloRing(target.Center, EroicaColors.Gold, 0.5f, 18);
                
                // AOE
                float aoeRadius = 160f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.whoAmI != target.whoAmI && !npc.immortal)
                    {
                        if (Vector2.Distance(npc.Center, target.Center) <= aoeRadius)
                        {
                            int aoeDamage = (int)(damageDone * 0.6f);
                            npc.SimpleStrikeNPC(aoeDamage, 0, false, 0, null, false, 0, true);
                            npc.AddBuff(BuffID.OnFire, 240);
                            npc.AddBuff(ParadoxDebuffs[Main.rand.Next(ParadoxDebuffs.Length)], 180);
                            
                            CustomParticles.GenericFlare(npc.Center, SwanColors.GetRainbow(Main.rand.NextFloat()), 0.5f, 15);
                        }
                    }
                }
                
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item35, target.Center);
            }
            
            // Check for kill
            if (target.life <= 0 && !target.immortal)
            {
                TriggerHeroicKill(target);
            }
        }

        private void TriggerHeroicKill(NPC killedTarget)
        {
            Player.immune = true;
            Player.immuneTime = Math.Max(Player.immuneTime, invulnFramesOnKill);
            heroicSurgeTimer = 360;
            
            // Five-theme kill VFX
            CustomParticles.GenericFlare(killedTarget.Center, Color.White, 1.5f, 32);
            CustomParticles.GenericFlare(killedTarget.Center, MoonlightColors.Purple, 1.2f, 28);
            CustomParticles.GenericFlare(killedTarget.Center, EroicaColors.Gold, 1.0f, 25);
            CustomParticles.GenericFlare(killedTarget.Center, CampanellaColors.Orange, 0.9f, 22);
            CustomParticles.GenericFlare(killedTarget.Center, EnigmaColors.GreenFlame, 0.8f, 20);
            
            ThemedParticles.SakuraPetals(killedTarget.Center, 12, 50f);
            
            for (int i = 0; i < 15; i++)
            {
                float angle = MathHelper.TwoPi * i / 15f;
                Vector2 featherPos = killedTarget.Center + angle.ToRotationVector2() * 35f;
                CustomParticles.SwanFeatherDrift(featherPos, i % 2 == 0 ? SwanColors.White : MoonlightColors.Silver, 0.5f);
            }
            
            CustomParticles.GlyphBurst(killedTarget.Center, EnigmaColors.Purple, 8, 6f);
            
            for (int i = 0; i < 10; i++)
            {
                Color[] colors = new Color[]
                {
                    MoonlightColors.Purple, EroicaColors.Gold,
                    CampanellaColors.Orange, EnigmaColors.GreenFlame, SwanColors.GetRainbow(i / 10f)
                };
                CustomParticles.HaloRing(killedTarget.Center, colors[i % 5], 0.35f + i * 0.1f, 16 + i * 2);
            }
        }

        private void TriggerHarmonyCollapse(NPC target, int baseDamage, bool isNight)
        {
            // ULTIMATE HARMONY EXPLOSION
            CustomParticles.GenericFlare(target.Center, Color.White, 2.2f, 40);
            
            Color[] themeColors = new Color[]
            {
                isNight ? new Color(100, 150, 255) : MoonlightColors.Purple,
                EroicaColors.Gold,
                CampanellaColors.Orange,
                EnigmaColors.GreenFlame,
                SwanColors.GetRainbow(0f)
            };
            
            for (int i = 0; i < 5; i++)
            {
                CustomParticles.GenericFlare(target.Center, themeColors[i], 1.6f - i * 0.2f, 35 - i * 2);
            }
            
            // 15 halos cycling through all themes
            for (int ring = 0; ring < 15; ring++)
            {
                CustomParticles.HaloRing(target.Center, themeColors[ring % 5], 0.4f + ring * 0.12f, 22 + ring * 2);
            }
            
            ThemedParticles.SakuraPetals(target.Center, 15, 60f);
            
            // Feather explosion
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Vector2 featherPos = target.Center + angle.ToRotationVector2() * 50f;
                Color featherColor = i % 2 == 0 ? SwanColors.White : MoonlightColors.Silver;
                CustomParticles.SwanFeatherDrift(featherPos, featherColor, 0.6f);
            }
            
            // Glyph spiral
            for (int i = 0; i < 25; i++)
            {
                float angle = MathHelper.TwoPi * i / 25f;
                float radius = 40f + i * 6f;
                Vector2 pos = target.Center + angle.ToRotationVector2() * radius;
                CustomParticles.Glyph(pos, EnigmaColors.Purple, 0.6f, -1);
            }
            
            // Eye formation
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 eyePos = target.Center + angle.ToRotationVector2() * 65f;
                CustomParticles.EnigmaEyeGaze(eyePos, EnigmaColors.GreenFlame, 0.65f, target.Center);
            }
            
            for (int i = 0; i < 5; i++)
            {
                CustomParticles.ExplosionBurst(target.Center, themeColors[i], 15, 12f - i);
            }
            
            // Rainbow sparkle explosion
            for (int i = 0; i < 25; i++)
            {
                float angle = MathHelper.TwoPi * i / 25f;
                Vector2 sparklePos = target.Center + angle.ToRotationVector2() * 70f;
                
                var sparkle = new SparkleParticle(sparklePos, angle.ToRotationVector2() * 5f,
                    SwanColors.GetRainbow((float)i / 25f), 0.55f, 25);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // MASSIVE damage
            if (Main.myPlayer == Player.whoAmI)
            {
                int harmonyDamage = (int)(baseDamage * 4.0f);
                target.SimpleStrikeNPC(harmonyDamage, 0, false, 0, null, false, 0, true);
                
                float aoeRadius = 300f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.whoAmI != target.whoAmI && !npc.immortal)
                    {
                        if (Vector2.Distance(npc.Center, target.Center) <= aoeRadius)
                        {
                            npc.SimpleStrikeNPC(harmonyDamage / 2, 0, false, 0, null, false, 0, true);
                            npc.AddBuff(BuffID.OnFire, 420);
                            npc.AddBuff(ParadoxDebuffs[Main.rand.Next(ParadoxDebuffs.Length)], 300);
                            
                            CustomParticles.GenericFlare(npc.Center, themeColors[Main.rand.Next(5)], 0.7f, 20);
                        }
                    }
                }
            }
            
            MagnumScreenEffects.AddScreenShake(18f);
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.4f, Volume = 1.5f }, target.Center);
        }

        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (!completeHarmonyEquipped) return false;
            if (dodgeCooldown > 0) return false;
            
            bool isNight = !Main.dayTime;
            float dodgeChance = isNight ? 0.15f : 0.12f;
            
            if (Main.rand.NextFloat() < dodgeChance)
            {
                dodgeCooldown = 70;
                TriggerHarmonyDodge();
                return true;
            }
            
            return false;
        }

        private void TriggerHarmonyDodge()
        {
            // Ultimate harmony dodge
            CustomParticles.GenericFlare(Player.Center, Color.White, 1.6f, 30);
            
            Color[] themeColors = new Color[]
            {
                MoonlightColors.Purple,
                EroicaColors.Gold,
                CampanellaColors.Orange,
                EnigmaColors.GreenFlame,
                SwanColors.GetRainbow(0f)
            };
            
            for (int i = 0; i < 5; i++)
            {
                CustomParticles.GenericFlare(Player.Center, themeColors[i], 1.0f - i * 0.15f, 25 - i * 2);
            }
            
            for (int i = 0; i < 10; i++)
            {
                CustomParticles.HaloRing(Player.Center, themeColors[i % 5], 0.35f + i * 0.08f, 14 + i * 2);
            }
            
            ThemedParticles.SakuraPetals(Player.Center, 10, 45f);
            
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                CustomParticles.SwanFeatherDrift(Player.Center + angle.ToRotationVector2() * 30f, 
                    i % 2 == 0 ? SwanColors.White : MoonlightColors.Silver, 0.5f);
            }
            
            CustomParticles.GlyphBurst(Player.Center, EnigmaColors.Purple, 5, 4f);
            
            // Deal dodge damage to nearby enemies
            if (Main.myPlayer == Player.whoAmI)
            {
                int dodgeDamage = 150 + (int)(Player.GetTotalDamage(DamageClass.Generic).ApplyTo(100) * 0.3f);
                float damageRadius = 200f;
                
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && !npc.immortal && !npc.dontTakeDamage)
                    {
                        if (Vector2.Distance(npc.Center, Player.Center) <= damageRadius)
                        {
                            npc.SimpleStrikeNPC(dodgeDamage, 0, false, 0, null, false, 0, true);
                            CustomParticles.GenericFlare(npc.Center, themeColors[Main.rand.Next(5)], 0.5f, 15);
                        }
                    }
                }
            }
            
            Player.immune = true;
            Player.immuneTime = 35;
            
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 1.3f }, Player.Center);
        }
    }
    #endregion
}


