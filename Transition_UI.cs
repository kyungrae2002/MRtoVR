using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// �� UI ����� ���� ������ ���� Ŭ�����Դϴ�.
[System.Serializable]
public class UISequenceElement
{
    [Tooltip("������ ���� ���� UI ���� ������Ʈ (�ݵ�� CanvasGroup ������Ʈ�� �־�� �մϴ�)")]
    public GameObject uiObject;

    // ��ǥ ������ �����ϴ� ������ �߰��մϴ�.
    [Tooltip("���̵� �� �Ϸ� ���� ��ǥ ���� ���Դϴ�.")]
    [Range(0f, 1f)] // �ν����Ϳ��� 0�� 1 ������ �����̴��� ǥ�õ˴ϴ�.
    public float targetFadeInAlpha = 1.0f;

    [Tooltip("��Ÿ�� �� ����� ���� Ŭ��")]
    public AudioClip fadeInSound;

    [Tooltip("���̵� �� �Ǵ� �ð� (��)")]
    public float fadeInDuration = 0.5f;

    [Tooltip("������ ��Ÿ�� ���·� �����Ǵ� �ð� (��)")]
    public float displayDuration = 1.0f;

    [Tooltip("���̵� �ƿ� �Ǵ� �ð� (��)")]
    public float fadeOutDuration = 0.5f;

    [Tooltip("�� UI�� ����� �� ���� UI�� �Ѿ�� ���� ��� �ð� (��)")]
    public float delayAfter = 0.2f;
}

public class Transition_UI : MonoBehaviour
{
    // ... (��ũ��Ʈ�� ������ �κ��� ������ �����մϴ�) ...
    [Tooltip("���������� ������ UI ������ ���")]
    public List<UISequenceElement> sequenceElements;

    private Coroutine sequenceCoroutine;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void StartSequence()
    {
        if (sequenceCoroutine == null)
        {
            Debug.Log("UI ���̵� �������� �����մϴ�.");
            sequenceCoroutine = StartCoroutine(RunSequenceRoutine());
        }
    }

    // ... StopSequence, ShowElementBriefly, ShowElementRoutine, FadeRoutine �� �ٸ� �Լ��� �״�� ...
    public void StopSequence()
    {
        if (sequenceCoroutine != null)
        {
            Debug.Log("UI �������� �����մϴ�.");
            StopCoroutine(sequenceCoroutine);
            sequenceCoroutine = null;

            foreach (var element in sequenceElements)
            {
                if (element != null && element.uiObject != null)
                {
                    CanvasGroup cg = element.uiObject.GetComponent<CanvasGroup>();
                    if (cg != null)
                    {
                        cg.alpha = 0f;
                        cg.interactable = false;
                    }
                }
            }
        }
    }


    private IEnumerator RunSequenceRoutine()
    {
        // ... (�ʱ�ȭ �κ��� ����) ...
        foreach (var element in sequenceElements)
        {
            if (element != null && element.uiObject != null)
            {
                CanvasGroup cg = element.uiObject.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.alpha = 0f;
                    cg.interactable = false;
                }
            }
        }

        // --- ���������� ���̵� ��/�ƿ� �Ǵ� ������ ---
        foreach (var element in sequenceElements)
        {
            if (element == null || element.uiObject == null) continue;

            CanvasGroup canvasGroup = element.uiObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null) continue;

            // 1. ���带 ����ϰ� ���̵� ���մϴ�.
            if (element.fadeInSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(element.fadeInSound);
            }

            // ���̵� �� ��ǥ���� 1f�� �ƴ�, �� ��ҿ� ������ targetFadeInAlpha ������ �����մϴ�.
            yield return StartCoroutine(FadeRoutine(canvasGroup, element.targetFadeInAlpha, element.fadeInDuration));

            // 2. ������ ���̴� ���·� ����մϴ�.
            yield return new WaitForSeconds(element.displayDuration);

            // 3. ���̵� �ƿ��մϴ�. (���̵� �ƿ��� �׻� 0����)
            yield return StartCoroutine(FadeRoutine(canvasGroup, 0f, element.fadeOutDuration));

            // 4. ���� UI ������ ����մϴ�.
            yield return new WaitForSeconds(element.delayAfter);
        }

        Debug.Log("UI �������� ��� ����Ǿ����ϴ�.");
        sequenceCoroutine = null;
    }

    private IEnumerator FadeRoutine(CanvasGroup canvasGroup, float targetAlpha, float duration)
    {
        if (targetAlpha > 0)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

        if (targetAlpha == 0)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}