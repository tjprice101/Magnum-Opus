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
    /// VARIATIONS OF THE VOID - Enigma Melee Sword
    /// ============================================
    /// MECHANICS:
    /// - Normal melee swing
    /// - While channeling, 3 void beams spawn in a 45-degree cone
    /// - Beams slowly converge toward cursor over time
    /// - When beams fully align, a special resonance effect activates
    /// - Beams deal damage along their length like Cipher Nocturne
    /// </summary>
    public class VariationsOfTheVoid : ModItem
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
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
            Item.DamageType = DamageClass.Melee;
            Item.width = 60;
            Item.height = 60;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(gold: 18);
            Item.rare = ModContent.RarityType<EnigmaRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.channel = true;
            Item.noMelee = false; // Can still swing and hit
            Item.shoot = ModContent.ProjectileType<VoidConvergenceBeamSet>();
            Item.shootSpeed = 1f;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect1", "Channel to summon three void beams in a cone"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect2", "Beams slowly converge toward the cursor"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect3", "When aligned, the beams resonate with devastating power"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaLore", "'Three questions. One answer. The void.'") 
            { 
                OverrideColor = EnigmaPurple 
            });
        }
        
        public override void HoldItem(Player player)
        {
            // Rotate the weapon toward the cursor while channeling
            if (Main.myPlayer == player.whoAmI && player.channel)
            {
                Vector2 toCursor = Main.MouseWorld - player.Center;
                player.ChangeDir(toCursor.X > 0 ? 1 : -1);
            }
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Only spawn one beam set at a time
            int beamCount = player.ownedProjectileCounts[type];
            if (beamCount == 0)
            {
                Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            }
            return false;
        }
        
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            Vector2 hitCenter = hitbox.Center.ToVector2();
            
            // === SPECTACULAR SWING SYSTEM - ENDGAME TIER ===
            SpectacularMeleeSwing.OnSwing(player, hitbox, EnigmaPurple, EnigmaGreen, 
                SpectacularMeleeSwing.SwingTier.Endgame, SpectacularMeleeSwing.WeaponTheme.Enigma);
            
            // Subtle swing trail
            if (Main.rand.NextBool(8))
            {
                Vector2 pos = new Vector2(
                    Main.rand.Next(hitbox.Left, hitbox.Right),
                    Main.rand.Next(hitbox.Top, hitbox.Bottom));
                CustomParticles.GenericFlare(pos, GetEnigmaGradient(Main.rand.NextFloat()) * 0.6f, 0.3f, 10);
            }
        }
        
        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Melee hit effect
            CustomParticles.GenericFlare(target.Center, EnigmaGreen * 0.7f, 0.5f, 14);
            CustomParticles.HaloRing(target.Center, EnigmaPurple * 0.6f, 0.35f, 14);
            
            // Apply Paradox Brand on melee hits too
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 300);
            var brandNPC = target.GetGlobalNPC<ParadoxBrandNPC>();
            brandNPC.AddParadoxStack(target, 1);
            
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.6f);
            
            // Spawn seeking crystals on crit - Enigma power
            if (hit.Crit)
            {
                SeekingCrystalHelper.SpawnEnigmaCrystals(
                    player.GetSource_ItemUse(Item),
                    target.Center,
                    (Main.MouseWorld - target.Center).SafeNormalize(Vector2.UnitX) * 8f,
                    (int)(damageDone * 0.2f),
                    Item.knockBack * 0.4f,
                    player.whoAmI,
                    4);
            }
        }
    }
    
    /// <summary>
    /// The tri-beam projectile that manages the 3 converging void beams
    /// </summary>
    public class VoidConvergenceBeamSet : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private const float MaxBeamLength = 600f;
        private const float StartConeAngle = MathHelper.Pi / 4f; // 45 degrees total (22.5 each side)
        private const float ConvergenceTime = 180f; // 3 seconds to fully converge
        private const float AlignedThreshold = MathHelper.Pi / 36f; // ~5 degrees = "aligned"
        
        private int channelTime = 0;
        private float currentConeAngle = StartConeAngle;
        private bool isAligned = false;
        private int alignedTime = 0;
        
        // Per-beam properties
        private float[] beamLengths = new float[3];
        private Dictionary<int, int> targetHitTimes = new Dictionary<int, int>();
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override string Texture => "MagnumOpus/Assets/Particles/Glyphs6";
        
        public override bool PreDraw(ref Color lightColor) => false;
        
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 9999;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            
            for (int i = 0; i < 3; i++)
                beamLengths[i] = MaxBeamLength;
        }
        
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            // Check if still channeling
            if (!owner.channel || owner.dead || !owner.active)
            {
                TriggerBeamEnd();
                Projectile.Kill();
                return;
            }
            
            channelTime++;
            
            // Position at player
            Projectile.Center = owner.Center;
            
            // Aim center beam toward cursor
            Vector2 toMouse = (Main.MouseWorld - owner.Center).SafeNormalize(Vector2.UnitX);
            Projectile.velocity = toMouse;
            Projectile.rotation = toMouse.ToRotation();
            
            // Slowly converge the cone over time
            float convergenceProgress = Math.Min(1f, channelTime / ConvergenceTime);
            currentConeAngle = MathHelper.Lerp(StartConeAngle, 0f, convergenceProgress);
            
            // Check if beams are aligned
            bool wasAligned = isAligned;
            isAligned = currentConeAngle <= AlignedThreshold;
            
            if (isAligned)
            {
                alignedTime++;
                if (!wasAligned)
                {
                    // Just became aligned - play sound
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.5f, Volume = 0.8f }, owner.Center);
                }
            }
            
            // Calculate beam angles: center, left, right
            float baseAngle = toMouse.ToRotation();
            float[] beamAngles = new float[3]
            {
                baseAngle - currentConeAngle / 2f, // Left beam
                baseAngle,                          // Center beam
                baseAngle + currentConeAngle / 2f  // Right beam
            };
            
            // Raycast and draw each beam
            for (int beamIndex = 0; beamIndex < 3; beamIndex++)
            {
                Vector2 beamDir = beamAngles[beamIndex].ToRotationVector2();
                Vector2 beamStart = owner.Center;
                
                // Raycast for tiles
                beamLengths[beamIndex] = MaxBeamLength;
                for (int i = 0; i < (int)(MaxBeamLength / 16f); i++)
                {
                    Vector2 checkPos = beamStart + beamDir * (i * 16f);
                    Point tilePos = checkPos.ToTileCoordinates();
                    
                    if (WorldGen.InWorld(tilePos.X, tilePos.Y))
                    {
                        Tile tile = Main.tile[tilePos.X, tilePos.Y];
                        if (tile.HasTile && Main.tileSolid[tile.TileType])
                        {
                            beamLengths[beamIndex] = i * 16f;
                            break;
                        }
                    }
                }
                
                Vector2 beamEnd = beamStart + beamDir * beamLengths[beamIndex];
                
                // Draw the beam
                DrawVoidBeam(beamStart, beamEnd, beamIndex, convergenceProgress);
                
                // Deal damage along this beam
                DealBeamDamage(beamStart, beamEnd, beamDir, beamIndex);
            }
            
            // === ALIGNED RESONANCE EFFECT ===
            if (isAligned)
            {
                DrawResonanceEffect(owner.Center, toMouse, convergenceProgress);
            }
            
            // Beam sound
            if (channelTime % 20 == 0)
            {
                float pitch = isAligned ? 0.3f : -0.2f;
                SoundEngine.PlaySound(SoundID.Item15 with { Pitch = pitch, Volume = 0.35f }, owner.Center);
            }
            
            // Keep player facing
            owner.ChangeDir(toMouse.X > 0 ? 1 : -1);
            owner.heldProj = Projectile.whoAmI;
            owner.itemTime = 2;
            owner.itemAnimation = 2;
            
            Lighting.AddLight(owner.Center, EnigmaGreen.ToVector3() * 0.5f);
        }
        
        private void DrawVoidBeam(Vector2 start, Vector2 end, int beamIndex, float convergenceProgress)
        {
            Vector2 direction = (end - start).SafeNormalize(Vector2.Zero);
            float length = Vector2.Distance(start, end);
            int segments = (int)(length / 35f);
            
            float beamIntensity = 0.5f + convergenceProgress * 0.5f;
            if (isAligned)
                beamIntensity = 1f + (float)Math.Sin(alignedTime * 0.15f) * 0.2f;
            
            // Color varies by beam index
            Color beamBaseColor = beamIndex switch
            {
                0 => EnigmaPurple,    // Left = purple
                1 => EnigmaGreen,     // Center = green
                2 => EnigmaDeepPurple, // Right = deep purple
                _ => EnigmaPurple
            };
            
            // === CALAMITY-STANDARD VFX PATTERN - BEAM SEGMENTS ===
            for (int i = 0; i < segments; i++)
            {
                float t = (float)i / segments;
                Vector2 basePos = Vector2.Lerp(start, end, t);
                
                // Distortion wave
                float distortionAmount = 6f * beamIntensity * (float)Math.Sin(Main.GameUpdateCount * 0.2f + t * 8f + beamIndex * 2f);
                Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
                Vector2 distortedPos = basePos + perpendicular * distortionAmount;
                
                // Heavy dust trails along beam (2+ per segment)
                for (int d = 0; d < 2; d++)
                {
                    Vector2 dustOffset = Main.rand.NextVector2Circular(6f, 6f);
                    Dust dust = Dust.NewDustPerfect(distortedPos + dustOffset, DustID.PurpleTorch, 
                        -direction * Main.rand.NextFloat(1f, 3f), 0, beamBaseColor, 1.1f * beamIntensity);
                    dust.noGravity = true;
                    dust.fadeIn = 1.4f;
                }
                
                // Contrasting sparkles (1-in-2)
                if (Main.rand.NextBool(2))
                {
                    Color sparkleColor = beamIndex == 1 ? EnigmaPurple : EnigmaGreen;
                    var sparkle = new SparkleParticle(distortedPos, perpendicular * Main.rand.NextFloat(-1f, 1f), 
                        sparkleColor * beamIntensity, 0.4f, 18);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
                
                // Shimmer effect (1-in-3) - void spectrum hue cycling
                if (Main.rand.NextBool(3))
                {
                    float shimmerHue = 0.28f + Main.rand.NextFloat(0.17f) + t * 0.1f;
                    Color shimmerColor = Main.hslToRgb(shimmerHue, 0.9f, 0.65f) * beamIntensity;
                    var shimmer = new GenericGlowParticle(distortedPos, perpendicular * Main.rand.NextFloat(-0.5f, 0.5f),
                        shimmerColor, 0.25f, 15, true);
                    MagnumParticleHandler.SpawnParticle(shimmer);
                }
                
                // Pearlescent void effect (1-in-4)
                if (Main.rand.NextBool(4))
                {
                    float pearlShift = (float)Math.Sin(Main.GameUpdateCount * 0.08f + t * 3f) * 0.5f + 0.5f;
                    Color pearlColor = Color.Lerp(EnigmaPurple, EnigmaGreen, pearlShift) * 0.7f * beamIntensity;
                    var pearl = new GenericGlowParticle(distortedPos, Vector2.Zero, pearlColor, 0.3f, 12, true);
                    MagnumParticleHandler.SpawnParticle(pearl);
                }
                
                // Flares along beam (1-in-2)
                if (Main.rand.NextBool(2))
                {
                    Color beamColor = Color.Lerp(beamBaseColor, EnigmaGreen, t) * beamIntensity;
                    CustomParticles.GenericFlare(distortedPos, beamColor, 0.35f + beamIntensity * 0.2f, 10);
                }
                
                // Glyphs along beam (enhanced)
                if (i % 4 == beamIndex)
                {
                    CustomParticles.Glyph(distortedPos, EnigmaPurple * beamIntensity * 0.7f, 0.25f, -1);
                }
                
                // Music notes along beam (1-in-6) - VISIBLE SCALE
                if (Main.rand.NextBool(6))
                {
                    Color noteColor = Color.Lerp(beamBaseColor, EnigmaGreen, t) * beamIntensity;
                    Vector2 noteVel = perpendicular * Main.rand.NextFloat(-1f, 1f);
                    ThemedParticles.MusicNote(distortedPos, noteVel, noteColor, 0.85f + Main.rand.NextFloat(0.2f), 25);
                }
                
                // Pulsing light along beam
                float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.12f + t * 4f) * 0.15f;
                Lighting.AddLight(distortedPos, beamBaseColor.ToVector3() * beamIntensity * 0.4f * pulse);
            }
            
            // === BEAM END EFFECTS (ENHANCED) ===
            // Heavy dust burst at endpoint
            for (int d = 0; d < 3; d++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(4f, 4f);
                Dust dust = Dust.NewDustPerfect(end, DustID.CursedTorch, burstVel, 0, EnigmaGreen, 1.3f * beamIntensity);
                dust.noGravity = true;
                dust.fadeIn = 1.4f;
            }
            
            // Bright flare at end
            CustomParticles.GenericFlare(end, EnigmaGreen * beamIntensity, 0.6f, 12);
            CustomParticles.GenericFlare(end, beamBaseColor * beamIntensity * 0.7f, 0.45f, 10);
            
            if (isAligned)
            {
                // Extra aligned beam intensity
                CustomParticles.GenericFlare(end, EnigmaGreen * 0.9f, 0.5f, 8);
                
                // Sparkle burst at aligned beam end
                var sparkle = new SparkleParticle(end, Main.rand.NextVector2Circular(2f, 2f), 
                    EnigmaPurple, 0.5f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Pulsing light at beam end
            float endPulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f + beamIndex) * 0.2f;
            Lighting.AddLight(end, beamBaseColor.ToVector3() * beamIntensity * 0.6f * endPulse);
        }
        
        private void DrawResonanceEffect(Vector2 center, Vector2 direction, float convergenceProgress)
        {
            // When beams are aligned, draw special resonance at the convergence point
            float resonanceDistance = (beamLengths[0] + beamLengths[1] + beamLengths[2]) / 3f;
            Vector2 resonancePoint = center + direction * resonanceDistance;
            
            float pulse = (float)Math.Sin(alignedTime * 0.1f) * 0.3f + 0.7f;
            
            // === CALAMITY-STANDARD VFX PATTERN - RESONANCE CENTER ===
            
            // Heavy dust vortex at resonance point (2+ per frame)
            for (int d = 0; d < 3; d++)
            {
                float angle = alignedTime * 0.12f + MathHelper.TwoPi * d / 3f;
                Vector2 dustVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Dust dust = Dust.NewDustPerfect(resonancePoint + Main.rand.NextVector2Circular(20f, 20f), 
                    DustID.PurpleTorch, dustVel, 0, EnigmaPurple, 1.3f * pulse);
                dust.noGravity = true;
                dust.fadeIn = 1.4f;
            }
            
            // Contrasting sparkle burst (1-in-2)
            if (Main.rand.NextBool(2))
            {
                Color sparkleColor = Main.rand.NextBool() ? EnigmaGreen : EnigmaPurple;
                var sparkle = new SparkleParticle(resonancePoint + Main.rand.NextVector2Circular(25f, 25f),
                    Main.rand.NextVector2Circular(2f, 2f), sparkleColor, 0.55f * pulse, 22);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Shimmer void spectrum effect (1-in-3)
            if (Main.rand.NextBool(3))
            {
                float shimmerHue = 0.28f + Main.rand.NextFloat(0.17f);
                Color shimmerColor = Main.hslToRgb(shimmerHue, 0.95f, 0.7f) * pulse;
                Vector2 shimmerOffset = Main.rand.NextVector2Circular(30f, 30f);
                var shimmer = new GenericGlowParticle(resonancePoint + shimmerOffset, Vector2.Zero,
                    shimmerColor, 0.35f, 18, true);
                MagnumParticleHandler.SpawnParticle(shimmer);
            }
            
            // Pearlescent void effect (1-in-4)
            if (Main.rand.NextBool(4))
            {
                float pearlShift = (float)Math.Sin(alignedTime * 0.08f) * 0.5f + 0.5f;
                Color pearlColor = Color.Lerp(EnigmaPurple, EnigmaGreen, pearlShift) * 0.75f * pulse;
                var pearl = new GenericGlowParticle(resonancePoint + Main.rand.NextVector2Circular(15f, 15f),
                    Vector2.Zero, pearlColor, 0.4f, 15, true);
                MagnumParticleHandler.SpawnParticle(pearl);
            }
            
            // Central resonance flares (1-in-2)
            if (Main.rand.NextBool(2))
            {
                CustomParticles.GenericFlare(resonancePoint, EnigmaGreen, 0.85f * pulse, 15);
                CustomParticles.GenericFlare(resonancePoint, EnigmaPurple * 0.8f, 0.55f * pulse, 12);
            }
            
            // Expanding gradient halo rings
            if (Main.GameUpdateCount % 2 == 0)
            {
                for (int ring = 0; ring < 3; ring++)
                {
                    float ringProgress = (float)ring / 3f;
                    Color ringColor = Color.Lerp(EnigmaPurple, EnigmaGreen, ringProgress) * 0.6f * pulse;
                    CustomParticles.HaloRing(resonancePoint, ringColor, (0.3f + ring * 0.15f) * pulse, 12 + ring * 2);
                }
            }
            
            // Orbiting glyphs at resonance point (enhanced)
            for (int i = 0; i < 4; i++)
            {
                float angle = alignedTime * 0.08f + MathHelper.TwoPi * i / 4f;
                Vector2 glyphPos = resonancePoint + angle.ToRotationVector2() * 35f;
                CustomParticles.Glyph(glyphPos, EnigmaGreen * 0.85f, 0.35f, -1);
            }
            
            // Music notes - VISIBLE SCALE (1-in-6)
            if (Main.rand.NextBool(6))
            {
                Color noteColor = Color.Lerp(EnigmaPurple, EnigmaGreen, Main.rand.NextFloat()) * pulse;
                Vector2 noteVel = Main.rand.NextVector2Circular(2f, 2f);
                ThemedParticles.MusicNote(resonancePoint + Main.rand.NextVector2Circular(20f, 20f), noteVel, 
                    noteColor, 0.9f + Main.rand.NextFloat(0.25f), 30);
            }
            
            // Music notes burst periodically while aligned (enhanced)
            if (alignedTime % 25 == 0)
            {
                for (int n = 0; n < 5; n++)
                {
                    float noteAngle = MathHelper.TwoPi * n / 5f;
                    Vector2 noteVel = noteAngle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                    Color noteColor = Color.Lerp(EnigmaPurple, EnigmaGreen, (float)n / 5f);
                    ThemedParticles.MusicNote(resonancePoint, noteVel, noteColor, 0.95f, 35);
                }
            }
            
            // Eye appears at resonance point (enhanced)
            if (alignedTime % 40 == 0)
            {
                CustomParticles.EnigmaEyeGaze(resonancePoint, EnigmaPurple * 0.85f, 0.55f, direction);
            }
            
            // Pulsing light
            float lightPulse = 1f + (float)Math.Sin(alignedTime * 0.12f) * 0.2f;
            Lighting.AddLight(resonancePoint, EnigmaGreen.ToVector3() * pulse * lightPulse * 0.8f);
        }
        
        private void DealBeamDamage(Vector2 start, Vector2 end, Vector2 direction, int beamIndex)
        {
            float beamLength = beamLengths[beamIndex];
            float damageMultiplier = isAligned ? 2.5f : 1f; // Much more damage when aligned
            
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly) continue;
                
                // Check if NPC intersects with beam
                float distToLine = DistancePointToLine(npc.Center, start, end);
                if (distToLine > npc.width / 2f + 15f) continue;
                
                float projectionLength = Vector2.Dot(npc.Center - start, direction);
                if (projectionLength < 0 || projectionLength > beamLength) continue;
                
                // Track hit time for scaling damage
                int targetKey = npc.whoAmI * 10 + beamIndex;
                if (!targetHitTimes.ContainsKey(targetKey))
                    targetHitTimes[targetKey] = 0;
                targetHitTimes[targetKey]++;
                
                // Deal damage periodically
                if (channelTime % 10 == beamIndex * 3)
                {
                    float timeMultiplier = 1f + Math.Min(targetHitTimes[targetKey] / 40f, 1.5f);
                    int damage = (int)(Projectile.damage * damageMultiplier * timeMultiplier);
                    
                    npc.SimpleStrikeNPC(damage, 0, false, 0f, null, false, 0f, true);
                    npc.AddBuff(ModContent.BuffType<ParadoxBrand>(), 360);
                    var brandNPC = npc.GetGlobalNPC<ParadoxBrandNPC>();
                    brandNPC.AddParadoxStack(npc, isAligned ? 2 : 1);
                    
                    // Hit VFX
                    Vector2 hitPoint = start + direction * projectionLength;
                    CustomParticles.GenericFlare(hitPoint, EnigmaGreen, 0.45f, 10);
                    
                    if (isAligned)
                    {
                        // Extra aligned hit effect
                        CustomParticles.HaloRing(npc.Center, EnigmaPurple * 0.6f, 0.3f, 10);
                    }
                }
            }
        }
        
        private float DistancePointToLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            Vector2 line = lineEnd - lineStart;
            float lineLength = line.Length();
            if (lineLength < 0.001f) return Vector2.Distance(point, lineStart);
            
            Vector2 lineDir = line / lineLength;
            Vector2 toPoint = point - lineStart;
            float projection = Vector2.Dot(toPoint, lineDir);
            projection = MathHelper.Clamp(projection, 0f, lineLength);
            
            Vector2 closestPoint = lineStart + lineDir * projection;
            return Vector2.Distance(point, closestPoint);
        }
        
        private void TriggerBeamEnd()
        {
            if (channelTime < 20) return;
            
            Player owner = Main.player[Projectile.owner];
            Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            
            // If beams were aligned, trigger a big finale
            if (isAligned && alignedTime > 30)
            {
                float resonanceDistance = (beamLengths[0] + beamLengths[1] + beamLengths[2]) / 3f;
                Vector2 resonancePoint = owner.Center + direction * resonanceDistance;
                
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 1f }, resonancePoint);
                
                // Spawn powerful explosion
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), resonancePoint, Vector2.Zero,
                    ModContent.ProjectileType<VoidResonanceExplosion>(), (int)(Projectile.damage * 4f), 10f, Projectile.owner);
                
                // Big VFX
                CustomParticles.GenericFlare(resonancePoint, EnigmaGreen, 1.5f, 30);
                CustomParticles.GenericFlare(resonancePoint, EnigmaPurple, 1.2f, 28);
                
                for (int ring = 0; ring < 5; ring++)
                {
                    CustomParticles.HaloRing(resonancePoint, GetEnigmaGradient((float)ring / 5f), 0.5f + ring * 0.15f, 18 + ring * 3);
                }
                
                CustomParticles.GlyphBurst(resonancePoint, EnigmaPurple, 10, 6f);
                CustomParticles.EnigmaEyeExplosion(resonancePoint, EnigmaGreen, 6, 5f);
                ThemedParticles.EnigmaMusicNoteBurst(resonancePoint, 8, 8f);
            }
            else
            {
                // Normal beam end - small fizzle at each beam end
                SoundEngine.PlaySound(SoundID.Item10 with { Pitch = -0.2f, Volume = 0.5f }, owner.Center);
                
                float baseAngle = direction.ToRotation();
                float[] angles = { baseAngle - currentConeAngle / 2f, baseAngle, baseAngle + currentConeAngle / 2f };
                
                for (int i = 0; i < 3; i++)
                {
                    Vector2 beamEnd = owner.Center + angles[i].ToRotationVector2() * beamLengths[i];
                    CustomParticles.GenericFlare(beamEnd, EnigmaPurple * 0.6f, 0.4f, 15);
                    CustomParticles.Glyph(beamEnd, EnigmaGreen * 0.5f, 0.25f, -1);
                }
            }
        }
        
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => false;
    }
    
    /// <summary>
    /// Powerful explosion when beams are released while aligned
    /// </summary>
    public class VoidResonanceExplosion : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        public override string Texture => "MagnumOpus/Assets/Particles/StarBurst1";
        
        public override void SetDefaults()
        {
            Projectile.width = 200;
            Projectile.height = 200;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 20;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        
        public override void AI()
        {
            float lifeProgress = 1f - (Projectile.timeLeft / 20f);
            float intensity = 1f - lifeProgress;
            
            // Expanding damage radius
            float currentRadius = 100f + lifeProgress * 100f;
            Projectile.width = (int)(currentRadius * 2);
            Projectile.height = (int)(currentRadius * 2);
            Projectile.Center = Projectile.position + new Vector2(Projectile.width / 2f, Projectile.height / 2f);
            
            // === CALAMITY-STANDARD VFX PATTERN - VOID EXPLOSION ===
            
            // Heavy dust vortex (scaled to explosion size)
            int dustCount = (int)(3 + lifeProgress * 4);
            for (int d = 0; d < dustCount; d++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = currentRadius * Main.rand.NextFloat(0.5f, 1f);
                Vector2 dustPos = Projectile.Center + angle.ToRotationVector2() * dist;
                Vector2 dustVel = (angle + MathHelper.PiOver2).ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch, dustVel, 0, EnigmaPurple, 1.3f * intensity);
                dust.noGravity = true;
                dust.fadeIn = 1.4f;
            }
            
            // Additional cursed dust for green accent
            for (int d = 0; d < 2; d++)
            {
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(currentRadius * 0.7f, currentRadius * 0.7f);
                Vector2 dustVel = Main.rand.NextVector2Circular(3f, 3f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.CursedTorch, dustVel, 0, EnigmaGreen, 1.2f * intensity);
                dust.noGravity = true;
                dust.fadeIn = 1.4f;
            }
            
            // Contrasting sparkles (1-in-2)
            if (Main.rand.NextBool(2))
            {
                Color sparkleColor = Main.rand.NextBool() ? EnigmaGreen : EnigmaPurple;
                Vector2 sparklePos = Projectile.Center + Main.rand.NextVector2Circular(currentRadius * 0.8f, currentRadius * 0.8f);
                var sparkle = new SparkleParticle(sparklePos, Main.rand.NextVector2Circular(3f, 3f), 
                    sparkleColor * intensity, 0.5f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Shimmer void spectrum (1-in-3)
            if (Main.rand.NextBool(3))
            {
                float shimmerHue = 0.28f + Main.rand.NextFloat(0.17f);
                Color shimmerColor = Main.hslToRgb(shimmerHue, 0.95f, 0.7f) * intensity;
                Vector2 shimmerPos = Projectile.Center + Main.rand.NextVector2Circular(currentRadius * 0.6f, currentRadius * 0.6f);
                var shimmer = new GenericGlowParticle(shimmerPos, Main.rand.NextVector2Circular(1f, 1f),
                    shimmerColor, 0.35f, 15, true);
                MagnumParticleHandler.SpawnParticle(shimmer);
            }
            
            // Pearlescent void effect (1-in-4)
            if (Main.rand.NextBool(4))
            {
                float pearlShift = (float)Math.Sin(Main.GameUpdateCount * 0.1f + lifeProgress * 5f) * 0.5f + 0.5f;
                Color pearlColor = Color.Lerp(EnigmaPurple, EnigmaGreen, pearlShift) * 0.7f * intensity;
                Vector2 pearlPos = Projectile.Center + Main.rand.NextVector2Circular(currentRadius * 0.5f, currentRadius * 0.5f);
                var pearl = new GenericGlowParticle(pearlPos, Vector2.Zero, pearlColor, 0.4f, 14, true);
                MagnumParticleHandler.SpawnParticle(pearl);
            }
            
            // Expanding ring flares (enhanced)
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f + lifeProgress * 2f;
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * currentRadius * 0.85f;
                Color color = Color.Lerp(EnigmaPurple, EnigmaGreen, (float)i / 8f) * intensity;
                
                // Main flare
                if (Main.rand.NextBool(2))
                    CustomParticles.GenericFlare(pos, color, 0.55f * intensity, 12);
                
                // Glyph at edge
                if (i % 2 == 0)
                    CustomParticles.Glyph(pos, EnigmaPurple * 0.7f * intensity, 0.3f, -1);
            }
            
            // Central flares
            if (Main.rand.NextBool(2))
            {
                CustomParticles.GenericFlare(Projectile.Center, EnigmaGreen * intensity, 0.7f, 14);
                CustomParticles.GenericFlare(Projectile.Center, EnigmaPurple * 0.8f * intensity, 0.5f, 12);
            }
            
            // Gradient halo rings
            if (Main.GameUpdateCount % 3 == 0)
            {
                for (int ring = 0; ring < 3; ring++)
                {
                    float ringProgress = (float)ring / 3f;
                    Color ringColor = Color.Lerp(EnigmaPurple, EnigmaGreen, ringProgress) * 0.5f * intensity;
                    CustomParticles.HaloRing(Projectile.Center, ringColor, (0.4f + ring * 0.15f) * intensity, 14 + ring * 2);
                }
            }
            
            // Music notes - VISIBLE SCALE (1-in-4 for explosion intensity)
            if (Main.rand.NextBool(4))
            {
                Color noteColor = Color.Lerp(EnigmaPurple, EnigmaGreen, Main.rand.NextFloat()) * intensity;
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 1f));
                Vector2 notePos = Projectile.Center + Main.rand.NextVector2Circular(currentRadius * 0.6f, currentRadius * 0.6f);
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.9f + Main.rand.NextFloat(0.25f), 30);
            }
            
            // Enigma eyes watching outward
            if (Main.rand.NextBool(8))
            {
                float eyeAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 eyePos = Projectile.Center + eyeAngle.ToRotationVector2() * currentRadius * 0.7f;
                CustomParticles.EnigmaEyeGaze(eyePos, EnigmaPurple * 0.7f * intensity, 0.4f, eyeAngle.ToRotationVector2());
            }
            
            // Pulsing light
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.15f;
            Lighting.AddLight(Projectile.Center, EnigmaGreen.ToVector3() * intensity * pulse * 0.9f);
        }
        
        public override bool PreDraw(ref Color lightColor) => false;
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 480);
            var brandNPC = target.GetGlobalNPC<ParadoxBrandNPC>();
            brandNPC.AddParadoxStack(target, 3);
            
            CustomParticles.GenericFlare(target.Center, EnigmaGreen, 0.7f, 18);
            CustomParticles.HaloRing(target.Center, EnigmaPurple, 0.4f, 14);
            CustomParticles.EnigmaEyeImpact(target.Center, target.Center, EnigmaPurple * 0.7f, 0.4f);
        }
    }
}
