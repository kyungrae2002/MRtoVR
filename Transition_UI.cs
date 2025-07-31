using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 각 UI 요소의 개별 설정을 위한 클래스입니다.
[System.Serializable]
public class UISequenceElement
{
    [Tooltip("순서에 따라 켜질 UI 게임 오브젝트 (반드시 CanvasGroup 컴포넌트가 있어야 합니다)")]
    public GameObject uiObject;

    // 목표 투명도를 설정하는 변수를 추가합니다.
    [Tooltip("페이드 인 완료 시의 목표 투명도 값입니다.")]
    [Range(0f, 1f)] // 인스펙터에서 0과 1 사이의 슬라이더로 표시됩니다.
    public float targetFadeInAlpha = 1.0f;

    [Tooltip("나타날 때 재생할 사운드 클립")]
    public AudioClip fadeInSound;

    [Tooltip("페이드 인 되는 시간 (초)")]
    public float fadeInDuration = 0.5f;

    [Tooltip("완전히 나타난 상태로 유지되는 시간 (초)")]
    public float displayDuration = 1.0f;

    [Tooltip("페이드 아웃 되는 시간 (초)")]
    public float fadeOutDuration = 0.5f;

    [Tooltip("이 UI가 사라진 후 다음 UI로 넘어가기 전의 대기 시간 (초)")]
    public float delayAfter = 0.2f;
}

public class Transition_UI : MonoBehaviour
{
    // ... (스크립트의 나머지 부분은 이전과 동일합니다) ...
    [Tooltip("개별적으로 설정된 UI 시퀀스 목록")]
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
            Debug.Log("UI 페이드 시퀀스를 시작합니다.");
            sequenceCoroutine = StartCoroutine(RunSequenceRoutine());
        }
    }

    // ... StopSequence, ShowElementBriefly, ShowElementRoutine, FadeRoutine 등 다른 함수는 그대로 ...
    public void StopSequence()
    {
        if (sequenceCoroutine != null)
        {
            Debug.Log("UI 시퀀스를 중지합니다.");
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
        // ... (초기화 부분은 동일) ...
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

        // --- 순차적으로 페이드 인/아웃 되는 시퀀스 ---
        foreach (var element in sequenceElements)
        {
            if (element == null || element.uiObject == null) continue;

            CanvasGroup canvasGroup = element.uiObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null) continue;

            // 1. 사운드를 재생하고 페이드 인합니다.
            if (element.fadeInSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(element.fadeInSound);
            }

            // 페이드 인 목표값을 1f가 아닌, 각 요소에 설정된 targetFadeInAlpha 값으로 변경합니다.
            yield return StartCoroutine(FadeRoutine(canvasGroup, element.targetFadeInAlpha, element.fadeInDuration));

            // 2. 완전히 보이는 상태로 대기합니다.
            yield return new WaitForSeconds(element.displayDuration);

            // 3. 페이드 아웃합니다. (페이드 아웃은 항상 0으로)
            yield return StartCoroutine(FadeRoutine(canvasGroup, 0f, element.fadeOutDuration));

            // 4. 다음 UI 전까지 대기합니다.
            yield return new WaitForSeconds(element.delayAfter);
        }

        Debug.Log("UI 시퀀스가 모두 종료되었습니다.");
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