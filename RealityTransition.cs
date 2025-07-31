using UnityEngine;
using System.Collections;
using JetBrains.Annotations;

public class RealityTransition : MonoBehaviour
{
    [Header("필수 컴포넌트")]
    [SerializeField] private GameObject virtualEnvironment;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transition_UI uiSequencer;

    [Header("환경 초기화 설정")]
    [SerializeField] private Texture2D skyboxDay;
    [SerializeField] private Texture2D skyboxSunset;
    [SerializeField] private Gradient graddientDayToSunset;
    [SerializeField] private Light globalLight;

    [Header("타이머 및 UI 설정")]
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
        hasUiSequenceStarted = false; // MR 모드로 돌아오면 UI 시퀀스 플래그도 리셋
        Debug.Log("초기 상태: MR 모드로 시작합니다.");
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

        // isVREnabled는 SetupMRMode 또는 여기서 직접 설정해도 되지만,
        // 명확성을 위해 여기서 한 번 더 설정해 줍니다.
        isVREnabled = switchToVR;

        // 타이머는 매 전환마다 0으로 리셋
        elapsedTime = 0f;

        OVRScreenFade.instance.FadeIn();
        yield return new WaitForSeconds(OVRScreenFade.instance.fadeTime);
        Debug.Log(isVREnabled ? "VR 모드로 전환 완료." : "MR 모드로 전환 완료.");
        isSwitching = false;
    }

    //  환경 초기화 로직을 별도 함수로 분리
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
        Debug.Log("VR 환경(하늘, 조명)을 초기 상태로 설정했습니다.");
    }
}