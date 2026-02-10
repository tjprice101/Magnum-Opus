using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.DebugWeapons
{
    /// <summary>
    /// DEBUG WEAPON: HeavySmoke Particle System Test
    /// 
    /// Implements Calamity-style dual-layer heavy smoke exactly like Galaxia.
    /// Uses the DualLayerSmoke.Spawn() helper for proper two-layer rendering:
    /// - Layer 1: Base smoke (normal blend) for volumetric density
    /// - Layer 2: Glow overlay (additive blend) with hue shift for flame effect
    /// 
    /// Each swing spawns dense smoke clouds that merge and animate properly.
    /// </summary>
    public class DebugHEAVY : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.FlareGun;

        public override void SetDefaults()
        {
            Item.width = 60;
            Item.height = 60;
            Item.damage = 500;
            Item.knockBack = 4f;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.DamageType = DamageClass.Melee;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<DebugHEAVYSwungBlade>();
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
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Calamity-style HeavySmoke dual-layer particle test"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Base layer (normal blend) + Glow layer (additive + hue shift)"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Uses DualLayerSmoke.Spawn() - exactly like Galaxia"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Flames dance through billowing smoke clouds.'") 
            { 
                OverrideColor = new Color(255, 140, 80) 
            });
        }
    }

    /// <summary>
    /// The swung blade projectile with HEAVY SMOKE particle rendering.
    /// 
    /// Implements Calamity's Galaxia-style smoke effect:
    /// - Dense particle spawning along swing arc
    /// - Dual-layer system (base + glow)
    /// - Hue shifting for flame animation
    /// - Random delay for staggered particle groups
    /// </summary>
    public class DebugHEAVYSwungBlade : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.TerraBlade;

        private const float SwingWidth = MathHelper.Pi * 1.3f;
        private const int SwingDuration = 25;
        private const float BladeLength = 120f;

        private Player Owner => Main.player[Projectile.owner];
        private ref float Timer => ref Projectile.ai[0];
        private ref float SwingDirection => ref Projectile.ai[1];

        // Galaxia-style Phoenix color palette (VIBRANT orange/red flames)
        private static readonly Color PhoenixBase = new Color(255, 120, 30);
        private static readonly Color PhoenixGlow = new Color(255, 200, 100);
        
        // Polaris-style colors (VIBRANT blue/cyan)
        private static readonly Color PolarisBase = new Color(80, 180, 255);
        private static readonly Color PolarisGlow = new Color(180, 240, 255);
        
        // Andromeda-style colors (VIBRANT purple/pink)
        private static readonly Color AndromedaBase = new Color(180, 80, 255);
        private static readonly Color AndromedaGlow = new Color(220, 160, 255);

        public override void SetDefaults()
        {
            Projectile.width = 140;
            Projectile.height = 140;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.timeLeft = SwingDuration + 5;
            
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void AI()
        {
            // Update rotation history
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
                SoundEngine.PlaySound(SoundID.Item71 with { Pitch = -0.3f }, Projectile.Center);
            }

            Timer++;

            // Calculate swing progress with easing
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
            // PRIMARY EFFECT: HEAVY SMOKE DUAL-LAYER SYSTEM
            // Exactly like Calamity's Galaxia implementation
            // =====================================================
            DoHeavySmokeEffect();

            // Lighting
            Vector2 bladeDir = Projectile.rotation.ToRotationVector2();
            Lighting.AddLight(Owner.Center + bladeDir * BladeLength * 0.5f, PhoenixGlow.ToVector3() * 0.6f);
            Lighting.AddLight(Owner.Center + bladeDir * BladeLength, PhoenixGlow.ToVector3() * 0.8f);

            // End swing
            if (Timer >= SwingDuration)
            {
                Projectile.Kill();
            }
        }

        /// <summary>
        /// HEAVY SMOKE EFFECT - GALAXIA STYLE with vibrant additive blending.
        /// 
        /// Both layers use additive blend now - no more muddy brown!
        /// - Frame animation cycles through spritesheet
        /// - Hue shifting creates flame chaos
        /// - Vibrant saturated colors
        /// </summary>
        private void DoHeavySmokeEffect()
        {
            float swingProgress = Timer / SwingDuration;
            
            // Only spawn during active swing phase
            if (swingProgress < 0.1f || swingProgress > 0.9f) return;
            
            // Intensity peaks in middle of swing
            float intensity = (float)Math.Sin(swingProgress * MathHelper.Pi);
            intensity = (float)Math.Pow(intensity, 0.5f); // Boost intensity curve
            
            Vector2 bladeDir = Projectile.rotation.ToRotationVector2();
            Vector2 perpDir = bladeDir.RotatedBy(MathHelper.PiOver2);
            
            // Moderate particle count for visible effect
            int particleCount = (int)(4 * intensity) + 2;
            
            for (int i = 0; i < particleCount; i++)
            {
                // Position along blade (40% to 100% of length)
                float bladeT = 0.4f + Main.rand.NextFloat(0.6f);
                Vector2 spawnPos = Owner.Center + bladeDir * (BladeLength * bladeT * Projectile.scale);
                
                // Add perpendicular scatter
                spawnPos += perpDir * Main.rand.NextFloat(-18f, 18f);
                
                // Random scatter
                spawnPos += Main.rand.NextVector2Circular(10f, 10f);
                
                // Velocity - perpendicular to blade in swing direction
                Vector2 velocity = perpDir * SwingDirection * Main.rand.NextFloat(4f, 10f) * intensity;
                velocity += bladeDir * Main.rand.NextFloat(-3f, 3f);
                velocity += Owner.velocity * 0.4f;
                
                // VIBRANT flame colors with variation
                Color baseColor = Color.Lerp(PhoenixBase, AndromedaBase, Main.rand.NextFloat(0.35f));
                Color glowColor = Color.Lerp(PhoenixGlow, AndromedaGlow, Main.rand.NextFloat(0.35f));
                
                // Spawn dual-layer smoke - GALAXIA STYLE (large scale, short lifetime)
                DualLayerSmoke.Spawn(
                    position: spawnPos,
                    velocity: velocity,
                    baseColor: baseColor,
                    glowColor: glowColor,
                    lifetime: Main.rand.Next(10, 18),  // SHORT like PhoenixsPride (10-15 frames)
                    scale: Main.rand.NextFloat(2.4f, 3.2f) * intensity,  // LARGE like PhoenixsPride (2.8-3.1)
                    baseOpacity: Main.rand.NextFloat(0.6f, 0.85f),  // Higher opacity for visibility
                    glowOpacity: Main.rand.NextFloat(0.5f, 0.7f),  // Strong glow layer
                    hueShift: Main.rand.NextFloat(0.01f, 0.02f)  // Subtle hue shift
                );
            }
            
            // SHIMMER SPARKLES - bright additive dust for extra vibrancy
            if (Main.rand.NextBool(2))
            {
                float sparkleT = 0.5f + Main.rand.NextFloat(0.5f);
                Vector2 sparklePos = Owner.Center + bladeDir * (BladeLength * sparkleT * Projectile.scale);
                sparklePos += Main.rand.NextVector2Circular(25f, 25f);
                
                Vector2 sparkleVel = perpDir * SwingDirection * Main.rand.NextFloat(2f, 6f);
                sparkleVel += Main.rand.NextVector2Circular(3f, 3f);
                
                // Bright shimmer dust with high visibility
                Dust shimmer = Dust.NewDustPerfect(sparklePos, DustID.FireworkFountain_Yellow, sparkleVel, 0, 
                    Color.Lerp(PhoenixGlow, Color.White, 0.6f), Main.rand.NextFloat(1.0f, 1.5f));
                shimmer.noGravity = true;
                shimmer.fadeIn = 0.6f;
            }
            
            // Extra burst at blade tip
            if (Main.rand.NextBool(2))
            {
                Vector2 tipPos = Owner.Center + bladeDir * BladeLength * Projectile.scale;
                tipPos += Main.rand.NextVector2Circular(10f, 10f);
                
                Vector2 tipVel = perpDir * SwingDirection * Main.rand.NextFloat(5f, 12f);
                tipVel += bladeDir * Main.rand.NextFloat(2f, 5f);
                
                DualLayerSmoke.Spawn(
                    position: tipPos,
                    velocity: tipVel,
                    baseColor: PhoenixBase,
                    glowColor: PhoenixGlow,
                    lifetime: Main.rand.Next(12, 20),  // SHORT punchy smoke
                    scale: Main.rand.NextFloat(2.8f, 3.5f) * intensity,  // LARGE tip smoke
                    baseOpacity: 0.75f,
                    glowOpacity: 0.6f,
                    hueShift: 0.015f
                );
                
                // Tip shimmer sparkle - extra bright
                Dust tipShimmer = Dust.NewDustPerfect(tipPos, DustID.FireworkFountain_Red, tipVel * 0.6f, 0, 
                    Color.White, Main.rand.NextFloat(1.2f, 1.8f));
                tipShimmer.noGravity = true;
            }
            
            // Arc trail (reduced density)
            if (Timer > 1 && Projectile.oldRot.Length > 1 && Main.rand.NextBool(2))
            {
                float prevRotation = Projectile.oldRot[1];
                float arcSpan = Math.Abs(Projectile.rotation - prevRotation);
                
                if (arcSpan > 0.03f)
                {
                    int arcParticles = Math.Max(1, (int)(arcSpan / 0.25f));  // Fewer arc particles
                    for (int i = 0; i < arcParticles; i++)
                    {
                        float t = (float)i / arcParticles;
                        float arcRot = MathHelper.Lerp(prevRotation, Projectile.rotation, t);
                        Vector2 arcDir = arcRot.ToRotationVector2();
                        float arcBladeT = 0.6f + Main.rand.NextFloat(0.4f);
                        Vector2 arcPos = Owner.Center + arcDir * (BladeLength * arcBladeT);
                        arcPos += Main.rand.NextVector2Circular(5f, 5f);
                        
                        Vector2 arcVel = arcDir.RotatedBy(MathHelper.PiOver2) * SwingDirection * Main.rand.NextFloat(3f, 6f);
                        arcVel += Owner.velocity * 0.15f;
                        
                        DualLayerSmoke.Spawn(
                            position: arcPos,
                            velocity: arcVel,
                            baseColor: PhoenixBase,
                            glowColor: PhoenixGlow,
                            lifetime: Main.rand.Next(8, 14),  // SHORT arc smoke
                            scale: Main.rand.NextFloat(1.8f, 2.6f) * intensity,  // LARGE arc smoke
                            baseOpacity: 0.6f,
                            glowOpacity: 0.5f,
                            hueShift: 0.015f
                        );
                    }
                }
            }
        }

        /// <summary>
        /// Piecewise swing animation curve - starts slow, accelerates, then slows at end.
        /// Same pattern used in Galaxia for satisfying swing feel.
        /// </summary>
        private float PiecewiseSwingCurve(float progress)
        {
            if (progress < 0.15f)
            {
                // Wind-up: slow start
                return progress * progress * (3f - 2f * progress) * 0.5f;
            }
            else if (progress < 0.75f)
            {
                // Main swing: fast acceleration
                float t = (progress - 0.15f) / 0.6f;
                return 0.1f + t * 0.8f;
            }
            else
            {
                // Follow-through: slow ending
                float t = (progress - 0.75f) / 0.25f;
                return 0.9f + t * t * (3f - 2f * t) * 0.1f;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Arc collision for melee swing
            Vector2 bladeDir = Projectile.rotation.ToRotationVector2();
            Vector2 bladeEnd = Owner.Center + bladeDir * BladeLength * Projectile.scale;
            
            float collisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), 
                Owner.Center, bladeEnd, 30f, ref collisionPoint);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Draw simple blade with glow
            SpriteBatch sb = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Owner.Center - Main.screenPosition;
            
            float drawRotation = Projectile.rotation + MathHelper.PiOver4;
            if (SwingDirection < 0)
                drawRotation += MathHelper.PiOver2;
            
            SpriteEffects effects = SwingDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;
            
            // Glow layers (additive)
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            for (int i = 0; i < 3; i++)
            {
                float glowScale = Projectile.scale * (1.1f + i * 0.1f);
                Color glowColor = PhoenixGlow with { A = 0 } * (0.4f - i * 0.1f);
                sb.Draw(texture, drawPos, null, glowColor, drawRotation, origin, glowScale, effects, 0f);
            }
            
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, 
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Main blade
            sb.Draw(texture, drawPos, null, Color.White, drawRotation, origin, Projectile.scale, effects, 0f);
            
            return false;
        }
    }
}
