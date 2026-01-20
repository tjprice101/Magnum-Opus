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
    /// Fate2Ranged - Cosmic Accelerator
    /// Fires slow rounds that accelerate, pierce enemies, and explode on walls/timeout.
    /// On explosion, spawns 3 homing cosmic rockets.
    /// </summary>
    public class Fate2Ranged : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.SniperRifle;
        
        public override void SetDefaults()
        {
            Item.damage = 680;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 58;
            Item.height = 22;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(gold: 55);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item40;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<AcceleratingCosmicRound>();
            Item.shootSpeed = 6f; // Starts slow
            Item.useAmmo = AmmoID.Bullet;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Fires slow rounds that accelerate as they travel"));
            tooltips.Add(new TooltipLine(Mod, "FateSpecial1", "Rounds pierce enemies and explode on impact with tiles"));
            tooltips.Add(new TooltipLine(Mod, "FateSpecial2", "Explosions spawn 3 homing cosmic rockets"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Time bends as the cosmos accelerates toward annihilation'")
            {
                OverrideColor = FateCosmicVFX.FateBrightRed
            });
        }
        
        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            return Main.rand.NextFloat() > 0.5f; // 50% ammo conservation
        }
        
        public override void HoldItem(Player player)
        {
            // === COSMIC ACCELERATOR HOLD EFFECT ===
            // Spiraling energy particles (representing acceleration)
            if (Main.rand.NextBool(5))
            {
                float spiralAngle = Main.GameUpdateCount * 0.06f;
                float spiralRadius = 30f + (Main.GameUpdateCount % 60) * 0.5f;
                Vector2 spiralPos = player.Center + spiralAngle.ToRotationVector2() * spiralRadius;
                Color spiralColor = FateCosmicVFX.GetCosmicGradient((spiralRadius - 30f) / 30f);
                var spiral = new GenericGlowParticle(spiralPos, -spiralAngle.ToRotationVector2() * 1f, spiralColor, 0.2f, 15, true);
                MagnumParticleHandler.SpawnParticle(spiral);
            }
            
            // Heavy cosmic cloud wisps
            if (Main.rand.NextBool(12))
            {
                FateCosmicVFX.SpawnCosmicCloudTrail(player.Center + Main.rand.NextVector2Circular(25f, 25f), 
                    Main.rand.NextVector2Circular(1f, 1f), 0.35f);
            }
            
            // Orbiting glyphs
            if (Main.rand.NextBool(10))
            {
                float angle = Main.GameUpdateCount * 0.04f;
                CustomParticles.Glyph(player.Center + angle.ToRotationVector2() * 42f, FateCosmicVFX.FatePurple, 0.32f, -1);
            }
            
            // Star particles
            if (Main.rand.NextBool(9))
            {
                var star = new GenericGlowParticle(player.Center + Main.rand.NextVector2Circular(35f, 35f),
                    Main.rand.NextVector2Circular(0.5f, 0.5f), FateCosmicVFX.FateStarGold, 0.18f, 16, true);
                MagnumParticleHandler.SpawnParticle(star);
            }
            
            // Accelerating intensity light
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.15f + 0.85f;
            Lighting.AddLight(player.Center, FateCosmicVFX.FateDarkPink.ToVector3() * pulse * 0.4f);
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Fire the accelerating round
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<AcceleratingCosmicRound>(), damage, knockback, player.whoAmI);
            
            // Track for star circle effect
            player.GetModPlayer<FateWeaponEffectPlayer>()?.OnFateWeaponAttack(player.Center);
            
            // Heavy muzzle flash
            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.Zero) * 40f;
            FateCosmicVFX.SpawnCosmicExplosion(muzzlePos, 0.5f);
            FateCosmicVFX.SpawnGlyphBurst(muzzlePos, 2, 4f, 0.3f);
            
            // Cosmic cloud at muzzle
            FateCosmicVFX.SpawnCosmicCloudTrail(muzzlePos, -velocity.SafeNormalize(Vector2.Zero) * 2f, 4);
            
            SoundEngine.PlaySound(SoundID.Item38 with { Pitch = -0.3f }, position);
            
            return false;
        }
    }
}
