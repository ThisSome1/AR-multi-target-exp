using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ImageTracker : MonoBehaviour
{
    [Serializable]
    public struct ARTarget
    {
        public string name;
        public GameObject prefab;
    }

    [SerializeField] int maximumNumberOfARObjects = 10;
    [SerializeField] ARTarget[] arTargets;

    ARTrackedImageManager trackedImageManager;
    readonly Dictionary<int, GameObject> arObjects = new();
    readonly Dictionary<string, GameObject> arPrefabs = new();

    void Awake()
    {
        foreach (ARTarget arTarget in arTargets)
            arPrefabs.Add(arTarget.name, arTarget.prefab);
        trackedImageManager = GetComponent<ARTrackedImageManager>();
    }
    void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }
    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        // Create objects based on image tracked
        foreach (ARTrackedImage addedImage in args.added)
            UpdateArObject(addedImage.GetInstanceID(), addedImage.referenceImage.name, addedImage.transform.position, addedImage.transform.rotation);

        // Update tracking position
        foreach (ARTrackedImage updatedImage in args.updated)
            if (updatedImage.trackingState == TrackingState.Limited)
                RemoveArObject(updatedImage.GetInstanceID());
            else if (updatedImage.trackingState == TrackingState.Tracking)
                UpdateArObject(updatedImage.GetInstanceID(), updatedImage.referenceImage.name, updatedImage.transform.position, updatedImage.transform.rotation);
    }

    // Safely update or spawn an object for a tracked image
    void UpdateArObject(int id, string name, Vector3 position, Quaternion rotation)
    {
        // Check if the prefab exists
        if (!arPrefabs.ContainsKey(name ?? "")) return;


        // Check if prefab is already spawned
        if (arObjects.ContainsKey(id))
            arObjects[id].transform.SetPositionAndRotation(position, rotation);
        else if (arObjects.Count < maximumNumberOfARObjects)
        {
            Debug.Log($"Tracking added with name: {name}");
            arObjects.Add(id, Instantiate(arPrefabs[name], position, rotation));
        }
    }

    // Safely remove a object of a tracked image
    void RemoveArObject(int id)
    {
        // Check if the objcet exists
        if (arObjects.ContainsKey(id))
        {
            Debug.Log($"Tracking lost with name: {arObjects[id].name}");
            Destroy(arObjects[id]);
            arObjects.Remove(id);
        }
    }
}
