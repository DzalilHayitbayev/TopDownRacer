using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelTrailRendererHandler : MonoBehaviour
{
    public bool isOverpassEmitter = false;

    TopDownCarController topDownCarController;
    TrailRenderer trailRenderer;
    CarLayerHandler carLayerHandler;

    private void Awake()
    {
        topDownCarController = GetComponentInParent<TopDownCarController>();

        carLayerHandler = GetComponentInParent<CarLayerHandler>();

        trailRenderer = GetComponent<TrailRenderer>();
        trailRenderer.emitting = false;
    }

    private void Update()
    {
        trailRenderer.emitting = false;

        if(topDownCarController.IsTireScreeching(out float lateralVelocity, out bool isBraking))
        {
            if (carLayerHandler.IsDrivingOverpass() && isOverpassEmitter)
               trailRenderer.emitting = true;
            
            if(!carLayerHandler.IsDrivingOverpass() && !isOverpassEmitter)
                trailRenderer.emitting = true;
        }
        
    }
}
