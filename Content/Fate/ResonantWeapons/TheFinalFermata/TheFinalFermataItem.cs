using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.Fate.Debuffs;
using MagnumOpus.Content.Fate.ResonantWeapons.TheFinalFermata.Utilities;
using MagnumOpus.Content.Fate.ResonantWeapons.TheFinalFermata.Particles;
using MagnumOpus.Content.Fate.ResonantWeapons.TheFinalFermata.Projectiles;

namespace MagnumOpus.Content.Fate.ResonantWeapons.TheFinalFermata
{
    /// <summary>
    /// The Final Fermata — Fate endgame magic staff.
    /// Summons 3 spectral swords that orbit in triangle formation (or hexagonal at 6).
    /// Every 90 frames all swords slash toward the nearest enemy.
    /// At max 6 swords, damage is multiplied by 1.5x.
    ///
    /// Stats: 520 damage, Magic, useTime/Animation 45, mana 30, KB 4, shootSpeed 1, 55g sell.
    /// </summary>
    public class TheFinalFermataItem : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/TheFinalFermata";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

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
            Item.shoot = ModContent.ProjectileType<FermataSpectralSwordNew>();
            Item.shootSpeed = 1f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
                "Summons 3 spectral swords that orbit in triangular formation (max 6, hexagonal)"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "Every 90 frames, all swords perform a synchronized slash toward the nearest enemy"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
                "Fermata Power: sustained hold grants +10% damage per second, up to 5x at 5 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Effect4",
                "Harmonic Alignment: 3+ swords focus the same target with convergent crossfire"));
            tooltips.Add(new TooltipLine(Mod, "Effect5",
                "Hold for 10 seconds to manifest a Sustained Note \u2014 an autonomous minion"));
            tooltips.Add(new TooltipLine(Mod, "Effect6",
                "At 6 swords, synchronized slash damage is multiplied by 1.5x"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'Hold the note. Hold it until the stars remember what silence is.'")
            {
                OverrideColor = new Color(180, 40, 80) // Fate cosmic crimson
            });
        }

        public override bool CanUseItem(Player player)
        {
            // Limit to max 6 concurrent spectral swords
            return player.ownedProjectileCounts[
                ModContent.ProjectileType<FermataSpectralSwordNew>()] < 6;
        }

        public override void HoldItem(Player player)
        {
            if (Main.dedServ) return;

            float time = (float)Main.timeForVisualEffects;
            Vector2 center = player.MountedCenter;

            // Faint ambient glow pulse
            float pulse = 0.2f + FermataUtils.SinePulse(time, 0.04f) * 0.1f;
            Lighting.AddLight(center,
                FermataUtils.FermataPurple.ToVector3() * pulse);

            // Occasional mote near player
            if (Main.rand.NextBool(10))
            {
                FermataParticleHandler.EnsureInitialized();
                FermataParticleTypes.SpawnMote(
                    center + Main.rand.NextVector2Circular(40f, 40f),
                    FermataUtils.PaletteLerp(Main.rand.NextFloat(0.1f, 0.4f)) * 0.4f,
                    0.12f, 20);
            }

            // Occasional time shard
            if (Main.rand.NextBool(14))
            {
                FermataParticleHandler.EnsureInitialized();
                FermataParticleTypes.SpawnTimeShard(
                    center + Main.rand.NextVector2Circular(50f, 50f),
                    null,
                    FermataUtils.GhostSilver * 0.3f, 0.1f, 25);
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var fermata = player.Fermata();

            // Determine orbit offsets: first cast = 0°/120°/240°, second cast = 60°/180°/300°
            int existingSwords = player.ownedProjectileCounts[
                ModContent.ProjectileType<FermataSpectralSwordNew>()];
            float baseOffset = existingSwords >= 3 ? MathHelper.TwoPi / 6f : 0f;

            for (int i = 0; i < 3; i++)
            {
                float orbitAngle = baseOffset + MathHelper.TwoPi * i / 3f;
                Vector2 spawnOffset = FermataUtils.AngleToVector(orbitAngle) * OrbitSpawnRadius;
                Vector2 spawnPos = player.Center + spawnOffset;

                // ai[0] = orbit offset angle
                Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type,
                    damage, knockback, player.whoAmI, orbitAngle, 0f);
            }

            fermata.TotalCasts++;

            // === CAST VFX: temporal distortion wave ===
            if (!Main.dedServ)
            {
                FermataParticleHandler.EnsureInitialized();
                FermataParticleTypes.TemporalDistortionBurst(player.Center, 1f);
            }

            // Cast sound
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 0.7f }, player.Center);

            return false; // We handle projectile spawning manually
        }

        private const float OrbitSpawnRadius = 60f;

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor,
            Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            // World drop glow
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 drawPos = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.03f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, drawPos, null,
                FermataUtils.FermataPurple * 0.2f, rotation, origin,
                scale * 1.2f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null,
                FermataUtils.FermataCrimson * 0.15f, rotation, origin,
                scale * 1.1f * pulse, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            return true;
        }
    }
}
