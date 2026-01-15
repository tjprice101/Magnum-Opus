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
    /// Destiny Choir Baton - Summons 3 cosmic singers in formation
    /// </summary>
    public class Fate11 : ModItem
    {
        // Dark Prismatic color palette
        private static readonly Color FateBlack = new Color(15, 5, 20);
        private static readonly Color FateDarkPink = new Color(180, 50, 100);
        private static readonly Color FateBrightRed = new Color(255, 60, 80);
        private static readonly Color FatePurple = new Color(120, 30, 140);
        private static readonly Color FateWhite = new Color(255, 255, 255);
        
        public override string Texture => "Terraria/Images/Item_" + ItemID.StaffoftheFrostHydra;
        
        public override void SetDefaults()
        {
            Item.damage = 185;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 25;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 26);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item117;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<DestinyChoirSinger>();
            Item.buffType = ModContent.BuffType<DestinyChoirBuff>();
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Summons a formation of 3 Destiny Singers"));
            tooltips.Add(new TooltipLine(Mod, "FateEffect2", "Singers harmonize for bonus damage when all 3 attack together"));
            tooltips.Add(new TooltipLine(Mod, "FateLore", "'Their song is the universe's requiem'") 
            { 
                OverrideColor = FateDarkPink 
            });
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            
            int formationIndex = player.ownedProjectileCounts[type];
            
            // Spawn 3 singers in formation
            for (int i = 0; i < 3; i++)
            {
                float angle = MathHelper.TwoPi * i / 3f;
                Vector2 offset = angle.ToRotationVector2() * 20f;
                Projectile.NewProjectile(source, player.Center + offset, Vector2.Zero, type, damage, knockback, player.whoAmI, formationIndex, i);
            }
            
            // Grand summon VFX - Dark Prismatic choir conjuration
            CustomParticles.GenericFlare(player.Center, FateBlack, 1.0f, 20);
            CustomParticles.GenericFlare(player.Center, FateBrightRed, 0.85f, 18);
            CustomParticles.HaloRing(player.Center, FateDarkPink, 0.65f, 16);
            
            // Chromatic summon burst
            CustomParticles.GenericFlare(player.Center + new Vector2(-5, 0), Color.Red * 0.4f, 0.4f, 14);
            CustomParticles.GenericFlare(player.Center + new Vector2(5, 0), Color.Cyan * 0.4f, 0.4f, 14);
            
            // Choir summoning glyph circle - cosmic choir formation ENHANCED
            CustomParticles.GlyphCircle(player.Center, FateBrightRed, 12, 60f, 0.045f);
            CustomParticles.GlyphBurst(player.Center, FateDarkPink, 8, 4f);
            CustomParticles.GlyphTower(player.Center, FatePurple, 4, 0.45f);
            
            for (int ring = 0; ring < 4; ring++)
            {
                float ringProgress = ring / 4f;
                Color ringColor = Color.Lerp(FateBlack, FateBrightRed, ringProgress);
                CustomParticles.HaloRing(player.Center, ringColor * 0.75f, 0.32f + ring * 0.16f, 13 + ring * 3);
            }
            
            // Fractal burst for choir formation
            for (int i = 0; i < 9; i++)
            {
                float angle = MathHelper.TwoPi * i / 9f;
                Vector2 offset = angle.ToRotationVector2() * 40f;
                float progress = (float)i / 9f;
                Color burstColor = Color.Lerp(FateBlack, FateBrightRed, progress);
                CustomParticles.GenericFlare(player.Center + offset, burstColor, 0.38f, 14);
            }
            
            // CHOIR SUMMONING - Grand cosmic music note burst! The choir begins to sing!
            ThemedParticles.FateMusicNoteBurst(player.Center, 16, 7f);
            ThemedParticles.FateMusicNotes(player.Center, 12, 60f);
            
            return false;
        }
    }
    
    public class DestinyChoirBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.Confused;
        
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }
        
        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<DestinyChoirSinger>()] > 0)
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
    
    public class DestinyChoirSinger : ModProjectile
    {
        // Dark Prismatic color palette
        private static readonly Color FateBlack = new Color(15, 5, 20);
        private static readonly Color FateDarkPink = new Color(180, 50, 100);
        private static readonly Color FateBrightRed = new Color(255, 60, 80);
        private static readonly Color FatePurple = new Color(120, 30, 140);
        private static readonly Color FateWhite = new Color(255, 255, 255);
        
        private int FormationIndex => (int)Projectile.ai[0];
        private int SingerPosition => (int)Projectile.ai[1];
        private int AttackCooldown = 0;
        private static int LastHarmonyTime = 0;
        
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
            Projectile.width = 25;
            Projectile.height = 25;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.minionSlots = 0.33f;
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f + SingerPosition * 0.7f) * 0.15f + 1f;
            Color singerColor = GetFateGradient((float)SingerPosition / 3f);
            
            // Outer glow with singer color
            spriteBatch.Draw(texture, drawPos, null, singerColor * 0.35f, 0f, origin, 0.8f * pulse, SpriteEffects.None, 0f);
            // Middle layer
            spriteBatch.Draw(texture, drawPos, null, FateBrightRed * 0.5f, 0f, origin, 0.55f * pulse, SpriteEffects.None, 0f);
            // Inner core
            spriteBatch.Draw(texture, drawPos, null, FateWhite * 0.75f, 0f, origin, 0.3f * pulse, SpriteEffects.None, 0f);
            
            return false;
        }
        
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<DestinyChoirBuff>());
                return;
            }
            
            if (owner.HasBuff(ModContent.BuffType<DestinyChoirBuff>()))
            {
                Projectile.timeLeft = 2;
            }
            
            // Formation positioning
            float baseAngle = Main.GameUpdateCount * 0.025f + MathHelper.TwoPi * FormationIndex / 3f;
            float positionAngle = baseAngle + MathHelper.TwoPi * SingerPosition / 3f;
            float radius = 70f;
            float verticalOffset = (float)Math.Sin(Main.GameUpdateCount * 0.05f + SingerPosition * MathHelper.TwoPi / 3f) * 12f;
            
            Vector2 targetPos = owner.Center + new Vector2((float)Math.Cos(positionAngle) * radius, verticalOffset - 40f);
            Projectile.Center = Vector2.Lerp(Projectile.Center, targetPos, 0.08f);
            
            // Individual singer glow based on position - dark prismatic
            Color singerColor = GetFateGradient((float)SingerPosition / 3f);
            
            if (Main.GameUpdateCount % 3 == 0)
            {
                CustomParticles.GenericFlare(Projectile.Center, singerColor * 0.5f, 0.17f, 11);
            }
            
            // Chromatic singer aura
            if (Main.GameUpdateCount % 6 == 0)
            {
                CustomParticles.GenericFlare(Projectile.Center + new Vector2(-2, 0), Color.Red * 0.2f, 0.08f, 5);
                CustomParticles.GenericFlare(Projectile.Center + new Vector2(2, 0), Color.Cyan * 0.2f, 0.08f, 5);
            }
            
            // Connection lines between singers - enhanced with glyphs
            if (Main.GameUpdateCount % 6 == 0)
            {
                foreach (Projectile other in Main.ActiveProjectiles)
                {
                    if (other.type == Projectile.type && other.whoAmI != Projectile.whoAmI && other.owner == Projectile.owner)
                    {
                        if ((int)other.ai[0] == FormationIndex)
                        {
                            Vector2 midpoint = (Projectile.Center + other.Center) / 2f;
                            Color linkColor = singerColor * 0.4f;
                            CustomParticles.GenericFlare(midpoint, linkColor, 0.12f, 9);
                            
                            // Destiny glyph connection
                            if (Main.rand.NextBool(4))
                            {
                                CustomParticles.Glyph(midpoint, FateDarkPink * 0.5f, 0.18f, -1);
                            }
                        }
                    }
                }
            }
            
            // Attack logic
            AttackCooldown--;
            if (AttackCooldown <= 0)
            {
                NPC target = FindTarget();
                if (target != null)
                {
                    // Check for harmony - are all 3 singers attacking?
                    int singerCount = CountFormationSingers();
                    bool harmonyBonus = singerCount >= 3 && (Main.GameUpdateCount - LastHarmonyTime) < 20;
                    
                    FireNote(target, harmonyBonus);
                    AttackCooldown = 35;
                    LastHarmonyTime = (int)Main.GameUpdateCount;
                }
            }
            
            Lighting.AddLight(Projectile.Center, singerColor.ToVector3() * 0.3f);
        }
        
        private int CountFormationSingers()
        {
            int count = 0;
            foreach (Projectile proj in Main.ActiveProjectiles)
            {
                if (proj.type == Projectile.type && proj.owner == Projectile.owner && (int)proj.ai[0] == FormationIndex)
                {
                    count++;
                }
            }
            return count;
        }
        
        private NPC FindTarget()
        {
            float maxRange = 600f;
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
        
        private void FireNote(NPC target, bool harmonyBonus)
        {
            Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
            int damage = harmonyBonus ? (int)(Projectile.damage * 1.5f) : Projectile.damage;
            
            int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, direction * 14f,
                ModContent.ProjectileType<DestinyNote>(), damage, Projectile.knockBack, Projectile.owner, harmonyBonus ? 1 : 0);
            
            Color singerColor = GetFateGradient((float)SingerPosition / 3f);
            
            // Fire VFX - dark prismatic enhanced
            CustomParticles.GenericFlare(Projectile.Center, FateBlack, 0.5f, 12);
            CustomParticles.GenericFlare(Projectile.Center, singerColor * 0.75f, 0.45f, 11);
            
            // Destiny glyph on singer attack - enhanced
            CustomParticles.Glyph(Projectile.Center, singerColor, 0.36f, -1);
            
            // Chromatic note fire
            CustomParticles.GenericFlare(Projectile.Center + new Vector2(-2, 0), Color.Red * 0.25f, 0.15f, 7);
            CustomParticles.GenericFlare(Projectile.Center + new Vector2(2, 0), Color.Cyan * 0.25f, 0.15f, 7);
            
            // Choir sings with cosmic music notes!
            ThemedParticles.FateMusicNotes(Projectile.Center, 4, 25f);
            
            if (harmonyBonus)
            {
                // Harmony visual with extra glyphs - ENHANCED
                CustomParticles.HaloRing(Projectile.Center, FateBrightRed * 0.6f, 0.35f, 13);
                CustomParticles.GlyphCircle(Projectile.Center, FateBrightRed, 4, 28f, 0.05f);
                CustomParticles.GlyphTower(Projectile.Center, FatePurple, 2, 0.3f);
                
                // HARMONY BONUS - The choir sings in unison! Maximum cosmic music notes!
                ThemedParticles.FateMusicNoteBurst(Projectile.Center, 10, 5f);
            }
            
            SoundEngine.PlaySound(SoundID.Item26 with { Volume = 0.4f, Pitch = 0.2f + SingerPosition * 0.15f }, Projectile.Center);
        }
    }
    
    public class DestinyNote : ModProjectile
    {
        // Dark Prismatic color palette
        private static readonly Color FateBlack = new Color(15, 5, 20);
        private static readonly Color FateDarkPink = new Color(180, 50, 100);
        private static readonly Color FateBrightRed = new Color(255, 60, 80);
        private static readonly Color FatePurple = new Color(120, 30, 140);
        private static readonly Color FateWhite = new Color(255, 255, 255);
        
        private bool IsHarmony => Projectile.ai[0] > 0;
        
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
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
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
            float intensity = IsHarmony ? 1.15f : 0.7f;
            
            // Draw chromatic aberration trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                float trailAlpha = (1f - progress) * 0.5f * intensity;
                float trailScale = (0.4f - progress * 0.3f) * intensity;
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                
                // RGB separation for chromatic effect
                spriteBatch.Draw(texture, drawPos + new Vector2(-1.5f, 0), null, Color.Red * trailAlpha * 0.4f, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
                spriteBatch.Draw(texture, drawPos + new Vector2(1.5f, 0), null, Color.Cyan * trailAlpha * 0.4f, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
                
                Color trailColor = GetFateGradient(progress);
                spriteBatch.Draw(texture, drawPos, null, trailColor * trailAlpha, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }
            
            // Draw layered glow core
            Vector2 corePos = Projectile.Center - Main.screenPosition;
            spriteBatch.Draw(texture, corePos, null, FateDarkPink * 0.4f * intensity, Projectile.rotation, origin, 0.5f * intensity, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, corePos, null, FateBrightRed * 0.55f * intensity, Projectile.rotation, origin, 0.35f * intensity, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, corePos, null, FateWhite * 0.75f * intensity, Projectile.rotation, origin, 0.2f * intensity, SpriteEffects.None, 0f);
            
            return false;
        }
        
        public override void AI()
        {
            Projectile.rotation += 0.12f;
            
            float trailIntensity = IsHarmony ? 1.15f : 0.65f;
            float progress = Main.rand.NextFloat();
            Color trailColor = GetFateGradient(progress) * trailIntensity;
            
            if (Main.rand.NextBool(2))
            {
                CustomParticles.GenericFlare(Projectile.Center, trailColor * 0.55f, 0.17f, 11);
            }
            
            // Chromatic note trail
            if (Main.rand.NextBool(4))
            {
                CustomParticles.GenericFlare(Projectile.Center + new Vector2(-2, 0), Color.Red * 0.2f, 0.07f, 5);
                CustomParticles.GenericFlare(Projectile.Center + new Vector2(2, 0), Color.Cyan * 0.2f, 0.07f, 5);
            }
            
            // Harmony glow - enhanced with void core
            if (IsHarmony && Main.GameUpdateCount % 3 == 0)
            {
                CustomParticles.GenericFlare(Projectile.Center, FateBlack * 0.6f, 0.12f, 6);
                CustomParticles.GenericFlare(Projectile.Center, FateBrightRed * 0.45f, 0.22f, 9);
            }
            
            Lighting.AddLight(Projectile.Center, FateDarkPink.ToVector3() * 0.22f * trailIntensity);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            int stacks = IsHarmony ? 3 : 1;  // Enhanced harmony stacks
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);
            target.GetGlobalNPC<DestinyCollapseNPC>().AddStack(target, stacks);
            
            // === UNIFIED VFX HIT EFFECT - FATE THEME ===
            float vfxScale = IsHarmony ? 1.3f : 0.9f;
            UnifiedVFX.Fate.HitEffect(target.Center, vfxScale);
            
            // Hit VFX - dark prismatic enhanced
            float intensity = IsHarmony ? 1.35f : 0.85f;
            CustomParticles.GenericFlare(target.Center, FateBlack * 0.8f, 0.4f * intensity, 12);
            CustomParticles.GenericFlare(target.Center, FateBrightRed * 0.7f, 0.38f * intensity, 11);
            
            // === CHROMATIC ABERRATION ===
            CustomParticles.GenericFlare(target.Center + new Vector2(-3, 0), FateBrightRed * 0.4f, 0.3f, 12);
            CustomParticles.GenericFlare(target.Center + new Vector2(3, 0), FatePurple * 0.4f, 0.3f, 12);
            
            // === GLYPH FORMATIONS ===
            CustomParticles.GlyphImpact(target.Center, FateBlack, FateBrightRed, 0.55f);
            CustomParticles.GlyphCircle(target.Center, FateDarkPink, 5, 40f, 0.08f);
            
            // === TEMPORAL ECHO AFTERIMAGES ===
            for (int echo = 0; echo < 4; echo++)
            {
                Vector2 echoPos = target.Center + new Vector2(0, -echo * 10f);
                float echoAlpha = 1f - echo * 0.2f;
                CustomParticles.GenericFlare(echoPos, GetFateGradient((float)echo / 4f) * echoAlpha * 0.5f, 0.4f, 15);
            }
            
            // Cosmic glyph impact on note hits - enhanced
            if (IsHarmony)
            {
                CustomParticles.GlyphBurst(target.Center, FateDarkPink, 4, 2.5f);
                
                // Fractal harmony burst
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    Vector2 offset = angle.ToRotationVector2() * 18f;
                    Color hitColor = GetFateGradient((float)i / 6f);
                    CustomParticles.GenericFlare(target.Center + offset, hitColor * 0.55f, 0.28f, 11);
                }
                
                // Chromatic harmony impact
                CustomParticles.GenericFlare(target.Center + new Vector2(-4, 0), Color.Red * 0.35f, 0.22f, 9);
                CustomParticles.GenericFlare(target.Center + new Vector2(4, 0), Color.Cyan * 0.35f, 0.22f, 9);
            }
            
            // === COSMIC MUSIC NOTES ===
            ThemedParticles.FateMusicNoteBurst(target.Center, 10, 6f);
            ThemedParticles.FateMusicNotes(target.Center, IsHarmony ? 6 : 4, 35f);
            
            // Cosmic Revisit - choir echoes (stronger for harmony notes)
            float revisitMult = IsHarmony ? 0.30f : 0.20f;
            float revisitScale = IsHarmony ? 0.9f : 0.7f;
            int revisitDamage = (int)(damageDone * revisitMult);
            target.GetGlobalNPC<DestinyCollapseNPC>().QueueCosmicRevisit(target, revisitDamage, 25, Projectile.Center, revisitScale);
            
            Lighting.AddLight(target.Center, FateBrightRed.ToVector3() * 0.9f);
        }
        
        public override void OnKill(int timeLeft)
        {
            float intensity = IsHarmony ? 1.15f : 0.65f;
            
            // Void core burst
            CustomParticles.GenericFlare(Projectile.Center, FateBlack, 0.25f * intensity, 10);
            
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 vel = angle.ToRotationVector2() * 2.5f;
                Color burstColor = GetFateGradient((float)i / 5f);
                var glow = new GenericGlowParticle(Projectile.Center, vel, burstColor * intensity, 0.28f, 13, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Chromatic death flash
            CustomParticles.GenericFlare(Projectile.Center + new Vector2(-2, 0), Color.Red * 0.2f * intensity, 0.1f, 6);
            CustomParticles.GenericFlare(Projectile.Center + new Vector2(2, 0), Color.Cyan * 0.2f * intensity, 0.1f, 6);
        }
    }
}
