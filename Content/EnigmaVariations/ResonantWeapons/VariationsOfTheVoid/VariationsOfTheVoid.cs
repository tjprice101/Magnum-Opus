using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.EnigmaVariations.Debuffs;
using MagnumOpus.Content.FoundationWeapons.LaserFoundation;
using Terraria.GameContent;
using Terraria.Graphics;
using ReLogic.Content;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.VariationsOfTheVoid.Particles;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.VariationsOfTheVoid.Dusts;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.VariationsOfTheVoid.Utilities;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.VariationsOfTheVoid
{
    /// <summary>
    /// VARIATIONS OF THE VOID - Projectile classes (VoidConvergenceBeamSet and VoidResonanceExplosion)
    /// VoidConvergenceBeamSet: Channeled multi-beam attack that converges to a point
    /// VoidResonanceExplosion: Explosion triggered when beams align
    /// </summary>
    public class VoidConvergenceBeamSet : ModProjectile
    {
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private const int BeamCount = 3;
        private const float MaxBeamLength = 600f;
        private float[] beamLengths = new float[BeamCount];
        private int channelTime = 0;
        private Dictionary<int, int> targetHitTimes = new Dictionary<int, int>();

        // ConvergenceBeamShader pipeline (LaserFoundation pattern)
        private Effect beamShader;
        private float flareRotation;
        
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/MusicNote";
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            Player owner = Main.player[Projectile.owner];
            Vector2 toCursorDraw = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            float baseAngle = toCursorDraw.ToRotation();
            float convergenceProgress = Math.Min(channelTime / 120f, 1f);
            float startConeAngle = MathHelper.ToRadians(30f);
            float currentConeAngle = startConeAngle * (1f - convergenceProgress);
            float intensityRamp = MathHelper.Clamp(channelTime / 20f, 0f, 1f);

            // ── LOAD & CONFIGURE CONVERGENCE BEAM SHADER ──
            if (beamShader == null)
            {
                beamShader = ModContent.Request<Effect>(
                    "MagnumOpus/Content/FoundationWeapons/LaserFoundation/Shaders/ConvergenceBeamShader",
                    AssetRequestMode.ImmediateLoad).Value;
            }

            beamShader.Parameters["WorldViewProjection"].SetValue(
                Main.GameViewMatrix.NormalizedTransformationmatrix);
            beamShader.Parameters["onTex"].SetValue(LFTextures.BeamAlphaMask.Value);
            beamShader.Parameters["gradientTex"].SetValue(LFTextures.GradEnigma.Value);
            beamShader.Parameters["baseColor"].SetValue(Color.White.ToVector3());
            beamShader.Parameters["satPower"].SetValue(0.8f);

            beamShader.Parameters["sampleTexture1"].SetValue(LFTextures.DetailThinGlowLine.Value);
            beamShader.Parameters["sampleTexture2"].SetValue(LFTextures.DetailSpark.Value);
            beamShader.Parameters["sampleTexture3"].SetValue(LFTextures.DetailExtra.Value);
            beamShader.Parameters["sampleTexture4"].SetValue(LFTextures.DetailTrailLoop.Value);

            beamShader.Parameters["grad1Speed"].SetValue(0.66f);
            beamShader.Parameters["grad2Speed"].SetValue(0.66f);
            beamShader.Parameters["grad3Speed"].SetValue(1.03f);
            beamShader.Parameters["grad4Speed"].SetValue(0.77f);

            beamShader.Parameters["tex1Mult"].SetValue(1.25f);
            beamShader.Parameters["tex2Mult"].SetValue(1.5f);
            beamShader.Parameters["tex3Mult"].SetValue(1.15f);
            beamShader.Parameters["tex4Mult"].SetValue(2.5f);
            beamShader.Parameters["totalMult"].SetValue(intensityRamp);

            beamShader.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * -0.024f);

            // ── END SPRITEBATCH FOR RAW GPU DRAWS ──
            sb.End();

            // ── DRAW EACH BEAM BODY WITH VERTEXSTRIP ──
            for (int b = 0; b < BeamCount; b++)
            {
                float offset = (b - (BeamCount - 1) / 2f) * currentConeAngle / Math.Max(BeamCount - 1, 1);
                float beamAngle = baseAngle + offset;
                float beamLen = beamLengths[b];
                Vector2 beamDir = new Vector2((float)Math.Cos(beamAngle), (float)Math.Sin(beamAngle));
                Vector2 beamStart = owner.Center;
                Vector2 beamEnd = beamStart + beamDir * beamLen;

                float dist = beamLen;
                float repVal = dist / 2000f;
                beamShader.Parameters["gradientReps"].SetValue(0.75f * repVal);
                beamShader.Parameters["tex1reps"].SetValue(1.15f * repVal);
                beamShader.Parameters["tex2reps"].SetValue(1.15f * repVal);
                beamShader.Parameters["tex3reps"].SetValue(1.15f * repVal);
                beamShader.Parameters["tex4reps"].SetValue(1.15f * repVal);

                // Build multi-point strip for smooth tapered beam
                const int segCount = 20;
                Vector2[] positions = new Vector2[segCount];
                float[] rotations = new float[segCount];
                for (int s = 0; s < segCount; s++)
                {
                    float t = s / (float)(segCount - 1);
                    positions[s] = Vector2.Lerp(beamStart, beamEnd, t);
                    rotations[s] = beamAngle;
                }
                float baseWidth = 80f * intensityRamp;

                VertexStrip strip = new VertexStrip();
                strip.PrepareStrip(positions, rotations,
                    (float p) => Color.White,
                    (float p) => baseWidth * (0.3f + 0.7f * MathF.Sin(p * MathHelper.Pi)),
                    -Main.screenPosition, includeBacksides: true);

                beamShader.CurrentTechnique.Passes["MainPS"].Apply();
                strip.DrawTrail();
            }

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

            // ── ENDPOINT FLARES (Enigma-themed direct colors) ──
            flareRotation += 1.15f;

            Texture2D lensFlare = LFTextures.LensFlare.Value;
            Texture2D starFlare = LFTextures.StarFlare.Value;
            Texture2D glowOrb = LFTextures.GlowOrb.Value;
            Texture2D softGlow = LFTextures.SoftGlow.Value;

            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.EffectMatrix);

            float pulse = 1f + 0.1f * MathF.Sin(Main.GameUpdateCount * 0.15f);

            // Enigma-themed endpoint colors
            Color tipOuter = VoidVariationUtils.VariationViolet * (0.5f * intensityRamp);
            Color tipStar = VoidVariationUtils.RiftTeal * (0.45f * intensityRamp);
            Color baseOuter = VoidVariationUtils.AbyssPurple * (0.4f * intensityRamp);
            Color baseStar = VoidVariationUtils.VoidSurge * (0.35f * intensityRamp);

            // Per-beam tip flares (capped to 300px on 1024px textures)
            for (int b = 0; b < BeamCount; b++)
            {
                float bOffset = (b - (BeamCount - 1) / 2f) * currentConeAngle / Math.Max(BeamCount - 1, 1);
                float bAngle = baseAngle + bOffset;
                Vector2 bDir = new Vector2((float)Math.Cos(bAngle), (float)Math.Sin(bAngle));
                Vector2 tipDraw = owner.Center + bDir * beamLengths[b] - Main.screenPosition;

                sb.Draw(glowOrb, tipDraw, null, tipOuter * pulse, flareRotation * 0.1f,
                    glowOrb.Size() / 2f, MathHelper.Min(0.3f * intensityRamp * pulse, 0.2f), SpriteEffects.None, 0f);
                sb.Draw(starFlare, tipDraw, null, tipStar, flareRotation * 0.05f,
                    starFlare.Size() / 2f, MathHelper.Min(0.25f * intensityRamp, 0.18f), SpriteEffects.None, 0f);
                sb.Draw(starFlare, tipDraw, null, VoidVariationUtils.SunderingWhite * (0.25f * intensityRamp), -flareRotation * 0.08f,
                    starFlare.Size() / 2f, MathHelper.Min(0.15f * intensityRamp, 0.12f), SpriteEffects.None, 0f);
            }

            // Base flare at player origin
            Vector2 baseDraw = owner.Center - Main.screenPosition;
            Vector2 sigilScale = new Vector2(0.2f, 1f) * 0.4f * intensityRamp;
            // Cap sigilScale max dimension to 250px on 1024px textures
            float sigilMax = Math.Max(sigilScale.X, sigilScale.Y);
            if (sigilMax > 0.244f)
            {
                float sigilCap = 0.244f / sigilMax;
                sigilScale *= sigilCap;
            }
            sb.Draw(softGlow, baseDraw, null, baseOuter, baseAngle,
                softGlow.Size() / 2f, sigilScale, SpriteEffects.None, 0f);
            sb.Draw(starFlare, baseDraw, null, baseStar, baseAngle,
                starFlare.Size() / 2f, sigilScale, SpriteEffects.None, 0f);

            // Convergence point glow when nearly aligned (>70%)
            if (convergenceProgress > 0.7f)
            {
                Vector2 convergePt = owner.Center + toCursorDraw * Math.Min(beamLengths[0], MaxBeamLength) - Main.screenPosition;
                float convergeIntensity = (convergenceProgress - 0.7f) / 0.3f;

                sb.Draw(glowOrb, convergePt, null, VoidVariationUtils.VoidSurge * (0.45f * convergeIntensity * pulse), flareRotation * 0.1f,
                    glowOrb.Size() / 2f, MathHelper.Min(0.4f * convergeIntensity * pulse, 0.2f), SpriteEffects.None, 0f);
                sb.Draw(lensFlare, convergePt, null, VoidVariationUtils.VariationViolet * (0.35f * convergeIntensity), flareRotation * 0.02f,
                    lensFlare.Size() / 2f, MathHelper.Min(0.35f * convergeIntensity, 0.2f), SpriteEffects.None, 0f);
                sb.Draw(starFlare, convergePt, null, VoidVariationUtils.RiftTeal * (0.4f * convergeIntensity * pulse), flareRotation * 0.05f,
                    starFlare.Size() / 2f, MathHelper.Min(0.35f * convergeIntensity * pulse, 0.2f), SpriteEffects.None, 0f);
                sb.Draw(softGlow, convergePt, null, VoidVariationUtils.SunderingWhite * (0.3f * convergeIntensity), 0f,
                    softGlow.Size() / 2f, MathHelper.Min(0.45f * convergeIntensity, 0.2f), SpriteEffects.None, 0f);
            }

            // Theme texture accents
            VoidVariationUtils.DrawThemeAccents(sb, Projectile.Center, 1f, 0.6f);
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
        
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 9999;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 6;
            
            for (int i = 0; i < BeamCount; i++)
                beamLengths[i] = MaxBeamLength;
        }
        
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            // Check if still channeling
            if (!owner.channel || owner.dead || !owner.active)
            {
                TriggerBeamEnd();
                Projectile.Kill();
                return;
            }
            
            channelTime++;
            
            // Position at player
            Projectile.Center = owner.Center;
            
            // Aim toward cursor
            Vector2 toCursor = (Main.MouseWorld - owner.Center).SafeNormalize(Vector2.UnitX);
            Projectile.velocity = toCursor;
            Projectile.rotation = toCursor.ToRotation();
            
            // Convergence progress: beams start spread and converge over time
            float convergenceProgress = Math.Min(channelTime / 120f, 1f);
            float startConeAngle = MathHelper.ToRadians(30f);
            float currentConeAngle = startConeAngle * (1f - convergenceProgress);
            
            // Check if fully aligned
            bool isAligned = convergenceProgress >= 0.95f;
            
            // Calculate beam angles
            float baseAngle = toCursor.ToRotation();
            float[] beamAngles = new float[BeamCount];
            for (int i = 0; i < BeamCount; i++)
            {
                float offset = (i - (BeamCount - 1) / 2f) * currentConeAngle / (BeamCount - 1);
                beamAngles[i] = baseAngle + offset;
            }
            
            // Raycast each beam for tile collision
            for (int b = 0; b < BeamCount; b++)
            {
                Vector2 beamDir = new Vector2((float)Math.Cos(beamAngles[b]), (float)Math.Sin(beamAngles[b]));
                beamLengths[b] = MaxBeamLength;
                
                for (int i = 0; i < (int)(MaxBeamLength / 16f); i++)
                {
                    Vector2 checkPos = owner.Center + beamDir * (i * 16f);
                    Point tilePos = checkPos.ToTileCoordinates();
                    if (WorldGen.InWorld(tilePos.X, tilePos.Y))
                    {
                        Tile tile = Main.tile[tilePos.X, tilePos.Y];
                        if (tile.HasTile && Main.tileSolid[tile.TileType])
                        {
                            beamLengths[b] = i * 16f;
                            break;
                        }
                    }
                }
                
                // Deal beam damage
                Vector2 beamEnd = owner.Center + beamDir * beamLengths[b];
                DealBeamDamage(owner.Center, beamEnd, beamDir, beamLengths[b], isAligned);
                
                Lighting.AddLight(beamEnd, EnigmaGreen.ToVector3() * 0.3f);
            }
            
            // Beam sound
            if (channelTime % 30 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item15 with { Pitch = -0.2f + convergenceProgress * 0.5f, Volume = 0.3f }, owner.Center);
            }
            
            // Keep player facing the right direction
            owner.ChangeDir(toCursor.X > 0 ? 1 : -1);
            owner.heldProj = Projectile.whoAmI;
            owner.itemTime = 2;
            owner.itemAnimation = 2;
            
            Lighting.AddLight(owner.Center, EnigmaPurple.ToVector3() * 0.3f);

            // === Trail particles ===
            // Every 2 frames: TriBeamConvergenceMote along beam
            if (channelTime % 2 == 0)
            {
                VoidVariationParticleHandler.Spawn(new TriBeamConvergenceMote(
                    owner.Center + toCursor * Main.rand.NextFloat(50f, 200f),
                    toCursor, VoidVariationUtils.RiftTeal,
                    Main.rand.NextFloat(0.1f, 0.2f), Main.rand.Next(20, 35)));
            }

            // Every 3 frames: VoidWhisperMote drifting from beam
            if (channelTime % 3 == 0)
            {
                float randomOffset = Main.rand.NextFloat(-0.3f, 0.3f);
                float beamAngleRand = toCursor.ToRotation() + randomOffset;
                Vector2 beamPos = owner.Center + new Vector2((float)Math.Cos(beamAngleRand), (float)Math.Sin(beamAngleRand)) * Main.rand.NextFloat(60f, MaxBeamLength * 0.6f);
                VoidVariationParticleHandler.Spawn(new VoidWhisperMote(
                    beamPos, Main.rand.NextVector2Circular(1f, 1f), VoidVariationUtils.AbyssPurple,
                    Main.rand.NextFloat(0.08f, 0.14f), Main.rand.Next(25, 40)));
            }

            // Every 4 frames: VoidVariationDust
            if (channelTime % 4 == 0)
            {
                Vector2 dustPos = owner.Center + toCursor * Main.rand.NextFloat(30f, 150f);
                Dust.NewDustPerfect(dustPos, ModContent.DustType<VoidVariationDust>(),
                    Main.rand.NextVector2Circular(1.5f, 1.5f), 0, default, Main.rand.NextFloat(0.4f, 0.7f));
            }
        }
        
        private void DealBeamDamage(Vector2 start, Vector2 end, Vector2 direction, float beamLength, bool isAligned)
        {
            float alignmentMultiplier = isAligned ? 2.5f : 1f;
            
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly) continue;
                
                float distToLine = DistancePointToLine(npc.Center, start, end);
                if (distToLine > npc.width / 2f + 16f) continue;
                
                float projectionLength = Vector2.Dot(npc.Center - start, direction);
                if (projectionLength < 0 || projectionLength > beamLength) continue;
                
                // Track hit frequency per target
                if (!targetHitTimes.ContainsKey(npc.whoAmI))
                    targetHitTimes[npc.whoAmI] = 0;
                targetHitTimes[npc.whoAmI]++;
                
                if (targetHitTimes[npc.whoAmI] % 6 == 0)
                {
                    int damage = (int)(Projectile.damage * alignmentMultiplier);
                    npc.SimpleStrikeNPC(damage, 0, false, 0f, null, false, 0f, true);
                    npc.AddBuff(ModContent.BuffType<ParadoxBrand>(), 480);
                    npc.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(npc, isAligned ? 2 : 1);
                }
            }
        }
        
        private float DistancePointToLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            Vector2 line = lineEnd - lineStart;
            float lineLength = line.Length();
            if (lineLength < 0.001f) return Vector2.Distance(point, lineStart);
            
            Vector2 lineDir = line / lineLength;
            Vector2 toPoint = point - lineStart;
            float projection = Vector2.Dot(toPoint, lineDir);
            projection = MathHelper.Clamp(projection, 0f, lineLength);
            
            Vector2 closestPoint = lineStart + lineDir * projection;
            return Vector2.Distance(point, closestPoint);
        }
        
        private void TriggerBeamEnd()
        {
            Player owner = Main.player[Projectile.owner];
            float convergenceProgress = Math.Min(channelTime / 120f, 1f);
            bool isAligned = convergenceProgress >= 0.95f;
            
            if (isAligned)
            {
                // Fully aligned: spawn massive explosion at convergence point
                Vector2 toCursor = (Main.MouseWorld - owner.Center).SafeNormalize(Vector2.UnitX);
                Vector2 convergencePoint = owner.Center + toCursor * Math.Min(beamLengths[0], MaxBeamLength);
                
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.5f, Volume = 0.9f }, convergencePoint);
                
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), convergencePoint, Vector2.Zero,
                    ModContent.ProjectileType<VoidResonanceExplosion>(),
                    (int)(Projectile.damage * 3f), 10f, Projectile.owner);
            }
            else
            {
                // Not aligned: just a sound
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 0.5f }, owner.Center);
            }
        }
        
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => false;
        
        public override void OnKill(int timeLeft)
        {
            // 3-5 TriBeamConvergenceMote burst
            Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            for (int i = 0; i < Main.rand.Next(3, 6); i++)
            {
                Vector2 burstDir = dir.RotatedByRandom(MathHelper.Pi);
                VoidVariationParticleHandler.Spawn(new TriBeamConvergenceMote(
                    Projectile.Center, burstDir, VoidVariationUtils.RiftTeal,
                    Main.rand.NextFloat(0.15f, 0.25f), Main.rand.Next(20, 30)));
            }

            // 1 AbyssalEchoRing expanding
            VoidVariationParticleHandler.Spawn(new AbyssalEchoRing(
                Projectile.Center, VoidVariationUtils.RiftTeal, 0.2f, 30));

            // 2-3 VoidVariationDust
            for (int i = 0; i < Main.rand.Next(2, 4); i++)
            {
                Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f),
                    ModContent.DustType<VoidVariationDust>(),
                    Main.rand.NextVector2Circular(3f, 3f), 0, default, Main.rand.NextFloat(0.5f, 0.8f));
            }
        }
    }
    
    public class VoidResonanceExplosion : ModProjectile
    {
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom";
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float lifeProgress = 1f - (Projectile.timeLeft / 30f);
            float intensity = (float)Math.Sin(lifeProgress * MathHelper.Pi);
            float pulse = 1f + 0.08f * MathF.Sin(Main.GameUpdateCount * 0.2f);

            Texture2D bloom = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            Texture2D pixel = MagnumTextureRegistry.GetPointBloom();

            // === Shader overlay: Voronoi cell fracture explosion ===
            EnigmaShaderHelper.DrawShaderOverlay(sb, ShaderLoader.VoidSwingTrail,
                bloom, drawPos, bloom.Size() / 2f, 0.10f * intensity + 0.03f,
                VoidVariationUtils.VariationViolet.ToVector3(), VoidVariationUtils.VoidSurge.ToVector3(),
                opacity: 0.6f * intensity, intensity: 1.3f,
                noiseTexture: ShaderLoader.GetNoiseTexture("VoronoiNoise"),
                techniqueName: "VoidVariationSwingGlow");

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            // Layer 1: Large outer blast glow — VariationViolet, fading
            float outerScale = (0.065f + 0.025f * lifeProgress) * pulse;
            float outerAlpha = intensity * 0.3f;
            sb.Draw(bloom, drawPos, null, VoidVariationUtils.VariationViolet * outerAlpha, 0f,
                bloom.Size() / 2f, outerScale, SpriteEffects.None, 0f);

            // Layer 2: EN Power Effect Ring — rotating resonance field boundary
            Texture2D powerRing = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Enigma/Impact Effects/EN Power Effect Ring", AssetRequestMode.ImmediateLoad).Value;
            Texture2D starFlare = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Enigma/Impact Effects/EN Star Flare", AssetRequestMode.ImmediateLoad).Value;
            float ringRot = Main.GameUpdateCount * 0.02f;
            sb.Draw(powerRing, drawPos, null, VoidVariationUtils.RiftTeal * 0.45f * intensity, ringRot,
                powerRing.Size() / 2f, outerScale * 0.4f, SpriteEffects.None, 0f);
            sb.Draw(powerRing, drawPos, null, VoidVariationUtils.VariationViolet * 0.3f * intensity, -ringRot * 0.6f,
                powerRing.Size() / 2f, outerScale * 0.3f, SpriteEffects.None, 0f);

            // Layer 3: Inner core — VoidSurge/SunderingWhite, brighter
            Color coreColor = Color.Lerp(VoidVariationUtils.VoidSurge, VoidVariationUtils.SunderingWhite,
                MathF.Sin(Main.GameUpdateCount * 0.15f) * 0.5f + 0.5f);
            sb.Draw(bloom, drawPos, null, coreColor * intensity * 0.55f, 0f,
                bloom.Size() / 2f, outerScale * 0.5f * pulse, SpriteEffects.None, 0f);

            // Layer 4: White-hot center — SunderingWhite
            sb.Draw(bloom, drawPos, null, VoidVariationUtils.SunderingWhite * intensity * 0.7f, 0f,
                bloom.Size() / 2f, outerScale * 0.2f, SpriteEffects.None, 0f);

            // Layer 5: EN Star Flare — spinning starburst at resonance center
            float starRot = Main.GameUpdateCount * 0.035f;
            sb.Draw(starFlare, drawPos, null, VoidVariationUtils.VoidSurge * 0.5f * intensity, starRot,
                starFlare.Size() / 2f, outerScale * 0.35f, SpriteEffects.None, 0f);
            sb.Draw(starFlare, drawPos, null, VoidVariationUtils.RiftTeal * 0.3f * intensity, -starRot * 1.2f,
                starFlare.Size() / 2f, outerScale * 0.25f, SpriteEffects.None, 0f);

            // 4 rotating X-shaped beams — cosmic explosion arms
            float beamLength = 180f * intensity;
            float beamWidth = 6f * intensity;
            float rot = Main.GameUpdateCount * 0.03f;
            for (int arm = 0; arm < 4; arm++)
            {
                float armAngle = rot + arm * MathHelper.PiOver2 + MathHelper.PiOver4 * 0.5f;
                Color armColor = arm % 2 == 0 ? VoidVariationUtils.VariationViolet : VoidVariationUtils.RiftTeal;
                sb.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), armColor * 0.45f * intensity,
                    armAngle, new Vector2(0.5f, 0.5f), new Vector2(beamLength, beamWidth), SpriteEffects.None, 0f);
                // Thinner inner line
                sb.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), VoidVariationUtils.SunderingWhite * 0.25f * intensity,
                    armAngle, new Vector2(0.5f, 0.5f), new Vector2(beamLength * 0.8f, beamWidth * 0.4f), SpriteEffects.None, 0f);
            }
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
        
        public override void SetDefaults()
        {
            Projectile.width = 100;
            Projectile.height = 100;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 30;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        
        public override void AI()
        {
            float lifeProgress = 1f - (Projectile.timeLeft / 30f);
            
            // Expanding hitbox
            int size = (int)(100 + 200 * lifeProgress);
            Projectile.width = size;
            Projectile.height = size;
            Projectile.Center = Projectile.position + new Vector2(Projectile.width / 2f, Projectile.height / 2f);
            
            Lighting.AddLight(Projectile.Center, EnigmaGreen.ToVector3() * (1f - lifeProgress) * 0.8f);

            // === Particles ===
            // Every frame: 2-3 RiftSunderSpark in random directions
            for (int i = 0; i < Main.rand.Next(2, 4); i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(6f, 6f) * Main.rand.NextFloat(0.8f, 1.5f);
                VoidVariationParticleHandler.Spawn(new RiftSunderSpark(
                    Projectile.Center + Main.rand.NextVector2Circular(15f, 15f),
                    sparkVel, Main.rand.NextFloat(0.15f, 0.3f), Main.rand.Next(10, 20)));
            }

            // Every 2 frames: TriBeamConvergenceMote spiraling outward
            if (Projectile.timeLeft % 2 == 0)
            {
                float spiralAngle = Main.GameUpdateCount * 0.2f;
                Vector2 spiralDir = new Vector2(MathF.Cos(spiralAngle), MathF.Sin(spiralAngle));
                VoidVariationParticleHandler.Spawn(new TriBeamConvergenceMote(
                    Projectile.Center, spiralDir, VoidVariationUtils.RiftTeal,
                    Main.rand.NextFloat(0.1f, 0.2f), Main.rand.Next(15, 25)));
            }

            // Every 3 frames: VoidVariationDust
            if (Projectile.timeLeft % 3 == 0)
            {
                Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(30f, 30f),
                    ModContent.DustType<VoidVariationDust>(),
                    Main.rand.NextVector2Circular(3f, 3f), 0, default, Main.rand.NextFloat(0.5f, 0.9f));
            }
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 480);
            target.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(target, 3);

            // === VFX: AbyssalEchoRing at target ===
            VoidVariationParticleHandler.Spawn(new AbyssalEchoRing(
                target.Center, VoidVariationUtils.VariationViolet, 0.25f, 25));

            // === VFX: 2-3 RiftSunderSpark at target ===
            for (int sp = 0; sp < Main.rand.Next(2, 4); sp++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(5f, 5f);
                VoidVariationParticleHandler.Spawn(new RiftSunderSpark(
                    target.Center, sparkVel, Main.rand.NextFloat(0.15f, 0.25f), Main.rand.Next(12, 20)));
            }
        }
        
        public override void OnKill(int timeLeft)
        {
            // 1 large AbyssalEchoRing expanding — SunderingWhite
            VoidVariationParticleHandler.Spawn(new AbyssalEchoRing(
                Projectile.Center, VoidVariationUtils.SunderingWhite, 0.8f, 40));

            // 10-15 VoidWhisperMote drifting outward
            for (int i = 0; i < Main.rand.Next(10, 16); i++)
            {
                Vector2 driftVel = Main.rand.NextVector2CircularEdge(3f, 3f) * Main.rand.NextFloat(0.5f, 1.5f);
                VoidVariationParticleHandler.Spawn(new VoidWhisperMote(
                    Projectile.Center + Main.rand.NextVector2Circular(20f, 20f),
                    driftVel, VoidVariationUtils.VariationViolet,
                    Main.rand.NextFloat(0.1f, 0.2f), Main.rand.Next(30, 50)));
            }

            // 5-8 RiftSunderSpark burst
            for (int i = 0; i < Main.rand.Next(5, 9); i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(7f, 7f) * Main.rand.NextFloat(0.8f, 1.5f);
                VoidVariationParticleHandler.Spawn(new RiftSunderSpark(
                    Projectile.Center, sparkVel, Main.rand.NextFloat(0.2f, 0.35f), Main.rand.Next(15, 25)));
            }

            // 5-6 VoidVariationDust
            for (int i = 0; i < Main.rand.Next(5, 7); i++)
            {
                Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(30f, 30f),
                    ModContent.DustType<VoidVariationDust>(),
                    Main.rand.NextVector2Circular(4f, 4f), 0, default, Main.rand.NextFloat(0.6f, 1.0f));
            }
        }
    }
}
