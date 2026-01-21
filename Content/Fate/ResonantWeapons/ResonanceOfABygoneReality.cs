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
    /// Resonance of a Bygone Reality - Echoes of a universe that once was.
    /// A rapid-fire cosmic gun. Every 5th hit spawns a spectral blade that slashes enemies for 3 seconds.
    /// </summary>
    public class ResonanceOfABygoneReality : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/ResonanceOfABygoneReality";
        
        public override void SetDefaults()
        {
            Item.damage = 400;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 50;
            Item.height = 26;
            Item.useTime = 6;
            Item.useAnimation = 6;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 2f;
            Item.value = Item.sellPrice(gold: 52);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<FateRapidBullet>();
            Item.shootSpeed = 18f;
            Item.useAmmo = AmmoID.Bullet;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Rapid-fire cosmic bullets"));
            tooltips.Add(new TooltipLine(Mod, "FateSpecial", "Every 5th hit summons a spectral blade that slashes for 3 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Memories of a world erased from existence'")
            {
                OverrideColor = FateCosmicVFX.FateBrightRed
            });
        }
        
        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            return Main.rand.NextFloat() > 0.4f; // 40% chance to not consume
        }
        
        public override void HoldItem(Player player)
        {
            // === COSMIC REPEATER HOLD EFFECT ===
            // Rapid energy particles flowing toward weapon
            if (Main.rand.NextBool(4))
            {
                Vector2 spawnPos = player.Center + Main.rand.NextVector2Circular(50f, 50f);
                Vector2 toPlayer = (player.Center - spawnPos).SafeNormalize(Vector2.Zero) * 2f;
                Color sparkColor = FateCosmicVFX.GetCosmicGradient(Main.rand.NextFloat());
                var spark = new GlowSparkParticle(spawnPos, toPlayer, sparkColor, 0.18f, 12);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // Glyphs near weapon position
            if (Main.rand.NextBool(15))
            {
                Vector2 weaponPos = player.Center + new Vector2(player.direction * 25f, -5f);
                CustomParticles.Glyph(weaponPos + Main.rand.NextVector2Circular(15f, 15f), FateCosmicVFX.FateDarkPink, 0.25f, -1);
            }
            
            // Star particles ambient
            if (Main.rand.NextBool(8))
            {
                var star = new GenericGlowParticle(player.Center + Main.rand.NextVector2Circular(30f, 30f),
                    Main.rand.NextVector2Circular(0.4f, 0.4f), FateCosmicVFX.FateWhite, 0.15f, 14, true);
                MagnumParticleHandler.SpawnParticle(star);
            }
            
            // Energy buildup light
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.1f + 0.9f;
            Lighting.AddLight(player.Center, FateCosmicVFX.FateBrightRed.ToVector3() * pulse * 0.35f);
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Spawn rapid bullet with slight spread
            Vector2 bulletVel = velocity.RotatedByRandom(0.05f);
            Projectile.NewProjectile(source, position, bulletVel, ModContent.ProjectileType<FateRapidBullet>(), damage, knockback, player.whoAmI);
            
            // Track for star circle effect
            player.GetModPlayer<FateWeaponEffectPlayer>()?.OnFateWeaponAttack(player.Center);
            
            // Muzzle flash VFX
            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.Zero) * 30f;
            FateCosmicVFX.SpawnCosmicExplosion(muzzlePos, 0.3f);
            
            // Occasional glyph
            if (Main.rand.NextBool(5))
            {
                FateCosmicVFX.SpawnGlyphBurst(muzzlePos, 1, 3f, 0.2f);
            }
            
            return false;
        }
    }
}
