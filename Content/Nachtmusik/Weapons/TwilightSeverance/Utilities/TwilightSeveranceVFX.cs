using System;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Common.Systems.VFX.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Nachtmusik.Weapons.TwilightSeverance.Utilities
{
    public static class TwilightSeveranceVFX
    {
        private static readonly Color DeepIndigo = new Color(40, 30, 100);
        private static readonly Color CosmicBlue = new Color(60, 80, 180);
        private static readonly Color StarlightSilver = new Color(180, 200, 230);
        private static readonly Color StellarWhite = new Color(240, 245, 255);

        public static void AmbientShimmer(Player player)
        {
            if (Main.dedServ)
                return;

            Lighting.AddLight(player.Center, CosmicBlue.ToVector3() * 0.18f);

            if (Main.rand.NextBool(5))
            {
                float t = (float)Main.timeForVisualEffects;
                float angle = t * 1.2f + Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = 20f + Main.rand.NextFloat(14f);
                Vector2 offset = new Vector2(player.direction * 14f, -22f);
                Vector2 orbitPos = player.Center + offset + angle.ToRotationVector2() * radius;
                Vector2 vel = new Vector2(-player.direction * 0.15f, -0.3f - Main.rand.NextFloat(0.2f));

                Color moteColor = Color.Lerp(CosmicBlue, StarlightSilver, Main.rand.NextFloat(0.4f, 1f));
                Dust d = Dust.NewDustPerfect(orbitPos, DustID.ShimmerSpark, vel, 0, moteColor, 0.45f);
                d.noGravity = true;
                d.fadeIn = 0.8f;
            }
        }

        public static void SwingImpactVFX(Vector2 hitPos, int comboStep, bool onBeat, Vector2 swingDirection)
        {
            int dustCount = 3 + comboStep;

            // Bloom flash: bright central dust that reads as a brief silver flash
            Dust flash = Dust.NewDustPerfect(hitPos, DustID.ShimmerSpark, Vector2.Zero, 0, StellarWhite, 1.3f + comboStep * 0.15f);
            flash.noGravity = true;
            flash.fadeIn = 0.2f;

            for (int i = 0; i < dustCount; i++)
            {
                Vector2 vel = swingDirection.RotatedByRandom(0.6f) * Main.rand.NextFloat(1.5f, 3.5f);
                Color c = Color.Lerp(StarlightSilver, StellarWhite, Main.rand.NextFloat(0.3f, 1f));
                Dust d = Dust.NewDustPerfect(hitPos, DustID.ShimmerSpark, vel, 0, c, 0.65f);
                d.noGravity = true;
            }

            Lighting.AddLight(hitPos, StarlightSilver.ToVector3() * (0.35f + comboStep * 0.06f));

            if (onBeat)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(1.8f, 1.8f);
                    Dust d = Dust.NewDustPerfect(hitPos, DustID.Enchanted_Gold, vel, 0, StellarWhite, 0.55f);
                    d.noGravity = true;
                }
            }
        }

        public static void ConstellationBreakTargetVFX(Vector2 pos, int stacksConsumed)
        {
            int dustCount = 6 + stacksConsumed * 3;
            for (int i = 0; i < dustCount; i++)
            {
                float t = i / (float)dustCount;
                Vector2 vel = (MathHelper.TwoPi * t).ToRotationVector2() * Main.rand.NextFloat(2.5f, 5.5f);
                Color c = Color.Lerp(CosmicBlue, StellarWhite, t);
                Dust d = Dust.NewDustPerfect(pos, DustID.ShimmerSpark, vel, 0, c, 0.85f);
                d.noGravity = true;
            }

            Lighting.AddLight(pos, StarlightSilver.ToVector3() * 0.5f);
        }

        public static void ConstellationBreakVFX(Vector2 playerCenter, int consumedCount)
        {
            try
            {
                global::MagnumOpus.Common.Systems.MagnumScreenEffects.AddScreenShake(
                    MathHelper.Clamp(4f + consumedCount * 0.5f, 4f, 8f));
            }
            catch { }
        }

        public static void DrawBreakSlash(Projectile proj)
        {
            if (Main.dedServ)
                return;

            SpriteBatch sb = Main.spriteBatch;
            Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
            if (glowTex == null)
                return;

            Vector2 origin = glowTex.Size() * 0.5f;
            float fadeOut = Utils.GetLerpValue(0f, 8f, proj.timeLeft, true);
            float fadeIn = Utils.GetLerpValue(30f, 24f, proj.timeLeft, true);
            float alpha = fadeIn * fadeOut;

            Vector2 pos = proj.Center - Main.screenPosition;
            float rotation = proj.velocity.ToRotation();
            float t = (float)Main.timeForVisualEffects;

            // Main pass: DimensionalRift shader on the core streak
            Effect riftShader = ShaderLoader.DimensionalRift;
            bool hasShader = riftShader != null;

            sb.End();

            if (hasShader)
            {
                sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                try
                {
                    riftShader.Parameters["uColor"]?.SetValue(CosmicBlue.ToVector3());
                    riftShader.Parameters["uSecondaryColor"]?.SetValue(StarlightSilver.ToVector3());
                    riftShader.Parameters["uTime"]?.SetValue(t);
                    riftShader.Parameters["uOpacity"]?.SetValue(alpha);
                    riftShader.Parameters["uIntensity"]?.SetValue(1.8f);
                    riftShader.Parameters["uOverbrightMult"]?.SetValue(2.5f);
                    riftShader.Parameters["uScrollSpeed"]?.SetValue(2f);
                    riftShader.Parameters["uDistortionAmt"]?.SetValue(0.04f);
                    riftShader.Parameters["uHasSecondaryTex"]?.SetValue(0f);
                    riftShader.Parameters["uSecondaryTexScale"]?.SetValue(1f);
                    riftShader.Parameters["uSecondaryTexScroll"]?.SetValue(0.5f);

                    riftShader.CurrentTechnique = riftShader.Techniques["DimensionalRiftSlash"];
                    riftShader.CurrentTechnique.Passes[0].Apply();
                }
                catch
                {
                    hasShader = false;
                }
            }
            else
            {
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }

            // Core streak with DimensionalRift shader (or plain additive if shader fails)
            Color outerGlow = NachtmusikPalette.Additive(CosmicBlue, 0.35f * alpha);
            sb.Draw(glowTex, pos, null, outerGlow, rotation,
                origin, new Vector2(0.5f, 0.13f) * proj.scale, SpriteEffects.None, 0f);

            Color coreGlow = NachtmusikPalette.Additive(StarlightSilver, 0.65f * alpha);
            sb.Draw(glowTex, pos, null, coreGlow, rotation,
                origin, new Vector2(0.28f, 0.055f) * proj.scale, SpriteEffects.None, 0f);

            Color hotCore = NachtmusikPalette.Additive(StellarWhite, 0.85f * alpha);
            sb.Draw(glowTex, pos, null, hotCore, rotation,
                origin, new Vector2(0.13f, 0.028f) * proj.scale, SpriteEffects.None, 0f);

            // Bloom pass (softer glow layer, no shader distortion)
            if (hasShader)
            {
                try
                {
                    riftShader.CurrentTechnique = riftShader.Techniques["DimensionalRiftGlow"];
                    riftShader.CurrentTechnique.Passes[0].Apply();
                }
                catch { }
            }

            Color bloomGlow = NachtmusikPalette.Additive(CosmicBlue, 0.18f * alpha);
            sb.Draw(glowTex, pos, null, bloomGlow, rotation,
                origin, new Vector2(0.7f, 0.2f) * proj.scale, SpriteEffects.None, 0f);

            // Trail afterimages (plain additive, no shader needed)
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            for (int i = 1; i < proj.oldPos.Length; i++)
            {
                if (proj.oldPos[i] == Vector2.Zero)
                    continue;

                Vector2 trailPos = proj.oldPos[i] + proj.Size * 0.5f - Main.screenPosition;
                float trailAlpha = alpha * (1f - i / (float)proj.oldPos.Length) * 0.5f;
                Color trailColor = NachtmusikPalette.Additive(CosmicBlue, trailAlpha);
                float trailScale = (1f - i / (float)proj.oldPos.Length) * 0.7f;
                sb.Draw(glowTex, trailPos, null, trailColor, proj.oldRot[i],
                    origin, new Vector2(0.22f * trailScale, 0.045f * trailScale) * proj.scale,
                    SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void DrawBreakMusicNote(Vector2 worldPos, int stacksConsumed)
        {
            if (Main.dedServ)
                return;

            Texture2D noteTex = MagnumTextureRegistry.GetQuarterNote();
            if (noteTex == null)
                return;

            SpriteBatch sb = Main.spriteBatch;
            Vector2 origin = noteTex.Size() * 0.5f;
            Vector2 pos = worldPos - Main.screenPosition + new Vector2(0, -18f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            float t = (float)Main.timeForVisualEffects;
            float noteScale = 0.35f + stacksConsumed * 0.08f;
            float noteRot = MathF.Sin(t * 2f) * 0.15f;
            Color noteColor = NachtmusikPalette.Additive(StarlightSilver, 0.7f);

            sb.Draw(noteTex, pos, null, noteColor, noteRot, origin, noteScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void DrawSwingAfterimages(SpriteBatch sb, Projectile proj, Vector2 tipPosition,
            int comboStep, float progression, Vector2 swordDirection, float bladeLength)
        {
            if (Main.dedServ || progression < 0.12f || progression > 0.92f)
                return;

            Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
            if (glowTex == null)
                return;

            Vector2 origin = glowTex.Size() * 0.5f;
            float t = (float)Main.timeForVisualEffects;

            int afterimageCount = comboStep switch
            {
                0 => 4,
                2 => 5,
                3 => 7,
                _ => 3
            };

            float swingFade = MathHelper.SmoothStep(0f, 1f,
                Math.Min(Utils.GetLerpValue(0.12f, 0.25f, progression, true),
                         Utils.GetLerpValue(0.92f, 0.78f, progression, true)));

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            for (int i = 0; i < afterimageCount; i++)
            {
                float iFrac = i / (float)afterimageCount;
                float ghostAlpha = (1f - iFrac) * 0.35f * swingFade;
                float ghostScale = (1f - iFrac * 0.5f) * 0.18f;

                Color ghostColor = Color.Lerp(StarlightSilver, DeepIndigo, iFrac);
                ghostColor.A = 0;
                ghostColor *= ghostAlpha;

                float lagOffset = iFrac * 0.08f;
                float ghostProg = MathHelper.Clamp(progression - lagOffset, 0f, 1f);

                Vector2 ghostTip = tipPosition - swordDirection * bladeLength * iFrac * 0.06f;
                Vector2 ghostScreen = ghostTip - Main.screenPosition;
                float ghostRot = swordDirection.ToRotation() + iFrac * 0.12f;

                sb.Draw(glowTex, ghostScreen, null, ghostColor, ghostRot,
                    origin, new Vector2(ghostScale * 2.5f, ghostScale * 0.6f), SpriteEffects.None, 0f);
            }

            float tipPulse = 0.9f + 0.1f * MathF.Sin(t * 6f + comboStep);
            Vector2 tipScreen = tipPosition - Main.screenPosition;
            float phaseIntensity = 1f + comboStep * 0.12f;

            Color outerColor = NachtmusikPalette.Additive(CosmicBlue, 0.2f * swingFade);
            sb.Draw(glowTex, tipScreen, null, outerColor, 0f, origin,
                0.22f * phaseIntensity * tipPulse, SpriteEffects.None, 0f);

            Color innerColor = NachtmusikPalette.Additive(StarlightSilver, 0.4f * swingFade);
            sb.Draw(glowTex, tipScreen, null, innerColor, 0f, origin,
                0.1f * phaseIntensity * tipPulse, SpriteEffects.None, 0f);

            Color coreColor = NachtmusikPalette.Additive(StellarWhite, 0.6f * swingFade);
            sb.Draw(glowTex, tipScreen, null, coreColor, 0f, origin,
                0.04f * phaseIntensity * tipPulse, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }

    public static class TwilightSeveranceShaderBridge
    {
        private static Asset<Effect> starTrailShader;

        private static void EnsureLoaded()
        {
            if (Main.dedServ)
                return;

            starTrailShader ??= ModContent.Request<Effect>(
                "MagnumOpus/Effects/Nachtmusik/NachtmusikStarTrail",
                AssetRequestMode.ImmediateLoad);
        }

        public static void DrawTipAura(SpriteBatch sb, Vector2 worldPos, int comboStep, float progression)
        {
            if (Main.dedServ || progression < 0.1f || progression > 0.9f)
                return;

            EnsureLoaded();

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 pos = worldPos - Main.screenPosition;
            float t = (float)Main.timeForVisualEffects;
            Color primary = Color.Lerp(new Color(40, 30, 100), new Color(60, 80, 180), progression);
            Color secondary = Color.Lerp(new Color(180, 200, 230), new Color(240, 245, 255), progression);

            float swingFade = Math.Min(
                Utils.GetLerpValue(0.1f, 0.2f, progression, true),
                Utils.GetLerpValue(0.9f, 0.8f, progression, true));

            try
            {
                sb.End();
                sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Effect effect = starTrailShader?.Value;
                if (effect != null)
                {
                    effect.Parameters["uColor"]?.SetValue(primary.ToVector3());
                    effect.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
                    effect.Parameters["uTime"]?.SetValue(t);
                    effect.Parameters["uOpacity"]?.SetValue(swingFade);
                    effect.Parameters["uIntensity"]?.SetValue(1.2f + comboStep * 0.12f);
                    effect.Parameters["uOverbrightMult"]?.SetValue(2.2f);
                    effect.Parameters["uScrollSpeed"]?.SetValue(1f);
                    effect.Parameters["uDistortionAmt"]?.SetValue(0.05f);
                    effect.Parameters["uHasSecondaryTex"]?.SetValue(0f);

                    if (effect.Techniques["NachtmusikStarGlow"] != null)
                    {
                        effect.CurrentTechnique = effect.Techniques["NachtmusikStarGlow"];
                        effect.CurrentTechnique.Passes[0].Apply();
                    }
                }

                float size = 20f + comboStep * 4f;
                sb.Draw(pixel, new Rectangle((int)(pos.X - size * 0.5f), (int)(pos.Y - size * 0.5f),
                    (int)size, (int)size), secondary * (0.3f * swingFade));
            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }
    }
}
