using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class Telephone : MonoBehaviour {

   public KMBombInfo Bomb;
   public KMAudio Audio;
   public KMSelectable TheMod;
   public TextMesh ShownPhrase;

   public Material[] Colors;
   public GameObject[] LEDs;
   string[] ColorNames = { "red", "orange", "yellow", "green", "blue", "indigo", "violet" };

   enum Phase {
      Dormant,
      Active,
      ReadyToStrike,
      ReadyToSolve
   }

   Phase State = Phase.Dormant;
   string[] Phrases = PhraseList.Phrases;

   string DisplayedPhrase = "";
   string Submission = "";
   string LastSubmission = ""; //This is just so I don't run something unnecessary every frame

   static string CurrentMessage = "";

   string NormalAnswer = "";              //If you use only the screen
   string[] AnyAnswer = { "", "", "" };    //If you use any LED

   static bool CanCall = false;
   bool Vowel = false;
   bool Focused = false;

   static int ActiveModID = -1;

   static int ReadyToSolve = 0;

   int[] LEDColors = { 0, 0, 0 };

   float RingTime = 0f;
   float BombStartTime = 0f;

   private KeyCode[] TheKeys = {
        //KeyCode.Backspace, KeyCode.Return,
        KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T, KeyCode.Y, KeyCode.U, KeyCode.I, KeyCode.O, KeyCode.P,
        KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.J, KeyCode.K, KeyCode.L,
        KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V, KeyCode.B, KeyCode.N, KeyCode.M//, KeyCode.Return
        /*KeyCode.Space, KeyCode.Alpha1, KeyCode.Keypad1, KeyCode.Alpha2, KeyCode.Keypad2, KeyCode.Alpha3, KeyCode.Keypad3,
        KeyCode.Alpha4, KeyCode.Keypad4, KeyCode.Alpha5, KeyCode.Keypad5, KeyCode.Alpha6, KeyCode.Keypad6, KeyCode.Alpha7, KeyCode.Keypad7,
        KeyCode.Alpha8, KeyCode.Keypad8, KeyCode.Alpha9, KeyCode.Keypad9, KeyCode.Alpha0, KeyCode.Keypad0,
        KeyCode.LeftArrow, KeyCode.UpArrow, KeyCode.RightArrow, KeyCode.DownArrow*/
  };
   string TheLetters = "QWERTYUIOPASDFGHJKLZXCVBNM";

   //bool DuplicateTelephones = false;

   static int ModuleIdCounter = 1;
   int ModuleId;
   private bool ModuleSolved;
   static int AmountOfSolvesNeeded; //Sets up the 
   static int CurrentSubmissions;

   void Awake () {
      ModuleId = ModuleIdCounter++;
      /*
      foreach (KMSelectable object in keypad) {
          object.OnInteract += delegate () { keypadPress(object); return false; };
      }
      */

      //button.OnInteract += delegate () { buttonPress(); return false; };
      GetComponent<KMBombModule>().OnActivate += Activate;
      //GetComponent<KMBombInfo>().OnBombExploded += AntiSoftlock;
      

      TheMod.OnFocus += delegate { Highlight(); };
      if (Application.isEditor) {
         Focused = true;
         CanCall = true;
         Highlight();
      }
   }

   void Highlight () {
      Focused = true;
      if (!(CanCall && ActiveModID == -1)) {
         return;
      }
      ActiveModID = ModuleId;
      LEDColors = Enumerable.Range(0, 7).ToArray().Shuffle().Take(3).ToArray();

      DisplayedPhrase = PhraseList.Phrases.PickRandom();
      ShownPhrase.text = DisplayedPhrase;
      DisplayedPhrase = DisplayedPhrase.Replace(" ", "");

      int LowestRule = Math.Max(Math.Max(LEDColors[0], LEDColors[1]), LEDColors[2]);
      int HighestRule = Math.Min(Math.Min(LEDColors[0], LEDColors[1]), LEDColors[2]);
      int MiddleRule = LEDColors.Sum() - HighestRule - LowestRule;

      for (int i = 0; i < 3; i++) {
         LEDs[i].GetComponent<MeshRenderer>().material = Colors[LEDColors[i]];
      }

      if (CurrentMessage == "") {
         if (Vowel) {
            NormalAnswer = ModificationsScript.ModifyString(MiddleRule, LowestRule, DisplayedPhrase);
            Debug.LogFormat("[Telephone #{0}] Applying rules {1} and {2} gives \"{3}\"", ModuleId, ColorNames[MiddleRule], ColorNames[LowestRule], NormalAnswer);
         }
         else {
            NormalAnswer = ModificationsScript.ModifyString(HighestRule, MiddleRule, DisplayedPhrase);
            Debug.LogFormat("[Telephone #{0}] Applying rules {1} and {2} gives \"{3}\"", ModuleId, ColorNames[HighestRule], ColorNames[MiddleRule], NormalAnswer);
         }
         return;
      }
      else {
         NormalAnswer = ModificationsScript.ModifyString(HighestRule, MiddleRule, LowestRule, DisplayedPhrase);
         Debug.LogFormat("[Telephone #{0}] Applying rules {1}, {2}, and {3} gives \"{4}\"", ModuleId, ColorNames[HighestRule], ColorNames[MiddleRule], ColorNames[LowestRule], NormalAnswer);
         for (int i = 0; i < 3; i++) {
            AnyAnswer[i] = ModificationsScript.ModifyString(LEDColors[i], CurrentMessage);
         }
         Debug.LogFormat("[Telephone #{0}] Alternate Answer by applying rule {1}: \"{2}\"", ModuleId, ColorNames[LEDColors[0]], AnyAnswer[0]);
         Debug.LogFormat("[Telephone #{0}] Alternate Answer by applying rule {1}: \"{2}\"", ModuleId, ColorNames[LEDColors[1]], AnyAnswer[1]);
         Debug.LogFormat("[Telephone #{0}] Alternate Answer by applying rule {1}: \"{2}\"", ModuleId, ColorNames[LEDColors[2]], AnyAnswer[2]);
      }
   }

   void OnDestroy () {
      ActiveModID = -1;
      CanCall = false;
      ReadyToSolve = 0;
      AmountOfSolvesNeeded = 0;
      CurrentSubmissions = 0;
   }

   void Activate () {
      //Debug.Log(ModificationsScript.Violet("ALL WORK AND NO PLAY"));
      BombStartTime = Bomb.GetTime();
      //RingTime = Math.Min(BombStartTime * Rnd.Range(.1f, .3f), Rnd.Range(540f, 660f)); //Picks a time between 9-11 minutes or 20% of bomb time
      RingTime = 1f;
      StartCoroutine(WaitTime());
      AmountOfSolvesNeeded++;
   }

   IEnumerator WaitTime () {
      yield return new WaitForSecondsRealtime(RingTime);
      Audio.PlaySoundAtTransform("Phone Ring", transform);
      yield return new WaitForSecondsRealtime(5f);
      Audio.PlaySoundAtTransform("Phone Ring", transform);
      yield return new WaitForSecondsRealtime(5f);
      Audio.PlaySoundAtTransform("Phone Ring", transform);
      yield return new WaitForSecondsRealtime(5f);
      CanCall = true;
   }

   void Start () {
      string[] Vowels = { "A", "E", "I", "O", "U" };
      /*if (Bomb.GetModuleNames().Count(x => x == "Telephone") > 1) {
         DuplicateTelephones = true;
      }*/
      for (int i = 0; i < 6; i++) {
         if (Vowels.Contains(Bomb.GetSerialNumber()[i].ToString())) {
            Vowel = true;
         }
      }
   }

   void Update () {
      if (ModuleSolved || !Focused) {
         return;
      }
      if (Input.GetKeyDown(KeyCode.Backspace) && Submission != "") {
         Submission = Submission.Substring(0, Submission.Length - 1);
      }
      if (LastSubmission != Submission) {
         ShownPhrase.text = Submission;
         LastSubmission = Submission;
      }
      if (Focused) {
         for (int i = 0; i < TheKeys.Count(); i++) {
            if (Input.GetKeyDown(TheKeys[i])) {
               Submission += TheLetters[i].ToString();
            }
         }
      }
      if (Input.GetKeyDown(KeyCode.Return)) {
         CheckSubmission();
      }
   }

   void CheckSubmission () {
      ModuleSolved = true; //Put it here so that 
      CurrentSubmissions++;
      if (AnyAnswer.Any(x => x == Submission) || Submission == NormalAnswer) {
         CurrentMessage = Submission;
         if (AmountOfSolvesNeeded == 1) {
            Solve();
         }
         else {
            FakeSolve();
         }
      }
      else {
         if (AmountOfSolvesNeeded == 1) {
            Strike();
         }
         else {
            CurrentMessage = Submission;
            FakeStrike();
         }
      }

      if (CurrentSubmissions == AmountOfSolvesNeeded && CurrentSubmissions != 0) {
         if (State == Phase.ReadyToStrike) {
            Strike();
         }
         else {
            Solve();
         }
      }
   }

   void FakeSolve () {
      //CurrentMessage = Submission;
      CurrentSubmissions++;
      State = Phase.ReadyToSolve;
      ActiveModID = -1;
   }

   void FakeStrike () {
      //CurrentMessage = Submission;
      CurrentSubmissions++;
      State = Phase.ReadyToStrike;
      ActiveModID = -1;
   }

   void Solve () {
      GetComponent<KMBombModule>().HandlePass();
      CurrentMessage = Submission;
      CurrentSubmissions = 0;
      AmountOfSolvesNeeded--;
      State = Phase.Dormant;
   }

   void Strike () {
      ModuleSolved = false;
      CurrentSubmissions = 0;
      State = Phase.Dormant;
   }

#pragma warning disable 414
   private readonly string TwitchHelpMessage = @"Use !{0} to do something.";
#pragma warning restore 414

   IEnumerator ProcessTwitchCommand (string Command) {
      yield return null;
   }

   IEnumerator TwitchHandleForcedSolve () {
      yield return null;
   }
}
