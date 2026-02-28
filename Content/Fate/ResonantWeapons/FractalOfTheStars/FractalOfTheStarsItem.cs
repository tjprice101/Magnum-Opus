using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.Fate.ResonantWeapons.FractalOfTheStars.Utilities;
using MagnumOpus.Content.Fate.ResonantWeapons.FractalOfTheStars.Particles;
using MagnumOpus.Content.Fate.ResonantWeapons.FractalOfTheStars.Projectiles;

namespace MagnumOpus.Content.Fate.ResonantWeapons.FractalOfTheStars
{
    /// <summary>
    /// Fractal of the Stars — A blade forged from shattered constellations.
    ///
    /// SELF-CONTAINED WEAPON SYSTEM (no shared VFX libraries):
    ///   - Own particle system (FractalParticleHandler)
    ///   - Own GPU trail renderer (FractalTrailRenderer)
    ///   - Own shader pipeline (FractalShaderLoader → 4 .fx files)
    ///   - Own ModPlayer state (FractalPlayer via player.Fractal())
    ///   - Own projectiles (FractalSwingProjectile, FractalOrbitBlade)
    ///
    /// ATTACK PATTERN:
    ///   TRUE MELEE — fires FractalSwingProjectile as a held swing.
    ///   3-phase combo: Horizontal Sweep → Rising Uppercut → Gravity Slam
    ///   On hit: spawns orbiting spectral star blades (max 6)
    ///   Every 3rd hit (Gravity Slam): Star Fracture — geometric fractal explosion
    ///   Orbit blades periodically fire prismatic beams
    /// </summary>
    public class FractalOfTheStarsItem : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/FractalOfTheStars";

        private static Asset<Texture2D> _glowTex;

        public override void SetDefaults()
        {
            // === PRESERVED STATS ===
            Item.damage = 850;
            Item.DamageType = DamageClass.Melee;
            Item.width = 58;
            Item.height = 58;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.knockBack = 7f;
            Item.value = Item.sellPrice(gold: 58);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.autoReuse = true;

            // === HELD PROJECTILE SWING (TRUE MELEE) ===
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<FractalSwingProjectile>();
            Item.shootSpeed = 1f;
            Item.channel = false;
            Item.UseSound = null; // Swing projectile handles sounds
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Swings in a 3-phase combo: sweep, uppercut, and gravity slam"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "On hit, spawns orbiting spectral star blades that fire prismatic beams"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Every 3rd hit triggers a Star Fracture — a massive fractal explosion"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The stars shattered, and from their fractures, destiny was rewritten'")
            {
                OverrideColor = new Color(180, 40, 80) // Cosmic Crimson (Fate theme)
            });
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Fire the held swing projectile
            Projectile.NewProjectile(source, player.Center, Vector2.Zero, type, damage, knockback, player.whoAmI);

            return false; // We already spawned the projectile
        }

        public override void HoldItem(Player player)
        {
            if (Main.dedServ) return;

            float pulse = 0.6f + MathF.Sin((float)Main.timeForVisualEffects * 0.04f) * 0.15f;
            Lighting.AddLight(player.Center, FractalUtils.FractalPurple.ToVector3() * 0.3f * pulse);

            // Ambient stellar motes
            if (Main.rand.NextBool(10))
            {
                Vector2 motePos = player.Center + Main.rand.NextVector2Circular(25f, 25f);
                Color moteCol = FractalUtils.GetStarShimmer((float)Main.timeForVisualEffects * 0.03f + Main.rand.NextFloat());
                FractalParticleHandler.SpawnParticle(new FractalMote(
                    motePos, new Vector2(0, -0.3f), moteCol, 0.1f, 25));
            }

            // Occasional tiny star particle
            if (Main.rand.NextBool(20))
            {
                Vector2 starPos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                Vector2 starVel = Main.rand.NextVector2Circular(0.5f, 0.5f);
                FractalParticleHandler.SpawnParticle(new FractalStarParticle(
                    starPos, starVel, FractalUtils.ConstellationWhite, 0.08f, 30, 4));
            }
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            if (Main.dedServ) return;

            _glowTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow");
            if (_glowTex?.Value == null) return;

            Texture2D itemTex = Terraria.GameContent.TextureAssets.Item[Item.type].Value;
            Vector2 origin = itemTex.Size() / 2f;
            Vector2 drawPos = Item.Center - Main.screenPosition;
            float pulse = 0.85f + MathF.Sin((float)Main.timeForVisualEffects * 0.05f) * 0.15f;

            try
            {
                FractalUtils.BeginAdditive(spriteBatch);
                FractalUtils.DrawItemBloom(spriteBatch, itemTex, drawPos, origin, rotation, scale, pulse);
                FractalUtils.EndAdditive(spriteBatch);
            }
            catch
            {
                try { FractalUtils.EndAdditive(spriteBatch); } catch { }
            }
        }
    }
}
