using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;
using MagnumOpus.Content.Materials.Foundation;
using MagnumOpus.Content.Materials.EnemyDrops;
using MagnumOpus.Content.Spring.Materials;
using MagnumOpus.Content.Summer.Materials;
using MagnumOpus.Content.Autumn.Materials;
using MagnumOpus.Content.Winter.Materials;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// GlobalNPC that adds Foundation Material drops to vanilla and modded enemies.
    /// Handles all Phase 1 material drops from the Enhancements document.
    /// </summary>
    public class FoundationMaterialDrops : GlobalNPC
    {
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            // ========================================
            // PRE-HARDMODE DROPS
            // ========================================

            // Minor Music Note - Surface enemies (common) - 5%
            if (IsSurfaceEnemy(npc.type))
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<MinorMusicNote>(), 20)); // 5%
            }

            // Tuning Fork - Cavern enemies (uncommon) - 3%
            if (IsCavernEnemy(npc.type))
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<TuningFork>(), 33)); // 3%
            }

            // ========================================
            // SPRING ENEMY DROPS (Hardmode)
            // ========================================

            // Petal of Rebirth - Plantera's Tentacles (8%), Jungle HM enemies (3%)
            if (npc.type == NPCID.PlanterasTentacle)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<PetalOfRebirth>(), 12)); // ~8%
            }
            else if (IsHardmodeJungleEnemy(npc.type))
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<PetalOfRebirth>(), 33)); // 3%
            }

            // Vernal Dust - Jungle enemies (HM) - 5%
            if (IsHardmodeJungleEnemy(npc.type))
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<VernalDust>(), 20)); // 5%
            }

            // Rainbow Petal - Rainbow Slime - 10%
            if (npc.type == NPCID.RainbowSlime)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<RainbowPetal>(), 10)); // 10%
            }

            // Blossom Essence - Plantera (100% guaranteed 3-5)
            if (npc.type == NPCID.Plantera)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BlossomEssence>(), 1, 3, 5));
            }

            // ========================================
            // SUMMER ENEMY DROPS (Hardmode)
            // ========================================

            // Ember of Intensity - Solar Pillar enemies (5%), Lava enemies (3%)
            if (IsSolarPillarEnemy(npc.type))
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<EmberOfIntensity>(), 20)); // 5%
            }
            else if (IsLavaEnemy(npc.type))
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<EmberOfIntensity>(), 33)); // 3%
            }

            // Sunfire Core - Mothron - 15%
            if (npc.type == NPCID.Mothron)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<SunfireCore>(), 7)); // ~15%
            }

            // Heat Scale - Lava Bat, Fire Imp, Hell enemies - 6%
            if (IsHellEnemy(npc.type))
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<HeatScale>(), 17)); // 6%
            }

            // Solar Essence - Golem (100% guaranteed 3-5)
            if (npc.type == NPCID.Golem)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<SolarEssence>(), 1, 3, 5));
            }

            // ========================================
            // AUTUMN ENEMY DROPS (Hardmode)
            // ========================================

            // Leaf of Ending - Pumpking (12%), Eclipse enemies (3%)
            if (npc.type == NPCID.Pumpking)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<LeafOfEnding>(), 8)); // ~12%
            }
            else if (IsEclipseEnemy(npc.type))
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<LeafOfEnding>(), 33)); // 3%
            }

            // Twilight Wing Fragment - Mothron - 10%
            if (npc.type == NPCID.Mothron)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<TwilightWingFragment>(), 10)); // 10%
            }

            // Death's Note - Reaper - 8%
            if (npc.type == NPCID.Reaper)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<DeathsNote>(), 12)); // ~8%
            }

            // Decay Fragment - Any Eclipse enemy - 4%
            if (IsEclipseEnemy(npc.type))
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<DecayFragment>(), 25)); // 4%
            }

            // Decay Essence - Pumpking (100% guaranteed 3-5)
            if (npc.type == NPCID.Pumpking)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<DecayEssence>(), 1, 3, 5));
            }

            // ========================================
            // WINTER ENEMY DROPS (Hardmode)
            // ========================================

            // Shard of Stillness - Ice Queen (15%), HM Ice enemies (2%)
            if (npc.type == NPCID.IceQueen)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ShardOfStillness>(), 7)); // ~15%
            }
            else if (IsHardmodeIceEnemy(npc.type))
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ShardOfStillness>(), 50)); // 2%
            }

            // Frozen Core - Ice Golem - 20%
            if (npc.type == NPCID.IceGolem)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FrozenCore>(), 5)); // 20%
            }

            // Icicle Coronet - Ice Queen - 10%
            if (npc.type == NPCID.IceQueen)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<IcicleCoronet>(), 10)); // 10%
            }

            // Permafrost Shard - Any HM Ice enemy - 3%
            if (IsHardmodeIceEnemy(npc.type))
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<PermafrostShard>(), 33)); // 3%
            }

            // Frost Essence - Ice Queen (100% guaranteed 3-5)
            if (npc.type == NPCID.IceQueen)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FrostEssence>(), 1, 3, 5));
            }
        }

        #region Enemy Type Helpers

        private static bool IsSurfaceEnemy(int type)
        {
            return type == NPCID.BlueSlime ||
                   type == NPCID.GreenSlime ||
                   type == NPCID.PurpleSlime ||
                   type == NPCID.Zombie ||
                   type == NPCID.DemonEye ||
                   type == NPCID.Bunny || // Corrupt/Crimson Bunny variants
                   type == NPCID.Goldfish;
        }

        private static bool IsCavernEnemy(int type)
        {
            return type == NPCID.Skeleton ||
                   type == NPCID.SkeletonArcher ||
                   type == NPCID.UndeadMiner ||
                   type == NPCID.Tim ||
                   type == NPCID.GiantBat ||
                   type == NPCID.CaveBat ||
                   type == NPCID.Salamander ||
                   type == NPCID.GiantShelly ||
                   type == NPCID.Crawdad ||
                   type == NPCID.BlackRecluse ||
                   type == NPCID.GraniteGolem ||
                   type == NPCID.GraniteFlyer;
        }

        private static bool IsHardmodeJungleEnemy(int type)
        {
            return type == NPCID.Arapaima ||
                   type == NPCID.GiantTortoise ||
                   type == NPCID.AngryTrapper ||
                   type == NPCID.Moth ||
                   type == NPCID.MossHornet ||
                   type == NPCID.FungiBulb ||
                   type == NPCID.FungoFish ||
                   type == NPCID.JungleCreeper ||
                   type == NPCID.JungleCreeperWall;
        }

        private static bool IsSolarPillarEnemy(int type)
        {
            return type == NPCID.SolarCorite ||
                   type == NPCID.SolarCrawltipedeHead ||
                   type == NPCID.SolarDrakomire ||
                   type == NPCID.SolarDrakomireRider ||
                   type == NPCID.SolarSolenian ||
                   type == NPCID.SolarSroller;
        }

        private static bool IsLavaEnemy(int type)
        {
            return type == NPCID.LavaSlime ||
                   type == NPCID.Lavabat ||
                   type == NPCID.FireImp ||
                   type == NPCID.HellArmoredBones ||
                   type == NPCID.HellArmoredBonesMace ||
                   type == NPCID.HellArmoredBonesSpikeShield ||
                   type == NPCID.HellArmoredBonesSword;
        }

        private static bool IsHellEnemy(int type)
        {
            return type == NPCID.Demon ||
                   type == NPCID.VoodooDemon ||
                   type == NPCID.FireImp ||
                   type == NPCID.Lavabat ||
                   type == NPCID.LavaSlime ||
                   type == NPCID.Hellbat ||
                   type == NPCID.RedDevil;
        }

        private static bool IsEclipseEnemy(int type)
        {
            return type == NPCID.Eyezor ||
                   type == NPCID.Frankenstein ||
                   type == NPCID.SwampThing ||
                   type == NPCID.Vampire ||
                   type == NPCID.VampireBat ||
                   type == NPCID.CreatureFromTheDeep ||
                   type == NPCID.Fritz ||
                   type == NPCID.ThePossessed ||
                   type == NPCID.Butcher ||
                   type == NPCID.DeadlySphere ||
                   type == NPCID.DrManFly ||
                   type == NPCID.Nailhead ||
                   type == NPCID.Psycho ||
                   type == NPCID.Mothron ||
                   type == NPCID.Reaper;
        }

        private static bool IsHardmodeIceEnemy(int type)
        {
            return type == NPCID.IceTortoise ||
                   type == NPCID.IcyMerman ||
                   type == NPCID.IceElemental ||
                   type == NPCID.PigronCorruption ||
                   type == NPCID.PigronCrimson ||
                   type == NPCID.PigronHallow ||
                   type == NPCID.IceGolem ||
                   type == NPCID.IceMimic ||
                   type == NPCID.ArmoredViking ||
                   type == NPCID.IceBat;
        }

        #endregion
    }
}
