using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class Hand_Collider : MonoBehaviour
{
    [Header("����")]
    [SerializeField] private string handTag = "HandCollider";
    [SerializeField] private float requiredHoverTime = 2.0f;
    [SerializeField] private RealityTransition realitySwitcher;

    private Coroutine hoverTimerCoroutine;
    private int handCount = 0;

    void Start()
    {
        if (realitySwitcher == null)
        {
            Debug.LogError("RealitySwitcher�� �Ҵ���� �ʾҽ��ϴ�.", this.gameObject);
        }
    }

    // �� ������ ��ư �Է��� �����ϱ� ���� Update �Լ� �߰�
    void Update()
    {
        // ������ ��Ʈ�ѷ��� 'A' ��ư(OVRInput.Button.One)�� ���ȴ��� Ȯ��
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            Debug.Log("[Button]: 'A' ��ư�� ���Ƚ��ϴ�. VR ��ȯ�� �õ��մϴ�.");
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
                Debug.Log($"[Hand Hover]: ���� �����߽��ϴ�. {requiredHoverTime}�� ��� ����...");
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
                Debug.Log("[Hand Hover]: ��� ���� ������ϴ�. Ÿ�̸� �ʱ�ȭ.");
                StopCoroutine(hoverTimerCoroutine);
                hoverTimerCoroutine = null;
                handCount = 0;
            }
        }
    }

    private IEnumerator StartHoverTimer()
    {
        yield return new WaitForSeconds(requiredHoverTime);

        Debug.LogWarning($"[Hand Hover SUCCESS]: ���� {requiredHoverTime}�� �̻� �ӹ������ϴ�!");
        TriggerTransition(); // ȣ�� ���� �ÿ��� ������ ��ȯ �Լ� ȣ��
        hoverTimerCoroutine = null;
    }

    //  ȣ���� ��ư �Է� ��� �� �Լ��� ȣ���ϵ��� ����
    private void TriggerTransition()
    {
        // ��ȯ ���̰ų� �̹� VR ����� ���� �ߺ� ���� ����
        if (realitySwitcher != null && !realitySwitcher.IsSwitching() && !realitySwitcher.IsVREnabled())
        {
            realitySwitcher.SwitchToVR();
        }
    }
}