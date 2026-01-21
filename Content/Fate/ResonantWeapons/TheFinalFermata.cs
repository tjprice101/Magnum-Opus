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
    /// Spawns 3 spectral Coda of Annihilation swords that orbit the player,
    /// then cast themselves at the nearest enemy and slash through twice.
    /// </summary>
    public class TheFinalFermata : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/TheFinalFermata";
        
        public override void SetDefaults()
        {
            Item.damage = 520;
            Item.DamageType = DamageClass.Magic;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 45;
            Item.useAnimation = 45;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 55);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item105 with { Pitch = 0.2f, Volume = 0.8f };
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.mana = 30;
            Item.shoot = ModContent.ProjectileType<FermataSpectralSword>();
            Item.shootSpeed = 1f;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Summons 3 spectral Coda blades that orbit you"));
            tooltips.Add(new TooltipLine(Mod, "FateSpecial", "The blades lock onto the nearest enemy and slash through twice"));
            tooltips.Add(new TooltipLine(Mod, "FateSpecial2", "Each blade deals massive damage on both passes"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'In the silence between notes, worlds are born and die'")
            {
                OverrideColor = FateCosmicVFX.FateBrightRed
            });
        }
        
        public override bool CanUseItem(Player player)
        {
            // Limit concurrent spectral swords
            return player.ownedProjectileCounts[ModContent.ProjectileType<FermataSpectralSword>()] < 6;
        }
        
        public override void HoldItem(Player player)
        {
            // === COSMIC SPECTRAL SWORD HOLD EFFECT ===
            // Faint spectral swords orbiting at the ready
            if (Main.rand.NextBool(8))
            {
                float angle = Main.GameUpdateCount * 0.04f + Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 orbitPos = player.Center + angle.ToRotationVector2() * Main.rand.NextFloat(50f, 70f);
                Color swordColor = Main.rand.NextBool() ? FateCosmicVFX.FateDarkPink : FateCosmicVFX.FatePurple;
                
                var glow = new GenericGlowParticle(orbitPos, angle.ToRotationVector2() * 1f, swordColor * 0.4f, 0.2f, 15, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Zodiac glyphs floating
            if (Main.rand.NextBool(10))
            {
                float angle = Main.GameUpdateCount * 0.03f + Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 glyphPos = player.Center + angle.ToRotationVector2() * Main.rand.NextFloat(35f, 55f);
                CustomParticles.Glyph(glyphPos, FateCosmicVFX.FateDarkPink, 0.35f, -1);
            }
            
            // Star particles
            if (Main.rand.NextBool(7))
            {
                var star = new GenericGlowParticle(player.Center + Main.rand.NextVector2Circular(40f, 40f),
                    Main.rand.NextVector2Circular(0.3f, 0.3f), FateCosmicVFX.FateWhite, 0.2f, 18, true);
                MagnumParticleHandler.SpawnParticle(star);
            }
            
            // Cosmic glow
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.09f) * 0.18f + 0.82f;
            Lighting.AddLight(player.Center, FateCosmicVFX.FatePurple.ToVector3() * pulse * 0.45f);
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Track for star circle effect
            player.GetModPlayer<FateWeaponEffectPlayer>()?.OnFateWeaponAttack(player.Center);
            
            // Spawn 3 spectral Coda swords at different orbit positions
            for (int i = 0; i < 3; i++)
            {
                // Stagger spawn positions around player
                float spawnAngle = MathHelper.TwoPi * i / 3f;
                Vector2 spawnOffset = spawnAngle.ToRotationVector2() * 60f;
                Vector2 spawnPos = player.Center + spawnOffset;
                
                // ai[0] = phase (starts at 0 = Orbiting)
                // ai[1] = orbit index (0, 1, or 2)
                Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback, player.whoAmI, 0, i);
                
                // Spawn VFX per sword
                CustomParticles.GenericFlare(spawnPos, FateCosmicVFX.FateWhite, 0.5f, 15);
                FateCosmicVFX.SpawnGlyphBurst(spawnPos, 3, 4f, 0.3f);
            }
            
            // Central summoning VFX
            FateCosmicVFX.SpawnCosmicCloudBurst(player.Center, 0.6f, 12);
            FateCosmicVFX.SpawnCosmicMusicNotes(player.Center, 5, 40f, 0.35f);
            
            // Halo rings
            for (int i = 0; i < 4; i++)
            {
                Color haloColor = FateCosmicVFX.GetCosmicGradient((float)i / 4f);
                CustomParticles.HaloRing(player.Center, haloColor, 0.4f + i * 0.08f, 18 + i * 2);
            }
            
            // Dramatic sound
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 0.7f }, player.Center);
            
            return false;
        }
    }
}
