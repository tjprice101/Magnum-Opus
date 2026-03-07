using MagnumOpus.Common;
using MagnumOpus.Content.Eroica.Projectiles;
using MagnumOpus.Content.Eroica;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace MagnumOpus.Content.Eroica.Weapons.BlossomOfTheSakura
{
    /// <summary>
    /// Blossom of the Sakura — Eroica ranged weapon that rains sakura arrows from afar.
    /// Arrows bloom into petal explosions on contact, with a charged Petal Storm volley alt-fire
    /// and homing Tracer Blossom shots that mark targets for bonus damage.
    /// 
    /// Heat system: sustained fire heats the barrel (ai[0] = heatProgress 0-1).
    /// Every 5th shot fires a Tracer Blossom (brighter, larger, stronger homing).
    /// </summary>
    public class BlossomOfTheSakura : ModItem
    {
        /// <summary>Per-player heat value (0-1). Increases on fire, decays when idle.</summary>
        private float _heatLevel;
        /// <summary>Shot counter for 5th-shot Tracer Blossom mechanic.</summary>
        private int _shotCounter;
        /// <summary>Frames since last shot — used for heat decay.</summary>
        private int _idleTimer;

        private const float HeatPerShot = 0.04f;
        private const float HeatDecayRate = 0.008f;
        private const int HeatDecayDelay = 20; // Frames of idle before heat starts decaying
        private const int TracerInterval = 5; // Every 5th shot

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.ResearchUnlockCount = 1;
            Item.damage = 85; // Tier 2 (300-500 range), speed-proportional for useTime=4
            Item.DamageType = DamageClass.Ranged;
            Item.width = 64;
            Item.height = 28;
            Item.useTime = 4;
            Item.useAnimation = 4;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(gold: 38);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<BlossomOfTheSakuraBulletProjectile>();
            Item.shootSpeed = 18f;
            Item.useAmmo = AmmoID.Bullet;
            Item.noMelee = true;
            Item.maxStack = 1;
        }

        public override void HoldItem(Player player)
        {
            // Heat decay when not firing
            _idleTimer++;
            if (_idleTimer > HeatDecayDelay && _heatLevel > 0f)
            {
                _heatLevel -= HeatDecayRate;
                if (_heatLevel < 0f)
                    _heatLevel = 0f;
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Reset idle timer on fire
            _idleTimer = 0;

            // Increase heat
            _heatLevel += HeatPerShot;
            if (_heatLevel > 1f)
                _heatLevel = 1f;

            // Increment shot counter
            _shotCounter++;
            bool isTracer = _shotCounter % TracerInterval == 0;

            // Tracer Blossom: higher heat, more damage, bigger visual
            float shotHeat = isTracer ? MathHelper.Clamp(_heatLevel + 0.3f, 0f, 1f) : _heatLevel;
            int shotDamage = isTracer ? (int)(damage * 1.15f) : damage;

            // Always spawn our custom VFX bullet — ignore ammo's projectile type
            int projectileType = ModContent.ProjectileType<BlossomOfTheSakuraBulletProjectile>();
            Projectile.NewProjectile(source, position, velocity,
                projectileType, shotDamage, knockback, player.whoAmI, ai0: shotHeat);

            return false; // We handled spawning ourselves
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
            "High fire-rate blossom arrows that explode into petal bursts on contact")
            { OverrideColor = EroicaPalette.Sakura });
            tooltips.Add(new TooltipLine(Mod, "Effect2",
            "Sustained fire heats the barrel — hotter shots track harder and glow brighter")
            { OverrideColor = new Color(240, 180, 100) });
            tooltips.Add(new TooltipLine(Mod, "Effect3",
            "Every 5th shot fires a Tracer Blossom that marks targets for 10% bonus damage")
            { OverrideColor = EroicaPalette.Gold });
            tooltips.Add(new TooltipLine(Mod, "Lore",
            "'Every bullet carries a petal. Every petal, a prayer.'")
            { OverrideColor = EroicaPalette.Scarlet });
        }
    }
}
