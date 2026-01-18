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
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Fate.Debuffs;
using MagnumOpus.Content.Fate.Projectiles;

namespace MagnumOpus.Content.Fate.ResonantWeapons
{
    /// <summary>
    /// Fate2Sword - Cosmic Splitter
    /// Fires a big ball of cosmic energy that explodes into 5 smaller seeker balls on enemy hit.
    /// </summary>
    public class Fate2Sword : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.Zenith;
        
        public override void SetDefaults()
        {
            Item.damage = 720;
            Item.DamageType = DamageClass.Melee;
            Item.width = 50;
            Item.height = 50;
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(gold: 52);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<CosmicEnergyBall>();
            Item.shootSpeed = 12f;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Swings release a large cosmic energy ball"));
            tooltips.Add(new TooltipLine(Mod, "FateSpecial", "On enemy hit, explodes into 5 homing seeker balls"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The cosmos fragments into seeking vengeance'")
            {
                OverrideColor = FateCosmicVFX.FateBrightRed
            });
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Spawn the big cosmic energy ball
            Vector2 spawnPos = player.Center + velocity.SafeNormalize(Vector2.Zero) * 40f;
            Projectile.NewProjectile(source, spawnPos, velocity, type, damage, knockback, player.whoAmI);
            
            // Track for star circle effect
            player.GetModPlayer<FateWeaponEffectPlayer>()?.OnFateWeaponAttack(player.Center);
            
            // Cosmic spawn VFX
            FateCosmicVFX.SpawnCosmicExplosion(spawnPos, 0.6f);
            FateCosmicVFX.SpawnGlyphBurst(spawnPos, 3, 4f, 0.3f);
            
            SoundEngine.PlaySound(SoundID.Item117 with { Pitch = 0.3f, Volume = 0.9f }, player.Center);
            
            return false;
        }
        
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            Vector2 swingPos = hitbox.Center.ToVector2();
            
            // Cosmic sparks from swing
            if (Main.rand.NextBool(2))
            {
                Color sparkColor = FateCosmicVFX.GetCosmicGradient(Main.rand.NextFloat());
                Vector2 sparkVel = new Vector2(player.direction * 3f, Main.rand.NextFloat(-2f, 2f));
                var spark = new GlowSparkParticle(swingPos + Main.rand.NextVector2Circular(15f, 15f), sparkVel, sparkColor, 0.22f, 12);
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }
        
        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 240);
            
            // Impact VFX with glyphs and stars
            FateCosmicVFX.SpawnCosmicExplosion(target.Center, 0.8f);
            FateCosmicVFX.SpawnGlyphBurst(target.Center, 4, 5f, 0.35f);
            
            // Star particles
            for (int i = 0; i < 6; i++)
            {
                Vector2 starOffset = Main.rand.NextVector2Circular(30f, 30f);
                var star = new GenericGlowParticle(target.Center + starOffset, Main.rand.NextVector2Circular(2f, 2f), 
                    FateCosmicVFX.FateWhite, 0.25f, 18, true);
                MagnumParticleHandler.SpawnParticle(star);
            }
            
            Lighting.AddLight(target.Center, FateCosmicVFX.FateBrightRed.ToVector3() * 1.2f);
        }
    }
}
