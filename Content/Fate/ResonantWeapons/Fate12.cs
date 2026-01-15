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
    /// Singularity Familiar - Large cosmic summon that creates cosmic zones
    /// </summary>
    public class Fate12 : ModItem
    {
        // Dark Prismatic color palette - ULTIMATE
        private static readonly Color FateBlack = new Color(15, 5, 20);
        private static readonly Color FateDarkPink = new Color(180, 50, 100);
        private static readonly Color FateBrightRed = new Color(255, 60, 80);
        private static readonly Color FatePurple = new Color(120, 30, 140);
        private static readonly Color FateWhite = new Color(255, 255, 255);
        
        public override string Texture => "Terraria/Images/Item_" + ItemID.MoonlordTurretStaff;
        
        public override void SetDefaults()
        {
            Item.damage = 220;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 20;
            Item.width = 50;
            Item.height = 50;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(gold: 30);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item117;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<SingularityFamiliar>();
            Item.buffType = ModContent.BuffType<SingularityFamiliarBuff>();
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Summons a Singularity Familiar"));
            tooltips.Add(new TooltipLine(Mod, "FateEffect2", "Creates Cosmic Zones that damage and debuff enemies"));
            tooltips.Add(new TooltipLine(Mod, "FateEffect3", "Zones intensify over time and collapse explosively"));
            tooltips.Add(new TooltipLine(Mod, "FateLore", "'A tamed black hole, bound to your will'") 
            { 
                OverrideColor = FateDarkPink 
            });
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            
            Projectile.NewProjectile(source, player.Center, Vector2.Zero, type, damage, knockback, player.whoAmI);
            
            // ULTIMATE SINGULARITY SUMMONING VFX
            CustomParticles.GenericFlare(player.Center, FateBlack, 1.5f, 28);
            CustomParticles.GenericFlare(player.Center, FateBrightRed, 1.3f, 26);
            CustomParticles.HaloRing(player.Center, FateDarkPink, 0.9f, 20);
            CustomParticles.HaloRing(player.Center, FatePurple, 0.75f, 18);
            
            // Massive chromatic aberration summon burst
            CustomParticles.GenericFlare(player.Center + new Vector2(-8, 0), Color.Red * 0.6f, 0.6f, 18);
            CustomParticles.GenericFlare(player.Center, Color.Green * 0.5f, 0.55f, 17);
            CustomParticles.GenericFlare(player.Center + new Vector2(8, 0), Color.Cyan * 0.6f, 0.6f, 18);
            
            // EPIC singularity summoning - massive glyph ritual ENHANCED
            CustomParticles.GlyphCircle(player.Center, FateBrightRed, 16, 80f, 0.05f);
            CustomParticles.GlyphTower(player.Center, FatePurple, 7, 0.65f);
            CustomParticles.GlyphBurst(player.Center, FateDarkPink, 12, 5f);
            
            // Reality distortion effect - dark prismatic cascade
            for (int ring = 0; ring < 6; ring++)
            {
                float ringProgress = ring / 6f;
                Color ringColor = Color.Lerp(FateBlack, FateBrightRed, ringProgress);
                CustomParticles.HaloRing(player.Center, ringColor * 0.7f, 0.45f + ring * 0.22f, 16 + ring * 4);
            }
            
            // Chromatic fractal burst - ULTIMATE
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 offset = angle.ToRotationVector2() * 50f;
                float progress = (float)i / 16f;
                
                // Dark prismatic gradient
                Color burstColor;
                if (progress < 0.4f)
                    burstColor = Color.Lerp(FateBlack, FateDarkPink, progress / 0.4f);
                else if (progress < 0.8f)
                    burstColor = Color.Lerp(FateDarkPink, FateBrightRed, (progress - 0.4f) / 0.4f);
                else
                    burstColor = Color.Lerp(FateBrightRed, FateWhite, (progress - 0.8f) / 0.2f);
                
                CustomParticles.GenericFlare(player.Center + offset, burstColor, 0.42f, 15);
                
                // Chromatic per-point
                CustomParticles.GenericFlare(player.Center + offset + new Vector2(-3, 0), Color.Red * 0.35f, 0.25f, 10);
                CustomParticles.GenericFlare(player.Center + offset + new Vector2(3, 0), Color.Cyan * 0.35f, 0.25f, 10);
            }
            
            // Destiny glyphs orbiting during summon
            for (int j = 0; j < 6; j++)
            {
                float glyphAngle = MathHelper.TwoPi * j / 6f;
                Vector2 glyphPos = player.Center + glyphAngle.ToRotationVector2() * 60f;
                CustomParticles.Glyph(glyphPos, FateBrightRed, 0.45f, -1);
            }
            
            // ULTIMATE SINGULARITY - Cosmic music notes cascade outward!
            ThemedParticles.FateMusicNoteBurst(player.Center, 18, 8f);
            ThemedParticles.FateMusicNotes(player.Center, 14, 70f);
            
            return false;
        }
    }
    
    public class SingularityFamiliarBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.Confused;
        
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }
        
        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<SingularityFamiliar>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
    
    public class SingularityFamiliar : ModProjectile
    {
        // Dark Prismatic color palette - ULTIMATE
        private static readonly Color FateBlack = new Color(15, 5, 20);
        private static readonly Color FateDarkPink = new Color(180, 50, 100);
        private static readonly Color FateBrightRed = new Color(255, 60, 80);
        private static readonly Color FatePurple = new Color(120, 30, 140);
        private static readonly Color FateWhite = new Color(255, 255, 255);
        
        private int ZoneCooldown = 0;
        private int AttackCooldown = 0;
        
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
            Main.projPet[Projectile.type] = true;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.minionSlots = 2f;
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.15f + 1f;
            
            // Large outer glow - cosmic energy
            spriteBatch.Draw(texture, drawPos, null, FatePurple * 0.25f, Projectile.rotation, origin, 1.5f * pulse, SpriteEffects.None, 0f);
            // Dark pink layer
            spriteBatch.Draw(texture, drawPos, null, FateDarkPink * 0.4f, Projectile.rotation * 0.5f, origin, 1.1f * pulse, SpriteEffects.None, 0f);
            // Bright red layer
            spriteBatch.Draw(texture, drawPos, null, FateBrightRed * 0.5f, Projectile.rotation * 0.8f, origin, 0.75f * pulse, SpriteEffects.None, 0f);
            // White core
            spriteBatch.Draw(texture, drawPos, null, FateWhite * 0.8f, Projectile.rotation, origin, 0.45f * pulse, SpriteEffects.None, 0f);
            
            return false;
        }
        
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<SingularityFamiliarBuff>());
                return;
            }
            
            if (owner.HasBuff(ModContent.BuffType<SingularityFamiliarBuff>()))
            {
                Projectile.timeLeft = 2;
            }
            
            // Movement - follows player but with smooth acceleration
            Vector2 targetPos = owner.Center + new Vector2(owner.direction * -50f, -60f);
            float dist = Vector2.Distance(Projectile.Center, targetPos);
            
            if (dist > 300f)
            {
                Projectile.Center = targetPos;
            }
            else
            {
                Vector2 direction = (targetPos - Projectile.Center).SafeNormalize(Vector2.Zero);
                float speed = Math.Min(dist * 0.1f, 12f);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * speed, 0.1f);
                Projectile.Center += Projectile.velocity;
            }
            
            Projectile.rotation += 0.03f;
            
            // Cosmic swirling visuals around the familiar - ULTIMATE dark prismatic
            if (Main.GameUpdateCount % 2 == 0)
            {
                int armCount = 6;  // Enhanced from 4
                for (int arm = 0; arm < armCount; arm++)
                {
                    float armAngle = Main.GameUpdateCount * 0.07f + MathHelper.TwoPi * arm / armCount;
                    for (int point = 0; point < 6; point++)  // Enhanced from 5
                    {
                        float spiralAngle = armAngle + point * 0.28f;
                        float radius = 18f + point * 10f;
                        Vector2 spiralPos = Projectile.Center + spiralAngle.ToRotationVector2() * radius;
                        
                        float gradientProgress = (arm + point * 0.18f) / (armCount + 1f);
                        Color spiralColor = GetFateGradient(gradientProgress) * 0.55f;
                        
                        if (point % 2 == 0)
                        {
                            CustomParticles.GenericFlare(spiralPos, spiralColor * 0.45f, 0.14f, 7);
                        }
                    }
                }
            }
            
            // Chromatic aberration around familiar
            if (Main.GameUpdateCount % 4 == 0)
            {
                float chromAngle = Main.GameUpdateCount * 0.08f;
                Vector2 chromPos = Projectile.Center + chromAngle.ToRotationVector2() * 25f;
                CustomParticles.GenericFlare(chromPos + new Vector2(-3, 0), Color.Red * 0.25f, 0.1f, 5);
                CustomParticles.GenericFlare(chromPos + new Vector2(3, 0), Color.Cyan * 0.25f, 0.1f, 5);
            }
            
            // Central void pulsing - ULTIMATE intensity
            if (Main.GameUpdateCount % 6 == 0)
            {
                float pulse = 0.38f + (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.12f;
                CustomParticles.GenericFlare(Projectile.Center, FateBlack, pulse, 12);
                CustomParticles.GenericFlare(Projectile.Center, FateBrightRed * 0.4f, pulse * 0.7f, 10);
            }
            
            // Event horizon ring - ENHANCED
            if (Main.GameUpdateCount % 12 == 0)
            {
                CustomParticles.HaloRing(Projectile.Center, FateDarkPink * 0.5f, 0.4f, 14);
                CustomParticles.HaloRing(Projectile.Center, FatePurple * 0.35f, 0.32f, 11);
                
                // Cosmic glyph circle around the familiar's event horizon - enhanced
                CustomParticles.GlyphCircle(Projectile.Center, FateBrightRed * 0.55f, 7, 40f, -0.04f);
            }
            
            // Floating destiny glyphs around the singularity - enhanced frequency
            if (Main.GameUpdateCount % 22 == 0)
            {
                CustomParticles.GlyphAura(Projectile.Center, FateBrightRed * 0.45f, 52f, 3);
            }
            
            // Create cosmic zones
            ZoneCooldown--;
            if (ZoneCooldown <= 0)
            {
                NPC target = FindTarget();
                if (target != null)
                {
                    CreateCosmicZone(target.Center);
                    ZoneCooldown = 120;
                }
            }
            
            // Direct attacks
            AttackCooldown--;
            if (AttackCooldown <= 0)
            {
                NPC target = FindTarget();
                if (target != null)
                {
                    FireGravityBolt(target);
                    AttackCooldown = 25;
                }
            }
            
            Lighting.AddLight(Projectile.Center, FatePurple.ToVector3() * 0.5f);
        }
        
        private NPC FindTarget()
        {
            float maxRange = 700f;
            NPC closest = null;
            float closestDist = maxRange;
            
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly || npc.dontTakeDamage) continue;
                
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            
            return closest;
        }
        
        private void CreateCosmicZone(Vector2 position)
        {
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), position, Vector2.Zero,
                ModContent.ProjectileType<CosmicZone>(), Projectile.damage, 0f, Projectile.owner);
            
            // Zone creation VFX - ULTIMATE dark prismatic
            CustomParticles.GenericFlare(position, FateBlack, 0.85f, 18);
            CustomParticles.GenericFlare(position, FateBrightRed * 0.85f, 0.75f, 17);
            CustomParticles.HaloRing(position, FateDarkPink * 0.65f, 0.55f, 14);
            CustomParticles.HaloRing(position, FatePurple * 0.5f, 0.45f, 12);
            
            // Chromatic zone creation
            CustomParticles.GenericFlare(position + new Vector2(-5, 0), Color.Red * 0.4f, 0.35f, 12);
            CustomParticles.GenericFlare(position + new Vector2(5, 0), Color.Cyan * 0.4f, 0.35f, 12);
            
            // Cosmic zone creation with destiny glyphs - ENHANCED
            CustomParticles.GlyphCircle(position, FateBrightRed, 9, 52f, 0.04f);
            CustomParticles.GlyphTower(position, FatePurple, 3, 0.4f);
            
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 offset = angle.ToRotationVector2() * 30f;
                Color burstColor = GetFateGradient((float)i / 8f);
                CustomParticles.GenericFlare(position + offset, burstColor * 0.55f, 0.35f, 13);
            }
            
            // Cosmic zone creation spawns music notes!
            ThemedParticles.FateMusicNotes(position, 8, 45f);
            
            SoundEngine.PlaySound(SoundID.Item8 with { Volume = 0.6f, Pitch = -0.2f }, position);
        }
        
        private void FireGravityBolt(NPC target)
        {
            Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, direction * 16f,
                ModContent.ProjectileType<GravityBolt>(), Projectile.damage / 2, Projectile.knockBack, Projectile.owner);
            
            CustomParticles.GenericFlare(Projectile.Center, FateBlack, 0.5f, 12);
            CustomParticles.GenericFlare(Projectile.Center, FateBrightRed * 0.65f, 0.45f, 11);
            
            // Destiny glyph on gravity bolt fire - enhanced
            CustomParticles.Glyph(Projectile.Center, FateBrightRed, 0.4f, -1);
            
            // Chromatic fire flash
            CustomParticles.GenericFlare(Projectile.Center + new Vector2(-3, 0), Color.Red * 0.28f, 0.18f, 8);
            CustomParticles.GenericFlare(Projectile.Center + new Vector2(3, 0), Color.Cyan * 0.28f, 0.18f, 8);
        }
    }
    
    public class CosmicZone : ModProjectile
    {
        // Dark Prismatic color palette - ULTIMATE
        private static readonly Color FateBlack = new Color(15, 5, 20);
        private static readonly Color FateDarkPink = new Color(180, 50, 100);
        private static readonly Color FateBrightRed = new Color(255, 60, 80);
        private static readonly Color FatePurple = new Color(120, 30, 140);
        private static readonly Color FateWhite = new Color(255, 255, 255);
        
        private const int MaxDuration = 180;
        private const float BaseRadius = 110f;  // Enhanced from 100f
        private const float MaxRadius = 200f;   // Enhanced from 180f
        
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
        
        public override void SetDefaults()
        {
            Projectile.width = 100;
            Projectile.height = 100;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = MaxDuration;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            // The zone is fully particle-based, no sprite needed
            return false;
        }
        
        public override void AI()
        {
            float lifeProgress = 1f - (Projectile.timeLeft / (float)MaxDuration);
            float intensity = (float)Math.Sin(lifeProgress * MathHelper.Pi);
            float radius = BaseRadius + (MaxRadius - BaseRadius) * lifeProgress;
            
            Projectile.width = (int)(radius * 2);
            Projectile.height = (int)(radius * 2);
            
            // Swirling cosmic vortex - ULTIMATE dark prismatic
            int armCount = 8;  // Enhanced from 6
            for (int arm = 0; arm < armCount; arm++)
            {
                float armAngle = Main.GameUpdateCount * 0.06f + MathHelper.TwoPi * arm / armCount;
                
                if (Main.GameUpdateCount % 2 == 0)
                {
                    for (int point = 0; point < 10; point++)  // Enhanced from 8
                    {
                        float spiralAngle = armAngle + point * 0.22f;
                        float spiralRadius = radius * 0.18f + point * (radius / 10f);
                        Vector2 spiralPos = Projectile.Center + spiralAngle.ToRotationVector2() * spiralRadius;
                        
                        float gradientProgress = (arm + point * 0.08f) / (armCount + 1f);
                        Color spiralColor = GetFateGradient(gradientProgress) * intensity;
                        
                        if (point % 2 == 0)
                        {
                            CustomParticles.GenericFlare(spiralPos, spiralColor * 0.55f, 0.17f, 9);
                        }
                    }
                }
            }
            
            // Central void - ULTIMATE intensity
            if (Main.GameUpdateCount % 5 == 0)
            {
                CustomParticles.GenericFlare(Projectile.Center, FateBlack, 0.48f * intensity, 14);
                CustomParticles.GenericFlare(Projectile.Center, FateBrightRed * 0.35f * intensity, 0.35f, 11);
            }
            
            // Edge distortion with chromatic aberration - ENHANCED
            if (lifeProgress > 0.25f && Main.rand.NextBool(2))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 edgePos = Projectile.Center + angle.ToRotationVector2() * radius * 0.92f;
                
                CustomParticles.GenericFlare(edgePos + new Vector2(-3, 0), Color.Red * 0.45f * intensity, 0.17f, 9);
                CustomParticles.GenericFlare(edgePos, FateDarkPink * 0.35f * intensity, 0.14f, 8);
                CustomParticles.GenericFlare(edgePos + new Vector2(3, 0), Color.Cyan * 0.45f * intensity, 0.17f, 9);
            }
            
            // Outer halo pulse - ENHANCED
            if (Main.GameUpdateCount % 10 == 0)
            {
                CustomParticles.HaloRing(Projectile.Center, FateDarkPink * intensity * 0.5f, radius / 180f, 17);
                CustomParticles.HaloRing(Projectile.Center, FatePurple * intensity * 0.35f, radius / 220f, 14);
            }
            
            // Cosmic glyph circle within the zone - rotating destiny runes ENHANCED
            if (Main.GameUpdateCount % 8 == 0 && intensity > 0.25f)
            {
                CustomParticles.GlyphCircle(Projectile.Center, FateBrightRed * intensity, 8, radius * 0.55f, 0.03f);
            }
            
            // Floating destiny glyphs in the cosmic zone - ENHANCED frequency
            if (Main.GameUpdateCount % 14 == 0)
            {
                CustomParticles.GlyphAura(Projectile.Center, FateBrightRed * intensity * 0.65f, radius * 0.75f, 3);
            }
            
            // Pull and damage enemies
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly) continue;
                
                float dist = Vector2.Distance(npc.Center, Projectile.Center);
                if (dist <= radius)
                {
                    // Pull toward center - ENHANCED strength
                    Vector2 pullDir = (Projectile.Center - npc.Center).SafeNormalize(Vector2.Zero);
                    float pullStrength = (2.5f + lifeProgress * 5f) * (1f - dist / radius);
                    npc.velocity += pullDir * pullStrength * 0.1f;
                    
                    // Slow - ENHANCED
                    npc.velocity *= 0.94f;
                }
            }
            
            Lighting.AddLight(Projectile.Center, FateDarkPink.ToVector3() * intensity * 0.7f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            float lifeProgress = 1f - (Projectile.timeLeft / (float)MaxDuration);
            int stacks = 2 + (int)(lifeProgress * 3);  // Enhanced stacking
            
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);
            target.GetGlobalNPC<DestinyCollapseNPC>().AddStack(target, stacks);
            
            // === UNIFIED VFX HIT EFFECT - FATE THEME (scaling with zone intensity) ===
            float vfxScale = 0.8f + lifeProgress * 0.5f;
            UnifiedVFX.Fate.HitEffect(target.Center, vfxScale);
            
            // Dark prismatic zone damage VFX
            CustomParticles.GenericFlare(target.Center, FateBlack, 0.45f, 12);
            CustomParticles.GenericFlare(target.Center, FateBrightRed * 0.7f, 0.40f, 11);
            
            // === CHROMATIC ABERRATION ===
            CustomParticles.GenericFlare(target.Center + new Vector2(-3, 0), FateBrightRed * 0.4f, 0.3f, 12);
            CustomParticles.GenericFlare(target.Center + new Vector2(3, 0), FatePurple * 0.4f, 0.3f, 12);
            
            // === GLYPH FORMATIONS ===
            CustomParticles.GlyphImpact(target.Center, FateBlack, FateBrightRed, 0.5f);
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
            
            // Cosmic Revisit - zone echoes
            int revisitDamage = (int)(damageDone * 0.20f);
            target.GetGlobalNPC<DestinyCollapseNPC>().QueueCosmicRevisit(target, revisitDamage, 25, Projectile.Center, 0.7f);
            
            Lighting.AddLight(target.Center, FateBrightRed.ToVector3() * 0.9f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // ULTIMATE COLLAPSE EXPLOSION
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.3f, Volume = 1.1f }, Projectile.Center);
            
            // Central void implosion then explosion
            CustomParticles.GenericFlare(Projectile.Center, FateBlack, 2.0f, 30);
            CustomParticles.GenericFlare(Projectile.Center, FateBrightRed, 1.8f, 28);
            CustomParticles.GenericFlare(Projectile.Center, FateWhite * 0.8f, 1.5f, 25);
            
            // Multi-layer halo collapse - ENHANCED
            for (int ring = 0; ring < 10; ring++)
            {
                Color ringColor = GetFateGradient(ring / 10f);
                float scale = 1.0f - ring * 0.08f;
                CustomParticles.HaloRing(Projectile.Center, ringColor, scale, 16 + ring * 2);
            }
            
            // Chromatic collapse burst
            for (int chroma = 0; chroma < 6; chroma++)
            {
                float angle = MathHelper.TwoPi * chroma / 6f;
                Vector2 offset = angle.ToRotationVector2() * 25f;
                CustomParticles.GenericFlare(Projectile.Center + offset + new Vector2(-4, 0), Color.Red * 0.5f, 0.4f, 15);
                CustomParticles.GenericFlare(Projectile.Center + offset + new Vector2(4, 0), Color.Cyan * 0.5f, 0.4f, 15);
            }
            
            // MASSIVE cosmic zone collapse with glyph explosion - ULTIMATE
            CustomParticles.GlyphCircle(Projectile.Center, FateBrightRed, 14, 110f, 0.06f);
            CustomParticles.GlyphBurst(Projectile.Center, FateDarkPink, 18, 9f);
            CustomParticles.GlyphTower(Projectile.Center, FatePurple, 7, 0.7f);
            
            // Particle explosion - ENHANCED count and speed
            for (int i = 0; i < 40; i++)
            {
                float angle = MathHelper.TwoPi * i / 40f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(10f, 18f);
                Color burstColor = GetFateGradient((float)i / 40f);
                
                var glow = new GenericGlowParticle(Projectile.Center, vel, burstColor, 
                    Main.rand.NextFloat(0.5f, 0.85f), Main.rand.Next(35, 50), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Secondary fractal burst
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 offset = angle.ToRotationVector2() * 60f;
                Color fractColor = GetFateGradient((float)i / 12f);
                CustomParticles.GenericFlare(Projectile.Center + offset, fractColor, 0.55f, 18);
            }
            
            // ULTIMATE COSMIC COLLAPSE - Music notes explode outward in a symphony of destruction!
            ThemedParticles.FateMusicNoteBurst(Projectile.Center, 20, 10f);
            ThemedParticles.FateMusicNotes(Projectile.Center, 16, 80f);
            
            // Damage nearby enemies on collapse - ENHANCED
            float explosionRadius = MaxRadius * 1.6f;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.friendly)
                {
                    float dist = Vector2.Distance(npc.Center, Projectile.Center);
                    if (dist <= explosionRadius)
                    {
                        float falloff = 1f - (dist / explosionRadius) * 0.35f;
                        npc.SimpleStrikeNPC((int)(Projectile.damage * 1.8f * falloff), 0, true, 10f);
                        
                        npc.AddBuff(ModContent.BuffType<DestinyCollapse>(), 360);
                        npc.GetGlobalNPC<DestinyCollapseNPC>().AddStack(npc, 5);  // Enhanced stacks
                    }
                }
            }
        }
    }
    
    public class GravityBolt : ModProjectile
    {
        // Dark Prismatic color palette
        private static readonly Color FateBlack = new Color(15, 5, 20);
        private static readonly Color FateDarkPink = new Color(180, 50, 100);
        private static readonly Color FateBrightRed = new Color(255, 60, 80);
        private static readonly Color FatePurple = new Color(120, 30, 140);
        private static readonly Color FateWhite = new Color(255, 255, 255);
        
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
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() / 2f;
            
            // Draw chromatic aberration trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                float trailAlpha = (1f - progress) * 0.5f;
                float trailScale = 0.45f - progress * 0.3f;
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                
                // RGB separation for chromatic effect
                spriteBatch.Draw(texture, drawPos + new Vector2(-2, 0), null, Color.Red * trailAlpha * 0.4f, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
                spriteBatch.Draw(texture, drawPos + new Vector2(2, 0), null, Color.Cyan * trailAlpha * 0.4f, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
                
                Color trailColor = GetFateGradient(progress);
                spriteBatch.Draw(texture, drawPos, null, trailColor * trailAlpha, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }
            
            // Draw layered glow core
            Vector2 corePos = Projectile.Center - Main.screenPosition;
            spriteBatch.Draw(texture, corePos, null, FateDarkPink * 0.4f, Projectile.rotation, origin, 0.55f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, corePos, null, FateBrightRed * 0.55f, Projectile.rotation, origin, 0.38f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, corePos, null, FateWhite * 0.75f, Projectile.rotation, origin, 0.22f, SpriteEffects.None, 0f);
            
            return false;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            
            // Dark prismatic trail
            if (Main.rand.NextBool(2))
            {
                float progress = Main.rand.NextFloat();
                Color trailColor = GetFateGradient(progress);
                CustomParticles.GenericFlare(Projectile.Center, trailColor * 0.55f, 0.17f, 11);
            }
            
            // Chromatic bolt trail
            if (Main.rand.NextBool(4))
            {
                CustomParticles.GenericFlare(Projectile.Center + new Vector2(-2, 0), Color.Red * 0.22f, 0.08f, 6);
                CustomParticles.GenericFlare(Projectile.Center + new Vector2(2, 0), Color.Cyan * 0.22f, 0.08f, 6);
            }
            
            Lighting.AddLight(Projectile.Center, FateDarkPink.ToVector3() * 0.3f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);
            target.GetGlobalNPC<DestinyCollapseNPC>().AddStack(target, 2);  // Enhanced stacks
            
            // === UNIFIED VFX HIT EFFECT - FATE THEME ===
            UnifiedVFX.Fate.HitEffect(target.Center, 1.0f);
            
            // Dark prismatic hit
            CustomParticles.GenericFlare(target.Center, FateBlack, 0.35f, 12);
            CustomParticles.GenericFlare(target.Center, FateBrightRed * 0.65f, 0.32f, 11);
            
            // Destiny glyph on gravity bolt hit - enhanced
            if (Main.rand.NextBool(2))
            {
                CustomParticles.Glyph(target.Center, FateBrightRed, 0.35f, -1);
            }
            CustomParticles.GlyphImpact(target.Center, FateBlack, FateBrightRed, 0.5f);
            
            // === CHROMATIC ABERRATION ===
            CustomParticles.GenericFlare(target.Center + new Vector2(-3, 0), FateBrightRed * 0.4f, 0.3f, 12);
            CustomParticles.GenericFlare(target.Center + new Vector2(3, 0), FatePurple * 0.4f, 0.3f, 12);
            
            // === GLYPH FORMATIONS ===
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
            
            // Cosmic Revisit - gravity bolt echoes
            int revisitDamage = (int)(damageDone * 0.25f);
            target.GetGlobalNPC<DestinyCollapseNPC>().QueueCosmicRevisit(target, revisitDamage, 25, Projectile.Center, 0.8f);
            
            Lighting.AddLight(target.Center, FateBrightRed.ToVector3() * 0.9f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // Void core flash
            CustomParticles.GenericFlare(Projectile.Center, FateBlack, 0.25f, 10);
            
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * 2.5f;
                Color burstColor = GetFateGradient((float)i / 6f);
                var glow = new GenericGlowParticle(Projectile.Center, vel, burstColor, 0.24f, 12, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Chromatic death flash
            CustomParticles.GenericFlare(Projectile.Center + new Vector2(-2, 0), Color.Red * 0.2f, 0.1f, 6);
            CustomParticles.GenericFlare(Projectile.Center + new Vector2(2, 0), Color.Cyan * 0.2f, 0.1f, 6);
        }
    }
}
