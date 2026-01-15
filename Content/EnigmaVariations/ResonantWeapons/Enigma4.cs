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
    /// UNSOLVED PHANTOM - Enigma Summon Weapon
    /// ========================================
    /// UNIQUE MECHANICS:
    /// - Summon a minion made of shifting eyes and glyphs
    /// - Minion phases in and out of visibility (semi-intangible feel)
    /// - Attacks leave PARADOX RIFTS that persist and damage enemies who touch them
    /// - Minion creates "mystery zones" that slow and damage enemies
    /// - Eyes orbit the minion, watching potential targets
    /// - Attacks chain to nearby enemies with eerie lightning
    /// - Special: At 3+ minions, they create a collective glyph formation
    /// </summary>
    public class Enigma4 : ModItem
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        public override string Texture => "Terraria/Images/Item_" + ItemID.RavenStaff;
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override void SetDefaults()
        {
            Item.damage = 155;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 12;
            Item.width = 50;
            Item.height = 50;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(gold: 18);
            Item.rare = ModContent.RarityType<EnigmaRarity>();
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<UnsolvedPhantomMinion>();
            Item.buffType = ModContent.BuffType<UnsolvedPhantomBuff>();
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect1", "Summons an Unsolved Phantom - a being of eyes and glyphs"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect2", "Attacks leave paradox rifts and chain to nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect3", "Phases between visibility, watching all"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaLore", "'What cannot be solved cannot be stopped'") 
            { 
                OverrideColor = EnigmaPurple 
            });
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            
            // Find spawn position near player
            Vector2 spawnPos = player.Center + new Vector2(Main.rand.NextFloat(-50f, 50f), -50f);
            
            Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback, player.whoAmI);
            
            // Spawn VFX
            CustomParticles.GenericFlare(spawnPos, Color.White, 0.9f, 22);
            
            for (int ring = 0; ring < 4; ring++)
            {
                CustomParticles.HaloRing(spawnPos, Color.Lerp(EnigmaPurple, EnigmaGreen, ring / 4f), 0.35f + ring * 0.15f, 15 + ring * 3);
            }
            
            CustomParticles.GlyphCircle(spawnPos, EnigmaPurple, count: 6, radius: 40f, rotationSpeed: 0.06f);
            
            // Ethereal sparkle spiral - phantom manifests from light
            for (int arm = 0; arm < 5; arm++)
            {
                float armAngle = MathHelper.TwoPi * arm / 5f;
                for (int point = 0; point < 3; point++)
                {
                    float spiralAngle = armAngle + point * 0.4f;
                    float spiralRadius = 20f + point * 15f;
                    Vector2 spiralPos = spawnPos + spiralAngle.ToRotationVector2() * spiralRadius;
                    CustomParticles.GenericFlare(spiralPos, GetEnigmaGradient((arm * 3 + point) / 15f), 0.45f, 18);
                }
            }
            
            CustomParticles.GlyphBurst(spawnPos, EnigmaPurple, count: 8, speed: 4f);
            
            // Music notes for grand summoning - the phantom's theme begins
            ThemedParticles.EnigmaMusicNoteBurst(spawnPos, 10, 5f);
            ThemedParticles.EnigmaMusicNotes(spawnPos, 6, 45f);
            
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 0.7f }, spawnPos);
            
            return false;
        }
    }
    
    public class UnsolvedPhantomBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.StardustMinionBleed;
        
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }
        
        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<UnsolvedPhantomMinion>()] > 0)
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
    
    /// <summary>
    /// The phantom minion - made of eyes and glyphs, phases in and out
    /// </summary>
    public class UnsolvedPhantomMinion : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreenFlame = new Color(50, 220, 100);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private float phaseTimer = 0f;
        private float visibility = 1f;
        private int attackCooldown = 0;
        private int mysteryZoneCooldown = 0;
        private const int AttackCooldownMax = 30;
        private const int MysteryZoneCooldownMax = 180;
        
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = glowTex.Size() / 2f;
            
            float pulse = 0.8f + (float)Math.Sin(Main.GameUpdateCount * 0.08f + phaseTimer) * 0.2f;
            float alpha = visibility;
            
            // Draw pulsing/floating minion glow - phases in and out
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaBlack * alpha * pulse, 0f, origin, 1.2f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaDeepPurple * 0.7f * alpha, 0f, origin, 0.85f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaPurple * 0.8f * alpha, 0f, origin, 0.55f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaGreenFlame * 0.6f * alpha, 0f, origin, 0.3f * pulse, SpriteEffects.None, 0f);
            
            return false;
        }
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override void SetStaticDefaults()
        {
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.minionSlots = 1f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }
        
        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => visibility > 0.5f;
        
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            if (!CheckActive(owner))
                return;
            
            // Phase in and out
            phaseTimer += 0.03f;
            visibility = 0.4f + (float)Math.Sin(phaseTimer) * 0.3f + 0.3f; // Range: 0.4 to 1.0
            Projectile.Opacity = visibility;
            
            // Decrease cooldowns
            if (attackCooldown > 0) attackCooldown--;
            if (mysteryZoneCooldown > 0) mysteryZoneCooldown--;
            
            // Find target
            NPC target = FindTarget(800f);
            
            if (target != null)
            {
                // Move toward target
                Vector2 targetPos = target.Center + new Vector2(0, -80f);
                Vector2 moveDir = (targetPos - Projectile.Center).SafeNormalize(Vector2.Zero);
                float speed = 12f;
                
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, moveDir * speed, 0.1f);
                
                // Attack when close and visible enough
                float dist = Vector2.Distance(Projectile.Center, target.Center);
                if (dist < 200f && attackCooldown <= 0 && visibility > 0.6f)
                {
                    attackCooldown = AttackCooldownMax;
                    AttackTarget(target);
                }
                
                // Create mystery zone periodically
                if (mysteryZoneCooldown <= 0 && dist < 300f)
                {
                    mysteryZoneCooldown = MysteryZoneCooldownMax;
                    CreateMysteryZone(target.Center);
                }
            }
            else
            {
                // Idle near player
                Vector2 idlePos = owner.Center + new Vector2((Projectile.minionPos % 2 == 0 ? -1 : 1) * 60f, -50f);
                Vector2 moveDir = (idlePos - Projectile.Center).SafeNormalize(Vector2.Zero);
                float dist = Vector2.Distance(idlePos, Projectile.Center);
                
                if (dist > 20f)
                {
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, moveDir * 8f, 0.08f);
                }
                else
                {
                    Projectile.velocity *= 0.9f;
                }
            }
            
            // Limit velocity
            if (Projectile.velocity.Length() > 16f)
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f;
            
            // Visual effects based on visibility
            DrawMinionVisuals();
        }
        
        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<UnsolvedPhantomBuff>());
                return false;
            }
            
            if (owner.HasBuff(ModContent.BuffType<UnsolvedPhantomBuff>()))
            {
                Projectile.timeLeft = 2;
            }
            
            return true;
        }
        
        private NPC FindTarget(float range)
        {
            Player owner = Main.player[Projectile.owner];
            
            // Check for player-targeted NPC first
            if (owner.HasMinionAttackTargetNPC)
            {
                NPC target = Main.npc[owner.MinionAttackTargetNPC];
                if (target.active && !target.friendly && Vector2.Distance(Projectile.Center, target.Center) <= range)
                    return target;
            }
            
            // Find closest enemy
            NPC closest = null;
            float closestDist = range;
            
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly || npc.CountsAsACritter) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            
            return closest;
        }
        
        private void AttackTarget(NPC target)
        {
            SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.5f, Volume = 0.5f }, Projectile.Center);
            
            // Fire phantom bolt
            Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, direction * 14f,
                ModContent.ProjectileType<PhantomBolt>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            
            // Attack VFX
            CustomParticles.GenericFlare(Projectile.Center, EnigmaGreen * visibility, 0.6f, 16);
            CustomParticles.HaloRing(Projectile.Center, EnigmaPurple * visibility, 0.35f, 12);
            
            // Sparkle targeting beam toward target
            for (int i = 0; i < 3; i++)
            {
                Vector2 beamPos = Projectile.Center + direction * (10f + i * 12f);
                CustomParticles.GenericFlare(beamPos, GetEnigmaGradient((float)i / 3f) * visibility, 0.32f, 12);
            }
        }
        
        private void CreateMysteryZone(Vector2 position)
        {
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.4f, Volume = 0.6f }, position);
            
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), position, Vector2.Zero,
                ModContent.ProjectileType<MysteryZone>(), Projectile.damage / 2, 0f, Projectile.owner);
            
            // Zone spawn VFX
            CustomParticles.GlyphCircle(position, EnigmaPurple, count: 8, radius: 60f, rotationSpeed: 0.04f);
            
            // Radiant ring pattern - mystery zone boundary
            for (int ring = 0; ring < 2; ring++)
            {
                int pointsInRing = 8 + ring * 4;
                float ringRadius = 50f + ring * 25f;
                for (int i = 0; i < pointsInRing; i++)
                {
                    float angle = MathHelper.TwoPi * i / pointsInRing + ring * 0.15f;
                    Vector2 ringPos = position + angle.ToRotationVector2() * ringRadius;
                    CustomParticles.GenericFlare(ringPos, GetEnigmaGradient((float)i / pointsInRing), 0.4f - ring * 0.1f, 16);
                }
            }
            
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 offset = angle.ToRotationVector2() * 50f;
                CustomParticles.GenericFlare(position + offset, GetEnigmaGradient((float)i / 12f), 0.5f, 18);
            }
        }
        
        private void DrawMinionVisuals()
        {
            // Orbiting sparkle wisps around minion
            if (Main.GameUpdateCount % 8 == 0)
            {
                int wispCount = 3;
                float baseAngle = Main.GameUpdateCount * 0.04f;
                
                for (int i = 0; i < wispCount; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / wispCount;
                    float radius = 35f;
                    Vector2 wispPos = Projectile.Center + angle.ToRotationVector2() * radius;
                    
                    CustomParticles.GenericFlare(wispPos, GetEnigmaGradient((float)i / wispCount) * visibility, 0.3f * visibility, 12);
                    var wisp = new GenericGlowParticle(wispPos, angle.ToRotationVector2() * 0.5f,
                        EnigmaPurple * visibility * 0.5f, 0.2f, 10, true);
                    MagnumParticleHandler.SpawnParticle(wisp);
                }
            }
            
            // Rotating glyph aura
            if (Main.GameUpdateCount % 12 == 0)
            {
                CustomParticles.GlyphCircle(Projectile.Center, EnigmaPurple * visibility * 0.6f, count: 4, 
                    radius: 30f, rotationSpeed: 0.03f);
            }
            
            // Core glow
            if (Main.rand.NextBool(4))
            {
                CustomParticles.GenericFlare(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f), 
                    GetEnigmaGradient(Main.rand.NextFloat()) * visibility * 0.7f, 0.25f, 10);
            }
            
            // Phase effect particles
            if (visibility < 0.6f && Main.rand.NextBool(3))
            {
                Vector2 particlePos = Projectile.Center + Main.rand.NextVector2Circular(25f, 25f);
                var glow = new GenericGlowParticle(particlePos, Main.rand.NextVector2Circular(2f, 2f),
                    EnigmaBlack * 0.5f, 0.2f, 15, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            Lighting.AddLight(Projectile.Center, EnigmaPurple.ToVector3() * 0.4f * visibility);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 300);
            target.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(target, 1);
            
            // === NEW UNIFIED VFX HIT EFFECT ===
            UnifiedVFX.EnigmaVariations.HitEffect(target.Center, 1.2f);
            
            // === WATCHING EYE AT IMPACT ===
            CustomParticles.EnigmaEyeImpact(target.Center, target.Center, EnigmaGreen, 0.5f);
            
            // === MUSIC NOTES BURST ===
            ThemedParticles.EnigmaMusicNoteBurst(target.Center, 10, 6f);
            ThemedParticles.EnigmaMusicNotes(target.Center, 5, 35f);
            
            CustomParticles.GenericFlare(target.Center, EnigmaGreen * visibility, 0.5f, 16);
            
            // Sparkle burst indicator on hit
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 burstPos = target.Center - new Vector2(0, 25f) + angle.ToRotationVector2() * 18f;
                CustomParticles.GenericFlare(burstPos, GetEnigmaGradient((float)i / 5f), 0.38f, 14);
            }
            CustomParticles.HaloRing(target.Center, EnigmaPurple * 0.7f, 0.35f, 12);
            
            // === GLYPH CIRCLE FORMATION ===
            CustomParticles.GlyphCircle(target.Center, EnigmaPurple, count: 6, radius: 45f, rotationSpeed: 0.06f);
            
            // === DYNAMIC LIGHTING ===
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.8f);
        }
    }
    
    /// <summary>
    /// Bolt fired by the phantom minion
    /// </summary>
    public class PhantomBolt : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreenFlame = new Color(50, 220, 100);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private List<int> hitEnemies = new List<int>();
        
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
                    float trailAlpha = (1f - trailProgress) * 0.55f;
                    float trailScale = (1f - trailProgress * 0.4f) * 0.4f;
                    Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                    
                    Color trailColor = Color.Lerp(EnigmaGreenFlame, EnigmaPurple, trailProgress);
                    spriteBatch.Draw(glowTex, trailPos, null, trailColor * trailAlpha, 0f, origin, trailScale, SpriteEffects.None, 0f);
                }
            }
            
            // Draw layered glow core
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaBlack * 0.9f, 0f, origin, 0.75f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaDeepPurple * 0.7f, 0f, origin, 0.5f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaPurple * 0.8f, 0f, origin, 0.3f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaGreenFlame * 0.6f, 0f, origin, 0.15f, SpriteEffects.None, 0f);
            
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
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            
            // Trail
            if (Main.rand.NextBool(2))
            {
                float progress = Main.rand.NextFloat();
                Color trailColor = GetEnigmaGradient(progress);
                CustomParticles.GenericFlare(Projectile.Center, trailColor * 0.7f, 0.28f, 14);
                
                var glow = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f,
                    trailColor * 0.55f, 0.2f, 12, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Sparkle comet trail
            if (Projectile.timeLeft % 20 == 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 trailVel = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                    var comet = new GenericGlowParticle(Projectile.Center, trailVel,
                        GetEnigmaGradient(Main.rand.NextFloat()) * 0.6f, 0.3f, 14, true);
                    MagnumParticleHandler.SpawnParticle(comet);
                }
            }
            
            Lighting.AddLight(Projectile.Center, EnigmaGreen.ToVector3() * 0.35f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 240);
            target.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(target, 1);
            
            hitEnemies.Add(target.whoAmI);
            
            // === NEW UNIFIED VFX HIT EFFECT ===
            UnifiedVFX.EnigmaVariations.HitEffect(target.Center, 1.2f);
            
            // === WATCHING EYE AT IMPACT ===
            CustomParticles.EnigmaEyeImpact(target.Center, target.Center, EnigmaGreen, 0.5f);
            
            // === MUSIC NOTES BURST ===
            ThemedParticles.EnigmaMusicNoteBurst(target.Center, 10, 6f);
            ThemedParticles.EnigmaMusicNotes(target.Center, 5, 35f);
            
            // Chain to nearby enemies
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly || npc.whoAmI == target.whoAmI || hitEnemies.Contains(npc.whoAmI)) continue;
                
                float dist = Vector2.Distance(target.Center, npc.Center);
                if (dist <= 200f)
                {
                    MagnumVFX.DrawFractalLightning(target.Center, npc.Center, EnigmaGreen, 8, 20f, 3, 0.3f);
                    npc.SimpleStrikeNPC(Projectile.damage / 3, 0, false, 2f);
                    npc.AddBuff(ModContent.BuffType<ParadoxBrand>(), 120);
                    break; // Only chain to one additional enemy per hit
                }
            }
            
            // Leave a paradox rift
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero,
                ModContent.ProjectileType<PhantomRift>(), Projectile.damage / 2, 0f, Projectile.owner);
            
            // Impact VFX
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 offset = angle.ToRotationVector2() * 25f;
                CustomParticles.GenericFlare(target.Center + offset, GetEnigmaGradient((float)i / 8f), 0.4f, 15);
            }
            
            CustomParticles.HaloRing(target.Center, EnigmaPurple, 0.4f, 15);
            // Cascading sparkle shower on impact
            for (int i = 0; i < 6; i++)
            {
                float cascadeAngle = MathHelper.TwoPi * i / 6f;
                Vector2 cascadePos = target.Center - new Vector2(0, 28f) + cascadeAngle.ToRotationVector2() * 15f;
                CustomParticles.GenericFlare(cascadePos, GetEnigmaGradient((float)i / 6f), 0.38f, 15);
            }
            CustomParticles.HaloRing(target.Center, EnigmaGreen * 0.7f, 0.32f, 14);
            
            // === GLYPH CIRCLE FORMATION ===
            CustomParticles.GlyphCircle(target.Center, EnigmaPurple, count: 6, radius: 45f, rotationSpeed: 0.06f);
            
            // === DYNAMIC LIGHTING ===
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.8f);
        }
        
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * 3.5f;
                var glow = new GenericGlowParticle(Projectile.Center, vel, GetEnigmaGradient((float)i / 6f), 0.28f, 14, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }
    }
    
    /// <summary>
    /// Paradox rift left by phantom attacks
    /// </summary>
    public class PhantomRift : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreenFlame = new Color(50, 220, 100);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = glowTex.Size() / 2f;
            
            float lifeProgress = 1f - (Projectile.timeLeft / 90f);
            float pulse = 0.7f + (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.3f;
            float opacity = 1f - lifeProgress * 0.7f;
            
            // Draw pulsing rift glow
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaBlack * opacity * pulse, 0f, origin, 0.85f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaDeepPurple * 0.6f * opacity, 0f, origin, 0.6f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaPurple * 0.7f * opacity, 0f, origin, 0.4f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaGreenFlame * 0.5f * opacity, 0f, origin, 0.2f * pulse, SpriteEffects.None, 0f);
            
            return false;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }
        
        public override void AI()
        {
            float lifeProgress = 1f - (Projectile.timeLeft / 90f);
            float opacity = 1f - lifeProgress * 0.7f;
            
            // Swirling vortex
            if (Main.GameUpdateCount % 3 == 0)
            {
                int spiralCount = 5;
                for (int i = 0; i < spiralCount; i++)
                {
                    float angle = Main.GameUpdateCount * 0.1f + MathHelper.TwoPi * i / spiralCount;
                    float radius = 22f + (float)Math.Sin(Main.GameUpdateCount * 0.12f + i) * 8f;
                    Vector2 particlePos = Projectile.Center + angle.ToRotationVector2() * radius;
                    Vector2 vel = (Projectile.Center - particlePos).SafeNormalize(Vector2.Zero) * 2.5f;
                    
                    Color spiralColor = Color.Lerp(EnigmaPurple, EnigmaGreen, (float)i / spiralCount) * opacity;
                    var glow = new GenericGlowParticle(particlePos, vel, spiralColor * 0.55f, 0.22f, 12, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
            }
            
            // Central pulse
            if (Main.GameUpdateCount % 8 == 0)
            {
                CustomParticles.GenericFlare(Projectile.Center, EnigmaBlack * opacity, 0.32f, 10);
            }
            
            // Occasional sparkle wisp
            if (Main.rand.NextBool(25))
            {
                Vector2 wispPos = Projectile.Center + Main.rand.NextVector2Circular(15f, 15f);
                Vector2 wispVel = Main.rand.NextVector2Circular(1f, 1f);
                var wisp = new GenericGlowParticle(wispPos, wispVel,
                    EnigmaPurple * opacity, 0.25f, 14, true);
                MagnumParticleHandler.SpawnParticle(wisp);
            }
            
            Lighting.AddLight(Projectile.Center, EnigmaPurple.ToVector3() * 0.25f * opacity);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 180);
            target.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(target, 1);
            
            // === NEW UNIFIED VFX HIT EFFECT ===
            UnifiedVFX.EnigmaVariations.HitEffect(target.Center, 1.2f);
            
            // === WATCHING EYE AT IMPACT ===
            CustomParticles.EnigmaEyeImpact(target.Center, target.Center, EnigmaGreen, 0.5f);
            
            // === MUSIC NOTES BURST ===
            ThemedParticles.EnigmaMusicNoteBurst(target.Center, 10, 6f);
            ThemedParticles.EnigmaMusicNotes(target.Center, 5, 35f);
            
            CustomParticles.GenericFlare(target.Center, EnigmaPurple, 0.35f, 12);
            
            // === GLYPH CIRCLE FORMATION ===
            CustomParticles.GlyphCircle(target.Center, EnigmaPurple, count: 6, radius: 45f, rotationSpeed: 0.06f);
            
            // === DYNAMIC LIGHTING ===
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.8f);
        }
    }
    
    /// <summary>
    /// Mystery zone that slows and damages enemies
    /// </summary>
    public class MysteryZone : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreenFlame = new Color(50, 220, 100);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private const float ZoneRadius = 120f;
        private int damageTimer = 0;
        
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = glowTex.Size() / 2f;
            
            float lifeProgress = 1f - (Projectile.timeLeft / 240f);
            float opacity = Math.Min(1f, lifeProgress * 4f) * (1f - Math.Max(0f, lifeProgress - 0.7f) * 3.3f);
            float pulse = 0.9f + (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f;
            
            // Draw zone glow - large area effect
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaBlack * opacity * 0.4f, 0f, origin, 3f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaDeepPurple * 0.3f * opacity, 0f, origin, 2.2f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaPurple * 0.5f * opacity, 0f, origin, 1.4f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaGreenFlame * 0.4f * opacity, 0f, origin, 0.7f * pulse, SpriteEffects.None, 0f);
            
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
            Projectile.width = 240;
            Projectile.height = 240;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 240; // 4 seconds
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }
        
        public override bool? CanDamage() => false; // We handle damage manually
        
        public override void AI()
        {
            float lifeProgress = 1f - (Projectile.timeLeft / 240f);
            float opacity = Math.Min(1f, lifeProgress * 4f) * (1f - Math.Max(0f, lifeProgress - 0.7f) * 3.3f);
            
            damageTimer++;
            if (damageTimer >= 20)
            {
                damageTimer = 0;
                DealZoneDamage();
            }
            
            // Outer ring of sparkles pulsing inward
            if (Main.GameUpdateCount % 12 == 0)
            {
                int sparkleCount = 6;
                float baseAngle = Main.GameUpdateCount * 0.02f;
                for (int i = 0; i < sparkleCount; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / sparkleCount;
                    Vector2 sparklePos = Projectile.Center + angle.ToRotationVector2() * (ZoneRadius - 10f);
                    CustomParticles.GenericFlare(sparklePos, GetEnigmaGradient((float)i / sparkleCount) * opacity, 0.4f * opacity, 15);
                    
                    // Inward sparkle trail
                    Vector2 inwardVel = (Projectile.Center - sparklePos).SafeNormalize(Vector2.Zero) * 2f;
                    var trail = new GenericGlowParticle(sparklePos, inwardVel,
                        EnigmaPurple * opacity * 0.5f, 0.25f, 12, true);
                    MagnumParticleHandler.SpawnParticle(trail);
                }
            }
            
            // Multiple glyph circles at different radii
            if (Main.GameUpdateCount % 15 == 0)
            {
                CustomParticles.GlyphCircle(Projectile.Center, EnigmaPurple * opacity, count: 8, 
                    radius: ZoneRadius * 0.8f, rotationSpeed: 0.03f);
                CustomParticles.GlyphCircle(Projectile.Center, EnigmaGreen * opacity * 0.7f, count: 5, 
                    radius: ZoneRadius * 0.5f, rotationSpeed: -0.04f);
            }
            
            // Swirling particles throughout zone
            if (Main.rand.NextBool(2))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float radius = Main.rand.NextFloat(20f, ZoneRadius);
                Vector2 particlePos = Projectile.Center + angle.ToRotationVector2() * radius;
                
                // Spiral inward
                Vector2 vel = (Projectile.Center - particlePos).SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2) * 2f;
                vel += (Projectile.Center - particlePos).SafeNormalize(Vector2.Zero) * 0.5f;
                
                var glow = new GenericGlowParticle(particlePos, vel, GetEnigmaGradient(radius / ZoneRadius) * opacity * 0.5f, 
                    0.2f, 20, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Central dark core
            if (Main.GameUpdateCount % 6 == 0)
            {
                CustomParticles.GenericFlare(Projectile.Center, EnigmaBlack * opacity, 0.4f, 12);
            }
            
            Lighting.AddLight(Projectile.Center, EnigmaPurple.ToVector3() * 0.3f * opacity);
        }
        
        private void DealZoneDamage()
        {
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly) continue;
                
                float dist = Vector2.Distance(npc.Center, Projectile.Center);
                if (dist <= ZoneRadius)
                {
                    // Damage based on proximity to center
                    float proximityMult = 1f + (1f - dist / ZoneRadius);
                    npc.SimpleStrikeNPC((int)(Projectile.damage * proximityMult), 0, false, 1f);
                    npc.AddBuff(ModContent.BuffType<ParadoxBrand>(), 120);
                    npc.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(npc, 1);
                    
                    // Slow effect (knockback resistance reduction simulation)
                    if (npc.knockBackResist > 0)
                    {
                        Vector2 pullDir = (Projectile.Center - npc.Center).SafeNormalize(Vector2.Zero);
                        npc.velocity = npc.velocity * 0.92f + pullDir * 0.3f;
                    }
                    
                    // Damage indicator
                    CustomParticles.GenericFlare(npc.Center, EnigmaPurple * 0.5f, 0.22f, 8);
                }
            }
        }
        
        public override void OnKill(int timeLeft)
        {
            // Zone collapse VFX
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 offset = angle.ToRotationVector2() * 40f;
                CustomParticles.GenericFlare(Projectile.Center + offset, GetEnigmaGradient((float)i / 10f), 0.4f, 16);
            }
            
            CustomParticles.HaloRing(Projectile.Center, EnigmaPurple, 0.5f, 18);
            CustomParticles.GlyphBurst(Projectile.Center, EnigmaGreen, count: 6, speed: 3f);
        }
    }
}
