using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
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
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheSilentMeasure.Particles;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheSilentMeasure.Dusts;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheSilentMeasure.Utilities;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheSilentMeasure
{
    /// <summary>
    /// THE SILENT MEASURE - Ranged bow weapon that fires enigmatic arrows
    /// Every 5th shot is a Paradox Piercing Bolt with enhanced effects
    /// Regular arrows are Question Seeker Bolts that split on hit
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
            Item.rare = ModContent.RarityType<EnigmaRarity>();
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
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect", "Fires reality-warping arrows that split into homing seekers on hit"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect2", "Every 5th shot is a Paradox Piercing Bolt with chain lightning"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect3", "Seekers hunt down nearby enemies with relentless precision"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaLore", "'Silence measures what sound cannot — the space between notes where truth resides.'")
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
        
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/MusicNote";
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            // === Shader overlay: Phantom echo multi-ghost seeker trail ===
            {
                Texture2D shBloom = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
                EnigmaShaderHelper.DrawShaderOverlay(sb, ShaderLoader.SilentSeekerTrail,
                    shBloom, drawPos, shBloom.Size() / 2f, 0.8f,
                    SilentUtils.QuestionViolet.ToVector3(), SilentUtils.EnigmaEmerald.ToVector3(),
                    opacity: 0.5f, intensity: 1.0f, rotation: Projectile.velocity.ToRotation(),
                    noiseTexture: ShaderLoader.GetNoiseTexture("PerlinNoise"),
                    techniqueName: "SilentSeekerFlow");
            }
            
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Velocity-stretched glow trail
            Texture2D pixel = MagnumTextureRegistry.GetSoftGlow();
            if (pixel == null) return false;
            float rot = Projectile.velocity.ToRotation();
            Vector2 trailScale = new Vector2(16f / pixel.Width, 6f / pixel.Height);
            sb.Draw(pixel, drawPos, null, SilentUtils.QuestionViolet * 0.7f, rot, pixel.Size() / 2f, trailScale, SpriteEffects.None, 0f);
            
            // Bloom core
            Texture2D bloom = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            sb.Draw(bloom, drawPos, null, SilentUtils.QuestionViolet * 0.6f, 0f, bloom.Size() / 2f, 0.2f, SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null, SilentUtils.AnswerWhite * 0.3f, 0f, bloom.Size() / 2f, 0.08f, SpriteEffects.None, 0f);
            
            SilentUtils.ExitShaderRegion(sb);
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
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 300);
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
                npc.AddBuff(ModContent.BuffType<ParadoxBrand>(), 180);
                
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
        
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/CursiveMusicNote";
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            // === Shader overlay: Question-mark silhouette echo ===
            {
                Texture2D shBloom = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
                EnigmaShaderHelper.DrawShaderOverlay(sb, ShaderLoader.SilentQuestionBurst,
                    shBloom, drawPos, shBloom.Size() / 2f, 0.6f,
                    SilentUtils.QuestionViolet.ToVector3(), SilentUtils.BrightQuestion.ToVector3(),
                    opacity: 0.4f, intensity: 1.0f, rotation: Projectile.velocity.ToRotation(),
                    noiseTexture: ShaderLoader.GetNoiseTexture("SimplexNoise"),
                    techniqueName: "SilentQuestionBlast");
            }
            
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Small velocity-stretched glow trail
            Texture2D pixel = MagnumTextureRegistry.GetPointBloom();
            if (pixel == null) return false;
            float rot = Projectile.velocity.ToRotation();
            Vector2 trailScale = new Vector2(12f / pixel.Width, 4f / pixel.Height);
            sb.Draw(pixel, drawPos, null, SilentUtils.EnigmaEmerald * 0.65f, rot, pixel.Size() / 2f, trailScale, SpriteEffects.None, 0f);
            
            // Bloom core
            Texture2D bloom = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            sb.Draw(bloom, drawPos, null, SilentUtils.EnigmaEmerald * 0.5f, 0f, bloom.Size() / 2f, 0.15f, SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null, SilentUtils.AnswerWhite * 0.25f, 0f, bloom.Size() / 2f, 0.06f, SpriteEffects.None, 0f);
            
            // Faint glyph overlay
            Texture2D glyph = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/CursiveMusicNote", AssetRequestMode.ImmediateLoad).Value;
            float glyphAlpha = 0.15f + MathF.Sin(Projectile.timeLeft * 0.1f) * 0.05f;
            sb.Draw(glyph, drawPos, null, SilentUtils.QuestionViolet * glyphAlpha, Projectile.rotation + MathHelper.PiOver4,
                glyph.Size() / 2f, 0.3f, SpriteEffects.None, 0f);
            
            SilentUtils.ExitShaderRegion(sb);
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
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 300);
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
        
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/MusicNote";
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            
            Texture2D bloom = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            
            // Wide bright glow trail
            Texture2D pixel = MagnumTextureRegistry.GetSoftGlow();
            if (pixel == null) return false;
            float rot = Projectile.velocity.ToRotation();
            Vector2 trailScale = new Vector2(30f / pixel.Width, 14f / pixel.Height);
            Color trailColor = Color.Lerp(SilentUtils.BrightQuestion, SilentUtils.AnswerWhite, 0.4f);
            sb.Draw(pixel, drawPos, null, trailColor * 0.75f, rot, pixel.Size() / 2f, trailScale, SpriteEffects.None, 0f);
            
            // Outer bloom — QuestionViolet
            sb.Draw(bloom, drawPos, null, SilentUtils.QuestionViolet * 0.35f, 0f, bloom.Size() / 2f, 0.5f, SpriteEffects.None, 0f);
            
            // Inner bloom — EnigmaEmerald
            sb.Draw(bloom, drawPos, null, SilentUtils.EnigmaEmerald * 0.6f, 0f, bloom.Size() / 2f, 0.3f, SpriteEffects.None, 0f);
            
            // White hot core
            sb.Draw(bloom, drawPos, null, SilentUtils.AnswerWhite * 0.7f, 0f, bloom.Size() / 2f, 0.15f, SpriteEffects.None, 0f);
            
            SilentUtils.ExitShaderRegion(sb);
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
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 360);
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
            for (int i = 0; i < Main.rand.Next(6, 11); i++)
            {
                SilentParticleHandler.Spawn(new ChainLightningParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    Main.rand.NextVector2CircularEdge(3f, 3f) * Main.rand.NextFloat(1f, 3f),
                    SilentUtils.BrightQuestion,
                    Main.rand.NextFloat(0.15f, 0.25f),
                    18));
            }
            SilentParticleHandler.Spawn(new MeasureImpactRing(Projectile.Center, SilentUtils.AnswerWhite, 0.4f, 22));
            for (int i = 0; i < Main.rand.Next(4, 6); i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    ModContent.DustType<SilentMeasureDust>(),
                    Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
            }
        }
    }
}
