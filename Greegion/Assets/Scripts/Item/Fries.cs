using UnityEngine;

public class Fries : ColletableBase
{
    public override void Collect()
    {
        PigeonGUIController.EatFriesEvent.Invoke();
        base.Collect();
    }
}
