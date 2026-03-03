using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Enums;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.FoundationWeapons.LaserFoundation;

namespace MagnumOpus.Content.FoundationWeapons.ThinLaserFoundation
{
    /// <summary>
    /// ThinLaserBeam — A thin ricocheting beam projectile.
    /// 
    /// Fired by ThinLaserFoundation on each melee swing. The beam:
    /// - Instantly calculates a ricochet path from the player toward the cursor
    /// - Bounces off solid tiles up to 3 times
    /// - Exists for ~25 frames with fade in/out
    /// - Damages NPCs along all segments (including bounced segments)
    /// - Draws each segment using its own ThinBeamShader + VertexStrip
    /// - Adds glow flares at each bounce point for visual feedback
    /// 
    /// The ricochet path is recalculated each frame from the player's current
    /// position in the original firing direction, so the beam origin follows
    /// the player naturally.
    /// </summary>
    public class ThinLaserBeam : ModProjectile
    {
        // Use vanilla Last Prism projectile sprite as placeholder (never actually drawn)
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.LastPrism;

        // ---- CONSTANTS ----
        /// <summary>Maximum distance per beam segment before a bounce.</summary>
        private const float MaxSegmentLength = 1200f;

        /// <summary>Base beam width in pixels. Thin compared to LaserFoundation's 100px.</summary>
        private const float BaseBeamWidth = 10f;

        /// <summary>Maximum number of times the beam can ricochet off surfaces.</summary>
        private const int MaxBounces = 3;

        /// <summary>Total beam lifetime in frames.</summary>
        private const int TotalLifetime = 25;

        /// <summary>Raycast step size in pixels for tile collision detection.</summary>
        private const float RayStep = 8f;

        // ---- STATE ----
        /// <summary>Array of positions defining the ricochet path. [0]=origin, [1]=first hit, etc.</summary>
        private Vector2[] pathPositions;

        /// <summary>Direction the beam was fired in, stored on first frame. Never changes.</summary>
        private Vector2 firingDirection;

        /// <summary>Whether the firing direction has been captured.</summary>
        private bool initialized;

        /// <summary>Current alpha multiplier for fade in/out (0..1).</summary>
        private float alphaMultiplier = 1f;

        /// <summary>Whether bounce dust has been spawned (only once on spawn).</summary>
        private bool bounceDustSpawned;

        // ---- CACHED SHADER ----
        /// <summary>Cached thin beam shader. Loaded once, reused every frame.</summary>
        private Effect beamShader;

        /// <summary>The current beam color theme. Reads from the weapon's static field.</summary>
        private BeamTheme CurrentTheme => (BeamTheme)ThinLaserFoundation.CurrentThemeIndex;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 2400;
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.penetrate = -1; // Infinite — hits along full beam path
            Projectile.tileCollide = false; // We handle collision manually via ricochet
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = TotalLifetime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1; // Hit each NPC only once per beam
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            // Kill if owner is gone
            if (owner.dead || !owner.active)
            {
                Projectile.Kill();
                return;
            }

            // Capture firing direction on first frame
            if (!initialized)
            {
                firingDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                initialized = true;
            }

            // Anchor projectile to player (for collision system reference)
            Projectile.Center = owner.MountedCenter;

            // Recalculate ricochet path from player's current position each frame
            ComputeRicochetPath(owner.MountedCenter, firingDirection);

            // ---- FADE IN/OUT ----
            float lifeProgress = 1f - (float)Projectile.timeLeft / TotalLifetime;
            if (lifeProgress < 0.12f)
                alphaMultiplier = lifeProgress / 0.12f; // Quick fade in
            else if (lifeProgress > 0.6f)
                alphaMultiplier = (1f - lifeProgress) / 0.4f; // Gradual fade out
            else
                alphaMultiplier = 1f;

            // ---- LIGHTING ----
            if (pathPositions != null)
            {
                Color[] themeColors = LFTextures.GetDustColorsForTheme(CurrentTheme);
                Vector3 lightColor = themeColors[0].ToVector3() * 0.4f * alphaMultiplier;

                for (int i = 0; i < pathPositions.Length - 1; i++)
                {
                    Vector2 seg = pathPositions[i + 1] - pathPositions[i];
                    float segLen = seg.Length();
                    Vector2 segDir = seg.SafeNormalize(Vector2.UnitX);

                    for (float d = 0; d < segLen; d += 200f)
                    {
                        Lighting.AddLight(pathPositions[i] + segDir * d, lightColor);
                    }
                }

                // Extra light at bounce points
                for (int i = 1; i < pathPositions.Length - 1; i++)
                {
                    Lighting.AddLight(pathPositions[i], themeColors[0].ToVector3() * 0.6f * alphaMultiplier);
                }
            }

            // ---- BOUNCE DUST (once on spawn) ----
            if (!bounceDustSpawned && pathPositions != null && pathPositions.Length >= 3)
            {
                SpawnBounceDust();
                bounceDustSpawned = true;
            }
        }

        // ==============================================
        // RICOCHET PATH COMPUTATION
        // ==============================================

        /// <summary>
        /// Computes the full ricochet path from a start position in a given direction.
        /// Steps along rays at RayStep intervals, reflects off solid tiles, up to MaxBounces.
        /// </summary>
        private void ComputeRicochetPath(Vector2 startPos, Vector2 aimDir)
        {
            List<Vector2> positions = new() { startPos };

            Vector2 currentPos = startPos;
            Vector2 currentDir = aimDir;

            for (int bounce = 0; bounce <= MaxBounces; bounce++)
            {
                float hitDist = MaxSegmentLength;
                bool hitTile = false;
                Vector2 hitNormal = Vector2.Zero;

                // Raycast along current direction
                for (float d = RayStep; d < MaxSegmentLength; d += RayStep)
                {
                    Vector2 checkPoint = currentPos + currentDir * d;
                    Point tileCoords = checkPoint.ToTileCoordinates();

                    // Bounds check
                    if (tileCoords.X < 0 || tileCoords.X >= Main.maxTilesX ||
                        tileCoords.Y < 0 || tileCoords.Y >= Main.maxTilesY)
                    {
                        hitDist = d;
                        break;
                    }

                    Tile tile = Main.tile[tileCoords.X, tileCoords.Y];
                    if (tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
                    {
                        // Step back to last clear position
                        hitDist = MathHelper.Max(d - RayStep, 0f);
                        hitNormal = GetReflectionNormal(currentPos + currentDir * hitDist, currentDir);
                        hitTile = true;
                        break;
                    }
                }

                Vector2 endPos = currentPos + currentDir * hitDist;
                positions.Add(endPos);

                if (!hitTile || bounce == MaxBounces)
                    break;

                // Reflect direction off the surface and continue
                currentDir = Vector2.Reflect(currentDir, hitNormal);
                currentPos = endPos + currentDir * 4f; // Small nudge past surface to avoid re-hitting
            }

            pathPositions = positions.ToArray();
        }

        /// <summary>
        /// Determines the surface normal at a reflection point by comparing tile coordinates
        /// before and after the hit along the incoming direction.
        /// </summary>
        private Vector2 GetReflectionNormal(Vector2 preHitPos, Vector2 direction)
        {
            Point preTile = preHitPos.ToTileCoordinates();
            Vector2 nextPos = preHitPos + direction * 16f;
            Point nextTile = nextPos.ToTileCoordinates();

            int dx = nextTile.X - preTile.X;
            int dy = nextTile.Y - preTile.Y;

            // Pure horizontal movement → vertical wall
            if (dx != 0 && dy == 0)
                return new Vector2(-Math.Sign(dx), 0);

            // Pure vertical movement → horizontal surface
            if (dy != 0 && dx == 0)
                return new Vector2(0, -Math.Sign(dy));

            // Diagonal: check which neighbor tile is solid to determine the face
            bool xNeighborSolid = IsTileSolid(preTile.X + dx, preTile.Y);
            bool yNeighborSolid = IsTileSolid(preTile.X, preTile.Y + dy);

            if (xNeighborSolid && !yNeighborSolid)
                return new Vector2(-Math.Sign(dx), 0);
            if (yNeighborSolid && !xNeighborSolid)
                return new Vector2(0, -Math.Sign(dy));

            // Both solid (corner) or neither — reflect both axes
            Vector2 n = new Vector2(
                dx != 0 ? -Math.Sign(dx) : 0,
                dy != 0 ? -Math.Sign(dy) : 0);
            return n != Vector2.Zero ? Vector2.Normalize(n) : -Vector2.Normalize(direction);
        }

        /// <summary>Checks if a tile at the given coordinates is solid.</summary>
        private static bool IsTileSolid(int x, int y)
        {
            if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
                return false;
            Tile t = Main.tile[x, y];
            return t.HasTile && Main.tileSolid[t.TileType] && !Main.tileSolidTop[t.TileType];
        }

        // ==============================================
        // DUST & PARTICLES
        // ==============================================

        /// <summary>
        /// Spawns a burst of themed dust at each bounce point (interior path nodes).
        /// Called once on the first frame after the path is computed.
        /// </summary>
        private void SpawnBounceDust()
        {
            Color[] themeColors = LFTextures.GetDustColorsForTheme(CurrentTheme);

            // Dust burst at each bounce point (skip first = origin, skip last = endpoint)
            for (int i = 1; i < pathPositions.Length - 1; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(10f, 10f);
                    Color col = themeColors[Main.rand.Next(themeColors.Length)];
                    Dust dust = Dust.NewDustPerfect(pathPositions[i], DustID.RainbowMk2, vel,
                        newColor: col, Scale: Main.rand.NextFloat(0.3f, 0.6f));
                    dust.noGravity = true;
                    dust.fadeIn = 0.5f;
                }
            }

            // Small dust scatter at endpoint
            if (pathPositions.Length >= 2)
            {
                Vector2 endPos = pathPositions[pathPositions.Length - 1];
                for (int j = 0; j < 5; j++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                    Color col = themeColors[Main.rand.Next(themeColors.Length)];
                    Dust dust = Dust.NewDustPerfect(endPos, DustID.RainbowMk2, vel,
                        newColor: col, Scale: Main.rand.NextFloat(0.3f, 0.5f));
                    dust.noGravity = true;
                    dust.fadeIn = 0.4f;
                }
            }
        }

        // ==============================================
        // COLLISION
        // ==============================================

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (pathPositions == null || pathPositions.Length < 2)
                return false;

            // Check collision along every beam segment (including bounced ones)
            for (int i = 0; i < pathPositions.Length - 1; i++)
            {
                float _ = 0f;
                if (Collision.CheckAABBvLineCollision(
                    targetHitbox.TopLeft(), targetHitbox.Size(),
                    pathPositions[i], pathPositions[i + 1],
                    BaseBeamWidth * 0.4f, ref _))
                {
                    return true;
                }
            }
            return false;
        }

        public override void CutTiles()
        {
            if (pathPositions == null || pathPositions.Length < 2)
                return;

            DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
            for (int i = 0; i < pathPositions.Length - 1; i++)
            {
                Utils.PlotTileLine(pathPositions[i], pathPositions[i + 1],
                    BaseBeamWidth, DelegateMethods.CutTiles);
            }
        }

        public override bool ShouldUpdatePosition() => false;

        // ==============================================
        // RENDERING
        // ==============================================

        public override bool PreDraw(ref Color lightColor)
        {
            DrawBeamBody();
            DrawBounceFlares();
            return false;
        }

        /// <summary>
        /// Draws each beam segment using ThinBeamShader + VertexStrip.
        /// Each segment is drawn independently to avoid kinking artifacts at bounce points.
        /// Alpha decreases slightly per bounce to convey energy loss.
        /// </summary>
        private void DrawBeamBody()
        {
            if (pathPositions == null || pathPositions.Length < 2)
                return;

            // Load shader once
            if (beamShader == null)
            {
                beamShader = ModContent.Request<Effect>(
                    "MagnumOpus/Content/FoundationWeapons/ThinLaserFoundation/Shaders/ThinBeamShader",
                    AssetRequestMode.ImmediateLoad).Value;
            }

            // Configure shared shader uniforms (set once, used for all segments)
            beamShader.Parameters["WorldViewProjection"].SetValue(
                Main.GameViewMatrix.NormalizedTransformationmatrix);
            beamShader.Parameters["onTex"].SetValue(LFTextures.BeamAlphaMask.Value);
            beamShader.Parameters["gradientTex"].SetValue(LFTextures.GetGradientForTheme(CurrentTheme));
            beamShader.Parameters["baseColor"].SetValue(Color.White.ToVector3());
            beamShader.Parameters["satPower"].SetValue(0.85f);
            beamShader.Parameters["sampleTexture1"].SetValue(LFTextures.DetailThinGlowLine.Value);
            beamShader.Parameters["sampleTexture2"].SetValue(LFTextures.DetailSpark.Value);
            beamShader.Parameters["grad1Speed"].SetValue(0.9f);
            beamShader.Parameters["grad2Speed"].SetValue(1.3f);
            beamShader.Parameters["tex1Mult"].SetValue(1.5f);
            beamShader.Parameters["tex2Mult"].SetValue(1.8f);
            beamShader.Parameters["totalMult"].SetValue(1f);
            beamShader.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * -0.035f);

            // Draw each segment individually
            for (int i = 0; i < pathPositions.Length - 1; i++)
            {
                Vector2 segStart = pathPositions[i];
                Vector2 segEnd = pathPositions[i + 1];
                float segLen = (segEnd - segStart).Length();

                if (segLen < 4f) continue; // Skip degenerate segments

                float rot = (segEnd - segStart).ToRotation();
                Vector2[] positions = { segStart, segEnd };
                float[] rotations = { rot, rot };

                // Alpha decreases per bounce — beam loses energy as it ricochets
                float segAlpha = alphaMultiplier * (1f - i * 0.15f);

                Color StripColor(float progress) => Color.White * segAlpha;
                float StripWidth(float progress) => BaseBeamWidth;

                // UV repetition proportional to segment length
                float repVal = segLen / 1500f;
                beamShader.Parameters["gradientReps"].SetValue(0.75f * repVal);
                beamShader.Parameters["tex1reps"].SetValue(1.15f * repVal);
                beamShader.Parameters["tex2reps"].SetValue(1.15f * repVal);

                VertexStrip strip = new VertexStrip();
                strip.PrepareStrip(positions, rotations, StripColor, StripWidth,
                    -Main.screenPosition, includeBacksides: true);

                beamShader.CurrentTechnique.Passes["MainPS"].Apply();
                strip.DrawTrail();
            }

            // Reset pixel shader to default
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Draws additive-blended glow flares at the beam origin, each bounce point,
        /// and the endpoint. Uses theme-colored tinted glow sprites (no extra shader).
        /// </summary>
        private void DrawBounceFlares()
        {
            if (pathPositions == null || pathPositions.Length < 2)
                return;

            SpriteBatch sb = Main.spriteBatch;

            // Swap to additive blend for glow effects
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            Texture2D glowOrb = LFTextures.GlowOrb.Value;
            Texture2D starFlare = LFTextures.StarFlare.Value;
            Color[] themeColors = LFTextures.GetDustColorsForTheme(CurrentTheme);
            float pulse = 0.9f + 0.1f * MathF.Sin((float)Main.timeForVisualEffects * 0.12f);
            float time = (float)Main.timeForVisualEffects;

            // ---- BEAM ORIGIN ----
            Vector2 originDraw = pathPositions[0] - Main.screenPosition;
            Color originColor = themeColors[0] * alphaMultiplier;
            sb.Draw(glowOrb, originDraw, null, originColor * 0.5f, 0f,
                glowOrb.Size() / 2f, 0.2f * pulse, SpriteEffects.None, 0f);

            // ---- BOUNCE POINTS ----
            for (int i = 1; i < pathPositions.Length - 1; i++)
            {
                Vector2 pos = pathPositions[i] - Main.screenPosition;
                float bounceAlpha = alphaMultiplier * (1f - i * 0.12f);
                Color bounceColor = themeColors[i % themeColors.Length] * bounceAlpha;

                // Glow orb at bounce point
                sb.Draw(glowOrb, pos, null, bounceColor * 0.8f, 0f,
                    glowOrb.Size() / 2f, 0.3f * pulse, SpriteEffects.None, 0f);

                // Spinning star flare — alternates direction per bounce
                float starRot = time * 0.05f * (i % 2 == 0 ? 1f : -1f);
                sb.Draw(starFlare, pos, null, bounceColor * 0.5f, starRot,
                    starFlare.Size() / 2f, 0.15f, SpriteEffects.None, 0f);
            }

            // ---- ENDPOINT ----
            if (pathPositions.Length >= 2)
            {
                Vector2 endDraw = pathPositions[pathPositions.Length - 1] - Main.screenPosition;
                int lastBounce = pathPositions.Length - 2;
                float endAlpha = alphaMultiplier * (1f - lastBounce * 0.12f);
                Color endColor = themeColors[0] * endAlpha;

                sb.Draw(glowOrb, endDraw, null, endColor * 0.4f, 0f,
                    glowOrb.Size() / 2f, 0.25f * pulse, SpriteEffects.None, 0f);
            }

            // Restore alpha blend
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        // ==============================================
        // HIT EFFECTS
        // ==============================================

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Themed dust burst on hit
            Color[] themeColors = LFTextures.GetDustColorsForTheme(CurrentTheme);
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Color col = themeColors[Main.rand.Next(themeColors.Length)];
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.RainbowMk2, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.4f, 0.7f));
                dust.noGravity = true;
            }
        }
    }
}
