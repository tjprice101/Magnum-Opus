using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.EnigmaVariations.Debuffs;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons
{
    /// <summary>
    /// FUGUE OF THE UNKNOWN - Enigma Magic Tome
    /// =========================================
    /// A musical fugue where each voice builds upon the last, creating layered complexity.
    /// 
    /// UNIQUE MECHANICS:
    /// - Left-click fires "Voice" projectiles that orbit around you before launching
    /// - Each cast adds another voice to the fugue (up to 5 voices orbiting)
    /// - Right-click RELEASES all voices as a coordinated assault
    /// - Released voices spiral outward in a fugue pattern, homing on enemies
    /// - When voices hit, they leave "Echo Marks" on enemies
    /// - Enemies with 3+ Echo Marks trigger "Harmonic Convergence" - all marks detonate
    /// - Harmonic Convergence creates chain reactions between marked enemies
    /// - While holding voices, player has a mysterious aura with orbiting glyphs and eyes
    /// </summary>
    public class FugueOfTheUnknown : ModItem
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        
        public override string Texture => "MagnumOpus/Content/EnigmaVariations/ResonantWeapons/FugueOfTheUnknown";
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override void SetDefaults()
        {
            Item.damage = 380;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 8;
            Item.width = 24;
            Item.height = 24;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Swing; // Held like a lantern
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 20);
            Item.rare = ModContent.RarityType<EnigmaRarity>();
            Item.UseSound = SoundID.Item117;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<FugueVoiceProjectile>();
            Item.shootSpeed = 0f;
            Item.noMelee = true;
            Item.holdStyle = ItemHoldStyleID.HoldLamp; // Lantern hold style
            Item.scale = 0.32f; // 60% smaller
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect1", "Left-click summons orbiting Voices of the Fugue (up to 5)"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect2", "Right-click releases all voices in a spiraling assault"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect3", "Voices leave Echo Marks - 3 marks trigger Harmonic Convergence"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect4", "Convergence chains between all marked enemies for massive damage"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaLore", "'A fugue is never truly finished - each voice echoes into eternity'") 
            { 
                OverrideColor = EnigmaPurple 
            });
        }
        
        public override void HoldItem(Player player)
        {
            // Count active voice projectiles
            int voiceCount = 0;
            foreach (Projectile proj in Main.ActiveProjectiles)
            {
                if (proj.type == ModContent.ProjectileType<FugueVoiceProjectile>() && 
                    proj.owner == player.whoAmI && proj.ai[0] == 0) // ai[0] = 0 means orbiting
                {
                    voiceCount++;
                }
            }
            
            // Subtle ambient aura based on voice count
            if (voiceCount > 0)
            {
                float intensity = (float)voiceCount / 5f;
                
                // Occasional orbiting glyph
                if (Main.rand.NextBool(30))
                {
                    float angle = Main.GameUpdateCount * 0.04f;
                    Vector2 glyphPos = player.Center + angle.ToRotationVector2() * 50f;
                    CustomParticles.Glyph(glyphPos, GetEnigmaGradient(intensity), 0.3f, -1);
                }
                
                // Subtle music notes
                if (Main.rand.NextBool(40))
                {
                    ThemedParticles.EnigmaMusicNotes(player.Center + Main.rand.NextVector2Circular(40f, 40f), 1, 20f);
                }
                
                // Pulsing aura light
                float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.15f + 0.85f;
                Lighting.AddLight(player.Center, GetEnigmaGradient(intensity).ToVector3() * intensity * pulse * 0.4f);
            }
        }
        
        public override bool AltFunctionUse(Player player) => true;
        
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Right-click: Release all voices
                Item.useTime = 25;
                Item.useAnimation = 25;
                Item.mana = 0;
            }
            else
            {
                // Left-click: Summon voice
                Item.useTime = 18;
                Item.useAnimation = 18;
                Item.mana = 8;
            }
            return base.CanUseItem(player);
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                // RIGHT-CLICK: Release all orbiting voices!
                ReleaseAllVoices(player);
                return false;
            }
            
            // LEFT-CLICK: Summon a new voice
            // Count existing voices
            int voiceCount = 0;
            foreach (Projectile proj in Main.ActiveProjectiles)
            {
                if (proj.type == type && proj.owner == player.whoAmI && proj.ai[0] == 0)
                    voiceCount++;
            }
            
            if (voiceCount >= 5)
            {
                // Max voices - auto-release
                ReleaseAllVoices(player);
                return false;
            }
            
            // Spawn new voice
            int voiceIndex = voiceCount;
            Projectile.NewProjectile(source, player.Center, Vector2.Zero, type, damage, knockback, 
                player.whoAmI, ai0: 0, ai1: voiceIndex);
            
            // Summon VFX
            Vector2 summonPos = player.Center;
            CustomParticles.GenericFlare(summonPos, EnigmaPurple, 0.6f, 18);
            CustomParticles.HaloRing(summonPos, EnigmaGreen * 0.7f, 0.3f, 14);
            
            // Musical note burst - each voice adds to the fugue
            ThemedParticles.EnigmaMusicNoteBurst(summonPos, 4 + voiceCount, 3f);
            
            // Glyph at summon
            CustomParticles.Glyph(summonPos, GetEnigmaGradient((float)voiceCount / 5f), 0.45f, voiceCount);
            
            SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.2f + voiceCount * 0.15f, Volume = 0.6f }, summonPos);
            
            return false;
        }
        
        private void ReleaseAllVoices(Player player)
        {
            List<Projectile> voices = new List<Projectile>();
            
            foreach (Projectile proj in Main.ActiveProjectiles)
            {
                if (proj.type == ModContent.ProjectileType<FugueVoiceProjectile>() && 
                    proj.owner == player.whoAmI && proj.ai[0] == 0)
                {
                    voices.Add(proj);
                }
            }
            
            if (voices.Count == 0) return;
            
            // Release VFX - clean and focused
            CustomParticles.GenericFlare(player.Center, EnigmaGreen, 0.7f, 20);
            CustomParticles.HaloRing(player.Center, EnigmaPurple, 0.4f, 15);
            
            // Music notes
            ThemedParticles.EnigmaMusicNoteBurst(player.Center, 6, 4f);
            
            // Release each voice with staggered timing
            int index = 0;
            foreach (var voice in voices)
            {
                // Set to released state
                voice.ai[0] = 1; // Released
                voice.ai[1] = index; // Release order
                
                // Find target
                NPC target = FindNearestEnemy(voice.Center, 800f);
                if (target != null)
                {
                    voice.ai[2] = target.whoAmI;
                }
                else
                {
                    // No target - fire toward mouse
                    Vector2 toMouse = (Main.MouseWorld - voice.Center).SafeNormalize(Vector2.UnitX);
                    voice.velocity = toMouse * 14f;
                    voice.ai[2] = -1;
                }
                
                // Release VFX at voice position
                CustomParticles.GenericFlare(voice.Center, GetEnigmaGradient((float)index / voices.Count), 0.55f, 16);
                
                index++;
            }
            
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.4f, Volume = 0.8f }, player.Center);
        }
        
        private NPC FindNearestEnemy(Vector2 pos, float range)
        {
            NPC closest = null;
            float closestDist = range;
            
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly || npc.immortal) continue;
                float dist = Vector2.Distance(pos, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            
            return closest;
        }
    }
    
    /// <summary>
    /// Fugue Voice - Orbits the player then homes on enemies when released
    /// </summary>
    public class FugueVoiceProjectile : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private float orbitAngle = 0f;
        private float orbitRadius = 60f;
        private int releaseTimer = 0;
        private int glyphIndex = 0;
        
        // ai[0] = state (0 = orbiting, 1 = released)
        // ai[1] = voice index (for orbit position) / release order
        // ai[2] = target NPC index when released
        
        private bool IsOrbiting => Projectile.ai[0] == 0;
        private int VoiceIndex => (int)Projectile.ai[1];
        private int TargetIndex => (int)Projectile.ai[2];
        
        public override string Texture => "MagnumOpus/Assets/Particles/Glyphs7";
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            float pulse = 0.85f + (float)Math.Sin(Main.GameUpdateCount * 0.12f + VoiceIndex * 0.8f) * 0.2f;
            float voiceHue = (float)VoiceIndex / 5f;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Texture2D glyphTex = CustomParticleSystem.RandomGlyph().Value;
            Texture2D sparkleTex = CustomParticleSystem.RandomPrismaticSparkle().Value;
            Texture2D eyeTex = CustomParticleSystem.RandomEnigmaEye().Value;
            Texture2D flareTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow3").Value;
            
            // === TRAIL ===
            if (!IsOrbiting && ProjectileID.Sets.TrailCacheLength[Projectile.type] > 0)
            {
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero) continue;
                    float trailProgress = (float)i / Projectile.oldPos.Length;
                    float trailAlpha = (1f - trailProgress) * 0.8f;
                    float trailScale = (1f - trailProgress * 0.5f) * 0.4f;
                    Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                    
                    Color trailColor = GetEnigmaGradient(voiceHue + trailProgress * 0.3f);
                    spriteBatch.Draw(sparkleTex, trailPos, null, trailColor * trailAlpha, 
                        Main.GameUpdateCount * 0.1f + i * 0.3f, sparkleTex.Size() / 2f, trailScale, SpriteEffects.None, 0f);
                    
                    if (i % 3 == 0)
                    {
                        spriteBatch.Draw(glyphTex, trailPos, null, EnigmaPurple * trailAlpha * 0.6f, 
                            -Main.GameUpdateCount * 0.05f + i, glyphTex.Size() / 2f, trailScale * 0.6f, SpriteEffects.None, 0f);
                    }
                }
            }
            
            // === ORBITING ELEMENTS ===
            // Glyph halo
            for (int i = 0; i < 4; i++)
            {
                float glyphAngle = Main.GameUpdateCount * 0.08f + MathHelper.TwoPi * i / 4f;
                float glyphRadius = 18f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + i) * 4f;
                Vector2 glyphPos = drawPos + glyphAngle.ToRotationVector2() * glyphRadius;
                Color glyphColor = Color.Lerp(EnigmaPurple, EnigmaGreen, (float)i / 4f) * 0.7f * pulse;
                spriteBatch.Draw(glyphTex, glyphPos, null, glyphColor, -glyphAngle, 
                    glyphTex.Size() / 2f, 0.25f * pulse, SpriteEffects.None, 0f);
            }
            
            // Central eye (watches toward center when orbiting, toward velocity when released)
            float eyeRot = IsOrbiting ? 
                (Main.player[Projectile.owner].Center - Projectile.Center).ToRotation() :
                Projectile.velocity.ToRotation();
            spriteBatch.Draw(eyeTex, drawPos, null, EnigmaGreen * 0.8f * pulse, eyeRot, 
                eyeTex.Size() / 2f, 0.4f * pulse, SpriteEffects.None, 0f);
            
            // Core glow layers
            Color coreColor = GetEnigmaGradient(voiceHue);
            spriteBatch.Draw(flareTex, drawPos, null, coreColor * 0.9f, 0f, flareTex.Size() / 2f, 0.5f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaPurple * 0.7f, 0f, flareTex.Size() / 2f, 0.35f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaGreen * 0.6f, 0f, flareTex.Size() / 2f, 0.2f * pulse, SpriteEffects.None, 0f);
            
            // Musical note orbiting (because MUSIC MOD!)
            float noteAngle = -Main.GameUpdateCount * 0.06f + VoiceIndex * 1.2f;
            Vector2 noteOffset = noteAngle.ToRotationVector2() * 24f;
            spriteBatch.Draw(sparkleTex, drawPos + noteOffset, null, EnigmaGreen * 0.6f * pulse, 
                noteAngle, sparkleTex.Size() / 2f, 0.2f, SpriteEffects.None, 0f);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }
        
        public override void OnSpawn(IEntitySource source)
        {
            orbitAngle = MathHelper.TwoPi * VoiceIndex / 5f;
            glyphIndex = Main.rand.Next(12);
        }
        
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            if (IsOrbiting)
            {
                // === ORBITING STATE ===
                Projectile.timeLeft = 600; // Don't expire while orbiting
                
                // Orbit around player
                orbitAngle += 0.06f + VoiceIndex * 0.008f;
                float targetRadius = 60f + VoiceIndex * 12f;
                orbitRadius = MathHelper.Lerp(orbitRadius, targetRadius, 0.1f);
                
                Vector2 orbitTarget = owner.Center + orbitAngle.ToRotationVector2() * orbitRadius;
                Projectile.Center = Vector2.Lerp(Projectile.Center, orbitTarget, 0.15f);
                Projectile.velocity = Vector2.Zero;
                
                // Ambient particles
                if (Main.GameUpdateCount % 8 == VoiceIndex)
                {
                    CustomParticles.GenericFlare(Projectile.Center, GetEnigmaGradient((float)VoiceIndex / 5f) * 0.6f, 0.3f, 12);
                }
                
                // Occasional music note
                if (Main.GameUpdateCount % 30 == VoiceIndex * 6)
                {
                    ThemedParticles.EnigmaMusicNotes(Projectile.Center, 1, 15f);
                }
            }
            else
            {
                // === RELEASED STATE ===
                releaseTimer++;
                
                // Initial spiral outward
                if (releaseTimer < 20)
                {
                    float spiralAngle = orbitAngle + releaseTimer * 0.2f;
                    float spiralRadius = orbitRadius + releaseTimer * 8f;
                    Vector2 spiralTarget = owner.Center + spiralAngle.ToRotationVector2() * spiralRadius;
                    Projectile.velocity = (spiralTarget - Projectile.Center) * 0.3f;
                }
                else
                {
                    // Home toward target
                    if (TargetIndex >= 0 && TargetIndex < Main.maxNPCs)
                    {
                        NPC target = Main.npc[TargetIndex];
                        if (target.active && !target.friendly)
                        {
                            Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                            float speed = 16f + releaseTimer * 0.1f;
                            Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * speed, 0.12f);
                        }
                    }
                    else
                    {
                        // No target - maintain velocity with slight homing to mouse
                        Vector2 toMouse = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.Zero);
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, toMouse * 14f, 0.03f);
                    }
                }
                
                // === IRIDESCENT WINGSPAN-STYLE RADIANT TRAIL EFFECTS ===
                // Heavy dust trails (2+ per frame) - void fugue stream
                for (int d = 0; d < 2; d++)
                {
                    Vector2 dustOffset = Main.rand.NextVector2Circular(8f, 8f);
                    Dust dustPurple = Dust.NewDustPerfect(Projectile.Center + dustOffset, DustID.PurpleTorch, 
                        -Projectile.velocity * 0.25f + Main.rand.NextVector2Circular(1f, 1f), 0, EnigmaPurple, 1.2f);
                    dustPurple.noGravity = true;
                    dustPurple.fadeIn = 1.4f;
                    
                    Dust dustGreen = Dust.NewDustPerfect(Projectile.Center + dustOffset * 0.6f, DustID.CursedTorch, 
                        -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(0.6f, 0.6f), 0, EnigmaGreen, 1.0f);
                    dustGreen.noGravity = true;
                    dustGreen.fadeIn = 1.3f;
                }
                
                // Contrasting sparkles (1-in-2) - fugue shimmer
                if (Main.rand.NextBool(2))
                {
                    Vector2 sparkleOffset = Main.rand.NextVector2Circular(12f, 12f);
                    var sparkle = new SparkleParticle(Projectile.Center + sparkleOffset, 
                        -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f), 
                        EnigmaGreen, 0.4f, 18);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
                
                // Enigma shimmer trails (1-in-3) - void hue cycling
                if (Main.rand.NextBool(3))
                {
                    float hue = Main.rand.NextFloat(0.28f, 0.45f); // Purple-green void range
                    Color shimmerColor = Main.hslToRgb(hue, 0.85f, 0.65f);
                    var shimmer = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.15f, 
                        shimmerColor, 0.32f, 20, true);
                    MagnumParticleHandler.SpawnParticle(shimmer);
                }
                
                // Pearlescent void effect (1-in-4)
                if (Main.rand.NextBool(4))
                {
                    float shift = (float)Math.Sin(Main.GameUpdateCount * 0.1f + Projectile.whoAmI) * 0.5f + 0.5f;
                    Color pearlColor = Color.Lerp(EnigmaPurple, EnigmaGreen, shift) * 0.75f;
                    CustomParticles.GenericFlare(Projectile.Center, pearlColor, 0.35f, 14);
                }
                
                // Frequent flares (1-in-2) - arcane radiance
                if (Main.rand.NextBool(2))
                {
                    Vector2 flareOffset = Main.rand.NextVector2Circular(6f, 6f);
                    CustomParticles.GenericFlare(Projectile.Center + flareOffset, 
                        GetEnigmaGradient((float)VoiceIndex / 5f), 0.3f, 12);
                }
                
                // Music note trail (1-in-6) - the voice sings
                if (Main.rand.NextBool(6))
                {
                    Color noteColor = Color.Lerp(EnigmaPurple, EnigmaGreen, Main.rand.NextFloat());
                    Vector2 noteVel = -Projectile.velocity * 0.05f + new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -0.8f);
                    ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.85f, 30);
                }
                
                // Timeout after long flight
                if (releaseTimer > 300)
                {
                    Projectile.Kill();
                }
            }
            
            Projectile.rotation += 0.1f;
            Lighting.AddLight(Projectile.Center, GetEnigmaGradient((float)VoiceIndex / 5f).ToVector3() * 0.4f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Echo Mark
            target.AddBuff(ModContent.BuffType<EchoMark>(), 300);
            var echoNPC = target.GetGlobalNPC<EchoMarkNPC>();
            echoNPC.AddEchoStack(target);
            
            // Also apply Paradox Brand
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 240);
            target.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(target, 1);
            
            // Impact VFX
            UnifiedVFX.EnigmaVariations.HitEffect(target.Center, 1.2f);
            
            // Glyph circle on impact
            CustomParticles.GlyphCircle(target.Center, EnigmaPurple, 6, 40f, 0.06f);
            
            // Music note burst - the voice sings its final note
            ThemedParticles.EnigmaMusicNoteBurst(target.Center, 8, 5f);
            
            // Eye watching the impact
            CustomParticles.EnigmaEyeImpact(target.Center, target.Center, EnigmaGreen, 0.5f);
            
            // Show echo stacks
            int stacks = echoNPC.echoStacks;
            if (stacks > 0)
            {
                CustomParticles.GlyphStack(target.Center + new Vector2(0, -30f), EnigmaGreen, stacks, 0.35f);
            }
            
            // Check for Harmonic Convergence
            if (stacks >= 3)
            {
                TriggerHarmonicConvergence(target);
            }
            
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.8f);
        }
        
        private void TriggerHarmonicConvergence(NPC centralTarget)
        {
            Player owner = Main.player[Projectile.owner];
            
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.6f, Volume = 0.9f }, centralTarget.Center);
            SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.3f, Volume = 0.7f }, centralTarget.Center);
            
            // Reset echo stacks on this target
            centralTarget.GetGlobalNPC<EchoMarkNPC>().echoStacks = 0;
            
            // Find all other marked enemies
            List<NPC> markedEnemies = new List<NPC>();
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly || npc.whoAmI == centralTarget.whoAmI) continue;
                if (npc.HasBuff(ModContent.BuffType<EchoMark>()))
                {
                    markedEnemies.Add(npc);
                }
            }
            
            // === HARMONIC CONVERGENCE VFX ===
            // Central explosion
            CustomParticles.GenericFlare(centralTarget.Center, EnigmaGreen, 1.2f, 30);
            CustomParticles.GenericFlare(centralTarget.Center, EnigmaGreen, 1.0f, 28);
            
            // Massive glyph circle
            CustomParticles.GlyphCircle(centralTarget.Center, EnigmaPurple, 12, 80f, 0.1f);
            CustomParticles.GlyphBurst(centralTarget.Center, EnigmaGreen, 10, 8f);
            
            // Expanding halos
            for (int ring = 0; ring < 5; ring++)
            {
                Color ringColor = GetEnigmaGradient(ring / 5f);
                CustomParticles.HaloRing(centralTarget.Center, ringColor * 0.8f, 0.4f + ring * 0.2f, 20 + ring * 5);
            }
            
            // Eye formation watching outward
            CustomParticles.EnigmaEyeFormation(centralTarget.Center, EnigmaGreen, 5, 70f);
            
            // Music note cascade - the fugue reaches its climax!
            ThemedParticles.EnigmaMusicNoteBurst(centralTarget.Center, 16, 8f);
            
            // Spiral galaxy burst
            for (int arm = 0; arm < 6; arm++)
            {
                float armAngle = MathHelper.TwoPi * arm / 6f;
                for (int point = 0; point < 6; point++)
                {
                    float spiralAngle = armAngle + point * 0.35f;
                    float spiralRadius = 30f + point * 20f;
                    Vector2 spiralPos = centralTarget.Center + spiralAngle.ToRotationVector2() * spiralRadius;
                    CustomParticles.GenericFlare(spiralPos, GetEnigmaGradient((arm * 6 + point) / 36f), 0.5f, 22);
                }
            }
            
            // Deal convergence damage to central target
            int convergeDamage = Projectile.damage * 3;
            centralTarget.SimpleStrikeNPC(convergeDamage, 0, true, 15f);
            
            // Chain to all marked enemies
            foreach (NPC marked in markedEnemies)
            {
                float dist = Vector2.Distance(centralTarget.Center, marked.Center);
                if (dist > 600f) continue;
                
                // Draw chain lightning
                MagnumVFX.DrawFractalLightning(centralTarget.Center, marked.Center, EnigmaGreen, 14, 35f, 5, 0.5f);
                
                // Deal chain damage
                float falloff = 1f - (dist / 600f) * 0.4f;
                int chainDamage = (int)(convergeDamage * 0.6f * falloff);
                marked.SimpleStrikeNPC(chainDamage, 0, true, 10f);
                
                // Chain impact VFX
                CustomParticles.GenericFlare(marked.Center, EnigmaGreen, 0.7f, 20);
                CustomParticles.HaloRing(marked.Center, EnigmaPurple, 0.45f, 16);
                CustomParticles.GlyphImpact(marked.Center, EnigmaPurple, EnigmaGreen, 0.5f);
                
                // Reset their echo stacks too
                marked.GetGlobalNPC<EchoMarkNPC>().echoStacks = 0;
                
                // Add more paradox stacks
                marked.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(marked, 2);
            }
            
            // Reality distortion
            FateRealityDistortion.TriggerChromaticAberration(centralTarget.Center, 5f, 18);
        }
        
        public override void OnKill(int timeLeft)
        {
            // Death burst
            CustomParticles.GenericFlare(Projectile.Center, EnigmaPurple, 0.6f, 18);
            CustomParticles.GlyphBurst(Projectile.Center, EnigmaGreen, 4, 3f);
            
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * 4f;
                var glow = new GenericGlowParticle(Projectile.Center, vel, GetEnigmaGradient((float)i / 6f), 0.3f, 16, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }
    }
    
    /// <summary>
    /// Echo Mark debuff - Applied by Fugue voices, stacks up to trigger Harmonic Convergence
    /// </summary>
    public class EchoMark : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.Cursed;
        
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }
    
    public class EchoMarkNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        
        public int echoStacks = 0;
        private const int MaxEchoStacks = 5;
        
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        public void AddEchoStack(NPC npc)
        {
            echoStacks = Math.Min(echoStacks + 1, MaxEchoStacks);
        }
        
        public override void ResetEffects(NPC npc)
        {
            if (!npc.HasBuff(ModContent.BuffType<EchoMark>()))
            {
                echoStacks = 0;
            }
        }
        
        public override void PostAI(NPC npc)
        {
            if (echoStacks > 0 && npc.HasBuff(ModContent.BuffType<EchoMark>()))
            {
                // Visual indicator of echo stacks
                if (Main.GameUpdateCount % 20 == 0)
                {
                    float intensity = (float)echoStacks / MaxEchoStacks;
                    
                    // Orbiting glyphs representing stacks
                    for (int i = 0; i < echoStacks; i++)
                    {
                        float angle = Main.GameUpdateCount * 0.05f + MathHelper.TwoPi * i / echoStacks;
                        float radius = 25f + npc.width * 0.3f;
                        Vector2 glyphPos = npc.Center + angle.ToRotationVector2() * radius;
                        CustomParticles.Glyph(glyphPos, Color.Lerp(EnigmaPurple, EnigmaGreen, intensity) * 0.6f, 0.25f, i % 12);
                    }
                }
                
                // Pulsing aura at high stacks
                if (echoStacks >= 3 && Main.GameUpdateCount % 30 == 0)
                {
                    CustomParticles.HaloRing(npc.Center, EnigmaGreen * 0.4f, 0.25f, 12);
                }
            }
        }
    }
}
