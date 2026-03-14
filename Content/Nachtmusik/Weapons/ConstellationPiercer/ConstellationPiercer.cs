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
using MagnumOpus.Content.Nachtmusik.Weapons.ConstellationPiercer.Systems;

namespace MagnumOpus.Content.Nachtmusik.Weapons.ConstellationPiercer
{
    /// <summary>
    /// Constellation Piercer -- Precision celestial rifle that fires triple converging constellation bolts.
    /// Bolts pierce and mark enemies as Star Points with Celestial Harmony.
    /// 3+ Star Points connect with chain resonance arcs dealing passive damage.
    /// 5+ Star Points trigger Stellar Conduit: flowing starlight rivers for 3 seconds.
    /// Every 5th shot: Starfall -- light pillar strikes a Star Point, refreshing it.
    /// "Each star is an enemy. Each line of light between them is a death sentence."
    /// </summary>
    public class ConstellationPiercer : ModItem
    {
        private int starfallCounter = 0;

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 66;
            Item.damage = 1250;
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
            float centerAngle = velocity.ToRotation();

            starfallCounter++;

            // Center bolt at full damage (ai[0] = 0 means no convergence needed)
            Projectile.NewProjectile(source, position, velocity,
                ModContent.ProjectileType<ConstellationBoltProjectile>(), damage, knockback, player.whoAmI);

            // Two side bolts at +/-8 degrees with 0.7x damage; ai[0] = center angle for convergence
            for (int i = -1; i <= 1; i += 2)
            {
                float angleOffset = MathHelper.ToRadians(8f * i);
                Vector2 sideVel = velocity.RotatedBy(angleOffset);
                Projectile.NewProjectile(source, position, sideVel,
                    ModContent.ProjectileType<ConstellationBoltProjectile>(),
                    (int)(damage * 0.7f), knockback * 0.5f, player.whoAmI, ai0: centerAngle);
            }

            // Starfall every 5th shot: light pillar strikes a random Star Point
            if (starfallCounter >= 5)
            {
                starfallCounter = 0;

                int targetNpc = StarPointSystem.GetRandomStarPointNPC();
                if (targetNpc >= 0 && Main.npc[targetNpc].active)
                {
                    // Refresh the Star Point and trigger starfall VFX
                    StarPointSystem.RefreshStarPoint(targetNpc);
                    ConstellationPiercerVFX.StarfallVFX(Main.npc[targetNpc].Center);

                    // Deal AoE damage around the starfall point
                    Vector2 starfallPos = Main.npc[targetNpc].Center;
                    for (int n = 0; n < Main.maxNPCs; n++)
                    {
                        NPC npc = Main.npc[n];
                        if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                        if (Vector2.Distance(npc.Center, starfallPos) <= 80f)
                        {
                            player.ApplyDamageToNPC(npc, (int)(damage * 0.4f), 0f, 0, false);
                        }
                    }

                    SoundEngine.PlaySound(SoundID.Item25 with { Pitch = 0.6f, Volume = 0.7f }, starfallPos);
                }
                else
                {
                    // No Star Points exist -- spawn seeking crystals as fallback
                    SeekingCrystalHelper.SpawnNachtmusikCrystals(
                        source, position + direction * 30f, velocity * 0.8f,
                        (int)(damage * 0.5f), knockback, player.whoAmI, 4);
                    SoundEngine.PlaySound(SoundID.Item25 with { Pitch = 0.4f, Volume = 0.7f }, position);
                }
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
            tooltips.Add(new TooltipLine(Mod, "Triple", "Fires three converging constellation bolts per shot"));
            tooltips.Add(new TooltipLine(Mod, "StarPoint", "Bolts pierce and mark enemies as Star Points"));
            tooltips.Add(new TooltipLine(Mod, "ChainResonance", "3+ Star Points connect with chain resonance arcs that deal passive damage"));
            tooltips.Add(new TooltipLine(Mod, "Conduit", "5+ Star Points trigger Stellar Conduit: flowing starlight rivers for 3 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Starfall", "Every 5th shot: Starfall strikes a Star Point, refreshing it with AoE damage"));
            tooltips.Add(new TooltipLine(Mod, "Debuff", "Inflicts Celestial Harmony on all marked targets"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Each star is an enemy. Each line of light between them is a death sentence.'")
            {
                OverrideColor = NachtmusikPalette.LoreText
            });
        }
    }
}
