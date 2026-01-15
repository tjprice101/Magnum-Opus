using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
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
    /// Iridescent Wingspan - Magic weapon that fires 3 black and white flares in a 60 degree cone.
    /// On hit, creates rings and flares of pearlescent rainbow explosions.
    /// Rainbow (Swan) rarity, no crafting recipe.
    /// </summary>
    public class IridescentWingspan : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
            Item.staff[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 420; // Higher than Eroica weapons
            Item.DamageType = DamageClass.Magic;
            Item.width = 50;
            Item.height = 50;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(gold: 55);
            Item.rare = ModContent.RarityType<SwanRarity>();
            Item.UseSound = SoundID.Item30 with { Pitch = 0.4f, Volume = 0.7f }; // Fractal crystal sound
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<IridescentWingspanProjectile>();
            Item.shootSpeed = 16f;
            Item.mana = 16;
            Item.noMelee = true;
            Item.scale = 0.9f; // 90% size
        }

        public override void HoldItem(Player player)
        {
            // === UNIQUE: ETHEREAL WING SILHOUETTES ===
            // The "Wingspan" manifests as ghostly wings behind the player
            
            float time = Main.GameUpdateCount * 0.03f;
            float wingPulse = (float)Math.Sin(time * 2f) * 0.15f + 0.85f;
            
            // === IRIDESCENT WING FEATHERS ===
            // Two wings made of particles spread behind player
            for (int wing = 0; wing < 2; wing++)
            {
                float wingDirection = wing == 0 ? -1f : 1f; // Left and right wings
                
                // Each wing has 7 "feathers" spreading outward
                for (int feather = 0; feather < 7; feather++)
                {
                    // Feathers spread in arc behind player
                    float featherAngle = MathHelper.PiOver2 * wingDirection; // Pointing sideways
                    featherAngle += (feather - 3) * 0.12f * wingDirection; // Spread
                    featherAngle += MathHelper.Pi * 0.15f; // Slight backward tilt
                    
                    // Feather length increases toward wing tip
                    float featherLength = 20f + feather * 8f;
                    featherLength *= wingPulse; // Breathing effect
                    
                    Vector2 featherTip = player.Center + featherAngle.ToRotationVector2() * featherLength;
                    featherTip.Y -= 5f; // Slightly above center
                    
                    // Rainbow color cycling per feather
                    float hue = (feather / 7f + time * 0.5f) % 1f;
                    Color featherColor = Main.hslToRgb(hue, 0.6f, 0.75f);
                    Color baseColor = wing == 0 ? UnifiedVFX.SwanLake.White : UnifiedVFX.SwanLake.Black;
                    featherColor = Color.Lerp(baseColor, featherColor, 0.4f);
                    
                    if (Main.rand.NextBool(4))
                    {
                        CustomParticles.GenericFlare(featherTip, featherColor, 0.2f + feather * 0.03f, 8);
                    }
                }
            }
            
            // === FALLING FEATHER DRIFT ===
            if (Main.rand.NextBool(12))
            {
                float side = Main.rand.NextBool() ? -40f : 40f;
                Vector2 featherPos = player.Center + new Vector2(side, -20f);
                Color featherColor = Main.rand.NextBool() ? UnifiedVFX.SwanLake.White : UnifiedVFX.SwanLake.Black;
                CustomParticles.SwanFeatherDrift(featherPos, featherColor, 0.32f);
            }
            
            // === MUSIC NOTES - the swan's song ===
            if (Main.rand.NextBool(20))
            {
                ThemedParticles.SwanLakeMusicNotes(player.Center, 1, 25f);
            }
            
            // Pulsing rainbow light
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.12f + 0.85f;
            Vector3 lightColor = UnifiedVFX.SwanLake.GetRainbow(Main.GameUpdateCount * 0.012f).ToVector3();
            Lighting.AddLight(player.Center, lightColor * pulse * 0.6f);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Fire 3 projectiles in a 60 degree cone - like wing tips spreading
            Vector2 towardsMouse = velocity.SafeNormalize(Vector2.UnitX);
            float spreadAngle = MathHelper.ToRadians(60f);
            
            for (int i = 0; i < 3; i++)
            {
                float angleOffset = spreadAngle * ((float)i / 2f - 0.5f);
                Vector2 projectileVelocity = towardsMouse.RotatedBy(angleOffset) * Item.shootSpeed;
                
                Projectile.NewProjectile(source, player.Center, projectileVelocity, type, damage, knockback, player.whoAmI, i);
            }
            
            // === UNIQUE: WING UNFURL CAST EFFECT ===
            // When casting, ghostly wings dramatically spread outward
            Vector2 castPos = player.Center;
            
            // Two wings bursting outward on cast
            for (int wing = 0; wing < 2; wing++)
            {
                float wingDirection = wing == 0 ? -1f : 1f;
                
                // Wing unfurl - feathers burst outward from center
                for (int feather = 0; feather < 12; feather++)
                {
                    float unfurlAngle = MathHelper.PiOver2 * wingDirection;
                    unfurlAngle += (feather - 6) * 0.08f * wingDirection;
                    unfurlAngle += MathHelper.Pi * 0.1f; // Slightly back
                    
                    float featherSpeed = 3f + feather * 0.5f;
                    Vector2 featherVel = unfurlAngle.ToRotationVector2() * featherSpeed;
                    
                    // Rainbow gradient per feather
                    float hue = feather / 12f;
                    Color featherColor = Main.hslToRgb(hue, 0.7f, 0.8f);
                    Color baseColor = wing == 0 ? UnifiedVFX.SwanLake.White : UnifiedVFX.SwanLake.Black;
                    featherColor = Color.Lerp(baseColor, featherColor, 0.5f);
                    
                    CustomParticles.GenericFlare(castPos, featherColor, 0.35f - feather * 0.02f, 18);
                    
                    // Feather drift particles
                    if (feather % 3 == 0)
                    {
                        CustomParticles.SwanFeatherDrift(castPos + featherVel * 5f, baseColor, 0.35f);
                    }
                }
            }
            
            // Central prismatic burst
            CustomParticles.PrismaticSparkleRainbow(castPos, 8);
            
            // Sword arc double helix - black and white intertwined slashes
            Vector2 arcPos = player.Center + towardsMouse * 35f;
            CustomParticles.SwordArcDoubleHelix(arcPos, towardsMouse * 4f, Color.White, Color.Black, 0.4f);
            
            // Gradient halo rings - Black → White with rainbow shimmer (80% size)
            for (int ring = 0; ring < 4; ring++)
            {
                float progress = (float)ring / 4f;
                Color baseColor = Color.Lerp(new Color(20, 20, 30), Color.White, progress);
                // Add rainbow edge
                float hue = (ring * 0.25f + Main.GameUpdateCount * 0.01f) % 1f;
                Color ringColor = Color.Lerp(baseColor, Main.hslToRgb(hue, 0.6f, 0.8f), 0.3f);
                CustomParticles.HaloRing(castPos, ringColor, 0.4f + ring * 0.12f, 14 + ring * 4);
            }
            
            // Swan feather spiral on cast!
            CustomParticles.SwanFeatherSpiral(castPos, Color.White, 6);
            
            // Music notes - reduced
            ThemedParticles.SwanLakeMusicNotes(castPos, 5, 28f);
            
            // Black/white spark accent with GRADIENT
            for (int i = 0; i < 12; i++)
            {
                float progress = (float)i / 12f;
                // GRADIENT: Black → White with rainbow shimmer overlay
                Color baseColor = Color.Lerp(new Color(20, 20, 30), Color.White, progress);
                float hue = (progress + Main.GameUpdateCount * 0.015f) % 1f;
                Color col = Color.Lerp(baseColor, Main.hslToRgb(hue, 0.5f, 0.85f), 0.25f);
                int dustType = progress < 0.5f ? DustID.Shadowflame : DustID.WhiteTorch;
                Vector2 vel = towardsMouse.RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.NextFloat(4f, 10f);
                Dust d = Dust.NewDustPerfect(castPos, dustType, vel, progress < 0.5f ? 100 : 0, col, 1.5f);
                d.noGravity = true;
            }
            
            // Elegant casting light
            Lighting.AddLight(castPos, 1.4f, 1.4f, 1.7f);
            
            return false;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.15f + 1f;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // White outer glow
            spriteBatch.Draw(texture, position, null, Color.White * 0.4f, rotation, origin, scale * 0.9f * pulse * 1.3f, SpriteEffects.None, 0f);
            // Black inner shadow
            spriteBatch.Draw(texture, position, null, Color.Black * 0.3f, rotation, origin, scale * 0.9f * pulse * 1.15f, SpriteEffects.None, 0f);
            // Pearlescent shimmer
            Color pearlColor = Color.Lerp(new Color(255, 240, 245), new Color(240, 245, 255), (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.5f + 0.5f);
            spriteBatch.Draw(texture, position, null, pearlColor * 0.25f, rotation, origin, scale * 0.9f * pulse * 1.1f, SpriteEffects.None, 0f);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Draw main sprite at 90% scale
            spriteBatch.Draw(texture, position, null, lightColor, rotation, origin, scale * 0.9f, SpriteEffects.None, 0f);
            
            Lighting.AddLight(Item.Center, 0.5f, 0.5f, 0.6f);
            
            return false;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            // Draw at 90% scale in inventory
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            spriteBatch.Draw(texture, position, frame, drawColor, 0f, origin, scale * 0.9f, SpriteEffects.None, 0f);
            return false;
        }
    }

    public class IridescentWingspanProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/SwanLake/ResonantWeapons/IridescentWingspan";

        private bool isBlack => Projectile.ai[0] % 2 == 0;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            // === HOMING/TRACKING BEHAVIOR ===
            // Find closest enemy and home toward them
            float homingRange = 400f;
            float homingStrength = 0.08f;
            float maxSpeed = 18f;
            
            NPC closestTarget = null;
            float closestDist = homingRange;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy(Projectile))
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestTarget = npc;
                    }
                }
            }
            
            if (closestTarget != null)
            {
                // Home toward target
                Vector2 targetDirection = (closestTarget.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetDirection * maxSpeed, homingStrength);
                
                // Cap speed
                if (Projectile.velocity.Length() > maxSpeed)
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * maxSpeed;
            }
            
            // Rotation
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // === MASSIVE Black and white trailing particles! ===
            Color trailColor = isBlack ? new Color(20, 20, 30) : Color.White;
            Color oppositeColor = isBlack ? Color.White : new Color(20, 20, 30);
            
            // HEAVY main trail - constant flow!
            for (int i = 0; i < 2; i++)
            {
                int dustType = isBlack ? DustID.Shadowflame : DustID.WhiteTorch;
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f), dustType,
                    -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(2f, 2f),
                    isBlack ? 100 : 0, trailColor, 1.8f);
                d.noGravity = true;
                d.fadeIn = 1.4f;
            }
            
            // Contrasting sparkles!
            if (Main.rand.NextBool(2))
            {
                int oppDustType = isBlack ? DustID.WhiteTorch : DustID.Shadowflame;
                Dust opp = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), oppDustType,
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f),
                    isBlack ? 0 : 100, oppositeColor, 1.4f);
                opp.noGravity = true;
            }
            
            // Frequent flare effects!
            if (Main.rand.NextBool(2))
            {
                CustomParticles.GenericFlare(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), trailColor, 0.5f, 18);
            }
            
            // Rainbow shimmer trail!
            if (Main.rand.NextBool(3))
            {
                float hue = Main.rand.NextFloat();
                Color rainbow = Main.hslToRgb(hue, 1f, 0.7f);
                Dust r = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowTorch,
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1f, 1f), 0, rainbow, 1.5f);
                r.noGravity = true;
            }
            
            // Pearlescent shimmer trail
            if (Main.rand.NextBool(4))
            {
                Color pearl = Main.rand.NextBool() ? new Color(255, 240, 250) : new Color(240, 250, 255);
                CustomParticles.GenericFlare(Projectile.Center, pearl, 0.35f, 15);
            }
            
            // Rainbow flares
            if (Main.rand.NextBool(5))
            {
                float hue = Main.rand.NextFloat();
                CustomParticles.GenericFlare(Projectile.Center, Main.hslToRgb(hue, 1f, 0.7f), 0.4f, 16);
            }
            
            // Ambient fractal gem sparkle - the signature Swan Lake effect
            if (Main.rand.NextBool(8))
            {
                ThemedParticles.SwanLakeFractalTrail(Projectile.Center, 0.4f);
            }
            
            // Swan feather trail
            if (Main.rand.NextBool(4))
            {
                CustomParticles.SwanFeatherTrail(Projectile.Center, Projectile.velocity, 0.25f);
            }

            // BRIGHT pulsing light!
            float lightIntensity = isBlack ? 0.5f : 1.0f;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.2f + 1f;
            Lighting.AddLight(Projectile.Center, lightIntensity * pulse, lightIntensity * pulse, (lightIntensity + 0.15f) * pulse);
        }

        public override void OnKill(int timeLeft)
        {
            // Create pearlescent rainbow explosion
            CreateRainbowExplosion();
            
            // Fractal gem burst on death
            ThemedParticles.SwanLakeFractalGemBurst(Projectile.Center, isBlack ? Color.Black : Color.White, 0.6f, 6, false);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Flame of the Swan debuff
            target.AddBuff(ModContent.BuffType<FlameOfTheSwan>(), 300); // 5 seconds

            // Create rings and flares on hit
            CreateRainbowExplosion();
            
            // === EXTRA CUSTOM FLARES ON HIT! ===
            // Rainbow flare burst at impact!
            for (int i = 0; i < 8; i++)
            {
                float hue = i / 8f;
                Color flareColor = Main.hslToRgb(hue, 1f, 0.8f);
                CustomParticles.GenericFlare(target.Center + Main.rand.NextVector2Circular(18f, 18f), flareColor, 0.55f, 20);
            }
            
            // Swan Lake flare
            CustomParticles.SwanLakeFlare(target.Center, 0.5f);
            
            // Feather burst on hit
            CustomParticles.SwanFeatherBurst(target.Center, 5, 0.3f);
            
            // Extra sparkles
            ThemedParticles.SwanLakeSparkles(target.Center, 15, 30f);
        }

        private void CreateRainbowExplosion()
        {
            // === PEARLESCENT RAINBOW EXPLOSION (80% size)! ===
            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.75f, Pitch = 0.4f }, Projectile.Center); // Glass shatter
            SoundEngine.PlaySound(SoundID.Item107 with { Volume = 0.6f, Pitch = 0.5f }, Projectile.Center); // Crystal shatter
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Volume = 0.35f, Pitch = 0.6f }, Projectile.Center);
            
            // Monochrome impact that explodes into rainbow (80% scale)!
            ThemedParticles.SwanLakeImpact(Projectile.Center, 1.15f);
            ThemedParticles.SwanLakeRainbowExplosion(Projectile.Center, 1.3f);
            ThemedParticles.SwanLakeMusicalImpact(Projectile.Center, 1.0f, true);
            
            // Music notes burst (80% size)!
            ThemedParticles.SwanLakeMusicNotes(Projectile.Center, 10, 44f);
            ThemedParticles.SwanLakeAccidentals(Projectile.Center, 5, 36f);
            ThemedParticles.SwanLakeFeathers(Projectile.Center, 8, 40f);
            
            // Swan feather explosion (80% size)!
            CustomParticles.SwanFeatherExplosion(Projectile.Center, 8, 0.32f);
            
            // Stacked halo rings (80% of reduced size)!
            for (int ring = 0; ring < 3; ring++)
            {
                float hue = (Main.GameUpdateCount * 0.02f + ring * 0.25f) % 1f;
                Color ringColor = Main.hslToRgb(hue, 1f, 0.8f);
                CustomParticles.HaloRing(Projectile.Center, ringColor, 0.32f + ring * 0.1f, 14 + ring * 3);
            }
            CustomParticles.HaloRing(Projectile.Center, Color.White, 0.52f, 20);
            CustomParticles.HaloRing(Projectile.Center, Color.Black, 0.4f, 18);
            
            // Rainbow sparkle flares (80% size)!
            ThemedParticles.SwanLakeSparkles(Projectile.Center, 24, 36f);
            
            // Rainbow flare burst (80% size)!
            for (int i = 0; i < 11; i++)
            {
                float hue = i / 11f;
                Color flareColor = Main.hslToRgb(hue, 1f, 0.8f);
                CustomParticles.GenericFlare(Projectile.Center + Main.rand.NextVector2Circular(16f, 16f), flareColor, 0.68f, 24);
            }
            
            // Black and white explosion particles (80% count/size)!
            for (int i = 0; i < 19; i++)
            {
                float angle = MathHelper.TwoPi * i / 19f;
                Color bwColor = i % 2 == 0 ? Color.White : new Color(20, 20, 30);
                int dustType = i % 2 == 0 ? DustID.WhiteTorch : DustID.Shadowflame;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(5.6f, 11.2f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, dustType, vel, i % 2 == 0 ? 0 : 100, bwColor, 1.84f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }
            
            // Rainbow spark explosion (80% count/size)!
            for (int i = 0; i < 26; i++)
            {
                float angle = MathHelper.TwoPi * i / 26f;
                float hue = i / 26f;
                Color sparkColor = Main.hslToRgb(hue, 1f, 0.7f);
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(6.4f, 12.8f);
                Dust spark = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowTorch, vel, 0, sparkColor, 1.76f);
                spark.noGravity = true;
                spark.fadeIn = 1.2f;
            }
            
            // Pearlescent shimmer burst (80% size)!
            ThemedParticles.SwanLakeSparkles(Projectile.Center, 20, 48f);
            
            // Light burst (80% intensity)!
            Lighting.AddLight(Projectile.Center, 1.6f, 1.6f, 2f);
        }
        
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // === EXPLOSIVE GROUND/WALL HIT EFFECT! ===
            CreateRainbowExplosion();
            
            // Extra custom flares on tile hit!
            for (int i = 0; i < 10; i++)
            {
                float hue = i / 10f;
                Color flareColor = Main.hslToRgb(hue, 1f, 0.8f);
                CustomParticles.GenericFlare(Projectile.Center + Main.rand.NextVector2Circular(18f, 18f), flareColor, 0.55f, 20);
            }
            
            // Swan Lake flare
            CustomParticles.SwanLakeFlare(Projectile.Center, 0.5f);
            
            // Sparkles burst
            ThemedParticles.SwanLakeSparkles(Projectile.Center, 20, 40f);
            
            // Music notes on impact!
            ThemedParticles.SwanLakeMusicNotes(Projectile.Center, 8, 40f);
            ThemedParticles.SwanLakeAccidentals(Projectile.Center, 4, 32f);
            
            // Sound effect
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f, Pitch = 0.1f }, Projectile.Center);
            
            return true; // Destroy projectile
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Custom draw with glow
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            Color glowColor = isBlack ? Color.Black * 0.8f : Color.White * 0.8f;
            float scale = 0.4f; // Small projectile
            
            // Glow layers
            Main.EntitySpriteDraw(texture, drawPos, null, glowColor * 0.5f, Projectile.rotation, origin, scale * 1.4f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, glowColor * 0.3f, Projectile.rotation, origin, scale * 1.2f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, Color.White, Projectile.rotation, origin, scale, SpriteEffects.None, 0);

            return false;
        }
    }
}
