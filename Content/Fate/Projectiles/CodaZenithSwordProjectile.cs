using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Fate.Debuffs;

namespace MagnumOpus.Content.Fate.Projectiles
{
    /// <summary>
    /// The Zenith-style projectile that flies from Coda of Annihilation.
    /// Each instance renders as a copy of a different score's melee weapon.
    /// Features cosmic trails, glow effects, and Zenith-like homing behavior.
    /// </summary>
    public class CodaZenithSwordProjectile : ModProjectile
    {
        // All melee weapon textures from each score (14 total weapons)
        private static readonly string[] WeaponTextures = new string[]
        {
            // Moonlight Sonata
            "MagnumOpus/Content/MoonlightSonata/ResonantWeapons/IncisorOfMoonlight",
            "MagnumOpus/Content/MoonlightSonata/Weapons/EternalMoon",
            // Eroica
            "MagnumOpus/Content/Eroica/ResonantWeapons/SakurasBlossom",
            "MagnumOpus/Content/Eroica/ResonantWeapons/CelestialValor",
            // La Campanella
            "MagnumOpus/Content/LaCampanella/ResonantWeapons/IgnitionOfTheBell",
            "MagnumOpus/Content/LaCampanella/ResonantWeapons/DualFatedChime",
            // Enigma Variations
            "MagnumOpus/Content/EnigmaVariations/ResonantWeapons/VariationsOfTheVoid",
            "MagnumOpus/Content/EnigmaVariations/ResonantWeapons/TheUnresolvedCadence",
            // Swan Lake
            "MagnumOpus/Content/SwanLake/ResonantWeapons/CalloftheBlackSwan",
            // Fate (including self)
            "MagnumOpus/Content/Fate/ResonantWeapons/TheConductorsLastConstellation",
            "MagnumOpus/Content/Fate/ResonantWeapons/RequiemOfReality",
            "MagnumOpus/Content/Fate/ResonantWeapons/OpusUltima",
            "MagnumOpus/Content/Fate/ResonantWeapons/FractalOfTheStars",
            "MagnumOpus/Content/Fate/ResonantWeapons/CodaOfAnnihilation"
        };
        
        // Theme colors for each weapon (14 colors matching the weapons)
        private static readonly Color[] WeaponColors = new Color[]
        {
            // Moonlight Sonata - purple/blue lunar
            new Color(138, 43, 226),
            new Color(135, 206, 250),
            // Eroica - scarlet/gold/pink
            new Color(255, 100, 100),
            new Color(255, 200, 80),
            // La Campanella - orange/fire
            new Color(255, 140, 40),
            new Color(255, 180, 60),
            // Enigma Variations - purple/green
            new Color(140, 60, 200),
            new Color(50, 180, 100),
            // Swan Lake - white/prismatic
            new Color(255, 255, 255),
            // Fate - cosmic pink/red/purple
            new Color(180, 50, 100),
            new Color(200, 60, 80),
            new Color(140, 50, 160),
            new Color(160, 80, 140),
            new Color(220, 80, 120)
        };
        
        // Cached weapon textures
        private static Asset<Texture2D>[] cachedTextures;
        private static bool texturesLoaded = false;
        
        // Which weapon this projectile displays (set in ai[0])
        public int WeaponIndex
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        
        // Target NPC index (-1 = no target, use ai[1])
        public int TargetIndex
        {
            get => (int)Projectile.ai[1] - 1;
            set => Projectile.ai[1] = value + 1;
        }
        
        // Zenith-style state
        private float homingStrength = 0f;
        private float rotationSpeed = 0f;
        private Vector2[] trailPositions = new Vector2[12];
        private float[] trailRotations = new float[12];
        private int trailIndex = 0;
        private int attackTimer = 0;
        
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/CodaOfAnnihilation";
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 180;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.alpha = 0;
        }
        
        public override void OnSpawn(IEntitySource source)
        {
            // Initialize trail positions
            for (int i = 0; i < trailPositions.Length; i++)
            {
                trailPositions[i] = Projectile.Center;
                trailRotations[i] = Projectile.rotation;
            }
            
            // Random initial rotation speed - slower to show full circular animation
            rotationSpeed = Main.rand.NextFloat(0.06f, 0.12f) * (Main.rand.NextBool() ? 1 : -1);
            
            // Spawn VFX
            Color weaponColor = GetWeaponColor();
            CustomParticles.GenericFlare(Projectile.Center, Color.White, 0.6f, 15);
            CustomParticles.GenericFlare(Projectile.Center, weaponColor, 0.5f, 12);
            
            // Load textures if not loaded
            LoadWeaponTextures();
        }
        
        private static void LoadWeaponTextures()
        {
            if (texturesLoaded) return;
            
            cachedTextures = new Asset<Texture2D>[WeaponTextures.Length];
            for (int i = 0; i < WeaponTextures.Length; i++)
            {
                try
                {
                    cachedTextures[i] = ModContent.Request<Texture2D>(WeaponTextures[i], AssetRequestMode.ImmediateLoad);
                }
                catch
                {
                    cachedTextures[i] = null;
                }
            }
            texturesLoaded = true;
        }
        
        private Color GetWeaponColor()
        {
            int index = WeaponIndex;
            if (index >= 0 && index < WeaponColors.Length)
                return WeaponColors[index];
            return FateCosmicVFX.FateDarkPink;
        }
        
        private Texture2D GetWeaponTexture()
        {
            LoadWeaponTextures();
            
            int index = WeaponIndex;
            if (index >= 0 && index < cachedTextures.Length && cachedTextures[index] != null)
                return cachedTextures[index].Value;
            
            // Fallback to default texture
            return TextureAssets.Projectile[Projectile.type].Value;
        }
        
        public override void AI()
        {
            attackTimer++;
            Player owner = Main.player[Projectile.owner];
            
            // Update trail
            if (attackTimer % 2 == 0)
            {
                trailIndex = (trailIndex + 1) % trailPositions.Length;
                trailPositions[trailIndex] = Projectile.Center;
                trailRotations[trailIndex] = Projectile.rotation;
            }
            
            // Zenith-style rotation (sword spins as it flies)
            Projectile.rotation += rotationSpeed;
            
            // Homing behavior - finds and tracks targets
            NPC target = FindTarget();
            
            if (target != null)
            {
                TargetIndex = target.whoAmI;
                
                // Increase homing strength over time
                homingStrength = Math.Min(homingStrength + 0.008f, 0.25f);
                
                // Home toward target
                Vector2 toTarget = target.Center - Projectile.Center;
                float targetDistance = toTarget.Length();
                
                if (targetDistance > 0)
                {
                    toTarget.Normalize();
                    
                    // Zenith-style curved approach
                    float speed = Projectile.velocity.Length();
                    Vector2 desiredVelocity = toTarget * Math.Max(speed, 18f);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, homingStrength);
                }
                
                // Speed up when close to target (but not too fast to hide animation)
                if (targetDistance < 200f)
                {
                    rotationSpeed *= 1.005f; // Spin slightly faster as it approaches
                    if (Projectile.velocity.Length() < 25f)
                        Projectile.velocity *= 1.03f;
                }
            }
            else
            {
                // No target - fly outward then return to player
                TargetIndex = -1;
                
                float distanceToPlayer = Vector2.Distance(Projectile.Center, owner.Center);
                
                if (attackTimer > 60 || distanceToPlayer > 800f)
                {
                    // Return to player
                    Vector2 toPlayer = owner.Center - Projectile.Center;
                    toPlayer.Normalize();
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toPlayer * 20f, 0.1f);
                }
                
                // Despawn when very close to player while returning
                if (attackTimer > 90 && distanceToPlayer < 50f)
                {
                    Projectile.Kill();
                    return;
                }
            }
            
            // Cap maximum velocity
            float maxSpeed = 28f;
            if (Projectile.velocity.Length() > maxSpeed)
            {
                Projectile.velocity.Normalize();
                Projectile.velocity *= maxSpeed;
            }
            
            // === VFX ===
            SpawnTrailParticles();
            
            // Lighting
            Color lightColor = GetWeaponColor();
            Lighting.AddLight(Projectile.Center, lightColor.ToVector3() * 0.6f);
        }
        
        private NPC FindTarget()
        {
            // Check existing target
            if (TargetIndex >= 0 && TargetIndex < Main.maxNPCs)
            {
                NPC existing = Main.npc[TargetIndex];
                if (existing.active && !existing.friendly && existing.CanBeChasedBy())
                    return existing;
            }
            
            // Find new target
            float closestDist = 600f;
            NPC closest = null;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy())
                    continue;
                
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            
            return closest;
        }
        
        private void SpawnTrailParticles()
        {
            Color weaponColor = GetWeaponColor();
            
            // Cosmic trail particles
            if (Main.rand.NextBool(2))
            {
                Vector2 trailOffset = Main.rand.NextVector2Circular(10f, 10f);
                Color trailColor = Color.Lerp(weaponColor, FateCosmicVFX.FateWhite, Main.rand.NextFloat(0.3f));
                
                var trail = new GenericGlowParticle(
                    Projectile.Center + trailOffset,
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1f, 1f),
                    trailColor * 0.7f,
                    Main.rand.NextFloat(0.2f, 0.35f),
                    Main.rand.Next(12, 20),
                    true
                );
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // Star sparkles
            if (Main.rand.NextBool(5))
            {
                var star = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(15f, 15f),
                    Main.rand.NextVector2Circular(0.5f, 0.5f),
                    FateCosmicVFX.FateWhite,
                    0.15f,
                    15,
                    true
                );
                MagnumParticleHandler.SpawnParticle(star);
            }
            
            // Occasional glyph
            if (Main.rand.NextBool(15))
            {
                CustomParticles.Glyph(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    weaponColor * 0.6f,
                    0.25f,
                    -1
                );
            }
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Color weaponColor = GetWeaponColor();
            
            // Impact VFX
            CustomParticles.GenericFlare(target.Center, Color.White, 0.8f, 18);
            CustomParticles.GenericFlare(target.Center, weaponColor, 0.6f, 15);
            CustomParticles.HaloRing(target.Center, weaponColor * 0.7f, 0.35f, 15);
            
            // Spark burst
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(6f, 6f);
                var spark = new GlowSparkParticle(target.Center, sparkVel, weaponColor, 0.3f, 18);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // Apply Fate debuff - DestinyCollapse
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 180);
            
            // Sound
            SoundEngine.PlaySound(SoundID.Item71 with { Volume = 0.6f, Pitch = 0.2f }, target.Center);
        }
        
        public override void OnKill(int timeLeft)
        {
            Color weaponColor = GetWeaponColor();
            
            // Fade out VFX
            CustomParticles.GenericFlare(Projectile.Center, weaponColor, 0.5f, 12);
            
            for (int i = 0; i < 4; i++)
            {
                var glow = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    Main.rand.NextVector2Circular(2f, 2f),
                    weaponColor * 0.5f,
                    0.25f,
                    15,
                    true
                );
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D weaponTex = GetWeaponTexture();
            Color weaponColor = GetWeaponColor();
            
            if (weaponTex == null) return false;
            
            Vector2 origin = weaponTex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            // Draw trail (Zenith-style afterimages)
            for (int i = 0; i < trailPositions.Length; i++)
            {
                int actualIndex = (trailIndex - i + trailPositions.Length) % trailPositions.Length;
                Vector2 trailPos = trailPositions[actualIndex] - Main.screenPosition;
                float trailRot = trailRotations[actualIndex];
                
                float progress = (float)i / trailPositions.Length;
                float trailAlpha = (1f - progress) * 0.5f;
                float trailScale = 1f - progress * 0.3f;
                
                // Trail color gradient
                Color trailColor = Color.Lerp(weaponColor, FateCosmicVFX.FateWhite, progress * 0.3f) * trailAlpha;
                trailColor.A = 0; // Additive
                
                spriteBatch.Draw(
                    weaponTex,
                    trailPos,
                    null,
                    trailColor,
                    trailRot,
                    origin,
                    trailScale,
                    SpriteEffects.None,
                    0f
                );
            }
            
            // Outer glow layer
            Color outerGlow = weaponColor with { A = 0 } * 0.3f;
            spriteBatch.Draw(weaponTex, drawPos, null, outerGlow, Projectile.rotation, origin, 1.15f, SpriteEffects.None, 0f);
            
            // Main weapon sprite
            spriteBatch.Draw(weaponTex, drawPos, null, Color.White, Projectile.rotation, origin, 1f, SpriteEffects.None, 0f);
            
            // Inner glow bloom
            Color innerGlow = Color.Lerp(weaponColor, Color.White, 0.5f) with { A = 0 } * 0.4f;
            spriteBatch.Draw(weaponTex, drawPos, null, innerGlow, Projectile.rotation, origin, 0.9f, SpriteEffects.None, 0f);
            
            return false;
        }
    }
}
