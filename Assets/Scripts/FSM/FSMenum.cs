
/**
* @brief FSM state들을 열거하는 enum
* @details state명으로 애니메이션이 재생된다.
* @author Dripmaster, 한글닉이최고
* @date 2020-05-31
* @version 0.0.1
*
*/
public enum PlayerState { 
    idle = 0,
    move,
    attack,
    dead,
    skill,
}
public enum WeaponType { 
    sampleWeapon = 0,
}
public enum SampleWeaponState
{
    idle = 0,
    move,
    attack,
    skill,
    dead,
}
public enum MoveWhileAttack { 
    Move_Attack = 0, // player-move weapon-attack
    Move_Cancel_Attack , //player-move weapon-(attack->move)
    Cannot_Move //player-attack weapon-attack
}