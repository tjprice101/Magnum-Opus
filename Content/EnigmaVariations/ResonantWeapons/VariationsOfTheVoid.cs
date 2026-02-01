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
            // Swing trail when not channeling
            if (Main.rand.NextBool(3))
            {
                Vector2 pos = new Vector2(
                    Main.rand.Next(hitbox.Left, hitbox.Right),
                    Main.rand.Next(hitbox.Top, hitbox.Bottom));
                CustomParticles.GenericFlare(pos, GetEnigmaGradient(Main.rand.NextFloat()) * 0.7f, 0.35f, 12);
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
            
            if (Main.GameUpdateCount % 3 == 0)
            {
                for (int i = 0; i < segments; i++)
                {
                    float t = (float)i / segments;
                    Vector2 basePos = Vector2.Lerp(start, end, t);
                    
                    // Distortion wave
                    float distortionAmount = 6f * beamIntensity * (float)Math.Sin(Main.GameUpdateCount * 0.2f + t * 8f + beamIndex * 2f);
                    Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
                    Vector2 distortedPos = basePos + perpendicular * distortionAmount;
                    
                    Color beamColor = Color.Lerp(beamBaseColor, EnigmaGreen, t) * beamIntensity;
                    CustomParticles.GenericFlare(distortedPos, beamColor, 0.3f + beamIntensity * 0.2f, 10);
                    
                    // Glyphs along beam
                    if (i % 4 == beamIndex)
                    {
                        CustomParticles.Glyph(distortedPos, EnigmaPurple * beamIntensity * 0.6f, 0.2f, -1);
                    }
                }
            }
            
            // Beam end flare
            if (Main.GameUpdateCount % 4 == 0)
            {
                CustomParticles.GenericFlare(end, EnigmaGreen * beamIntensity, 0.5f, 12);
                
                if (isAligned)
                {
                    // Extra intensity when aligned
                    CustomParticles.GenericFlare(end, EnigmaGreen * 0.8f, 0.35f, 8);
                }
            }
            
            Lighting.AddLight(end, beamBaseColor.ToVector3() * beamIntensity * 0.5f);
        }
        
        private void DrawResonanceEffect(Vector2 center, Vector2 direction, float convergenceProgress)
        {
            // When beams are aligned, draw special resonance at the convergence point
            float resonanceDistance = (beamLengths[0] + beamLengths[1] + beamLengths[2]) / 3f;
            Vector2 resonancePoint = center + direction * resonanceDistance;
            
            float pulse = (float)Math.Sin(alignedTime * 0.1f) * 0.3f + 0.7f;
            
            if (Main.GameUpdateCount % 2 == 0)
            {
                // Central resonance flare
                CustomParticles.GenericFlare(resonancePoint, EnigmaGreen, 0.8f * pulse, 15);
                CustomParticles.GenericFlare(resonancePoint, EnigmaPurple * 0.8f, 0.5f * pulse, 12);
                
                // Expanding halo
                CustomParticles.HaloRing(resonancePoint, EnigmaPurple * 0.7f, 0.4f * pulse, 14);
                
                // Orbiting glyphs at resonance point
                for (int i = 0; i < 3; i++)
                {
                    float angle = alignedTime * 0.08f + MathHelper.TwoPi * i / 3f;
                    Vector2 glyphPos = resonancePoint + angle.ToRotationVector2() * 30f;
                    CustomParticles.Glyph(glyphPos, EnigmaGreen * 0.8f, 0.3f, -1);
                }
            }
            
            // Music notes burst periodically while aligned
            if (alignedTime % 30 == 0)
            {
                ThemedParticles.EnigmaMusicNotes(resonancePoint, 4, 40f);
            }
            
            // Eye appears at resonance point occasionally
            if (alignedTime % 45 == 0)
            {
                CustomParticles.EnigmaEyeGaze(resonancePoint, EnigmaPurple * 0.8f, 0.5f, direction);
            }
            
            Lighting.AddLight(resonancePoint, EnigmaGreen.ToVector3() * pulse);
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
            
            // Particle effects during explosion
            if (Main.GameUpdateCount % 2 == 0)
            {
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f + lifeProgress * 2f;
                    Vector2 pos = Projectile.Center + angle.ToRotationVector2() * currentRadius * 0.8f;
                    Color color = Color.Lerp(EnigmaPurple, EnigmaGreen, (float)i / 6f) * intensity;
                    CustomParticles.GenericFlare(pos, color, 0.5f * intensity, 10);
                }
            }
            
            // Music note trail - resonance explosion echoes with musical notes
            if (Main.rand.NextBool(4))
            {
                Color noteColor = Color.Lerp(EnigmaPurple, EnigmaGreen, Main.rand.NextFloat()) * intensity;
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-2f, 2f), -1.5f);
                ThemedParticles.MusicNote(Projectile.Center + Main.rand.NextVector2Circular(currentRadius * 0.5f, currentRadius * 0.5f), noteVel, noteColor, 0.4f, 30);
            }
            
            Lighting.AddLight(Projectile.Center, EnigmaGreen.ToVector3() * intensity);
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
