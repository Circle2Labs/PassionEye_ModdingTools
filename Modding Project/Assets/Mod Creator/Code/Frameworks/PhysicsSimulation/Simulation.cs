using Code.Tools;
using System;
using UnityEngine;

namespace Code.Frameworks.PhysicsSimulation
{
    [Serializable]
    public struct Simulation
    {
        [SerializeField]
        [Tooltip("Give a name to this simulation. Useful when running multiple simulation on a single object.")]
        public string Name;

        [SerializeField]
        [Tooltip("Enable or disable the simulation.")]
        public bool Enabled;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Ratio between the current animation posture and initial posture which will be considered for some calculations.")]
        public float AnimationPoseRatio;

        [SerializeField]
        [Tooltip("Not yet implemented. Coming soon!")]
        public EPreset Preset;

        [SerializeField]
        [Tooltip("The simulation mesh can be skinned automatically against these bones so it follows any movement of relevant body parts.")]
        public Transform[] SkinningBones;

        [Header("Collision Settings")]
        [SerializeField]
        [Tooltip("Vertex mode: Collision happens only on vertices using the radius parameter. Edge mode: Additional collision on edges.")]
        public ECollisionMode CollisionMode;
        
        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Collision radius around item vertices.")]
        public float Radius;

        [SerializeField]
        [BoundedCurve(0f,1f)]
        [Tooltip("Apply 'Radius' using curve values.")]
        public AnimationCurve RadiusCurve;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Friction between a collider and a vertex when they come into contact. Think of it as how 'slippery' the collider is during contact.")]
        public float Friction;

        [Header("Advanced Collision Settings")]
        [SerializeField]
        [Tooltip("Sets up a collision sphere around the vertex using this value, preventing the vertex from leaving it. Set to 0 to disable.")]
        public float MaxDistanceRadius;

        [SerializeField]
        [BoundedCurve(0f, 1f)]
        [Tooltip("Apply 'MaxDistanceRadius' using curve values.")]
        public AnimationCurve MaxDistanceRadiusCurve;

        [SerializeField]
        [Tooltip("Radially adjusts the normals of the simulation mesh around the transform. Backstop needs correctly setup normals to work. Please refer to the docs for a detailed explanation.")]
        public Transform BackstopNormalAlignment;

        [SerializeField]
        [Tooltip("The distance from the original vertex position to the outer edge of the backstop collision sphere.")]
        public float BackstopDistance;

        [SerializeField]
        [BoundedCurve(0f, 1f)]
        [Tooltip("Apply 'BackstopDistance' using curve values.")]
        public AnimationCurve BackstopDistanceCurve;

        [SerializeField]
        [Tooltip("Setup a collision sphere behind each vertex. The vertex will not be able to enter it.")]
        public float BackstopRadius;

        [SerializeField]
        [Tooltip("Repulsion strength. Lowering the value will make vertices move away softer from the backstop collision sphere.")]
        public float BackstopStiffness;

        [Header("Force Settings")]
        [SerializeField]
        [Range(0f, 10f)]
        [Tooltip("Gravitational pull on the item.")]
        public float Gravity;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Damping is the rate at which applied force to the item will lose its energy. Can be considered 'Air Resistance'.")]
        public float Damping;

        [SerializeField]
        [BoundedCurve(0f, 1f)]
        [Tooltip("Apply 'Damping' using curve values.")]
        public AnimationCurve DampingCurve;

        [Header("Angle Settings")]
        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Stiffness is the resistance of the item to bending on its edges. The higher the stiffness value, the faster the original rotation on edges is being restored.")]
        public float Stiffness;

        [SerializeField]
        [BoundedCurve(1f, 1f)]
        [Tooltip("Apply 'Stiffness' using curve values.")]
        public AnimationCurve StiffnessCurve;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Used together with stiffness to limit item movement. Simular to 'Damping', attenuation adds a resistance to the effects of the stiffness parameter making it more natural looking.")]
        public float VelocityAttenuation;

        [Header("Shape Settings")]
        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Also 'Stretchtiness'. How much each vertex keeps its distance from other connected vertices. Set it to 1 to have no strech.")]
        public float Rigidness;

        [SerializeField]
        [BoundedCurve(0f, 1f)]
        [Tooltip("Apply 'Rigidness' using curve values.")]
        public AnimationCurve RigidnessCurve;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("How much the item can compress. Limits the distance on how much item vertices can move towards the baseline of the item.")]
        public float Tether;

        [Header("Inertia Settings")]
        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("How much effect movement through world space (changes to position) has on the item.")]
        public float WorldInertia;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("How much effect internal movements (such as animations) have on the item.")]
        public float LocalInertia;

        [Header("Simulation Type")]
        [SerializeField]
        [Tooltip("Bone Cloth/Bone Spring: Simulation is based on bones. Mesh Cloth: Simulation is based on vertices.")]
        public ESimulationType SimulationType;

        [Header("Bone Cloth Settings")]
        [SerializeField]
        [Tooltip("Root bones of the simulation. The item will be simulated based on these bones.")]
        public Transform[] RootBones;

        [SerializeField]
        [Tooltip("Connection mode between bones. Will determine the type of proxy mesh constructed. See docs")]
        public EConnectionMode ConnectionMode;

        [Header("Mesh Cloth Settings")]
        [SerializeField]
        [Range(0f, 0.2f)]
        [Tooltip("Reduction of the mesh cloth simulation. The higher the value, the more the proxy mesh will be reduced.")]
        public float Reduction;

        [SerializeField]
        [Tooltip("Paint map to determine what parts of the item are fixed or flexible.")]
        public Texture2D PaintMap;

        public Simulation Clone()
        {
            var simulation = new Simulation();
            simulation.Name = Name;
            simulation.Enabled = Enabled;
            simulation.AnimationPoseRatio = AnimationPoseRatio;
            simulation.Preset = Preset;

            SkinningBones ??= Array.Empty<Transform>();

            var copiedSkinningBones = new Transform[SkinningBones.Length];
            Array.Copy(SkinningBones, copiedSkinningBones, SkinningBones.Length);

            simulation.SkinningBones = copiedSkinningBones;

            simulation.CollisionMode = CollisionMode;
            simulation.Radius = Radius;
            simulation.Friction = Friction;

            simulation.MaxDistanceRadius = MaxDistanceRadius;
            simulation.BackstopNormalAlignment = BackstopNormalAlignment;
            simulation.BackstopDistance = BackstopDistance;
            simulation.BackstopRadius = BackstopRadius;
            simulation.BackstopStiffness = BackstopStiffness;

            simulation.Gravity = Gravity;
            simulation.Damping = Damping;

            simulation.Stiffness = Stiffness;
            simulation.VelocityAttenuation = VelocityAttenuation;
            simulation.Rigidness = Rigidness;
            simulation.Tether = Tether;

            simulation.WorldInertia = WorldInertia;
            simulation.LocalInertia = LocalInertia;

            simulation.SimulationType = SimulationType;

            RootBones ??= Array.Empty<Transform>();

            var copiedRootBones = new Transform[RootBones.Length];
            Array.Copy(RootBones, copiedRootBones, RootBones.Length);

            simulation.RootBones = copiedRootBones;
            simulation.ConnectionMode = ConnectionMode;

            simulation.Reduction = Reduction;
            simulation.PaintMap = PaintMap;

            return simulation;
        }
        
        public static Simulation Default()
        {
            var simulation = new Simulation();

            simulation.Name = "New Simulation";
            simulation.Enabled = true;
            simulation.AnimationPoseRatio = 0;
            simulation.Preset = EPreset.None;

            simulation.SkinningBones = Array.Empty<Transform>();

            simulation.CollisionMode = ECollisionMode.Edge;
            simulation.Radius = 0.004f;
            simulation.RadiusCurve = null;
            simulation.Friction = 0.02f;

            simulation.MaxDistanceRadius = 0;
            simulation.BackstopNormalAlignment = null;
            simulation.BackstopDistance = 0;
            simulation.BackstopDistanceCurve = null;
            simulation.BackstopRadius = 2f;
            simulation.BackstopStiffness = 1;
            

            simulation.Gravity = 5;
            simulation.Damping = 0.1f;
            simulation.DampingCurve = null;

            simulation.Stiffness = 0.2f;
            var curve = new AnimationCurve(new Keyframe(0, 1, -0.805f, -0.805f, 0, 0.3334f), new Keyframe(1, 0.2f, -0.805f, -0.805f, 0.3334f, 0));
            curve.preWrapMode = WrapMode.ClampForever;
            curve.postWrapMode = WrapMode.ClampForever;
            simulation.StiffnessCurve = curve;
            simulation.VelocityAttenuation = 0.8f;

            simulation.Rigidness = 1f;
            simulation.RigidnessCurve = null;
            simulation.Tether = 0.8f;

            simulation.WorldInertia = 1;
            simulation.LocalInertia = 1;

            simulation.SimulationType = ESimulationType.BoneCloth;

            simulation.RootBones = Array.Empty<Transform>();
            simulation.ConnectionMode = EConnectionMode.AutomaticMesh;

            simulation.Reduction = 0f;
            simulation.PaintMap = null;

            return simulation;
        }
    }
}
