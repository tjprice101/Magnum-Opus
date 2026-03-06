using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;
using Terraria.GameContent;
using ReLogic.Content;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.EnigmaVariations.Debuffs;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.FugueOfTheUnknown.Particles;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.FugueOfTheUnknown.Dusts;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.FugueOfTheUnknown.Utilities;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.FugueOfTheUnknown.Primitives;
using MagnumOpus.Content.EnigmaVariations;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.FugueOfTheUnknown
{
    /// <summary>
    /// FUGUE OF THE UNKNOWN — Magic orbit-and-release weapon (Enigma Variations theme).
    /// A fugue — multiple voices weaving independently, then converging.
    /// 
    /// Left-click spawns orbiting voice projectiles (max 5, progressive positioning).
    /// Right-click releases all voices with homing + spiral toward nearest enemies.
    /// Hits apply EchoMark stacks; at 5 marks triggers Harmonic Convergence:
    ///   5x damage to primary target, 3x chain damage to all marked enemies within 400 units.
    /// 
    /// Custom Shaders: FugueVoiceTrail.fx, FugueConvergence.fx
    /// Foundation: SparkleProjectileFoundation + MaskFoundation + RibbonFoundation planned
    /// </summary>
    public class FugueOfTheUnknown : ModItem
    {
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        public override string Texture => "MagnumOpus/Content/EnigmaVariations/ResonantWeapons/FugueOfTheUnknown/FugueOfTheUnknown";
        
        public override void SetDefaults()
        {
            Item.damage = 252;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 12;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(gold: 20);
            Item.rare = ModContent.RarityType<EnigmaRarity>();
            Item.UseSound = SoundID.Item8;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<FugueVoiceProjectile>();
            Item.shootSpeed = 1f;
            Item.noMelee = true;
        }
        
        public override void HoldItem(Player player)
        {
            // Count active voices
            int voiceCount = 0;
            foreach (Projectile proj in Main.ActiveProjectiles)
            {
                if (proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<FugueVoiceProjectile>() && proj.ai[0] == 0)
                    voiceCount++;
            }
            
            Lighting.AddLight(player.Center, EnigmaPurple.ToVector3() * (0.1f + voiceCount * 0.05f));
        }
        
        public override bool AltFunctionUse(Player player) => true;
        
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Right click: release all voices
                ReleaseAllVoices(player);
                return false;
            }
            return true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Left-click spawns orbiting voice projectiles around you (max 5)"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Right-click releases all voices — they spiral and home toward enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Hits apply EchoMark stacks on enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "At 5 EchoMark stacks, triggers Harmonic Convergence — 5x damage to the target"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Convergence chains 3x damage to all Echo-marked enemies in range"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Five voices. One question. No answer.'")
            {
                OverrideColor = EnigmaPurple
            });
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Count existing orbiting voices
            int voiceCount = 0;
            foreach (Projectile proj in Main.ActiveProjectiles)
            {
                if (proj.owner == player.whoAmI && proj.type == type && proj.ai[0] == 0)
                    voiceCount++;
            }
            
            // Max 5 voices
            if (voiceCount >= 5)
            {
                ReleaseAllVoices(player);
                return false;
            }
            
            Projectile.NewProjectile(source, player.Center, Vector2.Zero, type, damage, knockback, player.whoAmI, 0f, voiceCount);
            SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.3f + voiceCount * 0.15f }, player.Center);
            
            return false;
        }
        
        private void ReleaseAllVoices(Player player)
        {
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 0.7f }, player.Center);
            
            foreach (Projectile voice in Main.ActiveProjectiles)
            {
                if (voice.owner != player.whoAmI || voice.type != ModContent.ProjectileType<FugueVoiceProjectile>() || voice.ai[0] != 0)
                    continue;
                    
                voice.ai[0] = 1; // Switch to released state
                
                // Find nearest enemy
                float bestDist = 800f;
                NPC bestTarget = null;
                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (npc.friendly) continue;
                    float dist = Vector2.Distance(voice.Center, npc.Center);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        bestTarget = npc;
                    }
                }
                
                if (bestTarget != null)
                {
                    voice.velocity = (bestTarget.Center - voice.Center).SafeNormalize(Vector2.UnitX) * 14f;
                }
                else
                {
                    voice.velocity = (Main.MouseWorld - voice.Center).SafeNormalize(Vector2.UnitX) * 14f;
                }
            }
        }
    }
    
    public class FugueVoiceProjectile : ModProjectile
    {
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        private readonly List<Vector2> _trailPositions = new(30);
        
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/MusicNote";
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = MathF.Sin((float)Main.GameUpdateCount * 0.08f + Projectile.ai[1]) * 0.5f + 0.5f;

            var bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            var glyphTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/MusicNote", AssetRequestMode.ImmediateLoad).Value;
            var starFlareTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Enigma/Impact Effects/EN Star Flare", AssetRequestMode.ImmediateLoad).Value;
            var powerRingTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Enigma/Impact Effects/EN Power Effect Ring", AssetRequestMode.ImmediateLoad).Value;
            Vector2 bloomOrigin = bloomTex.Size() / 2f;
            Vector2 glyphOrigin = glyphTex.Size() / 2f;
            Vector2 starFlareOrigin = starFlareTex.Size() / 2f;
            Vector2 powerRingOrigin = powerRingTex.Size() / 2f;

            // ═══════════════════════════════════════════════════════
            //  LAYER 1: GPU PRIMITIVE — orbit aura ring or release trail
            // ═══════════════════════════════════════════════════════
            if (Projectile.ai[0] == 0f)
            {
                // ORBITING STATE: Small undulating spectral ring around voice
                try
                {
                    sb.End();
                    int ringPts = 20;
                    float auraRadius = 14f + pulse * 4f;
                    var voiceRing = new List<Vector2>(ringPts + 1);
                    for (int i = 0; i <= ringPts; i++)
                    {
                        float a = (float)i / ringPts * MathHelper.TwoPi;
                        float wobble = 1f + 0.12f * MathF.Sin(a * 4f + Main.GameUpdateCount * 0.07f + Projectile.ai[1] * 2f);
                        voiceRing.Add(Projectile.Center + new Vector2(MathF.Cos(a), MathF.Sin(a)) * auraRadius * wobble);
                    }

                    if (ShaderLoader.FugueVoiceTrail != null)
                    {
                        var bodySettings = new FuguePrimitiveSettings(
                            widthFunction: c => (6f + 3f * MathF.Sin(c * MathHelper.TwoPi * 3f + Main.GameUpdateCount * 0.04f)) * (0.7f + pulse * 0.3f),
                            colorFunction: c => Color.Lerp(FugueUtils.VoicePurple, FugueUtils.EchoTeal, c) * (0.5f + pulse * 0.2f),
                            shader: ShaderLoader.FugueVoiceTrail);
                        FuguePrimitiveRenderer.RenderTrail(voiceRing, bodySettings);
                    }

                    if (ShaderLoader.FugueVoiceTrail != null)
                    {
                        var glowSettings = new FuguePrimitiveSettings(
                            widthFunction: c => (12f + pulse * 4f) * (0.5f + 0.5f * MathF.Sin(c * MathHelper.TwoPi * 2f)),
                            colorFunction: c => FugueUtils.DeepChorus * 0.3f,
                            shader: ShaderLoader.FugueVoiceTrail);
                        FuguePrimitiveRenderer.RenderTrail(voiceRing, glowSettings);
                    }

                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                        DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
                catch
                {
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                        DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }
            else
            {
                // RELEASED STATE: GPU primitive trail from recorded positions
                if (_trailPositions.Count > 2)
                {
                    try
                    {
                        sb.End();

                        if (ShaderLoader.FugueConvergence != null)
                        {
                            var bodySettings = new FuguePrimitiveSettings(
                                widthFunction: c => MathHelper.Lerp(12f, 2f, c),
                                colorFunction: c => Color.Lerp(FugueUtils.FugueCyan, FugueUtils.VoicePurple, c) * (0.7f - c * 0.4f),
                                shader: ShaderLoader.FugueConvergence);
                            FuguePrimitiveRenderer.RenderTrail(_trailPositions, bodySettings);
                        }

                        if (ShaderLoader.FugueConvergence != null)
                        {
                            var glowSettings = new FuguePrimitiveSettings(
                                widthFunction: c => MathHelper.Lerp(22f, 5f, c),
                                colorFunction: c => FugueUtils.DeepChorus * (0.3f - c * 0.2f),
                                shader: ShaderLoader.FugueConvergence);
                            FuguePrimitiveRenderer.RenderTrail(_trailPositions, glowSettings);
                        }

                        if (ShaderLoader.FugueConvergence != null)
                        {
                            var coreSettings = new FuguePrimitiveSettings(
                                widthFunction: c => MathHelper.Lerp(4f, 1f, c),
                                colorFunction: c => Color.Lerp(FugueUtils.HarmonicWhite, FugueUtils.FugueCyan, c) * (0.6f - c * 0.4f),
                                shader: ShaderLoader.FugueConvergence);
                            FuguePrimitiveRenderer.RenderTrail(_trailPositions, coreSettings);
                        }

                        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                            DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                    }
                    catch
                    {
                        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                            DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                    }
                }
            }

            // ═══════════════════════════════════════════════════════
            //  LAYER 2: SHADER OVERLAY — polyphonic voice spectrum
            // ═══════════════════════════════════════════════════════
            {
                Effect voiceShader = Projectile.ai[0] == 0f ? ShaderLoader.FugueVoiceTrail : ShaderLoader.FugueConvergence;
                string technique = Projectile.ai[0] == 0f ? "FugueVoiceFlow" : "FugueConvergenceWave";
                EnigmaShaderHelper.DrawShaderOverlay(sb, voiceShader,
                    bloomTex, drawPos, bloomOrigin, 1.2f + pulse * 0.3f,
                    FugueUtils.VoicePurple.ToVector3(), FugueUtils.EchoTeal.ToVector3(),
                    opacity: 0.5f, intensity: 1.1f,
                    noiseTexture: ShaderLoader.GetNoiseTexture("MusicalWavePattern"),
                    techniqueName: technique);
            }

            // ═══════════════════════════════════════════════════════
            //  LAYER 3: 6-LAYER BLOOM STACK — polyphonic radiance
            // ═══════════════════════════════════════════════════════
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            float scaleBase = Projectile.ai[0] == 0f ? 0.5f : 0.7f;
            float bloomPulse = scaleBase + pulse * 0.15f;

            // [0] ShadowVoid — outermost haze
            sb.Draw(bloomTex, drawPos, null, FugueUtils.ShadowVoid * 0.15f, 0f,
                bloomOrigin, bloomPulse * 3f, SpriteEffects.None, 0f);
            // [1] DeepChorus — deep purple glow
            sb.Draw(bloomTex, drawPos, null, FugueUtils.DeepChorus * 0.25f, 0f,
                bloomOrigin, bloomPulse * 2.2f, SpriteEffects.None, 0f);
            // [2] VoicePurple — the melodic line
            sb.Draw(bloomTex, drawPos, null, FugueUtils.VoicePurple * (0.35f + pulse * 0.15f), 0f,
                bloomOrigin, bloomPulse * 1.5f, SpriteEffects.None, 0f);
            // [3] EchoTeal — the answer phrase
            sb.Draw(bloomTex, drawPos, null, FugueUtils.EchoTeal * 0.45f, 0f,
                bloomOrigin, bloomPulse * 1f, SpriteEffects.None, 0f);
            // [4] FugueCyan — voices converging
            sb.Draw(bloomTex, drawPos, null, FugueUtils.FugueCyan * (0.25f + pulse * 0.15f), 0f,
                bloomOrigin, bloomPulse * 0.55f, SpriteEffects.None, 0f);
            // [5] HarmonicWhite — pure harmonic core
            sb.Draw(bloomTex, drawPos, null, FugueUtils.HarmonicWhite * (0.4f + pulse * 0.2f), 0f,
                bloomOrigin, bloomPulse * 0.25f, SpriteEffects.None, 0f);

            // ═══════════════════════════════════════════════════════
            //  LAYER 4: THEME TEXTURES — Enigma identity
            // ═══════════════════════════════════════════════════════
            // EN Star Flare — dual counter-rotating
            float flareRotA = (float)Main.GameUpdateCount * 0.02f + Projectile.ai[1] * 1.5f;
            float flareRotB = -(float)Main.GameUpdateCount * 0.015f + Projectile.ai[1] * 2.1f;
            float flareScale = (Projectile.ai[0] == 0f ? 0.2f : 0.3f) + pulse * 0.08f;
            sb.Draw(starFlareTex, drawPos, null, FugueUtils.VoicePurple * (0.4f + pulse * 0.15f), flareRotA,
                starFlareOrigin, flareScale, SpriteEffects.None, 0f);
            sb.Draw(starFlareTex, drawPos, null, FugueUtils.EchoTeal * (0.3f + pulse * 0.1f), flareRotB,
                starFlareOrigin, flareScale * 0.8f, SpriteEffects.None, 0f);

            // EN Power Effect Ring — concentric spectral ring
            float ringRot = (float)Main.GameUpdateCount * 0.035f;
            float ringScale = (Projectile.ai[0] == 0f ? 0.15f : 0.25f) + pulse * 0.06f;
            sb.Draw(powerRingTex, drawPos, null, FugueUtils.EchoTeal * (0.25f + pulse * 0.12f), ringRot,
                powerRingOrigin, ringScale, SpriteEffects.None, 0f);
            sb.Draw(powerRingTex, drawPos, null, FugueUtils.DeepChorus * 0.18f, -ringRot * 0.7f,
                powerRingOrigin, ringScale * 1.4f, SpriteEffects.None, 0f);

            // EN Enigma Eye — materializes in released state when voices are converging
            if (Projectile.ai[0] != 0f)
            {
                Texture2D enigmaEye = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Enigma/Particles/EN Enigma Eye", AssetRequestMode.ImmediateLoad).Value;
                float eyePulse = 0.7f + 0.3f * MathF.Sin(Main.GameUpdateCount * 0.06f);
                sb.Draw(enigmaEye, drawPos, null, FugueUtils.HarmonicWhite * eyePulse * 0.4f, 0f,
                    enigmaEye.Size() / 2f, flareScale * 0.6f * eyePulse, SpriteEffects.None, 0f);
            }

            // Glyph sprite — the voice's musical identity
            Color glyphColor = Projectile.ai[0] == 0f
                ? Color.Lerp(FugueUtils.VoicePurple, FugueUtils.HarmonicWhite, pulse * 0.5f) * (0.5f + pulse * 0.5f)
                : Color.White * 0.9f;
            sb.Draw(glyphTex, drawPos, null, glyphColor, Projectile.rotation, glyphOrigin,
                Projectile.ai[0] == 0f ? 0.6f : 0.7f, SpriteEffects.None, 0f);

            // ═══════════════════════════════════════════════════════
            //  LAYER 5: THEME ACCENTS — ambient pulsing light
            // ═══════════════════════════════════════════════════════
            EnigmaVFXLibrary.AddPulsingLight(Projectile.Center, FugueUtils.VoicePurple, 0.4f, 0.3f + pulse * 0.3f);
            EnigmaVFXLibrary.AddPulsingLight(Projectile.Center, FugueUtils.EchoTeal, 0.3f, 0.2f + pulse * 0.2f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 9999;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }
        
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }
        
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            if (Projectile.ai[0] == 0)
            {
                // ORBITING STATE
                Projectile.timeLeft = 600;
                
                float orbitIndex = Projectile.ai[1];
                float orbitSpeed = 0.03f;
                float orbitRadius = 80f + orbitIndex * 20f;
                float angle = (float)Main.GameUpdateCount * orbitSpeed + orbitIndex * MathHelper.TwoPi / 5f;
                
                Vector2 targetPos = owner.Center + new Vector2(
                    (float)Math.Cos(angle) * orbitRadius,
                    (float)Math.Sin(angle) * orbitRadius
                );
                
                Projectile.Center = Vector2.Lerp(Projectile.Center, targetPos, 0.1f);
                Projectile.rotation += 0.05f;

                // --- Orbiting VFX ---
                if (Main.GameUpdateCount % 4 == 0)
                {
                    Vector2 outwardDrift = (Projectile.Center - owner.Center).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.3f, 0.8f);
                    FugueParticleHandler.Spawn(new VoiceWispParticle(
                        Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                        outwardDrift,
                        FugueUtils.VoicePurple,
                        Main.rand.NextFloat(0.3f, 0.5f),
                        Main.rand.Next(30, 50)
                    ));
                }

                // Echo dust every frame
                Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    ModContent.DustType<FugueEchoDust>(),
                    Main.rand.NextVector2Circular(0.4f, 0.4f),
                    0, default, Main.rand.NextFloat(0.3f, 0.6f)
                );
            }
            else
            {
                // RELEASED STATE - fly toward enemies with homing
                // Record position for GPU primitive trail
                _trailPositions.Add(Projectile.Center);
                if (_trailPositions.Count > 25)
                    _trailPositions.RemoveAt(0);

                float homingRange = 500f;
                float bestDist = homingRange;
                NPC bestTarget = null;
                
                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (npc.friendly) continue;
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        bestTarget = npc;
                    }
                }
                
                if (bestTarget != null)
                {
                    Vector2 toTarget = (bestTarget.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 14f, 0.04f);
                }
                
                // Spiral motion
                Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.ToRadians(2f));
                
                Projectile.rotation += 0.1f;
                Lighting.AddLight(Projectile.Center, EnigmaGreen.ToVector3() * 0.3f);
                
                // Eventually die
                if (Projectile.timeLeft > 300)
                    Projectile.timeLeft = 300;

                // --- Released VFX: trail motes every frame ---
                Color trailColor = Color.Lerp(FugueUtils.EchoTeal, FugueUtils.VoicePurple, Main.rand.NextFloat());
                FugueParticleHandler.Spawn(new FugueTrailMote(
                    Projectile.Center + Main.rand.NextVector2Circular(3f, 3f),
                    -Projectile.velocity * Main.rand.NextFloat(0.1f, 0.25f),
                    trailColor,
                    Main.rand.NextFloat(0.2f, 0.4f),
                    Main.rand.Next(15, 30)
                ));

                // Echo dust every 3 frames
                if (Main.GameUpdateCount % 3 == 0)
                {
                    Dust.NewDustPerfect(
                        Projectile.Center,
                        ModContent.DustType<FugueEchoDust>(),
                        -Projectile.velocity * 0.05f,
                        0, default, Main.rand.NextFloat(0.3f, 0.5f)
                    );
                }
            }
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Echo Mark
            target.AddBuff(ModContent.BuffType<EchoMark>(), 480);
            target.GetGlobalNPC<EchoMarkNPC>().AddEchoStack(target);
            
            // Apply Paradox Brand (8 seconds per doc)
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 480);
            target.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(target, 1);
            
            // Check for Harmonic Convergence
            int echoStacks = target.GetGlobalNPC<EchoMarkNPC>().echoStacks;
            if (echoStacks >= 5)
            {
                TriggerHarmonicConvergence(target);
            }
            
            Lighting.AddLight(target.Center, EnigmaPurple.ToVector3() * 0.6f);

            // Impact VFX burst on every hit
            int impactMotes = Main.rand.Next(3, 6);
            for (int i = 0; i < impactMotes; i++)
            {
                Vector2 impactVel = Main.rand.NextVector2CircularEdge(3f, 3f) * Main.rand.NextFloat(0.4f, 1.0f);
                Color impactCol = Color.Lerp(FugueUtils.EchoTeal, FugueUtils.VoicePurple, Main.rand.NextFloat());
                FugueParticleHandler.Spawn(new FugueTrailMote(
                    target.Center + Main.rand.NextVector2Circular(6f, 6f),
                    impactVel,
                    impactCol,
                    Main.rand.NextFloat(0.2f, 0.4f),
                    Main.rand.Next(15, 30)
                ));
            }

            // Convergence flash on hit
            FugueParticleHandler.Spawn(new ConvergenceFlashParticle(
                target.Center,
                FugueUtils.EchoTeal,
                Main.rand.NextFloat(0.8f, 1.2f),
                15
            ));
        }
        
        private void TriggerHarmonicConvergence(NPC target)
        {
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.5f, Volume = 0.8f }, target.Center);
            SoundEngine.PlaySound(SoundID.Item162 with { Pitch = 0.3f, Volume = 0.6f }, target.Center);
            
            // Reset echo stacks
            target.GetGlobalNPC<EchoMarkNPC>().echoStacks = 0;
            
            // Core damage to primary target
            target.SimpleStrikeNPC(Projectile.damage * 5, 0, false, 0f, null, false, 0f, true);
            
            // Find all marked enemies and chain lightning damage
            float chainRange = 400f;
            List<NPC> markedEnemies = new List<NPC>();
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly || npc.whoAmI == target.whoAmI) continue;
                if (Vector2.Distance(npc.Center, target.Center) > chainRange) continue;
                if (npc.HasBuff(ModContent.BuffType<EchoMark>()))
                {
                    markedEnemies.Add(npc);
                }
            }
            
            foreach (NPC marked in markedEnemies)
            {
                marked.SimpleStrikeNPC(Projectile.damage * 3, 0, false, 0f, null, false, 0f, true);
                marked.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(marked, 3);
                marked.GetGlobalNPC<EchoMarkNPC>().echoStacks = 0;
            }

            // ======= HARMONIC CONVERGENCE VFX =======
            // NOTE: This runs during OnHitNPC (update phase), NOT draw phase.
            // All VFX must use particle spawns — Main.spriteBatch.Draw() would crash here.

            // Convergence ring flash particles (replacing direct spriteBatch draws)
            FugueParticleHandler.Spawn(new ConvergenceFlashParticle(
                target.Center,
                FugueUtils.VoicePurple,
                Main.rand.NextFloat(1.5f, 2.0f),
                30
            ));
            FugueParticleHandler.Spawn(new ConvergenceFlashParticle(
                target.Center,
                FugueUtils.EchoTeal,
                Main.rand.NextFloat(2.0f, 2.5f),
                35
            ));

            // Convergence star flare particles (replacing direct spriteBatch draws)
            FugueParticleHandler.Spawn(new ConvergenceFlashParticle(
                target.Center,
                FugueUtils.HarmonicWhite,
                Main.rand.NextFloat(1.2f, 1.6f),
                25
            ));
            FugueParticleHandler.Spawn(new ConvergenceFlashParticle(
                target.Center,
                FugueUtils.FugueCyan,
                Main.rand.NextFloat(1.0f, 1.4f),
                25
            ));

            // Large flash at primary target — all voices resolving in unison
            FugueParticleHandler.Spawn(new ConvergenceFlashParticle(
                target.Center,
                FugueUtils.HarmonicWhite,
                Main.rand.NextFloat(2.0f, 3.0f),
                35
            ));

            // Burst of trail motes outward from target
            int burstCount = Main.rand.Next(20, 30);
            for (int i = 0; i < burstCount; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2CircularEdge(5f, 5f) * Main.rand.NextFloat(0.6f, 1.3f);
                Color burstCol = Color.Lerp(FugueUtils.FugueCyan, FugueUtils.HarmonicWhite, Main.rand.NextFloat(0.3f, 0.8f));
                FugueParticleHandler.Spawn(new FugueTrailMote(
                    target.Center + Main.rand.NextVector2Circular(8f, 8f),
                    burstVel,
                    burstCol,
                    Main.rand.NextFloat(0.3f, 0.5f),
                    Main.rand.Next(25, 45)
                ));
            }

            // Expanding voice wisps from target
            int wispCount = Main.rand.Next(5, 9);
            for (int i = 0; i < wispCount; i++)
            {
                Vector2 wispVel = Main.rand.NextVector2CircularEdge(1.5f, 1.5f) * Main.rand.NextFloat(0.4f, 0.8f);
                FugueParticleHandler.Spawn(new VoiceWispParticle(
                    target.Center + Main.rand.NextVector2Circular(10f, 10f),
                    wispVel,
                    FugueUtils.VoicePurple,
                    Main.rand.NextFloat(0.4f, 0.7f),
                    Main.rand.Next(40, 60)
                ));
            }

            // Chain VFX: flash at each marked enemy + connecting trail motes
            foreach (NPC marked in markedEnemies)
            {
                // Flash at chain target
                FugueParticleHandler.Spawn(new ConvergenceFlashParticle(
                    marked.Center,
                    FugueUtils.FugueCyan,
                    Main.rand.NextFloat(1.0f, 1.5f),
                    25
                ));

                // Trail motes along the chain line between target and marked enemy
                int chainMotes = Main.rand.Next(5, 9);
                for (int j = 0; j < chainMotes; j++)
                {
                    float t = (j + 1f) / (chainMotes + 1f);
                    Vector2 linePos = Vector2.Lerp(target.Center, marked.Center, t) + Main.rand.NextVector2Circular(4f, 4f);
                    Vector2 lineVel = (marked.Center - target.Center).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.3f, 0.8f);
                    FugueParticleHandler.Spawn(new FugueTrailMote(
                        linePos,
                        lineVel,
                        Color.Lerp(FugueUtils.EchoTeal, FugueUtils.HarmonicWhite, t),
                        Main.rand.NextFloat(0.2f, 0.4f),
                        Main.rand.Next(20, 35)
                    ));
                }
            }
        }
        
        public override void OnKill(int timeLeft)
        {
            // Death burst: scattered trail motes
            int moteCount = Main.rand.Next(10, 16);
            for (int i = 0; i < moteCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(4f, 4f) * Main.rand.NextFloat(0.5f, 1.4f);
                Color col = Color.Lerp(FugueUtils.EchoTeal, FugueUtils.VoicePurple, Main.rand.NextFloat());
                FugueParticleHandler.Spawn(new FugueTrailMote(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    vel,
                    col,
                    Main.rand.NextFloat(0.25f, 0.5f),
                    Main.rand.Next(20, 45)
                ));
            }

            // Voice wisp burst on death
            for (int i = 0; i < Main.rand.Next(2, 4); i++)
            {
                Vector2 wispVel = Main.rand.NextVector2CircularEdge(1.5f, 1.5f) * Main.rand.NextFloat(0.3f, 0.7f);
                FugueParticleHandler.Spawn(new VoiceWispParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    wispVel,
                    FugueUtils.EchoTeal,
                    Main.rand.NextFloat(0.3f, 0.6f),
                    Main.rand.Next(30, 50)
                ));
            }

            // Central convergence flash
            FugueParticleHandler.Spawn(new ConvergenceFlashParticle(
                Projectile.Center,
                FugueUtils.FugueCyan,
                1.5f,
                30
            ));

            // Echo dust burst
            for (int i = 0; i < Main.rand.Next(5, 8); i++)
            {
                Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    ModContent.DustType<FugueEchoDust>(),
                    Main.rand.NextVector2Circular(2f, 2f),
                    0, default, Main.rand.NextFloat(0.5f, 1.0f)
                );
            }
        }
    }
    
    /// <summary>
    /// Echo Mark debuff - applied by Fugue of the Unknown voices,
    /// stacks build toward Harmonic Convergence
    /// </summary>
    public class EchoMark : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_24";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }
    
    /// <summary>
    /// Tracks Echo Mark stacks on NPCs for Fugue of the Unknown
    /// </summary>
    public class EchoMarkNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        
        public int echoStacks = 0;
        
        public void AddEchoStack(NPC npc)
        {
            echoStacks++;
        }
        
        public override void ResetEffects(NPC npc)
        {
            if (!npc.HasBuff(ModContent.BuffType<EchoMark>()))
            {
                echoStacks = 0;
            }
        }
        
        public override void PostAI(NPC npc)
        {
            if (echoStacks <= 0) return;

            // Pulsing echo mark glyph above the enemy's head
            if (Main.GameUpdateCount % 10 == 0)
            {
                float markScale = 0.3f + echoStacks * 0.1f;
                FugueParticleHandler.Spawn(new EchoMarkParticle(
                    npc.Top - Vector2.UnitY * 20f,
                    12f,
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    FugueUtils.VoicePurple,
                    markScale,
                    60,
                    echoStacks
                ));
            }

            // At 3+ stacks, intensified echo dust
            if (echoStacks >= 3 && Main.GameUpdateCount % 5 == 0)
            {
                Dust.NewDustPerfect(
                    npc.Center + Main.rand.NextVector2Circular(npc.width * 0.4f, npc.height * 0.4f),
                    ModContent.DustType<FugueEchoDust>(),
                    Main.rand.NextVector2Circular(0.5f, 0.5f),
                    0, default, Main.rand.NextFloat(0.4f, 0.7f)
                );
            }
        }
    }
}
