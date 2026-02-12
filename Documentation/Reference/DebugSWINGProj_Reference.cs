// ============================================================================
// REFERENCE FILE: DebugSWINGProj.cs — The Gold-Standard Exoblade Swing Projectile
// ============================================================================
// This file is preserved as a reference before the DebugWeapons folder was deleted.
// It demonstrates:
//   1. Piecewise animation (CurveSegment) for smooth swing arcs
//   2. Blade squish/stretch deformation
//   3. Exoblade-style rainbow trail rendering
//   4. Lens flare at blade tip
//   5. Held-projectile pattern (item spawns proj, proj IS the swing)
//
// Original location: Content/DebugWeapons/DebugSWING/DebugSWINGProj.cs
// ============================================================================

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

namespace MagnumOpus.Documentation.Reference
{
    /// <summary>
    /// REFERENCE ONLY — DO NOT USE IN PRODUCTION.
    /// This is a preserved copy of DebugSWINGProj for study purposes.
    /// 
    /// ANIMATION BREAKDOWN:
    /// - SlowStart: Wind-up phase (0-27% of swing)
    /// - SwingFast: Main swing acceleration (27-85% of swing)
    /// - EndSwing: Deceleration/follow-through (85-100% of swing)
    /// 
    /// COLOR PALETTE (Exoblade Rainbow):
    /// Cyan → Lime → GreenYellow → Goldenrod → Orange
    /// </summary>
    // Commented out to prevent compilation — this is reference only
    /*
    public class DebugSWINGProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/CodaOfAnnihilation";
        private const string FallbackTexture = "Terraria/Images/Item_" + nameof(ItemID.TerraBlade);
        private const string NoiseTexturePath = "MagnumOpus/Assets/VFX/Noise/VoronoiNoise";

        #region Constants
        private const float BladeLength = 180f;
        private const float MaxSwingAngle = MathHelper.PiOver2 * 1.8f; // 162 degrees total swing arc
        #endregion

        #region Properties
        private Player Owner => Main.player[Projectile.owner];

        public int SwingTime
        {
            get => (int)Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }

        public float SquishFactor
        {
            get => Projectile.localAI[1];
            set => Projectile.localAI[1] = value;
        }

        private float Timer => SwingTime - Projectile.timeLeft;
        private float Progression => SwingTime > 0 ? Timer / SwingTime : 0f;

        public bool InPostSwingStasis
        {
            get => Projectile.ai[1] > 0;
            set => Projectile.ai[1] = value ? 1 : 0;
        }

        private int Direction => Math.Sign(Projectile.velocity.X) <= 0 ? -1 : 1;
        private float BaseRotation => Projectile.velocity.ToRotation();
        private Vector2 SquishVector => new Vector2(1f + (1 - SquishFactor) * 0.6f, SquishFactor);
        #endregion

        #region Piecewise Animation Curves
        private readonly CurveSegment SlowStart = new CurveSegment(EasingType.PolyOut, 0f, -1f, 0.3f, 2);
        private readonly CurveSegment SwingFast = new CurveSegment(EasingType.PolyIn, 0.27f, -0.7f, 1.6f, 4);
        private readonly CurveSegment EndSwing = new CurveSegment(EasingType.PolyOut, 0.85f, 0.9f, 0.1f, 2);

        private float SwingAngleShiftAtProgress(float progress)
        {
            return MaxSwingAngle * PiecewiseAnimation(progress, new CurveSegment[] { SlowStart, SwingFast, EndSwing });
        }

        private float SwordRotationAtProgress(float progress)
        {
            return BaseRotation + SwingAngleShiftAtProgress(progress) * Direction;
        }

        private float SquishAtProgress(float progress)
        {
            float angleShift = Math.Abs(SwingAngleShiftAtProgress(progress));
            return MathHelper.Lerp(SquishVector.X, SquishVector.Y, (float)Math.Abs(Math.Sin(angleShift)));
        }

        private Vector2 DirectionAtProgress(float progress)
        {
            return SwordRotationAtProgress(progress).ToRotationVector2() * SquishAtProgress(progress);
        }

        private float SwingAngleShift => SwingAngleShiftAtProgress(Progression);
        private float SwordRotation => SwordRotationAtProgress(Progression);
        private float CurrentSquish => SquishAtProgress(Progression);
        private Vector2 SwordDirection => DirectionAtProgress(Progression);
        #endregion

        #region Trail Points Generation
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

        private float RealProgressionAtTrailCompletion(float completion)
        {
            return MathHelper.Lerp(Progression, TrailEndProgression, completion);
        }

        private Vector2 DirectionAtProgressScuffed(float progress)
        {
            float angleShift = SwingAngleShiftAtProgress(progress);
            Vector2 anglePoint = angleShift.ToRotationVector2();
            anglePoint.X *= SquishVector.X;
            anglePoint.Y *= SquishVector.Y;
            angleShift = anglePoint.ToRotation();
            return (BaseRotation + angleShift * Direction).ToRotationVector2() * SquishAtProgress(progress);
        }

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

        #region Setup
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
        private void InitializationEffects(bool firstSwing)
        {
            Projectile.velocity = Owner.MountedCenter.DirectionTo(Main.MouseWorld);
            SquishFactor = Main.rand.NextFloat(0.67f, 1f);
            if (firstSwing)
                Projectile.scale = 0.02f;
            else
                Projectile.scale = 1f;
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
            if (Projectile.timeLeft >= 9999 || (Projectile.timeLeft == 1 && Owner.channel))
                InitializationEffects(Projectile.timeLeft >= 9999);
            DoBehavior_Swinging();
            Projectile.Center = Owner.RotatedRelativePoint(Owner.MountedCenter, true);
            Owner.heldProj = Projectile.whoAmI;
            Owner.SetDummyItemTime(2);
            Owner.ChangeDir(Direction);
            float armRotation = SwordRotation - MathHelper.PiOver2;
            Owner.SetCompositeArmFront(Math.Abs(armRotation) > 0.01f, Player.CompositeArmStretchAmount.Full, armRotation);
        }

        private void DoBehavior_Swinging()
        {
            Color lightColor = SwingShaderSystem.GetExobladeColor(Progression);
            float lightIntensity = (float)Math.Sin(Progression * MathHelper.Pi) * 1.6f;
            Lighting.AddLight(Owner.MountedCenter + SwordDirection * 100, lightColor.ToVector3() * lightIntensity);

            float idealSize = 1f;
            if (Projectile.scale < idealSize)
                Projectile.scale = MathHelper.Lerp(Projectile.scale, idealSize, 0.08f);
            if (!Owner.channel && Progression > 0.7f)
                Projectile.scale = (0.5f + 0.5f * (float)Math.Pow(1 - (Progression - 0.7f) / 0.3f, 0.5)) * idealSize;
        }
        #endregion

        #region Drawing
        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.Opacity <= 0f || InPostSwingStasis)
                return false;
            DrawSlashTrail();
            DrawBlade();
            DrawLensFlare();
            return false;
        }

        private void DrawSlashTrail()
        {
            if (Progression < 0.35f)
                return;
            List<Vector2> slashPoints = GenerateSlashPoints();
            DrawExobladeStyleTrail(slashPoints);
        }

        private void DrawExobladeStyleTrail(List<Vector2> points)
        {
            if (points.Count < 2) return;
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D glowTex = Terraria.GameContent.TextureAssets.Extra[98].Value;
            SwingShaderSystem.BeginAdditive(spriteBatch);

            for (int i = 0; i < points.Count - 1; i++)
            {
                float completion = (float)i / points.Count;
                float opacity = Utils.GetLerpValue(0.95f, 0.4f, completion, true) * Projectile.Opacity;
                float realProgress = RealProgressionAtTrailCompletion(completion);
                float width = SquishAtProgress(realProgress) * Projectile.scale * 55f;
                Vector2 start = Projectile.Center + points[i] - Main.screenPosition;
                Vector2 end = Projectile.Center + points[i + 1] - Main.screenPosition;

                float colorProgress = (completion + Progression * 0.5f) % 1f;
                Color trailColor = SwingShaderSystem.GetExobladeColor(colorProgress);
                trailColor.A = 0;

                Vector2 diff = end - start;
                float rotation = diff.ToRotation();
                float length = diff.Length();
                if (length < 1f) continue;

                spriteBatch.Draw(glowTex, start, null, trailColor * opacity * 0.7f, rotation,
                    new Vector2(0, glowTex.Height / 2f), new Vector2(length / glowTex.Width, width / glowTex.Height * 0.6f),
                    SpriteEffects.None, 0f);

                Color coreColor = Color.Lerp(trailColor, Color.White, 0.4f);
                coreColor.A = 0;
                spriteBatch.Draw(glowTex, start, null, coreColor * opacity * 0.9f, rotation,
                    new Vector2(0, glowTex.Height / 2f), new Vector2(length / glowTex.Width, width / glowTex.Height * 0.3f),
                    SpriteEffects.None, 0f);
            }

            SwingShaderSystem.RestoreSpriteBatch(spriteBatch);
        }

        private void DrawBlade()
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture;
            try { texture = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value; }
            catch { texture = Terraria.GameContent.TextureAssets.Item[ItemID.TerraBlade].Value; }

            SpriteEffects direction = Direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            float bladeRotation = SwordRotation + MathHelper.PiOver4 + (Direction == -1 ? MathHelper.Pi : 0f);
            bool shaderApplied = SwingShaderSystem.ApplySwingShader(spriteBatch, SwingAngleShift, 0.05f, Color.White);
            Vector2 drawScale = SquishVector * 2.5f * Projectile.scale;

            if (!shaderApplied)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                Color glowColor = SwingShaderSystem.GetExobladeColor(Progression);
                glowColor.A = 0;
                float glowIntensity = (float)Math.Sin(Progression * MathHelper.Pi) * 0.6f;
                spriteBatch.Draw(texture, Owner.MountedCenter - Main.screenPosition, null,
                    glowColor * glowIntensity, bladeRotation, texture.Size() / 2f, drawScale * 1.15f, direction, 0f);

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            spriteBatch.Draw(texture, Owner.MountedCenter - Main.screenPosition, null,
                Color.White, bladeRotation, texture.Size() / 2f, drawScale, direction, 0f);

            if (shaderApplied) SwingShaderSystem.RestoreSpriteBatch(spriteBatch);
        }

        private void DrawLensFlare()
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D shineTex;
            try { shineTex = ModContent.Request<Texture2D>("MagnumOpus/GlintStar", AssetRequestMode.ImmediateLoad).Value; }
            catch { shineTex = Terraria.GameContent.TextureAssets.Extra[98].Value; }

            Vector2 shineScale = new Vector2(0.8f, 2.5f);
            float lensFlareOpacity = 0f;
            if (Progression >= 0.25f)
            {
                lensFlareOpacity = (float)Math.Sin(MathHelper.Pi * (Progression - 0.25f) / 0.6f) * 0.85f;
                lensFlareOpacity = Math.Clamp(lensFlareOpacity, 0f, 1f);
            }
            if (lensFlareOpacity <= 0f) return;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            Color lensFlareColor = SwingShaderSystem.GetExobladeColor(Progression);
            lensFlareColor.A = 0;
            Vector2 bladePos = Owner.MountedCenter + DirectionAtProgressScuffed(Progression) * Projectile.scale * BladeLength;

            spriteBatch.Draw(shineTex, bladePos - Main.screenPosition, null,
                lensFlareColor * lensFlareOpacity, MathHelper.PiOver2, shineTex.Size() / 2f,
                shineScale * Projectile.scale, SpriteEffects.None, 0f);

            spriteBatch.Draw(shineTex, bladePos - Main.screenPosition, null,
                lensFlareColor * lensFlareOpacity * 0.5f, 0f, shineTex.Size() / 2f,
                shineScale * Projectile.scale * 0.6f, SpriteEffects.None, 0f);

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
    */
}
