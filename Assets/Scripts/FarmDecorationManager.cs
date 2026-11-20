
using System.Collections.Generic;
using UnityEngine;
using TMPro;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider))]
public class FarmDecorationManager : MonoBehaviour
{
    [Header("玩家")]
    public Transform player;

    [Header("参考植物（PlantModels下已摆好的植物）")]
    public GameObject referencePlant;

    [Header("生成植物的父物体")]
    public Transform plantContainer;

    [Header("种植设置")]
    [Min(1)]
    public int maxPlants = 10;

    [Min(0f)]
    public float minPlantDistance = 0.45f;

    [Min(0f)]
    public float clearDistance = 0.45f;

    [Min(0f)]
    public float plantingMargin = 0.05f;

    [Header("交互UI")]
    public GameObject interactUI;
    public TMP_Text interactText;

    private BoxCollider farmCollider;
    private CharacterController playerController;

    private readonly List<GameObject> plantedObjects =
        new List<GameObject>();

    void Awake()
    {
        farmCollider = GetComponent<BoxCollider>();

        if (player != null)
        {
            playerController =
                player.GetComponent<CharacterController>();
        }
    }

    void Start()
    {
        HideUI();
    }

    void OnDisable()
    {
        HideUI();
    }

    void Update()
    {
        plantedObjects.RemoveAll(item => item == null);

        if (!IsPlayerNearby())
        {
            HideUI();
            return;
        }

        int nearest = FindNearestPlant();

        if (InteractPressed())
        {
            if (nearest >= 0)
            {
                ClearPlant(nearest);
            }
            else
            {
                Plant();
            }
        }

        UpdateInteractionUI();
    }

    // 检查玩家是否在交互范围内
    bool IsPlayerNearby()
    {
        if (player == null ||
            farmCollider == null ||
            !farmCollider.enabled)
        {
            return false;
        }

        if (playerController == null)
        {
            playerController =
                player.GetComponent<CharacterController>();
        }

        if (playerController != null &&
            playerController.enabled)
        {
            return farmCollider.bounds.Intersects(
                playerController.bounds
            );
        }

        return farmCollider.bounds.Contains(
            player.position
        );
    }

    // E键输入
    bool InteractPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.E))
        {
            return true;
        }
#endif

        return false;
    }

    // 获取种植位置
    bool TryGetPlantPosition(out Vector3 position)
    {
        position = Vector3.zero;

        if (player == null ||
            farmCollider == null ||
            referencePlant == null)
        {
            return false;
        }

        Transform area = farmCollider.transform;

        Vector3 center =
            area.TransformPoint(farmCollider.center);

        Vector3 local = area.InverseTransformPoint(
            new Vector3(
                player.position.x,
                center.y,
                player.position.z
            )
        );

        Vector3 scale = area.lossyScale;

        float halfX =
            farmCollider.size.x * 0.5f -
            plantingMargin /
            Mathf.Max(Mathf.Abs(scale.x), 0.0001f);

        float halfZ =
            farmCollider.size.z * 0.5f -
            plantingMargin /
            Mathf.Max(Mathf.Abs(scale.z), 0.0001f);

        if (halfX <= 0f || halfZ <= 0f)
            return false;

        // 玩家必须处在农田XZ范围内
        if (Mathf.Abs(
            local.x - farmCollider.center.x
            ) > halfX)
            return false;

        if (Mathf.Abs(
            local.z - farmCollider.center.z
            ) > halfZ)
            return false;

        // XZ取玩家，Y直接取参考植物的世界高度
        position = new Vector3(
            player.position.x,
            referencePlant.transform.position.y,
            player.position.z
        );

        return true;
    }

    // 计算水平距离
    float DistanceXZ(Vector3 a, Vector3 b)
    {
        Vector2 p1 = new Vector2(a.x, a.z);
        Vector2 p2 = new Vector2(b.x, b.z);

        return Vector2.Distance(p1, p2);
    }

    // 检查种植间距
    bool HasSpace(Vector3 position)
    {
        foreach (GameObject plant in plantedObjects)
        {
            if (plant == null)
                continue;

            if (DistanceXZ(
                position,
                plant.transform.position
            ) < minPlantDistance)
            {
                return false;
            }
        }

        return true;
    }

    // 种植
    void Plant()
    {
        if (referencePlant == null)
        {
            Debug.LogWarning(
                "没有绑定参考植物！",
                this
            );
            return;
        }

        if (plantedObjects.Count >= maxPlants)
            return;

        if (!TryGetPlantPosition(out Vector3 position))
            return;

        if (!HasSpace(position))
            return;

        // 直接复制场景中已调整好的植物
        GameObject instance = Instantiate(
            referencePlant
        );

        // 保留参考植物的世界旋转和缩放
        instance.transform.SetPositionAndRotation(
            position,
            referencePlant.transform.rotation
        );

        // 放入PlantContainer，保持世界变换
        if (plantContainer != null)
        {
            instance.transform.SetParent(
                plantContainer,
                true
            );
        }

        // 确保复制出来的植物显示
        instance.SetActive(true);

        plantedObjects.Add(instance);

        Debug.Log(
            "种植成功：" +
            instance.name +
            " | 世界位置：" +
            instance.transform.position +
            " | 数量：" +
            plantedObjects.Count + "/" + maxPlants
        );
    }

    // 查找附近最近的一株植物
    int FindNearestPlant()
    {
        if (player == null)
            return -1;

        float nearestDistance =
            Mathf.Max(clearDistance, minPlantDistance);

        int nearestIndex = -1;

        for (int i = 0; i < plantedObjects.Count; i++)
        {
            GameObject plant = plantedObjects[i];

            if (plant == null)
                continue;

            float distance = DistanceXZ(
                player.position,
                plant.transform.position
            );

            if (distance <= nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }

        return nearestIndex;
    }

    // 清除单株植物
    void ClearPlant(int index)
    {
        if (index < 0 ||
            index >= plantedObjects.Count)
            return;

        GameObject target = plantedObjects[index];

        plantedObjects.RemoveAt(index);

        if (target != null)
        {
            target.SetActive(false);
            Destroy(target);
        }

        Debug.Log(
            "清除成功，剩余：" +
            plantedObjects.Count + "/" + maxPlants
        );
    }

    // 更新提示文字
    void UpdateInteractionUI()
    {
        if (interactUI == null ||
            interactText == null)
            return;

        interactUI.SetActive(true);

        string count =
            "  " +
            plantedObjects.Count +
            "/" + maxPlants;

        if (FindNearestPlant() >= 0)
        {
            interactText.text = "E 清除" + count;
        }
        else if (plantedObjects.Count >= maxPlants)
        {
            interactText.text = "种植上限" + count;
        }
        else if (!TryGetPlantPosition(out Vector3 position))
        {
            interactText.text = "请进入农田内部" + count;
        }
        else if (!HasSpace(position))
        {
            interactText.text = "距离其他植物太近" + count;
        }
        else
        {
            interactText.text = "E 种植" + count;
        }
    }

    void HideUI()
    {
        if (interactUI != null)
            interactUI.SetActive(false);
    }
}

