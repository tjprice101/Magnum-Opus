using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems.VFX
{
    /*
     * ============================================================
     *  EXAMPLE BEAM PROJECTILES - CALAMITY-STYLE IMPLEMENTATION
     * ============================================================
     *  
     *  These examples demonstrate how to use BaseBeamProjectile
     *  for each major theme. Copy and modify these patterns for
     *  your actual weapon projectiles.
     *  
     *  Each example shows different beam behaviors:
     *  - EroicaDeathRay: Fixed-length forward beam
     *  - FateCosmicLaser: Homing beam to nearest enemy
     *  - LaCampanellaInferno: Sweeping/rotating beam
     *  - SwanLakePrism: Bouncing/reflecting beam
     *  - MoonlightSonataRay: Pulsing ethereal beam
     *  - EnigmaVoidBeam: Erratic, reality-warping beam
     *  
     * ============================================================
     */

    #region EROICA - HEROIC DEATH RAY
    
    /// <summary>
    /// Eroica theme: Heroic, triumphant beam with sakura petal accents.
    /// Fixed forward projection with golden-scarlet gradient.
    /// </summary>
    public class EroicaDeathRayExample : BaseBeamProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.LastPrismLaser;
        
        // Theme configuration
        public override string ThemeName => "Eroica";
        public override float BeamWidth => 50f;
        public override CalamityBeamSystem.WidthStyle WidthStyle => CalamityBeamSystem.WidthStyle.QuadraticBump;
        public override float BloomMultiplier => 2.8f;
        public override float MaxBeamLength => 1800f;
        
        // Beam duration
        private const int BeamDuration = 180; // 3 seconds
        
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = BeamDuration;
        }
        
        protected override Vector2 GetBeamEndPoint()
        {
            // Simple forward projection
            Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            return Projectile.Center + direction * MaxBeamLength;
        }
        
        protected override void OnBeamAI()
        {
            // Keep beam anchored to owner
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }
            
            // Track to mouse position
            Vector2 toMouse = Main.MouseWorld - owner.Center;
            Projectile.velocity = toMouse.SafeNormalize(Vector2.UnitX) * 10f;
            Projectile.Center = owner.Center;
            
            // Sakura petals along beam (theme-specific)
            if (Main.rand.NextBool(8))
            {
                Vector2 petalPos = Vector2.Lerp(BeamStart, BeamEnd, Main.rand.NextFloat());
                ThemedParticles.SakuraPetals(petalPos, 1, 15f);
            }
        }
    }
    
    #endregion
    
    #region FATE - COSMIC LASER (HOMING)
    
    /// <summary>
    /// Fate theme: Cosmic, reality-bending beam that seeks enemies.
    /// Features chromatic aberration and glyph particles.
    /// </summary>
    public class FateCosmicLaserExample : BaseBeamProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.LastPrismLaser;
        
        public override string ThemeName => "Fate";
        public override float BeamWidth => 35f;
        public override CalamityBeamSystem.WidthStyle WidthStyle => CalamityBeamSystem.WidthStyle.PulsingWidth;
        public override float BloomMultiplier => 3.5f;
        public override float MaxBeamLength => 2000f;
        public override int SegmentCount => 60;
        
        private NPC _targetNPC;
        private float _homingStrength = 0.03f;
        
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = 240;
            
            // Override profile for pulsing
            RebuildBeamProfile();
        }
        
        protected override Vector2 GetBeamEndPoint()
        {
            // Home toward target NPC
            if (_targetNPC != null && _targetNPC.active && !_targetNPC.friendly)
            {
                return _targetNPC.Center;
            }
            
            // Fallback: Forward projection
            return Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * MaxBeamLength;
        }
        
        protected override void OnBeamAI()
        {
            // Find closest hostile NPC
            UpdateTarget();
            
            // Smoothly rotate toward target
            if (_targetNPC != null && _targetNPC.active)
            {
                Vector2 toTarget = (_targetNPC.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity.SafeNormalize(Vector2.UnitX), 
                    toTarget, _homingStrength).SafeNormalize(Vector2.UnitX) * 10f;
            }
            
            // FATE-SPECIFIC VFX: Glyphs and star sparkles
            if (Main.rand.NextBool(6))
            {
                Vector2 glyphPos = Vector2.Lerp(BeamStart, BeamEnd, Main.rand.NextFloat());
                CustomParticles.GlyphBurst(glyphPos, MagnumThemePalettes.FatePink, 2, 3f);
            }
            
            if (Main.rand.NextBool(4))
            {
                Vector2 starPos = Vector2.Lerp(BeamStart, BeamEnd, Main.rand.NextFloat());
                CustomParticles.GenericFlare(starPos, Color.White, 0.3f, 15);
            }
        }
        
        private void UpdateTarget()
        {
            if (_targetNPC != null && _targetNPC.active && !_targetNPC.friendly) return;
            
            float closestDist = float.MaxValue;
            _targetNPC = null;
            
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || npc.CountsAsACritter) continue;
                
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist && dist < MaxBeamLength)
                {
                    closestDist = dist;
                    _targetNPC = npc;
                }
            }
        }
    }
    
    #endregion
    
    #region LA CAMPANELLA - INFERNAL SWEEP
    
    /// <summary>
    /// La Campanella theme: Infernal fire beam that sweeps/rotates.
    /// Features heavy smoke and orange flame effects.
    /// </summary>
    public class LaCampanellaInfernoExample : BaseBeamProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.LastPrismLaser;
        
        public override string ThemeName => "LaCampanella";
        public override float BeamWidth => 60f;
        public override CalamityBeamSystem.WidthStyle WidthStyle => CalamityBeamSystem.WidthStyle.SourceTaper;
        public override float BloomMultiplier => 3.0f;
        public override float MaxBeamLength => 1600f;
        
        private float _rotation;
        private float _rotationSpeed = 0.02f;
        private float _sweepAngle = MathHelper.PiOver2; // 90 degree sweep
        
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = false;
            Projectile.hostile = true; // Boss attack
            Projectile.DamageType = DamageClass.Default;
            Projectile.timeLeft = 300; // 5 seconds
        }
        
        protected override Vector2 GetBeamEndPoint()
        {
            // Calculate sweeping direction
            float baseAngle = Projectile.velocity.ToRotation();
            float currentAngle = baseAngle + MathF.Sin(_rotation) * _sweepAngle;
            
            Vector2 direction = currentAngle.ToRotationVector2();
            return Projectile.Center + direction * MaxBeamLength;
        }
        
        protected override void OnBeamAI()
        {
            // Advance sweep rotation
            _rotation += _rotationSpeed;
            
            // Speed up over time (Calamity pattern)
            _rotationSpeed += 0.0001f;
            
            // LA CAMPANELLA-SPECIFIC VFX: Heavy smoke
            if (Main.rand.NextBool(3))
            {
                Vector2 smokePos = Vector2.Lerp(BeamStart, BeamEnd, Main.rand.NextFloat());
                Dust smoke = Dust.NewDustPerfect(smokePos, DustID.Smoke,
                    Main.rand.NextVector2Circular(2f, 2f), 150, Color.Black, 2f);
                smoke.noGravity = true;
            }
            
            // Flame particles
            if (Main.rand.NextBool(4))
            {
                Vector2 flamePos = Vector2.Lerp(BeamStart, BeamEnd, Main.rand.NextFloat());
                Dust flame = Dust.NewDustPerfect(flamePos, DustID.Torch,
                    Main.rand.NextVector2Circular(3f, 3f), 0, MagnumThemePalettes.CampanellaOrange, 1.5f);
                flame.noGravity = true;
            }
        }
    }
    
    #endregion
    
    #region SWAN LAKE - PRISMATIC PRISM
    
    /// <summary>
    /// Swan Lake theme: Elegant prismatic beam with rainbow colors.
    /// Features feather particles and graceful movement.
    /// </summary>
    public class SwanLakePrismExample : BaseBeamProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.LastPrismLaser;
        
        public override string ThemeName => "SwanLake";
        public override float BeamWidth => 40f;
        public override CalamityBeamSystem.WidthStyle WidthStyle => CalamityBeamSystem.WidthStyle.QuadraticBump;
        public override float BloomMultiplier => 2.5f;
        public override float MaxBeamLength => 2000f;
        public override float TextureScrollSpeed => 4f; // Faster scroll for prismatic
        
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = 200;
        }
        
        protected override Vector2 GetBeamEndPoint()
        {
            // Forward with slight wave motion
            float time = Main.GlobalTimeWrappedHourly * 3f;
            float waveOffset = MathF.Sin(time) * 50f;
            
            Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitX) * MaxBeamLength;
            Vector2 perpendicular = new Vector2(-forward.Y, forward.X).SafeNormalize(Vector2.Zero);
            
            return Projectile.Center + forward + perpendicular * waveOffset;
        }
        
        protected override void OnBeamAI()
        {
            // SWAN LAKE-SPECIFIC VFX: Rainbow sparkles
            if (Main.rand.NextBool(5))
            {
                Vector2 sparklePos = Vector2.Lerp(BeamStart, BeamEnd, Main.rand.NextFloat());
                float hue = Main.GlobalTimeWrappedHourly + Main.rand.NextFloat();
                Color rainbow = Main.hslToRgb(hue % 1f, 1f, 0.75f);
                CustomParticles.GenericFlare(sparklePos, rainbow, 0.4f, 20);
            }
            
            // Feather drift
            if (Main.rand.NextBool(15))
            {
                Vector2 featherPos = Vector2.Lerp(BeamStart, BeamEnd, Main.rand.NextFloat());
                ThemedParticles.SwanFeatherDrift(featherPos, Color.White, 0.5f);
            }
        }
    }
    
    #endregion
    
    #region MOONLIGHT SONATA - ETHEREAL RAY
    
    /// <summary>
    /// Moonlight Sonata theme: Soft, ethereal beam with lunar glow.
    /// Features gentle pulsing and purple mist.
    /// </summary>
    public class MoonlightSonataRayExample : BaseBeamProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.LastPrismLaser;
        
        public override string ThemeName => "MoonlightSonata";
        public override float BeamWidth => 45f;
        public override CalamityBeamSystem.WidthStyle WidthStyle => CalamityBeamSystem.WidthStyle.PulsingWidth;
        public override float BloomMultiplier => 3.2f;
        public override float CoreMultiplier => 0.25f;
        public override float MaxBeamLength => 1500f;
        
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = 150;
        }
        
        protected override Vector2 GetBeamEndPoint()
        {
            return Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * MaxBeamLength;
        }
        
        protected override void OnBeamAI()
        {
            // MOONLIGHT-SPECIFIC VFX: Purple mist
            if (Main.rand.NextBool(6))
            {
                Vector2 mistPos = Vector2.Lerp(BeamStart, BeamEnd, Main.rand.NextFloat());
                Dust mist = Dust.NewDustPerfect(mistPos, DustID.PurpleTorch,
                    Main.rand.NextVector2Circular(1f, 1f), 100, MagnumThemePalettes.MoonlightPurple, 1.2f);
                mist.noGravity = true;
                mist.fadeIn = 1.5f;
            }
            
            // Silver sparkles
            if (Main.rand.NextBool(8))
            {
                Vector2 sparklePos = Vector2.Lerp(BeamStart, BeamEnd, Main.rand.NextFloat());
                CustomParticles.GenericFlare(sparklePos, MagnumThemePalettes.MoonlightSilver, 0.35f, 18);
            }
            
            // Music notes (this is a music mod!)
            if (Main.rand.NextBool(12))
            {
                Vector2 notePos = Vector2.Lerp(BeamStart, BeamEnd, Main.rand.NextFloat());
                ThemedParticles.MoonlightMusicNotes(notePos, 1, 12f);
            }
        }
    }
    
    #endregion
    
    #region ENIGMA - VOID BEAM
    
    /// <summary>
    /// Enigma theme: Reality-warping void beam with erratic behavior.
    /// Features watching eyes and glyph circles.
    /// </summary>
    public class EnigmaVoidBeamExample : BaseBeamProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.LastPrismLaser;
        
        public override string ThemeName => "EnigmaVariations";
        public override float BeamWidth => 35f;
        public override CalamityBeamSystem.WidthStyle WidthStyle => CalamityBeamSystem.WidthStyle.QuadraticBump;
        public override float BloomMultiplier => 2.8f;
        public override float MaxBeamLength => 1400f;
        public override int SegmentCount => 70; // More segments for erratic wave
        
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = false;
            Projectile.hostile = true; // Boss attack
            Projectile.DamageType = DamageClass.Default;
            Projectile.timeLeft = 180;
        }
        
        protected override Vector2 GetBeamEndPoint()
        {
            // Erratic warping along beam path
            float time = Main.GlobalTimeWrappedHourly * 5f;
            float warp = MathF.Sin(time * 2.3f) * MathF.Cos(time * 1.7f) * 80f;
            
            Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitX) * MaxBeamLength;
            Vector2 perpendicular = new Vector2(-forward.Y, forward.X).SafeNormalize(Vector2.Zero);
            
            return Projectile.Center + forward + perpendicular * warp;
        }
        
        protected override void OnBeamAI()
        {
            // ENIGMA-SPECIFIC VFX: Watching eyes
            if (Main.rand.NextBool(20))
            {
                Vector2 eyePos = Vector2.Lerp(BeamStart, BeamEnd, Main.rand.NextFloat());
                Player target = Main.player[Player.FindClosest(eyePos, 1, 1)];
                CustomParticles.EnigmaEyeGaze(eyePos, MagnumThemePalettes.EnigmaPurple, 0.45f, target.Center);
            }
            
            // Void glyphs
            if (Main.rand.NextBool(10))
            {
                Vector2 glyphPos = Vector2.Lerp(BeamStart, BeamEnd, Main.rand.NextFloat());
                CustomParticles.GlyphBurst(glyphPos, MagnumThemePalettes.EnigmaGreen, 3, 4f);
            }
            
            // Green flame wisps
            if (Main.rand.NextBool(4))
            {
                Vector2 wispPos = Vector2.Lerp(BeamStart, BeamEnd, Main.rand.NextFloat());
                Dust wisp = Dust.NewDustPerfect(wispPos, DustID.CursedTorch,
                    Main.rand.NextVector2Circular(2f, 2f), 0, MagnumThemePalettes.EnigmaGreen, 1.3f);
                wisp.noGravity = true;
            }
        }
    }
    
    #endregion
}
