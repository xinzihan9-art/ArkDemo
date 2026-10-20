using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// One-time, editor-only setup. A request file explicitly enables execution.
[InitializeOnLoad]
public static class ShanbiIdleSetup
{
    private const string Root = "Assets/live2d/珊比";
    private const string Work = Root + "/.codex-export/";
    private const string Frames = Root + "/output/Idle/";
    private const string Request = Work + "unity-setup.request";
    private const string Report = Work + "unity-setup-result.json";

    static ShanbiIdleSetup() { EditorApplication.delayCall += RunPending; }

    private static void RunPending()
    {
        if (!File.Exists(Request)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.delayCall += RunPending;
            return;
        }
        // Consume before mutation so script reloads cannot repeat the operation.
        File.Move(Request, Work + "unity-setup.started");
        try { Setup(); }
        catch (Exception e)
        {
            File.WriteAllText(Report, JsonUtility.ToJson(new Result { success = false, error = e.ToString() }, true));
            Debug.LogException(e);
        }
    }

    private static void Setup()
    {
        var scene = SceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/SampleScene.unity")
            throw new InvalidOperationException("Expected SampleScene to be active; no scene changes made.");
        var reference = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<SpriteRenderer>(true))
            .Single(r => r.sprite != null && AssetDatabase.GetAssetPath(r.sprite).Contains("安洁莉娜"));
        if (scene.GetRootGameObjects().Any(g => g.name == "珊比"))
            throw new InvalidOperationException("A 珊比 root already exists; refusing to create a duplicate.");
        if (File.Exists(Frames + "Idle.anim") || File.Exists(Frames + "ShanbiIdle.controller"))
            throw new InvalidOperationException("Animation assets already exist; refusing to overwrite them.");
        // Preserve both the disk version and all currently unsaved scene edits.
        File.Copy(scene.path, Work + "SampleScene.before-on-disk.unity", true);
        if (!EditorSceneManager.SaveScene(scene, Work + "SampleScene.before-in-memory.unity", true))
            throw new InvalidOperationException("Could not back up current scene.");

        var refImporter = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(reference.sprite));
        var sprites = new Sprite[80];
        for (int i = 0; i < sprites.Length; i++)
        {
            string path = Frames + "char_4235_thumpy-Idle_" + i.ToString("00") + ".png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer == null) throw new InvalidOperationException("Missing PNG: " + path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = refImporter.spritePixelsPerUnit;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = refImporter.filterMode;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            settings.spritePivot = new Vector2(0.5f, 0.5f);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
            sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprites[i] == null) throw new InvalidOperationException("Sprite import failed: " + path);
        }

        var clip = new AnimationClip { name = "Idle", frameRate = 20f };
        var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        AnimationUtility.SetObjectReferenceCurve(clip, binding,
            sprites.Select((sprite, i) => new ObjectReferenceKeyframe { time = i / 20f, value = sprite }).ToArray());
        var clipSettings = AnimationUtility.GetAnimationClipSettings(clip);
        clipSettings.loopTime = true;
        clipSettings.startTime = 0;
        clipSettings.stopTime = 4;
        AnimationUtility.SetAnimationClipSettings(clip, clipSettings);
        AssetDatabase.CreateAsset(clip, Frames + "Idle.anim");
        var controller = AnimatorController.CreateAnimatorControllerAtPath(Frames + "ShanbiIdle.controller");
        var stateMachine = controller.layers[0].stateMachine;
        var idle = stateMachine.AddState("Idle");
        idle.motion = clip;
        stateMachine.defaultState = idle;

        var character = new GameObject("珊比");
        Undo.RegisterCreatedObjectUndo(character, "Create stationary Shanbi");
        SceneManager.MoveGameObjectToScene(character, scene);
        character.transform.position = reference.transform.parent.position + Vector3.left * 0.8f;
        var visual = new GameObject("SpriteVisual");
        visual.transform.SetParent(character.transform, false);
        visual.transform.localScale = reference.transform.localScale;
        visual.transform.localRotation = reference.transform.localRotation;
        // Match Angelina's foot baseline despite different canvas heights.
        var pos = reference.transform.localPosition;
        pos.y += (reference.sprite.bounds.min.y - sprites[0].bounds.min.y) * reference.transform.localScale.y;
        visual.transform.localPosition = pos;
        var renderer = visual.AddComponent<SpriteRenderer>();
        EditorUtility.CopySerialized(reference, renderer);
        renderer.sprite = sprites[0];
        var animator = visual.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        var billboard = reference.GetComponent<SpriteBillboard>();
        if (billboard != null)
            EditorUtility.CopySerialized(billboard, visual.AddComponent<SpriteBillboard>());

        // Verify all frame bindings on the real target, and ensure the clip changes no transform.
        var rootPosition = character.transform.position;
        var visualPosition = visual.transform.localPosition;
        var visualRotation = visual.transform.localRotation;
        var visualScale = visual.transform.localScale;
        for (int i = 0; i < sprites.Length; i++)
        {
            clip.SampleAnimation(visual, i / 20f + 0.001f);
            if (renderer.sprite != sprites[i]) throw new InvalidOperationException("Incorrect sprite at frame " + i);
            if (character.transform.position != rootPosition || visual.transform.localPosition != visualPosition ||
                visual.transform.localRotation != visualRotation || visual.transform.localScale != visualScale)
                throw new InvalidOperationException("Animation moved the character at frame " + i);
        }
        renderer.sprite = sprites[0];
        if (character.GetComponentInChildren<PlayerTestMovement>(true) != null ||
            character.GetComponentInChildren<CharacterController>(true) != null ||
            character.GetComponentInChildren<Rigidbody>(true) != null ||
            character.GetComponentInChildren<Rigidbody2D>(true) != null || animator.applyRootMotion ||
            AnimationUtility.GetCurveBindings(clip).Length != 0 ||
            controller.parameters.Length != 0 || stateMachine.states.Length != 1 ||
            Mathf.Abs(clip.length - 4f) > 0.001f)
            throw new InvalidOperationException("Stationary animation validation failed.");

        PrefabUtility.SaveAsPrefabAsset(character, Root + "/珊比.prefab");
        EditorUtility.SetDirty(clip);
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        Selection.activeGameObject = character;
        EditorGUIUtility.PingObject(character);
        File.WriteAllText(Report, JsonUtility.ToJson(new Result {
            success = true, frames = sprites.Length, fps = clip.frameRate, duration = clip.length,
            scene = scene.path, prefab = Root + "/珊比.prefab", position = rootPosition,
            stationary = true, controller = AssetDatabase.GetAssetPath(controller)
        }, true));
        Debug.Log("[ShanbiIdleSetup] Created and bound 80-frame, 20 FPS, 4-second stationary Idle. All frames verified; scene saved.");
    }

    [Serializable] private class Result
    {
        public bool success;
        public string error;
        public int frames;
        public float fps;
        public float duration;
        public string scene;
        public string prefab;
        public Vector3 position;
        public bool stationary;
        public string controller;
    }
}
