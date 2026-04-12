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
using Terraria.GameContent;
using ReLogic.Content;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheWatchingRefrain.Particles;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheWatchingRefrain.Dusts;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheWatchingRefrain.Utilities;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheWatchingRefrain.Primitives;
using MagnumOpus.Content.EnigmaVariations;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheWatchingRefrain
{
    /// <summary>
    /// THE WATCHING REFRAIN — Summoner weapon (Enigma Variations theme).
    /// The watcher — a phantom that observes, judges, and creates mystery zones
    /// of crowd control. Playing the refrain that never changes but always disturbs.
    /// 
    /// Summons UnsolvedPhantomMinion (1 slot, hovers 150u behind player).
    /// Minion fires PhantomBolts every 40 frames (penetrate 2, chain 30%).
    /// PhantomBolt on-hit spawns PhantomRift (60-frame lingering AoE zone).
    /// Creates MysteryZone every 300 frames (160×160 AoE, slows 0.85×, pulls to center).
    /// All hits apply ParadoxBrand.
    /// 
    /// Custom Shaders: WatchingPhantomAura.fx, WatchingMysteryZone.fx
    /// Foundation: MaskFoundation + SparkleProjectileFoundation + ImpactFoundation planned
    /// </summary>
    public class TheWatchingRefrain : ModItem
    {
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        public override string Texture => "MagnumOpus/Content/EnigmaVariations/ResonantWeapons/TheWatchingRefrain/TheWatchingRefrain";
        
        public override void SetDefaults()
        {
            Item.damage = 220;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 14;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(gold: 20);
            Item.rare = ModContent.RarityType<EnigmaRainbowRarity>();
            Item.UseSound = SoundID.Item44;
            Item.shoot = ModContent.ProjectileType<UnsolvedPhantomMinion>();
            Item.buffType = ModContent.BuffType<UnsolvedPhantomBuff>();
            Item.noMelee = true;
        }
        
        public override void HoldItem(Player player)
        {
            int phantomCount = 0;
            foreach (Projectile proj in Main.ActiveProjectiles)
            {
                if (proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<UnsolvedPhantomMinion>())
                    phantomCount++;
            }
            
            Lighting.AddLight(player.Center, EnigmaPurple.ToVector3() * (0.1f + phantomCount * 0.03f));
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Summons a watching phantom that observes and judges enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Phantom fires void bolts that spawn lingering rift zones on hit"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Periodically creates Mystery Zones that slow and pull enemies inward"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "All hits brand enemies with Paradox Brand"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'It watches. It always watches.'")
            {
                OverrideColor = EnigmaPurple
            });
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            Projectile.NewProjectile(source, position, Vector2.Zero, type, damage, knockback, player.whoAmI);
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 0.5f }, position);
            return false;
        }
    }
    
    public class UnsolvedPhantomBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }
        
        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<UnsolvedPhantomMinion>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
    
    public class UnsolvedPhantomMinion : ModProjectile
    {
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private int phaseTimer = 0;
        private float visibility = 0f;
        private int attackCooldown = 0;
        private int mysteryZoneCooldown = 0;
        
        public override string Texture => "MagnumOpus/Content/EnigmaVariations/ResonantWeapons/TheWatchingRefrain/UnsolvedPhantomMinion";
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float time = (float)Main.GameUpdateCount;

            // === Stage 1: GPU Primitive Spectral Aura Ring ===
            {
                try
                {
                    sb.End();
                    int ringPoints = 32;
                    var ringPositions = new List<Vector2>(ringPoints + 1);
                    float auraRadius = 28f + MathF.Sin(time * 0.04f) * 4f;
                    for (int i = 0; i <= ringPoints; i++)
                    {
                        float angle = MathHelper.TwoPi * i / ringPoints;
                        float wobble = MathF.Sin(angle * 3f + time * 0.06f) * 3f;
                        ringPositions.Add(Projectile.Center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (auraRadius + wobble));
                    }

                    // Pass 1: Body ring — RefrainPurple to GazeGreen
                    var bodySettings = new WatchingPrimitiveSettings(
                        c => 6f + MathF.Sin(c * MathHelper.TwoPi * 2f + time * 0.05f) * 2f,
                        c => Color.Lerp(WatchingUtils.RefrainPurple, WatchingUtils.GazeGreen, c) * (0.6f * Projectile.Opacity),
                        ShaderLoader.WatchingPhantomAura,
                        smoothing: true, maxPoints: 120);
                    WatchingPrimitiveRenderer.RenderTrail(ringPositions, bodySettings);

                    // Pass 2: Outer glow — WatcherDeep atmospheric
                    var glowSettings = new WatchingPrimitiveSettings(
                        c => 12f + MathF.Sin(c * MathHelper.TwoPi * 2f + time * 0.03f) * 3f,
                        c => WatchingUtils.WatcherDeep * (0.25f * Projectile.Opacity * (0.7f + MathF.Sin(c * MathHelper.TwoPi + time * 0.04f) * 0.3f)),
                        ShaderLoader.WatchingPhantomAura,
                        smoothing: true, maxPoints: 120);
                    WatchingPrimitiveRenderer.RenderTrail(ringPositions, glowSettings);

                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                        DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
                catch
                {
                    try { sb.End(); } catch { }
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                        DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }

            // === Stage 2: Shader overlay — procedural watching eyes ===
            {
                Texture2D shBloom = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
                EnigmaShaderHelper.DrawShaderOverlay(sb, ShaderLoader.WatchingPhantomAura,
                    shBloom, drawPos, shBloom.Size() / 2f, 0.045f,
                    WatchingUtils.GazeGreen.ToVector3(), WatchingUtils.RefrainPurple.ToVector3(),
                    opacity: 0.5f * Projectile.Opacity, intensity: 1.0f,
                    noiseTexture: ShaderLoader.GetNoiseTexture("PerlinNoise"),
                    techniqueName: "WatchingPhantomGhost");
            }

            // === Stage 3: 6-layer bloom stack (Additive) ===
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            Texture2D bloom = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            Vector2 bOrigin = bloom.Size() / 2f;
            float pulse = 0.85f + MathF.Sin(time * 0.05f) * 0.15f;
            float op = Projectile.Opacity;

            sb.Draw(bloom, drawPos, null, WatchingUtils.WatcherDeep * 0.30f * pulse * op, 0f, bOrigin, 0.038f, SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null, WatchingUtils.RefrainPurple * 0.40f * pulse * op, 0f, bOrigin, 0.028f, SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null, WatchingUtils.GazeGreen * 0.50f * pulse * op, 0f, bOrigin, 0.018f, SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null, WatchingUtils.SpectralMint * 0.35f * pulse * op, 0f, bOrigin, 0.01f, SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null, WatchingUtils.PhantomWhite * 0.45f * pulse * op, 0f, bOrigin, 0.005f, SpriteEffects.None, 0f);

            // === Stage 4: EN Star Flare — dual counter-rotating phantom flares ===
            {
                Texture2D sfTex = EnigmaThemeTextures.ENStarFlare?.Value;
                if (sfTex != null)
                {
                    Vector2 sfOrigin = sfTex.Size() / 2f;
                    float sfRotA = time * 0.025f;
                    float sfRotB = -time * 0.018f;
                    float sfScale = 0.07f + MathF.Sin(time * 0.04f) * 0.014f;
                    sb.Draw(sfTex, drawPos, null, WatchingUtils.GazeGreen * 0.35f * op, sfRotA, sfOrigin, sfScale, SpriteEffects.None, 0f);
                    sb.Draw(sfTex, drawPos, null, WatchingUtils.RefrainPurple * 0.25f * op, sfRotB, sfOrigin, sfScale * 0.85f, SpriteEffects.None, 0f);
                }
            }

            // === Stage 5: EN Power Effect Ring — phantom containment ring ===
            {
                Texture2D ringTex = EnigmaThemeTextures.ENPowerEffectRing?.Value;
                if (ringTex != null)
                {
                    Vector2 prOrigin = ringTex.Size() / 2f;
                    float prRot = time * 0.015f;
                    sb.Draw(ringTex, drawPos, null, WatchingUtils.RefrainPurple * 0.22f * op, prRot, prOrigin, 0.063f, SpriteEffects.None, 0f);
                    sb.Draw(ringTex, drawPos, null, WatchingUtils.GazeGreen * 0.15f * op, -prRot * 0.7f, prOrigin, 0.084f, SpriteEffects.None, 0f);
                }
            }

            // === Stage 6: EN Enigma Eye — the watcher's ever-present gaze ===
            {
                Texture2D eyeTex = EnigmaThemeTextures.ENEnigmaEye?.Value;
                if (eyeTex != null)
                {
                    Vector2 eyeOrigin = eyeTex.Size() / 2f;
                    float eyePulse = 0.6f + MathF.Sin(time * 0.03f) * 0.4f;
                    sb.Draw(eyeTex, drawPos, null, WatchingUtils.PhantomWhite * 0.35f * eyePulse * op, 0f, eyeOrigin, 0.035f, SpriteEffects.None, 0f);
                }
            }

            // === Stage 7: Minion glyph sprite ===
            {
                Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
                Vector2 texOrigin = tex.Size() / 2f;
                sb.Draw(tex, drawPos, null, WatchingUtils.RefrainPurple * (0.55f * op), Projectile.rotation, texOrigin, Projectile.scale, SpriteEffects.None, 0f);
            }

            // === Pulsing light accent ===
            EnigmaVFXLibrary.AddPulsingLight(Projectile.Center, WatchingUtils.RefrainPurple, 0.7f, 0.35f);
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
        
        public override void SetStaticDefaults()
        {
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 9999;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.minionSlots = 1f;
            Projectile.minion = true;
        }
        
        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true;
        
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            // Check if minion should still exist
            if (!CheckActive(owner)) return;
            
            phaseTimer++;
            
            // Fade in/out
            visibility = MathHelper.Lerp(visibility, 1f, 0.05f);
            Projectile.Opacity = visibility;
            
            // Decrease cooldowns
            if (attackCooldown > 0) attackCooldown--;
            if (mysteryZoneCooldown > 0) mysteryZoneCooldown--;
            
            // Find target
            NPC target = FindTarget(owner);
            
            if (target != null)
            {
                // Move toward enemy
                Vector2 toTarget = (target.Center - Projectile.Center);
                float dist = toTarget.Length();
                toTarget = toTarget.SafeNormalize(Vector2.UnitX);
                
                float desiredDist = 150f; // Hover at range
                if (dist > desiredDist + 50f)
                {
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 10f, 0.05f);
                }
                else if (dist < desiredDist - 50f)
                {
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, -toTarget * 5f, 0.05f);
                }
                else
                {
                    // Orbit sideways
                    Vector2 sideways = new Vector2(-toTarget.Y, toTarget.X);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, sideways * 4f, 0.05f);
                }
                
                // Attack
                if (attackCooldown <= 0 && dist < 400f)
                {
                    AttackTarget(target);
                    attackCooldown = 40;
                }
                
                // Create mystery zone
                if (mysteryZoneCooldown <= 0 && dist < 300f)
                {
                    CreateMysteryZone(target);
                    mysteryZoneCooldown = 300;
                }
            }
            else
            {
                // Idle: follow owner
                Vector2 toOwner = (owner.Center - Projectile.Center);
                float distToOwner = toOwner.Length();
                
                if (distToOwner > 300f)
                {
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toOwner.SafeNormalize(Vector2.UnitX) * 12f, 0.08f);
                }
                else if (distToOwner > 80f)
                {
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toOwner.SafeNormalize(Vector2.UnitX) * 4f, 0.03f);
                }
                else
                {
                    Projectile.velocity *= 0.95f;
                }
            }
            
            // Velocity cap
            if (Projectile.velocity.Length() > 14f)
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 14f;
            
            Projectile.rotation += 0.02f;
            Lighting.AddLight(Projectile.Center, EnigmaPurple.ToVector3() * 0.25f);
            
            // Ambient particles
            if (Main.GameUpdateCount % 3 == 0)
            {
                WatchingParticleHandler.Spawn(new PhantomWispParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    8f,
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    WatchingUtils.RefrainPurple,
                    Main.rand.NextFloat(0.2f, 0.3f),
                    40));
            }
            if (Main.GameUpdateCount % 10 == 0)
            {
                Vector2 eyeOffset = new Vector2(0, Main.rand.NextBool() ? -16f : 16f);
                WatchingParticleHandler.Spawn(new WatchingEyeParticle(
                    Projectile.Center + eyeOffset,
                    Vector2.Zero,
                    WatchingUtils.GazeGreen,
                    0.15f,
                    20));
            }
            if (Main.GameUpdateCount % 5 == 0)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    ModContent.DustType<WatchingPhantomDust>(), 0f, -0.5f);
            }
        }
        
        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<UnsolvedPhantomBuff>());
                Projectile.Kill();
                return false;
            }
            if (owner.HasBuff(ModContent.BuffType<UnsolvedPhantomBuff>()))
            {
                Projectile.timeLeft = 2;
            }
            return true;
        }
        
        private NPC FindTarget(Player owner)
        {
            NPC bestTarget = null;
            float bestDist = 800f;
            
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
            
            return bestTarget;
        }
        
        private void AttackTarget(NPC target)
        {
            SoundEngine.PlaySound(SoundID.Item125 with { Pitch = 0.3f, Volume = 0.4f }, Projectile.Center);
            
            Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 12f;
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, toTarget,
                ModContent.ProjectileType<PhantomBolt>(),
                Projectile.damage, Projectile.knockBack, Projectile.owner);
        }
        
        private void CreateMysteryZone(NPC target)
        {
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.2f, Volume = 0.5f }, target.Center);
            
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero,
                ModContent.ProjectileType<MysteryZone>(),
                (int)(Projectile.damage * 0.4f), 0f, Projectile.owner);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 480);
            target.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(target, 1);
            
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.4f);
        }
    }
    
    public class PhantomBolt : ModProjectile
    {
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private List<int> hitEnemies = new List<int>();
        private List<Vector2> _trailPositions = new List<Vector2>(30);
        
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/MusicNote";
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float time = (float)Main.GameUpdateCount;

            // === Stage 1: GPU Primitive Trail via WatchingPrimitiveRenderer ===
            if (_trailPositions.Count >= 2)
            {
                try
                {
                    sb.End();
                    // Pass 1: Body trail — RefrainPurple fading to GazeGreen
                    var bodySettings = new WatchingPrimitiveSettings(
                        completion => MathHelper.Lerp(12f, 2f, completion),
                        completion => Color.Lerp(WatchingUtils.RefrainPurple, WatchingUtils.GazeGreen, completion) * (1f - completion * 0.7f),
                        ShaderLoader.WatchingPhantomAura,
                        smoothing: true, maxPoints: 100);
                    WatchingPrimitiveRenderer.RenderTrail(_trailPositions, bodySettings);

                    // Pass 2: Outer glow — WatcherDeep ambient
                    var glowSettings = new WatchingPrimitiveSettings(
                        completion => MathHelper.Lerp(20f, 4f, completion),
                        completion => WatchingUtils.WatcherDeep * (0.30f * (1f - completion)),
                        ShaderLoader.WatchingPhantomAura,
                        smoothing: true, maxPoints: 100);
                    WatchingPrimitiveRenderer.RenderTrail(_trailPositions, glowSettings);

                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                        DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
                catch
                {
                    try { sb.End(); } catch { }
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                        DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }

            // === Stage 2: 6-layer bloom stack (Additive) ===
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            Texture2D bloom = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            Vector2 bOrigin = bloom.Size() / 2f;
            float pulse = 0.85f + MathF.Sin(time * 0.08f) * 0.15f;

            sb.Draw(bloom, drawPos, null, WatchingUtils.WatcherDeep * 0.28f * pulse, 0f, bOrigin, 0.026f, SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null, WatchingUtils.RefrainPurple * 0.45f * pulse, 0f, bOrigin, 0.018f, SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null, WatchingUtils.GazeGreen * 0.50f * pulse, 0f, bOrigin, 0.012f, SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null, WatchingUtils.SpectralMint * 0.35f * pulse, 0f, bOrigin, 0.0065f, SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null, WatchingUtils.PhantomWhite * 0.50f * pulse, 0f, bOrigin, 0.003f, SpriteEffects.None, 0f);

            // === Stage 3: EN Star Flare — dual-rotating spectral burst ===
            {
                Texture2D sfTex = EnigmaThemeTextures.ENStarFlare?.Value;
                if (sfTex != null)
                {
                    Vector2 sfOrigin = sfTex.Size() / 2f;
                    float sfRotA = time * 0.05f;
                    float sfRotB = -time * 0.035f;
                    float sfScale = 0.056f + MathF.Sin(time * 0.07f) * 0.0105f;
                    sb.Draw(sfTex, drawPos, null, WatchingUtils.GazeGreen * 0.35f, sfRotA, sfOrigin, sfScale, SpriteEffects.None, 0f);
                    sb.Draw(sfTex, drawPos, null, WatchingUtils.RefrainPurple * 0.22f, sfRotB, sfOrigin, sfScale * 0.85f, SpriteEffects.None, 0f);
                }
            }

            // === Stage 4: EN Power Effect Ring ===
            {
                Texture2D ringTex = EnigmaThemeTextures.ENPowerEffectRing?.Value;
                if (ringTex != null)
                {
                    Vector2 prOrigin = ringTex.Size() / 2f;
                    float prRot = time * 0.025f;
                    sb.Draw(ringTex, drawPos, null, WatchingUtils.RefrainPurple * 0.18f, prRot, prOrigin, 0.035f, SpriteEffects.None, 0f);
                }
            }

            // === Stage 5: Glyph sprite overlay ===
            {
                Texture2D glyphTex = TextureAssets.Projectile[Projectile.type].Value;
                Vector2 glyphOrigin = glyphTex.Size() / 2f;
                sb.Draw(glyphTex, drawPos, null, WatchingUtils.PhantomWhite * 0.65f, Projectile.rotation, glyphOrigin, Projectile.scale * 0.7f, SpriteEffects.None, 0f);
            }

            // === Pulsing light accent ===
            EnigmaVFXLibrary.AddPulsingLight(Projectile.Center, WatchingUtils.GazeGreen, 0.5f, 0.3f);
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
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Record trail positions for GPU primitive rendering
            _trailPositions.Insert(0, Projectile.Center);
            if (_trailPositions.Count > 25) _trailPositions.RemoveAt(_trailPositions.Count - 1);
            
            Lighting.AddLight(Projectile.Center, EnigmaPurple.ToVector3() * 0.2f);
            
            // Trail particles — every frame
            WatchingParticleHandler.Spawn(new PhantomBoltTrailMote(
                Projectile.Center + Main.rand.NextVector2Circular(2f, 2f),
                -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.3f, 0.3f),
                Color.Lerp(WatchingUtils.GazeGreen, WatchingUtils.SpectralMint, Main.rand.NextFloat()),
                Main.rand.NextFloat(0.1f, 0.15f),
                15));
            
            if (Main.GameUpdateCount % 3 == 0)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    ModContent.DustType<WatchingPhantomDust>(), Projectile.velocity.X * -0.15f, Projectile.velocity.Y * -0.15f);
            }
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 480);
            target.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(target, 1);
            
            // Impact VFX
            WatchingParticleHandler.Spawn(new MysteryZoneRipple(
                target.Center,
                WatchingUtils.RefrainPurple,
                40f,
                25));
            int burstCount = Main.rand.Next(5, 9);
            for (int i = 0; i < burstCount; i++)
            {
                WatchingParticleHandler.Spawn(new PhantomBoltTrailMote(
                    target.Center,
                    Main.rand.NextVector2CircularEdge(3f, 3f),
                    Color.Lerp(WatchingUtils.GazeGreen, WatchingUtils.SpectralMint, Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.1f, 0.22f),
                    18));
            }
            // Eye witness on impact
            WatchingParticleHandler.Spawn(new WatchingEyeParticle(
                target.Center + new Vector2(0, -24f),
                new Vector2(0, -0.3f),
                WatchingUtils.GazeGreen,
                0.2f,
                25));
            // Second ripple for layered impact
            WatchingParticleHandler.Spawn(new MysteryZoneRipple(
                target.Center,
                WatchingUtils.GazeGreen * 0.7f,
                25f,
                18));
            
            // Chain damage to one nearby enemy
            if (!hitEnemies.Contains(target.whoAmI))
            {
                hitEnemies.Add(target.whoAmI);
                
                float chainRange = 200f;
                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (npc.friendly || npc.whoAmI == target.whoAmI) continue;
                    if (Vector2.Distance(npc.Center, target.Center) > chainRange) continue;
                    
                    npc.SimpleStrikeNPC((int)(Projectile.damage * 0.3f), 0, false, 0f, null, false, 0f, true);
                    npc.AddBuff(ModContent.BuffType<ParadoxBrand>(), 480);
                    break;
                }
            }
            
            // Spawn a rift at the impact point
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero,
                ModContent.ProjectileType<PhantomRift>(),
                (int)(Projectile.damage * 0.4f), 0f, Projectile.owner);
            
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.5f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // Death burst VFX
            int moteCount = Main.rand.Next(6, 10);
            for (int i = 0; i < moteCount; i++)
            {
                WatchingParticleHandler.Spawn(new PhantomBoltTrailMote(
                    Projectile.Center,
                    Main.rand.NextVector2CircularEdge(3f, 3f),
                    Color.Lerp(WatchingUtils.SpectralMint, WatchingUtils.GazeGreen, Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.1f, 0.2f),
                    20));
            }
            WatchingParticleHandler.Spawn(new PhantomWispParticle(
                Projectile.Center,
                6f,
                Main.rand.NextFloat(MathHelper.TwoPi),
                WatchingUtils.RefrainPurple,
                0.4f,
                45));
            // Death ripple
            WatchingParticleHandler.Spawn(new MysteryZoneRipple(
                Projectile.Center,
                WatchingUtils.GazeGreen * 0.8f,
                30f,
                20));
            for (int i = 0; i < 5; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    ModContent.DustType<WatchingPhantomDust>(), Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f));
            }
        }
    }
    
    public class PhantomRift : ModProjectile
    {
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarHard";
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float time = (float)Main.GameUpdateCount;
            float op = Projectile.Opacity;

            // === Stage 1: GPU Primitive Rift Ring ===
            {
                try
                {
                    sb.End();
                    int ringPoints = 28;
                    var ringPositions = new List<Vector2>(ringPoints + 1);
                    float riftRadius = 22f + MathF.Sin(time * 0.08f) * 4f;
                    for (int i = 0; i <= ringPoints; i++)
                    {
                        float angle = MathHelper.TwoPi * i / ringPoints;
                        float wobble = MathF.Sin(angle * 4f + time * 0.1f) * 4f;
                        ringPositions.Add(Projectile.Center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (riftRadius + wobble));
                    }

                    // Pass 1: Body ring — GazeGreen to RefrainPurple
                    var bodySettings = new WatchingPrimitiveSettings(
                        c => 5f + MathF.Sin(c * MathHelper.TwoPi * 3f + time * 0.08f) * 2f,
                        c => Color.Lerp(WatchingUtils.GazeGreen, WatchingUtils.RefrainPurple, c) * (0.55f * op),
                        ShaderLoader.WatchingPhantomAura,
                        smoothing: true, maxPoints: 100);
                    WatchingPrimitiveRenderer.RenderTrail(ringPositions, bodySettings);

                    // Pass 2: Outer glow — PhantomBlack ethereal
                    var glowSettings = new WatchingPrimitiveSettings(
                        c => 10f + MathF.Sin(c * MathHelper.TwoPi * 3f + time * 0.06f) * 3f,
                        c => WatchingUtils.PhantomBlack * (0.20f * op),
                        ShaderLoader.WatchingPhantomAura,
                        smoothing: true, maxPoints: 100);
                    WatchingPrimitiveRenderer.RenderTrail(ringPositions, glowSettings);

                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                        DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
                catch
                {
                    try { sb.End(); } catch { }
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                        DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }

            // === Stage 2: 6-layer bloom stack (Additive) ===
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            Texture2D bloom = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            Vector2 bOrigin = bloom.Size() / 2f;
            float pulse = 0.80f + MathF.Sin(time * 0.06f) * 0.20f;
            float antiPulse = 0.80f + MathF.Sin(time * 0.06f + MathHelper.Pi) * 0.20f;

            sb.Draw(bloom, drawPos, null, WatchingUtils.WatcherDeep * 0.30f * antiPulse * op, -time * 0.005f, bOrigin, 0.035f, SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null, WatchingUtils.RefrainPurple * 0.40f * pulse * op, 0f, bOrigin, 0.025f, SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null, WatchingUtils.GazeGreen * 0.50f * antiPulse * op, 0f, bOrigin, 0.016f, SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null, WatchingUtils.SpectralMint * 0.35f * pulse * op, 0f, bOrigin, 0.009f, SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null, WatchingUtils.PhantomWhite * 0.45f * op, 0f, bOrigin, 0.0045f, SpriteEffects.None, 0f);

            // === Stage 3: EN Star Flare — dual-rotating rift starburst ===
            {
                Texture2D sfTex = EnigmaThemeTextures.ENStarFlare?.Value;
                if (sfTex != null)
                {
                    Vector2 sfOrigin = sfTex.Size() / 2f;
                    float sfRotA = time * 0.02f;
                    float sfRotB = -time * 0.015f;
                    float sfScale = 0.105f + MathF.Sin(time * 0.05f) * 0.021f;
                    sb.Draw(sfTex, drawPos, null, WatchingUtils.GazeGreen * 0.35f * op, sfRotA, sfOrigin, sfScale * pulse, SpriteEffects.None, 0f);
                    sb.Draw(sfTex, drawPos, null, WatchingUtils.RefrainPurple * 0.25f * op, sfRotB, sfOrigin, sfScale * 0.8f * antiPulse, SpriteEffects.None, 0f);
                }
            }

            // === Stage 4: EN Power Effect Ring — dual concentric rift rings ===
            {
                Texture2D ringTex = EnigmaThemeTextures.ENPowerEffectRing?.Value;
                if (ringTex != null)
                {
                    Vector2 prOrigin = ringTex.Size() / 2f;
                    float prRot = time * 0.01f;
                    sb.Draw(ringTex, drawPos, null, WatchingUtils.GazeGreen * 0.22f * op, prRot, prOrigin, 0.0875f + MathF.Sin(time * 0.07f) * 0.014f, SpriteEffects.None, 0f);
                    sb.Draw(ringTex, drawPos, null, WatchingUtils.RefrainPurple * 0.15f * op, -prRot * 0.6f, prOrigin, 0.1225f + MathF.Sin(time * 0.05f) * 0.0175f, SpriteEffects.None, 0f);
                }
            }

            // === Stage 5: EN Enigma Eye — rift gaze ===
            {
                Texture2D eyeTex = EnigmaThemeTextures.ENEnigmaEye?.Value;
                if (eyeTex != null)
                {
                    Vector2 eyeOrigin = eyeTex.Size() / 2f;
                    float eyePulse = 0.5f + MathF.Sin(time * 0.04f) * 0.5f;
                    sb.Draw(eyeTex, drawPos, null, WatchingUtils.PhantomWhite * 0.35f * eyePulse * op, 0f, eyeOrigin, 0.035f, SpriteEffects.None, 0f);
                }
            }

            // === Stage 6: Rift star sprite ===
            {
                Texture2D riftTex = TextureAssets.Projectile[Projectile.type].Value;
                Vector2 riftOrigin = riftTex.Size() / 2f;
                sb.Draw(riftTex, drawPos, null, WatchingUtils.RefrainPurple * 0.45f * op, time * 0.007f, riftOrigin, Projectile.scale * 0.55f, SpriteEffects.None, 0f);
            }

            // === Pulsing light accent ===
            EnigmaVFXLibrary.AddPulsingLight(Projectile.Center, WatchingUtils.GazeGreen, 0.6f * op, 0.35f);
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
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }
        
        public override void AI()
        {
            float lifeProgress = 1f - (Projectile.timeLeft / 60f);
            float opacity = (float)Math.Sin(lifeProgress * MathHelper.Pi);
            Projectile.Opacity = opacity;
            
            Lighting.AddLight(Projectile.Center, EnigmaGreen.ToVector3() * opacity * 0.3f);
            
            // Ambient rift particles
            if (Main.GameUpdateCount % 2 == 0)
            {
                WatchingParticleHandler.Spawn(new PhantomWispParticle(
                    Projectile.Center,
                    20f,
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    WatchingUtils.RefrainPurple,
                    Main.rand.NextFloat(0.15f, 0.25f),
                    30));
            }
            if (Main.GameUpdateCount % 6 == 0)
            {
                WatchingParticleHandler.Spawn(new WatchingEyeParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(25f, 25f),
                    Vector2.Zero,
                    WatchingUtils.GazeGreen,
                    0.15f,
                    20));
            }
            if (Main.GameUpdateCount % 4 == 0)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    ModContent.DustType<WatchingPhantomDust>(), 0f, -0.3f);
            }
            if (Main.GameUpdateCount % 10 == 0)
            {
                WatchingParticleHandler.Spawn(new MysteryZoneRipple(
                    Projectile.Center,
                    WatchingUtils.RefrainPurple * 0.6f,
                    40f,
                    30));
            }
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 480);
            target.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(target, 1);
            
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.3f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // Rift collapse VFX
            WatchingParticleHandler.Spawn(new MysteryZoneRipple(
                Projectile.Center,
                WatchingUtils.PhantomWhite,
                60f,
                35));
            // Double ripple for dramatic collapse
            WatchingParticleHandler.Spawn(new MysteryZoneRipple(
                Projectile.Center,
                WatchingUtils.GazeGreen * 0.8f,
                40f,
                25));
            int wispCount = Main.rand.Next(8, 12);
            for (int i = 0; i < wispCount; i++)
            {
                WatchingParticleHandler.Spawn(new PhantomWispParticle(
                    Projectile.Center,
                    15f,
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    Color.Lerp(WatchingUtils.RefrainPurple, WatchingUtils.GazeGreen, Main.rand.NextFloat(0.2f, 0.5f)),
                    Main.rand.NextFloat(0.2f, 0.4f),
                    35));
            }
            // Eye witness on rift death
            WatchingParticleHandler.Spawn(new WatchingEyeParticle(
                Projectile.Center,
                new Vector2(0, -0.5f),
                WatchingUtils.GazeGreen,
                0.25f,
                20));
            for (int i = 0; i < 7; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    ModContent.DustType<WatchingPhantomDust>(), Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-1.5f, 1.5f));
            }
        }
    }
    
    public class MysteryZone : ModProjectile
    {
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private int damageTimer = 0;
        
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle";
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float time = (float)Main.GameUpdateCount;
            float lifeFade = MathHelper.Clamp(Projectile.timeLeft / 60f, 0f, 1f);
            float zoneScale = Projectile.width / 64f;

            // === Stage 1: GPU Primitive Boundary Ring ===
            {
                try
                {
                    sb.End();
                    int ringPoints = 48;
                    var ringPositions = new List<Vector2>(ringPoints + 1);
                    float boundaryRadius = Projectile.width * 0.48f;
                    for (int i = 0; i <= ringPoints; i++)
                    {
                        float angle = MathHelper.TwoPi * i / ringPoints;
                        float wobble = MathF.Sin(angle * 5f + time * 0.04f) * 5f;
                        ringPositions.Add(Projectile.Center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (boundaryRadius + wobble));
                    }

                    // Pass 1: Body ring — RefrainPurple to GazeGreen boundary
                    var bodySettings = new WatchingPrimitiveSettings(
                        c => 4f + MathF.Sin(c * MathHelper.TwoPi * 4f + time * 0.05f) * 2f,
                        c => Color.Lerp(WatchingUtils.RefrainPurple, WatchingUtils.GazeGreen, c) * (0.35f * lifeFade),
                        ShaderLoader.WatchingMysteryZone,
                        smoothing: true, maxPoints: 150);
                    WatchingPrimitiveRenderer.RenderTrail(ringPositions, bodySettings);

                    // Pass 2: Outer glow — WatcherDeep atmospheric haze
                    var glowSettings = new WatchingPrimitiveSettings(
                        c => 8f + MathF.Sin(c * MathHelper.TwoPi * 4f + time * 0.03f) * 3f,
                        c => WatchingUtils.WatcherDeep * (0.18f * lifeFade),
                        ShaderLoader.WatchingMysteryZone,
                        smoothing: true, maxPoints: 150);
                    WatchingPrimitiveRenderer.RenderTrail(ringPositions, glowSettings);

                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                        DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
                catch
                {
                    try { sb.End(); } catch { }
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                        DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }

            // === Stage 2: Shader overlay — panopticon surveillance grid ===
            {
                Texture2D shBloom = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
                EnigmaShaderHelper.DrawShaderOverlay(sb, ShaderLoader.WatchingMysteryZone,
                    shBloom, drawPos, shBloom.Size() / 2f, MathHelper.Min(zoneScale * 0.14f, 0.139f),
                    WatchingUtils.GazeGreen.ToVector3(), WatchingUtils.RefrainPurple.ToVector3(),
                    opacity: 0.4f * lifeFade, intensity: 1.0f,
                    noiseTexture: ShaderLoader.GetNoiseTexture("VoronoiNoise"),
                    techniqueName: "WatchingMysteryField");
            }

            // === Stage 3: 6-layer bloom stack (Additive) ===
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            Texture2D bloom = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            Vector2 bOrigin = bloom.Size() / 2f;
            float pulse = 0.85f + MathF.Sin(time * 0.04f) * 0.15f;
            float slowRot = time * 0.008f;

            sb.Draw(bloom, drawPos, null, WatchingUtils.WatcherDeep * 0.20f * lifeFade * pulse, -slowRot * 0.7f, bOrigin, MathHelper.Min(zoneScale * 0.065f, 0.139f), SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null, WatchingUtils.RefrainPurple * 0.25f * lifeFade * pulse, 0f, bOrigin, MathHelper.Min(zoneScale * 0.052f, 0.139f), SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null, WatchingUtils.GazeGreen * 0.30f * lifeFade * pulse, 0f, bOrigin, MathHelper.Min(zoneScale * 0.037f, 0.139f), SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null, WatchingUtils.SpectralMint * 0.18f * lifeFade * pulse, 0f, bOrigin, MathHelper.Min(zoneScale * 0.021f, 0.139f), SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null, WatchingUtils.PhantomWhite * 0.12f * lifeFade, 0f, bOrigin, MathHelper.Min(zoneScale * 0.011f, 0.139f), SpriteEffects.None, 0f);

            // === Stage 4: EN Power Effect Ring — dual concentric zone boundaries ===
            {
                Texture2D ringTex = EnigmaThemeTextures.ENPowerEffectRing?.Value;
                if (ringTex != null)
                {
                    Vector2 prOrigin = ringTex.Size() / 2f;
                    float prRot = time * 0.005f;
                    float ringPulse = zoneScale * (0.49f + MathF.Sin(time * 0.04f) * 0.0525f);
                    sb.Draw(ringTex, drawPos, null, WatchingUtils.RefrainPurple * 0.18f * lifeFade, prRot, prOrigin, ringPulse, SpriteEffects.None, 0f);
                    sb.Draw(ringTex, drawPos, null, WatchingUtils.GazeGreen * 0.12f * lifeFade, -prRot * 0.6f, prOrigin, ringPulse * 0.7f, SpriteEffects.None, 0f);
                }
            }

            // === Stage 5: EN Star Flare — zone center flare ===
            {
                Texture2D sfTex = EnigmaThemeTextures.ENStarFlare?.Value;
                if (sfTex != null)
                {
                    Vector2 sfOrigin = sfTex.Size() / 2f;
                    float sfRot = time * 0.012f;
                    sb.Draw(sfTex, drawPos, null, WatchingUtils.GazeGreen * 0.20f * lifeFade, sfRot, sfOrigin, 0.0875f, SpriteEffects.None, 0f);
                }
            }

            // === Stage 6: EN Enigma Eye — zone surveillance eye ===
            {
                Texture2D eyeTex = EnigmaThemeTextures.ENEnigmaEye?.Value;
                if (eyeTex != null)
                {
                    Vector2 eyeOrigin = eyeTex.Size() / 2f;
                    float eyePulse = 0.5f + MathF.Sin(time * 0.035f) * 0.5f;
                    sb.Draw(eyeTex, drawPos, null, WatchingUtils.PhantomWhite * 0.25f * eyePulse * lifeFade, 0f, eyeOrigin, 0.049f, SpriteEffects.None, 0f);
                }
            }

            // === Pulsing light accent ===
            EnigmaVFXLibrary.AddPulsingLight(Projectile.Center, WatchingUtils.RefrainPurple, 0.5f * lifeFade, 0.25f);
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
            Projectile.width = 160;
            Projectile.height = 160;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }
        
        public override bool? CanDamage() => false;
        
        public override void AI()
        {
            damageTimer++;
            if (damageTimer >= 20)
            {
                damageTimer = 0;
                DealZoneDamage();
            }
            
            Lighting.AddLight(Projectile.Center, EnigmaPurple.ToVector3() * 0.2f);
            
            float zoneRadius = Projectile.width / 2f;
            
            // Ambient zone particles
            if (Main.GameUpdateCount % 3 == 0)
            {
                Vector2 randomPos = Projectile.Center + Main.rand.NextVector2Circular(zoneRadius * 0.8f, zoneRadius * 0.8f);
                WatchingParticleHandler.Spawn(new MysteryZoneRipple(
                    randomPos,
                    WatchingUtils.RefrainPurple * 0.5f,
                    Main.rand.NextFloat(30f, 50f),
                    25));
            }
            if (Main.GameUpdateCount % 6 == 0)
            {
                Vector2 driftPos = Projectile.Center + Main.rand.NextVector2Circular(zoneRadius * 0.6f, zoneRadius * 0.6f);
                WatchingParticleHandler.Spawn(new PhantomWispParticle(
                    driftPos,
                    10f,
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    WatchingUtils.GazeGreen,
                    Main.rand.NextFloat(0.15f, 0.25f),
                    30));
            }
            if (Main.GameUpdateCount % 8 == 0)
            {
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(zoneRadius * 0.7f, zoneRadius * 0.7f);
                Dust.NewDust(dustPos - new Vector2(4f), 8, 8,
                    ModContent.DustType<WatchingPhantomDust>(), 0f, -0.2f);
            }
        }
        
        private void DealZoneDamage()
        {
            float zoneRadius = Projectile.width / 2f;
            
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly) continue;
                float dist = Vector2.Distance(npc.Center, Projectile.Center);
                if (dist > zoneRadius) continue;
                
                // Damage
                npc.SimpleStrikeNPC(Projectile.damage, 0, false, 0f, null, false, 0f, true);
                npc.AddBuff(ModContent.BuffType<ParadoxBrand>(), 480);
                npc.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(npc, 1);
                
                // Slow effect (reduce velocity)
                npc.velocity *= 0.85f;
                
                // Pull toward center
                Vector2 pullDir = (Projectile.Center - npc.Center).SafeNormalize(Vector2.Zero);
                npc.velocity += pullDir * 1.5f;
            }
        }
        
        public override void OnKill(int timeLeft)
        {
            // Zone dissipation VFX
            WatchingParticleHandler.Spawn(new MysteryZoneRipple(
                Projectile.Center,
                WatchingUtils.PhantomWhite,
                80f,
                40));
            // Double expanding ripple on zone death
            WatchingParticleHandler.Spawn(new MysteryZoneRipple(
                Projectile.Center,
                WatchingUtils.RefrainPurple * 0.7f,
                60f,
                30));
            int wispCount = Main.rand.Next(7, 11);
            for (int i = 0; i < wispCount; i++)
            {
                WatchingParticleHandler.Spawn(new PhantomWispParticle(
                    Projectile.Center,
                    25f,
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    Color.Lerp(WatchingUtils.RefrainPurple, WatchingUtils.GazeGreen, Main.rand.NextFloat(0.2f, 0.5f)),
                    Main.rand.NextFloat(0.2f, 0.4f),
                    40));
            }
            // Eye particles on zone collapse
            for (int i = 0; i < Main.rand.Next(2, 4); i++)
            {
                Vector2 eyePos = Projectile.Center + Main.rand.NextVector2Circular(40f, 40f);
                WatchingParticleHandler.Spawn(new WatchingEyeParticle(
                    eyePos,
                    new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.4f),
                    WatchingUtils.GazeGreen,
                    0.18f,
                    22));
            }
            for (int i = 0; i < 6; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    ModContent.DustType<WatchingPhantomDust>(), Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1.5f, 0f));
            }
        }
    }
}
