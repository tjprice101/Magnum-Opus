using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Centralized boss dialogue system for MagnumOpus.
    /// Handles thematic dialogue during boss fights with timing control.
    /// </summary>
    public static class BossDialogueSystem
    {
        // Dialogue cooldown tracking per boss
        private static Dictionary<string, int> lastDialogueTime = new Dictionary<string, int>();
        private const int MinDialogueCooldown = 300; // 5 seconds between dialogues
        
        // Track which dialogues have been said this fight (per boss instance)
        private static Dictionary<int, HashSet<string>> spokenDialogues = new Dictionary<int, HashSet<string>>();
        
        /// <summary>
        /// Reset dialogue tracking for a boss (call on spawn)
        /// </summary>
        public static void ResetDialogueTracking(int bossWhoAmI)
        {
            if (spokenDialogues.ContainsKey(bossWhoAmI))
                spokenDialogues[bossWhoAmI].Clear();
            else
                spokenDialogues[bossWhoAmI] = new HashSet<string>();
        }
        
        /// <summary>
        /// Clean up dialogue tracking (call on boss death/despawn)
        /// </summary>
        public static void CleanupDialogue(int bossWhoAmI)
        {
            spokenDialogues.Remove(bossWhoAmI);
        }
        
        /// <summary>
        /// Attempt to say a dialogue line if cooldown allows
        /// </summary>
        public static bool TrySayDialogue(string bossKey, string dialogueKey, string message, Color color, bool uniquePerFight = false, int customCooldown = -1)
        {
            string fullKey = $"{bossKey}_{dialogueKey}";
            int cooldown = customCooldown > 0 ? customCooldown : MinDialogueCooldown;
            
            // Check cooldown
            if (lastDialogueTime.TryGetValue(fullKey, out int lastTime))
            {
                if (Main.GameUpdateCount - lastTime < cooldown)
                    return false;
            }
            
            // Check unique per fight
            if (uniquePerFight)
            {
                // Find this boss in tracking
                foreach (var kvp in spokenDialogues)
                {
                    if (kvp.Value.Contains(fullKey))
                        return false;
                }
            }
            
            // Say the dialogue
            Main.NewText(message, color);
            lastDialogueTime[fullKey] = (int)Main.GameUpdateCount;
            
            // Mark as spoken if unique
            if (uniquePerFight)
            {
                foreach (var kvp in spokenDialogues)
                {
                    kvp.Value.Add(fullKey);
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Say dialogue unconditionally (for important moments)
        /// </summary>
        public static void SayDialogue(string message, Color color)
        {
            Main.NewText(message, color);
        }
        
        #region ===== EROICA DIALOGUE =====
        // STORY: Eroica is a guardian who knows what Fate holds back - the Revolutionary Melodies
        // He warns the player about the chain of events they're starting
        // His death begins the path toward releasing the imprisoned songs
        
        public static class Eroica
        {
            public static readonly Color DialogueColor = new Color(255, 200, 80);
            public static readonly Color EnragedColor = new Color(255, 100, 50);
            public static readonly Color OminousColor = new Color(180, 150, 100);
            private const string Key = "Eroica";
            
            // Spawn dialogue
            public static void OnSpawn(int whoAmI)
            {
                ResetDialogueTracking(whoAmI);
                SayDialogue("Another warrior seeks glory. Do you know what melodies Fate holds back?", DialogueColor);
            }
            
            // Phase transitions - reveals more of the story
            public static void OnPhase2(int whoAmI)
            {
                TrySayDialogue(Key, "Phase2", "I became a guardian to PROTECT the world from what's imprisoned. Four revolutionary melodies that could unmake everything!", DialogueColor, true);
            }
            
            public static void OnPhase3(int whoAmI)
            {
                TrySayDialogue(Key, "Phase3", "Ode to Joy. Dies Irae. Nachtmusik. And the score of time itself... Clair de Lune. Fate holds them ALL back!", OminousColor, true);
            }
            
            // Combat taunts - mix of fighting spirit and warnings about the Revolutionary Melodies
            public static void RandomCombatTaunt()
            {
                string[] taunts = new[]
                {
                    "Fate is the Warden! He holds back melodies that could SHATTER reality!",
                    "Do you know what Dies Irae means? 'Day of Wrath.' It will be YOUR day if you free it!",
                    "Clair de Lune doesn't just play music. It plays TIME. Backwards, forwards, sideways...",
                    "Ode to Joy sounds beautiful until you realize what it CELEBRATES.",
                    "Nachtmusik whispers in the dark. Once you hear it, you never stop hearing it.",
                    "We became guardians to keep those melodies SILENT. And you're unmaking our work!",
                    "Your valor is admirable. Your ignorance will doom us all.",
                    "Fate holds the score of time itself. If he falls, time becomes... optional."
                };
                
                if (Main.rand.NextBool(180))
                {
                    TrySayDialogue(Key, "CombatTaunt", taunts[Main.rand.Next(taunts.Length)], DialogueColor, false, 360);
                }
            }
            
            // Attack-specific
            public static void OnHeroesJudgment()
            {
                TrySayDialogue(Key, "HeroesJudgment", "WITNESS THE HERO'S JUDGMENT! May it show you the truth!", EnragedColor, false, 600);
            }
            
            public static void OnUltimateValor()
            {
                TrySayDialogue(Key, "UltimateValor", "If you won't listen... then FALL like I did!", EnragedColor, true);
            }
            
            // Player low HP
            public static void OnPlayerLowHP()
            {
                TrySayDialogue(Key, "PlayerLow", "Stay down! It's not too late to stop this!", DialogueColor, false, 600);
            }
            
            // Enrage
            public static void OnEnrage()
            {
                TrySayDialogue(Key, "Enrage", "You cannot run from destiny! NONE OF US CAN!", EnragedColor, true);
            }
            
            // Death - the first guardian falls
            public static void OnDeath()
            {
                SayDialogue("No... the first guardian falls. The path to Fate begins.", OminousColor);
                SayDialogue("The Bell will mark your progress. The Mystery will question your resolve. The Swan will test your worth.", new Color(255, 220, 180));
                SayDialogue("And Fate... Fate will show you why those melodies must NEVER be free.", new Color(200, 180, 120));
            }
        }
        #endregion
        
        #region ===== LA CAMPANELLA DIALOGUE =====
        // STORY: La Campanella is the second guardian - the Bell that knows the rhythm of the imprisoned melodies
        // She speaks of the Revolutionary Melodies and what happens when Fate falls
        // The Bell has been counting since the beginning
        
        public static class LaCampanella
        {
            public static readonly Color DialogueColor = new Color(255, 140, 40);
            public static readonly Color EnragedColor = new Color(255, 80, 30);
            public static readonly Color PropheticColor = new Color(255, 200, 100);
            private const string Key = "LaCampanella";
            
            public static void OnSpawn(int whoAmI)
            {
                ResetDialogueTracking(whoAmI);
                SayDialogue("The Bell tolls... marking another step toward Fate. The Hero couldn't stop you.", DialogueColor);
            }
            
            public static void OnPhase2(int whoAmI)
            {
                TrySayDialogue(Key, "Phase2", "I've counted eons waiting for Fate to falter. Four melodies strain against his will every second.", PropheticColor, true);
            }
            
            public static void OnPhase3(int whoAmI)
            {
                TrySayDialogue(Key, "Phase3", "Dies Irae hungers for wrath! Nachtmusik whispers madness! And Clair de Lune... it wants to REWIND everything!", EnragedColor, true);
            }
            
            public static void RandomCombatTaunt()
            {
                string[] taunts = new[]
                {
                    "Fate holds back the score of time itself. Do you understand what that MEANS?",
                    "Ode to Joy will make you dance until your bones shatter from ecstasy.",
                    "Dies Irae is judgment incarnate. It decides who lives and who NEVER EXISTED.",
                    "Nachtmusik plays in the spaces between thoughts. It drives men to beautiful madness.",
                    "Clair de Lune is the gentlest... and the most terrifying. Time bends to its melody.",
                    "The Mystery knows what happens when all four melodies play at once. Ask her.",
                    "When Fate falls... four prisons open. Are you ready to face what's inside?",
                    "The Swan believes someone can survive the Revolutionary Melodies. I have my doubts."
                };
                
                if (Main.rand.NextBool(180))
                {
                    TrySayDialogue(Key, "CombatTaunt", taunts[Main.rand.Next(taunts.Length)], DialogueColor, false, 360);
                }
            }
            
            public static void OnInfernalJudgment()
            {
                TrySayDialogue(Key, "InfernalJudgment", "HEAR THE TOLL OF JUDGMENTS PAST!", EnragedColor, false, 600);
            }
            
            public static void OnGrandFinale()
            {
                TrySayDialogue(Key, "GrandFinale", "If the Bell falls silent... so does the warning. You'll face what comes DEAF!", EnragedColor, true);
            }
            
            public static void OnPlayerLowHP()
            {
                TrySayDialogue(Key, "PlayerLow", "Falling so soon? The Conductor's music is far more... intense.", DialogueColor, false, 600);
            }
            
            public static void OnEnrage()
            {
                TrySayDialogue(Key, "Enrage", "The Bell tolls for YOU! The countdown CANNOT BE STOPPED!", EnragedColor, true);
            }
            
            public static void OnDeath()
            {
                SayDialogue("The Bell... falls silent. Two guardians down.", PropheticColor);
                SayDialogue("The Mystery will tell you WHY we guard. The Swan will show you HOW to survive.", new Color(255, 200, 150));
                SayDialogue("And Fate... pray you never learn what Fate endures to keep those melodies caged.", new Color(200, 150, 100));
            }
        }
        #endregion
        
        #region ===== ENIGMA DIALOGUE =====
        // STORY: Enigma knows the full truth about the Revolutionary Melodies
        // She reveals their nature and why Fate must hold them back
        // Her riddles protect minds from comprehending the true danger
        
        public static class Enigma
        {
            public static readonly Color DialogueColor = new Color(140, 60, 200);
            public static readonly Color VoidColor = new Color(50, 220, 100);
            public static readonly Color TruthColor = new Color(200, 150, 255);
            private const string Key = "Enigma";
            
            public static void OnSpawn(int whoAmI)
            {
                ResetDialogueTracking(whoAmI);
                SayDialogue("The Bell is silenced. Now you seek answers. Here is a riddle: What song plays itself?", DialogueColor);
            }
            
            public static void OnPhase2(int whoAmI)
            {
                TrySayDialogue(Key, "Phase2", "Four melodies exist that were never COMPOSED. They simply ARE. They predate silence itself.", TruthColor, true);
            }
            
            public static void OnPhase3(int whoAmI)
            {
                TrySayDialogue(Key, "Phase3", "Ode to Joy is rapture. Dies Irae is wrath. Nachtmusik is madness. Clair de Lune is TIME. And Fate... Fate holds them ALL!", VoidColor, true);
            }
            
            public static void RandomCombatTaunt()
            {
                string[] taunts = new[]
                {
                    "Riddle: What conducts without hands? Answer: Clair de Lune conducts TIME.",
                    "Why do we guard Fate? Because if he falls, FOUR apocalypses begin simultaneously.",
                    "Ode to Joy makes you feel everything beautiful at once. Your mind cannot SURVIVE that much joy.",
                    "Dies Irae doesn't kill. It JUDGES. And its judgment is always the same: guilty.",
                    "Nachtmusik plays between your thoughts. Soon you can't tell which thoughts are yours.",
                    "Clair de Lune is gentle... until you realize it's been playing you on repeat for a thousand years.",
                    "Fate endures agony every second to keep those four melodies from playing. Every. Single. Second.",
                    "The Swan will test if you can survive ONE melody. Fate holds back FOUR.",
                    "When all four play together... reality doesn't end. It was never REAL to begin with.",
                    "You can still walk away. The melodies will stay caged. Fate will suffer alone. Is that mercy?"
                };
                
                if (Main.rand.NextBool(180))
                {
                    TrySayDialogue(Key, "CombatTaunt", taunts[Main.rand.Next(taunts.Length)], DialogueColor, false, 360);
                }
            }
            
            public static void OnParadoxJudgment()
            {
                TrySayDialogue(Key, "ParadoxJudgment", "THE TRUTH AND THE LIE ARE ONE!", VoidColor, false, 600);
            }
            
            public static void OnVoidLaserWeb()
            {
                TrySayDialogue(Key, "VoidWeb", "The web of secrets catches all who seek answers!", DialogueColor, false, 600);
            }
            
            public static void OnPlayerLowHP()
            {
                TrySayDialogue(Key, "PlayerLow", "Dying to avoid the truth? That's... almost merciful.", DialogueColor, false, 600);
            }
            
            public static void OnEnrage()
            {
                TrySayDialogue(Key, "Enrage", "You cannot run from questions you've already asked!", VoidColor, true);
            }
            
            public static void OnDeath()
            {
                SayDialogue("The riddle is answered. Three guardians fallen.", TruthColor);
                SayDialogue("The Swan awaits. She will show you what ONE melody can do. A taste of what Fate endures.", new Color(100, 180, 140));
                SayDialogue("Remember: Those melodies have been trying to escape since before time had a name.", VoidColor);
            }
        }
        #endregion
        
        #region ===== SWAN LAKE DIALOGUE =====
        // STORY: The Swan demonstrates the POWER of a single melody through combat
        // She prepares the player for what happens when Fate falls
        // Her dance is a taste of the beauty and terror the Revolutionary Melodies represent
        
        public static class SwanLake
        {
            public static readonly Color DialogueColor = new Color(255, 255, 255);
            public static readonly Color SternColor = new Color(180, 180, 220);
            public static readonly Color DyingSwanColor = new Color(255, 200, 255);
            public static readonly Color LoveColor = new Color(255, 220, 240);
            private const string Key = "SwanLake";
            
            public static void OnSpawn(int whoAmI)
            {
                ResetDialogueTracking(whoAmI);
                SayDialogue("The Mystery revealed the truth. Now I will show you what a SINGLE melody can do.", DialogueColor);
            }
            
            public static void OnGracefulMood(int whoAmI)
            {
                TrySayDialogue(Key, "Graceful", "My dance is but a whisper of what those four melodies contain. Watch. Learn. Survive.", LoveColor, true);
            }
            
            public static void OnTempestMood(int whoAmI)
            {
                TrySayDialogue(Key, "Tempest", "Fate holds back FOUR of these storms! Every moment! For EONS! Feel what ONE can do!", SternColor, true);
            }
            
            public static void OnDyingSwanMood(int whoAmI)
            {
                TrySayDialogue(Key, "DyingSwan", "My final dance... a fraction of what Clair de Lune alone could unleash. Are you ready for Fate?", DyingSwanColor, true);
            }
            
            public static void RandomCombatTaunt()
            {
                string[] taunts = new[]
                {
                    "Ode to Joy will make this dance feel like NOTHING. I am PREPARING you!",
                    "Dies Irae will judge you the moment Fate falls. You must be found WORTHY.",
                    "Nachtmusik will whisper in your dreams. You must learn to keep your own thoughts.",
                    "Clair de Lune will try to loop you in time forever. You must learn to BREAK free.",
                    "Fate is not your enemy! He is the only thing standing between you and FOUR apocalypses!",
                    "I've waited eons for someone who could survive my dance. You're the first.",
                    "The others fell to protect. I dance to PREPARE. There is a difference.",
                    "This beauty and terror? Imagine it multiplied by FOUR. That is what awaits.",
                    "When those melodies play, reality becomes their instrument. You must be the dissonance.",
                    "A mother's love can be fierce. This is the fiercest love I can give you."
                };
                
                if (Main.rand.NextBool(180))
                {
                    TrySayDialogue(Key, "CombatTaunt", taunts[Main.rand.Next(taunts.Length)], DialogueColor, false, 360);
                }
            }
            
            public static void OnSwanSerenade()
            {
                TrySayDialogue(Key, "SwanSerenade", "The Swan Serenade... a dance I choreographed FOR this moment!", DialogueColor, false, 600);
            }
            
            public static void OnMonochromaticApocalypse()
            {
                TrySayDialogue(Key, "Apocalypse", "WITNESS WHAT THE CONDUCTOR WILL DO A THOUSANDFOLD!", SternColor, false, 600);
            }
            
            public static void OnPrismaticRadiance()
            {
                TrySayDialogue(Key, "Prismatic", "Every color of existence! Learn to see through them all!", DialogueColor, false, 600);
            }
            
            public static void OnPlayerLowHP()
            {
                TrySayDialogue(Key, "PlayerLow", "Rise, child. You've survived worse than this. I've made SURE of it.", DyingSwanColor, false, 600);
            }
            
            public static void OnEnrage()
            {
                TrySayDialogue(Key, "Enrage", "Fleeing?! After everything we've given you?! COWARD!", SternColor, true);
            }
            
            public static void OnDeath()
            {
                SayDialogue("The dance ends... but yours is just beginning, child.", LoveColor);
                SayDialogue("Four guardians have fallen. Only Fate remains. He holds back the score of time itself.", DyingSwanColor);
                SayDialogue("Clair de Lune. Dies Irae. Nachtmusik. Ode to Joy. He contains them ALL.", new Color(255, 240, 255));
                SayDialogue("Free him from his burden... and pray you're ready for what follows.", new Color(220, 220, 255));
            }
        }
        #endregion
        
        #region ===== FATE DIALOGUE =====
        // STORY: Fate is the Warden who holds back the Revolutionary Melodies
        // He holds the score of time (Clair de Lune) and three other devastating compositions
        // His death releases: Ode to Joy, Dies Irae, Nachtmusik, and Clair de Lune
        
        public static class Fate
        {
            public static readonly Color DialogueColor = new Color(180, 50, 100);
            public static readonly Color CosmicColor = new Color(255, 60, 80);
            public static readonly Color DivinityColor = new Color(255, 255, 255);
            public static readonly Color DesperationColor = new Color(255, 180, 200);
            public static readonly Color DreadColor = new Color(100, 50, 80);
            public static readonly Color MelodyColor = new Color(200, 100, 150);
            private const string Key = "Fate";
            
            public static void OnSpawn(int whoAmI)
            {
                ResetDialogueTracking(whoAmI);
                SayDialogue("The Hero. The Bell. The Mystery. The Swan. Four guardians, slain by your hand.", DialogueColor);
            }
            
            public static void OnSpawnDelayed()
            {
                TrySayDialogue(Key, "SpawnDelayed", "Do you know what I hold back?! FOUR MELODIES that could unmake EVERYTHING!", DesperationColor, true, 120);
            }
            
            public static void OnPhase2(int whoAmI)
            {
                TrySayDialogue(Key, "Phase2", "I hold the score of time! Clair de Lune strains against me EVERY MOMENT! If I fall, time itself unravels!", CosmicColor, true);
            }
            
            public static void OnPhase3(int whoAmI)
            {
                TrySayDialogue(Key, "Phase3", "ODE TO JOY! DIES IRAE! NACHTMUSIK! CLAIR DE LUNE! They're SCREAMING to be free! Can't you HEAR them?!", CosmicColor, true);
            }
            
            public static void OnCosmicWrath(int whoAmI)
            {
                TrySayDialogue(Key, "CosmicWrath", "IF I MUST BREAK REALITY TO STOP YOU, SO BE IT! THOSE MELODIES CANNOT BE FREED!", DivinityColor, true);
            }
            
            public static void RandomCombatTaunt()
            {
                string[] taunts = new[]
                {
                    "Clair de Lune whispers to me every second. 'Let me play. Let me rewind. Let me FIX everything.'",
                    "Dies Irae SCREAMS for judgment! It wants to decide who deserves to EXIST!",
                    "Ode to Joy promises paradise. But joy that intense... it BURNS the soul to ash!",
                    "Nachtmusik hums in the dark. I haven't slept in eons. I CAN'T. It would escape.",
                    "The score of time wants to play BACKWARDS. Do you understand? UNMAKE everything!",
                    "I don't WANT this burden! But someone has to hold these melodies SILENT!",
                    "The Hero tried to warn you. The Bell counted down. The Mystery explained. WHY WON'T YOU STOP?!",
                    "I've held these four prisons for EONS. And you'll shatter them in MINUTES!",
                    "Do you hear them? The melodies? They know you're close. They're getting LOUDER.",
                    "I'm not your enemy! I'm the only thing standing between you and FOUR APOCALYPSES!"
                };
                
                if (Main.rand.NextBool(150))
                {
                    TrySayDialogue(Key, "CombatTaunt", taunts[Main.rand.Next(taunts.Length)], DialogueColor, false, 300);
                }
            }
            
            public static void OnUniversalJudgment()
            {
                TrySayDialogue(Key, "UniversalJudgment", "UNIVERSAL JUDGMENT! The stars themselves DEMAND you stop!", CosmicColor, false, 600);
            }
            
            public static void OnFinalMelody()
            {
                TrySayDialogue(Key, "FinalMelody", "THE FINAL MELODY! My own song against the four I contain! If this doesn't stop you...", DivinityColor, true);
            }
            
            public static void OnTimeSlice()
            {
                TrySayDialogue(Key, "TimeSlice", "A fraction of Clair de Lune's power! TIME BENDS to my will!", MelodyColor, false, 600);
            }
            
            public static void OnConstellationStrike()
            {
                TrySayDialogue(Key, "Constellation", "The stars are notes! The cosmos is sheet music! And I am the SILENCE between them!", DialogueColor, false, 600);
            }
            
            public static void OnPlayerLowHP()
            {
                TrySayDialogue(Key, "PlayerLow", "YES! Fall! Stay down! For the love of all music, STAY DOWN!", DesperationColor, false, 600);
            }
            
            public static void OnEnrage()
            {
                TrySayDialogue(Key, "Enrage", "There is NOWHERE to run! Not from me! NOT FROM HIM!", CosmicColor, true);
            }
            
            // Death - THE CLIMAX. The four Revolutionary Melodies are freed.
            public static void OnDeath()
            {
                SayDialogue("No... NO... The chains... shatter...", DreadColor);
                SayDialogue("Do you hear them? The melodies... they're FREE...", DesperationColor);
                SayDialogue("Ode to Joy... it rises...", new Color(255, 220, 100));
                SayDialogue("Dies Irae... it JUDGES...", new Color(200, 50, 50));
                SayDialogue("Nachtmusik... it WHISPERS...", new Color(100, 80, 150));
                SayDialogue("And Clair de Lune... the score of time... it PLAYS...", new Color(150, 200, 255));
                SayDialogue("I hope the Swan prepared you well. The Revolutionary Melodies... are AWAKE.", DivinityColor);
            }
            
            // Special method for post-death world message
            public static void OnPostDeath()
            {
                // Call this ~5 seconds after death for dramatic effect
                SayDialogue("Four prisons lie empty...", new Color(80, 40, 60));
                SayDialogue("Ode to Joy celebrates. Dies Irae judges. Nachtmusik whispers. Clair de Lune... rewinds.", new Color(150, 80, 100));
                SayDialogue("The Revolutionary Melodies have returned. And they remember EVERYTHING.", DivinityColor);
            }
        }
        #endregion
        
        #region ===== MINIBOSS DIALOGUE (Movements) =====
        
        public static class MovementI
        {
            public static readonly Color DialogueColor = new Color(255, 150, 200);
            private const string Key = "MovementI";
            
            public static void OnSpawn(int whoAmI)
            {
                ResetDialogueTracking(whoAmI);
                TrySayDialogue(Key, "Spawn", "The First Movement begins... your prelude to despair.", DialogueColor, true);
            }
            
            public static void OnDeath()
            {
                SayDialogue("Movement I has concluded...", DialogueColor);
            }
        }
        
        public static class MovementII
        {
            public static readonly Color DialogueColor = new Color(255, 120, 180);
            private const string Key = "MovementII";
            
            public static void OnSpawn(int whoAmI)
            {
                ResetDialogueTracking(whoAmI);
                TrySayDialogue(Key, "Spawn", "The Second Movement rises... the tempo quickens.", DialogueColor, true);
            }
            
            public static void OnDeath()
            {
                SayDialogue("Movement II has concluded...", DialogueColor);
            }
        }
        
        public static class MovementIII
        {
            public static readonly Color DialogueColor = new Color(255, 200, 80);
            private const string Key = "MovementIII";
            
            public static void OnSpawn(int whoAmI)
            {
                ResetDialogueTracking(whoAmI);
                TrySayDialogue(Key, "Spawn", "The Third Movement ERUPTS! Face the finale!", DialogueColor, true);
            }
            
            public static void OnDeath()
            {
                SayDialogue("Movement III has reached its finale!", DialogueColor);
            }
        }
        
        public static class MysterysEnd
        {
            public static readonly Color DialogueColor = new Color(140, 60, 200);
            public static readonly Color VoidColor = new Color(50, 220, 100);
            private const string Key = "MysterysEnd";
            
            public static void OnSpawn(int whoAmI)
            {
                ResetDialogueTracking(whoAmI);
                TrySayDialogue(Key, "Spawn", "The jungle hides many secrets... I am one of them.", DialogueColor, true);
            }
            
            public static void OnPhase2()
            {
                TrySayDialogue(Key, "Phase2", "You peel back layers... but mysteries only deepen.", VoidColor, true);
            }
            
            public static void OnDeath()
            {
                SayDialogue("The mystery... remains unsolved. For now.", DialogueColor);
            }
        }
        #endregion
        
        #region ===== UTILITY METHODS =====
        
        /// <summary>
        /// Call this in boss AI to check if player is low HP and trigger dialogue
        /// </summary>
        public static void CheckPlayerLowHP(Player target, string bossType)
        {
            if (target == null || !target.active) return;
            
            float hpPercent = (float)target.statLife / target.statLifeMax2;
            if (hpPercent < 0.25f)
            {
                switch (bossType.ToLower())
                {
                    case "eroica": Eroica.OnPlayerLowHP(); break;
                    case "lacampanella": LaCampanella.OnPlayerLowHP(); break;
                    case "enigma": Enigma.OnPlayerLowHP(); break;
                    case "swanlake": SwanLake.OnPlayerLowHP(); break;
                    case "fate": Fate.OnPlayerLowHP(); break;
                }
            }
        }
        
        /// <summary>
        /// Call this in boss AI every frame for random combat taunts
        /// </summary>
        public static void UpdateCombatDialogue(string bossType)
        {
            switch (bossType.ToLower())
            {
                case "eroica": Eroica.RandomCombatTaunt(); break;
                case "lacampanella": LaCampanella.RandomCombatTaunt(); break;
                case "enigma": Enigma.RandomCombatTaunt(); break;
                case "swanlake": SwanLake.RandomCombatTaunt(); break;
                case "fate": Fate.RandomCombatTaunt(); break;
            }
        }
        
        #endregion
    }
}
