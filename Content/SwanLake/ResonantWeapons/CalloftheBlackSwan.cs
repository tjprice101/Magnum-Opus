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
using MagnumOpus.Common.BaseClasses;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons
{
    /// <summary>
    /// Call of the Black Swan - Greatsword that sends 3 black/white flares on swing.
    /// Landing all 3 flares empowers the sword, making the next swing fire 8 smaller projectiles at double damage.
    /// Rainbow (Swan) rarity, no crafting recipe.
    /// Hold right-click to charge a devastating prismatic swan storm attack!
    /// </summary>
    public class CalloftheBlackSwan : MeleeSwingItemBase
    {
        // Track empowerment state per player
        private static Dictionary<int, int> flareHitCounts = new Dictionary<int, int>();
        private static Dictionary<int, bool> empoweredState = new Dictionary<int, bool>();
        private static Dictionary<int, int> empowermentTimer = new Dictionary<int, int>();

        public static void RegisterFlareHit(int playerIndex)
        {
            if (!flareHitCounts.ContainsKey(playerIndex))
                flareHitCounts[playerIndex] = 0;
            
            flareHitCounts[playerIndex]++;
            
            if (flareHitCounts[playerIndex] >= 3)
            {
                empoweredState[playerIndex] = true;
                empowermentTimer[playerIndex] = 300; // 5 seconds to use empowered swing
                flareHitCounts[playerIndex] = 0;
                
                // Visual feedback for empowerment
                Player player = Main.player[playerIndex];
                CustomParticles.SwanLakeImpactBurst(player.Center, 15);
                CustomParticles.HaloRing(player.Center, Color.Black, 1f, 40);
                CustomParticles.HaloRing(player.Center, Color.White, 0.8f, 30);
                SoundEngine.PlaySound(SoundID.Item119 with { Volume = 0.8f }, player.Center);
            }
        }

        public static bool IsEmpowered(int playerIndex)
        {
            return empoweredState.ContainsKey(playerIndex) && empoweredState[playerIndex];
        }

        public static void ConsumeEmpowerment(int playerIndex)
        {
            if (empoweredState.ContainsKey(playerIndex))
            {
                empoweredState[playerIndex] = false;
                empowermentTimer[playerIndex] = 0;
            }
        }

        public static void ResetFlareCount(int playerIndex)
        {
            flareHitCounts[playerIndex] = 0;
        }

        protected override int SwingProjectileType => ModContent.ProjectileType<CalloftheBlackSwanSwing>();
        protected override int ComboStepCount => 3;
        protected override Color GetLoreColor() => new Color(220, 225, 235);

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        protected override void SetWeaponDefaults()
        {
            Item.damage = 400;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.knockBack = 7f;
            Item.value = Item.sellPrice(gold: 60);
            Item.rare = ModContent.RarityType<SwanRarity>();
            Item.UseSound = SoundID.Item29 with { Pitch = -0.1f, Volume = 0.85f };
        }

        public override void HoldItem(Player player)
        {
            base.HoldItem(player);

            // Update empowerment timer
            if (empowermentTimer.ContainsKey(player.whoAmI) && empowermentTimer[player.whoAmI] > 0)
            {
                empowermentTimer[player.whoAmI]--;
                if (empowermentTimer[player.whoAmI] <= 0)
                {
                    empoweredState[player.whoAmI] = false;
                }
            }

            // EXPLOSIVE visual effects when empowered!
            if (IsEmpowered(player.whoAmI))
            {
                // Intense pulsing black/white aura with rainbow shimmer!
                float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.2f);
                
                // Magic sparkle field rising - elegant empowerment aura
                if (Main.rand.NextBool(4))
                {
                    CustomParticles.MagicSparkleFieldRising(player.Center, Color.White * 0.7f, 3);
                }
                
                // Prismatic rainbow sparkles - floating iridescent dust
                if (Main.rand.NextBool(5))
                {
                    CustomParticles.PrismaticSparkleRainbow(player.Center, 5);
                }
                
                // Swan feather aura - elegant floating feathers
                if (Main.rand.NextBool(6))
                {
                    CustomParticles.SwanFeatherAura(player.Center, 35f, 2);
                }
                
                // Black/white particle flow - reduced for elegance
                for (int i = 0; i < 2; i++)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(25f, 25f);
                    Color col = Main.rand.NextBool() ? Color.White : Color.Black;
                    int dustType = Main.rand.NextBool() ? DustID.WhiteTorch : DustID.Shadowflame;
                    Dust d = Dust.NewDustPerfect(player.Center + offset, dustType,
                        new Vector2(0, -1.8f) + Main.rand.NextVector2Circular(1f, 1f), col == Color.White ? 0 : 100, col, 1.5f);
                    d.noGravity = true;
                    d.fadeIn = 1.2f;
                }
                
                // BRIGHT pulsing rainbow light!
                float hueLight = (Main.GameUpdateCount * 0.02f) % 1f;
                Vector3 rainbowLight = Main.hslToRgb(hueLight, 0.8f, 0.6f).ToVector3();
                Lighting.AddLight(player.Center, (0.7f + pulse * 0.25f) * rainbowLight);
            }
            else
            {
                // === SUBTLE AMBIENT EFFECTS - Reduced for elegance ===
                // Only occasional fractal flares
                if (Main.rand.NextBool(25))
                {
                    float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                    float radius = Main.rand.NextFloat(30f, 60f);
                    Vector2 flarePos = player.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                    Color baseColor = Main.rand.NextBool() ? UnifiedVFX.SwanLake.Black : UnifiedVFX.SwanLake.White;
                    Color rainbow = UnifiedVFX.SwanLake.GetRainbow(Main.rand.NextFloat());
                    Color fractalColor = Color.Lerp(baseColor, rainbow, 0.35f);
                    CustomParticles.GenericFlare(flarePos, fractalColor, 0.28f, 18);
                }
                
                // Rare floating feathers - much less frequent
                if (Main.rand.NextBool(40))
                {
                    Color featherColor = Main.rand.NextBool() ? UnifiedVFX.SwanLake.White : UnifiedVFX.SwanLake.Black;
                    CustomParticles.SwanFeatherDrift(player.Center + Main.rand.NextVector2Circular(22f, 22f), featherColor, 0.25f);
                }
                
                // Subtle light
                float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.1f + 0.9f;
                Vector3 lightColor = UnifiedVFX.SwanLake.GetRainbow(Main.GameUpdateCount * 0.01f).ToVector3();
                Lighting.AddLight(player.Center, lightColor * pulse * 0.25f);
            }
        }

        protected override void AddWeaponTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Swings send black and white flares that track enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Landing 3 flares empowers the next swing with devastating force"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Hold right-click to charge a prismatic swan storm"));
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.15f + 1f;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Black ominous glow
            spriteBatch.Draw(texture, position, null, Color.Black * 0.5f, rotation, origin, scale * 0.9f * pulse * 1.4f, SpriteEffects.None, 0f);
            // White inner glow
            spriteBatch.Draw(texture, position, null, Color.White * 0.3f, rotation, origin, scale * 0.9f * pulse * 1.2f, SpriteEffects.None, 0f);
            
            // Rainbow shimmer
            float hue = (float)Main.GameUpdateCount * 0.004f % 1f;
            Color rainbow = Main.hslToRgb(hue, 0.7f, 0.6f);
            spriteBatch.Draw(texture, position, null, rainbow * 0.2f, rotation, origin, scale * 0.9f * pulse * 1.1f, SpriteEffects.None, 0f);
            
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

    public class BlackSwanFlare : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SwanFeather2";

        private bool isBlack => Projectile.ai[0] == 0;
        private bool isEmpowered => Projectile.ai[1] == 1;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // === HOMING/TRACKING BEHAVIOR ===
            float homingRange = 300f;
            float homingStrength = 0.08f;
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
                // Use varied target point so projectiles don't all converge on the same spot
                Vector2 targetPoint = TargetingUtilities.GetVariedTargetPoint(closestNPC, Projectile.whoAmI);
                Vector2 targetDir = (targetPoint - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetDir * Projectile.velocity.Length(), homingStrength);
            }

            // === MASSIVE BLACK AND WHITE BLAZING TRAIL! ===
            Color trailColor = isBlack ? new Color(20, 20, 30) : new Color(255, 255, 255);
            
            // HEAVY main trail - constant flow of particles!
            for (int i = 0; i < 3; i++)
            {
                int dustType = isBlack ? DustID.Shadowflame : DustID.WhiteTorch;
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), dustType, 
                    -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(2f, 2f),
                    isBlack ? 100 : 0, trailColor, isEmpowered ? 1.4f : 2.0f);
                d.noGravity = true;
                d.fadeIn = 1.4f;
            }

            // CONSTANT swirling contrasting spiral!
            float angle = Main.GameUpdateCount * 0.4f;
            for (int i = 0; i < 2; i++)
            {
                float spiralAngle = angle + i * MathHelper.Pi;
                Vector2 spiralOffset = new Vector2((float)Math.Cos(spiralAngle), (float)Math.Sin(spiralAngle)) * 6f;
                
                int spiralDustType = isBlack ? DustID.WhiteTorch : DustID.Shadowflame;
                Color spiralColor = isBlack ? Color.White : Color.Black;
                Dust spiral = Dust.NewDustPerfect(Projectile.Center + spiralOffset, spiralDustType, 
                    -Projectile.velocity * 0.1f, isBlack ? 0 : 100, spiralColor, isEmpowered ? 1.0f : 1.5f);
                spiral.noGravity = true;
            }
            
            // Rainbow shimmer particles along trail!
            if (Main.rand.NextBool(2))
            {
                float hue = Main.rand.NextFloat();
                Color rainbow = Main.hslToRgb(hue, 1f, 0.7f);
                Dust r = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowTorch,
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f), 0, rainbow, 1.5f);
                r.noGravity = true;
            }
            
            // Frequent flare particles!
            if (Main.rand.NextBool(3))
            {
                Color flareCol = isBlack ? Color.Black : Color.White;
                CustomParticles.GenericFlare(Projectile.Center, flareCol, 0.4f, 15);
            }
            
            // Rainbow flares occasionally
            if (Main.rand.NextBool(5))
            {
                float hue = Main.rand.NextFloat();
                CustomParticles.GenericFlare(Projectile.Center, Main.hslToRgb(hue, 1f, 0.7f), 0.35f, 15);
            }
            
            // Ambient fractal gem sparkle
            if (Main.rand.NextBool(8))
            {
                ThemedParticles.SwanLakeFractalTrail(Projectile.Center, 0.4f);
            }
            
            // ☁EMUSICAL NOTATION - Swan Lake graceful melody
            if (Main.rand.NextBool(6))
            {
                float hue = (Main.GameUpdateCount * 0.01f + Main.rand.NextFloat()) % 1f;
                Color noteColor = Main.hslToRgb(hue, 0.8f, 0.9f);
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f);
                float shimmer = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.1f;
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.8f * shimmer, 35);
            }

            // BRIGHT pulsing light!
            float intensity = isBlack ? 0.5f : 0.9f;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.2f + 1f;
            Lighting.AddLight(Projectile.Center, intensity * pulse, intensity * pulse, (intensity + 0.1f) * pulse);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply debuff
            target.AddBuff(ModContent.BuffType<FlameOfTheSwan>(), 240); // 4 seconds

            // Register hit for empowerment (only non-empowered flares count)
            if (!isEmpowered)
            {
                CalloftheBlackSwan.RegisterFlareHit(Projectile.owner);
            }

            // Explosion effect
            CreateFlareExplosion(target.Center);
        }

        private void CreateFlareExplosion(Vector2 position)
        {
            float scale = isEmpowered ? 0.8f : 1.3f;
            
            // === EXPLOSIVE IMPACT! ===
            
            // Monochrome impact that transitions to MASSIVE rainbow!
            ThemedParticles.SwanLakeImpact(position, scale * 1.5f);
            ThemedParticles.SwanLakeRainbowExplosion(position, scale * 1.2f);
            
            // HUGE rainbow sparkles burst!
            ThemedParticles.SwanLakeSparkles(position, (int)(20 * scale), 50f * scale);
            
            // Multiple music notes on impact!
            ThemedParticles.SwanLakeMusicNotes(position, (int)(8 * scale), 35f * scale);
            ThemedParticles.SwanLakeAccidentals(position, (int)(4 * scale), 25f * scale);
            
            // ☁EMUSICAL IMPACT - Swan's graceful chord
            ThemedParticles.MusicNoteBurst(position, Color.White, (int)(5 * scale), 3.5f * scale);
            
            // Stacked halo rings (reduced size)!
            for (int ring = 0; ring < 3; ring++)
            {
                float hue = (Main.GameUpdateCount * 0.02f + ring * 0.33f) % 1f;
                Color ringColor = Main.hslToRgb(hue, 1f, 0.75f);
                CustomParticles.HaloRing(position, ringColor, 0.25f * scale + ring * 0.075f, (int)(10 * scale) + ring * 3);
            }
            CustomParticles.HaloRing(position, Color.White, 0.35f * scale, (int)(12 * scale));
            CustomParticles.HaloRing(position, Color.Black, 0.25f * scale, (int)(10 * scale));
            
            // Rainbow sparkle flares!
            ThemedParticles.SwanLakeSparkles(position, (int)(12 * scale), 30f * scale);
            
            // Rainbow flare burst!
            for (int i = 0; i < (int)(10 * scale); i++)
            {
                float hue = i / (10f * scale);
                Color flareColor = Main.hslToRgb(hue, 1f, 0.75f);
                CustomParticles.GenericFlare(position + Main.rand.NextVector2Circular(15f, 15f), flareColor, 0.65f * scale, 22);
            }
            
            // Radial spark explosion!
            for (int i = 0; i < (int)(24 * scale); i++)
            {
                float angle = MathHelper.TwoPi * i / (24f * scale);
                float hue = i / (24f * scale);
                Color sparkColor = Main.hslToRgb(hue, 1f, 0.7f);
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(5f, 10f) * scale;
                Dust spark = Dust.NewDustPerfect(position, DustID.RainbowTorch, vel, 0, sparkColor, 1.8f * scale);
                spark.noGravity = true;
                spark.fadeIn = 1.4f;
            }
            
            // Black/white contrast burst!
            for (int i = 0; i < (int)(12 * scale); i++)
            {
                Color col = i % 2 == 0 ? Color.White : Color.Black;
                int dustType = i % 2 == 0 ? DustID.WhiteTorch : DustID.Shadowflame;
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f) * scale;
                Dust d = Dust.NewDustPerfect(position, dustType, vel, i % 2 == 0 ? 0 : 100, col, 1.8f * scale);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }
            
            // BRIGHT light burst!
            float lightIntensity = isEmpowered ? 1.2f : 1.8f;
            Lighting.AddLight(position, lightIntensity, lightIntensity, lightIntensity + 0.3f);

            SoundEngine.PlaySound(SoundID.Item93 with { Volume = 0.7f * scale, Pitch = isEmpowered ? 0.6f : 0.1f }, position);
        }

        public override void OnKill(int timeLeft)
        {
            // EXPLOSIVE death burst!
            for (int i = 0; i < 12; i++)
            {
                Color col = i % 2 == 0 ? Color.White : Color.Black;
                int dustType = i % 2 == 0 ? DustID.WhiteTorch : DustID.Shadowflame;
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, dustType, vel, i % 2 == 0 ? 0 : 100, col, isEmpowered ? 1.2f : 1.8f);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }
            
            // Rainbow death sparks!
            for (int i = 0; i < 6; i++)
            {
                float hue = i / 6f;
                Color rainbow = Main.hslToRgb(hue, 1f, 0.7f);
                Dust r = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowTorch,
                    Main.rand.NextVector2Circular(4f, 4f), 0, rainbow, 1.4f);
                r.noGravity = true;
            }
            
            // Small halo
            CustomParticles.HaloRing(Projectile.Center, isBlack ? Color.Black : Color.White, 0.35f, 12);
            
            // ☁EMUSICAL FINALE - Feathered symphony
            float finaleHue = (Main.GameUpdateCount * 0.02f) % 1f;
            Color finaleColor = Main.hslToRgb(finaleHue, 0.9f, 0.85f);
            ThemedParticles.MusicNoteBurst(Projectile.Center, finaleColor, 5, 3.5f);
            
            // Fractal gem burst on death!
            ThemedParticles.SwanLakeFractalGemBurst(Projectile.Center, isBlack ? Color.Black : Color.White, 0.7f, 5, false);
            
            // Music notes on death!
            ThemedParticles.SwanLakeMusicNotes(Projectile.Center, 4, 25f);
            
            Lighting.AddLight(Projectile.Center, 0.8f, 0.8f, 1f);
        }
        
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // === EXPLOSIVE GROUND/WALL HIT EFFECT! ===
            CreateFlareExplosion(Projectile.Center);
            
            // Extra custom flares on tile hit!
            for (int i = 0; i < 8; i++)
            {
                float hue = i / 8f;
                Color flareColor = Main.hslToRgb(hue, 1f, 0.8f);
                CustomParticles.GenericFlare(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f), flareColor, 0.5f, 18);
            }
            
            // Swan Lake flare
            CustomParticles.SwanLakeFlare(Projectile.Center, 0.45f);
            
            // Sparkles burst
            ThemedParticles.SwanLakeSparkles(Projectile.Center, 15, 30f);
            
            // Music notes on impact!
            ThemedParticles.SwanLakeMusicNotes(Projectile.Center, 6, 35f);
            ThemedParticles.SwanLakeAccidentals(Projectile.Center, 3, 25f);
            
            // Sound effect
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f, Pitch = 0.2f }, Projectile.Center);
            
            return true; // Destroy projectile
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            float scale = isEmpowered ? 0.6f : 0.975f; // 25% smaller
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.2f + 1f;
            
            Color mainColor = isBlack ? new Color(15, 15, 20) : new Color(255, 255, 255);
            Color glowColor = isBlack ? new Color(30, 30, 40) * 0.9f : Color.White * 0.95f;
            Color oppositeGlow = isBlack ? Color.White * 0.5f : new Color(20, 20, 30) * 0.5f;
            
            // Rainbow outer aura - cycling!
            float hue = (Main.GameUpdateCount * 0.025f) % 1f;
            Color rainbow = Main.hslToRgb(hue, 1f, 0.7f);
            Main.EntitySpriteDraw(texture, drawPos, null, rainbow * 0.5f, Projectile.rotation, origin, scale * pulse * 2.25f, SpriteEffects.None, 0);
            
            // Second rainbow layer (offset hue)
            Color rainbow2 = Main.hslToRgb((hue + 0.5f) % 1f, 0.9f, 0.65f);
            Main.EntitySpriteDraw(texture, drawPos, null, rainbow2 * 0.4f, Projectile.rotation, origin, scale * pulse * 1.875f, SpriteEffects.None, 0);
            
            // Main glow layers
            Main.EntitySpriteDraw(texture, drawPos, null, glowColor * 0.6f, Projectile.rotation, origin, scale * pulse * 1.65f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, oppositeGlow, Projectile.rotation, origin, scale * pulse * 1.35f, SpriteEffects.None, 0);
            
            // Core glow
            Main.EntitySpriteDraw(texture, drawPos, null, glowColor * 0.8f, Projectile.rotation, origin, scale * pulse * 1.05f, SpriteEffects.None, 0);
            
            // Main sprite
            Main.EntitySpriteDraw(texture, drawPos, null, mainColor, Projectile.rotation, origin, scale * pulse * 0.75f, SpriteEffects.None, 0);

            return false;
        }
    }
}
