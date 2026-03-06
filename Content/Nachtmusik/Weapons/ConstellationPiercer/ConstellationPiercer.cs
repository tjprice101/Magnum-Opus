using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Debuffs;
using MagnumOpus.Content.Nachtmusik.Weapons.ConstellationPiercer.Projectiles;
using MagnumOpus.Content.Nachtmusik.Weapons.ConstellationPiercer.Utilities;

namespace MagnumOpus.Content.Nachtmusik.Weapons.ConstellationPiercer
{
    /// <summary>
    /// Constellation Piercer — Precision celestial rifle that fires triple constellation bolts.
    /// Bolts pierce and chain between enemies, marking each as a Star Point.
    /// Every 5 shots spawns 4 seeking Nachtmusik crystals.
    /// Constellation Formation: 3+ Star Points auto-connect with luminous lines.
    /// "Each star is an enemy. Each line of light between them is a death sentence."
    /// </summary>
    public class ConstellationPiercer : ModItem
    {
        private int crystalCounter = 0;

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 66;
            Item.damage = 1150;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 12;
            Item.useAnimation = 12;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4.5f;
            Item.value = Item.sellPrice(gold: 45);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.UseSound = SoundID.Item41 with { Pitch = -0.2f, Volume = 0.9f };
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<ConstellationBoltProjectile>();
            Item.shootSpeed = 22f;
            Item.useAmmo = AmmoID.Bullet;
            Item.crit = 22;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 direction = velocity.SafeNormalize(Vector2.UnitX);

            crystalCounter++;

            // Center bolt at full damage
            Projectile.NewProjectile(source, position, velocity,
                ModContent.ProjectileType<ConstellationBoltProjectile>(), damage, knockback, player.whoAmI);

            // Two side bolts at ±8° with 0.7x damage
            for (int i = -1; i <= 1; i += 2)
            {
                float angleOffset = MathHelper.ToRadians(8f * i);
                Vector2 sideVel = velocity.RotatedBy(angleOffset);
                Projectile.NewProjectile(source, position, sideVel,
                    ModContent.ProjectileType<ConstellationBoltProjectile>(),
                    (int)(damage * 0.7f), knockback * 0.5f, player.whoAmI);
            }

            // Spawn 4 seeking crystals every 5 shots
            if (crystalCounter >= 5)
            {
                crystalCounter = 0;
                SeekingCrystalHelper.SpawnNachtmusikCrystals(
                    source,
                    position + direction * 30f,
                    velocity * 0.8f,
                    (int)(damage * 0.5f),
                    knockback,
                    player.whoAmI,
                    4
                );
                SoundEngine.PlaySound(SoundID.Item25 with { Pitch = 0.4f, Volume = 0.7f }, position);
            }

            // Muzzle flash VFX
            ConstellationPiercerVFX.MuzzleFlashVFX(position + direction * 25f, direction);

            return false;
        }

        public override void HoldItem(Player player)
        {
            ConstellationPiercerVFX.HoldItemVFX(player);
        }

        public override Vector2? HoldoutOffset() => new Vector2(-5f, 0f);

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D tex = TextureAssets.Item[Item.type].Value;
            Vector2 pos = Item.Center - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;

            float time = Main.GameUpdateCount * 0.06f;
            float pulse = 1f + (float)Math.Sin(time * 2.2f) * 0.08f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Constellation blue precision outer ring
            spriteBatch.Draw(tex, pos, null, NachtmusikPalette.ConstellationBlue with { A = 0 } * 0.35f,
                rotation, origin, scale * pulse * 1.3f, SpriteEffects.None, 0f);

            // Star gold crosshair shimmer
            float crosshairPulse = (float)Math.Sin(time * 3.5f) * 0.5f + 0.5f;
            spriteBatch.Draw(tex, pos, null, NachtmusikPalette.StarGold with { A = 0 } * 0.25f * crosshairPulse,
                rotation, origin, scale * pulse * 1.15f, SpriteEffects.None, 0f);

            // Star white precision core
            spriteBatch.Draw(tex, pos, null, NachtmusikPalette.StarWhite with { A = 0 } * 0.2f,
                rotation, origin, scale * pulse * 1.05f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, NachtmusikPalette.ConstellationBlue.ToVector3() * 0.4f);
            return true;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D tex = TextureAssets.Item[Item.type].Value;
            float time = Main.GameUpdateCount * 0.05f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.06f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            Color glowColor = NachtmusikPalette.GetStarfieldGradient((float)Math.Sin(time * 0.8f) * 0.5f + 0.5f) * 0.25f;
            spriteBatch.Draw(tex, position, frame, glowColor, 0f, origin, scale * pulse * 1.1f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            spriteBatch.Draw(tex, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Triple", "Fires three constellation bolts per shot"));
            tooltips.Add(new TooltipLine(Mod, "Chain", "Piercing bolts chain to up to 4 enemies, marking each as a Star Point"));
            tooltips.Add(new TooltipLine(Mod, "Formation", "3+ Star Points auto-connect with constellation lines"));
            tooltips.Add(new TooltipLine(Mod, "Crystals", "Every 5th shot spawns 4 seeking Nachtmusik crystals"));
            tooltips.Add(new TooltipLine(Mod, "Debuff", "Inflicts Celestial Harmony on all chained targets"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Each star is an enemy. Each line of light between them is a death sentence.'")
            {
                OverrideColor = new Color(100, 120, 200)
            });
        }
    }
}
