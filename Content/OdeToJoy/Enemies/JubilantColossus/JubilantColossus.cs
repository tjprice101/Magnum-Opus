using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Materials.EnemyDrops;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.OdeToJoy.Enemies
{
    public class JubilantColossus : ModNPC
    {
        private static readonly Color JoyGold = new Color(255, 200, 50);
        private static readonly Color JoyAmber = new Color(240, 170, 40);

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
            NPC.damage = 195;
            NPC.defense = 82;
            NPC.lifeMax = 7500;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.knockBackResist = 0.02f;
            NPC.value = Item.buyPrice(gold: 22);
            NPC.aiStyle = NPCAIStyleID.Fighter;
            AIType = NPCID.GraniteGolem;
            NPC.scale = 0.65f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheHallow,
                new FlavorTextBestiaryInfoElement(
                    "A jubilant colossus of gold and light, its massive form radiating warm amber energy. " +
                    "Each of its thundering steps sends ripples of golden joy through the hallowed earth.")
            });
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (MoonlightSonataSystem.FateBossKilledOnce &&
                spawnInfo.Player.ZoneHallow)
                return 0.045f;

            return 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<JoyEssence>(), 5, 1, 2));
        }

        public override void AI()
        {
            NPC.spriteDirection = NPC.direction;
            float pulse = 0.7f + 0.3f * MathF.Sin(Main.GlobalTimeWrappedHourly * 2.5f);
            Lighting.AddLight(NPC.Center, JoyGold.ToVector3() * 0.5f * pulse);

            if (Main.rand.NextBool(6))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.GoldFlame, Main.rand.NextFloat(-1f, 1f), -0.8f, 60, default, 0.8f);
                dust.noGravity = true;
            }
            if (Main.rand.NextBool(12))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.Enchanted_Gold, Main.rand.NextFloat(-0.5f, 0.5f), -0.4f, 40, JoyAmber, 0.5f);
                dust.noGravity = true;
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 28; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.GoldFlame, Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f), 40, default, 1.4f);
                }
                for (int i = 0; i < 10; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.Enchanted_Gold, Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f), 50, JoyAmber, 0.9f);
                }
            }
        }

        public override Color? GetAlpha(Color drawColor)
        {
            return Color.Lerp(drawColor, JoyGold, 0.2f);
        }
    }
}
