using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;
using System.Xml.Linq;

public class GourmetHamburgerScript : MonoBehaviour
{
    static int _moduleIdCounter = 1;
    int _moduleID = 0;

    public KMBombModule Module;
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMSelectable[] Buttons;
    public SpriteRenderer IngredientRend;
    public Sprite[] IngredientSprites;
    public GameObject ModuleMoveable;

    private class Ingredient
    {
        public int ID;
        public IngredientInfo Info;
        public bool ADDResult;

        public Ingredient(int id, GourmetHamburgerScript script)
        {
            ID = id;
            if (id == -1)
                Info = new IngredientInfo("None", 0);
            else
                Info = AllIngredientInfo[id];
            ADDResult = script.FindADDResult(id);
        }
    }

    private class IngredientInfo
    {
        public string Name;
        public int Category;

        public IngredientInfo(string name, int category)
        {
            Name = name;
            Category = category;
        }
    }

    private Sprite FindSprite(string name)
    {
        return IngredientSprites.Where(x => x.name == name.ToLowerInvariant()).First();
    }

    bool FindADDResult(int id)  // Putting these into a separate script was a pain, so I'm just gonna dump it all here.
    {
        if (id == -1)
            return false;
        var ingredient = AllIngredientInfo[id];
        switch (ingredient.Name)
        {
            case "Patty":
                return PreviouslyAdded.Count() != 0 && (PreviouslyAdded.Last().Info.Category != 1 && PreviouslyAdded.Any(x => x.Info.Category == 1));

            case "Lettuce":
                return PreviouslyAdded.Count(x => x.Info.Category == 2) < PreviouslyAdded.Count(x => x.Info.Category == 1);

            case "Cheese":
                return Bomb.GetSerialNumberLetters().Any(x => "SAOPL".Contains(x));

            case "Onions":
                return PreviouslyAdded.Count() == 0 || (PreviouslyAdded.Last().Info.Category != 2 && (PreviouslyAdded.Count() == 1 || PreviouslyAdded[PreviouslyAdded.Count() - 2].Info.Category != 2));

            case "Chicken":
                return PreviouslyAdded.Any(x => x.Info.Name == "Lettuce" || x.Info.Name == "Mayonnaise") || !PreviouslyAdded.Any(x => x.Info.Category == 1);

            case "Tomatoes":
                return PreviouslyAdded.Count() % 2 != PreviouslyAdded.Where(x => x.Info.Category == 2).Count() % 2;

            case "Ketchup":
                return !PreviouslyAdded.Any(x => x.Info.Name != "Ketchup" && x.Info.Category == 3) && !PreviouslyAdded.Any(x => x.Info.Name == "Chicken");

            case "Bun":
                return PreviouslyAdded.Count() > 0 && PreviouslyAdded.First().Info.Category == PreviouslyAdded.Last().Info.Category;

            case "Pickles":
                for (int i = 0; i < PreviouslyAdded.Count() - 1; i++) // Will not run if the count is less than 2.
                    if (PreviouslyAdded[i].Info.Category == PreviouslyAdded[i + 1].Info.Category)
                        return true;
                return false;

            case "Salt":
                return PreviouslyAdded.Count() >= 5 && !PreviouslyAdded.Any(x => x.Info.Name == "Salt");

            case "Pepper":
                return PreviouslyAdded.Count() >= 5 && !PreviouslyAdded.Any(x => x.Info.Name == "Pepper");

            case "Bacon":
                return PreviouslyAdded.Any(x => x.Info.Name == "Egg") || PreviouslyAdded.Count(x => x.Info.Category == 1) < PreviouslyAdded.Count(x => x.Info.Category == 3);

            case "Barbecue Sauce":
                return new[] { 1, 3 }.Contains(PreviouslyAdded.Count(x => x.Info.Category == 1));

            case "Burger Sauce":
                return PreviouslyAdded.Count(x => x.Info.Category == 1) == 2;

            case "Bell Peppers":
                var pepperCounts = new List<int>();
                for (int i = 0; i < 4; i++)
                    pepperCounts.Add(PreviouslyAdded.Count(x => x.Info.Category == i));
                pepperCounts.Sort();
                return pepperCounts[3] == pepperCounts[2] && pepperCounts[2] != pepperCounts[1];

            case "Ham":
                return PreviouslyAdded.Count(x => x.Info.Category == 3) == 1;

            case "Mayonnaise":
                return PreviouslyAdded.Count(x => x.Info.Category == 1) < 2 && !PreviouslyAdded.Any(x => x.Info.Category == 3);

            case "Egg":
                return Bomb.GetSerialNumberNumbers().Last() > PreviouslyAdded.Count();

            case "Mustard":
                return PreviouslyAdded.Count(x => x.Info.Category == 3) >= 2 && !PreviouslyAdded.Any(x => x.Info.Name == "Mustard");

            case "Salami":
                return PreviouslyAdded.Count() >= 3 && PreviouslyAdded[PreviouslyAdded.Count() - 1].Info.Category > 0 && PreviouslyAdded[PreviouslyAdded.Count() - 2].Info.Category > 0 && PreviouslyAdded[PreviouslyAdded.Count() - 3].Info.Category > 0;

            case "Basil":
                var basilCounts = new List<int>();
                for (int i = 0; i < 4; i++)
                    basilCounts.Add(PreviouslyAdded.Count(x => x.Info.Category == i));
                basilCounts.Sort();
                return basilCounts[0] == 0 && basilCounts[1] == 0 && basilCounts[2] != 0;

            case "Chili Powder":
                return PreviouslyAdded.Count() == PreviouslyAdded.Select(x => x.ID).Distinct().Count() && PreviouslyAdded.Count(x => x.Info.Category == 2) < 2;

            case "Asparagus":
                return PreviouslyAdded.Count() >= 2 && PreviouslyAdded.Last().Info.Name.Length > PreviouslyAdded[PreviouslyAdded.Count() - 2].Info.Name.Length;

            case "Cod":
                return PreviouslyAdded.Count() < 3;

            case "Curry Sauce":
                return PreviouslyAdded.Count() > 0 && (PreviouslyAdded.Any(x => x.Info.Name == "Chili Powder") || PreviouslyAdded.Any(x => x.Info.Name == "Cod") || PreviouslyAdded.Last().Info.Category == 1);

            case "Worcestershire Sauce":
                return PreviouslyAdded.Count() == 0 || (PreviouslyAdded.Last().Info.Category == 2 && PreviouslyAdded.First().Info.Category != 1);

            case "Salmon":
                return !PreviouslyAdded.Any(x => x.Info.Category == 3) && PreviouslyAdded.Any(x => x.Info.Category == 0);

            case "Green Beans":
                return false;

            default:
                Debug.LogFormat("[Gourmet Hamburger #{0}] Couldn't find {1}'s ADD condition! This is a bug.", _moduleID, ingredient.Name);
                return false;
        }
    }

    bool FindENDResult(int id)
    {
        var ingredient = AllIngredientInfo[id];
        switch (ingredient.Name)
        {
            case "Patty":
                return FindADDResult(id) && PreviouslyAdded.Count(x => x.Info.Category == 1) == 3;

            case "Lettuce":
                return PreviouslyAdded.Count() == 10;

            case "Cheese":
                return PreviouslyAdded.Count(x => x.Info.Name == "Cheese") == 2 || PreviouslyAdded.Count(x => x.Info.Category == 0) == 3;

            case "Onions":
                return PreviouslyAdded.Count() >= 4 && PreviouslyAdded.Last().Info.Category == 2 && PreviouslyAdded[PreviouslyAdded.Count() - 2].Info.Category == 2 && PreviouslyAdded[PreviouslyAdded.Count() - 3].Info.Category != 2 && PreviouslyAdded[PreviouslyAdded.Count() - 4].Info.Category != 2;  // Wow, this code sucks.

            case "Chicken":
                return PreviouslyAdded.Any(x => x.Info.Name == "Lettuce" || x.Info.Name == "Mayonnaise") && PreviouslyAdded.Count(x => x.Info.Category == 1) == 2;

            case "Tomatoes":
                return FindADDResult(id) && PreviouslyAdded.Count() == 5;

            case "Ketchup":
                return !FindADDResult(id) && PreviouslyAdded.Count(x => x.Info.Category == 3) == 3;

            case "Bun":
                return FindADDResult(id) && PreviouslyAdded.First().Info.Category == 0;    //Last is guaranteed to equal First, if ADD applies.

            case "Pickles":
                for (int i = 0; i < PreviouslyAdded.Count() - 2; i++) // Will not run if the count is less than 3.
                    if (PreviouslyAdded[i].Info.Category == PreviouslyAdded[i + 1].Info.Category && PreviouslyAdded[i + 1].Info.Category == PreviouslyAdded[i + 2].Info.Category)
                        return true;
                return false;

            case "Salt":
                return FindADDResult(id) && PreviouslyAdded.Any(x => x.Info.Name == "Pepper");

            case "Pepper":
                return FindADDResult(id) && PreviouslyAdded.Any(x => x.Info.Name == "Salt");

            case "Bacon":
                return PreviouslyAdded.Count(x => x.Info.Category == 1) > PreviouslyAdded.Count(x => x.Info.Category == 2) + PreviouslyAdded.Count(x => x.Info.Category == 3);

            case "Barbecue Sauce":
                return PreviouslyAdded.Count(x => x.Info.Category == 1) == 4;

            case "Burger Sauce":
                return FindADDResult(id) && PreviouslyAdded.Any(x => x.Info.Name == "Patty");

            case "Bell Peppers":
                var counts = new List<int>();
                for (int i = 0; i < 4; i++)
                    counts.Add(PreviouslyAdded.Count(x => x.Info.Category == i));
                counts.Sort();
                return counts[3] == counts[2] && counts[2] == counts[1] && counts[1] != counts[0];

            case "Ham":
                return PreviouslyAdded.Count(x => x.Info.Category == 3) == 2 && PreviouslyAdded.Last().Info.Category == 3;

            case "Mayonnaise":
                return PreviouslyAdded.Count(x => x.Info.Category == 1) < 2 && !PreviouslyAdded.Any(x => x.Info.Category == 3 && x.Info.Name != "Mayonnaise") && PreviouslyAdded.Any(x => x.Info.Name == "Mayonnaise");

            case "Egg":
                return Bomb.GetSerialNumberNumbers().Sum() < PreviouslyAdded.Count();

            case "Mustard":
                return FindADDResult(id) && PreviouslyAdded.Any(x => x.Info.Name == "Cheese") && Bomb.GetSerialNumberLetters().Any(x => "SAOPL".Contains(x));

            case "Salami":
                return FindADDResult(id) && PreviouslyAdded.Skip(PreviouslyAdded.Count() - 3).Count(x => x.Info.Category == 1) == 1;

            case "Basil":
                return PreviouslyAdded.Any(x => x.Info.Category == 0) && PreviouslyAdded.Any(x => x.Info.Category == 1) && PreviouslyAdded.Any(x => x.Info.Category == 2) && PreviouslyAdded.All(x => x.Info.Category != 3);

            case "Chili Powder":
                return PreviouslyAdded.Count(x => x.Info.Name == "Chili Powder") == 2;

            case "Asparagus":
                return FindADDResult(id) && 9 > PreviouslyAdded.Last().Info.Name.Length;

            case "Cod":
                return PreviouslyAdded.Count >= 8 && Bomb.GetSerialNumberLetters().Any(x => "AEIOU".Contains(x));

            case "Curry Sauce":
                return PreviouslyAdded.Count() > 0 && PreviouslyAdded.Last().Info.Category == 1 && PreviouslyAdded.Count(x => x.Info.Category == 1) == 1;

            case "Worcestershire Sauce":
                return PreviouslyAdded.Count() != 0 && FindADDResult(id) && PreviouslyAdded.First().Info.Category == 2;

            case "Salmon":
                return PreviouslyAdded.Any(x => x.Info.Category == 0) && PreviouslyAdded.Any(x => x.Info.Category == 1) && PreviouslyAdded.Any(x => x.Info.Category == 2) && PreviouslyAdded.Any(x => x.Info.Category == 3);   // I am a coding pro. :3

            case "Green Beans":
                return PreviouslyAdded.Any(x => x.Info.Name == "Green Beans");

            default:
                Debug.LogFormat("[Gourmet Hamburger #{0}] Couldn't find {1}'s END condition! This is a bug.", _moduleID, ingredient.Name);
                return false;
        }
    }

    private List<Ingredient> CurrentIngredients = new List<Ingredient>();
    private List<Ingredient> PreviouslyAdded = new List<Ingredient>();
    private Coroutine[] ButtonAnimCoroutines;
    private static readonly IngredientInfo[] AllIngredientInfo = new[]    // 1 = meat, 2 = veg, 3 = sauce, 0 = untyped
    {
        new IngredientInfo("Patty", 1),
        new IngredientInfo("Lettuce", 2),
        new IngredientInfo("Cheese", 0),
        new IngredientInfo("Onions", 2),
        new IngredientInfo("Chicken", 1),
        new IngredientInfo("Tomatoes", 2),
        new IngredientInfo("Ketchup", 3),
        new IngredientInfo("Bun", 0),
        new IngredientInfo("Pickles", 2),
        new IngredientInfo("Salt", 0),
        new IngredientInfo("Pepper", 0),
        new IngredientInfo("Bacon", 1),
        new IngredientInfo("Barbecue Sauce", 3),
        new IngredientInfo("Burger Sauce", 3),
        new IngredientInfo("Bell Peppers", 2),
        new IngredientInfo("Ham", 1),
        new IngredientInfo("Mayonnaise", 3),
        new IngredientInfo("Egg", 0),
        new IngredientInfo("Mustard", 3),
        new IngredientInfo("Salami", 1),
        new IngredientInfo("Basil", 0),
        new IngredientInfo("Chili Powder", 0),
        new IngredientInfo("Asparagus", 2),
        new IngredientInfo("Cod", 1),
        new IngredientInfo("Curry Sauce", 3),
        new IngredientInfo("Worcestershire Sauce", 3),
        new IngredientInfo("Salmon", 1),
        new IngredientInfo("Green Beans", 2)
    };
    private Ingredient RequiredIngredient;
    private int IngredientIx, NumberOfLayers;
    private bool CannotPress, Solved;
    private KMAudio.KMAudioRef Sound;

    void Awake()
    {
        _moduleID = _moduleIdCounter++;
        ButtonAnimCoroutines = new Coroutine[Buttons.Length];
        for (int i = 0; i < Buttons.Length; i++)
        {
            int x = i;
            Buttons[x].OnInteract += delegate { ButtonPress(x); return false; };
        }
        Bomb.OnBombExploded += delegate { if (Sound != null) Sound.StopSound(); };
    }

    // Use this for initialization
    void Start()
    {
        Debug.LogFormat("[Gourmet Hamburger #{0}] Ah, greetings, Monsieur Skinner! I have taken Monsieur Chalmers' order, and he wants your famous hamburger.", _moduleID);
        Debug.LogFormat("[Gourmet Hamburger #{0}] ...You WHAT?! You burned the recipe?... Oh dear... Ah, oui, let me help you remember it.", _moduleID);
        GenerateTriplet();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void GenerateTriplet()
    {
        CurrentIngredients = new List<Ingredient>();
        var chosen = Enumerable.Range(0, AllIngredientInfo.Length).ToList().Shuffle().Take(3).ToList();
        bool serve = false;
        for (int i = 0; i < 3; i++)
        {
            CurrentIngredients.Add(new Ingredient(chosen[i], this));
            Debug.LogFormat("[Gourmet Hamburger #{0}] The {1} option for layer {2} is {3}. Its ADD {4}.", _moduleID, new[] { "first", "second", "third" }[i], NumberOfLayers + 1, CurrentIngredients.Last().Info.Name, CurrentIngredients.Last().ADDResult ? "applies" : "does not apply");
            if (FindENDResult(CurrentIngredients.Last().ID))
            {
                serve = true;
                if (NumberOfLayers >= 5)
                    Debug.LogFormat("[Gourmet Hamburger #{0}] {1}'s END rule applies.", _moduleID, CurrentIngredients.Last().Info.Name);
            }

        }
        IngredientRend.sprite = FindSprite(CurrentIngredients.First().Info.Name);
        if (serve && NumberOfLayers >= 5)
        {
            RequiredIngredient = new Ingredient(-1, this);
            Debug.LogFormat("[Gourmet Hamburger #{0}] Since at least one of the possible ingredients' END rules apply, you should serve the burger.", _moduleID);
        }
        else if (NumberOfLayers >= 20)
        {
            RequiredIngredient = new Ingredient(-1, this);
            Debug.LogFormat("[Gourmet Hamburger #{0}] Since the number of layers is now twenty, you should serve the burger.", _moduleID);
        }
        else if (CurrentIngredients.Select(x => x.ADDResult).Distinct().Count() == 1)
        {
            RequiredIngredient = CurrentIngredients.OrderBy(x => x.ID).First();
            Debug.LogFormat("[Gourmet Hamburger #{0}] All three ADDs have the same result, so you should add the topmost ingredient, {1}.", _moduleID, RequiredIngredient.Info.Name);
        }
        else
        {
            RequiredIngredient = CurrentIngredients.Where(x => CurrentIngredients.Count(y => y.ADDResult == x.ADDResult) == 1).First();
            Debug.LogFormat("[Gourmet Hamburger #{0}] {1}'s ADDs {2}, so you should add the outlier, {3}.", _moduleID, CurrentIngredients.Where(x => x.ID != RequiredIngredient.ID).Select(x => x.Info.Name).Join(" and "), !RequiredIngredient.ADDResult ? "apply" : "do not apply", RequiredIngredient.Info.Name);
        }
    }

    void ButtonPress(int pos)
    {
        if (!Solved || CannotPress || pos != 3)
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, Buttons[pos].transform);
        Buttons[pos].AddInteractionPunch(((pos / 2) + 1) * 0.5f);
        if (ButtonAnimCoroutines[pos] != null)
            StopCoroutine(ButtonAnimCoroutines[pos]);
        ButtonAnimCoroutines[pos] = StartCoroutine(ButtonAnim(pos));
        if (!CannotPress)
        {
            if (!Solved)
                switch (pos)
                {
                    case 0:
                        IngredientIx = (IngredientIx + 2) % 3;
                        IngredientRend.sprite = FindSprite(CurrentIngredients[IngredientIx].Info.Name);
                        break;
                    case 1:
                        IngredientIx = (IngredientIx + 1) % 3;
                        IngredientRend.sprite = FindSprite(CurrentIngredients[IngredientIx].Info.Name);
                        break;
                    case 2:
                        HandleAddIngredient();
                        break;
                    case 3:
                        HandleSubmit();
                        break;
                }
            else if (pos == 3)
            {
                Audio.PlaySoundAtTransform("press secret", transform);
                StartCoroutine(HandleEasterEgg());
            }
        }
    }

    void HandleAddIngredient()
    {
        if (RequiredIngredient.ID == CurrentIngredients[IngredientIx].ID)
        {
            NumberOfLayers++;
            Debug.LogFormat("[Gourmet Hamburger #{0}] You added {1}. Exquisite selection.", _moduleID, CurrentIngredients[IngredientIx].Info.Name);
            if ((NumberOfLayers + 3) % 6 == 0)
            {
                if (Sound != null)
                    Sound.StopSound();
                Sound = Audio.HandlePlaySoundAtTransformWithRef("yes special", transform, false);
            }
            else
            {
                if (Sound != null)
                    Sound.StopSound();
                Sound = Audio.HandlePlaySoundAtTransformWithRef("yes " + ((NumberOfLayers % 2) + 1), transform, false);
            }
            PreviouslyAdded.Add(CurrentIngredients[IngredientIx]);
            IngredientIx = 0;
            GenerateTriplet();
        }
        else
        {
            Debug.LogFormat("[Gourmet Hamburger #{0}] You added {1}. How rancid. Monsieur Chalmers will be less than pleased. (Strike.)", _moduleID, CurrentIngredients[IngredientIx].Info.Name);
            if (Sound != null)
                Sound.StopSound();
            Sound = Audio.HandlePlaySoundAtTransformWithRef("strike skinner", transform, false);
            Module.HandleStrike();
            TimeWarp();
        }
    }

    void TimeWarp()
    {
        Debug.LogFormat("[Gourmet Hamburger #{0}] LEEEEEET'S DOOOOOO the TIIIIIIME WAAAAAARP AGAAAAAAIIIIIIN!", _moduleID);
        Debug.LogFormat("[Gourmet Hamburger #{0}] Ah, greetings, Monsieur Skinner! I have taken Monsieur Chalmers' order, and he wants your famous hamburger.", _moduleID);
        Debug.LogFormat("[Gourmet Hamburger #{0}] ...You WHAT?! You burned the recipe?... Oh dear... Ah, oui, let me help you remember it.", _moduleID);
        NumberOfLayers = 0;
        PreviouslyAdded = new List<Ingredient>();
        GenerateTriplet();
    }

    void HandleSubmit()
    {
        Debug.LogFormat("[Gourmet Hamburger #{0}] You served the burger.", _moduleID);
        StartCoroutine(SubmitAnim());
        if (Sound != null)
            Sound.StopSound();
        Sound = Audio.HandlePlaySoundAtTransformWithRef("submit" + (NumberOfLayers == 0 ? " nothing" : ""), transform, false);
    }

    private IEnumerator HandleEasterEgg(float duration = 0.75f, float magnitude = 0.015f)
    {
        CannotPress = true;
        var rand = Rnd.Range(0.25f, 5f);
        float timer = 0;
        while (timer < rand)
        {
            yield return null;
            timer += Time.deltaTime;
        }
        Audio.PlaySoundAtTransform("explosion", transform);
        Module.GetComponent<KMSelectable>().AddInteractionPunch(10);
        timer = 0;
        while (timer < duration)
        {
            yield return null;
            timer += Time.deltaTime;
            ModuleMoveable.transform.localPosition = new Vector3(Mathf.Lerp(Rnd.Range(-magnitude, magnitude), 0, timer / duration), 0, Mathf.Lerp(Rnd.Range(-magnitude, magnitude), 0, timer / duration));
        }
        ModuleMoveable.transform.localPosition = Vector3.zero;
        CannotPress = false;
    }

    private IEnumerator SubmitAnim()
    {
        CannotPress = true;
        if (NumberOfLayers == 0)
            IngredientRend.sprite = FindSprite("hamburger empty");
        else
            IngredientRend.sprite = FindSprite("hamburger");
        Audio.PlaySoundAtTransform("bell", transform);
        float timer = 0;
        while (timer < 0.15f)
        {
            yield return null;
            timer += Time.deltaTime;
        }
        Audio.PlaySoundAtTransform("bell", transform);
        timer = 0;
        while (timer < (NumberOfLayers == 0 ? 0.5f : 1.5f))
        {
            yield return null;
            timer += Time.deltaTime;
        }
        if (NumberOfLayers == 0)
        {
            Debug.LogFormat("[Gourmet Hamburger #{0}] MON DIEU!! You... you cannot simply serve an empty burger, that... THAT IS OUTRAGEOUS!! (Strike.)", _moduleID);
            goto skipEat;
        }
        else if (RequiredIngredient.ID != -1)
        {
            Debug.LogFormat("[Gourmet Hamburger #{0}] Oh dear. Monsieur Chalmers does not seem to be enjoying his luncheon. Perhaps the burger was not ready to serve, yet. (Strike.)", _moduleID);
        }
        Audio.PlaySoundAtTransform("eat", transform);
        IngredientRend.sprite = FindSprite("hamburger eaten");
        timer = 0;
        while (timer < 1.5f)
        {
            yield return null;
            timer += Time.deltaTime;
        }
        skipEat:
        if (RequiredIngredient.ID == -1)
        {
            Debug.LogFormat("[Gourmet Hamburger #{0}] Oh là là, Monsieur Chalmers, he... He seems rather impressed! Congratulations, you have once again served an incredible luncheon. (Solved.)", _moduleID);
            if (Sound != null)
                Sound.StopSound();
            Sound = Audio.HandlePlaySoundAtTransformWithRef("solve", transform, false);
            Module.HandlePass();
            CannotPress = false;
            Solved = true;
        }
        else
        {
            if (Sound != null)
                Sound.StopSound();
            Sound = Audio.HandlePlaySoundAtTransformWithRef("strike chalmers", transform, false);
            Module.HandleStrike();
            timer = 0;
            while (timer < 2f)
            {
                yield return null;
                timer += Time.deltaTime;
            }
            TimeWarp();
            CannotPress = false;
        }
    }

    private IEnumerator ButtonAnim(int pos, float duration = 0.075f)
    {
        float start = new[] { 0.0167f, 0.0167f, 0.0201f, 0.0201f }[pos];
        float depression = new[] { 0.0025f, 0.0025f, 0.005f, 0.005f }[pos];
        float timer = 0;
        while (timer < duration)
        {
            yield return null;
            timer += Time.deltaTime;
            Buttons[pos].transform.localPosition = new Vector3(Buttons[pos].transform.localPosition.x, Mathf.Lerp(start, start - depression, timer / duration), Buttons[pos].transform.localPosition.z);
        }
        Buttons[pos].transform.localPosition = new Vector3(Buttons[pos].transform.localPosition.x, start - depression, Buttons[pos].transform.localPosition.z);
        timer = 0;
        while (timer < duration)
        {
            yield return null;
            timer += Time.deltaTime;
            Buttons[pos].transform.localPosition = new Vector3(Buttons[pos].transform.localPosition.x, Mathf.Lerp(start - depression, start, timer / duration), Buttons[pos].transform.localPosition.z);
        }
        Buttons[pos].transform.localPosition = new Vector3(Buttons[pos].transform.localPosition.x, start, Buttons[pos].transform.localPosition.z);
    }

#pragma warning disable 414
    private string TwitchHelpMessage = "Use '!{0} cycle' to cycle the ingredients. Use '!{0} add onions' to add Onions to the burger and use '!{0} serve' to serve the burger.";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant();
        var commandArray = command.Split(' ');
        if (command == "cycle")
        {
            yield return null;
            Buttons[1].OnInteract();
            for (int i = 0; i < 2; i++)
            {
                float timer = 0;
                while (timer < 1.5f)
                {
                    yield return null;
                    timer += Time.deltaTime;
                }
                Buttons[1].OnInteract();
            }
        }
        else if (command == "serve")
        {
            yield return null;
            Buttons[3].OnInteract();
        }
        else if (commandArray.First() == "add")
        {
            var addedBit = command.Substring(4);
            if (CurrentIngredients.All(x => x.Info.Name.ToLowerInvariant() != addedBit))
            {
                yield return "sendtochaterror That isn't an option for this layer! Options are " + CurrentIngredients.Select(x => x.Info.Name).Join(", ") + ".";
                yield break;
            }
            yield return null;
            while (CurrentIngredients[IngredientIx].Info.Name.ToLowerInvariant() != addedBit)
            {
                Buttons[1].OnInteract();
                yield return new WaitForSeconds(0.2f);
            }
            Buttons[2].OnInteract();
        }
        else
        {
            yield return "sendtochaterror Invalid command.";
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        while (!Solved)
        {
            if (CannotPress)
                yield return true;
            else
            {
                if (RequiredIngredient.ID == -1)
                    Buttons[3].OnInteract();
                else
                {
                    while (CurrentIngredients[IngredientIx].ID != RequiredIngredient.ID)
                    {
                        Buttons[1].OnInteract();
                        yield return new WaitForSeconds(0.1f);
                    }
                    Buttons[2].OnInteract();
                }
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}
