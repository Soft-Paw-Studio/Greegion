// 角色状态枚举
public enum CharacterState
{
    Grounded,
    Jumping,
    Falling,
    WallSlide
}

// 状态接口
public interface ICharacterState
{
    void EnterState();
    void UpdateState();
    void FixedUpdateState();
    void ExitState();
    float GetRotationSpeed();
}