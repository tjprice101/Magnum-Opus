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
    /// Fractal of the Stars - A blade containing infinite celestial patterns.
    /// A true melee sword. On hit, spawns 3 spectral blades that orbit and attack enemies for 10 seconds.
    /// The spectral blades shoot prismatic beams that grow larger and more colorful over time.
    /// </summary>
    public class FractalOfTheStars : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/FractalOfTheStars";
        
        public override void SetDefaults()
        {
            Item.damage = 850;
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
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Each facet reflects a different universe's end'")
            {
                OverrideColor = FatePalette.BrightCrimson
            });
        }
        
        public override void HoldItem(Player player)
        {
            FractalOfTheStarsVFX.HoldItemVFX(player);
        }
        
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            FractalOfTheStarsVFX.SwingVFX(hitbox.Center.ToVector2(), player);
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
            FractalOfTheStarsVFX.ImpactVFX(target.Center, damageDone);

            SoundEngine.PlaySound(SoundID.Item105 with { Pitch = 0.2f, Volume = 0.8f }, target.Center);
            
            // Spawn seeking crystals on every hit - Fate endgame power
            SeekingCrystalHelper.SpawnFateCrystals(
                player.GetSource_ItemUse(Item),
                target.Center,
                (Main.MouseWorld - target.Center).SafeNormalize(Vector2.UnitX) * 10f,
                (int)(damageDone * 0.2f),
                Item.knockBack * 0.4f,
                player.whoAmI,
                4);
        }
    }
}
