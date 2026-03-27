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
    public class ChorusSprite : ModNPC
    {
        private static readonly Color JoyLight = new Color(255, 220, 100);
        private static readonly Color JoyRadiance = new Color(255, 240, 180);

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
            NPC.damage = 190;
            NPC.defense = 80;
            NPC.lifeMax = 8000;
            NPC.HitSound = SoundID.NPCHit5;
            NPC.DeathSound = SoundID.NPCDeath6;
            NPC.knockBackResist = 0.03f;
            NPC.value = Item.buyPrice(gold: 23);
            NPC.aiStyle = NPCAIStyleID.HoveringFighter;
            AIType = NPCID.Pixie;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.scale = 0.65f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheHallow,
                new FlavorTextBestiaryInfoElement(
                    "A radiant sprite of the chorus, its crystalline form humming with harmonic light. " +
                    "Golden notes trail from its wings as it dances through hallowed skies in celebration.")
            });
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (MoonlightSonataSystem.FateBossKilledOnce &&
                spawnInfo.Player.ZoneHallow)
                return 0.04f;

            return 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<JoyEssence>(), 5, 1, 2));
        }

        public override void AI()
        {
            NPC.spriteDirection = NPC.direction;
            NPC.rotation = NPC.velocity.X * 0.02f;
            float pulse = 0.7f + 0.3f * MathF.Sin(Main.GlobalTimeWrappedHourly * 3f);
            Lighting.AddLight(NPC.Center, JoyLight.ToVector3() * 0.5f * pulse);

            if (Main.rand.NextBool(5))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.GoldFlame, Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f), 50, default, 0.7f);
                dust.noGravity = true;
            }
            if (Main.rand.NextBool(10))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.Enchanted_Gold, Main.rand.NextFloat(-0.6f, 0.6f), -0.5f, 30, JoyRadiance, 0.5f);
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
                        DustID.GoldFlame, Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f), 30, default, 1.3f);
                }
                for (int i = 0; i < 12; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.Enchanted_Gold, Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f), 40, JoyRadiance, 1f);
                }
            }
        }

        public override Color? GetAlpha(Color drawColor)
        {
            float glow = 0.85f + 0.15f * MathF.Sin(Main.GlobalTimeWrappedHourly * 4f);
            return Color.Lerp(drawColor, JoyRadiance, 0.25f) * glow;
        }
    }
}
