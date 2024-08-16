using Code.Tools;
using System;
using UnityEngine;

namespace Code.Frameworks.PhysicsSimulation
{
    [Serializable]
    public struct SimulationData
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

        [Header("Collision Settings")]
        [SerializeField]
        [Tooltip("Vertex mode: Collision happens only on vertices using the radius parameter. Edge mode: Additional collision on edges.")]
        public ECollisionMode CollisionMode;
        
        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Collision radius around item vertices.")]
        public float Radius;

        [SerializeField]
        [BoundedCurve(1f,1f)]
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
        [BoundedCurve(1f, 1f)]
        [Tooltip("Apply 'MaxDistanceRadius' using curve values.")]
        public AnimationCurve MaxDistanceRadiusCurve;

        [SerializeField]
        [Tooltip("Radially adjusts the normals of the simulation mesh around the transform. Backstop needs correctly setup normals to work. Please refer to the docs for a detailed explanation.")]
        public string BackstopNormalAlignment;

        [SerializeField]
        [Tooltip("The distance from the original vertex position to the outer edge of the backstop collision sphere.")]
        public float BackstopDistance;

        [SerializeField]
        [BoundedCurve(1f, 1f)]
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
        [Tooltip("Gravitational pull on the item. Ignored for Bone Spring.")]
        public float Gravity;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Damping is the rate at which applied force to the item will lose its energy. Can be considered 'Air Resistance'.")]
        public float Damping;

        [SerializeField]
        [BoundedCurve(1f, 1f)]
        [Tooltip("Apply 'Damping' using curve values.")]
        public AnimationCurve DampingCurve;

        [Header("Angle Settings")]
        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Stiffness is the resistance of the item to bending on its edges. The higher the stiffness value, the faster the original rotation is being restored.")]
        public float Stiffness;

        [SerializeField]
        [BoundedCurve(1f, 1f)]
        [Tooltip("Apply 'Stiffness' using curve values.")]
        public AnimationCurve StiffnessCurve;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Used together with stiffness to limit item movement. Simular to 'Damping', attenuation adds a resistance to the effects of the stiffness parameter. Lower values makes the effect springy, higher values relaxed.")]
        public float VelocityAttenuation;

        [SerializeField]
        [Range(0f, 180f)]
        [Tooltip("Limit the bending of edges to a maximum angle value.")]
        public float AngleLimit;

        [SerializeField]
        [BoundedCurve(1f, 1f)]
        [Tooltip("Apply 'AngleLimit' using curve values.")]
        public AnimationCurve AngleLimitCurve;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Repulsion strengh after reaching the defined AngleLimit. Lowering the value makes for a softer return.")]
        public float AngleLimitStiffness;

        [Header("Shape Settings")]
        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Also 'Stretchtiness'. How much each vertex keeps its distance from other connected vertices. Set it to 1 to have no strech.")]
        public float Rigidness;

        [SerializeField]
        [BoundedCurve(1f, 1f)]
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

        [Header("Bone Cloth / Mesh Cloth Settings")]
        [SerializeField]
        [Tooltip("The simulation mesh can be skinned automatically against these bones so it follows any movement of relevant body parts. Not used for Bone Spring.")]
        public string[] SkinningBones;

        [Header("Bone Cloth / Bone Spring Settings")]
        [SerializeField]
        [Tooltip("Root bones of the simulation. The item will be simulated based on these bones.")]
        public string[] RootBones;

        [Header("Mesh Cloth Exclusive Settings")]
        [SerializeField]
        [Range(0f, 0.2f)]
        [Tooltip("Reduction of the mesh cloth simulation. The higher the value, the more the proxy mesh will be reduced.")]
        public float Reduction;

        [SerializeField]
        [Tooltip("Paint map to determine what parts of the item are fixed or flexible.")]
        public Texture2D PaintMap;

        [Header("Bone Cloth Exclusive Settings")]
        [SerializeField]
        [Tooltip("Connection mode between bones. Will determine the type of proxy mesh constructed. Always 'Line' for Bone Spring. See docs")]
        public EConnectionMode ConnectionMode;

        [Header("Bone Spring Exclusive Settings")]
        [SerializeField]
        [Range(0f, 0.2f)]
        [Tooltip("How strong the spring should be. Lower numbers leads to more jiggle. 0.01 (Soft) | 0.03 (Medium) | 0.06 (Hard).")]
        public float SpringStrength;

        [SerializeField]
        [Range(0f, 0.5f)]
        [Tooltip("Limit the maximum distance bones can move. Works nicely with LocalInertia!")]
        public float SpringDistance;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Ratio of movement relative to the bone normal direction. Great to mimic elliptic movement.")]
        public float SpringNormalDistanceRatio;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Add noise, desyncing multiple springs handled by this simulation.")]
        public float SpringNoise;

        public SimulationData Copy()
        {
            var simulation = new SimulationData();
            simulation.Name = Name;
            simulation.Enabled = Enabled;
            simulation.AnimationPoseRatio = AnimationPoseRatio;
            simulation.Preset = Preset;

            if (SkinningBones == null)
            {
                simulation.SkinningBones = Array.Empty<string>();
            }
            else
            {
                var skinningBonesCopy = new string[SkinningBones.Length];
                Array.Copy(SkinningBones, skinningBonesCopy, SkinningBones.Length);
                simulation.SkinningBones = skinningBonesCopy;
            }

            simulation.CollisionMode = CollisionMode;
            simulation.Radius = Radius;
            simulation.RadiusCurve = new AnimationCurve();
            if (RadiusCurve != null)
                simulation.RadiusCurve.CopyFrom(RadiusCurve);
            simulation.Friction = Friction;

            simulation.MaxDistanceRadius = MaxDistanceRadius;
            simulation.BackstopNormalAlignment = BackstopNormalAlignment;
            simulation.BackstopDistance = BackstopDistance;
            simulation.BackstopDistanceCurve = new AnimationCurve();
            if (BackstopDistanceCurve != null)
                simulation.BackstopDistanceCurve.CopyFrom(BackstopDistanceCurve);
            simulation.BackstopRadius = BackstopRadius;
            simulation.BackstopStiffness = BackstopStiffness;

            simulation.Gravity = Gravity;
            simulation.Damping = Damping;
            simulation.DampingCurve = new AnimationCurve();
            if (DampingCurve != null)
                simulation.DampingCurve.CopyFrom(DampingCurve);

            simulation.Stiffness = Stiffness;
            simulation.StiffnessCurve = new AnimationCurve();
            if (StiffnessCurve != null)
                simulation.StiffnessCurve.CopyFrom(StiffnessCurve);
            simulation.VelocityAttenuation = VelocityAttenuation;

            simulation.AngleLimit = AngleLimit;
            simulation.AngleLimitCurve = new AnimationCurve();
            if (AngleLimitCurve != null) 
                simulation.AngleLimitCurve.CopyFrom(AngleLimitCurve);
            simulation.AngleLimitStiffness = AngleLimitStiffness;

            simulation.Rigidness = Rigidness;
            simulation.RigidnessCurve = new AnimationCurve();
            if (RigidnessCurve != null)
                simulation.RigidnessCurve.CopyFrom(RigidnessCurve);
            simulation.Tether = Tether;


            simulation.WorldInertia = WorldInertia;
            simulation.LocalInertia = LocalInertia;

            simulation.SimulationType = SimulationType;

            if (RootBones == null)
            {
                simulation.RootBones = Array.Empty<string>();
            }
            else
            {
                var rootBonesCopy = new string[RootBones.Length];
                Array.Copy(RootBones, rootBonesCopy, RootBones.Length);
                simulation.RootBones = rootBonesCopy;
            }

            simulation.Reduction = Reduction;
            simulation.PaintMap = PaintMap;

            simulation.ConnectionMode = ConnectionMode;

            simulation.SpringStrength = SpringStrength;
            simulation.SpringDistance = SpringDistance;
            simulation.SpringNormalDistanceRatio = SpringNormalDistanceRatio;
            simulation.SpringNoise = SpringNoise;

            return simulation;
        }

        #region Presets
        public static SimulationData Create(EPreset preset)
        {
            return preset switch
            {
                EPreset.None => Default(),
                EPreset.TailBoneSpring => Tail_BoneSpring(),
                EPreset.Skirt => Skirt(),
                EPreset.Breasts => Breasts(),
                EPreset.Cape => Cape(),
                EPreset.Necktie => Necktie(),
                EPreset.HairStrains => Hair_Strains(),
                _ => Default(),
            };
        }

        public static SimulationData Default()
        {
            var simulation = new SimulationData();

            simulation.Name = "Default";
            simulation.Enabled = true;
            simulation.AnimationPoseRatio = 0;
            simulation.Preset = EPreset.None;

            simulation.SkinningBones = Array.Empty<string>();

            simulation.CollisionMode = ECollisionMode.Edge;
            simulation.Radius = 0.004f;
            simulation.RadiusCurve = new AnimationCurve();
            simulation.Friction = 0.02f;

            simulation.MaxDistanceRadius = 0;
            simulation.BackstopNormalAlignment = null;
            simulation.BackstopDistance = 0;
            simulation.BackstopDistanceCurve = new AnimationCurve();
            simulation.BackstopRadius = 2f;
            simulation.BackstopStiffness = 1;

            simulation.Gravity = 5;
            simulation.Damping = 0.1f;
            simulation.DampingCurve = new AnimationCurve();

            simulation.Stiffness = 0.2f;
            var curve = new AnimationCurve(new Keyframe(0, 1, -0.8f, -0.8f, 0, 0.3334f), new Keyframe(1, 0.2f, -0.8f, -0.8f, 0.3334f, 0));
            curve.preWrapMode = WrapMode.ClampForever;
            curve.postWrapMode = WrapMode.ClampForever;
            simulation.StiffnessCurve = curve;
            simulation.VelocityAttenuation = 0.8f;
            simulation.AngleLimit = 60f;
            curve = new AnimationCurve(new Keyframe(0, 0.2f, 0.8f, 0.8f, 0, 0.3334f), new Keyframe(1, 1, 0.8f, 0.8f, 0.3334f, 0));
            curve.preWrapMode = WrapMode.ClampForever;
            curve.postWrapMode = WrapMode.ClampForever;
            simulation.AngleLimitCurve = curve;
            simulation.AngleLimitStiffness = 0.5f;

            simulation.Rigidness = 1f;
            simulation.RigidnessCurve = new AnimationCurve();
            simulation.Tether = 0.8f;

            simulation.WorldInertia = 1;
            simulation.LocalInertia = 1;

            simulation.SimulationType = ESimulationType.BoneCloth;

            simulation.RootBones = Array.Empty<string>();
            simulation.ConnectionMode = EConnectionMode.AutomaticMesh;

            simulation.SpringStrength = 0;
            simulation.SpringDistance = 0;
            simulation.SpringNormalDistanceRatio = 0;
            simulation.SpringNoise = 0;

            simulation.Reduction = 0;
            simulation.PaintMap = null;

            return simulation;
        }

        public static SimulationData Tail_BoneSpring()
        {
            var simulation = new SimulationData();

            simulation.Name = "Tail (Spring)";
            simulation.Enabled = true;
            simulation.AnimationPoseRatio = 0;
            simulation.Preset = EPreset.None;

            simulation.SkinningBones = Array.Empty<string>();

            simulation.CollisionMode = ECollisionMode.None;
            simulation.Radius = 0;
            simulation.RadiusCurve = new AnimationCurve();
            simulation.Friction = 0;

            simulation.MaxDistanceRadius = 0;
            simulation.BackstopNormalAlignment = null;
            simulation.BackstopDistance = 0;
            simulation.BackstopDistanceCurve = new AnimationCurve();
            simulation.BackstopRadius = 0;
            simulation.BackstopStiffness = 0;

            simulation.Gravity = 0;
            simulation.Damping = 0.3f;
            simulation.DampingCurve = new AnimationCurve();

            simulation.Stiffness = 0.2f;
            var curve = new AnimationCurve(new Keyframe(0, 1, -0.8f, -0.805f, 0, 0.3334f), new Keyframe(1, 0.2f, -0.8f, -0.8f, 0.3334f, 0));
            curve.preWrapMode = WrapMode.ClampForever;
            curve.postWrapMode = WrapMode.ClampForever;
            simulation.StiffnessCurve = curve;
            simulation.VelocityAttenuation = 0.8f;
            simulation.AngleLimit = 15;
            curve = new AnimationCurve(new Keyframe(0, 0.2f, 0.8f, 0.8f, 0, 0.3334f), new Keyframe(1, 1, 0.8f, 0.8f, 0.3334f, 0));
            curve.preWrapMode = WrapMode.ClampForever;
            curve.postWrapMode = WrapMode.ClampForever;
            simulation.AngleLimitCurve = curve;
            simulation.AngleLimitStiffness = 0.5f;

            simulation.Rigidness = 0;
            simulation.RigidnessCurve = new AnimationCurve();
            simulation.Tether = 0;

            simulation.WorldInertia = 1;
            simulation.LocalInertia = 0.8f;

            simulation.SimulationType = ESimulationType.BoneSpring;

            simulation.RootBones = Array.Empty<string>();
            simulation.ConnectionMode = EConnectionMode.Line;

            simulation.SpringStrength = 0.1f;
            simulation.SpringDistance = 0.05f;
            simulation.SpringNormalDistanceRatio = 1f;
            simulation.SpringNoise = 0;

            simulation.Reduction = 0;
            simulation.PaintMap = null;

            return simulation;
        }

        public static SimulationData Skirt()
        {
            var simulation = new SimulationData();

            simulation.Name = "Skirt";
            simulation.Enabled = true;
            simulation.AnimationPoseRatio = 0;
            simulation.Preset = EPreset.None;

            simulation.SkinningBones = Array.Empty<string>();

            simulation.CollisionMode = ECollisionMode.Edge;
            simulation.Radius = 0.004f;
            simulation.RadiusCurve = new AnimationCurve();
            simulation.Friction = 0.02f;

            simulation.MaxDistanceRadius = 0;
            simulation.BackstopNormalAlignment = null;
            simulation.BackstopDistance = 0;
            simulation.BackstopDistanceCurve = new AnimationCurve();
            simulation.BackstopRadius = 2f;
            simulation.BackstopStiffness = 1;

            simulation.Gravity = 5;
            simulation.Damping = 0.1f;
            simulation.DampingCurve = new AnimationCurve();

            simulation.Stiffness = 0.2f;
            var curve = new AnimationCurve(new Keyframe(0, 1, -0.8f, -0.8f, 0, 0.3334f), new Keyframe(1, 0.2f, -0.8f, -0.8f, 0.3334f, 0));
            curve.preWrapMode = WrapMode.ClampForever;
            curve.postWrapMode = WrapMode.ClampForever;
            simulation.StiffnessCurve = curve;
            simulation.VelocityAttenuation = 0.7f;
            simulation.AngleLimit = 60f;
            curve = new AnimationCurve(new Keyframe(0, 0.2f, 0.8f, 0.8f, 0, 0.3334f), new Keyframe(1, 1, 0.8f, 0.8f, 0.3334f, 0));
            curve.preWrapMode = WrapMode.ClampForever;
            curve.postWrapMode = WrapMode.ClampForever;
            simulation.AngleLimitCurve = curve;
            simulation.AngleLimitStiffness = 0.5f;

            simulation.Rigidness = 1f;
            curve = new AnimationCurve(new Keyframe(0, 1, 0, -0.5f, 0, 0), new Keyframe(1, 0.5f, -0.5f, 0, 0, 0));
            curve.preWrapMode = WrapMode.ClampForever;
            curve.postWrapMode = WrapMode.ClampForever;
            simulation.RigidnessCurve = curve;
            simulation.Tether = 0.7f;

            simulation.WorldInertia = 1;
            simulation.LocalInertia = 1;

            simulation.SimulationType = ESimulationType.BoneCloth;

            simulation.RootBones = Array.Empty<string>();
            simulation.ConnectionMode = EConnectionMode.AutomaticMesh;

            simulation.SpringStrength = 0;
            simulation.SpringDistance = 0;
            simulation.SpringNormalDistanceRatio = 0;
            simulation.SpringNoise = 0;

            simulation.Reduction = 0;
            simulation.PaintMap = null;

            return simulation;
        }

        public static SimulationData Breasts()
        {
            var simulation = new SimulationData();

            simulation.Name = "Breasts";
            simulation.Enabled = true;
            simulation.AnimationPoseRatio = 0;
            simulation.Preset = EPreset.None;

            simulation.SkinningBones = Array.Empty<string>();

            simulation.CollisionMode = ECollisionMode.None;
            simulation.Radius = 0;
            simulation.RadiusCurve = new AnimationCurve();
            simulation.Friction = 0;

            simulation.MaxDistanceRadius = 0;
            simulation.BackstopNormalAlignment = null;
            simulation.BackstopDistance = 0;
            simulation.BackstopDistanceCurve = new AnimationCurve();
            simulation.BackstopRadius = 0;
            simulation.BackstopStiffness = 0;

            simulation.Gravity = 0;
            simulation.Damping = 0.3f;
            simulation.DampingCurve = new AnimationCurve();

            simulation.Stiffness = 0.6f;
            var curve = new AnimationCurve(new Keyframe(0, 1, 0, 0, 0, 0), new Keyframe(1, 0.5f, -0.8f, -0.8f, 0, 0));
            curve.preWrapMode = WrapMode.ClampForever;
            curve.postWrapMode = WrapMode.ClampForever;
            simulation.StiffnessCurve = curve;
            simulation.VelocityAttenuation = 0.8f;
            simulation.AngleLimit = 0;
            simulation.AngleLimitCurve = new AnimationCurve();
            simulation.AngleLimitStiffness = 0;

            simulation.Rigidness = 0;
            simulation.RigidnessCurve = new AnimationCurve();
            simulation.Tether = 0;

            simulation.WorldInertia = 1;
            simulation.LocalInertia = 0.8f;

            simulation.SimulationType = ESimulationType.BoneSpring;

            simulation.RootBones = Array.Empty<string>();
            simulation.ConnectionMode = EConnectionMode.AutomaticMesh;

            simulation.SpringStrength = 0.055f;
            simulation.SpringDistance = 0.05f;
            simulation.SpringNormalDistanceRatio = 1f;
            simulation.SpringNoise = 0;

            simulation.Reduction = 0;
            simulation.PaintMap = null;

            return simulation;
        }

        public static SimulationData Cape()
        {
            var simulation = new SimulationData();

            simulation.Name = "Cape";
            simulation.Enabled = true;
            simulation.AnimationPoseRatio = 0;
            simulation.Preset = EPreset.None;

            simulation.SkinningBones = Array.Empty<string>();

            simulation.CollisionMode = ECollisionMode.Edge;
            simulation.Radius = 0.004f;
            simulation.RadiusCurve = new AnimationCurve();
            simulation.Friction = 0.02f;

            simulation.MaxDistanceRadius = 0;
            simulation.BackstopNormalAlignment = null;
            simulation.BackstopDistance = 0;
            simulation.BackstopDistanceCurve = new AnimationCurve();
            simulation.BackstopRadius = 2f;
            simulation.BackstopStiffness = 1;

            simulation.Gravity = 7;
            simulation.Damping = 0.1f;
            simulation.DampingCurve = new AnimationCurve();

            simulation.Stiffness = 0.15f;
            var curve = new AnimationCurve(new Keyframe(0, 1, 0, 0, 0, 0), new Keyframe(1, 0.1f, -0.7f, -0.7f, 0, 0));
            curve.preWrapMode = WrapMode.ClampForever;
            curve.postWrapMode = WrapMode.ClampForever;
            simulation.StiffnessCurve = curve;
            simulation.VelocityAttenuation = 0.8f;
            simulation.AngleLimit = 45f;
            curve = new AnimationCurve(new Keyframe(0, 0, 2, 2, 0, 0), new Keyframe(1, 1, 0, 0, 0, 0));
            curve.preWrapMode = WrapMode.ClampForever;
            curve.postWrapMode = WrapMode.ClampForever;
            simulation.AngleLimitCurve = curve;
            simulation.AngleLimitStiffness = 1;

            simulation.Rigidness = 1f;
            curve = new AnimationCurve(new Keyframe(0, 1, 0, -0.5f, 0, 0), new Keyframe(1, 0.5f, -0.5f, 0, 0, 0));
            curve.preWrapMode = WrapMode.ClampForever;
            curve.postWrapMode = WrapMode.ClampForever;
            simulation.RigidnessCurve = curve;
            simulation.Tether = 0.8f;

            simulation.WorldInertia = 1;
            simulation.LocalInertia = 1;

            simulation.SimulationType = ESimulationType.BoneCloth;

            simulation.RootBones = Array.Empty<string>();
            simulation.ConnectionMode = EConnectionMode.AutomaticMesh;

            simulation.SpringStrength = 0;
            simulation.SpringDistance = 0;
            simulation.SpringNormalDistanceRatio = 0;
            simulation.SpringNoise = 0;

            simulation.Reduction = 0;
            simulation.PaintMap = null;

            return simulation;
        }

        public static SimulationData Necktie()
        {
            var simulation = new SimulationData();

            simulation.Name = "Necktie";
            simulation.Enabled = false;

            return simulation;
        }

        public static SimulationData Hair_Strains()
        {
            var simulation = new SimulationData();

            simulation.Name = "Hair (Long Strains)";
            simulation.Enabled = true;
            simulation.AnimationPoseRatio = 0;
            simulation.Preset = EPreset.HairStrains;

            simulation.SkinningBones = Array.Empty<string>();

            simulation.CollisionMode = ECollisionMode.Edge;
            simulation.Radius = 0.008f;
            simulation.RadiusCurve = new AnimationCurve();
            simulation.Friction = 0.05f;

            simulation.MaxDistanceRadius = 0;
            simulation.BackstopNormalAlignment = null;
            simulation.BackstopDistance = 0;
            simulation.BackstopDistanceCurve = new AnimationCurve();
            simulation.BackstopRadius = 2f;
            simulation.BackstopStiffness = 1;

            simulation.Gravity = 5;
            simulation.Damping = 0.1f;
            simulation.DampingCurve = new AnimationCurve();

            simulation.Stiffness = 0.2f;
            var curve = new AnimationCurve(new Keyframe(0, 1, -2.15f, -2.15f, 0, 0.3f), new Keyframe(1, 0.1f, -0.35f, -0.35f, 0.068f, 0));
            curve.preWrapMode = WrapMode.ClampForever;
            curve.postWrapMode = WrapMode.ClampForever;
            simulation.StiffnessCurve = curve;
            simulation.VelocityAttenuation = 0.8f;
            simulation.AngleLimit = 75f;
            var kf1 = new Keyframe(0, 0.2f, 2f, 2f, 0, 0);
            kf1.weightedMode = WeightedMode.None;
            var kf2 = new Keyframe(1, 1, 0, 0, 0, 0);
            kf2.weightedMode = WeightedMode.None;
            curve = new AnimationCurve(kf1, kf2);
            curve.preWrapMode = WrapMode.ClampForever;
            curve.postWrapMode = WrapMode.ClampForever;
            simulation.AngleLimitCurve = curve;
            simulation.AngleLimitStiffness = 0.3f;

            simulation.Rigidness = 1f;
            simulation.RigidnessCurve = new AnimationCurve();
            simulation.Tether = 1f;

            simulation.WorldInertia = 1;
            simulation.LocalInertia = 1;

            simulation.SimulationType = ESimulationType.BoneCloth;

            simulation.RootBones = Array.Empty<string>();
            simulation.ConnectionMode = EConnectionMode.Line;

            simulation.SpringStrength = 0;
            simulation.SpringDistance = 0;
            simulation.SpringNormalDistanceRatio = 0;
            simulation.SpringNoise = 0;

            simulation.Reduction = 0;
            simulation.PaintMap = null;

            return simulation;
        }

        #endregion
    }
}
