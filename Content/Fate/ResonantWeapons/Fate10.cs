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
    /// Fate Wisp Conductor - Summons orbiting cosmic wisps
    /// </summary>
    public class Fate10 : ModItem
    {
        // Dark Prismatic color palette
        private static readonly Color FateBlack = new Color(15, 5, 20);
        private static readonly Color FateDarkPink = new Color(180, 50, 100);
        private static readonly Color FateBrightRed = new Color(255, 60, 80);
        private static readonly Color FatePurple = new Color(120, 30, 140);
        private static readonly Color FateWhite = new Color(255, 255, 255);
        
        public override string Texture => "Terraria/Images/Item_" + ItemID.StardustDragonStaff;
        
        public override void SetDefaults()
        {
            Item.damage = 195;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 15;
            Item.width = 44;
            Item.height = 44;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(gold: 25);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item117;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<CosmicFateWisp>();
            Item.buffType = ModContent.BuffType<CosmicFateWispBuff>();
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Summons a Cosmic Wisp that orbits around you"));
            tooltips.Add(new TooltipLine(Mod, "FateEffect2", "The wisp fires reality-piercing beams at enemies"));
            tooltips.Add(new TooltipLine(Mod, "FateLore", "'A fragment of a star's dying breath'") 
            { 
                OverrideColor = FateDarkPink 
            });
        }
        
        public override void HoldItem(Player player)
        {
            // === UNIQUE: CONDUCTOR'S BATON AURA ===
            // The staff glows with conductor's energy, commanding the wisps
            
            Vector2 staffTip = player.Center + new Vector2(player.direction * 28f, -10f);
            float conductTime = Main.GameUpdateCount * 0.03f;
            
            // === CONDUCTOR'S BATON GLOW ===
            // The tip of the staff has a pulsing conductor's light
            float pulse = (float)Math.Sin(conductTime * 3f) * 0.2f + 0.8f;
            if (Main.rand.NextBool(5))
            {
                CustomParticles.GenericFlare(staffTip, FateBrightRed * pulse, 0.25f, 8);
                CustomParticles.GenericFlare(staffTip, FateDarkPink * (1f - pulse * 0.3f), 0.18f, 6);
            }
            
            // === MUSICAL STAFF LINES ===
            // Faint horizontal lines like a musical staff
            if (Main.GameUpdateCount % 20 == 0)
            {
                for (int line = 0; line < 5; line++)
                {
                    float yOffset = -15f + line * 8f;
                    Vector2 linePos = staffTip + new Vector2(Main.rand.NextFloat(-15f, 15f), yOffset);
                    Color lineColor = Color.Lerp(FatePurple, FateDarkPink, line / 4f) * 0.4f;
                    CustomParticles.GenericFlare(linePos, lineColor, 0.1f, 10);
                }
            }
            
            // === CONNECTION TO ACTIVE WISPS ===
            // Draw faint energy to player's summoned wisps
            if (Main.rand.NextBool(12))
            {
                // Particles drift outward toward where wisps would be
                float angle = conductTime + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 driftVel = angle.ToRotationVector2() * 2f;
                var drift = new GenericGlowParticle(staffTip, driftVel, FateDarkPink * 0.5f, 0.1f, 18, true);
                MagnumParticleHandler.SpawnParticle(drift);
            }
            
            // === MUSIC NOTES - The conductor's melody ===
            if (Main.rand.NextBool(20))
            {
                ThemedParticles.FateMusicNotes(staffTip, 1, 18f);
            }
            
            // Conductor's light
            Lighting.AddLight(staffTip, FateDarkPink.ToVector3() * 0.35f * pulse);
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            
            int wispCount = player.ownedProjectileCounts[ModContent.ProjectileType<CosmicFateWisp>()];
            Projectile.NewProjectile(source, player.Center, Vector2.Zero, type, damage, knockback, player.whoAmI, wispCount);
            
            // Dark prismatic summon VFX with chromatic aberration
            CustomParticles.GenericFlare(player.Center, FateBlack, 0.8f, 18);
            CustomParticles.GenericFlare(player.Center, FateBrightRed, 0.7f, 16);
            CustomParticles.HaloRing(player.Center, FateDarkPink, 0.55f, 14);
            
            // Chromatic summon flash
            CustomParticles.GenericFlare(player.Center + new Vector2(-4, 0), Color.Red * 0.4f, 0.35f, 12);
            CustomParticles.GenericFlare(player.Center + new Vector2(4, 0), Color.Cyan * 0.4f, 0.35f, 12);
            
            // Summon glyph circle - cosmic wisp conjuration enhanced
            CustomParticles.GlyphCircle(player.Center, FateBrightRed, 8, 50f, 0.04f);
            CustomParticles.GlyphTower(player.Center, FatePurple, 3, 0.4f);
            
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 offset = angle.ToRotationVector2() * 30f;
                float progress = (float)i / 8f;
                Color burstColor = Color.Lerp(FateBlack, FateBrightRed, progress);
                CustomParticles.GenericFlare(player.Center + offset, burstColor, 0.4f, 14);
            }
            
            // CONDUCTOR SUMMONING - Cosmic symphony begins! Music notes burst forth!
            ThemedParticles.FateMusicNoteBurst(player.Center, 12, 6f);
            ThemedParticles.FateMusicNotes(player.Center, 10, 50f);
            
            return false;
        }
    }
    
    public class CosmicFateWispBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.Confused;
        
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }
        
        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<CosmicFateWisp>()] > 0)
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
    
    public class CosmicFateWisp : ModProjectile
    {
        // Dark Prismatic color palette
        private static readonly Color FateBlack = new Color(15, 5, 20);
        private static readonly Color FateDarkPink = new Color(180, 50, 100);
        private static readonly Color FateBrightRed = new Color(255, 60, 80);
        private static readonly Color FatePurple = new Color(120, 30, 140);
        private static readonly Color FateWhite = new Color(255, 255, 255);
        
        private int OrbitIndex => (int)Projectile.ai[0];
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
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.minionSlots = 1f;
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f + OrbitIndex * 0.7f) * 0.15f + 1f;
            
            // Outer glow
            spriteBatch.Draw(texture, drawPos, null, FateDarkPink * 0.35f, 0f, origin, 0.9f * pulse, SpriteEffects.None, 0f);
            // Middle layer
            spriteBatch.Draw(texture, drawPos, null, FateBrightRed * 0.5f, 0f, origin, 0.6f * pulse, SpriteEffects.None, 0f);
            // Inner core
            spriteBatch.Draw(texture, drawPos, null, FateWhite * 0.75f, 0f, origin, 0.35f * pulse, SpriteEffects.None, 0f);
            
            return false;
        }
        
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<CosmicFateWispBuff>());
                return;
            }
            
            if (owner.HasBuff(ModContent.BuffType<CosmicFateWispBuff>()))
            {
                Projectile.timeLeft = 2;
            }
            
            // Orbit around player
            float orbitSpeed = 0.04f;
            float orbitRadius = 80f + OrbitIndex * 25f;
            float angle = Main.GameUpdateCount * orbitSpeed + MathHelper.TwoPi * OrbitIndex / 5f;
            float verticalOffset = (float)Math.Sin(Main.GameUpdateCount * 0.06f + OrbitIndex) * 15f;
            
            Vector2 targetPos = owner.Center + new Vector2((float)Math.Cos(angle) * orbitRadius, verticalOffset);
            Projectile.Center = Vector2.Lerp(Projectile.Center, targetPos, 0.1f);
            
            Projectile.rotation = angle + MathHelper.PiOver2;
            
            // Dark prismatic visual trail
            if (Main.GameUpdateCount % 3 == 0)
            {
                float trailProgress = ((int)Main.GameUpdateCount * 0.02f) % 1f;
                Color trailColor = GetFateGradient(trailProgress);
                CustomParticles.GenericFlare(Projectile.Center, trailColor * 0.55f, 0.24f, 14);
            }
            
            // Chromatic aberration orbit effect
            if (Main.GameUpdateCount % 5 == 0)
            {
                CustomParticles.GenericFlare(Projectile.Center + new Vector2(-2, 0), Color.Red * 0.25f, 0.1f, 6);
                CustomParticles.GenericFlare(Projectile.Center + new Vector2(2, 0), Color.Cyan * 0.25f, 0.1f, 6);
            }
            
            // Orbiting particles around wisp with dark prismatic gradient
            if (Main.GameUpdateCount % 5 == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    float particleAngle = Main.GameUpdateCount * 0.18f + i * MathHelper.Pi;
                    Vector2 particlePos = Projectile.Center + particleAngle.ToRotationVector2() * 14f;
                    Color particleColor = GetFateGradient((float)i / 3f);
                    CustomParticles.GenericFlare(particlePos, particleColor * 0.45f, 0.12f, 9);
                }
            }
            
            // Cosmic glyph aura around the wisp - enhanced
            if (Main.GameUpdateCount % 20 == 0)
            {
                CustomParticles.GlyphAura(Projectile.Center, FateBrightRed * 0.55f, 24f, 2);
            }
            
            // Central void glow
            if (Main.GameUpdateCount % 8 == 0)
            {
                CustomParticles.GenericFlare(Projectile.Center, FateBlack, 0.25f, 10);
            }
            
            // Attack logic
            AttackCooldown--;
            if (AttackCooldown <= 0)
            {
                NPC target = FindTarget();
                if (target != null)
                {
                    FireBeam(target);
                    AttackCooldown = 45;
                }
            }
            
            Lighting.AddLight(Projectile.Center, FateDarkPink.ToVector3() * 0.4f);
        }
        
        private NPC FindTarget()
        {
            float maxRange = 500f;
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
        
        private void FireBeam(NPC target)
        {
            Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, direction * 18f,
                ModContent.ProjectileType<FateWispBeam>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            
            // Dark prismatic fire VFX
            CustomParticles.GenericFlare(Projectile.Center, FateBlack, 0.55f, 14);
            CustomParticles.GenericFlare(Projectile.Center, FateBrightRed * 0.85f, 0.5f, 13);
            CustomParticles.HaloRing(Projectile.Center, FateDarkPink * 0.55f, 0.28f, 10);
            
            // Destiny glyph on wisp attack - enhanced
            CustomParticles.Glyph(Projectile.Center, FateBrightRed, 0.4f, -1);
            
            // Chromatic fire flash
            CustomParticles.GenericFlare(Projectile.Center + new Vector2(-3, 0), Color.Red * 0.3f, 0.2f, 8);
            CustomParticles.GenericFlare(Projectile.Center + new Vector2(3, 0), Color.Cyan * 0.3f, 0.2f, 8);
            
            // Conductor directs with cosmic music notes!
            ThemedParticles.FateMusicNotes(Projectile.Center, 5, 30f);
            
            SoundEngine.PlaySound(SoundID.Item75 with { Volume = 0.5f, Pitch = 0.3f }, Projectile.Center);
        }
    }
    
    public class FateWispBeam : ModProjectile
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
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 3;
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
                float trailScale = 0.35f - progress * 0.25f;
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                
                // RGB separation for chromatic effect
                spriteBatch.Draw(texture, drawPos + new Vector2(-1.5f, 0), null, Color.Red * trailAlpha * 0.4f, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
                spriteBatch.Draw(texture, drawPos + new Vector2(1.5f, 0), null, Color.Cyan * trailAlpha * 0.4f, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
                
                Color trailColor = GetFateGradient(progress);
                spriteBatch.Draw(texture, drawPos, null, trailColor * trailAlpha, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }
            
            // Draw layered glow core
            Vector2 corePos = Projectile.Center - Main.screenPosition;
            spriteBatch.Draw(texture, corePos, null, FateDarkPink * 0.4f, Projectile.rotation, origin, 0.45f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, corePos, null, FateBrightRed * 0.55f, Projectile.rotation, origin, 0.3f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, corePos, null, FateWhite * 0.75f, Projectile.rotation, origin, 0.18f, SpriteEffects.None, 0f);
            
            return false;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Dark prismatic reality-tearing trail
            if (Main.rand.NextBool(2))
            {
                float progress = Main.rand.NextFloat();
                Color trailColor = GetFateGradient(progress);
                CustomParticles.GenericFlare(Projectile.Center, trailColor * 0.55f, 0.15f, 10);
            }
            
            // Chromatic trail
            if (Main.rand.NextBool(4))
            {
                CustomParticles.GenericFlare(Projectile.Center + new Vector2(-2, 0), Color.Red * 0.25f, 0.08f, 5);
                CustomParticles.GenericFlare(Projectile.Center + new Vector2(2, 0), Color.Cyan * 0.25f, 0.08f, 5);
            }
            
            Lighting.AddLight(Projectile.Center, FateDarkPink.ToVector3() * 0.25f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);
            target.GetGlobalNPC<DestinyCollapseNPC>().AddStack(target, 1);
            
            // === UNIFIED VFX HIT EFFECT - FATE THEME ===
            UnifiedVFX.Fate.HitEffect(target.Center, 0.9f);
            
            // Dark prismatic hit
            CustomParticles.GenericFlare(target.Center, FateBlack, 0.4f, 12);
            CustomParticles.GenericFlare(target.Center, FateBrightRed * 0.7f, 0.35f, 11);
            
            // Destiny glyph on beam impact
            if (Main.rand.NextBool(2))
            {
                CustomParticles.Glyph(target.Center, FateDarkPink, 0.3f, -1);
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
            
            // Cosmic Revisit - wisp beam echoes
            int revisitDamage = (int)(damageDone * 0.20f);
            target.GetGlobalNPC<DestinyCollapseNPC>().QueueCosmicRevisit(target, revisitDamage, 25, Projectile.Center, 0.7f);
            
            Lighting.AddLight(target.Center, FateBrightRed.ToVector3() * 0.9f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // Dark prismatic death burst
            CustomParticles.GenericFlare(Projectile.Center, FateBlack, 0.3f, 12);
            
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 vel = angle.ToRotationVector2() * 2.5f;
                Color burstColor = GetFateGradient((float)i / 5f);
                var glow = new GenericGlowParticle(Projectile.Center, vel, burstColor, 0.24f, 12, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Chromatic flash
            CustomParticles.GenericFlare(Projectile.Center + new Vector2(-2, 0), Color.Red * 0.22f, 0.1f, 7);
            CustomParticles.GenericFlare(Projectile.Center + new Vector2(2, 0), Color.Cyan * 0.22f, 0.1f, 7);
        }
    }
}
