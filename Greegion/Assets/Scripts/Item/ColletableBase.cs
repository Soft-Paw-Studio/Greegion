using System;
using Unity.Mathematics;
using UnityEngine;

public abstract class ColletableBase : MonoBehaviour,ICollectable
{
    public ParticleSystem afterCollectEffect;
    public float rotateRate;
    public float floatingHeight;

    private Transform childRoot;
    private Vector3 storePosition;

    private void Start()
    {
        //transform.position.SnapToGrid();
        childRoot = transform.GetChild(0);
        storePosition = transform.position;
    }
    
    private void Update()
    {
        childRoot.Rotate(Vector3.up,rotateRate,Space.World);

        var sinWave = Mathf.Sin(Time.time * 3.1415926f) * floatingHeight;
        transform.position = new Vector3(storePosition.x,storePosition.y + sinWave,storePosition.z);
    }

    public virtual void Collect()
    {
        var particle = Instantiate(afterCollectEffect, transform.position, quaternion.identity);
        particle.Play();
        Destroy(gameObject);
    }
}
