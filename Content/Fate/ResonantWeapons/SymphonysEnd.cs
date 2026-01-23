using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
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
            // === SPECTRAL BLADE STORM HOLD EFFECT ===
            // Ghostly blade echoes orbiting
            if (Main.rand.NextBool(8))
            {
                float angle = Main.GameUpdateCount * 0.05f + Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 bladePos = player.Center + angle.ToRotationVector2() * Main.rand.NextFloat(40f, 60f);
                Color bladeColor = Color.Lerp(FateCosmicVFX.FateDarkPink, FateCosmicVFX.FateWhite, Main.rand.NextFloat());
                var blade = new GenericGlowParticle(bladePos, angle.ToRotationVector2() * 0.8f, bladeColor * 0.6f, 0.22f, 16, true);
                MagnumParticleHandler.SpawnParticle(blade);
            }
            
            // Glyphs in storm pattern
            if (Main.rand.NextBool(12))
            {
                float stormAngle = Main.GameUpdateCount * 0.07f;
                float stormRadius = 35f + (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 15f;
                CustomParticles.Glyph(player.Center + stormAngle.ToRotationVector2() * stormRadius, 
                    FateCosmicVFX.FatePurple, 0.3f, -1);
            }
            
            // Star sparkles in tempest
            if (Main.rand.NextBool(6))
            {
                var star = new GenericGlowParticle(player.Center + Main.rand.NextVector2Circular(45f, 45f),
                    Main.rand.NextVector2Circular(1f, 1f), FateCosmicVFX.FateWhite, 0.17f, 15, true);
                MagnumParticleHandler.SpawnParticle(star);
            }
            
            // Storm energy light
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.15f + 0.85f;
            Lighting.AddLight(player.Center, FateCosmicVFX.FateDarkPink.ToVector3() * pulse * 0.4f);
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
            
            // Spawn VFX at blade origin
            FateCosmicVFX.SpawnGlyphBurst(spawnPos, 1, 3f, 0.2f);
            
            // Occasional cosmic spark at player
            if (Main.rand.NextBool(3))
            {
                var spark = new GlowSparkParticle(player.Center + Main.rand.NextVector2Circular(20f, 20f),
                    Main.rand.NextVector2Circular(2f, 2f), FateCosmicVFX.FateDarkPink, 0.2f, 10);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            return false;
        }
    }
}
