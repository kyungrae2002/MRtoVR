using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class Hand_Collider : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private string handTag = "HandCollider";
    [SerializeField] private float requiredHoverTime = 2.0f;
    [SerializeField] private RealityTransition realitySwitcher;

    private Coroutine hoverTimerCoroutine;
    private int handCount = 0;

    void Start()
    {
        if (realitySwitcher == null)
        {
            Debug.LogError("RealitySwitcher가 할당되지 않았습니다.", this.gameObject);
        }
    }

    // 매 프레임 버튼 입력을 감지하기 위한 Update 함수 추가
    void Update()
    {
        // 오른쪽 컨트롤러의 'A' 버튼(OVRInput.Button.One)이 눌렸는지 확인
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            Debug.Log("[Button]: 'A' 버튼이 눌렸습니다. VR 전환을 시도합니다.");
            TriggerTransition();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(handTag))
        {
            handCount++;
            if (handCount > 0 && hoverTimerCoroutine == null)
            {
                Debug.Log($"[Hand Hover]: 손이 진입했습니다. {requiredHoverTime}초 대기 시작...");
                hoverTimerCoroutine = StartCoroutine(StartHoverTimer());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(handTag))
        {
            handCount--;
            if (handCount <= 0 && hoverTimerCoroutine != null)
            {
                Debug.Log("[Hand Hover]: 모든 손이 벗어났습니다. 타이머 초기화.");
                StopCoroutine(hoverTimerCoroutine);
                hoverTimerCoroutine = null;
                handCount = 0;
            }
        }
    }

    private IEnumerator StartHoverTimer()
    {
        yield return new WaitForSeconds(requiredHoverTime);

        Debug.LogWarning($"[Hand Hover SUCCESS]: 손이 {requiredHoverTime}초 이상 머물렀습니다!");
        TriggerTransition(); // 호버 성공 시에도 동일한 전환 함수 호출
        hoverTimerCoroutine = null;
    }

    //  호버와 버튼 입력 모두 이 함수를 호출하도록 통합
    private void TriggerTransition()
    {
        // 전환 중이거나 이미 VR 모드일 때는 중복 실행 방지
        if (realitySwitcher != null && !realitySwitcher.IsSwitching() && !realitySwitcher.IsVREnabled())
        {
            realitySwitcher.SwitchToVR();
        }
    }
}