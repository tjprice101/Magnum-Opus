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
    /// The Final Fermata - The last pause before eternal silence.
    /// Channels cosmic lightning toward the cursor. When 3+ unique enemies are hit,
    /// triggers a screen-wide zodiac explosion.
    /// </summary>
    public class TheFinalFermata : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/TheFinalFermata";
        
        public override void SetDefaults()
        {
            Item.damage = 460;
            Item.DamageType = DamageClass.Magic;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 2f;
            Item.value = Item.sellPrice(gold: 55);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item15;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.mana = 15;
            Item.shoot = ModContent.ProjectileType<CosmicElectricityStaff>();
            Item.shootSpeed = 1f;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Hold to channel cosmic lightning toward the cursor"));
            tooltips.Add(new TooltipLine(Mod, "FateSpecial", "Hitting 3+ unique enemies triggers a devastating zodiac explosion"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'In the silence between notes, worlds are born and die'")
            {
                OverrideColor = FateCosmicVFX.FateBrightRed
            });
        }
        
        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[ModContent.ProjectileType<CosmicElectricityStaff>()] < 1;
        }
        
        public override void HoldItem(Player player)
        {
            // === COSMIC LIGHTNING STAFF HOLD EFFECT ===
            // Electric arcs around player
            if (Main.rand.NextBool(6))
            {
                Vector2 arcStart = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                Vector2 arcEnd = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                Color arcColor = Main.rand.NextBool() ? FateCosmicVFX.FateCyan : FateCosmicVFX.FateBrightRed;
                
                // Small electric sparks along arc
                for (int i = 0; i < 3; i++)
                {
                    Vector2 sparkPos = Vector2.Lerp(arcStart, arcEnd, (float)i / 3f);
                    var spark = new GlowSparkParticle(sparkPos, Main.rand.NextVector2Circular(1f, 1f), arcColor, 0.15f, 8);
                    MagnumParticleHandler.SpawnParticle(spark);
                }
            }
            
            // Zodiac glyphs floating
            if (Main.rand.NextBool(10))
            {
                float angle = Main.GameUpdateCount * 0.03f + Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 glyphPos = player.Center + angle.ToRotationVector2() * Main.rand.NextFloat(35f, 55f);
                CustomParticles.Glyph(glyphPos, FateCosmicVFX.FateDarkPink, 0.35f, -1);
            }
            
            // Star particles (zodiac stars)
            if (Main.rand.NextBool(7))
            {
                var star = new GenericGlowParticle(player.Center + Main.rand.NextVector2Circular(40f, 40f),
                    Main.rand.NextVector2Circular(0.3f, 0.3f), FateCosmicVFX.FateWhite, 0.2f, 18, true);
                MagnumParticleHandler.SpawnParticle(star);
            }
            
            // Electric cosmic glow
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.09f) * 0.18f + 0.82f;
            Lighting.AddLight(player.Center, FateCosmicVFX.FateCyan.ToVector3() * pulse * 0.45f);
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Track for star circle effect
            player.GetModPlayer<FateWeaponEffectPlayer>()?.OnFateWeaponAttack(player.Center);
            
            // Spawn channeled lightning staff
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            
            // Initial cast VFX
            FateCosmicVFX.SpawnGlyphBurst(player.Center, 4, 5f, 0.4f);
            
            return false;
        }
    }
}
