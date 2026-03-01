using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Nachtmusik.Debuffs;
using MagnumOpus.Content.Nachtmusik.Projectiles;

namespace MagnumOpus.Content.Nachtmusik.ResonantWeapons
{
    /// <summary>
    /// Constellation Piercer - A celestial rifle that fires star-chaining energy bolts.
    /// Bolts chain between enemies, marking them with constellations.
    /// DAMAGE: 750 (chains make it much higher effective DPS)
    /// </summary>
    public class ConstellationPiercer : ModItem
    {
        private int crystalCounter = 0;
        
        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 66;
            Item.damage = 1150; // POST-FATE ULTIMATE RANGED - 35%+ above Fate tier
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 12;
            Item.useAnimation = 12;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4.5f;
            Item.value = Item.sellPrice(gold: 45);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.UseSound = SoundID.Item41 with { Pitch = -0.2f, Volume = 0.9f }; // Gun sound
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<ConstellationBoltProjectile>();
            Item.shootSpeed = 22f;
            Item.useAmmo = AmmoID.Bullet;
            Item.crit = 22;
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Always fire constellation bolts, ignore arrow type
            Vector2 direction = velocity.SafeNormalize(Vector2.UnitX);
            
            // Track shots for crystal burst
            crystalCounter++;
            
            // Fire main bolt
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<ConstellationBoltProjectile>(), damage, knockback, player.whoAmI);
            
            // Fire two side bolts at slight angles
            for (int i = -1; i <= 1; i += 2)
            {
                float angleOffset = MathHelper.ToRadians(8f * i);
                Vector2 sideVel = velocity.RotatedBy(angleOffset);
                Projectile.NewProjectile(source, position, sideVel, ModContent.ProjectileType<ConstellationBoltProjectile>(), (int)(damage * 0.7f), knockback * 0.5f, player.whoAmI);
            }
            
            // === SPAWN SEEKING CRYSTALS EVERY 5 SHOTS ===
            if (crystalCounter >= 5)
            {
                crystalCounter = 0;
                SeekingCrystalHelper.SpawnNachtmusikCrystals(
                    source,
                    position + direction * 30f,
                    velocity * 0.8f,
                    (int)(damage * 0.5f),
                    knockback,
                    player.whoAmI,
                    4
                );
                SoundEngine.PlaySound(SoundID.Item25 with { Pitch = 0.4f, Volume = 0.7f }, position);
            }
            
            // Muzzle flash VFX
            ConstellationPiercerVFX.MuzzleFlashVFX(position + direction * 25f, direction);
            
            return false;
        }
        
        public override void HoldItem(Player player)
        {
            ConstellationPiercerVFX.HoldItemVFX(player);
        }
        
        public override Vector2? HoldoutOffset() => new Vector2(-5f, 0f);

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D tex = TextureAssets.Item[Item.type].Value;
            Vector2 pos = Item.Center - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;

            float time = Main.GameUpdateCount * 0.06f;
            float pulse = 1f + (float)Math.Sin(time * 2.2f) * 0.08f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Constellation blue precision outer ring
            spriteBatch.Draw(tex, pos, null, NachtmusikPalette.ConstellationBlue with { A = 0 } * 0.35f,
                rotation, origin, scale * pulse * 1.3f, SpriteEffects.None, 0f);

            // Star gold crosshair shimmer
            float crosshairPulse = (float)Math.Sin(time * 3.5f) * 0.5f + 0.5f;
            spriteBatch.Draw(tex, pos, null, NachtmusikPalette.StarGold with { A = 0 } * 0.25f * crosshairPulse,
                rotation, origin, scale * pulse * 1.15f, SpriteEffects.None, 0f);

            // Star white precision core
            spriteBatch.Draw(tex, pos, null, NachtmusikPalette.StarWhite with { A = 0 } * 0.2f,
                rotation, origin, scale * pulse * 1.05f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, NachtmusikPalette.ConstellationBlue.ToVector3() * 0.4f);
            return true;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D tex = TextureAssets.Item[Item.type].Value;
            float time = Main.GameUpdateCount * 0.05f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.06f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            Color glowColor = NachtmusikPalette.GetStarfieldGradient((float)Math.Sin(time * 0.8f) * 0.5f + 0.5f) * 0.25f;
            spriteBatch.Draw(tex, position, frame, glowColor, 0f, origin, scale * pulse * 1.1f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            spriteBatch.Draw(tex, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            return false;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Triple", "Fires three constellation bolts per shot"));
            tooltips.Add(new TooltipLine(Mod, "Chain", "Bolts chain to up to 4 nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "Debuff", "Inflicts Celestial Harmony on all chained targets"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Each shot etches another star into the cosmos'")
            {
                OverrideColor = NachtmusikPalette.RadianceGold
            });
        }
    }

    /// <summary>
    /// Nebula's Whisper - A cosmic cannon that fires splitting nebula blasts.
    /// Shots split on hit, creating a cloud of cosmic damage.
    /// DAMAGE: 680 (splits make effective damage much higher)
    /// </summary>
    public class NebulasWhisper : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 58;
            Item.damage = 1050; // POST-FATE ULTIMATE RANGED - 40%+ above Fate tier
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3.5f;
            Item.value = Item.sellPrice(gold: 42);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.UseSound = SoundID.Item38 with { Pitch = 0.1f, Volume = 0.85f }; // Cosmic gun sound
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<NebulaArrowProjectile>();
            Item.shootSpeed = 20f;
            Item.useAmmo = AmmoID.Bullet;
            Item.crit = 18;
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Fire nebula arrow
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<NebulaArrowProjectile>(), damage, knockback, player.whoAmI);
            
            // Muzzle flash VFX
            Vector2 direction = velocity.SafeNormalize(Vector2.UnitX);
            NebulasWhisperVFX.MuzzleFlashVFX(position + direction * 15f, direction);
            
            return false;
        }
        
        public override void HoldItem(Player player)
        {
            NebulasWhisperVFX.HoldItemVFX(player);
        }
        
        public override Vector2? HoldoutOffset() => new Vector2(-3f, 0f);

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D tex = TextureAssets.Item[Item.type].Value;
            Vector2 pos = Item.Center - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;

            float time = Main.GameUpdateCount * 0.04f;
            // Slow nebula breathing — soft, atmospheric pulsing
            float breathe = 1f + (float)Math.Sin(time * 1.3f) * 0.12f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Diffuse cosmic purple nebula haze — wide, soft
            spriteBatch.Draw(tex, pos, null, NachtmusikPalette.CosmicPurple with { A = 0 } * 0.3f,
                rotation, origin, scale * breathe * 1.45f, SpriteEffects.None, 0f);

            // Nebula pink mid-layer — the whisper's color
            float nebulaShift = (float)Math.Sin(time * 1.8f) * 0.15f + 0.85f;
            spriteBatch.Draw(tex, pos, null, NachtmusikPalette.NebulaPink with { A = 0 } * 0.28f * nebulaShift,
                rotation, origin, scale * breathe * 1.25f, SpriteEffects.None, 0f);

            // Serenade glow inner warmth
            spriteBatch.Draw(tex, pos, null, NachtmusikPalette.SerenadeGlow with { A = 0 } * 0.22f,
                rotation, origin, scale * breathe * 1.1f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, NachtmusikPalette.NebulaPink.ToVector3() * 0.35f);
            return true;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D tex = TextureAssets.Item[Item.type].Value;
            float time = Main.GameUpdateCount * 0.035f;
            float breathe = 1f + (float)Math.Sin(time * 1.5f) * 0.08f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            // Nebula pink/purple cycling glow
            float cycle = (float)Math.Sin(time * 0.6f) * 0.5f + 0.5f;
            Color glowColor = Color.Lerp(NachtmusikPalette.CosmicPurple, NachtmusikPalette.NebulaPink, cycle) * 0.26f;
            spriteBatch.Draw(tex, position, frame, glowColor, 0f, origin, scale * breathe * 1.12f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            spriteBatch.Draw(tex, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            return false;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Split", "Shots split into 4 nebula fragments on first hit"));
            tooltips.Add(new TooltipLine(Mod, "Nebula", "Fragments spread cosmic damage across an area"));
            tooltips.Add(new TooltipLine(Mod, "Debuff", "Inflicts Celestial Harmony"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A whisper from the depths of space'")
            {
                OverrideColor = NachtmusikPalette.NebulaPink
            });
        }
    }
    
    /// <summary>
    /// Serenade of Distant Stars - A cosmic rifle that fires homing star projectiles.
    /// Each shot releases multiple stars that seek out enemies.
    /// DAMAGE: 580 per star, fires 4-5 stars
    /// </summary>
    public class SerenadeOfDistantStars : ModItem
    {
        private int shotCounter = 0;
        
        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 70;
            Item.damage = 920; // POST-FATE ULTIMATE RANGED - 36%+ above Fate tier
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 48);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.UseSound = SoundID.Item91 with { Pitch = 0.2f, Volume = 0.8f }; // Cosmic gun sound
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<SerenadeStarProjectile>();
            Item.shootSpeed = 16f;
            Item.useAmmo = AmmoID.Bullet;
            Item.crit = 20;
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            shotCounter++;
            
            // Fire 4-5 homing stars
            int starCount = 4 + (shotCounter % 3 == 0 ? 1 : 0);
            
            for (int i = 0; i < starCount; i++)
            {
                float angleOffset = MathHelper.ToRadians(-20f + 10f * i);
                Vector2 starVel = velocity.RotatedBy(angleOffset + Main.rand.NextFloat(-0.05f, 0.05f));
                
                // Stagger spawn positions slightly
                Vector2 starPos = position + Main.rand.NextVector2Circular(8f, 8f);
                
                Projectile.NewProjectile(source, starPos, starVel, ModContent.ProjectileType<SerenadeStarProjectile>(), damage, knockback, player.whoAmI);
            }
            
            // Serenade muzzle flash VFX
            Vector2 direction = velocity.SafeNormalize(Vector2.UnitX);
            SerenadeOfDistantStarsVFX.MuzzleFlashVFX(position + direction * 25f, direction);
            
            return false;
        }
        
        public override void HoldItem(Player player)
        {
            SerenadeOfDistantStarsVFX.HoldItemVFX(player);
        }
        
        public override Vector2? HoldoutOffset() => new Vector2(-6f, 0f);

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D tex = TextureAssets.Item[Item.type].Value;
            Vector2 pos = Item.Center - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;

            float time = Main.GameUpdateCount * 0.05f;
            // Warm starlit pulsing — like a distant star twinkling
            float twinkle = 1f + (float)Math.Sin(time * 2.5f) * 0.06f
                + (float)Math.Sin(time * 4.1f) * 0.03f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Deep blue stellar foundation
            spriteBatch.Draw(tex, pos, null, NachtmusikPalette.DeepBlue with { A = 0 } * 0.3f,
                rotation, origin, scale * twinkle * 1.35f, SpriteEffects.None, 0f);

            // Warm star gold melody glow — the serenade's warmth
            float warmPulse = (float)Math.Sin(time * 1.7f) * 0.5f + 0.5f;
            spriteBatch.Draw(tex, pos, null, NachtmusikPalette.StarGold with { A = 0 } * 0.3f * warmPulse,
                rotation, origin, scale * twinkle * 1.2f, SpriteEffects.None, 0f);

            // Moonlit silver highlight core
            spriteBatch.Draw(tex, pos, null, NachtmusikPalette.MoonlitSilver with { A = 0 } * 0.2f,
                rotation, origin, scale * twinkle * 1.08f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, NachtmusikPalette.StarGold.ToVector3() * 0.35f);
            return true;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D tex = TextureAssets.Item[Item.type].Value;
            float time = Main.GameUpdateCount * 0.04f;
            float twinkle = 1f + (float)Math.Sin(time * 2.3f) * 0.07f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            // Warm gold-to-silver cycling — serenade warmth
            float cycle = (float)Math.Sin(time * 0.7f) * 0.5f + 0.5f;
            Color glowColor = Color.Lerp(NachtmusikPalette.StarGold, NachtmusikPalette.MoonlitSilver, cycle) * 0.24f;
            spriteBatch.Draw(tex, position, frame, glowColor, 0f, origin, scale * twinkle * 1.1f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            spriteBatch.Draw(tex, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            return false;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Stars", "Fires 4-5 homing star projectiles per shot"));
            tooltips.Add(new TooltipLine(Mod, "Homing", "Stars aggressively home in on enemies"));
            tooltips.Add(new TooltipLine(Mod, "Debuff", "Inflicts stacking Celestial Harmony"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A love song sung by the stars themselves'")
            {
                OverrideColor = NachtmusikPalette.StarWhite
            });
        }
    }
}
