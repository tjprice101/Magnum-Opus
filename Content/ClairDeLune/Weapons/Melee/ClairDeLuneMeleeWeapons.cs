using MagnumOpus.Common;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace MagnumOpus.Content.ClairDeLune.Weapons.Melee
{
    public class Chronologicality : ModItem
    {
        public override void SetDefaults()
        {
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.width = 58;
            Item.height = 28;
            Item.useTime = 1;
            Item.useAnimation = 1;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.channel = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item22;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<ChronologicalityProjectile>();
            Item.shootSpeed = 45f;
        }
    }

    public class ChronologicalityProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 58;
            Projectile.height = 28;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.ownerHitCheck = true;
            Projectile.aiStyle = 20;
            Projectile.hide = true;
            Projectile.timeLeft = 2;
        }

        public override void AI()
        {
            Projectile.timeLeft = 2;
            Player player = Main.player[Projectile.owner];
            Projectile.Center = player.Center;
            Projectile.rotation += 0.3f * Projectile.direction;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Slow, 240);
            target.AddBuff(BuffID.Frostburn2, 180);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    public class TemporalRiftProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
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

    public class TemporalPiercer : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 84;
            Item.height = 84;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 7f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item71;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.crit = 22;
            Item.shoot = ModContent.ProjectileType<TemporalPiercerProjectile>();
            Item.shootSpeed = 5f;
        }
    }

    public class TemporalPiercerProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 20;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
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

    public class TimeFractureExplosion : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 200;
            Projectile.height = 200;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 10;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Slow, 300);
            target.AddBuff(BuffID.Frostburn2, 240);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    public class ClockworkHarmony : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 88;
            Item.height = 88;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 9f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item71;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.scale = 1.6f;
            Item.crit = 22;
            Item.shoot = ModContent.ProjectileType<GearWaveProjectile>();
            Item.shootSpeed = 16f;
        }
    }

    public class GearWaveProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 8;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
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

    public class SynchronizedGearExplosion : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 280;
            Projectile.height = 280;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 12;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Slow, 360);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }
}