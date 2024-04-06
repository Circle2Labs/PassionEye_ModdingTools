using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Code.Frameworks.Character.Structs
{
	[Serializable]
	public struct SAvatarData
	{
		public List<SAvatarHumanBone> HumanBones;
		public List<SAvatarSkeletonBone> SkeletonBones;

		public float UpperArmTwist;
		public float LowerArmTwist;
		public float UpperLegTwist;
		public float LowerLegTwist;

		public float ArmStretch;
		public float LegStretch;

		public float FeetSpacing;

		public bool HasTranslationDoF;

		public SAvatarData(HumanDescription humanDescription)
		{
			HumanBones = new List<SAvatarHumanBone>();
			SkeletonBones = new List<SAvatarSkeletonBone>();

			foreach (var humanBone in humanDescription.human)
				HumanBones.Add(new SAvatarHumanBone(humanBone));

			foreach (var skeletonBone in humanDescription.skeleton)
				SkeletonBones.Add(new SAvatarSkeletonBone(skeletonBone));

			UpperArmTwist = humanDescription.upperArmTwist;
			LowerArmTwist = humanDescription.lowerArmTwist;
			UpperLegTwist = humanDescription.upperLegTwist;
			LowerLegTwist = humanDescription.lowerLegTwist;

			ArmStretch = humanDescription.armStretch;
			LegStretch = humanDescription.legStretch;

			FeetSpacing = humanDescription.feetSpacing;

			HasTranslationDoF = humanDescription.hasTranslationDoF;
		}

		public HumanDescription ToHumanDescription()
		{
			var humanDescription = new HumanDescription();

			humanDescription.human = new HumanBone[HumanBones.Count];
			for (var i = 0; i < HumanBones.Count; i++)
			{
				var humanBone = new HumanBone();
				humanBone.boneName = HumanBones[i].BoneName;
				humanBone.humanName = HumanBones[i].HumanName;

				var humanLimit = new HumanLimit();
				humanLimit.useDefaultValues = HumanBones[i].Limit.UseDefaultValues;
				humanLimit.min = HumanBones[i].Limit.Min;
				humanLimit.max = HumanBones[i].Limit.Max;
				humanLimit.center = HumanBones[i].Limit.Center;
				humanLimit.axisLength = HumanBones[i].Limit.AxisLength;

				humanBone.limit = humanLimit;

				humanDescription.human[i] = humanBone;
			}

			humanDescription.skeleton = new SkeletonBone[SkeletonBones.Count];
			for (var i = 0; i < SkeletonBones.Count; i++)
			{
				var skeletonBone = new SkeletonBone();
				skeletonBone.name = SkeletonBones[i].Name;
				typeof(SkeletonBone).GetField("parentName", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(skeletonBone, SkeletonBones[i].ParentName);
				skeletonBone.position = SkeletonBones[i].Position;
				skeletonBone.scale = SkeletonBones[i].Scale;
				skeletonBone.rotation = SkeletonBones[i].Rotation;

				humanDescription.skeleton[i] = skeletonBone;
			}

			humanDescription.upperArmTwist = UpperArmTwist;
			humanDescription.lowerArmTwist = LowerArmTwist;
			humanDescription.upperLegTwist = UpperLegTwist;
			humanDescription.lowerLegTwist = LowerLegTwist;

			humanDescription.armStretch = ArmStretch;
			humanDescription.legStretch = LegStretch;

			humanDescription.feetSpacing = FeetSpacing;

			humanDescription.hasTranslationDoF = HasTranslationDoF;

			return humanDescription;
		}

		[Serializable]
		public struct SAvatarHumanBone
		{
			public string BoneName;
			public string HumanName;

			public SAvatarLimit Limit;

			public SAvatarHumanBone(HumanBone humanBone)
			{
				BoneName = humanBone.boneName;
				HumanName = humanBone.humanName;
				Limit = new SAvatarLimit(humanBone.limit);
			}
		}

		[Serializable]
		public struct SAvatarSkeletonBone
		{
			public string Name;
			public string ParentName;

			public Vector3 Position;
			public Vector3 Scale;

			public Quaternion Rotation;

			public SAvatarSkeletonBone(SkeletonBone skeletonBone)
			{
				Name = skeletonBone.name;
				ParentName = (string)typeof(SkeletonBone).GetField("parentName", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(skeletonBone);
				Position = skeletonBone.position;
				Scale = skeletonBone.scale;
				Rotation = skeletonBone.rotation;
			}
		}

		[Serializable]
		public struct SAvatarLimit
		{
			public bool UseDefaultValues;

			public Vector3 Min;
			public Vector3 Max;
			public Vector3 Center;

			public float AxisLength;

			public SAvatarLimit(HumanLimit humanLimit)
			{
				UseDefaultValues = humanLimit.useDefaultValues;
				Min = humanLimit.min;
				Max = humanLimit.max;
				Center = humanLimit.center;
				AxisLength = humanLimit.axisLength;
			}
		}
	}
}