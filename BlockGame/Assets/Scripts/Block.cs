using System.Collections;
using UnityEngine;

public class Block : MonoBehaviour
{
    public BlockData data;
    public void AnimationPlacement()
    {
        StartCoroutine(PlacementAnimationRoutine());
    }
    private IEnumerator PlacementAnimationRoutine()
    {
        // Hedef boyutlar
        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = Vector3.one * 1.05f; // y�zde 5 daha b�y�k
        float duration = 0.1f; // animasyon s�resi

        // b�y�me
        float timer = 0f;
        while (timer < duration)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, timer / duration);
            timer += Time.deltaTime;
            yield return null; 
        }
        transform.localScale = targetScale; 

        // k���lme
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
