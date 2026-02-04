using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.EnigmaVariations.Debuffs;

namespace MagnumOpus.Content.EnigmaVariations.Bosses
{
    /// <summary>
    /// Fast web projectile shot by Enigma boss
    /// </summary>
    public class EnigmaWebShot : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/EnigmaEye1";
        
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 150;
            Projectile.alpha = 0;
            Projectile.penetrate = 1;
            Projectile.extraUpdates = 1; // Faster movement
            Projectile.scale = 0.5f;
        }
        
        public override void AI()
        {
            // Shimmer pulse for dynamic lighting
            float shimmerPulse = (float)Math.Sin(Projectile.timeLeft * 0.15f) * 0.3f + 0.7f;
            
            // Trail particles with variety
            if (Main.rand.NextBool(2))
            {
                Color trailColor = Main.rand.NextBool() ? EnigmaPurple : EnigmaGreen;
                CustomParticles.GenericGlow(Projectile.Center, trailColor * 0.6f, 0.25f * shimmerPulse, 12);
            }
            
            // Periodic sparkles for shimmer effect
            if (Main.rand.NextBool(5))
            {
                var sparkle = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), 
                    -Projectile.velocity * 0.1f, EnigmaGreen, 0.3f, 15);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Dynamic pulsing light
            Lighting.AddLight(Projectile.Center, EnigmaGreen.ToVector3() * 0.5f * shimmerPulse);
        }
        
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 300);
        }
        
        public override void OnKill(int timeLeft)
        {
            // Boss web shot - compact void implode
            DynamicParticleEffects.EnigmaDeathVoidImplode(Projectile.Center, 0.5f);
            SoundEngine.PlaySound(SoundID.Item10 with { Pitch = 0.3f, Volume = 0.4f }, Projectile.Center);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnigmaEye1").Value;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            
            // Core
            Main.spriteBatch.Draw(glow, pos, null, EnigmaGreen, 0f, glow.Size() / 2, 0.5f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(glow, pos, null, EnigmaPurple * 0.6f, 0f, glow.Size() / 2, 0.7f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(glow, pos, null, Color.White * 0.4f, 0f, glow.Size() / 2, 0.3f, SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    /// <summary>
    /// Glyph projectile that rises from ground eruption
    /// </summary>
    public class EnigmaGlyphProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/Glyphs8";
        
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 100;
            Projectile.penetrate = -1;
            Projectile.scale = 0.5f;
        }
        
        public override void AI()
        {
            Projectile.velocity.Y += 0.18f; // Slightly faster gravity
            Projectile.rotation += 0.12f;
            
            // Dynamic shimmer pulse
            float shimmerPulse = (float)Math.Sin(Projectile.timeLeft * 0.2f) * 0.25f + 0.75f;
            
            // Glyph particles with variety
            if (Main.rand.NextBool(3))
            {
                CustomParticles.Glyph(Projectile.Center + Main.rand.NextVector2Circular(8, 8), 
                    EnigmaPurple * 0.7f * shimmerPulse, 0.25f);
            }
            
            // Eye sparkle effect
            if (Main.rand.NextBool(8))
            {
                CustomParticles.EnigmaEyeGaze(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f),
                    EnigmaGreen * 0.6f, 0.2f, Projectile.velocity);
            }
            
            Projectile.alpha = (int)(255 * (1f - Projectile.timeLeft / 100f));
            Lighting.AddLight(Projectile.Center, EnigmaPurple.ToVector3() * 0.4f * shimmerPulse);
        }
        
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 240);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/Glyphs8").Value;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            float alpha = 1f - Projectile.alpha / 255f;
            
            Main.spriteBatch.Draw(glow, pos, null, EnigmaPurple * alpha, Projectile.rotation, glow.Size() / 2, 0.6f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(glow, pos, null, EnigmaGreen * alpha * 0.5f, Projectile.rotation * 0.5f, glow.Size() / 2, 0.8f, SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    /// <summary>
    /// Homing eye projectile
    /// </summary>
    public class EnigmaHomingEye : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/EnigmaEye1";
        
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private int TargetWhoAmI => (int)Projectile.ai[0];
        
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 240;
            Projectile.penetrate = 1;
            Projectile.scale = 0.55f;
        }
        
        public override void AI()
        {
            // Dynamic shimmer pulse
            float shimmerPulse = (float)Math.Sin(Projectile.timeLeft * 0.12f) * 0.2f + 0.8f;
            
            // Homing with faster speed
            Player target = Main.player[TargetWhoAmI];
            if (target.active && !target.dead)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                float homingStrength = 0.06f;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 14f, homingStrength);
            }
            
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Rich trail with variety
            if (Main.rand.NextBool(2))
            {
                Color trailColor = Color.Lerp(EnigmaGreen, new Color(100, 255, 150), Main.rand.NextFloat()) * 0.5f;
                CustomParticles.GenericGlow(Projectile.Center, trailColor, 0.18f * shimmerPulse, 12);
            }
            
            // Occasional eye sparkle
            if (Main.rand.NextBool(6))
            {
                var sparkle = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    -Projectile.velocity * 0.05f, EnigmaGreen, 0.25f, 12);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            Lighting.AddLight(Projectile.Center, EnigmaGreen.ToVector3() * 0.5f * shimmerPulse);
        }
        
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 300);
        }
        
        public override void OnKill(int timeLeft)
        {
            // Boss homing eye - blink and shatter
            DynamicParticleEffects.EnigmaDeathEyeBlinkShatter(Projectile.Center, 0.55f);
            SoundEngine.PlaySound(SoundID.Item8 with { Pitch = -0.2f, Volume = 0.4f }, Projectile.Center);
        }
    }
    
    /// <summary>
    /// Void web line that damages on contact
    /// </summary>
    public class EnigmaVoidWeb : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/EnigmaEye2";
        
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private float Angle => Projectile.ai[0];
        private float MaxRadius => Projectile.ai[1];
        
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 180;
            Projectile.penetrate = -1;
        }
        
        public override void AI()
        {
            // Draw web line particles
            if (Projectile.timeLeft % 5 == 0)
            {
                float currentRadius = MaxRadius * (1f - Projectile.timeLeft / 180f);
                for (float r = 0; r < currentRadius; r += 30f)
                {
                    Vector2 pos = Projectile.Center + Angle.ToRotationVector2() * r;
                    Color webColor = Color.Lerp(EnigmaPurple, EnigmaGreen, r / MaxRadius) * 0.6f;
                    CustomParticles.GenericGlow(pos, webColor, 0.2f, 8);
                }
            }
        }
        
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float currentRadius = MaxRadius * (1f - Projectile.timeLeft / 180f);
            
            // Line collision
            Vector2 start = Projectile.Center;
            Vector2 end = Projectile.Center + Angle.ToRotationVector2() * currentRadius;
            
            float dist = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 15f, ref dist);
        }
        
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.Slow, 120);
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 180);
        }
        
        public override bool PreDraw(ref Color lightColor) => false; // Handled by particles
    }
    
    /// <summary>
    /// Ground shockwave projectile
    /// </summary>
    public class EnigmaShockwave : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/GlowingHalo1";
        
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private int DelayFrames => (int)Projectile.ai[0];
        
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 40;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 90;
            Projectile.penetrate = -1;
        }
        
        public override void AI()
        {
            // Delay before starting
            if (Projectile.timeLeft > 90 - DelayFrames)
            {
                Projectile.velocity = Vector2.Zero;
                return;
            }
            
            // Ground check - stay on ground
            Vector2 checkPos = Projectile.Bottom + new Vector2(0, 16);
            Point tilePos = checkPos.ToTileCoordinates();
            if (!WorldGen.SolidTile(tilePos.X, tilePos.Y))
            {
                Projectile.velocity.Y += 0.5f;
            }
            else
            {
                Projectile.velocity.Y = 0;
            }
            
            // Visual particles
            if (Main.rand.NextBool(2))
            {
                Vector2 dustPos = Projectile.Bottom + Main.rand.NextVector2Circular(30, 5);
                Color dustColor = Main.rand.NextBool() ? EnigmaPurple : EnigmaGreen;
                var smoke = new HeavySmokeParticle(dustPos, new Vector2(Main.rand.NextFloat(-1f, 1f), -2f), dustColor, Main.rand.Next(15, 25), 0.3f, 0.5f, 0.02f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            Lighting.AddLight(Projectile.Center, EnigmaPurple.ToVector3() * 0.4f);
        }
        
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 240);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.timeLeft > 90 - DelayFrames) return false;
            
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/GlowingHalo1").Value;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            
            // Shockwave effect - scaled for player-sized projectile
            float progress = 1f - Projectile.timeLeft / (90f - DelayFrames);
            float alpha = 1f - progress;
            
            Main.spriteBatch.Draw(glow, pos, null, EnigmaPurple * alpha, 0f, glow.Size() / 2, new Vector2(0.5f, 0.6f), SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(glow, pos, null, EnigmaGreen * alpha * 0.6f, 0f, glow.Size() / 2, new Vector2(0.35f, 0.45f), SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    // EnigmaMiniSpider has been removed - it was an unnecessary enemy
    
    /// <summary>
    /// Void beam projectile for VoidBeamPincer attack
    /// Spawns from orbs and fires a damaging beam toward the player
    /// </summary>
    public class EnigmaVoidBeam : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/Glyphs9";
        
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private float BeamAngle => Projectile.ai[0];
        private int BeamDuration => (int)Projectile.ai[1];
        private float BeamLength => 800f;
        private float BeamWidth => 35f;
        
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 200;
        }
        
        public override void AI()
        {
            // Cap timeLeft to beam duration
            if (Projectile.timeLeft > BeamDuration)
            {
                Projectile.timeLeft = BeamDuration;
            }
            
            float progress = 1f - (float)Projectile.timeLeft / BeamDuration;
            float fadeProgress = progress > 0.7f ? (progress - 0.7f) / 0.3f : 0f;
            float intensity = 1f - fadeProgress;
            
            // Beam visual particles along the beam path
            if (Projectile.timeLeft % 2 == 0)
            {
                Vector2 beamDir = BeamAngle.ToRotationVector2();
                
                for (float t = 0; t < BeamLength; t += 25f)
                {
                    Vector2 beamPos = Projectile.Center + beamDir * t;
                    
                    // Core beam particles
                    Color beamColor = Color.Lerp(EnigmaPurple, EnigmaGreen, t / BeamLength) * intensity;
                    CustomParticles.GenericFlare(beamPos, beamColor, 0.35f * intensity, 4);
                    
                    // Edge particles
                    if (Main.rand.NextBool(3))
                    {
                        Vector2 edgeOffset = beamDir.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-BeamWidth * 0.5f, BeamWidth * 0.5f);
                        CustomParticles.GenericGlow(beamPos + edgeOffset, EnigmaPurple * 0.5f * intensity, 0.2f, 8);
                    }
                }
                
                // Source glow
                CustomParticles.GenericFlare(Projectile.Center, EnigmaGreen * intensity, 0.5f, 5);
                
                // Glyphs along beam
                if (Main.rand.NextBool(4))
                {
                    Vector2 glyphPos = Projectile.Center + beamDir * Main.rand.NextFloat(BeamLength);
                    CustomParticles.Glyph(glyphPos, EnigmaPurple * 0.6f * intensity, 0.3f, -1);
                }
            }
            
            Lighting.AddLight(Projectile.Center, EnigmaGreen.ToVector3() * 0.8f * intensity);
        }
        
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float progress = 1f - (float)Projectile.timeLeft / BeamDuration;
            float fadeProgress = progress > 0.7f ? (progress - 0.7f) / 0.3f : 0f;
            
            // No collision while fading
            if (fadeProgress > 0.5f) return false;
            
            Vector2 beamDir = BeamAngle.ToRotationVector2();
            Vector2 beamEnd = Projectile.Center + beamDir * BeamLength;
            
            float dist = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), 
                Projectile.Center, beamEnd, BeamWidth, ref dist);
        }
        
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 360);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/Glyphs9").Value;
            SpriteBatch spriteBatch = Main.spriteBatch;
            
            float progress = 1f - (float)Projectile.timeLeft / BeamDuration;
            float fadeProgress = progress > 0.7f ? (progress - 0.7f) / 0.3f : 0f;
            float intensity = 1f - fadeProgress;
            
            Vector2 beamDir = BeamAngle.ToRotationVector2();
            
            // Draw beam as series of overlapping glows
            for (float t = 0; t < BeamLength; t += 20f)
            {
                Vector2 drawPos = Projectile.Center + beamDir * t - Main.screenPosition;
                
                float tProgress = t / BeamLength;
                Color beamColor = Color.Lerp(EnigmaPurple, EnigmaGreen, tProgress);
                beamColor.A = 0; // Additive blending
                
                // Multi-layer bloom
                spriteBatch.Draw(glow, drawPos, null, beamColor * 0.3f * intensity, BeamAngle, glow.Size() / 2f, 
                    new Vector2(0.5f, 0.4f), SpriteEffects.None, 0f);
                spriteBatch.Draw(glow, drawPos, null, beamColor * 0.5f * intensity, BeamAngle, glow.Size() / 2f, 
                    new Vector2(0.35f, 0.25f), SpriteEffects.None, 0f);
                spriteBatch.Draw(glow, drawPos, null, Color.White * 0.3f * intensity, BeamAngle, glow.Size() / 2f, 
                    new Vector2(0.2f, 0.1f), SpriteEffects.None, 0f);
            }
            
            // Source orb
            Vector2 sourcePos = Projectile.Center - Main.screenPosition;
            spriteBatch.Draw(glow, sourcePos, null, EnigmaGreen * intensity, 0f, glow.Size() / 2f, 0.6f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glow, sourcePos, null, EnigmaPurple * 0.5f * intensity, 0f, glow.Size() / 2f, 0.8f, SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    /// <summary>
    /// Glyph wall segment for MysteryMaze attack
    /// Creates a wall of glyphs that damages players on contact
    /// </summary>
    public class EnigmaMazeWall : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/Glyphs10";
        
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private float WallAngle => Projectile.ai[0];
        private int WallDuration => (int)Projectile.ai[1];
        private float WallLength => 200f;
        private float WallWidth => 25f;
        
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
        }
        
        public override void AI()
        {
            // Cap timeLeft to wall duration
            if (Projectile.timeLeft > WallDuration)
            {
                Projectile.timeLeft = WallDuration;
            }
            
            float progress = 1f - (float)Projectile.timeLeft / WallDuration;
            float fadeProgress = progress > 0.8f ? (progress - 0.8f) / 0.2f : 0f;
            float intensity = 1f - fadeProgress;
            
            // Wall visual particles
            if (Projectile.timeLeft % 4 == 0)
            {
                Vector2 wallDir = WallAngle.ToRotationVector2();
                
                for (float t = 0; t <= WallLength; t += 30f)
                {
                    Vector2 wallPos = Projectile.Center + wallDir * t;
                    
                    // Glyphs along wall
                    CustomParticles.Glyph(wallPos + Main.rand.NextVector2Circular(10f, 10f), 
                        EnigmaPurple * (0.5f + intensity * 0.3f), 0.35f, -1);
                    
                    // Connecting glow particles
                    Color wallColor = Color.Lerp(EnigmaPurple, EnigmaGreen, t / WallLength) * intensity;
                    CustomParticles.GenericGlow(wallPos, wallColor * 0.4f, 0.2f, 6);
                }
                
                // Eye at wall endpoint
                if (Main.rand.NextBool(3))
                {
                    Vector2 eyePos = Projectile.Center + wallDir * WallLength;
                    CustomParticles.EnigmaEyeGaze(eyePos, EnigmaGreen * 0.5f * intensity, 0.25f, 
                        eyePos - wallDir); // Looking back along wall
                }
            }
            
            // Ambient lighting
            Lighting.AddLight(Projectile.Center + WallAngle.ToRotationVector2() * WallLength * 0.5f, 
                EnigmaPurple.ToVector3() * 0.3f * intensity);
        }
        
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float progress = 1f - (float)Projectile.timeLeft / WallDuration;
            float fadeProgress = progress > 0.8f ? (progress - 0.8f) / 0.2f : 0f;
            
            // No collision while fading
            if (fadeProgress > 0.3f) return false;
            
            Vector2 wallDir = WallAngle.ToRotationVector2();
            Vector2 wallEnd = Projectile.Center + wallDir * WallLength;
            
            float dist = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), 
                Projectile.Center, wallEnd, WallWidth, ref dist);
        }
        
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 240);
            target.AddBuff(BuffID.Slow, 90);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/Glyphs10").Value;
            SpriteBatch spriteBatch = Main.spriteBatch;
            
            float progress = 1f - (float)Projectile.timeLeft / WallDuration;
            float fadeProgress = progress > 0.8f ? (progress - 0.8f) / 0.2f : 0f;
            float intensity = 1f - fadeProgress;
            
            Vector2 wallDir = WallAngle.ToRotationVector2();
            
            // Draw wall as connected glowing segments
            for (float t = 0; t <= WallLength; t += 15f)
            {
                Vector2 drawPos = Projectile.Center + wallDir * t - Main.screenPosition;
                
                float tProgress = t / WallLength;
                Color wallColor = Color.Lerp(EnigmaPurple, EnigmaGreen, tProgress);
                wallColor.A = 0;
                
                // Multi-layer bloom for wall segment
                spriteBatch.Draw(glow, drawPos, null, wallColor * 0.4f * intensity, WallAngle + MathHelper.PiOver2, 
                    glow.Size() / 2f, new Vector2(0.15f, 0.5f), SpriteEffects.None, 0f);
                spriteBatch.Draw(glow, drawPos, null, wallColor * 0.6f * intensity, WallAngle + MathHelper.PiOver2, 
                    glow.Size() / 2f, new Vector2(0.1f, 0.35f), SpriteEffects.None, 0f);
            }
            
            // End caps
            Vector2 startPos = Projectile.Center - Main.screenPosition;
            Vector2 endPos = Projectile.Center + wallDir * WallLength - Main.screenPosition;
            
            spriteBatch.Draw(glow, startPos, null, EnigmaPurple * intensity, 0f, glow.Size() / 2f, 0.3f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glow, endPos, null, EnigmaGreen * intensity, 0f, glow.Size() / 2f, 0.3f, SpriteEffects.None, 0f);
            
            return false;
        }
    }
}
