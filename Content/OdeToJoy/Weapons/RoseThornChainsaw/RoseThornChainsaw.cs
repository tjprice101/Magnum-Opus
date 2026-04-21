using MagnumOpus.Content.OdeToJoy.Weapons.RoseThornChainsaw.Projectiles;
using MagnumOpus.Content.OdeToJoy.Systems;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.RoseThornChainsaw
{
    /// <summary>
    /// Rose Thorn Chainsaw — Ode to Joy melee sword.
    /// Left-click: Swing fires 3 orbs in rapid burst (tight cone ±5°). Short timeLeft (40 frames), no homing.
    /// Right-click: Activate 5-second empowerment aura. Orbs gain pierce 2, wider spread (±8°), accelerate.
    /// </summary>
    public class RoseThornChainsaw : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 65;
            Item.height = 65;
            Item.scale = 0.09f;
            Item.damage = 260;
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
            Item.shoot = ModContent.ProjectileType<RoseThornChainsawProjectile>();
            Item.shootSpeed = 8f;
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
        }

        public override bool CanShoot(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;
        public override bool? CanHitNPC(Player player, NPC target) => false;
        public override bool CanHitPvp(Player player, Player target) => false;

        public override bool AltFunctionUse(Player player) => true;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                // Right-click: Activate empowerment aura for 5 seconds
                var combat = player.GetModPlayer<OdeToJoyCombatPlayer>();
                combat.ActivateChainsawEmpowerment();

                SoundEngine.PlaySound(SoundID.Item73 with { Pitch = 0.4f, Volume = 0.7f }, player.Center);

                // Empowerment activation VFX
                OdeToJoyVFXLibrary.SpawnBloomBurst(player.MountedCenter, 8, 0.9f);
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6;
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                    Color col = Color.Lerp(OdeToJoyPalette.RosePink, OdeToJoyPalette.GoldenPollen, Main.rand.NextFloat());
                    Dust d = Dust.NewDustPerfect(player.MountedCenter, DustID.GoldFlame, vel, 0, col, 0.9f);
                    d.noGravity = true;
                }

                return false;
            }

            // Left-click: Normal swing
            Projectile.NewProjectile(source, player.MountedCenter,
                (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX),
                type, damage, knockback, player.whoAmI, 0f, 0);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var combat = player.GetModPlayer<OdeToJoyCombatPlayer>();

            if (combat.ChainsawEmpowered)
            {
                tooltips.Add(new TooltipLine(Mod, "Empowered", "[c/FFD700:EMPOWERMENT AURA ACTIVE]")
                { OverrideColor = OdeToJoyPalette.GoldenPollen });
            }

            tooltips.Add(new TooltipLine(Mod, "Effect1", "Swing fires 3 rapid orbs in tight cone (±5°)"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Right-click activates empowerment for 5 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Empowered orbs: pierce 2, wider spread, accelerating"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
            "'Every rose has its chainsaw.'")
            { OverrideColor = OdeToJoyPalette.LoreText });
        }
    }
}
