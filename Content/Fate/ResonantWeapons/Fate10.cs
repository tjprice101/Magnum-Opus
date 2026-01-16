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
    /// STAFF OF DESTINED CHAMPIONS - Summon Weapon #1
    /// 
    /// UNIQUE ABILITY: "AVATAR OF FATE"
    /// Summons a large, impressive fate avatar that fights alongside you.
    /// The avatar has 3 attack modes that cycle:
    /// 1. COSMIC SWORD STRIKES - Wide sweeping attacks with kaleidoscopic trails
    /// 2. DESTINY ORBS - Fires seeking orbs that explode on contact
    /// 3. REALITY RIFT - Opens portals that damage enemies passing through
    /// 
    /// PASSIVE: The avatar creates constant lens flare effects and leaves
    /// chromatic afterimages when moving. Multiple avatars synchronize attacks.
    /// </summary>
    public class Fate10 : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.StaffoftheFrostHydra;
        
        public override void SetDefaults()
        {
            Item.damage = 155;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 20;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 30);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<FateAvatarMinion>();
            Item.buffType = ModContent.BuffType<FateAvatarBuff>();
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Summons an Avatar of Fate to fight for you"));
            tooltips.Add(new TooltipLine(Mod, "FateEffect2", "The avatar cycles through sword strikes, orbs, and reality rifts"));
            tooltips.Add(new TooltipLine(Mod, "FateEffect3", "Multiple avatars coordinate their attacks"));
            tooltips.Add(new TooltipLine(Mod, "FateLore", "'Champions of destiny, bound to your will'") { OverrideColor = FateLensFlare.FateDarkPink });
        }
        
        public override void HoldItem(Player player)
        {
            Vector2 staffPos = player.Center + new Vector2(player.direction * 25f, -10f);
            
            // Destiny energy swirling around staff
            if (Main.rand.NextBool(6))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 particlePos = staffPos + angle.ToRotationVector2() * Main.rand.NextFloat(15f, 30f);
                Color particleColor = FateLensFlare.GetFateGradient(Main.rand.NextFloat()) * 0.4f;
                
                var swirl = new GenericGlowParticle(particlePos, (staffPos - particlePos).SafeNormalize(Vector2.Zero) * 1.5f,
                    particleColor, 0.12f, 18, true);
                MagnumParticleHandler.SpawnParticle(swirl);
            }
            
            Lighting.AddLight(staffPos, FateLensFlare.FatePurple.ToVector3() * 0.25f);
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            
            // Spawn avatar at cursor position
            Vector2 spawnPos = Main.MouseWorld;
            Projectile.NewProjectileDirect(source, spawnPos, Vector2.Zero, type, damage, knockback, player.whoAmI);
            
            // Summon VFX
            FateLensFlareDrawLayer.AddFlare(spawnPos, 1f, 0.8f, 25);
            FateLensFlare.KaleidoscopeBurst(spawnPos, 0.9f, 8);
            
            // Portal opening effect
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Color portalColor = FateLensFlare.GetFateGradient((float)i / 20f);
                Vector2 offset = angle.ToRotationVector2() * 50f;
                CustomParticles.GenericFlare(spawnPos + offset, portalColor * 0.6f, 0.3f, 18);
            }
            
            CustomParticles.HaloRing(spawnPos, FateLensFlare.FateDarkPink, 0.7f, 20);
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f }, spawnPos);
            
            return false;
        }
    }
    
    public class FateAvatarBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.StardustGuardianMinion;
        
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }
        
        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<FateAvatarMinion>()] > 0)
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
    
    public class FateAvatarMinion : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private enum AttackMode { SwordStrikes, DestinyOrbs, RealityRift }
        private AttackMode currentMode = AttackMode.SwordStrikes;
        
        private int attackTimer = 0;
        private const int ModeChangeDuration = 300; // 5 seconds per mode
        private int modeTimer = 0;
        
        private NPC targetNPC;
        private Vector2 idleOffset;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            Main.projPet[Projectile.type] = true;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 2f;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            
            idleOffset = new Vector2(Main.rand.NextFloat(-60f, 60f), Main.rand.NextFloat(-80f, -40f));
        }
        
        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => false;
        
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            // Check if should stay alive
            if (!owner.active || owner.dead)
            {
                owner.ClearBuff(ModContent.BuffType<FateAvatarBuff>());
                Projectile.Kill();
                return;
            }
            
            if (owner.HasBuff(ModContent.BuffType<FateAvatarBuff>()))
                Projectile.timeLeft = 2;
            
            // Find target
            targetNPC = FindTarget(1000f);
            
            // Movement
            UpdateMovement(owner);
            
            // Mode cycling
            modeTimer++;
            if (modeTimer >= ModeChangeDuration)
            {
                modeTimer = 0;
                currentMode = (AttackMode)(((int)currentMode + 1) % 3);
                
                // Mode change VFX
                FateLensFlareDrawLayer.AddFlare(Projectile.Center, 0.6f, 0.5f, 15);
                FateLensFlare.KaleidoscopeBurst(Projectile.Center, 0.5f, 6);
            }
            
            // Attack
            attackTimer++;
            if (targetNPC != null)
            {
                ExecuteAttack();
            }
            
            // === AVATAR VISUALS ===
            DrawAvatarEffect();
        }
        
        private void UpdateMovement(Player owner)
        {
            Vector2 targetPos;
            
            if (targetNPC != null)
            {
                // Move toward target but maintain distance
                float idealDist = 150f;
                Vector2 toTarget = (targetNPC.Center - Projectile.Center);
                float dist = toTarget.Length();
                
                if (dist > idealDist + 50f)
                    targetPos = targetNPC.Center - toTarget.SafeNormalize(Vector2.Zero) * idealDist;
                else if (dist < idealDist - 50f)
                    targetPos = Projectile.Center - toTarget.SafeNormalize(Vector2.Zero) * 50f;
                else
                    targetPos = Projectile.Center;
            }
            else
            {
                // Idle: float near player
                float bob = (float)Math.Sin(Main.GameUpdateCount * 0.03f + Projectile.whoAmI) * 10f;
                targetPos = owner.Center + idleOffset + new Vector2(0, bob);
            }
            
            // Smooth movement
            Vector2 toTarget2 = targetPos - Projectile.Center;
            float speed = Math.Min(toTarget2.Length() * 0.1f, 15f);
            Projectile.velocity = toTarget2.SafeNormalize(Vector2.Zero) * speed;
            
            // Chromatic afterimages while moving fast
            if (Projectile.velocity.Length() > 5f)
            {
                Color trailColor = FateLensFlare.GetFateGradient(Main.rand.NextFloat()) * 0.3f;
                CustomParticles.GenericFlare(Projectile.Center - Projectile.velocity * 0.5f, trailColor, 0.2f, 10);
            }
        }
        
        private void ExecuteAttack()
        {
            switch (currentMode)
            {
                case AttackMode.SwordStrikes:
                    if (attackTimer >= 30)
                    {
                        attackTimer = 0;
                        SpawnSwordStrike();
                    }
                    break;
                    
                case AttackMode.DestinyOrbs:
                    if (attackTimer >= 45)
                    {
                        attackTimer = 0;
                        SpawnDestinyOrb();
                    }
                    break;
                    
                case AttackMode.RealityRift:
                    if (attackTimer >= 90)
                    {
                        attackTimer = 0;
                        SpawnRealityRift();
                    }
                    break;
            }
        }
        
        private void SpawnSwordStrike()
        {
            if (targetNPC == null) return;
            
            Vector2 toTarget = (targetNPC.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
            
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, toTarget * 20f,
                ModContent.ProjectileType<AvatarSwordSlash>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            
            // Sword swing VFX
            FateLensFlareDrawLayer.AddFlare(Projectile.Center, 0.4f, 0.4f, 10);
            SoundEngine.PlaySound(SoundID.Item1 with { Pitch = 0.3f }, Projectile.Center);
        }
        
        private void SpawnDestinyOrb()
        {
            if (targetNPC == null) return;
            
            Vector2 toTarget = (targetNPC.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
            
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, toTarget * 12f,
                ModContent.ProjectileType<AvatarDestinyOrb>(), Projectile.damage, Projectile.knockBack / 2f, Projectile.owner);
            
            // Orb spawn VFX
            CustomParticles.GenericFlare(Projectile.Center, FateLensFlare.FateDarkPink, 0.5f, 12);
            CustomParticles.HaloRing(Projectile.Center, FateLensFlare.FatePurple, 0.3f, 10);
            SoundEngine.PlaySound(SoundID.Item8, Projectile.Center);
        }
        
        private void SpawnRealityRift()
        {
            if (targetNPC == null) return;
            
            // Spawn rift at target location
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), targetNPC.Center, Vector2.Zero,
                ModContent.ProjectileType<AvatarRealityRift>(), Projectile.damage * 2, 0f, Projectile.owner);
            
            // Rift creation VFX
            FateLensFlareDrawLayer.AddFlare(targetNPC.Center, 0.8f, 0.6f, 18);
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.3f, Volume = 0.6f }, targetNPC.Center);
        }
        
        private NPC FindTarget(float maxDist)
        {
            // Check player's target first
            Player owner = Main.player[Projectile.owner];
            if (owner.HasMinionAttackTargetNPC)
            {
                NPC playerTarget = Main.npc[owner.MinionAttackTargetNPC];
                if (playerTarget.active && !playerTarget.friendly && Vector2.Distance(Projectile.Center, playerTarget.Center) < maxDist)
                    return playerTarget;
            }
            
            // Find closest enemy
            NPC closest = null;
            float closestDist = maxDist;
            
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || npc.CountsAsACritter) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            return closest;
        }
        
        private void DrawAvatarEffect()
        {
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.15f + 0.85f;
            
            // Central avatar glow
            CustomParticles.GenericFlare(Projectile.Center, FateLensFlare.GetFateGradient((Main.GameUpdateCount * 0.01f) % 1f) * 0.5f, 0.4f * pulse, 8);
            
            // Orbiting kaleidoscope particles
            int orbitCount = 6;
            for (int i = 0; i < orbitCount; i++)
            {
                float orbitAngle = Main.GameUpdateCount * 0.04f + MathHelper.TwoPi * i / orbitCount;
                float orbitRadius = 35f + (float)Math.Sin(Main.GameUpdateCount * 0.08f + i) * 8f;
                Vector2 orbitPos = Projectile.Center + orbitAngle.ToRotationVector2() * orbitRadius;
                Color orbitColor = FateLensFlare.GetFateGradient((float)i / orbitCount);
                CustomParticles.GenericFlare(orbitPos, orbitColor * 0.4f, 0.15f, 6);
            }
            
            // Mode indicator color
            Color modeColor = currentMode switch
            {
                AttackMode.SwordStrikes => FateLensFlare.FateBrightRed,
                AttackMode.DestinyOrbs => FateLensFlare.FateDarkPink,
                AttackMode.RealityRift => FateLensFlare.FatePurple,
                _ => FateLensFlare.FateWhite
            };
            
            // Mode ring
            int ringPoints = 12;
            for (int i = 0; i < ringPoints; i++)
            {
                float ringAngle = MathHelper.TwoPi * i / ringPoints - Main.GameUpdateCount * 0.02f;
                Vector2 ringPos = Projectile.Center + ringAngle.ToRotationVector2() * 45f;
                CustomParticles.GenericFlare(ringPos, modeColor * 0.25f, 0.08f, 5);
            }
            
            // Periodic lens flare
            if (Main.GameUpdateCount % 15 == 0)
                FateLensFlareDrawLayer.AddFlare(Projectile.Center, 0.35f * pulse, 0.35f, 10);
            
            Lighting.AddLight(Projectile.Center, modeColor.ToVector3() * 0.4f * pulse);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            
            Color avatarColor = FateLensFlare.GetFateGradient((Main.GameUpdateCount * 0.01f) % 1f);
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.9f;
            
            // Outer ethereal glow
            sb.Draw(glow, Projectile.Center - Main.screenPosition, null, avatarColor * 0.3f,
                Main.GameUpdateCount * 0.02f, glow.Size() / 2f, 1.5f * pulse, SpriteEffects.None, 0f);
            
            // Inner core
            sb.Draw(glow, Projectile.Center - Main.screenPosition, null, FateLensFlare.FateWhite * 0.4f,
                0f, glow.Size() / 2f, 0.5f * pulse, SpriteEffects.None, 0f);
            
            return false;
        }
        
        public override void OnKill(int timeLeft)
        {
            FateLensFlareDrawLayer.AddFlare(Projectile.Center, 0.8f, 0.6f, 20);
            FateLensFlare.KaleidoscopeBurst(Projectile.Center, 0.6f, 6);
            CustomParticles.ExplosionBurst(Projectile.Center, FateLensFlare.FateDarkPink, 15, 5f);
        }
    }
    
    public class AvatarSwordSlash : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 40;
            Projectile.tileCollide = false;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Kaleidoscopic slash trail
            Color slashColor = FateLensFlare.GetFateGradient((Main.GameUpdateCount * 0.03f) % 1f);
            
            var trail = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.15f,
                slashColor * 0.5f, 0.3f, 12, true);
            MagnumParticleHandler.SpawnParticle(trail);
            
            // Wide arc particles
            Vector2 perp = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2);
            for (int side = -1; side <= 1; side += 2)
            {
                Vector2 arcPos = Projectile.Center + perp * side * 25f;
                Color arcColor = FateLensFlare.GetFateGradient(Main.rand.NextFloat()) * 0.4f;
                CustomParticles.GenericFlare(arcPos, arcColor, 0.15f, 8);
            }
            
            Lighting.AddLight(Projectile.Center, slashColor.ToVector3() * 0.4f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            
            // Trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                Color trailColor = FateLensFlare.GetFateGradient(progress) * (1f - progress) * 0.4f;
                
                sb.Draw(glow, Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition, null,
                    trailColor, Projectile.oldRot[i], glow.Size() / 2f, new Vector2(1f - progress * 0.5f, 0.3f), SpriteEffects.None, 0f);
            }
            
            // Main slash
            Color slashColor = FateLensFlare.GetFateGradient((Main.GameUpdateCount * 0.03f) % 1f);
            sb.Draw(glow, Projectile.Center - Main.screenPosition, null, slashColor * 0.6f,
                Projectile.rotation, glow.Size() / 2f, new Vector2(1.2f, 0.4f), SpriteEffects.None, 0f);
            
            return false;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 120);
            CustomParticles.GenericFlare(target.Center, FateLensFlare.FateBrightRed, 0.4f, 12);
        }
    }
    
    public class AvatarDestinyOrb : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private const float HomingStrength = 0.05f;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
        }
        
        public override void AI()
        {
            Projectile.rotation += 0.1f;
            
            // Mild homing
            NPC target = FindTarget(500f);
            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 14f, HomingStrength);
            }
            
            // Trail
            Color orbColor = FateLensFlare.GetFateGradient((Main.GameUpdateCount * 0.02f) % 1f);
            if (Main.rand.NextBool(2))
            {
                var trail = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f,
                    orbColor * 0.4f, 0.15f, 12, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            Lighting.AddLight(Projectile.Center, orbColor.ToVector3() * 0.3f);
        }
        
        private NPC FindTarget(float maxDist)
        {
            NPC closest = null;
            float closestDist = maxDist;
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || npc.CountsAsACritter) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            return closest;
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            
            // Trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                Color trailColor = FateLensFlare.GetFateGradient(progress) * (1f - progress) * 0.3f;
                sb.Draw(glow, Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition, null,
                    trailColor, 0f, glow.Size() / 2f, 0.25f * (1f - progress * 0.5f), SpriteEffects.None, 0f);
            }
            
            // Main orb
            Color orbColor = FateLensFlare.GetFateGradient((Main.GameUpdateCount * 0.02f) % 1f);
            sb.Draw(glow, Projectile.Center - Main.screenPosition, null, orbColor * 0.5f,
                Projectile.rotation, glow.Size() / 2f, 0.35f, SpriteEffects.None, 0f);
            sb.Draw(glow, Projectile.Center - Main.screenPosition, null, Color.White * 0.3f,
                0f, glow.Size() / 2f, 0.15f, SpriteEffects.None, 0f);
            
            return false;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 120);
        }
        
        public override void OnKill(int timeLeft)
        {
            FateLensFlare.KaleidoscopeBurst(Projectile.Center, 0.4f, 4);
            CustomParticles.ExplosionBurst(Projectile.Center, FateLensFlare.FateDarkPink, 8, 4f);
        }
    }
    
    public class AvatarRealityRift : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private const int RiftDuration = 120;
        
        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = RiftDuration;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }
        
        public override void AI()
        {
            float progress = 1f - (float)Projectile.timeLeft / RiftDuration;
            float openProgress = Math.Min(progress * 5f, 1f);
            float closeProgress = Math.Max((progress - 0.8f) * 5f, 0f);
            float intensity = openProgress * (1f - closeProgress);
            
            // Swirling rift visuals
            FateLensFlare.KaleidoscopeBurst(Projectile.Center, intensity * 0.4f, (int)(6 * intensity) + 1);
            
            // Edge particles
            int edgeCount = (int)(16 * intensity);
            for (int i = 0; i < edgeCount; i++)
            {
                float angle = Main.GameUpdateCount * 0.1f + MathHelper.TwoPi * i / edgeCount;
                float radius = 40f * intensity;
                Vector2 edgePos = Projectile.Center + angle.ToRotationVector2() * radius;
                Color edgeColor = FateLensFlare.GetFateGradient((float)i / edgeCount);
                CustomParticles.GenericFlare(edgePos, edgeColor * 0.4f, 0.12f, 6);
            }
            
            // Heat distortion
            FateLensFlare.DrawHeatWaveDistortion(Projectile.Center, 60f * intensity, 0.4f * intensity);
            
            // Lens flare
            if (Main.GameUpdateCount % 8 == 0)
                FateLensFlareDrawLayer.AddFlare(Projectile.Center, 0.4f * intensity, 0.4f, 10);
            
            Lighting.AddLight(Projectile.Center, FateLensFlare.FatePurple.ToVector3() * intensity * 0.5f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            
            float progress = 1f - (float)Projectile.timeLeft / RiftDuration;
            float openProgress = Math.Min(progress * 5f, 1f);
            float closeProgress = Math.Max((progress - 0.8f) * 5f, 0f);
            float intensity = openProgress * (1f - closeProgress);
            
            // Outer rift glow
            sb.Draw(glow, Projectile.Center - Main.screenPosition, null, FateLensFlare.FatePurple * 0.3f * intensity,
                Main.GameUpdateCount * 0.03f, glow.Size() / 2f, 1.2f * intensity, SpriteEffects.None, 0f);
            
            // Inner void
            sb.Draw(glow, Projectile.Center - Main.screenPosition, null, FateLensFlare.FateBlack * 0.5f * intensity,
                -Main.GameUpdateCount * 0.05f, glow.Size() / 2f, 0.6f * intensity, SpriteEffects.None, 0f);
            
            return false;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 180);
            CustomParticles.GenericFlare(target.Center, FateLensFlare.FateBrightRed, 0.4f, 12);
        }
    }
}
