using HarmonyLib;
using Planetbase;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace Tahvohck_Mods
{
    using Module = Planetbase.Module;
    using Entry = UnityModManager.ModEntry;

    public class UAAS_Main
    {
        internal static Entry.ModLogger Logger;

        public static void Load(Entry UMMData)
        {
            Logger = UMMData.Logger;

            try {
                Harmony harmony = new Harmony(typeof(UAAS_Main).FullName);
                harmony.PatchAll();
                Logger.Log("Setup complete.");
            } catch (HarmonyException hError) {
                string error =
                    $"Harmony patch failure, try enabling debug." +
                    $"\n  {hError.Message}";
                Logger.Log(error);
            } catch (Exception exGeneric) {
                Logger.Log(
                    $"Generic issue happened. Catching it instead of propagating. Contact Tahvohck." +
                    $"\n  {exGeneric.Message}"
                );
            }

            try {
                FieldInfo mtlInstance = typeof(TypeList<ModuleType, ModuleTypeList>)
                    .GetField("mInstance", BindingFlags.NonPublic | BindingFlags.Static);
                mtlInstance?.SetValue(null, null);
            } catch (Exception e) {
                Logger.Log($"{e.Message}" +
                    $"\n  {e.GetType().FullName}\n{e.StackTrace}");
            }

#if DEBUG
            RunChecks();
#endif
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


    /// <summary>
    /// Patches to be reused for all the different modules.
    /// Inherits from ModuleType to be able to access the needed bits.
    /// </summary>
    public class PatchRequirements
    {
        public static ModuleTypeRef emptyRequirement = new ModuleTypeRef();

        public static void Postfix<T>(T __instance) where T : ModuleType
        {

            try {
                var mReqField = typeof(T).GetField(
                    "mRequiredStructure",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                mReqField?.SetValue(__instance, new ModuleTypeRef());
#if DEBUG
                UAAS_Main.Logger.Log($"Removed requirements on: {__instance?.getName()}");
#endif
            } catch (Exception e) {
                UAAS_Main.Logger.Log($"Error while patching {__instance.getName()}");
                UAAS_Main.Logger.Log($"\n{e.Message}\n{e.StackTrace}");
            }
        }
    }


    #region Internal Modules
    [HarmonyPatch(typeof(ModuleTypeStorage))]
    [HarmonyPatch(MethodType.Constructor)]
    public class PatchStorageDome
    {
        public static void Postfix(ModuleTypeStorage __instance) => PatchRequirements.Postfix(__instance);
    }


    [HarmonyPatch(typeof(ModuleTypeCanteen))]
    [HarmonyPatch(MethodType.Constructor)]
    public class PatchCanteen
    {
        public static void Postfix(ModuleTypeCanteen __instance) => PatchRequirements.Postfix(__instance);
    }


    [HarmonyPatch(typeof(ModuleTypeMultiDome))]
    [HarmonyPatch(MethodType.Constructor)]
    public class PatchMultiDome
    {
        public static void Postfix(ModuleTypeMultiDome __instance) => PatchRequirements.Postfix(__instance);
    }


    [HarmonyPatch(typeof(ModuleTypeBioDome))]
    [HarmonyPatch(MethodType.Constructor)]
    public class PatchBioDome
    {
        public static void Postfix(ModuleTypeBioDome __instance) => PatchRequirements.Postfix(__instance);
    }


    [HarmonyPatch(typeof(ModuleTypeProcessingPlant))]
    [HarmonyPatch(MethodType.Constructor)]
    public class PatchProcessingPlant
    {
        public static void Postfix(ModuleTypeProcessingPlant __instance) =>
            PatchRequirements.Postfix(__instance);
    }
    #endregion


    #region External Modules
    [HarmonyPatch(typeof(ModuleTypeWindTurbine))]
    [HarmonyPatch(MethodType.Constructor)]
    public class PatchWindTurbine
    {
        public static void Postfix(ModuleTypeWindTurbine __instance) =>
            PatchRequirements.Postfix(__instance);
    }


    [HarmonyPatch(typeof(ModuleTypeLandingPad))]
    [HarmonyPatch(MethodType.Constructor)]
    public class PatchLandingPad
    {
        public static void Postfix(ModuleTypeLandingPad __instance) => PatchRequirements.Postfix(__instance);
    }


    [HarmonyPatch(typeof(ModuleTypeWaterTank))]
    [HarmonyPatch(MethodType.Constructor)]
    public class PatchWaterTank
    {
        public static void Postfix(ModuleTypeWaterTank __instance) => PatchRequirements.Postfix(__instance);
    }


    [HarmonyPatch(typeof(ModuleTypeSignpost))]
    [HarmonyPatch(MethodType.Constructor)]
    public class PatchSignpost
    {
        public static void Postfix(ModuleTypeSignpost __instance) => PatchRequirements.Postfix(__instance);
    }
    #endregion


    //[HarmonyPatch(typeof(GuiMenuItem), "areRequirementsMet")]
    //public class PatchGUIMI_areRequirementsMet
    //{
    //    internal static List<ModuleType> SeenModules = new List<ModuleType>();
    //
    //    public static void Prefix(GuiMenuItem __instance)
    //    {
    //        if (__instance.getModuleType() is null) { return; }
    //        if (SeenModules.Contains(__instance.getModuleType())) {
    //            return;
    //        } else {
    //            SeenModules.Add(__instance.getModuleType());
    //        }
    //        string item = __instance.getModuleType().getName();
    //
    //        ModuleType mAltType = __instance.getRequiredModuleType();
    //        bool isReqTypeNull = __instance.mRequiredModuleType is null;
    //        bool thisTypeIsNull = __instance.mAlternativeRequiredModuleType is null;
    //        bool isReqTypeBuilt = !isReqTypeNull && Module.isModuleTypeBuilt(__instance.mRequiredModuleType);
    //        bool thisIsBuilt = !thisTypeIsNull && Module.isModuleTypeBuilt(mAltType);
    //
    //        bool isEnabled =  isReqTypeNull || thisIsBuilt || isReqTypeBuilt;
    //
    //        UAAS_Main.Logger.Log($"Checking menu item {item,17}, {isEnabled,-5} (hasReq: {!isReqTypeNull,-5}" +
    //            $" - BuiltAlready: {thisIsBuilt,-5} - ReqBuilt: {isReqTypeBuilt,-5})");
    //    }
    //}
}
