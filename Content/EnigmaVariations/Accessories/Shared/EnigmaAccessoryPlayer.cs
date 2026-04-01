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
    /// Colors: Black ↁEDeep Purple ↁEEerie Green Flame
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
                    
                    if (puzzleMasteryTimer <= 0)
                    {
                        puzzleMasteryActive = false;
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
            
            // Expanding halo rings
            
            // Eyes exploding outward - WATCHING EVERYTHING
            
            // Glyph cascade
            
            // Music notes spiral
            
            // Fractal burst pattern
            
            // Apply "Watched" debuff to all enemies in range
            float watchRadius = 300f;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.Distance(burstCenter) < watchRadius)
                {
                    // Apply watched debuff (increases damage taken)
                    npc.AddBuff(ModContent.BuffType<Debuffs.WatchedDebuff>(), 300); // 5 seconds
                    
                    // Eye spawns watching this enemy
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
                    activeFragments.RemoveAt(i);
                }
            }
        }
        
        private void CollectPuzzleFragment(PuzzleFragment fragment)
        {
            puzzleFragments++;
            
            // Collection effect
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
            
            // Central burst
            
            // Eyes form a watching formation
            
            // Expanding halos
            
            // Music explosion
            
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
                    }
                    
                    // Glyph swirl
                    
                    // Music note
                    if (Main.rand.NextBool(2))
                    
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
