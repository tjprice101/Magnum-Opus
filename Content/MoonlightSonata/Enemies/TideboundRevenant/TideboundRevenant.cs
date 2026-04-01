using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Materials.EnemyDrops;

namespace MagnumOpus.Content.MoonlightSonata.Enemies
{
    public class TideboundRevenant : ModNPC
    {
        public override string Texture => "MagnumOpus/Content/MoonlightSonata/Enemies/TideboundRevenant/TideboundRevenant";

        private static readonly Color MoonlightPurple = new Color(140, 100, 200);
        private static readonly Color MoonlightBlue = new Color(100, 140, 220);

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
            NPC.damage = 90;
            NPC.defense = 42;
            NPC.lifeMax = 1800;
            NPC.HitSound = SoundID.NPCHit36;
            NPC.DeathSound = SoundID.NPCDeath39;
            NPC.knockBackResist = 0.15f;
            NPC.value = Item.buyPrice(gold: 5);
            NPC.aiStyle = NPCAIStyleID.Fighter;
            AIType = NPCID.Herpling;
            NPC.scale = 0.65f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                new FlavorTextBestiaryInfoElement(
                    "A spectral revenant bound by tidal sorrow, draped in flowing violet robes " +
                    "that ripple like moonlit water. Its hollow eyes weep silver light.")
            });
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (NPC.downedMoonlord &&
                !Main.dayTime &&
                spawnInfo.Player.ZoneOverworldHeight)
                return 0.06f;

            return 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<LunarEssence>(), 5, 1, 2));
        }

        public override void AI()
        {
            NPC.spriteDirection = NPC.direction;
            float pulse = 0.7f + 0.3f * MathF.Sin(Main.GlobalTimeWrappedHourly * 2.5f);
            Lighting.AddLight(NPC.Center, MoonlightPurple.ToVector3() * 0.4f * pulse);

            if (Main.rand.NextBool(8))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.PurpleTorch, Main.rand.NextFloat(-1f, 1f), -0.8f, 100, default, 0.7f);
                dust.noGravity = true;
            }
            if (Main.rand.NextBool(14))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.BlueTorch, Main.rand.NextFloat(-0.5f, 0.5f), -0.5f, 80, default, 0.5f);
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
                        DustID.PurpleTorch, Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f), 60, default, 1.3f);
                }
                for (int i = 0; i < 10; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.BlueTorch, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 1f), 80, default, 0.9f);
                }
            }
        }

        public override Color? GetAlpha(Color drawColor)
        {
            return Color.Lerp(drawColor, MoonlightBlue, 0.25f);
        }
    }
}
