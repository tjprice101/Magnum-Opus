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
    /// Call of the Pearlescent Lake - Assault rifle that fires black and white flaming rockets.
    /// Creates rainbow pearlescent explosions on contact and ignites with Flame of the Swan.
    /// Rainbow (Swan) rarity, no crafting recipe.
    /// </summary>
    public class CallofthePearlescentLake : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 380; // Higher than Eroica weapons
            Item.DamageType = DamageClass.Ranged;
            Item.width = 60;
            Item.height = 30;
            Item.useTime = 8; // Fast assault rifle
            Item.useAnimation = 8;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 55);
            Item.rare = ModContent.RarityType<SwanRarity>();
            Item.UseSound = SoundID.Item30 with { Pitch = 0.2f, Volume = 0.75f }; // Fractal crystal sound
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<PearlescentRocket>();
            Item.shootSpeed = 18f;
            Item.useAmmo = AmmoID.Bullet; // Uses bullets but fires rockets
            Item.noMelee = true;
            Item.scale = 0.9f; // 90% size
        }

        public override void HoldItem(Player player)
        {
            // === UnifiedVFX SWAN LAKE AMBIENT AURA ===
            UnifiedVFX.SwanLake.Aura(player.Center, 35f, 0.3f);
            
            // === AMBIENT FRACTAL FLARES - dual-polarity with rainbow shimmer ===
            if (Main.rand.NextBool(7))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float radius = Main.rand.NextFloat(30f, 60f);
                Vector2 flarePos = player.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                // Gradient: Black ↁEWhite with rainbow overlay
                Color baseColor = Main.rand.NextBool() ? UnifiedVFX.SwanLake.Black : UnifiedVFX.SwanLake.White;
                Color rainbow = UnifiedVFX.SwanLake.GetRainbow(Main.rand.NextFloat());
                Color fractalColor = Color.Lerp(baseColor, rainbow, 0.35f);
                CustomParticles.GenericFlare(flarePos, fractalColor, 0.32f, 20);
                ThemedParticles.SwanLakeFractalTrail(flarePos, 0.25f);
            }
            
            // Rainbow shimmer particles at muzzle
            if (Main.rand.NextBool(5))
            {
                Color rainbow = UnifiedVFX.SwanLake.GetRainbow(Main.rand.NextFloat());
                Vector2 muzzleOffset = new Vector2(30f * player.direction, 0) + Main.rand.NextVector2Circular(15f, 15f);
                CustomParticles.GenericFlare(player.Center + muzzleOffset, rainbow, 0.38f, 16);
            }
            
            // Floating feathers - dual-polarity
            if (Main.rand.NextBool(10))
            {
                Color featherColor = Main.rand.NextBool() ? UnifiedVFX.SwanLake.White : UnifiedVFX.SwanLake.Black;
                CustomParticles.SwanFeatherDrift(player.Center + new Vector2(35f * player.direction, 0), featherColor, 0.28f);
            }
            
            // Pulsing rainbow light
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.15f + 0.85f;
            Vector3 lightColor = UnifiedVFX.SwanLake.GetRainbow(Main.GameUpdateCount * 0.012f).ToVector3();
            Lighting.AddLight(player.Center, lightColor * pulse * 0.7f);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            // Override ammo type to always fire our rocket
            type = ModContent.ProjectileType<PearlescentRocket>();
            
            // Add slight spread for assault rifle feel
            velocity = velocity.RotatedByRandom(MathHelper.ToRadians(5f));
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // === UnifiedVFX SWAN LAKE MUZZLE IMPACT ===
            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.Zero) * 50f;
            Vector2 direction = velocity.SafeNormalize(Vector2.Zero);
            UnifiedVFX.SwanLake.Impact(muzzlePos, 1.0f);
            
            // HUGE rainbow spark burst in firing direction!
            ThemedParticles.SwanLakeSparks(muzzlePos, direction, 14, 11f);
            ThemedParticles.SwanLakeSparkles(muzzlePos, 10, 28f);
            
            // Dual-polarity fractal flare burst with gradient
            for (int i = 0; i < 10; i++)
            {
                float progress = (float)i / 10f;
                Color baseColor = Color.Lerp(UnifiedVFX.SwanLake.Black, UnifiedVFX.SwanLake.White, progress);
                Color rainbow = UnifiedVFX.SwanLake.GetRainbow(progress);
                Color flareColor = Color.Lerp(baseColor, rainbow, 0.4f);
                float angle = MathHelper.TwoPi * progress;
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 15f;
                CustomParticles.GenericFlare(muzzlePos + offset, flareColor, 0.6f, 20);
            }
            
            // Gradient halo rings - Black ↁEWhite with rainbow edge
            for (int ring = 0; ring < 4; ring++)
            {
                float progress = (float)ring / 4f;
                Color baseColor = Color.Lerp(UnifiedVFX.SwanLake.Black, UnifiedVFX.SwanLake.White, progress);
                Color rainbow = UnifiedVFX.SwanLake.GetRainbow(progress);
                Color ringColor = Color.Lerp(baseColor, rainbow, 0.35f);
                CustomParticles.HaloRing(muzzlePos, ringColor, 0.45f + ring * 0.1f, 16 + ring * 3);
            }
            
            // Swan feather burst on shot!
            ThemedParticles.SwanFeatherBurst(muzzlePos, 6, 30f);
            
            // HEAVY muzzle spark dust!
            for (int i = 0; i < 16; i++)
            {
                Color col = i % 2 == 0 ? Color.White : Color.Black;
                int dustType = i % 2 == 0 ? DustID.WhiteTorch : DustID.Shadowflame;
                Vector2 vel = direction.RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.NextFloat(5f, 12f);
                Dust d = Dust.NewDustPerfect(muzzlePos, dustType, vel, i % 2 == 0 ? 0 : 100, col, 1.8f);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }
            
            // Rainbow dust burst!
            for (int i = 0; i < 10; i++)
            {
                float hue = i / 10f;
                Color rainbow = Main.hslToRgb(hue, 1f, 0.7f);
                Vector2 vel = direction.RotatedBy(Main.rand.NextFloat(-0.4f, 0.4f)) * Main.rand.NextFloat(4f, 9f);
                Dust r = Dust.NewDustPerfect(muzzlePos, DustID.RainbowTorch, vel, 0, rainbow, 1.6f);
                r.noGravity = true;
            }
            
            // Bright muzzle light!
            Lighting.AddLight(muzzlePos, 1.5f, 1.5f, 1.8f);
            
            // Fire the rocket
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            
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
            
            // Alternating black/white glow
            Color glowCol = ((int)(Main.GameUpdateCount * 0.1f) % 2 == 0) ? Color.White * 0.4f : Color.Black * 0.3f;
            spriteBatch.Draw(texture, position, null, glowCol, rotation, origin, scale * 0.9f * pulse * 1.25f, SpriteEffects.None, 0f);
            
            // Pearlescent shimmer
            Color pearl = Color.Lerp(new Color(255, 200, 220), new Color(200, 220, 255), (float)Math.Sin(Main.GameUpdateCount * 0.07f) * 0.5f + 0.5f);
            spriteBatch.Draw(texture, position, null, pearl * 0.2f, rotation, origin, scale * 0.9f * pulse * 1.15f, SpriteEffects.None, 0f);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Draw main sprite at 90% scale
            spriteBatch.Draw(texture, position, null, lightColor, rotation, origin, scale * 0.9f, SpriteEffects.None, 0f);
            
            Lighting.AddLight(Item.Center, 0.4f, 0.4f, 0.5f);
            
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
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Rapidly fires black and white flaming rockets"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Creates pearlescent rainbow explosions on impact"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Where feathers fall, the lake remembers'") 
            { 
                OverrideColor = new Color(220, 225, 235) 
            });
        }
    }

    public class PearlescentRocket : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/GlowingHalo1";

        private bool isBlackRocket => Projectile.localAI[0] == 0;

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.extraUpdates = 1;
        }

        public override void OnSpawn(IEntitySource source)
        {
            // Randomize black or white rocket
            Projectile.localAI[0] = Main.rand.NextBool() ? 0 : 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // === MASSIVE BLACK AND WHITE BLAZING ROCKET TRAIL! ===
            Color trailColor = isBlackRocket ? new Color(20, 20, 30) : Color.White;
            Color oppositeColor = isBlackRocket ? Color.White : new Color(20, 20, 30);
            
            // HEAVY main flame trail - constant flow!
            for (int i = 0; i < 3; i++)
            {
                int dustType = isBlackRocket ? DustID.Shadowflame : DustID.WhiteTorch;
                Dust flame = Dust.NewDustPerfect(Projectile.Center - Projectile.velocity * 0.3f + Main.rand.NextVector2Circular(5f, 5f), 
                    dustType, 
                    -Projectile.velocity * 0.4f + Main.rand.NextVector2Circular(3f, 3f),
                    isBlackRocket ? 100 : 0, trailColor, 2.2f);
                flame.noGravity = true;
                flame.fadeIn = 1.5f;
            }
            
            // Contrasting sparkles - constant!
            for (int i = 0; i < 2; i++)
            {
                if (Main.rand.NextBool(2))
                {
                    int oppDustType = isBlackRocket ? DustID.WhiteTorch : DustID.Shadowflame;
                    Dust opp = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), oppDustType,
                        -Projectile.velocity * 0.25f + Main.rand.NextVector2Circular(2f, 2f),
                        isBlackRocket ? 0 : 100, oppositeColor, 1.6f);
                    opp.noGravity = true;
                }
            }
            
            // Rainbow shimmer trail - HEAVY!
            for (int i = 0; i < 2; i++)
            {
                float hue = Main.rand.NextFloat();
                Color rainbow = Main.hslToRgb(hue, 1f, 0.75f);
                Dust r = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), DustID.RainbowTorch,
                    -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1.5f, 1.5f), 0, rainbow, 1.8f);
                r.noGravity = true;
                r.fadeIn = 1.4f;
            }
            
            // Frequent flare effects!
            if (Main.rand.NextBool(2))
            {
                CustomParticles.GenericFlare(Projectile.Center, oppositeColor, 0.45f, 18);
            }
            
            // Pearlescent shimmer particles
            if (Main.rand.NextBool(3))
            {
                Color pearl = Main.rand.NextBool() ? new Color(255, 230, 240) : new Color(230, 240, 255);
                Dust p = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f), DustID.WhiteTorch, 
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1f, 1f), 30, pearl, 1.4f);
                p.noGravity = true;
            }
            
            // Rainbow flares along trail
            if (Main.rand.NextBool(4))
            {
                float hue = Main.rand.NextFloat();
                CustomParticles.GenericFlare(Projectile.Center, Main.hslToRgb(hue, 1f, 0.7f), 0.5f, 18);
            }
            
            // Mini halo rings along path occasionally
            if (Main.rand.NextBool(8))
            {
                CustomParticles.HaloRing(Projectile.Center, isBlackRocket ? Color.Black : Color.White, 0.25f, 10);
            }
            
            // Swan feather trail on rocket
            if (Main.rand.NextBool(5))
            {
                CustomParticles.SwanFeatherTrail(Projectile.Center, Projectile.velocity, 0.2f);
            }
            
            // Ambient fractal gem sparkle
            if (Main.rand.NextBool(6))
            {
                ThemedParticles.SwanLakeFractalTrail(Projectile.Center, 0.45f);
            }
            
            // ☁EMUSICAL NOTATION - Swan Lake graceful melody
            if (Main.rand.NextBool(5))
            {
                float hue = (Main.GameUpdateCount * 0.01f + Main.rand.NextFloat()) % 1f;
                Color noteColor = Main.hslToRgb(hue, 0.8f, 0.9f);
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f);
                float shimmer = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.1f;
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.8f * shimmer, 35);
            }

            // BRIGHT pulsing light!
            float intensity = isBlackRocket ? 0.7f : 1.1f;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.2f + 1f;
            float hueLight = (Main.GameUpdateCount * 0.02f) % 1f;
            Vector3 rainbowLight = Main.hslToRgb(hueLight, 0.6f, 0.6f).ToVector3();
            Lighting.AddLight(Projectile.Center, (rainbowLight + new Vector3(intensity)) * pulse * 0.5f);
        }

        public override void OnKill(int timeLeft)
        {
            CreatePearlescentExplosion();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Flame of the Swan debuff
            target.AddBuff(ModContent.BuffType<FlameOfTheSwan>(), 360); // 6 seconds
            
            // === SEEKING CRYSTALS - 25% chance on hit ===
            if (Main.rand.NextBool(4))
            {
                SeekingCrystalHelper.SpawnSwanLakeCrystals(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Projectile.velocity,
                    (int)(damageDone * 0.18f),
                    Projectile.knockBack,
                    Projectile.owner,
                    3
                );
            }
            
            // === EXPLOSIVE HIT EFFECT! ===
            CreateHitExplosion(target.Center);
        }
        
        private void CreateHitExplosion(Vector2 position)
        {
            // Rainbow impact burst!
            ThemedParticles.SwanLakeImpact(position, 0.81f);
            ThemedParticles.SwanLakeRainbowExplosion(position, 0.72f);
            
            // Rainbow sparkle flares!
            ThemedParticles.SwanLakeSparkles(position, 18, 35f);
            
            // Custom rainbow flares on hit!
            for (int i = 0; i < 8; i++)
            {
                float hue = i / 8f;
                Color flareColor = Main.hslToRgb(hue, 1f, 0.8f);
                CustomParticles.GenericFlare(position + Main.rand.NextVector2Circular(12f, 12f), flareColor, 0.5f, 18);
            }
            
            // B/W dust burst!
            for (int i = 0; i < 10; i++)
            {
                Color col = i % 2 == 0 ? Color.White : new Color(20, 20, 30);
                int dustType = i % 2 == 0 ? DustID.WhiteTorch : DustID.Shadowflame;
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Dust d = Dust.NewDustPerfect(position, dustType, vel, i % 2 == 0 ? 0 : 100, col, 1.4f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }
            
            // Small halo ring
            float haloHue = (Main.GameUpdateCount * 0.03f) % 1f;
            CustomParticles.HaloRing(position, Main.hslToRgb(haloHue, 1f, 0.7f), 0.3f, 12);
            
            // Swan feather burst on hit!
            CustomParticles.SwanFeatherBurst(position, 5, 0.3f);
            
            // ☁EMUSICAL IMPACT - Swan's graceful chord
            ThemedParticles.MusicNoteBurst(position, Color.White, 5, 3.5f);
            
            // Fractal gem burst on hit!
            ThemedParticles.SwanLakeFractalGemBurst(position, isBlackRocket ? Color.Black : Color.White, 0.7f, 6, false);
            
            // Lighting!
            float lightHue = (Main.GameUpdateCount * 0.025f) % 1f;
            Vector3 lightColor = Main.hslToRgb(lightHue, 0.8f, 0.7f).ToVector3();
            Lighting.AddLight(position, lightColor * 0.9f);
        }

        private void CreatePearlescentExplosion()
        {
            // === RAINBOW PEARLESCENT EXPLOSION (70% size - reduced 30%)! ===
            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.63f, Pitch = 0.3f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item107 with { Volume = 0.53f, Pitch = 0.4f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Volume = 0.28f, Pitch = 0.5f }, Projectile.Center);
            
            // Monochrome to rainbow explosion (70% size)
            ThemedParticles.SwanLakeRainbowExplosion(Projectile.Center, 0.79f);
            ThemedParticles.SwanLakeMusicalImpact(Projectile.Center, 0.63f, true);
            ThemedParticles.SwanLakeImpact(Projectile.Center, 0.63f);
            
            // Stacked halo rings (70% size)!
            for (int ring = 0; ring < 3; ring++)
            {
                float hue = (Main.GameUpdateCount * 0.02f + ring * 0.25f) % 1f;
                Color ringColor = Main.hslToRgb(hue, 1f, 0.8f);
                CustomParticles.HaloRing(Projectile.Center, ringColor, 0.35f + ring * 0.11f, 14 + ring * 4);
            }
            CustomParticles.HaloRing(Projectile.Center, Color.White, 0.63f, 21);
            CustomParticles.HaloRing(Projectile.Center, Color.Black, 0.53f, 20);
            
            // Music notes burst (70% size)
            ThemedParticles.SwanLakeMusicNotes(Projectile.Center, 6, 25f);
            ThemedParticles.SwanLakeAccidentals(Projectile.Center, 3, 20f);
            ThemedParticles.SwanLakeFeathers(Projectile.Center, 4, 21f);
            
            // ☁EMUSICAL FINALE - Feathered symphony
            float finaleHue = (Main.GameUpdateCount * 0.02f) % 1f;
            Color finaleColor = Main.hslToRgb(finaleHue, 0.9f, 0.85f);
            ThemedParticles.MusicNoteBurst(Projectile.Center, finaleColor, 6, 4f);
            
            // Swan feather explosion on rocket death (70% size)!
            CustomParticles.SwanFeatherExplosion(Projectile.Center, 7, 0.28f);
            
            // Sparkle flares (70% size)!
            ThemedParticles.SwanLakeSparkles(Projectile.Center, 35, 39f);
            
            // Rainbow flare explosion (70% count/size)!
            for (int i = 0; i < 11; i++)
            {
                float hue = i / 11f;
                Color flareColor = Main.hslToRgb(hue, 1f, 0.8f);
                CustomParticles.GenericFlare(Projectile.Center + Main.rand.NextVector2Circular(14f, 14f), flareColor, 0.46f, 18);
            }
            
            // Black flame burst (70% count/size)
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(3.5f, 7f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Shadowflame, vel, 100, new Color(20, 20, 30), 1.26f);
                d.noGravity = true;
                d.fadeIn = 0.91f;
            }
            
            // White flame burst (70% count/size)
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f + MathHelper.Pi / 8f;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(2.8f, 6.3f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch, vel, 0, Color.White, 1.12f);
                d.noGravity = true;
                d.fadeIn = 0.84f;
            }
            
            // Rainbow spark explosion (70% count/size)!
            for (int i = 0; i < 14; i++)
            {
                float angle = MathHelper.TwoPi * i / 14f;
                float hue = i / 14f;
                Color sparkColor = Main.hslToRgb(hue, 1f, 0.7f);
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(4.2f, 8.4f);
                Dust spark = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowTorch, vel, 0, sparkColor, 1.12f);
                spark.noGravity = true;
                spark.fadeIn = 0.91f;
            }
            
            // Extra pearlescent shimmer burst (70% size)!
            ThemedParticles.SwanLakeSparkles(Projectile.Center, 14, 32f);
            
            // Light explosion (70% intensity)
            Lighting.AddLight(Projectile.Center, 1.05f, 1.05f, 1.26f);
        }
        
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // === EXPLOSIVE GROUND/WALL HIT EFFECT! ===
            CreatePearlescentExplosion();
            
            // Extra custom flares on tile hit!
            for (int i = 0; i < 10; i++)
            {
                float hue = i / 10f;
                Color flareColor = Main.hslToRgb(hue, 1f, 0.8f);
                CustomParticles.GenericFlare(Projectile.Center + Main.rand.NextVector2Circular(18f, 18f), flareColor, 0.6f, 22);
            }
            
            // Swan Lake flare
            CustomParticles.SwanLakeFlare(Projectile.Center, 0.5f);
            
            // Sparkles burst
            ThemedParticles.SwanLakeSparkles(Projectile.Center, 25, 40f);
            
            // Music notes on impact!
            ThemedParticles.SwanLakeMusicNotes(Projectile.Center, 8, 40f);
            ThemedParticles.SwanLakeAccidentals(Projectile.Center, 4, 30f);
            
            // Sound effect
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f, Pitch = 0.1f }, Projectile.Center);
            
            return true; // Destroy projectile
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            Color mainColor = isBlackRocket ? new Color(50, 50, 60) : new Color(240, 240, 255);
            
            // Glow effect
            Color glowColor = isBlackRocket ? Color.Black * 0.6f : Color.White * 0.7f;
            Main.EntitySpriteDraw(texture, drawPos, null, glowColor, Projectile.rotation, origin, 1.3f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, mainColor, Projectile.rotation, origin, 1f, SpriteEffects.None, 0);

            return false;
        }
    }
}
