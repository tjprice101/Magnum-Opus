using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;
using MagnumOpus.Content.OdeToJoy.HarmonicCores;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Content.OdeToJoy.Weapons.RoseThornChainsaw.Projectiles;
using MagnumOpus.Content.OdeToJoy.Weapons.RoseThornChainsaw.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.RoseThornChainsaw
{
    /// <summary>
    /// Rose Thorn Chainsaw — Drill-style holdout melee weapon of Ode to Joy.
    /// Rips through enemies with thorned blades, spawning thorn chain projectiles
    /// and layered particle effects. Applies Poisoned + Venom on contact.
    /// </summary>
    public class RoseThornChainsaw : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
            ItemID.Sets.IsDrill[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 4400;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.width = 54;
            Item.height = 24;

            Item.useTime = 1;
            Item.useAnimation = 1;

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.channel = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;

            Item.knockBack = 1f;
            Item.crit = 10;
            Item.autoReuse = true;

            Item.shoot = ModContent.ProjectileType<ChainsawHoldoutProjectile>();
            Item.shootSpeed = 32f;

            Item.UseSound = SoundID.Item22;
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.OdeToJoyRarity>();
            Item.value = Item.sellPrice(platinum: 4);
        }

        public override bool CanUseItem(Player player)
        {
            // Prevent stacking holdout projectiles
            return player.ownedProjectileCounts[ModContent.ProjectileType<ChainsawHoldoutProjectile>()] <= 0;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(
                source,
                player.MountedCenter,
                velocity,
                ModContent.ProjectileType<ChainsawHoldoutProjectile>(),
                damage,
                knockback,
                player.whoAmI);

            return false;
        }

        // ── WORLD DROP RENDERING ──

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor,
            ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            // Chainsaw buzz: high-frequency micro-jitter with rapid pink↔green color oscillation
            float time = Main.GameUpdateCount * 0.06f;
            float buzz = (float)Math.Sin(time * 12f) * 0.03f; // Very fast, tiny amplitude vibration
            float buzzPulse = 1f + buzz;
            float motorHum = (float)Math.Sin(time * 8f); // Mechanical hum
            float colorFlash = (float)Math.Sin(time * 6f) * 0.5f + 0.5f; // Rapid green↔pink

            // Micro-jitter offset simulating buzzing blade
            Vector2 jitter = new Vector2(
                (float)Math.Sin(time * 15f) * 1.2f,
                (float)Math.Cos(time * 13f) * 0.8f);

            RoseThornChainsawUtils.BeginAdditive(spriteBatch);

            // Venomous outer buzz — green-dominant, jittering
            Color outerColor = Color.Lerp(RoseThornChainsawUtils.VerdantGreen, RoseThornChainsawUtils.VenomViolet, colorFlash * 0.4f);
            spriteBatch.Draw(texture, position + jitter, null,
                RoseThornChainsawUtils.Additive(outerColor, 0.35f + motorHum * 0.05f),
                rotation, origin, scale * buzzPulse * 1.25f, SpriteEffects.None, 0f);

            // Rose-pink inner oscillation — counter-phase to outer
            Color innerColor = Color.Lerp(RoseThornChainsawUtils.RosePink, RoseThornChainsawUtils.GoldenPollen, 1f - colorFlash);
            spriteBatch.Draw(texture, position - jitter * 0.5f, null,
                RoseThornChainsawUtils.Additive(innerColor, 0.28f),
                rotation, origin, scale * (1f - buzz) * 1.12f, SpriteEffects.None, 0f);

            // White sparking core — staccato mechanical flash
            float sparkFlash = (float)Math.Pow(Math.Max(0f, (float)Math.Sin(time * 9f)), 6f); // Sharp narrow peaks
            spriteBatch.Draw(texture, position, null,
                RoseThornChainsawUtils.Additive(RoseThornChainsawUtils.WhiteBloom, 0.4f * sparkFlash),
                rotation, origin, scale * (1f + sparkFlash * 0.08f), SpriteEffects.None, 0f);

            RoseThornChainsawUtils.BeginDefault(spriteBatch);

            Lighting.AddLight(Item.Center, 0.4f + sparkFlash * 0.2f, 0.6f + motorHum * 0.1f, 0.25f);
            return true;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame,
            Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            float time = Main.GameUpdateCount * 0.05f;
            float pulse = 1f + (float)Math.Sin(time * 2.2f) * 0.08f;
            float flicker = Main.rand.NextFloat(0.9f, 1f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            Color glowColor = Color.Lerp(RoseThornChainsawUtils.VerdantGreen, RoseThornChainsawUtils.GoldenPollen,
                (float)Math.Sin(time * 0.8f) * 0.5f + 0.5f);
            spriteBatch.Draw(texture, position, frame,
                RoseThornChainsawUtils.Additive(glowColor, 0.3f * flicker),
                0f, origin, scale * pulse * 1.12f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            spriteBatch.Draw(texture, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            return false;
        }

        // ── TOOLTIPS ──

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Rapidly shreds enemies with a whirling storm of enchanted thorns"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Periodically launches thorn chains that ricochet off terrain"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Contact inflicts venomous bloom — Poisoned and Venom stack on hit"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Critical strikes spawn bonus thorn shrapnel"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'From the garden's deepest tangle, where joy takes root in thorns, the song of reckless spring roars forth'")
            {
                OverrideColor = new Color(255, 200, 50)
            });
        }

        // ── RECIPE ──

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfOdeToJoy>(), 25)
                .AddIngredient(ModContent.ItemType<OdeToJoyResonantEnergy>(), 20)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfOdeToJoy>(), 3)
                .AddIngredient(ItemID.LunarBar, 20)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }
}
