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
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.OdeToJoy.Weapons.Magic
{
    #region Hymn of the Victorious
    
    /// <summary>
    /// Hymn of the Victorious - Staff that summons a chorus of radiant notes
    /// Notes orbit player then launch outward in harmonious patterns
    /// Full combo creates a symphonic explosion that heals allies
    /// </summary>
    public class HymnOfTheVictorious : ModItem
    {
        private int noteCombo = 0;

        public override void SetDefaults()
        {
            Item.width = 58;
            Item.height = 58;
            Item.damage = 3600; // Post-Dies Irae magic
            Item.DamageType = DamageClass.Magic;
            Item.mana = 20;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
            Item.UseSound = SoundID.Item117 with { Pitch = 0.3f };
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<VictoriousNoteProjectile>();
            Item.shootSpeed = 0f;
            Item.crit = 18;
            Item.staff[Type] = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Summons 8 radiant music notes that orbit you"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Notes launch outward in harmonic patterns"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Every 5th cast creates a symphonic explosion that heals nearby players"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The hymn that heralds triumph'") 
            { 
                OverrideColor = OdeToJoyColors.GoldenPollen 
            });
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            noteCombo++;
            
            SoundEngine.PlaySound(SoundID.Item117 with { Pitch = 0.2f + noteCombo * 0.05f }, player.Center);
            
            // Spawn 8 orbiting notes
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 spawnOffset = angle.ToRotationVector2() * 70f;
                
                Projectile.NewProjectile(source, player.Center + spawnOffset, Vector2.Zero,
                    type, damage, knockback, player.whoAmI, ai0: angle);
                
                // Spawn VFX
                OdeToJoyVFX.SpawnMusicNote(player.Center + spawnOffset, 
                    Main.rand.NextVector2Circular(2f, 2f), OdeToJoyColors.GetGradient(i / 8f), 0.9f);
            }
            
            // Central burst VFX - enhanced chromatic effect
            OdeToJoyVFX.HarmonicNoteSparkle(player.Center, 6, 4f, 0.6f, false);
            
            // Every 5th cast - SIGNATURE EXPLOSION!
            if (noteCombo >= 5)
            {
                noteCombo = 0;
                
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.5f, Volume = 0.9f }, player.Center);
                
                // Symphonic explosion projectile
                Projectile.NewProjectile(source, player.Center, Vector2.Zero,
                    ModContent.ProjectileType<SymphonicExplosionProjectile>(), damage * 2, knockback * 2, player.whoAmI);
                
                OdeToJoyVFX.OdeToJoySignatureExplosion(player.Center, 1.4f);
                
                // Massive music note burst
                for (int i = 0; i < 20; i++)
                {
                    float angle = MathHelper.TwoPi * i / 20f;
                    OdeToJoyVFX.SpawnMusicNote(player.Center, 
                        angle.ToRotationVector2() * Main.rand.NextFloat(5f, 10f),
                        OdeToJoyColors.GetGradient(i / 20f), 1.0f);
                }
            }
            
            return false;
        }

        public override void HoldItem(Player player)
        {
            // Ambient glow
            if (Main.rand.NextBool(8))
            {
                Vector2 staffTip = player.Center + player.direction * new Vector2(40f, -25f);
                OdeToJoyVFX.SpawnMusicNote(staffTip, Main.rand.NextVector2Circular(1f, 1f),
                    OdeToJoyColors.GetGradient(Main.rand.NextFloat()), 0.6f);
            }
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
    /// Victorious Note Projectile - Orbits then launches
    /// </summary>
    public class VictoriousNoteProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/MusicNote";

        private float orbitAngle;
        private int phase = 0; // 0 = orbit, 1 = launch
        private const int OrbitTime = 40;

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            orbitAngle = Projectile.ai[0];
            
            if (phase == 0)
            {
                // Orbiting phase
                orbitAngle += 0.08f;
                Projectile.ai[0] = orbitAngle;
                
                float radius = 70f;
                Projectile.Center = owner.Center + orbitAngle.ToRotationVector2() * radius;
                Projectile.rotation = orbitAngle + MathHelper.PiOver2;
                
                if (Projectile.timeLeft <= 240 - OrbitTime)
                {
                    phase = 1;
                    
                    // Find target and launch
                    NPC target = FindClosestNPC(800f);
                    if (target != null)
                    {
                        Projectile.velocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 18f;
                    }
                    else
                    {
                        Projectile.velocity = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX) * 18f;
                    }
                    
                    Projectile.tileCollide = true;
                }
            }
            else
            {
                // Launching phase
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                
                // Gentle homing
                NPC target = FindClosestNPC(400f);
                if (target != null)
                {
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), 0.03f);
                }
            }
            
            // Trail particles
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
            
            Lighting.AddLight(Projectile.Center, OdeToJoyColors.GoldenPollen.ToVector3() * 0.5f);
        }

        private NPC FindClosestNPC(float maxRange)
        {
            NPC closest = null;
            float closestDist = maxRange;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage)
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

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Confused, 60);
            // Use new Harmonic Note Sparkle for magical impact!
            OdeToJoyVFX.HarmonicNoteSparkle(target.Center, 8, 4f, 0.7f, true);
        }

        public override void OnKill(int timeLeft)
        {
            // Use Chromatic Rose Petal Burst for beautiful death effect!
            OdeToJoyVFX.ChromaticRosePetalBurst(Projectile.Center, 10, 5f, 0.7f, true);
            
            for (int i = 0; i < 5; i++)
            {
                OdeToJoyVFX.SpawnMusicNote(Projectile.Center,
                    Main.rand.NextVector2Circular(4f, 4f),
                    OdeToJoyColors.GetGradient(Main.rand.NextFloat()), 0.8f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.15f;
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Bloom layers
            Main.spriteBatch.Draw(texture, drawPos, null, OdeToJoyColors.GoldenPollen * 0.5f, Projectile.rotation, origin, 0.9f * pulse, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texture, drawPos, null, OdeToJoyColors.RosePink * 0.4f, Projectile.rotation, origin, 0.7f * pulse, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texture, drawPos, null, Color.White * 0.7f, Projectile.rotation, origin, 0.5f * pulse, SpriteEffects.None, 0f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
    }

    /// <summary>
    /// Symphonic Explosion - Heals nearby players, damages enemies
    /// </summary>
    [AllowLargeHitbox("Healing/damage explosion requires large hitbox for AoE effect")]
    public class SymphonicExplosionProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow2";

        public override void SetDefaults()
        {
            Projectile.width = 300;
            Projectile.height = 300;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 20;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override void AI()
        {
            // Heal nearby players
            if (Projectile.timeLeft == 19)
            {
                Player owner = Main.player[Projectile.owner];
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player player = Main.player[i];
                    if (!player.active || player.dead)
                        continue;
                    
                    float dist = Vector2.Distance(Projectile.Center, player.Center);
                    if (dist < 200f)
                    {
                        int healAmount = 50;
                        player.Heal(healAmount);
                        
                        // Healing VFX
                        for (int j = 0; j < 5; j++)
                        {
                            OdeToJoyVFX.SpawnMusicNote(player.Center,
                                new Vector2(0, -3f).RotatedByRandom(0.5f),
                                OdeToJoyColors.VerdantGreen, 0.8f);
                        }
                    }
                }
            }
            
            // Expanding visual
            float progress = 1f - (Projectile.timeLeft / 20f);
            
            for (int i = 0; i < 3; i++)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float radius = progress * 150f;
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * radius;
                
                var particle = new GenericGlowParticle(
                    pos,
                    Main.rand.NextVector2Circular(2f, 2f),
                    OdeToJoyColors.GetGradient(progress) * (1f - progress),
                    0.4f,
                    15,
                    true
                );
                MagnumParticleHandler.SpawnParticle(particle);
            }
            
            Lighting.AddLight(Projectile.Center, OdeToJoyColors.GoldenPollen.ToVector3() * (1f - progress));
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Confused, 180);
            OdeToJoyVFX.HarmonicNoteSparkle(target.Center, 8, 5f, 0.7f, true);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }
    
    #endregion

    #region Elysian Verdict
    
    /// <summary>
    /// Elysian Verdict - Cursor-tracking orb that spawns vine missiles
    /// Left click to launch, right click to detonate with massive bloom explosion
    /// </summary>
    public class ElysianVerdict : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 50;
            Item.height = 50;
            Item.damage = 3200; // Post-Dies Irae magic
            Item.DamageType = DamageClass.Magic;
            Item.mana = 28;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
            Item.UseSound = SoundID.Item73 with { Pitch = 0.2f };
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<ElysianOrbProjectile>();
            Item.shootSpeed = 14f;
            Item.crit = 22;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Launches an Elysian orb that tracks your cursor"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "While airborne, spawns homing vine missiles"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Explodes on impact or when clicking again"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The garden's final judgement upon the wicked'") 
            { 
                OverrideColor = OdeToJoyColors.VerdantGreen 
            });
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 launchPos = player.Center;
            Vector2 launchVel = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX) * Item.shootSpeed;
            
            Projectile.NewProjectile(source, launchPos, launchVel, type, damage, knockback, player.whoAmI);
            
            // Launch VFX - enhanced chromatic effects
            OdeToJoyVFX.ChromaticVineGrowthBurst(launchPos, 4, 5f, 0.6f, true);
            OdeToJoyVFX.HarmonicNoteSparkle(launchPos, 6, 4f, 0.5f, false);
            
            for (int i = 0; i < 6; i++)
            {
                OdeToJoyVFX.SpawnMusicNote(launchPos,
                    launchVel.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.7f, 0.7f)) * 4f,
                    OdeToJoyColors.VerdantGreen, 0.8f);
            }
            
            return false;
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
    /// Elysian Orb Projectile - Cursor tracking, spawns vine missiles
    /// </summary>
    public class ElysianOrbProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow3";

        private int missileTimer = 0;

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            // Track cursor
            Vector2 toCursor = (Main.MouseWorld - Projectile.Center);
            if (toCursor.Length() > 20f)
            {
                toCursor = toCursor.SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toCursor * 16f, 0.08f);
            }
            
            Projectile.rotation += 0.1f;
            
            // Spawn vine missiles periodically
            missileTimer++;
            if (missileTimer >= 20)
            {
                missileTimer = 0;
                
                NPC target = FindClosestNPC(600f);
                if (target != null && Main.myPlayer == Projectile.owner)
                {
                    Vector2 missileVel = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 10f;
                    missileVel = missileVel.RotatedByRandom(0.3f);
                    
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        missileVel,
                        ModContent.ProjectileType<VineMissileProjectile>(),
                        Projectile.damage / 3,
                        Projectile.knockBack / 2,
                        Projectile.owner
                    );
                    
                    OdeToJoyVFX.ChromaticVineGrowthBurst(Projectile.Center, 2, 3f, 0.35f, false);
                }
            }
            
            // Trail
            for (int i = 0; i < 2; i++)
            {
                Color trailColor = Color.Lerp(OdeToJoyColors.VerdantGreen, OdeToJoyColors.GoldenPollen, Main.rand.NextFloat());
                var trail = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    -Projectile.velocity * 0.1f,
                    trailColor * 0.6f,
                    0.35f,
                    18,
                    true
                );
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // Click to explode
            if (owner.channel && Projectile.timeLeft < 280)
            {
                Explode();
            }
            
            Lighting.AddLight(Projectile.Center, OdeToJoyColors.VerdantGreen.ToVector3() * 0.6f);
        }

        private NPC FindClosestNPC(float maxRange)
        {
            NPC closest = null;
            float closestDist = maxRange;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage)
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

        private void Explode()
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.3f }, Projectile.Center);
            
            // Spawn explosion
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                Vector2.Zero,
                ModContent.ProjectileType<ElysianExplosionProjectile>(),
                Projectile.damage,
                Projectile.knockBack * 2,
                Projectile.owner
            );
            
            OdeToJoyVFX.OdeToJoySignatureExplosion(Projectile.Center, 1.2f);
            
            Projectile.Kill();
        }

        public override void OnKill(int timeLeft)
        {
            if (timeLeft > 0)
            {
                Explode();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.2f;
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Main.spriteBatch.Draw(texture, drawPos, null, OdeToJoyColors.VerdantGreen * 0.6f, Projectile.rotation, origin, 0.7f * pulse, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texture, drawPos, null, OdeToJoyColors.GoldenPollen * 0.5f, -Projectile.rotation * 0.8f, origin, 0.5f * pulse, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texture, drawPos, null, Color.White * 0.7f, Projectile.rotation * 1.2f, origin, 0.35f * pulse, SpriteEffects.None, 0f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
    }

    /// <summary>
    /// Vine Missile Projectile - Homing vine shot
    /// </summary>
    public class VineMissileProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/OdeToJoy/Projectiles/ThornProjectile";

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            
            // Homing
            NPC target = FindClosestNPC(400f);
            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 14f, 0.06f);
            }
            
            // Trail
            if (Main.rand.NextBool(2))
            {
                var trail = new GenericGlowParticle(
                    Projectile.Center,
                    -Projectile.velocity * 0.1f,
                    OdeToJoyColors.VerdantGreen * 0.6f,
                    0.2f,
                    12,
                    true
                );
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            Lighting.AddLight(Projectile.Center, OdeToJoyColors.VerdantGreen.ToVector3() * 0.3f);
        }

        private NPC FindClosestNPC(float maxRange)
        {
            NPC closest = null;
            float closestDist = maxRange;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage)
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

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 180);
            OdeToJoyVFX.ChromaticVineGrowthBurst(target.Center, 2, 3f, 0.35f, false);
        }

        public override void OnKill(int timeLeft)
        {
            OdeToJoyVFX.ChromaticRosePetalBurst(Projectile.Center, 5, 2f, 0.35f, false);
        }
    }

    /// <summary>
    /// Elysian Explosion Projectile - AOE damage
    /// </summary>
    [AllowLargeHitbox("AoE explosion requires large hitbox for area damage")]
    public class ElysianExplosionProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow2";

        public override void SetDefaults()
        {
            Projectile.width = 200;
            Projectile.height = 200;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 15;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            float progress = 1f - (Projectile.timeLeft / 15f);
            
            for (int i = 0; i < 5; i++)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float radius = progress * 100f;
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * radius;
                
                Color particleColor = OdeToJoyColors.GetGradient(Main.rand.NextFloat());
                var particle = new GenericGlowParticle(
                    pos,
                    Main.rand.NextVector2Circular(3f, 3f),
                    particleColor * (1f - progress),
                    0.4f,
                    12,
                    true
                );
                MagnumParticleHandler.SpawnParticle(particle);
            }
            
            Lighting.AddLight(Projectile.Center, OdeToJoyColors.VerdantGreen.ToVector3() * (1f - progress));
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 300);
            OdeToJoyVFX.HarmonicNoteSparkle(target.Center, 6, 4f, 0.55f, false);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }
    
    #endregion

    #region Anthem of Glory
    
    /// <summary>
    /// Anthem of Glory - Tome that fires 3 blazing petal shards in a spread
    /// On impact, shards chain to nearby enemies with golden vines
    /// Every 3rd cast fires a massive glory beam
    /// </summary>
    public class AnthemOfGlory : ModItem
    {
        private int castCounter = 0;

        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 42;
            Item.damage = 2800; // Post-Dies Irae magic
            Item.DamageType = DamageClass.Magic;
            Item.mana = 14;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
            Item.UseSound = SoundID.Item71;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<GloryShardProjectile>();
            Item.shootSpeed = 16f;
            Item.crit = 16;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires 3 blazing petal shards"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Shards chain golden vines to nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Every 3rd cast fires a massive glory beam"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The anthem that crowns all victories'") 
            { 
                OverrideColor = OdeToJoyColors.RosePink 
            });
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            castCounter++;
            
            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.UnitX) * 30f;
            
            // Fire 3 shards in spread
            for (int i = -1; i <= 1; i++)
            {
                Vector2 spreadVel = velocity.RotatedBy(MathHelper.ToRadians(i * 12f));
                Projectile.NewProjectile(source, position, spreadVel, type, damage, knockback, player.whoAmI);
            }
            
            // Enhanced muzzle VFX with chromatic effects
            OdeToJoyVFX.ChromaticRosePetalBurst(muzzlePos, 6, 3f, 0.45f, false);
            
            for (int i = 0; i < 3; i++)
            {
                OdeToJoyVFX.SpawnMusicNote(muzzlePos,
                    velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * 3f,
                    OdeToJoyColors.GetPetalGradient(Main.rand.NextFloat()), 0.75f);
            }
            
            // Every 3rd cast - glory beam with SIGNATURE effect!
            if (castCounter >= 3)
            {
                castCounter = 0;
                
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.4f }, player.Center);
                
                Projectile.NewProjectile(source, position, velocity * 1.5f,
                    ModContent.ProjectileType<OdeToJoyGloryBeamProjectile>(), damage * 3, knockback * 2, player.whoAmI);
                
                OdeToJoyVFX.OdeToJoySignatureExplosion(muzzlePos, 0.9f);
            }
            
            return false;
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
    /// Glory Shard Projectile - Chains to nearby enemies
    /// </summary>
    public class GloryShardProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare4";

        private bool hasChained = false;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Trail
            if (Main.rand.NextBool(2))
            {
                Color trailColor = OdeToJoyColors.GetPetalGradient(Main.rand.NextFloat());
                var trail = new GenericGlowParticle(
                    Projectile.Center,
                    -Projectile.velocity * 0.1f,
                    trailColor * 0.6f,
                    0.25f,
                    12,
                    true
                );
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            Lighting.AddLight(Projectile.Center, OdeToJoyColors.RosePink.ToVector3() * 0.4f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!hasChained)
            {
                hasChained = true;
                ChainToNearbyEnemies(target);
            }
            
            target.AddBuff(BuffID.Poisoned, 120);
            OdeToJoyVFX.ChromaticRosePetalBurst(target.Center, 7, 3f, 0.5f, false);
        }

        private void ChainToNearbyEnemies(NPC origin)
        {
            int chainCount = 0;
            int maxChains = 3;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (chainCount >= maxChains) break;
                
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.whoAmI == origin.whoAmI)
                    continue;
                
                float dist = Vector2.Distance(origin.Center, npc.Center);
                if (dist < 250f)
                {
                    // Deal chain damage
                    npc.SimpleStrikeNPC(Projectile.damage / 2, 0, false, 0f, DamageClass.Magic);
                    
                    // VFX: enhanced chromatic vine chain
                    DrawVineChain(origin.Center, npc.Center);
                    OdeToJoyVFX.ChromaticVineGrowthBurst(npc.Center, 2, 3f, 0.4f, false);
                    
                    chainCount++;
                }
            }
        }

        private void DrawVineChain(Vector2 start, Vector2 end)
        {
            int segments = 8;
            for (int i = 0; i < segments; i++)
            {
                float progress = i / (float)segments;
                Vector2 pos = Vector2.Lerp(start, end, progress);
                
                Color vineColor = Color.Lerp(OdeToJoyColors.GoldenPollen, OdeToJoyColors.VerdantGreen, progress);
                var vine = new GenericGlowParticle(
                    pos + Main.rand.NextVector2Circular(3f, 3f),
                    Main.rand.NextVector2Circular(1f, 1f),
                    vineColor * 0.7f,
                    0.25f,
                    15,
                    true
                );
                MagnumParticleHandler.SpawnParticle(vine);
            }
        }

        public override void OnKill(int timeLeft)
        {
            OdeToJoyVFX.ChromaticRosePetalBurst(Projectile.Center, 6, 2f, 0.4f, true);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.15f;
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Main.spriteBatch.Draw(texture, drawPos, null, OdeToJoyColors.RosePink * 0.6f, Projectile.rotation, origin, 0.5f * pulse, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texture, drawPos, null, OdeToJoyColors.GoldenPollen * 0.5f, Projectile.rotation * 0.8f, origin, 0.4f * pulse, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texture, drawPos, null, Color.White * 0.7f, Projectile.rotation, origin, 0.25f * pulse, SpriteEffects.None, 0f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
    }

    /// <summary>
    /// Glory Beam Projectile - Massive piercing beam (Magic Weapon variant)
    /// </summary>
    public class OdeToJoyGloryBeamProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/ParticleTrail1";

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.extraUpdates = 3;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Heavy trail
            for (int i = 0; i < 3; i++)
            {
                Color trailColor = OdeToJoyColors.GetGradient(Main.rand.NextFloat());
                var trail = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    -Projectile.velocity * 0.1f,
                    trailColor * 0.8f,
                    0.4f,
                    20,
                    true
                );
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // Sparkles
            if (Main.rand.NextBool(2))
            {
                var sparkle = new SparkleParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    Main.rand.NextVector2Circular(2f, 2f),
                    OdeToJoyColors.GoldenPollen,
                    0.35f,
                    15
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Music notes
            if (Main.rand.NextBool(4))
            {
                OdeToJoyVFX.SpawnMusicNote(Projectile.Center,
                    Main.rand.NextVector2Circular(2f, 2f),
                    OdeToJoyColors.GetGradient(Main.rand.NextFloat()), 0.8f);
            }
            
            Lighting.AddLight(Projectile.Center, OdeToJoyColors.GoldenPollen.ToVector3() * 0.8f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire, 300);
            target.AddBuff(BuffID.Confused, 120);
            OdeToJoyVFX.HarmonicNoteSparkle(target.Center, 8, 5f, 0.65f, true);
        }

        public override void OnKill(int timeLeft)
        {
            // Signature explosion for projectile death
            OdeToJoyVFX.OdeToJoySignatureExplosion(Projectile.Center, 0.85f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.2f;
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Main.spriteBatch.Draw(texture, drawPos, null, OdeToJoyColors.GoldenPollen * 0.7f, Projectile.rotation, origin, 1.0f * pulse, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texture, drawPos, null, OdeToJoyColors.RosePink * 0.5f, Projectile.rotation, origin, 0.8f * pulse, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texture, drawPos, null, Color.White * 0.8f, Projectile.rotation, origin, 0.5f * pulse, SpriteEffects.None, 0f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
    }
    
    #endregion
}
