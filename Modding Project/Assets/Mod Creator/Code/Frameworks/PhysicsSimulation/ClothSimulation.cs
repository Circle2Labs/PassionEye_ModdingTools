using System;
using UnityEngine;

namespace Code.Frameworks.PhysicsSimulation
{
    [Serializable]
    public struct ClothSimulation
    {
        [SerializeField]
        public bool Enabled;

        [SerializeField]
        public EPreset Preset;

        [SerializeField]
        public bool UseCustomSkinning;

        [SerializeField]
        public Transform[] SkinningBones;

        [Header("Collision Settings")]
        [SerializeField]
        public ECollisionMode CollisionMode;

        [SerializeField]
        [Range(0f, 1f)]
        public float Radius;

        [SerializeField]
        [Range(0f, 1f)]
        public float Friction;

        [SerializeField]
        public bool UseBackstop;

        [Header("Simulation Settings")]
        [SerializeField]
        public ESimulationType SimulationType;

        [Header("Bone Settings")]
        [SerializeField]
        public Transform[] RootBones;

        [SerializeField]
        public EConnectionMode ConnectionMode;

        [Header("Mesh Settings")]
        [SerializeField]
        [Range(0f, 0.2f)]
        public float Reduction;

        [SerializeField]
        public Texture2D PaintMap;

        public ClothSimulation Clone()
        {
            var clothSim = new ClothSimulation();
            clothSim.Enabled = Enabled;
            clothSim.Preset = Preset;
            clothSim.UseCustomSkinning = UseCustomSkinning;

            SkinningBones ??= Array.Empty<Transform>();

            var copiedSkinningBones = new Transform[SkinningBones.Length];
            Array.Copy(SkinningBones, copiedSkinningBones, SkinningBones.Length);

            clothSim.SkinningBones = copiedSkinningBones;
            clothSim.CollisionMode = CollisionMode;
            clothSim.Radius = Radius;
            clothSim.Friction = Friction;
            clothSim.UseBackstop = UseBackstop;
            clothSim.SimulationType = SimulationType;

            RootBones ??= Array.Empty<Transform>();

            var copiedRootBones = new Transform[RootBones.Length];
            Array.Copy(RootBones, copiedRootBones, RootBones.Length);

            clothSim.RootBones = copiedRootBones;
            clothSim.ConnectionMode = ConnectionMode;
            clothSim.Reduction = Reduction;
            clothSim.PaintMap = PaintMap;

            return clothSim;
        }
    }
}
