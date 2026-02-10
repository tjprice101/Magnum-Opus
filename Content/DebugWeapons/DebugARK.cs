using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;
using Terraria.GameContent;
using MagnumOpus.Common.Systems.Metaballs;

namespace MagnumOpus.Content.DebugWeapons
{
    /// <summary>
    /// DEBUG WEAPON: Galaxia/Ark of the Cosmos Style Metaball VFX Test
    /// 
    /// TERRA BLADE GREEN EDITION with proper Calamity-style metaball cosmic cloud effect!
    /// 
    /// The metaball system creates smooth, merging blob shapes that use the
    /// two cosmic layer textures (EmeraldCosmicNebula + ToxicEnergyVortex) to
    /// create the swirling cosmic energy effect seen in Galaxia.
    /// </summary>
    public class DebugARK : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.TerraBlade;

        public override void SetDefaults()
        {
            Item.width = 80;
            Item.height = 80;
            Item.damage = 500;
            Item.knockBack = 6f;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.DamageType = DamageClass.Melee;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<DebugARKSwungBlade>();
            Item.shootSpeed = 1f;
            Item.rare = ItemRarityID.Red;
            Item.value = Item.sellPrice(gold: 50);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, player.Center, Vector2.Zero, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Galaxia/Ark of the Cosmos style METABALL VFX test"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Terra Blade Green - Cosmic cloud metaballs merge together"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Emerald chaos flows through the blade like nebula clouds.'") 
            { 
                OverrideColor = new Color(50, 255, 100) 
            });
        }
    }

    /// <summary>
    /// The swung blade projectile with METABALL cosmic cloud rendering.
    /// 
    /// The PRIMARY effect is now the metaball system - smooth, merging blobs
    /// of cosmic energy that follow the swing path. Secondary effects are
    /// minimal to let the metaballs shine.
    /// </summary>
    public class DebugARKSwungBlade : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.TerraBlade;

        private const float SwingWidth = MathHelper.Pi * 1.3f;
        private const int SwingDuration = 30;
        private const float BladeLength = 140f;

        private Player Owner => Main.player[Projectile.owner];
        private ref float Timer => ref Projectile.ai[0];
        private ref float SwingDirection => ref Projectile.ai[1];

        // Terra Blade green color palette
        private static readonly Color ColorNeonGreen = new Color(120, 255, 160);
        private static readonly Color ColorBrightGreen = new Color(80, 255, 120);
        private static readonly Color ColorMidGreen = new Color(50, 200, 90);
        private static readonly Color ColorDarkGreen = new Color(30, 140, 60);

        public override void SetDefaults()
        {
            Projectile.width = 160;
            Projectile.height = 160;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.timeLeft = SwingDuration + 5;
            
            // Simple trail for blade afterimages
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void AI()
        {
            // Update rotation history manually (held projectile doesn't move)
            for (int i = Projectile.oldRot.Length - 1; i > 0; i--)
            {
                Projectile.oldRot[i] = Projectile.oldRot[i - 1];
            }
            Projectile.oldRot[0] = Projectile.rotation;
            
            // Lock to owner
            Projectile.Center = Owner.Center;
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;

            // Initialize
            if (Timer == 0)
            {
                SwingDirection = Owner.direction;
                SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.2f }, Projectile.Center);
            }

            Timer++;

            // Calculate swing progress
            float swingProgress = MathHelper.Clamp(Timer / SwingDuration, 0f, 1f);
            float animatedProgress = PiecewiseSwingCurve(swingProgress);

            // Calculate current swing angle
            float startAngle = -SwingWidth / 2f * SwingDirection;
            float endAngle = SwingWidth / 2f * SwingDirection;
            float currentAngle = MathHelper.Lerp(startAngle, endAngle, animatedProgress);

            // Base angle toward mouse
            Vector2 toMouse = Main.MouseWorld - Owner.Center;
            float baseAngle = toMouse.ToRotation();
            Projectile.rotation = baseAngle + currentAngle;

            // Update owner arm
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, 
                Projectile.rotation - MathHelper.PiOver2);

            // =====================================================
            // PRIMARY EFFECT: METABALL COSMIC CLOUD
            // This is the main visual - merging blob particles
            // =====================================================
            DoMetaballEffect();

            // =====================================================
            // SECONDARY: Minimal dust for sparkle accents
            // =====================================================
            if (swingProgress > 0.1f && swingProgress < 0.9f && Main.rand.NextBool(3))
            {
                SpawnAccentDust();
            }

            // Lighting
            Vector2 bladeDir = Projectile.rotation.ToRotationVector2();
            Lighting.AddLight(Owner.Center + bladeDir * BladeLength, ColorBrightGreen.ToVector3() * 0.8f);

            // End swing
            if (Timer >= SwingDuration)
            {
                Projectile.Kill();
            }
        }

        /// <summary>
        /// METABALL EFFECT - The primary visual!
        /// Spawns merging blob particles along the swing path.
        /// These particles use the TerraMetaball system which renders with
        /// the two cosmic layer textures for that Galaxia-like swirling effect.
        /// </summary>
        private void DoMetaballEffect()
        {
            float swingProgress = Timer / SwingDuration;
            
            // Only spawn during active swing
            if (swingProgress < 0.08f || swingProgress > 0.92f) return;
            
            // Intensity peaks in middle of swing
            float intensity = MathHelper.Clamp((float)Math.Sin(swingProgress * MathHelper.Pi), 0f, 1f);
            intensity = (float)Math.Pow(intensity, 0.5f); // More aggressive intensity
            
            Vector2 bladeDir = Projectile.rotation.ToRotationVector2();
            Vector2 perpDir = bladeDir.RotatedBy(MathHelper.PiOver2);
            
            // Spawn MANY particles for denser, more visible cloud
            int particleCount = (int)(7 * intensity) + 4;
            
            for (int i = 0; i < particleCount; i++)
            {
                // Position along blade (20% to 110% of length for full coverage)
                float bladeT = 0.2f + Main.rand.NextFloat(0.9f);
                Vector2 spawnPos = Owner.Center + bladeDir * (BladeLength * bladeT * Projectile.scale);
                
                // Add perpendicular scatter for width
                spawnPos += perpDir * Main.rand.NextFloat(-30f, 30f);
                
                // Add some random scatter
                spawnPos += Main.rand.NextVector2Circular(12f, 12f);
                
                // Velocity - perpendicular to blade in swing direction
                // This makes the cloud "trail" behind the swing
                Vector2 velocity = perpDir * SwingDirection * Main.rand.NextFloat(5f, 14f) * intensity;
                velocity += bladeDir * Main.rand.NextFloat(-4f, 4f);
                velocity += Owner.velocity * 0.5f;
                
                // Size - MUCH LARGER particles for more visible effect
                // Galaxia uses roughly 80-200 size range
                float size = Main.rand.NextFloat(80f, 180f) * intensity;
                
                // Spawn the metaball particle!
                TerraMetaball.SpawnParticle(spawnPos, velocity, size);
            }
            
            // Extra LARGE burst at blade tip for emphasis
            for (int j = 0; j < 2; j++)
            {
                Vector2 tipPos = Owner.Center + bladeDir * BladeLength * Projectile.scale;
                tipPos += Main.rand.NextVector2Circular(15f, 15f);
                
                Vector2 tipVel = perpDir * SwingDirection * Main.rand.NextFloat(8f, 18f);
                tipVel += bladeDir * Main.rand.NextFloat(3f, 7f);
                tipVel += Main.rand.NextVector2Circular(3f, 3f);
                
                float tipSize = Main.rand.NextFloat(120f, 200f) * intensity;  // EXTRA LARGE at tip
                TerraMetaball.SpawnParticle(tipPos, tipVel, tipSize);
            }
            
            // Spawn along the arc that was just traveled for continuous trail
            if (Timer > 1)
            {
                float prevRotation = Projectile.oldRot[1];
                float arcSpan = Math.Abs(Projectile.rotation - prevRotation);
                
                // Spawn MORE particles along the arc for denser trail
                int arcParticles = Math.Max(2, (int)(arcSpan / 0.08f));
                for (int i = 0; i < arcParticles; i++)
                {
                    float t = (float)i / arcParticles;
                    float arcRot = MathHelper.Lerp(prevRotation, Projectile.rotation, t);
                    Vector2 arcDir = arcRot.ToRotationVector2();
                    
                    float arcT = 0.4f + Main.rand.NextFloat(0.6f);
                    Vector2 arcPos = Owner.Center + arcDir * (BladeLength * arcT);
                    arcPos += Main.rand.NextVector2Circular(15f, 15f);
                    
                    Vector2 arcPerp = arcDir.RotatedBy(MathHelper.PiOver2);
                    Vector2 arcVel = arcPerp * SwingDirection * Main.rand.NextFloat(4f, 12f);
                    
                    float arcSize = Main.rand.NextFloat(60f, 120f) * intensity;  // LARGER arc particles
                    TerraMetaball.SpawnParticle(arcPos, arcVel, arcSize);
                }
            }
        }

        /// <summary>
        /// Minimal accent dust - just a few sparkles to complement the metaballs.
        /// </summary>
        private void SpawnAccentDust()
        {
            Vector2 bladeDir = Projectile.rotation.ToRotationVector2();
            
            float t = Main.rand.NextFloat(0.5f, 1f);
            Vector2 dustPos = Owner.Center + bladeDir * (BladeLength * t);
            dustPos += Main.rand.NextVector2Circular(15f, 15f);
            
            Vector2 vel = -bladeDir * Main.rand.NextFloat(0.5f, 2f);
            vel.Y -= Main.rand.NextFloat(0.5f, 1f);
            
            // Green dust types
            int dustType = Main.rand.Next(3) switch
            {
                0 => DustID.TerraBlade,
                1 => DustID.GreenFairy,
                _ => DustID.GemEmerald
            };
            
            Dust dust = Dust.NewDustPerfect(dustPos, dustType, vel, 0, default, Main.rand.NextFloat(0.8f, 1.4f));
            dust.noGravity = true;
        }

        private float PiecewiseSwingCurve(float progress)
        {
            if (progress < 0.15f)
            {
                float t = progress / 0.15f;
                return t * t * 0.15f;
            }
            else if (progress < 0.65f)
            {
                float t = (progress - 0.15f) / 0.5f;
                float ease = 1f - MathF.Pow(1f - t, 3f);
                return 0.15f + ease * 0.75f;
            }
            else
            {
                float t = (progress - 0.65f) / 0.35f;
                return 0.9f + t * 0.1f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D sword = TextureAssets.Item[ItemID.TerraBlade].Value;
            if (sword == null) return false;
            
            Vector2 drawOrigin = new Vector2(0f, sword.Height);
            Vector2 baseDrawOffset = Owner.Center - Main.screenPosition;
            float bladeScale = BladeLength / sword.Width;
            
            // ================================================================
            // SIMPLIFIED AFTERIMAGES - Let metaballs be the star
            // ================================================================
            for (int i = 1; i < Projectile.oldRot.Length; i++)
            {
                if (Projectile.oldRot[i] == 0f) continue;
                
                float progress = i / (float)Projectile.oldRot.Length;
                Color trailColor = Color.Lerp(ColorBrightGreen, ColorDarkGreen, progress) * (1f - progress);
                trailColor *= 0.5f; // Subtle afterimage
                
                float trailRot = Projectile.oldRot[i] + MathHelper.PiOver4;
                float trailScale = bladeScale * (1f - progress * 0.2f);
                
                Main.spriteBatch.Draw(sword, baseDrawOffset, null, trailColor, trailRot, 
                    drawOrigin, trailScale, SpriteEffects.None, 0f);
            }
            
            // ================================================================
            // CURRENT BLADE
            // ================================================================
            float currentRotation = Projectile.rotation + MathHelper.PiOver4;
            Main.spriteBatch.Draw(sword, baseDrawOffset, null, lightColor, currentRotation, 
                drawOrigin, bladeScale, SpriteEffects.None, 0f);
            
            // ================================================================
            // SIMPLE GLOW - Green additive glow on blade
            // ================================================================
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, 
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            
            Color glow = ColorBrightGreen with { A = 0 };
            Main.spriteBatch.Draw(sword, baseDrawOffset, null, glow * 0.4f, currentRotation, 
                drawOrigin, bladeScale * 1.1f, SpriteEffects.None, 0f);
            
            Color glow2 = ColorNeonGreen with { A = 0 };
            Main.spriteBatch.Draw(sword, baseDrawOffset, null, glow2 * 0.25f, currentRotation, 
                drawOrigin, bladeScale * 1.2f, SpriteEffects.None, 0f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, 
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
    }
}
