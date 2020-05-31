using HarmonyLib;
using Planetbase;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;

namespace Tahvohck_Mods
{
using Module = Planetbase.Module;
    public class UAAS_Main : IMod
    {
        public void Init()
        {
#if TRACE
            Harmony.DEBUG = true;
#endif
            ZZZ_Modhooker.PreResetEvent += Setup;
#if DEBUG
            ZZZ_Modhooker.PostResetEvent += RunChecks;
#endif
            TahvUtil.Log("Mod initialized.");
            ModuleTypeList.mInstance = new ModuleTypeList(); // Re-instantiate the canonical instance
        }

        public void Update()
        {
            //throw new NotImplementedException();
        }

        public static void Setup(object caller, EventArgs args)
        {
            try {
                Harmony harmony = new Harmony(typeof(UAAS_Main).FullName);
                harmony.PatchAll();
                TahvUtil.Log("Setup complete.");
            } catch (HarmonyException hError) {
                string error =
                    $"Harmony patch failure, try enabling debug." +
                    $"\n  {hError.Message}";
                TahvUtil.Log(error);
            } catch (Exception exGeneric) {
                TahvUtil.Log(
                    $"Generic issue happened. Catching it instead of propagating. Contact Tahvohck." +
                    $"\n  {exGeneric.Message}"
                );
            }
        }

        public static void RunChecks(object caller, EventArgs args)
        {
            ModuleType solar = new ModuleTypeSolarPanel();
            TahvUtil.Log(solar.Render());
            TahvUtil.Log(new ModuleTypeStorage().Render());
            TahvUtil.Log(new ModuleTypeAirlock().Render());
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
                $"\n  REQUIRED: {module.getRequiredModuleType()} ({module.getRequiredModuleType() is null})" +
                $"\n  REQName:  {module.mRequiredStructure.mName} ({module.mRequiredStructure.mName is null})" +
                $"\n  REQModule {module.mRequiredStructure.mModuleType} ({module.mRequiredStructure.mModuleType is null})";
            return render;
        }
    }


    /// <summary>
    /// Patches to be reused for all the different modules.
    /// </summary>
    public class PatchRequirements
    {
        public static void Postfix<T>(T __instance) where T : ModuleType
        {
            __instance.mRequiredStructure.mModuleType = null;
            __instance.mRequiredStructure.mName = null;
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


    [HarmonyPatch(typeof(GuiMenuItem), "areRequirementsMet")]
    public class PatchGUIMI_areRequirementsMet
    {
        internal static List<ModuleType> SeenModules = new List<ModuleType>();

        public static void Prefix(GuiMenuItem __instance)
        {
            if (__instance.getModuleType() is null) { return; }
            if (SeenModules.Contains(__instance.getModuleType())) {
                return;
            } else {
                SeenModules.Add(__instance.getModuleType());
            }
            string item = __instance.getModuleType().getName();

            ModuleType mAltType = __instance.mAlternativeRequiredModuleType;
            bool isReqTypeNull = __instance.mRequiredModuleType is null;
            bool thisTypeIsNull = __instance.mAlternativeRequiredModuleType is null;
            bool isReqTypeBuilt = !isReqTypeNull && Module.isModuleTypeBuilt(__instance.mRequiredModuleType);
            bool thisIsBuilt = !thisTypeIsNull && Module.isModuleTypeBuilt(mAltType);

            bool isEnabled =  isReqTypeNull || thisIsBuilt || isReqTypeBuilt;

            TahvUtil.Log($"Checking menu item {item,17}, {isEnabled,-5} (hasReq: {!isReqTypeNull,-5}" +
                $" - BuiltAlready: {thisIsBuilt,-5} - ReqBuilt: {isReqTypeBuilt,-5})");
        }
    }
}
