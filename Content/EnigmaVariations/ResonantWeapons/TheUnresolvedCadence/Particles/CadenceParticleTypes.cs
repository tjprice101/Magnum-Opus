using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheUnresolvedCadence.Utilities;
using ReLogic.Content;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheUnresolvedCadence.Particles
{
    // =========================================================================
    //  VOID CLEAVE PARTICLE 窶・Dark slash afterimage fragments shattering off
    //  the blade during VoidCleave phase. Additive, 2-layer stretched glow,
    //  velocity-squished, deep purple core
    // =========================================================================
    public class VoidCleaveParticle : CadenceParticle
    {
        private readonly float _baseScale;
        private readonly float _stretchMultiplier;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public VoidCleaveParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
            _baseScale = scale;
            _stretchMultiplier = Main.rand.NextFloat(1.5f, 3.0f);
            Rotation = velocity.ToRotation();
        }

        public override void Update()
        {
            Velocity *= 0.93f;
            Position += Velocity;
            Rotation = Velocity.ToRotation();
            Scale = _baseScale * (1f - LifetimeCompletion * 0.7f);
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            float alpha = MathF.Pow(1f - LifetimeCompletion, 0.6f);
            if (alpha <= 0f) return;

            var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            Vector2 drawPos = Position - Main.screenPosition;

            // Velocity-squished stretching 竜・ afterimage slash fragments (capped to 300px on 2160px texture)
            float speed = MathHelper.Clamp(Velocity.Length() * 0.15f, 1f, _stretchMultiplier);
            Vector2 outerScale = new(Scale * speed * 2.0f, Scale * 0.6f);
            float cleaveCap = outerScale.X > 0.139f ? 0.139f / outerScale.X : 1f;
            outerScale *= cleaveCap;
            Vector2 innerScale = new Vector2(Scale * speed, Scale * 0.35f) * cleaveCap;

            // Layer 1: Outer dark violet halo 窶・the void's afterimage
            sb.Draw(tex, drawPos, null, Color * alpha * 0.4f, Rotation, tex.Size() / 2f, outerScale, SpriteEffects.None, 0f);
            // Layer 2: Brighter core 窶・the slash's trailing edge
            sb.Draw(tex, drawPos, null, CadenceUtils.CadenceViolet * alpha * 0.7f, Rotation, tex.Size() / 2f, innerScale, SpriteEffects.None, 0f);
        }
    }

    // =========================================================================
    //  PARADOX SLASH RIPPLE 窶・Rippling distortion particles spawned during
    //  ParadoxSlash phase. Reality tearing apart as concentric rings expand
    //  outward with pulsing opacity. AlphaBlend, Halo1 texture
    // =========================================================================
    public class ParadoxSlashRipple : CadenceParticle
    {
        private readonly float _baseScale;
        private readonly float _expandRate;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true; // SoftCircle has black bg
        public override bool UseCustomDraw => true;

        public ParadoxSlashRipple(Vector2 position, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = Vector2.Zero;
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
            _baseScale = scale;
            _expandRate = Main.rand.NextFloat(2.5f, 4.5f);
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            // Expand outward like a reality distortion ring
            Scale = _baseScale + LifetimeCompletion * _expandRate;
            Rotation += 0.008f;
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            // Pulsing opacity 窶・the distortion flickers between dimensions
            float pulse = MathF.Sin(Time * 0.25f) * 0.15f + 0.85f;
            float fade = CadenceUtils.SineBump(LifetimeCompletion);
            float alpha = fade * pulse * 0.55f;
            if (alpha <= 0.01f) return;

            var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle", AssetRequestMode.ImmediateLoad).Value;
            Vector2 drawPos = Position - Main.screenPosition;

            // The distortion ring 窶・reality bending outward (capped to 300px on 2160px SoftCircle)
            sb.Draw(tex, drawPos, null, Color * alpha, Rotation, tex.Size() / 2f, MathHelper.Min(Scale, 0.139f), SpriteEffects.None, 0f);
        }
    }

    // =========================================================================
    //  DIMENSIONAL RIFT MOTE 窶・Tiny bright motes scattered at dimensional
    //  slash impact points. Green-to-purple gradient. Additive, 2-layer
    //  with SparkleFlare1
    // =========================================================================
    public class DimensionalRiftMote : CadenceParticle
    {
        private readonly float _baseScale;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public DimensionalRiftMote(Vector2 position, Vector2 velocity, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            // Gradient from dimensional green to cadence violet based on random seed
            float colorT = Main.rand.NextFloat();
            Color = Color.Lerp(CadenceUtils.DimensionalGreen, CadenceUtils.CadenceViolet, colorT);
            Scale = scale;
            Lifetime = lifetime;
            _baseScale = scale;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            Velocity *= 0.94f;
            Position += Velocity;
            Rotation += 0.03f;
            Scale = _baseScale * (1f - LifetimeCompletion);
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            float alpha = 1f - LifetimeCompletion;
            if (alpha <= 0f) return;

            var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Pixel/PartiGlow", AssetRequestMode.ImmediateLoad).Value;
            Vector2 drawPos = Position - Main.screenPosition;

            // Layer 1: Soft wide glow 窶・the rift's ambient energy
            sb.Draw(tex, drawPos, null, Color * alpha * 0.35f, Rotation, tex.Size() / 2f, Scale * 2.0f, SpriteEffects.None, 0f);
            // Layer 2: Bright compact core 窶・the rift's hot center
            sb.Draw(tex, drawPos, null, Color * alpha * 0.85f, Rotation, tex.Size() / 2f, Scale * 0.6f, SpriteEffects.None, 0f);
        }
    }

    // =========================================================================
    //  INEVITABILITY GLYPH PARTICLE 窶・Orbiting glyphs that accumulate around
    //  the player as Inevitability stacks build. Orbit radius shrinks as stacks
    //  increase (the paradox tightens). AlphaBlend, Glyph textures, pulsing
    // =========================================================================
    public class InevitabilityGlyphParticle : CadenceParticle
    {
        private static readonly string[] NoteTextureNames = new[]
        {
            "MusicNote", "CursiveMusicNote", "MusicNoteWithSlashes",
            "QuarterNote", "TallMusicNote", "WholeNote"
        };

        private Vector2 _orbitCenter;
        private float _orbitAngle;
        private readonly float _baseOrbitRadius;
        private readonly float _orbitSpeed;
        private readonly int _glyphVariant;
        private readonly int _inevitabilityStacks;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true; // SoftRadialBloom halo has black bg
        public override bool UseCustomDraw => true;

        public InevitabilityGlyphParticle(Vector2 orbitCenter, float orbitRadius, float startAngle, int stacks, Color color, float scale, int lifetime)
        {
            _orbitCenter = orbitCenter;
            _baseOrbitRadius = orbitRadius;
            _orbitAngle = startAngle;
            _inevitabilityStacks = stacks;
            _orbitSpeed = Main.rand.NextFloat(0.02f, 0.05f) * (Main.rand.NextBool() ? 1 : -1);
            _glyphVariant = Main.rand.Next(NoteTextureNames.Length);
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
            Position = orbitCenter + _orbitAngle.ToRotationVector2() * orbitRadius;
        }

        /// <summary>Update the orbit center (follow the player).</summary>
        public void SetOrbitCenter(Vector2 center) => _orbitCenter = center;

        public override void Update()
        {
            _orbitAngle += _orbitSpeed;
            // Orbit radius shrinks as stacks increase 窶・the paradox tightens
            float radiusMult = MathHelper.Lerp(1f, 0.35f, _inevitabilityStacks / 10f);
            float currentRadius = _baseOrbitRadius * radiusMult;
            Position = _orbitCenter + _orbitAngle.ToRotationVector2() * currentRadius;
            Rotation += 0.02f;
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            // Pulsing visibility 窶・glyphs throb with inevitability
            float pulse = CadenceUtils.SineBump(LifetimeCompletion);
            float flicker = MathF.Sin(Time * 0.15f) * 0.2f + 0.8f;
            float alpha = pulse * flicker;
            if (alpha <= 0.01f) return;

            string glyphPath = $"MagnumOpus/Assets/Particles Asset Library/{NoteTextureNames[_glyphVariant]}";
            var tex = ModContent.Request<Texture2D>(glyphPath, AssetRequestMode.ImmediateLoad).Value;
            Vector2 drawPos = Position - Main.screenPosition;

            // Soft glow halo behind the glyph 窶・inevitability's aura
            var glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            sb.Draw(glowTex, drawPos, null, Color * alpha * 0.2f, 0f, glowTex.Size() / 2f, MathHelper.Min(Scale * 3.0f, 0.139f), SpriteEffects.None, 0f);
            // The glyph itself
            sb.Draw(tex, drawPos, null, Color * alpha, Rotation, tex.Size() / 2f, Scale, SpriteEffects.None, 0f);
        }
    }

    // =========================================================================
    //  PARADOX COLLAPSE FLASH 窶・Massive flash burst when Paradox Collapse
    //  triggers (10 stacks). Additive, expanding 3-layer bloom (enormous
    //  scale 2-8), screen-filling white竊恥urple fade
    // =========================================================================
    public class ParadoxCollapseFlash : CadenceParticle
    {
        private readonly float _baseScale;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public ParadoxCollapseFlash(Vector2 position, float scale, int lifetime)
        {
            Position = position;
            Velocity = Vector2.Zero;
            Color = Color.White;
            Scale = scale;
            Lifetime = lifetime;
            _baseScale = scale;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            // Expand rapidly then slow 窶・implosion-into-explosion
            float expand = CadenceUtils.ExpOut(LifetimeCompletion);
            Scale = _baseScale + expand * 6f;
            Rotation += 0.005f;
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            float progress = LifetimeCompletion;
            float alpha = MathF.Pow(1f - progress, 1.5f);
            if (alpha <= 0f) return;

            // Color: starts white, fades through paradox white into cadence violet
            Color flashColor = Color.Lerp(Color.White, CadenceUtils.CadenceViolet, progress * 0.8f);

            var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            Vector2 drawPos = Position - Main.screenPosition;

            // Layer 1: Enormous outer bloom 竜・ the paradox collapsing outward (capped to 300px on 2160px texture)
            sb.Draw(tex, drawPos, null, flashColor * alpha * 0.2f, Rotation, tex.Size() / 2f, MathHelper.Min(Scale * 1.0f, 0.139f), SpriteEffects.None, 0f);
            // Layer 2: Mid bloom 竜・ the shockwave
            sb.Draw(tex, drawPos, null, CadenceUtils.ParadoxWhite * alpha * 0.45f, Rotation * 0.7f, tex.Size() / 2f, MathHelper.Min(Scale * 0.55f, 0.139f), SpriteEffects.None, 0f);
            // Layer 3: White-hot core 竜・ the singularity point
            sb.Draw(tex, drawPos, null, Color.White * alpha * 0.7f, Rotation * 0.3f, tex.Size() / 2f, MathHelper.Min(Scale * 0.15f, 0.139f), SpriteEffects.None, 0f);
        }
    }
}