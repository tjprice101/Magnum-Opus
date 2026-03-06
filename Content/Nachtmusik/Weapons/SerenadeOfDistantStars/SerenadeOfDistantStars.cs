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
using MagnumOpus.Content.Nachtmusik.Weapons.SerenadeOfDistantStars.Projectiles;
using MagnumOpus.Content.Nachtmusik.Weapons.SerenadeOfDistantStars.Utilities;

namespace MagnumOpus.Content.Nachtmusik.Weapons.SerenadeOfDistantStars
{
    /// <summary>
    /// Serenade of Distant Stars — Romantic homing star rifle.
    /// Fires homing star projectiles with 80-tile range and moderate homing.
    /// Serenade Rhythm: firing at consistent intervals builds stacks (max 5).
    /// +10% homing per stack. At 5 stacks: perfect homing.
    /// Star Memory: stars remember enemies passed within 5 tiles and fire echoes.
    /// "The light left a star ages ago, just to find you. And it never missed."
    /// </summary>
    public class SerenadeOfDistantStars : ModItem
    {
        private int rhythmStacks = 0;
        private int lastFireTime = 0;
        private const int RhythmWindowMin = 50;
        private const int RhythmWindowMax = 70;
        private const int MaxRhythmStacks = 5;

        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 70;
            Item.damage = 950;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 48);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.UseSound = SoundID.Item91 with { Pitch = 0.2f, Volume = 0.8f };
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<SerenadeStarProjectile>();
            Item.shootSpeed = 16f;
            Item.useAmmo = AmmoID.Bullet;
            Item.crit = 20;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int currentTime = (int)Main.GameUpdateCount;
            int timeSinceLast = currentTime - lastFireTime;

            // === SERENADE RHYTHM SYSTEM ===
            if (timeSinceLast >= RhythmWindowMin && timeSinceLast <= RhythmWindowMax)
            {
                // In rhythm — build stacks
                rhythmStacks = Math.Min(rhythmStacks + 1, MaxRhythmStacks);

                if (rhythmStacks >= MaxRhythmStacks)
                {
                    SoundEngine.PlaySound(SoundID.Item25 with { Pitch = 0.6f, Volume = 0.6f }, position);
                }
            }
            else if (lastFireTime > 0)
            {
                // Out of rhythm — reset
                rhythmStacks = 0;
            }

            lastFireTime = currentTime;

            // Fire homing star with rhythm stack info
            Vector2 direction = velocity.SafeNormalize(Vector2.UnitX);
            Projectile.NewProjectile(source, position, velocity,
                ModContent.ProjectileType<SerenadeStarProjectile>(),
                damage, knockback, player.whoAmI, ai0: rhythmStacks);

            // Muzzle flash
            SerenadeOfDistantStarsVFX.MuzzleFlashVFX(position + direction * 25f, direction);

            return false;
        }

        public override void HoldItem(Player player)
        {
            SerenadeOfDistantStarsVFX.HoldItemVFX(player, rhythmStacks);
        }

        public override Vector2? HoldoutOffset() => new Vector2(-6f, 0f);

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D tex = TextureAssets.Item[Item.type].Value;
            Vector2 pos = Item.Center - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;

            float time = Main.GameUpdateCount * 0.05f;
            float twinkle = 1f + (float)Math.Sin(time * 2.5f) * 0.06f
                + (float)Math.Sin(time * 4.1f) * 0.03f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Deep blue stellar foundation
            spriteBatch.Draw(tex, pos, null, NachtmusikPalette.DeepBlue with { A = 0 } * 0.3f,
                rotation, origin, scale * twinkle * 1.35f, SpriteEffects.None, 0f);

            // Warm star gold melody glow
            float warmPulse = (float)Math.Sin(time * 1.7f) * 0.5f + 0.5f;
            spriteBatch.Draw(tex, pos, null, NachtmusikPalette.StarGold with { A = 0 } * 0.3f * warmPulse,
                rotation, origin, scale * twinkle * 1.2f, SpriteEffects.None, 0f);

            // Moonlit silver highlight core
            spriteBatch.Draw(tex, pos, null, NachtmusikPalette.MoonlitSilver with { A = 0 } * 0.2f,
                rotation, origin, scale * twinkle * 1.08f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, NachtmusikPalette.StarGold.ToVector3() * 0.35f);
            return true;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D tex = TextureAssets.Item[Item.type].Value;
            float time = Main.GameUpdateCount * 0.04f;
            float twinkle = 1f + (float)Math.Sin(time * 2.3f) * 0.07f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            float cycle = (float)Math.Sin(time * 0.7f) * 0.5f + 0.5f;
            Color glowColor = Color.Lerp(NachtmusikPalette.StarGold, NachtmusikPalette.MoonlitSilver, cycle) * 0.24f;
            spriteBatch.Draw(tex, position, frame, glowColor, 0f, origin, scale * twinkle * 1.1f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            spriteBatch.Draw(tex, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Homing", "Fires a homing star projectile with 80-tile range"));
            tooltips.Add(new TooltipLine(Mod, "Rhythm", "Serenade Rhythm: fire at consistent intervals to build stacks (max 5)"));
            tooltips.Add(new TooltipLine(Mod, "RhythmBonus", "+10% homing strength per stack — at 5 stacks: perfect homing"));
            tooltips.Add(new TooltipLine(Mod, "Memory", "Star Memory: stars remember enemies passed within 5 tiles and fire echoes back"));
            tooltips.Add(new TooltipLine(Mod, "Debuff", "Inflicts Celestial Harmony"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The light left a star ages ago, just to find you. And it never missed.'")
            {
                OverrideColor = new Color(100, 120, 200)
            });
        }
    }
}
