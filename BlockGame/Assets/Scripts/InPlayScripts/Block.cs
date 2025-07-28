using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public BlockData data;
    public Color color;
    public List<Vector2Int> currentShapeCells;
    private void Start()
    {
       if(data!=null) currentShapeCells = new List<Vector2Int>(data.cells);
    }
    public void RotateCells()
    {
        if (BlockRotateData.Instance != null)
        {
            currentShapeCells = BlockRotateData.Instance.RotateBlockDataCells(currentShapeCells);
            UpdateBlockPos();
        }
    }
    void UpdateBlockPos()
    {
        int i = 0;
        foreach (Transform child in transform)
        {
            child.localPosition = new Vector3(currentShapeCells[i].x, currentShapeCells[i].y,child.position.z);
            i ++;
        }
    }
    public void AnimationPlacement()
    {
        StartCoroutine(PlacementAnimationRoutine());
    }
    private IEnumerator PlacementAnimationRoutine()
    {
        // Hedef boyutlar
        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = Vector3.one * 1.05f; // yüzde 5 daha büyük
        float duration = 0.1f; // animasyon süresi

        // büyüme
        float timer = 0f;
        while (timer < duration)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, timer / duration);
            timer += Time.deltaTime;
            yield return null; 
        }
        transform.localScale = targetScale; 

        // küçülme
        timer = 0f;
        while (timer < duration)
        {
            transform.localScale = Vector3.Lerp(targetScale, originalScale, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.localScale = originalScale;
    }
}
