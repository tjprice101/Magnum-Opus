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
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow4";
        
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
            
            // === IRIDESCENT WINGSPAN-STYLE HEAVY DUST TRAILS ===
            // Heavy purple dust trail #1
            float trailProgress1 = Main.rand.NextFloat();
            Color purpleGradient = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.MediumPurple, trailProgress1);
            Vector2 trailOffset1 = Main.rand.NextVector2Circular(Projectile.width * 0.3f, Projectile.height * 0.3f);
            Dust heavyPurple = Dust.NewDustDirect(Projectile.Center + trailOffset1 - Vector2.One * 4, 8, 8, 
                DustID.PurpleTorch, -Projectile.velocity.X * 0.15f, -Projectile.velocity.Y * 0.15f, 100, purpleGradient, 1.5f);
            heavyPurple.noGravity = true;
            heavyPurple.fadeIn = 1.4f;
            
            // Heavy blue dust trail #2
            float trailProgress2 = Main.rand.NextFloat();
            Color blueGradient = Color.Lerp(UnifiedVFX.MoonlightSonata.MediumPurple, UnifiedVFX.MoonlightSonata.LightBlue, trailProgress2);
            Vector2 trailOffset2 = Main.rand.NextVector2Circular(Projectile.width * 0.25f, Projectile.height * 0.25f);
            Dust heavyBlue = Dust.NewDustDirect(Projectile.Center + trailOffset2 - Vector2.One * 4, 8, 8, 
                DustID.BlueTorch, -Projectile.velocity.X * 0.12f, -Projectile.velocity.Y * 0.12f, 80, blueGradient, 1.4f);
            heavyBlue.noGravity = true;
            heavyBlue.fadeIn = 1.3f;
            
            // === CONTRASTING SILVER SPARKLES (1-in-2) ===
            if (Main.rand.NextBool(2))
            {
                Vector2 sparkleOffset = Main.rand.NextVector2Circular(Projectile.width * 0.3f, Projectile.height * 0.3f);
                CustomParticles.PrismaticSparkle(Projectile.Center + sparkleOffset, UnifiedVFX.MoonlightSonata.Silver, 0.35f);
                
                Dust silverDust = Dust.NewDustDirect(Projectile.Center + sparkleOffset, 1, 1, DustID.SilverCoin, 0f, 0f, 100, default, 0.8f);
                silverDust.noGravity = true;
            }
            
            // === LUNAR SHIMMER TRAIL (1-in-3) ===
            if (Main.rand.NextBool(3))
            {
                float lunarHue = 0.7f + (Main.GameUpdateCount * 0.015f % 0.15f);
                Color shimmerColor = Main.hslToRgb(lunarHue, 0.9f, 0.75f);
                Vector2 shimmerOffset = Main.rand.NextVector2Circular(Projectile.width * 0.25f, Projectile.height * 0.25f);
                CustomParticles.GenericFlare(Projectile.Center + shimmerOffset, shimmerColor, 0.4f, 12);
            }
            
            // === FREQUENT FLARES (1-in-2) ===
            if (Main.rand.NextBool(2))
            {
                Vector2 flareOffset = Main.rand.NextVector2Circular(Projectile.width * 0.35f, Projectile.height * 0.35f);
                Color flareColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, Main.rand.NextFloat());
                CustomParticles.GenericFlare(Projectile.Center + flareOffset, flareColor, 0.4f, 12);
            }
            
            // === PEARLESCENT MOONSTONE EFFECTS (1-in-4) ===
            if (Main.rand.NextBool(4))
            {
                float pearlShift = (Main.GameUpdateCount * 0.02f) % 1f;
                Color pearlColor = Color.Lerp(Color.Lerp(UnifiedVFX.MoonlightSonata.LightBlue, Color.White, pearlShift), 
                    UnifiedVFX.MoonlightSonata.MediumPurple, (float)Math.Sin(pearlShift * MathHelper.TwoPi) * 0.3f + 0.3f);
                Vector2 pearlOffset = Main.rand.NextVector2Circular(Projectile.width * 0.3f, Projectile.height * 0.3f);
                CustomParticles.GenericFlare(Projectile.Center + pearlOffset, pearlColor * 0.8f, 0.35f, 15);
            }

            // === MUSIC NOTES (1-in-6) ===
            if (Main.rand.NextBool(6))
            {
                Vector2 notePos = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.Zero) * 20f;
                Color noteColor = Color.Lerp(UnifiedVFX.MoonlightSonata.MediumPurple, UnifiedVFX.MoonlightSonata.Silver, Main.rand.NextFloat());
                ThemedParticles.MusicNote(notePos, Main.rand.NextVector2Circular(1.5f, 1.5f), noteColor, 0.85f, 40);
            }

            // Bright light emission - moon glow
            float intensity = 1f - (Projectile.alpha / 255f);
            Lighting.AddLight(Projectile.Center, 0.6f * intensity, 0.4f * intensity, 1f * intensity);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Music's Dissonance debuff
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 300);
            
            // === SEEKING CRYSTALS - 25% chance on hit ===
            if (Main.rand.NextBool(4))
            {
                SeekingCrystalHelper.SpawnMoonlightCrystals(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Projectile.velocity,
                    (int)(damageDone * 0.2f),
                    Projectile.knockBack,
                    Projectile.owner,
                    3
                );
            }

            // === LUNAR IMPACT (Trust UnifiedVFX for core effect) ===
            UnifiedVFX.MoonlightSonata.Impact(target.Center, 1.0f);
            
            // === IRIDESCENT WINGSPAN-STYLE GRADIENT HALO RINGS (4 stacked) ===
            CustomParticles.HaloRing(target.Center, UnifiedVFX.MoonlightSonata.DarkPurple, 0.5f, 15);
            CustomParticles.HaloRing(target.Center, UnifiedVFX.MoonlightSonata.MediumPurple, 0.4f, 13);
            CustomParticles.HaloRing(target.Center, UnifiedVFX.MoonlightSonata.LightBlue, 0.3f, 11);
            CustomParticles.HaloRing(target.Center, Color.White * 0.85f, 0.2f, 9);
            
            // === MUSIC NOTES BURST (6 notes) ===
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2.5f, 4.5f);
                Color noteColor = Color.Lerp(UnifiedVFX.MoonlightSonata.MediumPurple, UnifiedVFX.MoonlightSonata.Silver, Main.rand.NextFloat());
                ThemedParticles.MusicNote(target.Center, noteVel, noteColor, 0.85f, 30);
            }
            
            // === LUNAR SHIMMER FLARE BURST (8 flares) ===
            for (int i = 0; i < 8; i++)
            {
                float lunarHue = 0.7f + Main.rand.NextFloat(0.15f);
                Color shimmerColor = Main.hslToRgb(lunarHue, 0.9f, 0.8f);
                Vector2 flarePos = target.Center + Main.rand.NextVector2Circular(20f, 20f);
                CustomParticles.GenericFlare(flarePos, shimmerColor, 0.4f, 15);
            }
            
            // === RADIAL DUST EXPLOSION (16 dust particles) ===
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 dustVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                
                Dust purpleDust = Dust.NewDustDirect(target.Center, 1, 1, DustID.PurpleTorch, dustVel.X, dustVel.Y, 100, default, 1.5f);
                purpleDust.noGravity = true;
                purpleDust.fadeIn = 1.2f;
            }
            
            // === SILVER CONTRASTING SPARKLES (6 sparkles) ===
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparklePos = target.Center + Main.rand.NextVector2Circular(25f, 25f);
                CustomParticles.PrismaticSparkle(sparklePos, UnifiedVFX.MoonlightSonata.Silver, 0.35f);
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
            // === LUNAR CRESCENT DISSIPATION (Trust UnifiedVFX for core effect) ===
            UnifiedVFX.MoonlightSonata.Impact(Projectile.Center, 1.2f);
            
            // === IRIDESCENT WINGSPAN-STYLE GRADIENT HALO RINGS (4 stacked) ===
            CustomParticles.HaloRing(Projectile.Center, UnifiedVFX.MoonlightSonata.DarkPurple, 0.6f, 18);
            CustomParticles.HaloRing(Projectile.Center, UnifiedVFX.MoonlightSonata.MediumPurple, 0.5f, 16);
            CustomParticles.HaloRing(Projectile.Center, UnifiedVFX.MoonlightSonata.LightBlue, 0.4f, 14);
            CustomParticles.HaloRing(Projectile.Center, Color.White * 0.9f, 0.3f, 12);
            
            // === MUSIC NOTES BURST (8 notes) ===
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                Color noteColor = Color.Lerp(UnifiedVFX.MoonlightSonata.MediumPurple, UnifiedVFX.MoonlightSonata.Silver, Main.rand.NextFloat());
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.9f, 35);
            }
            
            // === LUNAR SHIMMER FLARE BURST (12 flares) ===
            for (int i = 0; i < 12; i++)
            {
                float lunarHue = 0.7f + Main.rand.NextFloat(0.15f);
                Color shimmerColor = Main.hslToRgb(lunarHue, 0.9f, 0.8f);
                Vector2 flarePos = Projectile.Center + Main.rand.NextVector2Circular(30f, 30f);
                CustomParticles.GenericFlare(flarePos, shimmerColor, 0.5f, 18);
            }
            
            // === RADIAL DUST EXPLOSION (20 dust particles) ===
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Vector2 dustVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 9f);
                
                Dust purpleDust = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.PurpleTorch, dustVel.X, dustVel.Y, 100, default, 1.6f);
                purpleDust.noGravity = true;
                purpleDust.fadeIn = 1.3f;
            }
            
            // === SILVER CONTRASTING SPARKLES (8 sparkles) ===
            for (int i = 0; i < 8; i++)
            {
                Vector2 sparklePos = Projectile.Center + Main.rand.NextVector2Circular(35f, 35f);
                CustomParticles.PrismaticSparkle(sparklePos, UnifiedVFX.MoonlightSonata.Silver, 0.4f);
            }
            
            // === CRYSTAL SHARD BURST (6 crystals) ===
            for (int i = 0; i < 6; i++)
            {
                Vector2 crystalVel = Main.rand.NextVector2Circular(5f, 5f);
                Dust crystal = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.PurpleCrystalShard, crystalVel.X, crystalVel.Y, 100, default, 1.2f);
                crystal.noGravity = true;
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

