using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Materials.EnemyDrops;

namespace MagnumOpus.Content.SwanLake.Enemies
{
    public class CygnusSentinel : ModNPC
    {
        private static readonly Color SwanBlack = new Color(40, 40, 50);
        private static readonly Color SwanSilver = new Color(200, 210, 230);

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
            NPC.damage = 140;
            NPC.defense = 64;
            NPC.lifeMax = 4000;
            NPC.HitSound = SoundID.NPCHit5;
            NPC.DeathSound = SoundID.NPCDeath6;
            NPC.knockBackResist = 0.06f;
            NPC.value = Item.buyPrice(gold: 10);
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
                    "An armored sentinel of the swan's court, its onyx plating contrasted with " +
                    "alabaster feather crests. It guards the hallowed ballet with silent, elegant menace.")
            });
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (NPC.downedMoonlord &&
                (spawnInfo.Player.ZoneHallow || spawnInfo.Player.ZoneSkyHeight))
                return 0.045f;

            return 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<GraceEssence>(), 5, 1, 2));
        }

        public override void AI()
        {
            NPC.spriteDirection = NPC.direction;
            float pulse = 0.6f + 0.3f * MathF.Sin(Main.GlobalTimeWrappedHourly * 2f);
            Lighting.AddLight(NPC.Center, SwanSilver.ToVector3() * 0.3f * pulse);

            if (Main.rand.NextBool(10))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.WhiteTorch, Main.rand.NextFloat(-0.5f, 0.5f), -0.4f, 100, default, 0.6f);
                dust.noGravity = true;
            }
            if (Main.rand.NextBool(18))
            {
                float hue = Main.rand.NextFloat();
                Color prism = Main.hslToRgb(hue, 0.3f, 0.85f);
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.Enchanted_Gold, 0f, -0.3f, 60, prism, 0.4f);
                dust.noGravity = true;
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 20; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.WhiteTorch, Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f), 60, default, 1.3f);
                }
                for (int i = 0; i < 8; i++)
                {
                    float hue = Main.rand.NextFloat();
                    Color prism = Main.hslToRgb(hue, 0.4f, 0.9f);
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.Enchanted_Gold, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 40, prism, 0.9f);
                }
            }
        }

        public override Color? GetAlpha(Color drawColor)
        {
            return Color.Lerp(drawColor, SwanSilver, 0.15f);
        }
    }
}
