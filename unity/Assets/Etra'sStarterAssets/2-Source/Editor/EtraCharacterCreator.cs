using UnityEngine;
using UnityEditor;
using StarterAssets;
using System.Collections.Generic;
using System;
using static StarterAssets.EtraCharacterMainController;

public class EtraCharacterCreator : EditorWindow
{

    //Make enum variables
    enum CharacterCreatorPage
    {
        IntroAndGeneralAbilities,
        FirstPersonAbilities,
        ThirdPersonAbilities,
        FirstPersonItems
    }

    //Selectable enum variable instances
    CharacterCreatorPage characterCreatorPage = EtraCharacterCreator.CharacterCreatorPage.IntroAndGeneralAbilities;
    EtraCharacterMainController.GameplayType gameplayType = EtraCharacterMainController.GameplayType.FirstPerson;
    EtraCharacterMainController.Model model = EtraCharacterMainController.Model.DefaultArmature;


    #region Text Style Types Variables and Functions
    bool m_Initialized;

    GUIStyle LinkStyle { get { return m_LinkStyle; } }
    [SerializeField] GUIStyle m_LinkStyle;

    GUIStyle TitleStyle { get { return m_TitleStyle; } }
    [SerializeField] GUIStyle m_TitleStyle;

    GUIStyle HeadingStyle { get { return m_HeadingStyle; } }
    [SerializeField] GUIStyle m_HeadingStyle;

    GUIStyle BodyStyle { get { return m_BodyStyle; } }
    [SerializeField] GUIStyle m_BodyStyle;

    GUIStyle ToggleBox { get { return m_ToggleBox; } }
    [SerializeField] GUIStyle m_ToggleBox;

    void setUpTextStyleTypes()
    {
        if (m_Initialized)
            return;

        m_BodyStyle = new GUIStyle(EditorStyles.label);
        m_BodyStyle.wordWrap = true;
        m_BodyStyle.fontSize = 14;
        m_BodyStyle.richText = true;

        m_TitleStyle = new GUIStyle(m_BodyStyle);
        m_TitleStyle.fontSize = 26;
        m_TitleStyle.fontStyle = FontStyle.Bold;
        m_TitleStyle.normal.background = MakeTex(600, 1, new Color(0.1f, 0.1f, 0.1f, 1.0f));


        m_HeadingStyle = new GUIStyle(m_BodyStyle);
        m_HeadingStyle.fontSize = 18;

        m_LinkStyle = new GUIStyle(m_BodyStyle);
        m_LinkStyle.wordWrap = false;
        // Match selection color which works nicely for both light and dark skins
        m_LinkStyle.normal.textColor = new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f);
        m_LinkStyle.stretchWidth = false;

        m_ToggleBox = new GUIStyle(m_BodyStyle);
        m_ToggleBox.alignment = TextAnchor.UpperRight;

        m_Initialized = true;
    }


    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];

        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();

        return result;
    }


    #endregion
    #region Dynamic Ability List Setup Variables and Functions

    //Variables for ability list setup
    bool listSetup = false;

    string[] abilityStringArray;
    bool[] basicAbilityBools;
    string[] FPSAbilityStringArray;
    bool[] FPSAbilityBools;
    string[] FPSItemsStringArray;
    bool[] FPSItemsBools;
    string[] TPSAbilityStringArray;
    bool[] TPSAbilityBools;

    private void setUpAbilityLists()
    {
        if (listSetup) { return; }


        List<string> basicAbilityFilePathStrings = new List<string>();
        List<string> FPSAbilityFilePathStrings = new List<string>();
        List<string> TPSAbilityFilePathStrings = new List<string>();

        List<string> FPSItemsFilePathStrings = new List<string>();


        string[] filePaths;
        filePaths = AssetDatabase.FindAssets("ABILITY_");
        foreach (string abilityGUID in filePaths)
        {
            string abilityPath = AssetDatabase.GUIDToAssetPath(abilityGUID);
            if (abilityPath.Contains("_FPS_"))
            {
                FPSAbilityFilePathStrings.Add(abilityPath);
            }
            else if (abilityPath.Contains("_TPS_"))
            {
                TPSAbilityFilePathStrings.Add(abilityPath);
            }
            else
            {
                basicAbilityFilePathStrings.Add(abilityPath);
            }
        }

        filePaths = AssetDatabase.FindAssets("USABLEITEM_");
        foreach (string abilityGUID in filePaths)
        {
            string abilityPath = AssetDatabase.GUIDToAssetPath(abilityGUID);
            if (abilityPath.Contains("_FPS_"))
            {
                FPSItemsFilePathStrings.Add(abilityPath);
            }
            //Add TPS Items here eventually
        }




        basicAbilityBools = new bool[basicAbilityFilePathStrings.Count];
        FPSAbilityBools = new bool[FPSAbilityFilePathStrings.Count];
        TPSAbilityBools = new bool[TPSAbilityFilePathStrings.Count];

        FPSItemsBools = new bool[FPSItemsFilePathStrings.Count];

        abilityStringArray = basicAbilityFilePathStrings.ToArray();
        FPSAbilityStringArray = FPSAbilityFilePathStrings.ToArray();
        TPSAbilityStringArray = TPSAbilityFilePathStrings.ToArray();

        FPSItemsStringArray= FPSItemsFilePathStrings.ToArray();

        listSetup = true;
    }



    string getAbilityNameFromFilePath(string abilityFilePath)
    {
        //Check if FPS or TPA and account for that
        String finalAbiltyName = "";

        if (abilityFilePath.Contains("_FPS_"))
        {
            finalAbiltyName += "FPS ";
        }
        else if (abilityFilePath.Contains("_TPS_"))
        {
            finalAbiltyName += "TPS ";
        }


        int charCounter = 0;
        int startingPoint = 0;
        for (int i = abilityFilePath.Length - 1; i >= 0; i--)
        {
            if (abilityFilePath[i].Equals('_'))
            {
                startingPoint = i;
                i = 0;
            }
            else
            {
                charCounter++;
            }
        }

        string abilityNameAsOneWord = abilityFilePath.Substring(startingPoint + 1, charCounter - 3);



        for (int i = 0; i < abilityNameAsOneWord.Length; i++)
        {
            if (Char.IsUpper(abilityNameAsOneWord[i]))
            {
                finalAbiltyName += " ";
                finalAbiltyName += abilityNameAsOneWord[i];
            }
            else
            {
                finalAbiltyName += abilityNameAsOneWord[i];
            }
        }

        return finalAbiltyName;

    }


    string getScriptNameFromFilePath(string abilityFilePath)
    {

        int charCounter = 0;
        int startingPoint = 0;
        for (int i = abilityFilePath.Length - 1; i >= 0; i--)
        {
            if (abilityFilePath[i].Equals('/'))
            {
                startingPoint = i;
                i = 0;
            }
            else
            {
                charCounter++;
            }


        }
        string scriptName = abilityFilePath.Substring(startingPoint + 1, charCounter - 3);
        return scriptName;
    }


    #endregion

    [MenuItem("Window/Etra'sStarterAssets/EtraCharacterCreator")]
    public static void ShowWindow() //Functionally Start();
    {
        //Set Window size and name
        EtraCharacterCreator window = (EtraCharacterCreator)GetWindow(typeof(EtraCharacterCreator));
        window.minSize = new Vector2(400, 400);
        window.maxSize = new Vector2(400, 400); //default start size
        GetWindow<EtraCharacterCreator>("Etra Character Creator");
        window.maxSize = new Vector2(800, 800); //true max size
    }


    private void OnGUI() //Functionally Update();
    {
        //Set up functions that should only run once with their internal checks
        setUpTextStyleTypes();
        setUpAbilityLists();

        //Function that is updated each frame
        constructAndDisplayCorrectPage();

    }

    void constructAndDisplayCorrectPage()
    {

        int iterator = 0;
        switch (characterCreatorPage)
        {
            //~~~INTRO PAGE~~~
            case EtraCharacterCreator.CharacterCreatorPage.IntroAndGeneralAbilities:
                model = EtraCharacterMainController.Model.DefaultArmature;

                GUILayout.Label("Etra Character Creator:", TitleStyle);
                GUILayout.Space(5);
                GUILayout.Label("Select your Character's GENERAL Abilities...", m_HeadingStyle);

                iterator = 0;
                foreach (string filePath in abilityStringArray)
                {
                    basicAbilityBools[iterator] = EditorGUILayout.Toggle(getAbilityNameFromFilePath(filePath), basicAbilityBools[iterator]);
                    iterator++;
                }

                GUILayout.Space(10);

                GUILayout.Label("Select your Character Type...", m_HeadingStyle);
                gameplayType = (EtraCharacterMainController.GameplayType)EditorGUILayout.EnumPopup(gameplayType);
                GUILayout.Space(10);

                GUILayout.Space(10);


                if (GUILayout.Button("Next"))
                {
                    switch (gameplayType)
                    {
                        case EtraCharacterMainController.GameplayType.FirstPerson:
                            characterCreatorPage = EtraCharacterCreator.CharacterCreatorPage.FirstPersonAbilities;
                            break;

                        case EtraCharacterMainController.GameplayType.ThirdPerson:
                            characterCreatorPage = EtraCharacterCreator.CharacterCreatorPage.ThirdPersonAbilities;
                            break;
                    }
                }
                
                break;

            //~~~FPS PAGE~~~
            case EtraCharacterCreator.CharacterCreatorPage.FirstPersonAbilities:

                GUILayout.Label("Etra Character Creator:", TitleStyle);
                GUILayout.Space(5);

                if (GUILayout.Button("Back"))
                {
                    characterCreatorPage = EtraCharacterCreator.CharacterCreatorPage.IntroAndGeneralAbilities;
                }


                GUILayout.Label("Select your Character's FPS Abilities...", m_HeadingStyle);

                iterator = 0;
                foreach (string filePath in FPSAbilityStringArray)
                {
                    FPSAbilityBools[iterator] = EditorGUILayout.Toggle(getAbilityNameFromFilePath(filePath), FPSAbilityBools[iterator]);
                    iterator++;
                }


                GUILayout.Space(10);

                if (GUILayout.Button("Next"))
                {
                    characterCreatorPage = EtraCharacterCreator.CharacterCreatorPage.FirstPersonItems;
                }


                break;


            //~~~FPS ITEMS~~~
            case EtraCharacterCreator.CharacterCreatorPage.FirstPersonItems:

                model = EtraCharacterMainController.Model.Capsule;

                GUILayout.Label("Etra Character Creator:", TitleStyle);
                GUILayout.Space(5);

                if (GUILayout.Button("Back"))
                {
                    characterCreatorPage = EtraCharacterCreator.CharacterCreatorPage.FirstPersonAbilities;
                }

                GUILayout.Label("Select your Character's FPS Items...", m_HeadingStyle);


                iterator = 0;
                foreach (string filePath in FPSItemsStringArray)
                {
                    FPSItemsBools[iterator] = EditorGUILayout.Toggle(getAbilityNameFromFilePath(filePath), FPSItemsBools[iterator]);
                    iterator++;
                }

                GUILayout.Space(10);

                if (GUILayout.Button("CREATE"))
                {
                    createEtraCharacter();
                }

                break;


            //~~~TPS PAGE~~~
            case EtraCharacterCreator.CharacterCreatorPage.ThirdPersonAbilities:

                GUILayout.Label("Etra Character Creator:", TitleStyle);
                GUILayout.Space(5);

                if (GUILayout.Button("Back"))
                {
                    characterCreatorPage = EtraCharacterCreator.CharacterCreatorPage.IntroAndGeneralAbilities;
                }


                GUILayout.Label("Select your Character Model...", m_HeadingStyle);
                model = (EtraCharacterMainController.Model)EditorGUILayout.EnumPopup(model);
                GUILayout.Space(10);

                GUILayout.Label("Select your Character's TPS Abilities...", m_HeadingStyle);

                iterator = 0;
                foreach (string filePath in TPSAbilityStringArray)
                {
                    TPSAbilityBools[iterator] = EditorGUILayout.Toggle(getAbilityNameFromFilePath(filePath), TPSAbilityBools[iterator]);
                    iterator++;
                }


                GUILayout.Space(10);

                if (GUILayout.Button("CREATE"))
                {
                    createEtraCharacter();
                }

                break;
        }
    }

    void createEtraCharacter()
    {

        if (GameObject.Find("EtraCharacterAssetBase"))
        {
            Debug.LogWarning("Please delete the first EtraCharacterAssetGroup in the scene before adding another one.");
            return;
        }


        EtrasResourceGrabbingFunctions.addPrefabFromAssetsByName("EtraCharacterAssetGroup");

        GameObject assetBase = GameObject.Find("EtraCharacterAssetBase");
        GameObject abilityManager = GameObject.Find("EtraAbilityManager");


        //Add base abilities
        int iterator;
        iterator= 0;
        foreach (string filePath in abilityStringArray)
        {
            if (basicAbilityBools[iterator])
            {
                EtraAbilityBaseClass abilityToAdd = (EtraAbilityBaseClass)AssetDatabase.LoadAssetAtPath(filePath, typeof(EtraAbilityBaseClass));
                abilityManager.AddComponent(EtrasResourceGrabbingFunctions.GetComponentFromAssetsByName(getScriptNameFromFilePath(filePath)));
            }
            iterator++;
        }


        //Set character up correctly.
        switch (gameplayType)
        {
            case EtraCharacterMainController.GameplayType.FirstPerson:

                //Add FPS Abilities 
                iterator = 0;
                foreach (string filePath in FPSAbilityStringArray)
                {
                    if (FPSAbilityBools[iterator])
                    {
                        abilityManager.AddComponent(EtrasResourceGrabbingFunctions.GetComponentFromAssetsByName(getScriptNameFromFilePath(filePath)));
                    }
                    iterator++;
                }


                //Add FPS Items

                //Check if any FPS ITems are added
                bool itemsAddedToCharacter = false;
                for (int i = 0; i < FPSItemsBools.Length; i++)
                {
                    if (FPSItemsBools[i] == true)
                    {
                        itemsAddedToCharacter = true;
                        i = FPSItemsBools.Length;
                    }
                }

                if (itemsAddedToCharacter)
                {
                    //Add Usable Item Manager 
                    GameObject FPSUsableItemManager  =  EtrasResourceGrabbingFunctions.addPrefabFromAssetsByName("EtraFPSUsableItemManagerPrefab", assetBase.transform);


                    FPSUsableItemManager.GetComponent<EtraFPSUsableItemManager>().Reset();

                    //Add correct usable items
                    iterator = 0;
                    foreach (string filePath in FPSItemsStringArray)
                    {
                        if (FPSItemsBools[iterator])
                        {
                            FPSUsableItemManager.AddComponent(EtrasResourceGrabbingFunctions.GetComponentFromAssetsByName(getScriptNameFromFilePath(filePath)));
                        }
                        iterator++;
                    }
                }

                abilityManager.transform.parent.transform.GetComponent<EtraCharacterMainController>().applyGameplayChanges(gameplayType, model);
                break;

            case EtraCharacterMainController.GameplayType.ThirdPerson:

                //Add TPS Abilities maybe
                iterator = 0;
                foreach (string filePath in TPSAbilityStringArray)
                {
                    if (TPSAbilityBools[iterator])
                    {
                        EtraAbilityBaseClass abilityToAdd = (EtraAbilityBaseClass)AssetDatabase.LoadAssetAtPath(filePath, typeof(EtraAbilityBaseClass));
                        abilityManager.AddComponent(EtrasResourceGrabbingFunctions.GetComponentFromAssetsByName( getScriptNameFromFilePath(filePath)));
                    }
                    iterator++;
                }



                abilityManager.transform.parent.transform.GetComponent<EtraCharacterMainController>().applyGameplayChanges(gameplayType, model);
                break;
        }


    }





}
