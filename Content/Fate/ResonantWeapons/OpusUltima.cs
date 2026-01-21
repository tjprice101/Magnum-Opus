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
    /// Opus Ultima - The final masterwork blade.
    /// Fires a big ball of cosmic energy that explodes into 5 smaller seeker balls on enemy hit.
    /// </summary>
    public class OpusUltima : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/OpusUltima";
        
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
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The ultimate composition, the magnum opus of destruction'")
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
        
        public override void HoldItem(Player player)
        {
            // === CELESTIAL COSMIC HOLD EFFECT ===
            // Orbiting energy spheres
            if (Main.rand.NextBool(10))
            {
                float angle = Main.GameUpdateCount * 0.035f;
                for (int i = 0; i < 2; i++)
                {
                    float orbitAngle = angle + MathHelper.Pi * i;
                    Vector2 orbitPos = player.Center + orbitAngle.ToRotationVector2() * 40f;
                    var orb = new GenericGlowParticle(orbitPos, Vector2.Zero, FateCosmicVFX.GetCosmicGradient((float)i / 2f), 0.3f, 12, true);
                    MagnumParticleHandler.SpawnParticle(orb);
                }
            }
            
            // Star particle aura
            if (Main.rand.NextBool(7))
            {
                Vector2 offset = Main.rand.NextVector2Circular(38f, 38f);
                var star = new GenericGlowParticle(player.Center + offset, Main.rand.NextVector2Circular(0.4f, 0.4f), 
                    FateCosmicVFX.FateWhite, 0.2f, 16, true);
                MagnumParticleHandler.SpawnParticle(star);
            }
            
            // Occasional glyph
            if (Main.rand.NextBool(15))
            {
                CustomParticles.Glyph(player.Center + Main.rand.NextVector2Circular(30f, 30f), FateCosmicVFX.FatePurple, 0.3f, -1);
            }
            
            // Ambient light
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.055f) * 0.12f + 0.88f;
            Lighting.AddLight(player.Center, FateCosmicVFX.FateDarkPink.ToVector3() * pulse * 0.35f);
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
