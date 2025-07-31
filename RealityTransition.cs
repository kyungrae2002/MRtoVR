using UnityEngine;
using System.Collections;
using JetBrains.Annotations;

public class RealityTransition : MonoBehaviour
{
    [Header("�ʼ� ������Ʈ")]
    [SerializeField] private GameObject virtualEnvironment;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transition_UI uiSequencer;

    [Header("ȯ�� �ʱ�ȭ ����")]
    [SerializeField] private Texture2D skyboxDay;
    [SerializeField] private Texture2D skyboxSunset;
    [SerializeField] private Gradient graddientDayToSunset;
    [SerializeField] private Light globalLight;

    [Header("Ÿ�̸� �� UI ����")]
    [SerializeField] private float uiSequenceDelay = 5.0f;
    [SerializeField] private float mrReturnTime = 60.0f;
    [SerializeField] private float elapsedTime;

    private bool isSwitching = false;
    private bool isVREnabled = false;
    private bool hasUiSequenceStarted = false;

    void Start()
    {
        SetupMRMode();
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;
        if (isVREnabled && !hasUiSequenceStarted && elapsedTime >= uiSequenceDelay)
        {
            if (uiSequencer != null)
            {
                uiSequencer.StartSequence();
                hasUiSequenceStarted = true;
            }
        }
        if (isVREnabled && elapsedTime >= mrReturnTime)
        {
            SwitchToMR();
        }
    }
    private void SetupMRMode()
    {
        OVRManager.instance.isInsightPassthroughEnabled = true;
        if (virtualEnvironment != null) virtualEnvironment.SetActive(false);

        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.backgroundColor = Color.clear;

        isVREnabled = false;
        hasUiSequenceStarted = false; // MR ���� ���ƿ��� UI ������ �÷��׵� ����
        Debug.Log("�ʱ� ����: MR ���� �����մϴ�.");
    }

    public bool IsVREnabled() { return isVREnabled; }
    public bool IsSwitching() { return isSwitching; }

    public void SwitchToVR()
    {
        if (isSwitching || isVREnabled) return;
        StartCoroutine(TransitionTo(true));
    }

    public void SwitchToMR()
    {
        if (isSwitching || !isVREnabled) return;
        StartCoroutine(TransitionTo(false));
    }

    private IEnumerator TransitionTo(bool switchToVR)
    {
        isSwitching = true;
        OVRScreenFade.instance.FadeOut();
        yield return new WaitForSeconds(OVRScreenFade.instance.fadeTime);

        OVRManager.instance.isInsightPassthroughEnabled = !switchToVR;
        if (virtualEnvironment != null)
        {
            virtualEnvironment.SetActive(switchToVR);
        }

        if (switchToVR)
        {
            mainCamera.clearFlags = CameraClearFlags.Skybox;
            InitializeEnvironment();
        }
        else
        {
            SetupMRMode();
        }

        // isVREnabled�� SetupMRMode �Ǵ� ���⼭ ���� �����ص� ������,
        // ��Ȯ���� ���� ���⼭ �� �� �� ������ �ݴϴ�.
        isVREnabled = switchToVR;

        // Ÿ�̸Ӵ� �� ��ȯ���� 0���� ����
        elapsedTime = 0f;

        OVRScreenFade.instance.FadeIn();
        yield return new WaitForSeconds(OVRScreenFade.instance.fadeTime);
        Debug.Log(isVREnabled ? "VR ���� ��ȯ �Ϸ�." : "MR ���� ��ȯ �Ϸ�.");
        isSwitching = false;
    }

    //  ȯ�� �ʱ�ȭ ������ ���� �Լ��� �и�
    private void InitializeEnvironment()
    {
        if (RenderSettings.skybox != null)
        {
            RenderSettings.skybox.SetTexture("_Texture1", skyboxDay);
            RenderSettings.skybox.SetTexture("_Texture2", skyboxSunset);
            RenderSettings.skybox.SetFloat("_Blend", 0);
        }
        if (globalLight != null && graddientDayToSunset != null)
        {
            globalLight.color = graddientDayToSunset.Evaluate(0f);
            RenderSettings.fogColor = globalLight.color;
        }
        Debug.Log("VR ȯ��(�ϴ�, ����)�� �ʱ� ���·� �����߽��ϴ�.");
    }
}