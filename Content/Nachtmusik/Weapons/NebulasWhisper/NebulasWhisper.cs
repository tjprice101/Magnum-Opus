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
using MagnumOpus.Content.Nachtmusik.Weapons.NebulasWhisper.Projectiles;
using MagnumOpus.Content.Nachtmusik.Weapons.NebulasWhisper.Utilities;

namespace MagnumOpus.Content.Nachtmusik.Weapons.NebulasWhisper
{
    /// <summary>
    /// Nebula's Whisper — Cosmic cannon that fires expanding nebula shots.
    /// Shots phase through first 3 tiles of walls, expand as they travel, leave residue.
    /// Alt fire (after 5 shots): Whisper Storm — converges all residue on cursor position.
    /// "The nebula does not shout. It barely breathes. But entire stars are born in its exhale."
    /// </summary>
    public class NebulasWhisper : ModItem
    {
        private int whisperShotCount = 0;

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 58;
            Item.damage = 1050;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3.5f;
            Item.value = Item.sellPrice(gold: 42);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.UseSound = SoundID.Item38 with { Pitch = 0.1f, Volume = 0.85f };
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<NebulaWhisperShot>();
            Item.shootSpeed = 20f;
            Item.useAmmo = AmmoID.Bullet;
            Item.crit = 18;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Require 5+ shots for Whisper Storm
                if (whisperShotCount < 5) return false;
                Item.useTime = 30;
                Item.useAnimation = 30;
            }
            else
            {
                Item.useTime = 16;
                Item.useAnimation = 16;
            }
            return base.CanUseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 direction = velocity.SafeNormalize(Vector2.UnitX);

            if (player.altFunctionUse == 2 && whisperShotCount >= 5)
            {
                // === WHISPER STORM — converge residue on cursor ===
                Vector2 cursorPos = Main.MouseWorld;
                int stormDamage = (int)(damage * 2.5f);

                // Spawn converging storm projectile at cursor
                Projectile.NewProjectile(source, cursorPos, Vector2.Zero,
                    ModContent.ProjectileType<NebulaWhisperShot>(),
                    stormDamage, knockback * 2f, player.whoAmI, ai0: 1f); // ai0 = 1 marks storm mode

                NebulasWhisperVFX.WhisperStormVFX(cursorPos);
                SoundEngine.PlaySound(SoundID.Item74 with { Pitch = -0.3f, Volume = 0.9f }, cursorPos);

                whisperShotCount = 0;
                return false;
            }

            // === NORMAL FIRE — expanding nebula shot ===
            whisperShotCount++;

            Projectile.NewProjectile(source, position, velocity,
                ModContent.ProjectileType<NebulaWhisperShot>(), damage, knockback, player.whoAmI);

            // Muzzle flash: soft nebula puff
            NebulasWhisperVFX.MuzzleFlashVFX(position + direction * 15f, direction);

            return false;
        }

        public override void HoldItem(Player player)
        {
            NebulasWhisperVFX.HoldItemVFX(player);
        }

        public override Vector2? HoldoutOffset() => new Vector2(-3f, 0f);

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D tex = TextureAssets.Item[Item.type].Value;
            Vector2 pos = Item.Center - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;

            float time = Main.GameUpdateCount * 0.04f;
            float breathe = 1f + (float)Math.Sin(time * 1.3f) * 0.12f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Cosmic purple nebula haze — wide, soft outer
            spriteBatch.Draw(tex, pos, null, NachtmusikPalette.CosmicPurple with { A = 0 } * 0.3f,
                rotation, origin, scale * breathe * 1.45f, SpriteEffects.None, 0f);

            // Violet mid-layer — the whisper's chromatic heart
            float nebulaShift = (float)Math.Sin(time * 1.8f) * 0.15f + 0.85f;
            spriteBatch.Draw(tex, pos, null, NachtmusikPalette.Violet with { A = 0 } * 0.28f * nebulaShift,
                rotation, origin, scale * breathe * 1.25f, SpriteEffects.None, 0f);

            // Star white shimmer core
            spriteBatch.Draw(tex, pos, null, NachtmusikPalette.StarWhite with { A = 0 } * 0.22f,
                rotation, origin, scale * breathe * 1.1f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, NachtmusikPalette.CosmicPurple.ToVector3() * 0.35f);
            return true;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D tex = TextureAssets.Item[Item.type].Value;
            float time = Main.GameUpdateCount * 0.035f;
            float breathe = 1f + (float)Math.Sin(time * 1.5f) * 0.08f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            float cycle = (float)Math.Sin(time * 0.6f) * 0.5f + 0.5f;
            Color glowColor = Color.Lerp(NachtmusikPalette.CosmicPurple, NachtmusikPalette.Violet, cycle) * 0.26f;
            spriteBatch.Draw(tex, position, frame, glowColor, 0f, origin, scale * breathe * 1.12f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            spriteBatch.Draw(tex, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Expand", "Fires nebula shots that expand as they travel"));
            tooltips.Add(new TooltipLine(Mod, "Phase", "Shots phase through the first 3 tiles of walls"));
            tooltips.Add(new TooltipLine(Mod, "Residue", "Leaves lingering nebula residue that damages enemies"));
            tooltips.Add(new TooltipLine(Mod, "Storm", "Right click after 5 shots: Whisper Storm — converges all residue on cursor"));
            tooltips.Add(new TooltipLine(Mod, "Debuff", "Inflicts Celestial Harmony"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The nebula does not shout. It barely breathes. But entire stars are born in its exhale.'")
            {
                OverrideColor = new Color(100, 120, 200)
            });
        }
    }
}
