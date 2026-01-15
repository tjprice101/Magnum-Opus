using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;
using Terraria.GameContent;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Fate.Debuffs;

namespace MagnumOpus.Content.Fate.ResonantWeapons
{
    /// <summary>
    /// Destiny Cleaver - Ultimate melee sword
    /// Swings create "fate lines" with temporal echoes
    /// Every 5th hit: FATE SEVER reality slice
    /// DARK PRISMATIC: Black → Dark Pink → Bright Red
    /// </summary>
    public class Fate1 : ModItem
    {
        // DARK PRISMATIC COLOR PALETTE
        private static readonly Color FateBlack = new Color(15, 5, 20);
        private static readonly Color FateDarkPink = new Color(180, 50, 100);
        private static readonly Color FateBrightRed = new Color(255, 60, 80);
        private static readonly Color FatePurple = new Color(120, 30, 140);
        private static readonly Color FateWhite = new Color(255, 255, 255);
        
        private int strikeCounter = 0;
        private List<Vector2> recentHitPositions = new List<Vector2>();
        
        public override string Texture => "Terraria/Images/Item_" + ItemID.Meowmere;
        
        private Color GetFateGradient(float progress)
        {
            if (progress < 0.4f)
                return Color.Lerp(FateBlack, FateDarkPink, progress / 0.4f);
            else if (progress < 0.8f)
                return Color.Lerp(FateDarkPink, FateBrightRed, (progress - 0.4f) / 0.4f);
            else
                return Color.Lerp(FateBrightRed, FateWhite, (progress - 0.8f) / 0.2f);
        }
        
        public override void SetDefaults()
        {
            Item.damage = 680;
            Item.DamageType = DamageClass.Melee;
            Item.width = 70;
            Item.height = 70;
            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 7f;
            Item.value = Item.sellPrice(gold: 30);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<FateLineProjectile>();
            Item.shootSpeed = 12f;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Every 5th strike unleashes Fate Sever, a reality-cleaving slash"));
            tooltips.Add(new TooltipLine(Mod, "FateEffect2", "Swings create temporal echoes that damage enemies"));
            tooltips.Add(new TooltipLine(Mod, "FateLore", "'The blade that severs destiny itself'") 
            { 
                OverrideColor = FateBrightRed 
            });
        }
        
        public override void HoldItem(Player player)
        {
            // === PHASE 1: DARK PRISMATIC AMBIENT AURA ===
            if (Main.rand.NextBool(5))
            {
                float baseAngle = Main.GameUpdateCount * 0.04f;
                for (int i = 0; i < 5; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / 5f;
                    float radius = 35f + (float)Math.Sin(Main.GameUpdateCount * 0.07f + i * 0.7f) * 12f;
                    Vector2 flarePos = player.Center + angle.ToRotationVector2() * radius;
                    float progress = (float)i / 5f + Main.GameUpdateCount * 0.008f % 1f;
                    Color auraColor = GetFateGradient(progress) * 0.65f;
                    CustomParticles.GenericFlare(flarePos, auraColor, 0.32f, 18);
                    
                    // Chromatic aberration on ambient particles
                    if (i % 2 == 0)
                    {
                        CustomParticles.GenericFlare(flarePos + new Vector2(-2, 0), FateBrightRed * 0.25f, 0.18f, 12);
                        CustomParticles.GenericFlare(flarePos + new Vector2(2, 0), FatePurple * 0.25f, 0.18f, 12);
                    }
                }
            }
            
            // === PHASE 2: ORBITING GLYPHS - COSMIC DESTINY MARKERS ===
            if (Main.rand.NextBool(12))
            {
                float orbitAngle = Main.GameUpdateCount * 0.03f;
                Vector2 glyphPos = player.Center + orbitAngle.ToRotationVector2() * 45f;
                CustomParticles.Glyph(glyphPos, FateDarkPink * 0.55f, 0.28f, -1);
            }
            
            // === PHASE 3: TEMPORAL ECHO TRACES ===
            if (Main.rand.NextBool(20))
            {
                for (int echo = 0; echo < 3; echo++)
                {
                    Vector2 echoPos = player.Center + new Vector2(0, -echo * 12f);
                    CustomParticles.GenericFlare(echoPos, GetFateGradient((float)echo / 3f) * 0.3f, 0.22f, 10 + echo * 3);
                }
            }
            
            // === PHASE 4: AMBIENT COSMIC MUSIC NOTES ===
            if (Main.rand.NextBool(25))
            {
                ThemedParticles.FateMusicNotes(player.Center + Main.rand.NextVector2Circular(40f, 40f), 1, 20f);
            }
            
            // Pulsing dark prismatic lighting
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.12f + 0.88f;
            Lighting.AddLight(player.Center, FateDarkPink.ToVector3() * 0.4f * pulse);
        }
        
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // === HEAVY DARK PRISMATIC SWING TRAIL WITH CHROMATIC ABERRATION ===
            if (Main.rand.NextBool(2))
            {
                Vector2 pos = new Vector2(
                    Main.rand.Next(hitbox.Left, hitbox.Right),
                    Main.rand.Next(hitbox.Top, hitbox.Bottom));
                    
                float progress = Main.rand.NextFloat();
                Color trailColor = GetFateGradient(progress);
                CustomParticles.GenericFlare(pos, trailColor * 0.85f, 0.45f, 16);
                
                // === HEAVY CHROMATIC ABERRATION - REALITY BENDS ===
                CustomParticles.GenericFlare(pos + new Vector2(-4, -1), FateBrightRed * 0.45f, 0.28f, 12);
                CustomParticles.GenericFlare(pos + new Vector2(4, 1), FatePurple * 0.45f, 0.28f, 12);
                CustomParticles.GenericFlare(pos + new Vector2(0, -2), FateWhite * 0.3f, 0.2f, 10);
            }
            
            // === TEMPORAL ECHO PARTICLES - SHARP AFTERIMAGES ===
            if (Main.rand.NextBool(3))
            {
                Vector2 pos = new Vector2(
                    Main.rand.Next(hitbox.Left, hitbox.Right),
                    Main.rand.Next(hitbox.Top, hitbox.Bottom));
                    
                for (int echo = 0; echo < 3; echo++)
                {
                    Vector2 echoPos = pos + new Vector2(0, -echo * 8f);
                    float echoAlpha = 1f - echo * 0.25f;
                    CustomParticles.GenericFlare(echoPos, FateBlack * echoAlpha * 0.6f, 0.32f - echo * 0.06f, 15 + echo * 2);
                }
            }
            
            // === COSMIC MUSIC NOTES DANCE IN THE BLADE'S WAKE ===
            if (Main.rand.NextBool(3))
            {
                Vector2 notePos = hitbox.Center.ToVector2();
                ThemedParticles.FateMusicNotes(notePos, 2, 22f);
            }
            
            // === GLYPH ACCENTS ===
            if (Main.rand.NextBool(6))
            {
                Vector2 glyphPos = hitbox.Center.ToVector2() + Main.rand.NextVector2Circular(15f, 15f);
                CustomParticles.Glyph(glyphPos, FateDarkPink * 0.6f, 0.25f, -1);
            }
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Spawn fate line projectile on every swing
            Vector2 direction = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            Projectile.NewProjectile(source, player.Center, direction * Item.shootSpeed, type, damage / 2, knockback / 2, player.whoAmI);
            
            // VFX at swing origin
            CustomParticles.HaloRing(player.Center + direction * 40f, FateDarkPink * 0.7f, 0.35f, 12);
            CustomParticles.GlyphBurst(player.Center + direction * 30f, FatePurple, 4, 3f);
            
            return false;
        }
        
        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply DestinyCollapse debuff
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);
            target.GetGlobalNPC<DestinyCollapseNPC>().AddStack(target, 1);
            
            strikeCounter++;
            recentHitPositions.Add(target.Center);
            if (recentHitPositions.Count > 10) recentHitPositions.RemoveAt(0);
            
            // === USE THE SPECTACULAR NEW UNIFIED VFX ===
            UnifiedVFX.Fate.HitEffect(target.Center, 1.2f);
            
            // === ADDITIONAL CHROMATIC FRACTAL BURST - REALITY BREAKS ON IMPACT ===
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 offset = angle.ToRotationVector2() * 35f;
                float progress = (float)i / 10f;
                Color burstColor = GetFateGradient(progress);
                CustomParticles.GenericFlare(target.Center + offset, burstColor, 0.55f, 18);
                
                // === HEAVY RGB CHROMATIC ABERRATION ===
                CustomParticles.GenericFlare(target.Center + offset + new Vector2(-3, -1), FateBrightRed * 0.4f, 0.3f, 12);
                CustomParticles.GenericFlare(target.Center + offset + new Vector2(3, 1), FatePurple * 0.4f, 0.3f, 12);
            }
            
            // === TEMPORAL ECHO AFTERIMAGES ===
            for (int echo = 0; echo < 4; echo++)
            {
                Vector2 echoPos = target.Center + new Vector2(0, -echo * 10f);
                float echoAlpha = 1f - echo * 0.2f;
                Color echoColor = GetFateGradient((float)echo / 4f) * echoAlpha * 0.5f;
                CustomParticles.GenericFlare(echoPos, echoColor, 0.4f, 15);
            }
            
            // === DESTINY GLYPH MARKING ===
            CustomParticles.GlyphImpact(target.Center, FateBlack, FateBrightRed, 0.55f);
            CustomParticles.GlyphCircle(target.Center, FateDarkPink, 5, 40f, 0.08f);
            
            // === COSMIC MUSIC NOTES ON IMPACT ===
            ThemedParticles.FateMusicNoteBurst(target.Center, 10, 6f);
            ThemedParticles.FateMusicNotes(target.Center, 4, 30f);
            
            // === RADIAL SPARK BURST ===
            for (int spark = 0; spark < 12; spark++)
            {
                float angle = MathHelper.TwoPi * spark / 12f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                var sparkParticle = new GenericGlowParticle(target.Center, sparkVel, 
                    GetFateGradient((float)spark / 12f), 0.35f, 22, true);
                MagnumParticleHandler.SpawnParticle(sparkParticle);
            }
            
            // Visualize stacks with glyph tower
            int stacks = target.GetGlobalNPC<DestinyCollapseNPC>().GetStacks(target);
            if (stacks > 0)
            {
                CustomParticles.GlyphStack(target.Center + new Vector2(0, -30f), FateDarkPink, stacks, 0.28f);
            }
            
            // FATE SEVER every 5th hit!
            if (strikeCounter >= 5)
            {
                strikeCounter = 0;
                TriggerFateSever(player, target.Center);
                target.GetGlobalNPC<DestinyCollapseNPC>().AddStack(target, 3);
            }
            
            Lighting.AddLight(target.Center, FateBrightRed.ToVector3() * 0.9f);
        }
        
        private void TriggerFateSever(Player player, Vector2 position)
        {
            SoundEngine.PlaySound(SoundID.Item71 with { Pitch = -0.3f, Volume = 0.9f }, position);
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.5f, Volume = 0.7f }, position);
            
            // === USE THE NEW SPECTACULAR UNIFIED VFX EXPLOSION ===
            UnifiedVFX.Fate.Explosion(position, 1.6f);
            
            // === FATE-EXCLUSIVE SCREEN DISTORTION - REALITY BREAKS ===
            // This triggers chromatic aberration, screen slice, and inversion pulse
            Vector2 sliceDir = player.direction == 1 ? new Vector2(1, -0.2f) : new Vector2(-1, -0.2f);
            sliceDir.Normalize();
            FateRealityDistortion.TriggerFullRealityBreak(position, sliceDir, 1.2f);
            
            // Reality-slicing line effect across screen
            float sliceLength = 500f;
            
            // === PHASE 1: MAIN REALITY SLICE WITH HEAVY CHROMATIC ABERRATION ===
            for (int i = 0; i < 40; i++)
            {
                float t = i / 40f;
                Vector2 slicePos = position + sliceDir * (t * sliceLength - sliceLength * 0.5f);
                Color sliceColor = GetFateGradient(t);
                
                // Main slice glow
                CustomParticles.GenericFlare(slicePos, sliceColor, 0.8f - t * 0.25f, 24);
                
                // === HEAVY RGB CHROMATIC ABERRATION - REALITY IS BREAKING ===
                CustomParticles.GenericFlare(slicePos + new Vector2(-5, -2), FateBrightRed * 0.6f, 0.45f, 18);
                CustomParticles.GenericFlare(slicePos + new Vector2(5, 2), FatePurple * 0.6f, 0.45f, 18);
                CustomParticles.GenericFlare(slicePos + new Vector2(0, -3), FateWhite * 0.4f, 0.35f, 15);
                
                // Perpendicular fracture lines
                Vector2 perpendicular = new Vector2(-sliceDir.Y, sliceDir.X);
                if (i % 3 == 0)
                {
                    CustomParticles.GenericFlare(slicePos + perpendicular * 20f, FateDarkPink * 0.7f, 0.4f, 16);
                    CustomParticles.GenericFlare(slicePos - perpendicular * 20f, FateDarkPink * 0.7f, 0.4f, 16);
                }
                
                // Glyph markers along the slice
                if (i % 5 == 0)
                {
                    CustomParticles.Glyph(slicePos, GetFateGradient(t), 0.5f, -1);
                }
            }
            
            // === PHASE 2: TEMPORAL ECHO AFTERIMAGES ===
            for (int echo = 0; echo < 5; echo++)
            {
                float echoDelay = echo * 0.15f;
                float echoAlpha = 1f - echo * 0.15f;
                for (int i = 0; i < 20; i++)
                {
                    float t = i / 20f;
                    Vector2 echoPos = position + sliceDir * (t * sliceLength - sliceLength * 0.5f);
                    echoPos += new Vector2(0, -echo * 8f); // Temporal offset upward
                    Color echoColor = GetFateGradient((t + echo * 0.1f) % 1f) * echoAlpha * 0.4f;
                    CustomParticles.GenericFlare(echoPos, echoColor, 0.35f, 12 + echo * 2);
                }
            }
            
            // Spawn actual damage projectile
            Projectile.NewProjectile(player.GetSource_ItemUse(Item), position, sliceDir * 20f, 
                ModContent.ProjectileType<FateSeverSlash>(), Item.damage * 2, 10f, player.whoAmI);
            
            // === PHASE 3: DOUBLE SPIRAL GALAXY - COSMIC DESTINY UNFOLDS ===
            for (int arm = 0; arm < 8; arm++)
            {
                float armAngle = MathHelper.TwoPi * arm / 8f + Main.GameUpdateCount * 0.05f;
                for (int point = 0; point < 6; point++)
                {
                    float spiralAngle = armAngle + point * 0.4f;
                    float spiralRadius = 35f + point * 25f;
                    Vector2 spiralPos = position + spiralAngle.ToRotationVector2() * spiralRadius;
                    float gradientProgress = (arm * 6 + point) / 48f;
                    CustomParticles.GenericFlare(spiralPos, GetFateGradient(gradientProgress), 0.6f + point * 0.05f, 25);
                    
                    // Trailing temporal echoes
                    var sparkle = new GenericGlowParticle(spiralPos, -spiralAngle.ToRotationVector2() * 3f, 
                        GetFateGradient(gradientProgress) * 0.6f, 0.3f, 18, true);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }
            
            // === PHASE 4: MASSIVE LAYERED HALO CASCADE ===
            for (int ring = 0; ring < 8; ring++)
            {
                float ringProgress = ring / 8f;
                Color ringColor = GetFateGradient(ringProgress);
                CustomParticles.HaloRing(position, ringColor, 0.5f + ring * 0.2f, 22 + ring * 4);
            }
            
            // === PHASE 5: COSMIC GLYPH FORMATIONS ===
            CustomParticles.GlyphCircle(position, FateDarkPink, 12, 90f, 0.05f);
            CustomParticles.GlyphCircle(position, FateBrightRed, 8, 130f, -0.04f);
            CustomParticles.GlyphBurst(position, FateWhite, 16, 8f);
            CustomParticles.GlyphTower(position, FateDarkPink, 5, 0.5f);
            
            // === PHASE 6: SYMPHONY OF COSMIC MUSIC NOTES ===
            ThemedParticles.FateMusicNoteBurst(position, 20, 10f);
            for (int wave = 0; wave < 4; wave++)
            {
                float waveRadius = 50f + wave * 40f;
                for (int note = 0; note < 8; note++)
                {
                    float noteAngle = MathHelper.TwoPi * note / 8f + wave * 0.25f;
                    Vector2 notePos = position + noteAngle.ToRotationVector2() * waveRadius;
                    ThemedParticles.FateMusicNotes(notePos, 2, 18f);
                }
            }
            
            // Damage all enemies in a wide radius
            float severRadius = 320f;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.friendly && npc.CanBeChasedBy() && Vector2.Distance(npc.Center, position) <= severRadius)
                {
                    npc.SimpleStrikeNPC(Item.damage, player.direction, true, 8f);
                    npc.AddBuff(ModContent.BuffType<DestinyCollapse>(), 480);
                    npc.GetGlobalNPC<DestinyCollapseNPC>().AddStack(npc, 2);
                    
                    // === USE UNIFIED VFX HIT EFFECT ON EACH STRUCK ENEMY ===
                    UnifiedVFX.Fate.HitEffect(npc.Center, 0.9f);
                    
                    // Lightning arc to chained enemies
                    MagnumVFX.DrawFractalLightning(position, npc.Center, FateBrightRed, 10, 35f, 4, 0.5f);
                }
            }
            
            // Intense cosmic lighting
            Lighting.AddLight(position, FateBrightRed.ToVector3() * 2f);
        }
        
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.12f + 1f;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Dark prismatic glow layers
            spriteBatch.Draw(texture, position, null, FateBlack * 0.5f, rotation, origin, scale * pulse * 1.35f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, FateDarkPink * 0.35f, rotation, origin, scale * pulse * 1.2f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, FateBrightRed * 0.25f, rotation, origin, scale * pulse * 1.1f, SpriteEffects.None, 0f);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            spriteBatch.Draw(texture, position, null, lightColor, rotation, origin, scale, SpriteEffects.None, 0f);
            
            Lighting.AddLight(Item.Center, FateDarkPink.ToVector3() * 0.4f);
            
            return false;
        }
    }
    
    /// <summary>
    /// Fate Line Projectile - temporal echo slash
    /// </summary>
    public class FateLineProjectile : ModProjectile
    {
        private static readonly Color FateBlack = new Color(15, 5, 20);
        private static readonly Color FateDarkPink = new Color(180, 50, 100);
        private static readonly Color FateBrightRed = new Color(255, 60, 80);
        private static readonly Color FatePurple = new Color(120, 30, 140);
        private static readonly Color FateWhite = new Color(255, 255, 255);
        
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        private Color GetFateGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(FateBlack, FateDarkPink, progress * 2f);
            else
                return Color.Lerp(FateDarkPink, FateBrightRed, (progress - 0.5f) * 2f);
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 45;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.extraUpdates = 1;
        }
        
        public override void AI()
        {
            // Heavy gradient trail
            float lifeProgress = 1f - (Projectile.timeLeft / 45f);
            
            for (int i = 0; i < 3; i++)
            {
                float progress = (lifeProgress + i * 0.1f) % 1f;
                Color trailColor = GetFateGradient(progress) * (1f - lifeProgress * 0.5f);
                Vector2 offset = Main.rand.NextVector2Circular(8f, 8f);
                CustomParticles.GenericFlare(Projectile.Center + offset, trailColor, 0.4f - lifeProgress * 0.2f, 12);
            }
            
            // Chromatic aberration trail
            CustomParticles.GenericFlare(Projectile.Center + new Vector2(-3, 0), FateBrightRed * 0.3f, 0.2f, 8);
            CustomParticles.GenericFlare(Projectile.Center + new Vector2(3, 0), FateBlack * 0.4f, 0.2f, 8);
            
            Lighting.AddLight(Projectile.Center, FateDarkPink.ToVector3() * 0.4f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply debuff
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 180);
            target.GetGlobalNPC<DestinyCollapseNPC>().AddStack(target, 1);
            
            // Impact burst
            for (int i = 0; i < 5; i++)
            {
                float progress = (float)i / 5f;
                CustomParticles.GenericFlare(target.Center + Main.rand.NextVector2Circular(20f, 20f), GetFateGradient(progress), 0.4f, 14);
            }
            
            // Cosmic Revisit - delayed cosmic flare that strikes again
            int revisitDamage = (int)(damageDone * 0.25f);
            target.GetGlobalNPC<DestinyCollapseNPC>().QueueCosmicRevisit(target, revisitDamage, 25, Projectile.Center, 0.8f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = glowTex.Size() / 2f;
            
            // Draw chromatic aberration trail
            if (ProjectileID.Sets.TrailCacheLength[Projectile.type] > 0)
            {
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero) continue;
                    float trailProgress = (float)i / Projectile.oldPos.Length;
                    float trailAlpha = (1f - trailProgress) * 0.6f;
                    float trailScale = (1f - trailProgress * 0.4f) * 0.5f;
                    Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                    
                    spriteBatch.Draw(glowTex, trailPos + new Vector2(-3, 0), null, Color.Red * trailAlpha * 0.4f, 0f, origin, trailScale, SpriteEffects.None, 0f);
                    spriteBatch.Draw(glowTex, trailPos, null, FateDarkPink * trailAlpha * 0.5f, 0f, origin, trailScale, SpriteEffects.None, 0f);
                    spriteBatch.Draw(glowTex, trailPos + new Vector2(3, 0), null, Color.Cyan * trailAlpha * 0.4f, 0f, origin, trailScale, SpriteEffects.None, 0f);
                }
            }
            
            // Draw layered glow core
            spriteBatch.Draw(glowTex, drawPos, null, FateBlack * 0.9f, 0f, origin, 1.0f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, FateDarkPink * 0.7f, 0f, origin, 0.7f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, FateBrightRed * 0.8f, 0f, origin, 0.45f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, FateWhite * 0.9f, 0f, origin, 0.25f, SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    /// <summary>
    /// Fate Sever Slash - massive reality-cutting damage projectile
    /// </summary>
    public class FateSeverSlash : ModProjectile
    {
        private static readonly Color FateBlack = new Color(15, 5, 20);
        private static readonly Color FateDarkPink = new Color(180, 50, 100);
        private static readonly Color FateBrightRed = new Color(255, 60, 80);
        private static readonly Color FatePurple = new Color(120, 30, 140);
        private static readonly Color FateWhite = new Color(255, 255, 255);
        
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 100;
            Projectile.height = 100;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 30;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
        
        public override void AI()
        {
            float lifeProgress = 1f - (Projectile.timeLeft / 30f);
            
            // Expanding slash effect
            for (int i = 0; i < 5; i++)
            {
                float spread = lifeProgress * 50f;
                Vector2 perpendicular = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2);
                Vector2 pos = Projectile.Center + perpendicular * Main.rand.NextFloat(-spread, spread);
                
                float progress = Main.rand.NextFloat();
                Color slashColor;
                if (progress < 0.5f)
                    slashColor = Color.Lerp(FateBlack, FateDarkPink, progress * 2f);
                else
                    slashColor = Color.Lerp(FateDarkPink, FateBrightRed, (progress - 0.5f) * 2f);
                
                CustomParticles.GenericFlare(pos, slashColor * (1f - lifeProgress * 0.6f), 0.5f, 10);
            }
            
            Lighting.AddLight(Projectile.Center, FateBrightRed.ToVector3() * 0.6f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 360);
            target.GetGlobalNPC<DestinyCollapseNPC>().AddStack(target, 2);
            
            // Reality tear effect at impact
            CustomParticles.GlyphBurst(target.Center, FateDarkPink, 6, 4f);
            CustomParticles.HaloRing(target.Center, FateBrightRed, 0.5f, 15);
            
            // Cosmic Revisit - powerful charged attack revisit
            int revisitDamage = (int)(damageDone * 0.35f);
            target.GetGlobalNPC<DestinyCollapseNPC>().QueueCosmicRevisit(target, revisitDamage, 20, Projectile.Center, 1.0f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = glowTex.Size() / 2f;
            
            // Draw chromatic aberration trail
            if (ProjectileID.Sets.TrailCacheLength[Projectile.type] > 0)
            {
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero) continue;
                    float trailProgress = (float)i / Projectile.oldPos.Length;
                    float trailAlpha = (1f - trailProgress) * 0.6f;
                    float trailScale = (1f - trailProgress * 0.4f) * 0.8f;
                    Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                    
                    spriteBatch.Draw(glowTex, trailPos + new Vector2(-3, 0), null, Color.Red * trailAlpha * 0.4f, 0f, origin, trailScale, SpriteEffects.None, 0f);
                    spriteBatch.Draw(glowTex, trailPos, null, FateDarkPink * trailAlpha * 0.5f, 0f, origin, trailScale, SpriteEffects.None, 0f);
                    spriteBatch.Draw(glowTex, trailPos + new Vector2(3, 0), null, Color.Cyan * trailAlpha * 0.4f, 0f, origin, trailScale, SpriteEffects.None, 0f);
                }
            }
            
            // Draw layered glow core - larger for slash effect
            spriteBatch.Draw(glowTex, drawPos, null, FateBlack * 0.9f, 0f, origin, 1.5f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, FateDarkPink * 0.7f, 0f, origin, 1.0f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, FateBrightRed * 0.8f, 0f, origin, 0.6f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, FateWhite * 0.9f, 0f, origin, 0.3f, SpriteEffects.None, 0f);
            
            return false;
        }
    }
}
