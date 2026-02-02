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
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.EnigmaVariations.Debuffs;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons
{
    /// <summary>
    /// CIPHER NOCTURNE - Magic beam weapon that channels mysterious arcane energy
    /// Creates visual distortion effects and increasing damage over beam duration
    /// When beam ends, all damage areas "snap back" with a burst
    /// </summary>
    public class CipherNocturne : ModItem
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
            Item.damage = 290;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 6;
            Item.width = 28;
            Item.height = 28;
            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 1f;
            Item.value = Item.sellPrice(gold: 20);
            Item.rare = ModContent.RarityType<EnigmaRarity>();
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<RealityUnravelerBeam>();
            Item.shootSpeed = 1f;
            Item.noMelee = true;
            Item.channel = true;
            Item.staff[Item.type] = true; // Makes it point forward like a staff
        }
        
        public override void HoldItem(Player player)
        {
            // Rotate the weapon toward the cursor while holding
            if (Main.myPlayer == player.whoAmI)
            {
                Vector2 toCursor = Main.MouseWorld - player.Center;
                player.itemRotation = toCursor.ToRotation();
                if (player.direction == -1)
                    player.itemRotation += MathHelper.Pi;
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect", "Hold to channel a mysterious beam of arcane energy"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect2", "The beam warps and distorts the space around it"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect3", "Damage increases the longer beam is held on target"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect4", "Releasing the beam causes all affected areas to snap back"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaLore", "'Pull at a thread of existence, and watch it come undone.'") 
            { 
                OverrideColor = EnigmaPurple 
            });
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Only spawn one beam
            int beamCount = player.ownedProjectileCounts[type];
            if (beamCount == 0)
            {
                Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
                
                // Subtle music notes on beam start
                ThemedParticles.EnigmaMusicNotes(position, 3, 30f);
            }
            return false;
        }
    }
    
    public class RealityUnravelerBeam : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreenFlame = new Color(50, 220, 100);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private const float MaxBeamLength = 800f;
        private float currentBeamLength = 0f;
        private float beamIntensity = 0f;
        private int channelTime = 0;
        private List<Vector2> unravelPoints = new List<Vector2>();
        private Dictionary<int, int> targetHitTimes = new Dictionary<int, int>();
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override string Texture => "MagnumOpus/Assets/Particles/Glyphs11";
        
        public override bool PreDraw(ref Color lightColor)
        {
            // Beam is invisible projectile - all visuals drawn via particles in AI
            return false;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 9999;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
        }
        
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            // Check if still channeling
            if (!owner.channel || owner.dead || !owner.active)
            {
                TriggerSnapBack();
                Projectile.Kill();
                return;
            }
            
            // Drain mana
            if (channelTime % 10 == 0 && channelTime > 0)
            {
                if (!owner.CheckMana(3, true, false))
                {
                    TriggerSnapBack();
                    Projectile.Kill();
                    return;
                }
            }
            
            channelTime++;
            beamIntensity = Math.Min(1f, channelTime / 60f);
            
            // Position at player
            Projectile.Center = owner.Center;
            
            // Aim towards cursor - beam stretches TO cursor position, not past it
            Vector2 toMouse = (Main.MouseWorld - owner.Center).SafeNormalize(Vector2.UnitX);
            float distanceToCursor = Vector2.Distance(Main.MouseWorld, owner.Center);
            Projectile.velocity = toMouse;
            Projectile.rotation = toMouse.ToRotation();
            
            // Beam length is the distance to cursor (capped at MaxBeamLength)
            // This makes the beam stretch exactly to where cursor is
            currentBeamLength = Math.Min(distanceToCursor, MaxBeamLength);
            Vector2 beamStart = owner.Center;
            Vector2 beamEnd = beamStart + toMouse * currentBeamLength;
            
            // Check for tiles - stop beam if it hits a tile before reaching cursor
            for (int i = 0; i < (int)(currentBeamLength / 16f); i++)
            {
                Vector2 checkPos = beamStart + toMouse * (i * 16f);
                Point tilePos = checkPos.ToTileCoordinates();
                
                if (WorldGen.InWorld(tilePos.X, tilePos.Y))
                {
                    Tile tile = Main.tile[tilePos.X, tilePos.Y];
                    if (tile.HasTile && Main.tileSolid[tile.TileType])
                    {
                        currentBeamLength = i * 16f;
                        break;
                    }
                }
            }
            
            beamEnd = beamStart + toMouse * currentBeamLength;
            
            // Draw the beam with reality distortion
            DrawUnravelingBeam(beamStart, beamEnd);
            
            // Store unravel points for snap-back
            if (channelTime % 5 == 0)
            {
                unravelPoints.Add(beamEnd);
                if (unravelPoints.Count > 30)
                    unravelPoints.RemoveAt(0);
            }
            
            // Deal damage along beam
            DealBeamDamage(beamStart, beamEnd, toMouse);
            
            // Beam sound
            if (channelTime % 15 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item15 with { Pitch = -0.3f + beamIntensity * 0.3f, Volume = 0.4f }, Projectile.Center);
            }
            
            // Keep player facing the beam
            owner.ChangeDir(toMouse.X > 0 ? 1 : -1);
            owner.heldProj = Projectile.whoAmI;
            owner.itemTime = 2;
            owner.itemAnimation = 2;
            
            Lighting.AddLight(owner.Center, EnigmaGreen.ToVector3() * beamIntensity);
        }
        
        private void DrawUnravelingBeam(Vector2 start, Vector2 end)
        {
            Vector2 direction = (end - start).SafeNormalize(Vector2.Zero);
            float length = Vector2.Distance(start, end);
            // OPTIMIZED: Reduced segment count from length/12 to length/40 (roughly 3x fewer segments)
            int segments = (int)(length / 40f);
            
            // Only process segments every 4 frames instead of 2
            if (Main.GameUpdateCount % 4 == 0)
            {
                for (int i = 0; i < segments; i++)
                {
                    float t = (float)i / segments;
                    Vector2 basePos = Vector2.Lerp(start, end, t);
                    
                    // Reality distortion - offset positions randomly and progressively
                    float distortionAmount = 8f * beamIntensity * (float)Math.Sin(Main.GameUpdateCount * 0.2f + t * 10f);
                    Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
                    Vector2 distortedPos = basePos + perpendicular * distortionAmount;
                    
                    // Gradient color along beam
                    Color beamColor = GetEnigmaGradient(t) * beamIntensity;
                    
                    // Main beam particles - now spawns for all segments but less frequently
                    CustomParticles.GenericFlare(distortedPos, beamColor, 0.35f + beamIntensity * 0.3f, 12);
                    
                    // Glyphs scattered along beam - every 3rd segment instead of every 8th
                    if (i % 3 == 0)
                    {
                        CustomParticles.Glyph(distortedPos, EnigmaPurple * beamIntensity, 0.22f, -1);
                    }
                }
            }
            
            // === IRIDESCENT WINGSPAN STYLE - HEAVY DUST TRAILS (2+ per frame at beam end) ===
            for (int i = 0; i < 2; i++)
            {
                // Main enigma trail - Purple to Green gradient
                float progress = Main.rand.NextFloat();
                Color dustColor = GetEnigmaGradient(progress);
                int dustType = progress < 0.5f ? DustID.PurpleTorch : DustID.GreenTorch;
                Dust d = Dust.NewDustPerfect(end + Main.rand.NextVector2Circular(8f, 8f), dustType,
                    Main.rand.NextVector2Circular(3f, 3f),
                    progress < 0.3f ? 80 : 0, dustColor * beamIntensity, 1.6f);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }
            
            // === CONTRASTING SPARKLES - Green flame against void black ===
            if (Main.rand.NextBool(2))
            {
                Dust green = Dust.NewDustPerfect(end + Main.rand.NextVector2Circular(10f, 10f), DustID.GreenTorch,
                    Main.rand.NextVector2Circular(2f, 2f),
                    0, EnigmaGreenFlame * beamIntensity, 1.4f);
                green.noGravity = true;
            }
            
            // === VOID SHIMMER TRAIL - Cycling through enigma colors ===
            if (Main.rand.NextBool(3))
            {
                // Cycle through void colors: purple -> deep purple -> green
                float voidPhase = (Main.GameUpdateCount * 0.015f + Main.rand.NextFloat()) % 1f;
                Color voidShimmer = GetEnigmaGradient(voidPhase);
                Dust v = Dust.NewDustPerfect(end, DustID.PurpleTorch,
                    Main.rand.NextVector2Circular(2f, 2f), 0, voidShimmer * beamIntensity, 1.4f);
                v.noGravity = true;
            }
            
            // === FREQUENT FLARES at beam end ===
            if (Main.rand.NextBool(2))
            {
                Color flareColor = GetEnigmaGradient(Main.rand.NextFloat());
                CustomParticles.GenericFlare(end + Main.rand.NextVector2Circular(12f, 12f), flareColor * beamIntensity, 0.5f, 16);
            }
            
            // === ENIGMA EYE watching ===
            if (Main.rand.NextBool(8))
            {
                CustomParticles.EnigmaEyeGaze(end + Main.rand.NextVector2Circular(20f, 20f), EnigmaPurple * beamIntensity, 0.35f);
            }
            
            // === MUSIC NOTES - The enigma's riddle ===
            if (Main.rand.NextBool(6))
            {
                Color noteColor = GetEnigmaGradient(Main.rand.NextFloat());
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f);
                ThemedParticles.MusicNote(end, noteVel, noteColor * beamIntensity, 0.35f, 35);
            }
            
            // End point unraveling effect - reduced from 6 to 4 particles, every 6 frames
            if (Main.GameUpdateCount % 6 == 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    float angle = MathHelper.TwoPi * i / 4f + Main.GameUpdateCount * 0.15f;
                    float radius = 20f + (float)Math.Sin(Main.GameUpdateCount * 0.2f + i) * 15f;
                    Vector2 endOffset = angle.ToRotationVector2() * radius * beamIntensity;
                    
                    Color endColor = GetEnigmaGradient((float)i / 4f) * beamIntensity;
                    CustomParticles.GenericFlare(end + endOffset, endColor, 0.4f, 14);
                }
            }
            
            // Dazzling sparkles along the beam's path - kept but less frequent
            if (Main.GameUpdateCount % 30 == 0)
            {
                Vector2 sparklePos = Vector2.Lerp(start, end, Main.rand.NextFloat(0.3f, 0.8f));
                sparklePos += Main.rand.NextVector2Circular(20f, 20f);
                Color sparkleColor = GetEnigmaGradient(Main.rand.NextFloat()) * beamIntensity;
                CustomParticles.GenericFlare(sparklePos, sparkleColor, 0.55f, 20);
            }
            
            // Beam origin VFX - ENHANCED MULTI-LAYER BLOOM
            if (Main.GameUpdateCount % 8 == 0)
            {
                EnhancedParticles.BloomFlare(start, EnigmaGreen * beamIntensity, 0.55f, 14, 3, 0.9f);
            }
            
            // Beam end VFX - ENHANCED WITH BLOOM
            if (Main.GameUpdateCount % 8 == 0)
            {
                EnhancedParticles.BloomFlare(end, EnigmaGreen * beamIntensity, 0.65f, 16, 4, 1.0f);
                EnhancedThemedParticles.EnigmaBloomBurstEnhanced(end, beamIntensity * 0.5f);
            }
            
            Lighting.AddLight(end, GetEnigmaGradient(0.7f).ToVector3() * beamIntensity * 0.8f);
        }
        
        private void DealBeamDamage(Vector2 start, Vector2 end, Vector2 direction)
        {
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly) continue;
                
                // Check if NPC intersects with beam
                float distToLine = DistancePointToLine(npc.Center, start, end);
                if (distToLine > npc.width / 2f + 20f) continue;
                
                // Also check if within beam length
                float projectionLength = Vector2.Dot(npc.Center - start, direction);
                if (projectionLength < 0 || projectionLength > currentBeamLength) continue;
                
                // Track hit time for increasing damage
                if (!targetHitTimes.ContainsKey(npc.whoAmI))
                    targetHitTimes[npc.whoAmI] = 0;
                targetHitTimes[npc.whoAmI]++;
                
                // Deal damage - increases over time on same target
                if (channelTime % 8 == 0)
                {
                    float damageMultiplier = 1f + Math.Min(targetHitTimes[npc.whoAmI] / 30f, 2f); // Up to 3x damage
                    int damage = (int)(Projectile.damage * damageMultiplier);
                    
                    npc.SimpleStrikeNPC(damage, 0, false, 0f, null, false, 0f, true);
                    npc.AddBuff(ModContent.BuffType<ParadoxBrand>(), 360);
                    var brandNPC = npc.GetGlobalNPC<ParadoxBrandNPC>();
                    brandNPC.AddParadoxStack(npc, 1);
                    
                    // Hit VFX
                    Vector2 hitPoint = start + direction * projectionLength;
                    CustomParticles.GenericFlare(hitPoint, EnigmaGreen, 0.5f * beamIntensity, 12);
                    
                    // Prismatic sparkle burst at impact
                    if (Main.rand.NextBool(3))
                    {
                        for (int sparkle = 0; sparkle < 4; sparkle++)
                        {
                            float sparkAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                            Vector2 sparkVel = sparkAngle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 3f);
                            Color sparkColor = GetEnigmaGradient(Main.rand.NextFloat());
                            var sparkGlow = new GenericGlowParticle(npc.Center - new Vector2(0, 25f), sparkVel, sparkColor, 0.3f, 18, true);
                            MagnumParticleHandler.SpawnParticle(sparkGlow);
                        }
                    }
                    
                    // Glyph stack
                    int stacks = brandNPC.paradoxStacks;
                    if (stacks > 0 && Main.rand.NextBool(5))
                    {
                        CustomParticles.GlyphStack(npc.Center + new Vector2(0, -20f), EnigmaGreen, stacks, 0.18f);
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
        
        private void TriggerSnapBack()
        {
            if (channelTime < 15) return; // Must have channeled for a bit
            
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.4f, Volume = 0.9f }, Projectile.Center);
            
            // Snap-back at all unravel points
            foreach (Vector2 point in unravelPoints)
            {
                // Spawn snap-back explosion
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), point, Vector2.Zero,
                    ModContent.ProjectileType<RealitySnapBack>(), (int)(Projectile.damage * 0.8f * beamIntensity), 
                    5f, Projectile.owner);
            }
            
            // Extra snap-back at beam end position
            Player owner = Main.player[Projectile.owner];
            Vector2 beamDir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 beamEnd = owner.Center + beamDir * currentBeamLength;
            
            // Massive final snap
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), beamEnd, Vector2.Zero,
                ModContent.ProjectileType<RealitySnapBack>(), (int)(Projectile.damage * 1.5f * beamIntensity), 
                8f, Projectile.owner, 1f); // ai[0] = 1 for enhanced version
            
            // Arcane sparkle formation at snap
            for (int spark = 0; spark < 8; spark++)
            {
                float sparkAngle = MathHelper.TwoPi * spark / 8f;
                Vector2 sparkPos = beamEnd + sparkAngle.ToRotationVector2() * 80f;
                Vector2 sparkVel = (beamEnd - sparkPos).SafeNormalize(Vector2.Zero) * 2f;
                Color sparkColor = GetEnigmaGradient((float)spark / 8f);
                CustomParticles.GenericFlare(sparkPos, sparkColor, 0.5f, 20);
                var sparkGlow = new GenericGlowParticle(sparkPos, sparkVel, sparkColor * 0.7f, 0.35f, 22, true);
                MagnumParticleHandler.SpawnParticle(sparkGlow);
            }
            
            // Glyph circles at snap
            CustomParticles.GlyphCircle(beamEnd, EnigmaPurple, count: 10, radius: 60f, rotationSpeed: 0.1f);
            CustomParticles.GlyphBurst(beamEnd, EnigmaGreen, count: 12, speed: 6f);
        }
        
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => false;
    }
    
    public class RealitySnapBack : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreenFlame = new Color(50, 220, 100);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private bool IsEnhanced => Projectile.ai[0] > 0f;
        private float ExplosionRadius => IsEnhanced ? 120f : 70f;
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override string Texture => "MagnumOpus/Assets/Particles/Glyphs12";
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            float lifeProgress = 1f - (Projectile.timeLeft / 25f);
            float intensity = 1f - lifeProgress;
            float scale = IsEnhanced ? 1.5f : 1f;
            float pulse = lifeProgress < 0.4f ? (1f - lifeProgress / 0.4f) : lifeProgress;
            
            // Switch to additive blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Texture2D eyeTex = CustomParticleSystem.EnigmaEyes[(int)(lifeProgress * 7) % 8].Value;
            Texture2D glyphTex = CustomParticleSystem.Glyphs[(int)(Main.GameUpdateCount / 8) % 12].Value;
            Texture2D sparkleTex = CustomParticleSystem.PrismaticSparkles[(int)(Main.GameUpdateCount / 6) % 8].Value;
            Texture2D flareTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2").Value;
            
            // Draw imploding/exploding glyph ring
            int glyphCount = lifeProgress < 0.4f ? 8 : 12;
            float ringRadius = lifeProgress < 0.4f ? (40f * (1f - lifeProgress / 0.4f)) : (15f + lifeProgress * 60f);
            for (int i = 0; i < glyphCount; i++)
            {
                float angle = Main.GameUpdateCount * (lifeProgress < 0.4f ? 0.15f : -0.08f) + MathHelper.TwoPi * i / glyphCount;
                Vector2 glyphPos = drawPos + angle.ToRotationVector2() * ringRadius * scale;
                Color glyphColor = Color.Lerp(EnigmaDeepPurple, EnigmaGreenFlame, (float)i / glyphCount) * intensity * 0.7f;
                float glyphScale = 0.2f * scale * pulse;
                spriteBatch.Draw(glyphTex, glyphPos, null, glyphColor, angle * 2f + lifeProgress * 5f, glyphTex.Size() / 2f, glyphScale, SpriteEffects.None, 0f);
            }
            
            // Draw orbiting sparkles
            for (int i = 0; i < 6; i++)
            {
                float angle = -Main.GameUpdateCount * 0.12f + MathHelper.TwoPi * i / 6f;
                float sparkRadius = ringRadius * 0.6f;
                Vector2 sparkPos = drawPos + angle.ToRotationVector2() * sparkRadius * scale;
                Color sparkColor = Color.Lerp(EnigmaPurple, EnigmaGreenFlame, (float)i / 6f) * intensity * 0.6f;
                spriteBatch.Draw(sparkleTex, sparkPos, null, sparkColor, angle * 1.5f, sparkleTex.Size() / 2f, 0.12f * scale * pulse, SpriteEffects.None, 0f);
            }
            
            // Draw central watching eye during implosion phase
            if (lifeProgress < 0.5f)
            {
                float eyeScale = 0.4f * scale * intensity * (lifeProgress < 0.4f ? 1f : (1f - (lifeProgress - 0.4f) * 10f));
                spriteBatch.Draw(eyeTex, drawPos, null, EnigmaPurple * intensity * 0.9f, 0f, eyeTex.Size() / 2f, eyeScale, SpriteEffects.None, 0f);
            }
            
            // Inner flare core
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaDeepPurple * intensity * 0.8f, Main.GameUpdateCount * 0.05f, flareTex.Size() / 2f, 0.35f * scale * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaGreenFlame * intensity * 0.6f, -Main.GameUpdateCount * 0.07f, flareTex.Size() / 2f, 0.18f * scale * pulse, SpriteEffects.None, 0f);
            
            // Restore normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 70;
            Projectile.height = 70;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 25;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 5;
        }
        
        public override void AI()
        {
            float lifeProgress = 1f - (Projectile.timeLeft / 25f);
            float intensity = 1f - lifeProgress;
            float scale = IsEnhanced ? 1.5f : 1f;
            
            // OPTIMIZED: Changed from % 2 to % 4 for less frequent particle spawns
            // Implosion then explosion effect
            float effectProgress;
            if (lifeProgress < 0.4f)
            {
                // Implosion phase - reduced from 8 to 5 particles
                effectProgress = lifeProgress / 0.4f;
                float implodeRadius = ExplosionRadius * (1f - effectProgress) * scale;
                
                if (Main.GameUpdateCount % 4 == 0)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 5f + Main.GameUpdateCount * 0.2f;
                        Vector2 particlePos = Projectile.Center + angle.ToRotationVector2() * implodeRadius;
                        Vector2 vel = (Projectile.Center - particlePos).SafeNormalize(Vector2.Zero) * 6f;
                        
                        Color particleColor = GetEnigmaGradient((float)i / 5f) * intensity;
                        var glow = new GenericGlowParticle(particlePos, vel, particleColor, 0.4f * scale, 14, true);
                        MagnumParticleHandler.SpawnParticle(glow);
                    }
                }
            }
            else
            {
                // Explosion phase - reduced from 12 to 6 particles
                effectProgress = (lifeProgress - 0.4f) / 0.6f;
                float explodeRadius = ExplosionRadius * effectProgress * scale;
                
                if (Main.GameUpdateCount % 4 == 0)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 6f;
                        Vector2 particlePos = Projectile.Center + angle.ToRotationVector2() * explodeRadius;
                        
                        Color particleColor = GetEnigmaGradient((float)i / 6f) * intensity;
                        CustomParticles.GenericFlare(particlePos, particleColor, 0.45f * scale * intensity, 12);
                    }
                }
            }
            
            // Central pulse - reduced frequency
            if (Main.GameUpdateCount % 6 == 0)
            {
                CustomParticles.GenericFlare(Projectile.Center, EnigmaGreen * intensity, 0.65f * scale * intensity, 12);
            }
            
            // Music note trail - reality snap echoes with musical notes
            if (Main.rand.NextBool(5))
            {
                Color noteColor = Color.Lerp(EnigmaPurple, EnigmaGreenFlame, Main.rand.NextFloat()) * intensity;
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-1f, 1f), -1.5f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.35f * scale, 32);
            }
            
            // Mystical flare at center during snap
            if (Projectile.timeLeft == 15)
            {
                CustomParticles.GenericFlare(Projectile.Center, EnigmaPurple, 0.75f * scale, 20);
                CustomParticles.HaloRing(Projectile.Center, EnigmaGreen, 0.45f * scale, 16);
            }
            
            Lighting.AddLight(Projectile.Center, GetEnigmaGradient(0.5f).ToVector3() * intensity * scale);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 420);
            var brandNPC = target.GetGlobalNPC<ParadoxBrandNPC>();
            brandNPC.AddParadoxStack(target, IsEnhanced ? 3 : 2);
            
            // === SEEKING CRYSTALS - 25% chance on beam hit ===
            if (Main.rand.NextBool(4))
            {
                SeekingCrystalHelper.SpawnEnigmaCrystals(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Projectile.velocity,
                    (int)(damageDone * 0.15f),
                    Projectile.knockBack,
                    Projectile.owner,
                    IsEnhanced ? 4 : 3
                );
            }
            
            // === REALITY UNRAVELING DISTORTION ===
            float distortionIntensity = IsEnhanced ? 5f : 3.5f;
            int distortionLifetime = IsEnhanced ? 18 : 12;
            FateRealityDistortion.TriggerChromaticAberration(target.Center, distortionIntensity, distortionLifetime);
            if (IsEnhanced)
                FateRealityDistortion.TriggerInversionPulse(6);
            
            // === ENHANCED HIT EFFECT WITH MULTI-LAYER BLOOM ===
            UnifiedVFXBloom.EnigmaVariations.ImpactEnhanced(target.Center, IsEnhanced ? 1.2f : 0.9f);
            
            // === WATCHING EYE AT IMPACT ===
            CustomParticles.EnigmaEyeImpact(target.Center, target.Center, EnigmaGreen, 0.45f);
            
            // === MUSIC NOTES ===
            EnhancedThemedParticles.EnigmaMusicNotesEnhanced(target.Center, 4, 5f);
            
            // Central bloom flare
            EnhancedParticles.BloomFlare(target.Center, EnigmaGreen, 0.55f, 14, 3, 0.9f);
            
            // === GLYPH CIRCLE ===
            CustomParticles.GlyphCircle(target.Center, EnigmaPurple, count: 4, radius: 40f, rotationSpeed: 0.06f);
            
            // === DYNAMIC LIGHTING ===
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.8f);
        }
        
        public override void OnKill(int timeLeft)
        {
            float scale = IsEnhanced ? 1.5f : 1f;
            
            // === BEAM COLLAPSE REALITY WARP ===
            float collapseIntensity = IsEnhanced ? 5f : 3.5f;
            int collapseLifetime = IsEnhanced ? 18 : 12;
            FateRealityDistortion.TriggerChromaticAberration(Projectile.Center, collapseIntensity, collapseLifetime);
            if (IsEnhanced)
                FateRealityDistortion.TriggerInversionPulse(6);
            
            // === ENHANCED BLOOM BURST ===
            EnhancedThemedParticles.EnigmaBloomBurstEnhanced(Projectile.Center, scale);
            
            // Final burst with enhanced particles
            for (int i = 0; i < (IsEnhanced ? 10 : 6); i++)
            {
                float angle = MathHelper.TwoPi * i / (IsEnhanced ? 10 : 6);
                Vector2 vel = angle.ToRotationVector2() * (4f * scale);
                Color burstColor = GetEnigmaGradient((float)i / (IsEnhanced ? 10 : 6));
                
                // Use EnhancedParticlePool for bloom
                var particle = EnhancedParticlePool.GetParticle()
                    .Setup(Projectile.Center, vel, burstColor, 0.4f * scale, 18)
                    .WithBloom(2, 0.8f)
                    .WithDrag(0.96f);
                EnhancedParticlePool.SpawnParticle(particle);
            }
            
            // Enhanced halo with bloom
            EnhancedParticles.BloomFlare(Projectile.Center, EnigmaPurple, 0.5f * scale, 15, 3, 0.9f);
            
            if (IsEnhanced)
            {
                CustomParticles.GlyphBurst(Projectile.Center, EnigmaGreen, count: 6, speed: 3f);
                
                // === WATCHING EYES ===
                CustomParticles.EnigmaEyeExplosion(Projectile.Center, EnigmaPurple, 6, 4f);
            }
            else
            {
                // === WATCHING EYE at beam end ===
                CustomParticles.EnigmaEyeGaze(Projectile.Center, EnigmaGreen * 0.7f, 0.4f, Projectile.velocity.SafeNormalize(Vector2.UnitX));
            }
        }
    }
}
