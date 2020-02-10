﻿using Chroma.Beatmap.Events;
using Chroma.Settings;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chroma.HarmonyPatches {

    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(TrackLaneRingsRotationEffectSpawner))]
    [HarmonyPatch("HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger")]
    class TrackLaneRingsRotationEffectSpawnerHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger {

        //Ring rotation
        static bool Prefix(TrackLaneRingsRotationEffectSpawner __instance, ref BeatmapEventData beatmapEventData, ref BeatmapEventType ____beatmapEventType, ref TrackLaneRingsRotationEffect ____trackLaneRingsRotationEffect, ref float ____rotationStep, ref float ____rotationPropagationSpeed, ref float ____rotationFlexySpeed) {

            try {
                if (beatmapEventData.type == ____beatmapEventType) {
                    if (beatmapEventData is CustomBeatmapEventData customData) {
                        dynamic dynData = customData.customData;
                        if (dynData != null) {

                            string nameFilter = Trees.at(dynData, "_nameFilter");
                            if (nameFilter != null) {
                                if (!__instance.name.ToLower().Contains(nameFilter.ToLower())) return false;
                            }

                            bool? reset = Trees.at(dynData, "_reset");
                            if (reset != null && reset == true) {
                                ChromaLogger.Log("Ring spin [RESET]");
                                ResetRings(__instance, ref ____trackLaneRingsRotationEffect, ref ____rotationStep, ref ____rotationPropagationSpeed, ref ____rotationFlexySpeed);
                                return false;
                            }

                            float? stepMult = (float?)Trees.at(dynData, "_stepMult");
                            if (stepMult == null) stepMult = 1f;
                            float? propMult = (float?)Trees.at(dynData, "_propMult");
                            if (propMult == null) propMult = 1f;
                            float? speedMult = (float?)Trees.at(dynData, "_speedMult");
                            if (speedMult == null) speedMult = 1f;

                            int? dir = (int?)Trees.at(dynData, "_direction");
                            if (dir == null) dir = -1;

                            bool rotRight;
                            if (dir == -1) {
                                rotRight = UnityEngine.Random.value < 0.5f;
                            }
                            else rotRight = dir == 1 ? true : false;

                            bool? counterSpin = Trees.at(dynData, "_counterSpin");
                            if (counterSpin != null && counterSpin == true) {
                                if (__instance.name.ToLower().Contains("small")) {
                                    rotRight = !rotRight;
                                }
                            }

                            if (ChromaConfig.DebugMode) ChromaLogger.Log("[[CJD]] Ring Spin (" + __instance.name + "_" + beatmapEventData.time + ") - [Dir:" + (dir == 0 ? "random" : rotRight ? "right" : "left") + "] - [Step:" + stepMult + "] - [Prop:" + propMult + "] - [Speed:" + speedMult + "]");

                            TriggerRotation(rotRight, __instance, ref ____trackLaneRingsRotationEffect, ref ____rotationStep, ref ____rotationPropagationSpeed, ref ____rotationFlexySpeed, (float)stepMult, (float)propMult, (float)speedMult);
                            return false;
                        }
                    }
                }
            }
            catch (Exception e) {
                ChromaLogger.Log("INVALID _customData", ChromaLogger.Level.WARNING);
                ChromaLogger.Log(e);
            }

            return true;
        }

        private static void TriggerRotation(bool rotRight, TrackLaneRingsRotationEffectSpawner __instance, ref TrackLaneRingsRotationEffect ____trackLaneRingsRotationEffect, ref float ____rotationStep, ref float ____rotationPropagationSpeed, ref float ____rotationFlexySpeed, float ringStepMult = 1f, float ringPropagationMult = 1f, float ringSpeedMult = 1f) {
            ____trackLaneRingsRotationEffect.AddRingRotationEffect(____trackLaneRingsRotationEffect.GetFirstRingDestinationRotationAngle() + (float)(90 * (rotRight ? -1 : 1)), UnityEngine.Random.Range(0f, ____rotationStep * ringStepMult), ____rotationPropagationSpeed * ringPropagationMult, ____rotationFlexySpeed * ringSpeedMult);
        }

        private static void ResetRings(TrackLaneRingsRotationEffectSpawner __instance, ref TrackLaneRingsRotationEffect ____trackLaneRingsRotationEffect, ref float ____rotationStep, ref float ____rotationPropagationSpeed, ref float ____rotationFlexySpeed) {
            TriggerRotation(UnityEngine.Random.value < 0.5f, __instance, ref ____trackLaneRingsRotationEffect, ref ____rotationStep, ref ____rotationPropagationSpeed, ref ____rotationFlexySpeed, 0f, 116f, 116f);
        }

    }

}