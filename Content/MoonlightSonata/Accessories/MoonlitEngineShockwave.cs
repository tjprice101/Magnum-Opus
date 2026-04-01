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

namespace MagnumOpus.Content.MoonlightSonata.Accessories
{
    /// <summary>
    /// Moonlit Engine Shockwave - A devastating AoE shockwave projectile.
    /// Features unique purple, light blue, and white particle combinations.
    /// </summary>
    [AllowLargeHitbox("Expanding shockwave requires large hitbox for AoE damage")]
    public class MoonlitEngineShockwave : ModProjectile
    {
        // Custom texture - no vanilla textures allowed
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle";
        
        private float shockwaveRadius = 0f;
        private const float MaxRadius = 200f;
        private const float ExpansionSpeed = 25f;
        private float fadeAlpha = 1f; // For fade out effect
        
        public override void SetDefaults()
        {
            Projectile.width = 400;
            Projectile.height = 400;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 40; // Longer duration for fade out
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1; // Only hit once
        }
        
        public override void AI()
        {
            // Expand the shockwave
            shockwaveRadius += ExpansionSpeed;
            if (shockwaveRadius > MaxRadius)
                shockwaveRadius = MaxRadius;
            
            float progress = shockwaveRadius / MaxRadius;
            
            // Fade out after reaching max radius
            if (shockwaveRadius >= MaxRadius)
            {
                fadeAlpha -= 0.05f; // Gradual fade
                if (fadeAlpha <= 0f)
                {
                    Projectile.Kill();
                    return;
                }
            }
            
            // === PHASE 1: Inner Core Burst - Gradient explosion near center (KEEP THIS) ===
            if (Projectile.timeLeft > 15)
            {
                // Central white flash - the gradient center the user loves
                for (int i = 0; i < 12; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                    Dust flash = Dust.NewDustPerfect(Projectile.Center, DustID.SparksMech, vel, 0, MagnumThemePalettes.MoonlightMoonWhite, 2f);
                    flash.noGravity = true;
                    flash.fadeIn = 1.3f;
                }
                
                // Purple/blue gradient core - fading from white center to purple edge
                for (int i = 0; i < 15; i++)
                {
                    float angle = MathHelper.TwoPi * i / 15f;
                    float dist = Main.rand.NextFloat(20f, 60f);
                    Vector2 pos = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * dist;
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 4f;
                    
                    // Gradient: closer to center = lighter, further = more purple
                    int dustType = dist < 35f ? DustID.IceTorch : DustID.PurpleTorch;
                    float scale = 2f - dist / 60f;
                    
                    Dust core = Dust.NewDustPerfect(pos, dustType, vel, 0, default, scale);
                    core.noGravity = true;
                    core.fadeIn = 1.2f;
                }
            }
            
            // === PHASE 2: Subtle Expanding Ring - Much less intense ===
            // Only a few particles for the ring, not overwhelming
            int ringParticles = (int)(8 + progress * 5);
            for (int i = 0; i < ringParticles; i++)
            {
                float angle = MathHelper.TwoPi * i / ringParticles + Main.GameUpdateCount * 0.1f;
                Vector2 ringPos = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * shockwaveRadius;
                
                Vector2 outwardVel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 2f;
                
                // Subtle particles - mostly light blue, occasional white
                int dustType = i % 3 == 0 ? DustID.SparksMech : DustID.IceTorch;
                float scale = 1.2f * (1f - progress * 0.4f);
                
                Dust ring = Dust.NewDustPerfect(ringPos, dustType, outwardVel, 0, dustType == DustID.SparksMech ? MagnumThemePalettes.MoonlightMoonWhite : default, scale);
                ring.noGravity = true;
            }
            
            // Lighting - palette-driven Moonlight Sonata purple
            Vector3 lightVec = MagnumThemePalettes.MoonlightLavender.ToVector3();
            float lightIntensity = (1f - progress * 0.5f) * 0.5f;
            Lighting.AddLight(Projectile.Center, lightVec.X * lightIntensity, lightVec.Y * lightIntensity, lightVec.Z * lightIntensity);
            
            // Musical notation - Engine shockwave melody
            if (Projectile.timeLeft % 6 == 0)
            {
                float noteAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 notePos = Projectile.Center + noteAngle.ToRotationVector2() * shockwaveRadius * 0.4f;
                MoonlightVFXLibrary.SpawnMusicNotes(notePos, 1, 8f, 0.6f, 0.75f, 28);
            }
        }
        
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Circle collision based on current radius
            Vector2 targetCenter = targetHitbox.Center.ToVector2();
            float dist = Vector2.Distance(Projectile.Center, targetCenter);
            
            // Hit if within the ring (between inner and outer radius)
            float innerHit = shockwaveRadius * 0.3f;
            float outerHit = shockwaveRadius + 30f;
            
            return dist >= innerHit && dist <= outerHit;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Impact burst on each enemy hit
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 4f;
                int dustType = i % 2 == 0 ? DustID.PurpleTorch : DustID.IceTorch;
                Dust impact = Dust.NewDustPerfect(target.Center, dustType, vel, 0, default, 1.8f);
                impact.noGravity = true;
            }
            
            // Musical impact - Engine's moonlit resonance
            MoonlightVFXLibrary.SpawnMusicNotes(target.Center, 3, 12f, 0.7f, 0.9f, 22);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            // No custom drawing - particles handle all visuals
            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }
    }
}
