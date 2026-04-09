using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Nachtmusik.Accessories
{
    /// <summary>
    /// Tracks all Nachtmusik-themed enemy debuffs applied by accessories and equipment chains.
    /// Sotto Voce, Serenade's Echo, Lullaby.
    /// </summary>
    public class NachtmusikAccessoryGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        // Sotto Voce: -8 defense, slowed (melee crit proc)
        public int sottoVoceTimer;

        // Serenade's Echo: +10% magic damage taken, spreads (magic hit proc)
        public int serenadeEchoTimer;

        // Lullaby: -15% movement speed, -5 defense (summoner chain proc)
        public int lullabyTimer;

        public override void ResetEffects(NPC npc)
        {
            if (sottoVoceTimer > 0) sottoVoceTimer--;
            if (serenadeEchoTimer > 0) serenadeEchoTimer--;
            if (lullabyTimer > 0) lullabyTimer--;
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            ApplyDefenseReductions(ref modifiers);

            if (serenadeEchoTimer > 0 && projectile.CountsAsClass(DamageClass.Magic))
                modifiers.FinalDamage += 0.10f;
        }

        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            ApplyDefenseReductions(ref modifiers);

            if (serenadeEchoTimer > 0 && item.CountsAsClass(DamageClass.Magic))
                modifiers.FinalDamage += 0.10f;
        }

        private void ApplyDefenseReductions(ref NPC.HitModifiers modifiers)
        {
            if (sottoVoceTimer > 0)
                modifiers.Defense.Flat -= 8;
            if (lullabyTimer > 0)
                modifiers.Defense.Flat -= 5;
        }

        public override void PostAI(NPC npc)
        {
            // Slow enemies with active debuffs
            if (sottoVoceTimer > 0)
                npc.velocity *= 0.85f;
            if (lullabyTimer > 0)
                npc.velocity *= 0.85f;
        }

        public void ApplySottoVoce(int durationFrames)
        {
            sottoVoceTimer = System.Math.Max(sottoVoceTimer, durationFrames);
        }

        public void ApplySerenadeEcho(int durationFrames)
        {
            serenadeEchoTimer = System.Math.Max(serenadeEchoTimer, durationFrames);
        }

        public void SpreadSerenadeEcho(NPC source, int maxSpread, int durationFrames)
        {
            int spread = 0;
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                if (npc.whoAmI == source.whoAmI) continue;
                if (npc.GetGlobalNPC<NachtmusikAccessoryGlobalNPC>().serenadeEchoTimer > 0) continue;
                if (Vector2.Distance(source.Center, npc.Center) > 300f) continue;

                npc.GetGlobalNPC<NachtmusikAccessoryGlobalNPC>().serenadeEchoTimer = durationFrames;
                spread++;
                if (spread >= maxSpread) break;
            }
        }

        public void ApplyLullaby(int durationFrames)
        {
            lullabyTimer = System.Math.Max(lullabyTimer, durationFrames);
        }
    }
}
