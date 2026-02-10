using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// ENEMY SIGNATURE VFX - Unique Visual Effects for Each Enemy Type
    /// 
    /// Each enemy in MagnumOpus gets a signature set of VFX based on their theme.
    /// These automatically enhance spawn, attack, and death effects.
    /// 
    /// Enemy Types by Theme:
    /// - Eroica: Centurions, Flames of Valor, Behemoths, Blitzers
    /// - Fate: Heralds of Fate
    /// - Swan Lake: Shattered Prima
    /// - Moonlight Sonata: Waning Deer, Shards
    /// - La Campanella: Crawlers of the Bell
    /// - Enigma: Mystery's End
    /// </summary>
    public static class EnemySignatureVFX
    {
        #region Eroica Enemies
        
        /// <summary>
        /// Centurion attack VFX - heroic warrior strikes.
        /// </summary>
        public static void EroicaCenturionStrike(Vector2 position, Vector2 direction, float intensity = 1f)
        {
            // Golden warrior flash
            CustomParticles.GenericFlare(position, new Color(255, 200, 80), 0.6f * intensity, 15);
            CustomParticles.GenericFlare(position, new Color(200, 50, 50), 0.4f * intensity, 18);
            
            // Sparks in strike direction
            for (int i = 0; i < 5; i++)
            {
                float angle = direction.ToRotation() + Main.rand.NextFloat(-0.4f, 0.4f);
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                
                Dust spark = Dust.NewDustPerfect(position, DustID.Enchanted_Gold, sparkVel, 0, Color.Gold, 0.9f);
                spark.noGravity = true;
            }
            
            CustomParticles.HaloRing(position, new Color(255, 200, 80), 0.25f * intensity, 12);
        }
        
        /// <summary>
        /// Flame of Valor attack - heroic fire.
        /// </summary>
        public static void EroicaFlameAttack(Vector2 position, Vector2 direction, float intensity = 1f)
        {
            // Fire core
            CustomParticles.GenericFlare(position, Color.White, 0.5f * intensity, 12);
            CustomParticles.GenericFlare(position, new Color(255, 150, 50), 0.4f * intensity, 15);
            
            // Fire stream
            for (int i = 0; i < 8; i++)
            {
                float angle = direction.ToRotation() + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 fireVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 10f);
                
                Dust fire = Dust.NewDustPerfect(position, DustID.Torch, fireVel, 0, new Color(255, 100, 50), 1.2f);
                fire.noGravity = true;
            }
        }
        
        /// <summary>
        /// Behemoth of Valor heavy strike.
        /// </summary>
        public static void EroicaBehemothStrike(Vector2 position, float intensity = 1f)
        {
            // Heavy impact
            CustomParticles.GenericFlare(position, Color.White, 1f * intensity, 18);
            CustomParticles.GenericFlare(position, new Color(200, 50, 50), 0.8f * intensity, 20);
            
            // Ground crack effect
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 crackVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                crackVel.Y = Math.Abs(crackVel.Y) * -0.5f; // Bias upward
                
                Dust crack = Dust.NewDustPerfect(position, DustID.Enchanted_Gold, crackVel, 0, Color.Gold, 1.3f);
                crack.noGravity = false;
            }
            
            // Shockwave halos
            for (int ring = 0; ring < 3; ring++)
            {
                CustomParticles.HaloRing(position, Color.Lerp(new Color(255, 200, 80), new Color(200, 50, 50), ring / 3f),
                    0.35f * intensity + ring * 0.1f, 14 + ring * 2);
            }
        }
        
        /// <summary>
        /// Funeral Blitzer dash attack.
        /// </summary>
        public static void EroicaBlitzerDash(Vector2 position, Vector2 direction, float intensity = 1f)
        {
            // Speed streak
            CustomParticles.GenericFlare(position, new Color(255, 200, 80), 0.5f * intensity, 12);
            
            // Afterimage trail
            for (int i = 0; i < 4; i++)
            {
                Vector2 trailPos = position - direction * i * 10f;
                float fade = 1f - (i / 4f);
                CustomParticles.GenericFlare(trailPos, new Color(200, 50, 50) * fade, 0.3f * intensity * fade, 8);
            }
            
            // Sparks
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(4f, 4f) - direction * 2f;
                Dust spark = Dust.NewDustPerfect(position, DustID.Enchanted_Gold, sparkVel, 0, Color.Gold, 0.8f);
                spark.noGravity = true;
            }
        }
        
        #endregion
        
        #region Fate Enemies
        
        /// <summary>
        /// Herald of Fate attack - cosmic energy.
        /// </summary>
        public static void FateHeraldAttack(Vector2 position, Vector2 direction, float intensity = 1f)
        {
            // Cosmic core
            CustomParticles.GenericFlare(position, Color.White, 0.7f * intensity, 15);
            CustomParticles.GenericFlare(position, new Color(180, 50, 100), 0.5f * intensity, 18);
            
            // Glyph accent
            CustomParticles.Glyph(position, new Color(200, 80, 120), 0.35f * intensity, Main.rand.Next(12));
            
            // Star sparkles
            for (int i = 0; i < 5; i++)
            {
                float angle = direction.ToRotation() + Main.rand.NextFloat(-0.5f, 0.5f);
                Vector2 starVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 9f);
                CustomParticles.GenericFlare(position + starVel * 2f, Color.White, 0.2f, 12);
            }
            
            CustomParticles.HaloRing(position, new Color(180, 50, 100), 0.3f * intensity, 14);
        }
        
        /// <summary>
        /// Herald of Fate cosmic gaze.
        /// </summary>
        public static void FateHeraldGaze(Vector2 eyePosition, Vector2 targetDirection, float intensity = 1f)
        {
            // Eye glow
            CustomParticles.GenericFlare(eyePosition, new Color(255, 60, 80), 0.4f * intensity, 10);
            
            // Glyph watching
            CustomParticles.Glyph(eyePosition, new Color(200, 80, 120), 0.3f * intensity, 8); // Eye glyph
            
            // Focused beam particles
            for (int i = 0; i < 3; i++)
            {
                Vector2 beamPos = eyePosition + targetDirection * (i + 1) * 15f;
                CustomParticles.GenericFlare(beamPos, new Color(180, 50, 100) * 0.5f, 0.15f, 8);
            }
        }
        
        #endregion
        
        #region Swan Lake Enemies
        
        /// <summary>
        /// Shattered Prima attack - fractured elegance.
        /// </summary>
        public static void SwanLakePrimaStrike(Vector2 position, Vector2 direction, float intensity = 1f)
        {
            // Monochrome flash
            CustomParticles.GenericFlare(position, Color.White, 0.6f * intensity, 15);
            CustomParticles.GenericFlare(position, new Color(30, 30, 40), 0.4f * intensity, 18);
            
            // Feather burst
            for (int i = 0; i < 4; i++)
            {
                float angle = direction.ToRotation() + Main.rand.NextFloat(-0.4f, 0.4f);
                Vector2 featherPos = position + angle.ToRotationVector2() * Main.rand.NextFloat(15f, 35f);
                CustomParticles.SwanFeatherDrift(featherPos, Main.rand.NextBool() ? Color.White : new Color(20, 20, 30), 0.35f);
            }
            
            // Rainbow sparkle accent
            float hue = (Main.GameUpdateCount * 0.02f) % 1f;
            CustomParticles.PrismaticSparkle(position + Main.rand.NextVector2Circular(20f, 20f), 
                Main.hslToRgb(hue, 1f, 0.8f), 0.25f);
            
            CustomParticles.HaloRing(position, Color.White, 0.25f * intensity, 12);
        }
        
        /// <summary>
        /// Shattered Prima dance effect - graceful movement.
        /// </summary>
        public static void SwanLakePrimaDance(Vector2 position, float intensity = 1f)
        {
            // Graceful swirl
            float angle = Main.GameUpdateCount * 0.05f;
            for (int i = 0; i < 3; i++)
            {
                float swirl = angle + MathHelper.TwoPi * i / 3f;
                Vector2 swirlPos = position + swirl.ToRotationVector2() * 25f;
                
                bool isWhite = i % 2 == 0;
                CustomParticles.GenericFlare(swirlPos, isWhite ? Color.White : new Color(30, 30, 40), 0.2f, 10);
            }
            
            // Occasional feather
            if (Main.rand.NextBool(5))
            {
                CustomParticles.SwanFeatherDrift(position + Main.rand.NextVector2Circular(30f, 30f), 
                    Main.rand.NextBool() ? Color.White : new Color(20, 20, 30), 0.3f);
            }
        }
        
        #endregion
        
        #region Moonlight Sonata Enemies
        
        /// <summary>
        /// Waning Deer attack - ethereal lunar strike.
        /// </summary>
        public static void MoonlightDeerAttack(Vector2 position, Vector2 direction, float intensity = 1f)
        {
            // Lunar flash
            CustomParticles.GenericFlare(position, Color.White, 0.5f * intensity, 14);
            CustomParticles.GenericFlare(position, new Color(135, 206, 250), 0.35f * intensity, 16);
            
            // Ethereal mist trail
            for (int i = 0; i < 5; i++)
            {
                Vector2 mistPos = position + direction * i * 8f + Main.rand.NextVector2Circular(10f, 10f);
                Dust mist = Dust.NewDustPerfect(mistPos, DustID.PurpleTorch, Main.rand.NextVector2Circular(1f, 1f), 100, 
                    new Color(75, 0, 130), 0.6f);
                mist.noGravity = true;
                mist.fadeIn = 1.1f;
            }
            
            CustomParticles.HaloRing(position, new Color(75, 0, 130), 0.25f * intensity, 12);
        }
        
        /// <summary>
        /// Waning Deer ambient glow.
        /// </summary>
        public static void MoonlightDeerGlow(Vector2 position, float intensity = 1f)
        {
            // Soft lunar glow
            if (Main.rand.NextBool(4))
            {
                CustomParticles.GenericFlare(position + Main.rand.NextVector2Circular(20f, 20f), 
                    new Color(135, 206, 250) * 0.4f, 0.2f, 15);
            }
            
            // Ethereal mist
            if (Main.rand.NextBool(6))
            {
                Dust mist = Dust.NewDustPerfect(position + Main.rand.NextVector2Circular(15f, 15f), DustID.PurpleTorch, 
                    Main.rand.NextVector2Circular(0.5f, 0.5f), 100, new Color(75, 0, 130), 0.5f);
                mist.noGravity = true;
                mist.fadeIn = 1.2f;
            }
        }
        
        /// <summary>
        /// Shards of Moonlit Tempo attack.
        /// </summary>
        public static void MoonlightShardAttack(Vector2 position, Vector2 direction, float intensity = 1f)
        {
            // Shard flash
            CustomParticles.GenericFlare(position, new Color(220, 220, 235), 0.5f * intensity, 12);
            
            // Crystal shards flying
            for (int i = 0; i < 4; i++)
            {
                float angle = direction.ToRotation() + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 shardVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 10f);
                
                Dust shard = Dust.NewDustPerfect(position, DustID.GemAmethyst, shardVel, 0, new Color(135, 206, 250), 0.8f);
                shard.noGravity = true;
            }
        }
        
        #endregion
        
        #region La Campanella Enemies
        
        /// <summary>
        /// Crawler of the Bell attack - infernal strike.
        /// </summary>
        public static void CampanellaCrawlerAttack(Vector2 position, Vector2 direction, float intensity = 1f)
        {
            // Fire flash
            CustomParticles.GenericFlare(position, new Color(255, 140, 40), 0.5f * intensity, 14);
            CustomParticles.GenericFlare(position, new Color(255, 100, 0), 0.35f * intensity, 16);
            
            // Fire burst
            for (int i = 0; i < 6; i++)
            {
                float angle = direction.ToRotation() + Main.rand.NextFloat(-0.4f, 0.4f);
                Vector2 fireVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 9f);
                
                Dust fire = Dust.NewDustPerfect(position, DustID.Torch, fireVel, 0, new Color(255, 100, 50), 1.1f);
                fire.noGravity = true;
            }
            
            // Smoke puff
            Dust smoke = Dust.NewDustPerfect(position, DustID.Smoke, Main.rand.NextVector2Circular(1f, 1f) + new Vector2(0, -1f), 
                100, new Color(50, 40, 40), 1f);
            smoke.noGravity = false;
            
            CustomParticles.HaloRing(position, new Color(255, 140, 40), 0.25f * intensity, 12);
        }
        
        /// <summary>
        /// Crawler ember trail while moving.
        /// </summary>
        public static void CampanellaCrawlerTrail(Vector2 position, float intensity = 1f)
        {
            // Ember trail
            if (Main.rand.NextBool(3))
            {
                Vector2 emberVel = Main.rand.NextVector2Circular(1f, 1f) + new Vector2(0, -1f);
                Dust ember = Dust.NewDustPerfect(position + Main.rand.NextVector2Circular(8f, 8f), DustID.Torch, emberVel, 0, 
                    new Color(255, 100 + Main.rand.Next(100), 0), 0.8f);
                ember.noGravity = true;
            }
            
            // Occasional smoke
            if (Main.rand.NextBool(8))
            {
                Dust smoke = Dust.NewDustPerfect(position + Main.rand.NextVector2Circular(10f, 10f), DustID.Smoke, 
                    new Vector2(0, -0.5f), 80, new Color(40, 30, 30), 0.7f);
                smoke.noGravity = false;
            }
        }
        
        #endregion
        
        #region Enigma Enemies
        
        /// <summary>
        /// Mystery's End attack - void strike.
        /// </summary>
        public static void EnigmaMysteryAttack(Vector2 position, Vector2 direction, float intensity = 1f)
        {
            // Void flash
            CustomParticles.GenericFlare(position, new Color(50, 220, 100), 0.5f * intensity, 14);
            CustomParticles.GenericFlare(position, new Color(140, 60, 200), 0.35f * intensity, 16);
            
            // Glyph accent
            CustomParticles.Glyph(position, new Color(140, 60, 200), 0.3f * intensity, Main.rand.Next(12));
            
            // Mystery particles
            for (int i = 0; i < 5; i++)
            {
                float angle = direction.ToRotation() + Main.rand.NextFloat(-0.5f, 0.5f);
                Vector2 mysteryVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                
                Color mysteryColor = Main.rand.NextBool() ? new Color(50, 220, 100) : new Color(140, 60, 200);
                CustomParticles.GenericFlare(position + mysteryVel * 2f, mysteryColor * 0.6f, 0.2f, 12);
            }
            
            CustomParticles.HaloRing(position, new Color(50, 220, 100), 0.25f * intensity, 12);
        }
        
        /// <summary>
        /// Mystery's End ambient void effect.
        /// </summary>
        public static void EnigmaMysteryAmbient(Vector2 position, float intensity = 1f)
        {
            // Void particles drifting
            if (Main.rand.NextBool(5))
            {
                CustomParticles.GenericFlare(position + Main.rand.NextVector2Circular(20f, 20f), 
                    new Color(140, 60, 200) * 0.4f, 0.15f, 15);
            }
            
            // Occasional glyph
            if (Main.rand.NextBool(15))
            {
                CustomParticles.Glyph(position + Main.rand.NextVector2Circular(25f, 25f), 
                    new Color(50, 220, 100), 0.25f, Main.rand.Next(12));
            }
        }
        
        #endregion
        
        #region Generic Enemy Effects
        
        /// <summary>
        /// Generic enemy spawn effect.
        /// </summary>
        public static void GenericEnemySpawn(Vector2 position, string theme, float intensity = 1f)
        {
            var style = UniqueWeaponVFXStyles.GetStyle(theme);
            
            // Spawn flash
            CustomParticles.GenericFlare(position, style.Fog.PrimaryColor, 0.6f * intensity, 18);
            CustomParticles.GenericFlare(position, style.Fog.SecondaryColor, 0.4f * intensity, 20);
            
            // Particle burst
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                CustomParticles.GenericFlare(position + vel * 3f, style.Fog.PrimaryColor, 0.25f, 12);
            }
            
            // Halo
            CustomParticles.HaloRing(position, style.Fog.PrimaryColor, 0.3f * intensity, 15);
            
            // Fog puff
            WeaponFogVFX.SpawnAttackFog(position, theme, 0.5f * intensity, Vector2.Zero);
        }
        
        /// <summary>
        /// Generic enemy death effect.
        /// </summary>
        public static void GenericEnemyDeath(Vector2 position, string theme, float intensity = 1f)
        {
            var style = UniqueWeaponVFXStyles.GetStyle(theme);
            
            // Death flash
            CustomParticles.GenericFlare(position, Color.White, 0.8f * intensity, 20);
            CustomParticles.GenericFlare(position, style.Fog.PrimaryColor, 0.6f * intensity, 22);
            CustomParticles.GenericFlare(position, style.Fog.SecondaryColor, 0.4f * intensity, 24);
            
            // Particle explosion
            for (int i = 0; i < 15; i++)
            {
                float angle = MathHelper.TwoPi * i / 15f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 10f);
                Color particleColor = Color.Lerp(style.Fog.PrimaryColor, style.Fog.SecondaryColor, Main.rand.NextFloat());
                
                CustomParticles.GenericFlare(position + vel * 2f, particleColor, 0.3f + Main.rand.NextFloat(0.2f), 20);
            }
            
            // Halos
            for (int ring = 0; ring < 4; ring++)
            {
                Color ringColor = Color.Lerp(style.Fog.PrimaryColor, style.Fog.SecondaryColor, ring / 4f);
                CustomParticles.HaloRing(position, ringColor, 0.3f * intensity + ring * 0.1f, 14 + ring * 2);
            }
            
            // Fog burst
            WeaponFogVFX.SpawnAttackFog(position, theme, 0.8f * intensity, Vector2.Zero);
            LightBeamImpactVFX.SpawnImpact(position, theme, 0.7f * intensity);
        }
        
        /// <summary>
        /// Generic enemy hit effect.
        /// </summary>
        public static void GenericEnemyHit(Vector2 position, Vector2 hitDirection, string theme, bool isCrit, float intensity = 1f)
        {
            var style = UniqueWeaponVFXStyles.GetStyle(theme);
            
            float critMult = isCrit ? 1.5f : 1f;
            
            // Hit flash
            CustomParticles.GenericFlare(position, style.Fog.PrimaryColor, 0.4f * intensity * critMult, 12);
            
            // Sparks in hit direction
            int sparkCount = isCrit ? 8 : 4;
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = hitDirection.ToRotation() + Main.rand.NextFloat(-0.4f, 0.4f);
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f) * critMult;
                
                CustomParticles.GenericFlare(position + sparkVel * 2f, style.Fog.SecondaryColor, 0.2f, 10);
            }
            
            // Halo on crit
            if (isCrit)
            {
                CustomParticles.HaloRing(position, style.Fog.PrimaryColor, 0.25f * intensity, 10);
            }
        }
        
        #endregion
    }
}
