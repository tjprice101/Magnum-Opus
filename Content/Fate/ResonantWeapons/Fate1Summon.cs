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
    /// Fate1Summon - Cosmic Deity Staff
    /// Summons a cosmic deity that rapidly slashes enemies and fires cosmic light beams.
    /// </summary>
    public class Fate1Summon : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.StardustDragonStaff;
        
        public override void SetDefaults()
        {
            Item.damage = 400;
            Item.DamageType = DamageClass.Summon;
            Item.width = 44;
            Item.height = 44;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(gold: 55);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.mana = 20;
            Item.shoot = ModContent.ProjectileType<CosmicDeityMinion>();
            Item.buffType = ModContent.BuffType<CosmicDeityBuff>();
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Summons a cosmic deity that rapidly slashes enemies"));
            tooltips.Add(new TooltipLine(Mod, "FateSpecial", "The deity periodically fires cosmic light beams"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The cosmos made manifest, a god of stars at your command'")
            {
                OverrideColor = FateCosmicVFX.FateBrightRed
            });
        }
        
        public override void HoldItem(Player player)
        {
            // === COSMIC DEITY STAFF HOLD EFFECT ===
            // Divine constellation pattern
            if (Main.rand.NextBool(7))
            {
                // 6-point star formation
                float baseAngle = Main.GameUpdateCount * 0.02f;
                int point = Main.rand.Next(6);
                float starAngle = baseAngle + MathHelper.TwoPi * point / 6f;
                Vector2 starPos = player.Center + starAngle.ToRotationVector2() * 45f;
                var star = new GenericGlowParticle(starPos, Vector2.Zero, FateCosmicVFX.FateStarGold, 0.25f, 20, true);
                MagnumParticleHandler.SpawnParticle(star);
            }
            
            // Orbiting deity essence glyphs
            if (Main.rand.NextBool(10))
            {
                float glyphAngle = Main.GameUpdateCount * 0.035f;
                for (int i = 0; i < 2; i++)
                {
                    Vector2 glyphPos = player.Center + (glyphAngle + MathHelper.Pi * i).ToRotationVector2() * 50f;
                    CustomParticles.Glyph(glyphPos, FateCosmicVFX.FateDarkPink, 0.38f, -1);
                }
            }
            
            // Cosmic cloud aura
            if (Main.rand.NextBool(15))
            {
                FateCosmicVFX.SpawnCosmicCloudTrail(player.Center + Main.rand.NextVector2Circular(30f, 30f),
                    Main.rand.NextVector2Circular(0.5f, 0.5f), 0.3f);
            }
            
            // Divine radiance light
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.2f + 0.8f;
            Lighting.AddLight(player.Center, FateCosmicVFX.FateWhite.ToVector3() * pulse * 0.35f);
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            
            // Spawn deity at cursor
            Vector2 spawnPos = Main.MouseWorld;
            Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback, player.whoAmI);
            
            // Track for star circle effect
            player.GetModPlayer<FateWeaponEffectPlayer>()?.OnFateWeaponAttack(player.Center);
            
            // Spawn cosmic explosion effect at summon location
            FateCosmicVFX.SpawnCosmicExplosion(spawnPos, 1.2f);
            FateCosmicVFX.SpawnGlyphBurst(spawnPos, 6, 6f, 0.5f);
            
            // Star particles for celestial appearance
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 starVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                var star = new GlowSparkParticle(spawnPos, starVel, FateCosmicVFX.FateWhite, 0.3f, 20);
                MagnumParticleHandler.SpawnParticle(star);
            }
            
            return false;
        }
    }
}
