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
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheUnresolvedCadence.Particles;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheUnresolvedCadence.Dusts;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheUnresolvedCadence.Utilities;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheUnresolvedCadence
{
    /// <summary>
    /// THE UNRESOLVED CADENCE - Projectile classes (DimensionalSlash and ParadoxCollapseUltimate)
    /// DimensionalSlash: Fast-moving dimensional tear projectile spawned by the melee swing.
    /// Features shader-driven crack pattern, velocity-stretched trail, multi-layer bloom,
    /// and glitch-aesthetic particle effects matching the Enigma Variations theme.
    /// ParadoxCollapseUltimate: Large AOE implosion triggered by paradox stack threshold.
    /// </summary>
    public class DimensionalSlash : ModProjectile
    {
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private float slashAngle;
        private List<Vector2> slashTrail = new List<Vector2>();
        
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/ImpactEffects/ImpactEllipse";
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float lifeProgress = 1f - (Projectile.timeLeft / 30f);
            float alpha = 1f - lifeProgress;

            // === Shader overlay: Dimensional tear crack pattern ===
            {
                Texture2D shBloom = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
                EnigmaShaderHelper.DrawShaderOverlay(sb, ShaderLoader.CadenceSwingTrail,
                    shBloom, drawPos, shBloom.Size() / 2f, 1.5f,
                    CadenceUtils.CadenceViolet.ToVector3(), CadenceUtils.DimensionalGreen.ToVector3(),
                    opacity: 0.6f * alpha, intensity: 1.2f, rotation: Projectile.velocity.ToRotation(),
                    noiseTexture: ShaderLoader.GetNoiseTexture("RealityCrackPattern"),
                    techniqueName: "CadenceSwingGlow");
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            // Velocity-stretched energy streak — CadenceViolet outer + DimensionalGreen core
            Texture2D pixel = MagnumTextureRegistry.GetSoftGlow();
            if (pixel == null) return false;
            float rot = Projectile.velocity.ToRotation();
            float speed = Projectile.velocity.Length();
            Vector2 trailScale = new Vector2(MathHelper.Clamp(speed * 3f, 10f, 24f), 10f);
            sb.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), CadenceUtils.CadenceViolet * alpha * 0.7f,
                rot, new Vector2(0.5f, 0.5f), trailScale, SpriteEffects.None, 0f);
            // Inner bright core line
            sb.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), CadenceUtils.DimensionalGreen * alpha * 0.5f,
                rot, new Vector2(0.5f, 0.5f), new Vector2(trailScale.X * 0.7f, 4f), SpriteEffects.None, 0f);

            // Multi-layer bloom at center using EN Star Flare for Enigma theme identity
            Texture2D bloom = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            Texture2D starFlare = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Enigma/Impact Effects/EN Star Flare", AssetRequestMode.ImmediateLoad).Value;
            float pulse = 1f + 0.15f * MathF.Sin(Main.GameUpdateCount * 0.15f);
            
            // Layer 1: Wide DimensionalGreen ambient glow
            sb.Draw(bloom, drawPos, null, CadenceUtils.DimensionalGreen * alpha * 0.35f, 0f,
                bloom.Size() / 2f, 0.4f * pulse, SpriteEffects.None, 0f);
            // Layer 2: Medium CadenceViolet glow
            sb.Draw(bloom, drawPos, null, CadenceUtils.CadenceViolet * alpha * 0.3f, 0f,
                bloom.Size() / 2f, 0.25f * pulse, SpriteEffects.None, 0f);
            // Layer 3: Tight ParadoxWhite hot core
            sb.Draw(bloom, drawPos, null, CadenceUtils.ParadoxWhite * alpha * 0.25f, 0f,
                bloom.Size() / 2f, 0.12f * pulse, SpriteEffects.None, 0f);
            // Layer 4: Star flare overlay — rotating Enigma identity burst
            float starRot = Main.GameUpdateCount * 0.03f;
            sb.Draw(starFlare, drawPos, null, CadenceUtils.DimensionalGreen * alpha * 0.4f, starRot,
                starFlare.Size() / 2f, 0.25f * pulse, SpriteEffects.None, 0f);
            sb.Draw(starFlare, drawPos, null, CadenceUtils.CadenceViolet * alpha * 0.2f, -starRot * 0.7f,
                starFlare.Size() / 2f, 0.18f * pulse, SpriteEffects.None, 0f);

            // Theme texture accents
            CadenceUtils.DrawThemeAccents(sb, Projectile.Center, 1f, 0.6f);
            }
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
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 30;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }
        
        public override void AI()
        {
            float lifeProgress = 1f - (Projectile.timeLeft / 30f);
            
            // Record trail
            slashTrail.Add(Projectile.Center);
            if (slashTrail.Count > 15) slashTrail.RemoveAt(0);
            
            // Slow down over life
            Projectile.velocity *= 0.95f;
            
            slashAngle = Projectile.velocity.ToRotation();
            Projectile.rotation = slashAngle;
            
            Lighting.AddLight(Projectile.Center, EnigmaPurple.ToVector3() * (0.5f * (1f - lifeProgress)));

            // Particle VFX
            if (!Main.dedServ)
            {
                // Every frame: 1 DimensionalRiftMote trailing behind
                Vector2 backVel = -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 2f);
                CadenceParticleHandler.Spawn(new DimensionalRiftMote(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    backVel, Main.rand.NextFloat(0.15f, 0.3f), Main.rand.Next(12, 20)));

                // Every 3 frames: 1 CadenceRiftDust
                if ((30 - Projectile.timeLeft) % 3 == 0)
                {
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                        ModContent.DustType<CadenceRiftDust>(),
                        Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-1.5f, 1.5f));
                }
            }
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Paradox Brand (8 seconds per doc) + 2 stacks
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 480);
            target.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(target, 2);
            
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.6f);

            // Impact VFX: ripple + radial burst + glitch fragments
            if (!Main.dedServ)
            {
                // Impact ripple at target center
                CadenceParticleHandler.Spawn(new ParadoxSlashRipple(
                    target.Center, CadenceUtils.CadenceViolet, 0.3f, 25));
                // Second ripple, slightly delayed scale for layered effect
                CadenceParticleHandler.Spawn(new ParadoxSlashRipple(
                    target.Center, CadenceUtils.DimensionalGreen, 0.2f, 30));

                // 4-7 DimensionalRiftMotes radiating outward
                for (int i = 0; i < Main.rand.Next(4, 8); i++)
                {
                    Vector2 burstVel = Main.rand.NextVector2CircularEdge(4f, 4f) * Main.rand.NextFloat(0.5f, 1.5f);
                    CadenceParticleHandler.Spawn(new DimensionalRiftMote(
                        target.Center, burstVel, Main.rand.NextFloat(0.2f, 0.4f), Main.rand.Next(12, 22)));
                }

                // 2-3 VoidCleaveParticle — "digital debris" at impact point
                for (int i = 0; i < Main.rand.Next(2, 4); i++)
                {
                    Vector2 debrisVel = Main.rand.NextVector2CircularEdge(3f, 3f) * Main.rand.NextFloat(0.6f, 1.4f);
                    Color debrisColor = Main.rand.NextBool() ? CadenceUtils.SeveranceLime : CadenceUtils.DimensionalGreen;
                    CadenceParticleHandler.Spawn(new VoidCleaveParticle(
                        target.Center + Main.rand.NextVector2Circular(8f, 8f),
                        debrisVel, debrisColor, Main.rand.NextFloat(0.2f, 0.4f), Main.rand.Next(8, 15)));
                }
            }
        }
        
        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;

            // 5-8 DimensionalRiftMote burst
            for (int i = 0; i < Main.rand.Next(5, 9); i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(3f, 3f) * Main.rand.NextFloat(0.5f, 1.5f);
                CadenceParticleHandler.Spawn(new DimensionalRiftMote(
                    Projectile.Center, vel, Main.rand.NextFloat(0.2f, 0.45f), Main.rand.Next(15, 30)));
            }

            // 1 ParadoxSlashRipple expanding from center
            CadenceParticleHandler.Spawn(new ParadoxSlashRipple(
                Projectile.Center, CadenceUtils.DimensionalGreen, 0.35f, 30));

            // 2-4 VoidCleaveParticle for glitch debris
            for (int i = 0; i < Main.rand.Next(2, 5); i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(3.5f, 3.5f) * Main.rand.NextFloat(0.4f, 1.3f);
                CadenceParticleHandler.Spawn(new VoidCleaveParticle(
                    Projectile.Center, vel, CadenceUtils.CadenceViolet, Main.rand.NextFloat(0.3f, 0.6f), Main.rand.Next(10, 20)));
            }

            // 3-4 CadenceRiftDust
            for (int i = 0; i < Main.rand.Next(3, 5); i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    ModContent.DustType<CadenceRiftDust>(),
                    Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f));
            }
        }
    }
    
    public class ParadoxCollapseUltimate : ModProjectile
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
            float lifeProgress = 1f - (Projectile.timeLeft / 90f);
            float intensity = (float)Math.Sin(lifeProgress * MathHelper.Pi);

            // Phase sizing: grows then shrinks with the collapse
            float outerScale = 2.5f * intensity;
            float pulse = 1f + 0.1f * MathF.Sin(Main.GameUpdateCount * 0.2f);

            // === Shader overlay: Geometric mandala implosion ===
            {
                Texture2D shBloom = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
                EnigmaShaderHelper.DrawShaderOverlay(sb, ShaderLoader.CadenceCollapse,
                    shBloom, drawPos, shBloom.Size() / 2f, 3.0f * intensity,
                    CadenceUtils.CadenceViolet.ToVector3(), CadenceUtils.SeveranceLime.ToVector3(),
                    opacity: 0.6f * intensity, intensity: 1.5f,
                    noiseTexture: ShaderLoader.GetNoiseTexture("CosmicNebulaClouds"),
                    techniqueName: "CadenceCollapseWarp");
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            Texture2D bloom = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            Texture2D pixel = MagnumTextureRegistry.GetPointBloom();
            Texture2D starFlare = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Enigma/Impact Effects/EN Star Flare", AssetRequestMode.ImmediateLoad).Value;
            Texture2D powerRing = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Enigma/Impact Effects/EN Power Effect Ring", AssetRequestMode.ImmediateLoad).Value;

            // Layer 1: Large outer glow — CadenceViolet breathing ambient
            sb.Draw(bloom, drawPos, null, CadenceUtils.CadenceViolet * 0.2f * intensity, 0f,
                bloom.Size() / 2f, outerScale * pulse, SpriteEffects.None, 0f);

            // Layer 2: EN Power Effect Ring — rotating dimensional rift boundary
            float ringRot = Main.GameUpdateCount * 0.015f;
            sb.Draw(powerRing, drawPos, null, CadenceUtils.DimensionalGreen * 0.5f * intensity, ringRot,
                powerRing.Size() / 2f, outerScale * 0.45f * pulse, SpriteEffects.None, 0f);
            sb.Draw(powerRing, drawPos, null, CadenceUtils.CadenceViolet * 0.3f * intensity, -ringRot * 0.7f,
                powerRing.Size() / 2f, outerScale * 0.35f * pulse, SpriteEffects.None, 0f);

            // Layer 3: Inner core — oscillating DimensionalGreen / SeveranceLime
            Color coreColor = Color.Lerp(CadenceUtils.DimensionalGreen, CadenceUtils.SeveranceLime,
                MathF.Sin(Main.GameUpdateCount * 0.12f) * 0.5f + 0.5f);
            sb.Draw(bloom, drawPos, null, coreColor * 0.45f * intensity, 0f,
                bloom.Size() / 2f, outerScale * 0.5f * pulse, SpriteEffects.None, 0f);

            // Layer 4: White-hot center — ParadoxWhite singularity
            sb.Draw(bloom, drawPos, null, CadenceUtils.ParadoxWhite * 0.6f * intensity, 0f,
                bloom.Size() / 2f, outerScale * 0.2f * pulse, SpriteEffects.None, 0f);

            // Layer 5: EN Star Flare overlay — spinning Enigma starburst at center
            float starRot = Main.GameUpdateCount * 0.025f;
            sb.Draw(starFlare, drawPos, null, CadenceUtils.DimensionalGreen * 0.5f * intensity, starRot,
                starFlare.Size() / 2f, outerScale * 0.3f * pulse, SpriteEffects.None, 0f);
            sb.Draw(starFlare, drawPos, null, CadenceUtils.SeveranceLime * 0.3f * intensity, -starRot * 1.3f,
                starFlare.Size() / 2f, outerScale * 0.22f * pulse, SpriteEffects.None, 0f);

            // Crosshatch beams — "implosion" look, slowly rotating
            float beamLength = 200f * intensity;
            float beamWidth = 8f * intensity;
            float rot1 = Main.GameUpdateCount * 0.02f;
            sb.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), CadenceUtils.CadenceViolet * 0.4f * intensity,
                rot1, new Vector2(0.5f, 0.5f), new Vector2(beamLength, beamWidth), SpriteEffects.None, 0f);
            sb.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), CadenceUtils.DimensionalGreen * 0.35f * intensity,
                rot1 + MathHelper.PiOver2, new Vector2(0.5f, 0.5f), new Vector2(beamLength, beamWidth), SpriteEffects.None, 0f);
            sb.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), CadenceUtils.SeveranceLime * 0.2f * intensity,
                rot1 + MathHelper.PiOver4, new Vector2(0.5f, 0.5f), new Vector2(beamLength * 0.7f, beamWidth * 0.6f), SpriteEffects.None, 0f);
            sb.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), CadenceUtils.RiftDeep * 0.25f * intensity,
                rot1 - MathHelper.PiOver4, new Vector2(0.5f, 0.5f), new Vector2(beamLength * 0.7f, beamWidth * 0.6f), SpriteEffects.None, 0f);
            }
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
            Projectile.width = 400;
            Projectile.height = 400;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 6;
        }
        
        public override void AI()
        {
            float lifeProgress = 1f - (Projectile.timeLeft / 90f);
            float intensity = (float)Math.Sin(lifeProgress * MathHelper.Pi);
            float currentRadius = 200f * intensity;
            
            // Phase: 0 = expanding, 1 = peak, 2 = collapsing
            int phase = lifeProgress < 0.3f ? 0 : (lifeProgress < 0.7f ? 1 : 2);
            
            // Periodic damage application via debuff (in addition to contact damage)
            if (Projectile.timeLeft % 10 == 0)
            {
                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (npc.friendly) continue;
                    if (Vector2.Distance(npc.Center, Projectile.Center) > currentRadius) continue;
                    
                    npc.AddBuff(ModContent.BuffType<ParadoxBrand>(), 900);
                    npc.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(npc, 1);
                }
            }
            
            Lighting.AddLight(Projectile.Center, EnigmaGreen.ToVector3() * intensity * 0.8f);

            // Particle VFX
            if (!Main.dedServ)
            {
                int frameTimer = 90 - Projectile.timeLeft;

                // Every frame: 2-3 DimensionalRiftMotes spiraling inward toward center
                for (int i = 0; i < Main.rand.Next(2, 4); i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float dist = currentRadius * Main.rand.NextFloat(0.5f, 1.2f);
                    Vector2 spawnPos = Projectile.Center + angle.ToRotationVector2() * dist;
                    Vector2 inwardVel = (Projectile.Center - spawnPos).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2f, 5f);
                    // Add spiral tangent for swirling motion
                    inwardVel += (angle + MathHelper.PiOver2).ToRotationVector2() * Main.rand.NextFloat(1f, 3f);
                    CadenceParticleHandler.Spawn(new DimensionalRiftMote(
                        spawnPos, inwardVel, Main.rand.NextFloat(0.2f, 0.5f), Main.rand.Next(15, 30)));
                }

                // Every 2 frames: 1 InevitabilityGlyphParticle orbiting the collapse
                if (frameTimer % 2 == 0)
                {
                    float startAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                    CadenceParticleHandler.Spawn(new InevitabilityGlyphParticle(
                        Projectile.Center, currentRadius * 0.6f, startAngle,
                        TheUnresolvedCadenceItem.GetInevitabilityStacks(),
                        CadenceUtils.CadenceViolet, Main.rand.NextFloat(0.3f, 0.5f), Main.rand.Next(25, 40)));
                }

                // Every 4 frames: 1 CadenceRiftDust
                if (frameTimer % 4 == 0)
                {
                    Vector2 dustOffset = Main.rand.NextVector2Circular(currentRadius * 0.5f, currentRadius * 0.5f);
                    Dust.NewDust(Projectile.Center + dustOffset, 0, 0,
                        ModContent.DustType<CadenceRiftDust>(),
                        Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f));
                }
            }
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Paradox Brand (15 seconds for collapse) + 5 stacks
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 900);
            target.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(target, 5);
            
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.8f);

            // Impact VFX: layered ripple + burst + glyph
            if (!Main.dedServ)
            {
                // Double-ripple impact — purple then green for dimensional fracture feel
                CadenceParticleHandler.Spawn(new ParadoxSlashRipple(
                    target.Center, CadenceUtils.DimensionalGreen, 0.4f, 30));
                CadenceParticleHandler.Spawn(new ParadoxSlashRipple(
                    target.Center, CadenceUtils.CadenceViolet, 0.25f, 35));

                // 4-7 DimensionalRiftMotes radiating
                for (int i = 0; i < Main.rand.Next(4, 8); i++)
                {
                    Vector2 burstVel = Main.rand.NextVector2CircularEdge(5f, 5f) * Main.rand.NextFloat(0.5f, 1.5f);
                    CadenceParticleHandler.Spawn(new DimensionalRiftMote(
                        target.Center, burstVel, Main.rand.NextFloat(0.25f, 0.5f), Main.rand.Next(12, 22)));
                }

                // 2-3 VoidCleaveParticle — glitch fragments
                for (int i = 0; i < Main.rand.Next(2, 4); i++)
                {
                    Vector2 vel = Main.rand.NextVector2CircularEdge(3f, 3f) * Main.rand.NextFloat(0.5f, 1.2f);
                    CadenceParticleHandler.Spawn(new VoidCleaveParticle(
                        target.Center, vel, CadenceUtils.SeveranceLime, Main.rand.NextFloat(0.3f, 0.5f), Main.rand.Next(10, 18)));
                }

                // Inevitability glyph on stack increment
                float glyphAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                CadenceParticleHandler.Spawn(new InevitabilityGlyphParticle(
                    target.Center, 40f, glyphAngle,
                    TheUnresolvedCadenceItem.GetInevitabilityStacks(),
                    CadenceUtils.SeveranceLime, 0.4f, 35));
            }
        }
        
        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;

            // 1 ParadoxCollapseFlash at center — screen-filling white burst
            CadenceParticleHandler.Spawn(new ParadoxCollapseFlash(Projectile.Center, 2.5f, 45));

            // 18-28 DimensionalRiftMotes in all directions — dense mote explosion
            int moteCount = Main.rand.Next(18, 29);
            for (int i = 0; i < moteCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(6f, 6f) * Main.rand.NextFloat(0.5f, 2f);
                CadenceParticleHandler.Spawn(new DimensionalRiftMote(
                    Projectile.Center + Main.rand.NextVector2Circular(20f, 20f),
                    vel, Main.rand.NextFloat(0.3f, 0.7f), Main.rand.Next(20, 40)));
            }

            // 6-10 VoidCleaveParticles in all directions — glitch debris
            for (int i = 0; i < Main.rand.Next(6, 11); i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(5f, 5f) * Main.rand.NextFloat(0.8f, 1.8f);
                Color debrisColor = Main.rand.NextBool() ? CadenceUtils.CadenceViolet : CadenceUtils.SeveranceLime;
                CadenceParticleHandler.Spawn(new VoidCleaveParticle(
                    Projectile.Center, vel, debrisColor, Main.rand.NextFloat(0.5f, 0.9f), Main.rand.Next(18, 30)));
            }

            // 2 expanding ParadoxSlashRipples at different rates
            CadenceParticleHandler.Spawn(new ParadoxSlashRipple(
                Projectile.Center, CadenceUtils.DimensionalGreen, 0.5f, 40));
            CadenceParticleHandler.Spawn(new ParadoxSlashRipple(
                Projectile.Center, CadenceUtils.CadenceViolet, 0.35f, 50));

            // 4-6 InevitabilityGlyphParticles floating outward — residual question marks
            for (int i = 0; i < Main.rand.Next(4, 7); i++)
            {
                float angle = MathHelper.TwoPi * i / 6f + Main.rand.NextFloat(-0.3f, 0.3f);
                CadenceParticleHandler.Spawn(new InevitabilityGlyphParticle(
                    Projectile.Center, Main.rand.NextFloat(60f, 120f), angle,
                    TheUnresolvedCadenceItem.GetInevitabilityStacks(),
                    CadenceUtils.DimensionalGreen, Main.rand.NextFloat(0.4f, 0.7f), Main.rand.Next(30, 50)));
            }

            // 10-12 CadenceRiftDust
            for (int i = 0; i < Main.rand.Next(10, 13); i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    ModContent.DustType<CadenceRiftDust>(),
                    Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f));
            }
        }
    }
}
