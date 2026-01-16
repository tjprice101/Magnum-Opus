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
    /// MYSTERY REPEATER - Enigma Ranged Gun
    /// ====================================
    /// UNIQUE MECHANICS:
    /// - Fire bullets that split into 3 HOMING seekers on first enemy hit
    /// - Each seeker tracks different enemies with heavy particle trails
    /// - On final impact: creates "?" shaped particle explosion
    /// - Chain lightning connects ALL hit enemies with eerie green beams
    /// - Every 5th bullet fired is a PARADOX BOLT that pierces infinitely and chains on every hit
    /// - Mysterious eyes watch from bullet trails and at impact points
    /// </summary>
    public class Enigma2 : ModItem
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private int shotCounter = 0;
        
        public override string Texture => "Terraria/Images/Item_" + ItemID.Xenopopper;
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override void SetDefaults()
        {
            Item.damage = 235;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 40;
            Item.height = 26;
            Item.useTime = 14;
            Item.useAnimation = 14;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 18);
            Item.rare = ModContent.RarityType<EnigmaRarity>();
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.Bullet;
            Item.shootSpeed = 16f;
            Item.useAmmo = AmmoID.Bullet;
            Item.noMelee = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect1", "Bullets split into 3 homing seekers on first hit"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect2", "Hit enemies are connected by chain lightning"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect3", "Every 5th bullet is a paradox bolt that pierces infinitely"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaLore", "'Each shot poses a question that tears at the fabric of reality'") 
            { 
                OverrideColor = EnigmaPurple 
            });
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            shotCounter++;
            
            bool isParadoxArrow = shotCounter >= 5;
            if (isParadoxArrow)
                shotCounter = 0;
            
            // Spawn main bullet
            int projType = isParadoxArrow ? 
                ModContent.ProjectileType<ParadoxPiercingBolt>() : 
                ModContent.ProjectileType<QuestionSeekerBolt>();
            
            Projectile.NewProjectile(source, position, velocity, projType, damage, knockback, player.whoAmI);
            
            // Muzzle flash with gradient
            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.Zero) * 25f;
            
            if (isParadoxArrow)
            {
                // Enhanced muzzle flash for paradox arrow
                CustomParticles.GenericFlare(muzzlePos, Color.White, 0.8f, 18);
                CustomParticles.GenericFlare(muzzlePos, EnigmaGreen, 0.65f, 16);
                
                for (int ring = 0; ring < 3; ring++)
                {
                    CustomParticles.HaloRing(muzzlePos, GetEnigmaGradient(ring / 3f), 0.35f + ring * 0.15f, 14 + ring * 3);
                }
                
                CustomParticles.GlyphBurst(muzzlePos, EnigmaPurple, count: 6, speed: 4f);
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.5f, Volume = 0.6f }, muzzlePos);
            }
            else
            {
                // Normal muzzle flash
                for (int i = 0; i < 6; i++)
                {
                    float angle = velocity.ToRotation() + MathHelper.PiOver4 * ((float)i / 6f - 0.5f);
                    Vector2 sparkVel = angle.ToRotationVector2() * 5f;
                    float progress = (float)i / 6f;
                    Color sparkColor = GetEnigmaGradient(progress);
                    var glow = new GenericGlowParticle(muzzlePos, sparkVel, sparkColor, 0.32f, 14, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
                
                CustomParticles.GenericFlare(muzzlePos, EnigmaPurple, 0.55f, 15);
                CustomParticles.HaloRing(muzzlePos, EnigmaGreen * 0.65f, 0.28f, 13);
                
                // Music notes at muzzle - each shot is a note in the mystery
                ThemedParticles.EnigmaMusicNotes(muzzlePos, 4, 25f);
            }
            
            return false;
        }
    }
    
    /// <summary>
    /// Main bullet that splits on first hit
    /// </summary>
    public class QuestionSeekerBolt : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreenFlame = new Color(50, 220, 100);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private bool hasSplit = false;
        private static List<int> recentlyHitEnemies = new List<int>();
        
        // Eye texture index for mysterious watching effect
        private int eyeTextureIndex = 0;
        
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare";
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            
            // Load sparkle and flare textures for dazzling trail
            Texture2D flareTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Texture2D sparkleTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/PrismaticSparkle" + (1 + (Projectile.whoAmI % 8))).Value;
            Texture2D eyeTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnigmaEye" + (1 + eyeTextureIndex)).Value;
            
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 flareOrigin = flareTex.Size() / 2f;
            Vector2 sparkleOrigin = sparkleTex.Size() / 2f;
            Vector2 eyeOrigin = eyeTex.Size() / 2f;
            
            // Switch to additive blending for sparkly magical look
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // === SPARKLE TRAIL - Dazzling magical sparkles behind the bullet ===
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float trailProgress = (float)i / Projectile.oldPos.Length;
                float trailAlpha = (1f - trailProgress) * 0.85f;
                float trailScale = (1f - trailProgress * 0.5f) * 0.45f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                
                // Gradient color along trail
                Color trailColor = Color.Lerp(EnigmaGreenFlame, EnigmaPurple, trailProgress);
                
                // Draw sparkle at each trail position
                float sparkleRot = Main.GameUpdateCount * 0.15f + i * 0.5f;
                spriteBatch.Draw(sparkleTex, trailPos, null, trailColor * trailAlpha, sparkleRot, sparkleOrigin, trailScale, SpriteEffects.None, 0f);
                
                // Additional flare sparkles offset for more density
                if (i % 2 == 0)
                {
                    Vector2 offsetPos = trailPos + new Vector2(MathF.Sin(i * 0.7f) * 6f, MathF.Cos(i * 0.7f) * 6f);
                    spriteBatch.Draw(flareTex, offsetPos, null, EnigmaGreen * trailAlpha * 0.6f, 0f, flareOrigin, trailScale * 0.5f, SpriteEffects.None, 0f);
                }
            }
            
            // === MYSTERIOUS EYE - Watching from within the bullet ===
            float eyePulse = 0.8f + MathF.Sin(Main.GameUpdateCount * 0.1f) * 0.2f;
            float eyeRot = Projectile.velocity.ToRotation();
            spriteBatch.Draw(eyeTex, drawPos, null, EnigmaPurple * 0.6f, eyeRot, eyeOrigin, 0.35f * eyePulse, SpriteEffects.None, 0f);
            
            // === CORE FLARE - Bright energetic center ===
            float pulse = 0.9f + MathF.Sin(Main.GameUpdateCount * 0.2f) * 0.15f;
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaGreen * 0.9f, 0f, flareOrigin, 0.5f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaPurple * 0.7f, 0f, flareOrigin, 0.35f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, Color.White * 0.8f, 0f, flareOrigin, 0.2f * pulse, SpriteEffects.None, 0f);
            
            // === RADIAL SPARKLE BURST around core ===
            for (int i = 0; i < 4; i++)
            {
                float angle = Main.GameUpdateCount * 0.08f + i * MathHelper.PiOver2;
                Vector2 sparkleOffset = angle.ToRotationVector2() * (12f + MathF.Sin(Main.GameUpdateCount * 0.15f + i) * 4f);
                Color sparkleColor = Color.Lerp(EnigmaGreen, EnigmaPurple, (float)i / 4f);
                spriteBatch.Draw(sparkleTex, drawPos + sparkleOffset, null, sparkleColor * 0.7f, angle, sparkleOrigin, 0.25f, SpriteEffects.None, 0f);
            }
            
            // Restore normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Cycle eye texture for mysterious effect
            if (Projectile.timeLeft % 30 == 0)
                eyeTextureIndex = Main.rand.Next(8);
            
            // === HEAVY SPARKLE DUST TRAIL ===
            // Constant dust flow like Swan Lake weapons
            for (int d = 0; d < 2; d++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), 
                    DustID.PurpleTorch, -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f));
                dust.noGravity = true;
                dust.scale = 1.2f + Main.rand.NextFloat(0.4f);
            }
            
            // Green flame sparkle dust
            if (Main.rand.NextBool(2))
            {
                Dust greenDust = Dust.NewDustPerfect(Projectile.Center, DustID.GreenTorch, 
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(2f, 2f));
                greenDust.noGravity = true;
                greenDust.scale = 1.0f + Main.rand.NextFloat(0.3f);
            }
            
            // Particle flares along trail
            if (Main.rand.NextBool(3))
            {
                float progress = Main.rand.NextFloat();
                Color trailColor = GetEnigmaGradient(progress);
                CustomParticles.GenericFlare(Projectile.Center, trailColor * 0.75f, 0.32f, 15);
                
                var glow = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f,
                    trailColor * 0.55f, 0.24f, 12, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Shimmering sparkle bursts
            if (Projectile.timeLeft % 12 == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    float offsetAngle = (i - 1) * 0.5f;
                    Vector2 trailVel = -Projectile.velocity.RotatedBy(offsetAngle) * 0.12f;
                    var sparkle = new GenericGlowParticle(Projectile.Center, trailVel + Main.rand.NextVector2Circular(1f, 1f),
                        GetEnigmaGradient((float)i / 3f) * 0.8f, 0.32f, 18, true);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }
            
            // Periodic glyph
            if (Projectile.timeLeft % 20 == 0)
            {
                CustomParticles.GlyphTrail(Projectile.Center, Projectile.velocity, EnigmaPurple, 0.25f);
            }
            
            // Music notes trailing
            if (Projectile.timeLeft % 15 == 0)
            {
                ThemedParticles.MusicNotes(Projectile.Center, GetEnigmaGradient(0.6f), 1, 10f);
            }
            
            Lighting.AddLight(Projectile.Center, EnigmaPurple.ToVector3() * 0.6f);
            Lighting.AddLight(Projectile.Center, EnigmaGreen.ToVector3() * 0.3f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 300);
            var brandNPC = target.GetGlobalNPC<ParadoxBrandNPC>();
            brandNPC.AddParadoxStack(target, 1);
            
            // Track hit enemies for chain lightning
            if (!recentlyHitEnemies.Contains(target.whoAmI))
                recentlyHitEnemies.Add(target.whoAmI);
            
            if (recentlyHitEnemies.Count > 10)
                recentlyHitEnemies.RemoveAt(0);
            
            // SPLIT on first hit!
            if (!hasSplit)
            {
                hasSplit = true;
                SplitIntoSeekers(target);
            }
            
            // Chain lightning to other recently hit enemies
            DrawChainLightning(target);
            
            // === REALITY WARP DISTORTION ===
            FateRealityDistortion.TriggerChromaticAberration(target.Center, 3f, 10);
            
            // === NEW UNIFIED VFX HIT EFFECT ===
            UnifiedVFX.EnigmaVariations.HitEffect(target.Center, 1.2f);
            
            // Impact VFX - "?" shaped explosion
            CreateQuestionMarkExplosion(target.Center);
            
            // === WATCHING EYE AT IMPACT ===
            CustomParticles.EnigmaEyeImpact(target.Center, target.Center, EnigmaGreen, 0.5f);
            
            // === MUSIC NOTES BURST ===
            ThemedParticles.EnigmaMusicNoteBurst(target.Center, 10, 6f);
            ThemedParticles.EnigmaMusicNotes(target.Center, 5, 35f);
            
            // Ascending sparkle plume at impact point
            for (int i = 0; i < 4; i++)
            {
                Vector2 riseVel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), -3f - i * 0.8f);
                var plume = new GenericGlowParticle(target.Center - new Vector2(0, 20f), riseVel,
                    GetEnigmaGradient((float)i / 4f), 0.35f + i * 0.05f, 22, true);
                MagnumParticleHandler.SpawnParticle(plume);
            }
            CustomParticles.HaloRing(target.Center, EnigmaGreen * 0.8f, 0.4f, 15);
            
            // === GLYPH CIRCLE FORMATION ===
            CustomParticles.GlyphCircle(target.Center, EnigmaPurple, count: 6, radius: 45f, rotationSpeed: 0.06f);
            
            // Glyph stacks visualization
            int stacks = brandNPC.paradoxStacks;
            if (stacks > 0)
            {
                CustomParticles.GlyphStack(target.Center + new Vector2(0, -25f), EnigmaPurple, stacks, 0.28f);
            }
            
            // === DYNAMIC LIGHTING ===
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.8f);
        }
        
        private void SplitIntoSeekers(NPC hitTarget)
        {
            SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.5f, Volume = 0.7f }, Projectile.Center);
            
            // Split VFX
            CustomParticles.GenericFlare(Projectile.Center, Color.White, 0.8f, 20);
            CustomParticles.HaloRing(Projectile.Center, EnigmaPurple, 0.45f, 16);
            CustomParticles.GlyphBurst(Projectile.Center, EnigmaGreen, 6, 4f);
            
            // Radial sparkle burst - the arrow shatters into light
            for (int i = 0; i < 8; i++)
            {
                float burstAngle = MathHelper.TwoPi * i / 8f;
                Vector2 burstVel = burstAngle.ToRotationVector2() * 4.5f;
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, 
                    GetEnigmaGradient((float)i / 8f), 0.38f, 18, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
            ThemedParticles.EnigmaMusicNoteBurst(Projectile.Center, 5, 3f);
            
            // Find 3 different targets
            List<NPC> targets = FindNearestEnemies(hitTarget.Center, 500f, 3, hitTarget.whoAmI);
            
            float baseAngle = Projectile.velocity.ToRotation();
            float[] offsets = { -MathHelper.Pi / 4f, 0f, MathHelper.Pi / 4f };
            
            for (int i = 0; i < 3; i++)
            {
                int targetIdx = i < targets.Count ? targets[i].whoAmI : -1;
                Vector2 newVel = (baseAngle + offsets[i]).ToRotationVector2() * 12f;
                
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, newVel,
                    ModContent.ProjectileType<HomingQuestionSeeker>(), Projectile.damage * 2 / 3, Projectile.knockBack * 0.6f, 
                    Projectile.owner, ai0: targetIdx);
                
                // Spawn trail for each seeker direction
                for (int j = 0; j < 4; j++)
                {
                    Vector2 trailPos = Projectile.Center + newVel.SafeNormalize(Vector2.Zero) * (j * 12f);
                    CustomParticles.GenericFlare(trailPos, GetEnigmaGradient((float)i / 3f), 0.35f, 12);
                }
            }
        }
        
        private List<NPC> FindNearestEnemies(Vector2 pos, float range, int count, int excludeIndex)
        {
            List<NPC> found = new List<NPC>();
            List<(NPC npc, float dist)> candidates = new List<(NPC, float)>();
            
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly || npc.whoAmI == excludeIndex) continue;
                float dist = Vector2.Distance(pos, npc.Center);
                if (dist <= range)
                    candidates.Add((npc, dist));
            }
            
            candidates.Sort((a, b) => a.dist.CompareTo(b.dist));
            
            for (int i = 0; i < Math.Min(count, candidates.Count); i++)
                found.Add(candidates[i].npc);
            
            return found;
        }
        
        private void DrawChainLightning(NPC target)
        {
            foreach (int npcIndex in recentlyHitEnemies)
            {
                if (npcIndex == target.whoAmI) continue;
                if (npcIndex < 0 || npcIndex >= Main.maxNPCs) continue;
                
                NPC other = Main.npc[npcIndex];
                if (!other.active || other.friendly) continue;
                
                float dist = Vector2.Distance(target.Center, other.Center);
                if (dist > 350f) continue;
                
                // Draw lightning beam between them
                MagnumVFX.DrawFractalLightning(target.Center, other.Center, EnigmaGreen, 12, 30f, 4, 0.35f);
                
                // Deal bonus chain damage
                other.SimpleStrikeNPC(Projectile.damage / 3, 0, false, 3f);
                other.AddBuff(ModContent.BuffType<ParadoxBrand>(), 180);
                
                // Lightning sparkle burst at chained target
                CustomParticles.GenericFlare(other.Center, EnigmaGreen, 0.5f, 16);
                CustomParticles.HaloRing(other.Center, EnigmaPurple * 0.6f, 0.3f, 12);
                var chainSparkle = new GenericGlowParticle(other.Center + new Vector2(0, -20f), 
                    new Vector2(0, -2f), GetEnigmaGradient(0.7f), 0.32f, 18, true);
                MagnumParticleHandler.SpawnParticle(chainSparkle);
            }
        }
        
        private void CreateQuestionMarkExplosion(Vector2 position)
        {
            // Dot of the "?"
            CustomParticles.GenericFlare(position + new Vector2(0, 28f), EnigmaGreen, 0.7f, 22);
            
            // Curve of the "?" with gradient
            for (int i = 0; i < 14; i++)
            {
                float t = (float)i / 14f;
                float curveAngle = MathHelper.Pi * 1.4f * t - MathHelper.Pi * 0.55f;
                float curveRadius = 22f - t * 10f;
                float yOffset = -12f - t * 35f;
                
                Vector2 curvePos = position + new Vector2((float)Math.Cos(curveAngle) * curveRadius, yOffset);
                CustomParticles.GenericFlare(curvePos, GetEnigmaGradient(t), 0.45f - t * 0.12f, 20);
            }
            
            // Burst around
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 offset = angle.ToRotationVector2() * 35f;
                CustomParticles.GenericFlare(position + offset, GetEnigmaGradient((float)i / 10f), 0.45f, 18);
            }
            
            CustomParticles.HaloRing(position, EnigmaPurple, 0.5f, 20);
            CustomParticles.GlyphCircle(position, EnigmaGreen, 6, 40f, 0.05f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // === REALITY WARP ON DEATH ===
            FateRealityDistortion.TriggerChromaticAberration(Projectile.Center, 3.5f, 12);
            
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * 4.5f;
                var glow = new GenericGlowParticle(Projectile.Center, vel, GetEnigmaGradient((float)i / 8f), 0.32f, 16, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            CustomParticles.HaloRing(Projectile.Center, EnigmaPurple * 0.7f, 0.38f, 15);
            
            // === WATCHING EYES burst outward ===
            CustomParticles.EnigmaEyeExplosion(Projectile.Center, EnigmaGreen, 4, 3f);
        }
    }
    
    /// <summary>
    /// Homing seeker that tracks targets - SPARKLE MISSILE
    /// </summary>
    public class HomingQuestionSeeker : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreenFlame = new Color(50, 220, 100);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private const float HomingStrength = 0.22f;
        private const float MaxSpeed = 18f;
        
        private int TargetIndex => (int)Projectile.ai[0];
        private int eyeTextureIndex = 0;
        
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare";
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 14;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            
            // Load sparkle textures for dazzling trail
            Texture2D flareTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Texture2D sparkleTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/PrismaticSparkle" + (1 + (Projectile.whoAmI % 8))).Value;
            Texture2D eyeTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnigmaEye" + (1 + eyeTextureIndex)).Value;
            
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 flareOrigin = flareTex.Size() / 2f;
            Vector2 sparkleOrigin = sparkleTex.Size() / 2f;
            Vector2 eyeOrigin = eyeTex.Size() / 2f;
            
            // Switch to additive blending for magical sparkles
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // === SPARKLE TRAIL ===
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float trailProgress = (float)i / Projectile.oldPos.Length;
                float trailAlpha = (1f - trailProgress) * 0.9f;
                float trailScale = (1f - trailProgress * 0.5f) * 0.35f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                
                Color trailColor = Color.Lerp(EnigmaGreenFlame, EnigmaPurple, trailProgress);
                float sparkleRot = Main.GameUpdateCount * 0.12f + i * 0.4f;
                spriteBatch.Draw(sparkleTex, trailPos, null, trailColor * trailAlpha, sparkleRot, sparkleOrigin, trailScale, SpriteEffects.None, 0f);
            }
            
            // === EYE watching from seeker ===
            float eyePulse = 0.7f + MathF.Sin(Main.GameUpdateCount * 0.12f) * 0.15f;
            float eyeRot = Projectile.velocity.ToRotation();
            spriteBatch.Draw(eyeTex, drawPos, null, EnigmaGreen * 0.7f, eyeRot, eyeOrigin, 0.3f * eyePulse, SpriteEffects.None, 0f);
            
            // === CORE FLARE ===
            float pulse = 0.85f + MathF.Sin(Main.GameUpdateCount * 0.18f) * 0.15f;
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaPurple * 0.85f, 0f, flareOrigin, 0.4f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaGreen * 0.7f, 0f, flareOrigin, 0.28f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, Color.White * 0.75f, 0f, flareOrigin, 0.15f * pulse, SpriteEffects.None, 0f);
            
            // Restore normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 200;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Cycle eye texture
            if (Projectile.timeLeft % 25 == 0)
                eyeTextureIndex = Main.rand.Next(8);
            
            // Aggressive homing
            NPC target = null;
            if (TargetIndex >= 0 && TargetIndex < Main.maxNPCs && Main.npc[TargetIndex].active && !Main.npc[TargetIndex].friendly)
            {
                target = Main.npc[TargetIndex];
            }
            else
            {
                target = FindClosestEnemy(500f);
            }
            
            if (target != null)
            {
                Vector2 desiredVel = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * MaxSpeed;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVel, HomingStrength);
            }
            
            // === HEAVY SPARKLE DUST TRAIL ===
            for (int d = 0; d < 2; d++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f), 
                    DustID.PurpleTorch, -Projectile.velocity * 0.12f + Main.rand.NextVector2Circular(1.5f, 1.5f));
                dust.noGravity = true;
                dust.scale = 1.1f + Main.rand.NextFloat(0.3f);
            }
            
            // Green sparkle dust
            if (Main.rand.NextBool(2))
            {
                Dust greenDust = Dust.NewDustPerfect(Projectile.Center, DustID.GreenTorch, 
                    -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(1.5f, 1.5f));
                greenDust.noGravity = true;
                greenDust.scale = 0.9f + Main.rand.NextFloat(0.3f);
            }
            
            // Particle flares
            if (Main.rand.NextBool(3))
            {
                float progress = Main.rand.NextFloat();
                Color trailColor = GetEnigmaGradient(progress);
                CustomParticles.GenericFlare(Projectile.Center, trailColor * 0.7f, 0.3f, 14);
                
                var glow = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f,
                    trailColor * 0.55f, 0.22f, 12, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Periodic glyph trail
            if (Projectile.timeLeft % 18 == 0)
            {
                CustomParticles.GlyphTrail(Projectile.Center, Projectile.velocity, EnigmaPurple, 0.24f);
            }
            
            Lighting.AddLight(Projectile.Center, EnigmaGreen.ToVector3() * 0.5f);
            Lighting.AddLight(Projectile.Center, EnigmaPurple.ToVector3() * 0.3f);
        }
        
        private NPC FindClosestEnemy(float range)
        {
            NPC closest = null;
            float closestDist = range;
            
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            
            return closest;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 300);
            var brandNPC = target.GetGlobalNPC<ParadoxBrandNPC>();
            brandNPC.AddParadoxStack(target, 2);
            
            // === REALITY WARP DISTORTION ===
            FateRealityDistortion.TriggerChromaticAberration(target.Center, 3f, 10);
            
            // === NEW UNIFIED VFX HIT EFFECT ===
            UnifiedVFX.EnigmaVariations.HitEffect(target.Center, 1.2f);
            
            // "?" impact
            CreateQuestionMarkExplosion(target.Center);
            
            // === WATCHING EYE AT IMPACT ===
            CustomParticles.EnigmaEyeImpact(target.Center, target.Center, EnigmaGreen, 0.5f);
            
            // === MUSIC NOTES ===
            ThemedParticles.EnigmaMusicNoteBurst(target.Center, 10, 6f);
            ThemedParticles.EnigmaMusicNotes(target.Center, 5, 35f);
            
            // Cascading sparkle shower on seeker impact
            for (int i = 0; i < 6; i++)
            {
                float cascadeAngle = MathHelper.TwoPi * i / 6f;
                Vector2 cascadePos = target.Center - new Vector2(0, 30f) + cascadeAngle.ToRotationVector2() * 15f;
                CustomParticles.GenericFlare(cascadePos, GetEnigmaGradient((float)i / 6f), 0.4f, 16);
            }
            
            // Glyph impact
            CustomParticles.GlyphImpact(target.Center, EnigmaPurple, EnigmaGreen, 0.55f);
            
            // === GLYPH CIRCLE FORMATION ===
            CustomParticles.GlyphCircle(target.Center, EnigmaPurple, count: 6, radius: 45f, rotationSpeed: 0.06f);
            
            // Glyph stacks
            int stacks = brandNPC.paradoxStacks;
            if (stacks > 0)
            {
                CustomParticles.GlyphStack(target.Center + new Vector2(0, -22f), EnigmaPurple, stacks, 0.26f);
            }
            
            // === DYNAMIC LIGHTING ===
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.8f);
        }
        
        private void CreateQuestionMarkExplosion(Vector2 position)
        {
            // Smaller "?" for seekers
            CustomParticles.GenericFlare(position + new Vector2(0, 20f), EnigmaGreen, 0.55f, 18);
            
            for (int i = 0; i < 10; i++)
            {
                float t = (float)i / 10f;
                float curveAngle = MathHelper.Pi * 1.2f * t - MathHelper.Pi * 0.45f;
                float curveRadius = 16f - t * 7f;
                float yOffset = -10f - t * 26f;
                
                Vector2 curvePos = position + new Vector2((float)Math.Cos(curveAngle) * curveRadius, yOffset);
                CustomParticles.GenericFlare(curvePos, GetEnigmaGradient(t), 0.38f, 16);
            }
            
            for (int i = 0; i < 7; i++)
            {
                float angle = MathHelper.TwoPi * i / 7f;
                Vector2 offset = angle.ToRotationVector2() * 26f;
                CustomParticles.GenericFlare(position + offset, GetEnigmaGradient((float)i / 7f), 0.38f, 15);
            }
            
            CustomParticles.HaloRing(position, EnigmaPurple * 0.75f, 0.4f, 16);
        }
        
        public override void OnKill(int timeLeft)
        {
            // === REALITY WARP ON DEATH ===
            FateRealityDistortion.TriggerChromaticAberration(Projectile.Center, 3f, 12);
            
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * 4f;
                var glow = new GenericGlowParticle(Projectile.Center, vel, GetEnigmaGradient((float)i / 6f), 0.3f, 14, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            CustomParticles.HaloRing(Projectile.Center, EnigmaGreen * 0.65f, 0.32f, 13);
        }
    }
    
    /// <summary>
    /// Special every-5th-shot bullet that pierces infinitely and chains on every hit
    /// MEGA SPARKLE BOLT - The ultimate mysterious projectile
    /// </summary>
    public class ParadoxPiercingBolt : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreenFlame = new Color(50, 220, 100);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private List<int> hitEnemies = new List<int>();
        private int eyeTextureIndex = 0;
        
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare";
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            
            // Load sparkle textures for MEGA dazzling trail
            Texture2D flareTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Texture2D sparkleTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/PrismaticSparkle" + (1 + (Projectile.whoAmI % 10))).Value;
            Texture2D eyeTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnigmaEye" + (1 + eyeTextureIndex)).Value;
            
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 flareOrigin = flareTex.Size() / 2f;
            Vector2 sparkleOrigin = sparkleTex.Size() / 2f;
            Vector2 eyeOrigin = eyeTex.Size() / 2f;
            
            // Switch to additive blending for maximum sparkle
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // === MEGA SPARKLE TRAIL - Enhanced for paradox bolt ===
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float trailProgress = (float)i / Projectile.oldPos.Length;
                float trailAlpha = (1f - trailProgress) * 0.95f;
                float trailScale = (1f - trailProgress * 0.4f) * 0.55f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                
                Color trailColor = Color.Lerp(EnigmaGreenFlame, EnigmaPurple, trailProgress);
                float sparkleRot = Main.GameUpdateCount * 0.18f + i * 0.6f;
                
                // Double-layered sparkles for extra density
                spriteBatch.Draw(sparkleTex, trailPos, null, trailColor * trailAlpha, sparkleRot, sparkleOrigin, trailScale, SpriteEffects.None, 0f);
                spriteBatch.Draw(flareTex, trailPos, null, EnigmaGreen * trailAlpha * 0.5f, 0f, flareOrigin, trailScale * 0.6f, SpriteEffects.None, 0f);
                
                // Extra flare sparks offset
                if (i % 2 == 0)
                {
                    Vector2 offsetPos = trailPos + new Vector2(MathF.Sin(i * 0.8f) * 8f, MathF.Cos(i * 0.8f) * 8f);
                    spriteBatch.Draw(sparkleTex, offsetPos, null, EnigmaPurple * trailAlpha * 0.6f, -sparkleRot, sparkleOrigin, trailScale * 0.4f, SpriteEffects.None, 0f);
                }
            }
            
            // === MYSTERIOUS EYES - Multiple watching from the paradox bolt ===
            float eyePulse = 0.85f + MathF.Sin(Main.GameUpdateCount * 0.12f) * 0.2f;
            float eyeRot = Projectile.velocity.ToRotation();
            spriteBatch.Draw(eyeTex, drawPos, null, EnigmaGreen * 0.8f, eyeRot, eyeOrigin, 0.45f * eyePulse, SpriteEffects.None, 0f);
            
            // Secondary eye orbiting
            float orbitAngle = Main.GameUpdateCount * 0.1f;
            Vector2 orbitOffset = orbitAngle.ToRotationVector2() * 15f;
            spriteBatch.Draw(eyeTex, drawPos + orbitOffset, null, EnigmaPurple * 0.5f, eyeRot + MathHelper.Pi, eyeOrigin, 0.25f * eyePulse, SpriteEffects.None, 0f);
            
            // === MASSIVE CORE FLARE ===
            float pulse = 0.95f + MathF.Sin(Main.GameUpdateCount * 0.22f) * 0.2f;
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaGreen * 1.0f, 0f, flareOrigin, 0.65f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaPurple * 0.8f, 0f, flareOrigin, 0.45f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, Color.White * 0.9f, 0f, flareOrigin, 0.25f * pulse, SpriteEffects.None, 0f);
            
            // === RADIAL SPARKLE BURST - 6 point star ===
            for (int i = 0; i < 6; i++)
            {
                float angle = Main.GameUpdateCount * 0.1f + i * MathHelper.Pi / 3f;
                float radius = 16f + MathF.Sin(Main.GameUpdateCount * 0.18f + i) * 5f;
                Vector2 sparkleOffset = angle.ToRotationVector2() * radius;
                Color sparkleColor = Color.Lerp(EnigmaGreen, EnigmaPurple, (float)i / 6f);
                spriteBatch.Draw(sparkleTex, drawPos + sparkleOffset, null, sparkleColor * 0.8f, angle * 2f, sparkleOrigin, 0.28f, SpriteEffects.None, 0f);
            }
            
            // Restore normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1; // Infinite pierce!
            Projectile.timeLeft = 400;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Cycle eye texture
            if (Projectile.timeLeft % 20 == 0)
                eyeTextureIndex = Main.rand.Next(8);
            
            // === MEGA HEAVY DUST TRAIL ===
            for (int d = 0; d < 3; d++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), 
                    DustID.PurpleTorch, -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(2f, 2f));
                dust.noGravity = true;
                dust.scale = 1.3f + Main.rand.NextFloat(0.5f);
            }
            
            // Green flame dust
            for (int d = 0; d < 2; d++)
            {
                Dust greenDust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), 
                    DustID.GreenTorch, -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1.5f, 1.5f));
                greenDust.noGravity = true;
                greenDust.scale = 1.1f + Main.rand.NextFloat(0.4f);
            }
            
            // Particle flares
            float progress = (Main.GameUpdateCount * 0.05f) % 1f;
            Color trailColor = GetEnigmaGradient(progress);
            CustomParticles.GenericFlare(Projectile.Center, trailColor * 0.9f, 0.42f, 18);
            
            if (Main.rand.NextBool(2))
            {
                var glow = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.12f + Main.rand.NextVector2Circular(1f, 1f),
                    GetEnigmaGradient(Main.rand.NextFloat()) * 0.7f, 0.3f, 16, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Glowing comet sparkle bursts
            if (Projectile.timeLeft % 8 == 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 cometVel = -Projectile.velocity * 0.12f + Main.rand.NextVector2Circular(2f, 2f);
                    var comet = new GenericGlowParticle(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), 
                        cometVel, GetEnigmaGradient((float)i / 4f), 0.38f, 20, true);
                    MagnumParticleHandler.SpawnParticle(comet);
                }
                CustomParticles.HaloRing(Projectile.Center, EnigmaGreen * 0.4f, 0.2f, 8);
            }
            
            // Glyph trail
            if (Projectile.timeLeft % 10 == 0)
            {
                CustomParticles.GlyphTrail(Projectile.Center, Projectile.velocity, EnigmaPurple, 0.3f);
            }
            
            // Green flame particles
            if (Main.rand.NextBool(3))
            {
                CustomParticles.GenericFlare(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), 
                    EnigmaGreen * 0.6f, 0.25f, 10);
            }
            
            Lighting.AddLight(Projectile.Center, EnigmaGreen.ToVector3() * 0.6f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 360);
            var brandNPC = target.GetGlobalNPC<ParadoxBrandNPC>();
            brandNPC.AddParadoxStack(target, 2);
            
            hitEnemies.Add(target.whoAmI);
            
            // CHAIN to all previously hit enemies!
            foreach (int npcIndex in hitEnemies)
            {
                if (npcIndex == target.whoAmI) continue;
                if (npcIndex < 0 || npcIndex >= Main.maxNPCs) continue;
                
                NPC other = Main.npc[npcIndex];
                if (!other.active || other.friendly) continue;
                
                float dist = Vector2.Distance(target.Center, other.Center);
                if (dist > 500f) continue;
                
                // Draw heavy chain lightning
                MagnumVFX.DrawFractalLightning(target.Center, other.Center, EnigmaGreen, 15, 40f, 5, 0.45f);
                
                // Chain damage
                other.SimpleStrikeNPC(Projectile.damage / 2, 0, false, 4f);
                other.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(other, 1);
                
                // Sparkle chain indicator at chained target  
                CustomParticles.GenericFlare(other.Center, EnigmaGreen, 0.5f, 16);
                var chainGlow = new GenericGlowParticle(other.Center + new Vector2(0, -25f), 
                    new Vector2(0, -2f), GetEnigmaGradient(0.6f), 0.32f, 16, true);
                MagnumParticleHandler.SpawnParticle(chainGlow);
            }
            
            // Impact VFX
            CustomParticles.GenericFlare(target.Center, Color.White, 0.85f, 22);
            
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 offset = angle.ToRotationVector2() * 35f;
                CustomParticles.GenericFlare(target.Center + offset, GetEnigmaGradient((float)i / 12f), 0.5f, 18);
            }
            
            // Multiple eyes watching
            // Convergent sparkle ring - reality bends inward
            for (int i = 0; i < 8; i++)
            {
                float ringAngle = MathHelper.TwoPi * i / 8f;
                Vector2 ringPos = target.Center + ringAngle.ToRotationVector2() * 45f;
                Vector2 convergeVel = (target.Center - ringPos).SafeNormalize(Vector2.Zero) * 3f;
                var converge = new GenericGlowParticle(ringPos, convergeVel, 
                    GetEnigmaGradient((float)i / 8f), 0.4f, 20, true);
                MagnumParticleHandler.SpawnParticle(converge);
            }
            ThemedParticles.EnigmaMusicNoteBurst(target.Center, 6, 3.5f);
            
            // Glyph circle
            CustomParticles.GlyphCircle(target.Center, EnigmaPurple, count: 6, radius: 45f, rotationSpeed: 0.06f);
            
            // Halo rings
            for (int ring = 0; ring < 3; ring++)
            {
                CustomParticles.HaloRing(target.Center, GetEnigmaGradient(ring / 3f), 0.4f + ring * 0.15f, 16 + ring * 3);
            }
            
            // Glyph stack
            int stacks = brandNPC.paradoxStacks;
            if (stacks > 0)
            {
                CustomParticles.GlyphStack(target.Center + new Vector2(0, -30f), EnigmaGreen, stacks, 0.32f);
            }
            
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 0.5f }, target.Center);
        }
        
        public override void OnKill(int timeLeft)
        {
            // === PARADOX BOLT REALITY WARP ===
            FateRealityDistortion.TriggerChromaticAberration(Projectile.Center, 4.5f, 15);
            FateRealityDistortion.TriggerInversionPulse(6);
            
            // Massive death burst
            CustomParticles.GenericFlare(Projectile.Center, Color.White, 0.9f, 25);
            
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 vel = angle.ToRotationVector2() * 6f;
                var glow = new GenericGlowParticle(Projectile.Center, vel, GetEnigmaGradient((float)i / 12f), 0.45f, 22, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            for (int ring = 0; ring < 4; ring++)
            {
                CustomParticles.HaloRing(Projectile.Center, GetEnigmaGradient(ring / 4f), 0.4f + ring * 0.2f, 18 + ring * 4);
            }
            
            // Final sparkle nova on projectile death
            for (int i = 0; i < 10; i++)
            {
                float novaAngle = MathHelper.TwoPi * i / 10f;
                Vector2 novaVel = novaAngle.ToRotationVector2() * (3.5f + Main.rand.NextFloat() * 2f);
                var nova = new GenericGlowParticle(Projectile.Center, novaVel,
                    GetEnigmaGradient((float)i / 10f), 0.38f, 22, true);
                MagnumParticleHandler.SpawnParticle(nova);
            }
            CustomParticles.HaloRing(Projectile.Center, EnigmaGreen, 0.5f, 18);
            CustomParticles.GlyphBurst(Projectile.Center, EnigmaGreen, count: 8, speed: 5f);
        }
    }
}
