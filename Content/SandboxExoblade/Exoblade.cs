using System.Linq;
using MagnumOpus.Content.SandboxExoblade.Projectiles;
using MagnumOpus.Content.SandboxExoblade.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace MagnumOpus.Content.SandboxExoblade
{
    public class Exoblade : ModItem
    {
        public static readonly SoundStyle SwingSound = new("MagnumOpus/Content/SandboxExoblade/Sounds/ExobladeSwing") { MaxInstances = 3, PitchVariance = 0.6f, Volume = 0.8f };
        public static readonly SoundStyle BigSwingSound = new("MagnumOpus/Content/SandboxExoblade/Sounds/ExobladeBigSwing") { MaxInstances = 3, PitchVariance = 0.2f };
        public static readonly SoundStyle BigHitSound = new("MagnumOpus/Content/SandboxExoblade/Sounds/ExobladeBigHit") { PitchVariance = 0.2f };
        public static readonly SoundStyle BeamHitSound = new("MagnumOpus/Content/SandboxExoblade/Sounds/ExobladeBeamSlash") { Volume = 0.4f, PitchVariance = 0.2f };
        public static readonly SoundStyle DashSound = new("MagnumOpus/Content/SandboxExoblade/Sounds/ExobladeDash") { Volume = 0.6f };
        public static readonly SoundStyle DashHitSound = new("MagnumOpus/Content/SandboxExoblade/Sounds/ExobladeDashImpact") { Volume = 0.85f };

        public static int BeamNoHomeTime = 24;
        public static float NotTrueMeleeDamagePenalty = 0.35f;
        public static float ExplosionDamageFactor = 1.8f;
        public static float LungeDamageFactor = 1.75f;
        public static int LungeCooldown = 60 * 3;
        public static float LungeMaxCorrection = MathHelper.PiOver4 * 0.05f;
        public static float LungeSpeed = 60f;
        public static float ReboundSpeed = 6f;
        public static float PercentageOfAnimationSpentLunging = 0.6f;
        public static int OpportunityForBigSlash = 37 * 3;
        public static float BigSlashUpscaleFactor = 1.5f;
        public static int DashTime = 49;
        public static int BaseUseTime = 49;
        public static int BeamsPerSwing = 4;

        public override void SetDefaults()
        {
            Item.width = 138;
            Item.height = 184;
            Item.damage = 915;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = BaseUseTime;
            Item.useAnimation = BaseUseTime;
            Item.useTurn = true;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.knockBack = 9f;
            Item.autoReuse = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.value = Item.buyPrice(platinum: 36);
            Item.shoot = ProjectileType<ExobladeProj>();
            Item.shootSpeed = 9f;
            Item.rare = ItemRarityID.Purple;
        }

        public override bool CanShoot(Player player)
        {
            if (player.altFunctionUse == 2)
                return !Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == ProjectileType<ExobladeProj>());

            return !Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == ProjectileType<ExobladeProj>() &&
            !(n.ai[0] == 1 && n.ai[1] == 1));
        }

        public override void HoldItem(Player player)
        {
            player.ExoBlade().rightClickListener = true;
            player.ExoBlade().mouseWorldListener = true;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool? CanHitNPC(Player player, NPC target) => false;

        public override bool CanHitPvp(Player player, Player target) => false;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float state = 0;

            bool empoweredSlash = false;
            foreach (Projectile p in Main.ActiveProjectiles)
            {
                if (p.owner == player.whoAmI && p.type == Item.shoot && p.ai[0] == 1 && p.ai[1] == 1 && p.timeLeft > LungeCooldown)
                {
                    empoweredSlash = true;
                    break;
                }
            }

            if (empoweredSlash)
            {
                state = 2;
                foreach (Projectile p in Main.ActiveProjectiles)
                {
                    if (p.owner != player.whoAmI || p.type != Item.shoot || p.ai[0] != 1 || p.ai[1] != 1)
                        continue;
                    p.timeLeft = LungeCooldown;
                    p.netUpdate = true;
                }
            }

            if (player.altFunctionUse == 2)
                state = 1;

            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, state, 0);
            return false;
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Item.DrawItemGlowmaskSingleFrame(spriteBatch, rotation, Request<Texture2D>("MagnumOpus/Content/SandboxExoblade/ExobladeGlow").Value);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Zenith)
                .AddIngredient(ItemID.LunarBar, 20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
