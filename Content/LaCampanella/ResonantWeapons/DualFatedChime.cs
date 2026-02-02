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

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Swings like the Zenith, casting spectral blades wreathed in bell-music flames"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Attacks fill a charge bar - right-click to unleash Inferno Waltz"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Inferno Waltz creates a devastating spinning flame dance"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Applies Resonant Toll on hit"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Two fates intertwined in the dance of the eternal chime'") { OverrideColor = new Color(255, 140, 40) });
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
            
            // === SWING SOUNDS (2 max) ===
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = Main.rand.NextFloat(0.2f, 0.6f), Volume = 0.5f }, player.Center);
            SoundEngine.PlaySound(SoundID.Item34 with { Pitch = Main.rand.NextFloat(-0.2f, 0.2f), Volume = 0.3f }, player.Center);
            
            // === SWING EFFECTS (cleaned) ===
            Vector2 swingPoint = player.Center + toMouse * 40f;
            
            // UnifiedVFX handles core swing aura
            UnifiedVFX.LaCampanella.SwingAura(swingPoint, toMouse, 1.0f);
            
            // Sparks (reduced to 4)
            ThemedParticles.LaCampanellaSparks(swingPoint, toMouse, 4, 6f);
            
            // Music notes (reduced to 2)
            ThemedParticles.LaCampanellaMusicNotes(player.Center, 2, 25f);
            
            // Single halo ring
            CustomParticles.HaloRing(swingPoint, ThemedParticles.CampanellaOrange, 0.35f, 14);
            
            // Light
            Lighting.AddLight(swingPoint, 0.9f, 0.45f, 0.12f);
            
            return false;
        }

        private void ActivateInfernoWaltz(Player player, IEntitySource source, int damage)
        {
            // === INFERNO WALTZ ACTIVATION (cleaned) ===
            // 2 sounds max
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.3f, Volume = 0.9f }, player.Center);
            SoundEngine.PlaySound(SoundID.Item45 with { Pitch = 0.3f, Volume = 0.7f }, player.Center);
            
            // Spawn the Inferno Waltz projectile
            Projectile.NewProjectile(source, player.Center, Vector2.Zero, 
                ModContent.ProjectileType<InfernoWaltzProjectile>(), (int)(damage * 2f), 10f, player.whoAmI);
            
            // Grant movement speed buff
            player.AddBuff(ModContent.BuffType<InfernoWaltzBuff>(), 900); // 15 seconds
            
            // === ACTIVATION VFX (cleaned) ===
            // UnifiedVFX handles core impact (this is a SPECIAL so slightly more VFX is ok)
            UnifiedVFX.LaCampanella.Explosion(player.Center, 1.5f);
            UnifiedVFX.LaCampanella.BellChime(player.Center, 1.2f);
            
            // Music notes (special so keep 5)
            ThemedParticles.LaCampanellaMusicNotes(player.Center, 5, 40f);
            
            // Single halo ring
            CustomParticles.HaloRing(player.Center, ThemedParticles.CampanellaOrange, 0.7f, 22);
            
            // Spark burst (reduced to 8 for special)
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 10f);
                Color color = Color.Lerp(ThemedParticles.CampanellaBlack, ThemedParticles.CampanellaOrange, (float)i / 8f);
                var spark = new GlowSparkParticle(player.Center, vel, color, 0.6f, Main.rand.Next(20, 32));
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // Light flash
            Lighting.AddLight(player.Center, 2.0f, 1.0f, 0.35f);
        }

        public override void HoldItem(Player player)
        {
            // === SUBTLE AMBIENT AURA (cleaned) ===
            // Rare ambient flares
            if (Main.rand.NextBool(15))
            {
                float angle = Main.GameUpdateCount * 0.03f + Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 flarePos = player.Center + angle.ToRotationVector2() * Main.rand.NextFloat(30f, 45f);
                Color flareColor = Main.rand.NextBool() ? ThemedParticles.CampanellaOrange : ThemedParticles.CampanellaGold;
                CustomParticles.GenericFlare(flarePos, flareColor, 0.2f, 12);
            }
            
            // Display charge bar visually
            DrawChargeBar(player);
            
            // Subtle pulsing light
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.15f;
            Lighting.AddLight(player.Center, 0.4f * pulse, 0.2f * pulse, 0.06f * pulse);
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
            
            // Black ↁEOrange gradient glow
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
            
            // === ENHANCED ZENITH-STYLE PARTICLE TRAIL (IRIDESCENT WINGSPAN PATTERN) ===
            
            // HEAVY DUST TRAILS - infernal orange/gold (2+ per frame)
            for (int d = 0; d < 2; d++)
            {
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(8f, 8f);
                Dust dust1 = Dust.NewDustPerfect(dustPos, DustID.Torch, -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1.5f, 1.5f), 0, ThemedParticles.CampanellaOrange, 1.2f);
                dust1.noGravity = true;
                dust1.fadeIn = 1.4f;
                
                Dust dust2 = Dust.NewDustPerfect(dustPos + Main.rand.NextVector2Circular(5f, 5f), DustID.GoldCoin, -Projectile.velocity * 0.15f, 0, Color.White, 0.95f);
                dust2.noGravity = true;
                dust2.fadeIn = 1.3f;
            }
            
            // CONTRASTING SPARKLES - golden bell sparkles (1-in-2)
            if (Main.rand.NextBool(2))
            {
                var sparkle = new SparkleParticle(Projectile.Center, -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.8f, 0.8f), ThemedParticles.CampanellaGold, 0.45f, 16);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // INFERNAL SHIMMER TRAILS - cycling orange to gold hues (1-in-3)
            if (Main.rand.NextBool(3))
            {
                float hue = Main.rand.NextFloat(0.06f, 0.12f);
                Color shimmerColor = Main.hslToRgb(hue, 1f, 0.6f);
                var shimmer = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.2f, 1.2f), shimmerColor, 0.32f, 20, true);
                MagnumParticleHandler.SpawnParticle(shimmer);
            }
            
            // PEARLESCENT FLAME EFFECTS (1-in-4)
            if (Main.rand.NextBool(4))
            {
                float colorShift = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.5f + 0.5f;
                Color pearlColor = Color.Lerp(ThemedParticles.CampanellaOrange, ThemedParticles.CampanellaGold, colorShift) * 0.65f;
                var pearl = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f, pearlColor, 0.28f, 18, true);
                MagnumParticleHandler.SpawnParticle(pearl);
            }
            
            // FREQUENT FLARES (1-in-3)
            if (Main.rand.NextBool(3))
            {
                Color flareColor = Color.Lerp(ThemedParticles.CampanellaOrange, Color.White, Main.rand.NextFloat() * 0.3f);
                CustomParticles.GenericFlare(Projectile.Center, flareColor, Main.rand.NextFloat(0.22f, 0.38f), 12);
            }
            
            // SMOKE WISPS (1-in-5)
            if (Main.rand.NextBool(5))
            {
                var smoke = new HeavySmokeParticle(Projectile.Center, -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    ThemedParticles.CampanellaBlack * 0.45f, Main.rand.Next(16, 26), 0.22f, 0.45f, 0.015f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Use the comprehensive Zenith trail effect
            ThemedParticles.LaCampanellaZenithTrail(Projectile.Center, Projectile.velocity, Projectile.rotation);
            
            // Additional fire trail
            ThemedParticles.LaCampanellaTrail(Projectile.Center, Projectile.velocity);
            
            // MUSIC NOTES - bell melody (1-in-5)
            if (Main.rand.NextBool(5))
            {
                Vector2 noteVel = -Projectile.velocity * 0.05f + new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(-1.5f, -0.3f));
                ThemedParticles.MusicNote(Projectile.Center, noteVel, ThemedParticles.CampanellaGold, 0.8f, 24);
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
            if (currentPhase != ZenithPhase.ReturningToPlayer)
            {
                currentPhase = ZenithPhase.ReturningToPlayer;
            }
            
            // === HIT SOUND (1 only) ===
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = Main.rand.NextFloat(0.2f, 0.6f), Volume = 0.5f }, target.Center);
            
            // === HIT VFX (cleaned) ===
            Vector2 hitDirection = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
            
            // UnifiedVFX handles core impact
            UnifiedVFX.LaCampanella.Impact(target.Center, 0.8f);
            
            // Music notes (2)
            ThemedParticles.LaCampanellaMusicNotes(target.Center, 2, 20f);
            
            // Sparks (4)
            ThemedParticles.LaCampanellaSparks(target.Center, hitDirection, 4, 5f);
            
            // Critical hit extra effects
            if (hit.Crit)
            {
                // Crit sound
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.8f, Volume = 0.4f }, target.Center);
                
                // Extra bloom
                ThemedParticles.LaCampanellaBloomBurst(target.Center, 0.5f);
                
                Lighting.AddLight(target.Center, 1.8f, 0.9f, 0.3f);
            }
            else
            {
                Lighting.AddLight(target.Center, 1.2f, 0.6f, 0.18f);
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
            // === DEATH EFFECT (cleaned) ===
            ThemedParticles.LaCampanellaBloomBurst(Projectile.Center, 0.5f);
            ThemedParticles.LaCampanellaMusicNotes(Projectile.Center, 2, 20f);
            
            // Spark burst (reduced to 4)
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                Color color = Main.rand.NextBool() ? ThemedParticles.CampanellaOrange : ThemedParticles.CampanellaYellow;
                var spark = new GlowSparkParticle(Projectile.Center, vel, true, 18, 0.35f, color,
                    new Vector2(0.4f, 1.2f), false, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            Lighting.AddLight(Projectile.Center, 0.5f, 0.25f, 0.08f);
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
            
            // === CONTINUOUS SOUND (reduced - every 10 frames) ===
            if (waveTimer % 10 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item34 with { Pitch = Main.rand.NextFloat(0.1f, 0.5f), Volume = 0.2f }, owner.Center);
            }
            
            // === CONTINUOUS PARTICLE EFFECTS (cleaned) ===
            // Spinning fire ring - reduced from 12 to 6 particles
            for (int i = 0; i < 6; i++)
            {
                float angle = waveTimer * 0.15f + MathHelper.TwoPi * i / 6f;
                float dist = 70f + (float)Math.Sin(pulsePhase + i) * 40f;
                Vector2 pos = owner.Center + angle.ToRotationVector2() * dist;
                
                Color color = Main.rand.NextBool() ? ThemedParticles.CampanellaOrange : ThemedParticles.CampanellaYellow;
                var glow = new GenericGlowParticle(pos, 
                    angle.ToRotationVector2() * -3f + new Vector2(0, -1.5f), 
                    color, Main.rand.NextFloat(0.35f, 0.55f), Main.rand.Next(12, 20), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Black smoke spiral - reduced to 1
            if (Main.rand.NextBool(3))
            {
                float smokeAngle = waveTimer * 0.1f;
                Vector2 smokePos = owner.Center + smokeAngle.ToRotationVector2() * Main.rand.NextFloat(40f, 80f);
                var smoke = new HeavySmokeParticle(smokePos, 
                    new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, -0.5f)),
                    ThemedParticles.CampanellaBlack, Main.rand.Next(30, 45), 
                    Main.rand.NextFloat(0.4f, 0.6f), 0.5f, 0.02f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Music notes - less frequent
            if (Main.rand.NextBool(6))
            {
                ThemedParticles.LaCampanellaMusicNotes(owner.Center + Main.rand.NextVector2Circular(60f, 60f), 2, 20f);
            }
            
            // Sparks - reduced
            if (Main.rand.NextBool(3))
            {
                Vector2 sparkDir = Main.rand.NextVector2Unit();
                ThemedParticles.LaCampanellaSparks(owner.Center + sparkDir * 50f, sparkDir, 3, 5f);
            }
            
            // Halo rings - less frequent
            if (waveTimer % 15 == 0)
            {
                CustomParticles.HaloRing(owner.Center, ThemedParticles.CampanellaOrange, 0.5f, 20);
            }
            
            // Dynamic lighting - pulsing
            float lightPulse = 1f + (float)Math.Sin(pulsePhase * 2f) * 0.3f;
            Lighting.AddLight(owner.Center, 1.4f * lightPulse, 0.7f * lightPulse, 0.2f * lightPulse);
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
            
            // Single bell chime effect
            ThemedParticles.LaCampanellaBellChime(owner.Center, 0.8f);
            
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.2f, Volume = 0.5f }, owner.Center);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Resonant Toll - double stacks for this ultimate attack
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 2);
            
            // === HIT SOUND (1 only) ===
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = Main.rand.NextFloat(0.2f, 0.5f), Volume = 0.4f }, target.Center);
            
            // === HIT VFX (cleaned) ===
            UnifiedVFX.LaCampanella.Impact(target.Center, 0.7f);
            
            // Sparks (4)
            ThemedParticles.LaCampanellaSparks(target.Center, Main.rand.NextVector2Unit(), 4, 5f);
            
            // Music notes (2)
            ThemedParticles.LaCampanellaMusicNotes(target.Center, 2, 20f);
            
            Lighting.AddLight(target.Center, 0.8f, 0.4f, 0.12f);
        }

        public override void OnKill(int timeLeft)
        {
            Player owner = Main.player[Projectile.owner];
            owner.fullRotation = 0f; // Reset rotation
            
            // Final explosion (cleaned)
            UnifiedVFX.LaCampanella.Explosion(owner.Center, 1.2f);
            
            // Spark burst (reduced to 8)
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 8f);
                Color color = Main.rand.NextBool() ? ThemedParticles.CampanellaOrange : ThemedParticles.CampanellaYellow;
                var spark = new GlowSparkParticle(owner.Center, vel, true, 25, 0.5f, color,
                    new Vector2(0.5f, 1.6f), false, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0f, Volume = 0.5f }, owner.Center);
            Lighting.AddLight(owner.Center, 1.5f, 0.75f, 0.25f);
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
            // === FLAME WAVE TRAIL (cleaned) ===
            // Fire glow trail
            ThemedParticles.LaCampanellaTrail(Projectile.Center, Projectile.velocity);
            
            // Occasional sparks
            if (Main.rand.NextBool(4))
            {
                ThemedParticles.LaCampanellaSparks(Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.UnitX), 2, 3f);
            }
            
            // Rare smoke trail
            if (Main.rand.NextBool(8))
            {
                var smoke = new HeavySmokeParticle(Projectile.Center, 
                    -Projectile.velocity * 0.1f + new Vector2(0, -0.5f),
                    ThemedParticles.CampanellaBlack, Main.rand.Next(20, 35), 
                    Main.rand.NextFloat(0.2f, 0.35f), 0.35f, 0.012f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            Lighting.AddLight(Projectile.Center, 0.4f, 0.2f, 0.06f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);
            
            // === HIT VFX (cleaned) ===
            Vector2 hitDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
            
            // Core impact
            UnifiedVFX.LaCampanella.Impact(target.Center, 0.5f);
            
            // Sparks (3)
            ThemedParticles.LaCampanellaSparks(target.Center, hitDir, 3, 4f);
            
            Lighting.AddLight(target.Center, 0.5f, 0.25f, 0.08f);
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
            
            // === SUBTLE FLAME TRAIL (cleaned) ===
            // Rare fire glow
            if (Main.rand.NextBool(6))
            {
                Color color = Main.rand.NextBool() ? ThemedParticles.CampanellaOrange : ThemedParticles.CampanellaYellow;
                var glow = new GenericGlowParticle(player.Center + Main.rand.NextVector2Circular(12f, 12f),
                    -player.velocity * 0.15f + new Vector2(0, -0.4f), color, 
                    Main.rand.NextFloat(0.18f, 0.3f), Main.rand.Next(12, 20), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Occasional sparks
            if (Main.rand.NextBool(12))
            {
                ThemedParticles.LaCampanellaSparks(player.Center, -player.velocity.SafeNormalize(Vector2.UnitY), 2, 2.5f);
            }
            
            // Subtle lighting
            Lighting.AddLight(player.Center, 0.35f, 0.18f, 0.05f);
        }
    }
}