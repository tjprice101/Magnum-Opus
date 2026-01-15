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
    /// Destiny Spiral Grimoire - Creates Fate Maelstrom cosmic vortex
    /// </summary>
    public class Fate7 : ModItem
    {
        // Dark Prismatic color palette
        private static readonly Color FateBlack = new Color(15, 5, 20);
        private static readonly Color FateDarkPink = new Color(180, 50, 100);
        private static readonly Color FateBrightRed = new Color(255, 60, 80);
        private static readonly Color FatePurple = new Color(120, 30, 140);
        private static readonly Color FateWhite = new Color(255, 255, 255);
        
        public override string Texture => "Terraria/Images/Item_" + ItemID.LunarFlareBook;
        
        public override void SetDefaults()
        {
            Item.damage = 560;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 20;
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(gold: 25);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item117;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<FateMaelstrom>();
            Item.shootSpeed = 0f;
            Item.noMelee = true;
            Item.channel = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Creates a Fate Maelstrom at cursor position"));
            tooltips.Add(new TooltipLine(Mod, "FateEffect2", "Hold to channel - the maelstrom grows stronger over time"));
            tooltips.Add(new TooltipLine(Mod, "FateLore", "'A grimoire written in the language of dying stars'") 
            { 
                OverrideColor = FateDarkPink 
            });
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 target = Main.MouseWorld;
            Projectile.NewProjectile(source, target, Vector2.Zero, ModContent.ProjectileType<FateMaelstrom>(), damage, knockback, player.whoAmI);
            
            // Dark prismatic cast effect with chromatic aberration
            CustomParticles.GenericFlare(player.Center, FateBlack, 0.7f, 18);
            CustomParticles.GenericFlare(player.Center, FateBrightRed, 0.6f, 15);
            CustomParticles.HaloRing(player.Center, FateDarkPink, 0.5f, 14);
            
            // Chromatic cast flash
            CustomParticles.GenericFlare(player.Center + new Vector2(-4, 0), Color.Red * 0.4f, 0.35f, 12);
            CustomParticles.GenericFlare(player.Center + new Vector2(4, 0), Color.Cyan * 0.4f, 0.35f, 12);
            
            // Grimoire casting glyph circle around player - enhanced
            CustomParticles.GlyphCircle(player.Center, FateBrightRed, 8, 55f, 0.04f);
            CustomParticles.GlyphTower(player.Center, FatePurple, 3, 0.4f);
            
            // Grimoire conjures cosmic music notes as the maelstrom forms!
            ThemedParticles.FateMusicNotes(player.Center, 8, 40f);
            ThemedParticles.FateMusicNotes(target, 6, 35f);
            
            return false;
        }
    }
    
    public class FateMaelstrom : ModProjectile
    {
        // Dark Prismatic color palette
        private static readonly Color FateBlack = new Color(15, 5, 20);
        private static readonly Color FateDarkPink = new Color(180, 50, 100);
        private static readonly Color FateBrightRed = new Color(255, 60, 80);
        private static readonly Color FatePurple = new Color(120, 30, 140);
        private static readonly Color FateWhite = new Color(255, 255, 255);
        
        private const int MaxChannelTime = 300;
        private int channelTime = 0;
        
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = glowTex.Size() / 2f;
            
            float channelProgress = channelTime / (float)MaxChannelTime;
            float intensity = 0.55f + channelProgress * 0.45f;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.1f + 1f;
            float rotation = Main.GameUpdateCount * 0.03f;
            
            // Draw swirling maelstrom core
            float scale = (1.5f + channelProgress * 2f) * pulse;
            spriteBatch.Draw(glowTex, drawPos, null, FateBlack * 0.9f * intensity, rotation, origin, scale * 1.4f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, FateDarkPink * 0.5f * intensity, -rotation * 0.7f, origin, scale * 1.0f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, FateBrightRed * 0.6f * intensity, rotation * 1.3f, origin, scale * 0.6f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, FateWhite * 0.7f * intensity, 0f, origin, scale * 0.25f, SpriteEffects.None, 0f);
            
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
            Projectile.width = 100;
            Projectile.height = 100;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
        
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            if (owner.channel && !owner.dead)
            {
                channelTime = Math.Min(channelTime + 1, MaxChannelTime);
                Projectile.timeLeft = 60;
            }
            
            float channelProgress = channelTime / (float)MaxChannelTime;
            float baseRadius = 90f + channelProgress * 140f;
            float intensity = 0.55f + channelProgress * 0.45f;
            
            Projectile.width = (int)(baseRadius * 2);
            Projectile.height = (int)(baseRadius * 2);
            
            // Dark prismatic swirling cosmic vortex
            int armCount = 6 + (int)(channelProgress * 4);
            float spiralSpeed = 0.09f + channelProgress * 0.07f;
            
            for (int arm = 0; arm < armCount; arm++)
            {
                float armAngle = Main.GameUpdateCount * spiralSpeed + MathHelper.TwoPi * arm / armCount;
                
                for (int point = 0; point < 12; point++)
                {
                    float spiralAngle = armAngle + point * 0.28f;
                    float spiralRadius = 18f + point * (baseRadius / 11f);
                    Vector2 spiralPos = Projectile.Center + spiralAngle.ToRotationVector2() * spiralRadius;
                    
                    float gradientProgress = (arm + point * 0.08f) / (armCount + 1f);
                    Color spiralColor = GetFateGradient(gradientProgress) * intensity;
                    
                    if (Main.GameUpdateCount % 2 == 0 && point % 2 == 0)
                    {
                        CustomParticles.GenericFlare(spiralPos, spiralColor * 0.65f, 0.22f + channelProgress * 0.12f, 10);
                    }
                }
            }
            
            // Central void - dark core emphasis
            if (Main.GameUpdateCount % 3 == 0)
            {
                CustomParticles.GenericFlare(Projectile.Center, FateBlack, 0.6f * intensity, 14);
                CustomParticles.HaloRing(Projectile.Center, FateDarkPink * intensity * 0.65f, 0.35f + channelProgress * 0.35f, 16);
            }
            
            // Outer halo pulses with dark prismatic gradient
            if (Main.GameUpdateCount % 10 == 0)
            {
                CustomParticles.HaloRing(Projectile.Center, FateBrightRed * intensity * 0.5f, 0.45f + channelProgress * 0.45f, 20);
            }
            
            // Reality-tearing chromatic edge effects
            if (channelProgress > 0.35f && Main.rand.NextBool(2))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 edgePos = Projectile.Center + angle.ToRotationVector2() * baseRadius * 0.9f;
                
                CustomParticles.GenericFlare(edgePos + new Vector2(-4, 0), Color.Red * 0.45f * intensity, 0.22f, 10);
                CustomParticles.GenericFlare(edgePos + new Vector2(4, 0), Color.Cyan * 0.45f * intensity, 0.22f, 10);
            }
            
            // Cosmic glyph maelstrom - destiny runes spiral with the vortex
            if (Main.GameUpdateCount % 7 == 0)
            {
                CustomParticles.GlyphCircle(Projectile.Center, FateBrightRed * intensity, armCount, baseRadius * 0.55f, -spiralSpeed);
            }
            
            // Floating destiny glyphs in the maelstrom
            if (Main.GameUpdateCount % 12 == 0 && channelProgress > 0.15f)
            {
                CustomParticles.GlyphAura(Projectile.Center, FateDarkPink * intensity * 0.75f, baseRadius * 0.8f, 3);
            }
            
            // Temporal echoes at high power
            if (channelProgress > 0.6f && Main.GameUpdateCount % 5 == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    float echoAngle = Main.GameUpdateCount * 0.1f + i * MathHelper.TwoPi / 3f;
                    Vector2 echoPos = Projectile.Center + echoAngle.ToRotationVector2() * baseRadius * 0.6f;
                    Color echoColor = GetFateGradient(i / 3f) * 0.4f;
                    CustomParticles.GenericFlare(echoPos, echoColor, 0.25f, 8);
                }
            }
            
            // Pull enemies toward center with increased strength
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly) continue;
                
                float dist = Vector2.Distance(npc.Center, Projectile.Center);
                if (dist <= baseRadius * 1.6f && dist > 20f)
                {
                    Vector2 pullDir = (Projectile.Center - npc.Center).SafeNormalize(Vector2.Zero);
                    float pullStrength = (2.5f + channelProgress * 5f) * (1f - dist / (baseRadius * 1.6f));
                    npc.velocity += pullDir * pullStrength * 0.12f;
                    npc.velocity = npc.velocity.RotatedBy(0.06f);
                }
            }
            
            Lighting.AddLight(Projectile.Center, FateDarkPink.ToVector3() * intensity * 0.75f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            float channelProgress = channelTime / (float)MaxChannelTime;
            int stacks = 1 + (int)(channelProgress * 3);
            
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);
            target.GetGlobalNPC<DestinyCollapseNPC>().AddStack(target, stacks);
            
            // === UNIFIED VFX HIT EFFECT - FATE THEME ===
            UnifiedVFX.Fate.HitEffect(target.Center, 1.0f + channelProgress * 0.5f);
            
            // Dark prismatic hit VFX
            CustomParticles.GenericFlare(target.Center, FateBlack, 0.45f, 12);
            
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 offset = angle.ToRotationVector2() * 22f;
                Color hitColor = GetFateGradient((float)i / 5f);
                CustomParticles.GenericFlare(target.Center + offset, hitColor * 0.75f, 0.32f, 12);
            }
            
            // === CHROMATIC ABERRATION ===
            CustomParticles.GenericFlare(target.Center + new Vector2(-3, 0), FateBrightRed * 0.4f, 0.3f, 12);
            CustomParticles.GenericFlare(target.Center + new Vector2(3, 0), FatePurple * 0.4f, 0.3f, 12);
            
            // Glyph impact
            CustomParticles.GlyphImpact(target.Center, FateBlack, FateBrightRed, 0.55f);
            CustomParticles.GlyphCircle(target.Center, FateDarkPink, 5, 40f, 0.08f);
            
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
            
            // Cosmic Revisit - maelstrom echoes
            int revisitDamage = (int)(damageDone * 0.25f);
            target.GetGlobalNPC<DestinyCollapseNPC>().QueueCosmicRevisit(target, revisitDamage, 25, Projectile.Center, 0.8f);
            
            Lighting.AddLight(target.Center, FateBrightRed.ToVector3() * 0.9f);
        }
        
        public override void OnKill(int timeLeft)
        {
            float channelProgress = channelTime / (float)MaxChannelTime;
            
            SoundEngine.PlaySound(SoundID.Item8 with { Pitch = -0.35f, Volume = 0.9f }, Projectile.Center);
            
            // Dark prismatic collapse burst
            CustomParticles.GenericFlare(Projectile.Center, FateBlack, 1.0f + channelProgress * 0.6f, 25);
            CustomParticles.GenericFlare(Projectile.Center, FateBrightRed, 0.85f + channelProgress * 0.5f, 22);
            
            for (int ring = 0; ring < 7; ring++)
            {
                Color ringColor = GetFateGradient(ring / 7f);
                CustomParticles.HaloRing(Projectile.Center, ringColor, 0.35f + ring * 0.16f, 16 + ring * 3);
            }
            
            // Chromatic collapse burst
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 offset = angle.ToRotationVector2() * 40f;
                CustomParticles.GenericFlare(Projectile.Center + offset + new Vector2(-4, 0), Color.Red * 0.45f, 0.3f, 15);
                CustomParticles.GenericFlare(Projectile.Center + offset + new Vector2(4, 0), Color.Cyan * 0.45f, 0.3f, 15);
            }
            
            for (int i = 0; i < 25; i++)
            {
                float angle = MathHelper.TwoPi * i / 25f;
                Vector2 vel = angle.ToRotationVector2() * (5f + channelProgress * 7f);
                Color burstColor = GetFateGradient((float)i / 25f);
                
                var glow = new GenericGlowParticle(Projectile.Center, vel, burstColor, 0.45f, 28, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Maelstrom collapse with glyph explosion - enhanced
            CustomParticles.GlyphBurst(Projectile.Center, FateBrightRed, 12 + (int)(channelProgress * 8), 6f + channelProgress * 4f);
            CustomParticles.GlyphTower(Projectile.Center, FateDarkPink, 4 + (int)(channelProgress * 3), 0.5f);
            CustomParticles.GlyphCircle(Projectile.Center, FatePurple, 8, 70f, 0.05f);
            
            // Maelstrom collapse releases cosmic music notes!
            ThemedParticles.FateMusicNoteBurst(Projectile.Center, 12 + (int)(channelProgress * 6), 7f);
            ThemedParticles.FateMusicNotes(Projectile.Center, 10, 55f);
        }
    }
}
