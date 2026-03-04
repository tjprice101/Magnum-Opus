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
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.DissonanceOfSecrets.Particles;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.DissonanceOfSecrets.Dusts;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.DissonanceOfSecrets.Utilities;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.DissonanceOfSecrets
{
    /// <summary>
    /// DISSONANCE OF SECRETS — Magic growing orb weapon (Enigma Variations theme).
    /// A riddle that grows larger the longer it exists — a void orb that swells and consumes.
    /// 
    /// Fires growing void orbs that scale from 0.5→2.0 over their 5s lifetime.
    /// Orbs decelerate (×0.985/frame), dealing aura damage every 15 frames (30%).
    /// Spawns homing riddlebolts every 60 frames like forbidden knowledge escaping containment.
    /// On death: cascade explosion + 4 SeekingCrystals.
    /// ParadoxBrand on all hits.
    /// 
    /// Custom Shaders: DissonanceOrbAura.fx, DissonanceRiddleTrail.fx
    /// Foundation: MagicOrbFoundation + MaskFoundation (RadialNoiseMaskShader) planned
    /// </summary>
    public class DissonanceOfSecrets : ModItem
    {
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        public override string Texture => "MagnumOpus/Content/EnigmaVariations/ResonantWeapons/DissonanceOfSecrets/DissonanceOfSecrets";
        
        public override void SetDefaults()
        {
            Item.damage = 275;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 20;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(gold: 20);
            Item.rare = ModContent.RarityType<EnigmaRarity>();
            Item.UseSound = SoundID.Item117;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<RiddleCascadeOrb>();
            Item.shootSpeed = 10f;
            Item.noMelee = true;
        }
        
        public override void HoldItem(Player player)
        {
            Lighting.AddLight(player.Center, EnigmaPurple.ToVector3() * 0.15f);
        }
        
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            return true;
        }
        
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            return true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires a growing void orb that swells and consumes as it drifts"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Orb deals periodic aura damage to nearby enemies as it grows"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Periodically spawns homing riddlebolts that seek nearby targets"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "On expiry, detonates in a cascade explosion spawning seeking crystals"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Hits brand enemies with Paradox Brand"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Some things grow when you don't look at them.'")
            {
                OverrideColor = EnigmaPurple
            });
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }
    }
    
    public class RiddleCascadeOrb : ModProjectile
    {
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private float currentScale = 0.5f;
        private const float MaxScale = 2.0f;
        private const float GrowthRate = 0.005f;
        private int auraDamageTimer = 0;
        private int riddleboltTimer = 0;
        
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/CursiveMusicNote";
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            Texture2D glyphTex = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = 1f + 0.12f * MathF.Sin(Main.GameUpdateCount * 0.08f);

            // === Shader overlay: Counter-rotating arcane circles ===
            EnigmaShaderHelper.DrawShaderOverlay(sb, ShaderLoader.DissonanceOrbAura,
                bloomTex, drawPos, bloomTex.Size() / 2f, currentScale * 3f * pulse,
                DissonanceUtils.SecretPurple.ToVector3(), DissonanceUtils.CascadeGreen.ToVector3(),
                opacity: 0.5f, intensity: 1.0f,
                noiseTexture: ShaderLoader.GetNoiseTexture("CosmicEnergyVortex"),
                techniqueName: "DissonanceOrbAuraMain");

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            // Layer 1: Outer soft bloom — the secret's aura
            Color outerColor = DissonanceUtils.SecretPurple * (0.35f + 0.15f * MathF.Sin(Main.GameUpdateCount * 0.06f));
            sb.Draw(bloomTex, drawPos, null, outerColor, 0f, bloomTex.Size() / 2f, currentScale * 2f * pulse, SpriteEffects.None, 0f);

            // Layer 2: Middle glow — cascade energy
            Color midColor = DissonanceUtils.CascadeGreen * 0.55f;
            sb.Draw(bloomTex, drawPos, null, midColor, 0f, bloomTex.Size() / 2f, currentScale * 1.2f * pulse, SpriteEffects.None, 0f);

            // Layer 3: White-hot core
            sb.Draw(bloomTex, drawPos, null, Color.White * 0.7f, 0f, bloomTex.Size() / 2f, currentScale * 0.4f, SpriteEffects.None, 0f);

            // Layer 4: EN Star Flare — rotating Enigma identity burst
            Texture2D starFlare = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Enigma/Impact Effects/EN Star Flare", AssetRequestMode.ImmediateLoad).Value;
            float starRot = Main.GameUpdateCount * 0.025f;
            sb.Draw(starFlare, drawPos, null, DissonanceUtils.CascadeGreen * 0.45f, starRot,
                starFlare.Size() / 2f, currentScale * 0.6f * pulse, SpriteEffects.None, 0f);
            sb.Draw(starFlare, drawPos, null, DissonanceUtils.SecretPurple * 0.3f, -starRot * 0.8f,
                starFlare.Size() / 2f, currentScale * 0.45f * pulse, SpriteEffects.None, 0f);

            // Layer 5: Glyph sprite tinted SecretPurple
            Color glyphColor = DissonanceUtils.SecretPurple * 0.85f;
            sb.Draw(glyphTex, drawPos, null, glyphColor, Projectile.rotation, glyphTex.Size() / 2f, currentScale * 0.8f, SpriteEffects.None, 0f);

            // Theme texture accents
            DissonanceUtils.DrawThemeAccents(sb, Projectile.Center, 1f, 0.6f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }
        
        public override void AI()
        {
            // Grow over time
            if (currentScale < MaxScale)
                currentScale += GrowthRate;
            
            Projectile.scale = currentScale;
            
            // Slow down
            Projectile.velocity *= 0.985f;
            
            // Rotation
            Projectile.rotation += 0.02f;
            
            // Aura damage (periodic)
            auraDamageTimer++;
            if (auraDamageTimer >= 15)
            {
                auraDamageTimer = 0;
                DealAuraDamage();
            }
            
            // Riddlebolt spawning
            riddleboltTimer++;
            if (riddleboltTimer >= 60)
            {
                riddleboltTimer = 0;
                ReleaseRiddlebolt();
            }
            
            Lighting.AddLight(Projectile.Center, EnigmaGreen.ToVector3() * currentScale * 0.3f);

            // --- VFX: Particle spawning ---
            if (Main.netMode != NetmodeID.Server)
            {
                float growthProgress = currentScale / MaxScale;

                // Every 2 frames: RiddleEchoParticle spiraling outward
                if (Main.GameUpdateCount % 2 == 0)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(10f, 10f);
                    Vector2 perpVel = new Vector2(-offset.Y, offset.X).SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(0.5f, 1.5f);
                    Color echoColor = Color.Lerp(DissonanceUtils.SecretPurple, DissonanceUtils.CascadeGreen, growthProgress);
                    DissonanceParticleHandler.Spawn(new RiddleEchoParticle(
                        Projectile.Center + offset, perpVel, echoColor, Main.rand.NextFloat(0.3f, 0.6f), Main.rand.Next(40, 70)));
                }

                // Every 15 frames: SecretGlyphParticle orbiting the orb
                if (Main.GameUpdateCount % 15 == 0)
                {
                    float orbitRadius = 30f * currentScale;
                    float startAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                    DissonanceParticleHandler.Spawn(new SecretGlyphParticle(
                        Projectile.Center, orbitRadius, startAngle, DissonanceUtils.SecretPurple * 0.8f, Main.rand.NextFloat(0.4f, 0.7f), Main.rand.Next(60, 100)));
                }

                // Every frame: DissonanceSecretDust in random radius
                float dustRadius = currentScale * 30f;
                Vector2 dustOffset = Main.rand.NextVector2Circular(dustRadius, dustRadius);
                Dust.NewDustPerfect(Projectile.Center + dustOffset, ModContent.DustType<DissonanceSecretDust>(),
                    Main.rand.NextVector2Circular(1f, 1f), 0, DissonanceUtils.CascadeGreen, Main.rand.NextFloat(0.5f, 0.9f));
            }

            // Update DissonancePlayer state
            if (Projectile.owner == Main.myPlayer)
            {
                var dp = Main.LocalPlayer.GetModPlayer<DissonancePlayer>();
                dp.OrbChargeLevel = (int)(currentScale * 50);
                dp.SecretIntensity = currentScale / MaxScale;
            }
        }
        
        private void DealAuraDamage()
        {
            float auraRadius = 80f * currentScale;
            
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly) continue;
                if (Vector2.Distance(npc.Center, Projectile.Center) > auraRadius) continue;
                
                npc.SimpleStrikeNPC((int)(Projectile.damage * 0.3f), 0, false, 0f, null, false, 0f, true);
                npc.AddBuff(ModContent.BuffType<ParadoxBrand>(), 480);
                npc.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(npc, 1);
            }
        }
        
        private void ReleaseRiddlebolt()
        {
            NPC target = FindClosestEnemy(400f);
            if (target == null) return;
            
            SoundEngine.PlaySound(SoundID.Item125 with { Pitch = 0.3f, Volume = 0.4f }, Projectile.Center);
            
            Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 12f;
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, toTarget,
                ModContent.ProjectileType<Riddlebolt>(),
                (int)(Projectile.damage * 0.6f), 3f, Projectile.owner);
        }
        
        private NPC FindClosestEnemy(float range)
        {
            NPC closest = null;
            float bestDist = range;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    closest = npc;
                }
            }
            return closest;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 480);
            target.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(target, 2);
            
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.5f);
        }
        
        public override void OnKill(int timeLeft)
        {
            TriggerCascadeExplosion();
        }
        
        private void TriggerCascadeExplosion()
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.3f, Volume = 0.8f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.1f, Volume = 0.6f }, Projectile.Center);
            
            float explosionRadius = 150f * currentScale;
            
            SeekingCrystalHelper.SpawnEnigmaCrystals(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                Vector2.Zero,
                (int)(Projectile.damage * 0.3f),
                5f,
                Projectile.owner,
                4
            );
            
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly) continue;
                if (Vector2.Distance(npc.Center, Projectile.Center) > explosionRadius) continue;
                
                npc.SimpleStrikeNPC((int)(Projectile.damage * 1.5f), 0, false, 0f, null, false, 0f, true);
                npc.AddBuff(ModContent.BuffType<ParadoxBrand>(), 480);
                npc.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(npc, 3);
            }

            // --- VFX: Cascade explosion burst ---
            if (Main.netMode != NetmodeID.Server)
            {
                // 15-25 CascadeSparkParticles in all directions
                int sparkCount = Main.rand.Next(15, 26);
                for (int i = 0; i < sparkCount; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float speed = Main.rand.NextFloat(8f, 15f);
                    Vector2 sparkVel = angle.ToRotationVector2() * speed;
                    Color sparkColor = Main.rand.NextBool() ? DissonanceUtils.CascadeGreen : DissonanceUtils.SecretPurple;
                    DissonanceParticleHandler.Spawn(new CascadeSparkParticle(
                        Projectile.Center, sparkVel, sparkColor, Main.rand.NextFloat(0.3f, 0.8f), Main.rand.Next(20, 40)));
                }

                // 3-5 large RiddleEchoParticles expanding outward slowly
                int echoCount = Main.rand.Next(3, 6);
                for (int i = 0; i < echoCount; i++)
                {
                    Vector2 echoVel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(0.5f, 1.5f);
                    Color echoColor = Color.Lerp(DissonanceUtils.SecretPurple, DissonanceUtils.RevelationLime, Main.rand.NextFloat());
                    DissonanceParticleHandler.Spawn(new RiddleEchoParticle(
                        Projectile.Center, echoVel, echoColor, Main.rand.NextFloat(1.5f, 2.5f), Main.rand.Next(50, 80)));
                }

                // 1 large SecretGlyphParticle at center
                DissonanceParticleHandler.Spawn(new SecretGlyphParticle(
                    Projectile.Center, 0.1f, Main.rand.NextFloat(MathHelper.TwoPi), DissonanceUtils.TruthWhite * 0.9f, 1.5f, 60));

                // 8-10 DissonanceSecretDust in all directions
                int dustCount = Main.rand.Next(8, 11);
                for (int i = 0; i < dustCount; i++)
                {
                    Vector2 dustVel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(3f, 7f);
                    Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<DissonanceSecretDust>(),
                        dustVel, 0, DissonanceUtils.CascadeGreen, Main.rand.NextFloat(0.7f, 1.2f));
                }

                // Set CascadeThisFrame flag
                if (Projectile.owner == Main.myPlayer)
                {
                    Main.LocalPlayer.GetModPlayer<DissonancePlayer>().CascadeThisFrame = true;
                }
            }
        }
    }
    
    public class Riddlebolt : ModProjectile
    {
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/MusicNote";
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D pixel = MagnumTextureRegistry.GetSoftGlow();
            Texture2D bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float velRot = Projectile.velocity.ToRotation();

            // === Shader overlay: Encrypted/decrypted riddle trail segments ===
            EnigmaShaderHelper.DrawShaderOverlay(sb, ShaderLoader.DissonanceRiddleTrail,
                bloomTex, drawPos, bloomTex.Size() / 2f, 0.8f,
                DissonanceUtils.SecretPurple.ToVector3(), DissonanceUtils.CascadeGreen.ToVector3(),
                opacity: 0.5f, intensity: 1.0f, rotation: velRot,
                noiseTexture: ShaderLoader.GetNoiseTexture("VoronoiNoise"),
                techniqueName: "DissonanceRiddleFlow");

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            // Glow trail: MagicPixel stretched along velocity direction
            float trailLength = MathHelper.Clamp(Projectile.velocity.Length() * 3f, 12f, 60f);
            Vector2 trailCenter = drawPos - Projectile.velocity.SafeNormalize(Vector2.Zero) * trailLength * 0.4f;
            for (int i = 0; i < 3; i++)
            {
                float alphaFade = 0.3f - i * 0.08f;
                float widthFade = 8f - i * 2f;
                sb.Draw(pixel, trailCenter, new Rectangle(0, 0, 1, 1), DissonanceUtils.SecretPurple * alphaFade,
                    velRot, new Vector2(0.5f, 0.5f), new Vector2(trailLength + i * 6f, widthFade), SpriteEffects.None, 0f);
            }

            // Bright core circle
            float coreScale = 0.3f + 0.05f * MathF.Sin(Main.GameUpdateCount * 0.15f);
            sb.Draw(bloomTex, drawPos, null, DissonanceUtils.CascadeGreen * 0.8f, 0f,
                bloomTex.Size() / 2f, coreScale, SpriteEffects.None, 0f);
            sb.Draw(bloomTex, drawPos, null, Color.White * 0.5f, 0f,
                bloomTex.Size() / 2f, coreScale * 0.4f, SpriteEffects.None, 0f);

            // EN Star Flare — rotating green starburst on riddlebolt
            Texture2D starFlare = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Enigma/Impact Effects/EN Star Flare", AssetRequestMode.ImmediateLoad).Value;
            float starRot = Main.GameUpdateCount * 0.04f;
            sb.Draw(starFlare, drawPos, null, DissonanceUtils.CascadeGreen * 0.35f, starRot,
                starFlare.Size() / 2f, coreScale * 0.8f, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Mild homing
            NPC target = FindClosestEnemy(400f);
            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), 0.03f);
            }
            
            Lighting.AddLight(Projectile.Center, EnigmaPurple.ToVector3() * 0.25f);

            // --- VFX: Trail particles ---
            if (Main.netMode != NetmodeID.Server)
            {
                // Every frame: RiddleboltTrailMote trailing behind
                Vector2 trailVel = -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.3f, 0.8f);
                DissonanceParticleHandler.Spawn(new RiddleboltTrailMote(
                    Projectile.Center, trailVel, DissonanceUtils.CascadeGreen, Main.rand.NextFloat(0.15f, 0.3f), Main.rand.Next(15, 25)));

                // Every 3 frames: DissonanceSecretDust
                if (Main.GameUpdateCount % 3 == 0)
                {
                    Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<DissonanceSecretDust>(),
                        Main.rand.NextVector2Circular(0.5f, 0.5f), 0, DissonanceUtils.SecretPurple, Main.rand.NextFloat(0.3f, 0.6f));
                }
            }
        }
        
        private NPC FindClosestEnemy(float range)
        {
            NPC closest = null;
            float bestDist = range;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    closest = npc;
                }
            }
            return closest;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 480);
            target.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(target, 1);
            
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.4f);
        }
        
        public override void OnKill(int timeLeft)
        {
            if (Main.netMode == NetmodeID.Server) return;

            // 5-8 CascadeSparkParticles
            int sparkCount = Main.rand.Next(5, 9);
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(3f, 7f);
                Vector2 sparkVel = angle.ToRotationVector2() * speed;
                Color sparkColor = Main.rand.NextBool() ? DissonanceUtils.CascadeGreen : DissonanceUtils.SecretPurple;
                DissonanceParticleHandler.Spawn(new CascadeSparkParticle(
                    Projectile.Center, sparkVel, sparkColor, Main.rand.NextFloat(0.2f, 0.5f), Main.rand.Next(15, 30)));
            }

            // 1 RiddleEchoParticle fading
            DissonanceParticleHandler.Spawn(new RiddleEchoParticle(
                Projectile.Center, Vector2.Zero, DissonanceUtils.SecretPurple * 0.7f, 0.5f, 30));

            // 3 DissonanceSecretDust
            for (int i = 0; i < 3; i++)
            {
                Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<DissonanceSecretDust>(),
                    Main.rand.NextVector2Circular(2f, 2f), 0, DissonanceUtils.CascadeGreen, Main.rand.NextFloat(0.4f, 0.8f));
            }
        }
    }
}
