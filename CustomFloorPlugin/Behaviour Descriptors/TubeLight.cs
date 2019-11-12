﻿using CustomFloorPlugin.Util;
using CustomUI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Harmony;

namespace CustomFloorPlugin
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]

    public class TubeLight : MonoBehaviour
    {
        public enum LightsID
        {
            Static = 0,
            BackLights = 1,
            BigRingLights = 2,
            LeftLasers = 3,
            RightLasers = 4,
            TrackAndBottom = 5,
            Unused5 = 6,
            Unused6 = 7,
            Unused7 = 8,
            RingsRotationEffect = 9,
            RingsStepEffect = 10,
            Unused10 = 11,
            Unused11 = 12,
            RingSpeedLeft = 13,
            RingSpeedRight = 14,
            Unused14 = 15,
            Unused15 = 16
        }

        public float width = 0.5f;
        public float length = 1f;
        [Range(0, 1)]
        public float center = 0.5f;
        public Color color = Color.blue;
        public LightsID lightsID = LightsID.Static;
        private static LightWithIdManager _lightManager;
        public static LightWithIdManager lightManager
        {
            get
            {
                if (!_lightManager)
                    //_lightManager = new GameObject("CustomPlatformsLightManager").AddComponent<LightWithIdManager>();
                    _lightManager = Plugin.LightsWithId_Patch.GameLightManager;
                return _lightManager;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = color;
            Gizmos.matrix = transform.localToWorldMatrix;
            Vector3 cubeCenter = Vector3.up * (0.5f - center) * length;
            Gizmos.DrawCube(cubeCenter, new Vector3(2 * width, length, 2 * width));
        }

        // ----------------

        private TubeBloomPrePassLight tubeBloomLight;

        private void Awake()
        {
            var prefab = Resources.FindObjectsOfTypeAll<TubeBloomPrePassLight>().First(x => x.name == "Neon");

            TubeLight[] localDescriptors = GetComponentsInChildren<TubeLight>(true);

            if (localDescriptors == null) return;

            TubeLight tl = this;

            tubeBloomLight = Instantiate(prefab);
            tubeBloomLight.transform.SetParent(tl.transform);
            tubeBloomLight.transform.localRotation = Quaternion.identity;
            tubeBloomLight.transform.localPosition = Vector3.zero;
            tubeBloomLight.transform.localScale = new Vector3(1 / tl.transform.lossyScale.x, 1 / tl.transform.lossyScale.y, 1 / tl.transform.lossyScale.z);

            if (tl.GetComponent<MeshFilter>().mesh.vertexCount == 0)
            {
                tl.GetComponent<MeshRenderer>().enabled = false;
            }
            else
            {
                // swap for MeshBloomPrePassLight
                tubeBloomLight.gameObject.SetActive(false);
                MeshBloomPrePassLight meshbloom = ReflectionUtil.CopyComponent(tubeBloomLight, typeof(TubeBloomPrePassLight), typeof(MeshBloomPrePassLight), tubeBloomLight.gameObject) as MeshBloomPrePassLight;
                meshbloom.Init(tl.GetComponent<Renderer>());
                tubeBloomLight.gameObject.SetActive(true);
                DestroyImmediate(tubeBloomLight);
                tubeBloomLight = meshbloom;
            }
            tubeBloomLight.gameObject.SetActive(false);

            var lightWithId = tubeBloomLight.GetComponent<LightWithId>();
            if (lightWithId)
            {
                //lightWithId.SetPrivateField("_tubeBloomPrePassLight", tubeBloomLight);
                //var runtimeFields = typeof(LightWithId).GetTypeInfo().GetRuntimeFields();
                //runtimeFields.First(f => f.Name == "_ID").SetValue(lightWithId, (int)tl.lightsID);
                //var lightManagers = Resources.FindObjectsOfTypeAll<LightWithIdManager>();
                //lightManager = lightManagers.FirstOrDefault();

                //runtimeFields.First(f => f.Name == "_lighManager").SetValue(lightWithId, lightManager);
            }

            tubeBloomLight.SetPrivateField("_width", tl.width * 2);
            tubeBloomLight.SetPrivateField("_length", tl.length);
            tubeBloomLight.SetPrivateField("_center", tl.center);
            tubeBloomLight.SetPrivateField("_transform", tubeBloomLight.transform);
            tubeBloomLight.SetPrivateField("_maxAlpha", 0.1f);
            var parabox = tubeBloomLight.GetComponentInChildren<ParametricBoxController>();
            tubeBloomLight.SetPrivateField("_parametricBoxController", parabox);
            var parasprite = tubeBloomLight.GetComponentInChildren<Parametric3SliceSpriteController>();
            tubeBloomLight.SetPrivateField("_dynamic3SliceSprite", parasprite);
            parasprite.Init();
            parasprite.GetComponent<MeshRenderer>().enabled = false;
            tubeBloomLight.color = color * 0.9f;

            tubeBloomLight.gameObject.SetActive(true);
            tubeBloomLight.Refresh();
            //TubeLightManager.UpdateEventTubeLightList();
           
        }

        private void GameSceneLoaded()
        {
            tubeBloomLight.color = Color.black.ColorWithAlpha(0);
            tubeBloomLight.Refresh();
        }

        private void OnBeatmapEvent(BeatmapEventData obj)
        {
            int type = (int)obj.type + 1;
            if (type == (int)lightsID)
            {
                tubeBloomLight.color = lightManager.GetColorForId(type) * 0.9f;
                tubeBloomLight.Refresh();
            }
        }

        private void OnEnable()
        {
            BSEvents.menuSceneLoaded += SetColorToDefault;
            BSEvents.menuSceneLoadedFresh += SetColorToDefault;
            BSEvents.beatmapEvent += OnBeatmapEvent;
            BSEvents.gameSceneLoaded += GameSceneLoaded;
            SetColorToDefault();
        }

        private void OnDisable()
        {
            BSEvents.menuSceneLoaded -= SetColorToDefault;
            BSEvents.menuSceneLoadedFresh -= SetColorToDefault;
            BSEvents.beatmapEvent -= OnBeatmapEvent;
            BSEvents.gameSceneLoaded -= GameSceneLoaded;
        }

        private void SetColorToDefault()
        {
            tubeBloomLight.color = color * 0.9f;
            tubeBloomLight.Refresh();
        }
    }
}