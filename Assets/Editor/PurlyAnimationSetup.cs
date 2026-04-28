using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class PurlyAnimationSetup
{
    private const string PrefabPath = "Assets/Prefabs/Purly.prefab";
    private const string AnimationFolder = "Assets/Animations";
    private const string PurlyAnimationFolder = "Assets/Animations/Purly";
    private const string ControllerPath = "Assets/Animations/Purly/Purly.controller";
    private const string IdleClipPath = "Assets/Animations/Purly/PurlyIdle.anim";
    private const string WalkRightClipPath = "Assets/Animations/Purly/PurlyWalkRight.anim";
    private const string WalkLeftClipPath = "Assets/Animations/Purly/PurlyWalkLeft.anim";
    private const string JumpClipPath = "Assets/Animations/Purly/PurlyJump.anim";

    private const string LeftArmPath = "middleSphere/LeftArm";
    private const string RightArmPath = "middleSphere/RightArm";
    private const string LeftLegPath = "middleSphere/LeftLeg";
    private const string RightLegPath = "middleSphere/RightLeg";
    private const string MiddleSpherePath = "middleSphere";

    [MenuItem("Tools/Snowman/Setup Purly Animations")]
    public static void SetupPurlyAnimations()
    {
        EnsureFolder(AnimationFolder);
        EnsureFolder(PurlyAnimationFolder);

        AnimationClip idleClip = CreateIdleClip();
        AnimationClip walkRightClip = CreateWalkRightClip();
        AnimationClip walkLeftClip = CreateWalkLeftClip();
        AnimationClip jumpClip = CreateJumpClip();

        SaveClip(idleClip, IdleClipPath);
        SaveClip(walkRightClip, WalkRightClipPath);
        SaveClip(walkLeftClip, WalkLeftClipPath);
        SaveClip(jumpClip, JumpClipPath);

        AnimatorController controller = CreateController();
        AssignControllerToPrefab(controller);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Purly animations and Animator Controller were created and assigned.");
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = System.IO.Path.GetDirectoryName(path)?.Replace("\\", "/");
        string folderName = System.IO.Path.GetFileName(path);
        AssetDatabase.CreateFolder(parent ?? "Assets", folderName);
    }

    private static void SaveClip(AnimationClip clip, string path)
    {
        AnimationClip existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (existing == null)
        {
            AssetDatabase.CreateAsset(clip, path);
            return;
        }

        EditorUtility.CopySerialized(clip, existing);
    }

    private static AnimationClip CreateIdleClip()
    {
        AnimationClip clip = new AnimationClip
        {
            frameRate = 60f,
            name = "PurlyIdle"
        };

        SetLoop(clip, true);
        SetRotationZ(clip, LeftArmPath, new[] { 0f, 0.4f }, new[] { 52f, 52f });
        SetRotationZ(clip, RightArmPath, new[] { 0f, 0.4f }, new[] { -52f, -52f });
        SetRotationZ(clip, LeftLegPath, new[] { 0f, 0.4f }, new[] { 0f, 0f });
        SetRotationZ(clip, RightLegPath, new[] { 0f, 0.4f }, new[] { 0f, 0f });
        SetPositionY(clip, MiddleSpherePath, new[] { 0f, 0.4f }, new[] { 0.17782f, 0.17782f });
        SetPositionX(clip, MiddleSpherePath, new[] { 0f, 0.4f }, new[] { 0.98127997f, 0.98127997f });
        SetRotationZ(clip, MiddleSpherePath, new[] { 0f, 0.4f }, new[] { 0f, 0f });
        SetScaleXY(clip, MiddleSpherePath, new[] { 0f, 0.4f }, new[] { 2.165f, 2.165f }, new[] { 2.086286f, 2.086286f });
        return clip;
    }

    private static AnimationClip CreateWalkRightClip()
    {
        AnimationClip clip = new AnimationClip
        {
            frameRate = 60f,
            name = "PurlyWalkRight"
        };

        SetLoop(clip, true);
        float[] times = { 0f, 0.15f, 0.3f, 0.45f, 0.6f };
        SetRotationZ(clip, LeftArmPath, times, new[] { 28f, 48f, 78f, 48f, 28f });
        SetRotationZ(clip, RightArmPath, times, new[] { -78f, -48f, -28f, -48f, -78f });
        SetRotationZ(clip, LeftLegPath, times, new[] { 28f, 18f, 6f, 16f, 28f });
        SetRotationZ(clip, RightLegPath, times, new[] { -20f, -10f, -2f, -10f, -20f });
        SetPositionY(clip, LeftLegPath, times, new[] { -1.54f, -1.48f, -1.4f, -1.46f, -1.54f });
        SetPositionY(clip, RightLegPath, times, new[] { -1.4f, -1.46f, -1.52f, -1.46f, -1.4f });
        SetPositionY(clip, MiddleSpherePath, times, new[] { 0.14f, 0.195f, 0.16f, 0.195f, 0.14f });
        SetPositionX(clip, MiddleSpherePath, times, new[] { 0.95f, 1.01f, 1.03f, 1.01f, 0.95f });
        SetRotationZ(clip, MiddleSpherePath, times, new[] { 3.5f, 0f, -3.5f, 0f, 3.5f });
        SetScaleXY(clip, MiddleSpherePath, times, new[] { 2.23f, 2.11f, 2.2f, 2.11f, 2.23f }, new[] { 2.01f, 2.15f, 2.06f, 2.15f, 2.01f });
        return clip;
    }

    private static AnimationClip CreateWalkLeftClip()
    {
        AnimationClip clip = new AnimationClip
        {
            frameRate = 60f,
            name = "PurlyWalkLeft"
        };

        SetLoop(clip, true);
        float[] times = { 0f, 0.15f, 0.3f, 0.45f, 0.6f };
        SetRotationZ(clip, LeftArmPath, times, new[] { 78f, 48f, 28f, 48f, 78f });
        SetRotationZ(clip, RightArmPath, times, new[] { -28f, -48f, -78f, -48f, -28f });
        SetRotationZ(clip, LeftLegPath, times, new[] { 20f, 10f, 2f, 10f, 20f });
        SetRotationZ(clip, RightLegPath, times, new[] { -28f, -18f, -6f, -16f, -28f });
        SetPositionY(clip, LeftLegPath, times, new[] { -1.4f, -1.46f, -1.52f, -1.46f, -1.4f });
        SetPositionY(clip, RightLegPath, times, new[] { -1.54f, -1.48f, -1.4f, -1.46f, -1.54f });
        SetPositionY(clip, MiddleSpherePath, times, new[] { 0.14f, 0.195f, 0.16f, 0.195f, 0.14f });
        SetPositionX(clip, MiddleSpherePath, times, new[] { 1.01f, 0.95f, 0.93f, 0.95f, 1.01f });
        SetRotationZ(clip, MiddleSpherePath, times, new[] { -3.5f, 0f, 3.5f, 0f, -3.5f });
        SetScaleXY(clip, MiddleSpherePath, times, new[] { 2.23f, 2.11f, 2.2f, 2.11f, 2.23f }, new[] { 2.01f, 2.15f, 2.06f, 2.15f, 2.01f });
        return clip;
    }

    private static AnimationClip CreateJumpClip()
    {
        AnimationClip clip = new AnimationClip
        {
            frameRate = 60f,
            name = "PurlyJump"
        };

        SetLoop(clip, false);
        float[] times = { 0f, 0.08f, 0.16f, 0.28f, 0.42f };
        SetRotationZ(clip, LeftArmPath, times, new[] { 52f, 34f, 12f, 12f, 52f });
        SetRotationZ(clip, RightArmPath, times, new[] { -52f, -96f, -62f, -26f, -52f });
        SetRotationZ(clip, LeftLegPath, times, new[] { -10f, 4f, 22f, 26f, 0f });
        SetRotationZ(clip, RightLegPath, times, new[] { 18f, -62f, -26f, 4f, 0f });
        SetPositionY(clip, LeftLegPath, times, new[] { -1.5f, -1.45f, -1.36f, -1.32f, -1.5f });
        SetPositionY(clip, RightLegPath, times, new[] { -1.46f, -1.74f, -1.56f, -1.4f, -1.5f });
        SetPositionY(clip, MiddleSpherePath, times, new[] { 0.16f, 0.1f, 0.23f, 0.31f, 0.17782f });
        SetPositionX(clip, MiddleSpherePath, times, new[] { 0.98127997f, 0.955f, 0.99f, 1.02f, 0.98127997f });
        SetRotationZ(clip, MiddleSpherePath, times, new[] { 0f, -7f, 3f, 5f, 0f });
        SetScaleXY(clip, MiddleSpherePath, times, new[] { 2.18f, 2.28f, 2.12f, 2.08f, 2.165f }, new[] { 2.07f, 1.95f, 2.14f, 2.18f, 2.086286f });
        return clip;
    }

    private static void SetRotationZ(AnimationClip clip, string path, float[] times, float[] values)
    {
        EditorCurveBinding binding = EditorCurveBinding.FloatCurve(path, typeof(Transform), "localEulerAnglesRaw.z");
        AnimationCurve curve = new AnimationCurve();

        for (int i = 0; i < times.Length; i++)
        {
            curve.AddKey(new Keyframe(times[i], values[i]));
        }

        for (int i = 0; i < curve.length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
            AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
        }

        AnimationUtility.SetEditorCurve(clip, binding, curve);
    }

    private static void SetPositionY(AnimationClip clip, string path, float[] times, float[] values)
    {
        EditorCurveBinding binding = EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalPosition.y");
        AnimationCurve curve = new AnimationCurve();

        for (int i = 0; i < times.Length; i++)
        {
            curve.AddKey(new Keyframe(times[i], values[i]));
        }

        for (int i = 0; i < curve.length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
            AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
        }

        AnimationUtility.SetEditorCurve(clip, binding, curve);
    }

    private static void SetPositionX(AnimationClip clip, string path, float[] times, float[] values)
    {
        EditorCurveBinding binding = EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalPosition.x");
        AnimationCurve curve = new AnimationCurve();

        for (int i = 0; i < times.Length; i++)
        {
            curve.AddKey(new Keyframe(times[i], values[i]));
        }

        for (int i = 0; i < curve.length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
            AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
        }

        AnimationUtility.SetEditorCurve(clip, binding, curve);
    }

    private static void SetScaleXY(AnimationClip clip, string path, float[] times, float[] xValues, float[] yValues)
    {
        SetFloatCurve(clip, path, "m_LocalScale.x", times, xValues);
        SetFloatCurve(clip, path, "m_LocalScale.y", times, yValues);
    }

    private static void SetFloatCurve(AnimationClip clip, string path, string propertyName, float[] times, float[] values)
    {
        EditorCurveBinding binding = EditorCurveBinding.FloatCurve(path, typeof(Transform), propertyName);
        AnimationCurve curve = new AnimationCurve();

        for (int i = 0; i < times.Length; i++)
        {
            curve.AddKey(new Keyframe(times[i], values[i]));
        }

        for (int i = 0; i < curve.length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
            AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
        }

        AnimationUtility.SetEditorCurve(clip, binding, curve);
    }

    private static AnimatorController CreateController()
    {
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath) != null)
        {
            AssetDatabase.DeleteAsset(ControllerPath);
        }

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        controller.AddParameter("MoveX", AnimatorControllerParameterType.Float);
        controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);

        AnimatorControllerLayer layer = controller.layers[0];
        AnimatorStateMachine stateMachine = layer.stateMachine;

        AnimationClip idleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(IdleClipPath);
        AnimationClip walkRightClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(WalkRightClipPath);
        AnimationClip walkLeftClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(WalkLeftClipPath);
        AnimationClip jumpClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(JumpClipPath);

        AnimatorState idle = stateMachine.AddState("Idle", new Vector3(250f, 150f));
        AnimatorState walkRight = stateMachine.AddState("WalkRight", new Vector3(450f, 60f));
        AnimatorState walkLeft = stateMachine.AddState("WalkLeft", new Vector3(450f, 240f));
        AnimatorState jump = stateMachine.AddState("Jump", new Vector3(650f, 150f));

        idle.motion = idleClip;
        walkRight.motion = walkRightClip;
        walkLeft.motion = walkLeftClip;
        jump.motion = jumpClip;

        stateMachine.defaultState = idle;

        AddTransition(idle, walkRight, false, transition =>
        {
            transition.AddCondition(AnimatorConditionMode.Greater, 0.1f, "MoveX");
        });
        AddTransition(idle, walkLeft, false, transition =>
        {
            transition.AddCondition(AnimatorConditionMode.Less, -0.1f, "MoveX");
        });
        AddTransition(walkRight, idle, false, transition =>
        {
            transition.AddCondition(AnimatorConditionMode.Less, 0.1f, "MoveX");
        });
        AddTransition(walkLeft, idle, false, transition =>
        {
            transition.AddCondition(AnimatorConditionMode.Greater, -0.1f, "MoveX");
        });
        AddTransition(walkRight, walkLeft, false, transition =>
        {
            transition.AddCondition(AnimatorConditionMode.Less, -0.1f, "MoveX");
        });
        AddTransition(walkLeft, walkRight, false, transition =>
        {
            transition.AddCondition(AnimatorConditionMode.Greater, 0.1f, "MoveX");
        });

        AnimatorStateTransition anyToJump = stateMachine.AddAnyStateTransition(jump);
        anyToJump.hasExitTime = false;
        anyToJump.duration = 0.05f;
        anyToJump.AddCondition(AnimatorConditionMode.If, 0f, "Jump");

        AddTransition(jump, idle, true, transition => { });

        return controller;
    }

    private static void AddTransition(AnimatorState from, AnimatorState to, bool hasExitTime, System.Action<AnimatorStateTransition> configure)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = hasExitTime;
        transition.exitTime = hasExitTime ? 0.95f : 0f;
        transition.hasFixedDuration = true;
        transition.duration = 0.08f;
        configure(transition);
    }

    private static void AssignControllerToPrefab(AnimatorController controller)
    {
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(PrefabPath);
        Animator animator = prefabRoot.GetComponent<Animator>();
        if (animator == null)
        {
            animator = prefabRoot.AddComponent<Animator>();
        }

        animator.runtimeAnimatorController = controller;
        PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath);
        PrefabUtility.UnloadPrefabContents(prefabRoot);
    }

    private static void SetLoop(AnimationClip clip, bool loop)
    {
        SerializedObject serializedClip = new SerializedObject(clip);
        SerializedProperty settings = serializedClip.FindProperty("m_AnimationClipSettings");
        if (settings != null)
        {
            settings.FindPropertyRelative("m_LoopTime").boolValue = loop;
            settings.FindPropertyRelative("m_LoopBlend").boolValue = loop;
            serializedClip.ApplyModifiedProperties();
            return;
        }

        PropertyInfo legacySettings = typeof(AnimationClip).GetProperty("wrapMode");
        if (legacySettings != null)
        {
            clip.wrapMode = loop ? WrapMode.Loop : WrapMode.Once;
        }
    }
}
