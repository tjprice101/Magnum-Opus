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
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Fate.Debuffs;
using MagnumOpus.Content.Fate.Projectiles;

namespace MagnumOpus.Content.Fate.ResonantWeapons
{
    /// <summary>
    /// Symphony's End - Where all melodies find their conclusion.
    /// Spawns random spectral sword blades that spiral toward the cursor and explode on contact.
    /// Enhanced with cosmic VFX, glowing world sprite, and musical identity.
    /// </summary>
    public class SymphonysEnd : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/SymphonysEnd";
        
        public override void SetDefaults()
        {
            Item.damage = 500;
            Item.DamageType = DamageClass.Magic;
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 8;
            Item.useAnimation = 8;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 55);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item8;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.mana = 8;
            Item.shoot = ModContent.ProjectileType<SpiralingSpectralBlade>();
            Item.shootSpeed = 10f;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Spawns aggressive spectral blades that hunt enemies"));
            tooltips.Add(new TooltipLine(Mod, "FateSpecial", "Blades dash at targets rapidly and explode on contact"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Every song must end, but this ending reshapes the cosmos'")
            {
                OverrideColor = FateCosmicVFX.FateBrightRed
            });
        }
        
        public override void HoldItem(Player player)
        {
            // === ENHANCED SPECTRAL BLADE STORM HOLD EFFECT ===
            float time = Main.GameUpdateCount * 0.05f;
            
            // === LAYER 1: Ghostly blade echoes orbiting (3 distinct blades) ===
            for (int blade = 0; blade < 3; blade++)
            {
                float bladeAngle = time * 1.2f + MathHelper.TwoPi * blade / 3f;
                float bladeRadius = 45f + (float)Math.Sin(time * 2f + blade) * 8f;
                Vector2 bladePos = player.Center + bladeAngle.ToRotationVector2() * bladeRadius;
                
                if (Main.GameUpdateCount % 6 == blade * 2)
                {
                    Color bladeColor = Color.Lerp(FateCosmicVFX.FateDarkPink, FateCosmicVFX.FateWhite, blade / 3f);
                    var bladeGlow = new GenericGlowParticle(bladePos, bladeAngle.ToRotationVector2() * 1f, bladeColor * 0.65f, 0.25f, 14, true);
                    MagnumParticleHandler.SpawnParticle(bladeGlow);
                }
            }
            
            // === LAYER 2: Glyphs in storm pattern (6-point cosmic mandala) ===
            if (Main.rand.NextBool(10))
            {
                for (int i = 0; i < 6; i++)
                {
                    float glyphAngle = time * 0.6f + MathHelper.TwoPi * i / 6f;
                    float glyphRadius = 55f + (float)Math.Sin(time + i * 0.5f) * 10f;
                    Vector2 glyphPos = player.Center + glyphAngle.ToRotationVector2() * glyphRadius;
                    Color glyphColor = Color.Lerp(FateCosmicVFX.FatePurple, FateCosmicVFX.FateDarkPink, (float)i / 6f);
                    CustomParticles.Glyph(glyphPos, glyphColor, 0.28f, -1);
                }
            }
            
            // === LAYER 3: Inner star ring (counter-rotating) ===
            if (Main.GameUpdateCount % 8 == 0)
            {
                for (int star = 0; star < 4; star++)
                {
                    float starAngle = -time * 1.5f + MathHelper.TwoPi * star / 4f;
                    Vector2 starPos = player.Center + starAngle.ToRotationVector2() * 25f;
                    CustomParticles.GenericFlare(starPos, FateCosmicVFX.FateWhite, 0.2f, 10);
                }
            }
            
            // === LAYER 4: Cosmic dust motes drifting upward ===
            if (Main.rand.NextBool(5))
            {
                Vector2 dustPos = player.Center + new Vector2(Main.rand.NextFloat(-50f, 50f), Main.rand.NextFloat(20f, 50f));
                var dust = new GenericGlowParticle(dustPos, new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.7f),
                    FateCosmicVFX.FatePurple * 0.5f, 0.12f, 28, true);
                MagnumParticleHandler.SpawnParticle(dust);
            }
            
            // === LAYER 5: Music notes rising (VISIBLE - scale 0.7f+) ===
            if (Main.rand.NextBool(12))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), -1.2f);
                Color noteColor = Color.Lerp(FateCosmicVFX.FateDarkPink, FateCosmicVFX.FateWhite, Main.rand.NextFloat());
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.75f, 40);
            }
            
            // === COSMIC LIGHT AURA with color shift ===
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.2f + 0.8f;
            float hueShift = (Main.GameUpdateCount * 0.005f) % 1f;
            Color auraColor = Color.Lerp(FateCosmicVFX.FateDarkPink, FateCosmicVFX.FatePurple, (float)Math.Sin(hueShift * MathHelper.TwoPi) * 0.5f + 0.5f);
            Lighting.AddLight(player.Center, auraColor.ToVector3() * pulse * 0.5f);
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Track for star circle effect
            player.GetModPlayer<FateWeaponEffectPlayer>()?.OnFateWeaponAttack(player.Center);
            
            // Spawn blade at random offset from player
            Vector2 spawnOffset = Main.rand.NextVector2CircularEdge(60f, 60f);
            Vector2 spawnPos = player.Center + spawnOffset;
            
            // Direction toward cursor with spiral component
            Vector2 toCursor = (Main.MouseWorld - spawnPos).SafeNormalize(Vector2.UnitX);
            float spiralAngle = Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4);
            Vector2 spiralVel = toCursor.RotatedBy(spiralAngle) * velocity.Length();
            
            Projectile.NewProjectile(source, spawnPos, spiralVel, type, damage, knockback, player.whoAmI, 
                Main.MouseWorld.X, Main.MouseWorld.Y);
            
            // === ENHANCED SPAWN VFX ===
            // Layered glyph + flare burst at blade origin
            FateCosmicVFX.SpawnGlyphBurst(spawnPos, 2, 4f, 0.25f);
            CustomParticles.GenericFlare(spawnPos, FateCosmicVFX.FateWhite, 0.45f, 12);
            CustomParticles.HaloRing(spawnPos, FateCosmicVFX.FateDarkPink * 0.7f, 0.25f, 10);
            
            // Cosmic sparks radiating outward
            for (int i = 0; i < 4; i++)
            {
                float sparkAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 sparkVel = sparkAngle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                var spark = new GlowSparkParticle(spawnPos, sparkVel, 
                    Color.Lerp(FateCosmicVFX.FateDarkPink, FateCosmicVFX.FateWhite, Main.rand.NextFloat()), 0.22f, 12);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // Music note at spawn (VISIBLE scale)
            if (Main.rand.NextBool(2))
            {
                ThemedParticles.MusicNote(spawnPos, spiralVel * 0.3f, FateCosmicVFX.FateDarkPink, 0.7f, 25);
            }
            
            // Bright spawn flash
            Lighting.AddLight(spawnPos, FateCosmicVFX.FateDarkPink.ToVector3() * 0.8f);
            
            return false;
        }
        
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            // === COSMIC GLOWING WORLD SPRITE ===
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            // Pulsing scale for visual appeal
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.15f + 1f;
            
            // Switch to additive blending for glow layers
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Outer cosmic pink glow
            spriteBatch.Draw(texture, position, null, FateCosmicVFX.FateDarkPink * 0.45f, rotation, origin, scale * pulse * 1.4f, SpriteEffects.None, 0f);
            
            // Middle purple glow
            spriteBatch.Draw(texture, position, null, FateCosmicVFX.FatePurple * 0.35f, rotation, origin, scale * pulse * 1.2f, SpriteEffects.None, 0f);
            
            // Inner white-hot core
            spriteBatch.Draw(texture, position, null, FateCosmicVFX.FateWhite * 0.3f, rotation, origin, scale * pulse * 1.05f, SpriteEffects.None, 0f);
            
            // Restore normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Draw main sprite
            spriteBatch.Draw(texture, position, null, lightColor, rotation, origin, scale, SpriteEffects.None, 0f);
            
            // Cosmic world light
            Lighting.AddLight(Item.Center, FateCosmicVFX.FateDarkPink.ToVector3() * 0.6f);
            
            return false;
        }
        
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            // Subtle pulse effect in inventory for visual feedback
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.08f + 1f;
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            
            // Draw glow behind
            spriteBatch.Draw(texture, position, frame, FateCosmicVFX.FateDarkPink * 0.25f, 0f, origin, scale * pulse * 1.1f, SpriteEffects.None, 0f);
            
            // Draw main item
            spriteBatch.Draw(texture, position, frame, drawColor, 0f, origin, scale * pulse, SpriteEffects.None, 0f);
            
            return false;
        }
    }
}
