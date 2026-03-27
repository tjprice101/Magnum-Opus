using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Materials.EnemyDrops;

namespace MagnumOpus.Content.LaCampanella.Enemies
{
    public class CinderVirtuoso : ModNPC
    {
        private static readonly Color CinderFlame = new Color(255, 100, 20);
        private static readonly Color CinderEmber = new Color(200, 80, 10);

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;
            NPCID.Sets.NPCBestiaryDrawModifiers bestiaryData = new()
            {
                Velocity = 1f
            };
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = bestiaryData;
        }

        public override void SetDefaults()
        {
            NPC.width = 130;
            NPC.height = 77;
            NPC.damage = 110;
            NPC.defense = 46;
            NPC.lifeMax = 2500;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.knockBackResist = 0.12f;
            NPC.value = Item.buyPrice(gold: 7);
            NPC.aiStyle = NPCAIStyleID.Fighter;
            AIType = NPCID.GraniteGolem;
            NPC.lavaImmune = true;
            NPC.scale = 0.65f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Desert,
                new FlavorTextBestiaryInfoElement(
                    "A virtuoso of flame and percussion, its molten body pulses with the rhythm of an infernal symphony. " +
                    "Each step leaves scorched footprints that smolder with golden embers.")
            });
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (NPC.downedMoonlord &&
                spawnInfo.Player.ZoneDesert)
                return 0.05f;

            return 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BellEssence>(), 5, 1, 2));
        }

        public override void AI()
        {
            NPC.spriteDirection = NPC.direction;
            float flicker = Main.rand.NextFloat(0.85f, 1.15f);
            float pulse = 0.6f + 0.4f * MathF.Sin(Main.GlobalTimeWrappedHourly * 4f);
            Lighting.AddLight(NPC.Center, CinderFlame.ToVector3() * 0.45f * pulse * flicker);

            if (Main.rand.NextBool(4))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.Torch, Main.rand.NextFloat(-1f, 1f), -1.2f, 60, default, 0.9f);
                dust.noGravity = true;
            }
            if (Main.rand.NextBool(10))
            {
                Dust dust = Dust.NewDustDirect(NPC.position + new Vector2(0, NPC.height * 0.7f), NPC.width, 10,
                    DustID.GoldFlame, NPC.velocity.X * 0.3f, -0.3f, 100, default, 0.6f);
                dust.noGravity = true;
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 25; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.Torch, Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f), 40, default, 1.5f);
                }
                for (int i = 0; i < 12; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.GoldFlame, Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f), 80, default, 1f);
                }
            }
        }

        public override Color? GetAlpha(Color drawColor)
        {
            return Color.Lerp(drawColor, CinderEmber, 0.15f);
        }
    }
}
