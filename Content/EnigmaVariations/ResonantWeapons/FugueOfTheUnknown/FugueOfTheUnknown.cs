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
        
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/MusicNote";
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = MathF.Sin((float)Main.GameUpdateCount * 0.08f + Projectile.ai[1]) * 0.5f + 0.5f; // 0..1

            var bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            var glyphTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/MusicNote", AssetRequestMode.ImmediateLoad).Value;
            var starFlareTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Enigma/Impact Effects/EN Star Flare", AssetRequestMode.ImmediateLoad).Value;
            var powerRingTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Enigma/Impact Effects/EN Power Effect Ring", AssetRequestMode.ImmediateLoad).Value;
            Vector2 bloomOrigin = bloomTex.Size() / 2f;
            Vector2 glyphOrigin = glyphTex.Size() / 2f;
            Vector2 starFlareOrigin = starFlareTex.Size() / 2f;
            Vector2 powerRingOrigin = powerRingTex.Size() / 2f;

            // === Shader overlay: Polyphonic voice spectrum / standing wave interference ===
            {
                Effect voiceShader = Projectile.ai[0] == 0f ? ShaderLoader.FugueVoiceTrail : ShaderLoader.FugueConvergence;
                string technique = Projectile.ai[0] == 0f ? "FugueVoiceFlow" : "FugueConvergenceWave";
                EnigmaShaderHelper.DrawShaderOverlay(sb, voiceShader,
                    bloomTex, drawPos, bloomOrigin, 1.2f + pulse * 0.3f,
                    FugueUtils.VoicePurple.ToVector3(), FugueUtils.EchoTeal.ToVector3(),
                    opacity: 0.45f, intensity: 1.0f,
                    noiseTexture: ShaderLoader.GetNoiseTexture("MusicalWavePattern"),
                    techniqueName: technique);
            }

            if (Projectile.ai[0] == 0f)
            {
                // === ORBITING STATE — ghostly eye/glyph with pulsing bloom ===

                // Outer bloom: Voice Purple, pulsing scale
                float outerScale = 0.5f + pulse * 0.15f;
                Color outerColor = FugueUtils.VoicePurple * (0.35f + pulse * 0.15f);
                sb.Draw(bloomTex, drawPos, null, outerColor, 0f, bloomOrigin, outerScale, SpriteEffects.None, 0f);

                // Inner bloom: Echo Teal, smaller
                float innerScale = 0.25f + pulse * 0.08f;
                Color innerColor = FugueUtils.EchoTeal * (0.4f + pulse * 0.2f);
                sb.Draw(bloomTex, drawPos, null, innerColor, 0f, bloomOrigin, innerScale, SpriteEffects.None, 0f);

                // Core glyph sprite with sin-wave pulsing opacity (0.5 - 1.0)
                float glyphAlpha = 0.5f + pulse * 0.5f;
                sb.Draw(glyphTex, drawPos, null, Color.White * glyphAlpha, Projectile.rotation, glyphOrigin, 0.6f, SpriteEffects.None, 0f);

                // Layer 4: EN Star Flare — dual-rotating spectral flare
                float flareRotA = (float)Main.GameUpdateCount * 0.02f + Projectile.ai[1] * 1.5f;
                float flareRotB = -(float)Main.GameUpdateCount * 0.015f + Projectile.ai[1] * 2.1f;
                float flareScale = 0.18f + pulse * 0.06f;
                sb.Draw(starFlareTex, drawPos, null, FugueUtils.VoicePurple * (0.35f + pulse * 0.15f), flareRotA, starFlareOrigin, flareScale, SpriteEffects.None, 0f);
                sb.Draw(starFlareTex, drawPos, null, FugueUtils.EchoTeal * (0.25f + pulse * 0.1f), flareRotB, starFlareOrigin, flareScale * 0.8f, SpriteEffects.None, 0f);
            }
            else
            {
                // === RELEASED STATE — motion trail stretched in velocity direction ===
                float speed = Projectile.velocity.Length();
                float velAngle = Projectile.velocity.ToRotation();

                // Stretched MagicPixel "motion trail" behind the voice
                var pixel = MagnumTextureRegistry.GetSoftGlow();
                if (pixel == null) return false;
                float trailLength = MathHelper.Clamp(speed * 3f, 20f, 80f);
                Rectangle trailRect = new Rectangle(0, 0, (int)trailLength, 6);
                Vector2 trailOrigin = new Vector2(trailLength, 3f); // anchor at right-center so it trails behind
                Color trailColor = FugueUtils.EchoTeal * 0.6f;
                sb.Draw(pixel, drawPos, trailRect, trailColor, velAngle, trailOrigin, 1f, SpriteEffects.None, 0f);

                // Brighter wider inner trail
                Rectangle trailRect2 = new Rectangle(0, 0, (int)(trailLength * 0.6f), 3);
                Vector2 trailOrigin2 = new Vector2(trailLength * 0.6f, 1.5f);
                sb.Draw(pixel, drawPos, trailRect2, FugueUtils.FugueCyan * 0.8f, velAngle, trailOrigin2, 1f, SpriteEffects.None, 0f);

                // Bloom core — larger, more intense
                float coreScale = 0.4f + pulse * 0.1f;
                sb.Draw(bloomTex, drawPos, null, FugueUtils.VoicePurple * 0.7f, 0f, bloomOrigin, coreScale, SpriteEffects.None, 0f);
                sb.Draw(bloomTex, drawPos, null, FugueUtils.HarmonicWhite * 0.4f, 0f, bloomOrigin, coreScale * 0.35f, SpriteEffects.None, 0f);

                // Glyph sprite at full intensity
                sb.Draw(glyphTex, drawPos, null, Color.White * 0.9f, Projectile.rotation, glyphOrigin, 0.7f, SpriteEffects.None, 0f);

                // Layer 5: EN Power Effect Ring — rotating ring behind released voice
                float ringRot = (float)Main.GameUpdateCount * 0.04f;
                float ringScale = 0.22f + pulse * 0.08f;
                sb.Draw(powerRingTex, drawPos, null, FugueUtils.EchoTeal * (0.3f + pulse * 0.15f), ringRot, powerRingOrigin, ringScale, SpriteEffects.None, 0f);
                sb.Draw(powerRingTex, drawPos, null, FugueUtils.VoicePurple * (0.2f + pulse * 0.1f), -ringRot * 0.7f, powerRingOrigin, ringScale * 1.3f, SpriteEffects.None, 0f);

                // Layer 6: EN Star Flare — streaking behind released voice
                float relFlareRot = velAngle + MathF.Sin((float)Main.GameUpdateCount * 0.1f) * 0.3f;
                sb.Draw(starFlareTex, drawPos, null, FugueUtils.FugueCyan * (0.35f + pulse * 0.15f), relFlareRot, starFlareOrigin, 0.2f + pulse * 0.05f, SpriteEffects.None, 0f);
            }

            // Theme texture accents (additive pass)
            try
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                FugueUtils.DrawThemeAccents(sb, Projectile.Center, 1f, 0.6f);
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            catch { }

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

            // EN Power Effect Ring — expanding concentric rings at convergence point
            {
                var convRingTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Enigma/Impact Effects/EN Power Effect Ring", AssetRequestMode.ImmediateLoad).Value;
                Vector2 convRingOrigin = convRingTex.Size() / 2f;
                Vector2 convDrawPos = target.Center - Main.screenPosition;
                float convRot1 = Main.rand.NextFloat(MathHelper.TwoPi);
                float convRot2 = convRot1 + MathHelper.PiOver4;
                Main.spriteBatch.Draw(convRingTex, convDrawPos, null, FugueUtils.VoicePurple * 0.6f, convRot1, convRingOrigin, 0.5f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(convRingTex, convDrawPos, null, FugueUtils.EchoTeal * 0.45f, convRot2, convRingOrigin, 0.7f, SpriteEffects.None, 0f);
            }

            // EN Star Flare — brilliant convergence flash flare
            {
                var convFlareTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Enigma/Impact Effects/EN Star Flare", AssetRequestMode.ImmediateLoad).Value;
                Vector2 convFlareOrigin = convFlareTex.Size() / 2f;
                Vector2 convDrawPos = target.Center - Main.screenPosition;
                float convFlareRot = Main.rand.NextFloat(MathHelper.TwoPi);
                Main.spriteBatch.Draw(convFlareTex, convDrawPos, null, FugueUtils.HarmonicWhite * 0.7f, convFlareRot, convFlareOrigin, 0.4f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(convFlareTex, convDrawPos, null, FugueUtils.FugueCyan * 0.5f, convFlareRot + MathHelper.PiOver2, convFlareOrigin, 0.35f, SpriteEffects.None, 0f);
            }

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
