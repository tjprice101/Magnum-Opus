using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.DataStructures;
using ReLogic.Content;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.EnigmaVariations.Accessories
{
    /// <summary>
    /// ModPlayer class that handles all Enigma Variations accessory effects.
    /// Theme: Mystery, the unknowable, arcane secrets, questioning reality
    /// Colors: Black → Deep Purple → Eerie Green Flame
    /// </summary>
    public class EnigmaAccessoryPlayer : ModPlayer
    {
        // Enigma color palette
        public static readonly Color EnigmaBlack = new Color(15, 10, 20);
        public static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        public static readonly Color EnigmaPurple = new Color(140, 60, 200);
        public static readonly Color EnigmaGreenFlame = new Color(50, 220, 100);
        public static readonly Color EnigmaDarkGreen = new Color(30, 100, 50);
        
        // ========== IGNITION OF MYSTERY (Melee) ==========
        // Effect: "Mysteries Unveiled" - Every melee hit builds mystery stacks.
        // At 10 stacks, your next melee attack unleashes a massive eye burst that marks enemies
        // with "Watched" debuff, making them take 15% more damage from all sources.
        // Additionally, melee speed increases by 2% per stack (up to 20%).
        public bool hasIgnitionOfMystery = false;
        public int mysteryStacks = 0;
        private const int MaxMysteryStacks = 10;
        public int watchedProcCooldown = 0;
        private const int WatchedProcCooldownMax = 30; // 0.5 second minimum between procs
        
        // ========== PENDANT OF A THOUSAND PUZZLES (Mage) ==========
        // Effect: "Arcane Enigma" - Magic attacks have a 15% chance to spawn a "Puzzle Fragment".
        // Collecting 5 fragments grants "Puzzle Mastery" buff for 8 seconds:
        // +30% magic damage, -20% mana cost, and magic projectiles leave glowing glyph trails.
        // Fragments orbit the player and can be collected by touching them.
        public bool hasPendantOfAThousandPuzzles = false;
        public int puzzleFragments = 0;
        private const int MaxPuzzleFragments = 5;
        public int puzzleMasteryTimer = 0;
        private const int PuzzleMasteryDuration = 480; // 8 seconds
        public bool puzzleMasteryActive = false;
        private List<PuzzleFragment> activeFragments = new List<PuzzleFragment>();
        
        // ========== ALCHEMICAL PARADOX (Ranger) ==========
        // Effect: "Paradox Shots" - Every 4th ranged projectile fired becomes a "Paradox Bolt"
        // that splits into 2-3 smaller projectiles on hit and applies "Paradox" debuff.
        // Enemies with Paradox take damage over time and explode on death, damaging nearby enemies.
        // Additionally, +8% ranged critical strike chance.
        public bool hasAlchemicalParadox = false;
        public int rangedShotCounter = 0;
        private const int ParadoxTriggerEvery = 4;
        public int paradoxProcCooldown = 0;
        
        // ========== RIDDLEMASTER'S CAULDRON (Summoner) ==========
        // Effect: "Cauldron's Brew" - Minions periodically release "Mystery Vapors" that 
        // confuse enemies and reduce their damage by 15%. Every 5 seconds, a random minion
        // gains "Riddle's Blessing" - doubled attack speed for 3 seconds.
        // Additionally, +1 max minion slot.
        public bool hasRiddlemastersCauldron = false;
        public int vaporTimer = 0;
        private const int VaporInterval = 180; // 3 seconds
        public int blessingTimer = 0;
        private const int BlessingInterval = 300; // 5 seconds
        public int blessingDuration = 0;
        private const int BlessingDurationMax = 180; // 3 seconds
        public int blessedMinionIndex = -1;
        
        // ========== FLOATING VISUAL ==========
        public float floatAngle = 0f;
        
        public override void ResetEffects()
        {
            hasIgnitionOfMystery = false;
            hasPendantOfAThousandPuzzles = false;
            hasAlchemicalParadox = false;
            hasRiddlemastersCauldron = false;
        }
        
        public override void PostUpdate()
        {
            floatAngle += 0.025f;
            
            // ========== IGNITION OF MYSTERY ==========
            if (watchedProcCooldown > 0)
                watchedProcCooldown--;
            
            if (!hasIgnitionOfMystery)
                mysteryStacks = 0;
            
            // ========== PENDANT OF A THOUSAND PUZZLES ==========
            if (hasPendantOfAThousandPuzzles)
            {
                // Update and collect fragments
                UpdatePuzzleFragments();
                
                // Puzzle Mastery duration
                if (puzzleMasteryActive)
                {
                    puzzleMasteryTimer--;
                    
                    // ====== VISUAL INDICATOR - Orbiting glyphs + aura showing buff is active ======
                    // Pulsing aura to clearly indicate the buff is active
                    float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.2f + 0.8f;
                    Lighting.AddLight(Player.Center, EnigmaGreenFlame.ToVector3() * 0.5f * pulse);
                    
                    // Orbiting glyphs around player (clear visual indicator)
                    if (Main.GameUpdateCount % 4 == 0)
                    {
                        float orbitAngle = Main.GameUpdateCount * 0.05f;
                        for (int i = 0; i < 3; i++)
                        {
                            float angle = orbitAngle + MathHelper.TwoPi * i / 3f;
                            Vector2 glyphPos = Player.Center + angle.ToRotationVector2() * 45f;
                            Color glyphColor = Color.Lerp(EnigmaPurple, EnigmaGreenFlame, (float)i / 3f) * pulse;
                            CustomParticles.Glyph(glyphPos, glyphColor, 0.35f, -1);
                        }
                    }
                    
                    // Occasional sparkle to reinforce the active state
                    if (Main.rand.NextBool(8))
                    {
                        Vector2 sparklePos = Player.Center + Main.rand.NextVector2Circular(30f, 30f);
                        CustomParticles.GenericFlare(sparklePos, EnigmaGreenFlame * 0.7f, 0.25f, 12);
                    }
                    
                    // Show remaining time via subtle intensity change
                    float timeRemainingPercent = (float)puzzleMasteryTimer / PuzzleMasteryDuration;
                    if (timeRemainingPercent < 0.25f && Main.GameUpdateCount % 10 == 0)
                    {
                        // Warning flash when buff is about to expire
                        CustomParticles.GenericFlare(Player.Center, EnigmaPurple, 0.4f, 8);
                    }
                    
                    if (puzzleMasteryTimer <= 0)
                    {
                        puzzleMasteryActive = false;
                        // End of mastery burst
                        CustomParticles.GlyphBurst(Player.Center, EnigmaPurple, 8, 5f);
                        ThemedParticles.EnigmaMusicNotes(Player.Center, 4, 30f);
                    }
                }
            }
            else
            {
                puzzleFragments = 0;
                puzzleMasteryActive = false;
                puzzleMasteryTimer = 0;
                activeFragments.Clear();
            }
            
            // ========== ALCHEMICAL PARADOX ==========
            if (paradoxProcCooldown > 0)
                paradoxProcCooldown--;
            
            if (!hasAlchemicalParadox)
                rangedShotCounter = 0;
            
            // ========== RIDDLEMASTER'S CAULDRON ==========
            if (hasRiddlemastersCauldron)
            {
                // Vapor timer
                vaporTimer++;
                if (vaporTimer >= VaporInterval)
                {
                    vaporTimer = 0;
                    ReleaseMysteryVapors();
                }
                
                // Blessing timer
                blessingTimer++;
                if (blessingTimer >= BlessingInterval)
                {
                    blessingTimer = 0;
                    GrantRiddlesBlessing();
                }
                
                // Blessing duration countdown
                if (blessingDuration > 0)
                {
                    blessingDuration--;
                    if (blessingDuration <= 0)
                        blessedMinionIndex = -1;
                }
            }
            else
            {
                vaporTimer = 0;
                blessingTimer = 0;
                blessingDuration = 0;
                blessedMinionIndex = -1;
            }
        }
        
        public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)
        {
            // Ignition of Mystery - melee stack building
            if (hasIgnitionOfMystery && item.DamageType == DamageClass.Melee)
            {
                mysteryStacks++;
                
                // Visual feedback per stack
                CustomParticles.GenericFlare(target.Center, GetEnigmaGradient((float)mysteryStacks / MaxMysteryStacks), 0.4f, 15);
                
                if (mysteryStacks >= MaxMysteryStacks)
                {
                    // TRIGGER THE MYSTERY BURST!
                    TriggerMysteryBurst(target);
                    mysteryStacks = 0;
                }
            }
        }
        
        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            // Ignition of Mystery - melee projectile stack building
            if (hasIgnitionOfMystery && proj.DamageType == DamageClass.Melee)
            {
                mysteryStacks++;
                CustomParticles.GenericFlare(target.Center, GetEnigmaGradient((float)mysteryStacks / MaxMysteryStacks), 0.4f, 15);
                
                if (mysteryStacks >= MaxMysteryStacks)
                {
                    TriggerMysteryBurst(target);
                    mysteryStacks = 0;
                }
            }
            
            // Pendant - magic projectile fragment spawn
            if (hasPendantOfAThousandPuzzles && proj.DamageType == DamageClass.Magic)
            {
                if (Main.rand.NextFloat() < 0.15f && activeFragments.Count < 8)
                {
                    SpawnPuzzleFragment(target.Center);
                }
            }
        }
        
        public override bool Shoot(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Alchemical Paradox - count ranged shots and mark every 4th as paradox
            if (hasAlchemicalParadox && item.DamageType == DamageClass.Ranged)
            {
                rangedShotCounter++;
                if (rangedShotCounter >= ParadoxTriggerEvery)
                {
                    rangedShotCounter = 0;
                    // Mark this projectile as a Paradox Bolt (handled in GlobalProjectile)
                    paradoxProcCooldown = 5;
                    
                    // Visual spawn effect
                    CustomParticles.GlyphBurst(position, EnigmaGreenFlame, 4, 3f);
                    CustomParticles.GenericFlare(position, EnigmaPurple, 0.6f, 15);
                    ThemedParticles.EnigmaMusicNotes(position, 2, 15f);
                }
            }
            
            return true;
        }
        
        #region Ignition of Mystery Methods
        
        private void TriggerMysteryBurst(NPC initialTarget)
        {
            if (watchedProcCooldown > 0) return;
            watchedProcCooldown = WatchedProcCooldownMax;
            
            // Sound effect
            SoundEngine.PlaySound(SoundID.Item119 with { Pitch = 0.3f, Volume = 1.2f }, Player.Center);
            
            Vector2 burstCenter = initialTarget.Center;
            
            // MASSIVE EYE BURST - the mystery is revealed!
            // Central flare
            CustomParticles.GenericFlare(burstCenter, EnigmaGreenFlame, 1.2f, 30);
            CustomParticles.GenericFlare(burstCenter, EnigmaPurple, 0.9f, 25);
            CustomParticles.GenericFlare(burstCenter, Color.White, 0.6f, 20);
            
            // Expanding halo rings
            for (int ring = 0; ring < 5; ring++)
            {
                float progress = (float)ring / 5f;
                Color ringColor = GetEnigmaGradient(progress);
                CustomParticles.HaloRing(burstCenter, ringColor, 0.4f + ring * 0.25f, 18 + ring * 4);
            }
            
            // Eyes exploding outward - WATCHING EVERYTHING
            CustomParticles.EnigmaEyeExplosion(burstCenter, EnigmaGreenFlame, 10, 6f);
            
            // Glyph cascade
            CustomParticles.GlyphBurst(burstCenter, EnigmaPurple, 12, 5f);
            CustomParticles.GlyphCircle(burstCenter, EnigmaDeepPurple, 8, 60f, 0.03f);
            
            // Music notes spiral
            ThemedParticles.EnigmaMusicNoteBurst(burstCenter, 12, 5f);
            
            // Fractal burst pattern
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 offset = angle.ToRotationVector2() * 45f;
                CustomParticles.GenericFlare(burstCenter + offset, GetEnigmaGradient((float)i / 8f), 0.5f, 20);
            }
            
            // Apply "Watched" debuff to all enemies in range
            float watchRadius = 300f;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.Distance(burstCenter) < watchRadius)
                {
                    // Apply watched debuff (increases damage taken)
                    npc.AddBuff(ModContent.BuffType<Debuffs.WatchedDebuff>(), 300); // 5 seconds
                    
                    // Eye spawns watching this enemy
                    CustomParticles.EnigmaEyeImpact(npc.Center, burstCenter, EnigmaGreenFlame, 0.5f);
                }
            }
            
            // Screen effect
            Lighting.AddLight(burstCenter, EnigmaGreenFlame.ToVector3() * 2f);
        }
        
        #endregion
        
        #region Pendant of a Thousand Puzzles Methods
        
        private void SpawnPuzzleFragment(Vector2 position)
        {
            // Create a new puzzle fragment
            var fragment = new PuzzleFragment
            {
                Position = position,
                Angle = Main.rand.NextFloat() * MathHelper.TwoPi,
                OrbitRadius = 60f + Main.rand.NextFloat(20f),
                Lifetime = 600, // 10 seconds to collect
                Color = GetEnigmaGradient(Main.rand.NextFloat())
            };
            activeFragments.Add(fragment);
            
            // Spawn visual
            CustomParticles.GenericFlare(position, EnigmaPurple, 0.5f, 15);
            CustomParticles.Glyph(position, EnigmaGreenFlame, 0.4f, -1);
            SoundEngine.PlaySound(SoundID.Item4 with { Pitch = 0.5f, Volume = 0.5f }, position);
        }
        
        private void UpdatePuzzleFragments()
        {
            for (int i = activeFragments.Count - 1; i >= 0; i--)
            {
                var fragment = activeFragments[i];
                
                // Move toward player orbit
                fragment.Angle += 0.04f;
                Vector2 targetPos = Player.Center + fragment.Angle.ToRotationVector2() * fragment.OrbitRadius;
                fragment.Position = Vector2.Lerp(fragment.Position, targetPos, 0.08f);
                fragment.Lifetime--;
                
                // Visual effect
                if (Main.rand.NextBool(3))
                {
                    CustomParticles.GenericGlow(fragment.Position, fragment.Color * 0.7f, 0.2f, 12);
                }
                
                // Occasional glyph
                if (Main.rand.NextBool(20))
                {
                    CustomParticles.Glyph(fragment.Position, EnigmaPurple * 0.6f, 0.25f, -1);
                }
                
                // Check if collected (close to player)
                if (fragment.Position.Distance(Player.Center) < 30f)
                {
                    CollectPuzzleFragment(fragment);
                    activeFragments.RemoveAt(i);
                    continue;
                }
                
                // Remove if expired
                if (fragment.Lifetime <= 0)
                {
                    // Fade out effect
                    CustomParticles.GenericFlare(fragment.Position, EnigmaPurple * 0.5f, 0.3f, 10);
                    activeFragments.RemoveAt(i);
                }
            }
        }
        
        private void CollectPuzzleFragment(PuzzleFragment fragment)
        {
            puzzleFragments++;
            
            // Collection effect
            CustomParticles.GenericFlare(fragment.Position, EnigmaGreenFlame, 0.6f, 18);
            CustomParticles.GlyphBurst(fragment.Position, EnigmaPurple, 4, 2f);
            ThemedParticles.EnigmaMusicNotes(fragment.Position, 2, 15f);
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.2f + puzzleFragments * 0.1f }, fragment.Position);
            
            if (puzzleFragments >= MaxPuzzleFragments)
            {
                // PUZZLE MASTERY ACTIVATED!
                ActivatePuzzleMastery();
                puzzleFragments = 0;
            }
        }
        
        private void ActivatePuzzleMastery()
        {
            puzzleMasteryActive = true;
            puzzleMasteryTimer = PuzzleMasteryDuration;
            
            // Epic activation effect
            SoundEngine.PlaySound(SoundID.Item119 with { Pitch = 0.5f, Volume = 1.3f }, Player.Center);
            
            // Massive glyph circle
            CustomParticles.GlyphCircle(Player.Center, EnigmaGreenFlame, 12, 80f, 0.025f);
            CustomParticles.GlyphCircle(Player.Center, EnigmaPurple, 8, 50f, -0.03f);
            
            // Central burst
            CustomParticles.GenericFlare(Player.Center, Color.White, 1.5f, 30);
            CustomParticles.GenericFlare(Player.Center, EnigmaGreenFlame, 1.2f, 25);
            CustomParticles.GenericFlare(Player.Center, EnigmaPurple, 0.9f, 22);
            
            // Eyes form a watching formation
            CustomParticles.EnigmaEyeFormation(Player.Center, EnigmaGreenFlame, 6, 70f);
            
            // Expanding halos
            for (int i = 0; i < 6; i++)
            {
                float progress = (float)i / 6f;
                CustomParticles.HaloRing(Player.Center, GetEnigmaGradient(progress), 0.5f + i * 0.2f, 20 + i * 5);
            }
            
            // Music explosion
            ThemedParticles.EnigmaMusicNoteBurst(Player.Center, 16, 7f);
            
            Lighting.AddLight(Player.Center, EnigmaGreenFlame.ToVector3() * 2.5f);
        }
        
        #endregion
        
        #region Riddlemaster's Cauldron Methods
        
        private void ReleaseMysteryVapors()
        {
            // Find all player's minions and release vapor from them
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.owner == Player.whoAmI && proj.minion)
                {
                    // Visual vapor effect
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 8f;
                        Vector2 vaporVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                        CustomParticles.GenericGlow(proj.Center, GetEnigmaGradient(Main.rand.NextFloat()) * 0.6f, 
                            0.3f + Main.rand.NextFloat(0.2f), 35);
                    }
                    
                    // Glyph swirl
                    CustomParticles.GlyphBurst(proj.Center, EnigmaPurple * 0.7f, 3, 2f);
                    
                    // Music note
                    if (Main.rand.NextBool(2))
                        ThemedParticles.EnigmaMusicNotes(proj.Center, 1, 10f);
                    
                    // Apply confusion to nearby enemies
                    foreach (NPC npc in Main.npc)
                    {
                        if (npc.active && !npc.friendly && npc.Distance(proj.Center) < 150f)
                        {
                            npc.AddBuff(BuffID.Confused, 120); // 2 seconds
                            npc.AddBuff(ModContent.BuffType<Debuffs.MysteryVaporDebuff>(), 180); // 3 seconds reduced damage
                        }
                    }
                }
            }
            
            // Sound
            SoundEngine.PlaySound(SoundID.Item34 with { Pitch = -0.3f, Volume = 0.6f }, Player.Center);
        }
        
        private void GrantRiddlesBlessing()
        {
            // Find a random minion to bless
            List<int> minionIndices = new List<int>();
            for (int i = 0; i < Main.projectile.Length; i++)
            {
                var proj = Main.projectile[i];
                if (proj.active && proj.owner == Player.whoAmI && proj.minion)
                {
                    minionIndices.Add(i);
                }
            }
            
            if (minionIndices.Count > 0)
            {
                blessedMinionIndex = minionIndices[Main.rand.Next(minionIndices.Count)];
                blessingDuration = BlessingDurationMax;
                
                var blessedProj = Main.projectile[blessedMinionIndex];
                
                // Blessing activation visual
                CustomParticles.GenericFlare(blessedProj.Center, EnigmaGreenFlame, 0.8f, 25);
                CustomParticles.GenericFlare(blessedProj.Center, Color.White, 0.5f, 20);
                CustomParticles.GlyphCircle(blessedProj.Center, EnigmaPurple, 6, 40f, 0.04f);
                CustomParticles.EnigmaEyeFormation(blessedProj.Center, EnigmaGreenFlame * 0.8f, 3, 30f);
                ThemedParticles.EnigmaMusicNotes(blessedProj.Center, 4, 20f);
                
                SoundEngine.PlaySound(SoundID.Item4 with { Pitch = 0.3f, Volume = 0.8f }, blessedProj.Center);
            }
        }
        
        #endregion
        
        #region Stat Modifications
        
        public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
        {
            // Puzzle Mastery - +30% magic damage
            if (puzzleMasteryActive && item.DamageType == DamageClass.Magic)
            {
                damage += 0.30f;
            }
        }
        
        public override void ModifyManaCost(Item item, ref float reduce, ref float mult)
        {
            // Puzzle Mastery - -20% mana cost
            if (puzzleMasteryActive && item.DamageType == DamageClass.Magic)
            {
                mult *= 0.8f;
            }
        }
        
        public override void ModifyWeaponCrit(Item item, ref float crit)
        {
            // Alchemical Paradox - +8% ranged crit
            if (hasAlchemicalParadox && item.DamageType == DamageClass.Ranged)
            {
                crit += 8f;
            }
        }
        
        public override void UpdateEquips()
        {
            // Ignition of Mystery - melee speed per stack
            if (hasIgnitionOfMystery && mysteryStacks > 0)
            {
                float speedBonus = mysteryStacks * 0.02f; // 2% per stack, max 20%
                Player.GetAttackSpeed(DamageClass.Melee) += speedBonus;
            }
            
            // Riddlemaster's Cauldron - +1 minion
            if (hasRiddlemastersCauldron)
            {
                Player.maxMinions += 1;
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        public static Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreenFlame, (progress - 0.5f) * 2f);
        }
        
        #endregion
        
        /// <summary>
        /// Internal class to track puzzle fragments
        /// </summary>
        private class PuzzleFragment
        {
            public Vector2 Position;
            public float Angle;
            public float OrbitRadius;
            public int Lifetime;
            public Color Color;
        }
    }
}
