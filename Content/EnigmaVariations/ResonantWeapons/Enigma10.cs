using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    /// The Watching Obelisk - Stationary sentry covered in eyes that tracks all enemies
    /// Eyes draw visible gaze lines to targets and periodically fire paradox beams
    /// Creates expanding "awareness zones" that debuff enemies
    /// </summary>
    public class Enigma10 : ModItem
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override string Texture => "Terraria/Images/Item_" + ItemID.StaffoftheFrostHydra;
        
        public override void SetDefaults()
        {
            Item.damage = 145;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 20;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 18);
            Item.rare = ModContent.RarityType<EnigmaRarity>();
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<WatchingObelisk>();
            Item.sentry = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect", "Places a stationary Watching Obelisk covered in eyes"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect2", "Arcane tendrils track ALL enemies on screen with mystical connections"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect3", "Periodically fires paradox beams at watched targets"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect4", "Creates expanding awareness zones that slow and debuff"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaLore", "'The unknown weaves its mysteries through all who dare approach.'") 
            { 
                OverrideColor = EnigmaPurple 
            });
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Place at cursor position
            Vector2 spawnPos = Main.MouseWorld;
            
            player.UpdateMaxTurrets();
            
            Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback, player.whoAmI);
            
            // Massive placement VFX
            CustomParticles.GenericFlare(spawnPos, EnigmaGreen, 1f, 25);
            CustomParticles.HaloRing(spawnPos, EnigmaPurple, 0.7f, 20);
            
            // Prismatic sparkles spiral at placement
            for (int sparkle = 0; sparkle < 8; sparkle++)
            {
                float sparkleAngle = MathHelper.TwoPi * sparkle / 8f;
                Vector2 sparklePos = spawnPos + sparkleAngle.ToRotationVector2() * 60f;
                Color sparkleColor = GetEnigmaGradient((float)sparkle / 8f);
                CustomParticles.GenericFlare(sparklePos, sparkleColor, 0.55f, 20);
                var sparkleGlow = new GenericGlowParticle(sparklePos, sparkleAngle.ToRotationVector2() * 2f, sparkleColor, 0.4f, 25, true);
                MagnumParticleHandler.SpawnParticle(sparkleGlow);
            }
            
            // Glyph magic circle at base
            CustomParticles.GlyphCircle(spawnPos, EnigmaGreen, count: 8, radius: 50f, rotationSpeed: 0.04f);
            CustomParticles.GlyphTower(spawnPos, EnigmaPurple, layers: 4, baseScale: 0.5f);
            
            // Fractal burst
            for (int layer = 0; layer < 3; layer++)
            {
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f + layer * 0.15f;
                    float radius = 35f + layer * 20f;
                    Vector2 offset = angle.ToRotationVector2() * radius;
                    Color burstColor = GetEnigmaGradient((float)(layer * 8 + i) / 24f);
                    CustomParticles.GenericFlare(spawnPos + offset, burstColor, 0.5f - layer * 0.1f, 18);
                }
            }
            
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.2f, Volume = 0.8f }, spawnPos);
            
            // Music notes for grand obelisk summoning - the watcher's theme
            ThemedParticles.EnigmaMusicNoteBurst(spawnPos, 10, 5f);
            ThemedParticles.EnigmaMusicNotes(spawnPos, 8, 55f);
            
            return false;
        }
    }
    
    public class WatchingObelisk : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreenFlame = new Color(50, 220, 100);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private const float VisionRange = 600f;
        private const float AwarenessZoneRadius = 200f;
        private int attackCooldown = 0;
        private int awarenessZoneCooldown = 0;
        private List<NPC> watchedTargets = new List<NPC>();
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = glowTex.Size() / 2f;
            
            float pulse = 0.85f + (float)Math.Sin(Main.GameUpdateCount * 0.07f) * 0.15f;
            float watchIntensity = watchedTargets.Count > 0 ? 1.2f : 1f;
            
            // Draw pulsing obelisk glow - stationary watcher
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaBlack * 0.95f, 0f, origin, 1.4f * pulse * watchIntensity, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaDeepPurple * 0.7f, 0f, origin, 1f * pulse * watchIntensity, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaPurple * 0.8f, 0f, origin, 0.65f * pulse * watchIntensity, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaGreenFlame * 0.65f, 0f, origin, 0.35f * pulse * watchIntensity, SpriteEffects.None, 0f);
            
            return false;
        }
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Projectile.SentryLifeTime;
            Projectile.sentry = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }
        
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            attackCooldown--;
            awarenessZoneCooldown--;
            
            // Update watched targets - track ALL enemies in range
            watchedTargets.Clear();
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.friendly && Vector2.Distance(npc.Center, Projectile.Center) <= VisionRange)
                {
                    watchedTargets.Add(npc);
                }
            }
            
            // Draw gaze lines to all watched targets
            if (Main.GameUpdateCount % 3 == 0)
            {
                foreach (NPC target in watchedTargets)
                {
                    DrawGazeLine(target);
                }
            }
            
            // Attack the closest watched target
            if (attackCooldown <= 0 && watchedTargets.Count > 0)
            {
                attackCooldown = 45;
                
                // Sort by distance and attack closest
                watchedTargets.Sort((a, b) => 
                    Vector2.Distance(a.Center, Projectile.Center).CompareTo(
                    Vector2.Distance(b.Center, Projectile.Center)));
                
                FireParadoxBeam(watchedTargets[0]);
            }
            
            // Create awareness zone periodically
            if (awarenessZoneCooldown <= 0)
            {
                awarenessZoneCooldown = 180;
                CreateAwarenessZone();
            }
            
            // Ambient obelisk VFX
            if (Main.GameUpdateCount % 4 == 0)
            {
                // Swirling particles around the obelisk
                float angle = Main.GameUpdateCount * 0.1f;
                for (int i = 0; i < 3; i++)
                {
                    float particleAngle = angle + MathHelper.TwoPi * i / 3f;
                    float radius = 25f + (float)Math.Sin(Main.GameUpdateCount * 0.05f + i) * 10f;
                    Vector2 particlePos = Projectile.Center + particleAngle.ToRotationVector2() * radius;
                    Color particleColor = GetEnigmaGradient((float)i / 3f);
                    CustomParticles.GenericFlare(particlePos, particleColor * 0.5f, 0.25f, 12);
                }
            }
            
            // Central pulsing eye
            if (Main.GameUpdateCount % 15 == 0)
            {
                float pulse = 0.4f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.2f;
                CustomParticles.GenericFlare(Projectile.Center, EnigmaGreen, pulse, 15);
            }
            
            // Orbiting arcane sparkles around the obelisk
            if (Main.GameUpdateCount % 30 == 0)
            {
                for (int orb = 0; orb < 4; orb++)
                {
                    float orbitAngle = Main.GameUpdateCount * 0.03f + MathHelper.TwoPi * orb / 4f;
                    Vector2 orbitPos = Projectile.Center + orbitAngle.ToRotationVector2() * 40f;
                    Color orbitColor = GetEnigmaGradient((float)orb / 4f);
                    CustomParticles.GenericFlare(orbitPos, orbitColor, 0.4f, 18);
                    var orbitGlow = new GenericGlowParticle(orbitPos, (orbitAngle + MathHelper.PiOver2).ToRotationVector2() * 1.5f, orbitColor * 0.7f, 0.25f, 15, true);
                    MagnumParticleHandler.SpawnParticle(orbitGlow);
                }
            }
            
            // Glyphs orbiting at base
            if (Main.GameUpdateCount % 20 == 0)
            {
                CustomParticles.GlyphAura(Projectile.Center + new Vector2(0, 20f), EnigmaPurple, radius: 35f, count: 2);
            }
            
            // Number of enemies being watched visualization
            if (watchedTargets.Count > 0 && Main.GameUpdateCount % 40 == 0)
            {
                CustomParticles.GlyphStack(Projectile.Center - new Vector2(0, 40f), EnigmaGreen, 
                    Math.Min(watchedTargets.Count, 10), 0.2f);
            }
            
            Lighting.AddLight(Projectile.Center, GetEnigmaGradient(0.5f).ToVector3() * 0.6f);
        }
        
        private void DrawGazeLine(NPC target)
        {
            Vector2 start = Projectile.Center;
            Vector2 end = target.Center;
            float dist = Vector2.Distance(start, end);
            int segments = (int)(dist / 25f);
            
            for (int i = 0; i < segments; i++)
            {
                float t = (float)i / segments;
                Vector2 linePos = Vector2.Lerp(start, end, t);
                Color lineColor = GetEnigmaGradient(t) * (0.3f + (1f - t) * 0.3f);
                
                // Pulsing visibility based on time
                float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f + t * 5f) * 0.5f + 0.5f;
                CustomParticles.GenericFlare(linePos, lineColor * pulse, 0.12f, 6);
            }
            
            // Mystical sparkle at target connection point
            if (Main.GameUpdateCount % 15 == 0)
            {
                Vector2 sparkOffset = (start - end).SafeNormalize(Vector2.Zero) * 30f;
                CustomParticles.GenericFlare(end + sparkOffset, EnigmaPurple * 0.8f, 0.45f, 18);
                CustomParticles.HaloRing(end + sparkOffset, EnigmaGreen * 0.5f, 0.25f, 12);
            }
        }
        
        private void FireParadoxBeam(NPC target)
        {
            SoundEngine.PlaySound(SoundID.Item157 with { Pitch = 0.2f, Volume = 0.6f }, Projectile.Center);
            
            // Fire beam projectile
            Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, direction * 20f,
                ModContent.ProjectileType<ObeliskParadoxBeam>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            
            // Fire VFX
            CustomParticles.GenericFlare(Projectile.Center, EnigmaGreen, 0.7f, 15);
            CustomParticles.HaloRing(Projectile.Center, EnigmaPurple, 0.4f, 12);
            
            // Arcane burst at fire - mystical energy unleashed
            for (int burst = 0; burst < 5; burst++)
            {
                float burstAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 burstVel = burstAngle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color burstColor = GetEnigmaGradient(Main.rand.NextFloat());
                var burstGlow = new GenericGlowParticle(Projectile.Center, burstVel, burstColor, 0.35f, 20, true);
                MagnumParticleHandler.SpawnParticle(burstGlow);
            }
            
            // Glyph at fire
            CustomParticles.Glyph(Projectile.Center, EnigmaPurple, 0.35f, -1);
        }
        
        private void CreateAwarenessZone()
        {
            SoundEngine.PlaySound(SoundID.Item8 with { Pitch = -0.3f, Volume = 0.5f }, Projectile.Center);
            
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                ModContent.ProjectileType<AwarenessZone>(), Projectile.damage / 3, 0f, Projectile.owner);
            
            // Zone creation VFX
            CustomParticles.HaloRing(Projectile.Center, EnigmaGreen, 0.6f, 18);
            
            // Prismatic sparkles burst outward as awareness expands
            for (int spark = 0; spark < 8; spark++)
            {
                float sparkAngle = MathHelper.TwoPi * spark / 8f;
                Vector2 sparkVel = sparkAngle.ToRotationVector2() * 3f;
                Color sparkColor = GetEnigmaGradient((float)spark / 8f);
                var sparkGlow = new GenericGlowParticle(Projectile.Center, sparkVel, sparkColor, 0.4f, 22, true);
                MagnumParticleHandler.SpawnParticle(sparkGlow);
            }
            
            // Glyph circle at zone creation
            CustomParticles.GlyphCircle(Projectile.Center, EnigmaGreen, count: 8, radius: 45f, rotationSpeed: 0.06f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // Death explosion
            for (int layer = 0; layer < 3; layer++)
            {
                for (int i = 0; i < 10; i++)
                {
                    float angle = MathHelper.TwoPi * i / 10f + layer * 0.2f;
                    float radius = 30f + layer * 20f;
                    Vector2 vel = angle.ToRotationVector2() * (4f + layer * 2f);
                    Color burstColor = GetEnigmaGradient((float)(layer * 10 + i) / 30f);
                    var glow = new GenericGlowParticle(Projectile.Center, vel, burstColor, 0.5f, 22, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
            }
            
            // Multiple halo rings
            for (int ring = 0; ring < 4; ring++)
            {
                Color ringColor = GetEnigmaGradient(ring / 4f);
                CustomParticles.HaloRing(Projectile.Center, ringColor, 0.6f - ring * 0.1f, 18 + ring * 3);
            }
            
            // Dazzling sparkle cascade on death
            for (int cascade = 0; cascade < 12; cascade++)
            {
                float cascadeAngle = MathHelper.TwoPi * cascade / 12f;
                Vector2 cascadeVel = cascadeAngle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                Color cascadeColor = GetEnigmaGradient((float)cascade / 12f);
                var cascadeGlow = new GenericGlowParticle(Projectile.Center, cascadeVel, cascadeColor, 0.5f, 25, true);
                MagnumParticleHandler.SpawnParticle(cascadeGlow);
            }
            CustomParticles.GenericFlare(Projectile.Center, EnigmaGreen, 0.8f, 22);
            
            // Glyphs burst
            CustomParticles.GlyphBurst(Projectile.Center, EnigmaPurple, count: 10, speed: 4f);
            
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 0.7f }, Projectile.Center);
        }
    }
    
    public class ObeliskParadoxBeam : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreenFlame = new Color(50, 220, 100);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = glowTex.Size() / 2f;
            
            // Draw eerie trail
            if (ProjectileID.Sets.TrailCacheLength[Projectile.type] > 0)
            {
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero) continue;
                    float trailProgress = (float)i / Projectile.oldPos.Length;
                    float trailAlpha = (1f - trailProgress) * 0.5f;
                    float trailScale = (1f - trailProgress * 0.4f) * 0.3f;
                    Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                    
                    Color trailColor = Color.Lerp(EnigmaGreenFlame, EnigmaPurple, trailProgress);
                    spriteBatch.Draw(glowTex, trailPos, null, trailColor * trailAlpha, 0f, origin, trailScale, SpriteEffects.None, 0f);
                }
            }
            
            // Draw layered glow core - laser beam
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaBlack * 0.9f, 0f, origin, 0.55f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaDeepPurple * 0.7f, 0f, origin, 0.38f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaPurple * 0.8f, 0f, origin, 0.22f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaGreenFlame * 0.6f, 0f, origin, 0.1f, SpriteEffects.None, 0f);
            
            return false;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 3;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Trail
            if (Main.rand.NextBool(2))
            {
                float progress = (90 - Projectile.timeLeft) / 90f;
                Color trailColor = GetEnigmaGradient(progress);
                var glow = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.05f, 
                    trailColor * 0.6f, 0.2f, 12, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            Lighting.AddLight(Projectile.Center, EnigmaGreen.ToVector3() * 0.3f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 360);
            var brandNPC = target.GetGlobalNPC<ParadoxBrandNPC>();
            brandNPC.AddParadoxStack(target, 2);
            
            // === NEW UNIFIED VFX HIT EFFECT ===
            UnifiedVFX.EnigmaVariations.HitEffect(target.Center, 1.2f);
            
            // === WATCHING EYE AT IMPACT ===
            CustomParticles.EnigmaEyeImpact(target.Center, target.Center, EnigmaGreen, 0.5f);
            
            // === MUSIC NOTES BURST ===
            ThemedParticles.EnigmaMusicNoteBurst(target.Center, 10, 6f);
            ThemedParticles.EnigmaMusicNotes(target.Center, 5, 35f);
            
            // Impact VFX
            CustomParticles.GenericFlare(target.Center, EnigmaGreen, 0.6f, 15);
            CustomParticles.HaloRing(target.Center, EnigmaPurple, 0.35f, 12);
            
            // Sparkle burst above struck target
            CustomParticles.GenericFlare(target.Center - new Vector2(0, 25f), EnigmaPurple, 0.55f, 18);
            CustomParticles.HaloRing(target.Center - new Vector2(0, 25f), EnigmaGreen * 0.7f, 0.3f, 14);
            for (int hitIdx = 0; hitIdx < 4; hitIdx++)
            {
                float hitAngle = MathHelper.TwoPi * hitIdx / 4f;
                Vector2 hitVel = hitAngle.ToRotationVector2() * 2.5f;
                var hitSparkle = new GenericGlowParticle(target.Center - new Vector2(0, 25f), hitVel, GetEnigmaGradient((float)hitIdx / 4f), 0.28f, 16, true);
                MagnumParticleHandler.SpawnParticle(hitSparkle);
            }
            
            // === GLYPH CIRCLE FORMATION ===
            CustomParticles.GlyphCircle(target.Center, EnigmaPurple, count: 6, radius: 45f, rotationSpeed: 0.06f);
            
            // Glyph stack visualization
            int stacks = brandNPC.paradoxStacks;
            CustomParticles.GlyphStack(target.Center + new Vector2(0, -20f), EnigmaGreen, stacks, 0.2f);
            
            // === DYNAMIC LIGHTING ===
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.8f);
        }
        
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * 2.5f;
                Color burstColor = GetEnigmaGradient((float)i / 6f);
                var glow = new GenericGlowParticle(Projectile.Center, vel, burstColor, 0.3f, 15, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }
    }
    
    public class AwarenessZone : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreenFlame = new Color(50, 220, 100);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private const float MaxRadius = 250f;
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = glowTex.Size() / 2f;
            
            float lifeProgress = 1f - (Projectile.timeLeft / 150f);
            float intensity = (float)Math.Sin(lifeProgress * MathHelper.Pi);
            float currentRadius = MaxRadius * Math.Min(lifeProgress * 2f, 1f);
            float scale = currentRadius / 60f;
            
            // Draw awareness zone - expanding field of perception
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaBlack * intensity * 0.35f, 0f, origin, scale * 1.1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaDeepPurple * 0.25f * intensity, 0f, origin, scale * 0.8f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaPurple * 0.4f * intensity, 0f, origin, scale * 0.5f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaGreenFlame * 0.35f * intensity, 0f, origin, scale * 0.25f, SpriteEffects.None, 0f);
            
            return false;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 150;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 45;
        }
        
        public override void AI()
        {
            float lifeProgress = 1f - (Projectile.timeLeft / 150f);
            float intensity = (float)Math.Sin(lifeProgress * MathHelper.Pi);
            float currentRadius = MaxRadius * Math.Min(lifeProgress * 2f, 1f);
            
            // Expanding awareness ring
            if (Main.GameUpdateCount % 2 == 0)
            {
                int points = 16;
                for (int i = 0; i < points; i++)
                {
                    float angle = MathHelper.TwoPi * i / points + Main.GameUpdateCount * 0.03f;
                    Vector2 particlePos = Projectile.Center + angle.ToRotationVector2() * currentRadius;
                    Color particleColor = GetEnigmaGradient((float)i / points) * intensity * 0.5f;
                    CustomParticles.GenericFlare(particlePos, particleColor, 0.2f, 10);
                }
            }
            
            // Inner radial lines
            if (Main.GameUpdateCount % 4 == 0)
            {
                int lines = 8;
                for (int i = 0; i < lines; i++)
                {
                    float angle = MathHelper.TwoPi * i / lines + Main.GameUpdateCount * 0.02f;
                    int segments = 5;
                    for (int s = 0; s < segments; s++)
                    {
                        float t = (float)s / segments;
                        Vector2 linePos = Projectile.Center + angle.ToRotationVector2() * (currentRadius * t);
                        Color lineColor = GetEnigmaGradient(t) * intensity * 0.3f;
                        CustomParticles.GenericFlare(linePos, lineColor, 0.12f, 8);
                    }
                }
            }
            
            // Sparkles at the zone edge
            if (Main.GameUpdateCount % 25 == 0)
            {
                float edgeAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 edgePos = Projectile.Center + edgeAngle.ToRotationVector2() * currentRadius * 0.8f;
                CustomParticles.GenericFlare(edgePos, EnigmaPurple * intensity, 0.45f, 16);
                CustomParticles.HaloRing(edgePos, EnigmaGreen * intensity * 0.6f, 0.25f, 12);
            }
            
            // Glyphs in the zone
            if (Main.GameUpdateCount % 15 == 0)
            {
                CustomParticles.GlyphAura(Projectile.Center, EnigmaGreen * intensity, radius: currentRadius * 0.5f, count: 2);
            }
            
            // Apply effects to enemies in zone
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly) continue;
                
                float dist = Vector2.Distance(npc.Center, Projectile.Center);
                if (dist <= currentRadius)
                {
                    // Slow enemies
                    npc.velocity *= 0.96f;
                    
                    // Periodic debuff
                    if (Main.GameUpdateCount % 60 == 0)
                    {
                        npc.AddBuff(ModContent.BuffType<ParadoxBrand>(), 300);
                        var brandNPC = npc.GetGlobalNPC<ParadoxBrandNPC>();
                        brandNPC.AddParadoxStack(npc, 1);
                        
                        // Sparkle above debuffed target
                        CustomParticles.GenericFlare(npc.Center - new Vector2(0, 20f), EnigmaPurple * intensity, 0.4f, 14);
                        CustomParticles.HaloRing(npc.Center - new Vector2(0, 20f), EnigmaGreen * intensity * 0.6f, 0.22f, 10);
                    }
                }
            }
            
            Lighting.AddLight(Projectile.Center, GetEnigmaGradient(0.5f).ToVector3() * intensity * 0.4f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // Collapse VFX
            for (int ring = 0; ring < 4; ring++)
            {
                Color ringColor = GetEnigmaGradient(ring / 4f);
                CustomParticles.HaloRing(Projectile.Center, ringColor, 0.5f - ring * 0.08f, 15 + ring * 3);
            }
            
            // Sparkle explosion burst
            for (int collapseIdx = 0; collapseIdx < 4; collapseIdx++)
            {
                float collapseAngle = MathHelper.TwoPi * collapseIdx / 4f;
                Vector2 collapseVel = collapseAngle.ToRotationVector2() * 3f;
                var collapseSparkle = new GenericGlowParticle(Projectile.Center, collapseVel, GetEnigmaGradient((float)collapseIdx / 4f), 0.38f, 18, true);
                MagnumParticleHandler.SpawnParticle(collapseSparkle);
            }
            CustomParticles.GlyphBurst(Projectile.Center, EnigmaPurple, count: 6, speed: 3f);
        }
    }
}
