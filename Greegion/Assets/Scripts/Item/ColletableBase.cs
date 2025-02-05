using System;
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
        transform.position = SnapToGrid(transform.position);
        childRoot = transform.GetChild(0);
        storePosition = transform.position;
    }
    private Vector3 SnapToGrid(Vector3 pos)
    {
        return new Vector3
        {
            x = Mathf.Round(pos.x),
            y = Mathf.Round(pos.y),
            z = Mathf.Round(pos.z)
        };
    }
    private void Update()
    {
        childRoot.Rotate(Vector3.up,rotateRate,Space.World);

        var sinWave = Mathf.Sin(Time.time * 3.1415926f) * floatingHeight;
        transform.position = new Vector3(storePosition.x,storePosition.y + sinWave,storePosition.z);
    }

    public virtual void Collect()
    {
        afterCollectEffect.transform.SetParent(null);
        afterCollectEffect.Play();
        Destroy(gameObject);
    }
}
