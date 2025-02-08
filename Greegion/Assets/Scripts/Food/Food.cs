using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class Food : MonoBehaviour,IFood
{
    //Parameters
    [SerializeField][Range(0,1)]
    private float calories;
    [SerializeField][Range(0,1)]
    private float rotateRate;
    [SerializeField][Range(0,0.5f)]
    private float floatingHeight;
    [SerializeField] 
    private ParticleSystem onCollectParticle;

    //Store Values
    private Transform childRoot;
    private Vector3 storePosition;

    //GetSet
    public float GetCalories() => calories;
    
    //Unity life cycle
    private void Start()
    {
        transform.SnapToGrid();
        childRoot = transform.GetChild(0);
        storePosition = transform.position;
    }
    
    private void Update()
    {
        childRoot.Rotate(Vector3.up,rotateRate,Space.World);

        var sinWave = Mathf.Sin(Time.time * 3.1415926f) * floatingHeight;
        transform.position = new Vector3(storePosition.x,storePosition.y + sinWave,storePosition.z);
    }
    
    //Functions
    public virtual void Collect()
    {
        var particle = Instantiate(onCollectParticle, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}