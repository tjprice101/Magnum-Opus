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
    /// Chromatic Swan Song - Magic pistol that fires tracking black and white shots.
    /// Hitting the same target 3 times causes a massive black/white explosion with rainbow particles.
    /// Rainbow (Swan) rarity, no crafting recipe.
    /// </summary>
    public class ChromaticSwanSong : ModItem
    {
        // Track hits per enemy for the 3-hit explosion mechanic
        public static Dictionary<int, int> EnemyHitCounts = new Dictionary<int, int>();

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 290; // Higher than Eroica, balanced for tracking
            Item.DamageType = DamageClass.Magic;
            Item.width = 40;
            Item.height = 25;
            Item.useTime = 12;
            Item.useAnimation = 12;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(gold: 55);
            Item.rare = ModContent.RarityType<SwanRarity>();
            Item.UseSound = SoundID.Item29 with { Pitch = 0.5f, Volume = 0.7f }; // Fractal crystal sound
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<ChromaticSwanSongProjectile>();
            Item.shootSpeed = 14f;
            Item.mana = 8;
            Item.noMelee = true;
            Item.scale = 0.9f; // 90% size
        }

        public override void HoldItem(Player player)
        {
            // === UnifiedVFX SWAN LAKE AMBIENT AURA ===
            UnifiedVFX.SwanLake.Aura(player.Center, 35f, 0.3f);
            
            // === AMBIENT FRACTAL FLARES - signature Swan Lake look ===
            if (Main.rand.NextBool(7))
            {
                // Spawn subtle fractal flares around player with dual-polarity
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float radius = Main.rand.NextFloat(30f, 60f);
                Vector2 flarePos = player.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                // Alternating black/white with rainbow shimmer
                Color baseColor = Main.rand.NextBool() ? UnifiedVFX.SwanLake.White : UnifiedVFX.SwanLake.Black;
                Color rainbowAccent = UnifiedVFX.SwanLake.GetRainbow(Main.rand.NextFloat());
                Color fractalColor = Color.Lerp(baseColor, rainbowAccent, 0.3f);
                CustomParticles.GenericFlare(flarePos, fractalColor, 0.32f, 20);
            }
            
            // Rainbow shimmer particles
            if (Main.rand.NextBool(5))
            {
                Color rainbow = UnifiedVFX.SwanLake.GetRainbow(Main.rand.NextFloat());
                CustomParticles.GenericFlare(player.Center + Main.rand.NextVector2Circular(22f, 22f), rainbow, 0.42f, 18);
            }
            
            // Floating feathers - enhanced
            if (Main.rand.NextBool(8))
            {
                Color featherColor = Main.rand.NextBool() ? UnifiedVFX.SwanLake.White : UnifiedVFX.SwanLake.Black;
                CustomParticles.SwanFeatherDrift(player.Center + Main.rand.NextVector2Circular(28f, 28f), featherColor, 0.28f);
            }
            
            // Pulsing rainbow light
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.15f + 0.85f;
            Vector3 lightColor = UnifiedVFX.SwanLake.GetRainbow(Main.GameUpdateCount * 0.01f).ToVector3();
            Lighting.AddLight(player.Center, lightColor * pulse * 0.6f);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Alternate between black and white shots
            int projectileType = Main.rand.NextBool() ? 0 : 1;
            
            // === UnifiedVFX SWAN LAKE MUZZLE IMPACT ===
            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.Zero) * 30f;
            Vector2 direction = velocity.SafeNormalize(Vector2.Zero);
            UnifiedVFX.SwanLake.Impact(muzzlePos, 0.8f);
            
            // HEAVY spark burst!
            ThemedParticles.SwanLakeSparks(muzzlePos, direction, 12, 9f);
            ThemedParticles.SwanLakeSparkles(muzzlePos, 10, 22f);
            
            // Dual-polarity fractal burst - Black/White with rainbow shimmer
            for (int i = 0; i < 10; i++)
            {
                float progress = (float)i / 10f;
                // Alternate black/white base
                Color baseColor = Color.Lerp(UnifiedVFX.SwanLake.Black, UnifiedVFX.SwanLake.White, progress);
                // Rainbow shimmer overlay
                Color rainbow = UnifiedVFX.SwanLake.GetRainbow(progress);
                Color flareColor = Color.Lerp(baseColor, rainbow, 0.4f);
                Vector2 offset = new Vector2((float)Math.Cos(MathHelper.TwoPi * progress), (float)Math.Sin(MathHelper.TwoPi * progress)) * 18f;
                CustomParticles.GenericFlare(muzzlePos + offset, flareColor, 0.55f, 20);
            }
            
            // Gradient halo rings - Black → White with rainbow edge
            for (int ring = 0; ring < 4; ring++)
            {
                float progress = (float)ring / 4f;
                Color ringColor = Color.Lerp(UnifiedVFX.SwanLake.Black, UnifiedVFX.SwanLake.White, progress);
                Color rainbow = UnifiedVFX.SwanLake.GetRainbow(progress);
                ringColor = Color.Lerp(ringColor, rainbow, 0.3f);
                CustomParticles.HaloRing(muzzlePos, ringColor, 0.32f + ring * 0.12f, 13 + ring * 3);
            }
            
            // Feather burst on shot
            ThemedParticles.SwanFeatherBurst(muzzlePos, 6, 30f);
            
            // Bright muzzle light
            Lighting.AddLight(muzzlePos, 1.3f, 1.3f, 1.5f);
            
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, projectileType);
            
            return false;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.07f) * 0.12f + 1f;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Chromatic shimmer (cycling colors)
            float hue = (float)Main.GameUpdateCount * 0.01f % 1f;
            Color chromatic = Main.hslToRgb(hue, 0.3f, 0.8f);
            spriteBatch.Draw(texture, position, null, chromatic * 0.3f, rotation, origin, scale * 0.9f * pulse * 1.2f, SpriteEffects.None, 0f);
            
            // Black/white alternating
            Color bw = ((int)(Main.GameUpdateCount * 0.15f) % 2 == 0) ? Color.White * 0.3f : Color.Black * 0.2f;
            spriteBatch.Draw(texture, position, null, bw, rotation, origin, scale * 0.9f * pulse * 1.1f, SpriteEffects.None, 0f);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
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
    }

    public class ChromaticSwanSongProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/SwanLake/ResonantWeapons/ChromaticSwanSong";

        private bool isBlack => Projectile.ai[0] == 0;
        private int targetNPC = -1;

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Tracking AI - find nearest enemy
            if (targetNPC < 0 || !Main.npc[targetNPC].active || !Main.npc[targetNPC].CanBeChasedBy())
            {
                targetNPC = -1;
                float closestDist = 500f; // Extended detection range!
                
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && npc.CanBeChasedBy())
                    {
                        float dist = Vector2.Distance(Projectile.Center, npc.Center);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            targetNPC = i;
                        }
                    }
                }
            }

            // Home in on target - slightly faster
            if (targetNPC >= 0)
            {
                NPC target = Main.npc[targetNPC];
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                float turnSpeed = 0.1f;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity.SafeNormalize(Vector2.Zero), toTarget, turnSpeed) * Projectile.velocity.Length();
            }

            // === MASSIVE BLACK AND WHITE BLAZING TRAIL! ===
            Color trailColor = isBlack ? new Color(20, 20, 30) : Color.White;
            Color oppositeColor = isBlack ? Color.White : new Color(20, 20, 30);
            
            // HEAVY main trail - constant flow!
            for (int i = 0; i < 2; i++)
            {
                int dustType = isBlack ? DustID.Shadowflame : DustID.WhiteTorch;
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), dustType,
                    -Projectile.velocity * 0.25f + Main.rand.NextVector2Circular(2f, 2f), isBlack ? 100 : 0, trailColor, 1.8f);
                d.noGravity = true;
                d.fadeIn = 1.4f;
            }
            
            // Contrasting sparkles - frequent!
            if (Main.rand.NextBool(2))
            {
                int oppDustType = isBlack ? DustID.WhiteTorch : DustID.Shadowflame;
                Dust opp = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f), oppDustType,
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f),
                    isBlack ? 0 : 100, oppositeColor, 1.4f);
                opp.noGravity = true;
            }
            
            // Rainbow shimmer trail!
            if (Main.rand.NextBool(2))
            {
                float hue = Main.rand.NextFloat();
                Color rainbow = Main.hslToRgb(hue, 1f, 0.7f);
                Dust r = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowTorch,
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1f, 1f), 0, rainbow, 1.5f);
                r.noGravity = true;
            }
            
            // Frequent contrasting flare
            if (Main.rand.NextBool(3))
            {
                CustomParticles.GenericFlare(Projectile.Center, oppositeColor, 0.35f, 15);
            }
            
            // Rainbow flares occasionally
            if (Main.rand.NextBool(5))
            {
                float hue = Main.rand.NextFloat();
                CustomParticles.GenericFlare(Projectile.Center, Main.hslToRgb(hue, 1f, 0.7f), 0.3f, 14);
            }
            
            // Ambient fractal gem sparkle - the signature Swan Lake effect
            if (Main.rand.NextBool(10))
            {
                ThemedParticles.SwanLakeFractalTrail(Projectile.Center, 0.35f);
            }
            
            // Swan feather trail
            if (Main.rand.NextBool(5))
            {
                CustomParticles.SwanFeatherTrail(Projectile.Center, Projectile.velocity, 0.2f);
            }

            // BRIGHT pulsing light!
            float intensity = isBlack ? 0.45f : 0.85f;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.2f + 1f;
            Lighting.AddLight(Projectile.Center, intensity * pulse, intensity * pulse, (intensity + 0.1f) * pulse);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Flame of the Swan debuff
            target.AddBuff(ModContent.BuffType<FlameOfTheSwan>(), 240); // 4 seconds
            
            // === SMALL EXPLOSION ON EVERY HIT with lighting! ===
            CreateSmallHitExplosion(target.Center);

            // Track hits for this enemy
            int npcIndex = target.whoAmI;
            if (!ChromaticSwanSong.EnemyHitCounts.ContainsKey(npcIndex))
            {
                ChromaticSwanSong.EnemyHitCounts[npcIndex] = 0;
            }
            
            ChromaticSwanSong.EnemyHitCounts[npcIndex]++;

            // Check if we've hit 3 times
            if (ChromaticSwanSong.EnemyHitCounts[npcIndex] >= 3)
            {
                // Reset counter
                ChromaticSwanSong.EnemyHitCounts[npcIndex] = 0;
                
                // Fractal gem burst on the big explosion!
                ThemedParticles.SwanLakeFractalGemBurst(target.Center, Color.White, 1.2f, 8, true);
                
                // Create massive triple explosion
                for (int explosion = 0; explosion < 3; explosion++)
                {
                    Vector2 explosionOffset = Main.rand.NextVector2Circular(30f, 30f);
                    CreateMassiveExplosion(target.Center + explosionOffset);
                }
                
                // Extra damage effect
                target.SimpleStrikeNPC(damageDone / 2, 0, false, 0f, null, false, 0, false);
            }
            else
            {
                // Small hit effect
                CustomParticles.GenericFlare(target.Center, isBlack ? Color.Black : Color.White, 0.4f, 15);
            }
        }
        
        private void CreateSmallHitExplosion(Vector2 position)
        {
            // === SMALL HIT EXPLOSION with rainbow sparkles and lighting! ===
            
            // Rainbow sparkle burst!
            ThemedParticles.SwanLakeSparkles(position, 12, 25f);
            
            // Small rainbow flares
            for (int i = 0; i < 6; i++)
            {
                float hue = i / 6f;
                Color flareColor = Main.hslToRgb(hue, 1f, 0.75f);
                CustomParticles.GenericFlare(position + Main.rand.NextVector2Circular(10f, 10f), flareColor, 0.35f, 15);
            }
            
            // Small halo ring
            float ringHue = (Main.GameUpdateCount * 0.03f) % 1f;
            CustomParticles.HaloRing(position, Main.hslToRgb(ringHue, 1f, 0.7f), 0.25f, 12);
            
            // B/W dust burst
            for (int i = 0; i < 6; i++)
            {
                Color col = i % 2 == 0 ? Color.White : new Color(20, 20, 30);
                int dustType = i % 2 == 0 ? DustID.WhiteTorch : DustID.Shadowflame;
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Dust d = Dust.NewDustPerfect(position, dustType, vel, i % 2 == 0 ? 0 : 100, col, 1.2f);
                d.noGravity = true;
            }
            
            // Rainbow dust
            for (int i = 0; i < 4; i++)
            {
                float hue = i / 4f;
                Color rainbow = Main.hslToRgb(hue, 1f, 0.7f);
                Dust r = Dust.NewDustPerfect(position, DustID.RainbowTorch, Main.rand.NextVector2Circular(2.5f, 2.5f), 0, rainbow, 1.1f);
                r.noGravity = true;
            }
            
            // Music notes on hit!
            ThemedParticles.SwanLakeMusicNotes(position, 4, 20f);
            
            // Small feather burst
            CustomParticles.SwanFeatherBurst(position, 3, 0.25f);
            
            // Lighting effect!
            float lightHue = (Main.GameUpdateCount * 0.025f) % 1f;
            Vector3 lightColor = Main.hslToRgb(lightHue, 0.8f, 0.7f).ToVector3();
            Lighting.AddLight(position, lightColor * 0.8f);
        }

        private void CreateMassiveExplosion(Vector2 position)
        {
            // === DEVASTATING TRIPLE EXPLOSION (reduced halo sizes)! ===
            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 1.0f, Pitch = 0.1f }, position); // Glass shatter
            SoundEngine.PlaySound(SoundID.Item107 with { Volume = 0.85f, Pitch = 0.2f }, position); // Crystal shatter
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Volume = 0.6f, Pitch = 0.3f }, position);
            
            // Monochrome impact that explodes into rainbow!
            ThemedParticles.SwanLakeRainbowExplosion(position, 1.62f);
            ThemedParticles.SwanLakeMusicalImpact(position, 1.26f, true);
            ThemedParticles.SwanLakeImpact(position, 1.08f);
            
            // Stacked halo rings (50% reduced)!
            for (int ring = 0; ring < 5; ring++)
            {
                float hue = (Main.GameUpdateCount * 0.02f + ring * 0.2f) % 1f;
                Color ringColor = Main.hslToRgb(hue, 1f, 0.8f);
                CustomParticles.HaloRing(position, ringColor, 0.6f + ring * 0.175f, 25 + ring * 6);
            }
            CustomParticles.HaloRing(position, Color.White, 1.0f, 35);
            CustomParticles.HaloRing(position, Color.Black, 0.85f, 32);
            
            // Rainbow sparkle flares!
            ThemedParticles.SwanLakeSparkles(position, 45, 60f);
            
            // Music notes burst!
            ThemedParticles.SwanLakeMusicNotes(position, 14, 55f);
            ThemedParticles.SwanLakeAccidentals(position, 8, 45f);
            ThemedParticles.SwanLakeFeathers(position, 10, 50f);
            
            // DEVASTATING feather explosion!
            CustomParticles.SwanFeatherExplosion(position, 12, 0.45f);
            
            // Rainbow flare explosion!
            for (int i = 0; i < 18; i++)
            {
                float hue = i / 18f;
                Color flareColor = Main.hslToRgb(hue, 1f, 0.85f);
                CustomParticles.GenericFlare(position + Main.rand.NextVector2Circular(25f, 25f), flareColor, 0.8f, 30);
            }
            
            // Black/white explosion particles - HUGE!
            for (int i = 0; i < 32; i++)
            {
                float angle = MathHelper.TwoPi * i / 32f;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(10f, 20f);
                int dustType = i % 2 == 0 ? DustID.WhiteTorch : DustID.Shadowflame;
                Color col = i % 2 == 0 ? Color.White : new Color(20, 20, 30);
                Dust d = Dust.NewDustPerfect(position, dustType, vel, i % 2 == 0 ? 0 : 100, col, 2.8f);
                d.noGravity = true;
                d.fadeIn = 1.6f;
            }
            
            // MASSIVE rainbow spark spiral!
            for (int i = 0; i < 48; i++)
            {
                float angle = MathHelper.TwoPi * i / 48f;
                float hue = i / 48f;
                Color sparkColor = Main.hslToRgb(hue, 1f, 0.75f);
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(12f, 22f);
                Dust spark = Dust.NewDustPerfect(position, DustID.RainbowTorch, vel, 0, sparkColor, 2.6f);
                spark.noGravity = true;
                spark.fadeIn = 1.7f;
            }
            
            // Pearlescent shimmer explosion!
            ThemedParticles.SwanLakeSparkles(position, 40, 80f);
            
            // MASSIVE light explosion!
            Lighting.AddLight(position, 3f, 3f, 3.5f);
        }

        public override void OnKill(int timeLeft)
        {
            // Small death effect
            for (int i = 0; i < 6; i++)
            {
                Color col = i % 2 == 0 ? Color.White : Color.Black;
                Dust d = Dust.NewDustPerfect(Projectile.Center, i % 2 == 0 ? DustID.WhiteTorch : DustID.Smoke,
                    Main.rand.NextVector2Circular(3f, 3f), i % 2 == 0 ? 0 : 150, col, 1f);
                d.noGravity = true;
            }
            
            // Rainbow sparks on death
            for (int i = 0; i < 4; i++)
            {
                float hue = i / 4f;
                Color rainbow = Main.hslToRgb(hue, 1f, 0.7f);
                Dust r = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowTorch, Main.rand.NextVector2Circular(2.5f, 2.5f), 0, rainbow, 1.1f);
                r.noGravity = true;
            }
        }
        
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // === EXPLOSIVE GROUND/WALL HIT EFFECT! ===
            CreateSmallHitExplosion(Projectile.Center);
            
            // Extra custom flares on tile hit!
            for (int i = 0; i < 8; i++)
            {
                float hue = i / 8f;
                Color flareColor = Main.hslToRgb(hue, 1f, 0.8f);
                CustomParticles.GenericFlare(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f), flareColor, 0.45f, 16);
            }
            
            // Swan Lake flare
            CustomParticles.SwanLakeFlare(Projectile.Center, 0.4f);
            
            // Sparkles burst
            ThemedParticles.SwanLakeSparkles(Projectile.Center, 15, 30f);
            
            // Music notes on impact!
            ThemedParticles.SwanLakeMusicNotes(Projectile.Center, 6, 30f);
            ThemedParticles.SwanLakeAccidentals(Projectile.Center, 3, 22f);
            
            // Sound effect
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.4f, Pitch = 0.2f }, Projectile.Center);
            
            return true; // Destroy projectile
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            Color mainColor = isBlack ? new Color(30, 30, 35) : new Color(250, 250, 255);
            Color glowColor = isBlack ? Color.Black * 0.7f : Color.White * 0.8f;
            
            // Blazing glow effect
            Main.EntitySpriteDraw(texture, drawPos, null, glowColor * 0.6f, Projectile.rotation, origin, 0.5f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, glowColor * 0.4f, Projectile.rotation, origin, 0.4f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, mainColor, Projectile.rotation, origin, 0.3f, SpriteEffects.None, 0);

            return false;
        }
    }
}
