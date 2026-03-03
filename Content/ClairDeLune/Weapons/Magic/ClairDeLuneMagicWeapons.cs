using MagnumOpus.Common;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace MagnumOpus.Content.ClairDeLune.Weapons.Magic
{
    public class ClockworkGrimoire : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 16;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item103;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<GrimoireLightningProjectile>();
            Item.shootSpeed = 16f;
            Item.crit = 20;
        }
    }

    public class GrimoireLightningProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 2;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Electrified, 180);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    public class GrimoireCrystalProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn2, 120);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    public class GrimoireGearProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 2;
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
            target.AddBuff(BuffID.BetsysCurse, 180);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    public class GrimoireTimeFractureProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Slow, 240);
            target.AddBuff(BuffID.Confused, 120);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    public class OrreryOfDreams : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 48;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 22;
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item117;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<OrreryOrbitProjectile>();
            Item.shootSpeed = 0f;
            Item.crit = 22;
            Item.channel = true;
        }
    }

    public class OrreryControllerProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 600;
            Projectile.penetrate = -1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    public class OrreryOrbitProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = 1;
            Projectile.tileCollide = false;
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    public class OrreryBeamProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 3;
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    public class CosmicLaserProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.extraUpdates = 2;
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Electrified, 300);
            target.AddBuff(BuffID.Frostburn2, 300);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    public class RequiemOfTime : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 60;
            Item.height = 60;
            Item.mana = 12;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.knockBack = 7f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item71;
            Item.autoReuse = true;
            Item.noUseGraphic = false;
            Item.shoot = ModContent.ProjectileType<TemporalSlashProjectile>();
            Item.shootSpeed = 18f;
            Item.crit = 24;
        }
    }

    public class TemporalSlashProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.rotation = Projectile.velocity.ToRotation();
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
            target.AddBuff(BuffID.Slow, 120);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    public class TimeFreezeSweepProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 45;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Slow, 300);
            target.AddBuff(BuffID.Frozen, 60);
            target.AddBuff(BuffID.Frostburn2, 240);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }
}