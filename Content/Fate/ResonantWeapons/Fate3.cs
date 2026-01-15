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
    /// Cosmic Decree - Ultimate magic staff
    /// Marks an area with destiny, then unleashes judgment
    /// DARK PRISMATIC: Black → Dark Pink → Bright Red
    /// </summary>
    public class Fate3 : ModItem
    {
        // DARK PRISMATIC COLOR PALETTE
        private static readonly Color FateBlack = new Color(15, 5, 20);
        private static readonly Color FateDarkPink = new Color(180, 50, 100);
        private static readonly Color FateBrightRed = new Color(255, 60, 80);
        private static readonly Color FatePurple = new Color(120, 30, 140);
        private static readonly Color FateWhite = new Color(255, 255, 255);
        
        public override string Texture => "Terraria/Images/Item_" + ItemID.LastPrism;
        
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
            Item.staff[Type] = true;
        }
        
        public override void SetDefaults()
        {
            Item.damage = 620;
            Item.DamageType = DamageClass.Magic;
            Item.width = 50;
            Item.height = 50;
            Item.useTime = 45;
            Item.useAnimation = 45;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(gold: 30);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item117;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<CosmicDecreeMarker>();
            Item.shootSpeed = 0f;
            Item.mana = 25;
            Item.noMelee = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Mark an area with cosmic decree"));
            tooltips.Add(new TooltipLine(Mod, "FateEffect2", "After a delay, the marked area experiences Reality Judgment"));
            tooltips.Add(new TooltipLine(Mod, "FateEffect3", "Enemies cannot escape their fate"));
            tooltips.Add(new TooltipLine(Mod, "FateLore", "'By cosmic law, your doom is sealed'") 
            { 
                OverrideColor = FateBrightRed 
            });
        }
        
        public override void HoldItem(Player player)
        {
            // Dark prismatic ambient aura
            if (Main.rand.NextBool(5))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float radius = Main.rand.NextFloat(30f, 60f);
                Vector2 flarePos = player.Center + angle.ToRotationVector2() * radius;
                float progress = Main.rand.NextFloat();
                Color auraColor = GetFateGradient(progress) * 0.5f;
                CustomParticles.GenericFlare(flarePos, auraColor, 0.3f, 16);
            }
            
            // Orbiting destiny glyphs
            if (Main.rand.NextBool(10))
            {
                float orbitAngle = Main.GameUpdateCount * 0.025f;
                for (int i = 0; i < 3; i++)
                {
                    float individualAngle = orbitAngle + MathHelper.TwoPi * i / 3f;
                    Vector2 glyphPos = player.Center + individualAngle.ToRotationVector2() * 45f;
                    CustomParticles.Glyph(glyphPos, FateDarkPink * 0.4f, 0.22f, -1);
                }
            }
            
            Lighting.AddLight(player.Center, FateDarkPink.ToVector3() * 0.35f);
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Place decree marker at mouse position
            Vector2 targetPos = Main.MouseWorld;
            
            Projectile.NewProjectile(source, targetPos, Vector2.Zero, type, damage, knockback, player.whoAmI);
            
            // Casting effect at player
            Vector2 castPos = player.Center;
            
            // Glyph circle at cast origin
            CustomParticles.GlyphCircle(castPos, FatePurple, 6, 35f, 0.04f);
            
            // Dark prismatic burst
            for (int i = 0; i < 8; i++)
            {
                float progress = (float)i / 8f;
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 offset = angle.ToRotationVector2() * 25f;
                CustomParticles.GenericFlare(castPos + offset, GetFateGradient(progress), 0.45f, 14);
            }
            
            // Beam connection from player to target
            Vector2 toTarget = (targetPos - castPos).SafeNormalize(Vector2.Zero);
            float distance = Vector2.Distance(castPos, targetPos);
            int beamPoints = (int)(distance / 30f);
            
            for (int i = 0; i < beamPoints; i++)
            {
                float t = (float)i / beamPoints;
                Vector2 beamPos = Vector2.Lerp(castPos, targetPos, t);
                Color beamColor = GetFateGradient(t) * 0.6f;
                CustomParticles.GenericFlare(beamPos, beamColor, 0.25f, 10);
            }
            
            // Cosmic Decree cast with cosmic music notes!
            ThemedParticles.FateMusicNotes(castPos, 8, 40f);
            ThemedParticles.FateMusicNotes(targetPos, 6, 35f);
            
            Lighting.AddLight(castPos, FateBrightRed.ToVector3() * 0.6f);
            
            return false;
        }
        
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.12f + 1f;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Dark prismatic glow layers
            spriteBatch.Draw(texture, position, null, FateBlack * 0.45f, rotation, origin, scale * pulse * 1.3f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, FateDarkPink * 0.35f, rotation, origin, scale * pulse * 1.18f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, FateBrightRed * 0.25f, rotation, origin, scale * pulse * 1.08f, SpriteEffects.None, 0f);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            spriteBatch.Draw(texture, position, null, lightColor, rotation, origin, scale, SpriteEffects.None, 0f);
            
            Lighting.AddLight(Item.Center, FateDarkPink.ToVector3() * 0.35f);
            
            return false;
        }
    }
    
    /// <summary>
    /// Cosmic Decree Marker - marks area, then explodes after delay
    /// </summary>
    public class CosmicDecreeMarker : ModProjectile
    {
        private static readonly Color FateBlack = new Color(15, 5, 20);
        private static readonly Color FateDarkPink = new Color(180, 50, 100);
        private static readonly Color FateBrightRed = new Color(255, 60, 80);
        private static readonly Color FatePurple = new Color(120, 30, 140);
        private static readonly Color FateWhite = new Color(255, 255, 255);
        
        private const int ChargeDuration = 90; // 1.5 seconds before judgment
        private const float JudgmentRadius = 200f;
        
        private List<int> markedEnemies = new List<int>();
        
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = glowTex.Size() / 2f;
            
            int timer = ChargeDuration + 30 - Projectile.timeLeft;
            float chargeProgress = Math.Min(1f, (float)timer / ChargeDuration);
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.15f + 1f;
            
            // Draw expanding layered glow based on charge progress
            float scale = (0.8f + chargeProgress * 1.5f) * pulse;
            spriteBatch.Draw(glowTex, drawPos, null, FateBlack * 0.9f * chargeProgress, 0f, origin, scale * 1.2f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, FateDarkPink * 0.6f * chargeProgress, 0f, origin, scale * 0.9f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, FateBrightRed * 0.7f * chargeProgress, 0f, origin, scale * 0.6f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, FateWhite * 0.8f * chargeProgress, 0f, origin, scale * 0.3f, SpriteEffects.None, 0f);
            
            return false;
        }
        
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
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = ChargeDuration + 30; // Charge + explosion duration
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }
        
        public override void AI()
        {
            int timer = ChargeDuration + 30 - Projectile.timeLeft;
            float chargeProgress = Math.Min(1f, (float)timer / ChargeDuration);
            
            // Charging phase
            if (timer < ChargeDuration)
            {
                // Rotating glyph circle that intensifies
                float rotation = Main.GameUpdateCount * 0.04f;
                int glyphCount = 8 + (int)(chargeProgress * 8);
                float currentRadius = 40f + chargeProgress * 80f;
                
                if (timer % 3 == 0)
                {
                    for (int i = 0; i < glyphCount; i++)
                    {
                        float angle = rotation + MathHelper.TwoPi * i / glyphCount;
                        Vector2 glyphPos = Projectile.Center + angle.ToRotationVector2() * currentRadius;
                        float progress = (float)i / glyphCount;
                        Color glyphColor = GetFateGradient(progress) * (0.4f + chargeProgress * 0.4f);
                        CustomParticles.Glyph(glyphPos, glyphColor, 0.25f + chargeProgress * 0.15f, -1);
                    }
                }
                
                // Converging particles toward center
                if (timer % 2 == 0)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                        float dist = JudgmentRadius * (1f - chargeProgress * 0.3f);
                        Vector2 particleStart = Projectile.Center + angle.ToRotationVector2() * dist;
                        Vector2 velocity = (Projectile.Center - particleStart).SafeNormalize(Vector2.Zero) * 3f;
                        
                        float progress = Main.rand.NextFloat();
                        Color particleColor = GetFateGradient(progress) * (0.3f + chargeProgress * 0.5f);
                        var glow = new GenericGlowParticle(particleStart, velocity, particleColor, 0.25f + chargeProgress * 0.15f, 20, true);
                        MagnumParticleHandler.SpawnParticle(glow);
                    }
                }
                
                // Central pulsing void
                float pulse = (float)Math.Sin(timer * 0.15f) * 0.15f + 1f;
                CustomParticles.GenericFlare(Projectile.Center, FateBlack * chargeProgress, 0.5f * pulse, 8);
                
                // Chromatic aberration at center
                CustomParticles.GenericFlare(Projectile.Center + new Vector2(-4 * chargeProgress, 0), FateBrightRed * 0.4f * chargeProgress, 0.3f, 6);
                CustomParticles.GenericFlare(Projectile.Center + new Vector2(4 * chargeProgress, 0), FatePurple * 0.4f * chargeProgress, 0.3f, 6);
                
                // Mark enemies in range - they CANNOT ESCAPE
                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (!npc.friendly && npc.CanBeChasedBy() && Vector2.Distance(npc.Center, Projectile.Center) <= JudgmentRadius)
                    {
                        if (!markedEnemies.Contains(npc.whoAmI))
                            markedEnemies.Add(npc.whoAmI);
                        
                        // Visual connection to marked enemies
                        if (timer % 10 == 0)
                        {
                            Vector2 toEnemy = (npc.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                            for (int i = 0; i < 5; i++)
                            {
                                float t = (float)i / 5f;
                                Vector2 connectionPos = Vector2.Lerp(Projectile.Center, npc.Center, t);
                                CustomParticles.GenericFlare(connectionPos, FateDarkPink * 0.5f, 0.2f, 8);
                            }
                        }
                        
                        // Destiny marker on enemy
                        if (timer % 15 == 0)
                        {
                            CustomParticles.Glyph(npc.Center, FateBrightRed * 0.6f, 0.35f, -1);
                        }
                    }
                }
                
                // Ominous sound cue as charge completes
                if (timer == ChargeDuration - 10)
                {
                    SoundEngine.PlaySound(SoundID.Item119 with { Pitch = -0.5f, Volume = 0.7f }, Projectile.Center);
                }
                
                Lighting.AddLight(Projectile.Center, GetFateGradient(chargeProgress).ToVector3() * (0.3f + chargeProgress * 0.5f));
            }
            // JUDGMENT phase
            else if (timer == ChargeDuration)
            {
                TriggerJudgment();
            }
            // Post-judgment dissipation
            else
            {
                float dissipateProgress = (float)(timer - ChargeDuration) / 30f;
                
                // Fading afterglow
                if (Main.rand.NextBool(2))
                {
                    float progress = Main.rand.NextFloat();
                    CustomParticles.GenericFlare(Projectile.Center + Main.rand.NextVector2Circular(50f, 50f), 
                        GetFateGradient(progress) * (1f - dissipateProgress) * 0.5f, 0.3f, 10);
                }
            }
        }
        
        private void TriggerJudgment()
        {
            SoundEngine.PlaySound(SoundID.Item71 with { Pitch = -0.4f, Volume = 1f }, Projectile.Center);
            
            Player owner = Main.player[Projectile.owner];
            
            // === FATE-EXCLUSIVE REALITY DISTORTION - JUDGMENT SHATTERS REALITY ===
            FateRealityDistortion.TriggerChromaticAberration(Projectile.Center, 8f, 30);
            FateRealityDistortion.TriggerInversionPulse(10);
            MagnumScreenEffects.AddScreenShake(15f);
            MagnumScreenEffects.SetFlashEffect(Projectile.Center, 1.8f, 25);
            
            // MASSIVE JUDGMENT EXPLOSION
            
            // Multi-layer fractal burst
            for (int layer = 0; layer < 5; layer++)
            {
                int points = 12 + layer * 4;
                float radius = 30f + layer * 35f;
                
                for (int i = 0; i < points; i++)
                {
                    float angle = MathHelper.TwoPi * i / points + layer * 0.15f;
                    Vector2 offset = angle.ToRotationVector2() * radius;
                    float gradientProgress = ((float)i / points + layer * 0.2f) % 1f;
                    Color burstColor = GetFateGradient(gradientProgress);
                    CustomParticles.GenericFlare(Projectile.Center + offset, burstColor, 0.65f - layer * 0.08f, 25);
                    
                    // Heavy RGB separation
                    CustomParticles.GenericFlare(Projectile.Center + offset + new Vector2(-4, 0), FateBrightRed * 0.4f, 0.3f, 15);
                    CustomParticles.GenericFlare(Projectile.Center + offset + new Vector2(4, 0), FatePurple * 0.4f, 0.3f, 15);
                }
            }
            
            // Massive expanding halos
            for (int ring = 0; ring < 6; ring++)
            {
                float ringProgress = ring / 6f;
                Color ringColor = GetFateGradient(ringProgress);
                CustomParticles.HaloRing(Projectile.Center, ringColor, 0.5f + ring * 0.2f, 20 + ring * 5);
            }
            
            // Glyph explosion
            CustomParticles.GlyphBurst(Projectile.Center, FateDarkPink, 16, 8f);
            CustomParticles.GlyphCircle(Projectile.Center, FateBrightRed, 12, 100f, 0.06f);
            
            // Central white flash
            CustomParticles.GenericFlare(Projectile.Center, FateWhite, 1.2f, 15);
            
            // DEAL MASSIVE DAMAGE TO ALL MARKED ENEMIES
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.friendly && npc.CanBeChasedBy())
                {
                    bool inRange = Vector2.Distance(npc.Center, Projectile.Center) <= JudgmentRadius;
                    bool wasMarked = markedEnemies.Contains(npc.whoAmI);
                    
                    if (inRange || wasMarked)
                    {
                        // Marked enemies take EXTRA damage - they couldn't escape!
                        int multiplier = wasMarked ? 3 : 2;
                        npc.SimpleStrikeNPC(Projectile.damage * multiplier, 0, true, 10f);
                        npc.AddBuff(ModContent.BuffType<DestinyCollapse>(), 600);
                        npc.GetGlobalNPC<DestinyCollapseNPC>().AddStack(npc, wasMarked ? 4 : 2);
                        
                        // Individual judgment effect on each enemy
                        for (int i = 0; i < 6; i++)
                        {
                            float progress = (float)i / 6f;
                            CustomParticles.GenericFlare(npc.Center + Main.rand.NextVector2Circular(20f, 20f), GetFateGradient(progress), 0.5f, 16);
                        }
                        
                        CustomParticles.GlyphImpact(npc.Center, FateBlack, FateBrightRed, 0.6f);
                    }
                }
            }
            
            // Intense light burst
            Lighting.AddLight(Projectile.Center, FateBrightRed.ToVector3() * 1.5f);
        }
        
        public override bool? CanDamage() => false; // Damage is dealt manually in TriggerJudgment
    }
}
