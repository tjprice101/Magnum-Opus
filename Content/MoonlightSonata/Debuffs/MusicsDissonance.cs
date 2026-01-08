using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Debuffs
{
    public class MusicsDissonance : ModBuff 
    {
        public override void SetStaticDefaults()
        {
            // This is a debuff, so NPCs should be able to have it
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<MusicsDissonanceNPC>().HasDissonance = true;
        }
    }

    public class MusicsDissonanceNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool HasDissonance { get; set; } = false;
        private float waveTimer = 0f;

        public override void ResetEffects(NPC npc)
        {
            HasDissonance = false;
        }

        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (HasDissonance)
            {
                // 5% more vulnerable to damage
                modifiers.FinalDamage *= 1.05f;
            }
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (HasDissonance)
            {
                // Wavy purple visual effect particles
                waveTimer += 0.1f;
                
                if (Main.rand.NextBool(3))
                {
                    // Create wavy particle effect around the NPC
                    float waveOffset = (float)System.Math.Sin(waveTimer) * 8f;
                    Vector2 dustPos = npc.Center + new Vector2(waveOffset, Main.rand.NextFloat(-npc.height / 2, npc.height / 2));
                    
                    Dust dust = Dust.NewDustDirect(dustPos, 1, 1, DustID.PurpleTorch, 0f, -1f, 150, default, 1.0f);
                    dust.noGravity = true;
                    dust.velocity *= 0.3f;
                }

                if (Main.rand.NextBool(5))
                {
                    // Occasional brighter sparkle
                    Dust sparkle = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Enchanted_Pink, 0f, 0f, 100, default, 0.6f);
                    sparkle.noGravity = true;
                    sparkle.velocity = Main.rand.NextVector2Circular(1f, 1f);
                }
            }
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (HasDissonance)
            {
                // Wavy purple highlight color
                float wave = (float)System.Math.Sin(Main.GameUpdateCount * 0.1f + npc.whoAmI) * 0.5f + 0.5f;
                
                // Blend with purple based on wave
                byte r = (byte)MathHelper.Lerp(drawColor.R, 180, wave * 0.4f);
                byte g = (byte)MathHelper.Lerp(drawColor.G, 80, wave * 0.5f);
                byte b = (byte)MathHelper.Lerp(drawColor.B, 255, wave * 0.4f);
                
                drawColor = new Color(r, g, b, drawColor.A);
            }
        }
    }
}
