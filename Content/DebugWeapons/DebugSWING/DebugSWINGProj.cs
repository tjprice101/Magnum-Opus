using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using static MagnumOpus.Common.Systems.Particles.Particle;

namespace MagnumOpus.Content.DebugWeapons.DebugSWING
{
    /// <summary>
    /// DEBUG PROJECTILE: Exo Blade Swing Animation
    /// 
    /// This projectile replicates the Exo Blade's swing mechanics:
    /// 1. Piecewise animation curves (CurveSegment) for smooth swing
    /// 2. Blade stretching/squishing based on swing angle
    /// 3. SwingSprite shader for blade sprite deformation (with fallback)
    /// 4. Slash trail using primitive rendering
    /// 
    /// ANIMATION BREAKDOWN:
    /// - SlowStart: Wind-up phase (0-27% of swing)
    /// - SwingFast: Main swing acceleration (27-85% of swing)
    /// - EndSwing: Deceleration/follow-through (85-100% of swing)
    /// 
    /// COLOR PALETTE (Exoblade Rainbow):
    /// Cyan → Lime → GreenYellow → Goldenrod → Orange
    /// </summary>
    public class DebugSWINGProj : ModProjectile
    {
        // Use Coda of Annihilation as the swing blade texture
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/CodaOfAnnihilation";
        
        // Fallback texture if Coda isn't available
        private const string FallbackTexture = "Terraria/Images/Item_" + nameof(ItemID.TerraBlade);
        
        // Noise texture for trail shader effects
        private const string NoiseTexturePath = "MagnumOpus/Assets/VFX/Noise/VoronoiNoise";

        #region Constants
        private const float BladeLength = 180f;
        private const float MaxSwingAngle = MathHelper.PiOver2 * 1.8f; // 162 degrees total swing arc
        #endregion

        #region Properties
        private Player Owner => Main.player[Projectile.owner];

        /// <summary>
        /// The total swing time in frames.
        /// </summary>
        public int SwingTime
        {
            get => (int)Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }

        /// <summary>
        /// Squish factor for the blade (0.67f - 1f).
        /// Lower = more squished/stretched perpendicular to swing.
        /// </summary>
        public float SquishFactor
        {
            get => Projectile.localAI[1];
            set => Projectile.localAI[1] = value;
        }

        /// <summary>
        /// Current timer within the swing (frames elapsed).
        /// </summary>
        private float Timer => SwingTime - Projectile.timeLeft;

        /// <summary>
        /// Swing progression from 0 to 1.
        /// </summary>
        private float Progression => SwingTime > 0 ? Timer / SwingTime : 0f;

        /// <summary>
        /// Whether the projectile is in post-swing stasis (cooldown).
        /// </summary>
        public bool InPostSwingStasis
        {
            get => Projectile.ai[1] > 0;
            set => Projectile.ai[1] = value ? 1 : 0;
        }

        /// <summary>
        /// Direction of swing (-1 = left, 1 = right).
        /// </summary>
        private int Direction => Math.Sign(Projectile.velocity.X) <= 0 ? -1 : 1;

        /// <summary>
        /// Base rotation (direction player is facing).
        /// </summary>
        private float BaseRotation => Projectile.velocity.ToRotation();

        /// <summary>
        /// Vector representing the squish transformation.
        /// X = horizontal stretch, Y = vertical squish based on SquishFactor.
        /// </summary>
        private Vector2 SquishVector => new Vector2(1f + (1 - SquishFactor) * 0.6f, SquishFactor);
        #endregion

        #region Piecewise Animation Curves
        // These CurveSegments define the Exo Blade's characteristic swing motion:
        // - Slow start with anticipation
        // - Fast main swing
        // - Gentle ending

        /// <summary>
        /// SlowStart: Wind-up phase. Eases out with a slight overshoot.
        /// StartX=0, StartY=-1 (start behind), Lift=0.3 (move forward 30%)
        /// </summary>
        private readonly CurveSegment SlowStart = new CurveSegment(EasingType.PolyOut, 0f, -1f, 0.3f, 2);

        /// <summary>
        /// SwingFast: Main acceleration phase. Fast swing through.
        /// StartX=0.27, StartY=-0.7, Lift=1.6 (swing through 160%)
        /// </summary>
        private readonly CurveSegment SwingFast = new CurveSegment(EasingType.PolyIn, 0.27f, -0.7f, 1.6f, 4);

        /// <summary>
        /// EndSwing: Deceleration/follow-through.
        /// StartX=0.85, StartY=0.9, Lift=0.1 (slow to 100%)
        /// </summary>
        private readonly CurveSegment EndSwing = new CurveSegment(EasingType.PolyOut, 0.85f, 0.9f, 0.1f, 2);

        /// <summary>
        /// Calculate the swing angle shift at a given progress (0-1).
        /// </summary>
        private float SwingAngleShiftAtProgress(float progress)
        {
            return MaxSwingAngle * PiecewiseAnimation(progress, new CurveSegment[] { SlowStart, SwingFast, EndSwing });
        }

        /// <summary>
        /// Calculate the sword rotation at a given progress.
        /// </summary>
        private float SwordRotationAtProgress(float progress)
        {
            return BaseRotation + SwingAngleShiftAtProgress(progress) * Direction;
        }

        /// <summary>
        /// Calculate the squish factor at a given progress based on swing angle.
        /// </summary>
        private float SquishAtProgress(float progress)
        {
            float angleShift = Math.Abs(SwingAngleShiftAtProgress(progress));
            return MathHelper.Lerp(SquishVector.X, SquishVector.Y, (float)Math.Abs(Math.Sin(angleShift)));
        }

        /// <summary>
        /// Calculate the sword direction vector at a given progress (includes stretch).
        /// </summary>
        private Vector2 DirectionAtProgress(float progress)
        {
            return SwordRotationAtProgress(progress).ToRotationVector2() * SquishAtProgress(progress);
        }

        // Current frame values
        private float SwingAngleShift => SwingAngleShiftAtProgress(Progression);
        private float SwordRotation => SwordRotationAtProgress(Progression);
        private float CurrentSquish => SquishAtProgress(Progression);
        private Vector2 SwordDirection => DirectionAtProgress(Progression);
        #endregion

        #region Trail Points Generation
        /// <summary>
        /// The "end point" progression for the trail (trails behind current position).
        /// </summary>
        private float TrailEndProgression
        {
            get
            {
                float endProgression;
                if (Progression < 0.75f)
                    endProgression = Progression - 0.5f + 0.1f * (Progression / 0.75f);
                else
                    endProgression = Progression - 0.4f * (1 - (Progression - 0.75f) / 0.75f);

                return Math.Clamp(endProgression, 0, 1);
            }
        }

        /// <summary>
        /// Get the progression at a given completion ratio along the trail.
        /// </summary>
        private float RealProgressionAtTrailCompletion(float completion)
        {
            return MathHelper.Lerp(Progression, TrailEndProgression, completion);
        }

        /// <summary>
        /// Direction at progress, adjusted for better trail rendering at high squish.
        /// </summary>
        private Vector2 DirectionAtProgressScuffed(float progress)
        {
            float angleShift = SwingAngleShiftAtProgress(progress);

            // Get the coordinates of the angle shift
            Vector2 anglePoint = angleShift.ToRotationVector2();

            // Squish the angle point's coordinate
            anglePoint.X *= SquishVector.X;
            anglePoint.Y *= SquishVector.Y;

            // And back into an angle
            angleShift = anglePoint.ToRotation();

            return (BaseRotation + angleShift * Direction).ToRotationVector2() * SquishAtProgress(progress);
        }

        /// <summary>
        /// Generate points for the slash trail primitive.
        /// </summary>
        private List<Vector2> GenerateSlashPoints()
        {
            List<Vector2> result = new List<Vector2>();

            for (int i = 0; i < 40; i++)
            {
                float progress = MathHelper.Lerp(Progression, TrailEndProgression, i / 40f);
                result.Add(DirectionAtProgressScuffed(progress) * (BladeLength - 6f) * Projectile.scale);
            }

            return result;
        }
        #endregion

        #region Risk of Dust (for future particle effects)
        /// <summary>
        /// Probability of spawning dust at current swing progress.
        /// </summary>
        private float RiskOfDust
        {
            get
            {
                if (Progression > 0.85f)
                    return 0;
                if (Progression < 0.4f)
                    return (float)Math.Pow(Progression / 0.3f, 2) * 0.2f;
                if (Progression < 0.5f)
                    return 0.2f + 0.7f * (Progression - 0.4f) / 0.1f;
                return 0.9f;
            }
        }
        #endregion

        #region Projectile Setup
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 120;
        }

        public override void SetDefaults()
        {
            Projectile.width = 98;
            Projectile.height = 98;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 9999;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.MaxUpdates = 3;
            Projectile.localNPCHitCooldown = Projectile.MaxUpdates * 8;
            Projectile.noEnchantmentVisuals = true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(SwingTime);
            writer.Write(SquishFactor);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            SwingTime = (int)reader.ReadSingle();
            SquishFactor = reader.ReadSingle();
        }
        #endregion

        #region Collision
        public override bool ShouldUpdatePosition() => false;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            Vector2 start = Projectile.Center;
            Vector2 end = start + SwordDirection * (BladeLength + 50) * Projectile.scale;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, Projectile.scale * 30f, ref _);
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize the swing animation.
        /// </summary>
        private void InitializationEffects(bool firstSwing)
        {
            // Recalculate swing direction based on mouse position
            Projectile.velocity = Owner.MountedCenter.DirectionTo(Main.MouseWorld);

            // Random squish factor for visual variety
            SquishFactor = Main.rand.NextFloat(0.67f, 1f);

            // First swing starts small (scale grows)
            if (firstSwing)
                Projectile.scale = 0.02f;
            else
                Projectile.scale = 1f;

            // Set swing duration (78 frames base with 3x extra updates)
            SwingTime = 78;
            Projectile.timeLeft = SwingTime;

            Projectile.netUpdate = true;
        }
        #endregion

        #region AI
        public override void AI()
        {
            if (InPostSwingStasis || Projectile.timeLeft == 0)
                return;

            // Initialize on first frame or when continuing swing
            if (Projectile.timeLeft >= 9999 || (Projectile.timeLeft == 1 && Owner.channel))
                InitializationEffects(Projectile.timeLeft >= 9999);

            // Perform swing behavior
            DoBehavior_Swinging();

            // Glue the sword to its owner
            Projectile.Center = Owner.RotatedRelativePoint(Owner.MountedCenter, true);
            Owner.heldProj = Projectile.whoAmI;
            Owner.SetDummyItemTime(2);
            Owner.ChangeDir(Direction);

            // Decide the arm rotation for the owner
            float armRotation = SwordRotation - MathHelper.PiOver2;
            Owner.SetCompositeArmFront(Math.Abs(armRotation) > 0.01f, Player.CompositeArmStretchAmount.Full, armRotation);
        }

        /// <summary>
        /// Main swing behavior - handles animation and effects.
        /// </summary>
        private void DoBehavior_Swinging()
        {
            // Dynamic lighting along blade using Exoblade rainbow palette
            Color lightColor = SwingShaderSystem.GetExobladeColor(Progression);
            float lightIntensity = (float)Math.Sin(Progression * MathHelper.Pi) * 1.6f;
            Lighting.AddLight(
                Owner.MountedCenter + SwordDirection * 100,
                lightColor.ToVector3() * lightIntensity
            );

            // Scale growth/shrink
            float idealSize = 1f;
            if (Projectile.scale < idealSize)
                Projectile.scale = MathHelper.Lerp(Projectile.scale, idealSize, 0.08f);

            // Shrink near end of swing if not channeling
            if (!Owner.channel && Progression > 0.7f)
                Projectile.scale = (0.5f + 0.5f * (float)Math.Pow(1 - (Progression - 0.7f) / 0.3f, 0.5)) * idealSize;
        }
        #endregion

        #region Drawing
        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.Opacity <= 0f || InPostSwingStasis)
                return false;

            // Draw in correct order: trail behind, then blade, then flare on top
            DrawSlashTrail();
            DrawBlade();
            DrawLensFlare();

            return false;
        }

        /// <summary>
        /// Draw the slash trail behind the blade using Exoblade-style rainbow gradient.
        /// </summary>
        private void DrawSlashTrail()
        {
            // Only draw trail during active swing portion (after wind-up)
            if (Progression < 0.35f)
                return;

            List<Vector2> slashPoints = GenerateSlashPoints();
            
            // Use primitive trail system if available, otherwise fallback
            DrawExobladeStyleTrail(slashPoints);
        }

        /// <summary>
        /// Draw Exoblade-style rainbow trail with glow effect.
        /// </summary>
        private void DrawExobladeStyleTrail(List<Vector2> points)
        {
            if (points.Count < 2)
                return;

            SpriteBatch spriteBatch = Main.spriteBatch;
            
            // Get glow textures
            Texture2D glowTex = Terraria.GameContent.TextureAssets.Extra[98].Value; // Vanilla glow
            Texture2D bloomTex = Terraria.GameContent.TextureAssets.Projectile[ProjectileID.RainbowRodBullet].Value;
            
            // Begin additive blending for glow
            SwingShaderSystem.BeginAdditive(spriteBatch);
            
            // Draw trail segments with rainbow color cycling
            for (int i = 0; i < points.Count - 1; i++)
            {
                float completion = (float)i / points.Count;
                float nextCompletion = (float)(i + 1) / points.Count;
                
                // Calculate opacity fade
                float opacity = Utils.GetLerpValue(0.95f, 0.4f, completion, true) * Projectile.Opacity;
                
                // Get width based on swing progress
                float realProgress = RealProgressionAtTrailCompletion(completion);
                float width = SquishAtProgress(realProgress) * Projectile.scale * 55f;
                
                Vector2 start = Projectile.Center + points[i] - Main.screenPosition;
                Vector2 end = Projectile.Center + points[i + 1] - Main.screenPosition;
                
                // Rainbow color using Exoblade palette
                // Offset the color by completion so it cycles along the trail
                float colorProgress = (completion + Progression * 0.5f) % 1f;
                Color trailColor = SwingShaderSystem.GetExobladeColor(colorProgress);
                trailColor.A = 0; // Additive blend
                
                // Draw glow segment
                Vector2 diff = end - start;
                float rotation = diff.ToRotation();
                float length = diff.Length();
                
                if (length < 1f)
                    continue;
                
                // Main trail glow
                spriteBatch.Draw(
                    glowTex,
                    start,
                    null,
                    trailColor * opacity * 0.7f,
                    rotation,
                    new Vector2(0, glowTex.Height / 2f),
                    new Vector2(length / glowTex.Width, width / glowTex.Height * 0.6f),
                    SpriteEffects.None,
                    0f
                );
                
                // Inner brighter core
                Color coreColor = Color.Lerp(trailColor, Color.White, 0.4f);
                coreColor.A = 0;
                spriteBatch.Draw(
                    glowTex,
                    start,
                    null,
                    coreColor * opacity * 0.9f,
                    rotation,
                    new Vector2(0, glowTex.Height / 2f),
                    new Vector2(length / glowTex.Width, width / glowTex.Height * 0.3f),
                    SpriteEffects.None,
                    0f
                );
            }
            
            // Restore normal blending
            SwingShaderSystem.RestoreSpriteBatch(spriteBatch);
        }

        /// <summary>
        /// Simple trail drawing without shaders (fallback).
        /// </summary>
        private void DrawSimpleTrail(List<Vector2> points)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            
            // Get a soft glow texture
            Texture2D glowTex = Terraria.GameContent.TextureAssets.Extra[98].Value; // Vanilla glow texture
            
            for (int i = 0; i < points.Count - 1; i++)
            {
                float completion = (float)i / points.Count;
                float opacity = Utils.GetLerpValue(0.9f, 0.4f, completion, true) * Projectile.Opacity;
                float width = SquishAtProgress(RealProgressionAtTrailCompletion(completion)) * Projectile.scale * 60.5f;
                
                Vector2 start = Projectile.Center + points[i] - Main.screenPosition;
                Vector2 end = Projectile.Center + points[i + 1] - Main.screenPosition;
                
                // Get gradient color
                Color trailColor = Color.Lerp(Color.GreenYellow, Color.DeepPink, (float)Math.Pow(completion, 2)) * opacity;
                trailColor.A = 0; // Additive blend
                
                // Draw glow segment
                Vector2 diff = end - start;
                float rotation = diff.ToRotation();
                float length = diff.Length();
                
                spriteBatch.Draw(
                    glowTex,
                    start,
                    null,
                    trailColor * 0.5f,
                    rotation,
                    new Vector2(0, glowTex.Height / 2f),
                    new Vector2(length / glowTex.Width, width / glowTex.Height * 0.5f),
                    SpriteEffects.None,
                    0f
                );
            }
        }

        /// <summary>
        /// Draw the blade sprite with stretch/squish deformation.
        /// Uses the Coda of Annihilation texture with Exoblade-style rendering.
        /// </summary>
        private void DrawBlade()
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            
            // Load the blade texture (Coda of Annihilation)
            Texture2D texture;
            try
            {
                texture = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
            }
            catch
            {
                // Fallback to vanilla Terra Blade
                texture = Terraria.GameContent.TextureAssets.Item[ItemID.TerraBlade].Value;
            }
            
            SpriteEffects direction = Direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            
            // Calculate rotation for the swing - this is the key to the animation
            float bladeRotation = SwordRotation + MathHelper.PiOver4 + (Direction == -1 ? MathHelper.Pi : 0f);
            
            // Apply swing shader for proper deformation (if available)
            bool shaderApplied = SwingShaderSystem.ApplySwingShader(spriteBatch, SwingAngleShift, 0.05f, Color.White);
            
            // Calculate the scale with squish applied
            Vector2 drawScale = SquishVector * 2.5f * Projectile.scale;
            
            // Draw bloom glow behind blade first (additive)
            if (!shaderApplied)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                
                // Rainbow glow based on swing progress
                Color glowColor = SwingShaderSystem.GetExobladeColor(Progression);
                glowColor.A = 0;
                float glowIntensity = (float)Math.Sin(Progression * MathHelper.Pi) * 0.6f;
                
                spriteBatch.Draw(
                    texture,
                    Owner.MountedCenter - Main.screenPosition,
                    null,
                    glowColor * glowIntensity,
                    bladeRotation,
                    texture.Size() / 2f,
                    drawScale * 1.15f,
                    direction,
                    0f
                );
                
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            
            // Draw the main blade
            spriteBatch.Draw(
                texture,
                Owner.MountedCenter - Main.screenPosition,
                null,
                Color.White,
                bladeRotation,
                texture.Size() / 2f,
                drawScale,
                direction,
                0f
            );
            
            if (shaderApplied)
            {
                SwingShaderSystem.RestoreSpriteBatch(spriteBatch);
            }
        }

        /// <summary>
        /// Try to apply the swing sprite shader for proper blade deformation.
        /// This is now handled by SwingShaderSystem but kept for compatibility.
        /// </summary>
        private bool TryApplySwingShader(float rotation)
        {
            return SwingShaderSystem.ApplySwingShader(Main.spriteBatch, rotation, 0.05f, Color.White);
        }

        /// <summary>
        /// Draw a lens flare at the blade tip for extra visual flair.
        /// Uses Exoblade-style rainbow cycling.
        /// </summary>
        private void DrawLensFlare()
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            
            // Use GlintStar texture from main mod folder for lens flare
            Texture2D shineTex;
            try
            {
                shineTex = ModContent.Request<Texture2D>("MagnumOpus/GlintStar", AssetRequestMode.ImmediateLoad).Value;
            }
            catch
            {
                // Fallback to vanilla sparkle
                shineTex = Terraria.GameContent.TextureAssets.Extra[98].Value;
            }
            
            Vector2 shineScale = new Vector2(0.8f, 2.5f);
            
            // Opacity based on swing progress (peak at middle of swing)
            float lensFlareOpacity = 0f;
            if (Progression >= 0.25f)
            {
                lensFlareOpacity = (float)Math.Sin(MathHelper.Pi * (Progression - 0.25f) / 0.6f) * 0.85f;
                lensFlareOpacity = Math.Clamp(lensFlareOpacity, 0f, 1f);
            }
            
            if (lensFlareOpacity <= 0f)
                return;
            
            // Switch to additive blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Rainbow color using Exoblade palette
            Color lensFlareColor = SwingShaderSystem.GetExobladeColor(Progression);
            lensFlareColor.A = 0; // Additive blend
            
            Vector2 bladePos = Owner.MountedCenter + DirectionAtProgressScuffed(Progression) * Projectile.scale * BladeLength;
            
            // Main flare
            spriteBatch.Draw(
                shineTex,
                bladePos - Main.screenPosition,
                null,
                lensFlareColor * lensFlareOpacity,
                MathHelper.PiOver2,
                shineTex.Size() / 2f,
                shineScale * Projectile.scale,
                SpriteEffects.None,
                0f
            );
            
            // Cross flare (perpendicular)
            spriteBatch.Draw(
                shineTex,
                bladePos - Main.screenPosition,
                null,
                lensFlareColor * lensFlareOpacity * 0.5f,
                0f,
                shineTex.Size() / 2f,
                shineScale * Projectile.scale * 0.6f,
                SpriteEffects.None,
                0f
            );
            
            // Restore normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        #endregion

        #region Cleanup
        public override void OnKill(int timeLeft)
        {
            Owner.fullRotation = 0f;
        }
        #endregion
    }
}
