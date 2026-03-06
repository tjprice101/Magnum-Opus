using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Content;
using MagnumOpus.Content.Fate.Debuffs;
using MagnumOpus.Content.Fate.ResonantWeapons.CodaOfAnnihilation.Utilities;
using MagnumOpus.Content.Fate.ResonantWeapons.CodaOfAnnihilation.Particles;
using MagnumOpus.Content.Fate.ResonantWeapons.CodaOfAnnihilation.Primitives;
using MagnumOpus.Content.Fate.ResonantWeapons.CodaOfAnnihilation.Shaders;

namespace MagnumOpus.Content.Fate.ResonantWeapons.CodaOfAnnihilation.Projectiles
{
    /// <summary>
    /// The held spinning sword that orbits the player during each Coda swing.
    /// Orbit radius 65f, �E�ｱ144�E�ｰ arc (0.8�E�), swing speed 0.12 rad/frame.
    /// Deals melee damage via line collision from player center to tip.
    /// Self-contained VFX 窶・uses own particle handler for all effects.
    /// </summary>
    public class CodaHeldSwing : ModProjectile
    {
        // Swing speed in radians per frame
        private const float SwingSpeed = 0.12f;

        // Orbit radius around player
        private const float OrbitRadius = 65f;

        // Maximum swing arc: �E�ｱ144�E�ｰ = 0.8�E�
        private const float MaxSwingArc = MathHelper.Pi * 0.8f;

        // Current swing angle offset
        private float SwingAngle
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        // Starting angle toward cursor
        private float BaseAngle
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        private bool initialized = false;

        // 12-position trail
        private Vector2[] trailPositions = new Vector2[12];
        private float[] trailRotations = new float[12];
        private int trailIndex = 0;

        // Shader-driven trail rendering
        private CodaTrailRenderer _trailRenderer;
        private static Asset<Texture2D> _noiseTex;

        // SmearDistort overlay textures
        private static Asset<Texture2D> _smearArcTexture;
        private static Asset<Texture2D> _smearNoiseTex;
        private static Asset<Texture2D> _smearGradientTex;
        private Effect _smearDistortShader;
        private bool _smearShaderLoaded;
        // CrescentBloom textures
        private static Asset<Texture2D> _bloomCircle;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _starFlareTex;

        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/CodaOfAnnihilation";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            // Kill when animation ends
            if (!owner.active || owner.dead || owner.itemAnimation <= 1)
            {
                Projectile.Kill();
                return;
            }

            // Initialize base angle toward cursor on first frame
            if (!initialized)
            {
                initialized = true;
                Vector2 toCursor = Main.MouseWorld - owner.Center;
                BaseAngle = toCursor.ToRotation();
                SwingAngle = 0f;

                // Initialize trail
                for (int i = 0; i < trailPositions.Length; i++)
                {
                    trailPositions[i] = owner.Center;
                    trailRotations[i] = BaseAngle;
                }
            }

            // Keep alive
            Projectile.timeLeft = 2;

            // Advance swing angle 窶・�E�ｱ144�E�ｰ arc
            SwingAngle += SwingSpeed * owner.direction;
            SwingAngle = MathHelper.Clamp(SwingAngle, -MaxSwingArc, MaxSwingArc);

            float actualAngle = BaseAngle + SwingAngle;

            // Position orbiting player
            Projectile.Center = owner.Center + actualAngle.ToRotationVector2() * OrbitRadius;
            Projectile.rotation = actualAngle + MathHelper.PiOver4;

            // Update trail
            trailIndex = (trailIndex + 1) % trailPositions.Length;
            trailPositions[trailIndex] = Projectile.Center;
            trailRotations[trailIndex] = Projectile.rotation;

            // VFX at sword tip
            Vector2 tipOffset = Projectile.rotation.ToRotationVector2() * 45f;
            Vector2 tipPos = Projectile.Center + tipOffset;

            SpawnSwingParticles(owner, actualAngle, tipPos);

            // Dynamic lighting
            Lighting.AddLight(Projectile.Center, CodaUtils.CodaPurple.ToVector3() * 0.8f);
            Lighting.AddLight(tipPos, CodaUtils.CodaCrimson.ToVector3() * 0.5f);
        }

        private void SpawnSwingParticles(Player owner, float actualAngle, Vector2 tipPos)
        {
            float swingProgress = (SwingAngle + MaxSwingArc) / (MaxSwingArc * 2f);
            swingProgress = MathHelper.Clamp(swingProgress, 0f, 1f);

            // Trail sparks at tip
            if (Main.GameUpdateCount % 3 == 0)
            {
                Color sparkColor = CodaUtils.GetAnnihilationGradient(swingProgress);
                Vector2 sparkVel = actualAngle.ToRotationVector2().RotatedBy(MathHelper.PiOver2 * Main.player[Projectile.owner].direction) * 3f;
                sparkVel += Main.rand.NextVector2Circular(1f, 1f);
                CodaParticleHandler.SpawnParticle(new ArcSparkParticle(
                    tipPos, sparkVel, sparkColor * 0.8f, 0.3f, 15));
            }

            // Swing trail glow
            if (Main.GameUpdateCount % 4 == 0)
            {
                Color trailColor = CodaUtils.GetAnnihilationGradient(0.4f + swingProgress * 0.4f);
                CodaParticleHandler.SpawnParticle(new SwingTrailParticle(
                    tipPos,
                    actualAngle.ToRotationVector2() * 2f,
                    trailColor * 0.6f,
                    0.25f,
                    15));
            }

            // Cosmic motes along the arc
            if (Main.rand.NextBool(4))
            {
                float dist = Main.rand.NextFloat(30f, OrbitRadius);
                Vector2 motePos = Main.player[Projectile.owner].Center + actualAngle.ToRotationVector2() * dist;
                Color moteColor = Color.Lerp(CodaUtils.CodaPurple, CodaUtils.CodaPink, Main.rand.NextFloat());
                CodaParticleHandler.SpawnParticle(new CosmicMoteParticle(
                    motePos,
                    Main.rand.NextVector2Circular(0.5f, 0.5f),
                    moteColor * 0.5f,
                    0.18f,
                    14));
            }

            // Glyphs 窶・fate's runes
            if (Main.rand.NextBool(15))
            {
                CodaParticleHandler.SpawnParticle(new GlyphBurstParticle(
                    tipPos + Main.rand.NextVector2Circular(10f, 10f),
                    CodaUtils.CodaPink * 0.5f,
                    0.25f,
                    16));
            }

            // Music notes 窶・the coda's symphony
            if (Main.rand.NextBool(8))
            {
                Color noteColor = Color.Lerp(CodaUtils.CodaCrimson, CodaUtils.CodaPurple, Main.rand.NextFloat());
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f);
                CodaParticleHandler.SpawnParticle(new ZenithNoteParticle(
                    tipPos, noteVel, noteColor, 0.35f, 35));
            }

            // Star sparkles
            if (Main.rand.NextBool(12))
            {
                Vector2 sparkPos = Main.player[Projectile.owner].Center + actualAngle.ToRotationVector2() * Main.rand.NextFloat(30f, OrbitRadius);
                CodaParticleHandler.SpawnParticle(new CosmicMoteParticle(
                    sparkPos,
                    Main.rand.NextVector2Circular(0.3f, 0.3f),
                    CodaUtils.AnnihilationWhite * 0.4f,
                    0.15f,
                    10));
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply DestinyCollapse 300 ticks (longer than flying sword)
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);

            Vector2 hitPos = target.Center;

            // === Multi-layer bloom flash (Foundation-tier) ===
            _bloomCircle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
            _softRadialBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            if (_softRadialBloom?.Value != null && !Main.dedServ)
            {
                SpriteBatch sb = Main.spriteBatch;
                try
                {
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                    Texture2D softBloom = _softRadialBloom.Value;
                    Vector2 drawPos = hitPos - Main.screenPosition;
                    Vector2 bloomOrigin = softBloom.Size() / 2f;

                    sb.Draw(softBloom, drawPos, null, CodaUtils.Additive(CodaUtils.VoidBlack, 0.25f),
                        0f, bloomOrigin, 1.8f, SpriteEffects.None, 0f);
                    sb.Draw(softBloom, drawPos, null, CodaUtils.Additive(CodaUtils.CodaCrimson, 0.4f),
                        0f, bloomOrigin, 1.2f, SpriteEffects.None, 0f);
                    sb.Draw(softBloom, drawPos, null, CodaUtils.Additive(CodaUtils.CodaPink, 0.35f),
                        0f, bloomOrigin, 0.7f, SpriteEffects.None, 0f);
                    if (_bloomCircle?.Value != null)
                    {
                        sb.Draw(_bloomCircle.Value, drawPos, null, CodaUtils.Additive(CodaUtils.AnnihilationWhite, 0.6f),
                            0f, _bloomCircle.Value.Size() / 2f, 0.35f, SpriteEffects.None, 0f);
                    }
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                        DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
                catch
                {
                    try { sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                        DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix); } catch { }
                }
            }

            // Annihilation flare (enhanced)
            CodaParticleHandler.SpawnParticle(new AnnihilationFlareParticle(
                hitPos, CodaUtils.AnnihilationWhite, 0.9f, 20));
            CodaParticleHandler.SpawnParticle(new AnnihilationFlareParticle(
                hitPos, CodaUtils.CodaCrimson, 0.6f, 16));

            // Glyph burst (enhanced count)
            for (int i = 0; i < 6; i++)
            {
                CodaParticleHandler.SpawnParticle(new GlyphBurstParticle(
                    hitPos + Main.rand.NextVector2Circular(20f, 20f),
                    CodaUtils.CodaPink * 0.8f, 0.4f, 22));
            }

            // Radial spark burst (increased)
            for (int i = 0; i < 14; i++)
            {
                float angle = MathHelper.TwoPi * i / 14f + Main.rand.NextFloat(-0.1f, 0.1f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 10f);
                Color c = CodaUtils.GetAnnihilationGradient(Main.rand.NextFloat());
                CodaParticleHandler.SpawnParticle(new ArcSparkParticle(
                    hitPos, vel, c * 0.9f, 0.4f, 22));
            }

            // Directional slash sparks
            Vector2 slashDir = (Projectile.rotation + MathHelper.PiOver2).ToRotationVector2();
            for (int i = 0; i < 6; i++)
            {
                float spread = Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 markVel = slashDir.RotatedBy(spread) * Main.rand.NextFloat(5f, 12f);
                Color markCol = Color.Lerp(CodaUtils.StarGold, CodaUtils.AnnihilationWhite, Main.rand.NextFloat());
                CodaParticleHandler.SpawnParticle(new ArcSparkParticle(
                    hitPos, markVel, markCol, 0.3f, 16));
            }

            // Music note burst (enhanced cascade)
            for (int i = 0; i < 7; i++)
            {
                float angle = MathHelper.TwoPi * i / 7f + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                noteVel.Y -= 1.5f;
                Color noteCol = Color.Lerp(CodaUtils.CodaCrimson, CodaUtils.CodaPurple, Main.rand.NextFloat());
                CodaParticleHandler.SpawnParticle(new ZenithNoteParticle(
                    hitPos, noteVel, noteCol, 0.45f, 35));
            }

            // Cosmic motes expanding outward
            for (int i = 0; i < 6; i++)
            {
                Vector2 moteVel = Main.rand.NextVector2Circular(3f, 3f);
                moteVel.Y -= 1f;
                Color moteCol = Color.Lerp(CodaUtils.CodaPurple, CodaUtils.CodaPink, Main.rand.NextFloat());
                CodaParticleHandler.SpawnParticle(new CosmicMoteParticle(
                    hitPos + Main.rand.NextVector2Circular(15f, 15f), moteVel,
                    moteCol * 0.7f, 0.25f, 28));
            }

            // Impact sound
            SoundEngine.PlaySound(SoundID.Item71 with { Volume = 0.7f, Pitch = 0.2f }, hitPos);

            Lighting.AddLight(hitPos, CodaUtils.CodaCrimson.ToVector3() * 1.2f);
            Lighting.AddLight(hitPos, CodaUtils.AnnihilationWhite.ToVector3() * 0.8f);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Line collision from player center to tip (40f width)
            Player owner = Main.player[Projectile.owner];
            float _ = 0f;
            Vector2 lineStart = owner.Center;
            Vector2 tipOffset = Projectile.rotation.ToRotationVector2() * 45f;
            Vector2 lineEnd = Projectile.Center + tipOffset;

            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), lineStart, lineEnd, 40f, ref _);
        }

        // ======================== SMEAR DISTORT OVERLAY ========================

        /// <summary>
        /// Foundation-tier SmearDistort overlay adapted for orbital swing.
        /// Coda identity: annihilation void with crimson bleeding through.
        /// </summary>
        private void DrawSmearOverlay(SpriteBatch sb)
        {
            _smearArcTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/SlashArcs/SwordArcSmear");
            _smearNoiseTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/TileableFBMNoise");
            _smearGradientTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/ColorGradients/FateGradientLUTandRAMP");

            if (_smearArcTexture?.Value == null) return;

            if (!_smearShaderLoaded)
            {
                _smearShaderLoaded = true;
                try
                {
                    _smearDistortShader = ModContent.Request<Effect>(
                        "MagnumOpus/Content/FoundationWeapons/SwordSmearFoundation/Shaders/SmearDistortShader",
                        AssetRequestMode.ImmediateLoad).Value;
                }
                catch { _smearDistortShader = null; }
            }

            Player owner = Main.player[Projectile.owner];
            Vector2 center = owner.Center - Main.screenPosition;
            float swingRotation = (BaseAngle + SwingAngle) + MathHelper.PiOver4;
            Texture2D smearTex = _smearArcTexture.Value;
            Vector2 smearOrigin = smearTex.Size() / 2f;
            float baseScale = OrbitRadius / (smearTex.Width * 0.35f);
            float time = (float)Main.timeForVisualEffects * 0.01f;

            Color outerColor = CodaUtils.Additive(CodaUtils.VoidBlack, 0.3f);
            Color mainColor = CodaUtils.Additive(CodaUtils.CodaCrimson, 0.55f);
            Color coreColor = CodaUtils.Additive(CodaUtils.CodaPink, 0.65f);
            int dir = owner.direction;
            SpriteEffects fx = dir < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            try
            {
                if (_smearDistortShader != null)
                {
                    sb.End();
                    var shaderParams = _smearDistortShader.Parameters;
                    shaderParams["uTime"]?.SetValue(time);
                    if (_smearNoiseTex?.Value != null)
                    {
                        Main.graphics.GraphicsDevice.Textures[1] = _smearNoiseTex.Value;
                        Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
                    }
                    if (_smearGradientTex?.Value != null)
                    {
                        Main.graphics.GraphicsDevice.Textures[2] = _smearGradientTex.Value;
                        Main.graphics.GraphicsDevice.SamplerStates[2] = SamplerState.LinearClamp;
                    }

                    shaderParams["distortStrength"]?.SetValue(0.07f);
                    sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap,
                        DepthStencilState.None, RasterizerState.CullNone, _smearDistortShader, Main.GameViewMatrix.TransformationMatrix);
                    sb.Draw(smearTex, center, null, outerColor, swingRotation, smearOrigin, baseScale * 1.18f, fx, 0f);
                    sb.End();

                    shaderParams["distortStrength"]?.SetValue(0.04f);
                    sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap,
                        DepthStencilState.None, RasterizerState.CullNone, _smearDistortShader, Main.GameViewMatrix.TransformationMatrix);
                    sb.Draw(smearTex, center, null, mainColor, swingRotation, smearOrigin, baseScale, fx, 0f);
                    sb.End();

                    shaderParams["distortStrength"]?.SetValue(0.02f);
                    sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap,
                        DepthStencilState.None, RasterizerState.CullNone, _smearDistortShader, Main.GameViewMatrix.TransformationMatrix);
                    sb.Draw(smearTex, center, null, coreColor, swingRotation, smearOrigin, baseScale * 0.82f, fx, 0f);
                    sb.End();

                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                        DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
                else
                {
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                    sb.Draw(smearTex, center, null, outerColor, swingRotation, smearOrigin, baseScale * 1.18f, fx, 0f);
                    sb.Draw(smearTex, center, null, mainColor, swingRotation, smearOrigin, baseScale, fx, 0f);
                    sb.Draw(smearTex, center, null, coreColor, swingRotation, smearOrigin, baseScale * 0.82f, fx, 0f);
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                        DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }
            catch
            {
                try { sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix); } catch { }
            }
        }

        // ======================== CRESCENT BLOOM ========================

        /// <summary>
        /// Foundation-tier 6-layer graduated bloom at blade tip.
        /// Coda identity: annihilation void with crimson/pink bleeding.
        /// </summary>
        private void DrawCrescentBloom(SpriteBatch sb)
        {
            _bloomCircle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
            _softRadialBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            _starFlareTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/StarFlare");

            if (_bloomCircle?.Value == null || _softRadialBloom?.Value == null) return;

            Vector2 tipOffset = Projectile.rotation.ToRotationVector2() * 45f;
            Vector2 tipWorld = Projectile.Center + tipOffset;
            Vector2 tipDraw = tipWorld - Main.screenPosition;
            float breath = 0.85f + MathF.Sin((float)Main.timeForVisualEffects * 0.06f) * 0.15f;
            float intensity = 0.8f * breath;

            try
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Texture2D bloom = _softRadialBloom.Value;
                Texture2D point = _bloomCircle.Value;
                Vector2 bloomOrigin = bloom.Size() / 2f;
                Vector2 pointOrigin = point.Size() / 2f;

                sb.Draw(bloom, tipDraw, null, CodaUtils.Additive(CodaUtils.VoidBlack, 0.15f * intensity),
                    0f, bloomOrigin, 1.6f * intensity, SpriteEffects.None, 0f);
                sb.Draw(bloom, tipDraw, null, CodaUtils.Additive(CodaUtils.CodaCrimson, 0.3f * intensity),
                    0f, bloomOrigin, 1.1f * intensity, SpriteEffects.None, 0f);
                sb.Draw(bloom, tipDraw, null, CodaUtils.Additive(CodaUtils.CodaPink, 0.35f * intensity),
                    0f, bloomOrigin, 0.65f * intensity, SpriteEffects.None, 0f);
                sb.Draw(point, tipDraw, null, CodaUtils.Additive(CodaUtils.StarGold, 0.45f * intensity),
                    0f, pointOrigin, 0.35f * intensity, SpriteEffects.None, 0f);
                sb.Draw(point, tipDraw, null, CodaUtils.Additive(CodaUtils.AnnihilationWhite, 0.55f * intensity),
                    0f, pointOrigin, 0.18f * intensity, SpriteEffects.None, 0f);

                if (_starFlareTex?.Value != null)
                {
                    float starRot = (float)Main.timeForVisualEffects * 0.02f;
                    Texture2D starTex = _starFlareTex.Value;
                    Vector2 starOrigin = starTex.Size() / 2f;
                    sb.Draw(starTex, tipDraw, null, CodaUtils.Additive(CodaUtils.CodaCrimson, 0.3f * intensity),
                        starRot, starOrigin, 0.4f * intensity, SpriteEffects.None, 0f);
                    sb.Draw(starTex, tipDraw, null, CodaUtils.Additive(CodaUtils.AnnihilationWhite, 0.2f * intensity),
                        -starRot * 0.7f, starOrigin, 0.25f * intensity, SpriteEffects.None, 0f);
                }

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            catch
            {
                try { sb.End(); sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix); } catch { }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D weaponTex = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;

            if (weaponTex == null) return false;

            _noiseTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/VoronoiNoise");

            Vector2 origin = weaponTex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            try
            {
                // === Layer 0: SmearDistort Overlay (Foundation-tier, adapted for orbital swing) ===
                DrawSmearOverlay(spriteBatch);

                // === Layer 0.5: Shader-driven swing arc trail (GPU primitives) ===
                spriteBatch.End();
                DrawShaderTrail();

                // Restart SpriteBatch for sprite-based layers
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                // === Layer 1: Afterimage trail (sprite-based, with chromatic separation) ===
                for (int i = 0; i < trailPositions.Length; i++)
                {
                    int actualIndex = (trailIndex - i + trailPositions.Length) % trailPositions.Length;
                    if (trailPositions[actualIndex] == Vector2.Zero) continue;

                    Vector2 trailPos = trailPositions[actualIndex] - Main.screenPosition;
                    float trailRot = trailRotations[actualIndex];

                    float progress = (float)i / trailPositions.Length;
                    float trailAlpha = (1f - progress) * 0.4f;
                    float trailScale = 1f - progress * 0.2f;

                    // Chromatic split: crimson and purple ghosts offset in opposite directions
                    Vector2 chromDir = trailRot.ToRotationVector2();
                    Vector2 chromOffset = new Vector2(-chromDir.Y, chromDir.X) * (1.5f + progress * 3f);

                    Color purpleGhost = CodaUtils.CodaPurple with { A = 0 } * (trailAlpha * 0.5f);
                    Color crimsonGhost = CodaUtils.CodaCrimson with { A = 0 } * (trailAlpha * 0.4f);

                    spriteBatch.Draw(weaponTex, trailPos - chromOffset, null, purpleGhost, trailRot, origin, trailScale, SpriteEffects.None, 0f);
                    spriteBatch.Draw(weaponTex, trailPos + chromOffset, null, crimsonGhost, trailRot, origin, trailScale * 0.95f, SpriteEffects.None, 0f);

                    // Core trail (original)
                    Color trailColor = Color.Lerp(CodaUtils.CodaPink, CodaUtils.CodaPurple, progress);
                    trailColor = trailColor with { A = 0 } * trailAlpha;
                    spriteBatch.Draw(weaponTex, trailPos, null, trailColor, trailRot, origin, trailScale, SpriteEffects.None, 0f);
                }

                // === Layer 2: Outer cosmic glow (pulsing) ===
                float pulse = 1f + MathF.Sin((float)Main.timeForVisualEffects * 0.07f) * 0.08f;
                Color outerGlow = CodaUtils.CodaPurple with { A = 0 } * 0.35f;
                spriteBatch.Draw(weaponTex, drawPos, null, outerGlow, Projectile.rotation, origin, 1.2f * pulse, SpriteEffects.None, 0f);

                // === Layer 3: Middle glow layer ===
                Color midGlow = CodaUtils.CodaPink with { A = 0 } * 0.4f;
                spriteBatch.Draw(weaponTex, drawPos, null, midGlow, Projectile.rotation, origin, 1.1f, SpriteEffects.None, 0f);

                // === Layer 4: Main weapon sprite ===
                spriteBatch.Draw(weaponTex, drawPos, null, lightColor, Projectile.rotation, origin, 1f, SpriteEffects.None, 0f);

                // === Layer 5: Inner bright glow ===
                Color innerGlow = Color.Lerp(CodaUtils.CodaCrimson, Color.White, 0.4f) with { A = 0 } * 0.35f;
                spriteBatch.Draw(weaponTex, drawPos, null, innerGlow, Projectile.rotation, origin, 0.85f, SpriteEffects.None, 0f);

                // === Layer 6: CrescentBloom at blade tip ===
                DrawCrescentBloom(spriteBatch);
            }
            catch
            {
                try
                {
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                        DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
                catch { }
            }

            // Theme accents (additive pass)
            try
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                CodaUtils.DrawThemeAccents(spriteBatch, Projectile.Center, 1f, 0.6f);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            catch
            {
                try
                {
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                        DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
                catch { }
            }

            return false;
        }

        /// <summary>Renders the swing arc using the CodaSwingArc shader with Voronoi cracking.</summary>
        private void DrawShaderTrail()
        {
            if (!CodaShaderLoader.HasSwingArc) return;

            try
            {
                _trailRenderer ??= new CodaTrailRenderer();

                // Linearize the circular buffer into a contiguous position array
                Vector2[] linearPositions = new Vector2[trailPositions.Length];
                int validCount = 0;
                for (int i = 0; i < trailPositions.Length; i++)
                {
                    int actualIndex = (trailIndex - i + trailPositions.Length) % trailPositions.Length;
                    if (trailPositions[actualIndex] == Vector2.Zero) continue;
                    linearPositions[validCount++] = trailPositions[actualIndex];
                }

                if (validCount < 3) return;

                // Get the swing arc shader and configure it
                MiscShaderData shader = GameShaders.Misc["MagnumOpus:CodaSwingArc"];
                shader.UseColor(CodaUtils.CodaCrimson.ToVector3());
                shader.UseSecondaryColor(CodaUtils.CodaPurple.ToVector3());
                shader.Shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 3f);
                shader.Shader.Parameters["uOpacity"]?.SetValue(0.7f);
                shader.Shader.Parameters["uIntensity"]?.SetValue(1.5f);

                // Bind Voronoi noise texture if available
                if (_noiseTex?.Value != null)
                    shader.UseImage1(_noiseTex);

                // Use the SwingTrail preset with our shader
                CodaTrailSettings settings = CodaTrailSettings.SwingTrail(shader);

                // Resize to valid count
                Vector2[] validPositions = new Vector2[validCount];
                Array.Copy(linearPositions, validPositions, validCount);

                _trailRenderer.RenderTrail(validPositions, settings, validCount);
            }
            catch { }
        }

        public override void OnKill(int timeLeft)
        {
            // Final burst of particles
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color burstColor = Color.Lerp(CodaUtils.CodaPink, CodaUtils.AnnihilationWhite, Main.rand.NextFloat());
                CodaParticleHandler.SpawnParticle(new CosmicMoteParticle(
                    Projectile.Center, burstVel, burstColor * 0.7f, 0.3f, 18));
            }

            // Annihilation flare
            CodaParticleHandler.SpawnParticle(new AnnihilationFlareParticle(
                Projectile.Center, CodaUtils.CodaPurple * 0.5f, 0.4f, 12));
        }
    }
}
