using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Graphics;
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
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheSilentMeasure.Particles;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheSilentMeasure.Dusts;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheSilentMeasure.Utilities;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheSilentMeasure.Primitives;
using MagnumOpus.Content.EnigmaVariations;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheSilentMeasure
{
    /// <summary>
    /// THE SILENT MEASURE — Ranged bow weapon (Enigma Variations theme).
    /// A measure of silence — arrows that ask questions and seek answers.
    /// 
    /// Normal shots: QuestionSeekerBolt (pierce 2), apply ParadoxBrand.
    /// On first hit: arrow splits into 3 homing QuestionSeekers (50% damage).
    /// Chain lightning damage on QuestionSeekerBolt hit (30-40% to nearest).
    /// Every 5th shot fires ParadoxPiercingBolt (2x damage, pierce 5, chain to 3).
    /// SeekingCrystals (2 per arrow, 20% damage).
    /// 
    /// Custom Shaders: SilentSeekerTrail.fx, SilentQuestionBurst.fx
    /// Foundation: RibbonFoundation + SparkleProjectileFoundation + ThinLaserFoundation planned
    /// </summary>
    public class TheSilentMeasure : ModItem
    {
        private int shotCounter = 0;
        
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        public override string Texture => "MagnumOpus/Content/EnigmaVariations/ResonantWeapons/TheSilentMeasure/TheSilentMeasure";
        
        public override void SetDefaults()
        {
            Item.damage = 245;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 30;
            Item.height = 60;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3.5f;
            Item.value = Item.sellPrice(gold: 20);
            Item.rare = ModContent.RarityType<EnigmaVariationsRarity>();
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<QuestionSeekerBolt>();
            Item.shootSpeed = 16f;
            Item.noMelee = true;
            Item.useAmmo = AmmoID.Arrow;
        }
        
        public override void HoldItem(Player player)
        {
            Lighting.AddLight(player.Center, EnigmaPurple.ToVector3() * 0.1f);
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Arrows split into 3 homing seekers on first hit"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Every 5th shot fires a Paradox Piercing Bolt with chain lightning"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Hits chain lightning damage to nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Spawns homing seeking crystals from arrow impacts"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Hits brand enemies with Paradox Brand"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The question has weight. The answer, none.'")
            {
                OverrideColor = EnigmaPurple
            });
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            shotCounter++;
            bool isParadoxArrow = (shotCounter % 5 == 0);
            
            if (isParadoxArrow)
            {
                int paradoxDamage = (int)(damage * 2.0f);
                Projectile.NewProjectile(source, position, velocity,
                    ModContent.ProjectileType<ParadoxPiercingBolt>(),
                    paradoxDamage, knockback * 2f, player.whoAmI);
                    
                SoundEngine.PlaySound(SoundID.Item125 with { Pitch = -0.3f, Volume = 0.7f }, position);
            }
            else
            {
                Projectile.NewProjectile(source, position, velocity,
                    ModContent.ProjectileType<QuestionSeekerBolt>(),
                    damage, knockback, player.whoAmI);
            }
            
            return false;
        }
    }
    
    public class QuestionSeekerBolt : ModProjectile
    {
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private bool hasSplit = false;
        private List<int> recentlyHitEnemies = new List<int>();
        private int eyeTextureIndex = 0;
        private List<Vector2> _trailPositions = new List<Vector2>(30);
        
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/MusicNote";
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float time = (float)Main.GameUpdateCount;

            // === Stage 1: GPU Primitive Trail via SilentPrimitiveRenderer ===
            if (_trailPositions.Count >= 2)
            {
                try
                {
                    sb.End();
                    // Pass 1: Body trail — QuestionViolet fading to EnigmaEmerald
                    var bodySettings = new SilentPrimitiveSettings(
                        completion => MathHelper.Lerp(14f, 2f, completion),
                        completion => Color.Lerp(SilentUtils.QuestionViolet, SilentUtils.EnigmaEmerald, completion) * (1f - completion * 0.7f),
                        ShaderLoader.SilentSeekerTrail,
                        smoothing: true, maxPoints: 100);
                    SilentPrimitiveRenderer.RenderTrail(_trailPositions, bodySettings);

                    // Pass 2: Outer glow — HushedDepth ambient
                    var glowSettings = new SilentPrimitiveSettings(
                        completion => MathHelper.Lerp(22f, 4f, completion),
                        completion => SilentUtils.HushedDepth * (0.35f * (1f - completion)),
                        ShaderLoader.SilentSeekerTrail,
                        smoothing: true, maxPoints: 100);
                    SilentPrimitiveRenderer.RenderTrail(_trailPositions, glowSettings);

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

            // === Stage 2: Shader overlay — phantom echo seeker signature ===
            {
                Texture2D shBloom = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
                EnigmaShaderHelper.DrawShaderOverlay(sb, ShaderLoader.SilentSeekerTrail,
                    shBloom, drawPos, shBloom.Size() / 2f, 0.045f,
                    SilentUtils.QuestionViolet.ToVector3(), SilentUtils.EnigmaEmerald.ToVector3(),
                    opacity: 0.45f, intensity: 1.0f, rotation: Projectile.velocity.ToRotation(),
                    noiseTexture: ShaderLoader.GetNoiseTexture("PerlinNoise"),
                    techniqueName: "SilentSeekerFlow");
            }

            // === Stage 3: 6-layer bloom stack (Additive) ===
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            Texture2D bloom = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            Vector2 bOrigin = bloom.Size() / 2f;
            float pulse = 0.85f + MathF.Sin(time * 0.07f) * 0.15f;

            sb.Draw(bloom, drawPos, null, SilentUtils.HushedDepth * 0.30f * pulse, 0f, bOrigin, new Vector2(0.04f * 0.7f, 0.04f), SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null, SilentUtils.QuestionViolet * 0.50f * pulse, 0f, bOrigin, new Vector2(0.028f * 0.7f, 0.028f), SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null, SilentUtils.EnigmaEmerald * 0.45f * pulse, 0f, bOrigin, new Vector2(0.018f * 0.7f, 0.018f), SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null, SilentUtils.BrightQuestion * 0.35f * pulse, 0f, bOrigin, new Vector2(0.010f * 0.7f, 0.010f), SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null, SilentUtils.AnswerWhite * 0.55f * pulse, 0f, bOrigin, new Vector2(0.005f * 0.7f, 0.005f), SpriteEffects.None, 0f);

            // === Stage 4: EN Star Flare ===
            {
                Texture2D sfTex = EnigmaThemeTextures.ENStarFlare.Value;
                Vector2 sfOrigin = sfTex.Size() / 2f;
                float sfRot = time * 0.035f;
                float sfScale = 0.14f + MathF.Sin(time * 0.06f) * 0.03f;
                sb.Draw(sfTex, drawPos, null, SilentUtils.QuestionViolet * 0.30f, sfRot, sfOrigin, sfScale, SpriteEffects.None, 0f);
            }

            // === Stage 5: EN Power Effect Ring — subtle rotating ring ===
            {
                Texture2D ringTex = EnigmaThemeTextures.ENPowerEffectRing.Value;
                Vector2 prOrigin = ringTex.Size() / 2f;
                float prRot = time * 0.02f;
                sb.Draw(ringTex, drawPos, null, SilentUtils.QuestionViolet * 0.18f, prRot, prOrigin, 0.10f, SpriteEffects.None, 0f);
            }

            // === Pulsing light accent ===
            EnigmaVFXLibrary.AddPulsingLight(Projectile.Center, SilentUtils.QuestionViolet, 0.6f, 0.3f);
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
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Record trail positions for GPU primitive rendering
            _trailPositions.Insert(0, Projectile.Center);
            if (_trailPositions.Count > 25) _trailPositions.RemoveAt(_trailPositions.Count - 1);
            
            // Cycle eye texture for visual variety
            if (Projectile.timeLeft % 30 == 0)
                eyeTextureIndex = (eyeTextureIndex + 1) % 3;
            
            Lighting.AddLight(Projectile.Center, EnigmaPurple.ToVector3() * 0.3f);
            
            // Trail particles
            if (Main.GameUpdateCount % 2 == 0)
            {
                SilentParticleHandler.Spawn(new SeekerTrailDot(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    SilentUtils.QuestionViolet,
                    Main.rand.NextFloat(0.1f, 0.2f),
                    20));
            }
            if (Main.GameUpdateCount % 4 == 0)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    ModContent.DustType<SilentMeasureDust>(), Projectile.velocity.X * -0.2f, Projectile.velocity.Y * -0.2f);
            }
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 480);
            target.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(target, 1);
            
            SeekingCrystalHelper.SpawnEnigmaCrystals(
                Projectile.GetSource_FromThis(),
                target.Center,
                Projectile.velocity,
                (int)(damageDone * 0.2f),
                5f,
                Projectile.owner,
                2
            );
            
            recentlyHitEnemies.Add(target.whoAmI);
            
            // Split into seekers on first hit
            if (!hasSplit)
            {
                hasSplit = true;
                SplitIntoSeekers(target);
            }
            
            // Chain lightning damage
            DrawChainLightning(target);
            
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.5f);
            
            // Impact VFX
            SilentParticleHandler.Spawn(new MeasureImpactRing(target.Center, SilentUtils.QuestionViolet, 0.3f, 20));
            for (int i = 0; i < Main.rand.Next(3, 6); i++)
            {
                SilentParticleHandler.Spawn(new SeekerTrailDot(
                    target.Center + Main.rand.NextVector2Circular(12f, 12f),
                    SilentUtils.QuestionViolet,
                    Main.rand.NextFloat(0.1f, 0.2f),
                    15));
            }
            SilentParticleHandler.Spawn(new QuestionMarkParticle(
                target.Center + new Vector2(0f, -24f), new Vector2(0f, -0.5f),
                SilentUtils.AnswerWhite, 0.4f, 40));
        }
        
        private void SplitIntoSeekers(NPC hitTarget)
        {
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 0.5f }, Projectile.Center);
            
            List<NPC> nearbyTargets = FindNearestEnemies(hitTarget.Center, 600f, 3);
            
            for (int i = 0; i < 3; i++)
            {
                Vector2 seekerVel;
                if (i < nearbyTargets.Count)
                {
                    seekerVel = (nearbyTargets[i].Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 10f;
                }
                else
                {
                    float angle = MathHelper.TwoPi / 3f * i;
                    seekerVel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 10f;
                }
                
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, seekerVel,
                    ModContent.ProjectileType<HomingQuestionSeeker>(),
                    (int)(Projectile.damage * 0.5f), 3f, Projectile.owner);
            }
        }
        
        private List<NPC> FindNearestEnemies(Vector2 position, float range, int count)
        {
            List<NPC> enemies = new List<NPC>();
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly) continue;
                if (Vector2.Distance(npc.Center, position) <= range)
                    enemies.Add(npc);
            }
            enemies.Sort((a, b) => Vector2.Distance(a.Center, position).CompareTo(Vector2.Distance(b.Center, position)));
            return enemies.Take(count).ToList();
        }
        
        private void DrawChainLightning(NPC target)
        {
            // Chain damage to one nearby enemy (the "lightning" visual is gutted, damage remains)
            float chainRange = 200f;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly || npc.whoAmI == target.whoAmI) continue;
                if (Vector2.Distance(npc.Center, target.Center) > chainRange) continue;
                
                npc.SimpleStrikeNPC((int)(Projectile.damage * 0.3f), 0, false, 0f, null, false, 0f, true);
                npc.AddBuff(ModContent.BuffType<ParadoxBrand>(), 480);
                
                // Chain lightning VFX between target and chained enemy
                Vector2 chainStart = target.Center;
                Vector2 chainEnd = npc.Center;
                int chainSegs = Main.rand.Next(6, 11);
                for (int s = 0; s < chainSegs; s++)
                {
                    float t = s / (float)(chainSegs - 1);
                    Vector2 segPos = Vector2.Lerp(chainStart, chainEnd, t);
                    Vector2 perp = (chainEnd - chainStart).SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2);
                    segPos += perp * Main.rand.NextFloat(-8f, 8f);
                    Color segCol = s % 2 == 0 ? SilentUtils.BrightQuestion : SilentUtils.AnswerWhite;
                    SilentParticleHandler.Spawn(new ChainLightningParticle(
                        segPos, (chainEnd - chainStart).SafeNormalize(Vector2.Zero) * 2f, segCol, 0.15f, 12));
                }
                break;
            }
        }
        
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < Main.rand.Next(3, 6); i++)
            {
                SilentParticleHandler.Spawn(new SeekerTrailDot(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    SilentUtils.QuestionViolet,
                    Main.rand.NextFloat(0.1f, 0.2f),
                    20));
            }
            SilentParticleHandler.Spawn(new QuestionMarkParticle(
                Projectile.Center, new Vector2(0f, -0.6f),
                SilentUtils.QuestionViolet, 0.3f, 30));
            for (int i = 0; i < Main.rand.Next(2, 4); i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    ModContent.DustType<SilentMeasureDust>(),
                    Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f));
            }
        }
    }
    
    public class HomingQuestionSeeker : ModProjectile
    {
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private int eyeTextureIndex = 0;
        private List<Vector2> _trailPositions = new List<Vector2>(30);
        private VertexStrip _strip;
        
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/CursiveMusicNote";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            IncisorOrbRenderer.DrawOrbVisuals(Main.spriteBatch, Projectile, IncisorOrbRenderer.Enigma, ref _strip);
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
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Record trail positions for GPU primitive rendering
            _trailPositions.Insert(0, Projectile.Center);
            if (_trailPositions.Count > 25) _trailPositions.RemoveAt(_trailPositions.Count - 1);
            
            // Cycle eye texture
            if (Projectile.timeLeft % 20 == 0)
                eyeTextureIndex = (eyeTextureIndex + 1) % 3;
            
            // Homing
            NPC target = null;
            if (Projectile.ai[0] > 0)
            {
                int targetIndex = (int)Projectile.ai[0] - 1;
                if (targetIndex >= 0 && targetIndex < Main.maxNPCs && Main.npc[targetIndex].active && !Main.npc[targetIndex].friendly)
                    target = Main.npc[targetIndex];
            }
            
            if (target == null)
                target = FindClosestEnemy(600f);
            
            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 14f, 0.06f);
            }
            
            Lighting.AddLight(Projectile.Center, EnigmaGreen.ToVector3() * 0.2f);
            
            // Trail particles — every frame
            SilentParticleHandler.Spawn(new SeekerTrailDot(
                Projectile.Center + Main.rand.NextVector2Circular(3f, 3f),
                SilentUtils.EnigmaEmerald,
                0.1f,
                15));
            
            // Occasional question mark
            if (Main.GameUpdateCount % 5 == 0)
            {
                SilentParticleHandler.Spawn(new QuestionMarkParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    new Vector2(0f, -0.3f),
                    SilentUtils.BrightQuestion, 0.15f, 20));
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
            for (int i = 0; i < Main.rand.Next(3, 6); i++)
            {
                SilentParticleHandler.Spawn(new SeekerTrailDot(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    SilentUtils.EnigmaEmerald,
                    Main.rand.NextFloat(0.1f, 0.2f),
                    18));
            }
            SilentParticleHandler.Spawn(new QuestionMarkParticle(
                Projectile.Center, new Vector2(0f, -0.5f),
                SilentUtils.EnigmaEmerald, 0.25f, 25));
            for (int i = 0; i < 2; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    ModContent.DustType<SilentMeasureDust>(),
                    Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f));
            }
        }
    }
    
    public class ParadoxPiercingBolt : ModProjectile
    {
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private int eyeTextureIndex = 0;
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

            // === Stage 1: GPU Primitive Trail — paradox bolt ribbon (3 passes) ===
            if (_trailPositions.Count >= 2)
            {
                try
                {
                    sb.End();
                    // Pass 1: Body trail — BrightQuestion to QuestionViolet (reversed gradient for paradox)
                    var bodySettings = new SilentPrimitiveSettings(
                        completion => MathHelper.Lerp(20f, 3f, completion),
                        completion => Color.Lerp(SilentUtils.BrightQuestion, SilentUtils.QuestionViolet, completion) * (1f - completion * 0.6f),
                        ShaderLoader.SilentSeekerTrail,
                        smoothing: true, maxPoints: 120);
                    SilentPrimitiveRenderer.RenderTrail(_trailPositions, bodySettings);

                    // Pass 2: Outer glow — HushedDepth wide ambient
                    var glowSettings = new SilentPrimitiveSettings(
                        completion => MathHelper.Lerp(32f, 6f, completion),
                        completion => SilentUtils.HushedDepth * (0.30f * (1f - completion)),
                        ShaderLoader.SilentSeekerTrail,
                        smoothing: true, maxPoints: 120);
                    SilentPrimitiveRenderer.RenderTrail(_trailPositions, glowSettings);

                    // Pass 3: Core filament — AnswerWhite to BrightQuestion inner thread
                    var coreSettings = new SilentPrimitiveSettings(
                        completion => MathHelper.Lerp(6f, 0.5f, completion),
                        completion => Color.Lerp(SilentUtils.AnswerWhite, SilentUtils.BrightQuestion, completion) * (0.8f * (1f - completion * 0.5f)),
                        ShaderLoader.SilentSeekerTrail,
                        smoothing: true, maxPoints: 120);
                    SilentPrimitiveRenderer.RenderTrail(_trailPositions, coreSettings);

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

            // === Stage 2: Shader overlay — paradox energy field ===
            {
                Texture2D shBloom = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
                EnigmaShaderHelper.DrawShaderOverlay(sb, ShaderLoader.SilentSeekerTrail,
                    shBloom, drawPos, shBloom.Size() / 2f, 1.0f,
                    SilentUtils.BrightQuestion.ToVector3(), SilentUtils.AnswerWhite.ToVector3(),
                    opacity: 0.5f, intensity: 1.2f, rotation: Projectile.velocity.ToRotation(),
                    noiseTexture: ShaderLoader.GetNoiseTexture("PerlinNoise"),
                    techniqueName: "SilentSeekerFlow");
            }

            // === Stage 3: 6-layer bloom stack (Additive) ===
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            Texture2D bloom = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            Vector2 bOrigin = bloom.Size() / 2f;
            float pulse = 0.85f + MathF.Sin(time * 0.06f) * 0.15f;

            sb.Draw(bloom, drawPos, null, SilentUtils.HushedDepth * 0.35f * pulse, 0f, bOrigin, new Vector2(0.139f * 0.7f, 0.139f), SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null, SilentUtils.QuestionViolet * 0.50f * pulse, 0f, bOrigin, new Vector2(0.10f * 0.7f, 0.10f), SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null, SilentUtils.EnigmaEmerald * 0.55f * pulse, 0f, bOrigin, new Vector2(0.07f * 0.7f, 0.07f), SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null, SilentUtils.BrightQuestion * 0.50f * pulse, 0f, bOrigin, new Vector2(0.05f * 0.7f, 0.05f), SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null, SilentUtils.AnswerWhite * 0.65f * pulse, 0f, bOrigin, new Vector2(0.03f * 0.7f, 0.03f), SpriteEffects.None, 0f);

            // === Stage 4: EN Star Flare — dual counter-rotating paradox flares ===
            {
                Texture2D sfTex = EnigmaThemeTextures.ENStarFlare.Value;
                Vector2 sfOrigin = sfTex.Size() / 2f;
                float sfRotA = time * 0.045f;
                float sfRotB = -time * 0.028f;
                float sfScale = 0.22f + MathF.Sin(time * 0.08f) * 0.04f;
                sb.Draw(sfTex, drawPos, null, SilentUtils.EnigmaEmerald * 0.45f, sfRotA, sfOrigin, sfScale, SpriteEffects.None, 0f);
                sb.Draw(sfTex, drawPos, null, SilentUtils.QuestionViolet * 0.30f, sfRotB, sfOrigin, sfScale * 0.85f, SpriteEffects.None, 0f);
            }

            // === Stage 5: EN Power Effect Ring — dual concentric void rings ===
            {
                Texture2D ringTex = EnigmaThemeTextures.ENPowerEffectRing.Value;
                Vector2 prOrigin = ringTex.Size() / 2f;
                float prRot = time * 0.03f;
                sb.Draw(ringTex, drawPos, null, SilentUtils.QuestionViolet * 0.30f, prRot, prOrigin, 0.18f, SpriteEffects.None, 0f);
                sb.Draw(ringTex, drawPos, null, SilentUtils.BrightQuestion * 0.20f, -prRot * 0.6f, prOrigin, 0.25f, SpriteEffects.None, 0f);
            }

            // === Stage 6: EN Enigma Eye — always visible for paradox bolt ===
            {
                Texture2D eyeTex = EnigmaThemeTextures.ENEnigmaEye.Value;
                Vector2 eyeOrigin = eyeTex.Size() / 2f;
                float eyePulse = 0.7f + MathF.Sin(time * 0.05f) * 0.3f;
                sb.Draw(eyeTex, drawPos, null, SilentUtils.AnswerWhite * 0.40f * eyePulse, 0f, eyeOrigin, 0.12f, SpriteEffects.None, 0f);
            }

            // === Stage 7: Spinning glyph ===
            {
                Texture2D glyph = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/MusicNote", AssetRequestMode.ImmediateLoad).Value;
                float glyphRot = time * 0.06f;
                float glyphAlpha = 0.18f + MathF.Sin(time * 0.08f) * 0.06f;
                sb.Draw(glyph, drawPos, null, SilentUtils.BrightQuestion * glyphAlpha, glyphRot,
                    glyph.Size() / 2f, 0.30f, SpriteEffects.None, 0f);
            }

            // === Pulsing light accent ===
            EnigmaVFXLibrary.AddPulsingLight(Projectile.Center, SilentUtils.BrightQuestion, 0.8f, 0.45f);
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
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Record trail positions for GPU primitive rendering
            _trailPositions.Insert(0, Projectile.Center);
            if (_trailPositions.Count > 25) _trailPositions.RemoveAt(_trailPositions.Count - 1);
            
            // Cycle eye texture
            if (Projectile.timeLeft % 20 == 0)
                eyeTextureIndex = (eyeTextureIndex + 1) % 3;
            
            Lighting.AddLight(Projectile.Center, EnigmaGreen.ToVector3() * 0.4f);
            
            // Dense trail particles
            SilentParticleHandler.Spawn(new ChainLightningParticle(
                Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 3f),
                SilentUtils.BrightQuestion,
                Main.rand.NextFloat(0.15f, 0.25f),
                15));
            
            if (Main.GameUpdateCount % 2 == 0)
            {
                SilentParticleHandler.Spawn(new SeekerTrailDot(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    SilentUtils.EnigmaEmerald,
                    0.15f,
                    18));
            }
            if (Main.GameUpdateCount % 3 == 0)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    ModContent.DustType<SilentMeasureDust>(),
                    Projectile.velocity.X * -0.15f, Projectile.velocity.Y * -0.15f);
            }
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 480);
            target.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(target, 2);
            
            // Chain lightning damage to nearby enemies
            if (!hitEnemies.Contains(target.whoAmI))
            {
                hitEnemies.Add(target.whoAmI);
                
                float chainRange = 300f;
                int chainsHit = 0;
                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (npc.friendly || npc.whoAmI == target.whoAmI) continue;
                    if (hitEnemies.Contains(npc.whoAmI)) continue;
                    if (Vector2.Distance(npc.Center, target.Center) > chainRange) continue;
                    
                    npc.SimpleStrikeNPC((int)(Projectile.damage * 0.4f), 0, false, 0f, null, false, 0f, true);
                    npc.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(npc, 1);
                    
                    // Chain lightning VFX between target and chained NPC
                    {
                        Vector2 cStart = target.Center;
                        Vector2 cEnd = npc.Center;
                        int segs = Main.rand.Next(6, 11);
                        for (int s = 0; s < segs; s++)
                        {
                            float t = s / (float)(segs - 1);
                            Vector2 segPos = Vector2.Lerp(cStart, cEnd, t);
                            Vector2 perp = (cEnd - cStart).SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2);
                            segPos += perp * Main.rand.NextFloat(-10f, 10f);
                            Color segCol = s % 2 == 0 ? SilentUtils.BrightQuestion : SilentUtils.AnswerWhite;
                            SilentParticleHandler.Spawn(new ChainLightningParticle(
                                segPos, (cEnd - cStart).SafeNormalize(Vector2.Zero) * 3f, segCol, 0.2f, 14));
                        }
                    }
                    
                    chainsHit++;
                    if (chainsHit >= 3) break;
                }
                
                if (chainsHit > 0)
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.5f, Volume = 0.3f }, target.Center);
            }
            
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.5f);
        }
        
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < Main.rand.Next(10, 16); i++)
            {
                SilentParticleHandler.Spawn(new ChainLightningParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(12f, 12f),
                    Main.rand.NextVector2CircularEdge(4f, 4f) * Main.rand.NextFloat(1f, 4f),
                    SilentUtils.BrightQuestion,
                    Main.rand.NextFloat(0.15f, 0.3f),
                    20));
            }
            SilentParticleHandler.Spawn(new MeasureImpactRing(Projectile.Center, SilentUtils.AnswerWhite, 0.5f, 25));
            SilentParticleHandler.Spawn(new MeasureImpactRing(Projectile.Center, SilentUtils.EnigmaEmerald * 0.7f, 0.7f, 30));
            for (int i = 0; i < Main.rand.Next(3, 5); i++)
            {
                SilentParticleHandler.Spawn(new QuestionMarkParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(15f, 15f),
                    Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(0.3f, 0.8f),
                    SilentUtils.BrightQuestion,
                    Main.rand.NextFloat(0.2f, 0.4f),
                    Main.rand.Next(20, 35)));
            }
            for (int i = 0; i < Main.rand.Next(6, 9); i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    ModContent.DustType<SilentMeasureDust>(),
                    Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f));
            }
        }
    }
}
