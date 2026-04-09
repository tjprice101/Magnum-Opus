using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Accessories
{
    /// <summary>
    /// Global NPC tracking Clair de Lune accessory debuffs on enemies.
    /// Brumes (melee slow), Voiles (miss chance), Pas sur la Neige (ranged slow),
    /// Berceuse (summoner defense/slow).
    /// </summary>
    public class ClairDeLuneAccessoryGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        // --- Brumes (Reverie Gauntlet melee hits) ---
        public int brumesTimer;

        // --- Voiles (Luminous Reverie Pendant magic hits) ---
        public int voilesTimer;

        // --- Pas sur la Neige (Dreambow Clasp ranged crits) ---
        public int pasSurLaNeigeTimer;

        // --- Berceuse (Dreamsinger Sigil minion hits) ---
        public int berceuseTimer;

        public override void ResetEffects(NPC npc)
        {
            if (brumesTimer > 0) brumesTimer--;
            if (voilesTimer > 0) voilesTimer--;
            if (pasSurLaNeigeTimer > 0) pasSurLaNeigeTimer--;
            if (berceuseTimer > 0) berceuseTimer--;
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            // Berceuse: -5 defense
            if (berceuseTimer > 0)
                modifiers.Defense.Flat -= 5;
        }

        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            if (berceuseTimer > 0)
                modifiers.Defense.Flat -= 5;
        }

        public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
        {
            // Voiles: 15% miss chance (approximated as 15% damage reduction)
            if (voilesTimer > 0)
                modifiers.FinalDamage *= 0.85f;
        }

        public override void PostAI(NPC npc)
        {
            // Brumes: -12% movement speed, -8% attack speed (via velocity)
            if (brumesTimer > 0)
                npc.velocity *= 0.88f;

            // Pas sur la Neige: -15% movement speed
            if (pasSurLaNeigeTimer > 0)
                npc.velocity *= 0.85f;

            // Berceuse: -10% movement speed
            if (berceuseTimer > 0)
                npc.velocity *= 0.90f;
        }

        public void ApplyBrumes(int duration = 180) { brumesTimer = duration; }
        public void ApplyVoiles(int duration = 240) { voilesTimer = duration; }
        public void ApplyPasSurLaNeige(int duration = 180) { pasSurLaNeigeTimer = duration; }
        public void ApplyBerceuse(int duration = 180) { berceuseTimer = duration; }

        public bool HasBrumes => brumesTimer > 0;
        public bool HasVoiles => voilesTimer > 0;
        public bool HasPasSurLaNeige => pasSurLaNeigeTimer > 0;
        public bool HasBerceuse => berceuseTimer > 0;
    }
}