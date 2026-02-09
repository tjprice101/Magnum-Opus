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
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.DebugWeapons
{
    /// <summary>
    /// DEBUG WEAPON: Ark of the Cosmos / Galaxia Style Melee VFX Test
    /// 
    /// Uses the ACTUAL Calamity pattern:
    /// 1. A persistent CircularSmear particle that creates the arc trail
    /// 2. HeavySmokeParticle spawned along blade that drifts away
    /// 3. The smear particle is kept alive by resetting Time = 0 each frame
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
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Ark of the Cosmos / Galaxia style VFX test"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "CircularSmear particle + HeavySmoke drift"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The cosmos burns with every swing.'") 
            { 
                OverrideColor = new Color(255, 140, 60) 
            });
        }
    }

    /// <summary>
    /// CircularSmear particle - kept alive by resetting Time = 0 each frame.
    /// This is the EXACT pattern from Calamity's CircularSmearSmokeyVFX.
    /// </summary>
    public class CircularSmearParticle : Particle
    {
        public override string Texture => "MagnumOpus/Assets/Particles/CircularSmear";
        public override bool UseCustomDraw => true;
        public override bool UseAdditiveBlend => true;
        public override bool SetLifetime => true;
        
        public float Opacity = 1f;
        
        public CircularSmearParticle(Vector2 position, Color color, float rotation, float scale)
        {
            Position = position;
            Velocity = Vector2.Zero;
            Color = color;
            Scale = scale;
            Rotation = rotation;
            Lifetime = 2; // Short lifetime - kept alive by Time = 0 reset
        }
        
        public override void Update()
        {
            // Minimal update - position/rotation/scale set externally each frame
            // Time is auto-incremented by particle handler
        }
        
        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
            if (tex == null) return;
            
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Position - Main.screenPosition;
            
            // Additive blending color (A = 0 for proper additive)
            Color drawColor = Color;
            drawColor.A = 0;
            
            spriteBatch.Draw(tex, drawPos, null, drawColor * Opacity, Rotation, origin, Scale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// The swung blade projectile implementing the ACTUAL Galaxia/Ark of the Cosmos pattern.
    /// 
    /// KEY INSIGHT FROM CALAMITY SOURCE:
    /// - Store a single CircularSmear particle as a class field
    /// - Each frame, update its Position, Rotation, Scale, Color
    /// - Reset Time = 0 to keep it alive
    /// - Spawn HeavySmokeParticle along blade that drifts away
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

        // Fire color ramp
        private static readonly Color ColorHottest = new Color(255, 255, 220);
        private static readonly Color ColorHot = new Color(255, 180, 60);
        private static readonly Color ColorMid = new Color(255, 80, 40);
        private static readonly Color ColorCool = new Color(180, 40, 80);

        // THE KEY: Persistent smear particle reference
        private CircularSmearParticle smear = null;

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
        }

        public override void AI()
        {
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
            // THE CALAMITY PATTERN: Persistent smear particle
            // =====================================================
            DoSmearParticleEffect();

            // =====================================================
            // Spawn smoke particles along blade that drift away
            // =====================================================
            DoSmokeParticles();

            // Lighting
            Vector2 bladeDir = Projectile.rotation.ToRotationVector2();
            Lighting.AddLight(Owner.Center + bladeDir * BladeLength, ColorHot.ToVector3() * 0.9f);
            Lighting.AddLight(Owner.Center + bladeDir * (BladeLength * 0.5f), ColorMid.ToVector3() * 0.6f);

            // End swing
            if (Timer >= SwingDuration)
            {
                Projectile.Kill();
            }
        }

        /// <summary>
        /// The EXACT pattern from Calamity's Galaxia/ArkOfTheCosmos:
        /// - If smear is null, create it and spawn via particle handler
        /// - If smear exists, update its properties and reset Time = 0 to keep alive
        /// </summary>
        private void DoSmearParticleEffect()
        {
            float swingProgress = Timer / SwingDuration;
            
            // Color shifts through swing
            Color currentColor = Color.Lerp(ColorHot, Color.Chocolate, swingProgress);
            currentColor *= MathHelper.Clamp((float)Math.Sin(swingProgress * MathHelper.Pi), 0.3f, 1f);
            
            // Opacity based on swing progress (fade in, sustain, fade out)
            float opacity = (float)Math.Sin(swingProgress * MathHelper.Pi) * 0.6f;
            
            // Scale pulses slightly
            float scale = 1.8f + (float)Math.Sin(swingProgress * MathHelper.Pi) * 0.4f;
            
            if (smear == null)
            {
                // Create the smear particle
                smear = new CircularSmearParticle(
                    Owner.Center, 
                    currentColor, 
                    Projectile.rotation + MathHelper.PiOver4, 
                    scale
                );
                smear.Opacity = opacity;
                MagnumParticleHandler.SpawnParticle(smear);
            }
            else
            {
                // Update the smear each frame - THIS IS THE KEY
                smear.Rotation = Projectile.rotation + MathHelper.PiOver4 + (SwingDirection < 0 ? MathHelper.Pi : 0f);
                smear.Time = 0; // RESET TIME TO KEEP ALIVE
                smear.Position = Owner.Center;
                smear.Scale = scale;
                smear.Color = currentColor;
                smear.Opacity = opacity;
            }
        }

        /// <summary>
        /// Spawn smoke particles along the blade that drift away.
        /// This creates the "muddy fire" effect as particles linger in the air.
        /// </summary>
        private void DoSmokeParticles()
        {
            float swingProgress = Timer / SwingDuration;
            
            // Only spawn during active swing (not at very start/end)
            if (swingProgress < 0.1f || swingProgress > 0.9f) return;
            
            float scaleFactor = MathHelper.Clamp((float)Math.Sin(swingProgress * MathHelper.Pi), 0f, 1f);
            float smokeOpacity = scaleFactor * 0.4f;
            
            // Spawn smoke at multiple points along blade
            if (Main.rand.NextBool())
            {
                for (float i = 0.3f; i <= 1f; i += 0.35f)
                {
                    Vector2 smokePos = Owner.Center + 
                        (Projectile.rotation.ToRotationVector2() * (BladeLength * i * Projectile.scale)) + 
                        Projectile.rotation.ToRotationVector2().RotatedBy(-MathHelper.PiOver2) * 30f * scaleFactor * Main.rand.NextFloat();
                    
                    Vector2 smokeSpeed = Projectile.rotation.ToRotationVector2().RotatedBy(-MathHelper.PiOver2 * SwingDirection) * 20f * scaleFactor + Owner.velocity;
                    
                    // Color gradient along blade
                    Color smokeColor = Color.Lerp(Color.DodgerBlue, Color.MediumVioletRed, i);
                    
                    // Spawn the smoke particle
                    var smoke = new HeavySmokeParticle(
                        smokePos, 
                        smokeSpeed, 
                        smokeColor, 
                        6 + Main.rand.Next(5), 
                        scaleFactor * Main.rand.NextFloat(2.5f, 3f), 
                        smokeOpacity + Main.rand.NextFloat(0f, 0.15f),
                        0f, 
                        false
                    );
                    MagnumParticleHandler.SpawnParticle(smoke);
                    
                    // Occasional golden highlight smoke
                    if (Main.rand.NextBool(3))
                    {
                        var glowSmoke = new HeavySmokeParticle(
                            smokePos, 
                            smokeSpeed, 
                            Main.rand.NextBool(5) ? Color.Gold : Color.Chocolate, 
                            5, 
                            scaleFactor * Main.rand.NextFloat(2f, 2.4f), 
                            smokeOpacity * 2f,
                            0f, 
                            true // Glowing
                        );
                        MagnumParticleHandler.SpawnParticle(glowSmoke);
                    }
                }
            }
            
            // Spawn dust for extra sparkle
            SpawnDust();
        }

        private void SpawnDust()
        {
            Vector2 bladeDir = Projectile.rotation.ToRotationVector2();
            
            int dustCount = Main.rand.Next(2, 4);
            for (int i = 0; i < dustCount; i++)
            {
                float t = Main.rand.NextFloat(0.4f, 1f);
                Vector2 dustPos = Owner.Center + bladeDir * (BladeLength * t);
                
                Vector2 perp = bladeDir.RotatedBy(MathHelper.PiOver2);
                dustPos += perp * Main.rand.NextFloat(-12f, 12f);
                
                Vector2 vel = -bladeDir * Main.rand.NextFloat(1f, 3f);
                vel += perp * Main.rand.NextFloat(-2f, 2f) * SwingDirection;
                vel.Y -= Main.rand.NextFloat(0.5f, 1.5f);
                
                int dustType = Main.rand.NextBool(3) ? DustID.Torch : DustID.SolarFlare;
                
                Dust dust = Dust.NewDustPerfect(dustPos, dustType, vel, 0, default, Main.rand.NextFloat(0.8f, 1.4f));
                dust.noGravity = true;
                dust.fadeIn = 0.8f;
            }
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
            SpriteBatch sb = Main.spriteBatch;
            
            Texture2D bladeTex = TextureAssets.Item[ItemID.TerraBlade].Value;
            if (bladeTex == null) return false;
            
            Vector2 bladeOrigin = new Vector2(0, bladeTex.Height / 2f);
            Vector2 drawPos = Owner.Center - Main.screenPosition;
            float bladeScale = BladeLength / bladeTex.Width;
            SpriteEffects flip = SwingDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;
            
            // Glow layers
            Color layer1 = new Color(255, 100, 50) * 0.2f;
            sb.Draw(bladeTex, drawPos, null, layer1, Projectile.rotation, bladeOrigin, bladeScale * 1.15f, flip, 0f);
            
            Color layer2 = new Color(255, 160, 60) * 0.25f;
            sb.Draw(bladeTex, drawPos, null, layer2, Projectile.rotation, bladeOrigin, bladeScale * 1.08f, flip, 0f);
            
            Color layer3 = new Color(255, 220, 120) * 0.3f;
            sb.Draw(bladeTex, drawPos, null, layer3, Projectile.rotation, bladeOrigin, bladeScale * 1.03f, flip, 0f);
            
            // Main blade
            sb.Draw(bladeTex, drawPos, null, Color.White, Projectile.rotation, bladeOrigin, bladeScale, flip, 0f);
            
            return false;
        }
    }
}
