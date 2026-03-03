using MagnumOpus.Common;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace MagnumOpus.Content.ClairDeLune.Weapons.Ranged
{
    public class StarfallWhisper : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 68;
            Item.height = 28;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item75;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<StarfallBoltProjectile>();
            Item.shootSpeed = 22f;
            Item.useAmmo = AmmoID.Bullet;
        }
    }

    public class StarfallBoltProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 2;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn2, 180);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    public class StarfallRiftProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    public class StarfallCrystalProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Slow, 120);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    public class MidnightMechanism : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 72;
            Item.height = 36;
            Item.DamageType = DamageClass.Ranged;
            Item.useAnimation = 6;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 2f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<MechanismBoltProjectile>();
            Item.shootSpeed = 20f;
            Item.useAmmo = AmmoID.Bullet;
            Item.crit = 16;
        }
    }

    public class MechanismBoltProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    public class SynchronizedGearBoltProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 150;
            Projectile.tileCollide = true;
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Slow, 180);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    public class CogAndHammer : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 64;
            Item.height = 32;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 12f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item61;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<ClockworkBombProjectile>();
            Item.shootSpeed = 14f;
            Item.useAmmo = AmmoID.Rocket;
            Item.crit = 18;
        }
    }

    public class ClockworkBombProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    public class GearShrapnelProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = true;
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    public class TemporalSingularityProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 120;
            Projectile.penetrate = -1;
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }
}