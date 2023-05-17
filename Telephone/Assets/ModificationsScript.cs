using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class ModificationsScript : MonoBehaviour {

   public static string ModifyString (int Rule, string Message) {
      string StrBuilder = "";
      switch (Rule) {
         case 0:
            StrBuilder = Red(Message);
            break;
         case 1:
            StrBuilder = Orange(Message);
            break;
         case 2:
            StrBuilder = Yellow(Message);
            break;
         case 3:
            StrBuilder = Green(Message);
            break;
         case 4:
            StrBuilder = Blue(Message);
            break;
         case 5:
            StrBuilder = Indigo(Message);
            break;
         case 6:
            StrBuilder = Violet(Message);
            break;
      }
      return StrBuilder;
   }

   public static string ModifyString (int Rule, int Rule2, string Message) {
      string StrBuilder = "";
      StrBuilder = ModifyString(Rule, Message);
      StrBuilder = ModifyString(Rule2, StrBuilder);
      return StrBuilder;
   }

   public static string ModifyString (int Rule, int Rule2, int Rule3, string Message) {
      string StrBuilder = "";
      StrBuilder = ModifyString(Rule, Message);
      StrBuilder = ModifyString(Rule2, StrBuilder);
      StrBuilder = ModifyString(Rule3, StrBuilder);
      return StrBuilder;
   }

   public static string Red (string Message) {
      string StrBuilder = "";
      
      for (int i = 0; i < Message.Length / 2; i++) {
         StrBuilder += Message[i * 2 + 1].ToString() + Message[i * 2].ToString();
      }
      return StrBuilder;
   }

   public static string Orange (string Message) {
      char[] charArray = Message.ToCharArray();
      Array.Reverse(charArray);
      return new string(charArray);
   }

   public static string Yellow (string Message) {
      string[] Vowels = { "A", "E", "I", "O", "U"};
      string[] Shift = { "B", "F", "J", "P", "V" };
      string StrBuilder = "";
      for (int i = 0; i < Message.Length; i++) {
         StrBuilder += Vowels.Contains(Message[i].ToString()) ? Shift[Array.IndexOf(Vowels, Message[i].ToString())] : Message[i].ToString();
      }

      return StrBuilder;
   }

   public static string Green (string Message) {
      string StrBuilder = "";
      for (int i = 0; i < Message.Length; i += 2) {
         StrBuilder += Message[i];
      }
      for (int i = 1; i < Message.Length; i += 2) {
         StrBuilder += Message[i];
      }
      return StrBuilder;
   }

   public static string Blue (string Message) {
      string StrBuilder = "";
      for (int i = 0; i < Message.Length; i++) {
         if (ExMath.IsPrime(i + 1)) {
            StrBuilder += Message[i];
         }
      }
      for (int i = 0; i < Message.Length; i++) {
         if (!ExMath.IsPrime(i + 1)) {
            StrBuilder += Message[i];
         }
      }
      return StrBuilder;
   }

   public static string Indigo (string Message) {
      return Message.Last() + Message.Substring(0, Message.Length - 1);
   }

   public static string Violet (string Message) {
      return Message.Substring(1) + Message[0];
   }
}
