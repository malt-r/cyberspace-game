using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WireTask : MonoBehaviour {
   public List<Color> _wireColors = new List<Color>();
   public List<Wire> _leftWires = new List<Wire>(); 
   public List<Wire> _rightWires = new List<Wire>();
   public List<string> _letters = new List<string>();
   
   public Wire CurrentDraggedWire;
   public Wire CurrentHoveredWire;
 
   public bool IsTaskCompleted = false;
   
   private List<Color> _availableColors;
   private List<int> _availableLeftWireIndex;
   private List<int> _availableRightWireIndex;
   private List<string> _availableLetters;

   private Coroutine coroutine;

   public void StartMinigame()
   {
      _availableColors = new List<Color>(_wireColors);
      _availableLetters = new List<string>(_letters);
      _availableLeftWireIndex = new List<int>();
      _availableRightWireIndex = new List<int>();
     
      for (int i = 0; i < _leftWires.Count; i++)
      {
         _availableLeftWireIndex.Add(i); 
      }
  
      for (int i = 0; i < _rightWires.Count; i++) 
      {
         _availableRightWireIndex.Add(i);
      }
 
      while (_availableColors.Count > 0 && 
             _availableLeftWireIndex.Count > 0 && 
             _availableRightWireIndex.Count > 0)
      {
         var tmp = Random.Range(0, _availableColors.Count);
         Color pickedColor = _availableColors[tmp];
         string pickedLetter = _availableLetters[tmp];
  
         int pickedLeftWireIndex = Random.Range(0,
            _availableLeftWireIndex.Count);
         int pickedRightWireIndex = Random.Range(0,
            _availableRightWireIndex.Count);
         _leftWires[_availableLeftWireIndex[pickedLeftWireIndex]]
            .SetColor(pickedColor, pickedLetter);
         _rightWires[_availableRightWireIndex[pickedRightWireIndex]]
            .SetColor(pickedColor, pickedLetter);
       
         _availableColors.Remove(pickedColor);
         _availableLetters.Remove(pickedLetter);
         _availableLeftWireIndex.RemoveAt(pickedLeftWireIndex);
         _availableRightWireIndex.RemoveAt(pickedRightWireIndex);
      }
   
      coroutine = StartCoroutine(CheckTaskCompletion());
   }

   public void ResetMinigame()
   {
      IsTaskCompleted = false;
      for (int i = 0; i < _leftWires.Count; i++)
      {
         _leftWires[i].ResetWire();
      }
  
      for (int i = 0; i < _rightWires.Count; i++) 
      {
         _rightWires[i].ResetWire();
      }
      
      if (coroutine != null)
      {
         StopCoroutine(coroutine);
      }
   }

   private void Start() {
      
   }
 
   private IEnumerator CheckTaskCompletion() {
      while (!IsTaskCompleted) {
         int successfulWires = 0;
         
         for (int i = 0; i < _rightWires.Count; i++) {
            if (_rightWires[i].IsSuccess) { successfulWires++; }
         }
         if (successfulWires >= _rightWires.Count) {
            IsTaskCompleted = true;
         }
       
         yield return new WaitForSeconds(0.5f);
     }
   }
}
