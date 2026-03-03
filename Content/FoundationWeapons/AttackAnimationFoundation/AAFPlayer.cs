using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.AttackAnimationFoundation
{
    /// <summary>
    /// AAFPlayer — ModPlayer that controls camera, screen shake, progressive blur/brightness,
    /// and the black-and-white impact frame for the AttackAnimationFoundation weapon.
    ///
    /// When an attack animation starts:
    /// 1. Camera smoothly pans to the cursor target position
    /// 2. Each slash increases blur intensity and brightness
    /// 3. The final slash triggers a B&amp;W impact frame + heavy screen shake
    /// 4. Camera smoothly returns to the player
    ///
    /// All rendering overlays are drawn via a ModSystem to ensure they happen
    /// after the game world draws but before UI.
    /// </summary>
    public sealed class AAFPlayer : ModPlayer
    {
        // ---- CAMERA STATE ----
        /// <summary>True while the attack animation is controlling the camera.</summary>
        public bool CameraActive;

        /// <summary>World position the camera should be centered on.</summary>
        public Vector2 CameraTarget;

        /// <summary>0-1 interpolation of camera toward target. 1 = fully at target.</summary>
        public float CameraLerp;

        /// <summary>True when camera is returning to the player after animation ends.</summary>
        public bool CameraReturning;

        // ---- SCREEN EFFECTS ----
        /// <summary>0-1 progressive blur intensity. Increases with each slash.</summary>
        public float BlurIntensity;

        /// <summary>0-1 progressive brightness overlay. Increases with each slash.</summary>
        public float BrightnessIntensity;

        /// <summary>Screen shake intensity in pixels.</summary>
        public float ScreenShakeIntensity;

        /// <summary>Frames remaining for the black-and-white impact frame.</summary>
        public int ImpactFrameTimer;

        /// <summary>Maximum impact frame duration for fade calculation.</summary>
        private const int ImpactFrameMaxDuration = 12;

        // ---- SLASH TRACKING ----
        /// <summary>Number of slashes completed so far in the current animation.</summary>
        public int SlashCount;

        /// <summary>World position of the struck enemy (if any) for slash VFX.</summary>
        public Vector2 HitEnemyPosition;

        /// <summary>Whether an enemy has been hit during this animation.</summary>
        public bool HasHitEnemy;

        /// <summary>NPC whoAmI of the hit target, -1 if none.</summary>
        public int HitEnemyIndex = -1;

        // ---- NOISE ZONE ----
        /// <summary>Current radius of the circular noise zone building on the enemy.</summary>
        public float NoiseZoneRadius;

        /// <summary>Current intensity of the noise zone (brightness).</summary>
        public float NoiseZoneIntensity;

        public override void ResetEffects()
        {
            // Decay screen shake
            if (ScreenShakeIntensity > 0f)
                ScreenShakeIntensity *= 0.85f;

            // Decay blur and brightness when animation is not active
            if (!CameraActive)
            {
                BlurIntensity *= 0.9f;
                BrightnessIntensity *= 0.9f;
                NoiseZoneRadius *= 0.92f;
                NoiseZoneIntensity *= 0.9f;
            }

            // Decay impact frame
            if (ImpactFrameTimer > 0)
                ImpactFrameTimer--;
        }

        public override void ModifyScreenPosition()
        {
            // ---- CAMERA CONTROL ----
            if (CameraActive)
            {
                // Move camera toward target (cursor position in world)
                if (!CameraReturning)
                {
                    CameraLerp = MathHelper.Lerp(CameraLerp, 1f, 0.12f);
                }
                else
                {
                    // Returning: lerp back to 0 (player-centered)
                    CameraLerp = MathHelper.Lerp(CameraLerp, 0f, 0.08f);
                    if (CameraLerp < 0.02f)
                    {
                        CameraLerp = 0f;
                        CameraActive = false;
                        CameraReturning = false;
                    }
                }

                if (CameraLerp > 0.01f)
                {
                    // Default camera center = player center
                    Vector2 screenSize = new Vector2(Main.screenWidth, Main.screenHeight);
                    Vector2 defaultCenter = Player.Center - screenSize / 2f;
                    Vector2 targetCenter = CameraTarget - screenSize / 2f;

                    Vector2 desiredPos = Vector2.Lerp(defaultCenter, targetCenter, CameraLerp);
                    Main.screenPosition = desiredPos;
                }
            }

            // ---- SCREEN SHAKE ----
            if (ScreenShakeIntensity > 0.5f)
            {
                Main.screenPosition += Main.rand.NextVector2Circular(
                    ScreenShakeIntensity, ScreenShakeIntensity);
            }
        }

        /// <summary>
        /// Starts the attack animation by setting the camera target.
        /// </summary>
        public void BeginAnimation(Vector2 cursorWorldPos)
        {
            CameraActive = true;
            CameraReturning = false;
            CameraTarget = cursorWorldPos;
            CameraLerp = 0f;
            SlashCount = 0;
            BlurIntensity = 0f;
            BrightnessIntensity = 0f;
            HasHitEnemy = false;
            HitEnemyIndex = -1;
            NoiseZoneRadius = 0f;
            NoiseZoneIntensity = 0f;
        }

        /// <summary>
        /// Called when a slash connects. Increases blur/brightness and builds the noise zone.
        /// </summary>
        public void RegisterSlashHit(Vector2 enemyPos, int npcIndex, bool isFinal)
        {
            SlashCount++;
            HasHitEnemy = true;
            HitEnemyPosition = enemyPos;
            HitEnemyIndex = npcIndex;

            // Progressive blur & brightness
            float step = 1f / 5f;
            BlurIntensity = MathHelper.Clamp(BlurIntensity + step, 0f, 0.8f);
            BrightnessIntensity = MathHelper.Clamp(BrightnessIntensity + step * 0.7f, 0f, 0.6f);

            // Build noise zone
            NoiseZoneRadius = MathHelper.Clamp(NoiseZoneRadius + 15f, 0f, 80f);
            NoiseZoneIntensity = MathHelper.Clamp(NoiseZoneIntensity + 0.2f, 0f, 1f);

            // Mild screen shake per slash
            ScreenShakeIntensity = MathHelper.Clamp(ScreenShakeIntensity + 3f, 0f, 8f);

            if (isFinal)
            {
                // Big impact frame + heavy shake
                ImpactFrameTimer = ImpactFrameMaxDuration;
                ScreenShakeIntensity = 20f;
                BlurIntensity = 0f; // Clear blur for the clean B&W impact
                BrightnessIntensity = 0f;
            }
        }

        /// <summary>
        /// Ends the animation and begins returning the camera.
        /// </summary>
        public void EndAnimation()
        {
            CameraReturning = true;
        }
    }

    /// <summary>
    /// Extension method for convenient access.
    /// </summary>
    public static class AAFPlayerExt
    {
        public static AAFPlayer AttackAnimation(this Player player)
            => player.GetModPlayer<AAFPlayer>();
    }

    /// <summary>
    /// ModSystem that draws screen overlay effects (blur, brightness, impact frame, noise zone)
    /// after the game world but before UI.
    /// </summary>
    public class AAFScreenEffectSystem : ModSystem
    {
        public override void PostDrawTiles()
        {
            Player player = Main.LocalPlayer;
            if (player == null || !player.active) return;

            AAFPlayer aaf = player.AttackAnimation();
            bool hasEffects = aaf.BlurIntensity > 0.01f ||
                              aaf.BrightnessIntensity > 0.01f ||
                              aaf.ImpactFrameTimer > 0 ||
                              (aaf.HasHitEnemy && aaf.NoiseZoneIntensity > 0.01f);

            if (!hasEffects) return;

            SpriteBatch sb = Main.spriteBatch;

            // Start our own spritebatch for screen overlays
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.PointClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            // ---- NOISE ZONE ON ENEMY ----
            if (aaf.HasHitEnemy && aaf.NoiseZoneIntensity > 0.01f)
            {
                DrawNoiseZone(sb, aaf);
            }

            sb.End();

            // ---- ADDITIVE OVERLAYS ----
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.PointClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            // ---- PROGRESSIVE BRIGHTNESS OVERLAY ----
            if (aaf.BrightnessIntensity > 0.01f)
            {
                DrawBrightnessOverlay(sb, aaf);
            }

            sb.End();

            // ---- B&W IMPACT FRAME (alpha blend for desaturation effect) ----
            if (aaf.ImpactFrameTimer > 0)
            {
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    SamplerState.PointClamp, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);

                DrawImpactFrame(sb, aaf);

                sb.End();
            }
        }

        private void DrawNoiseZone(SpriteBatch sb, AAFPlayer aaf)
        {
            // Update world position — track the enemy if still alive
            if (aaf.HitEnemyIndex >= 0 && aaf.HitEnemyIndex < Main.maxNPCs)
            {
                NPC target = Main.npc[aaf.HitEnemyIndex];
                if (target.active)
                    aaf.HitEnemyPosition = target.Center;
            }

            Vector2 drawPos = aaf.HitEnemyPosition - Main.screenPosition;
            float alpha = aaf.NoiseZoneIntensity;
            float radius = aaf.NoiseZoneRadius;

            // Scale the SoftCircle texture to match desired radius
            Texture2D circleTex = AAFTextures.SoftCircle.Value;
            Vector2 origin = circleTex.Size() / 2f;
            float texScale = radius / (circleTex.Width / 2f);

            // Draw noise-masked zone effect using layered circles
            // Layer 1: wide soft outer glow
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.PointClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            Texture2D softGlow = AAFTextures.SoftGlow.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;

            float pulse = 0.9f + 0.1f * MathF.Sin((float)Main.timeForVisualEffects * 0.08f);

            // Outer atmospheric glow
            sb.Draw(softGlow, drawPos, null,
                AAFTextures.ZoneColors[0] * (alpha * 0.35f * pulse),
                0f, glowOrigin, texScale * 1.5f, SpriteEffects.None, 0f);

            // Mid glow
            sb.Draw(softGlow, drawPos, null,
                AAFTextures.ZoneColors[1] * (alpha * 0.5f * pulse),
                0f, glowOrigin, texScale * 1.0f, SpriteEffects.None, 0f);

            // Core circle (noise-masked look via the hard mask)
            Texture2D hardMask = AAFTextures.HardCircleMask.Value;
            Vector2 maskOrigin = hardMask.Size() / 2f;
            float maskScale = radius / (hardMask.Width / 2f);
            sb.Draw(hardMask, drawPos, null,
                AAFTextures.ZoneColors[2] * (alpha * 0.4f),
                0f, maskOrigin, maskScale * 0.8f, SpriteEffects.None, 0f);

            // Sparkle accents inside the zone
            Texture2D pointBloom = AAFTextures.PointBloom.Value;
            Vector2 pbOrigin = pointBloom.Size() / 2f;
            float time = (float)Main.timeForVisualEffects;

            for (int i = 0; i < 4; i++)
            {
                float angle = time * 0.03f + i * MathHelper.PiOver2;
                float sparkleRadius = radius * 0.5f;
                Vector2 sparklePos = drawPos + new Vector2(
                    MathF.Cos(angle) * sparkleRadius,
                    MathF.Sin(angle) * sparkleRadius);
                float sparkleAlpha = 0.4f + 0.3f * MathF.Sin(time * 0.1f + i * 1.5f);

                sb.Draw(pointBloom, sparklePos, null,
                    AAFTextures.ZoneColors[2] * (alpha * sparkleAlpha),
                    0f, pbOrigin, 0.15f, SpriteEffects.None, 0f);
            }

            // Restore to AlphaBlend for caller
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.PointClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawBrightnessOverlay(SpriteBatch sb, AAFPlayer aaf)
        {
            // Draw a full-screen white additive overlay
            Texture2D pixel = AAFTextures.SoftGlow.Value;
            Vector2 origin = pixel.Size() / 2f;
            Vector2 screenCenter = new Vector2(Main.screenWidth, Main.screenHeight) / 2f;

            // Scale to cover whole screen
            float scaleX = Main.screenWidth / (float)pixel.Width * 2f;
            float scaleY = Main.screenHeight / (float)pixel.Height * 2f;

            sb.Draw(pixel, screenCenter, null,
                Color.White * (aaf.BrightnessIntensity * 0.3f),
                0f, origin, new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
        }

        private void DrawImpactFrame(SpriteBatch sb, AAFPlayer aaf)
        {
            float progress = 1f - (aaf.ImpactFrameTimer / 12f);
            float alpha = 1f - progress; // Fades from 1 to 0

            // Draw a full-screen white flash that fades rapidly
            // The "B&W" effect is simulated by a bright white then a desaturating dark overlay
            Texture2D glow = AAFTextures.SoftGlow.Value;
            Vector2 origin = glow.Size() / 2f;
            Vector2 screenCenter = new Vector2(Main.screenWidth, Main.screenHeight) / 2f;
            float scaleX = Main.screenWidth / (float)glow.Width * 3f;
            float scaleY = Main.screenHeight / (float)glow.Height * 3f;

            if (progress < 0.3f)
            {
                // Initial bright white flash
                float flashAlpha = (1f - progress / 0.3f);
                sb.Draw(glow, screenCenter, null,
                    Color.White * (flashAlpha * 0.9f),
                    0f, origin, new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
            }
            else
            {
                // Desaturation effect — dark overlay with slight transparency
                float desatAlpha = alpha * 0.5f;
                sb.Draw(glow, screenCenter, null,
                    new Color(10, 10, 15) * desatAlpha,
                    0f, origin, new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
            }
        }
    }
}
