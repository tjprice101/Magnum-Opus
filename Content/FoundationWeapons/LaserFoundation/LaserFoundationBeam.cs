using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.LaserFoundation
{
    /// <summary>
    /// LaserFoundationBeam — The convergence beam projectile.
    /// 
    /// This is a channeled held projectile that fires a continuous beam from the player
    /// toward the cursor. It demonstrates:
    /// 
    /// BEAM BODY RENDERING:
    /// - Uses Terraria's built-in VertexStrip class to construct a triangle strip mesh
    /// - 2 positions (start + end) → creates a rectangular quad
    /// - Custom .fx shader (ConvergenceBeamShader) handles all visual compositing
    /// - 4 scrolling detail textures tinted by a theme-specific gradient LUT
    /// - Theme is set via ai[1] from the item (right-click cycles themes)
    /// 
    /// ENDPOINT FLARES:
    /// - Additive-blended sprite layers at beam origin and endpoint
    /// - Multiple flare textures at different scales/rotations → stacked glow
    /// 
    /// COLLISION:
    /// - Raycast from player center along aim direction
    /// - Stops at tiles or max distance
    /// - Hurts NPCs along the beam path via CutTiles and custom collision
    /// 
    /// CHANNELING:
    /// - Stays alive while player holds mouse button (channel = true on item)
    /// - Smoothly tracks cursor position
    /// - Dies when player releases or becomes unable to channel
    /// </summary>
    public class LaserFoundationBeam : ModProjectile
    {
        // Use vanilla Last Prism projectile sprite as placeholder
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.LastPrism;

        // ---- CONSTANTS ----
        /// <summary>How far ahead of the player the beam starts (avoids overlapping the held sprite).</summary>
        private const float BeamStartOffset = 60f;

        /// <summary>Maximum beam length in pixels.</summary>
        private const float MaxBeamLength = 2400f;

        /// <summary>Base beam width in pixels.</summary>
        private const float BaseBeamWidth = 100f;

        /// <summary>How fast the beam rotates toward the cursor (radians per frame). Lower = smoother.</summary>
        private const float AimSpeed = 0.08f;

        // ---- STATE ----
        /// <summary>Current beam length (updated by collision raycasting each frame).</summary>
        private float beamLength;

        /// <summary>Rotation of the endpoint flare — increments each frame for spinning effect.</summary>
        private float flareRotation;

        /// <summary>
        /// The current beam color theme. Reads from the item's static field so it
        /// updates LIVE when the player right-clicks to cycle themes — even while
        /// the beam is actively firing.
        /// </summary>
        private BeamTheme CurrentTheme => (BeamTheme)LaserFoundation.CurrentThemeIndex;

        /// <summary>Cached beam body shader. Loaded once, reused every frame.</summary>
        private Effect beamShader;

        /// <summary>Cached flare rainbow shader. Converts white flares into spinning rainbow flares.</summary>
        private Effect flareShader;

        // ---- GRADIENT SCROLL SPEEDS ----
        // These control how fast each of the 4 gradient color samples scroll along the beam.
        // Different speeds create parallax/depth — the rainbow bands shift at different rates.
        private readonly float grad1Speed = 0.66f;
        private readonly float grad2Speed = 0.66f;
        private readonly float grad3Speed = 1.03f;
        private readonly float grad4Speed = 0.77f;

        public override void SetStaticDefaults()
        {
            // Don't show this projectile in the bestiary or other UI
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 2400;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.penetrate = -1; // Infinite penetrate
            Projectile.tileCollide = false; // We handle collision manually
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = 2; // Reset each frame while channeling
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10; // Hit each NPC every 10 frames
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            // ---- CHANNELING CHECK ----
            // Keep the projectile alive while the player is channeling
            if (!owner.channel || owner.dead || !owner.active || owner.noItems || owner.CCed)
            {
                Projectile.Kill();
                return;
            }

            // Reset timeLeft so the projectile stays alive while channeling
            Projectile.timeLeft = 2;

            // ---- AIM TRACKING ----
            // Smoothly rotate toward cursor position
            Vector2 aimDirection = (Main.MouseWorld - owner.MountedCenter).SafeNormalize(Vector2.UnitX);
            float targetAngle = aimDirection.ToRotation();
            float currentAngle = Projectile.velocity.ToRotation();

            // Smooth rotation with clamped turn speed
            float angleDiff = MathHelper.WrapAngle(targetAngle - currentAngle);
            float clampedDiff = MathHelper.Clamp(angleDiff, -AimSpeed, AimSpeed);
            Projectile.velocity = (currentAngle + clampedDiff).ToRotationVector2();

            // ---- POSITION ----
            // Lock projectile to player center
            Projectile.Center = owner.MountedCenter;

            // Set player direction to face the beam
            owner.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);

            // Hold the player's arm in the beam direction
            owner.heldProj = Projectile.whoAmI;
            owner.itemTime = 2;
            owner.itemAnimation = 2;
            owner.itemRotation = (float)Math.Atan2(Projectile.velocity.Y * owner.direction,
                Projectile.velocity.X * owner.direction);

            // ---- COLLISION RAYCAST ----
            // Cast a ray from beam start to find where it hits a tile
            beamLength = MaxBeamLength;
            Vector2 beamStart = Projectile.Center + Projectile.velocity * BeamStartOffset;

            // Step along the beam checking for tile collisions
            for (float d = 0; d < MaxBeamLength; d += 16f)
            {
                Vector2 checkPoint = beamStart + Projectile.velocity * d;
                Point tileCoords = checkPoint.ToTileCoordinates();

                if (tileCoords.X < 0 || tileCoords.X >= Main.maxTilesX ||
                    tileCoords.Y < 0 || tileCoords.Y >= Main.maxTilesY)
                    continue;

                Tile tile = Main.tile[tileCoords.X, tileCoords.Y];
                if (tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
                {
                    beamLength = d;
                    break;
                }
            }

            // ---- LIGHTING ----
            // Add light along the beam at intervals
            for (float d = 0; d < beamLength; d += 200f)
            {
                Vector2 lightPos = beamStart + Projectile.velocity * d;
                Lighting.AddLight(lightPos, Color.White.ToVector3() * 0.8f);
            }

            // Add extra light at the endpoint
            Vector2 endPoint = beamStart + Projectile.velocity * beamLength;
            Lighting.AddLight(endPoint, Color.White.ToVector3() * 1.2f);

            // ---- DUST ACCENTS ----
            SpawnBeamDust(beamStart, endPoint);

            // Increment flare rotation
            flareRotation += 1.15f * (Projectile.velocity.X > 0 ? 1f : -1f);
        }

        /// <summary>
        /// Spawns dust particles along the beam and at the endpoint.
        /// </summary>
        private void SpawnBeamDust(Vector2 beamStart, Vector2 endPoint)
        {
            float rot = Projectile.velocity.ToRotation();
            Color[] themeColors = LFTextures.GetDustColorsForTheme(CurrentTheme);

            // ---- ALONG BEAM ----
            // Every 150px, 25% chance to spawn a glowing dust in the current theme's colors
            for (float d = 0; d < beamLength; d += 150f)
            {
                if (!Main.rand.NextBool(4))
                    continue;

                Vector2 pos = beamStart + Projectile.velocity * d;
                Vector2 perpOffset = rot.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-30f, 30f);
                Vector2 vel = rot.ToRotationVector2().RotatedByRandom(0.05f) * Main.rand.NextFloat(2f, 6f);
                Color col = themeColors[Main.rand.Next(themeColors.Length)];

                Dust dust = Dust.NewDustPerfect(pos + perpOffset, DustID.RainbowMk2, vel, newColor: col, Scale: Main.rand.NextFloat(0.4f, 0.8f));
                dust.noGravity = true;
                dust.fadeIn = 0.8f;
            }

            // ---- AT ENDPOINT ----
            // 3-4 dusts per frame at the endpoint in theme colors
            for (int i = 0; i < 3 + Main.rand.Next(0, 2); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(15f, 15f);
                Color col = themeColors[Main.rand.Next(themeColors.Length)];

                Dust dust = Dust.NewDustPerfect(endPoint, DustID.RainbowMk2, vel, newColor: col, Scale: Main.rand.NextFloat(0.5f, 0.8f));
                dust.noGravity = true;
                dust.fadeIn = 0.6f;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Check collision along the beam line
            Vector2 beamStart = Projectile.Center + Projectile.velocity * BeamStartOffset;
            Vector2 beamEnd = beamStart + Projectile.velocity * beamLength;

            float _ = 0f;
            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(), targetHitbox.Size(),
                beamStart, beamEnd,
                BaseBeamWidth * 0.5f, ref _);
        }

        public override void CutTiles()
        {
            // Cut tiles (grass, pots, etc.) along the beam path
            DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
            Vector2 beamStart = Projectile.Center + Projectile.velocity * BeamStartOffset;
            Vector2 beamEnd = beamStart + Projectile.velocity * beamLength;
            Utils.PlotTileLine(beamStart, beamEnd, BaseBeamWidth, DelegateMethods.CutTiles);
        }

        public override bool ShouldUpdatePosition() => false; // Don't move — position is locked to player

        public override bool PreDraw(ref Color lightColor)
        {
            // Draw the beam body and endpoint flares
            DrawBeamBody();
            DrawEndpointFlares();

            return false; // Don't draw the default projectile sprite
        }

        /// <summary>
        /// Draws the main beam body using a VertexStrip and custom shader.
        /// 
        /// HOW IT WORKS:
        /// 1. Build a 2-point VertexStrip (start + end) → creates a rectangular quad
        /// 2. Load and configure the ConvergenceBeamShader with all texture/uniform inputs
        /// 3. Apply the shader pass and draw the triangle strip
        /// 4. Reset the pixel shader to default
        /// 
        /// The shader composites 4 scrolling detail textures tinted by a rainbow gradient LUT,
        /// creating a rich multi-layered beam effect. See ConvergenceBeamShader.fx for full docs.
        /// </summary>
        private void DrawBeamBody()
        {
            float rot = Projectile.velocity.ToRotation();
            Vector2 startPoint = Projectile.Center + Projectile.velocity * BeamStartOffset;
            Vector2 endPoint = startPoint + Projectile.velocity * beamLength;

            // ---- BUILD VERTEX STRIP ----
            // Two positions create a simple rectangular quad.
            // VertexStrip generates the triangle mesh with UV coordinates:
            //   UV.x = 0..1 along beam length
            //   UV.y = 0..1 across beam width
            Vector2[] positions = { startPoint, endPoint };
            float[] rotations = { rot, rot };

            // Vertex color is white — the shader handles all coloring
            Color StripColor(float progress) => Color.White;

            // Uniform beam width
            float StripWidth(float progress) => BaseBeamWidth;

            VertexStrip strip = new VertexStrip();
            strip.PrepareStrip(positions, rotations, StripColor, StripWidth,
                -Main.screenPosition, includeBacksides: true);

            // ---- LOAD & CONFIGURE SHADER ----
            if (beamShader == null)
            {
                beamShader = ModContent.Request<Effect>(
                    "MagnumOpus/Content/FoundationWeapons/LaserFoundation/Shaders/ConvergenceBeamShader",
                    AssetRequestMode.ImmediateLoad).Value;
            }

            // Transform matrix — required for the vertex shader to project world positions to screen
            beamShader.Parameters["WorldViewProjection"].SetValue(
                Main.GameViewMatrix.NormalizedTransformationmatrix);

            // Alpha mask texture — shapes the beam cross-section
            beamShader.Parameters["onTex"].SetValue(LFTextures.BeamAlphaMask.Value);

            // Color gradient LUT — theme-specific gradient that scrolls along the beam
            beamShader.Parameters["gradientTex"].SetValue(LFTextures.GetGradientForTheme(CurrentTheme));

            // Base color (white) and saturation power
            beamShader.Parameters["baseColor"].SetValue(Color.White.ToVector3());
            beamShader.Parameters["satPower"].SetValue(0.8f);

            // 4 detail textures that provide visual structure
            beamShader.Parameters["sampleTexture1"].SetValue(LFTextures.DetailThinGlowLine.Value);
            beamShader.Parameters["sampleTexture2"].SetValue(LFTextures.DetailSpark.Value);
            beamShader.Parameters["sampleTexture3"].SetValue(LFTextures.DetailExtra.Value);
            beamShader.Parameters["sampleTexture4"].SetValue(LFTextures.DetailTrailLoop.Value);

            // Gradient scroll speeds — different per layer for parallax
            beamShader.Parameters["grad1Speed"].SetValue(grad1Speed);
            beamShader.Parameters["grad2Speed"].SetValue(grad2Speed);
            beamShader.Parameters["grad3Speed"].SetValue(grad3Speed);
            beamShader.Parameters["grad4Speed"].SetValue(grad4Speed);

            // Detail texture intensity multipliers
            beamShader.Parameters["tex1Mult"].SetValue(1.25f);
            beamShader.Parameters["tex2Mult"].SetValue(1.5f);
            beamShader.Parameters["tex3Mult"].SetValue(1.15f);
            beamShader.Parameters["tex4Mult"].SetValue(2.5f);
            beamShader.Parameters["totalMult"].SetValue(1f);

            // UV repetition — proportional to beam length so patterns don't stretch
            float dist = (endPoint - startPoint).Length();
            float repVal = dist / 2000f;
            beamShader.Parameters["gradientReps"].SetValue(0.75f * repVal);
            beamShader.Parameters["tex1reps"].SetValue(1.15f * repVal);
            beamShader.Parameters["tex2reps"].SetValue(1.15f * repVal);
            beamShader.Parameters["tex3reps"].SetValue(1.15f * repVal);
            beamShader.Parameters["tex4reps"].SetValue(1.15f * repVal);

            // Time drives all UV scrolling — negative = scrolls toward beam tip
            beamShader.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * -0.024f);

            // ---- DRAW ----
            // Apply the pixel shader pass and draw the vertex strip
            beamShader.CurrentTechnique.Passes["MainPS"].Apply();
            strip.DrawTrail();

            // Reset to Terraria's default pixel shader so other things draw correctly
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Draws additive-blended flare sprites at the beam origin and endpoint,
        /// using the FlareRainbowShader to convert white flare textures into
        /// spinning radial rainbow flares.
        /// 
        /// The shader is passed as the SpriteBatch effect parameter, so every
        /// Draw call within that batch automatically gets the rainbow treatment.
        /// 
        /// Shader parameters:
        ///   rotation       — spins the texture UVs (driven by flareRotation)
        ///   rainbowRotation — shifts the HSV color wheel (driven at slower rate)
        ///   intensity       — brightness of the rainbow coloring
        ///   fadeStrength    — directional alpha fade (1.0 = fade across full texture)
        /// </summary>
        private void DrawEndpointFlares()
        {
            SpriteBatch sb = Main.spriteBatch;

            float rot = Projectile.velocity.ToRotation();
            Vector2 drawPos = Projectile.Center - Main.screenPosition
                + Projectile.velocity * BeamStartOffset;
            Vector2 endPoint = drawPos + Projectile.velocity * beamLength;

            // Pulsing scale for the lens flare
            float sinPulse = MathF.Sin((float)Main.timeForVisualEffects * 0.04f);
            float cosPulse = MathF.Cos((float)Main.timeForVisualEffects * 0.06f);

            Texture2D lensFlare = LFTextures.LensFlare.Value;
            Texture2D starFlare = LFTextures.StarFlare.Value;
            Texture2D glowOrb = LFTextures.GlowOrb.Value;
            Texture2D softGlow = LFTextures.SoftGlow.Value;

            // ---- LOAD FLARE SHADER ----
            if (flareShader == null)
            {
                flareShader = ModContent.Request<Effect>(
                    "MagnumOpus/Content/FoundationWeapons/LaserFoundation/Shaders/FlareRainbowShader",
                    AssetRequestMode.ImmediateLoad).Value;
            }

            // Configure the rainbow flare shader
            // rotation drives the spinning UV effect, rainbowRotation shifts the color wheel
            flareShader.Parameters["rotation"].SetValue(flareRotation * 0.03f * 2.5f);
            flareShader.Parameters["rainbowRotation"].SetValue(flareRotation * 0.01f * 2.5f);
            flareShader.Parameters["intensity"].SetValue(1f);
            flareShader.Parameters["fadeStrength"].SetValue(1f);

            // ---- SWAP TO ADDITIVE + RAINBOW SHADER ----
            // The shader is passed as the Effect parameter of SpriteBatch.Begin,
            // so every Draw call in this batch gets the rainbow pixel shader applied.
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, flareShader,
                Main.GameViewMatrix.EffectMatrix);

            // ---- BEAM ORIGIN FLARES ----
            // Sigil-style stretched flare (squished X, full Y) — matches SLP's sigilScale pattern
            Vector2 sigilScale = new Vector2(0.2f, 1f) * 0.55f;
            sigilScale.Y = MathHelper.Min(sigilScale.Y, 0.293f); // Cap Y to 300px on 1024px
            Vector2 sigilScalePulse = sigilScale * (1.75f + 0.25f * sinPulse);
            sigilScalePulse.Y = MathHelper.Min(sigilScalePulse.Y, 0.293f); // Cap Y to 300px on 1024px

            // Double-draw sigil for extra brightness (additive stacking)
            sb.Draw(softGlow, drawPos, null, Color.White, rot,
                softGlow.Size() / 2f, sigilScale, SpriteEffects.None, 0f);
            sb.Draw(softGlow, drawPos, null, Color.White, rot,
                softGlow.Size() / 2f, sigilScale, SpriteEffects.None, 0f);

            // Lens flare with sin-driven oscillation along beam direction
            float sinOffset = -MathF.Cos(((float)Main.timeForVisualEffects * 0.08f) / 2f) + 1f;
            sb.Draw(lensFlare, drawPos + new Vector2(1f, 0f).RotatedBy(rot) * (15f * sinOffset),
                null, Color.White, rot, lensFlare.Size() / 2f, sigilScalePulse, SpriteEffects.None, 0f);

            // Star flares at origin (double-draw for brightness)
            sb.Draw(starFlare, drawPos, null, Color.White, rot,
                starFlare.Size() / 2f, sigilScale, SpriteEffects.None, 0f);
            sb.Draw(starFlare, drawPos, null, Color.White, rot,
                starFlare.Size() / 2f, sigilScale, SpriteEffects.None, 0f);

            // ---- BEAM ENDPOINT FLARES ----
            // Glow orb
            sb.Draw(glowOrb, endPoint, null, Color.White, flareRotation * 0.1f,
                glowOrb.Size() / 2f, 0.293f, SpriteEffects.None, 0f);

            // Lens flare and star flares at endpoint
            float endScale = 0.7f;
            sb.Draw(lensFlare, endPoint, null, Color.White, flareRotation * 0.02f,
                lensFlare.Size() / 2f, MathHelper.Min(endScale * 0.45f, 0.293f), SpriteEffects.None, 0f);
            sb.Draw(starFlare, endPoint, null, Color.White, flareRotation * 0.05f,
                starFlare.Size() / 2f, MathHelper.Min(endScale * 0.6f, 0.293f), SpriteEffects.None, 0f);
            sb.Draw(starFlare, endPoint, null, Color.White, flareRotation * 0.077f,
                starFlare.Size() / 2f, endScale * 0.35f, SpriteEffects.None, 0f);

            // ---- RESTORE ALPHA BLEND ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Small burst of dust on hit in theme colors for feedback
            Color[] themeColors = LFTextures.GetDustColorsForTheme(CurrentTheme);
            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                Color col = themeColors[Main.rand.Next(themeColors.Length)];
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.RainbowMk2, vel, newColor: col, Scale: Main.rand.NextFloat(0.6f, 1f));
                dust.noGravity = true;
            }
        }
    }
}
