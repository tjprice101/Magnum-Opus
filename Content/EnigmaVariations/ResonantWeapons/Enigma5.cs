using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.EnigmaVariations.Debuffs;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons
{
    /// <summary>
    /// PARADOX IMPALER - Enigma Melee Spear/Polearm
    /// =============================================
    /// UNIQUE MECHANICS:
    /// - Thrust creates AFTERIMAGES that continue the attack pattern
    /// - Each afterimage deals 50% damage and applies stacking debuff
    /// - At 5 Paradox stacks on enemy, they EXPLODE with paradox energy
    /// - Extended combos (3+ thrusts without stopping) create glyph formations
    /// - Hold attack to CHARGE: release creates massive reality-piercing thrust
    /// - Charge attack pierces through all enemies and terrain briefly
    /// - Eyes watch from afterimage positions
    /// </summary>
    public class Enigma5 : ModItem
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private int comboCount = 0;
        private int comboTimer = 0;
        private const int ComboResetTime = 60;
        private int chargeTimer = 0;
        private const int MaxChargeTime = 60;
        private bool isCharging = false;
        
        public override string Texture => "Terraria/Images/Item_" + ItemID.ChlorophytePartisan;
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override void SetDefaults()
        {
            Item.damage = 500;
            Item.DamageType = DamageClass.Melee;
            Item.width = 60;
            Item.height = 60;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Thrust;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(gold: 18);
            Item.rare = ModContent.RarityType<EnigmaRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<ParadoxImpalerThrust>();
            Item.shootSpeed = 6f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true; // Allow charging
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect1", "Thrusts create damaging afterimages"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect2", "Enemies at 5 Paradox stacks trigger an explosion"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect3", "Hold to charge a reality-piercing thrust"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaLore", "'Each thrust impales causality itself'") 
            { 
                OverrideColor = EnigmaPurple 
            });
        }
        
        public override void HoldItem(Player player)
        {
            if (comboTimer > 0)
            {
                comboTimer--;
                if (comboTimer <= 0)
                {
                    comboCount = 0;
                }
            }
            
            // Combo visual effects
            if (comboCount >= 3)
            {
                float comboIntensity = Math.Min(1f, comboCount / 6f);
                
                // Rotating glyph formation around player
                if (Main.rand.NextBool(5))
                {
                    CustomParticles.GlyphCircle(player.Center, GetEnigmaGradient(comboIntensity), 
                        count: comboCount, radius: 50f, rotationSpeed: 0.05f);
                }
                
                // Orbiting sparkle wisps
                if (Main.rand.NextBool(10))
                {
                    float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                    Vector2 wispPos = player.Center + angle.ToRotationVector2() * 55f;
                    CustomParticles.GenericFlare(wispPos, GetEnigmaGradient(Main.rand.NextFloat()) * 0.7f, 0.35f, 14);
                }
            }
            
            // Charging mechanics
            if (player.channel && player.itemAnimation > 0)
            {
                if (!isCharging)
                {
                    isCharging = true;
                    chargeTimer = 0;
                }
                
                chargeTimer++;
                if (chargeTimer > MaxChargeTime)
                    chargeTimer = MaxChargeTime;
                
                float chargeProgress = (float)chargeTimer / MaxChargeTime;
                
                // Charge visuals
                if (chargeProgress > 0.2f)
                {
                    // Particles converging on player
                    if (Main.rand.NextBool(3))
                    {
                        float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                        float radius = 80f * (1f - chargeProgress * 0.5f);
                        Vector2 particlePos = player.Center + angle.ToRotationVector2() * radius;
                        Vector2 vel = (player.Center - particlePos).SafeNormalize(Vector2.Zero) * (4f + chargeProgress * 4f);
                        
                        var glow = new GenericGlowParticle(particlePos, vel, GetEnigmaGradient(chargeProgress) * 0.8f, 
                            0.3f + chargeProgress * 0.2f, 15, true);
                        MagnumParticleHandler.SpawnParticle(glow);
                    }
                    
                    // Growing glyph circle
                    if (Main.GameUpdateCount % 10 == 0)
                    {
                        CustomParticles.GlyphCircle(player.Center, GetEnigmaGradient(chargeProgress), 
                            count: 4 + (int)(chargeProgress * 4), radius: 35f + chargeProgress * 20f, rotationSpeed: 0.08f);
                    }
                    
                    // Converging sparkle ring
                    if (chargeProgress > 0.6f && Main.rand.NextBool(8))
                    {
                        float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                        Vector2 sparklePos = player.Center + angle.ToRotationVector2() * 60f * (1f - chargeProgress * 0.3f);
                        Vector2 sparkleVel = (player.Center - sparklePos).SafeNormalize(Vector2.Zero) * 3f;
                        var sparkle = new GenericGlowParticle(sparklePos, sparkleVel, EnigmaGreen * 0.8f, 0.35f, 14, true);
                        MagnumParticleHandler.SpawnParticle(sparkle);
                    }
                }
                
                Lighting.AddLight(player.Center, EnigmaPurple.ToVector3() * 0.5f * chargeProgress);
            }
            else if (isCharging)
            {
                isCharging = false;
                chargeTimer = 0;
            }
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            comboCount++;
            comboTimer = ComboResetTime;
            
            // Check if charged attack
            float chargeProgress = isCharging ? (float)chargeTimer / MaxChargeTime : 0f;
            bool isChargedAttack = chargeProgress >= 0.9f;
            
            if (isChargedAttack)
            {
                // CHARGED THRUST - massive reality-piercing attack
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.2f, Volume = 0.9f }, player.Center);
                
                int chargedDamage = (int)(damage * 2.5f);
                Projectile.NewProjectile(source, position, velocity * 1.5f, 
                    ModContent.ProjectileType<ChargedParadoxThrust>(), chargedDamage, knockback * 2f, player.whoAmI);
                
                // Charged attack VFX
                CustomParticles.GenericFlare(player.Center, Color.White, 1.0f, 22);
                
                for (int ring = 0; ring < 5; ring++)
                {
                    CustomParticles.HaloRing(player.Center, GetEnigmaGradient(ring / 5f), 0.4f + ring * 0.2f, 16 + ring * 3);
                }
                
                CustomParticles.GlyphBurst(player.Center, EnigmaGreen, count: 10, speed: 6f);
                
                // Sparkle explosion burst
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 burstVel = angle.ToRotationVector2() * 5f;
                    var sparkle = new GenericGlowParticle(player.Center, burstVel, GetEnigmaGradient((float)i / 8f), 0.4f, 18, true);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
                
                // Music notes burst for charged attack - the paradox symphony
                ThemedParticles.EnigmaMusicNoteBurst(player.Center, 12, 6f);
                
                chargeTimer = 0;
                isCharging = false;
            }
            else
            {
                // Normal thrust with afterimages
                Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, ai0: comboCount);
                
                // Spawn afterimages for combo thrusts
                if (comboCount >= 2)
                {
                    int afterimageCount = Math.Min(comboCount - 1, 3);
                    for (int i = 1; i <= afterimageCount; i++)
                    {
                        float angleOffset = (i % 2 == 0 ? 1 : -1) * MathHelper.Pi * 0.1f * i;
                        Vector2 afterimageVel = velocity.RotatedBy(angleOffset);
                        Vector2 afterimagePos = position + afterimageVel.SafeNormalize(Vector2.Zero) * 15f * i;
                        
                        Projectile.NewProjectile(source, afterimagePos, afterimageVel * 0.9f, 
                            ModContent.ProjectileType<ParadoxAfterimage>(), damage / 2, knockback * 0.5f, 
                            player.whoAmI, ai0: i);
                    }
                }
                
                // Thrust VFX
                Vector2 thrustPos = position + velocity.SafeNormalize(Vector2.Zero) * 40f;
                for (int i = 0; i < 6; i++)
                {
                    float angle = velocity.ToRotation() + MathHelper.PiOver4 * ((float)i / 6f - 0.5f);
                    Vector2 offset = angle.ToRotationVector2() * 25f;
                    CustomParticles.GenericFlare(thrustPos + offset, GetEnigmaGradient((float)i / 6f), 0.4f, 14);
                }
                
                CustomParticles.HaloRing(thrustPos, EnigmaPurple * 0.7f, 0.3f, 12);
                
                // Music notes on thrust - each impale strikes a chord
                ThemedParticles.EnigmaMusicNotes(thrustPos, 4, 30f);
            }
            
            return false;
        }
    }
    
    /// <summary>
    /// Normal thrust projectile
    /// </summary>
    public class ParadoxImpalerThrust : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private int ComboCount => (int)Projectile.ai[0];
        
        // Use invisible texture - we render everything with particles
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            // Custom-drawn paradox impaler with enigma eyes and glyphs
            SpriteBatch spriteBatch = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            // Switch to additive blending for vibrant effects
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Texture2D glyphTex = CustomParticleSystem.RandomGlyph().Value;
            Texture2D sparkleTex = CustomParticleSystem.RandomPrismaticSparkle().Value;
            Texture2D flareTex = CustomParticleSystem.EnergyFlares[0].Value;
            
            // Draw glyph trail at old positions
            for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float trailProgress = (float)i / Projectile.oldPos.Length;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Color trailColor = Color.Lerp(EnigmaPurple, EnigmaGreen, trailProgress) * (1f - trailProgress) * 0.6f;
                float trailRot = Projectile.oldRot[i] + i * 0.3f;
                spriteBatch.Draw(glyphTex, trailPos, null, trailColor, trailRot, glyphTex.Size() / 2f, 0.18f * (1f - trailProgress * 0.5f), SpriteEffects.None, 0f);
            }
            
            // Draw orbiting sparkles around thrust
            for (int i = 0; i < 5; i++)
            {
                float angle = Main.GameUpdateCount * 0.1f + MathHelper.TwoPi * i / 5f;
                float radius = 18f + (float)Math.Sin(Main.GameUpdateCount * 0.08f + i) * 6f;
                Vector2 sparkPos = drawPos + angle.ToRotationVector2() * radius;
                Color sparkColor = Color.Lerp(EnigmaPurple, EnigmaGreen, (float)i / 5f) * 0.7f;
                spriteBatch.Draw(sparkleTex, sparkPos, null, sparkColor, angle * 2f, sparkleTex.Size() / 2f, 0.15f, SpriteEffects.None, 0f);
            }
            
            // Draw central flare core with rotation
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.15f + 0.85f;
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaPurple * 0.8f, Main.GameUpdateCount * 0.04f, flareTex.Size() / 2f, 0.45f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaGreen * 0.7f, -Main.GameUpdateCount * 0.05f, flareTex.Size() / 2f, 0.25f * pulse, SpriteEffects.None, 0f);
            
            // Restore normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false; // Don't draw default sprite
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 35;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.extraUpdates = 1;
        }
        
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            
            // Position relative to player
            float progress = 1f - (Projectile.timeLeft / 35f);
            float extension = (float)Math.Sin(progress * MathHelper.Pi) * 60f;
            Projectile.Center = owner.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * extension;
            
            // Trail
            if (Main.rand.NextBool(2))
            {
                float gradientProgress = Main.rand.NextFloat();
                Color trailColor = GetEnigmaGradient(gradientProgress);
                CustomParticles.GenericFlare(Projectile.Center, trailColor * 0.7f, 0.32f, 14);
                
                var glow = new GenericGlowParticle(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
                    -Projectile.velocity * 0.15f, trailColor * 0.5f, 0.22f, 12, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Glyph trail for combo thrusts
            if (ComboCount >= 2 && Projectile.timeLeft % 8 == 0)
            {
                CustomParticles.GlyphTrail(Projectile.Center, Projectile.velocity, EnigmaPurple, 0.28f);
            }
            
            Lighting.AddLight(Projectile.Center, EnigmaPurple.ToVector3() * 0.45f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 300);
            var brandNPC = target.GetGlobalNPC<ParadoxBrandNPC>();
            brandNPC.AddParadoxStack(target, 1);
            
            int stacks = brandNPC.paradoxStacks;
            
            // At 5 stacks: EXPLOSION
            if (stacks >= 5)
            {
                TriggerParadoxExplosion(target, brandNPC);
            }
            
            // === REALITY WARP DISTORTION ===
            FateRealityDistortion.TriggerChromaticAberration(target.Center, 3f, 10);
            
            // === NEW UNIFIED VFX HIT EFFECT ===
            UnifiedVFX.EnigmaVariations.HitEffect(target.Center, 1.2f);
            
            // === WATCHING EYE AT IMPACT ===
            CustomParticles.EnigmaEyeImpact(target.Center, target.Center, EnigmaGreen, 0.5f);
            
            // === MUSIC NOTES BURST ===
            ThemedParticles.EnigmaMusicNoteBurst(target.Center, 10, 6f);
            ThemedParticles.EnigmaMusicNotes(target.Center, 5, 35f);
            
            // Impact VFX
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 offset = angle.ToRotationVector2() * 25f;
                CustomParticles.GenericFlare(target.Center + offset, GetEnigmaGradient((float)i / 8f), 0.45f, 16);
            }
            
            // Sparkle crown above target
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.Pi + MathHelper.Pi * i / 5f - MathHelper.PiOver2;
                Vector2 crownPos = target.Center - new Vector2(0, 30f) + angle.ToRotationVector2() * 22f;
                CustomParticles.GenericFlare(crownPos, GetEnigmaGradient((float)i / 5f), 0.42f, 16);
            }
            CustomParticles.HaloRing(target.Center, EnigmaPurple, 0.4f, 15);
            
            // === GLYPH CIRCLE FORMATION ===
            CustomParticles.GlyphCircle(target.Center, EnigmaPurple, count: 6, radius: 45f, rotationSpeed: 0.06f);
            
            // Glyph stack display
            if (stacks > 0 && stacks < 5)
            {
                CustomParticles.GlyphStack(target.Center + new Vector2(0, -28f), EnigmaGreen, stacks, 0.28f);
            }
            
            // === DYNAMIC LIGHTING ===
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.8f);
        }
        
        private void TriggerParadoxExplosion(NPC target, ParadoxBrandNPC brandNPC)
        {
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 1.0f }, target.Center);
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.5f, Volume = 0.8f }, target.Center);
            
            brandNPC.paradoxStacks = 0;
            
            float explosionRadius = 180f;
            
            // Central flash
            CustomParticles.GenericFlare(target.Center, Color.White, 1.3f, 26);
            
            // Halo rings
            for (int ring = 0; ring < 6; ring++)
            {
                CustomParticles.HaloRing(target.Center, GetEnigmaGradient(ring / 6f), 0.4f + ring * 0.22f, 17 + ring * 4);
            }
            
            // Fractal burst
            for (int layer = 0; layer < 3; layer++)
            {
                int points = 8 + layer * 3;
                float radius = 35f + layer * 40f;
                
                for (int i = 0; i < points; i++)
                {
                    float angle = MathHelper.TwoPi * i / points + layer * 0.25f;
                    Vector2 offset = angle.ToRotationVector2() * radius;
                    CustomParticles.GenericFlare(target.Center + offset, GetEnigmaGradient((float)i / points), 
                        0.65f - layer * 0.12f, 22);
                }
            }
            
            // Grand sparkle nova
            for (int layer = 0; layer < 2; layer++)
            {
                int points = 6 + layer * 2;
                float radius = 50f + layer * 25f;
                for (int i = 0; i < points; i++)
                {
                    float angle = MathHelper.TwoPi * i / points + layer * 0.2f;
                    Vector2 sparklePos = target.Center + angle.ToRotationVector2() * radius;
                    CustomParticles.GenericFlare(sparklePos, GetEnigmaGradient((float)i / points), 0.5f - layer * 0.1f, 20);
                    
                    Vector2 sparkleVel = angle.ToRotationVector2() * 4f;
                    var glow = new GenericGlowParticle(sparklePos, sparkleVel, EnigmaPurple * 0.7f, 0.35f, 16, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
            }
            ThemedParticles.EnigmaMusicNoteBurst(target.Center, 8, 4f);
            
            // Glyph circles
            CustomParticles.GlyphCircle(target.Center, EnigmaPurple, count: 8, radius: 50f, rotationSpeed: 0.06f);
            CustomParticles.GlyphBurst(target.Center, EnigmaGreen, count: 10, speed: 5f);
            
            // Damage nearby enemies
            Player owner = Main.player[Projectile.owner];
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly || npc.whoAmI == target.whoAmI) continue;
                
                float dist = Vector2.Distance(npc.Center, target.Center);
                if (dist <= explosionRadius)
                {
                    float falloff = 1f - (dist / explosionRadius) * 0.4f;
                    int explosionDamage = (int)(Projectile.damage * 2f * falloff);
                    npc.SimpleStrikeNPC(explosionDamage, owner.direction, true, 10f);
                    npc.AddBuff(ModContent.BuffType<ParadoxBrand>(), 240);
                    npc.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(npc, 2);
                    
                    // Chain lightning
                    MagnumVFX.DrawFractalLightning(target.Center, npc.Center, EnigmaGreen, 10, 28f, 4, 0.35f);
                    
                    // Sparkle indicator at struck enemy
                    for (int sparkle = 0; sparkle < 4; sparkle++)
                    {
                        float sparkleAngle = MathHelper.TwoPi * sparkle / 4f;
                        Vector2 sparklePos = npc.Center - new Vector2(0, 22f) + sparkleAngle.ToRotationVector2() * 14f;
                        CustomParticles.GenericFlare(sparklePos, GetEnigmaGradient((float)sparkle / 4f), 0.36f, 14);
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Afterimage projectile from combo attacks
    /// </summary>
    public class ParadoxAfterimage : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private int AfterimageIndex => (int)Projectile.ai[0];
        
        // Custom-rendered projectile - no vanilla sprites
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            // Custom ghost afterimage with watching eye
            SpriteBatch spriteBatch = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float opacity = 1f - Projectile.alpha / 255f;
            
            // Switch to additive blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Select eye based on afterimage index for variety
            Texture2D eyeTex = CustomParticleSystem.EnigmaEyes[AfterimageIndex % 8].Value;
            Texture2D sparkleTex = CustomParticleSystem.RandomPrismaticSparkle().Value;
            Texture2D flareTex = CustomParticleSystem.EnergyFlares[0].Value;
            
            // Draw small orbiting sparkles around the ghost
            for (int i = 0; i < 3; i++)
            {
                float angle = Main.GameUpdateCount * 0.12f + MathHelper.TwoPi * i / 3f + AfterimageIndex * 0.5f;
                float radius = 12f * Projectile.scale;
                Vector2 sparkPos = drawPos + angle.ToRotationVector2() * radius;
                Color sparkColor = GetEnigmaGradient((float)i / 3f) * opacity * 0.6f;
                spriteBatch.Draw(sparkleTex, sparkPos, null, sparkColor, angle * 1.5f, sparkleTex.Size() / 2f, 0.12f * Projectile.scale, SpriteEffects.None, 0f);
            }
            
            // Draw faint outer glow
            Color ghostColor = GetEnigmaGradient((float)AfterimageIndex / 3f) * opacity * 0.5f;
            spriteBatch.Draw(flareTex, drawPos, null, ghostColor, Main.GameUpdateCount * 0.02f, flareTex.Size() / 2f, 0.35f * Projectile.scale, SpriteEffects.None, 0f);
            
            // Draw central watching eye - pointing toward projectile velocity
            float eyeRotation = Projectile.velocity.ToRotation();
            Color eyeColor = Color.Lerp(EnigmaPurple, EnigmaGreen, (float)AfterimageIndex / 3f) * opacity * 0.8f;
            spriteBatch.Draw(eyeTex, drawPos, null, eyeColor, eyeRotation, eyeTex.Size() / 2f, 0.22f * Projectile.scale, SpriteEffects.None, 0f);
            
            // Restore normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 25;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 120;
            Projectile.extraUpdates = 1;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            
            // Fade out
            Projectile.alpha = 120 + (int)((1f - Projectile.timeLeft / 25f) * 135);
            Projectile.scale = 0.8f - (1f - Projectile.timeLeft / 25f) * 0.3f;
            
            // Ghost trail
            if (Main.rand.NextBool(2))
            {
                Color ghostColor = GetEnigmaGradient((float)AfterimageIndex / 3f) * (1f - Projectile.alpha / 255f);
                CustomParticles.GenericFlare(Projectile.Center, ghostColor * 0.6f, 0.25f, 12);
            }
            
            // Sparkle flash at afterimage peak
            if (Projectile.timeLeft == 15)
            {
                CustomParticles.GenericFlare(Projectile.Center, GetEnigmaGradient((float)AfterimageIndex / 3f), 0.42f, 16);
                CustomParticles.HaloRing(Projectile.Center, EnigmaPurple * 0.5f, 0.28f, 12);
            }
            
            Lighting.AddLight(Projectile.Center, EnigmaPurple.ToVector3() * 0.25f * (1f - Projectile.alpha / 255f));
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 240);
            target.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(target, 1);
            
            // === REALITY WARP DISTORTION ===
            FateRealityDistortion.TriggerChromaticAberration(target.Center, 2.5f, 8);
            
            // === NEW UNIFIED VFX HIT EFFECT ===
            UnifiedVFX.EnigmaVariations.HitEffect(target.Center, 1.2f);
            
            // === WATCHING EYE AT IMPACT ===
            CustomParticles.EnigmaEyeImpact(target.Center, target.Center, EnigmaGreen, 0.5f);
            
            // === MUSIC NOTES BURST ===
            ThemedParticles.EnigmaMusicNoteBurst(target.Center, 10, 6f);
            ThemedParticles.EnigmaMusicNotes(target.Center, 5, 35f);
            
            // Light impact
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 offset = angle.ToRotationVector2() * 18f;
                CustomParticles.GenericFlare(target.Center + offset, GetEnigmaGradient((float)i / 5f) * 0.6f, 0.32f, 12);
            }
            
            CustomParticles.HaloRing(target.Center, EnigmaPurple * 0.5f, 0.28f, 12);
            
            // === GLYPH CIRCLE FORMATION ===
            CustomParticles.GlyphCircle(target.Center, EnigmaPurple, count: 6, radius: 45f, rotationSpeed: 0.06f);
            
            // === DYNAMIC LIGHTING ===
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.8f);
        }
    }
    
    /// <summary>
    /// Charged thrust that pierces everything
    /// </summary>
    public class ChargedParadoxThrust : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private List<int> hitEnemies = new List<int>();
        
        // Custom-rendered reality-piercing thrust - no vanilla sprites
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            // Massive charged thrust with orbiting glyphs and eyes
            SpriteBatch spriteBatch = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            // Switch to additive blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Texture2D eyeTex = CustomParticleSystem.RandomEnigmaEye().Value;
            Texture2D glyphTex = CustomParticleSystem.RandomGlyph().Value;
            Texture2D sparkleTex = CustomParticleSystem.RandomPrismaticSparkle().Value;
            Texture2D flareTex = CustomParticleSystem.EnergyFlares[0].Value;
            
            // Draw intense glyph trail at old positions
            for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float trailProgress = (float)i / Projectile.oldPos.Length;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Color trailColor = Color.Lerp(EnigmaPurple, EnigmaGreen, trailProgress) * (1f - trailProgress) * 0.7f;
                Texture2D trailGlyph = CustomParticleSystem.GetGlyph(i % 12).Value;
                spriteBatch.Draw(trailGlyph, trailPos, null, trailColor, Projectile.oldRot[i] + i * 0.4f, trailGlyph.Size() / 2f, 0.25f * (1f - trailProgress * 0.4f), SpriteEffects.None, 0f);
            }
            
            // Draw orbiting glyphs - larger formation for charged attack
            for (int i = 0; i < 8; i++)
            {
                float angle = Main.GameUpdateCount * 0.06f + MathHelper.TwoPi * i / 8f;
                float radius = 35f + (float)Math.Sin(Main.GameUpdateCount * 0.08f + i) * 10f;
                Vector2 glyphPos = drawPos + angle.ToRotationVector2() * radius;
                Color glyphColor = Color.Lerp(EnigmaBlack, Color.Lerp(EnigmaPurple, EnigmaGreen, (float)i / 8f), 0.7f) * 0.75f;
                Texture2D orbitGlyph = CustomParticleSystem.GetGlyph(i % 12).Value;
                spriteBatch.Draw(orbitGlyph, glyphPos, null, glyphColor, angle * 2.5f, orbitGlyph.Size() / 2f, 0.22f, SpriteEffects.None, 0f);
            }
            
            // Draw orbiting sparkles in inner ring
            for (int i = 0; i < 6; i++)
            {
                float angle = Main.GameUpdateCount * 0.1f + MathHelper.TwoPi * i / 6f;
                float radius = 20f;
                Vector2 sparkPos = drawPos + angle.ToRotationVector2() * radius;
                Color sparkColor = Color.Lerp(EnigmaPurple, EnigmaGreen, (float)i / 6f) * 0.65f;
                spriteBatch.Draw(sparkleTex, sparkPos, null, sparkColor, angle * 1.8f, sparkleTex.Size() / 2f, 0.18f, SpriteEffects.None, 0f);
            }
            
            // Draw central watching eye
            float eyeRotation = Projectile.velocity.ToRotation();
            spriteBatch.Draw(eyeTex, drawPos, null, EnigmaGreen * 0.85f, eyeRotation, eyeTex.Size() / 2f, 0.35f, SpriteEffects.None, 0f);
            
            // Draw pulsing flare core behind eye
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.2f + 1f;
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaPurple * 0.7f, Main.GameUpdateCount * 0.03f, flareTex.Size() / 2f, 0.55f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaGreen * 0.5f, -Main.GameUpdateCount * 0.04f, flareTex.Size() / 2f, 0.35f * pulse, SpriteEffects.None, 0f);
            
            // Restore normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1; // Infinite pierce
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false; // Pierces terrain!
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            
            // Massive trail effect
            float gradientProgress = (Main.GameUpdateCount * 0.08f) % 1f;
            Color trailColor = GetEnigmaGradient(gradientProgress);
            
            CustomParticles.GenericFlare(Projectile.Center, trailColor, 0.55f, 18);
            
            if (Main.rand.NextBool(2))
            {
                var glow = new GenericGlowParticle(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(2f, 2f),
                    GetEnigmaGradient(Main.rand.NextFloat()) * 0.75f, 0.35f, 18, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Sparkle beacon along thrust path
            if (Projectile.timeLeft % 10 == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    float beaconAngle = MathHelper.TwoPi * i / 3f;
                    Vector2 beaconPos = Projectile.Center + beaconAngle.ToRotationVector2() * 12f;
                    CustomParticles.GenericFlare(beaconPos, GetEnigmaGradient((float)i / 3f), 0.42f, 14);
                }
            }
            
            // Glyph trail
            if (Projectile.timeLeft % 8 == 0)
            {
                CustomParticles.GlyphTrail(Projectile.Center, Projectile.velocity, EnigmaPurple, 0.35f);
            }
            
            // Periodic halo
            if (Projectile.timeLeft % 12 == 0)
            {
                CustomParticles.HaloRing(Projectile.Center, GetEnigmaGradient(Projectile.timeLeft / 60f), 0.35f, 14);
            }
            
            // Green flame accents
            if (Main.rand.NextBool(3))
            {
                CustomParticles.GenericFlare(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f), 
                    EnigmaGreen * 0.6f, 0.28f, 12);
            }
            
            Lighting.AddLight(Projectile.Center, EnigmaGreen.ToVector3() * 0.7f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 360);
            var brandNPC = target.GetGlobalNPC<ParadoxBrandNPC>();
            brandNPC.AddParadoxStack(target, 3); // Heavy stacking from charged attack
            
            hitEnemies.Add(target.whoAmI);
            
            // === CHARGED THRUST REALITY WARP (STRONGER) ===
            FateRealityDistortion.TriggerChromaticAberration(target.Center, 5f, 18);
            FateRealityDistortion.TriggerInversionPulse(6);
            
            // === NEW UNIFIED VFX EXPLOSION (CHARGED ATTACK) ===
            UnifiedVFX.EnigmaVariations.Explosion(target.Center, 1.5f);
            
            // === WATCHING EYE AT IMPACT ===
            CustomParticles.EnigmaEyeImpact(target.Center, target.Center, EnigmaGreen, 0.5f);
            
            // === MUSIC NOTES BURST ===
            ThemedParticles.EnigmaMusicNoteBurst(target.Center, 10, 6f);
            ThemedParticles.EnigmaMusicNotes(target.Center, 5, 35f);
            
            // Chain to all previously hit enemies
            foreach (int npcIndex in hitEnemies)
            {
                if (npcIndex == target.whoAmI) continue;
                if (npcIndex < 0 || npcIndex >= Main.maxNPCs) continue;
                
                NPC other = Main.npc[npcIndex];
                if (!other.active || other.friendly) continue;
                
                float dist = Vector2.Distance(target.Center, other.Center);
                if (dist > 400f) continue;
                
                // Heavy chain lightning
                MagnumVFX.DrawFractalLightning(target.Center, other.Center, EnigmaGreen, 14, 35f, 5, 0.4f);
                
                // Chain damage
                other.SimpleStrikeNPC(Projectile.damage / 3, 0, false, 4f);
            }
            
            // Massive impact VFX
            CustomParticles.GenericFlare(target.Center, Color.White, 0.9f, 22);
            
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 offset = angle.ToRotationVector2() * 40f;
                CustomParticles.GenericFlare(target.Center + offset, GetEnigmaGradient((float)i / 12f), 0.55f, 20);
            }
            
            for (int ring = 0; ring < 4; ring++)
            {
                CustomParticles.HaloRing(target.Center, GetEnigmaGradient(ring / 4f), 0.45f + ring * 0.18f, 16 + ring * 4);
            }
            
            // Radiant crown above target
            for (int i = 0; i < 6; i++)
            {
                float crownAngle = MathHelper.Pi + MathHelper.Pi * i / 6f - MathHelper.PiOver2;
                Vector2 crownPos = target.Center - new Vector2(0, 35f) + crownAngle.ToRotationVector2() * 25f;
                CustomParticles.GenericFlare(crownPos, GetEnigmaGradient((float)i / 6f), 0.52f, 18);
            }
            
            // === GLYPH CIRCLE FORMATION ===
            CustomParticles.GlyphCircle(target.Center, EnigmaPurple, count: 6, radius: 45f, rotationSpeed: 0.07f);
            
            int stacks = brandNPC.paradoxStacks;
            if (stacks >= 5)
            {
                // Trigger explosion at 5 stacks
                brandNPC.paradoxStacks = 0;
                CustomParticles.GlyphBurst(target.Center, EnigmaGreen, count: 12, speed: 6f);
                
                // Sparkle explosion burst
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 burstVel = angle.ToRotationVector2() * 5f;
                    var sparkle = new GenericGlowParticle(target.Center, burstVel, GetEnigmaGradient((float)i / 8f), 0.45f, 18, true);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
                ThemedParticles.EnigmaMusicNotes(target.Center, 6, 40f);
                
                // Damage nearby from explosion
                Player owner = Main.player[Projectile.owner];
                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (npc.friendly || npc.whoAmI == target.whoAmI) continue;
                    float dist = Vector2.Distance(npc.Center, target.Center);
                    if (dist <= 150f)
                    {
                        npc.SimpleStrikeNPC(Projectile.damage, owner.direction, true, 8f);
                        npc.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(npc, 2);
                    }
                }
            }
            else if (stacks > 0)
            {
                CustomParticles.GlyphStack(target.Center + new Vector2(0, -30f), EnigmaGreen, stacks, 0.32f);
            }
        }
        
        public override void OnKill(int timeLeft)
        {
            // === CHARGED THRUST DEATH REALITY WARP ===
            FateRealityDistortion.TriggerChromaticAberration(Projectile.Center, 5f, 18);
            FateRealityDistortion.TriggerInversionPulse(6);
            
            // End burst
            CustomParticles.GenericFlare(Projectile.Center, Color.White, 1.0f, 25);
            
            for (int i = 0; i < 15; i++)
            {
                float angle = MathHelper.TwoPi * i / 15f;
                Vector2 vel = angle.ToRotationVector2() * 6f;
                var glow = new GenericGlowParticle(Projectile.Center, vel, GetEnigmaGradient((float)i / 15f), 0.45f, 22, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            for (int ring = 0; ring < 5; ring++)
            {
                CustomParticles.HaloRing(Projectile.Center, GetEnigmaGradient(ring / 5f), 0.45f + ring * 0.2f, 18 + ring * 4);
            }
            
            // Sparkle constellation finale
            for (int layer = 0; layer < 2; layer++)
            {
                int points = 6;
                float radius = 40f + layer * 25f;
                for (int i = 0; i < points; i++)
                {
                    float angle = MathHelper.TwoPi * i / points + layer * 0.15f;
                    Vector2 constPos = Projectile.Center + angle.ToRotationVector2() * radius;
                    CustomParticles.GenericFlare(constPos, GetEnigmaGradient((float)i / points), 0.45f - layer * 0.08f, 18);
                }
            }
            ThemedParticles.EnigmaMusicNoteBurst(Projectile.Center, 8, 5f);
            CustomParticles.GlyphBurst(Projectile.Center, EnigmaPurple, count: 10, speed: 5f);
        }
    }
}
