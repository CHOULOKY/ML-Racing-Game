using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Policies;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MainCamera : MonoBehaviour
{
    public bool isTraining = false;
    public bool isObserver = true;

    public AnimalAgent[] animals;
    public int curAnimalIndex;

    private Transform targetTransform;
    public Vector3 offsetPosition;
    public float targetDistance;
    public float damping = 5.0f;    // 카메라 이동 부드러움
    public float rotationDamping = 3.0f; // 카메라 회전 부드러움

    public GameObject[] courseCameras;
    public int curCameraIndex;

    private void Awake()
    {
        animals = new AnimalAgent[4];
        foreach (AnimalAgent agent in FindObjectsByType<AnimalAgent>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) {
            animals[agent.animalNumber] = agent;
        }
    }

    private void Start()
    {
        curAnimalIndex = 0;

        targetTransform = animals[0].transform;
    }

    private void Update()
    {
        if (isTraining) {
            courseCameras[curCameraIndex].SetActive(true);
            return;
        }

        if (Input.GetKeyDown(KeyCode.BackQuote)) {
            isObserver = !isObserver;
        }

        if (isObserver) {
            if (Input.GetKeyDown(KeyCode.Alpha0)) {
                courseCameras[curCameraIndex].SetActive(true);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1)) {
                courseCameras[curCameraIndex].SetActive(false);
                curAnimalIndex = 0;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2)) {
                courseCameras[curCameraIndex].SetActive(false);
                curAnimalIndex = 1;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3)) {
                courseCameras[curCameraIndex].SetActive(false);
                curAnimalIndex = 2;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4)) {
                courseCameras[curCameraIndex].SetActive(false);
                curAnimalIndex = 3;
            }
        }
        else {
            courseCameras[curCameraIndex].SetActive(false);
            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                animals[curAnimalIndex].gameObject.GetComponent<BehaviorParameters>().BehaviorType = BehaviorType.InferenceOnly;
                curAnimalIndex = 0;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2)) {
                animals[curAnimalIndex].gameObject.GetComponent<BehaviorParameters>().BehaviorType = BehaviorType.InferenceOnly;
                curAnimalIndex = 1;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3)) {
                animals[curAnimalIndex].gameObject.GetComponent<BehaviorParameters>().BehaviorType = BehaviorType.InferenceOnly;
                curAnimalIndex = 2;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4)) {
                animals[curAnimalIndex].gameObject.GetComponent<BehaviorParameters>().BehaviorType = BehaviorType.InferenceOnly;
                curAnimalIndex = 3;
            }
            animals[curAnimalIndex].gameObject.GetComponent<BehaviorParameters>().BehaviorType = BehaviorType.HeuristicOnly;
        }
    }

    private void LateUpdate()
    {
        if (targetTransform == null || isTraining) return;

        targetTransform = animals[curAnimalIndex].transform;

        Vector3 targetPosition = targetTransform.position - targetTransform.forward * targetDistance + offsetPosition;
        transform.position = Vector3.Lerp(transform.position, targetPosition, damping * Time.deltaTime);

        transform.LookAt(targetTransform.position + Vector3.up * 1.5f);
    }
}
