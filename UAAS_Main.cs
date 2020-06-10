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
        internal static Entry.ModLogger Logger;
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
            Logger = UMMData.Logger;
            defaultOnUpdate = UMMData.OnUpdate;
            UMMData.OnUpdate = Update;
        }

        public static void RunChecks()
        {
            Logger.Log(
                TypeList<ModuleType, ModuleTypeList>
                .find<ModuleTypeStorage>()
                .Render());
            Logger.Log(
                TypeList<ModuleType, ModuleTypeList>
                .find<ModuleTypeWindTurbine>()
                .Render());
        }

        public static void Update(Entry UMMData, float tDelta)
        {
            if (!(GameManager.getInstance().getGameState() is GameStateLogo)) {
                skippedIDX++;
                return;
            }
            Logger.Log($"Skipped {skippedIDX} frames before being ready.");

            foreach (Type t in typesToEnable) {
                FieldInfo mReqField = t
                    .GetField("mRequiredStructure", BindingFlags.NonPublic | BindingFlags.Instance);
                ModuleType moduleInst = ModuleTypeList.find(t.Name);

#if DEBUG
                Logger.Log(
                    $"Inst is null? {moduleInst is null}\t" +
                    $"ReqField is null? {mReqField is null}\t" +
                    $"[{t?.Name.Remove(0, 10)}]");
#endif

                mReqField?.SetValue(moduleInst, new ModuleTypeRef());
            }

#if DEBUG
            RunChecks();
#endif
            Logger.Log("Done patching.");
            UMMData.OnUpdate = defaultOnUpdate;
        }
    }


    public static class Extensions
    {
        public static string Render(this ModuleType module)
        {
            int powGen = module.getPowerGeneration(1, Planet.Quantity.High, Planet.Quantity.High);
            int powStore = module.getPowerStorageCapacity(1);
            int powCollected = module.getPowerCollection(1);
            int sMax, sMin;
            sMax = module.getMaxSize();
            sMin = module.getMinSize();

            string render =
                $"\n  NAME:     {module.getName()}" +
                $"\n  SizeMax:  {sMax,2}\tSizeMin:  {sMin,2}\tHeight:  {module.getHeight(),4}" +
                $"\n  POW_COL:  {powCollected}\tPOW_GEN:  {powGen}\tPOW_STO:   {powStore}" +
                $"\n  UsersMax: {module.getMaxUsers()}" +
                $"\n  O2_s1:    {module.getOxygenGeneration(1f)}" +
                $"\n  REQUIRED: {module.getRequiredModuleType()} ({module.getRequiredModuleType() is null})";
            return render;
        }
    }
}
