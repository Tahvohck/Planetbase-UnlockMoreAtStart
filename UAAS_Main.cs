using Planetbase;
using System;
using System.Reflection;
using UnityModManagerNet;

namespace Tahvohck_Mods
{
    using Entry = UnityModManager.ModEntry;

    public class UAAS_Main
    {
        internal static int skippedIDX = 0;
        internal static BufferedLogger Logger;
        internal static Action<Entry, float> defaultOnUpdate;
        internal static Type[] typesToEnable = new[] {
            // Internal modules
            typeof(ModuleTypeStorage),
            typeof(ModuleTypeCanteen),
            typeof(ModuleTypeMultiDome),
            typeof(ModuleTypeBioDome),
            typeof(ModuleTypeProcessingPlant),
            typeof(ModuleTypeControlCenter),

            // External modules
            typeof(ModuleTypeWindTurbine),
            typeof(ModuleTypeLandingPad),
            typeof(ModuleTypeWaterTank),
            typeof(ModuleTypeSignpost),
        };

        [LoaderOptimization(LoaderOptimization.NotSpecified)]
        public static void Load(Entry UMMData)
        {
            Logger = new BufferedLogger(UMMData);
            defaultOnUpdate = UMMData.OnUpdate;
            TahvohckUtil.FirstUpdate += Update;
        }

        public static void RunChecks()
        {
            Logger.Buffer(
                TypeList<ModuleType, ModuleTypeList>
                .find<ModuleTypeStorage>()
                .Representation());
            Logger.Write(
                TypeList<ModuleType, ModuleTypeList>
                .find<ModuleTypeWindTurbine>()
                .Representation());
        }

        public static void Update()
        {
            if (!(GameManager.getInstance().getGameState() is GameStateLogo)) {
                skippedIDX++;
                return;
            }
            Logger.Buffer($"Skipped {skippedIDX} frames before being ready.");

            foreach (Type t in typesToEnable) {
                FieldInfo mReqField = t
                    .GetField("mRequiredStructure", BindingFlags.NonPublic | BindingFlags.Instance);
                ModuleType moduleInst = ModuleTypeList.find(t.Name);

#if DEBUG
                Logger.Buffer(
                    $"Inst is null? {moduleInst is null}\t" +
                    $"ReqField is null? {mReqField is null}\t" +
                    $"[{t?.Name.Remove(0, 10)}]");
#endif

                mReqField?.SetValue(moduleInst, new ModuleTypeRef());
            }
            Logger.Flush();

#if DEBUG
            RunChecks();
#endif
            Logger.Write("Done patching.");
        }
    }
}
