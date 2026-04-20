using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace MagnumOpus.Common.Systems.UI
{
    public class ResonantWeaponOverdrivePlayer : ModPlayer
    {
        private int _activationLockout;

        public IResonantOverdrive GetHeldOverdrive()
        {
            if (Player.HeldItem?.ModItem is IOverdriveItem overdriveItem)
                return overdriveItem.GetOverdrivePlayer(Player);

            return null;
        }

        public override void ResetEffects()
        {
            if (_activationLockout > 0)
                _activationLockout--;
        }

        public bool TryActivateHeldWeaponOverdrive()
        {
            if (_activationLockout > 0)
                return false;

            IResonantOverdrive overdrive = GetHeldOverdrive();
            if (overdrive == null || !overdrive.IsOverdriveReady)
                return false;

            if (Player.whoAmI != Main.myPlayer)
                return true;

            if (overdrive.IsOverdriveOnCooldown)
            {
                string msg = overdrive.OverdriveCooldownMessage ?? "Cooling down";
                CombatText.NewText(Player.Hitbox, Color.MediumPurple, msg, true);
                return false;
            }

            bool result = overdrive.ActivateOverdrive(Player);
            if (result)
                _activationLockout = 20;

            return result;
        }
    }

    public class ResonantOverdriveGlobalItem : GlobalItem
    {
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
            => entity.ModItem is IOverdriveItem;

        public override bool AltFunctionUse(Item item, Player player)
            => item.ModItem is IOverdriveItem;

        public override bool CanUseItem(Item item, Player player)
        {
            if (player.altFunctionUse != 2)
                return base.CanUseItem(item, player);

            if (item.ModItem is not IOverdriveItem overdriveItem)
                return base.CanUseItem(item, player);

            IResonantOverdrive overdrive = overdriveItem.GetOverdrivePlayer(player);
            if (overdrive == null || !overdrive.IsOverdriveReady)
                return false;

            ResonantWeaponOverdrivePlayer router = player.GetModPlayer<ResonantWeaponOverdrivePlayer>();
            bool activated = router.TryActivateHeldWeaponOverdrive();
            return !activated;
        }
    }

    public class ResonantOverdriveGlobalProjectile : GlobalProjectile
    {
        private bool _empowerApplied;
        private int _freezeTimer;
        private bool _stored;
        private Vector2 _storedVelocity;

        public override bool InstancePerEntity => true;

        public void FreezeFor(int ticks)
        {
            _freezeTimer = Math.Max(_freezeTimer, ticks);
        }

        public override void AI(Projectile projectile)
        {
            if (_freezeTimer > 0)
            {
                if (!_stored)
                {
                    _storedVelocity = projectile.velocity;
                    _stored = true;
                }

                projectile.velocity = Vector2.Zero;
                _freezeTimer--;

                if (_freezeTimer <= 0 && _stored)
                {
                    projectile.velocity = _storedVelocity;
                    _stored = false;
                }
            }

            if (!projectile.active || projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
                return;
        }
    }

    public class ResonantOverdriveGlobalNpc : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        private int _fractalDotTimer;
        private int _fractalTickDamage;

        public void ApplyFractalDot(int weaponBaseDamage)
        {
            _fractalDotTimer = 180;
            _fractalTickDamage = Math.Max(1, (int)Math.Ceiling((weaponBaseDamage * 2f) / 6f));
        }

        public override void AI(NPC npc)
        {
            if (_fractalDotTimer <= 0)
                return;

            _fractalDotTimer--;
            if (_fractalDotTimer % 30 == 0 && npc.active && !npc.friendly && !npc.dontTakeDamage && npc.life > 0)
            {
                npc.SimpleStrikeNPC(_fractalTickDamage, 0, false, 0f, DamageClass.Melee, false, 0f, true);
            }
        }
    }

    public class ResonantOverdriveChargeBarLayer : PlayerDrawLayer
    {
        private const int BarWidth = 76;
        private const int BarHeight = 8;
        private const int YOffset = 36;

        public override Position GetDefaultPosition()
            => new AfterParent(PlayerDrawLayers.FrontAccFront);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            if (player.dead || player.invis)
                return false;

            return player.GetModPlayer<ResonantWeaponOverdrivePlayer>().GetHeldOverdrive() != null;
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            IResonantOverdrive overdrive = player.GetModPlayer<ResonantWeaponOverdrivePlayer>().GetHeldOverdrive();
            if (overdrive == null)
                return;

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            if (pixel == null)
                return;

            float progress = overdrive.OverdriveCharge;
            bool full = overdrive.IsOverdriveReady;

            Vector2 center = drawInfo.Position - Main.screenPosition + player.Size * 0.5f;
            float x = center.X - BarWidth * 0.5f;
            float y = center.Y + YOffset;

            DrawRect(ref drawInfo, pixel, x - 1, y - 1, BarWidth + 2, BarHeight + 2, Color.Black * 0.75f);
            DrawRect(ref drawInfo, pixel, x, y, BarWidth, BarHeight, new Color(20, 20, 20, 220));

            int fillWidth = Math.Max(0, (int)(BarWidth * progress));
            if (fillWidth > 0)
            {
                Color fill = Color.Lerp(overdrive.OverdriveLowColor, overdrive.OverdriveHighColor, progress);
                if (full)
                {
                    float pulse = 0.5f + 0.5f * MathF.Sin(Main.GlobalTimeWrappedHourly * 8f);
                    fill = Color.Lerp(overdrive.OverdriveHighColor, Color.White, pulse * 0.35f);
                }

                DrawRect(ref drawInfo, pixel, x, y, fillWidth, BarHeight, fill);
                DrawRect(ref drawInfo, pixel, x, y, fillWidth, 1, Color.White * 0.45f);
            }

            DrawRect(ref drawInfo, pixel, x - 1, y - 1, BarWidth + 2, 1, Color.White * 0.25f);
            DrawRect(ref drawInfo, pixel, x - 1, y + BarHeight, BarWidth + 2, 1, Color.White * 0.25f);
            DrawRect(ref drawInfo, pixel, x - 1, y, 1, BarHeight, Color.White * 0.2f);
            DrawRect(ref drawInfo, pixel, x + BarWidth, y, 1, BarHeight, Color.White * 0.2f);
        }

        private static void DrawRect(ref PlayerDrawSet drawInfo, Texture2D pixel, float x, float y, int width, int height, Color color)
        {
            if (width <= 0 || height <= 0)
                return;

            DrawData data = new DrawData(
                pixel,
                new Vector2(x, y),
                new Rectangle(0, 0, 1, 1),
                color,
                0f,
                Vector2.Zero,
                new Vector2(width, height),
                SpriteEffects.None,
                0);

            drawInfo.DrawDataCache.Add(data);
        }
    }
}
