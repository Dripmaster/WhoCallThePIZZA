/**
* @brief FSM state들을 열거하는 enum
* @details state명으로 애니메이션이 재생된다.
* @author Dripmaster, 한글닉이최고
* @date 2020-05-31
* @version 0.0.1
*
*/
public enum PlayerState { //플레이어와 무기의 스테이트(고유 번호 고정)
    idle = 0,
    move,
    attack,
    dead,
    skill,
    dash,
}
public enum WeaponType { //무기목록
    sampleWeapon = 0,
}
public enum MoveWhileAttack { 
    Move_Attack = 0, // player-move weapon-attack
    Move_Cancel_Attack , //player-move weapon-(attack->move)
    Cannot_Move //player-attack weapon-attack
}