using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.DiesIrae.ResonanceEnergies;
using MagnumOpus.Content.DiesIrae.HarmonicCores;
using MagnumOpus.Content.DiesIrae.Projectiles;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.DiesIrae.ResonantWeapons
{
    /// <summary>
    /// Wrath's Cleaver - Massive hellfire cleaver. Post-Nachtmusik tier melee.
    /// On-swing: Sends blazing cascading wave of wrathful energy
    /// Every 3rd swing: Creates 5 crystallized flaming projectiles that home and explode
    /// </summary>
    public class WrathsCleaver : ModItem
    {
        private static readonly Color BloodRed = new Color(139, 0, 0);
        private static readonly Color EmberOrange = new Color(255, 69, 0);
        
        private int swingCounter = 0;

        public override void SetDefaults()
        {
            Item.width = 70;
            Item.height = 70;
            Item.damage = 2800; // POST-NACHTMUSIK ULTIMATE - 51%+ above Nachtmusik (1850)
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 9f;
            Item.value = Item.sellPrice(platinum: 2, gold: 50);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.UseSound = SoundID.Item71;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.scale = 1.6f;
            Item.crit = 20;
            Item.shoot = ModContent.ProjectileType<WrathWaveProjectile>();
            Item.shootSpeed = 12f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "On swing, releases a blazing wave of wrathful energy"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Every 3rd swing spawns 5 homing crystallized flame projectiles"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Strikes ignite enemies with hellfire"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Forged in the flames of final judgment'") 
            { 
                OverrideColor = BloodRed 
            });
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            swingCounter++;
            
            // Always fire the wrath wave
            Vector2 mouseDir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            Projectile.NewProjectile(source, player.Center, mouseDir * Item.shootSpeed, 
                ModContent.ProjectileType<WrathWaveProjectile>(), damage, knockback, player.whoAmI);
            
            // VFX for swing
            DiesIraeVFX.FireImpact(player.Center + mouseDir * 40f, 0.6f);
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot with { Volume = 0.7f }, player.Center);
            
            // Every 3rd swing - spawn 5 homing crystallized flames
            if (swingCounter >= 3)
            {
                swingCounter = 0;
                
                SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Pitch = 0.3f }, player.Center);
                
                for (int i = 0; i < 5; i++)
                {
                    float angle = mouseDir.ToRotation() + MathHelper.ToRadians(-30f + i * 15f);
                    Vector2 crystalVel = angle.ToRotationVector2() * 8f;
                    
                    Projectile.NewProjectile(source, player.Center + mouseDir * 30f, crystalVel,
                        ModContent.ProjectileType<CrystallizedFlameProjectile>(), damage * 3 / 4, knockback / 2, player.whoAmI);
                }
                
                // Extra VFX burst for the crystals
                for (int i = 0; i < 8; i++)
                {
                    DiesIraeVFX.SpawnMusicNote(player.Center + mouseDir * 40f, 
                        Main.rand.NextVector2Circular(4f, 4f), DiesIraeColors.HellfireGold, 0.9f);
                }
            }
            
            return false;
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 300);
            
            if (hit.Crit)
            {
                target.AddBuff(BuffID.Daybreak, 180);
                DiesIraeVFX.FireImpact(target.Center, 1.2f);
            }
            else
            {
                DiesIraeVFX.FireImpact(target.Center, 0.7f);
            }
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // Swing trail particles
            if (Main.rand.NextBool(2))
            {
                Vector2 dustPos = new Vector2(
                    Main.rand.Next(hitbox.Left, hitbox.Right),
                    Main.rand.Next(hitbox.Top, hitbox.Bottom)
                );
                
                Color trailColor = DiesIraeColors.GetGradient(Main.rand.NextFloat());
                var trail = new GenericGlowParticle(dustPos, player.velocity * 0.2f, trailColor, 0.4f, 15, true);
                MagnumParticleHandler.SpawnParticle(trail);
                
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.Torch, player.velocity * 0.3f, 0, default, 1.5f);
                dust.noGravity = true;
            }
            
            // Music notes in swing
            if (Main.rand.NextBool(6))
            {
                Vector2 notePos = new Vector2(
                    Main.rand.Next(hitbox.Left, hitbox.Right),
                    Main.rand.Next(hitbox.Top, hitbox.Bottom)
                );
                DiesIraeVFX.SpawnMusicNote(notePos, Vector2.Zero, DiesIraeColors.EmberOrange, 0.8f);
            }
        }
        
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            // === HELLFIRE GLOW EFFECT ===
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            float time = Main.GameUpdateCount * 0.07f;
            float pulse = 1f + (float)System.Math.Sin(time * 2f) * 0.12f;
            float flicker = Main.rand.NextFloat(0.9f, 1f);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Blood red outer glow
            spriteBatch.Draw(texture, position, null, BloodRed * 0.4f * flicker, rotation, origin, scale * pulse * 1.35f, SpriteEffects.None, 0f);
            
            // Ember orange mid glow
            spriteBatch.Draw(texture, position, null, EmberOrange * 0.35f * flicker, rotation, origin, scale * pulse * 1.2f, SpriteEffects.None, 0f);
            
            // Hot core shimmer
            float shimmer = (float)System.Math.Sin(time * 3f) * 0.5f + 0.5f;
            spriteBatch.Draw(texture, position, null, Color.Yellow * 0.25f * shimmer, rotation, origin, scale * pulse * 1.08f, SpriteEffects.None, 0f);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Lighting.AddLight(Item.Center, 0.8f, 0.4f, 0.1f);
            
            return true;
        }
        
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            // === HELLFIRE INVENTORY GLOW ===
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            
            float time = Main.GameUpdateCount * 0.06f;
            float pulse = 1f + (float)System.Math.Sin(time * 2.2f) * 0.1f;
            float flicker = Main.rand.NextFloat(0.85f, 1f);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
            
            // Hellfire gradient glow
            Color glowColor = Color.Lerp(BloodRed, EmberOrange, (float)System.Math.Sin(time * 0.8f) * 0.5f + 0.5f) * 0.35f * flicker;
            spriteBatch.Draw(texture, position, frame, glowColor, 0f, origin, scale * pulse * 1.15f, SpriteEffects.None, 0f);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
            
            spriteBatch.Draw(texture, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfDiesIrae>(), 20)
                .AddIngredient(ModContent.ItemType<DiesIraeResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfDiesIrae>(), 2)
                .AddIngredient(ItemID.LunarBar, 15)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    /// <summary>
    /// Chain of Judgment - Hellfire chain whip. Post-Nachtmusik tier melee.
    /// On-swing: Sends out a blazing spectral version that spins and ricochets between enemies
    /// Can bounce up to 4 times, explodes on each impact, then returns
    /// </summary>
    public class ChainOfJudgment : ModItem
    {
        private static readonly Color BloodRed = new Color(139, 0, 0);
        private static readonly Color EmberOrange = new Color(255, 69, 0);

        public override void SetDefaults()
        {
            Item.width = 50;
            Item.height = 50;
            Item.damage = 2400; // POST-NACHTMUSIK ULTIMATE - 33%+ above Nachtmusik
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(platinum: 2, gold: 50);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.UseSound = SoundID.Item153;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.scale = 1.3f;
            Item.crit = 15;
            Item.shoot = ModContent.ProjectileType<JudgmentChainProjectile>();
            Item.shootSpeed = 16f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Throws a blazing spectral chain that spins and ricochets"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Bounces up to 4 times between enemies, exploding on each hit"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Returns to you after bouncing"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The chains that bind the damned to their fate'") 
            { 
                OverrideColor = BloodRed 
            });
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 mouseDir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            
            Projectile.NewProjectile(source, player.Center, mouseDir * Item.shootSpeed,
                ModContent.ProjectileType<JudgmentChainProjectile>(), damage, knockback, player.whoAmI);
            
            // Launch VFX
            DiesIraeVFX.FireImpact(player.Center + mouseDir * 30f, 0.8f);
            
            for (int i = 0; i < 4; i++)
            {
                DiesIraeVFX.SpawnMusicNote(player.Center + mouseDir * 30f, 
                    mouseDir.RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * 3f, DiesIraeColors.HellfireGold, 0.85f);
            }
            
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfDiesIrae>(), 20)
                .AddIngredient(ModContent.ItemType<DiesIraeResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfDiesIrae>(), 2)
                .AddIngredient(ItemID.LunarBar, 15)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    /// <summary>
    /// Executioner's Verdict - Massive guillotine blade. Post-Nachtmusik tier melee.
    /// On-swing: Creates 3 ignited bolts that track enemies, explode, then spawn 3 spectral swords
    /// Execute mechanic: Instant kill below 15% HP
    /// </summary>
    public class ExecutionersVerdict : ModItem
    {
        private static readonly Color BloodRed = new Color(139, 0, 0);
        private static readonly Color EmberOrange = new Color(255, 69, 0);

        public override void SetDefaults()
        {
            Item.width = 80;
            Item.height = 80;
            Item.damage = 4200; // POST-NACHTMUSIK ULTIMATE - Massive execute damage
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 32;
            Item.useAnimation = 32;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 12f;
            Item.value = Item.sellPrice(platinum: 2, gold: 50);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.UseSound = SoundID.Item71 with { Pitch = -0.3f };
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.scale = 1.8f;
            Item.crit = 25;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "On swing, creates 3 ignited bolts that track enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Bolts explode and spawn 3 spectral sword strikes"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Executes non-boss enemies below 15% health instantly"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Deals 50% more damage to enemies below 30% health"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The final sentence is always death'") 
            { 
                OverrideColor = BloodRed 
            });
        }

        public override void UseItemFrame(Player player)
        {
            // Spawn ignited bolts during swing
            if (player.itemAnimation == player.itemAnimationMax - 5)
            {
                Vector2 mouseDir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
                
                SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot, player.Center);
                
                // Spawn 3 ignited bolts around the player
                for (int i = 0; i < 3; i++)
                {
                    float angle = mouseDir.ToRotation() + MathHelper.ToRadians(-30f + i * 30f);
                    Vector2 spawnPos = player.Center + angle.ToRotationVector2() * 50f;
                    Vector2 velocity = angle.ToRotationVector2() * 10f;
                    
                    Projectile.NewProjectile(
                        player.GetSource_ItemUse(Item), 
                        spawnPos, 
                        velocity,
                        ModContent.ProjectileType<IgnitedWrathBolt>(), 
                        Item.damage / 2, 
                        Item.knockBack / 2, 
                        player.whoAmI
                    );
                    
                    // Spawn VFX at each bolt location
                    DiesIraeVFX.FireImpact(spawnPos, 0.5f);
                }
                
                // Central VFX burst with music notes
                DiesIraeVFX.FireImpact(player.Center + mouseDir * 40f, 0.8f);
                for (int i = 0; i < 6; i++)
                {
                    DiesIraeVFX.SpawnMusicNote(player.Center + mouseDir * 40f, 
                        Main.rand.NextVector2Circular(5f, 5f), DiesIraeColors.HellfireGold, 0.9f);
                }
            }
        }

        public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers)
        {
            float healthPercent = (float)target.life / target.lifeMax;
            if (healthPercent < 0.30f)
            {
                modifiers.FinalDamage *= 1.5f;
            }
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Execution threshold
            float healthPercent = (float)target.life / target.lifeMax;
            if (healthPercent < 0.15f && !target.boss)
            {
                target.life = 0;
                target.HitEffect();
                target.checkDead();
                
                // Execute VFX
                DiesIraeVFX.FireImpact(target.Center, 2f);
                SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Pitch = -0.5f }, target.Center);
                
                for (int i = 0; i < 10; i++)
                {
                    DiesIraeVFX.SpawnMusicNote(target.Center, 
                        Main.rand.NextVector2Circular(8f, 8f), DiesIraeColors.BloodRed, 1f);
                }
            }
            else
            {
                DiesIraeVFX.FireImpact(target.Center, 1f);
            }
            
            target.AddBuff(BuffID.OnFire3, 240);
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // Heavy swing trail
            if (Main.rand.NextBool(2))
            {
                Vector2 dustPos = new Vector2(
                    Main.rand.Next(hitbox.Left, hitbox.Right),
                    Main.rand.Next(hitbox.Top, hitbox.Bottom)
                );
                
                Color trailColor = DiesIraeColors.GetGradient(Main.rand.NextFloat());
                var trail = new GenericGlowParticle(dustPos, player.velocity * 0.3f, trailColor, 0.5f, 18, true);
                MagnumParticleHandler.SpawnParticle(trail);
                
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.Torch, player.velocity * 0.4f, 0, default, 2f);
                dust.noGravity = true;
            }
            
            if (Main.rand.NextBool(5))
            {
                Vector2 notePos = new Vector2(
                    Main.rand.Next(hitbox.Left, hitbox.Right),
                    Main.rand.Next(hitbox.Top, hitbox.Bottom)
                );
                DiesIraeVFX.SpawnMusicNote(notePos, Vector2.Zero, DiesIraeColors.BloodRed, 0.85f);
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfDiesIrae>(), 25)
                .AddIngredient(ModContent.ItemType<DiesIraeResonantEnergy>(), 20)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfDiesIrae>(), 3)
                .AddIngredient(ItemID.LunarBar, 20)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }
}
