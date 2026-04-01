using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Materials.EnemyDrops;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.DiesIrae.Enemies
{
    public class WrathHerald : ModNPC
    {
        public override string Texture => "MagnumOpus/Content/DiesIrae/Enemies/WrathHerald/WrathHerald";

        private static readonly Color WrathRed = new Color(200, 50, 30);
        private static readonly Color EmberOrange = new Color(255, 120, 40);

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
            NPC.damage = 180;
            NPC.defense = 76;
            NPC.lifeMax = 6500;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.knockBackResist = 0.03f;
            NPC.value = Item.buyPrice(gold: 18);
            NPC.aiStyle = NPCAIStyleID.Fighter;
            AIType = NPCID.GraniteGolem;
            NPC.lavaImmune = true;
            NPC.scale = 0.65f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheUnderworld,
                new FlavorTextBestiaryInfoElement(
                    "A herald of divine wrath, its blood-red armor cracked and seeping ember-orange fire. " +
                    "It marches through the underworld announcing judgment with every thunderous step.")
            });
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (MoonlightSonataSystem.FateBossKilledOnce &&
                (spawnInfo.Player.ZoneUnderworldHeight || spawnInfo.Player.ZoneDungeon))
                return 0.05f;

            return 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<WrathEssence>(), 5, 1, 2));
        }

        public override void AI()
        {
            NPC.spriteDirection = NPC.direction;
            float flicker = Main.rand.NextFloat(0.85f, 1.15f);
            float pulse = 0.6f + 0.4f * MathF.Sin(Main.GlobalTimeWrappedHourly * 3.5f);
            Lighting.AddLight(NPC.Center, WrathRed.ToVector3() * 0.5f * pulse * flicker);

            if (Main.rand.NextBool(4))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.Torch, Main.rand.NextFloat(-1.5f, 1.5f), -1.2f, 60, default, 1f);
                dust.noGravity = true;
            }
            if (Main.rand.NextBool(8))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.CrimsonTorch, Main.rand.NextFloat(-0.8f, 0.8f), -0.5f, 80, default, 0.8f);
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
                        DustID.Torch, Main.rand.NextFloat(-7f, 7f), Main.rand.NextFloat(-7f, 7f), 40, default, 1.5f);
                }
                for (int i = 0; i < 12; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.CrimsonTorch, Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f), 60, default, 1.2f);
                }
            }
        }

        public override Color? GetAlpha(Color drawColor)
        {
            return Color.Lerp(drawColor, WrathRed, 0.2f);
        }
    }
}
