using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.AttackAnimationFoundation
{
    /// <summary>
    /// AttackAnimationProjectile — The main orchestrator projectile for the cinematic
    /// attack animation. This projectile does NOT move through the world; instead it
    /// drives a timed sequence of visual events:
    ///
    /// Phase 0 (frames 0-15):   Camera pans to cursor, player fades out
    /// Phase 1 (frames 16-75):  5 rapid slashes from random angles through the center
    ///                           Each slash: player appears at edge → dashes through center → exits
    ///                           Each slash deals damage to enemies near the center
    ///                           Each hit builds blur, brightness, and a noise zone
    /// Phase 2 (frames 76-85):  Final slash from the top, B/W impact frame, heavy screen shake
    /// Phase 3 (frames 86-110): Camera returns to player, effects fade
    ///
    /// The player character is drawn by this projectile at the cinematic position
    /// rather than at their actual world position during the animation.
    /// </summary>
    public class AttackAnimationProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowCrystalExplosion;

        // ---- TIMING CONSTANTS ----
        private const int PanDuration = 15;          // Frames to pan camera
        private const int SlashCount = 5;             // Number of rapid slashes
        private const int FramesPerSlash = 12;        // Frames per slash animation
        private const int FinalSlashHold = 10;        // Extra hold for final slash impact
        private const int ReturnDuration = 25;        // Frames to return camera

        private const int SlashPhaseStart = PanDuration;
        private const int SlashPhaseEnd = SlashPhaseStart + SlashCount * FramesPerSlash;
        private const int FinalStart = SlashPhaseEnd;
        private const int FinalEnd = FinalStart + FinalSlashHold;
        private const int TotalLifetime = FinalEnd + ReturnDuration;

        // ---- SLASH GEOMETRY ----
        private const float SlashRadius = 300f;       // How far from center slashes originate
        private const float DamageRadius = 80f;       // Radius for dealing damage to nearby enemies

        // ---- STATE ----
        private int timer;
        private Vector2 cameraTarget;                 // World position of cursor (camera center)
        private float[] slashAngles;                  // Pre-computed random angles for each slash
        private bool initialized;
        private int currentSlashIndex = -1;
        private float slashProgress;                  // 0-1 progress of current slash dash
        private Vector2 slashStartPos;                // World pos: where the current slash starts
        private Vector2 slashEndPos;                  // World pos: where the current slash ends
        private bool[] slashDamageDealt;              // Whether each slash has dealt its damage
        private List<SlashTrailEntry> slashTrails;    // Motion blur trail entries

        // ---- AFTERIMAGE STRUCTS ----
        private struct SlashTrailEntry
        {
            public Vector2 Position;
            public float Rotation;
            public float Alpha;
            public float Scale;
            public int SlashIndex;
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 2000;
        }

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = false; // We handle damage manually
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = TotalLifetime + 5;
            Projectile.hide = false;
            Projectile.alpha = 255; // Invisible base sprite
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            // ---- INITIALIZATION ----
            if (!initialized)
            {
                initialized = true;
                cameraTarget = new Vector2(Projectile.ai[0], Projectile.ai[1]);

                // Pre-compute slash angles — random directions, final slash from top
                slashAngles = new float[SlashCount];
                for (int i = 0; i < SlashCount - 1; i++)
                {
                    slashAngles[i] = Main.rand.NextFloat(MathHelper.TwoPi);
                }
                // Final slash always comes from the top
                slashAngles[SlashCount - 1] = -MathHelper.PiOver2; // Points downward from above

                slashDamageDealt = new bool[SlashCount];
                slashTrails = new List<SlashTrailEntry>();

                // Start camera animation
                AAFPlayer aaf = owner.AttackAnimation();
                aaf.BeginAnimation(cameraTarget);

                // Lock the player position during animation
                Projectile.Center = owner.Center;
            }

            timer++;

            // Keep velocity zeroed — this projectile doesn't move
            Projectile.velocity = Vector2.Zero;

            // Hold the player in place and make them invisible during the animation
            if (timer < TotalLifetime - ReturnDuration)
            {
                owner.velocity = Vector2.Zero;
                owner.immuneTime = 10;
                owner.immune = true;
            }

            AAFPlayer aafState = owner.AttackAnimation();

            // ---- PHASE LOGIC ----
            if (timer <= PanDuration)
            {
                // Phase 0: Camera panning — handled by AAFPlayer
            }
            else if (timer <= SlashPhaseEnd)
            {
                // Phase 1: Rapid slashes
                int slashFrame = timer - SlashPhaseStart;
                int newSlashIndex = slashFrame / FramesPerSlash;

                if (newSlashIndex >= SlashCount)
                    newSlashIndex = SlashCount - 1;

                // Start a new slash
                if (newSlashIndex != currentSlashIndex)
                {
                    currentSlashIndex = newSlashIndex;
                    float angle = slashAngles[currentSlashIndex];

                    // Slash goes from one side through center to the other
                    slashStartPos = cameraTarget + angle.ToRotationVector2() * SlashRadius;
                    slashEndPos = cameraTarget - angle.ToRotationVector2() * SlashRadius;

                    // Play whoosh sound
                    SoundEngine.PlaySound(SoundID.Item71 with { Volume = 0.7f, Pitch = 0.2f + currentSlashIndex * 0.1f },
                        cameraTarget);
                }

                // Calculate slash progress within current slash
                int frameInSlash = slashFrame - currentSlashIndex * FramesPerSlash;
                slashProgress = MathHelper.Clamp(frameInSlash / (float)FramesPerSlash, 0f, 1f);

                // Apply ease-in-out for smooth acceleration
                float easedProgress = EaseInOutCubic(slashProgress);

                // Current player position along the slash
                Vector2 currentPos = Vector2.Lerp(slashStartPos, slashEndPos, easedProgress);

                // Teleport owner to slash position visually
                owner.Center = currentPos;
                owner.direction = (slashEndPos.X > slashStartPos.X) ? 1 : -1;

                // ---- MOTION BLUR TRAIL ----
                if (timer % 2 == 0)
                {
                    float angle = slashAngles[currentSlashIndex];
                    slashTrails.Add(new SlashTrailEntry
                    {
                        Position = currentPos,
                        Rotation = angle,
                        Alpha = 0.7f,
                        Scale = 1f,
                        SlashIndex = currentSlashIndex
                    });
                }

                // ---- DEAL DAMAGE AT MID-POINT ----
                if (slashProgress > 0.35f && slashProgress < 0.65f
                    && !slashDamageDealt[currentSlashIndex])
                {
                    slashDamageDealt[currentSlashIndex] = true;
                    DealSlashDamage(owner, currentSlashIndex);
                }

                // Spawn motion dust along the slash path
                if (Main.rand.NextBool(2))
                {
                    Vector2 dustVel = (slashEndPos - slashStartPos).SafeNormalize(Vector2.UnitX) * 3f;
                    Dust dust = Dust.NewDustPerfect(
                        currentPos + Main.rand.NextVector2Circular(10f, 10f),
                        DustID.RainbowMk2, dustVel + Main.rand.NextVector2Circular(1f, 1f),
                        newColor: AAFTextures.SlashColors[Main.rand.Next(3)],
                        Scale: Main.rand.NextFloat(0.3f, 0.6f));
                    dust.noGravity = true;
                    dust.fadeIn = 0.3f;
                }
            }
            else if (timer <= FinalEnd)
            {
                // Phase 2: Final slash hold — impact frame is active
                owner.Center = cameraTarget;
            }
            else
            {
                // Phase 3: Return camera
                if (timer == FinalEnd + 1)
                {
                    aafState.EndAnimation();
                    owner.Center = Projectile.Center; // Restore actual position
                }

                // Smoothly return player to original position
                owner.velocity = Vector2.Zero;
            }

            // ---- DECAY AFTERIMAGE TRAILS ----
            for (int i = slashTrails.Count - 1; i >= 0; i--)
            {
                var trail = slashTrails[i];
                trail.Alpha *= 0.88f;
                trail.Scale *= 0.97f;
                slashTrails[i] = trail;
                if (trail.Alpha < 0.02f)
                    slashTrails.RemoveAt(i);
            }

            // Kill when done
            if (timer >= TotalLifetime)
            {
                Projectile.Kill();
            }
        }

        /// <summary>
        /// Deals damage to enemies within range of the camera center (slash target).
        /// Spawns a visual slash effect projectile on hit.
        /// </summary>
        private void DealSlashDamage(Player owner, int slashIndex)
        {
            bool isFinal = slashIndex == SlashCount - 1;
            float angle = slashAngles[slashIndex];
            bool hitSomething = false;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal)
                    continue;

                float dist = Vector2.Distance(npc.Center, cameraTarget);
                if (dist > DamageRadius)
                    continue;

                // Deal damage
                int damage = Projectile.damage;
                if (isFinal) damage = (int)(damage * 1.5f); // Final slash bonus

                int dir = (npc.Center.X > owner.Center.X) ? 1 : -1;
                npc.StrikeNPC(npc.CalculateHitInfo(damage, dir, false, Projectile.knockBack,
                    Projectile.DamageType, true));

                hitSomething = true;

                // Register hit for screen effects
                AAFPlayer aaf = owner.AttackAnimation();
                aaf.RegisterSlashHit(npc.Center, npc.whoAmI, isFinal);

                // Spawn slash effect visual at the enemy
                SpawnSlashEffect(npc.Center, angle, slashIndex, isFinal);
            }

            if (!hitSomething)
            {
                // Still spawn slash VFX at center even if no enemies
                SpawnSlashEffect(cameraTarget, angle, slashIndex, isFinal);
            }
        }

        private void SpawnSlashEffect(Vector2 position, float angle, int slashIndex, bool isFinal)
        {
            // ai[0] = angle, ai[1] = slash index (packed with isFinal flag)
            float packed = slashIndex + (isFinal ? 100f : 0f);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), position, Vector2.Zero,
                ModContent.ProjectileType<SlashVFXProjectile>(),
                0, 0f, Projectile.owner,
                ai0: angle, ai1: packed);
        }

        // =====================================================================
        // RENDERING
        // =====================================================================

        public override bool PreDraw(ref Color lightColor)
        {
            if (timer < SlashPhaseStart || timer > SlashPhaseEnd + FinalSlashHold)
                return false;

            SpriteBatch sb = Main.spriteBatch;
            Player owner = Main.player[Projectile.owner];

            // ---- DRAW AFTERIMAGE TRAILS (velocity blur effect) ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            DrawMotionBlurTrails(sb, owner);
            DrawSlashLines(sb);

            // ---- RESTORE ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        /// <summary>
        /// Draws stretched afterimage blurs along the slash path for velocity feel.
        /// </summary>
        private void DrawMotionBlurTrails(SpriteBatch sb, Player owner)
        {
            Texture2D softGlow = AAFTextures.SoftGlow.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;

            foreach (var trail in slashTrails)
            {
                Vector2 drawPos = trail.Position - Main.screenPosition;
                float stretchX = 0.15f * trail.Scale;
                float stretchY = 0.03f * trail.Scale;

                // Stretched glow along the slash direction for velocity feel
                sb.Draw(softGlow, drawPos, null,
                    AAFTextures.SlashColors[0] * (trail.Alpha * 0.4f),
                    trail.Rotation, glowOrigin,
                    new Vector2(stretchX, stretchY), SpriteEffects.None, 0f);

                // Brighter core
                sb.Draw(softGlow, drawPos, null,
                    AAFTextures.SlashColors[2] * (trail.Alpha * 0.3f),
                    trail.Rotation, glowOrigin,
                    new Vector2(stretchX * 0.5f, stretchY * 0.4f), SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws bright dash lines for the active slash.
        /// </summary>
        private void DrawSlashLines(SpriteBatch sb)
        {
            if (currentSlashIndex < 0 || currentSlashIndex >= SlashCount)
                return;

            float angle = slashAngles[currentSlashIndex];
            float easedProgress = EaseInOutCubic(slashProgress);
            Vector2 currentPos = Vector2.Lerp(slashStartPos, slashEndPos, easedProgress);
            Vector2 drawPos = currentPos - Main.screenPosition;

            Texture2D softGlow = AAFTextures.SoftGlow.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;

            // Main velocity line
            float lineAlpha = MathF.Sin(slashProgress * MathF.PI); // Peaks at midpoint
            float lineLength = 0.3f + slashProgress * 0.2f;

            sb.Draw(softGlow, drawPos, null,
                AAFTextures.SlashColors[1] * (lineAlpha * 0.6f),
                angle, glowOrigin,
                new Vector2(lineLength, 0.015f), SpriteEffects.None, 0f);

            // Bright point at current slash position
            Texture2D pointBloom = AAFTextures.PointBloom.Value;
            Vector2 pbOrigin = pointBloom.Size() / 2f;
            sb.Draw(pointBloom, drawPos, null,
                AAFTextures.SlashColors[2] * (lineAlpha * 0.8f),
                0f, pbOrigin, 0.2f * (1f + lineAlpha * 0.3f), SpriteEffects.None, 0f);
        }

        private static float EaseInOutCubic(float t)
        {
            return t < 0.5f
                ? 4f * t * t * t
                : 1f - MathF.Pow(-2f * t + 2f, 3f) / 2f;
        }

        public override Color? GetAlpha(Color lightColor) => Color.Transparent;
    }
}
