using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "PigeonStats", menuName = "Game/Pigeon Stats")]
public class PigeonStats : ScriptableObject
{
    [Serializable]
    public class LevelStats
    {
        [ReadOnly, PropertyOrder(-1)]
        public float weight; // 自动计算的体重值
        public float moveSpeed = 3f; // 默认值
        public float jumpHeight = 2f; // 默认值
    }

    [SerializeField, Range(2, 10), OnValueChanged("UpdateLevelStats")]
    private int levels = 5; // 总级数

    [SerializeField, Tooltip("是否对中间值进行线性插值")]
    private bool useInterpolation = true;

    [TableList]
    public List<LevelStats> levelStats = new();

    private void OnEnable()
    {
        UpdateLevelStats();
    }

    private void OnValidate()
    {
        UpdateLevelStats();
    }

    private void UpdateLevelStats()
    {
        if (levels < 2) levels = 2;
        
        while (levelStats.Count < levels)
        {
            levelStats.Add(new LevelStats 
            { 
                moveSpeed = 3f, 
                jumpHeight = 2f,
                weight = levelStats.Count / (float)(levels - 1) // 默认视觉大小与等级成正比
            });
        }
        while (levelStats.Count > levels)
        {
            levelStats.RemoveAt(levelStats.Count - 1);
        }

        for (int i = 0; i < levels; i++)
        {
            levelStats[i].weight = i / (float)(levels - 1);
        }
    }

    public (float speed, float jump, float visual) GetStatsForWeight(float currentWeight)
    {
        if (levelStats == null || levelStats.Count < 2)
        {
            UpdateLevelStats();
        }

        if (currentWeight <= levelStats[0].weight)
            return (levelStats[0].moveSpeed, levelStats[0].jumpHeight, levelStats[0].weight);

        if (currentWeight >= levelStats[^1].weight)
            return (levelStats[^1].moveSpeed, levelStats[^1].jumpHeight, levelStats[^1].weight);

        if (useInterpolation)
        {
            for (int i = 0; i < levelStats.Count - 1; i++)
            {
                if (currentWeight >= levelStats[i].weight && currentWeight <= levelStats[i + 1].weight)
                {
                    float t = Mathf.InverseLerp(levelStats[i].weight, levelStats[i + 1].weight, currentWeight);
                    float speed = Mathf.Lerp(levelStats[i].moveSpeed, levelStats[i + 1].moveSpeed, t);
                    float jump = Mathf.Lerp(levelStats[i].jumpHeight, levelStats[i + 1].jumpHeight, t);
                    float visual = Mathf.Lerp(levelStats[i].weight, levelStats[i + 1].weight, t);
                    return (speed, jump, visual);
                }
            }
        }
        else
        {
            for (int i = 0; i < levelStats.Count - 1; i++)
            {
                if (currentWeight >= levelStats[i].weight && currentWeight < levelStats[i + 1].weight)
                {
                    return (levelStats[i].moveSpeed, levelStats[i].jumpHeight, levelStats[i].weight);
                }
            }
        }

        return (3f, 2f, 0f);
    }
}