using MagnumOpus.Common;
using MagnumOpus.Common.Systems.UI;
using MagnumOpus.Content.ClairDeLune;
using MagnumOpus.Content.ClairDeLune.Weapons.ClockworkHarmony.Projectiles;
using MagnumOpus.Content.ClairDeLune.Weapons.ClockworkHarmony.Utilities;
using MagnumOpus.Content.SandboxExoblade.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.ClockworkHarmony
{
    /// <summary>
    /// Clockwork Harmony — Clair de Lune's gear-driven melee weapon.
    /// Exoblade-architecture weapon with moonlit clockwork VFX.
    /// </summary>
    public class ClockworkHarmony : ModItem, IOverdriveItem
    {
        public IResonantOverdrive GetOverdrivePlayer(Player player) => player.GetModPlayer<ClockworkHarmonyPlayer>();

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 70;
            Item.height = 70;
            Item.scale = 0.09f;
            Item.damage = 3400;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useTurn = true;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.knockBack = 9f;
            Item.autoReuse = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.channel = true;
            Item.value = Item.sellPrice(platinum: 5);
            Item.shoot = ModContent.ProjectileType<ClockworkHarmonySwing>();
            Item.shootSpeed = 8f;
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.crit = 18;
        }

        public override bool CanShoot(Player player)
        {
            bool isDash = player.altFunctionUse == 2;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active || p.owner != player.whoAmI || p.type != Item.shoot)
                    continue;
                if (isDash) return false;
                if (!(p.ai[0] == 1 && p.ai[1] == 1)) return false;
            }
            return true;
        }

        public override void HoldItem(Player player)
        {
            player.ExoBlade().rightClickListener = true;
            player.ExoBlade().mouseWorldListener = true;

            if (Main.rand.NextBool(4))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                Color col = ClairDeLunePalette.GetClockworkGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.WhiteTorch,
                    new Vector2(0, -0.8f) + Main.rand.NextVector2Circular(0.4f, 0.4f), 0, col, 0.5f);
                d.noGravity = true;
            }

            float pulse = 0.7f + 0.3f * MathF.Sin(Main.GlobalTimeWrappedHourly * 3f);
            Lighting.AddLight(player.Center, ClairDeLunePalette.SoftBlue.ToVector3() * 0.35f * pulse);
            player.GetModPlayer<ClockworkHarmonyPlayer>().IsHoldingClockworkHarmony = true;
        }

        public override bool AltFunctionUse(Player player) => true;
        public override bool? CanHitNPC(Player player, NPC target) => false;
        public override bool CanHitPvp(Player player, Player target) => false;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                var cp = player.GetModPlayer<ClockworkHarmonyPlayer>();
                if (cp.IsChargeFull)
                {
                    cp.ConsumeCharge();
                    SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -0.4f }, player.Center);
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi / 8f * i;
                        Vector2 bombVel = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 6f;
                        Projectile.NewProjectile(source, player.Center, bombVel,
                            ModContent.ProjectileType<ClockworkHarmonySpecialProj>(),
                            damage * 2, knockback, player.whoAmI);
                    }
                }
                else
                    SoundEngine.PlaySound(SoundID.Item16 with { Pitch = 0.5f, Volume = 0.5f }, player.Center);
                return false;
            }

            Projectile.NewProjectile(source, player.MountedCenter,
                (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX),
                type, damage, knockback, player.whoAmI, 0f, 0);
            return false;
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.2f;
            Texture2D tex = Terraria.GameContent.TextureAssets.Item[Type].Value;
            Vector2 drawPos = Item.position - Main.screenPosition + new Vector2(Item.width / 2f, Item.height);
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height);

            spriteBatch.Draw(tex, drawPos, null, ClairDeLunePalette.SoftBlue with { A = 0 } * pulse,
                rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, ClairDeLunePalette.PearlWhite with { A = 0 } * (pulse * 0.7f),
                rotation, origin, scale * 1.02f, SpriteEffects.None, 0f);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Clockwork Resonance — interlocking gear-strikes that build harmonic momentum"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Each swing leaves meshing gear echoes that deal sustained AoE damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Right-click to dash through enemies with a drive gear burst"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Harmony isn't found. It's engineered.'")
            {
                OverrideColor = ClairDeLunePalette.LoreText
            });
        }
    }
}
