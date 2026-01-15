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
using MagnumOpus.Content.Fate.Debuffs;

namespace MagnumOpus.Content.Fate.ResonantWeapons
{
    /// <summary>
    /// Singularity Scepter - Fires homing Singularity Fragments
    /// </summary>
    public class Fate9 : ModItem
    {
        // Dark Prismatic color palette
        private static readonly Color FateBlack = new Color(15, 5, 20);
        private static readonly Color FateDarkPink = new Color(180, 50, 100);
        private static readonly Color FateBrightRed = new Color(255, 60, 80);
        private static readonly Color FatePurple = new Color(120, 30, 140);
        private static readonly Color FateWhite = new Color(255, 255, 255);
        
        public override string Texture => "Terraria/Images/Item_" + ItemID.NebulaBlaze;
        
        public override void SetDefaults()
        {
            Item.damage = 520;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 14;
            Item.width = 36;
            Item.height = 36;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 23);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item117;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<SingularityFragment>();
            Item.shootSpeed = 10f;
            Item.noMelee = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Fires homing Singularity Fragments"));
            tooltips.Add(new TooltipLine(Mod, "FateEffect2", "Fragments multiply on hit, each spawning 2 more"));
            tooltips.Add(new TooltipLine(Mod, "FateLore", "'Each fragment carries the gravity of a dying star'") 
            { 
                OverrideColor = FateDarkPink 
            });
        }
        
        public override void HoldItem(Player player)
        {
            // === UNIQUE: SINGULARITY GRAVITY AURA ===
            // Particles are pulled toward the scepter like a black hole
            
            Vector2 scepterTip = player.Center + new Vector2(player.direction * 30f, -5f);
            
            // === GRAVITATIONAL PULL PARTICLES ===
            // Particles spiral inward toward scepter
            if (Main.rand.NextBool(5))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float startRadius = Main.rand.NextFloat(40f, 60f);
                Vector2 startPos = scepterTip + angle.ToRotationVector2() * startRadius;
                Vector2 pullVel = (scepterTip - startPos).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1.5f, 3f);
                
                Color pullColor = Color.Lerp(FatePurple, FateBrightRed, Main.rand.NextFloat());
                var pull = new GenericGlowParticle(startPos, pullVel, pullColor * 0.7f, 0.15f, 20, true);
                MagnumParticleHandler.SpawnParticle(pull);
            }
            
            // === VOID CORE GLOW ===
            // Central dark core with bright edge
            if (Main.rand.NextBool(6))
            {
                CustomParticles.GenericFlare(scepterTip, FateBlack, 0.3f, 8);
                CustomParticles.GenericFlare(scepterTip, FateBrightRed * 0.6f, 0.2f, 6);
            }
            
            // === ORBITING MICRO-SINGULARITIES ===
            // Tiny points orbit the scepter
            float orbitTime = Main.GameUpdateCount * 0.05f;
            for (int i = 0; i < 3; i++)
            {
                float orbitAngle = orbitTime + MathHelper.TwoPi * i / 3f;
                float orbitRadius = 18f;
                Vector2 orbitPos = scepterTip + orbitAngle.ToRotationVector2() * orbitRadius;
                
                if (Main.rand.NextBool(3))
                {
                    Color orbitColor = Color.Lerp(FateDarkPink, FateBrightRed, (float)i / 3f);
                    CustomParticles.GenericFlare(orbitPos, orbitColor, 0.12f, 5);
                }
            }
            
            // === MUSIC NOTES - Cosmic harmony ===
            if (Main.rand.NextBool(25))
            {
                ThemedParticles.FateMusicNotes(scepterTip, 1, 20f);
            }
            
            // Gravity well lighting
            Lighting.AddLight(scepterTip, FateDarkPink.ToVector3() * 0.35f);
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < 2; i++)
            {
                float spread = MathHelper.ToRadians(-8f + i * 16f);
                Vector2 spreadVel = velocity.RotatedBy(spread);
                Projectile.NewProjectile(source, position, spreadVel, type, damage, knockback, player.whoAmI, 0f);
            }
            
            // Dark prismatic cast effect with chromatic aberration
            CustomParticles.GenericFlare(position, FateBlack, 0.6f, 15);
            CustomParticles.GenericFlare(position, FateBrightRed, 0.5f, 12);
            CustomParticles.HaloRing(position, FateDarkPink * 0.7f, 0.35f, 12);
            
            // Chromatic cast flash
            CustomParticles.GenericFlare(position + new Vector2(-3, 0), Color.Red * 0.35f, 0.25f, 10);
            CustomParticles.GenericFlare(position + new Vector2(3, 0), Color.Cyan * 0.35f, 0.25f, 10);
            
            // Singularity casting glyph circle - enhanced
            CustomParticles.GlyphCircle(position, FateBrightRed, 7, 45f, 0.035f);
            CustomParticles.GlyphTower(position, FatePurple, 2, 0.35f);
            
            // Singularity fragments spawn with cosmic music notes!
            ThemedParticles.FateMusicNotes(position, 6, 35f);
            
            return false;
        }
    }
    
    public class SingularityFragment : ModProjectile
    {
        // Dark Prismatic color palette
        private static readonly Color FateBlack = new Color(15, 5, 20);
        private static readonly Color FateDarkPink = new Color(180, 50, 100);
        private static readonly Color FateBrightRed = new Color(255, 60, 80);
        private static readonly Color FatePurple = new Color(120, 30, 140);
        private static readonly Color FateWhite = new Color(255, 255, 255);
        
        private int Generation => (int)Projectile.ai[0];
        private const int MaxGeneration = 3;
        
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private Color GetFateGradient(float progress)
        {
            if (progress < 0.4f)
                return Color.Lerp(FateBlack, FateDarkPink, progress / 0.4f);
            else if (progress < 0.8f)
                return Color.Lerp(FateDarkPink, FateBrightRed, (progress - 0.4f) / 0.4f);
            else
                return Color.Lerp(FateBrightRed, FateWhite, (progress - 0.8f) / 0.2f);
        }
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = false;
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() / 2f;
            float intensity = 1f - Generation * 0.18f;
            
            // Draw chromatic aberration trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                float trailAlpha = (1f - progress) * 0.5f * intensity;
                float trailScale = (0.5f - progress * 0.3f) * intensity;
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                
                // RGB separation for chromatic effect
                spriteBatch.Draw(texture, drawPos + new Vector2(-2, 0), null, Color.Red * trailAlpha * 0.4f, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
                spriteBatch.Draw(texture, drawPos + new Vector2(2, 0), null, Color.Cyan * trailAlpha * 0.4f, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
                
                Color trailColor = GetFateGradient(progress);
                spriteBatch.Draw(texture, drawPos, null, trailColor * trailAlpha, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }
            
            // Draw layered glow core
            Vector2 corePos = Projectile.Center - Main.screenPosition;
            spriteBatch.Draw(texture, corePos, null, FateDarkPink * 0.4f * intensity, Projectile.rotation, origin, 0.7f * intensity, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, corePos, null, FateBrightRed * 0.5f * intensity, Projectile.rotation, origin, 0.5f * intensity, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, corePos, null, FateWhite * 0.7f * intensity, Projectile.rotation, origin, 0.3f * intensity, SpriteEffects.None, 0f);
            
            return false;
        }
        
        public override void AI()
        {
            Projectile.rotation += 0.18f;
            
            // Homing with enhanced range
            float homingRange = 450f - Generation * 70f;
            float homingSpeed = 9f + Generation * 2.5f;
            
            NPC target = null;
            float closestDist = homingRange;
            
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly || npc.dontTakeDamage) continue;
                
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    target = npc;
                }
            }
            
            if (target != null)
            {
                Vector2 targetDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetDir * homingSpeed, 0.09f);
            }
            
            // Dark prismatic trail effect based on generation
            float trailIntensity = 1f - Generation * 0.18f;
            
            if (Main.rand.NextBool(2))
            {
                float progress = Main.rand.NextFloat();
                Color trailColor = GetFateGradient(progress) * trailIntensity;
                var glow = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.12f, trailColor * 0.65f, 0.24f, 14, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Chromatic aberration trail
            if (Main.rand.NextBool(3))
            {
                CustomParticles.GenericFlare(Projectile.Center + new Vector2(-2, 0), Color.Red * 0.25f * trailIntensity, 0.1f, 6);
                CustomParticles.GenericFlare(Projectile.Center + new Vector2(2, 0), Color.Cyan * 0.25f * trailIntensity, 0.1f, 6);
            }
            
            // Orbiting particles with dark prismatic gradient
            if (Main.GameUpdateCount % 4 == 0)
            {
                float orbitAngle = Main.GameUpdateCount * 0.22f;
                for (int i = 0; i < 4; i++)
                {
                    float angle = orbitAngle + MathHelper.TwoPi * i / 4f;
                    float radius = 14f + (float)Math.Sin(Main.GameUpdateCount * 0.12f + i) * 5f;
                    Vector2 orbitPos = Projectile.Center + angle.ToRotationVector2() * radius;
                    Color orbitColor = GetFateGradient((float)i / 4f) * trailIntensity;
                    CustomParticles.GenericFlare(orbitPos, orbitColor * 0.55f, 0.14f, 8);
                }
            }
            
            // Central void glow - dark emphasis
            if (Main.GameUpdateCount % 5 == 0)
            {
                CustomParticles.GenericFlare(Projectile.Center, FateBlack, 0.3f * trailIntensity, 10);
            }
            
            // Destiny glyph trail on fragments
            if (Main.rand.NextBool(6 - Generation))
            {
                CustomParticles.GlyphTrail(Projectile.Center, Projectile.velocity, FateBrightRed * trailIntensity * 0.7f, 0.22f + Generation * 0.04f);
            }
            
            // Glyph aura on larger fragments
            if (Generation == 0 && Main.GameUpdateCount % 18 == 0)
            {
                CustomParticles.GlyphAura(Projectile.Center, FateDarkPink * 0.55f, 28f, 1);
            }
            
            Lighting.AddLight(Projectile.Center, FateDarkPink.ToVector3() * 0.35f * trailIntensity);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);
            target.GetGlobalNPC<DestinyCollapseNPC>().AddStack(target, 2);
            
            // === UNIFIED VFX HIT EFFECT - FATE THEME ===
            UnifiedVFX.Fate.HitEffect(target.Center, 1.1f);
            
            // Dark prismatic hit VFX
            CustomParticles.GenericFlare(target.Center, FateBlack, 0.5f, 14);
            CustomParticles.GenericFlare(target.Center, FateBrightRed * 0.8f, 0.45f, 13);
            CustomParticles.HaloRing(target.Center, FateDarkPink * 0.55f, 0.28f, 12);
            
            // Cosmic glyph impact on singularity hits
            CustomParticles.GlyphImpact(target.Center, FateBlack, FateBrightRed, 0.55f);
            
            // === CHROMATIC ABERRATION ===
            CustomParticles.GenericFlare(target.Center + new Vector2(-3, 0), FateBrightRed * 0.4f, 0.3f, 12);
            CustomParticles.GenericFlare(target.Center + new Vector2(3, 0), FatePurple * 0.4f, 0.3f, 12);
            
            // === GLYPH FORMATIONS ===
            CustomParticles.GlyphCircle(target.Center, FateDarkPink, 5, 40f, 0.08f);
            
            // Chromatic impact burst
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 offset = angle.ToRotationVector2() * 18f;
                Color hitColor = GetFateGradient((float)i / 5f);
                CustomParticles.GenericFlare(target.Center + offset, hitColor * 0.55f, 0.28f, 12);
                
                // Chromatic split
                if (i % 2 == 0)
                {
                    CustomParticles.GenericFlare(target.Center + offset + new Vector2(-2, 0), Color.Red * 0.3f, 0.12f, 8);
                    CustomParticles.GenericFlare(target.Center + offset + new Vector2(2, 0), Color.Cyan * 0.3f, 0.12f, 8);
                }
            }
            
            // === TEMPORAL ECHO AFTERIMAGES ===
            for (int echo = 0; echo < 4; echo++)
            {
                Vector2 echoPos = target.Center + new Vector2(0, -echo * 10f);
                float echoAlpha = 1f - echo * 0.2f;
                CustomParticles.GenericFlare(echoPos, GetFateGradient((float)echo / 4f) * echoAlpha * 0.5f, 0.4f, 15);
            }
            
            // === COSMIC MUSIC NOTES ===
            ThemedParticles.FateMusicNoteBurst(target.Center, 10, 6f);
            ThemedParticles.FateMusicNotes(target.Center, 4, 30f);
            
            // Cosmic Revisit - singularity echoes
            int revisitDamage = (int)(damageDone * 0.25f);
            target.GetGlobalNPC<DestinyCollapseNPC>().QueueCosmicRevisit(target, revisitDamage, 25, Projectile.Center, 0.8f);
            
            // Spawn child fragments if not max generation - enhanced
            if (Generation < MaxGeneration)
            {
                SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.25f + Generation * 0.22f, Volume = 0.55f }, Projectile.Center);
                
                int childCount = 3; // Increased from 2
                float childDamageMult = 0.65f;
                
                for (int i = 0; i < childCount; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 childVel = angle.ToRotationVector2() * 7f;
                    
                    int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, childVel,
                        ModContent.ProjectileType<SingularityFragment>(), (int)(Projectile.damage * childDamageMult), 
                        Projectile.knockBack * 0.7f, Projectile.owner, Generation + 1);
                    
                    // Enhanced child spawn VFX with chromatic effect
                    Color spawnColor = GetFateGradient((float)i / childCount);
                    CustomParticles.GenericFlare(target.Center + childVel.SafeNormalize(Vector2.Zero) * 12f, spawnColor * 0.7f, 0.35f, 12);
                    CustomParticles.GenericFlare(target.Center + childVel.SafeNormalize(Vector2.Zero) * 12f + new Vector2(-2, 0), Color.Red * 0.25f, 0.15f, 8);
                    CustomParticles.GenericFlare(target.Center + childVel.SafeNormalize(Vector2.Zero) * 12f + new Vector2(2, 0), Color.Cyan * 0.25f, 0.15f, 8);
                }
                
                // Singularity fragmentation spawns cosmic music notes!
                ThemedParticles.FateMusicNotes(target.Center, 5, 30f);
            }
            
            Lighting.AddLight(target.Center, FateBrightRed.ToVector3() * 0.9f);
        }
        
        public override void OnKill(int timeLeft)
        {
            float intensity = 1f - Generation * 0.18f;
            
            // Dark prismatic death burst
            CustomParticles.GenericFlare(Projectile.Center, FateBlack, 0.4f * intensity, 14);
            
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * 3.5f;
                Color burstColor = GetFateGradient((float)i / 8f);
                var glow = new GenericGlowParticle(Projectile.Center, vel, burstColor * intensity, 0.35f, 17, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            CustomParticles.HaloRing(Projectile.Center, FateDarkPink * intensity * 0.55f, 0.25f, 12);
            
            // Chromatic death flash
            CustomParticles.GenericFlare(Projectile.Center + new Vector2(-3, 0), Color.Red * 0.3f * intensity, 0.18f, 10);
            CustomParticles.GenericFlare(Projectile.Center + new Vector2(3, 0), Color.Cyan * 0.3f * intensity, 0.18f, 10);
        }
    }
}
