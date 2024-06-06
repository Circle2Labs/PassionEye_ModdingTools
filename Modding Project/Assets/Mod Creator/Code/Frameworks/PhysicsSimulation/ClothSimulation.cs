using System;
using UnityEngine;

namespace Code.Frameworks.PhysicsSimulation
{
    [Serializable]
    public struct ClothSimulation
    {
        [SerializeField]
        [Tooltip("Enable or disable the cloth simulation.")]
        public string Name;

        [SerializeField]
        [Tooltip("Enable or disable the cloth simulation.")]
        public bool Enabled;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Ratio between animation and pose which will be considered during calculations on angle and shape related settings.")]
        public float AnimationPoseRatio;

        [SerializeField]
        [Tooltip("Coming soon :)")]
        public EPreset Preset;

        [SerializeField]
        [Tooltip("Add all bones of the base mesh that are skinned to the cloth. The proxy mesh will be skinned against these bones so it follows any movement of relevant body parts.")]
        public Transform[] SkinningBones;

        [Header("Collision Settings")]
        [SerializeField]
        [Tooltip("Vertex mode: Collision happens only on vertices using the radius parameter. Edge mode: Additional collision on edges.")]
        public ECollisionMode CollisionMode;
        
        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Collision radius around cloth vertices.")]
        public float Radius;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Friction between a collider and a vertex when they come into contact. Think of it as how 'slippery' the collider is during contact.")]
        public float Friction;

        [Header("Advanced Collision Settings")]
        [SerializeField]
        [Tooltip("Sets up a collision sphere around the vertex using this value, preventing the vertex from leaving it. Set to 0 to disable.")]
        public float MaxDistanceRadius;

        [SerializeField]
        [Tooltip("Radially adjusts the normals of the proxy mesh around the transform. Backstop needs correctly setup normals to work. Please refer to the docs for a detailed explanation.")]
        public Transform BackstopNormalAlignment;

        [SerializeField]
        [Tooltip("The distance from the original vertex position to the outer edge of the backstop collider. Direction is the inverted vertex normal. BackstopDistance + BackstopRadius = Center of backstop collision sphere. Set to 0 to disable backstop.")]
        public float BackstopDistance;

        [SerializeField]
        [Tooltip("Radius of the backstop collision sphere. The vertex will not be able to enter this sphere.  BackstopDistance + BackstopRadius = Center of backstop collision sphere. Set to 0 to disable backstop.")]
        public float BackstopRadius;

        [SerializeField]
        [Tooltip("Single value. No support for curves yet. Better leave it at one for now.")]
        public float BackstopStiffness;

        [Header("Force Settings")]
        [SerializeField]
        [Range(0f, 10f)]
        [Tooltip("Gravitational pull on the clothing.")]
        public float Gravity;

        [Range(0f, 1f)]
        [SerializeField]
        [Tooltip("Damping is the rate at which applied force to the clothing will lose its energy. Can be considered 'Air Resistance'.")]
        public float Damping;

        [Header("Angle Settings")]
        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Stiffness is the resistance of the cloth to bending on its edges. The higher the stiffness value, the faster the original rotation on edges is being restored.")]
        public float Stiffness;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Used together with stiffness to limit cloth movement. Simular to 'Damping', attenuation adds a resistance to the effects of the stiffness parameter making it more natural looking.")]
        public float VelocityAttenuation;

        [Header("Shape Settings")]
        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Also 'Stretchtiness'. How much each vertex keeps its distance from other connected vertices. Set it to 1 to have no strech.")]
        public float Rigidness;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("How much the cloth can compress. Limits the distance on how much clothing vertices can move towards the baseline of the cloth.")]
        public float Tether;

        [Header("Inertia Settings")]
        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("How much effect movement through world space (changes to position) has on the cloth.")]
        public float WorldInertia;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("How much effect internal movements (such as animations) have on the cloth.")]
        public float LocalInertia;

        [Header("Simulation Type")]
        [SerializeField]
        [Tooltip("Bone Cloth: Cloth simulation is based on bones. Mesh Cloth: Cloth simulation is based on vertices.")]
        public ESimulationType SimulationType;

        [Header("Bone Cloth Settings")]
        [SerializeField]
        [Tooltip("Root bones of the cloth simulation. The cloth will be simulated based on these bones.")]
        public Transform[] RootBones;

        [SerializeField]
        [Tooltip("Connection mode between bones. Will determine the type of proxy mesh constructed.")]
        public EConnectionMode ConnectionMode;

        [Header("Mesh Cloth Settings")]
        [SerializeField]
        [Range(0f, 0.2f)]
        [Tooltip("Reduction of the mesh cloth simulation. The higher the value, the more the proxy mesh will be reduced.")]
        public float Reduction;

        [SerializeField]
        [Tooltip("Paint map to determine what parts of the cloth are static or flexible.")]
        public Texture2D PaintMap;

        public ClothSimulation Clone()
        {
            var clothSim = new ClothSimulation();
            clothSim.Name = Name;
            clothSim.Enabled = Enabled;
            clothSim.AnimationPoseRatio = AnimationPoseRatio;
            clothSim.Preset = Preset;

            SkinningBones ??= Array.Empty<Transform>();

            var copiedSkinningBones = new Transform[SkinningBones.Length];
            Array.Copy(SkinningBones, copiedSkinningBones, SkinningBones.Length);

            clothSim.SkinningBones = copiedSkinningBones;

            clothSim.CollisionMode = CollisionMode;
            clothSim.Radius = Radius;
            clothSim.Friction = Friction;

            clothSim.MaxDistanceRadius = MaxDistanceRadius;
            clothSim.BackstopNormalAlignment = BackstopNormalAlignment;
            clothSim.BackstopDistance = BackstopDistance;
            clothSim.BackstopRadius = BackstopRadius;
            clothSim.BackstopStiffness = BackstopStiffness;

            clothSim.Gravity = Gravity;
            clothSim.Damping = Damping;

            clothSim.Stiffness = Stiffness;
            clothSim.VelocityAttenuation = VelocityAttenuation;
            clothSim.Rigidness = Rigidness;
            clothSim.Tether = Tether;

            clothSim.WorldInertia = WorldInertia;
            clothSim.LocalInertia = LocalInertia;

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
        
        public static ClothSimulation Default()
        {
            var clothSimulation = new ClothSimulation();

            clothSimulation.Name = "New ClothSimulation";
            clothSimulation.Enabled = true;
            clothSimulation.AnimationPoseRatio = 0;
            clothSimulation.Preset = EPreset.None;

            clothSimulation.SkinningBones = Array.Empty<Transform>();

            clothSimulation.CollisionMode = ECollisionMode.Edge;
            clothSimulation.Radius = 0.004f;
            clothSimulation.Friction = 0.02f;

            clothSimulation.MaxDistanceRadius = 0;
            clothSimulation.BackstopNormalAlignment = null;
            clothSimulation.BackstopDistance = 0;
            clothSimulation.BackstopRadius = 2f;
            clothSimulation.BackstopStiffness = 1;

            clothSimulation.Gravity = 5;
            clothSimulation.Damping = 0.1f;

            clothSimulation.Stiffness = 0.2f;
            clothSimulation.VelocityAttenuation = 0.8f;

            clothSimulation.Rigidness = 1f;
            clothSimulation.Tether = 0.8f;

            clothSimulation.WorldInertia = 1;
            clothSimulation.LocalInertia = 1;

            clothSimulation.SimulationType = ESimulationType.BoneCloth;

            clothSimulation.RootBones = Array.Empty<Transform>();
            clothSimulation.ConnectionMode = EConnectionMode.AutomaticMesh;

            clothSimulation.Reduction = 0f;
            clothSimulation.PaintMap = null;

            return clothSimulation;
        }
    }
}
