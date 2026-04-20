using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation.Utilities
{
    public class TheStandingOvationPlayer : ModPlayer
    {
        // Ovation level builds toward an encore performance
        public int ovationLevel;
        public bool encoreReady;
        public bool isActive;
        public int activeTimer;

        // Combat timer for meter drain
        public int combatTimer;
        private int _drainTimer;

        public override void ResetEffects()
        {
            if (!isActive)
            {
                if (activeTimer > 0)
                    activeTimer--;
                if (activeTimer <= 0)
                {
                    ovationLevel = 0;
                    encoreReady = false;
                }
            }
            isActive = false;
        }

        public override void PostUpdate()
        {
            // Decrement combat timer
            if (combatTimer > 0)
                combatTimer--;

            // Out-of-combat meter drain: if no combat for 120 frames, drain 1 ovation every 120 frames
            if (combatTimer <= 0 && ovationLevel > 0)
            {
                _drainTimer++;
                if (_drainTimer >= 120)
                {
                    _drainTimer = 0;
                    ovationLevel = System.Math.Max(0, ovationLevel - 1);
                }
            }
            else
            {
                _drainTimer = 0;
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Only count hits from Standing Ovation orbs (GenericHomingOrbChild with OdeToJoy theme)
            // and from the StandingOvationMinion itself
            int minionType = ModContent.ProjectileType<Projectiles.StandingOvationMinion>();
            int orbType = ModContent.ProjectileType<GenericHomingOrbChild>();
            if (proj.type == minionType || proj.type == orbType)
            {
                combatTimer = 120;
                if (hit.Crit)
                    AddOvation(2);
                else
                    AddOvation(1);

                // Kill bonus: if target is about to die
                if (target.life - damageDone <= 0)
                    KillBonus();
            }
        }

        public void AddOvation(int amount = 1)
        {
            ovationLevel = System.Math.Min(ovationLevel + amount, 10);
            activeTimer = 120;

            if (ovationLevel >= 10)
                encoreReady = true;
        }

        public void KillBonus()
        {
            AddOvation(3);
        }

        public void TriggerEncore()
        {
            ovationLevel = 0;
            encoreReady = false;
        }

        public float GetOvationIntensity()
        {
            return ovationLevel / 10f;
        }
    }

    public static class TheStandingOvationPlayerExtensions
    {
        public static TheStandingOvationPlayer TheStandingOvation(this Player player)
            => player.GetModPlayer<TheStandingOvationPlayer>();
    }
}
