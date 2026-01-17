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
    /// Fate4Sword - Spectral Blade Master
    /// A true melee sword. On hit, spawns 3 spectral blades that orbit and attack enemies for 10 seconds.
    /// The spectral blades shoot prismatic beams that grow larger and more colorful over time.
    /// </summary>
    public class Fate4Sword : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.Zenith;
        
        public override void SetDefaults()
        {
            Item.damage = 750;
            Item.DamageType = DamageClass.Melee;
            Item.width = 60;
            Item.height = 60;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 7f;
            Item.value = Item.sellPrice(gold: 58);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            // No projectile on swing - true melee
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "True melee strikes summon 3 spectral blades for 10 seconds"));
            tooltips.Add(new TooltipLine(Mod, "FateSpecial", "Spectral blades orbit and fire prismatic beams that intensify over time"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Three blades of fate orbit the master of destiny'")
            {
                OverrideColor = FateCosmicVFX.FateBrightRed
            });
        }
        
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            Vector2 swingPos = hitbox.Center.ToVector2();
            
            // Cosmic sparks from swing
            if (Main.rand.NextBool(2))
            {
                Color sparkColor = FateCosmicVFX.GetCosmicGradient(Main.rand.NextFloat());
                Vector2 sparkVel = new Vector2(player.direction * 4f, Main.rand.NextFloat(-3f, 3f));
                var spark = new GlowSparkParticle(swingPos + Main.rand.NextVector2Circular(18f, 18f), sparkVel, sparkColor, 0.28f, 14);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // Occasional glyph
            if (Main.rand.NextBool(6))
            {
                FateCosmicVFX.SpawnGlyphBurst(swingPos, 1, 3f, 0.25f);
            }
        }
        
        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);
            
            // Track for star circle effect
            player.GetModPlayer<FateWeaponEffectPlayer>()?.OnFateWeaponAttack(player.Center);
            
            // Spawn 3 orbiting spectral blades
            for (int i = 0; i < 3; i++)
            {
                float startAngle = MathHelper.TwoPi * i / 3f;
                Projectile.NewProjectile(
                    player.GetSource_OnHit(target),
                    player.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<OrbitingSpectralBlade>(),
                    (int)(damageDone * 0.4f),
                    4f,
                    player.whoAmI,
                    startAngle, // ai[0] = starting angle offset
                    0f // ai[1] = timer (starts at 0)
                );
            }
            
            // Impact VFX
            FateCosmicVFX.SpawnCosmicExplosion(target.Center, 1.0f);
            FateCosmicVFX.SpawnGlyphBurst(target.Center, 6, 7f, 0.4f);
            FateCosmicVFX.SpawnCosmicMusicNotes(target.Center, 4, 30f, 0.3f);
            
            // Star particle burst
            for (int i = 0; i < 10; i++)
            {
                Vector2 starOffset = Main.rand.NextVector2Circular(35f, 35f);
                var star = new GenericGlowParticle(target.Center + starOffset, Main.rand.NextVector2Circular(3f, 3f), 
                    FateCosmicVFX.FateWhite, 0.3f, 22, true);
                MagnumParticleHandler.SpawnParticle(star);
            }
            
            SoundEngine.PlaySound(SoundID.Item105 with { Pitch = 0.2f, Volume = 0.8f }, target.Center);
            
            Lighting.AddLight(target.Center, FateCosmicVFX.FateBrightRed.ToVector3() * 1.5f);
        }
    }
}
