using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;
using JetBrains.Annotations;
using System.Runtime.InteropServices;

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
    public static string FolderName = "";
    string newControllerName = "";
    string States = "";
    string newStateMachine = "";
    int directionCount;
    List<MotionClip> spriteLists = new List<MotionClip>();
    Vector2 scrollPoint;
    public static AnimatorController targetController;
    public static Color notSavedColor = new Color(.5f, .8f, .5f);
    public static  bool notSaved = false; 
    public static EditorCurveBinding spriteBinding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
    bool getAnims= false;

    bool WeaponNumberUse = false;

    int weaponNumber = 0;
    int stateNumber = 0;
    int comboCount = 0;
    int parentNumber = 0;
    bool oneShot;

    string targetClipName = "";
    string[] popups;


    [MenuItem("Cobra/AnimationCreate #_a"), CanEditMultipleObjects]
    static void Init()
    {
        AnimationMaker window = (AnimationMaker)EditorWindow.GetWindow(typeof(AnimationMaker));
        window.Show();
    }
    void OnGUI()
    {
        GUILayout.Label("AnimationCreate", EditorStyles.boldLabel);
        GUILayout.Label("방향과 상태가 있는 오브젝트의 애니메이션을 만듭니다.\n" +
            "Generate AnimationClips and their Controller with States and Directions\n", EditorStyles.helpBox);
        /*
        GUILayout.Label("스프라이트경로를 다음과 같이 배치합니다. : Assets/Resources/오브젝트경로/상태/방향/0~n\n" +
            "Assets/Resources/오브젝트 경로/anims 폴더를 만들어 둡니다.\n" +
            "Put sprites in path : Assets/Resources/(objectPath)/(State)/(Direction)/0~n\n" +
            "Make the path Assets/Resources/objectPath/anims for save", EditorStyles.helpBox);
        GUILayout.Label("오브젝트 경로(Object Path):", EditorStyles.miniLabel);
        FolderName = GUILayout.TextField(FolderName, EditorStyles.textField);
        

        GUILayout.Label("상태(States):", EditorStyles.miniLabel);
        GUILayout.Label("상태들을 열거합니다. ';'로 구분합니다.\n" +
            "Input the states. It will slice with ';'.", EditorStyles.helpBox);
            */
        

        GUILayout.Label("방향스프라이트 갯수:", EditorStyles.miniLabel);
        if(!targetController)
        directionCount = EditorGUILayout.IntSlider( directionCount,1,8);
        else
        {
            EditorGUILayout.IntSlider(directionCount, 1, 8);
        }
        GUILayout.Label("컨트롤러(새로 생성 시 비워둠):", EditorStyles.miniLabel);
        var target = EditorGUILayout.ObjectField(targetController, typeof(AnimatorController), true)
            as AnimatorController;
        if(target != targetController)
        {
            targetController = target;
            getAnims = false;
        }
        if(!targetController)
            newControllerName = EditorGUILayout.TextField("새 컨트롤러 이름:Resource/", newControllerName);
        if (!targetController&& GUILayout.Button("Create Animator"))
        {
            CreateAnimator(newControllerName);
        }
        scrollPoint = EditorGUILayout.BeginScrollView(scrollPoint);
        if(targetController && !getAnims)
        {
            GetAnimation();
            getAnims = true;
            FolderName = AssetDatabase.GetAssetPath(targetController).Split('.')[0];
        }
        else if (!targetController)
        {
            getAnims = false;
        }
        if(targetController)
        foreach (var item in spriteLists)
        {
            item.show();
        }
        EditorGUILayout.EndScrollView();
        
        GUILayout.Label("부모 상태이름:", EditorStyles.miniLabel);
        newStateMachine = GUILayout.TextField(newStateMachine, EditorStyles.textField);
        if (GUILayout.Button("새 부모상태 생성"))
        {
            targetController.layers[0].stateMachine.AddStateMachine(newStateMachine);
            GetAnimation();
        }

        if (GUILayout.Button("새 상태 생성"))
        {
            AnimatorStateMachine machine;
            if (popups[parentNumber] == "Default")
            {
                machine = targetController.layers[0].stateMachine;
            }
            else
            {
                machine = spriteLists[parentNumber].myStateMachine;
            }

            MotionClip m = new MotionClip(null,machine,null,targetClipName);
            m.stateNumber = stateNumber;
            m.weaponNumber = weaponNumber;
            m.comboCount = comboCount;
            m.oneShot = oneShot;
            m.state = m.parent.AddState(targetClipName);
            m.setTransition();
            m.state.speedParameterActive = true;
            m.state.speedParameter = "SpeedParam";
            spriteLists.Add(m);
            GetAnimation();
        }
        GUILayout.Label("새 상태 설정", EditorStyles.miniLabel);
        stateNumber = EditorGUILayout.IntField("상태번호", stateNumber);
        WeaponNumberUse = GUILayout.Toggle(WeaponNumberUse,"무기번호 사용");
        if (WeaponNumberUse)
        {
            weaponNumber = EditorGUILayout.IntField("무기번호",weaponNumber);
        }
        comboCount = EditorGUILayout.IntField("구분번호",comboCount);
        oneShot = EditorGUILayout.Toggle("OneShot", oneShot);
        GUILayout.Label("상태이름:", EditorStyles.miniLabel);
        targetClipName = GUILayout.TextField(targetClipName, EditorStyles.textField);

        GUILayout.Label("부모 상태", EditorStyles.miniLabel);
        if (popups != null)
        parentNumber = EditorGUILayout.Popup(parentNumber, popups);
        if (notSaved)
        GUI.color = notSavedColor;
        if(GUILayout.Button("모든 변경사항 저장하기"))
        {
            foreach (var item in spriteLists)
            {
                item.save();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            GetAnimation();
            notSaved = false;
        }
        
    }
    void GetAnimation()
    {
        spriteLists = new List<MotionClip>();
        string result = "";
        int viewCount = 1;
        if (targetController != null)
        {
            var StateList = targetController.layers[0].stateMachine.stateMachines;
            foreach (var state in StateList)
            {
                var motionList = state.stateMachine.states;
                result += state.stateMachine.name+" : ";
                MotionClip parent = new MotionClip(null, targetController.layers[0].stateMachine,null, state.stateMachine.name);
                parent.myStateMachine = state.stateMachine;
                foreach (var motion in motionList)
                {
                    result += (motion.state.name) + ";";

                    var clip = motion.state.motion as AnimationClip;
                    MotionClip m = new MotionClip(motion.state.motion, state.stateMachine, motion.state, motion.state.name);
                    m.direction = 0;
                   
                    
                    if (clip)
                    {
                        var aniSprites = AnimationUtility.GetObjectReferenceCurve(clip, spriteBinding);

                        m.frameRate = (int)clip.frameRate;
                        if (aniSprites != null)
                        {
                            List<Sprite> spList = new List<Sprite>();
                            foreach (var sprite in aniSprites)
                            {
                                Sprite s = sprite.value as Sprite;
                                if (s != null)
                                {
                                    spList.Add(s);
                                }
                            }

                            if (spList.Count > 0)
                            {
                                m.sprites = spList;
                            }
                        }
                        else
                        {
                        }
                    }
                    parent.addChild(m);
                }
                spriteLists.Add(parent);
                result += "\n";
            }
            var StateList2 = targetController.layers[0].stateMachine.states;
            foreach (var state in StateList2)
            {
                result += state.state.name + ";";
                var clip = state.state.motion as AnimationClip;

                MotionClip m = new MotionClip(state.state.motion, targetController.layers[0].stateMachine,state.state, state.state.name);
                m.direction = 0;
                if (clip)
                {
                    var aniSprites = AnimationUtility.GetObjectReferenceCurve(clip, spriteBinding);
                    if (aniSprites != null)
                    {
                        m.frameRate = (int)clip.frameRate;
                        List<Sprite> spList = new List<Sprite>();
                        foreach (var sprite in aniSprites)
                        {
                            Sprite s = sprite.value as Sprite;
                            if (s != null)
                            {
                                spList.Add(s);
                            }
                        }
                        if (spList.Count > 0)
                        {
                            m.sprites = spList;
                        }
                    }
                    else
                    {
                    }
                }
                spriteLists.Add(m);
            }
            foreach (var param in targetController.parameters)
            {
                if(param.name == "MaxView")
                {

                    viewCount = param.defaultInt;
                }
            }

            popups = new string[spriteLists.Count + 1];
            int i = 0;
            foreach (var item in spriteLists)
            {
                if (spriteLists[i].myStateMachine)
                {
                    popups[i] = spriteLists[i].name;
                    i++;
                }
            }
            popups[i] = "Default";
        }
        else
        {
            result = "There is no Animaton";
        }
        States = result;
        directionCount = viewCount;
    }
    void CreateAnimator(string path)
    {
        targetController = new AnimatorController();
        try
        {
            AssetDatabase.CreateAsset(targetController, "Assets/Resource/" + path + ".controller");
        }
        catch
        {
            Debug.Log("path Error!! 경로를 확인해 주세요");
            targetController = null;
            return;
        }
        targetController.AddLayer("base");
        AnimatorControllerParameter param = new AnimatorControllerParameter();
        param.type = AnimatorControllerParameterType.Float;
        param.defaultFloat = 1f;
        param.name = "SpeedParam";
        targetController.AddParameter(param);
        targetController.AddParameter("State", AnimatorControllerParameterType.Int);
        targetController.AddParameter("WeaponNumber", AnimatorControllerParameterType.Int);
        targetController.AddParameter("ComboCount", AnimatorControllerParameterType.Int);
        targetController.AddParameter("OneShot", AnimatorControllerParameterType.Trigger);
        AnimatorControllerParameter maxViewParam = new AnimatorControllerParameter();
        maxViewParam.type = AnimatorControllerParameterType.Int;
        maxViewParam.defaultInt =directionCount;
        maxViewParam.name = "maxView";
        targetController.AddParameter(maxViewParam);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
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
            AnimatorState aniState = controller.CreateBlendTreeInController(item.Replace("/", "_"), out blendTree);

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
                    if (animClip != null)
                    {//기존애니메이션이 있을 시 추가 생성 따로 안함

                    }
                    else
                    {//기존 애니메이션이 없을 시 생성
                        Sprite[] sprites = Resources.LoadAll<Sprite>(FolderName + "/" + item + "/" + i);
                        AssetDatabase.CreateAsset(animClip, "Assets/Resources/" + FolderName + "/anims/" + item.Replace("/", "_") + "_" + i + ".anim");
                        animClip.frameRate = 12;   // FPS
                        

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
    public static Object[] DropAreaGUI(string state,int view)
    {
        Event evt = Event.current;
        Rect drop_area = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(drop_area, state+","+"direction:"+view+"\nSprites/Animation Drag & Drop here",
           GUI.skin.button);
        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!drop_area.Contains(evt.mousePosition))
                    return null;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    return DragAndDrop.objectReferences;
                }
                break;
        }
        return null;
    }
    class MotionClip
    {
        public int direction;
        public int frameRate;
        public int stateNumber;
        public int weaponNumber;
        public int comboCount;
        public Motion motion;
        public string name;
        public List<Sprite> sprites;
        public List<MotionClip> childs = new List<MotionClip>();
        public AnimatorStateMachine parent;
        public AnimatorStateMachine myStateMachine;
        public AnimatorState state;
        public bool oneShot;

        public bool needSave = false;

        bool foldout;
        bool foldoutInner = false;
        AnimatorStateTransition tran;

        public MotionClip(Motion m,AnimatorStateMachine parent, AnimatorState state, string name)
        {
            motion = m;
            this.parent = parent;
            this.state = state;
            this.name = name;
            var trans = targetController.layers[0].stateMachine.anyStateTransitions;
            if (state)
            {
                var clip = (state.motion as AnimationClip);
                if (clip)
                    frameRate = (int)clip.frameRate;
            }
            foreach (var item in trans)
            {
                if(item.destinationState == state)
                {
                    tran = item;
                    var cons = item.conditions;
                    foreach (var con in cons)
                    {
                        if (con.parameter == "WeaponNumber")
                            weaponNumber = (int)con.threshold;

                        if (con.parameter == "ComboCount")
                            comboCount = (int)con.threshold;

                        if (con.parameter == "State")
                            stateNumber = (int)con.threshold;
                        if (con.parameter == "OneShot")
                        {
                            oneShot = true;
                        }
                    }
                }
            }
        }
        public void addChild(MotionClip c)
        {
            childs.Add(c);
        }
        public void show()
        {
            if (needSave)
            {
                notSaved = true;
            }
            if (foldout = EditorGUILayout.Foldout(foldout, name,true))
            {
                if (myStateMachine)
                {

                }
                else
                {
                    var dragged = DropAreaGUI(name, direction);
                    if (foldoutInner = EditorGUILayout.Foldout(foldoutInner, "Sprites", true))
                    {
                        if (sprites != null)
                        {
                            for (int i = 0; i < sprites.Count; i++)
                            {
                                sprites[i] = EditorGUILayout.ObjectField(sprites[i].name, sprites[i], typeof(Sprite), true) as Sprite;
                            }
                        }
                    }
                    if (dragged != null)
                    {
                        needSave = true;
                        if (dragged[0] as Sprite)
                        {
                            sprites = new List<Sprite>();
                            foreach (var item in dragged)
                            {
                                Sprite s = item as Sprite;
                                if (s)
                                {
                                    sprites.Add(s);
                                }
                            }
                            if (sprites.Count > 0)
                            {
                                AnimationClip animClip = new AnimationClip();
                                AssetDatabase.CreateAsset(animClip, FolderName + name + ".anim");
                                if (frameRate <= 0)
                                    frameRate = 12;
                                animClip.frameRate = frameRate;   // FPS

                                ObjectReferenceKeyframe[] spriteKeyFrames = new ObjectReferenceKeyframe[sprites.Count];

                                int index = 0;
                                foreach (var s in sprites)
                                {
                                    spriteKeyFrames[index] = new ObjectReferenceKeyframe();
                                    spriteKeyFrames[index].time = index / animClip.frameRate;
                                    spriteKeyFrames[index].value = s;
                                    index++;
                                }
                                AnimationUtility.SetObjectReferenceCurve(animClip, spriteBinding, spriteKeyFrames);

                                animClip.name = name;
                                AnimationClipSettings animClipSett = AnimationUtility.GetAnimationClipSettings(animClip);
                                animClipSett.loopTime = true;

                                AnimationUtility.SetAnimationClipSettings(animClip, animClipSett);

                                motion = animClip;

                                
                            }
                        }
                        
                    }
                    var m = motion;
                    motion = EditorGUILayout.ObjectField(parent.name+":"+name, motion, typeof(Motion), true) as Motion;
                    if (motion != m)
                        needSave = true;
                    int sum = frameRate + stateNumber + weaponNumber + comboCount;
                    frameRate = EditorGUILayout.IntField("FrameRate", frameRate);
                    stateNumber = EditorGUILayout.IntField("상태번호", stateNumber);

                    comboCount = EditorGUILayout.IntField("구분", comboCount);
                    oneShot = EditorGUILayout.Toggle("OneShot",oneShot);
                    weaponNumber = EditorGUILayout.IntField("무기번호", weaponNumber);
                    if (sum != frameRate + stateNumber + weaponNumber + comboCount)
                        needSave = true;
                }
                foreach (var item in childs)
                {
                    item.show();
                    EditorGUILayout.Space();
                }
            }
            EditorGUILayout.Space();
        }
        public void setTransition()
        {
            if (tran == null)
            {
            tran = targetController.layers[0].stateMachine.AddAnyStateTransition(state);
            tran.duration = 0;
            tran.hasFixedDuration = false;
            tran.hasExitTime = false;
            tran.canTransitionToSelf = false;
            }

            AnimatorCondition animatorCondition = new AnimatorCondition();
            animatorCondition.mode = AnimatorConditionMode.Equals;
            animatorCondition.parameter = "WeaponNumber";
            animatorCondition.threshold = weaponNumber;

            var cons = tran.conditions;
            for (int i = 0; i < cons.Length; i++)
            {
                tran.RemoveCondition(cons[i]);
            }
            tran.AddCondition(AnimatorConditionMode.Equals, weaponNumber, "WeaponNumber");
            tran.AddCondition(AnimatorConditionMode.Equals, comboCount, "ComboCount");
            tran.AddCondition(AnimatorConditionMode.Equals, stateNumber, "State");
            if (oneShot)
            {
                tran.canTransitionToSelf = true;
                tran.AddCondition(AnimatorConditionMode.If, 0, "OneShot");
            }

        }
        public void save()
        {
            if (needSave)
            {
                if (state && motion)
                    state.motion = motion;
                setTransition();
                needSave = false;
            }

            foreach (var item in childs)
            {
                item.save();
            }

        }
    }
}