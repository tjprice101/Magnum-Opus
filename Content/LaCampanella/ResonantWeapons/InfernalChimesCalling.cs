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
            Item.shoot = ModContent.ProjectileType<CampanellaChoirMinion>();
            Item.buffType = ModContent.BuffType<CampanellaChoirBuff>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            
            // Spawn minion at mouse position
            Vector2 spawnPos = Main.MouseWorld;
            Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback, player.whoAmI);
            
            // === EPIC SPAWN SOUND STACK ===
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = Main.rand.NextFloat(0.2f, 0.5f), Volume = 0.6f }, spawnPos);
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.4f, Volume = 0.4f }, spawnPos);
            SoundEngine.PlaySound(SoundID.Item44 with { Pitch = 0.3f, Volume = 0.35f }, spawnPos);
            
            // === UnifiedVFX LA CAMPANELLA EXPLOSION ===
            UnifiedVFX.LaCampanella.Explosion(spawnPos, 1.3f);
            
            // === MASSIVE SPAWN EFFECTS WITH CUSTOM PARTICLES ===
            ThemedParticles.LaCampanellaBellChime(spawnPos, 1.25f);
            ThemedParticles.LaCampanellaMusicalImpact(spawnPos, 0.85f, true);
            
            // === GRAND SPAWN EFFECTS ===
            ThemedParticles.LaCampanellaGrandImpact(spawnPos, 1f);
            ThemedParticles.LaCampanellaHaloBurst(spawnPos, 0.8f);
            ThemedParticles.LaCampanellaPrismaticBurst(spawnPos, 8, 0.8f);
            
            // === GRADIENT COLOR DEFINITIONS ===
            Color campanellaOrange = ThemedParticles.CampanellaOrange;
            Color campanellaYellow = ThemedParticles.CampanellaYellow;
            Color campanellaGold = ThemedParticles.CampanellaGold;
            Color campanellaBlack = ThemedParticles.CampanellaBlack;
            
            // === HEAVY SMOKE BURST - Infernal summoning smoke ===
            for (int s = 0; s < 12; s++)
            {
                float angle = MathHelper.TwoPi * s / 12f;
                Vector2 smokeVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                var smoke = new HeavySmokeParticle(
                    spawnPos + Main.rand.NextVector2Circular(15f, 15f),
                    smokeVel,
                    campanellaBlack,
                    Main.rand.Next(40, 70),
                    Main.rand.NextFloat(0.4f, 0.7f),
                    Main.rand.NextFloat(0.6f, 0.9f),
                    0.015f,
                    false
                );
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // === RADIAL FLARE BURST with GRADIENT ===
            for (int f = 0; f < 10; f++)
            {
                Vector2 flarePos = spawnPos + (MathHelper.TwoPi * f / 10).ToRotationVector2() * Main.rand.NextFloat(25f, 45f);
                float progress = (float)f / 10f;
                Color flareColor = Color.Lerp(Color.Lerp(campanellaOrange, campanellaYellow, progress * 2f), 
                    campanellaGold, Math.Max(0, progress * 2f - 1f));
                CustomParticles.GenericFlare(flarePos, flareColor, 0.5f, 15);
            }
            
            // Expanding halo rings with GRADIENT
            for (int i = 0; i < 4; i++)
            {
                float progress = (float)i / 4f;
                Color ringColor = Color.Lerp(campanellaOrange, campanellaGold, progress);
                CustomParticles.HaloRing(spawnPos, ringColor, 0.35f + i * 0.12f, 14 + i * 3);
            }
            
            // Fractal geometric burst - signature pattern
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 flarePos = spawnPos + angle.ToRotationVector2() * 40f;
                float progress = (float)i / 6f;
                Color fractalColor = Color.Lerp(campanellaOrange, campanellaGold, progress);
                CustomParticles.GenericFlare(flarePos, fractalColor, 0.45f, 16);
            }
            
            // Music notes
            ThemedParticles.LaCampanellaMusicNotes(spawnPos, 5, 30f);
            
            // Sparks burst with gradient
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                float progress = (float)i / 8f;
                Color sparkColor = Color.Lerp(campanellaYellow, campanellaGold, progress);
                CustomParticles.GenericGlow(spawnPos, sparkColor, 0.3f, 18);
            }
            
            // Screen shake
            // REMOVED: Screen shake disabled for La Campanella weapons
            // player.GetModPlayer<ScreenShakePlayer>()?.AddShake(4f, 8);
            
            Lighting.AddLight(spawnPos, 1f, 0.5f, 0.15f);
            
            return false;
        }

        public override void HoldItem(Player player)
        {
            // === UnifiedVFX LA CAMPANELLA AURA ===
            UnifiedVFX.LaCampanella.Aura(player.Center, 28f, 0.28f);
            
            // === SIGNATURE HOLD AURA - VIBRANT PARTICLES WHILE HELD! ===
            ThemedParticles.LaCampanellaHoldAura(player.Center, 0.75f);
            
            // Gradient ambient particles
            if (Main.rand.NextBool(12))
            {
                Vector2 offset = Main.rand.NextVector2Circular(22f, 22f);
                float progress = Main.rand.NextFloat();
                Color color = Color.Lerp(UnifiedVFX.LaCampanella.Orange, UnifiedVFX.LaCampanella.Gold, progress);
                CustomParticles.GenericGlow(player.Center + offset, color, 0.25f, 16);
            }
            
            // Gradient light
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.9f;
            Vector3 lightColor = Color.Lerp(UnifiedVFX.LaCampanella.Orange, UnifiedVFX.LaCampanella.Gold, 0.5f).ToVector3();
            Lighting.AddLight(player.Center, lightColor * pulse * 0.45f);
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
            if (player.ownedProjectileCounts[ModContent.ProjectileType<CampanellaChoirMinion>()] > 0)
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
            
            // === CHAINSAW BELL ATTACK SOUNDS ===
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.4f + Main.rand.NextFloat(0.3f), Volume = 0.45f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item34 with { Pitch = Main.rand.NextFloat(0.2f, 0.5f), Volume = 0.3f }, Projectile.Center);
            
            // === MASSIVE ATTACK PARTICLES ===
            ThemedParticles.LaCampanellaSparks(Projectile.Center, toTarget, 6, 6f);
            ThemedParticles.LaCampanellaBloomBurst(Projectile.Center, 0.35f);
            ThemedParticles.LaCampanellaMusicNotes(Projectile.Center, 2, 20f);
            
            // Halo ring on attack
            CustomParticles.HaloRing(Projectile.Center, ThemedParticles.CampanellaOrange, 0.3f, 12);
            
            // Flares in attack direction
            for (int i = 0; i < 3; i++)
            {
                Vector2 flarePos = Projectile.Center + toTarget * (15f + i * 15f);
                Color flareColor = Main.rand.NextBool() ? ThemedParticles.CampanellaOrange : ThemedParticles.CampanellaYellow;
                CustomParticles.GenericFlare(flarePos, flareColor, 0.3f, 10);
            }
            
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
            // === EPIC SHOCKWAVE SOUND STACK ===
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 0.7f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = Main.rand.NextFloat(-0.1f, 0.3f), Volume = 0.5f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item34 with { Pitch = Main.rand.NextFloat(0.1f, 0.4f), Volume = 0.35f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Pitch = 0.3f, Volume = 0.3f }, Projectile.Center);
            
            // === MASSIVE SHOCKWAVE EFFECTS WITH CUSTOM PARTICLES ===
            ThemedParticles.LaCampanellaBellChime(Projectile.Center, 1.8f);
            ThemedParticles.LaCampanellaShockwave(Projectile.Center, 1.5f);
            ThemedParticles.LaCampanellaMusicalImpact(Projectile.Center, 1f, true);
            
            // === GRAND IMPACT WITH ALL EFFECTS ===
            ThemedParticles.LaCampanellaGrandImpact(Projectile.Center, 1.4f);
            ThemedParticles.LaCampanellaHaloBurst(Projectile.Center, 1.1f);
            ThemedParticles.LaCampanellaPrismaticBurst(Projectile.Center, 12, 1f);
            
            // === EXPLOSIVE RADIAL FLARE BURST ===
            for (int f = 0; f < 10; f++)
            {
                Vector2 flarePos = Projectile.Center + (MathHelper.TwoPi * f / 10).ToRotationVector2() * Main.rand.NextFloat(30f, 55f);
                Color flareColor = Main.rand.Next(3) switch { 0 => ThemedParticles.CampanellaYellow, 1 => ThemedParticles.CampanellaOrange, _ => ThemedParticles.CampanellaGold };
                CustomParticles.GenericFlare(flarePos, flareColor, 0.55f, 16);
            }
            
            // === CRESCENT WAVES EXPANDING ===
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                ThemedParticles.LaCampanellaCrescentWave(Projectile.Center, angle.ToRotationVector2(), 0.7f);
            }
            
            // Multiple expanding halo rings
            for (int i = 0; i < 4; i++)
            {
                Color ringColor = i % 2 == 0 ? ThemedParticles.CampanellaOrange : ThemedParticles.CampanellaYellow;
                CustomParticles.HaloRing(Projectile.Center, ringColor, 0.45f + i * 0.15f, 16 + i * 3);
            }
            // Black shadow rings
            for (int i = 0; i < 2; i++)
            {
                CustomParticles.HaloRing(Projectile.Center, ThemedParticles.CampanellaBlack, 0.35f + i * 0.2f, 14 + i * 3);
            }
            
            // Radial flare burst
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 flarePos = Projectile.Center + angle.ToRotationVector2() * Main.rand.NextFloat(35f, 60f);
                Color flareColor = Main.rand.Next(3) switch
                {
                    0 => ThemedParticles.CampanellaOrange,
                    1 => ThemedParticles.CampanellaYellow,
                    _ => ThemedParticles.CampanellaGold
                };
                CustomParticles.GenericFlare(flarePos, flareColor, 0.5f, 16);
            }
            
            // Radial spark burst
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                ThemedParticles.LaCampanellaSparks(Projectile.Center, angle.ToRotationVector2(), 3, 7f);
            }
            
            // Music notes explosion
            ThemedParticles.LaCampanellaMusicNotes(Projectile.Center, 8, 45f);
            
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
                    
                    // Hit effects on each enemy - with flares and prismatic sparkles
                    ThemedParticles.LaCampanellaSparks(npc.Center, (npc.Center - Projectile.Center).SafeNormalize(Vector2.UnitX), 6, 5f);
                    ThemedParticles.LaCampanellaBloomBurst(npc.Center, 0.4f);
                    ThemedParticles.LaCampanellaPrismaticSparkles(npc.Center, 3, 0.5f);
                    CustomParticles.HaloRing(npc.Center, ThemedParticles.CampanellaOrange, 0.3f, 12);
                    // Radial flares around enemy
                    for (int f = 0; f < 4; f++)
                    {
                        Vector2 flarePos = npc.Center + (MathHelper.TwoPi * f / 4).ToRotationVector2() * 15f;
                        Color flareColor = Main.rand.NextBool() ? ThemedParticles.CampanellaYellow : ThemedParticles.CampanellaOrange;
                        CustomParticles.GenericFlare(flarePos, flareColor, 0.4f, 12);
                    }
                    
                    Lighting.AddLight(npc.Center, 0.7f, 0.35f, 0.1f);
                }
            }
            
            Lighting.AddLight(Projectile.Center, 1.3f, 0.65f, 0.2f);
        }

        private void SpawnAmbientParticles()
        {
            // === BLAZING LA CAMPANELLA MINION AURA ===
            // Fire glow particles orbiting
            if (Main.rand.NextBool(3))
            {
                ThemedParticles.LaCampanellaTrail(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f), Projectile.velocity);
            }
            
            // === ORBITING PARTICLES AROUND MINION ===
            if (Main.rand.NextBool(6))
            {
                ThemedParticles.LaCampanellaOrbiting(Projectile.Center, 25f, 4);
            }
            
            // === OCCASIONAL PRISMATIC SPARKLES ===
            if (Main.rand.NextBool(10))
            {
                ThemedParticles.LaCampanellaPrismaticSparkles(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f), 2, 0.4f);
            }
            
            // Occasional music note floating up
            if (Main.rand.NextBool(15))
            {
                ThemedParticles.LaCampanellaMusicNotes(Projectile.Center, 1, 20f);
            }
            
            // Black smoke wisps
            if (Main.rand.NextBool(8))
            {
                var smoke = new HeavySmokeParticle(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f),
                    new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-1.2f, -0.5f)),
                    ThemedParticles.CampanellaBlack, Main.rand.Next(30, 50), 
                    Main.rand.NextFloat(0.2f, 0.35f), 0.5f, 0.015f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Sparkles around the minion
            if (Main.rand.NextBool(10))
            {
                ThemedParticles.LaCampanellaSparkles(Projectile.Center, 2, 25f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Draw minion with bell-flame glow effect
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            float pulse = (float)Math.Sin(floatTimer * 2f) * 0.12f + 1f;
            
            // === ADDITIVE GLOW LAYERS ===
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Outer orange flame glow
            Main.EntitySpriteDraw(texture, drawPos, null, ThemedParticles.CampanellaOrange * 0.4f, Projectile.rotation, origin,
                Projectile.scale * pulse * 1.4f, SpriteEffects.None, 0);
            
            // Middle yellow glow
            Main.EntitySpriteDraw(texture, drawPos, null, ThemedParticles.CampanellaYellow * 0.3f, Projectile.rotation, origin,
                Projectile.scale * pulse * 1.2f, SpriteEffects.None, 0);
            
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
            
            // Notify parent minion of hit
            if (parentMinionIndex >= 0 && Main.projectile[parentMinionIndex].active &&
                Main.projectile[parentMinionIndex].ModProjectile is CampanellaChoirMinion minion)
            {
                minion.OnMinionHit();
            }
            
            // === CHAINSAW BELL HIT SOUND ===
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = Main.rand.NextFloat(0.4f, 0.7f), Volume = 0.4f }, target.Center);
            
            // === EXPLOSIVE SMOKE BURST! ===
            for (int i = 0; i < 3; i++)
            {
                Vector2 smokeVel = Main.rand.NextVector2Circular(3f, 3f);
                var smoke = new HeavySmokeParticle(
                    target.Center + Main.rand.NextVector2Circular(8f, 8f),
                    smokeVel,
                    ThemedParticles.CampanellaBlack,
                    Main.rand.Next(30, 45),
                    Main.rand.NextFloat(0.35f, 0.5f),
                    0.55f, 0.018f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // === BLACK SMOKE SPARKLE MAGIC - SIGNATURE HIT EFFECT! ===
            Vector2 hitDir = Main.rand.NextVector2Unit();
            ThemedParticles.LaCampanellaSignatureHit(target.Center, hitDir, 0.75f);
            
            // === PRISMATIC BURST ===
            ThemedParticles.LaCampanellaPrismaticSparkles(target.Center, 5, 0.5f);
            
            // === MASSIVE IMPACT EFFECTS ===
            ThemedParticles.LaCampanellaSparks(target.Center, hitDir, 10, 7f);
            ThemedParticles.LaCampanellaBloomBurst(target.Center, 0.6f);
            ThemedParticles.LaCampanellaMusicNotes(target.Center, 4, 25f);
            
            // Triple halo rings on hit
            CustomParticles.HaloRing(target.Center, ThemedParticles.CampanellaYellow, 0.45f, 15);
            CustomParticles.HaloRing(target.Center, ThemedParticles.CampanellaOrange, 0.35f, 12);
            CustomParticles.HaloRing(target.Center, ThemedParticles.CampanellaGold, 0.25f, 10);
            
            // Radial flare burst around impact
            for (int i = 0; i < 5; i++)
            {
                Vector2 flarePos = target.Center + (MathHelper.TwoPi * i / 5).ToRotationVector2() * 15f;
                Color flareColor = Main.rand.Next(3) switch
                {
                    0 => ThemedParticles.CampanellaYellow,
                    1 => ThemedParticles.CampanellaOrange,
                    _ => ThemedParticles.CampanellaGold
                };
                CustomParticles.GenericFlare(flarePos + Main.rand.NextVector2Circular(5f, 5f), flareColor, 0.35f, 12);
            }
            
            // Fire glow burst
            for (int i = 0; i < 4; i++)
            {
                Vector2 glowVel = Main.rand.NextVector2Circular(3f, 3f);
                Color glowColor = Main.rand.NextBool() ? ThemedParticles.CampanellaYellow : ThemedParticles.CampanellaOrange;
                var glow = new GenericGlowParticle(
                    target.Center + Main.rand.NextVector2Circular(8f, 8f),
                    glowVel, glowColor, Main.rand.NextFloat(0.3f, 0.5f), Main.rand.Next(12, 20), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            Lighting.AddLight(target.Center, 0.9f, 0.45f, 0.12f);
        }

        public override void OnKill(int timeLeft)
        {
            // Death burst
            ThemedParticles.LaCampanellaBloomBurst(Projectile.Center, 0.25f);
            ThemedParticles.LaCampanellaSparkles(Projectile.Center, 3, 15f);
        }

        public override bool PreDraw(ref Color lightColor) => false; // Pure particle visual
    }
}
