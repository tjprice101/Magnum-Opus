using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.Eroica.Projectiles;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Eroica.ResonantWeapons
{
    /// <summary>
    /// Blossom of the Sakura - Assault rifle with explosive ammunition.
    /// Rainbow rarity, higher tier than Moonlight weapons.
    /// UNIQUE VFX: Heat buildup system - gun visually heats up with sustained fire!
    /// </summary>
    public class BlossomOfTheSakura : ModItem
    {
        // Heat buildup system for unique VFX
        private int heatLevel = 0;
        private int heatDecayCooldown = 0;
        private const int MaxHeat = 40;
        
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 75; // Balanced: ~1125 DPS (75 × 60/4)
            Item.DamageType = DamageClass.Ranged;
            Item.width = 64;
            Item.height = 28;
            Item.useTime = 4; // Very fast fire rate
            Item.useAnimation = 4;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(gold: 38);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<BlossomOfTheSakuraBulletProjectile>();
            Item.shootSpeed = 18f;
            Item.useAmmo = AmmoID.Bullet;
            Item.noMelee = true;
            Item.maxStack = 1;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-2f, 0f);
        }

        public override void HoldItem(Player player)
        {
            // === UNIQUE: HEAT BUILDUP VISUALIZATION ===
            // This assault rifle HEATS UP with sustained fire - show it!
            
            // Heat decay when not firing
            if (heatDecayCooldown > 0)
                heatDecayCooldown--;
            else if (heatLevel > 0)
                heatLevel--;
            
            float heatProgress = (float)heatLevel / MaxHeat; // 0 to 1
            Vector2 gunBarrel = player.Center + new Vector2(40f * player.direction, -2f);
            Vector2 gunBody = player.Center + new Vector2(20f * player.direction, -2f);
            
            // === HEAT SHIMMER EFFECT ===
            // Rising heat waves when gun is hot
            if (heatProgress > 0.2f && Main.rand.NextBool((int)(8 - heatProgress * 5)))
            {
                float shimmerIntensity = (heatProgress - 0.2f) / 0.8f;
                Vector2 shimmerPos = gunBody + new Vector2(Main.rand.NextFloat(-15f, 25f) * player.direction, 0f);
                Vector2 shimmerVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(1f, 2.5f));
                Color shimmerColor = Color.Lerp(UnifiedVFX.Eroica.Sakura * 0.3f, UnifiedVFX.Eroica.Gold * 0.5f, shimmerIntensity);
                var shimmer = new GenericGlowParticle(shimmerPos, shimmerVel, shimmerColor, 0.15f + shimmerIntensity * 0.1f, 20, true);
                MagnumParticleHandler.SpawnParticle(shimmer);
            }
            
            // === BARREL GLOW ===
            // Gun barrel glows from pink to gold to white-hot
            if (heatProgress > 0.1f)
            {
                Color barrelColor;
                if (heatProgress < 0.4f)
                    barrelColor = Color.Lerp(UnifiedVFX.Eroica.Sakura * 0.5f, UnifiedVFX.Eroica.Crimson, (heatProgress - 0.1f) / 0.3f);
                else if (heatProgress < 0.7f)
                    barrelColor = Color.Lerp(UnifiedVFX.Eroica.Crimson, UnifiedVFX.Eroica.Gold, (heatProgress - 0.4f) / 0.3f);
                else
                    barrelColor = Color.Lerp(UnifiedVFX.Eroica.Gold, Color.White, (heatProgress - 0.7f) / 0.3f);
                
                float glowScale = 0.2f + heatProgress * 0.4f;
                CustomParticles.GenericFlare(gunBarrel, barrelColor, glowScale, 3);
                
                // Secondary glow along barrel
                if (heatProgress > 0.5f)
                {
                    Vector2 midBarrel = Vector2.Lerp(gunBody, gunBarrel, 0.5f);
                    CustomParticles.GenericFlare(midBarrel, barrelColor * 0.7f, glowScale * 0.6f, 3);
                }
            }
            
            // === EMBER SPARKS ===
            // Hot sparks fly off when overheated
            if (heatProgress > 0.6f && Main.rand.NextBool((int)(10 - heatProgress * 6)))
            {
                Vector2 sparkPos = gunBarrel + Main.rand.NextVector2Circular(5f, 5f);
                Vector2 sparkVel = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, -1f));
                Color sparkColor = Color.Lerp(UnifiedVFX.Eroica.Gold, Color.White, Main.rand.NextFloat(0.3f));
                var spark = new GenericGlowParticle(sparkPos, sparkVel, sparkColor, 0.12f, 15, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // === SMOKE WISPS ===
            // Thin smoke when very hot
            if (heatProgress > 0.7f && Main.rand.NextBool(12))
            {
                Vector2 smokePos = gunBarrel + new Vector2(Main.rand.NextFloat(-5f, 5f), -5f);
                var smoke = new HeavySmokeParticle(smokePos, new Vector2(0, -0.8f), 
                    Color.Gray * 0.4f, Main.rand.Next(20, 35), 0.15f, 0.3f, 0.01f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // === SAKURA PETALS - burning away in the heat ===
            if (Main.rand.NextBool(8))
            {
                Vector2 petalPos = player.Center + Main.rand.NextVector2Circular(25f, 25f);
                ThemedParticles.SakuraPetals(petalPos, 1, 20f);
                
                // At high heat, petals catch fire
                if (heatProgress > 0.5f && Main.rand.NextBool(3))
                {
                    var firePetal = new GenericGlowParticle(petalPos, Vector2.Zero, UnifiedVFX.Eroica.Crimson * 0.5f, 0.15f, 12, true);
                    MagnumParticleHandler.SpawnParticle(firePetal);
                }
            }
            
            // === MUSIC NOTES - The rhythm of battle ===
            if (heatProgress > 0.3f && Main.rand.NextBool(20))
            {
                ThemedParticles.EroicaMusicNotes(gunBarrel, 1, 15f);
            }
            
            // Dynamic lighting based on heat
            float baseLightIntensity = 0.25f;
            float heatLightBonus = heatProgress * 0.5f;
            Vector3 lightVec = Color.Lerp(UnifiedVFX.Eroica.Sakura, 
                heatProgress > 0.5f ? UnifiedVFX.Eroica.Gold : UnifiedVFX.Eroica.Crimson, 
                heatProgress).ToVector3();
            Lighting.AddLight(gunBarrel, lightVec * (baseLightIntensity + heatLightBonus));
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            // Draw glowing backlight effect when dropped in world
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            // Calculate pulse - gentle like sakura petals falling
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.045f) * 0.08f + 1f;
            
            // Begin additive blending for glow
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Outer crimson aura - blood of heroes
            spriteBatch.Draw(texture, position, null, new Color(180, 50, 50) * 0.4f, rotation, origin, scale * pulse * 1.35f, SpriteEffects.None, 0f);
            
            // Middle pink/sakura glow - cherry blossom essence
            spriteBatch.Draw(texture, position, null, new Color(255, 180, 180) * 0.3f, rotation, origin, scale * pulse * 1.18f, SpriteEffects.None, 0f);
            
            // Inner golden-white glow - heroic valor
            spriteBatch.Draw(texture, position, null, new Color(255, 230, 200) * 0.22f, rotation, origin, scale * pulse * 1.06f, SpriteEffects.None, 0f);
            
            // Return to normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Add lighting
            Lighting.AddLight(Item.Center, 0.6f, 0.35f, 0.3f);
            
            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // === HEAT BUILDUP ===
            heatLevel = Math.Min(heatLevel + 2, MaxHeat);
            heatDecayCooldown = 20; // Delay decay while firing
            float heatProgress = (float)heatLevel / MaxHeat;
            
            // Always use our custom projectile (ignore ammo type)
            type = ModContent.ProjectileType<BlossomOfTheSakuraBulletProjectile>();

            // Add slight random spread
            Vector2 perturbedVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(3));

            Projectile.NewProjectile(source, position, perturbedVelocity, type, damage, knockback, player.whoAmI);

            // === HEAT-REACTIVE MUZZLE FLASH ===
            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.Zero) * 25f;
            
            // Color shifts with heat: Pink → Crimson → Gold → White-hot
            Color muzzleColor;
            if (heatProgress < 0.3f)
                muzzleColor = Color.Lerp(UnifiedVFX.Eroica.Sakura, UnifiedVFX.Eroica.Crimson, heatProgress / 0.3f);
            else if (heatProgress < 0.6f)
                muzzleColor = Color.Lerp(UnifiedVFX.Eroica.Crimson, UnifiedVFX.Eroica.Gold, (heatProgress - 0.3f) / 0.3f);
            else
                muzzleColor = Color.Lerp(UnifiedVFX.Eroica.Gold, Color.White, (heatProgress - 0.6f) / 0.4f);
            
            // Scale and intensity increase with heat
            float flashScale = 0.5f + heatProgress * 0.4f;
            int flashCount = 5 + (int)(heatProgress * 4);
            
            // Geometric muzzle flash that grows with heat
            for (int i = 0; i < flashCount; i++)
            {
                float angle = MathHelper.TwoPi * i / flashCount + velocity.ToRotation();
                float radius = 12f + heatProgress * 8f;
                Vector2 flareOffset = angle.ToRotationVector2() * radius;
                float progress = (float)i / flashCount;
                Color fractalColor = Color.Lerp(muzzleColor, UnifiedVFX.Eroica.Sakura, progress * 0.5f);
                CustomParticles.GenericFlare(muzzlePos + flareOffset, fractalColor, flashScale * 0.7f, 12);
            }
            
            // Central flash
            CustomParticles.GenericFlare(muzzlePos, muzzleColor, flashScale, 10);
            CustomParticles.HaloRing(muzzlePos, muzzleColor * 0.8f, 0.2f + heatProgress * 0.2f, 10);
            
            // === OVERHEATED BONUS EFFECTS ===
            if (heatProgress > 0.7f)
            {
                // Extra flame burst when overheated
                CustomParticles.ExplosionBurst(muzzlePos, UnifiedVFX.Eroica.Gold, 4, 3f);
                
                // Smoke from overheated barrel
                if (Main.rand.NextBool(3))
                {
                    var smoke = new HeavySmokeParticle(muzzlePos, velocity.SafeNormalize(Vector2.Zero) * 2f, 
                        Color.Gray * 0.5f, Main.rand.Next(15, 25), 0.2f, 0.4f, 0.015f, false);
                    MagnumParticleHandler.SpawnParticle(smoke);
                }
            }
            
            // Sakura petals burst (less when overheated - they're burning!)
            if (Main.rand.NextBool(heatProgress > 0.5f ? 5 : 3))
                ThemedParticles.SakuraPetals(muzzlePos, 2, 18f);
            
            // Music notes more frequently
            if (Main.rand.NextBool(4))
                ThemedParticles.EroicaMusicNotes(position, 2, 18f);

            // Bright muzzle light
            Lighting.AddLight(muzzlePos, UnifiedVFX.Eroica.Sakura.ToVector3() * 0.8f);

            return false;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            // Adjust spawn position to gun barrel
            position += velocity.SafeNormalize(Vector2.UnitX * player.direction) * 40f;
        }

        // Recipe removed - drops from Eroica, God of Valor
        // public override void AddRecipes()
        // {
        //     CreateRecipe()
        //         .AddIngredient(ModContent.ItemType<ResonantCoreOfEroica>(), 22)
        //         .AddIngredient(ModContent.ItemType<EroicasResonantEnergy>(), 18)
        //         .AddIngredient(ItemID.LunarBar, 14)
        //         .AddTile(TileID.LunarCraftingStation)
        //         .Register();
        // }
    }
}
