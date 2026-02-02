using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.LaCampanella.Debuffs;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons
{
    /// <summary>
    /// Infernal Chime's Calling - Summon weapon that creates Campanella Choir minions.
    /// Attack: Summons minions that sing in harmony, creating waves of bell-music flames.
    /// Each minion hit applies Resonant Toll, and every few hits triggers a musical shockwave.
    /// </summary>
    public class InfernalChimesCalling : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
            ItemID.Sets.GamepadWholeScreenUseRange[Type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 145;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 20;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<LaCampanellaRainbowRarity>();
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<InfernalBellMinion>();
            Item.buffType = ModContent.BuffType<CampanellaChoirBuff>();
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Summons Campanella Choir minions that sing in harmony"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Minions create waves of bell-music flames and apply Resonant Toll"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Every few hits triggers a musical shockwave"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The choir rises, their infernal hymn echoing through eternity'") { OverrideColor = new Color(255, 140, 40) });
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            
            // Spawn minion at mouse position
            Vector2 spawnPos = Main.MouseWorld;
            Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback, player.whoAmI);
            
            // === SPAWN SOUNDS (2 max) ===
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = Main.rand.NextFloat(0.2f, 0.5f), Volume = 0.6f }, spawnPos);
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.4f, Volume = 0.4f }, spawnPos);
            
            // === UnifiedVFX LA CAMPANELLA EXPLOSION (handles core VFX) ===
            UnifiedVFX.LaCampanella.Explosion(spawnPos, 1.3f);
            
            // === GRADIENT COLOR DEFINITIONS ===
            Color campanellaBlack = ThemedParticles.CampanellaBlack;
            
            // === LIGHT SMOKE BURST (3 particles) ===
            for (int s = 0; s < 3; s++)
            {
                Vector2 smokeVel = Main.rand.NextVector2Circular(3f, 3f);
                var smoke = new HeavySmokeParticle(
                    spawnPos + Main.rand.NextVector2Circular(15f, 15f),
                    smokeVel,
                    campanellaBlack,
                    Main.rand.Next(35, 50),
                    Main.rand.NextFloat(0.3f, 0.5f),
                    0.5f,
                    0.02f,
                    false
                );
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Single halo ring
            CustomParticles.HaloRing(spawnPos, ThemedParticles.CampanellaOrange, 0.4f, 15);
            
            // Music notes (reduced)
            ThemedParticles.LaCampanellaMusicNotes(spawnPos, 3, 25f);
            
            // Screen shake
            // REMOVED: Screen shake disabled for La Campanella weapons
            // player.GetModPlayer<ScreenShakePlayer>()?.AddShake(4f, 8);
            
            Lighting.AddLight(spawnPos, 1f, 0.5f, 0.15f);
            
            return false;
        }

        public override void HoldItem(Player player)
        {
            // === SUBTLE HOLD AURA ===
            UnifiedVFX.LaCampanella.Aura(player.Center, 25f, 0.25f);
            
            // Rare ambient particles (1/15 chance)
            if (Main.rand.NextBool(15))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                Color color = Color.Lerp(UnifiedVFX.LaCampanella.Orange, UnifiedVFX.LaCampanella.Gold, Main.rand.NextFloat());
                CustomParticles.GenericGlow(player.Center + offset, color, 0.2f, 14);
            }
            
            // Subtle light
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.9f;
            Lighting.AddLight(player.Center, 0.4f * pulse, 0.2f * pulse, 0.06f * pulse);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.12f + 1f;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            spriteBatch.Draw(texture, position, null, new Color(255, 100, 0) * 0.4f, rotation, origin, scale * pulse * 1.2f, SpriteEffects.None, 0f);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Lighting.AddLight(Item.Center, 0.5f, 0.3f, 0.08f);
            
            return true;
        }
    }

    /// <summary>
    /// Buff for Campanella Choir minions.
    /// </summary>
    public class CampanellaChoirBuff : ModBuff
    {
        // Use the summoner weapon as the buff icon
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/InfernalChimesCalling";
        
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // Check for both old and new minion types for compatibility
            if (player.ownedProjectileCounts[ModContent.ProjectileType<InfernalBellMinion>()] > 0 ||
                player.ownedProjectileCounts[ModContent.ProjectileType<CampanellaChoirMinion>()] > 0)
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
    /// Campanella Choir minion - bell-themed summon that creates flame waves.
    /// Uses the weapon sprite as the minion visual with bell-flame effects.
    /// </summary>
    public class CampanellaChoirMinion : ModProjectile
    {
        // Use the summoner weapon as the minion appearance
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/InfernalChimesCalling";
        
        private int hitCounter = 0;
        private const int ShockwaveThreshold = 5;
        private float floatTimer = 0f;
        private int attackCooldown = 0;
        private NPC targetNPC = null;
        
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
            Projectile.minionSlots = 1f;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            // Check buff
            if (!CheckActive(owner))
                return;
            
            floatTimer += 0.08f;
            attackCooldown = Math.Max(0, attackCooldown - 1);
            
            // Find target
            FindTarget(owner);
            
            if (targetNPC != null && targetNPC.active)
            {
                // Move toward target
                MoveTowardTarget();
                
                // Attack
                if (attackCooldown <= 0)
                {
                    Attack();
                    attackCooldown = 40;
                }
            }
            else
            {
                // Return to owner
                ReturnToOwner(owner);
            }
            
            // Ambient particles
            SpawnAmbientParticles();
            
            // Floating animation
            Projectile.rotation = (float)Math.Sin(floatTimer) * 0.1f;
            
            // Lighting
            Lighting.AddLight(Projectile.Center, 0.5f, 0.3f, 0.1f);
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<CampanellaChoirBuff>());
                return false;
            }
            
            if (owner.HasBuff(ModContent.BuffType<CampanellaChoirBuff>()))
            {
                Projectile.timeLeft = 2;
            }
            
            return true;
        }

        private void FindTarget(Player owner)
        {
            // Check for player-designated target
            if (owner.HasMinionAttackTargetNPC)
            {
                NPC designated = Main.npc[owner.MinionAttackTargetNPC];
                if (designated.active && designated.CanBeChasedBy())
                {
                    targetNPC = designated;
                    return;
                }
            }
            
            // Find nearest enemy
            float nearestDist = 800f;
            targetNPC = null;
            
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    targetNPC = npc;
                }
            }
        }

        private void MoveTowardTarget()
        {
            Vector2 toTarget = targetNPC.Center - Projectile.Center;
            float distance = toTarget.Length();
            
            // Maintain some distance for ranged attacks
            float idealDist = 150f;
            float speed = 12f;
            
            if (distance > idealDist + 50f)
            {
                // Move closer
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget.SafeNormalize(Vector2.Zero) * speed, 0.1f);
            }
            else if (distance < idealDist - 30f)
            {
                // Move away
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, -toTarget.SafeNormalize(Vector2.Zero) * speed * 0.5f, 0.1f);
            }
            else
            {
                // Orbit/hover
                Vector2 orbitVel = toTarget.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2) * speed * 0.3f;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, orbitVel, 0.05f);
            }
            
            // Add floating motion
            Projectile.velocity.Y += (float)Math.Sin(floatTimer * 2f) * 0.1f;
        }

        private void ReturnToOwner(Player owner)
        {
            Vector2 targetPos = owner.Center + new Vector2(0, -60f);
            Vector2 toTarget = targetPos - Projectile.Center;
            
            if (toTarget.Length() > 600f)
            {
                // Teleport if too far
                Projectile.Center = targetPos;
            }
            else if (toTarget.Length() > 30f)
            {
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget.SafeNormalize(Vector2.Zero) * 10f, 0.1f);
            }
            else
            {
                Projectile.velocity *= 0.9f;
            }
            
            // Floating animation
            Projectile.velocity.Y += (float)Math.Sin(floatTimer) * 0.2f;
        }

        private void Attack()
        {
            if (targetNPC == null) return;
            
            Vector2 toTarget = (targetNPC.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
            
            // Fire flame wave projectile
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, toTarget * 12f,
                ModContent.ProjectileType<ChoirFlameWave>(), Projectile.damage, Projectile.knockBack, Projectile.owner, Projectile.whoAmI);
            
            // === ATTACK SOUNDS (2 max) ===
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.4f + Main.rand.NextFloat(0.3f), Volume = 0.45f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item34 with { Pitch = Main.rand.NextFloat(0.2f, 0.5f), Volume = 0.25f }, Projectile.Center);
            
            // === CLEAN ATTACK PARTICLES ===
            ThemedParticles.LaCampanellaSparks(Projectile.Center, toTarget, 4, 5f);
            ThemedParticles.LaCampanellaMusicNotes(Projectile.Center, 2, 18f);
            CustomParticles.HaloRing(Projectile.Center, ThemedParticles.CampanellaOrange, 0.3f, 12);
            
            Lighting.AddLight(Projectile.Center, 0.7f, 0.35f, 0.1f);
        }

        public void OnMinionHit()
        {
            hitCounter++;
            
            if (hitCounter >= ShockwaveThreshold)
            {
                hitCounter = 0;
                TriggerShockwave();
            }
        }

        private void TriggerShockwave()
        {
            // === SHOCKWAVE SOUNDS (2 max) ===
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 0.7f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = Main.rand.NextFloat(-0.1f, 0.3f), Volume = 0.5f }, Projectile.Center);
            
            // === CLEAN SHOCKWAVE VFX ===
            ThemedParticles.LaCampanellaShockwave(Projectile.Center, 1.2f);
            
            // Single halo ring
            CustomParticles.HaloRing(Projectile.Center, ThemedParticles.CampanellaOrange, 0.5f, 18);
            
            // Music notes
            ThemedParticles.LaCampanellaMusicNotes(Projectile.Center, 4, 35f);
            
            // Sparks (reduced)
            ThemedParticles.LaCampanellaSparks(Projectile.Center, Vector2.UnitX, 6, 6f);
            
            // Screen shake
            // REMOVED: Screen shake disabled for La Campanella weapons
            // Player owner = Main.player[Projectile.owner];
            // owner.GetModPlayer<ScreenShakePlayer>()?.AddShake(6f, 12);
            
            // Damage and debuff nearby enemies
            float shockwaveRadius = 200f;
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                
                if (Vector2.Distance(Projectile.Center, npc.Center) <= shockwaveRadius)
                {
                    npc.SimpleStrikeNPC(Projectile.damage, 0, false, 0f, null, false, 0f, true);
                    npc.GetGlobalNPC<ResonantTollNPC>().AddStacks(npc, 2);
                    
                    // Clean hit effects
                    ThemedParticles.LaCampanellaSparks(npc.Center, (npc.Center - Projectile.Center).SafeNormalize(Vector2.UnitX), 4, 4f);
                    CustomParticles.HaloRing(npc.Center, ThemedParticles.CampanellaOrange, 0.3f, 10);
                    
                    Lighting.AddLight(npc.Center, 0.6f, 0.3f, 0.1f);
                }
            }
            
            Lighting.AddLight(Projectile.Center, 1.3f, 0.65f, 0.2f);
        }

        private void SpawnAmbientParticles()
        {
            // Musical ambient (rare)
            if (Main.rand.NextBool(12))
            {
                Color noteColor = Color.Lerp(new Color(255, 140, 40), new Color(255, 200, 50), Main.rand.NextFloat());
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.8f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.28f, 35);
            }
            
            // Occasional fire trail
            if (Main.rand.NextBool(6))
            {
                ThemedParticles.LaCampanellaTrail(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f), Projectile.velocity);
            }
            
            // Black smoke wisps (rare)
            if (Main.rand.NextBool(15))
            {
                var smoke = new HeavySmokeParticle(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f),
                    new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-1f, -0.4f)),
                    ThemedParticles.CampanellaBlack, Main.rand.Next(25, 40), 
                    Main.rand.NextFloat(0.15f, 0.25f), 0.4f, 0.015f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Draw minion with bell-flame glow effect
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            float pulse = (float)Math.Sin(floatTimer * 2f) * 0.12f + 1f;
            
            // === ADDITIVE GLOW LAYERS WITH GLYPHS ===
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // === ORBITING GLYPHS AROUND MINION ===
            if (CustomParticleSystem.TexturesLoaded)
            {
                Texture2D glyphTex = CustomParticleSystem.RandomGlyph().Value;
                for (int i = 0; i < 4; i++)
                {
                    float glyphAngle = floatTimer * 2f + MathHelper.TwoPi * i / 4f;
                    float glyphRadius = 25f + (float)Math.Sin(floatTimer + i) * 5f;
                    Vector2 glyphPos = drawPos + glyphAngle.ToRotationVector2() * glyphRadius;
                    Color glyphColor = Color.Lerp(ThemedParticles.CampanellaBlack, ThemedParticles.CampanellaOrange, (float)i / 4f) * 0.55f;
                    Main.EntitySpriteDraw(glyphTex, glyphPos, null, glyphColor, glyphAngle * 1.5f, glyphTex.Size() / 2f, 0.16f * pulse, SpriteEffects.None, 0);
                }
            }
            
            // Outer orange flame glow
            Main.EntitySpriteDraw(texture, drawPos, null, ThemedParticles.CampanellaOrange * 0.4f, Projectile.rotation, origin,
                Projectile.scale * pulse * 1.4f, SpriteEffects.None, 0);
            
            // Middle black-to-orange gradient glow
            Main.EntitySpriteDraw(texture, drawPos, null, Color.Lerp(ThemedParticles.CampanellaBlack, ThemedParticles.CampanellaOrange, 0.5f) * 0.35f, Projectile.rotation, origin,
                Projectile.scale * pulse * 1.25f, SpriteEffects.None, 0);
            
            // Inner black depth
            Main.EntitySpriteDraw(texture, drawPos, null, ThemedParticles.CampanellaBlack * 0.25f, Projectile.rotation, origin,
                Projectile.scale * 1.05f, SpriteEffects.None, 0);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Main sprite with golden tint
            Color mainColor = Color.Lerp(Color.White, ThemedParticles.CampanellaGold, 0.15f);
            Main.EntitySpriteDraw(texture, drawPos, null, mainColor, Projectile.rotation, origin,
                Projectile.scale * 0.6f, SpriteEffects.None, 0);
            
            return false;
        }
    }

    /// <summary>
    /// Flame wave projectile from Choir minions.
    /// Pure particle visual - no texture drawn, entirely custom VFX.
    /// </summary>
    public class ChoirFlameWave : ModProjectile
    {
        // Uses weapon texture as base but entirely particle-drawn
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/InfernalChimesCalling";
        
        private int parentMinionIndex = -1;
        
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 200;
        }

        public override void AI()
        {
            if (parentMinionIndex < 0)
                parentMinionIndex = (int)Projectile.ai[0];
            
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.alpha = Math.Max(0, Projectile.alpha - 8);
            
            // Wave pattern
            float wave = (float)Math.Sin(Projectile.ai[1] += 0.25f) * 2f;
            Vector2 perp = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2);
            Projectile.Center += perp * wave * 0.3f;
            
            // === FLAMING DARK SMOKE TRAIL - BLAZING WAVE! ===
            
            // ☁EMUSICAL NOTATION - Choir flame wave melodic trail
            if (Main.rand.NextBool(6))
            {
                Color noteColor = Color.Lerp(new Color(255, 140, 40), new Color(255, 200, 50), Main.rand.NextFloat());
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), -0.7f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.32f, 32);
            }
            
            // Heavy black smoke trail
            if (Main.rand.NextBool(2))
            {
                var smoke = new HeavySmokeParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
                    -Projectile.velocity * 0.12f + Main.rand.NextVector2Circular(1f, 1f) + new Vector2(0, -0.5f),
                    ThemedParticles.CampanellaBlack,
                    Main.rand.Next(28, 45),
                    Main.rand.NextFloat(0.28f, 0.42f),
                    0.55f, 0.018f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Fire glow trail
            Color trailColor = Main.rand.Next(3) switch
            {
                0 => ThemedParticles.CampanellaYellow,
                1 => ThemedParticles.CampanellaGold,
                _ => ThemedParticles.CampanellaOrange
            };
            var glow = new GenericGlowParticle(
                Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(0.8f, 0.8f),
                trailColor, Main.rand.NextFloat(0.22f, 0.35f), Main.rand.Next(10, 16), true);
            MagnumParticleHandler.SpawnParticle(glow);
            
            // === BLAZING FLAME WAVE TRAIL ===
            ThemedParticles.LaCampanellaTrail(Projectile.Center, Projectile.velocity);
            
            // Fire sparks streaming behind
            if (Main.rand.NextBool(2))
            {
                ThemedParticles.LaCampanellaSparks(Projectile.Center, -Projectile.velocity.SafeNormalize(Vector2.UnitX), 3, 4f);
            }
            
            // Prismatic sparkle
            if (Main.rand.NextBool(2))
            {
                CustomParticles.PrismaticSparkle(Projectile.Center, ThemedParticles.CampanellaYellow, 0.32f);
            }
            
            // === GLITTERING SPARKLE TRAIL ===
            ThemedParticles.LaCampanellaSparkles(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), 2, 8f);
            ThemedParticles.LaCampanellaPrismaticSparkles(Projectile.Center, 1, 0.4f);
            
            // Occasional flare
            if (Main.rand.NextBool(5))
            {
                Color flareColor = Main.rand.NextBool() ? ThemedParticles.CampanellaYellow : ThemedParticles.CampanellaOrange;
                CustomParticles.GenericFlare(Projectile.Center, flareColor, 0.22f, 10);
            }
            
            Lighting.AddLight(Projectile.Center, 0.6f, 0.3f, 0.1f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);
            
            // === SEEKING CRYSTALS - 25% chance on summon minion hit ===
            if (Main.rand.NextBool(4))
            {
                SeekingCrystalHelper.SpawnLaCampanellaCrystals(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Projectile.velocity,
                    (int)(damageDone * 0.18f),
                    Projectile.knockBack,
                    Projectile.owner,
                    3
                );
            }
            
            // Notify parent minion of hit
            if (parentMinionIndex >= 0 && Main.projectile[parentMinionIndex].active &&
                Main.projectile[parentMinionIndex].ModProjectile is CampanellaChoirMinion minion)
            {
                minion.OnMinionHit();
            }
            
            // === HIT SOUND (1) ===
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = Main.rand.NextFloat(0.4f, 0.7f), Volume = 0.4f }, target.Center);
            
            // === CLEAN HIT EFFECTS ===
            // Smoke burst (reduced to 2)
            for (int i = 0; i < 2; i++)
            {
                Vector2 smokeVel = Main.rand.NextVector2Circular(2f, 2f);
                var smoke = new HeavySmokeParticle(
                    target.Center + Main.rand.NextVector2Circular(6f, 6f),
                    smokeVel,
                    ThemedParticles.CampanellaBlack,
                    Main.rand.Next(25, 35),
                    Main.rand.NextFloat(0.25f, 0.4f),
                    0.45f, 0.02f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Sparks and music notes
            Vector2 hitDir = Main.rand.NextVector2Unit();
            ThemedParticles.LaCampanellaSparks(target.Center, hitDir, 5, 5f);
            ThemedParticles.LaCampanellaMusicNotes(target.Center, 2, 20f);
            
            // Single halo ring
            CustomParticles.HaloRing(target.Center, ThemedParticles.CampanellaOrange, 0.35f, 12);
            
            Lighting.AddLight(target.Center, 0.7f, 0.35f, 0.1f);
        }

        public override void OnKill(int timeLeft)
        {
            // ☁EMUSICAL FINALE - Golden notes on choir flame death
            ThemedParticles.MusicNoteBurst(Projectile.Center, new Color(218, 165, 32), 4, 3f);
            
            // Death burst
            ThemedParticles.LaCampanellaBloomBurst(Projectile.Center, 0.25f);
            ThemedParticles.LaCampanellaSparkles(Projectile.Center, 3, 15f);
        }

        public override bool PreDraw(ref Color lightColor) => false; // Pure particle visual
    }
}
