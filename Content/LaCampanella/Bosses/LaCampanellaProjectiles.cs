using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.LaCampanella.Bosses
{
    /// <summary>
    /// Infernal bell laser projectile - fast-moving beam with orange/black trail.
    /// </summary>
    public class InfernalBellLaser : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.ShadowBeamFriendly;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Trail particles - ENHANCED with flares and glows
            if (Main.rand.NextBool(2))
            {
                ThemedParticles.LaCampanellaTrail(Projectile.Center, Projectile.velocity);
            }
            
            // Periodic flares along trail
            if (Main.rand.NextBool(4))
            {
                Color flareColor = Main.rand.NextBool() ? ThemedParticles.CampanellaOrange : ThemedParticles.CampanellaYellow;
                CustomParticles.GenericFlare(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), flareColor, 0.25f, 12);
            }
            
            // Lighting - enhanced
            Lighting.AddLight(Projectile.Center, ThemedParticles.CampanellaOrange.ToVector3() * 0.8f);
            
            // Dust trail
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                DustID.Torch, -Projectile.velocity.X * 0.2f, -Projectile.velocity.Y * 0.2f, 100, default, 1.2f);
            dust.noGravity = true;
        }

        public override void OnKill(int timeLeft)
        {
            // ENHANCED death effects with halo and flares
            ThemedParticles.LaCampanellaBloomBurst(Projectile.Center, 0.8f);
            ThemedParticles.LaCampanellaHaloBurst(Projectile.Center, 0.6f);
            CustomParticles.GenericFlare(Projectile.Center, ThemedParticles.CampanellaYellow, 0.5f, 18);
            CustomParticles.GenericFlare(Projectile.Center, ThemedParticles.CampanellaOrange, 0.4f, 15);
            SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
            
            for (int i = 0; i < 8; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.Torch, Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-3, 3), 100, default, 1.5f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Draw glowing trail
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);
            
            // Trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                float progress = (float)i / Projectile.oldPos.Length;
                Color trailColor = Color.Lerp(ThemedParticles.CampanellaOrange, ThemedParticles.CampanellaYellow, progress) * (1f - progress);
                float scale = Projectile.scale * (1f - progress * 0.5f);
                
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition;
                Main.EntitySpriteDraw(texture, drawPos, null, trailColor, Projectile.oldRot[i], origin, scale, SpriteEffects.None, 0);
            }
            
            // Main projectile glow
            Color mainColor = Color.Lerp(ThemedParticles.CampanellaOrange, Color.White, 0.3f);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, mainColor, 
                Projectile.rotation, origin, Projectile.scale * 1.2f, SpriteEffects.None, 0);
            
            return false;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
    }

    /// <summary>
    /// Explosive bell projectile - arcing projectile that explodes on impact.
    /// </summary>
    public class ExplosiveBellProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Grenade;
        
        private float rotationSpeed = 0f;

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
        }

        public override void AI()
        {
            // Gravity
            Projectile.velocity.Y += 0.2f;
            if (Projectile.velocity.Y > 16f)
                Projectile.velocity.Y = 16f;
            
            // Rotation
            rotationSpeed = MathHelper.Lerp(rotationSpeed, Projectile.velocity.X * 0.05f, 0.1f);
            Projectile.rotation += rotationSpeed;
            
            // Trail particles - ENHANCED
            if (Main.rand.NextBool(3))
            {
                ThemedParticles.LaCampanellaTrail(Projectile.Center, Projectile.velocity * 0.5f);
            }
            
            // Occasional sparkles
            if (Main.rand.NextBool(6))
            {
                ThemedParticles.LaCampanellaSparkles(Projectile.Center, 2, 15f);
            }
            
            // Pulsing flare effect around the bell
            float pulse = (float)Math.Sin(Projectile.ai[0] * 0.1f) * 0.2f + 0.8f;
            Projectile.ai[0]++;
            
            if (Main.rand.NextBool(5))
            {
                Color flareColor = Main.rand.Next(3) switch
                {
                    0 => ThemedParticles.CampanellaOrange,
                    1 => ThemedParticles.CampanellaYellow,
                    _ => ThemedParticles.CampanellaGold
                };
                CustomParticles.GenericFlare(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f), flareColor, 0.2f * pulse, 12);
            }
            
            // Lighting - enhanced pulsing
            Lighting.AddLight(Projectile.Center, ThemedParticles.CampanellaOrange.ToVector3() * 0.6f * pulse);
        }

        public override void OnKill(int timeLeft)
        {
            // ENHANCED Explosion with full VFX suite!
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.2f }, Projectile.Center);
            
            // Visual explosion - full VFX
            ThemedParticles.LaCampanellaImpact(Projectile.Center, 2f);
            ThemedParticles.LaCampanellaShockwave(Projectile.Center, 1.5f);
            ThemedParticles.LaCampanellaHaloBurst(Projectile.Center, 1.2f);
            
            // Flare burst
            CustomParticles.GenericFlare(Projectile.Center, ThemedParticles.CampanellaYellow, 0.8f, 22);
            CustomParticles.GenericFlare(Projectile.Center, ThemedParticles.CampanellaOrange, 0.6f, 18);
            CustomParticles.ExplosionBurst(Projectile.Center, ThemedParticles.CampanellaOrange, 12, 6f);
            
            // Screen shake for nearby players
            float dist = Vector2.Distance(Main.LocalPlayer.Center, Projectile.Center);
            if (dist < 400f)
            {
                float shake = (1f - dist / 400f) * 5f;
                Main.LocalPlayer.position += Main.rand.NextVector2Circular(shake, shake);
            }
            
            // Explosion dust
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Vector2 dir = angle.ToRotationVector2();
                Dust dust = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.Torch, 
                    dir.X * 5f, dir.Y * 5f, 100, default, 2f);
                dust.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);
            
            // Glow effect
            float pulse = (float)Math.Sin(Projectile.ai[0] * 0.15f) * 0.3f + 0.7f;
            Color glowColor = ThemedParticles.CampanellaOrange * 0.5f * pulse;
            
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = (MathHelper.TwoPi * i / 4f + Projectile.ai[0] * 0.05f).ToRotationVector2() * 4f;
                Main.EntitySpriteDraw(texture, Projectile.Center + offset - Main.screenPosition, null, glowColor, 
                    Projectile.rotation, origin, Projectile.scale * 1.1f, SpriteEffects.None, 0);
            }
            
            // Main bell
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, 
                Color.Lerp(lightColor, ThemedParticles.CampanellaGold, 0.3f), Projectile.rotation, origin, 
                Projectile.scale, SpriteEffects.None, 0);
            
            return false;
        }
    }

    /// <summary>
    /// Infernal ground fire - lingering fire hazard left after landing impacts.
    /// </summary>
    public class InfernalGroundFire : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Flames;

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.alpha = 100;
        }

        public override void AI()
        {
            // Gravity until grounded
            if (!Projectile.tileCollide)
            {
                Projectile.tileCollide = true;
            }
            
            Projectile.velocity.Y += 0.3f;
            if (Projectile.velocity.Y > 12f)
                Projectile.velocity.Y = 12f;
            
            // Once landed, stick and burn
            if (Projectile.velocity.Y == 0 || Math.Abs(Projectile.velocity.Y) < 0.5f)
            {
                Projectile.velocity = Vector2.Zero;
                
                // Fire particles - ENHANCED
                if (Main.rand.NextBool(2))
                {
                    Vector2 pos = Projectile.Center + new Vector2(Main.rand.NextFloat(-15, 15), 0);
                    var glow = new GenericGlowParticle(pos, new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -2f),
                        Main.rand.NextBool() ? ThemedParticles.CampanellaOrange : ThemedParticles.CampanellaYellow,
                        Main.rand.NextFloat(0.2f, 0.4f), Main.rand.Next(15, 30), true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
                
                // Periodic flares while burning
                if (Main.rand.NextBool(8))
                {
                    Color flareColor = Main.rand.NextBool() ? ThemedParticles.CampanellaOrange : ThemedParticles.CampanellaYellow;
                    CustomParticles.GenericFlare(Projectile.Center + new Vector2(Main.rand.NextFloat(-10f, 10f), -10f), flareColor, 0.25f, 15);
                }
            }
            
            // Fade out near end
            if (Projectile.timeLeft < 30)
            {
                Projectile.alpha = (int)MathHelper.Lerp(255, 100, Projectile.timeLeft / 30f);
            }
            
            // Lighting - enhanced
            float fade = Projectile.timeLeft > 30 ? 1f : Projectile.timeLeft / 30f;
            Lighting.AddLight(Projectile.Center, ThemedParticles.CampanellaOrange.ToVector3() * 0.7f * fade);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.velocity.Y = 0;
            Projectile.velocity.X *= 0.5f;
            return false; // Don't die on tile collision
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Draw as flame column
            float fade = Projectile.timeLeft > 30 ? 1f : Projectile.timeLeft / 30f;
            
            for (int i = 0; i < 3; i++)
            {
                float yOffset = i * -10f;
                float scale = (1f - i * 0.2f) * fade;
                Color color = Color.Lerp(ThemedParticles.CampanellaOrange, ThemedParticles.CampanellaYellow, i * 0.3f) * scale;
                
                Vector2 drawPos = Projectile.Center + new Vector2(0, yOffset) - Main.screenPosition;
                Rectangle rect = new Rectangle((int)drawPos.X - 8, (int)drawPos.Y - 15, 16, 30);
                Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, rect, color * 0.5f);
            }
            
            return false;
        }
    }

    /// <summary>
    /// Infernal fire wave - ground-traveling fire wave attack.
    /// </summary>
    public class InfernalFireWave : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DD2BetsyFlameBreath;

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 80;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 100;
        }

        public override void AI()
        {
            Projectile.ai[0]++;
            
            // Wave motion - follow ground
            Vector2 checkPos = Projectile.Bottom + new Vector2(Math.Sign(Projectile.velocity.X) * 30, 20);
            Point tileCheck = checkPos.ToTileCoordinates();
            
            // Rise over gaps or fall to meet ground
            if (!WorldGen.SolidTile(tileCheck.X, tileCheck.Y))
            {
                // Check for ground below
                for (int y = 0; y < 10; y++)
                {
                    if (WorldGen.SolidTile(tileCheck.X, tileCheck.Y + y))
                    {
                        Projectile.velocity.Y = 5f;
                        break;
                    }
                }
            }
            else
            {
                // Check if we need to rise
                Point aboveCheck = new Point(tileCheck.X, (int)(Projectile.Bottom.Y / 16) - 1);
                if (WorldGen.SolidTile(aboveCheck.X, aboveCheck.Y))
                {
                    Projectile.velocity.Y = -5f;
                }
                else
                {
                    Projectile.velocity.Y = MathHelper.Lerp(Projectile.velocity.Y, 0, 0.2f);
                }
            }
            
            // Fire particles - ENHANCED
            if (Main.rand.NextBool(2))
            {
                Vector2 pos = Projectile.Center + new Vector2(Main.rand.NextFloat(-25, 25), Main.rand.NextFloat(-30, 30));
                ThemedParticles.LaCampanellaSparks(pos, new Vector2(0, -1), 2, 4f);
            }
            
            // Heavy smoke
            if (Main.rand.NextBool(4))
            {
                var smoke = new HeavySmokeParticle(Projectile.Center + Main.rand.NextVector2Circular(20, 20), 
                    new Vector2(Projectile.velocity.X * 0.3f, -1.5f), ThemedParticles.CampanellaBlack, 
                    Main.rand.Next(30, 50), Main.rand.NextFloat(0.5f, 0.8f), 0.6f, 0.02f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // ENHANCED: Flares along the wave top
            if (Main.rand.NextBool(3))
            {
                Vector2 flarePos = Projectile.Center + new Vector2(Main.rand.NextFloat(-20f, 20f), -30f);
                Color flareColor = Main.rand.Next(3) switch
                {
                    0 => ThemedParticles.CampanellaOrange,
                    1 => ThemedParticles.CampanellaYellow,
                    _ => ThemedParticles.CampanellaGold
                };
                CustomParticles.GenericFlare(flarePos, flareColor, 0.35f, 15);
            }
            
            // Grow as it travels
            float growthFactor = Math.Min(Projectile.ai[0] / 30f, 1.5f);
            Projectile.scale = 1f + growthFactor * 0.5f;
            
            // Lighting - ENHANCED
            Lighting.AddLight(Projectile.Center, ThemedParticles.CampanellaOrange.ToVector3() * 1.0f);
            
            // Periodic halo pulse
            if (Projectile.ai[0] % 20 == 0)
            {
                CustomParticles.HaloRing(Projectile.Center, ThemedParticles.CampanellaOrange * 0.6f, 0.4f, 18);
            }
            
            // Sound
            if (Projectile.ai[0] % 15 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item34 with { Volume = 0.3f, Pitch = -0.3f }, Projectile.Center);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Draw as large flame column
            float baseHeight = 80f * Projectile.scale;
            
            for (int layer = 0; layer < 5; layer++)
            {
                float layerProgress = layer / 5f;
                float layerHeight = baseHeight * (1f - layerProgress * 0.5f);
                float layerWidth = 40f * (1f - layerProgress * 0.3f) * Projectile.scale;
                
                Color color = Color.Lerp(ThemedParticles.CampanellaOrange, ThemedParticles.CampanellaYellow, layerProgress);
                if (layer == 0) color = ThemedParticles.CampanellaBlack * 0.8f;
                
                // Wave motion
                float waveOffset = (float)Math.Sin(Projectile.ai[0] * 0.2f + layer * 0.5f) * 5f;
                
                Vector2 drawPos = Projectile.Bottom + new Vector2(waveOffset, -layerHeight / 2) - Main.screenPosition;
                Rectangle rect = new Rectangle((int)(drawPos.X - layerWidth / 2), (int)(drawPos.Y - layerHeight / 2), 
                    (int)layerWidth, (int)layerHeight);
                
                Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, rect, color * (0.6f - layer * 0.08f));
            }
            
            return false;
        }
    }

    /// <summary>
    /// Massive infernal laser - huge sweeping beam attack.
    /// </summary>
    public class MassiveInfernalLaser : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.LastPrismLaser;
        
        private const float MaxLength = 2000f;
        private float currentLength = 0f;
        private float beamWidth = 60f;

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.ai[0]++;
            
            // Extend beam rapidly
            if (currentLength < MaxLength)
            {
                currentLength += 150f;
                if (currentLength > MaxLength)
                    currentLength = MaxLength;
            }
            
            // Rotation towards stored direction
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Beam width pulses
            beamWidth = 50f + (float)Math.Sin(Projectile.ai[0] * 0.3f) * 15f;
            
            // ENHANCED: Flares along beam core
            if (Main.rand.NextBool(2))
            {
                float randomDist = Main.rand.NextFloat(currentLength);
                Vector2 particlePos = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * randomDist;
                particlePos += Main.rand.NextVector2Circular(beamWidth * 0.3f, beamWidth * 0.3f);
                
                ThemedParticles.LaCampanellaSparks(particlePos, Main.rand.NextVector2Unit(), 2, 3f);
                
                // Core flares
                if (Main.rand.NextBool(3))
                {
                    Color flareColor = Main.rand.NextBool() ? ThemedParticles.CampanellaYellow : ThemedParticles.CampanellaOrange;
                    CustomParticles.GenericFlare(particlePos, flareColor, 0.4f, 15);
                }
            }
            
            // ENHANCED: Halos at beam source
            if (Projectile.ai[0] % 8 == 0)
            {
                CustomParticles.HaloRing(Projectile.Center, ThemedParticles.CampanellaOrange, 0.5f, 20);
                CustomParticles.HaloRing(Projectile.Center, ThemedParticles.CampanellaYellow * 0.7f, 0.35f, 15);
            }
            
            // End of beam explosion - ENHANCED
            if (Projectile.ai[0] % 5 == 0)
            {
                Vector2 endPos = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * currentLength;
                ThemedParticles.LaCampanellaBloomBurst(endPos, 0.6f);
                CustomParticles.GenericFlare(endPos, ThemedParticles.CampanellaYellow, 0.6f, 18);
            }
            
            // ENHANCED: Periodic massive halo at impact point
            if (Projectile.ai[0] % 12 == 0)
            {
                Vector2 endPos = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * currentLength;
                ThemedParticles.LaCampanellaHaloBurst(endPos, 0.8f);
            }
            
            // Lighting along beam - ENHANCED
            for (int i = 0; i < 10; i++)
            {
                float dist = i * currentLength / 10f;
                Vector2 lightPos = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * dist;
                Lighting.AddLight(lightPos, ThemedParticles.CampanellaOrange.ToVector3() * 1.4f);
            }
            
            // Sound
            if (Projectile.ai[0] == 1)
            {
                SoundEngine.PlaySound(SoundID.Zombie104 with { Pitch = -0.2f, Volume = 1.2f }, Projectile.Center);
            }
            
            // Fade out
            if (Projectile.timeLeft < 20)
            {
                beamWidth *= 0.9f;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Line collision
            float point = 0f;
            Vector2 endPos = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * currentLength;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), 
                Projectile.Center, endPos, beamWidth * 0.5f, ref point);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (currentLength <= 0) return false;
            
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            float rotation = direction.ToRotation();
            
            // Draw beam layers
            for (int layer = 4; layer >= 0; layer--)
            {
                float layerWidth = beamWidth * (1f + layer * 0.3f);
                float opacity = 0.15f + (4 - layer) * 0.15f;
                
                Color color;
                switch (layer)
                {
                    case 4: color = ThemedParticles.CampanellaBlack * 0.6f; break;
                    case 3: color = ThemedParticles.CampanellaRed * 0.7f; break;
                    case 2: color = ThemedParticles.CampanellaOrange * 0.8f; break;
                    case 1: color = ThemedParticles.CampanellaYellow * 0.9f; break;
                    default: color = Color.White; break;
                }
                
                // Draw beam as stretched rectangle
                Vector2 scale = new Vector2(currentLength / pixel.Width, layerWidth / pixel.Height);
                Vector2 origin = new Vector2(0, pixel.Height / 2);
                
                Main.spriteBatch.Draw(pixel, Projectile.Center - Main.screenPosition, null, 
                    color * opacity, rotation, origin, scale, SpriteEffects.None, 0f);
            }
            
            // Draw end explosion
            Vector2 endPos = Projectile.Center + direction * currentLength - Main.screenPosition;
            float endSize = beamWidth * 1.5f;
            Rectangle endRect = new Rectangle((int)(endPos.X - endSize / 2), (int)(endPos.Y - endSize / 2), 
                (int)endSize, (int)endSize);
            Main.spriteBatch.Draw(pixel, endRect, ThemedParticles.CampanellaYellow * 0.8f);
            
            return false;
        }
    }
}
