using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.Projectiles;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;
using MagnumOpus.Content.OdeToJoy.HarmonicCores;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.OdeToJoy.Weapons.Melee
{
    #region Rose Thorn Chainsaw
    
    /// <summary>
    /// Rose Thorn Chainsaw - Drill AI chainsaw with ripchain mechanics
    /// Held like a drill, vibrates and points at cursor, chainsaw sound
    /// Fires thorn segments that chain between enemies in a line
    /// Post-Dies Irae tier ultimate melee weapon
    /// </summary>
    public class RoseThornChainsaw : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
            // Mark as drill for proper behavior
            ItemID.Sets.IsDrill[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 4400; // POST-DIES IRAE - 57% above WrathsCleaver (2800)
            Item.DamageType = DamageClass.MeleeNoSpeed; // Drill-style, no attack speed bonuses
            Item.width = 54;
            Item.height = 24;
            
            // Fast drill timing
            Item.useTime = 1;
            Item.useAnimation = 1;
            
            // DRILL-SPECIFIC settings
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.channel = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
            Item.UseSound = SoundID.Item22; // Chainsaw sound
            Item.autoReuse = true;
            Item.crit = 22;

            // Shoot the chainsaw projectile
            Item.shoot = ModContent.ProjectileType<RoseThornChainsawProjectile>();
            Item.shootSpeed = 40f; // Holdout distance
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Rapidly shreds enemies with thorned blades"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Spawns thorn chains that ricochet to nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Contact applies blooming poison that spreads on death"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The garden's most beautiful danger'") 
            { 
                OverrideColor = OdeToJoyColors.RosePink 
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfOdeToJoy>(), 25)
                .AddIngredient(ModContent.ItemType<OdeToJoyResonantEnergy>(), 20)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfOdeToJoy>(), 3)
                .AddIngredient(ItemID.LunarBar, 20)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    /// <summary>
    /// Rose Thorn Chainsaw held projectile - drill AI with thorn chain spawning
    /// </summary>
    public class RoseThornChainsawProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/OdeToJoy/Weapons/Melee/RoseThornChainsaw";

        private int chainTimer = 0;
        private const int ChainInterval = 12; // Spawn thorn chain every 12 frames
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 54;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.ownerHitCheck = true;
            Projectile.aiStyle = 20; // Vanilla drill AI - jitter, cursor pointing, sound
            Projectile.hide = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            
            // Spawn thorn chain projectiles periodically
            chainTimer++;
            if (chainTimer >= ChainInterval)
            {
                chainTimer = 0;
                
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 chainDir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
                    Vector2 spawnPos = player.Center + chainDir * 60f;
                    
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        spawnPos,
                        chainDir * 18f,
                        ModContent.ProjectileType<ThornChainProjectile>(),
                        Projectile.damage / 3,
                        Projectile.knockBack / 2f,
                        Projectile.owner
                    );
                }
                
                // VFX burst when chain spawns - enhanced chromatic effect
                OdeToJoyVFX.ChromaticRosePetalBurst(player.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 40f, 5, 4f, 0.5f, false);
            }
            
            // === DENSE PARTICLE TRAIL ===
            // Sawdust / petal spray from chainsaw
            for (int i = 0; i < 2; i++)
            {
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(15f, 15f);
                Vector2 dustVel = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedByRandom(0.5f) * Main.rand.NextFloat(2f, 5f);
                
                Color trailColor = OdeToJoyColors.GetPetalGradient(Main.rand.NextFloat());
                var petal = new GenericGlowParticle(dustPos, dustVel, trailColor, 0.35f, 18, true);
                MagnumParticleHandler.SpawnParticle(petal);
            }
            
            // Green leaf particles
            if (Main.rand.NextBool(3))
            {
                Vector2 leafPos = Projectile.Center + Main.rand.NextVector2Circular(20f, 20f);
                var leaf = new GenericGlowParticle(
                    leafPos,
                    Main.rand.NextVector2Circular(3f, 3f) - Vector2.UnitY * 1.5f,
                    OdeToJoyColors.VerdantGreen * 0.7f,
                    0.3f,
                    25,
                    true
                );
                MagnumParticleHandler.SpawnParticle(leaf);
            }
            
            // Golden pollen sparkles
            if (Main.rand.NextBool(2))
            {
                Vector2 sparklePos = Projectile.Center + Main.rand.NextVector2Circular(25f, 25f);
                var sparkle = new SparkleParticle(
                    sparklePos,
                    Main.rand.NextVector2Circular(2f, 2f),
                    OdeToJoyColors.GoldenPollen,
                    0.35f,
                    20
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Music notes occasionally
            if (Main.rand.NextBool(8))
            {
                OdeToJoyVFX.SpawnMusicNote(
                    Projectile.Center + Main.rand.NextVector2Circular(30f, 30f),
                    Main.rand.NextVector2Circular(2f, 2f),
                    OdeToJoyColors.RosePink,
                    0.75f
                );
            }
            
            Lighting.AddLight(Projectile.Center, OdeToJoyColors.VerdantGreen.ToVector3() * 0.8f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply blooming poison debuff
            target.AddBuff(BuffID.Poisoned, 300);
            target.AddBuff(BuffID.Venom, 180);
            
            // Impact VFX - enhanced harmonic sparkle
            OdeToJoyVFX.HarmonicNoteSparkle(target.Center, 6, 4f, 0.55f, false);
            
            // Spawn extra thorn on crit
            if (hit.Crit && Main.myPlayer == Projectile.owner)
            {
                Vector2 thornDir = (target.Center - Main.player[Projectile.owner].Center).SafeNormalize(Vector2.UnitX);
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    thornDir.RotatedByRandom(0.3f) * 12f,
                    ModContent.ProjectileType<ThornChainProjectile>(),
                    Projectile.damage / 4,
                    2f,
                    Projectile.owner
                );
            }
        }
    }

    /// <summary>
    /// Thorn Chain Projectile - Ricochets between enemies like ripchain
    /// </summary>
    public class ThornChainProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/OdeToJoy/Projectiles/ThornProjectile";
        
        private int bounceCount = 0;
        private const int MaxBounces = 5;
        private NPC lastHitTarget = null;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = MaxBounces + 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // === THORN TRAIL ===
            if (Main.rand.NextBool(2))
            {
                Color trailColor = OdeToJoyColors.GetGradient(Main.rand.NextFloat());
                var trail = new GenericGlowParticle(
                    Projectile.Center,
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f),
                    trailColor * 0.7f,
                    0.3f,
                    15,
                    true
                );
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // Sparkles
            if (Main.rand.NextBool(3))
            {
                var sparkle = new SparkleParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
                    Main.rand.NextVector2Circular(1f, 1f),
                    OdeToJoyColors.GoldenPollen,
                    0.25f,
                    15
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            Lighting.AddLight(Projectile.Center, OdeToJoyColors.VerdantGreen.ToVector3() * 0.4f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            bounceCount++;
            lastHitTarget = target;
            
            // Impact VFX - Use new Chromatic Vine Growth effect!
            OdeToJoyVFX.ChromaticVineGrowthBurst(target.Center, 4, 4f, 0.6f, false);
            
            // Apply poison
            target.AddBuff(BuffID.Poisoned, 180);
            
            // Ricochet to next enemy
            if (bounceCount <= MaxBounces)
            {
                NPC nextTarget = FindNextTarget(target);
                if (nextTarget != null)
                {
                    Vector2 newDir = (nextTarget.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    Projectile.velocity = newDir * Projectile.velocity.Length();
                    
                    // Chain VFX line with vine tendrils
                    for (int i = 0; i < 8; i++)
                    {
                        float progress = i / 8f;
                        Vector2 linePos = Vector2.Lerp(target.Center, nextTarget.Center, progress);
                        var lineParticle = new GenericGlowParticle(
                            linePos + Main.rand.NextVector2Circular(5f, 5f),
                            Vector2.Zero,
                            OdeToJoyColors.VerdantGreen * 0.6f,
                            0.2f,
                            10,
                            true
                        );
                        MagnumParticleHandler.SpawnParticle(lineParticle);
                        
                        // Add sparkle accents along chain
                        if (i % 2 == 0)
                        {
                            var sparkle = new SparkleParticle(
                                linePos,
                                Vector2.Zero,
                                OdeToJoyColors.GoldenPollen,
                                0.25f,
                                12
                            );
                            MagnumParticleHandler.SpawnParticle(sparkle);
                        }
                    }
                }
            }
        }

        private NPC FindNextTarget(NPC excludeTarget)
        {
            float maxRange = 400f;
            NPC closest = null;
            float closestDist = maxRange;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc == excludeTarget || npc == lastHitTarget)
                    continue;
                
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            
            return closest;
        }

        public override void OnKill(int timeLeft)
        {
            OdeToJoyVFX.ChromaticRosePetalBurst(Projectile.Center, 8, 5f, 0.6f, true);
            SoundEngine.PlaySound(SoundID.Grass with { Volume = 0.6f, Pitch = 0.3f }, Projectile.Center);
        }
    }
    
    #endregion

    #region Thornbound Reckoning
    
    /// <summary>
    /// Thornbound Reckoning - Massive vine-wrapped greatsword
    /// On swing releases cascading vine waves
    /// Every 4th swing creates a blooming explosion that marks enemies
    /// Marked enemies take 30% more damage from all sources
    /// </summary>
    public class ThornboundReckoning : ModItem
    {
        private int swingCounter = 0;

        public override void SetDefaults()
        {
            Item.width = 80;
            Item.height = 80;
            Item.damage = 4200; // Post-Dies Irae tier
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
            Item.UseSound = SoundID.Item71;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.scale = 1.5f;
            Item.crit = 18;
            Item.shoot = ModContent.ProjectileType<VineWaveProjectile>();
            Item.shootSpeed = 14f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "On swing releases cascading vine waves"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Every 4th swing creates a blooming explosion"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Marked enemies take 30% increased damage"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Nature's wrath given steel form'") 
            { 
                OverrideColor = OdeToJoyColors.VerdantGreen 
            });
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            swingCounter++;
            
            // Always fire vine wave
            Vector2 mouseDir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            Projectile.NewProjectile(source, player.Center, mouseDir * Item.shootSpeed, 
                ModContent.ProjectileType<VineWaveProjectile>(), damage, knockback, player.whoAmI);
            
            // VFX for swing - enhanced chromatic vine effect
            OdeToJoyVFX.ChromaticVineGrowthBurst(player.Center + mouseDir * 50f, 3, 4f, 0.55f, false);
            SoundEngine.PlaySound(SoundID.Grass with { Volume = 0.8f }, player.Center);
            
            // Every 4th swing - blooming explosion with signature effect
            if (swingCounter >= 4)
            {
                swingCounter = 0;
                
                SoundEngine.PlaySound(SoundID.Item105 with { Pitch = 0.2f }, player.Center);
                
                // Spawn bloom explosion projectile
                Projectile.NewProjectile(source, player.Center + mouseDir * 60f, Vector2.Zero,
                    ModContent.ProjectileType<BloomExplosionProjectile>(), damage * 2, knockback * 2, player.whoAmI);
                
                // SIGNATURE EXPLOSION VFX on 4th swing!
                OdeToJoyVFX.OdeToJoySignatureExplosion(player.Center + mouseDir * 60f, 1.1f);
                
                for (int i = 0; i < 10; i++)
                {
                    OdeToJoyVFX.SpawnMusicNote(player.Center + mouseDir * 60f, 
                        Main.rand.NextVector2Circular(5f, 5f), OdeToJoyColors.RosePink, 0.9f);
                }
            }
            
            return false;
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 240);
            
            if (hit.Crit)
            {
                // Enhanced signature explosion on critical hits
                OdeToJoyVFX.ChromaticRosePetalBurst(target.Center, 12, 6f, 0.8f, true);
                OdeToJoyVFX.HarmonicNoteSparkle(target.Center, 8, 4f, 0.6f, false);
                target.AddBuff(BuffID.Venom, 180);
            }
            else
            {
                OdeToJoyVFX.ChromaticRosePetalBurst(target.Center, 6, 3f, 0.5f, false);
            }
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // Dense petal/vine trail
            if (Main.rand.NextBool(2))
            {
                Vector2 dustPos = new Vector2(
                    Main.rand.Next(hitbox.Left, hitbox.Right),
                    Main.rand.Next(hitbox.Top, hitbox.Bottom)
                );
                
                Color trailColor = OdeToJoyColors.GetGradient(Main.rand.NextFloat());
                var trail = new GenericGlowParticle(dustPos, player.velocity * 0.2f, trailColor, 0.4f, 15, true);
                MagnumParticleHandler.SpawnParticle(trail);
                
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.JungleGrass, player.velocity * 0.3f, 0, default, 1.5f);
                dust.noGravity = true;
            }
            
            // Music notes
            if (Main.rand.NextBool(5))
            {
                Vector2 notePos = new Vector2(
                    Main.rand.Next(hitbox.Left, hitbox.Right),
                    Main.rand.Next(hitbox.Top, hitbox.Bottom)
                );
                OdeToJoyVFX.SpawnMusicNote(notePos, Vector2.Zero, OdeToJoyColors.VerdantGreen, 0.8f);
            }
        }
        
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            float time = Main.GameUpdateCount * 0.06f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.1f;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Verdant green outer glow
            spriteBatch.Draw(texture, position, null, OdeToJoyColors.VerdantGreen * 0.35f, rotation, origin, scale * pulse * 1.3f, SpriteEffects.None, 0f);
            
            // Rose pink mid glow
            spriteBatch.Draw(texture, position, null, OdeToJoyColors.RosePink * 0.3f, rotation, origin, scale * pulse * 1.15f, SpriteEffects.None, 0f);
            
            // Golden shimmer
            float shimmer = (float)Math.Sin(time * 3f) * 0.5f + 0.5f;
            spriteBatch.Draw(texture, position, null, OdeToJoyColors.GoldenPollen * 0.2f * shimmer, rotation, origin, scale * pulse * 1.05f, SpriteEffects.None, 0f);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Lighting.AddLight(Item.Center, OdeToJoyColors.VerdantGreen.ToVector3() * 0.5f);
            
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfOdeToJoy>(), 20)
                .AddIngredient(ModContent.ItemType<OdeToJoyResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfOdeToJoy>(), 2)
                .AddIngredient(ItemID.LunarBar, 15)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    /// <summary>
    /// Vine Wave Projectile - Cascading nature wave
    /// </summary>
    public class VineWaveProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/OdeToJoy/Projectiles/BlossomWaveProjectile";

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 50;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.alpha += 3;
            
            if (Projectile.alpha >= 255)
                Projectile.Kill();
            
            // Dense vine trail
            for (int i = 0; i < 2; i++)
            {
                Vector2 trailPos = Projectile.Center + Main.rand.NextVector2Circular(20f, 20f);
                Color trailColor = OdeToJoyColors.GetGradient(Main.rand.NextFloat());
                var trail = new GenericGlowParticle(trailPos, -Projectile.velocity * 0.1f, trailColor * 0.7f, 0.35f, 18, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // Leaf particles
            if (Main.rand.NextBool(2))
            {
                Dust leaf = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(25f, 25f), 
                    DustID.JungleGrass, -Projectile.velocity * 0.15f, 0, default, 1.3f);
                leaf.noGravity = true;
            }
            
            Lighting.AddLight(Projectile.Center, OdeToJoyColors.VerdantGreen.ToVector3() * 0.6f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 180);
            OdeToJoyVFX.ChromaticRosePetalBurst(target.Center, 6, 3f, 0.45f, false);
        }

        public override void OnKill(int timeLeft)
        {
            OdeToJoyVFX.ChromaticVineGrowthBurst(Projectile.Center, 3, 4f, 0.45f, false);
        }
    }

    /// <summary>
    /// Bloom Explosion - Marks enemies for increased damage
    /// </summary>
    [AllowLargeHitbox("Bloom explosion requires large hitbox for AoE damage marking")]
    public class BloomExplosionProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare";

        public override void SetDefaults()
        {
            Projectile.width = 150;
            Projectile.height = 150;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 30;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override void AI()
        {
            // Expanding bloom effect
            Projectile.scale = 1f + (30 - Projectile.timeLeft) * 0.1f;
            Projectile.alpha = (int)(255 * (1f - Projectile.timeLeft / 30f));
            
            // Ring burst particles
            if (Projectile.timeLeft == 29)
            {
                for (int i = 0; i < 20; i++)
                {
                    float angle = MathHelper.TwoPi * i / 20f;
                    Vector2 vel = angle.ToRotationVector2() * 8f;
                    
                    var ring = new BloomRingParticle(
                        Projectile.Center,
                        vel,
                        OdeToJoyColors.GetPetalGradient(i / 20f),
                        0.5f,
                        25
                    );
                    MagnumParticleHandler.SpawnParticle(ring);
                }
            }
            
            Lighting.AddLight(Projectile.Center, OdeToJoyColors.RosePink.ToVector3() * (Projectile.timeLeft / 30f));
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply bloom mark debuff (30% damage increase)
            // Using Midas as a placeholder for damage boost
            target.AddBuff(BuffID.Midas, 300);
            target.AddBuff(BuffID.Poisoned, 300);
            target.AddBuff(BuffID.Venom, 180);
            
            // Full chromatic explosion on marked enemies!
            OdeToJoyVFX.ChromaticRosePetalBurst(target.Center, 14, 7f, 0.9f, true);
            OdeToJoyVFX.HarmonicNoteSparkle(target.Center, 10, 5f, 0.7f, true);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Don't draw the actual projectile, only particles
            return false;
        }
    }
    
    #endregion

    #region The Gardener's Fury
    
    /// <summary>
    /// The Gardener's Fury - Fast striking floral rapier
    /// Extremely fast attacks with petal projectiles
    /// Consecutive hits increase attack speed and spawn petal storms
    /// Max stacks: 10, each stack +5% attack speed
    /// </summary>
    public class TheGardenersFury : ModItem
    {
        private int comboCounter = 0;
        private int comboTimer = 0;

        public override void SetDefaults()
        {
            Item.width = 50;
            Item.height = 50;
            Item.damage = 3200; // Lower base damage, compensated by speed
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 8; // Very fast base
            Item.useAnimation = 8;
            Item.useStyle = ItemUseStyleID.Rapier; // Rapier thrust style
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.crit = 25;
            Item.shoot = ModContent.ProjectileType<GardenerFuryProjectile>();
            Item.shootSpeed = 5f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Lightning-fast floral strikes"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Consecutive hits build Gardener's Fervor (max 10 stacks)"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+5% attack speed per stack, spawns petal storms at high stacks"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Swift as spring winds, deadly as thorns'") 
            { 
                OverrideColor = OdeToJoyColors.RosePink 
            });
        }

        public override void HoldItem(Player player)
        {
            comboTimer++;
            if (comboTimer > 90) // 1.5 seconds without hitting resets combo
            {
                comboCounter = 0;
            }
            
            // Apply speed bonus based on combo
            if (comboCounter > 0)
            {
                float speedBonus = 1f + (comboCounter * 0.05f); // 5% per stack
                player.GetAttackSpeed(DamageClass.Melee) += comboCounter * 0.05f;
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // The rapier thrust projectile
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, comboCounter);
            
            // Spawn petal projectiles at high combo
            if (comboCounter >= 5)
            {
                int petalCount = Math.Min(comboCounter - 4, 5); // 1-5 extra petals
                for (int i = 0; i < petalCount; i++)
                {
                    Vector2 petalVel = velocity.RotatedByRandom(0.4f) * Main.rand.NextFloat(0.8f, 1.2f);
                    Projectile.NewProjectile(source, position, petalVel * 2f, 
                        ModContent.ProjectileType<SmallPetalProjectile>(), damage / 3, knockback / 2, player.whoAmI);
                }
            }
            
            return false;
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            comboTimer = 0;
            comboCounter = Math.Min(comboCounter + 1, 10);
            
            // VFX scales with combo - enhanced chromatic effect!
            float vfxScale = 0.5f + comboCounter * 0.1f;
            int petalCount = 4 + comboCounter / 2;
            OdeToJoyVFX.ChromaticRosePetalBurst(target.Center, petalCount, 3f * vfxScale, 0.4f + comboCounter * 0.05f, comboCounter >= 5);
            
            // At max combo, SIGNATURE EXPLOSION!
            if (comboCounter >= 10 && hit.Crit)
            {
                OdeToJoyVFX.OdeToJoySignatureExplosion(target.Center, 1.3f);
            }
            
            target.AddBuff(BuffID.Poisoned, 120 + comboCounter * 12);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfOdeToJoy>(), 20)
                .AddIngredient(ModContent.ItemType<OdeToJoyResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfOdeToJoy>(), 2)
                .AddIngredient(ItemID.LunarBar, 15)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    /// <summary>
    /// Gardener's Fury rapier thrust projectile
    /// </summary>
    public class GardenerFuryProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/OdeToJoy/Weapons/Melee/TheGardenersFury";

        public float ComboStacks => Projectile.ai[0];

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 12;
            Projectile.tileCollide = false;
            Projectile.ownerHitCheck = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            
            Projectile.direction = (Projectile.velocity.X > 0f) ? 1 : -1;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            if (Projectile.direction < 0)
                Projectile.rotation += MathHelper.PiOver2;
            
            // Position in front of player
            Projectile.Center = player.Center + Projectile.velocity * (12 - Projectile.timeLeft);
            
            // Trail particles - more intense with combo
            int particleCount = 1 + (int)(ComboStacks / 3);
            for (int i = 0; i < particleCount; i++)
            {
                if (Main.rand.NextBool(2))
                {
                    Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(10f, 10f);
                    Color trailColor = OdeToJoyColors.GetPetalGradient(Main.rand.NextFloat());
                    var trail = new GenericGlowParticle(dustPos, -Projectile.velocity * 0.2f, trailColor * 0.7f, 0.3f, 12, true);
                    MagnumParticleHandler.SpawnParticle(trail);
                }
            }
            
            // Sparkles at high combo
            if (ComboStacks >= 5 && Main.rand.NextBool(2))
            {
                var sparkle = new SparkleParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    Main.rand.NextVector2Circular(2f, 2f),
                    OdeToJoyColors.GoldenPollen,
                    0.35f,
                    15
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            Lighting.AddLight(Projectile.Center, OdeToJoyColors.RosePink.ToVector3() * (0.4f + ComboStacks * 0.06f));
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            OdeToJoyVFX.ChromaticRosePetalBurst(target.Center, 4, 2f, 0.35f, false);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            // Glow based on combo stacks
            float glowIntensity = 0.3f + ComboStacks * 0.07f;
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Main.spriteBatch.Draw(texture, drawPos, null, OdeToJoyColors.RosePink * glowIntensity, 
                Projectile.rotation, origin, Projectile.scale * 1.2f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texture, drawPos, null, OdeToJoyColors.GoldenPollen * (glowIntensity * 0.6f), 
                Projectile.rotation, origin, Projectile.scale * 1.1f, SpriteEffects.None, 0f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Main.spriteBatch.Draw(texture, drawPos, null, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    #endregion
}
