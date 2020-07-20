using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;

/**
* @brief 애니메이션 제작 자동화 클래스.
* @details state별로 blend Tree를 만들고, BlendParam을 이용해 방향에 대응한다. 각 state를 순서대로 Transition을 만든다. 속도 멀티플러로 SpeedParam을 생성한다.
* @author Dripmaster, 한글닉이최고
* @date 2020-05-31
* @version 0.0.3
*
*/
public class AnimationMaker : EditorWindow
{
    string FolderName = "";
    string States = "";
    string GetAnims = "";
    bool modifyCondition = false;
    [MenuItem("Cobra/AnimationCreate #_a")]
    static void Init()
    {
        AnimationMaker window = (AnimationMaker)EditorWindow.GetWindow(typeof(AnimationMaker));
        window.Show();
    }
    void OnGUI()
    {
        GUILayout.Label("AnimationCreate", EditorStyles.boldLabel);
        GUILayout.Label("방향과 상태가 있는 오브젝트의 애니메이션을 만듭니다.\n"+
            "Generate AnimationClips and their Controller with States and 8-Directions\n", EditorStyles.helpBox);
        GUILayout.Label("스프라이트경로를 다음과 같이 배치합니다. : Assets/Resources/오브젝트경로/상태/방향/0~n\n" +
            "Assets/Resources/오브젝트 경로/anims 폴더를 만들어 둡니다.\n" +
            "Put sprites in path : Assets/Resources/(objectPath)/(State)/(Direction)/0~n\n" +
            "Make the path Assets/Resources/objectPath/anims for save", EditorStyles.helpBox);
        GUILayout.Label("오브젝트 경로(Object Path):", EditorStyles.miniLabel);
        FolderName = GUILayout.TextField(FolderName, EditorStyles.textField);

        GUILayout.Label("상태(States):", EditorStyles.miniLabel);
        GUILayout.Label("상태들을 열거합니다. ';'로 구분합니다.\n" +
            "Input the states. It will slice with ';'.", EditorStyles.helpBox);
        States = GUILayout.TextField(States, EditorStyles.textField);
        modifyCondition = GUILayout.Toggle(modifyCondition,"Modify Current Anims");


        if (GUILayout.Button("Create Animation(취소 불가Cannot Undo)"))
        {
            CreateAnimation();
        }
        if (GUILayout.Button("Get Current Animation"))
        {
            GetAnims = GetAnimation();
        }
        if(GetAnims.Length>=1)
        GetAnims = GUILayout.TextField(GetAnims, EditorStyles.textField);
    }
    string GetAnimation() {
        AnimatorController controller = Resources.Load<AnimatorController>(FolderName + "/anims/controller");
        string result = "";
        if (controller != null)
        {
            var StateList = controller.layers[0].stateMachine.states;
            foreach (var state in StateList)
            {
                result += (state.state.name)+";";
            }
        }
        else {
            result = "There is no Animaton";
        }
        return result;
    }

    void CreateAnimation()
    {
        Debug.Log("Create Animation Object : " + FolderName);
        string[] stateNames = States.Split(';');
        AnimatorController controller = Resources.Load<AnimatorController>(FolderName + "/anims/controller");
        if (controller == null)
        {
            controller = new AnimatorController();
            AssetDatabase.CreateAsset(controller, "Assets/Resources/" + FolderName + "/anims/controller.controller");
        }
        controller.AddLayer("base");
        controller.AddParameter("State", AnimatorControllerParameterType.Int);
        AnimatorControllerParameter param = new AnimatorControllerParameter();
        param.type = AnimatorControllerParameterType.Float;
        param.defaultFloat = 1f;
        param.name = "SpeedParam";
        controller.AddParameter(param);

        AnimatorControllerParameter blendParam = new AnimatorControllerParameter();
        blendParam.type = AnimatorControllerParameterType.Float;
        blendParam.defaultFloat = 0f;
        blendParam.name = "BlendParam";
        controller.AddParameter(blendParam);

        int count = 0;
        foreach (var item in stateNames)
        {
            BlendTree blendTree;
            AnimatorState aniState = controller.CreateBlendTreeInController(item.Replace("/", "_"),out blendTree);

            blendTree.blendParameter = "BlendParam";
            blendTree.maxThreshold = 0.7f;
            blendTree.minThreshold = 0;

            aniState.speedParameter = "SpeedParam";
            aniState.speedParameterActive = true;

            var transition = controller.layers[0].stateMachine.AddAnyStateTransition(aniState);
            transition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, count, "State");
            transition.duration = 0;
            transition.hasFixedDuration = false;
            transition.hasExitTime = false;
            transition.canTransitionToSelf = false;

            try
            {
                for (int i = 0; i < 8; i++)
                {

                    AnimationClip animClip = Resources.Load<AnimationClip>(FolderName + "/anims/" + item.Replace("/", "_") + "_" + i);
                    if ( animClip!= null && !modifyCondition)
                    {//기존애니메이션이 있을 시 추가 생성 따로 안함
                        
                    }
                    else
                    {//기존 애니메이션이 없을 시 생성
                        Sprite[] sprites = Resources.LoadAll<Sprite>(FolderName + "/" + item + "/" + i);
                        AssetDatabase.CreateAsset(animClip, "Assets/Resources/" + FolderName + "/anims/" + item.Replace("/", "_") + "_" + i + ".anim");
                        animClip.frameRate = 12;   // FPS
                        EditorCurveBinding spriteBinding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");

                        ObjectReferenceKeyframe[] spriteKeyFrames = new ObjectReferenceKeyframe[sprites.Length];
                        foreach (var s in sprites)
                        {
                            int index = int.Parse(s.name);
                            spriteKeyFrames[index] = new ObjectReferenceKeyframe();
                            spriteKeyFrames[index].time = index / animClip.frameRate;
                            spriteKeyFrames[index].value = s;
                        }
                        AnimationUtility.SetObjectReferenceCurve(animClip, spriteBinding, spriteKeyFrames);

                        animClip.name = item.Replace("/", "_");
                        AnimationClipSettings animClipSett = AnimationUtility.GetAnimationClipSettings(animClip);
                        animClipSett.loopTime = true;

                        AnimationUtility.SetAnimationClipSettings(animClip, animClipSett);
                    }
                    blendTree.AddChild(animClip, i * 0.1f);
                }
                Debug.Log("Success, States : " + item + "StateNumber : " + count);
                count++;
            }
            catch
            {
                Debug.Log("Fail, States : " + item);
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

}

