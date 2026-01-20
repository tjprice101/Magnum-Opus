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
    /// THE WATCHING REFRAIN - Enigma Summon Weapon
    /// ============================================
    /// UNIQUE MECHANICS:
    /// - Summon a minion made of shifting eyes and glyphs
    /// - Minion phases in and out of visibility (semi-intangible feel)
    /// - Attacks leave PARADOX RIFTS that persist and damage enemies who touch them
    /// - Minion creates "mystery zones" that slow and damage enemies
    /// - Eyes orbit the minion, watching potential targets
    /// - Attacks chain to nearby enemies with eerie lightning
    /// - Special: At 3+ minions, they create a collective glyph formation
    /// </summary>
    public class TheWatchingRefrain : ModItem
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
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
        
        public override void HoldItem(Player player)
        {
            // === WATCHING REFRAIN HOLD EFFECT - Phantom preview and eye formation ===
            // Count active phantoms for formation effects
            int phantomCount = player.ownedProjectileCounts[ModContent.ProjectileType<UnsolvedPhantomMinion>()];
            
            // Eyes watching in formation around player
            if (Main.rand.NextBool(15))
            {
                int eyeCount = Math.Min(phantomCount + 1, 4);
                for (int i = 0; i < eyeCount; i++)
                {
                    if (Main.rand.NextBool(3))
                    {
                        float eyeAngle = Main.GameUpdateCount * 0.02f + MathHelper.TwoPi * i / eyeCount;
                        Vector2 eyePos = player.Center + eyeAngle.ToRotationVector2() * (35f + phantomCount * 5f);
                        Vector2 lookDir = (-eyeAngle).ToRotationVector2();
                        CustomParticles.EnigmaEyeGaze(eyePos, EnigmaGreen * 0.6f, 0.2f, lookDir);
                    }
                }
            }
            
            // Phantom preview - spectral wisps near summon location
            if (Main.rand.NextBool(10))
            {
                Vector2 previewPos = player.Center + new Vector2(Main.rand.NextFloat(-40f, 40f), -40f);
                var wisp = new GenericGlowParticle(previewPos, new Vector2(0, -0.5f), 
                    GetEnigmaGradient(Main.rand.NextFloat()) * 0.5f, 0.2f, 20, true);
                MagnumParticleHandler.SpawnParticle(wisp);
            }
            
            // Glyphs if multiple phantoms (formation bonus preview)
            if (phantomCount >= 2 && Main.rand.NextBool(12))
            {
                float formationAngle = Main.GameUpdateCount * 0.025f;
                CustomParticles.Glyph(player.Center + formationAngle.ToRotationVector2() * 50f, EnigmaPurple, 0.28f);
            }
            
            // Paradox rift preview particles
            if (Main.rand.NextBool(18))
            {
                Vector2 riftPos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                CustomParticles.GenericFlare(riftPos, EnigmaDeepPurple, 0.2f, 14);
            }
            
            // Ambient phantom light - intensity scales with phantom count
            float intensity = 0.25f + (phantomCount * 0.08f);
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.12f + 0.88f;
            Lighting.AddLight(player.Center, EnigmaPurple.ToVector3() * pulse * intensity);
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            
            // Find spawn position near player
            Vector2 spawnPos = player.Center + new Vector2(Main.rand.NextFloat(-50f, 50f), -50f);
            
            Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback, player.whoAmI);
            
            // Spawn VFX
            CustomParticles.GenericFlare(spawnPos, EnigmaGreen, 0.9f, 22);
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
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            float pulse = 0.8f + (float)Math.Sin(Main.GameUpdateCount * 0.08f + phaseTimer) * 0.2f;
            float alpha = visibility;
            
            // Switch to additive blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Texture2D eyeTex = CustomParticleSystem.EnigmaEyes[((int)(Main.GameUpdateCount * 0.05f)) % 8].Value;
            Texture2D glyphTex = CustomParticleSystem.RandomGlyph().Value;
            Texture2D sparkleTex = CustomParticleSystem.RandomPrismaticSparkle().Value;
            Texture2D flareTex = CustomParticleSystem.EnergyFlares[0].Value;
            
            // Orbiting glyphs around the phantom
            for (int i = 0; i < 5; i++)
            {
                float angle = Main.GameUpdateCount * 0.04f + MathHelper.TwoPi * i / 5f;
                float radius = 30f + (float)Math.Sin(Main.GameUpdateCount * 0.06f + i) * 10f;
                Vector2 glyphPos = drawPos + angle.ToRotationVector2() * radius * pulse;
                Color glyphColor = Color.Lerp(EnigmaDeepPurple, EnigmaGreenFlame, (float)i / 5f) * 0.6f * alpha;
                spriteBatch.Draw(glyphTex, glyphPos, null, glyphColor, angle * 2f, glyphTex.Size() / 2f, 0.18f * pulse, SpriteEffects.None, 0f);
            }
            
            // Orbiting sparkles
            for (int i = 0; i < 4; i++)
            {
                float angle = Main.GameUpdateCount * 0.07f + MathHelper.TwoPi * i / 4f;
                float radius = 18f;
                Vector2 sparkPos = drawPos + angle.ToRotationVector2() * radius * pulse;
                Color sparkColor = Color.Lerp(EnigmaPurple, EnigmaGreenFlame, (float)i / 4f) * 0.55f * alpha;
                spriteBatch.Draw(sparkleTex, sparkPos, null, sparkColor, angle * 1.5f, sparkleTex.Size() / 2f, 0.14f * pulse, SpriteEffects.None, 0f);
            }
            
            // Central watching eye - the phantom's core
            spriteBatch.Draw(eyeTex, drawPos, null, EnigmaPurple * 0.85f * alpha, Main.GameUpdateCount * 0.015f, eyeTex.Size() / 2f, 0.4f * pulse, SpriteEffects.None, 0f);
            
            // Inner glow core
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaDeepPurple * 0.7f * alpha, -Main.GameUpdateCount * 0.03f, flareTex.Size() / 2f, 0.5f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaGreenFlame * 0.5f * alpha, Main.GameUpdateCount * 0.04f, flareTex.Size() / 2f, 0.25f * pulse, SpriteEffects.None, 0f);
            
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
            // OPTIMIZED: Orbiting sparkle wisps - reduced from 8 to 20 frames, fewer particles
            if (Main.GameUpdateCount % 20 == 0)
            {
                int wispCount = 2;
                float baseAngle = Main.GameUpdateCount * 0.04f;
                
                for (int i = 0; i < wispCount; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / wispCount;
                    float radius = 35f;
                    Vector2 wispPos = Projectile.Center + angle.ToRotationVector2() * radius;
                    
                    CustomParticles.GenericFlare(wispPos, GetEnigmaGradient((float)i / wispCount) * visibility, 0.35f * visibility, 14);
                }
            }
            
            // OPTIMIZED: Rotating glyph aura - reduced from 12 to 30 frames, fewer glyphs
            if (Main.GameUpdateCount % 30 == 0)
            {
                CustomParticles.GlyphCircle(Projectile.Center, EnigmaPurple * visibility * 0.65f, count: 2, 
                    radius: 30f, rotationSpeed: 0.03f);
            }
            
            // OPTIMIZED: Core glow - reduced from NextBool(4) to every 15 frames
            if (Main.GameUpdateCount % 15 == 0)
            {
                CustomParticles.GenericFlare(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f), 
                    GetEnigmaGradient(Main.rand.NextFloat()) * visibility * 0.75f, 0.3f, 12);
            }
            
            // OPTIMIZED: Phase effect particles - reduced from NextBool(3) to every 20 frames when phased
            if (visibility < 0.6f && Main.GameUpdateCount % 20 == 0)
            {
                Vector2 particlePos = Projectile.Center + Main.rand.NextVector2Circular(20f, 20f);
                var glow = new GenericGlowParticle(particlePos, Main.rand.NextVector2Circular(1.5f, 1.5f),
                    EnigmaBlack * 0.55f, 0.22f, 16, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            Lighting.AddLight(Projectile.Center, EnigmaPurple.ToVector3() * 0.4f * visibility);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 300);
            target.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(target, 1);
            
            // === REALITY WARP DISTORTION ===
            FateRealityDistortion.TriggerChromaticAberration(target.Center, 3f, 10);
            
            // === OPTIMIZED UNIFIED VFX HIT EFFECT ===
            UnifiedVFX.EnigmaVariations.HitEffect(target.Center, 1.0f);
            
            // === WATCHING EYE AT IMPACT ===
            CustomParticles.EnigmaEyeImpact(target.Center, target.Center, EnigmaGreen, 0.45f);
            
            // === MUSIC NOTES BURST - Reduced from 10+5 to 4 total ===
            ThemedParticles.EnigmaMusicNoteBurst(target.Center, 4, 5f);
            
            CustomParticles.GenericFlare(target.Center, EnigmaGreen * visibility, 0.5f, 16);
            CustomParticles.HaloRing(target.Center, EnigmaPurple * 0.7f, 0.38f, 14);
            
            // === GLYPH CIRCLE FORMATION - Reduced ===
            CustomParticles.GlyphCircle(target.Center, EnigmaPurple, count: 3, radius: 40f, rotationSpeed: 0.06f);
            
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
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            // Switch to additive blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Texture2D glyphTex = CustomParticleSystem.RandomGlyph().Value;
            Texture2D sparkleTex = CustomParticleSystem.RandomPrismaticSparkle().Value;
            Texture2D flareTex = CustomParticleSystem.EnergyFlares[0].Value;
            
            // Eerie glyph trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float trailProgress = (float)i / Projectile.oldPos.Length;
                float trailAlpha = (1f - trailProgress) * 0.55f;
                float trailScale = (1f - trailProgress * 0.4f) * 0.15f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                
                Color trailColor = Color.Lerp(EnigmaGreenFlame, EnigmaPurple, trailProgress);
                spriteBatch.Draw(sparkleTex, trailPos, null, trailColor * trailAlpha, i * 0.15f, sparkleTex.Size() / 2f, trailScale, SpriteEffects.None, 0f);
            }
            
            // Orbiting glyphs
            for (int i = 0; i < 4; i++)
            {
                float angle = Main.GameUpdateCount * 0.08f + MathHelper.TwoPi * i / 4f;
                float radius = 12f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + i) * 4f;
                Vector2 glyphPos = drawPos + angle.ToRotationVector2() * radius;
                Color glyphColor = Color.Lerp(EnigmaDeepPurple, EnigmaGreenFlame, (float)i / 4f) * 0.6f;
                spriteBatch.Draw(glyphTex, glyphPos, null, glyphColor, angle * 2f, glyphTex.Size() / 2f, 0.12f, SpriteEffects.None, 0f);
            }
            
            // Orbiting sparkles
            for (int i = 0; i < 3; i++)
            {
                float angle = Main.GameUpdateCount * 0.1f + MathHelper.TwoPi * i / 3f;
                float radius = 8f;
                Vector2 sparkPos = drawPos + angle.ToRotationVector2() * radius;
                spriteBatch.Draw(sparkleTex, sparkPos, null, EnigmaPurple * 0.55f, angle * 1.5f, sparkleTex.Size() / 2f, 0.1f, SpriteEffects.None, 0f);
            }
            
            // Phantom bolt core
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaDeepPurple * 0.85f, Main.GameUpdateCount * 0.04f, flareTex.Size() / 2f, 0.35f, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaPurple * 0.7f, -Main.GameUpdateCount * 0.05f, flareTex.Size() / 2f, 0.22f, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaGreenFlame * 0.55f, 0f, flareTex.Size() / 2f, 0.1f, SpriteEffects.None, 0f);
            
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
            
            // Trail - every 4 frames instead of 50% every frame
            if (Projectile.timeLeft % 4 == 0)
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
            
            // === REALITY WARP DISTORTION ===
            FateRealityDistortion.TriggerChromaticAberration(target.Center, 3f, 10);
            
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
            // === REALITY WARP ON DEATH ===
            FateRealityDistortion.TriggerChromaticAberration(Projectile.Center, 3.5f, 12);
            
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
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            float lifeProgress = 1f - (Projectile.timeLeft / 90f);
            float pulse = 0.7f + (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.3f;
            float opacity = 1f - lifeProgress * 0.7f;
            
            // Switch to additive blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Texture2D eyeTex = CustomParticleSystem.EnigmaEyes[((int)(Main.GameUpdateCount * 0.03f)) % 8].Value;
            Texture2D glyphTex = CustomParticleSystem.RandomGlyph().Value;
            Texture2D sparkleTex = CustomParticleSystem.RandomPrismaticSparkle().Value;
            Texture2D flareTex = CustomParticleSystem.EnergyFlares[0].Value;
            
            // Swirling glyphs around the rift
            for (int i = 0; i < 4; i++)
            {
                float angle = Main.GameUpdateCount * 0.06f + MathHelper.TwoPi * i / 4f;
                float radius = 20f * pulse;
                Vector2 glyphPos = drawPos + angle.ToRotationVector2() * radius;
                Color glyphColor = Color.Lerp(EnigmaDeepPurple, EnigmaGreenFlame, (float)i / 4f) * 0.55f * opacity;
                spriteBatch.Draw(glyphTex, glyphPos, null, glyphColor, angle * 2f, glyphTex.Size() / 2f, 0.15f * pulse, SpriteEffects.None, 0f);
            }
            
            // Orbiting sparkles
            for (int i = 0; i < 3; i++)
            {
                float angle = Main.GameUpdateCount * 0.08f + MathHelper.TwoPi * i / 3f;
                float radius = 12f * pulse;
                Vector2 sparkPos = drawPos + angle.ToRotationVector2() * radius;
                spriteBatch.Draw(sparkleTex, sparkPos, null, EnigmaPurple * 0.5f * opacity, angle * 1.5f, sparkleTex.Size() / 2f, 0.1f, SpriteEffects.None, 0f);
            }
            
            // Central watching eye - the rift observes
            spriteBatch.Draw(eyeTex, drawPos, null, EnigmaPurple * 0.75f * opacity, Main.GameUpdateCount * 0.02f, eyeTex.Size() / 2f, 0.3f * pulse, SpriteEffects.None, 0f);
            
            // Rift core glow
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaDeepPurple * 0.65f * opacity, -Main.GameUpdateCount * 0.03f, flareTex.Size() / 2f, 0.4f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaGreenFlame * 0.45f * opacity, Main.GameUpdateCount * 0.04f, flareTex.Size() / 2f, 0.18f * pulse, SpriteEffects.None, 0f);
            
            // Restore normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
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
            
            // === PHANTOM RIFT REALITY WARP ===
            FateRealityDistortion.TriggerChromaticAberration(target.Center, 4f, 15);
            
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
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            float lifeProgress = 1f - (Projectile.timeLeft / 240f);
            float opacity = Math.Min(1f, lifeProgress * 4f) * (1f - Math.Max(0f, lifeProgress - 0.7f) * 3.3f);
            float pulse = 0.9f + (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f;
            
            // Switch to additive blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Texture2D eyeTex = CustomParticleSystem.EnigmaEyes[((int)(Main.GameUpdateCount * 0.02f)) % 8].Value;
            Texture2D glyphTex = CustomParticleSystem.RandomGlyph().Value;
            Texture2D sparkleTex = CustomParticleSystem.RandomPrismaticSparkle().Value;
            Texture2D flareTex = CustomParticleSystem.EnergyFlares[0].Value;
            
            // Outer ring of glyphs - zone boundary
            for (int i = 0; i < 8; i++)
            {
                float angle = Main.GameUpdateCount * 0.02f + MathHelper.TwoPi * i / 8f;
                float radius = 100f * pulse;
                Vector2 glyphPos = drawPos + angle.ToRotationVector2() * radius;
                Color glyphColor = Color.Lerp(EnigmaDeepPurple, EnigmaGreenFlame, (float)i / 8f) * 0.5f * opacity;
                spriteBatch.Draw(glyphTex, glyphPos, null, glyphColor, angle * 2f, glyphTex.Size() / 2f, 0.22f * pulse, SpriteEffects.None, 0f);
            }
            
            // Inner ring of watching eyes
            for (int i = 0; i < 4; i++)
            {
                float angle = -Main.GameUpdateCount * 0.03f + MathHelper.TwoPi * i / 4f;
                float radius = 55f * pulse;
                Vector2 eyePos = drawPos + angle.ToRotationVector2() * radius;
                spriteBatch.Draw(eyeTex, eyePos, null, EnigmaPurple * 0.6f * opacity, 0f, eyeTex.Size() / 2f, 0.25f, SpriteEffects.None, 0f);
            }
            
            // Sparkles throughout the zone
            for (int i = 0; i < 6; i++)
            {
                float angle = Main.GameUpdateCount * 0.04f + MathHelper.TwoPi * i / 6f;
                float radius = 35f + (float)Math.Sin(Main.GameUpdateCount * 0.08f + i) * 15f;
                Vector2 sparkPos = drawPos + angle.ToRotationVector2() * radius * pulse;
                spriteBatch.Draw(sparkleTex, sparkPos, null, EnigmaGreenFlame * 0.45f * opacity, angle * 1.5f, sparkleTex.Size() / 2f, 0.14f, SpriteEffects.None, 0f);
            }
            
            // Central eye - the mystery watches
            spriteBatch.Draw(eyeTex, drawPos, null, EnigmaPurple * 0.8f * opacity, Main.GameUpdateCount * 0.01f, eyeTex.Size() / 2f, 0.5f * pulse, SpriteEffects.None, 0f);
            
            // Zone core glow
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaDeepPurple * 0.5f * opacity, -Main.GameUpdateCount * 0.02f, flareTex.Size() / 2f, 1.5f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaPurple * 0.4f * opacity, Main.GameUpdateCount * 0.025f, flareTex.Size() / 2f, 0.9f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaGreenFlame * 0.35f * opacity, 0f, flareTex.Size() / 2f, 0.4f * pulse, SpriteEffects.None, 0f);
            
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
            
            // Swirling particles throughout zone - every 6 frames
            if (Main.GameUpdateCount % 6 == 0)
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
            // === REALITY WARP ON ZONE COLLAPSE ===
            FateRealityDistortion.TriggerChromaticAberration(Projectile.Center, 4f, 15);
            
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
