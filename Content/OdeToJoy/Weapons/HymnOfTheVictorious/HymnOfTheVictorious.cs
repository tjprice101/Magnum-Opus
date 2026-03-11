using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.Weapons.HymnOfTheVictorious.Dusts;
using MagnumOpus.Content.OdeToJoy.Weapons.HymnOfTheVictorious.Projectiles;
using MagnumOpus.Content.OdeToJoy.Weapons.HymnOfTheVictorious.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Content.OdeToJoy.HarmonicCores;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;

namespace MagnumOpus.Content.OdeToJoy.Weapons.HymnOfTheVictorious
{
    /// <summary>
    /// Hymn of the Victorious — 4-verse magic cycle weapon (MagicOrbFoundation).
    /// Each verse is unique: Exordium (piercing bolt), Rising (3-fan + burn),
    /// Apex (hover orb + AoE), Gloria (splitting bolt + 6 fragments).
    /// Complete Hymn fires all 4 simultaneously. Encore on kill.
    /// </summary>
    public class HymnOfTheVictorious : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.damage = 3100; // Tier 9 (2100-3200 range)
            Item.DamageType = DamageClass.Magic;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(platinum: 4);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.OdeToJoyRarity>();
            Item.UseSound = SoundID.Item43;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.crit = 15;
            Item.mana = 25;
            Item.shoot = ModContent.ProjectileType<ExordiumBoltProjectile>();
            Item.shootSpeed = 16f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            HymnPlayer hp = player.GetModPlayer<HymnPlayer>();
            int verse = hp.AdvanceVerse();
            Vector2 dir = velocity.SafeNormalize(Vector2.UnitX);
            float speed = velocity.Length();

            if (verse == -1)
            {
                // Complete Hymn — fire all 4 verse types simultaneously
                float spread = 0.08f;
                Projectile.NewProjectile(source, position, dir.RotatedBy(-spread * 1.5f) * speed, ModContent.ProjectileType<ExordiumBoltProjectile>(), (int)(damage * 1.3f), knockback, player.whoAmI);
                Projectile.NewProjectile(source, position, dir.RotatedBy(-spread * 0.5f) * speed, ModContent.ProjectileType<RisingBoltProjectile>(), damage, knockback, player.whoAmI);
                Projectile.NewProjectile(source, position, dir.RotatedBy(spread * 0.5f) * speed * 0.7f, ModContent.ProjectileType<ApexOrbProjectile>(), (int)(damage * 1.5f), knockback, player.whoAmI);
                Projectile.NewProjectile(source, position, dir.RotatedBy(spread * 1.5f) * speed, ModContent.ProjectileType<GloriaBoltProjectile>(), (int)(damage * 1.2f), knockback, player.whoAmI);

                // Complete Hymn VFX burst at player
                for (int i = 0; i < 25; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                    Color[] colors = { HymnTextures.BloomGold, HymnTextures.PetalPink, HymnTextures.JubilantLight, HymnTextures.RadiantAmber };
                    Dust d = Dust.NewDustDirect(player.Center, 1, 1, ModContent.DustType<HymnVerseDust>(), vel.X, vel.Y, 80, colors[i % 4], 1.0f);
                    d.noGravity = true;
                    d.fadeIn = 1.5f;
                }

                // Complete Hymn screen VFX
                OdeToJoyVFXLibrary.ScreenShake(10f, 20);
                OdeToJoyVFXLibrary.ScreenFlash(OdeToJoyPalette.GoldenPollen, 1.3f);
                OdeToJoyVFXLibrary.CelebrationBurst(player.Center, 1.5f);
                OdeToJoyVFXLibrary.HarmonicPulseRing(player.Center, 1.5f, 16, OdeToJoyPalette.GoldenPollen);
                return false;
            }

            // Individual verse shot  
            switch (verse)
            {
                case 0: // Exordium — single piercing bolt
                    Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<ExordiumBoltProjectile>(), damage, knockback, player.whoAmI);
                    break;
                case 1: // Rising — 3-way fan
                    for (int i = -1; i <= 1; i++)
                    {
                        Vector2 fanVel = dir.RotatedBy(i * 0.12f) * speed;
                        Projectile.NewProjectile(source, position, fanVel, ModContent.ProjectileType<RisingBoltProjectile>(), (int)(damage * 0.7f), knockback, player.whoAmI);
                    }
                    break;
                case 2: // Apex — large hover orb
                    Projectile.NewProjectile(source, position, velocity * 0.7f, ModContent.ProjectileType<ApexOrbProjectile>(), (int)(damage * 1.2f), knockback, player.whoAmI);
                    break;
                case 3: // Gloria — splitting bolt
                    Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<GloriaBoltProjectile>(), damage, knockback, player.whoAmI);
                    break;
            }

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Cycles through 4 unique verse types: Exordium, Rising, Apex, and Gloria"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Completing the full cycle fires the devastating Complete Hymn"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Enemies hit by 3+ verse types within 5s gain Hymn Resonance (+25% magic damage taken)"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Encore: killing with Complete Hymn locks Gloria for repeated devastating cycles"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Each verse is a victory. The final verse is annihilation.'")
            {
                OverrideColor = new Color(255, 200, 50)
            });
        }
    
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D tex = TextureAssets.Item[Item.type].Value;
            Vector2 pos = Item.Center - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;

            float time = Main.GameUpdateCount * 0.05f;
            float pulse = 1f + (float)Math.Sin(time * 2.2f) * 0.05f
                + (float)Math.Sin(time * 3.8f) * 0.03f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            OdeToJoyPalette.DrawItemBloom(spriteBatch, tex, pos, origin, rotation, scale, pulse);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, OdeToJoyPalette.GoldenPollen.ToVector3() * 0.35f);
            return true;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D tex = TextureAssets.Item[Item.type].Value;
            float time = Main.GameUpdateCount * 0.04f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.06f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            float cycle = (float)Math.Sin(time * 0.7f) * 0.5f + 0.5f;
            Color glowColor = Color.Lerp(OdeToJoyPalette.GoldenPollen, OdeToJoyPalette.RosePink, cycle) * 0.24f;
            spriteBatch.Draw(tex, position, frame, glowColor with { A = 0 }, 0f, origin, scale * pulse * 1.1f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            spriteBatch.Draw(tex, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}