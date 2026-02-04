using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.SwanLake.Debuffs;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons
{
    /// <summary>
    /// Feather of the Iridescent Flock - Summon weapon that summons 3 black/white crystals
    /// that orbit the player, fire flaming flares, and create explosive beams.
    /// Rainbow (Swan) rarity, no crafting recipe.
    /// </summary>
    public class FeatheroftheIridescentFlock : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 260;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 20;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(gold: 58);
            Item.rare = ModContent.RarityType<SwanRarity>();
            Item.UseSound = SoundID.Item30 with { Pitch = 0.6f, Volume = 0.7f }; // Fractal crystal sound
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<IridescentCrystal>();
            Item.buffType = ModContent.BuffType<IridescentFlockBuff>();
            Item.scale = 0.9f; // 90% size
        }

        public override void HoldItem(Player player)
        {
            // === UnifiedVFX SWAN LAKE AMBIENT AURA ===
            UnifiedVFX.SwanLake.Aura(player.Center, 28f, 0.22f);
            
            // === AMBIENT FRACTAL FLARES - dual-polarity with rainbow shimmer ===
            if (Main.rand.NextBool(7))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float radius = Main.rand.NextFloat(30f, 60f);
                Vector2 flarePos = player.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                Color baseColor = Main.rand.NextBool() ? UnifiedVFX.SwanLake.Black : UnifiedVFX.SwanLake.White;
                Color rainbow = UnifiedVFX.SwanLake.GetRainbow(Main.rand.NextFloat());
                Color fractalColor = Color.Lerp(baseColor, rainbow, 0.35f);
                CustomParticles.GenericFlare(flarePos, fractalColor, 0.3f, 20);
                ThemedParticles.SwanLakeFractalTrail(flarePos, 0.25f);
            }
            
            // Subtle feather particles when holding
            if (Main.rand.NextBool(10))
            {
                Color featherCol = Main.rand.NextBool() ? UnifiedVFX.SwanLake.White : UnifiedVFX.SwanLake.Black;
                CustomParticles.SwanFeatherDrift(player.Center + Main.rand.NextVector2Circular(22f, 22f), featherCol, 0.28f);
            }
            
            // Occasional rainbow shimmer
            if (Main.rand.NextBool(18))
            {
                Color rainbow = UnifiedVFX.SwanLake.GetRainbow(Main.rand.NextFloat()) * 0.5f;
                CustomParticles.GenericFlare(player.Center + Main.rand.NextVector2Circular(20f, 20f), rainbow, 0.18f, 12);
            }
            
            // Subtle pulsing rainbow light
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.1f + 0.45f;
            Vector3 lightColor = UnifiedVFX.SwanLake.GetRainbow(Main.GameUpdateCount * 0.015f).ToVector3();
            Lighting.AddLight(player.Center, lightColor * pulse * 0.45f);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position = Main.MouseWorld;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Apply buff
            player.AddBuff(Item.buffType, 2);

            // Summon 3 crystals at once
            for (int i = 0; i < 3; i++)
            {
                float angleOffset = MathHelper.TwoPi * i / 3f;
                // ai[0] = crystal index (0, 1, 2)
                // ai[1] = starting angle offset
                Projectile.NewProjectile(source, player.Center, Vector2.Zero, type, damage, knockback, player.whoAmI, i, angleOffset);
            }

            // Reduced summon effect - elegant not overwhelming
            ThemedParticles.SwanLakeRainbowExplosion(position, 0.72f);
            ThemedParticles.SwanLakeMusicalImpact(position, 0.54f, false);
            
            // Two subtle halo rings
            CustomParticles.HaloRing(position, Color.White * 0.6f, 0.3f, 12);
            CustomParticles.HaloRing(position, Color.Black * 0.5f, 0.25f, 10);
            
            // Subtle rainbow sparkles
            ThemedParticles.SwanLakeSparkles(position, 10, 30f);
            
            // Modest feather burst
            ThemedParticles.SwanLakeFeathers(position, 8, 35f);
            
            // Smaller feather explosion
            CustomParticles.SwanFeatherExplosion(position, 6, 0.35f);
            
            // Subtle rainbow flares - fewer and dimmer
            for (int i = 0; i < 6; i++)
            {
                float hue = i / 6f;
                Color flareColor = Main.hslToRgb(hue, 0.7f, 0.5f);
                CustomParticles.GenericFlare(position + Main.rand.NextVector2Circular(15f, 15f), flareColor * 0.5f, 0.4f, 15);
            }
            
            // Reduced spark burst
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                float hue = i / 16f;
                Color sparkColor = Main.hslToRgb(hue, 0.7f, 0.5f);
                Vector2 sparkVel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(4f, 8f);
                Dust spark = Dust.NewDustPerfect(position, DustID.Cloud, sparkVel, 0, sparkColor * 0.6f, 1.2f);
                spark.noGravity = true;
            }
            
            // Modest light burst
            Lighting.AddLight(position, 0.8f, 0.8f, 1f);
            
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 0.6f }, position);

            return false;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.1f + 1f;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // White feather glow
            spriteBatch.Draw(texture, position, null, Color.White * 0.35f, rotation, origin, scale * 0.9f * pulse * 1.3f, SpriteEffects.None, 0f);
            // Black shadow
            spriteBatch.Draw(texture, position, null, Color.Black * 0.25f, rotation, origin, scale * 0.9f * pulse * 1.5f, SpriteEffects.None, 0f);
            
            // Rainbow shimmer
            float hue = (float)Main.GameUpdateCount * 0.006f % 1f;
            Color rainbow = Main.hslToRgb(hue, 0.6f, 0.7f);
            spriteBatch.Draw(texture, position, null, rainbow * 0.2f, rotation, origin, scale * 0.9f * pulse * 1.15f, SpriteEffects.None, 0f);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            spriteBatch.Draw(texture, position, null, lightColor, rotation, origin, scale * 0.9f, SpriteEffects.None, 0f);
            
            Lighting.AddLight(Item.Center, 0.35f, 0.35f, 0.4f);
            
            return false;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            spriteBatch.Draw(texture, position, frame, drawColor, 0f, origin, scale * 0.9f, SpriteEffects.None, 0f);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Summons 3 orbiting crystals that fire flaming flares"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Crystals create explosive rainbow beams when targeting enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Resummoning stacks crystal power for faster attacks"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A flock of light dances at your command'") 
            { 
                OverrideColor = new Color(220, 225, 235) 
            });
        }
    }

    public class IridescentFlockBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<IridescentCrystal>()] > 0)
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

    public class IridescentCrystal : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/PrismaticSparkle11";

        private int crystalIndex => (int)Projectile.ai[0];
        private float baseAngleOffset => Projectile.ai[1];
        
        private bool isBlack => crystalIndex == 1; // Middle crystal is black
        private float orbitRadius = 80f;
        private float baseOrbitSpeed = 0.035f;
        private int attackTimer = 0;
        private int beamCooldown = 0;
        
        // Track summon count for scaling
        private int summonStackCount = 1;

        public override void SetStaticDefaults()
        {
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = false; // Don't sacrifice - instead stack
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 2;
            Projectile.minionSlots = 0.34f; // 3 crystals = ~1 slot
            Projectile.ignoreWater = true;
        }
        
        /// <summary>
        /// Gets the summon stack multiplier based on how many times this summon was used.
        /// More stacks = faster rotation and attacks!
        /// </summary>
        private float GetSummonStackMultiplier()
        {
            Player owner = Main.player[Projectile.owner];
            // Count how many sets of 3 crystals exist (each use spawns 3)
            int crystalCount = owner.ownedProjectileCounts[Type];
            int stackCount = Math.Max(1, crystalCount / 3);
            summonStackCount = stackCount;
            // Each additional stack adds 25% speed, capped at 3x (8 stacks)
            return 1f + (stackCount - 1) * 0.25f;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            // Check if the buff is active
            if (!CheckActive(owner))
                return;
            
            // Get scaling multiplier based on stack count
            float stackMultiplier = GetSummonStackMultiplier();
            float orbitSpeed = baseOrbitSpeed * stackMultiplier;

            // Orbit around player - faster with more stacks!
            float orbitAngle = baseAngleOffset + (float)Main.GameUpdateCount * orbitSpeed;
            Vector2 targetPos = owner.Center + orbitAngle.ToRotationVector2() * orbitRadius;
            
            // Smooth movement to orbit position
            Vector2 toTarget = targetPos - Projectile.Center;
            Projectile.velocity = toTarget * 0.15f;
            Projectile.Center += Projectile.velocity;
            
            // Rotation
            Projectile.rotation = orbitAngle + MathHelper.PiOver2;

            // Visual trail
            SpawnOrbitTrail();

            // Find target
            NPC target = FindClosestNPC(600f);
            
            // Attack rates scale with stacks!
            int flareAttackRate = Math.Max(10, (int)(30 / stackMultiplier)); // Faster with more stacks, min 10 ticks
            int beamAttackRate = Math.Max(50, (int)(150 / stackMultiplier)); // Faster with more stacks, min 50 ticks
            
            if (target != null)
            {
                attackTimer++;
                
                // Fire flares - faster with more stacks!
                if (attackTimer >= flareAttackRate)
                {
                    attackTimer = 0;
                    FireFlare(target);
                }
                
                // Fire beam - faster with more stacks!
                beamCooldown++;
                if (beamCooldown >= beamAttackRate)
                {
                    beamCooldown = 0;
                    FireExplosiveBeam(target);
                }
            }
            else
            {
                attackTimer = 0;
            }

            // === RAINBOW LIGHTING - brighter with more stacks! ===
            float hue = (Main.GameUpdateCount * 0.02f + crystalIndex * 0.33f) % 1f;
            Vector3 lightColor = Main.hslToRgb(hue, 0.5f + stackMultiplier * 0.1f, 0.4f + stackMultiplier * 0.1f).ToVector3();
            float intensity = isBlack ? 0.2f : 0.35f;
            intensity *= (0.8f + stackMultiplier * 0.2f); // Brighter with more stacks
            Lighting.AddLight(Projectile.Center, lightColor * intensity);
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<IridescentFlockBuff>());
                Projectile.Kill();
                return false;
            }

            if (owner.HasBuff(ModContent.BuffType<IridescentFlockBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            return true;
        }

        private void SpawnOrbitTrail()
        {
            // === MASSIVE EXPLOSIVE RAINBOW ORBIT TRAIL ===
            
            // Constant rainbow sparkle trail - MUCH MORE!
            for (int i = 0; i < 2; i++)
            {
                float hue = (Main.GameUpdateCount * 0.04f + crystalIndex * 0.33f + i * 0.2f) % 1f;
                Color rainbowCol = Main.hslToRgb(hue, 1f, 0.75f);
                Dust rainbow = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f), DustID.RainbowTorch, 
                    -Projectile.velocity * 0.5f + Main.rand.NextVector2Circular(2f, 2f),
                    0, rainbowCol, 1.8f);
                rainbow.noGravity = true;
                rainbow.fadeIn = 1.5f;
            }
            
            // HEAVY black/white core trail
            for (int i = 0; i < 2; i++)
            {
                Color col = isBlack ? new Color(20, 20, 30) : new Color(255, 255, 255);
                int dustType = isBlack ? DustID.Shadowflame : DustID.WhiteTorch;
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), dustType, 
                    -Projectile.velocity * 0.4f + Main.rand.NextVector2Circular(1.5f, 1.5f),
                    isBlack ? 100 : 0, col, 1.5f);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }
            
            // Frequent rainbow flares along orbit!
            if (Main.rand.NextBool(3))
            {
                float flareHue = Main.rand.NextFloat();
                Color flareColor = Main.hslToRgb(flareHue, 1f, 0.75f);
                CustomParticles.GenericFlare(Projectile.Center, flareColor, 0.55f, 20);
            }
            
            // Pearlescent shimmer flares
            if (Main.rand.NextBool(4))
            {
                CustomParticles.SwanLakeFlare(Projectile.Center, 0.45f);
            }
            
            // Swan feather trail on orbit
            if (Main.rand.NextBool(6))
            {
                CustomParticles.SwanFeatherTrail(Projectile.Center, Projectile.velocity, 0.25f);
            }
            
            // Occasional mini explosion burst
            if (Main.rand.NextBool(12))
            {
                for (int i = 0; i < 6; i++)
                {
                    float hue = i / 6f;
                    Color burstColor = Main.hslToRgb(hue, 1f, 0.7f);
                    Vector2 burstVel = Main.rand.NextVector2Circular(4f, 4f);
                    Dust burst = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowTorch, burstVel, 0, burstColor, 1.6f);
                    burst.noGravity = true;
                }
                CustomParticles.HaloRing(Projectile.Center, Main.rand.NextBool() ? Color.White : Color.Black, 0.3f, 12);
            }
            
            // Ambient fractal gem sparkle - signature Swan Lake effect
            if (Main.rand.NextBool(10))
            {
                ThemedParticles.SwanLakeFractalTrail(Projectile.Center, 0.35f);
            }
            
            // ☁EMUSICAL NOTATION - Swan Lake graceful melody
            if (Main.rand.NextBool(8))
            {
                float hue = (Main.GameUpdateCount * 0.01f + Main.rand.NextFloat()) % 1f;
                Color noteColor = Main.hslToRgb(hue, 0.8f, 0.9f);
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.3f, 30);
            }
        }

        private NPC FindClosestNPC(float maxDistance)
        {
            NPC closest = null;
            float closestDist = maxDistance;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy(this))
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist && Collision.CanHitLine(Projectile.Center, 1, 1, npc.Center, 1, 1))
                    {
                        closest = npc;
                        closestDist = dist;
                    }
                }
            }

            return closest;
        }

        private void FireFlare(NPC target)
        {
            Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
            Vector2 velocity = direction * 16f;
            
            // ai[0] = isBlack
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity,
                ModContent.ProjectileType<IridescentFlare>(), Projectile.damage / 2, Projectile.knockBack, Projectile.owner, isBlack ? 1 : 0);
            
            // === MASSIVE EXPLOSIVE RAINBOW FIRING BURST ===
            
            // HUGE rainbow flare burst on fire!
            for (int i = 0; i < 14; i++)
            {
                float hue = i / 14f;
                Color flareColor = Main.hslToRgb(hue, 1f, 0.75f);
                CustomParticles.GenericFlare(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f), flareColor, 0.75f, 25);
            }
            
            // MASSIVE rainbow spark burst in firing direction!
            for (int i = 0; i < 24; i++)
            {
                float hue = i / 24f;
                Color sparkColor = Main.hslToRgb(hue, 1f, 0.7f);
                Vector2 sparkVel = direction.RotatedBy(Main.rand.NextFloat(-0.7f, 0.7f)) * Main.rand.NextFloat(5f, 12f);
                Dust spark = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowTorch, sparkVel, 0, sparkColor, 2.0f);
                spark.noGravity = true;
                spark.fadeIn = 1.4f;
            }
            
            // Black/white contrast burst
            for (int i = 0; i < 12; i++)
            {
                Color col = i % 2 == 0 ? Color.White : Color.Black;
                int dustType = i % 2 == 0 ? DustID.WhiteTorch : DustID.Shadowflame;
                Vector2 vel = direction.RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.NextFloat(4f, 9f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, dustType, vel, i % 2 == 0 ? 0 : 100, col, 1.8f);
                d.noGravity = true;
            }
            
            // Enhanced themed particles
            ThemedParticles.SwanLakeSparks(Projectile.Center, direction, 12, 8f);
            ThemedParticles.SwanLakeSparkles(Projectile.Center, 10, 25f);
            ThemedParticles.SwanLakeBloomBurst(Projectile.Center, 0.7f);
            
            // Swan feather burst on fire!
            CustomParticles.SwanFeatherBurst(Projectile.Center, 5, 0.3f);
            
            // Multiple muzzle flash halos!
            CustomParticles.HaloRing(Projectile.Center, Color.White, 0.7f, 25);
            CustomParticles.HaloRing(Projectile.Center, Color.Black, 0.5f, 20);
            for (int ring = 0; ring < 3; ring++)
            {
                float hue = (Main.GameUpdateCount * 0.02f + ring * 0.33f) % 1f;
                Color ringColor = Main.hslToRgb(hue, 1f, 0.7f);
                CustomParticles.HaloRing(Projectile.Center, ringColor, 0.4f + ring * 0.15f, 15 + ring * 3);
            }
            
            // Bright muzzle light
            Lighting.AddLight(Projectile.Center, 1.2f, 1.2f, 1.5f);
            
            SoundEngine.PlaySound(SoundID.Item12 with { Volume = 0.7f, Pitch = 0.3f }, Projectile.Center);
        }

        private void FireExplosiveBeam(NPC target)
        {
            // Create a beam from crystal to target
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                ModContent.ProjectileType<IridescentBeam>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 
                target.whoAmI, isBlack ? 1 : 0);
            
            // === RAINBOW BEAM CHARGING EXPLOSION ===
            
            // Rainbow explosion at charge point!
            ThemedParticles.SwanLakeRainbowExplosion(Projectile.Center, 1.44f);
            ThemedParticles.SwanLakeMusicalImpact(Projectile.Center, 1.08f, true);
            
            // Rainbow shockwave rings (50% reduced)!
            for (int ring = 0; ring < 4; ring++)
            {
                float hue = (Main.GameUpdateCount * 0.02f + ring * 0.25f) % 1f;
                Color ringColor = Main.hslToRgb(hue, 1f, 0.75f);
                CustomParticles.HaloRing(Projectile.Center, ringColor, 0.4f + ring * 0.1f, 15 + ring * 4);
            }
            CustomParticles.HaloRing(Projectile.Center, Color.White, 0.75f, 25);
            CustomParticles.HaloRing(Projectile.Center, Color.Black, 0.6f, 22);
            
            // Rainbow sparkle flares!
            ThemedParticles.SwanLakeSparkles(Projectile.Center, 35, 50f);
            
            // Swan feather spiral on beam charge!
            CustomParticles.SwanFeatherSpiral(Projectile.Center, Color.White, 10);
            
            // Rainbow flare burst!
            for (int i = 0; i < 16; i++)
            {
                float hue = i / 16f;
                Color flareColor = Main.hslToRgb(hue, 1f, 0.8f);
                CustomParticles.GenericFlare(Projectile.Center + Main.rand.NextVector2Circular(22f, 22f), flareColor, 0.7f, 28);
            }
            
            // HUGE rainbow sparks toward target - like a rainbow meteor shower!
            Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
            for (int i = 0; i < 36; i++)
            {
                float hue = i / 36f;
                Color sparkColor = Main.hslToRgb(hue, 1f, 0.7f);
                Vector2 sparkVel = toTarget.RotatedBy(Main.rand.NextFloat(-1.2f, 1.2f)) * Main.rand.NextFloat(6f, 16f);
                Dust spark = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowTorch, sparkVel, 0, sparkColor, 2.5f);
                spark.noGravity = true;
                spark.fadeIn = 1.6f;
            }
            
            // Enhanced themed particles
            ThemedParticles.SwanLakeShockwave(Projectile.Center, 1.5f);
            ThemedParticles.SwanLakeMusicNotes(Projectile.Center, 15, 50f);
            ThemedParticles.SwanLakeAccidentals(Projectile.Center, 8, 40f);
            ThemedParticles.SwanLakeFeathers(Projectile.Center, 15, 55f);
            
            // EXPLOSIVE Black/white contrast burst!
            for (int i = 0; i < 20; i++)
            {
                Color col = i % 2 == 0 ? Color.White : Color.Black;
                int dustType = i % 2 == 0 ? DustID.WhiteTorch : DustID.Shadowflame;
                Vector2 vel = toTarget * Main.rand.NextFloat(5f, 12f) + Main.rand.NextVector2Circular(5f, 5f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, dustType, vel, i % 2 == 0 ? 0 : 100, col, 2.2f);
                d.noGravity = true;
                d.fadeIn = 1.5f;
            }
            
            // MASSIVE bright light explosion!
            Lighting.AddLight(Projectile.Center, 2.5f, 2.5f, 3f);
            
            SoundEngine.PlaySound(SoundID.Item122 with { Volume = 1.0f, Pitch = 0f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Volume = 0.8f, Pitch = 0.3f }, Projectile.Center);
        }

        public override bool? CanDamage() => false; // Crystal itself doesn't deal damage

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f + baseAngleOffset) * 0.15f + 1f;
            
            Color mainColor = isBlack ? new Color(30, 30, 40) : new Color(255, 255, 255);
            Color glowColor = isBlack ? Color.Black * 0.4f : Color.White * 0.5f;
            
            // Outer rainbow glow - cycling through spectrum (REDUCED BRIGHTNESS)
            float hue1 = (Main.GameUpdateCount * 0.02f + crystalIndex * 0.33f) % 1f;
            Color rainbow1 = Main.hslToRgb(hue1, 0.6f, 0.5f);
            Main.EntitySpriteDraw(texture, drawPos, null, rainbow1 * 0.25f, Projectile.rotation, origin, pulse * 1.6f, SpriteEffects.None, 0);
            
            // Second rainbow layer (offset hue) - REDUCED
            float hue2 = (hue1 + 0.33f) % 1f;
            Color rainbow2 = Main.hslToRgb(hue2, 0.5f, 0.45f);
            Main.EntitySpriteDraw(texture, drawPos, null, rainbow2 * 0.2f, Projectile.rotation, origin, pulse * 1.35f, SpriteEffects.None, 0);
            
            // Third rainbow layer (further offset) - REDUCED
            float hue3 = (hue1 + 0.66f) % 1f;
            Color rainbow3 = Main.hslToRgb(hue3, 0.5f, 0.4f);
            Main.EntitySpriteDraw(texture, drawPos, null, rainbow3 * 0.15f, Projectile.rotation, origin, pulse * 1.15f, SpriteEffects.None, 0);
            
            // White/black core glow - REDUCED
            Main.EntitySpriteDraw(texture, drawPos, null, glowColor * 0.35f, Projectile.rotation, origin, pulse * 1.1f, SpriteEffects.None, 0);
            
            // Main sprite
            Main.EntitySpriteDraw(texture, drawPos, null, mainColor, Projectile.rotation, origin, pulse, SpriteEffects.None, 0);

            return false;
        }
    }

    public class IridescentFlare : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/PrismaticSparkle14";

        private bool isBlack => Projectile.ai[0] == 1;
        private int trailTimer = 0;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            trailTimer++;

            // === HOMING/TRACKING BEHAVIOR ===
            float homingRange = 350f;
            float homingStrength = 0.1f;
            NPC closestNPC = null;
            float closestDist = homingRange;
            
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.CanBeChasedBy() && !npc.friendly)
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestNPC = npc;
                    }
                }
            }
            
            if (closestNPC != null)
            {
                Vector2 targetDir = (closestNPC.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetDir * Projectile.velocity.Length(), homingStrength);
            }

            // === ABSOLUTELY MASSIVE RAINBOW FLARE TRAIL - EXPLOSIVE! ===
            
            // CONSTANT rainbow flare particles along the path - HEAVY!
            if (trailTimer % 2 == 0)
            {
                float hue = (Main.GameUpdateCount * 0.03f + trailTimer * 0.15f) % 1f;
                Color rainbowFlare = Main.hslToRgb(hue, 1f, 0.75f);
                CustomParticles.GenericFlare(Projectile.Center, rainbowFlare, 0.8f, 28);
            }
            
            // HEAVY constant rainbow sparkle trail
            for (int i = 0; i < 2; i++)
            {
                float sparkleHue = (Main.GameUpdateCount * 0.04f + i * 0.5f) % 1f;
                Color sparkleColor = Main.hslToRgb(sparkleHue, 1f, 0.8f);
                Dust sparkle = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), DustID.RainbowTorch,
                    -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(2f, 2f), 0, sparkleColor, 1.9f);
                sparkle.noGravity = true;
                sparkle.fadeIn = 1.5f;
            }
            ThemedParticles.SwanLakeSparkles(Projectile.Center, 3, 12f);
            
            // MASSIVE rainbow glow particles trailing behind
            for (int i = 0; i < 2; i++)
            {
                float trailHue = Main.rand.NextFloat();
                Color trailRainbow = Main.hslToRgb(trailHue, 1f, 0.75f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowTorch, 
                    -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(2f, 2f),
                    0, trailRainbow, 2.0f);
                d.noGravity = true;
                d.fadeIn = 1.4f;
            }
            
            // HEAVY Black/White core trail
            for (int i = 0; i < 2; i++)
            {
                Color coreColor = isBlack ? new Color(20, 20, 30) : Color.White;
                int dustType = isBlack ? DustID.Shadowflame : DustID.WhiteTorch;
                Dust d = Dust.NewDustPerfect(Projectile.Center, dustType, 
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f),
                    isBlack ? 100 : 0, coreColor, 1.6f);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }
            
            // FREQUENT explosive mini flare bursts along path!
            if (trailTimer % 4 == 0)
            {
                for (int i = 0; i < 8; i++)
                {
                    float burstHue = (i / 8f + Main.GameUpdateCount * 0.03f) % 1f;
                    Color burstColor = Main.hslToRgb(burstHue, 1f, 0.7f);
                    Vector2 burstVel = Main.rand.NextVector2Circular(4f, 4f);
                    Dust burst = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowTorch, burstVel, 0, burstColor, 1.8f);
                    burst.noGravity = true;
                }
                // Mini halo rings along path
                float ringHue = (trailTimer * 0.05f) % 1f;
                CustomParticles.HaloRing(Projectile.Center, Main.hslToRgb(ringHue, 1f, 0.7f), 0.3f, 12);
            }
            
            // Pearlescent shimmer flares
            if (Main.rand.NextBool(3))
            {
                CustomParticles.SwanLakeFlare(Projectile.Center, 0.45f);
            }
            
            // Ambient fractal gem sparkle
            if (Main.rand.NextBool(6))
            {
                ThemedParticles.SwanLakeFractalTrail(Projectile.Center, 0.5f);
            }
            
            // ☁EMUSICAL NOTATION - Swan Lake graceful melody
            if (Main.rand.NextBool(5))
            {
                float hue = (Main.GameUpdateCount * 0.01f + Main.rand.NextFloat()) % 1f;
                Color noteColor = Main.hslToRgb(hue, 0.8f, 0.9f);
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.35f, 35);
            }

            // BRIGHT rainbow lighting - cycles through spectrum
            float lightHue = (Main.GameUpdateCount * 0.025f) % 1f;
            Vector3 lightColor = Main.hslToRgb(lightHue, 0.9f, 0.7f).ToVector3();
            Lighting.AddLight(Projectile.Center, lightColor * 1.3f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<FlameOfTheSwan>(), 180); // 3 seconds
            
            // === SEEKING CRYSTALS - 33% chance on summon hit ===
            if (Main.rand.NextBool(3))
            {
                SeekingCrystalHelper.SpawnSwanLakeCrystals(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Projectile.velocity,
                    (int)(damageDone * 0.2f),
                    Projectile.knockBack,
                    Projectile.owner,
                    4
                );
            }

            // === MASSIVE RAINBOW EXPLOSION ON IMPACT ===
            CreateMassiveRainbowExplosion(target.Center);
            
            // === EXTRA CUSTOM FLARES ON HIT! ===
            // Rainbow flare burst!
            for (int i = 0; i < 10; i++)
            {
                float hue = i / 10f;
                Color flareColor = Main.hslToRgb(hue, 1f, 0.85f);
                CustomParticles.GenericFlare(target.Center + Main.rand.NextVector2Circular(20f, 20f), flareColor, 0.6f, 22);
            }
            
            // Swan Lake flare
            CustomParticles.SwanLakeFlare(target.Center, 0.55f);
            
            // ☁EMUSICAL IMPACT - Swan's graceful chord
            ThemedParticles.MusicNoteBurst(target.Center, Color.White, 6, 4f);
            
            // Extra feathers and sparkles!
            ThemedParticles.SwanLakeFeathers(target.Center, 6, 30f);
            ThemedParticles.SwanLakeSparkles(target.Center, 20, 35f);
            
            // Fractal gem burst on hit!
            ThemedParticles.SwanLakeFractalGemBurst(target.Center, isBlack ? Color.Black : Color.White, 0.8f, 6, false);
        }

        public override void OnKill(int timeLeft)
        {
            // === RAINBOW EXPLOSION WHEN PROJECTILE DIES ===
            CreateMassiveRainbowExplosion(Projectile.Center);
        }
        
        private void CreateMassiveRainbowExplosion(Vector2 position)
        {
            // === RAINBOW EXPLOSION (75% size - reduced 25%)! ===
            SoundEngine.PlaySound(SoundID.Item27 with { Pitch = 0.2f, Volume = 0.7f }, position);
            SoundEngine.PlaySound(SoundID.Item107 with { Volume = 0.6f, Pitch = 0.3f }, position);
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Volume = 0.4f, Pitch = 0.5f }, position);
            
            // Core rainbow explosion (75% scale)!
            ThemedParticles.SwanLakeRainbowExplosion(position, 1.485f);
            ThemedParticles.SwanLakeMusicalImpact(position, 1.215f, true);
            
            // Rainbow flare bursts (75% count/size)!
            for (int i = 0; i < 14; i++)
            {
                float hue = i / 14f;
                Color flareColor = Main.hslToRgb(hue, 1f, 0.8f);
                CustomParticles.GenericFlare(position + Main.rand.NextVector2Circular(19f, 19f), flareColor, 0.825f, 26);
            }
            
            // Rainbow spark burst - radial explosion (75% count/size)!
            for (int i = 0; i < 36; i++)
            {
                float angle = MathHelper.TwoPi * i / 36f;
                float hue = i / 36f;
                Color sparkColor = Main.hslToRgb(hue, 1f, 0.7f);
                float speed = Main.rand.NextFloat(6f, 13.5f);
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                
                Dust spark = Dust.NewDustPerfect(position, DustID.RainbowTorch, vel, 0, sparkColor, 2.1f);
                spark.noGravity = true;
                spark.fadeIn = 1.3f;
            }
            
            // Golden and white inner burst (75% count/size)!
            for (int i = 0; i < 21; i++)
            {
                float angle = MathHelper.TwoPi * i / 21f;
                float speed = Main.rand.NextFloat(3.75f, 9f);
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                
                Color col = i % 2 == 0 ? Color.White : new Color(255, 220, 150);
                Dust inner = Dust.NewDustPerfect(position, DustID.WhiteTorch, vel, 0, col, 1.875f);
                inner.noGravity = true;
                inner.fadeIn = 1.1f;
            }
            
            // Pearlescent shimmer particles (75% size)
            ThemedParticles.SwanLakeSparkles(position, 19, 49f);
            ThemedParticles.SwanLakeFeathers(position, 9, 38f);
            
            // Musical note burst (75% size)!
            ThemedParticles.SwanLakeMusicNotes(position, 11, 41f);
            ThemedParticles.SwanLakeAccidentals(position, 6, 34f);
            
            // ☁EMUSICAL FINALE - Feathered symphony
            float finaleHue = (Main.GameUpdateCount * 0.02f) % 1f;
            Color finaleColor = Main.hslToRgb(finaleHue, 0.9f, 0.85f);
            ThemedParticles.MusicNoteBurst(position, finaleColor, 6, 4f);
            
            // Black/white contrast burst (75% count/size)!
            for (int i = 0; i < 15; i++)
            {
                Color col = i % 2 == 0 ? Color.White : Color.Black;
                int dustType = i % 2 == 0 ? DustID.WhiteTorch : DustID.Shadowflame;
                Vector2 vel = Main.rand.NextVector2Circular(5.25f, 5.25f);
                Dust d = Dust.NewDustPerfect(position, dustType, vel, i % 2 == 0 ? 0 : 100, col, 1.65f);
                d.noGravity = true;
                d.fadeIn = 1.05f;
            }
            
            // Halo ring effects (75% of reduced size)!
            for (int ring = 0; ring < 3; ring++)
            {
                float hue = (Main.GameUpdateCount * 0.02f + ring * 0.25f) % 1f;
                Color ringColor = Main.hslToRgb(hue, 1f, 0.75f);
                CustomParticles.HaloRing(position, ringColor, 0.34f + ring * 0.075f, 14 + ring * 3);
            }
            CustomParticles.HaloRing(position, Color.White, 0.49f, 19);
            CustomParticles.HaloRing(position, Color.Black, 0.375f, 17);
            
            // Rainbow sparkle flares (75% size)!
            ThemedParticles.SwanLakeSparkles(position, 19, 34f);
            
            // Rainbow light burst (75% intensity)!
            float lightHue = (Main.GameUpdateCount * 0.02f) % 1f;
            Vector3 lightColor = Main.hslToRgb(lightHue, 0.9f, 0.8f).ToVector3();
            Lighting.AddLight(position, lightColor * 1.875f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            // === RAINBOW TRAIL DRAWING ===
            // Draw afterimages with rainbow gradient
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                float progress = (float)i / Projectile.oldPos.Length;
                float hue = (Main.GameUpdateCount * 0.02f + progress * 0.5f) % 1f;
                Color rainbowTrail = Main.hslToRgb(hue, 0.9f, 0.7f) * (1f - progress) * 0.8f;
                float trailScale = 1.2f * (1f - progress * 0.6f);
                
                Main.EntitySpriteDraw(texture, trailPos, null, rainbowTrail, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0);
            }
            
            // Outer rainbow glow
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.15f + 1f;
            float hueGlow = (Main.GameUpdateCount * 0.03f) % 1f;
            Color rainbowGlow = Main.hslToRgb(hueGlow, 0.85f, 0.65f);
            Main.EntitySpriteDraw(texture, drawPos, null, rainbowGlow * 0.7f, Projectile.rotation, origin, 1.8f * pulse, SpriteEffects.None, 0);
            
            // Second rainbow layer (offset hue)
            Color rainbowGlow2 = Main.hslToRgb((hueGlow + 0.33f) % 1f, 0.8f, 0.6f);
            Main.EntitySpriteDraw(texture, drawPos, null, rainbowGlow2 * 0.5f, Projectile.rotation, origin, 1.5f * pulse, SpriteEffects.None, 0);
            
            // White/black core glow
            Color coreColor = isBlack ? Color.Black * 0.8f : Color.White * 0.9f;
            Main.EntitySpriteDraw(texture, drawPos, null, coreColor, Projectile.rotation, origin, 1.3f, SpriteEffects.None, 0);
            
            // Main sprite
            Color mainColor = isBlack ? new Color(40, 40, 50) : new Color(255, 255, 255);
            Main.EntitySpriteDraw(texture, drawPos, null, mainColor, Projectile.rotation, origin, 1f, SpriteEffects.None, 0);

            return false;
        }
    }

    public class IridescentBeam : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow3";

        private int targetNPC => (int)Projectile.ai[0];
        private bool isBlack => Projectile.ai[1] == 1;
        
        private int beamTimer = 0;
        private const int BEAM_DURATION = 40;

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = BEAM_DURATION;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            beamTimer++;
            
            NPC target = Main.npc[targetNPC];
            if (!target.active || target.life <= 0)
            {
                Projectile.Kill();
                return;
            }

            // Beam visual effect along the path
            Vector2 start = Projectile.Center;
            Vector2 end = target.Center;
            Vector2 direction = (end - start).SafeNormalize(Vector2.Zero);
            float distance = Vector2.Distance(start, end);

            // === RAINBOW FLARE BEAM ===
            // Spawn rainbow flares along the beam path
            int particleCount = (int)(distance / 12f);
            for (int i = 0; i < particleCount; i++)
            {
                Vector2 pos = start + direction * (i * 12f);
                
                // Rainbow flare particles along beam
                if (Main.rand.NextBool(2))
                {
                    float hue = (i / (float)particleCount + Main.GameUpdateCount * 0.02f) % 1f;
                    Color rainbowCol = Main.hslToRgb(hue, 0.9f, 0.7f);
                    
                    Dust rainbow = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(8f, 8f), DustID.RainbowTorch,
                        Main.rand.NextVector2Circular(1.5f, 1.5f), 0, rainbowCol, 1.4f);
                    rainbow.noGravity = true;
                }
                
                // Black/white core particles
                if (Main.rand.NextBool(4))
                {
                    Color col = i % 2 == 0 ? Color.White : Color.Black;
                    int dustType = i % 2 == 0 ? DustID.WhiteTorch : DustID.Smoke;
                    Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(5f, 5f), dustType,
                        Main.rand.NextVector2Circular(1f, 1f), i % 2 == 0 ? 0 : 150, col, 1.2f);
                    d.noGravity = true;
                }
            }
            
            // Spawn rainbow flares at intervals along beam
            if (beamTimer % 4 == 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    float t = i / 5f;
                    Vector2 flarePos = Vector2.Lerp(start, end, t);
                    float hue = (t + Main.GameUpdateCount * 0.03f) % 1f;
                    Color flareColor = Main.hslToRgb(hue, 1f, 0.7f);
                    CustomParticles.GenericFlare(flarePos, flareColor, 0.4f, 15);
                }
                
                // ☁EMUSICAL NOTATION - Swan Lake graceful melody along beam
                Vector2 notePos = Vector2.Lerp(start, end, Main.rand.NextFloat(0.2f, 0.8f));
                float noteHue = (Main.GameUpdateCount * 0.01f + Main.rand.NextFloat()) % 1f;
                Color noteColor = Main.hslToRgb(noteHue, 0.8f, 0.9f);
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f);
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.35f, 35);
            }

            // Explosive hit at target location periodically - MORE OFTEN
            if (beamTimer % 8 == 0)
            {
                CreateExplosion(target.Center);
            }

            // Rainbow light along beam
            for (int i = 0; i < particleCount; i += 2)
            {
                Vector2 pos = start + direction * (i * 12f);
                float hue = (i / (float)particleCount + Main.GameUpdateCount * 0.02f) % 1f;
                Vector3 lightColor = Main.hslToRgb(hue, 0.8f, 0.6f).ToVector3();
                Lighting.AddLight(pos, lightColor * 0.6f);
            }
        }

        private void CreateExplosion(Vector2 position)
        {
            // === MASSIVE RAINBOW BEAM EXPLOSION ===
            
            // Core rainbow explosion with multiple rings
            ThemedParticles.SwanLakeRainbowExplosion(position, 1.35f);
            
            // Multiple rainbow halo rings
            for (int ring = 0; ring < 3; ring++)
            {
                float hue = (Main.GameUpdateCount * 0.02f + ring * 0.33f) % 1f;
                Color ringColor = Main.hslToRgb(hue, 0.9f, 0.7f);
                CustomParticles.HaloRing(position, ringColor, 0.6f + ring * 0.2f, 25 + ring * 5);
            }
            
            // Rainbow flare burst
            for (int i = 0; i < 12; i++)
            {
                float hue = i / 12f;
                Color flareColor = Main.hslToRgb(hue, 1f, 0.7f);
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                CustomParticles.GenericFlare(position + offset, flareColor, 0.65f, 22);
            }
            
            // Huge rainbow spark spiral
            for (int i = 0; i < 32; i++)
            {
                float angle = MathHelper.TwoPi * i / 32f;
                float hue = i / 32f;
                Color sparkColor = Main.hslToRgb(hue, 1f, 0.65f);
                float speed = Main.rand.NextFloat(4f, 9f);
                Vector2 sparkVel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                
                Dust spark = Dust.NewDustPerfect(position, DustID.RainbowTorch, sparkVel, 0, sparkColor, 2.2f);
                spark.noGravity = true;
                spark.fadeIn = 1.5f;
            }
            
            // Pearlescent shimmer
            ThemedParticles.SwanLakeSparkles(position, 16, 50f);
            
            // Musical notes explosion
            ThemedParticles.SwanLakeMusicNotes(position, 8, 40f);
            
            // ☁EMUSICAL IMPACT - Swan's graceful chord
            ThemedParticles.MusicNoteBurst(position, Color.White, 5, 3.5f);
            
            // Black and white contrast core
            for (int i = 0; i < 12; i++)
            {
                Color col = i % 2 == 0 ? Color.White : Color.Black;
                Dust d = Dust.NewDustPerfect(position, i % 2 == 0 ? DustID.WhiteTorch : DustID.Smoke,
                    Main.rand.NextVector2Circular(5f, 5f), i % 2 == 0 ? 0 : 180, col, 1.4f);
                d.noGravity = true;
            }
            
            // Massive rainbow lighting burst
            float lightHue = (Main.GameUpdateCount * 0.02f) % 1f;
            Vector3 lightColor = Main.hslToRgb(lightHue, 0.85f, 0.75f).ToVector3();
            Lighting.AddLight(position, lightColor * 2f);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // Beam always hits
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<FlameOfTheSwan>(), 300); // 5 seconds
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            NPC target = Main.npc[targetNPC];
            if (!target.active) return false;

            // Check collision along the beam line
            float point = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                Projectile.Center, target.Center, 20, ref point);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // === RAINBOW BEAM DRAWING ===
            NPC target = Main.npc[targetNPC];
            if (!target.active) return false;

            Vector2 start = Projectile.Center - Main.screenPosition;
            Vector2 end = target.Center - Main.screenPosition;
            
            float beamWidth = 12f + (float)Math.Sin(beamTimer * 0.3f) * 5f;
            
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 direction = (end - start).SafeNormalize(Vector2.Zero);
            float rotation = direction.ToRotation();
            float distance = Vector2.Distance(start, end);
            
            // Draw multiple rainbow layers
            for (float d = 0; d < distance; d += 4f)
            {
                Vector2 pos = start + direction * d;
                float progress = d / distance;
                
                // Rainbow outer glow - cycling through spectrum
                float hue = (progress + Main.GameUpdateCount * 0.025f) % 1f;
                Color rainbowColor = Main.hslToRgb(hue, 0.9f, 0.65f);
                float outerScale = (beamWidth * 1.5f) / texture.Width;
                Main.EntitySpriteDraw(texture, pos, null, rainbowColor * 0.6f, rotation, texture.Size() / 2f, new Vector2(outerScale, 0.5f), SpriteEffects.None, 0);
                
                // Second rainbow layer (offset hue)
                Color rainbow2 = Main.hslToRgb((hue + 0.5f) % 1f, 0.85f, 0.6f);
                float midScale = beamWidth / texture.Width;
                Main.EntitySpriteDraw(texture, pos, null, rainbow2 * 0.4f, rotation, texture.Size() / 2f, new Vector2(midScale, 0.35f), SpriteEffects.None, 0);
                
                // White/black core
                Color coreColor = isBlack ? Color.Black * 0.9f : Color.White * 0.95f;
                float innerScale = (beamWidth * 0.5f) / texture.Width;
                Main.EntitySpriteDraw(texture, pos, null, coreColor, rotation, texture.Size() / 2f, new Vector2(innerScale, 0.2f), SpriteEffects.None, 0);
            }
            
            // Add rainbow flare bursts at intervals along beam
            for (float d = 0; d < distance; d += 30f)
            {
                Vector2 flarePos = start + direction * d;
                float hue = (d / distance + Main.GameUpdateCount * 0.02f) % 1f;
                Color flareColor = Main.hslToRgb(hue, 1f, 0.75f);
                float flareScale = 0.4f + (float)Math.Sin(beamTimer * 0.2f + d * 0.1f) * 0.15f;
                
                // Draw flare glow at this point
                Main.EntitySpriteDraw(texture, flarePos, null, flareColor * 0.8f, rotation + MathHelper.PiOver2, texture.Size() / 2f, flareScale * 2f, SpriteEffects.None, 0);
            }

            return false;
        }
    }
}
