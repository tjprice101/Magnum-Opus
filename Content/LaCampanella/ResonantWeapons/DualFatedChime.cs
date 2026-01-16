using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
using MagnumOpus.Content.LaCampanella.Debuffs;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons
{
    /// <summary>
    /// Dual-fated Chime - Zenith-style melee weapon with bell-music flames.
    /// Attack: Swings like the Zenith, casting spectral versions of the blade with bell-music flames.
    /// Special: Inferno Waltz - charge bar fills with attacks, right-click unleashes spinning flame dance.
    /// All attacks apply Resonant Toll debuff.
    /// </summary>
    public class DualFatedChime : ModItem
    {
        private float chargeBar = 0f;
        private const float MaxCharge = 100f;
        public const float ChargePerHit = 8f;
        
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 380;
            Item.DamageType = DamageClass.Melee;
            Item.width = 80;
            Item.height = 80;
            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6.5f;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<LaCampanellaRainbowRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<DualFatedChimeProjectile>();
            Item.shootSpeed = 16f;
            Item.noMelee = true; // Projectiles do the damage
            Item.noUseGraphic = true; // Hide the held item
            Item.channel = true;
        }

        public override bool AltFunctionUse(Player player) => chargeBar >= MaxCharge;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2 && chargeBar >= MaxCharge)
            {
                // Activate Inferno Waltz
                Item.useTime = 60;
                Item.useAnimation = 60;
                return true;
            }
            
            Item.useTime = 16;
            Item.useAnimation = 16;
            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2 && chargeBar >= MaxCharge)
            {
                // Inferno Waltz - spinning flame attack
                ActivateInfernoWaltz(player, source, damage);
                chargeBar = 0f;
                return false;
            }
            
            // Normal attack - spawn Zenith-style projectiles
            Vector2 toMouse = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            
            // Spawn multiple spectral blades
            int bladeCount = Main.rand.Next(2, 5);
            for (int i = 0; i < bladeCount; i++)
            {
                float offsetAngle = Main.rand.NextFloat(-0.4f, 0.4f);
                Vector2 projVelocity = toMouse.RotatedBy(offsetAngle) * Item.shootSpeed * Main.rand.NextFloat(0.8f, 1.2f);
                Vector2 spawnPos = player.Center + toMouse.RotatedBy(offsetAngle) * Main.rand.NextFloat(30f, 80f);
                
                Projectile.NewProjectile(source, spawnPos, projVelocity, type, damage, knockback, player.whoAmI, 
                    Main.rand.NextFloat(MathHelper.TwoPi)); // ai[0] = random rotation
            }
            
            // === GUTURAL CHAINSAW BELL SOUND STACK ===
            // Multiple layered bell sounds for intense chainsaw-like effect
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = Main.rand.NextFloat(0.2f, 0.6f), Volume = 0.6f }, player.Center);
            SoundEngine.PlaySound(SoundID.Item34 with { Pitch = Main.rand.NextFloat(-0.2f, 0.2f), Volume = 0.35f }, player.Center);
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot with { Pitch = 0.5f, Volume = 0.25f }, player.Center);
            
            // === GRADIENT COLOR DEFINITIONS ===
            Color campanellaBlack = ThemedParticles.CampanellaBlack;
            Color campanellaOrange = ThemedParticles.CampanellaOrange;
            Color campanellaYellow = ThemedParticles.CampanellaYellow;
            Color campanellaGold = ThemedParticles.CampanellaGold;
            
            // === VIBRANT SWING AURA - Swan Lake style visual flare! ===
            ThemedParticles.LaCampanellaSwingAura(player.Center, toMouse, 1.2f);
            
            // === MASSIVE SWING EFFECTS ===
            // Directional flame burst
            ThemedParticles.LaCampanellaSparks(player.Center + toMouse * 50f, toMouse, 8, 8f);
            ThemedParticles.LaCampanellaMusicNotes(player.Center, 4, 35f);
            
            // Orange/gold bloom on swing
            ThemedParticles.LaCampanellaBloomBurst(player.Center + toMouse * 40f, 0.5f);
            
            // Gradient halo ring burst - BLACK → ORANGE
            for (int ring = 0; ring < 3; ring++)
            {
                float progress = (float)ring / 3f;
                Color ringColor = Color.Lerp(campanellaBlack, campanellaOrange, progress);
                CustomParticles.HaloRing(player.Center + toMouse * 30f, ringColor, 0.3f + ring * 0.1f, 12 + ring * 3);
            }
            
            // === GLYPH BURST ON SWING ===
            if (CustomParticleSystem.TexturesLoaded)
            {
                for (int i = 0; i < 4; i++)
                {
                    float glyphAngle = toMouse.ToRotation() + MathHelper.PiOver2 * i;
                    Vector2 glyphPos = player.Center + glyphAngle.ToRotationVector2() * 35f;
                    Color glyphColor = Color.Lerp(campanellaBlack, campanellaOrange, (float)i / 4f) * 0.65f;
                    CustomParticles.Glyph(glyphPos, glyphColor, 0.25f, -1);
                }
            }
            
            // Fire flares spiraling outward with BLACK → ORANGE GRADIENT
            for (int i = 0; i < 6; i++)
            {
                float flareAngle = toMouse.ToRotation() + MathHelper.PiOver4 * i;
                Vector2 flarePos = player.Center + flareAngle.ToRotationVector2() * Main.rand.NextFloat(30f, 60f);
                float progress = (float)i / 6f;
                Color flareColor = Color.Lerp(campanellaBlack, campanellaOrange, progress);
                CustomParticles.GenericFlare(flarePos, flareColor, 0.45f, 16);
            }
            
            // Fractal geometric burst with BLACK → ORANGE gradient
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 offset = angle.ToRotationVector2() * 35f;
                float progress = (float)i / 8f;
                Color fractalColor = Color.Lerp(campanellaBlack, campanellaOrange, progress);
                CustomParticles.GenericFlare(player.Center + toMouse * 20f + offset, fractalColor, 0.35f, 14);
            }
            
            // Screen shake on each swing
            // REMOVED: Screen shake disabled for La Campanella weapons
            // player.GetModPlayer<ScreenShakePlayer>()?.AddShake(2f, 5);
            
            // Intense lighting flash
            Lighting.AddLight(player.Center + toMouse * 40f, 1.2f, 0.6f, 0.15f);
            
            return false;
        }

        private void ActivateInfernoWaltz(Player player, IEntitySource source, int damage)
        {
            // === EPIC INFERNO WALTZ ACTIVATION ===
            // Multiple dramatic bell sounds for chainsaw-like intensity
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.3f, Volume = 1f }, player.Center);
            SoundEngine.PlaySound(SoundID.Item45 with { Pitch = 0.3f, Volume = 0.8f }, player.Center);
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Pitch = -0.2f, Volume = 0.7f }, player.Center);
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = -0.5f, Volume = 0.6f }, player.Center);
            
            // Spawn the Inferno Waltz projectile (spinning flame dance)
            Projectile.NewProjectile(source, player.Center, Vector2.Zero, 
                ModContent.ProjectileType<InfernoWaltzProjectile>(), (int)(damage * 2f), 10f, player.whoAmI);
            
            // Grant movement speed buff
            player.AddBuff(ModContent.BuffType<InfernoWaltzBuff>(), 900); // 15 seconds
            
            // === MASSIVE PARTICLE EXPLOSION! ===
            ThemedParticles.LaCampanellaImpact(player.Center, 3f);
            ThemedParticles.LaCampanellaBellChime(player.Center, 2.5f);
            ThemedParticles.LaCampanellaShockwave(player.Center, 2f);
            ThemedParticles.LaCampanellaMusicalImpact(player.Center, 2f, true);
            
            // === GRADIENT COLOR DEFINITIONS ===
            Color campanellaOrange = ThemedParticles.CampanellaOrange;
            Color campanellaYellow = ThemedParticles.CampanellaYellow;
            Color campanellaGold = ThemedParticles.CampanellaGold;
            Color campanellaBlack = ThemedParticles.CampanellaBlack;
            
            // Radial flare burst with BLACK → ORANGE GRADIENT
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 flarePos = player.Center + angle.ToRotationVector2() * Main.rand.NextFloat(40f, 80f);
                float progress = (float)i / 16f;
                Color flareColor = Color.Lerp(campanellaBlack, campanellaOrange, progress);
                CustomParticles.GenericFlare(flarePos, flareColor, 0.8f, 25);
            }
            
            // === GLYPH EXPLOSION - INFERNO WALTZ ACTIVATION ===
            if (CustomParticleSystem.TexturesLoaded)
            {
                for (int i = 0; i < 8; i++)
                {
                    float glyphAngle = MathHelper.TwoPi * i / 8f;
                    Vector2 glyphPos = player.Center + glyphAngle.ToRotationVector2() * 55f;
                    Color glyphColor = Color.Lerp(campanellaBlack, campanellaOrange, (float)i / 8f) * 0.7f;
                    CustomParticles.Glyph(glyphPos, glyphColor, 0.35f, -1);
                }
            }
            
            // Multiple halo rings with BLACK → ORANGE GRADIENT expanding outward
            for (int ring = 0; ring < 5; ring++)
            {
                float progress = (float)ring / 5f;
                Color ringColor = Color.Lerp(campanellaBlack, campanellaOrange, progress);
                CustomParticles.HaloRing(player.Center, ringColor, 0.6f + ring * 0.25f, 25 + ring * 5);
            }
            
            // Massive spark burst with BLACK → ORANGE GRADIENT
            for (int i = 0; i < 24; i++)
            {
                float angle = MathHelper.TwoPi * i / 24f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(8f, 15f);
                float progress = (float)i / 24f;
                Color color = Color.Lerp(campanellaBlack, campanellaOrange, progress);
                var spark = new GlowSparkParticle(player.Center, vel, color, 0.8f, Main.rand.Next(25, 40));
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // Fractal geometric burst - BLACK → ORANGE signature pattern
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 offset = angle.ToRotationVector2() * 50f;
                float progress = (float)i / 8f;
                Color fractalColor = Color.Lerp(campanellaBlack, campanellaOrange, progress);
                CustomParticles.GenericFlare(player.Center + offset, fractalColor, 0.6f, 20);
            }
            
            // Screen shake - DRAMATIC
            // REMOVED: Screen shake disabled for La Campanella weapons
            // player.GetModPlayer<ScreenShakePlayer>()?.AddShake(15f, 30);
            
            // Intense light flash
            Lighting.AddLight(player.Center, 2.5f, 1.2f, 0.4f);
        }

        public override void HoldItem(Player player)
        {
            // === VIBRANT HOLD AURA - Swan Lake style visual flare! ===
            ThemedParticles.LaCampanellaHoldAura(player.Center, 1.2f);
            
            // === AMBIENT INFERNAL AURA ===
            // Fire aura particles
            if (Main.rand.NextBool(6))
            {
                ThemedParticles.LaCampanellaAura(player.Center, 40f);
            }
            
            // Ambient flares orbiting player
            if (Main.rand.NextBool(8))
            {
                float angle = Main.GameUpdateCount * 0.03f + Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 flarePos = player.Center + angle.ToRotationVector2() * Main.rand.NextFloat(35f, 55f);
                Color flareColor = Main.rand.NextBool() ? ThemedParticles.CampanellaOrange : ThemedParticles.CampanellaGold;
                CustomParticles.GenericFlare(flarePos, flareColor, 0.25f, 15);
            }
            
            // Rising music notes
            if (Main.rand.NextBool(12))
            {
                ThemedParticles.LaCampanellaMusicNotes(player.Center + new Vector2(Main.rand.NextFloat(-20f, 20f), 10f), 1, 15f);
            }
            
            // Black smoke wisps
            if (Main.rand.NextBool(15))
            {
                var smoke = new HeavySmokeParticle(player.Center + Main.rand.NextVector2Circular(25f, 25f),
                    new Vector2(0, -0.5f), ThemedParticles.CampanellaBlack,
                    Main.rand.Next(30, 50), Main.rand.NextFloat(0.15f, 0.25f), 0.3f, 0.01f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Display charge bar visually
            DrawChargeBar(player);
            
            // Dynamic pulsing light
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.2f;
            Lighting.AddLight(player.Center, 0.6f * pulse, 0.3f * pulse, 0.1f * pulse);
        }

        private void DrawChargeBar(Player player)
        {
            if (chargeBar <= 0) return;
            
            // Visual charge indicator particles
            float chargePercent = chargeBar / MaxCharge;
            if (Main.rand.NextFloat() < chargePercent * 0.3f)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 offset = angle.ToRotationVector2() * 25f * chargePercent;
                Color color = Color.Lerp(ThemedParticles.CampanellaOrange, ThemedParticles.CampanellaYellow, chargePercent);
                
                Dust indicator = Dust.NewDustPerfect(player.Center + offset, DustID.Torch, 
                    -offset.SafeNormalize(Vector2.Zero) * 2f, 100, color, 1f + chargePercent);
                indicator.noGravity = true;
            }
        }

        public void AddCharge(float amount)
        {
            chargeBar = Math.Min(chargeBar + amount, MaxCharge);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.15f + 1f;
            
            // Additive glow
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // === ORBITING GLYPHS AROUND WEAPON ===
            if (CustomParticleSystem.TexturesLoaded)
            {
                Texture2D glyphTex = CustomParticleSystem.RandomGlyph().Value;
                for (int i = 0; i < 5; i++)
                {
                    float glyphAngle = Main.GameUpdateCount * 0.05f + MathHelper.TwoPi * i / 5f;
                    Vector2 glyphPos = position + glyphAngle.ToRotationVector2() * 22f;
                    Color glyphColor = Color.Lerp(ThemedParticles.CampanellaBlack, ThemedParticles.CampanellaOrange, (float)i / 5f) * 0.6f;
                    spriteBatch.Draw(glyphTex, glyphPos, null, glyphColor, glyphAngle * 2f, glyphTex.Size() / 2f, 0.18f * pulse, SpriteEffects.None, 0f);
                }
            }
            
            // Black → Orange gradient glow
            spriteBatch.Draw(texture, position, null, ThemedParticles.CampanellaOrange * 0.5f, rotation, origin, scale * pulse * 1.3f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, Color.Lerp(ThemedParticles.CampanellaBlack, ThemedParticles.CampanellaOrange, 0.4f) * 0.35f, rotation, origin, scale * pulse * 1.2f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, ThemedParticles.CampanellaBlack * 0.3f, rotation, origin, scale * pulse * 1.1f, SpriteEffects.None, 0f);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Lighting.AddLight(Item.Center, 0.7f, 0.4f, 0.1f);
            
            return true;
        }
    }

    /// <summary>
    /// Spectral blade projectile with TRUE Zenith behavior:
    /// 1. Flies toward the cursor
    /// 2. Orbits around the cursor briefly
    /// 3. Returns to the player
    /// Draws the actual weapon sprite with spectral bell-flame effects.
    /// </summary>
    public class DualFatedChimeProjectile : ModProjectile
    {
        // Use the weapon's texture for Zenith-style spectral blades!
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/DualFatedChime";
        
        private float rotationSpeed;
        private float spectralPulse;
        private float trailOpacity = 1f;
        
        // Zenith orbit behavior states
        private enum ZenithPhase { FlyingToCursor, Orbiting, ReturningToPlayer }
        private ZenithPhase currentPhase = ZenithPhase.FlyingToCursor;
        private Vector2 orbitCenter;
        private float orbitAngle;
        private float orbitTimer;
        private const float OrbitRadius = 60f;
        private const float OrbitDuration = 25f; // frames to orbit
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1; // Infinite penetration for Zenith-style
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 50;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            // Initialize rotation speed and spectral effects
            if (Projectile.localAI[0] == 0)
            {
                rotationSpeed = Main.rand.NextFloat(0.3f, 0.5f) * (Main.rand.NextBool() ? 1 : -1);
                Projectile.localAI[0] = 1;
                spectralPulse = Main.rand.NextFloat(MathHelper.TwoPi);
                orbitAngle = Projectile.velocity.ToRotation();
                orbitCenter = Main.MouseWorld;
            }
            
            // Spectral pulsing effect
            spectralPulse += 0.15f;
            trailOpacity = 0.7f + (float)Math.Sin(spectralPulse) * 0.3f;
            
            // === ZENITH-STYLE BEHAVIOR ===
            switch (currentPhase)
            {
                case ZenithPhase.FlyingToCursor:
                    // Fly toward the cursor
                    orbitCenter = Main.MouseWorld;
                    Vector2 toCursor = orbitCenter - Projectile.Center;
                    float distToCursor = toCursor.Length();
                    
                    if (distToCursor < 80f)
                    {
                        // Reached cursor - start orbiting
                        currentPhase = ZenithPhase.Orbiting;
                        orbitTimer = 0;
                        orbitAngle = (Projectile.Center - orbitCenter).ToRotation();
                        
                        // Bell chime on arrival with MASSIVE effects!
                        SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.8f, Volume = 0.4f }, Projectile.Center);
                        ThemedParticles.LaCampanellaHaloBurst(Projectile.Center, 0.7f);
                        ThemedParticles.LaCampanellaSwordArcBurst(Projectile.Center, 4, 0.4f);
                        ThemedParticles.LaCampanellaPrismaticSparkles(Projectile.Center, 6, 25f);
                    }
                    else
                    {
                        // Home toward cursor with increasing speed
                        toCursor = toCursor.SafeNormalize(Vector2.UnitX);
                        float speed = MathHelper.Lerp(16f, 28f, 1f - Math.Min(distToCursor / 400f, 1f));
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, toCursor * speed, 0.18f);
                    }
                    break;
                    
                case ZenithPhase.Orbiting:
                    // Orbit around the cursor position
                    orbitTimer++;
                    orbitAngle += 0.25f * (rotationSpeed > 0 ? 1 : -1); // Fast orbit
                    
                    // Position on orbit circle
                    Vector2 targetOrbitPos = orbitCenter + orbitAngle.ToRotationVector2() * OrbitRadius;
                    Projectile.velocity = (targetOrbitPos - Projectile.Center) * 0.3f;
                    
                    // Spawn orbit trail particles with sword arcs!
                    if (Main.rand.NextBool(2))
                    {
                        ThemedParticles.LaCampanellaSparks(Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.UnitX), 3, 5f);
                        ThemedParticles.LaCampanellaSwordArc(Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.UnitX), 0.35f);
                    }
                    
                    // Prismatic sparkles while orbiting
                    if (Main.rand.NextBool(4))
                    {
                        ThemedParticles.LaCampanellaPrismaticSparkles(Projectile.Center, 2, 15f);
                    }
                    
                    // After orbit duration, return to player
                    if (orbitTimer >= OrbitDuration)
                    {
                        currentPhase = ZenithPhase.ReturningToPlayer;
                        
                        // Bell chime on departure with effects!
                        SoundEngine.PlaySound(SoundID.Item34 with { Pitch = 0.5f, Volume = 0.35f }, Projectile.Center);
                        ThemedParticles.LaCampanellaHaloBurst(Projectile.Center, 0.5f);
                    }
                    break;
                    
                case ZenithPhase.ReturningToPlayer:
                    // Return to player
                    Vector2 toPlayer = owner.Center - Projectile.Center;
                    float distToPlayer = toPlayer.Length();
                    
                    if (distToPlayer < 40f)
                    {
                        // Reached player - die with flourish
                        Projectile.Kill();
                        return;
                    }
                    
                    // Accelerate toward player
                    toPlayer = toPlayer.SafeNormalize(Vector2.UnitX);
                    float returnSpeed = MathHelper.Lerp(20f, 35f, 1f - Math.Min(distToPlayer / 300f, 1f));
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toPlayer * returnSpeed, 0.2f);
                    break;
            }
            
            // Rotation - faster spinning
            Projectile.rotation += rotationSpeed;
            
            // === ENHANCED ZENITH-STYLE PARTICLE TRAIL ===
            // Use the new comprehensive Zenith trail effect
            ThemedParticles.LaCampanellaZenithTrail(Projectile.Center, Projectile.velocity, Projectile.rotation);
            
            // Additional fire trail
            ThemedParticles.LaCampanellaTrail(Projectile.Center, Projectile.velocity);
            
            // Occasional music notes floating up
            if (Main.rand.NextBool(5))
            {
                ThemedParticles.LaCampanellaMusicNotes(Projectile.Center, 2, 20f);
            }
            
            // Constant bell-flame sparks with sword arcs
            if (Main.rand.NextBool(3))
            {
                ThemedParticles.LaCampanellaSparks(Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.UnitX), 3, 5f);
            }
            
            // Sword arc following the blade rotation
            if (Main.rand.NextBool(4))
            {
                Vector2 arcDir = Projectile.rotation.ToRotationVector2();
                ThemedParticles.LaCampanellaSwordArc(Projectile.Center + arcDir * 20f, arcDir, 0.4f);
            }
            
            // Spectral glow particles swirling around the blade
            if (Main.rand.NextBool(3))
            {
                float angle = Main.GameUpdateCount * 0.2f + Projectile.whoAmI;
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 30f;
                Color glowColor = Main.rand.Next(3) switch
                {
                    0 => ThemedParticles.CampanellaOrange,
                    1 => ThemedParticles.CampanellaYellow,
                    _ => ThemedParticles.CampanellaGold
                };
                var glow = new GenericGlowParticle(Projectile.Center + offset, Vector2.Zero, glowColor, 
                    Main.rand.NextFloat(0.25f, 0.45f), Main.rand.Next(15, 25), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Prismatic sparkles for extra flair
            if (Main.rand.NextBool(6))
            {
                ThemedParticles.LaCampanellaPrismaticSparkles(Projectile.Center, 2, 20f);
            }
            
            // Halo glow pulse
            if (Main.rand.NextBool(10))
            {
                CustomParticles.HaloRing(Projectile.Center, ThemedParticles.CampanellaGold * 0.5f, 0.3f, 15);
            }
            
            // Dynamic lighting
            float lightIntensity = 0.6f + (float)Math.Sin(spectralPulse) * 0.25f;
            Lighting.AddLight(Projectile.Center, 0.8f * lightIntensity, 0.4f * lightIntensity, 0.12f * lightIntensity);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Resonant Toll
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);
            
            // Add charge to the weapon
            Player owner = Main.player[Projectile.owner];
            if (owner.HeldItem.ModItem is DualFatedChime chime)
            {
                chime.AddCharge(DualFatedChime.ChargePerHit);
            }
            
            // === TRANSITION TO RETURN PHASE ON HIT ===
            // After hitting an enemy, the blade should return to the player (Zenith behavior)
            if (currentPhase != ZenithPhase.ReturningToPlayer)
            {
                currentPhase = ZenithPhase.ReturningToPlayer;
            }
            
            // === GUTURAL CHAINSAW BELL HIT SOUNDS ===
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = Main.rand.NextFloat(0.2f, 0.6f), Volume = 0.5f }, target.Center);
            SoundEngine.PlaySound(SoundID.Item34 with { Pitch = Main.rand.NextFloat(-0.1f, 0.3f), Volume = 0.3f }, target.Center);
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Pitch = 0.3f, Volume = 0.25f }, target.Center);
            
            // === BLACK SMOKE SPARKLE MAGIC - SIGNATURE HIT EFFECT! ===
            Vector2 hitDirection = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
            ThemedParticles.LaCampanellaSignatureHit(target.Center, hitDirection, 1f);
            
            // === ENHANCED ZENITH-STYLE HIT EFFECTS! ===
            // Use the new comprehensive Zenith hit effect
            ThemedParticles.LaCampanellaZenithHit(target.Center, hitDirection, 1f);
            
            // === SIGNATURE FRACTAL FLARE BURST - BLACK → ORANGE ===
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 flareOffset = angle.ToRotationVector2() * 30f;
                float progress = (float)i / 6f;
                Color fractalColor = Color.Lerp(ThemedParticles.CampanellaBlack, ThemedParticles.CampanellaOrange, progress);
                CustomParticles.GenericFlare(target.Center + flareOffset, fractalColor, 0.45f, 18);
            }
            
            // === GLYPH IMPACT - ZENITH BLADE HIT ===
            if (CustomParticleSystem.TexturesLoaded)
            {
                for (int i = 0; i < 4; i++)
                {
                    float glyphAngle = MathHelper.TwoPi * i / 4f + Main.GameUpdateCount * 0.05f;
                    Vector2 glyphPos = target.Center + glyphAngle.ToRotationVector2() * 25f;
                    Color glyphColor = Color.Lerp(ThemedParticles.CampanellaBlack, ThemedParticles.CampanellaOrange, (float)i / 4f) * 0.65f;
                    CustomParticles.Glyph(glyphPos, glyphColor, 0.25f, -1);
                }
            }
            
            // Music notes on hit
            ThemedParticles.LaCampanellaMusicNotes(target.Center, 3, 25f);
            
            // Additional radial spark burst
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 dir = angle.ToRotationVector2();
                ThemedParticles.LaCampanellaSparks(target.Center, dir, 3, 6f);
            }
            
            // Screen shake removed from weapons - reserved for bosses only
            // owner.GetModPlayer<ScreenShakePlayer>()?.AddShake(3f, 6);
            
            // Critical hit extra effects!
            if (hit.Crit)
            {
                // MORE CHAINSAW SOUNDS FOR CRITS
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.8f, Volume = 0.4f }, target.Center);
                SoundEngine.PlaySound(SoundID.Item45 with { Pitch = 0.3f, Volume = 0.3f }, target.Center);
                
                // Use the enhanced crit effect!
                ThemedParticles.LaCampanellaZenithCrit(target.Center, hitDirection, 1.2f);
                
                // Extra grand impact for crits
                ThemedParticles.LaCampanellaGrandImpact(target.Center, 0.6f);
                
                // Extra black smoke sparkle on crits!
                ThemedParticles.LaCampanellaBlackSmokeSparkle(target.Center, 0.8f);
                
                // Screen shake removed from weapons - reserved for bosses only
                // owner.GetModPlayer<ScreenShakePlayer>()?.AddShake(8f, 12);
                
                Lighting.AddLight(target.Center, 2.5f, 1.2f, 0.4f);
            }
            else
            {
                Lighting.AddLight(target.Center, 1.5f, 0.7f, 0.2f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            
            // === TRUE ZENITH-STYLE SPECTRAL BLADE TRAIL ===
            // Draw multiple ghostly copies of the weapon sprite at old positions
            for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                float progress = 1f - (i / (float)Projectile.oldPos.Length);
                float trailScale = Projectile.scale * (0.3f + progress * 0.7f);
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                float trailRotation = Projectile.oldRot[i];
                
                // Gradient from black to orange to yellow (innermost to outermost)
                Color trailColor;
                if (progress < 0.4f)
                {
                    trailColor = Color.Lerp(ThemedParticles.CampanellaBlack, ThemedParticles.CampanellaOrange, progress / 0.4f);
                }
                else
                {
                    trailColor = Color.Lerp(ThemedParticles.CampanellaOrange, ThemedParticles.CampanellaYellow, (progress - 0.4f) / 0.6f);
                }
                trailColor *= progress * 0.6f * trailOpacity;
                
                Main.EntitySpriteDraw(texture, drawPos, null, trailColor, trailRotation, origin, 
                    trailScale, SpriteEffects.None, 0);
            }
            
            // Main sprite position
            Vector2 mainPos = Projectile.Center - Main.screenPosition;
            
            // === SPECTRAL BLADE RENDERING - SAME TEXTURE WITH BRIGHT GRADIENT OVERLAY ===
            // This distinguishes spectral copies from the real blade through intense glowing overlays
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            float pulse = 1f + (float)Math.Sin(spectralPulse) * 0.25f;
            float spectralTime = Main.GameUpdateCount * 0.08f + Projectile.whoAmI * 0.5f;
            
            // === BRIGHT GRADIENT OVERLAY - Makes spectral blades unmistakably different ===
            // Outer blazing aura - very visible orange/gold gradient
            float outerGradient = ((float)Math.Sin(spectralTime) + 1f) * 0.5f;
            Color outerColor = Color.Lerp(ThemedParticles.CampanellaOrange, ThemedParticles.CampanellaGold, outerGradient);
            Main.EntitySpriteDraw(texture, mainPos, null, outerColor * 0.7f, Projectile.rotation, origin,
                Projectile.scale * 1.5f * pulse, SpriteEffects.None, 0);
            
            // Mid layer - pulsing yellow/orange 
            float midGradient = ((float)Math.Sin(spectralTime * 1.3f + 0.5f) + 1f) * 0.5f;
            Color midColor = Color.Lerp(ThemedParticles.CampanellaYellow, ThemedParticles.CampanellaOrange, midGradient);
            Main.EntitySpriteDraw(texture, mainPos, null, midColor * 0.6f, Projectile.rotation, origin,
                Projectile.scale * 1.35f * pulse, SpriteEffects.None, 0);
            
            // Inner bright core - white/gold cycling
            float innerGradient = ((float)Math.Sin(spectralTime * 1.7f + 1f) + 1f) * 0.5f;
            Color innerColor = Color.Lerp(Color.White, ThemedParticles.CampanellaGold, innerGradient);
            Main.EntitySpriteDraw(texture, mainPos, null, innerColor * 0.5f, Projectile.rotation, origin,
                Projectile.scale * 1.2f * pulse, SpriteEffects.None, 0);
            
            // Hot white center glow
            Main.EntitySpriteDraw(texture, mainPos, null, Color.White * 0.35f, Projectile.rotation, origin,
                Projectile.scale * 1.05f * pulse, SpriteEffects.None, 0);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Base sprite with spectral tint - same texture but with golden glow tint
            Color spectralTint = Color.Lerp(Color.White, ThemedParticles.CampanellaGold, 0.4f + outerGradient * 0.2f);
            Main.EntitySpriteDraw(texture, mainPos, null, spectralTint * trailOpacity, Projectile.rotation, origin,
                Projectile.scale, SpriteEffects.None, 0);
            
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // === DEATH EXPLOSION! ===
            ThemedParticles.LaCampanellaBloomBurst(Projectile.Center, 0.7f);
            ThemedParticles.LaCampanellaMusicNotes(Projectile.Center, 4, 30f);
            ThemedParticles.LaCampanellaSparkles(Projectile.Center, 6, 25f);
            
            // Final spark burst
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color color = Main.rand.NextBool() ? ThemedParticles.CampanellaOrange : ThemedParticles.CampanellaYellow;
                var spark = new GlowSparkParticle(Projectile.Center, vel, true, 20, 0.4f, color,
                    new Vector2(0.4f, 1.4f), false, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            Lighting.AddLight(Projectile.Center, 0.6f, 0.3f, 0.1f);
        }

        private const float ChargePerHit = 8f;
    }

    /// <summary>
    /// Inferno Waltz - spinning flame dance AOE attack.
    /// Pure particle-based visual - no texture needed, drawn entirely with custom VFX.
    /// </summary>
    public class InfernoWaltzProjectile : ModProjectile
    {
        // Pure particle effect - no texture drawn
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/DualFatedChime";
        
        private int waveTimer = 0;
        private float pulsePhase = 0f;
        
        public override void SetDefaults()
        {
            Projectile.width = 300;
            Projectile.height = 300;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120; // 2 seconds
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            Projectile.Center = owner.Center;
            pulsePhase += 0.1f;
            
            // Spin the player visually
            owner.fullRotation = MathHelper.WrapAngle(owner.fullRotation + 0.3f);
            owner.fullRotationOrigin = owner.Size / 2f;
            
            waveTimer++;
            
            // Release flame waves periodically
            if (waveTimer % 15 == 0)
            {
                SpawnFlameWave(owner);
            }
            
            // === CHAINSAW-BELL CONTINUOUS SOUND ===
            // Layered sounds for sustained intense bell grinding
            if (waveTimer % 5 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item34 with { Pitch = Main.rand.NextFloat(0.1f, 0.5f), Volume = 0.25f }, owner.Center);
            }
            if (waveTimer % 8 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item35 with { Pitch = Main.rand.NextFloat(0.3f, 0.7f), Volume = 0.35f }, owner.Center);
            }
            if (waveTimer % 12 == 0)
            {
                SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot with { Pitch = 0.4f, Volume = 0.2f }, owner.Center);
            }
            
            // === MASSIVE CONTINUOUS PARTICLE EFFECTS ===
            // Spinning fire ring particles - MORE DENSE
            for (int i = 0; i < 12; i++)
            {
                float angle = waveTimer * 0.15f + MathHelper.TwoPi * i / 12f;
                float dist = 70f + (float)Math.Sin(pulsePhase + i) * 40f;
                Vector2 pos = owner.Center + angle.ToRotationVector2() * dist;
                
                // Fire glow particles
                Color color = Main.rand.Next(4) switch
                {
                    0 => ThemedParticles.CampanellaOrange,
                    1 => ThemedParticles.CampanellaYellow,
                    2 => ThemedParticles.CampanellaGold,
                    _ => ThemedParticles.CampanellaRed
                };
                
                var glow = new GenericGlowParticle(pos, 
                    angle.ToRotationVector2() * -3f + new Vector2(0, -1.5f), 
                    color, Main.rand.NextFloat(0.35f, 0.6f), Main.rand.Next(12, 22), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Black smoke spiral - DENSER
            for (int i = 0; i < 3; i++)
            {
                if (Main.rand.NextBool(2))
                {
                    float smokeAngle = waveTimer * 0.1f + i * MathHelper.TwoPi / 3f;
                    Vector2 smokePos = owner.Center + smokeAngle.ToRotationVector2() * Main.rand.NextFloat(30f, 90f);
                    var smoke = new HeavySmokeParticle(smokePos, 
                        new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, -1f)),
                        ThemedParticles.CampanellaBlack, Main.rand.Next(35, 55), 
                        Main.rand.NextFloat(0.5f, 0.8f), 0.6f, 0.025f, false);
                    MagnumParticleHandler.SpawnParticle(smoke);
                }
            }
            
            // Music notes spiraling outward - MORE
            if (Main.rand.NextBool(2))
            {
                ThemedParticles.LaCampanellaMusicNotes(owner.Center + Main.rand.NextVector2Circular(70f, 70f), 3, 25f);
            }
            
            // Spark bursts - MORE INTENSE
            for (int i = 0; i < 2; i++)
            {
                Vector2 sparkDir = Main.rand.NextVector2Unit();
                ThemedParticles.LaCampanellaSparks(owner.Center + sparkDir * 60f, sparkDir, 4, 7f);
            }
            
            // Halo rings pulsing outward - MORE FREQUENT
            if (waveTimer % 6 == 0)
            {
                float ringPhase = (waveTimer * 0.05f) % 1f;
                Color ringColor = Color.Lerp(ThemedParticles.CampanellaOrange, ThemedParticles.CampanellaYellow, ringPhase);
                CustomParticles.HaloRing(owner.Center, ringColor, 0.6f + (waveTimer % 20) * 0.03f, 25);
                CustomParticles.HaloRing(owner.Center, ThemedParticles.CampanellaBlack, 0.4f + (waveTimer % 15) * 0.02f, 18);
            }
            
            // Flare bursts at cardinal directions - MORE
            if (waveTimer % 5 == 0)
            {
                for (int i = 0; i < 6; i++)
                {
                    float flareAngle = MathHelper.TwoPi * i / 6f + waveTimer * 0.12f;
                    Vector2 flarePos = owner.Center + flareAngle.ToRotationVector2() * Main.rand.NextFloat(50f, 85f);
                    Color flareColor = Main.rand.Next(3) switch
                    {
                        0 => ThemedParticles.CampanellaOrange,
                        1 => ThemedParticles.CampanellaYellow,
                        _ => ThemedParticles.CampanellaGold
                    };
                    CustomParticles.GenericFlare(flarePos, flareColor, 0.5f, 18);
                }
            }
            
            // Continuous shockwaves
            if (waveTimer % 10 == 0)
            {
                ThemedParticles.LaCampanellaShockwave(owner.Center, 0.8f);
            }
            
            // Dynamic lighting - pulsing orange glow - BRIGHTER
            float lightPulse = 1f + (float)Math.Sin(pulsePhase * 2f) * 0.4f;
            Lighting.AddLight(owner.Center, 1.8f * lightPulse, 0.9f * lightPulse, 0.3f * lightPulse);
        }

        private void SpawnFlameWave(Player owner)
        {
            // Create circular flame wave
            int waveCount = 12;
            for (int i = 0; i < waveCount; i++)
            {
                float angle = MathHelper.TwoPi * i / waveCount;
                Vector2 velocity = angle.ToRotationVector2() * 12f;
                
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), owner.Center, velocity,
                    ModContent.ProjectileType<BellFlameWave>(), Projectile.damage / 2, 5f, Projectile.owner);
            }
            
            // Bell chime effect - MASSIVE!
            ThemedParticles.LaCampanellaBellChime(owner.Center, 1.2f);
            ThemedParticles.LaCampanellaShockwave(owner.Center, 1f);
            
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.2f, Volume = 0.6f }, owner.Center);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Resonant Toll - double stacks for this ultimate attack
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 2);
            
            // === CHAINSAW BELL HIT SOUNDS ===
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = Main.rand.NextFloat(0.2f, 0.5f), Volume = 0.45f }, target.Center);
            SoundEngine.PlaySound(SoundID.Item34 with { Pitch = Main.rand.NextFloat(0f, 0.3f), Volume = 0.3f }, target.Center);
            
            // === BLACK SMOKE SPARKLE MAGIC - SIGNATURE HIT EFFECT! ===
            Vector2 hitDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
            ThemedParticles.LaCampanellaSignatureHit(target.Center, hitDir, 1.2f);
            
            // Massive impact effects
            ThemedParticles.LaCampanellaImpact(target.Center, 0.9f);
            ThemedParticles.LaCampanellaMusicalImpact(target.Center, 0.7f, false);
            ThemedParticles.LaCampanellaSparks(target.Center, Main.rand.NextVector2Unit(), 10, 6f);
            ThemedParticles.LaCampanellaMusicNotes(target.Center, 5, 30f);
            
            // Halo rings
            CustomParticles.HaloRing(target.Center, ThemedParticles.CampanellaOrange, 0.5f, 20);
            CustomParticles.HaloRing(target.Center, ThemedParticles.CampanellaGold, 0.35f, 15);
            
            // Flares around impact
            for (int i = 0; i < 4; i++)
            {
                Color flareColor = Main.rand.NextBool() ? ThemedParticles.CampanellaOrange : ThemedParticles.CampanellaYellow;
                CustomParticles.GenericFlare(target.Center + Main.rand.NextVector2Circular(20f, 20f), flareColor, 0.4f, 15);
            }
            
            // Extra black smoke sparkle burst!
            ThemedParticles.LaCampanellaBlackSmokeSparkle(target.Center, 0.7f);
            
            // Screen shake removed from weapons - reserved for bosses only
            Player owner = Main.player[Projectile.owner];
            // owner.GetModPlayer<ScreenShakePlayer>()?.AddShake(2f, 4);
            
            Lighting.AddLight(target.Center, 1f, 0.5f, 0.15f);
        }

        public override void OnKill(int timeLeft)
        {
            Player owner = Main.player[Projectile.owner];
            owner.fullRotation = 0f; // Reset rotation
            
            // Final massive explosion
            ThemedParticles.LaCampanellaImpact(owner.Center, 2f);
            ThemedParticles.LaCampanellaBellChime(owner.Center, 1.5f);
            ThemedParticles.LaCampanellaMusicalImpact(owner.Center, 1.2f, true);
            
            // Radial spark burst
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 10f);
                Color color = Main.rand.Next(3) switch
                {
                    0 => ThemedParticles.CampanellaOrange,
                    1 => ThemedParticles.CampanellaYellow,
                    _ => ThemedParticles.CampanellaGold
                };
                var spark = new GlowSparkParticle(owner.Center, vel, true, 30, 0.6f, color,
                    new Vector2(0.5f, 1.8f), false, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0f, Volume = 0.6f }, owner.Center);
            Lighting.AddLight(owner.Center, 2f, 1f, 0.3f);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Circular collision
            float radius = Projectile.width / 2f;
            Vector2 center = Projectile.Center;
            
            return Vector2.Distance(center, targetHitbox.Center.ToVector2()) < radius + Math.Max(targetHitbox.Width, targetHitbox.Height) / 2f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Don't draw sprite - purely particle-based visual
            return false;
        }
    }

    /// <summary>
    /// Bell flame wave projectile from Inferno Waltz.
    /// Pure particle visual with no sprite needed.
    /// </summary>
    public class BellFlameWave : ModProjectile
    {
        // No actual texture needed - entirely particle-based
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/DualFatedChime";
        
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            // === BLAZING FLAME WAVE TRAIL ===
            // Continuous fire glow particles
            ThemedParticles.LaCampanellaTrail(Projectile.Center, Projectile.velocity);
            
            // Fire sparks
            if (Main.rand.NextBool(2))
            {
                ThemedParticles.LaCampanellaSparks(Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.UnitX), 2, 3f);
            }
            
            // Occasional music notes
            if (Main.rand.NextBool(8))
            {
                ThemedParticles.LaCampanellaMusicNotes(Projectile.Center, 1, 10f);
            }
            
            // Black smoke trail
            if (Main.rand.NextBool(4))
            {
                var smoke = new HeavySmokeParticle(Projectile.Center, 
                    -Projectile.velocity * 0.1f + new Vector2(0, -0.5f),
                    ThemedParticles.CampanellaBlack, Main.rand.Next(25, 40), 
                    Main.rand.NextFloat(0.25f, 0.4f), 0.4f, 0.015f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Dynamic lighting
            Lighting.AddLight(Projectile.Center, 0.5f, 0.25f, 0.08f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);
            
            // === CHAINSAW BELL HIT SOUND ===
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = Main.rand.NextFloat(0.3f, 0.7f), Volume = 0.35f }, target.Center);
            
            // === BLACK SMOKE SPARKLE MAGIC - SIGNATURE HIT EFFECT! ===
            Vector2 hitDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
            ThemedParticles.LaCampanellaSignatureHit(target.Center, hitDir, 0.8f);
            
            // Impact effect
            ThemedParticles.LaCampanellaSparks(target.Center, hitDir, 6, 5f);
            ThemedParticles.LaCampanellaBloomBurst(target.Center, 0.5f);
            ThemedParticles.LaCampanellaMusicNotes(target.Center, 2, 20f);
            
            // Black smoke sparkle burst!
            ThemedParticles.LaCampanellaBlackSmokeSparkle(target.Center, 0.5f);
            
            // Halo ring on hit
            CustomParticles.HaloRing(target.Center, ThemedParticles.CampanellaOrange, 0.35f, 12);
            
            Lighting.AddLight(target.Center, 0.6f, 0.3f, 0.1f);
        }

        public override void OnKill(int timeLeft)
        {
            // Small death burst
            ThemedParticles.LaCampanellaBloomBurst(Projectile.Center, 0.3f);
            ThemedParticles.LaCampanellaSparkles(Projectile.Center, 4, 15f);
        }

        public override bool PreDraw(ref Color lightColor) => false; // Pure particle visual
    }

    /// <summary>
    /// Inferno Waltz buff - movement speed increase with fire trail.
    /// </summary>
    public class InfernoWaltzBuff : ModBuff
    {
        // Use the weapon texture as a base - will be drawn with custom overlay
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/DualFatedChime";
        
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.moveSpeed += 0.35f;
            player.maxRunSpeed += 3f;
            
            // === BLAZING FLAME TRAIL WHILE BUFFED ===
            // Fire glow trail
            if (Main.rand.NextBool(3))
            {
                Color color = Main.rand.NextBool() ? ThemedParticles.CampanellaOrange : ThemedParticles.CampanellaYellow;
                var glow = new GenericGlowParticle(player.Center + Main.rand.NextVector2Circular(15f, 15f),
                    -player.velocity * 0.2f + new Vector2(0, -0.5f), color, 
                    Main.rand.NextFloat(0.2f, 0.4f), Main.rand.Next(15, 25), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Occasional sparks
            if (Main.rand.NextBool(6))
            {
                ThemedParticles.LaCampanellaSparks(player.Center, -player.velocity.SafeNormalize(Vector2.UnitY), 2, 3f);
            }
            
            // Black smoke wisps
            if (Main.rand.NextBool(8))
            {
                var smoke = new HeavySmokeParticle(player.Center + Main.rand.NextVector2Circular(10f, 10f),
                    -player.velocity * 0.1f, ThemedParticles.CampanellaBlack,
                    Main.rand.Next(20, 35), Main.rand.NextFloat(0.15f, 0.25f), 0.4f, 0.015f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Dynamic lighting around player
            Lighting.AddLight(player.Center, 0.5f, 0.25f, 0.08f);
        }
    }
}