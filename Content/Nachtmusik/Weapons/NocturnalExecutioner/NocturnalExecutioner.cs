using MagnumOpus.Common;
using MagnumOpus.Common.Systems.UI;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.SandboxExoblade.Utilities;
using MagnumOpus.Content.Nachtmusik.Weapons.NocturnalExecutioner.Projectiles;
using MagnumOpus.Content.Nachtmusik.Weapons.NocturnalExecutioner.Utilities;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Nachtmusik.Weapons.NocturnalExecutioner
{
    /// <summary>
    /// Nocturnal Executioner — Nachtmusik's void greatsword. Exoblade-architecture weapon item.
    /// The blade doesn't shine — it CONSUMES. Features a 4-phase combo with escalating
    /// nocturnal blade projectiles and devastating void burst finale.
    /// </summary>
    public class NocturnalExecutioner : ModItem, IOverdriveItem
    {
        public IResonantOverdrive GetOverdrivePlayer(Player player) => player.GetModPlayer<NocturnalExecutionerPlayer>();

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 80;
            Item.height = 80;
            Item.scale = 0.09f;
            Item.damage = 350;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useTurn = true;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.knockBack = 7.5f;
            Item.autoReuse = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.channel = true;
            Item.value = Item.sellPrice(gold: 45);
            Item.shoot = ModContent.ProjectileType<NocturnalExecutionerSwing>();
            Item.shootSpeed = 8f;
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
        }

        public override bool CanShoot(Player player)
        {
            if (player.altFunctionUse == 2)
                return true;

            return player.ownedProjectileCounts[Item.shoot] <= 0;
        }

        public override void HoldItem(Player player)
        {
            player.ExoBlade().rightClickListener = true;
            player.ExoBlade().mouseWorldListener = true;
            player.GetModPlayer<NocturnalExecutionerPlayer>().IsHoldingNocturnalExecutioner = true;
        }

        public override bool AltFunctionUse(Player player) => true;
        public override bool? CanHitNPC(Player player, NPC target) => false;
        public override bool CanHitPvp(Player player, Player target) => false;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                var ep = player.GetModPlayer<NocturnalExecutionerPlayer>();
                if (ep.IsChargeFull)
                {
                    ep.ConsumeCharge();
                    SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -0.5f }, player.Center);

                    // Always fire VoidRift at cursor regardless of time
                    Vector2 toCursor = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
                    Projectile.NewProjectile(source, Main.MouseWorld, Vector2.Zero,
                        ModContent.ProjectileType<VoidRiftProjectile>(),
                        damage * 3, knockback, player.whoAmI, toCursor.ToRotation());
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

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
            "Devastating void greatsword with 4-phase combo spawning nocturnal blades"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
            "Right-click performs a void dash strike through enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
            "Combo escalates: flanking blades, wide spreads, then cardinal burst finale"));
            tooltips.Add(new TooltipLine(Mod, "Effect4",
            "Hits apply Frostburn and release void-edged starlight sparks"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
            "'At midnight, the executioner does not knock. The stars simply go dark.'")
            { OverrideColor = new Color(100, 120, 200) });
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.2f;
            Texture2D tex = Terraria.GameContent.TextureAssets.Item[Type].Value;
            Vector2 drawPos = Item.position - Main.screenPosition + new Vector2(Item.width / 2f, Item.height);
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height);
            spriteBatch.Draw(tex, drawPos, null, NachtmusikPalette.CosmicPurple with { A = 0 } * pulse,
                rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, NachtmusikPalette.StarGold with { A = 0 } * (pulse * 0.5f),
                rotation, origin, scale * 1.02f, SpriteEffects.None, 0f);
        }
    }
}
