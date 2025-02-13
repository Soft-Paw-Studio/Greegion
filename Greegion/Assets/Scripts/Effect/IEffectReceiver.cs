public interface IEffectReceiver
{
    void ApplyEffect(EffectType effectType, float intensity, float duration);
    void RemoveEffect(EffectType effectType);
}