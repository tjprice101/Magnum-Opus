using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

// Dynamic particle effects for aesthetically pleasing animations
using static MagnumOpus.Common.Systems.DynamicParticleEffects;

namespace MagnumOpus.Content.Autumn.Projectiles
{
    /// <summary>
    /// Decay Bolt - Main projectile for Withering Grimoire
    /// TRUE_VFX_STANDARDS: Layered spinning flares, dense dust, orbiting music notes, hslToRgb color oscillation
    /// Creates entropic fields on impact
    /// </summary>
    public class DecayBoltProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow2";
        
        private static readonly Color DecayPurple = new Color(100, 50, 120);
        private static readonly Color DeathGreen = new Color(80, 120, 60);
        private static readonly Color WitherBrown = new Color(90, 60, 40);
        private static readonly Color DecayWhite = new Color(200, 180, 160);
        
        // Hue range for color oscillation (decay purple-green range: 0.75-0.35 wrapping through purple-blue-green)
        private const float HueMin = 0.28f;  // Green-ish
        private const float HueMax = 0.38f;  // Yellow-green

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // ═══════════════════════════════════════════════════════════════
            // DENSE DUST TRAIL - 2+ particles per frame! (TRUE_VFX_STANDARDS)
            // ═══════════════════════════════════════════════════════════════
            for (int i = 0; i < 2; i++)
            {
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(6f, 6f);
                Vector2 dustVel = -Projectile.velocity * 0.12f + Main.rand.NextVector2Circular(2f, 2f);
                
                // COLOR OSCILLATION with Main.hslToRgb
                float hue = HueMin + ((Main.GameUpdateCount * 0.022f + i * 0.15f) % 1f) * (HueMax - HueMin);
                Color oscillatingColor = Main.hslToRgb(hue, 0.55f, 0.45f);
                
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.CursedTorch, dustVel, 0, oscillatingColor, 1.5f);
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
            }

            // ═══════════════════════════════════════════════════════════════
            // CONTRASTING SPARKLES - 1-in-2 for visual pop
            // ═══════════════════════════════════════════════════════════════
            if (Main.rand.NextBool(2))
            {
                Vector2 sparklePos = Projectile.Center + Main.rand.NextVector2Circular(8f, 8f);
                Dust contrast = Dust.NewDustPerfect(sparklePos, DustID.GreenTorch, 
                    -Projectile.velocity * 0.1f, 0, DeathGreen, 1.2f);
                contrast.noGravity = true;
            }

            // ═══════════════════════════════════════════════════════════════
            // FREQUENT FLARES littering the air - 1-in-2
            // ═══════════════════════════════════════════════════════════════
            if (Main.rand.NextBool(2))
            {
                Vector2 flareOffset = Main.rand.NextVector2Circular(10f, 10f);
                float hue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color flareColor = Main.hslToRgb(hue, 0.6f, 0.5f);
                CustomParticles.GenericFlare(Projectile.Center + flareOffset, flareColor, 0.4f, 14);
            }

            // ═══════════════════════════════════════════════════════════════
            // ENTROPIC SPIRAL TRAIL - Enhanced
            // ═══════════════════════════════════════════════════════════════
            float spiralAngle = Main.GameUpdateCount * 0.22f;
            Vector2 spiralOffset = spiralAngle.ToRotationVector2() * 9f;
            Vector2 trailVel = -Projectile.velocity * 0.12f + spiralAngle.ToRotationVector2() * 1.5f;
            Color trailColor = Color.Lerp(DecayPurple, DeathGreen, (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.5f + 0.5f) * 0.6f;
            var trail = new GenericGlowParticle(Projectile.Center + spiralOffset, trailVel, trailColor, 0.35f, 24, true);
            MagnumParticleHandler.SpawnParticle(trail);

            // ═══════════════════════════════════════════════════════════════
            // ARCANE GLYPH ORBIT - Enhanced with more glyphs
            // ═══════════════════════════════════════════════════════════════
            if (Main.GameUpdateCount % 8 == 0)
            {
                float glyphAngle = Main.GameUpdateCount * 0.1f;
                for (int g = 0; g < 3; g++)
                {
                    float thisGlyphAngle = glyphAngle + MathHelper.TwoPi * g / 3f;
                    Vector2 glyphPos = Projectile.Center + thisGlyphAngle.ToRotationVector2() * 16f;
                    float hue = HueMin + (float)g / 3f * (HueMax - HueMin);
                    Color glyphColor = Main.hslToRgb(hue, 0.5f, 0.5f);
                    CustomParticles.Glyph(glyphPos, glyphColor * 0.75f, 0.32f, Main.rand.Next(1, 13));
                }
            }

            // ═══════════════════════════════════════════════════════════════
            // ORBITING MUSIC NOTES - Locked to projectile! (TRUE_VFX_STANDARDS)
            // ═══════════════════════════════════════════════════════════════
            float musicOrbitAngle = Main.GameUpdateCount * 0.09f;
            float musicOrbitRadius = 14f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 4f;
            
            if (Main.rand.NextBool(5))
            {
                for (int i = 0; i < 3; i++)
                {
                    float noteAngle = musicOrbitAngle + MathHelper.TwoPi * i / 3f;
                    Vector2 noteOffset = noteAngle.ToRotationVector2() * musicOrbitRadius;
                    Vector2 notePos = Projectile.Center + noteOffset;
                    
                    // Note velocity matches projectile + slight outward drift
                    Vector2 noteVel = Projectile.velocity * 0.6f + noteAngle.ToRotationVector2() * 0.5f;
                    
                    // VISIBLE SCALE (0.8f+) with shimmer
                    float shimmer = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f + i) * 0.12f;
                    float hue = HueMin + (float)i / 3f * (HueMax - HueMin);
                    Color noteColor = Main.hslToRgb(hue, 0.55f, 0.55f);
                    ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.8f * shimmer, 36);
                    
                    // Sparkle companion
                    var sparkle = new SparkleParticle(notePos, noteVel * 0.4f, DecayWhite * 0.5f, 0.26f, 16);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }

            // ═══════════════════════════════════════════════════════════════
            // DEATH MOTE CLOUD - Enhanced
            // ═══════════════════════════════════════════════════════════════
            if (Main.rand.NextBool(3))
            {
                float moteAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float moteRadius = Main.rand.NextFloat(12f, 22f);
                Vector2 motePos = Projectile.Center + moteAngle.ToRotationVector2() * moteRadius;
                Vector2 moteVel = new Vector2(Main.rand.NextFloat(-0.6f, 0.6f), Main.rand.NextFloat(-1.8f, -0.4f));
                float hue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color moteColor = Main.hslToRgb(hue, 0.5f, 0.45f) * 0.45f;
                var mote = new GenericGlowParticle(motePos, moteVel, moteColor, 0.16f, 30, true);
                MagnumParticleHandler.SpawnParticle(mote);
            }

            // Core glow - enhanced pulsing
            float corePulse = 0.4f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.1f;
            CustomParticles.GenericFlare(Projectile.Center, DecayPurple * corePulse, 0.28f, 7);
            
            // ═══════════════════════════════════════════════════════════════
            // DYNAMIC PARTICLE EFFECTS - Autumn decay pulsing and spirals
            // ═══════════════════════════════════════════════════════════════
            if (Main.GameUpdateCount % 6 == 0)
            {
                PulsingGlow(Projectile.Center, Vector2.Zero, DecayPurple, DeathGreen, 0.3f, 20, 0.14f, 0.22f);
            }
            if (Main.GameUpdateCount % 35 == 0)
            {
                SpiralVortex(Projectile.Center, DeathGreen, DecayPurple, 6, 20f, 0.02f, 2f, 0.22f, 30);
            }
            if (Main.rand.NextBool(4))
            {
                TwinklingSparks(Projectile.Center, DecayWhite, 2, 14f, 0.2f, 24);
            }

            // BRIGHT lighting
            float lightPulse = (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.2f + 0.8f;
            Lighting.AddLight(Projectile.Center, DeathGreen.ToVector3() * lightPulse);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Life Drain: 5% heal
            Player owner = Main.player[Projectile.owner];
            int healAmount = Math.Max(1, damageDone / 20);
            owner.Heal(healAmount);

            // Apply withering debuff
            target.AddBuff(BuffID.CursedInferno, 180);

            // Create entropic field
            if (Main.myPlayer == Projectile.owner && Projectile.penetrate <= 1)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_OnHit(target),
                    target.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<EntropicField>(),
                    Projectile.damage / 4,
                    0f,
                    Projectile.owner
                );
            }

            // ═══════════════════════════════════════════════════════════════
            // GLIMMER IMPACT - Layered cascade (TRUE_VFX_STANDARDS)
            // ═══════════════════════════════════════════════════════════════
            
            // Central flash cascade
            CustomParticles.GenericFlare(target.Center, DecayWhite * 0.7f, 0.7f, 22);
            CustomParticles.GenericFlare(target.Center, DeathGreen, 0.55f, 20);
            CustomParticles.GenericFlare(target.Center, DecayPurple, 0.45f, 18);
            
            // Music notes BURST outward with gradient
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                float hue = HueMin + (float)i / 8f * (HueMax - HueMin);
                Color noteColor = Main.hslToRgb(hue, 0.55f, 0.55f);
                ThemedParticles.MusicNote(target.Center, noteVel, noteColor, 0.85f, 38);
            }
            
            // Decay wisp burst - 6 point pattern
            for (int wisp = 0; wisp < 6; wisp++)
            {
                float wispAngle = MathHelper.TwoPi * wisp / 6f;
                Vector2 wispPos = target.Center + wispAngle.ToRotationVector2() * 14f;
                float hue = HueMin + (float)wisp / 6f * (HueMax - HueMin);
                Color wispColor = Main.hslToRgb(hue, 0.5f, 0.5f);
                CustomParticles.GenericFlare(wispPos, wispColor * 0.75f, 0.22f, 12);
            }
            
            // Halo ring expansion
            CustomParticles.HaloRing(target.Center, DeathGreen * 0.6f, 0.45f, 18);
            CustomParticles.HaloRing(target.Center, DecayPurple * 0.5f, 0.32f, 16);
            
            // Sparkle burst ring
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 6f);
                var sparkle = new SparkleParticle(target.Center, sparkVel, DecayWhite * 0.7f, 0.32f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Dust burst for density
            for (int i = 0; i < 10; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(6f, 6f);
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.CursedTorch, sparkVel, 0, DeathGreen, 1.3f);
                dust.noGravity = true;
                dust.fadeIn = 1.1f;
            }
            
            // Glyph burst for arcane impact
            CustomParticles.GlyphBurst(target.Center, DecayPurple, 5, 4.5f);
            
            // === DYNAMIC PARTICLE EFFECTS - Autumn impact ===
            AutumnImpact(target.Center, 1f);
            DramaticImpact(target.Center, DecayWhite, DeathGreen, 0.6f, 24);
            
            // Bright impact lighting
            Lighting.AddLight(target.Center, DeathGreen.ToVector3() * 1.2f);
        }

        public override void OnKill(int timeLeft)
        {
            // ═══════════════════════════════════════════════════════════════
            // GLIMMER CASCADE - NOT a puff! (TRUE_VFX_STANDARDS)
            // ═══════════════════════════════════════════════════════════════
            
            // Central glimmer - multiple layered spinning flares
            for (int layer = 0; layer < 4; layer++)
            {
                float layerScale = 0.32f + layer * 0.14f;
                float layerAlpha = 0.8f - layer * 0.15f;
                Color layerColor = Color.Lerp(DecayWhite, DecayPurple, layer / 4f);
                CustomParticles.GenericFlare(Projectile.Center, layerColor * layerAlpha, layerScale, 18 - layer * 2);
            }
            
            // Expanding glow rings
            for (int ring = 0; ring < 4; ring++)
            {
                float hue = HueMin + (float)ring / 4f * (HueMax - HueMin);
                Color ringColor = Main.hslToRgb(hue, 0.5f, 0.5f);
                CustomParticles.HaloRing(Projectile.Center, ringColor * 0.65f, 0.32f + ring * 0.12f, 15 + ring * 3);
            }
            
            // Decay wisp burst pattern
            for (int wisp = 0; wisp < 6; wisp++)
            {
                float wispAngle = MathHelper.TwoPi * wisp / 6f;
                Vector2 wispPos = Projectile.Center + wispAngle.ToRotationVector2() * 14f;
                float hue = HueMin + (float)wisp / 6f * (HueMax - HueMin);
                Color wispColor = Main.hslToRgb(hue, 0.5f, 0.5f);
                CustomParticles.GenericFlare(wispPos, wispColor * 0.7f, 0.2f, 11);
            }
            
            // Music note finale - burst outward with gradient
            for (int i = 0; i < 9; i++)
            {
                float angle = MathHelper.TwoPi * i / 9f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                float hue = HueMin + (float)i / 9f * (HueMax - HueMin);
                Color noteColor = Main.hslToRgb(hue, 0.55f, 0.55f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.85f, 40);
            }
            
            // Radial sparkle burst
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 sparkleVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                var sparkle = new SparkleParticle(Projectile.Center, sparkleVel, 
                    Color.Lerp(DecayWhite, DeathGreen, Main.rand.NextFloat()), 0.36f, 22);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Glow burst
            for (int i = 0; i < 10; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(5f, 5f);
                Color burstColor = Color.Lerp(DecayPurple, DeathGreen, Main.rand.NextFloat()) * 0.55f;
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor, 0.26f, 20, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            // Dust explosion for density
            for (int i = 0; i < 12; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(5f, 5f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.CursedTorch, dustVel, 0, DeathGreen, 1.3f);
                dust.noGravity = true;
                dust.fadeIn = 1f;
            }
            
            // Glyph circle on death
            CustomParticles.GlyphBurst(Projectile.Center, DeathGreen, 6, 4f);
            
            // Bright lighting flash
            Lighting.AddLight(Projectile.Center, DeathGreen.ToVector3() * 1.4f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            
            // Load MULTIPLE flare textures for layered spinning effect
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2").Value;
            Texture2D flare1 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Texture2D flare2 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare3").Value;
            
            Vector2 origin = texture.Size() / 2f;
            Vector2 flareOrigin1 = flare1.Size() / 2f;
            Vector2 flareOrigin2 = flare2.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float time = Main.GameUpdateCount * 0.055f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.15f;
            
            // Colors with alpha removed for proper additive blending (Fargos pattern)
            Color purpleBloom = DecayPurple with { A = 0 };
            Color greenBloom = DeathGreen with { A = 0 };
            Color brownBloom = WitherBrown with { A = 0 };
            Color whiteBloom = DecayWhite with { A = 0 };

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // ═══════════════════════════════════════════════════════════════
            // TRAIL RENDERING with gradient
            // ═══════════════════════════════════════════════════════════════
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                float progress = (float)i / Projectile.oldPos.Length;
                float trailAlpha = (1f - progress) * 0.55f;
                float trailScale = 0.35f * (1f - progress * 0.5f);
                Color trailColor = Color.Lerp(purpleBloom, greenBloom, progress) * trailAlpha;
                
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                spriteBatch.Draw(texture, trailPos, null, trailColor, Projectile.oldRot[i], 
                    origin, trailScale, SpriteEffects.None, 0f);
            }

            // ═══════════════════════════════════════════════════════════════
            // 4+ LAYERED SPINNING FLARES (TRUE_VFX_STANDARDS)
            // ═══════════════════════════════════════════════════════════════
            
            // Layer 1: Soft glow base (large, dim)
            spriteBatch.Draw(texture, drawPos, null, purpleBloom * 0.4f, 0f, 
                origin, 0.55f * pulse, SpriteEffects.None, 0f);
            
            // Layer 2: First flare spinning clockwise (green)
            spriteBatch.Draw(flare1, drawPos, null, greenBloom * 0.5f, time, 
                flareOrigin1, 0.4f * pulse, SpriteEffects.None, 0f);
            
            // Layer 3: Second flare spinning counter-clockwise (brown accent)
            spriteBatch.Draw(flare2, drawPos, null, brownBloom * 0.4f, -time * 0.75f, 
                flareOrigin2, 0.35f * pulse, SpriteEffects.None, 0f);
            
            // Layer 4: Third flare different rotation speed (purple)
            spriteBatch.Draw(flare1, drawPos, null, purpleBloom * 0.5f, time * 1.4f, 
                flareOrigin1, 0.32f * pulse, SpriteEffects.None, 0f);
            
            // Layer 5: Main glow texture
            spriteBatch.Draw(texture, drawPos, null, greenBloom * 0.55f, 0f, 
                origin, 0.3f * pulse, SpriteEffects.None, 0f);
            
            // Layer 6: Pale white core
            spriteBatch.Draw(texture, drawPos, null, whiteBloom * 0.55f, 0f, 
                origin, 0.15f, SpriteEffects.None, 0f);

            // ═══════════════════════════════════════════════════════════════
            // ORBITING SPARK POINTS (entropic wisps)
            // ═══════════════════════════════════════════════════════════════
            float sparkOrbitAngle = time * 1.6f;
            for (int i = 0; i < 4; i++)
            {
                float sparkAngle = sparkOrbitAngle + MathHelper.TwoPi * i / 4f;
                float sparkRadius = 10f + (float)Math.Sin(time * 3f + i) * 3f;
                Vector2 sparkPos = drawPos + sparkAngle.ToRotationVector2() * sparkRadius;
                Color sparkColor = Color.Lerp(purpleBloom, greenBloom, i / 4f);
                spriteBatch.Draw(texture, sparkPos, null, sparkColor * 0.6f, 0f, 
                    origin, 0.1f * pulse, SpriteEffects.None, 0f);
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Entropic Field - Damaging zone created on bolt impact
    /// TRUE_VFX_STANDARDS: Dense particles, orbiting music notes, layered glows
    /// </summary>
    public class EntropicField : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/GlowingHalo2";
        
        private static readonly Color DecayPurple = new Color(100, 50, 120);
        private static readonly Color DeathGreen = new Color(80, 120, 60);
        private static readonly Color DecayWhite = new Color(200, 180, 160);
        
        // Hue range for color oscillation
        private const float HueMin = 0.28f;
        private const float HueMax = 0.38f;

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            float lifeProgress = 1f - (float)Projectile.timeLeft / 180f;
            float fadeOut = Projectile.timeLeft < 30 ? (float)Projectile.timeLeft / 30f : 1f;

            // ═══════════════════════════════════════════════════════════════
            // DENSE SWIRLING DECAY PARTICLES - 2+ per frame (TRUE_VFX_STANDARDS)
            // ═══════════════════════════════════════════════════════════════
            for (int i = 0; i < 2; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = Main.rand.NextFloat(10f, 42f);
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * dist;
                Vector2 vel = (angle + MathHelper.PiOver2).ToRotationVector2() * Main.rand.NextFloat(1.5f, 3.5f);
                vel += new Vector2(0, -0.6f);
                
                // COLOR OSCILLATION with hslToRgb
                float hue = HueMin + ((Main.GameUpdateCount * 0.02f + i * 0.2f) % 1f) * (HueMax - HueMin);
                Color color = Main.hslToRgb(hue, 0.5f, 0.45f) * 0.55f * fadeOut;
                var particle = new GenericGlowParticle(pos, vel, color, 0.28f, 26, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }
            
            // Contrasting sparkles 1-in-2
            if (Main.rand.NextBool(2))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = Main.rand.NextFloat(15f, 35f);
                Vector2 sparkPos = Projectile.Center + angle.ToRotationVector2() * dist;
                var sparkle = new SparkleParticle(sparkPos, new Vector2(0, -1f), DecayWhite * 0.4f * fadeOut, 0.22f, 14);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Central glow - enhanced pulsing
            float corePulse = 0.35f + (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.1f;
            CustomParticles.GenericFlare(Projectile.Center, DecayPurple * corePulse * fadeOut, 0.35f, 7);

            // ═══════════════════════════════════════════════════════════════
            // ORBITING MUSIC NOTES - Locked to field center (TRUE_VFX_STANDARDS)
            // ═══════════════════════════════════════════════════════════════
            float musicOrbitAngle = Main.GameUpdateCount * 0.06f;
            float musicOrbitRadius = 30f + (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 8f;
            
            if (Main.rand.NextBool(6))
            {
                for (int i = 0; i < 3; i++)
                {
                    float noteAngle = musicOrbitAngle + MathHelper.TwoPi * i / 3f;
                    Vector2 noteOffset = noteAngle.ToRotationVector2() * musicOrbitRadius;
                    Vector2 notePos = Projectile.Center + noteOffset;
                    Vector2 noteVel = noteAngle.ToRotationVector2() * 0.5f + new Vector2(0, -1.2f);
                    
                    float shimmer = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f + i) * 0.12f;
                    float hue = HueMin + (float)i / 3f * (HueMax - HueMin);
                    Color noteColor = Main.hslToRgb(hue, 0.5f, 0.5f) * fadeOut;
                    ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.75f * shimmer, 32);
                    
                    // Sparkle companion
                    var sparkle = new SparkleParticle(notePos, noteVel * 0.3f, DecayWhite * 0.35f * fadeOut, 0.2f, 14);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }
            
            // ═══════════════════════════════════════════════════════════════
            // DECAY GLYPH ORBIT - Enhanced
            // ═══════════════════════════════════════════════════════════════
            if (Main.GameUpdateCount % 10 == 0)
            {
                float glyphAngle = Main.GameUpdateCount * 0.05f;
                for (int g = 0; g < 4; g++)
                {
                    float thisGlyphAngle = glyphAngle + MathHelper.TwoPi * g / 4f;
                    Vector2 glyphPos = Projectile.Center + thisGlyphAngle.ToRotationVector2() * 35f;
                    float hue = HueMin + (float)g / 4f * (HueMax - HueMin);
                    Color glyphColor = Main.hslToRgb(hue, 0.45f, 0.45f) * fadeOut;
                    CustomParticles.Glyph(glyphPos, glyphColor * 0.55f, 0.25f, Main.rand.Next(1, 13));
                }
            }

            // Bright lighting
            float lightPulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.15f + 0.7f;
            Lighting.AddLight(Projectile.Center, DeathGreen.ToVector3() * 0.5f * fadeOut * lightPulse);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Life Drain: 5% heal
            Player owner = Main.player[Projectile.owner];
            int healAmount = Math.Max(1, damageDone / 20);
            owner.Heal(healAmount);

            target.AddBuff(BuffID.CursedInferno, 60);

            // ═══════════════════════════════════════════════════════════════
            // ENHANCED HIT VFX (TRUE_VFX_STANDARDS)
            // ═══════════════════════════════════════════════════════════════
            
            // Central flash cascade
            CustomParticles.GenericFlare(target.Center, DecayWhite * 0.5f, 0.45f, 16);
            CustomParticles.GenericFlare(target.Center, DeathGreen * 0.7f, 0.35f, 14);
            CustomParticles.GenericFlare(target.Center, DecayPurple * 0.6f, 0.28f, 12);
            
            // Music note ring burst - VISIBLE SCALE
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 noteVel = angle.ToRotationVector2() * 2.5f + new Vector2(0, -1f);
                float hue = HueMin + (float)i / 5f * (HueMax - HueMin);
                Color noteColor = Main.hslToRgb(hue, 0.5f, 0.5f);
                ThemedParticles.MusicNote(target.Center, noteVel, noteColor, 0.72f, 28);
            }
            
            // Sparkle burst
            for (int i = 0; i < 5; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(4f, 4f);
                Color sparkColor = Color.Lerp(DecayPurple, DeathGreen, Main.rand.NextFloat()) * 0.5f;
                var spark = new GenericGlowParticle(target.Center, sparkVel, sparkColor, 0.22f, 16, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // Decay glyph accent
            CustomParticles.Glyph(target.Center, DecayPurple * 0.5f, 0.25f, Main.rand.Next(1, 13));
            
            // Halo ring
            CustomParticles.HaloRing(target.Center, DeathGreen * 0.4f, 0.28f, 14);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/GlowingHalo2").Value;
            Texture2D flare = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            Vector2 flareOrigin = flare.Size() / 2f;

            float time = Main.GameUpdateCount * 0.045f;
            float pulse = (float)Math.Sin(time * 2f) * 0.12f + 1f;
            float fadeOut = Projectile.timeLeft < 30 ? (float)Projectile.timeLeft / 30f : 1f;
            
            // Colors with alpha removed (Fargos pattern)
            Color purpleBloom = DecayPurple with { A = 0 };
            Color greenBloom = DeathGreen with { A = 0 };

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Layer 1: Large halo base
            spriteBatch.Draw(texture, drawPos, null, purpleBloom * 0.25f * fadeOut, 0f, 
                origin, 0.85f * pulse, SpriteEffects.None, 0f);
            
            // Layer 2: Spinning flare (clockwise)
            spriteBatch.Draw(flare, drawPos, null, greenBloom * 0.35f * fadeOut, time, 
                flareOrigin, 0.5f * pulse, SpriteEffects.None, 0f);
            
            // Layer 3: Spinning flare (counter-clockwise)
            spriteBatch.Draw(flare, drawPos, null, purpleBloom * 0.3f * fadeOut, -time * 0.7f, 
                flareOrigin, 0.4f * pulse, SpriteEffects.None, 0f);
            
            // Layer 4: Inner halo
            spriteBatch.Draw(texture, drawPos, null, greenBloom * 0.2f * fadeOut, 0f, 
                origin, 0.55f * pulse, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Withering Wave - Charged attack projectile
    /// TRUE_VFX_STANDARDS: Dense particles, orbiting music notes, layered spinning flares, hslToRgb
    /// </summary>
    public class WitheringWave : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/ParticleTrail2";
        
        private static readonly Color DecayPurple = new Color(100, 50, 120);
        private static readonly Color DeathGreen = new Color(80, 120, 60);
        private static readonly Color WitherBrown = new Color(90, 60, 40);
        private static readonly Color DecayWhite = new Color(200, 180, 160);
        
        // Hue range for color oscillation
        private const float HueMin = 0.08f;  // Orange-brown
        private const float HueMax = 0.15f;  // Yellow-brown

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity *= 0.97f;

            // Expand hitbox as it travels
            if (Projectile.ai[0] < 30)
            {
                Projectile.ai[0]++;
                Projectile.width = (int)MathHelper.Lerp(80, 150, Projectile.ai[0] / 30f);
                Projectile.height = (int)MathHelper.Lerp(40, 80, Projectile.ai[0] / 30f);
            }

            float fadeOut = Projectile.timeLeft < 30 ? (float)Projectile.timeLeft / 30f : 1f;

            // ═══════════════════════════════════════════════════════════════
            // INTENSE DENSE WAVE PARTICLES - 4+ per frame (TRUE_VFX_STANDARDS)
            // ═══════════════════════════════════════════════════════════════
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(Projectile.width / 3f, Projectile.height / 3f);
                Vector2 particleVel = -Projectile.velocity * 0.18f + Main.rand.NextVector2Circular(2.5f, 2.5f);
                
                // COLOR OSCILLATION with hslToRgb
                float hue = HueMin + ((Main.GameUpdateCount * 0.025f + i * 0.15f) % 1f) * (HueMax - HueMin);
                Color color = Main.hslToRgb(hue, 0.45f, 0.4f) * 0.55f * fadeOut;
                var particle = new GenericGlowParticle(Projectile.Center + offset, particleVel, color, 0.38f, 24, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }
            
            // ═══════════════════════════════════════════════════════════════
            // CONTRASTING SPARKLES - 1-in-2 (TRUE_VFX_STANDARDS)
            // ═══════════════════════════════════════════════════════════════
            if (Main.rand.NextBool(2))
            {
                Vector2 sparklePos = Projectile.Center + Main.rand.NextVector2Circular(Projectile.width / 2.5f, Projectile.height / 2.5f);
                var sparkle = new SparkleParticle(sparklePos, -Projectile.velocity * 0.1f, DecayWhite * 0.5f * fadeOut, 0.28f, 16);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // ═══════════════════════════════════════════════════════════════
            // FREQUENT FLARES - 1-in-2 (TRUE_VFX_STANDARDS)
            // ═══════════════════════════════════════════════════════════════
            if (Main.rand.NextBool(2))
            {
                Vector2 flareOffset = Main.rand.NextVector2Circular(Projectile.width / 3f, Projectile.height / 3f);
                float hue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color flareColor = Main.hslToRgb(hue, 0.5f, 0.45f);
                CustomParticles.GenericFlare(Projectile.Center + flareOffset, flareColor * fadeOut, 0.35f, 12);
            }

            // Core wave glow - enhanced
            float corePulse = 0.45f + (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.1f;
            CustomParticles.GenericFlare(Projectile.Center, DecayPurple * corePulse * fadeOut, 0.45f, 7);

            // ═══════════════════════════════════════════════════════════════
            // ORBITING MUSIC NOTES - Locked to wave (TRUE_VFX_STANDARDS)
            // ═══════════════════════════════════════════════════════════════
            float musicOrbitAngle = Main.GameUpdateCount * 0.08f;
            float musicOrbitRadius = 20f + Projectile.ai[0] * 0.5f; // Expands with wave
            
            if (Main.rand.NextBool(4))
            {
                for (int i = 0; i < 4; i++)
                {
                    float noteAngle = musicOrbitAngle + MathHelper.TwoPi * i / 4f;
                    Vector2 noteOffset = noteAngle.ToRotationVector2() * musicOrbitRadius;
                    Vector2 notePos = Projectile.Center + noteOffset;
                    Vector2 noteVel = Projectile.velocity * 0.5f + noteAngle.ToRotationVector2() * 0.6f + new Vector2(0, -1.5f);
                    
                    float shimmer = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f + i) * 0.12f;
                    float hue = HueMin + (float)i / 4f * (HueMax - HueMin);
                    Color noteColor = Main.hslToRgb(hue, 0.5f, 0.5f) * fadeOut;
                    ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.78f * shimmer, 30);
                    
                    // Sparkle companion
                    var sparkle = new SparkleParticle(notePos, noteVel * 0.3f, DecayWhite * 0.35f * fadeOut, 0.2f, 14);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }
            
            // ═══════════════════════════════════════════════════════════════
            // WITHER GLYPH TRAIL
            // ═══════════════════════════════════════════════════════════════
            if (Main.GameUpdateCount % 8 == 0)
            {
                float hue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color glyphColor = Main.hslToRgb(hue, 0.45f, 0.45f) * fadeOut;
                CustomParticles.Glyph(Projectile.Center + Main.rand.NextVector2Circular(15f, 10f), 
                    glyphColor * 0.5f, 0.28f, Main.rand.Next(1, 13));
            }

            // Bright lighting
            float lightPulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.15f + 0.8f;
            Lighting.AddLight(Projectile.Center, DeathGreen.ToVector3() * 0.7f * fadeOut * lightPulse);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Life Drain: 5% heal
            Player owner = Main.player[Projectile.owner];
            int healAmount = Math.Max(1, damageDone / 20);
            owner.Heal(healAmount);

            // Heavy debuffs
            target.AddBuff(BuffID.CursedInferno, 300);
            target.AddBuff(BuffID.Ichor, 240);

            // Create entropic field
            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_OnHit(target),
                    target.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<EntropicField>(),
                    Projectile.damage / 3,
                    0f,
                    Projectile.owner
                );
            }

            // ═══════════════════════════════════════════════════════════════
            // HEAVY IMPACT VFX - Glimmer cascade (TRUE_VFX_STANDARDS)
            // ═══════════════════════════════════════════════════════════════
            
            // Central flash cascade
            CustomParticles.GenericFlare(target.Center, DecayWhite * 0.7f, 0.75f, 24);
            CustomParticles.GenericFlare(target.Center, DeathGreen, 0.65f, 22);
            CustomParticles.GenericFlare(target.Center, DecayPurple, 0.55f, 20);
            CustomParticles.GenericFlare(target.Center, WitherBrown, 0.45f, 18);
            
            // Music note burst - gradient with hslToRgb
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 6f);
                float hue = HueMin + (float)i / 10f * (HueMax - HueMin);
                Color noteColor = Main.hslToRgb(hue, 0.5f, 0.5f);
                ThemedParticles.MusicNote(target.Center, noteVel, noteColor, 0.85f, 38);
            }
            
            // Heavy decay wisp burst - 6 point pattern
            for (int wisp = 0; wisp < 6; wisp++)
            {
                float wispAngle = MathHelper.TwoPi * wisp / 6f;
                Vector2 wispPos = target.Center + wispAngle.ToRotationVector2() * 18f;
                float hue = HueMin + (float)wisp / 6f * (HueMax - HueMin);
                Color wispColor = Main.hslToRgb(hue, 0.5f, 0.5f);
                CustomParticles.GenericFlare(wispPos, wispColor * 0.75f, 0.28f, 14);
            }
            
            // Halo ring expansion
            CustomParticles.HaloRing(target.Center, DeathGreen * 0.6f, 0.5f, 20);
            CustomParticles.HaloRing(target.Center, DecayPurple * 0.5f, 0.38f, 18);
            CustomParticles.HaloRing(target.Center, WitherBrown * 0.4f, 0.28f, 16);

            // Sparkle burst
            for (int i = 0; i < 12; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(7f, 7f);
                Color sparkColor = Color.Lerp(DecayPurple, DeathGreen, Main.rand.NextFloat()) * 0.65f;
                var spark = new GenericGlowParticle(target.Center, sparkVel, sparkColor, 0.32f, 22, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // Glyph burst for arcane impact
            CustomParticles.GlyphBurst(target.Center, WitherBrown, 6, 5f);
            
            // Bright impact lighting
            Lighting.AddLight(target.Center, DeathGreen.ToVector3() * 1.3f);
        }

        public override void OnKill(int timeLeft)
        {
            // ═══════════════════════════════════════════════════════════════
            // GLIMMER CASCADE DISSIPATION (TRUE_VFX_STANDARDS)
            // ═══════════════════════════════════════════════════════════════
            
            // Central glimmer - multiple layers
            for (int layer = 0; layer < 4; layer++)
            {
                float layerScale = 0.35f + layer * 0.15f;
                float layerAlpha = 0.8f - layer * 0.15f;
                Color layerColor = Color.Lerp(DecayWhite, DecayPurple, layer / 4f);
                CustomParticles.GenericFlare(Projectile.Center, layerColor * layerAlpha, layerScale, 18 - layer * 2);
            }
            
            // Expanding glow rings with hslToRgb gradient
            for (int ring = 0; ring < 4; ring++)
            {
                float hue = HueMin + (float)ring / 4f * (HueMax - HueMin);
                Color ringColor = Main.hslToRgb(hue, 0.5f, 0.5f);
                CustomParticles.HaloRing(Projectile.Center, ringColor * 0.6f, 0.35f + ring * 0.12f, 16 + ring * 3);
            }
            
            // Decay wisp burst - 6 point pattern
            for (int wisp = 0; wisp < 6; wisp++)
            {
                float wispAngle = MathHelper.TwoPi * wisp / 6f;
                Vector2 wispPos = Projectile.Center + wispAngle.ToRotationVector2() * 16f;
                float hue = HueMin + (float)wisp / 6f * (HueMax - HueMin);
                Color wispColor = Main.hslToRgb(hue, 0.5f, 0.5f);
                CustomParticles.GenericFlare(wispPos, wispColor * 0.7f, 0.22f, 12);
            }
            
            // Music note finale - grand withering crescendo
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 6f);
                float hue = HueMin + (float)i / 12f * (HueMax - HueMin);
                Color noteColor = Main.hslToRgb(hue, 0.5f, 0.55f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.9f, 42);
            }
            
            // Radial glow burst
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 9f);
                Color burstColor = Color.Lerp(DecayPurple, DeathGreen, (float)i / 16f) * 0.55f;
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor, 0.3f, 24, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
            
            // Sparkle burst
            for (int i = 0; i < 10; i++)
            {
                Vector2 sparkleVel = Main.rand.NextVector2Circular(6f, 6f);
                var sparkle = new SparkleParticle(Projectile.Center, sparkleVel, DecayWhite * 0.6f, 0.35f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Glyph circle finale
            CustomParticles.GlyphBurst(Projectile.Center, DeathGreen, 8, 5f);
            
            // Bright lighting flash
            Lighting.AddLight(Projectile.Center, DeathGreen.ToVector3() * 1.4f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/ParticleTrail2").Value;
            Texture2D flare = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare3").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            Vector2 flareOrigin = flare.Size() / 2f;

            float time = Main.GameUpdateCount * 0.05f;
            float pulse = (float)Math.Sin(time * 2f) * 0.12f + 1f;
            float fadeOut = Projectile.timeLeft < 30 ? (float)Projectile.timeLeft / 30f : 1f;
            float expansion = Projectile.ai[0] / 30f;
            
            // Colors with alpha removed (Fargos pattern)
            Color purpleBloom = DecayPurple with { A = 0 };
            Color greenBloom = DeathGreen with { A = 0 };
            Color brownBloom = WitherBrown with { A = 0 };
            Color whiteBloom = DecayWhite with { A = 0 };

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Trail rendering
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                float progress = (float)i / Projectile.oldPos.Length;
                float trailAlpha = (1f - progress) * 0.5f * fadeOut;
                float stretch = (2f + expansion) * (1f - progress * 0.3f);
                Color trailColor = Color.Lerp(purpleBloom, greenBloom, progress) * trailAlpha;
                
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                spriteBatch.Draw(texture, trailPos, null, trailColor, Projectile.oldRot[i], 
                    origin, new Vector2(0.35f * stretch, 0.18f), SpriteEffects.None, 0f);
            }

            // Wave shape - stretched horizontally with 4+ layers
            float stretch2 = 2.5f + expansion;
            
            // Layer 1: Large purple base
            spriteBatch.Draw(texture, drawPos, null, purpleBloom * 0.45f * fadeOut, Projectile.rotation, 
                origin, new Vector2(0.55f * stretch2, 0.28f) * pulse, SpriteEffects.None, 0f);
            
            // Layer 2: Spinning flare (clockwise)
            spriteBatch.Draw(flare, drawPos, null, greenBloom * 0.4f * fadeOut, time, 
                flareOrigin, 0.4f * pulse, SpriteEffects.None, 0f);
            
            // Layer 3: Spinning flare (counter-clockwise)
            spriteBatch.Draw(flare, drawPos, null, brownBloom * 0.35f * fadeOut, -time * 0.7f, 
                flareOrigin, 0.35f * pulse, SpriteEffects.None, 0f);
            
            // Layer 4: Green mid layer
            spriteBatch.Draw(texture, drawPos, null, greenBloom * 0.4f * fadeOut, Projectile.rotation, 
                origin, new Vector2(0.45f * stretch2, 0.22f) * pulse, SpriteEffects.None, 0f);
            
            // Layer 5: Another spinning flare
            spriteBatch.Draw(flare, drawPos, null, purpleBloom * 0.35f * fadeOut, time * 1.3f, 
                flareOrigin, 0.3f * pulse, SpriteEffects.None, 0f);
            
            // Layer 6: White core
            spriteBatch.Draw(texture, drawPos, null, whiteBloom * 0.5f * fadeOut, Projectile.rotation, 
                origin, new Vector2(0.28f * stretch2, 0.14f) * pulse, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
