using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.MoonlightSonata.ResonantWeapons
{
    /// <summary>
    /// Lunar Crescent Beam - A large sweeping crescent moon that expands as it travels.
    /// Unique, moon-like, beam-like, and visually stunning!
    /// </summary>
    public class MoonlightWaveProjectile : ModProjectile
    {
        // Custom invisible texture - particle-based projectile
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
            // Enable trail storage for beam effect
            ProjectileID.Sets.TrailCacheLength[Type] = 35;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 60; // Larger than before
            Projectile.height = 60;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 8; // Can hit more enemies
            Projectile.timeLeft = 90; // Lasts 1.5 seconds
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 0;
            Projectile.scale = 0.8f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            // Expand as it travels - growing crescent moon
            Projectile.scale += 0.015f;
            if (Projectile.scale > 2.2f)
                Projectile.scale = 2.2f;
            
            // Grow hitbox
            Projectile.width = (int)(60 * Projectile.scale);
            Projectile.height = (int)(60 * Projectile.scale);
            
            // Fade out near end of life
            if (Projectile.timeLeft < 30)
                Projectile.alpha += 8;

            if (Projectile.alpha > 255)
            {
                Projectile.Kill();
                return;
            }

            // Gentle homing toward nearest enemy
            if (Projectile.ai[0] == 0f)
            {
                float maxDetectDistance = 400f;
                NPC closest = null;
                float closestDist = maxDetectDistance;
                
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.CanBeChasedBy() && npc.active)
                    {
                        float dist = Vector2.Distance(Projectile.Center, npc.Center);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            closest = npc;
                        }
                    }
                }
                
                if (closest != null)
                {
                    Vector2 toTarget = closest.Center - Projectile.Center;
                    toTarget.Normalize();
                    float homingStrength = 0.08f; // Gentle homing
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), homingStrength);
                    Projectile.velocity.Normalize();
                    Projectile.velocity *= 12f;
                }
            }

            // Set rotation to match velocity direction
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Sweeping crescent moon particles along the beam
            if (Main.rand.NextBool(2))
            {
                Vector2 offset = Main.rand.NextVector2Circular(Projectile.width * 0.4f, Projectile.height * 0.4f);
                CustomParticles.SwordArcCrescent(Projectile.Center + offset, Projectile.velocity * 0.15f, 
                    UnifiedVFX.MoonlightSonata.LightBlue * 0.6f, 0.4f);
            }
            
            // Moon phase particles trailing behind
            if (Main.rand.NextBool(3))
            {
                Vector2 offset = -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(20f, 40f);
                offset += Main.rand.NextVector2Circular(10f, 10f);
                float hue = Main.rand.NextFloat();
                Color moonColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, hue);
                CustomParticles.GenericFlare(Projectile.Center + offset, moonColor * 0.7f, 0.35f, 15);
            }
            
            // Prismatic sparkle accents - moonlight glitter
            if (Main.rand.NextBool(3))
            {
                Vector2 offset = Main.rand.NextVector2Circular(Projectile.width * 0.3f, Projectile.height * 0.3f);
                CustomParticles.PrismaticSparkle(Projectile.Center + offset, Color.White, 0.3f);
            }

            // Purple-blue flame trail particles
            if (Main.rand.NextBool(2))
            {
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(Projectile.width * 0.25f, Projectile.height * 0.25f);
                Dust flame = Dust.NewDustDirect(dustPos - Vector2.One * 4, 8, 8, 
                    DustID.PurpleTorch, 0f, 0f, 100, default, 1.3f);
                flame.noGravity = true;
                flame.velocity = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f);
            }
            
            // Musical notes trailing elegantly
            if (Main.rand.NextBool(8))
            {
                Vector2 notePos = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.Zero) * 20f;
                ThemedParticles.MusicNote(notePos, Main.rand.NextVector2Circular(1.5f, 1.5f), 
                    UnifiedVFX.MoonlightSonata.MediumPurple, 0.35f, 40);
            }

            // Bright light emission - moon glow
            float intensity = 1f - (Projectile.alpha / 255f);
            Lighting.AddLight(Projectile.Center, 0.6f * intensity, 0.4f * intensity, 1f * intensity);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Music's Dissonance debuff
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 300);

            // === LUNAR IMPACT BURST ===
            UnifiedVFX.MoonlightSonata.Impact(target.Center, 1.3f);
            
            // Crescent moon explosion
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 crescentVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                CustomParticles.SwordArcCrescent(target.Center, crescentVel, 
                    UnifiedVFX.MoonlightSonata.LightBlue, 0.5f);
            }
            
            // === SIGNATURE FRACTAL FLARE BURST ===
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 flareOffset = angle.ToRotationVector2() * 35f;
                float progress = (float)i / 8f;
                Color fractalColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, progress);
                CustomParticles.GenericFlare(target.Center + flareOffset, fractalColor, 0.55f, 20);
            }
            
            // Expanding halo rings
            for (int ring = 0; ring < 4; ring++)
            {
                float progress = (float)ring / 4f;
                Color ringColor = Color.Lerp(UnifiedVFX.MoonlightSonata.MediumPurple, UnifiedVFX.MoonlightSonata.Silver, progress);
                CustomParticles.HaloRing(target.Center, ringColor, 0.4f + ring * 0.12f, 16 + ring * 2);
            }
            
            // Music notes burst on hit
            ThemedParticles.MoonlightMusicNotes(target.Center, 5, 35f);
            
            // Prismatic sparkle impact burst
            CustomParticles.PrismaticSparkleBurst(target.Center, Color.White, 8);
            
            // Purple-blue dust explosion
            for (int i = 0; i < 15; i++)
            {
                Dust dust = Dust.NewDustDirect(target.Center, 1, 1, DustID.PurpleTorch, 0f, 0f, 100, default, 1.4f);
                dust.noGravity = true;
                dust.velocity = Main.rand.NextVector2Circular(6f, 6f);
            }
            
            // Impact sound
            SoundEngine.PlaySound(SoundID.Item9 with { Volume = 0.6f, Pitch = -0.2f }, target.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            
            // Switch to additive blending for glow effect
            MagnumVFX.BeginAdditiveBlend(spriteBatch);
            
            Texture2D glowTex = Terraria.GameContent.TextureAssets.Extra[ExtrasID.SharpTears].Value;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            
            // Draw expanding crescent moon beam trail
            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) 
                    continue;
                
                float progress = (float)i / Projectile.oldPos.Length;
                float alpha = (1f - progress) * 0.9f;
                float trailScale = (1f - progress * 0.3f) * Projectile.scale * 0.8f;
                
                Vector2 drawPos = Projectile.oldPos[i] - Main.screenPosition + Projectile.Size / 2f;
                
                // Large outer moon glow - light blue
                Color outerMoon = UnifiedVFX.MoonlightSonata.LightBlue with { A = 0 };
                spriteBatch.Draw(glowTex, drawPos, null, outerMoon * alpha * 0.4f, 
                    Projectile.rotation, glowOrigin, trailScale * 2.2f, SpriteEffects.None, 0f);
                
                // Middle purple core
                Color midMoon = UnifiedVFX.MoonlightSonata.MediumPurple with { A = 0 };
                spriteBatch.Draw(glowTex, drawPos, null, midMoon * alpha * 0.6f, 
                    Projectile.rotation, glowOrigin, trailScale * 1.5f, SpriteEffects.None, 0f);
                
                // Inner crescent highlight
                Color innerMoon = Color.White with { A = 0 };
                spriteBatch.Draw(glowTex, drawPos, null, innerMoon * alpha * 0.5f, 
                    Projectile.rotation, glowOrigin, trailScale * 0.9f, SpriteEffects.None, 0f);
            }
            
            // Draw massive main crescent glow at projectile center
            float fadeAlpha = 1f - (Projectile.alpha / 255f);
            Vector2 mainPos = Projectile.Center - Main.screenPosition;
            
            // Outermost ethereal glow
            spriteBatch.Draw(glowTex, mainPos, null, UnifiedVFX.MoonlightSonata.DarkPurple with { A = 0 } * fadeAlpha * 0.3f, 
                Projectile.rotation, glowOrigin, Projectile.scale * 2.8f, SpriteEffects.None, 0f);
            
            // Large blue moon aura
            spriteBatch.Draw(glowTex, mainPos, null, UnifiedVFX.MoonlightSonata.LightBlue with { A = 0 } * fadeAlpha * 0.5f, 
                Projectile.rotation, glowOrigin, Projectile.scale * 2.0f, SpriteEffects.None, 0f);
            
            // Mid purple crescent
            spriteBatch.Draw(glowTex, mainPos, null, UnifiedVFX.MoonlightSonata.MediumPurple with { A = 0 } * fadeAlpha * 0.7f, 
                Projectile.rotation, glowOrigin, Projectile.scale * 1.3f, SpriteEffects.None, 0f);
            
            // Bright white core
            spriteBatch.Draw(glowTex, mainPos, null, Color.White with { A = 0 } * fadeAlpha * 0.8f, 
                Projectile.rotation, glowOrigin, Projectile.scale * 0.6f, SpriteEffects.None, 0f);
            
            // Reset to normal blending
            MagnumVFX.EndAdditiveBlend(spriteBatch);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // === LUNAR CRESCENT DISSIPATION ===
            UnifiedVFX.MoonlightSonata.Impact(Projectile.Center, 1.5f);
            
            // Expanding crescent moons burst
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 crescentVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                Color crescentColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, (float)i / 12f);
                CustomParticles.SwordArcCrescent(Projectile.Center, crescentVel, crescentColor * 0.7f, 0.45f);
            }
            
            // === SIGNATURE FRACTAL FLARE BURST ===
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 flareOffset = angle.ToRotationVector2() * 40f;
                float progress = (float)i / 8f;
                Color fractalColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, progress);
                CustomParticles.GenericFlare(Projectile.Center + flareOffset, fractalColor, 0.6f, 22);
            }
            
            // Cascading halo rings
            for (int ring = 0; ring < 6; ring++)
            {
                float progress = (float)ring / 6f;
                Color ringColor = Color.Lerp(UnifiedVFX.MoonlightSonata.MediumPurple, UnifiedVFX.MoonlightSonata.Silver, progress);
                CustomParticles.HaloRing(Projectile.Center, ringColor, 0.3f + ring * 0.15f, 18 + ring * 3);
            }
            
            // Music notes on death
            ThemedParticles.MoonlightMusicNotes(Projectile.Center, 8, 50f);
            
            // Prismatic sparkle explosion
            CustomParticles.PrismaticSparkleBurst(Projectile.Center, Color.White, 15);
            
            // Purple-blue dust finale
            for (int i = 0; i < 20; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.PurpleTorch, 0f, 0f, 100, default, 1.5f);
                dust.noGravity = true;
                dust.velocity = Main.rand.NextVector2Circular(6f, 6f);
            }
            
            // Dissipation sound
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f, Pitch = -0.4f }, Projectile.Center);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            float alpha = 1f - (Projectile.alpha / 255f);
            return UnifiedVFX.MoonlightSonata.MediumPurple * alpha;
        }
    }
}

