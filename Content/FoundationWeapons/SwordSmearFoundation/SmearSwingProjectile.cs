using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Content;

namespace MagnumOpus.Content.FoundationWeapons.SwordSmearFoundation
{
    /// <summary>
    /// SmearSwingProjectile 遯ｶ繝ｻThe visual swing projectile for SwordSmearFoundation.
    ///
    /// Performs a simple angular swing from one side to the other (like vanilla Swing
    /// useStyle), then renders the currently selected SlashArc texture as a large
    /// additive overlay centered on the player during the swing.
    ///
    /// Visual layers:
    ///  1. SMEAR ARC 遯ｶ繝ｻThe selected SlashArc texture rendered additively (3 layers:
    ///     outer glow, main, core) centered on the player, rotated to follow the swing.
    ///  2. BLADE SPRITE 遯ｶ繝ｻThe vanilla Katana texture drawn along the swing angle.
    ///  3. TIP GLOW 遯ｶ繝ｻAdditive bloom at the blade tip position.
    ///  4. ROOT GLOW 遯ｶ繝ｻSoft bloom at the swing origin (hand position).
    ///  5. DUST 遯ｶ繝ｻStyle-colored dust particles along the swing arc.
    ///
    /// ai[0] = SmearStyle index from the item.
    /// </summary>
    public class SmearSwingProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.Katana;

        // ---- SWING CONSTANTS ----
        private const float BladeLength = 80f;
        private const float SwingArcDeg = 150f;  // Total degrees of swing arc
        private const int SwingDuration = 24;      // Frames for full swing

        // ---- STATE ----
        private int timer;
        private float startAngle;
        private int swingDirection; // +1 or -1
        private SmearStyle CurrentStyle => (SmearStyle)(int)Projectile.ai[0];

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = SwingDuration + 2;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1; // Hit each NPC once per swing
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (owner.dead || !owner.active)
            {
                Projectile.Kill();
                return;
            }

            // ---- INITIALIZE ON FIRST FRAME ----
            if (timer == 0)
            {
                // Aim toward where the player was pointing when they swung
                float aimAngle = Projectile.velocity.ToRotation();

                // Alternate swing direction based on player facing
                swingDirection = owner.direction;
                startAngle = aimAngle - MathHelper.ToRadians(SwingArcDeg / 2f) * swingDirection;

                Projectile.rotation = startAngle;
            }

            timer++;

            // ---- SWING ANIMATION (eased) ----
            float progress = MathHelper.Clamp((float)timer / SwingDuration, 0f, 1f);
            // Smoothstep easing 遯ｶ繝ｻaccelerates then decelerates
            float eased = progress * progress * (3f - 2f * progress);

            float currentAngle = startAngle + MathHelper.ToRadians(SwingArcDeg) * eased * swingDirection;
            Projectile.rotation = currentAngle;

            // ---- ANCHOR TO PLAYER ----
            Projectile.Center = owner.MountedCenter;

            // Face swing direction
            owner.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);
            owner.heldProj = Projectile.whoAmI;
            owner.itemTime = 2;
            owner.itemAnimation = 2;
            owner.itemRotation = (float)Math.Atan2(
                MathF.Sin(currentAngle) * owner.direction,
                MathF.Cos(currentAngle) * owner.direction);

            // ---- BLADE TIP FOR COLLISION ----
            Vector2 tipPos = owner.MountedCenter + currentAngle.ToRotationVector2() * BladeLength;
            Projectile.position = tipPos - Projectile.Size / 2f;

            // ---- DUST ----
            if (timer % 2 == 0 && progress < 0.9f)
            {
                SpawnSwingDust(owner.MountedCenter, currentAngle, progress);
            }

            if (timer >= SwingDuration)
            {
                Projectile.Kill();
            }
        }

        private void SpawnSwingDust(Vector2 origin, float angle, float progress)
        {
            Color[] colors = SMFTextures.GetStyleColors(CurrentStyle);

            // Dust along the blade length
            float dustDist = BladeLength * Main.rand.NextFloat(0.4f, 1.0f);
            Vector2 pos = origin + angle.ToRotationVector2() * dustDist;
            Vector2 vel = angle.ToRotationVector2().RotatedBy(MathHelper.PiOver2 * swingDirection) * Main.rand.NextFloat(1f, 3f);
            Color col = colors[Main.rand.Next(colors.Length)];

            Dust dust = Dust.NewDustPerfect(pos, DustID.RainbowMk2, vel, newColor: col, Scale: Main.rand.NextFloat(0.3f, 0.6f));
            dust.noGravity = true;
            dust.fadeIn = 0.6f;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Player owner = Main.player[Projectile.owner];
            Vector2 origin = owner.MountedCenter;
            Vector2 tip = origin + Projectile.rotation.ToRotationVector2() * BladeLength;

            float _ = 0f;
            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(), targetHitbox.Size(),
                origin, tip, 20f, ref _);
        }

        public override bool ShouldUpdatePosition() => false;

        public override bool PreDraw(ref Color lightColor)
        {
            Player owner = Main.player[Projectile.owner];
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawOrigin = owner.MountedCenter - Main.screenPosition;

            float progress = MathHelper.Clamp((float)timer / SwingDuration, 0f, 1f);
            float currentAngle = Projectile.rotation;

            Color[] styleColors = SMFTextures.GetStyleColors(CurrentStyle);

            // ---- FADE ENVELOPE ----
            // Fade in quickly, sustain, fade out at end
            float smearAlpha;
            if (progress < 0.1f)
                smearAlpha = progress / 0.1f;
            else if (progress > 0.85f)
                smearAlpha = (1f - progress) / 0.15f;
            else
                smearAlpha = 1f;

            // ==================================================================
            //  LAYER 1: SMEAR ARC OVERLAY (shader-driven distortion + flow)
            // ==================================================================
            Texture2D smearTex = SMFTextures.GetSmearTexture(CurrentStyle);
            Vector2 smearOrigin = smearTex.Size() / 2f;

            // Scale the smear to roughly match blade length
            float maxDim = MathF.Max(smearTex.Width, smearTex.Height);
            float smearScale = (BladeLength * 2.4f) / maxDim;

            float smearRotation = currentAngle;

            Effect shader = SMFTextures.SmearDistortShader;

            if (shader != null)
            {
                // --- SHADER PATH: fluid distortion + gradient coloring ---
                sb.End();
                sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                    SamplerState.LinearWrap, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.EffectMatrix);

                float time = (float)Main.gameTimeCache.TotalGameTime.TotalSeconds;

                shader.Parameters["uTime"]?.SetValue(time);
                shader.Parameters["fadeAlpha"]?.SetValue(smearAlpha);
                shader.Parameters["distortStrength"]?.SetValue(0.06f);
                shader.Parameters["flowSpeed"]?.SetValue(0.4f);
                shader.Parameters["noiseScale"]?.SetValue(2.5f);
                shader.Parameters["noiseTex"]?.SetValue(SMFTextures.FBMNoise.Value);
                shader.Parameters["gradientTex"]?.SetValue(SMFTextures.GetGradientForStyle(CurrentStyle).Value);

                // Sub-layer A: Wide outer glow (stronger distortion)
                shader.Parameters["distortStrength"]?.SetValue(0.08f);
                shader.CurrentTechnique.Passes[0].Apply();
                sb.Draw(smearTex, drawOrigin, null,
                    Color.White * smearAlpha * 0.5f,
                    smearRotation, smearOrigin,
                    smearScale * 1.15f, SpriteEffects.None, 0f);

                // Sub-layer B: Main smear (medium distortion)
                shader.Parameters["distortStrength"]?.SetValue(0.05f);
                shader.CurrentTechnique.Passes[0].Apply();
                sb.Draw(smearTex, drawOrigin, null,
                    Color.White * smearAlpha * 0.8f,
                    smearRotation, smearOrigin,
                    smearScale, SpriteEffects.None, 0f);

                // Sub-layer C: Bright core (subtle distortion)
                shader.Parameters["distortStrength"]?.SetValue(0.025f);
                shader.CurrentTechnique.Passes[0].Apply();
                sb.Draw(smearTex, drawOrigin, null,
                    Color.White * smearAlpha * 0.65f,
                    smearRotation, smearOrigin,
                    smearScale * 0.85f, SpriteEffects.None, 0f);

                sb.End();
            }
            else
            {
                // --- FALLBACK: static colored layers (no shader) ---
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.EffectMatrix);

                sb.Draw(smearTex, drawOrigin, null,
                    styleColors[0] * smearAlpha * 0.4f,
                    smearRotation, smearOrigin,
                    smearScale * 1.15f, SpriteEffects.None, 0f);

                sb.Draw(smearTex, drawOrigin, null,
                    styleColors[1] * smearAlpha * 0.7f,
                    smearRotation, smearOrigin,
                    smearScale, SpriteEffects.None, 0f);

                sb.Draw(smearTex, drawOrigin, null,
                    styleColors[2] * smearAlpha * 0.55f,
                    smearRotation, smearOrigin,
                    smearScale * 0.85f, SpriteEffects.None, 0f);

                sb.End();
            }

            // ==================================================================
            //  LAYER 2: TIP GLOW
            // ==================================================================
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.EffectMatrix);

            Vector2 tipDrawPos = drawOrigin + currentAngle.ToRotationVector2() * BladeLength;
            Texture2D starFlare = SMFTextures.StarFlare.Value;
            Texture2D softGlow = SMFTextures.SoftGlow.Value;

            // Soft glow at tip
            sb.Draw(softGlow, tipDrawPos, null,
                styleColors[1] * smearAlpha * 0.5f, 0f,
                softGlow.Size() / 2f, 0.2f, SpriteEffects.None, 0f);

            // Star flare at tip 遯ｶ繝ｻrotates with swing
            sb.Draw(starFlare, tipDrawPos, null,
                styleColors[2] * smearAlpha * 0.4f, currentAngle * 0.5f,
                starFlare.Size() / 2f, 0.12f, SpriteEffects.None, 0f);

            // ==================================================================
            //  LAYER 3: ROOT GLOW
            // ==================================================================
            sb.Draw(softGlow, drawOrigin, null,
                styleColors[0] * smearAlpha * 0.3f, 0f,
                softGlow.Size() / 2f, 0.15f, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            // ==================================================================
            //  LAYER 4: BLADE SPRITE
            // ==================================================================
            Texture2D bladeTex = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
            Vector2 bladeOrigin = new Vector2(0, bladeTex.Height); // Bottom-left origin for rotation

            // Flip sprite based on swing direction
            SpriteEffects flip = swingDirection < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None;

            sb.Draw(bladeTex, drawOrigin, null,
                lightColor, currentAngle + MathHelper.PiOver4,
                bladeOrigin, 1f, flip, 0f);

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Color[] colors = SMFTextures.GetStyleColors(CurrentStyle);
            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Color col = colors[Main.rand.Next(colors.Length)];
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.RainbowMk2, vel, newColor: col, Scale: Main.rand.NextFloat(0.5f, 0.8f));
                dust.noGravity = true;
            }
        }
    }
}