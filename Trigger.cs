using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger : MonoBehaviour
{
    [SerializeField] private Texture2D skyboxDay;
    [SerializeField] private Texture2D skyboxSunset;

    [SerializeField] private Gradient graddientDayToSunset;
    [SerializeField] private Gradient graddientSunsetToNight;

    [SerializeField] private Light globalLight;

    [SerializeField] private GameObject particlePrefab;

    [SerializeField] private float lifetime = 20;

    private bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // ���� ������Ʈ�� �� �θ� �߿� OVRSkeleton ������Ʈ�� �ִ��� Ȯ���մϴ�.
        if (other != null && other.CompareTag("HandCollider"))
        {

            Debug.Log("���� ��ҽ��ϴ�! ��ƼŬ ����!");

            if (particlePrefab != null)
            {
                Vector3 spawnPosition = other.transform.position;
                GameObject spawnedEffect = Instantiate(particlePrefab, spawnPosition, Quaternion.identity);
                Destroy(spawnedEffect, lifetime);
            }

            if (isTriggered)
            {
                return;
            }

            //Ʈ���Ű� ó�� �ߵ��ϴ� ���� true�� �ٲ� �ٽô� ������� �ʵ��� �Ѵ�.
            isTriggered = true;

            StartCoroutine(LerpSkybox(skyboxDay, skyboxSunset, 5f));
            StartCoroutine(LerpLight(graddientDayToSunset, 5f));
        }
    }

    private IEnumerator LerpSkybox(Texture2D a, Texture2D b, float time)
    {
        RenderSettings.skybox.SetTexture("_Texture1", a);
        RenderSettings.skybox.SetTexture("_Texture2", b);
        RenderSettings.skybox.SetFloat("_Blend", 0);
        for (float i = 0; i < time; i += Time.deltaTime)
        {
            RenderSettings.skybox.SetFloat("_Blend", i / time);
            yield return null;
        }
        RenderSettings.skybox.SetTexture("_Texture1", b);
    }

    private IEnumerator LerpLight(Gradient lightGradient, float time)
    {
        for (float i = 0; i < time; i += Time.deltaTime)
        {
            globalLight.color = lightGradient.Evaluate(i / time);
            RenderSettings.fogColor = globalLight.color;
            yield return null;
        }
    }
}