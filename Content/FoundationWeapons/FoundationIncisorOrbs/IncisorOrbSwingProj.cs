using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.FoundationIncisorOrbs
{
    /// <summary>
    /// IncisorOrbSwingProj — 1-to-1 skeleton of IncisorSwingProj's swing behavior,
    /// focused on the orb-firing mechanic.
    ///
    /// This is the channelled swing projectile that:
    /// 1. Animates a sword swing using CurveSegment piecewise animation
    ///    (Grave → Allegro → Diminuendo, identical to IncisorSwingProj)
    /// 2. Fires IncisorOrbProj homing orbs during 60–100% of the swing arc
    ///    (identical timing to IncisorSwingProj's LunarBeamProj spawning)
    /// 3. Spawns constellation spark particles along the blade
    /// 4. Draws the blade sprite with rotation
    ///
    /// The swing arc, timing, and orb-firing logic are 1-to-1 with IncisorSwingProj.
    /// Dash behavior, empowered swing, and slash shader trail are omitted
    /// to focus purely on the orb-on-swing skeleton.
    /// </summary>
    public class IncisorOrbSwingProj : ModProjectile
    {
        public Player Owner => Main.player[Projectile.owner];
        const float BladeLength = 150;

        // =====================================================================
        // TIMING — identical to IncisorSwingProj
        // =====================================================================

        public int GetSwingTime => 72;

        public float Timer => SwingTime - Projectile.timeLeft;
        public float Progression => Timer / SwingTime;

        // =====================================================================
        // STATE
        // =====================================================================

        public ref float SwingTime => ref Projectile.localAI[0];
        public ref float SquishFactor => ref Projectile.localAI[1];

        // =====================================================================
        // ANGLES AND ANIMATION CURVES — identical to IncisorSwingProj
        // =====================================================================

        public int Direction => Math.Sign(Projectile.velocity.X) <= 0 ? -1 : 1;
        public float BaseRotation => Projectile.velocity.ToRotation();
        public Vector2 SquishVector => new(1f + (1 - SquishFactor) * 0.6f, SquishFactor);

        public static float MaxSwingAngle = MathHelper.PiOver2 * 1.75f;

        // Musical movement: Grave (slow pull) → Allegro (fast swing) → Diminuendo (settle)
        // These are the exact same CurveSegments from IncisorSwingProj
        public IOFUtils.CurveSegment GravePull = new(IOFUtils.PolyOutEasing, 0f, -1f, 0.28f, 2);
        public IOFUtils.CurveSegment AllegroSwing = new(IOFUtils.PolyInEasing, 0.25f, -0.72f, 1.62f, 4);
        public IOFUtils.CurveSegment DiminuendoSettle = new(IOFUtils.PolyOutEasing, 0.83f, 0.9f, 0.1f, 2);

        public float SwingAngleShiftAtProgress(float progress)
            => MaxSwingAngle * IOFUtils.PiecewiseAnimation(progress, GravePull, AllegroSwing, DiminuendoSettle);

        public float SwordRotationAtProgress(float progress)
            => BaseRotation + SwingAngleShiftAtProgress(progress) * Direction;

        public float SquishAtProgress(float progress)
            => MathHelper.Lerp(SquishVector.X, SquishVector.Y,
                (float)Math.Abs(Math.Sin(SwingAngleShiftAtProgress(progress))));

        public Vector2 DirectionAtProgress(float progress)
            => SwordRotationAtProgress(progress).ToRotationVector2() * SquishAtProgress(progress);

        public float SwingAngleShift => SwingAngleShiftAtProgress(Progression);
        public float SwordRotation => SwordRotationAtProgress(Progression);
        public float CurrentSquish => SquishAtProgress(Progression);
        public Vector2 SwordDirection => DirectionAtProgress(Progression);

        // =====================================================================
        // DUST DENSITY — identical to IncisorSwingProj
        // =====================================================================

        public float DustDensity
        {
            get
            {
                if (Progression > 0.85f) return 0;
                if (Progression < 0.4f) return (float)Math.Pow(Progression / 0.3f, 2) * 0.2f;
                if (Progression < 0.5f) return 0.2f + 0.7f * (Progression - 0.4f) / 0.1f;
                return 0.9f;
            }
        }

        // =====================================================================
        // SETUP
        // =====================================================================

        public override string Texture => "Terraria/Images/Item_" + ItemID.Katana;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 120;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 80;
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
            SwingTime = reader.ReadSingle();
            SquishFactor = reader.ReadSingle();
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            Vector2 start = Projectile.Center;
            Vector2 end = start + SwordDirection * (BladeLength + 40) * Projectile.scale;
            float width = Projectile.scale * 26f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, width, ref _);
        }

        // =====================================================================
        // INITIALIZATION — identical to IncisorSwingProj (swing mode only)
        // =====================================================================

        public void InitializationEffects(bool startInit)
        {
            Projectile.velocity = Owner.MountedCenter.DirectionTo(Main.MouseWorld);
            SquishFactor = Main.rand.NextFloat(0.67f, 1f);

            if (startInit)
                Projectile.scale = 0.02f;
            else
                Projectile.scale = 1f;

            SwingTime = GetSwingTime;
            Projectile.timeLeft = (int)SwingTime;
            Projectile.netUpdate = true;
        }

        // =====================================================================
        // MAIN AI — identical structure to IncisorSwingProj
        // =====================================================================

        public override void AI()
        {
            if (Projectile.timeLeft == 0) return;

            if (Projectile.timeLeft >= 9999 || (Projectile.timeLeft == 1 && Owner.channel))
                InitializationEffects(Projectile.timeLeft >= 9999);

            DoBehavior_Swinging();

            // Glue to owner
            Projectile.Center = Owner.RotatedRelativePoint(Owner.MountedCenter, true);
            Owner.heldProj = Projectile.whoAmI;
            Owner.SetDummyItemTime(2);
            Owner.ChangeDir(Direction);

            // Arm rotation
            float armRot = SwordRotation - MathHelper.PiOver2;
            Owner.SetCompositeArmFront(Math.Abs(armRot) > 0.01f, Player.CompositeArmStretchAmount.Full, armRot);
        }

        // =====================================================================
        // SWINGING BEHAVIOR — THE CORE ORB-ON-SWING MECHANIC
        // Identical logic to IncisorSwingProj.DoBehavior_Swinging()
        // =====================================================================

        public void DoBehavior_Swinging()
        {
            // Sound at 20% of swing — identical to IncisorSwingProj
            if (Projectile.timeLeft == (int)(SwingTime / 5))
                SoundEngine.PlaySound(SoundID.Item71 with { PitchVariance = 0.3f, Volume = 0.7f }, Projectile.Center);

            // Glow from blade — identical to IncisorSwingProj
            Vector3 lightColor = new(0.45f, 0.35f, 0.75f);
            Lighting.AddLight(Owner.MountedCenter + SwordDirection * 90, lightColor * 1.4f
                * (float)Math.Sin(Progression * MathHelper.Pi));

            // Scale animation — identical to IncisorSwingProj
            if (Projectile.scale < 1f)
                Projectile.scale = MathHelper.Lerp(Projectile.scale, 1f, 0.08f);
            if (!Owner.channel && Progression > 0.7f)
                Projectile.scale = (0.5f + 0.5f * (float)Math.Pow(1 - (Progression - 0.7f) / 0.3f, 0.5));

            // Constellation sparks shed from blade edge during swing — identical to IncisorSwingProj
            if (Main.rand.NextFloat() < DustDensity * 0.5f)
            {
                float bladeT = (float)Math.Pow(Main.rand.NextFloat(0.4f, 1f), 0.5f);
                Vector2 sparkPos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * bladeT;
                Vector2 sparkVel = SwordDirection.RotatedBy(-MathHelper.PiOver2 * Direction) * Main.rand.NextFloat(2f, 5f);
                Color sparkColor = IOFUtils.MulticolorLerp(Main.rand.NextFloat(), IOFUtils.FoundationPalette);

                // Vanilla dust as a visual stand-in for ConstellationSparkParticle
                Dust spark = Dust.NewDustPerfect(sparkPos, DustID.PurpleTorch, sparkVel * 0.5f,
                    newColor: sparkColor, Scale: Main.rand.NextFloat(0.5f, 1.0f));
                spark.noGravity = true;
                spark.fadeIn = 0.3f;
            }

            // =====================================================================
            // FIRE HOMING ORB PROJECTILES DURING 60–100% OF SWING
            // This is the EXACT mechanic being extracted.
            // Identical timing logic to IncisorSwingProj's LunarBeamProj spawning.
            // =====================================================================
            int beamStart = (int)(SwingTime * 0.6f);
            int beamPeriod = (int)(SwingTime * 0.4f);
            int beamEnd = beamStart + beamPeriod;
            int beamsPerSwing = FoundationIncisorOrbs.BeamsPerSwing;
            beamPeriod /= Math.Max(beamsPerSwing - 1, 1);

            if (Main.myPlayer == Projectile.owner && Timer >= beamStart && Timer < beamEnd
                && (Timer - beamStart) % beamPeriod == 0)
            {
                int dmg = (int)(Projectile.damage * FoundationIncisorOrbs.OrbDamagePenalty);
                Vector2 boltVel = Projectile.velocity.RotatedByRandom(MathHelper.PiOver4 * 0.3);
                boltVel *= Owner.HeldItem.shootSpeed;
                Projectile.NewProjectile(Projectile.GetSource_FromAI(),
                    Projectile.Center + boltVel * 5f, boltVel,
                    ModContent.ProjectileType<IncisorOrbProj>(),
                    dmg, Projectile.knockBack / 3f, Projectile.owner);
            }
        }

        // =====================================================================
        // DRAWING — Simplified blade draw (no slash shader, no pierce trail)
        // Focuses on demonstrating the swing rotation for the orb-firing mechanic.
        // =====================================================================

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            if (Projectile.Opacity <= 0f) return false;
            DrawBlade(lightColor);
            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }

        public void DrawBlade(Color lightColor)
        {
            var texture = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
            SpriteEffects dir = Direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            float rotation = SwordRotation + MathHelper.PiOver4;
            if (Direction == -1)
                rotation += MathHelper.PiOver2;

            Vector2 origin = texture.Size() / 2f;
            Vector2 bladeDrawPos = Owner.MountedCenter + SwordDirection * 14f * Projectile.scale - Main.screenPosition;

            // Simple blade draw — real implementations would use IncisorSwingSprite shader
            Main.EntitySpriteDraw(texture, bladeDrawPos, null,
                lightColor * Projectile.Opacity, rotation, origin,
                Projectile.scale * 1.5f, dir, 0);

            // Soft additive glow on blade
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            Color glowColor = IOFUtils.MulticolorLerp(Progression, IOFUtils.FoundationPalette) with { A = 0 };
            Main.EntitySpriteDraw(texture, bladeDrawPos, null,
                glowColor * 0.3f * Projectile.Opacity, rotation, origin,
                Projectile.scale * 1.6f, dir, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
